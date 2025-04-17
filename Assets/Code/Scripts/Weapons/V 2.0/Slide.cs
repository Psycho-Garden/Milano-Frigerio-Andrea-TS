using System;
using UnityEngine;
using Sirenix.OdinInspector;

namespace AF.TS.Weapons
{
    [Serializable]
    public class Slide
    {
        [Tooltip("")]
        [SerializeField] private Transform m_slideTransform;

        [Tooltip(""), Unit(Units.Meter, Units.Centimeter)]
        [SerializeField] private float m_slideEscursion = 0f;

        public Transform SlideTransform => m_slideTransform;
        public float SlideEscursion => m_slideEscursion;
    }
}
