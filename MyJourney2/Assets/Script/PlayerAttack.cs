using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerAttack : MonoBehaviour
{
    [Header("攻击设置")]
    [SerializeField] private KeyCode attackKey = KeyCode.J;

    [Tooltip("玩家攻击力")]
    public int attackDamage = 1;

    [Tooltip("攻击范围半径")]
    public float attackRange = 1f;

    [Tooltip("攻击点偏移")]
    public Vector2 attackOffset = new Vector2(0.5f, 0);

    [Tooltip("攻击冷却时间")]
    public float attackCooldown = 0.5f;

    [Header("组件")]
    public Transform attackPoint;
    public Animator anim;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private PlayerMove playerMove;
    private bool isAttacking;
    private float lastAttackTime;
    private int enemyLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        enemyLayer = LayerMask.NameToLayer("Enemy");

        // 获取 PlayerMove 组件
        playerMove = GetComponent<PlayerMove>();
    }

    private void Start()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        // 如果没有指定攻击点，使用自身
        if (attackPoint == null)
        {
            GameObject pointObj = new GameObject("AttackPoint");
            pointObj.transform.SetParent(transform);
            pointObj.transform.localPosition = Vector3.zero;
            attackPoint = pointObj.transform;
        }
    }

    private void Update()
    {
        // 按下攻击键，且当前不在攻击状态
        if (Input.GetKeyDown(attackKey) && !isAttacking)
        {
            Attack();
        }
    }

    public void Attack()
    {
        // 攻击冷却中，不能攻击

        if (Time.time - lastAttackTime < attackCooldown)
        {
            Debug.Log("[PlayerState] 攻击失败：冷却中");
            return;
        }
        // 正在攻击中，不能攻击
        if (isAttacking)
        {
            Debug.Log("[PlayerState] 攻击失败：正在攻击");
            return;
        }

        Debug.Log("[PlayerState] ===== 攻击开始 =====");
        isAttacking = true;
        lastAttackTime = Time.time;

        // 通知 PlayerMove 停止移动
        if (playerMove != null)
        {
            playerMove.isAttacking = true;
        }

        // 设置攻击动画
        anim.SetBool("isAttack", true);

        // 根据朝向调整攻击点偏移
        Vector2 offset = attackOffset;
        if (sr != null && sr.flipX)
        {
            offset.x = -attackOffset.x;
        }

        if (attackPoint != null)
        {
            attackPoint.localPosition = (Vector3)offset;
        }
    }

    // Animation Event: 伤害结算（攻击命中瞬间调用）
    public void OnAttack()
    {
        Debug.Log("[PlayerState] OnAttack 触发！攻击点位置: " + attackPoint.position + ", 范围: " + attackRange);

        // 检测范围内的敌人（单体攻击：只攻击最近的一个）
        Collider2D closestEnemy = Physics2D.OverlapCircle(
            (Vector2)attackPoint.position,
            attackRange,
            1 << enemyLayer
        );

        if (closestEnemy != null)
        {
            // 检查是否是友军基地（不可被攻击）
            PlayerBase playerBase = closestEnemy.GetComponent<PlayerBase>();
            if (playerBase != null && !playerBase.canBeAttackedByPlayer)
            {
                Debug.Log("[PlayerState] 基地不可被攻击，忽略");
                return;
            }

            Debug.Log("[PlayerState] 命中敌人: " + closestEnemy.gameObject.name);

            EnemyHealth health = closestEnemy.GetComponent<EnemyHealth>();
            if (health != null)
            {
                // 计算伤害
                int finalDamage = attackDamage;

                // 检查暴击
                bool isCritical = false;
                if (PlayerLevelSystem.Instance != null)
                {
                    isCritical = PlayerLevelSystem.Instance.IsCritical();
                    if (isCritical)
                    {
                        finalDamage = Mathf.RoundToInt(attackDamage * PlayerLevelSystem.Instance.GetCriticalDamageMultiplier());
                        Debug.Log($"[PlayerState] 暴击！伤害从 {attackDamage} 提升到 {finalDamage}");
                    }
                }

                health.TakeDamage(finalDamage, true, isCritical);
                Debug.Log("[PlayerState] 敌人受伤!");
            }
        }
        else
        {
            Debug.Log("[PlayerState] 攻击落空");
        }

        // isAttacking 在 OnAttackEnd 中重置，不要在这里重置
    }

    // Animation Event: 攻击结束（动画播放完毕后调用）
    public void OnAttackEnd()
    {
        Debug.Log("[PlayerState] ===== 攻击结束 =====");
        isAttacking = false;
        anim.SetBool("isAttack", false);

        // 通知 PlayerMove 恢复移动
        if (playerMove != null)
        {
            playerMove.isAttacking = false;
            Debug.Log("[PlayerState] 恢复移动");
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (attackPoint == null) return;

        // 绘制攻击范围
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere((Vector2)attackPoint.position, attackRange);

        // 绘制攻击点
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)attackPoint.position, 0.1f);
    }
}