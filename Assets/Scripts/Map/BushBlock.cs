using System.Collections.Generic;
using Mirror;
using UnityEngine;
using HR.Network;
using HR.Object.Player;

namespace HR.Map{
// Walkable terrain - see GridCell.Bush/GridManager.IsOccupied for why it
// doesn't block movement. Conceals whoever's standing in it (see
// CharacterBase.SetHiddenInBush) and can be burned away by an explosion
// passing through (see BombBase.SideExoplosion), unlike DestructibleBlock:
// no item drop, and the blast isn't stopped by it either.
[RequireComponent(typeof(GridCell))]
public class BushBlock : NetworkBehaviour
{
    [Tooltip("Swapped in for someone on the SAME team as whoever's hiding inside - lets you see your own teammate's silhouette through it. An enemy team's clients never see this swap; for them the bush keeps its default material and the hidden character just disappears entirely (see CharacterBase.OnHiddenInBushChanged). A separate Transparent material rather than varying alpha on one material, because Unity's URP Lit shader GUI force-disables the DepthOnly/SHADOWCASTER passes the moment Surface Type is Transparent at all - that made the default (should look fully solid) state skip this project's SSAO and stop casting a shadow, and let other objects' outlines incorrectly draw through it even at alpha 1.")]
    [SerializeField] Material occupiedMaterial;
    // Exposed for PlaceholderBlockCreator, which generates this material at
    // the same time as the GameObject itself (before this ever becomes a
    // real prefab asset the Inspector could drag-and-drop onto).
    public Material OccupiedMaterial { get => occupiedMaterial; set => occupiedMaterial = value; }
    Renderer bushRenderer;
    Material defaultMaterial;

    // Tracked so Break() can reveal anyone still inside - the same blast
    // that breaks this bush will very likely also hit them, but not
    // guaranteed (e.g. they've got a mount absorbing the hit), and leaving
    // them hidden inside terrain that no longer exists would be a bug.
    readonly List<CharacterBase> hiddenCharacters = new();
    // One entry per currently-hiding character (duplicates allowed - two
    // teammates hiding at once shouldn't un-fade the bush the moment just
    // one of them leaves). Replicated so every client can decide FOR
    // ITSELF whether to render this bush transparent, based on whether its
    // own local player's team shows up in here - a plain synced bool can't
    // express "transparent only to your own team", since every peer would
    // otherwise be forced to see the exact same state.
    readonly SyncList<int> occupantTeamIds = new();

    void Awake()
    {
        bushRenderer = GetComponent<Renderer>();
        defaultMaterial = bushRenderer.sharedMaterial;
    }

    void OnEnable()
    {
        occupantTeamIds.OnChange += OnOccupantTeamsChanged;
        // Covers this bush itself spawning in after I'm already connected.
        // Does NOT cover me joining late into a match where this bush
        // already has occupants and nobody enters/exits afterward - a
        // known, narrow gap (nothing currently re-syncs bush state to a
        // freshly spawned player).
        RefreshVisual();
    }

    void OnDisable()
    {
        occupantTeamIds.OnChange -= OnOccupantTeamsChanged;
    }

    void OnOccupantTeamsChanged(SyncList<int>.Operation op, int index, int item)
    {
        RefreshVisual();
    }

    [ServerCallback]
    void OnTriggerEnter(Collider other)
    {
        CharacterBase character = other.GetComponentInParent<CharacterBase>();
        if (character == null || hiddenCharacters.Contains(character)) return;

        hiddenCharacters.Add(character);
        character.SetHiddenInBush(true);
        occupantTeamIds.Add(character.TeamID);
    }

    [ServerCallback]
    void OnTriggerExit(Collider other)
    {
        CharacterBase character = other.GetComponentInParent<CharacterBase>();
        if (character == null) return;

        hiddenCharacters.Remove(character);
        character.SetHiddenInBush(false);
        occupantTeamIds.Remove(character.TeamID); // removes just one matching entry
    }

    // Re-evaluated from scratch (not incrementally) whenever occupantTeamIds
    // changes - cheap enough for how rarely that happens, and avoids having
    // to reason about add/remove ordering vs. my own team's count.
    void RefreshVisual()
    {
        if (bushRenderer == null) return;
        bool showOccupied = occupiedMaterial != null && IsAllyOccupant();
        bushRenderer.sharedMaterial = showOccupied ? occupiedMaterial : defaultMaterial;
    }

    bool IsAllyOccupant()
    {
        Network_Manager manager = Network_Manager.singleton as Network_Manager;
        if (manager == null) return false;

        CharacterBase localCharacter = manager.Player_List.Find(p => p != null && p.isLocalPlayer);
        return localCharacter != null && occupantTeamIds.Contains(localCharacter.TeamID);
    }

    [Server]
    public void Break()
    {
        foreach (CharacterBase character in hiddenCharacters)
        {
            if (character != null) character.SetHiddenInBush(false);
        }
        hiddenCharacters.Clear();
        occupantTeamIds.Clear();

        // Same "don't rely solely on NetworkServer.Destroy's scene-object
        // handling" immediate cleanup DestructibleBlock.Break() does.
        foreach (Collider col in GetComponentsInChildren<Collider>())
        {
            col.enabled = false;
        }
        foreach (Renderer rend in GetComponentsInChildren<Renderer>())
        {
            rend.enabled = false;
        }

        NetworkServer.Destroy(gameObject);
    }
}
}
