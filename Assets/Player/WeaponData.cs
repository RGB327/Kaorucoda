using System;
using UnityEngine;

/// <summary>무기 애셋. 기본 공격과 보조 공격을 각각 정의한다 — 보조 공격만 물리/마법이 갈리는 게 아니라
/// 둘 다 자기 데미지 타입을 따로 가진다 (무기에 따라 어느 쪽이든 될 수 있음).</summary>
[CreateAssetMenu(fileName = "NewWeapon", menuName = "Player/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName = "New Weapon";
    public WeaponAttackData basicAttack = new();
    public WeaponAttackData secondaryAttack = new();
}

[Serializable]
public class WeaponAttackData
{
    public DamageType damageType = DamageType.Physical;
    public float basePower = 20f;
    public float baseCooldown = 0.5f;
    public float range = 2f;
}
