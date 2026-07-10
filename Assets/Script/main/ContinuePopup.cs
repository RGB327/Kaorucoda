using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 이전 세이브가 있을 때 플레이 버튼을 누르면 뜨는 팝업.
/// 이어하기 / 삭제하기 / 닫기(X, 아무 것도 안 하고 그대로 보존) 세 가지 동작을 제공한다.
/// </summary>
public class ContinuePopup : Popup
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button closeButton;

    private Action _onContinue;
    private Action _onDelete;

    protected override void Awake()
    {
        base.Awake();

        continueButton.onClick.AddListener(() =>
        {
            var cb = _onContinue;
            PopupManager.Instance.CloseTop();
            cb?.Invoke();
        });

        deleteButton.onClick.AddListener(() =>
        {
            var cb = _onDelete;
            PopupManager.Instance.CloseTop();
            cb?.Invoke();
        });

        // X는 그냥 닫기만 한다 — 기록은 그대로 보존.
        closeButton.onClick.AddListener(() => PopupManager.Instance.CloseTop());
    }

    public void Setup(Action onContinue, Action onDelete)
    {
        if (messageText != null) messageText.text = "이전 기록이 있습니다. 이어하시겠습니까?";
        _onContinue = onContinue;
        _onDelete = onDelete;
    }
}
