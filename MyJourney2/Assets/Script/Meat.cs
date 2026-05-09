using UnityEngine;

public class Meat : MonoBehaviour
{
    [Header("采集设置")]
    public int healAmount = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检测是否是玩家
        if (collision.CompareTag("Player"))
        {
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null)
            {
                // 恢复玩家生命值
                health.Heal(healAmount);

                // 销毁肉对象
                Destroy(gameObject);

                Debug.Log("采集到肉！恢复 " + healAmount + " 点生命值");
            }
        }
    }
}