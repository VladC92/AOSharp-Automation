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
    [AoContract((int)IPCOpcode.Test)]
    public class TestMessage : IPCMessage
    {
        public override short Opcode => (short)IPCOpcode.Test;


        [AoMember(1)]
        public int PlayfieldId { get; set; }

        [AoMember(2)]
        public Vector3 Position { get; set; }

    }
}
