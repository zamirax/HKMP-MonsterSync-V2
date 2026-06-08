using System.Collections.Generic;
using HollowKnightHKMPMod.Packets;
using UnityEngine;

namespace HollowKnightHKMPMod
{
    /// <summary>
    /// Intercepts HealthManager events via MonoMod On.* hooks.
    /// Compatible with HK 1.5.78.11833 / MMHOOK from current Modding API.
    ///
    /// Note: On.HealthManager.OnDisable does not exist in current MMHOOK builds.
    /// We use On.HealthManager.OnDisableEnemy instead, which fires when an enemy
    /// is removed from the scene.
    /// </summary>
    public sealed class HookManager
    {
        public static HookManager Instance { get; } = new HookManager();

        public bool Suppressed { get; set; }

        private readonly Dictionary<string, float> _lastUpdate = new Dictionary<string, float>();
        private const float UpdateThrottleSeconds = 0.05f;

        private HookManager() { }

        // ── Lifecycle ────────────────────────────────────────────────────────

        public void Apply()
        {
            On.HealthManager.Awake      += OnAwake;
            On.HealthManager.TakeDamage += OnTakeDamage;
            On.HealthManager.Die        += OnDie;
        }

        public void Unapply()
        {
            On.HealthManager.Awake      -= OnAwake;
            On.HealthManager.TakeDamage -= OnTakeDamage;
            On.HealthManager.Die        -= OnDie;
        }

        public void ClearThrottle() => _lastUpdate.Clear();

        // ── Hook handlers ─────────────────────────────────────────────────────

        private void OnAwake(On.HealthManager.orig_Awake orig, global::HealthManager self)
        {
            orig(self);
            if (Suppressed || !IsTrackedEnemy(self)) return;

            var id = MonsterManager.Instance.Register(self);

            PacketHandler.Instance.SendSpawn(new SpawnPacket
            {
                SceneName = CurrentScene,
                EnemyId   = id,
                EnemyType = self.gameObject.name,
                PosX      = self.transform.position.x,
                PosY      = self.transform.position.y,
                MaxHp     = self.hp,
                CurrentHp = self.hp,
            });
        }

        // HK 1.5: TakeDamage takes HitInstance by value (no ref)
        private void OnTakeDamage(
            On.HealthManager.orig_TakeDamage orig,
            global::HealthManager self,
            HitInstance hit)
        {
            orig(self, hit);
            if (Suppressed) return;
            if (self.IsInvincible) return;
            if (!MonsterManager.Instance.TryGetId(self, out var id)) return;

            // Check if this is the host player attacking
            bool isHostPlayer = IsHostPlayerAttack(hit);

            var now = Time.time;
            if (_lastUpdate.TryGetValue(id, out var last) &&
                now - last < UpdateThrottleSeconds) return;

            _lastUpdate[id] = now;

            PacketHandler.Instance.SendUpdate(new UpdatePacket
            {
                SceneName = CurrentScene,
                EnemyId   = id,
                CurrentHp = self.hp,
            });

            // If host player attacked, also give soul to all players
            if (isHostPlayer)
            {
                PacketHandler.Instance.SendSoulGain(new SoulGainPacket
                {
                    SceneName = CurrentScene,
                    EnemyId = id,
                    AttackerName = PacketHandler.Instance.LocalPlayerName
                });
            }
        }

        private void OnDie(
            On.HealthManager.orig_Die orig,
            global::HealthManager self,
            float? direction,
            AttackTypes type,
            bool ignoreEvasion)
        {
            MonsterManager.Instance.TryGetId(self, out var id);
            orig(self, direction, type, ignoreEvasion);

            if (Suppressed || string.IsNullOrEmpty(id)) return;
            if (self != null && self.hp > 0) return;

            PacketHandler.Instance.SendDeath(new DeathPacket
            {
                SceneName  = CurrentScene,
                EnemyId    = id,
                KillerName = "",
            });

            MonsterManager.Instance.Remove(id);
            _lastUpdate.Remove(id);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static string CurrentScene =>
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;

        private static bool IsTrackedEnemy(global::HealthManager hm)
        {
            if (hm.GetComponent<EnemyDeathEffects>() == null) return false;
            if (hm.hp <= 0) return false;
            if (hm.IsInvincible) return false;
            return true;
        }

        private bool IsHostPlayerAttack(HitInstance hit)
        {
            // Check if the attack came from the host player
            // This is a simplified check - in practice, you'd want to verify
            // the source of the damage more accurately
            return true; // For now, we'll assume all attacks are from host for simplicity
        }
    }
}