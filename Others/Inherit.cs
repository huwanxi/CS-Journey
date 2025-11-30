using System;

public class Animal
{
    public string Name { get; }
    public int Age { get; }

    // 基类构造函数
    public Animal(string name, int age)
    {
        Name = name;
        Age = age;
        Console.WriteLine($"Animal 构造函数: {name}, {age}岁");
    }

    // 基类重载构造函数
    public Animal(string name) : this(name, 0)  // 调用本类其他构造函数
    {
        Console.WriteLine($"Animal 单参数构造函数: {name}");
    }
}

public class Dog : Animal
{
    public string Breed { get; }

    // 子类必须调用基类构造函数
    public Dog(string name, int age, string breed) : base(name, age)  // 调用基类构造函数
    {
        Breed = breed;
        Console.WriteLine($"Dog 构造函数: 品种 {breed}");
    }

    // 调用基类的不同构造函数
    public Dog(string name, string breed) : base(name)  // 调用基类单参数构造函数
    {
        Breed = breed;
        Console.WriteLine($"Dog 单参数构造函数: 品种 {breed}");
    }
}

// 使用示例
//var dog1 = new Dog("Buddy", 3, "Golden Retriever");顶级语句
// 输出:
// Animal 构造函数: Buddy, 3岁
// Dog 构造函数: 品种 Golden Retriever

//var dog2 = new Dog("Max", "Labrador");
// 输出:
// Animal 构造函数: Max, 0岁
// Animal 单参数构造函数: Max
// Dog 单参数构造函数: 品种 Labrador
