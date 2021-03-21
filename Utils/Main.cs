using System;
using System.Collections.Generic;
using System.Linq;
using AOSharp.Core;
using AOSharp.Core.UI;
using AOSharp.Core.Inventory;
using AOSharp.Common.GameData;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using SmokeLounge.AOtomation.Messaging.Messages;
using System.Runtime.InteropServices;
using System.Diagnostics;
using SmokeLounge.AOtomation.Messaging.GameData;
using System.Threading.Tasks;

namespace AOUtils
{
    public class Main : AOPluginEntry
    {
        private static int[] MaxSlots = new int[21];
        

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        public override void Run(string pluginDir)
        {
            Chat.WriteLine("AO Duping tool Loaded", ChatColor.Green);
            Network.N3MessageSent += Network_N3MessageSent;

        }

        private bool IsActiveWindow => GetForegroundWindow() == Process.GetCurrentProcess().MainWindowHandle;

        private bool IsActiveCharacter()
        {
            return IsActiveWindow;
        }

        private void Network_N3MessageSent(object sender, N3Message n3Msg)
        {
            if (!IsActiveCharacter())
                return;

            if (n3Msg.Identity != DynelManager.LocalPlayer.Identity)
                return;


            if (n3Msg.N3MessageType == N3MessageType.CharacterAction)
            {
                CharacterActionMessage charMsg = (CharacterActionMessage)n3Msg;

                if (charMsg.Action == CharacterActionType.UseItemOnItem)
                {
                    var source = new Identity(IdentityType.Inventory, charMsg.Parameter2);
                    if (Inventory.Find(source, out Item targetItem) && Inventory.Find(charMsg.Target, out Item sourceItem))
                    {
                        Identity bank = new Identity();
                        bank.Type = IdentityType.BankByRef;

                        var sourceBackpack = Inventory.Backpacks.FirstOrDefault(x => x.Slot == sourceItem.Slot);
                        var targetBackpack = Inventory.Backpacks.FirstOrDefault(x => x.Slot == targetItem.Slot);
                        if (sourceBackpack != null && targetBackpack != null)
                        {
                            MoveItems(sourceBackpack, targetBackpack);
                          

                        }


                    }

                }

            }

        }

        private void MoveItems(Container source, Container target)
        {


            if (source.Items.Count > 0 && (MaxSlots.Length - target.Items.Count() > 0))
            {

              
                Task.Delay(100).ContinueWith(x =>
                {
                    source.Items.First().MoveToContainer(target);
                    MoveItems(source, target);




                });
            }
               else
              {
                Chat.WriteLine("Added all Items", ChatColor.Green);
             }
        }


    }
}
