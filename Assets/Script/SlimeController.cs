using UnityEngine;

/// <summary>
/// 投射物目标类型
/// </summary>
public enum ProjectileTarget
{
    PlayerBase,  // 玩家基地
    Player,      // 玩家
    Both         // 两者都攻击
}

/// <summary>
/// 控制 Slime 怪物
/// </summary>
public class SlimeController : MonoBehaviour
{
    [Header("Slime 编号")]
    [Tooltip("Slime 的编号")]
    public int slimeNumber = 0;

    [Header("投射物设置")]
    [Tooltip("投射物预制体（需要设置才能发射）")]
    public GameObject projectilePrefab;

    [Tooltip("投射物飞行速度")]
    public float projectileSpeed = 5f;

    [Tooltip("投射物伤害值")]
    public int projectileDamage = 1;

    [Tooltip("投射物旋转速度（度/秒），0为不旋转")]
    public float projectileRotationSpeed = 360f;

    [Tooltip("攻击目标类型")]
    public ProjectileTarget targetType = ProjectileTarget.PlayerBase;

    private void Start()
    {
        Debug.Log($"[SlimeController] Slime {slimeNumber} 初始化完成");
    }

    /// <summary>
    /// 直接发射投射物（供Android调用）
    /// </summary>
    public void Shoot()
    {
        // 确保预制体已设置
        if (projectilePrefab == null)
        {
            Debug.LogWarning($"[SlimeController] Slime {slimeNumber} 的预制体未设置！");
            return;
        }

        // 查找目标
        Transform target = null;
        switch (targetType)
        {
            case ProjectileTarget.PlayerBase:
                PlayerBase playerBase = FindObjectOfType<PlayerBase>();
                target = playerBase != null ? playerBase.transform : null;
                break;
            case ProjectileTarget.Player:
                // 查找玩家（需要根据你的玩家脚本名称调整）
                PlayerAttack player = FindObjectOfType<PlayerAttack>();
                if (player == null)
                {
                    // 尝试其他可能的玩家脚本
                    MonoBehaviour[] allComponents = FindObjectsOfType<MonoBehaviour>();
                    foreach (var comp in allComponents)
                    {
                        if (comp.GetType().Name.Contains("Player"))
                        {
                            target = comp.transform;
                            break;
                        }
                    }
                }
                else
                {
                    target = player.transform;
                }
                break;
            case ProjectileTarget.Both:
                // 优先攻击玩家，如果没有玩家则攻击基地
                PlayerAttack p = FindObjectOfType<PlayerAttack>();
                if (p != null)
                {
                    target = p.transform;
                }
                else
                {
                    PlayerBase pb = FindObjectOfType<PlayerBase>();
                    target = pb != null ? pb.transform : null;
                }
                break;
        }

        if (target == null)
        {
            Debug.LogWarning($"[SlimeController] Slime {slimeNumber} 未找到目标！");
            return;
        }

        // 获取对应预制体的对象池
        SlimeProjectilePool pool = SlimeProjectilePool.GetPool(projectilePrefab);
        if (pool == null)
        {
            Debug.LogWarning("[SlimeController] 获取对象池失败！");
            return;
        }

        // 计算发射方向：指向目标
        Vector2 shootPosition = transform.position;
        Vector2 targetPosition = target.position;
        Vector2 direction = (targetPosition - shootPosition).normalized;

        Debug.Log($"[SlimeController] Slime {slimeNumber} 发射投射物，预制体: {projectilePrefab.name}，目标: {target.name}，方向: {direction}");

        // 从对象池获取投射物
        pool.Get(shootPosition, direction, projectileSpeed, projectileDamage, projectileRotationSpeed, targetType);
    }
}