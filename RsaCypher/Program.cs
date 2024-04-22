using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Text;

namespace RsaCypher
{
    internal class Program
    {
        private const string SPLITER = "|NEW_LINE|";

        static void Main(string[] args)
        {
            var encryptedBuilder = new StringBuilder();
            var decryptedBuilder = new StringBuilder();

            string message = "Поток (Stream) – это мощный концепт в программировании, который предоставляет удобный способ работы с последовательными данными, будь то чтение информации из файла, запись в сетевое соединение или обработка данных в памяти. Это абстракция, которая позволяет программистам эффективно манипулировать данными разного типа и происхождения без необходимости загрузки их целиком в память." +
                $"{SPLITER}" +
                "Основная идея потока заключается в том, что данные обрабатываются или передаются постепенно, порциями, что позволяет экономить ресурсы памяти и обрабатывать большие объемы информации без перегрузки оперативной памяти. Вместо того, чтобы загружать весь файл или данные из сети целиком в память, поток позволяет читать или записывать информацию по мере ее доступности." +
                $"{SPLITER}" +
                "Потоки можно рассматривать как каналы связи между источником данных (например, файлом, сетевым соединением или памятью) и программой, которая их обрабатывает. Они предоставляют унифицированный способ взаимодействия с данными независимо от их происхождения, что делает их незаменимым инструментом в различных сценариях программирования." +
                $"{SPLITER}" +
                "Каждый тип потока предоставляет свой набор методов и свойства для работы с данными. Например, FileStream в C# предоставляет функциональность для чтения и записи данных в файлы, а NetworkStream используется для обмена данными по сети. Несмотря на различия в способах использования, основные принципы работы с потоками остаются общими." +
                $"{SPLITER}" +
                "Важно отметить, что потоки обеспечивают не только удобство, но и эффективность при работе с данными. За счет возможности читать или записывать данные порциями, они позволяют обрабатывать информацию в реальном времени и экономить ресурсы как оперативной памяти, так и процессора." +
                $"{SPLITER}" +
                "Таким образом, потоки являются ключевым инструментом в арсенале разработчика, обеспечивая эффективное управление данными в различных сценариях программирования и способствуя созданию более эффективных и масштабируемых приложений.";

            // Создаем ключи (Публичный и Приватный) фиксированной длины в 1024 бита
            var keys = GenerateKeys();

            // Сообщение разбивается на блоки по 32 байта, для блочного шифрования.
            // Каждый блок шифруется, кодируется в BASE64 и добавляется в билдер
            foreach (string originalChunk in SplitString(message, 32))
                encryptedBuilder.Append(Convert.ToBase64String(EncryptString(originalChunk, keys.Public)));

            // Зашифрованное сообщение разбивается на блоки, где разделителем будет знак '='
            // Знак '=' всегда будет последним знаком при кодировке в BASE64
            var blocks = Regex.Matches(encryptedBuilder.ToString(), @".*?=");

            // Консольный вывод каждого блока
            Console.ForegroundColor = ConsoleColor.DarkRed;
            for (int i = 0; i < blocks.Count; i++)
                Console.WriteLine($"number block {i}:\n{blocks[i].Value}");

            Console.WriteLine("\n\n");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Enter any key to watch decrypted message");
            Console.ReadKey();
            Console.WriteLine("\n\n");

            // Проходим по каждому блоку, декодируем из BASE64, расшифровываем и записываем в билдер
            for (int i = 0; i < blocks.Count; i++)
                decryptedBuilder.Append(DecryptString(Convert.FromBase64String(blocks[i].Value), keys.Private));

            // Расшифрованный текст сплитим по константному сплит знаку
            Console.ForegroundColor = ConsoleColor.Green;
            var text = decryptedBuilder.ToString().Replace(SPLITER, "\n\n");

            // Дополнительно разбиваем текст на строки фиксированной длины
            string[] lines = SplitText(text, 100);
            foreach (string line in lines)
                Console.WriteLine(line);

            Console.ResetColor();
        }

        public static string[] SplitText(string text, int maxLength)
        {
            string[] words = text.Split(' ');

            var lines = new List<string>();

            string currentLine = "";

            foreach (string word in words)
            {
                if ((currentLine + word).Length > maxLength)
                {
                    lines.Add(currentLine);
                    currentLine = word + " ";
                }
                else
                    currentLine += word + " ";
            }

            if (!string.IsNullOrEmpty(currentLine))
                lines.Add(currentLine);

            return lines.ToArray();
        }

        public static IEnumerable<string> SplitString(string input, int chunkSize)
        {
            for (int i = 0; i < input.Length; i += chunkSize)
            {
                int length = Math.Min(chunkSize, input.Length - i);
                yield return input.Substring(i, length);
            }
        }

        public static Keys GenerateKeys()
        {
            using RSACryptoServiceProvider rsa = new();
            rsa.KeySize = 1024;

            string privateKeyXml = rsa.ToXmlString(true);
            string publicKeyXml = rsa.ToXmlString(false);

            return new Keys { Private = privateKeyXml, Public = publicKeyXml };
        }

        static byte[] EncryptString(string text, string publicKey)
        {
            using RSACryptoServiceProvider rsa = new();
            rsa.FromXmlString(publicKey);

            byte[] textBytes = Encoding.UTF8.GetBytes(text);
            return rsa.Encrypt(textBytes, true);
        }

        static string DecryptString(byte[] encryptedBytes, string privateKey)
        {
            using RSACryptoServiceProvider rsa = new();
            rsa.FromXmlString(privateKey);

            byte[] decryptedBytes = rsa.Decrypt(encryptedBytes, true);
            return Encoding.UTF8.GetString(decryptedBytes);
        }
    }

    public class Keys
    {
        public string Private { get; set; }
        public string Public { get; set; }
    }
}
