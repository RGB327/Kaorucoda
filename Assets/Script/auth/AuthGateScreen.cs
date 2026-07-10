using UnityEngine;

/// <summary>
/// 앱 시작 시 첫 화면(ScreenManager의 rootScreen). 저장된 세션으로 자동 로그인을 시도해서
/// 성공하면 MainMenuScreen으로, 실패하면 LoginScreen으로 스택 루트를 바꾼다.
/// </summary>
public class AuthGateScreen : UIScreen
{
    [Header("이동할 화면")]
    [SerializeField] private MainMenuScreen mainMenuScreen;
    [SerializeField] private LoginScreen loginScreen;

    [Header("확인 중 표시 (선택)")]
    [SerializeField] private GameObject loadingIndicator;

    public override void OnEnter()
    {
        base.OnEnter();
        if (loadingIndicator != null) loadingIndicator.SetActive(true);

        AuthManager.Instance.TryAutoLogin(success =>
        {
            if (loadingIndicator != null) loadingIndicator.SetActive(false);
            ScreenManager.Instance.SetRoot(success ? (UIScreen)mainMenuScreen : loginScreen);
        });
    }
}
