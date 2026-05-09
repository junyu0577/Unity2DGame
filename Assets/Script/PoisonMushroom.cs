using UnityEngine;

public class PoisonMushroom : MonoBehaviour
{
    [Header("毒蘑菇设置")]
    public int damage = 1;

    [Tooltip("弹开力度")]
    public float knockbackForce = 5f;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 检测是否是玩家
        if (collision.CompareTag("Player"))
        {
            PlayerHealth health = collision.GetComponent<PlayerHealth>();
            if (health != null)
            {
                // 扣血
                health.TakeDamage(damage);
            }

            // 弹开玩家
            PlayerMove playerMove = collision.GetComponent<PlayerMove>();
            if (playerMove != null)
            {
                // 计算弹开方向（从蘑菇指向玩家）
                Vector2 direction = (collision.transform.position - transform.position).normalized;

                // 如果方向为0（重叠），随机弹开
                if (direction == Vector2.zero)
                {
                    direction = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
                }

                // 使用新方法弹开玩家
                playerMove.Knockback(direction, knockbackForce);
            }

            Debug.Log("碰到毒蘑菇！受到 " + damage + " 点伤害");
        }
    }
}