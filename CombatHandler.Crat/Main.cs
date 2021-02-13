using AOSharp.Core;
using AOSharp.Core.Combat;
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
                Chat.WriteLine("Crat Combat Handler Loaded!", AOSharp.Common.GameData.ChatColor.DarkPink);
                AOSharp.Core.Combat.CombatHandler.Set(new CratCombatHandler());
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
    }
}
