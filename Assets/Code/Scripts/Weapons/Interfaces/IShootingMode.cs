using System;

namespace AF.TS.Weapons
{
    [Obsolete("Use the version for NewGunController", true)]
    public interface IShootingMode
    {
        void Init(GunController controller);
        void Shoot();
        void OnUpdate();
    }
}