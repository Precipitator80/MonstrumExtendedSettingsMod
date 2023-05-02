// ~Beginning Of File
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using System.Collections;

namespace MonstrumExtendedSettingsMod
{

    public partial class ExtendedSettingsModScript
    {
        /*----------------------------------------------------------------------------------------------------*/
        // ~SparkyMode
        private static class SparkyMode
        {
            public static List<float> sparkyAggroTimers;
            public static List<GameObject> sparkyList;
            public static List<Monster> sparkyListMonsterComponents;
            public static List<string> sparkyState;
            private static List<Light[]> bruteSparkyEyes;
            private static List<Material[]> customSparkyEyes;
            public static float maxAggro;
            public static GameObject simpleSparkyGO;
            public static SkinnedMeshRenderer sparkyGlobalSMR;
            public static Transform armatureTransform;

            // #SparkyModeBeforeGenerationInitialisation
            public static void SparkyModeBeforeGenerationInitialisation()
            {

            }

            public static void SimpleSparkyModelTest(GameObject bruteSparkyGameObject)
            {
                // Create Simple Sparky
                simpleSparkyGO = Instantiate((GameObject)sparkyPrefab);
                simpleSparkyGO.SetActive(true);
                simpleSparkyGO.transform.SetParent(bruteSparkyGameObject.transform, false);
                SkinnedMeshRenderer sparkySkinnedMeshRenderer = simpleSparkyGO.GetComponentInChildren<SkinnedMeshRenderer>();
                sparkySkinnedMeshRenderer.enabled = true;
                /*
                Transform eyeTransform = RecursiveTransformSearch(simpleSparkyGO.transform, "Eye_Inner.L");//skinnedMeshRenderer.rootBone.FindChild("Eye_Outer.L");
                if (eyeTransform != null)
                {
                    MeshRenderer eyeMR = eyeTransform.gameObject.GetComponentInChildren<MeshRenderer>();
                    if (eyeMR != null)
                    {
                        if (eyeMR.material != null)
                        {
                            //eyeMR.material.EnableKeyword("_EMISSION");
                            eyeMR.material.SetColor("_EmissionColor", Color.red * 5f); // How to access Emission Color of a Material in Script? - Unshackled - https://answers.unity.com/questions/1019974/how-to-access-emission-color-of-a-material-in-scri.html - Accessed 12.01.2022
                            //DynamicGI.UpdateEnvironment();

                            //eyeMR.material.SetColor("_Emission", Color.red);
                            //DynamicGI.UpdateEnvironment();
                        }
                        else
                        {
                            Debug.Log("Could not find Sparky's eye's SMR's material");
                        }
                    }
                    else
                    {
                        Debug.Log("Could not find Sparky's eye's SMR");
                    }
                }
                else
                {
                    Debug.Log("Could not find Sparky's eye");
                }
                */
                Animator sparkyAnimator = simpleSparkyGO.GetComponent<Animator>();
                sparkyAnimator.enabled = false;

                Animator monsterAnimator = bruteSparkyGameObject.GetComponentInChildren<Animator>();
                monsterAnimator.runtimeAnimatorController = Instantiate(sparkyAnimatorOCPrefab);
                monsterAnimator.avatar = sparkyAnimator.avatar;
                Debug.Log("Does Brute use root motion? " + monsterAnimator.applyRootMotion);
                monsterAnimator.applyRootMotion = true;
                SkinnedMeshRenderer monsterSMR = bruteSparkyGameObject.GetComponentInChildren<SkinnedMeshRenderer>();

                monsterSMR.sharedMesh = sparkySkinnedMeshRenderer.sharedMesh; // I don't think it is recommended to edit anything using sharedMesh. Does this break other Brute models?
                monsterSMR.bones = sparkySkinnedMeshRenderer.bones;
                monsterSMR.material = sparkySkinnedMeshRenderer.material;
                monsterSMR.materials = sparkySkinnedMeshRenderer.materials;
                monsterSMR.rootBone = sparkySkinnedMeshRenderer.rootBone;
                monsterSMR.localBounds = sparkySkinnedMeshRenderer.localBounds;
                monsterSMR.lightProbeUsage = sparkySkinnedMeshRenderer.lightProbeUsage;

                BoxCollider[] boxColliders = bruteSparkyGameObject.GetComponentsInChildren<BoxCollider>();
                foreach (BoxCollider boxCollider in boxColliders)
                {
                    Debug.Log("Collider is " + boxCollider.name);
                    if (boxCollider.name.Equals("StopStandingOnMonsters"))
                    {
                        boxCollider.size = new Vector3(1.5f, 3f, 2.5f);
                    }
                }

                ModSettings.finishedCreatingSimpleSparky = true;

                BaseFeatures.DisableMonsterParticles(bruteSparkyGameObject);
            }

            // #SparkyModeAfterGenerationInitialisation
            public static void SparkyModeAfterGenerationInitialisation()
            {
                bool sparkyInGame = false;
                if (ModSettings.numberOfMonsters == 1 && References.Monster.GetComponent<Monster>().monsterType.Equals("Sparky"))
                {
                    sparkyInGame = true;
                }
                else if (ModSettings.numberOfMonsters > 1)
                {
                    foreach (Monster monster in ManyMonstersMode.monsterListMonsterComponents) // MMM is forced when Sparky is used.
                    {
                        if (monster.monsterType.Equals("Sparky"))
                        {
                            sparkyInGame = true;
                            break;
                        }
                    }
                }

                if (!sparkyInGame)
                {
                    return;
                }

                maxAggro = ModSettings.sparkyLurkMaxAggro;
                sparkyAggroTimers = new List<float>();
                sparkyList = new List<GameObject>();
                sparkyListMonsterComponents = new List<Monster>();
                sparkyState = new List<string>();
                bruteSparkyEyes = new List<Light[]>();
                customSparkyEyes = new List<Material[]>();

                // Sparky FSM adjustment
                foreach (Monster monster in ManyMonstersMode.monsterListMonsterComponents) // MMM is forced when Sparky is used.
                {
                    if (monster.monsterType.Equals("Sparky"))
                    {
                        // Add aura and disruptor for chases
                        FiendAura fiendAura = ModSettings.GiveMonsterFiendAuraAndDisruptor(monster, 0.3f, 2.5f, 5f);
                        if (!ModSettings.giveAllMonstersAFiendAura)
                        {
                            fiendAura.enabled = false;
                            monster.GetComponent<FiendLightDisruptor>().enabled = false;
                        }

                        // Setup lists
                        sparkyAggroTimers.Add(0f);
                        sparkyList.Add(monster.gameObject);
                        sparkyListMonsterComponents.Add(monster);
                        sparkyState.Add("");

                        // Change Brute Sparky's Light
                        Light[] lights = monster.GetComponentsInChildren<Light>();
                        if (ModSettings.customSparkyModel)
                        {
                            foreach (Light light in lights)
                            {
                                light.color = new Color(0f, 0f, 0f, 0f);
                            }

                            Material eyeL = Utilities.RecursiveTransformSearch(monster.gameObject.transform, "Eye_Inner.L").gameObject.GetComponentInChildren<MeshRenderer>().material;
                            Material eyeR = Utilities.RecursiveTransformSearch(monster.gameObject.transform, "Eye_Inner.R").gameObject.GetComponentInChildren<MeshRenderer>().material;
                            eyeL.SetColor("_EmissionColor", Color.red * 5f);
                            eyeR.SetColor("_EmissionColor", Color.red * 5f);

                            customSparkyEyes.Add(new Material[] { eyeL, eyeR });
                        }
                        else
                        {
                            // Brute Sparky
                            bruteSparkyEyes.Add(new Light[] { lights[4] /*Brute's left eye*/, lights[5] /*Brute's right eye*/ });
                            foreach (Light light in lights)
                            {
                                light.color = new Color(1f, 1f, 1f, 0f);
                            }
                        }

                        // Make FSM adjustments
                        MState mState = monster.GetComponent<MState>();

                        if (mState != null)
                        {
                            FSMState lurkState = monster.gameObject.AddComponent<MLurkState>();

                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("-----\nSparky FSM information BEFORE adding MLurkState:");
                            }
                            foreach (FSMState state in mState.Fsm.States)
                            {
                                if (ModSettings.logDebugText)
                                {
                                    Debug.Log("-----\nOne of Sparky's states is: " + state);
                                    foreach (FSMTransition transition in state.transitions)
                                    {
                                        Debug.Log("Transitions from this are: " + transition.name);
                                    }
                                }

                                // Add state transitions to other states going to MLurkState
                                Type stateType = state.GetType();
                                if (stateType == typeof(MWanderState) || stateType == typeof(MIdleState) || stateType == typeof(MSearchingState) || stateType == typeof(MDestroyState))
                                {
                                    state.AddTransition("Lurk", lurkState);
                                }

                                // Add state transitions to MLurkState going to other states
                                if (stateType == typeof(MChasingState))
                                {
                                    lurkState.AddTransition("Chase", state);
                                }
                                if (stateType == typeof(MDestroyState))
                                {
                                    lurkState.AddTransition("Destroy", state);
                                }
                            }

                            mState.Fsm.States.Add(lurkState);
                            lurkState.SetFSM(mState.Fsm);
                            if (ModSettings.logDebugText)
                            {
                                mState.Fsm.outputDebug = true;

                                Debug.Log("-----\nSparky FSM information AFTER adding MLurkState:");
                                foreach (FSMState state in mState.Fsm.States)
                                {
                                    Debug.Log("-----\nOne of Sparky's states is: " + state);
                                    foreach (FSMTransition transition in state.transitions)
                                    {
                                        Debug.Log("Transitions from this are: " + transition.name);
                                    }
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("Sparky MState is null");
                        }
                    }
                }

                ElectricTrapManager electricTrapManager = References.Player.AddComponent<ElectricTrapManager>();
                SparkyAura.empDictionary = new Dictionary<PrimaryRegionType, PrimaryRegionType>();
                SparkyAura.empDictionary.Add(PrimaryRegionType.CrewDeck, PrimaryRegionType.CrewDeck);
                SparkyAura.empDictionary.Add(PrimaryRegionType.OuterDeckCargo, PrimaryRegionType.CrewDeck);
                SparkyAura.empDictionary.Add(PrimaryRegionType.LowerDeck, PrimaryRegionType.LowerDeck);
                SparkyAura.empDictionary.Add(PrimaryRegionType.CargoHold, PrimaryRegionType.LowerDeck);
                SparkyAura.empDictionary.Add(PrimaryRegionType.SubEscape, PrimaryRegionType.LowerDeck);
                SparkyAura.empDictionary.Add(PrimaryRegionType.UpperDeck, PrimaryRegionType.UpperDeck);
                SparkyAura.empDictionary.Add(PrimaryRegionType.OuterDeck, PrimaryRegionType.UpperDeck);
                SparkyAura.empDictionary.Add(PrimaryRegionType.Engine, PrimaryRegionType.Engine);

                List<PrimaryRegionType> primaryFuseBoxRegions = new List<PrimaryRegionType>();
                primaryFuseBoxRegions.AddRange(new PrimaryRegionType[] { PrimaryRegionType.CrewDeck, PrimaryRegionType.LowerDeck, PrimaryRegionType.UpperDeck });
                primaryFuseBoxRegions.Remove(LevelGeneration.Instance.StartRoom.PrimaryRegion);
                foreach (PrimaryRegionType primaryRegionType in primaryFuseBoxRegions)
                {
                    FuseBox fusebox = FuseBoxManager.Instance.fuseboxes[primaryRegionType];
                    if (!ModSettings.noPreFilledFuseBoxes)
                    {
                        fusebox.AddFuse();
                        fusebox.AddPreExistingFuse();
                    }
                }

                if (sparkyAudioClips != null)
                {
                    foreach (AudioClip audioClip in sparkyAudioClips)
                    {
                        string path = "Music/";
                        if (audioClip.name.Contains("ChaseCool"))
                        {
                            path += "Cooldown";
                        }
                        else if (audioClip.name.Contains("ChaseLoop"))
                        {
                            path += "HighAlert";
                        }
                        else if (audioClip.name.Contains("HideCool"))
                        {
                            path += "Cooldown/AfterHide";
                        }
                        else if (audioClip.name.Contains("HideLoop"))
                        {
                            path += "Hiding";
                        }
                        else if (audioClip.name.Contains("WanderLoop"))
                        {
                            path += "NoAlert";
                        }

                        AudioLibrary audioLibrary = Instantiate(AudioSystem.GetLibraryFromName(path + "/Brute"));
                        audioLibrary.clips = new List<AudioClip>();
                        audioLibrary.AddClip(audioClip);

                        AudioSystem.instance.libraries.Add(path + "/Sparky", audioLibrary);

                        // Also make a sub theme for the high alert theme.
                        if (path.Contains("HighAlert"))
                        {
                            AudioSystem.instance.libraries.Add("Music/SubEvent/Sparky", audioLibrary);
                        }
                    }

                    /*
                    string[] musicTypes = new string[] { "Cooldown/AfterHide", "Cooldown", "NoAlert", "Hiding", "HighAlert", "SubEvent" };
                    foreach (string monsterName in ModSettings.monsterNames)
                    {
                        foreach (string musicType in musicTypes)
                        {
                            string path = "Music/" + musicType + "/" + monsterName;
                            AudioLibrary audioLibrary = AudioSystem.GetLibraryFromName(path);
                            if (audioLibrary != null)
                            {
                                Debug.Log("Library clips for " + path + ":");
                                foreach (AudioClip audioClip in audioLibrary.clips)
                                {
                                    Debug.Log(audioClip);
                                }
                            }
                            else
                            {
                                Debug.Log("No library for " + path);
                            }
                        }
                    }
                    */
                }
            }

            // #InitialiseSparkyMode
            public static void InitialiseSparkyMode()
            {
                if (!ModSettings.enableMultiplayer)
                {
                    On.FootStepManager.SetUpStep += new On.FootStepManager.hook_SetUpStep(HookFootStepManagerSetUpStep);
                }
                On.MChasingState.DoDoorCheck += new On.MChasingState.hook_DoDoorCheck(HookMChasingStateDoDoorCheck);
                On.Monster.TeleportTo += new On.Monster.hook_TeleportTo(HookMonsterTeleportTo);
                /*
                // Moved to BaseFeatures.
                if (!ModSettings.enableCrewVSMonsterMode)
                {
                    On.MovementControl.LerpRotate += new On.MovementControl.hook_LerpRotate(HookMovementControlLerpRotate);
                }
                */
                On.MWanderState.StateChanges += new On.MWanderState.hook_StateChanges(HookMWanderStateStateChanges);
                On.ToggleRoomLightSwitch.OnHandGrab += new On.ToggleRoomLightSwitch.hook_OnHandGrab(HookToggleRoomLightSwitchOnHandGrab);
                //On.WeightedSelection.Choose += new On.WeightedSelection.hook_Choose(HookWeightedSelectionChoose); // Moved to original LevelGeneration.Awake Code.
            }

            /*----------------------------------------------------------------------------------------------------*/
            // #SparkyCreatorProofOfConcept

            public static AnimatorOverrideController sparkyAnimatorOCPrefab;
            public static GameObject sparkyPrefab;
            public static List<AudioClip> sparkyAudioClips;

            public static Monster sparkyMonster;

            private static GameObject CreateBruteSparky()
            {
                MonsterSelection monsterSelection = UnityEngine.Object.FindObjectOfType<MonsterSelection>();

                GameObject sparkyGameObject = Instantiate<GameObject>(monsterSelection.NameToObject("Brute"));
                sparkyGameObject.name = "Sparky";

                sparkyMonster = sparkyGameObject.GetComponent<Monster>();
                sparkyMonster.monsterType = "Sparky";

                sparkyGameObject.AddComponent<SparkyAura>();

                return sparkyGameObject;
            }

            // @CreateSparkyGameObject
            public static GameObject CreateSparkyGameObject()
            {
                GameObject sparkyGameObject = CreateBruteSparky();
                if (ModSettings.customSparkyModel || ModSettings.customSparkyMusic)
                {
                    LoadSparkyAssetBundle();
                }

                if (ModSettings.customSparkyModel)
                {
                    SimpleSparkyModelTest(sparkyGameObject);
                }
                return sparkyGameObject;
            }

            private static void LoadSparkyAssetBundle()
            {
                try
                {
                    if (sparkyPrefab == null)
                    {
                        /*
                        string sparkyFilePathNew = Path.Combine(Directory.GetCurrentDirectory(), "Mods/MESMAssetBundles/mesmassetbundle");
                        Debug.Log("File path used for Sparky Asset Bundle is: " + sparkyFilePathNew);
                        try
                        {
                            try
                            {
                                sparkyAssetBundle = AssetBundle.LoadFromFile(sparkyFilePathNew);
                            }
                            catch
                            {
                                Debug.Log("Error loading Asset Bundle from file");
                            }
                            try
                            {
                                if (sparkyAssetBundle != null)
                                {
                                    sparkyAssetBundleObjects = sparkyAssetBundle.LoadAllAssets();
                                }
                                else
                                {
                                    Debug.Log("Sparky Asset Bundle is still null when trying to load all assets from it");
                                }
                            }
                            catch
                            {
                                Debug.Log("Error loading all assets from asset bundle.");
                            }
                        }
                        catch
                        {
                            Debug.Log("Error getting Sparky Asset Bundle");
                        }
                        */

                        UnityEngine.Object[] sparkyAssetBundleObjects = Utilities.LoadAssetBundle("sparky");

                        try
                        {
                            foreach (UnityEngine.Object sparkyObject in sparkyAssetBundleObjects)
                            {
                                Debug.Log("Sparky object from asset bundle is called " + sparkyObject.name + " and has type " + sparkyObject.GetType());

                                try
                                {
                                    if (sparkyObject.GetType() == typeof(GameObject))
                                    {
                                        sparkyPrefab = (GameObject)sparkyObject;

                                        if (ModSettings.logDebugText)
                                        {
                                            if (sparkyPrefab.GetComponent<Rigidbody>() != null)
                                            {
                                                Debug.Log("GetComponent Rigidbody is not null");
                                            }

                                            if (sparkyPrefab.GetComponentInChildren<Rigidbody>() != null)
                                            {
                                                Debug.Log("GetComponentInChildren Rigidbody is not null");
                                            }

                                            Component[] sparkyGOComponents = sparkyPrefab.GetComponents<Component>();
                                            foreach (Component component in sparkyGOComponents)
                                            {
                                                Debug.Log("Sparky GO component name is " + component.name + " and type is " + component.GetType());
                                            }
                                        }

                                        Component[] sparkyGOComponentsInChildren = sparkyPrefab.GetComponentsInChildren<Component>();
                                        foreach (Component component in sparkyGOComponentsInChildren)
                                        {
                                            if (ModSettings.logDebugText)
                                            {
                                                Debug.Log("Sparky GO component in children name is " + component.name + " and type is " + component.GetType());
                                            }
                                            if (component.GetType() == typeof(SkinnedMeshRenderer))
                                            {
                                                sparkyGlobalSMR = (SkinnedMeshRenderer)component;
                                                if (ModSettings.logDebugText)
                                                {
                                                    Debug.Log("Assigned Sparky Global SMR. Is this null? " + (component == null) + ". Is casted null? " + (sparkyGlobalSMR == null));
                                                }
                                            }
                                            else if (component.GetType() == typeof(Transform) && component.name.Equals("Armature"))
                                            {
                                                armatureTransform = (Transform)component;
                                            }
                                        }
                                    }
                                    else if (sparkyObject.GetType() == typeof(AnimatorOverrideController))
                                    {
                                        sparkyAnimatorOCPrefab = (AnimatorOverrideController)sparkyObject;
                                    }
                                    else if (sparkyObject.GetType() == typeof(AudioClip))
                                    {
                                        if (sparkyAudioClips == null)
                                        {
                                            sparkyAudioClips = new List<AudioClip>();
                                        }
                                        sparkyAudioClips.Add((AudioClip)sparkyObject);
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.Log("Error while loading Sparky object:\n" + e.ToString());
                                }
                            }
                        }
                        catch
                        {
                            Debug.Log("Error when trying to analyse objects from Sparky Asset Bundle");
                        }
                    }
                }
                catch
                {
                    Debug.Log("Error assigning Sparky Asset Bundle");
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Sparky Utility Functions

            public static IEnumerator UpdateFiendDisruptorAfterAFrame(FiendLightDisruptor fiendLightDisruptor)
            {
                yield return null;
                fiendLightDisruptor.timeSinceDisrupt = fiendLightDisruptor.maxSinceDisrupt + 0.01f;
                yield break;
            }

            public static IEnumerator DisableFiendAuraAfterAFrame(FiendAura fiendAura, FiendLightDisruptor fiendLightDisruptor)
            {
                // Reset light disruption.
                float[] auraValues = new float[6] { fiendAura.smallRadius, fiendAura.mediumRadius, fiendAura.largeRadius, fiendAura.srt_smlRad, fiendAura.srt_medRad, fiendAura.srt_lrgRad };
                fiendAura.smallRadius = 0f;
                fiendAura.mediumRadius = 0f;
                fiendAura.largeRadius = 0f;
                fiendAura.srt_smlRad = 0f;
                fiendAura.srt_medRad = 0f;
                fiendAura.srt_lrgRad = 0f;
                fiendLightDisruptor.DisruptLights(); // This resets the normal lights, but not the flashlight.

                // Update the disruptor.
                // Set this to minus the frame time as the frame time is also added in the disruptor's late update. This means the value will be 0f after the late update, allowing the flashlight to update.
                // Doesn't work with normal Fiend for some reason...? See Alternating Monsters Mode code. Had to update FiendLightController to get it to work.
                fiendLightDisruptor.timeSinceDisrupt = -Time.deltaTime;
                yield return null;

                // Disable the Fiend components.
                fiendAura.enabled = false;
                fiendLightDisruptor.enabled = false;

                // Reset the disruptor values.
                fiendAura.smallRadius = auraValues[0];
                fiendAura.mediumRadius = auraValues[1];
                fiendAura.largeRadius = auraValues[2];
                fiendAura.srt_smlRad = auraValues[3];
                fiendAura.srt_medRad = auraValues[4];
                fiendAura.srt_lrgRad = auraValues[5];
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SparkyActiveFeatures

            public static void SparkyActiveFeatures()
            {
                for (int sparkyNumber = 0; sparkyNumber < sparkyList.Count; sparkyNumber++)
                {
                    // Change aggro timers
                    switch (sparkyState[sparkyNumber])
                    {
                        case "MonstrumExtendedSettingsMod.ExtendedSettingsModScript+MLurkState":
                            {
                                break;
                            }
                        case "MChasingState":
                            {
                                sparkyAggroTimers[sparkyNumber] = maxAggro;
                                break;
                            }
                        default:
                            {
                                sparkyAggroTimers[sparkyNumber] -= Time.deltaTime / 5f;
                                break;
                            }
                    }
                    // Clamp the aggro timer
                    sparkyAggroTimers[sparkyNumber] = Mathf.Clamp(sparkyAggroTimers[sparkyNumber], 0f, maxAggro);

                    // Change lights
                    float upperLimit = 0.84f;
                    if (ModSettings.customSparkyModel)
                    {
                        foreach (Material material in customSparkyEyes[sparkyNumber])
                        {
                            material.SetColor("_EmissionColor", new Color(upperLimit, upperLimit - upperLimit * sparkyAggroTimers[sparkyNumber] / maxAggro, upperLimit - upperLimit * sparkyAggroTimers[sparkyNumber] / maxAggro, 1f) * 5f);
                        }
                    }
                    else
                    {
                        // Brute Sparky
                        foreach (Light light in bruteSparkyEyes[sparkyNumber])
                        {
                            light.color = new Color(upperLimit, upperLimit - upperLimit * sparkyAggroTimers[sparkyNumber] / maxAggro, upperLimit - upperLimit * sparkyAggroTimers[sparkyNumber] / maxAggro, 1f);
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FootStepManager

            private static void HookFootStepManagerSetUpStep(On.FootStepManager.orig_SetUpStep orig, FootStepManager footStepManager, int _speed)
            {
                if (((MonoBehaviour)footStepManager).transform.root.tag == "Monster" && footStepManager.monster.monsterType.Equals("Sparky") && footStepManager.monster.GetComponent<MState>().Fsm.Current.GetType() == typeof(MLurkState))
                {
                    return;
                }
                orig.Invoke(footStepManager, _speed);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MChasingState

            private static void HookMChasingStateDoDoorCheck(On.MChasingState.orig_DoDoorCheck orig, MChasingState mChasingState, bool _overwrite)
            {
                if (!mChasingState.sinceDoorCheck.timerStarted || _overwrite)
                {
                    if (!mChasingState.sinceDoorCheck.timerStarted)
                    {
                        mChasingState.sinceDoorCheck.StartTimer();
                    }
                    if (ModSettings.giveAllMonstersAFireShroud)
                    {
                        ((MState)mChasingState).monster.GetComponent<FireShroud>().FireBlast();
                    }
                    if (((MState)mChasingState).monster.monsterType.Equals("Sparky"))
                    {
                        ((MState)mChasingState).monster.GetComponent<SparkyAura>().SpawnTrapsNearSparky(0f, 0.75f * mChasingState.timeBetweenDoorCheck);
                    }
                }
                else if (mChasingState.sinceDoorCheck.TimeElapsed > mChasingState.timeBetweenDoorCheck)
                {
                    if (((MState)mChasingState).monster.MonsterType != Monster.MonsterTypeEnum.Fiend)
                    {
                        mChasingState.sinceDoorCheck.ResetTimer();
                    }
                    if (ModSettings.giveAllMonstersAFireShroud)
                    {
                        ((MState)mChasingState).monster.GetComponent<FireShroud>().FireBlast();
                    }
                    if (((MState)mChasingState).monster.monsterType.Equals("Sparky"))
                    {
                        ((MState)mChasingState).monster.GetComponent<SparkyAura>().SpawnTrapsNearSparky(0f, 0.75f * mChasingState.timeBetweenDoorCheck);
                    }
                }
                orig.Invoke(mChasingState, _overwrite);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Monster

            private static void HookMonsterTeleportTo(On.Monster.orig_TeleportTo orig, Monster monster, Vector3 TPToHere)
            {
                if (monster.MonsterType == Monster.MonsterTypeEnum.Hunter)
                {
                    monster.GetEars.MoveBackToOriginal();
                }
                ((MonoBehaviour)monster).transform.position = TPToHere;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MWanderState

            private static void HookMWanderStateStateChanges(On.MWanderState.orig_StateChanges orig, MWanderState mWanderState)
            {
                if (((MState)mWanderState).monster.SubEventBeenStarted())
                {
                    ((MState)mWanderState).SendEvent("EventStarted");
                }
                else if (((MState)mWanderState).monster.IsMonsterDestroying)
                {
                    ((MState)mWanderState).SendEvent("Destroy");
                }
                else if (((MState)mWanderState).monster.MoveControl.shouldClimb)
                {
                    ((MState)mWanderState).SendEvent("Climb");
                }
                else if ((((MState)mWanderState).monster.MoveControl.IsAtDestination || ((MState)mWanderState).monster.IsStuck) && ((MState)mWanderState).monster.Patrol.PointChosen != Vector3.zero && ((MState)mWanderState).TimeInState > 10f && ((MState)mWanderState).monster.Pathfinding.IsComplete)
                {
                    ((MState)mWanderState).SendEvent("GoToIdle");
                }
                else if (((MState)mWanderState).monster.MoveControl.PathNotCorrect() && ((MState)mWanderState).TimeInState > 10f)
                {
                    ((MState)mWanderState).SendEvent("GoToIdle");
                }
                else if (mWanderState.PlayerFarFromGoal() && ((MState)mWanderState).TimeInState > 10f)
                {
                    ((MState)mWanderState).SendEvent("GoToIdle");
                }
                else if (((MState)mWanderState).monster.GetAlertMeters.mSightAlert > 99f && ((MState)mWanderState).monster.IsPlayerLocationKnown)
                {
                    if (!((MState)mWanderState).monster.monsterType.Equals("Sparky"))
                    {
                        ((MState)mWanderState).SendEvent("Chase");
                    }
                    else
                    {
                        ((MState)mWanderState).SendEvent("Lurk");
                    }
                }
                else if (((MState)mWanderState).monster.CanSensePlayerNear)
                {
                    ((MState)mWanderState).SendEvent("Search");
                }
                else if (((MState)mWanderState).monster.HasPointOfInterest && ((MState)mWanderState).monster.CanHearNoise)
                {
                    ((MState)mWanderState).SendEvent("Search");
                }
                else if (((MState)mWanderState).monster.CanSeePlayerNotHiding)
                {
                    ((MState)mWanderState).SendEvent("Search");
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ToggleRoomLightSwitch

            private static void HookToggleRoomLightSwitchOnHandGrab(On.ToggleRoomLightSwitch.orig_OnHandGrab orig, ToggleRoomLightSwitch toggleRoomLightSwitch)
            {
                orig.Invoke(toggleRoomLightSwitch);
                if (sparkyList.Count > 0 && (!ModSettings.darkShip || ModSettings.powerableLights))
                {
                    Room room = ((MonoBehaviour)toggleRoomLightSwitch).transform.GetParentOfType<Room>();
                    Collider[] colliders = Physics.OverlapBox(room.RoomBounds.center, room.RoomBounds.extents + new Vector3(2f, 0f, 2f));
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        Room nearbyRoom = colliders[i].GetComponentInChildren<Room>();
                        if (nearbyRoom != null && nearbyRoom.GetComponentInChildren<ToggleRoomLightSwitch>() == null)
                        {
                            foreach (GenericLight genericLight in nearbyRoom.RoomLights)
                            {
                                genericLight.CurrentLightType = GenericLight.LightTypes.Normal; // Normal does not mean on, so this is fine without a powered check.
                            }
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @WeightedSelection

            /* Moved to LevelGeneration.Awake
            private static GameObject HookWeightedSelectionChoose(On.WeightedSelection.orig_Choose orig, WeightedSelection weightedSelection)
            {
                // If the original weighted selection is used, which it only is when the player uses random monsters, then let Sparky be chosen too at a 1/4 chance.
                if (weightedSelection.playerPrefIdentifier == "MonsterCounts")
                {
                    int upperIndexLimit = GameObject.FindGameObjectsWithTag("Monster").Length;
                    int randomNumber = UnityEngine.Random.Range(0, upperIndexLimit + 1);
                    if (randomNumber == upperIndexLimit)
                    {
                        return CreateSparkyGameObject();
                    }
                }
                return orig.Invoke(weightedSelection);
            }
            */

            /*----------------------------------------------------------------------------------------------------*/
        }

        // ~MLurkState
        // The monster will be seeing the player when entering this state.
        public class MLurkState : MState
        {
            int sparkyNumber;
            float minDistanceToPlayer;
            float maxSuperEMPDistance;
            float superEMPChargeTimeToWait;
            float superEMPChargeTimer;
            bool doingSuperEMP;

            MLurkState()
            {
                minDistanceToPlayer = ModSettings.sparkyLurkMinimumDistanceToPlayer;
                maxSuperEMPDistance = ModSettings.sparkyLurkMaxSuperEMPDistance;
                superEMPChargeTimeToWait = ModSettings.sparkyLurkSuperEMPChargeTimeToWait;
            }

            public override void OnEnter()
            {
                base.OnEnter();
                typeofState = FSMState.StateTypes.LowAlert;
                sparkyNumber = SparkyMode.sparkyListMonsterComponents.IndexOf(base.monster);
                SparkyMode.sparkyState[sparkyNumber] = "MonstrumExtendedSettingsMod.ExtendedSettingsModScript+MLurkState";
                superEMPChargeTimer = 0f;
                doingSuperEMP = false;
            }

            public override void OnUpdate()
            {
                base.OnUpdate();
                if (!doingSuperEMP)
                {
                    StateChanges();
                    PlayerVisionCheck();
                    SuperEMPCheck(); // Do this after the StateChanges so that the monster is kept still.
                }
            }

            public override void OnExit()
            {
                base.monster.PreviousWasDestroy = false;
                base.monster.PreviousWasClimb = false;
                SparkyMode.sparkyState[sparkyNumber] = "";
            }

            private void PlayerVisionCheck()
            {
                Camera playerCamera;
                if (!ModSettings.enableMultiplayer)
                {
                    playerCamera = References.Cam;
                }
                else
                {
                    playerCamera = MultiplayerMode.PlayerCamera(base.monster.player.GetComponent<NewPlayerClass>());
                }

                if (monster.CanSeePlayer && Vector3.Angle(playerCamera.transform.forward, -monster.transform.forward) <= playerCamera.fieldOfView)
                {
                    SparkyMode.sparkyAggroTimers[sparkyNumber] += Time.deltaTime;
                    //Debug.Log("Player can see Sparky during lurk 1! " + SparkyMode.sparkyAggroTimers[sparkyNumber] / SparkyMode.maxAggro);
                }
            }

            private void SuperEMPCheck()
            {
                if (ManyMonstersMode.PlayerToMonsterDistance(base.monster) < maxSuperEMPDistance - (maxSuperEMPDistance - minDistanceToPlayer) / 4f && monster.CanSeePlayer)
                {
                    base.monster.MoveControl.AnimationSpeed = 0f;
                    superEMPChargeTimer += Time.deltaTime;
                    //Debug.Log("Charging super EMP: " + superEMPChargeTimer / superEMPTimeToWait);
                    AudioSystem.PlaySound("Noises/Enviro/Electrical/Sparks/Long", monster.AudSource);
                    if (superEMPChargeTimer >= superEMPChargeTimeToWait && ManyMonstersMode.PlayerToMonsterDistance(base.monster) < maxSuperEMPDistance)
                    {
                        base.StartCoroutine(SuperEMP());
                    }
                }
                else
                {
                    superEMPChargeTimer -= Time.deltaTime;
                    if (superEMPChargeTimer < 0f)
                    {
                        superEMPChargeTimer = 0f;
                    }
                }
            }

            private IEnumerator SuperEMP()
            {
                Debug.Log("Carrying out super EMP.");
                doingSuperEMP = true;
                superEMPChargeTimer = 0f;
                float time = 0f;
                NewPlayerClass newPlayerClass = monster.player.GetComponent<NewPlayerClass>();
                AudioSystem.PlaySound("Noises/Enviro/Electrical/Sparks/Long", newPlayerClass.GetHeadSource);
                newPlayerClass.PushBackDir = 1f;
                newPlayerClass.playerAnimator.applyRootMotion = true;
                while (time <= 1f)
                {
                    base.monster.MoveControl.AnimationSpeed = 0f;
                    time += Time.deltaTime;
                    yield return null;
                }
                Debug.Log("Chasing from successful super EMP.");
                base.SendEvent("Chase");
                yield break;
            }

            private void StateChanges()
            {
                if (SparkyMode.sparkyAggroTimers[sparkyNumber] == SparkyMode.maxAggro || ManyMonstersMode.PlayerToMonsterDistance(base.monster) < minDistanceToPlayer || base.monster.TimeOutVision.TimeElapsed > 3f)
                {
                    /*
                    if (SparkyMode.sparkyAggroTimers[sparkyNumber] == SparkyMode.maxAggro)
                    {
                        Debug.Log("Starting chase from max aggro");
                    }
                    if (ManyMonstersMode.PlayerToMonsterDistance(base.monster) < minDistanceToPlayer)
                    {
                        Debug.Log("Starting chase from being too close");
                    }
                    if (base.monster.TimeOutVision.TimeElapsed > 3f)
                    {
                        Debug.Log("Starting chase from TimeOutVision");
                    }
                    */
                    base.SendEvent("Chase");
                }
                else
                {
                    if (base.monster != null && base.monster.MoveControl != null)
                    {
                        base.monster.MoveControl.MaxSpeed = 100f;
                    }
                    if (base.monster.IsMonsterDestroying)
                    {
                        base.SendEvent("Destroy");
                    }
                }
            }
        }

        class SparkyAura : MonoBehaviour
        {
            float minEMPWait;
            float maxEMPWait;
            float maxRoomBuffPercentage;
            float sparkyElectricTrapSpawnChance;
            float empRange;
            float distantSwitchChance;
            float distantSwitchFailChanceAddition;
            float regionEMPRoomThreshold;
            float lightFailTime = 1f;
            public float buffPercentage; // Decimal fraction / decimal percentage. Not technically a ratio. // A number between 0 and 1 - like a percentage but expressed as a decimal [closed] - https://english.stackexchange.com/questions/218655/a-number-between-0-and-1-like-a-percentage-but-expressed-as-a-decimal - Accessed 24.08.2022
            float timeUntilEMP;
            float timeUntilCheck;
            int failedSwitchAttempts;
            List<Room> nearbyRooms;
            List<Room> nearbyActiveRooms;
            Monster monster;
            public static Dictionary<PrimaryRegionType, PrimaryRegionType> empDictionary;

            void Start()
            {
                minEMPWait = ModSettings.sparkyAuraMinEMPWait;
                maxEMPWait = ModSettings.sparkyAuraMaxEMPWait;
                maxRoomBuffPercentage = ModSettings.sparkyAuraMaxRoomBuffPercentage / 100f;
                sparkyElectricTrapSpawnChance = ModSettings.sparkyElectricTrapSpawnChance / 100f;
                empRange = ModSettings.sparkyAuraEMPRange;
                distantSwitchChance = ModSettings.sparkyAuraDistantSwitchChance / 100f;
                distantSwitchFailChanceAddition = ModSettings.sparkyAuraDistantSwitchFailChanceAddition / 100f;
                regionEMPRoomThreshold = ModSettings.sparkyRegionEMPRoomThreshold / 100f;
                failedSwitchAttempts = 0;

                this.monster = base.GetComponent<Monster>();
                nearbyRooms = new List<Room>();
                nearbyActiveRooms = new List<Room>();
            }

            void Update()
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    timeUntilCheck -= Time.deltaTime;
                    timeUntilEMP -= Time.deltaTime;
                    if (timeUntilCheck <= 0f)
                    {
                        CheckRegion();
                    }
                    if (timeUntilEMP <= 0f)
                    {
                        EMP();
                    }
                }
            }

            void CheckRegion()
            {
                //Debug.Log("Performing region check.");
                if (monster.RoomDetect.nodeData != null && empDictionary.ContainsKey(monster.RoomDetect.nodeData.primaryRegion))
                {
                    PrimaryRegionType regionToCheck = (monster.RoomDetect.nodeData.primaryRegion == PrimaryRegionType.Engine) ? PrimaryRegionType.LowerDeck : empDictionary[monster.RoomDetect.nodeData.primaryRegion];
                    if (!FuseBoxManager.Instance.fuseboxes[regionToCheck].powered)
                    {
                        //Debug.Log("Sparky is in unpowered region.");
                        buffPercentage = 1f;
                    }
                    else
                    {
                        //Debug.Log("Performing region check for powered region from " + RegionManager.Instance.ConvertPointToRegionNode(monster.transform.position + Vector3.up));
                        nearbyRooms = new List<Room>();
                        nearbyActiveRooms = new List<Room>();
                        Collider[] colliders = Physics.OverlapBox(monster.transform.position + Vector3.up, new Vector3(empRange, 0.75f, empRange));
                        for (int i = 0; i < colliders.Length; i++)
                        {
                            Room nearbyRoom = colliders[i].GetComponentInChildren<Room>();
                            if (nearbyRoom != null)
                            {
                                ToggleRoomLightSwitch toggleRoomLightSwitch = nearbyRoom.GetComponentInChildren<ToggleRoomLightSwitch>();
                                if (toggleRoomLightSwitch != null)
                                {
                                    //Debug.Log("Found room near Sparky with light switch: " + RegionManager.Instance.ConvertPointToRegionNode(nearbyRoom.transform.position));
                                    nearbyRooms.Add(nearbyRoom);
                                    if (toggleRoomLightSwitch.lightSwitch[0].On)
                                    {
                                        //Debug.Log("The room is on!");
                                        nearbyActiveRooms.Add(nearbyRoom);
                                    }
                                }
                            }
                        }
                        if (nearbyRooms.Count > 0)
                        {
                            buffPercentage = (nearbyActiveRooms.Count / nearbyRooms.Count) * maxRoomBuffPercentage;
                        }
                        else
                        {
                            buffPercentage = 0f;
                        }
                    }
                }
                else
                {
                    buffPercentage = 0f;
                }
                timeUntilCheck = 5f;
            }

            public void EMP(bool fromChase = false)
            {
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Doing EMP");
                }
                CheckRegion();
                // Only mess with room power when the region is powered on.
                // # Remember to add extra spark effects. Could also switch off lights next to EMPd rooms.
                if (monster.RoomDetect.nodeData != null && empDictionary.ContainsKey(monster.RoomDetect.nodeData.primaryRegion) && FuseBoxManager.Instance.fuseboxes.ContainsKey(empDictionary[monster.RoomDetect.nodeData.primaryRegion]))
                {
                    PrimaryRegionType regionToEMP = empDictionary[monster.RoomDetect.nodeData.primaryRegion];
                    // Special OuterDeck Check. OuterDeck (Liferaft area) is treated as Upper Deck. Also makes sense to make a special case for it though.
                    if ((monster.RoomDetect.nodeData.primaryRegion == PrimaryRegionType.OuterDeck || monster.RoomDetect.nodeData.primaryRegion == PrimaryRegionType.SubEscape) && FuseBoxManager.Instance.fuseboxes.ContainsKey(monster.RoomDetect.nodeData.primaryRegion) && FuseBoxManager.Instance.fuseboxes[monster.RoomDetect.nodeData.primaryRegion].powered && UnityEngine.Random.value <= distantSwitchChance)
                    {
                        FuseBoxManager.Instance.fuseboxes[monster.RoomDetect.nodeData.primaryRegion].transform.parent.GetComponentInChildren<FuseBoxLever>().PullLever(true);
                    }
                    if (FuseBoxManager.Instance.fuseboxes[regionToEMP].powered)
                    {
                        AudioSystem.PlaySound("Noises/Enviro/Electrical/Sparks/Long", monster.AudSource);
                        if (nearbyActiveRooms.Count > 0)
                        {
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("Found " + nearbyActiveRooms.Count + " active rooms near Sparky to switch off!");
                            }
                            for (int i = 0; i < nearbyActiveRooms.Count; i++)
                            {
                                //Debug.Log("Switching off nearby active room: " + RegionManager.Instance.ConvertPointToRegionNode(nearbyActiveRooms[i].transform.position));
                                ToggleRoomLightSwitch toggleRoomLightSwitch = nearbyActiveRooms[i].GetComponentInChildren<ToggleRoomLightSwitch>();
                                if (toggleRoomLightSwitch != null)
                                {
                                    base.StartCoroutine(KillCorridorLightsOfAndAroundRoom(nearbyActiveRooms[i], toggleRoomLightSwitch));
                                }
                            }
                        }
                        else
                        {
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("No active rooms near Sparky!");
                            }
                            bool switchARoom = UnityEngine.Random.value <= distantSwitchChance + distantSwitchFailChanceAddition * failedSwitchAttempts;
                            if (switchARoom)
                            {
                                failedSwitchAttempts = 0;
                            }
                            else
                            {
                                failedSwitchAttempts++;
                            }
                            int numberOfRoomsInRegion = 0;
                            int numberOfRoomsActiveInRegion = 0;
                            List<Room> regionRooms = FuseBoxManager.Instance.rooms[regionToEMP];
                            for (int i = 0; i < regionRooms.Count; i++)
                            {
                                //Debug.Log("Found room in Sparky's region: " + RegionManager.Instance.ConvertPointToRegionNode(regionRooms[i].transform.position));
                                ToggleRoomLightSwitch toggleRoomLightSwitch = regionRooms[i].GetComponentInChildren<ToggleRoomLightSwitch>();
                                if (toggleRoomLightSwitch != null)
                                {
                                    //Debug.Log("Room has a light switch.");
                                    numberOfRoomsInRegion++;
                                    if (toggleRoomLightSwitch.lightSwitch[0].On)
                                    {
                                        //Debug.Log("Room is On.");
                                        if (switchARoom)
                                        {
                                            if (ModSettings.logDebugText)
                                            {
                                                Debug.Log("Switching off distant room.");
                                            }
                                            base.StartCoroutine(KillCorridorLightsOfAndAroundRoom(regionRooms[i], toggleRoomLightSwitch));
                                            switchARoom = false;
                                        }
                                        else
                                        {
                                            numberOfRoomsActiveInRegion++;
                                        }
                                    }
                                }
                            }
                            if (ModSettings.logDebugText)
                            {
                                if (numberOfRoomsInRegion > 0)
                                {
                                    Debug.Log("Number of rooms in region = " + numberOfRoomsInRegion + ". Number of Rooms active in region = " + numberOfRoomsActiveInRegion + ". Percentage of active to all = " + ((float)numberOfRoomsActiveInRegion / numberOfRoomsInRegion));
                                }
                                else
                                {
                                    Debug.Log("No rooms in Sparky's region!");
                                }
                            }
                            if (numberOfRoomsInRegion > 0 && ((float)numberOfRoomsActiveInRegion / numberOfRoomsInRegion) < regionEMPRoomThreshold)
                            {
                                RegionEMP(regionToEMP); // Should this whole check be done even when there are active rooms nearby? Maybe I should have a big EMP every 90 seconds or something. Nothing should ever be laggy though.
                                if (fromChase)
                                {
                                    TimeScaleManager.Instance.StartCoroutine(ElectricTrapManager.instance.ElectrifyRegion(regionToEMP));  // Electrify the region when Sparky starts chasing the player. This case happens only after a region EMP.
                                }
                            }
                        }
                        if (monster.GetComponent<FSM>().Current.typeofState != FSMState.StateTypes.Chase)
                        {
                            base.StartCoroutine(ShortFlicker());
                        }
                    }
                    else if (fromChase)
                    {
                        if (ModSettings.logDebugText)
                        {
                            Debug.Log("Starting electrification coroutine.");
                        }
                        TimeScaleManager.Instance.StartCoroutine(ElectricTrapManager.instance.ElectrifyRegion(regionToEMP)); // Electrify the region when Sparky starts chasing the player. This case happens any time the region is powered off.
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("Region is not powered.");
                    }
                    timeUntilEMP = UnityEngine.Random.Range(minEMPWait + buffPercentage * (maxEMPWait - minEMPWait), maxEMPWait);
                }
                else
                {
                    if (ModSettings.logDebugText)
                    {
                        Debug.Log("Sparky is not in a normal region. Using reduced EMP cooldown");
                    }
                    timeUntilEMP = UnityEngine.Random.Range((minEMPWait + buffPercentage * (maxEMPWait - minEMPWait)) / 4f, maxEMPWait / 4f);
                }
            }

            IEnumerator KillCorridorLightsOfAndAroundRoom(Room room, ToggleRoomLightSwitch toggleRoomLightSwitch)
            {
                if (!ModSettings.darkShip || ModSettings.powerableLights)
                {
                    yield return new WaitForSeconds(UnityEngine.Random.Range(0.75f * lightFailTime, 1.25f * lightFailTime));
                    toggleRoomLightSwitch.OnHandGrab();
                    Collider[] colliders = Physics.OverlapBox(room.RoomBounds.center, room.RoomBounds.extents + new Vector3(2f, 0f, 2f));
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        Room nearbyRoom = colliders[i].GetComponentInChildren<Room>();
                        if (nearbyRoom != null && nearbyRoom.GetComponentInChildren<ToggleRoomLightSwitch>() == null)
                        {
                            foreach (GenericLight genericLight in nearbyRoom.RoomLights)
                            {
                                genericLight.CurrentLightType = GenericLight.LightTypes.Off;
                            }
                        }
                    }
                }
                yield break;
            }

            IEnumerator ShortFlicker()
            {
                if (!ModSettings.giveAllMonstersAFiendAura)
                {
                    // Switch on Sparky's fiend components for a moment.
                    FiendAura fiendAura = monster.GetComponent<FiendAura>();
                    fiendAura.enabled = true;
                    FiendLightDisruptor fiendLightDisruptor = monster.GetComponent<FiendLightDisruptor>();
                    fiendLightDisruptor.enabled = true;
                    TimeScaleManager.Instance.StartCoroutine(SparkyMode.UpdateFiendDisruptorAfterAFrame(fiendLightDisruptor));
                    yield return new WaitForSeconds(lightFailTime);

                    // Then switch them off again.
                    TimeScaleManager.Instance.StartCoroutine(SparkyMode.DisableFiendAuraAfterAFrame(fiendAura, fiendLightDisruptor));
                }
                yield break;
            }

            void RegionEMP(PrimaryRegionType regionToEMP)
            {
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Doing region EMP");
                }
                if (FuseBoxManager.Instance.rooms.ContainsKey(regionToEMP) && empDictionary.ContainsKey(regionToEMP) && FuseBoxManager.Instance.fuseboxes[regionToEMP].powered)
                {
                    foreach (Room room in FuseBoxManager.Instance.rooms[regionToEMP])
                    {
                        ToggleRoomLightSwitch toggleRoomLightSwitch = room.GetComponentInChildren<ToggleRoomLightSwitch>();
                        if (toggleRoomLightSwitch != null && toggleRoomLightSwitch.lightSwitch[0].On)
                        {
                            toggleRoomLightSwitch.OnHandGrab();
                        }
                    }
                    FuseBoxManager.Instance.fuseboxes[regionToEMP].transform.parent.GetComponentInChildren<FuseBoxLever>().PullLever(true);
                }
            }

            public void SpawnTrapsNearSparky(float minTime, float maxTime)
            {
                Collider[] colliders = Physics.OverlapBox(monster.transform.position + Vector3.up, new Vector3(empRange, 0.75f, empRange));
                if (colliders != null)
                {
                    for (int i = 0; i < colliders.Length; i++)
                    {
                        Room nearbyRoom = colliders[i].GetComponentInChildren<Room>();
                        if (nearbyRoom != null && nearbyRoom.RoomType == RoomStructure.Corridor && UnityEngine.Random.value <= sparkyElectricTrapSpawnChance)
                        {
                            base.StartCoroutine(ElectricTrapManager.instance.SpawnElectricTrapWithRandomTimer(nearbyRoom.RoomBounds, buffPercentage, minTime, maxTime));
                        }
                    }
                }
            }
        }

        class ElectricTrapManager : MonoBehaviour
        {
            GameObject electricTrapPrefab;
            Dictionary<PrimaryRegionType, bool> regionsElectrified;
            public static ElectricTrapManager instance; // StartCoroutine in a public static void - Bunny83 - https://answers.unity.com/questions/1207534/startcoroutine-in-a-public-static-void.html - Accessed 21.08.2022
            float regionElectricTrapSpawnChance;
            float regionElectricTrapSlowFactor;
            float regionElectricTrapScaleMultiplier;
            float regionElectricTrapLifeTime;
            float regionElectricTrapMinSpawnTime;
            float regionElectricTrapMaxSpawnTime;
            float regionElectrificationRoomRecoveryPercentage;
            float sparkyElectricTrapBaseSlowFactor;
            float maxTrapSlowFactorDecreaseFromSparkyBuff;
            float sparkyElectricTrapBaseScaleMultiplier;
            float maxTrapScaleMultiplierIncreaseFromSparkyBuff;
            float sparkyElectricTrapBaseLifeTime;
            float maxTrapLifeTimeIncreaseFromSparkyBuff;

            void Start()
            {
                regionElectricTrapSpawnChance = ModSettings.regionElectricTrapSpawnChance / 100f;
                regionElectricTrapSlowFactor = ModSettings.regionElectricTrapSlowFactor;
                regionElectricTrapScaleMultiplier = ModSettings.regionElectricTrapScaleMultiplier;
                regionElectricTrapLifeTime = ModSettings.regionElectricTrapLifeTime;
                regionElectricTrapMinSpawnTime = ModSettings.regionElectricTrapMinSpawnTime;
                regionElectricTrapMaxSpawnTime = ModSettings.regionElectricTrapMaxSpawnTime;
                regionElectrificationRoomRecoveryPercentage = ModSettings.regionElectrificationRoomRecoveryPercentage / 100f;
                sparkyElectricTrapBaseSlowFactor = ModSettings.sparkyElectricTrapBaseSlowFactor;
                maxTrapSlowFactorDecreaseFromSparkyBuff = ModSettings.maxTrapSlowFactorDecreaseFromSparkyBuff;
                sparkyElectricTrapBaseScaleMultiplier = ModSettings.sparkyElectricTrapBaseScaleMultiplier;
                maxTrapScaleMultiplierIncreaseFromSparkyBuff = ModSettings.maxTrapScaleMultiplierIncreaseFromSparkyBuff;
                sparkyElectricTrapBaseLifeTime = ModSettings.sparkyElectricTrapBaseLifeTime;
                maxTrapLifeTimeIncreaseFromSparkyBuff = ModSettings.maxTrapLifeTimeIncreaseFromSparkyBuff;
                CreateElectricTrapPrefab();
                if (instance == null)
                {
                    ElectricTrapManager.instance = this;
                }
            }

            /*
            Found material through ps: Fire_Sprite_v3 (Instance) (UnityEngine.Material)
            Found material through ps: Fire_Sprite_v3 (Instance) (UnityEngine.Material)
            Found material through ps: FuseBoxOnParticles (Instance) (UnityEngine.Material)
            Found material through ps: FuseBoxOnParticles (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: Fuel2 (Instance) (UnityEngine.Material)
            Found material through ps: Fuel2 (Instance) (UnityEngine.Material)
            Found material through ps: Fuel2 (Instance) (UnityEngine.Material)
            Found material through ps: Fuel2 (Instance) (UnityEngine.Material)
            Found material through ps: Fuel2 (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: SteamVent (Instance) (UnityEngine.Material)
            Found material through ps: Smoke (Instance) (UnityEngine.Material)
            Found material through ps: Smoke (Instance) (UnityEngine.Material)
            Found material through ps: Smoke (Instance) (UnityEngine.Material)
            Found material through ps: Smoke (Instance) (UnityEngine.Material)
            Found material through ps: Smoke (Instance) (UnityEngine.Material)
            Found material through ps: Smoke (Instance) (UnityEngine.Material)
            Found material through ps: Smoke (Instance) (UnityEngine.Material)
            Found material through ps: Smoke (Instance) (UnityEngine.Material)
            Found material through ps: Smoke (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: FuseBoxOnParticles (Instance) (UnityEngine.Material)
            Found material through ps: FuseBoxOnParticles (Instance) (UnityEngine.Material)
            Found material through ps: OculusSteam (Instance) (UnityEngine.Material)
            Found material through ps: OculusSteam (Instance) (UnityEngine.Material)
            Found material through ps: FuseBoxOnParticles (Instance) (UnityEngine.Material)
            Found material through ps: Dust Material 1 (Instance) (UnityEngine.Material)
            Found material through ps: OculusSteam (Instance) (UnityEngine.Material)
            Found material through ps: OculusSteam (Instance) (UnityEngine.Material)
            Found material through ps: FuseBoxOnParticles (Instance) (UnityEngine.Material)
            Found material through ps: FuseBoxOnParticles (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: DoorDust (Instance) (UnityEngine.Material)
            Found material through ps: FuseBoxOnParticles (Instance) (UnityEngine.Material)
            */

            public void CreateElectricTrapPrefab()
            {
                regionsElectrified = new Dictionary<PrimaryRegionType, bool>();
                foreach (PrimaryRegionType primaryRegionType in Enum.GetValues(typeof(PrimaryRegionType)))
                {
                    regionsElectrified.Add(primaryRegionType, false);
                }
                GameObject electricTrap = new GameObject();

                electricTrap.SetActive(false);
                electricTrapPrefab = electricTrap;

                ParticleSystemRenderer psr = electricTrap.AddComponent<ParticleSystemRenderer>();
                ParticleSystemRenderer[] pss = FindObjectsOfType<ParticleSystemRenderer>();
                foreach (ParticleSystemRenderer ps in pss)
                {
                    if (ps.material != null)
                    {
                        //Debug.Log("Found material through ps: " + ps.material);
                        if (ps.material.name.Contains("FuseBoxOnParticles")) // EmberSpark is only available when there is a Brute in the round.
                        {
                            //Debug.Log("Using material through ps: " + ps.material);
                            psr.material = ps.material;
                            break;
                        }
                    }
                }

                ParticleSystem particleSystem = electricTrap.gameObject.AddComponent<ParticleSystem>();
                Utilities.CopyParticleSystem(particleSystem, FindObjectOfType<Welder>().sparks);

                ParticleSystem.MainModule main = particleSystem.main;
                main.startSize = 0.025f;
                main.startSpeed = new ParticleSystem.MinMaxCurve(5f, 10f);
                main.startSpeedMultiplier = 1f;
                main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.35f);
                main.gravityModifier = 1f;
                main.simulationSpeed = 1f;

                ParticleSystem.ShapeModule shape = particleSystem.shape;
                shape.shapeType = ParticleSystemShapeType.Box;
                shape.box = new Vector3(1.25f, 0.05f, 1.25f);
                shape.randomDirectionAmount = 0.45f;

                // Using ColorOverLifetime Doesn't work because material uses wrong shader.

                particleSystem.transform.Rotate(new Vector3(0f, 90f, 0f));

                ParticleSystem.EmissionModule psem = particleSystem.emission;
                psem.enabled = true;
            }

            public void SpawnElectricTrap(Bounds roomBounds, float buffPercentage, float customSlowFactor = 0f, float customLocalScale = 0f, float customTime = 0f)
            {
                GameObject electricTrap = Instantiate<GameObject>(electricTrapPrefab, roomBounds.center - 0.75f * new Vector3(0f, roomBounds.extents.y, 0f), Quaternion.identity);

                PlayerSlower playerSlower = electricTrap.AddComponent<PlayerSlower>();
                playerSlower.slowFactor = customSlowFactor == 0f ? sparkyElectricTrapBaseSlowFactor - maxTrapSlowFactorDecreaseFromSparkyBuff * buffPercentage : customSlowFactor; // This does not act like a percentage due to how the slowing is applied.
                electricTrap.transform.localScale *= customLocalScale == 0f ? sparkyElectricTrapBaseScaleMultiplier + maxTrapScaleMultiplierIncreaseFromSparkyBuff * buffPercentage : customLocalScale;
                electricTrap.SetActive(true);

                float maxRate = 1000f;
                ParticleSystem particleSystem = electricTrap.GetComponent<ParticleSystem>();
                ParticleSystem.EmissionModule psem = particleSystem.emission;
                psem.rateOverTime = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0f), new Keyframe(0.15f, maxRate), new Keyframe(0.70f, maxRate), new Keyframe(1f, 0f)));

                float lifeTime = customTime == 0f ? sparkyElectricTrapBaseLifeTime + maxTrapLifeTimeIncreaseFromSparkyBuff * buffPercentage : customTime;
                ParticleSystem.MainModule main = particleSystem.main;
                main.duration = lifeTime;

                Destroy(electricTrap, lifeTime);
            }

            public IEnumerator ElectrifyRegion(PrimaryRegionType primaryRegionType)
            {
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Trying to electrify region.");
                }
                if (FuseBoxManager.Instance.fuseboxes.ContainsKey(primaryRegionType) && SparkyAura.empDictionary.ContainsKey(primaryRegionType) && !FuseBoxManager.Instance.fuseboxes[primaryRegionType].powered && regionsElectrified.ContainsKey(primaryRegionType) && !regionsElectrified[primaryRegionType])
                {
                    //Debug.Log("Electrifying region.");
                    regionsElectrified[primaryRegionType] = true;
                    foreach (Room room in FuseBoxManager.Instance.rooms[primaryRegionType])
                    {
                        if (room.RoomType == RoomStructure.Corridor)
                        {
                            TimeScaleManager.Instance.StartCoroutine(ContinuouslySpawnElectricTrapWithRandomTimer(primaryRegionType, room.RoomBounds));
                        }
                    }
                    while (!FuseBoxManager.Instance.fuseboxes[primaryRegionType].powered)
                    {
                        yield return null;
                    }
                    //Debug.Log("Region has been powered on. Stopping electrification.");
                    //Debug.Log("Switching rooms back on.");
                    List<Room> regionRooms = FuseBoxManager.Instance.rooms[primaryRegionType];
                    for (int i = 0; i < regionRooms.Count; i++)
                    {
                        ToggleRoomLightSwitch toggleRoomLightSwitch = regionRooms[i].GetComponentInChildren<ToggleRoomLightSwitch>();
                        if (toggleRoomLightSwitch != null && UnityEngine.Random.value <= regionElectrificationRoomRecoveryPercentage && !toggleRoomLightSwitch.lightSwitch[0].On)
                        {
                            toggleRoomLightSwitch.OnHandGrab();
                        }
                    }
                    regionsElectrified[primaryRegionType] = false;
                }
                yield break;
            }

            public IEnumerator ContinuouslySpawnElectricTrapWithRandomTimer(PrimaryRegionType primaryRegionType, Bounds roomBounds)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(regionElectricTrapMinSpawnTime / 2f, regionElectricTrapMaxSpawnTime / 2f));
                while (!FuseBoxManager.Instance.fuseboxes[primaryRegionType].powered)
                {
                    if (UnityEngine.Random.value <= regionElectricTrapSpawnChance)
                    {
                        SpawnElectricTrap(roomBounds, 0f, regionElectricTrapSlowFactor, regionElectricTrapScaleMultiplier, regionElectricTrapLifeTime);
                    }
                    yield return new WaitForSeconds(UnityEngine.Random.Range(regionElectricTrapMinSpawnTime, regionElectricTrapMaxSpawnTime));
                }
                yield break;
            }

            public IEnumerator SpawnElectricTrapWithRandomTimer(Bounds roomBounds, float buffPercentage, float minTime, float maxTime, float customSlowFactor = 0f, float customLocalScale = 0f, float customTime = 0f)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(minTime, maxTime));
                SpawnElectricTrap(roomBounds, buffPercentage, customSlowFactor, customLocalScale, customTime);
                yield break;
            }
        }

        class PlayerSlower : MonoBehaviour
        {
            BoxCollider boxCollider;
            public float slowFactor;
            List<IEnumerator> slowingProcesses;
            List<int> motorIDs;
            AudioSource audioSource;

            void Start()
            {
                boxCollider = gameObject.AddComponent<BoxCollider>();
                boxCollider.size = new Vector3(1.25f, 0.25f, 1.25f);
                boxCollider.isTrigger = true;
                slowingProcesses = new List<IEnumerator>();
                motorIDs = new List<int>();
                audioSource = gameObject.AddComponent<AudioSource>();
                AudioSource monsterSource = References.Monster.GetComponent<Monster>().AudSource;
                monsterSource.CopyTo(audioSource);
                monsterSource.CopyEffectsTo(audioSource);
            }

            void Update()
            {
                Vector3 closestPlayerPosition;
                if (ModSettings.enableMultiplayer && !MultiplayerMode.useLegacyAudio)
                {
                    closestPlayerPosition = MultiplayerMode.newPlayerClasses[MultiplayerMode.ClosestPlayerToThis(this.transform.position)].transform.position;
                }
                else
                {
                    closestPlayerPosition = References.Player.transform.position;
                }
                if (Vector3.Distance(closestPlayerPosition, this.transform.position) < 8f && Mathf.Abs(closestPlayerPosition.y - this.transform.position.y) < 0.5f)
                {
                    //AudioSystem.PlaySound("Noises/Enviro/Electrical/Sparks/Long", audioSource);
                    AudioSystem.PlaySound("Noises/Enviro/Electrical/Sparks/Short", audioSource);
                    //AudioSystem.PlaySound("Noises/Atmosphere/Buzzes", audioSource);
                }
            }

            void OnTriggerEnter(Collider _collider)
            {
                if (_collider.name.Contains("humanBody"))
                {
                    //Debug.Log("Slowing down player through OnTriggerEnter!");
                    PlayerMotor playerMotor = _collider.gameObject.GetComponentInParent<NewPlayerClass>().Motor;
                    IEnumerator slowingProcess = SlowPlayer(playerMotor);
                    slowingProcesses.Add(slowingProcess);
                    motorIDs.Add(playerMotor.GetInstanceID());
                    base.StartCoroutine(slowingProcess);
                }
            }

            void OnTriggerExit(Collider _collider)
            {
                if (_collider.name.Contains("humanBody"))
                {
                    //Debug.Log("Stopping player slow down through OnTriggerExit!");
                    PlayerMotor playerMotor = _collider.gameObject.GetComponentInParent<NewPlayerClass>().Motor;
                    int motorID = playerMotor.GetInstanceID();
                    for (int i = 0; i < slowingProcesses.Count; i++)
                    {
                        if (motorID == motorIDs[i])
                        {
                            base.StopCoroutine(slowingProcesses[i]); // How to stop coroutine with parameters? - Bunny83 - https://answers.unity.com/questions/891122/how-to-stop-coroutine-with-parameters.html - Accessed 20.08.2022
                            slowingProcesses.RemoveAt(i);
                            motorIDs.RemoveAt(i);
                        }
                    }
                }
            }

            IEnumerator SlowPlayer(PlayerMotor playerMotor)
            {
                for (; ; )
                {
                    // This reduces much more over time than intended:
                    /*
                    Player movement stats are: x = 0, z = 4.4. With a slow factor of 0.75 this should give 0 and 3.3
                    Player movement stats are: x = 0, z = 3.749148. With a slow factor of 0.75 this should give 0 and 2.811861
                    Player movement stats are: x = 0, z = 3.190663. With a slow factor of 0.75 this should give 0 and 2.392997
                    Player movement stats are: x = 0, z = 2.817843. With a slow factor of 0.75 this should give 0 and 2.113382
                    Player movement stats are: x = 0, z = 2.564431. With a slow factor of 0.75 this should give 0 and 1.923323
                    */
                    //Debug.Log("Player movement stats are: x = " + playerMotor.xMovement + ", z = " + playerMotor.zMovement + ". With a slow factor of " + slowFactor + " this should give " + playerMotor.xMovement * slowFactor + " and " + playerMotor.zMovement * slowFactor);
                    playerMotor.xMovement *= slowFactor;
                    playerMotor.zMovement *= slowFactor;
                    yield return null;
                }
            }
        }
    }
}
// ~End Of File