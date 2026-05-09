using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    [Header("生命值设置")]
    public int maxHealth = 1;
    public int currentHealth;

    [Header("伤害数字设置")]
    [Tooltip("伤害数字预制体")]
    public GameObject damageNumberPrefab;

    [Tooltip("暴击伤害数字预制体")]
    public GameObject damageNumberDoublePrefab;

    [Tooltip("伤害数字显示在头顶的偏移量")]
    public float damageNumberHeightOffset = 0.5f;

    [Tooltip("是否自动根据物体大小计算偏移")]
    public bool autoCalculateOffset = true;

    [Tooltip("自动计算时的额外偏移")]
    public float autoOffsetExtra = 0.3f;

    /// <summary>
    /// 死亡回调（可外部赋值自定义死亡逻辑）
    /// </summary>
    public System.Action onDeath;

    private void Awake()
    {
        currentHealth = maxHealth;

        // 如果没有指定预制体，尝试自动获取
        if (damageNumberPrefab == null)
        {
            damageNumberPrefab = Resources.Load<GameObject>("DamageNumber");
        }

        if (damageNumberDoublePrefab == null)
        {
            damageNumberDoublePrefab = Resources.Load<GameObject>("DamageNumberDouble");
        }
    }

    public void TakeDamage(int damage, bool showDamageNumber = true, bool isCritical = false)
    {
        currentHealth -= damage;

        // 显示伤害数字
        if (showDamageNumber)
        {
            ShowDamageNumber(damage, isCritical);
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    /// <summary>
    /// 显示伤害数字
    /// </summary>
    private void ShowDamageNumber(int damage, bool isCritical = false)
    {
        // 选择预制体
        GameObject prefabToUse = isCritical ? damageNumberDoublePrefab : damageNumberPrefab;
        string prefabName = isCritical ? "DamageNumberDouble" : "DamageNumber";

        Debug.Log($"[EnemyHealth] ShowDamageNumber 开始，isCritical={isCritical}, prefab={prefabToUse}, prefabName={prefabName}");

        if (damageNumberPrefab == null)
        {
            Debug.LogWarning("[EnemyHealth] 伤害数字预制体未设置！");
            return;
        }

        Debug.Log($"[EnemyHealth] 显示伤害数字: {damage}, 位置: {transform.position}");

        // 计算偏移位置
        Vector2 offset = new Vector2(0, damageNumberHeightOffset);
        if (autoCalculateOffset)
        {
            offset = CalculateAutoOffset();
        }

        // 打印Collider边界信息
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            Bounds bounds = collider.bounds;
            Debug.Log($"[EnemyHealth] Collider边界: min={bounds.min}, max={bounds.max}, extents={bounds.extents}");
        }

        Vector3 damagePosition = transform.position + (Vector3)offset;
        Debug.Log($"[EnemyHealth] 怪物位置: {transform.position}, 伤害数字位置: {damagePosition}, 偏移: {offset}");

        Debug.Log($"[EnemyHealth] 伤害数字偏移: {offset}");

        // 创建伤害数字实例
        GameObject damageObj = Instantiate(prefabToUse);
        Debug.Log($"[EnemyHealth] Instantiate 完成");

        // 获取伤害数字的 RectTransform
        RectTransform rectTransform = damageObj.GetComponent<RectTransform>();
        if (rectTransform == null)
        {
            Debug.LogWarning("[EnemyHealth] 伤害数字没有 RectTransform 组件！");
            return;
        }

        // 查找场景中的 Canvas
        Canvas canvas = FindObjectOfType<Canvas>();
        Debug.Log($"[EnemyHealth] 找到 Canvas: {canvas}");

        if (canvas != null)
        {
            // 设置为 Canvas 的子对象
            damageObj.transform.SetParent(canvas.transform, false);

            // 尝试获取主相机
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }

            if (mainCamera == null)
            {
                Debug.LogWarning("[EnemyHealth] 没有找到相机！");
                return;
            }

            Debug.Log($"[EnemyHealth] 找到相机: {mainCamera}");

            // 将世界坐标转换为屏幕坐标
            Vector3 screenPos = mainCamera.WorldToScreenPoint(damagePosition);
            Debug.Log($"[EnemyHealth] 屏幕位置: {screenPos}");

            // 根据 Canvas 渲染模式处理
            if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            {
                // Screen Space Overlay 模式：计算 UI 坐标
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(
                        canvasRect,
                        screenPos,
                        null,
                        out Vector2 localPoint
                    );
                    rectTransform.anchoredPosition = localPoint;
                    Debug.Log($"[EnemyHealth] UI 位置设置完成，anchoredPosition: {localPoint}");
                }
            }
            else
            {
                // 其他模式：直接使用屏幕坐标
                rectTransform.position = screenPos;
                Debug.Log($"[EnemyHealth] 使用屏幕坐标模式");
            }
        }
        else
        {
            Debug.LogWarning("[EnemyHealth] 场景中没有找到 Canvas！");
            return;
        }

        // 调用显示方法
        DamageNumber damageNumber = damageObj.GetComponent<DamageNumber>();
        if (damageNumber != null)
        {
            Debug.Log($"[EnemyHealth] 调用 DamageNumber.Show");
            damageNumber.Show(damagePosition, damage);
        }
        else
        {
            Debug.LogWarning("[EnemyHealth] DamageNumber 组件未找到！");
        }
    }

    /// <summary>
    /// 自动计算偏移量
    /// </summary>
    private Vector2 CalculateAutoOffset()
    {
        float height = 0.5f;

        // 优先使用 CapsuleCollider2D
        CapsuleCollider2D capsule = GetComponent<CapsuleCollider2D>();
        if (capsule != null)
        {
            height = capsule.size.y * 0.5f;
            Debug.Log($"[EnemyHealth] 使用 CapsuleCollider2D 计算，高度: {height}");
            return new Vector2(0, height + autoOffsetExtra);
        }

        // 其次使用其他 Collider2D 的边界
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            height = collider.bounds.extents.y;
            return new Vector2(0, height + autoOffsetExtra);
        }

        // 使用 SpriteRenderer 的大小
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            height = spriteRenderer.bounds.extents.y;
            return new Vector2(0, height + autoOffsetExtra);
        }

        // 使用默认值
        return new Vector2(0, damageNumberHeightOffset);
    }

    private void Die()
    {
        Debug.Log(gameObject.name + " 已死亡");

        // 掉落金币
        if (GameManager.Instance != null)
        {
            GameManager.Instance.AddGold(1);
        }

        // 掉落经验
        if (PlayerLevelSystem.Instance != null)
        {
            PlayerLevelSystem.Instance.AddExperience(1);
        }

        // 如果有自定义死亡回调，调用它；否则默认销毁
        if (onDeath != null)
        {
            onDeath.Invoke();
        }
        else
        {
            Destroy(gameObject);
        }
    }
}