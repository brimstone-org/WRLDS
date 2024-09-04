using System.Collections;
using UnityEngine;

namespace _DPS
{
    public class GunComponent : MonoBehaviour
    {
        [Header("Refs")]
        public Transform Emmiter;
        public SpriteRenderer GunSprite;
        
        [Header("Params")]
        public int GunPowerUpId = -1;
        public ShootingSystem.FiringModes FireModes = ShootingSystem.FiringModes.Single;
        public ShootingSystem.GunTypes GunType;
        public float FireRate = 1;
        public float ProjectileSpeed = 1;
        public bool EnableShooting;
        private bool inCooldown, sideCoolDown;
        private Coroutine shootingDelay, sideShootingDelay;
        public Collider2D Catcher { get; set; }
        [SerializeField]
        private Transform sideEmmiter;
        public void StartShooting()
        {
            if (shootingDelay == null)
            {
                shootingDelay = StartCoroutine(ShootingDelay());
            }
        }

        
        private void SideShooting()
        {
            if (sideShootingDelay == null)
            {
                sideShootingDelay= StartCoroutine(SideShootingDelay());
            }
        }


        private Vector3[] SetTarget()
        {
        
            switch (GunType)
            {

                case ShootingSystem.GunTypes.BlunderBus:
                    return new []{new Vector3(-.2f,1,0), Vector3.up, new Vector3(.2f,1,0)};
                default:
                    return new []{Vector3.up};
                //return new []{new Vector3(-.1f,1,0), Vector3.up, new Vector3(.1f,1,0)};
            }
        }


        private IEnumerator ShootingDelay()
        {
            inCooldown = true;
            var targets = SetTarget();
            for (int i = 0; i < targets.Length; i++)
            {
                ShootingSystem.Shoot(GunType, FireModes, targets[i], Emmiter, ProjectileSpeed, Catcher);
            }

            var t = 1f / FireRate;
            while (t>0)
            {
                if (Time.timeScale < .1)
                {
                    yield return StartCoroutine(GameManager.HandlePauseState());
                }
                t -= Time.deltaTime;
                yield return null;
            }
            

            inCooldown = false;
            shootingDelay = null;
        }

        private IEnumerator SideShootingDelay()
        {
            sideCoolDown = true;
            var targets = new[] {new Vector3(1, 0, 0), new Vector3(-1, 0, 0)};
            for (int i = 0; i < targets.Length; i++)
            {
                ShootingSystem.Shoot(GunType, FireModes, targets[i], sideEmmiter, ProjectileSpeed, Catcher);
            }

            
            var t = 1f / FireRate * 8;
            while (t>0)
            {
                if (Time.timeScale < .1)
                {
                    yield return StartCoroutine(GameManager.HandlePauseState());
                }
                t -= Time.deltaTime;
                yield return null;
            }

            sideCoolDown = false;
            sideShootingDelay = null;
        }

        void FixedUpdate()
        {
            if(!EnableShooting) return;
            if (!inCooldown)
            {
                StartShooting();
            }

            if (!sideCoolDown)
            {
                SideShooting();
            }
        }

    }
}
