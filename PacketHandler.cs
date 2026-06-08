using System.Collections.Generic;
using Hkmp.Api.Client;
using Hkmp.Api.Server;
using HollowKnightHKMPMod.Packets;
using UnityEngine;

namespace HollowKnightHKMPMod
{
    /// <summary>
    /// Handles sending and receiving packets between clients and server.
    /// </summary>
    public sealed class PacketHandler
    {
        public static PacketHandler Instance { get; } = new PacketHandler();

        public string LocalPlayerName { get; set; } = "Player";

        private IClientAddonApi _clientAddonApi;
        private IServerAddonApi _serverAddonApi;

        private readonly Dictionary<string, HealthManager> _localEnemies = new();

        private PacketHandler() { }

        // ── Client registration ────────────────────────────────────────────────

        public void RegisterClient(IClientAddonApi clientAddonApi)
        {
            _clientAddonApi = clientAddonApi;

            _clientAddonApi.PacketManager.RegisterServerPacketHandler<SpawnPacket>(
                ToServerPacket.MonsterSpawn,
                OnServerSpawn
            );

            _clientAddonApi.PacketManager.RegisterServerPacketHandler<UpdatePacket>(
                ToServerPacket.MonsterUpdate,
                OnServerUpdate
            );

            _clientAddonApi.PacketManager.RegisterServerPacketHandler<DeathPacket>(
                ToServerPacket.MonsterDeath,
                OnServerDeath
            );

            _clientAddonApi.PacketManager.RegisterServerPacketHandler<SoulGainPacket>(
                ToServerPacket.SoulGain,
                OnServerSoulGain
            );
        }

        // ── Server registration ────────────────────────────────────────────────

        public void RegisterServer(IServerAddonApi serverAddonApi)
        {
            _serverAddonApi = serverAddonApi;

            _serverAddonApi.PacketManager.RegisterClientPacketHandler<SpawnPacket>(
                ToClientPacket.MonsterSpawn,
                OnClientSpawn
            );

            _serverAddonApi.PacketManager.RegisterClientPacketHandler<UpdatePacket>(
                ToClientPacket.MonsterUpdate,
                OnClientUpdate
            );

            _serverAddonApi.PacketManager.RegisterClientPacketHandler<DeathPacket>(
                ToClientPacket.MonsterDeath,
                OnClientDeath
            );

            _serverAddonApi.PacketManager.RegisterClientPacketHandler<SoulGainPacket>(
                ToClientPacket.SoulGain,
                OnClientSoulGain
            );
        }

        // ── Send methods ───────────────────────────────────────────────────────

        public void SendSpawn(SpawnPacket packet)
        {
            _clientAddonApi?.PacketManager.SendServerPacket(ToServerPacket.MonsterSpawn, packet);
        }

        public void SendUpdate(UpdatePacket packet)
        {
            _clientAddonApi?.PacketManager.SendServerPacket(ToServerPacket.MonsterUpdate, packet);
        }

        public void SendDeath(DeathPacket packet)
        {
            _clientAddonApi?.PacketManager.SendServerPacket(ToServerPacket.MonsterDeath, packet);
        }

        public void SendSoulGain(SoulGainPacket packet)
        {
            _clientAddonApi?.PacketManager.SendServerPacket(ToServerPacket.SoulGain, packet);
        }

        // ── Server → Client handlers ───────────────────────────────────────────

        private void OnServerSpawn(ushort playerId, SpawnPacket packet)
        {
            // This is handled by the client-side logic
        }

        private void OnServerUpdate(ushort playerId, UpdatePacket packet)
        {
            if (!MonsterManager.Instance.TryGetEnemy(packet.EnemyId, out var enemy))
                return;

            // Apply health changes to all players' copies of the enemy
            enemy.hp = packet.CurrentHp;
        }

        private void OnServerDeath(ushort playerId, DeathPacket packet)
        {
            if (!MonsterManager.Instance.TryGetEnemy(packet.EnemyId, out var enemy))
                return;

            // Kill the enemy on all clients
            enemy.Die();
        }

        private void OnServerSoulGain(ushort playerId, SoulGainPacket packet)
        {
            // When any player gains soul, all players should gain soul
            // This is handled by client-side logic
            if (MonsterManager.Instance.TryGetEnemy(packet.EnemyId, out var enemy))
            {
                // Simulate gaining soul for all players
                // In practice, you'd trigger the soul collection effect here
                Debug.Log($"Player {packet.AttackerName} gained soul from attacking {enemy.gameObject.name}");
            }
        }

        // ── Client → Server handlers ───────────────────────────────────────────

        private void OnClientSpawn(ushort playerId, SpawnPacket packet)
        {
            // This is handled by the server-side logic
        }

        private void OnClientUpdate(ushort playerId, UpdatePacket packet)
        {
            if (!MonsterManager.Instance.TryGetEnemy(packet.EnemyId, out var enemy))
                return;

            // Apply health changes to all players' copies of the enemy
            enemy.hp = packet.CurrentHp;
        }

        private void OnClientDeath(ushort playerId, DeathPacket packet)
        {
            if (!MonsterManager.Instance.TryGetEnemy(packet.EnemyId, out var enemy))
                return;

            // Kill the enemy on all clients
            enemy.Die();
        }

        private void OnClientSoulGain(ushort playerId, SoulGainPacket packet)
        {
            // When any player gains soul, all players should gain soul
            if (MonsterManager.Instance.TryGetEnemy(packet.EnemyId, out var enemy))
            {
                // Simulate gaining soul for all players
                Debug.Log($"Player {packet.AttackerName} gained soul from attacking {enemy.gameObject.name}");
            }
        }
    }
}