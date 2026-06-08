using System.Collections.Generic;
using Hkmp.Api.Client;
using Hkmp.Api.Server;
using UnityEngine;

namespace HollowKnightHKMPMod
{
    /// <summary>
    /// Handles scene synchronization between clients and server.
    /// </summary>
    public sealed class SceneSync
    {
        public static SceneSync Instance { get; } = new SceneSync();

        public ushort LocalPlayerId { get; set; } = 0;

        private IClientAddonApi _clientAddonApi;
        private IServerAddonApi _serverAddonApi;

        private SceneSync() { }

        // ── Lifecycle ─────────────────────────────────────────────────────────

        public void Apply()
        {
            // Register for scene change events
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
        }

        public void Unapply()
        {
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        // ── Event handlers ───────────────────────────────────────────────────

        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
        {
            // Clear enemy tracking when scene changes
            MonsterManager.Instance.Clear();
            
            // Request current enemy states from server (if we're in a multiplayer session)
            if (_clientAddonApi != null)
            {
                // This would be used to sync up with the current state of enemies in the scene
                // The actual implementation would depend on how HKMP handles this
            }
        }

        // ── Client registration ────────────────────────────────────────────────

        public void RegisterClient(IClientAddonApi clientAddonApi)
        {
            _clientAddonApi = clientAddonApi;
        }

        // ── Server registration ────────────────────────────────────────────────

        public void RegisterServer(IServerAddonApi serverAddonApi)
        {
            _serverAddonApi = serverAddonApi;
        }
    }
}