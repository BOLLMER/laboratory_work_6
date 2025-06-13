using System;

class Program
{
    static int Calculate(ref int n, ref int m, ref ulong t)
    {
        if (n <= 0 || m <= 0 || t <= 0)
        {
            throw new ArgumentException("Введённые данные должны быть положительными!");
        }

        int weight = 0;
        while (n > 2 && m > 2)
        {
            ulong line = 4 + (ulong)((n - 2) * 2) + (ulong)((m - 2) * 2);
            if (t >= line)
            {
                t -= line;
                n -= 2;
                m -= 2;
                weight++;
            }
            else
            {
                break;
            }
        }
        return weight;
    }

    static void Main(string[] args)
    {
        Console.WriteLine("Введите размеры площади через пробел");
        string[] dimensions = Console.ReadLine().Split();
        int n = int.Parse(dimensions[0]);
        int m = int.Parse(dimensions[1]);

        Console.WriteLine("Введите количество имеющихся плиток");
        ulong t = ulong.Parse(Console.ReadLine());

        try
        {
            int result = Calculate(ref n, ref m, ref t);
            Console.WriteLine($"Максимально доступная ширина дорожки: {result}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
    }
}