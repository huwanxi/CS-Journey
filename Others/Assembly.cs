using System;
using System.Reflection;
using UnityEngine;

// Assembly 类用于表示一个程序集（.dll 或 .exe 文件）
// 它提供了访问程序集元数据、类型信息等的能力
public class AssemblyExample : MonoBehaviour
{
    void Start()
    {
        // 1. 加载程序集
        LoadAssemblyExample();

        // 2. 获取当前程序集
        GetCurrentAssemblyExample();

        // 3. 获取程序集信息
        GetAssemblyInfoExample();
    }

    void LoadAssemblyExample()
    {
        try
        {
            // 方法1: 通过程序集名称加载
            Assembly assembly1 = Assembly.Load("MyPluginAssembly");

            // 方法2: 通过完整名称加载
            Assembly assembly2 = Assembly.Load("MyPlugin, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null");

            // 方法3: 通过文件路径加载（需要绝对路径）
            // Assembly assembly3 = Assembly.LoadFrom(Application.dataPath + "/Plugins/MyPlugin.dll");

            Debug.Log("程序集加载成功: " + assembly1.FullName);
        }
        catch (Exception e)
        {
            Debug.LogError("程序集加载失败: " + e.Message);
        }
    }

    void GetCurrentAssemblyExample()
    {
        // 获取当前执行的程序集
        Assembly currentAssembly = Assembly.GetExecutingAssembly();
        Debug.Log("当前程序集: " + currentAssembly.FullName);

        // 获取入口程序集（通常是主程序）
        Assembly entryAssembly = Assembly.GetEntryAssembly();
        if (entryAssembly != null)
        {
            Debug.Log("入口程序集: " + entryAssembly.FullName);
        }
    }

    void GetAssemblyInfoExample()
    {
        Assembly assembly = Assembly.GetExecutingAssembly();

        // 获取程序集名称
        AssemblyName assemblyName = assembly.GetName();
        Debug.Log($"名称: {assemblyName.Name}");
        Debug.Log($"版本: {assemblyName.Version}");
        Debug.Log($"架构: {assemblyName.ProcessorArchitecture}");

        // 获取程序集属性（如版本信息等）
        object[] attributes = assembly.GetCustomAttributes(typeof(AssemblyVersionAttribute), false);
        if (attributes.Length > 0)
        {
            AssemblyVersionAttribute versionAttr = (AssemblyVersionAttribute)attributes[0];
            Debug.Log("版本属性: " + versionAttr.Version);
        }
    }
}