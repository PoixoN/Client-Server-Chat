namespace MPO.Project2.Server;

class Program
{
    static ServerManager? serverManager;
    static Thread? serverListenThread;

    static void Main()
    {
        try
        {
            serverManager = new ServerManager();
            serverListenThread = new Thread(new ThreadStart(serverManager.Listen));
            serverListenThread.Start();
        }
        catch (Exception ex)
        {
            if (serverManager is not null)
                serverManager.Disconnect();
            Console.WriteLine(ex.Message);
        }
    }
}
