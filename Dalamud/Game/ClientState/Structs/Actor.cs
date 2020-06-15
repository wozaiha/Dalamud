using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Dalamud.Game.ClientState.Actors;

namespace Dalamud.Game.ClientState.Structs
{
    /// <summary>
    /// Native memory representation of a FFXIV actor.
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    public struct Actor {
        [FieldOffset(0x30)] [MarshalAs(UnmanagedType.LPUTF8Str, SizeConst = 30)]
        public string Name;

        [FieldOffset(116)] public int ActorId;
        [FieldOffset(128)] public int DataId;
        [FieldOffset(132)] public int OwnerId;
        [FieldOffset(140)] public ObjectKind ObjectKind;
        [FieldOffset(141)] public byte SubKind;
        [FieldOffset(142)] public bool IsFriendly;
        [FieldOffset(144)] public byte YalmDistanceFromPlayerX; // Demo says one of these is x distance
        [FieldOffset(145)] public byte PlayerTargetStatus; // This is some kind of enum
        [FieldOffset(146)] public byte YalmDistanceFromPlayerY; // and the other is z distance
        [FieldOffset(160)] public Position3 Position;
        [FieldOffset(176)] public float Rotation; // Rotation around the vertical axis (yaw), from -pi to pi radians     

        [FieldOffset(0x17E8)] [MarshalAs(UnmanagedType.ByValArray, SizeConst = 28)] public byte[] Customize;

        [FieldOffset(0x1F0)] public int PlayerCharacterTargetActorId;
        [FieldOffset(0x1818)] public int BattleNpcTargetActorId;

        // This field can't be correctly aligned, so we have to cut it manually.
        [FieldOffset(0x17F8)] [MarshalAs(UnmanagedType.ByValArray, SizeConst = 7)]
        public byte[] CompanyTag;

        [FieldOffset(0x1888)] public int NameId;
        [FieldOffset(0x18A4)] public ushort CurrentWorld;
        [FieldOffset(0x18A6)] public ushort HomeWorld;
        [FieldOffset(0x18B8)] public int CurrentHp;
        [FieldOffset(0x18BC)] public int MaxHp;
        [FieldOffset(0x18C0)] public int CurrentMp;
        // This value is weird.  It seems to change semi-randomly between 0 and 10k, definitely
        // in response to mp-using events, but it doesn't often have a value and the changing seems
        // somewhat arbitrary.
        [FieldOffset(0x18CA)] public int MaxMp;
        [FieldOffset(0x18F4)] public byte ClassJob;
        [FieldOffset(0x18F6)] public byte Level;
        [FieldOffset(0x1978)][MarshalAs(UnmanagedType.ByValArray, SizeConst = 20)] public StatusEffect[] UIStatusEffects; 
        
    }
}
