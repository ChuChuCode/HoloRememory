using System.Collections;
using UnityEngine;
using TMPro;
using DG.Tweening;
using HR.Network;

namespace HR.UI{
// Shows "3, 2, 1" (ceil of remaining seconds) during the match-start input
// lock window, then hides. Driven by GameSettings.matchCountdownEndTime (a
// SyncVar, not a ClientRpc) - not driven off CharacterBase.isInputLocked
// directly either, since that SyncVar's hook doesn't reliably fire for its
// own initial synced value (Mirror only fires a hook on initial sync if
// the value differs from the compile-time default, and isInputLocked's
// default IS true).
public class MatchCountdownUI : MonoBehaviour
{
    public static MatchCountdownUI instance;
    // Also doubles as the show/hide target (countdownText.gameObject) -
    // deliberately not a separate GameObject field. Must NOT be the same
    // GameObject this script itself lives on: SetActive(false)-ing your
    // own object stops Unity from ever calling Start()/allowing
    // StartCoroutine() on it again until something else re-enables it -
    // this component needs to live on a stable parent, with this pointing
    // at a child.
    [SerializeField] TMP_Text countdownText;
    // Reserves the FINAL second of whatever duration StartCountdown() gets
    // for showing "Start" instead of a 4th number - e.g. a 4s duration
    // reads as 3, 2, 1 (one per second) then "Start" for the last second,
    // not a literal countdown to 0. Network_Manager.matchStartLockDuration
    // is the single source of truth for the whole window (numbers +
    // Start) - unlock/minion-wander/SP-regen/match-timer-start all wait
    // for the same total, so nothing goes live before "Start" finishes
    // showing.
    [SerializeField] float startFlashDuration = 1f;
    [Header("Punch Scale")]
    [SerializeField] float punchScale = 0.4f;
    [SerializeField] float punchDuration = 0.4f;
    [SerializeField] int punchVibrato = 6;
    [SerializeField] float punchElasticity = 0.6f;

    Coroutine countdownRoutine;

    void Awake()
    {
        if (instance == null) instance = this;
        if (countdownText != null) countdownText.gameObject.SetActive(false);
    }

    // Pulls whatever countdown state already exists - covers the case
    // where GameSettings.matchCountdownEndTime was already set (and its
    // SyncVar hook already fired) before this object finished loading, e.g.
    // Game_Canvas re-instantiating slightly later than GameSettings itself
    // during a scene transition. The hook (GameSettings.ApplyMatchCountdown)
    // covers the opposite ordering, when this object is already ready and
    // the value changes afterward - between the two, correct either way.
    void Start()
    {
        if (GameSettings.Instance == null) return;
        GameSettings.Instance.ApplyMatchCountdown();
        GameSettings.Instance.ApplyMatchFinished();
    }

    public void StartCountdown(float duration)
    {
        if (countdownRoutine != null) StopCoroutine(countdownRoutine);
        countdownRoutine = StartCoroutine(CountdownRoutine(duration));
    }

    IEnumerator CountdownRoutine(float duration)
    {
        if (countdownText != null) countdownText.gameObject.SetActive(true);

        float remaining = duration;
        int lastShown = -1;
        // Numeric phase - stops one startFlashDuration early, so the last
        // number (e.g. "1") gets its own full second before Start takes over.
        while (remaining > startFlashDuration)
        {
            int toShow = Mathf.CeilToInt(remaining - startFlashDuration);
            if (toShow != lastShown)
            {
                lastShown = toShow;
                SetTextAndPunch(toShow.ToString());
            }
            yield return null;
            remaining -= Time.deltaTime;
        }

        // Start phase - one flash, held for the remaining time instead of
        // re-triggering the punch every frame.
        SetTextAndPunch("Start");
        while (remaining > 0f)
        {
            yield return null;
            remaining -= Time.deltaTime;
        }

        if (countdownText != null) countdownText.gameObject.SetActive(false);
        countdownRoutine = null;
    }

    // One-shot "FINISH" flash at match end - reuses the same text/punch
    // display as the numeric countdown, just no ticking loop. Left showing
    // (not auto-hidden) since the scene changes to the Lobby a few seconds
    // later anyway, tearing this whole object down with it.
    public void ShowFinish()
    {
        if (countdownRoutine != null)
        {
            StopCoroutine(countdownRoutine);
            countdownRoutine = null;
        }
        if (countdownText != null) countdownText.gameObject.SetActive(true);
        SetTextAndPunch("FINISH");
    }

    void SetTextAndPunch(string text)
    {
        if (countdownText == null) return;
        countdownText.text = text;
        countdownText.transform.DOKill();
        countdownText.transform.localScale = Vector3.one;
        countdownText.transform.DOPunchScale(Vector3.one * punchScale, punchDuration, punchVibrato, punchElasticity);
    }
}
}
