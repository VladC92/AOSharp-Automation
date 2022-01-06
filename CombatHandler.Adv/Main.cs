using AOSharp.Core;
using System;
using AOSharp.Core.UI;

namespace Desu
{
    public class Main : AOPluginEntry
    {
        public override void Run(string pluginDir)
        {
            try
            {
                Chat.WriteLine("Adventurer Combat Handler Loaded!" , AOSharp.Common.GameData.ChatColor.LightBlue);
                AOSharp.Core.Combat.CombatHandler.Set(new AdvCombathandler());
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
    }
}
