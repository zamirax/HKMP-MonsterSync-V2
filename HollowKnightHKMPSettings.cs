namespace HollowKnightHKMPMod
{
    /// <summary>
    /// Settings for the Hollow Knight HKMP mod.
    /// </summary>
    public sealed class HollowKnightHKMPSettings
    {
        /// <summary>
        /// Whether to sync mob health from host to all players.
        /// </summary>
        public bool SyncMobHealth { get; set; } = true;

        /// <summary>
        /// Whether to give soul to all players when any player attacks a mob.
        /// </summary>
        public bool ShareSoulGain { get; set; } = true;

        /// <summary>
        /// Whether to sync boss health from host to all players.
        /// </summary>
        public bool SyncBossHealth { get; set; } = true;
    }
}