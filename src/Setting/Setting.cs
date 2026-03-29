namespace MonstrumExtendedSettingsMod.Setting
{
    abstract class Setting
    {
        private bool enabled;
        public bool Enabled { get => enabled; }
        public void SyncSettingState()
        {
            var shouldSettingBeEnabled = ShouldSettingBeEnabled();
            if (enabled == shouldSettingBeEnabled)
                return;

            enabled = shouldSettingBeEnabled;

            if (enabled)
                OnEnable();
            else
                OnDisable();
        }
        protected abstract bool ShouldSettingBeEnabled();
        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        public virtual void EarlyInitialisation() { }
        public virtual void LateInitialisation() { }
    }
}