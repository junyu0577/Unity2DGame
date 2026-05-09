using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("跟随目标")]
    [Tooltip("跟随的目标对象（Player）")]
    public Transform target;

    [Header("跟随设置")]
    [Tooltip("相机相对于目标的偏移量")]
    public Vector3 offset = new Vector3(0, 0, -10);

    [Tooltip("跟随平滑速度，值越大跟随越快")]
    public float smoothSpeed = 0.125f;

    private void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;
    }
}