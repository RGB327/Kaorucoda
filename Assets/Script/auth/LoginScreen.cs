using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>로그인 화면. 자동 로그인 실패 시(또는 로그아웃 시) 스택 루트가 된다.</summary>
public class LoginScreen : UIScreen
{
    [Header("입력")]
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private Button loginButton;

    [Header("이동할 화면")]
    [SerializeField] private MainMenuScreen mainMenuScreen;
    [SerializeField] private SignupScreen signupScreen;

    [Header("띄울 팝업 (씬에 미리 배치된 오브젝트)")]
    [SerializeField] private ConfirmPopup confirmPopup;

    public override void OnEnter()
    {
        base.OnEnter();
        SetError(string.Empty);
    }

    public void OnLoginClick()
    {
        SetError(string.Empty);
        SetBusy(true);

        AuthManager.Instance.Login(usernameField.text, passwordField.text, (ok, error) =>
        {
            SetBusy(false);

            if (!ok)
            {
                SetError(error);
                return;
            }

            usernameField.text = string.Empty;
            passwordField.text = string.Empty;
            ScreenManager.Instance.SetRoot(mainMenuScreen);
        });
    }

    public void OnGoToSignupClick()
    {
        ScreenManager.Instance.Push(signupScreen);
    }

    public override bool OnBackAtRoot()
    {
        PopupManager.Instance.Open(confirmPopup, p =>
            p.Setup("게임을 종료할까요?", onConfirm: Quit));
        return true;
    }

    private void SetBusy(bool busy)
    {
        if (loginButton != null) loginButton.interactable = !busy;
    }

    private void SetError(string message)
    {
        if (errorText != null) errorText.text = message ?? string.Empty;
    }

    private static void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
