using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HR.UI{
public class TransitionScene : MonoBehaviour
{
    public static TransitionScene Instance;
    [SerializeField] Animator animator;
    void Awake()
    {
        DontDestroyOnLoad(this);
        if (Instance == null)
        {
            Instance = this;
        }
        if (Instance != this)
        {
            Destroy(gameObject);
        }
    }
    public void Transition(float time)
    {
        StartCoroutine(nameof(Start_Transition),time);
    }
    IEnumerator Start_Transition(float time)
    {
        animator.SetBool("isFade",true);
        yield return new WaitForSeconds(time);
        animator.SetBool("isFade",false);
    }
}

}