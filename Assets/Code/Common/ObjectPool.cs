using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

namespace ho
{
    // this is not in its own class in order to not let unity add this behaviour directly as a component
    public class PoolableBehaviour : MonoBehaviour
    {
        ObjectPool pool;

        public void SetPool(ObjectPool pool)
        {
            this.pool = pool;
        }

        protected virtual void OnDisable()
        {
            pool.Release(gameObject);
        }
    }

    public class ObjectPool : MonoBehaviour
    {
        protected Stack<GameObject> instances = new Stack<GameObject>();

        [SerializeField]
        protected GameObject objectPrefab;

        public void Release(GameObject go)
        {
            go.SetActive(false);
            instances.Push(go);
        }

        public GameObject Fetch()
        {
            if (instances.Count == 0)
            {
                return Create();
            }

            GameObject go = instances.Pop();
            go.SetActive(true);
            return go;
        }

        public T Fetch<T>() where T : MonoBehaviour
        {
            return Fetch().GetComponent<T>();
        }

        public virtual GameObject Create()
        {
            GameObject go = Instantiate(objectPrefab, transform, false);
            PoolableBehaviour poolable = go.GetComponent<PoolableBehaviour>();
            if (poolable == null)
            {
                Debug.LogError($"Prefab in ObjectPool {name} has no PoolableBehaviour component");
            } else
            {
                poolable.SetPool(this);
            }

            return go;
        }
    }
}
