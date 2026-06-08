using System.IO;

namespace HollowKnightHKMPMod.Packets
{
    // ── Packet IDs ──────────────────────────────────────────────────────────────
    // These enums are shared between client→server and server→client directions.
    // Keep values stable — changing them breaks compatibility with older clients.

    public enum ToServerPacket : ushort
    {
        MonsterSpawn   = 0,
        MonsterUpdate  = 1,
        MonsterDeath   = 2,
        SceneRequest   = 3,   // "please send me your enemy list"
        SceneResponse  = 4,   // intentionally unused client→server; reserved
        SoulGain       = 5,   // New packet for soul gain notification
    }

    public enum ToClientPacket : ushort
    {
        MonsterSpawn   = 0,
        MonsterUpdate  = 1,
        MonsterDeath   = 2,
        SceneRequest   = 3,
        SceneResponse  = 4,   // server → specific client with enemy list
        SoulGain       = 5,   // New packet for soul gain notification
    }

    // ── Packet structs ──────────────────────────────────────────────────────────

    /// Sent on scene entry and in response to a SceneRequest.
    public sealed class SpawnPacket
    {
        public string SceneName  = "";
        public string EnemyId    = "";
        public string EnemyType  = "";
        public float  PosX;
        public float  PosY;
        public int    MaxHp;
        public int    CurrentHp;

        public byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms);
            w.Write(SceneName);
            w.Write(EnemyId);
            w.Write(EnemyType);
            w.Write(PosX);
            w.Write(PosY);
            w.Write(MaxHp);
            w.Write(CurrentHp);
            return ms.ToArray();
        }

        public static SpawnPacket Deserialize(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var r  = new BinaryReader(ms);
            return new SpawnPacket
            {
                SceneName  = r.ReadString(),
                EnemyId    = r.ReadString(),
                EnemyType  = r.ReadString(),
                PosX       = r.ReadSingle(),
                PosY       = r.ReadSingle(),
                MaxHp      = r.ReadInt32(),
                CurrentHp  = r.ReadInt32(),
            };
        }
    }

    /// Sent whenever an enemy takes a hit (throttled to once per 50 ms per enemy).
    public sealed class UpdatePacket
    {
        public string SceneName = "";
        public string EnemyId   = "";
        public int    CurrentHp;

        public byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms);
            w.Write(SceneName);
            w.Write(EnemyId);
            w.Write(CurrentHp);
            return ms.ToArray();
        }

        public static UpdatePacket Deserialize(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var r  = new BinaryReader(ms);
            return new UpdatePacket
            {
                SceneName = r.ReadString(),
                EnemyId   = r.ReadString(),
                CurrentHp = r.ReadInt32(),
            };
        }
    }

    /// Sent when an enemy actually dies (after orig() confirms it).
    public sealed class DeathPacket
    {
        public string SceneName  = "";
        public string EnemyId    = "";
        public string KillerName = ""; // HKMP player name or "" for environment

        public byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms);
            w.Write(SceneName);
            w.Write(EnemyId);
            w.Write(KillerName);
            return ms.ToArray();
        }

        public static DeathPacket Deserialize(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var r  = new BinaryReader(ms);
            return new DeathPacket
            {
                SceneName  = r.ReadString(),
                EnemyId    = r.ReadString(),
                KillerName = r.ReadString(),
            };
        }
    }

    /// Client broadcasts this to ask other players for their enemy states.
    /// FIX: server now only relays it to OTHER clients, not back to sender.
    public sealed class SceneRequestPacket
    {
        public string SceneName    = "";
        public ushort RequesterId;   // HKMP player ID — server uses this to exclude sender

        public byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms);
            w.Write(SceneName);
            w.Write(RequesterId);
            return ms.ToArray();
        }

        public static SceneRequestPacket Deserialize(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var r  = new BinaryReader(ms);
            return new SceneRequestPacket
            {
                SceneName   = r.ReadString(),
                RequesterId = r.ReadUInt16(),
            };
        }
    }

    /// New packet for notifying all players when any player gains soul from attacking an enemy.
    public sealed class SoulGainPacket
    {
        public string SceneName = "";
        public string EnemyId   = "";
        public string AttackerName = "";

        public byte[] Serialize()
        {
            using var ms = new MemoryStream();
            using var w  = new BinaryWriter(ms);
            w.Write(SceneName);
            w.Write(EnemyId);
            w.Write(AttackerName);
            return ms.ToArray();
        }

        public static SoulGainPacket Deserialize(byte[] data)
        {
            using var ms = new MemoryStream(data);
            using var r  = new BinaryReader(ms);
            return new SoulGainPacket
            {
                SceneName   = r.ReadString(),
                EnemyId     = r.ReadString(),
                AttackerName = r.ReadString(),
            };
        }
    }
}