using System;
using UnityEngine;

namespace PrinceTitan
{
    public sealed class WorldSimulation
    {
        private readonly WorldState state;
        private float eventClock;
        private int eventIndex;

        private readonly WorldEvent[] events =
        {
            new WorldEvent("Voo observado", "Uma aeronave de reconhecimento cruzou Meio-Dia Branco sem pousar.", "assembly"),
            new WorldEvent("Mercado aberto", "Os toldos dos Degraus de Açafrão subiram; remédios estão mais baratos.", "emberline"),
            new WorldEvent("Transferência Titan", "Aurelia registrou uma estrutura pesada seguindo para o Pátio Ferrous.", "aurelia"),
            new WorldEvent("Luzes na propriedade", "Todas as janelas do sul da Casa Mirador acenderam ao mesmo tempo.", "vesper"),
            new WorldEvent("Contrato silencioso", "Uma companhia de navegação mudou de mãos na Bolsa de Contratos.", "aurelia"),
            new WorldEvent("Chegada familiar", "Dois mensageiros Ember-Sol alcançaram as Casas do Vale antes do bonde.", "emberline"),
            new WorldEvent("Comunicado oficial", "O Porto de Vidro reabriu a rota de balsas do leste.", "assembly"),
            new WorldEvent("Sino da fundição", "O Pátio Ferrous terminou o ombro de um novo Titan.", "vesper")
        };

        public event Action<WorldEvent> EventRaised;

        public WorldSimulation(WorldState state)
        {
            this.state = state;
        }

        public void Tick(float deltaTime)
        {
            if (state == null || state.paused) return;
            var speed = Mathf.Clamp(state.timeScale, .25f, 4f);
            state.minuteOfDay += deltaTime * 2.2f * speed;
            while (state.minuteOfDay >= 1440f)
            {
                state.minuteOfDay -= 1440f;
                state.day++;
            }

            if (state.movers != null)
            {
                foreach (var mover in state.movers)
                {
                    var direction = mover.forward ? 1f : -1f;
                    mover.progress += mover.speed * deltaTime * speed * direction;
                    if (mover.progress >= 1f) { mover.progress = 1f; mover.forward = false; }
                    if (mover.progress <= 0f) { mover.progress = 0f; mover.forward = true; }
                }
            }

            if (state.markets != null)
            {
                foreach (var market in state.markets)
                {
                    var wave = Mathf.Sin(Time.unscaledTime * .22f + market.phase) * .035f;
                    market.activity = Mathf.Clamp(market.activity + wave * deltaTime * speed, 22f, 96f);
                }
            }

            if (state.factions != null)
            {
                for (var i = 0; i < state.factions.Count; i++)
                {
                    var drift = Mathf.Sin(Time.unscaledTime * .06f + i * 1.9f) * .008f;
                    state.factions[i].influence = Mathf.Clamp(state.factions[i].influence + drift * deltaTime * speed, 18f, 92f);
                }
            }

            eventClock += deltaTime * speed;
            if (eventClock >= 8.5f)
            {
                eventClock = 0f;
                var item = events[eventIndex % events.Length];
                eventIndex++;
                if (EventRaised != null) EventRaised(item);
            }
        }

        public string ClockText()
        {
            var total = Mathf.FloorToInt(state.minuteOfDay);
            return "DIA " + state.day.ToString("000") + "  ·  " + (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
        }
    }
}
