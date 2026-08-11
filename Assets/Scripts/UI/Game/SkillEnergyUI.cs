using UnityEngine;

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

    void Awake()
    {
        if (instance == null) instance = this;
    }

    public void UpdateSkillEnergy(float current, float max)
    {
        if (EnergyBar == null) return;
        EnergyBar.SetMaxValue(Mathf.RoundToInt(max));
        EnergyBar.SetValue(Mathf.RoundToInt(current));
    }
}
}
