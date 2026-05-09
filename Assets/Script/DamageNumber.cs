using UnityEngine;
using TMPro;

public class DamageNumber : MonoBehaviour
{
    [Header("进入动画")]
    [Tooltip("进入动画持续时间")]
    public float scaleInDuration = 0.3f;

    [Tooltip("进入动画起始大小")]
    public float scaleInStartSize = 0f;

    [Tooltip("进入动画最大大小")]
    public float scaleInPeakSize = 48f;

    [Tooltip("进入动画时向上移动的距离")]
    public float scaleInMoveDistance = 0.2f;

    [Header("停留动画")]
    [Tooltip("停留持续时间")]
    public float stayDuration = 0.5f;

    [Header("退出动画")]
    [Tooltip("退出动画持续时间")]
    public float fadeOutDuration = 0.5f;

    [Tooltip("退出动画最终大小")]
    public float fadeOutEndSize = 24f;

    [Tooltip("退出动画向上移动的距离")]
    public float fadeOutMoveDistance = 1f;

    [Header("描边设置")]
    [Tooltip("是否启用描边")]
    public bool enableOutline = true;

    [Tooltip("描边宽度")]
    public float outlineWidth = 0.2f;

    [Tooltip("描边颜色")]
    public Color outlineColor = Color.black;

    private TextMeshProUGUI textMesh;
    private RectTransform rectTransform;
    private Vector2 startPos;
    private float initialFontSize;

    private void Awake()
    {
        textMesh = GetComponent<TextMeshProUGUI>();
        rectTransform = GetComponent<RectTransform>();

        if (textMesh != null)
        {
            initialFontSize = textMesh.fontSize;
        }

        // 应用描边设置
        ApplyOutline();
    }

    /// <summary>
    /// 应用描边设置
    /// </summary>
    private void ApplyOutline()
    {
        if (textMesh == null) return;

        if (enableOutline)
        {
            textMesh.outlineWidth = outlineWidth;
            textMesh.outlineColor = outlineColor;
        }
    }

    /// <summary>
    /// 显示伤害数字
    /// </summary>
    public void Show(Vector3 position, int damage)
    {
        Debug.Log($"[DamageNumber] Show - 当前颜色: {textMesh?.color}, 预制体颜色: r={textMesh?.color.r}, g={textMesh?.color.g}, b={textMesh?.color.b}");

        if (textMesh != null)
        {
            textMesh.text = damage.ToString();
            textMesh.fontSize = scaleInStartSize;
            // 保持预制体中设置的颜色，只设置透明度为1
            Color originalColor = textMesh.color;
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, 1f);

            Debug.Log($"[DamageNumber] 设置后颜色: {textMesh.color}");

            // 重新应用描边
            if (enableOutline)
            {
                textMesh.outlineWidth = outlineWidth;
                textMesh.outlineColor = outlineColor;
            }
        }

        if (rectTransform != null)
        {
            startPos = rectTransform.anchoredPosition;
        }

        StartCoroutine(Animate());
    }

    private System.Collections.IEnumerator Animate()
    {
        // === 阶段1：进入动画（从小到大 + 轻微上移） ===
        float elapsed = 0f;
        while (elapsed < scaleInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / scaleInDuration;
            // 使用弹性曲线
            t = Mathf.Sin(t * Mathf.PI * 0.5f);

            if (textMesh != null)
            {
                textMesh.fontSize = Mathf.Lerp(scaleInStartSize, scaleInPeakSize, t);
            }

            if (rectTransform != null)
            {
                rectTransform.anchoredPosition = startPos + Vector2.up * scaleInMoveDistance * 50f * t;
            }

            yield return null;
        }

        // 确保到达最大尺寸
        if (textMesh != null)
        {
            textMesh.fontSize = scaleInPeakSize;
        }

        // === 阶段2：停留 ===
        yield return new WaitForSeconds(stayDuration);

        // === 阶段3：退出动画（向上同时缩小消失） ===
        elapsed = 0f;
        float stayEndPosY = rectTransform != null ? rectTransform.anchoredPosition.y : startPos.y;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;

            if (textMesh != null)
            {
                textMesh.fontSize = Mathf.Lerp(scaleInPeakSize, fadeOutEndSize, t);

                // 透明度逐渐消失
                Color color = textMesh.color;
                color.a = 1f - t;
                textMesh.color = color;
            }

            if (rectTransform != null)
            {
                float moveProgress = fadeOutMoveDistance * 50f * t;
                rectTransform.anchoredPosition = new Vector2(
                    startPos.x,
                    stayEndPosY + moveProgress
                );
            }

            yield return null;
        }

        // 动画结束，销毁对象
        Destroy(gameObject);
    }
}