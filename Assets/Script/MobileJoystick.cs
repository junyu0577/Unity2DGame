using UnityEngine;
using UnityEngine.EventSystems;

public class MobileJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("摇杆设置")]
    [Tooltip("摇杆背景半径")]
    public float handleRange = 100f;

    [Header("摇杆组件")]
    public RectTransform joystickBackground;
    public RectTransform joystickHandle;

    private Canvas canvas;
    private Camera cam;
    private Vector2 input = Vector2.zero;

    public Vector2 InputVector => input;
    public float Horizontal => input.x;
    public float Vertical => input.y;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogError("Joystick must be placed inside a canvas");
            return;
        }

        // 获取实际半径
        if (joystickBackground != null)
        {
            handleRange = joystickBackground.sizeDelta.x / 2f;
        }

        // 初始化手柄位置
        if (joystickHandle != null)
        {
            joystickHandle.anchoredPosition = Vector2.zero;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (joystickBackground == null || joystickHandle == null) return;

        cam = null;
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera)
            cam = canvas.worldCamera;

        // 获取摇杆背景的中心点（屏幕坐标）
        Vector2 backgroundPosition = RectTransformUtility.WorldToScreenPoint(cam, joystickBackground.position);
        Vector2 radius = joystickBackground.sizeDelta / 2f;

        // 计算输入向量（归一化）
        input = (eventData.position - backgroundPosition) / (radius * canvas.scaleFactor);

        // 限制输入向量长度
        if (input.magnitude > 1)
        {
            input = input.normalized;
        }

        // 设置手柄位置
        joystickHandle.anchoredPosition = input * radius.x;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        input = Vector2.zero;
        if (joystickHandle != null)
        {
            joystickHandle.anchoredPosition = Vector2.zero;
        }
    }

    public float GetHorizontal()
    {
        return input.x;
    }

    public float GetVertical()
    {
        return input.y;
    }
}