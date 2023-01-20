// ~Beginning Of File
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Text;

namespace MonstrumExtendedSettingsMod
{
    // # Additional ideas for the future: Help page in options with tips. Speedrun / challenge category with preset settings. Change Fiend aura stuff to multipliers

    // # Hiding together under bed means one player won't be able to get out. Life raft lerp pulls second player even though first player used button. Having two life rafts spawns too many new unique items and doesn't ready properly in escape checker and life raft debug sequence?

    /*
    # Things still to do:
    High priority:
    - Check notes left behind with hashtags.
    - Fix ship generic lights random colours bug [might have been fixed already]

    Low priority:
    - Space categories based on how many they are / let categories be put on multiple pages.
    - Make UI less blurry / anti-aliased.
    - Fix bug that certain buttons are highlighted before hoving over them once (reset to default in settings pages) and still highlighted after clicking through a page and exiting again.
    */

    public partial class ExtendedSettingsModScript
    {
        public abstract class MESMSetting
        {
            public static string[] modSettings;
            public static bool modSettingsExist;
            public static string currentCategoryBeingAssigned;

            public string modSettingsText;
            protected string modSettingsDescription;
            private string defaultValueString;
            public string userValueString;
            protected int modSettingsLine;
            public string category;
            //private int linesToResult;
            private bool childSetting;
            protected GameObject[] settingsButton;

            public MESMSetting(string modSettingsText, string modSettingsDescription, bool childSetting, string defaultValueString)
            {
                this.modSettingsText = modSettingsText;//modSettingsText.Split(new string[] { " -" }, System.StringSplitOptions.None)[0];
                //Debug.Log("Processing setting: " + this.modSettingsText);
                this.defaultValueString = defaultValueString;
                this.modSettingsDescription = modSettingsDescription + ".\nDefault = " + defaultValueString;
                int lineOfSettingText = (modSettingsExist ? FindLineOfSettingText(this.modSettingsText) : 0);
                this.childSetting = childSetting;
                this.modSettingsLine = lineOfSettingText + 1;
                this.category = currentCategoryBeingAssigned;
                ModSettings.allSettings.Add(this);
            }

            protected static int FindLineOfSettingText(string searchText)
            {
                // For each line of the settings string array, look for a snippet of text describing a setting. If the relevant line is found, skip ahead a given number of lines (default = 1) to find the corresponding setting in its raw format. This means the search can be automated instead of requiring defined numbers. While this may be a bit slower than the previous implementation, it should also make the settings file less prone to incorrect formatting.
                for (int line = 0; line < modSettings.Length; line++)
                {
                    if (modSettings[line].Contains(searchText + " -"))
                    {
                        return line;
                    }
                }
                Debug.Log("Could not find setting for " + searchText + " -");
                throw new ArgumentException();
            }

            public static string FindSetting(string searchText, int linesToResult = 1)
            {
                string result = modSettings[FindLineOfSettingText(searchText) + linesToResult];
                //Debug.Log("Returning " + result);
                return result;
            }

            public static void SaveSettings()
            {
                Debug.Log("Saving settings");
                StringBuilder modSettingsStringBuilder = new StringBuilder("Monstrum Extended Settings Mod Settings File:\nThe first line of each setting will tell you what the setting is and what its default value is, while any following lines are used by the game. Editing the default value shown does nothing. The default settings emulate an unmodded game. Creating or deleting lines will break the mod. If the game stops reading your settings correctly, download the mod again to get a clean settings file.");
                string category = string.Empty;
                int newNumberOfMonsters = 0;
                bool changedSettingThatRequiresRestart = false;
                List<string> restartSettingsChanged = new List<string>();
                List<string> badlyFormattedSettings = new List<string>();
                StringBuilder settingsLogger = new StringBuilder("Saved all settings:\t\t ");
                foreach (MESMSetting mESMSetting in ModSettings.allSettings)
                {
                    try
                    {
                        string value = mESMSetting.defaultValueString;
                        if (modSettingsExist)
                        {
                            Type t = mESMSetting.GetType();
                            if (t == typeof(MESMSettingString) || t == typeof(MESMSettingMultipleChoice))
                            {
                                // Update the variable and then the text file
                                try
                                {
                                    if (t == typeof(MESMSettingMultipleChoice))
                                    {
                                        value = mESMSetting.settingsButton[3].GetComponent<Text>().text;
                                    }
                                    else
                                    {
                                        value = mESMSetting.settingsButton[10].GetComponent<InputField>().text;
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.Log("Failed to process MESMSettingString");
                                    throw new Exception(e.ToString());
                                }
                                ((MESMSettingString)mESMSetting).userValue = value;
                            }
                            else if (t == typeof(MESMSettingRGB))
                            {
                                value = mESMSetting.settingsButton[10].GetComponent<InputField>().text;
                                ((MESMSettingRGB)mESMSetting).userValue = Convert.ToInt32(value);
                            }
                            else
                            {
                                try
                                {
                                    Type gt = mESMSetting.GetType().GetGenericArguments()[0]; // How to get the type of T from a member of a generic class or method - Tamas Czinege - https://stackoverflow.com/questions/557340/how-to-get-the-type-of-t-from-a-member-of-a-generic-class-or-method - Accessed 26.10.2021
                                    if (gt == typeof(int) || gt == typeof(float))
                                    {
                                        value = mESMSetting.settingsButton[10].GetComponent<InputField>().text;
                                        if (gt == typeof(int))
                                        {
                                            ((MESMSetting<int>)mESMSetting).userValue = Convert.ToInt32(value);
                                            if (mESMSetting.modSettingsText.Equals("Number of Random Monsters") || mESMSetting.modSettingsText.Equals("Number of Brutes") || mESMSetting.modSettingsText.Equals("Number of Hunters") || mESMSetting.modSettingsText.Equals("Number of Fiends") || mESMSetting.modSettingsText.Equals("Number of Sparkies"))
                                            {
                                                newNumberOfMonsters += ((MESMSetting<int>)mESMSetting).userValue;
                                            }
                                        }
                                        else
                                        {
                                            ((MESMSetting<float>)mESMSetting).userValue = Convert.ToSingle(value);
                                        }
                                    }
                                    else if (gt == typeof(bool))
                                    {
                                        value = mESMSetting.settingsButton[1].GetComponent<Text>().text.Equals("X") ? "True" : "False";
                                        ((MESMSetting<bool>)mESMSetting).userValue = Convert.ToBoolean(value);
                                    }
                                    else
                                    {
                                        Debug.Log("Cast for " + gt + " is not defined!");
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.Log("Failed to process MESMSetting<T>");
                                    throw new Exception(e.ToString());
                                }
                            }
                            //Debug.Log("Writing to line " + mESMSetting.modSettingsLine + ": " + value);
                            try
                            {
                                modSettings[mESMSetting.modSettingsLine] = value;
                            }
                            catch (Exception e)
                            {
                                Debug.Log("Failed to write to line " + mESMSetting.modSettingsLine + ": " + value);
                                throw new Exception(e.ToString());
                            }
                            //Debug.Log("Wrote to line " + mESMSetting.modSettingsLine + ": " + value);
                            if ((mESMSetting.modSettingsDescription.Contains("Restart") || mESMSetting.modSettingsDescription.Contains("restart")) && mESMSetting.userValueString != value)
                            {
                                changedSettingThatRequiresRestart = true;
                                restartSettingsChanged.Add(mESMSetting.modSettingsText);
                            }
                            mESMSetting.userValueString = value;
                            settingsLogger.Append(mESMSetting.modSettingsText);
                            settingsLogger.Append(" = ");
                            settingsLogger.Append(mESMSetting.userValueString);
                            settingsLogger.Append("\t| ");
                        }
                        else
                        {
                            if (!mESMSetting.category.Equals(category))
                            {
                                modSettingsStringBuilder.Append("\n----------------------------------------------------------------------------------------------------\n");
                                modSettingsStringBuilder.Append(mESMSetting.category);
                                modSettingsStringBuilder.Append(":");
                                category = mESMSetting.category;
                            }
                            modSettingsStringBuilder.Append("\n");
                            if (!mESMSetting.childSetting)
                            {
                                modSettingsStringBuilder.Append("------------------------------\n");
                            }
                            modSettingsStringBuilder.Append(mESMSetting.modSettingsText);
                            modSettingsStringBuilder.Append(" - ");
                            modSettingsStringBuilder.Append(mESMSetting.modSettingsDescription.Replace("\n", " - "));
                            modSettingsStringBuilder.Append("\n");
                            modSettingsStringBuilder.Append(value);
                        }
                    }
                    catch (Exception e)
                    {
                        if (mESMSetting != null && mESMSetting.modSettingsText != null)
                        {
                            Debug.Log("Error while saving setting: " + mESMSetting.modSettingsText + "\n" + e.ToString());
                            badlyFormattedSettings.Add(mESMSetting.modSettingsText);
                        }
                        else
                        {
                            Debug.Log("MESM setting was null while trying to report bad format error\n" + e.ToString());
                        }
                    }
                }
                if (modSettingsExist)
                {
                    StringBuilder warningBoxTextStringBuilder = new StringBuilder();
                    if (badlyFormattedSettings.Count > 0)
                    {
                        warningBoxTextStringBuilder.Append("Error while reading setting(s):\n----------");
                        foreach (string badlyFormattedSetting in badlyFormattedSettings)
                        {
                            warningBoxTextStringBuilder.Append("\n");
                            warningBoxTextStringBuilder.Append(badlyFormattedSetting);
                        }
                        warningBoxTextStringBuilder.Append("\n----------\nSettings have not been saved. Restart if unable to fix.\n\n");
                        warningBoxGOs[0].SetActive(true);
                        warningBoxGOs[2].SetActive(true);
                    }
                    else
                    {
                        File.WriteAllLines("modSettings.txt", modSettings);
                        string settingsLog = settingsLogger.ToString();
                        Debug.Log(settingsLog.Substring(0, settingsLog.Length - 3));
                    }
                    if (!ModSettings.startedWithMMM && newNumberOfMonsters > 1)
                    {
                        changedSettingThatRequiresRestart = true;
                        restartSettingsChanged.Add("Many Monsters Mode (From 0 or 1 to 2 or more monsters)");
                    }
                    else if (ModSettings.startedWithMMM && newNumberOfMonsters == 0)
                    {
                        changedSettingThatRequiresRestart = true;
                        restartSettingsChanged.Add("Many Monsters Mode (From 2 or more to 0 monsters)");
                    }
                    if (changedSettingThatRequiresRestart)
                    {
                        /*
                        foreach (GameObject gameObject in mesmButtonGOs)
                        {
                            gameObject.SetActive(false);
                        }
                        //mesmButtonGOs[0].SetActive(false);
                        */
                        if (restartSettingsChanged.Count > 1)
                        {
                            warningBoxTextStringBuilder.Append("Settings that require a restart have been changed:");
                        }
                        else
                        {
                            warningBoxTextStringBuilder.Append("A setting that requires a restart has been changed:");
                        }
                        warningBoxTextStringBuilder.Append("\n----------");
                        foreach (string restartSettingChanged in restartSettingsChanged)
                        {
                            warningBoxTextStringBuilder.Append("\n");
                            warningBoxTextStringBuilder.Append(restartSettingChanged);
                        }
                        warningBoxTextStringBuilder.Append("\n----------\nPlease restart the game unless otherwise specified in the setting");
                        if (restartSettingsChanged.Count > 1)
                        {
                            warningBoxTextStringBuilder.Append("s");
                        }
                        warningBoxTextStringBuilder.Append(".");

                        warningBoxGOs[0].SetActive(true);
                        warningBoxGOs[2].SetActive(true);
                    }
                    warningBoxTextStringBuilder.Append("\n\nOK");
                    warningBoxText.text = warningBoxTextStringBuilder.ToString();
                }
                else
                {
                    modSettingsStringBuilder.Append("\n----------------------------------------------------------------------------------------------------");
                    File.WriteAllLines("modSettings.txt", modSettingsStringBuilder.ToString().Split(new string[] { "\n" }, System.StringSplitOptions.None));
                }
                Debug.Log("Saved settings successfully");
            }

            public static void ResetSettingsToDefault(List<MESMSetting> mESMSettings)
            {
                foreach (MESMSetting mESMSetting in mESMSettings)
                {
                    //Debug.Log("Resetting " + mESMSetting.modSettingsText + " to default");
                    Type t = mESMSetting.GetType();
                    if (t == typeof(MESMSettingString) || t == typeof(MESMSettingMultipleChoice))
                    {
                        try
                        {
                            if (t == typeof(MESMSettingMultipleChoice))
                            {
                                mESMSetting.settingsButton[3].GetComponent<Text>().text = mESMSetting.defaultValueString;
                            }
                            else
                            {
                                mESMSetting.settingsButton[10].GetComponent<InputField>().text = mESMSetting.defaultValueString;
                            }
                        }
                        catch
                        {
                            Debug.Log("Failed to reset MESMSettingString to default");
                        }
                    }
                    else if (t == typeof(MESMSettingRGB))
                    {
                        mESMSetting.settingsButton[10].GetComponent<InputField>().text = mESMSetting.defaultValueString;
                        ((MESMSettingRGB)mESMSetting).ChangeTextColour();
                        mESMSetting.settingsButton[12].GetComponent<Slider>().value = Convert.ToSingle(mESMSetting.defaultValueString);
                    }
                    else
                    {
                        try
                        {
                            Type gt = mESMSetting.GetType().GetGenericArguments()[0]; // How to get the type of T from a member of a generic class or method - Tamas Czinege - https://stackoverflow.com/questions/557340/how-to-get-the-type-of-t-from-a-member-of-a-generic-class-or-method - Accessed 26.10.2021
                            if (gt == typeof(int) || gt == typeof(float))
                            {
                                mESMSetting.settingsButton[10].GetComponent<InputField>().text = mESMSetting.defaultValueString;
                                if (mESMSetting.settingsButton.Length > 12)
                                {
                                    mESMSetting.settingsButton[12].GetComponent<Slider>().value = Convert.ToSingle(mESMSetting.defaultValueString);
                                }
                            }
                            else if (gt == typeof(bool))
                            {
                                mESMSetting.settingsButton[1].GetComponent<Text>().text = ((MESMSetting<bool>)mESMSetting).defaultValue == true ? "X" : "";
                            }
                            else
                            {
                                Debug.Log("Cast for " + gt + " is not defined in reset to default!");
                            }
                        }
                        catch
                        {
                            Debug.Log("Failed to reset MESMSetting<T> to default");
                        }
                    }
                }
            }

            public abstract GameObject[] CreateButtonForSetting(Transform referenceTransform, Vector3 referenceOffset);
        }

        public class MESMSettingString : MESMSetting
        {
            public MESMSettingString(string modSettingsText, string modSettingsDescription, string defaultValue, bool absoluteValue = false, bool childSetting = false) : base(modSettingsText, modSettingsDescription, childSetting, defaultValue)
            {
                this.defaultValue = defaultValue;
                this.userValue = modSettingsExist ? modSettings[this.modSettingsLine] : defaultValue;
                this.userValueString = this.userValue.ToString();
            }

            public override GameObject[] CreateButtonForSetting(Transform referenceTransform, Vector3 referenceOffset)
            {
                settingsButton = MonstrumExtendedSettingsMod.ExtendedSettingsModScript.CreateInputFieldDescriptionBox(this.modSettingsDescription, this.modSettingsText, referenceTransform, referenceOffset, false, 0f, 0f);
                InputField inputField = settingsButton[10].GetComponent<InputField>();
                inputField.text = userValue.ToString();
                return settingsButton;
            }

            public string defaultValue;
            public string userValue;
        }

        public class MESMSettingMultipleChoice : MESMSettingString
        {
            private string[] choices;

            public MESMSettingMultipleChoice(string modSettingsText, string modSettingsDescription, string defaultValue, string[] choices, bool absoluteValue = false, bool childSetting = false) : base(modSettingsText, modSettingsDescription, defaultValue, absoluteValue, childSetting)
            {
                this.choices = choices;
            }

            public override GameObject[] CreateButtonForSetting(Transform referenceTransform, Vector3 referenceOffset)
            {
                settingsButton = MonstrumExtendedSettingsMod.ExtendedSettingsModScript.CreateMultipleChoiceDescriptionBox(this.modSettingsDescription, this.modSettingsText, choices, referenceTransform, referenceOffset);
                Text text = settingsButton[3].GetComponent<Text>();
                text.text = userValue.ToString();
                return settingsButton;
            }
        }

        // C# - Multiple generic types in one list - leppie - https://stackoverflow.com/questions/353126/c-sharp-multiple-generic-types-in-one-list - Accessed 26.10.2021
        public class MESMSetting<T> : MESMSetting where T : struct
        {
            public MESMSetting(string modSettingsText, string modSettingsDescription, T defaultValue, bool absoluteValue = false, bool childSetting = false, float lowerClamp = float.MinValue, float upperClamp = float.MaxValue) : base(modSettingsText, modSettingsDescription, childSetting, defaultValue.ToString())
            {
                this.defaultValue = defaultValue;
                this.userValue = (modSettingsExist ? FindSetting(this.modSettingsLine) : defaultValue);
                this.minClamp = lowerClamp == float.MinValue ? 0f : lowerClamp;
                this.maxClamp = upperClamp == float.MaxValue ? 0f : upperClamp;

                if (absoluteValue)
                {
                    if (typeof(T) == typeof(int))
                    {
                        this.userValue = (T)Convert.ChangeType(Math.Abs((int)Convert.ChangeType(this.userValue, typeof(int))), typeof(T));
                        this.defaultValue = (T)Convert.ChangeType(Math.Abs((int)Convert.ChangeType(this.defaultValue, typeof(int))), typeof(T));
                    }
                    else if (typeof(T) == typeof(float))
                    {
                        this.userValue = (T)Convert.ChangeType(Mathf.Abs((float)Convert.ChangeType(this.userValue, typeof(float))), typeof(T));
                        this.defaultValue = (T)Convert.ChangeType(Mathf.Abs((float)Convert.ChangeType(this.defaultValue, typeof(float))), typeof(T));
                    }
                    else throw new ArgumentException();
                }
                if (lowerClamp != float.MinValue || upperClamp != float.MaxValue)
                {
                    if (typeof(T) == typeof(int) || typeof(T) == typeof(float))
                    {
                        this.userValue = (T)Convert.ChangeType(Mathf.Clamp((float)Convert.ChangeType(this.userValue, typeof(float)), lowerClamp, upperClamp), typeof(T));
                        this.defaultValue = (T)Convert.ChangeType(Mathf.Clamp((float)Convert.ChangeType(this.defaultValue, typeof(float)), lowerClamp, upperClamp), typeof(T));
                    }
                    else throw new ArgumentException();
                }

                this.userValueString = this.userValue.ToString();
            }

            public T defaultValue;
            public T userValue;
            private float minClamp;
            private float maxClamp;

            // @FindSetting1
            private static T FindSetting(int lineOfSetting)
            {
                string result = modSettings[lineOfSetting];
                //Debug.Log("Returning " + result);
                // How do I make the return type of a method generic? - Jon Skeet - https://stackoverflow.com/questions/9808035/how-do-i-make-the-return-type-of-a-method-generic - Accessed 26.10.2021
                return (T)Convert.ChangeType(modSettings[lineOfSetting], typeof(T));
            }

            public static new T FindSetting(string searchText, int linesToResult = 1)
            {
                return FindSetting(FindLineOfSettingText(searchText) + linesToResult);
            }

            public override GameObject[] CreateButtonForSetting(Transform referenceTransform, Vector3 referenceOffset)
            {
                if (typeof(T) == typeof(int) || typeof(T) == typeof(float))
                {
                    if (this.minClamp != 0f || this.maxClamp != 0f)
                    {
                        settingsButton = MonstrumExtendedSettingsMod.ExtendedSettingsModScript.CreateSliderDescriptionBox(this.modSettingsDescription, this.modSettingsText, referenceTransform, referenceOffset, typeof(T) == typeof(int) ? true : false, this.minClamp, this.maxClamp);
                        settingsButton[12].GetComponent<Slider>().value = Convert.ToSingle(userValue);
                    }
                    else
                    {
                        settingsButton = MonstrumExtendedSettingsMod.ExtendedSettingsModScript.CreateInputFieldDescriptionBox(this.modSettingsDescription, this.modSettingsText, referenceTransform, referenceOffset, typeof(T) == typeof(int) ? true : false, this.minClamp, this.maxClamp);
                    }
                    InputField inputField = settingsButton[10].GetComponent<InputField>();
                    inputField.text = userValue.ToString();
                }
                else /*if (typeof(T) == typeof(bool))*/
                {
                    settingsButton = MonstrumExtendedSettingsMod.ExtendedSettingsModScript.CreateBoolDescriptionBox(this.modSettingsDescription, this.modSettingsText, referenceTransform, referenceOffset);
                    Text text = settingsButton[1].GetComponent<Text>();
                    if ((bool)Convert.ChangeType(userValue, typeof(bool)))
                    {
                        text.text = "X";
                    }
                    else
                    {
                        text.text = string.Empty;
                    }
                }
                return settingsButton;
            }
        }

        /*----------------------------------------------------------------------------------------------------*/
        // ~ModSettings

        private static class ModSettings
        {
            public static List<MESMSetting> allSettings;

            /*
            ((Base Class)Instance Variable) is used to mark where base is used in the original code.
            GetComponent might also have to be converted to GetComponentInParent.
            For example, base.GetComponent<Monster>() in MChasingState, a subclass/child of MState, this would be marked as ((MState)mChasingState).GetComponentInParent<Monster>()

            In Many Monsters Mode the code and comments may sometimes refer to lists as arrays. This is because originally almost exclusively arrays were used. In Java you cannot handle lists like arrays [indexing], but in C-Sharp / Unity you can. https://stackoverflow.com/questions/466946/how-to-initialize-a-listt-to-a-given-size-as-opposed-to-capacity - 05.01.2021

            You can insert a hook snippet in Visual Studio Code with Ctrl + Shift + I if the snippet has that keyboard shortcut assigned.

            Mod made by Precipitator with help of those credited fully in other documents (Naiden, Nöööls and Bee).
            */

            // #ReadModSettings
            public static void ReadModSettings()
            {
                try
                {
                    // Some of the settings require the MESMSetting to be declared separately so that validation can be performed. Is this really necessary? .userValue is not used for anything else once a round has started loading.
                    Debug.Log("READING EXTENDED SETTINGS FROM FILE [" + version + "]");
                    allSettings = new List<MESMSetting>();
                    try
                    {
                        MESMSetting.modSettings = File.ReadAllLines("modSettings.txt");
                        Debug.Log("Mod settings read. They are " + MESMSetting.modSettings.Length + " lines long.");
                        MESMSetting.modSettingsExist = true;
                    }
                    catch
                    {
                        Debug.Log("Mod settings not found. Creating them.");
                        MESMSetting.modSettingsExist = false; // This causes all MESMSettings to use the default value instead of attempting to find a value from the modSettings.txt file. This allows modSettings.txt to be created using the default values.
                    }
                    // Read Monster Settings Variables
                    modSettingsErrorString = "Monster";
                    MESMSetting.currentCategoryBeingAssigned = modSettingsErrorString + " Settings";

                    numberOfRandomMonsters = new MESMSetting<int>("Number of Random Monsters", "The number of randomly chosen monsters to spawn. These randomly chosen monsters are spawned additionally alongside monsters to spawn specified by type", 1, true).userValue;
                    numberOfBrutes = new MESMSetting<int>("Number of Brutes", "The number of Brutes to spawn", 0, true, true).userValue;
                    numberOfHunters = new MESMSetting<int>("Number of Hunters", "The number of Hunters to spawn", 0, true, true).userValue;
                    numberOfFiends = new MESMSetting<int>("Number of Fiends", "The number of Fiends to spawn", 0, true, true).userValue;
                    MESMSetting<int> numberOfSparkiesMESM = new MESMSetting<int>("Number of Sparkies", "The number of Sparkies to spawn. Requires the Add Sparky To The Game setting to be enabled", 0, true, true); // Create a MESMSetting first to allow for value verification.
                    //MESMSetting<int> numberOfSmokeMonstersMESM = new MESMSetting<int>("Number of Smoke Monsters", "The number of Smoke Monsters to spawn. Requires the Add Smoke Monster To The Game setting to be enabled", 0, true, true); // Create a MESMSetting first to allow for value verification.
                    // Use either of the below, not both.
                    useSparky = new MESMSetting<bool>("Add Sparky To The Game", "Adds an interpretation of the pre-alpha monster called Sparky to the game as a possible monster. Requires a restart to change", false).userValue; //AssignTrueBoolOnlyWhenFirstTimeReadingSettings(ref useSparky, Convert.ToBoolean(FindSetting("Add Sparky To The Game"))); // Not using this anymore
                    if (!useSparky)
                    {
                        numberOfSparkiesMESM.userValue = 0; // Force the number of Sparkies to be 0 if useSparky is not true.
                    }
                    numberOfSparkies = numberOfSparkiesMESM.userValue;
                    useSmokeMonster = false; //new MESMSetting<bool>("Add Smoke Monster To The Game", "Adds a Fiend-like monster to the game that prefers using mysterious smoke to take out the player to direct confrontation. Requires a restart to change", false).userValue;
                    /*
                    if (!useSmokeMonster)
                    {
                        numberOfSmokeMonstersMESM.userValue = 0;
                    }
                    */
                    numberOfSmokeMonsters = 0;//numberOfSmokeMonstersMESM.userValue;
                    numberOfMonsters = numberOfRandomMonsters + numberOfBrutes + numberOfHunters + numberOfFiends + numberOfSparkies;
                    disableRandomBrute = new MESMSetting<bool>("Disable Random Brute", "Stops Brute from being selected as a random monster", false).userValue;
                    disableRandomHunter = new MESMSetting<bool>("Disable Random Hunter", "Stops Hunter from being selected as a random monster", false, false, true).userValue;
                    disableRandomFiend = new MESMSetting<bool>("Disable Random Fiend", "Stops Fiend from being selected as a random monster", false, false, true).userValue;
                    bannedRandomMonsters = new List<string>();
                    if (disableRandomBrute)
                    {
                        bannedRandomMonsters.Add("Brute");
                    }
                    if (disableRandomHunter)
                    {
                        bannedRandomMonsters.Add("Hunter");
                    }
                    if (disableRandomFiend)
                    {
                        bannedRandomMonsters.Add("Fiend");
                    }
                    MESMSetting<bool> forceManyMonstersModeMESM = new MESMSetting<bool>("Force Many Monsters Mode", "Forces the game to start with Many Monsters Mode active. This lets features be used that randomise or add to monster counts without starting with 2 monsters. Requires a restart to change", false);
                    if (numberOfMonsters <= 0)
                    {
                        forceManyMonstersModeMESM.userValue = false;
                    }
                    forceManyMonstersMode = forceManyMonstersModeMESM.userValue;
                    minSpawnTime = new MESMSetting<float>("Minimum Monster Spawntime (seconds)", "A random number is chosen between this and the maximum value as the spawntime", 0, true).userValue;
                    maxSpawnTime = new MESMSetting<float>("Maximum Monster Spawntime (seconds)", "A random number is chosen between this and the minimum value as the spawntime", 0, true, true).userValue;
                    spawnMonsterInStarterRoom = new MESMSetting<bool>("Spawn Monster In Starter Room", "Any monster will spawn at the door of the starter room", false).userValue;
                    noMonsterStunImmunity = new MESMSetting<bool>("No Monster Stun Immunity", "Removes the immunity time monsters have after being stunned", false).userValue;
                    persistentMonster = new MESMSetting<bool>("Persistent Monster", "Once a monster sees you, it will not stop chasing you", false).userValue;
                    seerMonster = new MESMSetting<bool>("Seer Monster", "After a monster spawns or you respawn, it will start chasing you", false).userValue;
                    monsterAlwaysFindsYou = new MESMSetting<bool>("Monster Always Finds You In Hiding Spot", "Only guarantees the monster will pull you out if it checks your hiding spot", false).userValue;
                    monsterAnimationSpeedMultiplier = new MESMSetting<float>("Monster Animation Speed Multiplier", "Changes the speed of all the animations of a monster. Movement is bound to animations", 1).userValue;
                    monsterMovementSpeedMultiplier = new MESMSetting<float>("Monster Movement Speed Multiplier", "Decreases a monster's speed without decreasing the speed of its animations. Values above 1 will make the monster move closer to full running speed during other states, such as while wandering about or at the sub doors", 1).userValue;
                    monsterRotationSpeedMultiplier = new MESMSetting<float>("Monster Rotation Speed Multiplier", "Changes the rotation speed of a monster", 1f, true).userValue;
                    varyingMonsterSizes = new MESMSetting<bool>("Varying Monster Sizes", "Slightly changes the scale of each monster", false).userValue;
                    customMonsterScale = new MESMSetting<float>("Custom Monster Scale", "Change the scale of a monster. Hitbox and speed scale alongside this. Making a monster too small or too big can impede its movement", 1).userValue;
                    bruteChaseSpeedBuff = new MESMSetting<bool>("Brute Chase Speed Buff", "Lets the Brute gradually go beyond its normal maximum speed when running at its highest speed during a chase, which lets the Brute be faster when running along straight paths", false).userValue;
                    bruteChaseSpeedBuffMultiplier = new MESMSetting<float>("Brute Chase Speed Buff Multiplier", "Defines how much faster than the normal chasing speed the Brute can get", 1.35f, false, true).userValue;
                    applyChaseSpeedBuffToAllMonsters = new MESMSetting<bool>("Apply Chase Speed Buff To All Monsters", "Applies the chase speed buff to all monsters, not just the Brute", false, false, true).userValue;
                    overpoweredHunter = new MESMSetting<bool>("Overpowered Hunter", "Hunter always sets up traps when they have a chance to", false).userValue;
                    aggressiveHunter = new MESMSetting<bool>("Aggressive Hunter", "Hunter always comes out immediately after setting up traps", false, false, true).userValue;
                    fiendFlickerMin = new MESMSetting<float>("Fiend Flicker Minimum Range", "Changes the radius of Fiend's aura in which lights are affected the most. The game uses 3 by default", 0).userValue;
                    fiendFlickerMed = new MESMSetting<float>("Fiend Flicker Medium Range", "Changes the radius of Fiend's aura in which lights are affected in-between the minimum and maximum effect. The game uses 4.5 by default", 0, false, true).userValue;
                    fiendFlickerMax = new MESMSetting<float>("Fiend Flicker Maximum Range", "Changes the radius of Fiend's aura in which lights are affected the least. The game uses 6 by default", 0, false, true).userValue;
                    fiendDoorTeleportation = new MESMSetting<bool>("Fiend Door Teleportation", "Lets the Fiend teleport through doors", false).userValue;
                    applyDoorTeleportationToAllMonsters = new MESMSetting<bool>("Apply Door Teleportation To All Monsters", "Lets all monsters teleport through doors, not just the Fiend", false, false, true).userValue;
                    //letAllMonstersLockDoors = new MESMSetting<bool>("Let All Monsters Lock Doors", "Lets all monsters lock doors when chasing the player like the Fiend does", false).userValue; // Didn't work because door smoke requires a Fiend to be loaded.
                    giveAllMonstersAFiendAura = new MESMSetting<bool>("Give All Monsters A Fiend Aura", "Gives all monsters a Fiend Aura to disrupt lights like the Fiend does", false).userValue;
                    monstersSearchRandomly = new MESMSetting<bool>("Monsters Search Randomly", "Monsters search the ship randomly instead of always going near the player. Alerting the monster will still cause it to go to your position", false).userValue;
                    alternatingMonstersMode = new MESMSetting<bool>("Alternating Monsters Mode", "A variation of Many Monsters Mode. Instead of having all monsters active on the map at once, this setting will switch the monsters in and out throughout the round up to a limit. Requires Many Monsters Mode to be active", false).userValue;
                    numberOfAlternatingMonsters = new MESMSetting<int>("Number Of Alternating Monsters", "The number of monsters to have active in Alternating Monsters Mode at one time", 1, true, true).userValue;
                    giveAllMonstersASmokeShroud = new MESMSetting<bool>("Give All Monsters A Smoke Shroud", "Gives all monsters a smoke shroud that can kill the player when they stay too close to the monster", false).userValue;
                    smokeShroudRadius = new MESMSetting<float>("Smoke Shroud Radius", "Sets the radius of smoke shrouds", 8f, false, true).userValue;
                    smokeShroudDangerRadiusFactor = new MESMSetting<float>("Smoke Shroud Radius Danger Factor", "Sets what factor of the smoke shroud radius will be dangerous to the player. For example, 0.75 means three quarters of the radius is dangerous to the player", 0.75f, false, true).userValue;
                    giveAllMonstersAFireShroud = new MESMSetting<bool>("Give All Monsters A Fire Shroud", "Gives all monsters a fire shroud that keeps fire around the monster and lets it perform a fire blast", false).userValue;
                    fireShroudRadius = new MESMSetting<float>("Fire Shroud Radius", "Sets the radius of fire shrouds", 2f, false, true).userValue;
                    fireBlastRadius = new MESMSetting<float>("Fire Blast Radius", "Sets the radius of fire blasts", 16f, false, true).userValue;

                    sparkyDifficultyPreset = new MESMSettingMultipleChoice("Sparky Difficulty Preset", "Allows for various statistics presets to be used for Sparky. Can be User, Easy, Medium or Hard. Medium uses the default statistics", "User", new string[] { "User", "Easy", "Medium", "Hard" }).userValue;
                    MESMSetting<float> sparkyChaseFactorMESM = new MESMSetting<float>("Sparky Chase Speed Factor", "Sets the increase to the speed multiplier Sparky gets in chases. For example, 0.5 means Sparky will be 50% faster in chases than compared to the Brute", 0.5f, false, false);
                    MESMSetting<float> sparkyMaxChaseFactorIncreaseFromBuffMESM = new MESMSetting<float>("Sparky Max Chase Speed Factor Increase From Buff", "Sets the maximum increase to the Sparky chase speed factor Sparky gets from the Sparky Aura buff", 0.25f, false, true);
                    MESMSetting<float> sparkyMaxSpeedFactorIncreaseFromBuffMESM = new MESMSetting<float>("Sparky Max Movement Speed Factor Increase From Buff", "Sets the maximum increase to the speed multiplier Sparky gets when not in a chase", 0.35f, false, true);
                    MESMSetting<float> sparkyRotationSpeedFactorMESM = new MESMSetting<float>("Sparky Rotation Speed Factor", "Sets the change to Sparky's rotation speed in all states except their lurking state", -0.5f, false, true);
                    MESMSetting<float> sparkyLurkMinimumDistanceToPlayerMESM = new MESMSetting<float>("Sparky Minimum Lurk Distance", "Sets the minimum range Sparky needs to be away from the player to start lurking them instead of chasing them directly. Each single corridor piece is 2 metres wide", 8f, true, false);
                    MESMSetting<float> sparkyLurkMaxSuperEMPDistanceMESM = new MESMSetting<float>("Sparky Maximum Super EMP Distance", "Sets the maximum range Sparky needs to be within to charge a super EMP when lurking the player", 16f, true, true);
                    MESMSetting<float> sparkyLurkSuperEMPChargeTimeToWaitMESM = new MESMSetting<float>("Sparky Super EMP Charge Time", "Sets the time Sparky needs to charge a super EMP", 2.25f, true, true);
                    MESMSetting<float> sparkyLurkMaxAggroMESM = new MESMSetting<float>("Sparky Maximum Aggro", "Sets the maximum time the player may look at Sparky during their lurking before Sparky will chase the player", 1.5f, true, true);
                    MESMSetting<float> sparkyAuraMinEMPWaitMESM = new MESMSetting<float>("Sparky EMP Minimum Charge Time", "Sets the minimum time Sparky needs to wait between EMPs", 30f, true, false);
                    MESMSetting<float> sparkyAuraMaxEMPWaitMESM = new MESMSetting<float>("Sparky EMP Maximum Charge Time", "Sets the maximum time Sparky needs to wait between EMPs", 90f, true, true);
                    MESMSetting<float> sparkyAuraMaxRoomBuffPercentageMESM = new MESMSetting<float>("Sparky Maximum Buff Percentage From Rooms", "Sets the maximum buff percentage Sparky can get from nearby rooms being off compared to the entire region being off", 50f, true, true);
                    MESMSetting<float> sparkyAuraEMPRangeMESM = new MESMSetting<float>("Sparky EMP Range", "Sets the range of Sparky's EMP. Each single corridor piece is 2 metres wide", 8f, true, true);
                    MESMSetting<float> sparkyAuraDistantSwitchChanceMESM = new MESMSetting<float>("Sparky Distant EMP Switch Chance", "Sets the base percentage per EMP that Sparky will switch off a distant room in case there are no active rooms nearby", 50f, true, true);
                    MESMSetting<float> sparkyAuraDistantSwitchFailChanceAdditionMESM = new MESMSetting<float>("Sparky Distant EMP Switch Fail Chance Addition", "Sets how much should be added on to the base percentage each time Sparky fails to switch off a distant room", 10f, true, true);
                    MESMSetting<float> sparkyRegionEMPRoomThresholdMESM = new MESMSetting<float>("Sparky Region EMP Room Threshold", "Sets the percentage of rooms that must be off in a region for Sparky to be able to trigger a region EMP", 20f, true, true);
                    MESMSetting<float> regionElectricTrapSpawnChanceMESM = new MESMSetting<float>("Region Electric Trap Spawn Chance", "Sets the percentage per spawn chance that an electric trap will spawn in a corridor piece when a region is electrified", 30f, true, false);
                    MESMSetting<float> regionElectricTrapSlowFactorMESM = new MESMSetting<float>("Region Electric Trap Slow Factor", "Set the slow factor of region electric traps that can spawn when a region is electrified. The slow factor like a speed multiplier in the sense that it slows the player down for values between 0 and 1, but increments below roughly 0.8 are more noticeable than increments above 0.8", 0.8f, false, true);
                    MESMSetting<float> regionElectricTrapScaleMultiplierMESM = new MESMSetting<float>("Region Electric Trap Scale Multiplier", "Sets the scale multiplier of region electric traps. At the normal scale the width fills roughly one corridor piece", 1f, true, true);
                    MESMSetting<float> regionElectricTrapLifeTimeMESM = new MESMSetting<float>("Region Electric Trap Life Time", "Sets how long region electric traps will stay on the floor when spawned", 5f, true, true);
                    MESMSetting<float> regionElectricTrapMinSpawnTimeMESM = new MESMSetting<float>("Region Electric Trap Minimum Spawn Time", "Sets the minimum time between spawn chances of region electric traps", 7.5f, true, true);
                    MESMSetting<float> regionElectricTrapMaxSpawnTimeMESM = new MESMSetting<float>("Region Electric Trap Maximum Spawn Time", "Sets the maximum time between spawn chances of region electric traps", 15f, true, true);
                    MESMSetting<float> regionElectrificationRoomRecoveryPercentageMESM = new MESMSetting<float>("Region Electrification Room Recovery Percentage", "The rough percentage of off rooms to be switched back on after a region electrification triggered by Sparky chases in unpowered regions", 40f, true, true);
                    MESMSetting<float> sparkyElectricTrapSpawnChanceMESM = new MESMSetting<float>("Sparky Electric Trap Spawn Chance", "Sets the percentage for each nearby corridor piece that an electric trap will spawn there each time Sparky summons electric traps during chases", 50f, true, false);
                    MESMSetting<float> sparkyElectricTrapBaseSlowFactorMESM = new MESMSetting<float>("Sparky Electric Trap Base Slow Factor", "Sets the base slow factor of Sparky electric traps", 0.8f, false, true);
                    MESMSetting<float> maxTrapSlowFactorDecreaseFromSparkyBuffMESM = new MESMSetting<float>("Max Trap Slow Factor Decrease From Buff", "Sets how much the slow factor of Sparky electric traps can be decreased through Sparky's buff percentage. To slow down the player this value should not be higher than the base slow factor", 0.2f, false, true);
                    MESMSetting<float> sparkyElectricTrapBaseScaleMultiplierMESM = new MESMSetting<float>("Sparky Electric Trap Base Scale Multiplier", "Sets the base scale multiplier of Sparky electric traps", 1f, true, true);
                    MESMSetting<float> maxTrapScaleMultiplierIncreaseFromSparkyBuffMESM = new MESMSetting<float>("Max Trap Scale Multiplier Increase From Buff", "Sets how much the base scale multiplier of Sparky electric traps can be increased through Sparky's buff percentage", 0.5f, true, true);
                    MESMSetting<float> sparkyElectricTrapBaseLifeTimeMESM = new MESMSetting<float>("Sparky Electric Trap Base Life Time", "Sets the base life time of Sparky electric traps", 2.5f, true, true);
                    MESMSetting<float> maxTrapLifeTimeIncreaseFromSparkyBuffMESM = new MESMSetting<float>("Max Trap Life Time Increase From Buff", "Sets how much the base life time of Sparky electric traps can be increased through Sparky's buff percentage", 2.5f, true, true);

                    switch (ModSettings.sparkyDifficultyPreset)
                    {
                        case "Easy":
                            {
                                sparkyChaseFactor = sparkyChaseFactorMESM.defaultValue - 0.25f;
                                sparkyMaxChaseFactorIncreaseFromBuff = sparkyMaxChaseFactorIncreaseFromBuffMESM.defaultValue - 0.15f;
                                sparkyMaxSpeedFactorIncreaseFromBuff = sparkyMaxSpeedFactorIncreaseFromBuffMESM.defaultValue - 0.2f;
                                sparkyRotationSpeedFactor = sparkyRotationSpeedFactorMESM.defaultValue - 0.1f;
                                sparkyLurkMinimumDistanceToPlayer = sparkyLurkMinimumDistanceToPlayerMESM.defaultValue;
                                sparkyLurkMaxSuperEMPDistance = sparkyLurkMaxSuperEMPDistanceMESM.defaultValue;
                                sparkyLurkSuperEMPChargeTimeToWait = sparkyLurkSuperEMPChargeTimeToWaitMESM.defaultValue + 1.25f;
                                sparkyLurkMaxAggro = sparkyLurkMaxAggroMESM.defaultValue + 1f;
                                sparkyAuraMinEMPWait = sparkyAuraMinEMPWaitMESM.defaultValue + 30f;
                                sparkyAuraMaxEMPWait = sparkyAuraMaxEMPWaitMESM.defaultValue + 60f;
                                sparkyAuraMaxRoomBuffPercentage = sparkyAuraMaxRoomBuffPercentageMESM.defaultValue;
                                sparkyAuraEMPRange = sparkyAuraEMPRangeMESM.defaultValue;
                                sparkyAuraDistantSwitchChance = sparkyAuraDistantSwitchChanceMESM.defaultValue - 15f;
                                sparkyAuraDistantSwitchFailChanceAddition = sparkyAuraDistantSwitchFailChanceAdditionMESM.defaultValue - 2.5f;
                                sparkyRegionEMPRoomThreshold = sparkyRegionEMPRoomThresholdMESM.defaultValue - 10f;
                                regionElectricTrapSpawnChance = regionElectricTrapSpawnChanceMESM.defaultValue - 10f;
                                regionElectricTrapSlowFactor = regionElectricTrapSlowFactorMESM.defaultValue + 0.05f;
                                regionElectricTrapScaleMultiplier = regionElectricTrapScaleMultiplierMESM.defaultValue;
                                regionElectricTrapLifeTime = regionElectricTrapLifeTimeMESM.defaultValue - 2f;
                                regionElectricTrapMinSpawnTime = regionElectricTrapMinSpawnTimeMESM.defaultValue + 2.5f;
                                regionElectricTrapMaxSpawnTime = regionElectricTrapMaxSpawnTimeMESM.defaultValue + 5f;
                                regionElectrificationRoomRecoveryPercentage = regionElectrificationRoomRecoveryPercentageMESM.defaultValue + 20f;
                                sparkyElectricTrapSpawnChance = sparkyElectricTrapSpawnChanceMESM.defaultValue - 15f;
                                sparkyElectricTrapBaseSlowFactor = sparkyElectricTrapBaseSlowFactorMESM.defaultValue + 0.05f;
                                maxTrapSlowFactorDecreaseFromSparkyBuff = maxTrapSlowFactorDecreaseFromSparkyBuffMESM.defaultValue - 0.1f;
                                sparkyElectricTrapBaseScaleMultiplier = sparkyElectricTrapBaseScaleMultiplierMESM.defaultValue;
                                maxTrapScaleMultiplierIncreaseFromSparkyBuff = maxTrapScaleMultiplierIncreaseFromSparkyBuffMESM.defaultValue - 0.25f;
                                sparkyElectricTrapBaseLifeTime = sparkyElectricTrapBaseLifeTimeMESM.defaultValue - 0.5f;
                                maxTrapLifeTimeIncreaseFromSparkyBuff = maxTrapLifeTimeIncreaseFromSparkyBuffMESM.defaultValue - 1f;
                                break;
                            }
                        case "Medium":
                            {
                                sparkyChaseFactor = sparkyChaseFactorMESM.defaultValue;
                                sparkyMaxChaseFactorIncreaseFromBuff = sparkyMaxChaseFactorIncreaseFromBuffMESM.defaultValue;
                                sparkyMaxSpeedFactorIncreaseFromBuff = sparkyMaxSpeedFactorIncreaseFromBuffMESM.defaultValue;
                                sparkyRotationSpeedFactor = sparkyRotationSpeedFactorMESM.defaultValue;
                                sparkyLurkMinimumDistanceToPlayer = sparkyLurkMinimumDistanceToPlayerMESM.defaultValue;
                                sparkyLurkMaxSuperEMPDistance = sparkyLurkMaxSuperEMPDistanceMESM.defaultValue;
                                sparkyLurkSuperEMPChargeTimeToWait = sparkyLurkSuperEMPChargeTimeToWaitMESM.defaultValue;
                                sparkyLurkMaxAggro = sparkyLurkMaxAggroMESM.defaultValue;
                                sparkyAuraMinEMPWait = sparkyAuraMinEMPWaitMESM.defaultValue;
                                sparkyAuraMaxEMPWait = sparkyAuraMaxEMPWaitMESM.defaultValue;
                                sparkyAuraMaxRoomBuffPercentage = sparkyAuraMaxRoomBuffPercentageMESM.defaultValue;
                                sparkyAuraEMPRange = sparkyAuraEMPRangeMESM.defaultValue;
                                sparkyAuraDistantSwitchChance = sparkyAuraDistantSwitchChanceMESM.defaultValue;
                                sparkyAuraDistantSwitchFailChanceAddition = sparkyAuraDistantSwitchFailChanceAdditionMESM.defaultValue;
                                sparkyRegionEMPRoomThreshold = sparkyRegionEMPRoomThresholdMESM.defaultValue;
                                regionElectricTrapSpawnChance = regionElectricTrapSpawnChanceMESM.defaultValue;
                                regionElectricTrapSlowFactor = regionElectricTrapSlowFactorMESM.defaultValue;
                                regionElectricTrapScaleMultiplier = regionElectricTrapScaleMultiplierMESM.defaultValue;
                                regionElectricTrapLifeTime = regionElectricTrapLifeTimeMESM.defaultValue;
                                regionElectricTrapMinSpawnTime = regionElectricTrapMinSpawnTimeMESM.defaultValue;
                                regionElectricTrapMaxSpawnTime = regionElectricTrapMaxSpawnTimeMESM.defaultValue;
                                regionElectrificationRoomRecoveryPercentage = regionElectrificationRoomRecoveryPercentageMESM.defaultValue;
                                sparkyElectricTrapSpawnChance = sparkyElectricTrapSpawnChanceMESM.defaultValue;
                                sparkyElectricTrapBaseSlowFactor = sparkyElectricTrapBaseSlowFactorMESM.defaultValue;
                                maxTrapSlowFactorDecreaseFromSparkyBuff = maxTrapSlowFactorDecreaseFromSparkyBuffMESM.defaultValue;
                                sparkyElectricTrapBaseScaleMultiplier = sparkyElectricTrapBaseScaleMultiplierMESM.defaultValue;
                                maxTrapScaleMultiplierIncreaseFromSparkyBuff = maxTrapScaleMultiplierIncreaseFromSparkyBuffMESM.defaultValue;
                                sparkyElectricTrapBaseLifeTime = sparkyElectricTrapBaseLifeTimeMESM.defaultValue;
                                maxTrapLifeTimeIncreaseFromSparkyBuff = maxTrapLifeTimeIncreaseFromSparkyBuffMESM.defaultValue;
                                break;
                            }
                        case "Hard":
                            {
                                sparkyChaseFactor = sparkyChaseFactorMESM.defaultValue + 0.3f;
                                sparkyMaxChaseFactorIncreaseFromBuff = sparkyMaxChaseFactorIncreaseFromBuffMESM.defaultValue + 0.1f;
                                sparkyMaxSpeedFactorIncreaseFromBuff = sparkyMaxSpeedFactorIncreaseFromBuffMESM.defaultValue + 0.1f;
                                sparkyRotationSpeedFactor = sparkyRotationSpeedFactorMESM.defaultValue + 0.25f;
                                sparkyLurkMinimumDistanceToPlayer = sparkyLurkMinimumDistanceToPlayerMESM.defaultValue;
                                sparkyLurkMaxSuperEMPDistance = sparkyLurkMaxSuperEMPDistanceMESM.defaultValue;
                                sparkyLurkSuperEMPChargeTimeToWait = sparkyLurkSuperEMPChargeTimeToWaitMESM.defaultValue - 1f;
                                sparkyLurkMaxAggro = sparkyLurkMaxAggroMESM.defaultValue - 0.5f;
                                sparkyAuraMinEMPWait = sparkyAuraMinEMPWaitMESM.defaultValue - 10f;
                                sparkyAuraMaxEMPWait = sparkyAuraMaxEMPWaitMESM.defaultValue - 30f;
                                sparkyAuraMaxRoomBuffPercentage = sparkyAuraMaxRoomBuffPercentageMESM.defaultValue + 25f;
                                sparkyAuraEMPRange = sparkyAuraEMPRangeMESM.defaultValue + 4f;
                                sparkyAuraDistantSwitchChance = sparkyAuraDistantSwitchChanceMESM.defaultValue + 25f;
                                sparkyAuraDistantSwitchFailChanceAddition = sparkyAuraDistantSwitchFailChanceAdditionMESM.defaultValue + 10f;
                                sparkyRegionEMPRoomThreshold = sparkyRegionEMPRoomThresholdMESM.defaultValue + 15f;
                                regionElectricTrapSpawnChance = regionElectricTrapSpawnChanceMESM.defaultValue + 10f;
                                regionElectricTrapSlowFactor = regionElectricTrapSlowFactorMESM.defaultValue - 0.1f;
                                regionElectricTrapScaleMultiplier = regionElectricTrapScaleMultiplierMESM.defaultValue;
                                regionElectricTrapLifeTime = regionElectricTrapLifeTimeMESM.defaultValue + 2f;
                                regionElectricTrapMinSpawnTime = regionElectricTrapMinSpawnTimeMESM.defaultValue;
                                regionElectricTrapMaxSpawnTime = regionElectricTrapMaxSpawnTimeMESM.defaultValue - 5f;
                                regionElectrificationRoomRecoveryPercentage = regionElectrificationRoomRecoveryPercentageMESM.defaultValue - 15f;
                                sparkyElectricTrapSpawnChance = sparkyElectricTrapSpawnChanceMESM.defaultValue + 15f;
                                sparkyElectricTrapBaseSlowFactor = sparkyElectricTrapBaseSlowFactorMESM.defaultValue - 0.1f;
                                maxTrapSlowFactorDecreaseFromSparkyBuff = maxTrapSlowFactorDecreaseFromSparkyBuffMESM.defaultValue + 0.3f;
                                sparkyElectricTrapBaseScaleMultiplier = sparkyElectricTrapBaseScaleMultiplierMESM.defaultValue;
                                maxTrapScaleMultiplierIncreaseFromSparkyBuff = maxTrapScaleMultiplierIncreaseFromSparkyBuffMESM.defaultValue + 0.25f;
                                sparkyElectricTrapBaseLifeTime = sparkyElectricTrapBaseLifeTimeMESM.defaultValue + 2.5f;
                                maxTrapLifeTimeIncreaseFromSparkyBuff = maxTrapLifeTimeIncreaseFromSparkyBuffMESM.defaultValue + 2.5f;
                                break;
                            }
                        default:
                            {
                                sparkyChaseFactor = sparkyChaseFactorMESM.userValue;
                                sparkyMaxChaseFactorIncreaseFromBuff = sparkyMaxChaseFactorIncreaseFromBuffMESM.userValue;
                                sparkyMaxSpeedFactorIncreaseFromBuff = sparkyMaxSpeedFactorIncreaseFromBuffMESM.userValue;
                                sparkyRotationSpeedFactor = sparkyRotationSpeedFactorMESM.userValue;
                                sparkyLurkMinimumDistanceToPlayer = sparkyLurkMinimumDistanceToPlayerMESM.userValue;
                                sparkyLurkMaxSuperEMPDistance = sparkyLurkMaxSuperEMPDistanceMESM.userValue;
                                sparkyLurkSuperEMPChargeTimeToWait = sparkyLurkSuperEMPChargeTimeToWaitMESM.userValue;
                                sparkyLurkMaxAggro = sparkyLurkMaxAggroMESM.userValue;
                                sparkyAuraMinEMPWait = sparkyAuraMinEMPWaitMESM.userValue;
                                sparkyAuraMaxEMPWait = sparkyAuraMaxEMPWaitMESM.userValue;
                                sparkyAuraMaxRoomBuffPercentage = sparkyAuraMaxRoomBuffPercentageMESM.userValue;
                                sparkyAuraEMPRange = sparkyAuraEMPRangeMESM.userValue;
                                sparkyAuraDistantSwitchChance = sparkyAuraDistantSwitchChanceMESM.userValue;
                                sparkyAuraDistantSwitchFailChanceAddition = sparkyAuraDistantSwitchFailChanceAdditionMESM.userValue;
                                sparkyRegionEMPRoomThreshold = sparkyRegionEMPRoomThresholdMESM.userValue;
                                regionElectricTrapSpawnChance = regionElectricTrapSpawnChanceMESM.userValue;
                                regionElectricTrapSlowFactor = regionElectricTrapSlowFactorMESM.userValue;
                                regionElectricTrapScaleMultiplier = regionElectricTrapScaleMultiplierMESM.userValue;
                                regionElectricTrapLifeTime = regionElectricTrapLifeTimeMESM.userValue;
                                regionElectricTrapMinSpawnTime = regionElectricTrapMinSpawnTimeMESM.userValue;
                                regionElectricTrapMaxSpawnTime = regionElectricTrapMaxSpawnTimeMESM.userValue;
                                regionElectrificationRoomRecoveryPercentage = regionElectrificationRoomRecoveryPercentageMESM.userValue;
                                sparkyElectricTrapSpawnChance = sparkyElectricTrapSpawnChanceMESM.userValue;
                                sparkyElectricTrapBaseSlowFactor = sparkyElectricTrapBaseSlowFactorMESM.userValue;
                                maxTrapSlowFactorDecreaseFromSparkyBuff = maxTrapSlowFactorDecreaseFromSparkyBuffMESM.userValue;
                                sparkyElectricTrapBaseScaleMultiplier = sparkyElectricTrapBaseScaleMultiplierMESM.userValue;
                                maxTrapScaleMultiplierIncreaseFromSparkyBuff = maxTrapScaleMultiplierIncreaseFromSparkyBuffMESM.userValue;
                                sparkyElectricTrapBaseLifeTime = sparkyElectricTrapBaseLifeTimeMESM.userValue;
                                maxTrapLifeTimeIncreaseFromSparkyBuff = maxTrapLifeTimeIncreaseFromSparkyBuffMESM.userValue;
                                break;
                            }
                    }


                    // Read Gamemode Settings Variables
                    modSettingsErrorString = "Gamemode";
                    MESMSetting.currentCategoryBeingAssigned = modSettingsErrorString + " Settings";
                    enableMultiplayer = new MESMSetting<bool>("Enable Multiplayer Mode", "Local co-op multiplayer that can be played online via programs such as Parsec. The first player uses the keyboard and the second a controller. Can be played split screen or with two monitors if two monitors using the same resolution are connected and the use multiple displays option is enabled. If a player were to die normally, they are downed instead and can be revived by throwing a smashable onto them. Requires a restart to change", false).userValue;
                    MESMSetting<bool> enableCrewVSMonsterModeMESM = new MESMSetting<bool>("Enable Crew VS Monster Mode", "Lets the second player control the monster. Each monster has a unique ability that the monster player can use with the crouch button. Hunter can double tap to use their ability differently while in hiding or trapping. Hiding spots can be searched with the jump button. Requires a restart to change and multiplayer to be enabled. The AI can be given back control by pressing the journal button, which lets the normal AI be spectated", false, false, true);
                    if (!enableMultiplayer)
                    {
                        enableCrewVSMonsterMode = false;
                    }
                    enableCrewVSMonsterMode = enableCrewVSMonsterModeMESM.userValue;
                    int numberOfMonsterPlayer = new MESMSetting<int>("Number Of Monster Player", "Selects which player will be the monster player. Can be either 1 or 2", 2, true, true).userValue - 1;
                    RandomNumberOfMonsterPlayers = 0; //Math.Abs(Convert.ToInt32(FindSetting("Random Number Of Monster Players")));
                    numbersOfMonsterPlayers = new List<int>();
                    if (ModSettings.enableCrewVSMonsterMode)
                    {
                        numbersOfMonsterPlayers.Add(numberOfMonsterPlayer);
                    }
                    //numbersOfMonsterPlayers.Add(1);
                    /*
                    if (RandomNumberOfMonsterPlayers > 0)
                    {
                        int monstersChosen = 0;
                        while (monstersChosen < RandomNumberOfMonsterPlayers && monstersChosen < NumberOfPlayers)
                        {
                            int randomPlayerNumber = UnityEngine.Random.Range(0, NumberOfPlayers); // The maximum in Random.Range using integers is not included. This means that here the number of players can be used as the maximum input.
                            if (!numbersOfMonsterPlayers.Contains(randomPlayerNumber))
                            {
                                numbersOfMonsterPlayers.Add(randomPlayerNumber);
                                monstersChosen++;
                            }
                        }
                    }
                    else
                    {
                        // Numbers of monster players are read as (actualNumber + 1) in text. Player numbering starts from 1 instead of 0.
                        try
                        {
                            string[] specificMonsterPlayerNumbers = FindSetting("Numbers Of Monster Players").Split(',');
                            for (int i = 0; i < specificMonsterPlayerNumbers.Length; i++)
                            {
                                specificMonsterPlayerNumbers[i] = specificMonsterPlayerNumbers[i].Replace(" ", "");
                                int playerNumber = Int32.Parse(specificMonsterPlayerNumbers[i]) - 1;
                                if (playerNumber >= 0 && playerNumber <= 3)
                                {
                                    numbersOfMonsterPlayers.Add(playerNumber);
                                }
                                else
                                {
                                    Debug.Log("CREW VS MONSTER MODE PLAYER NUMBER OUT OF RANGE " + playerNumber + ". MAKING ONLY PLAYER 1 A MONSTER.");
                                    numbersOfMonsterPlayers = new List<int>();
                                    numbersOfMonsterPlayers.Add(1);
                                }
                            }
                        }
                        catch
                        {
                            Debug.Log("CREW VS MONSTER MODE PLAYER NUMBERS HAVE NOT BEEN SPECIFIED IN THE CORRECT FORMAT. MAKING ONLY PLAYER 1 A MONSTER.");
                            errorWhileReadingModSettings = true;
                            numbersOfMonsterPlayers = new List<int>();
                            numbersOfMonsterPlayers.Add(1);
                        }
                    }
                    */
                    if (enableMultiplayer)
                    {
                        NumberOfPlayers = 2; //Convert.ToInt32(FindSetting("Number Of Players").userValue; //2;
                    }
                    else
                    {
                        NumberOfPlayers = 1;
                    }
                    giveMonsterAnInventory = new MESMSetting<bool>("Give Monster An Inventory", "Lets monster players use an inventory like a crew player", false).userValue;
                    letMonsterUseInteractiveObjects = new MESMSetting<bool>("Let Monster Use Interactive Objects", "Lets monster players interact with certain objects that play fixed animations for the player like dragging the liferaft or turning steam handles", false).userValue;

                    bruteAbilityActiveTime = new MESMSetting<float>("Brute Ability Active Time", "Set the active time of Brute's charge ability", 3, true).userValue;
                    bruteAbilityCooldownTime = new MESMSetting<float>("Brute Ability Cooldown Time", "Set the cooldown time of Brute's charge ability", 15, true, true).userValue;
                    /*
                    try
                    {
                        //string bruteAbilityTimes = MESMSetting.FindSetting("Brute Ability Times");
                        //bruteAbilityTimes = bruteAbilityTimes.Replace(" ", "");
                        //string[] bruteAbilityTimesSplit = bruteAbilityTimes.Split(',');
                        //bruteAbilityActiveTime = new MESMSetting<float>(bruteAbilityTimesSplit[0], true).userValue;
                        //bruteAbilityCooldownTime = new MESMSetting<float>(bruteAbilityTimesSplit[1], true).userValue;

                        //bruteAbilityActiveTime = new MESMSetting<float>("Brute Ability Times", "Set the active time of Brute's charge ability", 3, true).userValue;
                        //bruteAbilityCooldownTime = new MESMSetting<float>("Brute Ability Times", "Set the cooldown time of Brute's charge ability", 15, true, 2).userValue;
                    }
                    catch
                    {
                        Debug.Log("Brute ability times were not in correct format.");
                        bruteAbilityActiveTime = Convert.ToSingle(3);
                        bruteAbilityCooldownTime = Convert.ToSingle(15);
                    }
                    */
                    bruteAbilitySpeedMultiplier = new MESMSetting<float>("Brute Ability Speed Multiplier", "Give the Brute an additional speed multiplier on top of the inherent speed increase due to the charging animation", 1, false, true).userValue;

                    hunterAbilityActiveTime = new MESMSetting<float>("Hunter Ability Active Time", "Set the active time of Hunter's vent usage ability", 0, true).userValue;
                    hunterAbilityCooldownTime = new MESMSetting<float>("Hunter Ability Cooldown Time", "Set the cooldown time of Hunter's vent usage ability", 0, true, true).userValue;
                    /*
                    try
                    {
                        //string hunterAbilityTimes = MESMSetting.FindSetting("Hunter Ability Times");
                        //hunterAbilityTimes = hunterAbilityTimes.Replace(" ", "");
                        //string[] hunterAbilityTimesSplit = hunterAbilityTimes.Split(',');
                        //hunterAbilityActiveTime = new MESMSetting<float>(hunterAbilityTimesSplit[0], true).userValue;
                        //hunterAbilityCooldownTime = new MESMSetting<float>(hunterAbilityTimesSplit[1], true).userValue;

                        //hunterAbilityActiveTime = new MESMSetting<float>("Hunter Ability Times", "Set the active time of Hunter's vent usage ability", 0, true).userValue;
                        //hunterAbilityCooldownTime = new MESMSetting<float>("Hunter Ability Times", "Set the cooldown time of Hunter's vent usage ability", 0, true, 2).userValue;
                    }
                    catch
                    {
                        Debug.Log("Hunter ability times were not in correct format.");
                        hunterAbilityActiveTime = Convert.ToSingle(0);
                        hunterAbilityCooldownTime = Convert.ToSingle(0);
                    }
                    */

                    hunterVentFrenzyAbilityActiveTime = new MESMSetting<float>("Hunter Vent Frenzy Active Time", "Set the active time of Hunter's vent frenzy ability", 2, true, true).userValue;
                    hunterVentFrenzyAbilityCooldownTime = new MESMSetting<float>("Hunter Vent Frenzy Cooldown Time", "Set the cooldown time of Hunter's vent frenzy ability", 3, true, true).userValue;
                    /*
                    try
                    {
                        //string hunterVentFrenzyAbilityTimes = MESMSetting.FindSetting("Hunter Vent Frenzy Ability Times");
                        //hunterVentFrenzyAbilityTimes = hunterVentFrenzyAbilityTimes.Replace(" ", "");
                        //string[] hunterVentFrenzyAbilityTimesSplit = hunterVentFrenzyAbilityTimes.Split(',');
                        //hunterVentFrenzyAbilityActiveTime = new MESMSetting<float>(hunterVentFrenzyAbilityTimesSplit[0], true).userValue;
                        //hunterVentFrenzyAbilityCooldownTime = new MESMSetting<float>(hunterVentFrenzyAbilityTimesSplit[1], true).userValue;

                        //hunterVentFrenzyAbilityActiveTime = new MESMSetting<float>("Hunter Vent Frenzy Ability Times", "Set the active time of Hunter's vent frenzy ability", 2, true).userValue;
                        //hunterVentFrenzyAbilityCooldownTime = new MESMSetting<float>("Hunter Vent Frenzy Ability Times", "Set the cooldown time of Hunter's vent frenzy ability", 3, true, 2).userValue;
                    }
                    catch
                    {
                        Debug.Log("Hunter vent frenzy ability times were not in correct format.");
                        hunterVentFrenzyAbilityActiveTime = Convert.ToSingle(2);
                        hunterVentFrenzyAbilityCooldownTime = Convert.ToSingle(3);
                    }
                    */
                    hunterVentFrenzyAbilitySpeedMultiplier = new MESMSetting<float>("Hunter Vent Frenzy Speed Multiplier", "Give the Hunter an additional speed multiplier on top of the coded speed increase when using the vent frenzy ability", 1, false, true).userValue;

                    fiendAbilityActiveTime = new MESMSetting<float>("Fiend Ability Active Time", "Set the active time of Fiend's door lock and EMP ability", 15, true).userValue;
                    fiendAbilityCooldownTime = new MESMSetting<float>("Fiend Ability Cooldown Time", "Set the cooldown time of Fiend's door lock and EMP ability", 45, true, true).userValue;
                    /*
                    try
                    {
                        //string fiendAbilityTimes = MESMSetting.FindSetting("Fiend Ability Times");
                        //fiendAbilityTimes = fiendAbilityTimes.Replace(" ", "");
                        //string[] fiendAbilityTimesSplit = fiendAbilityTimes.Split(',');
                        //fiendAbilityActiveTime = new MESMSetting<float>(fiendAbilityTimesSplit[0], true).userValue;
                        //fiendAbilityCooldownTime = new MESMSetting<float>(fiendAbilityTimesSplit[1], true).userValue;

                        //fiendAbilityActiveTime = new MESMSetting<float>("Fiend Ability Times", "Set the active time of Fiend's door lock and EMP ability", 15, true).userValue;
                        //fiendAbilityCooldownTime = new MESMSetting<float>("Fiend Ability Times", "Set the cooldown time of Fiend's door lock and EMP ability", 45, true, 2).userValue;
                    }
                    catch
                    {
                        Debug.Log("Fiend ability times were not in correct format.");
                        fiendAbilityActiveTime = Convert.ToSingle(15);
                        fiendAbilityCooldownTime = Convert.ToSingle(45);
                    }
                    */
                    fiendAbilityDoorLockRange = new MESMSetting<float>("Fiend Ability Door Lock Range", "Set how far from where the Fiend is looking doors should be locked during the ability", 10, false, true).userValue;
                    fiendAbilityAuraRangeMultiplier = new MESMSetting<float>("Fiend Ability Aura Range Multiplier", "Set the multiplier of how much Fiend's aura is changed during the ability", 5, false, true).userValue;


                    /*
                    // Keep in mind that this is the old layout and text.
                    ----------------------------------------------------------------------------------------------------
                    Other Settings:
                    ------------------------------
                    Multiplayer Mode - A early version of split screen multiplayer. Requires a restart to change. The first player uses the keyboard and the second a controller. Will be further developed in the future. - Default = False
                    True
                    Number Of Players - Specifies how many players to set up - Default = 2
                    4
                    Enable Crew VS Monster Mode - Lets the second player control the monster. Requires a restart to change and multiplayer to be enabled. The AI can be given back control by pressing the back/select button on the controller. - Default = False
                    True
                    Numbers Of Monster Players - Specify which players will be monsters. Use a player's number and separate players by a comma. Start counting from 1. For example, typing in "2,3" (without quotation marks) would make players 2 and 3 monsters. - Default = 2
                    2,3
                    Random Number Of Monster Players - An alternative way to select monster players. Instead of selecting which specific players will be monsters, a single number will define how many monster players there will be. Monster players will be chosen at random. Select 0 to disable this selection. - Default = 0
                    0
                    Use Multiple Displays If Possible - The game will split up the players' views evenly across all available screens. Each screen must be set to the same resolution as the game uses on the first screen. - Default = True
                    False
                    ------------------------------
                    */
                    useMultipleDisplaysIfPossible = new MESMSetting<bool>("Use Multiple Displays If Possible", "The game will split up the players' views evenly across all available screens. Each screen must be set to the same resolution as the game uses on the first screen in order for the game to render correctly", true).userValue;
                    if (numberOfMonsters < numbersOfMonsterPlayers.Count) // Check whether too few monsters have been selected to spawn to host all monster players.
                    {
                        numberOfRandomMonsters += numbersOfMonsterPlayers.Count - numberOfMonsters;
                        numberOfMonsters += numbersOfMonsterPlayers.Count - numberOfMonsters;
                    }
                    MultiplayerMode.useLegacyAudio = new MESMSetting<bool>("Use Legacy Audio In Multiplayer", "Does not make adjustments to the audio for multiplayer and instead only gives the first player audio. The custom multiplayer audio system is quite performance heavy and can cause lag spikes and audio issues in certain situations but plays the audio of all players instead of just player 1. Requires a restart to change and multiplayer to be enabled", false, false, true).userValue;
                    CrewVsMonsterMode.letAIControlMonster = new MESMSetting<bool>("Start With AI Having Control", "Starts Crew VS Monsters rounds with the AI having control over the monster. Useful if you simply want to use the mode for spectating the monster without having to press the control transfer button", false, false, true).userValue;
                    /*
                    ------------------------------
                    Use Custom Multiplayer Key Bindings - Enables remapping of key bindings.
                    False
                    Multiplayer Key Bindings - Specifies custom key binds for secondary players. Player 1's key binds are still determined by the game settings.
                    ----------
                    KeyBind Forward
                    W, 
                    ----------
                    ----------
                    */
                    useCustomKeyBinds = false; //Convert.ToBoolean(FindSetting("Use Custom Multiplayer Key Bindings").userValue;
                    darkShip = new MESMSetting<bool>("Dark Ship", "Almost all lights on the ship are turned off. Combines well with the Fiend", false).userValue;
                    powerableLights = new MESMSetting<bool>("Powerable Lights", "Lights can be powered on again while Dark Ship is enabled. Does nothing outwith Dark Ship mode", false, false, true).userValue;
                    randomiserMode = new MESMSetting<bool>("Randomiser Mode", "Randomises which settings are used and what they are set to", false).userValue;
                    chaosMultiplier = new MESMSetting<float>("Chaos Multiplier", "Increases or decreases the probability of settings being used and the magnitude of each setting", 1, false, true).userValue;
                    glowstickHunt = new MESMSetting<bool>("Glowstick Hunt", "A few glowsticks are chosen at random to be of a different colour than the rest. Activating all of the special glowsticks will prepare the liferaft for you", false).userValue;
                    specialGlowsticksRequired = new MESMSetting<int>("Special Glowsticks Required", "Sets how many glowsticks are required to start the Glowstick Hunt finale. The colours used cycle through the colours of the rainbow", 6, false, true).userValue;
                    specialGlowsticksToCreate = new MESMSetting<int>("Special Glowsticks To Create", "Sets how many normal glowsticks will be selected to be special glowsticks when the round starts. Will always be set to at least the number of special glowsticks required", 12, false, true).userValue;
                    useCustomDoors = new MESMSetting<bool>("Use Custom Doors Mode", "Sets each door on the ship to this door type. This may cause a bug when exiting the tutorial room", false).userValue;
                    customDoorTypeNumber = new MESMSetting<int>("Custom Door Type", "0 = Normal, 1 = Outside, 2 = SubRoom, 3 = Powered, 4 = Lower Deck / Cargo Hold, 5 = Barricade & 6 = Engine. For stability reasons, requires a restart to enable or disable", 0, true, true).userValue;
                    lightlyLockedDoors = new MESMSetting<bool>("Lightly Locked Doors", "If Custom Doors Mode is active, sets upper deck doors to normal doors and lower deck doors to engine room doors. Then, locks all the doors and spawns a welding kit for you to open them. As with Custom Door Type Mode, requires a restart to enable or disable", false).userValue;
                    scavengerMode = new MESMSetting<bool>("Scavenger Mode", "Shows the distance to an uncollected item to the player. Cannot run at the same time as other text-displaying options", false).userValue;
                    deathCountdown = new MESMSetting<float>("Death Countdown", "Kills the player after a specified number of seconds. Activates after leaving the starter room", 0, true).userValue;
                    showDeathCountdown = new MESMSetting<bool>("Show Death Countdown", "Displays the number of seconds you have left in a death countdown round. Cannot run at the same time as other text-displaying options", false, false, true).userValue;
                    itemMonsterFrenzy = new MESMSetting<bool>("Item Monster Frenzy", "Picking up a mission item spawns an additional monster. Must start the game with at least 2 monsters or force MMM to be enabled", false).userValue;
                    extraChaoticIMF = new MESMSetting<bool>("Extra Chaotic IMF", "Picking up any item will spawn an additional monster. Must start the game with at least 2 monsters", false).userValue;
                    monsterSpawnSpeedrunSpawnTime = new MESMSetting<float>("Monster Spawn Speedrun", "Spawns a new monster after each time a specified number of seconds has passed. Only starts after all original monsters have spawned. Must start the game with at least 2 monsters or force MMM to be enabled", 0, true).userValue;
                    monsterSpawningLimit = new MESMSetting<float>("Monster Spawning Limit", "Limit the maximum number of monsters that can spawn via settings that spawn additional monsters during the round. 0 means no limit", 10, true).userValue;
                    foggyShip = new MESMSetting<bool>("Foggy Ship", "Creates fog in front of the player, stopping them from seeing what is ahead of them", false).userValue;
                    fogDistance = new MESMSetting<float>("Fog Distance", "Defines how far away from the player the camera fog will be", 16f, true, true).userValue;
                    monsterVisionAffectedByFog = new MESMSetting<bool>("Monster Vision Affected By Fog", "Makes monster vision be affected by the fog, meaning that monsters will not see you from beyond it", false, false, true).userValue;
                    foggyShipAlternativeMode = new MESMSetting<bool>("Foggy Ship Alternative Mode", "Creates particle fog instead of simply not rendering past the fog distance", false, false, true).userValue;
                    smokyShip = new MESMSetting<bool>("Smoky Ship", "Fills the ship's corridors with dangerous smoke when they are unpowered", false).userValue;
                    alwaysSmoky = new MESMSetting<bool>("Always Smoky", "Corridors are filled with smoke even when they are powered. Does nothing outwith Smoky Ship mode.", false, false, true).userValue;
                    breathAmount = new MESMSetting<float>("Breath Amount", "The maximum number of seconds the player may be in smoke with a breath drain of 1", 20f, true, true).userValue;
                    breathRecovery = new MESMSetting<float>("Breath Recovery", "The amount of breath to recover per second outside of smoke", 2f, true, true).userValue;
                    smokeShroudBreathDrain = new MESMSetting<float>("Smoke Shroud Breath Drain", "The amount of breath to drain per second inside of a smoke shroud", 2f, true, true).userValue;
                    smokeCorridorBreathDrain = new MESMSetting<float>("Smoke Corridor Breath Drain", "The amount of breath to drain per second inside of a smoke corridor", 1f, true, true).userValue;


                    // Read Player Settings Variables
                    modSettingsErrorString = "Player";
                    MESMSetting.currentCategoryBeingAssigned = modSettingsErrorString + " Settings";
                    extraLives = new MESMSetting<int>("Extra Lives", "Allows you to teleport back to the starter room instead of dying if you have additional lives remaining. Also gives a few seconds of spawn protection", 0).userValue;
                    inventorySize = new MESMSetting<int>("Inventory Size", "Changes how many inventory slots you have when starting a game", 0).userValue;
                    minimumValueOnFOVSlider = new MESMSetting<float>("Minimum Value On FOV Slider", "Allows you to customise your FOV Slider. Set minimum to -180 and maximum to 180 for full range of FOVs. This sets the minimum value of the FOV slider", 50).userValue;
                    maximumValueOnFOVSlider = new MESMSetting<float>("Maximum Value On FOV Slider", "Allows you to customise your FOV Slider. Set minimum to -180 and maximum to 180 for full range of FOVs. This sets the maximum value of the FOV slider", 70, false, true).userValue;
                    playerStaminaMode = new MESMSetting<bool>("Player Stamina Mode", "Activates a custom stamina system that will slow the player's running speed down the longer they run", false).userValue;
                    MESMSetting<float> playerStaminaModeDurationMESM = new MESMSetting<float>("Player Stamina Mode Stamina Duration", "Defines in seconds how long it will take for the player to lose their stamina and get to roughly their lowest running speed", 30, true, true);
                    playerStaminaModeHalfStaminaDuration = playerStaminaModeDurationMESM.userValue / 2f;
                    //playerStaminaModeHalfStaminaDuration = (MESMSetting.modSettingsExist ? MESMSetting<float>.FindSetting("Player Stamina Mode Stamina Duration") / 2f : 30f); // Default = 30
                    float playerStaminaModeSpeedPenaltyPercentage = new MESMSetting<float>("Player Stamina Mode Stamina Penalty", "Defines as a percentage how much of a penalty is applied to the running speed of the player when they lose their stamina. The scale is from 0 (running speed) to 100 (walking speed)", 60, true, true, 0, 100).userValue;
                    playerStaminaModeSpeedPenaltyMultiplier = (100f - playerStaminaModeSpeedPenaltyPercentage) / 100f;
                    playerMovementSpeedMultiplier = new List<float>();
                    float globalPlayerMovementSpeedMultiplier = new MESMSetting<float>("Player Movement Speed Multiplier", "Only affects horizontal movement", 1).userValue;
                    for (int i = 0; i < NumberOfPlayers - numbersOfMonsterPlayers.Count; i++)
                    {
                        Debug.Log("Setting player movement speed multiplier");
                        playerMovementSpeedMultiplier.Add(globalPlayerMovementSpeedMultiplier);
                    }
                    customPlayerScale = new MESMSetting<float>("Custom Player Scale", "Change the scale of the player. Hitbox scales alongside this, but not speed. Can make monsters not see the player when they should be able to", 1).userValue;
                    unlockPlayerHead = new MESMSetting<bool>("Unlock Player Head", "Lets the player look up and down by the full 90 degrees instead of being locked to the default 60 degrees", false).userValue;


                    // Read Level Generation Settings Variables
                    modSettingsErrorString = "Level Generation";
                    MESMSetting.currentCategoryBeingAssigned = modSettingsErrorString + " Settings";
                    useCustomSeed = new MESMSetting<bool>("Use Custom Seed", "Specify whether to use a custom seed or not. The level seed only affects limited aspects of level generation", false).userValue;
                    seed = new MESMSetting<int>("Seed To Use", "The seed to use. The level seed only affects limited aspects of level generation", 0, false, true).userValue;
                    startRoomRegion = new MESMSettingMultipleChoice("Starter Room Region", "Can be Upper Deck, Lower Deck or Either", "Either", new string[] { "Either", "Upper Deck", "Lower Deck" }).userValue;
                    spawnDeactivatedItems = new MESMSetting<bool>("Spawn Deactivated Items", "Lets the compass and walkie talkie items spawn. These items were spawned in older versions of the game, but later removed", false).userValue;
                    noCameras = new MESMSetting<bool>("No Cameras", "All cameras are covered with duct tape", false).userValue;
                    noSteam = new MESMSetting<bool>("No Steam", "Sets the engine room's steam override to off. Model does not update", false).userValue;
                    allPreFilledFuseBoxes = new MESMSetting<bool>("All Pre-filled Fuse Boxes", "Spawns each fuse box with a fuse", false).userValue;
                    noPreFilledFuseBoxes = new MESMSetting<bool>("No Pre-filled Fuse Boxes", "Spawns each fuse box without a fuse", false).userValue;
                    noBarricadedDoors = new MESMSetting<bool>("No Barricaded Doors", "Removes the door of every barricaded room", false).userValue;
                    overpoweredSteamVents = new MESMSetting<bool>("Overpowered Steam Vents", "Lets steam be expelled from each possible point on a steam vent. This means every vent will have three steam spawn points and handles. Two handles will be clipped in each other, making it harder to turn off both", false).userValue;
                    unbreakablePitTraps = new MESMSetting<bool>("Unbreakable Pit Traps", "Pit traps are not destroyed when the player or monster runs over them", false).userValue;
                    addAdditionalCrewDeckBuilding = new MESMSetting<bool>("Add Additional Crew Deck Building", "Removes some of the cargo containers in front of the bridge and replaces them with a small crew deck building with another life raft on top of it", false).userValue;
                    additionalFuseBoxesToSetUpAfterGeneration = new List<FuseBox>();
                    useDeckFourOnSubmersibleSide = new MESMSetting<bool>("Use Deck Four On Submersible Side", "Allows deck 4 to spawn rooms like deck 3 on the submersible side. Also increases the spawn count of lower deck rooms a bit to help fill the additional space", false).userValue;
                    extendLowerDecks = new MESMSetting<bool>("Extend Lower Decks", "A setting that has a similar effect to the extend map one, but only affects the lower decks and has much faster loading times. Also increases the spawn count of lower deck rooms a tiny bit to help fill the additional space", false).userValue;
                    spawnAdditionalEngineRoomWorkshops = new MESMSetting<bool>("Spawn Additional Engine Room Workshops", "Spawns some more engine room workshops with phones", false).userValue;
                    aggressiveWorkshopSpawning = new MESMSetting<bool>("Aggressive Workshop Spawning", "Replaces nodes dedicated to decorative multi-floor engines by nodes that can spawn more workshops", false, false, true).userValue;
                    numberOfCorridorsToCargoHoldFromDeckThree = new MESMSetting<int>("Number Of Corridors To Cargo Hold From Deck Three", "Lets one or both side of deck 3 connect to the cargo hold. If the Use Deck Four On Submersible Side setting is enabled, then it will also similarly connect to the cargo hold. You can use 0, 1 or 2", 0, false, false, 0, 2).userValue; // Mathf.Clamp(Convert.ToInt32(FindSetting("Number Of Corridors To Cargo Hold From Deck Three")), 0, 2);
                    reduceNormalNumberOfCorridorsToCargoHold = new MESMSetting<int>("Reduce Normal Number Of Corridors To Cargo Hold", "Stops one or both sides of deck 1 and 2 on the submersible side and deck 3 and 4 on the engine side from connecting to the cargo hold. You can use 0, 1 or 2", 0, false, false, 0, 2).userValue; // Mathf.Clamp(Convert.ToInt32(FindSetting("Reduce Normal Number Of Corridors To Cargo Hold")), 0, 2);
                    lengthenedCargoHoldCorridors = new MESMSetting<bool>("Lengthened Cargo Hold Corridors", "Lets corridors going to the cargo hold go through the entire ship, not just to one side of the cargo hold", false).userValue;
                    shortenedCargoHoldCorridors = new MESMSetting<bool>("Shortened Cargo Hold Corridors", "Lets corridors going to the cargo hold only go to the first possible door. This forces the player to go through the cargo hold's cargo containers instead of simply taking the stairs", false).userValue;
                    addLowerDeckNextToEngineRoom = new MESMSetting<bool>("Add Lower Deck Next To Engine Room", "Creates a new area over decks 1 and 2 that often connects to the engine room and optionally cargo holds. Also increases the spawn count of lower deck rooms a bit to help fill the additional space", false).userValue;
                    numberOfCorridorsToCargoHoldFromNewLowerDeck = new MESMSetting<int>("Number Of Corridors To Cargo Hold From New Lower Deck", "Has the same effect as the Number Of Corridors To Cargo Hold From Deck Three setting but specifies the number of corridors for the new lower deck. You can use 0, 1 or 2", 2, false, true, 0, 2).userValue; // Mathf.Clamp(Convert.ToInt32(FindSetting("Number Of Corridors To Cargo Hold From New Lower Deck")), 0, 2);
                    addDeckZero = new MESMSetting<bool>("Add Deck Zero", "Adds a deck zero with access from deck 3 or deck 4 if it is enabled. If the Add Lower Deck Next To Engine Room setting is enabled then it will also connect to the staircases of that section", false).userValue;
                    shuffledRegions = new MESMSetting<bool>("Shuffled Regions", "Shuffles where all the regions generate by swapping two regions at a time. The below setting tells the game how many times to swap regions. Each swap will increase the loading time of the game", false).userValue;
                    numberOfTimesToShuffleRegions = new MESMSetting<int>("Number Of Shuffles", "The number of times to swap two regions in order to shuffle all regions", 3, true, true).userValue;
                    crazyShuffle = new MESMSetting<bool>("Crazy Shuffle", "Removes limitations from region shuffling. Has a high chance of making the game unplayable without debug mode. May also stop the game from loading. Does nothing without the Shuffled Regions setting", false).userValue;
                    extendMap = new MESMSetting<bool>("Extend Map", "Lets the game generate rooms and corridors in usually inaccessible regions. This can cause certain regions to generate in visually unusual ways. Increases loading time considerably", false).userValue;
                    extendMapAdditive = new MESMSetting<bool>("Extend Map Additively", "Lets any of the normal regions generate in usually inaccessible regions. Increases loading time considerably, even more than the Extend Map setting", false).userValue;
                    float increaseMapSizeX = new MESMSetting<float>("Increase Map Size X Component", "Increases the x dimension of the map. Further increases loading time if map is also set to be extended. The standard ship size is (76, 10, 16)", 0).userValue;
                    float increaseMapSizeY = new MESMSetting<float>("Increase Map Size Y Component", "Increases the y dimension of the map. Further increases loading time if map is also set to be extended. The standard ship size is (76, 10, 16)", 0, false, true).userValue;
                    float increaseMapSizeZ = new MESMSetting<float>("Increase Map Size Z Component", "Increases the z dimension of the map. Further increases loading time if map is also set to be extended. The standard ship size is (76, 10, 16)", 0, false, true).userValue;
                    increaseMapSizeVector = new Vector3(increaseMapSizeX, increaseMapSizeY, increaseMapSizeZ);
                    increaseRoomMinimumCount = new MESMSetting<int>("Increase Minimum Room Counts", "Increases the minimum times each room can occur. Increases loading time considerably", 0).userValue;
                    increaseRoomMaximumCount = new MESMSetting<int>("Increase Maximum Room Counts", "Increases the maximum times each room can occur. Increases loading time considerably", 0, false, true).userValue;
                    includeUniqueRoomsInCountChange = new MESMSetting<bool>("Include Unique Rooms In Count Change", "Also changes rooms that usually spawn only once in the room count change", false, false, true).userValue;
                    changeKeyItemSpawnNumbers = new MESMSetting<int>("Change Key Item Spawn Numbers", "Changes the number of key items that may appear around the ship", 0).userValue;
                    allowKeyItemsToNotSpawnAtAll = new MESMSetting<bool>("Allow Key Items To Not Spawn At All", "If the number of key items to spawn has been reduced to below 1, instead of setting them to 1 let them be 0", false, false, true).userValue;
                    diverseItemSpawns = new MESMSetting<bool>("Diverse Item Spawns", "Allows items to be spawned in regions they may not previously have been found", false, false, true).userValue;
                    spawnItemsAnywhere = new MESMSetting<bool>("Spawn Items Anywhere", "Allows items to be spawned in any valid spot", false, false, true).userValue;


                    // Read Item Settings Variables
                    modSettingsErrorString = "Item";
                    MESMSetting.currentCategoryBeingAssigned = modSettingsErrorString + " Settings";
                    infiniteFireExtinguisherFuel = new MESMSetting<bool>("Infinite Fire Extinguisher Fuel", "Fire extinguishers will have infinite fuel", false).userValue;
                    flareLifetime = new MESMSetting<float>("Flare Lifetime", "Sets the time that flares burn after being shot out of a flare gun", 10).userValue;
                    spawnWithFlareGun = new MESMSetting<bool>("Spawn With Flare Gun", "Gives the player a flare gun at the start of the game", false, false, true).userValue;
                    overpoweredFlareGun = new MESMSetting<bool>("Overpowered Flare Gun", "Removes the cooldown from the flare gun, lets it fire infinite flares and stun any monster", false, false, true).userValue;
                    flaresDisableMonsters = new MESMSetting<bool>("Flares Disable Monsters", "Disables any monster hit by a flare", false, false, true).userValue;
                    infiniteFlashlightPower = new MESMSetting<bool>("Infinite Flashlight Power", "Flashlights will have infinite power", false).userValue;
                    infiniteFuelCanFuel = new MESMSetting<bool>("Infinite Fuel Can Fuel", "Fuel cans will have infinite fuel", false).userValue;
                    fireDurationMultiplier = new MESMSetting<float>("Fire Duration Multiplier", "Changes the duration of how long fires burn", 1, true, true).userValue;
                    monsterCompass = new MESMSetting<bool>("Monster Compass", "Makes compasses point towards the nearest monster", false).userValue;
                    spawnWithLiferaftItems = new MESMSetting<bool>("Spawn With Liferaft Items", "Starts a round with the mission items required to complete the liferaft objectives in your inventory", false).userValue;
                    spawnWithHelicopterItems = new MESMSetting<bool>("Spawn With Helicopter Items", "Starts a round with the mission items required to complete the helicopter objectives in your inventory", false, false, true).userValue;
                    spawnWithSubmersibleItems = new MESMSetting<bool>("Spawn With Submersible Items", "Starts a round with the mission items required to complete the Submersible objectives in your inventory", false, false, true).userValue;
                    loopingRadio = new MESMSetting<bool>("Looping Radio", "Makes the radio loop its playlist instead of playing only from the start of the first song when turned on after all tracks have played once", false).userValue;
                    betterSmashables = new MESMSetting<bool>("Better Smashables", "Makes smashables not break when they impact with only a small velocity", false).userValue;
                    unsmashables = new MESMSetting<bool>("Unsmashables", "Makes smashables never smash. Try holding the throw button! Just a fun setting", false, false, true).userValue;
                    addSmokeGrenade = new MESMSetting<bool>("Add Smoke Grenade", "Gives smashable bottles a 33% chance to spawn as a smoke grenade. When smashed, the bottle will emit smoke that can stun the monster like fire extinguisher powder and block its vision", false).userValue;
                    addMolotov = new MESMSetting<bool>("Add Molotov", "Gives smashable bottles a 33% chance to spawn as a molotov. When smashed, the bottle will spawn fire that can stun the monster like lit fuel", false).userValue;


                    // Read Colour Settings Variables
                    modSettingsErrorString = "Colour";
                    MESMSetting.currentCategoryBeingAssigned = modSettingsErrorString + " Settings";
                    MESMSettingRGB bruteLightColourRSetting = new MESMSettingRGB("Brute Light Colour Red Component", "Sets the red component of Brute's light", -1, false, false, -1, 255);
                    MESMSettingRGB bruteLightColourGSetting = new MESMSettingRGB("Brute Light Colour Green Component", "Sets the green component of Brute's light", -1, false, true, -1, 255);
                    MESMSettingRGB bruteLightColourBSetting = new MESMSettingRGB("Brute Light Colour Blue Component", "Sets the blue component of Brute's light", -1, false, true, -1, 255);
                    if (bruteLightColourRSetting.userValue == -1 || bruteLightColourGSetting.userValue == -1 || bruteLightColourBSetting.userValue == -1)
                    {
                        bruteLightColour = "Default";
                    }
                    else
                    {
                        bruteLightColour = bruteLightColourRSetting.userValue + "," + bruteLightColourGSetting.userValue + "," + bruteLightColourBSetting.userValue; //new Color(bruteLightColourRSetting.userValue / 255f, bruteLightColourGSetting.userValue / 255f, bruteLightColourBSetting.userValue / 255f, 1f);
                    }
                    //bruteLightColour = new MESMSettingString("Brute Light Colour, Intensity Multiplier & Range Multiplier", "Each colour can be customised by typing in a RGB value. If you do not want to use a custom colour, fill the row with the word Default. If you want to use the default light intensity or range, fill the row with 1. Example of the correct colour format: 0,150,255", "Default").userValue;
                    bruteLightIntensityMultiplier = new MESMSetting<float>("Brute Light Intensity Multiplier", "Multiplies the intensity of Brute's light", 1, false, true).userValue;
                    bruteLightRangeMultiplier = new MESMSetting<float>("Brute Light Range Multiplier", "Multiplies the range of Brute's light", 1, false, true).userValue;
                    randomBruteLightColours = new MESMSetting<bool>("Random Brute Light Colours", "Assigns each Brute's light a random colour", false, false, true).userValue;
                    MESMSettingRGB flashlightColourRSetting = new MESMSettingRGB("Flashlight Colour Red Component", "Sets the red component of flashlights", -1, false, false, -1, 255);
                    MESMSettingRGB flashlightColourGSetting = new MESMSettingRGB("Flashlight Colour Green Component", "Sets the green component of flashlights", -1, false, true, -1, 255);
                    MESMSettingRGB flashlightColourBSetting = new MESMSettingRGB("Flashlight Colour Blue Component", "Sets the blue component of flashlights", -1, false, true, -1, 255);
                    if (flashlightColourRSetting.userValue == -1 || flashlightColourGSetting.userValue == -1 || flashlightColourBSetting.userValue == -1)
                    {
                        flashlightColour = "Default";
                    }
                    else
                    {
                        flashlightColour = flashlightColourRSetting.userValue + "," + flashlightColourGSetting.userValue + "," + flashlightColourBSetting.userValue;
                    }
                    //flashlightColour = new MESMSettingString("Flashlight Colour, Intensity Multiplier & Range Multiplier", "Each colour can be customised by typing in a RGB value. If you do not want to use a custom colour, fill the row with the word Default. If you want to use the default light intensity or range, fill the row with 1. Example of the correct colour format: 0,150,255", "Default").userValue;
                    flashlightIntensityMultiplier = new MESMSetting<float>("Flashlight Intensity Multiplier", "Multiplies the intensity of flashlights", 1, false, true).userValue;
                    flashlightRangeMultiplier = new MESMSetting<float>("Flashlight Range Multiplier", "Multiplies the range of flashlights", 1, false, true).userValue;
                    randomFlashlightColours = new MESMSetting<bool>("Random Flashlight Colours", "Assigns each flashlight a random colour", false, false, true).userValue;
                    MESMSettingRGB glowstickColourRSetting = new MESMSettingRGB("Glowstick Colour Red Component", "Sets the red component of glowsticks", -1, false, false, -1, 255);
                    MESMSettingRGB glowstickColourGSetting = new MESMSettingRGB("Glowstick Colour Green Component", "Sets the green component of glowsticks", -1, false, true, -1, 255);
                    MESMSettingRGB glowstickColourBSetting = new MESMSettingRGB("Glowstick Colour Blue Component", "Sets the blue component of glowsticks", -1, false, true, -1, 255);
                    if (glowstickColourRSetting.userValue == -1 || glowstickColourGSetting.userValue == -1 || glowstickColourBSetting.userValue == -1)
                    {
                        glowstickColour = "Default";
                    }
                    else
                    {
                        glowstickColour = glowstickColourRSetting.userValue + "," + glowstickColourGSetting.userValue + "," + glowstickColourBSetting.userValue;
                    }
                    //glowstickColour = new MESMSettingString("Glowstick Colour, Intensity Multiplier & Range Multiplier", "Each colour can be customised by typing in a RGB value. If you do not want to use a custom colour, fill the row with the word Default. If you want to use the default light intensity or range, fill the row with 1. Example of the correct colour format: 0,150,255", "Default").userValue;
                    glowstickIntensityMultiplier = new MESMSetting<float>("Glowstick Intensity Multiplier", "Multiplies the intensity of glowsticks", 1, false, true).userValue;
                    glowstickRangeMultiplier = new MESMSetting<float>("Glowstick Range Multiplier", "Multiplies the range of glowsticks", 1, false, true).userValue;
                    randomGlowstickColours = new MESMSetting<bool>("Random Glowstick Colours", "Assigns each glowstick a random colour", false, false, true).userValue;
                    MESMSettingRGB shipGenericLightsColourRSetting = new MESMSettingRGB("Ship Generic Lights Colour Red Component", "Sets the red component of ship generic lights", -1, false, false, -1, 255);
                    MESMSettingRGB shipGenericLightsColourGSetting = new MESMSettingRGB("Ship Generic Lights Colour Green Component", "Sets the green component of ship generic lights", -1, false, true, -1, 255);
                    MESMSettingRGB shipGenericLightsColourBSetting = new MESMSettingRGB("Ship Generic Lights Colour Blue Component", "Sets the blue component of ship generic lights", -1, false, true, -1, 255);
                    if (shipGenericLightsColourRSetting.userValue == -1 || shipGenericLightsColourGSetting.userValue == -1 || shipGenericLightsColourBSetting.userValue == -1)
                    {
                        shipGenericLightsColour = "Default";
                    }
                    else
                    {
                        shipGenericLightsColour = shipGenericLightsColourRSetting.userValue + "," + shipGenericLightsColourGSetting.userValue + "," + shipGenericLightsColourBSetting.userValue;
                    }
                    //shipGenericLightsColour = new MESMSettingString("Ship Generic Lights Colour, Intensity Multiplier & Range Multiplier", "Each colour can be customised by typing in a RGB value. If you do not want to use a custom colour, fill the row with the word Default. If you want to use the default light intensity or range, fill the row with 1. Example of the correct colour format: 0,150,255", "Default").userValue;
                    shipGenericLightIntensityMultiplier = new MESMSetting<float>("Ship Generic Lights Intensity Multiplier", "Multiplies the intensity of ship generic lights", 1, false, true).userValue;
                    shipGenericLightRangeMultiplier = new MESMSetting<float>("Ship Generic Lights Range Multiplier", "Multiplies the range of ship generic lights", 1, false, true).userValue;
                    randomShipGenericLightsColours = new MESMSetting<bool>("Random Ship Generic Light Colours", "Assigns each light on the ship a random colour", false, false, true).userValue;
                    MESMSettingRGB fogColourRSetting = new MESMSettingRGB("Fog Colour Red Component", "Sets the red component of the fog colour", -1, false, false, -1, 255);
                    MESMSettingRGB fogColourGSetting = new MESMSettingRGB("Fog Colour Green Component", "Sets the green component of the fog colour", -1, false, true, -1, 255);
                    MESMSettingRGB fogColourBSetting = new MESMSettingRGB("Fog Colour Blue Component", "Sets the blue component of the fog colour", -1, false, true, -1, 255);
                    if (fogColourRSetting.userValue == -1 || fogColourGSetting.userValue == -1 || fogColourBSetting.userValue == -1)
                    {
                        fogColour = "Default";
                    }
                    else
                    {
                        fogColour = fogColourRSetting.userValue + "," + fogColourGSetting.userValue + "," + fogColourBSetting.userValue;
                    }


                    // Read Utility Settings
                    modSettingsErrorString = "Utility";
                    MESMSetting.currentCategoryBeingAssigned = modSettingsErrorString + " Settings";
                    enableMod = new MESMSetting<bool>("Enable Mod", "Disables the mod the next time the game is started. The mod must be enabled again by manually editing or deleting the modSettings file. This setting can be found by scrolling down to the " + MESMSetting.currentCategoryBeingAssigned + " category", true).userValue; // Check whether to enable the mod or not.
                    debugMode = new MESMSetting<bool>("Enable Debug Mode", "Use G to toggle invincibility / god mode. Enables noclip using U, H, J & K, which can be toggled on and off using V. While noclip is on you will receive no fall damage. Use R, T & Y/Z to spawn an additional Brute, Hunter and Fiend. Use B, N & M to force a chase, force stop a chase and burn hunters / force them to retreat. Use comma and period to activate and deactivate monsters. Use I, O & P to fix the liferaft, helicopter and submersible. Allows for cloning of a radio and two walkie talkies with L and fuses with semicolon [GB/US] or Ü [DE]. Use apostrophe or hashtag to teleport all items to you. Removes jump cooldown to allow moon jumping", false).userValue;
                    startedWithInvincibilityMode = new MESMSetting<bool>("Invincibility / God Mode In Debug Mode", "The monster cannot attack you, you receive no fall damage and steam does not damage you", false, false, true).userValue;
                    invincibilityMode = new List<bool>();
                    if (startedWithInvincibilityMode && debugMode)
                    {
                        for (int i = 0; i < NumberOfPlayers - numbersOfMonsterPlayers.Count; i++)
                        {
                            invincibilityMode.Add(true);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < NumberOfPlayers - numbersOfMonsterPlayers.Count; i++)
                        {
                            invincibilityMode.Add(false);
                        }
                    }
                    InvisibleMode = new MESMSetting<bool>("Invisibility In Debug Mode", "Monsters cannot see you", false, false, true).userValue;
                    WallhacksMode = new MESMSetting<bool>("Use \"Wallhacks\" In Debug Mode", "Most walls will be see-through (requires SSAO and Fog for full effect)", false, false, true).userValue;
                    BreakTheGameLight = new MESMSetting<bool>("Break The Game Lightly", "Continuously sets References.Monster to null after level generation is finished", false, false, true).userValue;
                    BreakTheGameHeavy = new MESMSetting<bool>("Break The Game Heavily", "Continuously sets References.Monster to null even before level generation is finished - Requires Break The Game Lightly to be enabled", false, false, true).userValue;
                    skipSplashScreen = new MESMSetting<bool>("Skip Splash Screen", "Will skip the splash screen on startup but not start a new round immediately", false).userValue;
                    skipMenuScreen = new MESMSetting<bool>("Skip Menu Screen", "Will skip the menu screen on startup so that a new round is automatically started without having to click on New Game. Causes visual bugs while loading", false, false, true).userValue;
                    alwaysSkipMenuScreen = new MESMSetting<bool>("Always Skip Menu Screen", "Will always skip the menu screen, even after finishing a round. Not recommended as it makes changing settings between rounds difficult and does not let access the main menu", false, false, true).userValue;
                    if (alwaysSkipMenuScreen)
                    {
                        skipMenuScreen = true;
                    }
                    // Assign a special variable so that the skipMenuScreen can be edited without affecting the skipping behaviour during the same play session.
                    if ((firstTimeReadingSettings && skipMenuScreen) || alwaysSkipMenuScreen)
                    {
                        skippedMenuScreen = false;
                    }
                    else
                    {
                        skippedMenuScreen = true;
                    }
                    if (enableMultiplayer && (!firstTimeReadingSettings || skipMenuScreen))
                    {
                        SetMultiplayerKeyBinds(MESMSetting.modSettings);
                    }
                    disableCustomLoadingText = new MESMSetting<bool>("Disable Custom Loading Screen Text", "The game will not tell you whether the mod is installed correctly during the loading screen. Not recommended", false).userValue;
                    timeScaleMultiplier = new MESMSetting<float>("Timescale", "Can make the game run slower or faster", 1).userValue;
                    useMonsterUpdateGroups = new MESMSetting<bool>("Use Monster Update Groups", "Divide multiple monster AIs up into several update groups, where only one group of monster AIs is updated fully per frame. Can significantly decrease CPU usage at the cost of reducing how often monster AIs update. Maximum number of groups is equal to the number of monsters being used. Very high values may actually decrease performance", false).userValue;
                    NumberOfMonsterUpdateGroups = new MESMSetting<int>("Number Of Monster Update Groups", "The number of monster update groups to use", 10, true, true).userValue;
                    doNotPreRenderMonsters = new MESMSetting<bool>("Do Not Pre-Render Monsters", "Can increase performance but can also cause monsters to be invisible for a short time when first coming into view of the player", false).userValue;
                    doNotRenderBruteLight = new MESMSetting<bool>("Do Not Render Brute's Light", "Turns off the light shafts Brute emits from their face. Can have a big impact on increasing performance when using many Brutes", false).userValue;
                    disableMonsterParticles = new MESMSetting<bool>("Disable Monster Particles", "Turns off the particles that the Brute and the Fiend emit. May have a small impact on increasing performance when using many Brutes and / or Fiends. Can be used to maximum effect with monster update groups", false).userValue;
                    monsterRadar = new MESMSetting<bool>("Monster Radar", "Shows the distance to the nearest monster. Cannot run at the same time as other text-displaying options", false).userValue;
                    glowstickRadar = new MESMSetting<bool>("Glowstick Radar", "Shows the distance to the nearest inactive glowstick. Quite laggy. Aids in testing Glowstick Hunt. Cannot run at the same time as other text-displaying options", false).userValue;
                    playerStaminaModeStaminaText = new MESMSetting<bool>("Player Stamina Mode Stamina Text", "Shows on the screen what your current running speed multiplier is. Aids in testing Stamina Mode. Cannot run at the same time as other text-displaying options", false).userValue;
                    playerRegionNodeText = new MESMSetting<bool>("Player Region Node Text", "Shows on the screen what region node the player is currently in. Aids in making level generation modifications. Cannot run at the same time as other text-displaying options", false).userValue;
                    useSpeedrunTimer = new MESMSetting<bool>("Speedrun Timer", "Logs how long the player has spent on a round. The time when the round starts, the player leaves the starter room and finishes the round are recorded", false).userValue;
                    speedrunTimer = new Timer();
                    speedrunTimer.IsRealtime = true;
                    finalTime = string.Empty;
                    showSpeedrunTimerOnScreen = new MESMSetting<bool>("Show Speedrun Timer On Screen", "Shows the speedrun timer on the screen on top of logging times. Cannot run at the same time as other text-displaying options", false).userValue;
                    if (showSpeedrunTimerOnScreen)
                    {
                        useSpeedrunTimer = true;
                    }
                    logDebugText = new MESMSetting<bool>("Log Debug Text", "Lets the game log a lot of extra information that lags the game but can be useful when fixing bugs", false).userValue;


                    // Initialise Other Variables Used In Code
                    modSettingsErrorString = "Other";
                    MESMSetting.currentCategoryBeingAssigned = modSettingsErrorString + " Settings";
                    if (debugMode && !numbersOfMonsterPlayers.Contains(0))
                    {
                        noclip = true;
                    }
                    else
                    {
                        noclip = false;
                    }
                    if (randomiserMode)
                    {
                        SettingsRandomiser();
                    }
                    errorWhileReadingModSettings = false;
                    errorDuringLevelGeneration = false;
                    spawnProtection = new List<bool>();
                    firstTimeReadingSettings = false;
                    glowstickHuntCounter = 0;
                    playerMovementSpeedStartMultiplier = playerMovementSpeedMultiplier[0];
                    playerMovementSpeedDynamicMultiplier = new List<float>();
                    staminaTimer = new List<float>();
                    for (int i = 0; i < NumberOfPlayers - numbersOfMonsterPlayers.Count; i++)
                    {
                        spawnProtection.Add(false);
                        playerMovementSpeedDynamicMultiplier.Add(playerMovementSpeedStartMultiplier);
                        staminaTimer.Add(0f);
                    }
                    finishedCreatingSimpleSparky = false;
                    // This is already done when creating variables.
                    StringBuilder settingsLogger = new StringBuilder("Listing all settings:\t ");
                    foreach (MESMSetting mESMSetting in allSettings)
                    {
                        settingsLogger.Append(mESMSetting.modSettingsText);
                        settingsLogger.Append(" = ");
                        settingsLogger.Append(mESMSetting.userValueString);
                        settingsLogger.Append("\t| ");
                    }
                    string settingsLog = settingsLogger.ToString();
                    Debug.Log(settingsLog.Substring(0, settingsLog.Length - 3));
                    if (!MESMSetting.modSettingsExist)
                    {
                        // Create the modSettings  file.
                        MESMSetting.SaveSettings();

                        // Read settings from the modSettings file to verify that reading works under normal circumstances.
                        firstTimeReadingSettings = true;
                        ReadModSettings();
                    }
                    Debug.Log("READ EXTENDED SETTINGS FROM FILE [" + version + "]");
                }
                catch (Exception e)
                {
                    errorWhileReadingModSettings = true;
                    Debug.Log("ERROR WHILE READING MOD SETTINGS:\n" + e.ToString());
                }
            }

            // @SettingsRandomiser
            private static void SettingsRandomiser() // In the future choosing which settings to randomise would help players choose which features they want to keep definite. This would mean no guessing is required.
            {
                try
                {
                    // Randomise Monster Settings
                    if (startedWithMMM)
                    {
                        numberOfRandomMonsters = (int)NumberRandomiser(1, 100f);
                        Debug.Log("Number of random monsters is " + numberOfRandomMonsters);
                        numberOfBrutes = 0;
                        numberOfHunters = 0;
                        numberOfFiends = 0;
                        numberOfMonsters = numberOfRandomMonsters;
                    }
                    minSpawnTime = NumberRandomiser(60f);
                    maxSpawnTime = NumberRandomiser(90f);
                    if (maxSpawnTime > 180f)
                    {
                        maxSpawnTime = 180f;
                    }
                    spawnMonsterInStarterRoom = UsageRandomiser(5f);
                    noMonsterStunImmunity = UsageRandomiser();
                    persistentMonster = UsageRandomiser(1f);
                    seerMonster = UsageRandomiser(2.5f);
                    monsterAlwaysFindsYou = UsageRandomiser(2.5f);
                    monsterAnimationSpeedMultiplier = NumberRandomiser(1f);
                    monsterMovementSpeedMultiplier = NumberRandomiser(1f);
                    varyingMonsterSizes = UsageRandomiser(5f);
                    customMonsterScale = NumberRandomiser(1f);
                    bruteChaseSpeedBuff = UsageRandomiser(20f);
                    bruteChaseSpeedBuffMultiplier = NumberRandomiser(1.35f);
                    applyChaseSpeedBuffToAllMonsters = UsageRandomiser(10f);
                    overpoweredHunter = UsageRandomiser(15f);
                    aggressiveHunter = UsageRandomiser(5f);
                    fiendFlickerMin = NumberRandomiser(3f);
                    fiendFlickerMed = fiendFlickerMin + NumberRandomiser(4.5f);
                    fiendFlickerMax = fiendFlickerMed + NumberRandomiser(6f);
                    fiendDoorTeleportation = UsageRandomiser(15f);
                    applyDoorTeleportationToAllMonsters = UsageRandomiser(5f);
                    //letAllMonstersLockDoors = UsageRandomiser(5f);
                    giveAllMonstersAFiendAura = UsageRandomiser(5f);
                    giveAllMonstersASmokeShroud = UsageRandomiser(3f);
                    smokeShroudRadius = NumberRandomiser(8f);
                    smokeShroudDangerRadiusFactor = NumberRandomiser(0.75f);
                    giveAllMonstersAFireShroud = UsageRandomiser(3f);
                    fireShroudRadius = NumberRandomiser(2f);
                    fireBlastRadius = NumberRandomiser(16f);

                    // Randomise Gamemode Settings
                    darkShip = UsageRandomiser(15f);
                    powerableLights = UsageRandomiser(75f);
                    glowstickHunt = UsageRandomiser(35f);
                    useCustomDoors = UsageRandomiser(5f);
                    if (useCustomDoors)
                    {
                        customDoorTypeNumber = UnityEngine.Random.Range(0, 7);
                    }
                    lightlyLockedDoors = UsageRandomiser(5f);
                    deathCountdown = NumberRandomiser(900f, 0.5f);
                    if (startedWithMMM)
                    {
                        itemMonsterFrenzy = UsageRandomiser(0.5f);
                        extraChaoticIMF = UsageRandomiser(0.25f);
                        monsterSpawnSpeedrunSpawnTime = NumberRandomiser(180f, 0.5f);
                    }
                    foggyShip = UsageRandomiser(1f);
                    fogDistance = NumberRandomiser(8f);
                    monsterVisionAffectedByFog = UsageRandomiser(10f);
                    foggyShipAlternativeMode = UsageRandomiser(1f);
                    smokyShip = UsageRandomiser(3f);
                    alwaysSmoky = UsageRandomiser(1f);
                    breathAmount = NumberRandomiser(20f);
                    breathRecovery = NumberRandomiser(2f);
                    smokeShroudBreathDrain = NumberRandomiser(2f);
                    smokeCorridorBreathDrain = NumberRandomiser(1f);

                    // Do not randomise Player Settings. These should be chosen by the player.
                    inventorySize = (int)NumberRandomiser(6); // Should this be in item or player settings? This used to be in item settings, so perhaps this is the exception.

                    // Randomise Level Generation Settings
                    spawnDeactivatedItems = UsageRandomiser();
                    noCameras = UsageRandomiser(15f);
                    noSteam = UsageRandomiser(10f);
                    allPreFilledFuseBoxes = UsageRandomiser(20f);
                    noPreFilledFuseBoxes = UsageRandomiser(25f);
                    noBarricadedDoors = UsageRandomiser(10f);
                    overpoweredSteamVents = UsageRandomiser(5f);
                    unbreakablePitTraps = UsageRandomiser(7.5f);
                    diverseItemSpawns = UsageRandomiser(35f);

                    // Randomise Item Settings
                    flareLifetime = NumberRandomiser(10f, true);
                    spawnWithFlareGun = UsageRandomiser(10f);
                    overpoweredFlareGun = UsageRandomiser(5f);
                    fireDurationMultiplier = NumberRandomiser(1f, true);
                    monsterCompass = UsageRandomiser(7.5f);

                    // Randomise Colour & Light Settings
                    if (UsageRandomiser(10f))
                    {
                        bruteLightColour = string.Concat(new object[] { UnityEngine.Random.Range(0f, 255f), ",", UnityEngine.Random.Range(0f, 255f), ",", UnityEngine.Random.Range(0f, 255f) });
                    }
                    bruteLightIntensityMultiplier = NumberRandomiser(1f, true);
                    bruteLightRangeMultiplier = NumberRandomiser(1f, true);
                    randomBruteLightColours = UsageRandomiser(5f);
                    if (UsageRandomiser(10f))
                    {
                        flashlightColour = string.Concat(new object[] { UnityEngine.Random.Range(0f, 255f), ",", UnityEngine.Random.Range(0f, 255f), ",", UnityEngine.Random.Range(0f, 255f) });
                    }
                    flashlightIntensityMultiplier = NumberRandomiser(1f, true);
                    flashlightRangeMultiplier = NumberRandomiser(1f, true);
                    randomFlashlightColours = UsageRandomiser(5f);
                    if (UsageRandomiser())
                    {
                        glowstickColour = string.Concat(new object[] { UnityEngine.Random.Range(0f, 255f), ",", UnityEngine.Random.Range(0f, 255f), ",", UnityEngine.Random.Range(0f, 255f) });
                    }
                    glowstickIntensityMultiplier = NumberRandomiser(1f, true);
                    glowstickRangeMultiplier = NumberRandomiser(1f, true);
                    randomGlowstickColours = UsageRandomiser(10f);
                    if (UsageRandomiser(5f))
                    {
                        shipGenericLightsColour = string.Concat(new object[] { UnityEngine.Random.Range(0f, 255f), ",", UnityEngine.Random.Range(0f, 255f), ",", UnityEngine.Random.Range(0f, 255f) });
                    }
                    shipGenericLightIntensityMultiplier = NumberRandomiser(1f, true);
                    shipGenericLightRangeMultiplier = NumberRandomiser(1f, true);
                    randomShipGenericLightsColours = UsageRandomiser(2.5f);
                }
                catch
                {
                    Debug.Log("ERROR WHILE USING RANDOMISER");
                }
            }

            // @UsageRandomiser
            private static bool UsageRandomiser()
            {
                return UsageRandomiser(UnityEngine.Random.value * 100f); // Should this maybe just be 50% instead?
            }

            private static bool UsageRandomiser(float chance)
            {
                chance += (chaosMultiplier - 1f) * UnityEngine.Random.Range(0f, 10f); // Adds more chance depending on the chaos multiplier.
                chance += UnityEngine.Random.value * UnityEngine.Random.Range(0f, chance / 2) * RandomSign(); // How exactly does this line work?
                return (UnityEngine.Random.value * 100f) <= chance;
            }

            // @NumberRandomiser
            private static float NumberRandomiser(bool canBeZero)
            {
                return NumberRandomiser(UnityEngine.Random.value * chaosMultiplier, canBeZero);
            }

            private static float NumberRandomiser(float baseNumber, bool canBeZero = false)
            {
                return NumberRandomiser(baseNumber, UnityEngine.Random.value * 100f, canBeZero);
            }

            private static float NumberRandomiser(float baseNumber, float baseChance, bool canBeZero = false)
            {
                if (UsageRandomiser(baseChance))
                {
                    float randomRange = UnityEngine.Random.Range(0f, chaosMultiplier);
                    int randomPower = (int)Math.Round(randomRange);
                    float randomMultiplier = randomRange % 1;
                    float randomNumber = Mathf.Abs(baseNumber + baseNumber * UnityEngine.Random.value * (float)Math.Pow(chaosMultiplier, randomPower) * randomMultiplier * RandomSign()); // How does this random number tend to relate to the base number?
                    Debug.Log("Random range is " + randomRange + " and random power is " + randomPower + " and random multiplier is " + randomMultiplier + " and random number is " + randomNumber);
                    if (!canBeZero && randomNumber < 1f)
                    {
                        return randomNumber + 1f;
                    }
                    else
                    {
                        return randomNumber;
                    }
                }
                else
                {
                    if (!canBeZero && baseNumber < 1f)
                    {
                        return baseNumber + 1f;
                    }
                    else
                    {
                        return baseNumber;
                    }
                }
            }

            // @RandomSign
            private static int RandomSign()
            {
                // Random.sign - BrettFromLA - https://forum.unity.com/threads/random-sign.27218/ - Accessed 17.08.2020
                return (int)(UnityEngine.Random.Range(0, 2) - 0.5) * 2;
            }

            // @AssignTrueBoolOnlyWhenFirstTimeReadingSettings
            private static void AssignTrueBoolOnlyWhenFirstTimeReadingSettings(ref bool passedBool, bool passedValue)
            {
                // bool starts as false. Hence, if it was true before, it doesn't need to be updated again.
                if (ModSettings.firstTimeReadingSettings && passedBool == false)
                {
                    passedBool = passedValue;
                }
            }

            // #ReadBeforeGeneration
            public static void ReadBeforeGeneration()
            {
                if (enableMultiplayer)
                {
                    MultiplayerMode.MultiplayerModeVariableInitialisation();
                }
                if (useSparky)
                {
                    SparkyMode.SparkyModeBeforeGenerationInitialisation();
                }
            }

            // #ReadAfterGeneration
            public static void ReadAfterGeneration()
            {
                Debug.Log("READING LATE EXTENDED SETTINGS (AFTER GENERATION INITIALISATION)");
                Debug.LogError("READING LATE EXTENDED SETTINGS (AFTER GENERATION INITIALISATION)");

                if (!ModSettings.alwaysSkipMenuScreen)
                {
                    ModSettings.skippedMenuScreen = true;
                }

                if (foggyShip)
                {
                    // Default Variables: GlobalFog variables: CAMERA_NEAR = 0.03 | CAMERA_FAR = 50 | startDistance = 9 | height = 0 | heightScale = 100 | globalFogColor = RGBA(0.048, 0.059, 0.057, 1.000)
                    //Debug.Log("Using fog script");
                    //string fogStringStart = "Enabled: " + RenderSettings.fog + " - FogColor: " + RenderSettings.fogColor + " - FogDensity: " + RenderSettings.fogDensity + " - FogMode: " + RenderSettings.fogMode.ToString() + " - Start Distance: " + RenderSettings.fogStartDistance + " - End Distance: " + RenderSettings.fogEndDistance;
                    Camera mainCamera = GameObject.Find("CameraMain").GetComponent<Camera>();
                    GlobalFog globalFog = (GlobalFog)FindObjectOfType<QualitySettingsScript>().gameObject.GetComponent("GlobalFog");
                    globalFog.startDistance = fogDistance / 2f;//(9f / 50f) * fogDistance;
                    if (UseCustomColour(ModSettings.fogColour))
                    {
                        globalFog.globalFogColor = ConvertColourStringToColour(ModSettings.fogColour);
                    }
                    else
                    {
                        globalFog.globalFogColor = new Color(0.137f, 0.137f, 0.137f); // PS4//new Color(0.035f, 0.055f, 0.043f); //Switch // It is not grey by default.
                    }
                    globalFog.globalDensity = 50f;
                    mainCamera.farClipPlane = fogDistance;
                    mainCamera.clearFlags = CameraClearFlags.Color;
                    mainCamera.backgroundColor = globalFog.globalFogColor;
                    globalFog.fogMode = GlobalFog.FogMode.RelativeYAndDistance;
                }
                if (foggyShipAlternativeMode)
                {
                    if (ModSettings.enableMultiplayer)
                    {
                        foreach (NewPlayerClass newPlayerClass in MultiplayerMode.newPlayerClasses)
                        {
                            newPlayerClass.gameObject.AddComponent<DustFog>();
                        }
                    }
                    else
                    {
                        References.Player.AddComponent<DustFog>();
                    }
                }

                if (customPlayerScale != 1f)
                {
                    References.Player.transform.localScale = new Vector3(customPlayerScale, customPlayerScale, customPlayerScale);
                }

                if (startedWithMMM)
                {
                    ManyMonstersMode.ManyMonstersModeAfterGenerationInitialisation();
                }

                if (enableMultiplayer)
                {
                    MultiplayerMode.MultiplayerModeAfterGenerationInitialisation();
                    if (enableCrewVSMonsterMode)
                    {
                        CrewVsMonsterMode.CrewVsMonsterModeAfterGenerationInitialisation();
                    }
                }

                if (useSparky)
                {
                    SparkyMode.SparkyModeAfterGenerationInitialisation();
                }
                if (sparkyWithModel)
                {
                    SparkyMode.SimpleSparkyModelTest();
                }
                if (useSmokeMonster)
                {
                    //SmokeMonster.SmokeMonsterAfterGenerationInitialisation();
                }

                if (allPreFilledFuseBoxes/* || darkShip*/)
                {
                    /*
                    FuseBox[] fuseBoxes = FuseBoxManager.instance.fuseboxes.Values.ToList<FuseBox>().ToArray();
                    foreach (FuseBox fuseBox in fuseBoxes)
                    {
                        FuseBoxManager.instance.SetupFuse(fuseBox);
                    }
                    */
                    FuseBox[] allFuseBoxes = UnityEngine.Object.FindObjectsOfType<FuseBox>();
                    if (allPreFilledFuseBoxes)
                    {
                        foreach (FuseBox fuseBox in allFuseBoxes)
                        {
                            FuseBoxManager.instance.SetupFuse(fuseBox);
                        }
                    }
                    /*
                    if (darkShip)
                    {
                        foreach (FuseBox fuseBox in allFuseBoxes)
                        {
                            if (fuseBox.powered)
                            {
                                fuseBox.transform.parent.GetComponentInChildren<FuseBoxLever>().PullLever(false); // Just stop starting fuses from activating levers after generation.
                            }
                        }
                    }
                    */
                }

                if (noBarricadedDoors)
                {
                    Room[] rooms = LevelGeneration.Instance.RoomsInUse.ToArray();
                    foreach (Room room in rooms)
                    {
                        Door[] doors = room.roomDoors.ToArray();
                        foreach (Door door in doors)
                        {
                            if (door.DoorType == Door.doorType.Barricaded)
                            {
                                door.RipOffDoor2();
                            }
                        }
                    }
                }

                if (debugMode)
                {
                    if (noclip)
                    {
                        if (!enableMultiplayer)
                        {
                            NewPlayerClass.Instance.playerMotor.allowFallDamage = false;
                        }
                        else
                        {
                            foreach (NewPlayerClass newPlayerClass in MultiplayerMode.newPlayerClasses)
                            {
                                newPlayerClass.Motor.allowFallDamage = false;
                            }
                        }
                    }
                }

                if (useCustomDoors && lightlyLockedDoors)
                {
                    UnityEngine.Object.Instantiate<GameObject>(UnityEngine.Object.FindObjectOfType<Welder>().gameObject, References.Player.transform.position + References.Player.transform.up + References.Player.transform.forward, References.Player.transform.rotation);
                }

                if (spawnWithLiferaftItems || spawnWithHelicopterItems || spawnWithSubmersibleItems)
                {
                    int slotsRequired = 0;

                    if (spawnWithLiferaftItems)
                    {
                        slotsRequired += 3;
                        if (noPreFilledFuseBoxes)
                        {
                            slotsRequired++;
                        }
                    }
                    if (spawnWithHelicopterItems)
                    {
                        slotsRequired += 4;
                    }
                    if (spawnWithSubmersibleItems)
                    {
                        slotsRequired += 4;
                    }

                    if (slotsRequired > References.Inventory.maxInventoryCapacity)
                    {
                        int startingSlotCount = References.Inventory.maxInventoryCapacity;
                        for (int bonusSlot = 0; bonusSlot < slotsRequired - startingSlotCount; bonusSlot++)
                        {
                            References.Inventory.CreateSlot(bonusSlot + startingSlotCount);
                            References.Inventory.maxInventoryCapacity++;
                        }
                    }

                    InventoryItem[] allInventoryItems = UnityEngine.Object.FindObjectsOfType<InventoryItem>();
                    if (spawnWithLiferaftItems)
                    {
                        FindItemWithSpecificName("Chain spool").AddToInventory();
                        FindItemWithSpecificName("DuctTape").AddToInventory();
                        FindItemWithSpecificName("Pump").AddToInventory();
                        if (noPreFilledFuseBoxes)
                        {
                            FindItemWithSpecificName("Fuse").AddToInventory();
                        }
                    }

                    if (spawnWithHelicopterItems)
                    {
                        FindItemWithSpecificName("Helicopter Keys").AddToInventory();
                        FindItemWithSpecificName("BoltCutters").AddToInventory();
                        FindItemWithSpecificName("Gasoline Canister").AddToInventory();
                        FindItemWithSpecificName("Gasoline Canister").AddToInventory();
                    }

                    if (spawnWithSubmersibleItems)
                    {
                        FindItemWithSpecificName("Welding Kit").AddToInventory();
                        FindItemWithSpecificName("Sub HeadLights").AddToInventory();
                        FindItemWithSpecificName("Sub Battery").AddToInventory();
                        FindItemWithSpecificName("Fuse").AddToInventory();
                    }
                }

                if (glowstickHunt || randomGlowstickColours)
                {
                    GlowStick[] allGlowsticks = FindObjectsOfType<GlowStick>();
                    List<int> usedGlowstickIndices = new List<int>();
                    List<int> ignoredGlowstickIndices = new List<int>();
                    int glowsticksEdited = 0;

                    if (glowstickHunt)
                    {
                        glowstickHuntColours = new List<Color>();
                        glowstickHuntColours.AddRange(new Color[] { new Color(0.561f, 0f, 0.996f, 1f) /*Purple*/, Color.blue, Color.green, Color.yellow, new Color(0.996f, 0.631f, 0f, 1f) /*Orange*/, Color.red }); // Custom colours from here https://answers.unity.com/questions/785696/global-list-of-colour-names-and-colour-values.html .

                        // Shuffle the glowstick colours.
                        // How can i shuffle a list - sona.viswam - https://answers.unity.com/questions/486626/how-can-i-shuffle-alist.html - Accessed 24.08.2021
                        for (int i = 0; i < glowstickHuntColours.Count; i++)
                        {
                            Color temp = glowstickHuntColours[i];
                            int randomIndex = UnityEngine.Random.Range(i, glowstickHuntColours.Count);
                            glowstickHuntColours[i] = glowstickHuntColours[randomIndex];
                            glowstickHuntColours[randomIndex] = temp;
                        }

                        if (specialGlowsticksToCreate < specialGlowsticksRequired)
                        {
                            specialGlowsticksToCreate = specialGlowsticksRequired;
                        }

                        while (glowsticksEdited < specialGlowsticksToCreate)
                        {
                            // Choose a random glowstick and give it one of the rainbow colours if that glowstick wasn't already chosen.
                            int randomIndex = UnityEngine.Random.Range(0, allGlowsticks.Length);

                            if (specialGlowsticksToCreate > allGlowsticks.Length - ignoredGlowstickIndices.Count)
                            {
                                specialGlowsticksToCreate = allGlowsticks.Length - ignoredGlowstickIndices.Count;
                            }

                            if (!usedGlowstickIndices.Contains(randomIndex) && !ignoredGlowstickIndices.Contains(randomIndex))
                            {
                                NodeData nodeDataAtGlowstickPosition = LevelGeneration.GetNodeDataAtPosition(allGlowsticks[randomIndex].transform.position);
                                if (nodeDataAtGlowstickPosition != null && nodeDataAtGlowstickPosition.nodeRoom != null && !(nodeDataAtGlowstickPosition.nodeRoom.roomDoorData != null && nodeDataAtGlowstickPosition.nodeRoom.roomDoorData.Count == 1 && (nodeDataAtGlowstickPosition.nodeRoom.roomDoorData[0].connection == ConnectionType.Powered || nodeDataAtGlowstickPosition.nodeRoom.roomDoorData[0].connection == ConnectionType.Barricade)))
                                {
                                    BaseFeatures.AssignCustomGlowstickColour(allGlowsticks[randomIndex], glowstickHuntColours[glowsticksEdited % glowstickHuntColours.Count], true);
                                    usedGlowstickIndices.Add(randomIndex);
                                    glowsticksEdited++;
                                }
                                else
                                {
                                    ignoredGlowstickIndices.Add(randomIndex);
                                }
                            }
                        }
                    }

                    if (randomGlowstickColours)
                    {
                        while (glowsticksEdited < allGlowsticks.Length)
                        {
                            // Choose a random glowstick and give it a random colour if that glowstick wasn't already chosen.
                            int randomIndex = UnityEngine.Random.Range(0, allGlowsticks.Length);
                            if (!usedGlowstickIndices.Contains(randomIndex))
                            {
                                BaseFeatures.AssignCustomGlowstickColour(allGlowsticks[randomIndex], new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0.5f, 1f)), true); // UnityEngine.Random.ColorHSV() is also available, but does not produce as colourful colours.
                                usedGlowstickIndices.Add(randomIndex);
                                glowsticksEdited++;
                            }
                        }
                    }
                }

                if (ModSettings.spawnWithFlareGun)
                {
                    foreach (ItemCollection itemCollection in UnityEngine.Object.FindObjectOfType<SpawnPrefabs>().itemCollections)
                    {
                        foreach (SpawnItem spawnItem in itemCollection.items)
                        {
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("Spawn item is: " + spawnItem.prefab.name);
                            }
                        }
                    }
                    if (ModSettings.enableMultiplayer)
                    {
                        InventoryItem[] flareGuns = new InventoryItem[MultiplayerMode.newPlayerClasses.Count];
                        for (int i = 0; i < MultiplayerMode.newPlayerClasses.Count; i++)
                        {
                            if (i == 0)
                            {
                                flareGuns[i] = UnityEngine.Object.Instantiate<GameObject>(UnityEngine.Object.FindObjectOfType<FlareGun>().gameObject, MultiplayerMode.newPlayerClasses[i].transform.position + MultiplayerMode.newPlayerClasses[i].transform.up + MultiplayerMode.newPlayerClasses[i].transform.forward * 0.5f, MultiplayerMode.newPlayerClasses[i].transform.rotation).GetComponent<InventoryItem>(); // UnityEngine.Object.Instantiate<FlareGun>(UnityEngine.Object.FindObjectOfType<FlareGun>(), MultiplayerMode.newPlayerClasses[i].transform.position + MultiplayerMode.newPlayerClasses[i].transform.up + MultiplayerMode.newPlayerClasses[i].transform.forward, MultiplayerMode.newPlayerClasses[i].transform.rotation).item;
                            }
                            else
                            {
                                flareGuns[i] = Instantiate(flareGuns[0]);
                            }
                        }
                        for (int i = 0; i < MultiplayerMode.newPlayerClasses.Count; i++)
                        {
                            if (!MultiplayerMode.inventories[i].HasSpaceFor(flareGuns[i]))
                            {
                                MultiplayerMode.inventories[i].CreateSlot(1);
                                MultiplayerMode.inventories[i].maxInventoryCapacity++;
                            }
                            //InventoryItem.inventory = MultiplayerMode.inventories[i];
                            //flareGuns[i].AddToInventory();
                        }
                    }
                    else
                    {
                        InventoryItem flareGun = UnityEngine.Object.Instantiate<GameObject>(UnityEngine.Object.FindObjectOfType<FlareGun>().gameObject, References.Player.transform.position + References.Player.transform.up + References.Player.transform.forward * 0.5f, References.Player.transform.rotation).GetComponent<InventoryItem>(); // UnityEngine.Object.Instantiate<FlareGun>(UnityEngine.Object.FindObjectOfType<FlareGun>(), References.Player.transform.position + References.Player.transform.up + References.Player.transform.forward, References.Player.transform.rotation).item;
                        if (!References.Inventory.HasSpaceFor(flareGun))
                        {
                            References.Inventory.CreateSlot(1);
                            References.Inventory.maxInventoryCapacity++;
                        }
                        //flareGun.AddToInventory();
                    }
                }

                if (playerStaminaMode)
                {
                    playerMovementSpeedEndMultiplier = playerMovementSpeedMultiplier[0] * ((References.PlayerClass.Motor.walkX + playerStaminaModeSpeedPenaltyMultiplier * (References.PlayerClass.Motor.runX - References.PlayerClass.Motor.walkX)) / References.PlayerClass.Motor.runX);

                    staminaModeMaximumMultiplierChange = (playerMovementSpeedStartMultiplier - playerMovementSpeedEndMultiplier);

                    /*
                    float inner1 = (playerMovementSpeedEndMultiplier - playerMovementSpeedStartMultiplier) / staminaModeMaximumMultiplierChange;
                    float inner2 = Mathf.Exp(playerStaminaModeStaminaDuration) / (playerMovementSpeedEndMultiplier + staminaModeMaximumMultiplierChange * Mathf.Exp(playerStaminaModeStaminaDuration));
                    float inner3 = 1f / (inner1 + inner2);
                    float inner4 = inner3 - staminaModeMaximumMultiplierChange;
                    float inner5 = 1f / inner4;
                    float inner6 = playerMovementSpeedEndMultiplier * inner5;
                    float inner7 = Mathf.Log(inner6);
                    staminaModeHorizontalShiftConstant = playerStaminaModeStaminaDuration + inner7;

                    float inner8 = Mathf.Exp(staminaModeHorizontalShiftConstant) / (playerMovementSpeedEndMultiplier + staminaModeMaximumMultiplierChange * Mathf.Exp(staminaModeHorizontalShiftConstant));
                    float inner9 = (playerMovementSpeedStartMultiplier - staminaModeMaximumMultiplierChange * inner8) / 2f;
                    staminaModeVerticalShiftConstant = (playerMovementSpeedStartMultiplier / 2f) + inner9;

                    staminaModeTimeCoefficient = 5f / playerStaminaModeStaminaDuration;
                    staminaModeVerticalScalingCoefficient = staminaModeMaximumMultiplierChange / 2f;
                    */
                    staminaModeTimeCoefficient = 5f / playerStaminaModeHalfStaminaDuration;
                }

                if (ModSettings.addAdditionalCrewDeckBuilding)
                {
                    HookCondition[] hookConditions = FindObjectsOfType<HookCondition>();
                    CraneChain[] craneChains = FindObjectsOfType<CraneChain>();
                    CraneHook[] craneHooks = FindObjectsOfType<CraneHook>();
                    for (int i = 0; i < hookConditions.Length; i++)
                    {
                        craneChains[i].hookCondition = hookConditions[i];
                        craneHooks[i].hookCondition = hookConditions[i];
                    }


                    liferafts = FindObjectsOfType<Liferaft>();
                    cranes = FindObjectsOfType<Crane>();
                    EscapeLifeRaft[] escapeLifeRafts = FindObjectsOfType<EscapeLifeRaft>();
                    for (int i = 0; i < liferafts.Length && i < cranes.Length && i < escapeLifeRafts.Length; i++)
                    {
                        //Debug.Log("Setting up liferaft set " + (i + 1));
                        cranes[i].lifeRaft = liferafts[i];
                        cranes[i].escapeLifeRaft = escapeLifeRafts[i];
                        cranes[i].escapeLifeRaft.liferaft = liferafts[i];
                    }
                }

                foreach (FuseBox fuseBox in ModSettings.additionalFuseBoxesToSetUpAfterGeneration)
                {
                    if (fuseBox.powerRegion == PrimaryRegionType.OuterDeck || (fuseBox.powerRegion == PrimaryRegionType.CrewDeck && LevelGeneration.Instance.StartRoom.PrimaryRegion == PrimaryRegionType.CrewDeck) || (fuseBox.powerRegion == PrimaryRegionType.LowerDeck && LevelGeneration.Instance.StartRoom.PrimaryRegion == PrimaryRegionType.LowerDeck))
                    {
                        FuseBoxManager.Instance.SetupFuse(fuseBox);
                    }
                }

                if (giveAllMonstersAFiendAura)
                {
                    if (startedWithMMM)
                    {
                        foreach (Monster monster in ManyMonstersMode.monsterListMonsterComponents)
                        {
                            if (monster.MonsterType != Monster.MonsterTypeEnum.Fiend && !monster.monsterType.Equals("Sparky"))
                            {
                                GiveMonsterFiendAuraAndDisruptor(monster).enabled = true;
                                TimeScaleManager.Instance.StartCoroutine(SparkyMode.UpdateFiendDisruptorAfterAFrame(monster.GetComponent<FiendLightDisruptor>()));
                            }
                        }
                    }
                    else
                    {
                        Monster monster = References.Monster.GetComponent<Monster>();
                        if (monster.MonsterType != Monster.MonsterTypeEnum.Fiend && !monster.monsterType.Equals("Sparky"))
                        {
                            FiendAura fiendAura = GiveMonsterFiendAuraAndDisruptor(monster);
                            fiendAura.enabled = true;
                            FindObjectOfType<FiendLightController>().aura = fiendAura;
                            FiendLightDisruptor fiendLightDisruptor = monster.GetComponent<FiendLightDisruptor>();
                            TimeScaleManager.Instance.StartCoroutine(SparkyMode.UpdateFiendDisruptorAfterAFrame(fiendLightDisruptor));
                        }
                    }
                }

                if (giveAllMonstersASmokeShroud || smokyShip)
                {
                    SmokeShroud.allSmokeShrouds = new List<SmokeShroud>();

                    if (ModSettings.enableMultiplayer)
                    {
                        foreach (NewPlayerClass newPlayerClass in MultiplayerMode.crewPlayers)
                        {
                            newPlayerClass.gameObject.AddComponent<FogDamageTracker>();
                        }
                    }
                    else
                    {
                        References.Player.AddComponent<FogDamageTracker>();
                    }

                    if (giveAllMonstersASmokeShroud)
                    {
                        if (ModSettings.startedWithMMM)
                        {
                            foreach (GameObject monsterGameObject in ManyMonstersMode.monsterList)
                            {
                                monsterGameObject.AddComponent<SmokeShroud>();
                            }
                        }
                        else
                        {
                            References.Monster.AddComponent<SmokeShroud>();
                        }
                    }
                    if (smokyShip)
                    {
                        foreach (PrimaryRegionType primaryRegionType in Enum.GetValues(typeof(PrimaryRegionType)))
                        {
                            if (FuseBoxManager.Instance.rooms.ContainsKey(primaryRegionType))
                            {
                                // Add smoke emitters to all corridors.
                                List<Room> allRooms = FuseBoxManager.Instance.rooms[primaryRegionType];
                                foreach (Room room in allRooms)
                                {
                                    if (room.RoomType == RoomStructure.Corridor)
                                    {
                                        SmokeMonster.CreateSmokeEmitter(room);
                                    }
                                }
                            }
                        }
                    }
                }
                if (giveAllMonstersAFireShroud)
                {
                    if (ModSettings.startedWithMMM)
                    {
                        foreach (GameObject monsterGameObject in ManyMonstersMode.monsterList)
                        {
                            monsterGameObject.AddComponent<FireShroud>();
                        }
                    }
                    else
                    {
                        References.Monster.AddComponent<FireShroud>();
                    }
                }

                if (monstersSearchRandomly)
                {
                    List<int> allowedRegionsIndices = new List<int>();

                    allowedRegionsIndices.AddRange(new int[] { 1, 2, 4, 5, 7, 9, 10, 13, 15, 18, 28, 31, 39 });
                    accessibleNodes = new List<Vector3>();
                    for (int i = 1; i < RegionManager.Instance.regions.Count; i++)
                    {
                        //Debug.Log("Region with index " + i + " has name " + RegionManager.Instance.regions[i].regionName + " and ID " + RegionManager.Instance.regions[i].regionID);
                        if (allowedRegionsIndices.Contains(i))
                        {
                            //Debug.Log("Adding nodes of region " + RegionManager.Instance.regions[i].regionName);
                            for (int j = 0; j < RegionManager.Instance.regions[i].associatedNodes.Count; j++)
                            {
                                if (!accessibleNodes.Contains(RegionManager.Instance.regions[i].associatedNodes[j]))
                                {
                                    //Debug.Log("Adding node: " + RegionManager.Instance.regions[i].associatedNodes[j]);
                                    accessibleNodes.Add(RegionManager.Instance.regions[i].associatedNodes[j]);
                                }
                            }
                        }
                    }
                }
                Debug.Log("READ LATE EXTENDED SETTINGS (AFTER GENERATION INITIALISATION)");
                Debug.LogError("READ LATE EXTENDED SETTINGS (AFTER GENERATION INITIALISATION)");
            }

            // @FindItemWithSpecificName
            public static InventoryItem FindItemWithSpecificName(string passedItemName, InventoryItem[] allInventoryItems = null)
            {
                if (allInventoryItems == null)
                {
                    allInventoryItems = UnityEngine.Object.FindObjectsOfType<InventoryItem>();
                }
                foreach (InventoryItem inventoryItem in allInventoryItems)
                {
                    if (inventoryItem.itemName.Equals(passedItemName))
                    {
                        return inventoryItem;
                    }
                }
                Debug.Log("Could not find InventoryItem with name " + passedItemName + ". Returning first item of all items found.");
                return allInventoryItems[0];
            }

            /*
            InventoryItem.itemName s:

            BoltCutters
            Chain spool
            DuctTape
            Egg Timer
            Fire Extinguisher
            Flashlight
            Fuse
            Gasoline Canister
            Glowstick
            Helicopter Keys
            Lighter
            Pump
            Radio
            Smashable
            Sub Battery
            Sub HeadLights
            Welding Kit
            */

            public static void SetInvincibilityMode(bool value, bool setForAllPlayers)
            {
                if (ModSettings.enableMultiplayer)
                {
                    for (int crewPlayerIndex = 0; crewPlayerIndex < MultiplayerMode.crewPlayers.Count; crewPlayerIndex++)
                    {
                        SetInvincibilityMode(value, crewPlayerIndex);
                    }
                }
                else
                {
                    SetInvincibilityMode(value);
                }
            }

            public static void SetInvincibilityMode(bool value, NewPlayerClass newPlayerClass)
            {
                if (ModSettings.enableMultiplayer)
                {
                    SetInvincibilityMode(value, MultiplayerMode.crewPlayers.IndexOf(newPlayerClass));
                }
                else
                {
                    SetInvincibilityMode(value);
                }
            }

            public static void SetInvincibilityMode(bool value, int crewPlayerIndex = 0)
            {
                if (startedWithInvincibilityMode)
                {
                    invincibilityMode[crewPlayerIndex] = true;
                }
                else if (ModSettings.debugMode || spawnProtection[crewPlayerIndex])
                {
                    invincibilityMode[crewPlayerIndex] = value;
                }
                else
                {
                    invincibilityMode[crewPlayerIndex] = false;
                }
            }

            public static bool InvisibleMode
            {
                get
                {
                    return invisibleMode;
                }
                set
                {
                    if (ModSettings.debugMode)
                    {
                        invisibleMode = value;
                    }
                    else
                    {
                        invisibleMode = false;
                    }
                }
            }

            // @WallhacksMode
            public static bool WallhacksMode
            {
                get
                {
                    return wallhacksMode;
                }
                set
                {
                    if (ModSettings.debugMode)
                    {
                        wallhacksMode = value;
                    }
                    else
                    {
                        wallhacksMode = false;
                    }
                }
            }

            // @BreakTheGameLight
            public static bool BreakTheGameLight
            {
                get
                {
                    return breakTheGameLight;
                }
                set
                {
                    if (ModSettings.debugMode)
                    {
                        breakTheGameLight = value;
                    }
                    else
                    {
                        breakTheGameLight = false;
                    }
                }
            }

            // @BreakTheGameHeavy
            public static bool BreakTheGameHeavy
            {
                get
                {
                    return breakTheGameHeavy;
                }
                set
                {
                    if (ModSettings.BreakTheGameLight)
                    {
                        breakTheGameHeavy = value;
                    }
                    else
                    {
                        breakTheGameHeavy = false;
                    }
                }
            }

            // @NumberOfMonsterUpdateGroups
            public static int NumberOfMonsterUpdateGroups
            {
                get
                {
                    return numberOfMonsterUpdateGroups;
                }
                set
                {
                    if (value < numberOfMonsters)
                    {
                        numberOfMonsterUpdateGroups = value;
                    }
                    else
                    {
                        numberOfMonsterUpdateGroups = numberOfMonsters;
                    }
                }
            }

            // @NumberOfPlayers
            public static int NumberOfPlayers
            {
                get
                {
                    return numberOfPlayers;
                }
                set
                {
                    if (value < 1)
                    {
                        value = 1;
                    }
                    if (value > 4)
                    {
                        value = 4;
                    }
                    numberOfPlayers = value;
                }
            }

            // @RandomNumberOfMonsterPlayers
            public static int RandomNumberOfMonsterPlayers
            {
                get
                {
                    return randomNumberOfMonsterPlayers;
                }
                set
                {
                    if (value < 1)
                    {
                        value = 1;
                    }
                    if (value > 4)
                    {
                        value = 4;
                    }
                    randomNumberOfMonsterPlayers = value;
                }
            }

            // @VerifySpawnTimes
            public static void VerifySpawnTimes(float originalMinSpawnTime, float originalMaxSpawnTime)
            {
                if (maxSpawnTime == minSpawnTime)
                {
                    if (minSpawnTime == 0f)
                    {
                        minSpawnTime = originalMinSpawnTime;
                        maxSpawnTime = originalMaxSpawnTime;
                    }
                    else
                    {
                        maxSpawnTime++;
                    }
                }
                else if (maxSpawnTime < minSpawnTime)
                {
                    if (minSpawnTime < originalMaxSpawnTime)
                    {
                        maxSpawnTime = originalMaxSpawnTime;
                    }
                    else
                    {
                        maxSpawnTime = minSpawnTime + (originalMaxSpawnTime - originalMinSpawnTime);
                    }
                }

                if ((maxSpawnTime - minSpawnTime + 1) < numberOfMonsters && !ModSettings.spawnMonsterInStarterRoom)
                {
                    maxSpawnTime += numberOfMonsters;
                }
            }

            // @PlayerHasLivesLeft
            public static bool PlayerHasLivesLeft()
            {
                return extraLives > 0;
            }

            // @TakeLife
            public static void TakeLife(PlayerHealth playerHealth)
            {
                extraLives--;
                DropPlayerItems();
                if (temporaryPlayerPosition != null)
                {
                    playerHealth.NPC.transform.position = temporaryPlayerPosition;
                }
                else
                {
                    playerHealth.NPC.transform.position = LevelGeneration.Instance.StartRoom.transform.position;
                }
                FadeScript.CutToBlack();
                FadeScript.BeginFadeIn();
                playerHealth.currentHP = playerHealth.MaxHP;
                String objectiveTextString = "You died. You have " + extraLives + " extra ";
                if (extraLives == 1)
                {
                    objectiveTextString += "life left.";
                }
                else
                {
                    objectiveTextString += "lives left.";
                }
                TriggerObjectives.instance.StartCoroutine(SpawnProtectionCoroutine(10f, playerHealth.NPC));
                if (!enableMultiplayer)
                {
                    ShowTextOnScreen(objectiveTextString, 10f, true);
                }
                else
                {
                    int playerNumber = MultiplayerMode.PlayerNumber(playerHealth.NPC.GetInstanceID());
                    ShowTextOnScreen(objectiveTextString, 10f, true, playerNumber);
                }
                Debug.Log(objectiveTextString);
                if (persistentMonster)
                {
                    if (ModSettings.numberOfMonsters > 1 && ManyMonstersMode.monsterList != null)
                    {
                        for (int i = 0; i < ManyMonstersMode.monsterList.Count; i++)
                        {
                            ManyMonstersMode.monsterList[i].GetComponent<MState>().SendEvent("Chase");
                        }
                    }
                    else
                    {
                        References.Monster.GetComponent<MState>().SendEvent("Chase");
                    }
                }
                if (enableMultiplayer)
                {
                    foreach (NewPlayerClass newPlayerClass in MultiplayerMode.newPlayerClasses)
                    {
                        if (newPlayerClass != playerHealth.NPC)
                        {
                            if (PlayerHasLivesLeft())
                            {
                                MultiplayerMode.RevivePlayer(newPlayerClass);
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }
            }

            private static IEnumerator SpawnProtectionCoroutine(float secondsOfProtection, NewPlayerClass playerToProtect)
            {
                int crewPlayerIndex = 0;
                if (enableMultiplayer)
                {
                    crewPlayerIndex = MultiplayerMode.crewPlayers.IndexOf(playerToProtect);
                }
                if (!invincibilityMode[crewPlayerIndex])
                {
                    spawnProtection[crewPlayerIndex] = true;
                    SetInvincibilityMode(true, crewPlayerIndex);
                }
                yield return new WaitForSeconds(secondsOfProtection);
                if (spawnProtection[crewPlayerIndex])
                {
                    spawnProtection[crewPlayerIndex] = false;
                    SetInvincibilityMode(false, crewPlayerIndex);
                }
                yield break;
            }

            // @DropPlayerItems
            private static void DropPlayerItems()
            {
                foreach (InventorySlot inventorySlot in References.Inventory.inventorySlots)
                {
                    if (inventorySlot.Item != null)
                    {
                        References.Inventory.DropItem(inventorySlot);
                    }
                }
            }

            // @ClearObjectiveTextAfterTime
            private static IEnumerator ClearObjectiveTextAfterTime(string textToShow, PlayerObjectives playerObjectives, float timeToWait = 5f, bool priorityText = false, int playerNumber = 0)
            {
                yield return new WaitForSeconds(timeToWait);
                if (playerObjectives.playerObjectives.Equals(textToShow))
                {
                    playerObjectives.playerObjectives = string.Empty;
                }
                if (priorityText)
                {
                    showingPriorityText = false;
                }
                yield break;
            }

            // @ShowDebugModeText
            public static void ShowDebugModeText()
            {
                // Update trigger objectives to show the Debug Text above other text.
                TriggerObjectives.instance.inventoryScrollOnce = true;
                TriggerObjectives.instance.isStillTutorial = false;
                TriggerObjectives.instance.SetToNone();
                TriggerObjectives.instance.timer = 0f;
                ShowTextOnScreen("Debug Mode Active", 5f, true);
            }

            // @UseCustomColour
            public static bool UseCustomColour(String colourString)
            {
                if (colourString.Equals("Default") || colourString.Equals("default"))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }

            // @ConvertColourStringToColour
            public static Color ConvertColourStringToColour(String colourString)
            {
                // Split the RGB string into 3 strings using comma as a delimiter.
                String[] coloursArray = colourString.Split(',');

                // Turn the strings into floats and divide them by 255 to make them out of 1.
                float[] coloursArrayFloat = new float[3];
                for (int colourNumber = 0; colourNumber < 3; colourNumber++)
                {
                    coloursArrayFloat[colourNumber] = float.Parse(coloursArray[colourNumber]) / 255f;
                }

                // Return a new colour with each RGB value.
                return new Color(coloursArrayFloat[0], coloursArrayFloat[1], coloursArrayFloat[2]);
            }

            // @ShowTextOnScreen
            public static void ShowTextOnScreen(string textToShow, float timeToShow = 5f, bool priorityText = false, int playerNumber = 0)
            {
                if (!showingPriorityText || timeToShow <= 0f)
                {
                    // Change the text being shown.
                    PlayerObjectives playerObjectives;
                    if (!enableMultiplayer)
                    {
                        playerObjectives = TriggerObjectives.instance.playerObjectives;
                    }
                    else
                    {
                        playerObjectives = MultiplayerMode.playerObjectivesList[playerNumber];
                    }
                    playerObjectives.playerObjectives = textToShow;
                    playerObjectives.timer = 0f; // Timer goes up from 0, not down from the time to show. Not leaving the tutorial room seems to stop message from showing up.

                    // Set up the clearing of the text and how the method will respond to inputs in the future.
                    if (priorityText)
                    {
                        showingPriorityText = true;
                    }
                    if (timeToShow > 0f)
                    {
                        TriggerObjectives.instance.playerObjectives.StartCoroutine(ClearObjectiveTextAfterTime(textToShow, playerObjectives, timeToShow, priorityText, playerNumber));
                    }
                    else if (textToShow.Equals(string.Empty))
                    {
                        showingPriorityText = false;
                    }
                }
            }

            // @ForceChase
            public static void ForceChase(Monster specificMonster = null)
            {
                if (specificMonster == null)
                {
                    // If Many Monsters Mode is active and no specific monster was selected, make each monster chase the player. Else, make the specific monster the references monster.
                    if (ModSettings.numberOfMonsters > 1)
                    {
                        foreach (Monster monster in ManyMonstersMode.monsterListMonsterComponents)
                        {
                            if (monster.gameObject.activeInHierarchy)
                            {
                                if (monster.MonsterType == Monster.MonsterTypeEnum.Hunter)
                                {
                                    TimeScaleManager.Instance.StartCoroutine(ForceChaseHunter(monster));
                                    //((MonoBehaviour)monster).StartCoroutine(ForceChaseHunter(monster));
                                }
                                else
                                {
                                    monster.LastSeenPlayerPosition = monster.player.transform.position;
                                    monster.GetAlertMeters.mSightAlert = 100f;
                                    monster.GetComponent<MState>().SendEvent("Chase");
                                }
                            }
                        }

                        if (ModSettings.debugMode)
                        {
                            Debug.Log("Forced Chase");
                            ShowTextOnScreen("Forced Chase");
                        }
                        return;
                    }
                    else
                    {
                        specificMonster = References.Monster.GetComponent<Monster>();
                    }
                }

                if (specificMonster.gameObject.activeInHierarchy)
                {
                    if (specificMonster.MonsterType == Monster.MonsterTypeEnum.Hunter)
                    {
                        TimeScaleManager.Instance.StartCoroutine(ForceChaseHunter(specificMonster));
                        //((MonoBehaviour)specificMonster).StartCoroutine(ForceChaseHunter(specificMonster));
                    }
                    else
                    {
                        specificMonster.LastSeenPlayerPosition = specificMonster.player.transform.position;
                        specificMonster.GetAlertMeters.mSightAlert = 100f;
                        specificMonster.GetComponent<MState>().SendEvent("Chase");
                    }

                    // If debug mode is on, show a diagnostics message. Debug mode will NOT always be on as this function is used in non-debug mode applications too, like in the finale of Glowstick Hunt.
                    if (ModSettings.debugMode)
                    {
                        Debug.Log("Forced Chase");
                        ShowTextOnScreen("Forced Chase");
                    }
                }
                else if (ModSettings.debugMode)
                {
                    Debug.Log("Could not force chase as monster is inactive");
                    ShowTextOnScreen("Could not force chase as monster is inactive");
                }
            }

            // @ForceStopChase
            public static void ForceStopChase(Monster specificMonster = null)
            {
                if (specificMonster == null)
                {
                    // If Many Monsters Mode is active and no specific monster was selected, make each monster lose sight of the player. Else, make the specific monster the references monster.
                    if (ModSettings.numberOfMonsters > 1)
                    {
                        foreach (Monster monster in ManyMonstersMode.monsterListMonsterComponents)
                        {
                            if (monster.gameObject.activeInHierarchy)
                            {
                                TimeScaleManager.Instance.StartCoroutine(KeepForcingStopChase(monster));
                            }
                        }

                        if (ModSettings.debugMode)
                        {
                            Debug.Log("Forced Stop Chase");
                            ShowTextOnScreen("Forced Stop Chase");
                        }
                        return;
                    }
                    else
                    {
                        specificMonster = References.Monster.GetComponent<Monster>();
                    }
                }

                if (specificMonster.gameObject.activeInHierarchy)
                {
                    TimeScaleManager.Instance.StartCoroutine(KeepForcingStopChase(specificMonster));
                    if (ModSettings.debugMode)
                    {
                        Debug.Log("Forced Stop Chase");
                        ShowTextOnScreen("Forced Stop Chase");
                    }
                }
                else if (ModSettings.debugMode)
                {
                    Debug.Log("Could not force stop chase as monster is inactive");
                    ShowTextOnScreen("Could not force stop chase as monster is inactive");
                }
            }

            public static IEnumerator KeepForcingStopChase(Monster specificMonster)
            {
                if (ModSettings.enableMultiplayer)
                {
                    // If the monster sees a new player a frame later then do not force stop the chase.
                    yield return null;
                    if (specificMonster.EyeVision.CheckIfPlayer())
                    {
                        yield break;
                    }
                }
                FSM fsm = specificMonster.GetComponent<FSM>();
                while (fsm.Current.typeofState == FSMState.StateTypes.Chase)
                {

                    specificMonster.sawPlayerEnterHiding = false;
                    if (specificMonster.Hearing.SoundIsFromPlayer)
                    {
                        specificMonster.Hearing.SoundIsFromPlayer = false;
                        if (specificMonster.Hearing.currentSound != null)
                        {
                            specificMonster.Hearing.currentSound.explored = true;
                        }
                    }
                    specificMonster.TimeOutVision.SetTime(specificMonster.persistanceTime);
                    specificMonster.GetAlertMeters.mSightAlert = 0f;
                    specificMonster.GetComponent<MState>().SendEvent("PlayerLoseSight");
                    //((MonoBehaviour)specificMonster).StartCoroutine(BlindMonster(specificMonster, 5f));
                    //specificMonster.StunAndRetreat();
                    yield return null;
                }
                yield break;
            }

            // @BurnHunter
            public static void BurnHunter(Monster specificMonster = null)
            {
                if (specificMonster == null)
                {
                    // If Many Monsters Mode is active and no specific Hunter was selected, burn each Hunter. Else, make the specific Hunter the references monster if it is a Hunter.
                    if (ModSettings.numberOfMonsters > 1 && ManyMonstersMode.hunters.Count > 0)
                    {
                        foreach (Monster hunter in ManyMonstersMode.huntersMonsterComponents)
                        {
                            hunter.MoveControl.GetAniControl.HasBeenStunned = true;
                            hunter.IsMonsterRetreating = true;
                        }

                        if (ModSettings.debugMode)
                        {
                            ShowTextOnScreen("Burned Hunter(s)");
                        }
                        return;
                    }
                    else
                    {
                        specificMonster = References.Monster.GetComponent<Monster>();
                    }
                }

                // Check whether the specific monster is actually a Hunter.
                if (specificMonster.MonsterType == Monster.MonsterTypeEnum.Hunter)
                {
                    specificMonster.MoveControl.GetAniControl.HasBeenStunned = true;
                    specificMonster.IsMonsterRetreating = true;

                    if (ModSettings.debugMode)
                    {
                        ShowTextOnScreen("Burned Hunter");
                    }
                }
                else
                {
                    if (ModSettings.debugMode)
                    {
                        ShowTextOnScreen("Could not burn a Hunter as there are none in the game");
                    }
                }
            }

            // @ForceChaseHunter
            public static IEnumerator ForceChaseHunter(Monster hunter)
            {
                float timePassed = 0f;
                hunter.RoomDetect.CurrentRoom = LevelGeneration.GetNodeDataAtPosition(References.Player.transform.position).nodeRoom;
                bool originalOPHSetting = ModSettings.overpoweredHunter;
                bool originalAGHSetting = ModSettings.aggressiveHunter;
                ModSettings.overpoweredHunter = true;
                ModSettings.aggressiveHunter = true;
                while (timePassed < 5f)
                {
                    timePassed += Time.deltaTime;
                    yield return null;
                }
                hunter.LastSeenPlayerPosition = hunter.player.transform.position;
                hunter.GetAlertMeters.mSightAlert = 100f;
                hunter.GetComponent<MState>().SendEvent("Chase");
                ModSettings.overpoweredHunter = originalOPHSetting;
                ModSettings.aggressiveHunter = originalAGHSetting;
                yield break;
            }

            // @BlindMonster
            public static IEnumerator BlindMonster(Monster monster, float timeToBlind)
            {
                float timePassed = 0f;
                AnimationControl animationControl = monster.GetComponent<AnimationControl>();
                while (timePassed < timeToBlind)
                {
                    timePassed += Time.deltaTime;
                    monster.BeenBlinded = true;
                    animationControl.HasBeenStunned = true;
                    animationControl.immuneToStun = false;

                    yield return null;
                }
                monster.BeenBlinded = false;
                animationControl.HasBeenStunned = false;
                yield break;
            }

            // @DeathCountDown
            public static IEnumerator DeathCountDown(GameObject playerGameObject)
            {
                Debug.Log("Starting death countdown");
                MindAttackEffect playerMindAttackEffect = playerGameObject.GetComponentInChildren<MindAttackEffect>();
                float timeLeft = ModSettings.deathCountdown;
                while (timeLeft > 0f)
                {
                    playerMindAttackEffect.enabled = true;
                    timeLeft -= Time.deltaTime;
                    if (ModSettings.showDeathCountdown)
                    {
                        ModSettings.ShowTextOnScreen(Mathf.RoundToInt(timeLeft).ToString());
                    }

                    float deathTimerCompletionPercentage = (ModSettings.deathCountdown - timeLeft) / ModSettings.deathCountdown; // Set the mind attack strength to a ratio [not technically a ratio] / percentage [percentage fraction / decimal fraction] of the maximum timer.
                    if (playerMindAttackEffect.strength < deathTimerCompletionPercentage)
                    {
                        playerMindAttackEffect.strength = deathTimerCompletionPercentage;
                    }
                    yield return null;
                }
                timeLeft = 0f;
                if (ModSettings.showDeathCountdown)
                {
                    ModSettings.ShowTextOnScreen(Mathf.RoundToInt(timeLeft).ToString());
                }
                playerGameObject.GetComponentInChildren<PlayerHealth>().InstantKill(PlayerHealth.DamageTypes.MindAttack);
                yield break;
            }

            // @SetMultiplayerKeyBinds
            private static void SetMultiplayerKeyBinds(string[] modSettings)
            {
                Debug.Log("Attempting to set up multiplayer key binds.");
                try
                {
                    string[] keyBindNames = new string[] { "Interact", "LeanLeft", "LeanRight", "Crouch", "Jump", "Forward", "Back", "Left", "Right", "Sprint", "UseItem", "DropItem", "ViewNote", "ViewJournal", "ListenToLog" };
                    MultiplayerMode.customKeyBinds = new Dictionary<string, List<KeyBind>>();

                    // Set up lists to hold key binds for each category. A bigger lists holds smaller lists that match up to each category, in which each player's key bind for that category is stored.
                    List<List<KeyBind>> keyBindLists = new List<List<KeyBind>>();
                    for (int i = 0; i < keyBindNames.Length; i++)
                    {
                        keyBindLists.Add(new List<KeyBind>());
                    }

                    // Set the primary player's controls
                    KeyControls keyControls = FindObjectOfType<KeyControls>();
                    if (keyControls != null)
                    {
                        /*
                        List<KeyBind> playerOneKeyBinds = new List<KeyBind>(keyControls.keyCodes); // Maybe playerOneKeyBinds.AddRange(keyControls.keyCodes); could have been used instead. // How do I copy items from list to list without foreach? - Lasse V. Karlsen - https://stackoverflow.com/questions/1952185/how-do-i-copy-items-from-list-to-list-without-foreach - Accessed 04.07.2021

                        Debug.Log(keyControls.keyCodes.Count);
                        Debug.Log(playerOneKeyBinds.Count);
                        playerOneKeyBinds.RemoveAt(14);
                        */
                        Debug.Log("Setting key binds for player number 0");

                        List<KeyBind> playerOneKeyBinds = new List<KeyBind>();
                        if (PlayerPrefs.HasKey("SavedInteractKey"))
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind.key = keyControls.GetKeyPref("SavedInteractKey", keyControls.interactKey.Key);
                            keyBind.keyInUse = keyBind.key;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                        }
                        else
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind = keyControls.interactKey;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Default key bind set: keyBind.key = " + keyControls.interactKey.key + " & keyBind.gamepadButtonInUse = " + keyControls.interactKey.gamepadButtonInUse);
                        }
                        if (PlayerPrefs.HasKey("SavedLeanLeftKey"))
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind.key = keyControls.GetKeyPref("SavedLeanLeftKey", keyControls.leanLeftKey.Key);
                            keyBind.keyInUse = keyBind.key;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                        }
                        else
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind = keyControls.leanLeftKey;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Default key bind set: keyBind.key = " + keyControls.leanLeftKey.key + " & keyBind.gamepadButtonInUse = " + keyControls.leanLeftKey.gamepadButtonInUse);
                        }
                        if (PlayerPrefs.HasKey("SavedLeanRightKey"))
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind.key = keyControls.GetKeyPref("SavedLeanRightKey", keyControls.leanRightKey.Key);
                            keyBind.keyInUse = keyBind.key;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                        }
                        else
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind = keyControls.leanRightKey;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Default key bind set: keyBind.key = " + keyControls.leanRightKey.key + " & keyBind.gamepadButtonInUse = " + keyControls.leanRightKey.gamepadButtonInUse);
                        }
                        if (PlayerPrefs.HasKey("SavedCrouchKey"))
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind.key = keyControls.GetKeyPref("SavedCrouchKey", keyControls.crouchKey.Key);
                            keyBind.keyInUse = keyBind.key;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                        }
                        else
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind = keyControls.crouchKey;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Default key bind set: keyBind.key = " + keyControls.crouchKey.key + " & keyBind.gamepadButtonInUse = " + keyControls.crouchKey.gamepadButtonInUse);
                        }
                        if (PlayerPrefs.HasKey("SavedJumpKey"))
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind.key = keyControls.GetKeyPref("SavedJumpKey", keyControls.jumpKey.Key);
                            keyBind.keyInUse = keyBind.key;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                        }
                        else
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind = keyControls.jumpKey;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Default key bind set: keyBind.key = " + keyControls.jumpKey.key + " & keyBind.gamepadButtonInUse = " + keyControls.jumpKey.gamepadButtonInUse);
                        }
                        if (PlayerPrefs.HasKey("SavedForwardKey"))
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind.key = keyControls.GetKeyPref("SavedForwardKey", keyControls.forwardKey.Key);
                            keyBind.keyInUse = keyBind.key;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                        }
                        else
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind = keyControls.forwardKey;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Default key bind set: keyBind.key = " + keyControls.forwardKey.key + " & keyBind.gamepadButtonInUse = " + keyControls.forwardKey.gamepadButtonInUse);
                        }
                        if (PlayerPrefs.HasKey("SavedBackKey"))
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind.key = keyControls.GetKeyPref("SavedBackKey", keyControls.backKey.Key);
                            keyBind.keyInUse = keyBind.key;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                        }
                        else
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind = keyControls.backKey;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Default key bind set: keyBind.key = " + keyControls.backKey.key + " & keyBind.gamepadButtonInUse = " + keyControls.backKey.gamepadButtonInUse);
                        }
                        if (PlayerPrefs.HasKey("SavedLeftKey"))
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind.key = keyControls.GetKeyPref("SavedLeftKey", keyControls.leftKey.Key);
                            keyBind.keyInUse = keyBind.key;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                        }
                        else
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind = keyControls.leftKey;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Default key bind set: keyBind.key = " + keyControls.leftKey.key + " & keyBind.gamepadButtonInUse = " + keyControls.leftKey.gamepadButtonInUse);
                        }
                        if (PlayerPrefs.HasKey("SavedRightKey"))
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind.key = keyControls.GetKeyPref("SavedRightKey", keyControls.rightKey.Key);
                            keyBind.keyInUse = keyBind.key;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                        }
                        else
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind = keyControls.rightKey;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Default key bind set: keyBind.key = " + keyControls.rightKey.key + " & keyBind.gamepadButtonInUse = " + keyControls.rightKey.gamepadButtonInUse);
                        }
                        if (PlayerPrefs.HasKey("SavedSprintKey"))
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind.key = keyControls.GetKeyPref("SavedSprintKey", keyControls.sprintKey.Key);
                            keyBind.keyInUse = keyBind.key;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                        }
                        else
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind = keyControls.sprintKey;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Default key bind set: keyBind.key = " + keyControls.sprintKey.key + " & keyBind.gamepadButtonInUse = " + keyControls.sprintKey.gamepadButtonInUse);
                        }
                        if (PlayerPrefs.HasKey("SavedUseItemKey"))
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind.key = keyControls.GetKeyPref("SavedUseItemKey", keyControls.useItemKey.Key);
                            keyBind.keyInUse = keyBind.key;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                        }
                        else
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind = keyControls.useItemKey;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Default key bind set: keyBind.key = " + keyControls.useItemKey.key + " & keyBind.gamepadButtonInUse = " + keyControls.useItemKey.gamepadButtonInUse);
                        }
                        if (PlayerPrefs.HasKey("SavedDropItemKey"))
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind.key = keyControls.GetKeyPref("SavedDropItemKey", keyControls.dropItemKey.Key);
                            keyBind.keyInUse = keyBind.key;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                        }
                        else
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind = keyControls.dropItemKey;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Default key bind set: keyBind.key = " + keyControls.dropItemKey.key + " & keyBind.gamepadButtonInUse = " + keyControls.dropItemKey.gamepadButtonInUse);
                        }
                        if (PlayerPrefs.HasKey("SavedViewNoteKey"))
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind.key = keyControls.GetKeyPref("SavedViewNoteKey", keyControls.viewNoteKey.Key);
                            keyBind.keyInUse = keyBind.key;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                        }
                        else
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind = keyControls.viewNoteKey;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Default key bind set: keyBind.key = " + keyControls.viewNoteKey.key + " & keyBind.gamepadButtonInUse = " + keyControls.viewNoteKey.gamepadButtonInUse);
                        }
                        if (PlayerPrefs.HasKey("SavedViewJournalKey"))
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind.key = keyControls.GetKeyPref("SavedViewJournalKey", keyControls.viewJournalKey.Key);
                            keyBind.keyInUse = keyBind.key;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                        }
                        else
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind = keyControls.viewJournalKey;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Default key bind set: keyBind.key = " + keyControls.viewJournalKey.key + " & keyBind.gamepadButtonInUse = " + keyControls.viewJournalKey.gamepadButtonInUse);
                        }
                        /*
                        if (PlayerPrefs.HasKey("SavedHeadLookKey"))
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind.key = keyControls.GetKeyPref("SavedHeadLookKey", keyControls.headLookKey.Key);
                            keyBind.keyInUse = keyBind.key;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                        }
                        else
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind = keyControls.headLookKey;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Default key bind set: keyBind.key = " + keyControls.headLookKey.key + " & keyBind.gamepadButtonInUse = " + keyControls.headLookKey.gamepadButtonInUse);
                        }
                        */
                        if (PlayerPrefs.HasKey("SavedListenToLog"))
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind.key = keyControls.GetKeyPref("SavedListenToLog", keyControls.listenToLogKey.Key);
                            keyBind.keyInUse = keyBind.key;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                        }
                        else
                        {
                            KeyBind keyBind = new KeyBind();
                            keyBind = keyControls.listenToLogKey;
                            keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                            playerOneKeyBinds.Add(keyBind);
                            Debug.Log("Default key bind set: keyBind.key = " + keyControls.listenToLogKey.key + " & keyBind.gamepadButtonInUse = " + keyControls.listenToLogKey.gamepadButtonInUse);
                        }

                        for (int listIndex = 0; listIndex < keyBindLists.Count; listIndex++)
                        {
                            keyBindLists[listIndex].Add(playerOneKeyBinds[listIndex]);
                        }

                    }
                    else
                    {
                        Debug.Log("Could not find KeyControls!");
                    }


                    if (useCustomKeyBinds)
                    {
                        FindCustomKeyBinds(keyBindLists, modSettings, keyBindNames);
                    }
                    else
                    {
                        // Define default key binds.
                        Dictionary<string, string> defaultControllerKeysDictionary = new Dictionary<string, string>();
                        defaultControllerKeysDictionary.Add(keyBindNames[0], "Controller.X");
                        defaultControllerKeysDictionary.Add(keyBindNames[1], "Controller.LeftBumper");
                        defaultControllerKeysDictionary.Add(keyBindNames[2], "Controller.RightBumper");
                        defaultControllerKeysDictionary.Add(keyBindNames[3], "Controller.B");
                        defaultControllerKeysDictionary.Add(keyBindNames[4], "Controller.A");
                        defaultControllerKeysDictionary.Add(keyBindNames[5], "None"); // The controller axes must be handled differently.
                        defaultControllerKeysDictionary.Add(keyBindNames[6], "None");
                        defaultControllerKeysDictionary.Add(keyBindNames[7], "None");
                        defaultControllerKeysDictionary.Add(keyBindNames[8], "None");
                        defaultControllerKeysDictionary.Add(keyBindNames[9], "None"); // Sprint uses the left trigger, which is also variable.
                        defaultControllerKeysDictionary.Add(keyBindNames[10], "None"); // UseItem uses the right trigger, which is also variable.
                        defaultControllerKeysDictionary.Add(keyBindNames[11], "Controller.Y");
                        defaultControllerKeysDictionary.Add(keyBindNames[12], "Controller.DPadUp");
                        defaultControllerKeysDictionary.Add(keyBindNames[13], "Controller.Back");
                        defaultControllerKeysDictionary.Add(keyBindNames[14], "Controller.DPadDown");

                        // Give each secondary player the default key binds.
                        for (int i = 1; i < NumberOfPlayers; i++)
                        {
                            Debug.Log("Setting key binds for player number " + i);
                            for (int listIndex = 0; listIndex < keyBindLists.Count; listIndex++)
                            {
                                AddKeyBindToList(defaultControllerKeysDictionary[keyBindNames[listIndex]], keyBindLists[listIndex]);
                            }
                        }
                    }

                    // Set up the custom key binds dictionary by adding each key bind category name and matching the relevant key binds list to it.
                    for (int listIndex = 0; listIndex < keyBindLists.Count; listIndex++)
                    {
                        MultiplayerMode.customKeyBinds.Add(keyBindNames[listIndex], keyBindLists[listIndex]);
                    }
                    Debug.Log("Multiplayer key binds have successfully been set up");
                }
                catch (Exception e)
                {
                    Debug.Log("Error while setting up multiplayer key binds:\n" + e.ToString());
                }
            }

            // @FindCustomKeyBind
            private static void FindCustomKeyBinds(List<List<KeyBind>> keyBindLists, string[] modSettings, string[] keyBindNames)
            {
                // Add key binds to each category.
                for (int listIndex = 0; listIndex < keyBindLists.Count; listIndex++)
                {
                    // Get the relevant key bind row from the settings and split it up.
                    string keyBindCSV = MESMSetting.FindSetting("KeyBind " + keyBindNames[listIndex]);
                    string[] keyBinds = keyBindCSV.Split(',');
                    for (int playerIndex = 1; playerIndex < NumberOfPlayers; playerIndex++)
                    {
                        Debug.Log("Setting key binds for player number " + playerIndex);
                        // If not enough custom key binds were supplied, make any excess players use the same controls as the last player with supplied key binds.
                        if (playerIndex < keyBinds.Length)
                        {
                            AddKeyBindToList(keyBinds[playerIndex], keyBindLists[listIndex]);
                        }
                        else
                        {
                            AddKeyBindToList(keyBinds[keyBinds.Length - 1], keyBindLists[listIndex]);
                        }
                    }
                }
            }

            // @AddKeyBindToList
            private static void AddKeyBindToList(string keyBindString, List<KeyBind> keyBindList)
            {
                KeyBind keyBind = new KeyBind();
                try
                {
                    // Check whether the key bind is for a controller. If it is not, handle the key bind as a keyboard key bind.
                    if (keyBindString.Contains("Controller."))
                    {
                        keyBindString = keyBindString.Replace("Controller.", "");

                        keyBind.key = KeyCode.None;
                        keyBind.keyInUse = keyBind.key;
                        keyBind.gamepadButtonInUse = (XboxCtrlrInput.XboxButton)System.Enum.Parse(typeof(XboxCtrlrInput.XboxButton), keyBindString);
                    }
                    else
                    {
                        keyBind.key = (KeyCode)System.Enum.Parse(typeof(KeyCode), keyBindString); // String to KeyCode in CSharp - Lo0NuhtiK - https://answers.unity.com/questions/653106/string-to-keycode.html - Accessed 04.07.2021
                        keyBind.keyInUse = keyBind.key;
                        keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                    }
                }
                catch
                {
                    // If the supplied key was not valid, set the key bind to unbound and log an error message.
                    Debug.Log("Could not parse custom key bind: " + keyBindString);
                    keyBind.key = KeyCode.None;
                    keyBind.keyInUse = keyBind.key;
                    keyBind.gamepadButtonInUse = XboxCtrlrInput.XboxButton.None;
                }
                Debug.Log("Custom key bind set: keyBind.key = " + keyBind.key + " & keyBind.gamepadButtonInUse = " + keyBind.gamepadButtonInUse);
                keyBindList.Add(keyBind);
            }

            public static FiendAura GiveMonsterFiendAuraAndDisruptor(Monster monster, float auraSmallRadiusMultiplier = 1f, float auraMediumRadiusMultiplier = 1f, float auraLargeRadiusMultiplier = 1f)
            {
                FiendAura fiendAura = monster.gameObject.AddComponent<FiendAura>();
                fiendAura.largeRadius = 6f * auraLargeRadiusMultiplier;
                fiendAura.mediumRadius = 4.5f * auraMediumRadiusMultiplier;
                fiendAura.smallRadius = 3f * auraSmallRadiusMultiplier;
                // If a custom fiend aura size is being used, replace the Start() code.
                if (ModSettings.fiendFlickerMin != 0f && ModSettings.fiendFlickerMed != 0f && ModSettings.fiendFlickerMax != 0f)
                {
                    fiendAura.largeRadius = ModSettings.fiendFlickerMax * auraLargeRadiusMultiplier;
                    fiendAura.mediumRadius = ModSettings.fiendFlickerMed * auraMediumRadiusMultiplier;
                    fiendAura.smallRadius = ModSettings.fiendFlickerMin * auraSmallRadiusMultiplier;
                    fiendAura.srt_lrgRad = fiendAura.largeRadius;
                    fiendAura.srt_medRad = fiendAura.mediumRadius;
                    fiendAura.srt_smlRad = fiendAura.smallRadius;
                }
                else
                {
                    fiendAura.Start();
                }
                if (ModSettings.startedWithMMM)
                {
                    ManyMonstersMode.auras.Add(fiendAura);
                }

                FiendLightDisruptor fiendLightDisruptor = monster.gameObject.AddComponent<FiendLightDisruptor>();
                fiendLightDisruptor.Start();
                fiendLightDisruptor.fiendAura = fiendAura;
                fiendLightDisruptor.maxSinceDisrupt = 2f;

                return fiendAura;
            }


            // #Variables

            // Monster Settings Variables
            public static int numberOfRandomMonsters;
            public static int numberOfBrutes;
            public static int numberOfHunters;
            public static int numberOfFiends;
            public static int numberOfSparkies;
            public static int numberOfSmokeMonsters;
            public static bool useSparky;
            public static bool sparkyWithModel = false; // New Sparky with working model. Use 1 Brute instead of 1 Sparky with Use Sparky enabled.
            public static bool useSmokeMonster; // Variable that enables the smoke monster code.
            public static bool finishedCreatingSimpleSparky;
            public static bool disableRandomBrute;
            public static bool disableRandomHunter;
            public static bool disableRandomFiend;
            public static List<string> bannedRandomMonsters;
            public static bool forceManyMonstersMode;
            public static float minSpawnTime;
            public static float maxSpawnTime;
            public static bool spawnMonsterInStarterRoom;
            public static bool noMonsterStunImmunity;
            public static bool persistentMonster;
            public static bool seerMonster;
            public static bool monsterAlwaysFindsYou;
            public static float monsterAnimationSpeedMultiplier;
            public static float monsterMovementSpeedMultiplier;
            public static float monsterRotationSpeedMultiplier;
            public static bool varyingMonsterSizes;
            public static float customMonsterScale;
            public static bool bruteChaseSpeedBuff;
            public static float bruteChaseSpeedBuffMultiplier;
            public static bool applyChaseSpeedBuffToAllMonsters;
            public static bool overpoweredHunter;
            public static bool aggressiveHunter;
            public static float fiendFlickerMin;
            public static float fiendFlickerMed;
            public static float fiendFlickerMax;
            public static bool fiendDoorTeleportation;
            public static bool applyDoorTeleportationToAllMonsters;
            // public static bool letAllMonstersLockDoors;
            public static bool giveAllMonstersAFiendAura;
            public static bool monstersSearchRandomly;
            public static bool alternatingMonstersMode;
            public static int numberOfAlternatingMonsters;
            public static bool giveAllMonstersASmokeShroud;
            public static float smokeShroudRadius;
            public static float smokeShroudDangerRadiusFactor;
            public static bool giveAllMonstersAFireShroud;
            public static float fireShroudRadius;
            public static float fireBlastRadius;
            public static string sparkyDifficultyPreset;
            public static float sparkyChaseFactor;
            public static float sparkyMaxChaseFactorIncreaseFromBuff;
            public static float sparkyMaxSpeedFactorIncreaseFromBuff;
            public static float sparkyRotationSpeedFactor;
            public static float sparkyLurkMinimumDistanceToPlayer;
            public static float sparkyLurkMaxSuperEMPDistance;
            public static float sparkyLurkSuperEMPChargeTimeToWait;
            public static float sparkyLurkMaxAggro;
            public static float sparkyAuraMinEMPWait;
            public static float sparkyAuraMaxEMPWait;
            public static float sparkyAuraMaxRoomBuffPercentage;
            public static float sparkyElectricTrapSpawnChance;
            public static float sparkyAuraEMPRange;
            public static float sparkyAuraDistantSwitchChance;
            public static float sparkyAuraDistantSwitchFailChanceAddition;
            public static float sparkyRegionEMPRoomThreshold;
            public static float regionElectricTrapSpawnChance;
            public static float regionElectricTrapSlowFactor;
            public static float regionElectricTrapScaleMultiplier;
            public static float regionElectricTrapLifeTime;
            public static float regionElectricTrapMinSpawnTime;
            public static float regionElectricTrapMaxSpawnTime;
            public static float regionElectrificationRoomRecoveryPercentage;
            public static float sparkyElectricTrapBaseSlowFactor;
            public static float maxTrapSlowFactorDecreaseFromSparkyBuff;
            public static float sparkyElectricTrapBaseScaleMultiplier;
            public static float maxTrapScaleMultiplierIncreaseFromSparkyBuff;
            public static float sparkyElectricTrapBaseLifeTime;
            public static float maxTrapLifeTimeIncreaseFromSparkyBuff;


            // Gamemode Settings Variables
            public static bool enableMultiplayer;
            private static int numberOfPlayers;
            public static bool enableCrewVSMonsterMode;
            public static List<int> numbersOfMonsterPlayers;
            private static int randomNumberOfMonsterPlayers;
            public static bool giveMonsterAnInventory;
            public static bool letMonsterUseInteractiveObjects;
            public static float bruteAbilityActiveTime;
            public static float bruteAbilityCooldownTime;
            public static float bruteAbilitySpeedMultiplier;
            public static float hunterAbilityActiveTime;
            public static float hunterAbilityCooldownTime;
            public static float hunterVentFrenzyAbilityActiveTime;
            public static float hunterVentFrenzyAbilityCooldownTime;
            public static float hunterVentFrenzyAbilitySpeedMultiplier;
            public static float fiendAbilityActiveTime;
            public static float fiendAbilityCooldownTime;
            public static float fiendAbilityDoorLockRange;
            public static float fiendAbilityAuraRangeMultiplier;
            public static bool useMultipleDisplaysIfPossible;
            private static bool useCustomKeyBinds;
            // Start With AI Having Control assigns CrewVsMonsterMode.letAIControlMonster directly.
            public static bool darkShip;
            public static bool powerableLights;
            private static bool randomiserMode;
            private static float chaosMultiplier;
            public static bool glowstickHunt;
            public static int specialGlowsticksRequired;
            private static int specialGlowsticksToCreate;
            public static List<Color> glowstickHuntColours;
            public static int glowstickHuntCounter;
            public static bool useCustomDoors;
            public static int customDoorTypeNumber;
            public static bool lightlyLockedDoors;
            public static bool scavengerMode;
            public static float deathCountdown;
            public static bool showDeathCountdown;
            public static bool itemMonsterFrenzy;
            public static bool extraChaoticIMF;
            public static float monsterSpawnSpeedrunSpawnTime;
            public static float monsterSpawningLimit;
            public static bool foggyShip;
            public static float fogDistance;
            public static string fogColour;
            public static bool monsterVisionAffectedByFog;
            public static bool foggyShipAlternativeMode;
            public static bool smokyShip;
            public static bool alwaysSmoky;
            public static float breathAmount;
            public static float breathRecovery;
            public static float smokeShroudBreathDrain;
            public static float smokeCorridorBreathDrain;


            // Player Settings Variables
            public static int extraLives;
            public static int inventorySize;
            public static float minimumValueOnFOVSlider;
            public static float maximumValueOnFOVSlider;
            public static bool playerStaminaMode;
            public static float playerStaminaModeHalfStaminaDuration;
            public static float playerStaminaModeSpeedPenaltyMultiplier;
            public static List<float> playerMovementSpeedMultiplier;
            private static float customPlayerScale;
            public static bool unlockPlayerHead;


            // Level Generation Settings Variables
            public static bool useCustomSeed;
            public static int seed;
            public static string startRoomRegion;
            public static bool spawnDeactivatedItems;
            public static bool noCameras;
            public static bool noSteam;
            private static bool allPreFilledFuseBoxes;
            public static bool noPreFilledFuseBoxes;
            private static bool noBarricadedDoors;
            public static bool overpoweredSteamVents;
            public static bool unbreakablePitTraps;
            public static bool addAdditionalCrewDeckBuilding;
            public static List<FuseBox> additionalFuseBoxesToSetUpAfterGeneration;
            public static bool useDeckFourOnSubmersibleSide;
            public static bool extendLowerDecks;
            public static bool spawnAdditionalEngineRoomWorkshops;
            public static bool aggressiveWorkshopSpawning;
            public static int numberOfCorridorsToCargoHoldFromDeckThree;
            public static int reduceNormalNumberOfCorridorsToCargoHold;
            public static bool lengthenedCargoHoldCorridors;
            public static bool shortenedCargoHoldCorridors;
            public static bool addLowerDeckNextToEngineRoom;
            public static int numberOfCorridorsToCargoHoldFromNewLowerDeck;
            public static bool addDeckZero;
            public static bool shuffledRegions;
            public static int numberOfTimesToShuffleRegions;
            public static bool crazyShuffle;
            public static bool extendMap;
            public static bool extendMapAdditive;
            public static Vector3 increaseMapSizeVector;
            public static int increaseRoomMinimumCount;
            public static int increaseRoomMaximumCount;
            public static bool includeUniqueRoomsInCountChange;
            public static int changeKeyItemSpawnNumbers;
            public static bool allowKeyItemsToNotSpawnAtAll;
            public static bool diverseItemSpawns;
            public static bool spawnItemsAnywhere;
            public static bool experimentalShipExtension = false;


            // Item Settings Variables
            public static bool infiniteFireExtinguisherFuel;
            public static float flareLifetime;
            public static bool spawnWithFlareGun;
            public static bool overpoweredFlareGun;
            public static bool flaresDisableMonsters;
            public static bool infiniteFlashlightPower;
            public static bool infiniteFuelCanFuel;
            public static float fireDurationMultiplier;
            public static bool monsterCompass;
            private static bool spawnWithLiferaftItems;
            private static bool spawnWithHelicopterItems;
            private static bool spawnWithSubmersibleItems;
            public static bool loopingRadio;
            public static bool betterSmashables;
            public static bool unsmashables;
            public static bool addSmokeGrenade;
            public static bool addMolotov;


            // Colour & Light Settings
            public static string bruteLightColour;
            public static float bruteLightIntensityMultiplier;
            public static float bruteLightRangeMultiplier;
            public static bool randomBruteLightColours;
            public static string flashlightColour;
            public static float flashlightIntensityMultiplier;
            public static float flashlightRangeMultiplier;
            public static bool randomFlashlightColours;
            public static string glowstickColour;
            public static float glowstickIntensityMultiplier;
            public static float glowstickRangeMultiplier;
            public static bool randomGlowstickColours;
            public static string shipGenericLightsColour;
            public static float shipGenericLightIntensityMultiplier;
            public static float shipGenericLightRangeMultiplier;
            public static bool randomShipGenericLightsColours;


            // Utility Settings Variables
            public static bool debugMode;
            public static List<bool> invincibilityMode;
            private static bool wallhacksMode;
            private static bool breakTheGameLight;
            private static bool breakTheGameHeavy;
            public static bool skipSplashScreen;
            public static bool skipMenuScreen;
            public static bool alwaysSkipMenuScreen;
            public static bool skippedMenuScreen; // Variable to stop the game from skipping the menu screen after initial loadup.
            public static bool disableCustomLoadingText;
            public static float timeScaleMultiplier;
            public static bool useMonsterUpdateGroups;
            private static int numberOfMonsterUpdateGroups;
            public static bool doNotPreRenderMonsters;
            public static bool doNotRenderBruteLight;
            public static bool disableMonsterParticles;
            public static bool monsterRadar;
            public static bool glowstickRadar;
            public static bool playerStaminaModeStaminaText;
            public static bool playerRegionNodeText;
            public static bool useSpeedrunTimer;
            public static Timer speedrunTimer;
            public static string finalTime;
            public static bool showSpeedrunTimerOnScreen;
            public static bool logDebugText;

            // Other Variables Used In Code
            // Early Declaration Needed
            public static bool enableMod;
            public static int numberOfMonsters;
            public static bool noclip;
            public static string version = "5.0";
            public static bool errorWhileReadingModSettings;
            public static string modSettingsErrorString;
            public static bool errorDuringLevelGeneration;
            public static List<bool> spawnProtection;
            public static bool startedWithInvincibilityMode; // Used so that spawn protection / short timed invincibility events do not switch off godmode in case the round was started with godmode on.
            private static bool invisibleMode;
            public static bool startedWithMMM;
            public static bool firstTimeReadingSettings = true;
            public static float playerMovementSpeedStartMultiplier;
            public static List<float> playerMovementSpeedDynamicMultiplier;
            public static List<float> staminaTimer;

            // Late Declaration (After Generation) Needed
            public static float playerMovementSpeedEndMultiplier;
            public static float staminaModeMaximumMultiplierChange;
            //public static float staminaModeHorizontalShiftConstant;
            //public static float staminaModeVerticalShiftConstant;
            public static float staminaModeTimeCoefficient;
            //public static float staminaModeVerticalScalingCoefficient;
            public static Liferaft[] liferafts;
            public static Crane[] cranes;
            public static List<Vector3> accessibleNodes;

            // No Declaration Needed
            public static Vector3 temporaryPlayerPosition;
            public static bool showingPriorityText = false;
        }

    }
}
// ~End Of File