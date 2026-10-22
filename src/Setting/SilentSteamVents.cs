namespace MonstrumExtendedSettingsMod.Setting
{
    class SilentSteamVents : Setting
    {
        public static bool silentSteamVents;

        protected override bool ShouldSettingBeEnabled()
        {
            return silentSteamVents;
        }

        protected override void OnEnable()
        {
            // Mute the steam vents
            RegisterHook<On.SteamVents.hook_Start>(h => On.SteamVents.Start += h, h => On.SteamVents.Start -= h, HookSteamVentsStart);
        }

        private static void HookSteamVentsStart(On.SteamVents.orig_Start orig, SteamVents steamVents)
        {
            orig.Invoke(steamVents);
            steamVents.blastSource.mute = true;
            steamVents.ventSource.mute = true;
        }
    }
}
