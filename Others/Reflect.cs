using System;
using System.Reflection;

public class Person
{
    public string Name { get; set; }
    public int Age { get; set; }
    private string _secret = "隐藏信息";

    public Person() { }
    public Person(string name, int age)
    {
        Name = name;
        Age = age;
    }

    public void DisplayInfo()
    {
        Console.WriteLine($"姓名: {Name}, 年龄: {Age}");
    }

    private void SecretMethod()
    {
        Console.WriteLine("这是私有方法");
    }
}

class Program
{
    static void Main()
    {
        // 创建对象
        var person = new Person("张三", 25);

        // 1. 获取类型信息
        Type type = person.GetType();
        Console.WriteLine($"类型名称: {type.Name}");
        Console.WriteLine($"命名空间: {type.Namespace}");
        Console.WriteLine($"程序集: {type.Assembly.GetName().Name}");

        // 2. 获取所有公共属性
        Console.WriteLine("\n=== 公共属性 ===");
        PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
        foreach (var prop in properties)
        {
            Console.WriteLine($"属性: {prop.Name}, 类型: {prop.PropertyType}");

            // 读取属性值
            object value = prop.GetValue(person);
            Console.WriteLine($"  值: {value}");

            // 设置属性值
            if (prop.Name == "Age")
            {
                prop.SetValue(person, 30);
                Console.WriteLine($"  修改后年龄: {person.Age}");
            }
        }

        // 3. 获取所有公共方法
        Console.WriteLine("\n=== 公共方法 ===");
        MethodInfo[] methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance);
        foreach (var method in methods)
        {
            if (method.DeclaringType == type) // 只显示本类定义的方法
            {
                Console.WriteLine($"方法: {method.Name}, 返回类型: {method.ReturnType}");
            }
        }

        // 4. 动态调用方法
        Console.WriteLine("\n=== 动态调用方法 ===");
        MethodInfo displayMethod = type.GetMethod("DisplayInfo");
        displayMethod.Invoke(person, null);

        // 5. 创建对象实例
        Console.WriteLine("\n=== 动态创建对象 ===");
        object newPerson = Activator.CreateInstance(type);
        PropertyInfo nameProp = type.GetProperty("Name");
        nameProp.SetValue(newPerson, "李四");

        MethodInfo newDisplayMethod = type.GetMethod("DisplayInfo");
        newDisplayMethod.Invoke(newPerson, null);
    }
}

// 反射获取的区别

class ReflectionDemo
{
    static void Main()
    {
        Type type = typeof(Program);

        Console.WriteLine("=== PropertyInfo ===");
        PropertyInfo[] properties = type.GetProperties(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        );
        foreach (var prop in properties)
        {
            Console.WriteLine($"属性: {prop.Name}");
            Console.WriteLine($"  类型: {prop.PropertyType.Name}");
            Console.WriteLine($"  可读: {prop.CanRead}");
            Console.WriteLine($"  可写: {prop.CanWrite}");
            Console.WriteLine($"  声明类型: {prop.DeclaringType.Name}");
            Console.WriteLine();
        }

        Console.WriteLine("=== FieldInfo ===");
        FieldInfo[] fields = type.GetFields(
            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
        );
        foreach (var field in fields)
        {
            Console.WriteLine($"字段: {field.Name}");
            Console.WriteLine($"  类型: {field.FieldType.Name}");
            Console.WriteLine($"  访问级别: {field.Attributes}");
            Console.WriteLine($"  声明类型: {field.DeclaringType.Name}");
            Console.WriteLine($"  是否只读: {field.IsInitOnly}");
            Console.WriteLine();
        }
    }
}
