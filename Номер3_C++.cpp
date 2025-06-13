#include <iostream>
#include <vector>
#include <cmath>
#include <iomanip>

using namespace std;

typedef vector<vector<double>> Matrix;
typedef vector<double> Line;

const int MAX_ITER = 10000; //максимальное количество итераций для метода Зейделя
const double EPS = 1e-3; //точность, при достижении которой итерационный процесс останавливается
const int PRINT_STEP = 50;//шаг вывода промежуточных результатов

Line gauss(const Matrix& A_in, const Line& b_in) { //метод Гаусса
    int n = A_in.size();
    Matrix A = A_in;
    Line b = b_in;
    Line x(n, 0.0);

    for (int k = 0; k < n; ++k) { // Поиск строки с максимальным элементом в текущем столбце
        int pivotRow = k;
        for (int i = k + 1; i < n; ++i)
            if (fabs(A[i][k]) > fabs(A[pivotRow][k]))
                pivotRow = i;
        // Обмен строк местами
        swap(A[k], A[pivotRow]);
        swap(b[k], b[pivotRow]);

        for (int i = k + 1; i < n; ++i) { // преобразование матрицы к треугольному виду путем вычитания строк
            double factor = A[i][k] / A[k][k];
            for (int j = k; j < n; ++j)
                A[i][j] -= factor * A[k][j];
            b[i] -= factor * b[k];
        }
    }

    // Обратный ход
    //вычисляются значения неизвестных, начиная с последнего уравнения
    for (int i = n - 1; i >= 0; --i) {
        double sum = b[i];
        for (int j = i + 1; j < n; ++j)
            sum -= A[i][j] * x[j];
        x[i] = sum / A[i][i];
    }
    return x;
}

Line zeidel(const Matrix& A, const Line& b) {//метод Зейделя
    int n = A.size();
    //векторы для текущего и предыдущего приближений
    Line x(n, 0.0), prev(n, 0.0);
    bool printedLast = false;

    // Шапка таблицы
    cout << "\nМетод Зейделя: итерации каждые " << PRINT_STEP
         << " шагов + финал\n";
    cout << setw(8) << "iter";
    for (int i = 0; i < n; ++i) cout << setw(15) << ("x"+to_string(i+1));
    cout << setw(15) << "eps\n";
    cout << string(8 + 15*n + 15, '-') << "\n";

    for (int iter = 1; iter <= MAX_ITER; ++iter) {
        // вычисляем новую итерацию
        for (int i = 0; i < n; ++i) {
            if (fabs(A[i][i]) < 1e-15) {
                cerr << "Ошибка: нулевой диагональный элемент!\n";
                exit(1);
            }
            double s = b[i];
            for (int j = 0; j < i; ++j) s -= A[i][j] * x[j];
            for (int j = i+1; j < n; ++j) s -= A[i][j] * prev[j];
            x[i] = s / A[i][i];
        }
        // считаем максимальную разницу
        double eps = 0;
        for (int i = 0; i < n; ++i)
            eps = max(eps, fabs(x[i] - prev[i]));

        // печатаем, если шаг совпадает или это финал
        if (iter % PRINT_STEP == 0 || eps < EPS) {
            cout << setw(8) << iter;
            for (double xi : x) cout << setw(15) << xi;
            cout << setw(15) << eps << "\n";
            if (eps < EPS) {
                printedLast = true;
                cout << "\nСошлось за " << iter
                     << " итераций (eps < " << EPS << ").\n";
                return x;
            }
        }

        prev = x;
    }

    if (!printedLast) {
        // если по какой-то причине не попали на финальный принт
        double eps = 0;
        for (int i = 0; i < n; ++i) eps = max(eps, fabs(x[i] - prev[i]));
        cout << setw(8) << MAX_ITER;
        for (double xi : x) cout << setw(15) << xi;
        cout << setw(15) << eps << "\n";
        cerr << "Не сошлось за " << MAX_ITER << " итераций!\n";
    }
    exit(1);
}

int main() {
    double M = 0.93, N = 0.07, P = -0.84;
    Matrix A = {
        {M, -0.04, 0.21, -18},
        {0.25, -1.23, N, -0.09},
        {-0.21, N, 0.8, -0.13},
        {0.15, -1.31, 0.06, P}
    };
    Line b = {-1.24, P, 2.56, M};

    cout << "Решение методом Гаусса:" << endl;
    for (auto i : gauss(A, b)) {
        cout << setw(6) << i << " "; // конкретизировать
    }
    cout << endl << "Решение методом Зейделя:" << endl;
    for (auto i : zeidel(A, b)) {
        cout << setw(6) << i << " ";
    }
    cout << endl;
}
