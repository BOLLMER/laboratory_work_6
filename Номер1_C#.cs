using System;
using System.Collections.Generic;
using System.Threading;

class Program
{
    static void Z1(int M, int N)
    {
        double[,] matrix = new double[M, N];
        Random random = new Random();

        Console.WriteLine("Исходная матрица:");
        for (int i = 0; i < M; ++i)
        {
            for (int j = 0; j < N; ++j)
            {
                matrix[i, j] = Math.Round(random.NextDouble() * 100 - 50, 2);
                Console.Write(matrix[i, j] + " ");
            }
            Console.WriteLine();
        }

        // Переворачиваем строки матрицы
        for (int i = 0; i < M / 2; i++)
        {
            for (int j = 0; j < N; j++)
            {
                double temp = matrix[i, j];
                matrix[i, j] = matrix[M - 1 - i, j];
                matrix[M - 1 - i, j] = temp;
            }
        }

        Console.WriteLine("\nМатрица после переворота строк:");
        for (int i = 0; i < M; ++i)
        {
            for (int j = 0; j < N; ++j)
                Console.Write(matrix[i, j] + " ");
            Console.WriteLine();
        }

        // Собираем уникальные элементы
        SortedSet<double> uniqueElements = new SortedSet<double>();
        for (int i = 0; i < M; ++i)
            for (int j = 0; j < N; ++j)
                uniqueElements.Add(matrix[i, j]);

        List<double> sortedElements = new List<double>(uniqueElements);

        if (sortedElements.Count >= 2)
        {
            double secondMin = sortedElements[1];
            double secondMax = sortedElements[sortedElements.Count - 2];
            Console.WriteLine("\nВторой минимальный элемент: " + secondMin);
            Console.WriteLine("Второй максимальный элемент: " + secondMax);
        }
        else
        {
            Console.WriteLine("\nНедостаточно уникальных элементов для определения второго минимума/максимума");
        }
    }

    static void Z2(int N)
    {
        bool isEven = (N % 2 == 0);
        int N2 = isEven ? N / 2 : (N + 1) / 2;
        int[,] matrix = new int[N2, N2];
        int number = 100;

        // Заполняем верхнюю часть
        for (int i = 0; i < N2; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                matrix[i, j] = number;
                number += 5;
            }
        }

        // Выводим верхнюю часть
        for (int i = 0; i < N2; i++)
        {
            for (int j = 0; j <= i; j++)
            {
                Console.Write(matrix[i, j] + " ");
            }
            Console.WriteLine();
        }

        // Выводим нижнюю часть (перевернутую)
        for (int i = N2 - 1 - (isEven ? 0 : 1); i >= 0; i--)
        {
            for (int j = 0; j <= i; j++)
            {
                Console.Write(matrix[i, j] + " ");
            }
            Console.WriteLine();
        }
    }

    static List<List<string>> life = new List<List<string>>();

    static void InitLife(int I, int J)
    {
        Console.WriteLine("Делаем случайное(1) поле или устойчивую фигуру(2)?");
        int cmd = int.Parse(Console.ReadLine());

        life = new List<List<string>>();
        Random random = new Random();

        if (cmd == 1)
        {
            for (int i = 0; i < I; ++i)
            {
                List<string> row = new List<string>();
                for (int j = 0; j < J; ++j)
                {
                    row.Add(random.Next(2) == 0 ? "." : "#");
                }
                life.Add(row);
            }
        }
        else if (cmd == 2)
        {
            for (int i = 0; i < I; ++i)
            {
                List<string> row = new List<string>();
                for (int j = 0; j < J; ++j)
                {
                    row.Add(".");
                }
                life.Add(row);
            }
            // Создаем устойчивую фигуру (квадрат 2x2)
            life[0][0] = "#";
            life[0][1] = "#";
            life[1][0] = "#";
            life[1][1] = "#";
        }
    }

    static string PixelLife(int i, int j, int rules)
    {
        int count = 0;

        for (int di = -1; di <= 1; di++)
        {
            for (int dj = -1; dj <= 1; dj++)
            {
                if (di == 0 && dj == 0) continue;
                int ni = i + di;
                int nj = j + dj;

                if (ni >= 0 && ni < life.Count && nj >= 0 && nj < life[0].Count)
                {
                    if (life[ni][nj] == "#") count++;
                }
            }
        }

        if (rules == 1)
        {
            if (life[i][j] == ".")
            {
                return (count == 3) ? "#" : ".";
            }
            else
            {
                return (count < 2 || count > 3) ? "." : "#";
            }
        }
        else if (rules == 2)
        {
            if (life[i][j] == ".")
            {
                return (count % 2 == 0 && count != 0) ? "#" : ".";
            }
            else
            {
                return (count % 2 == 1) ? "#" : ".";
            }
        }
        else
        {
            return "X";
        }
    }

    static void PrintLife()
    {
        foreach (var row in life)
        {
            Console.WriteLine(string.Join("", row));
        }
        Console.WriteLine();
    }

    static void LifeGame(int I, int J)
    {
        InitLife(I, J);

        Console.WriteLine("Играем по:\n" +
                         "1. Cтандартным правилам (Живая умирает когда соседей <2 и >3, мертвая воскресает при 3 живых соседях)\n" +
                         "2. Измененным (Живая выживет если кол-во соседей нечётное, мертвая воскреснет если кол-во соседей чётное)\n" +
                         ">>> ");
        int rules = int.Parse(Console.ReadLine());

        while (true)
        {
            Console.Clear();
            PrintLife();

            List<List<string>> nextFrame = new List<List<string>>();
            for (int i = 0; i < I; ++i)
            {
                List<string> row = new List<string>();
                for (int j = 0; j < J; ++j)
                {
                    row.Add(PixelLife(i, j, rules));
                }
                nextFrame.Add(row);
            }

            life = nextFrame;
            Thread.Sleep(1000);
        }
    }

    static void Main(string[] args)
    {
        Console.Write("Какое задание демонстрировать? > ");
        int cmd = int.Parse(Console.ReadLine());
        Console.WriteLine();

        if (cmd == 1)
        {
            Console.WriteLine("Введите через пробел M и N размеры матрицы");
            string[] sizes = Console.ReadLine().Split();
            int M = int.Parse(sizes[0]);
            int N = int.Parse(sizes[1]);
            Z1(M, N);
        }
        else if (cmd == 2)
        {
            Console.WriteLine("Введите количество (N) строк");
            int N = int.Parse(Console.ReadLine());
            Z2(N);
        }
        else if (cmd == 3)
        {
            Console.WriteLine("Введите через пробел размеры поля");
            string[] sizes = Console.ReadLine().Split();
            int I = int.Parse(sizes[0]);
            int J = int.Parse(sizes[1]);
            LifeGame(I, J);
        }
        else
        {
            Console.WriteLine("Введена неверная команда!");
        }
    }
}