using UnityEngine;

/// <summary>
/// 화면. 서로 배타적이며 한 번에 하나만 보인다. Dimmer 없음.
/// 씬에 미리 배치해두고 SetActive 로만 토글한다(스크롤 위치 등 상태 유지 목적).
/// </summary>
public abstract class UIScreen : MonoBehaviour
{
    /// <summary>이 화면이 최상단이 되어 보여질 때.</summary>
    public virtual void OnEnter()
    {
        gameObject.SetActive(true);
    }

    /// <summary>다른 화면에 가려지거나 pop 될 때.</summary>
    public virtual void OnExit()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 이 화면이 스택의 루트인 상태에서 뒤로가기가 눌렸을 때.
    /// 직접 처리했으면 true, 아무것도 안 했으면 false 를 반환한다.
    /// </summary>
    public virtual bool OnBackAtRoot() => false;
}