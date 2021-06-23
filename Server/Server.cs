using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Server
{

    public class Server
    {

        private TcpListener listener;
        public List <Client> Clients { get; private set; } = new List <Client> ();     // Список из классов CLIENT.CS. //
        public event Action ClientConnected;

        public Server (int port)
        {
            listener = new TcpListener (IPAddress.Any, port);
        }

        public async Task WaitClientConnection ()
        {
            while (true)
            {
                if (Clients.Count > 0)                                                 // Если количество клиентов больше 0, то выход из метода. //
                {
                    return;
                }
                await Task.Delay (50);                                                 // Небольшая задержка для уменьшения нагрузки на ЦПУ. //
            }
        }

        public async Task Start ()
        {
            listener.Start ();                                                         // Запуск прослушивания порта. //
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync ();                   // Ожидание подлкючения клиента. //
                Clients.Add (new Client (client));
                ClientConnected?.Invoke ();                                            // Старт события добавления клиента. //
            } 
        }
    }
}