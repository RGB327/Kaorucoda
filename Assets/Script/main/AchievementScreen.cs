using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 업적 화면. 시작 메뉴와 배타적이므로 팝업이 아니라 화면이다.
/// 설정 팝업은 이 위에도 겹칠 수 있다.
/// </summary>
public class AchievementScreen : UIScreen
{
    [SerializeField] private AchievementItem itemPrefab;
    [SerializeField] private Transform contentRoot;      // ScrollRect > Viewport > Content
    [SerializeField] private SettingsPopup settingsPopup;

    private readonly List<AchievementItem> _items = new();

    public override void OnEnter()
    {
        base.OnEnter();
        Refresh();
    }

    public void OnSettingsClick()
    {
        PopupManager.Instance.Open(settingsPopup);
    }

    public void OnBackClick()
    {
        ScreenManager.Instance.Back();
    }

    private void Refresh()
    {
        IReadOnlyList<AchievementData> data = AchievementManager.GetAll();

        // 매번 Destroy / Instantiate 하지 않고 재사용한다.
        while (_items.Count < data.Count)
            _items.Add(Instantiate(itemPrefab, contentRoot));

        for (int i = 0; i < _items.Count; i++)
        {
            bool active = i < data.Count;
            _items[i].gameObject.SetActive(active);
            if (active) _items[i].Bind(data[i]);
        }
    }
}