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

    // 得分后调用（可修改为播放动画等）
    public void OnCleared()
    {
        print(12345);
    }
}
