using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 뒤로가기(ESC / 안드로이드 back) 입력을 위 레이어부터 순서대로 물어본다.
/// 씬에 하나만 둔다.
/// </summary>
public class UIRouter : MonoBehaviour
{
    private void Update()
    {
        // 프로젝트 Player Settings 의 Active Input Handling 이
        // "Input System Package" 이므로 구 Input 클래스 대신 이걸 쓴다.
        if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame) return;

        HandleBack();
    }

    private void HandleBack()
    {
        // 1순위: 팝업 레이어
        if (PopupManager.Instance != null && PopupManager.Instance.HasOpen)
        {
            PopupManager.Instance.CloseTop();
            return;
        }

        // 2순위: 화면 레이어
        ScreenManager.Instance?.Back();
    }
}