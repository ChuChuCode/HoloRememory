using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;
using HR.Story;

namespace HR.UI{
// Local-only playback for a DialogueScript - never networked. Lives in the
// Game_Canvas prefab (shared by every arena scene), not Main_Scene - a solo
// level's intro dialogue plays AFTER the scene transition into the arena
// (character already spawned, input still locked), gating the match-start
// countdown until it finishes. See
// Network_Manager.WaitForAllPlayersLoadedThenStartMatch.
public class DialogueUI : MonoBehaviour
{
    public static DialogueUI Instance;

    [SerializeField] GameObject panel;
    [SerializeField] Image portraitImage;
    [SerializeField] TMP_Text speakerNameText;
    [SerializeField] TMP_Text bodyText;
    // Other in-match HUD elements (top bar roster, local player panel, skill
    // energy, minimap, etc.) that would otherwise clutter the screen behind
    // the dialogue box while it's up - hidden for the duration of Play(),
    // restored once the last line is advanced past. Populate in the Editor;
    // empty is fine if nothing needs hiding.
    [SerializeField] List<GameObject> hideWhilePlaying = new List<GameObject>();

    const string StoryTableName = "Story";

    DialogueScript current;
    int lineIndex;
    Action onComplete;

    void Awake()
    {
        if (Instance == null) Instance = this;
        if (panel != null) panel.SetActive(false);
    }

    public void Play(DialogueScript script, Action onComplete)
    {
        if (script == null || script.Lines == null || script.Lines.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }
        current = script;
        this.onComplete = onComplete;
        lineIndex = 0;
        if (panel != null) panel.SetActive(true);
        SetHiddenUIActive(false);
        ShowLine();
    }

    // Click-anywhere-to-continue button on the panel itself.
    public void Advance()
    {
        if (current == null) return;
        lineIndex++;
        if (lineIndex >= current.Lines.Count)
        {
            Action callback = onComplete;
            current = null;
            onComplete = null;
            if (panel != null) panel.SetActive(false);
            SetHiddenUIActive(true);
            callback?.Invoke();
            return;
        }
        ShowLine();
    }

    void SetHiddenUIActive(bool active)
    {
        foreach (GameObject go in hideWhilePlaying)
        {
            if (go != null) go.SetActive(active);
        }
    }

    void ShowLine()
    {
        DialogueLine line = current.Lines[lineIndex];
        if (portraitImage != null) portraitImage.sprite = line.Character != null ? line.Character.GetPortrait(line.Expression) : null;
        if (speakerNameText != null)
        {
            speakerNameText.text = string.IsNullOrEmpty(line.SpeakerNameKey)
                ? ""
                : LocalizationSettings.StringDatabase.GetLocalizedString(StoryTableName, line.SpeakerNameKey);
        }
        if (bodyText != null)
        {
            bodyText.text = string.IsNullOrEmpty(line.TextKey)
                ? ""
                : LocalizationSettings.StringDatabase.GetLocalizedString(StoryTableName, line.TextKey);
        }
    }
}
}
