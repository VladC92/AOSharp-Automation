﻿using AOSharp.Core.IPC;
using AOSharp.Core;
using AOSharp.Core.UI;
using CombatHandler.Generic;
using AOSharp.Common.GameData;
using System;
using System.Collections.Generic;
using DynelManager = AOSharp.Core.DynelManager;
using Vector3 = AOSharp.Common.GameData.Vector3;
using System.Linq;
using System.Threading;




namespace Desu
{
    public class Assist : AOPluginEntry
    {
        public enum LeetCommands
        {
            on,
            off
        }

        private IPCChannel IPCChannel;
        SimpleChar player = null;
        private List<SimpleChar> _playersToHighlight = new List<SimpleChar>();
        private string currentlyAttacking = "";
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
        };
        public override void Run(string pluginDir)
        {
            try
            {
                Chat.WriteLine("PVPAssist Loaded!", ChatColor.LightBlue);


            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        private void PrintAssistCommandUsage(ChatWindow chatWindow)
        {
            string help = "Usage:\n" +
                "/pvpassist name \n" +
                "/p name \n";
            //"/stop ";    

            chatWindow.WriteLine(help, ChatColor.LightBlue);
        }


        public Assist()
        {


            Game.OnUpdate += OnUpdate;
            Game.TeleportEnded += OnZoned;
            Chat.RegisterCommand("pvpassist", PlayerAssist);
            Chat.RegisterCommand("p", PlayerAssist);
            Chat.RegisterCommand("find", FindPlayers);
        


        }




        private void FindPlayers(string command, string[] param, ChatWindow chatWindow)
        {

            try
            {
                _playersToHighlight.Clear();

                if (param.Length < 1)
                {
                    Chat.WriteLine($"You need to specify a name or profession", ChatColor.DarkPink);
                    return;
                }
                string name = param[0].ToLower();

                if (name == DynelManager.LocalPlayer.Name.ToLower())
                {
                    player = null;
                    Chat.WriteLine($"That's yourself N00b!", ChatColor.DarkPink);
                    return;
                }
                bool isProf;
                Profession prof;
                switch (name)
                {
                    case "doc":
                    case "doctor":
                    case "doctors":
                    case "docs":
                        isProf = true;
                        prof = Profession.Doctor;

                        break;
                    case "crats":
                    case "crat":
                    case "bureaucrat":
                    case "bureaucrats":
                        isProf = true;
                        prof = Profession.Bureaucrat;

                        break;
                    case "sol":
                    case "sols":
                    case "soldier":
                    case "soldiers":
                        isProf = true;
                        prof = Profession.Soldier;

                        break;
                    case "trad":
                    case "trads":
                    case "traders":
                    case "trader":
                        isProf = true;
                        prof = Profession.Trader;

                        break;
                    case "agent":
                    case "agents":
                        isProf = true;
                        prof = Profession.Agent;

                        break;
                    case "nt":
                    case "nts":
                        isProf = true;
                        prof = Profession.NanoTechnician;
                        break;
                    case "mp":
                    case "mps":
                        isProf = true;
                        prof = Profession.Metaphysicist;
                        break;
                    case "engi":
                    case "engis":
                    case "engs":
                    case "eng":
                    case "engineers":
                    case "engineer":
                        isProf = true;
                        prof = Profession.Engineer;

                        break;
                    case "adv":
                    case "advi":
                    case "advis":
                        isProf = true;
                        prof = Profession.Adventurer;

                        break;
                    case "enf":
                    case "enfs":
                    case "enfos":
                    case "enforcer":
                    case "enforcers":
                        isProf = true;
                        prof = Profession.Enforcer;
                        break;
                    case "fix":
                    case "fixers":
                    case "fixer":
                        isProf = true;
                        prof = Profession.Fixer;
                        break;
                    case "keep":
                    case "keeper":
                    case "keepers":
                        isProf = true;
                        prof = Profession.Keeper;
                        break;
                    case "shade":
                    case "shades":
                        isProf = true;
                        prof = Profession.Shade;
                        break;
                    default:
                        isProf = false;
                        prof = Profession.Unknown;
                        break;


                }
                if (prof != Profession.Unknown)
                    Chat.WriteLine($"Search for {prof}", ChatColor.DarkPink);
                foreach (SimpleChar p in DynelManager.Players)
                {
                    if (isProf == true)
                    {
                        if (p.Profession == prof && p.Side != Side.Clan)
                            Chat.WriteLine($"Found : " + p.Name, ChatColor.Gold);
                        _playersToHighlight.Add(p);

                    }
                    else
                    {

                        if (p.Name.ToLower().Contains(name))
                        {
                            _playersToHighlight.Add(p);
                            Chat.WriteLine("");
                            Chat.WriteLine($"Name: {p.Name}", ChatColor.DarkPink);
                            Chat.WriteLine($"Profession: {p.Profession}", ChatColor.DarkPink);
                            Chat.WriteLine($"Side: {p.Side}", ChatColor.DarkPink);
                            Chat.WriteLine($"Level: {p.Level}", ChatColor.DarkPink);
                            Chat.WriteLine($"Health: {p.Health}", ChatColor.DarkPink);
                            Chat.WriteLine($"Nano: {p.Nano}", ChatColor.DarkPink);
                            // This doesn't work well , so will comment it for now
                            //  Chat.WriteLine($"Nano Resist: {p.GetStat(Stat.NanoResist)}" , ChatColor.Green);
                            // Chat.WriteLine($"Mater Crea: {p.GetStat(Stat.MaterialCreation)}", ChatColor.Green);
                            Chat.WriteLine("");
                            return;
                        }
                    }
                }
                if (_playersToHighlight.Count == 0)
                    Chat.WriteLine($"Player {param[0]} not found", ChatColor.Yellow);

            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        private void AssistAttack(SimpleChar player)
        {

            try
            {

                if (player == null)
                    return;

                if (!player.IsAttacking && DynelManager.LocalPlayer.FightingTarget != null)
                    DynelManager.LocalPlayer.StopAttack();

                if (player.FightingTarget == null && currentlyAttacking != "")
                {

                    Chat.WriteLine($"Player is not in fight", ChatColor.Yellow);
                    currentlyAttacking = "";
                    return;
                }

                //  if (player.FightingTarget != null && player.FightingTarget.IsPet)
                // {
                //   Chat.WriteLine($"NOOB CALLER detected! {player.Name} is targeting a pet, choose another target.", ChatColor.Red);

                //   currentlyAttacking = "";
                //    return;
                // }


                if (player.FightingTarget != null && currentlyAttacking != player.FightingTarget.Name || player.FightingTarget != null && player.FightingTarget.Health > 50000 && currentlyAttacking != player.FightingTarget.Name)
                {
                    DynelManager.LocalPlayer.Attack(player.FightingTarget, true);

                    currentlyAttacking = player.FightingTarget.Name;

                    foreach (SpecialAttack attack in DynelManager.LocalPlayer.SpecialAttacks)
                    {
                        if (attack.IsAvailable())
                        {
                            SpecialAttack.AimedShot.UseOn(player.FightingTarget.Identity);
                            SpecialAttack.SneakAttack.UseOn(player.FightingTarget.Identity);


                        }

                    }

                    //if (!player.FightingTarget.IsPlayer)
                    //{
                    //    Chat.WriteLine($"Fighting target is not a player", ChatColor.DarkPink);
                    //    return;
                    //}

                }



            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }


        private void PlayerAssist(string command, string[] param, ChatWindow chatWindow)
        {

            try
            {

                if (param.Length < 1)
                {
                    PrintAssistCommandUsage(chatWindow);
                    player = null;
                    return;
                }
                string name = param[0].ToLower();

                if (name == DynelManager.LocalPlayer.Name.ToLower())
                {
                    player = null;
                    Chat.WriteLine($"You can't assist yourself N00b!", ChatColor.DarkPink);
                    return;
                }

                // Loop through all the players in the playfield to find the one that we want to assist

                foreach (SimpleChar p in DynelManager.Players)
                {


                    if (name == p.Name.ToLower())
                    {

                        // Save the player
                        player = p;

                        Chat.WriteLine($"Assisting  {player.Name}  , " +
                        $"\n Breed :{player.Breed} ,\n" +
                        $" Health : {player.Health}  \n" +
                        $" Profession : {player.Profession} ", ChatColor.Yellow);

                        AssistAttack(player);
                        if (player.FightingTarget == null || !player.IsAttacking)
                        {
                            {
                                Chat.WriteLine($"Player is not in fight ", ChatColor.Red);
                                return;
                            }
                        }
                        if (player.FightingTarget != null && player.FightingTarget.IsPet)
                        {
                            Chat.WriteLine($"NOOB CALLER detected! {player.Name} is targeting a pet, choose another target.", ChatColor.Red);

                            //   currentlyAttacking = "";
                            return;
                        }
                        else
                        {
                            DynelManager.LocalPlayer.Attack(player.FightingTarget, true);

                            foreach (SpecialAttack attack in DynelManager.LocalPlayer.SpecialAttacks)
                            {
                                if (attack.IsAvailable())
                                {
                                    SpecialAttack.AimedShot.UseOn(player.FightingTarget.Identity);
                                    SpecialAttack.SneakAttack.UseOn(player.FightingTarget.Identity);



                                }

                            }

                        }

                        Chat.WriteLine($"{player.Name} is targeting " + player.FightingTarget.Name + "\n" +
                      $" Breed {player.FightingTarget.Breed} \n" +
                        $" Health {player.FightingTarget.MaxHealth}", ChatColor.Green);

                    }

                }
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }


        private void DrawPlayer(SimpleChar player)
        {
            try
            {
                if (player != null)
                {

                    Debug.DrawSphere(player.Position, 1, DebuggingColor.Red);
                    Debug.DrawLine(DynelManager.LocalPlayer.Position, player.Position, DebuggingColor.Red);


                    if (player.FightingTarget != null)
                    {
                        Debug.DrawSphere(player.FightingTarget.Position, 1, DebuggingColor.Green);
                        Debug.DrawLine(DynelManager.LocalPlayer.Position, player.FightingTarget.Position, DebuggingColor.Green);

                    }

                }


            }

            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        private void DrawFoundPlayers()
        {
            try
            {
                foreach (SimpleChar p in _playersToHighlight)
                {
                    bool found = false;
                    if (found)
                    {
                        DrawPlayer(p);
                    }
                    else
                    {
                        return;
                    }
                }

            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
        private void OnZoned(object s, EventArgs e)
        {
            player = null;
            _playersToHighlight.Clear();
            currentlyAttacking = "";
        }
        private void PvpKeyProfs()
        {

            foreach (SimpleChar player in DynelManager.Players)
            {
                bool isKeyProf = false;

                switch (player.Profession)
                {

                    case Profession.Doctor:
                        isKeyProf = true;
                        break;

                    case Profession.Trader:

                        isKeyProf = true;
                        break;

                    case Profession.Engineer:
                        isKeyProf = true;
                        break;

                    case Profession.NanoTechnician:

                        isKeyProf = true;
                        break;
                    case Profession.MartialArtist:

                        isKeyProf = true;
                        break;
                    case Profession.Agent:

                        isKeyProf = true;
                        break;
                    case 0:

                        break;

                }
                if (isKeyProf && player.Side == Side.OmniTek && player.Level > 218 || isKeyProf && player.Side == Side.OmniTek && player.Level == 150 || isKeyProf && player.Side == Side.OmniTek && player.Level == 158 || isKeyProf && player.Side == Side.OmniTek && player.Level == 170 || isKeyProf && player.Side == Side.OmniTek && player.Level == 118)
                {
                    Debug.DrawSphere(player.Position, 1, ProfessionCollors[player.Profession]);
                    Debug.DrawLine(DynelManager.LocalPlayer.Position, player.Position, ProfessionCollors[player.Profession]);


                }

            }

        }


        private void OnUpdate(object sender, float e)
        {
            PvpKeyProfs();
            DrawFoundPlayers();


            if (player == null)
                return;

            bool found = false;
            foreach (SimpleChar p in DynelManager.Players)
            {
                if (p.Name == player.Name)
                {
                    found = true;

                }
            }
            if (found)
            {
                DrawPlayer(player);

                AssistAttack(player);

            }
            else
            {
                player = null;
                return;
            }

        }

    }
}