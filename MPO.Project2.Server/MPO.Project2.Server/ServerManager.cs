using System.Net;
using System.Net.Sockets;
using System.Text;

namespace MPO.Project2.Server;

public class ServerManager
{
    static TcpListener? Server;
    List<ClientManager> clients = new List<ClientManager>();

    protected internal void AddConnection(ClientManager clientObject) 
        => clients.Add(clientObject);

    protected internal void RemoveConnection(string id)
    {
        ClientManager? client = clients.FirstOrDefault(c => c.Id == id);

        if (client != null)
            clients.Remove(client);
    }

    protected internal void Listen()
    {
        try
        {
            Server = new TcpListener(IPAddress.Any, 8888);
            Server.Start();
            Console.WriteLine("Server Chat started");

            while (true)
            {
                TcpClient client = Server.AcceptTcpClient();

                ClientManager clientObject = new ClientManager(client, this);
                Thread clientThread = new Thread(new ThreadStart(clientObject.Process));
                clientThread.Start();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            Disconnect();
        }
    }


    protected internal void BroadcastMessageToAll(string message, string id)
    {
        byte[] data = Encoding.Unicode.GetBytes(message);
        for (int i = 0; i < clients.Count; i++)
            //Don't send message to current Client
            if (clients[i].Id != id)
                clients[i].Stream.Write(data, 0, data.Length);
    }

    protected internal void BroadcastMessageToUser(string message, string id, string userGetId)
    {
        byte[] data = Encoding.Unicode.GetBytes(message);
        for (int i = 0; i < clients.Count; i++)
            if (clients[i].Id == userGetId)
                //Don't send message to current Client
                if (clients[i].Id != id)
                    clients[i].Stream.Write(data, 0, data.Length);
    }

    protected internal void Disconnect()
    {
        if (Server is not null)
            Server.Stop();

        for (int i = 0; i < clients.Count; i++)
            clients[i].Close();

        Environment.Exit(0);
    }
}