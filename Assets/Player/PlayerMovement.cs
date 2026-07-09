using UnityEngine;

public class Movement : MonoBehaviour
{
    public Vector2 inputVec;
    public float moveSpeed = 7f;
    public float dashPower = 12f;

    bool canDash = true;
    bool isDash = false;
    
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;
    void Start()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        inputVec.x = Input.GetAxisRaw("Horizontal");
        inputVec.y = Input.GetAxisRaw("Vertical");

        if (canDash && Input.GetKeyDown(KeyCode.LeftShift))
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

            Invoke("EndDash", 0.2f);
            Invoke("ResetDash", 0.3f);
        }
    }
    
        void FixedUpdate()
    {
        if (isDash) return;

        rigid.linearVelocity = inputVec.normalized * moveSpeed;
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
