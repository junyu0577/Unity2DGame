using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyHome : MonoBehaviour
{
    [Header("窝点设置")]
    [Tooltip("生成敌人延迟（秒）")]
    public float spawnDelay = 3f;

    [Tooltip("生成敌人数量")]
    public int enemyCount = 3;

    [Tooltip("生成间隔（秒）")]
    public float spawnInterval = 0.5f;

    [Header("敌人预制体")]
    [Tooltip("敌人预制体类型1（优先使用）")]
    public GameObject enemyPrefab1;

    [Tooltip("敌人预制体类型2（如果设置了type1和type2，则交替生成）")]
    public GameObject enemyPrefab2;

    [Header("图片设置")]
    [Tooltip("窝点受损图片（受击后切换为此图片）")]
    public Sprite damagedSprite;

    private EnemyHealth enemyHealth;
    private SpriteRenderer spriteRenderer;
    private int lastHealth;
    private bool isDestroyed = false;

    private void Awake()
    {
        // 添加 EnemyHealth 组件（这样玩家攻击系统可以自动识别并造成伤害）
        enemyHealth = GetComponent<EnemyHealth>();
        if (enemyHealth == null)
        {
            enemyHealth = gameObject.AddComponent<EnemyHealth>();
        }

        // 拦截死亡事件，阻止默认销毁
        enemyHealth.onDeath = OnHomeDestroyed;

        // 获取 SpriteRenderer 组件
        spriteRenderer = GetComponent<SpriteRenderer>();

        // 设置为 Enemy 层，以便玩家攻击能检测到
        gameObject.layer = LayerMask.NameToLayer("Enemy");
    }

    private void Start()
    {
        lastHealth = enemyHealth.maxHealth;
        // 延迟生成敌人
        StartCoroutine(SpawnEnemiesAfterDelay());
    }

    private void Update()
    {
        // 检测受击（仅在未摧毁时）
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
        // 如果已被摧毁，不再变红
        if (isDestroyed) return;

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.red;
            Invoke("ResetColor", 0.1f);
            Debug.Log("窝点受击！变红");
        }
    }

    /// <summary>
    /// 恢复颜色
    /// </summary>
    private void ResetColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    /// <summary>
    /// 被摧毁时（不消失，切换图片）
    /// </summary>
    private void OnHomeDestroyed()
    {
        isDestroyed = true;

        // 切换为损坏图片
        if (damagedSprite != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = damagedSprite;
        }

        // 禁用碰撞体（不能再被攻击）
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        // 停止生成敌人（如果有协程在运行）
        StopAllCoroutines();

        Debug.Log("窝点已被摧毁！");
    }

    private IEnumerator SpawnEnemiesAfterDelay()
    {
        // 等待指定时间
        yield return new WaitForSeconds(spawnDelay);

        // 检查窝点是否已被摧毁
        if (enemyHealth == null || enemyHealth.currentHealth <= 0) yield break;

        Debug.Log("开始生成敌人...");

        // 分批生成敌人
        for (int i = 0; i < enemyCount; i++)
        {
            // 再次检查窝点是否还活着
            if (enemyHealth == null || enemyHealth.currentHealth <= 0) break;

            // 获取要生成的敌人预制体
            GameObject prefabToSpawn = GetEnemyPrefab(i);

            if (prefabToSpawn != null)
            {
                SpawnEnemy(prefabToSpawn);
            }
            else
            {
                Debug.LogWarning("EnemyHome: 未设置任何敌人预制体！");
                break;
            }

            // 最后一批不需要等待
            if (i < enemyCount - 1)
            {
                yield return new WaitForSeconds(spawnInterval);
            }
        }

        Debug.Log($"生成完成，共 {enemyCount} 个敌人");
    }

    /// <summary>
    /// 获取要生成的敌人预制体
    /// </summary>
    private GameObject GetEnemyPrefab(int index)
    {
        // 如果两个都设置了，交替生成
        if (enemyPrefab1 != null && enemyPrefab2 != null)
        {
            return (index % 2 == 0) ? enemyPrefab1 : enemyPrefab2;
        }
        // 只设置了type1
        if (enemyPrefab1 != null)
        {
            return enemyPrefab1;
        }
        // 只设置了type2
        if (enemyPrefab2 != null)
        {
            return enemyPrefab2;
        }
        // 都没有设置
        return null;
    }

    private void SpawnEnemy(GameObject enemyPrefab)
    {
        if (enemyPrefab == null) return;

        // 在窝点附近随机位置生成敌人（避免完全重叠）
        Vector2 randomOffset = Random.insideUnitCircle * 1f;
        Vector2 spawnPos = (Vector2)transform.position + randomOffset;

        GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);

        // 让敌人立即锁定并追踪玩家
        Enemy enemyScript = enemy.GetComponent<Enemy>();
        if (enemyScript != null)
        {
            enemyScript.StartChasingPlayer();
        }
        else
        {
            // 尝试弓箭手
            EnemyArcher archerScript = enemy.GetComponent<EnemyArcher>();
            if (archerScript != null)
            {
                archerScript.StartChasing();
            }
        }

        Debug.Log($"生成敌人: {enemy.name} at {spawnPos}");
    }

    private void OnDrawGizmosSelected()
    {
        // 绘制窝点范围
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}