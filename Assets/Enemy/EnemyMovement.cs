using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    public enum State
    {
        Idle, //가만히 멈춤(감지 범위 밖)
        Chase //추적
    }
    Rigidbody2D rigid;
    SpriteRenderer spriteRenderer;

    public float speed = 3f;
    public Rigidbody2D target;
    public State state;
    public float chaseRange = 5f; //감지 범위

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        StateUpdate();
    }
    

    void StateUpdate()
    {
        float distance = Vector2.Distance(target.position, rigid.position);

        if (distance <= chaseRange)
        {
            state = State.Chase;
        }
        else
        {
            state = State.Idle;
        }
    }

    void Idle()
    {
        
    }

    void Chase()
    {
        Vector2 dirVec = target.position - rigid.position;
        Vector2 nextVec = dirVec.normalized * speed * Time.fixedDeltaTime;
        rigid.MovePosition(rigid.position + nextVec);
    }
    void FixedUpdate()
    {
        switch (state)
        {
            case State.Idle:
                Idle();
                break;
            case State.Chase:
                Chase();
                break; 
        }

        if (state == State.Idle) return;

    }

    void LateUpdate()
    {
        if (target.position.x < rigid.position.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }
}
