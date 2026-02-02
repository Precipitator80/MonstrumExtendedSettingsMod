using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace MonstrumExtendedSettingsMod
{
    public partial class ExtendedSettingsModScript
    {
        public class HintImageManager : MonoBehaviour
        {
            /// <summary>
            /// All hint screen textures. Loaded once.
            /// </summary>
            static List<Texture2D> textures;

            /// <summary>
            /// The hint images applicable to the current game.
            /// </summary>
            readonly List<GameObject> hintImages = new List<GameObject>();
            /// <summary>
            /// The currently shown image or -1 if showing none.
            /// </summary>
            int currentImageIndex;

            void Start()
            {
                // Load the asset bundle once.
                if (textures == null)
                {
                    textures = new List<Texture2D>();
                    var assetBundle = Utilities.LoadAssetBundle("hintimages");
                    foreach (Object obj in assetBundle)
                    {
                        if (obj is Texture2D tex)
                        {
                            textures.Add(tex);
                        }
                    }
                }

                // Process the loaded textures each game.
                if (textures.Count > 0)
                {
                    // Set conditions for the images in the bundle.
                    Dictionary<string, bool> hintConditions = new Dictionary<string, bool>()
                    {
                        { "DebugMode", ModSettings.debugMode },
                        { "EscapeConditions", ModSettings.escapeConditionsToWin > 0 },
                        { "GlowstickHunt", ModSettings.glowstickHunt && !ModSettings.noGlowstickHuntFinale },
                        { "GlowstickHuntNoFinale", ModSettings.glowstickHunt && ModSettings.noGlowstickHuntFinale}
                    };

                    // Check which images to use.
                    foreach (Texture2D tex in textures)
                    {
                        if (hintConditions.ContainsKey(tex.name) && hintConditions[tex.name])
                        {
                            GameObject imageGO = new GameObject(tex.name);
                            imageGO.SetActive(false);

                            var trans = imageGO.AddComponent<RectTransform>();
                            var canvas = Reticule.Instance.reticuleCanvas.canvas;
                            trans.SetParent(canvas.transform);
                            trans.anchoredPosition = new Vector2(0f, 0f);
                            trans.sizeDelta = canvas.pixelRect.size * 0.85f;

                            var image = imageGO.AddComponent<Image>();
                            image.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

                            hintImages.Add(imageGO);
                        }
                    }

                    // Set the first image to enabled if there is one.
                    if (hintImages.Count > 0)
                    {
                        hintImages[0].SetActive(true);
                    }
                }
            }

            void Update()
            {
                // Open and cycle through images with X.
                if (Input.GetKeyDown(KeyCode.X))
                {
                    CycleHints();
                }
            }

            public void CycleHints()
            {
                if (hintImages.Count > 0)
                {
                    // Hide the current image.
                    if (currentImageIndex >= 0 && currentImageIndex < hintImages.Count)
                        hintImages[currentImageIndex].SetActive(false);

                    // Advance to the next image.
                    currentImageIndex++;

                    // If all have been shown, reset the index to -1 and return to hide all images.
                    if (currentImageIndex >= hintImages.Count)
                    {
                        currentImageIndex = -1;
                        return;
                    }

                    // If there is a next image, show it.
                    hintImages[currentImageIndex].SetActive(true);
                }
            }
        }
    }
}