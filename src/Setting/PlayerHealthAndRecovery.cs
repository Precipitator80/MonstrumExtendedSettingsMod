namespace MonstrumExtendedSettingsMod.Setting
{
    class PlayerHealthAndRecovery : Setting
    {
        public static int maxHealth;
        public static int recoveryValue;

        protected override bool ShouldSettingBeEnabled()
        {
            return maxHealth != 100 || recoveryValue != 1;
        }

        protected override void OnEnable()
        {
            // Player Max Health & Recovery
            RegisterHook<On.PlayerHealth.hook_Start>(h => On.PlayerHealth.Start += h, h => On.PlayerHealth.Start -= h, HookPlayerHealthStart);
        }

        private static void HookPlayerHealthStart(On.PlayerHealth.orig_Start orig, PlayerHealth playerHealth)
        {
            playerHealth.MaxHP = maxHealth;
            playerHealth.recoveryValue = recoveryValue;
            orig.Invoke(playerHealth);
        }
    }
}
