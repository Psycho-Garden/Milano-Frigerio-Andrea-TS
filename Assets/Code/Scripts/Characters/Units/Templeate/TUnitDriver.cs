using System;
using UnityEngine;

namespace AF.TS.Characters
{
    [Serializable]
    public abstract class TUnitDriver : TUnit, IUnitDriver
    {
        #region MEMBERS: ------------------------------------------------------------------------

        #endregion

        #region  INTERFACE PROPERTIES: ----------------------------------------------------------

        public virtual Vector2 GetMovementInput() => Vector2.zero;
        public virtual bool ShouldJump() => false;

        #endregion

        #region INITIALIZERS: -------------------------------------------------------------------

        public virtual void OnStartup(Character character) => this.Character = character;

        public virtual void AfterStartup(Character character) { }

        public virtual void OnDispose(Character character) { }

        public virtual void OnEnable() { }

        public virtual void OnDisable() { }

        #endregion

        #region UPDATE METHODS: -----------------------------------------------------------------

        public virtual void OnUpdate() { }

        public virtual void OnFixedUpdate() { }

        public virtual void OnDrawGizmos(Character character) { }

        #endregion
    }
}
