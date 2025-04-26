using UnityEngine;
using System;
using Sirenix.OdinInspector;
using AF.TS.Weapons;

namespace AF.TS.Characters
{
    [Serializable]
    public class DroneOverloadState : BaseState
    {
        #region Exposed Members --------------------------------------------------------------------

        [FoldoutGroup("Settings")]
        [Tooltip("Duration of the overload firing phase"), Unit(Units.Second)]
        [SerializeField, MinValue(0f)]
        private float m_overloadDuration = 3f;

        [FoldoutGroup("Settings")]
        [Tooltip("Cooldown duration after overload"), Unit(Units.Second)]
        [SerializeField, MinValue(0f)]
        private float m_cooldownDuration = 4f;

        [FoldoutGroup("Settings")]
        [Tooltip("Index of the next state to transition into after cooldown")]
        [SerializeField, MinValue(0)]
        private int m_nextStateIndex = 1;

        #endregion

        #region Private Members ---------------------------------------------------------------------

        private float m_timer = 0f;
        private bool m_firing = false;
        private bool m_cooling = false;

        #endregion

        #region State Methods -----------------------------------------------------------------------

        public override void OnStart(Boss boss)
        {
            base.OnStart(boss);

            this.m_timer = this.m_overloadDuration;
            this.m_firing = true;
            this.m_cooling = false;

            this.TriggerWeapons(true);
        }

        public override void OnUpdate()
        {
            if (this.m_firing)
            {
                this.m_timer -= Time.deltaTime;

                if (this.m_timer <= 0f)
                {
                    this.TriggerWeapons(false);
                    this.m_boss.OpenDoors(); // Expose cores
                    this.m_firing = false;
                    this.m_cooling = true;
                    this.m_timer = this.m_cooldownDuration;
                }
            }
            else if (this.m_cooling)
            {
                this.m_boss.ApplyHoverEffect(); // Optional hover effect during cooldown
                this.m_timer -= Time.deltaTime;

                if (this.m_timer <= 0f)
                {
                    this.m_boss.CloseDoors(); // Hide cores
                    this.m_boss.ChangeState(this.m_nextStateIndex);
                }
            }
        }

        public override void OnDispose()
        {
            this.TriggerWeapons(false);
        }

        #endregion

        #region Private Methods ---------------------------------------------------------------------

        private void TriggerWeapons(bool state)
        {
            if (state)
            {
                foreach (NewGunController weapon in this.m_boss.Weapons)
                {
                    weapon.OnFireInput();
                }
            }
            else
            {
                foreach (NewGunController weapon in this.m_boss.Weapons)
                {
                    weapon.OnFireRelease();
                }
            }
        }

        #endregion
    }

}