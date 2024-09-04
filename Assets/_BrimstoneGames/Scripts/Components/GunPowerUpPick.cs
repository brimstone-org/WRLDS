using System.Collections;
using UnityEngine;

namespace _DPS
{
    [RequireComponent(typeof(CircleCollider2D))]
    //[RequireComponent(typeof(Rigidbody2D))]
    public class GunPowerUpPick : MonoBehaviour
    {
        [Header("Refs")] public SpriteRenderer GunImage;
        public SpriteRenderer BackGroundFillImage;
        public SpriteRenderer BorderImage;

        [Header("Params")]
        public int GunPowerUpId = -1;
        public ShootingSystem.FiringModes FireModes = ShootingSystem.FiringModes.Single;
        public ShootingSystem.GunTypes GunType;
        public float FireRate = 1;
        public float ProjectileSpeed = 1;
        public Collider2D CatcherOverride { get; set; }

        public void StartFalling(float speed)
        {
            GetComponent<Rigidbody2D>().velocity = new Vector2(0,- speed);
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            //global::Logger.Log("entered collision with " + colliderObj);

            if (other.CompareTag("Player") && !other.isTrigger)
            {
                //success
                //global::Logger.Log("heh");
                foreach(var rend in GetComponentsInChildren<Renderer>())
                {
                    rend.enabled = false;
                }

                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                ShootingSystem.GunPick?.Invoke(this);

            }

            if (other == CatcherOverride || other == ShootingSystem.Instance.CollectorCollider)
            {
                GetComponent<Rigidbody2D>().velocity = Vector2.zero;
                StartCoroutine(DestroyWithDelay());
            }
        }

        private IEnumerator DestroyWithDelay()
        {
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }

        void OnDestroy()
        {
            ShootingSystem.RemoveGunFromSpawnedList?.Invoke(this);
        }
    }
}
