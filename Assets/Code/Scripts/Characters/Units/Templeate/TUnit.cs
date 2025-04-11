using System;
using UnityEngine;

namespace AF.TS.Characters
{
    [Serializable]
    public abstract class TUnit
    {
        [field: NonSerialized] public Character Character { get; set; }

        public Transform Transform => this.Character != null ? this.Character.transform : null;
    }

}
