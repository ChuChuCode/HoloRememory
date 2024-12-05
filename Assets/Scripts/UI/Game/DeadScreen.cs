using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
namespace HR.UI{
public class DeadScreen : MonoBehaviour
{
    public static DeadScreen instance ;
    Volume volume;
    ColorAdjustments colorAdjustments;
    
    void Awake()
    {
        instance = this;
        volume = GetComponent<Volume>();
    }
    void Start()
    {
        volume.profile.TryGet<ColorAdjustments>(out colorAdjustments);
    }

    // Update is called once per frame
    public void isDead(bool isDead)
    {
        colorAdjustments.saturation.value = isDead ? -100f : 0;
    }
}
}