using TShockAPI;
using Terraria;
using TMain = Terraria.Main;
using TerrariaApi.Server;
using Terraria.Localization;
using System.Linq;

namespace DoubleCharacterKick
{
    [ApiVersion(2, 1)]
    public class Main : TerrariaPlugin
    {
        private bool _enabled = true;

        public Main(TMain game) : base(game)
        {

        }

        public override void Initialize()
        {
            ServerApi.Hooks.ServerJoin.Register(this, OnJoin);

            Commands.ChatCommands.Add(new Command("doublecharacterkick", Toggle, "toggledoublecharacterkick"));
        }

        private void OnJoin(JoinEventArgs args)
        {
            if (_enabled)
            {
                var client = Netplay.Clients[args.Who];
                string addr = client.Socket.GetRemoteAddress().ToString();
                string ip = addr.Substring(0, addr.IndexOf(':'));  // GetRemoteAddress() includes the port which needs to be split

                var alreadyOnline = TSPlayer.FindByNameOrID(client.Name);

                if (alreadyOnline.Count > 0)
                {
                    // FindByNameOrID() doesnt always give exact matches which is what we are actually looking for
                    var tsplr = alreadyOnline.FirstOrDefault(p => p.Name == client.Name);
                    if (tsplr != null)
                    {
                        // scenario 1: a player with the same name and same ip kicks the player already on the server
                        // as players sometimes roam on the server after leaving
                        if (tsplr.IP == ip)
                        {
                            tsplr.Disconnect("");
                        }
                        // scenario 2: a player with the same name and different ip is kicked
                        else
                        {
                            client.PendingTermination = true;
                            NetMessage.SendData((int)PacketTypes.Disconnect, client.Id, -1, NetworkText.FromLiteral("A player with the same name is already on the server."));
                        }
                    }
                }
                // else they are guaranteed to be free to join
            }
        }

        private void Toggle(CommandArgs args)
        {
            _enabled = !_enabled;
            args.Player.SendSuccessMessage($"{(_enabled ? "En" : "Dis")}abled double character kick.");
        }
    }
}
