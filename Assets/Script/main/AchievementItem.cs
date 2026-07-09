using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class AchievementData
{
    public string Id;
    public string Title;
    public string Description;
    public bool Unlocked;
}

/// <summary>업적 리스트의 한 줄.</summary>
public class AchievementItem : MonoBehaviour
{
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private Image icon;
    [SerializeField] private GameObject lockOverlay;

    public void Bind(AchievementData data)
    {
        titleText.text = data.Title;
        descText.text = data.Description;
        lockOverlay.SetActive(!data.Unlocked);
        icon.color = data.Unlocked ? Color.white : new Color(1f, 1f, 1f, 0.35f);
    }
}

/// <summary>실제 저장·해금 로직으로 교체할 자리. 지금은 컴파일용 더미.</summary>
public static class AchievementManager
{
    private static readonly List<AchievementData> Cache = new();

    public static IReadOnlyList<AchievementData> GetAll() => Cache;
}