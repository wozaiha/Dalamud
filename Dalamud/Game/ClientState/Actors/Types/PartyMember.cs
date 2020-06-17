using Dalamud.Game.ClientState.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Dalamud.Game.ClientState.Actors.Types
{
    public class PartyMember
    {
        public string CharacterName;
        public long Unknown;
        public Actor Actor;
        public ObjectKind ObjectKind;
        internal static IntPtr NativeUtf8FromString(string managedString)
        {
            int len = Encoding.UTF8.GetByteCount(managedString);
            byte[] buffer = new byte[len + 1];
            Encoding.UTF8.GetBytes(managedString, 0, managedString.Length, buffer, 0);
            IntPtr nativeUtf8 = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, nativeUtf8, buffer.Length);
            return nativeUtf8;
        }

        internal static string StringFromNativeUtf8(IntPtr nativeUtf8)
        {
            int len = 0;
            while (Marshal.ReadByte(nativeUtf8, len) != 0) ++len;
            byte[] buffer = new byte[len];
            Marshal.Copy(nativeUtf8, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }
        public PartyMember(ActorTable table, Structs.PartyMember rawData)
        {
            CharacterName = StringFromNativeUtf8(rawData.namePtr);
            Unknown = rawData.unknown;
            Actor = null;
            for (var i = 0; i < table.Length; i++)
            {
                if (table[i] != null && table[i].ActorId == rawData.actorId)
                {
                    Actor = table[i];
                    break;
                }
            }
            ObjectKind = rawData.objectKind;
        }
    }
}
