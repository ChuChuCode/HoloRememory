using System;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace HR.GUI{
// Hand-rolled Google service-account OAuth2 flow (JWT-bearer grant) - no
// Google.Apis.* package. That official .NET client library pulls in a
// chain of NuGet dependencies Unity has no native way to install (would
// need NuGetForUnity or manual DLL wrangling, with real Mono-compatibility
// risk) for what's otherwise two small HTTP calls. This still talks to the
// real, official Google OAuth2/Sheets REST endpoints - it just skips the
// wrapper library.
public static class GoogleSheetsAuth
{
    // Project root (sibling of Assets/, Library/, etc.) - deliberately NOT
    // under Assets/, so Unity never imports this as an asset (no .meta, no
    // accidental inclusion in a build). Gitignored - see .gitignore.
    const string CredentialsFileName = "google-sheets-credentials.json";
    // Read-write scope - needed for LocalizationGoogleSheetExport to push
    // Table content back up, not just for LocalizationGoogleSheetImport's
    // reads. The service account also needs Editor (not just Viewer) access
    // on the Sheet itself for a write to actually succeed.
    const string Scope = "https://www.googleapis.com/auth/spreadsheets";

    static readonly HttpClient Http = new HttpClient();

    class ServiceAccountCredentials
    {
        [JsonProperty("client_email")] public string ClientEmail;
        [JsonProperty("private_key")] public string PrivateKey;
        [JsonProperty("token_uri")] public string TokenUri;
    }

    static string CredentialsPath => Path.Combine(Path.GetDirectoryName(Application.dataPath), CredentialsFileName);

    // Returns null (and logs a clear error) instead of throwing - every
    // call site here is a one-shot Editor menu command, not something that
    // benefits from an uncaught exception's stack trace over a plain
    // Console message pointing at what to fix.
    public static string GetAccessToken()
    {
        if (!File.Exists(CredentialsPath))
        {
            Debug.LogError($"Google Sheets credentials not found at '{CredentialsPath}'. " +
                "Create a Service Account JSON key in Google Cloud Console, share the Sheet with its " +
                "client_email as Viewer, and save the downloaded file at that path (project root, not under Assets/).");
            return null;
        }

        ServiceAccountCredentials credentials;
        try
        {
            credentials = JsonConvert.DeserializeObject<ServiceAccountCredentials>(File.ReadAllText(CredentialsPath));
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to parse '{CredentialsFileName}': {e.Message}");
            return null;
        }
        if (credentials == null || string.IsNullOrEmpty(credentials.ClientEmail) || string.IsNullOrEmpty(credentials.PrivateKey) || string.IsNullOrEmpty(credentials.TokenUri))
        {
            Debug.LogError($"'{CredentialsFileName}' is missing client_email/private_key/token_uri - is this a valid service account key file?");
            return null;
        }

        string jwt;
        try
        {
            jwt = BuildSignedJwt(credentials);
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to build/sign the auth JWT: {e.Message}");
            return null;
        }

        try
        {
            var content = new FormUrlEncodedContent(new[]
            {
                new System.Collections.Generic.KeyValuePair<string, string>("grant_type", "urn:ietf:params:oauth:grant-type:jwt-bearer"),
                new System.Collections.Generic.KeyValuePair<string, string>("assertion", jwt),
            });
            HttpResponseMessage response = Http.PostAsync(credentials.TokenUri, content).GetAwaiter().GetResult();
            string body = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            if (!response.IsSuccessStatusCode)
            {
                Debug.LogError($"Google token endpoint returned {(int)response.StatusCode}: {body}");
                return null;
            }

            JObject json = JObject.Parse(body);
            string accessToken = json["access_token"]?.ToString();
            if (string.IsNullOrEmpty(accessToken))
            {
                Debug.LogError($"Token response had no access_token: {body}");
                return null;
            }
            return accessToken;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to fetch an access token: {e.Message}");
            return null;
        }
    }

    static string BuildSignedJwt(ServiceAccountCredentials credentials)
    {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        string header = Base64UrlEncode(Encoding.UTF8.GetBytes("{\"alg\":\"RS256\",\"typ\":\"JWT\"}"));
        string claims = Base64UrlEncode(Encoding.UTF8.GetBytes(
            "{\"iss\":\"" + credentials.ClientEmail + "\"," +
            "\"scope\":\"" + Scope + "\"," +
            "\"aud\":\"" + credentials.TokenUri + "\"," +
            "\"iat\":" + now + "," +
            "\"exp\":" + (now + 3600) + "}"));
        string unsigned = header + "." + claims;

        using RSA rsa = RSA.Create();
        // Mono's RSA implementation (at least on macOS, in the Editor)
        // throws PlatformNotSupportedException from ImportPkcs8PrivateKey -
        // that convenience API is a newer (.NET Core 3.0+) addition that
        // isn't implemented everywhere. ImportParameters is the much older,
        // universally-supported API, so we parse the PKCS#8 DER ourselves
        // to get there.
        rsa.ImportParameters(Pkcs8RsaKey.Parse(DecodePemBody(credentials.PrivateKey)));
        byte[] signature = rsa.SignData(Encoding.UTF8.GetBytes(unsigned), HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        return unsigned + "." + Base64UrlEncode(signature);
    }

    // Strips the "-----BEGIN PRIVATE KEY-----"/"-----END PRIVATE KEY-----"
    // lines and decodes the remaining Base64 body to raw PKCS#8 DER bytes -
    // avoids relying on RSA.ImportFromPem (a newer convenience wrapper) in
    // favor of the lower-level, more broadly-supported ImportPkcs8PrivateKey.
    static byte[] DecodePemBody(string pem)
    {
        string base64 = pem
            .Replace("-----BEGIN PRIVATE KEY-----", "")
            .Replace("-----END PRIVATE KEY-----", "")
            .Replace("\r", "")
            .Replace("\n", "")
            .Trim();
        return Convert.FromBase64String(base64);
    }

    static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }
}

// Minimal ASN.1 DER reader - just enough to pull the RSA key components out
// of a PKCS#8 PrivateKeyInfo blob (which itself wraps a PKCS#1
// RSAPrivateKey), so GoogleSheetsAuth can hand them to RSA.ImportParameters
// instead of the not-fully-portable RSA.ImportPkcs8PrivateKey.
static class Pkcs8RsaKey
{
    public static RSAParameters Parse(byte[] pkcs8Der)
    {
        int pos = 0;
        ReadSequenceHeader(pkcs8Der, ref pos); // PrivateKeyInfo
        ReadInteger(pkcs8Der, ref pos); // version
        SkipTlv(pkcs8Der, ref pos); // AlgorithmIdentifier
        byte[] rsaKeyDer = ReadOctetString(pkcs8Der, ref pos);

        int kp = 0;
        ReadSequenceHeader(rsaKeyDer, ref kp); // RSAPrivateKey
        ReadInteger(rsaKeyDer, ref kp); // version
        byte[] n = ReadInteger(rsaKeyDer, ref kp);
        byte[] e = ReadInteger(rsaKeyDer, ref kp);
        byte[] d = ReadInteger(rsaKeyDer, ref kp);
        byte[] p = ReadInteger(rsaKeyDer, ref kp);
        byte[] q = ReadInteger(rsaKeyDer, ref kp);
        byte[] dp = ReadInteger(rsaKeyDer, ref kp);
        byte[] dq = ReadInteger(rsaKeyDer, ref kp);
        byte[] iq = ReadInteger(rsaKeyDer, ref kp);

        int modulusSize = n.Length;
        int halfSize = (modulusSize + 1) / 2;
        return new RSAParameters
        {
            Modulus = n,
            Exponent = e,
            D = Pad(d, modulusSize),
            P = Pad(p, halfSize),
            Q = Pad(q, halfSize),
            DP = Pad(dp, halfSize),
            DQ = Pad(dq, halfSize),
            InverseQ = Pad(iq, halfSize),
        };
    }

    static byte[] Pad(byte[] value, int length)
    {
        if (value.Length == length) return value;
        var padded = new byte[length];
        Array.Copy(value, 0, padded, length - value.Length, value.Length);
        return padded;
    }

    static void ReadSequenceHeader(byte[] data, ref int pos)
    {
        ExpectTag(data, ref pos, 0x30);
        ReadLength(data, ref pos);
    }

    static byte[] ReadOctetString(byte[] data, ref int pos)
    {
        ExpectTag(data, ref pos, 0x04);
        int len = ReadLength(data, ref pos);
        var value = new byte[len];
        Array.Copy(data, pos, value, 0, len);
        pos += len;
        return value;
    }

    static byte[] ReadInteger(byte[] data, ref int pos)
    {
        ExpectTag(data, ref pos, 0x02);
        int len = ReadLength(data, ref pos);
        int start = pos;
        pos += len;
        // Strip the leading 0x00 sign byte DER adds whenever the value's
        // high bit would otherwise read as negative - RSAParameters wants
        // the raw unsigned magnitude.
        if (len > 1 && data[start] == 0x00 && (data[start + 1] & 0x80) != 0)
        {
            start++;
            len--;
        }
        var value = new byte[len];
        Array.Copy(data, start, value, 0, len);
        return value;
    }

    static void SkipTlv(byte[] data, ref int pos)
    {
        pos++; // tag
        int len = ReadLength(data, ref pos);
        pos += len;
    }

    static void ExpectTag(byte[] data, ref int pos, byte expected)
    {
        if (data[pos] != expected)
            throw new FormatException($"Expected ASN.1 tag 0x{expected:X2} at offset {pos}, found 0x{data[pos]:X2}");
        pos++;
    }

    static int ReadLength(byte[] data, ref int pos)
    {
        int first = data[pos++];
        if ((first & 0x80) == 0) return first;
        int numBytes = first & 0x7F;
        int len = 0;
        for (int i = 0; i < numBytes; i++) len = (len << 8) | data[pos++];
        return len;
    }
}
}
