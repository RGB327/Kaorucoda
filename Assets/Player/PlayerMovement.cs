using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    public Vector2 inputVec;
    public float dashPower = 12f;
    public float dashCoolTime = 0.2f;
    public float dashTime = 0.3f;
    bool canDash = true;
    bool isDash = false;

    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    PlayerStats stats;

    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        stats = GetComponent<PlayerStats>();
    }

    void Update()
    {
        if (Keyboard.current == null) return;

        float x = (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed ? 1f : 0f)
                 - (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed ? 1f : 0f);
        float y = (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed ? 1f : 0f)
                 - (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed ? 1f : 0f);
        inputVec = new Vector2(x, y);

        if (canDash && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            canDash = false;
            isDash = true;

            if (spriteRenderer.flipX)
            {
                rigid.linearVelocity =
                    new Vector2(-dashPower, rigid.linearVelocity.y);
            }
            else
            {
                rigid.linearVelocity =
                    new Vector2(dashPower, rigid.linearVelocity.y);
            }

            // 원래 여기 하드코딩된 0.2f/0.3f가 있었는데, 이미 있는 dashCoolTime/dashTime 필드를
            // 안 쓰고 있어서 인스펙터에서 튜닝해도 반영이 안 되던 버그였음 — 필드를 쓰도록 고침.
            // (원래 숫자 그대로 유지: EndDash=0.2=dashCoolTime, ResetDash=0.3=dashTime)
            Invoke("EndDash", dashCoolTime);
            Invoke("ResetDash", dashTime);
        }
    }

    void FixedUpdate()
    {
        if (isDash) return;

        rigid.linearVelocity = inputVec.normalized * stats.GetValue(StatType.MoveSpeed);
    }
    void LateUpdate()
    {
        if (inputVec.x != 0)
        {
            spriteRenderer.flipX = inputVec.x < 0;
        }
    }

    void EndDash()
    {
        isDash = false;
    }

    void ResetDash()
    {
        canDash = true;
    }
}
