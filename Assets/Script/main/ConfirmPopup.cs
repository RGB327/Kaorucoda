using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>확인 / 취소 팝업. PopupManager.Open(prefab, p => p.Setup(...)) 로 사용.</summary>
public class ConfirmPopup : Popup
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button okButton;
    [SerializeField] private Button cancelButton;

    private Action _onConfirm;
    private Action _onCancel;

    protected override void Awake()
    {
        base.Awake();

        // 프리팹 인스턴스마다 한 번씩만 등록되므로 중첩 걱정이 없다.
        okButton.onClick.AddListener(() =>
        {
            var cb = _onConfirm;
            PopupManager.Instance.CloseTop();
            cb?.Invoke();
        });

        cancelButton.onClick.AddListener(() =>
        {
            var cb = _onCancel;
            PopupManager.Instance.CloseTop();
            cb?.Invoke();
        });
    }

    public void Setup(string message, Action onConfirm, Action onCancel = null)
    {
        messageText.text = message;
        _onConfirm = onConfirm;
        _onCancel = onCancel;
    }
}