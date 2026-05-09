using UnityEngine;
using UnityEngine.EventSystems;

public class AttackButton : MonoBehaviour, IPointerClickHandler
{
    [Header("攻击组件")]
    public PlayerAttack playerAttack;

    private void Start()
    {
        // 自动查找玩家攻击组件
        if (playerAttack == null)
            playerAttack = FindObjectOfType<PlayerAttack>();
    }

    // 实现 IPointerClickHandler 接口，处理点击事件
    public void OnPointerClick(PointerEventData eventData)
    {
        if (playerAttack != null)
        {
            playerAttack.Attack();
        }
    }
}