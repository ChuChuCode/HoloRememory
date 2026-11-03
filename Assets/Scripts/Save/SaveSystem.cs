using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
#if !DISABLESTEAMWORKS
using Steamworks;
#endif

namespace HR.Save{
// Lives on the same persistent GameObject as InputComponent
// (Assets/Prefabs/InputSystem.prefab, placed in Main_Scene) - that prefab's
// InputComponent.Awake() already calls DontDestroyOnLoad(this), which
// applies to the whole GameObject, so SaveSystem survives every scene
// change for free. Called again here anyway so this component doesn't
// silently depend on InputComponent staying on the same object forever.
//
// This currency is local/client-trusted by design (confirmed with the
// project owner) - it only unlocks convenience/cosmetic content (playable
// characters now, collectibles later), not anything that affects match
// fairness, so there's no server-authoritative validation here. It is
// unrelated to CharacterBase.ownMoney, which is a separate (currently
// unused) in-match/round currency stub. The save file is AES-encrypted
// (see Encrypt/Decrypt below) purely so it isn't a plaintext JSON a player
// can casually open in a text editor and hand-edit - a hardcoded key is not
// real protection against someone willing to decompile the build, same
// trust tradeoff as everything else here, just not obviously editable.
public class SaveSystem : MonoBehaviour
{
    public static SaveSystem Instance { get; private set; }

    public static event Action<int> OnCurrencyChanged;

    const string FileName = "save.dat";

    public SaveData Data { get; private set; }

    string LocalPath => Path.Combine(Application.persistentDataPath, FileName);

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    void Load()
    {
        byte[] bytes = ReadCloud() ?? ReadLocal();
        string json = bytes != null ? Decrypt(bytes) : null;
        Data = string.IsNullOrEmpty(json) ? new SaveData() : JsonConvert.DeserializeObject<SaveData>(json) ?? new SaveData();
    }

    public void Save()
    {
        byte[] bytes = Encrypt(JsonConvert.SerializeObject(Data));
        WriteLocal(bytes);
        WriteCloud(bytes);
    }

    public void AddCurrency(int amount)
    {
        if (amount <= 0) return;
        Data.Currency += amount;
        Save();
        OnCurrencyChanged?.Invoke(Data.Currency);
    }

    // No partial spend - either the full amount comes off, or nothing does.
    public bool SpendCurrency(int amount)
    {
        if (amount <= 0 || Data.Currency < amount) return false;
        Data.Currency -= amount;
        Save();
        OnCurrencyChanged?.Invoke(Data.Currency);
        return true;
    }

    public void Unlock(string id)
    {
        if (Data.UnlockedIds.Contains(id)) return;
        Data.UnlockedIds.Add(id);
        Save();
    }

    public bool IsUnlocked(string id) => Data.UnlockedIds.Contains(id);

    byte[] ReadLocal()
    {
        try
        {
            return File.Exists(LocalPath) ? File.ReadAllBytes(LocalPath) : null;
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveSystem] Failed to read local save: {e}");
            return null;
        }
    }

    void WriteLocal(byte[] bytes)
    {
        try
        {
            File.WriteAllBytes(LocalPath, bytes);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[SaveSystem] Failed to write local save: {e}");
        }
    }

    // Steam Cloud mirrors the exact same encrypted blob as the local file -
    // returns null whenever Steam isn't initialized or no cloud file exists
    // yet, so Load() falls back to the local copy.
    byte[] ReadCloud()
    {
#if !DISABLESTEAMWORKS
        if (!SteamManager.Initialized || !SteamRemoteStorage.FileExists(FileName)) return null;
        int size = SteamRemoteStorage.GetFileSize(FileName);
        if (size <= 0) return null;
        byte[] buffer = new byte[size];
        int read = SteamRemoteStorage.FileRead(FileName, buffer, size);
        return read > 0 ? buffer : null;
#else
        return null;
#endif
    }

    void WriteCloud(byte[] bytes)
    {
#if !DISABLESTEAMWORKS
        if (!SteamManager.Initialized) return;
        SteamRemoteStorage.FileWrite(FileName, bytes, bytes.Length);
#endif
    }

    // Obfuscation, not real security - see class comment above.
    static readonly byte[] Key = { 0xa8, 0x15, 0xa6, 0x27, 0x6d, 0xdd, 0xbd, 0x9d, 0xe9, 0xd3, 0xc2, 0x28, 0x0a, 0xe1, 0xc3, 0x73, 0x4c, 0x7e, 0x3d, 0x32, 0x99, 0x52, 0x11, 0xe9, 0x4b, 0xc0, 0x26, 0x3d, 0x3a, 0x39, 0x05, 0xac };
    static readonly byte[] IV = { 0x23, 0x27, 0x49, 0x55, 0xca, 0xb0, 0x4d, 0xaa, 0x35, 0xa6, 0x2c, 0xe3, 0xca, 0xf3, 0xc8, 0x94 };

    static byte[] Encrypt(string plainText)
    {
        using Aes aes = Aes.Create();
        aes.Key = Key;
        aes.IV = IV;
        using ICryptoTransform encryptor = aes.CreateEncryptor();
        byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
        return encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
    }

    static string Decrypt(byte[] cipherBytes)
    {
        try
        {
            using Aes aes = Aes.Create();
            aes.Key = Key;
            aes.IV = IV;
            using ICryptoTransform decryptor = aes.CreateDecryptor();
            byte[] plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);
            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (Exception e)
        {
            // Corrupt/tampered/pre-encryption save - fall back to a fresh one
            // rather than crashing on load.
            Debug.LogWarning($"[SaveSystem] Failed to decrypt save, starting fresh: {e}");
            return null;
        }
    }
}
}
