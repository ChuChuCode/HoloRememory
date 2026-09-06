using UnityEngine;
using UnityEngine.UI;
using TMPro;
using HR.Object.Player;
using HR.Network;

namespace HR.Network.Select{
// Lobby-only preview panel - shows the clicked character's portrait/name
// plus their skill's icon/name/description. Reads entirely from
// CharacterSelectComponent/SkillData (data assets), not a live
// CharacterSkillBase, since no character prefab is actually spawned in
// the Lobby to read one from.
public class CharacterDetailPanel : MonoBehaviour
{
    public static CharacterDetailPanel Instance;

    [SerializeField] Image characterImage;
    [SerializeField] TMP_Text characterNameText;
    [SerializeField] Image skillIconImage;
    [SerializeField] TMP_Text skillNameText;
    [SerializeField] TMP_Text skillDescriptionText;
    // SP cost to activate - just the number, not paired against bar size.
    [SerializeField] TMP_Text skillCostText;

    void Awake()
    {
        if (Instance == null) Instance = this;
    }

    // Called by CharacterSelectItem.Select_Character() when a character
    // button is clicked.
    public void ShowCharacter(int characterId)
    {
        Network_Manager manager = Network_Manager.singleton as Network_Manager;
        if (manager == null) return;

        CharacterSelectComponent component = manager.characterSelectComponentsList.Find(c => c.ID == characterId);
        if (component == null) return;

        if (characterImage != null) characterImage.sprite = component.CharacterImage;
        if (characterNameText != null) characterNameText.text = component.CharacterName;

        SkillData skill = component.Skill;
        if (skillIconImage != null) skillIconImage.sprite = skill != null ? skill.SkillIcon : null;
        if (skillNameText != null) skillNameText.text = skill != null ? skill.SkillName : "";
        if (skillDescriptionText != null) skillDescriptionText.text = skill != null ? skill.Description : "";
        if (skillCostText != null)
        {
            skillCostText.text = skill != null ? Mathf.RoundToInt(skill.Cost).ToString() : "";
        }
    }
}
}
