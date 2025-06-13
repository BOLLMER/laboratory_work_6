#include <iostream>

using namespace std;

int calculate(int& n, int& m, unsigned long long& t) {
    if (n <= 0 || m <= 0 || t <= 0) {
        throw invalid_argument("Введённые данные должны быть положительными!");
    }
    int weight = 0;
    while (n > 2 && m > 2) {
        unsigned long long line = 4 + (n-2)*2 + (m-2)*2;
        if (t >= line) {
            t -= line;
            n -= 2;
            m -= 2;
            weight++;
        } else {
            break;
        }
    }
    return weight;
}

int main() {
    int n, m;
    unsigned long long t;
    cout << "Введите размеры площади через пробел" << endl;
    cin >> n >> m;
    cout << "Введите количество имеющихся плиток" << endl;
    cin >> t;
    try {
        int result = calculate(n, m, t);
        cout << "Максимально доступная ширина дорожки: " << result << endl;
    } catch (const exception& e) {
        cout << e.what() << endl;
    }
}
