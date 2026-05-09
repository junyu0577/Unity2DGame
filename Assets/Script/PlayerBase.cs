using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerBase : MonoBehaviour
{
    [Header("基地设置")]
    [Tooltip("基地最大生命值")]
    public int maxHealth = 10;

    [Tooltip("是否可以被玩家攻击")]
    public bool canBeAttackedByPlayer = true;

    [Header("图片设置")]
    [Tooltip("基地被摧毁后的背景图片")]
    public Sprite destroyedBackground;

    [Header("UI组件")]
    [Tooltip("背景图片（如果是UI显示）")]
    public Image backgroundImage;

    private EnemyHealth enemyHealth;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isDestroyed = false;
    private int lastHealth;

    /// <summary>
    /// 基地是否已被摧毁
    /// </summary>
    public bool IsDestroyed => isDestroyed;

    private void Awake()
    {
        // 添加 EnemyHealth 组件
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.maxHealth = maxHealth;
            enemyHealth.currentHealth = maxHealth;
            // 拦截死亡事件
            enemyHealth.onDeath = OnBaseDestroyed;
        }

        // 获取 SpriteRenderer
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 获取 Animator（如果有）
        animator = GetComponent<Animator>();

        // 设置为可以被攻击的层级（复用 Enemy 层）
        gameObject.layer = LayerMask.NameToLayer("Enemy");
    }

    private void Start()
    {
        lastHealth = maxHealth;
        Debug.Log($"[PlayerBase] 基地创建完成，血量: {maxHealth}");
    }

    private void Update()
    {
        // 检测受击（未摧毁时）
        if (!isDestroyed && enemyHealth != null && enemyHealth.currentHealth < lastHealth)
        {
            // 受到伤害，变红
            OnHit();
        }

        if (enemyHealth != null)
        {
            lastHealth = enemyHealth.currentHealth;
        }
    }

    /// <summary>
    /// 受击时变红（仅在未摧毁时生效）
    /// </summary>
    private void OnHit()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            Invoke("ResetColor", 0.1f);
            Debug.Log("[PlayerBase] 基地受击！变红");
        }
    }

    /// <summary>
    /// 恢复颜色
    /// </summary>
    private void ResetColor()
    {
        if (spriteRenderer == null) return;

        if (isDestroyed)
        {
            // 已被摧毁，使用损坏后的图片颜色（白色或恢复为正常）
            spriteRenderer.color = Color.white;
        }
        else
        {
            spriteRenderer.color = Color.white;
        }
    }

    /// <summary>
    /// 基地被摧毁
    /// </summary>
    private void OnBaseDestroyed()
    {
        if (isDestroyed) return;
        isDestroyed = true;

        Debug.Log("[PlayerBase] 基地已被摧毁！");

        // 禁用 Animator（这样 SpriteRenderer 才能显示设置的图片）
        if (animator != null)
        {
            animator.enabled = false;
        }

        // 切换背景图片
        ChangeBackground();

        // 禁用碰撞体
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // 可以在这里触发游戏结束等其他逻辑
    }

    /// <summary>
    /// 切换背景图片
    /// </summary>
    private void ChangeBackground()
    {
        // 方式1：如果有设置 SpriteRenderer，切换图片
        if (destroyedBackground != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = destroyedBackground;
        }

        // 方式2：如果有设置 UI Image，切换图片
        if (destroyedBackground != null && backgroundImage != null)
        {
            backgroundImage.sprite = destroyedBackground;
        }

        Debug.Log("[PlayerBase] 背景已切换");
    }

    private void OnDrawGizmosSelected()
    {
        // 绘制攻击范围
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}