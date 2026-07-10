using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>회원가입 화면. LoginScreen에서 Push로 진입한다.</summary>
public class SignupScreen : UIScreen
{
    [Header("입력")]
    [SerializeField] private TMP_InputField usernameField;
    [SerializeField] private TMP_InputField passwordField;
    [SerializeField] private TMP_InputField confirmPasswordField;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private Button signupButton;

    [Header("이동할 화면")]
    [SerializeField] private MainMenuScreen mainMenuScreen;

    public override void OnEnter()
    {
        base.OnEnter();
        SetError(string.Empty);
    }

    public void OnSignupClick()
    {
        if (passwordField.text != confirmPasswordField.text)
        {
            SetError("비밀번호가 일치하지 않습니다.");
            return;
        }

        SetError(string.Empty);
        SetBusy(true);

        AuthManager.Instance.SignUp(usernameField.text, passwordField.text, (ok, error) =>
        {
            SetBusy(false);

            if (!ok)
            {
                SetError(error);
                return;
            }

            usernameField.text = string.Empty;
            passwordField.text = string.Empty;
            confirmPasswordField.text = string.Empty;
            // 가입 = 자동 로그인이므로 Login 화면으로 돌아가지 않고 바로 메인메뉴로 스택 루트를 바꾼다.
            ScreenManager.Instance.SetRoot(mainMenuScreen);
        });
    }

    public void OnBackClick()
    {
        ScreenManager.Instance.Back();
    }

    private void SetBusy(bool busy)
    {
        if (signupButton != null) signupButton.interactable = !busy;
    }

    private void SetError(string message)
    {
        if (errorText != null) errorText.text = message ?? string.Empty;
    }
}
