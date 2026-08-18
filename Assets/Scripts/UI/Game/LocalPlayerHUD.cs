using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HR.Object.Player;

namespace HR.UI{
// Bottom-left local-player panel (Self UI) - portrait, lives, bomb count,
// bomb power, move speed. Skill icon/description are deliberately left
// alone (no data for them yet); the skill bar itself is already handled by
// SkillEnergyUI, not this script. Event-driven - CharacterBase calls
// Refresh() whenever one of these values actually changes, instead of
// re-reading all of them every frame.
public class LocalPlayerHUD : MonoBehaviour
{
    public static LocalPlayerHUD Instance;

    [SerializeField] Image portraitImage;
    [SerializeField] TMP_Text livesText;
    [SerializeField] TMP_Text bombCountText;
    [SerializeField] TMP_Text bombPowerText;
    [SerializeField] TMP_Text speedText;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    public void Refresh(CharacterBase character)
    {
        if (portraitImage != null)
        {
            Sprite portrait = character.Portrait;
            portraitImage.sprite = portrait;
            portraitImage.gameObject.SetActive(portrait != null);
        }
        if (livesText != null) livesText.text = $"❤ {character.lives}";
        if (bombCountText != null) bombCountText.text = $"{character.bombAmount} / {character.maxBombAmount}";
        if (bombPowerText != null) bombPowerText.text = character.bombPower.ToString();
        if (speedText != null) speedText.text = character.EffectiveMoveSpeed.ToString("F1");
    }
}
}
