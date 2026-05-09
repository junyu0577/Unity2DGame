using UnityEngine;

/// <summary>
/// 蘑菇怪物巡逻脚本：在指定路径点上移动
/// </summary>
public class MushroomPatrol : MonoBehaviour
{
    [Header("巡逻设置")]
    [Tooltip("巡逻路径点")]
    public Transform[] patrolPoints;

    [Tooltip("移动速度")]
    public float moveSpeed = 2f;

    [Tooltip("停留时间（秒）")]
    public float waitTime = 1f;

    [Header("组件")]
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Animator anim;

    private int currentPointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();

        if (rb != null)
        {
            rb.gravityScale = 0;
            rb.freezeRotation = true;
            rb.isKinematic = true;
        }
    }

    private void Start()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("[MushroomPatrol] 没有设置巡逻点！");
            return;
        }

        // 从第一个点开始
        transform.position = patrolPoints[0].position;
    }

    private void Update()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        // 等待中
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTime)
            {
                isWaiting = false;
                waitTimer = 0f;
                // 移动到下一个点
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
                // 开始巡逻，切换到 patrol 状态
                SetPatrolState();
            }
            return;
        }

        // 移动向目标点
        MoveToTarget();
    }

    private void MoveToTarget()
    {
        Transform targetPoint = patrolPoints[currentPointIndex];
        Vector2 direction = (targetPoint.position - transform.position).normalized;

        // 移动
        rb.velocity = direction * moveSpeed;

        // 面向方向（向右走时水平镜像）
        if (direction.x > 0)
        {
            sr.flipX = true;
        }
        else if (direction.x < 0)
        {
            sr.flipX = false;
        }

        // 到达目标点
        float distance = Vector2.Distance(transform.position, targetPoint.position);
        if (distance < 0.1f)
        {
            transform.position = targetPoint.position;
            rb.velocity = Vector2.zero;
            isWaiting = true;
            // 到达等待，切换到 idle 状态
            SetIdleState();
            Debug.Log($"[MushroomPatrol] 到达巡逻点 {currentPointIndex}，等待 {waitTime} 秒");
        }
    }

    /// <summary>
    /// 设置 idle 状态
    /// </summary>
    private void SetIdleState()
    {
        if (anim != null)
        {
            anim.SetBool("isIdle", true);
            anim.SetBool("isPatrol", false);
        }
    }

    /// <summary>
    /// 设置 patrol 状态
    /// </summary>
    private void SetPatrolState()
    {
        if (anim != null)
        {
            anim.SetBool("isIdle", false);
            anim.SetBool("isPatrol", true);
        }
    }

    /// <summary>
    /// 绘制巡逻路径（编辑器中可见）
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Gizmos.color = Color.green;

        // 绘制路径点
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] != null)
            {
                Gizmos.DrawWireSphere(patrolPoints[i].position, 0.3f);

                // 连接路径
                if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                {
                    Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                }
            }
        }

        // 连接首尾
        if (patrolPoints.Length > 1 && patrolPoints[0] != null && patrolPoints[patrolPoints.Length - 1] != null)
        {
            Gizmos.DrawLine(patrolPoints[patrolPoints.Length - 1].position, patrolPoints[0].position);
        }
    }
}