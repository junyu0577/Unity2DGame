using UnityEngine;

public class Arrow : MonoBehaviour
{
    [Header("箭矢设置")]
    [Tooltip("箭矢飞行速度")]
    public float speed = 10f;

    [Tooltip("箭矢伤害")]
    public int damage = 1;

    [Tooltip("箭矢存活时间（秒）")]
    public float lifetime = 3f;

    [Tooltip("发射后延迟检测碰撞的时间（秒）")]
    public float collisionDelay = 0.1f;

    [Tooltip("箭矢图片")]
    public Sprite arrowSprite;

    [Tooltip("是否是玩家发射的箭（玩家发射的需要检查基地是否可被攻击）")]
    public bool isPlayerArrow = false;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 direction;
    private Transform target;
    private bool hasLaunched = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        rb.gravityScale = 0;
        rb.isKinematic = true;

        // 自动销毁
        Destroy(gameObject, lifetime);
    }

    /// <summary>
    /// 发射箭矢
    /// </summary>
    /// <param name="startPos">起始位置</param>
    /// <param name="targetTransform">目标Transform</param>
    /// <param name="arrowSprite">箭矢图片（可选）</param>
    public void Launch(Vector2 startPos, Transform targetTransform, Sprite arrowSprite = null)
    {
        Debug.Log($"[Arrow] Launch 被调用！位置: {startPos}, 目标: {(targetTransform == null ? "null" : targetTransform.name)}, SpriteRenderer: {(sr == null ? "null" : "存在")}");

        transform.position = startPos;
        target = targetTransform;

        // 设置箭矢图片
        if (arrowSprite != null && sr != null)
        {
            sr.sprite = arrowSprite;
            Debug.Log($"[Arrow] 已设置箭矢图片");
        }
        else if (sr != null && sr.sprite != null)
        {
            Debug.Log($"[Arrow] 使用预制体已有图片: {sr.sprite.name}");
        }

        // 设置初始方向（目标位置 - 起始位置）
        if (target != null)
        {
            direction = ((Vector2)target.position - startPos).normalized;
            Debug.Log($"[Arrow] 方向: {direction}");

            // 根据方向翻转图片（箭头朝右为默认）
            if (sr != null)
            {
                sr.flipX = direction.x < 0;
            }
        }

        // 开始移动
        rb.velocity = direction * speed;
        Debug.Log($"[Arrow] 速度: {rb.velocity}");

        // 旋转箭矢方向，使箭頭朝向目标
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        Debug.Log($"[Arrow] 旋转角度: {angle}");

        // 延迟开启碰撞检测，避免生成时立即击中
        Invoke("EnableCollision", collisionDelay);
    }

    private void EnableCollision()
    {
        hasLaunched = true;
        Debug.Log("[Arrow] 碰撞检测已开启");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasLaunched) return;

        // 碰到目标（玩家或基地）
        if (collision.CompareTag("Player"))
        {
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null)
            {
                health.TakeDamage(damage);
                Debug.Log("[Arrow] 射中玩家！");
            }
            Destroy(gameObject);
        }
        else if (collision.GetComponent<PlayerBase>() != null)
        {
            // 如果是玩家发射的箭，检查基地是否可被玩家攻击
            if (isPlayerArrow)
            {
                PlayerBase playerBase = collision.GetComponent<PlayerBase>();
                if (playerBase != null && !playerBase.canBeAttackedByPlayer)
                {
                    Debug.Log("[Arrow] 基地不可被玩家攻击，忽略");
                    Destroy(gameObject);
                    return;
                }
            }

            EnemyHealth health = collision.GetComponent<EnemyHealth>();
            if (health != null)
            {
                health.TakeDamage(damage, false);  // 攻击基地不显示伤害数字
                Debug.Log("[Arrow] 射中基地！");
            }
            Destroy(gameObject);
        }
        else if (!collision.isTrigger)
        {
            // 碰到墙壁或其他物体，销毁箭矢
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // 绘制箭矢飞行方向
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)direction * 2f);
    }
}