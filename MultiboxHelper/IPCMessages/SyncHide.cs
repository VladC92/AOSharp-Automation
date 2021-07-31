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
    [AoContract((int)IPCOpcode.Target)]
    public class HideMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Hide;

        [AoMember(0)]
        public Identity Target { get; set; }
    }
}
