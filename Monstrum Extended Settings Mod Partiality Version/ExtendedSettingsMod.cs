// ~Beginning Of File
using System.Security;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using Partiality.Modloader;
using UnityEngine;

[assembly: IgnoresAccessChecksTo("Assembly-CSharp")]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
[module: UnverifiableCode]

namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
    public class IgnoresAccessChecksToAttribute : Attribute
    {
        public IgnoresAccessChecksToAttribute(string assemblyName)
        {
            AssemblyName = assemblyName;
        }

        public string AssemblyName { get; }
    }
}

namespace MonstrumExtendedSettingsMod
{
    public class ExtendedSettingsMod : PartialityMod
    {
        public ExtendedSettingsMod()
        {
            this.ModID = "ExtendedSettingsMod";
            this.Version = "0410";
            this.author = "Precipitator";
        }

        private static ExtendedSettingsModScript extendedSettingsModScript;

        public override void OnEnable()
        {
            Debug.Log("EXTENDED SETTINGS MOD ACTIVATED");
            base.OnEnable();
            ExtendedSettingsModScript.mod = this;
            GameObject gameObject = new GameObject();
            extendedSettingsModScript = gameObject.AddComponent<ExtendedSettingsModScript>();
            extendedSettingsModScript.Initialise();
        }
    }

    public partial class ExtendedSettingsModScript : MonoBehaviour
    {
        public static ExtendedSettingsMod mod;

        public void Initialise()
        {
            Debug.Log("INITIALISING EXTENDED SETTINGS MOD");
            ModSettings.ReadModSettings();
            if (ModSettings.enableMod)
            {
                BaseFeatures.InitialiseBaseFeatures();
                if (ModSettings.numberOfMonsters > 1 || ModSettings.enableMultiplayer || ModSettings.forceManyMonstersMode || ModSettings.useSparky)
                {
                    ModSettings.startedWithMMM = true;
                    ManyMonstersMode.InitialiseManyMonstersMode();
                }
                if (ModSettings.enableMultiplayer)
                {
                    MultiplayerMode.InitialiseMultiplayerMode();
                    if (ModSettings.enableCrewVSMonsterMode)
                    {
                        CrewVsMonsterMode.InitialiseCrewVsMonsterMode();
                    }
                }
                if (ModSettings.useSparky)
                {
                    SparkyMode.InitialiseSparkyMode();
                }
                if (ModSettings.useSmokeMonster)
                {
                    SmokeMonster.InitialiseSmokeMonster();
                }
            }
            else
            {
                Debug.Log("MOD MANUALLY DISABLED IN SETTINGS");
            }
            Debug.Log("INITIALISED EXTENDED SETTINGS MOD");
        }

        /*----------------------------------------------------------------------------------------------------*/
    }
}
// ~End Of File