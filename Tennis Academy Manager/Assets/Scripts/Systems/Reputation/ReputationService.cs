using System;
using System.Collections.Generic;
using TennisAcademyManager.Core;
using UnityEngine;

namespace TennisAcademyManager.Systems
{
    public class ReputationService : IGameService
    {
        private readonly Dictionary<ReputationComponent, int> components = new();

        public event Action<ReputationComponent, int, string> OnReputationChanged;

        public void Initialize()
        {
            // GDD: start with no reputation
            foreach (ReputationComponent c in Enum.GetValues(typeof(ReputationComponent)))
                components[c] = 0;

            Debug.Log("[ReputationService] Initialized");
        }

        public int Get(ReputationComponent component) => components[component];

        public int GlobalReputation
        {
            get
            {
                int sum = 0;
                int count = 0;
                foreach (var kv in components)
                {
                    sum += kv.Value;
                    count++;
                }
                return count == 0 ? 0 : Mathf.RoundToInt((float)sum / count);
            }
        }

        public void Add(ReputationComponent component, int delta, string reason)
        {
            int oldValue = components[component];
            int newValue = Mathf.Clamp(oldValue + delta, 0, 100);

            if (newValue == oldValue) return;

            components[component] = newValue;
            Debug.Log($"[Reputation] {component} {oldValue} -> {newValue} ({reason})");
            OnReputationChanged?.Invoke(component, newValue, reason);
        }
    }
}
