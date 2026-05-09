using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Slime 投射物：飞向玩家基地
/// </summary>
public class SlimeProjectile : MonoBehaviour
{
    [Header("飞行设置")]
    [Tooltip("飞行速度")]
    public float speed = 5f;

    [Tooltip("伤害值")]
    public int damage = 1;

    [Tooltip("旋转速度（度/秒），0为不旋转")]
    public float rotationSpeed = 0f;

    [Header("暴击设置")]
    [Tooltip("暴击几率 (0-1)")]
    public float critChance = 0.4f;

    [Tooltip("暴击伤害倍率")]
    public float critMultiplier = 2f;

    [Header("伤害数字预制体")]
    [Tooltip("普通伤害数字")]
    public GameObject damageNumberPrefab;

    [Tooltip("暴击伤害数字")]
    public GameObject damageNumberDoublePrefab;

    [Tooltip("暴击伤害数字的Y轴偏移量")]
    public float critYOffset = 0f;

    [Header("组件")]
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // 静态伤害数字预制体（用于Resources加载）
    private static GameObject staticDamageNumberPrefab;
    private static GameObject staticDamageNumberDoublePrefab;

    // 对象池相关
    private bool isFromPool = false;

    // 飞行方向
    private Vector2 direction;

    // 所属对象池
    private SlimeProjectilePool ownerPool;

    // 攻击目标类型
    private ProjectileTarget targetType;

    // 静态标记是否已设置layer碰撞
    private static bool layerCollisionSetup = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        Debug.Log($"[SlimeProjectile] Awake - {gameObject.name}, RB={rb != null}");
    }

    private void OnEnable()
    {
        // 设置Layer并忽略同类碰撞（只执行一次）
        if (!layerCollisionSetup)
        {
            SetupLayerCollision();
            layerCollisionSetup = true;
        }

        gameObject.layer = LayerMask.NameToLayer("SlimeProjectile");
        Debug.Log($"[SlimeProjectile] OnEnable - {gameObject.name}");
    }

    /// <summary>
    /// 设置Layer忽略同类碰撞
    /// </summary>
    private void SetupLayerCollision()
    {
        // 创建 SlimeProjectile Layer（如果不存在）
        int layer = LayerMask.NameToLayer("SlimeProjectile");
        if (layer == -1)
        {
            Debug.LogWarning("[SlimeProjectile] SlimeProjectile Layer 不存在，请在 Unity 中添加！");
            return;
        }

        // 忽略 SlimeProjectile 层与自身的碰撞
        Physics2D.IgnoreLayerCollision(layer, layer, true);
        Debug.Log("[SlimeProjectile] Layer碰撞已设置");
    }

    private void OnDisable()
    {
        Debug.Log($"[SlimeProjectile] OnDisable - {gameObject.name}");
    }

    private void Update()
    {
        // 飞向目标方向
        if (rb != null && direction != Vector2.zero)
        {
            rb.velocity = direction.normalized * speed;
        }

        // 旋转
        if (rotationSpeed != 0f)
        {
            transform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[SlimeProjectile] OnTriggerEnter2D - 碰撞到: {collision.gameObject.name}, 目标类型: {targetType}");

        // 忽略其他投射物的碰撞
        if (collision.GetComponent<SlimeProjectile>() != null)
        {
            return;
        }

        // 忽略 Slime 怪物的碰撞
        if (collision.GetComponent<SlimeController>() != null)
        {
            return;
        }

        // 根据目标类型处理碰撞
        bool shouldAttack = false;
        string targetName = "";

        switch (targetType)
        {
            case ProjectileTarget.PlayerBase:
                if (collision.GetComponent<PlayerBase>() != null)
                {
                    shouldAttack = true;
                    targetName = "基地";
                }
                break;

            case ProjectileTarget.Player:
                if (IsPlayer(collision.gameObject))
                {
                    shouldAttack = true;
                    targetName = "玩家";
                }
                break;

            case ProjectileTarget.Both:
                if (collision.GetComponent<PlayerBase>() != null || IsPlayer(collision.gameObject))
                {
                    shouldAttack = true;
                    targetName = collision.GetComponent<PlayerBase>() != null ? "基地" : "玩家";
                }
                break;
        }

        if (shouldAttack)
        {
            EnemyHealth enemyHealth = collision.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                // 计算暴击
                int finalDamage = damage;
                bool isCrit = Random.value < critChance;
                if (isCrit)
                {
                    finalDamage = Mathf.RoundToInt(damage * critMultiplier);
                }

                // 传入 false 避免 EnemyHealth 再显示伤害数字，由 SlimeProjectile 统一控制
                enemyHealth.TakeDamage(finalDamage, false);

                // 显示伤害数字
                ShowDamageNumber(collision.transform.position, finalDamage, isCrit, collision);

                Debug.Log($"[SlimeProjectile] 击中{targetName}，造成 {(isCrit ? "暴击 " : "")}{finalDamage} 点伤害");
            }
            ReturnToPool();
        }
        else
        {
            Debug.Log($"[SlimeProjectile] 未击中有效目标，回收: {collision.gameObject.name}");
            ReturnToPool();
        }
    }

    /// <summary>
    /// 判断是否是玩家
    /// </summary>
    private bool IsPlayer(GameObject obj)
    {
        MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
        foreach (var comp in components)
        {
            if (comp.GetType().Name.Contains("Player"))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 显示伤害数字
    /// </summary>
    private void ShowDamageNumber(Vector3 position, int damage, bool isCrit, Collider2D targetCollider)
    {
        Debug.Log($"[SlimeProjectile] ShowDamageNumber - isCrit={isCrit}, damage={damage}");

        // 确定使用哪个预制体
        GameObject prefabToUse = null;
        string prefabName = "";

        if (isCrit)
        {
            // 优先使用Inspector中指定的预制体，否则从Resources加载
            if (damageNumberDoublePrefab != null)
            {
                prefabToUse = damageNumberDoublePrefab;
                prefabName = damageNumberDoublePrefab.name;
                Debug.Log("[SlimeProjectile] 使用Inspector指定的暴击预制体: " + prefabName);
            }
            else
            {
                if (staticDamageNumberDoublePrefab == null)
                {
                    staticDamageNumberDoublePrefab = Resources.Load<GameObject>("DamageNumberDouble");
                    Debug.Log("[SlimeProjectile] 从Resources加载暴击预制体, 结果: " + (staticDamageNumberDoublePrefab != null ? staticDamageNumberDoublePrefab.name : "null"));
                }
                prefabToUse = staticDamageNumberDoublePrefab;
                prefabName = prefabToUse != null ? prefabToUse.name : "null";
            }
        }
        else
        {
            if (damageNumberPrefab != null)
            {
                prefabToUse = damageNumberPrefab;
                prefabName = damageNumberPrefab.name;
                Debug.Log("[SlimeProjectile] 使用Inspector指定的普通预制体: " + prefabName);
            }
            else
            {
                if (staticDamageNumberPrefab == null)
                {
                    staticDamageNumberPrefab = Resources.Load<GameObject>("DamageNumber");
                    Debug.Log("[SlimeProjectile] 从Resources加载普通预制体, 结果: " + (staticDamageNumberPrefab != null ? staticDamageNumberPrefab.name : "null"));
                }
                prefabToUse = staticDamageNumberPrefab;
                prefabName = prefabToUse != null ? prefabToUse.name : "null";
            }
        }

        Debug.Log($"[SlimeProjectile] 最终使用预制体: {prefabName}");

        if (prefabToUse == null)
        {
            Debug.LogWarning("[SlimeProjectile] 伤害数字预制体为空！");
            return;
        }

        // 计算偏移位置：与 EnemyHealth 保持一致
        float heightOffset = 0.5f;
        // 尝试获取碰撞体的偏移量
        Collider2D collider = targetCollider;
        if (collider != null)
        {
            heightOffset = collider.bounds.extents.y + 0.3f;
        }
        else
        {
            SpriteRenderer sr = targetCollider.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                heightOffset = sr.bounds.extents.y + 0.3f;
            }
        }

        // 暴击时增加额外的Y偏移
        float critOffset = isCrit ? critYOffset : 0f;
        Vector3 displayPosition = position + Vector3.up * (heightOffset + critOffset);

        Debug.Log($"[SlimeProjectile] 显示位置 - collision位置: {position}, 基础偏移: {heightOffset}, 暴击偏移: {critOffset}, 最终: {displayPosition}");

        GameObject damageObj = Instantiate(prefabToUse, displayPosition, Quaternion.identity);

        // 处理 Canvas 显示
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas != null)
        {
            damageObj.transform.SetParent(canvas.transform, false);

            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindObjectOfType<Camera>();
            }

            if (mainCamera != null)
            {
                Vector3 screenPos = mainCamera.WorldToScreenPoint(displayPosition);

                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                    if (canvasRect != null)
                    {
                        RectTransformUtility.ScreenPointToLocalPointInRectangle(
                            canvasRect,
                            screenPos,
                            null,
                            out Vector2 localPoint
                        );

                        RectTransform rectTransform = damageObj.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            rectTransform.anchoredPosition = localPoint;

                            // 暴击时设置为 sibling 的最后一个，确保显示在最上层
                            if (isCrit)
                            {
                                damageObj.transform.SetAsLastSibling();
                            }
                        }
                    }
                }
                else
                {
                    RectTransform rectTransform = damageObj.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.position = screenPos;
                        if (isCrit)
                        {
                            damageObj.transform.SetAsLastSibling();
                        }
                    }
                }
            }
        }

        DamageNumber damageNumber = damageObj.GetComponent<DamageNumber>();
        if (damageNumber != null)
        {
            damageNumber.Show(displayPosition, damage);
        }
    }

    /// <summary>
    /// 回收投射物到对象池
    /// </summary>
    private void ReturnToPool()
    {
        Debug.Log($"[SlimeProjectile] ReturnToPool - isFromPool={isFromPool}");
        if (isFromPool && ownerPool != null)
        {
            ownerPool.Return(this);
        }
        else
        {
            Debug.Log($"[SlimeProjectile] 非池对象，直接销毁");
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// 初始化（从对象池取出时调用）
    /// </summary>
    public void Initialize(float speed, int damage, Vector2 direction, float rotationSpeed, bool fromPool, SlimeProjectilePool pool, ProjectileTarget targetType)
    {
        this.speed = speed;
        this.damage = damage;
        this.direction = direction;
        this.rotationSpeed = rotationSpeed;
        this.isFromPool = fromPool;
        this.ownerPool = pool;
        this.targetType = targetType;

        Debug.Log($"[SlimeProjectile] Initialize - speed={speed}, damage={damage}, direction={direction}, rotation={rotationSpeed}, target={targetType}");
    }

    /// <summary>
    /// 重置状态
    /// </summary>
    public void ResetState()
    {
        rb.velocity = Vector2.zero;
    }
}

/// <summary>
/// Slime 投射物对象池（支持多个预制体）
/// </summary>
public class SlimeProjectilePool : MonoBehaviour
{
    // 全局池管理器：用预制体作为key管理多个池
    private static Dictionary<GameObject, SlimeProjectilePool> poolManager = new Dictionary<GameObject, SlimeProjectilePool>();

    [Header("对象池设置")]
    [Tooltip("预制体")]
    public GameObject projectilePrefab;

    [Tooltip("预加载数量")]
    public int preloadCount = 10;

    private Queue<SlimeProjectile> pool = new Queue<SlimeProjectile>();

    /// <summary>
    /// 获取指定预制体的对象池
    /// </summary>
    public static SlimeProjectilePool GetPool(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[SlimeProjectilePool] 预制体为空！");
            return null;
        }

        if (poolManager.TryGetValue(prefab, out SlimeProjectilePool existingPool))
        {
            return existingPool;
        }

        // 创建新池
        GameObject poolObj = new GameObject($"SlimeProjectilePool_{prefab.name}");
        SlimeProjectilePool newPool = poolObj.AddComponent<SlimeProjectilePool>();
        newPool.projectilePrefab = prefab;
        newPool.preloadCount = 10;
        newPool.Preload();

        poolManager.Add(prefab, newPool);
        Debug.Log($"[SlimeProjectilePool] 为预制体 {prefab.name} 创建新池");

        return newPool;
    }

    private void Preload()
    {
        if (projectilePrefab == null)
        {
            Debug.LogWarning("[SlimeProjectilePool] 预制体未设置！");
            return;
        }

        for (int i = 0; i < preloadCount; i++)
        {
            GameObject obj = Instantiate(projectilePrefab, transform);
            obj.SetActive(false);
            SlimeProjectile projectile = obj.GetComponent<SlimeProjectile>();
            if (projectile != null)
            {
                pool.Enqueue(projectile);
            }
        }

        Debug.Log($"[SlimeProjectilePool] 预加载完成，预制体: {projectilePrefab.name}, 数量: {pool.Count}");
    }

    /// <summary>
    /// 获取投射物
    /// </summary>
    public SlimeProjectile Get(Vector2 position, Vector2 direction, float speed = 5f, int damage = 1, float rotationSpeed = 0f, ProjectileTarget targetType = ProjectileTarget.PlayerBase)
    {
        Debug.Log($"[SlimeProjectilePool] Get - 预制体: {projectilePrefab.name}, 池数量: {pool.Count}, 目标: {targetType}");

        SlimeProjectile projectile;

        if (pool.Count > 0)
        {
            projectile = pool.Dequeue();
            projectile.gameObject.SetActive(true);
            projectile.ResetState();
            Debug.Log($"[SlimeProjectilePool] 从池中取出，剩余: {pool.Count}");
        }
        else
        {
            // 池为空，动态创建，并补充到池里
            Debug.Log($"[SlimeProjectilePool] 池为空，动态创建");
            GameObject obj = Instantiate(projectilePrefab, transform);
            projectile = obj.GetComponent<SlimeProjectile>();

            // 额外补充5个到池中
            for (int i = 0; i < 5; i++)
            {
                GameObject extraObj = Instantiate(projectilePrefab, transform);
                extraObj.SetActive(false);
                SlimeProjectile extra = extraObj.GetComponent<SlimeProjectile>();
                if (extra != null)
                {
                    pool.Enqueue(extra);
                }
            }
        }

        projectile.transform.position = position;
        projectile.Initialize(speed, damage, direction, rotationSpeed, true, this, targetType);

        return projectile;
    }

    /// <summary>
    /// 回收投射物
    /// </summary>
    public void Return(SlimeProjectile projectile)
    {
        if (projectile == null) return;

        projectile.gameObject.SetActive(false);
        projectile.transform.SetParent(transform);
        pool.Enqueue(projectile);

        Debug.Log($"[SlimeProjectilePool] 回收完成，预制体: {projectilePrefab.name}, 池数量: {pool.Count}");
    }
}