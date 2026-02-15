# IO 工具库常用使用方法

本文档总结了 Unity开发中常用的 IO 操作、JSON 序列化/反序列化以及数据存储方法。

## 1. System.IO 常用操作

`System.IO` 命名空间提供了文件和目录的基本操作。

### 1.1 路径处理 (Path)

```csharp
using System.IO;
using UnityEngine;

public class PathExample
{
    public void ShowPaths()
    {
        // 常用路径
        string persistentDataPath = Application.persistentDataPath; // 可读写，适合热更资源、存档
        string streamingAssetsPath = Application.streamingAssetsPath; // 只读（Android），流式资源
        string dataPath = Application.dataPath; // 编辑器下 Assets 目录

        // 路径拼接 (自动处理分隔符)
        string fullPath = Path.Combine(persistentDataPath, "SaveData", "player.json");
        
        // 获取文件名/扩展名
        string fileName = Path.GetFileName(fullPath); // player.json
        string extension = Path.GetExtension(fullPath); // .json
        string dirName = Path.GetDirectoryName(fullPath); // .../SaveData
    }
}
```

### 1.2 目录操作 (Directory)

```csharp
using System.IO;

public static class DirectoryUtils
{
    // 创建目录（如果不存在）
    public static void CreateDirIfNotExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    // 删除目录 (recursive: true 表示递归删除子目录)
    public static void DeleteDir(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
    }
    
    // 获取目录下所有文件
    public static string[] GetAllFiles(string path, string searchPattern = "*.*")
    {
        if (!Directory.Exists(path)) return new string[0];
        return Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
    }
}
```

### 1.3 文件读写 (File)

```csharp
using System.IO;
using System.Text;

public static class FileUtils
{
    // 写入文本
    public static void WriteText(string path, string content)
    {
        // 确保目录存在
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

        File.WriteAllText(path, content, Encoding.UTF8);
    }

    // 读取文本
    public static string ReadText(string path)
    {
        if (!File.Exists(path)) return null;
        return File.ReadAllText(path, Encoding.UTF8);
    }

    // 写入字节
    public static void WriteBytes(string path, byte[] bytes)
    {
        string dir = Path.GetDirectoryName(path);
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        
        File.WriteAllBytes(path, bytes);
    }

    // 读取字节
    public static byte[] ReadBytes(string path)
    {
        if (!File.Exists(path)) return null;
        return File.ReadAllBytes(path);
    }
}
```

---

## 2. JSON 序列化与反序列化

### 2.1 JsonUtility (Unity 内置)
优点：高性能，无需额外库。
缺点：功能有限，不支持字典、顶层数组、私有字段（需加 `[SerializeField]`）。

```csharp
using UnityEngine;
using System;

[Serializable]
public class PlayerData
{
    public string name;
    public int level;
    public Vector3 position; // 支持 Unity 内置类型
}

public class JsonUtilityExample
{
    public void Test()
    {
        PlayerData data = new PlayerData { name = "Hero", level = 10, position = Vector3.zero };

        // 序列化
        string json = JsonUtility.ToJson(data, true); // true for pretty print
        Debug.Log(json);

        // 反序列化
        PlayerData loadedData = JsonUtility.FromJson<PlayerData>(json);
        
        // 覆盖现有对象数据
        JsonUtility.FromJsonOverwrite(json, data);
    }
}
```

### 2.2 Newtonsoft.Json (Json.NET)
优点：功能强大，支持字典、复杂嵌套、私有成员等。
注意：需在 Package Manager 安装 `Newtonsoft Json` 包。

```csharp
using Newtonsoft.Json;
using System.Collections.Generic;

public class ComplexData
{
    public int Id { get; set; }
    public Dictionary<string, int> Inventory; // 支持字典
}

public class NewtonsoftExample
{
    public void Test()
    {
        ComplexData data = new ComplexData 
        { 
            Id = 1, 
            Inventory = new Dictionary<string, int> { { "Potion", 5 }, { "Sword", 1 } } 
        };

        // 序列化
        string json = JsonConvert.SerializeObject(data);

        // 反序列化
        ComplexData loaded = JsonConvert.DeserializeObject<ComplexData>(json);
    }
}
```

---

## 3. 数据存储常用方式

### 3.1 PlayerPrefs (轻量级)
适合存储简单的设置，如音量、是否首次启动等。

```csharp
using UnityEngine;

public class PrefsExample
{
    public void SaveSettings()
    {
        PlayerPrefs.SetInt("Score", 100);
        PlayerPrefs.SetFloat("Volume", 0.8f);
        PlayerPrefs.SetString("PlayerName", "Guest");
        
        // 建议手动保存，虽然退出时会自动保存
        PlayerPrefs.Save();
    }

    public void LoadSettings()
    {
        int score = PlayerPrefs.GetInt("Score", 0); // 默认值 0
        float volume = PlayerPrefs.GetFloat("Volume", 1.0f);
        string name = PlayerPrefs.GetString("PlayerName", "");
    }
}
```

### 3.2 自定义 JSON 存档系统 (示例)

```csharp
using System.IO;
using UnityEngine;

public static class SaveSystem
{
    public static void SaveByJson<T>(string fileName, T data)
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);
    }

    public static T LoadByJson<T>(string fileName) where T : new()
    {
        string path = Path.Combine(Application.persistentDataPath, fileName);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<T>(json);
        }
        return new T();
    }
}
```

---

## 4. 常用文档格式读取 (CSV & XML)

除了 JSON，游戏中常用于配置表的格式还有 CSV 和 XML。

### 4.1 CSV 读取
CSV (Comma-Separated Values) 结构简单，常用于 Excel 导出的配置表。

```csharp
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class CSVLoader
{
    // 假设 CSV 格式: ID,Name,Price
    // 1,Apple,10
    // 2,Banana,20
    
    public void LoadCSV(string csvContent)
    {
        using (StringReader reader = new StringReader(csvContent))
        {
            string line;
            bool isHeader = true;
            
            while ((line = reader.ReadLine()) != null)
            {
                if (isHeader) 
                { 
                    isHeader = false; 
                    continue; // 跳过表头
                }
                
                if (string.IsNullOrWhiteSpace(line)) continue;

                string[] values = line.Split(',');
                
                int id = int.Parse(values[0]);
                string name = values[1];
                int price = int.Parse(values[2]);
                
                Debug.Log($"Item: {id}, {name}, {price}");
            }
        }
    }
}
```

### 4.2 XML 读取
XML 结构严谨，Unity 支持使用 `XmlSerializer` 进行反序列化。

```csharp
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

// 定义对应 XML 结构的数据类
[System.Serializable]
public class Item
{
    [XmlAttribute("id")]
    public int Id;
    public string Name;
    public int Price;
}

[System.Serializable]
[XmlRoot("Items")]
public class ItemDatabase
{
    [XmlElement("Item")]
    public List<Item> itemList;
}

public class XMLLoader
{
    public void LoadXML(string xmlContent)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(ItemDatabase));
        
        using (StringReader reader = new StringReader(xmlContent))
        {
            ItemDatabase db = (ItemDatabase)serializer.Deserialize(reader);
            
            foreach(var item in db.itemList)
            {
                Debug.Log($"XML Item: {item.Id} - {item.Name}");
            }
        }
    }
}
```

---

## 5. 特殊平台文件读取注意 (StreamingAssets)

在 Android/WebGL 平台，`StreamingAssets` 目录下的文件被压缩在包内，无法使用 `System.IO.File` 直接读取，必须使用 `UnityWebRequest`。

```csharp
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class StreamingAssetsLoader : MonoBehaviour
{
    public void ReadFile(string fileName)
    {
        StartCoroutine(ReadRoutine(fileName));
    }

    IEnumerator ReadRoutine(string fileName)
    {
        string path = Path.Combine(Application.streamingAssetsPath, fileName);
        
        // Android 路径通常包含 jar:file://...
        // WebGL 路径通常是 http://...
        // 使用 UnityWebRequest 可以自动处理这些差异
        
        using (UnityWebRequest uwr = UnityWebRequest.Get(path))
        {
            yield return uwr.SendWebRequest();

            if (uwr.result == UnityWebRequest.Result.Success)
            {
                string content = uwr.downloadHandler.text;
                Debug.Log("File Content: " + content);
            }
            else
            {
                Debug.LogError("Error reading file: " + uwr.error);
            }
        }
    }
}
```
