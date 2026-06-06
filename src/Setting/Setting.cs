using System;
using System.Collections.Generic;

namespace MonstrumExtendedSettingsMod.Setting
{
    abstract class Setting
    {
        private bool enabled;
        public bool Enabled { get => enabled; }
        private readonly List<Action> _disableHooks = new List<Action>();

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

        protected void RegisterHook<T>(Action<T> add, Action<T> remove, T hook) where T : Delegate
        {
            add(hook);
            _disableHooks.Add(() => remove(hook));
        }

        private void OnDisable()
        {
            foreach (var h in _disableHooks)
                try { h(); } catch (InvalidOperationException) { }

            _disableHooks.Clear();
        }

        protected abstract bool ShouldSettingBeEnabled();
        protected virtual void OnEnable() { }
        public virtual void EarlyInitialisation() { }
        public virtual void LateInitialisation() { }
    }
}