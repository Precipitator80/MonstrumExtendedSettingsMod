namespace MonstrumExtendedSettingsMod.Setting
{
    class DisableWorkshopPhones : Setting
    {
        public static bool disableWorkshopPhones;

        protected override bool ShouldSettingBeEnabled()
        {
            return disableWorkshopPhones;
        }

        protected override void OnEnable()
        {
            // Mark every phone as destroyed so it cannot be called
            RegisterHook<On.Phone.hook_Start>(h => On.Phone.Start += h, h => On.Phone.Start -= h, HookPhoneStart);
        }

        private static void HookPhoneStart(On.Phone.orig_Start orig, Phone phone)
        {
            orig.Invoke(phone);
            phone.phoneDestroyed = true;
        }
    }
}
