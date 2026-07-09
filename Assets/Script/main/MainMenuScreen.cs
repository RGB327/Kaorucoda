using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>시작 메뉴. 화면 스택의 루트.</summary>
public class MainMenuScreen : UIScreen
{
    [Header("이동할 화면")]
    [SerializeField] private AchievementScreen achievementScreen;

    [Header("씬에 미리 배치된 팝업 (PopupCanvas 아래)")]
    [SerializeField] private SettingsPopup settingsPopup;
    [SerializeField] private ConfirmPopup confirmPopup;

    [SerializeField] private string gameSceneName = "Game";

    // --- 버튼 OnClick 에 연결 ---

    public void OnStartClick()
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