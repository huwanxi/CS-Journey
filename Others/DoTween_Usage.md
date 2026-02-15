# DoTween 工具库常用使用方法

DoTween 是 Unity 中最流行的补间动画插件，用于代码控制物体的移动、旋转、缩放、颜色变化等。

## 1. 基础 Tweener 操作

### 1.1 移动、旋转、缩放

```csharp
using DG.Tweening;
using UnityEngine;

public class DoTweenBasics : MonoBehaviour
{
    public Transform target;

    void Start()
    {
        // 移动到 (5, 0, 0) 耗时 1 秒
        transform.DOMove(new Vector3(5, 0, 0), 1f);

        // 相对移动 (在当前位置基础上 +5)
        transform.DOMove(new Vector3(5, 0, 0), 1f).SetRelative();

        // 旋转
        transform.DORotate(new Vector3(0, 180, 0), 0.5f);

        // 缩放
        transform.DOScale(Vector3.one * 2, 1f);
        
        // 震动 (持续时间, 强度, 震动次数)
        transform.DOShakePosition(1f, 0.5f, 10);
    }
}
```

### 1.2 材质与 UI 动画

```csharp
using UnityEngine.UI;

public class ColorTween : MonoBehaviour
{
    public Image myImage;
    public Text myText;
    public Material myMat;

    void Start()
    {
        // UI 颜色渐变
        myImage.DOColor(Color.red, 1f);
        
        // UI 透明度渐变
        myImage.DOFade(0, 1f);

        // 文字打字机效果 (2秒内显示完文字)
        myText.DOText("Hello DoTween!", 2f);

        // 材质颜色
        myMat.DOColor(Color.green, "_Color", 1f);
    }
}
```

---

## 2. 动画设置与控制

### 2.1 常用链式设置

```csharp
transform.DOMoveX(10, 1f)
    .SetEase(Ease.OutBack)   // 设置缓动曲线 (OutBack 会稍微冲过头再回来)
    .SetLoops(-1, LoopType.Yoyo) // 无限循环，像悠悠球一样往返
    .SetDelay(0.5f)          // 延迟 0.5 秒启动
    .SetUpdate(true);        // 受 Time.timeScale 影响与否 (true 为不受影响)
```

### 2.2 回调函数

```csharp
transform.DOMove(Vector3.up, 1f)
    .OnStart(() => Debug.Log("开始移动"))
    .OnComplete(() => Debug.Log("移动结束"))
    .OnKill(() => Debug.Log("动画被销毁"));
```

### 2.3 动画控制

```csharp
Tweener myTween = transform.DOMove(Vector3.up, 1f);

// 暂停
myTween.Pause();

// 播放
myTween.Play();

// 重启
myTween.Restart();

// 杀掉动画 (重要：不用时最好 Kill，防止内存泄漏或逻辑冲突)
myTween.Kill();
```

---

## 3. 序列 (Sequence)

Sequence 用于将多个动画组合在一起，按顺序或并行执行。

```csharp
public void PlaySequence()
{
    // 创建序列
    Sequence mySequence = DOTween.Sequence();

    // 1. 添加一个移动动画 (0s 开始)
    mySequence.Append(transform.DOMoveX(5, 1f));

    // 2. 同时执行旋转 (Join 会和前一个 Append 同时发生)
    mySequence.Join(transform.DORotate(new Vector3(0, 180, 0), 1f));

    // 3. 延迟 0.5 秒
    mySequence.AppendInterval(0.5f);

    // 4. 执行缩放
    mySequence.Append(transform.DOScale(Vector3.zero, 0.5f));

    // 5. 序列结束回调
    mySequence.OnComplete(() => {
        Debug.Log("Sequence Finished");
    });
    
    // 设置整个序列循环
    mySequence.SetLoops(-1);
}
```

---

## 4. 常用技巧与性能优化

1. **设置全局容量**: 在游戏初始化时设置，避免运行时频繁扩容。
   ```csharp
   DOTween.SetTweensCapacity(500, 50); // 同时运行的 Tween 数和 Sequence 数
   ```

2. **回收利用**:
   对于频繁播放的动画（如金币飞行），可以使用 `SetAutoKill(false)` 并配合 `Restart()` 重复使用，避免 GC。

   ```csharp
   Tweener coinTween = transform.DOMoveY(10, 1f).SetAutoKill(false).Pause();
   
   // 需要播放时
   coinTween.Restart();
   ```

3. **异步等待 (Async/Await)**:
   DoTween 支持直接 await。
   ```csharp
   await transform.DOMove(Vector3.zero, 1f).AsyncWaitForCompletion();
   ```
