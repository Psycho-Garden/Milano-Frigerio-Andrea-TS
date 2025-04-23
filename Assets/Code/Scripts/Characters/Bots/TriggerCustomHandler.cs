using UnityEngine;
using Sirenix.OdinInspector;

namespace AF.TS.Utils
{
    [HideMonoScript]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class TriggerCustomHandler : ColliderVisualizer
    {
        #region Exposed Members ----------------------------------------------------------------------
        [FoldoutGroup("Settings")]
        [Tooltip("The LayerMask to filter which objects can trigger this event.")]
        [SerializeField]
        private LayerMask m_layer = Physics.DefaultRaycastLayers;

        [FoldoutGroup("Settings")]
        [Tooltip("Whether to trigger the event only once.")]
        [SerializeField]
        private bool m_triggerOnce = true;

        [HorizontalGroup("Settings/Enter", width: 16f)]
        [Tooltip("If true, the event will be triggered when objects enters the trigger zone.")]
        [SerializeField, HideLabel]
        private bool m_enter = true;

        [HorizontalGroup("Settings/Enter")]
        [Tooltip("Event triggered when the player enters the trigger zone.")]
        [SerializeField, EnableIf("m_enter")]
        private TriggerEvent m_onTriggerEnter = new();

        [HorizontalGroup("Settings/Stay", width: 16f)]
        [Tooltip("If true, the event will be triggered when objects stays in the trigger zone.")]
        [SerializeField, HideLabel]
        private bool m_stay = true;

        [HorizontalGroup("Settings/Stay")]
        [Tooltip("Event triggered when the player stays in the trigger zone.")]
        [SerializeField, EnableIf("m_stay")]
        private TriggerEvent m_onTriggerStay = new();

        [HorizontalGroup("Settings/Exit", width: 16f)]
        [Tooltip("If true, the event will be triggered when objects exits the trigger zone.")]
        [SerializeField, HideLabel]
        private bool m_exit = true;

        [HorizontalGroup("Settings/Exit")]
        [Tooltip("Event triggered when the player exits the trigger zone.")]
        [SerializeField, EnableIf("m_exit")]
        private TriggerEvent m_onTriggerExit = new();

        [FoldoutGroup("Gizmos")]
        [Tooltip("The color of the gizmos connections for triggers enter.")]
        [SerializeField, ColorPalette]
        private Color m_gizmoColorTriggerEnter = Color.green;

        [FoldoutGroup("Gizmos")]
        [Tooltip("The color of the gizmos connections for triggers stay.")]
        [SerializeField, ColorPalette]
        private Color m_gizmoColorTriggerStay = Color.yellow;

        [FoldoutGroup("Gizmos")]
        [Tooltip("The color of the gizmos connections for triggers exit.")]
        [SerializeField, ColorPalette]
        private Color m_gizmoColorTriggerExit = Color.red;

        #endregion

        #region Validation -------------------------------------------------------------------------

        private void OnValidate()
        {
#if UNITY_EDITOR
            this.m_onTriggerEnter.TriggerEventCheckAuto(this);
            this.m_onTriggerExit.TriggerEventCheckAuto(this);
            this.m_onTriggerStay.TriggerEventCheckAuto(this);
#endif
        }

        #endregion

        #region Members -----------------------------------------------------------------------------

        private bool m_isTriggered = false;

        #endregion

        #region Trigger -----------------------------------------------------------------------------

        private void OnTriggerEnter(Collider other)
        {
            if (this.m_enter)
            {
                TriggerEvent(this.m_onTriggerEnter, other);
            }
        }

        private void OnTriggerStay(Collider other)
        {
            if (this.m_stay)
            {
                TriggerEvent(this.m_onTriggerStay, other);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (this.m_exit)
            {
                TriggerEvent(this.m_onTriggerExit, other);
            }
        }

        #endregion

        #region Private Methods ---------------------------------------------------------------------

        private void TriggerEvent(TriggerEvent triggerEvent, Collider other)
        {
            if (((1 << other.gameObject.layer) & this.m_layer.value) != 0)
            {
                triggerEvent?.Invoke(this.transform);
            }

            if (!this.m_isTriggered && this.m_triggerOnce)
            {
                this.gameObject.SetActive(false);
            }
        }

        #endregion

        #region Gizmos ------------------------------------------------------------------------------

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

#if UNITY_EDITOR
            this.m_onTriggerEnter.DrawConnectionGizmo(this.transform, this.m_gizmoColorTriggerEnter);
            this.m_onTriggerStay.DrawConnectionGizmo(this.transform, this.m_gizmoColorTriggerStay);
            this.m_onTriggerExit.DrawConnectionGizmo(this.transform, this.m_gizmoColorTriggerExit);
#endif
        }

        #endregion
    }
}