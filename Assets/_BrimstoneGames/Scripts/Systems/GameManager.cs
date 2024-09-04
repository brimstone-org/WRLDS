using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace _DPS
{
    public class GameManager : Singleton<GameManager>
    {
        protected GameManager(){}

        //todo this release?
        public int LoadFromSceneId;




        [Header("Player resources")]
        public GameObject PlayerPrefab;
        public PlayerParams PlayerParams;
        public float SoftBounce = 1;
        public float MediumBounce = 1.3f;
        public float HardBounce = 1.75f;

        public float BounceStrMultiplierH = .05f;
        public float BounceStrMultiplierV = .05f;
        public float PlayerJumpHeight = 1.5f;
        public float PlayerJumpLength = 0.1f;
        public float DoubleTapTimer = 0.5f;
        public float PlayerGravityScale = 0f;
        public PhysicsMaterial2D PlayerMaterial;
        public float PlayerSpeed = 3f;
        public int MaxHealth, MaxArmor;
        public List<PlayerStats> PlayerSceneParams;
    
        public static int PlayerScore;
        public bool FirstLevel;
        public bool LastSavedLevel;
        public static int CurrentScene = -1;
        public Coroutine kill, playerHud;

        public static Action<EffectOnPlayer> ApplyEffect;
        public static Action<EffectOnPlayer> RemoveEffect;
        public static Action StopBall;
        public static Action<float> StopBallVertical;
        public static Action ResetPlatformsToStart;
        public static Action TriggerPlatformMovement;
        public static Action<Transform> StopBallCatch;
        public static Action<int> ScorePick;
        private bool isPaused, isInvincible;
        private float invincibilityTime = 3f;
        private Coroutine _stomeBallPlz;
        [Flags]
        public enum NpcTypes
        {
            NONE = 0,

            Roller = 1 << 0,
            Jumper = 1 << 2,
            Shooter = 1 << 3,
            Thrower = 1 << 4
        }

        public class EnemyParams
        {
            public int EnemyId = -1;

        }
        #region Inits
        void Awake()
        {
            //Application.targetFrameRate = -1;
            //QualitySettings.vSyncCount = -1;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
        
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                Destroy(gameObject);
            }
            DontDestroyOnLoad(Instance);
            //SceneManager.sceneLoaded += OnSceneLoaded;
            //if (!string.IsNullOrEmpty(SceneToLoadFirst))
            //{
            //    SceneManager.LoadScene(SceneToLoadFirst, LoadSceneMode.Single);
            //}
            //PlayerPrefs.DeleteAll();
            //GetLastSavedScene();

            //subscribe to events
            ScorePick += OnScorePick;
            StopBallVertical += OnStopBallVertical;
            GameManager.ApplyEffect += OnApplyEffect;
            GameManager.RemoveEffect += OnRemoveEffect;
            GameManager.StopBall += OnStopBall;
            GameManager.StopBallCatch += OnStopBallCatch;
        }

        
        private void OnApplyEffect(EffectOnPlayer playerEffect)
        {
            global::Logger.Log("~~~~~~~~~~~~~~ " + playerEffect.EffectType);
            if (!BallController.playerEffects.ContainsKey(playerEffect.EffectId) ||
                BallController.BallEffect.NON_EFFECT.Contains(playerEffect.EffectType))
            {
                switch (playerEffect.EffectType)
                {
                    case BallController.BallEffect.AreaForce:
                        global::Logger.Log("Area Force");
                        if (LevelBuilder.Player.GetComponentInChildren<MagnetCollectibles>() == null)
                        {
                            if (playerEffect.VisualPickPrefab != null)
                            {
                                Instantiate(playerEffect.VisualPickPrefab, LevelBuilder.Player.transform);
                            }

                            switch (playerEffect.AffectType)
                            {
                                case BallController.BallEffectAffects.Collectibles:
                                    var magnet = LevelBuilder.Player.gameObject.AddComponent<MagnetCollectibles>();
                                    magnet.Radius = playerEffect.EffectRadius;
                                    var spriteCircle = LevelBuilder.Player.GetComponentInChildren<SpriteEffectTag>().transform;
                                    spriteCircle.localScale *= magnet.Radius;
                                    var magnetCollider = magnet.gameObject.AddComponent<CircleCollider2D>();
                                    magnetCollider.isTrigger = true;
                                    magnetCollider.radius = magnet.Radius;

                                    magnet.Speed = playerEffect.EffectMagnitude;
                                    break;

                            }
                        }
                        //add to hud
                        BallController.Instance.AddEffectInDictionaries(playerEffect);
                        break;
                    case BallController.BallEffect.Force:
                        global::Logger.Log("Force");
                        //add
                        BallController.VerticalPower += playerEffect.EffectJumpAdd;
                        BallController.LateralPower += playerEffect.EffectPowerAdd;
                        //add to hud
                        BallController.Instance.AddEffectInDictionaries(playerEffect);
                        HudManager.Instance.SetPlusImage(true);
                        break;
                    case BallController.BallEffect.DoubleJump:
                        //add
                        global::Logger.Log("DBJump");
                        BallController.Instance.DoubleJump = BallController.Instance.DblJumpOn = true;
                        //add to hud
                        BallController.Instance.AddEffectInDictionaries(playerEffect);
                        HudManager.Instance.SetArrowImage(false);
                        HudManager.Instance.SetArrowDirection();
                        break;
                    case BallController.BallEffect.Armor:
                        if (GameManager.Instance.PlayerParams.NumberOfArmor < GameManager.Instance.MaxArmor)
                        {
                           // global::Logger.Log("Armor");
                            GameManager.Instance.PlayerParams.NumberOfArmor++;
                            GameManager.Instance.HandleArmor(true);
                        }
                        break;
                    case BallController.BallEffect.Health:
                        if (GameManager.Instance.PlayerParams.NumberOfLives < GameManager.Instance.MaxHealth)
                        {
                            global::Logger.Log("Life");
                            GameManager.Instance.PlayerParams.NumberOfLives++;
                            GameManager.Instance.HandleLife();
                        }
                        break;
                }
            }
            else
            {
                global::Logger.Log("just reapplied timer");
            
                BallController.playerEffects[playerEffect.EffectId] = playerEffect;
                //just reapply timer
                BallController.playerEffects[playerEffect.EffectId].EffectDuration = EffectsCatalogue.Instance.Effects[playerEffect.EffectId].EffectDuration;
                GameManager.Instance.PlayerParams.EffectsOnPlayer[playerEffect.EffectId] = BallController.playerEffects[playerEffect.EffectId].EffectDuration;
            }

        }

        
        public void OnRemoveEffect(EffectOnPlayer playerEffect)
        {
            OnRemoveEffect(playerEffect.EffectId);
        }

        public void OnRemoveEffect(int playerEffectId)
        {

            var playerEffect = BallController.playerEffects[playerEffectId];

            switch (playerEffect.EffectType)
            {
                case BallController.BallEffect.Force:
                    HudManager.Instance.SetPlusImage();
                    break;
            }

            //set
            BallController.VerticalPower -= playerEffect.EffectJumpAdd;
            BallController.LateralPower -= playerEffect.EffectPowerAdd;
            BallController.playerEffects.Remove(playerEffectId);
            GameManager.Instance.PlayerParams.EffectsOnPlayer.Remove(playerEffectId);
            GameManager.Instance.HandleBuffsHud(playerEffectId, false);
        }

        public void OnStopBall()
        {
            BallController._rigidbody2D.velocity = Vector2.zero;
            BallController._rigidbody2D.angularVelocity = 0;
        }

        
        public void OnStopBallCatch(Transform catcher)
        {
            BallController._rigidbody2D.velocity = Vector2.zero;
            BallController._rigidbody2D.angularVelocity = 0;
            //transform.DOMove(new Vector3(catcher.transform.position.x, transform.position.y,transform.position.z),0).SetEase(Ease.Linear);
        }

        public void OnStopBallVertical(float minValue)
        {
            if (_stomeBallPlz != null)
            {
                StopCoroutine(_stomeBallPlz);
                
            }
            global::Logger.Log("!!!@@$%%^%^% The coroutine to stap the ball is being called");
            _stomeBallPlz = null;
            _stomeBallPlz=StartCoroutine(StopMeBallPlz(minValue));
            //_rigidbody2D.angularVelocity = 0;
        }


        public void StopCoroutineThatStopsBall()
        {
            if (_stomeBallPlz != null)
            {
                StopCoroutine(_stomeBallPlz);
            }
        }

        private IEnumerator StopMeBallPlz(float minVal)
        {
            float multiplicationFactor = Mathf.Clamp(1 / BallController._rigidbody2D.velocity.magnitude, 0.8f, 0.9f);
            global::Logger.Log(" The multiplier factor is " +multiplicationFactor);
            while (Mathf.Abs(BallController._rigidbody2D.velocity.x) > minVal)
            {
                BallController._rigidbody2D.velocity = new Vector2(Mathf.Clamp(BallController._rigidbody2D.velocity.x  * multiplicationFactor, minVal, 1000000f) , BallController._rigidbody2D.velocity.y);
                BallController._rigidbody2D.angularVelocity = 0;
                yield return null;
            }
            
            // _rigidbody2D.velocity = new Vector2(0, _rigidbody2D.velocity.y);
        }
        public void GetLastSavedScene()
        {
            StartCoroutine(GetLastScene());
        }

        private IEnumerator GetLastScene()
        {
        
            yield return  CurrentScene = LoadFromSceneId > 0 ? LoadFromSceneId : GetPlayerParams().LastScene;
            var current = SceneManager.GetActiveScene().buildIndex;
            if (current == 0)
            {
                SceneLoader.Instance.LoadScene(CurrentScene);
            }
            global::Logger.Log("~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ " + CurrentScene);
        }

        public PlayerParams GetPlayerParams()
        {

            global::Logger.Log("path " + Application.persistentDataPath + "/PlayerParams.es3");
            if (ES3.FileExists(Application.persistentDataPath + "/PlayerParams.es3"))
            {
                //todo remove this in release
                var ret = ES3.Load<PlayerParams>("PlayerParams",Application.persistentDataPath + "/PlayerParams.es3");
                ret.LastScene = 19;
                return ret;
            }
        
            var p = new PlayerParams();
            //todo same
            p.LastScene = 19;
            ES3.Save<PlayerParams>("PlayerParams", p, Application.persistentDataPath + "/PlayerParams.es3");
            return p;

        }

        public void SavePlayerParams(PlayerParams p)
        {
            ES3.Save<PlayerParams>("PlayerParams", p, Application.persistentDataPath + "/PlayerParams.es3");
        }

        public void Init()
        {
            PlayerParams = GetPlayerParams();
            kill = null;
            LevelBuilder.Instance.Init();
        }


        public void SetPlayerStats()
        {
            //player stats
            foreach (var playerStats in PlayerSceneParams)
            {
                if(playerStats.SceneName != GetSceneName()) continue;
                PlayerJumpHeight = playerStats.PlayerJumpHeight;
                PlayerJumpLength = playerStats.PlayerJumpLength;
                DoubleTapTimer = playerStats.DoubleTapTimer;
                PlayerGravityScale = playerStats.PlayerGravityScale;
                PlayerMaterial = playerStats.PlayerMaterial;
                PlayerSpeed = playerStats.PlayerSpeed;
                MaxHealth = playerStats.MaxHealth;
                MaxArmor = playerStats.MaxArmor;
            }

            //clean
            if (playerHud != null)
            {
                StopCoroutine(playerHud);
            }
            //start updating HUD
            playerHud = StartCoroutine(InitPlayerHud());
            PlayerParams.LastScene = SceneManager.GetActiveScene().buildIndex;
            SavePlayerParams(PlayerParams);
        }

        private IEnumerator InitPlayerHud()
        {
            //wait one
            yield return null;
            //init lives
            SetLives();
            SetArmor();
            ClearBuffs();
            SetScore();
        }

        private void SetScore()
        {
            HudManager.Instance.ScoreText.text = PlayerParams.PlayerScore.ToString("N0");
        }

        private void SetLives()
        {
            //clear
            foreach (Transform child in HudManager.Instance.LifeHolder.transform)
            {
                Destroy(child.gameObject);
            }
            //populate
            for (int i = 0; i < PlayerParams.NumberOfLives; i++)
            {
                Instantiate(HudManager.Instance.LifePrefab, HudManager.Instance.LifeHolder.transform);
            }
        }

        public void HandleLife(bool add = true)
        {
            if (add)
            {
                Instantiate(HudManager.Instance.LifePrefab, HudManager.Instance.LifeHolder.transform);
            }
            else
            {
                //BallController.Instance.StopBall();
                PlayLostLife((() =>
                {
                    if (PlayerParams.NumberOfLives == 1)
                    {
                        //StartCoroutine(KillAndReload());
                    }
                }));

            }
        }

        private void PlayLostLife(Action completed)
        {
            var respawn = LevelBuilder.Instance.SceneType.Contains(LevelBuilder.SceneTypes.FlappyBird);
            if (!respawn)
            {
                Destroy(HudManager.Instance.LifeHolder.transform.GetChild(0).gameObject);
                BallController._rigidbody2D.AddForce(new Vector2(BallController.Instance.DirectionLeft ? -BallController.LateralPower*2 : BallController.LateralPower*2 ,0), ForceMode2D.Impulse);
            }
            BallController.Instance.CharacterFace.DOFade(0.5f, 0.5f).SetLoops(5, LoopType.Yoyo).OnComplete((() =>
            {
                BallController.Instance.CharacterFace.color = Color.white;
                completed?.Invoke();
            }));
        }

        private void SetArmor()
        {
            //clear
            foreach (Transform child in HudManager.Instance.ArmorHolder.transform)
            {
                Destroy(child.gameObject);
            }
            //populate
            for (int i = 0; i < PlayerParams.NumberOfArmor; i++)
            {
                Instantiate(HudManager.Instance.ArmorPrefab, HudManager.Instance.ArmorHolder.transform);
            }
        }

        public void HandleArmor(bool add)
        {
            if (add)
            {
                Instantiate(HudManager.Instance.ArmorPrefab, HudManager.Instance.ArmorHolder.transform);
            }
            else
            {
                Destroy(HudManager.Instance.ArmorHolder.transform.GetChild(0).gameObject);
            }
        }

        private void ClearBuffs()
        {
            //clear
            foreach (Transform child in HudManager.Instance.BuffHolder.transform)
            {
                Destroy(child.gameObject);
            }
        }
      

        #endregion

        public void HandleBuffsHud(int effectId, bool add = true)
        {
            // add to hud
            var effect = EffectsCatalogue.Instance.Effects[effectId];

            if(add)
            {
                global::Logger.Log("w " + effect.HudImagePrefab.name);
                var hudGo = Instantiate(HudManager.Instance.BuffPrefab, HudManager.Instance.BuffHolder.transform);
                var hudItem = hudGo.GetComponent<BuffParams>();
                hudItem.EffectOnPlayerId = effectId;
                hudItem.Background.color = Color.yellow;
                hudItem.BuffIcon.sprite = effect.HudImagePrefab;
                HudManager.Instance.Buffs.Add(effectId, hudGo);
            }
            else
            {
                //remove from hud
                if (HudManager.Instance.Buffs.ContainsKey(effectId))
                {
                    var hudGo = HudManager.Instance.Buffs[effectId];
                    if (hudGo != null)
                    {
                        Destroy(hudGo);
                    }

                    HudManager.Instance.Buffs.Remove(effectId);
                }
            }
        }

        private Coroutine _invincibilityRun;
        private IEnumerator Invincibility()
        {
            isInvincible = true;
            yield return new WaitForSeconds(invincibilityTime);
            isInvincible = false;
        }

        public void Reload()
        {
            var scene = SceneManager.GetActiveScene().buildIndex;
            BallController.Started = false;
            HudManager.Instance.CleanHud();
            //SavePlayerParams(PlayerParams);
            PlayerParams.EffectsOnPlayer = new Dictionary<int, float>();
            HudManager.Instance.Buffs = new Dictionary<int, GameObject>();
            ShootingSystem.ProjectilePools = new Dictionary<int, Dictionary<int, ProjectileComponent[]>>();
            ShootingSystem.FallingObjPools = new Dictionary<int, Dictionary<int, FallingObjectComponent[]>>();
            BallController.playerEffects = new Dictionary<int, EffectOnPlayer>();

            Instance.StartCoroutine(Instance.LoadScene(scene));
            global::Logger.Log("+++++++++++++++++++++ reload" + scene);
        }

        public IEnumerator KillAndReload()
        {

            var respawn = LevelBuilder.Instance.SceneType.Contains(LevelBuilder.SceneTypes.FlappyBird);// | LevelBuilder.SceneTypes.UpTube | LevelBuilder.SceneTypes.DownTube);
            
            global::Logger.Log("Respawn=== " + respawn);
            if (!respawn)
            {
                if(CheckInvincibility()) yield break;
            }
            
            //if we got armor, early out
            if (PlayerParams.NumberOfArmor > 0)
            {
                AudioManager.Instance.Play("takeArmor");
                PlayerParams.NumberOfArmor--;
                HandleArmor(false);
                SavePlayerParams(PlayerParams);
                if (!respawn)
                {
                    yield break;
                }
                Reload();
                yield break;
            }

            if (PlayerParams.NumberOfLives > 1 && PlayerParams.NumberOfArmor == 0)
            {
                AudioManager.Instance.Play("takeDamage");
                PlayerParams.NumberOfLives--;
                HandleLife(false);
                SavePlayerParams(PlayerParams);
                if(!respawn) {yield break;}
                Reload();
                yield break;
            }

            AudioManager.Instance.Play("death");
            var scene = SceneManager.GetActiveScene().buildIndex;
            BallController.Started = false;


            if (PlayerParams.NumberOfLives <= 1 && PlayerParams.NumberOfArmor == 0)
            {
                //restart game completely
                PlayerParams.NumberOfLives--;
                HandleLife(false);
                PlayerParams = new PlayerParams();
                Time.timeScale = 0;
                //SavePlayerParams(PlayerParams);
                HudManager.Instance.FireDeathWrap();
                yield return new WaitForSecondsRealtime(1f);
                Instance.StartCoroutine(Instance.LoadScene(scene));
                //scene = 1;
            }


            //GameManager.Instance.PlayerParams.EffectsOnPlayer.Clear();
            //HudManager.Instance.Buffs.Clear();
            //ShootingSystem.ProjectilePools.Clear();
            //ShootingSystem.FallingObjPools.Clear();
            //BallController.playerEffects.Clear();
        
            
            global::Logger.Log("+++++++++++++++++++++ " + scene);
        }

        private bool CheckInvincibility()
        {
            if (isInvincible) return true;
            // add invincibility
            if (!isInvincible && PlayerParams.NumberOfLives > 1)
            {
                if (_invincibilityRun != null)
                {
                    StopCoroutine(_invincibilityRun);
                }

                _invincibilityRun = StartCoroutine(Invincibility());
            }

            return false;
        }

        public void Cleanup()
        {
            HudManager.Instance.ToggleBossHp(false);

            BallController.Started = false;
            HudManager.Instance.CleanHud();
            PlayerParams.EffectsOnPlayer = new Dictionary<int, float>();
            HudManager.Instance.Buffs = new Dictionary<int, GameObject>();
            ShootingSystem.ProjectilePools = new Dictionary<int, Dictionary<int, ProjectileComponent[]>>();
            ShootingSystem.FallingObjPools = new Dictionary<int, Dictionary<int, FallingObjectComponent[]>>();
            BallController.playerEffects = new Dictionary<int, EffectOnPlayer>();
         
        }

        public IEnumerator LoadScene(int sceneId)
        {
            ControlManager.Instance.DisableBall();
            SavePlayerParams(PlayerParams);
            if (LevelBuilder.Player != null)
            {
                BallController.Instance.UnsubscribeEvents();
            }
            //set next scene id
            CurrentScene = sceneId;

            SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene());
            Cleanup();
            SceneLoader.Instance.LoadScene(sceneId);
            yield return null;
        }

        [Serializable]
        public class PlayerStats
        {
            public string SceneName;
            public float BounceStrMultiplierH = .05f;
            public float BounceStrMultiplierV = .05f;
            public float PlayerJumpHeight = 1.5f;
            public float PlayerJumpLength = 0.1f;
            public float DoubleTapTimer = 0.5f;
            public float PlayerGravityScale = 0f;
            public PhysicsMaterial2D PlayerMaterial;
            public float PlayerSpeed = 3f;
            public int MaxHealth = 6;
            public int MaxArmor = 6;
        }
    
        public string GetSceneName()
        {
            var scene = SceneManager.GetActiveScene();
            return scene.name;
        }

        private void OnScorePick(int scoreIncrease)
        {
            //global::Logger.Log("~~~~~~~~~~ Score " + GameManager.Instance.PlayerParams.PlayerScore);
            GameManager.Instance.PlayerParams.PlayerScore += scoreIncrease;
            HudManager.Instance.ScoreText.text = GameManager.Instance.PlayerParams.PlayerScore.ToString("N0");
        }
        /// <summary>
        /// helper to deal with paused time since Time.Deltatime does not work when Time.timescale == 0
        /// </summary>
        /// <returns></returns>
        public static IEnumerator HandlePauseState()
        {

            while (Time.timeScale <= 0)
            {
                yield return null;
            }
        }

        public void Quit()
        {
            global::Logger.Log("Shuttinh Down");
            Application.Quit();
        }

        void OnApplicationQuit()
        {
            SavePlayerParams(PlayerParams);
        }

        void OnApplicationFocus(bool hasFocus)
        {
            isPaused = !hasFocus;
            if (isPaused)
            {
                //SavePlayerParams(PlayerParams);
            }
        }

        void OnApplicationPause(bool pauseStatus)
        {
            isPaused = pauseStatus;
        }
    }
}
