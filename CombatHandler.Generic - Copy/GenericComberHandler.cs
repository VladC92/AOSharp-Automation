using AOSharp.Common.GameData;
using AOSharp.Common.Helpers;
using AOSharp.Common.Unmanaged.Imports;
using AOSharp.Core;
using AOSharp.Core.Inventory;
using AOSharp.Core.IPC;
using AOSharp.Core.UI;
using SmokeLounge.AOtomation.Messaging.Messages;
using SmokeLounge.AOtomation.Messaging.Messages.N3Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace AOSharp.StackDetector
{
    public class Main : AOPluginEntry
    {
        private Dictionary<int, List<EquipSlot>> _observedEquips = new Dictionary<int, List<EquipSlot>>();
        private Dictionary<string, string> _observedStacks = new Dictionary<string, string>();
        private double _lastUpdateTime = 0;

        public override void Run(string pluginDir)
        {
            Game.OnUpdate += OnUpdate;
            Network.N3MessageReceived += Network_N3MessageReceived;
            DynelManager.DynelSpawned += DynelSpawned;
        }

        private void DynelSpawned(object s, Dynel dynel)
        {
            if (dynel.Identity.Type == IdentityType.SimpleChar)
            {
                SimpleChar character = dynel.Cast<SimpleChar>();

                if (!character.IsPlayer)
                    return;

                if (_observedEquips.ContainsKey(dynel.Identity.Instance))
                    _observedEquips[dynel.Identity.Instance] = new List<EquipSlot>();
            }
        }

        private void OnUpdate(object s, float deltaTime)
        {
            if (Time.NormalTime > _lastUpdateTime + 15f)
            {
                foreach (var stack in _observedStacks)
                    Chat.WriteLine($"Stacking detected! {stack.Key} stacked {stack.Value}" , ChatColor.Green);

                _observedStacks.Clear();

                _lastUpdateTime = Time.NormalTime;
            }
        }

        private void Network_N3MessageReceived(object s, N3Message n3Msg)
        {
            if (n3Msg.N3MessageType == N3MessageType.TemplateAction)
            {
                TemplateActionMessage templateAction = n3Msg as TemplateActionMessage;

                if (DynelManager.Find(templateAction.Identity, out SimpleChar character))
                {
                    if (!_observedEquips.ContainsKey(character.Identity.Instance))
                        _observedEquips.Add(character.Identity.Instance, new List<EquipSlot>());

                    EquipSlot equipSlot = (EquipSlot)templateAction.Placement.Instance;

                    if (templateAction.Unknown2 == 7)
                    {
                        if (_observedEquips[character.Identity.Instance].Contains(equipSlot))
                            _observedEquips[character.Identity.Instance].Remove(equipSlot);
                    }
                    else if (templateAction.Unknown2 == 6)
                    {
                        if (_observedEquips[character.Identity.Instance].Contains(equipSlot))
                        {
                            if (!_observedStacks.TryGetValue(character.Name, out _))
                                _observedStacks.Add(character.Name, GetItemName(templateAction.ItemLowId, templateAction.ItemHighId, templateAction.Quality));
                        }
                        else
                        {
                            _observedEquips[character.Identity.Instance].Add(equipSlot);
                        }
                    }
                }
            }
        }

        private unsafe string GetItemName(int lowId, int highId, int ql)
        {
            //Identity none = Identity.None;
            IntPtr pEngine = N3Engine_t.GetInstance();

            if (!DummyItem.CreateDummyItemID(lowId, highId, ql, out Identity dummyItemId))
                throw new Exception($"Failed to create dummy item. LowId: {lowId}\tLowId: {highId}\tLowId: {ql}");

           // IntPtr pItem = N3EngineClientAnarchy_t.GetItemByTemplate(pEngine, dummyItemId , ref none);

          //  if (pItem == IntPtr.Zero)
                throw new Exception($"DummyItem::DummyItem - Unable to locate item. LowId: {lowId}\tLowId: {highId}\tLowId: {ql}");

           // return Utils.UnsafePointerToString((*(MemStruct*)pItem).Name);
        }

        [StructLayout(LayoutKind.Explicit, Pack = 0)]
        private struct MemStruct
        {
            [FieldOffset(0x14)]
            public Identity Identity;

            [FieldOffset(0x9C)]
            public IntPtr Name;
        }
    }
}
