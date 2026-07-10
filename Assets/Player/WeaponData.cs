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

    [Header("표시/연출 (그림·애니메이션 준비되면 채우기)")]
    [Tooltip("플레이어가 들고 있는 무기 이미지. WeaponHolder의 SpriteRenderer에 적용됨.")]
    public Sprite icon;

    [Tooltip("이 무기 전용 공격 모션. 비워두면 Player의 기본 Animator Controller 그대로 사용 — " +
             "즉 그림/애니메이션 없이도 지금 당장 문제없이 동작한다.")]
    public AnimatorOverrideController animatorOverride;
}

[Serializable]
public class WeaponAttackData
{
    public DamageType damageType = DamageType.Physical;
    public float basePower = 20f;
    public float baseCooldown = 0.5f;
    public float range = 2f;
}
