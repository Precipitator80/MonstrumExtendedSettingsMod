// ~Beginning Of File
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Collections;

namespace MonstrumExtendedSettingsMod
{

    public partial class ExtendedSettingsModScript
    {
        /*----------------------------------------------------------------------------------------------------*/
        // ~Sparky
        public class Sparky : MESMMonster
        {
            public static MESMMonster CreateSparky(MonsterSelection monsterSelection)
            {
                MESMMonster mESMMonster = CreateMESMMonster(monsterSelection, Monster.MonsterTypeEnum.Brute);
                mESMMonster.gameObject.AddComponent<Sparky>();
                return mESMMonster;
            }

            protected override void Awake()
            {
                base.Awake();
                mState = monster.GetComponent<MState>();

                //// Brute Sparky
                this.gameObject.name = "Sparky";
                monster.monsterType = "Sparky";
                this.gameObject.AddComponent<SparkyAura>();

                // Make FSM adjustments
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

                // Add aura and disruptor for chases
                FiendAura fiendAura = ModSettings.GiveMonsterFiendAuraAndDisruptor(monster, 0.3f, 2.5f, 5f);
                if (!ModSettings.giveAllMonstersAFiendAura)
                {
                    fiendAura.enabled = false;
                    monster.GetComponent<FiendLightDisruptor>().enabled = false;
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

                // Change Brute Sparky's Light
                Light[] lights = monster.GetComponentsInChildren<Light>();
                bruteEyes = new Light[] { lights[4] /*Brute's left eye*/, lights[5] /*Brute's right eye*/ };
                foreach (Light light in lights)
                {
                    light.color = new Color(1f, 1f, 1f, 0f);
                }

                // Sparky with Model
                if (ModSettings.customSparkyModel || ModSettings.customSparkyMusic)
                {
                    SparkyMode.LoadSparkyAssetBundle();
                }

                if (ModSettings.customSparkyModel)
                {
                    // Create Simple Sparky
                    GameObject simpleSparkyGO = Instantiate(SparkyMode.sparkyPrefab);
                    simpleSparkyGO.SetActive(true);
                    simpleSparkyGO.transform.SetParent(this.gameObject.transform, false);
                    SkinnedMeshRenderer sparkySkinnedMeshRenderer = simpleSparkyGO.GetComponentInChildren<SkinnedMeshRenderer>();
                    sparkySkinnedMeshRenderer.enabled = true;

                    Animator sparkyAnimator = simpleSparkyGO.GetComponent<Animator>();
                    sparkyAnimator.enabled = false;

                    Animator monsterAnimator = this.gameObject.GetComponentInChildren<Animator>();

                    foreach (AnimationClip bruteClip in monsterAnimator.runtimeAnimatorController.animationClips)
                    {
                        foreach (AnimationClip sparkyClip in SparkyMode.sparkyAnimatorOCPrefab.animationClips)
                        {
                            if (sparkyClip.name.Contains(bruteClip.name))
                            {
                                sparkyClip.wrapMode = bruteClip.wrapMode;
                                if (sparkyClip.events.Length == 0)
                                {
                                    for (int eventIndex = 0; eventIndex < bruteClip.events.Length; eventIndex++)
                                    {
                                        // Add the event to Sparky's animation.
                                        AnimationEvent bruteEvent = bruteClip.events[eventIndex];

                                        AnimationEvent sparkyEvent = new AnimationEvent();
                                        sparkyEvent.functionName = bruteEvent.functionName;
                                        sparkyEvent.intParameter = bruteEvent.intParameter;
                                        sparkyEvent.floatParameter = bruteEvent.floatParameter;
                                        sparkyEvent.stringParameter = bruteEvent.stringParameter;
                                        sparkyEvent.objectReferenceParameter = bruteEvent.objectReferenceParameter;
                                        float scaledBruteTime = (bruteEvent.time / bruteClip.length); // Scale the event to Sparky's animation.
                                        sparkyEvent.time = sparkyClip.length * scaledBruteTime;
                                        sparkyClip.AddEvent(sparkyEvent);
                                    }
                                }
                            }
                        }
                    }

                    monsterAnimator.runtimeAnimatorController = Instantiate(SparkyMode.sparkyAnimatorOCPrefab);
                    monsterAnimator.avatar = sparkyAnimator.avatar;
                    Debug.Log("Does Brute use root motion? " + monsterAnimator.applyRootMotion);
                    monsterAnimator.applyRootMotion = true;
                    SkinnedMeshRenderer monsterSMR = this.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();

                    monsterSMR.sharedMesh = sparkySkinnedMeshRenderer.sharedMesh; // I don't think it is recommended to edit anything using sharedMesh. Does this break other Brute models?
                    monsterSMR.bones = sparkySkinnedMeshRenderer.bones;
                    monsterSMR.material = sparkySkinnedMeshRenderer.material;
                    monsterSMR.materials = sparkySkinnedMeshRenderer.materials;
                    monsterSMR.rootBone = sparkySkinnedMeshRenderer.rootBone;
                    monsterSMR.localBounds = sparkySkinnedMeshRenderer.localBounds;
                    monsterSMR.lightProbeUsage = sparkySkinnedMeshRenderer.lightProbeUsage;

                    BaseFeatures.DisableMonsterParticles(this.gameObject);

                    // Sparky Eyes
                    MeshRenderer eyeLMR = Utilities.RecursiveTransformSearch(monster.gameObject.transform, "Eye_Inner.L").gameObject.GetComponentInChildren<MeshRenderer>();
                    MeshRenderer eyeRMR = Utilities.RecursiveTransformSearch(monster.gameObject.transform, "Eye_Inner.R").gameObject.GetComponentInChildren<MeshRenderer>();

                    Material eyeLM = eyeLMR.material;
                    Material eyeRM = eyeRMR.material;
                    eyeLM.SetColor("_EmissionColor", Color.red * 5f);
                    eyeRM.SetColor("_EmissionColor", Color.red * 5f);
                    sparkyEyes = new Material[] { eyeLM, eyeRM };

                    // Brute Eyes
                    // Set Brute light positions to fit Sparky.
                    lights[4].transform.SetParent(eyeLMR.transform.parent);
                    lights[5].transform.SetParent(eyeRMR.transform.parent);
                    lights[4].transform.localPosition = Vector3.zero;
                    lights[5].transform.localPosition = Vector3.zero;
                    Vector3 eyeOffset = 0.075f * Vector3.forward;
                    Debug.Log("Sparky eye offset: " + eyeOffset);
                    lights[4].transform.position += eyeOffset;
                    lights[5].transform.position += eyeOffset;
                    lights[4].transform.localRotation = Quaternion.Euler(-75f, 0f, 0f);
                    lights[5].transform.localRotation = Quaternion.Euler(-75f, 0f, 0f);

                    /*
                    Debug.Log("Eye Light Position: " + lights[4].transform.position + " | EyeVision Position: " + monster.EyeVision.transform.position + " | LocalPosition: " + monster.EyeVision.transform.localPosition + " | EV GO P: " + monster.EyeVision.gameObject.transform.position + " | EV GO LP: " + monster.EyeVision.gameObject.transform.localPosition);
                    */

                    GameObject cameraTransformGameObject = new GameObject("Sparky Camera Transform");
                    cameraTransformGameObject.transform.SetParent(lights[4].transform.parent);
                    cameraTransformGameObject.transform.localPosition = Vector3.zero;
                    monster.EyeVision.GetComponentInChildren<FollowTransform2>().TranToFollow = cameraTransformGameObject.transform;

                    /*
                    Debug.Log("Eye Light Position: " + lights[4].transform.position + " | EyeVision Position: " + monster.EyeVision.transform.position + " | LocalPosition: " + monster.EyeVision.transform.localPosition + " | EV GO P: " + monster.EyeVision.gameObject.transform.position + " | EV GO LP: " + monster.EyeVision.gameObject.transform.localPosition);
                    */

                    /*
                    if (ModSettings.sparkyDebugLight)
                    {
                        Light spotlight = monster.EyeVision.gameObject.AddComponent<Light>();
                        spotlight.type = LightType.Spot;
                        spotlight.color = Color.magenta;
                        if (ModSettings.sparkyDebugLightSmallerAngle)
                        {
                            spotlight.range *= 2f;
                        }
                        else
                        {
                            spotlight.spotAngle = monster.EyeVision.coneAngle;
                        }
                    }
                    */

                    // Set their starting colour.
                    foreach (Light light in lights)
                    {
                        light.color = new Color(0f, 0f, 0f, 0f);
                    }

                    // Set their intensity and range.
                    lights[4].intensity /= 15f;
                    lights[5].intensity /= 15f;
                    lights[4].range *= 1.5f;
                    lights[5].range *= 1.5f;
                }
            }

            void Update()
            {
                // Change aggro timers
                if (mState.Fsm.Current.GetType() == typeof(MChasingState))
                {
                    aggroTimer = ModSettings.sparkyLurkMaxAggro;
                }
                else if (mState.Fsm.Current.GetType() != typeof(MLurkState))
                {
                    aggroTimer -= Time.deltaTime / 5f;
                }

                // Clamp the aggro timer
                aggroTimer = Mathf.Clamp(aggroTimer, 0f, ModSettings.sparkyLurkMaxAggro);

                // Change lights
                float scaledAggro = aggroTimer / ModSettings.sparkyLurkMaxAggro;
                float scaledColour = upperLimit - upperLimit * scaledAggro; // Go to 0 as aggro is maxed (red) and upper limit as aggro is minimised (white).
                if (ModSettings.customSparkyModel)
                {
                    foreach (Material material in sparkyEyes)
                    {
                        material.SetColor("_EmissionColor", new Color(upperLimit, scaledColour, scaledColour, 1f) * 5f);
                    }

                    foreach (Light light in bruteEyes)
                    {
                        light.color = new Color(upperLimit * scaledAggro, 0f, 0f, 1f);
                    }
                }
                else
                {
                    // Brute Sparky
                    foreach (Light light in bruteEyes)
                    {
                        light.color = new Color(upperLimit, scaledColour, scaledColour, 1f);
                    }
                }
            }

            Light[] bruteEyes;
            Material[] sparkyEyes;
            public float aggroTimer;
            static float upperLimit = 0.84f;
            MState mState;
        }

        /*----------------------------------------------------------------------------------------------------*/
        // ~SparkyMode
        private static class SparkyMode
        {
            private static bool sparkyInGame;

            // #SparkyModeAfterGenerationInitialisation
            public static void SparkyModeAfterGenerationInitialisation()
            {
                sparkyInGame = false;
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

                ElectricTrapManager electricTrapManager = References.Player.AddComponent<ElectricTrapManager>();

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

            public static void LoadSparkyAssetBundle()
            {
                try
                {
                    if (sparkyPrefab == null)
                    {
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
                    if (ModSettings.giveAllMonstersAFireShroud || (ModSettings.bruteFireShroud && mChasingState.monster.MonsterType == Monster.MonsterTypeEnum.Brute))
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
                    if (ModSettings.giveAllMonstersAFireShroud || (ModSettings.bruteFireShroud && mChasingState.monster.MonsterType == Monster.MonsterTypeEnum.Brute))
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
                if (sparkyInGame && (!ModSettings.darkShip || ModSettings.powerableLights))
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
        }

        // ~MLurkState
        // The monster will be seeing the player when entering this state.
        public class MLurkState : MState
        {
            Sparky sparky;
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

            public void Start()
            {
                sparky = monster.gameObject.GetComponent<Sparky>();
            }

            public override void OnEnter()
            {
                base.OnEnter();
                typeofState = FSMState.StateTypes.LowAlert;
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
                    sparky.aggroTimer += Time.deltaTime;
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
                if (sparky.aggroTimer == ModSettings.sparkyLurkMaxAggro || ManyMonstersMode.PlayerToMonsterDistance(base.monster) < minDistanceToPlayer || base.monster.TimeOutVision.TimeElapsed > 3f)
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
            public static readonly Dictionary<PrimaryRegionType, PrimaryRegionType> empDictionary = new Dictionary<PrimaryRegionType, PrimaryRegionType>(){
                {PrimaryRegionType.CrewDeck, PrimaryRegionType.CrewDeck},
                {PrimaryRegionType.OuterDeckCargo, PrimaryRegionType.CrewDeck},
                {PrimaryRegionType.LowerDeck, PrimaryRegionType.LowerDeck},
                {PrimaryRegionType.CargoHold, PrimaryRegionType.LowerDeck},
                {PrimaryRegionType.SubEscape, PrimaryRegionType.LowerDeck},
                {PrimaryRegionType.UpperDeck, PrimaryRegionType.UpperDeck},
                {PrimaryRegionType.OuterDeck, PrimaryRegionType.UpperDeck},
                {PrimaryRegionType.Engine, PrimaryRegionType.Engine}
            };

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
                            base.StartCoroutine(ElectricTrapManager.instance.SpawnElectricTrapWithRandomTimer(nearbyRoom, buffPercentage, 500f, minTime, maxTime));
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

            public Vector3 TrapPositionWithOffset(Room room)
            {
                return room.RoomBounds.center - 0.75f * new Vector3(0f, room.RoomBounds.extents.y, 0f);
            }

            public void SpawnElectricTrap(Room room, float buffPercentage, float maxRate, float customSlowFactor = 0f, float customLocalScale = 0f, float customTime = 0f)
            {
                GameObject electricTrap = Instantiate<GameObject>(electricTrapPrefab, TrapPositionWithOffset(room), Quaternion.identity, room.transform);

                PlayerSlower playerSlower = electricTrap.AddComponent<PlayerSlower>();
                playerSlower.slowFactor = customSlowFactor == 0f ? sparkyElectricTrapBaseSlowFactor - maxTrapSlowFactorDecreaseFromSparkyBuff * buffPercentage : customSlowFactor; // This does not act like a percentage due to how the slowing is applied.
                electricTrap.transform.localScale *= customLocalScale == 0f ? sparkyElectricTrapBaseScaleMultiplier + maxTrapScaleMultiplierIncreaseFromSparkyBuff * buffPercentage : customLocalScale;
                electricTrap.SetActive(true);

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
                            TimeScaleManager.Instance.StartCoroutine(ContinuouslySpawnElectricTrapWithRandomTimer(primaryRegionType, room));
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

            public IEnumerator ContinuouslySpawnElectricTrapWithRandomTimer(PrimaryRegionType primaryRegionType, Room room)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(regionElectricTrapMinSpawnTime / 2f, regionElectricTrapMaxSpawnTime / 2f));
                while (!FuseBoxManager.Instance.fuseboxes[primaryRegionType].powered)
                {
                    if (UnityEngine.Random.value <= regionElectricTrapSpawnChance)
                    {
                        Vector3 closestPlayerPosition;
                        Vector3 trapPositionWithOffset = TrapPositionWithOffset(room);
                        if (ModSettings.enableMultiplayer && !MultiplayerMode.useLegacyAudio)
                        {
                            closestPlayerPosition = MultiplayerMode.newPlayerClasses[MultiplayerMode.ClosestPlayerToThis(trapPositionWithOffset)].transform.position;
                        }
                        else
                        {
                            closestPlayerPosition = References.Player.transform.position;
                        }
                        if (Vector3.Distance(closestPlayerPosition, trapPositionWithOffset) < 16f && Mathf.Abs(closestPlayerPosition.y - trapPositionWithOffset.y) < 0.5f)
                        {
                            SpawnElectricTrap(room, 0f, 250f, regionElectricTrapSlowFactor, regionElectricTrapScaleMultiplier, regionElectricTrapLifeTime);
                        }
                    }
                    yield return new WaitForSeconds(UnityEngine.Random.Range(regionElectricTrapMinSpawnTime, regionElectricTrapMaxSpawnTime));
                }
                yield break;
            }

            public IEnumerator SpawnElectricTrapWithRandomTimer(Room room, float buffPercentage, float maxRate, float minTime, float maxTime, float customSlowFactor = 0f, float customLocalScale = 0f, float customTime = 0f)
            {
                yield return new WaitForSeconds(UnityEngine.Random.Range(minTime, maxTime));
                SpawnElectricTrap(room, buffPercentage, maxRate, customSlowFactor, customLocalScale, customTime);
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