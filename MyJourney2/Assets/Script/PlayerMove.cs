using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("玩家移动速度")]
    public float speed = 5f;

    [Header("组件")]
    public MobileJoystick joystick;

    private Rigidbody2D rb;
    private Animator anim;
    private SpriteRenderer sr;
    private Vector2 moveInput;

    private bool isKnockback;
    private float knockbackTimer;
    private float knockbackDuration = 0.2f;

    /// <summary>
    /// 是否正在攻击（攻击时停止移动）
    /// </summary>
    public bool isAttacking { get; set; }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;

        // 自动查找摇杆
        if (joystick == null)
            joystick = FindObjectOfType<MobileJoystick>();
    }

    private void Update()
    {
        // 如果正在被弹开，不处理输入
        if (isKnockback) return;

        float horizontal = 0f;
        float vertical = 0f;

        // 优先使用摇杆输入
        if (joystick != null && (Mathf.Abs(joystick.GetHorizontal()) > 0.1f || Mathf.Abs(joystick.GetVertical()) > 0.1f))
        {
            horizontal = joystick.GetHorizontal();
            vertical = joystick.GetVertical();
        }
        // 使用键盘输入
        else
        {
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
        }

        moveInput = new Vector2(horizontal, vertical).normalized;
    }

    private void FixedUpdate()
    {
        // 如果正在被弹开，不应用正常移动
        if (isKnockback)
        {
            knockbackTimer -= Time.fixedDeltaTime;
            if (knockbackTimer <= 0)
            {
                isKnockback = false;
                Debug.Log("[PlayerState] Knockback结束，恢复正常");
            }
            return;
        }

        // 如果正在攻击，停止移动
        if (isAttacking)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        // 只有在有输入时才移动
        if (moveInput.magnitude > 0.1f)
        {
            Debug.Log($"[PlayerState] 移动: {moveInput}");
        }

        rb.velocity = moveInput * speed;

        // sprite翻转
        if (moveInput.x > 0)
            sr.flipX = false;
        else if (moveInput.x < 0)
            sr.flipX = true;

        anim.SetFloat("hor", Mathf.Abs(moveInput.x));
        anim.SetFloat("ver", Mathf.Abs(moveInput.y));
    }

    /// <summary>
    /// 弹开玩家
    /// </summary>
    /// <param name="direction">弹开方向</param>
    /// <param name="force">弹开力度</param>
    public void Knockback(Vector2 direction, float force)
    {
        Debug.Log($"[PlayerState] 被弹飞! direction={direction}, force={force}");
        rb.velocity = direction * force;
        isKnockback = true;
        knockbackTimer = knockbackDuration;
    }
}