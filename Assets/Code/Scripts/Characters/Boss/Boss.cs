using UnityEngine;
using Sirenix.OdinInspector;
using System;

namespace AF.TS.Characters
{
    [HideMonoScript]
    public class Boss : MonoBehaviour
    {
        #region Exposed Members -----------------------------------------------------------------

        [BoxGroup("States Settings")]
        [Tooltip("The states of the boss")]
        [SerializeReference]
        private BaseState[] m_states = new BaseState[0];

        [FoldoutGroup("Debug")]
        [Tooltip("The current state of the boss")]
        [ShowInInspector, ReadOnly]
        private bool m_debugLog = true;

        [FoldoutGroup("Debug")]
        [Tooltip("The current state of the boss")]
        [ShowInInspector, ReadOnly]
        private BaseState m_currentState;

        [FoldoutGroup("Debug")]
        [Tooltip("The type of gizmos to draw")]
        [SerializeField]
        private GizmosType m_drawGizmos = GizmosType.None;

#if UNITY_EDITOR

        [FoldoutGroup("Debug")]
        [SerializeField, InlineProperty, HideLabel] private HealthSystemEditorHelper m_editorHelper = new();

        private void OnValidate()
        {
            m_editorHelper.OnValidate(this.gameObject);
        }

        private void OnTransformChildrenChanged()
        {
            m_editorHelper.OnTransformChildrenChanged(this.gameObject);
        }

#endif

        #endregion

        #region Private Members -----------------------------------------------------------------
        #endregion

        #region Callback Methods ----------------------------------------------------------------

        private void Start()
        {
            m_currentState = m_states[0];
            m_currentState.OnStart(this);
        }

        private void OnDestroy()
        {
            m_currentState.OnDispose();
        }

        #endregion

        #region Update Methods ------------------------------------------------------------------

        private void Update()
        {
            m_currentState.OnUpdate();
        }

        private void FixedUpdate()
        {
            m_currentState.OnFixedUpdate();
        }

        #endregion

        #region Public Methods ------------------------------------------------------------------
        
        /// <summary>
        /// Change the state of the boss by state
        /// </summary>
        /// <param name="state">New state</param>
        public void ChangeState(BaseState state)
        {
            Debug.Log($"[<Color=green>Boss</colore>] Transition to state: {state.GetType().Name}");

            this.m_currentState.OnDispose();
            this.m_currentState = state;
            this.m_currentState.OnStart(this);
        }

        /// <summary>
        /// Change the state of the boss by index
        /// </summary>
        /// <param name="index">Index of the state in the array</param>
        public void ChangeState(int index)
        {
            if (index < 0 || index >= this.m_states.Length)
            {
                Debug.LogWarning($"[<Color=Red>Boss</colore>] Invalid state index: {index}");
                return;
            }

            ChangeState(this.m_states[index]);
        }

        #endregion

        #region Gizmos Methods ------------------------------------------------------------------

        private void OnDrawGizmos()
        {
            if (m_drawGizmos != GizmosType.All)
            {
                return;
            }

            m_currentState.OnDrawGizmos();
        }

        private void OnDrawGizmosSelected()
        {
            if (m_drawGizmos != GizmosType.Selected)
            {
                return;
            }

            m_currentState.OnDrawGizmosSelected();
        }

        #endregion
    }

    [Serializable]
    public enum GizmosType { None, All, Selected }

    public interface IState
    {
        public void OnStart(Boss boss);
        public void OnUpdate();
        public void OnFixedUpdate();
        public void OnDispose();
        public void OnDrawGizmos();
        public void OnDrawGizmosSelected();
    }

    [Serializable]
    public class BaseState : IState
    {
        protected Boss m_boss;

        public virtual void OnStart(Boss boss)
        {
            this.m_boss = boss;
        }

        public virtual void OnDispose() { }

        public virtual void OnFixedUpdate() { }

        public virtual void OnUpdate() { }

        public virtual void OnDrawGizmos() { }

        public virtual void OnDrawGizmosSelected() { }
    }
}