using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class Server
{
    static void Main()
    {
        const int port = 50007;
        var listener = new TcpListener(IPAddress.Loopback, port);
        listener.Start();
        Console.WriteLine($"Сервер слушает 127.0.0.1:{port}...");

        using TcpClient client = listener.AcceptTcpClient();
        Console.WriteLine($"Подключен клиент: {client.Client.RemoteEndPoint}");

        using NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];

        while (true)
        {
            int bytesRead = stream.Read(buffer, 0, buffer.Length);
            if (bytesRead == 0) break; // клиент закрыл соединение

            string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
            Console.WriteLine($"Получено: {message}");

            byte[] response = Encoding.UTF8.GetBytes("Echo: " + message);
            stream.Write(response, 0, response.Length);
        }

        listener.Stop();
    }
}