using UnityEngine;

namespace DDD.Scripts.Core.HOG.Core
{
    public class DDDPoolable : MonoBehaviour
    {
        public PoolNames poolName;

        public virtual void OnReturnedToPool()
        {
            this.gameObject.SetActive(false);
        }
        
        public virtual void OnTakenFromPool()
        {
            this.gameObject.SetActive(true);
        }
        
        public virtual void PreDestroy()
        {
        }
    }
}