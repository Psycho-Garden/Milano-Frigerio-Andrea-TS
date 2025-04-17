using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;

namespace AF.TS.Utils
{
    [DefaultExecutionOrder(-100)]
    public class ServiceLocator : Singleton<ServiceLocator>
    {
        #region Fields

#if UNITY_EDITOR
        [ShowInInspector, ReadOnly]
        private List<UnityEngine.Object> m_editorServicesView = new();
#endif

        #endregion

        #region Static Dictionaries

        private static readonly Dictionary<Type, object> m_services = new();

        // Services with specific interfaces
        private static readonly Dictionary<Type, IUpdateService> m_updateServices = new();
        private static readonly Dictionary<Type, IFixedUpdateService> m_FixedUpdateServices = new();

        #endregion

        #region Unity Callbacks

        private void Start()
        {
#if UNITY_EDITOR
            PopulateSerializedServices();
#endif
        }

        private void Update()
        {
            foreach (IUpdateService service in m_updateServices.Values)
            {
                service.OnUpdate();
            }
        }

        private void FixedUpdate()
        {
            foreach (IFixedUpdateService service in m_FixedUpdateServices.Values)
            {
                service.OnFixedUpdate();
            }
        }

        #endregion

        #region Registration & Unregistration

        public static void Register<T>(T service)
        {
            var type = typeof(T);

            if (m_services.ContainsKey(type))
            {
                Debug.LogWarning($"Service of type {type.Name} is already registered. Overwriting.");
            }

            m_services[type] = service;

            // Interface-based categorization
            if (service is IUpdateService updateService)
            {
                m_updateServices[type] = updateService;
            }

            if (service is IFixedUpdateService fixedUpdateService)
            {
                m_FixedUpdateServices[type] = fixedUpdateService;
            }

#if UNITY_EDITOR
            Instance?.PopulateSerializedServices();
#endif
        }

        public static void Unregister<T>()
        {
            var type = typeof(T);

            if (m_services.TryGetValue(type, out var service))
            {
                // Clean up from specialized dictionaries
                if (service is IUpdateService)
                {
                    m_updateServices.Remove(type);
                }

                if (service is IFixedUpdateService)
                {
                    m_FixedUpdateServices.Remove(type);
                }

                m_services.Remove(type);

#if UNITY_EDITOR
                Instance?.PopulateSerializedServices();
#endif
            }
        }

        #endregion

        #region Getters

        public static T Get<T>()
        {
            if (m_services.TryGetValue(typeof(T), out var service))
            {
                return (T)service;
            }

            throw new KeyNotFoundException($"Service of type {typeof(T).Name} not found.");
        }

        public static IEnumerable<object> GetAllServices() => m_services.Values;

        #endregion

#if UNITY_EDITOR
        #region Editor-Only

        private void PopulateSerializedServices()
        {
            m_editorServicesView.Clear();

            foreach (var entry in m_services)
            {
                if (entry.Value is UnityEngine.Object unityObj)
                {
                    m_editorServicesView.Add(unityObj);
                }
            }
        }

        #endregion
#endif
    }
}
