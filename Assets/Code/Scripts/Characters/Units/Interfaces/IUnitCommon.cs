namespace AF.TS.Characters
{
    public interface IUnitCommon
    {
        void OnStartup(Character character);
        void AfterStartup(Character character);
        void OnDispose(Character character);

        void OnEnable();
        void OnDisable();

        void OnUpdate();
        void OnFixedUpdate();

        void OnDrawGizmos(Character character);
    }

}
