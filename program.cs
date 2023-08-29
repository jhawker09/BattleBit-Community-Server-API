using System.Net;
using BattleBitAPI;
using BattleBitAPI.Common;
using BattleBitAPI.Server;

namespace DaGameroom;

internal static class Program

{
    private static void Main(string[] args)
    {
        var listener = new ServerListener<MyPlayer, MyGameServer>();
        // Add callbacks to the listener. This listener does not really contain any in-game event callbacks
        // All of the callbacks are specific to the API events, such as game servers trying to connect etc.
        // We can override functions for in-game events in the MyPlayer and MyGameServer classes.
        listener.OnCreatingGameServerInstance = OnCreatingGameServerInstance;
        listener.OnCreatingPlayerInstance = OnCreatingPlayerInstance;
        listener.OnValidateGameServerToken = OnValidateGameServerToken;
        listener.OnGameServerConnected = OnGameServerConnected;
        listener.OnGameServerDisconnected = OnGameServerDisconnected;
        listener.Start(29294);

        // The listener is running, now we just want this thread to sleep so that the program does not exit.
        // Alternatively, you could make the listener a hosted service, which allows for more flexibility including
        // dependency injection. See https://github.com/Julgers/Database-connection-example/blob/main/Program.cs for an
        // example on how to do that.
        Thread.Sleep(Timeout.Infinite);
    }

    private static async Task OnGameServerDisconnected(GameServer<MyPlayer> arg)
    {
        await Console.Out.WriteLineAsync($"Gameserver {arg.GameIP}:{arg.GamePort} disconnected");
    }

    private static async Task OnGameServerConnected(GameServer<MyPlayer> arg)
    {
        await Console.Out.WriteLineAsync($"Gameserver {arg.GameIP}:{arg.GamePort} connected");
    }

    // With this function, you can require any game server that wants to connect to your API to send an API token.
    // For this, you would need to add "-apiToken=Your_super_secret_token" to the startup parameters of the game server(s)
    private static async Task<bool> OnValidateGameServerToken(IPAddress ip, ushort port, string token)
    {
        await Console.Out.WriteLineAsync($"{ip}:{port} sent {token}");
        return token == "feb57a4e-820b-4855-b587-0499a185130c";
    }

    // Use these functions if you want to pass things to the constructors of your MyPlayer & MyGameServer.
    // This could be anything, including something like a service provider. Or just simple stuff like the URL of your
    // Discord webhook... All up to you ¯\_(ツ)_/¯

    private static MyPlayer OnCreatingPlayerInstance(ulong steamId)
    {
        return new MyPlayer();
    }

    private static MyGameServer OnCreatingGameServerInstance(IPAddress ip, ushort port)
    {
        return new MyGameServer();
    }
}

// We can now start with MyPlayer and MyGameserver. We can give them private properties/fields and then
// add overrides for the callbacks for in-game events, and also requests from the gameserver that we can deny/accept/change.
internal class MyPlayer : Player<MyPlayer>
{

}

internal class MyGameServer : GameServer<MyPlayer>
{
   public override async Task OnConnected()
   {
        ServerSettings.CanVoteNight = false;
        mResources._RoundSettings.PlayersToStart = 2
    }
}