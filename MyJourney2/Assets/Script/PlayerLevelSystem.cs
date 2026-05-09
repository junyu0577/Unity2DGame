using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class PlayerLevelSystem : MonoBehaviour
{
    public static PlayerLevelSystem Instance;

    [Header("等级设置")]
    [Tooltip("初始等级")]
    public int level = 1;

    [Tooltip("每只怪物提供的经验值")]
    public int experiencePerKill = 1;

    [Header("升级所需经验（可配置）")]
    [Tooltip("每级升级所需经验，索引0表示1级升2级所需经验，索引1表示2级升3级，以此类推")]
    public int[] experiencePerLevel = new int[] { 1, 10 };

    [Header("暴击设置")]
    [Tooltip("暴击率（0-100），2级及以上为100%")]
    [Range(0, 100)]
    public int criticalRate = 0;

    [Tooltip("暴击伤害倍率")]
    public float criticalDamageMultiplier = 2f;

    [Header("UI组件")]
    [Tooltip("等级显示文本")]
    public TextMeshProUGUI levelText;

    private int currentExperience = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[PlayerLevelSystem] 场景已加载: {scene.name}");
        // 重新查找等级文本并更新显示
        levelText = null;
        FindLevelText();
        UpdateLevelUI();
    }

    private void Start()
    {
        // 自动查找等级文本
        FindLevelText();
        UpdateLevelUI();
    }

    private void FindLevelText()
    {
        if (levelText == null)
        {
            GameObject levelTextObj = GameObject.Find("player_level_text");
            if (levelTextObj != null)
            {
                levelText = levelTextObj.GetComponent<TextMeshProUGUI>();
                Debug.Log($"[PlayerLevelSystem] 找到等级文本: {levelText}");
            }
            else
            {
                Debug.LogWarning("[PlayerLevelSystem] 未找到 player_level_text UI对象！");
            }
        }
    }

    /// <summary>
    /// 增加经验值
    /// </summary>
    public void AddExperience(int amount)
    {
        currentExperience += amount;
        Debug.Log($"[PlayerLevelSystem] 获得经验: {amount}, 当前经验: {currentExperience}");

        CheckLevelUp();
    }

    /// <summary>
    /// 检查是否可以升级
    /// </summary>
    private void CheckLevelUp()
    {
        // 获取当前升级所需经验
        int expNeeded = GetExperienceForNextLevel();

        while (currentExperience >= expNeeded && expNeeded > 0)
        {
            currentExperience -= expNeeded;
            level++;
            Debug.Log($"[PlayerLevelSystem] 升级！当前等级: {level}, 剩余经验: {currentExperience}");

            // 更新升级所需经验
            expNeeded = GetExperienceForNextLevel();

            // 根据等级更新暴击率
            UpdateCriticalRate();

            // 更新UI
            UpdateLevelUI();
        }
    }

    /// <summary>
    /// 根据等级更新暴击率
    /// </summary>
    private void UpdateCriticalRate()
    {
        if (level >= 2)
        {
            criticalRate = 100;
            Debug.Log($"[PlayerLevelSystem] 2级及以上，暴击率设置为: {criticalRate}%");
        }
    }

    /// <summary>
    /// 获取下一级升级所需经验
    /// </summary>
    private int GetExperienceForNextLevel()
    {
        // 数组索引 = 等级 - 1（例如：1级升2级用索引0）
        int index = level - 1;

        if (index < experiencePerLevel.Length)
        {
            return experiencePerLevel[index];
        }

        // 如果超出配置数组，使用最后配置的值
        return experiencePerLevel[experiencePerLevel.Length - 1];
    }

    /// <summary>
    /// 更新等级UI显示
    /// </summary>
    public void UpdateLevelUI()
    {
        if (levelText != null)
        {
            levelText.text = "lv" + level;
            Debug.Log($"[PlayerLevelSystem] 更新等级UI: {level}");
        }
    }

    /// <summary>
    /// 获取当前等级
    /// </summary>
    public int GetLevel()
    {
        return level;
    }

    /// <summary>
    /// 获取当前经验值
    /// </summary>
    public int GetCurrentExperience()
    {
        return currentExperience;
    }

    /// <summary>
    /// 获取升级所需经验
    /// </summary>
    public int GetExperienceNeeded()
    {
        return GetExperienceForNextLevel();
    }

    /// <summary>
    /// 获取暴击率（0-100）
    /// </summary>
    public int GetCriticalRate()
    {
        return criticalRate;
    }

    /// <summary>
    /// 获取暴击伤害倍率
    /// </summary>
    public float GetCriticalDamageMultiplier()
    {
        return criticalDamageMultiplier;
    }

    /// <summary>
    /// 判断是否暴击
    /// </summary>
    public bool IsCritical()
    {
        // 随机数0-99，如果小于暴击率则暴击
        return Random.Range(0, 100) < criticalRate;
    }

    /// <summary>
    /// 重置等级和经验（玩家死亡时调用）
    /// </summary>
    public void ResetLevel()
    {
        level = 1;
        currentExperience = 0;
        criticalRate = 0;
        UpdateLevelUI();
        Debug.Log("[PlayerLevelSystem] 等级和经验已重置");
    }
}