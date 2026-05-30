using System.Collections.Generic;
using UnityEngine;

namespace Core.Support.Performance
{
    public static class ObjectPoolManager
    {
        private static readonly Dictionary<int, RuntimeObjectPool> pools = new Dictionary<int, RuntimeObjectPool>();
        private static readonly Dictionary<GameObject, int> objectPrefabMap = new Dictionary<GameObject, int>();

        public static RuntimeObjectPool GetPool(GameObject prefab, Transform root = null)
        {
            if (prefab == null)
            {
                return null;
            }

            int id = prefab.GetInstanceID();
            if (!pools.ContainsKey(id))
            {
                pools[id] = new RuntimeObjectPool(prefab, root);
            }

            return pools[id];
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation, Transform parent = null)
        {
            if (prefab == null)
            {
                return null;
            }

            RuntimeObjectPool pool = GetPool(prefab, parent);
            if (pool != null)
            {
                GameObject instance = pool.Get(position, rotation, parent);
                objectPrefabMap[instance] = prefab.GetInstanceID();
                return instance;
            }

            GameObject obj = Object.Instantiate(prefab, position, rotation, parent);
            objectPrefabMap[obj] = prefab.GetInstanceID();
            return obj;
        }

        public static void Release(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            if (objectPrefabMap.TryGetValue(instance, out int prefabId) && pools.TryGetValue(prefabId, out RuntimeObjectPool pool))
            {
                objectPrefabMap.Remove(instance);
                pool.Release(instance);
                return;
            }

            Object.Destroy(instance);
        }

        public static void ClearAll()
        {
            foreach (RuntimeObjectPool pool in pools.Values)
            {
                pool.Clear();
            }

            pools.Clear();
            objectPrefabMap.Clear();
        }

        public static void WarmPool(GameObject prefab, int count, Transform root = null)
        {
            RuntimeObjectPool pool = GetPool(prefab, root);
            pool?.Warm(count);
        }
    }
}
