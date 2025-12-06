using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class Example : MonoBehaviour
{
    [System.Serializable]
    public class Player
    {
        public string Name;
        public int Level;
        public int Score;
        public string ClassType;
    }
    string x { get; }
    //[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void StartGame()
    {
        ///mathf，基本运算
        float a = 5.7f;
        float b = 3.2f;

        //1.基本运算
        float absvalue = Mathf.Abs(-10.8f);//绝对运算：10.5
        float sqrtvalue = Mathf.Sqrt(16f);//平方根：4
        float powValue = Mathf.Pow(2f, 3f);//幕

        //lerp插值
        float lerpedValue = Mathf.Lerp(a, b, 0.5f);

        //限制 
        float clampedValue = Mathf.Clamp(12f, 0f, 10f);
        float clamped01 = Mathf.Clamp01(5.5f);

        //三角函数
        float sinVclue = Mathf.Sin(45f * Mathf.Deg2Rad);
        float cosValue = Mathf.Cos(45f * Mathf.Deg2Rad); // 余弦

        // 5. 最大值和最小值
        float maxValue = Mathf.Max(1, 5, 3, 9, 2); // 9
        float minValue = Mathf.Min(1, 5, 3, 9, 2); // 1

        
        ///Linq
        List<Player> players = new List<Player>
        {
        new Player { Name = "Warrior", Level = 5, Score = 1000, ClassType = "Melee" },
        new Player { Name = "Mage", Level = 8, Score = 1500, ClassType = "Ranged" },
        new Player { Name = "Archer", Level = 3, Score = 800, ClassType = "Ranged" },
        new Player { Name = "Tank", Level = 10, Score = 2000, ClassType = "Melee" }
        };


        List<int> numbers = new List<int> { 1, 2, 3, 4, 5 };
        //int[] numbers = { 1, 2, 3, 4, 5 };
        bool hasEvenNumber = numbers.Any(n => n % 2 == 0); // 是否有偶数？ true
        bool allPositive = numbers.All(n => n > 0); // 是否所有数都大于0？ true

        // 1. 筛选：所有远程职业
        var rangedPlayers = players.Where(p => p.ClassType == "Ranged");

        // 2. 排序：按等级降序排列
        var sortedByLevel = players.OrderByDescending(p => p.Level);

        // 3. 选择：只获取玩家名字和分数
        var nameAndScore = players.Select(p => new { p.Name, p.Score });

        // 4. 聚合：计算总分和平均分
        float totalScore = players.Sum(p => p.Score);
        double averageLevel = players.Average(p => p.Level);

        // 5. 分组：按职业类型分组
        var groupedByClass = players.GroupBy(p => p.ClassType);
        foreach (var group in groupedByClass)
        {
            Debug.Log($"职业: {group.Key}, 人数: {group.Count()}");
        }

        // 6. 组合查询：高等级的近战职业
        var highLevelMelee = players
            .Where(p => p.ClassType == "Melee" && p.Level > 4)
            .OrderByDescending(p => p.Score)
            .Select(p => p.Name);

        var _numbers = numbers.ToArray();

        Debug.Log("高等级近战职业: " + string.Join(", ", highLevelMelee));

        
    }
    private void Update()
    {
        ///Other
        // 球形检测
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f);

        // 盒形检测
        Collider[] boxColliders = Physics.OverlapBox(
            transform.position,
            Vector3.one,
            transform.rotation
        );
    }

}
