using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace httpServer
{
    class Client
    {
        public Client(TcpClient Client)
        {
            //string html = "<html><body><h1>It works!</h1></body></html>";

            //string Str = "HTTP|1.1 200 OK\nContent-type: text/html\nContent-Lenght:" + html.Length.ToString() + "\n\n" + html;

            //byte[] buffer = Encoding.ASCII.GetBytes(Str);

            //Client.GetStream().Write(buffer, 0, buffer.Length);

            //Client.Close();

            string Request = "";

            byte[] buffer = new byte[1024];

            int count;

            while ((count=Client.GetStream().Read(buffer,0,buffer.Length))>0)
            {
                Request += Encoding.ASCII.GetString(buffer, 0, count);

                if (Request.IndexOf("\r\n\r\n") >= 0 ||Request.Length>4096)
                {
                    break;
                }
            }

            Match ReqMatch = Regex.Match(Request, @"^\w+\s+([^\s\?]+)[^\s]*\s+HTTP/.*|");

            if (ReqMatch == Match.Empty)
            {
                SendError(Client, 400);

                return;
            }

            string RequestURI = ReqMatch.Groups[1].Value;

            RequestURI = Uri.UnescapeDataString(RequestURI);

            if (RequestURI.IndexOf("..")>0)
            {
                SendError(Client, 400);
                return;
            }

            if (RequestURI.EndsWith("/"))
            {
                RequestURI += "index.html";
            }

            string FilePath = "www/" + RequestURI;

            if (!File.Exists(FilePath))
            {
                SendError(Client, 404);
                return;
            }


            string Extension = RequestURI.Substring(RequestURI.LastIndexOf('.'));

            string ContextType = "";

            switch (Extension)
            {
                case ".htm":
                case ".html":
                    ContextType = "text/html";
                    break;
                case ".css":
                    ContextType = "text/styleshet";
                    break;
                case ".js":
                    ContextType = "text/javascript";
                    break;
                case ".jpg":
                    ContextType = "image/jpeg";
                    break;
                case ".jpeg":
                case ".png":
                case ".gif":
                    ContextType = "image/" + Extension.Substring(1);
                    break;
                default:
                    if(Extension.Length>1)
                    {
                        ContextType = "application/" + Extension.Substring(1);
                    }
                    else
                    {
                        ContextType = "application/unknown";

                    }
                    break;

            }

            FileStream FS;
            try
            {
                FS = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            }
            catch
            {
                SendError(Client, 500);
                return;
            }

            string Header = "HTTP/1.1 200 OK\nContent-Type: " + ContextType + "\nContent-Length: " + FS.Length + "\n\n";

            byte[] HeadersBuffer = Encoding.ASCII.GetBytes(Header);

            Client.GetStream().Write(HeadersBuffer, 0, HeadersBuffer.Length);

            while (FS.Position < FS.Length)
            {
                count = FS.Read(buffer, 0, buffer.Length);
                Client.GetStream().Write(buffer, 0, count);

            }

            FS.Close();

            Client.Close();

        }

        private void SendError(TcpClient Client, int Code)
        {
            // Получаем строку вида "200 OK"
            // HttpStatusCode хранит в себе все статус-коды HTTP/1.1
            string CodeStr = Code.ToString() + " " + ((HttpStatusCode)Code).ToString();
            // Код простой HTML-странички
            string Html = "<html><body><h1>" + CodeStr + "</h1></body></html>";
            // Необходимые заголовки: ответ сервера, тип и длина содержимого. После двух пустых строк - само содержимое
            string Str = "HTTP/1.1 " + CodeStr + "\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;
            // Приведем строку к виду массива байт
            byte[] Buffer = Encoding.ASCII.GetBytes(Str);
            // Отправим его клиенту
            Client.GetStream().Write(Buffer, 0, Buffer.Length);
            // Закроем соединение
            Client.Close();
        }
    }
}
