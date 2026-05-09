using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Android 原生与 Unity 通信的桥接脚本
/// </summary>
public class AndroidBridge : MonoBehaviour
{
    private static AndroidBridge _instance;
    public static AndroidBridge Instance => _instance;

    private AndroidJavaObject _currentActivity;

    [Header("Slime 管理")]
    [Tooltip("Slime 预制体列表，按索引对应编号（0,2,3,4,5,6,7）")]
    public List<GameObject> slimePrefabs;

    // 缓存场景中的 Slime 对象，按编号存储
    private Dictionary<int, SlimeController> slimeControllers = new Dictionary<int, SlimeController>();

    private void Awake()
    {
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 获取 Android 当前 Activity
#if UNITY_ANDROID && !UNITY_EDITOR
        using (AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
        {
            _currentActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
        }
        Debug.Log("[AndroidBridge] Android Activity 获取成功");
#endif

        // 查找场景中所有的 Slime 控制器
        FindAllSlimes();
    }

    /// <summary>
    /// 查找场景中所有的 Slime 控制器
    /// </summary>
    private void FindAllSlimes()
    {
        slimeControllers.Clear();

        SlimeController[] slimes = FindObjectsOfType<SlimeController>();
        foreach (var slime in slimes)
        {
            slimeControllers[slime.slimeNumber] = slime;
            Debug.Log($"[AndroidBridge] 找到 Slime {slime.slimeNumber}");
        }

        Debug.Log($"[AndroidBridge] 共找到 {slimeControllers.Count} 个 Slime");
    }

    /// <summary>
    /// 接收 Android 原生传输的字符串（由 Android 端调用）
    /// </summary>
    /// <param name="message">Android 传输的字符串</param>
    public void ReceiveStringFromAndroid(string message)
    {
        Debug.Log($"[AndroidBridge] 收到 Android 消息: {message}");

        // 解析收到的数字并触发对应 Slime 的 Jump 动画
        if (int.TryParse(message, out int slimeNumber))
        {
            TriggerSlimeJump(slimeNumber);
        }
    }

    /// <summary>
    /// 触发指定编号的 Slime 跳跃
    /// </summary>
    /// <param name="slimeNumber">Slime 编号</param>
    public void TriggerSlimeJump(int slimeNumber)
    {
        // 如果字典为空，尝试重新查找
        if (slimeControllers.Count == 0)
        {
            FindAllSlimes();
        }

        if (slimeControllers.TryGetValue(slimeNumber, out SlimeController slime))
        {
            slime.Shoot();
            Debug.Log($"[AndroidBridge] 触发 Slime {slimeNumber} 发射投射物");
        }
        else
        {
            Debug.LogWarning($"[AndroidBridge] 未找到 Slime {slimeNumber}");
        }
    }

    /// <summary>
    /// 接收 Android 原生传输的整数值
    /// </summary>
    public void ReceiveIntFromAndroid(int value)
    {
        Debug.Log($"[AndroidBridge] 收到 Android 整数: {value}");
        TriggerSlimeJump(value);
    }

    /// <summary>
    /// 调用 Android 原生方法（无参数）
    /// </summary>
    public void CallAndroidMethod(string methodName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_currentActivity != null)
        {
            _currentActivity.Call(methodName);
            Debug.Log($"[AndroidBridge] 调用 Android 方法: {methodName}");
        }
#endif
    }

    /// <summary>
    /// 调用 Android 原生方法（带字符串参数）
    /// </summary>
    public void CallAndroidMethod(string methodName, string param)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_currentActivity != null)
        {
            _currentActivity.Call(methodName, param);
            Debug.Log($"[AndroidBridge] 调用 Android 方法: {methodName}, 参数: {param}");
        }
#endif
    }

    /// <summary>
    /// 调用 Android 原生方法（带整型参数）
    /// </summary>
    public void CallAndroidMethod(string methodName, int param)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (_currentActivity != null)
        {
            _currentActivity.Call(methodName, param);
            Debug.Log($"[AndroidBridge] 调用 Android 方法: {methodName}, 参数: {param}");
        }
#endif
    }
}