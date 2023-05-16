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

        public static GameObject[] mesmButtonGOs;
        public static GameObject[] warningBoxGOs;
        public static Text warningBoxText;

        private static float sizeReduction;

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
            MenuSounds menuSounds = FindObjectOfType<MenuSounds>();
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

            // Create the categories menu and button to go into it on the original options page.
            mesmButtonGOs = CreateSubMenu("MES Mod", optionsUI.optionsButtons, buttonList[3].transform, (buttonList[3].transform.position - buttonList[3].transform.parent.position) - 3f * (buttonList[3].transform.position - buttonList[1].transform.position), menuSounds, null, false, true);

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
                GameObject[] subMenu = CreateSubMenu(categories[i], mesmButtonGOs[0], mesmButtonGOs[1].transform, new Vector3(0f, 52.5f - (52.5f * i), 0f), menuSounds, settingsAssociatedToCategory[i], false, false);
            }

            GameObject[] challengesMenu = CreateSubMenu("Challenges", mesmButtonGOs[0], mesmButtonGOs[1].transform, new Vector3(0f, 52.5f - (52.5f * categories.Count), 0f), menuSounds, null, false, false, true);

            // Create a warning box to alert the user of restart conditions and errors.
            // Create the base GameObjects to serve as the warning box and adjust their sorting order to appear on top of the menu buttons.
            warningBoxGOs = CreateTextAndImageButton("WarningBox", /*gameObjectReferencePos.transform*/optionsUI.optionsButtons.transform/*buttonList[3].transform*//*.parent.parent.parent*/, new Vector3(0f, 0/*200f*/, -7.5f), false, false, clipboardTransform, false);
            warningBoxGOs[0].GetComponent<Canvas>().sortingOrder = 5;
            warningBoxGOs[2].GetComponent<Canvas>().sortingOrder = 4;

            // Adjust the area and opacity of the image.
            Image warningBoxImage = warningBoxGOs[3].GetComponent<Image>();
            warningBoxImage.rectTransform.sizeDelta = new Vector2(5f * warningBoxImage.rectTransform.sizeDelta.x, 15f * warningBoxImage.rectTransform.sizeDelta.y); // Big variant
            warningBoxImage.color = new Color(warningBoxImage.color.r, warningBoxImage.color.g, warningBoxImage.color.b, 1f);

            // Change the area and size of the text.
            warningBoxText = warningBoxGOs[1].GetComponent<Text>();
            warningBoxText.rectTransform.sizeDelta = new Vector2(0.975f * warningBoxImage.rectTransform.sizeDelta.x, 0.975f * warningBoxImage.rectTransform.sizeDelta.y);
            warningBoxText.fontSize = (int)(warningBoxText.fontSize / 2.5f); // Big variant

            // Add functionality to the box.
            Button warningBoxButton = warningBoxGOs[3].GetComponent<Button>();
            warningBoxButton.onClick.AddListener(delegate ()
            {
                warningBoxGOs[0].SetActive(false);
                warningBoxGOs[2].SetActive(false);
            });
            warningBoxButton.onClick.AddListener(delegate ()
            {
                menuSounds.ButtonClickGoBack();
            });

            // Set it inactive so that it is not show when first in the menu.
            warningBoxGOs[0].SetActive(false);
            warningBoxGOs[2].SetActive(false);
        }

        private static void SwitchToPage(GameObject newPage, GameObject originalPage)
        {
            originalPage.SetActive(false);
            newPage.SetActive(true);
        }

        // CreateSubMenu
        // Creates a settings sub menu.
        // name: The name of the menu to display in the parent menu.
        // originalPage: The original menu page to display a button to the sub menu on.
        // buttonTransform: A reference transform of another button to position the new button.
        // referenceOffset: The offset to use from the buttonTransform.
        // menuSounds: A MenuSounds instance to play sounds.
        // settings: A list of settings to allow customisation of on the page.
        // smallOption: Whether to show certain text in a small font.
        // topLevel: Whether the menu is a parent menu or a sub menu (whether it is at the top level or not).
        // useResetButton: Whether to use a reset button or not.
        private static GameObject[] CreateSubMenu(string name, GameObject originalPage, Transform buttonTransform, Vector3 referenceOffset, MenuSounds menuSounds, List<MESMSetting> settings = null, bool smallOption = false, bool topLevel = false, bool blankPage = false)
        {
            GameObject[] entryButtonGOs = CreateTextButton(name, buttonTransform, referenceOffset, smallOption, topLevel);
            Text entryButtonText = entryButtonGOs[1].GetComponentInChildren<Text>();
            entryButtonText.rectTransform.sizeDelta = new Vector2(entryButtonText.rectTransform.sizeDelta.x * 1.75f, entryButtonText.rectTransform.sizeDelta.y);

            GameObject subMenuPage = new GameObject(name.Replace(' ', '_') + "-CanvasGameObject");
            Canvas canvas = subMenuPage.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;//RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            subMenuPage.AddComponent<CanvasScaler>();
            subMenuPage.AddComponent<GraphicRaycaster>();

            GameObject subMenuCanvasRendererGameObject = new GameObject(name.Replace(' ', '_') + "-CanvasRendererGameObject");
            subMenuCanvasRendererGameObject.AddComponent<CanvasRenderer>();
            subMenuCanvasRendererGameObject.transform.SetParent(subMenuPage.transform, false);

            subMenuPage.transform.SetParent(buttonTransform, false);
            subMenuPage.transform.SetParent(clipboardTransform, true);

            subMenuCanvasRendererGameObject.transform.localPosition += new Vector3(0f, topLevel ? 0f : 60f, -5f); // Move text in front of the clipboard to avoid clipping of text on the sides of the clipboard

            //Debug.Log(name + " page is at " + subMenuPage.transform.position + " / " + subMenuPage.transform.localPosition + " with rotation " + subMenuPage.transform.rotation + " / " + subMenuPage.transform.localRotation);
            //Debug.Log(name + " page Canvas Renderer is at " + subMenuCanvasRendererGameObject.transform.position + " / " + subMenuCanvasRendererGameObject.transform.localPosition + " with rotation " + subMenuCanvasRendererGameObject.transform.rotation + " / " + subMenuCanvasRendererGameObject.transform.localRotation);

            Button entryButton = entryButtonGOs[1].GetComponentInChildren<Button>();
            GameObject[] exitButtonGOs = CreateTextButton("Exit", subMenuCanvasRendererGameObject.transform/*referenceImageSmallTextAndImageButton.transform*/, new Vector3(0f, topLevel ? -315f : -375f/*topLevel ? -375f : -500f*//*topLevel ? -525f : -700f*/, 0f), false, false);
            Button exitButton = exitButtonGOs[1].GetComponentInChildren<Button>();
            Text exitButtonText = exitButtonGOs[1].GetComponent<Text>();

            // Create reference sizes.
            Vector2 largeReferenceSizeDelta = referenceCategoryImage.rectTransform.sizeDelta + new Vector2(0.25f * referenceCategoryImage.rectTransform.sizeDelta.x, 0);
            Vector2 largeButtonSizeDelta = new Vector2(1.2f * largeReferenceSizeDelta.x, 3f * largeReferenceSizeDelta.y);
            int largeButtonFontSize = (int)(1.25f * referenceCategoryText.fontSize);
            Vector2 smallButtonSizeDelta = new Vector2(1.2f * referenceOptionImage.rectTransform.sizeDelta.x, 3f * referenceOptionImage.rectTransform.sizeDelta.y);
            int smallButtonFontSize = (int)(1.25f * referenceOptionText.fontSize);

            exitButtonText.rectTransform.sizeDelta = largeButtonSizeDelta;

            entryButton.onClick.AddListener(delegate ()
            {
                SwitchToPage(subMenuPage, originalPage);
            });
            exitButton.onClick.AddListener(delegate ()
            {
                SwitchToPage(originalPage, subMenuPage);
            });
            entryButton.onClick.AddListener(delegate ()
            {
                menuSounds.ButtonClickGoForward();
            });
            exitButton.onClick.AddListener(delegate ()
            {
                menuSounds.ButtonClickGoBack();
            });

            subMenuPage.SetActive(false);

            string shortTitle = name.Split(new string[] { " Settings" }, System.StringSplitOptions.None)[0];
            GameObject[] categoryTitleGOs = CreateText(shortTitle, exitButtonGOs[0].transform, new Vector3(0f, (shortTitle.Length > 10 ? 450f : 435f), 0f), false, false); // Give long titles two lines rather than one.
            Text text = categoryTitleGOs[1].GetComponent<Text>();
            text.rectTransform.sizeDelta = new Vector2(/*1.25f * */text.rectTransform.sizeDelta.x, 2f * text.rectTransform.sizeDelta.y);


            if (!blankPage)
            {
                // Create a save button.
                GameObject[] saveButtonGOs = CreateTextButton((topLevel ? "Save All Settings" : "Save"), exitButtonGOs[0].transform, (topLevel ? new Vector3(250f, 435f, 0f) : new Vector3(0f, 50f, 0f)), topLevel, false);
                Button saveButton = saveButtonGOs[1].GetComponentInChildren<Button>();
                Text saveButtonText = saveButtonGOs[1].GetComponent<Text>();

                if (!topLevel)
                {
                    saveButtonText.rectTransform.sizeDelta = smallButtonSizeDelta;
                }
                else
                {
                    saveButtonText.rectTransform.sizeDelta = largeButtonSizeDelta;
                    saveButtonText.fontSize = smallButtonFontSize;
                }
                saveButton.onClick.AddListener(delegate ()
                {
                    MESMSetting.SaveSettings();
                });

                // Create a reset to default button
                GameObject[] resetButtonGOs = CreateTextButton("Reset To Default (Press Save Afterwards)", exitButtonGOs[0].transform, new Vector3(-250f, 435f, 0f), true, false);
                Button resetButton = resetButtonGOs[1].GetComponentInChildren<Button>();
                Text resetButtonText = resetButtonGOs[1].GetComponent<Text>();
                resetButtonText.rectTransform.sizeDelta = smallButtonSizeDelta;
                resetButtonText.fontSize = smallButtonFontSize;

                if (!topLevel)
                {
                    resetButton.onClick.AddListener(delegate ()
                    {
                        MESMSetting.ResetSettingsToDefault(settings);
                    });
                }
                else
                {
                    resetButton.onClick.AddListener(delegate ()
                    {
                        MESMSetting.ResetSettingsToDefault(ModSettings.allSettings);
                    });
                }

                if (topLevel)
                {
                    GameObject[] versionTextGOs = CreateText("V" + VERSION_WITH_TEXT + "\nPrecipitator", exitButtonGOs[0].transform, new Vector3(-250f, 0f, 0f), false, false);
                    Text versionText = versionTextGOs[1].GetComponent<Text>();
                    versionText.rectTransform.sizeDelta = smallButtonSizeDelta;
                    versionText.fontSize /= 2;
                }

                if (!topLevel)
                {
                    // Create guide text in the top right on settings pages.
                    GameObject[] guideTextGOs = CreateText("Hover over settings to view descriptions", exitButtonGOs[0].transform, new Vector3(250f, 435f, 0f), true, false);
                    Text guideText = guideTextGOs[1].GetComponent<Text>();
                    guideText.rectTransform.sizeDelta = smallButtonSizeDelta;
                    guideText.fontSize = smallButtonFontSize;

                    // Set up variables to correctly place settings on pages.
                    int k = 0;
                    Vector3 offset = new Vector3(-175f, 0f, 0f) / sizeReduction; //Vector3.zero;//new Vector3(-150f, 0f, 0f);
                    int verticalShift = (int)(45 / sizeReduction);
                    int maximumOptionsPerRow = 335 / verticalShift;
                    Transform settingsReferenceTransform = subMenuCanvasRendererGameObject.transform;

                    // Create pages to hold settings.
                    List<List<GameObject[]>> settingsPages = new List<List<GameObject[]>>();
                    settingsPages.Add(new List<GameObject[]>());
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
                                    GameObject[] displayTextGOs = CreateText(settings[i].modSettingsText.Replace(" Red Component", ""), settingsReferenceTransform, new Vector3(0, -(verticalShift * k), 0f) + offset);
                                    Text displayText = displayTextGOs[1].GetComponent<Text>();
                                    settingsPages[settingsPages.Count - 1].Add(displayTextGOs);
                                    k++;
                                    settingsPages[settingsPages.Count - 1].Add(((MESMSettingRGB)settings[i]).CreateRGBButton(settingsReferenceTransform, new Vector3(0, -(verticalShift * k), 0f) + offset, displayText));
                                }
                            }
                            else
                            {
                                // Create another component button by using the last component's reference to the display text.
                                settingsPages[settingsPages.Count - 1].Add(((MESMSettingRGB)settings[i]).CreateRGBButton(settingsReferenceTransform, new Vector3(0, -(verticalShift * k), 0f) + offset, ((MESMSettingRGB)settings[i - 1]).displayText));
                                /*
                                // If the last component is being assigned, set the display text colour based on the settings.
                                if (((MESMSettingRGB)settings[i]).colour == MESMSettingRGB.MESMSettingRGBColourEnum.blue)
                                {
                                    if (((MESMSettingRGB)settings[i]).userValue == -1 || ((MESMSettingRGB)settings[i - 1]).userValue == -1 || ((MESMSettingRGB)settings[i - 2]).userValue == -1)
                                    {
                                        ((MESMSettingRGB)settings[i]).displayText.color = referenceOptionText.color;
                                    }
                                    else
                                    {
                                        ((MESMSettingRGB)settings[i]).displayText.color = new Color(Convert.ToSingle(((MESMSettingRGB)settings[i - 2]).inputField.text) / 255f, Convert.ToSingle(((MESMSettingRGB)settings[i - 1]).inputField.text) / 255f, Convert.ToSingle(((MESMSettingRGB)settings[i]).inputField.text) / 255f);
                                    }
                                }
                                */
                            }
                        }
                        else
                        {
                            // Create a button for a setting on a certain page.
                            settingsPages[settingsPages.Count - 1].Add(settings[i].CreateButtonForSetting(settingsReferenceTransform, new Vector3(0, -(verticalShift * k), 0f) + offset));
                        }

                        k++;
                        if (k % maximumOptionsPerRow == 0)
                        {
                            offset += new Vector3(400f / sizeReduction, maximumOptionsPerRow * verticalShift, 0f);
                            if (k == 2 * maximumOptionsPerRow && i != settings.Count - 1)
                            {
                                settingsPages.Add(new List<GameObject[]>());
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

                            foreach (GameObject[] gameObjects in settingsPages[i])
                            {
                                foreach (GameObject gameObject in gameObjects)
                                {
                                    if (!gameObject.name.Contains("DESCRIPTION_BOX") && !gameObject.name.Contains("DROPDOWN_BOX") && !gameObject.name.Contains("DROPDOWN_CHOICE"))
                                    {
                                        gameObject.SetActive(false);
                                    }
                                }
                            }
                            /*
                            foreach (GameObject[] gameObjects in settingsPages[i])
                            {
                                gameObjects[0].SetActive(false);
                                if (gameObjects.Length > 3)
                                {
                                    gameObjects[2].SetActive(false);
                                }
                                if (gameObjects.Length > 5)
                                {
                                    gameObjects[4].SetActive(false);
                                }
                                if (gameObjects.Length > 11)
                                {
                                    gameObjects[12].SetActive(false); // Sliders have this many gameobjects.
                                }
                            }
                            */
                        }

                        List<GameObject[]> navigationButtons = new List<GameObject[]>();
                        for (int i = 0; i < settingsPages.Count; i++)
                        {
                            navigationButtons.Add(CreateTextButton("→_" + name + "_NextPage_" + (i + 1), exitButtonGOs[0].transform, new Vector3(150f, 20f, 0f), topLevel));
                            navigationButtons.Add(CreateTextButton("←_" + name + "_LastPage_" + (i + 1), exitButtonGOs[0].transform, new Vector3(-150f, 20f, 0f), topLevel));
                            Text navigationButtonsText1 = navigationButtons[2 * i][1].GetComponent<Text>();
                            navigationButtonsText1.rectTransform.sizeDelta = new Vector2(navigationButtonsText1.rectTransform.sizeDelta.x / 1.5f, navigationButtonsText1.rectTransform.sizeDelta.y);
                            Text navigationButtonsText2 = navigationButtons[2 * i + 1][1].GetComponent<Text>();
                            navigationButtonsText2.rectTransform.sizeDelta = new Vector2(navigationButtonsText2.rectTransform.sizeDelta.x / 1.5f, navigationButtonsText2.rectTransform.sizeDelta.y);
                            if (i > 0)
                            {
                                navigationButtons[2 * i][1].SetActive(false);
                                navigationButtons[2 * i + 1][1].SetActive(false);
                            }
                        }

                        //Debug.Log("Starting settings page next and last arrow assignment");
                        //Debug.Log("Number of settings groups is " + settingsGroups.Count);
                        //Debug.Log("Number of navigation buttons is " + navigationButtons.Count);

                        for (int i = 0; i < settingsPages.Count; i++)
                        {
                            int storedIndex = i; // Passing a temporary variable to add listener - Mmmpies - https://answers.unity.com/questions/908847/passing-a-temporary-variable-to-add-listener.html - Accessed 15.12.2021
                                                 //Debug.Log("Page " + storedIndex);
                            GameObject nextButtonGO = navigationButtons[2 * storedIndex][1];
                            GameObject lastButtonGO = navigationButtons[2 * storedIndex + 1][1];

                            Button nextButton = nextButtonGO.GetComponentInChildren<Button>();
                            Button lastButton = lastButtonGO.GetComponentInChildren<Button>();

                            //Debug.Log("Current page buttons found");

                            GameObject nextButtonFromNextPageGO;
                            GameObject lastButtonFromNextPageGO;
                            GameObject nextButtonFromLastPageGO;
                            GameObject lastButtonFromLastPageGO;
                            if (storedIndex + 1 == settingsPages.Count)
                            {
                                //Debug.Log("Using alt next button");
                                nextButtonFromNextPageGO = navigationButtons[0][1];
                                lastButtonFromNextPageGO = navigationButtons[1][1];
                                //Debug.Log("Used alt next button");
                            }
                            else
                            {
                                nextButtonFromNextPageGO = navigationButtons[2 * (storedIndex + 1)][1];
                                lastButtonFromNextPageGO = navigationButtons[2 * (storedIndex + 1) + 1][1];
                            }
                            //Debug.Log("Next page buttons found");
                            if (storedIndex - 1 == -1)
                            {
                                //Debug.Log("Using alt last button");
                                nextButtonFromLastPageGO = navigationButtons[navigationButtons.Count - 2][1];
                                lastButtonFromLastPageGO = navigationButtons[navigationButtons.Count - 1][1];
                                //Debug.Log("Used alt last button");
                            }
                            else
                            {
                                nextButtonFromLastPageGO = navigationButtons[2 * (storedIndex - 1)][1];
                                lastButtonFromLastPageGO = navigationButtons[2 * (storedIndex - 1) + 1][1];
                            }
                            //Debug.Log("Last page buttons found");


                            if (storedIndex + 1 == settingsPages.Count)
                            {
                                nextButton.onClick.AddListener(delegate ()
                                {
                                    //Debug.Log("Using indices 0 and " + storedIndex + " for " + settingsGroups.Count + " (Next alt)");
                                    SwitchSettings(settingsPages[0], settingsPages[storedIndex], new GameObject[] { nextButtonFromNextPageGO, lastButtonFromNextPageGO }, new GameObject[] { nextButtonGO, lastButtonGO });
                                });
                            }
                            else
                            {
                                //Debug.Log("Using indices " + (storedIndex + 1) + " and " + storedIndex + " for " + settingsGroups.Count + " (Next normal)");
                                nextButton.onClick.AddListener(delegate ()
                                {
                                    SwitchSettings(settingsPages[storedIndex + 1], settingsPages[storedIndex], new GameObject[] { nextButtonFromNextPageGO, lastButtonFromNextPageGO }, new GameObject[] { nextButtonGO, lastButtonGO });
                                });
                            }
                            //Debug.Log("Next page buttons assigned");

                            if (storedIndex - 1 == -1)
                            {
                                //Debug.Log("Using indices " + (settingsGroups.Count - 1) + " and " + storedIndex + " for " + settingsGroups.Count + " (Last alt)");
                                lastButton.onClick.AddListener(delegate ()
                                {
                                    SwitchSettings(settingsPages[settingsPages.Count - 1], settingsPages[storedIndex], new GameObject[] { nextButtonFromLastPageGO, lastButtonFromLastPageGO }, new GameObject[] { nextButtonGO, lastButtonGO });
                                });
                            }
                            else
                            {
                                //Debug.Log("Using indices " + (storedIndex - 1) + " and " + storedIndex + " for " + settingsGroups.Count + " (Last normal)");
                                lastButton.onClick.AddListener(delegate ()
                                {
                                    SwitchSettings(settingsPages[storedIndex - 1], settingsPages[storedIndex], new GameObject[] { nextButtonFromLastPageGO, lastButtonFromLastPageGO }, new GameObject[] { nextButtonGO, lastButtonGO });
                                });
                            }
                            //Debug.Log("Last page buttons assigned");

                            nextButton.onClick.AddListener(delegate ()
                            {
                                menuSounds.ButtonClickGoForward();
                            });
                            lastButton.onClick.AddListener(delegate ()
                            {
                                menuSounds.ButtonClickGoBack();
                            });
                        }
                        //Debug.Log("Finished settings page next and last arrow assignment");
                    }
                }
            }

            return new GameObject[] { subMenuPage, subMenuCanvasRendererGameObject, entryButtonGOs[0], entryButtonGOs[1], exitButtonGOs[0], exitButtonGOs[1] };
        }

        private static void SwitchSettings(List<GameObject[]> settingsToActivate, List<GameObject[]> settingsToDeactivate, GameObject[] buttonsToActivate, GameObject[] buttonsToDeactivate)
        {
            foreach (GameObject[] gameObjects in settingsToDeactivate)
            {
                foreach (GameObject gameObject in gameObjects)
                {
                    if (!gameObject.name.Contains("DESCRIPTION_BOX") && !gameObject.name.Contains("DROPDOWN_BOX") && !gameObject.name.Contains("DROPDOWN_CHOICE"))
                    {
                        gameObject.SetActive(false);
                    }
                }
            }
            foreach (GameObject gameObject in buttonsToDeactivate)
            {
                gameObject.SetActive(false);
            }
            foreach (GameObject[] gameObjects in settingsToActivate)
            {
                foreach (GameObject gameObject in gameObjects)
                {
                    if (!gameObject.name.Contains("DESCRIPTION_BOX") && !gameObject.name.Contains("DROPDOWN_BOX") && !gameObject.name.Contains("DROPDOWN_CHOICE"))
                    {
                        gameObject.SetActive(true);
                    }
                }
            }
            foreach (GameObject gameObject in buttonsToActivate)
            {
                gameObject.SetActive(true);
            }
        }

        /*
        Code for first working canvas.
        private static GameObject[] CreateSpecialPanel(List<Button> buttonList, UnityEngine.Object[] optionsButtonsObjects, List<UnityEngine.Object> panelComboList)
        {
            // Create Unity UI Panel via Script - prof - https://answers.unity.com/questions/1034060/create-unity-ui-panel-via-script.html - Accessed 23.10.2021
            GameObject newCanvas = new GameObject("Canvas");
            Canvas canvas = newCanvas.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;//RenderMode.ScreenSpaceOverlay;
            newCanvas.AddComponent<CanvasScaler>();
            newCanvas.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject("Panel");
            panel.AddComponent<CanvasRenderer>();
            panel.transform.SetParent(newCanvas.transform, false);

            newCanvas.transform.position = buttonList[3].transform.position - 3f * (buttonList[3].transform.position - buttonList[1].transform.position);//new Vector3(206.7f, 17.6f, 135.0f);
            newCanvas.transform.rotation = buttonList[3].transform.rotation;
            newCanvas.transform.SetParent(buttonList[3].transform.parent);
            newCanvas.transform.localScale = buttonList[3].transform.localScale;

            // Can't add text to panel gameObject because it already has image... But adding to newCanvas makes text act weird and it still doesn't show up.
            Button but = panel.AddComponent<Button>();
            Text tex = panel.AddComponent<Text>();
            tex.text = "MES Mod";
            tex.font = referenceCategoryText.font;
            tex.color = referenceCategoryText.color;
            tex.fontSize = referenceCategoryText.fontSize;
            tex.fontStyle = referenceCategoryText.fontStyle;
            tex.rectTransform.sizeDelta = referenceCategoryImage.rectTransform.sizeDelta + new Vector2(0.25f * referenceCategoryImage.rectTransform.sizeDelta.x, 0);
            tex.alignment = referenceCategoryText.alignment;
            return new GameObject[] { newCanvas, panel };
        }
        */

        public static GameObject[] CreateMultipleChoiceDescriptionBox(string description, string name, string[] choices, Transform referenceTransform, Vector3 referenceOffset, bool smallOption = true, bool topLevel = false)
        {
            float xShift = 65f / sizeReduction + Mathf.Abs((dropDownOptionImage.rectTransform.sizeDelta.x - ((referenceOptionImage.rectTransform.sizeDelta.x / 2.25f) / sizeReduction)) / 2);
            //Debug.Log("dropDownOptionImage.rectTransform.sizeDelta.x = " + dropDownOptionImage.rectTransform.sizeDelta.x + ", referenceOptionImage.rectTransform.sizeDelta.x = " + referenceOptionImage.rectTransform.sizeDelta.x + ", ((referenceOptionImage.rectTransform.sizeDelta.x / 2.25f) / sizeReduction) = " + ((referenceOptionImage.rectTransform.sizeDelta.x / 2.25f) / sizeReduction) + ",(dropDownOptionImage.rectTransform.sizeDelta.x - ((referenceOptionImage.rectTransform.sizeDelta.x / 2.25f) / sizeReduction)) / 2 = " + (dropDownOptionImage.rectTransform.sizeDelta.x - ((referenceOptionImage.rectTransform.sizeDelta.x / 2.25f) / sizeReduction)) / 2 + ", Total = " + xShift);
            float arrowShift = xShift + dropDownOptionImage.rectTransform.sizeDelta.x / 2 + 10f / sizeReduction;/*62.5f / sizeReduction;*/
            GameObject[] nameTextGOs = CreateText(name, referenceTransform, referenceOffset - new Vector3(60f / sizeReduction, 0f, 0f), smallOption, topLevel); // Name of setting
            GameObject[] leftImageGOs = CreateTextAndImage(name, referenceTransform, referenceOffset + new Vector3(xShift, 0f, 0f), smallOption, topLevel); // Selected choice
            GameObject[] rightImageBoxBaseGOs = CreateImageButton(name, referenceTransform, referenceOffset + new Vector3(arrowShift, 0f, 0f), topLevel);
            GameObject[] rightImageArrowGOs = CreateImage(name, referenceTransform, referenceOffset + new Vector3(arrowShift, 0f, 0f), topLevel);

            Text nameText = nameTextGOs[1].GetComponent<Text>();
            nameText.transform.localPosition -= new Vector3(0.25f * nameText.rectTransform.sizeDelta.x, 0f, 0f) / sizeReduction;
            nameText.rectTransform.sizeDelta = new Vector2(1.5f * nameText.rectTransform.sizeDelta.x, 1.5f * nameText.rectTransform.sizeDelta.y);
            nameText.raycastTarget = true;
            nameText.alignment = TextAnchor.MiddleRight;

            Image leftImage = leftImageGOs[3].GetComponent<Image>();
            leftImage.rectTransform.sizeDelta = dropDownOptionImage.rectTransform.sizeDelta;
            Text leftImageText = leftImageGOs[1].GetComponent<Text>();
            Image rightImageBoxBase = rightImageBoxBaseGOs[1].GetComponent<Image>();
            rightImageBoxBase.rectTransform.sizeDelta = dropDownButtonBoxImage.rectTransform.sizeDelta;
            Button rightImageBoxBaseButton = rightImageBoxBaseGOs[1].GetComponent<Button>();
            AddColourEventToImage(rightImageBoxBaseGOs[1]);

            Image rightImageArrow = rightImageArrowGOs[1].GetComponent<Image>(); // Not initially disabled. Should be disabled with background box.
            rightImageArrow.rectTransform.sizeDelta = dropDownArrowImage.rectTransform.sizeDelta;
            rightImageArrow.sprite = dropDownArrowImage.sprite;
            rightImageArrow.color = dropDownArrowImage.color;
            rightImageArrow.raycastTarget = false;

            // Create and resize a dropdown box and then put dropdown choices on top of it.
            List<GameObject> dropdownGOs = new List<GameObject>();
            GameObject[] dropdownBoxGOs = CreateImage(name + "DROPDOWN_BOX", leftImage.transform, new Vector3(0f, -2f * leftImage.rectTransform.sizeDelta.y, 0f), topLevel, referenceTransform.parent);
            Image backgroundImage = dropdownBoxGOs[1].GetComponent<Image>();
            if (choices.Length > 3)
            {
                backgroundImage.transform.localPosition = new Vector3(0f, -leftImage.rectTransform.sizeDelta.y / 2f, 0f); // For some reason the background image is not shifted properly when there are only 3 options.
            }
            backgroundImage.rectTransform.sizeDelta = new Vector2(leftImage.rectTransform.sizeDelta.x, choices.Length * leftImage.rectTransform.sizeDelta.y);
            backgroundImage.color = new Color(backgroundImage.color.r, backgroundImage.color.g, backgroundImage.color.b, 1f);
            dropdownGOs.AddRange(dropdownBoxGOs);
            for (int i = 0; i < choices.Length; i++)
            {
                GameObject[] choiceGOs = CreateTextAndImageButton(choices[i] + "_DROPDOWN_CHOICE", leftImage.transform, new Vector3(0f, (i + 1) * -leftImage.rectTransform.sizeDelta.y, 0f), smallOption, topLevel, referenceTransform.parent);
                choiceGOs[3].GetComponent<Image>().rectTransform.sizeDelta = leftImage.rectTransform.sizeDelta;
                dropdownGOs.AddRange(choiceGOs);

                int storableInt = i;
                choiceGOs[3].GetComponent<Button>().onClick.AddListener(delegate ()
                {
                    leftImageText.text = choices[storableInt];
                    foreach (GameObject gameObject in dropdownGOs)
                    {
                        gameObject.SetActive(false);
                    }
                });
            }
            foreach (GameObject gameObject in dropdownGOs)
            {
                gameObject.SetActive(false);
            }

            // Add button functionality to the drop down menu.
            rightImageBoxBaseButton.onClick.AddListener(delegate ()
            {
                if (dropdownBoxGOs[0].activeSelf)
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

            List<GameObject> allChoiceGOs = new List<GameObject>();
            allChoiceGOs.AddRange(leftImageGOs);
            allChoiceGOs.AddRange(rightImageBoxBaseGOs);
            allChoiceGOs.AddRange(rightImageArrowGOs);

            // Add a hover description box to the nameText.
            GameObject[] descriptionTextGameObjects = CreateDescriptionBox(description, name, referenceTransform, referenceOffset, smallOption, topLevel, nameTextGOs, allChoiceGOs.ToArray());

            return new GameObject[] { nameTextGOs[0], nameTextGOs[1], leftImageGOs[0], leftImageGOs[1], leftImageGOs[2], leftImageGOs[3], rightImageBoxBaseGOs[0], rightImageBoxBaseGOs[1], rightImageArrowGOs[0], rightImageArrowGOs[1], descriptionTextGameObjects[0], descriptionTextGameObjects[1], descriptionTextGameObjects[2], descriptionTextGameObjects[3] };
        }

        public static GameObject[] CreateInputFieldDescriptionBox(string description, string name, Transform referenceTransform, Vector3 referenceOffset, bool useInt, float minClamp, float maxClamp, bool smallOption = true, bool topLevel = false)
        {
            GameObject[] inputFieldGOs = CreateInputField(name, referenceTransform, referenceOffset, useInt, minClamp, maxClamp, smallOption, topLevel);
            inputFieldGOs[0].transform.localPosition += new Vector3(65f, 0f, 0f) / sizeReduction;
            inputFieldGOs[2].transform.localPosition += new Vector3(65f, 0f, 0f) / sizeReduction;

            GameObject[] textGOs = CreateText(name + "_TEXT", referenceTransform, referenceOffset, smallOption, topLevel);
            Text nameText = textGOs[1].GetComponentInChildren<Text>();
            nameText.transform.localPosition -= new Vector3(0.25f * nameText.rectTransform.sizeDelta.x, 0f, 0f) / sizeReduction;
            nameText.rectTransform.sizeDelta = new Vector2(1.5f * nameText.rectTransform.sizeDelta.x, 1.5f * nameText.rectTransform.sizeDelta.y);
            nameText.raycastTarget = true;
            nameText.transform.localPosition -= new Vector3(60f, 0f, 0f) / sizeReduction;
            nameText.alignment = TextAnchor.MiddleRight;

            GameObject[] descriptionTextGameObjects = CreateDescriptionBox(description, name, referenceTransform, referenceOffset, smallOption, topLevel, textGOs, inputFieldGOs);

            return new GameObject[] { inputFieldGOs[0], inputFieldGOs[1], inputFieldGOs[2], inputFieldGOs[3], textGOs[0], textGOs[1], descriptionTextGameObjects[0], descriptionTextGameObjects[1], descriptionTextGameObjects[2], descriptionTextGameObjects[3], inputFieldGOs[4] };
        }

        public static GameObject[] CreateBoolDescriptionBox(string description, string name, Transform referenceTransform, Vector3 referenceOffset, bool smallOption = true, bool topLevel = false)
        {
            GameObject[] boolButtonGameObjects = CreateBoolButton(name, referenceTransform, referenceOffset, smallOption, topLevel);
            Text nameText = boolButtonGameObjects[5].GetComponentInChildren<Text>();
            nameText.transform.localPosition -= new Vector3(0.25f * nameText.rectTransform.sizeDelta.x, 0f, 0f) / sizeReduction;
            nameText.rectTransform.sizeDelta = new Vector2(1.5f * nameText.rectTransform.sizeDelta.x, 1.5f * nameText.rectTransform.sizeDelta.y);

            GameObject[] descriptionTextGameObjects = CreateDescriptionBox(description, name, referenceTransform, referenceOffset, smallOption, topLevel, boolButtonGameObjects, null, 5);

            return new GameObject[10] { boolButtonGameObjects[0], boolButtonGameObjects[1], boolButtonGameObjects[2], boolButtonGameObjects[3], boolButtonGameObjects[4], boolButtonGameObjects[5], descriptionTextGameObjects[0], descriptionTextGameObjects[1], descriptionTextGameObjects[2], descriptionTextGameObjects[3] };
        }

        private static GameObject[] CreateDescriptionBox(string description, string name, Transform referenceTransform, Vector3 referenceOffset, bool smallOption, bool topLevel, GameObject[] textCanvasGOs, GameObject[] additionalGOsToHideOnHover = null, int indexOfTextCanvas = 1)
        {
            GameObject[] descriptionTextGameObjects = CreateTextAndImage(name + "DESCRIPTION_BOX", referenceTransform, referenceOffset + new Vector3(0f, 0f, -5f), smallOption, topLevel, referenceTransform.parent);
            //descriptionTextGameObjects[0].GetComponent<Canvas>().sortingOrder = 3;
            //descriptionTextGameObjects[2].GetComponent<Canvas>().sortingOrder = 2;
            Image descriptionImage = descriptionTextGameObjects[3].GetComponentInChildren<Image>();
            descriptionImage.rectTransform.sizeDelta = new Vector2(2.75f * descriptionImage.rectTransform.sizeDelta.x, 5f * descriptionImage.rectTransform.sizeDelta.y * ((description.Length + (110f - (description.Length % 55f))) / 325f));
            descriptionImage.color = new Color(descriptionImage.color.r, descriptionImage.color.g, descriptionImage.color.b, 1f);
            Text text = descriptionTextGameObjects[1].GetComponentInChildren<Text>();
            text.rectTransform.sizeDelta = new Vector2(0.975f * descriptionImage.rectTransform.sizeDelta.x, 0.975f * descriptionImage.rectTransform.sizeDelta.y);
            text.text = description;

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

            List<GameObject> settingsButtonGameObjects = new List<GameObject>();
            settingsButtonGameObjects.AddRange(textCanvasGOs);
            if (additionalGOsToHideOnHover != null)
            {
                settingsButtonGameObjects.AddRange(additionalGOsToHideOnHover);
            }


            EventTrigger trigger = textCanvasGOs[indexOfTextCanvas].AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((eventData) =>
            {
                AudioSystem.instance.StartCoroutine(RevealDescriptionAfterTime(descriptionTextGameObjects, settingsButtonGameObjects, trigger));
            });
            trigger.triggers.Add(entry);


            EventTrigger trigger2 = descriptionTextGameObjects[2].AddComponent<EventTrigger>();
            EventTrigger.Entry entry2 = new EventTrigger.Entry();
            entry2.eventID = EventTriggerType.PointerExit;
            entry2.callback.AddListener((eventData) =>
            {
                foreach (GameObject gameObject in descriptionTextGameObjects)
                {
                    gameObject.SetActive(false);
                }

                foreach (GameObject gameObject in settingsButtonGameObjects)
                {
                    gameObject.SetActive(true);
                }
            });
            trigger2.triggers.Add(entry2);

            descriptionTextGameObjects[0].SetActive(false);
            descriptionTextGameObjects[2].SetActive(false);

            return descriptionTextGameObjects;
        }

        private static IEnumerator RevealDescriptionAfterTime(GameObject[] descriptionGameObjects, List<GameObject> settingsButtonGameObjects, EventTrigger trigger)
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
                foreach (GameObject gameObject in descriptionGameObjects)
                {
                    gameObject.SetActive(true);
                }

                foreach (GameObject gameObject in settingsButtonGameObjects)
                {
                    gameObject.SetActive(false);
                }
            }
            trigger.triggers.Remove(entry);
        }

        public static GameObject[] CreateSliderDescriptionBox(string description, string name, Transform referenceTransform, Vector3 referenceOffset, bool useInt, float minClamp, float maxClamp, bool smallOption = true, bool topLevel = false, Transform alternativeParentTransform = null)
        {
            GameObject[] sliderBackgroundGOs = CreateSlider(name, referenceTransform, referenceOffset + new Vector3(102.5f, 0f, 0f), useInt, minClamp, maxClamp, smallOption, topLevel, alternativeParentTransform); // 102.5 Clyde 1

            // Create an input field to hold the value for the slider and to allow for manual entry of the value.
            GameObject[] inputFieldWithDescriptionBox = CreateInputFieldDescriptionBox(description, name, referenceTransform, referenceOffset, useInt, minClamp, maxClamp, smallOption, topLevel);

            InputField inputField = inputFieldWithDescriptionBox[10].GetComponent<InputField>();

            Slider slider = sliderBackgroundGOs[1].GetComponent<Slider>();
            slider.onValueChanged.AddListener((sliderValue) => { inputField.text = sliderValue.ToString(); });

            EventTrigger trigger = inputFieldWithDescriptionBox[3].GetComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((eventData) =>
            {
                inputField.StartCoroutine(UpdateSliderAfterInputField(inputField, slider));
            });
            trigger.triggers.Add(entry);

            return new GameObject[] { inputFieldWithDescriptionBox[0], inputFieldWithDescriptionBox[1], inputFieldWithDescriptionBox[2], inputFieldWithDescriptionBox[3], inputFieldWithDescriptionBox[4], inputFieldWithDescriptionBox[5], inputFieldWithDescriptionBox[6], inputFieldWithDescriptionBox[7], inputFieldWithDescriptionBox[8], inputFieldWithDescriptionBox[9], inputFieldWithDescriptionBox[10], sliderBackgroundGOs[0], sliderBackgroundGOs[1] };
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

        public static GameObject[] CreateSlider(string name, Transform referenceTransform, Vector3 referenceOffset, bool useInt, float minClamp, float maxClamp, bool smallOption = true, bool topLevel = false, Transform alternativeParentTransform = null)
        {
            // Unity Slider in 4 Minutes - [Unity Tutorial] - Tarodev - https://youtu.be/nTLgzvklgU8 - Accessed 03.01.2022
            GameObject[] sliderBackgroundGOs = CreateImage(name, referenceTransform, referenceOffset, topLevel, alternativeParentTransform);

            Slider slider = sliderBackgroundGOs[1].AddComponent<Slider>();
            slider.colors = referenceSlider.colors;
            slider.direction = referenceSlider.direction;
            slider.navigation = referenceSlider.navigation;
            slider.spriteState = referenceSlider.spriteState;
            slider.transition = referenceSlider.transition;
            //slider.wholeNumbers = referenceSlider.wholeNumbers;
            slider.wholeNumbers = useInt ? true : false;
            slider.minValue = minClamp;
            slider.maxValue = maxClamp;

            Image sliderBackgroundImage = sliderBackgroundGOs[1].GetComponent<Image>();
            sliderBackgroundImage.sprite = referenceSliderBackgroundImage.sprite;
            sliderBackgroundImage.type = referenceSliderBackgroundImage.type;
            sliderBackgroundImage.material = referenceSliderBackgroundImage.material;
            //sliderBackgroundImage.color = referenceSliderBackgroundImage.color;

            GameObject[] fillGOs = CreateImage(name, referenceTransform, referenceOffset, topLevel, sliderBackgroundGOs[1].transform);
            Image fillImage = fillGOs[1].GetComponent<Image>();
            fillImage.sprite = referenceSliderFillImage.sprite;
            fillImage.type = referenceSliderFillImage.type;
            fillImage.material = referenceSliderFillImage.material;
            fillImage.color = referenceSliderFillImage.color;
            fillImage.raycastTarget = false;

            GameObject[] handleGOs = CreateImage(name, referenceTransform, referenceOffset, topLevel, sliderBackgroundGOs[1].transform);
            Image handleImage = handleGOs[1].GetComponent<Image>();
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

            sliderBackgroundImage.rectTransform.sizeDelta = new Vector2(0.945f * sliderBackgroundImage.rectTransform.sizeDelta.x, sliderBackgroundImage.rectTransform.sizeDelta.y / 2f);//slider.image.rectTransform.sizeDelta = new Vector2(slider.image.rectTransform.sizeDelta.x, slider.image.rectTransform.sizeDelta.y / 2f);
            slider.fillRect.sizeDelta = new Vector2(/*2f * */referenceSliderFillImage.rectTransform.sizeDelta.x, slider.fillRect.sizeDelta.y - fillHeightDifference - fillHeightBefore / 2f);
            slider.handleRect.sizeDelta = new Vector2(slider.handleRect.sizeDelta.x, slider.handleRect.sizeDelta.y - handleHeightDifference);

            // Scale the slider and adjust for slider scaling
            slider.transform.localScale = new Vector3(0.5f, 1f, 1f); // how to change the scale property of rect transform unity - Obnoxious Oystercatcher - https://www.codegrepper.com/code-examples/csharp/how+to+change+the+scale+property+of+rect+transform+unity - Accessed 03.01.2022
            handleImage.transform.localScale = new Vector3(2f, 1f, 1f);

            //Debug.Log("-----\nRects after correction:\nBackground = " + sliderBackgroundImage.rectTransform.rect + "\nFill = " + fillImage.rectTransform.rect + "\n Handle = " + handleImage.rectTransform.rect + "\n-----\nSizeDeltas after correction:\nBackground = " + sliderBackgroundImage.rectTransform.sizeDelta + "\nFill = " + fillImage.rectTransform.sizeDelta + "\nHandle = " + handleImage.rectTransform.sizeDelta);
            return sliderBackgroundGOs;
        }

        public static GameObject[] CreateBoolButton(string name, Transform referenceTransform, Vector3 referenceOffset, bool smallOption = true, bool topLevel = false)
        {
            GameObject[] textAndImageGameObjects = CreateTextAndImageButton(name, referenceTransform, referenceOffset, smallOption, topLevel);
            Image smallImage = textAndImageGameObjects[3].GetComponentInChildren<Image>();
            smallImage.rectTransform.sizeDelta = referenceBoolButtonImage.rectTransform.sizeDelta;
            GameObject[] smallTextGameObject = CreateText(name + "_ImageText", referenceTransform, referenceOffset, smallOption, topLevel);
            smallTextGameObject[0].transform.SetParent(referenceTransform, false);
            Text smallText = textAndImageGameObjects[1].GetComponentInChildren<Text>();
            Text independentText = smallTextGameObject[1].GetComponentInChildren<Text>();
            independentText.raycastTarget = true;
            independentText.transform.localPosition -= new Vector3(60f, 0f, 0f) / sizeReduction;
            independentText.alignment = TextAnchor.MiddleRight;
            smallText.text = "X";
            smallText.font = referenceBoolButtonText.font;
            smallText.color = referenceBoolButtonText.color;
            smallText.fontSize = referenceBoolButtonText.fontSize;
            smallText.fontStyle = referenceBoolButtonText.fontStyle;
            smallText.rectTransform.sizeDelta = referenceBoolButtonImage.rectTransform.sizeDelta;
            smallText.alignment = referenceBoolButtonText.alignment;
            smallText.transform.localPosition += new Vector3(65f, 0f, 0f) / sizeReduction;
            smallImage.transform.localPosition += new Vector3(65f, 0f, 0f) / sizeReduction;

            Button smallButton = textAndImageGameObjects[3].GetComponentInChildren<Button>();
            smallButton.onClick.AddListener(delegate ()
                {
                    if (smallText.text.Equals("X"))
                    {
                        smallText.text = string.Empty;
                    }
                    else
                    {
                        smallText.text = "X";
                    }
                });
            //Debug.Log("Bool button positions: Image = " + smallImage.transform.position + ", Independent Text: " + textAndImageGameObjects[1].GetComponentInChildren<Text>().transform.position + ", Small Text = " + smallText.transform.position);
            return new GameObject[6] { textAndImageGameObjects[0], textAndImageGameObjects[1], textAndImageGameObjects[2], textAndImageGameObjects[3], smallTextGameObject[0], smallTextGameObject[1] };
        }

        public static GameObject[] CreateInputField(string name, Transform referenceTransform, Vector3 referenceOffset, bool useInt, float minClamp, float maxClamp, bool smallOption = true, bool topLevel = false)
        {
            GameObject[] canvasGameObjects = CreateTextAndImage(name, referenceTransform, referenceOffset, smallOption, topLevel);

            Image image = canvasGameObjects[3].GetComponentInChildren<Image>();
            image.rectTransform.sizeDelta = new Vector2((image.rectTransform.sizeDelta.x / 2.25f) / sizeReduction, image.rectTransform.sizeDelta.y);


            GameObject inputFieldGameObject = new GameObject(name.Replace(' ', '_') + "-InputFieldGameObject");
            InputField inputField = inputFieldGameObject.AddComponent<InputField>();
            inputFieldGameObject.transform.SetParent(canvasGameObjects[3].transform, false);
            inputFieldGameObject.transform.localPosition = Vector3.zero;

            Text text = canvasGameObjects[1].GetComponentInChildren<Text>();
            text.rectTransform.sizeDelta = image.rectTransform.sizeDelta;

            inputField.targetGraphic = image;
            inputField.textComponent = text;
            inputField.text = "This is an input field";

            EventTrigger trigger = canvasGameObjects[3].AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerClick;
            entry.callback.AddListener((eventData) =>
            {
                inputField.ActivateInputField();
                inputField.StartCoroutine(InputFieldInputValidation(inputField, useInt, minClamp, maxClamp));
            });
            trigger.triggers.Add(entry);

            EventTrigger.Entry entry2 = new EventTrigger.Entry();
            entry2.eventID = EventTriggerType.PointerEnter;
            entry2.callback.AddListener((eventData) => { image.color = new Color(245f / 255f, 210f / 255f, 140f / 255f, referenceOptionImage.color.a); });
            trigger.triggers.Add(entry2);

            EventTrigger.Entry entry3 = new EventTrigger.Entry();
            entry3.eventID = EventTriggerType.PointerExit;
            entry3.callback.AddListener((eventData) => { image.color = referenceOptionImage.color; });
            trigger.triggers.Add(entry3);

            //Debug.Log("Image is at " + image.transform.position + " and input field is at " + inputField.transform.position + " with text at " + text.transform.position);
            return new GameObject[] { canvasGameObjects[0], canvasGameObjects[1], canvasGameObjects[2], canvasGameObjects[3], inputFieldGameObject };
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

        private static GameObject[] CreateTextAndImage(string name, Transform referenceTransform, Vector3 referenceOffset, bool smallOption = true, bool topLevel = false, Transform alternativeParentTransform = null)
        {
            // Order of creation matters with layering
            GameObject[] canvasGameObjectsImageButton = CreateImage(name, referenceTransform, referenceOffset, topLevel);
            GameObject[] canvasGameObjectsText = CreateText(name, referenceTransform, referenceOffset, smallOption, topLevel);

            if (alternativeParentTransform != null)
            {
                //Debug.Log("Setting alternative transform for TextAndImage");
                canvasGameObjectsImageButton[0].transform.SetParent(alternativeParentTransform, true);
                canvasGameObjectsText[0].transform.SetParent(alternativeParentTransform, true);
            }

            //Debug.Log("-----\nCreated " + canvasGameObjectsText[0].name + ":\nPosition: " + canvasGameObjectsText[0].transform.position + " / " + canvasGameObjectsText[0].transform.localPosition + "\nRotation: " + canvasGameObjectsText[0].transform.rotation + " / " + canvasGameObjectsText[0].transform.localRotation + "\n...and " + canvasGameObjectsImageButton[1].name + ":\nPosition: " + canvasGameObjectsImageButton[1].transform.position + " / " + canvasGameObjectsImageButton[1].transform.localPosition + "\nRotation: " + canvasGameObjectsImageButton[1].transform.rotation + " / " + canvasGameObjectsImageButton[1].transform.localRotation + "\n-----");
            return new GameObject[] { canvasGameObjectsText[0], canvasGameObjectsText[1], canvasGameObjectsImageButton[0], canvasGameObjectsImageButton[1] };
        }

        private static GameObject[] CreateTextAndImageButton(string name, Transform referenceTransform, Vector3 referenceOffset, bool smallOption = true, bool topLevel = false, Transform alternativeParentTransform = null, bool addColourEvent = true)
        {
            // Order of creation matters with layering
            GameObject[] canvasGameObjectsImageButton = CreateImageButton(name, referenceTransform, referenceOffset, topLevel);
            GameObject[] canvasGameObjectsText = CreateText(name, referenceTransform, referenceOffset, smallOption, topLevel);

            Button button = canvasGameObjectsImageButton[1].GetComponentInChildren<Button>();
            if (addColourEvent)
            {
                AddColourEventToImage(canvasGameObjectsImageButton[1]);
            }

            if (alternativeParentTransform != null)
            {
                //Debug.Log("Setting alternative transform for TextAndImageButton");
                canvasGameObjectsImageButton[0].transform.SetParent(alternativeParentTransform, true);
                canvasGameObjectsText[0].transform.SetParent(alternativeParentTransform, true);
            }

            //Debug.Log("-----\nCreated " + canvasGameObjectsText[0].name + ":\nPosition: " + canvasGameObjectsText[0].transform.position + " / " + canvasGameObjectsText[0].transform.localPosition + "\nRotation: " + canvasGameObjectsText[0].transform.rotation + " / " + canvasGameObjectsText[0].transform.localRotation + "\n...and " + canvasGameObjectsImageButton[1].name + ":\nPosition: " + canvasGameObjectsImageButton[1].transform.position + " / " + canvasGameObjectsImageButton[1].transform.localPosition + "\nRotation: " + canvasGameObjectsImageButton[1].transform.rotation + " / " + canvasGameObjectsImageButton[1].transform.localRotation + "\n-----");
            return new GameObject[] { canvasGameObjectsText[0], canvasGameObjectsText[1], canvasGameObjectsImageButton[0], canvasGameObjectsImageButton[1] };
        }

        private static void AddColourEventToImage(GameObject imageCanvasGameObject)
        {
            Image image = imageCanvasGameObject.GetComponentInChildren<Image>();
            EventTrigger trigger = imageCanvasGameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((eventData) => { image.color = new Color(245f / 255f, 210f / 255f, 140f / 255f, referenceOptionImage.color.a); });
            trigger.triggers.Add(entry);

            EventTrigger.Entry entry2 = new EventTrigger.Entry();
            entry2.eventID = EventTriggerType.PointerExit;
            entry2.callback.AddListener((eventData) => { image.color = referenceOptionImage.color; });
            trigger.triggers.Add(entry2);
        }

        private static GameObject[] CreateTextButton(string name, Transform referenceTransform, Vector3 referenceOffset, bool smallOption = true, bool topLevel = false)
        {
            GameObject[] canvasGameObjects = CreateText(name, referenceTransform, referenceOffset, smallOption, topLevel);
            Button button = canvasGameObjects[1].AddComponent<Button>();
            Text text = canvasGameObjects[1].GetComponentInChildren<Text>();
            text.raycastTarget = true;

            EventTrigger trigger = canvasGameObjects[1].AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((eventData) => { text.color = Color.black; });
            trigger.triggers.Add(entry);

            EventTrigger.Entry entry2 = new EventTrigger.Entry();
            entry2.eventID = EventTriggerType.PointerExit;
            entry2.callback.AddListener((eventData) => { text.color = referenceCategoryText.color; });
            trigger.triggers.Add(entry2);

            //Debug.Log("-----\nCreated " + canvasGameObjects[0].name + ":\nPosition: " + canvasGameObjects[0].transform.position + " / " + canvasGameObjects[0].transform.localPosition + "\nRotation: " + canvasGameObjects[0].transform.rotation + " / " + canvasGameObjects[0].transform.localRotation + "\n...and " + canvasGameObjects[1].name + ":\nPosition: " + canvasGameObjects[1].transform.position + " / " + canvasGameObjects[1].transform.localPosition + "\nRotation: " + canvasGameObjects[1].transform.rotation + " / " + canvasGameObjects[1].transform.localRotation + "\n-----");
            return canvasGameObjects;
        }

        private static GameObject[] CreateText(string name, Transform referenceTransform, Vector3 referenceOffset, bool smallOption = true, bool topLevel = false)
        {
            GameObject[] canvasGameObjects = CreateCanvasGameObjects(name, referenceTransform, referenceOffset, topLevel);
            Text text = canvasGameObjects[1].AddComponent<Text>();
            CustomiseText(text, name, smallOption/*, canvasGameObjects[1]*/);
            return canvasGameObjects;
        }

        private static void CustomiseText(Text text, string name, bool smallOption/*, GameObject referenceCanvasGameObject*/)
        {
            text.text = name.Split(new string[] { "_" }, System.StringSplitOptions.None)[0];
            text.font = referenceCategoryText.font;
            text.color = referenceCategoryText.color;
            text.fontSize = referenceCategoryText.fontSize;
            text.fontStyle = referenceCategoryText.fontStyle;
            text.raycastTarget = false;
            if (smallOption)
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

        private static GameObject[] CreateImageButton(string name, Transform referenceTransform, Vector3 referenceOffset, bool topLevel)
        {
            GameObject[] canvasGameObjects = CreateImage(name, referenceTransform, referenceOffset, topLevel);
            Button button = canvasGameObjects[1].AddComponent<Button>();
            return canvasGameObjects;
        }

        private static GameObject[] CreateImage(string name, Transform referenceTransform, Vector3 referenceOffset, bool topLevel, Transform alternativeParentTransform = null)
        {
            GameObject[] canvasGameObjects = CreateCanvasGameObjects(name, referenceTransform, referenceOffset, topLevel);
            Image image = canvasGameObjects[1].AddComponent<Image>();
            image.rectTransform.sizeDelta = referenceOptionImage.rectTransform.sizeDelta;
            image.sprite = referenceOptionImage.sprite;
            image.type = referenceOptionImage.type;
            image.material = referenceOptionImage.material;
            image.color = referenceOptionImage.color;

            if (alternativeParentTransform != null)
            {
                //Debug.Log("Setting alternative transform for Image");
                canvasGameObjects[0].transform.SetParent(alternativeParentTransform, true);
            }
            return canvasGameObjects;
        }

        private static GameObject[] CreateCanvasGameObjects(string name, Transform referenceTransform, Vector3 referenceOffset, bool topLevel)
        {
            // Create Unity UI Panel via Script - prof - https://answers.unity.com/questions/1034060/create-unity-ui-panel-via-script.html - Accessed 23.10.2021
            GameObject canvasGameObject = new GameObject(name.Replace(' ', '_') + "-CanvasGameObject");
            Canvas canvas = canvasGameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;//RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            canvasGameObject.AddComponent<CanvasScaler>();
            canvasGameObject.AddComponent<GraphicRaycaster>();

            GameObject canvasRendererGameObject = CreateCanvasRendererGameObject(name, canvasGameObject.transform);

            if (topLevel)
            {
                canvasGameObject.transform.SetParent(referenceTransform.parent, false);
            }
            else
            {
                canvasGameObject.transform.SetParent(referenceTransform, false);
            }

            canvasGameObject.transform.localPosition = Vector3.zero;
            canvasGameObject.transform.localRotation = Quaternion.identity;

            if (topLevel)
            {
                canvasGameObject.transform.position += referenceOffset;
            }
            else
            {
                canvasGameObject.transform.localPosition += referenceOffset;
            }

            return new GameObject[] { canvasGameObject, canvasRendererGameObject };
        }

        private static GameObject CreateCanvasRendererGameObject(string name, Transform parentTransform)
        {
            GameObject canvasRendererGameObject = new GameObject(name.Replace(' ', '_') + "-CanvasRendererGameObject");
            canvasRendererGameObject.AddComponent<CanvasRenderer>();
            canvasRendererGameObject.transform.SetParent(parentTransform, false);
            return canvasRendererGameObject;
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

            public GameObject[] CreateRGBButton(Transform referenceTransform, Vector3 referenceOffset, Text displayText)
            {
                this.displayText = displayText;
                base.CreateButtonForSetting(referenceTransform, referenceOffset); // Creates and assigns settingsButton

                // Edit settingsButton
                inputField = settingsButton[10].GetComponent<InputField>();

                EventTrigger trigger = settingsButton[3].GetComponent<EventTrigger>();
                EventTrigger.Entry entry = new EventTrigger.Entry();
                entry.eventID = EventTriggerType.PointerClick;

                entry.callback.AddListener((eventData) =>
                {
                    inputField.StartCoroutine(ChangeDisplayTextColourAfterInput());
                });
                trigger.triggers.Add(entry);

                Slider slider = settingsButton[12].GetComponent<Slider>();
                slider.onValueChanged.AddListener((sliderValue) => { ChangeTextColour(); });

                ChangeTextColour();

                return settingsButton;
            }
        }

        /*----------------------------------------------------------------------------------------------------*/
    }
}
// ~End Of File