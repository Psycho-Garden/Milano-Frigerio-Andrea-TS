using UnityEngine;
using Sirenix.OdinInspector;

namespace AF.TS.Utils
{
    /// <summary>
    /// Generic MonoBehaviour Singleton pattern.
    /// </summary>
    [HideMonoScript]
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T m_instance;
        private static readonly object m_lock = new object();

        #region Fields ------------------------------------------------------------------ 

        [Title("Singleton settings")]
        [Tooltip("If true, this instance will persist across scene loads.")]
        [SerializeField]
        private bool m_dontDestroyOnLoad = false;

        #endregion

        #region Public methods ----------------------------------------------------------

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance '{typeof(T)}' already destroyed on application quit.");
                    return null;
                }

                if (m_instance != null) return m_instance;

                lock (m_lock)
                {
                    if (m_instance == null)
                    {
                        m_instance = FindFirstObjectByType<T>();

                        if (m_instance != null && FindObjectsByType<T>(FindObjectsSortMode.None).Length > 1)
                        {
                            Debug.LogError($"[Singleton] Multiple instances of singleton {typeof(T)} detected!");
                            return m_instance;
                        }

                        if (m_instance == null)
                        {
                            GameObject singletonObj = new GameObject($"{typeof(T)} (Singleton)");
                            m_instance = singletonObj.AddComponent<T>();
                        }
                    }

                    return m_instance;
                }
            }
        }

        #endregion

        #region Private methods ---------------------------------------------------------

        private static bool applicationIsQuitting = false;

        protected virtual void Awake()
        {
            if (m_instance == null || m_instance == this)
            {
                m_instance = this as T;

                if (m_dontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
            else
            {
                Debug.LogWarning($"[Singleton] Duplicate instance of {typeof(T)} found. Destroying the new one.");
                Destroy(gameObject);
            }
        }

        protected virtual void OnApplicationQuit()
        {
            applicationIsQuitting = true;
        }

        #endregion

    }
}