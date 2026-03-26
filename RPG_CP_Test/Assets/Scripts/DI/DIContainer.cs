using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.DI
{
    public class DIContainer : MonoBehaviour
    {
        private static DIContainer _instance;
        private Dictionary<Type, object> _singletons = new Dictionary<Type, object>();
        private Dictionary<Type, Type> _transients = new Dictionary<Type, Type>();

        public static DIContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject go = new GameObject("DIContainer");
                    _instance = go.AddComponent<DIContainer>();
                    DontDestroyOnLoad(go);
                }
                return _instance;
            }
        }

        public void Register<T>(T implementation) where T : class
        {
            var type = typeof(T);
            if (!_singletons.ContainsKey(type))
                _singletons[type] = implementation;
        }

        public void RegisterTransient<TInterface, TImplementation>()
            where TInterface : class
            where TImplementation : class, TInterface, new()
        {
            _transients[typeof(TInterface)] = typeof(TImplementation);
        }

        public T Resolve<T>() where T : class
        {
            var type = typeof(T);

            if (_singletons.TryGetValue(type, out var service))
                return service as T;

            if (_transients.TryGetValue(type, out var implementation))
                return Activator.CreateInstance(implementation) as T;

            Debug.LogError($"Service {type.Name} not registered");
            return null;
        }

        public object Resolve(Type type)
        {
            if (_singletons.TryGetValue(type, out var service))
                return service;

            if (_transients.TryGetValue(type, out var implementation))
                return Activator.CreateInstance(implementation);

            Debug.LogError($"Service {type.Name} not registered");
            return null;
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;
        }
    }
}