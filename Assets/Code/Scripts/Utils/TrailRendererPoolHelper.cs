using UnityEngine;
using Sirenix.OdinInspector;

namespace AF.TS.Utils
{
    [HideMonoScript]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(TrailRenderer))]
    public class TrailRendererPoolHelper : MonoBehaviour
    {
        private TrailRenderer[] m_trailRenderers;

        private void Awake()
        {
            this.m_trailRenderers = GetComponentsInChildren<TrailRenderer>();
        }

        private void OnEnable()
        {
            foreach (var trail in this.m_trailRenderers)
            {
                trail.Clear();
                trail.enabled = false;
                trail.enabled = true;
            }
        }
    }
}