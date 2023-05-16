using System.IO;
using System.Text;
using UnityEngine;

namespace MonstrumExtendedSettingsMod
{
    public partial class ExtendedSettingsModScript
    {
        public static class ChallengeParser
        {
            private static readonly string SEPARATOR = " - ";
            private static readonly string REFERENCE_LINE = "Setting" + SEPARATOR + "Custom Value";

            public static void SaveChallenge(string challengeName)
            {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.Append("Challenge Name: ");
                stringBuilder.Append(challengeName);
                stringBuilder.Append("\nVersion: ");
                stringBuilder.Append(ExtendedSettingsModScript.VERSION_WITH_TEXT);
                stringBuilder.Append("\n");
                stringBuilder.Append(REFERENCE_LINE);
                foreach (MESMSetting mESMSetting in ModSettings.allSettings)
                {
                    if (!mESMSetting.userValueString.Equals(mESMSetting.defaultValueString))
                    {
                        stringBuilder.Append("\n");
                        stringBuilder.Append(mESMSetting.modSettingsText);
                        stringBuilder.Append(SEPARATOR);
                        stringBuilder.Append(mESMSetting.userValueString);
                    }
                }
                File.WriteAllText("Challenges/" + challengeName + ".txt", stringBuilder.ToString());
            }

            public static void ReadChallenge(string challengeName)
            {
                string[] challengeInformation = File.ReadAllLines("Challenges/" + challengeName + ".txt");
                int startLineNumber = -1;
                for (int lineNumber = 0; lineNumber < challengeInformation.Length; lineNumber++)
                {
                    if (challengeInformation[lineNumber].Equals(REFERENCE_LINE))
                    {
                        startLineNumber = lineNumber + 1;
                        break;
                    }
                }

                // Ensure the reference line was found.
                if (startLineNumber == -1)
                {
                    Debug.Log("Could not read challenge! Reference line was not found!");
                    return;
                }

                // Change the userValueString of each setting specified in the challenge file.
                for (int lineNumber = startLineNumber; lineNumber < challengeInformation.Length; lineNumber++)
                {
                    string[] settingNameAndValue = challengeInformation[lineNumber].Split(new string[] { SEPARATOR }, System.StringSplitOptions.None);
                    if (settingNameAndValue.Length == 2)
                    {
                        string settingName = settingNameAndValue[0];
                        string settingValue = settingNameAndValue[1];
                        foreach (MESMSetting mESMSetting in ModSettings.allSettings)
                        {
                            if (mESMSetting.modSettingsText.Equals(settingName))
                            {
                                mESMSetting.userValueString = settingValue;
                            }
                        }
                    }
                    else
                    {
                        Debug.Log("Could not read challenge! Challenge was not in correct format!");
                    }
                }

                // Update the modSettings string array with all user value strings.
                foreach (MESMSetting mESMSetting in ModSettings.allSettings)
                {
                    MESMSetting.modSettings[mESMSetting.modSettingsLine] = mESMSetting.userValueString;
                }

                // Rewrite and re-read the modSettings file. Maybe do this via menu buttons instead to make it consistent.
                File.WriteAllLines("modSettings.txt", MESMSetting.modSettings);
                ModSettings.ReadModSettings();
            }
        }
    }
}