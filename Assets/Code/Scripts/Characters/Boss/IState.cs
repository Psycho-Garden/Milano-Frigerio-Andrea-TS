namespace AF.TS.Characters
{
    public interface IState
    {
        public void OnStart(Boss boss);
        public void OnUpdate();
        public void OnFixedUpdate();
        public void OnDispose();
        public void OnDrawGizmos();
        public void OnDrawGizmosSelected();
    }
}