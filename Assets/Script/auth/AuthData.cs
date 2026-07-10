using UnityEngine;

/// <summary>
/// 로그인 세션(refresh token)을 PlayerPrefs에 저장한다. SettingsData와 동일한 패턴.
/// PlayerPrefs는 암호화되지 않지만, 단일 플레이어 학생 프로젝트의 위협 모델상 허용 가능한 수준으로 판단.
/// </summary>
public static class AuthData
{
    private const string RefreshTokenKey = "auth.refreshToken";

    public static string RefreshToken
    {
        get => PlayerPrefs.GetString(RefreshTokenKey, string.Empty);
        set
        {
            PlayerPrefs.SetString(RefreshTokenKey, value);
            PlayerPrefs.Save();
        }
    }

    public static bool HasSession => !string.IsNullOrEmpty(RefreshToken);

    public static void Clear()
    {
        PlayerPrefs.DeleteKey(RefreshTokenKey);
        PlayerPrefs.Save();
    }
}
