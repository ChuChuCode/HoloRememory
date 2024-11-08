using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MianInfoUI : MonoBehaviour
{
    public static MianInfoUI instance;
    [SerializeField] Bar HP;
    [SerializeField] Bar MP;
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public void updateInfo(IHealth health)
    {
        HP.SetMaxValue(health.maxHealth);
        HP.SetValue(health.currentHealth);
    }
}
