using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>단순 오류 안내 팝업. 확인 버튼 하나로 닫는다.</summary>
public class ErrorPopup : Popup
{
    [SerializeField] private TMP_Text messageText;
    [SerializeField] private Button okButton;

    protected override void Awake()
    {
        base.Awake();
        okButton.onClick.AddListener(() => PopupManager.Instance.CloseTop());
    }

    public void Setup(string message)
    {
        if (messageText != null) messageText.text = message;
    }
}
