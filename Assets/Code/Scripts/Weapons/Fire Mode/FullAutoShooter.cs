using UnityEngine;
using DG.Tweening;

namespace AF.TS.Weapons
{
    public class FullAutoShooter : IShootingMode
    {
        private GunController m_controller;
        private bool m_isFiring;
        private bool m_isWaitingToFire;

        public void Init(GunController controller)
        {
            m_controller = controller;
        }

        public void Shoot()
        {
            m_isFiring = true;
        }

        public void OnUpdate()
        {
            if (!m_isFiring || m_isWaitingToFire || !CanShoot())
                return;

            m_isWaitingToFire = true;
            m_controller.NextShootTime = Time.time + m_controller.Definition.GetFireRate;

            float delay = m_controller.Definition.GetFireDelay;

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
            return m_controller.CurrentAmmo > 0 &&
                   !m_controller.IsReloading &&
                   Time.time >= m_controller.NextShootTime;
        }

        public void StopFiring()
        {
            m_isFiring = false;
        }
    }
}
