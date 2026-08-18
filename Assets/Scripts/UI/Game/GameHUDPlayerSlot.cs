using TMPro;
using UnityEngine;
using UnityEngine.UI;
using HR.Object.Player;

namespace HR.UI{
// One of the 8 slots in the top player bar - name, portrait, lives. Kept
// dumb on purpose (just setters, no polling of its own) - GameHUDManager
// decides what each slot should show every frame.
public class GameHUDPlayerSlot : MonoBehaviour
{
    [SerializeField] TMP_Text nameText;
    [SerializeField] Image portraitImage;
    [SerializeField] TMP_Text livesText;

    public void SetPlayer(CharacterBase character)
    {
        if (nameText != null)
        {
            nameText.text = character.PlayerName;
            nameText.color = Color.white;
        }
        if (portraitImage != null)
        {
            Sprite portrait = character.Portrait;
            portraitImage.sprite = portrait;
            portraitImage.gameObject.SetActive(portrait != null);
        }
        if (livesText != null)
        {
            livesText.text = $"❤ {character.lives}";
            // Eliminated players stay visible (name/portrait) - only the
            // lives readout greys out, so an eliminated slot still reads
            // differently from one nobody ever joined (see SetEmpty).
            livesText.color = character.isDead ? Color.gray : Color.white;
        }
    }
    // Slot has no player in it (fewer than 8 people in this match). Same
    // "grey out, hide portrait" convention as LobbyPlayerSlot.SetEmpty, so
    // the two screens read consistently.
    public void SetEmpty()
    {
        if (nameText != null)
        {
            nameText.text = "";
            nameText.color = Color.gray;
        }
        if (portraitImage != null)
        {
            portraitImage.sprite = null;
            portraitImage.gameObject.SetActive(false);
        }
        if (livesText != null) livesText.text = "";
    }
}
}
