using UnityEngine;

namespace HR.Object.Player{
[CreateAssetMenu(fileName = "Rui_HawkEye", menuName = "HoloBomber/Skill/Rui Hawk Eye")]
public class RuiHawkEyeData : SkillData
{
    [Tooltip("How long the blast-range preview stays visible after activating.")]
    public float Duration = 10f;
    [Tooltip("Seconds between recomputing which cells are highlighted - bombs can move/appear/explode during the effect, so this needs to refresh periodically rather than being computed once.")]
    public float RefreshInterval = 0.15f;
    [Tooltip("Highlight color for a bomb that was just placed (see BombBase.FuseProgress).")]
    public Color FreshFuseColor = new Color(1f, 0.85f, 0.1f);
    [Tooltip("Highlight color for a bomb about to explode.")]
    public Color AboutToExplodeColor = new Color(1f, 0.1f, 0.05f);
}
}
