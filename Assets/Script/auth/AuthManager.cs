using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 로그인 세션을 관리하는 씬 전환 생존 싱글턴. PopupManager와 동일한 DontDestroyOnLoad 패턴.
/// 토큰은 메모리에만 보관하고, 재로그인을 위한 refresh token만 AuthData(PlayerPrefs)에 남긴다.
/// </summary>
public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    private AuthConfig _config;
    private string region => _config.region;
    private string clientId => _config.clientId;

    private readonly CognitoAuthClient _client = new();
    private DateTime _accessTokenExpiresAtUtc;

    public string AccessToken { get; private set; }
    public string IdToken { get; private set; }
    public bool IsLoggedIn => !string.IsNullOrEmpty(AccessToken);
    public string SaveApiBaseUrl => _config.saveApiBaseUrl;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        _config = AuthConfig.Load();
        if (_config == null)
        {
            enabled = false;
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void SignUp(string username, string password, Action<bool, string> onComplete)
        => StartCoroutine(SignUpRoutine(username, password, onComplete));

    public void Login(string username, string password, Action<bool, string> onComplete)
        => StartCoroutine(LoginRoutine(username, password, onComplete));

    /// <summary>저장된 refresh token으로 조용히 재로그인을 시도한다. 앱 시작 시 사용.</summary>
    public void TryAutoLogin(Action<bool> onComplete)
        => StartCoroutine(TryAutoLoginRoutine(onComplete));

    private IEnumerator TryAutoLoginRoutine(Action<bool> onComplete)
    {
        if (!AuthData.HasSession)
        {
            // 세션이 없어도 반드시 코루틴으로 한 프레임 미룬다.
            // 여기서 바로 콜백을 부르면 AuthGateScreen.OnEnter() -> SetRoot() 호출이
            // 아직 끝나지 않은 ScreenManager.Push() 안으로 재진입해서 스택이 꼬인다
            // (그 결과 최초 1회에 한해 로그인 화면이 안 꺼지고 남는 버그가 있었다).
            yield return null;
            onComplete?.Invoke(false);
            yield break;
        }

        yield return RefreshRoutine(onComplete);
    }

    /// <summary>액세스 토큰이 곧 만료되면 미리 갱신한다. SaveDataClient가 API 호출 전에 사용.</summary>
    public void EnsureFreshAccessToken(Action<bool> onReady)
    {
        if (IsLoggedIn && DateTime.UtcNow < _accessTokenExpiresAtUtc)
        {
            onReady?.Invoke(true);
            return;
        }
        TryAutoLogin(onReady);
    }

    public void Logout()
    {
        AccessToken = null;
        IdToken = null;
        AuthData.Clear();
    }

    private IEnumerator SignUpRoutine(string username, string password, Action<bool, string> onComplete)
    {
        // Cognito User Pool이 email 속성을 요구하도록 설정돼 있어서, 화면에서는 안 받는 대신
        // 아이디 기반으로 값을 만들어 채운다. 실제로 발송/사용되지 않는다.
        string placeholderEmail = $"{username}@noemail.local";

        CognitoAuthResult signUpResult = null;
        yield return _client.SignUp(region, clientId, username, password, placeholderEmail, r => signUpResult = r);

        if (signUpResult == null || !signUpResult.Success)
        {
            onComplete?.Invoke(false, signUpResult?.Error ?? "회원가입에 실패했습니다.");
            yield break;
        }

        // SignUp은 토큰을 주지 않으므로, 성공 즉시 로그인해서 "가입 = 자동 로그인"을 만족시킨다.
        yield return LoginRoutine(username, password, onComplete);
    }

    private IEnumerator LoginRoutine(string username, string password, Action<bool, string> onComplete)
    {
        CognitoAuthResult result = null;
        yield return _client.InitiateAuthPassword(region, clientId, username, password, r => result = r);

        if (result == null || !result.Success)
        {
            onComplete?.Invoke(false, result?.Error ?? "로그인에 실패했습니다.");
            yield break;
        }

        ApplyAuthResult(result);
        onComplete?.Invoke(true, null);
    }

    private IEnumerator RefreshRoutine(Action<bool> onComplete)
    {
        CognitoAuthResult result = null;
        yield return _client.InitiateAuthRefresh(region, clientId, AuthData.RefreshToken, r => result = r);

        if (result == null || !result.Success)
        {
            AuthData.Clear();
            onComplete?.Invoke(false);
            yield break;
        }

        ApplyAuthResult(result);
        onComplete?.Invoke(true);
    }

    private void ApplyAuthResult(CognitoAuthResult result)
    {
        AccessToken = result.AccessToken;
        IdToken = result.IdToken;
        _accessTokenExpiresAtUtc = DateTime.UtcNow.AddSeconds(result.ExpiresIn - 30);

        // REFRESH_TOKEN_AUTH 응답에는 새 refresh token이 안 올 수 있다 — 그 경우 기존 걸 유지.
        if (!string.IsNullOrEmpty(result.RefreshToken))
            AuthData.RefreshToken = result.RefreshToken;
    }

    [ContextMenu("Debug: Test SignUp (claudetest99)")]
    private void DebugTestSignUp()
    {
        SignUp("claudetest99", "Test1234!", (ok, error) =>
            Debug.Log(ok ? $"[AuthManager] SignUp+Login 성공. AccessToken={AccessToken?.Substring(0, 12)}..." : $"[AuthManager] SignUp 실패: {error}"));
    }

    [ContextMenu("Debug: Test Login (claudetest01)")]
    private void DebugTestLogin()
    {
        Login("claudetest01", "Test1234!", (ok, error) =>
            Debug.Log(ok ? $"[AuthManager] Login 성공. AccessToken={AccessToken?.Substring(0, 12)}..." : $"[AuthManager] Login 실패: {error}"));
    }

    [ContextMenu("Debug: Test TryAutoLogin")]
    private void DebugTestAutoLogin()
    {
        TryAutoLogin(ok => Debug.Log(ok ? "[AuthManager] 자동 로그인 성공" : "[AuthManager] 자동 로그인 실패 (세션 없음/만료)"));
    }

    [ContextMenu("Debug: Logout")]
    private void DebugLogout()
    {
        Logout();
        Debug.Log("[AuthManager] 로그아웃 완료");
    }
}
