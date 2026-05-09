using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneSwitcher : MonoBehaviour
{
    [Header("场景设置")]
    [Tooltip("要切换到的场景名称")]
    public string targetSceneName;

    [Tooltip("是否需要玩家触发（否则是自动触发）")]
    public bool requirePlayerTrigger = true;

    [Header("过渡动画设置")]
    [Tooltip("Animator组件")]
    public Animator transitionAnimator;

    [Tooltip("过渡动画的状态名称")]
    public string transitionStateName = "FadeIn";

    [Tooltip("延迟执行动画的时间（秒）")]
    public float delayTime = 1f;

    [Tooltip("动画播放完成后等待的时间")]
    public float postAnimationDelay = 0.5f;

    private bool isTransitioning = false;

    private void Start()
    {
        Debug.Log($"[SceneSwitcher] 已添加到 {gameObject.name}，目标场景: {targetSceneName}");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log($"[SceneSwitcher] OnTriggerEnter2D: {collision.gameObject.name}, Tag: {collision.tag}");

        if (requirePlayerTrigger && !collision.CompareTag("Player"))
        {
            Debug.Log("[SceneSwitcher] 不是玩家，忽略");
            return;
        }

        SwitchScene();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"[SceneSwitcher] OnCollisionEnter2D: {collision.gameObject.name}, Tag: {collision.gameObject.tag}");

        if (requirePlayerTrigger && !collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("[SceneSwitcher] 不是玩家，忽略");
            return;
        }

        SwitchScene();
    }

    private void SwitchScene()
    {
        if (string.IsNullOrEmpty(targetSceneName))
        {
            Debug.LogError("[SceneSwitcher] 未设置目标场景名称！");
            return;
        }

        if (isTransitioning)
        {
            Debug.Log("[SceneSwitcher] 正在过渡中，忽略重复触发");
            return;
        }

        if (transitionAnimator != null)
        {
            // 使用过渡动画
            isTransitioning = true;
            StartCoroutine(TransitionRoutine());
        }
        else
        {
            // 直接切换场景
            int currentGold = GameManager.Instance != null ? GameManager.Instance.gold : 0;
            Debug.Log($"[SceneSwitcher] 切换到场景: {targetSceneName}，当前金币: {currentGold}");
            SceneManager.LoadScene(targetSceneName);
        }
    }

    private IEnumerator TransitionRoutine()
    {
        int currentGold = GameManager.Instance != null ? GameManager.Instance.gold : 0;
        Debug.Log($"[SceneSwitcher] 等待 {delayTime} 秒后播放过渡动画，当前金币: {currentGold}");

        // 等待指定延迟时间
        yield return new WaitForSeconds(delayTime);

        // 播放过渡动画
        if (transitionAnimator != null)
        {
            // 输出Animator当前状态
            AnimatorStateInfo stateInfo = transitionAnimator.GetCurrentAnimatorStateInfo(0);
            Debug.Log($"[SceneSwitcher] Animator当前状态: {stateInfo.shortNameHash} ({(Time.frameCount % 100)})");

            // 使用Play方法直接播放状态
            if (!string.IsNullOrEmpty(transitionStateName))
            {
                transitionAnimator.Play(transitionStateName);
                Debug.Log($"[SceneSwitcher] 使用Play方法触发状态: {transitionStateName}");
            }

            // 检查Animator是否有有效的Controller
            if (transitionAnimator.runtimeAnimatorController == null)
            {
                Debug.LogError("[SceneSwitcher] Animator没有绑定Controller！");
            }
            else
            {
                Debug.Log($"[SceneSwitcher] AnimatorController: {transitionAnimator.runtimeAnimatorController.name}");
            }
        }
        else
        {
            Debug.LogWarning("[SceneSwitcher] transitionAnimator为null！");
        }

        // 等待动画完成
        yield return new WaitForSeconds(postAnimationDelay);

        // 切换场景
        Debug.Log($"[SceneSwitcher] 过渡动画完成，切换到场景: {targetSceneName}，当前金币: {currentGold}");
        SceneManager.LoadScene(targetSceneName);

        isTransitioning = false;
    }
}