using UnityEngine;

/// <summary>
/// 무기 이미지·공격 모션 담당. PlayerAttack은 전투 계산만 하고, "화면에 어떻게 보이는지"는
/// 전부 여기로 분리했다. Animator/스프라이트가 아직 없어도(그림 준비 전) 에러 없이 그냥
/// 아무것도 안 그리고 넘어가므로 지금 바로 붙여놔도 안전하다.
/// </summary>
public class PlayerWeaponVisual : MonoBehaviour
{
    private static readonly int BasicAttackTrigger = Animator.StringToHash("BasicAttack");
    private static readonly int SecondaryAttackTrigger = Animator.StringToHash("SecondaryAttack");

    [Tooltip("무기 이미지를 그릴 자식 오브젝트의 SpriteRenderer (예: Player/WeaponHolder).")]
    [SerializeField] private SpriteRenderer weaponRenderer;

    [Tooltip("공격 모션을 재생할 Animator. Player 본체에 붙임 (없으면 비워둬도 됨).")]
    [SerializeField] private Animator animator;

    private RuntimeAnimatorController defaultController;

    void Awake()
    {
        if (animator != null) defaultController = animator.runtimeAnimatorController;
    }

    /// <summary>무기를 바꿔 낄 때(장착/스왑) 호출 — 이미지와 전용 모션을 갱신한다.</summary>
    public void EquipWeapon(WeaponData weapon)
    {
        if (weaponRenderer != null) weaponRenderer.sprite = weapon != null ? weapon.icon : null;

        if (animator == null) return;

        animator.runtimeAnimatorController =
            weapon != null && weapon.animatorOverride != null ? weapon.animatorOverride : defaultController;
    }

    public void PlayBasicAttack()
    {
        if (animator != null) animator.SetTrigger(BasicAttackTrigger);
    }

    public void PlaySecondaryAttack()
    {
        if (animator != null) animator.SetTrigger(SecondaryAttackTrigger);
    }
}
