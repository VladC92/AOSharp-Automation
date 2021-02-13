using System;
using System.Diagnostics;
using AOSharp.Core;
using AOSharp.Core.IPC;
using AOSharp.Core.Movement;
using AOSharp.Core.UI;
using AOSharp.Core.UI.Options;
using AOSharp.Core.Combat;
using AOSharp.Common;
using AOSharp.Common.GameData;
using SmokeLounge.AOtomation.Messaging.Messages;
using MultiboxHelper.IPCMessages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using SmokeLounge.AOtomation.Messaging.GameData;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using AOSharp.Core.Inventory;

namespace MultiboxHelper
{
    public class MultiboxHelper : AOPluginEntry
    {
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

     
        private IPCChannel IPCChannel;
        private bool syncMoves = true;
        private bool syncAttacks = true;
        private bool syncUse = true;
        private int floor;
        private bool monitorPOHBuffs = false;
        private int altarTickCounter = 0;
        bool needGreenLight = false;

        // Keeper commands
        private bool castAntifear = false;

        MovementController movementController = new MovementController(true);
        private Vector3 floor2_support_pos = new Vector3(312.0, 1.5, 335.5);
        private Vector3 floor2_tank_pos = new Vector3(319.0, 1.5, 366.5);
        private Vector3 red_pedestal_12man = new Vector3(35.6, 29.3, 30.0);  
        private Vector3 yellow_pedestal_12man = new Vector3(164.2, 29.3, 30.4);
        

        public enum EngiDebufAuras
        {
            none,
            NSD,
            blinds
        }
        public enum KeeperCommands
        {
            antifear,
            stop
        }
        public enum DocCommands
        {
            CH
        }

        private bool IsActiveWindow => GetForegroundWindow() == Process.GetCurrentProcess().MainWindowHandle;

        public override void Run(string pluginDir)
        {
            IPCChannel = new IPCChannel(111);
            IPCChannel.RegisterCallback((int)IPCOpcode.Move, OnMoveMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.Target, OnTargetMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.Attack, OnAttackMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.StopAttack, OnStopAttackMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.Use, OnUseMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.SyncMoves, OnSyncMovesMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.SyncAttacks, OnSyncAttacksMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.Engi, OnEngiMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.Keeper, OnKeeperMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.Doc, OnDocMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.Floor, OnFloorMessage);
            IPCChannel.RegisterCallback((int)IPCOpcode.Test, OnTestMessage);


            //_menu = new Menu("MultiboxHelper", "MultiboxHelper");
            //_menu.AddItem(new MenuBool("SyncMove", "Sync Movement", true));
            //_menu.AddItem(new MenuBool("SyncAttack", "Sync Attacks", true));
            //_menu.AddItem(new MenuBool("SyncUse", "Sync Use", true));
            //OptionPanel.AddMenu(_menu);

            Network.N3MessageSent += Network_N3MessageSent;

            Chat.RegisterCommand("syncmoves", SyncMovesCommand);
            Chat.RegisterCommand("sm", SyncMovesCommand);
            Chat.RegisterCommand("syncattacks", SyncAttacksCommand);
            Chat.RegisterCommand("sa", SyncAttacksCommand);
            Chat.RegisterCommand("engi", EngiCommand);
            Chat.RegisterCommand("antifear", AntiFearCommand);
            Chat.RegisterCommand("doc", DocCommand);
            Chat.RegisterCommand("poh", FloorCommand);
            Chat.RegisterCommand("floor", FloorCommand);
            Chat.RegisterCommand("test", TestCommand);

            Game.OnUpdate += OnUpdate;

            Chat.WriteLine("Multibox Helper Loaded! V1.4", AOSharp.Common.GameData.ChatColor.DarkPink);
        }

        private void TestCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                Vector3 pos = new Vector3(639.9, 36.4, 1555.8);

                if (param.Length == 4 )
                {
                    pos = new Vector3(float.Parse(param[1]), float.Parse(param[2]), float.Parse(param[3]));
                }
                IPCChannel.Broadcast(new TestMessage()
                {
                    PlayfieldId = Playfield.Identity.Instance,
                    Position = pos
                });
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
        
        private void AntiFearCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                int keeperCommand = (int)KeeperCommands.antifear;

                if (param.Length > 0)
                    keeperCommand = (int)KeeperCommands.stop;

                IPCChannel.Broadcast(new KeeperMessage()
                {
                    command = keeperCommand
                });
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
        private void FloorCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {

                if (param.Length < 1)
                {
                    Chat.WriteLine("Usage: /floor <floor>");
                    Chat.WriteLine("Example: /floor 3");
                    return;
                }
                floor = int.Parse(param[0]);

                if (floor != 5)
                {
                    syncMoves = false;
                    syncAttacks = false;
                }
                else
                {
                    syncMoves = false;
                    syncAttacks = true;
                }
                FloorMessage fm = new FloorMessage()
                {
                    Floor = floor
                };

                IPCChannel.Broadcast(fm);


                switch (floor)
                {
                    case 2:
                    case 5:
                        OnFloorMessage(0, fm);

                        break;

                    default:
                        break;
                }
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }

        }

        private void SyncMovesCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {
                string commandParam;
                if (param.Length < 1)
                    commandParam = "true";
                else
                    commandParam = param[0].ToLower();

                switch (commandParam)
                {
                    case "true":
                    case "start":
                    case "on":
                        Chat.WriteLine("Start syncing moves...");
                        syncMoves = true;
                        break;
                    case "false":
                    case "stop":
                    case "off":
                        Chat.WriteLine("Stop syncing moves...");
                        syncMoves = false;
                        break;
                }
                IPCChannel.Broadcast(new SyncMovesMessage()
                {
                    syncMoves = syncMoves
                });
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        private void SyncAttacksCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {

                string commandParam;
                if (param.Length < 1)
                    commandParam = "true";
                else
                    commandParam = param[0].ToLower();

                switch (commandParam)
                {
                    case "true":
                    case "start":
                    case "on":
                        Chat.WriteLine("Start syncing attacks...");
                        syncAttacks = true;
                        break;
                    case "false":
                    case "stop":
                    case "off":
                        Chat.WriteLine("Stop syncing attacks...");
                        syncAttacks = false;
                        break;

                }
                IPCChannel.Broadcast(new SyncAttacksMessage()
                {
                    syncAttacks = syncAttacks
                });
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
        private void DocCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {

                string commandParam;
                if (param.Length < 1)
                    commandParam = "ch";
                else
                    commandParam = param[0].ToLower();

                DocCommands docCommands;
                switch (commandParam)
                {
                    default:
                    case "ch":
                        Chat.WriteLine("Doc will cast Alpha Omega...");
                        docCommands = DocCommands.CH;
                        break;
                }
                IPCChannel.Broadcast(new DocMessage()
                {
                    buff = (int)docCommands
                }) ; 
                 
            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }
        private void EngiCommand(string command, string[] param, ChatWindow chatWindow)
        {
            try
            {

                string commandParam;
                if (param.Length < 1)
                    commandParam = "none";
                else
                    commandParam = param[0].ToLower();
                EngiDebufAuras engiAura;
                switch (commandParam)
                {
                    case "nsd":
                        Chat.WriteLine("Engi will cast NSD...");
                        engiAura = EngiDebufAuras.NSD;
                        break;
                    case "blinds":
                        Chat.WriteLine("Engi will cast blinds...");
                        engiAura = EngiDebufAuras.blinds;
                        break;
                    default:
                    case "none":

                        Chat.WriteLine("Engi will remove blinds and NSD...");
                        engiAura = EngiDebufAuras.none;
                        break;

                }
                IPCChannel.Broadcast(new EngiMessage()
                {
                    buff = (int)engiAura
                });

            }
            catch (Exception e)
            {
                Chat.WriteLine(e.Message);
            }
        }

        private void Network_N3MessageSent(object s, N3Message n3Msg)
        {
            if (n3Msg.N3MessageType == N3MessageType.Attack)
            {
                //If this is an attack only the team leader will issue the command
                //if (!Team.IsInTeam || !Team.IsLeader)
                if (!Team.IsInTeam || !IsActiveWindow)
                    return;

            }
            else
            {
                //Only the active window will issue commands
                if (!Team.IsInTeam || !IsActiveWindow)
                    return;
            }


            if (n3Msg.Identity != DynelManager.LocalPlayer.Identity)
                return;

            if (n3Msg.N3MessageType == N3MessageType.CharDCMove)
            {
                if (!syncMoves)
                    return;

                CharDCMoveMessage charDCMoveMsg = (CharDCMoveMessage)n3Msg;
                IPCChannel.Broadcast(new MoveMessage()
                {
                    MoveType = charDCMoveMsg.MoveType,
                    PlayfieldId = Playfield.Identity.Instance,
                    Position = charDCMoveMsg.Position,
                    Rotation = charDCMoveMsg.Heading
                });

            }
            else if (n3Msg.N3MessageType == N3MessageType.CharacterAction)
            {
                if (!syncMoves)
                    return;

                CharacterActionMessage charActionMsg = (CharacterActionMessage)n3Msg;

                if (charActionMsg.Action != CharacterActionType.StandUp)
                    return;

                IPCChannel.Broadcast(new MoveMessage()
                {
                    MoveType = MovementAction.LeaveSit,
                    PlayfieldId = Playfield.Identity.Instance,
                    Position = DynelManager.LocalPlayer.Position,
                    Rotation = DynelManager.LocalPlayer.Rotation
                });
            }
            else if (n3Msg.N3MessageType == N3MessageType.LookAt)
            {
                LookAtMessage lookAtMsg = (LookAtMessage)n3Msg;
                IPCChannel.Broadcast(new TargetMessage()
                {
                    Target = lookAtMsg.Target
                });
            }
            else if (n3Msg.N3MessageType == N3MessageType.Attack)
            {
                if (!syncAttacks)
                    return;

                AttackMessage attackMsg = (AttackMessage)n3Msg;
                IPCChannel.Broadcast(new AttackIPCMessage()
                {
                    Target = attackMsg.Target,
                    PlayfieldId = Playfield.Identity.Instance
                });
            }
            else if (n3Msg.N3MessageType == N3MessageType.StopFight)
            {
                if (!syncAttacks)
                    return;

                StopFightMessage lookAtMsg = (StopFightMessage)n3Msg;
                IPCChannel.Broadcast(new StopAttackIPCMessage());
            }
            else if (n3Msg.N3MessageType == N3MessageType.GenericCmd)
            {
                if (!syncUse) //!_menu.GetBool("SyncUse"))
                    return;

                GenericCmdMessage genericCmdMsg = (GenericCmdMessage)n3Msg;
                Chat.WriteLine("Using " + genericCmdMsg.Target.Type.ToString());


                List<Item> characterItems = Inventory.Items;

                bool found = false;
                foreach (Item item in characterItems)
                {
                    //Chat.WriteLine($"{item.Slot} - {item.LowId} - {item.Name} - {item.QualityLevel} - {item.UniqueIdentity}");

                    if (item.Slot == genericCmdMsg.Target)
                    {
                        found = true;
                        break;
                    }
                }
                List<Container> backpacks = Inventory.Backpacks;
                foreach (Container backpack in backpacks)
                {
                    //Chat.WriteLine($"{backpack.Identity} - IsOpen:{backpack.IsOpen}{((backpack.IsOpen) ? $" - Items:{backpack.Items.Count}" : "")}");

                    foreach(Item i in backpack.Items)
                    {
                        if (i.Name == "Insurance Claim Recall Beacon")
                        {
                            // insurance item
                            int a = 1;
                            Chat.WriteLine($"{backpack.Identity} - Item: {i.Name}   identity: {i.UniqueIdentity}");

                        }

                    }
                }
                SimpleItem simpleItem = DynelManager.GetDynel<SimpleItem>(genericCmdMsg.Target);
                if (genericCmdMsg.Action == GenericCmdAction.Use && 
                    (genericCmdMsg.Target.Type == IdentityType.Terminal )
/*                    || 
                        (
                            genericCmdMsg.Target.Name == "Keys" &&
                            genericCmdMsg.Target.Type == IdentityType.Backpack
                        )*/
                    )
                {




                    //Chat.WriteLine("Using " + );
                    IPCChannel.Broadcast(new UseMessage()
                    {
                        Target = genericCmdMsg.Target
                    });
                }
            }
        }
        private void OnTestMessage(int sender, IPCMessage msg)
        {
            if (Game.IsZoning)
                return;

            TestMessage testMsg = (TestMessage)msg;

            if (Playfield.Identity.Instance != testMsg.PlayfieldId)
                return;
            DynelManager.LocalPlayer.Position = testMsg.Position;

        }
        private void OnMoveMessage(int sender, IPCMessage msg)
        {
            //Only followers will act on commands
            if (!Team.IsInTeam || IsActiveWindow)
                return;

            if (Game.IsZoning)
                return;

            MoveMessage moveMsg = (MoveMessage)msg;

            if (Playfield.Identity.Instance != moveMsg.PlayfieldId)
                return;

            DynelManager.LocalPlayer.Position = moveMsg.Position;
            DynelManager.LocalPlayer.Rotation = moveMsg.Rotation;
            MovementController.Instance.SetMovement(moveMsg.MoveType);
        }

        private void OnTargetMessage(int sender, IPCMessage msg)
        {
            //if (!Team.IsInTeam || IsActiveWindow)
            //    return;

            if (Game.IsZoning)
                return;

            TargetMessage targetMsg = (TargetMessage)msg;
            Targeting.SetTarget(targetMsg.Target);
        }

        private void OnAttackMessage(int sender, IPCMessage msg)
        {
            //if (!Team.IsInTeam || IsActiveWindow)
              //  return;

            if (Game.IsZoning)
                return;

            AttackIPCMessage attackMsg = (AttackIPCMessage)msg;


            if (Playfield.Identity.Instance != attackMsg.PlayfieldId)
                return;

            // Verify that this isn't "Azdaja the Joyous", if it is, and you are not soldja or enfo, 
            // and its health is not 20%, do not attack

            foreach (SimpleChar npc in DynelManager.Characters)
            {
                if (npc.Identity == attackMsg.Target)
                {
                    if (npc.Name == "Azdaja the Joyous")
                    {

                        if (npc.HealthPercent > 20.0 &&
                            (
                             DynelManager.LocalPlayer.Profession != Profession.Enforcer &&
                             DynelManager.LocalPlayer.Profession != Profession.Soldier 
                            )
                           )
                        {
                            return;
                        }
                    }
                }
            }

            DynelManager.LocalPlayer.Attack(attackMsg.Target);
            DynelManager.LocalPlayer.Pets.Attack(attackMsg.Target);
            Chat.WriteLine("Attacking with pets");
        }

        private void OnStopAttackMessage(int sender, IPCMessage msg)
        {
            if (!Team.IsInTeam || IsActiveWindow)
                return;

            if (Game.IsZoning)
                return;

            DynelManager.LocalPlayer.StopAttack();
        }

        private void OnUseMessage(int sender, IPCMessage msg)
        {
            if (!Team.IsInTeam || IsActiveWindow)
                return;

            if (Game.IsZoning)
                return;

            UseMessage useMsg = (UseMessage)msg;
            DynelManager.GetDynel<SimpleItem>(useMsg.Target)?.Use();
        }
        
        private void OnSyncMovesMessage(int sender, IPCMessage msg)
        {
            if (!Team.IsInTeam || IsActiveWindow)
                return;

            if (Game.IsZoning)
                return;

            SyncMovesMessage smMsg = (SyncMovesMessage)msg;
            syncMoves = smMsg.syncMoves;
        }
        private void OnSyncAttacksMessage(int sender, IPCMessage msg)
        {
            if (!Team.IsInTeam || IsActiveWindow)
                return;

            if (Game.IsZoning)
                return;

            SyncAttacksMessage saMsg = (SyncAttacksMessage)msg;
            syncAttacks = saMsg.syncAttacks;
        }
        
        private void OnDocMessage(int sender, IPCMessage msg)
        {

            if (Game.IsZoning)
                return;

            if (DynelManager.LocalPlayer.Profession == Profession.Doctor)
            {
                DocMessage docMsg = (DocMessage)msg;
                DocCommands requestedBuff = (DocCommands)docMsg.buff;

                if (requestedBuff == DocCommands.CH)
                {
                    //Alpha Omega nano ID is 42409 https://aoitems.com/item/42409/alpha-and-omega/

                    Spell.Find(42409, out Spell curSpell);
                    if (curSpell != null)
                    {
                        curSpell.Cast();
                    }
                }
            }
        }
        private void OnEngiMessage(int sender, IPCMessage msg)
        {

            if (Game.IsZoning)
                return;

            if (DynelManager.LocalPlayer.Profession == Profession.Engineer)
            {
                EngiMessage engiMsg = (EngiMessage)msg;
                EngiDebufAuras requestedBuff = (EngiDebufAuras)engiMsg.buff;

                // Since NSD is the top of the stacking chain, if it's requested, it will remove other nanos.
                // However if blinds are requested and NSD is running, we need to remove it
                if (requestedBuff == EngiDebufAuras.NSD)
                {
                    //NSD nano ID is 154725 https://aoitems.com/item/154725/null-space-disruptor/

                    Spell.Find(154725, out Spell curSpell);
                    if (curSpell != null)
                    {
                        curSpell.Cast();
                    }
                }
                else if (requestedBuff == EngiDebufAuras.blinds)
                {

                    if (DynelManager.LocalPlayer.Buffs.Find(NanoLine.EngineerDebuffAuras, out Buff buff))
                    {
                        if (buff.Name == "Null Space Disruptor")
                            buff.Remove();
                    }
                    //Disruptive Void Projector nano ID is 154715 https://aoitems.com/item/154715/disruptive-void-projector/
                    Spell.Find(154715, out Spell curSpell);
                    if (curSpell != null)
                    {
                        curSpell.Cast();
                    }
                }
                else
                {
                    if (DynelManager.LocalPlayer.Buffs.Find(NanoLine.EngineerDebuffAuras, out Buff buff))
                    {
                        buff.Remove();
                    }
                }
            }
        }
        private void OnKeeperMessage(int sender, IPCMessage msg)
        {

            if (Game.IsZoning)
                return;

            if (DynelManager.LocalPlayer.Profession == Profession.Keeper)
            {
                KeeperMessage engiMsg = (KeeperMessage)msg;
                KeeperCommands keeperCommand = (KeeperCommands)engiMsg.command;

                // Since NSD is the top of the stacking chain, if it's requested, it will remove other nanos.
                // However if blinds are requested and NSD is running, we need to remove it
                if (keeperCommand == KeeperCommands.antifear)
                {
                    castAntifear = true;
                }
                else // stop
                {
                    castAntifear = false;
                }
            }
        }

        private bool CheckForPOHDebuffs()
        {
            foreach (Buff buff in DynelManager.LocalPlayer.Buffs)
            {
                if (buff.Name == "Call of Rust" || buff.Name == "Festering Skin")
                {
                    if (DynelManager.LocalPlayer.IsAttacking)
                        DynelManager.LocalPlayer.StopAttack();

                    foreach (TeamMember m in Team.Members)
                    {
                        foreach (Buff b in m.Character.Buffs)
                        {
                            if (b.Name == "Call of Rust" || b.Name == "Festering Skin")
                                Chat.WriteLine($"{m.Name} has {b.Name}");
                        }
                    }
                    return true;
                }
            }

            return false;
        }
        private bool IsLocalPlayerOnAltar()
        {
            IEnumerable<SimpleChar> targets = DynelManager.Characters
                .Where(c => (c.Name == "Altar of Purification" || c.Name == "Altar of Torture"));
            foreach (SimpleChar altar in targets)
            {
                if (DynelManager.LocalPlayer.Position == altar.Position)
                    return true;
            }
                
            return false;
        }
        private SimpleChar FindPOHGreenAltar()
        {
            IEnumerable<SimpleChar> targets = DynelManager.Characters
                .Where(c => (c.Name == "Altar of Purification" || c.Name == "Altar of Torture"));

            foreach (SimpleChar altar in targets)
            {
                foreach (Buff buff in altar.Buffs)
                {
                    if (buff.Name == "Altar of Purification")
                    {
                        return altar;
                    }
                }

            }
            return null;
        }
        private void OnUpdate(object sender, float e)
        {
            try
            {
                if (DynelManager.LocalPlayer.Profession == Profession.Keeper && castAntifear == true)
                {
                    //Courage of the Just is 279378

                    Spell.Find(279378, out Spell curSpell);
                    if (curSpell != null)
                    {
                        curSpell.Cast();
                    }
                }
                if (monitorPOHBuffs)
                {
                    if (needGreenLight == false)
                    {
                        needGreenLight = CheckForPOHDebuffs();
                    }

                    if (needGreenLight == true)
                    {
                        // Find an altar and meep there when the light is green
                        SimpleChar altar = FindPOHGreenAltar();
                        DynelManager.LocalPlayer.Position = altar.Position;
                        needGreenLight = false;
                    }

                    if (needGreenLight == false && IsLocalPlayerOnAltar() == true)
                    { 
                        PositionProfsForFloor5();
                        Floor5Attack();
                    }
                }
            }
            catch (Exception ex)
            {
                Chat.WriteLine(ex.ToString());
            }
        }
        private void Floor5Attack()
        {
            SimpleChar target = null;
            switch (DynelManager.LocalPlayer.Profession)
            {
                case Profession.Doctor:
                case Profession.Trader:
                case Profession.Bureaucrat:
                case Profession.Engineer:
                case Profession.Soldier:
                    // ok we are looking for (in that order)
                    // Sorrowful Voidling
                    // Fearful Voidling
                    // Pained Voidling

                    target = DynelManager.Characters
                        .Where(c => c.Name == "Sorrowful Voidling")
                        .FirstOrDefault();
                    if (target != null && (!DynelManager.LocalPlayer.IsAttacking || DynelManager.LocalPlayer.IsAttacking && DynelManager.LocalPlayer.FightingTarget.Name != "Sorrowful Voidling"))
                    {
                        DynelManager.LocalPlayer.Attack(target, true);
                        Chat.WriteLine("Fighting Sorrowful Voidling");
                        break;
                    }
                    else if (target == null)
                        target = DynelManager.Characters
                        .Where(c => c.Name == "Fearful Voidling")
                        .FirstOrDefault();
                    if (target != null && (!DynelManager.LocalPlayer.IsAttacking || DynelManager.LocalPlayer.IsAttacking && DynelManager.LocalPlayer.FightingTarget.Name != "Fearful Voidling"))
                    {
                        DynelManager.LocalPlayer.Attack(target, true);
                        Chat.WriteLine("Fighting Fearful Voidling");
                        break;
                    }

                    if (target == null)
                        target = DynelManager.Characters
                        .Where(c => c.Name == "Pained Voidling")
                        .FirstOrDefault();
                    if (target != null && (!DynelManager.LocalPlayer.IsAttacking || DynelManager.LocalPlayer.IsAttacking && DynelManager.LocalPlayer.FightingTarget.Name != "Pained Voidling"))
                    {
                        DynelManager.LocalPlayer.Attack(target, true);
                        Chat.WriteLine("Fighting Pained Voidling");
                        break;
                    }

                    break;
                case Profession.Enforcer:
                    target = DynelManager.Characters
                        .Where(c => c.Name == "Azdaja the Joyous")
                        .FirstOrDefault();
                    if ((target != null && (!DynelManager.LocalPlayer.IsAttacking) || (
                        DynelManager.LocalPlayer.IsAttacking && DynelManager.LocalPlayer.FightingTarget.Name != "Azdaja the Joyous")))
                        DynelManager.LocalPlayer.Attack(target, true);
                    break;
            }

        }
        private void PositionProfsForFloor5()
        {
            switch (DynelManager.LocalPlayer.Profession)
            {
                case Profession.Doctor:
                case Profession.Trader:
                case Profession.Bureaucrat:
                case Profession.Engineer:
                    DynelManager.LocalPlayer.Position = new Vector3(88.3, 1.0, 278.8);
                    break;
                case Profession.Enforcer:
                case Profession.Soldier:
                    DynelManager.LocalPlayer.Position = new Vector3(55.4, 1.0, 279.5);
                    break;
            }
            movementController.Update();
        }
        private void OnFloorMessage(int sender, IPCMessage msg)
        {
            if (Game.IsZoning)
                return;
            // Stop syncing moves and attacks, we gonna prep for floor position in order to kill portal mobs or bosses
            syncMoves = false;
            syncAttacks = false;

            Vector3 lookAt;
            FloorMessage floorMsg = (FloorMessage)msg;

            floor = floorMsg.Floor;
            Chat.WriteLine($"Positioning for floor {floor}");
            switch (floor)
            {
                // Floor 1 there is nothing to do, just kill the two keeper mobs at entrance and go
                case 1:
                    break;
                // too easy for now to do anything, kill the portal mobs, you can actually kill first, and rush to kill second and the vortex boss will appear
                case 2:
                // Floor 3 separate tank + DD from support profs
                case 3:
                    switch (DynelManager.LocalPlayer.Profession)
                    {
                        case Profession.Enforcer:
                        case Profession.Soldier:
                        case Profession.Shade:
                        case Profession.MartialArtist:
                        case Profession.Keeper:
                            DynelManager.LocalPlayer.Position = floor2_tank_pos;
                            Chat.WriteLine($"Setting position for TANK spot on {floor}");

                            break;
                        case Profession.Doctor:
                        case Profession.Trader:
                        case Profession.Bureaucrat:
                        case Profession.Engineer:
                            DynelManager.LocalPlayer.Position = floor2_support_pos;

                            Chat.WriteLine($"Setting position for Support spot on {floor}");

                            break;

                    }
                    break;
                // Floor 4 "meep" 3 toons to the 3 portal mobs and kill them
                case 4:
                    switch (DynelManager.LocalPlayer.Profession)
                    {
                        case Profession.Soldier:
                        case Profession.MartialArtist:
                            // Put soldja or MA to the north position, just close enough so the door is open, but far enough so it gets no aggro
                            DynelManager.LocalPlayer.Position = new Vector3(70.9, 6.0f, 112.7);
                            lookAt = new Vector3(0, -0.9989639, 0);
                            movementController.AppendDestination(DynelManager.LocalPlayer.Position);

                            movementController.Update();
                            break;
                        case Profession.Engineer:
                            // Enfo on the west side
                            DynelManager.LocalPlayer.Position = new Vector3(58.3, 6.0f, 49.1);
                            break;
                        case Profession.Bureaucrat:
                            // Finally crat on East side.
                            DynelManager.LocalPlayer.Position = new Vector3(123.1, 6.0f, 46.6);
                            break;

                        default:
                            break;
                    }

                    //find a portal
                    IEnumerable<SimpleChar> targets = DynelManager.Characters
                        .Where(c => (c.Name == "Portal Warden"));

                    foreach (SimpleChar target in targets)
                    {
                        if (target.IsAlive == true)
                        {
                            target.Target();
                            DynelManager.LocalPlayer.Attack(target);
                            break;
                        }
                        else
                        {
                            Chat.WriteLine($"Found a ghost Portal Warden, skipping ...");
                        }
                    }
                    break;
                // Floor 5 serious shit starts here.
                case 5:
                    monitorPOHBuffs = true;
                    syncAttacks = true;
                    PositionProfsForFloor5();

                    break;

            }
            movementController.Update();

        }
    }
}
