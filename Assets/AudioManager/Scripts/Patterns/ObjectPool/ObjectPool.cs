using System.Collections.Generic;
using UnityEngine;

namespace Massitao.Pattern.ObjectPool
{
    [System.Serializable] public class Pool
    {
        public GameObject objectToPool;
        public uint initialNumberOfObjects;
        public bool shouldExpand;

        public Pool(GameObject objectToPool, uint initialNumberOfObjects, bool shouldExpand)
        {
            this.objectToPool = objectToPool;
            this.initialNumberOfObjects = initialNumberOfObjects;
            this.shouldExpand = shouldExpand;
        }
    }

    public class ObjectPool : MonoBehaviour
    {
        [Header("Object Pool Settings")]
        [SerializeField] protected Pool pool;
        [SerializeField] protected Queue<GameObject> objectsPooled;


        public void InitializePool()
        {
           objectsPooled = new Queue<GameObject>();

            for (uint i = 0; i < pool.initialNumberOfObjects; i++)
            {
                InstantiateNewObject();
            }
        }
        public void InitializePool(Pool customPool)
        {
            pool = customPool;

            objectsPooled = new Queue<GameObject>();

            for (uint i = 0; i < pool.initialNumberOfObjects; i++)
            {
                InstantiateNewObject();
            }
        }
        protected void InstantiateNewObject()
        {
            GameObject newObjectGO = Instantiate(pool.objectToPool, Vector3.zero, Quaternion.identity, transform);
            newObjectGO.SetActive(false);
            objectsPooled.Enqueue(newObjectGO);

            if (!newObjectGO.TryGetComponent(out ObjectPoolTrack objectTracker))
            {
                objectTracker = newObjectGO.AddComponent<ObjectPoolTrack>();
            }
            objectTracker.SetPooledObjectOwner(this);
        }

        public GameObject GetObjectFromPool()
        {
            if (objectsPooled.Count == 0)
            {
                if (pool.shouldExpand)
                {
                    InstantiateNewObject();
                }
                else
                {
                    return null;
                }
            }

            GameObject objectToReturn = objectsPooled.Dequeue();
            objectToReturn.gameObject.SetActive(true);

            return objectToReturn;
        }
        public void ReturnToPool(GameObject objectToReturn)
        {
            if (objectToReturn.activeInHierarchy)
            {
                objectToReturn.SetActive(false);
            }

            objectsPooled.Enqueue(objectToReturn);
        }

        public void ClearPool()
        {
            for (uint i = 0; i < objectsPooled.Count; i++)
            {
                Destroy(objectsPooled.Dequeue());
            }

            objectsPooled.Clear();
        }
    }
}