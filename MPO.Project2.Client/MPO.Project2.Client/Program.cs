using System.Data.SqlTypes;
using System.Drawing;
using System.Net.Sockets;
using System.Text;

namespace ClientChat;

class Program
{
    static string userName;
    private const string host = "127.0.0.1";
    private const int port = 8888;
    static TcpClient client;
    static NetworkStream stream;

    static void Main(string[] args)
    {
        Console.Write("Enter name: ");
        userName = Console.ReadLine();
        client = new TcpClient();
        try
        {
            client.Connect(host, port); //connect client
            stream = client.GetStream();

            string message = userName;
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);

            // new thread for each client
            Thread receiveThread = new Thread(new ThreadStart(ReceiveMessage));
            receiveThread.Start();
            Console.WriteLine($"Hey {userName}, you were connected successfully");
            Console.WriteLine("Just send message in chat");
            Console.WriteLine($"===============================================");
            SendMessage();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
        finally
        {
            Disconnect();
        }
    }

    static void SendMessage()
    {
        while (true)
        {
            string message = Console.ReadLine();
            byte[] data = Encoding.Unicode.GetBytes(message);
            stream.Write(data, 0, data.Length);
        }
    }

    static void ReceiveMessage()
    {
        while (true)
        {
            try
            {
                byte[] data = new byte[64]; // buffer
                StringBuilder builder = new StringBuilder();
                int bytes = 0;
                do
                {
                    bytes = stream.Read(data, 0, data.Length);
                    builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
                }
                while (stream.DataAvailable);

                string message = builder.ToString();
                ConsoleColor? recievedMessageColor = null;

                if (message.Contains("/d "))
                {
                    message = message.Replace("/d ", "");

                    if (message.StartsWith("> "))
                    {
                        message = message.Replace("> ", "");
                        var clientId = message.Substring(0, 3);
                        recievedMessageColor = GetColorByClientId(clientId);
                    }

                    Console.ForegroundColor = GetBackgroundColorByForegroundColor(recievedMessageColor ?? ConsoleColor.DarkBlue);
                    Console.BackgroundColor = recievedMessageColor ?? ConsoleColor.DarkBlue;
                    Console.Write("Direct to You >>>>>> ");
                    Console.ResetColor();
                }

                if (recievedMessageColor is null && message.StartsWith("> "))
                {
                    message = message.Replace("> ", "");
                    var clientId = message.Substring(0, 3);
                    recievedMessageColor = GetColorByClientId(clientId);
                }

                Console.ForegroundColor = recievedMessageColor ?? ConsoleColor.DarkBlue;
                Console.WriteLine(message);
                Console.ResetColor();
            }
            catch
            {
                Console.WriteLine("You was disconnected!");
                Console.ReadLine();
                Disconnect();
            }
        }
    }

    static void Disconnect()
    {
        if (stream != null)
            stream.Close();
        if (client != null)
            client.Close();
        Environment.Exit(0);
    }

    private static ConsoleColor GetColorByClientId(string id)
        => ConsoleColorFromColor(Color.FromArgb(id.GetHashCode()));

    private static ConsoleColor GetBackgroundColorByForegroundColor(ConsoleColor foregroundColor)
        => (int)foregroundColor < 10 ? ConsoleColor.White : ConsoleColor.Black;

    public static ConsoleColor ConsoleColorFromColor(System.Drawing.Color c)
    {
        int index = (c.R > 128 | c.G > 128 | c.B > 128) ? 8 : 0; // Bright bit
        index |= (c.R > 64) ? 4 : 0; // Red bit
        index |= (c.G > 64) ? 2 : 0; // Green bit
        index |= (c.B > 64) ? 1 : 0; // Blue bit

        return (ConsoleColor)(index == 15 ? index - 1 : index);
    }
}