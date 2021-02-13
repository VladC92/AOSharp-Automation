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
                Chat.WriteLine("Keeper Combat Handler V1.1 Loaded!", AOSharp.Common.GameData.ChatColor.DarkPink);
                AOSharp.Core.Combat.CombatHandler.Set(new KeeperCombatHandler());
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
    }
}
