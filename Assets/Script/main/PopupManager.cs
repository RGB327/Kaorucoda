using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 팝업 레이어. PopupCanvas(Sort Order 100) 에 붙인다.
/// 씬 전환에도 살아남는 DontDestroyOnLoad 싱글턴이므로, 최초 씬에만 배치하면
/// 이후 로드되는 모든 씬에서 PopupManager.Instance 로 그대로 이어서 쓸 수 있다.
///
/// 팝업은 화면(UIScreen)과 마찬가지로 프리팹이 아니라 씬에 미리 배치해두고
/// Open/Close 로 SetActive 만 토글한다. Instantiate/Destroy 하지 않는다.
///
/// 계층 전제:
///   PopupCanvas
///   └── PopupRoot        ← popupRoot
///       ├── Dimmer       ← dimmer (반드시 popupRoot 의 자식)
///       └── (팝업들이 여기 자식으로 미리 배치되어 있다, 기본 비활성)
/// </summary>
public class PopupManager : MonoBehaviour
{
    public static PopupManager Instance { get; private set; }

    [SerializeField] private Transform popupRoot;
    [SerializeField] private GameObject dimmer;

    private readonly Stack<Popup> _stack = new();

    public bool HasOpen => _stack.Count > 0;

    private void Awake()
    {
        Debug.Log($"[PopupManager] Awake on '{name}', activeInHierarchy={gameObject.activeInHierarchy}", this);

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        dimmer.SetActive(false);
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>
    /// 팝업을 띄운다. popup 은 씬에 미리 배치된 인스턴스를 그대로 넘긴다(프리팹 아님).
    /// setup 은 열기 애니메이션이 시작되기 전에 호출되므로
    /// 여기서 텍스트·콜백 등을 안전하게 채워 넣을 수 있다.
    /// </summary>
    public T Open<T>(T popup, Action<T> setup = null) where T : Popup
    {
        setup?.Invoke(popup);

        popup.Open();
        _stack.Push(popup);
        RefreshDimmer();

        return popup;
    }

    /// <summary>최상단 팝업을 닫는다. 뒤로가기(ESC)가 여기로 들어온다.</summary>
    public void CloseTop()
    {
        if (_stack.Count == 0) return;
        if (_stack.Peek().IsClosing) return;   // 이미 닫히는 중이면 무시

        _stack.Pop().Close();
        RefreshDimmer();
    }

    /// <summary>특정 팝업을 닫는다. 스택 중간에 있어도 안전하게 제거한다.</summary>
    public void Close(Popup target)
    {
        if (target == null || !_stack.Contains(target)) return;

        var remaining = new List<Popup>(_stack.Count);
        foreach (var p in _stack)          // Stack 순회는 top → bottom
            if (p != target) remaining.Add(p);

        _stack.Clear();
        for (int i = remaining.Count - 1; i >= 0; i--)
            _stack.Push(remaining[i]);

        target.Close();
        RefreshDimmer();
    }

    /// <summary>
    /// 전부 즉시 닫는다. 화면 전환 시 호출.
    /// 화면 전환 연출과 팝업 닫기 연출이 겹치면 지저분하므로 애니메이션 없이 끈다.
    /// </summary>
    public void CloseAll()
    {
        while (_stack.Count > 0)
            _stack.Pop().CloseImmediate();

        RefreshDimmer();
    }

    /// <summary>Dimmer 의 Button.onClick 에 연결한다.</summary>
    public void OnDimmerClick()
    {
        if (_stack.Count == 0) return;
        if (!_stack.Peek().CloseOnBackgroundClick) return;

        CloseTop();
    }

    /// <summary>
    /// Dimmer 를 항상 "최상단 팝업 바로 아래"로 옮긴다.
    /// 팝업이 여러 겹일 때 아래쪽 팝업들이 자연스럽게 어두워진다.
    /// </summary>
    private void RefreshDimmer()
    {
        bool visible = _stack.Count > 0;
        dimmer.SetActive(visible);

        if (!visible) return;

        // 닫히는 중인 팝업이 아직 자식으로 남아 있을 수 있으므로,
        // 최상단 팝업을 명시적으로 맨 뒤로 올린 뒤 Dimmer 를 그 바로 아래에 둔다.
        _stack.Peek().transform.SetAsLastSibling();
        dimmer.transform.SetSiblingIndex(popupRoot.childCount - 2);
    }
}