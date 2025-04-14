using AF.TS.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace AF.TS.Characters
{
    [HideMonoScript]
    public class Character : MonoBehaviour
    {
        #region Exposed Members: -----------------------------------------------------------------------

        [SerializeField] private bool m_isPlayer = false;

        [SerializeReference, InlineProperty, HideLabel] protected TUnitMotion m_Motion = new UnitMotion();
        [SerializeReference, InlineProperty, HideLabel] protected TUnitDriver m_Driver;
        [SerializeReference, InlineProperty, HideLabel] protected TUnitStats m_Stats;
        [SerializeReference, InlineProperty, HideLabel] protected TUnitHealth m_Health;
        [SerializeReference, InlineProperty, HideLabel] protected TUnitInventory m_Inventory;

        #endregion

        #region Members: -------------------------------------------------------------------------------

        #endregion

        #region Properties: ----------------------------------------------------------------------------

        public TUnitMotion Motion => this.m_Motion;
        public TUnitDriver Driver => this.m_Driver;
        public TUnitStats Stats => this.m_Stats;
        public TUnitHealth Health => this.m_Health;
        public TUnitInventory Inventory => this.m_Inventory;

        #endregion

        #region Initializers: --------------------------------------------------------------------------

        protected virtual void Awake()
        {
            if(m_isPlayer) ServiceLocator.Register<Character>(this);

            this.m_Motion?.OnStartup(this);
            this.m_Driver?.OnStartup(this);
            this.m_Stats?.OnStartup(this);
            this.m_Health?.OnStartup(this);
            this.m_Inventory?.OnStartup(this);
        }

        protected virtual void Start()
        {
            this.m_Motion?.AfterStartup(this);
            this.m_Driver?.AfterStartup(this);
            this.m_Stats?.AfterStartup(this);
            this.m_Health?.AfterStartup(this);
            this.m_Inventory?.AfterStartup(this);
        }

        protected virtual void OnDestroy()
        {
            if (m_isPlayer) ServiceLocator.Unregister<Character>();

            this.m_Motion?.OnDispose(this);
            this.m_Driver?.OnDispose(this);
            this.m_Stats?.OnDispose(this);
            this.m_Health?.OnDispose(this);
            this.m_Inventory?.OnDispose(this);
        }

        protected virtual void OnEnable()
        {
            this.m_Motion?.OnEnable();
            this.m_Driver?.OnEnable();
            this.m_Stats?.OnEnable();
            this.m_Health?.OnEnable();
            this.m_Inventory?.OnEnable();
        }

        protected virtual void OnDisable()
        {
            if (m_isPlayer) ServiceLocator.Unregister<Character>();

            this.m_Motion?.OnDisable();
            this.m_Driver?.OnDisable();
            this.m_Stats?.OnDisable();
            this.m_Health?.OnDisable();
            this.m_Inventory?.OnDisable();
        }

        #endregion

        #region Update Methods: ------------------------------------------------------------------------

        protected virtual void Update()
        {
            this.m_Motion?.OnUpdate();
            this.m_Driver?.OnUpdate();
            this.m_Stats?.OnUpdate();
            this.m_Health?.OnUpdate();
            this.m_Inventory?.OnUpdate();
        }

        protected virtual void LateUpdate() { }

        protected virtual void FixedUpdate()
        {
            this.m_Motion?.OnFixedUpdate();
            this.m_Driver?.OnFixedUpdate();
            this.m_Stats?.OnFixedUpdate();
            this.m_Health?.OnFixedUpdate();
            this.m_Inventory?.OnFixedUpdate();
        }

        #endregion

        #region Protected Methods: ---------------------------------------------------------------------

        protected virtual void OnDrawGizmosSelected()
        {
            this.m_Motion?.OnDrawGizmos(this);
            this.m_Driver?.OnDrawGizmos(this);
            this.m_Stats?.OnDrawGizmos(this);
            this.m_Health?.OnDrawGizmos(this);
            this.m_Inventory?.OnDrawGizmos(this);
        }

        #endregion

        #region Public Methods: ------------------------------------------------------------------------

        #endregion
    }

}
