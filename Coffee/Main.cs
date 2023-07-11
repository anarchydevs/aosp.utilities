using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Common.GameData;
using AOSharp.Common.GameData.UI;
using AOSharp.Core.GameData;
using AOSharp.Core.UI.Options;
using AOSharp.Core.IPC;
using AOSharp.Common.Unmanaged.Imports;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System.Threading;
using AOSharp.Common.Unmanaged.DataTypes;
using Zoltu.IO;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.ChatMessages;
using System.Runtime.InteropServices;
using System.Drawing;
using AOSharp.Common.SharedEventArgs;
using SmokeLounge.AOtomation.Messaging.GameData;

namespace Coffee
{
    public class Main : AOPluginEntry
    {
        public static bool Toggle = false;

        public static double _timer = 0f;

        public override void Run(string pluginDir)
        {
            try
            {
                Chat.WriteLine("Coffee loaded!");
                Chat.WriteLine("/coffee for toggle.");

                Game.OnUpdate += OnUpdate;

                Chat.RegisterCommand("Coffee", (string command, string[] param, ChatWindow chatWindow) =>
                {
                    Toggle = !Toggle;
                    Chat.WriteLine($"Coffee : {Toggle}");
                });
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        private void OnUpdate(object s, float deltaTime)
        {
            if (Toggle)
            {
                Item coffee = Inventory.Items.Where(c => c.Name == "Miyashiro Superior Enhanced Coffee Machine").FirstOrDefault();

                List<Item> list = Inventory.Items.FindAll((Item x) => x.Name == "Steaming Hot Cup of Enhanced Coffee");

                if (list?.Count > 1)
                {
                    CharacterActionMessage characterActionMessage = new CharacterActionMessage();
                    characterActionMessage.Action = (CharacterActionType)53;
                    characterActionMessage.Target = list[1].Slot;
                    Identity slot = list[0].Slot;
                    characterActionMessage.Parameter1 = (int)slot.Type;
                    slot = list[0].Slot;
                    characterActionMessage.Parameter2 = slot.Instance;
                    Network.Send(characterActionMessage);
                }
                else
                {
                    if (Time.NormalTime > _timer + 2.0 && !DynelManager.LocalPlayer.Cooldowns.ContainsKey(Stat.ElectricalEngineering))
                    {
                        if (coffee != null)
                        {
                            coffee.Use(null, false);
                            _timer = Time.NormalTime;
                        }
                    }
                }
            }
        }

        public override void Teardown()
        {
        }
    }
}
