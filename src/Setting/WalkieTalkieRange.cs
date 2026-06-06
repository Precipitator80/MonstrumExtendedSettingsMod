namespace MonstrumExtendedSettingsMod.Setting
{
    class WalkieTalkieRange : Setting
    {
        protected override bool ShouldSettingBeEnabled()
        {
            return ExtendedSettingsModScript.ModSettings.walkieTalkieRange != 20f;
        }

        protected override void OnEnable()
        {
            RegisterHook<On.WalkieTalkie.hook_Awake>(h => On.WalkieTalkie.Awake += h, h => On.WalkieTalkie.Awake -= h, HookWalkieTalkieAwake);
        }

        private static void HookWalkieTalkieAwake(On.WalkieTalkie.orig_Awake orig, WalkieTalkie walkieTalkie)
        {
            orig.Invoke(walkieTalkie);
            // Run after orig so that the audio source distance is not affected.
            walkieTalkie.range = ExtendedSettingsModScript.ModSettings.walkieTalkieRange;
        }
    }
}