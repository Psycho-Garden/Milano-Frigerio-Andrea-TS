using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace AF.TS.Characters
{
    [System.Serializable]
    public class UnitStats : TUnitStats
    {
        [ShowInInspector]
        private Dictionary<string, float> stats = new()
        {
            { "Health", 100 },
            { "Stamina", 50 },
            { "Strength", 20 }
        };

        public override float GetStat(string key) =>
            stats.TryGetValue(key, out var value) ? value : 0;

        public override void SetStat(string key, float value)
        {
            if (stats.ContainsKey(key)) stats[key] = value;
        }
    }
}
