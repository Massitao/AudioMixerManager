using UnityEngine;

namespace Massitao.Pattern.ObjectPool
{
    public class ObjectPoolTrack : MonoBehaviour
    {
        [Header("Object Pool Tracker Owner")]
        [SerializeField] private ObjectPool pooledObjectOwner;

        public void SetPooledObjectOwner(ObjectPool objectPool)
        {
            pooledObjectOwner = objectPool;
        }
        public void ForceReturnToPool()
        {
            pooledObjectOwner.ReturnToPool(gameObject);
        }

        private void OnDisable()
        {
            ForceReturnToPool();
        }
    }
}