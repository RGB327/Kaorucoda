using TMPro;
using UnityEngine;

/// <summary>
/// 비밀번호 입력창 옆 눈 아이콘 버튼에 붙인다. 누를 때마다 평문/마스킹을 토글한다.
/// 아이콘 스프라이트를 하나만 쓰거나 안 쓰는 경우를 위해 eyeOpen/eyeClosed는 비워둬도 동작한다.
/// </summary>
public class PasswordVisibilityToggle : MonoBehaviour
{
    [SerializeField] private TMP_InputField passwordField;

    [Header("아이콘 (선택 — 없으면 토글만 되고 아이콘은 안 바뀜)")]
    [SerializeField] private GameObject eyeOpenIcon;
    [SerializeField] private GameObject eyeClosedIcon;

    private bool _visible;

    private void Awake()
    {
        ApplyState();
    }

    public void OnToggleClick()
    {
        _visible = !_visible;
        ApplyState();
    }

    private void ApplyState()
    {
        passwordField.contentType = _visible
            ? TMP_InputField.ContentType.Standard
            : TMP_InputField.ContentType.Password;

        // contentType만 바꾸면 이미 그려진 텍스트가 안 바뀌므로 강제로 다시 그리게 한다.
        passwordField.ForceLabelUpdate();

        if (eyeOpenIcon != null) eyeOpenIcon.SetActive(_visible);
        if (eyeClosedIcon != null) eyeClosedIcon.SetActive(!_visible);
    }
}
