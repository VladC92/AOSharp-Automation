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
    [AoContract((int)IPCOpcode.Leet)]
    public class LeetMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Leet;

        [AoMember(0)]
        public int buff { get; set; }
    }
}

