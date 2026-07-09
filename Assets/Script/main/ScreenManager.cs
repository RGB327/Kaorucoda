using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 화면 레이어. ScreenCanvas(Sort Order 0) 에 붙인다.
/// 화면들은 씬에 미리 배치하고 rootScreen 만 지정하면 된다.
/// </summary>
public class ScreenManager : MonoBehaviour
{
    public static ScreenManager Instance { get; private set; }

    [SerializeField] private UIScreen rootScreen;

    [Tooltip("씬에 배치된 모든 화면. Start 에서 전부 꺼둔다.")]
    [SerializeField] private UIScreen[] allScreens;

    private readonly Stack<UIScreen> _stack = new();

    public UIScreen Current => _stack.Count > 0 ? _stack.Peek() : null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    private void Start()
    {
        foreach (var s in allScreens)
            if (s != null) s.gameObject.SetActive(false);

        Push(rootScreen);
    }

    public void Push(UIScreen screen)
    {
        if (screen == null || screen == Current) return;

        // 화면이 바뀌면 그 위에 떠 있던 팝업은 전부 정리한다.
        PopupManager.Instance?.CloseAll();

        if (_stack.Count > 0) _stack.Peek().OnExit();

        screen.OnEnter();
        _stack.Push(screen);
    }

    /// <summary>
    /// 한 단계 뒤로. 처리했으면 true.
    /// 루트에서는 해당 화면의 OnBackAtRoot() 에 위임한다.
    /// </summary>
    public bool Back()
    {
        if (_stack.Count == 0) return false;

        if (_stack.Count == 1)
            return _stack.Peek().OnBackAtRoot();

        PopupManager.Instance?.CloseAll();

        _stack.Pop().OnExit();
        _stack.Peek().OnEnter();
        return true;
    }
}