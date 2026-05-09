using UnityEngine;
using UnityEngine.SceneManagement;

public class RestartButton : MonoBehaviour
{
    [Header("设置")]
    [Tooltip("重新开始后加载的第一个游戏场景名称")]
    public string firstLevelSceneName = "SampleScene";

    [Tooltip("是否重置金币")]
    public bool resetGold = true;

    [Tooltip("是否重置生命值")]
    public bool resetHealth = true;

    public void OnRestartButtonClicked()
    {
        Debug.Log("[RestartButton] 按钮被点击！");

        // 检查 GameManager 是否存在
        if (GameManager.Instance == null)
        {
            Debug.LogError("[RestartButton] GameManager.Instance 为空！检查是否在场景中放置了 GameManager");
        }
        else
        {
            Debug.Log("[RestartButton] GameManager 存在，当前金币: " + GameManager.Instance.gold + ", 当前生命: " + GameManager.Instance.playerHealth);

            // 重置GameManager数据
            if (resetGold)
            {
                GameManager.Instance.gold = 0;
                Debug.Log("[RestartButton] 金币已重置");
            }

            if (resetHealth)
            {
                GameManager.Instance.playerHealth = GameManager.Instance.playerMaxHealth;
                Debug.Log("[RestartButton] 生命值已重置");
            }
        }

        // 检查场景是否存在
        Debug.Log("[RestartButton] 准备加载场景: " + firstLevelSceneName);
        SceneManager.LoadScene(firstLevelSceneName);
    }
}