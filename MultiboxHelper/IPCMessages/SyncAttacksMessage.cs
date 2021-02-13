using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AOSharp.Common.GameData;
using AOSharp.Core.IPC;
using SmokeLounge.AOtomation.Messaging.Serialization.MappingAttributes;

namespace MultiboxHelper.IPCMessages
{
    [AoContract((int)IPCOpcode.SyncAttacks)]
    public class SyncAttacksMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.SyncAttacks;

        [AoMember(0)]
        public bool syncAttacks { get; set; }
    }
}
