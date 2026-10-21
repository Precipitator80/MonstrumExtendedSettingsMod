using UnityEngine;

namespace MonstrumExtendedSettingsMod.Setting
{
    class PlayerHealthAndRecovery : Setting
    {
        public static int maxHealth;
        public static float recoveryMultiplier;
        public static float recoveryDelay;

        /// <summary>
        /// PlayerHealth.DoDamage clamps health to this value regardless of PlayerHealth.MaxHP.
        /// </summary>
        private const float vanillaDamageClamp = 100f;
        /// <summary>
        /// The game recovers 1 health per frame, which is 60 health per second at 60 FPS.
        /// </summary>
        private const float vanillaRecoveryPerSecond = 60f;

        protected override bool ShouldSettingBeEnabled()
        {
            return maxHealth != 100 || recoveryMultiplier != 1f || recoveryDelay != 10f;
        }

        protected override void OnEnable()
        {
            // Player Max Health & Recovery
            RegisterHook<On.PlayerHealth.hook_Start>(h => On.PlayerHealth.Start += h, h => On.PlayerHealth.Start -= h, HookPlayerHealthStart);
            RegisterHook<On.PlayerHealth.hook_DoDamage>(h => On.PlayerHealth.DoDamage += h, h => On.PlayerHealth.DoDamage -= h, HookPlayerHealthDoDamage);
            RegisterHook<On.PlayerHealth.hook_RecoverHealth>(h => On.PlayerHealth.RecoverHealth += h, h => On.PlayerHealth.RecoverHealth -= h, HookPlayerHealthRecoverHealth);
        }

        private static void HookPlayerHealthStart(On.PlayerHealth.orig_Start orig, PlayerHealth playerHealth)
        {
            playerHealth.MaxHP = maxHealth;
            playerHealth.timeBeforerecovery = recoveryDelay;
            orig.Invoke(playerHealth);
        }

        /// <summary>
        /// Stops damage from knocking a player with more than 100 max health down to 100.
        /// While orig runs, health and damage are scaled by the same factor so that max health is 100, then health is scaled back.
        /// Every proportion stays the same, so other DoDamage hooks (invincibility, damage multipliers) work in any hook order.
        /// </summary>
        private static void HookPlayerHealthDoDamage(On.PlayerHealth.orig_DoDamage orig, PlayerHealth playerHealth, float damageVal, bool overTime, PlayerHealth.DamageTypes DMG, bool isAreaEffectDamage)
        {
            if (playerHealth.MaxHP <= vanillaDamageClamp)
            {
                orig.Invoke(playerHealth, damageVal, overTime, DMG, isAreaEffectDamage);
                return;
            }

            float scale = vanillaDamageClamp / playerHealth.MaxHP;
            int previousContinuousDPS = playerHealth.continuousDPS;
            playerHealth.continuousDPS = int.MinValue; // Marks whether orig starts continuous monster damage.
            playerHealth.currentHP *= scale;
            try
            {
                orig.Invoke(playerHealth, damageVal * scale, overTime, DMG, isAreaEffectDamage);
            }
            finally
            {
                playerHealth.currentHP /= scale;
                // UpdateDamage passes continuousDPS back into DoDamage every frame, where it is scaled again, so store it unscaled like the game would.
                playerHealth.continuousDPS = playerHealth.continuousDPS == int.MinValue ? previousContinuousDPS : (int)damageVal;
            }
        }

        /// <summary>
        /// The game rounds health up to a whole number every frame, so it recovers 1 health per frame regardless of frame rate.
        /// A custom multiplier instead recovers health smoothly at a rate based on 60 FPS.
        /// A negative multiplier drains health once the player has been damaged, as recovery only runs below max health.
        /// </summary>
        private static void HookPlayerHealthRecoverHealth(On.PlayerHealth.orig_RecoverHealth orig, PlayerHealth playerHealth)
        {
            if (recoveryMultiplier == 1f)
            {
                orig.Invoke(playerHealth);
                return;
            }
            if (recoveryMultiplier < 0f && IsInvincible(playerHealth))
            {
                return;
            }
            playerHealth.currentHP = Mathf.Clamp(playerHealth.currentHP + vanillaRecoveryPerSecond * recoveryMultiplier * Time.deltaTime, 0f, playerHealth.MaxHP);
        }

        /// <summary>
        /// Draining health does not go through DoDamage, so check invincibility the same way the DoDamage hook does.
        /// </summary>
        private static bool IsInvincible(PlayerHealth playerHealth)
        {
            int crewPlayerIndex = 0;
            if (ExtendedSettingsModScript.ModSettings.enableMultiplayer)
            {
                crewPlayerIndex = ExtendedSettingsModScript.MultiplayerMode.crewPlayers.IndexOf(playerHealth.NPC);
            }
            return ExtendedSettingsModScript.ModSettings.invincibilityMode[crewPlayerIndex];
        }
    }
}
