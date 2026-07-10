using UnityEngine;

public class PlayerHP : MonoBehaviour
{
    public int hp;

    private PlayerStats stats;
    private int maxHp;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        maxHp = Mathf.RoundToInt(stats.GetValue(StatType.MaxHP));
        hp = maxHp;
        stats.onStatsChanged.AddListener(RefreshMaxHp);
    }

    void OnDestroy()
    {
        if (stats != null) stats.onStatsChanged.RemoveListener(RefreshMaxHp);
    }

    // 최대체력이 늘어난 만큼만 같이 올린다 — 포인트 찍었다고 이미 입은 데미지까지 회복시키지는 않는다.
    void RefreshMaxHp()
    {
        int newMax = Mathf.RoundToInt(stats.GetValue(StatType.MaxHP));
        hp += newMax - maxHp;
        maxHp = newMax;
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;

        if (hp <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("dead");
        Destroy(gameObject);
    }
}
