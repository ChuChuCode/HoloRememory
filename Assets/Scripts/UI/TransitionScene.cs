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
    // Indeterminate-duration variant of the above, for covering a stretch of
    // time whose length isn't known up front (e.g. LevelSelectPanel hiding
    // the brief Lobby_Scene pass-through a solo level's StartHost() causes -
    // Network_Manager explicitly calls FadeOut() once the arena is actually
    // ready to reveal, instead of guessing a fixed delay).
    //
    // Scene_FadeOut.anim (played while isFade=true) animates alpha 0->1 over
    // a full second - fine for Transition(time)'s cosmetic flash, but far
    // too slow to actually cover anything: the Lobby_Scene pass-through this
    // exists to hide can finish before the screen is even half-black.
    // Animator.Play(..., 1f) jumps straight to the clip's last frame
    // (fully opaque) instead of animating there.
    public void FadeIn()
    {
        animator.SetBool("isFade", true);
        animator.Play("Scene_FadeOut", 0, 1f);
    }
    // Reveal stays a smooth animated fade (SetBool only, no Play jump) -
    // nothing left to hide at this point, so there's no reason to rush it.
    public void FadeOut()
    {
        animator.SetBool("isFade", false);
    }
}

}