using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using Steamworks;

namespace HR.Network.Lobby{
public class LobbyPlayerSlot : MonoBehaviour
{
    public string PlayerName;
    public int ConnectionID;
    public ulong PlayerSteamID;
    public int TeamID;
    bool AvatarRecieved;

    public TMP_Text PlayerNameText;
    public RawImage PlayerIcon;
    [SerializeField] Image Background;
    public bool isReady;
    protected Callback<AvatarImageLoaded_t> ImageLoaded;

    // Slot 1..8 -> White/Red/Orange/Yellow/Green/Blue/Purple/Pink
    static readonly Color[] SlotColors = {
        Color.white,
        Color.red,
        new Color(1f, 0.5f, 0f),
        Color.yellow,
        Color.green,
        Color.blue,
        new Color(0.5f, 0f, 0.5f),
        new Color(1f, 0.4f, 0.7f),
    };

    // Called by LobbyController with the real player behind this slot.
    // Resets the cached avatar flag when a different player takes over this
    // slot (Ready no longer matches ConnectionID), so the new player's Steam
    // avatar actually gets re-fetched instead of keeping the old one.
    public void SetPlayer(PlayerObject player)
    {
        if (ConnectionID != player.ConnectionID)
        {
            AvatarRecieved = false;
        }
        PlayerName = player.PlayerName;
        ConnectionID = player.ConnectionID;
        PlayerSteamID = player.PlayerSteamID;
        TeamID = player.TeamID;
        isReady = player.Ready;
        SetPlayerValues();
    }
    // Slot has no player in it (fewer than 8 people in the room right now).
    public void SetEmpty()
    {
        PlayerName = "";
        ConnectionID = -1;
        PlayerSteamID = 0;
        TeamID = 0;
        isReady = false;
        AvatarRecieved = false;
        if (PlayerNameText != null)
        {
            PlayerNameText.text = "";
            PlayerNameText.color = Color.gray;
        }
        if (Background != null) Background.color = Color.gray;
        if (PlayerIcon != null)
        {
            PlayerIcon.texture = null;
            PlayerIcon.gameObject.SetActive(false);
        }
    }

    void Start()
    {
        ImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
    }
    public void SetPlayerValues()
    {
        PlayerNameText.text = PlayerName;
        if (PlayerIcon != null) PlayerIcon.gameObject.SetActive(true);
        SetSlotColor();
        if (!AvatarRecieved)
        {
            GetPlayerIcon();
        }
    }
    // Only the name text follows the player's picked team color - Background
    // is a plain panel, not tied to team color (only greyed out when empty).
    void SetSlotColor()
    {
        if (TeamID < 1 || TeamID > SlotColors.Length) return;
        if (PlayerNameText != null) PlayerNameText.color = SlotColors[TeamID - 1];
    }
    void GetPlayerIcon()
    {
        int ImageID = SteamFriends.GetLargeFriendAvatar((CSteamID)PlayerSteamID);
        if (ImageID == -1) return;
        PlayerIcon.texture = GetSteamImageAsTexture(ImageID);
    }
    void OnImageLoaded(AvatarImageLoaded_t callback)
    {
        if (callback.m_steamID.m_SteamID == PlayerSteamID)
        {
            PlayerIcon.texture = GetSteamImageAsTexture(callback.m_iImage);
        }
        else
        {
            return;
        }
    }

    Texture2D GetSteamImageAsTexture(int iImage)
    {
        Texture2D texture = null;

        bool isValid = SteamUtils.GetImageSize(iImage, out uint width, out uint height);
        if (isValid)
        {
            byte[] image = new byte[width * height * 4];

            isValid = SteamUtils.GetImageRGBA(iImage, image, (int)(width * height * 4));

            if (isValid)
            {
                texture = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false, true);
                texture.LoadRawTextureData(FlipImageVertically(image, width, height));
                texture.Apply();
            }
        }
        AvatarRecieved = true;
        return texture;
    }
    // Steam's GetImageRGBA returns rows bottom-to-top (OpenGL convention),
    // but Texture2D.LoadRawTextureData expects top-to-bottom - without this
    // flip the avatar renders upside down.
    static byte[] FlipImageVertically(byte[] image, uint width, uint height)
    {
        int rowBytes = (int)width * 4;
        byte[] flipped = new byte[image.Length];
        for (int y = 0; y < height; y++)
        {
            int srcOffset = y * rowBytes;
            int dstOffset = ((int)height - 1 - y) * rowBytes;
            System.Array.Copy(image, srcOffset, flipped, dstOffset, rowBytes);
        }
        return flipped;
    }
}

}
