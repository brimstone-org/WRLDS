using System;
using UnityEngine;
using UnityEngine.EventSystems;
using Button = UnityEngine.UI.Button;

namespace _DPS
{

    public class ControlManager : Singleton<ControlManager>
    {
        protected ControlManager(){}

        #region ui and navigation
        [Header("Main Screen ref - used for navigation")]
        public Canvas MainScreen;
    //    [Header("GameWindow ref")]
    //    public ChoseGameWindow GameWindow;
        [Header("GamePrefab")]
        public GameObject GameButtonPrefab;
    
        [Header("choose avatars ref")]
        public GameObject ChooseAvatarPopUp;
        [Header("Next button after picking the avatars ref")]
        public Button NextAvatarButton;

        [Header("Contextual back button ref")]
        public Button BackButton;
        [Header("Play button ref")]
        public Button PlayButton;
        [Header("After choosing the games the next button to avatsr ref")]
        public Button GamePlayButton;

    //    [Header("Game Refs")]
    //    public GameHelp GameHelpWindowParams;
        public Button HelpButton;


        [Header("Avatars refs")]
        public Color InactiveAvatarColor = Color.white;
        public Color ActiveAvatarColor = Color.green;

        #endregion

    

        #region emulation/bounce
    
        public static bool IsPressed;
        public static float PressLenght;
        /// <summary>
        /// with 30 holding uner one sec = soft, 1 to 2 seconds = normal, 2 seconds+ = hard bounce
        /// </summary>
        [Header("with 30 holding uner one sec = soft, 1 to 2 seconds = normal, 2 seconds+ = hard bounce")]
        public float PressMultiplier = 30;
        #endregion
        private float timerLimit;


        #region Classes and enums

        public enum SoundTypes
        {
            Soft,
            Normal,
            Hard
        }

        #endregion

        #region Events

        public static event Action<int, float> OnBounceEmulation;
        public static event Action<int, bool> OnAvatarPick;

        #endregion



        #region Init Logic

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                Destroy(gameObject);
            }
            DontDestroyOnLoad(Instance);
            //Initialize();
        }

        private void Initialize()
        {

            //set help button to disabled
            HelpButton.gameObject.SetActive(false);
            //LanguageManager.Instance.SetLanguage("EN");
        }


        #endregion
    

        #region Bounce Play Logic

        public void EnableBall()
        {
            OnBounceEmulation += TriggerBounce;
        }

        public void DisableBall()
        {
            OnBounceEmulation -= TriggerBounce;
        }

        /// <summary>
        /// called from event will start the play sequence
        /// </summary>
        /// <param name="type"></param>
        /// <param name="strength"></param>
        public void TriggerBounce(int type, float strength)
        {
            //global::Logger.Log($"Bounce type {type} strenght {strength}");
            BallController.Instance.SetBounceData(type, strength);
        }


        #endregion

        ///// <summary>
        ///// this is exposed to be called from button
        ///// </summary>
        //public void GamePlayButtonClick()
        //{
        //    // set first game as game Id
        //    ChooseGame(GameSystem.GameSequenceIds[0]);
        //}

        ///// <summary>
        ///// chose the game and fire the help context with play button
        ///// </summary>
        ///// <param name="gameId"></param>
        //public void ChooseGame(int gameId)
        //{   
        //    //GameComponent.CurrentGameId = gameId;
        //    if (GameSystem.ActivePlayers.Count >= GameSystem.GetMinNumberOfPlayers())
        //    {
        //        PopulateHelpAndFireThePopUp(gameId, false, true);
        //    }
        //    else
        //    {
        //        ChooseAvatarPopUp.SetActive(true);
        //    }
        //}


        ///// <summary>
        ///// populates the help parameters for the chosen/active game and displays the pop-up
        ///// </summary>
        ///// <param name="gameId">the GameCatalogue Id of the game</param>
        ///// <param name="next">used for game sequence</param>
        ///// <param name="play">true to show the play button</param>
        //public static void PopulateHelpAndFireThePopUp(int gameId, bool next, bool play = false)
        //{
        //    //get game
        //    var game = GameCatalogue._instance.GameEntities[gameId];

        //    //populate params
        //    //add help image is we have any and we do not have a spine animation
        //    if (game.GameHelpImage != null && game.SpineHelpAnimation == null)
        //    {
        //        Instance.GameHelpWindowParams.SpineHelpAnimation.enabled = false;
        //        Instance.GameHelpWindowParams.GameHelpImage.sprite = game.GameHelpImage;
        //    }

        //    //override with spine animation if we got any
        //    if (game.SpineHelpAnimation != null)
        //    {
        //        Instance.GameHelpWindowParams.GameHelpImage.enabled = false;
        //        Instance.GameHelpWindowParams.SpineHelpAnimation = game.SpineHelpAnimation;
        //    }

        //    //those are mandatory help fields - should throw error if they are left empty
        //    Instance.GameHelpWindowParams.GameIcon.sprite = game.GameIcon;
        //    Instance.GameHelpWindowParams.GameTitleText.gameObject.GetComponent<LocalizationResponder>().lzLabel = Const.GameNameTxt+ "_" + (game.GameId + 1);
        //    Instance.GameHelpWindowParams.GameHelpText.gameObject.GetComponent<LocalizationResponder>().lzLabel = Const.InstructionsTxt+"_" + (game.GameId + 1);

        //    if (play && !next)
        //    {
        //        Instance.GameHelpWindowParams.PlayBtn.onClick.RemoveAllListeners();
        //        Instance.GameHelpWindowParams.PlayBtn.onClick.AddListener(GameSystem.StartGameSequence);

        //        SetHelpButton(gameId);
        //        //set context: show play button if the pop-up was fired from the choose game button
        //        Instance.GameHelpWindowParams.PlayBtn.gameObject.SetActive(true);
        //        //fire up the popup
        //        Instance.GameHelpWindowParams.gameObject.SetActive(true);
        //    }

        //    if (play && next)
        //    {
        //        Instance.GameHelpWindowParams.PlayBtn.onClick.RemoveAllListeners();
        //        Instance.GameHelpWindowParams.PlayBtn.onClick.AddListener(()=>
        //        {
        //            GameSystem.ThisGameLoopCompleted = true;
        //        });

        //        SetHelpButton(gameId);
        //        //set context: show play button if the pop-up was fired from the choose game button
        //        Instance.GameHelpWindowParams.PlayBtn.gameObject.SetActive(true);
        //        //fire up the popup
        //        Instance.GameHelpWindowParams.gameObject.SetActive(true);
        //    }

        //}

        ///// <summary>
        ///// setup the help button
        ///// </summary>
        ///// <param name="gameId"></param>
        //public static void SetHelpButton(int gameId)
        //{
        //    global::Logger.Log("Play game " + gameId);
        //    //set listener to the helpButton
        //    Instance.HelpButton.onClick.RemoveAllListeners();
        //    Instance.HelpButton.onClick.AddListener(() =>
        //    {
        //        PopulateHelpAndFireThePopUp(gameId, false);
        //    });

        //    // set help button to true
        //    Instance.HelpButton.gameObject.SetActive(true);
        //}

        #region PressingEmulation

        /// <summary>
        /// dounts the strength of emulator press
        /// </summary>
        public static void CountPressingEmulation()
        {
            //soft 0-30
            // normal 30-60
            // hard 60 >
            PressLenght += Time.deltaTime * Instance.PressMultiplier;
        }

        /// <summary>
        /// returns bounce type and invokes the fart event
        /// </summary>
        public static void EndPressing()
        {
            int type = 0;
            //after done with the while put arm back the bool
            //todo to be later used for controlling bounces as well
            if (PressLenght >= 0 && PressLenght <= 30)
            {
                type = (int)SoundTypes.Soft;
            }
            else if (PressLenght > 30 && PressLenght < 60)
            {
                type = (int)SoundTypes.Normal;
            }
            else if (PressLenght > 60)
            {
                type = (int)SoundTypes.Hard;
            }
            //global::Logger.Log("pressed for " + PressLenght + " type = " + type);
            //invoke bounce
            OnBounceEmulation?.Invoke(type, PressLenght);
            //reset
            PressLenght = 0;
        }

        #endregion
        //void OnApplicationFocus( bool focusStatus )
        //{
        //    if (!focusStatus)
        //    {
        //        Time.timeScale = 1;
        //        //AudioManager.instance.StopAllSounds();
        //        PlayerPrefs.Save();
        //    }
        //    else
        //    {
        //        Time.timeScale = 0;
        //    }
        //}

        //void OnApplicationPause( bool pauseStatus )
        //{
        //    if (pauseStatus)
        //    {            
        //        Time.timeScale = 1;
        //        //AudioManager.instance.StopAllSounds();
        //        PlayerPrefs.Save();
        //    }
        //    else
        //    {
        //        Time.timeScale = 0;
        //    }
        //}

        void Update()
        {
            if(EventSystem.current.IsPointerOverGameObject()) return;
            if (Input.GetKeyUp(KeyCode.Space) || (!ConnectionManager.Instance.EnableConnection && Input.GetButtonUp("Fire1")))
            {
                
                EndPressing();
            }
            if (Input.GetKey(KeyCode.Space) ||  (!ConnectionManager.Instance.EnableConnection && Input.GetButton("Fire1")))
            {
                CountPressingEmulation();
            }

        }
    }
}