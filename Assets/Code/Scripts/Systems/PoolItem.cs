using UnityEngine;

namespace AF.TS.Weapons
{
    [System.Serializable]
    public class PoolItem
    {
        public GameObject prefab;
        [Min(1)] public int amount;
    }
}
