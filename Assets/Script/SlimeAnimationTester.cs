using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Slime 动画状态切换测试脚本
/// 每隔指定时间切换到下一个动画状态
/// </summary>
public class SlimeAnimationTester : MonoBehaviour
{
    [Header("动画设置")]
    [Tooltip("要切换的动画状态列表")]
    public List<string> animationStates = new List<string> { "slime_0_idle", "slime_0_walk", "slime_0_jump", "slime_0_die" };

    [Tooltip("切换间隔时间（秒）")]
    public float interval = 1f;

    [Header("组件")]
    [Tooltip("Animator 组件")]
    public Animator animator;

    private int currentIndex = 0;
    private float timer = 0f;

    private void Start()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        // 播放第一个动画
        if (animationStates.Count > 0 && animator != null)
        {
            PlayCurrentAnimation();
        }
    }

    private void Update()
    {
        if (animator == null || animationStates.Count == 0) return;

        timer += Time.deltaTime;

        if (timer >= interval)
        {
            timer = 0f;
            SwitchToNextAnimation();
        }
    }

    /// <summary>
    /// 切换到下一个动画
    /// </summary>
    private void SwitchToNextAnimation()
    {
        currentIndex = (currentIndex + 1) % animationStates.Count;
        PlayCurrentAnimation();
    }

    /// <summary>
    /// 播放当前索引的动画
    /// </summary>
    private void PlayCurrentAnimation()
    {
        string stateName = animationStates[currentIndex];
        animator.Play(stateName);
        Debug.Log($"[SlimeAnimationTester] 切换到动画: {stateName}");
    }

    /// <summary>
    /// 立即切换到指定动画
    /// </summary>
    public void SwitchToAnimation(int index)
    {
        if (index >= 0 && index < animationStates.Count)
        {
            currentIndex = index;
            PlayCurrentAnimation();
            timer = 0f;
        }
    }

    /// <summary>
    /// 立即切换到指定动画（按名称）
    /// </summary>
    public void SwitchToAnimation(string stateName)
    {
        int index = animationStates.IndexOf(stateName);
        if (index >= 0)
        {
            SwitchToAnimation(index);
        }
        else
        {
            Debug.LogWarning($"[SlimeAnimationTester] 未找到动画状态: {stateName}");
        }
    }
}