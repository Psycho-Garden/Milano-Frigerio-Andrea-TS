using UnityEngine;
using System;
using Sirenix.OdinInspector;

namespace AF.TS.Weapons
{
    public interface INewShootingMode
    {
        public void Init(NewGunController controller);
        public void TriggerPressed();
        public void TriggerReleased();
        public void OnUpdate();
        public void Shoot();
    }

    [Serializable]
    public class ShootModeBase : INewShootingMode
    {
        protected NewGunController m_controller;

        public virtual void Init(NewGunController controller)
        {
            this.m_controller = controller;
        }

        public virtual void OnUpdate() { }

        public virtual void TriggerPressed() { }

        public virtual void TriggerReleased() { }

        public virtual void Shoot()
        {
            this.m_controller.ShootLogic();
            this.m_controller.Feedback();
        }
    }

    [Serializable]
    public class SemiAutoMode : ShootModeBase
    {
        public override void TriggerPressed()
        {
            if (this.m_controller.CanShoot())
            {
                Shoot();
            }
        }
    }

    [Serializable]
    public class BurstMode : ShootModeBase
    {
        [SerializeField, MinValue(1)]
        private int m_burstCount = 3;

        private int m_shotsRemaining = 0;
        private float m_lastShotTime = 0f;

        public override void TriggerPressed()
        {
            if (this.m_shotsRemaining <= 0 && this.m_controller.CanShoot())
            {
                this.m_shotsRemaining = this.m_burstCount;
            }
        }

        public override void OnUpdate()
        {
            if (this.m_shotsRemaining > 0 && Time.time >= this.m_lastShotTime + this.m_controller.ShootRate)
            {
                if (this.m_controller.CanShoot())
                {
                    Shoot();
                    this.m_shotsRemaining--;
                    this.m_lastShotTime = Time.time;
                }
                else
                {
                    this.m_shotsRemaining = 0;
                }
            }
        }

        public override void TriggerReleased()
        {
            this.m_shotsRemaining = 0;
        }
    }

    [Serializable]
    public class FullAutoMode : ShootModeBase
    {
        private float m_lastShotTime = 0f;
        private bool m_triggerHeld = false;

        public override void TriggerPressed()
        {
            this.m_triggerHeld = true;
        }

        public override void TriggerReleased()
        {
            this.m_triggerHeld = false;
        }

        public override void OnUpdate()
        {
            if (this.m_triggerHeld && Time.time >= this.m_lastShotTime + this.m_controller.ShootRate)
            {
                if (this.m_controller.CanShoot())
                {
                    Shoot();
                    this.m_lastShotTime = Time.time;
                }
            }
        }
    }

}
