using System;
using System.Runtime.InteropServices;
using System.Text;

namespace Dalamud.Game.ClientState.Actors.Types
{
    /// <summary>
    /// This class represents a party member.
    /// </summary>
    public class PartyMember
    {
        /// <summary>
        /// The name of the character.
        /// </summary>
        public string CharacterName;

        /// <summary>
        /// Unknown.
        /// </summary>
        public long Unknown;

        /// <summary>
        /// The actor object that corresponds to this party member.
        /// </summary>
        public Actor Actor;

        /// <summary>
        /// The kind or type of actor.
        /// </summary>
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

        /// <summary>
        /// Read string from Utf8 bytes.
        /// </summary>
        /// <param name="utf8">The pointer of the string bytes.</param>
        public static string StringFromNativeUtf8(IntPtr utf8)
        {
            int len = 0;
            while (Marshal.ReadByte(utf8, len) != 0) ++len;
            byte[] buffer = new byte[len];
            Marshal.Copy(utf8, buffer, 0, buffer.Length);
            return Encoding.UTF8.GetString(buffer);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyMember"/> class.
        /// </summary>
        /// <param name="table">The ActorTable instance.</param>
        /// <param name="rawData">The interop data struct.</param>
        public PartyMember(ActorTable table, Structs.PartyMember rawData)
        {
            this.CharacterName = StringFromNativeUtf8(rawData.namePtr);
            this.Unknown = rawData.unknown;
            this.Actor = null;
            for (var i = 0; i < table.Length; i++)
            {
                if (table[i] != null && table[i].ActorId == rawData.actorId)
                {
                    this.Actor = table[i];
                    break;
                }
            }

            this.ObjectKind = rawData.objectKind;
        }
    }
}
