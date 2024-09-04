using UnityEngine;

namespace _DPS
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class MagnetCollectibles : MonoBehaviour
    {
        public bool IsBoss;
        public float Radius;
        public float Speed;
        //public List<Transform> PickUps = new List<Transform>();

        void OnTriggerEnter2D(Collider2D other)
        {
            if (IsBoss)
            {
                if (other.GetComponent<BossAnimalCollector>() && !other.GetComponent<BossAnimalCollector>().IsCought)
                {
                    other.GetComponent<BossAnimalCollector>().IsCought = true;
                    other.GetComponent<BossAnimalCollector>().Catcher = transform;
                }
                return;
            }

            if (other.GetComponent<ScorePickComponent>() && 
                !other.GetComponent<ScorePickComponent>().IsCought &&
                other.GetComponent<NpcController>() == null
                )
            {
                other.GetComponent<ScorePickComponent>().IsCought = true;

                if (other.GetComponent<FallingObjectComponent>() != null )
                {
                    other.GetComponent<FallingObjectComponent>().isCought = true;
                }


/*                if (other.gameObject != null && !PickUps.Contains(other.transform))
                {
                    PickUps.Add(other.transform);
                }*/
            }

            //catch simple score objects
            if (other.GetComponent<StaticScorePick>() && !other.GetComponent<StaticScorePick>().isCought)
            {
                other.GetComponent<StaticScorePick>().isCought = true;
                //other.gameObject.SetActive(false);
            }

        }
/*
        void Update()
        {
            var tmp = new List<Transform>();
            tmp = PickUps;
            for (int i = 0; i < tmp.Count; i++)
            {
                if (tmp[i] != null)
                {
                    tmp[i].position = Vector3.MoveTowards(tmp[i].position, transform.position, Time.deltaTime * Speed);
                }
            }
/*            foreach (var pick in PickUps)
            {
                if (pick.gameObject == null)
                {
                    tmp.Add(pick.gameObject);
                    continue;
                }
                
            }

            foreach (var p in tmp)
            {
                Destroy(p);
            }#1#
        }*/
    }
}
