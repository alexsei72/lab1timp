using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Server
{

    public class Client
    {
        
        private TcpClient client;
        public IPEndPoint EndPoint { get; private set; }                                     // Хранит IP-адреса клиента. //
        private byte [] fileBuffer;                                                          // Буфер для отправки файла. //
        
        public Client (TcpClient client)
        {
            this.client = client;
            networkStream = client.GetStream ();                                             // Получение сетевого потока. //
            client.SendBufferSize = 1024 * 1024;                                             // Определение размера буффера в байтах. В настоящем случае - 1 MB. //
            fileBuffer = new byte [client.SendBufferSize];                                   // Создание буффера. //
            EndPoint = client.Client.RemoteEndPoint as IPEndPoint;                           // Инициализация конечной точки (IP-адреса клиента). //
        }

        private NetworkStream networkStream;
         
        public async Task SendFile (string file)
        {
            if (!client.Connected)                                                           // Если клиент не подлючен, то выход из метода. //
            {
                Console.Write ("\nКлиент не подключен.\n");
                return;
            }
            using (var stream = File.OpenRead (file))                                        // Открытие файла на чтение. //
            {
                var lengthBuffer = BitConverter.GetBytes (stream.Length);                    // Определение длины файла как массива байт. //
                await networkStream.WriteAsync (lengthBuffer, 0, lengthBuffer.Length);       // Передача длины файла в сетевой поток. //
                if (networkStream.ReadByte () == 1)                                          // Если клиент принимает файл. //
                {
                    var name = Path.GetFileName (file);
                    var bytes = BitConverter.GetBytes ((long) name.Length * 2);              // Определение длины строки названия файла как массива байт. //
                    await networkStream.WriteAsync (bytes, 0, bytes.Length);                 // Передается длина строки в сетевой поток. //
                    var stringBytes = Encoding.Unicode.GetBytes (name);                      // Определение названия файла как массива байт. //
                    await networkStream.WriteAsync (stringBytes, 0, stringBytes.Length);     // Пишем строку в сетевой поток
                    Console.Write ("\nКлиент подтвердил готовность получения файла.\n");
                    while (stream.Position < stream.Length)                                  // Пока не достигнут конец файла. //
                    {
                        int readedAmount = await stream.ReadAsync (fileBuffer, 0, fileBuffer.Length);     // Чтение части файла в файловый буфер. //
                        await networkStream.WriteAsync (fileBuffer, 0, readedAmount);        // Передается считаная часть файла в сетевой поток. //
                    }
                    Console.Write ("\nФайл успешно отправлен.\n");
                    Console.ReadKey ();                                                      // Пауза перед MYPROCESS.KILL (). //
                }
                else
                {
                    Console.Write ("\nКлиент не потвердил отправку файла.\n");
                }
            }
        }
    }
}