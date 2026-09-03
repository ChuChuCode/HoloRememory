using UnityEngine;
using TMPro;

namespace HR.UI{
// Minimal, purpose-built HUD hook for the local player's own Skill Energy
// bar. Deliberately its own small script instead of piggybacking on
// MainInfoUI - that's an old MOBA-era panel that isn't actually placed
// anywhere in the current scene, and its other fields (HP/Money/Level)
// aren't null-checked, so partially wiring just one field on it would risk
// a NullReferenceException the moment it got activated.
public class SkillEnergyUI : MonoBehaviour
{
    public static SkillEnergyUI instance;
    [SerializeField] Bar EnergyBar;
    [SerializeField] TMP_Text nameText;
    [SerializeField] TMP_Text descriptionText;

    void Awake()
    {
        if (instance == null) instance = this;
    }

    // Call once, up front (max never changes mid-match) - SetMaxValue also
    // resets the slider's current position, so calling it again on every
    // regen tick would fight UpdateSkillEnergy's own animation.
    public void InitSkillEnergy(float current, float max)
    {
        if (EnergyBar == null) return;
        EnergyBar.SetMaxValue(Mathf.RoundToInt(max));
        EnergyBar.SetValue(current);
    }

    // Call on every change (regen tick, activation) - value only, no max.
    public void UpdateSkillEnergy(float current)
    {
        if (EnergyBar == null) return;
        EnergyBar.SetValue(current);
    }

    // Set once (the local player's own skill never changes mid-match) -
    // not tied to the energy SyncVar hook like UpdateSkillEnergy is.
    public void SetSkillInfo(string skillName, string description)
    {
        if (nameText != null) nameText.text = skillName;
        if (descriptionText != null) descriptionText.text = description;
    }
}
}
