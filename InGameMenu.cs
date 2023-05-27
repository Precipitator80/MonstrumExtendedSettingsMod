// ~Beginning Of File
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections.Generic;
using System;
using System.Collections;
namespace MonstrumExtendedSettingsMod
{
    public partial class ExtendedSettingsModScript : MonoBehaviour
    {
        /*----------------------------------------------------------------------------------------------------*/
        // ~User Interface

        private static Transform clipboardTransform;
        public static Image dropDownButtonBoxImage; // Arrow box image
        public static Image dropDownArrowImage; // Down arrow image
        public static Image dropDownOptionImage; // Option in drop down menu image

        private static Image referenceCategoryImage;
        private static Text referenceCategoryText;

        private static Image referenceOptionImage;
        private static Text referenceOptionText;

        private static Image referenceBoolButtonImage;
        private static Text referenceBoolButtonText;

        private static Slider referenceSlider;
        private static Image referenceSliderBackgroundImage;
        private static Image referenceSliderFillImage;
        private static Image referenceSliderHandleImage;

        private static MenuSounds menuSounds;

        public static MenuTextImageButton warningBox;

        private static float sizeReduction;

        private static Vector2 largeReferenceSizeDelta;
        private static Vector2 largeButtonSizeDelta;
        private static int largeButtonFontSize;
        private static Vector2 smallButtonSizeDelta;
        private static int smallButtonFontSize;
        private static int mediumFontSize;

        private class ChallengeSubPage : SubMenu
        {
            public Text completionTime;
            public ChallengeSubPage(Challenge challenge, GameObject parentPage, Vector3 parentPageOffset, Button entryButton) : base(challenge.name, parentPage, parentPageOffset, entryButton)
            {
                GameObjectFollowingRectTransform gridParent = new GameObjectFollowingRectTransform(challenge.name, gameObject.transform, new Vector3(0f, -50f, 0));
                gridParent.rectTransform.sizeDelta = new Vector2(5f * gridParent.rectTransform.sizeDelta.x, 7.5f * gridParent.rectTransform.sizeDelta.y);
                GridLayoutGroup parentGridLayoutGroup = gridParent.gameObject.AddComponent<GridLayoutGroup>();
                parentGridLayoutGroup.cellSize = new Vector2(gridParent.rectTransform.sizeDelta.x / 2, gridParent.rectTransform.sizeDelta.y / 2);
                parentGridLayoutGroup.spacing = new Vector2(0f, 10f);

                string[] categories = new string[] { "Author", "Completed", "Difficulty", "Completion Time" };
                Text[] categoriesTextElements = new Text[categories.Length];
                for (int i = 0; i < categories.Length; i++)
                {
                    GameObjectFollowingRectTransform categoryParent = new GameObjectFollowingRectTransform(categories[i], gridParent.gameObject.transform, Vector3.zero);
                    GridLayoutGroup categoryGridLayoutGroup = categoryParent.gameObject.AddComponent<GridLayoutGroup>();
                    categoryGridLayoutGroup.cellSize = new Vector2(parentGridLayoutGroup.cellSize.x, parentGridLayoutGroup.cellSize.y / 2);
                    MenuText title = new MenuText(categories[i], categoryParent.gameObject.transform, Vector3.zero);
                    MenuText textElement = new MenuText(categories[i], categoryParent.gameObject.transform, Vector3.zero);
                    title.text.fontSize = mediumFontSize;
                    textElement.text.fontSize = title.text.fontSize;
                    categoriesTextElements[i] = textElement.text;
                }
                categoriesTextElements[0].text = challenge.author;
                categoriesTextElements[1].text = challenge.completionTime != TimeSpan.MaxValue ? "✓" : "×";
                categoriesTextElements[2].text = challenge.difficulty;
                categoriesTextElements[3].text = challenge.CompletionTimeString();

                ChallengeSettingsList challengeSettingsList = new ChallengeSettingsList(challenge.name, gameObject.transform, gridParent.gameObject.transform.localPosition - new Vector3(0f, 40f + parentGridLayoutGroup.cellSize.y * 2, 0f), challenge.settings);

                // Create a load button.
                MenuTextButton loadButton = new MenuTextButton("Load Challenge", gameObject.transform, new Vector3(250f, 60f, 0f));
                loadButton.text.rectTransform.sizeDelta = largeButtonSizeDelta;
                loadButton.text.fontSize = smallButtonFontSize;
                loadButton.button.onClick.AddListener(delegate ()
                {
                    challenge.ApplyChallenge();
                });

                // Create a delete button.
                MenuTextButton deleteButton = new MenuTextButton("Delete Challenge", gameObject.transform, new Vector3(-250f, 60f, 0f));
                deleteButton.text.rectTransform.sizeDelta = largeButtonSizeDelta;
                deleteButton.text.fontSize = smallButtonFontSize;
                deleteButton.button.onClick.AddListener(delegate ()
                {
                    Debug.Log("Delete challenge: " + challenge.name);
                });
            }
        }

        // Menu to provide functionality to save and load presets / challenges.
        private class ChallengesMenu : SubMenu
        {
            public ChallengesMenu(GameObject parentPage, Vector3 entryButtonOffset) : base("Challenges", parentPage, Vector3.zero, parentPage.transform, entryButtonOffset)
            {
                ChallengesList challengesList = new ChallengesList("ChallengesList", gameObject.transform, new Vector3(0f, -175f, -7.5f));
            }
        }

        // Top page sub menu with buttons to other sub menus.
        private class NavigationSubMenu : SubMenu
        {
            public NavigationSubMenu(string name, GameObject parentPage, Vector3 parentPageOffset, Transform entryButtonParentTransform, Vector3 entryButtonOffset) : base(name, parentPage, parentPageOffset, entryButtonParentTransform, entryButtonOffset)
            {
                // Create a save button.
                MenuTextButton saveButton = new MenuTextButton("Save All Settings", gameObject.transform, exitButtonGO.transform, new Vector3(250f, 435f, 0f), true);
                saveButton.text.rectTransform.sizeDelta = largeButtonSizeDelta;
                saveButton.text.fontSize = smallButtonFontSize;
                saveButton.button.onClick.AddListener(delegate ()
                {
                    MESMSetting.SaveSettings();
                });

                // Create a reset to default button
                MenuTextButton resetButton = new MenuTextButton("Reset To Default (Press Save Afterwards)", gameObject.transform, exitButtonGO.transform, new Vector3(-250f, 435f, 0f), true);
                resetButton.text.rectTransform.sizeDelta = smallButtonSizeDelta;
                resetButton.text.fontSize = smallButtonFontSize;

                resetButton.button.onClick.AddListener(delegate ()
                {
                    MESMSetting.ResetSettingsToDefault(ModSettings.allSettings);
                });

                MenuText versionText = new MenuText("V" + VERSION_WITH_TEXT + "\nPrecipitator", exitButtonGO.transform, new Vector3(-250f, 0f, 0f), false);
                versionText.text.rectTransform.sizeDelta = smallButtonSizeDelta;
                versionText.text.fontSize /= 2;
            }
        }

        // A single page of a settings sub menu.
        private class SettingsSubMenuPage : GameObjectFollowingTransform
        {
            public MenuTextButton nextButton;
            public MenuTextButton previousButton;

            public SettingsSubMenuPage(string name, Transform parentTransform, Transform exitButtonTransform, int index) : base(name, parentTransform, Vector3.zero)
            {
                nextButton = new MenuTextButton("→_" + name + "_NextPage_" + index, gameObject.transform, exitButtonTransform, new Vector3(150f, 20f, 0f), false);
                nextButton.text.rectTransform.sizeDelta = new Vector2(nextButton.text.rectTransform.sizeDelta.x / 1.5f, nextButton.text.rectTransform.sizeDelta.y);

                previousButton = new MenuTextButton("←_" + name + "_LastPage_" + index, gameObject.transform, exitButtonTransform, new Vector3(-150f, 20f, 0f), false);
                previousButton.text.rectTransform.sizeDelta = nextButton.text.rectTransform.sizeDelta;

                nextButton.button.onClick.AddListener(delegate ()
                {
                    menuSounds.ButtonClickGoForward();
                });
                previousButton.button.onClick.AddListener(delegate ()
                {
                    menuSounds.ButtonClickGoBack();
                });
            }
        }

        // Sub menu to show settings of a certain category.
        private class SettingsSubMenu : SubMenu
        {
            List<SettingsSubMenuPage> settingsPages;
            public SettingsSubMenu(string name, GameObject parentPage, Vector3 entryButtonOffset, List<MESMSetting> settings) : this(name, parentPage, Vector3.zero, parentPage.transform, entryButtonOffset, settings)
            {
            }

            public SettingsSubMenu(string name, GameObject parentPage, Vector3 parentPageOffset, Transform entryButtonParentTransform, Vector3 entryButtonOffset, List<MESMSetting> settings) : base(name, parentPage, parentPageOffset, entryButtonParentTransform, entryButtonOffset)
            {
                // Create a save button.
                MenuTextButton saveButton = new MenuTextButton("Save", gameObject.transform, exitButtonGO.transform, new Vector3(0f, 50f, 0f), false);
                saveButton.text.rectTransform.sizeDelta = smallButtonSizeDelta;
                saveButton.button.onClick.AddListener(delegate ()
                {
                    MESMSetting.SaveSettings();
                });

                // Create a reset to default button
                MenuTextButton resetButton = new MenuTextButton("Reset To Default (Press Save Afterwards)", gameObject.transform, exitButtonGO.transform, new Vector3(-250f, 435f, 0f), true);
                resetButton.text.rectTransform.sizeDelta = smallButtonSizeDelta;
                resetButton.text.fontSize = smallButtonFontSize;
                resetButton.button.onClick.AddListener(delegate ()
                {
                    MESMSetting.ResetSettingsToDefault(settings);
                });

                // Create guide text in the top right on settings pages.
                MenuText guideText = new MenuText("Hover over settings to view descriptions", exitButtonGO.transform, new Vector3(250f, 435f, 0f), true);
                guideText.text.rectTransform.sizeDelta = smallButtonSizeDelta;
                guideText.text.fontSize = smallButtonFontSize;

                // Set up variables to correctly place settings on pages.
                int k = 0;
                Vector3 offset = new Vector3(-175f, 0f, 0f) / sizeReduction; //Vector3.zero;//new Vector3(-150f, 0f, 0f);
                int verticalShift = (int)(45 / sizeReduction);
                int maximumOptionsPerRow = 335 / verticalShift;

                // Create pages to hold settings.
                settingsPages = new List<SettingsSubMenuPage>();
                settingsPages.Add(new SettingsSubMenuPage(name, this.gameObject.transform, exitButtonGO.transform, 0));
                for (int i = 0; i < settings.Count; i++)
                {
                    if (settings[i].GetType() == typeof(MESMSettingRGB))
                    {
                        // Check whether this is the first part of the colour setting so that a colour setting is not split across two pages.
                        if (((MESMSettingRGB)settings[i]).colour == MESMSettingRGB.MESMSettingRGBColourEnum.red)
                        {
                            if (2 * maximumOptionsPerRow - k < 3)
                            {
                                k = 2 * maximumOptionsPerRow - 1;
                                i--;
                            }
                            else if (maximumOptionsPerRow - (k % maximumOptionsPerRow) < 3)
                            {
                                i--;
                            }
                            else
                            {
                                // Create colour display text and the first component button.
                                MenuText displayText = new MenuText(settings[i].modSettingsText.Replace(" Red Component", ""), settingsPages[settingsPages.Count - 1].gameObject.transform, new Vector3(0, -(verticalShift * k), 0f) + offset);
                                k++;
                                ((MESMSettingRGB)settings[i]).CreateRGBButton(settingsPages[settingsPages.Count - 1].gameObject.transform, new Vector3(0, -(verticalShift * k), 0f) + offset, displayText.text);
                            }
                        }
                        else
                        {
                            // Create another component button by using the last component's reference to the display text.
                            ((MESMSettingRGB)settings[i]).CreateRGBButton(settingsPages[settingsPages.Count - 1].gameObject.transform, new Vector3(0, -(verticalShift * k), 0f) + offset, ((MESMSettingRGB)settings[i - 1]).displayText);
                        }
                    }
                    else
                    {
                        // Create a button for a setting on a certain page.
                        settings[i].CreateButtonForSetting(settingsPages[settingsPages.Count - 1].gameObject.transform, new Vector3(0, -(verticalShift * k), 0f) + offset);
                    }

                    // Keep an index of column entry to check when to switch to the second and back to the first column.
                    k++;
                    if (k % maximumOptionsPerRow == 0)
                    {
                        offset += new Vector3(400f / sizeReduction, maximumOptionsPerRow * verticalShift, 0f);
                        if (k == 2 * maximumOptionsPerRow && i != settings.Count - 1)
                        {
                            settingsPages.Add(new SettingsSubMenuPage(name, this.gameObject.transform, exitButtonGO.transform, i));
                            offset = new Vector3(-175f, 0f, 0f) / sizeReduction;
                            k = 0;
                        }
                    }
                }
                if (settingsPages.Count > 1)
                {
                    // If multiple pages will be used, disable all parent GameObjects so that one page can be reactivated later.
                    for (int i = 1; i < settingsPages.Count; i++)
                    {
                        settingsPages[i].gameObject.SetActive(false);
                    }

                    for (int i = 0; i < settingsPages.Count; i++)
                    {
                        int storedIndex = i; // Passing a temporary variable to add listener - Mmmpies - https://answers.unity.com/questions/908847/passing-a-temporary-variable-to-add-listener.html - Accessed 15.12.2021
                                             //Debug.Log("Page " + storedIndex);

                        if (storedIndex + 1 == settingsPages.Count)
                        {
                            settingsPages[storedIndex].nextButton.button.onClick.AddListener(delegate ()
                            {
                                //Debug.Log("Using indices 0 and " + storedIndex + " for " + settingsGroups.Count + " (Next alt)");
                                SwitchToPage(settingsPages[0].gameObject, settingsPages[storedIndex].gameObject);
                            });
                        }
                        else
                        {
                            //Debug.Log("Using indices " + (storedIndex + 1) + " and " + storedIndex + " for " + settingsGroups.Count + " (Next normal)");
                            settingsPages[storedIndex].nextButton.button.onClick.AddListener(delegate ()
                            {
                                SwitchToPage(settingsPages[storedIndex + 1].gameObject, settingsPages[storedIndex].gameObject);
                            });
                        }
                        //Debug.Log("Next page buttons assigned");

                        if (storedIndex - 1 == -1)
                        {
                            //Debug.Log("Using indices " + (settingsGroups.Count - 1) + " and " + storedIndex + " for " + settingsGroups.Count + " (Last alt)");
                            settingsPages[storedIndex].previousButton.button.onClick.AddListener(delegate ()
                            {
                                SwitchToPage(settingsPages[settingsPages.Count - 1].gameObject, settingsPages[storedIndex].gameObject);
                            });
                        }
                        else
                        {
                            //Debug.Log("Using indices " + (storedIndex - 1) + " and " + storedIndex + " for " + settingsGroups.Count + " (Last normal)");
                            settingsPages[storedIndex].previousButton.button.onClick.AddListener(delegate ()
                            {
                                SwitchToPage(settingsPages[storedIndex - 1].gameObject, settingsPages[storedIndex].gameObject);
                            });
                        }
                        //Debug.Log("Last page buttons assigned");
                    }
                    //Debug.Log("Finished settings page next and last arrow assignment");
                }
                else if (settingsPages.Count == 1)
                {
                    settingsPages[0].nextButton.gameObject.SetActive(false);
                    settingsPages[0].previousButton.gameObject.SetActive(false);
                }

                // Reverse the order of child transform to layer text correctly.
                // how can I reverse my children indices? - LeftyRighty -  https://forum.unity.com/threads/how-can-i-reverse-my-children-indices.436052/ - Accessed 23.05.2023
                for (int pageNumber = 0; pageNumber < settingsPages.Count; pageNumber++)
                {
                    if (settingsPages[pageNumber].gameObject.transform.childCount > 0)
                    {
                        for (int i = 0; i < settingsPages[pageNumber].gameObject.transform.childCount; i++)
                        {
                            settingsPages[pageNumber].gameObject.transform.GetChild(0).SetSiblingIndex(settingsPages[pageNumber].gameObject.transform.childCount - i);
                        }
                    }
                }
            }
        }

        // SubMenu - A settings sub menu.
        private abstract class SubMenu : CanvasFollowingTransform
        {
            protected GameObject exitButtonGO;

            // name: The name of the menu to display in the parent menu.
            // parentPage: The parent menu page to display a button to the sub menu on.
            // parentPageOffset: The offset from the parent page to use for hte sub page.
            // entryButtonParentTransform: A reference transform of another button to position the entry button.
            // entryButtonOffset: The offset from the entryButtonParentTransform to use.
            protected SubMenu(string name, GameObject parentPage, Vector3 parentPageOffset, Transform entryButtonParentTransform, Vector3 entryButtonOffset) : this(name, parentPage, parentPageOffset)
            {
                MenuTextButton entryButton = new MenuTextButton(name, entryButtonParentTransform, entryButtonOffset, false);
                entryButton.text.rectTransform.sizeDelta = new Vector2(2f * entryButton.text.rectTransform.sizeDelta.x, entryButton.text.rectTransform.sizeDelta.y);
                AddListenersToEntryButton(entryButton.button, parentPage);
            }

            protected SubMenu(string name, GameObject parentPage, Vector3 parentPageOffset, Button entryButton) : this(name, parentPage, parentPageOffset)
            {
                AddListenersToEntryButton(entryButton, parentPage);
            }

            private SubMenu(string name, GameObject parentPage, Vector3 parentPageOffset) : base(name, parentPage.transform, parentPageOffset)
            {
                this.gameObject.transform.SetParent(clipboardTransform, true); // Use the clipboardTransform so that a sub menu is displayed even when parent menus are disabled.

                MenuTextButton exitButton = new MenuTextButton("Exit", this.gameObject.transform, new Vector3(0f, -375f, 0f), false);
                exitButtonGO = exitButton.gameObject;
                exitButton.button.onClick.AddListener(delegate ()
                {
                    SwitchToPage(parentPage, this.gameObject);
                });
                exitButton.button.onClick.AddListener(delegate ()
                {
                    menuSounds.ButtonClickGoBack();
                });

                string shortTitle = name.Split(new string[] { " Settings" }, System.StringSplitOptions.None)[0];
                MenuText titleText = new MenuText(shortTitle, exitButtonGO.transform, new Vector3(0f, (shortTitle.Length > 10 ? 450f : 435f), 0f), false); // Give long titles two lines rather than one.
                titleText.text.rectTransform.sizeDelta = new Vector2(titleText.text.rectTransform.sizeDelta.x, 2f * titleText.text.rectTransform.sizeDelta.y);

                this.gameObject.SetActive(false);
            }

            private void AddListenersToEntryButton(Button entryButton, GameObject parentPage)
            {
                entryButton.onClick.AddListener(delegate ()
                {
                    SwitchToPage(this.gameObject, parentPage);
                });
                entryButton.onClick.AddListener(delegate ()
                {
                    menuSounds.ButtonClickGoForward();
                });
            }
        }

        private static void HookOptionsUIStart(On.OptionsUI.orig_Start orig, OptionsUI optionsUI)
        {
            /*
            Things required for custom menu:
            
            Methods:
            CreateAdditionalOption - Put a button next to the normal options.
            CreatePageForSettingsCategory - Put a large button in the categories sub menu that is accessed via the CreateAdditionalOption method button.

            Reference Texts (Remember to use the image's rect transform for size delta if the text has an image):
            CategoryText - Text for a text button placed on the original options menu or categories sub menu that leads to another page. Large text.
            OptionText - Text for all the options displayed in the deepest menus.
            MenuTitleText - Text at the top of each menu to denote what page it is.
            BoolButtonText - Text for boolean option checkbox.

            Reference Images:
            CategoryImage - Image for the CategoryText text.
            OptionImage - Image for composite text and image buttons like the advanced graphics button.
            BoolButtonImage - Image for boolean option checkbox.

            Types Of Option Buttons:
            string = Text + InputField / Text + MultiChoiceDropDown
            int = Text + InputField
            float = Text + InputField
            bool = Text + BoolButton
            
            Additional Things To Take Into Consideration:
            Some buttons may need input validation, like AbsInt and AbsFlt

            Pseudocode For Creating Menus:
            * Read Mod Settings (Already done automatically when game launches).
            * Assign all reference variables.
            * Create options button leading to the categories menu.
            - This should automatically generate with the title and exit button.
            * Fill categories menu with buttons leading to the categories pages.
            - This should automatically generate with the title as well as "exit" and "save and exit" buttons.
            * Check the associated settings for each page and add in an appropriate button for each.

            Required MESMSetting Variables For Creating Buttons:
            * Name
            * Description
            * Category
            * Button Type
            */

            /*
            Canvas GameObject Hierarchy:
            newCanvas
            - Canvas
            - CanvasScaler
            - GraphicRaycaster
            
            panel
            - Image
            */

            /*
            VSync Button Order:
            RectTransform
            CanvasRenderer
            UI.Image
            UI.Button
            */

            /*
            buttonList[0] has k = 9
            buttonList[1] has k = 24
            buttonList[2] has k = 33
            buttonList[3] has k = 42
            ClipboardTransform has i = 5 and j = 0 and k = 9
            Reference category image has k = 41
            gms has i = 4 and k = 42
            Reference option image has index 18
            Reference slider has index 19
            Reference option text has index 22
            BoolButtonText has i = 82
            BoolButtonImage has i = 95
            TextureQuality has i = 103
            */

            // Assign all the reference components from indices discovered through foreach loops in previous versions.
            UnityEngine.Object[] optionsButtonsObjects = optionsUI.optionsButtons.GetComponentsInChildren<UnityEngine.Object>();

            List<Button> buttonList = new List<Button>();

            MenuUI menuUI = optionsButtonsObjects[26] as MenuUI;
            menuSounds = FindObjectOfType<MenuSounds>();
            ExitButton exitButton = FindObjectOfType<ExitButton>();

            buttonList.Add((Button)optionsButtonsObjects[9]);
            buttonList.Add((Button)optionsButtonsObjects[24]);
            buttonList.Add((Button)optionsButtonsObjects[33]);
            buttonList.Add((Button)optionsButtonsObjects[42]);

            Button clipboardTransformButton = (Button)optionsButtonsObjects[9];
            GameObject clipboardTransformGameObject = (GameObject)clipboardTransformButton.onClick.GetPersistentTarget(5);
            clipboardTransform = clipboardTransformGameObject.GetComponentsInChildren<Transform>()[0];

            referenceCategoryImage = Instantiate((Image)optionsButtonsObjects[41]);

            Button gmsButton = (Button)optionsButtonsObjects[42];
            GraphicsMenuSorter gms = (GraphicsMenuSorter)gmsButton.onClick.GetPersistentTarget(4);
            UnityEngine.Object[] graphicsMenuObjects = gms.graphicsMenu.GetComponentsInChildren<UnityEngine.Object>();

            referenceOptionImage = Instantiate((Image)graphicsMenuObjects[18]);

            Button sliderButton = (Button)graphicsMenuObjects[19];
            referenceSlider = (Slider)((GameObject)sliderButton.onClick.GetPersistentTarget(1)).GetComponentsInChildren<UnityEngine.Object>()[110];
            referenceSliderBackgroundImage = Instantiate(referenceSlider.image);
            referenceSliderFillImage = Instantiate(referenceSlider.fillRect.gameObject.GetComponent<Image>());
            referenceSliderHandleImage = Instantiate(referenceSlider.handleRect.gameObject.GetComponent<Image>());

            referenceOptionText = Instantiate((Text)graphicsMenuObjects[22]);

            referenceBoolButtonText = Instantiate((Text)graphicsMenuObjects[82]);
            referenceBoolButtonImage = Instantiate((Image)graphicsMenuObjects[95]);

            dropDownButtonBoxImage = Instantiate((Image)graphicsMenuObjects[106]);
            dropDownArrowImage = Instantiate((Image)graphicsMenuObjects[111]);
            dropDownOptionImage = Instantiate((Image)graphicsMenuObjects[114]);

            referenceCategoryText = Instantiate(menuUI.GetComponentsInChildren<UnityEngine.Object>()[8] as Text);

            // Adjust the size of all the references so that the settings fit on the clipboard.
            sizeReduction = 1.35f;
            referenceOptionImage.rectTransform.sizeDelta /= sizeReduction;
            referenceOptionText.rectTransform.sizeDelta /= sizeReduction;
            referenceOptionText.fontSize = (int)(referenceOptionText.fontSize / sizeReduction);
            referenceBoolButtonImage.rectTransform.sizeDelta /= sizeReduction;
            referenceBoolButtonText.rectTransform.sizeDelta /= sizeReduction;
            referenceBoolButtonText.fontSize = (int)(referenceOptionText.fontSize / sizeReduction);
            dropDownButtonBoxImage.rectTransform.sizeDelta /= sizeReduction;
            dropDownArrowImage.rectTransform.sizeDelta /= sizeReduction;
            dropDownOptionImage.rectTransform.sizeDelta /= sizeReduction;

            // Create reference sizes.
            largeReferenceSizeDelta = referenceCategoryImage.rectTransform.sizeDelta + new Vector2(0.25f * referenceCategoryImage.rectTransform.sizeDelta.x, 0);
            largeButtonSizeDelta = new Vector2(1.2f * largeReferenceSizeDelta.x, 3f * largeReferenceSizeDelta.y);
            largeButtonFontSize = (int)(1.25f * referenceCategoryText.fontSize);
            smallButtonSizeDelta = new Vector2(1.2f * referenceOptionImage.rectTransform.sizeDelta.x, 3f * referenceOptionImage.rectTransform.sizeDelta.y);
            smallButtonFontSize = (int)(1.25f * referenceOptionText.fontSize);
            mediumFontSize = (2 * referenceOptionText.fontSize);

            // Create the initial navigation sub menu.
            NavigationSubMenu navigationSubMenu = new NavigationSubMenu("MES Mod", optionsUI.optionsButtons, new Vector3(0f, 140f, -5f), buttonList[3].transform.parent, (buttonList[3].transform.localPosition - buttonList[3].transform.parent.localPosition) - 3f * (buttonList[3].transform.localPosition - buttonList[1].transform.localPosition));

            // Discover all the categories used for the settings in order to set up pages for them.
            List<string> categories = new List<string>();
            for (int i = 0; i < ModSettings.allSettings.Count; i++)
            {
                if (!categories.Contains(ModSettings.allSettings[i].category))
                {
                    categories.Add(ModSettings.allSettings[i].category);
                }
            }

            // Discover which page each setting should be assigned to.
            List<List<MESMSetting>> settingsAssociatedToCategory = new List<List<MESMSetting>>();
            for (int i = 0; i < categories.Count; i++)
            {
                settingsAssociatedToCategory.Add(new List<MESMSetting>());
                for (int j = 0; j < ModSettings.allSettings.Count; j++)
                {
                    if (ModSettings.allSettings[j].category.Equals(categories[i]))
                    {
                        settingsAssociatedToCategory[i].Add(ModSettings.allSettings[j]);
                    }
                }
            }

            // Create pages for all of the categories.
            for (int i = 0; i < categories.Count; i++)
            {
                SettingsSubMenu settingsSubMenu = new SettingsSubMenu(categories[i], navigationSubMenu.gameObject, new Vector3(0f, -(52.5f * i), 0f), settingsAssociatedToCategory[i]);
            }

            ChallengesMenu challengesMenu = new ChallengesMenu(navigationSubMenu.gameObject, new Vector3(0f, -(52.5f * categories.Count), 0f));

            // Create a warning box to alert the user of restart conditions and errors.
            // Use a base canvas and adjust its sorting order to appear on top of the menu buttons.
            CanvasFollowingTransform warningBoxCanvas = new CanvasFollowingTransform("WarningBox", optionsUI.optionsButtons.transform, new Vector3(0f, 0f, -7.5f));
            warningBox = new MenuTextImageButton("WarningBox", warningBoxCanvas.gameObject.transform, Vector3.zero);
            warningBoxCanvas.gameObject.transform.SetParent(clipboardTransform, true);
            warningBoxCanvas.canvas.sortingOrder = 2;

            // Adjust the area and opacity of the image.
            warningBox.image.rectTransform.sizeDelta = new Vector2(5f * warningBox.image.rectTransform.sizeDelta.x, 15f * warningBox.image.rectTransform.sizeDelta.y); // Big variant
            warningBox.image.color = new Color(warningBox.image.color.r, warningBox.image.color.g, warningBox.image.color.b, 1f);

            // Change the area and size of the text.
            warningBox.text.rectTransform.sizeDelta = new Vector2(0.975f * warningBox.image.rectTransform.sizeDelta.x, 0.975f * warningBox.image.rectTransform.sizeDelta.y);

            // Add functionality to the box.
            warningBox.button.onClick.AddListener(delegate ()
            {
                warningBox.gameObject.SetActive(false);
            });
            warningBox.button.onClick.AddListener(delegate ()
            {
                menuSounds.ButtonClickGoBack();
            });

            // Set it inactive so that it is not show when first in the menu.
            warningBox.gameObject.SetActive(false);
        }

        // Method to switch between sub pages and settings sub-sub-pages
        private static void SwitchToPage(GameObject newPage, GameObject originalPage)
        {
            originalPage.SetActive(false);
            newPage.SetActive(true);
        }

        public class MenuMultipleChoiceButtonWithDescription : MenuDescriptionBox
        {
            MenuTextImage selectedOptionImage;
            public MenuMultipleChoiceButtonWithDescription(string description, string name, string[] choices, Transform parentTransform, Vector3 referenceOffset, bool smallText = true) : base(description, name, parentTransform, referenceOffset, smallText)
            {
                float xShift = Mathf.Abs((dropDownOptionImage.rectTransform.sizeDelta.x - ((referenceOptionImage.rectTransform.sizeDelta.x / 2.25f) / sizeReduction)) / 2);
                Vector3 xShiftVector = new Vector3(xShift, 0f, 0f);

                //Debug.Log("dropDownOptionImage.rectTransform.sizeDelta.x = " + dropDownOptionImage.rectTransform.sizeDelta.x + ", referenceOptionImage.rectTransform.sizeDelta.x = " + referenceOptionImage.rectTransform.sizeDelta.x + ", ((referenceOptionImage.rectTransform.sizeDelta.x / 2.25f) / sizeReduction) = " + ((referenceOptionImage.rectTransform.sizeDelta.x / 2.25f) / sizeReduction) + ",(dropDownOptionImage.rectTransform.sizeDelta.x - ((referenceOptionImage.rectTransform.sizeDelta.x / 2.25f) / sizeReduction)) / 2 = " + (dropDownOptionImage.rectTransform.sizeDelta.x - ((referenceOptionImage.rectTransform.sizeDelta.x / 2.25f) / sizeReduction)) / 2 + ", Total = " + xShift);
                float arrowShift = dropDownOptionImage.rectTransform.sizeDelta.x / 2 + 10f / sizeReduction;/*62.5f / sizeReduction;*/
                Vector3 arrowShiftVector = new Vector3(arrowShift, 0f, 0f);
                selectedOptionImage = new MenuTextImage(name, nameTextCustomTransform, nameOffsetConstant * nameTextCustomTransform.localPosition + xShiftVector, smallText);
                MenuImageButton arrowImageBase = new MenuImageButton(name, selectedOptionImage.gameObject.transform, arrowShiftVector, true);
                MenuImage arrowImage = new MenuImage(name, arrowImageBase.gameObject.transform, Vector3.zero);

                selectedOptionImage.image.rectTransform.sizeDelta = dropDownOptionImage.rectTransform.sizeDelta;
                arrowImageBase.image.rectTransform.sizeDelta = dropDownButtonBoxImage.rectTransform.sizeDelta;

                arrowImage.image.rectTransform.sizeDelta = dropDownArrowImage.rectTransform.sizeDelta;
                arrowImage.image.sprite = dropDownArrowImage.sprite;
                arrowImage.image.color = dropDownArrowImage.color;
                arrowImage.image.raycastTarget = false;

                // Create and resize a dropdown box and then put dropdown choices on top of it.
                List<GameObject> dropdownGOs = new List<GameObject>();
                MenuImage backgroundImage = new MenuImage(name + "DROPDOWN_BOX", selectedOptionImage.gameObject.transform, Vector3.zero);
                backgroundImage.image.rectTransform.sizeDelta = new Vector2(selectedOptionImage.image.rectTransform.sizeDelta.x, choices.Length * selectedOptionImage.image.rectTransform.sizeDelta.y);
                backgroundImage.image.color = new Color(backgroundImage.image.color.r, backgroundImage.image.color.g, backgroundImage.image.color.b, 1f);

                for (int i = 0; i < choices.Length; i++)
                {
                    MenuTextImageButton choiceGOs = new MenuTextImageButton(choices[i] + "_DROPDOWN_CHOICE", selectedOptionImage.gameObject.transform, new Vector3(0f, (i + 1) * -selectedOptionImage.image.rectTransform.sizeDelta.y, 0f), smallText, true);
                    choiceGOs.image.rectTransform.sizeDelta = selectedOptionImage.image.rectTransform.sizeDelta;
                    dropdownGOs.Add(choiceGOs.gameObject);

                    int storableInt = i;
                    choiceGOs.button.onClick.AddListener(delegate ()
                    {
                        selectedOptionImage.text.text = choices[storableInt];
                        foreach (GameObject gameObject in dropdownGOs)
                        {
                            gameObject.SetActive(false);
                        }
                        choiceGOs.image.color = referenceOptionImage.color;
                    });
                }

                // Reposition the background image. Ideally this would be done through anchoring and then a constant shift below the displayed choice, but I could not figure out how to do this.
                if (dropdownGOs.Count % 2 == 1)
                {
                    // The middle choice.
                    backgroundImage.image.transform.position = dropdownGOs[dropdownGOs.Count / 2].transform.position;
                }
                else
                {
                    // The middle of the middle two choices.
                    backgroundImage.image.transform.position = dropdownGOs[dropdownGOs.Count / 2].transform.position;
                    backgroundImage.image.transform.position -= (dropdownGOs[(dropdownGOs.Count / 2) + 1].transform.position - dropdownGOs[dropdownGOs.Count / 2].transform.position) / 2f;
                }

                dropdownGOs.Add(backgroundImage.gameObject);

                foreach (GameObject gameObject in dropdownGOs)
                {
                    gameObject.SetActive(false);
                }

                // Add button functionality to the drop down menu.
                arrowImageBase.button.onClick.AddListener(delegate ()
                {
                    if (backgroundImage.gameObject.activeSelf)
                    {
                        foreach (GameObject gameObject in dropdownGOs)
                        {
                            gameObject.SetActive(false);
                        }
                    }
                    else
                    {
                        foreach (GameObject gameObject in dropdownGOs)
                        {
                            gameObject.SetActive(true);
                        }
                    }
                });
            }

            public override string GetText()
            {
                return selectedOptionImage.text.text;
            }

            public override void SetText(string value)
            {
                selectedOptionImage.text.text = value;
            }
        }

        public class MenuBoolButtonWithDescription : MenuDescriptionBox
        {
            public MenuBoolButton menuBoolButton;
            public MenuBoolButtonWithDescription(string description, string name, Transform parentTransform, Vector3 referenceOffset, bool smallText = true) : base(description, name, parentTransform, referenceOffset, smallText)
            {
                menuBoolButton = new MenuBoolButton(name, nameTextCustomTransform, nameOffsetConstant * nameTextCustomTransform.localPosition, smallText);
            }

            public override string GetText()
            {
                return menuBoolButton.text.text;
            }

            public override void SetText(string value)
            {
                menuBoolButton.SetValueAndUpdateText(bool.Parse(value));
            }
        }

        public class MenuSliderInputFieldWithDescription : MenuInputFieldWithDescription
        {
            // Use an input field as part of the base to hold the value for the slider and to allow for manual entry of the value.
            public MenuSlider menuSlider;
            public MenuSliderInputFieldWithDescription(string description, string name, Transform parentTransform, Vector3 referenceOffset, bool useInt, float minClamp, float maxClamp, bool smallText = true) : base(description, name, parentTransform, referenceOffset, useInt, minClamp, maxClamp, smallText)
            {
                menuSlider = new MenuSlider(name, nameTextCustomTransform, nameOffsetConstant * nameTextCustomTransform.localPosition + new Vector3(62.5f, 0f, 0f), useInt, minClamp, maxClamp);
                menuSlider.slider.onValueChanged.AddListener((sliderValue) => { menuInputField.inputField.text = sliderValue.ToString(); });

                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;
                entry.callback.AddListener((eventData) =>
                {
                    menuInputField.inputField.StartCoroutine(UpdateSliderAfterInputField(menuInputField.inputField, menuSlider.slider));
                });
                menuInputField.eventTrigger.triggers.Add(entry);
            }

            private static IEnumerator UpdateSliderAfterInputField(InputField inputField, Slider slider)
            {
                yield return null;
                while (inputField.isFocused)
                {
                    yield return null;
                }
                // This is where you should update the slider
                slider.value = Convert.ToSingle(inputField.text);
                yield break;
            }
        }

        public class MenuInputFieldWithDescription : MenuDescriptionBox
        {
            public MenuInputField menuInputField;
            public MenuInputFieldWithDescription(string description, string name, Transform parentTransform, Vector3 referenceOffset, bool useInt, float minClamp, float maxClamp, bool smallText = true) : base(description, name, parentTransform, referenceOffset, smallText)
            {
                menuInputField = new MenuInputField(name, nameTextCustomTransform, nameOffsetConstant * nameTextCustomTransform.localPosition, useInt, minClamp, maxClamp, smallText);
            }

            public override string GetText()
            {
                return menuInputField.inputField.text;
            }

            public override void SetText(string value)
            {
                menuInputField.inputField.text = value;
            }
        }

        public abstract class MenuDescriptionBox : GameObjectFollowingTransform
        {
            protected static float nameOffsetConstant = -1.7f;
            public MenuTextImage descriptionMenuTextImage;
            private MenuText nameMenuText;
            protected Transform nameTextCustomTransform;
            public MenuDescriptionBox(string description, string name, Transform parentTransform, Vector3 referenceOffset, bool smallText = true) : base(name + "DESCRIPTION_BOX_BASE", parentTransform, referenceOffset)
            {
                // Create a graphic for the description box with a canvas that follows a separate transform and has a higher sorting order to allow it to be displayed on top of other settings.
                CanvasFollowingTransform descriptionBoxCanvas = new CanvasFollowingTransform(name, gameObject.transform, Vector3.zero);
                descriptionMenuTextImage = new MenuTextImage(name + "DESCRIPTION_BOX", descriptionBoxCanvas.gameObject.transform, new Vector3(0f, 0f, -5f));
                descriptionBoxCanvas.gameObject.transform.SetParent(parentTransform.parent, true);
                descriptionBoxCanvas.canvas.sortingOrder = 1;

                descriptionMenuTextImage.image.rectTransform.sizeDelta = new Vector2(2.75f * descriptionMenuTextImage.image.rectTransform.sizeDelta.x, 5f * descriptionMenuTextImage.image.rectTransform.sizeDelta.y * ((description.Length + (110f - (description.Length % 55f))) / 325f));
                descriptionMenuTextImage.image.color = new Color(descriptionMenuTextImage.image.color.r, descriptionMenuTextImage.image.color.g, descriptionMenuTextImage.image.color.b, 1f);
                descriptionMenuTextImage.text.rectTransform.sizeDelta = new Vector2(0.975f * descriptionMenuTextImage.image.rectTransform.sizeDelta.x, 0.975f * descriptionMenuTextImage.image.rectTransform.sizeDelta.y);
                descriptionMenuTextImage.text.text = description;

                GameObjectFollowingTransform nameTextParent = new GameObjectFollowingTransform(name, gameObject.transform, -(new Vector3(0.25f * referenceOptionImage.rectTransform.sizeDelta.x, 0f, 0f) / sizeReduction + new Vector3(60f, 0f, 0f) / sizeReduction));
                nameTextCustomTransform = nameTextParent.gameObject.transform;

                nameMenuText = new MenuText(name, nameTextCustomTransform, Vector3.zero, smallText);
                nameMenuText.text.rectTransform.sizeDelta = new Vector2(1.5f * nameMenuText.text.rectTransform.sizeDelta.x, 1.5f * nameMenuText.text.rectTransform.sizeDelta.y);
                nameMenuText.text.raycastTarget = true;
                nameMenuText.text.alignment = TextAnchor.MiddleRight;

                EventTrigger revealCheckTrigger = nameMenuText.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
                pointerEnter.eventID = EventTriggerType.PointerEnter;
                pointerEnter.callback.AddListener((eventData) =>
                {
                    AudioSystem.instance.StartCoroutine(RevealDescriptionAfterTime(descriptionMenuTextImage.gameObject, nameMenuText.gameObject, revealCheckTrigger));
                });
                revealCheckTrigger.triggers.Add(pointerEnter);

                EventTrigger hideTrigger = descriptionMenuTextImage.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry pointerExit = new EventTrigger.Entry();
                pointerExit.eventID = EventTriggerType.PointerExit;
                pointerExit.callback.AddListener((eventData) =>
                {
                    descriptionMenuTextImage.gameObject.SetActive(false);

                    nameMenuText.gameObject.SetActive(true);
                });
                hideTrigger.triggers.Add(pointerExit);

                descriptionMenuTextImage.gameObject.SetActive(false);
            }

            // Methods to get and set the text component of the object corresponding to the selected user value.
            public abstract string GetText();
            public abstract void SetText(string value);

            private static IEnumerator RevealDescriptionAfterTime(GameObject descriptionGameObject, GameObject settingsButtonGameObject, EventTrigger trigger)
            {
                // Wait until some time has expired to show the description or until the setting name text has been exited.
                float t = 0f;
                bool exitedText = false;
                EventTrigger.Entry pointerExit = new EventTrigger.Entry();
                pointerExit.eventID = EventTriggerType.PointerExit;
                pointerExit.callback.AddListener((eventData) =>
                {
                    exitedText = true;
                });
                trigger.triggers.Add(pointerExit);
                while (t < 0.5f)
                {
                    t += Time.deltaTime;
                    if (exitedText)
                    {
                        t = 0.5f;
                    }
                    yield return null;
                }
                // Show the description once the waiting time has expired if the setting name text has not been exited.
                if (!exitedText)
                {
                    descriptionGameObject.SetActive(true);

                    settingsButtonGameObject.SetActive(false);
                }
                trigger.triggers.Remove(pointerExit);
            }
        }

        public class MenuScrollbar : GameObjectFollowingTransform
        {
            public Scrollbar scrollbar;
            public Image fillImage;
            public Image handleImage;
            public MenuScrollbar(string name, Transform parentTransform, Vector3 referenceOffset, Vector2 rectSize) : base(name, parentTransform, referenceOffset)
            {
                MenuImage menuFillImage = new MenuImage(name, gameObject.transform, Vector3.zero);
                fillImage = menuFillImage.image;
                fillImage.sprite = referenceSliderBackgroundImage.sprite;
                fillImage.type = referenceSliderBackgroundImage.type;
                fillImage.material = referenceSliderBackgroundImage.material;

                MenuImage handleMenuImage = new MenuImage(name, menuFillImage.gameObject.transform, Vector3.zero);
                handleImage = handleMenuImage.image;
                handleImage.sprite = referenceSliderHandleImage.sprite;
                handleImage.type = referenceSliderHandleImage.type;
                handleImage.material = referenceSliderHandleImage.material;
                handleImage.color = referenceSliderHandleImage.color;
                handleImage.rectTransform.sizeDelta = referenceBoolButtonImage.rectTransform.sizeDelta / 2f;

                RectTransform scrollbarRect = gameObject.AddComponent<RectTransform>();
                scrollbarRect.sizeDelta = rectSize;

                HorizontalLayoutGroup horizontalLayoutGroup = gameObject.AddComponent<HorizontalLayoutGroup>();
                horizontalLayoutGroup.padding = new RectOffset(5, 5, 5, 5);
                horizontalLayoutGroup.childAlignment = TextAnchor.MiddleRight;
                horizontalLayoutGroup.childForceExpandWidth = false;
                horizontalLayoutGroup.childForceExpandHeight = true;

                scrollbar = gameObject.AddComponent<Scrollbar>();
                scrollbar.colors = referenceSlider.colors;
                scrollbar.direction = Scrollbar.Direction.BottomToTop;
                scrollbar.navigation = referenceSlider.navigation;
                scrollbar.spriteState = referenceSlider.spriteState;
                scrollbar.transition = referenceSlider.transition;
                scrollbar.image = handleImage;
                scrollbar.handleRect = handleImage.rectTransform;
            }
        }

        public class MenuSlider : MenuImage
        {
            public Slider slider;
            public Image handleImage;
            public MenuSlider(string name, Transform parentTransform, Vector3 referenceOffset, bool useInt, float minClamp, float maxClamp) : base(name, parentTransform, referenceOffset)
            {
                // Unity Slider in 4 Minutes - [Unity Tutorial] - Tarodev - https://youtu.be/nTLgzvklgU8 - Accessed 03.01.2022
                slider = gameObject.AddComponent<Slider>();
                slider.colors = referenceSlider.colors;
                slider.direction = referenceSlider.direction;
                slider.navigation = referenceSlider.navigation;
                slider.spriteState = referenceSlider.spriteState;
                slider.transition = referenceSlider.transition;
                slider.wholeNumbers = useInt; // Might be good to add extra update to round float numbers to 1/2 DP.
                slider.minValue = minClamp;
                slider.maxValue = maxClamp;

                image.sprite = referenceSliderBackgroundImage.sprite;
                image.type = referenceSliderBackgroundImage.type;
                image.material = referenceSliderBackgroundImage.material;

                MenuImage handleMenuImage = new MenuImage(name, gameObject.transform, Vector3.zero);
                handleImage = handleMenuImage.image;
                handleImage.sprite = referenceSliderHandleImage.sprite;
                handleImage.type = referenceSliderHandleImage.type;
                handleImage.material = referenceSliderHandleImage.material;
                handleImage.color = referenceSliderHandleImage.color;
                handleImage.rectTransform.sizeDelta = referenceBoolButtonImage.rectTransform.sizeDelta;

                //Debug.Log("-----\nReference rects:\nBackground = " + referenceSliderBackgroundImage.rectTransform.rect + "\nFill = " + referenceSliderFillImage.rectTransform.rect + "\nHandle = " + referenceSliderHandleImage.rectTransform.rect + "\n-----\nReference sizeDeltas:\nBackground = " + referenceSliderBackgroundImage.rectTransform.sizeDelta + "\nFill = " + referenceSliderFillImage.rectTransform.sizeDelta + "\nHandle = " + referenceSliderHandleImage.rectTransform.sizeDelta);
                //Debug.Log("-----\nRects before assignment to slider:\nBackground = " + sliderBackgroundImage.rectTransform.rect + "\nFill = " + fillImage.rectTransform.rect + "\n Handle = " + handleImage.rectTransform.rect + "\n-----\nSizeDeltas before assignment to slider:\nBackground = " + sliderBackgroundImage.rectTransform.sizeDelta + "\nFill = " + fillImage.rectTransform.sizeDelta + "\nHandle = " + handleImage.rectTransform.sizeDelta);

                float handleHeightBefore = handleImage.rectTransform.rect.height;

                // The slider makes the fill and handle rects not be centred / zeroed anymore and increases them 100 in height / -50 in sizeDelta.
                slider.image = handleImage;//sliderBackgroundImage;
                slider.handleRect = handleImage.rectTransform;

                //Debug.Log("-----\nRects after assignment to slider:\nBackground = " + sliderBackgroundImage.rectTransform.rect + "\nFill = " + fillImage.rectTransform.rect + "\n Handle = " + handleImage.rectTransform.rect + "\n-----\nSizeDeltas after assignment to slider:\nBackground = " + sliderBackgroundImage.rectTransform.sizeDelta + "\nFill = " + fillImage.rectTransform.sizeDelta + "\nHandle = " + handleImage.rectTransform.sizeDelta);

                float handleHeightDifference = handleImage.rectTransform.rect.height - handleHeightBefore;

                image.rectTransform.sizeDelta = new Vector2(0.945f * image.rectTransform.sizeDelta.x, image.rectTransform.sizeDelta.y / 2f);//slider.image.rectTransform.sizeDelta = new Vector2(slider.image.rectTransform.sizeDelta.x, slider.image.rectTransform.sizeDelta.y / 2f);
                slider.handleRect.sizeDelta = new Vector2(slider.handleRect.sizeDelta.x, slider.handleRect.sizeDelta.y - handleHeightDifference / 2f);

                // Scale the slider and adjust for slider scaling
                slider.transform.localScale = new Vector3(0.5f, 1f, 1f); // how to change the scale property of rect transform unity - Obnoxious Oystercatcher - https://www.codegrepper.com/code-examples/csharp/how+to+change+the+scale+property+of+rect+transform+unity - Accessed 03.01.2022
                handleImage.transform.localScale = new Vector3(2f, 1f, 1f);

                //Debug.Log("-----\nRects after correction:\nBackground = " + sliderBackgroundImage.rectTransform.rect + "\nFill = " + fillImage.rectTransform.rect + "\n Handle = " + handleImage.rectTransform.rect + "\n-----\nSizeDeltas after correction:\nBackground = " + sliderBackgroundImage.rectTransform.sizeDelta + "\nFill = " + fillImage.rectTransform.sizeDelta + "\nHandle = " + handleImage.rectTransform.sizeDelta);

            }
        }

        public class MenuBoolButton : MenuTextImageButton
        {
            private bool value;
            public MenuBoolButton(string name, Transform parentTransform, Vector3 referenceOffset, bool smallText = true) : base(name, parentTransform, referenceOffset, smallText, true)
            {
                image.rectTransform.sizeDelta = referenceBoolButtonImage.rectTransform.sizeDelta;

                text.text = "X";
                text.font = referenceBoolButtonText.font;
                text.color = referenceBoolButtonText.color;
                text.fontSize = referenceBoolButtonText.fontSize;
                text.fontStyle = referenceBoolButtonText.fontStyle;
                text.rectTransform.sizeDelta = referenceBoolButtonImage.rectTransform.sizeDelta;
                text.alignment = referenceBoolButtonText.alignment;

                // Event to switch the button.
                button.onClick.AddListener(delegate ()
                {
                    SetValueAndUpdateText(!value);
                });
            }

            public bool GetValue()
            {
                return this.value;
            }

            public void SetValueAndUpdateText(bool value)
            {
                this.value = value;
                text.text = value ? "X" : string.Empty;
            }
        }

        public class MenuInputField : MenuTextImage
        {
            public InputField inputField;
            public EventTrigger eventTrigger;
            public MenuInputField(string name, Transform parentTransform, Vector3 referenceOffset, bool useInt, float minClamp, float maxClamp, bool smallText = true) : base(name, parentTransform, referenceOffset, smallText, true)
            {
                image.rectTransform.sizeDelta = new Vector2((image.rectTransform.sizeDelta.x / 2.25f) / sizeReduction, image.rectTransform.sizeDelta.y);

                inputField = gameObject.AddComponent<InputField>();

                text.rectTransform.sizeDelta = image.rectTransform.sizeDelta;

                inputField.targetGraphic = image;
                inputField.textComponent = text;
                inputField.text = "This is an input field";

                eventTrigger = gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry pointerClickEvent = new EventTrigger.Entry();
                pointerClickEvent.eventID = EventTriggerType.PointerClick;
                pointerClickEvent.callback.AddListener((eventData) =>
                {
                    inputField.ActivateInputField();
                    inputField.StartCoroutine(InputFieldInputValidation(inputField, useInt, minClamp, maxClamp));
                });
                eventTrigger.triggers.Add(pointerClickEvent);

                EventTrigger.Entry pointerEnterEvent = new EventTrigger.Entry();
                pointerEnterEvent.eventID = EventTriggerType.PointerEnter;
                pointerEnterEvent.callback.AddListener((eventData) => { image.color = new Color(245f / 255f, 210f / 255f, 140f / 255f, referenceOptionImage.color.a); });
                eventTrigger.triggers.Add(pointerEnterEvent);

                EventTrigger.Entry pointerExitEvent = new EventTrigger.Entry();
                pointerExitEvent.eventID = EventTriggerType.PointerExit;
                pointerExitEvent.callback.AddListener((eventData) => { image.color = referenceOptionImage.color; });
                eventTrigger.triggers.Add(pointerExitEvent);
            }

            private static IEnumerator InputFieldInputValidation(InputField inputField, bool useInt, float minClamp, float maxClamp)
            {
                yield return null;
                while (inputField.isFocused)
                {
                    yield return null;
                }
                // This is where you should verify inputs
                if (useInt)
                {
                    float floatValue = Convert.ToSingle(inputField.text);
                    float roundedInt = Mathf.RoundToInt(floatValue);
                    inputField.text = roundedInt.ToString();
                }
                if (minClamp != 0f || maxClamp != 0f)
                {
                    float fieldValue = Convert.ToSingle(inputField.text);
                    if (fieldValue > maxClamp)
                    {
                        inputField.text = maxClamp.ToString();
                    }
                    else if (fieldValue < minClamp)
                    {
                        inputField.text = minClamp.ToString();
                    }
                }
                yield break;
            }
        }

        public class MenuTextImageButton : MenuTextImage
        {
            public Button button;
            public MenuTextImageButton(string name, Transform parentTransform, Vector3 referenceOffset, bool smallText = true, bool addColourEventToImage = false) : base(name, parentTransform, referenceOffset, smallText, addColourEventToImage)
            {
                button = this.gameObject.AddComponent<Button>();
            }
        }

        public class MenuTextImage : MenuImage
        {
            public Text text;
            public MenuTextImage(string name, Transform parentTransform, Vector3 referenceOffset, bool smallText = true, bool addColourEventToImage = false) : base(name, parentTransform, referenceOffset, addColourEventToImage)
            {
                MenuText menuText = new MenuText(name, this.gameObject.transform, Vector3.zero, smallText);
                text = menuText.text;
            }
        }

        public class MenuImageButton : MenuImage
        {
            public Button button;
            public MenuImageButton(string name, Transform parentTransform, Vector3 referenceOffset, bool addColourEvent = false) : base(name, parentTransform, referenceOffset, addColourEvent)
            {
                button = this.gameObject.AddComponent<Button>();

                if (addColourEvent)
                {
                    button.onClick.AddListener(delegate ()
                    {
                        image.color = referenceOptionImage.color;
                    });
                }
            }
        }

        public class MenuList : MenuImage
        {
            protected GameObjectFollowingRectTransform contentGameObject; // GameObject to hold any items displayed in the list.
            public MenuList(string name, Transform parentTransform, Vector3 referenceOffset, Vector2 rectSize, string[] headers) : base(name, parentTransform, referenceOffset)
            {
                Mask mask = gameObject.AddComponent<Mask>(); // Mask to hide entries outside of the image.

                // Adjust the area and opacity of the image.
                image.rectTransform.sizeDelta = rectSize;
                image.color = new Color(image.color.r, image.color.g, image.color.b, 0.5f);

                contentGameObject = new GameObjectFollowingRectTransform(name, gameObject.transform, Vector3.zero);
                //contentRect.sizeDelta = new Vector2(0.975f * image.rectTransform.sizeDelta.x, 0.975f * image.rectTransform.sizeDelta.y); // Might not be necessary. y is controlled automatically.
                contentGameObject.rectTransform.pivot = new Vector2(0.5f, 1f); // Make rect centre at the top centre of the list.
                VerticalLayoutGroup verticalLayoutGroup = contentGameObject.gameObject.AddComponent<VerticalLayoutGroup>();
                //verticalLayoutGroup.padding = new RectOffset(10, 10, 10, 10);
                verticalLayoutGroup.spacing = 5;
                verticalLayoutGroup.childAlignment = TextAnchor.UpperCenter; // Might not be necessary due to contentSizeFitter.
                verticalLayoutGroup.childControlWidth = false;
                verticalLayoutGroup.childControlHeight = false;
                verticalLayoutGroup.childForceExpandWidth = false;
                verticalLayoutGroup.childForceExpandHeight = false;

                ContentSizeFitter contentSizeFitter = contentGameObject.gameObject.AddComponent<ContentSizeFitter>(); // Changes the content rect height to fit all the list items.
                contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

                // Vertical scrollbar on the right side of the list.
                MenuScrollbar menuScrollbar = new MenuScrollbar(name, parentTransform, referenceOffset + new Vector3(20f, 0f, 0f), image.rectTransform.sizeDelta);

                ScrollRect scrollRect = gameObject.AddComponent<ScrollRect>();
                scrollRect.horizontal = false;
                scrollRect.vertical = true;
                scrollRect.verticalScrollbar = menuScrollbar.scrollbar;
                scrollRect.content = contentGameObject.rectTransform;
                scrollRect.scrollSensitivity = 15;

                GameObjectFollowingRectTransform headersParent = new GameObjectFollowingRectTransform("HeadersParent", parentTransform, referenceOffset + new Vector3(0f, image.rectTransform.sizeDelta.y / 2f + 15f, 0f));
                headersParent.rectTransform.sizeDelta = new Vector2(image.rectTransform.sizeDelta.x, 1.5f * headersParent.rectTransform.sizeDelta.y);

                HorizontalLayoutGroup horizontalLayoutGroup = headersParent.gameObject.AddComponent<HorizontalLayoutGroup>();
                horizontalLayoutGroup.childControlWidth = false;

                foreach (string header in headers)
                {
                    MenuTextButton headerMenuTextButton = new MenuTextButton(header, headersParent.gameObject.transform, Vector3.zero);
                    headerMenuTextButton.text.fontSize = mediumFontSize;
                    headerMenuTextButton.text.rectTransform.sizeDelta = new Vector2(headersParent.rectTransform.sizeDelta.x / headers.Length, headersParent.rectTransform.sizeDelta.y);
                }
            }
        }

        public class MenuImage : CanvasRenderable
        {
            public Image image;
            public MenuImage(string name, Transform parentTransform, Vector3 referenceOffset, bool addColourEvent = false) : base(name, parentTransform, referenceOffset)
            {
                image = this.gameObject.AddComponent<Image>();
                image.rectTransform.sizeDelta = referenceOptionImage.rectTransform.sizeDelta;
                image.sprite = referenceOptionImage.sprite;
                image.type = referenceOptionImage.type;
                image.material = referenceOptionImage.material;
                image.color = referenceOptionImage.color;

                if (addColourEvent)
                {
                    EventTrigger trigger = this.gameObject.AddComponent<EventTrigger>();
                    EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
                    pointerEnter.eventID = EventTriggerType.PointerEnter;
                    pointerEnter.callback.AddListener((eventData) => { image.color = new Color(245f / 255f, 210f / 255f, 140f / 255f, referenceOptionImage.color.a); });
                    trigger.triggers.Add(pointerEnter);

                    EventTrigger.Entry pointerExit = new EventTrigger.Entry();
                    pointerExit.eventID = EventTriggerType.PointerExit;
                    pointerExit.callback.AddListener((eventData) => { image.color = referenceOptionImage.color; });
                    trigger.triggers.Add(pointerExit);
                }
            }
        }


        private class MenuTextButton : MenuText
        {
            public Button button;
            public MenuTextButton(string name, Transform parentTransform, Transform referenceTransform, Vector3 referenceOffset, bool smallText = true) : this(name, referenceTransform, referenceOffset, smallText)
            {
                this.gameObject.transform.SetParent(parentTransform, true);
            }

            public MenuTextButton(string name, Transform parentTransform, Vector3 referenceOffset, bool smallText = true) : base(name, parentTransform, referenceOffset, smallText)
            {
                button = this.gameObject.AddComponent<Button>();
                text.raycastTarget = true;

                EventTrigger trigger = text.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry textHighlight = new EventTrigger.Entry();
                textHighlight.eventID = EventTriggerType.PointerEnter;
                textHighlight.callback.AddListener((eventData) => { text.color = Color.black; });
                trigger.triggers.Add(textHighlight);

                EventTrigger.Entry textHighlightReset = new EventTrigger.Entry();
                textHighlightReset.eventID = EventTriggerType.PointerExit;
                textHighlightReset.callback.AddListener((eventData) => { text.color = referenceCategoryText.color; });
                trigger.triggers.Add(textHighlightReset);

                // Event to reset text colour when clicking on a button to fix highlights persisting when going in and out of a sub menu.
                button.onClick.AddListener(delegate ()
                {
                    text.color = referenceCategoryText.color;
                });

                //Debug.Log("-----\nCreated " + canvasGameObjects[0].name + ":\nPosition: " + canvasGameObjects[0].transform.position + " / " + canvasGameObjects[0].transform.localPosition + "\nRotation: " + canvasGameObjects[0].transform.rotation + " / " + canvasGameObjects[0].transform.localRotation + "\n...and " + canvasGameObjects[1].name + ":\nPosition: " + canvasGameObjects[1].transform.position + " / " + canvasGameObjects[1].transform.localPosition + "\nRotation: " + canvasGameObjects[1].transform.rotation + " / " + canvasGameObjects[1].transform.localRotation + "\n-----");
            }
        }

        public class MenuText : CanvasRenderable
        {
            public Text text;
            public MenuText(string name, Transform parentTransform, Vector3 referenceOffset, bool smallText = true) : base(name, parentTransform, referenceOffset)
            {
                text = this.gameObject.AddComponent<Text>();
                text.text = name.Split(new string[] { "_" }, System.StringSplitOptions.None)[0];
                text.font = referenceCategoryText.font;
                text.color = referenceCategoryText.color;
                text.fontSize = referenceCategoryText.fontSize;
                text.fontStyle = referenceCategoryText.fontStyle;
                text.raycastTarget = false;
                if (smallText)
                {
                    text.rectTransform.sizeDelta = referenceOptionImage.rectTransform.sizeDelta;

                    text.fontSize = referenceOptionText.fontSize;
                    text.color = referenceOptionText.color;
                    text.fontStyle = referenceOptionText.fontStyle;

                    // Overlay text blocking buttons under it. - JAKJ - https://forum.unity.com/threads/overlay-text-blocking-buttons-under-it.265680/ - Accessed 24.10.2021 [Used for old code]
                    // Can just use text.raycastTarget = false instead
                    //Debug.Log("Small text rect is " + referenceOptionText.rectTransform.sizeDelta + " and big text rect is " + referenceCategoryText.rectTransform.sizeDelta);
                    //Debug.Log("Small image rect is " + referenceOptionImage.rectTransform.sizeDelta + " and big image rect is " + referenceCategoryImage.rectTransform.sizeDelta);
                }
                else
                {
                    text.rectTransform.sizeDelta = new Vector2(1.25f * referenceCategoryImage.rectTransform.sizeDelta.x, referenceCategoryImage.rectTransform.sizeDelta.y);
                }
                text.alignment = referenceCategoryText.alignment;
            }
        }

        public abstract class CanvasRenderable : GameObjectFollowingTransform
        {
            protected CanvasRenderable(string name, Transform parentTransform, Vector3 referenceOffset) : base(name + "-CanvasRendererGameObject", parentTransform, referenceOffset)
            {
                this.gameObject.AddComponent<CanvasRenderer>();
            }
        }

        public class CanvasFollowingTransform : GameObjectFollowingTransform
        {
            public Canvas canvas;
            public CanvasFollowingTransform(string name, Transform parentTransform, Vector3 referenceOffset) : base(name + "-CanvasGameObject", parentTransform, referenceOffset)
            {
                // Create Unity UI Panel via Script - prof - https://answers.unity.com/questions/1034060/create-unity-ui-panel-via-script.html - Accessed 23.10.2021
                canvas = this.gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.WorldSpace;
                canvas.sortingOrder = 0;
                this.gameObject.AddComponent<CanvasScaler>();
                this.gameObject.AddComponent<GraphicRaycaster>();
            }
        }

        public class GameObjectFollowingRectTransform : GameObjectFollowingTransform
        {
            public RectTransform rectTransform;
            public GameObjectFollowingRectTransform(string name, Transform parentTransform, Vector3 referenceOffset) : base(name, parentTransform, referenceOffset)
            {
                rectTransform = gameObject.AddComponent<RectTransform>();
                rectTransform.sizeDelta = referenceOptionImage.rectTransform.sizeDelta;
            }
        }

        public class GameObjectFollowingTransform
        {
            public GameObject gameObject;

            public GameObjectFollowingTransform(string name, Transform parentTransform, Vector3 referenceOffset)
            {
                this.gameObject = new GameObject(name.Replace(' ', '_'));
                this.gameObject.transform.SetParent(parentTransform, false);
                this.gameObject.transform.localPosition += referenceOffset;
            }
        }

        public class ChallengeSettingsList : MenuList
        {
            private static readonly string[] headers = new string[] { "Setting", "Value" };
            public ChallengeSettingsList(string name, Transform parentTransform, Vector3 referenceOffset, List<MESMSettingCompact> settings) : base(name, parentTransform, referenceOffset, new Vector2(3.5f * referenceOptionImage.rectTransform.sizeDelta.x, 7.5f * referenceOptionImage.rectTransform.sizeDelta.y), headers)
            {
                foreach (MESMSettingCompact setting in settings)
                {
                    MenuImage background = new MenuImageButton(setting.name, contentGameObject.gameObject.transform, Vector3.zero);
                    background.image.rectTransform.sizeDelta = new Vector2(image.rectTransform.sizeDelta.x, 2f * background.image.rectTransform.sizeDelta.y);
                    HorizontalLayoutGroup horizontalLayoutGroup = background.gameObject.AddComponent<HorizontalLayoutGroup>();
                    horizontalLayoutGroup.childControlWidth = false;

                    MenuText settingName = new MenuText(setting.name, background.gameObject.transform, Vector3.zero);
                    MenuText settingValue = new MenuText(setting.value, background.gameObject.transform, Vector3.zero);
                    settingName.text.rectTransform.sizeDelta = new Vector2(background.image.rectTransform.sizeDelta.x / 2, settingName.text.rectTransform.sizeDelta.y);
                    settingValue.text.rectTransform.sizeDelta = settingName.text.rectTransform.sizeDelta;
                }
            }
        }

        public class ChallengesList : MenuList
        {
            private static readonly string[] headers = new string[] { "Name", "Author", "Difficulty", "Completed" };
            public static List<Challenge> challenges;
            public ChallengesList(string name, Transform parentTransform, Vector3 referenceOffset) : base(name, parentTransform, referenceOffset, new Vector2(5f * referenceOptionImage.rectTransform.sizeDelta.x, 15f * referenceOptionImage.rectTransform.sizeDelta.y), headers)
            {
                challenges = ChallengeParser.ReadAllChallenges();
                foreach (Challenge challenge in challenges)
                {
                    MenuImageButton background = new MenuImageButton(challenge.name, contentGameObject.gameObject.transform, Vector3.zero, true);
                    background.image.rectTransform.sizeDelta = new Vector2(image.rectTransform.sizeDelta.x, 2f * background.image.rectTransform.sizeDelta.y);
                    HorizontalLayoutGroup horizontalLayoutGroup = background.gameObject.AddComponent<HorizontalLayoutGroup>();
                    horizontalLayoutGroup.childControlWidth = false;

                    MenuText menuTextImageName = new MenuText(challenge.name, background.gameObject.transform, Vector3.zero);
                    MenuText menuTextImageAuthor = new MenuText(challenge.author, background.gameObject.transform, Vector3.zero);
                    MenuText menuTextImageDifficulty = new MenuText(challenge.difficulty, background.gameObject.transform, Vector3.zero);
                    MenuText menuTextImageCompleted = new MenuText(challenge.completionTime == TimeSpan.MaxValue ? "Uncompleted" : challenge.CompletionTimeString(), background.gameObject.transform, Vector3.zero);
                    menuTextImageName.text.rectTransform.sizeDelta = new Vector2(background.image.rectTransform.sizeDelta.x / 4, menuTextImageName.text.rectTransform.sizeDelta.y);
                    menuTextImageAuthor.text.rectTransform.sizeDelta = menuTextImageName.text.rectTransform.sizeDelta;
                    menuTextImageDifficulty.text.rectTransform.sizeDelta = menuTextImageName.text.rectTransform.sizeDelta;
                    menuTextImageCompleted.text.rectTransform.sizeDelta = menuTextImageName.text.rectTransform.sizeDelta;

                    // Create a submenu for the challenge.
                    ChallengeSubPage challengeSubPage = new ChallengeSubPage(challenge, parentTransform.gameObject, Vector3.zero, background.button);
                }
            }
        }

        public class MESMSettingRGB : MESMSetting<int>
        {
            public Text displayText;
            public InputField inputField;
            public MESMSettingRGBColourEnum colour;

            public enum MESMSettingRGBColourEnum
            {
                red,
                green,
                blue
            }

            public MESMSettingRGB(string modSettingsText, string modSettingsDescription, int defaultValue, bool absoluteValue = false, bool childSetting = false, float lowerClamp = float.MinValue, float upperClamp = float.MaxValue) : base(modSettingsText, modSettingsDescription, defaultValue, absoluteValue, childSetting, lowerClamp, upperClamp)
            {
                if (modSettingsText.Contains("Red Component"))
                {
                    colour = MESMSettingRGBColourEnum.red;
                }
                else if (modSettingsText.Contains("Green Component"))
                {
                    colour = MESMSettingRGBColourEnum.green;
                }
                else if (modSettingsText.Contains("Blue Component"))
                {
                    colour = MESMSettingRGBColourEnum.blue;
                }
                else
                {
                    Debug.Log("Could not create MESMSettingRGB because of incorrectly formatted modSettingsText!");
                }
            }

            // On stop using slider [or input field for now], update colour of display text. Change component corresponding to which of rgb this slider is.
            private IEnumerator ChangeDisplayTextColourAfterInput()
            {
                yield return null;
                while (inputField.isFocused)
                {
                    yield return null;
                }
                // This is where you should change the display text's colour
                if (inputField.text.Equals("-1"))
                {
                    displayText.color = referenceOptionText.color;
                }
                else
                {
                    ChangeTextColour();
                }
                yield break;
            }

            public void ChangeTextColour()
            {
                switch (colour)
                {
                    case (MESMSettingRGBColourEnum.red):
                        {
                            // Change the red component of the display text
                            displayText.color = new Color(Convert.ToSingle(inputField.text) / 255f, displayText.color.g, displayText.color.b, displayText.color.a);
                            break;
                        }
                    case (MESMSettingRGBColourEnum.green):
                        {
                            // Change the green component of the display text
                            displayText.color = new Color(displayText.color.r, Convert.ToSingle(inputField.text) / 255f, displayText.color.b, displayText.color.a);
                            break;
                        }
                    case (MESMSettingRGBColourEnum.blue):
                        {
                            // Change the blue component of the display text
                            displayText.color = new Color(displayText.color.r, displayText.color.g, Convert.ToSingle(inputField.text) / 255f, displayText.color.a);
                            break;
                        }
                }
            }

            public MenuDescriptionBox CreateRGBButton(Transform referenceTransform, Vector3 referenceOffset, Text displayText)
            {
                this.displayText = displayText;
                base.CreateButtonForSetting(referenceTransform, referenceOffset); // Creates and assigns settingsButton

                // Edit settingsButton
                MenuSliderInputFieldWithDescription menuSliderInputFieldWithDescription = ((MenuSliderInputFieldWithDescription)settingsButton);
                inputField = menuSliderInputFieldWithDescription.menuInputField.inputField;

                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;

                entry.callback.AddListener((eventData) =>
                {
                    inputField.StartCoroutine(ChangeDisplayTextColourAfterInput());
                });
                menuSliderInputFieldWithDescription.menuInputField.eventTrigger.triggers.Add(entry);

                menuSliderInputFieldWithDescription.menuSlider.slider.onValueChanged.AddListener((sliderValue) => { ChangeTextColour(); });

                ChangeTextColour();

                return settingsButton;
            }
        }

        /*----------------------------------------------------------------------------------------------------*/
    }
}
// ~End Of File