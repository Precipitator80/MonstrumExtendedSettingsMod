// ~Beginning Of File
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using MonoMod.RuntimeDetour;
using System.Diagnostics;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace MonstrumExtendedSettingsMod
{

    public partial class ExtendedSettingsModScript
    {
        /*----------------------------------------------------------------------------------------------------*/
        // ~MultiplayerMode

        private static class MultiplayerMode
        {
            public static List<NewPlayerClass> newPlayerClasses; // Players can be retrieved by getting newPlayerClasses[i].gameObject.
            public static List<Inventory> inventories;
            private static InventoryUI[] inventoryUIs;
            private static PlayerUpperBodyLock[] playerUpperBodyLocks;
            public static NewPlayerClass lastPlayerSentMessage;
            public static NewPlayerClass lastPlayerCheckingInteractableConditions;
            private static Dictionary<int, int> interactablesAssociatedWithPlayer;
            //private static AudioListener[] playerAudioListeners;
            private static GameObject[] reverbContainers;
            private static Vector3[] currentCamNodes;
            public static List<NewPlayerClass> crewPlayers;
            public static List<NewPlayerClass> monsterPlayers;
            public static List<bool> playersDowned;
            private static List<MindAttackEffect> mindAttackEffects;
            private static GameObject actualAudioListener;
            //private static List<AudioSource> originalAudioSources;
            //private static List<VirtualAudioSource_ClosestListenerOnly> virtualAudioSources;
            //private static List<GameObject> audioSourceCopies;
            //private static Dictionary<int, int> audioSourceCopyToOriginalDictionary;
            private static bool finishedAudioAssignment;
            public static Dictionary<string, List<KeyBind>> customKeyBinds;
            public static bool useLegacyAudio = false;
            public static List<TriggerObjectives> triggerObjectivesList;
            public static List<PlayerObjectives> playerObjectivesList;

            // #MultiplayerModeVariableInitialisation
            public static void MultiplayerModeVariableInitialisation()
            {
                Debug.Log("Number of players is " + ModSettings.NumberOfPlayers);
                newPlayerClasses = new List<NewPlayerClass>();
                previousCameraPositions = new Vector3[ModSettings.NumberOfPlayers];
                previousVisibilityData = new List<VisibilityData>[ModSettings.NumberOfPlayers];
                inventories = new List<Inventory>();
                inventoryUIs = new InventoryUI[ModSettings.NumberOfPlayers];
                playerUpperBodyLocks = new PlayerUpperBodyLock[ModSettings.NumberOfPlayers];
                interactablesAssociatedWithPlayer = new Dictionary<int, int>();
                //playerAudioListeners = new AudioListener[ModSettings.NumberOfPlayers];
                reverbContainers = new GameObject[ModSettings.NumberOfPlayers];
                currentCamNodes = new Vector3[ModSettings.NumberOfPlayers];
                crewPlayers = new List<NewPlayerClass>();
                monsterPlayers = new List<NewPlayerClass>();
                playersDowned = new List<bool>();
                mindAttackEffects = new List<MindAttackEffect>();
                newPlayerClasses.Add(References.npc);
                lastPlayerSentMessage = newPlayerClasses[0];
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Updating lastPlayerSentMessage: " + PlayerNumber(lastPlayerSentMessage.GetInstanceID()) + "\n" + new StackTrace().ToString() + "\n-----");
                }
                lastPlayerCheckingInteractableConditions = newPlayerClasses[0];
                inventories.Add(References.Inventory);
                inventoryUIs[0] = inventories[0].UI;
                if (ModSettings.enableCrewVSMonsterMode && ModSettings.numbersOfMonsterPlayers.Contains(0))
                {
                    monsterPlayers.Add(newPlayerClasses[0]);
                }
                else
                {
                    crewPlayers.Add(newPlayerClasses[0]);
                    playersDowned.Add(false);
                }
                triggerObjectivesList = new List<TriggerObjectives>();
                playerObjectivesList = new List<PlayerObjectives>();
                finishedAudioAssignment = false;
            }

            // #SetUpMultiplayerAudio
            private static void SetUpMultiplayerAudio()
            {
                // Remove the original AudioListeners from the players.
                foreach (NewPlayerClass newPlayerClass in newPlayerClasses)
                {
                    UnityEngine.Object.Destroy(newPlayerClass.GetComponentInChildren<AudioListener>());
                }

                // Create a new, general AudioListener. Update variables where the AudioListener is referenced.
                actualAudioListener = new GameObject();
                actualAudioListener.AddComponent<AudioListener>();
                Calculations.playerListener = actualAudioListener.GetComponent<AudioListener>();
                ReverbZone.listener = actualAudioListener.GetComponent<AudioListener>();
                SimpleOcclusion.listener = actualAudioListener.GetComponent<AudioListener>();

                // Give each player a VirtualAudioListener so that any VirtualAudioSources can know how to manipulate any copied AudioSources' positions.
                foreach (NewPlayerClass newPlayerClass in newPlayerClasses)
                {
                    VirtualAudioListener virtualAudioListener = PlayerCamera(newPlayerClass).transform.gameObject.AddComponent<VirtualAudioListener>();
                }

                // Create a list of AudioSource copies to be manipulated by VirtualAudioSources.
                /*
                originalAudioSources = new List<AudioSource>(3000);
                virtualAudioSources = new List<VirtualAudioSource_ClosestListenerOnly>(3000);
                audioSourceCopies = new List<GameObject>(3000);
                audioSourceCopyToOriginalDictionary = new Dictionary<int, int>(3000);
                */

                AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
                for (int i = 0; i < allAudioSources.Length; i++)
                {
                    AddVirtualAudioSourceToAudioSource(ref allAudioSources[i]);
                }

                /*
                for (int i = 0; i < originalAudioSources.Count; i++)
                {
                    if (originalAudioSources[i] == null)
                    {
                        Debug.Log("Original audio source is null after assignment.");
                        if (allAudioSources[i] != null)
                        {
                            Debug.Log("However, the source used to make the assignment is not null.");
                        }
                    }
                }
                */

                Debug.Log("FINISHED AUDIO ASSIGNMENT");
                Debug.LogError("FINISHED AUDIO ASSIGNMENT");
                finishedAudioAssignment = true;
            }

            public static void AssignOriginalAudioSourceFromCopiedSource(ref AudioSource audioSourceToCheck, ref SimpleOcclusion simpleOcclusion)
            {
                // ~SimpleOcclusion that might be needed
                return;
                /*
                try
                {
                    if (LevelGeneration.Instance.finishedGenerating & audioSourceToCheck != null && virtualAudioSources != null)
                    {
                        try
                        {
                            foreach (VirtualAudioSource virtualAudioSource in virtualAudioSources)
                            {
                                try
                                {
                                    if (virtualAudioSource != null && virtualAudioSource.originalAudioSource != null && virtualAudioSource.mySource != null && virtualAudioSource.mySource == audioSourceToCheck)
                                    {
                                        try
                                        {
                                            audioSourceToCheck = virtualAudioSource.originalAudioSource;
                                            //simpleOcclusion.source = virtualAudioSource.originalAudioSource; // This is done by the AudioSource being passed by reference
                                            if (simpleOcclusion.volume == null)
                                            {
                                                simpleOcclusion.volume = (simpleOcclusion.gameObject.AddComponent(typeof(VolumeController)) as VolumeController);
                                            }
                                            simpleOcclusion.volume.SetSource(virtualAudioSource.mySource);
                                            break;
                                        }
                                        catch
                                        {
                                            Debug.Log("Error in AssignOriginalAudioSourceFromCopiedSource 4");
                                        }
                                    }
                                }
                                catch
                                {
                                    Debug.Log("Error in AssignOriginalAudioSourceFromCopiedSource 3");
                                }
                            }
                        }
                        catch
                        {
                            Debug.Log("Error in AssignOriginalAudioSourceFromCopiedSource 2");
                        }
                    }
                }
                catch
                {
                    Debug.Log("Error in AssignOriginalAudioSourceFromCopiedSource 1");
                }
                */
            }

            // #AddVirtualAudioSourceToAudioSource
            public static VirtualAudioSource_ClosestListenerOnly AddVirtualAudioSourceToAudioSource(ref AudioSource audioSource)
            {
                if (LevelGeneration.Instance.finishedGenerating && audioSource != null)
                {
                    // Add a VirtualAudioSource to the original AudioSource's GameObject.
                    VirtualAudioSource_ClosestListenerOnly virtualAudioSource = audioSource.gameObject.AddComponent<VirtualAudioSource_ClosestListenerOnly>();

                    // Create a new GameObject that will hold a copy of the original AudioSource, which can be moved around the AudioListener by the VirtualAudioSource.
                    GameObject audioSourceCopyGameObject = new GameObject();
                    AudioSource audioSourceCopy = audioSourceCopyGameObject.AddComponent<AudioSource>();
                    audioSource.CopyTo(audioSourceCopy);
                    audioSource.CopyEffectsTo(audioSourceCopy);

                    VolumeController originalVolumeController = audioSource.GetComponent<VolumeController>();
                    if (originalVolumeController != null)
                    {
                        originalVolumeController.SetSource(audioSourceCopy);
                    }

                    audioSource.volume = 0f;

                    // Set the VirtualAudioSource's AudioSource to the copied AudioSource so that its position can be manipulated.
                    virtualAudioSource.originalAudioSource = audioSource;
                    virtualAudioSource.mySource = audioSourceCopy;
                    return virtualAudioSource;
                }
                return null;
            }

            // #GetPlayerKey
            public static KeyBind GetPlayerKey(string keyBindName, int playerNumber)
            {
                return customKeyBinds[keyBindName][playerNumber]; // Find the right key binds list and return the key bind at the right player index of the list.
            }

            // #GetPlayerAxisValue
            public static float GetPlayerAxisValue(string axisNameOrAxisKeyName, int playerNumber)
            {
                // Check whether the player is using a controller.
                if (customKeyBinds["Forward"][playerNumber].keyInUse == KeyCode.None)
                {
                    if (axisNameOrAxisKeyName.Equals("Y") || axisNameOrAxisKeyName.Equals("y"))
                    {
                        return XboxCtrlrInput.XCI.LeftStickValueY();
                    }
                    else if (axisNameOrAxisKeyName.Equals("X") || axisNameOrAxisKeyName.Equals("x"))
                    {
                        return XboxCtrlrInput.XCI.LeftStickValueX();
                    }
                }
                return 0f;
            }

            // #GetPlayerTriggerStateIfUsingController
            public static bool GetPlayerTriggerStateIfUsingController(string triggerSide, int playerNumber, bool justDown = false, bool justUp = false)
            {
                // Check whether the player is using a controller.
                if (customKeyBinds["Forward"][playerNumber].keyInUse == KeyCode.None)
                {
                    if (triggerSide.Contains("Left"))
                    {
                        if (justDown)
                        {
                            return XboxCtrlrInput.XCI.GetTriggerDown(XboxCtrlrInput.XboxAxis.LeftTrigger);
                        }
                        else if (justUp)
                        {
                            return XboxCtrlrInput.XCI.GetTriggerUp(XboxCtrlrInput.XboxAxis.LeftTrigger);
                        }
                        else
                        {
                            return XboxCtrlrInput.XCI.GetTrigger(XboxCtrlrInput.XboxAxis.LeftTrigger);
                        }
                    }
                    else if (triggerSide.Contains("Right"))
                    {
                        if (justDown)
                        {
                            return XboxCtrlrInput.XCI.GetTriggerDown(XboxCtrlrInput.XboxAxis.RightTrigger);
                        }
                        else if (justUp)
                        {
                            return XboxCtrlrInput.XCI.GetTriggerUp(XboxCtrlrInput.XboxAxis.RightTrigger);
                        }
                        else
                        {
                            return XboxCtrlrInput.XCI.GetTrigger(XboxCtrlrInput.XboxAxis.RightTrigger);
                        }
                    }
                }
                return false;
            }

            // #MultiplayerModeAfterGenerationInitialisation
            public static void MultiplayerModeAfterGenerationInitialisation()
            {

                //MultiplayerModeVariableInitialisation(); // Should be run at the start of level generation.

                inventories[0].SortInventoryList();
                playerUpperBodyLocks[0] = newPlayerClasses[0].upperBodyLock;
                //playerAudioListeners[0] = newPlayerClasses[0].gameObject.GetComponentInChildren<AudioListener>();
                mindAttackEffects.Add(newPlayerClasses[0].GetComponentInChildren<MindAttackEffect>());
                //playerObjectivesList.Add(TriggerObjectives.instance.playerObjectives);
                MouseLock mouseLockInstance = MouseLock.Instance;
                for (int i = 1; i < ModSettings.NumberOfPlayers; i++)
                {
                    newPlayerClasses.Add(UnityEngine.Object.Instantiate<NewPlayerClass>(newPlayerClasses[0], References.Player.transform.position + References.Player.transform.forward * 0.5f * i + References.Player.transform.right * 0.5f * i, References.Player.transform.rotation));
                    newPlayerClasses[i].playerMotor = newPlayerClasses[i].gameObject.GetComponent<PlayerMotor>();
                    newPlayerClasses[i].playerMotor.GenerationFinished();
                    newPlayerClasses[i].controller = newPlayerClasses[i].gameObject.GetComponent<CharacterController>();
                    newPlayerClasses[i].playerAnimator = ((MonoBehaviour)newPlayerClasses[i]).GetComponent<Animator>();
                    inventories.Add(((MonoBehaviour)newPlayerClasses[i]).GetComponentInChildren<Inventory>());
                    inventories[i].newPlayerClass = newPlayerClasses[i];
                    inventories[i].fio = newPlayerClasses[i].GetComponentInChildren<FindInteractiveObject>();
                    inventories[i].SortInventoryList();
                    inventoryUIs[i] = UnityEngine.Object.Instantiate<InventoryUI>(inventories[i].UI);
                    inventories[i].inventoryUI = inventoryUIs[i];
                    newPlayerClasses[i].upperBodyLock = UnityEngine.Object.FindObjectOfType<PlayerUpperBodyLock>();
                    //newPlayerClasses[i].upperBodyLock = newPlayerClasses[i].gameObject.GetComponent<PlayerUpperBodyLock>();
                    playerUpperBodyLocks[i] = newPlayerClasses[i].upperBodyLock;
                    if (ModSettings.enableCrewVSMonsterMode && ModSettings.numbersOfMonsterPlayers.Contains(i))
                    {
                        monsterPlayers.Add(newPlayerClasses[i]);
                    }
                    else
                    {
                        crewPlayers.Add(newPlayerClasses[i]);
                        playersDowned.Add(false);
                    }
                    mindAttackEffects.Add(newPlayerClasses[i].GetComponentInChildren<MindAttackEffect>());
                    if (useLegacyAudio)
                    {
                        UnityEngine.Object.Destroy(newPlayerClasses[i].GetComponentInChildren<AudioListener>());
                    }
                    AutoPronePlayer autoPronePlayer = newPlayerClasses[i].GetComponentInChildren<AutoPronePlayer>();
                    if (autoPronePlayer != null)
                    {
                        Debug.Log("Found autoPronePlayer in newPlayerClass");
                        autoPronePlayer.player = newPlayerClasses[i];
                    }
                    else
                    {
                        Debug.Log("Did not find autoPronePlayer in newPlayerClass");
                    }
                    PlayerDustParticles playerDustParticles = newPlayerClasses[i].GetComponentInChildren<PlayerDustParticles>();
                    if (playerDustParticles != null)
                    {
                        Debug.Log("Found playerDustParticles in newPlayerClass");
                    }
                    else
                    {
                        Debug.Log("Did not find playerDustParticles in newPlayerClass");
                    }
                    Debug.Log("PlayerMotor ID is: " + newPlayerClasses[i].Motor.GetInstanceID() + " and controller ID is: " + newPlayerClasses[i].controller.GetInstanceID() + " and detectRoom is " + newPlayerClasses[i].gameObject.GetComponentInChildren<DetectRoom>().GetInstanceID());
                }

                for (int i = 1; i < ModSettings.NumberOfPlayers; i++)
                {

                    Camera playerCamera = PlayerCamera(newPlayerClasses[i]);
                    //playerCamera = UnityEngine.Object.Instantiate<Camera>(References.camLeft);

                    References.camMiddle = playerCamera.transform;
                    References.camLeft = playerCamera;
                    References.camRight = playerCamera; // May not be necessary, but may also avoid double vision for player 2.

                    /*
                    Camera[] originalCameras = newPlayerClasses[0].GetComponentsInChildren<Camera>(false);
                    Camera[] cloneCameras = newPlayerClasses[i].GetComponentsInChildren<Camera>(false);
                    for (int j = 0; j < cloneCameras.Length; j++)
                    {
                        cloneCameras[j] = UnityEngine.Object.Instantiate<Camera>(originalCameras[j], newPlayerClasses[i].gameObject.transform.position, newPlayerClasses[i].gameObject.transform.rotation);
                    }
                    */
                }


                //AudioListener audioListener = Instantiate<AudioListener>(newPlayerClasses[0].gameObject.GetComponentInChildren<AudioListener>(), newPlayerClasses[0].transform.position, Quaternion.identity);

                //UnityEngine.Object.Destroy(newPlayerClasses[1].GetComponentInChildren<AudioListener>()); // #AudioListener Destruction

                // newPlayerClasses[1].GetComponentInChildren<AudioListener>().enabled = false;

                Debug.Log("The number of displays connected are: " + Display.displays.Length);
                // Display.displays[0] is the primary, default display and is always ON, so start at index 1.
                // Check if additional displays are available and activate each.

                if (ModSettings.useMultipleDisplaysIfPossible)
                {
                    for (int i = 1; i < Display.displays.Length && i < ModSettings.NumberOfPlayers; i++)
                    {
                        Display.displays[i].Activate();
                    }
                }

                TriggerObjectives[] triggerObjectives = FindObjectsOfType<TriggerObjectives>();
                PlayerObjectives[] playerObjectivesArray = FindObjectsOfType<PlayerObjectives>();


                for (int j = 0; j < triggerObjectives.Length; j++)
                {
                    triggerObjectives[j].playerObjectives = playerObjectivesArray[j];
                    triggerObjectives[j].newPlayerClass = newPlayerClasses[triggerObjectives.Length - 1 - j];
                    triggerObjectives[j].detectRoom = newPlayerClasses[triggerObjectives.Length - 1 - j].GetComponentInChildren<DetectRoom>();
                    triggerObjectives[j].player = newPlayerClasses[triggerObjectives.Length - 1 - j];
                    playerObjectivesList.Add(triggerObjectives[j].playerObjectives);
                    Debug.Log("Found triggerObjectives number " + j + ", which uses player number " + (triggerObjectives.Length - 1 - j));
                }


                /* Run this once after this frame? Hook awake to use this instead?
                TriggerObjectives[] triggerObjectives = FindObjectsOfType<TriggerObjectives>();
                for (int j = 0; j < triggerObjectives.Length; j++)
                {
                    //triggerObjectives[j].playerObjectives = playerObjectivesArray[j];
                    triggerObjectives[j].newPlayerClass = newPlayerClasses[triggerObjectives.Length - 1 - j];
                    triggerObjectives[j].detectRoom = newPlayerClasses[triggerObjectives.Length - 1 - j].GetComponentInChildren<DetectRoom>();
                    triggerObjectives[j].player = newPlayerClasses[triggerObjectives.Length - 1 - j];
                    //playerObjectivesList.Add(triggerObjectives[j].playerObjectives);
                    Debug.Log("Found triggerObjectives number " + j + ", which uses player number " + (triggerObjectives.Length - 1 - j));
                }
                */

                /*
                for (int j = 0; j < playerObjectivesArray.Length; j++)
                {
                    Debug.Log("Found triggerObjectives number " + j);
                    playerObjectivesList.Add(playerObjectivesArray[j]);
                }
                */

                Reticule[] reticules = FindObjectsOfType<Reticule>();
                /*
                Reticule[] reticulesUnfiltered = FindObjectsOfType<Reticule>(); // Seem to be found starting with the highest player number.
                Reticule[] reticules = new Reticule[ModSettings.NumberOfPlayers];
                reticules[0] = reticulesUnfiltered[0];
                int reticulesIndex = 1;
                for (int i = 1; i < reticulesUnfiltered.Length; i++){
                    if (reticulesIndex % 2 == 1)
                    {
                        reticules[reticulesIndex] = reticulesUnfiltered[i];
                        reticulesIndex++;
                    }
                    else{
                        //Destroy(reticulesUnfiltered[i]);
                        reticulesUnfiltered[i].enabled = false;
                    }
                }
                */
                // PlayerObjectivesList uses the opposite order. Rect Transform Position starts from bottom of screen.

                Debug.Log("Screen.width = " + Screen.width + ", Screen.height = " + Screen.height + ", Display.displays[0].renderingWidth = " + Display.displays[0].renderingWidth + ", Display.displays[0].renderingHeight = " + Display.displays[0].renderingHeight + ", Display.displays[0].systemWidth = " + Display.displays[0].systemWidth + ", Display.displays[0].systemHeight = " + Display.displays[0].systemHeight);
                switch (ModSettings.NumberOfPlayers)
                {
                    case (2):
                        if (Display.displays.Length == 2 && ModSettings.useMultipleDisplaysIfPossible)
                        {
                            for (int screenNumber = 0; screenNumber < Display.displays.Length; screenNumber++)
                            {
                                foreach (Camera camera in newPlayerClasses[screenNumber].GetComponentsInChildren<Camera>(false))
                                {
                                    camera.targetDisplay = screenNumber;
                                    camera.pixelRect = new Rect(0, 0, Display.displays[screenNumber].renderingWidth, Display.displays[screenNumber].renderingHeight);
                                    Debug.Log("Screen number " + screenNumber + "'s rendering width and height are: " + Display.displays[screenNumber].renderingWidth + "x" + Display.displays[screenNumber].renderingHeight + ".");
                                }
                            }
                            reticules[0].reticuleCanvas.canvas.targetDisplay = 1;
                            playerObjectivesList[1].uiText.canvas.targetDisplay = 1;
                        }
                        else
                        {
                            foreach (Camera camera in newPlayerClasses[0].GetComponentsInChildren<Camera>(false))
                            {
                                camera.pixelRect = new Rect(0, Screen.height / 2, Screen.width, Screen.height / 2);
                            }
                            foreach (Camera camera in newPlayerClasses[1].GetComponentsInChildren<Camera>(false))
                            {
                                camera.pixelRect = new Rect(0, 0, Screen.width, Screen.height / 2);
                            }
                            reticules[1].reticuleCanvas.rectTransform.position += new Vector3(0, reticules[1].reticuleCanvas.rectTransform.position.y / 2, 0);
                            playerObjectivesList[0].uiText.rectTransform.position += new Vector3(0, Screen.height / 2, 0);
                            reticules[0].reticuleCanvas.rectTransform.position -= new Vector3(0, reticules[0].reticuleCanvas.rectTransform.position.y / 2, 0);
                        }
                        break;
                    case (3):
                        if (ModSettings.useMultipleDisplaysIfPossible)
                        {
                            switch (Display.displays.Length)
                            {
                                case (3):
                                    for (int screenNumber = 0; screenNumber < Display.displays.Length; screenNumber++)
                                    {
                                        foreach (Camera camera in newPlayerClasses[screenNumber].GetComponentsInChildren<Camera>(false))
                                        {
                                            camera.targetDisplay = screenNumber;
                                            camera.pixelRect = new Rect(0, 0, Display.displays[screenNumber].renderingWidth, Display.displays[screenNumber].renderingHeight);
                                        }
                                        reticules[screenNumber].reticuleCanvas.canvas.targetDisplay = Display.displays.Length - 1 - screenNumber;
                                        playerObjectivesList[screenNumber].uiText.canvas.targetDisplay = screenNumber;
                                    }
                                    break;
                                case (2):
                                    foreach (Camera camera in newPlayerClasses[0].GetComponentsInChildren<Camera>(false))
                                    {
                                        camera.targetDisplay = 0;
                                    }
                                    foreach (Camera camera in newPlayerClasses[1].GetComponentsInChildren<Camera>(false))
                                    {
                                        camera.targetDisplay = 1;
                                        camera.pixelRect = new Rect(0, Display.displays[1].renderingHeight / 2, Display.displays[1].renderingWidth, Display.displays[1].renderingHeight / 2);
                                    }
                                    foreach (Camera camera in newPlayerClasses[2].GetComponentsInChildren<Camera>(false))
                                    {
                                        camera.targetDisplay = 2;
                                        camera.pixelRect = new Rect(0, 0, Display.displays[1].renderingWidth, Display.displays[1].renderingHeight / 2);
                                    }
                                    reticules[2].reticuleCanvas.canvas.targetDisplay = 0;
                                    reticules[1].reticuleCanvas.canvas.targetDisplay = 1;
                                    reticules[0].reticuleCanvas.canvas.targetDisplay = 1;
                                    reticules[1].reticuleCanvas.rectTransform.position += new Vector3(0, reticules[1].reticuleCanvas.rectTransform.position.y / 2, 0);
                                    playerObjectivesList[1].uiText.rectTransform.position += new Vector3(0, Display.displays[1].renderingHeight / 2, 0);
                                    reticules[0].reticuleCanvas.rectTransform.position -= new Vector3(0, reticules[0].reticuleCanvas.rectTransform.position.y / 2, 0);
                                    break;
                                default:
                                    foreach (Camera camera in newPlayerClasses[0].GetComponentsInChildren<Camera>(false))
                                    {
                                        camera.pixelRect = new Rect(0, 2 * Screen.height / 3, Screen.width, Screen.height / 3);
                                    }
                                    foreach (Camera camera in newPlayerClasses[1].GetComponentsInChildren<Camera>(false))
                                    {
                                        camera.pixelRect = new Rect(0, Screen.height / 3, Screen.width, Screen.height / 3);
                                    }
                                    foreach (Camera camera in newPlayerClasses[2].GetComponentsInChildren<Camera>(false))
                                    {
                                        camera.pixelRect = new Rect(0, 0, Screen.width, Screen.height / 3);
                                    }
                                    reticules[2].reticuleCanvas.rectTransform.position += new Vector3(0, reticules[2].reticuleCanvas.rectTransform.position.y / 3, 0);
                                    playerObjectivesList[0].uiText.rectTransform.position += new Vector3(0, 2 * Screen.height / 3, 0);
                                    reticules[0].reticuleCanvas.rectTransform.position -= new Vector3(0, reticules[0].reticuleCanvas.rectTransform.position.y / 3, 0);
                                    playerObjectivesList[1].uiText.rectTransform.position += new Vector3(0, Screen.height / 3, 0);
                                    break;
                            }
                        }
                        else
                        {
                            foreach (Camera camera in newPlayerClasses[0].GetComponentsInChildren<Camera>(false))
                            {
                                camera.pixelRect = new Rect(0, 2 * Screen.height / 3, Screen.width, Screen.height / 3);
                            }
                            foreach (Camera camera in newPlayerClasses[1].GetComponentsInChildren<Camera>(false))
                            {
                                camera.pixelRect = new Rect(0, Screen.height / 3, Screen.width, Screen.height / 3);
                            }
                            foreach (Camera camera in newPlayerClasses[2].GetComponentsInChildren<Camera>(false))
                            {
                                camera.pixelRect = new Rect(0, 0, Screen.width, Screen.height / 3);
                            }
                            reticules[2].reticuleCanvas.rectTransform.position += new Vector3(0, reticules[2].reticuleCanvas.rectTransform.position.y / 3, 0);
                            playerObjectivesList[0].uiText.rectTransform.position += new Vector3(0, 2 * Screen.height / 3, 0);
                            reticules[0].reticuleCanvas.rectTransform.position -= new Vector3(0, reticules[0].reticuleCanvas.rectTransform.position.y / 3, 0);
                            playerObjectivesList[1].uiText.rectTransform.position += new Vector3(0, Screen.height / 3, 0);
                        }
                        break;
                    case (4):
                        if (ModSettings.useMultipleDisplaysIfPossible)
                        {
                            switch (Display.displays.Length)
                            {
                                case (4):
                                    for (int screenNumber = 0; screenNumber < Display.displays.Length; screenNumber++)
                                    {
                                        foreach (Camera camera in newPlayerClasses[screenNumber].GetComponentsInChildren<Camera>(false))
                                        {
                                            camera.targetDisplay = screenNumber;
                                            camera.pixelRect = new Rect(0, 0, Display.displays[screenNumber].renderingWidth, Display.displays[screenNumber].renderingHeight);
                                        }
                                        reticules[screenNumber].reticuleCanvas.canvas.targetDisplay = Display.displays.Length - 1 - screenNumber;
                                        playerObjectivesList[screenNumber].uiText.canvas.targetDisplay = screenNumber;
                                    }
                                    break;
                                case (3):
                                    for (int screenNumber = 0; screenNumber < Display.displays.Length - 1; screenNumber++)
                                    {
                                        foreach (Camera camera in newPlayerClasses[screenNumber].GetComponentsInChildren<Camera>(false))
                                        {
                                            camera.targetDisplay = screenNumber;
                                            camera.pixelRect = new Rect(0, Display.displays[screenNumber].renderingHeight, Display.displays[screenNumber].renderingWidth, Display.displays[screenNumber].renderingHeight);
                                        }
                                    }
                                    foreach (Camera camera in newPlayerClasses[2].GetComponentsInChildren<Camera>(false))
                                    {
                                        camera.targetDisplay = 2;
                                        camera.pixelRect = new Rect(0, Display.displays[2].renderingHeight / 2, Display.displays[2].renderingWidth, Display.displays[2].renderingHeight / 2);
                                    }
                                    foreach (Camera camera in newPlayerClasses[3].GetComponentsInChildren<Camera>(false))
                                    {
                                        camera.targetDisplay = 2;
                                        camera.pixelRect = new Rect(0, 0, Display.displays[2].renderingWidth, Display.displays[2].renderingHeight / 2);
                                    }
                                    reticules[3].reticuleCanvas.canvas.targetDisplay = 0;
                                    reticules[2].reticuleCanvas.canvas.targetDisplay = 1;
                                    reticules[1].reticuleCanvas.canvas.targetDisplay = 2;
                                    reticules[0].reticuleCanvas.canvas.targetDisplay = 2;
                                    reticules[1].reticuleCanvas.rectTransform.position += new Vector3(0, reticules[1].reticuleCanvas.rectTransform.position.y / 2, 0);
                                    playerObjectivesList[2].uiText.rectTransform.position += new Vector3(0, Display.displays[2].renderingHeight / 2, 0);
                                    reticules[0].reticuleCanvas.rectTransform.position -= new Vector3(0, reticules[0].reticuleCanvas.rectTransform.position.y / 2, 0);
                                    break;
                                case (2):
                                    for (int screenNumber = 0; screenNumber < Display.displays.Length; screenNumber++)
                                    {
                                        foreach (Camera camera in newPlayerClasses[screenNumber].GetComponentsInChildren<Camera>(false))
                                        {
                                            camera.targetDisplay = screenNumber;
                                            camera.pixelRect = new Rect(0, Display.displays[screenNumber].renderingHeight / 2, Display.displays[screenNumber].renderingWidth, Display.displays[screenNumber].renderingHeight / 2);
                                        }
                                        foreach (Camera camera in newPlayerClasses[screenNumber + 1].GetComponentsInChildren<Camera>(false))
                                        {
                                            camera.targetDisplay = screenNumber;
                                            camera.pixelRect = new Rect(0, 0, Display.displays[screenNumber + 1].renderingWidth, Display.displays[screenNumber + 1].renderingHeight / 2);
                                        }
                                    }
                                    reticules[3].reticuleCanvas.canvas.targetDisplay = 0;
                                    reticules[2].reticuleCanvas.canvas.targetDisplay = 0;
                                    reticules[1].reticuleCanvas.canvas.targetDisplay = 1;
                                    reticules[0].reticuleCanvas.canvas.targetDisplay = 1;
                                    reticules[3].reticuleCanvas.rectTransform.position += new Vector3(0, reticules[3].reticuleCanvas.rectTransform.position.y / 2, 0);
                                    playerObjectivesList[0].uiText.rectTransform.position += new Vector3(0, Screen.height / 2, 0);
                                    reticules[2].reticuleCanvas.rectTransform.position -= new Vector3(0, reticules[2].reticuleCanvas.rectTransform.position.y / 2, 0);
                                    reticules[1].reticuleCanvas.rectTransform.position += new Vector3(0, reticules[1].reticuleCanvas.rectTransform.position.y / 2, 0);
                                    playerObjectivesList[2].uiText.rectTransform.position += new Vector3(0, Display.displays[1].renderingHeight / 2, 0);
                                    reticules[0].reticuleCanvas.rectTransform.position -= new Vector3(0, reticules[0].reticuleCanvas.rectTransform.position.y / 2, 0);
                                    break;
                                default:
                                    foreach (Camera camera in newPlayerClasses[0].GetComponentsInChildren<Camera>(false))
                                    {
                                        camera.pixelRect = new Rect(0, Screen.height / 2, Screen.width / 2, Screen.height / 2);
                                    }
                                    foreach (Camera camera in newPlayerClasses[1].GetComponentsInChildren<Camera>(false))
                                    {
                                        camera.pixelRect = new Rect(Screen.width / 2, Screen.height / 2, Screen.width / 2, Screen.height / 2);
                                    }
                                    foreach (Camera camera in newPlayerClasses[2].GetComponentsInChildren<Camera>(false))
                                    {
                                        camera.pixelRect = new Rect(0, 0, Screen.width / 2, Screen.height / 2);
                                    }
                                    foreach (Camera camera in newPlayerClasses[3].GetComponentsInChildren<Camera>(false))
                                    {
                                        camera.pixelRect = new Rect(Screen.width / 2, 0, Screen.width / 2, Screen.height / 2);
                                    }
                                    reticules[3].reticuleCanvas.rectTransform.position += new Vector3(-Screen.width / 4, reticules[3].reticuleCanvas.rectTransform.position.y / 2, 0);
                                    playerObjectivesList[0].uiText.rectTransform.position += new Vector3(-Screen.width / 4, Screen.height / 2, 0);
                                    reticules[2].reticuleCanvas.rectTransform.position += new Vector3(Screen.width / 4, reticules[2].reticuleCanvas.rectTransform.position.y / 2, 0);
                                    playerObjectivesList[1].uiText.rectTransform.position += new Vector3(Screen.width / 4, 0, 0);
                                    reticules[1].reticuleCanvas.rectTransform.position += new Vector3(-Screen.width / 4, -reticules[1].reticuleCanvas.rectTransform.position.y / 2, 0);
                                    playerObjectivesList[2].uiText.rectTransform.position += new Vector3(-Screen.width / 4, Screen.height / 2, 0);
                                    reticules[0].reticuleCanvas.rectTransform.position += new Vector3(Screen.width / 4, -reticules[0].reticuleCanvas.rectTransform.position.y / 2, 0);
                                    playerObjectivesList[3].uiText.rectTransform.position += new Vector3(Screen.width / 4, 0, 0);
                                    break;
                            }
                        }
                        else
                        {
                            foreach (Camera camera in newPlayerClasses[0].GetComponentsInChildren<Camera>(false))
                            {
                                camera.pixelRect = new Rect(0, Screen.height / 2, Screen.width / 2, Screen.height / 2);
                            }
                            foreach (Camera camera in newPlayerClasses[1].GetComponentsInChildren<Camera>(false))
                            {
                                camera.pixelRect = new Rect(Screen.width / 2, Screen.height / 2, Screen.width / 2, Screen.height / 2);
                            }
                            foreach (Camera camera in newPlayerClasses[2].GetComponentsInChildren<Camera>(false))
                            {
                                camera.pixelRect = new Rect(0, 0, Screen.width / 2, Screen.height / 2);
                            }
                            foreach (Camera camera in newPlayerClasses[3].GetComponentsInChildren<Camera>(false))
                            {
                                camera.pixelRect = new Rect(Screen.width / 2, 0, Screen.width / 2, Screen.height / 2);
                            }
                            reticules[3].reticuleCanvas.rectTransform.position += new Vector3(-Screen.width / 4, reticules[3].reticuleCanvas.rectTransform.position.y / 2, 0);
                            playerObjectivesList[0].uiText.rectTransform.position += new Vector3(-Screen.width / 4, Screen.height / 2, 0);
                            reticules[2].reticuleCanvas.rectTransform.position += new Vector3(Screen.width / 4, reticules[2].reticuleCanvas.rectTransform.position.y / 2, 0);
                            playerObjectivesList[1].uiText.rectTransform.position += new Vector3(Screen.width / 4, 0, 0);
                            reticules[1].reticuleCanvas.rectTransform.position += new Vector3(-Screen.width / 4, -reticules[1].reticuleCanvas.rectTransform.position.y / 2, 0);
                            playerObjectivesList[2].uiText.rectTransform.position += new Vector3(-Screen.width / 4, Screen.height / 2, 0);
                            reticules[0].reticuleCanvas.rectTransform.position += new Vector3(Screen.width / 4, -reticules[0].reticuleCanvas.rectTransform.position.y / 2, 0);
                            playerObjectivesList[3].uiText.rectTransform.position += new Vector3(Screen.width / 4, 0, 0);
                        }
                        break;
                }


                if (ModSettings.enableCrewVSMonsterMode)
                {
                    for (int i = 0; i < monsterPlayers.Count; i++)
                    {
                        //PlayerCamera(MultiplayerMode.newPlayerClasses[1]) = References.Monster.GetComponent<Monster>().EyeVision.eyesCamera;

                        /*
                        Camera[] cameras = newPlayerClasses[1].GetComponentsInChildren<Camera>();
                        for (int i = 0; i < cameras.Length; i++)
                        {
                            cameras[i] = References.Monster.GetComponentInChildren<Camera>();
                            Debug.Log("The second player's " + cameras[i].name + " is now the Monster camera.");
                            cameras[i].pixelRect = new Rect(0, 0, Screen.width, Screen.height / 2);
                        }
                        */

                        //References.camLeft = References.Monster.GetComponentInChildren<Camera>();
                        //References.camRight = References.Monster.GetComponentInChildren<Camera>();
                        //References.Monster.GetComponentInChildren<Camera>().pixelRect = new Rect(0, 0, Screen.width, Screen.height / 2);

                        //PlayerCamera(newPlayerClasses[1]) = References.camLeft = References.Monster.GetComponentInChildren<Camera>();
                        //newPlayerClasses[1].GetComponentInChildren<PlayerLayers2>().clipCamera = References.camLeft = References.Monster.GetComponentInChildren<Camera>();
                        monsterPlayers[i].gameObject.transform.localScale = Vector3.zero;
                        /*
                        if (References.Monster.GetComponent<Monster>().MonsterType == Monster.MonsterTypeEnum.Hunter)
                        {
                            PlayerCamera(newPlayerClasses[1]) = References.Monster.GetComponentInChildren<Camera>();
                            //References.camLeft = References.Monster.GetComponentInChildren<Camera>();
                            //References.camRight = References.Monster.GetComponentInChildren<Camera>();
                        }
                        */
                    }
                }

                /*
                foreach (AudioListener audioListener in playerAudioListeners)
                {
                    Debug.Log("Audio listener position is: " + audioListener.transform.position);
                }
                */

                foreach (PlayerUpperBodyLock playerUpperBodyLock in playerUpperBodyLocks)
                {
                    playerUpperBodyLock.UpdatePositions();
                }

                foreach (PlayerUpperBodyLock playerUpperBodyLock in playerUpperBodyLocks)
                {
                    try
                    {
                        Debug.Log("Player upper body lock instance ID is: " + playerUpperBodyLock.GetInstanceID());
                    }
                    catch { }
                }

                Debug.Log("Player Upper Body Locks Information");
                PlayerUpperBodyLock[] publList = UnityEngine.Object.FindObjectsOfType<PlayerUpperBodyLock>();
                foreach (PlayerUpperBodyLock publ in publList)
                {
                    Debug.Log("PUBL ID is " + publ.GetInstanceID());
                }

                Debug.Log("Inventory ID is " + inventories[1].GetInstanceID());

                Debug.Log("Inventories information:");
                Inventory[] inventoriesList = UnityEngine.Object.FindObjectsOfType<Inventory>();
                foreach (Inventory inventory in inventoriesList)
                {
                    Debug.Log("Inventory ID is " + inventory.GetInstanceID());
                }

                Debug.Log("InventoryUIs information:");
                InventoryUI[] inventoryUIsList = UnityEngine.Object.FindObjectsOfType<InventoryUI>();
                foreach (InventoryUI inventoryUI in inventoryUIsList)
                {
                    Debug.Log("InventoryUI ID is " + inventoryUI.GetInstanceID());
                }

                /*
                for (int i = 0; i < newPlayerClasses.Length; i++)
                {
                    foreach (Component component in newPlayerClasses[i].GetComponentsInParent<Component>())
                    {
                        Debug.Log("Parent Component information for player " + i + ": Name is " + component.ToString() + " and ID is " + component.GetInstanceID());
                    }
                    foreach (Component component in newPlayerClasses[i].GetComponents<Component>())
                    {
                        Debug.Log("Component information for player " + i + ": Name is " + component.ToString() + " and ID is " + component.GetInstanceID());
                    }
                    foreach (Component component in newPlayerClasses[i].GetComponentsInChildren<Component>())
                    {
                        Debug.Log("Child Component information for player " + i + ": Name is " + component.ToString() + " and ID is " + component.GetInstanceID());
                    }
                }
                */

                for (int i = 1; i < newPlayerClasses.Count; i++)
                {
                    ShoulderOverride shoulderOverride = newPlayerClasses[i].GetComponentInChildren<ShoulderOverride>();
                    shoulderOverride.player = newPlayerClasses[i];
                }

                if (!useLegacyAudio)
                {
                    SetUpMultiplayerAudio();
                }

                Debug.Log("Crew player numbers are:");
                foreach (NewPlayerClass newPlayerClass in crewPlayers)
                {
                    Debug.Log(PlayerNumber(newPlayerClass.GetInstanceID()));
                }
                Debug.Log("Monster player numbers are:");
                foreach (NewPlayerClass newPlayerClass in monsterPlayers)
                {
                    Debug.Log(PlayerNumber(newPlayerClass.GetInstanceID()));
                }

                UpdatePlayerEffects();

                MouseLock[] mouseLocks = FindObjectsOfType<MouseLock>();
                foreach (MouseLock mouseLock in mouseLocks)
                {
                    if (mouseLock != mouseLockInstance)
                    {
                        Destroy(mouseLock);
                    }
                }

                InventoryIcons[] inventoryIcons = FindObjectsOfType<InventoryIcons>();
                for (int j = 0; j < inventoryIcons.Length; j++)
                {
                    try
                    {
                        if (inventoryIcons[j].inventoryUI == null)
                        {
                            Debug.Log("InventoryUI is null from InventoryIcons");
                        }
                        else if (inventoryIcons[j].inventoryUI.newPlayerClass == null)
                        {
                            Debug.Log("InventoryUI NewPlayerClass is null from InventoryIcons");
                        }
                        int playerNumber = j;// PlayerNumber(foundInventoryIcons.inventoryUI.newPlayerClass.GetInstanceID()); // newPlayerClass is not assigned until Start is called in the original method.
                        Debug.Log("Player number in InventoryIcons.Update Setting Inventory Icons is " + playerNumber);
                        //.rectTransform.position
                        switch (ModSettings.NumberOfPlayers)
                        {
                            case (2):
                                try
                                {
                                    if (Display.displays.Length == 2 && ModSettings.useMultipleDisplaysIfPossible)
                                    {
                                        inventoryIcons[j].itemNameText.canvas.targetDisplay = 1;
                                    }
                                    else
                                    {
                                        if (playerNumber == 1)
                                        {
                                            inventoryIcons[j].itemNameText.rectTransform.position -= new Vector3(0, Screen.height / 2, 0);
                                            foreach (Image image in inventoryIcons[j].spriteIconList)
                                            {
                                                image.rectTransform.position -= new Vector3(0, Screen.height / 2, 0);
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                    Debug.Log("Error while trying to change InventoryIcons case");
                                }
                                break;
                                /*
                                case (3):
                                    if (ModSettings.useMultipleDisplaysIfPossible)
                                    {
                                        switch (Display.displays.Length)
                                        {
                                            case (3):
                                                for (int screenNumber = 0; screenNumber < Display.displays.Length; screenNumber++)
                                                {
                                                    foreach (Camera camera in newPlayerClasses[screenNumber].GetComponentsInChildren<Camera>(false))
                                                    {
                                                        camera.targetDisplay = screenNumber;
                                                        camera.pixelRect = new Rect(0, 0, Display.displays[screenNumber].renderingWidth, Display.displays[screenNumber].renderingHeight);
                                                    }
                                                    reticules[screenNumber].reticuleCanvas.canvas.targetDisplay = Display.displays.Length - 1 - screenNumber;
                                                    playerObjectivesList[screenNumber].uiText.canvas.targetDisplay = screenNumber;
                                                }
                                                break;
                                            case (2):
                                                foreach (Camera camera in newPlayerClasses[0].GetComponentsInChildren<Camera>(false))
                                                {
                                                    camera.targetDisplay = 0;
                                                }
                                                foreach (Camera camera in newPlayerClasses[1].GetComponentsInChildren<Camera>(false))
                                                {
                                                    camera.targetDisplay = 1;
                                                    camera.pixelRect = new Rect(0, Display.displays[1].renderingHeight / 2, Display.displays[1].renderingWidth, Display.displays[1].renderingHeight / 2);
                                                }
                                                foreach (Camera camera in newPlayerClasses[2].GetComponentsInChildren<Camera>(false))
                                                {
                                                    camera.targetDisplay = 2;
                                                    camera.pixelRect = new Rect(0, 0, Display.displays[1].renderingWidth, Display.displays[1].renderingHeight / 2);
                                                }
                                                reticules[2].reticuleCanvas.canvas.targetDisplay = 0;
                                                reticules[1].reticuleCanvas.canvas.targetDisplay = 1;
                                                reticules[0].reticuleCanvas.canvas.targetDisplay = 1;
                                                reticules[1].reticuleCanvas.rectTransform.position += new Vector3(0, reticules[1].reticuleCanvas.rectTransform.position.y / 2, 0);
                                                playerObjectivesList[1].uiText.rectTransform.position += new Vector3(0, Display.displays[1].renderingHeight / 2, 0);
                                                reticules[0].reticuleCanvas.rectTransform.position -= new Vector3(0, reticules[0].reticuleCanvas.rectTransform.position.y / 2, 0);
                                                break;
                                            default:
                                                foreach (Camera camera in newPlayerClasses[0].GetComponentsInChildren<Camera>(false))
                                                {
                                                    camera.pixelRect = new Rect(0, 2 * Screen.height / 3, Screen.width, Screen.height / 3);
                                                }
                                                foreach (Camera camera in newPlayerClasses[1].GetComponentsInChildren<Camera>(false))
                                                {
                                                    camera.pixelRect = new Rect(0, Screen.height / 3, Screen.width, Screen.height / 3);
                                                }
                                                foreach (Camera camera in newPlayerClasses[2].GetComponentsInChildren<Camera>(false))
                                                {
                                                    camera.pixelRect = new Rect(0, 0, Screen.width, Screen.height / 3);
                                                }
                                                reticules[2].reticuleCanvas.rectTransform.position += new Vector3(0, reticules[2].reticuleCanvas.rectTransform.position.y / 3, 0);
                                                playerObjectivesList[0].uiText.rectTransform.position += new Vector3(0, 2 * Screen.height / 3, 0);
                                                reticules[0].reticuleCanvas.rectTransform.position -= new Vector3(0, reticules[0].reticuleCanvas.rectTransform.position.y / 3, 0);
                                                playerObjectivesList[1].uiText.rectTransform.position += new Vector3(0, Screen.height / 3, 0);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        foreach (Camera camera in newPlayerClasses[0].GetComponentsInChildren<Camera>(false))
                                        {
                                            camera.pixelRect = new Rect(0, 2 * Screen.height / 3, Screen.width, Screen.height / 3);
                                        }
                                        foreach (Camera camera in newPlayerClasses[1].GetComponentsInChildren<Camera>(false))
                                        {
                                            camera.pixelRect = new Rect(0, Screen.height / 3, Screen.width, Screen.height / 3);
                                        }
                                        foreach (Camera camera in newPlayerClasses[2].GetComponentsInChildren<Camera>(false))
                                        {
                                            camera.pixelRect = new Rect(0, 0, Screen.width, Screen.height / 3);
                                        }
                                        reticules[2].reticuleCanvas.rectTransform.position += new Vector3(0, reticules[2].reticuleCanvas.rectTransform.position.y / 3, 0);
                                        playerObjectivesList[0].uiText.rectTransform.position += new Vector3(0, 2 * Screen.height / 3, 0);
                                        reticules[0].reticuleCanvas.rectTransform.position -= new Vector3(0, reticules[0].reticuleCanvas.rectTransform.position.y / 3, 0);
                                        playerObjectivesList[1].uiText.rectTransform.position += new Vector3(0, Screen.height / 3, 0);
                                    }
                                    break;
                                case (4):
                                    if (ModSettings.useMultipleDisplaysIfPossible)
                                    {
                                        switch (Display.displays.Length)
                                        {
                                            case (4):
                                                for (int screenNumber = 0; screenNumber < Display.displays.Length; screenNumber++)
                                                {
                                                    foreach (Camera camera in newPlayerClasses[screenNumber].GetComponentsInChildren<Camera>(false))
                                                    {
                                                        camera.targetDisplay = screenNumber;
                                                        camera.pixelRect = new Rect(0, 0, Display.displays[screenNumber].renderingWidth, Display.displays[screenNumber].renderingHeight);
                                                    }
                                                    reticules[screenNumber].reticuleCanvas.canvas.targetDisplay = Display.displays.Length - 1 - screenNumber;
                                                    playerObjectivesList[screenNumber].uiText.canvas.targetDisplay = screenNumber;
                                                }
                                                break;
                                            case (3):
                                                for (int screenNumber = 0; screenNumber < Display.displays.Length - 1; screenNumber++)
                                                {
                                                    foreach (Camera camera in newPlayerClasses[screenNumber].GetComponentsInChildren<Camera>(false))
                                                    {
                                                        camera.targetDisplay = screenNumber;
                                                        camera.pixelRect = new Rect(0, Display.displays[screenNumber].renderingHeight, Display.displays[screenNumber].renderingWidth, Display.displays[screenNumber].renderingHeight);
                                                    }
                                                }
                                                foreach (Camera camera in newPlayerClasses[2].GetComponentsInChildren<Camera>(false))
                                                {
                                                    camera.targetDisplay = 2;
                                                    camera.pixelRect = new Rect(0, Display.displays[2].renderingHeight / 2, Display.displays[2].renderingWidth, Display.displays[2].renderingHeight / 2);
                                                }
                                                foreach (Camera camera in newPlayerClasses[3].GetComponentsInChildren<Camera>(false))
                                                {
                                                    camera.targetDisplay = 2;
                                                    camera.pixelRect = new Rect(0, 0, Display.displays[2].renderingWidth, Display.displays[2].renderingHeight / 2);
                                                }
                                                reticules[3].reticuleCanvas.canvas.targetDisplay = 0;
                                                reticules[2].reticuleCanvas.canvas.targetDisplay = 1;
                                                reticules[1].reticuleCanvas.canvas.targetDisplay = 2;
                                                reticules[0].reticuleCanvas.canvas.targetDisplay = 2;
                                                reticules[1].reticuleCanvas.rectTransform.position += new Vector3(0, reticules[1].reticuleCanvas.rectTransform.position.y / 2, 0);
                                                playerObjectivesList[2].uiText.rectTransform.position += new Vector3(0, Display.displays[2].renderingHeight / 2, 0);
                                                reticules[0].reticuleCanvas.rectTransform.position -= new Vector3(0, reticules[0].reticuleCanvas.rectTransform.position.y / 2, 0);
                                                break;
                                            case (2):
                                                for (int screenNumber = 0; screenNumber < Display.displays.Length; screenNumber++)
                                                {
                                                    foreach (Camera camera in newPlayerClasses[screenNumber].GetComponentsInChildren<Camera>(false))
                                                    {
                                                        camera.targetDisplay = screenNumber;
                                                        camera.pixelRect = new Rect(0, Display.displays[screenNumber].renderingHeight / 2, Display.displays[screenNumber].renderingWidth, Display.displays[screenNumber].renderingHeight / 2);
                                                    }
                                                    foreach (Camera camera in newPlayerClasses[screenNumber + 1].GetComponentsInChildren<Camera>(false))
                                                    {
                                                        camera.targetDisplay = screenNumber;
                                                        camera.pixelRect = new Rect(0, 0, Display.displays[screenNumber + 1].renderingWidth, Display.displays[screenNumber + 1].renderingHeight / 2);
                                                    }
                                                }
                                                reticules[3].reticuleCanvas.canvas.targetDisplay = 0;
                                                reticules[2].reticuleCanvas.canvas.targetDisplay = 0;
                                                reticules[1].reticuleCanvas.canvas.targetDisplay = 1;
                                                reticules[0].reticuleCanvas.canvas.targetDisplay = 1;
                                                reticules[3].reticuleCanvas.rectTransform.position += new Vector3(0, reticules[3].reticuleCanvas.rectTransform.position.y / 2, 0);
                                                playerObjectivesList[0].uiText.rectTransform.position += new Vector3(0, Screen.height / 2, 0);
                                                reticules[2].reticuleCanvas.rectTransform.position -= new Vector3(0, reticules[2].reticuleCanvas.rectTransform.position.y / 2, 0);
                                                reticules[1].reticuleCanvas.rectTransform.position += new Vector3(0, reticules[1].reticuleCanvas.rectTransform.position.y / 2, 0);
                                                playerObjectivesList[2].uiText.rectTransform.position += new Vector3(0, Display.displays[1].renderingHeight / 2, 0);
                                                reticules[0].reticuleCanvas.rectTransform.position -= new Vector3(0, reticules[0].reticuleCanvas.rectTransform.position.y / 2, 0);
                                                break;
                                            default:
                                                foreach (Camera camera in newPlayerClasses[0].GetComponentsInChildren<Camera>(false))
                                                {
                                                    camera.pixelRect = new Rect(0, Screen.height / 2, Screen.width / 2, Screen.height / 2);
                                                }
                                                foreach (Camera camera in newPlayerClasses[1].GetComponentsInChildren<Camera>(false))
                                                {
                                                    camera.pixelRect = new Rect(Screen.width / 2, Screen.height / 2, Screen.width / 2, Screen.height / 2);
                                                }
                                                foreach (Camera camera in newPlayerClasses[2].GetComponentsInChildren<Camera>(false))
                                                {
                                                    camera.pixelRect = new Rect(0, 0, Screen.width / 2, Screen.height / 2);
                                                }
                                                foreach (Camera camera in newPlayerClasses[3].GetComponentsInChildren<Camera>(false))
                                                {
                                                    camera.pixelRect = new Rect(Screen.width / 2, 0, Screen.width / 2, Screen.height / 2);
                                                }
                                                reticules[3].reticuleCanvas.rectTransform.position += new Vector3(-Screen.width / 4, reticules[3].reticuleCanvas.rectTransform.position.y / 2, 0);
                                                playerObjectivesList[0].uiText.rectTransform.position += new Vector3(-Screen.width / 4, Screen.height / 2, 0);
                                                reticules[2].reticuleCanvas.rectTransform.position += new Vector3(Screen.width / 4, reticules[2].reticuleCanvas.rectTransform.position.y / 2, 0);
                                                playerObjectivesList[1].uiText.rectTransform.position += new Vector3(Screen.width / 4, 0, 0);
                                                reticules[1].reticuleCanvas.rectTransform.position += new Vector3(-Screen.width / 4, -reticules[1].reticuleCanvas.rectTransform.position.y / 2, 0);
                                                playerObjectivesList[2].uiText.rectTransform.position += new Vector3(-Screen.width / 4, Screen.height / 2, 0);
                                                reticules[0].reticuleCanvas.rectTransform.position += new Vector3(Screen.width / 4, -reticules[0].reticuleCanvas.rectTransform.position.y / 2, 0);
                                                playerObjectivesList[3].uiText.rectTransform.position += new Vector3(Screen.width / 4, 0, 0);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        foreach (Camera camera in newPlayerClasses[0].GetComponentsInChildren<Camera>(false))
                                        {
                                            camera.pixelRect = new Rect(0, Screen.height / 2, Screen.width / 2, Screen.height / 2);
                                        }
                                        foreach (Camera camera in newPlayerClasses[1].GetComponentsInChildren<Camera>(false))
                                        {
                                            camera.pixelRect = new Rect(Screen.width / 2, Screen.height / 2, Screen.width / 2, Screen.height / 2);
                                        }
                                        foreach (Camera camera in newPlayerClasses[2].GetComponentsInChildren<Camera>(false))
                                        {
                                            camera.pixelRect = new Rect(0, 0, Screen.width / 2, Screen.height / 2);
                                        }
                                        foreach (Camera camera in newPlayerClasses[3].GetComponentsInChildren<Camera>(false))
                                        {
                                            camera.pixelRect = new Rect(Screen.width / 2, 0, Screen.width / 2, Screen.height / 2);
                                        }
                                        reticules[3].reticuleCanvas.rectTransform.position += new Vector3(-Screen.width / 4, reticules[3].reticuleCanvas.rectTransform.position.y / 2, 0);
                                        playerObjectivesList[0].uiText.rectTransform.position += new Vector3(-Screen.width / 4, Screen.height / 2, 0);
                                        reticules[2].reticuleCanvas.rectTransform.position += new Vector3(Screen.width / 4, reticules[2].reticuleCanvas.rectTransform.position.y / 2, 0);
                                        playerObjectivesList[1].uiText.rectTransform.position += new Vector3(Screen.width / 4, 0, 0);
                                        reticules[1].reticuleCanvas.rectTransform.position += new Vector3(-Screen.width / 4, -reticules[1].reticuleCanvas.rectTransform.position.y / 2, 0);
                                        playerObjectivesList[2].uiText.rectTransform.position += new Vector3(-Screen.width / 4, Screen.height / 2, 0);
                                        reticules[0].reticuleCanvas.rectTransform.position += new Vector3(Screen.width / 4, -reticules[0].reticuleCanvas.rectTransform.position.y / 2, 0);
                                        playerObjectivesList[3].uiText.rectTransform.position += new Vector3(Screen.width / 4, 0, 0);
                                    }
                                    break;
                                    */
                        }
                    }
                    catch
                    {
                        Debug.Log("Error while trying to change InventoryIcons");
                    }
                }
            }

            // #InitialiseMultiplayerMode
            public static void InitialiseMultiplayerMode()
            {
                Debug.Log("INITIALISING MULTIPLAYER MODE");
                HookPlayerThings();
                HookMonsterThings();
                Debug.Log("INITIALISED MULTIPLAYER MODE");
            }

            // #HookPlayerThings
            private static void HookPlayerThings()
            {
                //HookAudioLibrary();
                On.BakedOcclusion.LateUpdate += new On.BakedOcclusion.hook_LateUpdate(HookBakedOcclusionLateUpdate);
                On.ContinuousDynamicController.Update += new On.ContinuousDynamicController.hook_Update(HookContinuousDynamicControllerUpdate); // Not sure whether required.
                On.CraneHook.OnHandGrab += new On.CraneHook.hook_OnHandGrab(HookCraneHookOnHandGrab);
                On.Door.MovePlayer += new On.Door.hook_MovePlayer(HookDoorMovePlayer);
                On.DoorPushPlayer.DeterminePushDirection += new On.DoorPushPlayer.hook_DeterminePushDirection(HookDoorPushPlayerDeterminePushDirection);
                On.DraggableObject.OnInteract += new On.DraggableObject.hook_OnInteract(HookDraggableObjectOnInteract);
                On.Drawer.OpenDrawer += new On.Drawer.hook_OpenDrawer(HookDrawerOpenDrawer);
                HookFindInteractiveObject();
                HookFixedInteractable();
                HookHandAnimationController();
                HookHandGrabCondition();
                HookHandGrabIK();
                HookInterpolateToPosition();
                HookInventory();
                HookInventoryItem();
                HookInventoryUI();
                HookItemGrabIK();
                HookItemPosition(); // May be unused by the game.
                On.LockToPosition.Begin += new On.LockToPosition.hook_Begin(HookLockToPositionBegin);
                //On.MonsterHearing.FindHidingSpot += new On.MonsterHearing.hook_FindHidingSpot(HookMonsterHearingFindHidingSpot); // Crashes the game for some reason. Are you modifying the same method somewhere else?
                HookMouseLookCustom();
                HookNewPlayerClass();
                HookPauseMenu();
                HookPlayerAnimationEvents();
                On.PlayerAnimationLayersController.MakeOnlyLayerActive += new On.PlayerAnimationLayersController.hook_MakeOnlyLayerActive(HookPlayerAnimationLayersControllerMakeOnlyLayerActive);
                On.PlayerLayers2.OnWillRenderObject += new On.PlayerLayers2.hook_OnWillRenderObject(HookPlayerLayers2OnWillRenderObject);
                HookPlayerUpperBodyLock();
                HookRealtimeOcclusion();

                HookReverbController();

                HookSteamHandle();


                // 2021 Code
                // Patch 1
                //On.AnimationEvents.Start += new On.AnimationEvents.hook_Start(HookAnimationEventsStart);
                On.FuelCan.OnUseItem += new On.FuelCan.hook_OnUseItem(HookFuelCanOnUseItem);
                //On.FuelPump.DestroyCan += new On.FuelPump.hook_DestroyCan(HookFuelPumpDestroyCan);
                On.HeadLights.OnStartItemAnimation += new On.HeadLights.hook_OnStartItemAnimation(HookHeadLightsOnStartItemAnimation);
                new Hook(typeof(Inventory).GetProperty("AllowShowItem", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetGetMethod(), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.MultiplayerMode).GetMethod("HookInventoryget_AllowShowItem"), null);

                // Patch 2
                On.ItemFlashManager.Update += new On.ItemFlashManager.hook_Update(HookItemFlashManagerUpdate);
                On.MAttackingState2.LerpToPosition += new On.MAttackingState2.hook_LerpToPosition(HookMAttackingState2LerpToPosition);
                //On.MChasingState.Chase += new On.MChasingState.hook_Chase(HookMChasingStateChase); // Moved to ManyMonstersMode
                On.MHuntingState.UpdateAudioZones += new On.MHuntingState.hook_UpdateAudioZones(HookMHuntingStateUpdateAudioZones);
                //On.MonsterStarter.ChooseSpawnPoint += new On.MonsterStarter.hook_ChooseSpawnPoint(HookMonsterStarterChooseSpawnPoint); // Causes crash
                On.MovementControl.ClimbChecking += new On.MovementControl.hook_ClimbChecking(HookMovementControlClimbChecking);
                On.MovementControl.JumpChecking += new On.MovementControl.hook_JumpChecking(HookMovementControlJumpChecking);
                On.MRoomSearch.ChaseChecking += new On.MRoomSearch.hook_ChaseChecking(HookMRoomSearchChaseChecking);
                On.MRoomSearch.FindDeckHidingSpots += new On.MRoomSearch.hook_FindDeckHidingSpots(HookMRoomSearchFindDeckHidingSpots);


                // Patch 3
                On.PlayerMotor.Update += new On.PlayerMotor.hook_Update(HookPlayerMotorUpdate);
                On.SteamVents.Update += new On.SteamVents.hook_Update(HookSteamVentsUpdate);
                On.SteamPushBack.OnTriggerEnter += new On.SteamPushBack.hook_OnTriggerEnter(HookSteamPushBackOnTriggerEnter);

                // Patch 4
                On.PlayerHealth.Start += new On.PlayerHealth.hook_Start(HookPlayerHealthStart);
                On.MAttackingState2.SetLayers += new On.MAttackingState2.hook_SetLayers(HookMAttackingState2SetLayers);
                On.MTrappingState.ChooseStandSpawnDirection += new On.MTrappingState.hook_ChooseStandSpawnDirection(HookMTrappingStateChooseStandSpawnDirection);
                On.MTrappingState.PickRoarType += new On.MTrappingState.hook_PickRoarType(HookMTrappingStatePickRoarType);
                On.MTrappingState.SetUpAmbush += new On.MTrappingState.hook_SetUpAmbush(HookMTrappingStateSetUpAmbush);
                On.DraggedOutHiding.Start += new On.DraggedOutHiding.hook_Start(HookDraggedOutHidingStart);
                On.DragPlayer.Mec_OnGrabPlayer += new On.DragPlayer.hook_Mec_OnGrabPlayer(HookDragPlayerMec_OnGrabPlayer);
                //On.DragPlayer.PullOutPlayer += new On.DragPlayer.hook_PullOutPlayer(HookDragPlayerPullOutPlayer);
                new Hook(typeof(DragPlayer).GetNestedType("<PullOutPlayer>c__Iterator0", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.MultiplayerMode).GetMethod("HookDragPlayerPullOutPlayerIntermediateHook"), null);
                On.MRoomSearch.OnExit += new On.MRoomSearch.hook_OnExit(HookMRoomSearchOnExit);
                //On.MChasingState.OnExit += new On.MChasingState.hook_OnExit(HookMChasingStateOnExit); // Moved to Many Monsters Mode
                On.Hiding.Awake += new On.Hiding.hook_Awake(HookHidingAwake);

                // Patch 5
                On.AnimationControl.ChangeAnimationValues += new On.AnimationControl.hook_ChangeAnimationValues(HookAnimationControlChangeAnimationValues);

                // Patch 6 (Summer 2021)
                HookMovePlayerUnderBed();
                On.SubmarineBattery.OnStartItemAnimation += new On.SubmarineBattery.hook_OnStartItemAnimation(HookSubmarineBatteryOnStartItemAnimation);
                On.Trolley.OnHandGrab += new On.Trolley.hook_OnHandGrab(HookTrolleyOnHandGrab);
                On.Trolley.OnHandRelease += new On.Trolley.hook_OnHandRelease(HookTrolleyOnHandRelease);
                On.Trolley.FixedUpdate += new On.Trolley.hook_FixedUpdate(HookTrolleyFixedUpdate);
                On.Trolley.LateUpdate += new On.Trolley.hook_LateUpdate(HookTrolleyLateUpdate);
                On.TrolleyInteraction.IsConditionMet += new On.TrolleyInteraction.hook_IsConditionMet(HookTrolleyInteractionIsConditionMet);

                On.HeightCondition.IsConditionMet += new On.HeightCondition.hook_IsConditionMet(HookHeightConditionIsConditionMet);
                On.ItemCondition.IsConditionMet += new On.ItemCondition.hook_IsConditionMet(HookItemConditionIsConditionMet);
                On.PhoneButton.IsConditionMet += new On.PhoneButton.hook_IsConditionMet(HookPhoneButtonIsConditionMet);
                On.FuseBox.OnStartFixedAnimation += new On.FuseBox.hook_OnStartFixedAnimation(HookFuseBoxOnStartFixedAnimation);
                On.Fuse.OnFuseStart += new On.Fuse.hook_OnFuseStart(HookFuseOnFuseStart);

                On.DetectRoom.Start += new On.DetectRoom.hook_Start(HookDetectRoomStart);
                On.ConstraintControl.OnStateEnter += new On.ConstraintControl.hook_OnStateEnter(HookConstraintControlOnStateEnter);
                On.ConstraintControl.OnStateExit += new On.ConstraintControl.hook_OnStateExit(HookConstraintControlOnStateExit);
                On.CraneController.OnHandGrab += new On.CraneController.hook_OnHandGrab(HookCraneControllerOnHandGrab);
                On.CraneController.OnHandRelease += new On.CraneController.hook_OnHandRelease(HookCraneControllerOnHandRelease);
                //On.Liferaft.Inflate += new On.Liferaft.hook_Inflate(HookLiferaftInflate);
                new Hook(typeof(Liferaft).GetNestedType("<Inflate>c__Iterator2", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.MultiplayerMode).GetMethod("HookLiferaftInflateIntermediateHook"), null);
                On.CraneSpoolBox.CheckForSpool += new On.CraneSpoolBox.hook_CheckForSpool(HookCraneSpoolBoxCheckForSpool);

                On.Backpack.OnInteract += new On.Backpack.hook_OnInteract(HookBackpackOnInteract);
                On.DraggedOutHiding.LockPlayer += new On.DraggedOutHiding.hook_LockPlayer(HookDraggedOutHidingLockPlayer);
                On.DuctTape.OnFinishItemAnimation += new On.DuctTape.hook_OnFinishItemAnimation(HookDuctTapeOnFinishItemAnimation);
                On.EggTimer.DestroyTimer += new On.EggTimer.hook_DestroyTimer(HookEggTimerDestroyTimer);
                On.EscapeChecker.Update += new On.EscapeChecker.hook_Update(HookEscapeCheckerUpdate);
                On.FuelPump.AddFuel += new On.FuelPump.hook_AddFuel(HookFuelPumpAddFuel);
                On.FuelPump.OnInteract += new On.FuelPump.hook_OnInteract(HookFuelPumpOnInteract);

                On.HelicopterChain.Break += new On.HelicopterChain.hook_Break(HookHelicopterChainBreak);
                On.HelicopterKeys.OnFinishItemAnimation += new On.HelicopterKeys.hook_OnFinishItemAnimation(HookHelicopterKeysOnFinishItemAnimation);
                //On.ItemAnimationListener.Start += new On.ItemAnimationListener.hook_Start(HookItemAnimationListenerStart);

                On.Radio.DestroyRadio += new On.Radio.hook_DestroyRadio(HookRadioDestroyRadio);
                On.RightHandIK.Start += new On.RightHandIK.hook_Start(HookRightHandIKStart);
                On.RightHandIK.OnAnimatorIK += new On.RightHandIK.hook_OnAnimatorIK(HookRightHandIKOnAnimatorIK);
                //On.ShoulderOverride.Awake += new On.ShoulderOverride.hook_Awake(HookShoulderOverrideAwake);
                On.ShoulderOverride.LateUpdate += new On.ShoulderOverride.hook_LateUpdate(HookShoulderOverrideLateUpdate);

                On.AnimationEvents.Start += new On.AnimationEvents.hook_Start(HookAnimationEventsStart);
                On.BodyTurn.Update += new On.BodyTurn.hook_Update(HookBodyTurnUpdate);
                On.AmbushPoint.CanTrapBeSeen += new On.AmbushPoint.hook_CanTrapBeSeen(HookAmbushPointCanTrapBeSeen);
                On.DeactivateSea.Update += new On.DeactivateSea.hook_Update(HookDeactivateSeaUpdate);

                On.FiendDoorSlam.AcquireCorridorDoors += new On.FiendDoorSlam.hook_AcquireCorridorDoors(HookFiendDoorSlamAcquireCorridorDoors);
                On.FiendMindAttack.PlayerEffects += new On.FiendMindAttack.hook_PlayerEffects(HookFiendMindAttackPlayerEffects);
                On.FiendMindAttack.Update += new On.FiendMindAttack.hook_Update(HookFiendMindAttackUpdate);

                On.FlareGun.OnUseItem += new On.FlareGun.hook_OnUseItem(HookFlareGunOnUseItem);
                //On.FootStepManager.Start += new On.FootStepManager.hook_Start(HookFootStepManagerStart);
                On.FootStepManager.SetUpStep += new On.FootStepManager.hook_SetUpStep(HookFootStepManagerSetUpStep);
                //On.FuseBoxDoor.OpenClose += new On.FuseBoxDoor.hook_OpenClose(HookFuseBoxDoorOpenClose);
                new Hook(typeof(FuseBoxDoor).GetNestedType("<OpenClose>c__Iterator0", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.MultiplayerMode).GetMethod("HookFuseBoxDoorOpenCloseIntermediateHook"), null);
                On.HeadIKMovement.CheckAngle += new On.HeadIKMovement.hook_CheckAngle(HookHeadIKMovementCheckAngle);
                On.HeadIKMovement.OnAnimatorIK += new On.HeadIKMovement.hook_OnAnimatorIK(HookHeadIKMovementOnAnimatorIK);
                On.HookCondition.IsConditionMet += new On.HookCondition.hook_IsConditionMet(HookHookConditionIsConditionMet);
                On.ItemModelTransform.SetInventoryTransform += new On.ItemModelTransform.hook_SetInventoryTransform(HookItemModelTransformSetInventoryTransform);
                On.MonsterHidingLeftRightAnimation.GetAnimation += new On.MonsterHidingLeftRightAnimation.hook_GetAnimation(HookMonsterHidingLeftRightAnimationGetAnimation);
                On.PitTrap.DestroyFloor += new On.PitTrap.hook_DestroyFloor(HookPitTrapDestroyFloor);
                On.PitTrap.Update += new On.PitTrap.hook_Update(HookPitTrapUpdate);
                On.TestJump.Update += new On.TestJump.hook_Update(HookTestJumpUpdate);
                On.TestPronePullOut.PullOutPlayer += new On.TestPronePullOut.hook_PullOutPlayer(HookTestPronePullOutPullOutPlayer);
                //new Hook(typeof(TestPronePullOut).GetNestedType("<PullOutPlayer>c__Iterator0", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.MultiplayerMode).GetMethod("HookTestPronePullOutPullOutPlayerIntermediateHook"), null);
                On.Welder.OnFinishItemAnimation += new On.Welder.hook_OnFinishItemAnimation(HookWelderOnFinishItemAnimation);
                HookWallClimb();
                HookAutoStand();
                On.BoltCutters.OnStartItemAnimation += new On.BoltCutters.hook_OnStartItemAnimation(HookBoltCuttersOnStartItemAnimation);
                On.DisableSteamInteractionWhenCrouch.Update += new On.DisableSteamInteractionWhenCrouch.hook_Update(HookDisableSteamInteractionWhenCrouchUpdate);
                On.Phone.OnStartFixedAnimation += new On.Phone.hook_OnStartFixedAnimation(HookPhoneOnStartFixedAnimation);
                On.PlayerFootsteps.Start += new On.PlayerFootsteps.hook_Start(HookPlayerFootstepsStart);
                On.PlayerHeadCollider.Start += new On.PlayerHeadCollider.hook_Start(HookPlayerHeadColliderStart);
                // Notes from today. Pit traps don't catch second player (Exception caused). Flashlight does not move with second player. Valve value has to be updated for all players at once for the correct animation to play. Fuse box cover does not move second player. AutoStand gets null reference exceptions. Phone use is stopped when other player presses interact key. Player 2 can still stand in lockers (no attempt was made to fix this today). // Player 1's inventory has stopped working? Pump does not play animation first time it is used by player 2 and does not stop the animation automatically when the life raft inflation is finished. Egg Timer can cause exception: Coroutine couldn't be started because the the game object 'EggTimer Key Item: (3)' is inactive!

                // Notes from next day. Pit trap and auto stand seem to have been fixed by using parent instead of same level. Inventory bug did not immediately happen again. The phone tracking the interaction button has been removed. Flashlight angle tracking has been added via an ItemAngleUpdater hook. Moving player 1 into locker also causes player 2 to lerp slightly. Make sure to look into the Hunter spawning algorithm again. Moving objects like chairs doesn't seem to work properly. Not using fiends in many monsters mode in a second round when they were used in the first round causes an exception in HookFiendLightControllerOnGenerationComplete. Camera can move down somehow in either multiplayer mode or crew vs monster mode.
                //On.RopeDragRelease.Test += new On.RopeDragRelease.hook_Test(HookRopeDragReleaseTest);
                new Hook(typeof(TestPronePullOut).GetNestedType("<PullOutPlayer>c__Iterator0", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.MultiplayerMode).GetMethod("HookRopeDragReleaseTestIntermediateHook"), null);
                On.ItemAngleUpdater.Update += new On.ItemAngleUpdater.hook_Update(HookItemAngleUpdaterUpdate);
                On.Smashable.Smash += new On.Smashable.hook_Smash(HookSmashableSmash);
                On.SearchState.Update += new On.SearchState.hook_Update(HookSearchStateUpdate);
                On.DetectPlayer.Update += new On.DetectPlayer.hook_Update(HookDetectPlayerUpdate);

                // Next day again. Whether audio plays or not seems to depend mainly on attenuation relating to AudioSource.maxDistance. PlayAtCustomLocation might be useful (AudioSystem hook). Unity secondary display when having two displays may allow for playing audio to different outputs. Steam still only damages player 1.

                //On.DamageScript.Start += new On.DamageScript.hook_Start(HookDamageScriptStart);
                //On.DamageScript.Update += new On.DamageScript.hook_Update(HookDamageScriptUpdate);
                On.DamageScript.OnTriggerEnter += new On.DamageScript.hook_OnTriggerEnter(HookDamageScriptOnTriggerEnter);
                On.Flammable.Update += new On.Flammable.hook_Update(HookFlammableUpdate);
                On.GenericLight.PlayerDistanceCheck += new On.GenericLight.hook_PlayerDistanceCheck(HookGenericLightPlayerDistanceCheck);
                On.GoopBurst.BurstPod += new On.GoopBurst.hook_BurstPod(HookGoopBurstBurstPod);
                On.DualSteamVent.OnStartFixedAnimation += new On.DualSteamVent.hook_OnStartFixedAnimation(HookDualSteamVentOnStartFixedAnimation); // Updates which way the player turns the valve. May be best to leave on References.Player. Updating for each player might fix animation.
                On.MasterControlValve.OnInteract += new On.MasterControlValve.hook_OnInteract(HookMasterControlValveOnInteract);
                On.SteamHandle.Update += new On.SteamHandle.hook_Update(HookSteamHandleUpdate);


                // Still need to implement: Mouse ScrollWheel (Inventory) & RightStickValue (MouseLookCustom & NewPlayerClass)
                // Also need to implement proper Inventory UI and reticule positioning and letting the player control monster features.

                // The flashlight collision check takes other players into account too. It should only care about the player holding the item.
                // Fiend mind attack sound does not stop properly using VAS.
                // Monsters can't break down locked doors in current system.


                On.AutoProne.Update += new On.AutoProne.hook_Update(HookAutoProneUpdate);

                // Inventory text is still only based on the first player and does not move screen. There may only be one instance at the moment.

                // Player is not chased properly in crew vs monsters mode when forcing chase and there is only one monster.

                // When monster starts room search player rigidbody is enabled again as player unhides for a moment?


                if (!useLegacyAudio)
                {
                    HookAudioSystem();

                    On.ReverbZone.CalculateReverbAmount += new On.ReverbZone.hook_CalculateReverbAmount(HookReverbZoneCalculateReverbAmount);
                    HookRoomReverbManager();
                    HookSimpleOcclusion(); // May be sound occlusion.

                    On.AlarmManager.StartPlayingSound += new On.AlarmManager.hook_StartPlayingSound(HookAlarmManagerStartPlayingSound);
                    On.AlarmManager.StopTheAlarm += new On.AlarmManager.hook_StopTheAlarm(HookAlarmManagerStopTheAlarm);
                    On.AudioLogsHighlightedButton.ButtonPressed += new On.AudioLogsHighlightedButton.hook_ButtonPressed(HookAudioLogsHighlightedButtonButtonPressed);
                    On.AudioLogsList.DisableAllLogs += new On.AudioLogsList.hook_DisableAllLogs(HookAudioLogsListDisableAllLogs);
                    On.AudioLogsList.ExitButtonPressed += new On.AudioLogsList.hook_ExitButtonPressed(HookAudioLogsListExitButtonPressed);
                    On.AudioLogsUnHighlightableButton.AudioLogButtonPressed += new On.AudioLogsUnHighlightableButton.hook_AudioLogButtonPressed(HookAudioLogsUnHighlightableButtonAudioLogButtonPressed);
                    On.Crane.OnChainEnd += new On.Crane.hook_OnChainEnd(HookCraneOnChainEnd);
                    On.Crane.OnChainStart += new On.Crane.hook_OnChainStart(HookCraneOnChainStart);
                    On.Crane.OnChainStop += new On.Crane.hook_OnChainStop(HookCraneOnChainStop);
                    On.Crane.OnRotationStop += new On.Crane.hook_OnRotationStop(HookCraneOnRotationStop);
                    On.CreakingSounds.Start += new On.CreakingSounds.hook_Start(HookCreakingSoundsStart);
                    On.CreakingSounds.Update += new On.CreakingSounds.hook_Update(HookCreakingSoundsUpdate);
                    On.DualSteamVent.DisableSteamVent += new On.DualSteamVent.hook_DisableSteamVent(HookDualSteamVentDisableSteamVent);
                    On.EggTimer.Reset += new On.EggTimer.hook_Reset(HookEggTimerReset);
                    On.EggTimer.TimeUp += new On.EggTimer.hook_TimeUp(HookEggTimerTimeUp);
                    On.FiendMindAttack.PlaySound += new On.FiendMindAttack.hook_PlaySound(HookFiendMindAttackPlaySound); // Moved to ManyMonstersMode // Not anymore
                    On.FireExtinguisher.OnDropItem += new On.FireExtinguisher.hook_OnDropItem(HookFireExtinguisherOnDropItem);
                    On.FireExtinguisher.SwitchOffFireExtinguisher += new On.FireExtinguisher.hook_SwitchOffFireExtinguisher(HookFireExtinguisherSwitchOffFireExtinguisher);
                    On.Flammable.OnFireEnd += new On.Flammable.hook_OnFireEnd(HookFlammableOnFireEnd);
                    On.Flashlight.ToggleFlashlight += new On.Flashlight.hook_ToggleFlashlight(HookFlashlightToggleFlashlight);
                    On.FuelPump.PlayHeliAudio += new On.FuelPump.hook_PlayHeliAudio(HookFuelPumpPlayHeliAudio);
                    On.FuseBox.PowerDown += new On.FuseBox.hook_PowerDown(HookFuseBoxPowerDown);
                    On.GlobalMusic.CrossFade += new On.GlobalMusic.hook_CrossFade(HookGlobalMusicCrossFade);
                    On.GlobalMusic.SwitchSources += new On.GlobalMusic.hook_SwitchSources(HookGlobalMusicSwitchSources);
                    On.HelicopterEscape.Update += new On.HelicopterEscape.hook_Update(HookHelicopterEscapeUpdate);
                    On.LightFlicker.Flicker += new On.LightFlicker.hook_Flicker(HookLightFlickerFlicker);
                    //On.MasterControlValve.TurnTheValve += new On.MasterControlValve.hook_TurnTheValve(HookMasterControlValveTurnTheValve);
                    new Hook(typeof(MasterControlValve).GetNestedType("<TurnTheValve>c__Iterator0", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.MultiplayerMode).GetMethod("HookMasterControlValveTurnTheValveIntermediateHook"), null);
                    On.MFiendSubDoors.DoorWasBlocked += new On.MFiendSubDoors.hook_DoorWasBlocked(HookMFiendSubDoorsDoorWasBlocked);
                    On.MonsterDoorSmoke.TurnOffDoorSmoke += new On.MonsterDoorSmoke.hook_TurnOffDoorSmoke(HookMonsterDoorSmokeTurnOffDoorSmoke);
                    On.Phone.EndPhoneCall += new On.Phone.hook_EndPhoneCall(HookPhoneEndPhoneCall);
                    On.Phone.EndReceive += new On.Phone.hook_EndReceive(HookPhoneEndReceive);
                    On.Phone.WaitAndDestroy += new On.Phone.hook_WaitAndDestroy(HookPhoneWaitAndDestroy);
                    On.Radio.TurnRadioOff += new On.Radio.hook_TurnRadioOff(HookRadioTurnRadioOff);
                    On.SecurityCamera.DisableAudio += new On.SecurityCamera.hook_DisableAudio(HookSecurityCameraDisableAudio);
                    On.SecurityCamera.MovementSound += new On.SecurityCamera.hook_MovementSound(HookSecurityCameraMovementSound);
                    On.SecurityCamera.PlayWarning += new On.SecurityCamera.hook_PlayWarning(HookSecurityCameraPlayWarning);
                    On.SecurityCamera.SetToGreen += new On.SecurityCamera.hook_SetToGreen(HookSecurityCameraSetToGreen);
                    On.SecurityCamera.SoundTheAlarm += new On.SecurityCamera.hook_SoundTheAlarm(HookSecurityCameraSoundTheAlarm);
                    On.SecurityMonitors.OnPowerDown += new On.SecurityMonitors.hook_OnPowerDown(HookSecurityMonitorsOnPowerDown);
                    On.SteamVents.TurnOffSteam += new On.SteamVents.hook_TurnOffSteam(HookSteamVentsTurnOffSteam);
                    //On.SubmarineDoor.Opening += new On.SubmarineDoor.hook_Opening(HookSubmarineDoorOpening);
                    new Hook(typeof(SubmarineDoor).GetNestedType("<Opening>c__Iterator0", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.MultiplayerMode).GetMethod("HookSubmarineDoorOpeningIntermediateHook"), null);
                    On.TapeRecorder.StopLog += new On.TapeRecorder.hook_StopLog(HookTapeRecorderStopLog);
                    On.TV.TurnOffSound += new On.TV.hook_TurnOffSound(HookTVTurnOffSound);
                    On.WalkieTalkie.OnEndReceive += new On.WalkieTalkie.hook_OnEndReceive(HookWalkieTalkieOnEndReceive);
                    On.WalkieTalkie.PlaySound += new On.WalkieTalkie.hook_PlaySound(HookWalkieTalkiePlaySound);
                    On.DestroyOnFinish.Update += new On.DestroyOnFinish.hook_Update(HookDestroyOnFinishUpdate);
                }
                //On.MonsterHearing.CalculateDSP += new On.MonsterHearing.hook_CalculateDSP(HookMonsterHearingCalculateDSP); // Also crashes the game.
                //On.MonsterHearing.FindLastHeardPosition += new On.MonsterHearing.hook_FindLastHeardPosition(HookMonsterHearingFindLastHeardPosition);

                //On.AmbienceRaycasting.Start += new On.AmbienceRaycasting.hook_Start(HookAmbienceRaycastingStart);

                //On.AutoPronePlayer.Awake += new On.AutoPronePlayer.hook_Awake(HookAutoPronePlayerAwake);
                //On.AutoPronePlayer.Update += new On.AutoPronePlayer.hook_Update(HookAutoPronePlayerUpdate);
                On.TriggerObjectives.Update += new On.TriggerObjectives.hook_Update(HookTriggerObjectivesUpdate);

                On.TriggerObjectives.SetToNone += new On.TriggerObjectives.hook_SetToNone(HookTriggerObjectivesSetToNone);
                On.TriggerObjectives.SetToThrow += new On.TriggerObjectives.hook_SetToThrow(HookTriggerObjectivesSetToThrow);
                On.TriggerObjectives.TutorialBedText += new On.TriggerObjectives.hook_TutorialBedText(HookTriggerObjectivesTutorialBedText);
                On.TriggerObjectives.TutorialCrouchText += new On.TriggerObjectives.hook_TutorialCrouchText(HookTriggerObjectivesTutorialCrouchText);
                On.TriggerObjectives.TutorialDoorText += new On.TriggerObjectives.hook_TutorialDoorText(HookTriggerObjectivesTutorialDoorText);
                On.TriggerObjectives.TutorialRunText += new On.TriggerObjectives.hook_TutorialRunText(HookTriggerObjectivesTutorialRunText);

                On.TriggerObjectives.Start += new On.TriggerObjectives.hook_Start(HookTriggerObjectivesStart);
                //On.ClimbUpPrompt.OnHoverEnter += new On.ClimbUpPrompt.hook_OnHoverEnter(HookClimbUpPromptOnHoverEnter);
                //On.ClimbUpPrompt.OnHoverExit += new On.ClimbUpPrompt.hook_OnHoverExit(HookClimbUpPromptOnHoverExit);
                On.QualitySettingsScript.SetPlayerEffects += new On.QualitySettingsScript.hook_SetPlayerEffects(HookQualitySettingsScriptSetPlayerEffects);
                On.LightCull2.CalculateEffectiveness += new On.LightCull2.hook_CalculateEffectiveness(HookLightCull2CalculateEffectiveness);
                On.LightCulling.ProcessLights += new On.LightCulling.hook_ProcessLights(HookLightCullingProcessLights);
                On.InventoryIcons.Update += new On.InventoryIcons.hook_Update(HookInventoryIconsUpdate);
                On.InventoryIcons.Start += new On.InventoryIcons.hook_Start(HookInventoryIconsStart);
                On.BillboardPlane.Update += new On.BillboardPlane.hook_Update(HookBillboardPlaneUpdate);
                On.VolumetricSphere.OnEnable += new On.VolumetricSphere.hook_OnEnable(HookVolumetricSphereOnEnable);

                // Picked up objects like chairs float towards player 2. This also caused player 1 not to be able to interact with the helicopter fuelpipeend. This occurs due to DraggableObject.DragPoint only using the last created player or reference player's position. //It was fixed by getting the player's DragMark component from children and using this in DraggableObject.DragPoint.
                // Hiding player meshes are not always disabled. Seems to be the case when the player is holding an item like the flashlight. //This was fixed by hiding the item when going into hiding.

                // Monster can now rip open any locker via the standard method, but desk animations will still not play and grabbing the player does not work yet. //This was fixed by sending different hiding messages.

                // Item rotation follows reference player information.

                // Hide under bed does not work for second player in CVSM mode when first player is monster. Requires input from the first player. //Fixed by using actual player number of crew player, not crew player number.

                // On.AvoidWalls.RaycastAvoidWalls += new On.AvoidWalls.hook_RaycastAvoidWalls(HookAvoidWallsRaycastAvoidWalls);

                // Monster may not run with player distance anymore? (The monster running when too far away from the player)
                // Walkie talkie inventory is not there when checked after dropping. Sometimes the inventory cannot be found in multiplayer. // Fixed by adding some additional code to use in a specific multiplayer case.
                // Player monster cannot climb containers yet. // Fixed by adding code to multiplayer mode MovementControl.
                // Music does not play. Audio source is null? Music does actually play now, but it is extremely quiet.
                // AI Monster may get stuck when searching a hiding spot. Seems to be the MonsterHearing.FindHidingSpot bug.
                // TV audio source does not loop? // Fixed by making source inherit any changes when playing via AudioSystem.
                // Code new monster compass feature. // Added this in now.
                // Skipping menu screen stops options button from working when going back to menu? MP + CVSM

                /*
                On.MouseLock.SubtractFromMenuStack += new On.MouseLock.hook_SubtractFromMenuStack(HookMouseLockSubtractFromMenuStack);
                On.PauseMenu.Show += new On.PauseMenu.hook_Show(HookPauseMenuShow);
                On.DisplayOptions.Hide += new On.DisplayOptions.hook_Hide(HookDisplayOptionsHide);
                On.PauseMenu.Update += new On.PauseMenu.hook_Update(HookPauseMenuUpdate); // ALREADY HOOKED IN BASEFEATURES. DO NOT UNCOMMENT WITHOUT ACCOUNTING FOR THIS.
                */

                // Heli fuel hose handle cannot be grabbed? // Fixed with chair bug.
                // Monster player can teleport through sub event door even when AI is controlling monster. // Seemed to be due to the Fiend teleport through door code being incorrect.
                // Inserted spools do not maintain the correct angle.
                // Monster player must be stopped from simply walking through steam after being stunned once. (Remove immunity?) // Fixed by using noMonsterStunImmunity when player monster controls a monster. Kind of works. Monster can still get through if trying 5 times as they will get slightly closer each timer.
                On.RopeLengthCondition.IsConditionMet += new On.RopeLengthCondition.hook_IsConditionMet(HookRopeLengthConditionIsConditionMet);
                // Monster player cannot choose which or when to destroy a sub door. // Remember that Fiend and Hunter work differently to Brute.

                /*
                I have added in Sub event monster player choice for the Brute and the Fiend and stopped the monsters from opening doors naturally that they should have to do a hiding spot search for. Any player monster will now also have no steam immunity, which makes it impossible to just walk through the helipad steam after being stunned once. Unfortunately it is still possible to walk through if you let yourself get stunned a few times as the monster will still have a tiny amount of immunity period. Now only one player can use the trolley at a time, which stops players from getting locked in place. The last player who used the trolley is still rotated when a player grabs the trolley for some reason.
                */

                // Trolley rotates last player that grabbed trolley. Similar bug has also occurred with different things I think.
                On.Trolley.OnTriggerEnter += new On.Trolley.hook_OnTriggerEnter(HookTrolleyOnTriggerEnter);

                // Stamina mode does not work in multiplayer mode yet. // Changed code to implement this.

                // Hunter ability idea: Press button while outside to make Hunter disappear, gain extreme speed for a moment and then reappear where the player was when the ability ran out or was cancelled.

                // Implement spawn protection and invincibility mode for multiplayer and make sure that the invincibility mode setting if true is not overriden. // This has now been added, but if two different routines change the invincibility mode at the same time it may be switched off earlier than it should be.

                // CHECK THAT EACH IENUMERATOR FUNCTION IS BEING HOOKED CORRECTLY!!!

                // Player motor can get softlocked at phone with Fiend Mind Attack and switched invincibility mode.

                // Fiend ability can create PSEM errors. Maybe due to invincibility mode.

                // Fiend mind attack cooldown audio is still weird. Found with new audio system in CVSMM.

                // Monster can be moved way too much when pulling player out of locker. Should be locked in place. Player is also often moved incorrectly.

                // Sub doors do not all open when player Fiend breaches the room.

                // SimpleOcclusion 8 still has an error.

                // Brute does not charge when Reference Player is prone. Fixed by updating an extra animation value.

                // Steam noises can still sometimes quietly be heard through walls.

                // Pathfinding does not seem to want to work sometimes in CVSMM.

                // May not be able to insert fuse until monster spawned???

                /*
                NullReferenceException: Object reference not set to an instance of an object
                MonstrumExtendedSettingsMod.ExtendedSettingsModScript / MultiplayerMode.HookFuseOnFuseStart(On.Fuse / orig_OnFuseStart, Fuse) < 0x0001c >
                */

                // Monster may not follow DistractionSound properly. // This might have been fixed by passing the original source instead of the VAS to GameplayAudio from AudioSystem.

                // Items you spawn with via settings seem to produce errors when cycling the inventory.

                // Death countdown does not seem to work (tested in CVSMM).

                // Monster door teleportation does not seem to work for non-Fiend monsters.

                // Mid-round monster spawning does not seem to work. New monsters are disabled.

                // Unbreakable pit traps softlocks player.

                // Going near the sub monster spawn point seems to enable all monsters again.


                // See Known Bugs for additional comments and bugs like the above.


                On.TutorialLockerModelSwap.Update += new On.TutorialLockerModelSwap.hook_Update(HookTutorialLockerModelSwapUpdate);
            }

            // #HookMonsterThings
            private static void HookMonsterThings()
            {
                HookMonster();
                On.MSearchingState.FindGoal += new On.MSearchingState.hook_FindGoal(HookMSearchingStateFindGoal);
                HookMWanderState();
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Multiplayer Mode Utility Functions

            public static void ChanceToChooseNewPlayer(Monster monster)
            {
                if (newPlayerClasses != null)
                {
                    float randomChance = UnityEngine.Random.value;
                    if (randomChance > 0.5f)
                    {
                        ChoosePlayerMidRound(monster);
                    }
                }
            }

            public static void ChoosePlayerMidRound(Monster monster)
            {
                monster.player = GetRandomPlayerMidRound(monster.transform.position);
                monster.playerRoomDetect = monster.player.GetComponentInChildren<DetectRoom>();
                monster.hiding = monster.player.GetComponentInChildren<Hiding>();
                monster.anievents.playerHealth = monster.player.GetComponentInChildren<PlayerHealth>();
                Debug.Log("Monster number " + ManyMonstersMode.MonsterNumber(monster.GetInstanceID()) + " now follows player number " + PlayerNumber(monster.player.GetComponent<NewPlayerClass>().GetInstanceID()));
            }

            private static GameObject GetRandomPlayerMidRound(Vector3 monsterPosition)
            {
                if (newPlayerClasses != null)
                {
                    float randomChance = UnityEngine.Random.value;
                    if (randomChance > 0.5f)
                    {
                        return ChooseRandomPlayer();
                    }
                    else
                    {
                        int closestPlayer = ClosestPlayerToThis(monsterPosition, true);
                        return newPlayerClasses[closestPlayer].gameObject;
                    }
                }
                else
                {
                    return References.Player;
                }
            }

            private static GameObject ChooseRandomPlayer()
            {
                if (crewPlayers != null)
                {
                    bool upPlayerFound = false;
                    int randomPlayer = 0;
                    while (!upPlayerFound)
                    {
                        randomPlayer = UnityEngine.Random.Range(0, crewPlayers.Count);
                        if (!playersDowned[randomPlayer])
                        {
                            upPlayerFound = true;
                        }
                    }
                    return crewPlayers[randomPlayer].gameObject;
                }
                else
                {
                    return References.Player;
                }
            }

            public static Camera PlayerCamera(NewPlayerClass newPlayerClass)
            {
                if (newPlayerClass != null)
                {
                    PlayerLayers2 playerLayers2 = newPlayerClass.GetComponentInChildren<PlayerLayers2>();
                    if (playerLayers2 == null)
                    {
                        Debug.Log("PlayerLayers2 is null in PlayerCamera\n" + new StackTrace().ToString());
                        return References.Cam;
                    }
                    else
                    {
                        if (playerLayers2.mainCamera == null)
                        {
                            Debug.Log("PlayerLayers2.mainCamera is null in PlayerCamera\n" + new StackTrace().ToString());
                            return References.Cam;
                        }
                        else
                        {
                            return playerLayers2.mainCamera;
                        }
                    }
                }
                else
                {
                    Debug.Log("NewPlayerClass is null in PlayerCamera\n" + new StackTrace().ToString());
                    return References.Cam;
                }
            }

            public static Inventory PlayerInventory(NewPlayerClass newPlayerClass)
            {
                return inventories[PlayerNumber(newPlayerClass.GetInstanceID())]; // If the player number is found only to get the inventories index, then this method using a NewPlayerClass reference. Otherwise, just reference inventories directly.
            }

            private static int PlayerNumberFromInventory(int passedInventoryInstanceID)
            {
                // Only use this in inventory start as each inventory has a newPlayerClass variable.
                int playerNumber = 0;
                if (inventories != null)
                {
                    for (int i = 1; i < inventories.Count; i++)
                    {
                        if (inventories[i].GetInstanceID() == passedInventoryInstanceID)
                        {
                            playerNumber = i;
                        }
                    }
                }
                return playerNumber;
            }

            public static int ClosestPlayerToThis(Vector3 passedPosition, bool crewOnly = false)
            {
                if (crewOnly == false)
                {
                    int closestPlayer = 0;
                    float closestDistance;

                    if (ModSettings.numbersOfMonsterPlayers.Contains(0) || !playersDowned[0])
                    {
                        closestDistance = Vector3.Distance(newPlayerClasses[0].gameObject.transform.position, passedPosition);
                    }
                    else
                    {
                        closestDistance = float.MaxValue;
                    }

                    for (int i = 1; i < newPlayerClasses.Count; i++)
                    {
                        if ((ModSettings.numbersOfMonsterPlayers.Contains(i) || !playersDowned[crewPlayers.IndexOf(newPlayerClasses[i])]) && Vector3.Distance(newPlayerClasses[i].gameObject.transform.position, passedPosition) < closestDistance)
                        {
                            closestDistance = Vector3.Distance(newPlayerClasses[i].gameObject.transform.position, passedPosition);
                            closestPlayer = i;
                        }
                    }
                    return closestPlayer;
                }
                else
                {
                    return ClosestCrewToThis(passedPosition);
                }
            }

            private static int ClosestCrewToThis(Vector3 passedPosition)
            {
                int closestCrewPlayer = 0;
                float closestDistance;

                if (!playersDowned[0])
                {
                    closestDistance = Vector3.Distance(crewPlayers[0].gameObject.transform.position, passedPosition);
                }
                else
                {
                    closestDistance = float.MaxValue;
                }

                for (int i = 1; i < crewPlayers.Count; i++)
                {
                    if (!playersDowned[i] && Vector3.Distance(crewPlayers[i].gameObject.transform.position, passedPosition) < closestDistance)
                    {
                        closestDistance = Vector3.Distance(crewPlayers[i].gameObject.transform.position, passedPosition);
                        closestCrewPlayer = i;
                    }
                }
                return PlayerNumber(crewPlayers[closestCrewPlayer].GetInstanceID()); // Get the global player number of the crew player.
            }

            private static InventoryItem InventoryItemFromItemClass(GameObject itemGameObject)
            {
                return itemGameObject.GetComponent<InventoryItem>();
            }

            public static Inventory InventoryFromItemClass(GameObject itemGameObject)
            {
                InventoryItem inventoryItem = InventoryItemFromItemClass(itemGameObject);
                return InventoryFromItemClass(inventoryItem);
            }

            public static Inventory InventoryFromItemClass(InventoryItem inventoryItem)
            {
                int inventoryItemInstanceID = inventoryItem.GetInstanceID();
                foreach (Inventory inventory in inventories)
                {
                    foreach (InventorySlot inventorySlot in inventory.inventorySlots)
                    {
                        if (inventorySlot != null && inventorySlot.Item != null && inventorySlot.Item.GetInstanceID() == inventoryItemInstanceID)
                        {
                            return inventory;
                        }
                    }
                }
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Could not find the Inventory of InventoryItem " + inventoryItem.itemName + ". Returning static Inventory.");
                }
                return InventoryItem.inventory;
            }

            public static void FindGameObjectLink(GameObject gameObject, String typeString)
            {
                bool foundLink = false;
                if (gameObject.GetComponentInParent(Type.GetType(typeString)) != null)
                {
                    Debug.Log("Link found in parent of " + gameObject.name + " for type " + typeString);
                    foundLink = true;
                }
                if (gameObject.GetComponent(Type.GetType(typeString)) != null)
                {
                    Debug.Log("Link found on same level of " + gameObject.name + " for type " + typeString);
                    foundLink = true;
                }
                if (gameObject.GetComponentInChildren(Type.GetType(typeString)) != null)
                {
                    Debug.Log("Link found in children of " + gameObject.name + " for type " + typeString);
                    foundLink = true;
                }
                if (!foundLink)
                {
                    Debug.Log("No link found for " + gameObject.name + " for type " + typeString);
                }
            }

            public static void DownPlayer(NewPlayerClass newPlayerClass) // If this is run while the player is in a pit trap the player will not be downed.
            {
                int crewPlayerNumber = crewPlayers.IndexOf(newPlayerClass);
                Debug.Log("Crew player " + crewPlayerNumber + " / player " + PlayerNumber(newPlayerClass.GetInstanceID()) + " was downed!");
                playersDowned[crewPlayerNumber] = true;
                newPlayerClass.stunned = true;
                newPlayerClass.Trapped = true;
                newPlayerClass.playerMotor.Halt();
                newPlayerClass.playerMotor.isHaulted = true;
                newPlayerClass.playerMotor.deathFromHeight = false;
                for (int i = 0; i < ManyMonstersMode.monsterList.Count; i++)
                {
                    if (ManyMonstersMode.monsterListMonsterComponents[i].player == newPlayerClass.gameObject)
                    {
                        //Debug.Log("Monster number " + i + " is choosing a new player as player " + playerNumber + " was downed.");
                        ChoosePlayerMidRound(ManyMonstersMode.monsterListMonsterComponents[i]);
                        ModSettings.ForceStopChase(ManyMonstersMode.monsterListMonsterComponents[i]);
                    }
                }
                ModSettings.ShowTextOnScreen("You have been downed.\nWait for a teammate to throw a smashable near you.", -1f, true, crewPlayerNumber);
            }

            public static void RevivePlayer(NewPlayerClass newPlayerClass)
            {
                int crewPlayerNumber = crewPlayers.IndexOf(newPlayerClass);
                Debug.Log("Crew player " + crewPlayerNumber + " / player " + PlayerNumber(newPlayerClass.GetInstanceID()) + " was revived.");
                playersDowned[crewPlayerNumber] = false;
                newPlayerClass.stunned = false;
                newPlayerClass.Trapped = false;
                newPlayerClass.playerMotor.isHaulted = false;
                newPlayerClass.transform.rotation = new Quaternion(newPlayerClass.transform.rotation.x, newPlayerClass.transform.rotation.y, 0f, newPlayerClass.transform.rotation.w);
                newPlayerClass.gameObject.transform.rotation = new Quaternion(newPlayerClass.gameObject.transform.rotation.x, newPlayerClass.gameObject.transform.rotation.y, 0f, newPlayerClass.gameObject.transform.rotation.w); // Two lines might not be necessary.
                //Camera playerCamera = PlayerCamera(newPlayerClass);
                //playerCamera.transform.rotation = new Quaternion(playerCamera.transform.rotation.x, playerCamera.transform.rotation.y, 0f, playerCamera.transform.rotation.w); // This just seems to skew the camera.
                if (ModSettings.giveAllMonstersASmokeShroud || ModSettings.smokyShip)
                {
                    newPlayerClass.GetComponent<FogDamageTracker>().smokeDamageActive = false; // Switch off the smoke damage of the player, allowing the Smoke Damage coroutine to be started again.
                }
                ModSettings.ShowTextOnScreen(string.Empty, -1f, true, crewPlayerNumber);
            }

            public static bool AllOtherPlayersDown(NewPlayerClass passedPlayer)
            {
                for (int i = 0; i < crewPlayers.Count; i++)
                {
                    if (passedPlayer != crewPlayers[i] && playersDowned[i] == false)
                    {
                        // If the player being checked is not the passed player, the player being checked is not a monster and the player being checked is not downed, return false.
                        return false;
                    }
                }
                // If no player passed the test, return true.
                return true;
            }

            // @Multiplayer Mode Active Features
            public static void MultiplayerModeActiveFeatures()
            {
                /*
                if (!useLegacyAudio && originalAudioSources != null)
                {
                    for (int i = 0; i < originalAudioSources.Count; i++)
                    {
                        if (virtualAudioSources[i] != null && originalAudioSources[i] != null)
                        {
                            // virtualAudioSources[i].time = originalAudioSources[i].time; // This lags the game too much, but is the fix for the Fiend Mind Attack audio issue.
                            if (virtualAudioSources[i].isPlaying && !originalAudioSources[i].isPlaying)
                            {
                                virtualAudioSources[i].Stop();
                            }
                        }
                    }
                }
                */
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AlarmManager

            private static void HookAlarmManagerStartPlayingSound(On.AlarmManager.orig_StartPlayingSound orig, AlarmManager alarmManager, Vector3 _audioTransform, Room _cameraRoom)
            {
                alarmManager.on = true;
                alarmManager.neededString = "Noises/Enviro/Alarm/SecurityCam/Alarmed";
                if (alarmManager.audiosource != null && alarmManager.neededString != alarmManager.currentString)
                {
                    alarmManager.audiosource.transform.position = _audioTransform;
                    alarmManager.audiosource.Stop();
                    VirtualAudioSource virtualAudioSource = alarmManager.audiosource.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                    AudioSystem.PlaySound(alarmManager.neededString, alarmManager.audiosource);
                    alarmManager.currentString = alarmManager.neededString;
                    alarmManager.DSP.transform.position = _cameraRoom.GetComponent<Collider>().bounds.center;
                }
            }

            private static void HookAlarmManagerStopTheAlarm(On.AlarmManager.orig_StopTheAlarm orig, AlarmManager alarmManager)
            {
                alarmManager.currentString = " ";
                if (alarmManager.audiosource != null && alarmManager.neededString != alarmManager.currentString)
                {
                    alarmManager.audiosource.Stop();
                    VirtualAudioSource virtualAudioSource = alarmManager.audiosource.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AmbienceRaycasting

            private static void HookAmbienceRaycastingStart(On.AmbienceRaycasting.orig_Start orig, AmbienceRaycasting ambienceRaycasting)
            {
                orig.Invoke(ambienceRaycasting);
                bool foundLink = false;
                if (ambienceRaycasting.gameObject.GetComponentInParent(typeof(NewPlayerClass)) != null)
                {
                    Debug.Log("Link found in parent of " + ambienceRaycasting.gameObject.name + " for type NewPlayerClass");
                    foundLink = true;
                }
                if (ambienceRaycasting.gameObject.GetComponent(typeof(NewPlayerClass)) != null)
                {
                    Debug.Log("Link found on same level of " + ambienceRaycasting.gameObject.name + " for type NewPlayerClass");
                    foundLink = true;
                }
                if (ambienceRaycasting.gameObject.GetComponentInChildren(typeof(NewPlayerClass)) != null)
                {
                    Debug.Log("Link found in children of " + ambienceRaycasting.gameObject.name + " for type NewPlayerClass");
                    foundLink = true;
                }
                if (!foundLink)
                {
                    Debug.Log("No link found for " + ambienceRaycasting.gameObject.name + " for type NewPlayerClass");
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AmbushPoint

            private static bool HookAmbushPointCanTrapBeSeen(On.AmbushPoint.orig_CanTrapBeSeen orig, AmbushPoint ambushPoint)
            {
                Ray ray = default(Ray);
                foreach (NewPlayerClass crewPlayer in crewPlayers)
                {
                    ray.origin = crewPlayer.transform.position + Vector3.up;
                    AfterSpawn componentInChildren = ((MonoBehaviour)ambushPoint).GetComponentInChildren<AfterSpawn>();
                    Vector3 position;
                    if (componentInChildren != null)
                    {
                        position = componentInChildren.transform.position;
                    }
                    else
                    {
                        position = ((MonoBehaviour)ambushPoint).transform.position;
                    }
                    ray.direction = position - ray.origin;
                    float num = Vector3.Distance(ray.origin, position);
                    Debug.DrawRay(ray.origin, ray.direction * num, Color.red, 3f);
                    RaycastHit raycastHit;
                    if (!Physics.Raycast(ray, out raycastHit, num, ambushPoint.ToPodMask) == true)
                    {
                        if (ManyMonstersMode.logHunterActions)
                        {
                            Debug.Log("Trap at " + ambushPoint.transform.position + " can be seen.");
                        }
                        return true;
                    }
                }
                if (ManyMonstersMode.logHunterActions)
                {
                    Debug.Log("Trap at " + ambushPoint.transform.position + " cannot be seen.");
                }
                return false;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AnimationControl

            private static void HookAnimationControlChangeAnimationValues(On.AnimationControl.orig_ChangeAnimationValues orig, AnimationControl animationControl)
            {
                orig.Invoke(animationControl);
                if (animationControl.monster.MonsterType == Monster.MonsterTypeEnum.Brute)
                {
                    animationControl.monsterAnimation.SetBool("PlayerProne", animationControl.monster.player.GetComponent<NewPlayerClass>().IsProne());


                    if (LevelGeneration.Instance.finishedGenerating && ModSettings.enableCrewVSMonsterMode)
                    {
                        int monsterNumber = ManyMonstersMode.MonsterNumber(animationControl.monster.GetInstanceID());
                        if (animationControl.monster.MonsterType == Monster.MonsterTypeEnum.Brute && monsterNumber < MultiplayerMode.monsterPlayers.Count)
                        {
                            animationControl.monsterAnimation.SetFloat("RunType", 0f);
                            if (CrewVsMonsterMode.monsterUsingActiveAbility[monsterNumber])
                            {
                                animationControl.monsterAnimation.SetFloat("Distance", 4f);
                                animationControl.monsterAnimation.SetBool("CanSeePlayer", true);
                                animationControl.monsterAnimation.SetBool("InCorridor", true);
                                animationControl.monsterAnimation.SetBool("InARoom", false);
                                animationControl.monsterAnimation.SetBool("PlayerProne", false);
                                // animationControl.monsterAnimation.SetFloat("RunType", 1f); // Forces the Brute to walk instead of running.
                                //animationControl.chargeDistance = 4f;
                                //animationControl.canSeePlayer = true;
                                //animationControl.runSpeed = float.MaxValue;
                                //animationControl.monsterAnimation.SetFloat("RunSpeed", animationControl.runSpeed);
                                /*
                                Debug.Log("Current animator layer count is: " + animationControl.monsterAnimation.layerCount);
                                for (int i = 0; i < animationControl.monsterAnimation.layerCount;i++)
                                {
                                    try
                                    {
                                        Debug.Log("Layer number " + i + " is called " + animationControl.monsterAnimation.GetLayerName(i) + ".");
                                    }
                                    catch
                                    {
                                        Debug.Log("Could not get information on layer number " + i + ".");
                                    }
                                }
                                */

                                /*
                                RuntimeAnimatorController[] rtacs = FindObjectsOfType<RuntimeAnimatorController>();
                                foreach (RuntimeAnimatorController rtac in rtacs)
                                {
                                    Debug.Log(rtac.animationClips);
                                }
                                */
                                //References.Monster.GetComponent<Monster>().player = MultiplayerMode.monsterPlayers[0].gameObject;
                            }
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AnimationEvents

            private static void HookAnimationEventsStart(On.AnimationEvents.orig_Start orig, AnimationEvents animationEvents)
            {
                orig.Invoke(animationEvents);
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    animationEvents.playerHealth = animationEvents.monster.player.GetComponentInChildren<PlayerHealth>();
                    Debug.Log("Player health assigned successfully to animation events.");
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AudioLibrary

            private static void AudioLibraryOnSoundPlayed(AudioLibrary audioLibrary, VirtualAudioSource virtualAudioSource)
            {
                if (virtualAudioSource.originalAudioSource != null && virtualAudioSource.mySource != null)
                {
                    SimpleOcclusion simpleOcclusion = virtualAudioSource.originalAudioSource.gameObject.GetComponent<SimpleOcclusion>();
                    AudioLibrary.AudioOcclusionType audioOcclusionType = audioLibrary.occlusionType;
                    if (audioOcclusionType != AudioLibrary.AudioOcclusionType.None)
                    {
                        if (audioOcclusionType == AudioLibrary.AudioOcclusionType.Simple)
                        {
                            if (simpleOcclusion == null)
                            {
                                simpleOcclusion = (virtualAudioSource.originalAudioSource.gameObject.AddComponent(typeof(SimpleOcclusion)) as SimpleOcclusion);
                            }
                            simpleOcclusion.source = virtualAudioSource.originalAudioSource;
                        }
                    }
                    else
                    {
                        GlobalMusic component = virtualAudioSource.originalAudioSource.gameObject.GetComponent<GlobalMusic>();
                        if (component != null)
                        {
                            component.lib = audioLibrary;
                        }
                        if (simpleOcclusion != null)
                        {
                            UnityEngine.Object.Destroy(simpleOcclusion);
                        }
                    }
                    VolumeController volumeController = virtualAudioSource.originalAudioSource.GetComponent<VolumeController>();
                    if (volumeController == null)
                    {
                        volumeController = (virtualAudioSource.originalAudioSource.gameObject.AddComponent(typeof(VolumeController)) as VolumeController);
                    }
                    if (virtualAudioSource.originalAudioSource.gameObject.GetComponent<SimpleOcclusion>())
                    {
                        volumeController.occlusion = 0f;
                    }
                    else
                    {
                        volumeController.Occlusion = 1f;
                    }
                    volumeController.SetSource(virtualAudioSource.mySource);
                    volumeController.SetLibrary(audioLibrary);
                    volumeController.StartVolume = audioLibrary.startVolume + UnityEngine.Random.Range(-audioLibrary.volumeRandomness, audioLibrary.volumeRandomness);
                    volumeController.CalculateVolume(virtualAudioSource.mySource, volumeController.fadeValue);
                    virtualAudioSource.mySource.pitch = audioLibrary.startPitch + UnityEngine.Random.Range(-audioLibrary.pitchRandomness, audioLibrary.pitchRandomness);
                    //Debug.Log("Printing AudioLibrary.OnSoundPlayed information: Original source volume: " + virtualAudioSource.originalAudioSource.volume + ". Virtual Audio Source volume: " + virtualAudioSource.mySource.volume + ". ORIGINAL AUDIO SOURCE: Current Duck Value is " + Ducking.GetCurrentDuckValue(volumeController.lib.duckTag) + ", Occlusion is " + volumeController.occlusion + ", Game Volume is " + volumeController.gameVolume + ", Start Volume is " + volumeController.startVolume + ", Fade Value is " + volumeController.fadeValue + ", Game End Fade is " + volumeController.gameEndFade + " and library type check gives " + ((volumeController.lib.type != AudioLibrary.SFXType.Music) ? AudioSystem.SFXVolume : AudioSystem.MusicVolume));
                }

                /*
                if (virtualAudioSource.originalAudioSource != null && virtualAudioSource.mySource != null)
                {
                    SimpleOcclusion simpleOcclusion = virtualAudioSource.mySource.gameObject.GetComponent<SimpleOcclusion>();
                    AudioLibrary.AudioOcclusionType audioOcclusionType = audioLibrary.occlusionType;
                    if (audioOcclusionType != AudioLibrary.AudioOcclusionType.None)
                    {
                        if (audioOcclusionType == AudioLibrary.AudioOcclusionType.Simple)
                        {
                            if (simpleOcclusion == null)
                            {
                                simpleOcclusion = (virtualAudioSource.mySource.gameObject.AddComponent(typeof(SimpleOcclusion)) as SimpleOcclusion);
                            }
                            simpleOcclusion.source = virtualAudioSource.originalAudioSource;
                        }
                    }
                    else
                    {
                        GlobalMusic component = virtualAudioSource.originalAudioSource.gameObject.GetComponent<GlobalMusic>();
                        if (component != null)
                        {
                            component.lib = audioLibrary;
                        }
                        if (simpleOcclusion != null)
                        {
                            UnityEngine.Object.Destroy(simpleOcclusion);
                        }
                    }
                    VolumeController volumeController = virtualAudioSource.mySource.GetComponent<VolumeController>();
                    if (volumeController == null)
                    {
                        volumeController = (virtualAudioSource.mySource.gameObject.AddComponent(typeof(VolumeController)) as VolumeController);
                    }
                    if (virtualAudioSource.mySource.gameObject.GetComponent<SimpleOcclusion>())
                    {
                        volumeController.occlusion = 0f;
                    }
                    else
                    {
                        volumeController.Occlusion = 1f;
                    }
                    volumeController.SetSource(virtualAudioSource.originalAudioSource);
                    volumeController.SetLibrary(audioLibrary);
                    volumeController.StartVolume = audioLibrary.startVolume + UnityEngine.Random.Range(-audioLibrary.volumeRandomness, audioLibrary.volumeRandomness);
                    volumeController.CalculateVolume(virtualAudioSource.originalAudioSource, volumeController.fadeValue);
                    virtualAudioSource.mySource.pitch = audioLibrary.startPitch + UnityEngine.Random.Range(-audioLibrary.pitchRandomness, audioLibrary.pitchRandomness);
                }
                */

                /*
                if (virtualAudioSource.originalAudioSource != null && virtualAudioSource.mySource != null)
                {
                    SimpleOcclusion simpleOcclusion = virtualAudioSource.mySource.gameObject.GetComponent<SimpleOcclusion>();
                    AudioLibrary.AudioOcclusionType audioOcclusionType = audioLibrary.occlusionType;
                    if (audioOcclusionType != AudioLibrary.AudioOcclusionType.None)
                    {
                        if (audioOcclusionType == AudioLibrary.AudioOcclusionType.Simple)
                        {
                            if (simpleOcclusion == null)
                            {
                                simpleOcclusion = (virtualAudioSource.mySource.gameObject.AddComponent(typeof(SimpleOcclusion)) as SimpleOcclusion);
                            }
                            simpleOcclusion.source = virtualAudioSource.originalAudioSource;
                        }
                    }
                    else
                    {
                        GlobalMusic component = virtualAudioSource.originalAudioSource.gameObject.GetComponent<GlobalMusic>();
                        if (component != null)
                        {
                            component.lib = audioLibrary;
                        }
                        if (simpleOcclusion != null)
                        {
                            UnityEngine.Object.Destroy(simpleOcclusion);
                        }
                    }
                    VolumeController volumeController = virtualAudioSource.mySource.GetComponent<VolumeController>();
                    if (volumeController == null)
                    {
                        volumeController = (virtualAudioSource.mySource.gameObject.AddComponent(typeof(VolumeController)) as VolumeController);
                    }
                    if (virtualAudioSource.mySource.gameObject.GetComponent<SimpleOcclusion>())
                    {
                        volumeController.occlusion = 0f;
                    }
                    else
                    {
                        volumeController.Occlusion = 1f;
                    }
                    volumeController.SetSource(virtualAudioSource.originalAudioSource);
                    volumeController.SetLibrary(audioLibrary);
                    volumeController.StartVolume = audioLibrary.startVolume + UnityEngine.Random.Range(-audioLibrary.volumeRandomness, audioLibrary.volumeRandomness);
                    volumeController.CalculateVolume(virtualAudioSource.mySource, volumeController.fadeValue);
                    virtualAudioSource.mySource.pitch = audioLibrary.startPitch + UnityEngine.Random.Range(-audioLibrary.pitchRandomness, audioLibrary.pitchRandomness);
                }
                */

                /*
                if (virtualAudioSource.originalAudioSource != null && virtualAudioSource.mySource != null)
                {
                    SimpleOcclusion simpleOcclusion = virtualAudioSource.originalAudioSource.gameObject.GetComponent<SimpleOcclusion>();
                    SimpleOcclusion simpleOcclusionFromVAS = virtualAudioSource.mySource.gameObject.GetComponent<SimpleOcclusion>();
                    AudioLibrary.AudioOcclusionType audioOcclusionType = audioLibrary.occlusionType;
                    if (audioOcclusionType != AudioLibrary.AudioOcclusionType.None)
                    {
                        if (audioOcclusionType == AudioLibrary.AudioOcclusionType.Simple)
                        {
                            if (simpleOcclusion == null)
                            {
                                simpleOcclusion = (virtualAudioSource.originalAudioSource.gameObject.AddComponent(typeof(SimpleOcclusion)) as SimpleOcclusion);
                            }
                            if (simpleOcclusionFromVAS == null)
                            {
                                simpleOcclusionFromVAS = (virtualAudioSource.mySource.gameObject.AddComponent(typeof(SimpleOcclusion)) as SimpleOcclusion);
                            }
                        }
                    }
                    else
                    {
                        GlobalMusic globalMusic = virtualAudioSource.originalAudioSource.gameObject.GetComponent<GlobalMusic>();
                        GlobalMusic globalMusicFromVAS = virtualAudioSource.mySource.gameObject.GetComponent<GlobalMusic>();
                        if (globalMusic != null)
                        {
                            globalMusic.lib = audioLibrary;
                        }
                        if (globalMusicFromVAS != null)
                        {
                            globalMusicFromVAS.lib = audioLibrary;
                        }
                        if (simpleOcclusion != null)
                        {
                            UnityEngine.Object.Destroy(simpleOcclusion);
                        }
                        if (simpleOcclusionFromVAS != null)
                        {
                            UnityEngine.Object.Destroy(simpleOcclusionFromVAS);
                        }
                    }
                    VolumeController volumeController = virtualAudioSource.originalAudioSource.GetComponent<VolumeController>();
                    VolumeController volumeControllerFromVAS = virtualAudioSource.mySource.GetComponent<VolumeController>();
                    if (volumeController == null)
                    {
                        volumeController = (virtualAudioSource.originalAudioSource.gameObject.AddComponent(typeof(VolumeController)) as VolumeController);
                    }
                    if (volumeControllerFromVAS == null)
                    {
                        volumeControllerFromVAS = (virtualAudioSource.mySource.gameObject.AddComponent(typeof(VolumeController)) as VolumeController);
                    }
                    if (virtualAudioSource.originalAudioSource.gameObject.GetComponent<SimpleOcclusion>())
                    {
                        volumeController.occlusion = 0f;
                    }
                    else
                    {
                        volumeController.Occlusion = 1f;
                    }
                    if (virtualAudioSource.mySource.gameObject.GetComponent<SimpleOcclusion>())
                    {
                        volumeControllerFromVAS.occlusion = 0f;
                    }
                    else
                    {
                        volumeControllerFromVAS.Occlusion = 1f;
                    }

                    simpleOcclusionFromVAS.source = virtualAudioSource.originalAudioSource;
                    volumeController.SetSource(virtualAudioSource.originalAudioSource);
                    volumeControllerFromVAS.SetSource(virtualAudioSource.originalAudioSource);
                    volumeController.SetLibrary(audioLibrary);
                    volumeControllerFromVAS.SetLibrary(audioLibrary);
                    float startVolume = audioLibrary.startVolume + UnityEngine.Random.Range(-audioLibrary.volumeRandomness, audioLibrary.volumeRandomness);
                    volumeController.StartVolume = startVolume;
                    volumeControllerFromVAS.StartVolume = startVolume;
                    volumeController.CalculateVolume(virtualAudioSource.originalAudioSource, volumeController.fadeValue);
                    volumeControllerFromVAS.CalculateVolume(virtualAudioSource.mySource, volumeControllerFromVAS.fadeValue);
                    float pitch = audioLibrary.startPitch + UnityEngine.Random.Range(-audioLibrary.pitchRandomness, audioLibrary.pitchRandomness);
                    virtualAudioSource.originalAudioSource.pitch = pitch;
                    virtualAudioSource.mySource.pitch = pitch;

                    Debug.Log("Printing AudioLibrary.OnSoundPlayed information: Original source volume: " + virtualAudioSource.originalAudioSource.volume + ". Virtual Audio Source volume: " + virtualAudioSource.mySource.volume + ". ORIGINAL AUDIO SOURCE: Current Duck Value is " + Ducking.GetCurrentDuckValue(volumeController.lib.duckTag) + ", Occlusion is " + volumeController.occlusion + ", Game Volume is " + volumeController.gameVolume + ", Start Volume is " + volumeController.startVolume + ", Fade Value is " + volumeController.fadeValue + ", Game End Fade is " + volumeController.gameEndFade + " and library type check gives " + ((volumeController.lib.type != AudioLibrary.SFXType.Music) ? AudioSystem.SFXVolume : AudioSystem.MusicVolume) + ". VIRTUAL AUDIO SOURCE: Current Duck Value is " + Ducking.GetCurrentDuckValue(volumeControllerFromVAS.lib.duckTag) + ", Occlusion is " + volumeControllerFromVAS.occlusion + ", Game Volume is " + volumeControllerFromVAS.gameVolume + ", Start Volume is " + volumeControllerFromVAS.startVolume + ", Fade Value is " + volumeControllerFromVAS.fadeValue + ", Game End Fade is " + volumeControllerFromVAS.gameEndFade + " and library type check gives " + ((volumeControllerFromVAS.lib.type != AudioLibrary.SFXType.Music) ? AudioSystem.SFXVolume : AudioSystem.MusicVolume));

                    //simpleOcclusionFromVAS = simpleOcclusion;
                    //volumeControllerFromVAS = volumeController;

                    // Copy more from original source. No errors, but also no volume.
                }
                else
                {
                    Debug.Log("Failed normal AudioLibrary.OnSoundPlayed");
                    AudioSource _source;
                    if (virtualAudioSource.mySource != null)
                    {
                        _source = virtualAudioSource.mySource;
                    }
                    else if (virtualAudioSource.originalAudioSource != null)
                    {
                        _source = virtualAudioSource.originalAudioSource;
                    }
                    else
                    {
                        return;
                    }
                    SimpleOcclusion simpleOcclusion = _source.gameObject.GetComponent<SimpleOcclusion>();
                    AudioLibrary.AudioOcclusionType audioOcclusionType = audioLibrary.occlusionType;
                    if (audioOcclusionType != AudioLibrary.AudioOcclusionType.None)
                    {
                        if (audioOcclusionType == AudioLibrary.AudioOcclusionType.Simple)
                        {
                            if (simpleOcclusion == null)
                            {
                                simpleOcclusion = (_source.gameObject.AddComponent(typeof(SimpleOcclusion)) as SimpleOcclusion);
                            }
                        }
                    }
                    else
                    {
                        GlobalMusic component = _source.gameObject.GetComponent<GlobalMusic>();
                        if (component != null)
                        {
                            component.lib = audioLibrary;
                        }
                        if (simpleOcclusion != null)
                        {
                            UnityEngine.Object.Destroy(simpleOcclusion);
                        }
                    }
                    VolumeController volumeController = _source.GetComponent<VolumeController>();
                    if (volumeController == null)
                    {
                        volumeController = (_source.gameObject.AddComponent(typeof(VolumeController)) as VolumeController);
                    }
                    if (_source.gameObject.GetComponent<SimpleOcclusion>())
                    {
                        volumeController.occlusion = 0f;
                    }
                    else
                    {
                        volumeController.Occlusion = 1f;
                    }
                    volumeController.SetSource(_source);
                    volumeController.SetLibrary(audioLibrary);
                    volumeController.StartVolume = audioLibrary.startVolume + UnityEngine.Random.Range(-audioLibrary.volumeRandomness, audioLibrary.volumeRandomness);
                    volumeController.CalculateVolume(_source, volumeController.fadeValue);
                    _source.pitch = audioLibrary.startPitch + UnityEngine.Random.Range(-audioLibrary.pitchRandomness, audioLibrary.pitchRandomness);
                }
                */
            }

            private static void HookAudioLibrary()
            {
                On.AudioLibrary.OnSoundPlayed += new On.AudioLibrary.hook_OnSoundPlayed(HookAudioLibraryOnSoundPlayed);
            }

            private static void HookAudioLibraryOnSoundPlayed(On.AudioLibrary.orig_OnSoundPlayed orig, AudioLibrary audioLibrary, AudioSource _source)
            {
                SimpleOcclusion simpleOcclusion = _source.gameObject.GetComponent<SimpleOcclusion>();
                AudioLibrary.AudioOcclusionType audioOcclusionType = audioLibrary.occlusionType;
                if (audioOcclusionType != AudioLibrary.AudioOcclusionType.None)
                {
                    if (audioOcclusionType == AudioLibrary.AudioOcclusionType.Simple)
                    {
                        if (simpleOcclusion == null)
                        {
                            simpleOcclusion = (_source.gameObject.AddComponent(typeof(SimpleOcclusion)) as SimpleOcclusion);
                        }
                    }
                }
                else
                {
                    GlobalMusic component = _source.gameObject.GetComponent<GlobalMusic>();
                    if (component != null)
                    {
                        component.lib = audioLibrary;
                    }
                    if (simpleOcclusion != null)
                    {
                        UnityEngine.Object.Destroy(simpleOcclusion);
                    }
                }
                VolumeController volumeController = _source.GetComponent<VolumeController>();
                if (volumeController == null)
                {
                    volumeController = (_source.gameObject.AddComponent(typeof(VolumeController)) as VolumeController);
                }
                if (_source.gameObject.GetComponent<SimpleOcclusion>())
                {
                    volumeController.occlusion = 0f;
                }
                else
                {
                    volumeController.Occlusion = 1f;
                }
                volumeController.SetSource(_source);
                volumeController.SetLibrary(audioLibrary);
                volumeController.StartVolume = audioLibrary.startVolume + UnityEngine.Random.Range(-audioLibrary.volumeRandomness, audioLibrary.volumeRandomness);
                Debug.Log("The source instance ID at audio library is " + _source.GetInstanceID() + " and volume is " + volumeController.CalculateVolume(_source, volumeController.fadeValue));
                _source.pitch = audioLibrary.startPitch + UnityEngine.Random.Range(-audioLibrary.pitchRandomness, audioLibrary.pitchRandomness);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AudioLogsHighlightedButton

            private static void HookAudioLogsHighlightedButtonButtonPressed(On.AudioLogsHighlightedButton.orig_ButtonPressed orig, AudioLogsHighlightedButton audioLogsHighlightedButton)
            {
                orig.Invoke(audioLogsHighlightedButton);
                VirtualAudioSource virtualAudioSource = audioLogsHighlightedButton.audioLogsList.audioSource.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    virtualAudioSource.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AudioLogsUnHighlightableButton

            private static void HookAudioLogsUnHighlightableButtonAudioLogButtonPressed(On.AudioLogsUnHighlightableButton.orig_AudioLogButtonPressed orig, AudioLogsUnHighlightableButton audioLogsUnHighlightableButton)
            {
                if (audioLogsUnHighlightableButton.isPressedAllowed)
                {
                    audioLogsUnHighlightableButton.timer.SetTime(0f);
                    if (audioLogsUnHighlightableButton.logID >= 0)
                    {
                        if (audioLogsUnHighlightableButton.audioLogsList.audioSource != null)
                        {
                            if (audioLogsUnHighlightableButton.highlightSprite.color.r < 0.4f && audioLogsUnHighlightableButton.audioLogsList.audioSource.isPlaying)
                            {
                                audioLogsUnHighlightableButton.audioLogsList.audioSource.Stop();
                                VirtualAudioSource virtualAudioSource = audioLogsUnHighlightableButton.audioLogsList.audioSource.gameObject.GetComponent<VirtualAudioSource>();
                                if (virtualAudioSource != null)
                                {
                                    virtualAudioSource.Stop();
                                }
                                else if (ModSettings.logDebugText)
                                {
                                    Debug.Log("VAS is null 1!\n" + new StackTrace().ToString());
                                }
                                audioLogsUnHighlightableButton.SetToWhite();
                            }
                            else
                            {
                                audioLogsUnHighlightableButton.audioLogsList.audioSource.Stop();
                                VirtualAudioSource virtualAudioSource = audioLogsUnHighlightableButton.audioLogsList.audioSource.gameObject.GetComponent<VirtualAudioSource>();
                                if (virtualAudioSource != null)
                                {
                                    virtualAudioSource.Stop();
                                }
                                else if (ModSettings.logDebugText)
                                {
                                    Debug.Log("VAS is null 2!\n" + new StackTrace().ToString());
                                }
                                audioLogsUnHighlightableButton.audioLogsList.tapeRecorder.PlayTape(audioLogsUnHighlightableButton.audioLogsList.tapeRecorder.GetTapeFromLogID(audioLogsUnHighlightableButton.logID), audioLogsUnHighlightableButton.audioLogsList.audioSource);
                                audioLogsUnHighlightableButton.SetToGreen();
                            }
                        }
                        else
                        {
                            audioLogsUnHighlightableButton.audioLogsList.audioSource.Stop();
                            VirtualAudioSource virtualAudioSource = audioLogsUnHighlightableButton.audioLogsList.audioSource.gameObject.GetComponent<VirtualAudioSource>();
                            if (virtualAudioSource != null)
                            {
                                virtualAudioSource.Stop();
                            }
                            else if (ModSettings.logDebugText)
                            {
                                Debug.Log("VAS is null 3!\n" + new StackTrace().ToString());
                            }
                            audioLogsUnHighlightableButton.audioLogsList.tapeRecorder.PlayTape(audioLogsUnHighlightableButton.audioLogsList.tapeRecorder.GetTapeFromLogID(audioLogsUnHighlightableButton.logID), audioLogsUnHighlightableButton.audioLogsList.audioSource);
                            audioLogsUnHighlightableButton.SetToGreen();
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AudioLogsList

            private static void HookAudioLogsListDisableAllLogs(On.AudioLogsList.orig_DisableAllLogs orig, AudioLogsList audioLogsList)
            {
                orig.Invoke(audioLogsList);
                VirtualAudioSource virtualAudioSource = audioLogsList.audioSource.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    virtualAudioSource.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                }
            }

            private static void HookAudioLogsListExitButtonPressed(On.AudioLogsList.orig_ExitButtonPressed orig, AudioLogsList audioLogsList)
            {
                orig.Invoke(audioLogsList);
                VirtualAudioSource virtualAudioSource = audioLogsList.audioSource.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    virtualAudioSource.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AudioSystem

            private static void HookAudioSystem()
            {
                On.AudioSystem.PlayAudio_1 += new On.AudioSystem.hook_PlayAudio_1(HookAudioSystemPlayAudio_1);
                //On.AudioSystem.CopyAudioSourceAndEffects += new On.AudioSystem.hook_CopyAudioSourceAndEffects(HookAudioSystemCopyAudioSourceAndEffects);
            }

            private static AudioSource HookAudioSystemCopyAudioSourceAndEffects(On.AudioSystem.orig_CopyAudioSourceAndEffects orig, Transform _transform, AudioSource _copySource)
            {
                if (_copySource == null)
                {
                    return null;
                }
                AudioSource audioSource = _transform.gameObject.AddComponent(typeof(AudioSource)) as AudioSource;

                /*
                VirtualAudioSource copyVirtualAudioSource = _copySource.GetComponent<VirtualAudioSource>();
                if (copyVirtualAudioSource != null)
                {
                    VirtualAudioSource virtualAudioSource = _transform.gameObject.AddComponent<VirtualAudioSource>();
                    virtualAudioSource = copyVirtualAudioSource;
                    virtualAudioSource.mySource = audioSource;
                    Debug.Log("Copied VirtualAudioSource to new AudioSource");
                }
                */
                _copySource.CopyTo(audioSource);
                _copySource.CopyEffectsTo(audioSource);
                if (finishedAudioAssignment)
                {
                    VirtualAudioSource originalVirtualAudioSource = _copySource.GetComponent<VirtualAudioSource>();
                    if (originalVirtualAudioSource != null)
                    {
                        //originalVirtualAudioSource.Stop();
                        //Debug.Log("Forced audio source " + originalVirtualAudioSource.clip.name + " to stop during copy");

                        //originalVirtualAudioSource.mySource = audioSource;
                    }
                    AddVirtualAudioSourceToAudioSource(ref audioSource);

                }
                return audioSource;
            }

            private static AudioSource HookAudioSystemPlayAudio_1(On.AudioSystem.orig_PlayAudio_1 orig, AudioLibrary _library, string _clipName, AudioSource _source)
            {
                if (finishedAudioAssignment && _source != null)
                {
                    // Try to find the VirtualAudioSource component previously assigned to the AudioSource.
                    VirtualAudioSource_ClosestListenerOnly _sourceToUse = _source.gameObject.GetComponent<VirtualAudioSource_ClosestListenerOnly>();

                    try
                    {
                        // If this is a new AudioSource, assign a VirtualAudioSource to it.
                        if (_sourceToUse == null)
                        {
                            if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "MainSecondary")
                            {
                                _sourceToUse = AddVirtualAudioSourceToAudioSource(ref _source);
                                //_sourceToUse = _source.gameObject.GetComponent<VirtualAudioSource_ClosestListenerOnly>();
                                if (_sourceToUse == null)
                                {
                                    if (ModSettings.logDebugText)
                                    {
                                        Debug.Log("Source to use is still null in HookAudioSystemPlayAudio_1");
                                    }
                                    return orig.Invoke(_library, _clipName, _source);
                                }
                            }
                            else
                            {
                                Debug.Log("The game is not in a round anymore.");
                                finishedAudioAssignment = false;
                                return orig.Invoke(_library, _clipName, _source);
                            }
                        }

                        /*
                        _sourceToUse.bypassEffects = _source.bypassEffects;
                        _sourceToUse.clip = _source.clip;
                        _sourceToUse.dopplerLevel = _source.dopplerLevel;
                        _sourceToUse.enabled = _source.enabled;
                        _sourceToUse.hideFlags = _source.hideFlags;
                        _sourceToUse.ignoreListenerPause = _source.ignoreListenerPause;
                        _sourceToUse.ignoreListenerVolume = _source.ignoreListenerVolume;
                        */

                        _sourceToUse.loop = _source.loop;

                        /*
                        _sourceToUse.maxDistance = _source.maxDistance;
                        _sourceToUse.minDistance = _source.minDistance;
                        _sourceToUse.mute = _source.mute;
                        _sourceToUse.pan = _source.panStereo;
                        _sourceToUse.panLevel = _source.spatialBlend;
                        _sourceToUse.pitch = _source.pitch;
                        _sourceToUse.playOnAwake = _source.playOnAwake;
                        _sourceToUse.priority = _source.priority;
                        _sourceToUse.rolloffMode = _source.rolloffMode;
                        _sourceToUse.spread = _source.spread;
                        */

                        _sourceToUse.time = _source.time;

                        /*
                        _sourceToUse.timeSamples = _source.timeSamples;
                        _sourceToUse.velocityUpdateMode = _source.velocityUpdateMode;
                        _sourceToUse.volume = _source.volume;
                        */
                    }
                    catch
                    {
                        Debug.Log("Error in AudioSystemPlayAudio_1 1\n" + new StackTrace().ToString());
                    }

                    try
                    {
                        if (_sourceToUse != null && AudioSystem.instance != null && _library != null && !AudioSystem.disableNewSounds)
                        {
                            try
                            {
                                if (_library.clips.Count > 0 && _library.FinishedGranular && !_source.isPlaying && !_sourceToUse.isPlaying && AudioSystem.instance.startMute < 0f)
                                {
                                    AudioClip clip = null;
                                    if (_clipName != string.Empty)
                                    {
                                        for (int i = 0; i < _library.clips.Count; i++)
                                        {
                                            if (_library.clips[i].name == _clipName)
                                            {
                                                clip = _library.clips[i];
                                                break;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        clip = _library.GetNext();
                                    }
                                    try
                                    {
                                        if (AudioSystem.pauseMenu != null && !AudioSystem.pauseMenu.pause)
                                        {
                                            _source.clip = clip;
                                            _sourceToUse.clip = clip;
                                        }
                                    }
                                    catch
                                    {
                                        Debug.Log("Error in AudioSystemPlayAudio_1 5\n" + new StackTrace().ToString());
                                    }
                                    try
                                    {
                                        if (AudioSystem.pauseMenu != null && !AudioSystem.pauseMenu.pause && _source.clip != null && _sourceToUse.clip != null)
                                        {
                                            try
                                            {
                                                if (_source.time > _source.clip.length && _sourceToUse.time > _sourceToUse.clip.length)
                                                {
                                                    _source.time = 0f;
                                                    _sourceToUse.time = 0f;
                                                }
                                            }
                                            catch
                                            {
                                                Debug.Log("Error in AudioSystemPlayAudio_1 8\n" + new StackTrace().ToString());
                                            }
                                            try
                                            {
                                                AudioLibrary.AudioLoopType loopType = _library.loopType;
                                                if (loopType != AudioLibrary.AudioLoopType.OneShot)
                                                {
                                                    try
                                                    {
                                                        if (loopType != AudioLibrary.AudioLoopType.Loop)
                                                        {
                                                            try
                                                            {
                                                                if (loopType == AudioLibrary.AudioLoopType.SuccessiveLoop)
                                                                {
                                                                    _sourceToUse.lockPlayingClipToListener = false;
                                                                    if (!_source.isPlaying && !_sourceToUse.isPlaying)
                                                                    {
                                                                        _source.Play();
                                                                        _sourceToUse.Play();
                                                                        if (ModSettings.logDebugText)
                                                                        {
                                                                            String textToShow = "The AudioSource is playing at " + _sourceToUse.mySource.transform.position + " from VirtualAudioSource at " + _sourceToUse.transform.position + " and having clip name " + _sourceToUse.mySource.clip.name;
                                                                            //ModSettings.ShowTextOnScreen(textToShow);
                                                                            Debug.Log("The source instance ID at audio system 1 is " + _sourceToUse.GetInstanceID());
                                                                            Debug.Log("Is original source playing? " + _source.isPlaying + ". Is it enabled? " + _source.isActiveAndEnabled + ". Is the copied source playing? " + _sourceToUse.isPlaying + ". Is it enabled? " + _sourceToUse.isActiveAndEnabled);
                                                                            Debug.Log(textToShow);
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                            catch
                                                            {
                                                                Debug.Log("Error in AudioSystemPlayAudio_1 11\n" + new StackTrace().ToString());
                                                            }
                                                        }
                                                        else
                                                        {
                                                            _sourceToUse.lockPlayingClipToListener = false;
                                                            _source.loop = true;
                                                            _sourceToUse.loop = true;
                                                            if (!_source.isPlaying && !_sourceToUse.isPlaying)
                                                            {
                                                                _source.Play();
                                                                _sourceToUse.Play();
                                                                if (ModSettings.logDebugText)
                                                                {
                                                                    String textToShow = "The AudioSource is playing at " + _sourceToUse.mySource.transform.position + " from VirtualAudioSource at " + _sourceToUse.transform.position + " and having clip name " + _sourceToUse.mySource.clip.name;
                                                                    //ModSettings.ShowTextOnScreen(textToShow);
                                                                    Debug.Log("The source instance ID at audio system 2 is " + _sourceToUse.GetInstanceID());
                                                                    Debug.Log("Is original source playing? " + _source.isPlaying + ". Is it enabled? " + _source.isActiveAndEnabled + ". Is the copied source playing? " + _sourceToUse.isPlaying + ". Is it enabled? " + _sourceToUse.isActiveAndEnabled);
                                                                    Debug.Log(textToShow);
                                                                }
                                                            }
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        Debug.Log("Error in AudioSystemPlayAudio_1 10\n" + new StackTrace().ToString());
                                                    }
                                                }
                                                else if (!_source.isPlaying && !_sourceToUse.isPlaying)
                                                {
                                                    try
                                                    {
                                                        _source.Play();
                                                        _sourceToUse.Play();
                                                        if (ModSettings.logDebugText)
                                                        {
                                                            String textToShow = "The AudioSource is playing at " + _sourceToUse.mySource.transform.position + " from VirtualAudioSource at " + _sourceToUse.transform.position + " and having clip name " + _sourceToUse.mySource.clip.name;
                                                            //ModSettings.ShowTextOnScreen(textToShow);
                                                            Debug.Log("The source instance ID at audio system 3 is " + _sourceToUse.GetInstanceID());
                                                            Debug.Log("Is original source playing? " + _source.isPlaying + ". Is it enabled? " + _source.isActiveAndEnabled + ". Is the copied source playing? " + _sourceToUse.isPlaying + ". Is it enabled? " + _sourceToUse.isActiveAndEnabled);
                                                            Debug.Log(textToShow);
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        Debug.Log("Error in AudioSystemPlayAudio_1 11\n" + new StackTrace().ToString());
                                                    }
                                                }
                                                else
                                                {
                                                    AudioSystem.PlaySound(_library, _clipName, _source.transform, _source);
                                                }
                                            }
                                            catch
                                            {
                                                Debug.Log("Error in AudioSystemPlayAudio_1 9\n" + new StackTrace().ToString());
                                            }
                                        }
                                        else
                                        {
                                            _source.Pause();
                                            _sourceToUse.Pause();
                                            if (ModSettings.logDebugText)
                                            {
                                                String textToShow = "The AudioSource is pausing at " + _sourceToUse.mySource.transform.position + " from VirtualAudioSource at " + _sourceToUse.transform.position + " and having clip name " + _sourceToUse.mySource.clip.name;
                                                //ModSettings.ShowTextOnScreen(textToShow);
                                                Debug.Log(textToShow);
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        Debug.Log("Error in AudioSystemPlayAudio_1 6\n" + new StackTrace().ToString());
                                    }
                                    try
                                    {
                                        if (AudioSystem.pauseMenu == null)
                                        {
                                            _source.clip = clip;
                                            _sourceToUse.clip = clip;
                                            AudioLibrary.AudioLoopType loopType2 = _library.loopType;
                                            if (loopType2 != AudioLibrary.AudioLoopType.OneShot)
                                            {
                                                if (loopType2 == AudioLibrary.AudioLoopType.Loop)
                                                {
                                                    _source.loop = true;
                                                    _sourceToUse.loop = true;
                                                    if (!_source.isPlaying && !_sourceToUse.isPlaying)
                                                    {
                                                        _source.Play();
                                                        _sourceToUse.Play();
                                                        if (ModSettings.logDebugText)
                                                        {
                                                            String textToShow = "The AudioSource is playing at " + _sourceToUse.mySource.transform.position + " from VirtualAudioSource at " + _sourceToUse.transform.position + " and having clip name " + _sourceToUse.mySource.clip.name;
                                                            //ModSettings.ShowTextOnScreen(textToShow);
                                                            Debug.Log("The source instance ID at audio system 4 is " + _sourceToUse.GetInstanceID());
                                                            Debug.Log("Is original source playing? " + _source.isPlaying + ". Is it enabled? " + _source.isActiveAndEnabled + ". Is the copied source playing? " + _sourceToUse.isPlaying + ". Is it enabled? " + _sourceToUse.isActiveAndEnabled);
                                                            Debug.Log(textToShow);
                                                        }
                                                    }
                                                }
                                            }
                                            else if (!_source.isPlaying && !_sourceToUse.isPlaying)
                                            {
                                                _source.Play();
                                                _sourceToUse.Play();
                                                if (ModSettings.logDebugText)
                                                {
                                                    String textToShow = "The AudioSource is playing at " + _sourceToUse.mySource.transform.position + " from VirtualAudioSource at " + _sourceToUse.transform.position + " and having clip name " + _sourceToUse.mySource.clip.name;
                                                    //ModSettings.ShowTextOnScreen(textToShow);
                                                    Debug.Log("The source instance ID at audio system 5 is " + _sourceToUse.GetInstanceID());
                                                    Debug.Log("Is original source playing? " + _source.isPlaying + ". Is it enabled? " + _source.isActiveAndEnabled + ". Is the copied source playing? " + _sourceToUse.isPlaying + ". Is it enabled? " + _sourceToUse.isActiveAndEnabled);
                                                    Debug.Log(textToShow);
                                                }
                                            }
                                            else
                                            {
                                                AudioSystem.PlaySound(_library, _clipName, _source.transform, _source);
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        Debug.Log("Error in AudioSystemPlayAudio_1 7\n" + new StackTrace().ToString());
                                    }
                                }
                            }
                            catch
                            {
                                Debug.Log("Error in AudioSystemPlayAudio_1 3\n" + new StackTrace().ToString());
                            }
                            try
                            {
                                if (_source.isPlaying && _sourceToUse.isPlaying)
                                {
                                    //_library.OnSoundPlayed(_sourceToUse.mySource);
                                    AudioLibraryOnSoundPlayed(_library, _sourceToUse);

                                    /*
                                    _sourceToUse.bypassEffects = _source.bypassEffects;
                                    _sourceToUse.clip = _source.clip;
                                    _sourceToUse.dopplerLevel = _source.dopplerLevel;
                                    _sourceToUse.enabled = _source.enabled;
                                    _sourceToUse.hideFlags = _source.hideFlags;
                                    _sourceToUse.ignoreListenerPause = _source.ignoreListenerPause;
                                    _sourceToUse.ignoreListenerVolume = _source.ignoreListenerVolume;

                                    _sourceToUse.loop = _source.loop;

                                    _sourceToUse.maxDistance = _source.maxDistance;
                                    _sourceToUse.minDistance = _source.minDistance;
                                    _sourceToUse.mute = _source.mute;
                                    _sourceToUse.pan = _source.panStereo;
                                    _sourceToUse.panLevel = _source.spatialBlend;
                                    _sourceToUse.pitch = _source.pitch;
                                    _sourceToUse.playOnAwake = _source.playOnAwake;
                                    _sourceToUse.priority = _source.priority;
                                    _sourceToUse.rolloffMode = _source.rolloffMode;
                                    _sourceToUse.spread = _source.spread;
                                    _sourceToUse.time = _source.time;
                                    _sourceToUse.timeSamples = _source.timeSamples;
                                    _sourceToUse.velocityUpdateMode = _source.velocityUpdateMode;
                                    _sourceToUse.volume = _source.volume;
                                    */

                                    if (_library.gameplayAudio != null)
                                    {
                                        _library.gameplayAudio.OnSoundPlayed(_source); // THIS CREATES THE DISTRACTION SOUND, SO THE ORIGINAL SOURCE MUST BE USED!
                                    }

                                    if (!_sourceToUse.mySource.clip.ToString().Contains("MUS"))
                                    {
                                        if (ModSettings.logDebugText)
                                        {
                                            String textToShow = "The AudioSource is already playing at " + _sourceToUse.mySource.transform.position + " from VirtualAudioSource at " + _sourceToUse.transform.position + " and having clip name " + _sourceToUse.mySource.clip.name;
                                            //ModSettings.ShowTextOnScreen(textToShow);
                                            Debug.Log(textToShow);
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                Debug.Log("Error in AudioSystemPlayAudio_1 4\n" + new StackTrace().ToString());
                            }
                            return _sourceToUse.mySource;
                        }
                    }
                    catch
                    {
                        Debug.Log("Error in AudioSystemPlayAudio_1 2\n" + new StackTrace().ToString());
                    }
                }
                else
                {
                    //Debug.Log("Original source is null or audio assignment has not been finished in HookAudioSystemPlayAudio_1");
                    return orig.Invoke(_library, _clipName, _source);
                }
                return null;
            }

            private static AudioSource HookAudioSystemPlayAudio_1WithDiagnostics(On.AudioSystem.orig_PlayAudio_1 orig, AudioLibrary _library, string _clipName, AudioSource _source)
            {
                if (finishedAudioAssignment && _source != null)
                {
                    if (_source != null && _source.clip != null && _source.clip.name != null && (_source.clip.name.Contains("Torch") || _clipName.Contains("Torch")))
                    {
                        Debug.Log("Checking source using clip " + _source.clip.name + " and clipName variable " + _clipName);
                    }

                    // Try to find the VirtualAudioSource component previously assigned to the AudioSource.
                    VirtualAudioSource _sourceToUse = _source.gameObject.GetComponent<VirtualAudioSource>();

                    // If this is a new AudioSource, assign a VirtualAudioSource to it.
                    if (_sourceToUse == null)
                    {
                        _sourceToUse = AddVirtualAudioSourceToAudioSource(ref _source);
                        //_sourceToUse = _source.gameObject.GetComponent<VirtualAudioSource>();
                        if (_sourceToUse == null)
                        {
                            Debug.Log("Source to use is still null");
                        }
                    }

                    bool specialSkip = false;
                    if (_sourceToUse != null && _sourceToUse.isPlaying && !_source.isPlaying)
                    {
                        specialSkip = true;
                        _sourceToUse.Stop();
                        _sourceToUse.mySource.Stop();
                        Debug.Log("Early in AudSys forced audio source " + _sourceToUse.clip.name + " to stop");

                        if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                        {
                            Debug.Log("Torch is at AUS: " + _source.clip.name);
                        }
                        _library.OnSoundPlayed(_sourceToUse.mySource);
                        if (_library.gameplayAudio != null)
                        {
                            _library.gameplayAudio.OnSoundPlayed(_sourceToUse.mySource);
                        }

                        if (!_sourceToUse.mySource.clip.ToString().Contains("MUS"))
                        {
                            String textToShow = "The AudioSource is already playing at AUS at " + _sourceToUse.mySource.transform.position + " from VirtualAudioSource at " + _sourceToUse.transform.position + " and having clip name " + _sourceToUse.mySource.clip.name;
                            ModSettings.ShowTextOnScreen(textToShow);
                            Debug.Log(textToShow);
                        }
                    }

                    if (_sourceToUse != null && AudioSystem.instance != null && _library != null && !AudioSystem.disableNewSounds)
                    {
                        if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                        {
                            Debug.Log("Torch is at A: " + _source.clip.name);
                            Debug.Log("Torch checks are " + (_library.clips.Count > 0) + ", " + _library.FinishedGranular + ", " + !_source.isPlaying + ", " + !_sourceToUse.isPlaying + ", " + (AudioSystem.instance.startMute < 0f));
                        }
                        if (_library.clips.Count > 0 && _library.FinishedGranular && !_source.isPlaying && (!_sourceToUse.isPlaying || specialSkip) && AudioSystem.instance.startMute < 0f)
                        {
                            if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                            {
                                Debug.Log("Torch is at B: " + _source.clip.name);
                            }
                            AudioClip clip = null;
                            if (_clipName != string.Empty)
                            {
                                if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                                {
                                    Debug.Log("Torch is at C: " + _source.clip.name);
                                }
                                for (int i = 0; i < _library.clips.Count; i++)
                                {
                                    if (_library.clips[i].name == _clipName)
                                    {
                                        clip = _library.clips[i];
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                                {
                                    Debug.Log("Torch is at D: " + _source.clip.name);
                                }
                                clip = _library.GetNext();
                            }
                            if (AudioSystem.pauseMenu != null && !AudioSystem.pauseMenu.pause)
                            {
                                if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                                {
                                    Debug.Log("Torch is at E1: " + _source.clip.name);
                                }
                                _source.clip = clip;
                                _sourceToUse.clip = clip;
                                if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                                {
                                    Debug.Log("Torch is at E2: " + _source.clip.name);
                                }
                            }
                            if (AudioSystem.pauseMenu != null && !AudioSystem.pauseMenu.pause && _source.clip != null && _sourceToUse.clip != null)
                            {
                                if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                                {
                                    Debug.Log("Torch is at F: " + _source.clip.name);
                                }
                                if (_source.time > _source.clip.length && _sourceToUse.time > _sourceToUse.clip.length)
                                {
                                    _source.time = 0f;
                                    _sourceToUse.time = 0f;
                                }
                                AudioLibrary.AudioLoopType loopType = _library.loopType;
                                if (loopType != AudioLibrary.AudioLoopType.OneShot)
                                {
                                    if (loopType != AudioLibrary.AudioLoopType.Loop)
                                    {
                                        if (loopType == AudioLibrary.AudioLoopType.SuccessiveLoop)
                                        {
                                            if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                                            {
                                                Debug.Log("Torch is at 1: " + _source.clip.name);
                                            }
                                            if (!_source.isPlaying && !_sourceToUse.isPlaying)
                                            {
                                                Debug.Log("The source instance ID at audio system 1 is " + _sourceToUse.GetInstanceID());
                                                _source.Play();
                                                _sourceToUse.Play();
                                                Debug.Log("Is original source playing? " + _source.isPlaying + ". Is it enabled? " + _source.isActiveAndEnabled + ". Is the copied source playing? " + _sourceToUse.isPlaying + ". Is it enabled? " + _sourceToUse.isActiveAndEnabled);
                                                String textToShow = "The AudioSource is playing at " + _sourceToUse.mySource.transform.position + " from VirtualAudioSource at " + _sourceToUse.transform.position + " and having clip name " + _sourceToUse.mySource.clip.name;
                                                ModSettings.ShowTextOnScreen(textToShow);
                                                Debug.Log(textToShow);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        _source.loop = true;
                                        _sourceToUse.loop = true;
                                        if (!_source.isPlaying && !_sourceToUse.isPlaying)
                                        {
                                            if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                                            {
                                                Debug.Log("Torch is at 2: " + _source.clip.name);
                                            }
                                            Debug.Log("The source instance ID at audio system 2 is " + _sourceToUse.GetInstanceID());
                                            _source.Play();
                                            _sourceToUse.Play();
                                            Debug.Log("Is original source playing? " + _source.isPlaying + ". Is it enabled? " + _source.isActiveAndEnabled + ". Is the copied source playing? " + _sourceToUse.isPlaying + ". Is it enabled? " + _sourceToUse.isActiveAndEnabled);
                                            String textToShow = "The AudioSource is playing at " + _sourceToUse.mySource.transform.position + " from VirtualAudioSource at " + _sourceToUse.transform.position + " and having clip name " + _sourceToUse.mySource.clip.name;
                                            ModSettings.ShowTextOnScreen(textToShow);
                                            Debug.Log(textToShow);
                                        }
                                    }
                                }
                                else if (!_source.isPlaying && !_sourceToUse.isPlaying)
                                {
                                    if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                                    {
                                        Debug.Log("Torch is at 3: " + _source.clip.name);
                                    }
                                    Debug.Log("The source instance ID at audio system 3 is " + _sourceToUse.GetInstanceID());
                                    _source.Play();
                                    _sourceToUse.Play();
                                    Debug.Log("Is original source playing? " + _source.isPlaying + ". Is it enabled? " + _source.isActiveAndEnabled + ". Is the copied source playing? " + _sourceToUse.isPlaying + ". Is it enabled? " + _sourceToUse.isActiveAndEnabled);
                                    String textToShow = "The AudioSource is playing at " + _sourceToUse.mySource.transform.position + " from VirtualAudioSource at " + _sourceToUse.transform.position + " and having clip name " + _sourceToUse.mySource.clip.name;
                                    ModSettings.ShowTextOnScreen(textToShow);
                                    Debug.Log(textToShow);
                                }
                                else
                                {
                                    if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                                    {
                                        Debug.Log("Torch is at G: " + _source.clip.name);
                                    }
                                    //AudioSystem.PlaySound(_library, _clipName, _sourceToUse.transform, _sourceToUse.mySource);
                                    AudioSystem.PlaySound(_library, _clipName, _source.transform, _source);
                                }
                            }
                            else
                            {
                                if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                                {
                                    Debug.Log("Torch is at 4: " + _source.clip.name);
                                }
                                _source.Pause();
                                _sourceToUse.Pause();
                                String textToShow = "The AudioSource is pausing at " + _sourceToUse.mySource.transform.position + " from VirtualAudioSource at " + _sourceToUse.transform.position + " and having clip name " + _sourceToUse.mySource.clip.name;
                                ModSettings.ShowTextOnScreen(textToShow);
                                Debug.Log(textToShow);
                            }
                            if (AudioSystem.pauseMenu == null)
                            {
                                _source.clip = clip;
                                _sourceToUse.clip = clip;
                                AudioLibrary.AudioLoopType loopType2 = _library.loopType;
                                if (loopType2 != AudioLibrary.AudioLoopType.OneShot)
                                {
                                    if (loopType2 == AudioLibrary.AudioLoopType.Loop)
                                    {
                                        _source.loop = true;
                                        _sourceToUse.loop = true;
                                        if (!_source.isPlaying && !_sourceToUse.isPlaying)
                                        {
                                            if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                                            {
                                                Debug.Log("Torch is at 5: " + _source.clip.name);
                                            }
                                            Debug.Log("The source instance ID at audio system 4 is " + _sourceToUse.GetInstanceID());
                                            _source.Play();
                                            _sourceToUse.Play();
                                            Debug.Log("Is original source playing? " + _source.isPlaying + ". Is it enabled? " + _source.isActiveAndEnabled + ". Is the copied source playing? " + _sourceToUse.isPlaying + ". Is it enabled? " + _sourceToUse.isActiveAndEnabled);
                                            String textToShow = "The AudioSource is playing at " + _sourceToUse.mySource.transform.position + " from VirtualAudioSource at " + _sourceToUse.transform.position + " and having clip name " + _sourceToUse.mySource.clip.name;
                                            ModSettings.ShowTextOnScreen(textToShow);
                                            Debug.Log(textToShow);
                                        }
                                    }
                                }
                                else if (!_source.isPlaying && !_sourceToUse.isPlaying)
                                {
                                    if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                                    {
                                        Debug.Log("Torch is at 6: " + _source.clip.name);
                                    }
                                    Debug.Log("The source instance ID at audio system 5 is " + _sourceToUse.GetInstanceID());
                                    _source.Play();
                                    _sourceToUse.Play();
                                    Debug.Log("Is original source playing? " + _source.isPlaying + ". Is it enabled? " + _source.isActiveAndEnabled + ". Is the copied source playing? " + _sourceToUse.isPlaying + ". Is it enabled? " + _sourceToUse.isActiveAndEnabled);
                                    String textToShow = "The AudioSource is playing at " + _sourceToUse.mySource.transform.position + " from VirtualAudioSource at " + _sourceToUse.transform.position + " and having clip name " + _sourceToUse.mySource.clip.name;
                                    ModSettings.ShowTextOnScreen(textToShow);
                                    Debug.Log(textToShow);
                                }
                                else
                                {
                                    if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                                    {
                                        Debug.Log("Torch is at 7: " + _source.clip.name);
                                    }
                                    //AudioSystem.PlaySound(_library, _clipName, _sourceToUse.transform, _sourceToUse.mySource);
                                    AudioSystem.PlaySound(_library, _clipName, _source.transform, _source);
                                }
                            }
                        }
                        if (_source.isPlaying && _sourceToUse.isPlaying)
                        {
                            if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                            {
                                Debug.Log("Torch is at 8: " + _source.clip.name + ". Source time is " + _source.time + " and VAS time is " + _sourceToUse.time + " and their durations are " + _source.clip.length + " and " + _sourceToUse.clip.length);
                            }
                            /*
                            if (!_source.isPlaying)
                            {
                                _sourceToUse.Pause();
                                Debug.Log("Forced audio source " + _sourceToUse.clip.name + " to stop");
                            }
                            else
                            {
                                */
                            if (_source.time > _source.clip.length && _sourceToUse.time > _sourceToUse.clip.length)
                            {
                                if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                                {
                                    Debug.Log("Torch is at 9: " + _source.clip.name);
                                }
                                _source.Stop();
                                _sourceToUse.Stop();
                            }
                            else
                            {
                                if (_source != null && _source.clip != null && _source.clip.name != null && _source.clip.name.Contains("Torch"))
                                {
                                    Debug.Log("Torch is at 10: " + _source.clip.name);
                                }
                                _library.OnSoundPlayed(_sourceToUse.mySource);
                                if (_library.gameplayAudio != null)
                                {
                                    _library.gameplayAudio.OnSoundPlayed(_sourceToUse.mySource);
                                }

                                if (!_sourceToUse.mySource.clip.ToString().Contains("MUS"))
                                {
                                    String textToShow = "The AudioSource is already playing at " + _sourceToUse.mySource.transform.position + " from VirtualAudioSource at " + _sourceToUse.transform.position + " and having clip name " + _sourceToUse.mySource.clip.name;
                                    ModSettings.ShowTextOnScreen(textToShow);
                                    Debug.Log(textToShow);
                                }
                            }
                            //}
                        }
                        return _sourceToUse.mySource;
                    }
                }
                return null;
            }

            public static float SignedAngleBetween(Vector3 a, Vector3 b, Vector3 n)
            {
                // From https://stackoverflow.com/questions/19675676/calculating-actual-angle-between-two-vectors-in-unity3d
                // angle in [0,180]
                float angle = Vector3.Angle(a, b);
                float sign = Mathf.Sign(Vector3.Dot(n, Vector3.Cross(a, b)));

                // angle in [-179,180]
                float signed_angle = angle * sign;

                // angle in [0,360] (not used but included here for completeness)
                //float angle360 =  (signed_angle + 180) % 360;

                return signed_angle;
            }

            private static IEnumerator PlaySoundAtCustomPosition(AudioSource _source, AudioLibrary _library)
            {
                if (newPlayerClasses != null)
                {
                    // Declare variables used during position calculation.
                    Vector3 closestPlayerPosition;
                    int closestPlayer;
                    Vector3 distanceFromSourceToClosestPlayer;
                    float angleBetweenReferencePlayerAndClosestPlayer;
                    Vector3 distanceFromSourceToClosestPlayerAngleAdjusted;
                    Vector3 distanceRelativeToAudioListener;

                    // Calculate where the sound should be played.
                    closestPlayer = ClosestPlayerToThis(_source.gameObject.transform.position);
                    Camera playerCamera = PlayerCamera(newPlayerClasses[closestPlayer]);
                    closestPlayerPosition = playerCamera.transform.position;
                    distanceFromSourceToClosestPlayer = _source.gameObject.transform.position - closestPlayerPosition;
                    angleBetweenReferencePlayerAndClosestPlayer = SignedAngleBetween(References.CamMiddle.forward, playerCamera.transform.forward, References.CamMiddle.up);
                    distanceFromSourceToClosestPlayerAngleAdjusted = Quaternion.AngleAxis(angleBetweenReferencePlayerAndClosestPlayer, Vector3.up) * distanceFromSourceToClosestPlayer;
                    distanceRelativeToAudioListener = SimpleOcclusion.listener.transform.position + References.CamMiddle.forward + distanceFromSourceToClosestPlayerAngleAdjusted;

                    // Set up the audio source.
                    GameObject audioSourceGameobject = new GameObject();
                    AudioSource _sourceCopy = audioSourceGameobject.AddComponent<AudioSource>();
                    _sourceCopy.clip = _source.clip;
                    _sourceCopy.volume = _source.volume;
                    _sourceCopy.pitch = _source.volume;
                    _sourceCopy.loop = _source.loop;
                    _sourceCopy.minDistance = _source.minDistance;
                    _sourceCopy.maxDistance = float.MaxValue;//_source.maxDistance;
                    _sourceCopy.transform.position = _source.transform.position;


                    audioSourceGameobject.transform.position = distanceRelativeToAudioListener;
                    _source.transform.position = distanceRelativeToAudioListener;

                    // Print diagnostics data.
                    Debug.Log("Positions before are: Player 0 " + newPlayerClasses[0].transform.position + " and player 1 " + newPlayerClasses[1].transform.position + " and 0 to source " + (_source.gameObject.transform.position - newPlayerClasses[0].transform.position) + " and 1 to source " + (_source.gameObject.transform.position - newPlayerClasses[1].transform.position) + " and source: " + _source.gameObject.transform.position + " and camera " + References.Cam.transform.position);
                    Debug.Log("The source instance ID at audio system 3 is " + _source.GetInstanceID() + " and volume is " + _sourceCopy.volume + " and distance to player " + closestPlayer + " is " + distanceFromSourceToClosestPlayer + " and angle adjusted distance used is " + distanceFromSourceToClosestPlayerAngleAdjusted + " and adjusted position is " + distanceRelativeToAudioListener + " and angle between players is " + angleBetweenReferencePlayerAndClosestPlayer);

                    foreach (AudioListener audioListener in FindObjectsOfType<AudioListener>())
                    {
                        Debug.Log("Audio listener instance id is " + audioListener.GetInstanceID() + " and position is " + audioListener.transform.position + " and enabled status is " + audioListener.enabled + " and active and enabled status is " + audioListener.isActiveAndEnabled);
                    }

                    // Play the virtual source and adjust its position as it is playing.
                    float originalVolume = _source.volume;
                    _source.volume = 0f;
                    _source.Play();
                    _sourceCopy.Play();
                    _library.OnSoundPlayed(_sourceCopy);
                    if (_library.gameplayAudio != null)
                    {
                        _library.gameplayAudio.OnSoundPlayed(_sourceCopy);
                    }

                    while (_sourceCopy.isPlaying)
                    {
                        //Debug.Log("Source copy position is " + _sourceCopy.transform.position);
                        if (_source != null)
                        {
                            closestPlayer = ClosestPlayerToThis(_source.gameObject.transform.position);
                            playerCamera = PlayerCamera(newPlayerClasses[closestPlayer]);
                            closestPlayerPosition = playerCamera.transform.position;
                            distanceFromSourceToClosestPlayer = _source.gameObject.transform.position - closestPlayerPosition;
                            angleBetweenReferencePlayerAndClosestPlayer = SignedAngleBetween(References.CamMiddle.forward, playerCamera.transform.forward, References.CamMiddle.up);
                            distanceFromSourceToClosestPlayerAngleAdjusted = Quaternion.AngleAxis(angleBetweenReferencePlayerAndClosestPlayer, Vector3.up) * distanceFromSourceToClosestPlayer;
                            distanceRelativeToAudioListener = SimpleOcclusion.listener.transform.position + References.CamMiddle.forward + distanceFromSourceToClosestPlayerAngleAdjusted;
                            audioSourceGameobject.transform.position = distanceRelativeToAudioListener;
                            _source.transform.position = distanceRelativeToAudioListener;

                        }
                        yield return null;
                    }

                    // Destroy the virtual source after it has played.
                    _source.volume = originalVolume;
                    _sourceCopy.gameObject.SetActive(false);
                    SimpleOcclusion simpleOcclusionOfCopy = _sourceCopy.gameObject.GetComponent<SimpleOcclusion>();
                    if (simpleOcclusionOfCopy != null)
                    {
                        Debug.Log("Destroying audio source copy's simple occlusion");
                        UnityEngine.Object.Destroy(simpleOcclusionOfCopy);
                    }
                    UnityEngine.Object.Destroy(_sourceCopy);
                }
                else
                {
                    Debug.Log("Npcs is null while trying to play sound.");
                    _source.Play();
                }
                yield break;
            }

            private static void PlaySoundAtCustomPositionSimple(AudioSource _source, AudioLibrary _library)
            {
                if (newPlayerClasses != null)
                {
                    // Declare variables used during position calculation.
                    Vector3 closestPlayerPosition;
                    int closestPlayer;
                    Vector3 distanceFromSourceToClosestPlayer;
                    float angleBetweenReferencePlayerAndClosestPlayer;
                    Vector3 distanceFromSourceToClosestPlayerAngleAdjusted;
                    Vector3 distanceRelativeToAudioListener;

                    // Calculate where the sound should be played.
                    closestPlayer = ClosestPlayerToThis(_source.gameObject.transform.position);
                    Camera playerCamera = PlayerCamera(newPlayerClasses[closestPlayer]);
                    closestPlayerPosition = playerCamera.transform.position;
                    distanceFromSourceToClosestPlayer = _source.gameObject.transform.position - closestPlayerPosition;
                    angleBetweenReferencePlayerAndClosestPlayer = SignedAngleBetween(References.CamMiddle.forward, playerCamera.transform.forward, References.CamMiddle.up);
                    distanceFromSourceToClosestPlayerAngleAdjusted = Quaternion.AngleAxis(angleBetweenReferencePlayerAndClosestPlayer, Vector3.up) * distanceFromSourceToClosestPlayer;
                    distanceRelativeToAudioListener = SimpleOcclusion.listener.transform.position + References.CamMiddle.forward + distanceFromSourceToClosestPlayerAngleAdjusted;

                    AudioSource.PlayClipAtPoint(_source.clip, distanceRelativeToAudioListener);
                }
                else
                {
                    Debug.Log("Npcs is null while trying to play sound.");
                    _source.Play();
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AutoProne

            private static void HookAutoProneUpdate(On.AutoProne.orig_Update orig, AutoProne autoProne)
            {

                foreach (NewPlayerClass newPlayerClass in crewPlayers)
                {
                    int playerNumber = PlayerNumber(newPlayerClass.GetInstanceID());
                    if (autoProne.enter.Hit && newPlayerClass.IsCrouched() && !GetPlayerKey("Left", playerNumber).IsDown() && !GetPlayerKey("Right", playerNumber).IsDown() && Mathf.Abs(GetPlayerAxisValue("X", playerNumber)) < 0.1f && !newPlayerClass.movePlayerUnderBed)
                    {
                        Vector3 playerDirection = newPlayerClass.Motor.GetPlayerDirection();
                        if (playerDirection.magnitude > 0f)
                        {
                            autoProne.ray.origin = newPlayerClass.transform.position + Vector3.up * 0.4f;
                            autoProne.ray.direction = playerDirection;
                            if (autoProne.proneCollider.Raycast(autoProne.ray, out autoProne.hit, 0.4f))
                            {
                                MovePlayerUnderBed movePlayerUnderBed = ((MonoBehaviour)autoProne).transform.parent.GetComponentInChildren<MovePlayerUnderBed>();
                                movePlayerUnderBed.player = newPlayerClass;
                                movePlayerUnderBed.StartTransition(autoProne.entryPoint.transform);
                            }
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AutoPronePlayer

            private static void HookAutoPronePlayerAwake(On.AutoPronePlayer.orig_Awake orig, AutoPronePlayer autoPronePlayer)
            {
                autoPronePlayer.box = (((MonoBehaviour)autoPronePlayer).GetComponent<Collider>() as BoxCollider);
                autoPronePlayer.player = autoPronePlayer.box.GetComponentInParent<NewPlayerClass>();
                if (autoPronePlayer.player != null)
                {
                    Debug.Log("Link found in parent of " + autoPronePlayer.box.name + " for type NewPlayerClass with player number " + PlayerNumber(autoPronePlayer.player.GetInstanceID()));
                }
            }

            private static void HookAutoPronePlayerUpdate(On.AutoPronePlayer.orig_Update orig, AutoPronePlayer autoPronePlayer)
            {
                bool foundLink = false;
                if (autoPronePlayer.box.GetComponentInParent(typeof(NewPlayerClass)) != null)
                {
                    autoPronePlayer.player = autoPronePlayer.box.GetComponentInParent<NewPlayerClass>();
                    Debug.Log("Link found in parent of " + autoPronePlayer.box.name + " for type NewPlayerClass");
                    foundLink = true;
                }
                if (autoPronePlayer.box.GetComponent(typeof(NewPlayerClass)) != null)
                {
                    Debug.Log("Link found on same level of " + autoPronePlayer.box.name + " for type NewPlayerClass");
                    foundLink = true;
                }
                if (autoPronePlayer.box.GetComponentInChildren(typeof(NewPlayerClass)) != null)
                {
                    Debug.Log("Link found in children of " + autoPronePlayer.box.name + " for type NewPlayerClass");
                    foundLink = true;
                }
                if (!foundLink)
                {
                    Debug.Log("No link found for " + autoPronePlayer.box.name + " for type NewPlayerClass");
                }
                if (newPlayerClasses != null)
                {
                    autoPronePlayer.player = newPlayerClasses[ClosestPlayerToThis(autoPronePlayer.transform.position)];
                }
                orig.Invoke(autoPronePlayer);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AutoStand

            private static void HookAutoStand()
            {
                On.AutoStand.OnTriggerEnter += new On.AutoStand.hook_OnTriggerEnter(HookAutoStandOnTriggerEnter);
                On.AutoStand.OnTriggerExit += new On.AutoStand.hook_OnTriggerExit(HookAutoStandOnTriggerExit);
            }

            private static void HookAutoStandOnTriggerEnter(On.AutoStand.orig_OnTriggerEnter orig, AutoStand autoStand, Collider _collider)
            {
                if (PlayerHelper.IsPlayerBody(_collider))
                {
                    _collider.gameObject.GetComponentInParent<NewPlayerClass>().IncreaseForceStand();
                }
            }

            private static void HookAutoStandOnTriggerExit(On.AutoStand.orig_OnTriggerExit orig, AutoStand autoStand, Collider _collider)
            {
                if (PlayerHelper.IsPlayerBody(_collider))
                {
                    _collider.gameObject.GetComponentInParent<NewPlayerClass>().DecreaseForceStand();
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AvoidWalls

            private static void HookAvoidWallsRaycastAvoidWalls(On.AvoidWalls.orig_RaycastAvoidWalls orig, AvoidWalls avoidWalls, Vector3 _point, Vector3 _dir)
            {
                Debug.Log("Vector " + _point + " is being used in AvoidWalls.RaycastAvoidWalls");
                orig.Invoke(avoidWalls, _point, _dir);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Backpack

            private static void HookBackpackOnInteract(On.Backpack.orig_OnInteract orig, Backpack backpack)
            {
                Inventory inventory = PlayerInventory(lastPlayerSentMessage);
                for (int i = 0; i < backpack.backpackSlots; i++)
                {
                    inventory.CreateSlot(i);
                    inventory.maxInventoryCapacity = inventory.maxInventoryCapacity + 1;
                    UnityEngine.Object.Destroy(((MonoBehaviour)backpack).gameObject);
                }
                inventory.DisplayInventory();
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @BakedOcclusion

            private static Vector3[] previousCameraPositions;
            private static List<VisibilityData>[] previousVisibilityData;

            private static void HookBakedOcclusionLateUpdate(On.BakedOcclusion.orig_LateUpdate orig, BakedOcclusion bakedOcclusion)
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    // This only renders the second camera's point of view for player 1's camera.
                    for (int playerNumber = 0; playerNumber < newPlayerClasses.Count; playerNumber++)
                    {
                        if (bakedOcclusion.baked)
                        {
                            Camera playerCamera;
                            if (!ModSettings.enableCrewVSMonsterMode || !ModSettings.numbersOfMonsterPlayers.Contains(playerNumber))
                            {
                                playerCamera = PlayerCamera(newPlayerClasses[playerNumber]);
                            }
                            else
                            {
                                playerCamera = PlayerCamera(newPlayerClasses[playerNumber]);
                                //playerCamera = ManyMonstersMode.monsterList[PlayerMonsterNumberFromPlayerNumber(playerNumber)].GetComponentInChildren<Camera>();
                            }
                            bakedOcclusion.camPos = MathHelper.RoundToNearest(playerCamera.transform.position, bakedOcclusion.resolution);
                            NodeData nodeDataAtPosition = LevelGeneration.GetNodeDataAtPosition(bakedOcclusion.camPos);
                            if (nodeDataAtPosition != null && nodeDataAtPosition.nodeRoom != null && nodeDataAtPosition.nodeRoom.ActiveModel != null && nodeDataAtPosition.nodeRoom.ActiveModel.GetComponent<Renderer>() != null && !nodeDataAtPosition.nodeRoom.ActiveModel.GetComponent<Renderer>().enabled)
                            {
                                bakedOcclusion.forceUpdate = true;
                            }
                            if (previousCameraPositions[playerNumber] != bakedOcclusion.camPos)
                            {
                                bakedOcclusion.forceUpdate = true;
                            }
                            if (Vector3.Angle(bakedOcclusion.lastDirection, playerCamera.transform.forward) > bakedOcclusion.angleTolerance)
                            {
                                bakedOcclusion.forceUpdate = true;
                            }
                            if (bakedOcclusion.forceUpdate)
                            {
                                UpdateCullers(bakedOcclusion, playerNumber);
                                previousCameraPositions[playerNumber] = bakedOcclusion.camPos;
                                bakedOcclusion.lastDirection = playerCamera.transform.forward;
                            }
                        }
                    }
                }
            }

            private static void UpdateCullers(BakedOcclusion bakedOcclusion, int playerNumber)
            {
                bakedOcclusion.forceUpdate = false;
                bakedOcclusion.camVisiblityData = bakedOcclusion.GetData(bakedOcclusion.camPos);
                if (bakedOcclusion.camVisiblityData != null)
                {
                    previousVisibilityData[playerNumber] = bakedOcclusion.current;
                    if (playerNumber == 0)
                    {
                        LightCulling2.Instance.ClearCurrentLights();
                    }
                    int count = previousVisibilityData[playerNumber].Count;
                    for (int i = 0; i < count; i++)
                    {
                        bakedOcclusion.tempVisData = previousVisibilityData[playerNumber][i];
                        int count2 = bakedOcclusion.tempVisData.Assigned.Count;
                        for (int j = 0; j < count2; j++)
                        {
                            if (bakedOcclusion.tempVisData.Assigned[j] != null)
                            {
                                bakedOcclusion.tempVisData.Assigned[j].Hide();
                            }
                        }
                    }
                    if (playerNumber == 0)
                    {
                        bakedOcclusion.current.Clear();
                    }
                    bakedOcclusion.current = Find(bakedOcclusion.camVisiblityData.position, bakedOcclusion.current, RealtimeOcclusion2.Instance, newPlayerClasses[playerNumber]);
                    count = bakedOcclusion.current.Count;
                    for (int k = 0; k < count; k++)
                    {
                        bakedOcclusion.tempVisData = bakedOcclusion.current[k];
                        foreach (LightCull2 culler in bakedOcclusion.tempVisData.Lights)
                        {
                            LightCulling2.Instance.AddToCurrentLights(culler);
                        }
                        int count2 = bakedOcclusion.tempVisData.Assigned.Count;
                        for (int l = 0; l < count2; l++)
                        {
                            if (bakedOcclusion.tempVisData.Assigned[l] != null)
                            {
                                bakedOcclusion.tempVisData.Assigned[l].Show();
                            }
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @BillboardPlane

            private static void HookBillboardPlaneUpdate(On.BillboardPlane.orig_Update orig, BillboardPlane billboardPlane)
            {
                billboardPlane.target = Quaternion.LookRotation(PlayerCamera(newPlayerClasses[ClosestPlayerToThis(((MonoBehaviour)billboardPlane).transform.position)]).transform.position - ((MonoBehaviour)billboardPlane).gameObject.transform.position);
                ((MonoBehaviour)billboardPlane).gameObject.transform.rotation = Quaternion.Slerp(((MonoBehaviour)billboardPlane).gameObject.transform.rotation, billboardPlane.target, 10f * Time.deltaTime);
                billboardPlane.rotationVector = new Vector3(billboardPlane.originalX, ((MonoBehaviour)billboardPlane).gameObject.transform.rotation.eulerAngles.y, billboardPlane.originalZ);
                ((MonoBehaviour)billboardPlane).gameObject.transform.rotation = Quaternion.Euler(billboardPlane.rotationVector);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @BodyTurn

            private static void HookBodyTurnUpdate(On.BodyTurn.orig_Update orig, BodyTurn bodyTurn)
            {
                if ((bodyTurn.monster.ProxAlert > 10f && !bodyTurn.monster.IsPlayerHiding) || bodyTurn.monster.IsPlayerLocationKnown)
                {
                    if (!bodyTurn.aniControl.IsAttacking)
                    {
                        bodyTurn.lookTarget = bodyTurn.monster.player.transform.position;
                        bodyTurn.shouldLook = true;
                    }
                }
                else if (bodyTurn.monster.MoveControl != null && bodyTurn.monster.MoveControl.Goal != Vector3.zero && !bodyTurn.monster.MoveControl.IsAtDestination)
                {
                    Vector3 vector = bodyTurn.monster.transform.position + Vector3.up * 1.5f;
                    Vector3 goal = bodyTurn.monster.MoveControl.Goal;
                    float maxDistance = Vector3.Distance(vector, goal);
                    Ray ray = new Ray(vector, goal - vector);
                    RaycastHit raycastHit;
                    if (Physics.Raycast(vector, ray.direction, out raycastHit, maxDistance, bodyTurn.toGoalMask))
                    {
                        if (raycastHit.collider != null)
                        {
                            bodyTurn.shouldLook = false;
                        }
                    }
                    else
                    {
                        bodyTurn.shouldLook = true;
                        bodyTurn.lookTarget = goal;
                    }
                }
                else
                {
                    bodyTurn.shouldLook = false;
                }
                if (bodyTurn.shouldLook)
                {
                    float target = Vector3.Angle(((MonoBehaviour)bodyTurn).transform.forward, bodyTurn.lookTarget - ((MonoBehaviour)bodyTurn).transform.position);
                    bool flag = Vector3.Cross(((MonoBehaviour)bodyTurn).transform.forward, (bodyTurn.lookTarget - bodyTurn.monster.MoveControl.AheadNodePos).normalized).y > 0f;
                    float num = bodyTurn.angle / 45f;
                    bodyTurn.angle = Mathf.MoveTowards(bodyTurn.angle, target, Time.deltaTime * 40f);
                    bodyTurn.currentRatio = Mathf.MoveTowards(bodyTurn.currentRatio, num, 0.5f * Time.deltaTime);
                    if (num > 1f)
                    {
                    }
                    if (flag)
                    {
                        bodyTurn.angle = -bodyTurn.angle;
                    }
                }
                else
                {
                    bodyTurn.angle = Mathf.MoveTowards(bodyTurn.angle, 0f, Time.deltaTime * 180f);
                    bodyTurn.currentRatio = Mathf.MoveTowards(bodyTurn.currentRatio, 0f, 0.5f * Time.deltaTime);
                }
                bodyTurn.angle = 0f;
                bodyTurn.aniControl.BodyTurn = bodyTurn.angle;
                if (bodyTurn.animator.layerCount > 2)
                {
                    bodyTurn.animator.SetLayerWeight(2, bodyTurn.currentRatio);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @BoltCutters

            private static void HookBoltCuttersOnStartItemAnimation(On.BoltCutters.orig_OnStartItemAnimation orig, BoltCutters boltCutters)
            {
                BoltCutters.npc = InventoryFromItemClass(boltCutters.gameObject).newPlayerClass;
                orig.Invoke(boltCutters);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ChooseAttack

            public static void GetPlayerPose(GameObject player)
            {
                NewPlayerClass newPlayerClass = player.GetComponent<NewPlayerClass>();
                if (newPlayerClass.IsStanding() || newPlayerClass.IsPushing())
                {
                    ChooseAttack.playerPose = ChooseAttack.PlayerPose.Stand;
                }
                else if (newPlayerClass.IsCrouched())
                {
                    ChooseAttack.playerPose = ChooseAttack.PlayerPose.Crouch;
                }
                else
                {
                    ChooseAttack.playerPose = ChooseAttack.PlayerPose.Prone;
                }
            }

            public static void SetPlayerLayers(NewPlayerClass newPlayerClass)
            {
                // NewPlayerClass component = References.Player.GetComponent<NewPlayerClass>(); // Original code.
                PlayerAnimationLayersController component2 = newPlayerClass.GetComponent<PlayerAnimationLayersController>();
                int death = component2.Death;
                component2.MakeOnlyLayerActive(death);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ClimbUpPrompt

            // Last Player Checking Interactable Conditions Is Not Updated At On Hover Enter

            private static void HookClimbUpPromptOnHoverEnter(On.ClimbUpPrompt.orig_OnHoverEnter orig, ClimbUpPrompt climbUpPrompt)
            {
                int playerNumber = PlayerNumber(lastPlayerCheckingInteractableConditions.GetInstanceID());
                triggerObjectivesList[playerNumber].ClimbUpHint(true);
                if (triggerObjectivesList[playerNumber].cargoJumpOnce && OculusManager.isOculusEnabled)
                {
                    climbUpPrompt.oculusClimpPrompt.SetActive(true);
                }
            }

            private static void HookClimbUpPromptOnHoverExit(On.ClimbUpPrompt.orig_OnHoverExit orig, ClimbUpPrompt climbUpPrompt)
            {
                int playerNumber = PlayerNumber(lastPlayerCheckingInteractableConditions.GetInstanceID());
                triggerObjectivesList[playerNumber].ClimbUpHint(false);
                climbUpPrompt.oculusClimpPrompt.SetActive(false);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ConstraintControl

            private static void HookConstraintControlOnStateEnter(On.ConstraintControl.orig_OnStateEnter orig, ConstraintControl constraintControl, Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
            {
                animator.GetComponentInParent<NewPlayerClass>().Motor.UnlockConstraints();
            }

            private static void HookConstraintControlOnStateExit(On.ConstraintControl.orig_OnStateExit orig, ConstraintControl ConstraintControl, Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
            {
                animator.GetComponentInParent<NewPlayerClass>().Motor.LockConstraints();
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ContinuousDynamicController

            private static void HookContinuousDynamicControllerUpdate(On.ContinuousDynamicController.orig_Update orig, ContinuousDynamicController continuousDynamicController)
            {
                if (LevelGeneration.Instance.finishedGenerating && ColliderCheck.continuousRequired > 0 && Vector3.Distance(newPlayerClasses[ClosestPlayerToThis(continuousDynamicController.trans.position)].transform.position, continuousDynamicController.trans.position) < continuousDynamicController.distance)
                {
                    continuousDynamicController.SetCCD(CollisionDetectionMode.ContinuousDynamic);
                    continuousDynamicController.body.centerOfMass = Vector3.zero;
                }
                else
                {
                    continuousDynamicController.SetCCD(CollisionDetectionMode.Discrete);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Crane

            private static void HookCraneOnChainEnd(On.Crane.orig_OnChainEnd orig, Crane crane)
            {
                Crane.chainStarted = false;
                if (crane.spoolBox.spool != null)
                {
                    crane.chainAudio.Stop();
                    VirtualAudioSource virtualAudioSource = crane.chainAudio.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                    AudioSystem.PlaySound("Noises/Actions/Crane/Chain/Unload/End", ((MonoBehaviour)crane).transform, crane.chainAudio);
                }
            }

            private static void HookCraneOnChainStart(On.Crane.orig_OnChainStart orig, Crane crane)
            {
                crane.chainAudio.Stop();
                VirtualAudioSource virtualAudioSource = crane.chainAudio.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    virtualAudioSource.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                }
                if (crane.spoolBox.spool != null)
                {
                    AudioSystem.PlaySound("Noises/Actions/Crane/Chain/Unload/Start", ((MonoBehaviour)crane).transform, crane.chainAudio);
                }
                Crane.chainStarted = true;
            }

            private static void HookCraneOnChainStop(On.Crane.orig_OnChainStop orig, Crane crane)
            {
                crane.chainDirection = 0f;
                if (crane.spoolBox.spool != null && !Crane.chainStarted)
                {
                    crane.chainAudio.Stop();
                    VirtualAudioSource virtualAudioSource = crane.chainAudio.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                    AudioSystem.PlaySound("Noises/Actions/Crane/Chain/Unload/End", crane.chainAudio);
                    Crane.chainStarted = true;
                }
            }

            private static void HookCraneOnRotationStop(On.Crane.orig_OnRotationStop orig, Crane crane)
            {
                if (crane.rotationDirection != 0f)
                {
                    crane.rotationDirection = 0f;
                    crane.craneAudio.Stop();
                    VirtualAudioSource virtualAudioSource = crane.chainAudio.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                    AudioSystem.PlaySound("Noises/Actions/Crane/Movement/End", crane.craneAudio.transform, crane.craneAudio);
                    Crane.startup = true;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @CraneController

            private static void HookCraneControllerOnHandGrab(On.CraneController.orig_OnHandGrab orig, CraneController craneController)
            {
                lastPlayerSentMessage.mouseLook.UnlockPlayerHead();
                lastPlayerSentMessage.LockPlayerBody();
                if (craneController.rotationControl)
                {
                    if (craneController.reverse)
                    {
                        craneController.crane.OnRotateLeft();
                    }
                    else
                    {
                        craneController.crane.OnRotateRight();
                    }
                }
                else if (craneController.reverse)
                {
                    craneController.crane.OnChainDown();
                }
                else
                {
                    craneController.crane.OnChainUp();
                }
            }

            private static void HookCraneControllerOnHandRelease(On.CraneController.orig_OnHandRelease orig, CraneController craneController)
            {
                lastPlayerSentMessage.mouseLook.LockPlayerHead();
                lastPlayerSentMessage.UnlockPlayerBody();
                if (craneController.rotationControl)
                {
                    craneController.crane.OnRotationStop();
                }
                else
                {
                    craneController.crane.OnChainStop();
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @CraneHook

            private static void HookCraneHookOnHandGrab(On.CraneHook.orig_OnHandGrab orig, CraneHook craneHook)
            {
                if (!craneHook.dragging)
                {
                    craneHook.dragging = true;
                    ((MonoBehaviour)craneHook).StartCoroutine(DragHook(craneHook, lastPlayerSentMessage.gameObject));
                }
            }

            private static IEnumerator DragHook(CraneHook craneHook, GameObject playerGameObject)
            {
                while (craneHook.dragging)
                {
                    Vector3 force = playerGameObject.transform.position + Vector3.up * 1.6f + playerGameObject.transform.forward * 0.5f - playerGameObject.transform.right * 0.3f - ((MonoBehaviour)craneHook).transform.position;
                    force *= craneHook.dragForce;
                    ((MonoBehaviour)craneHook).transform.parent.GetComponent<Rigidbody>().AddForce(force, ForceMode.Acceleration);
                    yield return null;
                }
                craneHook.itemGrab = playerGameObject.GetComponent<ItemGrabIK>();
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @CraneSpoolBox

            private static void HookCraneSpoolBoxCheckForSpool(On.CraneSpoolBox.orig_CheckForSpool orig, CraneSpoolBox craneSpoolBox)
            {
                Inventory inventory = PlayerInventory(lastPlayerSentMessage);
                InventoryItem currentItem = inventory.CurrentItem;
                if (currentItem != null)
                {
                    Spool spool = currentItem.GetComponent<Spool>();
                    if (spool != null)
                    {
                        inventory.DropItem(inventory.CurrentSlot);
                        spool.AddToSpoolBox(craneSpoolBox);
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @CreakingSounds

            private static void HookCreakingSoundsStart(On.CreakingSounds.orig_Start orig, CreakingSounds creakingSounds)
            {
                orig.Invoke(creakingSounds);
                VirtualAudioSource virtualAudioSource = creakingSounds.source.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    virtualAudioSource.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                }
            }

            private static void HookCreakingSoundsUpdate(On.CreakingSounds.orig_Update orig, CreakingSounds creakingSounds)
            {
                creakingSounds.creakTimer += Time.deltaTime;
                CreakingSounds.CreakType creakType = creakingSounds.creaktype;
                if (creakType != CreakingSounds.CreakType.Ambience)
                {
                    if (creakType == CreakingSounds.CreakType.Submarine)
                    {
                        creakingSounds.timeBetweenCreaks = 30f;
                        creakingSounds.timeUntilCreak = 0f;
                        if (creakingSounds.source != null && !creakingSounds.source.isPlaying && creakingSounds.creakTimer > creakingSounds.timeBetweenCreaks)
                        {
                            creakingSounds.creakTimer = 0f;
                            AudioSystem.PlaySound("Noises/Submarine/SubCreaks", creakingSounds.source);
                        }
                    }
                }
                else
                {
                    creakingSounds.timeUntilCreak = 60f;
                    creakingSounds.timeBetweenCreaks = 30f;
                    if (creakingSounds.monster == null && References.Monster != null)
                    {
                        creakingSounds.monster = References.Monster.GetComponent<Monster>();
                    }
                    if (creakingSounds.monster != null && creakingSounds.monster.TimeOutVision.TimeElapsed > creakingSounds.timeUntilCreak)
                    {
                        if (creakingSounds.source != null && !creakingSounds.source.isPlaying && creakingSounds.creakTimer > creakingSounds.timeBetweenCreaks)
                        {
                            if (creakingSounds.monster.PlayerDetectRoom.GetRoomCategory != RoomCategory.Outside)
                            {
                                creakingSounds.creakTimer = 0f;
                                creakingSounds.randNum = UnityEngine.Random.Range(0, 10);
                                if (creakingSounds.randNum <= 5)
                                {
                                    AudioSystem.PlaySound("Noises/Atmosphere/Creaks", creakingSounds.source);
                                }
                                else
                                {
                                    AudioSystem.PlaySound("Noises/Atmosphere/Creaks/Metal Strain", creakingSounds.source);
                                }
                                creakingSounds.timeBetweenCreaks = 30f;
                            }
                            else
                            {
                                creakingSounds.creakTimer = 0f;
                            }
                        }
                    }
                    else
                    {
                        creakingSounds.source.Stop();
                        VirtualAudioSource virtualAudioSource = creakingSounds.source.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource != null)
                        {
                            virtualAudioSource.Stop();
                        }
                        else if (ModSettings.logDebugText)
                        {
                            Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                        }
                        creakingSounds.creakTimer = 0f;
                        creakingSounds.timeBetweenCreaks = 0f;
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DamageScript

            private static void HookDamageScriptOnTriggerEnter(On.DamageScript.orig_OnTriggerEnter orig, DamageScript damageScript, Collider Other)
            {
                if (Other.name == "PlayerDamage")
                {
                    NewPlayerClass newPlayerClass = Other.GetComponentInParent<NewPlayerClass>();

                    if (!ModSettings.enableCrewVSMonsterMode || (ModSettings.enableCrewVSMonsterMode && !monsterPlayers.Contains(newPlayerClass)))
                    {
                        damageScript.playerHealth = newPlayerClass.GetComponentInChildren<PlayerHealth>();

                        if (damageScript.playerHealth != null)
                        {
                            switch (damageScript.painFactor)
                            {
                                case DamageScript.PainFactor.OverTime:
                                    damageScript.shouldDamage = true;
                                    damageScript.overTime = true;
                                    damageScript.isAOEDamage = false;
                                    break;
                                case DamageScript.PainFactor.InstantHit:
                                    damageScript.shouldDamage = true;
                                    damageScript.overTime = false;
                                    damageScript.isAOEDamage = false;
                                    break;
                                case DamageScript.PainFactor.InstantKill:
                                    damageScript.playerHealth.InstantKill(damageScript.DmgType);
                                    break;
                                case DamageScript.PainFactor.AreaDamage:
                                    damageScript.shouldDamage = true;
                                    damageScript.overTime = true;
                                    damageScript.isAOEDamage = true;
                                    break;
                            }
                        }
                        else
                        {
                            Debug.Log("Unable to damage player");
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DeactivateSea

            private static void HookDeactivateSeaUpdate(On.DeactivateSea.orig_Update orig, DeactivateSea deactivateSea)
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    foreach (NewPlayerClass newPlayerClass in newPlayerClasses)
                    {
                        if (newPlayerClass.transform.position.y < 15f || PlayerInHeliCrate(newPlayerClass.transform.position))
                        {
                            deactivateSea.sea.SetActive(false);
                            return;
                        }
                    }
                    if (!deactivateSea.removeSea)
                    {
                        deactivateSea.sea.SetActive(true);
                    }
                }
            }

            private static bool PlayerInHeliCrate(Vector3 position)
            {
                return 176f < position.x && position.x < 188f && position.y <= 21f && ((104f < position.z && position.z < 110f) || (122f < position.z && position.z < 128f));
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DestroyOnFinish

            private static void HookDestroyOnFinishUpdate(On.DestroyOnFinish.orig_Update orig, DestroyOnFinish destroyOnFinish)
            {
                if (destroyOnFinish.audioSource == null)
                {
                    UnityEngine.Object.Destroy(((MonoBehaviour)destroyOnFinish).gameObject);
                }
                else if (!destroyOnFinish.audioSource.isPlaying && !destroyOnFinish.gamePaused)
                {
                    VirtualAudioSource virtualAudioSource = ((MonoBehaviour)destroyOnFinish).GetComponent<VirtualAudioSource>();
                    if (finishedAudioAssignment && virtualAudioSource != null)
                    {
                        virtualAudioSource = destroyOnFinish.audioSource.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource != null)
                        {
                            UnityEngine.Object.Destroy(virtualAudioSource.gameObject);
                        }
                        else if (ModSettings.logDebugText)
                        {
                            Debug.Log("VAS is null 2!\n" + new StackTrace().ToString());
                        }
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null 1!\n" + new StackTrace().ToString());
                    }
                    UnityEngine.Object.Destroy(((MonoBehaviour)destroyOnFinish).gameObject);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DetectPlayer

            private static void HookDetectPlayerUpdate(On.DetectPlayer.orig_Update orig, DetectPlayer detectPlayer)
            {
                if (detectPlayer.camRend.enabled)
                {
                    if (!detectPlayer.securityCamera.disarmed)
                    {
                        Vector3 detectPlayerPosition = detectPlayer.startPoint.position;
                        Vector3 playerPosition = detectPlayer.cameraModel.transform.position; // Placeholder position.
                        bool spottedPlayer = false;
                        if (newPlayerClasses != null)
                        {
                            foreach (NewPlayerClass newPlayerClass in crewPlayers)
                            {
                                playerPosition = PlayerCamera(newPlayerClass).transform.position;
                                if (GeoHelper.InsideCone(detectPlayerPosition, playerPosition, detectPlayer.cameraModel.transform.forward, detectPlayer.coneAngle, detectPlayer.coneLength))
                                {
                                    Vector3 vector = playerPosition - detectPlayerPosition;
                                    RaycastHit raycastHit;
                                    if (!Physics.Raycast(detectPlayerPosition, vector.normalized, out raycastHit, vector.magnitude, detectPlayer.wallsLayer))
                                    {
                                        spottedPlayer = true;
                                        break;
                                    }
                                }
                            }
                        }
                        if (spottedPlayer)
                        {
                            detectPlayer.simpleTilt.enabled = false;
                            detectPlayer.target = Quaternion.LookRotation(playerPosition - detectPlayer.cameraModel.transform.position);
                            detectPlayer.tiltObject.transform.rotation = Quaternion.Slerp(detectPlayer.tiltObject.transform.rotation, detectPlayer.target, 10f * Time.deltaTime);
                            detectPlayer.tiltObject.transform.rotation = Quaternion.Euler(detectPlayer.originalX, detectPlayer.tiltObject.transform.rotation.eulerAngles.y, detectPlayer.originalZ);
                            detectPlayer.playerFound = true;
                        }
                        else
                        {
                            detectPlayer.simpleTilt.enabled = true;
                            detectPlayer.tiltObject.transform.rotation = Quaternion.Euler(detectPlayer.originalX, detectPlayer.tiltObject.transform.rotation.eulerAngles.y, detectPlayer.originalZ);
                            detectPlayer.playerFound = false;
                        }
                    }
                }
                else
                {
                    detectPlayer.playerFound = false;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DetectRoom

            private static void HookDetectRoomStart(On.DetectRoom.orig_Start orig, DetectRoom detectRoom)
            {
                orig.Invoke(detectRoom);
                detectRoom.player = ((MonoBehaviour)detectRoom).GetComponentInParent<NewPlayerClass>();
                Debug.Log("DetectRoom player is " + PlayerNumber(detectRoom.player.GetInstanceID()));
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DisableSteamInteractionWhenCrouch

            private static void HookDisableSteamInteractionWhenCrouchUpdate(On.DisableSteamInteractionWhenCrouch.orig_Update orig, DisableSteamInteractionWhenCrouch disableSteamInteractionWhenCrouch)
            {
                if (newPlayerClasses != null)
                {
                    if (newPlayerClasses[ClosestPlayerToThis(disableSteamInteractionWhenCrouch.interact.transform.position)].IsStanding())
                    {
                        disableSteamInteractionWhenCrouch.interact.active = true;
                    }
                    else
                    {
                        disableSteamInteractionWhenCrouch.interact.active = false;
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DisplayOptions

            private static void HookDisplayOptionsHide(On.DisplayOptions.orig_Hide orig, DisplayOptions displayOptions)
            {

            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Door

            private static void HookDoorMovePlayer(On.Door.orig_MovePlayer orig, Door door)
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    foreach (NewPlayerClass player in crewPlayers)
                    {
                        float doorPushForceOverTime = door.pushPlayerForce * Time.deltaTime;
                        Vector3 playerToDoorDistance = player.gameObject.transform.position - door.doorCenter.transform.position;
                        playerToDoorDistance.y = 0f;
                        float playerToDoorAngle = Vector3.Angle(door.doorNormal.transform.forward, playerToDoorDistance.normalized);
                        Vector3 normalized = player.Motor.GetPlayerDirection().normalized;
                        if (playerToDoorDistance.magnitude < 1.2f) // Check magnitude in all cases and before other calculations. (Custom code to prevent an excessive amount of players to be moving.)
                        {
                            if (door.opening)
                            {
                                Ray ray = default(Ray);
                                ray.origin = player.gameObject.transform.position + Vector3.up;
                                ray.direction = door.frameCenter.position - ray.origin;
                                RaycastHit raycastHit;
                                if (door.doorCollider.Raycast(ray, out raycastHit, 1.2f))
                                {
                                    if (playerToDoorAngle <= 90f && (Vector3.Angle(normalized, ((MonoBehaviour)door).transform.forward * -1f) < 90f || normalized.magnitude == 0f))
                                    {
                                        if (ModSettings.logDebugText)
                                        {
                                            Debug.Log("Moving player number in door 1: " + PlayerNumber(player.GetInstanceID()));
                                        }
                                        doorPushForceOverTime = Mathf.Clamp(doorPushForceOverTime, 0f, (1.2f - raycastHit.distance) / 8f);
                                        player.controller.Move(((!door.pushBackFlip) ? 1f : -1f) * ((MonoBehaviour)door).gameObject.transform.forward * doorPushForceOverTime);
                                        player.Motor.disableTime = 0.2f;
                                    }
                                }
                            }
                            else if ((playerToDoorAngle >= 90f || (door.isCrouchDoor && playerToDoorAngle <= 90f)) && door.doorPushPlayer != null && door.doorPushPlayer.pushPlayer)
                            {
                                if (door.doorPushPlayer.pushPlayerBack)
                                {
                                    if (Vector3.Angle(normalized, ((MonoBehaviour)door).transform.forward * 1f) < 90f || normalized.magnitude == 0f)
                                    {
                                        Vector3 playerPosition = player.gameObject.transform.position;
                                        Vector3 positionToMoveTo = Vector3.zero;
                                        if (door.moveToTransform == null)
                                        {
                                            if ((door.isCrouchDoor && player.playerState == NewPlayerClass.PlayerState.Crouched) || (!door.isCrouchDoor && player.playerState == NewPlayerClass.PlayerState.Standing))
                                            {
                                                positionToMoveTo = door.doorCenter.transform.position - door.doorNormal.transform.forward * door.pushBackDistance;
                                            }
                                        }
                                        else if ((door.isCrouchDoor && player.playerState == NewPlayerClass.PlayerState.Crouched) || (!door.isCrouchDoor && player.playerState == NewPlayerClass.PlayerState.Standing))
                                        {
                                            positionToMoveTo = door.moveToTransform.position;
                                        }
                                        if (Vector3.zero != positionToMoveTo)
                                        {
                                            positionToMoveTo.y = playerPosition.y;
                                            if (ModSettings.logDebugText)
                                            {
                                                Debug.Log("Moving player number in door 2: " + PlayerNumber(player.GetInstanceID()));
                                            }
                                            player.controller.Move((positionToMoveTo - player.gameObject.transform.position).normalized * doorPushForceOverTime);
                                        }
                                        player.Motor.Halt();
                                        player.Motor.ClampYVelocity(2f);
                                        player.Motor.disableTime = 0.2f;
                                    }
                                }
                                else if (player.gameObject.GetComponentInChildren<DetectRoom>().GetRoom == door.attachedRoom || door.attachedRoom == null)
                                {
                                    if (ModSettings.logDebugText)
                                    {
                                        Debug.Log("Moving player number in door 3: " + PlayerNumber(player.GetInstanceID()));
                                    }
                                    Vector3 playerPosition = player.gameObject.transform.position;
                                    Vector3 a = playerPosition - door.doorNormal.right;
                                    a.y = playerPosition.y;
                                    player.controller.Move((a - player.gameObject.transform.position).normalized * doorPushForceOverTime);
                                    player.Motor.Halt();
                                    player.Motor.ClampYVelocity(2f);
                                    player.Motor.disableTime = 0.2f;
                                }
                            }
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DoorPushPlayer

            private static void HookDoorPushPlayerDeterminePushDirection(On.DoorPushPlayer.orig_DeterminePushDirection orig, DoorPushPlayer doorPushPlayer)
            {
                doorPushPlayer.pushPlayerBack = true;
                if (doorPushPlayer.comparisonPoint == null)
                {
                    return;
                }
                switch (doorPushPlayer.attachedDoor.rotationQuadrant)
                {
                    case 0:
                        if (doorPushPlayer.XZMag(lastPlayerSentMessage.gameObject) > doorPushPlayer.XZMag(doorPushPlayer.comparisonPoint))
                        {
                            doorPushPlayer.pushPlayerBack = false;
                        }
                        break;
                    case 1:
                        if (doorPushPlayer.XZMag(lastPlayerSentMessage.gameObject) > doorPushPlayer.XZMag(doorPushPlayer.comparisonPoint))
                        {
                            doorPushPlayer.pushPlayerBack = false;
                        }
                        break;
                    case 2:
                        if (doorPushPlayer.XZMag(lastPlayerSentMessage.gameObject) < doorPushPlayer.XZMag(doorPushPlayer.comparisonPoint))
                        {
                            doorPushPlayer.pushPlayerBack = false;
                        }
                        break;
                    case 3:
                        if (doorPushPlayer.XZMag(lastPlayerSentMessage.gameObject) < doorPushPlayer.XZMag(doorPushPlayer.comparisonPoint))
                        {
                            doorPushPlayer.pushPlayerBack = false;
                        }
                        break;
                }
                if (doorPushPlayer.inverse)
                {
                    doorPushPlayer.pushPlayerBack = !doorPushPlayer.pushPlayerBack;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DraggableObject

            private static Vector3 DragPoint(DraggableObject draggableObject, NewPlayerClass newPlayerClass)
            {
                Transform dragMarkTransform = newPlayerClass.GetComponentInChildren<DragMark>().transform;
                Vector3 dragPointAccountingForPlayer = dragMarkTransform.position + dragMarkTransform.forward * draggableObject.offset.z + dragMarkTransform.right * draggableObject.offset.x + dragMarkTransform.up * draggableObject.offset.y;
                return dragPointAccountingForPlayer;
            }

            private static void HookDraggableObjectOnInteract(On.DraggableObject.orig_OnInteract orig, DraggableObject draggableObject)
            {
                draggableObject.dragging = true;
                draggableObject.allowTimer = false;
                draggableObject.beenDragged = true;
                ((MonoBehaviour)draggableObject).StartCoroutine(Dragging(draggableObject, lastPlayerSentMessage));
            }

            private static IEnumerator Dragging(DraggableObject draggableObject, NewPlayerClass newPlayerClass)
            {
                while (draggableObject.dragging)
                {
                    RaycastHit hit;
                    if (((MonoBehaviour)draggableObject).GetComponent<Collider>().bounds.center.y < newPlayerClass.transform.position.y)
                    {
                        draggableObject.OnRelease();
                    }
                    else if (!Physics.Raycast(newPlayerClass.transform.position + Vector3.up * 0.2f, Vector3.down, out hit, 0.7f, 1 << LayerMask.NameToLayer("DefaultNavVision")))
                    {
                        draggableObject.OnRelease();
                    }
                    else if (GetPlayerKey("Jump", PlayerNumber(newPlayerClass.GetInstanceID())).IsDown())
                    {
                        draggableObject.OnRelease();
                    }
                    else
                    {
                        if (draggableObject.setToKinematic)
                        {
                            draggableObject.body.isKinematic = true;
                        }
                        if (draggableObject.freezeRotation)
                        {
                            draggableObject.body.constraints = RigidbodyConstraints.FreezeRotation;
                        }
                        Vector3 pos = DragPoint(draggableObject, newPlayerClass);
                        float distance = Vector3.Distance(pos, draggableObject.body.position);
                        Vector3 direction = (pos - draggableObject.body.position).normalized;
                        if (draggableObject.mode == DraggableObject.DragMode.Move)
                        {
                            ((MonoBehaviour)draggableObject).transform.position = Vector3.MoveTowards(((MonoBehaviour)draggableObject).transform.position, pos, draggableObject.maxSpeed * Mathf.Lerp(0f, 1f, distance * 2f));
                        }
                        if (draggableObject.mode == DraggableObject.DragMode.Force)
                        {
                            draggableObject.body.velocity = direction * draggableObject.maxSpeed * Mathf.Lerp(0f, 1f, distance * 2f);
                        }
                        yield return null;
                    }
                }
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DraggedOutHiding

            private static void HookDraggedOutHidingStart(On.DraggedOutHiding.orig_Start orig, DraggedOutHiding draggedOutHiding)
            {
                draggedOutHiding.player = ((MonoBehaviour)draggedOutHiding).GetComponentInParent<NewPlayerClass>();

                if (draggedOutHiding.player.playerAnimator != null)
                {
                    draggedOutHiding.playerAnimator = draggedOutHiding.player.playerAnimator;
                    Debug.Log("Dragged out hiding player animator instance ID is: " + draggedOutHiding.player.playerAnimator.GetInstanceID());
                }
                else
                {
                    Debug.Log("Player number " + PlayerNumber(draggedOutHiding.player.GetInstanceID()) + "'s playerAnimator is still null in DraggedOutHidingStart");
                }

                if (ManyMonstersMode.lastMonsterSentMessage != null)
                {
                    draggedOutHiding.monster = ManyMonstersMode.lastMonsterSentMessage;
                }
                else
                {
                    draggedOutHiding.monster = References.Monster.GetComponent<Monster>();
                }
            }

            private static void HookDraggedOutHidingLockPlayer(On.DraggedOutHiding.orig_LockPlayer orig, DraggedOutHiding draggedOutHiding)
            {
                try
                {
                    draggedOutHiding.player.IsGrabbed = true;
                    draggedOutHiding.player.movePlayerUnderBed = false;
                }
                catch
                {
                    Debug.Log("Exception in MP DOH 1");
                }
                try
                {
                    Debug.Log("Switching off gravity of player number " + PlayerNumber(draggedOutHiding.player.GetInstanceID()) + " in dragged out hiding");
                    draggedOutHiding.player.Motor.useGravity = false;
                    draggedOutHiding.player.Motor.isHaulted = true;
                    draggedOutHiding.player.Motor.disableMove = true;
                }
                catch
                {
                    Debug.Log("Exception in MP DOH 2");
                }
                try
                {
                    draggedOutHiding.player.LockEverything();
                }
                catch
                {
                    Debug.Log("Exception in MP DOH 3");
                }
                try
                {
                    draggedOutHiding.player.GetComponent<Rigidbody>().isKinematic = true;
                }
                catch
                {
                    Debug.Log("Exception in MP DOH 4");
                }
                try
                {
                    if (draggedOutHiding.playerAnimator == null)
                    {
                        draggedOutHiding.playerAnimator = draggedOutHiding.player.playerAnimator;
                        if (draggedOutHiding.playerAnimator == null)
                        {
                            Debug.Log("DOH animator is still null");
                            draggedOutHiding.player.playerAnimator = draggedOutHiding.player.GetComponent<Animator>();
                            draggedOutHiding.playerAnimator = draggedOutHiding.player.playerAnimator;

                        }
                    }
                    if (draggedOutHiding.playerAnimator == null)
                    {
                        Debug.Log("DOH animator is still null after another try");
                    }
                    else
                    {
                        Debug.Log("DOH animator is not null in LockPlayer");
                        draggedOutHiding.playerAnimator.applyRootMotion = true;
                    }
                }
                catch
                {
                    Debug.Log("Exception in MP DOH 5");
                }
                try
                {
                    PlayerInventory(draggedOutHiding.player).hideItem = true;
                }
                catch
                {
                    Debug.Log("Exception in MP DOH 6");
                }
                try
                {
                    PlayerUpperBodyLock playerUpperBodyLock = draggedOutHiding.player.GetComponentInChildren<PlayerUpperBodyLock>();
                    if (playerUpperBodyLock != null)
                    {
                        playerUpperBodyLock.weighting = 0f;
                        playerUpperBodyLock.weighting2 = 0f;
                        playerUpperBodyLock.weighting3 = 0f;
                    }
                }
                catch
                {
                    Debug.Log("Exception in MP DOH 7");
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DragPlayer

            private static void HookDragPlayerMec_OnGrabPlayer(On.DragPlayer.orig_Mec_OnGrabPlayer orig, DragPlayer dragPlayer)
            {
                dragPlayer.finished = false;
                DraggedOutHiding draggedOutHiding = dragPlayer.monster.player.GetComponent<DraggedOutHiding>();
                draggedOutHiding.player = dragPlayer.monster.PlayerDetectRoom.player;
                draggedOutHiding.playerAnimator = draggedOutHiding.player.GetComponent<Animator>();
                draggedOutHiding.DragPlayer();
            }

            public static bool HookDragPlayerPullOutPlayerIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!ManyMonstersMode.IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookDragPlayerPullOutPlayer((DragPlayer)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    ManyMonstersMode.IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookDragPlayerPullOutPlayer(DragPlayer dragPlayer)
            {
                Debug.Log("This hook is working!: " + new StackTrace().ToString()); // # IEnumerator Hook Test
                Debug.Log("DragPlayer.PullOutPlayer() is being called! [MESM Multiplayer]");

                Vector3 pos = ((MonoBehaviour)dragPlayer).transform.position;
                NewPlayerClass newPlayerClass = dragPlayer.monster.player.GetComponent<NewPlayerClass>();
                Vector3 startPos = newPlayerClass.transform.position;
                HidingSpot spot = ((MonoBehaviour)dragPlayer).GetComponent<HidingSpot>();
                if (spot != null)
                {
                    pos = spot.MonsterPoint;
                }
                Vector3 targetAngle = ((MonoBehaviour)dragPlayer).transform.forward;
                if (spot != null)
                {
                    targetAngle = spot.MonsterNormal;
                }
                Vector3 lerpPoint = newPlayerClass.transform.position - spot.MonsterNormal;
                while (!dragPlayer.finished)
                {
                    float angle = Vector3.Angle(newPlayerClass.transform.forward, targetAngle);
                    dragPlayer.t += Time.deltaTime * 4f;
                    newPlayerClass.movePlayerUnderBed = false;
                    Vector3 pos2 = pos - newPlayerClass.transform.position;
                    pos2.y = 0f;
                    pos2.Normalize();
                    if (angle > 40f)
                    {
                        newPlayerClass.transform.rotation = Quaternion.RotateTowards(newPlayerClass.transform.rotation, Quaternion.LookRotation(pos2, Vector3.up), Time.deltaTime * 270f);
                    }
                    newPlayerClass.transform.position = Vector3.Lerp(startPos, lerpPoint, dragPlayer.t);
                    yield return null;
                }
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Drawer

            private static void HookDrawerOpenDrawer(On.Drawer.orig_OpenDrawer orig, Drawer drawer)
            {
                drawer.timer += Time.deltaTime;
                drawer.newPos.z = drawer.timer;
                drawer.drawerModel.transform.localPosition = drawer.newPos;
                foreach (NewPlayerClass newPlayerClass in crewPlayers)
                {
                    if (newPlayerClass.playerState == NewPlayerClass.PlayerState.Crouched)
                    {
                        float playerToDrawerAngle = Vector3.Angle(newPlayerClass.gameObject.transform.forward, ((MonoBehaviour)drawer).gameObject.transform.forward);
                        float playerToDrawerDistance = Vector3.Distance(newPlayerClass.gameObject.transform.position, ((MonoBehaviour)drawer).gameObject.transform.position);
                        if (playerToDrawerAngle >= 150f && playerToDrawerDistance <= 0.8f)
                        {
                            newPlayerClass.controller.Move(((MonoBehaviour)drawer).gameObject.transform.forward / 50f);
                        }
                    }
                }
                if (drawer.drawerModel.transform.localPosition.z >= drawer.openDistance)
                {
                    drawer.open = true;
                    drawer.newPos = drawer.drawerModel.transform.localPosition;
                    drawer.timer = drawer.newPos.z;
                    drawer.interact = false;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DualSteamVent

            private static void HookDualSteamVentOnStartFixedAnimation(On.DualSteamVent.orig_OnStartFixedAnimation orig, DualSteamVent dualSteamVent)
            {
                //lastPlayerSentMessage.ValveValue = ((!dualSteamVent.isLeft) ? -1f : 1f);
                foreach (NewPlayerClass newPlayerClass in newPlayerClasses)
                {
                    newPlayerClass.ValveValue = ((!dualSteamVent.isLeft) ? -1f : 1f);
                }
                ((MonoBehaviour)dualSteamVent).StartCoroutine(dualSteamVent.TurningValve());
            }

            private static void HookDualSteamVentDisableSteamVent(On.DualSteamVent.orig_DisableSteamVent orig, DualSteamVent dualSteamVent, DualSteamVent.SteamVent _vent)
            {
                for (int i = 0; i < _vent.renderer.Length; i++)
                {
                    ParticleSystem.EmissionModule dualSteamVentParticleSystemEmissionModule = _vent.renderer[i].emission;
                    dualSteamVentParticleSystemEmissionModule.enabled = false;
                }
                _vent.steamSound.Stop();
                VirtualAudioSource virtualAudioSource = _vent.steamSound.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    virtualAudioSource.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                }
                dualSteamVent.SetColliderEnabled(_vent.colliders, false);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DuctTape

            private static void HookDuctTapeOnFinishItemAnimation(On.DuctTape.orig_OnFinishItemAnimation orig, DuctTape ductTape)
            {
                if (ModSettings.addAdditionalCrewDeckBuilding)
                {
                    foreach (Liferaft liferaft in ModSettings.liferafts)
                    {
                        DuctTape.lifeRaft = liferaft;
                        MultiplayerDuctTapeDeletionCheck(ductTape);
                    }
                }
                else
                {
                    MultiplayerDuctTapeDeletionCheck(ductTape);
                }
            }

            private static void MultiplayerDuctTapeDeletionCheck(DuctTape ductTape)
            {
                if (Vector3.Distance(DuctTape.lifeRaft.transform.position, ((MonoBehaviour)ductTape).gameObject.transform.position) <= 3f)
                {
                    Inventory inventory = InventoryFromItemClass(ductTape.gameObject);
                    inventory.DestroyItem(inventory.CurrentSlot);
                }
                else
                {
                    ductTape.reset = true;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @EggTimer

            private static void HookEggTimerDestroyTimer(On.EggTimer.orig_DestroyTimer orig, EggTimer eggTimer)
            {
                eggTimer.timeUpSource.Stop();
                if (!useLegacyAudio)
                {
                    VirtualAudioSource virtualAudioSource = eggTimer.timeUpSource.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                }
                eggTimer.destroyed = true;
                DestroyableObject component = ((MonoBehaviour)eggTimer).transform.GetComponent<DestroyableObject>();
                Inventory inventory = InventoryFromItemClass(eggTimer.gameObject);
                if (inventory.CurrentItem.gameObject == ((MonoBehaviour)eggTimer).gameObject)
                {
                    inventory.DropItem(inventory.CurrentSlot);
                }
                if (component != null)
                {
                    component.DestroyObject();
                }
            }

            private static void HookEggTimerReset(On.EggTimer.orig_Reset orig, EggTimer eggTimer)
            {
                orig.Invoke(eggTimer);
                VirtualAudioSource virtualAudioSource1 = eggTimer.tickingSource.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource1 != null)
                {
                    virtualAudioSource1.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null 1!\n" + new StackTrace().ToString());
                }
                VirtualAudioSource virtualAudioSource2 = eggTimer.timeUpSource.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource2 != null)
                {
                    virtualAudioSource2.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null 2!\n" + new StackTrace().ToString());
                }
            }

            private static void HookEggTimerTimeUp(On.EggTimer.orig_TimeUp orig, EggTimer eggTimer)
            {
                VirtualAudioSource virtualAudioSource = eggTimer.tickingSource.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    virtualAudioSource.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                }
                orig.Invoke(eggTimer);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @EscapeChecker

            private static void HookEscapeCheckerUpdate(On.EscapeChecker.orig_Update orig, EscapeChecker escapeChecker)
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    if (References.Monster != null && escapeChecker.monEffect == null)
                    {
                        escapeChecker.monEffect = References.Monster.GetComponent<MonsterEffectiveness>();
                    }
                    foreach (Inventory inventory in inventories)
                    {
                        if (inventory != null)
                        {
                            if (inventory.ItemCount != escapeChecker.itemsHeld)
                            {
                                if (escapeChecker.itemsHeld < inventory.ItemCount)
                                {
                                    escapeChecker.WhatItem(inventory.GetItemInSlot<InventoryItem>(escapeChecker.itemsHeld));
                                }
                                escapeChecker.itemsHeld = inventory.ItemCount;
                            }
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FiendDoorSlam

            private static List<Door> HookFiendDoorSlamAcquireCorridorDoors(On.FiendDoorSlam.orig_AcquireCorridorDoors orig, Vector3 _worldPos)
            {
                List<Door> doorList = new List<Door>();
                Vector3 monsterRegionNodeVector = RegionManager.Instance.ConvertPointToRegionNode(_worldPos);
                Vector3 twoDimensionalVectorToPlayer = newPlayerClasses[ClosestPlayerToThis(_worldPos)].transform.position - _worldPos;
                bool useAbilityVersion = false;
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Fiend acquire corridor doors default target vector is: " + twoDimensionalVectorToPlayer);
                }
                if (ModSettings.enableCrewVSMonsterMode && !CrewVsMonsterMode.letAIControlMonster)
                {
                    int fiendNumber = 0;
                    for (int i = 0; i < ManyMonstersMode.fiendsMonsterComponents.Count; i++)
                    {
                        if (ManyMonstersMode.fiendsMonsterComponents[i].transform.position == _worldPos)
                        {
                            fiendNumber = i;
                            break;
                        }
                    }
                    int monsterNumber = ManyMonstersMode.MonsterNumber(ManyMonstersMode.fiendsMonsterComponents[fiendNumber].GetInstanceID());
                    if (monsterNumber < MultiplayerMode.monsterPlayers.Count && CrewVsMonsterMode.monsterUsingActiveAbility[monsterNumber])
                    {
                        DetectPlayer detectPlayer = FindObjectOfType<DetectPlayer>();
                        if (detectPlayer != null)
                        {
                            Vector3 monsterPosition = monsterPlayers[monsterNumber].transform.position;//ManyMonstersMode.fiendsMonsterComponents[fiendNumber].transform.position;
                            Vector3 monsterForwardPosition = monsterPlayers[monsterNumber].transform.forward;//ManyMonstersMode.fiendsMonsterComponents[fiendNumber].transform.forward;
                            Vector3 vector = monsterForwardPosition - monsterPosition;
                            RaycastHit raycastHit;
                            Physics.Raycast(monsterPosition, vector.normalized, out raycastHit, float.MaxValue, detectPlayer.wallsLayer);
                            if (raycastHit.point != null)
                            {
                                twoDimensionalVectorToPlayer = raycastHit.point /*- _worldPos*/;
                                useAbilityVersion = true;
                                if (ModSettings.logDebugText)
                                {
                                    Debug.Log("Found where Fiend player is looking: " + twoDimensionalVectorToPlayer);
                                }
                            }
                        }
                    }
                }
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Fiend acquire corridor doors target vector is: " + twoDimensionalVectorToPlayer);
                }

                if (!useAbilityVersion)
                {
                    twoDimensionalVectorToPlayer.Normalize();
                    twoDimensionalVectorToPlayer.x = (float)Mathf.RoundToInt(twoDimensionalVectorToPlayer.x);
                    twoDimensionalVectorToPlayer.z = (float)Mathf.RoundToInt(twoDimensionalVectorToPlayer.z);
                    twoDimensionalVectorToPlayer.y = 0f;
                    Vector3 monsterRegionNodeVectorCopy = monsterRegionNodeVector;
                    NodeData nodeDataAtMonsterRegionNodeVectorCopy = LevelGeneration.Instance.nodeData[(int)monsterRegionNodeVectorCopy.x][(int)monsterRegionNodeVectorCopy.y][(int)monsterRegionNodeVectorCopy.z];
                    int corridorPiecesChecked = 0;
                    int timesExpanded = 0;
                    int directionToPlayer = 0;
                    if (twoDimensionalVectorToPlayer.x == 1f)
                    {
                        directionToPlayer = 3;
                    }
                    if (twoDimensionalVectorToPlayer.x == -1f)
                    {
                        directionToPlayer = 2;
                    }
                    if (twoDimensionalVectorToPlayer.z == 1f)
                    {
                        directionToPlayer = 5;
                    }
                    if (twoDimensionalVectorToPlayer.z == -1f)
                    {
                        directionToPlayer = 4;
                    }
                    bool directionCheck = nodeDataAtMonsterRegionNodeVectorCopy.connectedNodesUDLRFB[directionToPlayer];
                    if (ModSettings.logDebugText)
                    {
                        Debug.Log("Fiend acquire doors direction to target is " + directionToPlayer + " and direction check has passed? " + directionCheck);
                    }
                    if (twoDimensionalVectorToPlayer != Vector3.zero)
                    {
                        if (ModSettings.logDebugText)
                        {
                            if (nodeDataAtMonsterRegionNodeVectorCopy.nodeRoom != null)
                            {
                                Debug.Log("BEFORE: Is node within ship bounds? " + CheckBoundariesLG.NodeWithinShipBounds(monsterRegionNodeVectorCopy) + ". Is node room not null? " + (nodeDataAtMonsterRegionNodeVectorCopy.nodeRoom != null) + ". Is node room type a corridor? " + (nodeDataAtMonsterRegionNodeVectorCopy.nodeRoom.RoomType == RoomStructure.Corridor) + ". Did the direction check pass? " + directionCheck);
                            }
                            else
                            {
                                Debug.Log("BEFORE: Is node within ship bounds? " + CheckBoundariesLG.NodeWithinShipBounds(monsterRegionNodeVectorCopy) + ". Is node room not null? " + (nodeDataAtMonsterRegionNodeVectorCopy.nodeRoom != null));
                            }
                        }
                        while (CheckBoundariesLG.NodeWithinShipBounds(monsterRegionNodeVectorCopy) && nodeDataAtMonsterRegionNodeVectorCopy.nodeRoom != null && nodeDataAtMonsterRegionNodeVectorCopy.nodeRoom.RoomType == RoomStructure.Corridor && directionCheck)
                        {
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("Got inside of corridor check of acquire doors");
                            }
                            if (timesExpanded > 3)
                            {
                                if (ModSettings.logDebugText)
                                {
                                    Debug.Log("Got inside of 3 run times limited corridor check of acquire doors");
                                }
                                if (nodeDataAtMonsterRegionNodeVectorCopy.nodeRoom.RoomConnectionsType == ConnectorType.Corner && nodeDataAtMonsterRegionNodeVectorCopy.attachedDoors.Count == 0)
                                {
                                    corridorPiecesChecked++;
                                }
                                if (nodeDataAtMonsterRegionNodeVectorCopy.nodeRoom.RoomConnectionsType == ConnectorType.TJunction)
                                {
                                    if (nodeDataAtMonsterRegionNodeVectorCopy.attachedDoors.Count == 0 && !directionCheck)
                                    {
                                        corridorPiecesChecked += 2;
                                    }
                                    else if (nodeDataAtMonsterRegionNodeVectorCopy.attachedDoors.Count == 0)
                                    {
                                        corridorPiecesChecked++;
                                    }
                                    if (nodeDataAtMonsterRegionNodeVectorCopy.attachedDoors.Count == 1)
                                    {
                                        corridorPiecesChecked++;
                                    }
                                }
                                if (nodeDataAtMonsterRegionNodeVectorCopy.nodeRoom.RoomConnectionsType == ConnectorType.FourWay)
                                {
                                    if (nodeDataAtMonsterRegionNodeVectorCopy.attachedDoors.Count == 0)
                                    {
                                        corridorPiecesChecked += 2;
                                    }
                                    if (nodeDataAtMonsterRegionNodeVectorCopy.attachedDoors.Count == 1)
                                    {
                                        corridorPiecesChecked++;
                                    }
                                    /*
                                    if (nodeData.attachedDoors.Count == 2)
                                    {
                                        num = num; // Pointless original code.
                                    }
                                    */
                                }
                                for (int i = 0; i < nodeDataAtMonsterRegionNodeVectorCopy.attachedDoors.Count; i++)
                                {
                                    if (nodeDataAtMonsterRegionNodeVectorCopy.attachedDoors[i].DoorType != Door.doorType.Sealed && nodeDataAtMonsterRegionNodeVectorCopy.attachedDoors[i].DoorType != Door.doorType.Barricaded && nodeDataAtMonsterRegionNodeVectorCopy.attachedDoors[i].DoorType != Door.doorType.Ripable && nodeDataAtMonsterRegionNodeVectorCopy.attachedDoors[i].DoorType != Door.doorType.Container)
                                    {
                                        doorList.Add(nodeDataAtMonsterRegionNodeVectorCopy.attachedDoors[i]);
                                    }
                                }
                            }
                            monsterRegionNodeVectorCopy += twoDimensionalVectorToPlayer;
                            nodeDataAtMonsterRegionNodeVectorCopy = LevelGeneration.Instance.nodeData[(int)monsterRegionNodeVectorCopy.x][(int)monsterRegionNodeVectorCopy.y][(int)monsterRegionNodeVectorCopy.z];
                            directionCheck = nodeDataAtMonsterRegionNodeVectorCopy.connectedNodesUDLRFB[directionToPlayer];
                            timesExpanded++;
                        }
                        if (ModSettings.logDebugText)
                        {
                            if (nodeDataAtMonsterRegionNodeVectorCopy.nodeRoom != null)
                            {
                                Debug.Log("AFTER: Is node within ship bounds? " + CheckBoundariesLG.NodeWithinShipBounds(monsterRegionNodeVectorCopy) + ". Is node room not null? " + (nodeDataAtMonsterRegionNodeVectorCopy.nodeRoom != null) + ". Is node room type a corridor? " + (nodeDataAtMonsterRegionNodeVectorCopy.nodeRoom.RoomType == RoomStructure.Corridor) + ". Did the direction check pass? " + directionCheck);
                            }
                            else
                            {
                                Debug.Log("AFTER: Is node within ship bounds? " + CheckBoundariesLG.NodeWithinShipBounds(monsterRegionNodeVectorCopy) + ". Is node room not null? " + (nodeDataAtMonsterRegionNodeVectorCopy.nodeRoom != null));
                            }
                        }
                    }
                }
                else
                {
                    DetectPlayer detectPlayer = FindObjectOfType<DetectPlayer>();
                    if (detectPlayer != null)
                    {
                        Vector3 raycastPoint = twoDimensionalVectorToPlayer;
                        int raycastPointDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(raycastPoint).y;
                        if (ModSettings.logDebugText)
                        {
                            Debug.Log("Raycast point is " + raycastPoint + " and monster point is " + _worldPos);
                        }
                        foreach (Door door in FindObjectsOfType<Door>())
                        {
                            if (door.DoorType != Door.doorType.Sealed && door.DoorType != Door.doorType.Barricaded && door.DoorType != Door.doorType.Ripable && door.DoorType != Door.doorType.Container && Vector3.Distance(raycastPoint, door.transform.position) < ModSettings.fiendAbilityDoorLockRange)
                            {
                                int doorDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(door.transform.position).y;
                                if (raycastPointDeck == doorDeck)
                                {
                                    /*
                                    Vector3 doorPosition = door.transform.position;
                                    Vector3 doorToRaycastPoint = raycastPoint - doorPosition;
                                    RaycastHit raycastHit;
                                    if (!Physics.Raycast(raycastPoint, doorToRaycastPoint.normalized, out raycastHit, doorToRaycastPoint.magnitude, detectPlayer.wallsLayer))
                                    {
                                        doorList.Add(door); // If there is no wall between the door and the raycast point, add the door to the doors list.
                                    }
                                    */
                                    doorList.Add(door);
                                }
                            }
                        }
                    }


                    /*
                    for (int x = (int)monsterRegionNodeVector.x - 10; x < monsterRegionNodeVector.x + 10; x++)
                    {
                        for (int z = (int)monsterRegionNodeVector.z - 10; z < monsterRegionNodeVector.z + 10; z++)
                        {
                            if (CheckBoundariesLG.NodeWithinShipBounds(new Vector3(x, (int)monsterRegionNodeVector.y, z)))
                            {
                                NodeData nodeData = LevelGeneration.Instance.nodeData[x][(int)monsterRegionNodeVector.y][z];

                                if (nodeData != null && nodeData.nodeRoom != null && nodeData.nodeRoom.roomDoors != null)
                                {
                                    foreach (Door door in nodeData.nodeRoom.roomDoors)
                                    {
                                        DetectPlayer detectPlayer = FindObjectOfType<DetectPlayer>();
                                        if (detectPlayer != null)
                                        {
                                            Vector3 doorPosition = door.transform.position;
                                            Vector3 raycastPoint = twoDimensionalVectorToPlayer;
                                            Vector3 doorToRaycastPoint = raycastPoint - doorPosition;
                                            RaycastHit raycastHit;
                                            if (!Physics.Raycast(raycastPoint, doorToRaycastPoint.normalized, out raycastHit, doorToRaycastPoint.magnitude, detectPlayer.wallsLayer))
                                            {
                                                doorList.Add(door); // If there is no wall between the door and the raycast point, add the door to the doors list.
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    */
                }
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Door list count at end of acquire doors is: " + doorList.Count);
                }
                return doorList;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FiendMindAttack

            private static void HookFiendMindAttackUpdate(On.FiendMindAttack.orig_Update orig, FiendMindAttack fiendMindAttack)
            {
                if (MonsterStarter.spawned)
                {
                    if (fiendMindAttack.playerHealth.NPC.gameObject != fiendMindAttack.monster.player)
                    {
                        int oldPlayerNumber = PlayerNumber(fiendMindAttack.playerHealth.NPC.GetInstanceID());
                        int newPlayerNumber = PlayerNumber(fiendMindAttack.monster.PlayerDetectRoom.player.GetInstanceID());
                        int monsterNumber = ManyMonstersMode.MonsterNumber(fiendMindAttack.monster.GetInstanceID());
                        ManyMonstersMode.fiendMindAttackFiendsTargetingPlayer[oldPlayerNumber].Remove(monsterNumber);
                        ManyMonstersMode.fiendMindAttackFiendsTargetingPlayer[newPlayerNumber].Add(monsterNumber);
                        fiendMindAttack.playerHealth = fiendMindAttack.monster.player.GetComponentInChildren<PlayerHealth>();
                        fiendMindAttack.mindAttackBleed = fiendMindAttack.monster.player.GetComponentInChildren<MindAttackEffect>();
                        fiendMindAttack.oculusMindAttackBleed = fiendMindAttack.monster.player.GetComponentInChildren<OculusMindAttackEffect>();
                        Debug.Log("Switched FiendMindAttack player to player number " + PlayerNumber(fiendMindAttack.playerHealth.NPC.GetInstanceID()));
                    }
                    if (ModSettings.logDebugText)
                    {
                        Debug.Log("Fiend Mind Attack player bleed from monster number " + ManyMonstersMode.MonsterNumber(fiendMindAttack.monster.GetInstanceID()) + " is enabled? " + fiendMindAttack.mindAttackBleed.enabled);
                    }
                    orig.Invoke(fiendMindAttack);
                    if (!useLegacyAudio)
                    {
                        if (fiendMindAttack.chargeSource != null)
                        {
                            VirtualAudioSource virtualAudioSource = fiendMindAttack.chargeSource.gameObject.GetComponent<VirtualAudioSource>();
                            if (virtualAudioSource != null)
                            {
                                if (virtualAudioSource.time != fiendMindAttack.chargeSource.time)
                                {
                                    virtualAudioSource.time = fiendMindAttack.chargeSource.time;
                                }
                            }
                            else
                            {
                                //Debug.Log("Could not synchronise FiendMindAttack.chargeSource VAS"); // These are probably not assigned until first used.
                            }
                        }
                        if (fiendMindAttack.coolSource != null)
                        {
                            VirtualAudioSource virtualAudioSource = fiendMindAttack.coolSource.gameObject.GetComponent<VirtualAudioSource>();
                            if (virtualAudioSource != null)
                            {
                                if (virtualAudioSource.time != fiendMindAttack.coolSource.time)
                                {
                                    virtualAudioSource.time = fiendMindAttack.coolSource.time;
                                }
                            }
                            else
                            {
                                //Debug.Log("Could not synchronise FiendMindAttack.coolSource VAS"); // These are probably not assigned until first used.
                            }
                        }
                        if (fiendMindAttack.hitSource != null)
                        {
                            VirtualAudioSource virtualAudioSource = fiendMindAttack.hitSource.gameObject.GetComponent<VirtualAudioSource>();
                            if (virtualAudioSource != null)
                            {
                                if (virtualAudioSource.time != fiendMindAttack.hitSource.time)
                                {
                                    virtualAudioSource.time = fiendMindAttack.hitSource.time;
                                }
                            }
                            else
                            {
                                //Debug.Log("Could not synchronise FiendMindAttack.hitSource VAS"); // These are probably not assigned until first used.
                            }
                        }
                    }
                    if (fiendMindAttack.monster == ManyMonstersMode.fiendsMonsterComponents[0] && mindAttackEffects != null)
                    {
                        for (int playerNumber = 0; playerNumber < newPlayerClasses.Count; playerNumber++)
                        {
                            MindAttackEffect mindAttackBleed = newPlayerClasses[playerNumber].GetComponentInChildren<MindAttackEffect>();
                            if (ManyMonstersMode.fiendMindAttackFiendsTargetingPlayer[playerNumber].Count == 0)
                            {
                                ManyMonstersMode.fiendMindAttackPlayerAttackTimers[playerNumber] -= Time.deltaTime;

                                mindAttackBleed.impact -= Time.deltaTime;
                                mindAttackBleed.impact = Mathf.Clamp01(mindAttackBleed.impact);
                                mindAttackBleed.strength = ManyMonstersMode.fiendMindAttackPlayerAttackTimers[playerNumber] / fiendMindAttack.maxTime;
                            }
                        }
                        /*
                        foreach (MindAttackEffect mindAttackEffect in mindAttackEffects)
                        {
                            if (!IsAFiendUsingMindAttackEffect(mindAttackEffect))
                            {
                                mindAttackEffect.impact -= Time.deltaTime;
                                mindAttackEffect.impact = Mathf.Clamp01(mindAttackEffect.impact);
                                mindAttackEffect.strength = (mindAttackEffect.strength * fiendMindAttack.maxTime - Time.deltaTime) - fiendMindAttack.maxTime;
                            }
                        }
                        */
                    }
                }
            }

            private static bool IsAFiendUsingMindAttackEffect(MindAttackEffect mindAttackEffect)
            {
                bool isAFiendUsingMindAttackEffect = false;
                foreach (GameObject fiendGameObject in ManyMonstersMode.fiends)
                {
                    FiendMindAttack fiendMindAttack = fiendGameObject.GetComponentInChildren<FiendMindAttack>();
                    if (mindAttackEffect == fiendMindAttack.mindAttackBleed)
                    {
                        isAFiendUsingMindAttackEffect = true;
                        break;
                    }
                }
                Debug.Log("Is a fiend using mind attack effect? " + isAFiendUsingMindAttackEffect);
                return isAFiendUsingMindAttackEffect;
            }

            private static void HookFiendMindAttackPlayerEffects(On.FiendMindAttack.orig_PlayerEffects orig, FiendMindAttack fiendMindAttack)
            {
                NewPlayerClass newPlayerClass = fiendMindAttack.playerHealth.NPC;
                DraggedOutHiding draggedOutHiding = newPlayerClass.GetComponent<DraggedOutHiding>();
                draggedOutHiding.LockPlayer();
                newPlayerClass.playerMecanim.SetTrigger("StunnedByAttack");
                newPlayerClass.StunnedByAttack = true;
                ((MonoBehaviour)fiendMindAttack).StartCoroutine(ReleasePlayer(newPlayerClass, 2f));
            }

            private static IEnumerator ReleasePlayer(NewPlayerClass newPlayerClass, float timeToWait)
            {
                yield return new WaitForSeconds(timeToWait);
                if (newPlayerClass != null)
                {
                    newPlayerClass.StunnedByAttack = false;
                    Debug.Log("Player is no longer stunned by attack through Fiend Mind Attack.");
                }
                else
                {
                    Debug.Log("Player is still stunned by attack through Fiend Mind Attack because newPlayerClass is null.");
                }
                yield break;
            }

            private static bool HookFiendMindAttackPlaySound(On.FiendMindAttack.orig_PlaySound orig, FiendMindAttack fiendMindAttack, string _clip, AudioSource _source)
            {
                if (fiendMindAttack.currentSound != _clip)
                {
                    if (_source != null)
                    {
                        // All of the below outcomes are called sometimes, making the code useful.
                        _source.Stop();
                        VirtualAudioSource virtualAudioSource = _source.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource != null)
                        {
                            virtualAudioSource.Stop();
                            virtualAudioSource.time = 0f;
                            //Debug.Log("Stopped FiendMindAttack generic VAS successfully");
                        }
                        else
                        {
                            //Debug.Log("VAS is null!\n" + new StackTrace().ToString()); // # This is often null for some reason.

                            if (fiendMindAttack.currentSound.Equals(fiendMindAttack.chargeSound))
                            {
                                VirtualAudioSource virtualAudioSource2 = fiendMindAttack.chargeSource.gameObject.GetComponent<VirtualAudioSource>();
                                if (virtualAudioSource2 != null)
                                {
                                    virtualAudioSource2.Stop();
                                    virtualAudioSource2.time = 0f;
                                    //Debug.Log("Stopped FiendMindAttack.chargeSource VAS successfully");
                                }
                                else
                                {
                                    //Debug.Log("Could not stop FiendMindAttack.chargeSource VAS");
                                }
                            }
                            else if (fiendMindAttack.currentSound.Equals(fiendMindAttack.coolSound))
                            {
                                VirtualAudioSource virtualAudioSource2 = fiendMindAttack.coolSource.gameObject.GetComponent<VirtualAudioSource>();
                                if (virtualAudioSource2 != null)
                                {
                                    virtualAudioSource2.Stop();
                                    virtualAudioSource2.time = 0f;
                                    //Debug.Log("Stopped FiendMindAttack.coolSource VAS successfully");
                                }
                                else
                                {
                                    //Debug.Log("Could not stop FiendMindAttack.coolSource VAS");
                                }
                            }
                            else if (fiendMindAttack.currentSound.Equals(fiendMindAttack.hitSound))
                            {
                                VirtualAudioSource virtualAudioSource2 = fiendMindAttack.hitSource.gameObject.GetComponent<VirtualAudioSource>();
                                if (virtualAudioSource2 != null)
                                {
                                    virtualAudioSource2.Stop();
                                    virtualAudioSource2.time = 0f;
                                    //Debug.Log("Stopped FiendMindAttack.hitSource VAS successfully");
                                }
                                else
                                {
                                    //Debug.Log("Could not stop FiendMindAttack.hitSource VAS");
                                }
                            }
                            else
                            {
                                //Debug.Log("FiendMindAttack.PlaySound VAS processing unsuccessful.");
                            }

                        }
                        _source.time = 0f;
                    }
                    AudioSystem.PlaySound(_clip, _source);
                    fiendMindAttack.currentSound = _clip;
                    return true;
                }
                return false;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FindInteractiveObject

            private static void HookFindInteractiveObject()
            {
                On.FindInteractiveObject.CheckInteractableConditions += new On.FindInteractiveObject.hook_CheckInteractableConditions(HookFindInteractiveObjectCheckInteractableConditions);
                //On.FindInteractiveObject.InteractablePoint += new On.FindInteractiveObject.hook_InteractablePoint(HookFindInteractiveObjectInteractablePoint); // Also crashes the game when launching. This even happens when returning Vector.zero.
                On.FindInteractiveObject.Update += new On.FindInteractiveObject.hook_Update(HookFindInteractiveObjectUpdate);
            }

            private static InteractableCondition HookFindInteractiveObjectCheckInteractableConditions(On.FindInteractiveObject.orig_CheckInteractableConditions orig, FindInteractiveObject findInteractiveObject, Interactable _interact, bool _allowAttempts)
            {
                List<InteractableCondition> conditions = _interact.Conditions;
                lastPlayerCheckingInteractableConditions = ((MonoBehaviour)findInteractiveObject).GetComponentInParent<NewPlayerClass>();
                //Debug.Log("Last player checking interactable conditions is " + PlayerNumber(lastPlayerCheckingInteractableConditions.GetInstanceID()));
                for (int i = 0; i < conditions.Count; i++)
                {
                    Type conditionType = conditions[i].GetType();
                    if (!conditions[i].IsConditionMet())
                    {
                        if (ModSettings.logDebugText)
                        {
                            Debug.Log("Condition " + conditions[i].name + " of type " + conditionType + " is not met for interactable " + _interact.name + " and player number " + PlayerNumber(lastPlayerCheckingInteractableConditions.GetInstanceID()));
                        }
                        if (conditions[i].Block != InteractableCondition.BlockType.BlockInteract || !_allowAttempts)
                        {
                            return conditions[i];
                        }
                    }
                    else
                    {
                        if (ModSettings.logDebugText)
                        {
                            Debug.Log("Condition " + conditions[i].name + " of type " + conditionType + " is met for interactable " + _interact.name + " and player number " + PlayerNumber(lastPlayerCheckingInteractableConditions.GetInstanceID()));
                        }
                    }
                }
                return null;
            }

            private static Vector3 HookFindInteractiveObjectInteractablePoint(On.FindInteractiveObject.orig_InteractablePoint orig, FindInteractiveObject findInteractiveObject, Interactable _interactable)
            {
                if (_interactable.HandGrab != null)
                {
                    return FindClosestIK(_interactable.HandGrab, ((MonoBehaviour)findInteractiveObject).GetComponentInParent<NewPlayerClass>()).position;
                }
                return _interactable.transform.position;
            }

            private static Interactable GetAimingInteractable(FindInteractiveObject findInteractiveObject, int playerNumber)
            {
                Camera playerCamera = PlayerCamera(newPlayerClasses[playerNumber]);
                //Debug.Log("Getting aiming interactable for player number " + playerNumber);
                Vector3 vector = Vector3.zero;
                if (!OculusManager.isOculusEnabled)
                {
                    vector = playerCamera.transform.position;
                }
                else
                {
                    vector = findInteractiveObject.oculusCam.transform.position;
                }
                bool flag = true;
                Vector3 origin = vector;
                Vector3 b = Vector3.zero;
                Collider collider = null;
                float num;
                if (!OculusManager.isOculusEnabled)
                {
                    num = Mathf.Lerp(findInteractiveObject.minRaycastLength, findInteractiveObject.maxRaycastLength, Vector3.Angle(((MonoBehaviour)findInteractiveObject).transform.forward, playerCamera.transform.forward) / 50f);
                }
                else
                {
                    num = Mathf.Lerp(findInteractiveObject.minRaycastLength, findInteractiveObject.maxRaycastLength, Vector3.Angle(((MonoBehaviour)findInteractiveObject).transform.forward, findInteractiveObject.oculusCam.transform.forward) / 50f);
                }
                string b2 = "ExtendedInteractable";
                findInteractiveObject.colliders.Clear();
                findInteractiveObject.layers.Clear();
                findInteractiveObject.colliders.Capacity = 10;
                findInteractiveObject.layers.Capacity = 10;
                Vector3 direction = Vector3.zero;
                if (!OculusManager.isOculusEnabled)
                {
                    direction = playerCamera.ViewportPointToRay(EyeTrack.FilteredViewPortPosition).direction;
                }
                else
                {
                    direction = findInteractiveObject.oculusCam.transform.forward;
                }
                while (flag)
                {
                    flag = false;
                    RaycastHit raycastHit;
                    if (num > 0f && Physics.Raycast(origin, direction, out raycastHit, num, findInteractiveObject.mask))
                    {
                        Interactable componentInParent = raycastHit.collider.GetComponentInParent<Interactable>();
                        if (componentInParent != null)
                        {
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("Found interactable " + componentInParent.name + " for player number " + playerNumber + " in get aiming interactable.");
                            }
                            findInteractiveObject.colliders.Add(raycastHit.collider);
                            findInteractiveObject.layers.Add(raycastHit.collider.gameObject.layer);
                            raycastHit.collider.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                            if (findInteractiveObject.CheckInteractableConditions(componentInParent, true) == null && componentInParent.active)
                            {
                                if (ModSettings.logDebugText)
                                {
                                    Debug.Log("Interactable " + componentInParent.name + " for player number " + playerNumber + " passes conditions in get aiming interactable.");
                                }
                                bool flag2 = true;
                                if (raycastHit.collider.name == b2)
                                {
                                    Vector3 vector2 = raycastHit.collider.bounds.center - vector;
                                    RaycastHit raycastHit2;
                                    if (Physics.Raycast(vector, vector2.normalized, out raycastHit2, vector2.magnitude, findInteractiveObject.mask) && raycastHit2.collider.GetComponentInParent<Interactable>() != componentInParent)
                                    {
                                        flag2 = false;
                                    }
                                }
                                if (flag2)
                                {
                                    if (ModSettings.logDebugText)
                                    {
                                        Debug.Log("Interactable " + componentInParent.name + " for player number " + playerNumber + " passes second conditions in get aiming interactable.");
                                    }
                                    if (collider == null || (Vector3.Distance(collider.bounds.center, b) > Vector3.Distance(raycastHit.collider.bounds.center, b) && collider.name == b2) || raycastHit.collider.name != b2)
                                    {
                                        if (collider == null)
                                        {
                                            b = raycastHit.point;
                                        }
                                        collider = raycastHit.collider;
                                    }
                                    if (collider != null && raycastHit.collider.name != b2)
                                    {
                                        break;
                                    }
                                }
                            }
                            flag = true;
                        }
                    }
                }
                for (int i = 0; i < findInteractiveObject.colliders.Count; i++)
                {
                    findInteractiveObject.colliders[i].gameObject.layer = findInteractiveObject.layers[i];
                }
                if (collider != null)
                {
                    return collider.GetComponentInParent<Interactable>();
                }
                return null;
            }

            private static bool IsKeyDown(Interactable _interactable, int playerNumber)
            {
                switch (_interactable.keyType)
                {
                    case Interactable.InputKey.Use:
                        return GetPlayerKey("UseItem", playerNumber).JustPressed() || GetPlayerTriggerStateIfUsingController("Right", playerNumber, true);
                    case Interactable.InputKey.PickupUse:
                        return ((!GetPlayerKey("Interact", playerNumber).IsDown() && (GetPlayerKey("UseItem", playerNumber).JustPressed() || GetPlayerTriggerStateIfUsingController("Right", playerNumber, true))) || (!(GetPlayerKey("UseItem", playerNumber).IsDown() || GetPlayerTriggerStateIfUsingController("Right", playerNumber)) && GetPlayerKey("Interact", playerNumber).JustPressed()));
                    case Interactable.InputKey.Jump:
                        return GetPlayerKey("Jump", playerNumber).JustPressed();
                    default:
                        return GetPlayerKey("Interact", playerNumber).JustPressed();
                }
            }

            private static bool IsKeyUp(Interactable _interactable, int playerNumber)
            {
                switch (_interactable.keyType)
                {
                    case Interactable.InputKey.Use:
                        return GetPlayerKey("UseItem", playerNumber).JustReleased() || GetPlayerTriggerStateIfUsingController("Right", playerNumber, false, true);
                    case Interactable.InputKey.PickupUse:
                        return !GetPlayerKey("Interact", playerNumber).IsDown() && !(GetPlayerKey("UseItem", playerNumber).IsDown() || GetPlayerTriggerStateIfUsingController("Right", playerNumber)) && (GetPlayerKey("Interact", playerNumber).JustReleased() || (GetPlayerKey("UseItem", playerNumber).JustReleased() || GetPlayerTriggerStateIfUsingController("Right", playerNumber, false, true)));
                    case Interactable.InputKey.Jump:
                        return GetPlayerKey("Jump", playerNumber).JustReleased();
                    default:
                        return GetPlayerKey("Interact", playerNumber).JustReleased();
                }
            }

            private static void HookFindInteractiveObjectUpdate(On.FindInteractiveObject.orig_Update orig, FindInteractiveObject findInteractiveObject)
            {
                if (LevelGeneration.Instance.finishedGenerating && !findInteractiveObject.pause && !findInteractiveObject.journal.IsActive)
                {
                    NewPlayerClass newPlayerClass = ((MonoBehaviour)findInteractiveObject).GetComponentInParent<NewPlayerClass>();
                    //lastPlayerCheckingInteractableConditions = newPlayerClass;
                    int newPlayerClassID = newPlayerClass.GetInstanceID();
                    int playerNumber = PlayerNumber(newPlayerClassID);
                    //Debug.Log("FindInteractiveObjectUpdate player number is " + playerNumber); // Seems to run for both players.
                    findInteractiveObject.interactThisFrame = false;
                    Interactable aimingInteractable = GetAimingInteractable(findInteractiveObject, playerNumber);
                    if (findInteractiveObject.activatedInteractable != null && IsKeyUp(findInteractiveObject.activatedInteractable, playerNumber))
                    {
                        int interactableID = findInteractiveObject.activatedInteractable.GetInstanceID();
                        if (IsInteractableAssociatedWithPlayer(interactableID, newPlayerClassID))
                        {
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("Activated Interactable " + findInteractiveObject.activatedInteractable.name + " is associated with player number " + playerNumber);
                            }
                            lastPlayerSentMessage = newPlayerClass;
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("Updating lastPlayerSentMessage: " + PlayerNumber(lastPlayerSentMessage.GetInstanceID()) + "\n" + new StackTrace().ToString() + "\n-----");
                            }
                            interactablesAssociatedWithPlayer.Remove(interactableID);
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("Last player sent message at OnReleaseKey is " + playerNumber);
                            }
                            findInteractiveObject.activatedInteractable.OnReleaseKey();
                            findInteractiveObject.activatedInteractable = null;
                        }
                        else
                        {
                            Debug.Log("Activated Interactable " + findInteractiveObject.activatedInteractable.name + " is not associated with player number " + playerNumber);
                        }
                    }
                    if (findInteractiveObject.activatedInteractable == null)
                    {
                        Interactable interactable = null;
                        if (aimingInteractable != null)
                        {
                            interactable = aimingInteractable.GetComponent<Interactable>();
                        }
                        if (interactable != null)
                        {
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("Found interactable " + interactable.name + " for player number " + playerNumber);
                            }
                        }
                        if (interactable == null && findInteractiveObject.reticule != null)
                        {
                            findInteractiveObject.reticule.NonInteractReticule();
                        }
                        if (interactable != findInteractiveObject.interactable)
                        {
                            if (findInteractiveObject.interactable != null)
                            {
                                findInteractiveObject.interactable.OnHoverEnd();
                            }
                            if (interactable != null && !interactablesAssociatedWithPlayer.ContainsKey(interactable.GetInstanceID()))
                            {
                                if (ModSettings.logDebugText)
                                {
                                    Debug.Log("Interactable " + interactable.name + " is not associated with player number " + playerNumber);
                                }
                                interactable.OnHoverBegin();
                                if (findInteractiveObject.reticule != null && (findInteractiveObject.interactable == null || (!(interactable.transform.parent == findInteractiveObject.interactable.transform.parent) && !(interactable.transform == findInteractiveObject.interactable.transform.parent) && !(interactable.transform.parent == findInteractiveObject.interactable.transform))))
                                {
                                    findInteractiveObject.reticule.ChangeInteractReticule();
                                }
                            }
                            else if (interactable != null)
                            {
                                if (ModSettings.logDebugText)
                                {
                                    Debug.Log("Interactable " + interactable.name + " is associated with player number " + playerNumber);
                                }
                            }
                            findInteractiveObject.interactable = interactable;
                        }
                        if (findInteractiveObject.interactable != null && IsKeyDown(findInteractiveObject.interactable, playerNumber))
                        {
                            InteractableCondition interactableCondition = findInteractiveObject.CheckInteractableConditions(findInteractiveObject.interactable, false);
                            if (interactableCondition == null)
                            {
                                int interactableID = findInteractiveObject.interactable.GetInstanceID();
                                if (!interactablesAssociatedWithPlayer.ContainsKey(interactableID))
                                {
                                    if (ModSettings.logDebugText)
                                    {
                                        Debug.Log("FindInteractiveObject.Interactable " + findInteractiveObject.interactable.name + " is not associated with player number " + playerNumber);
                                    }
                                    findInteractiveObject.interactThisFrame = true;
                                    lastPlayerSentMessage = newPlayerClass;
                                    if (ModSettings.logDebugText)
                                    {
                                        Debug.Log("Updating lastPlayerSentMessage: " + PlayerNumber(lastPlayerSentMessage.GetInstanceID()) + "\n" + new StackTrace().ToString() + "\n-----");
                                    }
                                    interactablesAssociatedWithPlayer.Add(interactableID, newPlayerClassID);
                                    if (ModSettings.logDebugText)
                                    {
                                        Debug.Log("Last player sent message at OnActivate is " + playerNumber);
                                    }
                                    findInteractiveObject.interactable.OnActivate();
                                    findInteractiveObject.activatedInteractable = findInteractiveObject.interactable;
                                }
                                else
                                {
                                    if (ModSettings.logDebugText)
                                    {
                                        Debug.Log("FindInteractiveObject.Interactable " + findInteractiveObject.interactable.name + " is associated with player number " + playerNumber);
                                    }
                                }
                            }
                            else
                            {
                                findInteractiveObject.reticule.InteractFailReticule();
                                findInteractiveObject.interactFail = true;
                                interactableCondition.OnInteractAttempt();
                                AudioSystem.PlaySound("Noises/UI/Reticule/UI_Reticule3");
                            }
                        }
                        else if (GetPlayerKey("Interact", playerNumber).JustPressed())
                        {
                            AudioSystem.PlaySound("Noises/UI/Reticule/UI_Reticule1");
                        }
                        if (findInteractiveObject.interactFail)
                        {
                            findInteractiveObject.timer += Time.deltaTime;
                            if (findInteractiveObject.timer >= 0.5f)
                            {
                                findInteractiveObject.timer = 0f;
                                findInteractiveObject.interactFail = false;
                                findInteractiveObject.reticule.ChangeInteractReticule();
                            }
                        }
                    }
                }
            }

            // https://stackoverflow.com/questions/9650355/how-to-check-if-a-key-value-pair-exists-in-a-dictionary

            private static bool IsInteractableAssociatedWithPlayer(int passedInteractableInstanceID, int passedPlayerInstanceID)
            {
                int playerInstanceIDFromDictionary;
                if (interactablesAssociatedWithPlayer.TryGetValue(passedInteractableInstanceID, out playerInstanceIDFromDictionary) && passedPlayerInstanceID == playerInstanceIDFromDictionary)
                {
                    return true;
                }
                return false;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FireExtinguisher

            private static void HookFireExtinguisherOnDropItem(On.FireExtinguisher.orig_OnDropItem orig, FireExtinguisher fireExtinguisher)
            {
                orig.Invoke(fireExtinguisher);
                VirtualAudioSource virtualAudioSource = fireExtinguisher.audSource.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    virtualAudioSource.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                }
            }

            private static void HookFireExtinguisherSwitchOffFireExtinguisher(On.FireExtinguisher.orig_SwitchOffFireExtinguisher orig, FireExtinguisher fireExtinguisher)
            {
                orig.Invoke(fireExtinguisher);
                VirtualAudioSource virtualAudioSource = fireExtinguisher.audSource.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    virtualAudioSource.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FixedInteractable

            private static void HookFixedInteractable()
            {
                On.FixedInteractable.OnInteract += new On.FixedInteractable.hook_OnInteract(HookFixedInteractableOnInteract);
                //On.FixedInteractable.PlayAnimation += new On.FixedInteractable.hook_PlayAnimation(HookFixedInteractablePlayAnimation);
            }

            private static void HookFixedInteractableOnInteract(On.FixedInteractable.orig_OnInteract orig, FixedInteractable fixedInteractable)
            {
                fixedInteractable.player = lastPlayerSentMessage;
                Debug.Log("Fixed interactable last player sent message is " + PlayerNumber(fixedInteractable.player.GetInstanceID()));
                orig.Invoke(fixedInteractable);
                /*
                if (!FixedInteractable.journal.IsActive && !lastPlayerSentMessage.MovingToStand)
                {
                    fixedInteractable.PlayAnimation();
                }
                */
            }

            /*
            private static void HookFixedInteractablePlayAnimation(On.FixedInteractable.orig_PlayAnimation orig, FixedInteractable fixedInteractable)
            {
                Transform transform;
                if (fixedInteractable.animationTransform2 != null && fixedInteractable.animationTransform != null)
                {
                    if (Vector3.Distance(fixedInteractable.animationTransform2.position, lastPlayerSentMessage.transform.position) < Vector3.Distance(fixedInteractable.animationTransform.position, lastPlayerSentMessage.transform.position))
                    {
                        transform = fixedInteractable.animationTransform2;
                    }
                    else
                    {
                        transform = fixedInteractable.animationTransform;
                    }
                }
                else
                {
                    transform = fixedInteractable.animationTransform;
                }
                if (lastPlayerSentMessage != null && transform != null)
                {
                    if (fixedInteractable.animationName != null)
                    {
                        lastPlayerSentMessage.StartFixedAnimation(transform, fixedInteractable.animation, fixedInteractable.specificFix, fixedInteractable.animationType, fixedInteractable.itemNeeded, fixedInteractable, fixedInteractable.animationName);
                    }
                    else
                    {
                        lastPlayerSentMessage.StartFixedAnimation(transform, fixedInteractable.animation, fixedInteractable.specificFix, fixedInteractable.animationType, fixedInteractable.itemNeeded, fixedInteractable, "Default");
                    }
                }
            }
            */

            /*----------------------------------------------------------------------------------------------------*/
            // @Flammable

            private static void HookFlammableUpdate(On.Flammable.orig_Update orig, Flammable flammable)
            {
                if (flammable.useRay && Physics.Raycast(((MonoBehaviour)flammable).gameObject.transform.position, Vector3.down, out flammable.hit, 2f, flammable.mask) && flammable.hit.collider.GetComponent<Flammable>() != null)
                {
                    flammable.touchingFires.Add(flammable.hit.collider.GetComponent<Flammable>());
                }
                if (flammable.OnFire)
                {
                    if (flammable.avoidFire != null)
                    {
                        flammable.avoidFire.SetActive(true);
                    }
                    if (flammable.bloom != null)
                    {
                        foreach (NewPlayerClass newPlayerClass in newPlayerClasses)
                        {
                            MonoBehaviour bloom = (PlayerCamera(newPlayerClass).GetComponent("BloomAndLensFlares") as MonoBehaviour);
                            bloom.enabled = false;
                        }
                    }
                    flammable.delayFireTimer = 0f;
                    if (flammable.fire != null)
                    {
                        flammable.fire.gameObject.SetActive(true);
                        flammable.psem.enabled = true;
                    }
                    flammable.fireIntensity += flammable.flammability * Time.deltaTime;
                    foreach (Flammable flammableTouchingFires in flammable.touchingFires)
                    {
                        if (flammableTouchingFires != null)
                        {
                            flammableTouchingFires.Spread(flammableTouchingFires.spreadability);
                        }
                    }
                    flammable.fireIntensity = Mathf.Clamp(0f, flammable.maxFireIntensity, flammable.fireIntensity);
                    flammable.fireFuel -= flammable.fireIntensity * Time.deltaTime;
                    if (flammable.fireFuel <= 0f)
                    {
                        flammable.OnFireEnd();
                    }
                    if (flammable.fire != null)
                    {
                        flammable.fire.SetIntensity(Mathf.Clamp(Mathf.Clamp(flammable.fireIntensity, 0.3f, 1f), 0f, flammable.fireFuel / flammable.lowFuel));
                    }
                    if (flammable.fireVolCon == null)
                    {
                        if (flammable.fireSource != null)
                        {
                            flammable.fireVolCon = flammable.fireSource.GetComponent<VolumeController>();
                        }
                    }
                    else
                    {
                        flammable.fireVolCon.fadeValue = flammable.fireIntensity;
                    }
                }
                else
                {
                    if (flammable.avoidFire != null)
                    {
                        flammable.avoidFire.SetActive(false);
                    }
                    flammable.delayFireTimer += Time.deltaTime;
                    if (flammable.delayFireTimer >= 15f && flammable.bloom != null)
                    {
                        foreach (NewPlayerClass newPlayerClass in newPlayerClasses)
                        {
                            MonoBehaviour bloom = (PlayerCamera(newPlayerClass).GetComponent("BloomAndLensFlares") as MonoBehaviour);
                            bloom.enabled = true;
                        }
                    }
                    if (flammable.fire != null)
                    {
                        flammable.psem.enabled = false;
                    }
                }
            }

            private static void HookFlammableOnFireEnd(On.Flammable.orig_OnFireEnd orig, Flammable flammable)
            {
                orig.Invoke(flammable);
                if (flammable.fireSource != null)
                {
                    VirtualAudioSource virtualAudioSource = flammable.fireSource.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FlareGun

            private static void HookFlareGunOnUseItem(On.FlareGun.orig_OnUseItem orig, FlareGun flareGun)
            {
                if (flareGun.readyTofire)
                {
                    InventoryFromItemClass(flareGun.gameObject).newPlayerClass.PlayItemAnimation();
                    flareGun.FireFlare2();
                    flareGun.readyTofire = false;
                    flareGun.ammoCount--;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Flashlight

            private static void HookFlashlightToggleFlashlight(On.Flashlight.orig_ToggleFlashlight orig, Flashlight flashlight)
            {
                flashlight.on = !flashlight.on;
                if (flashlight.on)
                {
                    if (flashlight.torchSource != null)
                    {
                        AudioSystem.PlaySound("Noises/Actions/Flashlight/Activate/On", flashlight.torchSource);
                    }
                    flashlight.SwitchOnFlashlight();
                }
                else
                {
                    if (flashlight.torchSource != null)
                    {
                        flashlight.torchSource.Stop();
                        VirtualAudioSource virtualAudioSource = flashlight.torchSource.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource != null)
                        {
                            virtualAudioSource.Stop();
                        }
                        else if (ModSettings.logDebugText)
                        {
                            Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                        }
                        AudioSystem.PlaySound("Noises/Actions/Flashlight/Activate/Off", flashlight.torchSource.transform, flashlight.torchSource);
                    }
                    flashlight.SwitchOffFlashlight();
                }
                if (flashlight.power <= 0f && flashlight.on)
                {
                    flashlight.power += flashlight.emergencyPower;
                    flashlight.emergencyPower -= 1f;
                    flashlight.flashLightIntensity = 0.03f;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FootStepManager

            private static void HookFootStepManagerStart(On.FootStepManager.orig_Start orig, FootStepManager footStepManager)
            {
                orig.Invoke(footStepManager);
                //FindGameObjectLink(footStepManager.gameObject, "FootStepManager");
                bool foundLink = false;
                if (footStepManager.gameObject.GetComponentInParent(typeof(NewPlayerClass)) != null)
                {
                    Debug.Log("Link found in parent of " + footStepManager.gameObject.name + " for type NewPlayerClass");
                    foundLink = true;
                }
                if (footStepManager.gameObject.GetComponent(typeof(NewPlayerClass)) != null)
                {
                    Debug.Log("Link found on same level of " + footStepManager.gameObject.name + " for type NewPlayerClass");
                    foundLink = true;
                }
                if (footStepManager.gameObject.GetComponentInChildren(typeof(NewPlayerClass)) != null)
                {
                    Debug.Log("Link found in children of " + footStepManager.gameObject.name + " for type NewPlayerClass");
                    foundLink = true;
                }
                if (!foundLink)
                {
                    Debug.Log("No link found for " + footStepManager.gameObject.name + " for type NewPlayerClass");
                }
            }

            private static void HookFootStepManagerSetUpStep(On.FootStepManager.orig_SetUpStep orig, FootStepManager footStepManager, int _speed)
            {
                if (((MonoBehaviour)footStepManager).transform.root.tag == "Player")
                {
                    footStepManager.room = ((MonoBehaviour)footStepManager).GetComponentInParent<NewPlayerClass>().GetComponentInChildren<DetectRoom>().GetRoom; // FootStepManager.roomdetect.GetRoom; // Original code that uses References.Player
                    if (footStepManager.room != null)
                    {
                        footStepManager.source.maxDistance = footStepManager.RunMaxDistance;
                        footStepManager.source2.maxDistance = footStepManager.RunMaxDistance;
                        footStepManager.DoFootstep(footStepManager.room.FloorMaterial);
                    }
                }
                else if (((MonoBehaviour)footStepManager).transform.root.tag == "Monster")
                {
                    if (footStepManager.monster.monsterType.Equals("Sparky") && footStepManager.monster.GetComponent<MState>().Fsm.Current.GetType() == typeof(MLurkState))
                    {
                        return;
                    }
                    float num = 1f;
                    if (footStepManager.monster.HeliStarted())
                    {
                        num = 3f;
                    }
                    else if (footStepManager.monster.SubEventBeenStarted())
                    {
                        num = 5f;
                    }
                    footStepManager.room = footStepManager.monster.RoomDetect.CurrentRoom;
                    if (footStepManager.room != null && (float)_speed == footStepManager.monster.MoveControl.MaxSpeed)
                    {
                        if (footStepManager.state == FootStepManager.MoveState.Run)
                        {
                            footStepManager.source.maxDistance = footStepManager.RunMaxDistance * num;
                            footStepManager.source2.maxDistance = footStepManager.RunMaxDistance * num;
                        }
                        else
                        {
                            footStepManager.source.maxDistance = footStepManager.WalkMaxDistance * num;
                            footStepManager.source2.maxDistance = footStepManager.WalkMaxDistance * num;
                        }
                        footStepManager.DoFootstep(footStepManager.room.FloorMaterial);
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FuelCan

            private static void HookFuelCanOnUseItem(On.FuelCan.orig_OnUseItem orig, FuelCan fuelCan)
            {
                fuelCan.inUse = true;
                fuelCan.particlesParent.transform.parent = null;
                fuelCan.particlesParent.transform.parent = InventoryFromItemClass(fuelCan.gameObject).newPlayerClass.transform;
                fuelCan.particlesParent.transform.localPosition = new Vector3(0.759f, 0.36f, 0.054f);
                fuelCan.particlesParent.transform.localRotation = fuelCan.originalRotation;
                if (!fuelCan.usingOnPump)
                {
                    if (fuelCan.fuel.fuel > 0f && !fuelCan.changingUsage)
                    {
                        ((MonoBehaviour)fuelCan).StartCoroutine(fuelCan.ChangeUsage());
                    }
                    ((MonoBehaviour)fuelCan).Invoke("StartPouring", fuelCan.prepareCanTime);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FuelPump

            private static void HookFuelPumpAddFuel(On.FuelPump.orig_AddFuel orig, FuelPump fuelPump, int _delta)
            {
                fuelPump.SetFuel(fuelPump.fuel + _delta);
                fuelPump.fuelScaleBar.ScaleUpFuelBar(fuelPump.fuel);
                ((MonoBehaviour)fuelPump).StartCoroutine(DestroyCan(PlayerNumber(lastPlayerSentMessage.GetInstanceID())));
                if (fuelPump.fuel == fuelPump.maxFuel)
                {
                    fuelPump.opening = true;
                    ((MonoBehaviour)fuelPump).Invoke("OpenCloseCap", 2f);
                    fuelPump.helicopterEscape.ReadyToFuel = true;
                    Objectives.Tasks("HelicopterEscape").Tasks("Fill Pump").Complete();
                    ((MonoBehaviour)fuelPump).gameObject.GetComponent<Interactable>().active = false;
                }
            }

            private static IEnumerator DestroyCan(int playerNumber)
            {
                FuelCan fuelCan = newPlayerClasses[playerNumber].GetComponentInChildren<FuelCan>();
                if (fuelCan != null)
                {
                    yield return new WaitForSeconds(3f);
                    if (fuelCan != null)
                    {
                        inventories[playerNumber].DestroyItem(fuelCan.GetComponent<InventoryItem>());
                    }
                }
                yield break;
            }

            /*
            private static IEnumerator HookFuelPumpDestroyCan(On.FuelPump.orig_DestroyCan orig, FuelPump fuelPump)
            {
                int playerNumber = ClosestPlayerToThis(fuelPump.transform.position);
                FuelCan can = newPlayerClasses[playerNumber].gameObject.GetComponentInChildren<FuelCan>();
                if (can != null)
                {
                    yield return new WaitForSeconds(3f);
                    if (can != null)
                    {
                        inventories[playerNumber].DestroyItem(can.GetComponent<InventoryItem>());
                    }
                }
                yield break;
            }
            */

            private static void HookFuelPumpOnInteract(On.FuelPump.orig_OnInteract orig, FuelPump fuelPump)
            {
                Inventory inventory = PlayerInventory(lastPlayerSentMessage);
                if (inventory.CurrentItem != null)
                {
                    FuelCan fuelCan = inventory.CurrentItem.GetComponent<FuelCan>();
                    if (fuelCan != null)
                    {
                        fuelPump.pouring = true;
                        fuelCan.UsingOnPump = true;
                        ((MonoBehaviour)fuelPump).Invoke("PostPour", 3f);
                        fuelCan.Invoke("FinishTrolleyPour", 3f);
                        fuelPump.AddFuel(1);
                    }
                }
            }

            private static void HookFuelPumpPlayHeliAudio(On.FuelPump.orig_PlayHeliAudio orig, FuelPump fuelPump)
            {
                if (!fuelPump.fuelSource.isPlaying && !fuelPump.helicopterEscape.IsDestroyed)
                {
                    FuelPump.FuelSourceState fss = fuelPump.FSS;
                    if (fss != FuelPump.FuelSourceState.Start)
                    {
                        if (fss != FuelPump.FuelSourceState.Loop)
                        {
                            if (fss == FuelPump.FuelSourceState.End)
                            {
                                fuelPump.playAudio = false;
                                AudioSystem.PlaySound("Noises/Helicopter/Fuel Pump/End", fuelPump.fuelSource);
                                fuelPump.fuelAnimation.Stop();
                                VirtualAudioSource virtualAudioSource = fuelPump.fuelAnimation.gameObject.GetComponent<VirtualAudioSource>();
                                if (virtualAudioSource != null)
                                {
                                    virtualAudioSource.Stop();
                                }
                                else if (ModSettings.logDebugText)
                                {
                                    Debug.Log("VAS is null 1!\n" + new StackTrace().ToString());
                                }
                                fuelPump.fuelScaleBar.fuelEventStarted = false;
                            }
                        }
                        else
                        {
                            AudioSystem.PlaySound("Noises/Helicopter/Fuel Pump/Loop", fuelPump.fuelSource);
                            if (fuelPump.fuelSource.clip.name == "ACT_FuelPumpLoop")
                            {
                                fuelPump.fuelSource.loop = true;
                                fuelPump.loopStarted = true;
                            }
                        }
                    }
                    else
                    {
                        fuelPump.fuelAnimation.Play("FuelPumpShudder2");
                        AudioSystem.PlaySound("Noises/Helicopter/Fuel Pump/Start", fuelPump.fuelSource);
                        fuelPump.FSS = FuelPump.FuelSourceState.Loop;
                        fuelPump.fuelScaleBar.fuelEventStarted = true;
                    }
                }
                else if (fuelPump.helicopterEscape.IsDestroyed)
                {
                    fuelPump.fuelScaleBar.fuelEventStarted = false;
                    fuelPump.playAudio = false;
                    fuelPump.fuelSource.Stop();
                    VirtualAudioSource virtualAudioSource2 = fuelPump.fuelSource.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource2 != null)
                    {
                        virtualAudioSource2.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null 2!\n" + new StackTrace().ToString());
                    }
                    fuelPump.fuelAnimation.Stop();
                    VirtualAudioSource virtualAudioSource3 = fuelPump.fuelAnimation.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource3 != null)
                    {
                        virtualAudioSource3.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null 3!\n" + new StackTrace().ToString());
                    }
                }
                if (fuelPump.DestroyPositionSource != null && fuelPump.helicopterEscape != null && !fuelPump.helicopterEscape.IsDestroyed)
                {
                    AudioSystem.PlaySound("Noises/Submarine/Loud Silence", fuelPump.DestroyPositionSource);
                    DistractionSound component = fuelPump.DestroyPositionSource.GetComponent<DistractionSound>();
                    if (component != null)
                    {
                        component.IsImportant = true;
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Fuse

            private static IEnumerator LerpFuse(Fuse fuse, NewPlayerClass newPlayerClass)
            {
                ItemLerp itemLerp = ((MonoBehaviour)fuse).GetComponent<ItemLerp>();
                itemLerp.target = fuse.box.fusePosition;
                ((MonoBehaviour)fuse).transform.parent = fuse.box.transform;
                yield return ((MonoBehaviour)fuse).StartCoroutine(itemLerp.Lerp(((MonoBehaviour)fuse).GetComponentInChildren<Renderer>().transform.parent));
                yield return new WaitForSeconds(1f);
                Collider[] colliders = ((MonoBehaviour)fuse).GetComponentsInChildren<Collider>();
                InventoryItem item = ((MonoBehaviour)fuse).GetComponent<InventoryItem>();
                if (item != null)
                {
                    item.enabled = false;
                }
                //Debug.Log("Lerping fuse for player number " + PlayerNumber(newPlayerClass.GetInstanceID()));
                while (newPlayerClass.InFixedAnimation())
                {
                    yield return null;
                }
                Inventory inventory = PlayerInventory(newPlayerClass);
                inventory.DropItem(inventory.CurrentSlot);
                ((MonoBehaviour)fuse).GetComponent<Interactable>().active = false;
                ((MonoBehaviour)fuse).GetComponent<Rigidbody>().isKinematic = true;
                foreach (Collider collider in colliders)
                {
                    collider.enabled = false;
                }
                yield break;
            }

            private static void OnFuseReachedBox(Fuse fuse, NewPlayerClass newPlayerClass)
            {
                ((MonoBehaviour)fuse).GetComponent<Interactable>().active = false;
                ((MonoBehaviour)fuse).StartCoroutine(LerpFuse(fuse, newPlayerClass));
            }

            private static void HookFuseOnFuseStart(On.Fuse.orig_OnFuseStart orig, Fuse fuse)
            {
                fuse.interactable = lastPlayerSentMessage.fixedInteractable; //lastPlayerCheckingInteractableConditions.fixedInteractable;
                fuse.box = fuse.interactable.GetComponent<FuseBox>();
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FuseBox

            private static void HookFuseBoxOnStartFixedAnimation(On.FuseBox.orig_OnStartFixedAnimation orig, FuseBox fuseBox)
            {
                Fuse fuse = lastPlayerSentMessage.GetComponentInChildren<Fuse>(); //lastPlayerCheckingInteractableConditions.GetComponentInChildren<Fuse>();
                fuse.OnFuseStart();
                //((MonoBehaviour)fuseBox).StartCoroutine(OnFuseReachedBox(fuseBox, lastPlayerCheckingInteractableConditions));
                ((MonoBehaviour)fuseBox).StartCoroutine(OnFuseReachedBox(fuseBox, lastPlayerSentMessage));
            }

            private static IEnumerator OnFuseReachedBox(FuseBox fuseBox, NewPlayerClass newPlayerClass)
            {
                yield return new WaitForSeconds(1.1f); // Wait 1.1 seconds to simulate the original invocation time.

                fuseBox.AddFuse();
                if (fuseBox.poweredGO == null)
                {
                    FuseBox.lightFuseBoxesFixed++;
                    if (FuseBox.lightFuseBoxesFixed == 2)
                    {
                        Achievements.Instance.CompleteAchievement("POWER_FUSEBOXES");
                    }
                }
                Fuse fuse = newPlayerClass.GetComponentInChildren<Fuse>();
                if (fuse)
                {
                    OnFuseReachedBox(fuse, newPlayerClass);
                }
                yield break;
            }

            private static void HookFuseBoxPowerDown(On.FuseBox.orig_PowerDown orig, FuseBox fuseBox)
            {
                orig.Invoke(fuseBox);
                if (fuseBox.actualSource != null)
                {
                    VirtualAudioSource virtualAudioSource = fuseBox.actualSource.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FuseBoxDoor

            public static bool HookFuseBoxDoorOpenCloseIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!ManyMonstersMode.IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookFuseBoxDoorOpenClose((FuseBoxDoor)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    ManyMonstersMode.IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookFuseBoxDoorOpenClose(FuseBoxDoor fuseBoxDoor)
            {
                fuseBoxDoor.transitioning = true;
                bool finished = false;
                while (!finished)
                {
                    foreach (NewPlayerClass newPlayerClass in crewPlayers)
                    {
                        float angle = Vector3.Angle(newPlayerClass.transform.forward, ((MonoBehaviour)fuseBoxDoor).gameObject.transform.parent.transform.forward);
                        float distance = Vector3.Distance(newPlayerClass.transform.position, ((MonoBehaviour)fuseBoxDoor).gameObject.transform.position);
                        if (angle >= 100f && distance <= 1.6f)
                        {
                            newPlayerClass.Motor.controller.Move(-newPlayerClass.transform.forward * (1.3f * Time.deltaTime / 1.5f));
                        }
                    }
                    fuseBoxDoor.t += fuseBoxDoor.speed * Time.deltaTime * ((!fuseBoxDoor.opening) ? -1f : 1f);
                    ((MonoBehaviour)fuseBoxDoor).transform.localRotation = Quaternion.Slerp(fuseBoxDoor.startRotation, Quaternion.AngleAxis(fuseBoxDoor.maxAngle, Vector3.up), fuseBoxDoor.t);
                    if (fuseBoxDoor.opening && fuseBoxDoor.t >= 1f)
                    {
                        finished = true;
                    }
                    else if (!fuseBoxDoor.opening && fuseBoxDoor.t <= 0f)
                    {
                        finished = true;
                    }
                    yield return null;
                }
                fuseBoxDoor.transitioning = false;
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @GenericLight

            private static bool HookGenericLightPlayerDistanceCheck(On.GenericLight.orig_PlayerDistanceCheck orig, GenericLight genericLight)
            {
                Vector3 lightPosition = ((MonoBehaviour)genericLight).transform.position;
                return Vector3.Distance(lightPosition, PlayerCamera(newPlayerClasses[ClosestPlayerToThis(lightPosition)]).transform.position) < 2.5f;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @GlobalMusic

            private static void HookGlobalMusicCrossFade(On.GlobalMusic.orig_CrossFade orig, GlobalMusic globalMusic)
            {
                if (globalMusic.previous != null && globalMusic.previousLib != null && globalMusic.previous.isPlaying)
                {
                    globalMusic.previousLib.fadeValue = Mathf.Lerp(globalMusic.previousLib.fadeValue, 0f, globalMusic.lerpTDown);
                    globalMusic.volumeCon.CalculateVolume(globalMusic.previous, globalMusic.previousLib.fadeValue);
                    globalMusic.prevFader = globalMusic.previousLib.fadeValue;
                    if (globalMusic.prevFader < 0.01f)
                    {
                        globalMusic.previous.volume = 0f;
                        globalMusic.prevFader = 0f;
                        globalMusic.previous.Stop();
                        VirtualAudioSource virtualAudioSource = globalMusic.previous.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource != null)
                        {
                            virtualAudioSource.Stop();
                        }
                        else if (ModSettings.logDebugText)
                        {
                            Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                        }
                        globalMusic.previousLib.fadeValue = 0f;
                        globalMusic.prevFader = globalMusic.previousLib.fadeValue;
                    }
                }
                if (globalMusic.volumeCon != null)
                {
                    globalMusic.volumeCon.StartVolume = globalMusic.lib.startVolume;
                    globalMusic.volumeCon.CalculateVolume(globalMusic.current, globalMusic.lerpTUp);
                }
            }

            private static void HookGlobalMusicSwitchSources(On.GlobalMusic.orig_SwitchSources orig, GlobalMusic globalMusic)
            {
                if (globalMusic.previousSong != globalMusic.song)
                {
                    globalMusic.lerpTDown = 0f;
                    globalMusic.lerpTUp = 0f;
                    if (globalMusic.previousLib != null && globalMusic.lib != null)
                    {
                        globalMusic.previousLib.fadeValue = globalMusic.lib.fadeValue;
                    }
                    globalMusic.previousLib = globalMusic.lib;
                    globalMusic.previousSong = globalMusic.song;
                    if (globalMusic.previous.isPlaying)
                    {
                        globalMusic.previous.Stop();
                        VirtualAudioSource virtualAudioSource = globalMusic.previous.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource != null)
                        {
                            virtualAudioSource.Stop();
                        }
                        else if (ModSettings.logDebugText)
                        {
                            Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                        }
                    }
                    if (globalMusic.current == globalMusic.sources[0])
                    {
                        globalMusic.previous = globalMusic.sources[0];
                        globalMusic.current = globalMusic.sources[1];
                    }
                    else
                    {
                        globalMusic.previous = globalMusic.sources[1];
                        globalMusic.current = globalMusic.sources[0];
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @GoopBurst

            private static void HookGoopBurstBurstPod(On.GoopBurst.orig_BurstPod orig, GoopBurst goopBurst)
            {
                Transform goopBurstTransform = ((MonoBehaviour)goopBurst).gameObject.transform;
                Vector3 goopBurstPosition = goopBurstTransform.position;
                Vector3 closestCrewCameraPosition = PlayerCamera(newPlayerClasses[ClosestPlayerToThis(goopBurstPosition, true)]).transform.position;
                if (GeoHelper.InsideCone(goopBurstPosition, closestCrewCameraPosition, goopBurstTransform.forward, 30f, 2f))
                {
                    goopBurst.DoBurstAnimation(goopBurst.forwardGoop);
                }
                else
                {
                    goopBurst.DoBurstAnimation(goopBurst.sideGoop);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @HandAnimationController

            private static void HookHandAnimationController()
            {
                On.HandAnimationController.LateUpdate += new On.HandAnimationController.hook_LateUpdate(HookHandAnimationControllerLateUpdate);
                On.HandAnimationController.Start += new On.HandAnimationController.hook_Start(HookHandAnimationControllerStart);
            }

            private static void HookHandAnimationControllerLateUpdate(On.HandAnimationController.orig_LateUpdate orig, HandAnimationController handAnimationController)
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    Inventory playerInventory = PlayerInventory(handAnimationController.player);
                    InventoryItem currentItem = playerInventory.CurrentItem;
                    try
                    {
                        HandAnimationController.HandMode key;
                        if (currentItem != null && !handAnimationController.player.fixedAnimationPlaying)
                        {
                            if (!currentItem.twoHanded)
                            {
                                key = HandAnimationController.HandMode.Single;
                            }
                            else
                            {
                                key = HandAnimationController.HandMode.Double;
                            }
                        }
                        else
                        {
                            key = HandAnimationController.HandMode.Default;
                        }
                        for (int i = 0; i < handAnimationController.maxLayers; i++)
                        {
                            bool flag = handAnimationController.modeLayers[key].Contains(i);
                            float maxDelta = Time.deltaTime * 5f;
                            switch (i)
                            {
                                case 1:
                                    if (!playerInventory.AllowShowItem)
                                    {
                                        flag = false;
                                    }
                                    break;
                                case 3:
                                    if (handAnimationController.itemGrab.grabbing)
                                    {
                                        flag = true;
                                        maxDelta = 1f;
                                    }
                                    break;
                                case 4:
                                    if (playerInventory.CurrentItem != null && playerInventory.CurrentItem.additiveAnimation != PlayerAnimations.RightArmAdditive.None)
                                    {
                                        flag = true;
                                        maxDelta = 1f;
                                    }
                                    break;
                                case 5:
                                    if (playerInventory.CurrentItem != null && playerInventory.CurrentItem.twoHandedAnimation != PlayerAnimations.TwoHandedAnimation.None)
                                    {
                                        flag = true;
                                    }
                                    break;
                            }
                            handAnimationController.layerWeights[i] = Mathf.MoveTowards(handAnimationController.layerWeights[i], (!flag) ? 0f : 1f, maxDelta);
                            handAnimationController.playerAnimator.SetLayerWeight(i, Mathf.Clamp01(handAnimationController.layerWeights[i]));
                        }
                    }
                    catch
                    {
                        Debug.Log("Error in HA 1");
                    }
                    try
                    {

                        PlayerAnimations.RightHandAnimations value = PlayerAnimations.RightHandAnimations.None;
                        bool value2 = false;
                        PlayerAnimations.RightArmAdditive value3 = PlayerAnimations.RightArmAdditive.None;
                        PlayerAnimations.TwoHandedAnimation value4 = PlayerAnimations.TwoHandedAnimation.None;
                        if (handAnimationController.player.IsStanding())
                        {
                            handAnimationController.bodyLock.weighting = Mathf.MoveTowards(handAnimationController.bodyLock.weighting, handAnimationController.layerWeights[1], Time.deltaTime * 4f);
                        }
                        else
                        {
                            handAnimationController.bodyLock.weighting = Mathf.MoveTowards(handAnimationController.bodyLock.weighting, 0f, Time.deltaTime * 4f);
                        }
                        if (currentItem != null)
                        {
                            value = currentItem.holdAnimation;
                            value2 = currentItem.twoHanded;
                            value3 = currentItem.additiveAnimation;
                            value4 = currentItem.twoHandedAnimation;
                        }
                        handAnimationController.playerAnimator.SetBool("TwoHandedItem", value2);
                        handAnimationController.playerAnimator.SetInteger("RightHandAnimation", (int)value);
                        handAnimationController.playerAnimator.SetInteger("RightArmAdditive", (int)value3);
                        handAnimationController.playerAnimator.SetInteger("TwoHandedAnimation", (int)value4);
                    }
                    catch
                    {
                        Debug.Log("Error in HA 2");
                    }
                }
            }

            private static void HookHandAnimationControllerStart(On.HandAnimationController.orig_Start orig, HandAnimationController handAnimationController)
            {
                orig.Invoke(handAnimationController);
                handAnimationController.player = ((MonoBehaviour)handAnimationController).GetComponent<NewPlayerClass>();
                handAnimationController.bodyLock = handAnimationController.player.upperBodyLock;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @HandGrabCondition

            private static void HookHandGrabCondition()
            {
                On.HandGrabCondition.IsConditionMet += new On.HandGrabCondition.hook_IsConditionMet(HookHandGrabConditionIsConditionMet);
            }

            private static bool HookHandGrabConditionIsConditionMet(On.HandGrabCondition.orig_IsConditionMet orig, HandGrabCondition handGrabCondition)
            {
                bool condition = handGrabCondition.handGrab.IsCloseEnough(FindClosestIK(handGrabCondition.handGrab, lastPlayerCheckingInteractableConditions).position);
                Debug.Log("Checking HandGrab condition. It is " + condition.ToString() + " for player number " + PlayerNumber(lastPlayerCheckingInteractableConditions.GetInstanceID()));
                return condition;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @HandGrabIK

            private static void HookHandGrabIK()
            {
                On.HandGrabIK.HandRelease += new On.HandGrabIK.hook_HandRelease(HookHandGrabIKHandRelease);
                On.HandGrabIK.IsCloseEnough += new On.HandGrabIK.hook_IsCloseEnough(HookHandGrabIKIsCloseEnough);
                On.HandGrabIK.StartGrab += new On.HandGrabIK.hook_StartGrab(HookHandGrabIKStartGrab);
            }

            private static float AngleToHandIK(HandGrabIK handGrabIK, GameObject playerGameObject)
            {
                Vector3 forward = playerGameObject.transform.forward;
                forward.y = 0f;
                Vector3 to = handGrabIK.handIK.position - playerGameObject.transform.position;
                to.Normalize();
                to.y = 0f;
                return Vector3.Angle(forward, to);
            }

            private static void BeginIK(HandGrabIK _item, NewPlayerClass newPlayerClass)
            {
                ItemGrabIK itemGrabIK = newPlayerClass.GetComponent<ItemGrabIK>();
                if (!itemGrabIK.grabbing && itemGrabIK.t < 0.2f)
                {
                    itemGrabIK.grabbing = true;
                    itemGrabIK.ikActive = true;
                    itemGrabIK.ikTarget = _item;
                    itemGrabIK.ikTransform = FindClosestIK(itemGrabIK.ikTarget, newPlayerClass);
                    itemGrabIK.grabbed = false;
                    itemGrabIK.ikPositionCache = itemGrabIK.ikTransform.position;
                    itemGrabIK.rotationTypeCache = itemGrabIK.ikTarget.rotationType;
                    itemGrabIK.ikTarget.ShouldRelease = false;
                    itemGrabIK.holdTime = 0f;
                    Vector3 position = ((MonoBehaviour)itemGrabIK).transform.position;
                    Vector3 b = itemGrabIK.ikPositionCache;
                    position.y = (b.y = 0f);
                    itemGrabIK.tScaler = Mathf.Clamp01(Vector3.Distance(position, b) / 0.65f);
                }
            }

            private static void CalculateClosestPointOnCollider(HandGrabIK handGrabIK, Vector3 playerCameraPosition)
            {
                Ray ray = default(Ray);
                Vector3 position = playerCameraPosition;
                Vector3 vector = ((MonoBehaviour)handGrabIK).transform.position - position;
                ray.origin = position;
                ray.direction = vector.normalized;
                RaycastHit raycastHit;
                handGrabIK.closestPointOnCollider.Raycast(ray, out raycastHit, vector.magnitude);
                handGrabIK.closestPointOnColliderTrans.position = raycastHit.point;
                handGrabIK.closestPointOnColliderTrans.rotation = Quaternion.identity;
            }

            private static Transform FindClosestIK(HandGrabIK handGrabIK, NewPlayerClass newPlayerClass)
            {
                Vector3 position = PlayerCamera(newPlayerClass).transform.position;

                /*
                Debug.Log("Trying to get item grab from player children.");
                handGrabIK.itemGrab = newPlayerClass.GetComponentInChildren<ItemGrabIK>();
                if (handGrabIK.itemGrab == null)
                {
                    Debug.Log("Trying to get item grab from player directly.");
                    handGrabIK.itemGrab = newPlayerClass.GetComponent<ItemGrabIK>();
                }
                else
                {
                    Debug.Log("Found ItemGrabIK from player's children. lastPlayerCheckingInteractableConditions player number is " + PlayerNumber(lastPlayerCheckingInteractableConditions.GetInstanceID()) + " and ItemGrabIK player number is " + handGrabIK.itemGrab.GetComponent<NewPlayerClass>());
                }
                */

                Transform transform = null;
                for (int i = 0; i < handGrabIK.transformIKs.Count; i++)
                {
                    if (transform == null || Vector3.Distance(transform.position, position) > Vector3.Distance(handGrabIK.transformIKs[i].position, position))
                    {
                        transform = handGrabIK.transformIKs[i];
                    }
                }
                if (transform == null)
                {
                    return ((MonoBehaviour)handGrabIK).transform;
                }
                return transform;
            }

            /*
            private static void HandRelease(HandGrabIK handGrabIK, NewPlayerClass newPlayerClass)
            {
                handGrabIK.SendMessageCustom("OnHandRelease");
                newPlayerClass.HandRelease();
            }
            */

            private static void HookHandGrabIKHandRelease(On.HandGrabIK.orig_HandRelease orig, HandGrabIK handGrabIK)
            {
                handGrabIK.SendMessageCustom("OnHandRelease");
                lastPlayerSentMessage.HandRelease();
            }

            private static bool HookHandGrabIKIsCloseEnough(On.HandGrabIK.orig_IsCloseEnough orig, HandGrabIK handGrabIK, Vector3 _handIKPosition)
            {
                float distance = Vector3.Distance(_handIKPosition, lastPlayerSentMessage.gameObject.GetComponent<ItemGrabIK>().shoulderTransform.position);
                return distance < handGrabIK.leaveTolerance;
            }

            private static IEnumerator RotateToFaceIK(HandGrabIK handGrabIK, NewPlayerClass newPlayerClass)
            {
                if (!ModSettings.enableCrewVSMonsterMode || (ModSettings.enableCrewVSMonsterMode && !ModSettings.numbersOfMonsterPlayers.Contains(PlayerNumber(newPlayerClass.GetInstanceID()))))
                {
                    if (ModSettings.logDebugText)
                    {
                        Debug.Log("Rotating player number " + PlayerNumber(newPlayerClass.GetInstanceID()) + " to face IK");
                    }
                    while (AngleToHandIK(handGrabIK, newPlayerClass.gameObject) > 5f && AngleToHandIK(handGrabIK, newPlayerClass.gameObject) < 90f && handGrabIK.grabbing)
                    {
                        Vector3 pos = handGrabIK.handIK.position - newPlayerClass.gameObject.transform.position;
                        pos.Normalize();
                        pos.y = 0f;
                        newPlayerClass.gameObject.transform.rotation = Quaternion.RotateTowards(handGrabIK.startRotation, Quaternion.LookRotation(pos, Vector3.up), Time.deltaTime * 100f);
                        handGrabIK.startRotation = newPlayerClass.gameObject.transform.rotation;
                        yield return null;
                    }
                }
                yield break;
            }

            private static void HookHandGrabIKStartGrab(On.HandGrabIK.orig_StartGrab orig, HandGrabIK handGrabIK)
            {
                if (!lastPlayerSentMessage.Motor.disableMove)
                {
                    if (ModSettings.logDebugText)
                    {
                        Debug.Log("Starting grab for player number " + PlayerNumber(lastPlayerSentMessage.GetInstanceID()));
                    }
                    handGrabIK.grabbing = true;
                    handGrabIK.startRotation = lastPlayerSentMessage.gameObject.transform.rotation;
                    handGrabIK.handIK = FindClosestIK(handGrabIK, lastPlayerSentMessage);
                    if (handGrabIK.rotateToFaceIK)
                    {
                        ((MonoBehaviour)handGrabIK).StartCoroutine(RotateToFaceIK(handGrabIK, lastPlayerSentMessage));
                    }
                    if (handGrabIK.closestPointOnCollider != null)
                    {
                        CalculateClosestPointOnCollider(handGrabIK, PlayerCamera(lastPlayerSentMessage).transform.position);
                    }
                    PlayerAnimations.LeftHandAnimations leftHandAnimations = handGrabIK.handAnimation;
                    if (leftHandAnimations == PlayerAnimations.LeftHandAnimations.Grab)
                    {
                        lastPlayerSentMessage.HandGrab();
                    }
                    BeginIK(handGrabIK, lastPlayerSentMessage);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @HeadIKMovement

            private static void HookHeadIKMovementCheckAngle(On.HeadIKMovement.orig_CheckAngle orig, HeadIKMovement headIKMovement)
            {
                if (Vector3.Angle(((MonoBehaviour)headIKMovement).transform.position, headIKMovement.monsterScript.player.transform.position) < 75f)
                {
                    headIKMovement.withinAngle = true;
                }
                else
                {
                    headIKMovement.withinAngle = false;
                }
            }

            private static void HookHeadIKMovementOnAnimatorIK(On.HeadIKMovement.orig_OnAnimatorIK orig, HeadIKMovement headIKMovement, int layer)
            {
                if (headIKMovement.ani == null)
                {
                    headIKMovement.ani = headIKMovement.monAnimation.GetAnimator;
                }
                if (headIKMovement.currentState.GetType() == typeof(MWanderState))
                {
                    headIKMovement.raycastTarget = headIKMovement.monsterScript.MoveControl.Goal;
                    if (!headIKMovement.castHit && headIKMovement.withinAngle)
                    {
                        headIKMovement.LerpHead(1f, headIKMovement.raycastTarget);
                    }
                    else
                    {
                        headIKMovement.currentPos = headIKMovement.monsterScript.MoveControl.AheadNodePos + ((MonoBehaviour)headIKMovement).transform.forward * 10f;
                        headIKMovement.LerpHead(0f, headIKMovement.currentPos);
                    }
                }
                else if (headIKMovement.currentState.GetType() == typeof(MSearchingState))
                {
                    if (headIKMovement.monsterScript.TimeSinceIncrease < 2f)
                    {
                        headIKMovement.raycastTarget = headIKMovement.monsterScript.Searcher.GetExactSpot + Vector3.up * 0.1f;
                    }
                    else
                    {
                        headIKMovement.raycastTarget = headIKMovement.monsterScript.MoveControl.Goal + Vector3.up * 0.1f;
                    }
                    if (!headIKMovement.castHit && headIKMovement.withinAngle)
                    {
                        headIKMovement.LerpHead(1f, headIKMovement.raycastTarget);
                    }
                    else
                    {
                        headIKMovement.currentPos = ((MonoBehaviour)headIKMovement).transform.position + ((MonoBehaviour)headIKMovement).transform.forward;
                        headIKMovement.LerpHead(0f, headIKMovement.currentPos);
                    }
                }
                else if (headIKMovement.currentState.GetType() == typeof(MChasingState))
                {
                    if (headIKMovement.monsterScript.CanSeePlayer && headIKMovement.withinAngle)
                    {
                        headIKMovement.LerpHead(1f, headIKMovement.monsterScript.player.transform.position);
                    }
                    else
                    {
                        headIKMovement.currentPos = headIKMovement.monsterScript.MoveControl.AheadNodePos + ((MonoBehaviour)headIKMovement).transform.forward * 10f;
                        headIKMovement.LerpHead(0f, headIKMovement.currentPos);
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @HeadLights

            private static void HookHeadLightsOnStartItemAnimation(On.HeadLights.orig_OnStartItemAnimation orig, HeadLights headLights)
            {
                int playerNumber = PlayerNumber(InventoryFromItemClass(headLights.gameObject).newPlayerClass.GetInstanceID());
                headLights.playerClipCamera = newPlayerClasses[playerNumber].gameObject.GetComponentInChildren<PlayerClipCamera>();
                ((MonoBehaviour)headLights).StartCoroutine(HeadLightsLerp(headLights, playerNumber));
            }

            private static IEnumerator HeadLightsLerp(HeadLights headLights, int playerNumber)
            {
                yield return new WaitForSeconds(1.1333f);
                ((MonoBehaviour)headLights).transform.parent = newPlayerClasses[playerNumber].gameObject.transform;
                headLights.lerp.target = Submarine.mainSub.headLightPosition;
                headLights.lerp.speed = 2f;
                yield return ((MonoBehaviour)headLights).StartCoroutine(headLights.lerp.Lerp(headLights.ani.transform.parent));
                Submarine.mainSub.subHeadLightsFixed = true;
                yield return new WaitForSeconds(0.5f);
                inventories[playerNumber].DestroyItem(inventories[playerNumber].CurrentSlot);
                headLights.playerClipCamera.EnableCamera(true);
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @HeightCondition

            private static bool HookHeightConditionIsConditionMet(On.HeightCondition.orig_IsConditionMet orig, HeightCondition heightCondition)
            {
                bool condition = lastPlayerCheckingInteractableConditions.transform.position.y - ((InteractableCondition)heightCondition).GetComponent<FixedInteractable>().animationTransform.gameObject.transform.position.y <= 0.5f;
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Checking height condition. It is " + condition.ToString() + " for player number " + PlayerNumber(lastPlayerCheckingInteractableConditions.GetInstanceID()));
                }
                return condition;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @HelicopterChain

            private static void HookHelicopterChainBreak(On.HelicopterChain.orig_Break orig, HelicopterChain helicopterChain)
            {
                if (!helicopterChain.broken)
                {
                    Inventory inventory = PlayerInventory(lastPlayerSentMessage);
                    if (inventory.CurrentItem != null && inventory.CurrentItem.name.Contains("BoltCutters"))
                    {
                        HelicopterChain.numberofChainsCut--;
                        if (HelicopterChain.numberofChainsCut <= 0)
                        {
                            Objectives.Tasks("HelicopterEscape").Tasks("CutCables").Complete();
                        }
                    }
                    helicopterChain.interactable.enabled = false;
                    ((MonoBehaviour)helicopterChain).GetComponent<Interactable>().active = false;
                    helicopterChain.broken = true;
                    Transform transform = ((MonoBehaviour)helicopterChain).transform.FindChild("Node 0 Link " + (helicopterChain.rope.transform.childCount / 2 + 1));
                    ConfigurableJoint component = transform.GetComponent<ConfigurableJoint>();
                    component.breakForce = 0f;
                    helicopterChain.helicopter.TestCableObjective();
                    Rigidbody[] componentsInChildren = ((MonoBehaviour)helicopterChain).GetComponentsInChildren<Rigidbody>();
                    foreach (Rigidbody rigidbody in componentsInChildren)
                    {
                        rigidbody.WakeUp();
                    }
                ((MonoBehaviour)helicopterChain).BroadcastMessage("OnRopeBreak", SendMessageOptions.DontRequireReceiver);
                    Transform transform2 = ((MonoBehaviour)helicopterChain).transform.FindChild("Node 0 Link " + helicopterChain.rope.transform.childCount / 2);
                    transform2.gameObject.AddComponent<ChainCutPoint>();
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @HelicopterEscape

            private static void HookHelicopterEscapeUpdate(On.HelicopterEscape.orig_Update orig, HelicopterEscape helicopterEscape)
            {
                if (!helicopterEscape.destroyed)
                {
                    if (helicopterEscape.fuellingBegin)
                    {
                        helicopterEscape.helicopterFuelTimer += Time.deltaTime;
                        if (helicopterEscape.startBlades)
                        {
                            helicopterEscape.smallRotor.rotorAcceleration = 100f;
                            helicopterEscape.bigRotor.rotorAcceleration = 60f;
                        }
                        else if (helicopterEscape.helicopterFuelTimer >= helicopterEscape.maxTimer)
                        {
                            Objectives.Tasks("HelicopterEscape").Tasks("DefendHelicopter").Complete();
                            helicopterEscape.StartBlades();
                        }
                    }
                }
                else if (helicopterEscape.helicopterFuelTimer < helicopterEscape.maxTimer)
                {
                    helicopterEscape.chopperSource.Stop();
                    VirtualAudioSource virtualAudioSource = helicopterEscape.chopperSource.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                    helicopterEscape.smallRotor.StopRotate();
                    helicopterEscape.bigRotor.StopRotate();
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @HelicopterKeys

            private static void HookHelicopterKeysOnFinishItemAnimation(On.HelicopterKeys.orig_OnFinishItemAnimation orig, HelicopterKeys helicopterKeys)
            {
                Inventory inventory = InventoryFromItemClass(helicopterKeys.gameObject);
                inventory.DestroyItem(inventory.CurrentSlot);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Hiding

            private static void HookHidingAwake(On.Hiding.orig_Awake orig, Hiding hiding)
            {
                hiding.player = ((MonoBehaviour)hiding).GetComponentInParent<NewPlayerClass>();
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @HookCondition

            private static bool HookHookConditionIsConditionMet(On.HookCondition.orig_IsConditionMet orig, HookCondition hookCondition)
            {
                hookCondition.itemGrab = lastPlayerCheckingInteractableConditions.GetComponent<ItemGrabIK>();
                bool condition = orig.Invoke(hookCondition);
                Debug.Log("Checking hook condition. It is " + condition.ToString() + " for player number " + PlayerNumber(lastPlayerCheckingInteractableConditions.GetInstanceID()));
                return condition;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @InterpolateToPosition

            private static void HookInterpolateToPosition()
            {
                On.InterpolateToPosition.StartLerp += new On.InterpolateToPosition.hook_StartLerp(HookInterpolateToPositionStartLerp);
            }

            private static IEnumerator InterpolateToPositionLerp(InterpolateToPosition interpolateToPosition, GameObject playerGameObject)
            {
                if (!ModSettings.enableCrewVSMonsterMode || (ModSettings.enableCrewVSMonsterMode && !ModSettings.numbersOfMonsterPlayers.Contains(PlayerNumber(playerGameObject.GetComponent<NewPlayerClass>().GetInstanceID()))))
                {
                    //Debug.Log("playerGameObject at InterpolateToPosition.Lerp is " + playerGameObject.GetInstanceID());
                    Debug.Log("Player number at InterpolateToPosition.Lerp is " + PlayerNumber(playerGameObject.GetComponent<NewPlayerClass>().GetInstanceID()));
                    Transform playerTrans = playerGameObject.transform;
                    CharacterController controller = playerGameObject.GetComponent<CharacterController>();
                    interpolateToPosition.startPosition = playerTrans.position;
                    interpolateToPosition.startRotation = playerTrans.rotation;
                    Vector3 goalPosition = interpolateToPosition.goal.position;
                    goalPosition.y = interpolateToPosition.startPosition.y;
                    Vector3 goalRotationPosition = interpolateToPosition.lookAtGoal.transform.position;
                    goalRotationPosition.y = goalPosition.y;
                    Quaternion goalRotation = Quaternion.LookRotation(goalRotationPosition - (goalPosition + Vector3.forward * 0.0001f), Vector3.up);
                    float distance = Vector3.Distance(interpolateToPosition.startPosition, goalPosition);
                    float angleDifference = Quaternion.Angle(interpolateToPosition.startRotation, goalRotation);
                    bool sentFinishEvent = false;
                    float moveT = 0f;
                    float angleT = 0f;
                    float moveTSpeed = interpolateToPosition.speed / distance;
                    float angleTSpeed = interpolateToPosition.angleSpeed / Mathf.Max(angleDifference, 10f);
                    bool collision = false;
                    Vector3 origin = Vector3.zero;
                    float radius = 0f;
                    while (moveT < 1f || angleT < 1f)
                    {
                        playerGameObject.transform.position = MathHelper.SmoothStep(interpolateToPosition.startPosition, goalPosition, moveT);
                        playerGameObject.transform.rotation = Quaternion.Slerp(interpolateToPosition.startRotation, goalRotation, angleT);
                        if (moveT > interpolateToPosition.sendFinishEventAt)
                        {
                            lastPlayerSentMessage = playerGameObject.GetComponent<NewPlayerClass>();
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("Updating lastPlayerSentMessage: " + PlayerNumber(lastPlayerSentMessage.GetInstanceID()) + "\n" + new StackTrace().ToString() + "\n-----");
                            }
                            ((MonoBehaviour)interpolateToPosition).SendMessage("OnInterpolateFinished");
                            sentFinishEvent = true;
                        }
                        origin = controller.bounds.center;
                        radius = controller.radius * 1.3f;
                        Collider[] colls = Physics.OverlapSphere(origin, radius, 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("DefaultNavVision"));
                        for (int i = 0; i < colls.Length; i++)
                        {
                            if (colls[i].transform.GetParentOfType<LevelGeneration>() != null)
                            {
                                collision = true;
                                break;
                            }
                        }
                        if (collision)
                        {
                            break;
                        }
                        if (Vector3.Distance(goalPosition, playerGameObject.transform.position) < interpolateToPosition.getWithin)
                        {
                            break;
                        }
                        moveT += Time.deltaTime * moveTSpeed;
                        angleT += Time.deltaTime * angleTSpeed;
                        yield return null;
                    }
                    if (moveT >= 1f)
                    {
                        playerGameObject.transform.position = goalPosition;
                    }
                    if (angleT >= 1f)
                    {
                        playerGameObject.transform.rotation = goalRotation;
                    }
                    if (!sentFinishEvent)
                    {
                        lastPlayerSentMessage = playerGameObject.GetComponent<NewPlayerClass>();
                        if (ModSettings.logDebugText)
                        {
                            Debug.Log("Updating lastPlayerSentMessage: " + PlayerNumber(lastPlayerSentMessage.GetInstanceID()) + "\n" + new StackTrace().ToString() + "\n-----");
                        }
                        ((MonoBehaviour)interpolateToPosition).SendMessage("OnInterpolateFinished");
                        sentFinishEvent = true;
                    }
                }
                else
                {
                    lastPlayerSentMessage = playerGameObject.GetComponent<NewPlayerClass>();
                    if (ModSettings.logDebugText)
                    {
                        Debug.Log("Updating lastPlayerSentMessage: " + PlayerNumber(lastPlayerSentMessage.GetInstanceID()) + "\n" + new StackTrace().ToString() + "\n-----");
                    }
                    ((MonoBehaviour)interpolateToPosition).SendMessage("OnInterpolateFinished");
                }
                yield break;
            }

            private static void HookInterpolateToPositionStartLerp(On.InterpolateToPosition.orig_StartLerp orig, InterpolateToPosition interpolateToPosition)
            {
                if (interpolateToPosition.goal != null)
                {
                    ((MonoBehaviour)interpolateToPosition).StartCoroutine(InterpolateToPositionLerp(interpolateToPosition, lastPlayerSentMessage.gameObject));
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Inventory

            private static void HookInventory()
            {
                // Start is already hooked in BaseFeatures. Disable that hook when multiplayer mode is enabled.
                On.Inventory.InventoryViaKeyboard += new On.Inventory.hook_InventoryViaKeyboard(HookInventoryInventoryViaKeyboard);
                On.Inventory.ScrollThroughInventory += new On.Inventory.hook_ScrollThroughInventory(HookInventoryScrollThroughInventory);
                On.Inventory.Start += new On.Inventory.hook_Start(HookInventoryStart);
                On.Inventory.Update += new On.Inventory.hook_Update(HookInventoryUpdate);
                On.Inventory.ThrowItem += new On.Inventory.hook_ThrowItem(HookInventoryThrowItem);
            }

            private static void HookInventoryThrowItem(On.Inventory.orig_ThrowItem orig, Inventory inventory, InventorySlot _slot)
            {
                InventoryItem item = _slot.Item;
                inventory.DropItem(_slot);
                InventoryItem.inventory = inventory;
                item.ThrowItem(inventory.throwPower);
            }

            public static bool HookInventoryget_AllowShowItem(Inventory inventory)
            {
                bool result = false;
                if (inventory.CurrentItem == null)
                {
                    result = true;
                }
                else if (inventory.newPlayerClass.IsStanding())
                {
                    result = true;
                }
                else if (inventory.newPlayerClass.IsCrouched() && inventory.CurrentItem.useItemCrouch && (inventory.newPlayerClass.IsFullyCrouched() || inventory.CurrentItem.useItemCrouch) && Mathf.Abs(PlayerCamera(inventory.newPlayerClass).transform.position.y - inventory.newPlayerClass.gameObject.transform.position.y) > 0.5f)
                {
                    result = true;
                }
                else if (inventory.newPlayerClass.IsProne())
                {
                    result = false;
                }
                if (inventory.hideItem)
                {
                    result = false;
                }
                return result;
            }

            private static void HookInventoryInventoryViaKeyboard(On.Inventory.orig_InventoryViaKeyboard orig, Inventory inventory)
            {
                if (PlayerNumber(inventory.newPlayerClass.GetInstanceID()) == 0)
                {
                    orig.Invoke(inventory);
                }
            }

            private static void HookInventoryScrollThroughInventory(On.Inventory.orig_ScrollThroughInventory orig, Inventory inventory)
            {
                int playerNumber = PlayerNumber(inventory.newPlayerClass.GetInstanceID());
                if (inventory.AllowItemSwitching)
                {
                    if ((playerNumber == 0 && Input.GetAxis("Mouse ScrollWheel") > 0f) || (playerNumber == 1 && XboxCtrlrInput.XCI.GetButtonDown(XboxCtrlrInput.XboxButton.DPadRight)))
                    {
                        inventory.SwitchItem(inventory.currentSlot + 1, false);
                    }
                    if ((playerNumber == 0 && Input.GetAxis("Mouse ScrollWheel") < 0f) || (playerNumber == 1 && XboxCtrlrInput.XCI.GetButtonDown(XboxCtrlrInput.XboxButton.DPadLeft)))
                    {
                        inventory.SwitchItem(inventory.currentSlot - 1, false);
                    }
                    if ((playerNumber == 0 && (Input.GetAxis("Mouse ScrollWheel") < 0f || Input.GetAxis("Mouse ScrollWheel") > 0f)) || (playerNumber == 1 && (XboxCtrlrInput.XCI.GetButtonDown(XboxCtrlrInput.XboxButton.DPadLeft) || XboxCtrlrInput.XCI.GetButtonDown(XboxCtrlrInput.XboxButton.DPadRight))))
                    {
                        inventory.DisplayInventory();
                    }
                }
            }

            private static void HookInventoryStart(On.Inventory.orig_Start orig, Inventory inventory)
            {
                if (ModSettings.inventorySize != 0)
                {
                    inventory.maxInventoryCapacity = ModSettings.inventorySize;
                }
                orig.Invoke(inventory);
                if (newPlayerClasses != null)
                {
                    int playerNumber = PlayerNumberFromInventory(inventory.GetInstanceID());
                    inventory.newPlayerClass = newPlayerClasses[playerNumber];
                }
            }

            private static void HookInventoryUpdate(On.Inventory.orig_Update orig, Inventory inventory)
            {
                if (!inventory.pause && !inventory.journal.IsActive)
                {
                    if (inventory.CurrentItem != null)
                    {
                        if (inventory.AllowShowItem)
                        {
                            if (!inventory.CurrentItem.gameObject.activeSelf)
                            {
                                inventory.EquipCurrentItem();
                            }
                        }
                        else if ((inventory.handAnimationController.layerWeights[1] <= 0f || inventory.newPlayerClass.IsProne()) && inventory.CurrentItem.gameObject.activeSelf)
                        {
                            inventory.CurrentItem.DeEquipItem();
                            inventory.DeactivateItem(inventory.CurrentSlotID);
                        }
                        int playerNumber = PlayerNumber(inventory.newPlayerClass.GetInstanceID());
                        if (!inventory.pauseBreak && !inventory.fio.InteractThisFrame)
                        {
                            if (GetPlayerKey("UseItem", playerNumber).JustPressed() || GetPlayerTriggerStateIfUsingController("Right", playerNumber, true))
                            {
                                inventory.CurrentItem.UseItem();
                            }
                            if (GetPlayerKey("UseItem", playerNumber).JustReleased() || GetPlayerTriggerStateIfUsingController("Right", playerNumber, false, true))
                            {
                                inventory.CurrentItem.UseReleaseItem();
                            }
                        }
                        else
                        {
                            inventory.pauseBreak = false;
                        }
                        if (inventory.AllowItemSwitching && (!inventory.newPlayerClass.IsCrouched() || (inventory.newPlayerClass.IsCrouched() && inventory.CurrentItem.useItemCrouch)))
                        {
                            if (GetPlayerKey("DropItem", playerNumber).IsDown())
                            {
                                inventory.throwPower += Time.deltaTime * inventory.throwChargeSpeed;
                            }
                            if (inventory.maxThrowPower < inventory.throwPower)
                            {
                                inventory.throwPower = inventory.maxThrowPower;
                            }
                            if (GetPlayerKey("DropItem", playerNumber).JustReleased() && inventory.CurrentItem != null)
                            {
                                bool flag = false;
                                Debug.Log("Player attempting to drop item is player number " + PlayerNumber(inventory.newPlayerClass.GetInstanceID()));
                                Vector3 position = inventory.newPlayerClass.gameObject.transform.position;
                                position.y = inventory.CurrentItem.transform.position.y;
                                Vector3 vector = inventory.CurrentItem.modelTrans.position + inventory.newPlayerClass.gameObject.transform.forward * 0.2f - position;
                                LayerMask mask = 1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("DefaultNavVision");
                                Vector3 position2 = inventory.CurrentItem.modelTrans.position;
                                if (Physics.Raycast(position, vector.normalized, vector.magnitude, mask))
                                {
                                    float num = 45f;
                                    Vector3 forward = inventory.newPlayerClass.gameObject.transform.forward;
                                    for (float num2 = 0f; num2 < 360f; num2 += num)
                                    {
                                        vector = Quaternion.AngleAxis(num2, Vector3.up) * forward;
                                        if (!Physics.Raycast(position, vector, 0.5f, mask))
                                        {
                                            Vector3 direction = Quaternion.AngleAxis(num2 + 15f, Vector3.up) * forward;
                                            if (!Physics.Raycast(position, direction, 0.5f, mask))
                                            {
                                                direction = Quaternion.AngleAxis(num2 - 15f, Vector3.up) * forward;
                                                if (!Physics.Raycast(position, direction, 0.5f, mask))
                                                {
                                                    position2 = position + vector * 0.4f;
                                                    flag = true;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                    if (!flag)
                                    {
                                        position = PlayerCamera(inventory.newPlayerClass).transform.position;
                                        if (!Physics.Raycast(position, Vector3.up, 0.4f, mask))
                                        {
                                            position2 = position + Vector3.up * 0.15f;
                                            flag = true;
                                        }
                                    }
                                }
                                else
                                {
                                    flag = true;
                                }
                                if (flag)
                                {
                                    inventory.CurrentItem.modelTrans.position = position2;
                                    inventory.ThrowItem(inventory.CurrentSlot);
                                    inventory.throwPower = inventory.startThrowPower;
                                }
                            }
                        }
                        inventory.InventoryViaKeyboard();
                        inventory.ScrollThroughInventory();
                        if (inventory.CurrentItem != null && inventory.AllowShowItem && inventory.CurrentItem.transform.parent == inventory.handTransform)
                        {
                            inventory.UpdateCurrentItem();
                        }
                    }
                }
                else if (inventory.hideItem)
                {
                    inventory.DeactivateItem(inventory.CurrentSlotID);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @InventoryIcons

            private static void HookInventoryIconsStart(On.InventoryIcons.orig_Start orig, InventoryIcons inventoryIcons)
            {
                orig.Invoke(inventoryIcons);
            }

            private static void HookInventoryIconsUpdate(On.InventoryIcons.orig_Update orig, InventoryIcons inventoryIcons)
            {
                Inventory inventory = PlayerInventory(inventoryIcons.inventoryUI.newPlayerClass);
                if (inventory.CurrentItem != null && inventoryIcons.inventoryUI.displayInventory && OculusManager.isOculusEnabled)
                {
                    inventoryIcons.itemNameCanvas.transform.parent = null;
                    inventoryIcons.oculusIconParent.transform.parent = null;
                    inventoryIcons.itemNameCanvas.transform.parent = InventoryIcons.oculusPlayerHUD.gameObject.transform;
                    inventoryIcons.oculusIconParent.transform.parent = InventoryIcons.oculusPlayerHUD.gameObject.transform;
                    inventoryIcons.itemNameCanvas.transform.localRotation = Quaternion.identity;
                    inventoryIcons.oculusIconParent.transform.localRotation = Quaternion.identity;
                    Vector3 localPosition = Vector3.zero;
                    localPosition = inventoryIcons.itemNameCanvas.transform.localPosition;
                    localPosition.x = 0f;
                    localPosition.y = 0f;
                    localPosition.z = 0.3f;
                    inventoryIcons.itemNameCanvas.transform.localPosition = localPosition;
                    inventoryIcons.oculusIconParent.transform.localPosition = localPosition;
                    if (inventoryIcons.itemNameCanvas.GetComponentInChildren<RectTransform>() != null)
                    {
                        inventoryIcons.itemNameCanvas.GetComponentInChildren<RectTransform>().localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
                        inventoryIcons.itemNameCanvas.GetComponentInChildren<RectTransform>().GetComponentInChildren<Text>().text = inventory.CurrentItem.itemName;
                        if (inventoryIcons.itemNameCanvas.GetComponentInChildren<RectTransform>().GetComponentInChildren<Text>() != null)
                        {
                            inventoryIcons.tempOculusColor = inventoryIcons.itemNameCanvas.GetComponentInChildren<RectTransform>().GetComponentInChildren<Text>().color;
                            inventoryIcons.tempOculusColor.a = inventoryIcons.originalColor.a;
                            inventoryIcons.itemNameCanvas.GetComponentInChildren<RectTransform>().GetComponentInChildren<Text>().color = inventoryIcons.tempOculusColor;
                        }
                    }
                    inventoryIcons.FadeString();
                    if (!inventoryIcons.lastDisplayed && inventoryIcons.inventoryUI.displayInventory)
                    {
                        inventoryIcons.currentColor = inventoryIcons.originalColor;
                        inventoryIcons.playerStyle.normal.textColor = inventoryIcons.originalColor;
                        inventoryIcons.itemNameText.color = inventoryIcons.originalColor;
                        for (int i = 0; i < inventoryIcons.oculusSpriteIconList.Count; i++)
                        {
                            inventoryIcons.oculusSpriteIconList[i].color = inventoryIcons.originalColor;
                        }
                    }
                    for (int j = 0; j < inventoryIcons.oculusSpriteIconList.Count; j++)
                    {
                        if (j < inventory.CurrentItem.itemIcons.Count)
                        {
                            inventoryIcons.oculusSpriteIconList[j].sprite = inventory.CurrentItem.itemIcons[j];
                        }
                        else
                        {
                            inventoryIcons.oculusSpriteIconList[j].color = new Color(1f, 1f, 1f, 0f);
                        }
                    }
                }
                if (inventory.CurrentItem != null && inventoryIcons.inventoryUI.displayInventory)
                {
                    string itemName = inventory.CurrentItem.itemName;
                    if (!OculusManager.isOculusEnabled)
                    {
                        if (!inventoryIcons.lastDisplayed && inventoryIcons.inventoryUI.displayInventory)
                        {
                            inventoryIcons.currentColor = inventoryIcons.originalColor;
                            inventoryIcons.playerStyle.normal.textColor = inventoryIcons.originalColor;
                            inventoryIcons.itemNameText.color = inventoryIcons.originalColor;
                            for (int k = 0; k < inventoryIcons.spriteIconList.Count; k++)
                            {
                                inventoryIcons.spriteIconList[k].color = inventoryIcons.originalColor;
                            }
                        }
                        inventoryIcons.FadeString();
                        inventoryIcons.itemNameText.text = itemName;
                        for (int l = 0; l < inventoryIcons.spriteIconList.Count; l++)
                        {
                            if (l < inventory.CurrentItem.itemIcons.Count)
                            {
                                inventoryIcons.spriteIconList[l].sprite = inventory.CurrentItem.itemIcons[l];
                            }
                            else
                            {
                                inventoryIcons.spriteIconList[l].color = new Color(1f, 1f, 1f, 0f);
                            }
                        }
                    }
                }
                if (inventory.ItemCount == 0 || !inventoryIcons.inventoryUI.displayInventory)
                {
                    if (!OculusManager.isOculusEnabled)
                    {
                        for (int m = 0; m < inventoryIcons.spriteIconList.Count; m++)
                        {
                            inventoryIcons.spriteIconList[m].color = new Color(1f, 1f, 1f, 0f);
                        }
                    }
                    else
                    {
                        for (int n = 0; n < inventoryIcons.oculusSpriteIconList.Count; n++)
                        {
                            inventoryIcons.oculusSpriteIconList[n].color = new Color(1f, 1f, 1f, 0f);
                        }
                    }
                    inventoryIcons.itemNameText.text = string.Empty;
                }
                inventoryIcons.lastDisplayed = inventoryIcons.inventoryUI.displayInventory;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @InventoryItem

            private static void HookInventoryItem()
            {
                On.InventoryItem.IsConditionMet += new On.InventoryItem.hook_IsConditionMet(HookInventoryItemIsConditionMet);
                On.InventoryItem.OnHandGrab += new On.InventoryItem.hook_OnHandGrab(HookInventoryItemOnHandGrab);
                On.InventoryItem.OnInteractAttempt += new On.InventoryItem.hook_OnInteractAttempt(HookInventoryItemOnInteractAttempt);
                On.InventoryItem.ThrowItem += new On.InventoryItem.hook_ThrowItem(HookInventoryItemThrowItem);
            }

            private static IEnumerator ApplyPlayerCollision(InventoryItem inventoryItem, GameObject player)
            {
                for (int i = 0; i < inventoryItem.mainColliders.Count; i++)
                {
                    if (inventoryItem.mainColliders[i] != null)
                    {
                        Physics.IgnoreCollision(inventoryItem.mainColliders[i], player.GetComponent<CharacterController>());
                    }
                }
                yield return new WaitForSeconds(0.5f);
                for (int j = 0; j < inventoryItem.mainColliders.Count; j++)
                {
                    if (inventoryItem.mainColliders[j] != null && inventoryItem.mainColliders[j].enabled)
                    {
                        inventoryItem.mainColliders[j].enabled = false;
                        inventoryItem.mainColliders[j].enabled = true;
                    }
                }
                yield break;
            }

            private static bool HookInventoryItemIsConditionMet(On.InventoryItem.orig_IsConditionMet orig, InventoryItem inventoryItem)
            {
                Inventory inventory = PlayerInventory(lastPlayerCheckingInteractableConditions);
                bool condition = inventory.HasSpaceFor(inventoryItem);
                if (ModSettings.enableCrewVSMonsterMode && !ModSettings.giveMonsterAnInventory)
                {
                    int playerNumber = PlayerNumber(inventory.newPlayerClass.GetInstanceID());
                    if (ModSettings.numbersOfMonsterPlayers.Contains(playerNumber))
                    {
                        condition = false;
                    }
                }
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Checking InventoryItem condition. It is " + condition.ToString() + " for player number " + PlayerNumber(lastPlayerCheckingInteractableConditions.GetInstanceID()));
                }
                return condition;
            }

            private static void HookInventoryItemOnHandGrab(On.InventoryItem.orig_OnHandGrab orig, InventoryItem inventoryItem)
            {
                if (!inventoryItem.IsInInventory())
                {
                    //int playerNumber = ClosestPlayerToThis(inventoryItem.gameObject.transform.position);
                    InventoryItem.inventory = PlayerInventory(lastPlayerSentMessage);
                    if (InventoryItem.inventory.HasSpaceFor(inventoryItem))
                    {
                        inventoryItem.AddToInventory();
                        if (InventoryItem.inventory.ItemCount > 0)
                        {
                            AudioSystem.PlaySound("Noises/UI/Bag/ShortRustle");
                        }
                    }
                }
            }

            private static void HookInventoryItemOnInteractAttempt(On.InventoryItem.orig_OnInteractAttempt orig, InventoryItem inventoryItem)
            {
                TriggerObjectives.instance.SetToThrow();
                Inventory inventory = PlayerInventory(lastPlayerCheckingInteractableConditions);
                inventory.DisplayInventory();
                Debug.Log("OnInteractAttempt inventory is " + inventory.GetInstanceID());
                ((Action)Activator.CreateInstance(typeof(Action), inventoryItem, typeof(InteractableCondition).GetMethod("OnInteractAttempt").MethodHandle.GetFunctionPointer()))();
            }

            private static void HookInventoryItemThrowItem(On.InventoryItem.orig_ThrowItem orig, InventoryItem inventoryItem, float _throwPower)
            {
                ((MonoBehaviour)inventoryItem).gameObject.SetActive(true);
                ((MonoBehaviour)inventoryItem).BroadcastMessage("OnThrowItem", SendMessageOptions.DontRequireReceiver);
                inventoryItem.rb.isKinematic = false;
                SimpleOcclusion[] componentsInChildren = ((MonoBehaviour)inventoryItem).GetComponentsInChildren<SimpleOcclusion>();
                foreach (SimpleOcclusion simpleOcclusion in componentsInChildren)
                {
                    if (simpleOcclusion != null)
                    {
                        simpleOcclusion.IsPlayerHolding = false;
                    }
                }
                ColliderCheck component = ((MonoBehaviour)inventoryItem).GetComponent<ColliderCheck>();
                if (component != null)
                {
                    component.SwitchOnCCD();
                }
                NewPlayerClass newPlayerClass = InventoryItem.inventory.newPlayerClass;
                Camera playerCamera = PlayerCamera(newPlayerClass);
                Vector3 point = playerCamera.transform.forward;
                if (!OculusManager.isOculusEnabled)
                {
                    point = playerCamera.ViewportPointToRay(EyeTrack.FilteredViewPortPosition).direction;
                }
                float angle = Mathf.Lerp(-25f, 0f, Vector3.Angle(newPlayerClass.gameObject.transform.forward, playerCamera.transform.forward) / 90f);
                inventoryItem.rb.AddForce(Quaternion.AngleAxis(angle, playerCamera.transform.right) * point * _throwPower, ForceMode.Impulse);
                ((MonoBehaviour)inventoryItem).StartCoroutine(ApplyPlayerCollision(inventoryItem, newPlayerClass.gameObject));
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @InventoryUI

            private static void HookInventoryUI()
            {
                On.InventoryUI.AddInventorySlot += new On.InventoryUI.hook_AddInventorySlot(HookInventoryUIAddInventorySlot);
                On.InventoryUI.Start += new On.InventoryUI.hook_Start(HookInventoryUIStart);
                On.InventoryUI.UpdateTextures += new On.InventoryUI.hook_UpdateTextures(HookInventoryUIUpdateTextures);
            }

            private static void HookInventoryUIAddInventorySlot(On.InventoryUI.orig_AddInventorySlot orig, InventoryUI inventoryUI)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(inventoryUI.inventorySlotUI);
                gameObject.transform.SetParent(inventoryUI.slotsGO.transform);
                gameObject.ResetTransform();
                InventorySlotUI component = gameObject.GetComponent<InventorySlotUI>();
                component.slot = inventoryUI.inventory.inventorySlots[inventoryUI.slots.Count];
                Vector3 v = component.newSlot.GetComponent<RectTransform>().anchoredPosition;
                v.x = -808f + (float)inventoryUI.slots.Count * inventoryUI.slotSpacingX;
                int playerNumber = PlayerNumber(inventoryUI.inventory.newPlayerClass.GetInstanceID());
                Debug.Log("Player number at addinventoryslots is " + PlayerNumber(inventoryUI.inventory.newPlayerClass.GetInstanceID()));
                switch (ModSettings.NumberOfPlayers)
                {
                    case (2):
                        if (Display.displays.Length == 2 && ModSettings.useMultipleDisplaysIfPossible)
                        {
                            component.newSlot.canvas.targetDisplay = playerNumber;
                            v.y = 423f;
                        }
                        else
                        {
                            if (playerNumber == 1)
                            {
                                v.y = -123f;
                            }
                            else
                            {
                                v.y = 423f;
                            }
                        }
                        break;
                    case (3):
                        if (ModSettings.useMultipleDisplaysIfPossible)
                        {
                            switch (Display.displays.Length)
                            {
                                case (3):
                                    component.newSlot.canvas.targetDisplay = playerNumber;
                                    v.y = 423f;
                                    break;
                                case (2):
                                    if (playerNumber == 0)
                                    {
                                        v.y = 423f;
                                    }
                                    else
                                    {
                                        component.newSlot.canvas.targetDisplay = 1;
                                        if (playerNumber == 1)
                                        {
                                            v.y = 423f;
                                        }
                                        else
                                        {
                                            v.y = -123f;
                                        }
                                    }
                                    break;
                                default:
                                    switch (playerNumber)
                                    {
                                        case (0):
                                            v.y = 423f;
                                            break;
                                        case (1):
                                            v.y = 423f - 1080f / 3f;
                                            break;
                                        case (2):
                                            v.y = 423f - 2f * 1080f / 3f;
                                            break;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            switch (playerNumber)
                            {
                                case (0):
                                    v.y = 423f;
                                    break;
                                case (1):
                                    v.y = 423f - 1080f / 3f;
                                    break;
                                case (2):
                                    v.y = 423f - 2f * 1080f / 3f;
                                    break;
                            }
                        }
                        break;
                    case (4):
                        if (ModSettings.useMultipleDisplaysIfPossible)
                        {
                            switch (Display.displays.Length)
                            {
                                case (4):
                                    component.newSlot.canvas.targetDisplay = playerNumber;
                                    v.y = 423f;
                                    break;
                                case (3):
                                    if (playerNumber == 0 || playerNumber == 1)
                                    {
                                        component.newSlot.canvas.targetDisplay = playerNumber;
                                        v.y = 423f;
                                    }
                                    else
                                    {
                                        component.newSlot.canvas.targetDisplay = 2;
                                        if (playerNumber == 2)
                                        {
                                            v.y = 423f;
                                        }
                                        else
                                        {
                                            v.y = -123f;
                                        }
                                    }

                                    break;
                                case (2):
                                    if (playerNumber == 0 || playerNumber == 2)
                                    {
                                        v.y = 423f;
                                    }
                                    else
                                    {
                                        v.y = -123f;
                                    }

                                    if (playerNumber == 0 || playerNumber == 1)
                                    {
                                        component.newSlot.canvas.targetDisplay = 0;
                                    }
                                    else
                                    {
                                        component.newSlot.canvas.targetDisplay = 1;
                                    }

                                    break;
                                default:
                                    if (playerNumber == 0 || playerNumber == 1)
                                    {
                                        v.y = 423f;
                                    }
                                    else
                                    {
                                        v.y = -123f;
                                    }

                                    if (playerNumber == 1)
                                    {
                                        v.x += Screen.width / 2;
                                    }
                                    else if (playerNumber == 3)
                                    {
                                        v.x += Display.displays[1].renderingWidth / 2;
                                    }
                                    break;
                            }
                        }
                        else
                        {
                            if (playerNumber == 0 || playerNumber == 1)
                            {
                                v.y = 423f;
                            }
                            else
                            {
                                v.y = -123f;
                            }

                            if (playerNumber == 1)
                            {
                                v.x += Screen.width / 2;
                            }
                            else if (playerNumber == 3)
                            {
                                v.x += Display.displays[1].renderingWidth / 2;
                            }
                        }
                        break;
                }
                component.newSlot.GetComponent<RectTransform>().anchoredPosition = v;
                inventoryUI.slots.Add(component);
                GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(inventoryUI.oculusInventorySlotUI);
                gameObject2.GetComponentInChildren<RectTransform>().localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
                if (inventoryUI.oculusPlayerHUD != null)
                {
                    gameObject2.transform.parent = inventoryUI.oculusPlayerHUD.gameObject.transform;
                }
                gameObject2.ResetTransform();
                InventorySlotUI component2 = gameObject2.GetComponent<InventorySlotUI>();
                component2.slot = inventoryUI.inventory.inventorySlots[inventoryUI.oculusSlots.Count];
                Vector3 localPosition = gameObject2.transform.localPosition;
                localPosition.x = inventoryUI.oculusStartPos.x + (float)inventoryUI.oculusSlots.Count * 0.07f;
                localPosition.y = 0f;
                localPosition.z = 0.3f;
                gameObject2.transform.localPosition = localPosition;
                if (gameObject2.GetComponentInChildren<RectTransform>() != null)
                {
                }
                inventoryUI.oculusSlots.Add(component2);
            }

            private static void HookInventoryUIStart(On.InventoryUI.orig_Start orig, InventoryUI inventoryUI)
            {
                inventoryUI.inventory = (UnityEngine.Object.FindObjectOfType(typeof(Inventory)) as Inventory);
                inventoryUI.oculusPlayerHUD = (UnityEngine.Object.FindObjectOfType(typeof(OculusPlayerHUD)) as OculusPlayerHUD);
                inventoryUI.newPlayerClass = (UnityEngine.Object.FindObjectOfType(typeof(NewPlayerClass)) as NewPlayerClass);
                inventoryUI.DeployRenderers();
                inventoryUI.oculusStartPos = new Vector3(0f, 0f, 0.1f);
                inventoryUI.playerAnimator = inventoryUI.newPlayerClass.gameObject.GetComponent<Animator>();
                inventoryUI.playerAniLayers = inventoryUI.newPlayerClass.gameObject.GetComponent<PlayerAnimationLayersController>();
                inventoryUI.triggerNotification = (UnityEngine.Object.FindObjectOfType(typeof(TriggerNotification)) as TriggerNotification);
                Debug.Log("Inventory UI Instance ID is: " + inventoryUI.GetInstanceID() + " and npc ID is " + inventoryUI.newPlayerClass.GetInstanceID() + " and inventory ID is " + inventoryUI.inventory.GetInstanceID());
            }

            private static void HookInventoryUIUpdateTextures(On.InventoryUI.orig_UpdateTextures orig, InventoryUI inventoryUI)
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    orig.Invoke(inventoryUI);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ItemAnimationListener

            private static void HookItemAnimationListenerStart(On.ItemAnimationListener.orig_Start orig, ItemAnimationListener itemAnimationListener)
            {
                orig.Invoke(itemAnimationListener);

                /*
                Debug.Log("Item animation listener instance ID is " + itemAnimationListener.GetInstanceID());
                try
                {
                    Debug.Log("Player associated to item animation listener is " + PlayerNumber(itemAnimationListener.GetComponentInParent<NewPlayerClass>().GetInstanceID()));
                }
                catch (Exception e)
                {
                    Debug.Log("Could not get player from item animation listener.\n" + e.ToString());
                }
                try
                {
                    Debug.Log("Player associated to item animation listener is " + PlayerNumber(itemAnimationListener.GetComponent<NewPlayerClass>().GetInstanceID()));
                }
                catch (Exception e)
                {
                    Debug.Log("Could not get player from item animation listener.\n" + e.ToString());
                }
                */
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ItemAngleUpdater

            private static void HookItemAngleUpdaterUpdate(On.ItemAngleUpdater.orig_Update orig, ItemAngleUpdater itemAngleUpdater)
            {
                /*
                bool foundLink = false;
                if (itemAngleUpdater.gameObject.GetComponentInParent<NewPlayerClass>() != null)
                {
                    Debug.Log("Link found in parent of " + itemAngleUpdater.gameObject.name + " for type NewPlayerClass");
                    foundLink = true;
                }
                if (itemAngleUpdater.gameObject.GetComponent<NewPlayerClass>() != null)
                {
                    Debug.Log("Link found on same level of " + itemAngleUpdater.gameObject.name + " for type NewPlayerClass");
                    foundLink = true;
                }
                if (itemAngleUpdater.gameObject.GetComponentInChildren<NewPlayerClass>() != null)
                {
                    Debug.Log("Link found in children of " + itemAngleUpdater.gameObject.name + " for type NewPlayerClass");
                    foundLink = true;
                }
                InventoryItem inventoryItem = InventoryItemFromItemClass(itemAngleUpdater.gameObject);
                if (inventoryItem.IsInInventory() && InventoryFromItemClass(inventoryItem).newPlayerClass != null)
                {
                    Debug.Log("Item Angle Updater link found via item class");
                    foundLink = true;
                }
                if (!foundLink)
                {
                    //Debug.Log("No link found for " + itemAngleUpdater.gameObject.name + " for type NewPlayerClass");
                }
                */

                //orig.Invoke(itemAngleUpdater);

                if (itemAngleUpdater.equip)
                {
                    NewPlayerClass newPlayerClass = InventoryFromItemClass(itemAngleUpdater.gameObject).newPlayerClass;
                    //Debug.Log("Player being used in item angle updater is player number " + PlayerNumber(newPlayerClass.GetInstanceID()));
                    Camera playerCamera = PlayerCamera(newPlayerClass);
                    Vector3 a = Vector3.zero;
                    Transform playerCameraTransform = playerCamera.transform;
                    if (OculusManager.isOculusEnabled)
                    {
                        playerCameraTransform = ItemAngleUpdater.oculusCentreCam.transform;
                    }
                    Vector3 vector = playerCameraTransform.forward;
                    Vector2 a2 = Vector3.zero;
                    if (itemAngleUpdater.enableEyeTrackingSupport)
                    {
                        a2 = EyeTrack.FilteredViewPortPosition;
                        a2 -= Vector2.one * 0.5f;
                        a2 *= 2f;
                        a2.x *= playerCamera.FieldOfViewHorizontal() * itemAngleUpdater.eyetrackingScale.x;
                        a2.y *= playerCamera.fieldOfView * itemAngleUpdater.eyetrackingScale.y;
                    }
                    if (!OculusManager.isOculusEnabled)
                    {
                        vector = playerCamera.ViewportPointToRay(EyeTrack.FilteredViewPortPosition).direction;
                    }
                    RaycastHit raycastHit;
                    if (Physics.Raycast(playerCameraTransform.position, vector, out raycastHit, 1000f, itemAngleUpdater.mask))
                    {
                        a = raycastHit.point;
                    }
                    else
                    {
                        a = playerCameraTransform.position + vector * 10f;
                    }
                    itemAngleUpdater.distance = (a - playerCameraTransform.position).magnitude;
                    itemAngleUpdater.aim = (a - itemAngleUpdater.aimDirection.position).normalized;
                    float num = -Vector3.Angle(playerCameraTransform.forward, Vector3.up) + 90f - itemAngleUpdater.angleOffsetY;
                    num += itemAngleUpdater.Cal(num, itemAngleUpdater.yCalibration) + a2.y;
                    float maxDelta = 99999.9f;
                    if (EyeTrack.IsTracking)
                    {
                        maxDelta = Time.deltaTime * itemAngleUpdater.eyetrackLerpSpeed * Mathf.Abs(itemAngleUpdater.aimY - num);
                    }
                    itemAngleUpdater.aimY = Mathf.MoveTowards(itemAngleUpdater.aimY, num, maxDelta);
                    if (OculusManager.isOculusEnabled)
                    {
                        if (ItemAngleUpdater.oculusCentreCam != null)
                        {
                            Vector3 forward = ItemAngleUpdater.oculusCentreCam.transform.forward;
                            forward.y = 0f;
                            Vector3 forward2 = newPlayerClass.transform.forward;
                            forward2.y = 0f;
                            forward.Normalize();
                            forward2.Normalize();
                            float num2 = Vector3.Angle(forward, forward2);
                            if (Vector3.Cross(forward, forward2).normalized.y < 0f)
                            {
                                num2 = -num2;
                            }
                            itemAngleUpdater.aimX = Mathf.MoveTowards(itemAngleUpdater.aimX, itemAngleUpdater.closenessCalibration.Evaluate(itemAngleUpdater.distance) + itemAngleUpdater.Cal(itemAngleUpdater.aimY, itemAngleUpdater.xCalibration) + itemAngleUpdater.angleOffsetX + -num2 / 2f, Time.deltaTime * itemAngleUpdater.lerpSpeed);
                        }
                    }
                    else
                    {
                        float num3 = itemAngleUpdater.closenessCalibration.Evaluate(itemAngleUpdater.distance) + itemAngleUpdater.Cal(itemAngleUpdater.aimY, itemAngleUpdater.xCalibration) + itemAngleUpdater.angleOffsetX + a2.x;
                        float num4 = itemAngleUpdater.lerpSpeed;
                        if (EyeTrack.IsTracking)
                        {
                            num4 = itemAngleUpdater.eyetrackLerpSpeed * Mathf.Abs(itemAngleUpdater.aimX - num3);
                        }
                        itemAngleUpdater.aimX = Mathf.MoveTowards(itemAngleUpdater.aimX, num3, Time.deltaTime * num4);
                    }
                    itemAngleUpdater.aimX = Mathf.Clamp(itemAngleUpdater.aimX, itemAngleUpdater.min.x, itemAngleUpdater.max.x);
                    Animator playerAnimator = newPlayerClass.GetComponent<Animator>();
                    float num5 = (itemAngleUpdater.aimX <= 0f) ? (-itemAngleUpdater.min.x) : itemAngleUpdater.max.x;
                    float num6 = (itemAngleUpdater.aimY <= 0f) ? (-itemAngleUpdater.min.y) : itemAngleUpdater.max.y;
                    playerAnimator.SetFloat("AimX", (itemAngleUpdater.aimX != 0f) ? (itemAngleUpdater.aimX / num5) : 0f);
                    playerAnimator.SetFloat("AimY", (itemAngleUpdater.aimY != 0f) ? Mathf.Clamp(itemAngleUpdater.aimY / num6, itemAngleUpdater.min.y, itemAngleUpdater.max.y) : 0f);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ItemCondition

            private static bool HookItemConditionIsConditionMet(On.ItemCondition.orig_IsConditionMet orig, ItemCondition itemCondition)
            {
                Inventory inventoryToCheck = PlayerInventory(lastPlayerCheckingInteractableConditions);
                bool condition = inventoryToCheck.CurrentItem != null && inventoryToCheck.CurrentItem.itemName == itemCondition.inventoryItem.itemName;
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Checking item condition. It is " + condition.ToString() + " for player number " + PlayerNumber(lastPlayerCheckingInteractableConditions.GetInstanceID()));
                }
                return condition;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ItemFlashManager

            private static void HookItemFlashManagerUpdate(On.ItemFlashManager.orig_Update orig, ItemFlashManager itemFlashManager)
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    //itemFlashManager.glowLightChecks = Physics.OverlapSphere(References.Player.transform.position, 5f, itemFlashManager.glowLightCheckLayer);
                    itemFlashManager.glowLightChecks = Physics.OverlapSphere(lastPlayerCheckingInteractableConditions.transform.position, 5f, itemFlashManager.glowLightCheckLayer);
                    itemFlashManager.itemFlashes.Clear();
                    for (int i = 0; i < itemFlashManager.glowLightChecks.Length; i++)
                    {
                        itemFlashManager.itemFlash = itemFlashManager.glowLightChecks[i].transform.parent.GetComponent<ItemFlashScript>();
                        if (itemFlashManager.itemFlash && itemFlashManager.itemFlash.thisLight.enabled && itemFlashManager.itemFlash.lightMesh.enabled)
                        {
                            itemFlashManager.itemFlashes.Add(itemFlashManager.itemFlash);
                        }
                    }
                    itemFlashManager.shouldGlow = false;
                    itemFlashManager.j = 0;
                    for (int j = 0; j < DecalManager.manager.inventoryItems.Length; j++)
                    {
                        itemFlashManager.item = DecalManager.manager.inventoryItems[j];
                        if (itemFlashManager.item != null)
                        {
                            itemFlashManager.itemTrans = itemFlashManager.item.transform;
                            /*
                            if (Vector3.Distance(itemFlashManager.itemTrans.position, References.Player.transform.position) < 6f)
                            {
                            */
                            if (Vector3.Distance(itemFlashManager.itemTrans.position, lastPlayerCheckingInteractableConditions.transform.position) < 6f)
                            {
                                itemFlashManager.shouldGlow = false;
                                if (itemFlashManager.item.enabled)
                                {
                                    itemFlashManager.j = 0;
                                    while (itemFlashManager.j < itemFlashManager.itemFlashes.Count)
                                    {
                                        itemFlashManager.count++;
                                        itemFlashManager.itemFlash = itemFlashManager.itemFlashes[itemFlashManager.j];
                                        if (itemFlashManager.itemFlash.thisLight.enabled)
                                        {
                                            itemFlashManager.lightPos = itemFlashManager.itemFlash.transform.position;
                                            itemFlashManager.itemPos = itemFlashManager.itemTrans.position;
                                            if (Vector3.Distance(itemFlashManager.itemPos, itemFlashManager.lightPos) < itemFlashManager.itemFlash.thisLight.range)
                                            {
                                                if (itemFlashManager.itemFlash.isSpotLight)
                                                {
                                                    if (GeoHelper.InsideCone(itemFlashManager.lightPos, itemFlashManager.itemPos, itemFlashManager.itemFlash.transform.forward, itemFlashManager.itemFlash.thisLight.spotAngle / 2f, itemFlashManager.itemFlash.thisLight.range))
                                                    {
                                                        itemFlashManager.shouldGlow = true;
                                                    }
                                                }
                                                else
                                                {
                                                    itemFlashManager.shouldGlow = true;
                                                }
                                                if (itemFlashManager.shouldGlow)
                                                {
                                                    if (itemFlashManager.item.lastCheckFrame < Time.frameCount - 1)
                                                    {
                                                        itemFlashManager.item.ItemFlashEnter();
                                                        itemFlashManager.item.lastCheckFrame = Time.frameCount;
                                                        break;
                                                    }
                                                    itemFlashManager.item.lastCheckFrame = Time.frameCount;
                                                }
                                            }
                                        }
                                        itemFlashManager.j++;
                                    }
                                }
                                if (!itemFlashManager.shouldGlow && itemFlashManager.item.lastCheckFrame == Time.frameCount - 1)
                                {
                                    itemFlashManager.item.ItemFlashExit();
                                }
                            }
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ItemGrabIK

            private static void HookItemGrabIK()
            {
                On.ItemGrabIK.BeginIK += new On.ItemGrabIK.hook_BeginIK(HookItemGrabIKBeginIK);
                On.ItemGrabIK.MoveDummy += new On.ItemGrabIK.hook_MoveDummy(HookItemGrabIKMoveDummy);
                On.ItemGrabIK.OnAnimatorIK += new On.ItemGrabIK.hook_OnAnimatorIK(HookItemGrabIKOnAnimatorIK);
                On.ItemGrabIK.Start += new On.ItemGrabIK.hook_Start(HookItemGrabIKStart);
                On.ItemGrabIK.Release += new On.ItemGrabIK.hook_Release(HookItemGrabIKRelease);
            }

            private static void HookItemGrabIKBeginIK(On.ItemGrabIK.orig_BeginIK orig, ItemGrabIK itemGrabIK, HandGrabIK _item)
            {
                if (!itemGrabIK.grabbing && itemGrabIK.t < 0.2f)
                {
                    itemGrabIK.grabbing = true;
                    itemGrabIK.ikActive = true;
                    itemGrabIK.ikTarget = _item;
                    itemGrabIK.ikTransform = FindClosestIK(itemGrabIK.ikTarget, itemGrabIK.motor.player);
                    itemGrabIK.grabbed = false;
                    itemGrabIK.ikPositionCache = itemGrabIK.ikTransform.position;
                    itemGrabIK.rotationTypeCache = itemGrabIK.ikTarget.rotationType;
                    itemGrabIK.ikTarget.ShouldRelease = false;
                    itemGrabIK.holdTime = 0f;
                    Vector3 position = ((MonoBehaviour)itemGrabIK).transform.position;
                    Vector3 b = itemGrabIK.ikPositionCache;
                    position.y = (b.y = 0f);
                    itemGrabIK.tScaler = Mathf.Clamp01(Vector3.Distance(position, b) / 0.65f);
                }
            }

            private static void HookItemGrabIKMoveDummy(On.ItemGrabIK.orig_MoveDummy orig, ItemGrabIK itemGrabIK)
            {
                itemGrabIK.dummyTransform.position = Vector3.MoveTowards(PlayerCamera(itemGrabIK.motor.player).transform.position, itemGrabIK.ikTransform.position, 1f);
                itemGrabIK.dummyTransform.rotation = itemGrabIK.ikTransform.rotation;
            }

            private static void HookItemGrabIKOnAnimatorIK(On.ItemGrabIK.orig_OnAnimatorIK orig, ItemGrabIK itemGrabIK)
            {
                if (itemGrabIK.animator && itemGrabIK.ikActive)
                {
                    if (itemGrabIK.grabbing)
                    {
                        itemGrabIK.t += Time.deltaTime * itemGrabIK.grabSpeed;
                        if (itemGrabIK.t > 1f)
                        {
                            itemGrabIK.t = 1f;
                        }
                        if (itemGrabIK.playerAnimator.GetLayerWeight(itemGrabIK.playerAniLayers.Oculus) == 0f)
                        {
                            itemGrabIK.animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, itemGrabIK.t * itemGrabIK.tScaler);
                            itemGrabIK.animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, itemGrabIK.t);
                            itemGrabIK.overide = false;
                        }
                        else
                        {
                            itemGrabIK.overide = true;
                        }
                        itemGrabIK.MoveDummy();
                        if (itemGrabIK.t == 1f || itemGrabIK.overide)
                        {
                            itemGrabIK.holdTime += Time.deltaTime;
                            bool flag = false;
                            bool flag2 = true;
                            switch (itemGrabIK.ikTarget.releaseType)
                            {
                                case HandGrabIK.ReleaseType.AutoRelease:
                                    flag = true;
                                    break;
                                case HandGrabIK.ReleaseType.HoldStand:
                                    if ((!itemGrabIK.ikTarget.IsInteracting && (itemGrabIK.holdTime == float.PositiveInfinity || itemGrabIK.holdTime > itemGrabIK.ikTarget.holdDelay)) || (!itemGrabIK.IsStillCloseEnough && itemGrabIK.motor.Moving))
                                    {
                                        flag = true;
                                    }
                                    break;
                                case HandGrabIK.ReleaseType.Hold:
                                    if ((!itemGrabIK.ikTarget.IsInteracting && (itemGrabIK.holdTime == float.PositiveInfinity || itemGrabIK.holdTime > itemGrabIK.ikTarget.holdDelay)) || !itemGrabIK.IsStillCloseEnough)
                                    {
                                        flag = true;
                                    }
                                    break;
                            }
                            if (itemGrabIK.grabbed)
                            {
                                flag2 = false;
                            }
                            if (itemGrabIK.ikTarget.ShouldRelease)
                            {
                                flag = true;
                            }
                            if (flag2)
                            {
                                lastPlayerSentMessage = itemGrabIK.motor.player;
                                if (ModSettings.logDebugText)
                                {
                                    Debug.Log("Updating lastPlayerSentMessage: " + PlayerNumber(lastPlayerSentMessage.GetInstanceID()) + "\n" + new StackTrace().ToString() + "\n-----");
                                }
                                itemGrabIK.Grab();
                            }
                            if (flag)
                            {
                                itemGrabIK.Release();
                            }
                        }
                    }
                    else
                    {
                        itemGrabIK.t -= Time.deltaTime * itemGrabIK.grabSpeed;
                        if (itemGrabIK.t < 0f)
                        {
                            itemGrabIK.t = 0f;
                        }
                        itemGrabIK.animator.SetIKPositionWeight(AvatarIKGoal.LeftHand, itemGrabIK.t * itemGrabIK.tScaler);
                        itemGrabIK.animator.SetIKRotationWeight(AvatarIKGoal.LeftHand, itemGrabIK.t);
                        if (itemGrabIK.t == 0f)
                        {
                            itemGrabIK.ikActive = false;
                        }
                    }
                    if (itemGrabIK.ikTransform != null)
                    {
                        itemGrabIK.animator.SetIKPosition(AvatarIKGoal.LeftHand, itemGrabIK.dummyTransform.position);
                        HandGrabIK.RotationFollowType rotationFollowType = itemGrabIK.rotationTypeCache;
                        if (rotationFollowType != HandGrabIK.RotationFollowType.PlayerForwardIKUp)
                        {
                            if (rotationFollowType != HandGrabIK.RotationFollowType.PlayerRotation)
                            {
                                if (rotationFollowType == HandGrabIK.RotationFollowType.IKRotation)
                                {
                                    itemGrabIK.animator.SetIKRotation(AvatarIKGoal.LeftHand, itemGrabIK.dummyTransform.rotation);
                                }
                            }
                            else
                            {
                                itemGrabIK.animator.SetIKRotation(AvatarIKGoal.LeftHand, Quaternion.AngleAxis(itemGrabIK.motor.player.gameObject.transform.rotation.eulerAngles.y, Vector3.up));
                            }
                        }
                        else
                        {
                            Quaternion goalRotation = Quaternion.AngleAxis(itemGrabIK.motor.player.gameObject.transform.rotation.eulerAngles.y, Vector3.up) * Quaternion.AngleAxis(-Vector3.Angle(Vector3.up, itemGrabIK.dummyTransform.up), Vector3.right);
                            itemGrabIK.animator.SetIKRotation(AvatarIKGoal.LeftHand, goalRotation);
                        }
                    }
                }
            }

            private static void HookItemGrabIKStart(On.ItemGrabIK.orig_Start orig, ItemGrabIK itemGrabIK)
            {
                itemGrabIK.animator = ((MonoBehaviour)itemGrabIK).GetComponent<Animator>();
                itemGrabIK.dummyTransform = new GameObject("IKDummy").transform;
                itemGrabIK.dummyTransform.position = Vector3.zero;
                itemGrabIK.dummyTransform.eulerAngles = Vector3.zero;
                itemGrabIK.dummyTransform.parent = ((MonoBehaviour)itemGrabIK).transform;
                itemGrabIK.motor = ((MonoBehaviour)itemGrabIK).GetComponent<PlayerMotor>();
                NewPlayerClass newPlayerClass = ((MonoBehaviour)itemGrabIK).GetComponent<NewPlayerClass>();//itemGrabIK.motor.player;//((MonoBehaviour)itemGrabIK).GetComponentInParent<NewPlayerClass>();

                if (newPlayerClass == null)
                {
                    Debug.Log("Tried alternative player assignment method in ItemGrabIKStart");
                    newPlayerClass = itemGrabIK.motor.GetComponent<NewPlayerClass>();
                }
                if (newPlayerClass != null)
                {
                    if (itemGrabIK.motor.player == null)
                    {
                        itemGrabIK.motor.player = newPlayerClass;
                    }

                    itemGrabIK.playerAnimator = newPlayerClass.gameObject.GetComponent<Animator>();
                    itemGrabIK.playerAniLayers = newPlayerClass.gameObject.GetComponent<PlayerAnimationLayersController>();
                    Debug.Log("ItemGrabIKStart NPC ID is: " + newPlayerClass.GetInstanceID() + "\nItemGrabIKStart Motor NPC ID is: " + itemGrabIK.motor.player.GetInstanceID());
                }
                else
                {
                    Debug.Log("Player is null in ItemGrabIKStart");
                }

                if (LevelGeneration.Instance.finishedGenerating)
                {
                    Debug.Log("ItemGrabIKStart NPC player number is: " + PlayerNumber(newPlayerClass.GetInstanceID()) + "\nItemGrabIKStart Motor NPC player number is: " + PlayerNumber(itemGrabIK.motor.player.GetInstanceID()));
                }

                /*
                itemGrabIK.motor = ((MonoBehaviour)itemGrabIK).GetComponent<PlayerMotor>();
                NewPlayerClass newPlayerClass = itemGrabIK.motor.player;//((MonoBehaviour)itemGrabIK).GetComponentInParent<NewPlayerClass>();
                itemGrabIK.animator = ((MonoBehaviour)itemGrabIK).GetComponent<Animator>();
                itemGrabIK.dummyTransform = new GameObject("IKDummy").transform;
                itemGrabIK.dummyTransform.position = Vector3.zero;
                itemGrabIK.dummyTransform.eulerAngles = Vector3.zero;
                itemGrabIK.dummyTransform.parent = ((MonoBehaviour)itemGrabIK).transform;
                //itemGrabIK.motor = //newPlayerClass.Motor;//.gameObject.GetComponent<PlayerMotor>();
                itemGrabIK.playerAnimator = newPlayerClass.gameObject.GetComponent<Animator>();
                itemGrabIK.playerAniLayers = newPlayerClass.gameObject.GetComponent<PlayerAnimationLayersController>();
                Debug.Log("ItemGrabIKStart NPC ID is: " + newPlayerClass.GetInstanceID() + "\nItemGrabIKStart Motor NPC ID is: " + itemGrabIK.motor.player.GetInstanceID());
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    Debug.Log("ItemGrabIKStart NPC player number is: " + PlayerNumber(newPlayerClass.GetInstanceID()) + "\nItemGrabIKStart Motor NPC player number is: " + PlayerNumber(itemGrabIK.motor.player.GetInstanceID()));
                }
                */
            }


            private static void HookItemGrabIKRelease(On.ItemGrabIK.orig_Release orig, ItemGrabIK itemGrabIK)
            {
                lastPlayerSentMessage = itemGrabIK.motor.player;
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Updating lastPlayerSentMessage: " + PlayerNumber(lastPlayerSentMessage.GetInstanceID()) + "\n" + new StackTrace().ToString() + "\n-----");
                }
                orig.Invoke(itemGrabIK);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ItemModelTransform

            private static void HookItemModelTransformSetInventoryTransform(On.ItemModelTransform.orig_SetInventoryTransform orig, ItemModelTransform itemModelTransform)
            {
                NewPlayerClass.PlayerState playerState = InventoryFromItemClass(((MonoBehaviour)itemModelTransform).GetComponentInParent<InventoryItem>()).newPlayerClass.playerState;
                if (playerState != NewPlayerClass.PlayerState.Standing)
                {
                    if (playerState != NewPlayerClass.PlayerState.Crouched)
                    {
                        if (playerState == NewPlayerClass.PlayerState.Prone)
                        {
                            ((MonoBehaviour)itemModelTransform).transform.localPosition = itemModelTransform.inventoryProneTrans.localPosition;
                            ((MonoBehaviour)itemModelTransform).transform.localRotation = itemModelTransform.inventoryProneTrans.localRotation;
                        }
                    }
                    else
                    {
                        ((MonoBehaviour)itemModelTransform).transform.localPosition = itemModelTransform.inventoryCrouchTrans.localPosition;
                        ((MonoBehaviour)itemModelTransform).transform.localRotation = itemModelTransform.inventoryCrouchTrans.localRotation;
                    }
                }
                else
                {
                    ((MonoBehaviour)itemModelTransform).transform.localPosition = itemModelTransform.inventoryStandTrans.localPosition;

                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ItemPosition

            private static void HookItemPosition()
            {
                On.ItemPosition.OnEnable += new On.ItemPosition.hook_OnEnable(HookItemPositionOnEnable);
                On.ItemPosition.Update += new On.ItemPosition.hook_Update(HookItemPositionUpdate);
                On.ItemPosition.DisableFollow += new On.ItemPosition.hook_DisableFollow(HookItemPositionDisableFollow);
            }

            private static void HookItemPositionOnEnable(On.ItemPosition.orig_OnEnable orig, ItemPosition itemPosition)
            {
                try
                {
                    itemPosition.npc = ((MonoBehaviour)itemPosition).GetComponentInParent<NewPlayerClass>();
                    Debug.Log("ItemPosition npc ID is: " + itemPosition.npc.GetInstanceID());
                }
                catch
                {
                    Debug.Log("ItemPositionOnEnable did not work. Running original.");
                    orig.Invoke(itemPosition);
                }
            }

            private static void HookItemPositionUpdate(On.ItemPosition.orig_Update orig, ItemPosition itemPosition)
            {
                Debug.Log("Running item position update");
                orig.Invoke(itemPosition);
                try
                {
                    if (LevelGeneration.Instance.finishedGenerating)
                    {
                        Debug.Log("ItemPosition npc ID is: " + itemPosition.npc.GetInstanceID());
                        Debug.Log("Retrieved npc id is  " + ((MonoBehaviour)itemPosition).GetComponentInParent<NewPlayerClass>().GetInstanceID());
                    }
                }
                catch
                {
                    Debug.Log("ItemPositionUpdate did not work.");
                }
            }


            private static void HookItemPositionDisableFollow(On.ItemPosition.orig_DisableFollow orig, ItemPosition itemPosition)
            {
                try
                {
                    itemPosition.mode = ItemPosition.Mode.Fixed;
                    ((MonoBehaviour)itemPosition).transform.parent = ((MonoBehaviour)itemPosition).GetComponentInParent<NewPlayerClass>().transform;
                }
                catch
                {
                    Debug.Log("ItemPositionDisableFollow did not work. Running original.");
                    orig.Invoke(itemPosition);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Liferaft

            public static bool HookLiferaftInflateIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!ManyMonstersMode.IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookLiferaftInflate((Liferaft)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    ManyMonstersMode.IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookLiferaftInflate(Liferaft liferaft)
            {
                NewPlayerClass newPlayerClass = lastPlayerSentMessage;
                liferaft.AdvanceState();
                float inflate = 0f;
                for (; ; )
                {
                    if (!liferaft.inflationPaused)
                    {
                        inflate += Time.deltaTime * liferaft.inflationSpeed;
                        ((MonoBehaviour)liferaft).gameObject.GetComponentInChildren<Renderer>().material.SetFloat("_Blend", inflate / 100f);
                        if (inflate > 100f)
                        {
                            inflate = 100f;
                        }
                        liferaft.raftModel.SetBlendShapeWeight(0, 100f - inflate);
                        if (inflate >= 100f)
                        {
                            break;
                        }
                    }
                    yield return null;
                }
                liferaft.colliders.SetActive(true);
                liferaft.interactPump.DisableInteraction();
                liferaft.interactPump.GetComponent<Collider>().enabled = false;
                liferaft.inflateComplete.Complete();
                liferaft.AdvanceState();
                newPlayerClass.BroadcastMessage("OnLoopExit");
                newPlayerClass.EndFixedAnimation();
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @LightCull2

            private static float HookLightCull2CalculateEffectiveness(On.LightCull2.orig_CalculateEffectiveness orig, LightCull2 lightCull2)
            {
                LightCulling2.camPos = PlayerCamera(newPlayerClasses[ClosestPlayerToThis(lightCull2.position)]).transform.position;
                return orig.Invoke(lightCull2);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @LightCulling

            private static void HookLightCullingProcessLights(On.LightCulling.orig_ProcessLights orig, LightCulling lightCulling)
            {
                int count = LightCulling.lightsList.Count;
                int layerMask = 1 << LayerMask.NameToLayer("LightCull");
                int frameCount = Time.frameCount;
                for (int playerNumber = 0; playerNumber < newPlayerClasses.Count; playerNumber++)
                {
                    Collider[] array = Physics.OverlapSphere(PlayerCamera(newPlayerClasses[playerNumber]).transform.position, 1f, layerMask);
                    for (int i = 0; i < array.Length; i++)
                    {
                        LightCull component = array[i].transform.parent.GetComponent<LightCull>();
                        component.lastFrameWhenClose = frameCount;
                    }
                }
                for (int j = 0; j < count; j++)
                {
                    LightCull lightCull = LightCulling.lightsList[j];
                    if (!(lightCull == null))
                    {
                        lightCull.cullPos = lightCull.trans.position;
                        if (lightCull.theLight.enabled)
                        {
                            lightCull.wasEnabled = true;
                            lightCull.tempIntensity = lightCull.theLight.intensity;
                        }
                        else
                        {
                            lightCull.wasEnabled = false;
                        }
                        lightCull.isVisible = false;
                        lightCull.cullReason = LightCull.LightCullReason.C_NotSeen;
                        lightCull.preCulled = false;
                        lightCull.theLight.shadows = LightShadows.None;
                        lightCull.theLight.intensity *= lightCulling.reducedLightStrength;
                        if (!lightCull.wasEnabled)
                        {
                            lightCull.cullReason = LightCull.LightCullReason.C_NotEnabled;
                        }
                        else if (!lightCull.go.activeInHierarchy)
                        {
                            lightCull.cullReason = LightCull.LightCullReason.C_GONotActive;
                        }
                        else if (lightCull.GetComponent<Light>().intensity == 0f)
                        {
                            lightCull.cullReason = LightCull.LightCullReason.C_ZeroIntensity;
                        }
                        if (lightCull.cullReason != LightCull.LightCullReason.C_NotSeen)
                        {
                            lightCull.preCulled = true;
                        }
                        else
                        {
                            if (lightCull.enabledLastFrame && lightCulling.keepFromLastFrame)
                            {
                                if (Mathf.Abs(lightCulling.nextTotal - lightCulling.nextTestBack) != 0)
                                {
                                    lightCull.cullReason = LightCull.LightCullReason.R_LastFrame;
                                }
                                else
                                {
                                    lightCull.cullReason = LightCull.LightCullReason.P_TestingVisibility;
                                }
                                lightCulling.nextTotal++;
                            }
                            if (!lightCull.enabledLastFrame || lightCull.cullReason == LightCull.LightCullReason.P_TestingVisibility)
                            {
                                if (lightCulling.frustumTesting && !lightCulling.IsVisibleFrom(lightCull))
                                {
                                    lightCull.cullReason = LightCull.LightCullReason.C_OutsideFrustum;
                                }
                                if (lightCull.lastFrameWhenClose == frameCount)
                                {
                                    lightCull.cullReason = LightCull.LightCullReason.R_Close;
                                }
                            }
                        }
                        if (lightCull.preCulled)
                        {
                            lightCull.renderSphere.enabled = false;
                        }
                        else
                        {
                            switch (lightCull.cullReason)
                            {
                                case LightCull.LightCullReason.R_Close:
                                    lightCull.renderSphere.enabled = false;
                                    lightCull.isVisible = true;
                                    lightCulling.visibleLights.Add(lightCull);
                                    goto IL_2C9;
                                case LightCull.LightCullReason.P_TestingVisibility:
                                    lightCull.renderSphere.enabled = true;
                                    goto IL_2C9;
                                case LightCull.LightCullReason.R_LastFrame:
                                    lightCull.renderSphere.enabled = false;
                                    lightCull.isVisible = true;
                                    lightCulling.visibleLights.Add(lightCull);
                                    goto IL_2C9;
                            }
                            lightCull.renderSphere.enabled = true;
                            lightCull.isVisible = false;
                        }
                    }
                IL_2C9:;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @LightFlicker

            private static void HookLightFlickerFlicker(On.LightFlicker.orig_Flicker orig, LightFlicker lightFlicker)
            {
                if (lightFlicker.time > lightFlicker.randomTime)
                {
                    lightFlicker.intenseValue = UnityEngine.Random.Range(lightFlicker.minIntenisty, lightFlicker.maxIntensity);
                    if (lightFlicker.currentFreq < lightFlicker.randomFreq)
                    {
                        if (lightFlicker.intenseValue > 0.4f)
                        {
                            ((MonoBehaviour)lightFlicker).GetComponent<Light>().intensity = lightFlicker.intenseValue;
                            AudioSystem.PlaySound("Noises/Atmosphere/Buzzes", lightFlicker.source);
                            lightFlicker.currentFreq++;
                        }
                        else
                        {
                            ((MonoBehaviour)lightFlicker).GetComponent<Light>().intensity = 0f;
                            lightFlicker.source.Stop();
                            VirtualAudioSource virtualAudioSource = lightFlicker.source.gameObject.GetComponent<VirtualAudioSource>();
                            if (virtualAudioSource != null)
                            {
                                virtualAudioSource.Stop();
                            }
                            else if (ModSettings.logDebugText)
                            {
                                Debug.Log("VAS is null 1!\n" + new StackTrace().ToString());
                            }
                        }
                    }
                    else
                    {
                        lightFlicker.currentFreq = 0;
                        lightFlicker.time = 0f;
                        lightFlicker.randomTime = UnityEngine.Random.Range(0.5f, lightFlicker.maxTime);
                        lightFlicker.randomFreq = UnityEngine.Random.Range(1, lightFlicker.maxFreq);
                    }
                }
                else
                {
                    ((MonoBehaviour)lightFlicker).GetComponent<Light>().intensity = lightFlicker.intenseValue - 0.1f;
                    lightFlicker.source.Stop();
                    VirtualAudioSource virtualAudioSource = lightFlicker.source.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null 2!\n" + new StackTrace().ToString());
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @LockerReverb

            public static void LockerReverbUpdate(LockerReverb lockerReverb)
            {
                LockerReverb.reverbController = LockerReverb.monster.player.GetComponentsInChildren<ReverbController>();
                LockerReverb.npc = LockerReverb.monster.player.GetComponent<NewPlayerClass>();

                lockerReverb.currentRoom = LockerReverb.monster.PlayerDetectRoom.GetRoom;
                if (lockerReverb.currentRoom != null)
                {
                    if (lockerReverb.CheckIfHidingHere())
                    {
                        if (lockerReverb.lockerReverbController != null && lockerReverb.lockerDoor != null)
                        {
                            if (!lockerReverb.lockerDoor.isOpen)
                            {
                                foreach (ReverbController reverbController in LockerReverb.reverbController)
                                {
                                    reverbController.multiplyer = 0.25f;
                                }
                                lockerReverb.lockerReverbController.reverbAmount = 1f;
                                lockerReverb.lockerReverbController.multiplyer = 1f;
                            }
                            else if (lockerReverb.lockerReverbController.reverbAmount != 0f)
                            {
                                foreach (ReverbController reverbController2 in LockerReverb.reverbController)
                                {
                                    reverbController2.multiplyer = 1f;
                                }
                                lockerReverb.lockerReverbController.reverbAmount = 0f;
                            }
                        }
                        else
                        {
                            LockerReverb.reverbController = LockerReverb.monster.player.GetComponentsInChildren<ReverbController>();
                            foreach (ReverbController reverbController3 in LockerReverb.reverbController)
                            {
                                if (reverbController3.GetComponent<LockerReverbFinder>() != null)
                                {
                                    lockerReverb.lockerReverbController = reverbController3;
                                }
                            }
                            lockerReverb.lockerDoor = ((MonoBehaviour)lockerReverb).transform.parent.GetComponent<Door>();
                        }
                    }
                    else if (lockerReverb.lockerReverbController != null && lockerReverb.lockerReverbController.reverbAmount != 0f)
                    {
                        foreach (ReverbController reverbController4 in LockerReverb.reverbController)
                        {
                            reverbController4.multiplyer = 1f;
                        }
                        lockerReverb.lockerReverbController.reverbAmount = 0f;
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @LockToPosition

            private static void HookLockToPositionBegin(On.LockToPosition.orig_Begin orig, LockToPosition lockToPosition)
            {
                if (!lockToPosition.locked)
                {
                    lockToPosition.locked = true;
                    ((MonoBehaviour)lockToPosition).StartCoroutine(Locking(lockToPosition, lastPlayerSentMessage.gameObject));
                }
            }

            private static IEnumerator Locking(LockToPosition lockToPosition, GameObject playerGameObject)
            {
                while (lockToPosition.locked)
                {
                    if (lockToPosition.target != null)
                    {
                        playerGameObject.transform.position = lockToPosition.target.position;
                        Vector3 forward = Quaternion.AngleAxis(90f, lockToPosition.target.right) * Vector3.up;
                        playerGameObject.transform.rotation = Quaternion.LookRotation(forward, Vector3.up);
                    }
                    yield return null;
                }
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MasterControlValve

            private static void HookMasterControlValveOnInteract(On.MasterControlValve.orig_OnInteract orig, MasterControlValve masterControlValve)
            {
                if (!masterControlValve.valveInUse)
                {
                    masterControlValve.valveInUse = true;
                    masterControlValve.valveGO.layer = LayerMask.NameToLayer("Player");
                    masterControlValve.svm.MasterControlOverride = !masterControlValve.svm.MasterControlOverride;
                    if (!masterControlValve.svm.MasterControlOverride)
                    {
                        foreach (NewPlayerClass newPlayerClass in newPlayerClasses)
                        {
                            newPlayerClass.ValveValue = -1f;
                        }
                    }
                    else
                    {
                        foreach (NewPlayerClass newPlayerClass in newPlayerClasses)
                        {
                            newPlayerClass.ValveValue = 1f;
                        }
                    }
                }
            }

            public static bool HookMasterControlValveTurnTheValveIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!ManyMonstersMode.IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookMasterControlValveTurnTheValve((MasterControlValve)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    ManyMonstersMode.IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookMasterControlValveTurnTheValve(MasterControlValve masterControlValve)
            {
                masterControlValve.currentRotation = masterControlValve.valveGO.transform.localRotation.eulerAngles;
                masterControlValve.progress = 0f;
                if (!masterControlValve.svm.MasterControlOverride)
                {
                    while (masterControlValve.progress < 1f)
                    {
                        masterControlValve.progress += Time.deltaTime * masterControlValve.rotationSpeed;
                        masterControlValve.currentRotation = Vector3.Lerp(masterControlValve.startRotation, masterControlValve.goalRotation, masterControlValve.progress);
                        masterControlValve.valveGO.transform.localRotation = Quaternion.Euler(masterControlValve.currentRotation);
                        masterControlValve.StateSignGO.transform.localRotation = Quaternion.Euler(masterControlValve.currentRotation);
                        yield return null;
                    }
                }
                else
                {
                    while (masterControlValve.progress < 1f)
                    {
                        masterControlValve.progress += Time.deltaTime * masterControlValve.rotationSpeed;
                        masterControlValve.currentRotation = Vector3.Lerp(masterControlValve.goalRotation, masterControlValve.startRotation, masterControlValve.progress);
                        masterControlValve.valveGO.transform.localRotation = Quaternion.Euler(masterControlValve.currentRotation);
                        masterControlValve.StateSignGO.transform.localRotation = Quaternion.Euler(masterControlValve.currentRotation);
                        yield return null;
                    }
                }
                if (!masterControlValve.svm.MasterControlOverride)
                {
                    masterControlValve.onLoopSource.Stop();
                    VirtualAudioSource virtualAudioSource = masterControlValve.onLoopSource.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                    Achievements.Instance.CompleteAchievement("MAIN_STEAM_VALVE");
                }
                else
                {
                    masterControlValve.onLoopSource.Play();
                    VirtualAudioSource virtualAudioSource = masterControlValve.onLoopSource.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Play();
                    }
                    else
                    {
                        virtualAudioSource = AddVirtualAudioSourceToAudioSource(ref masterControlValve.onLoopSource);
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                        //virtualAudioSource = masterControlValve.onLoopSource.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource != null)
                        {
                            virtualAudioSource.Play();
                        }
                        else if (ModSettings.logDebugText)
                        {
                            Debug.Log("VAS is still null!\n" + new StackTrace().ToString());
                        }
                    }
                }
                float t = 0f;
                while (t < 0.7f)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
                //yield return new WaitForSeconds(0.7f); // Can't use this because other methods cannot be returned by an IEnumerator method that uses an intermediate hook.
                masterControlValve.valveInUse = false;
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MAttackingState2

            private static void HookMAttackingState2LerpToPosition(On.MAttackingState2.orig_LerpToPosition orig, MAttackingState2 mAttackingState2)
            {
                Vector3 position = ((MState)mAttackingState2).monster.player.transform.position;
                mAttackingState2.dying = true;
                if (ChooseAttack.GetSideChosen == ChooseAttack.PlayerSide.Hiding && ChooseAttack.GetHideSpot != null)
                {
                    mAttackingState2.CheckForKillPosition();
                }
                else
                {
                    mAttackingState2.monsterTargetPos = ((MState)mAttackingState2).monster.transform.position;
                }
                if (!((MState)mAttackingState2).monster.IsPlayerHiding)
                {
                    Vector3 b = mAttackingState2.CalcDistVector(position - mAttackingState2.monsterTargetPos);
                    mAttackingState2.monsterTargetPos = position + b;
                }
                ((MState)mAttackingState2).StartCoroutine(mAttackingState2.LerpToThis(mAttackingState2.monsterTargetPos, ((MState)mAttackingState2).monster.player.transform.position, MAttackingState2.Characters.Monster));
                if (ChooseAttack.GetSideChosen == ChooseAttack.PlayerSide.Front)
                {
                    ((MState)mAttackingState2).StartCoroutine(mAttackingState2.LerpToThis(((MState)mAttackingState2).monster.player.transform.position, mAttackingState2.monsterTargetPos, MAttackingState2.Characters.Player));
                }
                else if (ChooseAttack.GetSideChosen == ChooseAttack.PlayerSide.Back)
                {
                    ((MState)mAttackingState2).StartCoroutine(mAttackingState2.LerpToThis(position, mAttackingState2.monsterTargetPos + (position - mAttackingState2.monsterTargetPos) * 1.5f, MAttackingState2.Characters.Player));
                }
                else if (!((MState)mAttackingState2).monster.player.GetComponent<NewPlayerClass>().IsProne())
                {
                    ((MState)mAttackingState2).StartCoroutine(mAttackingState2.LerpToThis(((MState)mAttackingState2).monster.player.transform.position, mAttackingState2.monsterTargetPos, MAttackingState2.Characters.Player));
                }
                else
                {
                    mAttackingState2.playerInPosition = true;
                }
            }

            private static void HookMAttackingState2SetLayers(On.MAttackingState2.orig_SetLayers orig, MAttackingState2 mAttackingState2)
            {
                AnimationLayerController getAniLayerController = ((MState)mAttackingState2).monster.GetAniLayerController;
                int attackLayer = getAniLayerController.AttackLayer;
                getAniLayerController.MakeOnlyLayerActive(attackLayer);
                mAttackingState2.playerClass = ((MState)mAttackingState2).monster.player.GetComponent<NewPlayerClass>();
                PlayerAnimationLayersController component = mAttackingState2.playerClass.GetComponent<PlayerAnimationLayersController>();
                int death = component.Death;
                component.MakeOnlyLayerActive(death);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MFiendSubDoors

            private static void HookMFiendSubDoorsDoorWasBlocked(On.MFiendSubDoors.orig_DoorWasBlocked orig, MFiendSubDoors mFiendSubDoors)
            {
                if (mFiendSubDoors.ChosenDoor != null && mFiendSubDoors.ChosenDoor.attached && !((MState)mFiendSubDoors).monster.TheSubAlarm.RoomBreached)
                {
                    mFiendSubDoors.shouldChooseNewDoor = true;
                    ((MState)mFiendSubDoors).monster.IsNearSubDoors = false;
                    if (mFiendSubDoors.fiendDoorSource != null)
                    {
                        mFiendSubDoors.fiendDoorSource.Stop();
                        VirtualAudioSource virtualAudioSource = mFiendSubDoors.fiendDoorSource.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource != null)
                        {
                            virtualAudioSource.Stop();
                        }
                        else if (ModSettings.logDebugText)
                        {
                            Debug.Log("VAS is null 1!\n" + new StackTrace().ToString());
                        }
                    }
                    if (mFiendSubDoors.doorPullSource != null)
                    {
                        mFiendSubDoors.doorPullSource.Stop();
                        VirtualAudioSource virtualAudioSource = mFiendSubDoors.doorPullSource.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource != null)
                        {
                            virtualAudioSource.Stop();
                        }
                        else if (ModSettings.logDebugText)
                        {
                            Debug.Log("VAS is null 2!\n" + new StackTrace().ToString());
                        }
                        AudioSystem.PlaySound("Noises/Enviro/Monster Metal Hits", mFiendSubDoors.doorPullSource);
                    }
                    mFiendSubDoors.ChosenDoor.SubDoorLock();
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MHuntingState

            private static void HookMHuntingStateUpdateAudioZones(On.MHuntingState.orig_UpdateAudioZones orig, MHuntingState mHuntingState)
            {
                Vector3 position = ((MState)mHuntingState).monster.player.transform.position;
                float num = mHuntingState.InnerZoneSize();
                float outerZoneRadius = (100f - ((MState)mHuntingState).monster.SoundAlert) / 10f + (num + 2f);
                mHuntingState.huntingSource.transform.position = mHuntingState.ExtraStartPositioning(position, num, outerZoneRadius);
                Vector3 a = mHuntingState.ExtraEndPositioning(position, num, outerZoneRadius);
                mHuntingState.moveVector = a - mHuntingState.huntingSource.transform.position;
                mHuntingState.moveTime = (float)UnityEngine.Random.Range(2, 5);
                mHuntingState.moveSpeed = ((MState)mHuntingState).monster.SoundAlert;
                mHuntingState.moveSpeed *= 0.01f;
                mHuntingState.ChooseMoveType();
                mHuntingState.beenMovingFor = 0f;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Monster

            private static void HookMonster()
            {
                On.Monster.Awake += new On.Monster.hook_Awake(HookMonsterAwake);
                new Hook(typeof(Monster).GetProperty("DistanceToPlayer", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetGetMethod(), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.MultiplayerMode).GetMethod("HookMonsterget_DistanceToPlayer"), null);
            }

            private static void HookMonsterAwake(On.Monster.orig_Awake orig, Monster monster)
            {
                monster.player = References.Player; // ChooseRandomPlayer(); // Causes exception on second round.
                monster.monsterMesh = ((MonoBehaviour)monster).GetComponentsInChildren<SkinnedMeshRenderer>();
                monster.alertMeters = ((MonoBehaviour)monster).GetComponent<MAlertMeters>();
                monster.pathfinding = ((MonoBehaviour)monster).GetComponent<RaycastPathfinding>();
                monster.moveControl = ((MonoBehaviour)monster).GetComponent<MovementControl>();
                monster.vision = ((MonoBehaviour)monster).GetComponentInChildren<Vision>();
                monster.hearing = ((MonoBehaviour)monster).GetComponent<MonsterHearing>();
                monster.searcher = ((MonoBehaviour)monster).GetComponent<MSearchingState>();
                monster.roomSearcher = ((MonoBehaviour)monster).GetComponent<MRoomSearch>();
                monster.effectiveness = ((MonoBehaviour)monster).GetComponent<MonsterEffectiveness>();
                monster.ears = ((MonoBehaviour)monster).transform.GetComponentInChildren<MonsterEars>();
                monster.playerRoomDetect = monster.player.GetComponentInChildren<DetectRoom>();
                monster.monsterRoomDetect = ((MonoBehaviour)monster).GetComponentInChildren<MonstDetectRoom>();
                monster.pathfinding = ((MonoBehaviour)monster).GetComponent<RaycastPathfinding>();
                monster.hiding = monster.player.GetComponentInChildren<Hiding>();
                monster.patrol = ((MonoBehaviour)monster).GetComponent<PatrolPoints>();
                monster.anievents = ((MonoBehaviour)monster).GetComponent<AnimationEvents>();
                monster.aniLayersController = ((MonoBehaviour)monster).GetComponent<AnimationLayerController>();
                monster.huntState = ((MonoBehaviour)monster).GetComponent<MHuntingState>();
                monster.trapState = ((MonoBehaviour)monster).GetComponent<MTrappingState>();
                monster.hunterAnims = ((MonoBehaviour)monster).GetComponentInChildren<HunterAnimationsScript>();
                monster.heliEscape = (UnityEngine.Object.FindObjectOfType(typeof(HelicopterEscape)) as HelicopterEscape);
                monster.MainCollider = ((MonoBehaviour)monster).GetComponent<CapsuleCollider>();
                monster.AllSources = ((MonoBehaviour)monster).gameObject.GetComponentsInChildren<AudioSource>();
                monster.attachPoints = ((MonoBehaviour)monster).GetComponentsInChildren<AttachPoint>();
                monster.SetupTimers();
                monster.monsterType = monster.MonsterType.ToString();
            }

            public static float HookMonsterget_DistanceToPlayer(Monster monster)
            {
                return Vector3.Distance(((MonoBehaviour)monster).transform.position, monster.player.transform.position);
            }

            public static bool HookMonsterget_IsPlayerInRoom(Monster monster)
            {
                return monster.player.GetComponent<NewPlayerClass>().IsInRoom;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MonsterDoorSmoke

            private static void HookMonsterDoorSmokeTurnOffDoorSmoke(On.MonsterDoorSmoke.orig_TurnOffDoorSmoke orig, MonsterDoorSmoke monsterDoorSmoke)
            {
                orig.Invoke(monsterDoorSmoke);
                VirtualAudioSource virtualAudioSource = monsterDoorSmoke.smokeSource.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    virtualAudioSource.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MonsterHearing

            private static Vector3 HookMonsterHearingFindHidingSpot(On.MonsterHearing.orig_FindHidingSpot orig, MonsterHearing monsterHearing)
            {
                if (monsterHearing.monster.PlayerDetectRoom != null && monsterHearing.monster.PlayerDetectRoom.GetRoom.HidingSpots != null)
                {
                    HidingSpot hidingSpot = monsterHearing.monster.PlayerDetectRoom.PlayerHidingSpot(monsterHearing.monster.PlayerDetectRoom.GetRoom.HidingSpots);
                    if (hidingSpot != null)
                    {
                        return hidingSpot.MonsterPoint;
                    }
                }
                return Vector3.zero;
            }

            private static Vector3 HookMonsterHearingCalculateDSP(On.MonsterHearing.orig_CalculateDSP orig, MonsterHearing monsterHearing)
            {
                return orig.Invoke(monsterHearing);
            }

            private static void HookMonsterHearingFindLastHeardPosition(On.MonsterHearing.orig_FindLastHeardPosition orig, MonsterHearing monsterHearing, Vector3 _addition)
            {
                orig.Invoke(monsterHearing, _addition);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MonsterHidingLeftRightAnimation

            private static PlayMonsterAnimation.MonsterAnimation HookMonsterHidingLeftRightAnimationGetAnimation(On.MonsterHidingLeftRightAnimation.orig_GetAnimation orig, MonsterHidingLeftRightAnimation monsterHidingLeftRightAnimation)
            {
                Monster monster;
                NewPlayerClass monstersPlayer;
                try
                {
                    monster = ManyMonstersMode.lastMonsterSentMessage;//((MonoBehaviour)monsterHidingLeftRightAnimation).GetComponent<Monster>();
                    if (monster == null)
                    {
                        Debug.Log("Monster is null in MonsterHidingLeftRightAnimation.GetAnimation");
                    }
                    else
                    {
                        Debug.Log("Monster is NOT null in MonsterHidingLeftRightAnimation.GetAnimation");
                    }
                    monstersPlayer = monster.player.GetComponent<NewPlayerClass>();
                    if (monstersPlayer == null)
                    {
                        Debug.Log("Monster's player is null in MonsterHidingLeftRightAnimation.GetAnimation");
                    }
                    else
                    {
                        Debug.Log("Monster's player is NOT null in MonsterHidingLeftRightAnimation.GetAnimation");
                    }
                    if (monsterHidingLeftRightAnimation.spot == null)
                    {
                        Debug.Log("Monster spot is null in MonsterHidingLeftRightAnimation.GetAnimation");
                    }
                    else
                    {
                        Debug.Log("Monster spot is NOT null in MonsterHidingLeftRightAnimation.GetAnimation");
                    }
                    if (monsterHidingLeftRightAnimation.spot.MonsterPoint == null)
                    {
                        Debug.Log("Monster spot point is null in MonsterHidingLeftRightAnimation.GetAnimation");
                    }
                    else
                    {
                        Debug.Log("Monster spot point is NOT null in MonsterHidingLeftRightAnimation.GetAnimation");
                    }
                    if (monsterHidingLeftRightAnimation.spot.MonsterNormal == null)
                    {
                        Debug.Log("Monster spot normal is null in MonsterHidingLeftRightAnimation.GetAnimation");
                    }
                    else
                    {
                        Debug.Log("Monster spot normal is NOT null in MonsterHidingLeftRightAnimation.GetAnimation");
                    }
                }
                catch
                {
                    monster = References.Monster.GetComponent<Monster>();
                    monstersPlayer = References.PlayerClass;
                }

                Vector3 monsterPoint = monsterHidingLeftRightAnimation.spot.MonsterPoint;
                Vector3 monsterNormal = monsterHidingLeftRightAnimation.spot.MonsterNormal;
                Vector3 b = Vector3.Cross(Vector3.down, monsterNormal) * 0.01f + monsterPoint;
                Vector3 b2 = Vector3.Cross(Vector3.up, monsterNormal) * 0.01f + monsterPoint;
                PlayMonsterAnimation.MonsterAnimation result;
                if (monsterHidingLeftRightAnimation.room != null)
                {
                    try
                    {
                        if (Vector3.Distance(monsterHidingLeftRightAnimation.room.RoomBounds.center, b) < Vector3.Distance(monsterHidingLeftRightAnimation.room.RoomBounds.center, b2))
                        {
                            result = monsterHidingLeftRightAnimation.leftMonsterAnimation;
                            if (monstersPlayer != null)
                            {
                                monstersPlayer.SetThrowDir = 0f;
                            }
                        }
                        else
                        {
                            result = monsterHidingLeftRightAnimation.rightMonsterAnimation;
                            if (monstersPlayer != null)
                            {
                                monstersPlayer.SetThrowDir = 1f;
                            }
                        }
                    }
                    catch
                    {
                        result = monsterHidingLeftRightAnimation.leftMonsterAnimation;
                        Debug.Log("Error in MonsterHidingLeftRightAnimation.GetAnimation 1");
                    }
                }
                else
                {
                    result = monsterHidingLeftRightAnimation.leftMonsterAnimation;
                }
                return result;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MonsterStarter

            private static Vector3 HookMonsterStarterChooseSpawnPoint(On.MonsterStarter.orig_ChooseSpawnPoint orig, MonsterStarter monsterStarter)
            {
                List<GameObject> monsterSpawnPoints = LevelGeneration.Instance.MonsterSpawnPoints;
                List<GameObject> potentialSpawns = new List<GameObject>();
                float minimumDistance = 10f;
                float maximumDistance = 30f;
                for (int i = 0; i < monsterSpawnPoints.Count; i++)
                {
                    monsterSpawnPoints.Swap(i, (int)UnityEngine.Random.value % monsterSpawnPoints.Count);
                }
                GameObject chosenSpawn = null;
                for (int j = 0; j < monsterSpawnPoints.Count; j++)
                {
                    // Check that the spawn point is not closer than the minimum distance for all players.
                    if (Vector3.Distance(monsterSpawnPoints[j].transform.position, newPlayerClasses[ClosestPlayerToThis(monsterSpawnPoints[j].transform.position, true)].gameObject.transform.position) > minimumDistance)
                    {
                        // Check that the spawn point is closer than the maximum distance for any player.
                        foreach (NewPlayerClass newPlayerClass in crewPlayers)
                        {
                            if (Vector3.Distance(monsterSpawnPoints[j].transform.position, newPlayerClass.gameObject.transform.position) < maximumDistance)
                            {
                                potentialSpawns.Add(monsterSpawnPoints[j]);
                                break;
                            }
                        }
                    }

                }
                if (potentialSpawns.Count > 0)
                {
                    int index = UnityEngine.Random.Range(0, potentialSpawns.Count);
                    chosenSpawn = potentialSpawns[index];
                }
                if (chosenSpawn != null)
                {
                    monsterStarter.chosenSpawn = chosenSpawn;
                    return chosenSpawn.transform.position;
                }
                return monsterSpawnPoints[UnityEngine.Random.Range(0, monsterSpawnPoints.Count)].transform.position;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MouseLock

            private static void HookMouseLockSubtractFromMenuStack(On.MouseLock.orig_SubtractFromMenuStack orig, MouseLock mouseLock, MonoBehaviour _object)
            {

            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MouseLookCustom

            private static void HookMouseLookCustom()
            {
                On.MouseLookCustom.Start += new On.MouseLookCustom.hook_Start(HookMouseLookCustomStart);
                On.MouseLookCustom.Update += new On.MouseLookCustom.hook_Update(HookMouseLookCustomUpdate);
                On.MouseLookCustom.CapMouseLookUp += new On.MouseLookCustom.hook_CapMouseLookUp(HookMouseLookCustomCapMouseLookUp);
                On.MouseLookCustom.LerpCameraFOV += new On.MouseLookCustom.hook_LerpCameraFOV(HookMouseLookCustomLerpCameraFOV);
                On.MouseLookCustom.GetCameraFOV += new On.MouseLookCustom.hook_GetCameraFOV(HookMouseLookCustomGetCameraFOV);
            }

            private static void HookMouseLookCustomStart(On.MouseLookCustom.orig_Start orig, MouseLookCustom mouseLookCustom)
            {
                MouseLookCustom.instance = mouseLookCustom;
                if (PlayerPrefs.HasKey("SensX") && PlayerPrefs.HasKey("SensY"))
                {
                    mouseLookCustom.mouse_SensitivityX = PlayerPrefs.GetFloat("SensX");
                    mouseLookCustom.mouse_SensitivityY = PlayerPrefs.GetFloat("SensY");
                }
                else
                {
                    mouseLookCustom.mouse_SensitivityX = 5f;
                    mouseLookCustom.mouse_SensitivityY = 5f;
                }
                mouseLookCustom.npc = ((MonoBehaviour)mouseLookCustom).GetComponentInParent<NewPlayerClass>();
                mouseLookCustom.invertX = ((PlayerPrefs.GetInt("InvertX") != 0) ? -1 : 1);
                mouseLookCustom.invertY = ((PlayerPrefs.GetInt("InvertY") != 0) ? -1 : 1);
                if (PlayerPrefs.HasKey("SensControlX") && PlayerPrefs.HasKey("SensControlY"))
                {
                    mouseLookCustom.stick_SensitivityX = PlayerPrefs.GetFloat("SensControlX");
                    mouseLookCustom.stick_SensitivityY = PlayerPrefs.GetFloat("SensControlY");
                }
                else
                {
                    mouseLookCustom.stick_SensitivityX = 7.5f;
                    mouseLookCustom.stick_SensitivityY = 7.5f;
                }
                if (!ModSettings.unlockPlayerHead)
                {
                    mouseLookCustom.maximumX = 70f;
                    mouseLookCustom.minimumX = -70f;
                    mouseLookCustom.maximumY = 60f;
                    mouseLookCustom.minimumY = -60f;
                }
                else
                {
                    mouseLookCustom.maximumX = 90f;
                    mouseLookCustom.minimumX = -90f;
                    mouseLookCustom.maximumY = 90f;
                    mouseLookCustom.minimumY = -90f;
                }
                mouseLookCustom.walkFOV = PlayerCamera(mouseLookCustom.npc).fieldOfView;
                mouseLookCustom.runFOV = mouseLookCustom.walkFOV + 5f;
                if (((MonoBehaviour)mouseLookCustom).GetComponent<Rigidbody>())
                {
                    ((MonoBehaviour)mouseLookCustom).GetComponent<Rigidbody>().freezeRotation = true;
                }
                if (mouseLookCustom.cam != null)
                {
                    mouseLookCustom.camStartPos = mouseLookCustom.cam.localPosition.z;
                    mouseLookCustom.camPos = mouseLookCustom.cam.localPosition;
                }
            }

            private static void HookMouseLookCustomUpdate(On.MouseLookCustom.orig_Update orig, MouseLookCustom mouseLookCustom)
            {
                if (!mouseLookCustom.paused)
                {
                    switch (mouseLookCustom.npc.playerState)
                    {
                        case NewPlayerClass.PlayerState.Crouched:
                            if (!ModSettings.unlockPlayerHead)
                            {
                                if (mouseLookCustom.maximumY != 55f || mouseLookCustom.minimumY != -70f)
                                {

                                    mouseLookCustom.maximumY = 55f;
                                    mouseLookCustom.minimumY = -70f;
                                }
                            }
                            break;
                        case NewPlayerClass.PlayerState.Prone:
                            if (!ModSettings.unlockPlayerHead)
                            {
                                mouseLookCustom.CapMouseLookUp(10f, 20f, 10f, 0.1f);
                            }
                            break;
                        case NewPlayerClass.PlayerState.Push:
                            break;
                        case NewPlayerClass.PlayerState.UnderBed:
                            if (!ModSettings.unlockPlayerHead)
                            {
                                mouseLookCustom.CapMouseLookUp(10f, 20f, 10f, 0.1f);
                            }
                            break;
                        default:
                            if (!ModSettings.unlockPlayerHead)
                            {
                                if (mouseLookCustom.maximumY != 55f || mouseLookCustom.minimumY != -70f)
                                {
                                    mouseLookCustom.maximumY = 55f;
                                    mouseLookCustom.minimumY = -70f;
                                }
                            }
                            break;
                    }
                    if (MouseLock.Instance.IsLocked)
                    {
                        if (!mouseLookCustom.resettingHead)
                        {
                            int playerNumber = PlayerNumber(mouseLookCustom.npc.GetInstanceID());
                            if (!mouseLookCustom.headLock)
                            {
                                if (playerNumber == 1)
                                {
                                    mouseLookCustom.rotationX += /*Input.GetAxis("Mouse X") * (mouseLookCustom.mouse_SensitivityX * (float)mouseLookCustom.invertX)*/ +XboxCtrlrInput.XCI.RightStickValueX() * (mouseLookCustom.stick_SensitivityX * (float)mouseLookCustom.invertX);
                                }
                                else
                                {
                                    mouseLookCustom.rotationX += Input.GetAxis("Mouse X") * (mouseLookCustom.mouse_SensitivityX * (float)mouseLookCustom.invertX);
                                }
                                mouseLookCustom.rotationX = Mathf.Clamp(mouseLookCustom.rotationX, mouseLookCustom.minimumX, mouseLookCustom.maximumX);
                            }
                            else if (mouseLookCustom.rotationX != 0f)
                            {
                                mouseLookCustom.ResetHeadX(180f);
                            }
                            if (!OculusManager.isOculusEnabled)
                            {
                                if (playerNumber == 1)
                                {
                                    mouseLookCustom.rotationY += /*Input.GetAxis("Mouse Y") * (mouseLookCustom.mouse_SensitivityY * (float)mouseLookCustom.invertY)*/ +XboxCtrlrInput.XCI.RightStickValueY() * (mouseLookCustom.stick_SensitivityY * (float)mouseLookCustom.invertY);
                                }
                                else
                                {
                                    mouseLookCustom.rotationY += Input.GetAxis("Mouse Y") * (mouseLookCustom.mouse_SensitivityY * (float)mouseLookCustom.invertY);
                                }
                                mouseLookCustom.rotationY = Mathf.Clamp(mouseLookCustom.rotationY, mouseLookCustom.minimumY, mouseLookCustom.maximumY);
                                if (mouseLookCustom.cam != null && mouseLookCustom.rotationY <= 0f)
                                {
                                    mouseLookCustom.camPos.z = Mathf.Lerp(mouseLookCustom.camStartPos, mouseLookCustom.camStartPos + 0.125f, mouseLookCustom.rotationY / mouseLookCustom.minimumY);
                                }
                                else
                                {
                                    mouseLookCustom.camPos.z = 0f;
                                }
                                mouseLookCustom.cam.localPosition = mouseLookCustom.camPos;
                            }
                            else
                            {
                                mouseLookCustom.rotationY = 0f;
                            }
                            ((MonoBehaviour)mouseLookCustom).transform.localEulerAngles = new Vector3(-mouseLookCustom.rotationY, mouseLookCustom.rotationX, mouseLookCustom.rotationZ);
                        }
                        else
                        {
                            mouseLookCustom.ResetHeadX(180f);
                            mouseLookCustom.ResetHeadY(4f);
                        }
                    }
                }
            }

            private static void HookMouseLookCustomCapMouseLookUp(On.MouseLookCustom.orig_CapMouseLookUp orig, MouseLookCustom mouseLookCustom, float _maxX, float _minY, float _default, float _rayDistance)
            {
                Camera playerCamera = PlayerCamera(mouseLookCustom.npc);
                if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.up, _rayDistance, mouseLookCustom.mask))
                {
                    mouseLookCustom.maximumY = Mathf.Lerp(mouseLookCustom.maximumY, -_maxX, 2.5f * Time.deltaTime);
                    mouseLookCustom.minimumY = Mathf.Lerp(mouseLookCustom.minimumY, -_minY, 2.5f * Time.deltaTime);
                }
                else
                {
                    mouseLookCustom.maximumY = Mathf.Lerp(mouseLookCustom.maximumY, _default, 2.5f * Time.deltaTime);
                    mouseLookCustom.minimumY = Mathf.Lerp(mouseLookCustom.minimumY, -_minY, 2.5f * Time.deltaTime);
                }
            }

            private static void HookMouseLookCustomLerpCameraFOV(On.MouseLookCustom.orig_LerpCameraFOV orig, MouseLookCustom mouseLookCustom, float _endValue)
            {
                if (!OculusManager.isOculusEnabled)
                {
                    Camera playerCamera = PlayerCamera(mouseLookCustom.npc);
                    playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, _endValue, 2f * Time.deltaTime);
                }
            }

            private static float HookMouseLookCustomGetCameraFOV(On.MouseLookCustom.orig_GetCameraFOV orig, MouseLookCustom mouseLookCustom)
            {
                return PlayerCamera(mouseLookCustom.npc).fieldOfView;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MovementControl

            private static bool HookMovementControlClimbChecking(On.MovementControl.orig_ClimbChecking orig, MovementControl movementControl)
            {
                bool result = false;
                if (movementControl.monster != null && movementControl.monster.RoomDetect != null && movementControl.monster.RoomDetect.CurrentRoom != null && movementControl.monster.RoomDetect.CurrentRoom.RoomType == RoomStructure.Cargo && movementControl.finishedJump && movementControl.GetAniControl.inClimbingZone && !movementControl.LockRotation)
                {
                    if (!ModSettings.enableCrewVSMonsterMode || CrewVsMonsterMode.letAIControlMonster)
                    {
                        if (movementControl.monster.CanSeePlayer || movementControl.monster.CanSensePlayerNear)
                        {
                            if (movementControl.monster.player.transform.position.y - ((MonoBehaviour)movementControl).transform.position.y > 1.5f)
                            {
                                result = true;
                            }
                        }
                        else if (movementControl.FarAheadNodePos().y - movementControl.CurrentNodePos.y > 1.75f && movementControl.CurrentNodePos != Vector3.zero && movementControl.FarAheadNodePos() != Vector3.zero)
                        {
                            result = true;
                        }
                    }
                    else
                    {
                        int playerNumber = ModSettings.numbersOfMonsterPlayers[ManyMonstersMode.MonsterNumber(movementControl.monster.GetInstanceID())];
                        if (GetPlayerKey("Jump", playerNumber).JustPressed())
                        {
                            result = true;
                        }
                    }
                }
                return result;
            }

            private static void HookMovementControlJumpChecking(On.MovementControl.orig_JumpChecking orig, MovementControl movementControl)
            {
                if (movementControl.nearJump && !movementControl.isClimbing && movementControl.finishedJump)
                {
                    if (!ModSettings.enableCrewVSMonsterMode || CrewVsMonsterMode.letAIControlMonster)
                    {
                        if (movementControl.monster.CanSeePlayerNotHiding || movementControl.monster.CanSensePlayerNear || movementControl.monster.PersistChase)
                        {
                            if (movementControl.monster.player.transform.position.y - movementControl.CurrentNodePos.y < -1f)
                            {
                                movementControl.JumpDownHole();
                            }
                        }
                        else if (Mathf.Abs(movementControl.AheadNodePos.y - movementControl.CurrentNodePos.y) > 1f && movementControl.CurrentNodePos != Vector3.zero)
                        {
                            movementControl.JumpDownHole();
                        }
                    }
                    else
                    {
                        int playerNumber = ModSettings.numbersOfMonsterPlayers[ManyMonstersMode.MonsterNumber(movementControl.monster.GetInstanceID())];
                        if (GetPlayerKey("Jump", playerNumber).JustPressed())
                        {
                            movementControl.JumpDownHole();
                        }
                        /*
                        else
                        {
                            movementControl.nearJump = false;
                        }
                        */
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MovePlayerUnderBed

            private static void HookMovePlayerUnderBed()
            {
                On.MovePlayerUnderBed.Lerp += new On.MovePlayerUnderBed.hook_Lerp(HookMovePlayerUnderBedLerp);
                On.MovePlayerUnderBed.SetAnimationTarget += new On.MovePlayerUnderBed.hook_SetAnimationTarget(HookMovePlayerUnderBedSetAnimationTarget);
                On.MovePlayerUnderBed.StartTransition += new On.MovePlayerUnderBed.hook_StartTransition(HookMovePlayerUnderBedStartTransition);
                On.MovePlayerUnderBed.Update += new On.MovePlayerUnderBed.hook_Update(HookMovePlayerUnderBedUpdate);
            }

            private static float[] movePlayerUnderBedTimers;

            private static bool IsProneNotTransitioning(NewPlayerClass newPlayerClass)
            {
                return PlayerCamera(newPlayerClass).transform.position.y - newPlayerClass.transform.position.y < 0.35f && newPlayerClass.IsProne();
            }

            private static void HookMovePlayerUnderBedLerp(On.MovePlayerUnderBed.orig_Lerp orig, MovePlayerUnderBed movePlayerUnderBed)
            {
                int playerCrewNumber = crewPlayers.IndexOf(movePlayerUnderBed.player);
                if (movePlayerUnderBedTimers[playerCrewNumber] <= 0f)
                {
                    if (IsProneNotTransitioning(movePlayerUnderBed.player))
                    {
                        movePlayerUnderBed.startPoint = movePlayerUnderBed.player.transform.position;
                        movePlayerUnderBed.startRotation = movePlayerUnderBed.player.transform.rotation;
                    }
                    if (movePlayerUnderBed.moveDirection < 0f)
                    {
                        movePlayerUnderBed.Finish();
                    }
                }
                if (IsProneNotTransitioning(movePlayerUnderBed.player))
                {
                    movePlayerUnderBed.custom.UnlockPlayerHead();
                    movePlayerUnderBedTimers[playerCrewNumber] += movePlayerUnderBed.moveDirection * Time.deltaTime * movePlayerUnderBed.speed;
                    movePlayerUnderBedTimers[playerCrewNumber] = Mathf.Clamp01(movePlayerUnderBedTimers[playerCrewNumber]);
                    movePlayerUnderBed.player.transform.position = Vector3.Lerp(movePlayerUnderBed.startPoint, movePlayerUnderBed.pointSelection.position, Mathf.SmoothStep(0f, 1f, movePlayerUnderBedTimers[playerCrewNumber]));
                    movePlayerUnderBed.player.transform.rotation = Quaternion.Slerp(movePlayerUnderBed.startRotation, movePlayerUnderBed.pointSelection.rotation, Mathf.SmoothStep(0f, 1f, movePlayerUnderBedTimers[playerCrewNumber]));
                    if (MovePlayerUnderBed.triggerObjectives.isStillTutorial)
                    {
                        MovePlayerUnderBed.triggerObjectives.exitBed = true;
                        if (MovePlayerUnderBed.tutorialBed != null)
                        {
                            UnityEngine.Object.Destroy(MovePlayerUnderBed.tutorialBed.gameObject);
                        }
                    }
                }
            }

            private static void HookMovePlayerUnderBedSetAnimationTarget(On.MovePlayerUnderBed.orig_SetAnimationTarget orig, MovePlayerUnderBed movePlayerUnderBed, float _bedTurn)
            {
                int playerCrewNumber = crewPlayers.IndexOf(movePlayerUnderBed.player);
                movePlayerUnderBed.player.BedTurnDir = Mathf.MoveTowards(movePlayerUnderBed.player.BedTurnDir, _bedTurn * Mathf.Clamp01(movePlayerUnderBed.animationSpeed.Evaluate(movePlayerUnderBedTimers[playerCrewNumber])), Time.deltaTime * 6f);
            }

            private static void HookMovePlayerUnderBedStartTransition(On.MovePlayerUnderBed.orig_StartTransition orig, MovePlayerUnderBed movePlayerUnderBed, Transform _entryPoint)
            {
                orig.Invoke(movePlayerUnderBed, _entryPoint);

                if (movePlayerUnderBedTimers == null)
                {
                    movePlayerUnderBedTimers = new float[crewPlayers.Count];
                }
                else
                {
                    movePlayerUnderBedTimers[PlayerNumber(movePlayerUnderBed.player.GetInstanceID())] = 0f;
                }
            }

            private static void HookMovePlayerUnderBedUpdate(On.MovePlayerUnderBed.orig_Update orig, MovePlayerUnderBed movePlayerUnderBed)
            {
                if (movePlayerUnderBed.active)
                {
                    for (int i = 0; i < crewPlayers.Count; i++)
                    {
                        int playerNumber = PlayerNumber(crewPlayers[i].GetInstanceID());
                        movePlayerUnderBed.player = crewPlayers[i];
                        movePlayerUnderBed.custom = movePlayerUnderBed.player.mouseLook;
                        if (movePlayerUnderBed.player.playerState == NewPlayerClass.PlayerState.Prone)
                        {
                            if (movePlayerUnderBed.player.movePlayerUnderBed)
                            {
                                bool flag = false;
                                movePlayerUnderBed.player.mouseLook.UnlockPlayerHead();
                                if (GetPlayerKey("Forward", playerNumber).IsDown() || GetPlayerAxisValue("Y", playerNumber) >= 0.01f)
                                {
                                    movePlayerUnderBed.moveDirection = 1f;
                                    flag = true;
                                }
                                if ((GetPlayerKey("Back", playerNumber).IsDown() || GetPlayerKey("Left", playerNumber).IsDown() || GetPlayerKey("Right", playerNumber).IsDown()) || GetPlayerAxisValue("Y", playerNumber) <= -0.01f)
                                {
                                    movePlayerUnderBed.moveDirection = -1f;
                                    flag = true;
                                }
                                if (!flag)
                                {
                                    movePlayerUnderBed.moveDirection = 0f;
                                }
                                movePlayerUnderBed.Lerp();
                                if (movePlayerUnderBedTimers[i] != 0f && movePlayerUnderBedTimers[i] != 1f)
                                {
                                    movePlayerUnderBed.SetAnimationTarget(movePlayerUnderBed.moveDirection);
                                }
                                else
                                {
                                    movePlayerUnderBed.SetAnimationTarget(0f);
                                }
                            }
                            else
                            {
                                movePlayerUnderBed.Finish();
                                movePlayerUnderBed.player.SetToCrouch();
                            }
                        }
                        else
                        {
                            movePlayerUnderBed.SetAnimationTarget(0f);
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MRoomSearch

            private static void HookMRoomSearchChaseChecking(On.MRoomSearch.orig_ChaseChecking orig, MRoomSearch mRoomSearch)
            {
                if (!mRoomSearch.changingState && mRoomSearch.allowChase)
                {
                    if (((MState)mRoomSearch).monster.CanSeePlayer)
                    {
                        mRoomSearch.changingState = true;
                        ((MState)mRoomSearch).SendEvent("Chase");
                    }
                    else if (((MState)mRoomSearch).monster.CanHearNoise)
                    {
                        if (((MState)mRoomSearch).monster.Hearing.lastHeardRoom != ((MState)mRoomSearch).monster.RoomDetect.CurrentRoom && !((MState)mRoomSearch).monster.BothOnDeck() && !((MState)mRoomSearch).monster.BothInEngineRoom())
                        {
                            mRoomSearch.changingState = true;
                            ((MState)mRoomSearch).SendEvent("Search");
                        }
                        else if (((MState)mRoomSearch).monster.Hearing.CurrentSoundSource != null)
                        {
                            ((MState)mRoomSearch).monster.Hearing.CurrentSoundSource.transform.position = new Vector3(((MState)mRoomSearch).monster.Hearing.CurrentSoundSource.transform.position.x, ((MState)mRoomSearch).monster.RoomDetect.CurrentRoomBounds.center.y, ((MState)mRoomSearch).monster.Hearing.CurrentSoundSource.transform.position.z);
                            ((MState)mRoomSearch).monster.player.transform.position = new Vector3(((MState)mRoomSearch).monster.player.transform.position.x, ((MState)mRoomSearch).monster.RoomDetect.CurrentRoomBounds.center.y, ((MState)mRoomSearch).monster.player.transform.position.z);
                            if (((MState)mRoomSearch).monster.Hearing.CurrentSoundSource.Source.transform.root.tag == "Player" || ((MState)mRoomSearch).monster.Hearing.CurrentSoundSource.Gameplay.highAlertSound)
                            {
                                mRoomSearch.changingState = true;
                                ((MState)mRoomSearch).SendEvent("Search");
                            }
                        }
                    }
                }
            }

            private static HidingSpot[] HookMRoomSearchFindDeckHidingSpots(On.MRoomSearch.orig_FindDeckHidingSpots orig, MRoomSearch mRoomSearch)
            {
                List<HidingSpot> list = new List<HidingSpot>();
                List<Room> list2 = new List<Room>();
                Collider[] array = Physics.OverlapSphere(((MState)mRoomSearch).monster.player.transform.position, 5f, mRoomSearch.floorMask);
                for (int i = 0; i < array.Length; i++)
                {
                    Room room = array[i].GetComponent<Room>();
                    if (room == null)
                    {
                        room = array[i].GetComponentInParent<Room>();
                    }
                    if (room != null)
                    {
                        bool flag = false;
                        foreach (Room y in list2)
                        {
                            if (room == y)
                            {
                                flag = true;
                            }
                        }
                        if (!flag)
                        {
                            list2.Add(room);
                            HidingSpot[] componentsInChildren = room.GetComponentsInChildren<HidingSpot>();
                            foreach (HidingSpot hidingSpot in componentsInChildren)
                            {
                                if (hidingSpot != null && hidingSpot.enabled)
                                {
                                    NodeData nodeDataAtPosition = LevelGeneration.GetNodeDataAtPosition(hidingSpot.gameObject.transform.position);
                                    RoomStructure roomType = nodeDataAtPosition.nodeRoom.RoomType;
                                    NodeData nodeDataAtPosition2 = LevelGeneration.GetNodeDataAtPosition(((MState)mRoomSearch).monster.player.transform.position);
                                    if (roomType != RoomStructure.Deck)
                                    {
                                        if (roomType != RoomStructure.Walkway)
                                        {
                                            if (roomType == RoomStructure.Engine)
                                            {
                                                if (nodeDataAtPosition.regionNode.y == nodeDataAtPosition2.regionNode.y)
                                                {
                                                    list.Add(hidingSpot);
                                                }
                                            }
                                        }
                                        else if (nodeDataAtPosition.regionNode.y == nodeDataAtPosition2.regionNode.y)
                                        {
                                            list.Add(hidingSpot);
                                        }
                                    }
                                    else
                                    {
                                        list.Add(hidingSpot);
                                    }
                                }
                            }
                        }
                    }
                }
                return list.ToArray();
            }

            private static void HookMRoomSearchOnExit(On.MRoomSearch.orig_OnExit orig, MRoomSearch mRoomSearch)
            {
                orig.Invoke(mRoomSearch);
                ChanceToChooseNewPlayer(((MState)mRoomSearch).monster);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MSearchingState

            private static void HookMSearchingStateFindGoal(On.MSearchingState.orig_FindGoal orig, MSearchingState mSearchingState)
            {
                ((MState)mSearchingState).monster.ShouldFindNewGoal = false;
                mSearchingState.AlertType = ((MState)mSearchingState).monster.GetAlertMeters.HighestAlert;
                if (((MState)mSearchingState).monster.GetAlertMeters.IsAtHighProx)
                {
                    mSearchingState.finishedFirstSearch = false;
                    mSearchingState.isSearchingSight = false;
                    mSearchingState.isSearchingSound = false;
                    mSearchingState.isSearchingProx = true;
                    mSearchingState.secondarySearch = false;
                    mSearchingState.RandomiseSearch(mSearchingState.monster.player.transform.position, "Prox");
                }
                else if (mSearchingState.AlertType == "Sight")
                {
                    mSearchingState.finishedFirstSearch = false;
                    mSearchingState.isSearchingSight = true;
                    mSearchingState.isSearchingSound = false;
                    mSearchingState.isSearchingProx = false;
                    mSearchingState.secondarySearch = false;
                    if (((MState)mSearchingState).monster.LastSeenPlayerPosition == Vector3.zero)
                    {
                        ((MState)mSearchingState).monster.GetAlertMeters.mSightAlert = 0f;
                    }
                    else
                    {
                        mSearchingState.RandomiseSearch(((MState)mSearchingState).monster.LastSeenPlayerPosition, "Sight");
                    }
                }
                else if (mSearchingState.AlertType == "Sound")
                {
                    mSearchingState.finishedFirstSearch = false;
                    mSearchingState.isSearchingSight = false;
                    mSearchingState.isSearchingSound = true;
                    mSearchingState.isSearchingProx = false;
                    mSearchingState.secondarySearch = false;
                    if (((MState)mSearchingState).monster.Hearing.CurrentSoundSource != null && ((MState)mSearchingState).monster.Hearing.CurrentSoundSource.Gameplay.highAlertSound && mSearchingState.currentDistraction != ((MState)mSearchingState).monster.Hearing.CurrentSoundSource)
                    {
                        mSearchingState.currentDistraction = ((MState)mSearchingState).monster.Hearing.CurrentSoundSource;
                        ((MState)mSearchingState).monster.MoveControl.GetAniControl.HeardASound = true;
                        ((MState)mSearchingState).monster.IsDistracted = true;
                    }
                    if (((MState)mSearchingState).monster.Hearing.CurrentSoundSource == null && ((MState)mSearchingState).monster.Hearing.lastHeardRoom != null)
                    {
                        mSearchingState.RandomiseSearch(((MState)mSearchingState).monster.Hearing.lastHeardRoom.RoomBounds.center, "Sound");
                    }
                    else if (((MState)mSearchingState).monster.Hearing.PointOfInterest != Vector3.zero)
                    {
                        mSearchingState.RandomiseSearch(((MState)mSearchingState).monster.Hearing.PointOfInterest, "Sound");
                    }
                    else
                    {
                        mSearchingState.RandomiseSearch(((MState)mSearchingState).monster.transform.position, "Sound");
                    }
                }
                else if (mSearchingState.AlertType == "Prox")
                {
                    mSearchingState.finishedFirstSearch = false;
                    mSearchingState.isSearchingSight = false;
                    mSearchingState.isSearchingSound = false;
                    mSearchingState.isSearchingProx = true;
                    mSearchingState.secondarySearch = false;
                    mSearchingState.RandomiseSearch(mSearchingState.monster.player.transform.position, "Prox");
                }
                else if (((MState)mSearchingState).IsActive)
                {
                    ((MState)mSearchingState).SendEvent("PlayerLost");
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MTrappingState

            private static void HookMTrappingStateChooseStandSpawnDirection(On.MTrappingState.orig_ChooseStandSpawnDirection orig, MTrappingState mTrappingState)
            {
                Vector3 vector = mTrappingState.triggeredAmbush.lookAt.position + mTrappingState.triggeredAmbush.transform.forward * -3f;
                Vector3 vector2 = mTrappingState.triggeredAmbush.lookAt.position + mTrappingState.triggeredAmbush.transform.forward * 3f;
                Vector3 playerPosition = ((MState)mTrappingState).monster.player.transform.position;
                float num = Vector3.Distance(vector, playerPosition);
                float num2 = Vector3.Distance(vector2, playerPosition);
                if (num2 < num)
                {
                    ((MState)mTrappingState).monster.MoveControl.SetToFace(vector2);
                }
                else
                {
                    ((MState)mTrappingState).monster.MoveControl.SetToFace(vector);
                }
            }

            private static void HookMTrappingStatePickRoarType(On.MTrappingState.orig_PickRoarType orig, MTrappingState mTrappingState)
            {
                float roarType = 0f;
                Vector3 position = ((MState)mTrappingState).monster.transform.position;
                Vector3 target = Vector3.zero;
                if (mTrappingState.triggeredByPlayer)
                {
                    target = ((MState)mTrappingState).monster.player.transform.position;
                }
                else if (((MState)mTrappingState).monster.HasPointOfInterest)
                {
                    target = ((MState)mTrappingState).monster.Hearing.PointOfInterest;
                }
                Ray ray = new Ray(((MState)mTrappingState).monster.transform.position, target - position);
                Vector3 item = Vector3.zero;
                List<float> list = new List<float>();
                List<Vector3> list2 = new List<Vector3>();
                float num = Vector3.Angle(((MState)mTrappingState).monster.transform.forward, ray.direction);
                item = position + ((MState)mTrappingState).monster.transform.forward * 5f;
                num = Mathf.Abs(num);
                list.Add(num);
                list2.Add(item);
                float num2 = Vector3.Angle(((MState)mTrappingState).monster.transform.right * -1f, ray.direction);
                num2 = Mathf.Abs(num2);
                item = position + ((MState)mTrappingState).monster.transform.right * -5f;
                list.Add(num2);
                list2.Add(item);
                float num3 = Vector3.Angle(((MState)mTrappingState).monster.transform.right, ray.direction);
                num3 = Mathf.Abs(num3);
                item = position + ((MState)mTrappingState).monster.transform.right * 5f;
                list.Add(num3);
                list2.Add(item);
                float num4 = 999f;
                mTrappingState.afterRoarLookAt = list2[0];
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] < num4)
                    {
                        num4 = list[i];
                        mTrappingState.afterRoarLookAt = list2[i];
                        roarType = (float)i;
                    }
                }
                ((MState)mTrappingState).monster.HunterAnimations.RoarType = roarType;
            }

            private static void HookMTrappingStateSetUpAmbush(On.MTrappingState.orig_SetUpAmbush orig, MTrappingState mTrappingState)
            {
                if (mTrappingState.ShouldSpawnImmediately)
                {
                    if (((MState)mTrappingState).monster.PlayerDetectRoom.GetRoomCategory == RoomCategory.Outside)
                    {
                        mTrappingState.Ambush(((MState)mTrappingState).monster.Hearing.PointOfInterest);
                    }
                    else if (mTrappingState.PlayerInCargoHold())
                    {
                        mTrappingState.isCargoPods = true;
                        AmbushSystem.DeployAreaAmbushes(AmbushPoint.TrapType.Cargo, new AmbushSystem.OnAmbushDeploy(mTrappingState.AmbushDeploy));
                    }
                    else if (mTrappingState.PlayerInEngineRoom())
                    {
                        mTrappingState.Ambush(((MState)mTrappingState).monster.Hearing.PointOfInterest);
                    }
                    else if (((MState)mTrappingState).monster.Hearing.PointOfInterest != Vector3.zero && ((MState)mTrappingState).monster.CanHearNoise)
                    {
                        mTrappingState.Ambush(((MState)mTrappingState).monster.Hearing.PointOfInterest);
                    }
                    else
                    {
                        mTrappingState.Ambush(((MState)mTrappingState).monster.player.transform.position);
                    }
                }
                else if (mTrappingState.PlayerInCargoHold())
                {
                    mTrappingState.isCargoPods = true;
                    AmbushSystem.DeployAreaAmbushes(AmbushPoint.TrapType.Cargo, new AmbushSystem.OnAmbushDeploy(mTrappingState.AmbushDeploy));
                }
                else if (mTrappingState.PlayerInEngineRoom())
                {
                    mTrappingState.isEnginePods = true;
                    AmbushSystem.DeployAreaAmbushes(AmbushPoint.TrapType.EnginePipe, new AmbushSystem.OnAmbushDeploy(mTrappingState.AmbushDeploy));
                }
                else
                {
                    AmbushSystem.Deploy(new AmbushSystem.OnAmbushDeploy(mTrappingState.AmbushDeploy));
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MWanderState

            private static void HookMWanderState()
            {
                // OnEnter moved to BaseFeatures.
                On.MWanderState.PlayerFarFromGoal += new On.MWanderState.hook_PlayerFarFromGoal(HookMWanderStatePlayerFarFromGoal);
            }

            private static bool HookMWanderStatePlayerFarFromGoal(On.MWanderState.orig_PlayerFarFromGoal orig, MWanderState mWanderState)
            {
                Vector3 position = mWanderState.monster.player.transform.position;
                Vector3 goal = ((MState)mWanderState).monster.MoveControl.Goal;
                bool result = false;
                float num = Vector3.Distance(position, goal);
                if (num > 30f)
                {
                    result = true;
                }
                return result;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @NewPlayerClass

            private static void HookNewPlayerClass()
            {
                On.NewPlayerClass.ButtonInput += new On.NewPlayerClass.hook_ButtonInput(HookNewPlayerClassButtonInput);
                On.NewPlayerClass.EndFixedAnimation += new On.NewPlayerClass.hook_EndFixedAnimation(HookNewPlayerClassEndFixedAnimation);
                On.NewPlayerClass.LeaningLogic += new On.NewPlayerClass.hook_LeaningLogic(HookNewPlayerClassLeaningLogic);
                On.NewPlayerClass.Lerp += new On.NewPlayerClass.hook_Lerp(HookNewPlayerClassLerp);
                On.NewPlayerClass.Mec_StartItemLoop += new On.NewPlayerClass.hook_Mec_StartItemLoop(HookNewPlayerClassMec_StartItemLoop);
                On.NewPlayerClass.MouseMovement += new On.NewPlayerClass.hook_MouseMovement(HookNewPlayerClassMouseMovement);
                On.NewPlayerClass.StartFixedAnimation += new On.NewPlayerClass.hook_StartFixedAnimation(HookNewPlayerClassStartFixedAnimation);
                On.NewPlayerClass.StateButtonLogic += new On.NewPlayerClass.hook_StateButtonLogic(HookNewPlayerClassStateButtonLogic);
                On.NewPlayerClass.StateLogic += new On.NewPlayerClass.hook_StateLogic(HookNewPlayerClassStateLogic);
            }

            private static void HookNewPlayerClassButtonInput(On.NewPlayerClass.orig_ButtonInput orig, NewPlayerClass newPlayerClass)
            {
                int playerNumber = PlayerNumber(newPlayerClass.GetInstanceID());
                if (crewPlayers.Contains(newPlayerClass))
                {
                    if (GetPlayerKey("Sprint", playerNumber).IsDown() || GetPlayerTriggerStateIfUsingController("Left", playerNumber))
                    {
                        switch (newPlayerClass.playerState)
                        {
                            case NewPlayerClass.PlayerState.Standing:
                                if (newPlayerClass.playerMotor.ZMovement > 0f && !newPlayerClass.playerRunning)
                                {
                                    newPlayerClass.playerRunning = true;
                                }
                                break;
                            case NewPlayerClass.PlayerState.Prone:
                                if (newPlayerClass.mouseLookCustom.IsPlayerHeadLocked())
                                {
                                    newPlayerClass.proneTurnAnimation = false;
                                    newPlayerClass.mouseLookCustom.UnlockPlayerHead();
                                }
                                break;
                            case NewPlayerClass.PlayerState.UnderBed:
                                if (newPlayerClass.mouseLookCustom.IsPlayerHeadLocked())
                                {
                                    newPlayerClass.proneTurnAnimation = false;
                                    newPlayerClass.mouseLookCustom.UnlockPlayerHead();
                                }
                                break;
                        }
                    }
                    if (!newPlayerClass.lockPlayerBody)
                    {
                        if (GetPlayerKey("LeanLeft", playerNumber).IsDown() && newPlayerClass.playerState == NewPlayerClass.PlayerState.Standing)
                        {
                            if (GetPlayerKey("LeanRight", playerNumber).IsDown())
                            {
                                newPlayerClass.leanDirection = 0f;
                            }
                            else
                            {
                                newPlayerClass.leanDirection = -1f;
                            }
                        }
                        if (GetPlayerKey("LeanRight", playerNumber).IsDown() && newPlayerClass.playerState == NewPlayerClass.PlayerState.Standing)
                        {
                            if (GetPlayerKey("LeanLeft", playerNumber).IsDown())
                            {
                                newPlayerClass.leanDirection = 0f;
                            }
                            else
                            {
                                newPlayerClass.leanDirection = 1f;
                            }
                        }
                        if (!newPlayerClass.IsPlayerLeaning && newPlayerClass.pushedBack == 0f)
                        {
                            if (GetPlayerKey("Crouch", playerNumber).JustPressed() && !newPlayerClass.InTransitionState() && newPlayerClass.standUpCount == 0 && newPlayerClass.animationStarted)
                            {
                                newPlayerClass.animationEnded = true;
                            }
                            if (GetPlayerKey("Jump", playerNumber).JustPressed() && !newPlayerClass.InTransitionState())
                            {
                                switch (newPlayerClass.playerState)
                                {
                                    case NewPlayerClass.PlayerState.Standing:
                                        if (!newPlayerClass.IsStunned)
                                        {
                                            newPlayerClass.playerMotor.Jump();
                                        }
                                        break;
                                    case NewPlayerClass.PlayerState.Crouched:
                                        newPlayerClass.playerRunning = false;
                                        if (newPlayerClass.CanStandFromCrouch)
                                        {
                                            AudioSystem.PlaySound("Noises/Actions/State Changes/CrouchStand", ((MonoBehaviour)newPlayerClass).transform, newPlayerClass.headSource);
                                            newPlayerClass.StandUp();
                                        }
                                        break;
                                    case NewPlayerClass.PlayerState.Prone:
                                        newPlayerClass.playerRunning = false;
                                        if (newPlayerClass.CanStandFromProne)
                                        {
                                            AudioSystem.PlaySound("Noises/Actions/State Changes/ProneStand", ((MonoBehaviour)newPlayerClass).transform, newPlayerClass.headSource);
                                            newPlayerClass.mouseLookCustom.LockPlayerHead();
                                            newPlayerClass.StandUp();
                                        }
                                        break;
                                }
                                if (newPlayerClass.animationStarted)
                                {
                                    newPlayerClass.animationEnded = true;
                                }
                            }
                            if (!newPlayerClass.movePlayerUnderBed)
                            {
                                if (GetPlayerKey("Forward", playerNumber).IsDown() || GetPlayerKey("Back", playerNumber).IsDown() || GetPlayerKey("Left", playerNumber).IsDown() || GetPlayerKey("Right", playerNumber).IsDown())
                                {
                                    float num = 0f;
                                    if (GetPlayerKey("Forward", playerNumber).IsDown())
                                    {
                                        num += 1f;
                                    }
                                    if (GetPlayerKey("Back", playerNumber).IsDown())
                                    {
                                        num -= 1f;
                                    }
                                    if (num > 0f)
                                    {
                                        newPlayerClass.zMovementDirection = 1f;
                                        newPlayerClass.playerMotor.PressForward(-1f);
                                    }
                                    if (num < 0f)
                                    {
                                        newPlayerClass.zMovementDirection = -1f;
                                        newPlayerClass.playerMotor.PressBack(-1f);
                                    }
                                    if (num != 0f)
                                    {
                                        if (newPlayerClass.animationStarted)
                                        {
                                            newPlayerClass.animationEnded = true;
                                        }
                                    }
                                    else
                                    {
                                        newPlayerClass.zMovementDirection = 0f;
                                    }
                                    float num2 = 0f;
                                    if (GetPlayerKey("Right", playerNumber).IsDown())
                                    {
                                        num2 += 1f;
                                    }
                                    if (GetPlayerKey("Left", playerNumber).IsDown())
                                    {
                                        num2 -= 1f;
                                    }
                                    if (num2 > 0f)
                                    {
                                        newPlayerClass.xMovementDirection = 1f;
                                        newPlayerClass.playerMotor.PressRight(-1f);
                                    }
                                    if (num2 < 0f)
                                    {
                                        newPlayerClass.xMovementDirection = -1f;
                                        newPlayerClass.playerMotor.PressLeft(-1f);
                                    }
                                    if (num2 != 0f)
                                    {
                                        if (newPlayerClass.animationStarted)
                                        {
                                            newPlayerClass.animationEnded = true;
                                        }
                                    }
                                    else
                                    {
                                        newPlayerClass.xMovementDirection = 0f;
                                    }
                                }
                                else if (GetPlayerKey("Forward", playerNumber).keyInUse == KeyCode.None)
                                {
                                    if (Mathf.Abs(GetPlayerAxisValue("Y", playerNumber)) > 0f)
                                    {
                                        if (GetPlayerAxisValue("Y", playerNumber) > 0f)
                                        {
                                            newPlayerClass.playerMotor.PressForward(GetPlayerAxisValue("Y", playerNumber));
                                        }
                                        else
                                        {
                                            newPlayerClass.playerMotor.PressBack(-GetPlayerAxisValue("Y", playerNumber));
                                        }
                                        newPlayerClass.zMovementDirection = newPlayerClass.Thumbstick2Movespeed(GetPlayerAxisValue("Y", playerNumber));
                                        if (newPlayerClass.animationStarted)
                                        {
                                            newPlayerClass.animationEnded = true;
                                        }
                                    }
                                    else
                                    {
                                        newPlayerClass.zMovementDirection = 0f;
                                    }
                                    if (Mathf.Abs(GetPlayerAxisValue("X", playerNumber)) > 0f)
                                    {
                                        if (GetPlayerAxisValue("X", playerNumber) > 0f)
                                        {
                                            newPlayerClass.playerMotor.PressRight(GetPlayerAxisValue("X", playerNumber));
                                        }
                                        else
                                        {
                                            newPlayerClass.playerMotor.PressLeft(-GetPlayerAxisValue("X", playerNumber));
                                        }
                                        newPlayerClass.xMovementDirection = newPlayerClass.Thumbstick2Movespeed(GetPlayerAxisValue("X", playerNumber));
                                        if (newPlayerClass.animationStarted)
                                        {
                                            newPlayerClass.animationEnded = true;
                                        }
                                    }
                                    else
                                    {
                                        newPlayerClass.xMovementDirection = 0f;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private static void HookNewPlayerClassEndFixedAnimation(On.NewPlayerClass.orig_EndFixedAnimation orig, NewPlayerClass newPlayerClass)
            {
                lastPlayerSentMessage = newPlayerClass;
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Updating lastPlayerSentMessage: " + PlayerNumber(lastPlayerSentMessage.GetInstanceID()) + "\n" + new StackTrace().ToString() + "\n-----");
                }
                newPlayerClass.fixedAnimationPlaying = false;
                newPlayerClass.fixedAnimationLoopPlaying = false;
                newPlayerClass.loopEntered = false;
                newPlayerClass.loopExited = false;
                newPlayerClass.animationStarted = false;
                newPlayerClass.animationEnded = false;
                newPlayerClass.lerpT = 0f;
                newPlayerClass.handReleased = true;
                newPlayerClass.animationType = PlayerAnimations.AnimationType.None;
                if (!newPlayerClass.mouseLookCustom.IsPlayerHeadLocked())
                {
                    newPlayerClass.mouseLookCustom.LockPlayerHead();
                }
                if (newPlayerClass.itemNeeded == PlayerAnimations.ItemNeeded.Yes)
                {
                    ((MonoBehaviour)newPlayerClass).BroadcastMessage("OnFinishItemAnimation", SendMessageOptions.DontRequireReceiver);
                }
                if (newPlayerClass.fixedInteractable != null)
                {
                    newPlayerClass.fixedInteractable.FinishedAnimation();
                }
                PlayerInventory(newPlayerClass).EnableItemChanges();
                newPlayerClass.readyToAnimate = true;
                newPlayerClass.interactingWithThis = null;
            }

            private static void HookNewPlayerClassLeaningLogic(On.NewPlayerClass.orig_LeaningLogic orig, NewPlayerClass newPlayerClass)
            {
                int playerNumber = PlayerNumber(newPlayerClass.GetInstanceID());
                float num = 4f;
                float target = newPlayerClass.leanDirection * 2f;
                if (!GetPlayerKey("LeanLeft", playerNumber).IsDown() && !GetPlayerKey("LeanRight", playerNumber).IsDown())
                {
                    target = 0f;
                    newPlayerClass.allowLean = true;
                }
                if (!newPlayerClass.allowLean && newPlayerClass.tapToRetryLean)
                {
                    target = 0f;
                }
                newPlayerClass.leanStrength = Mathf.MoveTowards(newPlayerClass.leanStrength, target, num * Time.deltaTime);
                float num2 = (newPlayerClass.leanStrength >= 0f) ? 2f : -2f;
                Vector3 vector = newPlayerClass.leanRaycaster.right * newPlayerClass.leanCalibration * num2;
                float num3 = newPlayerClass.leanStrength;
                bool flag = false;
                if (GetPlayerKey("LeanLeft", playerNumber).IsDown() || GetPlayerKey("LeanRight", playerNumber).IsDown() || newPlayerClass.leanStrength != 0f)
                {
                    num3 = newPlayerClass.LeanRaycast(newPlayerClass.leanRaycaster.position, vector, num3, num2, ref flag);
                    num3 = newPlayerClass.LeanRaycast(newPlayerClass.leanRaycaster.position, Quaternion.AngleAxis(10f, Vector3.up) * vector, num3, num2, ref flag);
                    num3 = newPlayerClass.LeanRaycast(newPlayerClass.leanRaycaster.position, Quaternion.AngleAxis(-10f, Vector3.up) * vector, num3, num2, ref flag);
                    if (newPlayerClass.leanStrength < 0f && num3 > newPlayerClass.leanStrength)
                    {
                        newPlayerClass.leanStrength = num3;
                    }
                    if (newPlayerClass.leanStrength > 0f && num3 < newPlayerClass.leanStrength)
                    {
                        newPlayerClass.leanStrength = num3;
                    }
                    if (flag)
                    {
                        newPlayerClass.leanStrength = Mathf.MoveTowards(newPlayerClass.leanStrength, 0f, num * Time.deltaTime * 2f);
                        newPlayerClass.allowLean = false;
                    }
                }
                if (newPlayerClass.leanStrength != 0f || newPlayerClass.fixedAnimation != PlayerAnimations.FixedAnimations.Default || newPlayerClass.IsGrabbed || newPlayerClass.IsProne() || newPlayerClass.Trapped || newPlayerClass.PushBackDir != 0f || GetPlayerKey("LeanLeft", playerNumber).IsDown() || GetPlayerKey("LeanRight", playerNumber).IsDown())
                {
                    newPlayerClass.upperBodyLock.weighting2 -= Time.deltaTime * 3f;
                }
                else
                {
                    newPlayerClass.upperBodyLock.weighting2 += Time.deltaTime * 3f;
                }
                newPlayerClass.upperBodyLock.weighting2 = Mathf.Clamp01(newPlayerClass.upperBodyLock.weighting2);
            }

            private static void HookNewPlayerClassLerp(On.NewPlayerClass.orig_Lerp orig, NewPlayerClass newPlayerClass)
            {
                newPlayerClass.lerpT += newPlayerClass.lerpSpeed * Time.deltaTime;
                if (newPlayerClass.lerpT > 1f)
                {
                    newPlayerClass.lerpT = 1f;
                }
                ((MonoBehaviour)newPlayerClass).transform.position = Vector3.Lerp(newPlayerClass.lerpStartPos, newPlayerClass.animationTransform.position, newPlayerClass.lerpT);
                ((MonoBehaviour)newPlayerClass).transform.rotation = Quaternion.Slerp(newPlayerClass.lerpStartRot, newPlayerClass.animationTransform.rotation, newPlayerClass.lerpT);
                if (newPlayerClass.lerpT == 1f)
                {
                    lastPlayerSentMessage = newPlayerClass;
                    if (ModSettings.logDebugText)
                    {
                        Debug.Log("Updating lastPlayerSentMessage: " + PlayerNumber(lastPlayerSentMessage.GetInstanceID()) + "\n" + new StackTrace().ToString() + "\n-----");
                    }
                    newPlayerClass.transformLerping = false;
                    newPlayerClass.fixedInteractable.StartedAnimation();
                    newPlayerClass.fixedAnimationPlaying = true;
                    if (newPlayerClass.itemNeeded == PlayerAnimations.ItemNeeded.Yes)
                    {
                        ((MonoBehaviour)newPlayerClass).BroadcastMessage("OnStartItemAnimation", SendMessageOptions.DontRequireReceiver);
                    }
                }
            }

            private static void HookNewPlayerClassMec_StartItemLoop(On.NewPlayerClass.orig_Mec_StartItemLoop orig, NewPlayerClass newPlayerClass)
            {
                Inventory inventory = PlayerInventory(newPlayerClass);
                if (inventory.CurrentItem != null)
                {
                    inventory.CurrentItem.SendMessage("Mec_OnStartItemLoop");
                }
            }

            private static void HookNewPlayerClassMouseMovement(On.NewPlayerClass.orig_MouseMovement orig, NewPlayerClass newPlayerClass)
            {
                if (!newPlayerClass.InProneTransition())
                {
                    NewPlayerClass.PlayerState playerState = newPlayerClass.playerState;
                    if (playerState != NewPlayerClass.PlayerState.Prone)
                    {
                        int playerNumber = PlayerNumber(newPlayerClass.GetInstanceID());
                        if (playerState != NewPlayerClass.PlayerState.Push)
                        {
                            if (newPlayerClass.playerState != NewPlayerClass.PlayerState.Prone && newPlayerClass.playerState != NewPlayerClass.PlayerState.UnderBed && !newPlayerClass.lockPlayerBody)
                            {
                                float yAngle;
                                if (playerNumber == 1)
                                {
                                    yAngle = XboxCtrlrInput.XCI.RightStickValueX() * (newPlayerClass.mouseLookCustom.stick_SensitivityX * 1.3f * (float)newPlayerClass.mouseLookCustom.invertX);
                                }
                                else
                                {
                                    yAngle = Input.GetAxis("Mouse X") * (newPlayerClass.mouseLookCustom.mouse_SensitivityX * (float)newPlayerClass.mouseLookCustom.invertX);
                                }
                                if (newPlayerClass.animationType == PlayerAnimations.AnimationType.None)
                                {
                                    ((MonoBehaviour)newPlayerClass).transform.Rotate(0f, yAngle, 0f);
                                    newPlayerClass.canRotate = true;
                                }
                                else
                                {
                                    if (newPlayerClass.fixedInteractable.freeLook && newPlayerClass.mouseLookCustom.IsPlayerHeadLocked())
                                    {
                                        newPlayerClass.mouseLookCustom.UnlockPlayerHead();
                                    }
                                    newPlayerClass.canRotate = false;
                                }
                            }
                        }
                        else
                        {
                            newPlayerClass.canRotate = false;
                            if (newPlayerClass.mouseLookCustom.IsPlayerHeadLocked())
                            {
                                float num;
                                if (playerNumber == 1)
                                {
                                    num = XboxCtrlrInput.XCI.RightStickValueX() * (newPlayerClass.mouseLookCustom.stick_SensitivityX * 1.3f * (float)newPlayerClass.mouseLookCustom.invertX);
                                }
                                else
                                {
                                    num = Input.GetAxis("Mouse X") * (newPlayerClass.mouseLookCustom.mouse_SensitivityX * (float)newPlayerClass.mouseLookCustom.invertX);
                                }
                                if (newPlayerClass.verticalMovementAxis != 0f || newPlayerClass.horizontalMovementAxis != 0f)
                                {
                                    newPlayerClass.proneTurnAnimation = false;
                                    ((MonoBehaviour)newPlayerClass).transform.Rotate(0f, num * 0.5f, 0f);
                                }
                            }
                        }
                    }
                    else
                    {
                        newPlayerClass.canRotate = false;
                    }
                }
            }

            private static void HookNewPlayerClassStartFixedAnimation(On.NewPlayerClass.orig_StartFixedAnimation orig, NewPlayerClass newPlayerClass, Transform _transform, PlayerAnimations.FixedAnimations _fixedAnimation, PlayerAnimations.SpecificFix _specificFix, PlayerAnimations.AnimationType _animationType, PlayerAnimations.ItemNeeded _itemNeeded, FixedInteractable _fixedInteractable, string _animationName = "Default")
            {
                // ~ Crew player seems to not be able to interact sometimes either.
                int playerNumber = PlayerNumber(newPlayerClass.GetInstanceID());
                if (newPlayerClass.readyToAnimate && (!ModSettings.enableCrewVSMonsterMode || (ModSettings.letMonsterUseInteractiveObjects && _fixedAnimation != PlayerAnimations.FixedAnimations.Climb) || !ModSettings.numbersOfMonsterPlayers.Contains(playerNumber)))
                {
                    lastPlayerSentMessage = newPlayerClass;
                    if (ModSettings.logDebugText)
                    {
                        Debug.Log("Updating lastPlayerSentMessage: " + PlayerNumber(lastPlayerSentMessage.GetInstanceID()) + "\n" + new StackTrace().ToString() + "\n-----");
                    }
                    Debug.Log("Starting fixed animation for player number " + playerNumber);
                    newPlayerClass.readyToAnimate = false;
                    newPlayerClass.animationTransform = _transform;
                    Vector3 position = newPlayerClass.animationTransform.position;
                    position.y = ((MonoBehaviour)newPlayerClass).transform.position.y;
                    newPlayerClass.animationTransform.position = position;
                    newPlayerClass.lerpStartPos = ((MonoBehaviour)newPlayerClass).transform.position;
                    newPlayerClass.lerpStartRot = ((MonoBehaviour)newPlayerClass).transform.rotation;
                    newPlayerClass.transformLerping = true;
                    newPlayerClass.lerpSpeed = 4f / (Vector3.Distance(newPlayerClass.lerpStartPos, position) + 0.01f);
                    newPlayerClass.lerpT = 0f;
                    newPlayerClass.holdTime = 0f;
                    newPlayerClass.interactingWithThis = _fixedInteractable.gameObject;
                    newPlayerClass.specFix = _specificFix;
                    newPlayerClass.fixedAnimation = _fixedAnimation;
                    newPlayerClass.StandUp();
                    newPlayerClass.animationType = _animationType;
                    newPlayerClass.fixedInteractable = _fixedInteractable;
                    newPlayerClass.itemNeeded = _itemNeeded;
                    newPlayerClass.handReleased = false;
                    newPlayerClass.hasBeenInFixedAnimation = false;
                    PlayerInventory(newPlayerClass).DisableItemChanges();
                }
            }

            private static void HookNewPlayerClassStateButtonLogic(On.NewPlayerClass.orig_StateButtonLogic orig, NewPlayerClass newPlayerClass)
            {
                int playerNumber = PlayerNumber(newPlayerClass.GetInstanceID());
                if (GetPlayerKey("Crouch", playerNumber).JustReleased() && (!ModSettings.enableCrewVSMonsterMode || (ModSettings.enableCrewVSMonsterMode && !ModSettings.numbersOfMonsterPlayers.Contains(playerNumber))))
                {
                    switch (newPlayerClass.playerState)
                    {
                        case NewPlayerClass.PlayerState.Standing:
                            if (newPlayerClass.standUpCount <= 0 && !newPlayerClass.movePlayerUnderBed && newPlayerClass.mouseLookCustom.headLock)
                            {
                                AudioSystem.PlaySound("Noises/Actions/State Changes/StandCrouch", ((MonoBehaviour)newPlayerClass).transform, newPlayerClass.headSource);
                                newPlayerClass.SetToCrouch();
                            }
                            break;
                        case NewPlayerClass.PlayerState.Crouched:
                            if (newPlayerClass.CanStandFromCrouch && newPlayerClass.mouseLookCustom.headLock)
                            {
                                AudioSystem.PlaySound("Noises/Actions/State Changes/CrouchStand", ((MonoBehaviour)newPlayerClass).transform, newPlayerClass.headSource);
                                newPlayerClass.StandUp();
                            }
                            break;
                        case NewPlayerClass.PlayerState.Prone:
                            if (newPlayerClass.CanStandFromProne)
                            {
                                AudioSystem.PlaySound("Noises/Actions/State Changes/ProneCrouch", ((MonoBehaviour)newPlayerClass).transform, newPlayerClass.headSource);
                                newPlayerClass.SetToCrouch();
                                newPlayerClass.mouseLookCustom.LockPlayerHead();
                            }
                            break;
                    }
                }
            }

            private static void HookNewPlayerClassStateLogic(On.NewPlayerClass.orig_StateLogic orig, NewPlayerClass newPlayerClass)
            {
                newPlayerClass.playerMotor.SetControllerSize(newPlayerClass.controller.radius, newPlayerClass.heightController.GetHeight());
                int playerNumber = PlayerNumber(newPlayerClass.GetInstanceID());
                if (!newPlayerClass.BetweenStates())
                {
                    if (!newPlayerClass.InTransitionState())
                    {
                        switch (newPlayerClass.playerState)
                        {
                            case NewPlayerClass.PlayerState.Standing:
                                if (newPlayerClass.playerRunning)
                                {
                                    if (newPlayerClass.animationType != PlayerAnimations.AnimationType.None)
                                    {
                                        newPlayerClass.playerRunning = false;
                                    }
                                    newPlayerClass.playerMotor.SetPose(PlayerMotor.Pose.Run);
                                    float num;
                                    if (PlayerPrefs.HasKey("FOV"))
                                    {
                                        num = PlayerPrefs.GetFloat("FOV") + 5f;
                                    }
                                    else
                                    {
                                        num = 65f;
                                    }
                                    newPlayerClass.getFOVOnce = false;
                                    if (newPlayerClass.mouseLookCustom.GetCameraFOV() < num)
                                    {
                                        newPlayerClass.mouseLookCustom.LerpCameraFOV(num);
                                    }
                                    newPlayerClass.timeRunning += Time.deltaTime;
                                    if (newPlayerClass.timeRunning > 5f && !newPlayerClass.headSource.isPlaying)
                                    {
                                        AudioSystem.PlaySound("Noises/Breathing/Heavy", newPlayerClass.headSource);
                                    }
                                }
                                else
                                {
                                    newPlayerClass.playerMotor.SetPose(PlayerMotor.Pose.Walk);
                                }
                                break;
                            case NewPlayerClass.PlayerState.Crouched:
                                newPlayerClass.playerRunning = false;
                                newPlayerClass.playerMotor.SetPose(PlayerMotor.Pose.Crouch);
                                break;
                            case NewPlayerClass.PlayerState.Prone:
                                newPlayerClass.playerRunning = false;
                                newPlayerClass.playerMotor.SetPose(PlayerMotor.Pose.Prone);
                                if (!(GetPlayerKey("Sprint", playerNumber).IsDown() || GetPlayerTriggerStateIfUsingController("Left", playerNumber)) && !newPlayerClass.mouseLookCustom.IsPlayerHeadLocked())
                                {
                                    newPlayerClass.mouseLookCustom.LockPlayerHead();
                                }
                                break;
                            case NewPlayerClass.PlayerState.Push:
                                newPlayerClass.playerRunning = false;
                                if (!newPlayerClass.mouseLookCustom.IsPlayerHeadLocked())
                                {
                                    newPlayerClass.mouseLookCustom.UnlockPlayerHead();
                                }
                                break;
                            case NewPlayerClass.PlayerState.UnderBed:
                                newPlayerClass.playerRunning = false;
                                newPlayerClass.playerMotor.SetPose(PlayerMotor.Pose.Prone);
                                if (!(GetPlayerKey("Sprint", playerNumber).IsDown() || GetPlayerTriggerStateIfUsingController("Left", playerNumber)) && !newPlayerClass.mouseLookCustom.IsPlayerHeadLocked())
                                {
                                    newPlayerClass.mouseLookCustom.LockPlayerHead();
                                }
                                break;
                        }
                    }
                    if (!GetPlayerKey("Forward", playerNumber).IsDown() && !GetPlayerKey("Back", playerNumber).IsDown() && Mathf.Abs(GetPlayerAxisValue("Y", playerNumber)) < 0.1f)
                    {
                        newPlayerClass.zMovementDirection = 0f;
                    }
                    if (!GetPlayerKey("Left", playerNumber).IsDown() && !GetPlayerKey("Right", playerNumber).IsDown() && Mathf.Abs(GetPlayerAxisValue("X", playerNumber)) < 0.1f)
                    {
                        newPlayerClass.xMovementDirection = 0f;
                    }
                }
                if (!(GetPlayerKey("Sprint", playerNumber).IsDown() || GetPlayerTriggerStateIfUsingController("Left", playerNumber)) || newPlayerClass.playerMotor.ZMovement <= 0f)
                {
                    newPlayerClass.playerRunning = false;
                }
                if (!newPlayerClass.playerRunning)
                {
                    if (!newPlayerClass.getFOVOnce)
                    {
                        if (PlayerPrefs.HasKey("FOV"))
                        {
                            newPlayerClass.walkFOV = PlayerPrefs.GetFloat("FOV");
                        }
                        newPlayerClass.getFOVOnce = true;
                    }
                    if (newPlayerClass.mouseLookCustom.GetCameraFOV() > newPlayerClass.walkFOV)
                    {
                        newPlayerClass.mouseLookCustom.LerpCameraFOV(newPlayerClass.walkFOV);
                    }
                    if (newPlayerClass.timeRunning > 0f)
                    {
                        newPlayerClass.timeRunning -= Time.deltaTime * 5f;
                    }
                }
                if (newPlayerClass.playerState != NewPlayerClass.PlayerState.Prone && !newPlayerClass.fixedAnimationPlaying && !newPlayerClass.lockPlayerBody && newPlayerClass.playerState != NewPlayerClass.PlayerState.Push && !newPlayerClass.mouseLookCustom.IsPlayerHeadLocked())
                {
                    newPlayerClass.mouseLookCustom.LockPlayerHead();
                }
                newPlayerClass.verticalMovementAxis = Mathf.Lerp(newPlayerClass.verticalMovementAxis, newPlayerClass.zMovementDirection, 5f * Time.deltaTime);
                newPlayerClass.horizontalMovementAxis = Mathf.Lerp(newPlayerClass.horizontalMovementAxis, newPlayerClass.xMovementDirection, 5f * Time.deltaTime);
                if (Mathf.Abs(newPlayerClass.verticalMovementAxis) < 0.01f)
                {
                    newPlayerClass.verticalMovementAxis = 0f;
                }
                if (Mathf.Abs(newPlayerClass.horizontalMovementAxis) < 0.01f)
                {
                    newPlayerClass.horizontalMovementAxis = 0f;
                }
            }

            public static int PlayerNumber(int passedPlayerInstanceID)
            {
                if (newPlayerClasses != null)
                {
                    for (int i = 1; i < newPlayerClasses.Count; i++)
                    {
                        if (newPlayerClasses[i].GetInstanceID() == passedPlayerInstanceID)
                        {
                            return i;
                        }
                    }
                }
                return 0;
            }

            // Using IndexOf is better as there are no duplicates in the ModSettings.numbersOfMonsterPlayersList. int monsterNumber = ModSettings.numbersOfMonsterPlayersList.GetIndex(playerNumber);
            /*
            public static int PlayerMonsterNumber(int passedPlayerInstanceID) // Links monster players to monster numbers. THIS METHOD IS NOT SAFE TO BE CALLED IF IT IS NOT KNOWN WHETHER THE PLAYER IS A MONSTER PLAYER AS IT WILL RETURN 0 EVEN IF THE PLAYER BEING CHECKED IS NOT A MONSTER.
            {
                if (monsterPlayers != null)
                {
                    for (int i = 1; i < monsterPlayers.Count; i++)
                    {
                        if (monsterPlayers[i].GetInstanceID() == passedPlayerInstanceID)
                        {
                            return i;
                        }
                    }
                }
                return 0;
            }

            public static int PlayerMonsterNumberFromPlayerNumber(int passedPlayerNumber) // Also links monster players to monster numbers. THIS METHOD IS NOT SAFE TO BE CALLED IF IT IS NOT KNOWN WHETHER THE PLAYER IS A MONSTER PLAYER AS IT WILL RETURN 0 EVEN IF THE PLAYER BEING CHECKED IS NOT A MONSTER.
            {
                if (ModSettings.numbersOfMonsterPlayers != null)
                {
                    for (int i = 1; i < ModSettings.numbersOfMonsterPlayers.Count; i++)
                    {
                        if (ModSettings.numbersOfMonsterPlayers[i] == passedPlayerNumber)
                        {
                            return i;
                        }
                    }
                }
                return 0;
            }
            */

            /*----------------------------------------------------------------------------------------------------*/
            // @PauseMenu

            private static void HookPauseMenu()
            {
                On.PauseMenu.OnExitPauseGame += new On.PauseMenu.hook_OnExitPauseGame(HookPauseMenuOnExitPauseGame);
                On.PauseMenu.OnPauseGame += new On.PauseMenu.hook_OnPauseGame(HookPauseMenuOnPauseGame);
            }

            private static void HookPauseMenuUpdate(On.PauseMenu.orig_Update orig, PauseMenu pauseMenu)
            {
                if (pauseMenu.fadeIn.fadeInComplete && pauseMenu.enableToggle)
                {
                    if (!pauseMenu.pause)
                    {
                        if (MouseLock.Instance.IsLocked && (Input.GetKeyDown(KeyCode.Escape) /*|| XboxCtrlrInput.XCI.GetButtonDown(XboxCtrlrInput.XboxButton.Start)*/ || pauseMenu.pauseFromOverlay))
                        {
                            MouseLock[] mouseLocks = FindObjectsOfType<MouseLock>();
                            MouseLookCustom[] mouseLookCustoms = FindObjectsOfType<MouseLookCustom>();
                            MouseLockMenuScene[] mouseLockMenuScenes = FindObjectsOfType<MouseLockMenuScene>();
                            MouseHandler[] mouseHandlers = FindObjectsOfType<MouseHandler>();
                            Debug.Log("Found " + mouseLocks.Length + " mouseLocks, " + mouseLookCustoms.Length + " mouseLookCustoms, " + mouseLockMenuScenes.Length + " mouseLockMenuScenes and " + mouseHandlers.Length + " mouseHandlers.");
                            foreach (MouseLock mouseLock in mouseLocks)
                            {
                                Debug.Log("Mouse lock has " + mouseLock.menus.Count + " menus");
                            }
                            Debug.Log("Toggling pause 1");
                            pauseMenu.pauseFromOverlay = false;
                            pauseMenu.TogglePause();
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Escape) /*|| XboxCtrlrInput.XCI.GetButtonDown(XboxCtrlrInput.XboxButton.Start)*/)
                    {
                        if (pauseMenu.optionsEnabled)
                        {
                            Debug.Log("Toggling pause 2");
                            pauseMenu.pauseButtons.SetActive(true);
                            pauseMenu.optionsButtons.SetActive(false);
                            pauseMenu.optionsEnabled = false;
                        }
                        else
                        {
                            Debug.Log("Toggling pause 3");
                            pauseMenu.TogglePause();
                        }
                    }
                }
            }

            private static void HookPauseMenuOnExitPauseGame(On.PauseMenu.orig_OnExitPauseGame orig, PauseMenu pauseMenu)
            {
                orig.Invoke(pauseMenu);
                for (int i = 1; i < inventories.Count; i++)
                {
                    inventories[i].hideItem = false;
                }
            }

            private static void HookPauseMenuOnPauseGame(On.PauseMenu.orig_OnPauseGame orig, PauseMenu pauseMenu)
            {
                if (OculusManager.isOculusEnabled)
                {
                    foreach (Inventory inventory in inventories)
                    {
                        inventory.hideItem = true;
                    }
                }
            }

            private static void HookPauseMenuShow(On.PauseMenu.orig_Show orig, PauseMenu pauseMenu)
            {
                pauseMenu.EnableToggle();
                MouseLock.Instance.AddToMenuStack(pauseMenu);
                /*
                if (InputHelper.IsGamepadConnected)
                {
                    Input.ResetInputAxes();
                    pauseMenu.selection = 0;
                    pauseMenu.SelectButton(pauseMenu.selection);
                    pauseMenu.SetActiveChildren(true);
                }
                else
                {
                    */
                pauseMenu.SetActiveChildren(true);
                //}
                if (pauseMenu.optionsEnabled)
                {
                    pauseMenu.pauseButtons.SetActive(false);
                    pauseMenu.optionsButtons.SetActive(true);
                }
                else
                {
                    pauseMenu.pauseButtons.SetActive(true);
                    pauseMenu.optionsButtons.SetActive(false);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Phone

            private static void HookPhoneOnStartFixedAnimation(On.Phone.orig_OnStartFixedAnimation orig, Phone phone)
            {
                phone.player = lastPlayerSentMessage;
                PhoneManager.Instance.Refresh();
                phone.EndPhoneCall(phone.current.connectedPhone);
                ((MonoBehaviour)phone).StopAllCoroutines();
                phone.player.usingPhone = true;
                phone.player.handReleased = false;
                ((MonoBehaviour)phone).StartCoroutine(WaitForEndCall(phone, phone.player));
                ((MonoBehaviour)phone).StartCoroutine(phone.CallPhone());
                phone.ani.Play("pickup");
                phone.handSetGO.layer = LayerMask.NameToLayer("Player");
                AudioSystem.PlaySound("Noises/Actions/Phone/Reciever/PickUp", phone.recieverSource);
            }

            private static IEnumerator WaitForEndCall(Phone phone, NewPlayerClass newPlayerClass)
            {
                float timeRang = 0f;
                bool released = false;
                float movementTimeout = 0f;
                phone.lastCalled = phone.current.connectedPhone;
                phone.dSound = null;
                if (phone.lastCalled != null)
                {
                    phone.dialToneSource.loop = true;
                    if (phone.lastCalled != phone && !phone.lastCalled.phoneDestroyed)
                    {
                        phone.dialToneSource = AudioSystem.PlaySound(phone.dialTone, phone.dialToneSource);
                        phone.lastCalled.Receive();
                    }
                    else
                    {
                        phone.dialToneSource = AudioSystem.PlaySound(phone.busyTone, phone.dialToneSource);
                        if (phone.lastCalled.phoneDestroyed)
                        {
                            phone.lastCalled.destroyedCallTimer.ResetTimer();
                            phone.lastCalled.destroyedCallTimer.StartTimer();
                        }
                    }
                    while ((!released || timeRang < 1.5f) && !newPlayerClass.handReleased)
                    {
                        if (phone.dSound == null)
                        {
                            phone.dSound = phone.lastCalled.ringSource.GetComponent<DistractionSound>();
                            if (phone.dSound != null)
                            {
                                phone.dSound.SetMonsterApproach(new DistractionSound.MonsterApproach(phone.MonsterApproach));
                            }
                        }
                        timeRang += Time.deltaTime;
                        yield return null;
                        int playerNumber = PlayerNumber(newPlayerClass.GetInstanceID());
                        if ((GetPlayerKey("Forward", playerNumber).IsDown() || GetPlayerKey("Back", playerNumber).IsDown() || GetPlayerKey("Left", playerNumber).IsDown() || GetPlayerKey("Right", playerNumber).IsDown()) || (Mathf.Abs(GetPlayerAxisValue("Y", playerNumber)) > 0.1f || Mathf.Abs(GetPlayerAxisValue("X", playerNumber)) > 0.1f))
                        {
                            movementTimeout += Time.deltaTime;
                        }
                        else
                        {
                            movementTimeout -= Time.deltaTime;
                            movementTimeout = Mathf.Clamp(movementTimeout, 0f, float.MaxValue);
                        }
                        if (phone.lastCalled.phoneDestroyed)
                        {
                            if (!phone.dialToneSource.isPlaying)
                            {
                                phone.dialToneSource = AudioSystem.PlaySound(phone.busyTone, phone.dialToneSource);
                            }
                            phone.lastCalled.EndReceive();
                        }
                        if (/*KeyBinds.InteractKeyBind.JustPressed() ||*/ movementTimeout > 0.1f || timeRang > 30f || (phone.lastCalled.phoneDestroyed && phone.lastCalled.destroyedCallTimer.TimeElapsed > 5f))
                        {
                            released = true;
                        }
                    }
                    phone.player = newPlayerClass;
                    phone.EndPhoneCall(phone.lastCalled);
                    phone.current = null;
                    phone.Refresh();
                }
                yield break;
            }

            private static void HookPhoneEndPhoneCall(On.Phone.orig_EndPhoneCall orig, Phone phone, Phone _calledPhone)
            {
                orig.Invoke(phone, _calledPhone);
                VirtualAudioSource virtualAudioSource = phone.dialToneSource.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    virtualAudioSource.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                }
            }

            private static void HookPhoneEndReceive(On.Phone.orig_EndReceive orig, Phone phone)
            {
                orig.Invoke(phone);
                VirtualAudioSource virtualAudioSource = phone.ringSource.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    virtualAudioSource.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                }
            }

            private static IEnumerator HookPhoneWaitAndDestroy(On.Phone.orig_WaitAndDestroy orig, Phone phone)
            {
                Debug.Log("This hook is working!: " + new StackTrace().ToString()); // # IEnumerator Hook Test
                yield return null;
                yield return null;
                phone.lastCalled.gameObject.SetActive(false);
                phone.lastCalled.destroyedObject.SetActive(true);
                phone.dialToneSource.Stop();
                VirtualAudioSource virtualAudioSource = phone.dialToneSource.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    virtualAudioSource.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                }
                phone.lastCalled.destroyedCallTimer.ResetTimer();
                phone.lastCalled.destroyedCallTimer.StartTimer();
                for (int i = 0; i < phone.lastCalled.lights.Length; i++)
                {
                    phone.lastCalled.lights[i].lightType = GenericLight.LightTypes.Off;
                    phone.lastCalled.lights[i].On = false;
                }
                PhoneManager.Instance.Refresh();
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @PhoneButton

            private static bool HookPhoneButtonIsConditionMet(On.PhoneButton.orig_IsConditionMet orig, PhoneButton phoneButton)
            {
                bool condition = lastPlayerCheckingInteractableConditions.handReleased && !lastPlayerCheckingInteractableConditions.IsCrouched() && !lastPlayerCheckingInteractableConditions.IsProne() && (phoneButton.phone.Powered || !phoneButton.phone.debugNeedsPower) && !(phoneButton.connectedPhone == null) && phoneButton.connectedPhone.Powered;
                //Debug.Log("Checking phone button condition. It is " + condition.ToString() + " for player number " + PlayerNumber(lastPlayerCheckingInteractableConditions.GetInstanceID()));
                return condition;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @PitTrap

            private static void HookPitTrapDestroyFloor(On.PitTrap.orig_DestroyFloor orig, PitTrap pitTrap, string _reason)
            {
                if (!ModSettings.unbreakablePitTraps)
                {
                    if (pitTrap.normalModel != null)
                    {
                        pitTrap.normalModel.SetActive(false);
                    }
                    if (pitTrap.destroyedModel != null)
                    {
                        pitTrap.destroyedModel.SetActive(true);
                    }
                    if (_reason == "Player" && pitTrap.player != null)
                    {
                        pitTrap.player.BroadcastMessage("OnPlayerStumble", SendMessageOptions.DontRequireReceiver);
                        pitTrap.selectedDirection = pitTrap.forwardsTrans;
                        Vector3 forward = pitTrap.player.transform.forward;
                        Vector3 forward2 = pitTrap.backwardsTrans.forward;
                        forward.y = (forward2.y = 0f);
                        if (Vector3.Angle(forward, forward2) < 90f)
                        {
                            pitTrap.selectedDirection = pitTrap.backwardsTrans;
                        }
                    }
                    if (_reason == "Monster" && PitTrap.monster != null)
                    {
                        PitTrap.monster.gameObject.BroadcastMessage("OnMonsterStumble", SendMessageOptions.DontRequireReceiver);
                    }
                    pitTrap.destroyed = true;
                }
                else if (_reason == "Player" && pitTrap.player != null)
                {
                    pitTrap.player.BroadcastMessage("OnPlayerStumble", SendMessageOptions.DontRequireReceiver);
                    pitTrap.selectedDirection = pitTrap.forwardsTrans;
                    Vector3 forward = pitTrap.player.transform.forward;
                    Vector3 forward2 = pitTrap.backwardsTrans.forward;
                    forward.y = (forward2.y = 0f);
                    if (Vector3.Angle(forward, forward2) < 90f)
                    {
                        pitTrap.selectedDirection = pitTrap.backwardsTrans;
                    }
                }
            }

            private static void HookPitTrapUpdate(On.PitTrap.orig_Update orig, PitTrap pitTrap)
            {
                if (pitTrap.destroyed && pitTrap.destroyedTime < 3f)
                {
                    pitTrap.destroyedTime += Time.deltaTime;
                    if (pitTrap.selectedDirection != null)
                    {
                        Vector3 position = pitTrap.player.transform.position;
                        Vector3 forward = pitTrap.player.transform.forward;
                        float num = Vector3.Angle(pitTrap.selectedDirection.forward, pitTrap.player.transform.forward);
                        if (pitTrap.destroyedTime < 0.7f)
                        {
                            Vector3 a = MathHelper.GetClosestPointToLine(pitTrap.selectedDirection.position + pitTrap.selectedDirection.forward * 2f, pitTrap.selectedDirection.position - pitTrap.selectedDirection.forward * 2f, position);
                            if (num >= 60f)
                            {
                                a -= forward * 0.3f;
                            }
                            a.y = position.y;
                            Vector3 vector = a - position;
                            if (vector.magnitude > 0f)
                            {
                                float num2 = Time.deltaTime * 3f;
                                num2 = Mathf.Clamp(num2, 0f, vector.magnitude);
                                pitTrap.player.GetComponent<CharacterController>().Move(vector.normalized * num2);
                            }
                        }
                        if (pitTrap.destroyedTime < 1.5f && num < 60f)
                        {
                            Quaternion to = Quaternion.LookRotation(pitTrap.selectedDirection.forward, Vector3.up);
                            Quaternion rotation = pitTrap.player.transform.rotation;
                            float num3 = num * 4f + 50f;
                            pitTrap.player.transform.rotation = Quaternion.RotateTowards(rotation, to, Time.deltaTime * num3);
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @PlayerAnimationEvents

            private static void HookPlayerAnimationEvents()
            {
                On.PlayerAnimationEvents.Awake += new On.PlayerAnimationEvents.hook_Awake(HookPlayerAnimationEventsAwake);
                On.PlayerAnimationEvents.Mec_InventoryItemSound += new On.PlayerAnimationEvents.hook_Mec_InventoryItemSound(HookPlayerAnimationEventsMec_InventoryItemSound); // Maybe do not play at another player's position unless you can play their audio too.
                On.PlayerAnimationEvents.Mec_StopUsingItem += new On.PlayerAnimationEvents.hook_Mec_StopUsingItem(HookPlayerAnimationEventsMec_StopUsingItem);
                On.PlayerAnimationEvents.Mec_UnlockThePlayer += new On.PlayerAnimationEvents.hook_Mec_UnlockThePlayer(HookPlayerAnimationEventsMec_UnlockThePlayer);
                On.PlayerAnimationEvents.Mec_UseCurrentItem += new On.PlayerAnimationEvents.hook_Mec_UseCurrentItem(HookPlayerAnimationEventsMec_UseCurrentItem);
                On.PlayerAnimationEvents.Mec_WireCut += new On.PlayerAnimationEvents.hook_Mec_WireCut(HookPlayerAnimationEventsMec_WireCut);
            }

            private static void HookPlayerAnimationEventsAwake(On.PlayerAnimationEvents.orig_Awake orig, PlayerAnimationEvents playerAnimationEvents)
            {
                playerAnimationEvents.headSource = ((MonoBehaviour)playerAnimationEvents).transform.FindChild("CameraParent").GetComponentInChildren<AudioSource>();
                playerAnimationEvents.playerClass = ((MonoBehaviour)playerAnimationEvents).GetComponentInParent<NewPlayerClass>();
                Debug.Log("Player animation events player class ID is " + playerAnimationEvents.playerClass.GetInstanceID());
            }

            private static void HookPlayerAnimationEventsMec_InventoryItemSound(On.PlayerAnimationEvents.orig_Mec_InventoryItemSound orig, PlayerAnimationEvents playerAnimationEvents, string soundlibrary)
            {
                playerAnimationEvents.headSource.Stop();
                if (!useLegacyAudio)
                {
                    VirtualAudioSource virtualAudioSource = playerAnimationEvents.headSource.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                }
                AudioSystem.PlaySound(soundlibrary, PlayerInventory(playerAnimationEvents.playerClass).CurrentItem.transform, playerAnimationEvents.headSource);
            }

            private static void HookPlayerAnimationEventsMec_StopUsingItem(On.PlayerAnimationEvents.orig_Mec_StopUsingItem orig, PlayerAnimationEvents playerAnimationEvents)
            {
                PlayerInventory(playerAnimationEvents.playerClass).CurrentItem.BroadcastMessage("Broadcast_TurnItemOff");
            }

            private static void HookPlayerAnimationEventsMec_UnlockThePlayer(On.PlayerAnimationEvents.orig_Mec_UnlockThePlayer orig, PlayerAnimationEvents playerAnimationEvents)
            {
                Animator animator = playerAnimationEvents.playerClass.playerAnimator;
                animator.applyRootMotion = false;
                playerAnimationEvents.playerClass.IsGrabbed = false;
                playerAnimationEvents.playerClass.Motor.isHaulted = false;
                playerAnimationEvents.playerClass.Motor.disableMove = false;
                playerAnimationEvents.playerClass.Motor.useGravity = true;
                Rigidbody playerRigidbody = playerAnimationEvents.playerClass.GetComponent<Rigidbody>();
                playerRigidbody.useGravity = true;
                playerRigidbody.isKinematic = true;
                playerAnimationEvents.playerClass.UnlockPlayerBody();
                playerAnimationEvents.playerClass.mouseLook.UnlockPlayerHead();
                PlayerInventory(playerAnimationEvents.playerClass).hideItem = false;
                PlayerUpperBodyLock componentInChildren = playerAnimationEvents.playerClass.GetComponentInChildren<PlayerUpperBodyLock>();
                if (componentInChildren != null)
                {
                    componentInChildren.weighting3 = 1f;
                }
                FollowTransform component2 = playerAnimationEvents.playerClass.mouseLook.GetComponent<FollowTransform>();
                if (component2 != null)
                {
                    component2.applyRotation = false;
                }
            }

            private static void HookPlayerAnimationEventsMec_UseCurrentItem(On.PlayerAnimationEvents.orig_Mec_UseCurrentItem orig, PlayerAnimationEvents playerAnimationEvents)
            {
                PlayerInventory(playerAnimationEvents.playerClass).CurrentItem.BroadcastMessage("Broadcast_TurnItemOn");
            }

            private static void HookPlayerAnimationEventsMec_WireCut(On.PlayerAnimationEvents.orig_Mec_WireCut orig, PlayerAnimationEvents playerAnimationEvents)
            {
                PlayerInventory(playerAnimationEvents.playerClass).CurrentItem.SendMessage("OnWireCut");
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @PlayerAnimationLayersController

            private static void HookPlayerAnimationLayersControllerMakeOnlyLayerActive(On.PlayerAnimationLayersController.orig_MakeOnlyLayerActive orig, PlayerAnimationLayersController playerAnimationLayersController, int layer)
            {
                NewPlayerClass newPlayerClass = ((MonoBehaviour)playerAnimationLayersController).GetComponentInParent<NewPlayerClass>();
                Debug.Log("Player animation layers controller player class ID is " + newPlayerClass.GetInstanceID());
                Animator playerMecanim = newPlayerClass.playerMecanim;
                if (playerMecanim != null)
                {
                    for (int i = 0; i < playerMecanim.layerCount; i++)
                    {
                        if (i != layer)
                        {
                            playerMecanim.SetLayerWeight(i, Mathf.MoveTowards(playerMecanim.GetLayerWeight(i), 0f, 5f * Time.deltaTime));
                        }
                        else
                        {
                            playerMecanim.SetLayerWeight(i, Mathf.MoveTowards(playerMecanim.GetLayerWeight(i), 1f, 5f * Time.deltaTime));
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @PlayerFootsteps

            private static void HookPlayerFootstepsStart(On.PlayerFootsteps.orig_Start orig, PlayerFootsteps playerFootsteps)
            {
                playerFootsteps.player = ((FootStepManager)playerFootsteps).GetComponentInParent<NewPlayerClass>();
                ((Action)Activator.CreateInstance(typeof(Action), playerFootsteps, typeof(FootStepManager).GetMethod("Start").MethodHandle.GetFunctionPointer()))(); // Prevent recursion.
                                                                                                                                                                     // ((FootStepManager)playerFootsteps).Start(); // This does not refer to base properly.
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @PlayerHeadCollider

            private static void HookPlayerHeadColliderStart(On.PlayerHeadCollider.orig_Start orig, PlayerHeadCollider playerHeadCollider)
            {
                orig.Invoke(playerHeadCollider);
                playerHeadCollider.newPlayerClass = ((MonoBehaviour)playerHeadCollider).GetComponentInParent<NewPlayerClass>();
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @PlayerHealth

            private static void HookPlayerHealthStart(On.PlayerHealth.orig_Start orig, PlayerHealth playerHealth)
            {
                orig.Invoke(playerHealth);
                playerHealth.NPC = playerHealth.GetComponentInParent<NewPlayerClass>();
                playerHealth.upperBodyLock = playerHealth.NPC.upperBodyLock;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @PlayerLayers2

            private static void HookPlayerLayers2OnWillRenderObject(On.PlayerLayers2.orig_OnWillRenderObject orig, PlayerLayers2 playerLayers2)
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    if (Camera.current == playerLayers2.clipCamera)
                    {
                        playerLayers2.leg1.transform.localScale = Vector3.zero;
                        playerLayers2.leg2.transform.localScale = Vector3.zero;
                    }
                    else
                    {
                        Inventory playerInventory = PlayerInventory(((MonoBehaviour)playerLayers2).GetComponentInParent<NewPlayerClass>());
                        if (playerInventory.CurrentItem != null && playerInventory.CurrentItem != playerLayers2.item)
                        {
                            playerLayers2.item = playerInventory.CurrentItem;
                            playerLayers2.itemRenderers = playerLayers2.item.GetComponentsInChildren<Renderer>(true);
                            if (playerLayers2.layers == null || playerLayers2.layers.Length != playerLayers2.itemRenderers.Length)
                            {
                                playerLayers2.layers = new int[playerLayers2.itemRenderers.Length];
                            }
                            for (int i = 0; i < playerLayers2.itemRenderers.Length; i++)
                            {
                                playerLayers2.layers[i] = playerLayers2.itemRenderers[i].gameObject.layer;
                            }
                        }
                        else if (playerInventory.CurrentItem == null)
                        {
                            playerLayers2.item = null;
                        }
                        if (playerLayers2.item != null)
                        {
                            for (int j = 0; j < playerLayers2.itemRenderers.Length; j++)
                            {
                                if (playerLayers2.itemRenderers[j] != null)
                                {
                                    playerLayers2.itemRenderers[j].gameObject.layer = playerLayers2.playerLayer;
                                }
                            }
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @PlayerMotor

            private static void HookPlayerMotorUpdate(On.PlayerMotor.orig_Update orig, PlayerMotor playerMotor)
            {
                if (LevelGeneration.Instance.finishedGenerating && (!ModSettings.enableCrewVSMonsterMode || (ModSettings.enableCrewVSMonsterMode && !monsterPlayers.Contains(playerMotor.player))))
                {
                    playerMotor.timeSinceLastJump += Time.deltaTime;
                    if (!playerMotor.disableMove)
                    {
                        bool flag = false;
                        float maxDistance = Mathf.Max(playerMotor.controller.height + 0.25f, 0.1f);
                        Vector3 origin = ((MonoBehaviour)playerMotor).transform.position + Vector3.up * playerMotor.controller.height;
                        if (Physics.Raycast(origin, Vector3.down, maxDistance, playerMotor.defaultMask))
                        {
                            flag = true;
                        }
                        if (playerMotor.controller.isGrounded)
                        {
                            flag = true;
                        }
                        /*
                        if (ModSettings.logDebugText)
                        {
                            Debug.Log("Is player number " + PlayerNumber(playerMotor.player.GetInstanceID()) + " using gravity? " + playerMotor.useGravity + ". Are they grounded? " + playerMotor.IsGrounded);
                        }
                        */
                        if (playerMotor.useGravity && !playerMotor.controller.isGrounded)
                        {
                            /*
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("Handling gravity");
                            }
                            */
                            playerMotor.yMovement += Physics.gravity.y * Time.deltaTime;
                        }
                        else if (playerMotor.yMovement < 0f || monsterPlayers.Contains(playerMotor.player))
                        {
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("Setting player movement to 0");
                            }
                            playerMotor.yMovement = 0f;
                        }
                        if (flag)
                        {
                            if (!playerMotor.lastGrounded && playerMotor.heightestPointInJump - ((MonoBehaviour)playerMotor).transform.position.y > ((!playerMotor.hasJumped) ? 2.5f : 0.025f))
                            {
                                AudioSource audioSource = AudioSystem.PlaySound("Noises/Footsteps/Jumplanding", playerMotor.player.transform, playerMotor.player.GetHeadSource);
                                if (audioSource != null)
                                {
                                    VolumeController component = audioSource.GetComponent<VolumeController>();
                                    if (component != null)
                                    {
                                        component.occlusion = 1f;
                                    }
                                }
                            }
                            if (playerMotor.timeSinceLastJump > 0.3f)
                            {
                                playerMotor.hasJumped = false;
                            }
                            playerMotor.lastGroundPosition = ((MonoBehaviour)playerMotor).transform.position;
                            playerMotor.heightestPointInJump = ((MonoBehaviour)playerMotor).transform.position.y;
                        }
                        else if (((MonoBehaviour)playerMotor).transform.position.y > playerMotor.heightestPointInJump)
                        {
                            playerMotor.heightestPointInJump = ((MonoBehaviour)playerMotor).transform.position.y;
                        }
                        if (playerMotor.player.PlayerFinishedStanding && ChooseAttack.GetAttack == ChooseAttack.AttackList.None)
                        {
                            playerMotor.controller.enabled = true;
                        }
                        if (playerMotor.allowFallDamage)
                        {
                            playerMotor.HandleFallDamage();
                        }
                        if (!playerMotor.IsUsingPhysics && playerMotor.player.PushBackDir == 0f && !playerMotor.player.StunnedByAttack)
                        {
                            playerMotor.rb.constraints = RigidbodyConstraints.FreezeAll;
                            playerMotor.ClampSpeed();
                            Vector3 a = ((MonoBehaviour)playerMotor).transform.forward * playerMotor.zMovement + ((MonoBehaviour)playerMotor).transform.right * playerMotor.xMovement;
                            a += ((MonoBehaviour)playerMotor).transform.up * playerMotor.yMovement;
                            if (playerMotor.disableTime <= 0f)
                            {
                                if (LevelGeneration.Instance.finishedGenerating && a.magnitude > 0f)
                                {
                                    playerMotor.Move(a * Time.deltaTime);
                                }
                            }
                            else
                            {
                                playerMotor.disableTime -= Time.deltaTime;
                            }
                            playerMotor.Reduce();
                            playerMotor.rb.constraints = RigidbodyConstraints.FreezeAll;
                        }
                        else
                        {
                            playerMotor.rb.constraints = (RigidbodyConstraints)80;
                            playerMotor.yMovement = 0f;
                        }
                        Vector3 playerCameraPosition = PlayerCamera(playerMotor.player).transform.position;
                        if (playerMotor.player.InProneTransition())
                        {

                            Vector3 b = playerMotor.player.transform.position - playerCameraPosition;
                            Vector3 a2 = playerMotor.startHeadPosition + b;
                            Vector3 vector = a2 - playerMotor.player.transform.position;
                            vector.y = 0f;
                            playerMotor.controller.Move(vector);
                            vector = playerMotor.player.transform.position;
                            vector.y = playerMotor.startPlayerPosition.y;
                            playerMotor.player.transform.position = vector;
                            playerMotor.disableTime = 0.1f;
                        }
                        else
                        {
                            playerMotor.startPlayerPosition = playerMotor.player.transform.position;
                            playerMotor.startHeadPosition = playerCameraPosition;
                        }
                        if (playerMotor.pose != playerMotor.lastPose)
                        {
                            if (playerMotor.lastPose == PlayerMotor.Pose.Prone)
                            {
                                playerMotor.UndoProne();
                            }
                            if (playerMotor.lastPose == PlayerMotor.Pose.Crouch)
                            {
                                playerMotor.UndoCrouch();
                            }
                            if (playerMotor.pose == PlayerMotor.Pose.Prone)
                            {
                                playerMotor.ChangedToProne();
                            }
                            if (playerMotor.pose == PlayerMotor.Pose.Crouch)
                            {
                                playerMotor.ChangedToCrouch();
                            }
                        }
                        playerMotor.ResetFrame();
                        playerMotor.lastGrounded = flag;
                    }
                    playerMotor.lastPose = playerMotor.pose;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @PlayerUpperBodyLock

            private static void HookPlayerUpperBodyLock()
            {
                On.PlayerUpperBodyLock.LateUpdate += new On.PlayerUpperBodyLock.hook_LateUpdate(HookPlayerUpperBodyLockLateUpdate);
                On.PlayerUpperBodyLock.UpdatePositions += new On.PlayerUpperBodyLock.hook_UpdatePositions(HookPlayerUpperBodyLockUpdatePositions);
            }

            private static void HookPlayerUpperBodyLockLateUpdate(On.PlayerUpperBodyLock.orig_LateUpdate orig, PlayerUpperBodyLock playerUpperBodyLock)
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    if (playerUpperBodyLock.once)
                    {
                        playerUpperBodyLock.once = false;
                        playerUpperBodyLock.UpdatePositions();
                    }
                    if (playerUpperBodyLock.weighting > 0f)
                    {
                        float t = Mathf.Clamp(Mathf.Min(playerUpperBodyLock.weighting, Mathf.Min(playerUpperBodyLock.weighting2, playerUpperBodyLock.weighting3)), 0f, playerUpperBodyLock.maxWeighting);
                        for (int i = 0; i < playerUpperBodyLock.keepRotation.Length; i++)
                        {
                            playerUpperBodyLock.keepRotations[i] = playerUpperBodyLock.keepRotation[i].rotation;
                        }
                        Quaternion b = Quaternion.identity;
                        for (int j = 0; j < playerUpperBodyLock.localFollowing.Length; j++)
                        {
                            if (playerUpperBodyLock.localFollowing[j] != null)
                            {
                                playerUpperBodyLock.localFollowing[j].localPosition = Vector3.Lerp(playerUpperBodyLock.localFollowing[j].localPosition, playerUpperBodyLock.lockLocalPositions[j], t);
                                if (playerUpperBodyLock.localFollowingRotations[j] != Vector3.zero)
                                {
                                    b = Quaternion.Euler(playerUpperBodyLock.localFollowingRotations[j]);
                                }
                                else
                                {
                                    b = playerUpperBodyLock.lockLocalRotation[j];
                                }
                                playerUpperBodyLock.localFollowing[j].localRotation = Quaternion.Lerp(playerUpperBodyLock.localFollowing[j].localRotation, b, t);
                            }
                        }
                        NewPlayerClass newPlayerClass = newPlayerClasses[PlayerNumberFromPlayerUpperBodyLock(playerUpperBodyLock.GetInstanceID())];
                        for (int k = 0; k < playerUpperBodyLock.localToPlayerFollowing.Length; k++)
                        {
                            if (playerUpperBodyLock.localFollowing[k] != null)
                            {
                                playerUpperBodyLock.temp = playerUpperBodyLock.localToPlayerFollowing[k].parent;
                                playerUpperBodyLock.localToPlayerFollowing[k].parent = newPlayerClass.gameObject.transform;
                                playerUpperBodyLock.localToPlayerFollowing[k].localPosition = Vector3.Lerp(playerUpperBodyLock.localToPlayerFollowing[k].localPosition, playerUpperBodyLock.lockGlobalPositions[k], t);
                                playerUpperBodyLock.localToPlayerFollowing[k].localRotation = Quaternion.Lerp(playerUpperBodyLock.localToPlayerFollowing[k].localRotation, playerUpperBodyLock.lockGlobalRotation[k], t);
                                playerUpperBodyLock.localToPlayerFollowing[k].parent = playerUpperBodyLock.temp;
                            }
                        }
                        for (int l = 0; l < playerUpperBodyLock.keepRotation.Length; l++)
                        {
                            playerUpperBodyLock.keepRotation[l].rotation = playerUpperBodyLock.keepRotations[l];
                        }
                    }
                }
            }

            private static void HookPlayerUpperBodyLockUpdatePositions(On.PlayerUpperBodyLock.orig_UpdatePositions orig, PlayerUpperBodyLock playerUpperBodyLock)
            {
                playerUpperBodyLock.lockLocalPositions = new Vector3[playerUpperBodyLock.localFollowing.Length];
                playerUpperBodyLock.lockLocalRotation = new Quaternion[playerUpperBodyLock.localFollowing.Length];
                playerUpperBodyLock.lockGlobalPositions = new Vector3[playerUpperBodyLock.localToPlayerFollowing.Length];
                playerUpperBodyLock.lockGlobalRotation = new Quaternion[playerUpperBodyLock.localToPlayerFollowing.Length];
                playerUpperBodyLock.keepRotations = new Quaternion[playerUpperBodyLock.keepRotation.Length];
                for (int i = 0; i < playerUpperBodyLock.localFollowing.Length; i++)
                {
                    if (playerUpperBodyLock.localFollowing[i] != null)
                    {
                        playerUpperBodyLock.lockLocalPositions[i] = playerUpperBodyLock.localFollowing[i].localPosition;
                        playerUpperBodyLock.lockLocalRotation[i] = playerUpperBodyLock.localFollowing[i].localRotation;
                    }
                }
                NewPlayerClass newPlayerClass = newPlayerClasses[PlayerNumberFromPlayerUpperBodyLock(playerUpperBodyLock.GetInstanceID())];
                for (int j = 0; j < playerUpperBodyLock.localToPlayerFollowing.Length; j++)
                {
                    if (playerUpperBodyLock.localToPlayerFollowing[j] != null)
                    {
                        playerUpperBodyLock.temp = playerUpperBodyLock.localToPlayerFollowing[j].parent;
                        playerUpperBodyLock.localToPlayerFollowing[j].parent = newPlayerClass.gameObject.transform;
                        playerUpperBodyLock.lockGlobalPositions[j] = playerUpperBodyLock.localToPlayerFollowing[j].localPosition;
                        playerUpperBodyLock.lockGlobalRotation[j] = playerUpperBodyLock.localToPlayerFollowing[j].localRotation;
                        playerUpperBodyLock.localToPlayerFollowing[j].parent = playerUpperBodyLock.temp;
                    }
                }
            }

            private static int PlayerNumberFromPlayerUpperBodyLock(int passedPlayerUpperBodyLockInstanceID)
            {
                int playerNumber = 0;
                if (playerUpperBodyLocks != null)
                {
                    for (int i = 1; i < playerUpperBodyLocks.Length; i++)
                    {
                        if (playerUpperBodyLocks[i].GetInstanceID() == passedPlayerUpperBodyLockInstanceID)
                        {
                            playerNumber = i;
                        }
                    }
                }
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Player upper body lock player number is " + playerNumber);
                }
                return playerNumber;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @QualitySettingsScript

            private static void HookQualitySettingsScriptSetPlayerEffects(On.QualitySettingsScript.orig_SetPlayerEffects orig, QualitySettingsScript qualitySettingsScript)
            {
                orig.Invoke(qualitySettingsScript);
                if (SceneManager.GetActiveScene().name != "Splash" && newPlayerClasses != null && LevelGeneration.Instance.finishedGenerating)
                {
                    //Debug.Log("Running UpdatePlayerEffects");
                    UpdatePlayerEffects();
                }
            }

            private static void UpdatePlayerEffects()
            {
                bool useBloom = PlayerPrefsBools.GetBool("BloomAndLensFlare", true);
                bool useSSAO = PlayerPrefsBools.GetBool("NewSSAO", true);
                bool useFog = PlayerPrefsBools.GetBool("GlobalFog", true);

                for (int i = 1; i < newPlayerClasses.Count; i++)
                {
                    //Debug.Log("Running UpdatePlayerEffects for player number " + i);
                    (PlayerCamera(newPlayerClasses[i]).transform.GetComponent("BloomAndLensFlares") as MonoBehaviour).enabled = useBloom;
                    (PlayerCamera(newPlayerClasses[i]).transform.GetComponent("NewSSAO") as MonoBehaviour).enabled = useSSAO;
                    (PlayerCamera(newPlayerClasses[i]).transform.GetComponent("GlobalFog") as MonoBehaviour).enabled = useFog;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Radio

            private static void HookRadioDestroyRadio(On.Radio.orig_DestroyRadio orig, Radio radio)
            {
                radio.destroyed = true;
                DestroyableObject destroyableObject = ((MonoBehaviour)radio).transform.GetComponent<DestroyableObject>();
                Inventory inventory = InventoryFromItemClass(radio.gameObject);
                if (inventory.CurrentItem.gameObject == ((MonoBehaviour)radio).gameObject)
                {
                    inventory.DropItem(inventory.CurrentSlot);
                }
                if (destroyableObject != null)
                {
                    destroyableObject.DestroyObject();
                }
            }

            private static void HookRadioTurnRadioOff(On.Radio.orig_TurnRadioOff orig, Radio radio)
            {
                if (radio.RadioSource != null)
                {
                    radio.RadioSource.Stop();
                    VirtualAudioSource virtualAudioSource = radio.RadioSource.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null 1!\n" + new StackTrace().ToString());
                    }
                }
                if (radio.StaticSource != null)
                {
                    radio.StaticSource.Stop();
                    VirtualAudioSource virtualAudioSource = radio.StaticSource.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null 2!\n" + new StackTrace().ToString());
                    }
                }
                radio.RestartOcclusions();
                radio.radioOn = false;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @RealtimeOcclusion

            private static void HookRealtimeOcclusion()
            {
                // Not sure whether this is still being used by the game.
                On.RealtimeOcclusion.Find2 += new On.RealtimeOcclusion.hook_Find2(HookRealtimeOcclusionFind2);
                On.RealtimeOcclusion.TestPoint += new On.RealtimeOcclusion.hook_TestPoint(HookRealtimeOcclusionTestPoint);
            }

            private static void HookRealtimeOcclusionFind2(On.RealtimeOcclusion.orig_Find2 orig, RealtimeOcclusion realtimeOcclusion)
            {
                for (int playerNumber = 0; playerNumber < newPlayerClasses.Count; playerNumber++)
                {
                    HashSet<CullBox> hashSet = null;
                    HashSet<CullBox> visibleBoxes = CullBox.GetVisibleBoxes();
                    if (!Input.GetKey(KeyCode.L))
                    {
                        hashSet = CullBox.GetPerimeterBoxes();
                    }
                    else
                    {
                        hashSet = CullBox.GetVisibleBoxes();
                    }
                    realtimeOcclusion.totalRaycasts = 0;
                    realtimeOcclusion.totalBoxes = hashSet.Count;
                    if (realtimeOcclusion.totalBoxes < realtimeOcclusion.methodThreshold)
                    {
                        realtimeOcclusion.needsUpdating = true;
                        int num = 10;
                        while (realtimeOcclusion.needsUpdating)
                        {
                            realtimeOcclusion.needsUpdating = false;
                            realtimeOcclusion.boxes.Clear();
                            foreach (CullBox item in visibleBoxes)
                            {
                                realtimeOcclusion.boxes.Add(item);
                            }
                            for (int i = 0; i < realtimeOcclusion.boxes.Count; i++)
                            {
                                realtimeOcclusion.UpDownChecker(realtimeOcclusion.boxes[i]);
                            }
                            realtimeOcclusion.boxes.Clear();
                            foreach (CullBox item2 in hashSet)
                            {
                                realtimeOcclusion.boxes.Add(item2);
                            }
                            foreach (CullBox box in realtimeOcclusion.boxes)
                            {
                                realtimeOcclusion.TestCullBox(box);
                            }
                            if (realtimeOcclusion.needsUpdating)
                            {
                                RealtimeOcclusion.Occlusion.forceUpdate = true;
                            }
                            num--;
                            if (num < 0)
                            {
                                Debug.Log("Timeout");
                                break;
                            }
                        }
                        realtimeOcclusion.testedPoint.Clear();
                    }
                    else
                    {
                        float screenWidth = (float)Screen.width;
                        float screenHeight = (float)Screen.height;
                        float raycastsX = (float)realtimeOcclusion.raycastsX;
                        float raycastsY = (float)realtimeOcclusion.raycastsY;
                        Camera camera;
                        if (!ModSettings.enableCrewVSMonsterMode || !ModSettings.numbersOfMonsterPlayers.Contains(playerNumber))
                        {
                            camera = PlayerCamera(newPlayerClasses[playerNumber]);
                        }
                        else
                        {
                            camera = PlayerCamera(newPlayerClasses[playerNumber]);
                            //camera = ManyMonstersMode.monsterList[PlayerMonsterNumberFromPlayerNumber(playerNumber)].GetComponentInChildren<Camera>();
                        }
                        Vector3 zero = Vector3.zero;
                        zero.z = 0.001f;
                        Ray ray = default(Ray);
                        realtimeOcclusion.offsetX += 2f;
                        if (realtimeOcclusion.offsetX >= screenWidth / raycastsX)
                        {
                            realtimeOcclusion.offsetX = 0f;
                            realtimeOcclusion.offsetY += 2f;
                            if (realtimeOcclusion.offsetY >= screenHeight / raycastsY)
                            {
                                realtimeOcclusion.offsetY = 0f;
                            }
                        }
                        for (float num6 = realtimeOcclusion.offsetY; num6 < screenHeight; num6 += screenHeight / raycastsY)
                        {
                            for (float num7 = realtimeOcclusion.offsetX; num7 < screenWidth; num7 += screenWidth / raycastsX)
                            {
                                zero.x = num7;
                                zero.y = num6;
                                ray = camera.ScreenPointToRay(zero);
                                realtimeOcclusion.RaycastStandard(ray.origin, ray.direction);
                            }
                        }
                    }
                }
            }

            private static void HookRealtimeOcclusionTestPoint(On.RealtimeOcclusion.orig_TestPoint orig, RealtimeOcclusion realtimeOcclusion, CullBox _box, Vector3 _position)
            {
                for (int playerNumber = 0; playerNumber < newPlayerClasses.Count; playerNumber++)
                {
                    if (!realtimeOcclusion.testedPoint.Contains(MathHelper.RoundToDP(_position, 1)))
                    {
                        realtimeOcclusion.testedPoint.Add(MathHelper.RoundToDP(_position, 1));
                        Camera playerCamera;
                        if (!ModSettings.enableCrewVSMonsterMode || !ModSettings.numbersOfMonsterPlayers.Contains(playerNumber))
                        {
                            playerCamera = PlayerCamera(newPlayerClasses[playerNumber]);
                        }
                        else
                        {
                            playerCamera = PlayerCamera(newPlayerClasses[playerNumber]);
                            //playerCamera = ManyMonstersMode.monsterList[PlayerMonsterNumberFromPlayerNumber(playerNumber)].GetComponentInChildren<Camera>();
                        }
                        Vector3 position = playerCamera.transform.position;
                        Vector3 vector = _position - position;
                        Vector3 normalized = vector.normalized;
                        Vector3 forward = playerCamera.transform.forward;
                        if (Vector3.Angle(forward, normalized) < playerCamera.fieldOfView * realtimeOcclusion.fieldOfViewMultiplier)
                        {
                            realtimeOcclusion.totalRaycasts++;
                            RaycastHit raycastHit;
                            if (Physics.Raycast(position, normalized, out raycastHit, 100000f, realtimeOcclusion.cullboxTestMaskAll))
                            {
                                if (realtimeOcclusion.debug)
                                {
                                    Debug.DrawLine(position, raycastHit.point, Color.yellow, 0.2f);
                                    _box.DrawCullBox(Color.green, 5f);
                                }
                                RealtimeOcclusion.Occlusion.AddPointToCurrentPos(raycastHit.point, false);
                                if (vector.magnitude <= Vector3.Distance(position, raycastHit.point))
                                {
                                    RealtimeOcclusion.Occlusion.AddPointToCurrentPos(_box.center, false);
                                }
                                realtimeOcclusion.needsUpdating = true;
                            }
                            else if (realtimeOcclusion.debug)
                            {
                                Debug.DrawLine(position, raycastHit.point, Color.magenta, 0.1f);
                            }
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @RealtimeOcclusion2

            private static List<VisibilityData> Find(Vector3 _point, List<VisibilityData> _seenData, RealtimeOcclusion2 realtimeOcclusion2, NewPlayerClass newPlayerClass)
            {
                realtimeOcclusion2.cullCam = newPlayerClass.cullingCam;

                realtimeOcclusion2.searchID++;
                realtimeOcclusion2.seenData = _seenData;
                realtimeOcclusion2.saveCamData = RealtimeOcclusion2.Occlusion.CamData;
                realtimeOcclusion2.startData = RealtimeOcclusion2.Occlusion.GetData(MathHelper.RoundToNearest(_point, Settings.CuboidDim));
                realtimeOcclusion2.startData.reasonRays.Clear();
                realtimeOcclusion2.startData.searchID = realtimeOcclusion2.searchID;
                realtimeOcclusion2.startPos = realtimeOcclusion2.startData.position;
                RealtimeOcclusion2.sqrFarClip = realtimeOcclusion2.playerCamera.farClipPlane * realtimeOcclusion2.playerCamera.farClipPlane;
                realtimeOcclusion2.expands = 0;

                Transform cameraTransform = PlayerCamera(newPlayerClass).transform;

                realtimeOcclusion2.cullCam.transform.position = cameraTransform.position - cameraTransform.forward * realtimeOcclusion2.cullCamDistance;
                realtimeOcclusion2.cullCam.transform.rotation = cameraTransform.rotation;
                realtimeOcclusion2.defaultID = VisibilityData.RoundToID(realtimeOcclusion2.cullCam.transform.forward);
                if (realtimeOcclusion2.startData != null)
                {
                    realtimeOcclusion2.seenData.Add(realtimeOcclusion2.startData);
                    for (int i = 0; i < 6; i++)
                    {
                        VisibilityData nodeSide = realtimeOcclusion2.startData.GetNodeSide(i);
                        if (nodeSide != null)
                        {
                            realtimeOcclusion2.seenData.Add(nodeSide);
                        }
                    }
                    RealtimeOcclusion2.Occlusion.CamData = realtimeOcclusion2.startData;
                    realtimeOcclusion2.expandList.Clear();
                    realtimeOcclusion2.expandList.Add(realtimeOcclusion2.startData);
                    realtimeOcclusion2.startData.cameFrom = null;
                    realtimeOcclusion2.startData.cameFromID = -1;
                    realtimeOcclusion2.startData.highestLinch = 0;
                    realtimeOcclusion2.startData.isLinch = true;
                    realtimeOcclusion2.startData.linchDistance = 0;
                    realtimeOcclusion2.startData.sideID = -1;
                    for (int j = 0; j < 6; j++)
                    {
                        realtimeOcclusion2.startData.allowedDirections[j] = true;
                    }
                    realtimeOcclusion2.escape = false;
                    realtimeOcclusion2.total = 0;
                    while (realtimeOcclusion2.expandList.Count > realtimeOcclusion2.expands && !realtimeOcclusion2.escape)
                    {
                        realtimeOcclusion2.total++;
                        realtimeOcclusion2.Expand();
                    }
                }
                RealtimeOcclusion2.Occlusion.CamData = realtimeOcclusion2.saveCamData;
                return realtimeOcclusion2.seenData;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ReverbController

            private static void HookReverbController()
            {
                // This fixes some null references.
                //On.ReverbController.Awake += new On.ReverbController.hook_Awake(HookReverbControllerAwake);
                On.ReverbController.Start += new On.ReverbController.hook_Start(HookReverbControllerStart);
                On.ReverbController.LateUpdate += new On.ReverbController.hook_LateUpdate(HookReverbControllerLateUpdate);
            }

            /*
            private static void HookReverbControllerAwake(On.ReverbController.orig_Awake orig, ReverbController reverbController)
            {
                ((MonoBehaviour)reverbController).StartCoroutine(WaitUntilGenerationIsFinishedReverbController(reverbController));
            }

            private static IEnumerator WaitUntilGenerationIsFinishedReverbController(ReverbController reverbController)
            {
                while (!LevelGeneration.Instance.finishedGenerating)
                {
                    yield return null;
                }
                NewPlayerClass newPlayerClass = ((MonoBehaviour)reverbController).GetComponent<NewPlayerClass>();
                if (newPlayerClass != null)
                {
                    Debug.Log("ReverbController NPC is not null");
                    int playerNumber = PlayerNumber(newPlayerClass.GetInstanceID());
                    reverbContainers[playerNumber] = new GameObject("Reverbs");
                    reverbContainers[playerNumber].transform.parent = playerAudioListeners[playerNumber].transform;
                    reverbContainers[playerNumber].transform.localPosition = Vector3.zero;
                    reverbContainers[playerNumber].transform.localScale = Vector3.one;
                    reverbContainers[playerNumber].transform.localRotation = Quaternion.identity;
                    reverbController.zone = (((MonoBehaviour)reverbController).gameObject.AddComponent(typeof(AudioReverbZone)) as AudioReverbZone);
                    ((MonoBehaviour)reverbController).transform.parent = reverbContainers[playerNumber].transform;
                    if (((MonoBehaviour)reverbController).GetComponent<LockerReverbFinder>() == null)
                    {
                        reverbController.reverbAmount = 0f;
                    }
                }
                else
                {
                    Debug.Log("ReverbController NPC is null");
                }
                yield break;
            }
            */

            private static void HookReverbControllerStart(On.ReverbController.orig_Start orig, ReverbController reverbController)
            {
                if (reverbController.zone != null)
                {
                    orig.Invoke(reverbController);
                }
            }

            private static void HookReverbControllerLateUpdate(On.ReverbController.orig_LateUpdate orig, ReverbController reverbController)
            {
                if (reverbController.zone != null)
                {
                    orig.Invoke(reverbController);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ReverbZone

            private static float HookReverbZoneCalculateReverbAmount(On.ReverbZone.orig_CalculateReverbAmount orig, ReverbZone reverbZone)
            {
                int playerNumber = ClosestPlayerToThis(reverbZone.room.transform.position);
                reverbZone.controller.transform.parent = newPlayerClasses[playerNumber].transform; // Part of new audio system.
                reverbZone.reverbAmount = 0f;
                Camera playerCamera;
                if (!ModSettings.enableCrewVSMonsterMode || !ModSettings.numbersOfMonsterPlayers.Contains(playerNumber))
                {
                    playerCamera = PlayerCamera(newPlayerClasses[playerNumber]);
                }
                else
                {
                    playerCamera = PlayerCamera(newPlayerClasses[playerNumber]);
                    //playerCamera = ManyMonstersMode.monsterList[PlayerMonsterNumberFromPlayerNumber(playerNumber)].GetComponentInChildren<Camera>();
                }
                Vector3 closestNodeData = GetClosestNodeData(reverbZone, playerCamera);
                Vector3 position = playerCamera.transform.position;
                Vector3 vector = position - closestNodeData;
                vector.x = Mathf.Abs(vector.x);
                vector.y = Mathf.Abs(vector.y);
                vector.z = Mathf.Abs(vector.z);
                if (vector.x < Settings.CuboidDim.x * 0.5f && vector.y < Settings.CuboidDim.y * 0.5f && vector.z < Settings.CuboidDim.z * 0.5f)
                {
                    return 1f * reverbZone.controller.multiplyer;
                }
                Vector3 b = MathHelper.ClosestPointToBox(position, closestNodeData, Settings.CuboidDim);
                Vector3 vector2 = position - b;
                if (vector2.magnitude < reverbZone.range)
                {
                    return (1f - vector2.magnitude / reverbZone.range) * reverbZone.controller.multiplyer;
                }
                return 0f;
            }

            private static Vector3 GetClosestNodeData(ReverbZone reverbZone, Camera playerCamera)
            {
                if (reverbZone.room.RoomNodes.Count > 0)
                {
                    Vector3 position = playerCamera.transform.position;
                    Vector3 vector = RegionManager.Instance.ConvertRegionNodeToShipWorldSpace(reverbZone.room.RoomNodes[0]) + Settings.CuboidDim * 0.5f;
                    float num = Vector3.Distance(vector, position);
                    for (int i = 1; i < reverbZone.room.RoomNodes.Count; i++)
                    {
                        Vector3 vector2 = RegionManager.Instance.ConvertRegionNodeToShipWorldSpace(reverbZone.room.RoomNodes[i]) + Settings.CuboidDim * 0.5f;
                        float num2 = Vector3.Distance(vector2, position);
                        if (num2 < num)
                        {
                            num = num2;
                            vector = vector2;
                        }
                    }
                    return vector;
                }
                return Vector3.zero;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @RightHandIK

            private static void HookRightHandIKStart(On.RightHandIK.orig_Start orig, RightHandIK rightHandIK)
            {
                rightHandIK.target = Vector3.zero;
                rightHandIK.animator = ((MonoBehaviour)rightHandIK).GetComponent<Animator>();
                rightHandIK.playerClass = ((MonoBehaviour)rightHandIK).GetComponent<NewPlayerClass>();
                Debug.Log("New player class in RightHandIK is " + PlayerNumber(rightHandIK.playerClass.GetInstanceID()));
            }

            private static void HookRightHandIKOnAnimatorIK(On.RightHandIK.orig_OnAnimatorIK orig, RightHandIK rightHandIK)
            {
                if (rightHandIK.animator)
                {
                    if (rightHandIK.dummyTransform != null)
                    {
                        Inventory inventory = PlayerInventory(rightHandIK.playerClass);
                        if ((rightHandIK.playerClass.playerState == NewPlayerClass.PlayerState.Prone && (inventory.CurrentItem == null || !inventory.CurrentItem.useItemProne)) || rightHandIK.playerClass.fixedAnimationPlaying)
                        {
                            rightHandIK.ReleaseHand();
                        }
                        else if (rightHandIK.playerClass.playItemAnimation)
                        {
                            rightHandIK.ReleaseHandOnly();
                        }
                        else
                        {
                            rightHandIK.LockHand();
                        }
                    }
                    else
                    {
                        rightHandIK.ReleaseHand();
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @RoomReverbManager

            private static void HookRoomReverbManager()
            {
                On.RoomReverbManager.Update += new On.RoomReverbManager.hook_Update(HookRoomReverbManagerUpdate);
            }

            private static void HookRoomReverbManagerUpdate(On.RoomReverbManager.orig_Update orig, RoomReverbManager roomReverbManager)
            {
                if (LevelGeneration.Instance != null && LevelGeneration.Instance.finishedGenerating)
                {
                    for (int playerNumber = 0; playerNumber < newPlayerClasses.Count; playerNumber++)
                    {
                        Camera playerCamera;
                        if (!ModSettings.enableCrewVSMonsterMode || !ModSettings.numbersOfMonsterPlayers.Contains(playerNumber))
                        {
                            playerCamera = PlayerCamera(newPlayerClasses[playerNumber]);
                        }
                        else
                        {
                            playerCamera = PlayerCamera(newPlayerClasses[playerNumber]);
                            //playerCamera = ManyMonstersMode.monsterList[PlayerMonsterNumberFromPlayerNumber(playerNumber)].GetComponentInChildren<Camera>();
                        }
                        Vector3 lhs = MathHelper.RoundToNearest(playerCamera.transform.position, Settings.CuboidDim);
                        if (lhs != currentCamNodes[playerNumber])
                        {
                            if (playerNumber == 0)
                            {
                                for (int j = 0; j < roomReverbManager.currentZones.Count; j++)
                                {
                                    roomReverbManager.currentZones[j].enabled = false;
                                }
                                roomReverbManager.currentZones.Clear();
                            }
                            Vector3 cuboidMinus = MathHelper.RoundToNearest(playerCamera.transform.position - Settings.CuboidDim, Settings.CuboidDim);
                            Vector3 cuboidPlus = MathHelper.RoundToNearest(playerCamera.transform.position + Settings.CuboidDim, Settings.CuboidDim);
                            Vector3 world = cuboidMinus;
                            world.x = cuboidMinus.x;
                            while (world.x <= cuboidPlus.x)
                            {
                                world.y = cuboidMinus.y;
                                while (world.y <= cuboidPlus.y)
                                {
                                    world.z = cuboidMinus.z;
                                    while (world.z <= cuboidPlus.z)
                                    {
                                        NodeData nodeDataAtPosition = LevelGeneration.GetNodeDataAtPosition(world);
                                        if (nodeDataAtPosition != null && nodeDataAtPosition.nodeRoom != null)
                                        {
                                            ReverbZone reverbZone = nodeDataAtPosition.nodeRoom.GetComponent<ReverbZone>();
                                            if (reverbZone != null)
                                            {
                                                roomReverbManager.currentZones.Add(reverbZone);
                                            }
                                        }
                                        world.z += Settings.CuboidDim.z;
                                    }
                                    world.y += Settings.CuboidDim.y;
                                }
                                world.x += Settings.CuboidDim.x;
                            }
                            UpdateCorridorReverb(roomReverbManager, playerCamera, newPlayerClasses.Count);
                        }
                        currentCamNodes[playerNumber] = lhs;
                    }
                    for (int j = 0; j < roomReverbManager.currentZones.Count; j++)
                    {
                        roomReverbManager.currentZones[j].enabled = true;
                    }
                }
            }

            private static void UpdateCorridorReverb(RoomReverbManager roomReverbManager, Camera playerCamera, int numberOfPlayers)
            {
                roomReverbManager.corridorStrength += Time.deltaTime / numberOfPlayers;
                NodeData nodeDataAtPosition = LevelGeneration.GetNodeDataAtPosition(playerCamera.transform.position);
                if (nodeDataAtPosition != null && nodeDataAtPosition.nodeRoom != null && nodeDataAtPosition.nodeRoom.RoomType == RoomStructure.Corridor)
                {
                    roomReverbManager.GetCorridorReverb(nodeDataAtPosition.nodeRoom, roomReverbManager.minReverbs[RoomReverb.ReverbType.Corridor], roomReverbManager.maxReverbs[RoomReverb.ReverbType.Corridor], roomReverbManager.reverbControllers[RoomReverb.ReverbType.Corridor].AudioZone);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @RopeDragRelease

            public static bool HookRopeDragReleaseTestIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!ManyMonstersMode.IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookRopeDragReleaseTest((RopeDragRelease)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    ManyMonstersMode.IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookRopeDragReleaseTest(RopeDragRelease ropeDragRelease)
            {
                Debug.Log("This hook is working!: " + new StackTrace().ToString()); // # IEnumerator Hook Test
                Debug.Log("RopeDragRelease is being used");
                PlayerMotor lastPlayerSentMessageMotor = lastPlayerSentMessage.Motor;
                while (ropeDragRelease.handGrab.IsInteracting)
                {
                    if (Vector3.Distance(ropeDragRelease.start.position, ropeDragRelease.end.position) > ropeDragRelease.rope.TotalRopeLength + ropeDragRelease.tolerance)
                    {
                        ropeDragRelease.handGrab.ShouldRelease = true;
                    }
                    if (!Physics.Raycast(lastPlayerSentMessageMotor.transform.position + Vector3.up * 0.25f, Vector3.down, 0.5f, ropeDragRelease.mask))
                    {
                        ropeDragRelease.handGrab.ShouldRelease = true;
                    }
                    yield return null;
                }
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @RopeLengthCondition

            private static bool HookRopeLengthConditionIsConditionMet(On.RopeLengthCondition.orig_IsConditionMet orig, RopeLengthCondition ropeLengthCondition)
            {
                Vector3 dragPointAccountingForPlayer = DragPoint(ropeLengthCondition.drag, lastPlayerCheckingInteractableConditions);
                // DraggableObject.Drag.transform.position = lastPlayerCheckingInteractableConditions.transform.position; // Drag is not updated properly.
                bool condition = Vector3.Distance(/*ropeLengthCondition.drag.DragPoint*/ dragPointAccountingForPlayer, ropeLengthCondition.ropeStart.transform.position) <= ropeLengthCondition.rope.TotalRopeLength;

                if (ModSettings.logDebugText)
                {
                    //Debug.Log("Rope drag point is at " + ropeLengthCondition.drag.DragPoint + ", RopeStart is at " + ropeLengthCondition.ropeStart.transform.position + ". The distance between these is " + Vector3.Distance(ropeLengthCondition.drag.DragPoint, ropeLengthCondition.ropeStart.transform.position) + ". TotalRopeLength is " + ropeLengthCondition.rope.TotalRopeLength + ". Drag is at " + DraggableObject.Drag.transform.position + ". Offset is " + ropeLengthCondition.drag.offset);
                    Debug.Log("Rope drag point accounting for player is at " + dragPointAccountingForPlayer + ", RopeStart is at " + ropeLengthCondition.ropeStart.transform.position + ". The distance between these is " + Vector3.Distance(dragPointAccountingForPlayer, ropeLengthCondition.ropeStart.transform.position) + ". TotalRopeLength is " + ropeLengthCondition.rope.TotalRopeLength + ". Drag is at " + DraggableObject.Drag.transform.position + ". Offset is " + ropeLengthCondition.drag.offset);
                    foreach (NewPlayerClass newPlayerClass in newPlayerClasses)
                    {
                        Debug.Log("Player is at " + newPlayerClass.transform.position);
                    }

                    Debug.Log("Checking RopeLengthCondition. It is " + condition.ToString() + " for player number " + PlayerNumber(lastPlayerCheckingInteractableConditions.GetInstanceID()));
                }
                return condition;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SearchState

            private static void HookSearchStateUpdate(On.SearchState.orig_Update orig, SearchState searchState)
            {
                foreach (NewPlayerClass newPlayerClass in crewPlayers)
                {
                    Camera playerCamera = PlayerCamera(newPlayerClass);
                    if (((CState)searchState).gameObject.layer != searchState.cullLayer && GeoHelper.InsideCone(searchState.cam.transform.position, playerCamera.transform.position, searchState.cam.transform.forward, searchState.coneAngle, searchState.coneLength) && Physics.Raycast(searchState.cam.transform.position, (playerCamera.transform.position - searchState.cam.transform.position).normalized, out searchState.raycastHit, searchState.coneLength, searchState.playerLayer))
                    {
                        if (searchState.raycastHit.collider.gameObject.layer == 9)
                        {
                            ((CState)searchState).SendEvent("FoundPlayer");
                        }
                        else
                        {
                            ((CState)searchState).SendEvent("LostPlayer");
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SecurityCamera

            private static void HookSecurityCameraDisableAudio(On.SecurityCamera.orig_DisableAudio orig, SecurityCamera securityCamera)
            {
                securityCamera.currentString = " ";
                securityCamera.audioDisabled = true;
                if (securityCamera.audiosources != null)
                {
                    securityCamera.audiosources.Stop();
                    VirtualAudioSource virtualAudioSource = securityCamera.audiosources.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                }
            }

            private static void HookSecurityCameraMovementSound(On.SecurityCamera.orig_MovementSound orig, SecurityCamera securityCamera)
            {
                securityCamera.neededString = "Noises/Enviro/Alarm/SecurityCam/Movement";
                if (securityCamera.currentString != securityCamera.neededString)
                {
                    securityCamera.fadeValue = Mathf.MoveTowards(securityCamera.fadeValue, 0f, 1f * Time.deltaTime);
                    if (securityCamera.audiosources.volume <= 0.01f)
                    {
                        securityCamera.audiosources.Stop();
                        VirtualAudioSource virtualAudioSource = securityCamera.audiosources.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource != null)
                        {
                            virtualAudioSource.Stop();
                        }
                        else if (ModSettings.logDebugText)
                        {
                            Debug.Log("VAS is null 1!\n" + new StackTrace().ToString());
                        }
                        securityCamera.fadeValue = 1f;
                        securityCamera.currentString = securityCamera.neededString;
                    }
                }
                if (!securityCamera.simpleTilt.IsDelayed)
                {
                    if (!securityCamera.audiosources.isPlaying && securityCamera.currentString == securityCamera.neededString && securityCamera.simpleTilt.render.enabled)
                    {
                        AudioSystem.PlaySound(securityCamera.currentString, securityCamera.audiosources);
                    }
                }
                else
                {
                    securityCamera.audiosources.Stop();
                    VirtualAudioSource virtualAudioSource = securityCamera.audiosources.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null 2!\n" + new StackTrace().ToString());
                    }
                }
            }

            private static void HookSecurityCameraPlayWarning(On.SecurityCamera.orig_PlayWarning orig, SecurityCamera securityCamera)
            {
                securityCamera.neededString = "Noises/Enviro/Alarm/SecurityCam/PowerUp";
                securityCamera.audiosources.loop = false;
                if (securityCamera.audiosources != null && !securityCamera.audioDisabled)
                {
                    securityCamera.audiosources.Stop();
                    VirtualAudioSource virtualAudioSource = securityCamera.audiosources.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                    securityCamera.currentString = securityCamera.neededString;
                    AudioSystem.PlaySound(securityCamera.currentString, securityCamera.audiosources);
                }
            }

            private static void HookSecurityCameraSetToGreen(On.SecurityCamera.orig_SetToGreen orig, SecurityCamera securityCamera)
            {
                if (securityCamera.ledRend.sharedMaterial == securityCamera.redLight)
                {
                    securityCamera.audiosources.Stop();
                    VirtualAudioSource virtualAudioSource = securityCamera.audiosources.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                }
                securityCamera.ledRend.sharedMaterial = securityCamera.greenLight;
            }

            private static void HookSecurityCameraSoundTheAlarm(On.SecurityCamera.orig_SoundTheAlarm orig, SecurityCamera securityCamera)
            {
                securityCamera.neededString = "Noises/Enviro/Alarm/SecurityCam/Alarmed";
                if (securityCamera.audiosources != null && !securityCamera.audioDisabled && securityCamera.neededString != securityCamera.currentString)
                {
                    securityCamera.audiosources.Stop();
                    VirtualAudioSource virtualAudioSource = securityCamera.audiosources.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                    AudioSystem.PlaySound(securityCamera.neededString, securityCamera.audiosources);
                    securityCamera.currentString = securityCamera.neededString;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SecurityMonitors

            private static void HookSecurityMonitorsOnPowerDown(On.SecurityMonitors.orig_OnPowerDown orig, SecurityMonitors securityMonitors)
            {
                orig.Invoke(securityMonitors);
                if (securityMonitors.monitorSource != null)
                {
                    VirtualAudioSource virtualAudioSource = securityMonitors.monitorSource.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ShoulderOverride

            private static void HookShoulderOverrideAwake(On.ShoulderOverride.orig_Awake orig, ShoulderOverride shoulderOverride)
            {
                bool foundLink = false;
                if (shoulderOverride.gameObject.GetComponentInParent<NewPlayerClass>() != null)
                {
                    Debug.Log("Link found in parent of " + shoulderOverride.gameObject.name + " for type ShoulderOverride");
                    foundLink = true;
                }
                if (shoulderOverride.gameObject.GetComponent<NewPlayerClass>() != null)
                {
                    Debug.Log("Link found on same level of " + shoulderOverride.gameObject.name + " for type ShoulderOverride");
                    foundLink = true;
                }
                if (shoulderOverride.gameObject.GetComponentInChildren<NewPlayerClass>() != null)
                {
                    Debug.Log("Link found in children of " + shoulderOverride.gameObject.name + " for type ShoulderOverride");
                    foundLink = true;
                }
                if (!foundLink)
                {
                    Debug.Log("No link found for " + shoulderOverride.gameObject.name + " for type ShoulderOverride");
                }
                /*
                shoulderOverride.player = ((MonoBehaviour)shoulderOverride).GetComponentInParent<NewPlayerClass>();
                Debug.Log("New player class in ShoulderOverride is " + PlayerNumber(shoulderOverride.player.GetInstanceID()));
                */

                /*
                Transform[] allTransforms = Resources.FindObjectsOfTypeAll<Transform>();
                Debug.Log("Listing player transforms.");
                foreach (Transform transform in allTransforms)
                {
                    Debug.Log(transform.name);
                }
                Debug.Log("Done listing player transforms.");
                */
            }

            private static void HookShoulderOverrideLateUpdate(On.ShoulderOverride.orig_LateUpdate orig, ShoulderOverride shoulderOverride)
            {
                if (shoulderOverride.player == null)
                {
                    shoulderOverride.player = ((MonoBehaviour)shoulderOverride).GetComponentInParent<NewPlayerClass>();
                    Debug.Log("New player class in late ShoulderOverride is " + PlayerNumber(shoulderOverride.player.GetInstanceID()));
                }
                else
                {
                    //Debug.Log("New player class in ShoulderOverride not declaration is " + PlayerNumber(shoulderOverride.player.GetInstanceID()));
                }

                if (LevelGeneration.Instance.finishedGenerating)
                {
                    Inventory inventory = PlayerInventory(shoulderOverride.player);
                    if (inventory.CurrentItem != null && inventory.CurrentItem.gameObject.activeSelf)
                    {
                        if (shoulderOverride.player.IsCrouched())
                        {
                            shoulderOverride.weighting += Time.deltaTime * 1.3f;
                        }
                        else
                        {
                            shoulderOverride.weighting -= Time.deltaTime * 3f;
                        }
                    }
                    else
                    {
                        shoulderOverride.weighting -= Time.deltaTime * 3f;
                    }
                    shoulderOverride.weighting = Mathf.Clamp01(shoulderOverride.weighting);
                    shoulderOverride.shoulderBone.rotation = Quaternion.Slerp(shoulderOverride.shoulderBone.rotation, shoulderOverride.player.transform.rotation * shoulderOverride.targetRotation.localRotation, shoulderOverride.weighting);
                    shoulderOverride.shoulderBone.position += Vector3.down * shoulderOverride.weighting * shoulderOverride.distanceDownWhenCrouched;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SimpleOcclusion

            private static void HookSimpleOcclusion() // Try making it raycast from the relevant position, but keep one audio listener.
            {
                //On.SimpleOcclusion.Start += new On.SimpleOcclusion.hook_Start(HookSimpleOcclusionStart);

                On.SimpleOcclusion.Update += new On.SimpleOcclusion.hook_Update(HookSimpleOcclusionUpdate);
                //On.SimpleOcclusion.RaycastOcclusion += new On.SimpleOcclusion.hook_RaycastOcclusion(HookSimpleOcclusionRaycastOcclusion);
                new Hook(typeof(SimpleOcclusion).GetNestedType("<RaycastOcclusion>c__Iterator0", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.MultiplayerMode).GetMethod("HookSimpleOcclusionRaycastOcclusionIntermediateHook"), null);
            }

            private static void HookSimpleOcclusionStart(On.SimpleOcclusion.orig_Start orig, SimpleOcclusion simpleOcclusion)
            {
                if (simpleOcclusion.source == null)
                {
                    simpleOcclusion.source = ((MonoBehaviour)simpleOcclusion).GetComponent<AudioSource>();
                }
                if (simpleOcclusion.source != null)
                {
                    AssignOriginalAudioSourceFromCopiedSource(ref simpleOcclusion.source, ref simpleOcclusion);
                }
                orig.Invoke(simpleOcclusion);
            }

            private static void CalculateOcclusion(int _hitLength, SimpleOcclusion simpleOcclusion, Vector3 playerPosition, AudioSource originalAudioSource)
            {
                if (_hitLength > 0 && !simpleOcclusion.source.mute)
                {
                    try
                    {
                        if (simpleOcclusion.DSP != null)
                        {
                            //Debug.Log("Using DSP position for simple occlusion: " + simpleOcclusion.DSP.transform.position);
                            simpleOcclusion.sourceNode = LevelGeneration.GetNodeDataAtPosition(simpleOcclusion.DSP.transform.position);
                        }
                        else
                        {
                            //Debug.Log("Using original audio source position for simple occlusion: " + originalAudioSource.transform.position);
                            simpleOcclusion.sourceNode = LevelGeneration.GetNodeDataAtPosition(originalAudioSource.transform.position);
                        }
                    }
                    catch
                    {
                        Debug.Log("Error in SimpleOcclusion.CalculateOcclusion 1");
                    }
                    try
                    {
                        simpleOcclusion.listenerNode = LevelGeneration.GetNodeDataAtPosition(playerPosition);

                    }
                    catch
                    {
                        Debug.Log("Error in SimpleOcclusion.CalculateOcclusion 2");
                    }
                    try
                    {
                        if (simpleOcclusion.sourceNode != null & simpleOcclusion.listenerNode != null)
                        {
                            simpleOcclusion.sourceY = simpleOcclusion.sourceNode.regionNode.y;
                            simpleOcclusion.listenerY = simpleOcclusion.listenerNode.regionNode.y;
                            simpleOcclusion.divider = Mathf.Abs(simpleOcclusion.sourceY - simpleOcclusion.listenerY);
                            simpleOcclusion.divider = (float)Mathf.RoundToInt(simpleOcclusion.divider);
                            simpleOcclusion.sourceRoom = simpleOcclusion.sourceNode.nodeRoom;
                            simpleOcclusion.listenerRoom = simpleOcclusion.listenerNode.nodeRoom;
                            if (simpleOcclusion.sourceRoom != null && simpleOcclusion.listenerRoom != null && simpleOcclusion.sourceRoom != simpleOcclusion.listenerRoom)
                            {
                                if (simpleOcclusion.sourceRoom.RoomType == RoomStructure.Room && simpleOcclusion.sourceRoom.roomDoors.Count > 0)
                                {
                                    simpleOcclusion.allDoorsShut = true;
                                    for (int i = 0; i < simpleOcclusion.sourceRoom.roomDoors.Count; i++)
                                    {
                                        Door door = simpleOcclusion.sourceRoom.roomDoors[i];
                                        if (door.isOpen)
                                        {
                                            simpleOcclusion.allDoorsShut = false;
                                            break;
                                        }
                                    }
                                    if (simpleOcclusion.allDoorsShut)
                                    {
                                        simpleOcclusion.divider += 1f;
                                        _hitLength += 2;
                                    }
                                }
                                if (simpleOcclusion.listenerRoom.RoomType == RoomStructure.Room && simpleOcclusion.listenerRoom.roomDoors.Count > 0)
                                {
                                    simpleOcclusion.allDoorsShut = true;
                                    for (int j = 0; j < simpleOcclusion.listenerRoom.roomDoors.Count; j++)
                                    {
                                        Door door2 = simpleOcclusion.listenerRoom.roomDoors[j];
                                        if (door2.isOpen)
                                        {
                                            simpleOcclusion.allDoorsShut = false;
                                            break;
                                        }
                                    }
                                    if (simpleOcclusion.allDoorsShut)
                                    {
                                        simpleOcclusion.divider += 1f;
                                        _hitLength += 2;
                                    }
                                }
                            }
                            if (simpleOcclusion.isAdvanced)
                            {
                                if (simpleOcclusion.advOcclusion != null)
                                {
                                    if (simpleOcclusion.listenerRoom != null && simpleOcclusion.sourceRoom != null && simpleOcclusion.currentListenerRoom != simpleOcclusion.listenerRoom)
                                    {
                                        simpleOcclusion.currentListenerRoom = simpleOcclusion.listenerRoom;
                                        if (simpleOcclusion.sourceRoom != simpleOcclusion.currentListenerRoom)
                                        {
                                            simpleOcclusion.advOcclusion.DoRaycasts(originalAudioSource, simpleOcclusion.sourceRoom, simpleOcclusion.currentListenerRoom);
                                        }
                                    }
                                    simpleOcclusion.distanceAll = (float)simpleOcclusion.advOcclusion.RoomsFilteredThrough;
                                }
                            }
                            else
                            {
                                simpleOcclusion.distanceAll = 0f;
                            }
                        }
                        else
                        {
                            simpleOcclusion.distanceAll = 0f;
                        }
                    }
                    catch
                    {
                        Debug.Log("Error in SimpleOcclusion.CalculateOcclusion 3");
                    }
                    try
                    {
                        if (simpleOcclusion.divider < 1f)
                        {
                            simpleOcclusion.divider = 1f;
                        }
                        else if (simpleOcclusion.volume != null)
                        {
                            if (simpleOcclusion.divider == 1f)
                            {
                                simpleOcclusion.divider = (float)simpleOcclusion.volume.GetLibrary.floorOcclusion * 0.5f;
                            }
                            else
                            {
                                simpleOcclusion.divider = Mathf.Pow((float)simpleOcclusion.volume.GetLibrary.floorOcclusion, simpleOcclusion.divider);
                            }
                        }
                    }
                    catch
                    {
                        Debug.Log("Error in SimpleOcclusion.CalculateOcclusion 4");
                    }
                }
                else
                {
                    /*
                    Debug.Log("Simple occlusion failed report:");
                    if (_hitLength <= 0)
                    {
                        Debug.Log("Hit length was not greater than 0");
                    }
                    if (simpleOcclusion.source.mute)
                    {
                        Debug.Log("Source was muted");
                    }
                    */
                    simpleOcclusion.divider = 1f;
                }
                try
                {
                    if (simpleOcclusion.volume != null)
                    {
                        _hitLength *= simpleOcclusion.volume.GetLibrary.wallOcclusion;
                    }
                }
                catch
                {
                    Debug.Log("Error in SimpleOcclusion.CalculateOcclusion 5");
                }
                try
                {
                    if (_hitLength <= 0)
                    {
                        _hitLength = 1;
                        simpleOcclusion.requiredVolume = 1f / simpleOcclusion.divider; //0f;//
                        simpleOcclusion.currentFreq = simpleOcclusion.lowPassMax;
                    }
                    else
                    {
                        simpleOcclusion.requiredVolume = 1f / ((float)_hitLength + simpleOcclusion.divider); //0f;//
                        simpleOcclusion.currentFreq = simpleOcclusion.lowPassMax / (1f + (float)_hitLength * 2f * simpleOcclusion.divider);
                    }
                }
                catch
                {
                    Debug.Log("Error in SimpleOcclusion.CalculateOcclusion 6");
                }
                try
                {
                    simpleOcclusion.currentHitsAfter = _hitLength;
                }
                catch
                {
                    Debug.Log("Error in SimpleOcclusion.CalculateOcclusion 7");
                }
                try
                {
                    if (simpleOcclusion.volume != null)
                    {
                        try
                        {
                            //simpleOcclusion.currentVolume = simpleOcclusion.volume.Occlusion; // New code that probably does nothing.
                            //simpleOcclusion.currentVolume = 1f;
                            if (!simpleOcclusion.isPlayer)
                            {
                                simpleOcclusion.currentVolume = Mathf.MoveTowards(simpleOcclusion.currentVolume, simpleOcclusion.requiredVolume, Time.deltaTime); // ~ YOU HAVE TO USE THE VOLUME OF THE VAS MYSOURCE INSTEAD! Actually, this uses the occlusion, so maybe using the volume source will not matter...?
                            }
                            else
                            {
                                simpleOcclusion.currentVolume = 1f;
                            }
                            //simpleOcclusion.currentVolume = 1f;
                        }
                        catch
                        {
                            Debug.Log("Error in SimpleOcclusion.CalculateOcclusion 9");
                        }

                        try
                        {
                            simpleOcclusion.filter.cutoffFrequency = Mathf.MoveTowards(simpleOcclusion.filter.cutoffFrequency, simpleOcclusion.currentFreq, 22000f * Time.deltaTime * 3f);
                            simpleOcclusion.volume.Occlusion = simpleOcclusion.currentVolume;//1f;//
                        }
                        catch
                        {
                            Debug.Log("Error in SimpleOcclusion.CalculateOcclusion 10");
                        }
                        try
                        {
                            //Debug.Log("Setting simpleOcclusion.volume.Occlusion to " + simpleOcclusion.currentVolume + " following the following checks and values: Is player? " + !simpleOcclusion.isPlayer + ". Current Volume: " + simpleOcclusion.currentVolume + ". Required Volume: " + simpleOcclusion.requiredVolume + ".");
                            VolumeControllerCalculateVolume(simpleOcclusion.volume, simpleOcclusion.volume.fadeValue);
                            //simpleOcclusion.volume.CalculateVolume(simpleOcclusion.source, simpleOcclusion.volume.fadeValue); // Original
                            //simpleOcclusion.volume.vol = 1f;
                        }
                        catch
                        {
                            Debug.Log("Error in SimpleOcclusion.CalculateOcclusion 11");
                        }
                    }
                    else
                    {
                        simpleOcclusion.volume = ((MonoBehaviour)simpleOcclusion).GetComponent<VolumeController>();
                    }
                }
                catch
                {
                    Debug.Log("Error in SimpleOcclusion.CalculateOcclusion 8");
                }
            }

            // # SimpleOcclusionRaycastOcclusion

            public static bool HookSimpleOcclusionRaycastOcclusionIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!ManyMonstersMode.IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookSimpleOcclusionRaycastOcclusion((SimpleOcclusion)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    ManyMonstersMode.IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookSimpleOcclusionRaycastOcclusion(SimpleOcclusion simpleOcclusion)
            {
                while (!finishedAudioAssignment)
                {
                    yield return null;
                }
                /*
                VirtualAudioSource virtualAudioSource = simpleOcclusion.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    simpleOcclusion.source = virtualAudioSource.originalAudioSource;//virtualAudioSource.gameObject.GetComponent<AudioSource>();
                    Debug.Log("VAS is not null in SimpleOcclusion.RaycastOcclusion!");
                }
                else
                {
                    Debug.Log("VAS is null in SimpleOcclusion.RaycastOcclusion!");
                    bool foundOriginalSource = false;
                    foreach (VirtualAudioSource VAS in virtualAudioSources)
                    {
                        if (VAS != null && VAS.mySource != null && simpleOcclusion.source != null && VAS.mySource == simpleOcclusion.source)
                        {
                            Debug.Log("VAS is not null via alternative method in SimpleOcclusion.RaycastOcclusion!");
                            simpleOcclusion.source = VAS.originalAudioSource;//VAS.gameObject.GetComponent<AudioSource>();
                            foundOriginalSource = true;
                            break;
                        }
                    }
                    if (!foundOriginalSource)
                    {
                        Debug.Log("Could not match VAS to simpleOcclusion.source in SimpleOcclusion.RaycastOcclusion!");
                    }
                }
                */
                if (simpleOcclusion.source == null)
                {
                    Debug.Log("Original source is null at start in SimpleOcclusion.RaycastOcclusion!");
                    yield break;
                }
                for (; ; )
                {
                    int hits = 0;
                    simpleOcclusion.roomsHit.Clear();
                    simpleOcclusion.roomsHit.Capacity = 15;

                    int closestPlayerNumber = ClosestPlayerToThis(simpleOcclusion.source.transform.position);
                    NewPlayerClass closestPlayer = newPlayerClasses[closestPlayerNumber];
                    Vector3 closestPlayerPosition = closestPlayer.transform.position;
                    if (simpleOcclusion.filter != null && simpleOcclusion.source != null && simpleOcclusion.source.maxDistance > Vector3.Distance(closestPlayerPosition, simpleOcclusion.source.transform.position /*((MonoBehaviour)simpleOcclusion).transform.position*/) - 5f)
                    {
                        Vector3 origin = closestPlayerPosition;
                        Vector3 target = simpleOcclusion.source.transform.position + Vector3.up * 0.3f;//((MonoBehaviour)simpleOcclusion).transform.position + Vector3.up * 0.3f;
                        bool atGoal = false;
                        simpleOcclusion.roomsHit.Clear();
                        while (!atGoal)
                        {
                            float distance = Vector3.Distance(origin, target);
                            if (distance > 0f)
                            {
                                Vector3 normalized = (target - origin).normalized;
                                RaycastHit hit;
                                if (Physics.Raycast(origin, normalized, out hit, distance, simpleOcclusion.layerMask))
                                {
                                    if (simpleOcclusion.source.isPlaying)
                                    {
                                        float num = (float)(1 / (hits + 1));
                                        num = Mathf.Clamp(num, 0.1f, 1f);
                                        Color color = new Color(1f, 0f, 0f, num);
                                    }
                                    origin = hit.point + normalized * 0.001f;
                                    if (hit.collider.GetComponent<OcclusionAllower>())
                                    {
                                        bool flag = true;
                                        simpleOcclusion.rm = hit.collider.GetComponentInParent<Room>();
                                        if (simpleOcclusion.rm != null)
                                        {
                                            bool flag2 = true;
                                            for (int i = 0; i < simpleOcclusion.roomsHit.Count; i++)
                                            {
                                                Room y = simpleOcclusion.roomsHit[i];
                                                if (simpleOcclusion.rm == y)
                                                {
                                                    flag2 = false;
                                                }
                                            }
                                            if (flag2)
                                            {
                                                simpleOcclusion.roomsHit.Add(simpleOcclusion.rm);
                                            }
                                            else
                                            {
                                                flag = false;
                                            }
                                        }
                                        Door componentInParent = hit.collider.GetComponentInParent<Door>();
                                        if (componentInParent != null)
                                        {
                                            Door componentInParent2 = simpleOcclusion.source.GetComponentInParent<Door>();//((MonoBehaviour)simpleOcclusion).GetComponentInParent<Door>();
                                            if (componentInParent2 != null && componentInParent2 == componentInParent)
                                            {
                                                flag = false;
                                            }
                                        }
                                        if (flag)
                                        {
                                            hits++;
                                        }
                                    }
                                }
                                else
                                {
                                    atGoal = true;
                                }
                            }
                            else
                            {
                                atGoal = true;
                            }
                            yield return null;
                        }
                        simpleOcclusion.currentHits = hits;
                    }
                    else
                    {
                        simpleOcclusion.currentHits = hits;
                    }
                    yield return null;
                }
                //yield break;
            }

            private static IEnumerator HookSimpleOcclusionRaycastOcclusionOne(SimpleOcclusion simpleOcclusion)
            {
                yield break;
                /*
                int hits = 0;
                simpleOcclusion.roomsHit.Clear();
                simpleOcclusion.roomsHit.Capacity = 15;
                simpleOcclusion.currentHits = hits;
                //Debug.Log("Running SimpleOcclusion.RaycastOcclusion 1");
                if (simpleOcclusion.source != null)
                {
                    //Debug.Log("Running SimpleOcclusion.RaycastOcclusion 2");
                    int sourceInstanceID = simpleOcclusion.source.GetInstanceID();
                    for (; ; )
                    {
                        hits = 0;
                        simpleOcclusion.roomsHit.Clear();
                        simpleOcclusion.roomsHit.Capacity = 15;
                        if (finishedAudioAssignment)
                        {
                            if (audioSourceCopyToOriginalDictionary.ContainsKey(sourceInstanceID))
                            {
                                //Debug.Log("Running SimpleOcclusion.RaycastOcclusion 3 (Loop)");

                                if (simpleOcclusion.filter != null && simpleOcclusion.source != null)
                                {
                                    AudioSource originalAudioSource = originalAudioSources[audioSourceCopyToOriginalDictionary[sourceInstanceID]];
                                    if (originalAudioSource != null)
                                    {
                                        int closestPlayerNumber = ClosestPlayerToThis(originalAudioSource.transform.position);
                                        NewPlayerClass closestPlayer = newPlayerClasses[closestPlayerNumber];
                                        Vector3 closestPlayerPosition = closestPlayer.transform.position;


                                        //if (originalAudioSource.maxDistance > Vector3.Distance(SimpleOcclusion.listener.transform.position, originalAudioSource.transform.position) - 5f)
                                        if (originalAudioSource.maxDistance > Vector3.Distance(closestPlayerPosition, originalAudioSource.transform.position) - 5f)
                                        {
                                            //Vector3 origin = SimpleOcclusion.listener.transform.position;
                                            Vector3 origin = closestPlayerPosition;
                                            Vector3 target = originalAudioSource.transform.position + Vector3.up * 0.3f;
                                            bool atGoal = false;
                                            simpleOcclusion.roomsHit.Clear();
                                            while (!atGoal)
                                            {
                                                float distance = Vector3.Distance(origin, target);
                                                if (distance > 0f)
                                                {
                                                    Vector3 normalized = (target - origin).normalized;
                                                    RaycastHit hit;
                                                    if (Physics.Raycast(origin, normalized, out hit, distance, simpleOcclusion.layerMask))
                                                    {
                                                        if (simpleOcclusion.source.isPlaying)
                                                        {
                                                            float num = (float)(1 / (hits + 1));
                                                            num = Mathf.Clamp(num, 0.1f, 1f);
                                                            Color color = new Color(1f, 0f, 0f, num);
                                                        }
                                                        origin = hit.point + normalized * 0.001f;
                                                        if (hit.collider.GetComponent<OcclusionAllower>())
                                                        {
                                                            bool flag = true;
                                                            simpleOcclusion.rm = hit.collider.GetComponentInParent<Room>();
                                                            if (simpleOcclusion.rm != null)
                                                            {
                                                                bool raycastRoomNotHit = true;
                                                                for (int i = 0; i < simpleOcclusion.roomsHit.Count; i++)
                                                                {
                                                                    Room roomToCheck = simpleOcclusion.roomsHit[i];
                                                                    if (simpleOcclusion.rm == roomToCheck)
                                                                    {
                                                                        raycastRoomNotHit = false;
                                                                    }
                                                                }
                                                                if (raycastRoomNotHit)
                                                                {
                                                                    simpleOcclusion.roomsHit.Add(simpleOcclusion.rm);
                                                                }
                                                                else
                                                                {
                                                                    flag = false;
                                                                }
                                                            }
                                                            Door raycastRoomDoor = hit.collider.GetComponentInParent<Door>();
                                                            if (raycastRoomDoor != null)
                                                            {
                                                                Door sourceDoor = originalAudioSource.GetComponentInParent<Door>(); //((MonoBehaviour)simpleOcclusion).GetComponentInParent<Door>();
                                                                if (sourceDoor != null && sourceDoor == raycastRoomDoor)
                                                                {
                                                                    flag = false;
                                                                }
                                                            }
                                                            if (flag)
                                                            {
                                                                hits++;
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        atGoal = true;
                                                    }
                                                }
                                                else
                                                {
                                                    atGoal = true;
                                                }
                                                yield return null;
                                            }
                                            //Debug.Log("Assigning current hits in SimpleOcclusion.RaycastOcclusion 1: " + hits);
                                            //simpleOcclusion.currentHits = hits;
                                        }
                                        else
                                        {
                                            //Debug.Log("Original source was too far away. Player: " + closestPlayerPosition + ". Source: " + originalAudioSource.transform.position);
                                        }
                                    }
                                    else
                                    {
                                        Debug.Log("Original source was null");
                                        Destroy(simpleOcclusion.source);
                                        Destroy(simpleOcclusion);
                                    }
                                }
                                else
                                {
                                    Debug.Log("Filter or source were null");
                                }
                            }
                            else
                            {
                                Debug.Log("SimpleOcclusion.RaycastOcclusion: Could not find original source in dictionary. " + simpleOcclusion.source.name + ".");
                            }
                        }
                        //Debug.Log("Assigning current hits in SimpleOcclusion.RaycastOcclusion 2: " + hits);
                        simpleOcclusion.currentHits = hits;
                        yield return null;
                    }
                }
                else
                {
                    Debug.Log("SimpleOcclusion.RaycastOcclusion: Source is null. Stopping coroutine.");
                }
                yield break; // Normally unreachable code.
                */
            }

            /*
            private static void CalculateOcclusion(int _hitLength, SimpleOcclusion simpleOcclusion, AudioListener audioListener)
            {
                if (_hitLength > 0 && !simpleOcclusion.source.mute)
                {
                    if (simpleOcclusion.DSP != null)
                    {
                        simpleOcclusion.sourceNode = LevelGeneration.GetNodeDataAtPosition(simpleOcclusion.DSP.transform.position);
                    }
                    else
                    {
                        simpleOcclusion.sourceNode = LevelGeneration.GetNodeDataAtPosition(simpleOcclusion.source.transform.position);
                    }
                    simpleOcclusion.listenerNode = LevelGeneration.GetNodeDataAtPosition(audioListener.transform.position);
                    if (simpleOcclusion.sourceNode != null & simpleOcclusion.listenerNode != null)
                    {
                        simpleOcclusion.sourceY = simpleOcclusion.sourceNode.regionNode.y;
                        simpleOcclusion.listenerY = simpleOcclusion.listenerNode.regionNode.y;
                        simpleOcclusion.divider = Mathf.Abs(simpleOcclusion.sourceY - simpleOcclusion.listenerY);
                        simpleOcclusion.divider = (float)Mathf.RoundToInt(simpleOcclusion.divider);
                        simpleOcclusion.sourceRoom = simpleOcclusion.sourceNode.nodeRoom;
                        simpleOcclusion.listenerRoom = simpleOcclusion.listenerNode.nodeRoom;
                        if (simpleOcclusion.sourceRoom != null && simpleOcclusion.listenerRoom != null && simpleOcclusion.sourceRoom != simpleOcclusion.listenerRoom)
                        {
                            if (simpleOcclusion.sourceRoom.RoomType == RoomStructure.Room && simpleOcclusion.sourceRoom.roomDoors.Count > 0)
                            {
                                simpleOcclusion.allDoorsShut = true;
                                for (int i = 0; i < simpleOcclusion.sourceRoom.roomDoors.Count; i++)
                                {
                                    Door door = simpleOcclusion.sourceRoom.roomDoors[i];
                                    if (door.isOpen)
                                    {
                                        simpleOcclusion.allDoorsShut = false;
                                        break;
                                    }
                                }
                                if (simpleOcclusion.allDoorsShut)
                                {
                                    simpleOcclusion.divider += 1f;
                                    _hitLength += 2;
                                }
                            }
                            if (simpleOcclusion.listenerRoom.RoomType == RoomStructure.Room && simpleOcclusion.listenerRoom.roomDoors.Count > 0)
                            {
                                simpleOcclusion.allDoorsShut = true;
                                for (int j = 0; j < simpleOcclusion.listenerRoom.roomDoors.Count; j++)
                                {
                                    Door door2 = simpleOcclusion.listenerRoom.roomDoors[j];
                                    if (door2.isOpen)
                                    {
                                        simpleOcclusion.allDoorsShut = false;
                                        break;
                                    }
                                }
                                if (simpleOcclusion.allDoorsShut)
                                {
                                    simpleOcclusion.divider += 1f;
                                    _hitLength += 2;
                                }
                            }
                        }
                        if (simpleOcclusion.isAdvanced)
                        {
                            if (simpleOcclusion.advOcclusion != null)
                            {
                                if (simpleOcclusion.listenerRoom != null && simpleOcclusion.sourceRoom != null && simpleOcclusion.currentListenerRoom != simpleOcclusion.listenerRoom)
                                {
                                    simpleOcclusion.currentListenerRoom = simpleOcclusion.listenerRoom;
                                    if (simpleOcclusion.sourceRoom != simpleOcclusion.currentListenerRoom)
                                    {
                                        simpleOcclusion.advOcclusion.DoRaycasts(simpleOcclusion.source, simpleOcclusion.sourceRoom, simpleOcclusion.currentListenerRoom);
                                    }
                                }
                                simpleOcclusion.distanceAll = (float)simpleOcclusion.advOcclusion.RoomsFilteredThrough;
                            }
                        }
                        else
                        {
                            simpleOcclusion.distanceAll = 0f;
                        }
                    }
                    else
                    {
                        simpleOcclusion.distanceAll = 0f;
                    }
                    if (simpleOcclusion.divider < 1f)
                    {
                        simpleOcclusion.divider = 1f;
                    }
                    else if (simpleOcclusion.volume != null)
                    {
                        if (simpleOcclusion.divider == 1f)
                        {
                            simpleOcclusion.divider = (float)simpleOcclusion.volume.GetLibrary.floorOcclusion * 0.5f;
                        }
                        else
                        {
                            simpleOcclusion.divider = Mathf.Pow((float)simpleOcclusion.volume.GetLibrary.floorOcclusion, simpleOcclusion.divider);
                        }
                    }
                }
                else
                {
                    simpleOcclusion.divider = 1f;
                }
                if (simpleOcclusion.volume != null)
                {
                    _hitLength *= simpleOcclusion.volume.GetLibrary.wallOcclusion;
                }
                if (_hitLength <= 0)
                {
                    _hitLength = 1;
                    simpleOcclusion.requiredVolume = 1f / simpleOcclusion.divider;
                    simpleOcclusion.currentFreq = simpleOcclusion.lowPassMax;
                }
                else
                {
                    simpleOcclusion.requiredVolume = 1f / ((float)_hitLength + simpleOcclusion.divider);
                    simpleOcclusion.currentFreq = simpleOcclusion.lowPassMax / (1f + (float)_hitLength * 2f * simpleOcclusion.divider);
                }
                simpleOcclusion.currentHitsAfter = _hitLength;
                if (simpleOcclusion.volume != null)
                {
                    if (!simpleOcclusion.isPlayer)
                    {
                        simpleOcclusion.currentVolume = Mathf.MoveTowards(simpleOcclusion.currentVolume, simpleOcclusion.requiredVolume, Time.deltaTime);
                    }
                    else
                    {
                        simpleOcclusion.currentVolume = 1f;
                    }
                    simpleOcclusion.filter.cutoffFrequency = Mathf.MoveTowards(simpleOcclusion.filter.cutoffFrequency, simpleOcclusion.currentFreq, 22000f * Time.deltaTime * 3f);
                    simpleOcclusion.volume.Occlusion = simpleOcclusion.currentVolume;
                    simpleOcclusion.volume.CalculateVolume(simpleOcclusion.source, simpleOcclusion.volume.fadeValue);
                }
                else
                {
                    simpleOcclusion.volume = ((MonoBehaviour)simpleOcclusion).GetComponent<VolumeController>();
                }
            }

            private static IEnumerator RaycastOcclusion(SimpleOcclusion simpleOcclusion, AudioListener playerAudioListener)
            {
                for (; ; )
                {
                    if (LevelGeneration.Instance.finishedGenerating)
                    {
                        int hits = 0;
                        simpleOcclusion.roomsHit.Clear();
                        simpleOcclusion.roomsHit.Capacity = 15;
                        if (simpleOcclusion.filter != null && simpleOcclusion.source != null && simpleOcclusion.source.maxDistance > Vector3.Distance(playerAudioListener.transform.position, ((MonoBehaviour)simpleOcclusion).transform.position) - 5f)
                        {
                            Vector3 origin = playerAudioListener.transform.position;
                            Vector3 target = ((MonoBehaviour)simpleOcclusion).transform.position + Vector3.up * 0.3f;
                            bool atGoal = false;
                            simpleOcclusion.roomsHit.Clear();
                            while (!atGoal)
                            {
                                float distance = Vector3.Distance(origin, target);
                                if (distance > 0f)
                                {
                                    Vector3 normalized = (target - origin).normalized;
                                    RaycastHit hit;
                                    if (Physics.Raycast(origin, normalized, out hit, distance, simpleOcclusion.layerMask))
                                    {
                                        if (simpleOcclusion.source.isPlaying)
                                        {
                                            float num = (float)(1 / (hits + 1));
                                            num = Mathf.Clamp(num, 0.1f, 1f);
                                            Color color = new Color(1f, 0f, 0f, num);
                                        }
                                        origin = hit.point + normalized * 0.001f;
                                        if (hit.collider.GetComponent<OcclusionAllower>())
                                        {
                                            bool flag = true;
                                            simpleOcclusion.rm = hit.collider.GetComponentInParent<Room>();
                                            if (simpleOcclusion.rm != null)
                                            {
                                                bool flag2 = true;
                                                for (int i = 0; i < simpleOcclusion.roomsHit.Count; i++)
                                                {
                                                    Room y = simpleOcclusion.roomsHit[i];
                                                    if (simpleOcclusion.rm == y)
                                                    {
                                                        flag2 = false;
                                                    }
                                                }
                                                if (flag2)
                                                {
                                                    simpleOcclusion.roomsHit.Add(simpleOcclusion.rm);
                                                }
                                                else
                                                {
                                                    flag = false;
                                                }
                                            }
                                            Door componentInParent = hit.collider.GetComponentInParent<Door>();
                                            if (componentInParent != null)
                                            {
                                                Door componentInParent2 = ((MonoBehaviour)simpleOcclusion).GetComponentInParent<Door>();
                                                if (componentInParent2 != null && componentInParent2 == componentInParent)
                                                {
                                                    flag = false;
                                                }
                                            }
                                            if (flag)
                                            {
                                                hits++;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        atGoal = true;
                                    }
                                }
                                else
                                {
                                    atGoal = true;
                                }
                                yield return null;
                            }
                            simpleOcclusion.currentHits = hits;
                        }
                        else
                        {
                            simpleOcclusion.currentHits = hits;
                        }
                    }
                    yield return null;
                }
                yield break;
            }

            private static void HookSimpleOcclusionStart(On.SimpleOcclusion.orig_Start orig, SimpleOcclusion simpleOcclusion)
            {
                if (((MonoBehaviour)simpleOcclusion).GetComponentInParent<NewPlayerClass>())
                {
                    simpleOcclusion.isPlayer = true;
                }
                if (simpleOcclusion.source == null)
                {
                    simpleOcclusion.source = ((MonoBehaviour)simpleOcclusion).GetComponent<AudioSource>();
                }
                if (simpleOcclusion.filter == null)
                {
                    simpleOcclusion.filter = ((MonoBehaviour)simpleOcclusion).GetComponent<AudioLowPassFilter>();
                }
                if (simpleOcclusion.filter == null)
                {
                    simpleOcclusion.filter = ((MonoBehaviour)simpleOcclusion).gameObject.AddComponent<AudioLowPassFilter>();
                }
                simpleOcclusion.layerMask.value = (1 << LayerMask.NameToLayer("Default") | 1 << LayerMask.NameToLayer("Interactable") | 1 << LayerMask.NameToLayer("DefaultNavVision"));
                simpleOcclusion.source.volume = 0f;
                simpleOcclusion.volume = simpleOcclusion.source.GetComponent<VolumeController>();
                if (SimpleOcclusion.listener == null)
                {
                    SimpleOcclusion.listener = References.Player.GetComponentInChildren<AudioListener>();
                }
                simpleOcclusion.advOcclusion = ((MonoBehaviour)simpleOcclusion).GetComponent<AdvancedOcclusion>();
                simpleOcclusion.DSP = simpleOcclusion.source.GetComponentInChildren<DestroySourcePosition>();
                ((MonoBehaviour)simpleOcclusion).StartCoroutine(WaitUntilGenerationIsFinishedSimpleOcclusion(simpleOcclusion));
            }

            private static IEnumerator WaitUntilGenerationIsFinishedSimpleOcclusion(SimpleOcclusion simpleOcclusion)
            {
                while (!LevelGeneration.Instance.finishedGenerating)
                {
                    yield return null;
                }
                foreach (AudioListener playerAudioListener in playerAudioListeners)
                {
                    ((MonoBehaviour)simpleOcclusion).StartCoroutine(RaycastOcclusion(simpleOcclusion, playerAudioListener));
                }
                yield break;
            }
            */

            /*
            private static void HookSimpleOcclusionUpdate(On.SimpleOcclusion.orig_Update orig, SimpleOcclusion simpleOcclusion)
            {
                if (simpleOcclusion.source != null && finishedAudioAssignment)
                {
                    int sourceInstanceID = simpleOcclusion.source.GetInstanceID();
                    if (audioSourceCopyToOriginalDictionary.ContainsKey(sourceInstanceID))
                    {
                        AudioSource originalAudioSource = originalAudioSources[audioSourceCopyToOriginalDictionary[sourceInstanceID]];
                        if (originalAudioSource != null)
                        {
                            //Debug.Log("Original audio source is NOT null in SimpleOcclusion.Update.");
                            int closestPlayerNumber = ClosestPlayerToThis(originalAudioSource.transform.position);
                            NewPlayerClass closestPlayer = newPlayerClasses[closestPlayerNumber];
                            Vector3 closestPlayerPosition = closestPlayer.transform.position;

                            if (Vector3.Distance(originalAudioSource.transform.position, closestPlayerPosition) < originalAudioSource.maxDistance)
                            {
                                CalculateOcclusion(simpleOcclusion.currentHits, simpleOcclusion, closestPlayerPosition, originalAudioSource);
                            }
                        }
                        else
                        {
                            //Debug.Log("Original audio source is null in SimpleOcclusion.Update.");
                            //Destroy(simpleOcclusion.source);
                            //Destroy(simpleOcclusion);
                        }
                    }
                }
            }
            */

            private static void HookSimpleOcclusionUpdate(On.SimpleOcclusion.orig_Update orig, SimpleOcclusion simpleOcclusion)
            {
                if (simpleOcclusion.source != null && finishedAudioAssignment)
                {
                    int closestPlayerNumber = ClosestPlayerToThis(simpleOcclusion.source.transform.position);
                    NewPlayerClass closestPlayer = newPlayerClasses[closestPlayerNumber];
                    Vector3 closestPlayerPosition = closestPlayer.transform.position;
                    if (Vector3.Distance(simpleOcclusion.source.transform.position, closestPlayerPosition) < simpleOcclusion.source.maxDistance)
                    {
                        CalculateOcclusion(simpleOcclusion.currentHits, simpleOcclusion, closestPlayerPosition, simpleOcclusion.source);
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Smashable

            private static void HookSmashableSmash(On.Smashable.orig_Smash orig, Smashable smashable, float _collisionForce)
            {
                for (int i = 0; i < crewPlayers.Count; i++)
                {
                    if (!monsterPlayers.Contains(crewPlayers[i]) && playersDowned[i] == true && Vector3.Distance(crewPlayers[i].transform.position, smashable.transform.position) < 2f)
                    {
                        RevivePlayer(crewPlayers[i]);
                        break;
                    }
                }
                orig.Invoke(smashable, _collisionForce);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SteamHandle

            private static void HookSteamHandle()
            {
                On.SteamHandle.OnFinishFixedAnimation += new On.SteamHandle.hook_OnFinishFixedAnimation(HookSteamHandleOnFinishFixedAnimation);
            }

            private static void HookSteamHandleOnFinishFixedAnimation(On.SteamHandle.orig_OnFinishFixedAnimation orig, SteamHandle steamHandle)
            {
                orig.Invoke(steamHandle);
                Debug.Log(new StackTrace().ToString());
            }


            private static void HookSteamHandleUpdate(On.SteamHandle.orig_Update orig, SteamHandle steamHandle)
            {
                if (steamHandle.turnTime)
                {
                    if (!steamHandle.flippedHandle)
                    {
                        steamHandle.rotate = 1f;
                    }
                    else
                    {
                        steamHandle.rotate = -1f;
                    }
                    steamHandle.timer += Time.deltaTime;
                    if (steamHandle.timer >= 0.8f)
                    {
                        steamHandle.turn = true;
                    }
                    if (steamHandle.timer >= 1.2f)
                    {
                        steamHandle.turn = false;
                    }
                    if (steamHandle.steamVents.ventOn)
                    {
                        foreach (NewPlayerClass newPlayerClass in newPlayerClasses)
                        {
                            newPlayerClass.ValveValue = 1f;
                        }
                    }
                    else
                    {
                        foreach (NewPlayerClass newPlayerClass in newPlayerClasses)
                        {
                            newPlayerClass.ValveValue = -1f;
                        }
                    }
                    if (steamHandle.turn)
                    {
                        if (steamHandle.steamVents.ventOn)
                        {
                            steamHandle.steamHandle.transform.RotateAround(steamHandle.steamHandle.transform.position, -steamHandle.room.transform.forward * steamHandle.rotate, 20f * (Time.deltaTime * 1.4f));
                        }
                        else
                        {
                            steamHandle.steamHandle.transform.RotateAround(steamHandle.steamHandle.transform.position, steamHandle.room.transform.forward * steamHandle.rotate, 20f * (Time.deltaTime * 1.4f));
                        }
                    }
                    else
                    {
                        steamHandle.steamHandle.transform.Rotate(0f, 0f, 0f);
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SteamPushBack

            private static float GetPushDirection(SteamPushBack steamPushBack, int playerNumber)
            {
                Transform playerTransform = newPlayerClasses[playerNumber].transform;
                Vector3 pos = playerTransform.position + playerTransform.forward * 0.2f;
                Vector3 pos2 = playerTransform.position + playerTransform.forward * -0.2f;
                if (steamPushBack.GetDistanceToVent(pos) > steamPushBack.GetDistanceToVent(pos2))
                {
                    return 1f;
                }
                return -1f;
            }

            private static void HookSteamPushBackOnTriggerEnter(On.SteamPushBack.orig_OnTriggerEnter orig, SteamPushBack steamPushBack, Collider collider)
            {
                if (collider.name == "PlayerDamage")
                {
                    PushBackPlayerAnimation(steamPushBack, PlayerNumber(collider.transform.GetParentOfType<NewPlayerClass>().GetInstanceID()));
                }
            }

            private static IEnumerator PushBack(SteamPushBack steamPushBack, int playerNumber)
            {
                Vector3 dir = steamPushBack.steamVent.innerBox.transform.right * 3f;
                Vector3 line = steamPushBack.steamVent.innerBox.transform.position - dir;
                Vector3 line2 = line + dir * 2f;
                line.y = (line2.y = 0f);
                float t = 0f;
                while (t < 1f)
                {
                    Vector3 pos = newPlayerClasses[playerNumber].transform.position;
                    pos.y = 0f;
                    if (MathHelper.DistanceFromPointToLine(pos, line, line2) > 0.1f)
                    {
                        Vector3 closestPointToLine = MathHelper.GetClosestPointToLine(line, line2, pos);
                        closestPointToLine.y = 0f;
                        Vector3 a = closestPointToLine - pos;
                        a.Normalize();
                        newPlayerClasses[playerNumber].Motor.Move(a * Time.deltaTime);
                    }
                    t += Time.deltaTime;
                    yield return null;
                }
                yield break;
            }

            private static void PushBackPlayerAnimation(SteamPushBack steamPushBack, int playerNumber)
            {
                newPlayerClasses[playerNumber].PushBackDir = GetPushDirection(steamPushBack, playerNumber);
                newPlayerClasses[playerNumber].playerAnimator.applyRootMotion = true;
                ((MonoBehaviour)steamPushBack).StartCoroutine(PushBack(steamPushBack, playerNumber));
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SteamVents

            private static void HookSteamVentsUpdate(On.SteamVents.orig_Update orig, SteamVents steamVents)
            {
                if (LevelGeneration.Instance.finishedGenerating && !steamVents.disableCones)
                {
                    if (SteamVents.svm.MasterControlOverride || steamVents.steamCurrentlyOn)
                    {
                        steamVents.startTime += Time.deltaTime;
                        if (!steamVents.firstPass && steamVents.ventOn)
                        {
                            if (steamVents.steamPipes != null)
                            {
                                if (steamVents.startTime >= 0f && steamVents.startTime < 2.5f && !steamVents.shakeAnimation.isPlaying)
                                {
                                    steamVents.shakeAnimation.Play();
                                    steamVents.RattleSound();
                                }
                            }
                            else
                            {
                                steamVents.shakeAnimation.Stop();
                            }
                            if (steamVents.startTime > 2.5f && steamVents.startTime < 3.5f)
                            {
                                steamVents.TurnOnSteam();
                                //Debug.Log("Steam vent blast source is at " + steamVents.blastSource.transform.position + " and its volume is " + steamVents.blastSource.volume);
                            }
                            if (steamVents.startTime >= 3.5f)
                            {
                                steamVents.TurnOffSteam();
                            }
                        }
                        if (steamVents.startTime >= (float)steamVents.offTimer && (steamVents.pipeModel.enabled || Vector3.Distance(newPlayerClasses[ClosestPlayerToThis(((MonoBehaviour)steamVents).transform.position)].transform.position, ((MonoBehaviour)steamVents).transform.position) < steamVents.ventSource.maxDistance * 1.3f))
                        {
                            steamVents.firstPass = false;
                            steamVents.startTime = 0f;
                            steamVents.offTimer = UnityEngine.Random.Range(5, 8);
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < steamVents.steamVentParticles.Length; i++)
                    {
                        steamVents.psem = steamVents.steamVentParticles[i].emission;
                        steamVents.psem.enabled = false;
                    }
                }
            }

            private static void HookSteamVentsTurnOffSteam(On.SteamVents.orig_TurnOffSteam orig, SteamVents steamVents)
            {
                steamVents.ventSource.Stop();
                VirtualAudioSource virtualAudioSource1 = steamVents.ventSource.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource1 != null)
                {
                    virtualAudioSource1.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null 1!\n" + new StackTrace().ToString());
                }
                steamVents.shakeAnimation.Stop();
                steamVents.steamCurrentlyOn = false;
                if (steamVents.outerBoxMeshColl.enabled)
                {
                    for (int i = 0; i < steamVents.steamVentParticles.Length; i++)
                    {
                        steamVents.psem = steamVents.steamVentParticles[i].emission;
                        steamVents.psem.enabled = false;
                    }
                    steamVents.outerBoxMeshColl.enabled = false;
                    steamVents.innerBoxMeshColl.enabled = false;
                    steamVents.stunBox.SetActive(false);
                    steamVents.blastSource.Stop();
                    VirtualAudioSource virtualAudioSource2 = steamVents.blastSource.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource2 != null)
                    {
                        virtualAudioSource2.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null 2!\n" + new StackTrace().ToString());
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SubmarineBattery

            private static void HookSubmarineBatteryOnStartItemAnimation(On.SubmarineBattery.orig_OnStartItemAnimation orig, SubmarineBattery submarineBattery)
            {
                NewPlayerClass newPlayerClass = lastPlayerSentMessage;
                PlayerClipCamera newPlayerClassClipCamera = newPlayerClass.GetComponentInChildren<PlayerClipCamera>();

                newPlayerClassClipCamera.EnableCamera(false);
                ((MonoBehaviour)submarineBattery).StartCoroutine(SubmarineBatteryLerp(submarineBattery, newPlayerClass, newPlayerClassClipCamera));
            }

            private static IEnumerator SubmarineBatteryLerp(SubmarineBattery submarineBattery, NewPlayerClass newPlayerClass, PlayerClipCamera playerClipCamera)
            {
                yield return new WaitForSeconds(1.21f);
                ((MonoBehaviour)submarineBattery).transform.parent = newPlayerClass.transform;
                submarineBattery.lerp.target = Submarine.mainSub.batteryPosition;
                submarineBattery.lerp.speed = 2f;
                yield return ((MonoBehaviour)submarineBattery).StartCoroutine(submarineBattery.lerp.Lerp(submarineBattery.ani.transform.parent));
                AudioSystem.PlaySound("Noises/Submarine/Fixing/Battery", Submarine.mainSub.SubmarineSource.transform, Submarine.mainSub.SubmarineSource);
                Submarine.mainSub.subBatteryFixed = true;
                yield return new WaitForSeconds(0.5f);
                Inventory inventory = PlayerInventory(newPlayerClass);
                inventory.DestroyItem(inventory.CurrentSlot);
                playerClipCamera.EnableCamera(true);
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SubmarineDoor

            public static bool HookSubmarineDoorOpeningIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!ManyMonstersMode.IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookSubmarineDoorOpening((SubmarineDoor)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    ManyMonstersMode.IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookSubmarineDoorOpening(SubmarineDoor submarineDoor)
            {
                float t = 0f;
                if (submarineDoor.mainDoor)
                {
                    AudioSystem.PlaySound("Noises/Actions/Crane/Movement/Move", SubmarineDoor.submarine.animationSource);
                }
                if (submarineDoor.water != null)
                {
                    submarineDoor.water.Activate();
                }
                while (t < 1f)
                {
                    t += Time.deltaTime / submarineDoor.speed;
                    ((MonoBehaviour)submarineDoor).transform.position = Vector3.Lerp(submarineDoor.startPos, submarineDoor.endPoint, t);
                    yield return null;
                }
                SubmarineDoor.submarine.animationSource.Stop();
                VirtualAudioSource virtualAudioSource = SubmarineDoor.submarine.animationSource.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    virtualAudioSource.Stop();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                }
                if (submarineDoor.mainDoor)
                {
                    AudioSystem.PlaySound("Noises/Actions/Crane/Movement/End", SubmarineDoor.submarine.animationSource.transform, SubmarineDoor.submarine.animationSource);
                }
                ((MonoBehaviour)submarineDoor).transform.position = submarineDoor.endPoint;
                Objectives.Tasks("SubRepair").Tasks("DefendSub").Complete();
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @TapeRecorder

            private static void HookTapeRecorderStopLog(On.TapeRecorder.orig_StopLog orig, TapeRecorder tapeRecorder)
            {
                if (tapeRecorder.cacheSource != null)
                {
                    tapeRecorder.cacheSource.Stop();
                    VirtualAudioSource virtualAudioSource = tapeRecorder.cacheSource.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Stop();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @TestJump

            private static void HookTestJumpUpdate(On.TestJump.orig_Update orig, TestJump testJump)
            {
                Debug.Log("Test jump is actually being used.");
                orig.Invoke(testJump);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @TestPronePullOut

            // Intermediate hook is not necessary as a single passed variable will stop the bug that does not properly do hooks due to not enough information being passed.
            /*
            public static bool HookTestPronePullOutPullOutPlayerIntermediateHook(IEnumerator self, Transform _transformTo)
            {
                IEnumerator replacement;
                if (!ManyMonstersMode.IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookTestPronePullOutPullOutPlayer((TestPronePullOut)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self), _transformTo);
                    ManyMonstersMode.IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }
            */

            private static IEnumerator HookTestPronePullOutPullOutPlayer(On.TestPronePullOut.orig_PullOutPlayer orig, TestPronePullOut testPronePullOut, Transform _transformTo)
            {
                Debug.Log("This hook is working!: " + new StackTrace().ToString()); // # IEnumerator Hook Test
                Debug.Log("Test Prone Pull Out is actually being used");
                yield return orig.Invoke(testPronePullOut, _transformTo);

                /*
                while (testPronePullOut.fracTime < 1f)
                {
                    float angle = Vector3.Angle(References.Player.transform.forward, _transformTo.forward);
                    if (angle >= 50f)
                    {
                        Vector3 forward = _transformTo.position - References.Player.transform.position;
                        forward.Normalize();
                        forward.y = 0f;
                        References.Player.transform.rotation = Quaternion.RotateTowards(References.Player.transform.rotation, Quaternion.LookRotation(forward, Vector3.up), Time.deltaTime * 100f);
                    }
                    else
                    {
                        testPronePullOut.journeyLength = Vector3.Distance(References.Player.transform.position, _transformTo.position);
                        float num = (Time.deltaTime - testPronePullOut.startTime) * testPronePullOut.speed / 2f;
                        float t = num / testPronePullOut.journeyLength;
                        testPronePullOut.fracTime = t;
                        References.Player.transform.position = Vector3.Lerp(References.Player.transform.position, _transformTo.position, t);
                    }
                    yield return null;
                }
                yield break;
                */
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @TriggerObjectives

            private static void HookTriggerObjectivesStart(On.TriggerObjectives.orig_Start orig, TriggerObjectives triggerObjectives)
            {
                /*
                if (playerObjectivesList != null && playerObjectivesList.Count == 0)
                {
                    TriggerObjectives[] foundTriggerObjectives = FindObjectsOfType<TriggerObjectives>();
                    PlayerObjectives[] playerObjectivesArray = FindObjectsOfType<PlayerObjectives>();
                    for (int j = 0; j < foundTriggerObjectives.Length; j++)
                    {
                        foundTriggerObjectives[j].playerObjectives = playerObjectivesArray[j];
                        foundTriggerObjectives[j].newPlayerClass = newPlayerClasses[foundTriggerObjectives.Length - 1 - j];
                        foundTriggerObjectives[j].detectRoom = newPlayerClasses[foundTriggerObjectives.Length - 1 - j].GetComponentInChildren<DetectRoom>();
                        foundTriggerObjectives[j].player = newPlayerClasses[foundTriggerObjectives.Length - 1 - j];
                        playerObjectivesList.Add(foundTriggerObjectives[j].playerObjectives);
                        Debug.Log("Found triggerObjectives number " + j + ", which uses player number " + (foundTriggerObjectives.Length - 1 - j));
                    }
                }
                */

                triggerObjectives.playerObjectives = (UnityEngine.Object.FindObjectOfType(typeof(PlayerObjectives)) as PlayerObjectives);
                playerObjectivesList.Add(triggerObjectives.playerObjectives);
                triggerObjectives.newPlayerClass = (UnityEngine.Object.FindObjectOfType(typeof(NewPlayerClass)) as NewPlayerClass); //triggerObjectives.newPlayerClass = References.PlayerClass;
                triggerObjectives.triggerNotification = (UnityEngine.Object.FindObjectOfType(typeof(TriggerNotification)) as TriggerNotification);
                triggerObjectives.detectRoom = triggerObjectives.newPlayerClass.GetComponentInChildren<DetectRoom>(); //triggerObjectives.detectRoom = References.Player.GetComponentInChildren<DetectRoom>();
                triggerObjectives.oculusTutorialPrompts = (UnityEngine.Object.FindObjectOfType(typeof(OculusTutorialPromptsManager)) as OculusTutorialPromptsManager);
                triggerObjectives.oculusJumpUpPrompt = (UnityEngine.Object.FindObjectOfType(typeof(OculusJumpUpPrompt)) as OculusJumpUpPrompt);
                triggerObjectives.itemHint = false;
                triggerObjectives.run = false;
                triggerObjectives.itemPickedUp = false;
                triggerObjectives.notes = false;
                triggerObjectives.cargoJump = false;
                triggerObjectives.cargoJumpOnce = true;
                triggerObjectives.doorTrigger = false;
                triggerObjectives.pitfall = false;
                triggerObjectives.inventoryScrollOnce = false;
                triggerObjectives.jumpUpTrapOnce = false;
                triggerObjectives.openStartDoor = false;
                triggerObjectives.mainDoor = false;
                triggerObjectives.isStillTutorial = true;
                triggerObjectives.scrollHint = false;
                triggerObjectives.itemCount = 0f;
                triggerObjectives.scrollTimer = 0f;
                triggerObjectives.throwTimer.ResetTimer();
                triggerObjectives.throwTimer.StartTimer();
                triggerObjectives.player = (UnityEngine.Object.FindObjectOfType(typeof(NewPlayerClass)) as NewPlayerClass); //triggerObjectives.player = References.Player.GetComponent<NewPlayerClass>();
                triggerObjectivesList.Add(triggerObjectives);
                Debug.Log("TriggerObjectives.Start playerNumber is " + PlayerNumber(triggerObjectives.newPlayerClass.GetInstanceID()));
            }

            private static void HookTriggerObjectivesUpdate(On.TriggerObjectives.orig_Update orig, TriggerObjectives triggerObjectives)
            {
                if (QualityCheck.isPlayerPromptsEnabled)
                {
                    int playerNumber = PlayerNumber(triggerObjectives.newPlayerClass.GetInstanceID());
                    if (ModSettings.logDebugText)
                    {
                        Debug.Log("TriggerObjectives.Update playerNumber is " + playerNumber);
                    }
                    if (!triggerObjectives.gamePaused)
                    {
                        if (triggerObjectives.isStillTutorial)
                        {
                            Camera playerCamera = PlayerCamera(triggerObjectives.newPlayerClass);
                            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out triggerObjectives.raycastHit, 1.2f, triggerObjectives.mask))
                            {
                                if (triggerObjectives.raycastHit.collider.GetComponent<HoverPlayerTrigger>() != null)
                                {
                                    if (triggerObjectives.raycastHit.collider.name == "TutorialBed")
                                    {
                                        triggerObjectives.TutorialBedText();
                                    }
                                }
                                else if (!OculusManager.isOculusEnabled)
                                {
                                    triggerObjectives.SetToNone();
                                }
                                if (triggerObjectives.raycastHit.collider.GetComponent<HoverPlayerTrigger>() != null)
                                {
                                    if (triggerObjectives.raycastHit.collider.name == "TutorialTable" || triggerObjectives.raycastHit.collider.name == "playerClimbing")
                                    {
                                        triggerObjectives.TutorialCrouchText();
                                    }
                                }
                                else if (!OculusManager.isOculusEnabled)
                                {
                                    triggerObjectives.SetToNone();
                                }
                                if (OculusManager.isOculusEnabled && triggerObjectives.raycastHit.collider.GetComponent<HoverPlayerTrigger>() != null && triggerObjectives.raycastHit.collider.name == "BathroomBlock")
                                {
                                    triggerObjectives.SetOculusPromptsToNone();
                                }
                            }
                            if (triggerObjectives.isStillTutorial && triggerObjectives.openStartDoor && !triggerObjectives.mainDoor)
                            {
                                triggerObjectives.TutorialDoorText();
                            }
                            if (triggerObjectives.run && !triggerObjectives.pitfall)
                            {
                                triggerObjectives.TutorialRunText();
                                triggerObjectives.timer += Time.deltaTime;
                                if (triggerObjectives.timer >= 4.5f)
                                {
                                    triggerObjectives.isStillTutorial = false;
                                    triggerObjectives.SetToNone();
                                    triggerObjectives.timer = 0f;
                                    triggerObjectives.run = false;
                                }
                                if (GetPlayerKey("Sprint", playerNumber).JustPressed() || GetPlayerTriggerStateIfUsingController("Left", playerNumber, true))
                                {
                                    triggerObjectives.isStillTutorial = false;
                                    triggerObjectives.SetToNone();
                                    triggerObjectives.timer = 0f;
                                    triggerObjectives.run = false;
                                }
                                if (triggerObjectives.player.IsRunning())
                                {
                                    triggerObjectives.run = false;
                                    triggerObjectives.timer = 0f;
                                    triggerObjectives.SetToNone();
                                    triggerObjectives.isStillTutorial = false;
                                }
                            }
                            if (triggerObjectives.itemHint)
                            {
                                if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                                {
                                    triggerObjectives.playerObjectives.SetObjectiveText("Press 'X' to pick up the torch", true);
                                }
                                else
                                {
                                    string interactKeyString = GetPlayerKey("Interact", playerNumber).Key.ToString();
                                    if (interactKeyString.Equals("None"))
                                    {
                                        interactKeyString = GetPlayerKey("Interact", playerNumber).gamepadButtonInUse.ToString();
                                    }
                                    triggerObjectives.playerObjectives.SetObjectiveText("Press '" + interactKeyString + "' to pick up the torch", true);
                                }
                                if (OculusManager.isOculusEnabled)
                                {
                                    triggerObjectives.oculusTutorialPrompts.EnableOne(6);
                                    triggerObjectives.oculusTutorialPrompts.SetOculusPromptString(triggerObjectives.playerObjectives.playerObjectives, 6);
                                }
                            }
                            if (triggerObjectives.isStillTutorial && triggerObjectives.drawInteraction && !triggerObjectives.drawOpened)
                            {
                                if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                                {
                                    triggerObjectives.playerObjectives.SetObjectiveText("Press 'X' to open a drawer", true);
                                }
                                else
                                {
                                    string interactKeyString = GetPlayerKey("Interact", playerNumber).Key.ToString();
                                    if (interactKeyString.Equals("None"))
                                    {
                                        interactKeyString = GetPlayerKey("Interact", playerNumber).gamepadButtonInUse.ToString();
                                    }
                                    triggerObjectives.playerObjectives.SetObjectiveText("Press '" + interactKeyString + "' to open a drawer", true);
                                }
                                if (OculusManager.isOculusEnabled)
                                {
                                    triggerObjectives.oculusTutorialPrompts.EnableOne(7);
                                    triggerObjectives.oculusTutorialPrompts.SetOculusPromptString(triggerObjectives.playerObjectives.playerObjectives, 7);
                                }
                            }
                            if (triggerObjectives.itemPickedUp && !triggerObjectives.doorTrigger && triggerObjectives.isStillTutorial && !triggerObjectives.turnedTorchOn)
                            {
                                if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                                {
                                    triggerObjectives.playerObjectives.SetObjectiveText("Press 'Right Trigger' to turn on torch", true);
                                }
                                else
                                {
                                    string useItemKeyString = GetPlayerKey("UseItem", playerNumber).Key.ToString();
                                    if (useItemKeyString.Equals("None"))
                                    {
                                        useItemKeyString = GetPlayerKey("UseItem", playerNumber).gamepadButtonInUse.ToString();
                                    }
                                    triggerObjectives.playerObjectives.SetObjectiveText("Press '" + useItemKeyString + "' to turn on torch", true);
                                }
                                if (GetPlayerKey("UseItem", playerNumber).JustPressed() || GetPlayerTriggerStateIfUsingController("Right", playerNumber, true))
                                {
                                    triggerObjectives.SetToNone();
                                    triggerObjectives.itemPickedUp = false;
                                }
                            }
                        }
                        if (!triggerObjectives.isStillTutorial && OculusManager.isOculusEnabled)
                        {
                            triggerObjectives.SetOculusPromptsToNone();
                        }
                        if (triggerObjectives.isStillTutorial && triggerObjectives.fuseInteraction)
                        {
                            if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                            {
                                triggerObjectives.playerObjectives.SetObjectiveText("Press 'X' to pick up the fuse", true);
                            }
                            else
                            {
                                string interactKeyString = GetPlayerKey("Interact", playerNumber).Key.ToString();
                                if (interactKeyString.Equals("None"))
                                {
                                    interactKeyString = GetPlayerKey("Interact", playerNumber).gamepadButtonInUse.ToString();
                                }
                                triggerObjectives.playerObjectives.SetObjectiveText("Press '" + interactKeyString + "' to pick up the fuse", true);
                            }
                            if (OculusManager.isOculusEnabled)
                            {
                                triggerObjectives.oculusTutorialPrompts.EnableOne(0);
                                triggerObjectives.oculusTutorialPrompts.SetOculusPromptString(triggerObjectives.playerObjectives.playerObjectives, 0);
                            }
                        }
                        if (triggerObjectives.isStillTutorial && triggerObjectives.noteInteraction)
                        {
                            if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                            {
                                triggerObjectives.playerObjectives.SetObjectiveText("Press 'X' to pick up the note", true);
                            }
                            else
                            {
                                string interactKeyString = GetPlayerKey("Interact", playerNumber).Key.ToString();
                                if (interactKeyString.Equals("None"))
                                {
                                    interactKeyString = GetPlayerKey("Interact", playerNumber).gamepadButtonInUse.ToString();
                                }
                                triggerObjectives.playerObjectives.SetObjectiveText("Press '" + interactKeyString + "' to pick up the note", true);
                            }
                            if (OculusManager.isOculusEnabled)
                            {
                                triggerObjectives.oculusTutorialPrompts.EnableOne(1);
                                triggerObjectives.oculusTutorialPrompts.SetOculusPromptString(triggerObjectives.playerObjectives.playerObjectives, 1);
                            }
                        }
                        if (triggerObjectives.isStillTutorial && triggerObjectives.doorLockedTrigger)
                        {
                            if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                            {
                                triggerObjectives.playerObjectives.SetObjectiveText("Press 'X' on the door Lock to unlock the door", true);
                            }
                            else
                            {
                                string interactKeyString = GetPlayerKey("Interact", playerNumber).Key.ToString();
                                if (interactKeyString.Equals("None"))
                                {
                                    interactKeyString = GetPlayerKey("Interact", playerNumber).gamepadButtonInUse.ToString();
                                }
                                triggerObjectives.playerObjectives.SetObjectiveText("Press '" + interactKeyString + "' on the door Lock to unlock the door", true);
                            }
                            if (OculusManager.isOculusEnabled)
                            {
                                triggerObjectives.oculusTutorialPrompts.EnableOne(5);
                                triggerObjectives.oculusTutorialPrompts.SetOculusPromptString(triggerObjectives.playerObjectives.playerObjectives, 5);
                            }
                        }
                        if (!triggerObjectives.doorTrigger && triggerObjectives.isStillTutorial && triggerObjectives.turnedTorchOn && !triggerObjectives.searchRoom)
                        {
                            triggerObjectives.playerObjectives.SetObjectiveText("Use the torch to search the room", true);
                            triggerObjectives.timer += Time.deltaTime;
                            if (triggerObjectives.timer >= 3f)
                            {
                                triggerObjectives.SetToNone();
                                triggerObjectives.timer = 0f;
                                triggerObjectives.searchRoom = true;
                            }
                        }
                        if (!triggerObjectives.isStillTutorial && !triggerObjectives.pitfall && !triggerObjectives.inventoryScrollOnce)
                        {
                            triggerObjectives.scrollTimer += Time.deltaTime;
                        }
                        if (triggerObjectives.isStillTutorial && triggerObjectives.exitBed && !triggerObjectives.exitedBed)
                        {
                            if (triggerObjectives.detectRoom.GetRoom.PrimaryRegion == PrimaryRegionType.CrewDeck)
                            {
                                if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                                {
                                    triggerObjectives.playerObjectives.SetObjectiveText("Move backwards to exit bed and stop hiding", true);
                                }
                                else
                                {
                                    string backKeyString = GetPlayerKey("Back", playerNumber).Key.ToString();
                                    if (backKeyString.Equals("None"))
                                    {
                                        backKeyString = GetPlayerKey("Back", playerNumber).gamepadButtonInUse.ToString();
                                    }
                                    triggerObjectives.playerObjectives.SetObjectiveText("Press '" + backKeyString + "' to move backwards and stop hiding", true);
                                }
                            }
                            else if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                            {
                                triggerObjectives.playerObjectives.SetObjectiveText("Move backwards to exit shelves and stop hiding", true);
                            }
                            else
                            {
                                string backKeyString = GetPlayerKey("Back", playerNumber).Key.ToString();
                                if (backKeyString.Equals("None"))
                                {
                                    backKeyString = GetPlayerKey("Back", playerNumber).gamepadButtonInUse.ToString();
                                }
                                triggerObjectives.playerObjectives.SetObjectiveText("Press '" + backKeyString + "' to move backwards and stop hiding", true);
                            }
                            if (OculusManager.isOculusEnabled)
                            {
                                triggerObjectives.oculusTutorialPrompts.EnableOne(9);
                                triggerObjectives.oculusTutorialPrompts.SetOculusPromptString(triggerObjectives.playerObjectives.playerObjectives, 9);
                            }
                        }
                        if (!triggerObjectives.pitfall && !triggerObjectives.inventoryScrollOnce && PlayerInventory(triggerObjectives.newPlayerClass).ItemCount >= 2)
                        {
                            triggerObjectives.scrollHint = true;
                            if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                            {
                                triggerObjectives.playerObjectives.SetObjectiveText("Use DPad Left/right to switch items", true);
                            }
                            else
                            {
                                triggerObjectives.playerObjectives.SetObjectiveText("Use the Mouse wheel to switch items", true);
                            }
                            triggerObjectives.timer += Time.deltaTime;
                            if (triggerObjectives.timer >= 3.5f)
                            {
                                triggerObjectives.SetToNone();
                                triggerObjectives.timer = 0f;
                                triggerObjectives.scrollHint = false;
                                triggerObjectives.inventoryScrollOnce = true;
                            }
                            if (Input.GetAxis("Mouse ScrollWheel") < 0f || Input.GetAxis("Mouse ScrollWheel") > 0f)
                            {
                                triggerObjectives.SetToNone();
                                triggerObjectives.timer = 0f;
                                triggerObjectives.scrollHint = false;
                                triggerObjectives.inventoryScrollOnce = true;
                            }
                        }
                        if (!triggerObjectives.isStillTutorial && !triggerObjectives.pitfall && !triggerObjectives.scrollHint)
                        {
                            if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                            {
                                if (triggerObjectives.cargoJump && triggerObjectives.cargoJumpOnce)
                                {
                                    triggerObjectives.playerObjectives.SetObjectiveText("Press 'A' to climb to container above", true);
                                }
                            }
                            else if (triggerObjectives.cargoJump && triggerObjectives.cargoJumpOnce)
                            {
                                string jumpKeyString = GetPlayerKey("Jump", playerNumber).Key.ToString();
                                if (jumpKeyString.Equals("None"))
                                {
                                    jumpKeyString = GetPlayerKey("Jump", playerNumber).gamepadButtonInUse.ToString();
                                }
                                triggerObjectives.playerObjectives.SetObjectiveText("Press '" + jumpKeyString + "' to climb to container above", true);
                            }
                        }
                        if (triggerObjectives.monsterThrow && OculusManager.isOculusEnabled)
                        {
                            triggerObjectives.oculusJumpUpPrompt.EnableOculusJumpPrompt();
                            if (GetPlayerKey("Jump", playerNumber).JustPressed())
                            {
                                triggerObjectives.oculusJumpUpPrompt.DisableOculusJumpPrompt();
                                triggerObjectives.monsterThrow = false;
                            }
                        }
                        if (triggerObjectives.newPlayerClass.fellInTrap || triggerObjectives.monsterThrow)
                        {
                            triggerObjectives.pitfall = true;
                            if (!triggerObjectives.jumpUpTrapOnce)
                            {
                                triggerObjectives.timer = 0f;
                                triggerObjectives.jumpUpTrapOnce = true;
                            }
                            if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                            {
                                triggerObjectives.playerObjectives.SetObjectiveText("Press 'A' to get up quickly", false);
                            }
                            else
                            {
                                triggerObjectives.playerObjectives.SetObjectiveText("Press the 'Space Bar' to get up quickly", false);
                            }
                            if (OculusManager.isOculusEnabled)
                            {
                                triggerObjectives.oculusJumpUpPrompt.EnableOculusJumpPrompt();
                            }
                            triggerObjectives.timer += Time.deltaTime;
                            if (triggerObjectives.timer >= 3.5f)
                            {
                                triggerObjectives.SetToNone();
                                triggerObjectives.oculusJumpUpPrompt.DisableOculusJumpPrompt();
                                triggerObjectives.newPlayerClass.fellInTrap = false;
                                triggerObjectives.monsterThrow = false;
                                triggerObjectives.pitfall = false;
                                triggerObjectives.timer = 0f;
                            }
                            if (GetPlayerKey("Jump", playerNumber).JustPressed())
                            {
                                triggerObjectives.SetToNone();
                                triggerObjectives.oculusJumpUpPrompt.DisableOculusJumpPrompt();
                                triggerObjectives.newPlayerClass.fellInTrap = false;
                                triggerObjectives.monsterThrow = false;
                                triggerObjectives.pitfall = false;
                                triggerObjectives.timer = 0f;
                            }
                        }
                        if (triggerObjectives.throwTimer.IsRunning && triggerObjectives.throwTimer.TimeElapsed > 1.5f)
                        {
                            triggerObjectives.SetToNone();
                            triggerObjectives.throwTimer.StopTimer();
                        }
                    }
                    else
                    {
                        triggerObjectives.SetToNone();
                        if (OculusManager.isOculusEnabled)
                        {
                            triggerObjectives.SetOculusPromptsToNone();
                        }
                    }
                    if (triggerObjectives.notes)
                    {
                        triggerObjectives.SetToNotes();
                        triggerObjectives.timer += Time.deltaTime;
                        if (triggerObjectives.timer >= 2.5f)
                        {
                            triggerObjectives.notes = false;
                            triggerObjectives.SetToNone();
                            triggerObjectives.timer = 0f;
                        }
                    }
                    if (TriggerObjectives.audioLogDuplicate)
                    {
                        triggerObjectives.SetToAudioLogsDuplicate();
                        triggerObjectives.timer += Time.deltaTime;
                        if (triggerObjectives.timer >= 2.5f)
                        {
                            TriggerObjectives.audioLogDuplicate = false;
                            triggerObjectives.SetToNone();
                            triggerObjectives.timer = 0f;
                        }
                    }
                    if (TriggerObjectives.audioLogFirst)
                    {
                        triggerObjectives.SetToAudioLogsFirst();
                        triggerObjectives.timer += Time.deltaTime;
                        if (triggerObjectives.timer >= 2.5f)
                        {
                            TriggerObjectives.audioLogFirst = false;
                            triggerObjectives.SetToNone();
                            triggerObjectives.timer = 0f;
                        }
                    }
                }
            }

            private static void HookTriggerObjectivesSetToNone(On.TriggerObjectives.orig_SetToNone orig, TriggerObjectives triggerObjectives)
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    int playerNumber = PlayerNumber(triggerObjectives.newPlayerClass.GetInstanceID());
                    if (ModSettings.logDebugText)
                    {
                        Debug.Log("TriggerObjectives.SetToNone playerNumber is " + playerNumber);
                    }
                    triggerObjectives.playerObjectives.SetObjectiveText(" ", false);
                }
            }

            private static void HookTriggerObjectivesSetToThrow(On.TriggerObjectives.orig_SetToThrow orig, TriggerObjectives triggerObjectives)
            {
                int playerNumber = PlayerNumber(triggerObjectives.newPlayerClass.GetInstanceID());
                if (ModSettings.logDebugText)
                {
                    Debug.Log("TriggerObjectives.SetToThrow playerNumber is " + playerNumber);
                }
                if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                {
                    triggerObjectives.playerObjectives.SetObjectiveText("Press 'Y' to drop items", true);
                }
                else
                {
                    string dropItemKeyString = GetPlayerKey("DropItem", playerNumber).Key.ToString();
                    if (dropItemKeyString.Equals("None"))
                    {
                        dropItemKeyString = GetPlayerKey("DropItem", playerNumber).gamepadButtonInUse.ToString();
                    }
                    triggerObjectives.playerObjectives.SetObjectiveText("'" + dropItemKeyString + "' to drop items", true);
                }
                triggerObjectives.throwTimer.ResetTimer();
                triggerObjectives.throwTimer.StartTimer();
            }

            private static void HookTriggerObjectivesTutorialBedText(On.TriggerObjectives.orig_TutorialBedText orig, TriggerObjectives triggerObjectives)
            {
                if (triggerObjectives.newPlayerClass != null)
                {
                    int playerNumber = PlayerNumber(triggerObjectives.newPlayerClass.GetInstanceID());
                    if (ModSettings.logDebugText)
                    {
                        Debug.Log("TriggerObjectives.TutorialBedText playerNumber is " + playerNumber);
                    }
                    if (triggerObjectives.detectRoom.GetRoom.PrimaryRegion != PrimaryRegionType.None)
                    {
                        if (triggerObjectives.detectRoom.GetRoom.PrimaryRegion == PrimaryRegionType.CrewDeck)
                        {
                            if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                            {
                                triggerObjectives.playerObjectives.SetObjectiveText("Press 'B' and move forward to hide under the bed", true);
                            }
                            else
                            {
                                string crouchKeyString = GetPlayerKey("Crouch", playerNumber).Key.ToString();
                                if (crouchKeyString.Equals("None"))
                                {
                                    crouchKeyString = GetPlayerKey("Crouch", playerNumber).gamepadButtonInUse.ToString();
                                }
                                triggerObjectives.playerObjectives.SetObjectiveText("Press '" + crouchKeyString + "' and move forward to hide under the bed", true);
                            }
                        }
                        else if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                        {
                            triggerObjectives.playerObjectives.SetObjectiveText("Press 'B' and move forward to hide under the shelves", true);
                        }
                        else
                        {
                            string crouchKeyString = GetPlayerKey("Crouch", playerNumber).Key.ToString();
                            if (crouchKeyString.Equals("None"))
                            {
                                crouchKeyString = GetPlayerKey("Crouch", playerNumber).gamepadButtonInUse.ToString();
                            }
                            triggerObjectives.playerObjectives.SetObjectiveText("Press '" + crouchKeyString + "' and move forward to hide under the shelves", true);
                        }
                    }
                    if (OculusManager.isOculusEnabled)
                    {
                        triggerObjectives.oculusTutorialPrompts.EnableOne(3);
                        triggerObjectives.oculusTutorialPrompts.SetOculusPromptString(triggerObjectives.playerObjectives.playerObjectives, 3);
                    }
                }
                else
                {
                    Debug.Log("TriggerObjectives.TutorialBedText playerNumber is null");
                }
            }

            private static void HookTriggerObjectivesTutorialCrouchText(On.TriggerObjectives.orig_TutorialCrouchText orig, TriggerObjectives triggerObjectives)
            {
                int playerNumber = PlayerNumber(triggerObjectives.newPlayerClass.GetInstanceID());
                if (ModSettings.logDebugText)
                {
                    Debug.Log("TriggerObjectives.TutorialCrouchText playerNumber is " + playerNumber);
                }
                if (triggerObjectives.detectRoom.GetRoom.PrimaryRegion != PrimaryRegionType.None)
                {
                    if (triggerObjectives.detectRoom.GetRoom.PrimaryRegion == PrimaryRegionType.CrewDeck)
                    {
                        if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                        {
                            triggerObjectives.playerObjectives.SetObjectiveText("Press 'B' then move forward to hide under the table", true);
                        }
                        else
                        {
                            string crouchKeyString = GetPlayerKey("Crouch", playerNumber).Key.ToString();
                            if (crouchKeyString.Equals("None"))
                            {
                                crouchKeyString = GetPlayerKey("Crouch", playerNumber).gamepadButtonInUse.ToString();
                            }
                            triggerObjectives.playerObjectives.SetObjectiveText("Press '" + crouchKeyString + "' then move forward to hide under the table", true);
                        }
                    }
                    else if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                    {
                        triggerObjectives.playerObjectives.SetObjectiveText("Press 'B' then move forward to hide inside the cupboard", true);
                    }
                    else
                    {
                        string crouchKeyString = GetPlayerKey("Crouch", playerNumber).Key.ToString();
                        if (crouchKeyString.Equals("None"))
                        {
                            crouchKeyString = GetPlayerKey("Crouch", playerNumber).gamepadButtonInUse.ToString();
                        }
                        triggerObjectives.playerObjectives.SetObjectiveText("Press '" + crouchKeyString + "' then move forward to hide inside the cupboard", true);
                    }
                }
                if (OculusManager.isOculusEnabled)
                {
                    triggerObjectives.oculusTutorialPrompts.EnableOne(2);
                    triggerObjectives.oculusTutorialPrompts.SetOculusPromptString(triggerObjectives.playerObjectives.playerObjectives, 2);
                }
            }

            private static void HookTriggerObjectivesTutorialDoorText(On.TriggerObjectives.orig_TutorialDoorText orig, TriggerObjectives triggerObjectives)
            {
                int playerNumber = PlayerNumber(triggerObjectives.newPlayerClass.GetInstanceID());
                if (ModSettings.logDebugText)
                {
                    Debug.Log("TriggerObjectives.TutorialDoorText playerNumber is " + playerNumber);
                }
                if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                {
                    triggerObjectives.playerObjectives.SetObjectiveText("Press 'X' to open the door", true);
                }
                else
                {
                    string interactKeyString = GetPlayerKey("Interact", playerNumber).Key.ToString();
                    if (interactKeyString.Equals("None"))
                    {
                        interactKeyString = GetPlayerKey("Interact", playerNumber).gamepadButtonInUse.ToString();
                    }
                    triggerObjectives.playerObjectives.SetObjectiveText("Press '" + interactKeyString + "' to open the door", true);
                }
                if (OculusManager.isOculusEnabled)
                {
                    if (!triggerObjectives.startRoomDoor)
                    {
                        triggerObjectives.oculusTutorialPrompts.EnableOne(4);
                        triggerObjectives.oculusTutorialPrompts.SetOculusPromptString(triggerObjectives.playerObjectives.playerObjectives, 4);
                    }
                    else
                    {
                        triggerObjectives.oculusTutorialPrompts.EnableOne(5);
                        triggerObjectives.oculusTutorialPrompts.SetOculusPromptString(triggerObjectives.playerObjectives.playerObjectives, 5);
                    }
                }
            }

            private static void HookTriggerObjectivesTutorialRunText(On.TriggerObjectives.orig_TutorialRunText orig, TriggerObjectives triggerObjectives)
            {
                int playerNumber = PlayerNumber(triggerObjectives.newPlayerClass.GetInstanceID());
                if (ModSettings.logDebugText)
                {
                    Debug.Log("TriggerObjectives.TutorialRunText playerNumber is " + playerNumber);
                }
                if (InputHelper.IsGamepadConnected && ControllerCheck.enableControllerSupport)
                {
                    triggerObjectives.playerObjectives.SetObjectiveText("Hold 'Left Trigger' to run", true);
                }
                else
                {
                    string interactKeyString = GetPlayerKey("Sprint", playerNumber).Key.ToString();
                    if (interactKeyString.Equals("None"))
                    {
                        //interactKeyString = GetPlayerKey("Sprint", playerNumber).gamepadButtonInUse.ToString();
                        interactKeyString = "Left Trigger";
                    }
                    triggerObjectives.playerObjectives.SetObjectiveText("Hold '" + KeyBinds.SprintKeyBind.Key.ToString() + "' to run", true);
                }
                if (OculusManager.isOculusEnabled)
                {
                    triggerObjectives.oculusTutorialPrompts.EnableOne(8);
                    triggerObjectives.oculusTutorialPrompts.SetOculusPromptString(triggerObjectives.playerObjectives.playerObjectives, 8);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Trolley

            private static void HookTrolleyFixedUpdate(On.Trolley.orig_FixedUpdate orig, Trolley trolley)
            {
                if (trolley.grabbed)
                {
                    int playerNumber = PlayerNumber(trolley.player.GetInstanceID());
                    Vector3 vector = ((MonoBehaviour)trolley).transform.up * trolley.trolleyTorque * Time.fixedDeltaTime * ((!(GetPlayerKey("Back", playerNumber).IsDown() || GetPlayerAxisValue("Y", playerNumber) < -0.1f)) ? 1f : -1f);
                    if (GetPlayerKey("Left", playerNumber).IsDown() || GetPlayerAxisValue("X", playerNumber) < -0.1f)
                    {
                        trolley.rigidBody.AddTorque(-vector);
                    }
                    if (GetPlayerKey("Right", playerNumber).IsDown() || GetPlayerAxisValue("X", playerNumber) > 0.1f)
                    {
                        trolley.rigidBody.AddTorque(vector);
                    }
                    if (GetPlayerKey("Forward", playerNumber).IsDown() || GetPlayerAxisValue("Y", playerNumber) > 0.1f)
                    {
                        trolley.rigidBody.AddForce(trolley.interact.target.forward * trolley.trolleyForce * Time.fixedDeltaTime);
                    }
                    if (GetPlayerKey("Back", playerNumber).IsDown() || GetPlayerAxisValue("Y", playerNumber) < -0.1f)
                    {
                        trolley.rigidBody.AddForce(-trolley.interact.target.forward * trolley.trolleyForce * Time.fixedDeltaTime);
                    }
                    if (GetPlayerKey("Interact", playerNumber).IsDown())
                    {
                        trolley.interact.GetComponentInChildren<HandGrabIK>().ShouldRelease = true;
                    }
                    trolley.ScaleTheNavBox();
                    if (trolley.player.IsDying || trolley.player.IsStunned)
                    {
                        lastPlayerSentMessage = trolley.player;
                        if (ModSettings.logDebugText)
                        {
                            Debug.Log("Updating lastPlayerSentMessage: " + PlayerNumber(lastPlayerSentMessage.GetInstanceID()) + "\n" + new StackTrace().ToString() + "\n-----");
                        }
                        trolley.OnHandRelease();
                        trolley.ReleaseIKS();
                    }
                }
            }


            private static void HookTrolleyLateUpdate(On.Trolley.orig_LateUpdate orig, Trolley trolley)
            {
                if (trolley.interact != null)
                {
                    if (trolley.rollingVolume != null)
                    {
                        trolley.rollingVolume.fadeValue = Mathf.Lerp(0f, 1f, Mathf.Max(trolley.rigidBody.velocity.magnitude / 2f, ((MonoBehaviour)trolley).GetComponent<Rigidbody>().angularVelocity.magnitude));
                        if ((double)trolley.rollingVolume.fadeValue > 0.1)
                        {
                            trolley.rattletime += Time.deltaTime;
                            if (trolley.rattletime > trolley.timebetweenrattles)
                            {
                                trolley.rattletime = 0f;
                                AudioSystem.PlaySound("Noises/Submarine/Trolley/Rattle", trolley.grabSource.transform, trolley.grabSource);
                            }
                        }
                    }
                    if (((MonoBehaviour)trolley).transform.localEulerAngles.x > trolley.toleranceResetValue || ((MonoBehaviour)trolley).transform.localEulerAngles.x < -trolley.toleranceResetValue || ((MonoBehaviour)trolley).transform.localEulerAngles.z > trolley.toleranceResetValue || ((MonoBehaviour)trolley).transform.localEulerAngles.z < -trolley.toleranceResetValue)
                    {
                        ((MonoBehaviour)trolley).transform.localRotation = Quaternion.Euler(0f, ((MonoBehaviour)trolley).transform.localRotation.eulerAngles.y, 0f);
                    }
                    if (!trolley.subEscape)
                    {
                        trolley.ElevationCheck();
                    }
                    Vector3 motion = trolley.interact.target.position - trolley.player.transform.position;
                    if (!trolley.atTrolleyInteraction)
                    {
                        Vector3 vector = motion.normalized * Time.deltaTime * trolley.speedToInteractionPoint;
                        if (motion.magnitude < vector.magnitude)
                        {
                            trolley.controller.Move(motion);
                            trolley.atTrolleyInteraction = true;
                        }
                        else
                        {
                            trolley.controller.Move(motion.normalized * Time.deltaTime * trolley.speedToInteractionPoint);
                        }
                    }
                    else
                    {
                        trolley.controller.Move(motion);
                    }
                    if (trolley.subEscape)
                    {
                        trolley.BlockingCode();
                    }
                    trolley.player.transform.rotation = Quaternion.RotateTowards(trolley.player.transform.rotation, Quaternion.Euler(0f, trolley.interact.target.rotation.eulerAngles.y, 0f), Time.deltaTime * trolley.speedToRotationPoint);
                }
            }

            private static void HookTrolleyOnHandGrab(On.Trolley.orig_OnHandGrab orig, Trolley trolley)
            {
                if (trolley.interact != null)
                {
                    Debug.Log("Setting trolley's player to player number " + PlayerNumber(lastPlayerSentMessage.GetInstanceID()));
                    trolley.custom = lastPlayerSentMessage.mouseLookCustom;
                    trolley.player = lastPlayerSentMessage;
                    trolley.controller = trolley.player.GetComponent<CharacterController>();
                    trolley.motor = trolley.player.GetComponent<PlayerMotor>();

                    trolley.custom.UnlockPlayerHead();
                    trolley.custom.headLock = false;
                    trolley.player.grabbedTrolley = true;
                    trolley.player.playerState = NewPlayerClass.PlayerState.Push;
                    trolley.trolleyPlayerClimbingBox.gameObject.SetActive(false);
                    Transform transform = trolley.interact.transform.FindChild("HandIK");
                    transform.parent = ((MonoBehaviour)trolley).transform;
                    if (trolley.interact.centreOfMass != null)
                    {
                        trolley.rigidBody.centerOfMass = trolley.interact.centreOfMass.transform.localPosition;
                    }
                    transform.parent = trolley.interact.transform;
                    PlayerInventory(trolley.player).hideItem = true;
                    trolley.grabbed = true;
                    trolley.rollingSource.Stop();
                    trolley.grabSource.Stop();
                    if (!useLegacyAudio)
                    {
                        VirtualAudioSource virtualAudioSource1 = trolley.rollingSource.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource1 != null)
                        {
                            virtualAudioSource1.Stop();
                        }
                        else if (ModSettings.logDebugText)
                        {
                            Debug.Log("VAS is null 1!\n" + new StackTrace().ToString());
                        }
                        VirtualAudioSource virtualAudioSource2 = trolley.grabSource.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource2 != null)
                        {
                            virtualAudioSource2.Stop();
                        }
                        else if (ModSettings.logDebugText)
                        {
                            Debug.Log("VAS is null 2!\n" + new StackTrace().ToString());
                        }
                    }
                    AudioSystem.PlaySound("Noises/Submarine/Trolley/Rolling", trolley.rollingSource);
                    AudioSystem.PlaySound("Noises/Submarine/Trolley/Grab", trolley.grabSource);
                    trolley.rollingVolume = trolley.rollingSource.GetComponent<VolumeController>();
                    trolley.interact.changeShader.Deactivate();
                    trolley.interact.changeShader.enabled = false;
                    trolley.motor.disableMove = true;
                }
            }

            private static void HookTrolleyOnHandRelease(On.Trolley.orig_OnHandRelease orig, Trolley trolley)
            {
                if (trolley.interact != null)
                {
                    trolley.player = lastPlayerSentMessage;
                    ((MonoBehaviour)trolley).transform.localRotation = Quaternion.Euler(0f, ((MonoBehaviour)trolley).transform.localRotation.eulerAngles.y, 0f);
                    trolley.interact.changeShader.enabled = true;
                    trolley.player.GetComponentInChildren<PlayerClipCamera>().enabled = true;
                    trolley.custom.LockPlayerHead();
                    trolley.custom.headLock = true;
                    trolley.player.grabbedTrolley = false;
                    trolley.player.HandRelease();
                    trolley.player.playerState = NewPlayerClass.PlayerState.Standing;
                    trolley.interact = null;
                    trolley.grabbed = false;
                    trolley.rigidBody.ResetCenterOfMass();
                    PlayerInventory(trolley.player).hideItem = false;
                    trolley.rollingSource.Stop();
                    if (!useLegacyAudio)
                    {
                        VirtualAudioSource virtualAudioSource = trolley.rollingSource.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource != null)
                        {
                            virtualAudioSource.Stop();
                        }
                        else if (ModSettings.logDebugText)
                        {
                            Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                        }
                    }
                    trolley.rattletime = 0f;
                    AudioSystem.PlaySound("Noises/Submarine/Trolley/Release", trolley.grabSource);
                    trolley.trolleyPlayerClimbingBox.gameObject.SetActive(true);
                    trolley.motor.disableMove = false;
                }
            }

            private static void HookTrolleyOnTriggerEnter(On.Trolley.orig_OnTriggerEnter orig, Trolley trolley, Collider other)
            {
                if (other.gameObject.name == "FuelPumpTrigger")
                {
                    lastPlayerSentMessage = trolley.player;
                    if (ModSettings.logDebugText)
                    {
                        Debug.Log("Updating lastPlayerSentMessage: " + PlayerNumber(lastPlayerSentMessage.GetInstanceID()) + "\n" + new StackTrace().ToString() + "\n-----");
                    }
                    trolley.OnHandRelease();
                    trolley.ReleaseIKS();
                    trolley.slerpFuelTrolley.StartTrolleySlerp();
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @TrolleyInteraction

            private static bool HookTrolleyInteractionIsConditionMet(On.TrolleyInteraction.orig_IsConditionMet orig, TrolleyInteraction trolleyInteraction)
            {
                if (trolleyInteraction.trolley.grabbed)
                {
                    return false;
                }
                else
                {
                    Vector3 position = lastPlayerCheckingInteractableConditions.transform.position;
                    Vector3 position2 = trolleyInteraction.handIK.position;
                    position2.y = position.y;
                    return Vector3.Angle(trolleyInteraction.handIK.forward, (position2 - position).normalized) < 90f;
                }
            }

            private static void HookTutorialLockerModelSwapUpdate(On.TutorialLockerModelSwap.orig_Update orig, TutorialLockerModelSwap tutorialLockerModelSwap)
            {
                // Leave empty so that the locker is not locked off when the References Player moves.
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @TV

            private static void HookTVTurnOffSound(On.TV.orig_TurnOffSound orig, TV tV)
            {
                tV.source.Stop();
                tV.source.loop = false;
                VirtualAudioSource virtualAudioSource = tV.source.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    virtualAudioSource.Stop();
                    virtualAudioSource.loop = false;
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                }
                AudioSystem.PlaySound("Noises/Enviro/TV/OnOff/Turn Off", tV.source.transform, tV.source);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @VolumeController

            private static float VolumeControllerCalculateVolume(VolumeController volumeController, float _fadeValue)
            {
                volumeController.fadeValue = _fadeValue;
                volumeController.vol = Ducking.GetCurrentDuckValue(volumeController.lib.duckTag) * volumeController.occlusion * volumeController.gameVolume * volumeController.startVolume * volumeController.fadeValue * volumeController.gameEndFade * ((volumeController.lib.type != AudioLibrary.SFXType.Music) ? AudioSystem.SFXVolume : AudioSystem.MusicVolume);
                volumeController.vol *= 0.9f;
                /*
                VirtualAudioSource virtualAudioSource = volumeController.source.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null && virtualAudioSource.mySource != null)
                {
                    volumeController.source = virtualAudioSource.mySource;
                }
                */

                if (volumeController.source.volume != volumeController.vol)
                {
                    volumeController.source.volume = volumeController.vol;
                    //Debug.Log("Printing VolumeController.CalculateVolume (Custom) information: Virtual Audio Source volume: " + volumeController.source.volume + ". ORIGINAL AUDIO SOURCE: Current Duck Value is " + Ducking.GetCurrentDuckValue(volumeController.lib.duckTag) + ", Occlusion is " + volumeController.occlusion + ", Game Volume is " + volumeController.gameVolume + ", Start Volume is " + volumeController.startVolume + ", Fade Value is " + volumeController.fadeValue + ", Game End Fade is " + volumeController.gameEndFade + " and library type check gives " + ((volumeController.lib.type != AudioLibrary.SFXType.Music) ? AudioSystem.SFXVolume : AudioSystem.MusicVolume) + ". Distance from original source to closest player is " + Vector3.Distance(newPlayerClasses[ClosestPlayerToThis(volumeController.transform.position)].transform.position, volumeController.transform.position) + ". Distance from volumeController.source to closest player to original source is " + Vector3.Distance(newPlayerClasses[ClosestPlayerToThis(volumeController.transform.position)].transform.position, volumeController.source.transform.position));
                }
                return volumeController.vol;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @VolumentricSphere

            private static void HookVolumetricSphereOnEnable(On.VolumetricSphere.orig_OnEnable orig, VolumetricSphere volumetricSphere)
            {
                foreach (NewPlayerClass newPlayerClass in newPlayerClasses)
                {
                    Camera playerCamera = PlayerCamera(newPlayerClass);
                    if (playerCamera.depthTextureMode == DepthTextureMode.None)
                    {
                        playerCamera.depthTextureMode = DepthTextureMode.Depth;
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @WalkieTalkie

            private static void HookWalkieTalkieOnEndReceive(On.WalkieTalkie.orig_OnEndReceive orig, WalkieTalkie walkieTalkie)
            {
                orig.Invoke(walkieTalkie);
                VirtualAudioSource virtualAudioSource = walkieTalkie.source.gameObject.GetComponent<VirtualAudioSource>();
                if (virtualAudioSource != null)
                {
                    virtualAudioSource.Pause();
                }
                else if (ModSettings.logDebugText)
                {
                    Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                }
            }

            private static void HookWalkieTalkiePlaySound(On.WalkieTalkie.orig_PlaySound orig, WalkieTalkie walkieTalkie)
            {
                if (walkieTalkie.receiveLED.On)
                {
                    if (walkieTalkie.currentClipName != walkieTalkie.staticLib)
                    {
                        walkieTalkie.source.loop = true;
                        AudioSystem.PlaySound(walkieTalkie.staticLib, walkieTalkie.source);
                        walkieTalkie.source.Play();
                        VirtualAudioSource virtualAudioSource = walkieTalkie.source.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource != null)
                        {
                            virtualAudioSource.Play();
                        }
                        else
                        {
                            virtualAudioSource = AddVirtualAudioSourceToAudioSource(ref walkieTalkie.source);
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                            }
                            //virtualAudioSource = walkieTalkie.source.gameObject.GetComponent<VirtualAudioSource>();
                            if (virtualAudioSource != null)
                            {
                                virtualAudioSource.Play();
                            }
                            else if (ModSettings.logDebugText)
                            {
                                Debug.Log("VAS is still null 1!\n" + new StackTrace().ToString());
                            }
                        }
                        walkieTalkie.currentClipName = walkieTalkie.staticLib;
                    }
                    else
                    {
                        walkieTalkie.source.Play();
                        VirtualAudioSource virtualAudioSource = walkieTalkie.source.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource != null)
                        {
                            virtualAudioSource.Play();
                        }
                        else
                        {
                            virtualAudioSource = AddVirtualAudioSourceToAudioSource(ref walkieTalkie.source);
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                            }
                            //virtualAudioSource = walkieTalkie.source.gameObject.GetComponent<VirtualAudioSource>();
                            if (virtualAudioSource != null)
                            {
                                virtualAudioSource.Play();
                            }
                            else if (ModSettings.logDebugText)
                            {
                                Debug.Log("VAS is still null 2!\n" + new StackTrace().ToString());
                            }
                        }
                    }
                }
                else if (walkieTalkie.currentClipName != walkieTalkie.useLib)
                {
                    walkieTalkie.source.loop = false;
                    AudioSystem.PlaySound(walkieTalkie.useLib, walkieTalkie.source);
                    walkieTalkie.source.Play();
                    VirtualAudioSource virtualAudioSource = walkieTalkie.source.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Play();
                    }
                    else
                    {
                        virtualAudioSource = AddVirtualAudioSourceToAudioSource(ref walkieTalkie.source);
                        if (ModSettings.logDebugText)
                        {
                            Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                        }
                        //virtualAudioSource = walkieTalkie.source.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource != null)
                        {
                            virtualAudioSource.Play();
                        }
                        else if (ModSettings.logDebugText)
                        {
                            Debug.Log("VAS is still null 3!\n" + new StackTrace().ToString());
                        }
                    }
                    walkieTalkie.currentClipName = walkieTalkie.useLib;
                }
                else
                {
                    walkieTalkie.source.Play();
                    VirtualAudioSource virtualAudioSource = walkieTalkie.source.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Play();
                    }
                    else
                    {
                        virtualAudioSource = AddVirtualAudioSourceToAudioSource(ref walkieTalkie.source);
                        if (ModSettings.logDebugText)
                        {
                            Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                        }
                        //virtualAudioSource = walkieTalkie.source.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource != null)
                        {
                            virtualAudioSource.Play();
                        }
                        else if (ModSettings.logDebugText)
                        {
                            Debug.Log("VAS is still null 4!\n" + new StackTrace().ToString());
                        }
                    }
                }
            }



            /*----------------------------------------------------------------------------------------------------*/
            // @WallClimb

            private static void HookWallClimb()
            {
                On.WallClimb.OnInteract += new On.WallClimb.hook_OnInteract(HookWallClimbOnInteract);
                On.WallClimb.OnStartFixedAnimation += new On.WallClimb.hook_OnStartFixedAnimation(HookWallClimbOnStartFixedAnimation);
                On.WallClimb.OnFinishFixedAnimation += new On.WallClimb.hook_OnFinishFixedAnimation(HookWallClimbOnFinishFixedAnimation);
            }

            private static void HookWallClimbOnInteract(On.WallClimb.orig_OnInteract orig, WallClimb wallClimb)
            {
                Debug.Log(new StackTrace().ToString());
                WallClimb.npc = lastPlayerSentMessage;
                WallClimb.inv = PlayerInventory(WallClimb.npc);
                orig.Invoke(wallClimb);
            }

            private static void HookWallClimbOnStartFixedAnimation(On.WallClimb.orig_OnStartFixedAnimation orig, WallClimb wallClimb)
            {
                Debug.Log(new StackTrace().ToString());
                WallClimb.npc = lastPlayerSentMessage;
                WallClimb.playerAnim = WallClimb.npc.playerAnimator;
                WallClimb.playerMot = WallClimb.npc.Motor;
                WallClimb.humanBodyCapsColl = WallClimb.npc.transform.FindChild("c_humanBodyCapsule").GetComponent<Collider>();
                WallClimb.charCont = WallClimb.npc.controller;
                WallClimb.playerClipCamera = WallClimb.npc.GetComponentInChildren<PlayerClipCamera>();
                orig.Invoke(wallClimb);
            }

            private static void HookWallClimbOnFinishFixedAnimation(On.WallClimb.orig_OnFinishFixedAnimation orig, WallClimb wallClimb)
            {
                Debug.Log(new StackTrace().ToString());
                WallClimb.npc = lastPlayerSentMessage;
                WallClimb.inv = PlayerInventory(WallClimb.npc);
                WallClimb.playerAnim = WallClimb.npc.playerAnimator;
                WallClimb.playerMot = WallClimb.npc.Motor;
                WallClimb.humanBodyCapsColl = WallClimb.npc.transform.FindChild("c_humanBodyCapsule").GetComponent<Collider>();
                WallClimb.charCont = WallClimb.npc.controller;
                WallClimb.playerClipCamera = WallClimb.npc.GetComponentInChildren<PlayerClipCamera>();
                orig.Invoke(wallClimb);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Welder

            private static void HookWelderOnFinishItemAnimation(On.Welder.orig_OnFinishItemAnimation orig, Welder welder)
            {
                NewPlayerClass newPlayerClass = InventoryFromItemClass(welder.gameObject).newPlayerClass; // References.Player.GetComponent<NewPlayerClass>(); // Original code.
                if (newPlayerClass != null && newPlayerClass.specFix == PlayerAnimations.SpecificFix.WeldDoor)
                {
                    Door door = newPlayerClass.CurrentlyInteractingWith.GetComponentInParent<Door>();
                    if (door != null)
                    {
                        door.Unlock(true);
                        DoorBolt componentInChildren = door.GetComponentInChildren<DoorBolt>();
                        if (componentInChildren != null)
                        {
                            componentInChildren.DestroyBolt();
                        }
                        door.Toggle(0.05f, "Player");
                    }
                }
                welder.welding = false;
            }

            /*----------------------------------------------------------------------------------------------------*/
        }

    }
}
// ~End Of File