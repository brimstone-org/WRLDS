using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace _DPS
{
    public class FallingObjectComponent : MonoBehaviour
    {
        //public bool IsLeftArrow;
        //public Image ArrowImage;
        //public bool IsAnimal;
        public int PoolId = -1;
        public ShootingSystem.FallingObjTypes FallingobjType;
        //todo[HideInInspector]
        public NpcEntity NpcEntity;
        public float FallingSpeed = 1;
        public float FallingDamage = 1;
        public float FallingObjScale = 1;
        public SpriteRenderer FallingObjSprite;
        public GameObject Prop1;
        public Sprite[] Prop1Sprites;
        public Transform Emmiter;
        public Vector3 Target;
        private Transform _player;
        public Collider2D CatcherOverride;/* { get; set; }*/
        /// <summary>
        /// used to flag success or fail globally
        /// </summary>
        public bool ObjIsShot, ObjIsFail, isActive, isGrounded, isCollectible, isDespawning, isCought;

        private Rigidbody2D _rigidBody;

        void Awake()
        {
            if (_rigidBody == null)
            {
                _rigidBody = GetComponent<Rigidbody2D>();
                _rigidBody.drag = 0;
            }
        }

        void OnEnable()
        {
            isDespawning = false;
            isCought = false;
            if (Prop1 != null)
            {
                Prop1.SetActive(true);
                Prop1.GetComponentInChildren<SpriteRenderer>().sprite = Prop1Sprites[Random.Range(0, Prop1Sprites.Length)];
                Prop1.transform.DOKill();
                Prop1.transform.localScale = Vector3.zero;
                Prop1.transform.DOScale(Vector3.one, 0.5f).SetDelay(2f);
            }
        }

        void OnDisable()
        {
            if (Prop1 != null)
            {
                Prop1.SetActive(false);
            }
            StopAllCoroutines();
        }
        private void OnTriggerEnter2D(Collider2D other)
        {
            //global::Logger.Log("entered collision with " + colliderObj);
/*
            if (other.CompareTag("Player") && !other.isTrigger)
            {
                //success
                if (!ObjIsFail)
                {
                    ObjIsShot = true;
                    GameManager.FallingObjEvent?.Invoke(this);
                }

                if (!ObjIsShot)
                {
                    ObjIsFail = true;
                    GameManager.FallingObjEvent?.Invoke(this);
                }

            }*/
            if (other.CompareTag("Player") && !other.isTrigger)
            {
                if (_despawnWithDelay != null)
                {
                    StopCoroutine(_despawnWithDelay);
                    _despawnWithDelay = StartCoroutine(DespawnsWithDelay());
                    DisableFallingObj();
                    return;
                }

                //fail
                if (!ObjIsShot)
                {
                    ObjIsFail = true;
                    ShootingSystem.FallingObjEvent?.Invoke(this);
                }
            }
            if (/*!isCollectible && */(other == CatcherOverride || other == ShootingSystem.Instance.CollectorCollider))
            {
                //reuse in pool
                ObjIsFail = false;
                ObjIsShot = false;
                isGrounded = true;
                transform.DOKill();
                if (Prop1 != null)
                {
                    Prop1.transform.DOPlayBackwards();
                }

                if (isCollectible && !isDespawning)
                {
                    isDespawning = true;
                    if (_despawnWithDelay != null)
                    {
                        StopCoroutine(_despawnWithDelay);
                        _despawnWithDelay = StartCoroutine(DespawnsWithDelay());
                        return;
                    }
                    _despawnWithDelay = StartCoroutine(DespawnsWithDelay());
                }


                //gameObject.SetActive(false);
            }
            
/*            if (!isDespawning && isCollectible && (other == CatcherOverride || other == ShootingSystem.Instance.CollectorCollider))
            {
                ObjIsFail = false;
                ObjIsShot = false;
                isGrounded = true;
                isActive = false;
                isDespawning = true;
                transform.DOKill(true);
                if (Prop1 != null)
                {
                    Prop1.transform.DOPlayBackwards();
                }
                if (_despawnWithDelay != null)
                {
                    StopCoroutine(_despawnWithDelay);
                    _despawnWithDelay = StartCoroutine(DespawnsWithDelay());
                    return;
                }
                _despawnWithDelay = StartCoroutine(DespawnsWithDelay());
            }*/
        }


        private Coroutine _despawnWithDelay;
        private IEnumerator DespawnsWithDelay()
        {
            //global::Logger.Log("meoaw");

            FallingObjSprite.DOFade(0.5f, 0.5f).SetLoops(-1, LoopType.Yoyo);
            yield return new WaitForSeconds(5f);
            FallingObjSprite.DOKill();
            FallingObjSprite.DOFade(0.5f, .25f).SetLoops(-1, LoopType.Yoyo);
            //FallingObjSprite.transform.DOShakePosition(0.25f, new Vector3(0.2f,0.2f,0), 5, 10).SetLoops(-1, LoopType.Yoyo);
            yield return new WaitForSeconds(5f);
            //FallingObjSprite.transform.DOKill(true);
            yield return FallingObjSprite.DOKill(true);
            gameObject.SetActive(false);
            isDespawning = false;
            FallingObjSprite.color = Color.white;
            //global::Logger.Log("meoaw pouf!");
        }
        /// <summary>
        /// used to finish the sequence and disable the object until re-use
        /// </summary>
        public void DisableFallingObj()
        {
            //global::Logger.Log("disable falling " + gameObject.name);
            GetComponent<Collider2D>().enabled = false;
            transform.DOKill();
            transform.localPosition = Vector3.zero;
            // return to pool
            ObjIsFail = false;
            ObjIsShot = false;
            isGrounded = false;
            isActive = false;
            isDespawning = false;
            FallingObjSprite.DOKill(true);
            FallingObjSprite.color = Color.white;
            GetComponent<Collider2D>().enabled = true;
            gameObject.SetActive(false);
        }

    
        public void StartFalling()
        {
            isActive = true;
            transform.DOMove(new Vector3(0,-100f,0),  FallingSpeed / Time.deltaTime).SetEase(Ease.OutQuad).SetRelative().SetAutoKill();
        }

        void Update()
        {
            if(!isActive) return;

            //if isgrounded start tracking player
            if (isGrounded && !isCollectible)
            {
                transform.position = Vector3.MoveTowards(transform.position, LevelBuilder.Player.transform.position, FallingSpeed * Time.deltaTime );
                //_rigidBody.AddForce((LevelBuilder.Player.transform.position - transform.position) * FallingSpeed * Time.deltaTime, ForceMode2D.Force);
                
                return;
            }

            if (isCought)
            {
                transform.DOKill();
                transform.position = Vector3.MoveTowards(transform.position, LevelBuilder.Player.transform.position, Time.deltaTime *10f);
            }
           
            //transform.Translate(Target.x * FallingSpeed * Time.deltaTime, Target.y * -FallingSpeed * Time.deltaTime, 0);
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
