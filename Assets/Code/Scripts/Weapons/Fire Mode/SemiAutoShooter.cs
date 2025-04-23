using UnityEngine;
using DG.Tweening;
using System;

namespace AF.TS.Weapons
{
    [Obsolete("Use the version for NewGunController", true)]
    public class SemiAutoShooter : IShootingMode
    {
        private GunController m_controller;
        private bool m_isWaitingToFire;

        public void Init(GunController controller)
        {
            m_controller = controller;
        }

        public void Shoot()
        {
            if (!CanShoot())
                return;

            m_isWaitingToFire = true;

            float delay = m_controller.Definition.GetFireDelay;
            m_controller.NextShootTime = Time.time + m_controller.Definition.GetFireRate;

            if (delay > 0f)
            {
                DOVirtual.DelayedCall(delay, Fire, false);
            }
            else
            {
                Fire();
            }
        }

        private void Fire()
        {
            m_isWaitingToFire = false;
            m_controller.ShootLogic();
            m_controller.VisualFeedback();
        }

        private bool CanShoot()
        {
            return !m_isWaitingToFire &&
                   m_controller.CurrentAmmo > 0 &&
                   !m_controller.IsReloading &&
                   Time.time >= m_controller.NextShootTime;
        }

        public void OnUpdate() { }
    }
}
