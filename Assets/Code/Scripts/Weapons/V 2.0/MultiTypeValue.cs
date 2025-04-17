using UnityEngine;
using System;
using static UnityEngine.ParticleSystem;

namespace AF.TS.Weapons
{
    [Serializable]
    public class MultiTypeValue
    {
        [HideInInspector] public int selectedIndex;

        [SerializeField] private float m_float;
        [SerializeField] private Vector2 m_vector2;
        [SerializeField] private AnimationCurve m_curve = new AnimationCurve();
        [SerializeField] private MinMaxCurve m_minMaxCurve = new MinMaxCurve(1f);

        public float Evaluate(float t = 0f) => Evaluate(t, null);

        public float Evaluate(float t, System.Random customRandom)
        {
            return selectedIndex switch
            {
                0 => m_float,
                1 => customRandom != null
                    ? Mathf.Lerp(m_vector2.x, m_vector2.y, (float)customRandom.NextDouble())
                    : UnityEngine.Random.Range(m_vector2.x, m_vector2.y),
                2 => m_curve.Evaluate(t),
                3 => m_minMaxCurve.Evaluate(t),
                _ => 0f
            };
        }

        public float EvaluateLerp(float t, float lerp)
        {
            lerp = Mathf.Clamp01(lerp);

            return selectedIndex switch
            {
                0 => m_float,
                1 => Mathf.Lerp(m_vector2.x, m_vector2.y, lerp),
                2 => m_curve.Evaluate(t),
                3 => m_minMaxCurve.mode switch
                {
                    ParticleSystemCurveMode.Constant => m_minMaxCurve.constant,
                    ParticleSystemCurveMode.TwoConstants => Mathf.Lerp(m_minMaxCurve.constantMin, m_minMaxCurve.constantMax, lerp),
                    ParticleSystemCurveMode.Curve => m_minMaxCurve.curve.Evaluate(t),
                    ParticleSystemCurveMode.TwoCurves => Mathf.Lerp(m_minMaxCurve.curveMin.Evaluate(t), m_minMaxCurve.curveMax.Evaluate(t), lerp),
                    _ => 0f
                },
                _ => 0f
            };
        }

        // Accessors for the drawer
        public float FloatValue { get => m_float; set => m_float = value; }
        public Vector2 Vector2Value { get => m_vector2; set => m_vector2 = value; }
        public AnimationCurve CurveValue { get => m_curve; set => m_curve = value; }
        public MinMaxCurve MinMaxCurveValue { get => m_minMaxCurve; set => m_minMaxCurve = value; }
    }
}
