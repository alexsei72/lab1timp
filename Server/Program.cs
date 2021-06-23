/*
 *Лабораторная: 1.
 *Источник: https://hwmw.blogspot.com/p/labstimp2.html
 *
 *Язык: C Sharp (C#) v7.3.
 *Среда: Microsoft Visual Studio 2019 v16.9.1.
 *Платформа: .NET Framework v4.6.1.
 *API: console.
 *Изменение: 09.03.2021.
 *Защита: 16.03.2021.
 *
 *Вариант: БН.
 *Задание: Разработка TCP клиента и сервера. Разработайте клиентское и серверные приложения на WIN32 и .NET языках позволяющие
 *    принимать и передавать файлы по TCP.
 *
 *Примечание:
 *1. Тип "Т" - универсальный параметр.
 *2. "=>" - лямбда-оператор используется для отделения входных параметров с левой стороны от тела лямбда-выражения с правой
 *    стороны.
 *3. OBJECT - корневой класс, который содержит в себе все остальные классы.
 *4. PREDICATE - возвращает TRUE, если объект OBJ удовлетворяет критериям, заданным в методе, который представляет этот делегат;
 *    в противном случае - FALSE.
 *5. Выражение PORT => PORT > 0 && PORT < 65535 и его аргумент - PREDICATE <T> VALIDATION = NULL, следует трактовать следующим
 *    образом:
 *    При переходе в функцию PROMT <T>, компилятором считываются условия: PORT > 0 && PORT < 65535. Затем в строке
 *    ICONVERTIBLE RES = CONSOLE.READLINE (), считывается введенный пользователем номер PORT. После этого, RES инициализируется
 *    в RESULT (T становится INT). И в строке VALIDATION (RESULT) происходит сравнение исходного условия -
 *    PORT > 0 && PORT < 65535 с тем, что получилось в RESULT. Если RESULT > 0 && RESULT < 65535, то TRUE, иначе FALSE
 *    (во-втором случае последует ELSE и "Ошибка ввода данных").
 *6. AWAIT - ожидание работы текущей асинхронной (ASYNC) функции, до конца выполнения текущего оператора.
 *7. Для передачи, бери файл размером не менее 10 MB, лучше даже более 100 MB (например, какой-нибудь PDF-файл).
 *8. "C:\\MVS\\TCP\\Client\\bin\\Debug\\Client.exe" и @"C:\MVS\TCP\Client\bin\Debug\Client.exe" - одно и тоже.
 *9. "@" - передача по ссылке.
 */

using System;
using System.IO;
using System.Diagnostics;                                                                       // Требуется для PROCESS. //
using System.Threading.Tasks;

namespace Server
{

    class Program
    {

        private static T Promt <T> (string promt,                                               // Функция конвертирует проверяет выходные данные. PROMT - "1. Запуск Server", VALIDATION - PORT => PORT > 0 && PORT < 65535, ERROREMESSAGE - "Ошибка ввода данных". //
            Predicate <T> validation = null,                                                    // Поскольку при объявлении инициализируется NULL, то аргумент становится необязательным. //
            string errorMessage = null                                                          // Поскольку при объявлении инициализируется NULL, то аргумент становится необязательным. //
        )
        {
            T result;
            while (true)                                                                        // Если TRUE выполняется, то срабатывает WHILE. Бесконечный цикл. //
            {
                Console.Write (promt);                                                          // Вывод на экран: "введите порт". //
                try
                {
                    IConvertible res = Console.ReadLine ();                                     // Считывание введенных данных пользователем (номера порта и IP-адрес сервера). //
                    result = (T) res.ToType (typeof (T), null);                                 // Конвертация RES в тип Т, и переинициализация RESULT. //
                }
                catch (FormatException e)
                {                  
                    Console.WriteLine (errorMessage ?? e.Message);                              // Если ошибка, то выводится сообщение об ошибке и WHILE продолжается. //
                    continue;
                }
                if (validation == null || validation (result))                                  // Если PORT не задан - NULL, или возвращает TRUE - [0, 65535], то выход из WHILE. //
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
            Console.Write ("1. Запуск Server:\n");
            int portNumber = Promt <int> ("\nВведите номер порта [0, 65535]: ",                 // Должен совпадать с портом клиента, иначе соединение не произойдет. //
                port => port > 0 && port < 65535,                                               // Проверка, что PORT E [0, 65535]. //
                "\nОшибка ввода данных. Порта c таким номером не существует.\n"
            );
            Server server = new Server (portNumber);                                            // Создание экземпляра сервера на введенном пользователе номере порта (класс - Server.cs). //
            var _ = Task.Factory.StartNew (server.Start, TaskCreationOptions.LongRunning);      // Старт прослушивания порта в отдельном потоке. //
            server.ClientConnected += () =>                                                     // Инициализация события добавления клиента. //
            {
                Console.Clear ();
                for (int i = 0; i < server.Clients.Count; i++)                                  // Вывод на экран каждого найденного клиента. //
                {
                    var client = server.Clients [i];
                    Console.Write ($"{i + 1}. Номер клиента: {i}. IP-адрес клиента: {client.EndPoint}\n");     // Вывод на экран номера клиента и его конечной точки (IP-адреса клиента). //
                }
            };
            Console.Write ("\nНачало прослушивания порта. Ожидание подключения клиента (-ов).\n");
            Process myProcess = new Process ();
            myProcess.StartInfo.FileName = @"C:\Users\agole\Desktop\тимп лабы 1-7\1 - TCP\Client\bin\Debug\Client.exe";     // Старт клиента в виде дочернего процесса. В противном случае, требуется запускать пользователю. //
            myProcess.Start ();                                                                 // Создание дочернего процесса. //
            await server.WaitClientConnection ();                                               // Ожидание первого клиента. //
            int clientIndex = Promt <int> ("\nВведите номер клиента: ",                         // Аналогично PORTNUMBER, выше. //
                index => index < server.Clients.Count,                                          // Проверка, что номер клиента не выходит за границы списка клиентов. //
                "\nОшибка ввода данных. Клиента с таким номером не существует.\n"
            );
            string pathToFile = Promt <string> ("\nУкажите путь к файлу (с расширением): ",     // Аналогично PORTNUMBER, выше. //
                file => File.Exists (file),                                                     // Проверка, что файл существует. //
                "\nОшибка ввода данных. Файл не найден.\n"
            );
            await server.Clients [clientIndex].SendFile (pathToFile);                           // Отправка указанного файла клиенту и ожидание завершения отправки. //
            myProcess.Kill ();                                                                  // Закрытие дочернего процесса в конце работы программы. //                                                   // Закрытие дочернего процесса в конце работы программы. //
        }
    }
}