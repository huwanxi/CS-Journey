using System;

// 1. 创建自定义 Attribute
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property)]
public class MyCustomAttribute : Attribute
{
    public string Description { get; }
    public int Priority { get; set; }

    public MyCustomAttribute(string description)
    {
        Description = description;
        Priority = 1;
    }

    // 添加一个简单的方法
    public void PrintInfo()
    {
        Console.WriteLine($"描述: {Description}, 优先级: {Priority}");
    }
}

// 2. 在类、方法、属性上使用 Attribute
[MyCustom("这是一个用户服务类", Priority = 2)]
public class UserService
{
    [MyCustom("用户名属性")]
    public string UserName { get; set; }

    [MyCustom("处理用户的方法")]
    public void ProcessUser()
    {
        Console.WriteLine("处理用户...");
    }

    // 没有 Attribute 的方法
    public void NormalMethod()
    {
        Console.WriteLine("普通方法...");
    }
}


public class AttributeReader
{
    // 读取类的 Attribute
    public static void ReadClassAttributes<T>() where T : class
    {
        Type type = typeof(T);

        // 获取类上的所有 MyCustomAttribute
        var attributes = type.GetCustomAttributes(typeof(MyCustomAttribute), false);

        foreach (MyCustomAttribute attr in attributes)
        {
            Console.WriteLine($"类 '{type.Name}' 的 Attribute:");
            attr.PrintInfo();
        }
    }

    // 读取类的所有方法和属性的 Attribute
    public static void ReadAllAttributes<T>() where T : class
    {
        Type type = typeof(T);

        Console.WriteLine($"=== 分析类: {type.Name} ===");

        // 1. 读取类本身的 Attribute
        var classAttributes = type.GetCustomAttributes(typeof(MyCustomAttribute), false);
        foreach (MyCustomAttribute attr in classAttributes)
        {
            Console.WriteLine($"[类] {type.Name}: {attr.Description} (优先级: {attr.Priority})");
        }

        // 2. 读取所有属性的 Attribute
        var properties = type.GetProperties();
        foreach (var property in properties)
        {
            var propAttributes = property.GetCustomAttributes(typeof(MyCustomAttribute), false);
            foreach (MyCustomAttribute attr in propAttributes)
            {
                Console.WriteLine($"[属性] {property.Name}: {attr.Description}");
            }
        }

        // 3. 读取所有方法的 Attribute
        var methods = type.GetMethods();
        foreach (var method in methods)
        {
            var methodAttributes = method.GetCustomAttributes(typeof(MyCustomAttribute), false);
            foreach (MyCustomAttribute attr in methodAttributes)
            {
                Console.WriteLine($"[方法] {method.Name}: {attr.Description}");
            }
        }
    }

    // 执行带有特定 Attribute 的方法
    public static void ExecuteMethodsWithAttribute<T>(T instance) where T : class
    {
        Type type = typeof(T);
        var methods = type.GetMethods();

        foreach (var method in methods)
        {
            var attributes = method.GetCustomAttributes(typeof(MyCustomAttribute), false);
            if (attributes.Length > 0)
            {
                Console.WriteLine($"执行带有 Attribute 的方法: {method.Name}");
                method.Invoke(instance, null); // 调用方法
            }
        }
    }
}