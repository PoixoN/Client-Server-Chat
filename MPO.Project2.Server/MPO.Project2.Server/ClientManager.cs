using System.Net.Sockets;
using System.Text;

namespace MPO.Project2.Server;

public class ClientManager
{
    protected internal string Id { get; private set; }
    protected internal NetworkStream Stream { get; private set; }

    protected internal string userName;
    protected internal TcpClient client;
    protected internal ServerManager server;

    public ClientManager(TcpClient tcpClient, ServerManager serverObject)
    {
        Id = Guid.NewGuid().ToString().Substring(0, 4);
        client = tcpClient;
        server = serverObject;
        serverObject.AddConnection(this);
    }

    public void Process()
    {
        try
        {
            Stream = client.GetStream();
            string message = GetMessage();
            userName = message;

            message =  $"> {Id} {userName} is online";

            server.BroadcastMessageToAll(message, Id);
            Console.WriteLine(message);

            while (true)
            {
                try
                {
                    message = GetMessage();
                    if (message.StartsWith("/d "))
                    {
                        string[] result = ProcessDirectMessage(message);
                        result[1] = String.Format($"> {Id} {userName}: {result[1]}");
                        Console.WriteLine(result[1]);
                        server.BroadcastMessageToUser(result[1], Id, result[0]);
                    }
                    else
                    {
                        message = String.Format($"> {Id} {userName}: {message}");
                        Console.WriteLine(message);
                        server.BroadcastMessageToAll(message, Id);
                    }

                }
                catch
                {
                    message = String.Format($"> {Id} {userName} is offline");
                    Console.WriteLine(message);
                    server.BroadcastMessageToAll(message, Id);
                    break;
                }
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
        }
        finally
        {

            server.RemoveConnection(Id);
            Close();
        }
    }


    private string GetMessage()
    {
        byte[] data = new byte[64];
        StringBuilder builder = new StringBuilder();
        int bytes = 0;
        do
        {
            bytes = Stream.Read(data, 0, data.Length);
            builder.Append(Encoding.Unicode.GetString(data, 0, bytes));
        }
        while (Stream.DataAvailable);

        return builder.ToString();
    }

    private string[] ProcessDirectMessage(string message)
    {
        message = message.Trim().Substring(3).Trim();
        string[] parts = message.Split(" ");

        message = "/d " + string.Join(" ", parts.Skip(1).ToArray());
        string[] result = { parts[0], message };

        return result;
    }


    protected internal void Close()
    {
        if (Stream != null)
            Stream.Close();
        if (client != null)
            client.Close();
    }
}
