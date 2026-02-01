using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class huaibiao : MonoBehaviour
{
    [Tooltip("分数要减少的值，按钮点击时会从当前分数中减去这个值")] 
    public int scoreDecrease = 5;

    [Tooltip("点击按钮后增加的时间（秒）")]
    public float timeIncrease = 10f;

    // 点击一次后标记，防止重复点击
    private bool clicked = false;

    // 这个方法可以在 Unity 编辑器的 Button OnClick() 中绑定
    public void OnButtonClick()
    {
        if (clicked) return;
        clicked = true;

        // 禁用 Button 组件以防交互（如果脚本挂在 Button 上）
        var btn = GetComponent<Button>();
        if (btn != null)
            btn.interactable = false;

        if (GameManager.Instance != null)
        {
            // 减少分数（ChangeScore 接受正数增加，负数减少）
            GameManager.Instance.ChangeScore(-scoreDecrease);
            // 增加时间
            GameManager.Instance.ChangeTimer(timeIncrease);
        }
        else
        {
            Debug.LogWarning("huaibiao: GameManager.Instance 未找到，无法修改分数或时间。");
        }
        AudioManager.Instance.PlayGetTime(Vector3.zero);
    }
}
