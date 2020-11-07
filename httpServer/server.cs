using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace httpServer
{
    class Server
    {
        TcpListener Listiner;

        public Server(int port)
        {
            Listiner = new TcpListener(IPAddress.Any, port);
            Listiner.Start();

            while (true)
            {
                //new Client(Listiner.AcceptTcpClient());

                ////variant 1
                //TcpClient client = Listiner.AcceptTcpClient();
                //Thread threadClient = new Thread(new ParameterizedThreadStart(ClientThread));
                //threadClient.Start(client);

                //variant2
                ThreadPool.QueueUserWorkItem(new WaitCallback(ClientThread), Listiner.AcceptTcpClient());

            }
        }

        static void ClientThread(object StateInfo)
        {
            new Client((TcpClient)StateInfo);
        }

        ~Server() //Остановка сервера
        {
            if (Listiner != null)
            {
                Listiner.Stop(); //Остановим его
            }
        }
    }
}
