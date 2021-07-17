using AOSharp.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing;
using System.Linq;
using System.Runtime.InteropServices;
using AOSharp.Common.GameData;
using AOSharp.Common.Unmanaged.Imports;
using AOSharp.Core.Inventory;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;

namespace Radar
{
    [Obsolete]
    public class Main : IAOPluginEntry
    {
        private List<string> _trackedNames;
        private Dictionary<Profession, Vector3> ProfessionCollors = new Dictionary<Profession, Vector3>

        {
        { Profession.Doctor , DebuggingColor.Red} ,
        { Profession.Trader , DebuggingColor.LightBlue} ,
        { Profession.Engineer , DebuggingColor.Green} ,
        { Profession.NanoTechnician , DebuggingColor.White} ,
        { Profession.Agent , DebuggingColor.Yellow} ,
        { Profession.MartialArtist , DebuggingColor.Purple} ,
        { Profession.Adventurer , DebuggingColor.White} ,
        { Profession.Enforcer , DebuggingColor.White} ,
        { Profession.Soldier , DebuggingColor.LightBlue} ,
        { Profession.Shade , DebuggingColor.White} ,
        { Profession.Keeper , DebuggingColor.White} ,
        { Profession.Bureaucrat , DebuggingColor.White} ,
        { Profession.Metaphysicist , DebuggingColor.White} ,
        { Profession.Fixer , DebuggingColor.White} ,
        { Profession.Unknown , DebuggingColor.White} ,
        };


        public void Run(string pluginDir)
        {
            try
            {
                Chat.WriteLine("Radar Reloaded loaded!", ChatColor.Orange);
                _trackedNames = new List<string>();
                Chat.RegisterCommand("track", TrackCallback);
                Chat.RegisterCommand("untrack", UntrackCallback);
                Game.OnUpdate += OnUpdate;
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        private void UntrackCallback(string command, string[] args, ChatWindow window)
        {
            if (args.Length > 0)
            {
                string name = string.Join(" ", args);
                if (_trackedNames.Contains(name))
                {
                    _trackedNames.Remove(name);
                    window.WriteLine($"Removed \"{name}\" from tracking list");
                }
                else
                {
                    window.WriteLine($"Not tracking \"{name}\"");
                }
            }
            else
            {
                window.WriteLine("Please specify a name");
            }
        }

        private void TrackCallback(string command, string[] args, ChatWindow window)
        {
            if (args.Length > 0)
            {
                string name = string.Join(" ", args);
                _trackedNames.Add(name);
                window.WriteLine($"Added \"{name}\" to tracking list");
            }
            else
            {
                window.WriteLine("Please specify a name");
            }
        }

        private void OnUpdate(object sender, float e)
        {
            DrawPlayers();
            DrawBots();
            DrawLifts();
            DrawTracked();
        }

        private void DrawTracked()
        {
            foreach (SimpleChar character in DynelManager.Characters)
            {
                if (_trackedNames.Contains(character.Name))
                {
                    Debug.DrawSphere(character.Position, 1, DebuggingColor.Green);
                    Debug.DrawLine(DynelManager.LocalPlayer.Position, character.Position, DebuggingColor.Green);
                }
            }
        }

        private void DrawPlayers()
        {
            int time = (int)Time.NormalTime;

            foreach (SimpleChar player in DynelManager.Players)
            {
                if (player.Identity == DynelManager.LocalPlayer.Identity)
                    continue;

                Vector3 debuggingColor;

                if (Playfield.IsBattleStation)
                {
                    debuggingColor = DynelManager.LocalPlayer.GetStat(Stat.BattlestationSide) != player.GetStat(Stat.BattlestationSide) ? DebuggingColor.Red : DebuggingColor.Green;

                    Debug.DrawSphere(player.Position, 1, debuggingColor);
                    Debug.DrawLine(DynelManager.LocalPlayer.Position, player.Position, debuggingColor);
                }
                else
                {
                    if (player.Buffs.Contains(new[] { 216382, 284620, 202732, 214879 }) && time % 2 == 0 && player.Level > 215) //player.Side == Side.OmniTek && player.Level > 218)
                    {

                        debuggingColor = DebuggingColor.Red;

                        Debug.DrawSphere(player.Position, 1, debuggingColor);
                        Debug.DrawLine(DynelManager.LocalPlayer.Position, player.Position, debuggingColor);
                    }
                    else
                    {

                        Vector3 profs = DebuggingColor.White;
                        Vector3 others = DebuggingColor.White;

                        switch (player.Profession)
                        {

                            case Profession.Doctor:

                                profs = DebuggingColor.Red;

                                break;

                            case Profession.Trader:

                                profs = DebuggingColor.LightBlue;

                                break;

                            case Profession.Engineer:

                                profs = DebuggingColor.Green;
                                break;

                            case Profession.NanoTechnician:

                                profs = DebuggingColor.White;
                                break;

                            case Profession.MartialArtist:

                                profs = DebuggingColor.Purple;
                                break;
                            case Profession.Agent:

                                profs = DebuggingColor.Yellow;
                                break;
                            case Profession.Unknown:
                                others = DebuggingColor.White;

                                break;

                            case Profession.Adventurer:
                                others = DebuggingColor.White;

                                break;

                            case Profession.Shade:
                                others = DebuggingColor.White;

                                break;

                            case Profession.Enforcer:
                                others = DebuggingColor.White;

                                break;

                            case Profession.Fixer:
                                others = DebuggingColor.White;

                                break;

                            case Profession.Bureaucrat:
                                others = DebuggingColor.White;

                                break;

                            case Profession.Keeper:
                                others = DebuggingColor.White;

                                break;
                        }

                        switch (player.Side)
                        {

                            case Side.OmniTek:

                                if (player.Level > 218 && player.Profession == Profession.Fixer
                             || player.Level > 218 && player.Profession == Profession.Shade || player.Level > 218 && player.Profession == Profession.Metaphysicist
                             || player.Level > 218 && player.Profession == Profession.Enforcer || player.Level > 218 && player.Profession == Profession.Soldier
                             || player.Level > 218 && player.Profession == Profession.Bureaucrat || player.Level > 218 && player.Profession == Profession.Adventurer
                             || player.Level > 218 && player.Profession == Profession.Keeper || player.Level > 218 && player.Profession == Profession.Unknown)
                                {
                                    Debug.DrawSphere(player.Position, 1, others);
                                    break;

                                }

                                else if (player.Level > 218 && player.Profession == Profession.Doctor
                                    || player.Level > 218 && player.Profession == Profession.Trader
                                    || player.Level > 218 && player.Profession == Profession.Engineer
                                    || player.Level > 218 && player.Profession == Profession.MartialArtist
                                    || player.Level > 218 && player.Profession == Profession.NanoTechnician
                                    || player.Level > 218 && player.Profession == Profession.Agent)

                                {
                                    Debug.DrawSphere(player.Position, 1, ProfessionCollors[(player.Profession)]);
                                    Debug.DrawLine(DynelManager.LocalPlayer.Position, player.Position, ProfessionCollors[player.Profession]);

                                }
                                break;
                        }
                    }
                }
            }
        }

        private void DrawLifts()
        {
            foreach (Dynel terminal in DynelManager.AllDynels.Where(t => t.Identity.Type == IdentityType.Terminal))
            {
                if (!terminal.Name.Contains("Button"))
                    continue;

                Debug.DrawSphere(terminal.Position, 1, DebuggingColor.White);
                Debug.DrawLine(DynelManager.LocalPlayer.Position, terminal.Position, DebuggingColor.White);
            }
        }

        private void DrawBots()
        {
            foreach (SimpleChar player in DynelManager.Players.Where(p => p.GetStat(Stat.InPlay) == 0))
            {
                Debug.DrawSphere(player.Position, 1, DebuggingColor.Blue);
                Debug.DrawLine(DynelManager.LocalPlayer.Position, player.Position, DebuggingColor.Blue);
            }
        }
    }
}
