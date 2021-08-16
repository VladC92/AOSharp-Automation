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
                Chat.WriteLine("PVPAssist Loaded!");
                AOSharp.Core.Combat.CombatHandler.Set(new Assist());
                
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

    }
}
