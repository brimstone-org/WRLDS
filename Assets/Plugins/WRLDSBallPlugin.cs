using UnityEngine;
using System;
using System.Collections;
using System.Runtime.InteropServices;
using AOT;

/**
 * This class interfaces with the Unity WRLDS iOS Native plugin
 */
public class WRLDSBallPlugin : Singleton<WRLDSBallPlugin>
{
#if UNITY_IOS

    #region Native Implementation Interface
    [DllImport("__Internal")]
    private static extern void initializer();

    [DllImport("__Internal")]
    private static extern void finalizer();

    // Interaction Delegate Callback Setters
    [DllImport("__Internal")]
    private static extern void set_interaction_delegate_ball_did_bounce_callback(BallDidBounceCallback callback);

    [DllImport("__Internal")]
    private static extern void set_interaction_delegate_ball_did_shake_callback(BallDidShakeCallback callback);

    [DllImport("__Internal")]
    private static extern void set_interaction_delegate_ball_did_receive_fifo_data_callback(BallDidReceiveFifoDataCallback callback);

    [DllImport("__Internal")]
    private static extern void set_connection_delegate_ball_did_change_state_callback(BallDidChangeConnectionStateCallback callback);

    // Properties
    [DllImport("__Internal")]
    private static extern void set_low_threshold(double threshold); // Default 30.0

    [DllImport("__Internal")]
    private static extern void set_high_threshold(double threshold); // Default 60.0

    [DllImport("__Internal")]
    private static extern void set_shake_interval(double intervalSeconds); // Default 0.250

    [DllImport("__Internal")]
    private static extern string get_connection_state();

    [DllImport("__Internal")]
    private static extern string get_device_address();

    // Actions
    [DllImport("__Internal")]
    private static extern void start_scanning();

    [DllImport("__Internal")]
    private static extern void stop_scanning();

    [DllImport("__Internal")]
    private static extern void connect_to_ball(string address);

    #endregion


    #region Native Callbacks
    // Define function pointers that can be passed as arguments
    private delegate void BallDidBounceCallback(int type, float sumG);
    private delegate void BallDidShakeCallback(double shakeProgress);
    private delegate void BallDidReceiveFifoDataCallback(IntPtr fifoData, int length);
    private delegate void BallDidChangeConnectionStateCallback(int state, string stateMessage);
    #endregion


    #region Native Callback function pointer retainers
    // Holds on to the function pointers so that they cannot be deallocated when GC runs
    private BallDidBounceCallback _ball_did_bounce_callback_holder;
    private BallDidShakeCallback _ball_did_shake_callback_holder;
    private BallDidReceiveFifoDataCallback _ball_did_receive_fifo_data_callback_holder;
    private BallDidChangeConnectionStateCallback _ball_did_change_connection_state_callback_holder;
    #endregion

    public delegate void BounceDelegate(int bounceType, float sumG);
    public delegate void ShakeDelegate(float shakeProgress);
    public delegate void FifoDataDelegate(float[] fifoDataObject);
    public delegate void ConnectionStateChangeDelegate(int connectionState, string stateMessage);

    public event BounceDelegate OnBounce;
    public event ShakeDelegate OnShake;
    public event FifoDataDelegate OnFifoData;
    public event ConnectionStateChangeDelegate OnConnectionStateChange;

#elif UNITY_ANDROID
    private AndroidJavaObject pluginClass;
    private WRLDSAndroidPluginBounceCallback onBounceCallback;
    private WRLDSAndroidPluginShakeCallback onShakeCallback;
    private WRLDSAndroidPluginFifoDataReceivedCallback onFifoDataCallback;
    private WRLDSAndroidPluginStateChangeCallback onStateChangeCallback;
    private WRLDSAndroidPluginGenericCallback onGenericCallback;

    public event WRLDSAndroidPluginBounceCallback.BounceDelegate OnBounce
    {
        add { onBounceCallback.OnBounce += value; }
        remove { onBounceCallback.OnBounce -= value; }
    }
    public event WRLDSAndroidPluginShakeCallback.ShakeDelegate OnShake
    {
        add { onShakeCallback.OnShake += value; }
        remove { onShakeCallback.OnShake -= value; }
    }
    public event WRLDSAndroidPluginFifoDataReceivedCallback.FifoDataDelegate OnFifoDataReceived
    {
        add { onFifoDataCallback.OnFifoDataReceived += value; }
        remove { onFifoDataCallback.OnFifoDataReceived -= value; }
    }
    public event WRLDSAndroidPluginStateChangeCallback.ConnectionStateChangedDelegate OnConnectionStateChange
    {
        add { onStateChangeCallback.OnConnectionStateChanged += value; }
        remove { onStateChangeCallback.OnConnectionStateChanged -= value; }
    }
    public event WRLDSAndroidPluginGenericCallback.GenericDelegate OnGenericReceived
    {
        add { onGenericCallback.OnGenericReceived += value; }
        remove { onGenericCallback.OnGenericReceived -= value; }
    }

#endif

    void Awake()
    {
#if UNITY_IOS
        DontDestroyOnLoad(this.gameObject);

        // Store the callback functions
        _ball_did_bounce_callback_holder = new BallDidBounceCallback(_ball_did_bounce_callback);
        _ball_did_shake_callback_holder = new BallDidShakeCallback(_ball_did_shake_callback);
        _ball_did_receive_fifo_data_callback_holder = new BallDidReceiveFifoDataCallback(_ball_did_receive_fifo_data_callback);
        _ball_did_change_connection_state_callback_holder = new BallDidChangeConnectionStateCallback(_ball_did_change_connection_state_callback);

        // Set native pointers to internal callbacks
        set_interaction_delegate_ball_did_bounce_callback(_ball_did_bounce_callback_holder);
        set_interaction_delegate_ball_did_shake_callback(_ball_did_shake_callback_holder);
        set_interaction_delegate_ball_did_receive_fifo_data_callback(_ball_did_receive_fifo_data_callback_holder);
        set_connection_delegate_ball_did_change_state_callback(_ball_did_change_connection_state_callback_holder);

        initializer();
#elif UNITY_ANDROID
        DontDestroyOnLoad(this.gameObject);

        // First the SDK needs the activity context, so we create a reference to the UnityPlayer class.
        AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        if (jc == null)
            Debug.Log("unable to get unity activity class");

        // Second we get a reference to the activity.
        AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");

        if (jo == null) {
            Debug.Log("unable to get current activity");
        }
        else
        {
            Debug.Log("Current activity read");
        }
        // The SDK needs to be instatiated with a reference to the activity and the Ball Listener callback interface.
        pluginClass = new AndroidJavaObject("com.wrlds.sdk.Ball", new object[] { jo });

        onBounceCallback = new WRLDSAndroidPluginBounceCallback();
        onShakeCallback = new WRLDSAndroidPluginShakeCallback();
        onFifoDataCallback = new WRLDSAndroidPluginFifoDataReceivedCallback();
        onStateChangeCallback = new WRLDSAndroidPluginStateChangeCallback();
        onGenericCallback = new WRLDSAndroidPluginGenericCallback();

        pluginClass.Call("setOnConnectionStateChangedListener", onStateChangeCallback);
        pluginClass.Call("setOnBounceListener", onBounceCallback);
        pluginClass.Call("setOnShakeListener", onShakeCallback);
        pluginClass.Call("setOnFifoDataReceivedListener", onFifoDataCallback);
        pluginClass.Call("setOnGenericReceivedListener", onGenericCallback);


#else
        Destroy(this);
#endif
    }


#if UNITY_IOS
    #region Internal Interaction Callbacks
    [MonoPInvokeCallback(typeof(BallDidBounceCallback))]
    private static void _ball_did_bounce_callback(int type, float sumG)
    {
        Debug.Log("WRLDSBall_iOS::BallDidBounce(" + type + ", " + sumG + ")");
        if (WRLDSBallPlugin.Instance.OnBounce != null)
        {
            WRLDSBallPlugin.Instance.OnBounce.Invoke(type, sumG);
        }
    }

    [MonoPInvokeCallback(typeof(BallDidShakeCallback))]
    private static void _ball_did_shake_callback(double shakeProgress)
    {
        // TODO: Trigger Unity based callback
        Debug.Log("WRLDSBall_iOS::BallDidShake(" + shakeProgress + ")");
    }

    [MonoPInvokeCallback(typeof(BallDidReceiveFifoDataCallback))]
    private static void _ball_did_receive_fifo_data_callback(IntPtr fifoData, int length)
    {
        var floatArray = new float[length];
        Marshal.Copy(fifoData, floatArray, 0, length);

        if (WRLDSBallPlugin.Instance.OnFifoData != null)
        {
            Debug.Log("OnFifoData");
            WRLDSBallPlugin.Instance.OnFifoData.Invoke(floatArray);
        }
    }

    [MonoPInvokeCallback(typeof(BallDidChangeConnectionStateCallback))]
    private static void _ball_did_change_connection_state_callback(int state, string stateMessage)
    {
        Debug.Log("WRLDSBall_iOS::BallDidChangeConnectionState(" + state + ")");
        if (WRLDSBallPlugin.Instance.OnConnectionStateChange != null)
        {
            WRLDSBallPlugin.Instance.OnConnectionStateChange.Invoke(state, stateMessage);
        }
    }
    #endregion
#endif

    private void _teardown()
    {
#if UNITY_IOS
        set_interaction_delegate_ball_did_bounce_callback(null);
        set_interaction_delegate_ball_did_shake_callback(null);
        set_connection_delegate_ball_did_change_state_callback(null);

        _ball_did_bounce_callback_holder = null;
        _ball_did_shake_callback_holder = null;
#elif UNITY_ANDROID
#endif
    }

    public void ScanForDevices()
    {
        Debug.Log("ScanForDevices");
#if UNITY_IOS
        start_scanning();
#elif UNITY_ANDROID
        pluginClass.Call("scanForDevices");
#endif
    }

    public string GetDeviceAddress()
    {
#if UNITY_IOS
        string hash = get_device_address();
#elif UNITY_ANDROID
		string hash = pluginClass.Call<string>("getDeviceAddress");
#else
        string hash = "00:00:00:00:00:00";
#endif
        return hash;
    }

    public void DisconnectDevice() {
#if UNITY_IOS
//
#elif UNITY_ANDROID
		pluginClass.Call("disconnectDevice");
		pluginClass.Call("destroy");
#endif
	}

	public void ConnectDevice(string hash)
	{
#if UNITY_IOS
        Debug.Log("Connecting to device: " + hash);

        connect_to_ball(hash);
#elif UNITY_ANDROID
		pluginClass.Call("connectDevice", hash);
#endif
	}

    public void SignUp(string mail, string password)
    {
#if UNITY_ANDROID
        pluginClass.Call("signUp", new object[] { mail, password });
#endif
    }

    public void SignIn(string mail, string password)
    {
#if UNITY_ANDROID
        pluginClass.Call("signIn", mail, password);
#endif
    }

    public void SignOut()
    {
#if UNITY_ANDROID
        pluginClass.Call("signOut");
#endif
    }

    public void CheckLoginState()
    {
#if UNITY_ANDROID
        pluginClass.Call("checkLoginState");
#endif
    }

    public void ChangePassword(string oldPassword, string newPassword)
    {
#if UNITY_ANDROID
        pluginClass.Call("changePassword", oldPassword, newPassword);
#endif
    }

    public void RequestNewPassword()
    {
#if UNITY_ANDROID
        pluginClass.Call("requestNewPassword");
#endif
    }

    public void ResetPassword(string newPassword, string code)
    {
#if UNITY_ANDROID
        pluginClass.Call("confirmPassword", newPassword, code);
#endif
    }

    public void SubmitMailCode(string attributeName, string code)
    {
#if UNITY_ANDROID
        pluginClass.Call("confirmAttribute", attributeName, code);
#endif
    }

    public void ChangeAttribute(string attributeKey, string attributeValue)
    {
#if UNITY_ANDROID
        pluginClass.Call("changeAttribute", attributeKey, attributeValue);
#endif
    }

    public void DeleteUserAccount(string user)
    {
#if UNITY_ANDROID
        pluginClass.Call("deleteUserAccount", user);
#endif
    }

#if UNITY_ANDROID

    public class WRLDSAndroidPluginBounceCallback : AndroidJavaProxy
    {
        public delegate void BounceDelegate(int bounceType, float sumG);
        public event BounceDelegate OnBounce;
        public WRLDSAndroidPluginBounceCallback() : base("com.wrlds.sdk.Ball$OnBounceListener") { }
        void onBounce(int bounceType, float sumG)
        {
            if (OnBounce != null)
            {OnBounce.Invoke(bounceType, sumG);}
        }
    }

    public class WRLDSAndroidPluginShakeCallback : AndroidJavaProxy
    {
        public delegate void ShakeDelegate(float shakeProgress);
        public event ShakeDelegate OnShake;
        public WRLDSAndroidPluginShakeCallback() : base("com.wrlds.sdk.Ball$OnShakeListener") { }
        void onShake(float shakeProgress)
        {
            if (OnShake != null)
            { OnShake.Invoke(shakeProgress);}
        }
    }

    public class WRLDSAndroidPluginFifoDataReceivedCallback : AndroidJavaProxy
    {
        public delegate void FifoDataDelegate(AndroidJavaObject fifoDataObject);
        public event FifoDataDelegate OnFifoDataReceived;
        public WRLDSAndroidPluginFifoDataReceivedCallback() : base("com.wrlds.sdk.Ball$OnFifoDataReceivedListener") { }
        void onFifoDataReceived(AndroidJavaObject fifoDataObject)
        {
            if (OnFifoDataReceived != null)
            {OnFifoDataReceived.Invoke(fifoDataObject);}
        }
    }

    public class WRLDSAndroidPluginStateChangeCallback : AndroidJavaProxy
    {

        public delegate void ConnectionStateChangedDelegate(int connectionState, string stateMessage);
        public event ConnectionStateChangedDelegate OnConnectionStateChanged;
        public WRLDSAndroidPluginStateChangeCallback() : base("com.wrlds.sdk.Ball$OnConnectionStateChangedListener") { }
        void onConnectionStateChanged(int connectionState, string stateMessage)
        {
            if (OnConnectionStateChanged != null)
            { OnConnectionStateChanged.Invoke(connectionState, stateMessage);}
        }
    }

    public class WRLDSAndroidPluginGenericCallback : AndroidJavaProxy
    {
        public delegate void GenericDelegate(string action, bool state, string message, AndroidJavaObject result);
        public event GenericDelegate OnGenericReceived;
        public WRLDSAndroidPluginGenericCallback() : base("com.wrlds.sdk.Ball$OnGenericReceivedListener") { }
        void onGenericReceived(string action, bool state, string message, AndroidJavaObject result)
        {
            if (OnGenericReceived != null)
            { OnGenericReceived.Invoke(action, state, message, result); }
        }
    }

    bool callOnResume = false;

    void OnApplicationPause(bool pause)
    {
        if (pause)
            pluginClass.Call("onPause");

        if (!pause && callOnResume)
        {
            pluginClass.Call("onResume");
        }
        //We do not want onResume() to trigger during the initialization of the app
        //Therefore callOnResume is initially false in order to block the first attempt to call onResume()
        //For more information you can look for android lifecycle
        callOnResume = true;
    }

    void OnDestroy()
    {
        pluginClass.Call("onDestroy");
    }

#endif

}
