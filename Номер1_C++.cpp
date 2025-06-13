#include <iostream>
#include <vector>
#include <algorithm>
#include <random>
#include <set>
#include <string>
#include <thread>
#include <chrono>

using namespace std;



void z1(int M, int N) {
    vector<vector<double>> matrix(M, vector<double>(N));

    random_device rd;
    mt19937 gen(rd());
    uniform_real_distribution<> dis(-50, 50);

    cout << "Исходная матрица:\n";
    for (int i = 0; i < M; ++i) {
        for (int j = 0; j < N; ++j) {
            matrix[i][j] = dis(gen);
            cout << matrix[i][j] << " ";
        }
        cout << endl;
    }

    reverse(matrix.begin(), matrix.end());

    cout << "\nМатрица после переворота строк:\n";
    for (auto& row : matrix) {
        for (double num : row)
            cout << num << " ";
        cout << endl;
    }

    set<double> unique_elements;
    for (auto& row : matrix)
        unique_elements.insert(row.begin(), row.end());

    vector<double> sorted_elements(unique_elements.begin(), unique_elements.end());

    double second_min = sorted_elements[1];
    double second_max = sorted_elements[sorted_elements.size() - 2];

    cout << "\nВторой минимальный элемент: " << second_min << endl;
    cout << "Второй максимальный элемент: " << second_max << endl;
}



void z2(int N) {
    bool chet = (N % 2 == 0);
    int N2;
    if (chet) {N2 = N/2;} else {N2 = (N+1)/2;}
    vector<vector<int>> matrix(N2, vector<int>(N2));
    int chisl = 100;
    for (int i = 0; i < N2; i++) {
        for (int j = 0; j < i+1; j++) {
            matrix[i][j] = chisl;
            chisl += 5;
        }
    }
    for (const auto& row : matrix) {
        for (const auto& elem : row) {
            if (elem != 0) {cout << elem << " ";}
        }
        cout << endl;
    }
    reverse(matrix.begin(), matrix.end());
    for (const auto& row : matrix) {
        if (!chet) {
            chet = true;
            continue;
        } else {
            for (const auto& elem : row) {
                if (elem != 0) {cout << elem << " ";}
            }
            cout << endl;
        }
    }
}





vector<vector<string>> life;

void initLife(int I, int J) {
    cout << "Делаем случайное(1) поле или устойчивую фигуру(2)?" << endl;
    int cmd;
    cin >> cmd;
    if (cmd == 1) {
        random_device rd;
        mt19937 gen(rd());
        uniform_int_distribution<> dis(0, 1);

        life.resize(I, vector<string>(J));

        for (int i = 0; i < I; ++i) {
            for (int j = 0; j < J; ++j) {
                life[i][j] = (dis(gen) == 0) ? "." : "#";
            }
        }
    } else if (cmd == 2) {
        life.resize(I, vector<string>(J, "."));
        life[0][0] = "#";
        life[0][1] = "#";
        life[1][0] = "#";
        life[1][1] = "#";
    }
}

string pixellife(int i, int j, int prav) {
    int count = 0;

    for (int di = -1; di <= 1; di++) {
        for (int dj = -1; dj <= 1; dj++) {
            if (di == 0 && dj == 0) continue;
            int ni = i + di;
            int nj = j + dj;

            if (ni >= 0 && ni < life.size() && nj >= 0 && nj < life[0].size()) {
                if (life[ni][nj] == "#") count++;
            }
        }
    }

    if (prav == 1) {
        if (life[i][j] == ".") {
            return (count == 3) ? "#" : ".";
        } else {
            return (count < 2 || count > 3) ? "." : "#";
        }
    } else if (prav == 2) {
        if (life[i][j] == ".") {
            return (count % 2 == 0 && count != 0) ? "#" : ".";
        } else {
            return (count % 2 == 1) ? "#" : ".";
        }
    } else {
        return "X";
    }
}

void printlife() {
    for (auto& row : life) {
        for (auto& cell : row) cout << cell;
        cout << endl;
    }
    cout << endl;
}

void lifegame(int I, int J) {
    initLife(I, J);

    cout << "Играем по:\n"
            "1. Cтандартным правилам (Живая умирает когда соседей <2 и >3, мертвая воскресает при 3 живых соседях)\n"
            "2. Измененным (Живая выживет если кол-во соседей нечётное, мертвая воскреснет если кол-во соседей чётное)\n"
            ">>> ";
    int prav;
    cin >> prav;

    while (true) {
        system("clear");
        printlife();

        vector<vector<string>> nextFrame(I, vector<string>(J));
        for (int i = 0; i < I; ++i) {
            for (int j = 0; j < J; ++j) {
                nextFrame[i][j] = pixellife(i, j, prav);
            }
        }

        life = nextFrame;

        this_thread::sleep_for(chrono::milliseconds(1000));
    }

}


int main() {
    cout << "Какое задание демонстрировать? > ";
    int cmd;
    cin >> cmd;
    cout << endl;
    if (cmd == 1) {
        cout << "Введите через пробел M и N размеры матрицы" << endl;
        int M, N;
        cin >> M >> N;
        z1(M,N);
    } else if (cmd == 2) {
        cout << "Введите количество (N) строк" << endl;
        int N;
        cin >> N;
        z2(N);
    } else if (cmd == 3) {
        cout << "Введите через пробел размеры поля" << endl;
        int I, J;
        cin >> I >> J;
        lifegame(I,J);
    } else {
        cout << "Введена неверная команда!" << endl;
    }
}
