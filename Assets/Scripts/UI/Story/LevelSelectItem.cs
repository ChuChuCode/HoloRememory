using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Localization.Settings;
using HR.Story;
using HR.Save;

namespace HR.UI{
// One row in LevelSelectPanel's list - mirrors the lock/unlock-button
// pattern sketched for character unlocking (never built) in the earlier
// Economy plan; this is SaveSystem's first real consumer.
public class LevelSelectItem : MonoBehaviour
{
    const string StoryTableName = "Story";

    [SerializeField] TMP_Text nameText;
    [SerializeField] Button selectButton;
    [SerializeField] GameObject lockedOverlay;
    [SerializeField] TMP_Text costText;
    [SerializeField] Button unlockButton;

    LevelConfig level;
    LevelSelectPanel owner;

    public void SetLevel(LevelConfig level, LevelSelectPanel owner)
    {
        this.level = level;
        this.owner = owner;
        Refresh();
    }

    public void Refresh()
    {
        if (level == null) return;
        if (nameText != null)
        {
            nameText.text = string.IsNullOrEmpty(level.DisplayNameKey)
                ? level.LevelId
                : LocalizationSettings.StringDatabase.GetLocalizedString(StoryTableName, level.DisplayNameKey);
        }

        bool unlocked = level.UnlockCost <= 0 || (SaveSystem.Instance != null && SaveSystem.Instance.IsUnlocked($"level:{level.LevelId}"));
        if (selectButton != null) selectButton.interactable = unlocked;
        if (lockedOverlay != null) lockedOverlay.SetActive(!unlocked);
        if (unlockButton != null) unlockButton.gameObject.SetActive(!unlocked);
        if (costText != null) costText.text = level.UnlockCost.ToString();
    }

    // Select Button - only interactable when unlocked (see Refresh above).
    public void Select()
    {
        owner?.SelectLevel(level);
    }

    // Unlock Button - shown only while locked.
    public void Unlock()
    {
        if (SaveSystem.Instance == null) return;
        if (SaveSystem.Instance.SpendCurrency(level.UnlockCost))
        {
            SaveSystem.Instance.Unlock($"level:{level.LevelId}");
            Refresh();
        }
    }
}
}
