using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

namespace HR.Object.Player{
public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance {get; private set;}
    CinemachineVirtualCamera cinemachineVirtualCamera;
    CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin;
    float Timer;
    void Awake()
    {
        Instance = this;
        cinemachineVirtualCamera = GetComponent<CinemachineVirtualCamera>();
        cinemachineBasicMultiChannelPerlin = cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    public void ShakeCamera(float intensity,float time)
    {
        //CinemachineBasicMultiChannelPerlin cinemachineBasicMultiChannelPerlin =
        //   cinemachineVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        
        cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = intensity;
        Timer = time;
    }
    private void Update()
    {
        if (Timer > 0)
        {
            Timer -= Time.deltaTime;
            if (Timer <= 0)
            {
                cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = 0f;
            }
        }
    }
}

}