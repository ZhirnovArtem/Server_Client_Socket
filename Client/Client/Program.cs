using System;
using System.Net.Sockets;
using System.Text;

class Client
{
    static void Main()
    {
        const string host = "127.0.0.1";
        const int port = 50007;

        using var client = new TcpClient();
        client.Connect(host, port);
        Console.WriteLine("Подключено к серверу.");

        using NetworkStream stream = client.GetStream();

        string message = "Привет, сервер!";
        byte[] data = Encoding.UTF8.GetBytes(message);
        stream.Write(data, 0, data.Length);

        byte[] buffer = new byte[1024];
        int bytesRead = stream.Read(buffer, 0, buffer.Length);
        Console.WriteLine($"Ответ сервера: {Encoding.UTF8.GetString(buffer, 0, bytesRead)}");
    }
}