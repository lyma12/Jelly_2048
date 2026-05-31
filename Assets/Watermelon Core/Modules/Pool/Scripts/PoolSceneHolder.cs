using UnityEngine;

namespace Watermelon
{
    [DefaultExecutionOrder(-50)]
    public class PoolSceneHolder : MonoBehaviour
    {
        [SerializeField] Pool[] pools;

        private void Awake()
        {
            foreach(Pool pool in pools)
            {
                Debug.LogError("ini");
                pool.Init();
            }
        }

        private void OnDestroy()
        {
            foreach (Pool pool in pools)
            {
                PoolManager.DestroyPool(pool);
            }
        }
    }
}