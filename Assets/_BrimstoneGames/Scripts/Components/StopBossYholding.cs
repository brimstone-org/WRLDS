using UnityEngine;

namespace _DPS
{

    public class StopBossYHolding : MonoBehaviour
    {
        void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                LevelBuilder.Instance.boss.GetComponent<HandleBossIntro>().StopAndParentToBall();
                //LevelBuilder.Instance.boss.GetComponent<Rigidbody2D>().isKinematic = true;

                //var bossColliders = LevelBuilder.Instance.boss.GetComponents<CircleCollider2D>();
                //for (int i = 0; i < bossColliders.Length; i++)
                //{
                //    if (!bossColliders[i].isTrigger)
                //    {
                //        bossColliders[i].enabled = false;
                //    }
                //}

            }
        }

    }
}