using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Client
{

    class Program
    {

        private static T Promt <T> (string promt,                                          // Функция конвертирует проверяет выходные данные. PROMT - "1. Запуск Server", VALIDATION - PORT => PORT > 0 && PORT < 65535, ERROREMESSAGE - "Ошибка ввода данных". //
            Predicate <T> validation = null,                                               // Поскольку при объявлении инициализируется NULL, то аргумент становится необязательным. //
            string errorMessage = null                                                     // Поскольку при объявлении инициализируется NULL, то аргумент становится необязательным. //
        )
        {
            T result;
            while (true)                                                                   // Если TRUE выполняется, то срабатывает WHILE. Бесконечный цикл. //
            {
                Console.Write (promt);                                                     // Вывод на экран: "введите порт" и "введите IP-адрес сервера". //
                try
                {
                    IConvertible res = Console.ReadLine ();                                // Считывание введенных данных пользователем (номера порта и IP-адрес сервера). //
                    result = (T) res.ToType (typeof (T), null);                            // Конвертация RES в тип Т, и переинициализация RESULT. //
                }
                catch (FormatException e)
                {
                    Console.WriteLine (errorMessage ?? e.Message);                         // Если ошибка, то выводится сообщение об ошибке и WHILE продолжается. //
                    continue;
                }
                if (validation == null || validation (result))                             // Если PORT не задан - NULL, или возвращает TRUE - [0, 65535], то выход из WHILE. //
                {
                    break;
                }
                else
                {
                    Console.Write (errorMessage ?? "Ошибка ввода данных.\n");
                }
            }
            return result;
        }

        static async Task Main (string [] args)
        {
            Console.Write ("2. Запуск Client:\n");
            int portNumber = Promt <int> ("\nВведите номер порта [0, 65535]: ",            // Должен совпадать с портом сервера, иначе соединение не произойдет. //
                port => port > 0 && port < 65535,                                          // Проверка, что PORT E [0, 65535]. //
                "\nОшибка ввода данных. Порта c таким номером не существует.\n"
            );
            var address = Promt <string> ("\nВведите IP-адрес сервера: ");                 // Адрес локального компьютера: 127.0.0.1. //
            TcpClient client = new TcpClient ();                                           // Создание экземпляра клиента. //
            try
            {
                client.Connect (address, portNumber);                                      // Подключение текущего клиента к серверу. //
            }
            catch (Exception)
            {
                Console.WriteLine ("\nНе удалось подключиться к серверу.");
                return;                                                                    // Если подключение не удалось, то программа завершается. //
            }
            var stream = client.GetStream ();                                              // Открытие сетевого потока. //
            client.ReceiveBufferSize = 1024 * 1024;                                        // Определение размера буффера в байтах. В настоящем случае - 1 MB. //
            var buffer = new byte [client.ReceiveBufferSize];                              // Создание буффера. //
            var lengthBuffer = new byte [8];                                               // Дополнительный буффер. //
            try
            {
                while (client.Connected)
                {
                    await stream.ReadAsync (lengthBuffer, 0, lengthBuffer.Length);         // Считывание длины файла в массив байт. //
                    var size = BitConverter.ToInt64 (lengthBuffer, 0);                     // Конвертация массива байт в число типа LONG. //
                    if (Promt <string> ($"\nВы хотите скачать файл размером {(size / 1024.0 / 1024.0).ToString ("F")} MB? (да/нет)\n\nВаш выбор: ").ToLower () == "да")     // Если пользователь подтверждает загрузку файла. //
                    {
                        stream.WriteByte (1);                                              // Подтверждение загрузки файла. //
                        await stream.ReadAsync (lengthBuffer, 0, lengthBuffer.Length);     // Считывание длины файла. //
                        var c = await stream.ReadAsync (buffer, 0, (int) BitConverter.ToInt64 (lengthBuffer, 0));     // Считывание названия файла. //
                        var name = Encoding.Unicode.GetString (buffer, 0, c);              // Конвертация массива байт в строку. //
                        Console.Write ("\nВведите путь для сохранения файла (без слеша в конце): ");     // При двойном левом слеше, произойдет ошибка в пути к файлу. //
                        string pathFile = Console.ReadLine ();
                        DirectoryInfo createFolder = new DirectoryInfo (pathFile);
                        createFolder.Create ();
                        if (!Directory.Exists ($"{pathFile}")) {                           // Если создать папку по указанному пути не удалось (например, из-за отсутствия прав). //
                            throw new SocketException ();
                        }
                        var fs = File.OpenWrite ($"{pathFile}" + @"\" + name);             // Создание файла по указанному пути и с укзанным именем. //
                        long readed = 0;                                                   // Количество считаных байт. //
                        while (readed < size)                                              // Пока количество считаных байт меньше размера файла происходит
                        {
                            var count = await stream.ReadAsync (buffer, 0, buffer.Length);     // Считывание пакета из сетевого потока. //
                            readed += count;
                            await fs.WriteAsync (buffer, 0, count);                        // Запись считанных данных в файл. //
                        }
                        fs.Close ();                                                       // Закрытие файла. //
                        Console.Write ($"\nФайл {name} сохранен.\n");
                    }
                    else
                    {
                        stream.WriteByte (0);                                              // Отказ от загрузки файла. //
                    }
                }
            }
            catch (SocketException)                                                        // Если сервер отключился, то на экран выводится сообщение. //
            {
                Console.Write ("\nСервер прервал соединие.\n");
            }
        }
    }
}