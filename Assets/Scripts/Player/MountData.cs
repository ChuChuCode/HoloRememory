using UnityEngine;

namespace HR.Object.Player{
// Data for a rideable mount (Coo/Louie-style: chick, turtles, UFOs, tank).
// A mount absorbs one hit - see CharacterBase.HealthDamage/Dismount - it
// never touches health/lives directly.
[CreateAssetMenu(fileName = "NewMountData", menuName = "HoloBomber/Mount Data")]
public class MountData : ScriptableObject
{
    public string MountName;
    [Tooltip("Added to the rider's own moveSpeed while mounted. Negative for a slow mount, positive for a fast one, 0 for neutral.")]
    public float MoveSpeedModifier = 0f;
    [Tooltip("UFO variants can't pick up items while ridden.")]
    public bool CanPickupItems = true;
    [Tooltip("Placeholder mount visual (see CharacterBase.RpcShowMountVisual) is tinted this color so the 6 mounts are at least tellable apart until real models/animations exist.")]
    public Color VisualColor = Color.white;
    [Tooltip("Placeholder shape for this specific mount (each of the 6 gets its own - chick/turtle/UFO/tank should at least look different from each other, not just be recolored). Replace with a real model/prefab later.")]
    public GameObject VisualPrefab;
}
}
