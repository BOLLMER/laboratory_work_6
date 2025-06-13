using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

class AesCbcEncryption
{
    private static readonly byte[] SBOX = {
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

    private static readonly byte[] RCON = {
        0x00,
        0x01, 0x02, 0x04, 0x08,
        0x10, 0x20, 0x40, 0x80,
        0x1B, 0x36
    };

    private static byte[][] GetRoundKey(byte[] key)
    {
        byte[][] W = new byte[44][];
        for (int i = 0; i < 44; i++) W[i] = new byte[4];

        for (int i = 0; i < 4; ++i)
            for (int j = 0; j < 4; ++j)
                W[i][j] = key[i * 4 + j];

        for (int i = 4; i < 44; ++i)
        {
            byte[] temp = (byte[])W[i - 1].Clone();
            if (i % 4 == 0)
            {
                // RotWord
                byte t = temp[0];
                temp[0] = temp[1];
                temp[1] = temp[2];
                temp[2] = temp[3];
                temp[3] = t;

                // SubWord
                for (int k = 0; k < 4; ++k)
                    temp[k] = SBOX[temp[k]];

                // Rcon
                temp[0] ^= RCON[i / 4];
            }

            for (int k = 0; k < 4; ++k)
                W[i][k] = (byte)(W[i - 4][k] ^ temp[k]);
        }

        byte[][] roundKeys = new byte[11][];
        for (int i = 0; i < 11; i++) roundKeys[i] = new byte[16];

        for (int round = 0; round < 11; ++round)
        {
            for (int byteIdx = 0; byteIdx < 16; ++byteIdx)
            {
                int wordIdx = round * 4 + (byteIdx / 4);
                int offset = byteIdx % 4;
                roundKeys[round][byteIdx] = W[wordIdx][offset];
            }
        }

        return roundKeys;
    }

    private static byte[] EncryptBlock(byte[] state, byte[][] roundKeys)
    {
        byte[] s = (byte[])state.Clone();
        byte tmp;
        for (int i = 0; i < 16; ++i)
            s[i] ^= roundKeys[0][i];

        for (int round = 1; round <= 9; ++round)
        {
            for (int i = 0; i < 16; ++i) { s[i] = SBOX[s[i]]; }

            // ShiftRows
            tmp = s[1]; s[1] = s[5]; s[5] = s[9]; s[9] = s[13]; s[13] = tmp;
            tmp = s[2]; s[2] = s[10]; s[10] = tmp;
            tmp = s[6]; s[6] = s[14]; s[14] = tmp;
            tmp = s[15]; s[15] = s[11]; s[11] = s[7]; s[7] = s[3]; s[3] = tmp;

            // MixColumns
            byte xtime(byte x) => (byte)((x << 1) ^ ((x & 0x80) != 0 ? 0x1B : 0));
            byte mul3(byte x) => (byte)(xtime(x) ^ x);

            for (int c = 0; c < 4; ++c)
            {
                int i = 4 * c;
                byte a0 = s[i], a1 = s[i + 1], a2 = s[i + 2], a3 = s[i + 3];
                s[i] = (byte)(xtime(a0) ^ mul3(a1) ^ a2 ^ a3);
                s[i + 1] = (byte)(a0 ^ xtime(a1) ^ mul3(a2) ^ a3);
                s[i + 2] = (byte)(a0 ^ a1 ^ xtime(a2) ^ mul3(a3));
                s[i + 3] = (byte)(mul3(a0) ^ a1 ^ a2 ^ xtime(a3));
            }

            for (int i = 0; i < 16; ++i) { s[i] ^= roundKeys[round][i]; }
        }

        // Final round
        for (int i = 0; i < 16; ++i) { s[i] = SBOX[s[i]]; }

        // ShiftRows
        tmp = s[1]; s[1] = s[5]; s[5] = s[9]; s[9] = s[13]; s[13] = tmp;
        tmp = s[2]; s[2] = s[10]; s[10] = tmp;
        tmp = s[6]; s[6] = s[14]; s[14] = tmp;
        tmp = s[15]; s[15] = s[11]; s[11] = s[7]; s[7] = s[3]; s[3] = tmp;

        for (int i = 0; i < 16; ++i) { s[i] ^= roundKeys[10][i]; }

        return s;
    }

    private static byte HexVal(char c)
    {
        if (c >= '0' && c <= '9') return (byte)(c - '0');
        else if (c >= 'a' && c <= 'f') return (byte)(c - 'a' + 10);
        else if (c >= 'A' && c <= 'F') return (byte)(c - 'A' + 10);
        return 0;
    }

    private static List<byte[]> CutText(string text, int method)
    {
        List<byte> data = new List<byte>();

        if (method == 1) // Hex to bytes
        {
            for (int i = 0; i + 1 < text.Length; i += 2)
            {
                byte b = (byte)((HexVal(text[i]) << 4) | HexVal(text[i + 1]));
                data.Add(b);
            }
        }
        else // String to bytes
        {
            data.AddRange(Encoding.UTF8.GetBytes(text));
        }

        List<byte[]> blocks = new List<byte[]>();
        for (int off = 0; off < data.Count; off += 16)
        {
            int len = Math.Min(16, data.Count - off);
            byte[] block = new byte[16];
            Array.Copy(data.ToArray(), off, block, 0, len);
            blocks.Add(block);
        }

        return blocks;
    }

    private static byte[] Encrypt(byte[] key, byte[] IV, string text)
    {
        List<byte[]> textBlocks = CutText(text, 0);
        byte[][] roundKeys = GetRoundKey(key);

        byte[] state = (byte[])IV.Clone();
        List<byte> encryptedText = new List<byte>();

        foreach (var textBlock in textBlocks)
        {
            byte[] eBlock = EncryptBlock(state, roundKeys);

            byte[] encryptedBlock = new byte[16];
            int L = textBlock.Length;
            for (int i = 0; i < L; ++i)
            {
                encryptedBlock[i] = (byte)(textBlock[i] ^ eBlock[i]);
                encryptedText.Add(encryptedBlock[i]);
            }
            state = encryptedBlock;
        }

        return encryptedText.ToArray();
    }

    private static string Decrypt(byte[] key, byte[] IV, string text)
    {
        List<byte[]> textBlocks = CutText(text, 1);
        byte[][] roundKeys = GetRoundKey(key);

        byte[] state = (byte[])IV.Clone();
        List<byte> decryptedText = new List<byte>();

        foreach (var textBlock in textBlocks)
        {
            byte[] eBlock = EncryptBlock(state, roundKeys);

            byte[] decryptedBlock = new byte[16];
            int L = textBlock.Length;
            for (int i = 0; i < L; ++i)
            {
                decryptedBlock[i] = (byte)(textBlock[i] ^ eBlock[i]);
                decryptedText.Add(decryptedBlock[i]);
            }
            state = textBlock;
        }

        return Encoding.UTF8.GetString(decryptedText.ToArray());
    }

    private static string PrintHex(byte[] data)
    {
        return BitConverter.ToString(data).Replace("-", "").ToLower();
    }

    private static void HexToFile(string name, byte[] data)
    {
        string filename = name + ".bin";
        File.WriteAllBytes(filename, data);
        Console.WriteLine("Успешно записано в " + filename);
    }

    private static (byte[] key, byte[] IV) GenKeyIv()
    {
        byte[] key = new byte[16];
        byte[] IV = new byte[16];

        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(key);
            rng.GetBytes(IV);
        }

        return (key, IV);
    }

    private static string GetText(int method)
    {
        Console.WriteLine("Текст будет введен\n1. Из файла\n2. В консоли\n    Ввод > ");
        int cmd = 0;
        while (true)
        {
            if (!int.TryParse(Console.ReadLine(), out cmd)) cmd = 0;
            if (cmd == 1 || cmd == 2) break;
            else Console.WriteLine("Введена неверная команда!");
        }

        string text = "";
        if (cmd == 1)
        {
            while (true)
            {
                Console.WriteLine("Путь к файлу > ");
                string path = Console.ReadLine();
                try
                {
                    if (method == 1)
                    {
                        byte[] fileBytes = File.ReadAllBytes(path);
                        text = PrintHex(fileBytes);
                    }
                    else
                    {
                        text = File.ReadAllText(path);
                    }

                    if (string.IsNullOrEmpty(text))
                    {
                        Console.WriteLine("Файл пустой или не существует!");
                    }
                    else
                    {
                        break;
                    }
                }
                catch
                {
                    Console.WriteLine("Файл пустой или не существует!");
                }
            }
        }
        else if (cmd == 2)
        {
            while (true)
            {
                Console.WriteLine("Введите текст (в одну строку!) и нажмите Enter.");
                Console.WriteLine("    Ввод > ");
                text = Console.ReadLine();
                if (string.IsNullOrEmpty(text))
                {
                    Console.WriteLine("Ничего не введено!");
                }
                else
                {
                    break;
                }
            }
        }

        return text;
    }

    private static (byte[] key, byte[] IV) GetKeyIv()
    {
        Console.WriteLine("Ключ и IV будет введен\n1. Из файла (bin)\n2. В консоли (hex)\n    Ввод > ");
        int cmd = 0;
        while (true)
        {
            if (!int.TryParse(Console.ReadLine(), out cmd)) cmd = 0;
            if (cmd == 1 || cmd == 2) break;
            else Console.WriteLine("Введена неверная команда!");
        }

        byte[] key = new byte[16];
        byte[] IV = new byte[16];

        if (cmd == 1)
        {
            while (true)
            {
                Console.WriteLine("Путь к файлу для ключа > ");
                string path = Console.ReadLine();
                try
                {
                    byte[] fileBytes = File.ReadAllBytes(path);
                    if (fileBytes.Length != 16)
                    {
                        Console.WriteLine("Неверный размер файла (должно быть 16 байт)!");
                        continue;
                    }
                    Array.Copy(fileBytes, key, 16);
                    break;
                }
                catch
                {
                    Console.WriteLine("Файл пустой или не существует!");
                }
            }

            while (true)
            {
                Console.WriteLine("Путь к файлу для IV > ");
                string path = Console.ReadLine();
                try
                {
                    byte[] fileBytes = File.ReadAllBytes(path);
                    if (fileBytes.Length != 16)
                    {
                        Console.WriteLine("Неверный размер файла (должно быть 16 байт)!");
                        continue;
                    }
                    Array.Copy(fileBytes, IV, 16);
                    break;
                }
                catch
                {
                    Console.WriteLine("Файл пустой или не существует!");
                }
            }
        }
        else if (cmd == 2)
        {
            while (true)
            {
                Console.WriteLine("Введите ключ (32 hex символа) и нажмите Enter.");
                Console.WriteLine("    Ввод > ");
                string keyhex = Console.ReadLine();
                if (keyhex.Length != 32)
                {
                    Console.WriteLine("Некорректный ввод!");
                    continue;
                }

                for (int i = 0; i < 16; ++i)
                {
                    key[i] = (byte)((HexVal(keyhex[2 * i]) << 4 | HexVal(keyhex[2 * i + 1])));
                }
                break;
            }

            while (true)
            {
                Console.WriteLine("Введите IV (32 hex символа) и нажмите Enter.");
                Console.WriteLine("    Ввод > ");
                string ivhex = Console.ReadLine();
                if (ivhex.Length != 32)
                {
                    Console.WriteLine("Некорректный ввод!");
                    continue;
                }

                for (int i = 0; i < 16; ++i)
                {
                    IV[i] = (byte)((HexVal(ivhex[2 * i]) << 4 | HexVal(ivhex[2 * i + 1])));
                }
                break;
            }
        }

        return (key, IV);
    }

    static void Main(string[] args)
    {
        Console.WriteLine("Шифровальная машина имени НГТУ\nДобро пожаловать!");
        Console.WriteLine("Будем\n1. Шифровать\n2. Дешифровать\n    Ввод > ");

        if (!int.TryParse(Console.ReadLine(), out int cmd)) cmd = 0;

        if (cmd == 1)
        {
            string text = GetText(0);
            Console.WriteLine("\nТекст для шифрования: " + text + "\n");
            var (key, IV) = GenKeyIv();
            Console.WriteLine("KEY: " + PrintHex(key));
            HexToFile("key", key);
            Console.WriteLine("IV: " + PrintHex(IV));
            HexToFile("IV", IV);

            byte[] eText = Encrypt(key, IV, text);
            Console.WriteLine("\nЗашифрованный текст:\n\n" + PrintHex(eText));
            HexToFile("eText", eText);
        }
        else if (cmd == 2)
        {
            string text = GetText(1);
            Console.WriteLine("\nТекст для расшифрования: " + text + "\n");
            var (key, IV) = GetKeyIv();
            Console.WriteLine("KEY: " + PrintHex(key));
            Console.WriteLine("IV: " + PrintHex(IV));

            string dText = Decrypt(key, IV, text);
            Console.WriteLine("\nРасшифрованный текст:\n\n" + dText);
        }
        else
        {
            Console.WriteLine("Введена неверная команда!");
        }
    }
}