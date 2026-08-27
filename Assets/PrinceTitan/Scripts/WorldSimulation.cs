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
            new WorldEvent("Flight observed", "A pale reconnaissance aircraft crossed White Noon without descending.", "assembly"),
            new WorldEvent("Market opens", "Saffron Steps raised its canvas awnings; medicine prices are easing.", "emberline"),
            new WorldEvent("Titan transfer", "Aurelia Works registered a heavy frame moving west toward Ferrous Yard.", "aurelia"),
            new WorldEvent("Estate lights", "Every southern window of Mirador House lit at the same hour.", "vesper"),
            new WorldEvent("Quiet contract", "A shipping company changed ownership at the Contract Exchange.", "aurelia"),
            new WorldEvent("Family arrival", "Two Ember-Sol couriers reached Vale Houses before the evening tram.", "emberline"),
            new WorldEvent("Government notice", "Glass Harbor reopened its eastern ferry lane.", "assembly"),
            new WorldEvent("Foundry bell", "Ferrous Yard completed another Titan shoulder assembly.", "vesper")
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
            return "DAY " + state.day.ToString("000") + "  •  " + (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
        }
    }
}
