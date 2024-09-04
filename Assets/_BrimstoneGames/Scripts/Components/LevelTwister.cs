using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using _DPS;
using Cinemachine;

public class LevelTwister : MonoBehaviour
{
    public Vector3 Angle; //the angle of rotation
    public float RotationTime;// the time it takes to rotate
    public float CameraOffsetX, CameraOffsetY; //the offsets for the camera relative to the player

    public IEnumerator RotateLevel(Vector3 _targetRotation, float _cameraOffsetXTarget, float _cameraOffsetYTarget, float _overTime)
    {
        float starTime = Time.time;
        //gett offsets
        float currentXOffset = LevelBuilder.Instance.CamSetup.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX;
        float currentYOffset = LevelBuilder.Instance.CamSetup.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY; 
        Vector3 currentRotation = LevelBuilder.Instance.CamSetup.transform.localEulerAngles; //get current camera rotation
        if (currentRotation != _targetRotation) //If the camera doesn't already have that rotation
        {
            while (Time.time < starTime + _overTime)
            {
                //lerp offsets and rotation to the target ones
                LevelBuilder.Instance.CamSetup.transform.localEulerAngles = Vector3.Lerp(currentRotation,
                    _targetRotation, (Time.time - starTime) / _overTime);
                LevelBuilder.Instance.CamSetup.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX =
                    Mathf.Lerp(currentXOffset, _cameraOffsetXTarget, (Time.time - starTime) / _overTime);
                LevelBuilder.Instance.CamSetup.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY =
                    Mathf.Lerp(currentYOffset, _cameraOffsetYTarget, (Time.time - starTime) / _overTime);
                yield return null;
            }

            LevelBuilder.Instance.CamSetup.transform.localEulerAngles = _targetRotation;
            LevelBuilder.Instance.CamSetup.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenX =
                _cameraOffsetXTarget;
            LevelBuilder.Instance.CamSetup.GetCinemachineComponent<CinemachineFramingTransposer>().m_ScreenY =
                _cameraOffsetYTarget;
        }
    }

/*
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player") && !other.isTrigger)
        {   
            global::Logger.Log("tWISTING LEVEL");
            StartCoroutine(RotateLevel(Angle, CameraOffsetX, CameraOffsetY, 0.5f));
        }
    }*/
}
