using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public WeaponData weapon;

    private PlayerStats stats;
    private PlayerWeaponVisual visual;
    private bool canBasicAttack = true;
    private bool canSecondaryAttack = true;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        visual = GetComponent<PlayerWeaponVisual>();
        visual?.EquipWeapon(weapon);
    }

    /// <summary>무기 교체 시 사용 (인벤토리/드랍 획득 등 나중에 붙을 시스템이 호출할 자리).</summary>
    public void EquipWeapon(WeaponData newWeapon)
    {
        weapon = newWeapon;
        visual?.EquipWeapon(weapon);
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        if (Keyboard.current.qKey.wasPressedThisFrame && canBasicAttack)
            TriggerAttack(weapon.basicAttack, isBasic: true);

        if (Keyboard.current.eKey.wasPressedThisFrame && canSecondaryAttack)
            TriggerAttack(weapon.secondaryAttack, isBasic: false);
    }

    void TriggerAttack(WeaponAttackData attackData, bool isBasic)
    {
        Attack(attackData);

        if (isBasic) visual?.PlayBasicAttack();
        else visual?.PlaySecondaryAttack();

        float cooldown = ComputeCooldown(attackData.baseCooldown);
        if (isBasic)
        {
            canBasicAttack = false;
            Invoke(nameof(ResetBasicAttack), cooldown);
        }
        else
        {
            canSecondaryAttack = false;
            Invoke(nameof(ResetSecondaryAttack), cooldown);
        }
    }

    void Attack(WeaponAttackData attackData)
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        foreach (GameObject enemy in enemies)
        {
            float distance = Vector2.Distance(transform.position, enemy.transform.position);

            if (distance <= attackData.range)
            {
                enemy.GetComponent<EnemyHP>().TakeDamage(ComputeDamage(attackData));
            }
        }
    }

    int ComputeDamage(WeaponAttackData attackData)
    {
        StatType relevantStat = attackData.damageType == DamageType.Physical
            ? StatType.PhysicalAttack
            : StatType.MagicAttack;

        float damage = attackData.basePower + stats.GetValue(relevantStat);

        if (Random.value < Mathf.Clamp01(stats.GetValue(StatType.CritChance)))
            damage *= stats.critMultiplier;

        return Mathf.RoundToInt(damage);
    }

    // 무기 기본 쿨타임을 AttackSpeed 스탯(% 보너스)으로 나눠서 최종 쿨타임을 낸다.
    float ComputeCooldown(float baseCooldown)
    {
        float attackSpeed = stats.GetValue(StatType.AttackSpeed);
        return Mathf.Max(0.05f, baseCooldown / (1f + attackSpeed * 0.01f));
    }

    void ResetBasicAttack() => canBasicAttack = true;
    void ResetSecondaryAttack() => canSecondaryAttack = true;
}
