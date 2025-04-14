using UnityEngine;
using DG.Tweening;

namespace AF.TS.Weapons
{
    public class BurstShooter : IShootingMode
    {
        private GunController m_controller;
        private bool m_isBursting;
        private int m_shotsLeft;

        public void Init(GunController controller)
        {
            m_controller = controller;
            m_isBursting = false;
            m_shotsLeft = 0;
        }

        public void Shoot()
        {
            if (m_isBursting || !CanShoot())
                return;

            m_shotsLeft = 3;
            m_isBursting = true;

            FireBurstShot();
        }

        public void OnUpdate() { }

        private void FireBurstShot()
        {
            if (!CanShoot())
            {
                m_isBursting = false;
                return;
            }

            m_controller.NextShootTime = Time.time + m_controller.Definition.GetFireRate;
            m_shotsLeft--;

            float delay = m_controller.Definition.GetFireDelay;

            if (delay > 0f)
            {
                DOVirtual.DelayedCall(delay, () =>
                {
                    m_controller.ShootLogic();
                    m_controller.VisualFeedback();

                    ContinueBurst();
                }, false);
            }
            else
            {
                m_controller.ShootLogic();
                m_controller.VisualFeedback();

                ContinueBurst();
            }
        }

        private void ContinueBurst()
        {
            if (m_shotsLeft > 0)
            {
                DOVirtual.DelayedCall(m_controller.Definition.GetFireRate, FireBurstShot, false);
            }
            else
            {
                m_isBursting = false;
            }
        }

        private bool CanShoot()
        {
            return m_controller.CurrentAmmo > 0 &&
                   !m_controller.IsReloading &&
                   Time.time >= m_controller.NextShootTime;
        }
    }
}
