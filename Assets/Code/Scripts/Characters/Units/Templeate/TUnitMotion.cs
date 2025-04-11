using System;
using UnityEngine;

namespace AF.TS.Characters
{
    [Serializable]
    public abstract class TUnitMotion : TUnit, IUnitMotion
    {
        #region MEMBERS: ------------------------------------------------------------------------

        #endregion

        #region  INTERFACE PROPERTIES: ----------------------------------------------------------

        public virtual void Move(Vector2 direction)
        {
            throw new NotImplementedException();
        }

        public virtual void Jump()
        {
            throw new NotImplementedException();
        }

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
