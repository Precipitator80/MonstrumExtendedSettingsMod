using UnityEngine;

namespace MonstrumExtendedSettingsMod.Setting
{
    class CameraDetection : Setting
    {
        public static bool doCameraStun;
        public static float camDetectDMGValue;

        protected override bool ShouldSettingBeEnabled()
        {
            return doCameraStun || camDetectDMGValue > 0;
        }

        protected override void OnEnable()
        {
            // Hurt or stun the player on detection
            RegisterHook<On.AlarmManager.hook_StartPlayingSound>(h => On.AlarmManager.StartPlayingSound += h, h => On.AlarmManager.StartPlayingSound -= h, HookAlarmManagerStartPlayingSound);
        }

        private static void HookAlarmManagerStartPlayingSound(On.AlarmManager.orig_StartPlayingSound orig, AlarmManager alarmManager, Vector3 _audioTransform, Room _cameraRoom)
        {
            orig.Invoke(alarmManager, _audioTransform, _cameraRoom);
            NewPlayerClass player;
            if (ExtendedSettingsModScript.ModSettings.enableMultiplayer)
            {
                player = ExtendedSettingsModScript.MultiplayerMode.crewPlayers[ExtendedSettingsModScript.MultiplayerMode.ClosestPlayerToThis(_audioTransform, true)];
            }
            else
            {
                player = Object.FindObjectOfType<NewPlayerClass>();
            }

            if (camDetectDMGValue > 0)
            {
                player.playerMotor.pHealth.DoDamage(camDetectDMGValue, false, PlayerHealth.DamageTypes.Steam, false);
            }

            if (doCameraStun)
            {
                player.PushBackDir = 1f;
            }
        }
    }
}
