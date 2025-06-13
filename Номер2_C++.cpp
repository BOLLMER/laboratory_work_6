#include <iostream>
#include <string>
#include <random>
#include <iomanip>
#include <vector>
#include <cstdint>
#include <fstream>
#include <iterator>
#include <sstream>
#include <array>
#include <algorithm>
#include <limits>

using namespace std;
using Byte = uint8_t;
using Block = array<Byte, 16>;

static const Byte SBOX[256] = {
    0x63,0x7c,0x77,0x7b,0xf2,0x6b,0x6f,0xc5,0x30,0x01,0x67,0x2b,0xfe,0xd7,0xab,0x76,
    0xca,0x82,0xc9,0x7d,0xfa,0x59,0x47,0xf0,0xad,0xd4,0xa2,0xaf,0x9c,0xa4,0x72,0xc0,
    0xb7,0xfd,0x93,0x26,0x36,0x3f,0xf7,0xcc,0x34,0xa5,0xe5,0xf1,0x71,0xd8,0x31,0x15,
    0x04,0xc7,0x23,0xc3,0x18,0x96,0x05,0x9a,0x07,0x12,0x80,0xe2,0xeb,0x27,0xb2,0x75,
    0x09,0x83,0x2c,0x1a,0x1b,0x6e,0x5a,0xa0,0x52,0x3b,0xd6,0xb3,0x29,0xe3,0x2f,0x84,
    0x53,0xd1,0x00,0xed,0x20,0xfc,0xb1,0x5b,0x6a,0xcb,0xbe,0x39,0x4a,0x4c,0x58,0xcf,
    0xd0,0xef,0xaa,0xfb,0x43,0x4d,0x33,0x85,0x45,0xf9,0x02,0x7f,0x50,0x3c,0x9f,0xa8,
    0x51,0xa3,0x40,0x8f,0x92,0x9d,0x38,0xf5,0xbc,0xb6,0xda,0x21,0x10,0xff,0xf3,0xd2,
    0xcd,0x0c,0x13,0xec,0x5f,0x97,0x44,0x17,0xc4,0xa7,0x7e,0x3d,0x64,0x5d,0x19,0x73,
    0x60,0x81,0x4f,0xdc,0x22,0x2a,0x90,0x88,0x46,0xee,0xb8,0x14,0xde,0x5e,0x0b,0xdb,
    0xe0,0x32,0x3a,0x0a,0x49,0x06,0x24,0x5c,0xc2,0xd3,0xac,0x62,0x91,0x95,0xe4,0x79,
    0xe7,0xc8,0x37,0x6d,0x8d,0xd5,0x4e,0xa9,0x6c,0x56,0xf4,0xea,0x65,0x7a,0xae,0x08,
    0xba,0x78,0x25,0x2e,0x1c,0xa6,0xb4,0xc6,0xe8,0xdd,0x74,0x1f,0x4b,0xbd,0x8b,0x8a,
    0x70,0x3e,0xb5,0x66,0x48,0x03,0xf6,0x0e,0x61,0x35,0x57,0xb9,0x86,0xc1,0x1d,0x9e,
    0xe1,0xf8,0x98,0x11,0x69,0xd9,0x8e,0x94,0x9b,0x1e,0x87,0xe9,0xce,0x55,0x28,0xdf,
    0x8c,0xa1,0x89,0x0d,0xbf,0xe6,0x42,0x68,0x41,0x99,0x2d,0x0f,0xb0,0x54,0xbb,0x16
};

static const Byte RCON[11] = {
    0x00,
    0x01, 0x02, 0x04, 0x08,
    0x10, 0x20, 0x40, 0x80,
    0x1B, 0x36
};

array<Block,11> getRoundKey(const Block& key) { //Создание раундовых ключей
    array<array<Byte,4>,44> W;


    for (int i = 0; i < 4; ++i)
        for (int j = 0; j < 4; ++j)
            W[i][j] = key[i*4 + j];

    for (int i = 4; i < 44; ++i) {
        array<Byte,4> temp = W[i-1];
        if (i % 4 == 0) {
            // RotWord
            Byte t = temp[0];
            temp[0] = temp[1];
            temp[1] = temp[2];
            temp[2] = temp[3];
            temp[3] = t;
            // SubWord
            for (int k = 0; k < 4; ++k)
                temp[k] = SBOX[temp[k]];
            // Rcon
            temp[0] ^= RCON[i/4];
        }

        for (int k = 0; k < 4; ++k)
            W[i][k] = W[i-4][k] ^ temp[k];
    }

    array<Block,11> roundKeys = {};
    for (int round = 0; round < 11; ++round) {
        // каждые 4 слова = 16 байт
        for (int byteIdx = 0; byteIdx < 16; ++byteIdx) {
            int wordIdx = round*4 + (byteIdx / 4);
            int offset  = byteIdx % 4;
            roundKeys[round][byteIdx] = W[wordIdx][offset];
        }
    }

    return roundKeys;
}

Block EncryptBlock(Block state1, const array<Block,11>& roundKeys) { //Логика AES
    Block s = state1;

    for (int i = 0; i < 16; ++i)
        s[i] ^= roundKeys[0][i];

    for (int round = 1; round <= 9; ++round) {
        for (int i = 0; i < 16; ++i) {s[i] = SBOX[s[i]];}


        Byte tmp;
        tmp      = s[1];  s[1]  = s[5];  s[5]  = s[9];  s[9]  = s[13]; s[13] = tmp;
        tmp      = s[2];  s[2]  = s[10]; s[10] = tmp;
        tmp      = s[6];  s[6]  = s[14]; s[14] = tmp;
        tmp      = s[15]; s[15] = s[11]; s[11] = s[7];  s[7]  = s[3];  s[3]  = tmp;

        // Внутри EncryptBlock, вместо старого mixColumns:
        auto xtime = [&](Byte x){ return Byte((x<<1)^((x&0x80)?0x1B:0)); };
        auto mul3  = [&](Byte x){ return Byte(xtime(x)^x); };
        for(int c = 0; c < 4; ++c){
            int i = 4*c;
            Byte a0 = s[i], a1 = s[i+1], a2 = s[i+2], a3 = s[i+3];
            s[i]   = xtime(a0) ^ mul3(a1) ^ a2        ^ a3;
            s[i+1] = a0        ^ xtime(a1) ^ mul3(a2) ^ a3;
            s[i+2] = a0        ^ a1        ^ xtime(a2) ^ mul3(a3);
            s[i+3] = mul3(a0)  ^ a1        ^ a2        ^ xtime(a3);
        }


        for (int i = 0; i < 16; ++i) {s[i] ^= roundKeys[round][i];}
    }


    for (int i = 0; i < 16; ++i) {s[i] = SBOX[s[i]];}


    Byte tmp;
    tmp      = s[1];  s[1]  = s[5];  s[5]  = s[9];  s[9]  = s[13]; s[13] = tmp;
    tmp      = s[2];  s[2]  = s[10]; s[10] = tmp;
    tmp      = s[6];  s[6]  = s[14]; s[14] = tmp;
    tmp      = s[15]; s[15] = s[11]; s[11] = s[7];  s[7]  = s[3];  s[3]  = tmp;


    for (int i = 0; i < 16; ++i) {s[i] ^= roundKeys[10][i];}

    return s;
}

static Byte hexVal(char c) { // Преобразует hex-символ в число
    if      (c >= '0' && c <= '9') return c - '0';
    else if (c >= 'a' && c <= 'f') return c - 'a' + 10;
    else if (c >= 'A' && c <= 'F') return c - 'A' + 10;
    return 0;
}

vector<Block> cutText(const string& text, int method) {//преобразует входной текст в блоки по 16 байт
    vector<Byte> data(text.begin(), text.end());
    if (method == 1) {
        data.clear();
        for (size_t i = 0; i + 1 < text.size(); i += 2) {
            Byte b = (hexVal(text[i]) << 4) | hexVal(text[i+1]);
            data.push_back(char(b));
        }
    }

    vector<Block> blocks;
    for (size_t off = 0; off < data.size(); off += 16) {
        size_t len = min<size_t>(16, data.size() - off);
        array<Byte,16> block{};
        copy_n(data.begin()+off, len, block.begin());
        blocks.push_back(block);
    }

    return blocks;
}

vector<Byte> encrypt(const array<Byte, 16>& key, const array<Byte, 16>& IV, string& text) { //реализует режим CBC (Cipher Block Chaining)
    vector<Block> textBlocks = cutText(text, 0);
    array<Block,11> roundKeys = getRoundKey(key);

    Block state = IV;
    vector<Byte> zhifrText;

    for (auto& textBlock : textBlocks) {
        Block eBlock = EncryptBlock(state, roundKeys);

        Block zhifrBlock = {};
        size_t L = textBlock.size();
        for (size_t i = 0; i < L; ++i) {
            zhifrBlock[i] = textBlock[i] ^ eBlock[i];
            zhifrText.push_back(zhifrBlock[i]);
        }
        state = zhifrBlock;
    }
    return zhifrText;
}

string decrypt(const array<Byte, 16>& key, const array<Byte, 16>& IV, string& text) { //реализует режим CBC (Cipher Block Chaining)
    vector<Block> textBlocks = cutText(text, 1);
    array<Block,11> roundKeys = getRoundKey(key);

    Block state = IV;
    vector<Byte> zhifrText;

    for (auto& textBlock : textBlocks) {
        Block eBlock = EncryptBlock(state, roundKeys);

        Block zhifrBlock = {};
        size_t L = textBlock.size();
        for (size_t i = 0; i < L; ++i) {
            zhifrBlock[i] = textBlock[i] ^ eBlock[i];
            zhifrText.push_back(zhifrBlock[i]);
        }
        state = textBlock;
    }
    return string(zhifrText.begin(), zhifrText.end());
}




template<typename Container>
string printHex(const Container& text) {// Преобразует данные в hex-строку
    ostringstream os;
    for (auto i : text) {
        os << hex << setw(2) << setfill('0') << nouppercase << static_cast<int>(i);
    }
    return os.str();
}

template<typename Container>
void hexToFile(const string& name, const Container& data) {// Записывает данные в файл
    string filename = name + ".bin";
    ofstream out(filename, ios::binary);

    out.write(reinterpret_cast<const char*>(data.data()), data.size());
    cout << "Успешно записано в " << filename << endl;
}

pair<array<Byte, 16>, array<Byte, 16>> gen_key_iv() { // Генерирует случайные ключ и IV
    array<Byte, 16> key;
    array<Byte, 16> IV;

    random_device rd;

    for (int i = 0; i < 16; ++i) {
        key[i] = static_cast<Byte>(rd());
        IV[i] = static_cast<Byte>(rd());
    }

    return {key, IV};
}

string getText(int method) {// Получает текст от пользователя
    cout << "Текст будет введен\n1. Из файла\n2. В консоли\n    Ввод > ";
    int cmd = 0;
    while (true) {
        cin >> cmd;
        if (cmd == 1 || cmd == 2) {break;}
        else {cout << "Введена неверная команда!" << endl;}
    }
    string text;
    if (cmd == 1) {
        while (true) {
            cout << "Путь к файлу > ";
            string path; cin >> path;
            if (method == 1) {
                ifstream in(path, ios::binary);
                in.seekg(0, ios::end); auto sz = in.tellg(); in.seekg(0);
                vector<Byte> textc(sz);
                in.read(reinterpret_cast<char*>(textc.data()), textc.size());
                text = printHex(textc);
            } else {
                ifstream in(path);
                text = string((istreambuf_iterator<char>(in)), istreambuf_iterator<char>());
            }
            if (text.empty()) {
                cout << "Файл пустой или не существует!" << endl;
            } else {
                break;
            }
        }
    } else if (cmd == 2) {
        while (true) {
            cin.ignore(numeric_limits<streamsize>::max(), '\n');
            cout << "Введите текст (в одну строку!) и нажмите Enter." << endl << "    Ввод > ";
            getline(cin, text);
            if (text.empty()) {
                cout << "Ничего не введено!" << endl;
            } else {
                break;
            }
        }
    }
    return text;
}

pair<Block, Block> getKeyIv() { // Получает ключ и IV от пользователя
    cout << "Ключ и IV будет введен\n1. Из файла (bin)\n2. В консоли (hex)\n    Ввод > ";
    int cmd = 0;
    while (true) {
        cin >> cmd;
        if (cmd == 1 || cmd == 2) {break;}
        else {cout << "Введена неверная команда!" << endl;}
    }
    Block key, IV;
    if (cmd == 1) {
        while (true) {
            cout << "Путь к файлу для ключа > ";
            string path; cin >> path;
            ifstream in(path, ios::binary);
            in.read(reinterpret_cast<char*>(key.data()), key.size());
            if (!in || in.gcount() != key.size()) {
                cout << "Файл пустой или не существует!" << endl;
            } else {
                break;
            }
        }
        while (true) {
            cout << "Путь к файлу для IV > ";
            string path; cin >> path;
            ifstream in(path, ios::binary);
            in.read(reinterpret_cast<char*>(IV.data()), IV.size());
            if (!in || in.gcount() != key.size()) {
                cout << "Файл пустой или не существует!" << endl;
            } else {
                break;
            }
        }
    } else if (cmd == 2) {
        while (true) {
            cout << "Введите ключ и нажмите Enter." << endl << "    Ввод > ";
            string keyhex;
            cin.ignore(numeric_limits<streamsize>::max(), '\n');
            cin >> keyhex;
            if (keyhex.size() != 32) {
                cout << "Неккорректный ввод!" << endl;
                continue;
            } else {
                for (int i = 0; i < 16; ++i) {
                    key[i] = Byte((hexVal(keyhex[2*i]) << 4) |  hexVal(keyhex[2*i + 1]));
                }
                break;
            }
        }
        while (true) {
            cout << "Введите IV и нажмите Enter." << endl << "    Ввод > ";
            string ivhex;
            cin.ignore(numeric_limits<streamsize>::max(), '\n');
            cin >> ivhex;
            if (ivhex.size() != 32) {
                cout << "Неккорректный ввод!" << endl;
                continue;
            } else {
                for (int i = 0; i < 16; ++i) {
                    IV[i] = Byte((hexVal(ivhex[2*i]) << 4) |  hexVal(ivhex[2*i + 1]));
                }
                break;
            }
        }
    }
    return {key, IV};
}

int main() {
    cout << "Шифровальная машина имени НГТУ\nДобро пожаловать!" << endl;
    cout << "Будем\n1. Шифровать\n2. Дешифровать\n    Ввод > ";
    int cmd; cin >> cmd;
    if (cmd == 1) {
        string text = getText(0);
        cout << endl << "Текст для шифрования: " << text << endl << endl;
        auto [key, IV] = gen_key_iv();
        cout << "KEY: " << printHex(key) << endl;
        hexToFile("key", key);
        cout << "IV: " << printHex(IV) << endl;
        hexToFile("IV", IV);

        vector<Byte> eText = encrypt(key, IV, text);
        cout << endl << "Зашифрованный текст:" << endl << endl << printHex(eText) << endl;
        hexToFile("eText", eText);
    } else if (cmd == 2) {
        string text = getText(1);
        cout << endl << "Текст для расшифрования: " << text << endl << endl;
        auto [key, IV] = getKeyIv();
        cout << "KEY: " << printHex(key) << endl;
        cout << "IV: " << printHex(IV) << endl;

        string dText = decrypt(key, IV, text);
        cout << endl << "Расшифрованный текст:" << endl << endl << dText << endl;

    } else {
        cout << "Введена неверная команда!" << endl;
    }
}
