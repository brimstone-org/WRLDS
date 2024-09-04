using System.Collections;
using System.Linq;
using UnityEngine;

namespace _DPS
{
    /// <summary>
    /// class to hold the projectile pramaeters onto a game object
    /// switch between sphere and circle collider based on projects needs
    /// </summary>
    [RequireComponent(typeof(CircleCollider2D))]
//[RequireComponent(typeof(SphereCollider))]
    public class ProjectileComponent : MonoBehaviour
    {
        [Header("refs")] 
        public GameObject AreaDamage;
        public GameObject Splash;
        public SpriteRenderer[] ProjectileImages;
        public Transform ProjectileEmmiter;
        public Collider2D Collector;
    
        [Header("params")]
        public float ProjectileSpeed;
        public int ProjectileDamage;
        public ShootingSystem.GunTypes GunType;
        public ShootingSystem.FiringModes FireMode;
        public Vector3 Target;
        public Transform TrackedTarget;
        [SerializeField] private float _autoKillTimer = 3f;
        private bool hasTarget, isActive, autoKill;
        private float _aInternalTimer;
        
        //private Vector3 lastTargetPosition;

        public void SetAutoRadius()
        {
            //2d canvas
            if (GetComponent<CircleCollider2D>() && GetComponent<RectTransform>())
            {
                GetComponent<CircleCollider2D>().isTrigger = true;
                GetComponent<CircleCollider2D>().radius = GetComponent<RectTransform>().rect.width / 2 * GetComponent<RectTransform>().localScale.normalized.magnitude;
            }

        }

        public void Shoot()
        {
            for (int i = 0; i < ProjectileImages.Length; i++)
            {
                ProjectileImages[i].enabled = true;
            }
            
            isActive = true;
            autoKill = true;
            _aInternalTimer = _autoKillTimer;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (isActive && 
                Collector.GetComponentsInChildren<BoxCollider2D>().Contains(other) || 
                other == ShootingSystem.Instance.ProjectileCollector)
            {
                isActive = false;
                gameObject.SetActive(false);
            }

            if (isActive && other.GetComponent<FallingObjectComponent>() != null && other.GetComponent<NpcController>() != null)
            {
                var arrow = other.GetComponent<FallingObjectComponent>();
                
                for (int i = 0; i < ProjectileImages.Length; i++)
                {
                    ProjectileImages[i].enabled = false;
                }

                if (!arrow.isCollectible)
                {
                    other.enabled = false;
                    arrow.ObjIsShot = true;
                    ShootingSystem.FallingObjEvent?.Invoke(arrow);
                    explodeC = StartCoroutine(Explode(FireMode == ShootingSystem.FiringModes.AreaSplash));
                    isActive = false;
                    arrow.DisableFallingObj();
                }

            }
        }

        private Coroutine explodeC;
        private IEnumerator Explode(bool explode)
        {
            if (Time.timeScale < .1)
            {
                yield return StartCoroutine(GameManager.HandlePauseState());
            }
            if (!explode)
            {
                //normal splash
                StartCoroutine(EnableSplash());
            }
            else
            {

                StartCoroutine(EnableAreaSplash());
            }
            if (Time.timeScale < .1)
            {
                yield return StartCoroutine(GameManager.HandlePauseState());
            }

            GetComponent<Collider2D>().enabled = true;
            for (int i = 0; i < ProjectileImages.Length; i++)
            {
                ProjectileImages[i].enabled = false;
            }

        }

        private IEnumerator EnableAreaSplash()
        {
            AreaDamage.SetActive(true);
            yield return new WaitForSeconds(0.5f);
            AreaDamage.SetActive(false);
            transform.localPosition = Vector3.zero;
            gameObject.SetActive(false);
        }
        private IEnumerator EnableSplash()
        {
            Splash.SetActive(true);

            yield return new WaitForSeconds(.15f);
            Splash.SetActive(false);
            transform.localPosition = Vector3.zero;
            gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            transform.localPosition = Vector3.zero;
            GetComponent<Collider2D>().enabled = true;
            for (int i = 0; i < ProjectileImages.Length; i++)
            {
                ProjectileImages[i].enabled = false;
            }
        }

        void FixedUpdate()
        {
            if(!isActive) return;
            if (autoKill)
            {
                _aInternalTimer -= Time.fixedDeltaTime;
                if (_aInternalTimer <= 0)
                {
                    autoKill = false;
                    isActive = false;
                    gameObject.SetActive(false);
                    return;
                }
                //start timer

            }
            transform.Translate(Target.x * ProjectileSpeed * Time.fixedDeltaTime,
                Target.y * ProjectileSpeed * Time.fixedDeltaTime, 0);
//        if (TrackedTarget == null)
//        {
//            transform.Translate(Target.x * ProjectileSpeed * Time.fixedDeltaTime,
//                Target.y * ProjectileSpeed * Time.fixedDeltaTime, 0);
//        }
//        else
//        {
//            transform.Translate(TrackedTarget.position.x * ProjectileSpeed * Time.fixedDeltaTime,
//                TrackedTarget.position.y * ProjectileSpeed * Time.fixedDeltaTime, 0);
//        }
        }
    }
}
