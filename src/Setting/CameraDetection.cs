using System.Collections.Generic;

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
            // Hurt or stun the players on detection. RedState is entered every time a camera spots a player, even if the alarm is already sounding.
            RegisterHook<On.RedState.hook_OnEnter>(h => On.RedState.OnEnter += h, h => On.RedState.OnEnter -= h, HookRedStateOnEnter);
        }

        private static void HookRedStateOnEnter(On.RedState.orig_OnEnter orig, RedState redState)
        {
            orig.Invoke(redState);
            foreach (NewPlayerClass player in SpottedPlayers(redState.securityCamera.detectPlayer))
            {
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

        /// <summary>
        /// Gets the players that a camera can see.
        /// The vanilla DetectPlayer only checks References.Player, so every crew player is checked in multiplayer.
        /// </summary>
        private static List<NewPlayerClass> SpottedPlayers(DetectPlayer detectPlayer)
        {
            if (!ExtendedSettingsModScript.ModSettings.enableMultiplayer)
            {
                return new List<NewPlayerClass> { References.PlayerClass };
            }
            return ExtendedSettingsModScript.MultiplayerMode.crewPlayers.FindAll(player => ExtendedSettingsModScript.MultiplayerMode.CameraCanSeePlayer(detectPlayer, player));
        }
    }
}
