# UniTask 工具库常用使用方法

UniTask 是一个为 Unity 量身定制的高性能 async/await 库，解决了原生 Task 在 Unity 中的内存分配和主线程调度问题。

## 1. 基础用法

### 1.1 安装
通常通过 UPM (Unity Package Manager) 安装 Git URL: `https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask`

### 1.2 替代 Coroutine
UniTask 可以完全替代协程，写起来更像同步代码。

```csharp
using Cysharp.Threading.Tasks;
using UnityEngine;

public class UniTaskBasics : MonoBehaviour
{
    private void Start()
    {
        // 启动异步方法，不等待其结束（Fire and Forget）
        RunGameLogic().Forget();
    }

    private async UniTaskVoid RunGameLogic()
    {
        Debug.Log("Start");

        // 等待 1 秒 (类似于 yield return new WaitForSeconds(1))
        await UniTask.Delay(1000); 

        // 等待下一帧 (类似于 yield return null)
        await UniTask.Yield();

        // 等待物理帧 (类似于 yield return new WaitForFixedUpdate())
        await UniTask.WaitForFixedUpdate();

        Debug.Log("End");
    }
}
```

---

## 2. 常用 API

### 2.1 等待条件

```csharp
// 等待直到条件满足
await UniTask.WaitUntil(() => transform.position.y < 0);

// 等待直到值改变
await UniTask.WaitUntilValueChanged(this.transform, x => x.position);
```

### 2.2 异步加载资源

```csharp
// 需引入 Cysharp.Threading.Tasks.Linq 或相关扩展
using UnityEngine.Networking;

public async UniTask LoadTextureAsync(string url)
{
    using (var uwr = UnityWebRequestTexture.GetTexture(url))
    {
        // 等待 WebRequest 发送完成
        await uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.Success)
        {
            var texture = DownloadHandlerTexture.GetContent(uwr);
            // 使用 texture...
        }
    }
}
```

### 2.3 切换线程
UniTask 允许在主线程和线程池之间切换。

```csharp
public async UniTask CalculateHeavyTask()
{
    // 切换到线程池执行耗时操作
    await UniTask.SwitchToThreadPool();
    
    int result = 0;
    for(int i=0; i<1000000; i++) result += i;

    // 切换回主线程更新 UI
    await UniTask.SwitchToMainThread();
    Debug.Log($"Result: {result}");
}
```

---

## 3. 并行与等待

### 3.1 WhenAll (等待所有完成)

```csharp
public async UniTask LoadAllAssets()
{
    var task1 = UniTask.Delay(1000);
    var task2 = UniTask.Delay(2000);
    var task3 = LoadConfigAsync();

    // 并行执行，等待所有任务完成
    await UniTask.WhenAll(task1, task2, task3);
    Debug.Log("All Loaded");
}
```

### 3.2 WhenAny (等待任意一个完成)

```csharp
public async UniTask RaceCondition()
{
    var taskA = UniTask.Delay(5000); // 超时任务
    var taskB = WaitPlayerInput();   // 玩家操作

    // 哪个先完成就返回哪个的索引
    int winIndex = await UniTask.WhenAny(taskA, taskB);
    
    if (winIndex == 0) Debug.Log("Timeout!");
    else Debug.Log("Player Input Received");
}
```

---

## 4. 取消操作 (CancellationToken)

在 GameObject 销毁时，异步任务应当停止，否则可能导致空引用异常。

```csharp
using System.Threading;

public class CancelExample : MonoBehaviour
{
    private CancellationTokenSource _cts;

    private void Start()
    {
        _cts = new CancellationTokenSource();
        DoTask(_cts.Token).Forget();
    }

    private async UniTaskVoid DoTask(CancellationToken token)
    {
        // 传递 token，如果取消则抛出 OperationCanceledException
        await UniTask.Delay(3000, cancellationToken: token);
        
        // 或者使用 MonoBehaviour 的 destroyCancellationToken (Unity 2022.2+)
        // await UniTask.Delay(3000, cancellationToken: this.GetCancellationTokenOnDestroy());
        
        Debug.Log("Finished");
    }

    private void OnDestroy()
    {
        // 取消任务
        _cts?.Cancel();
        _cts?.Dispose();
    }
}
```

## 5. 常见问题
- **UniTaskVoid**: 用于“发后不理”的场景（如 Start, Button Click）。
- **UniTask**: 用于需要 `await` 的场景。
- **UniTask<T>**: 用于需要返回值的场景。
