using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace _DPS
{
    public class BallController : MonoBehaviour
    {
        public static BallController Instance;

        //refs
        public SpriteRenderer CharacterFace, CharacterGlow, CharacterOuter, CharacterProp1, CharacterProp2, CharacterProp3;
        //params
        public static Rigidbody2D _rigidbody2D;
        //private static GameObject player;
        private int _layerInt, _noJumpLayerInt, _superSpeedInt;
        public static float LateralPower;
        public static float VerticalPower;
        private static float ballDoubleTapTimer = .5f;
        [HideInInspector]
        public bool CanMove, DoubleJump, DblJumpOn, CanChangeDirection, DirectionLeft;

        private static bool doubleTapTimerOn, doubleTapped;
        public static Dictionary<int,EffectOnPlayer> playerEffects = new Dictionary<int, EffectOnPlayer>();
        public static bool Started, IsOnPlatform;
        /// <summary>
        /// global player control switch
        /// </summary>
        public static bool AllowMovement;
        public static bool IsInsideTube;
        public static bool Swinger;

        private bool _isTouchingBase;
        private bool _isTouchingLand;
        private bool _isTouchingNoJump;
        private bool _isTouchingSuperSpeed;
        
        private bool _isAlive;
        private bool _firstInput;
        

        public static float VelocityMulti;
        public static GunComponent CurrentGun;
        public static OcclussionComponent CurrentParentOcclussionComponent = null;
        private bool bounced;
        public ToggleProgressColor[] childColor;

        [Flags]
        public enum BallEffect
        {
            NONE = 0,
            AreaForce = 1 << 0,
            Force = 1 << 1,
            Armor = 1 << 2,
            Health = 1 << 3,
            DoubleJump = 1 << 4,

            NON_EFFECT = Health | Armor
        
        }
        public enum BallEffectAffects
        {
            Player,
            Collectibles,
            Enemies,
        
        }

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(Instance);
            }
            Instance = this;
            //player = gameObject;
            //CanChangeDirection = true;
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _firstInput = false;
        }


        // Start is called before the first frame update
        void Start()
        {
            //params
            ResetPlayer();

            SetLayerMask();
            _noJumpLayerInt = LayerMask.NameToLayer("NoJump");
            _superSpeedInt = LayerMask.NameToLayer("SuperSpeed");

            HudManager.Instance.SetArrowImage();
            
        }

        public void UnsubscribeEvents()
        {

        }

        private void SetLayerMask()
        {
            HudManager.Instance.SetPlusImage();
            switch (LevelBuilder.Instance.SceneType)
            {
                case LevelBuilder.SceneTypes.Climber:
                    _layerInt = LayerMask.NameToLayer("Platform");
                   // HudManager.Instance.SetArrowDirection();
                    HudManager.Instance.SetArrowImageAndDirection();
                    break;
                case LevelBuilder.SceneTypes.FlappyBird:
                    _layerInt = LayerMask.NameToLayer("WaterWorld");
                   // HudManager.Instance.SetArrowDirection(HudManager.ArrowPoint.Right);
                    HudManager.Instance.SetArrowImageAndDirection(true, HudManager.ArrowPoint.Right);
                    break;
                case LevelBuilder.SceneTypes.UpTube:
                    _layerInt = LayerMask.NameToLayer("WaterWorld");
                   // HudManager.Instance.SetArrowDirection(HudManager.ArrowPoint.Up);
                    HudManager.Instance.SetArrowImageAndDirection(true, HudManager.ArrowPoint.Up);
                    break;
                case LevelBuilder.SceneTypes.Ascend:
                    _layerInt = LayerMask.NameToLayer("WaterWorld");
                    //HudManager.Instance.SetArrowDirection(HudManager.ArrowPoint.Up);
                    HudManager.Instance.SetArrowImageAndDirection(true, HudManager.ArrowPoint.Up);
                    break;
                case LevelBuilder.SceneTypes.DownTube:
                    _layerInt = LayerMask.NameToLayer("WaterWorld");
                    //HudManager.Instance.SetArrowDirection(HudManager.ArrowPoint.Down);
                    HudManager.Instance.SetArrowImageAndDirection(true, HudManager.ArrowPoint.Down);
                    break;

            }
            global::Logger.Log("got layer "+  _layerInt);
        }

        public void StopShooting()
        {
            CurrentGun.EnableShooting = false;
            Destroy(CurrentGun.gameObject);
            //StartCoroutine(StopShootingSequence());
        }


        private IEnumerator StopShootingSequence()
        {
            CurrentGun.enabled = false;
            ShootingSystem.Instance.ResetProjectiles();
            var t = 5f;
            while (t>0)
            {
                if (Time.timeScale < .1)
                {
                    yield return StartCoroutine(GameManager.HandlePauseState());
                }
                t -= Time.deltaTime;
                yield return null;
            }
            Destroy(CurrentGun.gameObject);
        }

        public void AddEffectInDictionaries(EffectOnPlayer playerEffect)
        {
            playerEffects.Add(playerEffect.EffectId, playerEffect);
            if (!GameManager.Instance.PlayerParams.EffectsOnPlayer.ContainsKey(playerEffect.EffectId))
            {
                GameManager.Instance.PlayerParams.EffectsOnPlayer.Add(playerEffect.EffectId,
                    playerEffect.EffectDuration);
            }

            GameManager.Instance.HandleBuffsHud(playerEffect.EffectId);
            global::Logger.Log("Added Effect " + EffectsCatalogue.Instance.Effects[playerEffect.EffectId].name);
        }

        public static IEnumerator StopTheBallWhenOnPlatform(float _overTime)
        {
            global::Logger.Log(" ___++++++ Stopping with the new coroutine ");
            float starTime = Time.time;
            while (Time.time < starTime + _overTime)
            {
                _rigidbody2D.velocity = Vector2.Lerp(_rigidbody2D.velocity, Vector2.zero, (Time.time - starTime) / _overTime);
                _rigidbody2D.angularVelocity = Mathf.Lerp(_rigidbody2D.angularVelocity, 0, (Time.time - starTime) / _overTime);
                yield return null;
            }
            _rigidbody2D.velocity = Vector2.zero;
            _rigidbody2D.angularVelocity = 0;


        }


        public void ResetPlayer()
        {
            _isAlive = true;
            VerticalPower = GameManager.Instance.PlayerJumpHeight;
            LateralPower = GameManager.Instance.PlayerJumpLength;
            ballDoubleTapTimer = GameManager.Instance.DoubleTapTimer;
            DoubleJump = false;
            DirectionLeft = false;
            var ctx = CharacterFace.transform.localScale;
            ctx.x = !DirectionLeft ? ctx.x : -ctx.x;
            CharacterFace.transform.localScale = ctx;
        }

        //public void ResetPlayer(bool turnOn)
        //{
        //    AllowMovement = turnOn;
        //}


        #region Collision

        private void OnCollisionEnter2D(Collision2D collision)
        {
            
            if (collision.gameObject.layer == _layerInt)
            {
                _isTouchingBase = true;
            }  
            if (collision.gameObject.CompareTag("Land"))
            {
                _isTouchingLand = true;
            }
            if (collision.gameObject.layer == _noJumpLayerInt)
            {
                _isTouchingNoJump = true;
                HudManager.Instance.SetArrowImageAndDirection(false, HudManager.ArrowPoint.Right);
            }

            if (collision.gameObject.layer == _superSpeedInt)
            {
                _isTouchingSuperSpeed = true;
                HudManager.Instance.SetArrowImageAndDirection(false, HudManager.ArrowPoint.Right);
            }
        }
        private void OnCollisionStay2D(Collision2D collision)
        {
            if (collision.gameObject.layer == _layerInt)
            {
                _isTouchingBase = true;
                if (playerEffects.ContainsKey(5))
                {
                    // HudManager.Instance.SetArrowImage(false);
                    HudManager.Instance.SetArrowImageAndDirection(false);
                }
                else
                {
                    // HudManager.Instance.SetArrowImage();
                    HudManager.Instance.SetArrowImageAndDirection();
                }
            }
            if (collision.gameObject.layer == _noJumpLayerInt)
            {
                _isTouchingNoJump = true;
            }

            if (collision.gameObject.layer == _superSpeedInt)
            {
                _isTouchingSuperSpeed = true;
                
            }
        }

        private void OnCollisionExit2D(Collision2D collision)
        {
            if (collision.gameObject.layer == _layerInt)
            {
                _isTouchingBase = false;
            }
            if (collision.gameObject.CompareTag("Land"))
            {
                _isTouchingLand = false;
            }
            if (collision.gameObject.layer == _noJumpLayerInt)
            {
                _isTouchingNoJump = false;
            }
            
            if (collision.gameObject.layer == _superSpeedInt)
            {
                _isTouchingSuperSpeed = false;
            }
        }

        #endregion

        public void SetProgress()
        {
            // player position on x in reference to the length of the terrain starting from 0 position
            var floatShape = 0f;
            var playerPosition = 0f;
            if(LevelBuilder.Instance.SceneType.Contains(LevelBuilder.SceneTypes.ALL_VERTICAL))
            {
                floatShape = LevelBuilder.Instance.SpriteShapeController.spline.GetPosition(0).y;
                playerPosition = transform.position.y;
            }
            else
            {
                floatShape = LevelBuilder.Instance.SpriteShapeController.spline.GetPosition(0).x;
                playerPosition = transform.position.x;
            }
            var norm = (Mathf.Abs(playerPosition) / (floatShape +LevelBuilder.Instance.SpriteShapeLenght));
            var pos = Mathf.Clamp(norm, 0, 1);
            HudManager.Instance.ProgressBackground.fillAmount = pos;
            for (int i = 0; i < childColor.Length; i++)
            {
                var normalizedX = Mathf.Clamp(childColor[i].transform.localPosition.x / HudManager.Instance.ProgressBackground.rectTransform.sizeDelta.x, 0, 1);
                if (!childColor[i].IsToggled && pos > normalizedX)
                {
                    childColor[i].ToggleColor(true);
                }

                if (childColor[i].IsToggled && pos < normalizedX)
                {
                    childColor[i].ToggleColor(false);
                }
            }
        }
        /// <summary>
        /// move the player up the tube
        /// </summary>
        /// <param name="target"></param>
        /// <param name="overTime"></param>
        /// <returns></returns>
        public IEnumerator MoveUpTube(Vector3 target, float overTime)
        {
            float starTime = Time.time;
            while (Time.time < starTime + overTime)
            {
                transform.position = Vector3.Lerp(transform.position, target, (Time.time - starTime) / overTime);
                yield return null;
            }
            transform.position = target;
            _rigidbody2D.velocity = Vector2.zero;
            _rigidbody2D.angularVelocity = 0;
            Hover(); //start hovering
        }

        /// <summary>
        /// hover in place inside the tube
        /// </summary>
        private void Hover()
        {
                
                transform.DOLocalMove(transform.position - new Vector3(0, 0.5f,0), 0.5f).SetEase(Ease.Linear).OnComplete((() =>
                {   
                    Debug.Log("!!!!!went down");
                    transform.DOLocalMove(transform.position + new Vector3(0, 0.5f, 0), 0.5f).OnComplete((() =>
                    {
                        Debug.Log("!!!!!went up");
                        Hover();
                        
                    }));
                }));
            
        }

        /// <summary>
        /// Responsible for all movement
        /// </summary>
        /// <param name="type">soft, mediu,hard bounce</param>
        /// <param name="strenght">G's in float</param>
        public void SetBounceData(int type, float strenght)
        {
            // strength control since a value between 0 and 150ish is a range that can cause issues
            // going to use additive value and scale it down by a factor
            //override strength
            var typePower = 0f;
            switch (type)
            {
                    case 0:
                        typePower = GameManager.Instance.SoftBounce;
                        break;
                    case 1:
                        typePower = GameManager.Instance.MediumBounce;
                        break;
                    case 2:
                        typePower = GameManager.Instance.HardBounce;
                        break;
            }


            if (!AllowMovement) return;
            bounced = true;
            if (IsInsideTube )
            {
                transform.DOPause();
                _rigidbody2D.velocity = Vector2.zero;
                _rigidbody2D.angularVelocity = 0;
                int _horizontalForce = UnityEngine.Random.value < .5 ? 1 : -1;
                Logger.Log("HorizontalForce is " + _horizontalForce);
                _rigidbody2D.AddForce(new Vector2(_horizontalForce*15/* * GameManager.Instance.BounceStrMultiplierH*/, 5), ForceMode2D.Impulse);
            }


            
            //todo change this when ball is implemented
            if (bounced/*Input.GetButtonUp("Fire1") || Input.GetKeyUp(KeyCode.Space)*/)
            {
                bounced = false;
                if (!_firstInput)
                {
                    _rigidbody2D.gravityScale = GameManager.Instance.PlayerGravityScale;
                    _rigidbody2D.simulated = true;
                    _firstInput = true;
                }
                else
                {
                    CanMove = true;
                    if (_isTouchingNoJump || _isTouchingSuperSpeed)
                    {   
                        
                        CanChangeDirection = !CanChangeDirection;
                    }
                }
            }

            //movement calculations

            if (CanMove && _isTouchingNoJump)
            {   
                
                _rigidbody2D.drag = 0;
                if (!Swinger)
                {
                    _rigidbody2D.AddForce(new Vector2(LateralPower /* * GameManager.Instance.BounceStrMultiplierH*/, 0) * typePower, ForceMode2D.Impulse);
                }
                else
                {
                    var dir = DirectionLeft ? -LateralPower : LateralPower;
                    //_rigidbody2D.AddForce(new Vector2(dir, 0), ForceMode2D.Force);
                    _rigidbody2D.velocity= new Vector2(dir * VelocityMulti, 0);
                    var arrow = DirectionLeft ? HudManager.ArrowPoint.Left : HudManager.ArrowPoint.Right;
                   // HudManager.Instance.SetArrowDirection(arrow);
                    //HudManager.Instance.SetArrowImage(false);
                    HudManager.Instance.SetArrowImageAndDirection(false, arrow);
                    DirectionLeft = CanChangeDirection;
                }
                return;
            }


            //if can changedirection is on and we did not have a double tap yet, start timer
            if (CanChangeDirection && CanMove && !doubleTapTimerOn)
            {
                //start timer
                doubleTapTimerOn = true;
                doubleTapped = false;
                ballDoubleTapTimer = GameManager.Instance.DoubleTapTimer;
                var arrow = DirectionLeft ? HudManager.ArrowPoint.Left : HudManager.ArrowPoint.Right;
               // HudManager.Instance.SetArrowDirection(arrow);
               // HudManager.Instance.SetArrowImage(false);
                HudManager.Instance.SetArrowImageAndDirection(false, arrow);
                //apply movement in current direction
                if (!DirectionLeft)
                {
                    _rigidbody2D.AddForce(new Vector2(LateralPower /* * GameManager.Instance.BounceStrMultiplierH*/, 0) * typePower, ForceMode2D.Impulse);
                }
                else
                {
                    _rigidbody2D.AddForce(new Vector2(-LateralPower /* * GameManager.Instance.BounceStrMultiplierH*/, 0) * typePower, ForceMode2D.Impulse);
                }
            }

            //apply double tap change and movement but this only when we are on next update
            else if (CanChangeDirection && CanMove && doubleTapTimerOn && !doubleTapped) // on next update
            {
            
                //switch direction
                DirectionLeft = !DirectionLeft;
                var ctx = CharacterFace.transform.localScale;
                ctx.x = !DirectionLeft ? ctx.x : -ctx.x;
                var arrow = DirectionLeft ? HudManager.ArrowPoint.Left : HudManager.ArrowPoint.Right;
                //HudManager.Instance.SetArrowDirection(arrow);
                HudManager.Instance.SetArrowImageAndDirection(false, arrow);
                CharacterFace.transform.localScale = ctx;
                GameManager.StopBall?.Invoke();
                global::Logger.Log("~~~~~~~~~~~~~~~~~Changed Direction to left " + DirectionLeft);
                //StopBall();
                //apply movement in current direction
                if (!DirectionLeft)
                {
                    _rigidbody2D.AddForce(new Vector2(LateralPower /* * GameManager.Instance.BounceStrMultiplierH*/, 0) * typePower, ForceMode2D.Impulse);
                }
                else
                {
                    _rigidbody2D.AddForce(new Vector2(-LateralPower /* * GameManager.Instance.BounceStrMultiplierH*/, 0) * typePower, ForceMode2D.Impulse);
                }

                doubleTapped = true;
                doubleTapTimerOn = false;
                ballDoubleTapTimer = GameManager.Instance.DoubleTapTimer;
                //early out so we do not jump, just roll in a direction
                CanMove = false;
                return;
            }



            if (CurrentParentOcclussionComponent != null && _isTouchingLand)
            {
                global::Logger.Log("~~~~~  reseting platforms");
                var occ =CurrentParentOcclussionComponent;
                CurrentParentOcclussionComponent = null;
                for (int i = 0; i < occ.m_PlatformMovers.Length; i++)
                {
                    occ.m_PlatformMovers[i].ResetToStart();
                }
                
            }


            //climber and ladder ascend controls controls
            if (CanMove && LevelBuilder.Instance.SceneType.Contains(LevelBuilder.SceneTypes.Climber| LevelBuilder.SceneTypes.Ascend))
            {
                if (_isTouchingBase/*_rigidbody2D.IsTouchingLayers(_layerInt)*/)
                {
                    Logger.Log("LateralPower " + LateralPower  +" typePower "+ typePower);
                    _rigidbody2D.AddForce(new Vector2(LateralPower/** GameManager.Instance.BounceStrMultiplierH*/
                        , VerticalPower/* * GameManager.Instance.BounceStrMultiplierV*/) * typePower, ForceMode2D.Impulse);
                    DblJumpOn = true;
                }else if (DoubleJump && DblJumpOn)
                {
                    global::Logger.Log("dbl jump");
                    _rigidbody2D.AddForce(new Vector2(LateralPower  /** GameManager.Instance.BounceStrMultiplierH*/
                        , VerticalPower/* * GameManager.Instance.BounceStrMultiplierV*/) * typePower, ForceMode2D.Impulse);
                    DblJumpOn = false;
                }
            }

            //flappy bird controls
            if (LevelBuilder.Instance.SceneType == LevelBuilder.SceneTypes.FlappyBird)
            {
                if (Started)
                {
                     if (CanMove)
                    {
                        _rigidbody2D.AddForce(new Vector2(0, VerticalPower /* * GameManager.Instance.BounceStrMultiplierV*/) * typePower, ForceMode2D.Impulse);
                    }
                }
            }
        
            //up only controls
            if (LevelBuilder.Instance.SceneType.Contains(LevelBuilder.SceneTypes.UpTube | LevelBuilder.SceneTypes.Ascend))
            {
                if (Started)
                {
                    //global::Logger.Log("started " + LevelBuilder.Instance.SceneType);
                    if (_isTouchingBase/*_rigidbody2D.IsTouchingLayers(_layerInt)*/)
                    {

                        if (GameManager.Instance.kill == null)
                        {
                            GameManager.Instance.kill= StartCoroutine(GameManager.Instance.KillAndReload());
                        }
                        //kill and early out
                        return;
                    }

                    if (CanMove)
                    {
                        _rigidbody2D.AddForce(new Vector2(0, VerticalPower /* * GameManager.Instance.BounceStrMultiplierV*/) * typePower, ForceMode2D.Impulse);
                    }
                }
            }        
            //down only controls
            if (LevelBuilder.Instance.SceneType == LevelBuilder.SceneTypes.DownTube)
            {
                if (Started)
                {
                    if (_isTouchingBase/*_rigidbody2D.IsTouchingLayers(_layerInt)*/)
                    {

                        if (GameManager.Instance.kill == null)
                        {
                            GameManager.Instance.kill= StartCoroutine(GameManager.Instance.KillAndReload());
                        }
                        //kill and early out
                        return;
                    }

                    if (CanMove)
                    {
                        _rigidbody2D.AddForce(new Vector2(0, -VerticalPower /* * GameManager.Instance.BounceStrMultiplierV*/) * typePower, ForceMode2D.Impulse);
                    }
                }
            }


            CanMove = false;
        }

        void Update()
        {
            SetProgress();
            
            //flappy bird controls
            if (LevelBuilder.Instance.SceneType == LevelBuilder.SceneTypes.FlappyBird)
            {
                if (Started)
                {
                    _rigidbody2D.AddForce(new Vector2(GameManager.Instance.PlayerSpeed, 0), ForceMode2D.Force);
                    if (_isTouchingBase && _isAlive /*_rigidbody2D.IsTouchingLayers(_layerInt)*/)
                    {
                        _isAlive = false;
                        global::Logger.Log("dead");
                        if (GameManager.Instance.kill == null)
                        {
                            GameManager.Instance.kill = StartCoroutine(GameManager.Instance.KillAndReload());
                        }

                        //kill and early out
                        return;
                    }
                }
            }

            //calculate effects first
            if (playerEffects.Count> 0)
            {
                var tmpList = new List<int>();
                foreach (var effect in playerEffects)
                {
                    if(!HudManager.Instance.Buffs.ContainsKey(effect.Key)) continue;
                    effect.Value.EffectDuration -= Time.deltaTime;
                    GameManager.Instance.PlayerParams.EffectsOnPlayer[effect.Key] = effect.Value.EffectDuration;
                
                    if (HudManager.Instance.Buffs.ContainsKey(effect.Key))
                    {
                        var buff = HudManager.Instance.Buffs[effect.Key].GetComponent<BuffParams>();
                        buff.DurationTxt.text = "";
                        if(effect.Value.EffectDuration > 0)
                        {
                            buff.DurationTxt.text = effect.Value.EffectDuration.ToString("0:##");
                        }

                    
                    }

                    if (effect.Value.EffectDuration > -1 && effect.Value.EffectDuration <= 0)
                    {
                        tmpList.Add(effect.Key);
                    }
                }

                
                if (_isTouchingBase/*_rigidbody2D.IsTouchingLayers(_layerInt)*/)
                {
                    CanChangeDirection = false;
                    //remove guns
                    ShootingSystem.GunRemove?.Invoke();
                    ShootingSystem.Instance.ResetFallingAllObjects();
                
                    //kill superspeed
               
                    if (playerEffects.ContainsKey(1))
                    {
                        DirectionLeft = false;
                        var ctx = CharacterFace.transform.localScale;
                        ctx.x = !DirectionLeft ? ctx.x : -ctx.x;
                        CharacterFace.transform.localScale = ctx;
                        // HudManager.Instance.SetArrowDirection();
                        if (playerEffects.ContainsKey(5))
                        {
                            // HudManager.Instance.SetArrowImage(false);
                            HudManager.Instance.SetArrowImageAndDirection(false);
                        }
                        else
                        {
                            // HudManager.Instance.SetArrowImage();
                            HudManager.Instance.SetArrowImageAndDirection();
                        }

                        global::Logger.Log("Removed SuperSpeed");
                        GameManager.Instance.OnRemoveEffect(1);
                    }
                }

                foreach (var id in tmpList)
                {
                    GameManager.Instance.OnRemoveEffect(id);
                }
            }

            //calculate timer for doubletap, turn it off if expired
            if (doubleTapTimerOn)
            {
                //global::Logger.Log("timerOn " + ballDoubleTapTimer);
                ballDoubleTapTimer -= Time.deltaTime;
                if (ballDoubleTapTimer <= 0)
                {
                    doubleTapTimerOn = false;
                    global::Logger.Log("timerOFF");
                }
                //early out so we do not jump, just roll in a direction same as above on first tap
                CanMove = false;
                return;
            }

        }

        
        //private void OnDestroy()
        //{
        //    //ControlManager.Instance.DisableBall();
        //    //if (HudManager.Instance.Buffs != null)
        //    //{
        //    //    HudManager.Instance.Buffs.Clear();
        //    //}
        //    //GameManager.Instance.PlayerParams.EffectsOnPlayer = new Dictionary<int, float>();
        //    GameManager.Instance.SavePlayerParams(GameManager.Instance.PlayerParams);
        //}
    }
}