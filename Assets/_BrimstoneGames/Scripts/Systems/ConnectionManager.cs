using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.UI;

namespace _DPS
{

    public class ConnectionManager : Singleton<ConnectionManager>
    {
        protected ConnectionManager(){}

        /// <summary>
        /// current ball macaddress
        /// </summary>
        public string macAdr = "";

        [Header("Send states in play mode, user the int field below")]
        public Button EmulateStateButton;

        /// <summary>
        /// state 2, connecting, 4 disconnected, 5 connecting, 6 connected, 7 starting, 10 disconnecting manually
        /// </summary>
        [Header("states in tooltip")]
        [Tooltip(
            "state 2, connecting, 4 disconnected, 5 connecting, 6 connected, 7 starting, 10 disconnecting manually")]
        public int EmulatorState = 2;


        public Button ResumeButton;
        public Canvas popUpCanvas;

        [Header("Debug Only, turn it off in release build")]
        public TextMeshProUGUI ConnectionState;

        [Header("Enable Ball BT Connection")] public bool EnableConnection;

        public float IosReconnectTimeOut = 200;
        private Coroutine _iosRetry;
        private static bool _isPaused;

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                Destroy(gameObject);
            }
            DontDestroyOnLoad(Instance);
        }

        void Start()
        {
            Time.timeScale = 0;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            ResumeButton.interactable = false;
            if (EnableConnection)
            {
                Singleton<WRLDSBallPlugin>.Instance.Ping();
            }

            EnableEmulator();

            ResumeButton.onClick.AddListener(ResumePlay);


        }

        /// <summary>
        /// shows the emulator btn
        /// </summary>
        private void EnableEmulator()
        {

            EmulateStateButton.gameObject.SetActive(!EnableConnection);
            if (EmulateStateButton.gameObject.activeSelf)
            {
                EmulateStateButton.onClick.AddListener(EmulateState);
                popUpCanvas.enabled = true;
            }
            else
            {
                StartCoroutine(Initialize());
            }

        }

        /// <summary>
        /// send emulator states
        /// </summary>
        public void EmulateState()
        {
            var message = "";
            macAdr = "123456";
            HandleConnectionStates(EmulatorState, message);
        }

        private IEnumerator Initialize()
        {
            if (!EnableConnection) yield break;
            //ControlManager.Instance.PlayButton.gameObject.SetActive(false);
            if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
            {
                //pop up permission
                Permission.RequestUserPermission(Permission.FineLocation);
            }

            yield return new WaitUntil(() => Permission.HasUserAuthorizedPermission(Permission.FineLocation));
            popUpCanvas.enabled = true;

            yield return null;
            Singleton<WRLDSBallPlugin>.Instance.ScanForDevices();
            Singleton<WRLDSBallPlugin>.Instance.OnBounce += ControlManager.Instance.TriggerBounce;
            Singleton<WRLDSBallPlugin>.Instance.OnConnectionStateChange += HandleConnectionStates;
        }

        /// <summary>
        /// Will handle connection window by states only
        /// </summary>
        /// <param name="state">state 2, connecting, 4 disconnected, 5 connecting, 6 connected, 7 starting, 10 disconnecting manually</param>
        /// <param name="message"></param>
        private void HandleConnectionStates(int state, string message)
        {
            global::Logger.Log("ConnectionState " + state);
            //ConnectionState.text = state.ToString("N0");
            //ResumeButton.gameObject.SetActive(false);
            // state 2, connecting, 4 disconnected, 5 connecting, 6 connected, 7 starting, 10 disconnecting manually
            // so the popup does not appear on startup.
            switch (state)
            {
                case 2:
                    ResumeButton.interactable = false;
                    ConnectionState.text = $"Scanning\nbounce the ball...";
                    Singleton<WRLDSBallPlugin>.Instance.ScanForDevices();
                    Time.timeScale = 0;
                    break;
                case 4:
                    popUpCanvas.enabled = true;
                    ResumeButton.interactable = false;
                    ConnectionState.text = $"Disconected!";
                    AudioManager.Instance.StopAllButMusic();
                    //pause the game
                    Time.timeScale = 0;
                    if (!string.IsNullOrEmpty(macAdr))
                    {
#if UNITY_IOS
                    _iosRetry = StartCoroutine(IosRetry());
#endif
                        Singleton<WRLDSBallPlugin>.Instance.ConnectDevice(macAdr);
                    }
                    else
                    {
                        Singleton<WRLDSBallPlugin>.Instance.ScanForDevices();
                    }

                    Time.timeScale = 0;
                    break;
                case 5:
                    popUpCanvas.enabled = true;
                    ResumeButton.interactable = false;
                    ConnectionState.text = $"Connecting...";
                    Time.timeScale = 0;
                    break;
                case 7:
                    popUpCanvas.enabled = true;
                    ResumeButton.interactable = false;
                    ConnectionState.text = $"Starting...";
                    Time.timeScale = 0;
                    break;
                case 9:
                    popUpCanvas.enabled = true;
                    ResumeButton.interactable = false;
                    ConnectionState.text = $"Failed to connect...\n Keep the ball close and bounce it!";
                    Time.timeScale = 0;
                    break;
                case 10:
                    popUpCanvas.enabled = true;
                    ResumeButton.interactable = false;
                    ConnectionState.text = $"Stopped Searching.";
                    Time.timeScale = 0;
                    break;
                case 6:
                    //stop retry for iOS
                    if (_iosRetry != null)
                    {
                        StopCoroutine(_iosRetry);
                        _iosRetry = null;
                        ConnectionState.enabled = true;
                    }

                    ConnectionState.text = $"Connected!\n You may play now.";
                    //Time.timeScale = 0.01f;
                    ResumeButton.interactable = true;
                    Time.timeScale = 0;
                    if (string.IsNullOrEmpty(macAdr))
                    {
                        macAdr = Singleton<WRLDSBallPlugin>.Instance.GetDeviceAddress();
                        global::Logger.Log("connected to - " + macAdr);
                    }


                    //ControlManager.Instance.PlayButton.gameObject.SetActive(true);
                    break;
            }
        }

        /// <summary>
        /// on resume button
        /// </summary>
        public void ResumePlay()
        {
            EmulateStateButton.gameObject.SetActive(false);
            popUpCanvas.enabled = false;
            Time.timeScale = 1;
        }

        private IEnumerator ResumeTimer()
        {
            AudioManager.Instance.PlayLooped("tickingClock");
            var t = 3f;
            while (t > 0)
            {
                t -= Time.unscaledDeltaTime;
                t = Mathf.Clamp(t, 0f, 3f);
                ConnectionState.text = "The game will resume in: " + t.ToString("N0");
                yield return new WaitForSecondsRealtime(Time.unscaledDeltaTime * 3f);
            }

            yield return new WaitUntil(() => t <= 0f);
            popUpCanvas.enabled = false;
            AudioManager.Instance.StopLoop("tickingClock");
            Time.timeScale = 1;
        }

        /// <summary>
        /// used to handle Ios Reconnecting until the SDk will be updated
        /// </summary>
        /// <returns></returns>
        private IEnumerator IosRetry()
        {
            var timer = IosReconnectTimeOut;
            while (timer > 0)
            {
                //scan every 5 frames to not spam too much and have time to stop properly
                ConnectionState.enabled = false;
                timer -= Time.unscaledDeltaTime;
                Singleton<WRLDSBallPlugin>.Instance.ConnectDevice(macAdr);
                yield return null;
            }
        }

        #region Application StatesHandling

        //void OnDestroy()
        //{
        //    UnSubscribeFromEvents();
        //}

        ///// <summary>
        ///// unsubscribes from sdk events
        ///// </summary>
        //private void UnSubscribeFromEvents()
        //{
        //    Singleton<WRLDSBallPlugin>.Instance.OnBounce -= ControlManager.Instance.TriggerBounce;
        //    Singleton<WRLDSBallPlugin>.Instance.OnConnectionStateChange -= HandleConnectionStates;
        //}

        #endregion

        //void Update()
        //{
        //    Time.timeScale = popUpCanvas.isActiveAndEnabled ? 0 : 1;
        //}
    }
}