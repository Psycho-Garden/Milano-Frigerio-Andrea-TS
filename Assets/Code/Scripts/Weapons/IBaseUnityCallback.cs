using UnityEngine;

namespace AF.TS.Weapons
{
    public interface IBaseUnityCallback<T>
    {
        public void OnStart(T parent);
        public void OnUpdate();
        public void OnEnable();
        public void OnDisable();
        public void OnDispose();
        public void OnDrawGizmos();
    }
}
