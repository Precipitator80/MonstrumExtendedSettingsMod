using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonstrumExtendedSettingsMod.Setting
{
    static class SettingManager
    {
        private static readonly Dictionary<Type, Setting> settings = new Dictionary<Type, Setting>();

        public static T Register<T>(T setting) where T : Setting
        {
            var type = typeof(T);

            if (settings.ContainsKey(type))
            {
                // Already registered → just return existing instance
                Debug.LogError($"Setting of type {setting} was already registered!");
                return (T)settings[type];
            }

            settings[type] = setting;
            return setting;
        }

        public static void EarlyInitialisation()
        {
            foreach (var setting in settings.Values)
            {
                setting.SyncSettingState();
                if (setting.Enabled)
                    setting.EarlyInitialisation();
            }
        }

        public static void LateInitialisation()
        {
            foreach (var setting in settings.Values)
                if (setting.Enabled)
                    setting.LateInitialisation();
        }
    }
}