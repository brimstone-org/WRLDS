using DG.Tweening;
using System.Collections;
using UnityEngine;

namespace _DPS
{
    public class ProjectileShooter : MonoBehaviour
    {
        public GameObject Projectile;
        public Sprite[] ArrayOfSprites;
        public bool isAngled;
        [HideInInspector]
        public Transform firingPoint;
        [HideInInspector]
        public Coroutine FiringSequence;
        public float ShootingCoolDown = 2f;
        [HideInInspector]
        public GameObject[] PooledProjectiles;

        public void InitializeWaterCanons()
        {
            if (Random.value < 0.5f)
            {
                transform.position = new Vector3(transform.position.x + 10, transform.position.y,
                    transform.position.z);
                transform.rotation = !isAngled
                    ? Quaternion.Euler(transform.rotation.x, 180, transform.rotation.z)
                    : Quaternion.Euler(transform.rotation.x, 180, Random.Range(-35, 36));
            }
            else
            {
                transform.position = new Vector3(transform.position.x - 10, transform.position.y,
                    transform.position.z);
                transform.rotation = !isAngled
                    ? Quaternion.Euler(transform.rotation.x, 0, transform.rotation.z)
                    : Quaternion.Euler(transform.rotation.x, 0, Random.Range(-35, 36));
            }

        }


        public void StartShootingCoroutine()
        {
            FiringSequence = StartCoroutine(StartShooting());
        }

        /// <summary>
        /// retrieves the next pooled projectile
        /// </summary>
        private GameObject GetPooledProjectile()
        {
            for (int i = 0; i < PooledProjectiles.Length; i++)
            {
                if (!PooledProjectiles[i].activeInHierarchy && !PooledProjectiles[i].GetComponent<Projectile>().HasBeenFired)
                {
                    return PooledProjectiles[i];
                }
            }

            for (int i = 0; i < PooledProjectiles.Length; i++)
            {
                PooledProjectiles[i].GetComponent<Projectile>().HasBeenFired = false;
            }
            return GetPooledProjectile();

        }

        private IEnumerator StartShooting()
        {   
            global::Logger.Log("!!!!!!!StartedShooting");
            while (true)
            {
                if (Time.timeScale < .1)
                {
                    yield return StartCoroutine(GameManager.HandlePauseState());
                }
                var projectile = GetPooledProjectile();
                if (projectile != null)
                {
                    projectile.GetComponent<Projectile>().Fire();
                }
                
                transform.DOShakeScale(.15f, new Vector3(0, 0.2f, 0), 1, 0, false).SetEase(Ease.InOutQuart).SetAutoKill().OnComplete(() => {transform.localScale = Vector3.one; });
                
                var t = ShootingCoolDown;
                while (t > 0f)
                {
                    if (Time.timeScale < .1)
                    {
                        yield return StartCoroutine(GameManager.HandlePauseState());
                    }
                    t -= Time.deltaTime;
                    yield return null;
                }
            }
        }

        void OnDisable()
        {
            if (FiringSequence != null)
            {
                StopCoroutine(FiringSequence);
            }

        }

    }

}

