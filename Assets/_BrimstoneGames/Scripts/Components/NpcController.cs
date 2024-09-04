using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _DPS
{
    public class NpcController : MonoBehaviour
    {
        public SpriteRenderer Char;


        //npc params
        public NpcEntity NpcTemplate;
        public int NpcId = -1;
        /// <summary>
        /// cached on runtime from poolList
        /// </summary>
        public int PoolId = -1;
        //public Sprite CharSprite;
        public int Health = 1;
        public float BossScaleMod = 3;
        public int Damage = 1;
        public float FireRate = 1;
        public int SpawnAmount = 1;
        public GameManager.NpcTypes NpcType;
        //set from call params
        public bool IsBoss;// { get; set; }
        public Transform Emmiter { get; set; }
        public float FallingSpeed { get; set; }
        public Collider2D Catcher { get; set; }
        [SerializeField]
        private ScorePickComponent _scorePickComponent;
        //public bool HasSight;
        //public float PatrollingLength;
        //public float WalkingSpeed = 3;
        //public float JumpSpeed = 3;
        //public float WalkDelay = 2;
        //private Rigidbody2D _rigidbody2D;
        //private bool canWalk;
        //private bool _isPatrolling, forward;
        
        //private Coroutine walking;
        //private Vector3 pointOfOrigin;
        private bool _hasAgro, isEnraged, isActive;
        private int _maxHealth;
        private int _npcIdToSpawn;
        private bool isMoving;

        void Awake()
        {
            if (_scorePickComponent == null)
            {
                _scorePickComponent = GetComponent<ScorePickComponent>();
            }
        }

        void OnEnable()
        {
            GetStats();
        }

        public void Init()
        {
            //StartCoroutine(StartBehaviourWithDelay(.1f));
        }

        private void GetStats()
        {
            NpcId = NpcTemplate.NpcId;
            //CharSprite = NpcTemplate.CharSprite;
            Health = IsBoss ? NpcTemplate.BossVersionHealth : NpcTemplate.Health;
            Damage = NpcTemplate.Damage;
            NpcType = NpcTemplate.NpcType;
            BossScaleMod = NpcTemplate.BossScaleMod;
            FireRate = NpcTemplate.FireRate;
            SpawnAmount = NpcTemplate.SpawnAmount;
            //HasSight = NpcTemplate.HasSight;
            //PatrollingLength = NpcTemplate.PatrollingLength;
            //WalkingSpeed = NpcTemplate.WalkingSpeed;
            //JumpSpeed = NpcTemplate.JumpSpeed;
            //WalkDelay = NpcTemplate.WalkDelay;
            //fill refs
            //_rigidbody2D = GetComponent<Rigidbody2D>();
            ApplyStats();
        }

        private void ApplyStats()
        {
            //populate HUD
            if (IsBoss)
            {
                transform.localScale *= BossScaleMod;
                _maxHealth = Health;
                //show health
                HudManager.Instance.ToggleBossHp(true);
                HudManager.Instance.SetBossHp(Health, 1f);
            }
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            //if it hits player
            if (other.CompareTag("Player") && !other.isTrigger)
            {

                if (GameManager.Instance.kill == null)
                {
                    GameManager.Instance.kill= StartCoroutine(GameManager.Instance.KillAndReload());
                }

                //if (GameManager.Instance.PlayerParams.NumberOfLives > 1)
                //{
                //    GameManager.Instance.PlayerParams.NumberOfLives--;
                //}
                //else
                //{
                //    StartCoroutine(GameManager.Instance.KillAndReload());
                //}
            }
            //if it got hit
            if (other.GetComponent<ProjectileComponent>())
            {
                //global::Logger.Log("~~~~~~~~~umf");
                Health = Health - other.GetComponent<ProjectileComponent>().ProjectileDamage;

                if (_scorePickComponent != null)
                {
                    GameManager.ScorePick?.Invoke(_scorePickComponent.ScoreValue);
                }

                //if boss start mad sequence
                if (!IsBoss)
                {
                    return;
                }
                //hit effect
                Char.DOBlendableColor(Color.yellow, 0.25f).SetEase(Ease.InOutQuad).OnComplete((() => Char.DORewind()));
                if (!_hasAgro)
                {
                    _hasAgro = true;
                    isActive = true;
                    StartCoroutine(ThrowingAttackSequence());
                    BossStartMovementOnX();
                }
                HudManager.Instance.SetBossHp(Health, Mathf.Clamp(Health / (float)_maxHealth, 0f, 1f));
                // enraging
                if (!isEnraged)
                {
                    
                    if (Health < Mathf.FloorToInt(_maxHealth / 2f))
                    {
                        isEnraged = true;
                        AudioManager.Instance.Play("bossEnrage");
                        FireRate /= 2f;
                        Char.DOBlendableColor(Color.red, 0.75f).SetLoops(-1, LoopType.Yoyo).SetEase(Ease.InOutQuad);
                    }
                }

                if (Health > 0 && Health % 10 == 0)
                {
                    DropCollectible();
                    DropGun();
                }

                //if (Health > 0 && Health % 20 == 0)
                //{
                //    DropGun();
                //}

                if (isActive && Health <= 0)
                {
                    isActive = false;
                    isMoving = false;
                    if (_bossMoving != null)
                    {
                        StopCoroutine(_bossMoving);
                    }
                    transform.DOKill();
                    AudioManager.Instance.Play("bossDeath");
                    HudManager.Instance.ToggleBossHp(false);
                    //end encounter
                    Char.DOKill();
                    transform.DOScale(transform.localScale / 1.5f, 2f).SetEase(Ease.OutQuad).OnComplete(() =>
                        {
                            StartCoroutine(Despawn());
                        });
                    transform.DOMove(new Vector3(0, -2f, 0), 2f).SetRelative();
                }
            }
        }

        private void StopShooting()
        {
            BallController.Instance.StopShooting();
        }

        private void DropCollectible()
        {
            DrawFalling(ShootingSystem.FallingObjTypes.Animals);
        }

        private void DropGun()
        {
            ShootingSystem.SpawnRandomGunPickOfRarityOnPosition.Invoke(2, GetRandomSpawnPositionOnEmmiterX(), Catcher);
        }

        private IEnumerator Despawn()
        {
            //spawn portal
            ShootingSystem.Instance.SpawnExitEncounter();
            yield return new WaitForSeconds(1f);
            StopShooting();
            //fly away and kill self
            transform.DOScale(new Vector3(.1f, .1f, 0), 3f).SetAutoKill();
            transform.DOMove(new Vector3(1, 1, 0), 1f).SetRelative().SetEase(Ease.Linear).SetLoops(5, LoopType.Incremental).SetAutoKill().OnComplete((() => Destroy(gameObject)));
            
        }

        private IEnumerator ThrowingAttackSequence()
        {
            //todo handle initial effects
            yield return new WaitForSeconds(1f);
            while (Health > 0)
            {
                if (Time.timeScale < .1)
                {
                    yield return StartCoroutine(GameManager.HandlePauseState());
                }
                if (isEnraged)
                {
                    transform.DOShakeScale(FireRate - 0.02f, new Vector3(0.1f, 0.1f, 0.1f), 1, 10f).SetId(1);
                }
                for (int i = 0; i < SpawnAmount; i++)
                {
                    DrawFalling(ShootingSystem.FallingObjTypes.Enemies);
                }
                var t = FireRate;
                while (t>0)
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

        private void BossStartMovementOnX()
        {
            isMoving = true;
            _bossMoving = StartCoroutine(BossMoving());
        }

        private Coroutine _bossMoving;
        private IEnumerator BossMoving()
        {
            while (isMoving)
            {
                if (Time.timeScale < .1)
                {
                    yield return StartCoroutine(GameManager.HandlePauseState());
                }
                var rngSpeed = UnityEngine.Random.Range(2f, 5f);
                yield return transform.DOMove(GetRandomSpawnPositionOnEmmiterX(), rngSpeed).SetEase(Ease.Linear).onComplete;
                var t = 1f;
                while (t > 0)
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

        private void DrawFalling(ShootingSystem.FallingObjTypes fallType)
        {
            var poolid = fallType == ShootingSystem.FallingObjTypes.Enemies ? PoolId : 
                Random.Range(0, Array.IndexOf(ShootingSystem.Instance._fallingObjList[(int)fallType].FallingObjList,
                    Array.Find(ShootingSystem.Instance._fallingObjList[(int)fallType].FallingObjList,
                        f => f.FallingobjType == fallType)));
            var rngpos = GetRandomSpawnPositionOnEmmiterX();
            ShootingSystem.Instance.StartFalling(fallType, poolid, Vector3.down, rngpos, FallingSpeed, Catcher);
        }

        private Vector3 GetRandomSpawnPositionOnEmmiterX()
        {
            return new Vector3(
                Random.Range(Emmiter.GetComponent<Renderer>().bounds.min.x + 1f,
                    Emmiter.GetComponent<Renderer>().bounds.max.x - 1f), Emmiter.position.y, 0);
        }

        //old behaviour
/*
        private IEnumerator StartBehaviourWithDelay(float delay)
        {
            yield return new WaitForSeconds(delay);

            EnableBehaviour();

        }

        private IEnumerator WalkDelaying()
        {
            canWalk = false;
            yield return new WaitForSeconds(WalkDelay);
            canWalk = true;
        }

        public void EnableBehaviour()
        {
            if (PatrollingLength <= 0)
            {
                _isPatrolling = false;
                return;
            }
            pointOfOrigin = transform.position;
            _isPatrolling = true;
            canWalk = true;
            forward = true;

        }

        // Start is called before the first frame update
        void Start()
        {
            Init();
        }

        // Update is called once per frame
        void Update()
        {
            //return to PoO
            global::Logger.Log("" + (pointOfOrigin.x - transform.position.x));
            if ((pointOfOrigin.x - transform.position.x) > (PatrollingLength + transform.position.x))
            {
                //turn
                forward = false;
            }
            else
            {
                forward = true;
            }
            if (_isPatrolling && canWalk)
            {
                global::Logger.Log("~~~ " + forward);
                _rigidbody2D.AddForce(new Vector2(forward ? -WalkingSpeed : WalkingSpeed, JumpSpeed), ForceMode2D.Impulse);
                walking = StartCoroutine(WalkDelaying());
            }
        }*/
    }
}
