using System;
using System.IO;
using UnityEngine;

/// <summary>
/// Cognito/세이브 API 서버 정보. 코드에 하드코딩하지 않고 StreamingAssets/auth-config.json에서 읽는다.
/// 실제 값이 든 auth-config.json은 .gitignore로 제외돼 있으니, 새로 받은 사람은
/// auth-config.example.json을 복사해서 auth-config.json으로 만들고 값을 채워야 한다.
/// </summary>
[Serializable]
public class AuthConfig
{
    public string region;
    public string clientId;
    public string saveApiBaseUrl;

    private const string FileName = "auth-config.json";

    public static AuthConfig Load()
    {
        string path = Path.Combine(Application.streamingAssetsPath, FileName);

        if (!File.Exists(path))
        {
            Debug.LogError($"[AuthConfig] {path} 가 없습니다. auth-config.example.json을 복사해서 값을 채워주세요.");
            return null;
        }

        return JsonUtility.FromJson<AuthConfig>(File.ReadAllText(path));
    }
}
