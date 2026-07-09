using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 모든 팝업의 베이스. 화면(UIScreen) 위에 겹쳐서 뜨며, 스택으로 관리된다.
/// </summary>
[RequireComponent(typeof(CanvasGroup))]
public abstract class Popup : MonoBehaviour
{
    [Header("Popup")]
    [Tooltip("스케일 연출을 적용할 패널. 비워두면 자기 자신을 사용한다.")]
    [SerializeField] protected RectTransform panel;

    [Tooltip("Dimmer(바깥 영역)를 클릭했을 때 닫을지 여부.")]
    [SerializeField] private bool closeOnBackgroundClick = true;

    [SerializeField] private float openDuration = 0.20f;
    [SerializeField] private float closeDuration = 0.15f;

    private CanvasGroup _canvasGroup;

    /// <summary>Close()가 호출되어 닫히는 중. 중복 입력 차단용.</summary>
    public bool IsClosing { get; private set; }

    public bool CloseOnBackgroundClick => closeOnBackgroundClick;

    protected virtual void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        if (panel == null) panel = (RectTransform)transform;
    }

    /// <summary>PopupManager가 호출한다. 직접 부르지 말 것.</summary>
    public virtual void Open()
    {
        IsClosing = false;
        gameObject.SetActive(true);
        _canvasGroup.blocksRaycasts = true;

        StopAllCoroutines();
        StartCoroutine(OpenRoutine());
    }

    /// <summary>PopupManager가 호출한다. 직접 부르지 말 것.</summary>
    public virtual void Close(Action onComplete = null)
    {
        if (IsClosing) return;
        IsClosing = true;

        // 닫히는 애니메이션 도중의 중복 클릭을 막는다.
        _canvasGroup.blocksRaycasts = false;

        StopAllCoroutines();
        StartCoroutine(CloseRoutine(onComplete));
    }

    /// <summary>애니메이션 없이 즉시 종료. 화면 전환 시 사용.</summary>
    public virtual void CloseImmediate()
    {
        IsClosing = true;
        StopAllCoroutines();
        _canvasGroup.blocksRaycasts = false;
        gameObject.SetActive(false);
    }

    private IEnumerator OpenRoutine()
    {
        float t = 0f;
        while (t < openDuration)
        {
            // Time.timeScale = 0 인 일시정지 중에도 동작해야 하므로 unscaled.
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / openDuration);

            _canvasGroup.alpha = p;
            panel.localScale = Vector3.one * Mathf.LerpUnclamped(0.85f, 1f, EaseOutBack(p));
            yield return null;
        }

        _canvasGroup.alpha = 1f;
        panel.localScale = Vector3.one;
    }

    private IEnumerator CloseRoutine(Action onComplete)
    {
        float t = 0f;
        while (t < closeDuration)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / closeDuration);

            _canvasGroup.alpha = 1f - p;
            panel.localScale = Vector3.one * Mathf.Lerp(1f, 0.85f, p);
            yield return null;
        }

        gameObject.SetActive(false);
        onComplete?.Invoke();
    }

    /// <summary>살짝 튕기는 느낌. p=1에서 정확히 1을 반환한다.</summary>
    private static float EaseOutBack(float p)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        float x = p - 1f;
        return 1f + c3 * x * x * x + c1 * x * x;
    }
}