using UnityEngine;
using Unity.Cinemachine;
using static UnityEngine.ParticleSystem;
using Sirenix.OdinInspector;
using AF.TS.Characters;


#if UNITY_EDITOR
using System;
#endif

namespace AF.TS.Weapons
{
    [HideMonoScript]
    [CreateAssetMenu(fileName = "BulletData", menuName = "AF/BulletData")]
    public class BulletData : ScriptableObject
    {
        #region Fields ------------------------------------------------------------------

        #region General
        [InfoBox("The <color=red>Red</color> fields are to be implemented", InfoMessageType.Warning)] // TODO: removed when implemented all fields
        [FoldoutGroup("General Settings")]
        [Tooltip("Defines the physics of the bullet, simulated or physical" +
            "\n<color=yellow>Simulated</color>: Bullet moves based on the simulation of the physics engine." +
            "\n<color=red>Physics</color>: Bullet moves based on the physics engine. (More Expensive)"
            )]
        [SerializeField]
        private PhysicsMode m_physicType = PhysicsMode.Simulated;

        [FoldoutGroup("General Settings")]
        [Tooltip("Speed at which the bullet travels."), Unit(Units.MetersPerSecond)]
        [SerializeField, MinValue(0f)]
        private float m_speed = 10f;

        [HorizontalGroup("General Settings/range", width: 16f)]
        [Tooltip("Enables maximum range limit.")]
        [SerializeField, HideLabel]
        private bool m_hasRange = true;

        [HorizontalGroup("General Settings/range")]
        [Tooltip("Max distance the bullet travels before despawning.")]
        [SerializeField, EnableIf("m_hasRange"), MultiType(typeof(float), typeof(Vector2), typeof(AnimationCurve), typeof(MinMaxCurve))]
        private MultiTypeValue m_range;

        [HorizontalGroup("General Settings/lifetime", width: 16f)]
        [Tooltip("Enables lifetime limit.")]
        [SerializeField, HideLabel]
        private bool m_hasLifetime = false;

        [HorizontalGroup("General Settings/lifetime")]
        [Tooltip("Max lifetime regardless of distance.")]
        [SerializeField, EnableIf("m_hasLifetime"), MultiType(typeof(float), typeof(Vector2), typeof(AnimationCurve), typeof(MinMaxCurve))]
        private MultiTypeValue m_lifetime;

        [GUIColor("red")] //TODO: Need to be implmented
        [FoldoutGroup("General Settings")]
        [Tooltip("Bullet accuracy or spread factor.")]
        [SerializeField, MultiType(typeof(float), typeof(Vector2), typeof(AnimationCurve), typeof(MinMaxCurve))]
        private MultiTypeValue m_spread;

        [FoldoutGroup("General Settings/Damage")]
        [Tooltip("Bullet damage amount.")]
        [SerializeField, MinValue(0f)]
        private float m_damage = 1f;

        [FoldoutGroup("General Settings/Damage")]
        [Tooltip("Bullet damage type.")]
        [SerializeField]
        private DamageType m_damageType = DamageType.Physical;

        [FoldoutGroup("General Settings/Damage")]
        [Tooltip("Bullet damage status effect.")]
        [SerializeField]
        private StatusEffectType m_statusEffect = StatusEffectType.None;

        [GUIColor("red")] //TODO: Need to be implmented
        [FoldoutGroup("General Settings")]
        [Tooltip("How many surfaces the bullet can pierce through.")]
        [SerializeField, MinValue(0)]
        private int m_penetration = 0;

        [GUIColor("red")] //TODO: Need to be implmented
        [FoldoutGroup("General Settings")]
        [Tooltip("Recoil behavior profile triggered on fire.")]
        [SerializeField]
        private CinemachineBasicMultiChannelPerlin m_recoilProfile;

        #endregion

        #region Physics Simulated

        [GUIColor("red")] //TODO: Need to be implmented
        [FoldoutGroup("Physics Settings")]
        [Tooltip("Bullet physical mass."), Unit(Units.Kilogram)]
        [SerializeField, ShowIf("m_physicType", PhysicsMode.Simulated), MinValue(0f)]
        private float m_mass = 1f;

        [GUIColor("red")] //TODO: Need to be implmented
        [FoldoutGroup("Physics Settings")]
        [Tooltip("Bullet physical drag."), Unit(Units.KilogramForce)]
        [SerializeField, ShowIf("m_physicType", PhysicsMode.Simulated), MinValue(0f)]
        private float m_drag = 1f;

        [GUIColor("red")] //TODO: Need to be implmented
        [FoldoutGroup("Physics Settings")]
        [Tooltip("Custom multiplier for gravity effect."), Unit(Units.KilogramForce)]
        [SerializeField, ShowIf("m_physicType", PhysicsMode.Simulated), MinValue(0f)]
        private float m_gravityMultiplier = 1f;

        [FoldoutGroup("Physics Settings")]
        [Tooltip("Layers this bullet can collide with.")]
        [SerializeField, ShowIf("m_physicType", PhysicsMode.Simulated)]
        private LayerMask m_impactLayers = UnityEngine.Physics.DefaultRaycastLayers;

        [FoldoutGroup("Physics Settings")]
        [Tooltip("Bullet physical radius."), Unit(Units.Centimeter)]
        [SerializeField, ShowIf("m_physicType", PhysicsMode.Simulated), MinValue(0f)]
        private float m_radius = 0.1f;

        #endregion

        #region Physics

        [ToggleGroup("Physics Settings/m_useAdvancedBallistics", "Advanced Ballistics")]
        [Tooltip("Enables ballistic simulation (drop and falloff).")]
        [SerializeField]
        private bool m_useAdvancedBallistics = false;

        [GUIColor("red")] //TODO: Need to be implmented
        [ToggleGroup("Physics Settings/m_useAdvancedBallistics")]
        [Tooltip("Bullet drop over travel time.")]
        [SerializeField, MultiType(typeof(float), typeof(AnimationCurve))]
        private MultiTypeValue m_dropOverDistance;

        [GUIColor("red")] //TODO: Need to be implmented
        [ToggleGroup("Physics Settings/m_useAdvancedBallistics")]
        [Tooltip("How accuracy decreases over time or distance.")]
        [SerializeField, MultiType(typeof(float), typeof(AnimationCurve))]
        private MultiTypeValue m_accuracyFalloff;

        [GUIColor("red")] //TODO: Need to be implmented
        [ToggleGroup("Physics Settings/m_useAdvancedBallistics")]
        [Tooltip("Wind direction and strength affecting trajectory.")]
        [SerializeField]
        private Vector2 m_windInfluence;

        [GUIColor("red")] //TODO: Need to be implmented
        [ToggleGroup("Physics Settings/m_useAdvancedBallistics")]
        [Tooltip("Air resistance affecting trajectory. More resistant means costant slower speed.")]
        [SerializeField, MultiType(typeof(float), typeof(AnimationCurve))]
        private MultiTypeValue m_airResistance;

        [HorizontalGroup("Physics Settings/ricochets", width: 16f)]
        [Tooltip("Enables bullet ricochet.")]
        [SerializeField, HideLabel]
        private bool m_ricochetEnabled = false;

        [GUIColor("red")] //TODO: Need to be implmented
        [HorizontalGroup("Physics Settings/ricochets")]
        [Tooltip("Max number of ricochets allowed.")]
        [SerializeField, EnableIf("m_ricochetEnabled"), MinValue(0)]
        private int m_ricochetCount = 1;

        #endregion

        #region Effects

        [FoldoutGroup("Effects Settings")]
        [Tooltip("Sound played on impact. Optional, can be null.")]
        [SerializeField]
        private AudioClip m_impactSound;

        [FoldoutGroup("Effects Settings")]
        [Tooltip("Visual effect played on impact. Optional, can be null.")]
        [SerializeField, AssetsOnly]
        private GameObject m_impactEffect;

        #endregion

        #region Debug

        [FoldoutGroup("Debug")]
        [ToggleGroup("Debug/m_drawGizmos", "Draw Gizmos")]
        [Tooltip("If true, gizmos will be drawn for this bullet.")]
        [SerializeField]
        private bool m_drawGizmos = false;

        [GUIColor("red")] //TODO: Need to be implmented
        [ToggleGroup("Debug/m_drawGizmos", "Draw Gizmos")]
        [Tooltip("If true, gizmos will be drawn for this bullet.")]
        [SerializeField]
        private bool m_drawPath = false;

        [ToggleGroup("Debug/m_drawGizmos", "Draw Gizmos")]
        [Tooltip("If true, gizmos will be drawn for this bullet.")]
        [SerializeField, ShowIf("m_physicType", PhysicsMode.Simulated)]
        private bool m_drawBody = false;

        #endregion

        #endregion

        #region Validation --------------------------------------------------------------

#if UNITY_EDITOR
        public event Action OnEditorValidate;

        private void OnValidate()
        {
            OnEditorValidate?.Invoke();
        }
#endif

        #endregion

        #region Getters -----------------------------------------------------------------

        #region General

        public PhysicsMode PhysicType => m_physicType;

        public float Speed => m_speed;

        public bool HasRange => m_hasRange;

        public float Range => m_range.Evaluate();

        public bool HasLifeTime => m_hasLifetime;

        public float LifeTime => m_lifetime.Evaluate();

        public float Spread => m_spread.Evaluate();

        public float Damage => m_damage;

        public DamageType DamageType => m_damageType;

        public StatusEffectType StatusEffect => m_statusEffect;

        public int Penetration => m_penetration;

        public CinemachineBasicMultiChannelPerlin RecoilProfile => m_recoilProfile;

        #endregion

        #region Physics Simulated

        public float Mass => m_mass;

        public float Drag => m_drag;

        public float GravityMultiplier => m_gravityMultiplier;

        public LayerMask ImpactLayers => m_impactLayers;

        public float Radius => m_radius;

        #endregion

        #region Physics

        public bool UseAdvancedBallistics => m_useAdvancedBallistics;

        public float DropOverDistance => m_dropOverDistance.Evaluate();

        public float AccuracyFalloff => m_accuracyFalloff.Evaluate();

        public Vector2 WindInfluence => m_windInfluence;

        public float AirResistance => m_airResistance.Evaluate();

        public bool RicochetEnabled => m_ricochetEnabled;

        public int RicochetCount => m_ricochetCount;

        #endregion

        #region Effects

        public AudioClip ImpactSound => m_impactSound;

        public GameObject ImpactEffect => m_impactEffect;

        #endregion

        #region Debug

        public bool DrawGizmos => m_drawGizmos;

        public bool DrawPath => m_drawGizmos && m_drawPath;

        public bool DrawBody => m_drawGizmos && m_drawBody;

        #endregion

        #endregion

    }

}
