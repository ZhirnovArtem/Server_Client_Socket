using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

class EchoServer
{
    private const int Port = 50007;

    static void Main()
    {
        var listener = new TcpListener(IPAddress.Loopback, Port);
        listener.Start();
        Console.WriteLine($"[SERVER] Слушаю 127.0.0.1:{Port} (синхронно, 1 клиент за раз)");

        try
        {
            while (true)
            {
                using TcpClient client = listener.AcceptTcpClient();
                Console.WriteLine($"[SERVER] Подключен: {client.Client.RemoteEndPoint}");

                try
                {
                    HandleClient(client);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[SERVER] Ошибка клиента: {ex.Message}");
                }
                finally
                {
                    Console.WriteLine("[SERVER] Клиент отключен. Жду следующего...");
                }
            }
        }
        finally
        {
            listener.Stop();
            Console.WriteLine("[SERVER] Остановлен.");
        }
    }

    private static void HandleClient(TcpClient client)
    {
        using NetworkStream stream = client.GetStream();

        while (true)
        {
            byte[] lenBuf = new byte[4];
            if (!ReadExact(stream, lenBuf, 4)) return; 
            int length = (lenBuf[0] << 24) | (lenBuf[1] << 16) | (lenBuf[2] << 8) | lenBuf[3];
            if (length <= 0 || length > 1_000_000)
                throw new InvalidOperationException($"Некорректная длина: {length}");

            byte[] body = new byte[length];
            if (!ReadExact(stream, body, length)) return;

            string text = Encoding.UTF8.GetString(body);
            string upper = text.ToUpperInvariant();
            Console.WriteLine($"[SERVER] Получено: \"{text}\" → \"{upper}\"");

            byte[] respBody = Encoding.UTF8.GetBytes(upper);
            byte[] respLen = {
                (byte)(respBody.Length >> 24),
                (byte)(respBody.Length >> 16),
                (byte)(respBody.Length >> 8),
                (byte)(respBody.Length)
            };
            stream.Write(respLen, 0, 4);
            stream.Write(respBody, 0, respBody.Length);
            stream.Flush();
        }
    }
    private static bool ReadExact(NetworkStream stream, byte[] buffer, int count)
    {
        int offset = 0;
        while (offset < count)
        {
            int read = stream.Read(buffer, offset, count - offset);
            if (read == 0) return false; 
            offset += read;
        }
        return true;
    }
}