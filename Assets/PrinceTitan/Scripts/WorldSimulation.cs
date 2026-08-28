using System;
using UnityEngine;

namespace PrinceTitan
{
    public sealed class WorldSimulation
    {
        private const float NarrativeMinutesPerSecond = 4f;
        private readonly WorldState state;

        public event Action<WorldEvent> EventRaised;

        public WorldSimulation(WorldState state)
        {
            this.state = state;
        }

        public void Tick(float deltaTime)
        {
            if (state == null || state.paused) return;
            var speed = Mathf.Clamp(state.timeScale, .25f, 8f);
            var narrativeDelta = deltaTime * NarrativeMinutesPerSecond * speed;
            state.minuteOfDay += narrativeDelta;
            while (state.minuteOfDay >= 1440f)
            {
                state.minuteOfDay -= 1440f;
                state.day++;
            }

            if (state.missions == null) return;
            foreach (var mission in state.missions)
            {
                if (mission == null || mission.status != MissionStatus.EnRoute) continue;
                mission.elapsedMinutes += narrativeDelta;
                if (mission.elapsedMinutes + .001f < mission.durationMinutes) continue;

                mission.elapsedMinutes = mission.durationMinutes;
                mission.status = MissionStatus.Arrived;
                var arrival = new WorldEvent(
                    "MISSÃO CHEGOU · " + mission.callsign,
                    mission.consequence,
                    mission.id,
                    mission.destinationSiteId,
                    mission.realm);
                Remember(arrival);
                var handler = EventRaised;
                if (handler != null) handler(arrival);
            }
        }

        public void TogglePause()
        {
            if (state != null) state.paused = !state.paused;
        }

        public void CycleSpeed()
        {
            if (state == null) return;
            state.timeScale = state.timeScale < 1.5f ? 2f : state.timeScale < 3f ? 4f : state.timeScale < 6f ? 8f : 1f;
        }

        public string ClockText()
        {
            if (state == null) return "DIA --- · --:--";
            var total = Mathf.FloorToInt(state.minuteOfDay);
            return "DIA " + state.day.ToString("000") + "  ·  " + (total / 60).ToString("00") + ":" + (total % 60).ToString("00");
        }

        public string EtaText(MissionData mission)
        {
            if (mission == null) return "ETA DESCONHECIDO";
            if (mission.status == MissionStatus.Arrived || mission.status == MissionStatus.Completed) return "CHEGOU";
            if (mission.status == MissionStatus.Interrupted) return "INTERROMPIDA";
            if (mission.status == MissionStatus.Missing) return "DESAPARECIDA";
            var remaining = Mathf.Max(0f, mission.durationMinutes - mission.elapsedMinutes);
            if (remaining >= 1440f)
            {
                var days = Mathf.FloorToInt(remaining / 1440f);
                var hours = Mathf.FloorToInt((remaining - days * 1440f) / 60f);
                return "ETA " + days + "D " + hours.ToString("00") + "H";
            }
            if (remaining >= 60f)
            {
                var hours = Mathf.FloorToInt(remaining / 60f);
                var minutes = Mathf.CeilToInt(remaining - hours * 60f);
                return "ETA " + hours + "H " + minutes.ToString("00") + "M";
            }
            return "ETA " + Mathf.CeilToInt(remaining) + " MIN";
        }

        public string StatusText(MissionData mission)
        {
            if (mission == null) return "SEM SINAL";
            switch (mission.status)
            {
                case MissionStatus.Planned: return "PLANEJADA";
                case MissionStatus.EnRoute: return state != null && state.paused ? "EM CURSO · PAUSADA" : "EM CURSO";
                case MissionStatus.Interrupted: return "INTERROMPIDA";
                case MissionStatus.Arrived: return "CHEGOU";
                case MissionStatus.Missing: return "DESAPARECIDA";
                case MissionStatus.Completed: return "CONCLUÍDA";
                default: return mission.status.ToString().ToUpperInvariant();
            }
        }

        private void Remember(WorldEvent item)
        {
            if (state == null) return;
            if (state.eventHistory == null) state.eventHistory = new System.Collections.Generic.List<IntelEventData>();
            state.eventHistory.Add(new IntelEventData
            {
                id = Guid.NewGuid().ToString("N"), title = item.title, detail = item.detail,
                missionId = item.missionId, siteId = item.siteId, realm = item.realm,
                day = state.day, minuteOfDay = state.minuteOfDay
            });
            while (state.eventHistory.Count > 80) state.eventHistory.RemoveAt(0);
        }
    }
}
