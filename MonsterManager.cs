using System.Collections.Generic;
using System.Linq;

namespace HollowKnightHKMPMod
{
    /// <summary>
    /// Tracks every living enemy in the current scene and assigns each one a
    /// stable, deterministic string ID that can be transmitted over the network.
    ///
    /// ID format: "{SanitisedName}_{x:F1}_{y:F1}"  (e.g. "Mantis_12.0_-5.3")
    /// Both clients compute the same ID because enemies spawn at the same world
    /// positions in a given scene.  Mid-scene spawns that collide on position
    /// get a suffix counter: "Crawler_4.0_2.0", "Crawler_4.0_2.0_2", etc.
    ///
    /// All public methods are safe to call from the Unity main thread.
    /// </summary>
    public sealed class MonsterManager
    {
        public static MonsterManager Instance { get; } = new();

        // enemyId → HealthManager
        private readonly Dictionary<string, HealthManager> _enemies = new();
        // HealthManager → enemyId  (reverse lookup)
        private readonly Dictionary<HealthManager, string> _ids     = new();
        // baseId → highest suffix used so far (for collision resolution)
        private readonly Dictionary<string, int>           _suffixes = new();

        private MonsterManager() { }

        // ── Lifecycle ────────────────────────────────────────────────────────

        /// Call when a new scene loads (wipes all state).
        public void Clear()
        {
            _enemies.Clear();
            _ids.Clear();
            _suffixes.Clear();
        }

        // ── Registration ─────────────────────────────────────────────────────

        /// Register an enemy and return its stable ID.
        /// Safe to call multiple times for the same object (idempotent).
        public string Register(HealthManager hm)
        {
            if (hm == null) return "";
            if (_ids.TryGetValue(hm, out var existing)) return existing;

            var id = AllocateId(hm);
            _enemies[id] = hm;
            _ids[hm]     = id;
            return id;
        }

        // ── Lookups ──────────────────────────────────────────────────────────

        public bool TryGetId(HealthManager hm, out string id)
        {
            if (hm == null) { id = ""; return false; }
            return _ids.TryGetValue(hm, out id!);
        }

        /// Returns false if the ID is unknown OR the GameObject was destroyed.
        public bool TryGetEnemy(string id, out HealthManager? hm)
        {
            if (!_enemies.TryGetValue(id, out var h) || h == null)
            {
                hm = null;
                return false;
            }
            hm = h;
            return true;
        }

        /// A snapshot copy — safe to iterate while the live dictionary changes.
        public IReadOnlyList<KeyValuePair<string, HealthManager>> Snapshot() =>
            _enemies.Where(kv => kv.Value != null).ToList();

        // ── Removal ──────────────────────────────────────────────────────────

        public void Remove(string id)
        {
            if (_enemies.TryGetValue(id, out var hm) && hm != null)
                _ids.Remove(hm);
            _enemies.Remove(id);
        }

        public void Remove(HealthManager hm)
        {
            if (hm == null) return;
            if (_ids.TryGetValue(hm, out var id))
                Remove(id);
        }

        // ── Private helpers ──────────────────────────────────────────────────

        private string AllocateId(HealthManager hm)
        {
            var pos   = hm.transform.position;
            var clean = hm.gameObject.name
                          .Replace("(Clone)", "")
                          .Trim()
                          .Replace(" ", "_");

            var baseId = $"{clean}_{pos.x:F1}_{pos.y:F1}";

            if (!_enemies.ContainsKey(baseId))
                return baseId;   // no collision — use as-is

            // Collision: bump the per-baseId counter
            _suffixes.TryGetValue(baseId, out var n);
            n++;
            _suffixes[baseId] = n;
            return $"{baseId}_{n}";
        }
    }
}