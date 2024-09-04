using System.Collections;
using UnityEngine;

public class SDKStart : MonoBehaviour
{

    void Start()
    {
        Singleton<WRLDSBallPlugin>.Instance.Ping();
        StartCoroutine(ExecuteAfterTime(5));
    }

    void Update()
    {
    }

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);

        Singleton<WRLDSBallPlugin>.Instance.ScanForDevices();
    }
}

