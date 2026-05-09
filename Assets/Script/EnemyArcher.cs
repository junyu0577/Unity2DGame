using UnityEngine;

/// <summary>
/// 弓箭手敌人 - 远程攻击（独立实现，不继承Enemy）
/// </summary>
public class EnemyArcher : MonoBehaviour
{
    [Header("移动设置")]
    [Tooltip("敌人移动速度")]
    public float moveSpeed = 2f;

    [Tooltip("攻击距离")]
    public float attackDistance = 3f;

    [Header("远程攻击设置")]
    [Tooltip("箭矢预制体")]
    public GameObject arrowPrefab;

    // 缓存arrowPrefab，避免被场景引用影响
    private GameObject _cachedArrowPrefab;

    [Tooltip("箭矢伤害")]
    public int arrowDamage = 1;

    [Tooltip("箭矢飞行速度")]
    public float arrowSpeed = 10f;

    [Tooltip("箭矢图片（可选）")]
    public Sprite arrowSprite;

    [Tooltip("攻击点偏移")]
    public Vector2 attackOffset = new Vector2(0.5f, 0);

    [Header("组件")]
    private Animator anim;
    private SpriteRenderer sr;
    private Rigidbody2D rb;

    private Transform player;
    private Transform playerBase;
    private bool isPlayerInRange;
    private bool isAttacking;
    private float attackCoolDown = 1.5f;
    private float lastAttackTime;
    private float attackStartTime;
    private float attackDuration = 2f;

    // 受击变红相关
    private int lastHealth;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.freezeRotation = true;
        rb.isKinematic = true;
        Debug.Log($"[EnemyArcher] Awake - 实例ID={GetInstanceID()}, name={gameObject.name}");

        // 获取EnemyHealth组件
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null)
        {
            lastHealth = health.currentHealth;
        }
    }

    private void OnEnable()
    {
        Debug.Log($"[EnemyArcher] OnEnable - 实例ID={GetInstanceID()}, name={gameObject.name}, arrowPrefab={(arrowPrefab == null ? "NULL!!" : arrowPrefab.name)}");
    }

    private void OnDisable()
    {
        Debug.Log($"[EnemyArcher] OnDisable - 实例ID={GetInstanceID()}, name={gameObject.name}");
    }

    private void Start()
    {
        SetIdleState();
        TryFindTargets();

        // 缓存arrowPrefab（如果是场景对象引用，会在运行时失效）
        _cachedArrowPrefab = arrowPrefab;
        Debug.Log($"[EnemyArcher] Start - 实例ID={GetInstanceID()}, name={gameObject.name}, arrowPrefab={(arrowPrefab == null ? "NULL!!" : arrowPrefab.name)}");
    }

    private void TryFindTargets()
    {
        // 查找玩家
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }

        // 查找基地
        PlayerBase baseComponent = FindObjectOfType<PlayerBase>();
        if (baseComponent != null)
        {
            playerBase = baseComponent.transform;
        }
    }

    /// <summary>
    /// 检测受击（变红）
    /// </summary>
    private void CheckHit()
    {
        EnemyHealth health = GetComponent<EnemyHealth>();
        if (health != null && health.currentHealth < lastHealth)
        {
            // 受到伤害，变红
            if (sr != null)
            {
                sr.color = Color.red;
                Invoke("ResetColor", 0.1f);
                Debug.Log("[EnemyArcher] 受击！变红");
            }
        }

        if (health != null)
        {
            lastHealth = health.currentHealth;
        }
    }

    /// <summary>
    /// 恢复颜色
    /// </summary>
    private void ResetColor()
    {
        if (sr != null)
        {
            sr.color = Color.white;
        }
    }

    /// <summary>
    /// 开始追踪（由窝点调用）
    /// </summary>
    public void StartChasing()
    {
        TryFindTargets();
        isPlayerInRange = true;
        Debug.Log($"{gameObject.name} 开始追踪！");
    }

    private void Update()
    {
        // 调试：显示所有EnemyArcher实例状态
        if (Input.GetKeyDown(KeyCode.F1))
        {
            EnemyArcher[] archers = FindObjectsOfType<EnemyArcher>();
            foreach (var a in archers)
            {
                Debug.Log($"[EnemyArcher] {a.gameObject.name} arrowPrefab = {(a.arrowPrefab == null ? "NULL!!" : a.arrowPrefab.name)}");
            }
        }

        // 检测受击（变红）
        CheckHit();

        Transform target = GetTarget();
        if (target == null) return;

        if (isAttacking) return;

        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        // 攻击冷却中
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

    private Transform GetTarget()
    {
        // 优先攻击基地
        if (playerBase != null)
        {
            PlayerBase baseComponent = playerBase.GetComponent<PlayerBase>();
            if (baseComponent != null && !baseComponent.IsDestroyed)
            {
                return playerBase;
            }
        }
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
        attackStartTime = Time.time;
        rb.velocity = Vector2.zero;

        // 面朝目标
        if (target != null)
        {
            Vector2 direction = (target.position - transform.position).normalized;
            if (direction.x > 0)
                sr.flipX = false;
            else if (direction.x < 0)
                sr.flipX = true;
        }

        SetAttackState();
        lastAttackTime = Time.time;
    }

    /// <summary>
    /// 远程射击（由Animation Event调用）
    /// </summary>
    public void ShootArrow()
    {
        // 使用缓存的arrowPrefab（避免被场景引用影响）
        GameObject prefabToUse = _cachedArrowPrefab != null ? _cachedArrowPrefab : arrowPrefab;

        // 调试日志
        Debug.Log($"[EnemyArcher] ShootArrow 调用！实例={GetInstanceID()}, arrowPrefab={(prefabToUse == null ? "NULL!!" : prefabToUse.name)}");

        if (prefabToUse == null)
        {
            Debug.LogError($"[EnemyArcher] {gameObject.name} (ID:{GetInstanceID()}) arrowPrefab未设置！");
            return;
        }

        Transform target = GetTarget();
        Debug.Log($"[EnemyArcher] 目标: {(target == null ? "null" : target.name)}");

        if (target == null)
        {
            Debug.Log("[EnemyArcher] 没有目标，取消射击");
            return;
        }

        // 计算发射位置
        Vector2 offset = attackOffset;
        if (sr != null && sr.flipX)
        {
            offset.x = -attackOffset.x;
        }
        Vector2 shootPos = (Vector2)transform.position + offset;

        Debug.Log($"[EnemyArcher] 发射位置: {shootPos}, 目标位置: {target.position}");

        // 生成箭矢
        GameObject arrowObj = Instantiate(prefabToUse, shootPos, Quaternion.identity);
        Arrow arrow = arrowObj.GetComponent<Arrow>();
        if (arrow != null)
        {
            arrow.damage = arrowDamage;
            arrow.speed = arrowSpeed;
            arrow.Launch(shootPos, target, arrowSprite);
            Debug.Log("[EnemyArcher] 箭矢已生成并发射！");
        }
        else
        {
            Debug.LogError("[EnemyArcher] 箭矢预制体上没有 Arrow 组件！");
        }
    }

    /// <summary>
    /// Animation Event: 攻击瞬间（调用 ShootArrow）
    /// </summary>
    public void OnAttack()
    {
        Debug.Log($"[EnemyArcher] OnAttack 调用！实例ID={GetInstanceID()}");
        ShootArrow();
    }

    /// <summary>
    /// 攻击结束（由Animation Event调用）
    /// </summary>
    public void OnAttackEnd()
    {
        isAttacking = false;
        SetIdleState();
    }

    private void SetIdleState()
    {
        if (anim != null)
        {
            anim.SetBool("isIdle", true);
            anim.SetBool("isWalk", false);
            anim.SetBool("isAttack", false);
        }
    }

    private void SetWalkState()
    {
        if (anim != null)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalk", true);
            anim.SetBool("isAttack", false);
        }
    }

    private void SetAttackState()
    {
        if (anim != null)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isWalk", false);
            anim.SetBool("isAttack", true);
        }
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

    private void OnDrawGizmosSelected()
    {
        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        // 绘制射击点
        if (sr != null)
        {
            Vector2 offset = sr.flipX ? new Vector2(-attackOffset.x, attackOffset.y) : attackOffset;
            Vector2 attackPos = (Vector2)transform.position + offset;
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(attackPos, 0.2f);
        }
    }
}