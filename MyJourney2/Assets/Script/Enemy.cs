using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("敌人移动速度")]
    public float moveSpeed = 2f;

    [Tooltip("攻击距离")]
    public float attackDistance = 1f;

    [Header("攻击设置")]
    public float attackRange = 1f;
    public int attackDamage = 1;

    [Header("组件")]
    public Transform attackPoint;

    private Animator anim;
    private SpriteRenderer sr;
    private Rigidbody2D rb;

    private Transform player;
    private Transform playerBase;
    private bool isPlayerInRange;
    private bool isAttacking;
    private float attackCoolDown = 1f;
    private float lastAttackTime;
    private float attackStartTime;
    private float attackDuration = 2f;  // 攻击超时保护

    private int playerLayer;
    private string playerTag = "Player";

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.isKinematic = true;

        playerLayer = LayerMask.NameToLayer("Player");
    }

    private void Start()
    {
        SetIdleState();
        // 尝试自动找到玩家
        TryFindPlayer();
    }

    /// <summary>
    /// 手动触发追踪玩家（由窝点生成敌人时调用）
    /// </summary>
    public void StartChasingPlayer()
    {
        TryFindPlayer();
        TryFindPlayerBase();
        isPlayerInRange = true;
        if (player != null)
        {
            Debug.Log($"{gameObject.name} 开始追踪玩家！");
        }
    }

    private void TryFindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
    }

    private void TryFindPlayerBase()
    {
        if (playerBase == null)
        {
            PlayerBase baseComponent = FindObjectOfType<PlayerBase>();
            if (baseComponent != null)
            {
                playerBase = baseComponent.transform;
                Debug.Log($"{gameObject.name} 找到基地!");
            }
        }
    }

    private void Update()
    {
        // 尝试查找基地
        TryFindPlayerBase();

        // 优先攻击基地，如果基地不存在或已摧毁，则攻击玩家
        Transform target = GetTarget();

        if (target == null) return;

        // 超时保护：如果攻击超时，自动重置攻击状态
        if (isAttacking && Time.time - attackStartTime > attackDuration)
        {
            isAttacking = false;
        }

        if (isAttacking) return;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        // 攻击冷却中，保持idle状态
        if (Time.time < lastAttackTime + attackCoolDown)
        {
            SetIdleState();
            return;
        }

        if (distanceToTarget <= attackDistance)
        {
            AttackTarget(target);
        }
        else if (isPlayerInRange)
        {
            ChaseTarget(target);
        }
        else
        {
            SetIdleState();
        }
    }

    /// <summary>
    /// 获取攻击目标（优先基地）
    /// </summary>
    private Transform GetTarget()
    {
        // 优先选择基地（如果存在且未摧毁）
        if (playerBase != null)
        {
            PlayerBase baseComponent = playerBase.GetComponent<PlayerBase>();
            if (baseComponent != null && !baseComponent.IsDestroyed)
            {
                return playerBase;
            }
        }

        // 基地不存在或已摧毁，攻击玩家
        TryFindPlayer();
        return player;
    }

    private void ChaseTarget(Transform target)
    {
        SetWalkState();

        Vector2 direction = (target.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;

        if (direction.x > 0)
            sr.flipX = false;
        else if (direction.x < 0)
            sr.flipX = true;
    }

    private void AttackTarget(Transform target)
    {
        isAttacking = true;
        attackStartTime = Time.time;  // 记录攻击开始时间
        rb.velocity = Vector2.zero;
        SetAttackState();
        lastAttackTime = Time.time;
    }

    private void ChasePlayer()
    {
        SetWalkState();

        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = direction * moveSpeed;

        if (direction.x > 0)
            sr.flipX = false;
        else if (direction.x < 0)
            sr.flipX = true;
    }

    private void AttackPlayer()
    {
        isAttacking = true;
        attackStartTime = Time.time;  // 记录攻击开始时间
        rb.velocity = Vector2.zero;
        SetAttackState();
        lastAttackTime = Time.time;
    }

    // Animation Event: 攻击瞬间
    public void OnAttack()
    {
        // 获取攻击点位置
        Vector2 attackPos = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;

        // 获取当前目标（基地或玩家）
        Transform target = GetTarget();

        if (target != null)
        {
            float distance = Vector2.Distance(attackPos, target.position);
            if (distance <= attackRange)
            {
                // 攻击基地
                PlayerBase baseComponent = target.GetComponent<PlayerBase>();
                if (baseComponent != null)
                {
                    EnemyHealth health = target.GetComponent<EnemyHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(attackDamage, false);  // 不显示伤害数字
                        Debug.Log("[Enemy] 对基地造成伤害！");
                    }
                    return;
                }

                // 攻击玩家
                if (target.CompareTag("Player"))
                {
                    PlayerHealth health = target.GetComponent<PlayerHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(attackDamage);
                        Debug.Log("[Enemy] 对玩家造成伤害！");
                    }
                }
            }
        }
    }

    // Animation Event: 攻击结束回到idle
    public void OnAttackEnd()
    {
        isAttacking = false;
        SetIdleState();
    }

    private void OnDrawGizmosSelected()
    {
        // 绘制攻击范围
        Vector2 attackPos = attackPoint != null ? (Vector2)attackPoint.position : (Vector2)transform.position;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPos, attackRange);

        // 绘制攻击点
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPos, 0.1f);
    }

    private void SetIdleState()
    {
        anim.SetBool("isIdle", true);
        anim.SetBool("isWalk", false);
        anim.SetBool("isAttack", false);
    }

    private void SetWalkState()
    {
        anim.SetBool("isIdle", false);
        anim.SetBool("isWalk", true);
        anim.SetBool("isAttack", false);
    }

    private void SetAttackState()
    {
        anim.SetBool("isIdle", false);
        anim.SetBool("isWalk", false);
        anim.SetBool("isAttack", true);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = true;
            player = collision.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            isPlayerInRange = false;
            rb.velocity = Vector2.zero;
            SetIdleState();
        }
    }
}