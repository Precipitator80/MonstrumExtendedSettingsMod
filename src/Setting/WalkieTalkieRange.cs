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
            On.WalkieTalkie.Awake += HookWalkieTalkieAwake;
        }

        protected override void OnDisable()
        {
            On.WalkieTalkie.Awake -= HookWalkieTalkieAwake;
        }

        private static void HookWalkieTalkieAwake(On.WalkieTalkie.orig_Awake orig, WalkieTalkie walkieTalkie)
        {
            orig.Invoke(walkieTalkie);
            // Run after orig so that the audio source distance is not affected.
            walkieTalkie.range = ExtendedSettingsModScript.ModSettings.walkieTalkieRange;
        }
    }
}