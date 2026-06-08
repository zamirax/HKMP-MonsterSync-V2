using Hkmp.Api.Client;
using Hkmp.Api.Server;
using Modding;
using System.Collections.Generic;
using UnityEngine;

namespace HollowKnightHKMPMod
{
    /// <summary>
    /// Hollow Knight HKMP mod that syncs mob and boss health from host to all players,
    /// and gives soul to all players when any player attacks mobs.
    /// </summary>
    public sealed class HollowKnightHKMPMod : Mod, IClientAddon, IServerAddon, ITogglableMod,
        IGlobalSettings<HollowKnightHKMPSettings>
    {
        public static HollowKnightHKMPMod Instance { get; private set; }

        private const string ModVersion = "1.0.0";

        public HollowKnightHKMPSettings Settings { get; private set; } = new HollowKnightHKMPSettings();

        public HollowKnightHKMPMod() : base("HollowKnightHKMPMod") { }

        public override string GetVersion() => ModVersion;

        string IClientAddon.Name         => HollowKnightHKMPAddon.Name;
        string IServerAddon.Name         => HollowKnightHKMPAddon.Name;
        bool   IClientAddon.NeedsNetwork => true;
        bool   IServerAddon.NeedsNetwork => true;

        // ── Initialisation ────────────────────────────────────────────────────

        public override void Initialize()
        {
            Instance = this;
            Log($"HollowKnightHKMPMod {ModVersion} initializing…");
            HookManager.Instance.Apply();
            SceneSync.Instance.Apply();
            Log("HollowKnightHKMPMod ready.");
        }

        public void Unload()
        {
            HookManager.Instance.Unapply();
            SceneSync.Instance.Unapply();
            MonsterManager.Instance.Clear();
            Log("HollowKnightHKMPMod unloaded.");
        }

        // ── HKMP ─────────────────────────────────────────────────────────────

        void IClientAddon.Initialize(IClientAddonApi clientAddonApi)
        {
            PacketHandler.Instance.RegisterClient(clientAddonApi);
            try
            {
                PacketHandler.Instance.LocalPlayerName =
                    clientAddonApi.ClientManager.Username ?? "Player";
                SceneSync.Instance.LocalPlayerId =
                    (ushort)(clientAddonApi.ClientManager.Id ?? 0);
            }
            catch
            {
                Log("HollowKnightHKMPMod: could not read HKMP player info — using defaults.");
            }
        }

        void IServerAddon.Initialize(IServerAddonApi serverAddonApi)
        {
            PacketHandler.Instance.RegisterServer(serverAddonApi);
        }

        // ── IGlobalSettings ───────────────────────────────────────────────────

        public void OnLoadGlobal(HollowKnightHKMPSettings s) => Settings = s;
        public HollowKnightHKMPSettings OnSaveGlobal()       => Settings;
    }

    public static class HollowKnightHKMPAddon
    {
        public const string Name = "HollowKnightHKMPMod";
    }
}