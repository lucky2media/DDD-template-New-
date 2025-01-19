using System.Collections.Generic;
using DDD.Scripts.Core.HOG.Core;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DDD.Scripts.Core
{
    public class DDDPoolManager
    {
        private Dictionary<PoolNames, DDDPool> Pools = new();
        
        private Transform rootPools;

        public DDDPoolManager()
        {
            rootPools = new GameObject("PoolsHolder").transform;
            Object.DontDestroyOnLoad(rootPools);
        }
        
        public void InitPool(PoolNames poolName, int amount)
        {
            
        }
        
        public void InitPool(string resourceName, int amount, int maxAmount = 100)
        {
            var original = Resources.Load<DDDPoolable>(resourceName);
            InitPool(original, amount,  maxAmount);
        }
        
        public void InitPool(DDDPoolable original, int amount, int maxAmount)
        {
            DDDManager.Instance.FactoryManager.MultiCreateAsync(original, Vector3.zero, amount, 
                delegate(List<DDDPoolable> list)
                {
                    foreach (var poolable in list)
                    {
                        poolable.name = original.name;
                        poolable.transform.parent = rootPools;
                        poolable.gameObject.SetActive(false);
                    }
                    
                    var pool = new DDDPool
                    {
                        AllPoolables = new Queue<DDDPoolable>(list),
                        UsedPoolables = new Queue<DDDPoolable>(),
                        AvailablePoolables = new Queue<DDDPoolable>(list),
                        MaxPoolables = maxAmount
                    };

                    Pools.Add(original.poolName, pool);
                });
        }

        public DDDPoolable GetPoolable(PoolNames poolName)
        {
            if (Pools.TryGetValue(poolName, out DDDPool pool))
            {
                if (pool.AvailablePoolables.TryDequeue(out DDDPoolable poolable))
                {

                    poolable.OnTakenFromPool();
                    
                    pool.UsedPoolables.Enqueue(poolable);
                    poolable.gameObject.SetActive(true);
                    return poolable;
                }
                

                return null;
            }

            return null;
        }
        
        
        public void ReturnPoolable(DDDPoolable poolable)
        {
            if (Pools.TryGetValue(poolable.poolName, out DDDPool pool))
            {
                pool.AvailablePoolables.Enqueue(poolable);
                poolable.OnReturnedToPool();
                poolable.gameObject.SetActive(false);
            }
        }


        public void DestroyPool(PoolNames name)
        {
            if (Pools.TryGetValue(name, out DDDPool pool))
            {
                foreach (var poolable in pool.AllPoolables)
                {
                    poolable.PreDestroy();
                    ReturnPoolable(poolable);
                }
                
                foreach (var poolable in pool.AllPoolables)
                {
                    Object.Destroy(poolable);
                }

                pool.AllPoolables.Clear();
                pool.AvailablePoolables.Clear();
                pool.UsedPoolables.Clear();
                
                Pools.Remove(name);
            }
        }
    }
}