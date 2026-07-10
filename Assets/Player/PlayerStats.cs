using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 플레이어 스탯 저장소. 스탯 = 기본값 + (배분된 포인트 * 포인트당 증가량).
/// 포인트를 어떻게 얻는지는 이 스크립트가 신경 쓰지 않는다 — AddPoints()는 나중에
/// 레벨업/경험치 같은 시스템이 호출할 자리로 열어둔 것뿐, 지금은 아무도 안 부른다.
/// </summary>
public class PlayerStats : MonoBehaviour
{
    [Header("스탯별 기본값 + 포인트당 증가량 (가안, 나중에 밸런싱)")]
    [SerializeField]
    private List<StatConfig> statConfigs = new()
    {
        new StatConfig { type = StatType.MaxHP, baseValue = 100f, perPoint = 10f },
        new StatConfig { type = StatType.MoveSpeed, baseValue = 7f, perPoint = 0.2f },
        new StatConfig { type = StatType.AttackSpeed, baseValue = 0f, perPoint = 1f },
        new StatConfig { type = StatType.PhysicalAttack, baseValue = 20f, perPoint = 2f },
        new StatConfig { type = StatType.MagicAttack, baseValue = 20f, perPoint = 2f },
        new StatConfig { type = StatType.CritChance, baseValue = 0.05f, perPoint = 0.01f },
    };

    [Header("특정 스탯을 일정량 찍으면 발화되는 훅 (실제 능력 내용은 나중에 연결)")]
    [SerializeField] private List<StatThreshold> thresholds = new();

    [Header("전투 관련 튜닝")]
    public float critMultiplier = 1.5f;

    public int AvailablePoints { get; private set; }
    public UnityEvent onStatsChanged;

    private readonly Dictionary<StatType, int> _allocated = new();

    public float GetValue(StatType type)
    {
        var config = statConfigs.Find(c => c.type == type);
        return config.baseValue + config.perPoint * GetAllocatedPoints(type);
    }

    public int GetAllocatedPoints(StatType type)
        => _allocated.TryGetValue(type, out var points) ? points : 0;

    /// <summary>가진 포인트가 있으면 하나를 소모해서 해당 스탯에 배분한다.</summary>
    public bool TryAllocate(StatType type)
    {
        if (AvailablePoints <= 0) return false;

        AvailablePoints--;
        _allocated[type] = GetAllocatedPoints(type) + 1;

        CheckThresholds(type);
        onStatsChanged?.Invoke();
        return true;
    }

    /// <summary>포인트 획득 시스템이 나중에 호출할 자리. 지금은 호출하는 곳이 없다.</summary>
    public void AddPoints(int amount)
    {
        AvailablePoints += amount;
        onStatsChanged?.Invoke();
    }

    private void CheckThresholds(StatType type)
    {
        int points = GetAllocatedPoints(type);
        foreach (var threshold in thresholds)
        {
            if (threshold.stat == type && !threshold.fired && points >= threshold.pointsRequired)
            {
                threshold.fired = true;
                threshold.onReached?.Invoke();
            }
        }
    }

    [ContextMenu("Debug: Grant 5 Stat Points")]
    private void DebugGrantPoints() => AddPoints(5);
}

[Serializable]
public class StatConfig
{
    public StatType type;
    public float baseValue;
    public float perPoint;
}

[Serializable]
public class StatThreshold
{
    public StatType stat;
    public int pointsRequired;
    public UnityEvent onReached;
    [HideInInspector] public bool fired;
}
