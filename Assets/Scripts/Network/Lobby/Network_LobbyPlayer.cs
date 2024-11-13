using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;
using UnityEngine.UI;
using Steamworks;

public class Network_LobbyPlayer : MonoBehaviour
{
    public string PlayerName;
    public int ConnectionID;
    public ulong PlayerSteamID;
    bool AvatarRecieved;

    public TMP_Text PlayerNameText;
    public RawImage PlayerIcon;
    public TMP_Text PlayerReadyText;
    public bool isReady;
    protected Callback<AvatarImageLoaded_t> ImageLoaded;

    public void ChangeReadyStatus()
    {
        if (isReady)
        {
            PlayerReadyText.text = "Ready";
            PlayerReadyText.color = Color.green;
        }
        else
        {
            PlayerReadyText.text = "Unready";
            PlayerReadyText.color = Color.red;
        }
    }

    void Start()
    {
        ImageLoaded = Callback<AvatarImageLoaded_t>.Create(OnImageLoaded);
    }
    public void SetPlayerValues()
    {
        PlayerNameText.text = PlayerName;
        ChangeReadyStatus();
        if (!AvatarRecieved)
        {
            GetPlayerIcon();
        }
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
                texture.LoadRawTextureData(image);
                texture.Apply();
            }
        }
        AvatarRecieved = true;
        return texture;
    }
}
