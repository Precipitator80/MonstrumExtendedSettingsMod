using System;
using System.Collections.Generic;
using UnityEngine;

namespace MonstrumExtendedSettingsMod.Setting
{
    class LoadingScreenManager : Setting
    {
        protected override bool ShouldSettingBeEnabled()
        {
            return !ExtendedSettingsModScript.ModSettings.disableCustomLoadingText;
        }

        protected override void OnEnable()
        {
            On.LoadingBackground.Awake += HookLoadingBackground;
        }

        protected override void OnDisable()
        {
            On.LoadingBackground.Awake -= HookLoadingBackground;
        }

        /// <summary>
        /// Contains both original and custom loading screens.
        /// </summary>
        private static Hints.HintCollection[] extendedLoadingScreenArray;
        /// <summary>
        /// Maps loading screen file names to arrays of possible loading messages.
        /// </summary>
        private static readonly Dictionary<string, string[]> customLoadingScreenMessages = new Dictionary<string, string[]>()
            {
                { "01MainMenu",               new string[]{"The release of Monstrum dates back to 2015."}},
                { "02Collectables",           new string[]{"The amount of hidden or unused content can be surprising."}},
                { "03MESMSettings",           new string[]{"The Extended Settings Mod, first released in 2020, offers hundreds of settings to customise your experience."}},
                { "04Monstrum2HisaMaru",      new string[]{"The Hisa Maru survived into the 21st century, severely damaged by time.", "The contents of some of the cargo carried aboard the Hisa Maru remained classified.", "While other parts of the ship decayed severely over time, the Hisa Maru's upper decks remained in relatively good condition.", "The lone ship Hisa Maru met its own end where it all began."}},
                { "05Monstrum2Documents",     new string[]{"The future answers many questions left unanswered by the past, but opens many others...", "The Brute specimen exhibits supreme strength with core temperatures of 4,000 Kelvin and over 661 pounds of mass."}},
                { "06TV",                     new string[]{"Some objects that seem useless at first glance may prove to be more useful than thought."}},
                { "07Clock",                  new string[]{"A good sense of time can help you to predict a monster's next action.", "A perfect sense of time can help in the most risky of situations."}},
                { "08AlphaBunks",             new string[]{"The structure of the Hisa Maru was changed a lot throughout construction.", "The old structure of the Hisa Maru was very different to what can be seen now."}},
                { "09Workstation",            new string[]{"Some items, notes and easter eggs may require a more thorough inspection of the environment to find.", "There are more interaction points in the game than you might think."}},
                { "10Submersible",            new string[]{"The submersible requires uninterrupted time charging, giving monsters ample opportunity to interfere.", "The submersible is quick to repair but has a lengthy launch procedure."}},
                { "11Liferaft",               new string[]{"The life raft is often the safest option to escape against any monsters.", "Inflating and hoisting the raft is a lengthy process."}},
                { "12Security",               new string[]{"Unfortunately, the ship's security system is not on your side."}},
                { "13Helicopter",             new string[]{"Refueling the helicopter is an elaborate process that is sure to attract the attention of any monsters nearby.", "The helicopter can be used for fast transportation to land in case of emergencies.", "Preparing the helicopter can be quite difficult as it puts you in a very vulnerable position."}},
                { "14Container",              new string[]{"The cargo hold contains a large variety of items, but its maze of containers can be difficult to navigate safely.", "Some of the cargo carried aboard the Hisa Maru proved difficult to contain...", "In some situations, the cargo hold is the worst place you can end up."}},
                { "15Map",                    new string[]{"Maps placed on the walls throughout the ship may help you relocate yourself if lost.", "The lower decks hold a lot of the ship's larger equipment in a maze of workshop rooms.", "The ability to expertly navigate the ship's interior is vital to your survival."}},
                { "16Bridge",                 new string[]{"The bridge often contains quite useful items."}},
                { "17BarricadedRoom",         new string[]{"Use noise sources to lure monsters into opening barricaded rooms for you."}},
                { "18LevelGenerationSettings",new string[]{"The Level Generation Settings can change the ship in the most unusual ways."}},
                { "19UpperDecks",             new string[]{"Rooms in the upper decks are filled with smaller items and plenty of hiding spots.", "No matter how cosy and safe a room may look, don't stay anywhere too long."}},
                { "20HunterPresence",         new string[]{"Some things on the ship are best not investigated too closely...", "The presence of danger inside of egg sacs can be detected by throwing an item."}},
                { "21Hunter",                 new string[]{"The Hunter is a gelatinous monster that inspects the ship using its ventilation system.", "Quiet and unpredictable, the Hunter inspects the ship to ambush the player at the most unexpected moments."}},
                { "22Brute",                  new string[]{"The Brute is a hulk of a monster that will excel in a direct chase.", "The Brute's lack of stealth is offset by its significant speed and extreme danger in close proximity."}},
                { "23Fiend",                  new string[]{"The Fiend is an intelligent monster with mysterious telekinetic powers.", "The Fiend's powers allow it to pose a threat even at a distance."}},
                { "24Lockers",                new string[]{"Every room has a hiding spot, which can prove vital in a chase."}},
                { "25SparkyPresence",         new string[]{"Sparky can drain the ship's power, requiring a region's electricity to be restored.", "Instability and power failures are big indicators of danger."}},
                { "26FiendPresence",          new string[]{"Having a keen ear will let you hear a monster before seeing it.", "Creeping darkness is a sign of impending danger."}},
                { "27EngineRoom",             new string[]{"The engine room is a trove of useful items, but can be dangerous to navigate."}},
                { "28SteamShutoff",           new string[]{"The ship's steam can be shut off, but doing so can prove difficult.", "The main steam valve is located in engine room workshop number 2."}},
                { "29Sparky",                 new string[]{"Sparky is a monster adept at lurking the player and interfering with the ship's power.", "Sparky was Monstrum's pre-alpha monster, reimagined in the mod with additional abilities.", "What is Sparky?"}},
                { "30Monstrum2Brute",         new string[]{"The Brute can excel even under extreme water pressure."}},
                { "31Darkless",               new string[]{"Mod settings let you create unique gameplay, such as in the Darkless challenge."}},
                { "32Subconscious",           new string[]{"Mod settings let you create a unique atmosphere, such as in the Subconscious challenge."}},
                { "33Underwater",             new string[]{"Mod settings let you create a unique environment, such as in the Underwater challenge."}},
                { "34Multiplayer",            new string[]{"Multiplayer mode lets you play with a friend on your PC given an extra controller. Third-party software enables online play."}},
                { "35Monstrum2SeaFort",       new string[]{"Genetic research on the monsters continued into the 21st century aboard a seemingly derelict array of sea forts.", "The monsters we know were created by the Hongsha Miller corporation as weapons."}},
                { "36DarkShip",               new string[]{"In Dark Ship mode, the Hisa Maru is plunged into darkness.", "Some monsters may benefit more than others from darkness.", "Dark ship, fog and light settings let you create the perfect horror atmosphere."}},
                { "37BrutePresence",          new string[]{"Paying attention to your surroundings can mean the difference between life and death.", "Sometimes a monster's location can be determined even without seeing it directly."}},
                { "38Bunks",                  new string[]{"The player can rotate their head under the bed while down the Shift key."}},
                { "39Welder",                 new string[]{"Locked doors can be opened from the outside with the help of a welding machine."}},
                { "40SurrealGeneration",      new string[]{"How about surreal generation?"}},
                { "41DeckZero",               new string[]{"Deck Zero is a dangerous stretch of corridors forming the deepest parts of the ship.", "Adding deck zero lets you dive even deeper into the depths of the steel maze."}},
                { "42DeactivatedItems",       new string[]{"Mod settings can bring back the deactivated compass and walkie-talkie."}},
                { "43ColourSettings",         new string[]{"Colour and light settings play a big part in shaping the atmosphere of the Hisa Maru."}},
                { "44SmokeGrenade",           new string[]{"Smoke grenades can block the monster's vision and even momentarily stun it."}},
                { "45Molotov",                new string[]{"The molotov cocktail lets you create a wall of fire when needed, which may avert even the hottest of monsters."}},
                { "46OverpoweredSteam",       new string[]{"Steam onboard the Hisa Maru can be quite dangerous, especially with additional modifications...", "Some settings can make the ship even more dangerous than the monsters themselves."}},
                { "47Monstrum2Monsters",      new string[]{"The trio we know are not the only monsters created in Hongsha Miller's experiments."}},
                { "48SparkyEasterEgg1",       new string[]{"Congratulations! You have stumbled upon the golden Sparky. This is the original model designed for the Monstrum 1 pre-alpha."}},
                { "49SparkyEasterEgg2",       new string[]{"Don't let Sparky catch you in the dark...", "It is unwise to let Sparky drain all the ship's power..." }},
                { "50SparkyEasterEgg3",       new string[]{"Implementing a new monster takes a lot of time and dedication, but opens up the possibility for many new experiences."}},
            };

        /// <summary>
        /// Supports custom loading screens including an option to disable them.
        /// </summary>
        private void HookLoadingBackground(On.LoadingBackground.orig_Awake orig, LoadingBackground loadingBackground)
        {
            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Menus" || !ExtendedSettingsModScript.ModSettings.skippedMenuScreen)
            {
                Hints.HintCollection randomHint = Hints.GetRandomHint();

                if (!ExtendedSettingsModScript.ModSettings.disableCustomLoadingText)
                {
                    //Debug.Log("Hints length before is: " + Hints.instance.hints.Length);

                    // Create an extended loading screen array of the default loading screens with the custom ones.
                    if (extendedLoadingScreenArray == null)
                    {
                        List<Hints.HintCollection> extendedLoadingScreenList = new List<Hints.HintCollection>(Hints.instance.hints);
                        UnityEngine.Object[] loadingScreensUnpacked = ExtendedSettingsModScript.Utilities.LoadAssetBundle("loadingscreens");
                        foreach (UnityEngine.Object loadingScreenObject in loadingScreensUnpacked)
                        {
                            if (loadingScreenObject.GetType() == typeof(Texture2D))
                            {
                                Texture2D loadingScreenTexture = (Texture2D)loadingScreenObject;
                                Sprite loadingScreenSprite = Sprite.Create(loadingScreenTexture, randomHint.texture.rect, randomHint.texture.pivot, randomHint.texture.pixelsPerUnit);

                                Hints.HintCollection customHint = new Hints.HintCollection();
                                customHint.texture = loadingScreenSprite;
                                if (customLoadingScreenMessages.ContainsKey(loadingScreenTexture.name))
                                {
                                    customHint.hints = customLoadingScreenMessages[loadingScreenTexture.name];
                                }
                                else
                                {
                                    Debug.Log("Could not find custom text for hint: " + loadingScreenTexture.name);
                                    customHint.hints = randomHint.hints;
                                }

                                extendedLoadingScreenList.Add(customHint);
                            }
                        }
                        extendedLoadingScreenArray = extendedLoadingScreenList.ToArray();
                        Debug.Log("Loaded custom loading screens");
                        /*
                        Image image = loadingBackground.GetComponent<Image>();
                        Debug.Log("Image size:" + image.rectTransform.sizeDelta);
                        Debug.Log("anchorMin: " + image.rectTransform.anchorMin);
                        Debug.Log("anchorMax: " + image.rectTransform.anchorMax);
                        Debug.Log("pivot: " + image.rectTransform.pivot);
                        Debug.Log("Preserve aspect: " + image.preserveAspect);
                        Debug.Log("anchoredPosition: " + image.rectTransform.anchoredPosition);
                        Debug.Log("anchoredPosition3D: " + image.rectTransform.anchoredPosition3D);
                        CanvasScaler canvasScaler = image.canvas.GetComponent<CanvasScaler>();
                        Debug.Log("canvasScaler: " + canvasScaler);
                        Debug.Log("canvasScaler scaleFactor:" + canvasScaler.scaleFactor);
                        Debug.Log("canvasScaler uiScaleMode:" + canvasScaler.uiScaleMode);
                        Debug.Log("canvasScaler matchWidthOrHeight:" + canvasScaler.matchWidthOrHeight);
                        Debug.Log("canvasScaler screenMatchMode:" + canvasScaler.screenMatchMode);
                        Debug.Log("canvasScaler referenceResolution:" + canvasScaler.referenceResolution);
                        Debug.Log("canvasScaler referencePixelsPerUnit:" + canvasScaler.referencePixelsPerUnit);
                        */
                    }

                    // Choose a hint from the extended array.
                    Hints.instance.hints = extendedLoadingScreenArray;
                    randomHint = Hints.GetRandomHint();

                    // Repick the hint if it is an easter egg and the random chance is not low enough.
                    // This makes the easter egg hints rarer.
                    if (randomHint.texture.texture.name.Contains("EasterEgg") && UnityEngine.Random.value > 0.02f)
                    {
                        for (int tries = 0; tries < 10 && randomHint.texture.texture.name.Contains("EasterEgg"); tries++)
                        {
                            randomHint = Hints.GetRandomHint();
                        }
                    }

                    //Debug.Log("Hints length after is: " + Hints.instance.hints.Length);
                }

                LoadingBackground.loadingSprite = randomHint.texture;
                if (randomHint.hints.Length != 0)
                {
                    LoadingBackground.hintText = randomHint.hints[UnityEngine.Random.Range(0, randomHint.hints.Length)];
                }
                else
                {
                    LoadingBackground.hintText = Hints.GetRandomGlobalHint();
                }
            }

            if (loadingBackground.gameObject.GetComponent<LoadingScreenInputHandler>() == null)
            {
                LoadingScreenInputHandler inputHandler = loadingBackground.gameObject.AddComponent<LoadingScreenInputHandler>();
                inputHandler.loadingBackground = loadingBackground;

                int currentScreenIndexLocal = -1;
                if (extendedLoadingScreenArray != null && LoadingBackground.loadingSprite != null)
                {
                    for (int i = 0; i < extendedLoadingScreenArray.Length; i++)
                    {
                        if (extendedLoadingScreenArray[i].texture == LoadingBackground.loadingSprite)
                        {
                            currentScreenIndexLocal = i;
                            break;
                        }
                    }
                }

                int currentTextIndexLocal = -1;
                if (currentScreenIndexLocal != -1 && extendedLoadingScreenArray[currentScreenIndexLocal].hints != null)
                {
                    currentTextIndexLocal = System.Array.IndexOf(extendedLoadingScreenArray[currentScreenIndexLocal].hints, LoadingBackground.hintText);
                }

                inputHandler.currentScreenIndex = currentScreenIndexLocal >= 0 ? currentScreenIndexLocal : 0;
                inputHandler.currentTextIndex = currentTextIndexLocal;
            }

            // Set the loading background without any progress text.
            SetLoadingText(loadingBackground);
        }

        /// <summary>
        /// Sets the loading screen background and text.
        /// Shows custom hints, the mod version, active challenge and any error.
        /// </summary>
        /// <param name="loadingBackground">The loading background to use.</param>
        /// <param name="loadingProgressText">Additional text to indicate loading progress.</param>
        public static void SetLoadingText(LoadingBackground loadingBackground, string loadingProgressText = "")
        {
            if (!ExtendedSettingsModScript.ModSettings.errorDuringLevelGeneration && !ExtendedSettingsModScript.ModSettings.disableCustomLoadingText) // Don't update the text if an error has occurred during level generation.
            {
                loadingBackground.text.text = string.Empty;

                if (LoadingBackground.loadingSprite != null)
                {
                    if (!OculusManager.oculusEnabledOnStart)
                    {
                        ((MonoBehaviour)loadingBackground).GetComponent<UnityEngine.UI.Image>().sprite = LoadingBackground.loadingSprite;
                        loadingBackground.text.text = LoadingBackground.hintText;
                    }
                    else
                    {
                        ((MonoBehaviour)loadingBackground).GetComponent<UnityEngine.UI.Image>().sprite = loadingBackground.blackSprite;
                        loadingBackground.text.text = string.Empty;
                    }
                }

                if (LoadingBackground.loadingSprite != null && !ExtendedSettingsModScript.ModSettings.disableCustomLoadingText)
                {
                    if (!ExtendedSettingsModScript.ModSettings.errorWhileReadingModSettings)
                    {
                        if (ExtendedSettingsModScript.ModSettings.currentChallenge == null)
                        {
                            loadingBackground.text.text += "\n\nMonstrum Extended Settings Mod Version " + ExtendedSettingsModScript.VERSION_WITH_TEXT + " Active";
                        }
                        else
                        {
                            loadingBackground.text.text += "\n\nMESM Version " + ExtendedSettingsModScript.VERSION_WITH_TEXT + " Active With Challenge: " + ExtendedSettingsModScript.ModSettings.currentChallenge.name;
                        }
                    }
                    else
                    {
                        loadingBackground.text.text += "\n\nError While Reading " + ExtendedSettingsModScript.ModSettings.modSettingsErrorString + " Mod Settings - Fix Required";
                    }

                    if (!ExtendedSettingsModScript.ModSettings.skippedMenuScreen)
                    {
                        loadingBackground.text.text += " [Skipped Menu Screen]";
                    }

                    if (!loadingProgressText.Equals(""))
                    {
                        loadingBackground.text.text += "\n" + loadingProgressText;
                    }
                }
            }
        }

        private class LoadingScreenInputHandler : MonoBehaviour
        {
            public LoadingBackground loadingBackground;
            public int currentScreenIndex = 0;
            public int currentTextIndex = -1;
            private float autoSwitchTimer = 10f;

            private void Update()
            {
                try
                {
                    if (extendedLoadingScreenArray == null || extendedLoadingScreenArray.Length == 0) return;

                    bool isEasterEgg = false;
                    Sprite sprite = extendedLoadingScreenArray[currentScreenIndex].texture;
                    if (sprite != null && sprite.texture != null && sprite.texture.name.Contains("EasterEgg"))
                    {
                        isEasterEgg = true;
                    }

                    // Auto-Switch Timer
                    if (!isEasterEgg)
                    {
                        autoSwitchTimer -= Time.deltaTime;
                        if (autoSwitchTimer <= 0f)
                        {
                            ChangeScreen(1);
                        }
                    }

                    // Manual Screen Navigation
                    if (KeyBinds.RightKeyBind.JustPressed() || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetMouseButtonDown(0))
                    {
                        ChangeScreen(1);
                    }
                    else if (KeyBinds.LeftKeyBind.JustPressed() || Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetMouseButtonDown(1))
                    {
                        ChangeScreen(-1);
                    }

                    // Manual Text Navigation
                    if (KeyBinds.BackKeyBind.JustPressed() || Input.GetKeyDown(KeyCode.DownArrow) || Input.mouseScrollDelta.y < 0)
                    {
                        ChangeText(1);
                    }
                    else if (KeyBinds.ForwardKeyBind.JustPressed() || Input.GetKeyDown(KeyCode.UpArrow) || Input.mouseScrollDelta.y > 0)
                    {
                        ChangeText(-1);
                    }
                }
                catch (Exception e)
                {
                    Debug.Log("EXCEPTION in LoadingScreenInputHandler.Update: " + e.ToString());
                }
            }

            private void ChangeScreen(int direction)
            {
                int startIndex = currentScreenIndex;
                int newIndex = currentScreenIndex;
                int maxScreens = extendedLoadingScreenArray.Length;

                do
                {
                    newIndex = (newIndex + direction + maxScreens) % maxScreens;

                    // Stop if we looped back to the start
                    if (newIndex == startIndex) break;

                    // Check if it's an easter egg and we need to skip
                    bool nextIsEasterEgg = false;
                    Sprite nextSprite = extendedLoadingScreenArray[newIndex].texture;
                    if (nextSprite != null && nextSprite.texture != null && nextSprite.texture.name.Contains("EasterEgg"))
                    {
                        nextIsEasterEgg = true;
                    }

                    if (!nextIsEasterEgg || ExtendedSettingsModScript.ModSettings.debugMode)
                    {
                        currentScreenIndex = newIndex;
                        break;
                    }
                } while (true);

                // Start text variation at 0 when changing screens
                if (extendedLoadingScreenArray[currentScreenIndex].hints != null && extendedLoadingScreenArray[currentScreenIndex].hints.Length > 0)
                {
                    currentTextIndex = ExtendedSettingsModScript.ModSettings.consistentLevelGeneration ? 0 : UnityEngine.Random.Range(0, extendedLoadingScreenArray[currentScreenIndex].hints.Length);
                }
                else
                {
                    currentTextIndex = -1;
                }

                UpdateLoadingScreen();
            }

            private void ChangeText(int direction)
            {
                string[] hints = extendedLoadingScreenArray[currentScreenIndex].hints;
                if (hints != null && hints.Length > 0)
                {
                    int maxText = hints.Length;
                    currentTextIndex = (currentTextIndex + direction + maxText) % maxText;
                    UpdateLoadingScreen();
                }
            }

            private void UpdateLoadingScreen()
            {
                autoSwitchTimer = 10f;
                LoadingBackground.loadingSprite = extendedLoadingScreenArray[currentScreenIndex].texture;
                if (currentTextIndex >= 0 && currentTextIndex < extendedLoadingScreenArray[currentScreenIndex].hints.Length)
                {
                    LoadingBackground.hintText = extendedLoadingScreenArray[currentScreenIndex].hints[currentTextIndex];
                }
                else
                {
                    LoadingBackground.hintText = Hints.GetRandomGlobalHint();
                }
                SetLoadingText(loadingBackground);
            }
        }
    }
}