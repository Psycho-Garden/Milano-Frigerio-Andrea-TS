namespace AF.TS.Weapons
{
    public interface IShootingMode
    {
        void Init(GunController controller);
        void Shoot();
        void OnUpdate();
    }
}