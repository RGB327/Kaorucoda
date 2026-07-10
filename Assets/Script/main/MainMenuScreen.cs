using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>시작 메뉴. 화면 스택의 루트.</summary>
public class MainMenuScreen : UIScreen
{
    [Header("이동할 화면")]
    [SerializeField] private AchievementScreen achievementScreen;
    [SerializeField] private LoginScreen loginScreen;

    [Header("씬에 미리 배치된 팝업 (PopupCanvas 아래)")]
    [SerializeField] private SettingsPopup settingsPopup;
    [SerializeField] private ConfirmPopup confirmPopup;
    [SerializeField] private ContinuePopup continuePopup;
    [SerializeField] private ErrorPopup errorPopup;

    [SerializeField] private string gameSceneName = "SampleScene2";

    [Header("서버 통신 중 표시 (선택)")]
    [SerializeField] private GameObject loadingIndicator;

    private readonly SaveDataClient _saveDataClient = new();
    private bool _startInProgress;

    // --- 버튼 OnClick 에 연결 ---

    public void OnStartClick()
    {
        // 이전 세이브 유무를 서버에 물어보는 동안 중복 클릭 방지.
        if (_startInProgress) return;
        _startInProgress = true;
        if (loadingIndicator != null) loadingIndicator.SetActive(true);

        _saveDataClient.GetSave((saveBlob, hadError) =>
        {
            _startInProgress = false;
            if (loadingIndicator != null) loadingIndicator.SetActive(false);

            if (hadError)
            {
                ShowError("세이브 정보를 불러오지 못했습니다. 잠시 후 다시 시도해주세요.");
                return;
            }

            if (string.IsNullOrEmpty(saveBlob))
            {
                LoadGameScene();
                return;
            }

            PopupManager.Instance.Open(continuePopup, p => p.Setup(
                onContinue: LoadGameScene,
                onDelete: () => _saveDataClient.DeleteSave(ok =>
                {
                    if (ok) LoadGameScene();
                    else ShowError("삭제에 실패했습니다. 잠시 후 다시 시도해주세요.");
                })));
        });
    }

    private void ShowError(string message)
    {
        PopupManager.Instance.Open(errorPopup, p => p.Setup(message));
    }

    private void LoadGameScene()
    {
        // PopupManager 는 DontDestroyOnLoad 라 씬을 넘어가도 열린 팝업이 그대로 남는다.
        PopupManager.Instance?.CloseAll();
        SceneManager.LoadScene(gameSceneName);
    }

    public void OnAchievementClick()
    {
        ScreenManager.Instance.Push(achievementScreen);
    }

    public void OnSettingsClick()
    {
        PopupManager.Instance.Open(settingsPopup);
    }

    public void OnQuitClick()
    {
        ShowQuitConfirm();
    }

    public void OnLogoutClick()
    {
        AuthManager.Instance.Logout();
        ScreenManager.Instance.SetRoot(loginScreen);
    }

    // --- 루트에서의 뒤로가기 ---

    public override bool OnBackAtRoot()
    {
        ShowQuitConfirm();
        return true;   // 처리했음
    }

    private void ShowQuitConfirm()
    {
        PopupManager.Instance.Open(confirmPopup, p =>
            p.Setup("게임을 종료할까요?", onConfirm: Quit));
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