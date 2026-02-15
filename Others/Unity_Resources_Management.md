# Unity 资源管理与热更新 (Resources, AB, Addressables)

本文档涵盖了 Unity 三种主要的资源加载方式及 Addressables 热更新流程。

## 1. Resources (旧式加载)
**特点**: 简单方便，但无法热更，增加包体大小，启动慢。
**适用**: 原型开发，极少量不需要热更的配置。

```csharp
using UnityEngine;

public class ResourcesExample : MonoBehaviour
{
    void LoadExample()
    {
        // 同步加载 (资源必须在 Assets/Resources 目录下)
        GameObject prefab = Resources.Load<GameObject>("Characters/Hero");
        Instantiate(prefab);

        // 异步加载
        ResourceRequest request = Resources.LoadAsync<Texture>("Textures/Bg");
        request.completed += (op) => {
            Texture tex = ((ResourceRequest)op).asset as Texture;
            Debug.Log("Loaded: " + tex.name);
        };
    }

    void UnloadExample()
    {
        // 卸载未使用的资源
        Resources.UnloadUnusedAssets();
    }
}
```

---

## 2. AssetBundle (传统热更方案)
**特点**: 灵活，支持热更，但依赖管理复杂，打包繁琐。
**流程**: 打标签 -> 打包 -> 上传 -> 下载 -> 加载 -> 卸载。

### 2.1 打包 (Editor 代码)
```csharp
using UnityEditor;
using System.IO;

public class ABBuilder
{
    [MenuItem("Build/Build AssetBundles")]
    static void BuildAllAssetBundles()
    {
        string dir = "AssetBundles";
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        
        // 打包到指定目录
        BuildPipeline.BuildAssetBundles(dir, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows64);
    }
}
```

### 2.2 加载 (Runtime 代码)
```csharp
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class ABLoader : MonoBehaviour
{
    IEnumerator LoadAB()
    {
        string url = Application.streamingAssetsPath + "/mybundle";
        
        // 1. 加载 Bundle
        UnityWebRequest request = UnityWebRequestAssetBundle.GetAssetBundle(url);
        yield return request.SendWebRequest();

        AssetBundle bundle = DownloadHandlerAssetBundle.GetContent(request);

        // 2. 从 Bundle 加载资源
        GameObject prefab = bundle.LoadAsset<GameObject>("MyCube");
        Instantiate(prefab);

        // 3. 卸载 Bundle (false: 只卸载镜像，保留实例; true: 暴力卸载所有)
        bundle.Unload(false);
    }
}
```

---

## 3. Addressables (新一代热更方案)
**特点**: 基于 AssetBundle 构建，自动管理依赖，API 统一，支持异步，轻松热更。

### 3.1 基础加载 (Runtime)
需引入 `UnityEngine.AddressableAssets` 和 `UnityEngine.ResourceManagement.AsyncOperations`.

```csharp
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressablesExample : MonoBehaviour
{
    // 在 Inspector 中直接拖拽赋值引用
    public AssetReference myPrefabRef;

    void Start()
    {
        // 1. 通过 Key (字符串) 加载
        Addressables.LoadAssetAsync<GameObject>("CubePrefab").Completed += OnLoaded;

        // 2. 通过 Reference 加载并实例化
        myPrefabRef.InstantiateAsync().Completed += (obj) => {
            Debug.Log("Instantiated via Reference");
        };
    }

    private void OnLoaded(AsyncOperationHandle<GameObject> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            Instantiate(handle.Result);
        }
        else
        {
            Debug.LogError("Load Failed");
        }
    }

    void OnDestroy()
    {
        // 释放资源 (重要！)
        // 如果是 InstantiateAsync 创建的，通常使用 ReleaseInstance
        // Addressables.ReleaseInstance(instance);
    }
}
```

### 3.2 Addressables 热更新流程

Addressables 的热更基于 Catalog（目录）和 Bundle 文件的差异对比。

**核心概念**:
- **Remote Load Path**: 远程服务器地址。
- **CheckForCatalogUpdates**: 检查是否有新版本的资源目录。
- **UpdateCatalogs**: 更新目录。
- **DownloadDependenciesAsync**: 下载差异包。

**代码示例**:

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AddressableUpdater : MonoBehaviour
{
    public string updateLabel = "default"; // 通常按 Label 更新，或者更新整个 Catalog

    IEnumerator Start()
    {
        // 1. 初始化
        yield return Addressables.InitializeAsync();

        // 2. 检查目录更新
        // 传入 false 表示不自动释放 handle
        var checkHandle = Addressables.CheckForCatalogUpdates(false);
        yield return checkHandle;

        if (checkHandle.Status == AsyncOperationStatus.Succeeded)
        {
            List<string> catalogs = checkHandle.Result;
            if (catalogs != null && catalogs.Count > 0)
            {
                Debug.Log("Found updates!");
                
                // 3. 更新目录
                var updateHandle = Addressables.UpdateCatalogs(catalogs, false);
                yield return updateHandle;

                // 4. 获取下载大小 (可选，用于显示进度条)
                // 这里传入 key 或 label
                var sizeHandle = Addressables.GetDownloadSizeAsync(updateLabel);
                yield return sizeHandle;
                long totalDownloadSize = sizeHandle.Result;

                if (totalDownloadSize > 0)
                {
                    Debug.Log($"Need to download: {totalDownloadSize} bytes");

                    // 5. 下载依赖资源
                    var downloadHandle = Addressables.DownloadDependenciesAsync(updateLabel, false);
                    
                    while (!downloadHandle.IsDone)
                    {
                        Debug.Log($"Downloading: {downloadHandle.PercentComplete * 100}%");
                        yield return null;
                    }

                    if (downloadHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        Debug.Log("Download Complete!");
                    }
                    Addressables.Release(downloadHandle);
                }
                Addressables.Release(sizeHandle);
                Addressables.Release(updateHandle);
            }
            else
            {
                Debug.Log("No updates found.");
            }
        }
        
        Addressables.Release(checkHandle);
        
        // 进入游戏逻辑...
        LoadGame();
    }

    void LoadGame()
    {
        // 此时加载的资源如果是新的，会自动读取本地缓存的新 Bundle
        Addressables.LoadAssetAsync<GameObject>("MyUpdatedPrefab");
    }
}
```

### 3.3 热更配置步骤简述 (Editor)
1.  **Window > Asset Management > Addressables > Groups**。
2.  创建 Group，将 Profile 设置为 **Remote** (Build Remote Catalog = true, Build & Load Paths = Remote)。
3.  **Manage Profiles**: 设置 RemoteLoadPath 为你的服务器地址 (如 `http://127.0.0.1/CDN/[BuildTarget]`)。
4.  **Build > New Build > Default Build Script**: 生成初始包。
5.  修改资源后，**Build > Update a Previous Build**: 选择之前的 `.bin` 文件生成差异包。
6.  将生成在 `ServerData` 下的文件上传到服务器。
