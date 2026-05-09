using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    [Header("生命值设置")]
    public int maxHealth = 5;
    public int currentHealth;

    [Header("UI")]
    public Image healthBarImage;  // 血条 Image（Fill 类型）

    [Header("死亡场景设置")]
    public string deathSceneName = "GameOver";  // 死亡后跳转的场景

    private SpriteRenderer sr;
    private Color originalColor;
    private bool isInvincible;
    private float invincibilityDuration = 0.2f;
    private float invincibilityTimer;

    private void Awake()
    {
        currentHealth = maxHealth;
        sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            originalColor = sr.color;
        }

        // 同步最大生命值到GameManager（要在恢复生命值之前）
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerMaxHealth(maxHealth);
            Debug.Log($"[PlayerHealth] Awake 同步 maxHealth = {maxHealth} 到 GameManager");
        }

        // 从GameManager恢复生命值
        if (GameManager.Instance != null)
        {
            currentHealth = GameManager.Instance.playerHealth;

            // 同步当前生命值到GameManager
            GameManager.Instance.SetPlayerHealth(currentHealth);

            // 如果GameManager有保存的最大血量，则恢复
            if (GameManager.Instance.playerMaxHealth > 0)
            {
                maxHealth = GameManager.Instance.playerMaxHealth;
            }
            Debug.Log($"[PlayerHealth] 从GameManager恢复: currentHealth={currentHealth}, maxHealth={maxHealth}");
        }
        else
        {
            currentHealth = maxHealth;
        }
    }

    private void Update()
    {
        if (isInvincible)
        {
            invincibilityTimer -= Time.deltaTime;
            if (invincibilityTimer <= 0)
            {
                isInvincible = false;
                if (sr != null)
                    sr.color = originalColor;
            }
        }
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;

        // 同步到GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerHealth(currentHealth);
        }

        // 闪烁红色
        if (sr != null)
        {
            sr.color = Color.red;
        }

        // 进入无敌时间
        isInvincible = true;
        invincibilityTimer = invincibilityDuration;

        // 更新血条
        UpdateHealthBar();

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Die();
        }

        Debug.Log("玩家受到伤害！剩余血量: " + currentHealth);
    }

    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        // 同步到GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetPlayerHealth(currentHealth);
        }

        // 更新血条
        UpdateHealthBar();

        Debug.Log("玩家恢复生命！当前血量: " + currentHealth);
    }

    private void UpdateHealthBar()
    {
        Debug.Log($"[PlayerHealth] UpdateHealthBar - currentHealth: {currentHealth}, maxHealth: {maxHealth}, healthBarImage: {healthBarImage}");
        if (healthBarImage != null)
        {
            healthBarImage.fillAmount = (float)currentHealth / maxHealth;
            Debug.Log($"[PlayerHealth] 设置 fillAmount: {(float)currentHealth / maxHealth}");
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] healthBarImage 为空！");
        }
    }

    /// <summary>
    /// 强制更新血条显示（供GameManager调用）
    /// </summary>
    public void ForceUpdateHealthBar()
    {
        UpdateHealthBar();
    }

    private void Die()
    {
        Debug.Log("玩家死亡！");

        // 重置等级和经验
        if (PlayerLevelSystem.Instance != null)
        {
            PlayerLevelSystem.Instance.ResetLevel();
        }

        // 切换到指定场景
        if (!string.IsNullOrEmpty(deathSceneName))
        {
            SceneManager.LoadScene(deathSceneName);
        }
        gameObject.SetActive(false);
    }

    // 初始化时更新血条显示
    private void Start()
    {
        Debug.Log($"[PlayerHealth] Start 被调用，maxHealth = {maxHealth}, currentHealth = {currentHealth}");

        // 同步最大生命值和当前生命值到GameManager
        if (GameManager.Instance != null)
        {
            Debug.Log($"[PlayerHealth] 同步 maxHealth 到 GameManager，GameManager.playerMaxHealth 当前值 = {GameManager.Instance.playerMaxHealth}");
            GameManager.Instance.SetPlayerMaxHealth(maxHealth);

            Debug.Log($"[PlayerHealth] 同步 currentHealth 到 GameManager，GameManager.playerHealth 当前值 = {GameManager.Instance.playerHealth}");
            GameManager.Instance.SetPlayerHealth(currentHealth);

            Debug.Log($"[PlayerHealth] 同步完成，GameManager.playerMaxHealth = {GameManager.Instance.playerMaxHealth}, playerHealth = {GameManager.Instance.playerHealth}");
        }
        else
        {
            Debug.LogWarning("[PlayerHealth] GameManager.Instance 为空！");
        }

        UpdateHealthBar();
    }
}