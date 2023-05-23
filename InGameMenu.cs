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

        private class ChallengesMenu : SubMenu
        {
            public ChallengesMenu(GameObject parentPage, Vector3 entryButtonOffset) : base("Challenges", parentPage, Vector3.zero, parentPage.transform, entryButtonOffset)
            {
                float yOffset = 375f;
                MenuTextButton challengeNameGOs = new MenuTextButton("Name", exitButtonGO.transform, new Vector3(-225f, yOffset, 0f));
                MenuTextButton authorGOs = new MenuTextButton("Author", exitButtonGO.transform, new Vector3(-75f, yOffset, 0f));
                MenuTextButton difficultyGOs = new MenuTextButton("Difficulty", exitButtonGO.transform, new Vector3(75f, yOffset, 0f));
                MenuTextButton completedGOs = new MenuTextButton("Completed", exitButtonGO.transform, new Vector3(225f, yOffset, 0f));

                int mediumFontSize = (2 * referenceOptionText.fontSize);
                challengeNameGOs.text.fontSize = mediumFontSize;
                challengeNameGOs.text.rectTransform.sizeDelta *= 1.4f;
                authorGOs.text.fontSize = mediumFontSize;
                authorGOs.text.rectTransform.sizeDelta *= 1.4f;
                difficultyGOs.text.fontSize = mediumFontSize;
                difficultyGOs.text.rectTransform.sizeDelta *= 1.4f;
                completedGOs.text.fontSize = mediumFontSize;
                completedGOs.text.rectTransform.sizeDelta *= 1.4f;
            }
        }

        private class NavigationSubMenu : SubMenu
        {
            public NavigationSubMenu(string name, GameObject parentPage, Vector3 parentPageOffset, Transform entryButtonParentTransform, Vector3 entryButtonOffset) : base(name, parentPage, parentPageOffset, entryButtonParentTransform, entryButtonOffset)
            {
                // Create a save button.
                MenuTextButton saveButton = new MenuTextButton("Save All Settings", exitButtonGO.transform, new Vector3(250f, 435f, 0f), true);

                saveButton.text.rectTransform.sizeDelta = largeButtonSizeDelta;
                saveButton.text.fontSize = smallButtonFontSize;

                saveButton.button.onClick.AddListener(delegate ()
                {
                    MESMSetting.SaveSettings();
                });

                // Create a reset to default button
                MenuTextButton resetButton = new MenuTextButton("Reset To Default (Press Save Afterwards)", exitButtonGO.transform, new Vector3(-250f, 435f, 0f), true);
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

        private class SettingsSubMenuPage : GameObjectFollowingTransform
        {
            public MenuTextButton nextButton;
            public MenuTextButton previousButton;

            public SettingsSubMenuPage(string name, Transform parentTransform, Transform exitButtonTransform, int index) : base(name, parentTransform, Vector3.zero)
            {
                nextButton = new MenuTextButton("→_" + name + "_NextPage_" + index, exitButtonTransform, new Vector3(150f, 20f, 0f), false);
                nextButton.gameObject.transform.SetParent(gameObject.transform, true);
                nextButton.text.rectTransform.sizeDelta = new Vector2(nextButton.text.rectTransform.sizeDelta.x / 1.5f, nextButton.text.rectTransform.sizeDelta.y);

                previousButton = new MenuTextButton("←_" + name + "_LastPage_" + index, exitButtonTransform, new Vector3(-150f, 20f, 0f), false);
                previousButton.gameObject.transform.SetParent(gameObject.transform, true);
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

        private class SettingsSubMenu : SubMenu
        {
            List<SettingsSubMenuPage> settingsPages;
            public SettingsSubMenu(string name, GameObject parentPage, Vector3 entryButtonOffset, List<MESMSetting> settings) : this(name, parentPage, Vector3.zero, parentPage.transform, entryButtonOffset, settings)
            {
            }

            public SettingsSubMenu(string name, GameObject parentPage, Vector3 parentPageOffset, Transform entryButtonParentTransform, Vector3 entryButtonOffset, List<MESMSetting> settings) : base(name, parentPage, parentPageOffset, entryButtonParentTransform, entryButtonOffset)
            {
                // Create a save button.
                MenuTextButton saveButton = new MenuTextButton("Save", exitButtonGO.transform, new Vector3(0f, 50f, 0f), false);
                saveButton.text.rectTransform.sizeDelta = smallButtonSizeDelta;
                saveButton.button.onClick.AddListener(delegate ()
                {
                    MESMSetting.SaveSettings();
                });

                // Create a reset to default button
                MenuTextButton resetButton = new MenuTextButton("Reset To Default (Press Save Afterwards)", exitButtonGO.transform, new Vector3(-250f, 435f, 0f), true);
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

                    k++;
                    if (k % maximumOptionsPerRow == 0)
                    {
                        offset += new Vector3(400f / sizeReduction, maximumOptionsPerRow * verticalShift, 0f);
                        if (k == 2 * maximumOptionsPerRow && i != settings.Count - 1)
                        {
                            settingsPages.Add(new SettingsSubMenuPage(name, this.gameObject.transform, exitButtonGO.transform, i));
                            offset = new Vector3(-175f, 0f, 0f) / sizeReduction; //Vector3.zero;//new Vector3(-150f, 0f, 0f);
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
                    //Debug.Log("Starting settings page next and last arrow assignment");
                    //Debug.Log("Number of settings groups is " + settingsGroups.Count);
                    //Debug.Log("Number of navigation buttons is " + navigationButtons.Count);

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
            protected SubMenu(string name, GameObject parentPage, Vector3 parentPageOffset, Transform entryButtonParentTransform, Vector3 entryButtonOffset) : base(name, parentPage.transform, parentPageOffset)
            {
                this.gameObject.transform.SetParent(clipboardTransform, true);

                MenuTextButton entryButton = new MenuTextButton(name, entryButtonParentTransform, entryButtonOffset, false);
                MenuTextButton exitButton = new MenuTextButton("Exit", this.gameObject.transform, new Vector3(0f, -375f, 0f), false);
                exitButtonGO = exitButton.gameObject;
                entryButton.button.onClick.AddListener(delegate ()
                {
                    SwitchToPage(this.gameObject, parentPage);
                });
                exitButton.button.onClick.AddListener(delegate ()
                {
                    SwitchToPage(parentPage, this.gameObject);
                });
                entryButton.button.onClick.AddListener(delegate ()
                {
                    menuSounds.ButtonClickGoForward();
                });
                exitButton.button.onClick.AddListener(delegate ()
                {
                    menuSounds.ButtonClickGoBack();
                });

                Text exitButtonText = exitButtonGO.GetComponentInChildren<Text>();

                exitButtonText.rectTransform.sizeDelta = largeButtonSizeDelta;

                string shortTitle = name.Split(new string[] { " Settings" }, System.StringSplitOptions.None)[0];
                MenuText text = new MenuText(shortTitle, exitButtonGO.transform, new Vector3(0f, (shortTitle.Length > 10 ? 450f : 435f), 0f), false); // Give long titles two lines rather than one.
                text.text.rectTransform.sizeDelta = new Vector2(/*1.25f * */text.text.rectTransform.sizeDelta.x, 2f * text.text.rectTransform.sizeDelta.y);

                this.gameObject.SetActive(false);
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
            // Create the base GameObjects to serve as the warning box and adjust their sorting order to appear on top of the menu buttons.
            warningBox = new MenuTextImageButton("WarningBox", /*gameObjectReferencePos.transform*/optionsUI.optionsButtons.transform/*buttonList[3].transform*//*.parent.parent.parent*/, new Vector3(0f, 0/*200f*/, -7.5f), false, clipboardTransform);
            //warningBoxGOs.GetComponent<Canvas>().sortingOrder = 5;
            //warningBoxGOs.GetComponent<Canvas>().sortingOrder = 4;

            // Adjust the area and opacity of the image.
            warningBox.image.rectTransform.sizeDelta = new Vector2(5f * warningBox.image.rectTransform.sizeDelta.x, 15f * warningBox.image.rectTransform.sizeDelta.y); // Big variant
            warningBox.image.color = new Color(warningBox.image.color.r, warningBox.image.color.g, warningBox.image.color.b, 1f);

            // Change the area and size of the text.
            warningBox.text.rectTransform.sizeDelta = new Vector2(0.975f * warningBox.text.rectTransform.sizeDelta.x, 0.975f * warningBox.text.rectTransform.sizeDelta.y);
            warningBox.text.fontSize = (int)(warningBox.text.fontSize / 2.5f); // Big variant

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

        private static void SwitchToPage(GameObject newPage, GameObject originalPage)
        {
            originalPage.SetActive(false);
            newPage.SetActive(true);
        }

        private static void SwitchSettings(List<GameObject> settingsToActivate, List<GameObject> settingsToDeactivate, GameObject[] buttonsToActivate, GameObject[] buttonsToDeactivate)
        {
            foreach (GameObject gameObject in settingsToDeactivate)
            {
                if (!gameObject.name.Contains("DESCRIPTION_BOX") && !gameObject.name.Contains("DROPDOWN_BOX") && !gameObject.name.Contains("DROPDOWN_CHOICE"))
                {
                    gameObject.SetActive(false);
                }
            }
            foreach (GameObject gameObject in buttonsToDeactivate)
            {
                gameObject.SetActive(false);
            }
            foreach (GameObject gameObject in settingsToActivate)
            {
                if (!gameObject.name.Contains("DESCRIPTION_BOX") && !gameObject.name.Contains("DROPDOWN_BOX") && !gameObject.name.Contains("DROPDOWN_CHOICE"))
                {
                    gameObject.SetActive(true);
                }
            }
            foreach (GameObject gameObject in buttonsToActivate)
            {
                gameObject.SetActive(true);
            }
        }

        public class MenuMultipleChoiceButtonWithDescription : MenuDescriptionBox
        {
            MenuTextImage leftImage;
            public MenuMultipleChoiceButtonWithDescription(string description, string name, string[] choices, Transform parentTransform, Vector3 referenceOffset, bool smallText = true) : base(description, name, parentTransform, referenceOffset, smallText)
            {
                float xShift = Mathf.Abs((dropDownOptionImage.rectTransform.sizeDelta.x - ((referenceOptionImage.rectTransform.sizeDelta.x / 2.25f) / sizeReduction)) / 2);
                Vector3 xShiftVector = new Vector3(xShift, 0f, 0f);
                //this.gameObject.transform.localPosition += new Vector3(xShift, 0f, 0f);

                //Debug.Log("dropDownOptionImage.rectTransform.sizeDelta.x = " + dropDownOptionImage.rectTransform.sizeDelta.x + ", referenceOptionImage.rectTransform.sizeDelta.x = " + referenceOptionImage.rectTransform.sizeDelta.x + ", ((referenceOptionImage.rectTransform.sizeDelta.x / 2.25f) / sizeReduction) = " + ((referenceOptionImage.rectTransform.sizeDelta.x / 2.25f) / sizeReduction) + ",(dropDownOptionImage.rectTransform.sizeDelta.x - ((referenceOptionImage.rectTransform.sizeDelta.x / 2.25f) / sizeReduction)) / 2 = " + (dropDownOptionImage.rectTransform.sizeDelta.x - ((referenceOptionImage.rectTransform.sizeDelta.x / 2.25f) / sizeReduction)) / 2 + ", Total = " + xShift);
                float arrowShift = dropDownOptionImage.rectTransform.sizeDelta.x / 2 + 10f / sizeReduction;/*62.5f / sizeReduction;*/
                Vector3 arrowShiftVector = new Vector3(arrowShift, 0f, 0f);
                leftImage = new MenuTextImage(name, nameTextCustomTransform, nameOffsetConstant * nameTextCustomTransform.localPosition + xShiftVector, smallText);
                MenuImageButton rightImageBoxBaseGOs = new MenuImageButton(name, leftImage.gameObject.transform, arrowShiftVector, true);
                MenuImage rightImageArrow = new MenuImage(name, rightImageBoxBaseGOs.gameObject.transform, Vector3.zero);

                leftImage.image.rectTransform.sizeDelta = dropDownOptionImage.rectTransform.sizeDelta;
                rightImageBoxBaseGOs.image.rectTransform.sizeDelta = dropDownButtonBoxImage.rectTransform.sizeDelta;

                rightImageArrow.image.rectTransform.sizeDelta = dropDownArrowImage.rectTransform.sizeDelta;
                rightImageArrow.image.sprite = dropDownArrowImage.sprite;
                rightImageArrow.image.color = dropDownArrowImage.color;
                rightImageArrow.image.raycastTarget = false;

                // Create and resize a dropdown box and then put dropdown choices on top of it.
                List<GameObject> dropdownGOs = new List<GameObject>();
                MenuImage backgroundImage = new MenuImage(name + "DROPDOWN_BOX", leftImage.gameObject.transform, Vector3.zero);
                backgroundImage.image.rectTransform.sizeDelta = new Vector2(leftImage.image.rectTransform.sizeDelta.x, choices.Length * leftImage.image.rectTransform.sizeDelta.y);
                backgroundImage.image.color = new Color(backgroundImage.image.color.r, backgroundImage.image.color.g, backgroundImage.image.color.b, 1f);

                for (int i = 0; i < choices.Length; i++)
                {
                    MenuTextImageButton choiceGOs = new MenuTextImageButton(choices[i] + "_DROPDOWN_CHOICE", leftImage.gameObject.transform, new Vector3(0f, (i + 1) * -leftImage.image.rectTransform.sizeDelta.y, 0f), smallText, true);
                    choiceGOs.image.rectTransform.sizeDelta = leftImage.image.rectTransform.sizeDelta;
                    dropdownGOs.Add(choiceGOs.gameObject);

                    int storableInt = i;
                    choiceGOs.button.onClick.AddListener(delegate ()
                    {
                        Debug.Log("Updating left image (before): " + leftImage.text.text);
                        leftImage.text.text = choices[storableInt];
                        Debug.Log("Updating left image (after): " + leftImage.text.text);
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
                rightImageBoxBaseGOs.button.onClick.AddListener(delegate ()
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

            public override Text ValueText()
            {
                Debug.Log("Left Image Text value: " + leftImage.text.text);
                return leftImage.text;
            }
        }

        public class MenuBoolButtonWithDescription : MenuDescriptionBox
        {
            MenuBoolButton menuBoolButton;
            public MenuBoolButtonWithDescription(string description, string name, Transform parentTransform, Vector3 referenceOffset, bool smallText = true) : base(description, name, parentTransform, referenceOffset, smallText)
            {
                menuBoolButton = new MenuBoolButton(name, nameTextCustomTransform, nameOffsetConstant * nameTextCustomTransform.localPosition, smallText);
            }

            public override Text ValueText()
            {
                return menuBoolButton.text;
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

            public override Text ValueText()
            {
                return menuInputField.inputField.textComponent;
            }
        }

        // To fix hover don't make parents children of nameMenuText.
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

                //descriptionTextGameObjects[0].GetComponent<Canvas>().sortingOrder = 3;
                //descriptionTextGameObjects[2].GetComponent<Canvas>().sortingOrder = 2;

                /*
                // CSF Experiment
                Image descriptionImage = descriptionTextGameObjects[3].GetComponentInChildren<Image>();
                descriptionImage.rectTransform.sizeDelta = new Vector2(2.75f * descriptionImage.rectTransform.sizeDelta.x, 5f * descriptionImage.rectTransform.sizeDelta.y);
                descriptionImage.color = new Color(descriptionImage.color.r, descriptionImage.color.g, descriptionImage.color.b, 1f);
                Text text = descriptionTextGameObjects[1].GetComponentInChildren<Text>();
                text.rectTransform.sizeDelta = descriptionImage.rectTransform.sizeDelta;
                text.text = description;
                ContentSizeFitter csf = descriptionTextGameObjects[1].AddComponent<ContentSizeFitter>();
                csf.verticalFit = ContentSizeFitter.FitMode.MinSize;
                descriptionImage.rectTransform.sizeDelta = new Vector2(descriptionImage.rectTransform.sizeDelta.x, text.rectTransform.sizeDelta.y);
                text.rectTransform.sizeDelta = new Vector2(0.975f * descriptionImage.rectTransform.sizeDelta.x, 0.975f * descriptionImage.rectTransform.sizeDelta.y);
                */

                EventTrigger trigger = nameMenuText.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerEnter;
                entry.callback.AddListener((eventData) =>
                {
                    AudioSystem.instance.StartCoroutine(RevealDescriptionAfterTime(descriptionMenuTextImage.gameObject, nameMenuText.gameObject, trigger));
                });
                trigger.triggers.Add(entry);

                EventTrigger trigger2 = descriptionMenuTextImage.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry entry2 = new EventTrigger.Entry();
                entry2.eventID = EventTriggerType.PointerExit;
                entry2.callback.AddListener((eventData) =>
                {
                    descriptionMenuTextImage.gameObject.SetActive(false);

                    nameMenuText.gameObject.SetActive(true);
                });
                trigger2.triggers.Add(entry2);

                descriptionMenuTextImage.gameObject.SetActive(false);
            }

            public abstract Text ValueText();

            private static IEnumerator RevealDescriptionAfterTime(GameObject descriptionGameObject, GameObject settingsButtonGameObject, EventTrigger trigger)
            {
                float t = 0f;
                bool exitedText = false;
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerExit;
                entry.callback.AddListener((eventData) =>
                {
                    exitedText = true;
                });
                trigger.triggers.Add(entry);
                while (t < 0.5f)
                {
                    t += Time.deltaTime;
                    if (exitedText)
                    {
                        t = 0.5f;
                    }
                    yield return null;
                }
                if (!exitedText)
                {
                    descriptionGameObject.SetActive(true);

                    settingsButtonGameObject.SetActive(false);
                }
                trigger.triggers.Remove(entry);
            }
        }

        public class MenuSlider : MenuImage
        {
            public Slider slider;
            public Image fillImage;
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
                //slider.wholeNumbers = referenceSlider.wholeNumbers;
                slider.wholeNumbers = useInt;
                slider.minValue = minClamp;
                slider.maxValue = maxClamp;

                image.sprite = referenceSliderBackgroundImage.sprite;
                image.type = referenceSliderBackgroundImage.type;
                image.material = referenceSliderBackgroundImage.material;
                //sliderBackgroundImage.color = referenceSliderBackgroundImage.color;

                MenuImage fillMenuImage = new MenuImage(name, gameObject.transform, Vector3.zero);
                fillImage = fillMenuImage.image;
                fillImage.sprite = referenceSliderFillImage.sprite;
                fillImage.type = referenceSliderFillImage.type;
                fillImage.material = referenceSliderFillImage.material;
                fillImage.color = referenceSliderFillImage.color;
                fillImage.raycastTarget = false;

                MenuImage handleMenuImage = new MenuImage(name, gameObject.transform, Vector3.zero);
                handleImage = handleMenuImage.image;
                handleImage.sprite = referenceSliderHandleImage.sprite;
                handleImage.type = referenceSliderHandleImage.type;
                handleImage.material = referenceSliderHandleImage.material;
                handleImage.color = referenceSliderHandleImage.color;
                handleImage.raycastTarget = false;
                handleImage.rectTransform.sizeDelta = referenceBoolButtonImage.rectTransform.sizeDelta;

                //Debug.Log("-----\nReference rects:\nBackground = " + referenceSliderBackgroundImage.rectTransform.rect + "\nFill = " + referenceSliderFillImage.rectTransform.rect + "\nHandle = " + referenceSliderHandleImage.rectTransform.rect + "\n-----\nReference sizeDeltas:\nBackground = " + referenceSliderBackgroundImage.rectTransform.sizeDelta + "\nFill = " + referenceSliderFillImage.rectTransform.sizeDelta + "\nHandle = " + referenceSliderHandleImage.rectTransform.sizeDelta);
                //Debug.Log("-----\nRects before assignment to slider:\nBackground = " + sliderBackgroundImage.rectTransform.rect + "\nFill = " + fillImage.rectTransform.rect + "\n Handle = " + handleImage.rectTransform.rect + "\n-----\nSizeDeltas before assignment to slider:\nBackground = " + sliderBackgroundImage.rectTransform.sizeDelta + "\nFill = " + fillImage.rectTransform.sizeDelta + "\nHandle = " + handleImage.rectTransform.sizeDelta);

                float fillHeightBefore = fillImage.rectTransform.rect.height;
                float handleHeightBefore = handleImage.rectTransform.rect.height;

                // The slider makes the fill and handle rects not be centred / zeroed anymore and increases them 100 in height / -50 in sizeDelta.
                slider.image = handleImage;//sliderBackgroundImage;
                slider.fillRect = fillImage.rectTransform;
                slider.handleRect = handleImage.rectTransform;

                //Debug.Log("-----\nRects after assignment to slider:\nBackground = " + sliderBackgroundImage.rectTransform.rect + "\nFill = " + fillImage.rectTransform.rect + "\n Handle = " + handleImage.rectTransform.rect + "\n-----\nSizeDeltas after assignment to slider:\nBackground = " + sliderBackgroundImage.rectTransform.sizeDelta + "\nFill = " + fillImage.rectTransform.sizeDelta + "\nHandle = " + handleImage.rectTransform.sizeDelta);

                float fillHeightDifference = fillImage.rectTransform.rect.height - fillHeightBefore;
                float handleHeightDifference = handleImage.rectTransform.rect.height - handleHeightBefore;

                image.rectTransform.sizeDelta = new Vector2(0.945f * image.rectTransform.sizeDelta.x, image.rectTransform.sizeDelta.y / 2f);//slider.image.rectTransform.sizeDelta = new Vector2(slider.image.rectTransform.sizeDelta.x, slider.image.rectTransform.sizeDelta.y / 2f);
                slider.fillRect.sizeDelta = new Vector2(/*2f * */referenceSliderFillImage.rectTransform.sizeDelta.x, slider.fillRect.sizeDelta.y - fillHeightDifference - fillHeightBefore / 2f);
                slider.handleRect.sizeDelta = new Vector2(slider.handleRect.sizeDelta.x, slider.handleRect.sizeDelta.y - handleHeightDifference / 2f);

                // Scale the slider and adjust for slider scaling
                slider.transform.localScale = new Vector3(0.5f, 1f, 1f); // how to change the scale property of rect transform unity - Obnoxious Oystercatcher - https://www.codegrepper.com/code-examples/csharp/how+to+change+the+scale+property+of+rect+transform+unity - Accessed 03.01.2022
                handleImage.transform.localScale = new Vector3(2f, 1f, 1f);

                //Debug.Log("-----\nRects after correction:\nBackground = " + sliderBackgroundImage.rectTransform.rect + "\nFill = " + fillImage.rectTransform.rect + "\n Handle = " + handleImage.rectTransform.rect + "\n-----\nSizeDeltas after correction:\nBackground = " + sliderBackgroundImage.rectTransform.sizeDelta + "\nFill = " + fillImage.rectTransform.sizeDelta + "\nHandle = " + handleImage.rectTransform.sizeDelta);

            }
        }


        public class MenuBoolButton : MenuTextImageButton
        {
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

                button.onClick.AddListener(delegate ()
                    {
                        if (text.text.Equals("X"))
                        {
                            text.text = string.Empty;
                        }
                        else
                        {
                            text.text = "X";
                        }
                    });
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
                    EventTrigger.Entry entry = new EventTrigger.Entry();
                    entry.eventID = EventTriggerType.PointerEnter;
                    entry.callback.AddListener((eventData) => { image.color = new Color(245f / 255f, 210f / 255f, 140f / 255f, referenceOptionImage.color.a); });
                    trigger.triggers.Add(entry);

                    EventTrigger.Entry entry2 = new EventTrigger.Entry();
                    entry2.eventID = EventTriggerType.PointerExit;
                    entry2.callback.AddListener((eventData) => { image.color = referenceOptionImage.color; });
                    trigger.triggers.Add(entry2);
                }
            }
        }


        private class MenuTextButton : MenuText
        {
            public Button button;
            public MenuTextButton(string name, Transform parentTransform, Vector3 referenceOffset, bool smallText = true) : base(name, parentTransform, referenceOffset, smallText)
            {
                button = this.gameObject.AddComponent<Button>();
                text.raycastTarget = true;

                EventTrigger trigger = text.gameObject.AddComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerEnter;
                entry.callback.AddListener((eventData) => { text.color = Color.black; });
                trigger.triggers.Add(entry);

                EventTrigger.Entry entry2 = new EventTrigger.Entry();
                entry2.eventID = EventTriggerType.PointerExit;
                entry2.callback.AddListener((eventData) => { text.color = referenceCategoryText.color; });
                trigger.triggers.Add(entry2);

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
                    text.rectTransform.sizeDelta = referenceCategoryImage.rectTransform.sizeDelta + new Vector2(0.25f * referenceCategoryImage.rectTransform.sizeDelta.x, 0);
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
                canvas.renderMode = RenderMode.WorldSpace;//RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 0;
                this.gameObject.AddComponent<CanvasScaler>();
                this.gameObject.AddComponent<GraphicRaycaster>();
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

        public static GameObject GameObjectArrayToSingleParent(string name, GameObject[] array)
        {
            // Only combine GameObjects if there are at least two.
            if (array != null && array.Length > 1)
            {
                GameObject parent = new GameObject(name);
                parent.transform.SetParent(array[0].transform.parent);
                foreach (GameObject child in array)
                {
                    child.transform.SetParent(parent.transform);
                }
                return parent;
            }
            return null;
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

            public GameObject CreateRGBButton(Transform referenceTransform, Vector3 referenceOffset, Text displayText)
            {
                this.displayText = displayText;
                base.CreateButtonForSetting(referenceTransform, referenceOffset); // Creates and assigns settingsButton

                // Edit settingsButton
                inputField = settingsButton.GetComponentInChildren<InputField>();

                EventTrigger trigger = settingsButton.GetComponentInChildren<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;

                entry.callback.AddListener((eventData) =>
                {
                    inputField.StartCoroutine(ChangeDisplayTextColourAfterInput());
                });
                trigger.triggers.Add(entry);

                Slider slider = settingsButton.GetComponentInChildren<Slider>();
                slider.onValueChanged.AddListener((sliderValue) => { ChangeTextColour(); });

                ChangeTextColour();

                return settingsButton;
            }
        }

        /*----------------------------------------------------------------------------------------------------*/
    }
}
// ~End Of File