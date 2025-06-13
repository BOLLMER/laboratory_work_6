using System;
using System.Collections.Generic;

class LinearEquationSolver
{
    const int MAX_ITER = 10000;
    const double EPS = 1e-3;
    const int PRINT_STEP = 50;

    static double[] Gauss(double[][] A, double[] b)
    {
        int n = A.Length;
        double[][] a = new double[n][];
        for (int i = 0; i < n; i++)
        {
            a[i] = new double[A[i].Length];
            Array.Copy(A[i], a[i], A[i].Length);
        }
        double[] B = new double[b.Length];
        Array.Copy(b, B, b.Length);
        double[] x = new double[n];

        for (int k = 0; k < n; ++k)
        {
            // Выбор главного элемента
            int pivotRow = k;
            for (int i = k + 1; i < n; ++i)
                if (Math.Abs(a[i][k]) > Math.Abs(a[pivotRow][k]))
                    pivotRow = i;

            // Перестановка строк
            double[] temp = a[k];
            a[k] = a[pivotRow];
            a[pivotRow] = temp;

            double t = B[k];
            B[k] = B[pivotRow];
            B[pivotRow] = t;

            // Исключение переменной
            for (int i = k + 1; i < n; ++i)
            {
                double factor = a[i][k] / a[k][k];
                for (int j = k; j < n; ++j)
                    a[i][j] -= factor * a[k][j];
                B[i] -= factor * B[k];
            }
        }

        // Обратный ход
        for (int i = n - 1; i >= 0; --i)
        {
            double sum = B[i];
            for (int j = i + 1; j < n; ++j)
                sum -= a[i][j] * x[j];
            x[i] = sum / a[i][i];
        }
        return x;
    }

    static double[] Zeidel(double[][] A, double[] b)
    {
        int n = A.Length;
        double[] x = new double[n];
        double[] prev = new double[n];
        bool printedLast = false;

        Console.WriteLine($"\nМетод Зейделя: итерации каждые {PRINT_STEP} шагов + финал");
        Console.Write("{0,8}", "iter");
        for (int i = 0; i < n; ++i) Console.Write("{0,15}", "x" + (i + 1));
        Console.WriteLine("{0,15}", "eps");
        Console.WriteLine(new string('-', 8 + 15 * n + 15));

        for (int iter = 1; iter <= MAX_ITER; ++iter)
        {
            // Вычисляем новую итерацию
            for (int i = 0; i < n; ++i)
            {
                if (Math.Abs(A[i][i]) < 1e-15)
                {
                    Console.Error.WriteLine("Ошибка: нулевой диагональный элемент!");
                    Environment.Exit(1);
                }
                double s = b[i];
                for (int j = 0; j < i; ++j) s -= A[i][j] * x[j];
                for (int j = i + 1; j < n; ++j) s -= A[i][j] * prev[j];
                x[i] = s / A[i][i];
            }

            // Считаем максимальную разницу
            double eps = 0;
            for (int i = 0; i < n; ++i)
                eps = Math.Max(eps, Math.Abs(x[i] - prev[i]));

            // Печатаем, если шаг совпадает или это финал
            if (iter % PRINT_STEP == 0 || eps < EPS)
            {
                Console.Write("{0,8}", iter);
                foreach (double xi in x) Console.Write("{0,15:F6}", xi);
                Console.WriteLine("{0,15:F6}", eps);

                if (eps < EPS)
                {
                    printedLast = true;
                    Console.WriteLine($"\nСошлось за {iter} итераций (eps < {EPS}).");
                    return x;
                }
            }

            Array.Copy(x, prev, n);
        }

        if (!printedLast)
        {
            double eps = 0;
            for (int i = 0; i < n; ++i) eps = Math.Max(eps, Math.Abs(x[i] - prev[i]));
            Console.Write("{0,8}", MAX_ITER);
            foreach (double xi in x) Console.Write("{0,15:F6}", xi);
            Console.WriteLine("{0,15:F6}", eps);
            Console.Error.WriteLine($"Не сошлось за {MAX_ITER} итераций!");
        }
        Environment.Exit(1);
        return null;
    }

    static void Main()
    {
        double M = 0.93, N = 0.07, P = -0.84;
        double[][] A = new double[][]
        {
            new double[] {M, -0.04, 0.21, -18},
            new double[] {0.25, -1.23, N, -0.09},
            new double[] {-0.21, N, 0.8, -0.13},
            new double[] {0.15, -1.31, 0.06, P}
        };
        double[] b = new double[] { -1.24, P, 2.56, M };

        Console.WriteLine("Решение методом Гаусса:");
        double[] gaussSolution = Gauss(A, b);
        foreach (double val in gaussSolution)
        {
            Console.Write("{0,8:F6} ", val);
        }

        Console.WriteLine("\nРешение методом Зейделя:");
        double[] zeidelSolution = Zeidel(A, b);
        foreach (double val in zeidelSolution)
        {
            Console.Write("{0,8:F6} ", val);
        }
        Console.WriteLine();
    }
}