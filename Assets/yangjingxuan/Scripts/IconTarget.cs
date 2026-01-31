using UnityEngine;

/// <summary>
/// 挂在每个小 icon 上的组件（图标现在为独立 GameObject）
/// 当被擦除判定为“已清除”时会触发 OnCleared（默认隐藏物体）
/// 可自定义在 OnCleared 中播放动画、声效等
/// </summary>
public class IconTarget : MonoBehaviour
{
    // 已被清除（得分）标记
    [HideInInspector] public bool isCleared = false;

    // 每个 icon 的得分（现在将记分逻辑放到这里）
    public int scoreValue = 10;

    // 得分后调用（可修改为播放动画等）
    public void OnCleared()
    {
        // 防止重复记分/重复触发
        if (isCleared) return;
        isCleared = true;

        // 这里处理得分逻辑（将 GameManager 的调用放在 IconTarget）
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ChangeScore(scoreValue);
        }

        // 保留原来的调试输出或在此播放音效/动画
        print(12345);
    }
}
