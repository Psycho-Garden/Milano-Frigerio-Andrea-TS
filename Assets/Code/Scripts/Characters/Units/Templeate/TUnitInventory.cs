using System;
using UnityEngine;

namespace AF.TS.Characters
{
    [Serializable]
    public abstract class TUnitInventory : TUnit, IUnitInventory
    {
        #region MEMBERS: ------------------------------------------------------------------------

        #endregion

        #region  INTERFACE PROPERTIES: ----------------------------------------------------------

        public void AddItem(GameObject item)
        {
            throw new NotImplementedException();
        }

        public void RemoveItem(GameObject item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region INITIALIZERS: -------------------------------------------------------------------

        public virtual void OnStartup(Character character)
        {
            this.Character = character;
        }

        public virtual void AfterStartup(Character character)
        {
            this.Character = character;
        }

        public virtual void OnDispose(Character character)
        {
            this.Character = character;
        }

        public virtual void OnEnable()
        { }

        public virtual void OnDisable()
        { }

        #endregion

        #region UPDATE METHODS: -----------------------------------------------------------------

        public virtual void OnUpdate()
        { }

        public virtual void OnFixedUpdate()
        { }

        public virtual void OnDrawGizmos(Character character)
        { }

        #endregion
    }

}
