using UnityEngine;
using UnityEngine.EventSystems;

public class CustomJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    [Header("摇杆组件")]
    [Tooltip("摇杆背景（父物体）")]
    public RectTransform background;

    [Tooltip("摇杆手柄（子物体）")]
    public RectTransform handle;

    [Header("摇杆设置")]
    [Tooltip("手柄移动半径")]
    public float moveRadius = 100f;

    [Tooltip("死区（小于此值返回0）")]
    public float deadZone = 0.1f;

    private Vector2 inputVector;
    private Vector2 backgroundCenter;
    private bool isDragging = false;

    public float GetHorizontal()
    {
        return inputVector.x;
    }

    public float GetVertical()
    {
        return inputVector.y;
    }

    public Vector2 GetInput()
    {
        return inputVector;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isDragging = true;
        if (background != null)
        {
            backgroundCenter = background.position;
            UpdateHandlePosition(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;
        UpdateHandlePosition(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isDragging = false;
        inputVector = Vector2.zero;
        if (handle != null)
        {
            handle.anchoredPosition = Vector2.zero;
        }
    }

    private void UpdateHandlePosition(PointerEventData eventData)
    {
        if (background == null || handle == null) return;

        Vector2 touchPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            background,
            eventData.position,
            eventData.pressEventCamera,
            out touchPos
        );

        // 限制在圆形范围内
        touchPos = Vector2.ClampMagnitude(touchPos, moveRadius);

        // 设置手柄位置
        handle.anchoredPosition = touchPos;

        // 计算输入向量（归一化）
        inputVector = touchPos / moveRadius;

        // 应用死区
        if (inputVector.magnitude < deadZone)
        {
            inputVector = Vector2.zero;
        }
    }
}