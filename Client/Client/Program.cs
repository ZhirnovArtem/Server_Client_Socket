using System;
using System.Net.Sockets;
using System.Text;

class Program
{
    static void Main()
    {
        using Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        socket.Connect("127.0.0.1", 50007);

        string message = "hello World";
        Console.WriteLine($"[CLIENT] Отправляем строку: \"{message}\"");

        byte[] body = Encoding.UTF8.GetBytes(message);
        byte[] packet = new byte[4 + body.Length];
        packet[0] = (byte)(body.Length >> 24);
        packet[1] = (byte)(body.Length >> 16);
        packet[2] = (byte)(body.Length >> 8);
        packet[3] = (byte)body.Length;
        Array.Copy(body, 0, packet, 4, body.Length);

        Console.WriteLine($"[CLIENT] Байты ОТПРАВЛЕННЫЕ (включая 4 байта длины): {BitConverter.ToString(packet)}");
        socket.Send(packet);

        byte[] lenBuf = new byte[4];
        ReadExact(socket, lenBuf, 4);
        int respLen = (lenBuf[0] << 24) | (lenBuf[1] << 16) | (lenBuf[2] << 8) | lenBuf[3];

        byte[] respBody = new byte[respLen];
        ReadExact(socket, respBody, respLen);

        Console.WriteLine($"[CLIENT] Байты ПОЛУЧЕННЫЕ (только тело): {BitConverter.ToString(respBody)}");

        string response = Encoding.UTF8.GetString(respBody);
        Console.WriteLine($"[CLIENT] Декодированная строка: \"{response}\"");
    }

    static void ReadExact(Socket socket, byte[] buffer, int count)
    {
        int offset = 0;
        while (offset < count)
        {
            int read = socket.Receive(buffer, offset, count - offset, SocketFlags.None);
            if (read == 0) throw new Exception("Сервер закрыл соединение.");
            offset += read;
        }
    }
}