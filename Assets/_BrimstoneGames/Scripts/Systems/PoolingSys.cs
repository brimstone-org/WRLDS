using System.Collections.Generic;
using UnityEngine;

namespace _DPS
{

    public class PoolingSys : MonoBehaviour
    {
        public List<Pool> PoolsList;
        /// <summary>
        /// int is poolId, GO array is the elements pool
        /// </summary>
        public Dictionary<int, GameObject[]> Pools = new Dictionary<int, GameObject[]>();

        [System.Serializable]
        public class Pool
        {
            public Transform ElementHolder;
            public GameObject Element;
            public GameObject[] Elements;
        }

        void Start()
        {
            Init();
        }

        /// <summary>
        /// call this to init pools manually
        /// </summary>
        public void Init()
        {
            for (int i = 0; i < PoolsList.Count; i++)
            {
                for (int j = 0; j < PoolsList[i].Elements.Length; j++)
                {
                    var e = Instantiate(PoolsList[i].Element, PoolsList[i].ElementHolder);
                    PoolsList[i].Elements[j] = e;
                }
                Pools.Add(i, PoolsList[i].Elements);
            }
        }

        public void TestPoolGet(int poolId)
        {
            global::Logger.Log("found obj active in pool " + GetObjFromPool(poolId).name);
        }

        /// <summary>
        /// gets the first active object in specified pool
        /// </summary>
        /// <param name="poolId"></param>
        /// <returns></returns>
        public GameObject GetObjFromPool(int poolId)
        {
            var activePool = Pools.ContainsKey(poolId);
            if (activePool)
            {
                for (int i = 0; i < Pools[poolId].Length; i++)
                {
                    if (Pools[poolId][i].activeSelf)
                    {
                        return Pools[poolId][i];
                    }
                }
            }

            return null;
        }
    }
}
