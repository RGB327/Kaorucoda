using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 스탯 포인트 배분 팝업. Player는 게임플레이 씬에만 있고 이 팝업은 다른 씬(PopupCanvas)에
/// 있어서 인스펙터로 직접 참조를 못 묶는다 — PopupManager/AuthManager와 같은 static Instance
/// 패턴을 쓰고, 대상 PlayerStats는 Setup()으로 매번 런타임에 주입받는다.
/// </summary>
public class StatAllocationPopup : Popup
{
    public static StatAllocationPopup Instance { get; private set; }

    [SerializeField] private TMP_Text availablePointsText;
    [SerializeField] private StatRow[] rows;

    private PlayerStats stats;

    protected override void Awake()
    {
        base.Awake();
        Instance = this;

        foreach (var row in rows)
        {
            var stat = row.stat;
            row.plusButton.onClick.AddListener(() =>
            {
                stats.TryAllocate(stat);
                RefreshUI();
            });
        }
    }

    private void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void Setup(PlayerStats target)
    {
        stats?.onStatsChanged.RemoveListener(RefreshUI);
        stats = target;
        stats.onStatsChanged.AddListener(RefreshUI);
        RefreshUI();
    }

    public override void Close(Action onComplete = null)
    {
        stats?.onStatsChanged.RemoveListener(RefreshUI);
        base.Close(onComplete);
    }

    private void RefreshUI()
    {
        if (stats == null) return;

        availablePointsText.text = $"포인트: {stats.AvailablePoints}";
        foreach (var row in rows)
            row.valueText.text = $"{row.label}: {stats.GetValue(row.stat):0.##} ({stats.GetAllocatedPoints(row.stat)})";
    }
}

[Serializable]
public class StatRow
{
    public StatType stat;
    public string label;
    public TMP_Text valueText;
    public Button plusButton;
}
