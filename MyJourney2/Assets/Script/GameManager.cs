using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("金币设置")]
    public int gold = 0;

    [Header("玩家数据")]
    public int playerHealth = 100;
    public int playerMaxHealth = 10;
    public Vector3 playerPosition;
    public string lastSceneName;

    [Header("UI组件")]
    public TextMeshProUGUI goldText;
    public Image healthBarImage;  // 血条 Image（Fill 类型）

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log($"[GameManager] 新建实例，DontDestroyOnLoad，初始金币: {gold}");

            // 监听场景切换，保存玩家位置
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Debug.Log($"[GameManager] 检测到重复实例，销毁，当前金币: {gold}");
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[GameManager] 场景已加载: {scene.name}");

        // 新场景加载后，重新查找UI引用并更新显示
        FindGoldText();
        FindHealthBarImage();
        UpdateGoldUI();
        UpdateHealthBarUI();
    }

    private void FindGoldText()
    {
        if (goldText == null)
        {
            GameObject goldTextObj = GameObject.Find("gold_text");
            if (goldTextObj != null)
            {
                goldText = goldTextObj.GetComponent<TextMeshProUGUI>();
            }
        }
    }

    private void FindHealthBarImage()
    {
        if (healthBarImage == null)
        {
            Debug.Log("[GameManager] 开始查找血条UI health_real...");
            GameObject healthBarObj = GameObject.Find("health_real");
            if (healthBarObj != null)
            {
                healthBarImage = healthBarObj.GetComponent<Image>();
                Debug.Log($"[GameManager] 找到血条Image: {healthBarImage}");
            }
            else
            {
                Debug.LogWarning("[GameManager] 未找到血条UI health_real！");
            }
        }
    }

    /// <summary>
    /// 更新所有PlayerHealth的血条显示
    /// </summary>
    private void UpdateHealthBarUI()
    {
        // 查找场景中所有的PlayerHealth组件
        PlayerHealth[] playerHealths = FindObjectsOfType<PlayerHealth>();
        foreach (var playerHealthScript in playerHealths)
        {
            // 将GameManager的血条Image传递给PlayerHealth
            if (healthBarImage != null)
            {
                playerHealthScript.healthBarImage = healthBarImage;
            }
            // 强制更新血条显示
            playerHealthScript.ForceUpdateHealthBar();
            Debug.Log($"[GameManager] 更新PlayerHealth血条，当前血量: {playerHealth}");
        }
    }

    private void Start()
    {
        // 尝试自动获取UI引用
        if (goldText == null)
        {
            GameObject goldTextObj = GameObject.Find("gold_text");
            if (goldTextObj != null)
            {
                goldText = goldTextObj.GetComponent<TextMeshProUGUI>();
            }
        }
        UpdateGoldUI();

        // 查找并更新血条UI
        FindHealthBarImage();
        UpdateHealthBarUI();
    }

    /// <summary>
    /// 增加金币
    /// </summary>
    public void AddGold(int amount)
    {
        gold += amount;
        UpdateGoldUI();
        Debug.Log($"[GameManager] 金币+{amount}，当前金币: {gold}");
    }

    /// <summary>
    /// 设置玩家位置
    /// </summary>
    public void SetPlayerPosition(Vector3 position)
    {
        playerPosition = position;
    }

    /// <summary>
    /// 设置玩家生命值
    /// </summary>
    public void SetPlayerHealth(int health)
    {
        playerHealth = health;
        Debug.Log($"[GameManager] 玩家生命值: {playerHealth}");
    }

    /// <summary>
    /// 设置玩家最大生命值
    /// </summary>
    public void SetPlayerMaxHealth(int maxHealth)
    {
        playerMaxHealth = maxHealth;
        Debug.Log($"[GameManager] 玩家最大生命值: {playerMaxHealth}");
    }

    /// <summary>
    /// 更新金币UI显示
    /// </summary>
    public void UpdateGoldUI()
    {
        if (goldText != null)
        {
            goldText.text = gold.ToString();
        }
    }
}