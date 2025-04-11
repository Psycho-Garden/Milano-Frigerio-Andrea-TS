using System;

namespace AF.TS.Characters
{
    [Serializable]
    public abstract class TUnitStats : TUnit, IUnitStats
    {
        #region MEMBERS: ------------------------------------------------------------------------

        #endregion

        #region  INTERFACE PROPERTIES: ----------------------------------------------------------

        public virtual void SetStat(string name, float value)
        {
            throw new NotImplementedException();
        }

        public virtual float GetStat(string name) { throw new NotImplementedException(); }

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
