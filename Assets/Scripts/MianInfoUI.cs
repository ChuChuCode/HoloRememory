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
    public void updateInfo(Health health)
    {
        HP.SetMaxValue(health.maxHealth);
        HP.SetValue(health.currentHealth);
    }
}
