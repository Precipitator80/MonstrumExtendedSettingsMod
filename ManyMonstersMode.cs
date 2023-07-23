// ~Beginning Of File
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using MonoMod.RuntimeDetour;
using System.Diagnostics;

namespace MonstrumExtendedSettingsMod
{
    public partial class ExtendedSettingsModScript
    {
        /*----------------------------------------------------------------------------------------------------*/
        // ~ManyMonstersMode

        private static class ManyMonstersMode
        {
            public static List<GameObject> monsterList;
            public static List<Monster> monsterListMonsterComponents;
            private static Dictionary<int, int> monsterInstanceIDtoMonsterNumberDict;
            private static List<MState> monsterListStates;

            public static List<GameObject> brutes;
            public static List<Monster> brutesMonsterComponents;
            private static Dictionary<int, int> brutesInstanceIDs;

            public static List<GameObject> hunters;
            public static List<Monster> huntersMonsterComponents;
            private static Dictionary<int, int> huntersInstanceIDs;

            private static List<MTrappingState> huntersTrappingStates;
            private static List<bool> sightBelowThresholdList;
            private static List<bool> soundBelowThresholdList;
            private static List<bool> proxBelowThresholdList;
            private static List<bool> allBelowThresholdList;
            private static List<float> hunterThresholdValList;
            public static bool logHunterActions;

            public static List<GameObject> fiends;
            public static List<Monster> fiendsMonsterComponents;
            private static Dictionary<int, int> fiendsInstanceIDs;

            private static Monster lastMonsterSeen;
            public static Monster lastMonsterSentMessage;


            public static List<bool> monstersFinishedLerpToHidingSpot;
            public static List<bool> monstersFinishedGrab;

            public readonly static Dictionary<IEnumerator, IEnumerator> IEnumeratorDictionary = new Dictionary<IEnumerator, IEnumerator>();

            // #ManyMonstersModeAfterGenerationInitialisation
            public static void ManyMonstersModeAfterGenerationInitialisation()
            {
                monstersFinishedLerpToHidingSpot = new List<bool>();
                monstersFinishedGrab = new List<bool>();

                foreach (GameObject gameObject in monsterList)
                {
                    monstersFinishedLerpToHidingSpot.Add(true);
                    monstersFinishedGrab.Add(true);
                }

                if (hunters != null)
                {
                    ambushPointsSet = 0;
                    deployedPoints = new List<AmbushPoint>();
                    trapsActiveFor = new Timer();
                    deployingTraps = false;
                    deployingSuccessfully = false;
                    logHunterActions = false;

                    if (logHunterActions)
                    {
                        Debug.Log("Logging Hunter Actions");
                    }

                    huntersTrappingStates = new List<MTrappingState>();
                    sightBelowThresholdList = new List<bool>(new bool[hunters.Count]);
                    soundBelowThresholdList = new List<bool>(new bool[hunters.Count]);
                    proxBelowThresholdList = new List<bool>(new bool[hunters.Count]);
                    allBelowThresholdList = new List<bool>(new bool[hunters.Count]);
                    hunterThresholdValList = new List<float>(new float[hunters.Count]);

                    for (int i = 0; i < hunters.Count; i++)
                    {
                        huntersTrappingStates.Add(huntersMonsterComponents[i].TrappingState);
                        sightBelowThresholdList.Add(true);
                        soundBelowThresholdList.Add(true);
                        proxBelowThresholdList.Add(true);
                        allBelowThresholdList.Add(true);
                        hunterThresholdValList.Add(0f);
                    }
                }

                if (fiends != null)
                {
                    fiendsThatAreInRangeOfPlayer = new List<bool>(new bool[fiends.Count]);
                    isAFiendInRangeOfPlayer = false;
                    fiendsThatSeePlayer = new List<bool>(new bool[fiends.Count]);
                    doesAFiendSeeThePlayer = false;
                    fiendMindAttackPlayerAttackTimers = new List<float>();
                    fiendMindAttackPlayerDelayTimers = new List<float>();
                    fiendMindAttackPlayerCurrentClip = new List<string>();
                    fiendMindAttackFiendsTargetingPlayer = new List<List<int>>();
                    if (auras == null)
                    {
                        auras = new List<FiendAura>();
                    }

                    if (!ModSettings.enableMultiplayer)
                    {
                        fiendMindAttackPlayerAttackTimers.Add(0f);
                        fiendMindAttackPlayerDelayTimers.Add(0f);
                        fiendMindAttackPlayerCurrentClip.Add("");
                        fiendMindAttackFiendsTargetingPlayer.Add(new List<int>());
                    }
                    else
                    {
                        for (int i = 0; i < ModSettings.NumberOfPlayers; i++)
                        {
                            fiendMindAttackPlayerAttackTimers.Add(0f);
                            fiendMindAttackPlayerDelayTimers.Add(0f);
                            fiendMindAttackPlayerCurrentClip.Add("");
                            fiendMindAttackFiendsTargetingPlayer.Add(new List<int>());
                        }
                    }

                    for (int i = 0; i < fiends.Count; i++)
                    {
                        fiendMindAttackFiendsTargetingPlayer[0].Add(MonsterNumber(fiendsMonsterComponents[i].GetInstanceID()));
                        auras.Add(fiends[i].GetComponentsInChildren<FiendAura>(true)[0]);
                    }

                    /*
                    SpriteRenderer[] allLights = Resources.FindObjectsOfTypeAll<SpriteRenderer>();
                    try
                    {
                        foreach (SpriteRenderer light in allLights)
                        {
                            if (light != null)
                            {
                                Debug.Log(light.name);
                                light.color = Color.red;
                            }
                        }
                    }
                    catch
                    {
                        Debug.Log("Error while looking at lights.");
                    }
                    */
                }

                if (ModSettings.useMonsterUpdateGroups)
                {
                    DeclareMonsterGroups();
                }

                if (ModSettings.monsterSpawnSpeedrunSpawnTime != 0)
                {
                    TimeScaleManager.Instance.StartCoroutine(MonsterSpawnSpeedrunMonsterSpawner());
                }

                alternatingMonsterNumbersToSpawn = new List<int>();

                if (ModSettings.alternatingMonstersMode && ModSettings.numberOfMonsters > ModSettings.numberOfAlternatingMonsters)
                {
                    while (alternatingMonsterNumbersToSpawn.Count < ModSettings.numberOfAlternatingMonsters)
                    {
                        int randomMonsterNumber = UnityEngine.Random.Range(0, ModSettings.numberOfMonsters);
                        if (!alternatingMonsterNumbersToSpawn.Contains(randomMonsterNumber))
                        {
                            alternatingMonsterNumbersToSpawn.Add(randomMonsterNumber);
                        }
                    }
                }

                /*
                if (ModSettings.numberOfMonsters > 0)
                {
                    lastMonsterSentMessage = monsterListMonsterComponents[0];
                }
                */
            }

            // #InitialiseManyMonstersMode
            public static void InitialiseManyMonstersMode()
            {
                Debug.Log("INITIALISING MANY MONSTERS MODE");
                HookDoorScripts();
                HookMiscellaneousMonsterScripts();
                HookMonsterStateScripts();
                HookObjectScripts();
                HookRoomSearchScripts();
                HookUtilityScripts();
                HookBonusFeatures();
                Debug.Log("INITIALISED MANY MONSTERS MODE");
            }

            // #HookDoorScripts
            private static void HookDoorScripts()
            {
                On.BarricadeDoorDestroyEffect.Start += new On.BarricadeDoorDestroyEffect.hook_Start(HookBarricadeDoorDestroyEffect);
                HookDoor();
                On.DoorBreak.OnDoorDestroy += new On.DoorBreak.hook_OnDoorDestroy(HookDoorBreak);
                On.DoorDebris.Force += new On.DoorDebris.hook_Force(HookDoorDebris);
                HookDoorJoint();
                On.LockerReverb.Update += new On.LockerReverb.hook_Update(HookLockerReverb);
                On.MetalDoorBreak.OnDoorDestroy += new On.MetalDoorBreak.hook_OnDoorDestroy(HookMetalDoorBreak);
                On.SealedDoorBreach.OnDoorDestroy += new On.SealedDoorBreach.hook_OnDoorDestroy(HookSealedDoorBreach);
            }

            // #HookMiscellaeousMonsterScripts
            private static void HookMiscellaneousMonsterScripts()
            {
                HookAmbushPoint();
                HookAmbushSystem();
                HookAnimationControl();
                On.AnimationEvents.Mec_AnimationFinished += new On.AnimationEvents.hook_Mec_AnimationFinished(HookAnimationEvents);
                On.AnimationLayerController.AnimatorNullCheck += new On.AnimationLayerController.hook_AnimatorNullCheck(HookAnimationLayerController);
                On.ChooseAttack.ChooseSoundByMonster += new On.ChooseAttack.hook_ChooseSoundByMonster(HookChooseAttackChooseSoundByMonster);
                HookClimbCheck();
                On.ConeControl.Start += new On.ConeControl.hook_Start(HookConeControl);
                HookDetectRoom();
                On.DistractionSound.Init += new On.DistractionSound.hook_Init(HookDistractionSound);
                On.EncounterMonster.Update += new On.EncounterMonster.hook_Update(HookEncounterMonster);
                HookFiendLightController();
                HookFiendLightDisruptor();
                HookFiendMindAttack();
                HookHunterAnimationsScript();
                HookJumpCheck();
                On.MAlertMeters.ProximityChecker += new On.MAlertMeters.hook_ProximityChecker(HookMAlertMeters);
                HookMonstDetectRoom();
                HookMonster();
                On.MonsterApproachCamera.OnMonsterApproach += new On.MonsterApproachCamera.hook_OnMonsterApproach(HookMonsterApproachCamera);
                On.MonsterBreathing.Update += new On.MonsterBreathing.hook_Update(HookMonsterBreathing);
                HookMonsterHearing();
                On.MonsterFootsteps.Start += new On.MonsterFootsteps.hook_Start(HookMonsterFootsteps);
                On.MonsterLight.Update += new On.MonsterLight.hook_Update(HookMonsterLight);
                HookMonsterReaction();
                HookMonsterStarter();
                On.MovementControl.LerpToThis += new On.MovementControl.hook_LerpToThis(HookMovementControl);
                HookPlayMonsterAnimation();
                HookResizeCones();
                HookVision();
            }

            // #HookMonsterStateScripts
            private static void HookMonsterStateScripts()
            {
                On.FSM.Update += new On.FSM.hook_Update(HookFSM);
                On.LockedInState.LerpToVent += new On.LockedInState.hook_LerpToVent(HookLockedInState);
                HookMAttackingState2();
                HookMChasingState();
                HookMClimbingState();
                new Hook(typeof(MDestroyState).GetNestedType("<LerpToDoor>c__Iterator0", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.ManyMonstersMode).GetMethod("HookMDestroyStateIntermediateHook"), null);
                HookMHideState();
                HookMHuntingState();
                On.MIdleState.OnUpdate += new On.MIdleState.hook_OnUpdate(HookMIdleState);
                // On.MInvestigateState.OnUpdate += new On.MInvestigateState.hook_OnUpdate(HookMInvestigateState); Code contains nothing?
                HookMRoomSearch();
                HookMSearchingState();
                On.MState.OnReset += new On.MState.hook_OnReset(HookMState);
                HookMTrappingState();
                On.MVentFrenzyState.OnUpdate += new On.MVentFrenzyState.hook_OnUpdate(HookMVentFrenzyState);
                On.MWanderState.OnUpdate += new On.MWanderState.hook_OnUpdate(HookMWanderState);
            }

            // #HookObjectScripts
            private static void HookObjectScripts()
            {
                On.DualSteamVent.DoStun += new On.DualSteamVent.hook_DoStun(HookDualSteamVent);
                HookFlareObject();
                On.Flashlight.UpdateLightVolume += new On.Flashlight.hook_UpdateLightVolume(HookFlashlight);
                On.Helicopter.Update += new On.Helicopter.hook_Update(HookHelicopter);
                HookHelicopterEscape();
                On.Liferaft.Start += new On.Liferaft.hook_Start(HookLiferaft);
                HookPitTrap();
                On.Sub.Update += new On.Sub.hook_Update(HookSub);
                HookSubAlarm();
                On.Submarine.Start += new On.Submarine.hook_Start(HookSubmarine);
                On.Trolley.BlockingCode += new On.Trolley.hook_BlockingCode(HookTrolley);
            }

            // #HookRoomSearchScripts
            private static void HookRoomSearchScripts()
            {
                On.DraggedOutHiding.DragPlayer += new On.DraggedOutHiding.hook_DragPlayer(HookDraggedOutHidingDragPlayer);
                On.DragPlayer.Mec_OnReleasePlayer += new On.DragPlayer.hook_Mec_OnReleasePlayer(HookDragPlayer);
                new Hook(typeof(LerpToHidingSpot).GetNestedType("<LerpToPosRot>c__Iterator0", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.ManyMonstersMode).GetMethod("HookLerpToHidingSpotIntermediateHook"), null);
                new Hook(typeof(LerpToSearchSpot).GetNestedType("<LerpToPosRot>c__Iterator0", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.ManyMonstersMode).GetMethod("HookLerpToSearchSpotIntermediateHook"), null);
                On.NoIntroAnimation.OnHidingEventTriggered += new On.NoIntroAnimation.hook_OnHidingEventTriggered(HookNoIntroAnimation);
                On.NoSearchAnimation.OnHidingEventTriggered += new On.NoSearchAnimation.hook_OnHidingEventTriggered(HookNoSearchAnimation);
                On.RipOffCurtain.Mec_OnGrabDoor += new On.RipOffCurtain.hook_Mec_OnGrabDoor(HookRipOffCurtain);
                HookRipOutPlayer();
            }

            // #HookUtilityScripts
            private static void HookUtilityScripts()
            {
                On.AlarmManager.MonsterApproach += new On.AlarmManager.hook_MonsterApproach(HookAlarmManager);
                On.EscapeChecker.UpdateAllCompleteness += new On.EscapeChecker.hook_UpdateAllCompleteness(HookEscapeChecker);
                On.GameplayAudio.OnSoundPlayed += new On.GameplayAudio.hook_OnSoundPlayed(HookGameplayAudio);
                HookGlobalMusic();
                On.LevelGeneration.Awake += new On.LevelGeneration.hook_Awake(HookLevelGenerationAwake);
                new Hook(typeof(RenderOnce).GetNestedType("<Render>c__Iterator1", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.ManyMonstersMode).GetMethod("HookRenderOnceIntermediateHook"), null);
                // On.Room.ChooseRandomRoomID += new On.Room.hook_ChooseRandomRoomID(HookRoom); // Breaks Level Generation.
            }

            // #HookBonusFeatures

            private static void HookBonusFeatures()
            {
                // Indev / New Area
                On.Inventory.AddToInventory += new On.Inventory.hook_AddToInventory(HookInventoryAddToInventory);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AlarmManager

            private static void HookAlarmManager(On.AlarmManager.orig_MonsterApproach orig, AlarmManager alarmManager, Transform _monster)
            {
                alarmManager.StopTheAlarm();
                Monster monster = _monster.gameObject.GetComponentInParent<Monster>();
                monster.GetComponent<MovementControl>().MaxSpeed = 0f;
                monster.GetComponent<MonsterHearing>().ForceDrop = true;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AmbushPoint

            private static void HookAmbushPoint()
            {
                On.AmbushPoint.OnTriggerEnter += new On.AmbushPoint.hook_OnTriggerEnter(HookAmbushPointOnTriggerEnter);
                On.AmbushPoint.Update += new On.AmbushPoint.hook_Update(HookAmbushPointUpdate);
                On.AmbushPoint.TrapTriggered += new On.AmbushPoint.hook_TrapTriggered(HookAmbushPointTrapTriggered);
            }

            private static void HookAmbushPointOnTriggerEnter(On.AmbushPoint.orig_OnTriggerEnter orig, AmbushPoint ambushPoint, Collider _triggerer)
            {
                if (ambushPoint.monster != null && ambushPoint.monster.TheSubAlarm != null && ambushPoint.deployed && !ambushPoint.monster.SubEventBeenStarted())
                {
                    bool triggeredByItem = _triggerer.GetComponentInParent<InventoryItem>() != null;
                    if ((PlayerHelper.IsPlayerBody(_triggerer) || triggeredByItem) && (_triggerer.transform.parent == null || (_triggerer.transform.parent != null && _triggerer.transform.parent.name != "Trolley")) && !ambushPoint.IsTriggered && (!ModSettings.enableCrewVSMonsterMode || (ModSettings.enableCrewVSMonsterMode && !MultiplayerMode.monsterPlayers.Contains(_triggerer.GetComponentInParent<NewPlayerClass>()) && CrewVsMonsterMode.letAIControlMonster)))
                    {
                        foreach (MTrappingState mTrappingState in huntersTrappingStates)
                        {
                            if (!mTrappingState.OutOfTrap && !mTrappingState.TrapSet)
                            {
                                ambushPoint.trapState = mTrappingState;

                                if (logHunterActions)
                                {
                                    Debug.Log("Attempting ambush from AmbushPoint");
                                }
                                if (triggeredByItem)
                                {
                                    if (_triggerer.attachedRigidbody != null && _triggerer.attachedRigidbody.velocity.magnitude > 0f)
                                    {
                                        ambushPoint.TrapTriggered(_triggerer, true);
                                        return;
                                    }
                                }
                                else
                                {
                                    ambushPoint.TrapTriggered(_triggerer, false);
                                    return;
                                }
                            }
                        }
                    }
                }
            }

            private static void HookAmbushPointUpdate(On.AmbushPoint.orig_Update orig, AmbushPoint ambushPoint)
            {
                if (AmbushSystem.AllowDisarm && ambushPoint.deployed && !ambushPoint.CanTrapBeSeen())
                {
                    if (logHunterActions)
                    {
                        Debug.Log("Disarming trap from update");
                    }
                    ambushPoint.DisarmTrap();
                }
            }

            private static void HookAmbushPointTrapTriggered(On.AmbushPoint.orig_TrapTriggered orig, AmbushPoint ambushPoint, Collider _triggerCol, bool _triggerIsItem)
            {
                ambushPoint.triggered = true;
                ambushPoint.trigger.enabled = false;
                if (ambushPoint.trapState != null)
                {
                    if (ambushPoint.lookAt == null)
                    {
                        ambushPoint.lookAt = _triggerCol.transform;
                    }
                    ambushPoint.trapState.triggeredAmbush = ambushPoint;
                    ambushPoint.trapState.TriggerCollider = _triggerCol;
                    if (ambushPoint.trapState.SetOffTrap(ambushPoint.MonsterSpawnPoint.position, ambushPoint.lookAt.position))
                    {
                        DisarmAfterTime(ambushPoint, 2f);
                        if (_triggerIsItem)
                        {
                            Achievements.Instance.CompleteAchievement("ITEM_HUNTER_TRAP");
                        }
                    }
                }
            }

            private static IEnumerator DisarmAfterTime(AmbushPoint ambushPoint, float timeToWait)
            {
                float t = 0;
                while (t < timeToWait)
                {
                    t += Time.deltaTime;
                    yield return null;
                }
                ambushPoint.DisarmTrap();
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AmbushSystem

            private static void HookAmbushSystem()
            {
                On.AmbushSystem.DeactivateTraps += new On.AmbushSystem.hook_DeactivateTraps(HookAmbushSystemDeactivateTraps);
                On.AmbushSystem.Deploy += new On.AmbushSystem.hook_Deploy(HookAmbushSystemDeploy);
                On.AmbushSystem.DeployAreaAmbushes += new On.AmbushSystem.hook_DeployAreaAmbushes(HookAmbushSystemDeployAreaAmbushes);
                On.AmbushSystem.Expand += new On.AmbushSystem.hook_Expand(HookAmbushSystemExpand);
                On.AmbushSystem.OnExploreFinished += new On.AmbushSystem.hook_OnExploreFinished(HookAmbushSystemOnExploreFinished);
                On.AmbushSystem.RaycastExplore += new On.AmbushSystem.hook_RaycastExplore(HookAmbushSystemRaycastExplore);
            }


            private static IEnumerator HookAmbushSystemRaycastExplore(On.AmbushSystem.orig_RaycastExplore orig, Vector3 _start, Vector3 _direction)
            {
                Debug.Log("AmbushSystem.RaycastExplore StackTrace:\n" + new StackTrace().ToString());
                yield return orig.Invoke(_start, _direction);
            }

            private static void HookAmbushSystemDeactivateTraps(On.AmbushSystem.orig_DeactivateTraps orig)
            {
                if (!AreOtherHuntersInTrappingState(true) && AmbushSystem.AllowDisarm)
                {
                    if (logHunterActions)
                    {
                        Debug.Log("Deactivating traps from ambushsystem deactivatetraps");
                    }
                    orig.Invoke();
                    ambushPointsSet = 0;
                    AmbushSystem.AllowDisarm = false;
                }
            }

            private static AmbushPoint FindAmbush(Vector3 monsterPointOfInterest, bool isAmbushToHideIn, Monster monster)
            {
                AmbushPoint ambushPoint = null;
                float zeroFloat = 0f;
                monsterPointOfInterest += Vector3.up;
                RoomCategory playerRoomCategory = monster.PlayerDetectRoom.GetRoomCategory;
                AmbushPoint.TrapType trapType = AmbushPoint.TrapType.Pod;
                if (playerRoomCategory != RoomCategory.Outside)
                {
                    if (playerRoomCategory != RoomCategory.Cargo)
                    {
                        if (playerRoomCategory == RoomCategory.Engine)
                        {
                            trapType = AmbushPoint.TrapType.EnginePipe;
                        }
                    }
                    else
                    {
                        trapType = AmbushPoint.TrapType.Cargo;
                    }
                }
                else
                {
                    trapType = AmbushPoint.TrapType.Deck;
                }
                for (int i = 0; i < AmbushSystem.ambushPoints.Count; i++)
                {
                    AmbushPoint potentialAmbushPoint = AmbushSystem.ambushPoints[i];
                    float distanceBetweenAmbushAndInterestPoint = Vector3.Distance(potentialAmbushPoint.transform.position, monsterPointOfInterest);
                    float heightDifference = Mathf.Abs(potentialAmbushPoint.transform.position.y - monsterPointOfInterest.y);
                    if (trapType == AmbushPoint.TrapType.Pod)
                    {
                        trapType = potentialAmbushPoint.trapType;
                    }
                    if ((!isAmbushToHideIn && trapType == potentialAmbushPoint.trapType && potentialAmbushPoint.trapType != AmbushPoint.TrapType.LWDVent) || (isAmbushToHideIn && (potentialAmbushPoint.trapType == AmbushPoint.TrapType.Ceiling || potentialAmbushPoint.trapType == AmbushPoint.TrapType.EnginePipe || potentialAmbushPoint.trapType == AmbushPoint.TrapType.Deck)))
                    {
                        if (potentialAmbushPoint.trapType != trapType)
                        {
                            heightDifference *= 100f;
                            distanceBetweenAmbushAndInterestPoint += heightDifference;
                        }
                        if (ambushPoint == null || (distanceBetweenAmbushAndInterestPoint < zeroFloat && distanceBetweenAmbushAndInterestPoint > 3f))
                        {
                            zeroFloat = distanceBetweenAmbushAndInterestPoint;
                            ambushPoint = potentialAmbushPoint;
                        }
                    }
                }
                return ambushPoint;
            }

            private static void HookAmbushSystemDeploy(On.AmbushSystem.orig_Deploy orig, AmbushSystem.OnAmbushDeploy _onDeploy)
            {
                if (logHunterActions)
                {
                    Debug.Log("Disarming is " + AmbushSystem.AllowDisarm + ", ambushPointsSet is " + ambushPointsSet + " and deployingTraps is " + deployingTraps);
                }
                if ((AmbushSystem.AllowDisarm || ambushPointsSet == 0) && !deployingTraps)
                {
                    if (logHunterActions)
                    {
                        Debug.Log("Got to 1");
                    }
                    AmbushSystem.onDeploy = _onDeploy;
                    if (logHunterActions)
                    {
                        Debug.Log("Clearing ambush points from deploy.");
                    }
                    AmbushSystem.finalAmbushes.Clear();
                    if (logHunterActions)
                    {
                        Debug.Log("Got to 2");
                    }
                    Monster monster = lastMonsterSentMessage;
                    NodeData playerNodeData;
                    if (!ModSettings.enableCrewVSMonsterMode || (ModSettings.enableCrewVSMonsterMode && !CrewVsMonsterMode.setUpAtPlayerMonster))
                    {
                        if (logHunterActions)
                        {
                            Debug.Log("Got to 2.1");
                            ModSettings.ShowTextOnScreen("Using player position for deploying traps");
                        }
                        playerNodeData = LevelGeneration.GetNodeDataAtPosition(monster.player.transform.position + Vector3.up / 2f);
                    }
                    else
                    {
                        if (lastMonsterSentMessage == null)
                        {
                            if (logHunterActions)
                            {
                                Debug.Log("Last monster sent message is null");
                            }
                            if (monsterListMonsterComponents == null)
                            {
                                if (logHunterActions)
                                {
                                    Debug.Log("Monster list monster components is also null");
                                }
                            }
                            else
                            {
                                if (logHunterActions)
                                {
                                    Debug.Log("Listing monster list monster components 0 monster instance ID");
                                }
                                monster = monsterListMonsterComponents[0];
                                Debug.Log(monsterListMonsterComponents[0].GetInstanceID());
                            }
                        }
                        else
                        {
                            if (logHunterActions)
                            {
                                Debug.Log("Listing last monster sent message instance ID");
                                Debug.Log(lastMonsterSentMessage.GetInstanceID());
                            }
                        }
                        if (logHunterActions)
                        {
                            //Debug.Log("Monster deploying number is " + monsterDeployingNumber);
                            Debug.Log("Got to 2.20");
                        }
                        int monsterInstanceID = monster.GetInstanceID();
                        if (logHunterActions)
                        {
                            Debug.Log("Got to 2.21");
                        }
                        int monsterNumber = MonsterNumber(monsterInstanceID);
                        if (logHunterActions)
                        {
                            Debug.Log("Got to 2.22");
                        }
                        NewPlayerClass playerMonster = MultiplayerMode.monsterPlayers[monsterNumber];
                        if (logHunterActions)
                        {
                            Debug.Log("Got to 2.23");
                        }
                        playerNodeData = LevelGeneration.GetNodeDataAtPosition(playerMonster.transform.position);
                        if (logHunterActions)
                        {
                            Debug.Log("Got to 2.24");
                            ModSettings.ShowTextOnScreen("Using monster position for deploying traps");
                        }
                    }
                    if (logHunterActions)
                    {
                        Debug.Log("Got to 3");
                    }
                    if (playerNodeData == null && logHunterActions)
                    {
                        Debug.Log("Player node data is null.");
                    }
                    else if (logHunterActions)
                    {
                        Debug.Log("Player node data is not null.");
                        if (playerNodeData.nodeRoom == null)
                        {
                            Debug.Log("Player node data is null.");
                        }
                        else
                        {
                            Debug.Log("Player node data is not null.");
                            if (playerNodeData.nodeRoom.PrimaryRegion == PrimaryRegionType.SubEscape)
                            {
                                Debug.Log("Player node room region is sub escape.");
                            }
                        }
                    }

                    for (int i = 0; i < AmbushSystem.ambushPoints.Count; i++)
                    {
                        if (playerNodeData != null && playerNodeData.nodeRoom != null)
                        {
                            if (playerNodeData.nodeRoom.PrimaryRegion != PrimaryRegionType.SubEscape)
                            {
                                AmbushSystem.ambushPoints[i].PrepareTrapForSearch();
                            }
                        }
                        else
                        {
                            AmbushSystem.ambushPoints[i].PrepareTrapForSearch();
                        }
                    }
                    if (logHunterActions)
                    {
                        Debug.Log("Got to 4");
                    }
                    AmbushSystem.AllowDisarm = false;
                    if (playerNodeData != null)
                    {
                        if (logHunterActions)
                        {
                            Debug.Log("Got to 5");
                        }
                        Room playerNodeRoom = playerNodeData.nodeRoom;
                        if (playerNodeRoom != null)
                        {
                            if (logHunterActions)
                            {
                                Debug.Log("Got to 6");
                            }
                            RoomStructure roomType = playerNodeRoom.RoomType;
                            if (roomType == RoomStructure.Room || (ModSettings.enableCrewVSMonsterMode && !CrewVsMonsterMode.letAIControlMonster))
                            {
                                if (roomType != RoomStructure.Room)
                                {
                                    if (logHunterActions)
                                    {
                                        Debug.Log("Node room is not of type room.");
                                    }
                                    NewPlayerClass playerMonster = MultiplayerMode.monsterPlayers[MonsterNumber(monster.GetInstanceID())];
                                    Room[] allRoomsUnfiltered = FindObjectsOfType<Room>();
                                    List<Room> allRooms = new List<Room>();
                                    foreach (Room room in allRoomsUnfiltered)
                                    {
                                        if (room.RoomType == RoomStructure.Room)
                                        {
                                            allRooms.Add(room);
                                        }
                                    }

                                    if (allRooms.Count > 0)
                                    {
                                        if (logHunterActions)
                                        {
                                            Debug.Log("Using closest room for raycasting in Deploy");
                                        }
                                        Room closestRoom = allRooms[0];
                                        float closestDoorDistance = Vector3.Distance(playerMonster.transform.position, allRooms[0].transform.position);
                                        for (int doorToCheck = 1; doorToCheck < allRooms.Count; doorToCheck++)
                                        {
                                            float distanceToDoor = Vector3.Distance(playerMonster.transform.position, allRooms[doorToCheck].transform.position);
                                            if (distanceToDoor < closestDoorDistance)
                                            {
                                                closestRoom = allRooms[doorToCheck];
                                                closestDoorDistance = distanceToDoor;
                                            }
                                        }
                                        playerNodeRoom = closestRoom;
                                    }
                                }

                                if (logHunterActions)
                                {
                                    Debug.Log("Got to 7");
                                }
                                AmbushSystem.finishedCount = playerNodeRoom.roomDoors.Count;
                                if (playerNodeRoom.roomDoors != null)
                                {
                                    if (logHunterActions)
                                    {
                                        Debug.Log("Got to 8");
                                    }
                                    deployingTraps = true;
                                    AmbushSystem.instance.StartCoroutine(CheckWhetherDeployingSuccessfully());
                                    if (playerNodeRoom.roomDoors.Count == 0 && ModSettings.enableCrewVSMonsterMode && !CrewVsMonsterMode.letAIControlMonster && CrewVsMonsterMode.setUpAtPlayerMonster)
                                    {
                                        if (logHunterActions)
                                        {
                                            Debug.Log("No room doors found for deploy but hunter requested spawning. Using closest door instead.");
                                        }
                                        NewPlayerClass playerMonster = MultiplayerMode.monsterPlayers[MonsterNumber(monster.GetInstanceID())];
                                        Door[] allDoors = FindObjectsOfType<Door>();
                                        if (allDoors.Length > 0)
                                        {
                                            if (logHunterActions)
                                            {
                                                Debug.Log("Using closest door for raycasting in Deploy");
                                            }
                                            Door closestDoor = allDoors[0];
                                            float closestDoorDistance = Vector3.Distance(playerMonster.transform.position, allDoors[0].transform.position);
                                            for (int doorToCheck = 1; doorToCheck < allDoors.Length; doorToCheck++)
                                            {
                                                float distanceToDoor = Vector3.Distance(playerMonster.transform.position, allDoors[doorToCheck].transform.position);
                                                if (distanceToDoor < closestDoorDistance)
                                                {
                                                    closestDoor = allDoors[doorToCheck];
                                                    closestDoorDistance = distanceToDoor;
                                                }
                                            }
                                            playerNodeRoom.roomDoors.Add(closestDoor);
                                        }
                                        else
                                        {
                                            if (logHunterActions)
                                            {
                                                Debug.Log("Could not find any doors at all in Deploy");
                                            }
                                        }
                                    }
                                    foreach (Door door in playerNodeRoom.roomDoors)
                                    {
                                        Transform doorTransform = door.transform;
                                        Vector3 doorPosition = doorTransform.position;
                                        doorPosition -= doorTransform.forward;
                                        doorPosition = MathHelper.RoundToNearest(doorPosition, Settings.CuboidDim);
                                        doorPosition += Settings.CuboidDim / 2f;
                                        if (logHunterActions)
                                        {
                                            Debug.Log("Starting RaycastExplore coroutine");
                                            ModSettings.ShowTextOnScreen("Starting RaycastExplore coroutine");
                                        }
                                        AmbushSystem.instance.StartCoroutine(AmbushSystem.RaycastExplore(doorPosition, -doorTransform.forward));
                                    }

                                    Debug.Log("Got to 9");
                                }
                                else if (logHunterActions)
                                {
                                    ModSettings.ShowTextOnScreen("Room doors is null");
                                }
                            }
                        }
                        else if (logHunterActions)
                        {
                            ModSettings.ShowTextOnScreen("Node room is null");
                        }
                    }
                    else if (logHunterActions)
                    {
                        ModSettings.ShowTextOnScreen("Node data is null");
                    }
                }
                if (CrewVsMonsterMode.setUpAtPlayerMonster)
                {
                    CrewVsMonsterMode.setUpAtPlayerMonster = false;
                }
            }

            private static void HookAmbushSystemDeployAreaAmbushes(On.AmbushSystem.orig_DeployAreaAmbushes orig, AmbushPoint.TrapType _trapType, AmbushSystem.OnAmbushDeploy _onDeploy)
            {
                if (logHunterActions)
                {
                    Debug.Log("Running deploy area ambushes");
                    ModSettings.ShowTextOnScreen("Running deploy area ambushes");
                }
                AmbushSystem.onDeploy = _onDeploy;
                if (AmbushSystem.AllowDisarm)
                {
                    if (logHunterActions)
                    {
                        Debug.Log("Clearing ambush points from deployareaambushes.");
                    }
                    AmbushSystem.finalAmbushes.Clear();
                }
                AmbushSystem.AllowDisarm = false;
                for (int i = 0; i < AmbushSystem.ambushPoints.Count; i++)
                {
                    if (AmbushSystem.ambushPoints[i].trapType == _trapType && !AmbushSystem.ambushPoints[i].CanTrapBeSeen() && !AmbushSystem.ambushPoints[i].deployed && !AmbushSystem.finalAmbushes.Contains(AmbushSystem.ambushPoints[i]))
                    {
                        AmbushSystem.ambushPoints[i].DeployTrap();
                        AmbushSystem.finalAmbushes.Add(AmbushSystem.ambushPoints[i]);
                    }
                }
                AmbushSystem.onDeploy(AmbushSystem.finalAmbushes);
            }

            private static void HookAmbushSystemExpand(On.AmbushSystem.orig_Expand orig, Vector3 _start, Vector3 _position, List<Vector3> _expandList, HashSet<Vector3> _expanded, List<AmbushPoint> foundAmbushPoints)
            {
                deployingSuccessfully = true;
                if (!_expanded.Contains(_position))
                {
                    _expanded.Add(_position);
                }
                if (logHunterActions)
                {
                    Debug.Log("The hunter factor will be less than or equal to " + Math.Ceiling(hunters.Count / 3f));
                    Debug.Log("AmbushSystem.Expand StackTrace:\n" + new StackTrace().ToString());
                    ModSettings.ShowTextOnScreen("Running Ambush System expand using start " + _start + " and position " + _position);
                }
                for (int hunterFactor = 1; hunterFactor <= Math.Ceiling(hunters.Count / 3f); hunterFactor++)
                {
                    for (int i = 0; i < AmbushSystem.directions.Count; i++)
                    {
                        Vector3 vector = _position + AmbushSystem.directions[i] * Settings.CuboidDim.x * hunterFactor;
                        NodeData nodeDataAtPosition = LevelGeneration.GetNodeDataAtPosition(vector);
                        if (CheckBoundariesLG.WorldPositionInShipBounds(vector) && nodeDataAtPosition != null && nodeDataAtPosition.nodeRoom != null && !_expanded.Contains(vector) && !_expandList.Contains(vector))
                        {
                            if (!Physics.Raycast(_position, AmbushSystem.directions[i], out AmbushSystem.hit, Settings.CuboidDim.x, AmbushSystem.instance.mask))
                            {
                                _expanded.Add(vector);
                                _expandList.Add(vector);
                            }
                            else if (AmbushSystem.hit.collider.attachedRigidbody != null)
                            {
                                AmbushPoint ambushPoint = AmbushSystem.hit.collider.attachedRigidbody.GetComponent<AmbushPoint>();
                                if (ambushPoint != null)
                                {
                                    if (!ambushPoint.SearchIgnore && Vector3.Distance(_start, vector) > AmbushSystem.instance.ignoreRadius && !foundAmbushPoints.Contains(ambushPoint))
                                    {
                                        foundAmbushPoints.Add(ambushPoint);
                                    }
                                    else
                                    {
                                        _expanded.Add(vector);
                                        _expandList.Add(vector);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            private static void HookAmbushSystemOnExploreFinished(On.AmbushSystem.orig_OnExploreFinished orig)
            {
                if (logHunterActions)
                {
                    Debug.Log("Running on explore finished");
                }
                for (int i = 0; i < AmbushSystem.ambushPoints.Count; i++)
                {
                    if (!AmbushSystem.ambushPoints[i].CanTrapBeSeen())
                    {
                        AmbushSystem.ambushPoints[i].DisarmTrap();
                    }
                }
                for (int j = 0; j < AmbushSystem.finalAmbushes.Count; j++)
                {
                    if (!AmbushSystem.finalAmbushes[j].deployed)
                    {
                        if (logHunterActions)
                        {
                            Debug.Log("Deploying trap from on explore finished: " + AmbushSystem.finalAmbushes[j].transform.position);
                        }
                        AmbushSystem.finalAmbushes[j].DeployTrap();
                    }
                }
                AmbushSystem.onDeploy(AmbushSystem.finalAmbushes);
                deployingTraps = false;
                deployingSuccessfully = false;
            }

            private static AmbushPoint FindAmbushInArea(Vector3 monsterPointOfInterest, float minDisAway, float maxDisAway, Monster monster, bool shouldBeEggSac)
            {
                if (ModSettings.enableCrewVSMonsterMode && CrewVsMonsterMode.setUpAtPlayerMonster)
                {
                    monsterPointOfInterest = MultiplayerMode.monsterPlayers[MonsterNumber(monster.GetInstanceID())].transform.position; //AmbushSystem.ambushPoints[currentAmbushPoint[playerMonsterNumber]].transform.position;
                }
                AmbushPoint result = null;
                AmbushPoint ambushPoint = null;
                List<AmbushPoint> list = new List<AmbushPoint>();
                float minimum = 0f;
                float maximum = 999f;
                if (!shouldBeEggSac)
                {
                    for (int i = 0; i < AmbushSystem.ambushPoints.Count; i++)
                    {
                        AmbushPoint potentialAmbushPoint = AmbushSystem.ambushPoints[i];
                        if (potentialAmbushPoint != null && potentialAmbushPoint.trapType != AmbushPoint.TrapType.Pod && potentialAmbushPoint.trapType != AmbushPoint.TrapType.LWDPod && potentialAmbushPoint.trapType != AmbushPoint.TrapType.LWDVent)
                        {
                            Room componentInParent = potentialAmbushPoint.GetComponentInParent<Room>();
                            RoomCategory getRoomCategory = monster.PlayerDetectRoom.GetRoomCategory;
                            AmbushPoint.TrapType trapType = AmbushPoint.TrapType.Vent;
                            if (componentInParent != null)
                            {
                                if (componentInParent.RegionNode.y == monster.PlayerDetectRoom.GetRoom.RegionNode.y || getRoomCategory == RoomCategory.Engine || getRoomCategory == RoomCategory.Cargo || getRoomCategory == RoomCategory.Outside)
                                {
                                    switch (getRoomCategory)
                                    {
                                        case RoomCategory.Engine:
                                            trapType = AmbushPoint.TrapType.EnginePipe;
                                            break;
                                        default:
                                            if (getRoomCategory != RoomCategory.Outside)
                                            {
                                                if (potentialAmbushPoint.trapType == AmbushPoint.TrapType.Ceiling)
                                                {
                                                    trapType = AmbushPoint.TrapType.Ceiling;
                                                }
                                                else
                                                {
                                                    trapType = AmbushPoint.TrapType.Vent;
                                                }
                                            }
                                            else
                                            {
                                                trapType = AmbushPoint.TrapType.Deck;
                                            }
                                            break;
                                        case RoomCategory.Cargo:
                                            trapType = AmbushPoint.TrapType.Cargo;
                                            break;
                                    }
                                }
                            }
                            else if (getRoomCategory == RoomCategory.Engine)
                            {
                                trapType = AmbushPoint.TrapType.EnginePipe;
                            }
                            if (trapType == potentialAmbushPoint.trapType)
                            {

                                float ambushToInterest = Vector3.Distance(potentialAmbushPoint.transform.position, monsterPointOfInterest);
                                float ambushToPlayer;
                                if (!ModSettings.enableCrewVSMonsterMode || (ModSettings.enableCrewVSMonsterMode && !CrewVsMonsterMode.setUpAtPlayerMonster))
                                {
                                    ambushToPlayer = Vector3.Distance(potentialAmbushPoint.transform.position, monster.player.transform.position);
                                }
                                else
                                {
                                    ambushToPlayer = ambushToInterest;
                                }
                                float heightFactor = Mathf.Abs(potentialAmbushPoint.transform.position.y - monsterPointOfInterest.y) * 10f;
                                if (ambushToInterest < maximum)
                                {
                                    maximum = ambushToInterest;
                                    ambushPoint = potentialAmbushPoint;
                                }
                                if (ambushToInterest > minDisAway && ambushToInterest < maxDisAway && ambushToPlayer > minDisAway)
                                {
                                    list.Add(potentialAmbushPoint);
                                    minimum += ambushToInterest + heightFactor;
                                }
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < AmbushSystem.ambushPoints.Count; i++)
                    {
                        AmbushPoint potentialAmbushPoint = AmbushSystem.ambushPoints[i];
                        if (potentialAmbushPoint.IsDeployed && !potentialAmbushPoint.IsTriggered)
                        {
                            float ambushToInterest = Vector3.Distance(potentialAmbushPoint.transform.position, monsterPointOfInterest);
                            float ambushToPlayer;
                            if (!ModSettings.enableCrewVSMonsterMode || (ModSettings.enableCrewVSMonsterMode && !CrewVsMonsterMode.setUpAtPlayerMonster))
                            {
                                ambushToPlayer = Vector3.Distance(potentialAmbushPoint.transform.position, monster.player.transform.position);
                            }
                            else
                            {
                                ambushToPlayer = ambushToInterest;
                            }
                            float heightDifference = Mathf.Abs(potentialAmbushPoint.transform.position.y - monsterPointOfInterest.y) * 10f;
                            if (ambushToInterest < maximum)
                            {
                                maximum = ambushToInterest;
                                ambushPoint = potentialAmbushPoint;
                            }
                            if (ambushToInterest > minDisAway && ambushToInterest < maxDisAway && ambushToPlayer > minDisAway)
                            {
                                list.Add(potentialAmbushPoint);
                                minimum += ambushToInterest + heightDifference;
                            }
                        }
                    }
                }
                if (list.Count > 0)
                {
                    float num6 = minimum / (float)list.Count;
                    List<AmbushPoint> list2 = new List<AmbushPoint>();
                    list2.Clear();
                    float num7 = 99f;
                    foreach (AmbushPoint potentialAmbushPoint in list)
                    {
                        float ambushToInterest = Vector3.Distance(potentialAmbushPoint.transform.position, monsterPointOfInterest);
                        float heightDifference = Mathf.Abs(potentialAmbushPoint.transform.position.y - monsterPointOfInterest.y) * 10f;
                        if (ambushToInterest + heightDifference <= num6)
                        {
                            list2.Add(potentialAmbushPoint);
                        }
                        if (ambushToInterest < num7)
                        {
                            num7 = ambushToInterest;
                            ambushPoint = potentialAmbushPoint;
                        }
                    }
                    if (list2.Count > 0)
                    {
                        int index = UnityEngine.Random.Range(0, list2.Count);
                        if (list2[index] != null)
                        {
                            result = list2[index];
                        }
                    }
                    else
                    {
                        result = ambushPoint;
                    }
                }
                else
                {
                    result = ambushPoint;
                }
                if (result != null)
                {
                    result.triggered = true;
                }
                if (ModSettings.enableCrewVSMonsterMode && CrewVsMonsterMode.setUpAtPlayerMonster)
                {
                    CrewVsMonsterMode.setUpAtPlayerMonster = false;
                }
                return result;
            }

            private static IEnumerator CheckWhetherDeployingSuccessfully()
            {
                yield return new WaitForSeconds(5f);
                if (!deployingSuccessfully)
                {
                    deployingTraps = false;
                }
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AnimationControl

            private static void HookAnimationControl()
            {
                On.AnimationControl.NewGlobalValues += new On.AnimationControl.hook_NewGlobalValues(HookAnimationControlNewGlobalValues);
                On.AnimationControl.TurnHead += new On.AnimationControl.hook_TurnHead(HookAnimationControlTurnHead); // May be unused by the game.
            }

            private static void HookAnimationControlNewGlobalValues(On.AnimationControl.orig_NewGlobalValues orig, AnimationControl animationControl)
            {
                animationControl.chargeDistance = PlayerToMonsterDistance(animationControl.monster);
                animationControl.attackType = ChooseAttack.GetAttack;
                animationControl.CheckForDistraction();
            }

            private static void HookAnimationControlTurnHead(On.AnimationControl.orig_TurnHead orig, AnimationControl animationControl)
            {
                if (PlayerToMonsterCast(animationControl.monster))
                {
                    if (!PlayerToMonsterCast2(animationControl.monster))
                    {
                        bool flag = Vector3.Cross(((MonoBehaviour)animationControl).transform.forward, (animationControl.monster.player.transform.position - animationControl.moveControl.AheadNodePos).normalized).y > 0f;
                        animationControl.headDir = Vector3.Angle(((MonoBehaviour)animationControl).transform.forward, (animationControl.monster.player.transform.position - ((MonoBehaviour)animationControl).transform.position).normalized);
                        if (animationControl.headDir > 1f)
                        {
                            animationControl.headDir = 1f;
                        }
                        if (flag)
                        {
                            animationControl.headDir *= -1f;
                        }
                        animationControl.curHeadDir = Mathf.MoveTowards(animationControl.curHeadDir, animationControl.headDir, 0.2f * Time.deltaTime);
                    }
                    else
                    {
                        animationControl.curHeadDir = Mathf.MoveTowards(animationControl.curHeadDir, 0f, 0.2f * Time.deltaTime);
                    }
                }
                else
                {
                    animationControl.curHeadDir = Mathf.MoveTowards(animationControl.curHeadDir, 0f, 0.2f * Time.deltaTime);
                }
                UnityEngine.Debug.Log(string.Concat(new object[] { "AnimationControl TurnHead " + animationControl.monster.GetInstanceID() }));
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AnimationEvents

            private static void HookAnimationEvents(On.AnimationEvents.orig_Mec_AnimationFinished orig, AnimationEvents animationEvents)
            {
                AnimationControl monsterAnimation = ((MonoBehaviour)animationEvents).GetComponentInParent<AnimationControl>();
                monsterAnimation.fixedAnimation = PlayMonsterAnimation.MonsterAnimation.None;
                monsterAnimation.fixedEffectAnimation = PlayMonsterAnimation.MonsterEffectAnimation.None;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AnimationLayerController

            private static void HookAnimationLayerController(On.AnimationLayerController.orig_AnimatorNullCheck orig, AnimationLayerController animationLayerController)
            {
                if (animationLayerController.animator == null)
                {
                    animationLayerController.animator = ((MonoBehaviour)animationLayerController).GetComponentInParent<Monster>().MoveControl.GetAniControl.GetAnimator;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @BarricadeDoorDestroyEffect

            private static void HookBarricadeDoorDestroyEffect(On.BarricadeDoorDestroyEffect.orig_Start orig, BarricadeDoorDestroyEffect barricadeDoorDestroyEffect)
            {
                MonsterDestroyDoorEffect component = ((MonoBehaviour)barricadeDoorDestroyEffect).GetComponentInParent<MonsterDestroyDoorEffect>();
                Monster.MonsterTypeEnum monsterType = monsterListMonsterComponents[ClosestMonsterToThis(component.door.transform.position)].MonsterType;
                if (monsterType != Monster.MonsterTypeEnum.Hunter)
                {
                    if (monsterType == Monster.MonsterTypeEnum.Brute)
                    {
                        component.destroyEffect = MonsterDestroyDoorEffect.DestroyEffect.Blast;
                        return;
                    }
                    if (monsterType == Monster.MonsterTypeEnum.Fiend)
                    {
                        component.destroyEffect = MonsterDestroyDoorEffect.DestroyEffect.Blast;
                        return;
                    }
                }
                else
                {
                    component.destroyEffect = MonsterDestroyDoorEffect.DestroyEffect.UnlockAndOpen;
                    component.destroyAfter = 3f;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Calculations

            public static bool PlayerToMonsterCast(Monster monster)
            {
                SetupVars(monster);
                Calculations.playerPos = monster.player.transform.position;
                if (Calculations.monstCamera != null)
                {
                    Calculations.mRay.origin = Calculations.monstCamera.transform.position;
                }
                if (Calculations.playerController != null)
                {
                    Calculations.playerPos.y = Calculations.playerPos.y + Calculations.playerController.height * 0.5f;
                }
                Calculations.mRay.direction = Calculations.playerPos - Calculations.mRay.origin;
                return Calculations.DoCast(Calculations.mRay.origin, Calculations.playerPos, Calculations.mRay.direction);
            }

            private static bool PlayerToMonsterCast2(Monster monster)
            {
                Calculations.playerPos = monster.player.transform.position;
                if (Calculations.playerController != null)
                {
                    Calculations.playerPos.y = Calculations.playerPos.y + Calculations.playerController.height * 0.5f;
                }
                Vector3 vector = Calculations.playerPos;
                Calculations.mRay2.origin = monster.gameObject.transform.position + Vector3.up * 0.5f;
                Calculations.mRay2.direction = vector - Calculations.mRay2.origin;
                return Calculations.DoCast(Calculations.mRay2.origin, vector, Calculations.mRay2.direction);
            }

            public static float PlayerToMonsterDistance(Monster monster)
            {
                return Vector3.Distance(monster.gameObject.transform.position, monster.player.transform.position);
            }

            private static void SetupVars(Monster monster)
            {
                Calculations.monstCamera = monster.gameObject.GetComponentInChildren<Camera>();
                Calculations.playerListener = monster.player.GetComponentInChildren<AudioListener>();
                Calculations.playerController = monster.player.GetComponent<CharacterController>();
                Calculations.isSetup = true;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ChooseAttack

            private static void HookChooseAttackChooseSoundByMonster(On.ChooseAttack.orig_ChooseSoundByMonster orig)
            {
                string monsterText = monsterListMonsterComponents[ClosestMonsterToPlayer()].monsterType;

                string attackText = " ";
                switch (ChooseAttack.atkList)
                {
                    case ChooseAttack.AttackList.StandFront:
                        attackText = "Stand/Front";
                        break;
                    case ChooseAttack.AttackList.CrouchFront:
                        attackText = "Crouch/Front";
                        break;
                    case ChooseAttack.AttackList.ProneFront:
                        attackText = "Prone/Front";
                        break;
                    case ChooseAttack.AttackList.StandBehind:
                        attackText = "Stand/Behind";
                        break;
                    case ChooseAttack.AttackList.CrouchBehind:
                        attackText = "Crouch/Behind";
                        break;
                    case ChooseAttack.AttackList.ProneBehind:
                        attackText = "Prone/Behind";
                        break;
                    case ChooseAttack.AttackList.StandHiding:
                        attackText = "Stand/Hiding";
                        break;
                    case ChooseAttack.AttackList.CrouchHidingNoDoor:
                        attackText = "Crouch/Hiding/Table";
                        break;
                    case ChooseAttack.AttackList.ProneHidingL:
                        attackText = "Prone/Hiding/Left";
                        break;
                    case ChooseAttack.AttackList.ProneHidingR:
                        attackText = "Prone/Hiding/Right";
                        break;
                    case ChooseAttack.AttackList.CrouchHidingWithDoor:
                        attackText = "Crouch/Hiding/Counter";
                        break;
                }
                if (monsterText != " " && attackText != " ")
                {
                    ChooseAttack.audioString = "Noises/Animations/Death Animations/" + monsterText + "/" + attackText;
                }
            }

            private static void AttackDirection(Monster monster)
            {
                Vector3 vector = monster.transform.position + Vector3.up;
                Vector3 direction = monster.player.transform.position + Vector3.up - vector;
                direction.Normalize();
                Ray ray = new Ray(vector, direction);
                LayerMask mask = 1 << LayerMask.NameToLayer("TransparentFX");
                RaycastHit raycastHit;
                if (Physics.Raycast(ray, out raycastHit, 5f, mask))
                {
                    AttackDetection component = raycastHit.collider.GetComponent<AttackDetection>();
                    if (component != null)
                    {
                        ChooseAttack._whichSide = component.whichSide;
                        if (ChooseAttack._whichSide == AttackDetection.WhichSide.Front)
                        {
                            ChooseAttack.playerSide = ChooseAttack.PlayerSide.Front;
                            return;
                        }
                        ChooseAttack.playerSide = ChooseAttack.PlayerSide.Back;
                    }
                }
            }

            private static void SetMonsterLayers(Monster monster)
            {
                AnimationLayerController getAniLayerController = monster.GetAniLayerController;
                int attackLayer = getAniLayerController.AttackLayer;
                getAniLayerController.MakeOnlyLayerActive(attackLayer);
            }

            public static void WhatAttackOpen(Monster monster)
            {
                AttackDirection(monster);
                MultiplayerMode.GetPlayerPose(monster.player);
                ChooseAttack.CalculateAttack();
                ChooseAttack.ChooseSoundByMonster();
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ClimbCheck

            private static void HookClimbCheck()
            {
                On.ClimbCheck.OnTriggerEnter += new On.ClimbCheck.hook_OnTriggerEnter(HookClimbCheckOnTriggerEnter);
                On.ClimbCheck.OnTriggerExit += new On.ClimbCheck.hook_OnTriggerExit(HookClimbCheckOnTriggerExit);
            }

            private static void HookClimbCheckOnTriggerEnter(On.ClimbCheck.orig_OnTriggerEnter orig, ClimbCheck climbCheck, Collider _Other)
            {
                if (_Other.transform.root.tag == "Monster")
                {
                    Monster monster = _Other.GetComponentInParent<Monster>();
                    monster.MoveControl.GetAniControl.inClimbingZone = true;
                    monster.Climber.LerpPosition = ((MonoBehaviour)climbCheck).transform.position;
                    monster.Climber.CurrentClimb = climbCheck;
                    monster.Climber.CurrentClimbObj = ((MonoBehaviour)climbCheck).gameObject;
                }
            }

            private static void HookClimbCheckOnTriggerExit(On.ClimbCheck.orig_OnTriggerExit orig, ClimbCheck climbCheck, Collider _Other)
            {
                if (_Other.transform.root.tag == "Monster")
                {
                    _Other.GetComponentInParent<Monster>().MoveControl.GetAniControl.inClimbingZone = false;
                }
            }

            // Custom SetFaceThis moved to BaseFeatures (See MClimbingState).

            /*----------------------------------------------------------------------------------------------------*/
            // @ConeControl

            private static void HookConeControl(On.ConeControl.orig_Start orig, ConeControl coneControl)
            {
                coneControl.monster = ((MonoBehaviour)coneControl).GetComponentInParent<Monster>();
                coneControl.vision = ((MonoBehaviour)coneControl).GetComponentInParent<Vision>();
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DetectRoom

            private static void HookDetectRoom()
            {
                On.DetectRoom.PlayerHidingSpot += new On.DetectRoom.hook_PlayerHidingSpot(HookDetectRoomPlayerHidingSpot);
                On.DetectRoom.Update += new On.DetectRoom.hook_Update(HookDetectRoomUpdate);
            }

            private static HidingSpot HookDetectRoomPlayerHidingSpot(On.DetectRoom.orig_PlayerHidingSpot orig, DetectRoom detectRoom, HidingSpot[] _spots)
            {
                bool isPlayerHiding = false;

                if (monsterList != null)
                {
                    foreach (Monster monster in monsterListMonsterComponents)
                    {
                        if (monster.IsPlayerHiding)
                        {
                            isPlayerHiding = true;
                            break;
                        }
                    }
                }

                // Original Code
                if (_spots != null && isPlayerHiding)
                {
                    HidingSpot hidingSpot = null;
                    float num = 0f;
                    for (int i = 0; i < _spots.Length; i++)
                    {
                        Vector3 hidingSpotPoint = _spots[i].HidingSpotPoint;
                        hidingSpotPoint.y = 0f;
                        Vector3 position;
                        if (!ModSettings.enableMultiplayer)
                        {
                            position = References.CamMiddle.position;
                        }
                        else
                        {
                            position = MultiplayerMode.PlayerCamera(detectRoom.player).transform.position;
                        }
                        position.y = 0f;
                        float num2 = Vector3.Distance(hidingSpotPoint, position);
                        if ((hidingSpot == null || num2 < num) && num2 < 1f)
                        {
                            num = num2;
                            hidingSpot = _spots[i];
                        }
                    }
                    return hidingSpot;
                }
                return null;
            }

            private static void HookDetectRoomUpdate(On.DetectRoom.orig_Update orig, DetectRoom detectRoom)
            {
                detectRoom.UpdateRoomData();

                bool isAMonsterInRoom = false;
                if (monsterList != null)
                {
                    for (int i = 0; i < monsterList.Count && !isAMonsterInRoom; i++)
                    {
                        if (monsterListMonsterComponents[i].IsPlayerHiding)
                        {
                            if (monsterListMonsterComponents[i].RoomDetect.CurrentRoom == detectRoom.room)
                            {
                                isAMonsterInRoom = true;
                            }
                        }
                    }
                }

                if (isAMonsterInRoom)
                {
                    detectRoom.monsterRunType = 1f;
                }
                else
                {
                    detectRoom.monsterRunType = 0f;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DistractionSound

            private static void HookDistractionSound(On.DistractionSound.orig_Init orig, DistractionSound distractionSound, AudioSource _source, AudioLibrary _library, GameplayAudio _audio, Transform _monsterTrans)
            {
                distractionSound.source = _source;
                distractionSound.library = _library;
                distractionSound.gameplayAudio = _audio;
                distractionSound.layerMask.value = 1 << LayerMask.NameToLayer("DefaultNavVision");
                for (int i = 0; i < monsterList.Count; i++)
                {
                    if (monsterList[i].transform == _monsterTrans)
                    {
                        distractionSound.monster = monsterListMonsterComponents[i];
                        break;
                    }
                }
            }

            private static float CalculateVolume(DistractionSound distractionSound, Monster monster)
            {
                float volume = 0f;
                if (distractionSound.source != null)
                {
                    float maxDistance = distractionSound.source.maxDistance;
                    distractionSound.distanceBetween = Vector3.Distance(distractionSound.source.transform.position, monster.GetEars.transform.position);
                    if (distractionSound.distanceBetween < maxDistance)
                    {
                        volume = distractionSound.gameplayAudio.gameVolume;
                        if (!distractionSound.gameplayAudio.IgnoreOcclusion)
                        {
                            volume = volume * Mathf.Clamp01((maxDistance - distractionSound.distanceBetween) / maxDistance) * GetActualVolume(distractionSound, monster);
                        }
                    }
                    if (volume < distractionSound.gameplayAudio.minIncrease)
                    {
                        volume = distractionSound.gameplayAudio.minIncrease;
                    }
                }
                else
                {
                    Debug.Log("DistractionSound source is null");
                }
                return volume;
            }

            private static float GetActualVolume(DistractionSound distractionSound, Monster monster)
            {
                try
                {
                    if (!distractionSound.enableSound)
                    {
                        return 0f;
                    }
                    if (monster != null && monster.HunterAnimations != null && monster.HunterAnimations.isHiding)
                    {
                        return 1f;
                    }
                }
                catch
                {
                    Debug.Log("Error in GetActualVolume 1");
                }
                try
                {


                    Vector3 position = distractionSound.source.transform.position;
                    Vector3 direction = monster.GetEars.transform.position - position;
                    RaycastHit[] array = Physics.RaycastAll(position, direction, distractionSound.distanceBetween, distractionSound.layerMask);
                    distractionSound.divider = 1f + Mathf.Abs(distractionSound.source.transform.position.y - monster.GetEars.transform.position.y);
                    if (distractionSound.divider < 0f)
                    {
                        distractionSound.divider *= -1f;
                    }
                    try
                    {
                        try
                        {
                            if (array != null)
                            {
                                try
                                {
                                    if (distractionSound != null)
                                    {
                                        try
                                        {
                                            if (distractionSound.source != null)
                                            {
                                                try
                                                {
                                                    try
                                                    {
                                                        if (distractionSound.transform != null)
                                                        {
                                                            try
                                                            {
                                                                if (distractionSound.transform.gameObject != null)
                                                                {
                                                                    try
                                                                    {
                                                                        distractionSound.hitlength = array.Length + Mathf.FloorToInt(distractionSound.divider / 3f);
                                                                    }
                                                                    catch
                                                                    {
                                                                        Debug.Log("Could not assign distractionSound.hitlength in GetActualVolume");
                                                                    }
                                                                    try
                                                                    {
                                                                        for (int i = 0; i < array.Length; i++)
                                                                        {
                                                                            try
                                                                            {
                                                                                if (array[i].collider.gameObject == ((MonoBehaviour)distractionSound).transform.gameObject || (distractionSound.hitlength <= 1 && Vector3.Distance(distractionSound.source.transform.position, monster.GetEars.transform.position) < 1.5f))
                                                                                {
                                                                                    distractionSound.hitlength--;
                                                                                }
                                                                            }
                                                                            catch
                                                                            {
                                                                                Debug.Log("Could not run loop in GetActualVolume at index " + i);
                                                                            }
                                                                        }
                                                                    }
                                                                    catch
                                                                    {
                                                                        Debug.Log("Could not run loop in GetActualVolume for some reason");
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    Debug.Log("distractionSound.transform.gameObject is null");
                                                                }
                                                            }
                                                            catch
                                                            {
                                                                Debug.Log("Error in GetActualVolume 3f");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            Debug.Log("distractionSound.transform is null");
                                                        }
                                                    }
                                                    catch
                                                    {
                                                        Debug.Log("Error in GetActualVolume 3d");
                                                    }
                                                }
                                                catch
                                                {
                                                    Debug.Log("Error in GetActualVolume 3c");
                                                }
                                            }
                                            else
                                            {
                                                Debug.Log("GetActualVolume Source is null");
                                            }
                                        }
                                        catch
                                        {
                                            Debug.Log("Error in GetActualVolume 3b");
                                        }
                                    }
                                    else
                                    {
                                        //Debug.Log("DistractionSound is null in 3e."); // This is where the issue occurs.
                                    }
                                }
                                catch
                                {
                                    Debug.Log("Error in GetActualVolume 3e");
                                }
                            }
                            else
                            {
                                Debug.Log("GetActualVolume Array is null");
                            }
                        }
                        catch
                        {
                            Debug.Log("Error in GetActualVolume 3a");
                        }
                    }
                    catch
                    {
                        Debug.Log("Error in GetActualVolume 3");
                    }
                }
                catch
                {
                    Debug.Log("Error in GetActualVolume 2");
                }
                try
                {
                    float volume;
                    if (distractionSound.hitlength == 0 || distractionSound.hitlength < 0)
                    {
                        distractionSound.hitlength = 1;
                        distractionSound.divider = 1f;
                        volume = distractionSound.BaseVolume;
                    }
                    else
                    {
                        volume = distractionSound.BaseVolume / ((float)distractionSound.hitlength * distractionSound.divider);
                    }
                    return volume;
                }
                catch
                {
                    Debug.Log("Error in GetActualVolume 4");
                }
                Debug.Log("Returning 0f in GetActualVolume 5");
                return 0f;
            }

            private static float CalculateVolumeDelta(DistractionSound distractionSound, Monster monster)
            {
                return CalculateVolume(distractionSound, monster) * Time.deltaTime;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Door

            private static void HookDoor()
            {
                On.Door.BlastOffDoor += new On.Door.hook_BlastOffDoor(HookDoorBlastOffDoor);
                On.Door.ClosestSide += new On.Door.hook_ClosestSide(HookDoorClosestSide);
                On.Door.RipOffDoor += new On.Door.hook_RipOffDoor(HookDoorRipOffDoor);
                On.Door.Start += new On.Door.hook_Start(HookDoorStart);
            }

            private static void HookDoorBlastOffDoor(On.Door.orig_BlastOffDoor orig, Door door, float _power)
            {
                if (door.attached)
                {
                    door.UnHinge();
                    door.doorSource.maxDistance = 10f;
                    Vector3 vector = monsterList[ClosestMonsterToThis(((MonoBehaviour)door).transform.position)].transform.forward;
                    vector.y = 0f;
                    vector.Normalize();
                    vector *= _power;
                    door.attached = false;
                    door.doorRigidbody.AddForce(vector, ForceMode.Impulse);
                    ((MonoBehaviour)door).BroadcastMessage("OnDoorDestroy", SendMessageOptions.DontRequireReceiver);
                    AudioSystem.PlaySound(door.onDestroyLib, door.doorSource);
                    door.isOpen = true;
                    door.CheckIfBarricade();
                    door.SetOcclusion(true);
                    door.visionOccluder.layer = LayerMask.NameToLayer("Default");
                    ((MonoBehaviour)door).Invoke("DeactivateAfterTime", 7f);
                }
            }

            private static void BlastOffDoor(GameObject monsterGameObject, Door door, float _power)
            {
                if (door.attached)
                {
                    door.UnHinge();
                    door.doorSource.maxDistance = 10f;
                    Vector3 vector = monsterGameObject.transform.forward;
                    vector.y = 0f;
                    vector.Normalize();
                    vector *= _power;
                    door.attached = false;
                    door.doorRigidbody.AddForce(vector, ForceMode.Impulse);
                    ((MonoBehaviour)door).BroadcastMessage("OnDoorDestroy", SendMessageOptions.DontRequireReceiver);
                    AudioSystem.PlaySound(door.onDestroyLib, door.doorSource);
                    door.isOpen = true;
                    door.CheckIfBarricade();
                    door.SetOcclusion(true);
                    door.visionOccluder.layer = LayerMask.NameToLayer("Default");
                    ((MonoBehaviour)door).Invoke("DeactivateAfterTime", 7f);
                }
            }

            private static GameObject HookDoorClosestSide(On.Door.orig_ClosestSide orig, Door door)
            {
                MonsterDoorTransform[] componentsInChildren = ((MonoBehaviour)door).GetComponentsInChildren<MonsterDoorTransform>();
                GameObject result = null;
                foreach (MonsterDoorTransform monsterDoorTransform in componentsInChildren)
                {
                    Room raycastRoom = null;
                    Ray ray = new Ray(monsterDoorTransform.transform.position + Vector3.up * 0.5f, Vector3.down);
                    Debug.DrawRay(ray.origin, ray.direction, Color.white, 2f);
                    RaycastHit raycastHit;
                    if (Physics.Raycast(ray, out raycastHit, 2f, door.roomLayerMask))
                    {
                        raycastRoom = raycastHit.collider.GetComponentInParent<Room>();
                    }
                    if (raycastRoom != null)
                    {
                        int closestMonster = ClosestMonsterToThis(((MonoBehaviour)door).transform.position);
                        if (monsterListMonsterComponents[closestMonster] != null)
                        {
                            MonstDetectRoom monstDetectRoom = monsterListMonsterComponents[closestMonster].GetComponentInChildren<MonstDetectRoom>();
                            if (monstDetectRoom != null && monstDetectRoom.CurrentRoom != null)
                            {
                                if (raycastRoom == monstDetectRoom.CurrentRoom)
                                {
                                    result = monsterDoorTransform.gameObject;
                                }
                                else
                                {
                                    door.OtherSide = monsterDoorTransform.gameObject;
                                }
                            }
                        }
                    }
                }
                return result;
            }

            private static GameObject ClosestSide(Monster monster, Door door)
            {
                MonsterDoorTransform[] doorMonsterDoorTransforms = ((MonoBehaviour)door).GetComponentsInChildren<MonsterDoorTransform>();
                GameObject result = null;
                foreach (MonsterDoorTransform monsterDoorTransform in doorMonsterDoorTransforms)
                {
                    Room raycastRoom = null;
                    Ray ray = new Ray(monsterDoorTransform.transform.position + Vector3.up * 0.5f, Vector3.down);
                    Debug.DrawRay(ray.origin, ray.direction, Color.white, 2f);
                    RaycastHit raycastHit;
                    if (Physics.Raycast(ray, out raycastHit, 2f, door.roomLayerMask))
                    {
                        raycastRoom = raycastHit.collider.GetComponentInParent<Room>();
                    }
                    if (raycastRoom != null)
                    {
                        //int closestMonster = ClosestMonsterToThis(((MonoBehaviour)door).transform.position);
                        if (monster/*monsterListMonsterComponents[closestMonster]*/ != null)
                        {
                            MonstDetectRoom monstDetectRoom = monster.GetComponentInChildren<MonstDetectRoom>();
                            if (monstDetectRoom != null && monstDetectRoom.CurrentRoom != null)
                            {
                                if (raycastRoom == monstDetectRoom.CurrentRoom)
                                {
                                    result = monsterDoorTransform.gameObject;
                                }
                                else
                                {
                                    door.OtherSide = monsterDoorTransform.gameObject;
                                }
                            }
                        }
                    }
                }
                return result;
            }

            private static void HookDoorRipOffDoor(On.Door.orig_RipOffDoor orig, Door door)
            {
                door.UnHinge();
                door.DamageDoor(1000);
                door.doorSource.maxDistance = 10f;
                door.NavBoxCheck();
                int closestMonster = ClosestMonsterToThis(((MonoBehaviour)door).transform.position);
                /*
                if (!ModSettings.enableCrewVSMonsterMode || (ModSettings.enableCrewVSMonsterMode && (closestMonster >= MultiplayerMode.monsterPlayers.Count)))
                {
                    ((MonoBehaviour)door).transform.parent = monsterListMonsterComponents[closestMonster].CurrentHand.transform;
                }
                */
                ((MonoBehaviour)door).transform.parent = monsterListMonsterComponents[closestMonster].CurrentHand.transform;
                monsterListMonsterComponents[closestMonster].ResetHandSelection();
                door.doorCollider.enabled = false;
                door.doorRigidbody.constraints = RigidbodyConstraints.FreezeAll;
                door.doorRigidbody.useGravity = false;
                door.attached = false;
                door.isOpen = true;
                door.CheckIfBarricade();
            }

            private static void HookDoorStart(On.Door.orig_Start orig, Door door)
            {
                door.monsterDoorSmoke1 = ((MonoBehaviour)door).GetComponentInChildren<MonsterDoorSmoke>();
                if (ModSettings.numberOfFiends == 0)
                {
                    if (door.monsterDoorSmoke1 != null)
                    {
                        door.monsterDoorSmoke1.gameObject.SetActive(false);
                    }
                    if (door.monsterDoorSmoke2 != null)
                    {
                        door.monsterDoorSmoke2.gameObject.SetActive(false);
                    }
                }
                if (door.DoorType == Door.doorType.Barricaded || door.DoorType == Door.doorType.Metal || door.DoorType == Door.doorType.Normal || door.DoorType == Door.doorType.Powered || door.DoorType == Door.doorType.Sealed)
                {
                    LevelGeneration.GetNodeDataAtPosition(((MonoBehaviour)door).transform.position).attachedDoors.Add(door);
                    if (((MonoBehaviour)door).transform.rotation.eulerAngles.y < 1f || (((MonoBehaviour)door).transform.rotation.eulerAngles.y > 179f && ((MonoBehaviour)door).transform.rotation.eulerAngles.y < 181f))
                    {
                        LevelGeneration.GetNodeDataAtPosition(((MonoBehaviour)door).transform.position + new Vector3(0f, 0f, -1f)).attachedDoors.Add(door);
                    }
                    else
                    {
                        LevelGeneration.GetNodeDataAtPosition(((MonoBehaviour)door).transform.position + new Vector3(-1f, 0f, 0f)).attachedDoors.Add(door);
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DoorBreak

            private static void HookDoorBreak(On.DoorBreak.orig_OnDoorDestroy orig, DoorBreak doorBreak)
            {
                for (int i = 0; i < doorBreak.brokenDoor.Length; i++)
                {
                    doorBreak.brokenDoor[i].GetComponentInChildren<Collider>().gameObject.layer = doorBreak.defaultLayer;
                    doorBreak.brokenDoor[i].GetComponent<Rigidbody>().gameObject.layer = doorBreak.defaultLayer;
                    doorBreak.brokenDoor[i].GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    doorBreak.brokenDoor[i].GetComponent<Rigidbody>().mass = 0.5f;
                    doorBreak.brokenDoor[i].GetComponent<Rigidbody>().AddForce(monsterList[ClosestMonsterToThis(((MonoBehaviour)doorBreak).transform.position)].transform.forward, ForceMode.Impulse);
                }
                doorBreak.doorRigidbody.gameObject.SetActive(false);
                InterpolateToPosition component = ((MonoBehaviour)doorBreak).GetComponentInParent<InterpolateToPosition>();
                if (component != null)
                {
                    UnityEngine.Object.Destroy(component);
                }
                ((MonoBehaviour)doorBreak).StartCoroutine(doorBreak.FadeOut());
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DoorDebris

            private static void HookDoorDebris(On.DoorDebris.orig_Force orig, DoorDebris doorDebris)
            {
                Vector3 vector = monsterList[ClosestMonsterToThis(((MonoBehaviour)doorDebris).transform.position)].transform.forward;
                vector.y = 0f;
                vector.Normalize();
                vector *= UnityEngine.Random.Range(doorDebris.forceMin, doorDebris.forceMax);
                ((MonoBehaviour)doorDebris).GetComponent<Rigidbody>().AddForce(vector, ForceMode.Impulse);
                ((MonoBehaviour)doorDebris).GetComponent<Rigidbody>().AddRelativeTorque(UnityEngine.Random.insideUnitSphere, ForceMode.Impulse);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DoorJoint

            private static void HookDoorJoint()
            {
                On.DoorJoint.OnTriggerEnter += new On.DoorJoint.hook_OnTriggerEnter(HookDoorJointOnTriggerEnter);
                On.DoorJoint.OnTriggerExit += new On.DoorJoint.hook_OnTriggerExit(HookDoorJointOnTriggerExit);
            }

            private static void HookDoorJointOnTriggerEnter(On.DoorJoint.orig_OnTriggerEnter orig, DoorJoint doorJoint, Collider _collider)
            {

                if (_collider.name == "Centre" && _collider.transform.root.tag == "Monster")
                {
                    Monster monster = _collider.GetComponentInParent<Monster>();
                    monster.MoveControl.MonsterNearDoor = true;
                    monster.MoveControl.GetAniControl.GoThroughADoor = true;
                }
            }

            private static void HookDoorJointOnTriggerExit(On.DoorJoint.orig_OnTriggerExit orig, DoorJoint doorJoint, Collider _collider)
            {

                if (_collider.name == "Centre" && _collider.transform.root.tag == "Monster")
                {
                    Monster monster = _collider.GetComponentInParent<Monster>();
                    monster.MoveControl.MonsterNearDoor = false;
                    monster.MoveControl.GetAniControl.GoThroughADoor = false;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DraggedOutHiding

            private static void HookDraggedOutHidingDragPlayer(On.DraggedOutHiding.orig_DragPlayer orig, DraggedOutHiding draggedOutHiding)
            {
                if (lastMonsterSentMessage != null)
                {
                    draggedOutHiding.monster = lastMonsterSentMessage;
                }
                orig.Invoke(draggedOutHiding);
                if (ModSettings.enableCrewVSMonsterMode)
                {
                    int monsterNumber = MonsterNumber(draggedOutHiding.monster.GetInstanceID());
                    if (monsterNumber < ModSettings.numbersOfMonsterPlayers.Count && CrewVsMonsterMode.monsterInHidingEvent[monsterNumber])
                    {
                        HidingSpot playerHidingSpot = draggedOutHiding.monster.PlayerDetectRoom.PlayerHidingSpot(draggedOutHiding.monster.PlayerDetectRoom.GetRoom.HidingSpots);
                        if (playerHidingSpot.Hide == HidingSpot.PlaceType.Prone)
                        {
                            draggedOutHiding.player.SetThrowDir = playerHidingSpot.ThrowDirAtPoint;
                        }
                        else
                        {
                            draggedOutHiding.player.LerpByMonster(draggedOutHiding.player.transform, draggedOutHiding.monster.transform.position, 2f);
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DragPlayer

            private static void HookDragPlayer(On.DragPlayer.orig_Mec_OnReleasePlayer orig, DragPlayer dragPlayer)
            {
                if (lastMonsterSentMessage != null)
                {
                    dragPlayer.monster = lastMonsterSentMessage;
                }

                dragPlayer.finished = true;
                dragPlayer.monster.LastSeenPlayerPosition = References.Player.transform.position;
                dragPlayer.monster.GetMainCollider.enabled = true;

                dragPlayer.player = dragPlayer.monster.player.GetComponent<NewPlayerClass>();

                dragPlayer.player.IsGrabbed = false;
                dragPlayer.player.Motor.enabled = true;
                dragPlayer.player.Motor.useGravity = true;
                dragPlayer.player.SetToProne();
                TriggerObjectives.instance.monsterThrow = true;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DualSteamVent

            private static void HookDualSteamVent(On.DualSteamVent.orig_DoStun orig, DualSteamVent dualSteamVent)
            {
                if (dualSteamVent.SinceLastStun.TimeElapsed > 5f)
                {
                    dualSteamVent.SinceLastStun.ResetTimer();
                    if (dualSteamVent.isLeft)
                    {
                        RandomiseNavBoxes.Randomise(dualSteamVent.LeftNav);
                        dualSteamVent.left.coolTime = 30f;
                    }
                    else
                    {
                        RandomiseNavBoxes.Randomise(dualSteamVent.RightNav);
                        dualSteamVent.right.coolTime = 30f;
                    }
                    monsterListMonsterComponents[ClosestMonsterToThis(((MonoBehaviour)dualSteamVent).transform.position)].StunAndRetreat();
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @EncounterMonster

            private static void HookEncounterMonster(On.EncounterMonster.orig_Update orig, EncounterMonster encounterMonster)
            {
                if (!encounterMonster.completed && monsterList != null)
                {
                    bool firstEscape = false;

                    foreach (Monster monster in monsterListMonsterComponents)
                    {
                        if (monster.firstEscape)
                        {
                            firstEscape = true;
                            break;
                        }
                    }

                    if (firstEscape)
                    {
                        encounterMonster.completed = true;
                        ((MonoBehaviour)encounterMonster).GetComponent<Task>().Fail();
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @EscapeChecker

            private static void HookEscapeChecker(On.EscapeChecker.orig_UpdateAllCompleteness orig, EscapeChecker escapeChecker)
            {
                orig.Invoke(escapeChecker);
                if (monsterList != null)
                {
                    for (int i = 1; i < monsterList.Count; i++)
                    {
                        monsterList[i].GetComponent<MonsterEffectiveness>().HighestEscapeCompleteness = escapeChecker.Highest;
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @EscapeRoute

            // OnInteract has been moved to BaseFeatures.

            /*----------------------------------------------------------------------------------------------------*/
            // @FadeCalculator

            private static float CalcMonsterFade(Monster monster)
            {
                Debug.Log("MonsterFade hooked successfully.");
                return monster.GetAlertMeters.currentHigh * 0.01f;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FiendLightController
            // ~This is for the flashlight. It has this as a component.

            private static void HookFiendLightController()
            {
                On.FiendLightController.OnGenerationComplete += new On.FiendLightController.hook_OnGenerationComplete(HookFiendLightControllerOnGenerationComplete);
                On.FiendLightController.LateUpdate += new On.FiendLightController.hook_LateUpdate(HookFiendLightControllerLateUpdate);
            }

            private static void HookFiendLightControllerOnGenerationComplete(On.FiendLightController.orig_OnGenerationComplete orig, FiendLightController fiendLightController)
            {
                fiendLightController.flashlight = ((MonoBehaviour)fiendLightController).GetComponentInParent<Flashlight>();
                fiendLightController.lights = ((MonoBehaviour)fiendLightController).GetComponentsInChildren<Light>(true);
                /*
                if (fiends != null)
                {
                    fiendLightController.flashlight = ((MonoBehaviour)fiendLightController).GetComponentInParent<Flashlight>();
                    fiendLightController.lights = ((MonoBehaviour)fiendLightController).GetComponentsInChildren<Light>(true);
                }
                */
            }

            private static void HookFiendLightControllerLateUpdate(On.FiendLightController.orig_LateUpdate orig, FiendLightController fiendLightController)
            {
                if (auras != null)
                {
                    bool updateOverride = false;
                    bool anyActive = false;
                    GenericLight.LightTypes newOverride = GenericLight.LightTypes.Normal;
                    for (int i = 0; i < auras.Count; i++)
                    {
                        if (auras[i] != null && auras[i].enabled)
                        {
                            anyActive = true;
                            FiendLightDisruptor fiendLightDisruptor = auras[i].GetComponent<FiendLightDisruptor>();
                            if (fiendLightDisruptor.timeSinceDisrupt == 0f)
                            {
                                updateOverride = true;
                                if (MathHelper.RoundToNearest(auras[i].transform.position, Settings.CuboidDim.y).y == MathHelper.RoundToNearest(((MonoBehaviour)fiendLightController).transform.position, Settings.CuboidDim.y).y)
                                {
                                    float distanceToFlashlight = Vector3.Distance(auras[i].transform.position, ((MonoBehaviour)fiendLightController).transform.position);
                                    if (distanceToFlashlight < auras[i].largeRadius * Settings.CuboidDim.x)
                                    {
                                        if (distanceToFlashlight < auras[i].smallRadius * Settings.CuboidDim.x)
                                        {
                                            newOverride = GenericLight.LightTypes.Off;
                                            break;
                                        }
                                        else
                                        {
                                            newOverride = GenericLight.LightTypes.Flickering2;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (updateOverride || !anyActive)
                    {
                        fiendLightController.fiendOverride = newOverride;
                    }
                }
                if (fiendLightController.fiendOverride != GenericLight.LightTypes.Normal && fiendLightController.flashlight.on)
                {
                    if (fiendLightController.fiendOverride != GenericLight.LightTypes.Off)
                    {
                        if (fiendLightController.fiendOverride == GenericLight.LightTypes.Flickering2)
                        {
                            fiendLightController.Flicker();
                        }
                    }
                    else
                    {
                        foreach (Light light in fiendLightController.lights)
                        {
                            light.intensity = 0f;
                        }
                    }
                }

                /*
                if (fiends != null)
                {
                    for (int i = 0; i < fiends.Count; i++)
                    {
                        if (FiendLightDisruptor.isFiendLightUpdateFrame)
                        {
                            if (auras[i] != null)
                            {
                                if (MathHelper.RoundToNearest(fiends[i].transform.position, Settings.CuboidDim.y).y == MathHelper.RoundToNearest(((MonoBehaviour)fiendLightController).transform.position, Settings.CuboidDim.y).y)
                                {
                                    float distance = Vector3.Distance(fiends[i].transform.position, ((MonoBehaviour)fiendLightController).transform.position);
                                    if (distance < auras[i].largeRadius * Settings.CuboidDim.x)
                                    {
                                        if (distance < auras[i].smallRadius * Settings.CuboidDim.x)
                                        {
                                            fiendLightController.fiendOverride = GenericLight.LightTypes.Off;
                                        }
                                        else
                                        {
                                            fiendLightController.fiendOverride = GenericLight.LightTypes.Flickering2;
                                        }
                                    }
                                    else
                                    {
                                        fiendLightController.fiendOverride = GenericLight.LightTypes.Normal;
                                    }
                                }
                                else
                                {
                                    fiendLightController.fiendOverride = GenericLight.LightTypes.Normal;
                                }
                            }
                            else
                            {
                                auras[i] = fiends[i].GetComponentsInChildren<FiendAura>(true)[0];
                            }
                        }
                        if (fiendLightController.fiendOverride != GenericLight.LightTypes.Normal && fiendLightController.flashlight.on)
                        {
                            GenericLight.LightTypes lightTypes = fiendLightController.fiendOverride;
                            if (lightTypes != GenericLight.LightTypes.Off)
                            {
                                if (lightTypes == GenericLight.LightTypes.Flickering2)
                                {
                                    fiendLightController.Flicker();
                                }
                            }
                            else
                            {
                                foreach (Light light in fiendLightController.lights)
                                {
                                    light.intensity = 0f;
                                }
                            }
                        }
                    }
                }
                */
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FiendLightDisruptor

            private static void HookFiendLightDisruptor()
            {
                On.FiendLightDisruptor.Update += new On.FiendLightDisruptor.hook_Update(HookFiendLightDisruptorUpdate);
            }

            private static void DisruptLights(FiendLightDisruptor fiendLightDisruptor, Monster monster)
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    fiendLightDisruptor.smallNodes.Clear();
                    fiendLightDisruptor.smallRooms.Clear();
                    fiendLightDisruptor.fiendAura.FindNodes(fiendLightDisruptor.fiendAura.smallRadius, fiendLightDisruptor.smallNodes);
                    fiendLightDisruptor.fiendAura.FindRooms(fiendLightDisruptor.fiendAura.smallRadius, fiendLightDisruptor.smallRooms);
                    SetLights(fiendLightDisruptor.smallNodes, fiendLightDisruptor.smallRooms, GenericLight.LightTypes.Off, fiendLightDisruptor, monster, false);
                    fiendLightDisruptor.smallNodes.Clear();
                    fiendLightDisruptor.smallRooms.Clear();
                    fiendLightDisruptor.fiendAura.FindNodes(fiendLightDisruptor.fiendAura.mediumRadius, fiendLightDisruptor.smallNodes);
                    fiendLightDisruptor.fiendAura.FindRooms(fiendLightDisruptor.fiendAura.mediumRadius, fiendLightDisruptor.smallRooms);
                    SetLights(fiendLightDisruptor.smallNodes, fiendLightDisruptor.smallRooms, GenericLight.LightTypes.FiendFlickerInner, fiendLightDisruptor, monster, false);
                    fiendLightDisruptor.SwitchLists();
                    fiendLightDisruptor.largeNodes.Clear();
                    fiendLightDisruptor.largeRooms.Clear();
                    fiendLightDisruptor.fiendAura.FindNodes(fiendLightDisruptor.fiendAura.largeRadius, fiendLightDisruptor.largeNodes);
                    fiendLightDisruptor.fiendAura.FindRooms(fiendLightDisruptor.fiendAura.largeRadius, fiendLightDisruptor.largeRooms);
                    SetLights(fiendLightDisruptor.largeNodes, fiendLightDisruptor.largeRooms, GenericLight.LightTypes.FiendFlickerOuter, fiendLightDisruptor, monster, false);
                    SetLights(fiendLightDisruptor.oldLargeNodes, fiendLightDisruptor.oldLargeRooms, GenericLight.LightTypes.Normal, fiendLightDisruptor, monster, true);
                }
            }

            private static void SetLights(List<NodeData> _nodes, List<Room> _rooms, GenericLight.LightTypes _lightOverride, FiendLightDisruptor fiendLightDisruptor, Monster monster, bool _overrideY = false)
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    if (_nodes == null)
                    {
                        return;
                    }
                    if (fiendLightDisruptor.largeRooms == null)
                    {
                        return;
                    }
                    for (int i = 0; i < _nodes.Count; i++)
                    {
                        if (_nodes[i] != null && _nodes[i].nodeLights.Count > 0 && (_overrideY || MathHelper.RoundToNearest(fiendLightDisruptor.RegionNodeToWorld(_nodes[i].regionNode), Settings.CuboidDim.y).y == fiendLightDisruptor.monsterY))
                        {
                            for (int j = 0; j < _nodes[i].nodeLights.Count; j++)
                            {
                                fiendLightDisruptor.SetLight(_nodes[i].nodeLights[j], _lightOverride);
                                fiendLightDisruptor.UpdateLight(_nodes[i].nodeLights[j]);
                            }
                        }
                    }
                    for (int k = 0; k < _rooms.Count; k++)
                    {
                        if (_rooms[k] != null && (_rooms[k] == monster.RoomDetect.CurrentRoom || _lightOverride != GenericLight.LightTypes.Off) && (_overrideY || MathHelper.RoundToNearest(_rooms[k].transform.position, Settings.CuboidDim.y).y == fiendLightDisruptor.monsterY || _rooms[k] == monster.RoomDetect.CurrentRoom))
                        {
                            bool flag = false;
                            if (_rooms[k].roomTags.Count > 0)
                            {
                                for (int l = 0; l < _rooms[k].roomTags.Count; l++)
                                {
                                    if (_rooms[k].roomTags[l] == "VictoryArea")
                                    {
                                        flag = true;
                                    }
                                }
                            }
                            if (flag)
                            {
                                float num = 99f;
                                if (_lightOverride != GenericLight.LightTypes.Off)
                                {
                                    if (_lightOverride != GenericLight.LightTypes.FiendFlickerInner)
                                    {
                                        if (_lightOverride == GenericLight.LightTypes.FiendFlickerOuter)
                                        {
                                            num = fiendLightDisruptor.fiendAura.largeRadius;
                                        }
                                    }
                                    else
                                    {
                                        num = fiendLightDisruptor.fiendAura.mediumRadius;
                                    }
                                }
                                else
                                {
                                    num = fiendLightDisruptor.fiendAura.smallRadius;
                                }
                                for (int m = 0; m < _rooms[k].RoomLights.Length; m++)
                                {
                                    Vector3 position = _rooms[k].RoomLights[m].transform.position;
                                    position.y = ((MonoBehaviour)fiendLightDisruptor).transform.position.y;
                                    if (Vector3.Distance(position, ((MonoBehaviour)fiendLightDisruptor).transform.position) <= num)
                                    {
                                        fiendLightDisruptor.SetLight(_rooms[k].RoomLights[m], _lightOverride);
                                        fiendLightDisruptor.UpdateLight(_rooms[k].RoomLights[m]);
                                    }
                                }
                            }
                            else
                            {
                                for (int n = 0; n < _rooms[k].RoomLights.Length; n++)
                                {
                                    fiendLightDisruptor.SetLight(_rooms[k].RoomLights[n], _lightOverride);
                                    fiendLightDisruptor.UpdateLight(_rooms[k].RoomLights[n]);
                                }
                            }
                        }
                    }
                }
            }

            private static void HookFiendLightDisruptorUpdate(On.FiendLightDisruptor.orig_Update orig, FiendLightDisruptor fiendLightDisruptor)
            {
                // New implementation keeping in mind that each Fiend has a LightDisruptor
                Monster fiend = ((MonoBehaviour)fiendLightDisruptor).GetComponentInParent<Monster>();
                fiendLightDisruptor.monsterY = MathHelper.RoundToNearest(fiend.transform.position, Settings.CuboidDim).y;
                fiendLightDisruptor.timeSinceDisrupt += Time.deltaTime;
                if (fiendLightDisruptor.timeSinceDisrupt > fiendLightDisruptor.maxSinceDisrupt)
                {
                    DisruptLights(fiendLightDisruptor, fiend);
                    fiendLightDisruptor.timeSinceDisrupt = 0f;
                    FiendLightDisruptor.isFiendLightUpdateFrame = true;
                }
                else
                {
                    FiendLightDisruptor.isFiendLightUpdateFrame = false;
                }

                /*
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    bool disruptLights = false;
                    fiendLightDisruptor.timeSinceDisrupt += Time.deltaTime;
                    if (ModSettings.useSparky)
                    {
                        for (int i = 0; i < SparkyMode.sparkyListMonsterComponents.Count; i++)
                        {
                            fiendLightDisruptor.monsterY = MathHelper.RoundToNearest(SparkyMode.sparkyList[i].transform.position, Settings.CuboidDim).y;
                            if (fiendLightDisruptor.timeSinceDisrupt > fiendLightDisruptor.maxSinceDisrupt)
                            {
                                DisruptLights(fiendLightDisruptor, SparkyMode.sparkyListMonsterComponents[i]);
                                FiendLightDisruptor.isFiendLightUpdateFrame = true;
                            }
                            else
                            {
                                FiendLightDisruptor.isFiendLightUpdateFrame = false;
                            }
                        }
                    }
                    for (int i = 0; i < fiends.Count; i++)
                    {
                        fiendLightDisruptor.monsterY = MathHelper.RoundToNearest(fiends[i].transform.position, Settings.CuboidDim).y;
                        if (fiendLightDisruptor.timeSinceDisrupt > fiendLightDisruptor.maxSinceDisrupt || disruptLights)
                        {
                            DisruptLights(fiendLightDisruptor, fiendsMonsterComponents[i]);
                            fiendLightDisruptor.timeSinceDisrupt = 0f;
                            FiendLightDisruptor.isFiendLightUpdateFrame = true;
                            if (i == 0)
                            {
                                disruptLights = true;
                            }
                            else if (i == fiends.Count - 1)
                            {
                                disruptLights = false;
                            }
                        }
                        else
                        {
                            FiendLightDisruptor.isFiendLightUpdateFrame = false;
                        }
                    }
                }
                */
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FiendMindAttack

            private static List<bool> fiendsThatAreInRangeOfPlayer;
            private static bool isAFiendInRangeOfPlayer;
            private static List<bool> fiendsThatSeePlayer;
            private static bool doesAFiendSeeThePlayer;
            public static List<float> fiendMindAttackPlayerAttackTimers;
            public static List<float> fiendMindAttackPlayerDelayTimers;
            public static List<string> fiendMindAttackPlayerCurrentClip;
            public static List<List<int>> fiendMindAttackFiendsTargetingPlayer;
            public static List<FiendAura> auras;

            private static void HookFiendMindAttack()
            {
                On.FiendMindAttack.AttackCheck += new On.FiendMindAttack.hook_AttackCheck(HookFiendMindAttackAttackCheck);
                On.FiendMindAttack.ChangeTimers += new On.FiendMindAttack.hook_ChangeTimers(HookFiendMindAttackChangeTimers);
                On.FiendMindAttack.Start += new On.FiendMindAttack.hook_Start(HookFiendMindAttackStart);
                //On.FiendMindAttack.PlaySound += new On.FiendMindAttack.hook_PlaySound(HookFiendMindAttackPlaySound);
            }

            private static bool HookFiendMindAttackPlaySound(On.FiendMindAttack.orig_PlaySound orig, FiendMindAttack fiendMindAttack, string _clip, AudioSource _source)
            {
                if (fiendMindAttack.currentSound != _clip)
                {
                    if (_source != null)
                    {
                        _source.Stop();
                        if (ModSettings.enableMultiplayer && !MultiplayerMode.useLegacyAudio)
                        {
                            VirtualAudioSource virtualAudioSource = _source.gameObject.GetComponent<VirtualAudioSource>();
                            if (virtualAudioSource != null)
                            {
                                virtualAudioSource.Stop();
                                virtualAudioSource.time = 0f;
                            }
                            else if (ModSettings.logDebugText)
                            {
                                Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                            }
                        }
                        _source.time = 0f;

                        /*
                        int playerNumber;
                        if (!ModSettings.enableMultiplayer)
                        {
                            playerNumber = 0;
                        }
                        else
                        {
                            playerNumber = MultiplayerMode.PlayerNumber(fiendMindAttack.monster.PlayerDetectRoom.player.GetInstanceID());
                        }
                        if (_clip == fiendMindAttack.chargeSound)
                        {
                            for (int i = 0; i < fiendMindAttackFiendsTargetingPlayer[playerNumber].Count; i++)
                            {
                                int monsterNumberToCheck = fiendMindAttackFiendsTargetingPlayer[playerNumber][i];
                                AudioSource audioSource = monsterListMonsterComponents[i].GetComponent<FiendMindAttack>().chargeSource;
                                audioSource.Stop();
                                audioSource.time = 0f;

                                if (ModSettings.enableMultiplayer && !MultiplayerMode.useLegacyAudio)
                                {
                                    VirtualAudioSource virtualAudioSource = _source.gameObject.GetComponent<VirtualAudioSource>();
                                    if (virtualAudioSource != null)
                                    {
                                        virtualAudioSource.Stop();
                                        virtualAudioSource.time = 0f;
                                    }
                                    else if (ModSettings.logDebugText)
                                    {
                                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < fiendMindAttackFiendsTargetingPlayer[playerNumber].Count; i++)
                            {
                                int monsterNumberToCheck = fiendMindAttackFiendsTargetingPlayer[playerNumber][i];
                                AudioSource audioSource = monsterListMonsterComponents[i].GetComponent<FiendMindAttack>().coolSource;
                                audioSource.Stop();
                                audioSource.time = 0f;

                                if (ModSettings.enableMultiplayer && !MultiplayerMode.useLegacyAudio)
                                {
                                    VirtualAudioSource virtualAudioSource = _source.gameObject.GetComponent<VirtualAudioSource>();
                                    if (virtualAudioSource != null)
                                    {
                                        virtualAudioSource.Stop();
                                        virtualAudioSource.time = 0f;
                                    }
                                    else if (ModSettings.logDebugText)
                                    {
                                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                                    }
                                }
                            }
                        }
                        */
                    }
                    AudioSystem.PlaySound(_clip, _source);
                    fiendMindAttack.currentSound = _clip;
                    return true;
                }
                return false;
            }

            private static void HookFiendMindAttackStart(On.FiendMindAttack.orig_Start orig, FiendMindAttack fiendMindAttack)
            {
                fiendMindAttack.monster = ((MonoBehaviour)fiendMindAttack).GetComponentInParent<Monster>();
                fiendMindAttack.stateMachine = fiendMindAttack.monster.GetComponent<FSM>();
                fiendMindAttack.fiendAnims = fiendMindAttack.monster.GetComponentInChildren<FiendAnimations>();
                fiendMindAttack.playerHealth = fiendMindAttack.monster.player.GetComponentInChildren<PlayerHealth>();
                fiendMindAttack.mindAttackBleed = fiendMindAttack.monster.player.GetComponentInChildren<MindAttackEffect>();
                fiendMindAttack.oculusMindAttackBleed = fiendMindAttack.monster.player.GetComponentInChildren<OculusMindAttackEffect>();
                int monsterNumber = MonsterNumber(fiendMindAttack.monster.GetInstanceID());
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Fiend mind attack monster number is " + monsterNumber);
                }

                /*
                if (monsterNumber > MonsterNumber(fiendsMonsterComponents[0].GetInstanceID()))
                {
                    FiendMindAttack fiendMindAttackOfFirstMonster = monsterListMonsterComponents[0].GetComponent<FiendMindAttack>();
                    fiendMindAttack.chargeSource = fiendMindAttackOfFirstMonster.chargeSource;
                    fiendMindAttack.coolSource = fiendMindAttackOfFirstMonster.coolSource;
                }
                */
            }

            private static void HookFiendMindAttackChangeTimers(On.FiendMindAttack.orig_ChangeTimers orig, FiendMindAttack fiendMindAttack)
            {
                if (MonsterStarter.spawned)
                {
                    int monsterNumber = MonsterNumber(fiendMindAttack.monster.GetInstanceID());
                    int playerNumber;
                    if (!ModSettings.enableMultiplayer)
                    {
                        playerNumber = 0;
                    }
                    else
                    {
                        playerNumber = MultiplayerMode.PlayerNumber(fiendMindAttack.monster.PlayerDetectRoom.player.GetInstanceID());
                    }
                    try
                    {
                        fiendMindAttack.attackTimer = fiendMindAttackPlayerAttackTimers[playerNumber];
                        fiendMindAttack.delayTimer = fiendMindAttackPlayerDelayTimers[playerNumber];
                        //fiendMindAttack.currentSound = fiendMindAttackPlayerCurrentClip[playerNumber];
                    }
                    catch
                    {

                        Debug.Log("Error in FiendMindAttackChangeTimers Change timer start");
                    }
                    if (LevelGeneration.Instance.finishedGenerating/* && IsFiendAllowedToChangeTimer(playerNumber, monsterNumber)*/)
                    {
                        bool flag = false;
                        try
                        {
                            if (fiendMindAttack.stateMachine.Current.ToString().Contains("MChasingState") && fiendMindAttack.monster.CanSeePlayer)
                            {
                                flag = true;
                            }
                            if (ModSettings.logDebugText)
                            {
                                if (fiendMindAttack.stateMachine.Current.ToString().Contains("MChasingState"))
                                {
                                    Debug.Log("Fiend number " + monsterNumber + " is in chasing state");
                                }
                                if (fiendMindAttack.monster.CanSeePlayer)
                                {
                                    Debug.Log("Fiend number " + monsterNumber + " can see the player");
                                }
                            }
                        }
                        catch
                        {
                            Debug.Log("Error in FiendMindAttackChangeTimers 1");
                        }
                        try
                        {
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("Fiend number " + monsterNumber + " has delay timer " + fiendMindAttack.delayTimer + " and max delay " + fiendMindAttack.maxDelay);
                            }
                            if (fiendMindAttack.delayTimer == fiendMindAttack.maxDelay)
                            {
                                if (ModSettings.logDebugText)
                                {
                                    Debug.Log("Fiend number " + monsterNumber + " has gotten into the first proper block");
                                }
                                try
                                {
                                    if (flag && fiendMindAttack.attackTimer < fiendMindAttack.maxTime)
                                    {
                                        if (ModSettings.logDebugText)
                                        {
                                            Debug.Log("Fiend number " + monsterNumber + " passed the test after the first proper block");
                                        }
                                        fiendMindAttack.attackTimer += Time.deltaTime * ModSettings.fiendMindAttackAttackTimerChargeRate;
                                        try
                                        {
                                            if (fiendMindAttack.PlaySound(fiendMindAttack.chargeSound, fiendMindAttack.chargeSource))
                                            {
                                                if (ModSettings.logDebugText)
                                                {
                                                    Debug.Log("Fiend number " + monsterNumber + " is playing charge sound");
                                                }
                                                fiendMindAttack.chargeSource.time = fiendMindAttack.attackTimer;
                                                try
                                                {
                                                    if (fiendMindAttack.chargeVolume == null)
                                                    {
                                                        if (fiendMindAttack.chargeSource != null)
                                                        {
                                                            fiendMindAttack.chargeVolume = fiendMindAttack.chargeSource.GetComponent<VolumeController>();
                                                        }
                                                        else
                                                        {
                                                            Debug.Log("FiendMindAttackChangeTimers 2h chargeSource is null");
                                                        }
                                                    }
                                                    if (fiendMindAttack.chargeVolume != null)
                                                    {
                                                        fiendMindAttack.chargeVolume.fadeValue = 0f;
                                                    }
                                                    else
                                                    {
                                                        Debug.Log("FiendMindAttackChangeTimers 2h chargeVolume is still null");
                                                    }
                                                }
                                                catch
                                                {
                                                    Debug.Log("Error in FiendMindAttackChangeTimers 2h");
                                                }
                                            }
                                        }
                                        catch
                                        {
                                            Debug.Log("Error in FiendMindAttackChangeTimers 2f");
                                        }
                                        try
                                        {
                                            fiendMindAttack.fadeIncDec = 1f;
                                        }
                                        catch
                                        {
                                            Debug.Log("Error in FiendMindAttackChangeTimers 2g");
                                        }
                                    }
                                    else
                                    {
                                        if (ModSettings.logDebugText)
                                        {
                                            Debug.Log("Fiend number " + monsterNumber + " did not pass the test after the first proper block");
                                        }
                                        try
                                        {
                                            if (IsFiendAllowedToReduceTimer(playerNumber, monsterNumber))
                                            {
                                                fiendMindAttack.attackTimer -= Time.deltaTime * ModSettings.fiendMindAttackAttackTimerDecayRate;
                                            }
                                        }
                                        catch
                                        {
                                            Debug.Log("Error in FiendMindAttackChangeTimers 2b");
                                        }
                                        try
                                        {
                                            if (fiendMindAttack.currentSound != string.Empty)
                                            {
                                                float time = 0f;
                                                try
                                                {
                                                    if (fiendMindAttack.coolSource != null)
                                                    {
                                                        float clipLength = 0f;
                                                        float clipTime = 0f;
                                                        try
                                                        {
                                                            if (fiendMindAttack.chargeSource != null)
                                                            {
                                                                if (fiendMindAttack.chargeSource.clip == null)
                                                                {
                                                                    Debug.Log("Fiend 2d1 clip is null");
                                                                    FiendMindAttack[] allFiendMindAttacks = FindObjectsOfType<FiendMindAttack>();
                                                                    foreach (FiendMindAttack fiendMindAttackToCheck in allFiendMindAttacks)
                                                                    {
                                                                        if (fiendMindAttackToCheck.chargeSource != null && fiendMindAttackToCheck.chargeSource.clip != null)
                                                                        {
                                                                            fiendMindAttack.chargeSource.clip = fiendMindAttackToCheck.chargeSource.clip;
                                                                            break;
                                                                        }
                                                                    }
                                                                }
                                                                if (fiendMindAttack.chargeSource.clip != null)
                                                                {
                                                                    clipLength = fiendMindAttack.chargeSource.clip.length; // This seems to cause the error.
                                                                }
                                                                else
                                                                {
                                                                    Debug.Log("Fiend 2d1 clip is still null");
                                                                }
                                                            }
                                                            else
                                                            {
                                                                Debug.Log("Fiend 2d1 chargeSource is null");
                                                            }
                                                        }
                                                        catch
                                                        {
                                                            Debug.Log("Error in FiendMindAttackChangeTimers 2d1");
                                                        }
                                                        try
                                                        {
                                                            clipTime = fiendMindAttack.chargeSource.time;
                                                        }
                                                        catch
                                                        {
                                                            Debug.Log("Error in FiendMindAttackChangeTimers 2d2");
                                                        }
                                                        time = clipLength - clipTime;
                                                    }
                                                }
                                                catch
                                                {
                                                    Debug.Log("Error in FiendMindAttackChangeTimers 2d");
                                                }
                                                try
                                                {
                                                    if (fiendMindAttack.PlaySound(fiendMindAttack.coolSound, fiendMindAttack.coolSource))
                                                    {
                                                        if (ModSettings.logDebugText)
                                                        {
                                                            Debug.Log("Fiend number " + monsterNumber + " is playing cool sound");
                                                        }
                                                        fiendMindAttack.coolSource.time = time;
                                                        if (fiendMindAttack.coolVolume == null)
                                                        {
                                                            fiendMindAttack.coolVolume = fiendMindAttack.coolSource.GetComponent<VolumeController>();
                                                        }
                                                        if (fiendMindAttack.coolVolume != null)
                                                        {
                                                            fiendMindAttack.coolVolume.fadeValue = 0f;
                                                        }
                                                    }
                                                }
                                                catch
                                                {
                                                    Debug.Log("Error in FiendMindAttackChangeTimers 2e");
                                                }
                                                fiendMindAttack.fadeIncDec = -1f;
                                            }
                                        }
                                        catch
                                        {
                                            Debug.Log("Error in FiendMindAttackChangeTimers 2c");
                                        }
                                    }
                                }
                                catch
                                {
                                    Debug.Log("Error in FiendMindAttackChangeTimers 2a");
                                }
                            }
                            else if (IsFiendMinimumInGroup(playerNumber, monsterNumber))
                            {
                                fiendMindAttack.attackTimer -= Time.deltaTime * 3f * ModSettings.fiendMindAttackAttackTimerDecayRate;
                            }
                        }
                        catch
                        {
                            Debug.Log("Error in FiendMindAttackChangeTimers 2");
                        }
                        try
                        {
                            if (fiendMindAttack.chargeVolume != null)
                            {
                                fiendMindAttack.chargeVolume.fadeValue = Mathf.Clamp01(fiendMindAttack.chargeVolume.fadeValue + fiendMindAttack.fadeIncDec * Time.deltaTime * 3f);
                            }
                        }
                        catch
                        {
                            Debug.Log("Error in FiendMindAttackChangeTimers 3");
                        }
                        try
                        {
                            if (fiendMindAttack.coolVolume != null)
                            {
                                fiendMindAttack.coolVolume.fadeValue = Mathf.Clamp01(fiendMindAttack.coolVolume.fadeValue - fiendMindAttack.fadeIncDec * Time.deltaTime * 3f);
                            }
                        }
                        catch
                        {
                            Debug.Log("Error in FiendMindAttackChangeTimers 4");
                        }
                        try
                        {
                            fiendMindAttack.attackTimer = Mathf.Clamp(fiendMindAttack.attackTimer, 0f, fiendMindAttack.maxTime);
                        }
                        catch
                        {
                            Debug.Log("Error in FiendMindAttackChangeTimers 5");
                        }
                        if (IsFiendMinimumInGroup(playerNumber, monsterNumber))
                        {
                            try
                            {
                                if (fiendMindAttack.attackTimer == 0f/* && !IsOtherFiendChangingTimer(fiendMindAttack)*/)
                                {
                                    if (ModSettings.logDebugText)
                                    {
                                        Debug.Log("Fiend number " + monsterNumber + " is disabling its mind attack bleed");
                                    }
                                    if (!OculusManager.isOculusEnabled)
                                    {
                                        fiendMindAttack.mindAttackBleed.enabled = false;
                                    }
                                    else
                                    {
                                        fiendMindAttack.oculusMindAttackBleed.enabled = false;
                                    }
                                }
                                else if (!OculusManager.isOculusEnabled)
                                {
                                    if (ModSettings.logDebugText)
                                    {
                                        Debug.Log("Fiend number " + monsterNumber + " is enabling its mind attack bleed");
                                    }
                                    fiendMindAttack.mindAttackBleed.enabled = true;
                                    /*
                                    if (!IsOtherFiendChangingTimer(fiendMindAttack))
                                    {
                                        */
                                    fiendMindAttack.mindAttackBleed.impact -= Time.deltaTime;
                                    fiendMindAttack.mindAttackBleed.impact = Mathf.Clamp01(fiendMindAttack.mindAttackBleed.impact);
                                    fiendMindAttack.mindAttackBleed.strength = fiendMindAttack.attackTimer / fiendMindAttack.maxTime;
                                    //}
                                }
                                else
                                {
                                    fiendMindAttack.oculusMindAttackBleed.enabled = true;
                                    fiendMindAttack.oculusMindAttackBleed.impact -= Time.deltaTime;
                                    fiendMindAttack.oculusMindAttackBleed.impact = Mathf.Clamp01(fiendMindAttack.oculusMindAttackBleed.impact);
                                    fiendMindAttack.oculusMindAttackBleed.strength = fiendMindAttack.attackTimer / fiendMindAttack.maxTime;
                                }
                            }
                            catch
                            {
                                Debug.Log("Error in FiendMindAttackChangeTimers 6");
                            }
                            try
                            {
                                if (!OculusManager.isOculusEnabled)
                                {
                                    if (fiendMindAttack.mindAttackBleed.strength > 0.1f)
                                    {
                                        fiendMindAttack.playerHealth.DoDamage(0f, false, PlayerHealth.DamageTypes.MindAttack, false);
                                    }
                                }
                                else if (fiendMindAttack.oculusMindAttackBleed.strength > 0.1f)
                                {
                                    fiendMindAttack.playerHealth.DoDamage(0f, false, PlayerHealth.DamageTypes.MindAttack, false);
                                }
                            }
                            catch
                            {
                                Debug.Log("Error in FiendMindAttackChangeTimers 7");
                            }
                            try
                            {
                                fiendMindAttack.delayTimer += Time.deltaTime * ModSettings.fiendMindAttackDelayTimerRate;
                                fiendMindAttack.delayTimer = Mathf.Clamp(fiendMindAttack.delayTimer, 0f, fiendMindAttack.maxDelay);
                            }
                            catch
                            {
                                Debug.Log("Error in FiendMindAttackChangeTimers 8");
                            }
                        }
                        try
                        {
                            fiendMindAttackPlayerAttackTimers[playerNumber] = fiendMindAttack.attackTimer;
                            fiendMindAttackPlayerDelayTimers[playerNumber] = fiendMindAttack.delayTimer;
                            //fiendMindAttackPlayerCurrentClip[playerNumber] = fiendMindAttack.currentSound;
                        }
                        catch
                        {

                            Debug.Log("Error in FiendMindAttackChangeTimers Change timer end");
                        }
                    }
                }
            }

            private static bool IsOtherFiendChangingTimer(FiendMindAttack originalFiendMindAttack)
            {
                bool isOtherFiendChangingTimer = false;
                foreach (GameObject fiendGameObject in ManyMonstersMode.fiends)
                {
                    FiendMindAttack otherFiendMindAttack = fiendGameObject.GetComponentInChildren<FiendMindAttack>();
                    if (otherFiendMindAttack != originalFiendMindAttack && otherFiendMindAttack.playerHealth.NPC == originalFiendMindAttack.playerHealth.NPC && otherFiendMindAttack.attackTimer > 0f)
                    {
                        isOtherFiendChangingTimer = true;
                        break;
                    }
                }
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Is other Fiend changing mind attack timer of monster number " + ManyMonstersMode.MonsterNumber(originalFiendMindAttack.monster.GetInstanceID()) + "? " + isOtherFiendChangingTimer);
                }
                return isOtherFiendChangingTimer;
            }

            private static bool IsFiendAllowedToChangeTimer(int playerNumber, int monsterNumber)
            {
                int minimumMonsterNumber = 0;
                int numberToStartFrom = 0;
                bool canAFiendSeeThePlayer = false;
                for (int i = 0; i < fiendMindAttackFiendsTargetingPlayer[playerNumber].Count; i++)
                {
                    int monsterNumberToCheck = fiendMindAttackFiendsTargetingPlayer[playerNumber][i];
                    if (monsterListMonsterComponents[i].CanSeePlayer)
                    {
                        minimumMonsterNumber = monsterNumberToCheck;
                        numberToStartFrom = i + 1;
                        canAFiendSeeThePlayer = true;
                        break;
                    }
                }
                if (!canAFiendSeeThePlayer)
                {
                    minimumMonsterNumber = fiendMindAttackFiendsTargetingPlayer[playerNumber][0];
                }
                else
                {
                    for (int i = numberToStartFrom; i < fiendMindAttackFiendsTargetingPlayer[playerNumber].Count; i++)
                    {
                        int monsterNumberToCheck = fiendMindAttackFiendsTargetingPlayer[playerNumber][i];
                        if (monsterNumberToCheck < minimumMonsterNumber && monsterListMonsterComponents[monsterNumberToCheck].CanSeePlayer)
                        {
                            minimumMonsterNumber = monsterNumberToCheck;
                        }
                    }
                }

                if (monsterNumber == minimumMonsterNumber)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private static bool IsFiendMinimumInGroup(int playerNumber, int monsterNumber)
            {
                int minimumMonsterNumber = fiendMindAttackFiendsTargetingPlayer[playerNumber][0];
                for (int i = 1; i < fiendMindAttackFiendsTargetingPlayer[playerNumber].Count; i++)
                {
                    int monsterNumberToCheck = fiendMindAttackFiendsTargetingPlayer[playerNumber][i];
                    if (monsterNumberToCheck < minimumMonsterNumber && monsterListMonsterComponents[monsterNumberToCheck].CanSeePlayer)
                    {
                        minimumMonsterNumber = monsterNumberToCheck;
                    }
                }

                if (monsterNumber == minimumMonsterNumber)
                {
                    if (ModSettings.logDebugText)
                    {
                        Debug.Log("Fiend with monster number " + monsterNumber + " is minimum in group");
                    }
                    return true;
                }
                else
                {
                    if (ModSettings.logDebugText)
                    {
                        Debug.Log("Fiend with monster number " + monsterNumber + " is not minimum in group");
                    }
                    return false;
                }
            }

            private static bool IsFiendAllowedToReduceTimer(int playerNumber, int monsterNumber)
            {
                bool canAFiendInGroupSeePlayer = false;

                for (int i = 0; i < fiendMindAttackFiendsTargetingPlayer[playerNumber].Count; i++)
                {
                    int monsterNumberToCheck = fiendMindAttackFiendsTargetingPlayer[playerNumber][i];
                    if (monsterListMonsterComponents[monsterNumberToCheck].CanSeePlayer)
                    {
                        canAFiendInGroupSeePlayer = true;
                        break;
                    }
                }

                return (IsFiendMinimumInGroup(playerNumber, monsterNumber) && !canAFiendInGroupSeePlayer);
            }

            // Legacy code used in old versions when I thought there was only one FiendMindAttack. // Adapted to new system.
            private static void HookFiendMindAttackAttackCheck(On.FiendMindAttack.orig_AttackCheck orig, FiendMindAttack fiendMindAttack)
            {
                int monsterNumber = MonsterNumber(fiendMindAttack.monster.GetInstanceID());
                int playerNumber;
                if (!ModSettings.enableMultiplayer)
                {
                    playerNumber = 0;
                }
                else
                {
                    playerNumber = MultiplayerMode.PlayerNumber(fiendMindAttack.monster.PlayerDetectRoom.player.GetInstanceID());
                }
                if (LevelGeneration.Instance.finishedGenerating && IsFiendAllowedToChangeTimer(playerNumber, monsterNumber))
                {
                    fiendMindAttack.attackTimer = fiendMindAttackPlayerAttackTimers[playerNumber];
                    fiendMindAttack.delayTimer = fiendMindAttackPlayerDelayTimers[playerNumber];
                    for (int i = 0; i < fiends.Count; i++)
                    {
                        if (PlayerToMonsterDistance(fiendsMonsterComponents[i]) < fiendMindAttack.attackRange)
                        {
                            fiendsThatAreInRangeOfPlayer[i] = true;
                            if (!isAFiendInRangeOfPlayer)
                            {
                                isAFiendInRangeOfPlayer = true;
                            }
                        }
                        else
                        {
                            fiendsThatAreInRangeOfPlayer[i] = false;
                        }

                        if (fiendsMonsterComponents[i].CanSeePlayer)
                        {
                            fiendsThatSeePlayer[i] = true;
                            if (!doesAFiendSeeThePlayer)
                            {
                                doesAFiendSeeThePlayer = true;
                            }
                        }
                        else
                        {
                            fiendsThatSeePlayer[i] = false;
                        }
                    }

                    if (isAFiendInRangeOfPlayer && doesAFiendSeeThePlayer && fiendMindAttack.attackTimer == fiendMindAttack.maxTime && fiendMindAttack.delayTimer == fiendMindAttack.maxDelay)
                    {
                        for (int i = 0; i < fiends.Count; i++)
                        {
                            if (fiendsThatAreInRangeOfPlayer[i] && fiendsThatSeePlayer[i])
                            {
                                fiendsMonsterComponents[i].GetComponentInChildren<FiendAnimations>().DoMindAttack = true;
                                fiendsMonsterComponents[i].MoveControl.GetAniControl.DesiredUpperBodyWeight = 1f;
                            }
                        }
                        fiendMindAttack.playerHealth.DoDamage(35f * ModSettings.fiendMindAttackDamageMultiplier, false, PlayerHealth.DamageTypes.MindAttack, false);
                        if (!OculusManager.isOculusEnabled)
                        {
                            fiendMindAttack.mindAttackBleed.impact = 2f;
                        }
                        else
                        {
                            fiendMindAttack.oculusMindAttackBleed.impact = 2f;
                        }
                        fiendMindAttack.PlayAttackSound();
                        fiendMindAttack.PlayerEffects();
                        fiendMindAttack.delayTimer = 0f;
                        fiendMindAttackPlayerDelayTimers[playerNumber] = fiendMindAttack.delayTimer;
                    }
                }
            }

            /*
            private static void HookFiendMindAttackChangeTimers(On.FiendMindAttack.orig_ChangeTimers orig, FiendMindAttack fiendMindAttack)
            {
                if (fiendMindAttack.delayTimer == fiendMindAttack.maxDelay)
                {
                    int fiendsRelevant = 0;
                    for (int i = 0; i < fiends.Count; i++)
                    {
                        if (fiends[i].GetComponent<FSM>().Current.ToString().Contains("MChasingState") && fiendsMonsterComponents[i].CanSeePlayer)
                        {
                            fiendsRelevant++;
                        }
                    }

                    if (fiendsRelevant > 0 && fiendMindAttack.attackTimer < fiendMindAttack.maxTime)
                    {
                        for (int i = 0; i < fiendsRelevant; i++)
                        {
                            fiendMindAttack.attackTimer += Time.deltaTime;
                        }
                        if (fiendMindAttack.PlaySound(fiendMindAttack.chargeSound, fiendMindAttack.chargeSource))
                        {
                            fiendMindAttack.chargeSource.time = fiendMindAttack.attackTimer;
                            if (fiendMindAttack.chargeVolume == null)
                            {
                                fiendMindAttack.chargeVolume = fiendMindAttack.chargeSource.GetComponent<VolumeController>();
                            }
                            fiendMindAttack.chargeVolume.fadeValue = 0f;
                        }
                        fiendMindAttack.fadeIncDec = 1f;
                    }
                    else
                    {
                        fiendMindAttack.attackTimer -= Time.deltaTime;
                        if (fiendMindAttack.currentSound != string.Empty)
                        {
                            float time = 0f;
                            if (fiendMindAttack.coolSource != null)
                            {
                                time = fiendMindAttack.chargeSource.clip.length - fiendMindAttack.chargeSource.time;
                            }
                            if (fiendMindAttack.PlaySound(fiendMindAttack.coolSound, fiendMindAttack.coolSource))
                            {
                                fiendMindAttack.coolSource.time = time;
                                if (fiendMindAttack.coolVolume == null)
                                {
                                    fiendMindAttack.coolVolume = fiendMindAttack.coolSource.GetComponent<VolumeController>();
                                }
                                if (fiendMindAttack.coolVolume != null)
                                {
                                    fiendMindAttack.coolVolume.fadeValue = 0f;
                                }
                            }
                            fiendMindAttack.fadeIncDec = -1f;
                        }
                    }
                }
                else
                {
                    fiendMindAttack.attackTimer -= Time.deltaTime * 3f;
                }
                if (fiendMindAttack.chargeVolume != null)
                {
                    fiendMindAttack.chargeVolume.fadeValue = Mathf.Clamp01(fiendMindAttack.chargeVolume.fadeValue + fiendMindAttack.fadeIncDec * Time.deltaTime * 3f);
                }
                if (fiendMindAttack.coolVolume != null)
                {
                    fiendMindAttack.coolVolume.fadeValue = Mathf.Clamp01(fiendMindAttack.coolVolume.fadeValue - fiendMindAttack.fadeIncDec * Time.deltaTime * 3f);
                }
                fiendMindAttack.attackTimer = Mathf.Clamp(fiendMindAttack.attackTimer, 0f, fiendMindAttack.maxTime);
                if (fiendMindAttack.attackTimer == 0f)
                {
                    if (!OculusManager.isOculusEnabled)
                    {
                        fiendMindAttack.mindAttackBleed.enabled = false;
                    }
                    else
                    {
                        fiendMindAttack.oculusMindAttackBleed.enabled = false;
                    }
                }
                else if (!OculusManager.isOculusEnabled)
                {
                    fiendMindAttack.mindAttackBleed.enabled = true;
                    fiendMindAttack.mindAttackBleed.impact -= Time.deltaTime;
                    fiendMindAttack.mindAttackBleed.impact = Mathf.Clamp01(fiendMindAttack.mindAttackBleed.impact);
                    fiendMindAttack.mindAttackBleed.strength = fiendMindAttack.attackTimer / fiendMindAttack.maxTime;
                }
                else
                {
                    fiendMindAttack.oculusMindAttackBleed.enabled = true;
                    fiendMindAttack.oculusMindAttackBleed.impact -= Time.deltaTime;
                    fiendMindAttack.oculusMindAttackBleed.impact = Mathf.Clamp01(fiendMindAttack.oculusMindAttackBleed.impact);
                    fiendMindAttack.oculusMindAttackBleed.strength = fiendMindAttack.attackTimer / fiendMindAttack.maxTime;
                }
                if (!OculusManager.isOculusEnabled)
                {
                    if (fiendMindAttack.mindAttackBleed.strength > 0.1f)
                    {
                        fiendMindAttack.playerHealth.DoDamage(0f, false, PlayerHealth.DamageTypes.MindAttack, false);
                    }
                }
                else if (fiendMindAttack.oculusMindAttackBleed.strength > 0.1f)
                {
                    fiendMindAttack.playerHealth.DoDamage(0f, false, PlayerHealth.DamageTypes.MindAttack, false);
                }
                fiendMindAttack.delayTimer += Time.deltaTime;
                fiendMindAttack.delayTimer = Mathf.Clamp(fiendMindAttack.delayTimer, 0f, fiendMindAttack.maxDelay);
            }
            */

            /*----------------------------------------------------------------------------------------------------*/
            // @FlareObject

            private static void HookFlareObject()
            {
                On.FlareObject.CheckForMonster += new On.FlareObject.hook_CheckForMonster(HookFlareObjectCheckForMonster);
            }

            private static void HookFlareObjectCheckForMonster(On.FlareObject.orig_CheckForMonster orig, FlareObject flareObject)
            {
                foreach (Monster monster in monsterListMonsterComponents)
                {
                    flareObject.monster = monster;
                    orig.Invoke(flareObject);
                }
            }

            // HitMonster moved to BaseFeatures.

            /*----------------------------------------------------------------------------------------------------*/
            // @Flashlight

            private static void HookFlashlight(On.Flashlight.orig_UpdateLightVolume orig, Flashlight flashlight)
            {
                if (flashlight.on && flashlight.inventoryItem.IsInInventory())
                {
                    flashlight.lightRay.origin = flashlight.lightVolume.transform.parent.transform.position;
                    flashlight.lightRay.direction = flashlight.lightBox.transform.forward;
                    RaycastHit raycastHit;
                    if (Physics.Raycast(flashlight.lightRay, out raycastHit, flashlight.flashLight.range, LayerMask.NameToLayer("Default") | LayerMask.NameToLayer("DefaultNavVision")) && raycastHit.collider.transform.root.tag == "Monster")
                    {
                        raycastHit.collider.GetComponentInParent<Monster>().alertMeters.IncreaseAlert("Sight", 100f);
                    }
                    flashlight.lightBox.enabled = true;
                    flashlight.lightBox.gameObject.layer = LayerMask.NameToLayer("LightVolume");
                    RaycastHit raycastHit2 = default(RaycastHit);
                    if (Physics.Raycast(flashlight.lightBox.transform.position, flashlight.lightBox.transform.forward, out raycastHit2, flashlight.flashLight.range, flashlight.lightVolumeLayerMask))
                    {
                        flashlight.lightVolume.transform.localScale = new Vector3(1f, 1f, Vector3.Distance(((MonoBehaviour)flashlight).transform.position, raycastHit2.point));
                    }
                }
                else
                {
                    flashlight.lightBox.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
                    flashlight.lightBox.enabled = false;
                    flashlight.lightVolume.transform.localScale = Vector3.zero;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FSM

            private static void HookFSM(On.FSM.orig_Update orig, FSM fSM)
            {
                if (LevelGeneration.Instance != null && LevelGeneration.Instance.finishedGenerating)
                {
                    orig.Invoke(fSM);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @GameplayAudio

            private static void HookGameplayAudio(On.GameplayAudio.orig_OnSoundPlayed orig, GameplayAudio gameplayAudio, AudioSource _source)
            {
                if (gameplayAudio.gameVolume > 0f && monsterList != null)
                {
                    DistractionSound[] components = _source.gameObject.GetComponents<DistractionSound>();
                    DistractionSound distractionSound = null;
                    for (int i = 0; i < components.Length; i++)
                    {
                        if (components[i].Source == _source)
                        {
                            distractionSound = components[i];
                            break;
                        }
                    }
                    if (distractionSound == null)
                    {
                        if (_source.gameObject.GetComponent<DistractionSound>() == null)
                        {
                            distractionSound = (_source.gameObject.AddComponent(typeof(DistractionSound)) as DistractionSound);
                        }
                        else
                        {
                            distractionSound = _source.gameObject.GetComponent<DistractionSound>();
                        }
                    }
                    if (!_source.isPlaying)
                    {
                        distractionSound.enabled = false;
                    }
                    else
                    {
                        distractionSound.enabled = true;
                    }
                    if (gameplayAudio.monsterSound == null)
                    {
                        gameplayAudio.monsterSound = References.Monster.GetComponent<MonsterHearing>();
                    }
                    if (gameplayAudio.monsterSound != null)
                    {
                        distractionSound.Init(_source, gameplayAudio.library, gameplayAudio, gameplayAudio.monsterSound.transform);
                        foreach (GameObject monsterGameObject in monsterList)
                        {
                            monsterGameObject.GetComponent<MonsterHearing>().AddSound(distractionSound);
                        }
                    }
                }
                else if (_source != null)
                {
                    UnityEngine.Object.Destroy(_source.gameObject.GetComponent<DistractionSound>());
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @GlobalMusic

            private static void HookGlobalMusic()
            {
                On.GlobalMusic.CheckMusicState += new On.GlobalMusic.hook_CheckMusicState(HookGlobalMusicCheckMusicState);
                On.GlobalMusic.Update += new On.GlobalMusic.hook_Update(HookGlobalMusicUpdate);
            }

            private static void HookGlobalMusicCheckMusicState(On.GlobalMusic.orig_CheckMusicState orig, GlobalMusic globalMusic)
            {
                bool doesAPlayerHaveSpawnProtection = false;
                for (int i = 0; i < ModSettings.spawnProtection.Count; i++)
                {
                    if (ModSettings.spawnProtection[i])
                    {
                        doesAPlayerHaveSpawnProtection = true;
                        break;
                    }
                }
                if (globalMusic.monstFSM == null && lastMonsterSeen == null)
                {
                    globalMusic.monstFSM = globalMusic.monster.GetComponent<FSM>();
                }
                else
                {

                    if (!doesAPlayerHaveSpawnProtection && lastMonsterSeen != null && globalMusic.monstFSM.Current.typeofState.ToString() != lastMonsterSeen.GetComponent<FSM>().Current.typeofState.ToString())
                    {
                        globalMusic.monster = lastMonsterSeen;
                        globalMusic.monsterType = lastMonsterSeen.monsterType;
                        globalMusic.monstFSM = lastMonsterSeen.GetComponent<FSM>();
                    }
                }
                if (globalMusic.monster != null && globalMusic.monsterBeenEncountered)
                {
                    if ((globalMusic.currentState != null && globalMusic.currentState != globalMusic.monstFSM.Current.typeofState.ToString()) || globalMusic.changeSong)
                    {
                        globalMusic.prevState = globalMusic.currentState;
                        globalMusic.currentState = globalMusic.monstFSM.Current.typeofState.ToString();
                        if (GlobalMusic.Hascooled || globalMusic.changeSong)
                        {
                            if (globalMusic.monsterType.Equals("Sparky") && SparkyMode.sparkyAudioClips == null)
                            {
                                globalMusic.monsterType = "Brute"; // Make sure music plays for Sparky.
                            }
                            if (!doesAPlayerHaveSpawnProtection)
                            {
                                string text = globalMusic.currentState;
                                if (text != null)
                                {
                                    if (!(text == "FinaleEvent"))
                                    {
                                        if (!(text == "Chase"))
                                        {
                                            if (!(text == "RoomSearch"))
                                            {
                                                if (!(text == "LowAlert"))
                                                {
                                                    if (text == "IgnoreThis" && GlobalMusic.Hascooled && (globalMusic.song == "Music/Cooldown/AfterHide/" + globalMusic.monsterType || globalMusic.song == "Music/Cooldown/" + globalMusic.monsterType))
                                                    {
                                                        globalMusic.song = "Music/NoAlert/" + globalMusic.monsterType;
                                                    }
                                                }
                                                else if ((globalMusic.prevState == "RoomSearch" || globalMusic.prevState == "Chase") && !GlobalMusic.cooldown)
                                                {
                                                    globalMusic.monster.firstEscape = true;
                                                    GlobalMusic.cooldown = true;
                                                }
                                                else if (globalMusic.monster.MonsterType == Monster.MonsterTypeEnum.Fiend)
                                                {
                                                    globalMusic.song = "Music/NoAlert/Fiend";
                                                }
                                                else
                                                {
                                                    globalMusic.song = "Music/NoAlert/" + globalMusic.monsterType;
                                                }
                                            }
                                            else if (globalMusic.monster.RoomDetect.CurrentRoom == globalMusic.monster.PlayerDetectRoom.GetRoom || globalMusic.monster.RoomDetect.CurrentRoom.Category == RoomCategory.Outside)
                                            {
                                                globalMusic.song = "Music/Hiding/" + globalMusic.monsterType;
                                                GlobalMusic.cooltime = 0f;
                                            }
                                        }
                                        else
                                        {
                                            globalMusic.song = "Music/HighAlert/" + globalMusic.monsterType;
                                            GlobalMusic.cooltime = 0f;
                                        }
                                    }
                                    else
                                    {
                                        globalMusic.song = "Music/SubEvent/" + globalMusic.monsterType;
                                        GlobalMusic.cooltime = 0f;
                                    }
                                }
                            }
                            else
                            {
                                globalMusic.song = "Music/NoAlert/" + globalMusic.monsterType;
                            }
                            if (globalMusic.currentlyPlaying != globalMusic.song)
                            {
                                globalMusic.ChangeVariables(false, true, true, false, 0f);
                                return;
                            }
                        }
                    }
                }
                else
                {
                    globalMusic.song = "Music/NoAlert/Original";
                }
            }

            private static void HookGlobalMusicUpdate(On.GlobalMusic.orig_Update orig, GlobalMusic globalMusic)
            {
                try
                {
                    if (LevelGeneration.Instance.GetMonster != null && globalMusic.monster == null)
                    {
                        globalMusic.monster = LevelGeneration.Instance.GetMonster.GetComponent<Monster>();
                    }
                    if (globalMusic.volumeCon == null)
                    {
                        globalMusic.volumeCon = ((MonoBehaviour)globalMusic).gameObject.GetComponent<VolumeController>();
                    }
                    if (lastMonsterSeen != null)
                    {
                        globalMusic.monsterType = lastMonsterSeen.monsterType;
                    }
                    if ((globalMusic.monster != null && (globalMusic.monster.HasPlayerBeenSeen || globalMusic.monster.TheSubAlarm.eventStarted || (globalMusic.monster.HeliEscape != null && globalMusic.monster.HeliEscape.heliEscapeStarted) || (globalMusic.monster.RoomDetect.CurrentRoom != null && globalMusic.monster.RoomDetect.CurrentRoom == globalMusic.monster.PlayerDetectRoom.GetRoom))) || lastMonsterSeen != null)
                    {
                        globalMusic.monsterBeenEncountered = true;
                    }
                    if (globalMusic.monsterType.Equals("Sparky") && SparkyMode.sparkyAudioClips == null)
                    {
                        globalMusic.monsterType = "Brute"; // Make sure music plays for Sparky.
                    }
                    globalMusic.CheckMusicState();
                    globalMusic.SwitchSources();
                    globalMusic.CheckForCooldown();
                    globalMusic.LerpAndPlay();
                    globalMusic.CrossFade();
                    if (globalMusic.lib != null)
                    {
                        globalMusic.faderVal = globalMusic.lib.fadeValue;
                    }
                }
                catch /*(Exception e)*/
                {
                    // # Still happens when not using legacy audio.
                    //Debug.Log("There is still an error in GlobalMusic\n" + e.ToString());
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Helicopter

            private static void HookHelicopter(On.Helicopter.orig_Update orig, Helicopter helicopter)
            {
                /*
                if (orig != null && helicopter != null && EscapeChecker.Completeness.Complete != null)
                {
                    if (helicopter.heliEsc != null && helicopter.comHoseAttached != null && helicopter.comHoseAttached != null)
                    {
                        if (helicopter.heliEsc.AttachedToHeli && helicopter.comHoseAttached != EscapeChecker.Completeness.Complete)
                        {
                            helicopter.comHoseAttached = EscapeChecker.Completeness.Complete;
                            helicopter.UpdateList();
                        }
                    }

                    if (helicopter.heliEsc.CablesCut != null && helicopter.comBoltCutters != null && helicopter.comBoltCutters != null)
                    {
                        if (helicopter.heliEsc.CablesCut && helicopter.comBoltCutters != EscapeChecker.Completeness.Complete)
                        {
                            helicopter.comBoltCutters = EscapeChecker.Completeness.Complete;
                            helicopter.UpdateList();
                        }
                    }

                    if (helicopter.heliEsc.KeyUsed != null && helicopter.comHeliKey != null && helicopter.comHeliKey != null)
                    {
                        if (helicopter.heliEsc.KeyUsed && helicopter.comHeliKey != EscapeChecker.Completeness.Complete)
                        {
                            helicopter.comHeliKey = EscapeChecker.Completeness.Complete;
                            helicopter.UpdateList();
                        }
                    }

                    if (helicopter.heliEsc.ReadyToFuel != null && helicopter.comFuel != null && helicopter.comFuel != null)
                    {
                        if (helicopter.heliEsc.ReadyToFuel && helicopter.comFuel != EscapeChecker.Completeness.Complete)
                        {
                            helicopter.comFuel = EscapeChecker.Completeness.Complete;
                            helicopter.UpdateList();
                        }
                    }

                    if (helicopter.heliEsc.fuellingBegin != null && helicopter.comFullyFuelled != null && helicopter.comFullyFuelled != null)
                    {
                        if (helicopter.heliEsc.fuellingBegin && helicopter.comFullyFuelled != EscapeChecker.Completeness.Complete)
                        {
                            helicopter.comFullyFuelled = EscapeChecker.Completeness.Complete;
                            helicopter.UpdateList();
                        }
                    }
                }
                */
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @HelicopterEscape

            private static void HookHelicopterEscape()
            {
                On.HelicopterEscape.BeginFuelling += new On.HelicopterEscape.hook_BeginFuelling(HookHelicopterEscapeBeginFueling);
                On.HelicopterEscape.Start += new On.HelicopterEscape.hook_Start(HookHelicopterEscapeStart);
            }

            private static void HookHelicopterEscapeBeginFueling(On.HelicopterEscape.orig_BeginFuelling orig, HelicopterEscape helicopterEscape)
            {
                orig.Invoke(helicopterEscape);
                for (int i = 1; i < monsterList.Count; i++)
                {
                    monsterListMonsterComponents[i].HeliEscape = helicopterEscape;
                }
            }

            private static void HookHelicopterEscapeStart(On.HelicopterEscape.orig_Start orig, HelicopterEscape helicopterEscape)
            {
                orig.Invoke(helicopterEscape);
                for (int i = 1; i < monsterList.Count; i++)
                {
                    monsterList[i].GetComponentsInChildren<Helicopter>(true)[0].heliEsc = helicopterEscape;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @HunterAnimationScript

            private static void HookHunterAnimationsScript()
            {
                On.HunterAnimationsScript.Start += new On.HunterAnimationsScript.hook_Start(HookHunterAnimationsScriptStart);
                On.HunterAnimationsScript.UpdateHunterVals += new On.HunterAnimationsScript.hook_UpdateHunterVals(HookHunterAnimationsScriptUpdateHunterVals);
            }

            private static void HookHunterAnimationsScriptStart(On.HunterAnimationsScript.orig_Start orig, HunterAnimationsScript hunterAnimationsScript)
            {
                hunterAnimationsScript.monster = ((MonoBehaviour)hunterAnimationsScript).GetComponentInParent<Monster>();
                hunterAnimationsScript.GetHunterAnimator();
            }

            private static void HookHunterAnimationsScriptUpdateHunterVals(On.HunterAnimationsScript.orig_UpdateHunterVals orig, HunterAnimationsScript hunterAnimationsScript)
            {
                if (hunterAnimationsScript.monster != null && hunterAnimationsScript.monster.MoveControl != null)
                {
                    hunterAnimationsScript.maxSpeed = hunterAnimationsScript.monster.MoveControl.MaxSpeed;
                }
                else if (hunterAnimationsScript.monster == null)
                {
                    hunterAnimationsScript.monster = monsterListMonsterComponents[MonsterNumber(((MonoBehaviour)hunterAnimationsScript).GetComponentInParent<Monster>().GetInstanceID())];
                }
                if (hunterAnimationsScript.hunterAnimations == null)
                {
                    hunterAnimationsScript.GetHunterAnimator();
                }
                if (hunterAnimationsScript.hunterAnimations != null)
                {
                    hunterAnimationsScript.hunterAnimations.SetBool("GoToHiding", hunterAnimationsScript.GoToHiding);
                    hunterAnimationsScript.hunterAnimations.SetFloat("SpawnType", hunterAnimationsScript.SpawnType);
                    hunterAnimationsScript.hunterAnimations.SetFloat("IdleType", hunterAnimationsScript.idleType);
                    hunterAnimationsScript.hunterAnimations.SetFloat("RoarType", hunterAnimationsScript.roarType);
                    hunterAnimationsScript.hunterAnimations.SetFloat("HideType", hunterAnimationsScript.hideType);
                    hunterAnimationsScript.hunterAnimations.SetFloat("MaxSpeed", hunterAnimationsScript.maxSpeed);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @HunterThreshold

            private static void HunterThresholdChanges(Monster monster)
            {
                if (monster != null && monster.MonsterType == Monster.MonsterTypeEnum.Hunter)
                {
                    HunterThresholdChanges(monster, HunterNumber(monster.GetInstanceID()));
                }
            }

            private static void HunterThresholdChanges(Monster monster, int hunterNumber)
            {
                try
                {
                    if (monster.SightAlert > hunterThresholdValList[hunterNumber])
                    {
                        sightBelowThresholdList[hunterNumber] = false;
                    }
                    else
                    {
                        sightBelowThresholdList[hunterNumber] = true;
                    }
                }
                catch
                {
                    Debug.Log("Error in HunterThresholdChanges 1");
                }

                try
                {
                    if (monster.SoundAlert > hunterThresholdValList[hunterNumber])
                    {
                        soundBelowThresholdList[hunterNumber] = false;
                    }
                    else
                    {
                        soundBelowThresholdList[hunterNumber] = true;
                    }
                }
                catch
                {
                    Debug.Log("Error in HunterThresholdChanges 2");
                }

                try
                {
                    if (monster.ProxAlert > hunterThresholdValList[hunterNumber])
                    {
                        proxBelowThresholdList[hunterNumber] = false;
                    }
                    else
                    {
                        proxBelowThresholdList[hunterNumber] = true;
                    }
                }
                catch
                {
                    Debug.Log("Error in HunterThresholdChanges 3");
                }

                try
                {
                    if (sightBelowThresholdList[hunterNumber] && soundBelowThresholdList[hunterNumber] && proxBelowThresholdList[hunterNumber])
                    {
                        allBelowThresholdList[hunterNumber] = true;
                    }
                    else
                    {
                        HunterThreshold.AllBelowThreshold = false;
                    }
                }
                catch
                {
                    Debug.Log("Error in HunterThresholdChanges 4");
                }
            }

            private static bool IsBelowThreshold(Monster monster)
            {
                if (monster != null && monster.MonsterType == Monster.MonsterTypeEnum.Hunter)
                {
                    HunterThresholdChanges(monster);
                    return HunterThreshold.AllBelowThreshold;
                }
                else
                {
                    return false;
                }
            }

            private static void SetThresholdValue(Monster monster)
            {
                if (monster != null && monster.MonsterType == Monster.MonsterTypeEnum.Hunter)
                {
                    int hunterNumber = HunterNumber(monster.GetInstanceID());
                    hunterThresholdValList[hunterNumber] = monster.thresholdBaseValue - (monster.thresholdDecrease + 5f * monster.GetMonEffectiveness.HowEffective);
                    hunterThresholdValList[hunterNumber] = Mathf.Clamp(hunterThresholdValList[hunterNumber], monster.thresholdLowerClamp, monster.thresholdUpperClamp);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Inventory

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

            static List<string> missionItems = new List<string>(new string[] { "BoltCutters", "Chain spool", "DuctTape", "Gasoline Canister", "Helicopter Keys", "Pump", "Sub Battery", "Sub HeadLights", "Welding Kit" });

            private static void HookInventoryAddToInventory(On.Inventory.orig_AddToInventory orig, Inventory inventory, InventoryItem _inventoryItem)
            {
                // Spawn an additional monster if item monster frenzy is enabled AND [the item picked up is a mission item OR extra chaotic IMF is enabled] AND [the monster spawning limit is set to unlimited OR the number of monsters is less than the monster spawning limit].
                if (((ModSettings.itemMonsterFrenzy && missionItems.Contains(_inventoryItem.itemName)) || ModSettings.extraChaoticIMF) && (ModSettings.monsterSpawningLimit == 0 || ModSettings.numberOfMonsters <= ModSettings.monsterSpawningLimit))
                {
                    Debug.Log(string.Concat(new object[] { "Picked up ", _inventoryItem.itemName, ". Spawning new monster." }));
                    CreateNewMonster();
                }
                orig.Invoke(inventory, _inventoryItem);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @JumpCheck

            private static void HookJumpCheck()
            {
                On.JumpCheck.OnTriggerEnter += new On.JumpCheck.hook_OnTriggerEnter(HookJumpCheckOnTriggerEnter);
                On.JumpCheck.OnTriggerExit += new On.JumpCheck.hook_OnTriggerExit(HookJumpCheckOnTriggerExit);
                On.JumpCheck.Update += new On.JumpCheck.hook_Update(HookJumpCheckUpdate);
            }

            private static void HookJumpCheckOnTriggerEnter(On.JumpCheck.orig_OnTriggerEnter orig, JumpCheck jumpCheck, Collider _Other)
            {
                if (_Other.transform.root.tag == "Monster")
                {
                    Monster monster = _Other.GetComponentInParent<Monster>();
                    monster.MoveControl.nearJump = true;
                    monster.MoveControl.JumpCentre = ((MonoBehaviour)jumpCheck).transform;
                    monster.MoveControl.JumpFromHere = ((MonoBehaviour)jumpCheck).GetComponent<Collider>().ClosestPointOnBounds(monster.transform.position);
                }
            }

            private static void HookJumpCheckOnTriggerExit(On.JumpCheck.orig_OnTriggerExit orig, JumpCheck jumpCheck, Collider _Other)
            {
                if (_Other.transform.root.tag == "Monster")
                {
                    _Other.GetComponentInParent<Monster>().MoveControl.nearJump = false;
                }
            }

            private static void HookJumpCheckUpdate(On.JumpCheck.orig_Update orig, JumpCheck jumpCheck)
            {
                for (int i = 0; i < monsterList.Count; i++)
                {
                    jumpCheck.monster = monsterListMonsterComponents[i];
                    if (jumpCheck.monster.RoomDetect.CurrentRoom != null)
                    {
                        if (jumpCheck.monster.RoomDetect.CurrentRoom.RoomType != RoomStructure.Cargo)
                        {
                            jumpCheck.JumpToFalse();
                        }
                        else if (jumpCheck.monster.MoveControl.nearJump)
                        {
                            jumpCheck.timeNearJump += Time.deltaTime;
                            if (jumpCheck.timeNearJump > 20f)
                            {
                                jumpCheck.JumpToFalse();
                            }
                        }
                    }
                    else
                    {
                        jumpCheck.JumpToFalse();
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @LerpToHidingSpot

            public static bool HookLerpToHidingSpotIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookLerpToHidingSpot((LerpToHidingSpot)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookLerpToHidingSpot(LerpToHidingSpot lerpToHidingSpot)
            {
                HidingSpot spot = ((MonsterHidingEvents)lerpToHidingSpot).GetComponentInParent<HidingSpot>();
                if (spot != null && lastMonsterSentMessage != null)
                {
                    int monsterNumber = MonsterNumber(lastMonsterSentMessage.GetInstanceID());
                    monstersFinishedLerpToHidingSpot[monsterNumber] = false;
                    GameObject monsterGameObject = lastMonsterSentMessage.gameObject;
                    float t = 0f;
                    Vector3 startPosition = monsterGameObject.transform.position;
                    Vector3 targetPosition = spot.MonsterPoint;
                    Quaternion startRotation = monsterGameObject.transform.rotation;
                    Vector3 normal = spot.MonsterNormal;
                    normal.y = 0f;
                    normal.Normalize();
                    Quaternion targetRotation = Quaternion.LookRotation(normal, Vector3.up);
                    while (t < 1f)
                    {
                        t += Time.deltaTime * lerpToHidingSpot.lerpSpeed;
                        monsterGameObject.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                        monsterGameObject.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                        yield return null;
                    }
                    monsterGameObject.transform.position = targetPosition;
                    monsterGameObject.transform.rotation = targetRotation;
                    monsterGameObject.gameObject.SendMessage("OnMonsterFinishLerp");
                    monstersFinishedLerpToHidingSpot[monsterNumber] = true;
                }
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @LerpToSearchSpot

            public static bool HookLerpToSearchSpotIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookLerpToSearchSpot((LerpToSearchSpot)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookLerpToSearchSpot(LerpToSearchSpot lerpToSearchSpot)
            {
                SearchEventSpot spot = ((RoomSearchEvent)lerpToSearchSpot).GetComponentInParent<SearchEventSpot>();
                if (spot != null && lastMonsterSentMessage != null)
                {
                    GameObject monsterGameObject = lastMonsterSentMessage.gameObject;
                    float t = 0f;
                    Vector3 startPosition = monsterGameObject.transform.position;
                    Vector3 targetPosition = spot.MonsterPoint;
                    Quaternion startRotation = monsterGameObject.transform.rotation;
                    Vector3 normal = spot.MonsterNormal;
                    normal.y = 0f;
                    normal.Normalize();
                    Quaternion targetRotation = Quaternion.LookRotation(normal, Vector3.up);
                    while (t < 1f)
                    {
                        t += Time.deltaTime * lerpToSearchSpot.lerpSpeed;
                        monsterGameObject.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                        monsterGameObject.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                        yield return null;
                    }
                    monsterGameObject.transform.position = targetPosition;
                    monsterGameObject.transform.rotation = targetRotation;
                    monsterGameObject.gameObject.SendMessage("OnMonsterFinishLerp");
                }
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @LevelGeneration

            private static void HookLevelGenerationAwake(On.LevelGeneration.orig_Awake orig, LevelGeneration levelGeneration)
            {
                BaseFeatures.CommonLevelGeneration1(levelGeneration);
                MonsterSelection monsterSelection = UnityEngine.Object.FindObjectOfType<MonsterSelection>();
                levelGeneration.allMonsters = GameObject.FindGameObjectsWithTag("Monster");

                // Initialise the monster list lists [not arrays anymore].
                monsterList = new List<GameObject>();
                monsterListMonsterComponents = new List<Monster>();
                monsterInstanceIDtoMonsterNumberDict = new Dictionary<int, int>();
                monsterListStates = new List<MState>();

                if (monsterSelection != null)
                {
                    /*
                    // Run the base features level generation script.
                    BaseFeatures.HookLevelGeneration(orig, levelGeneration);
                    */

                    for (int i = 0; i < ModSettings.numberOfMonsters; i++)
                    {
                        try
                        {
                            if (i < ModSettings.numberOfRandomMonsters)
                            {
                                GameObject monsterGameObject = Instantiate(monsterSelection.Select());
                                if (ModSettings.useSparky)
                                {
                                    // If the original weighted selection is used, which it only is when the player uses random monsters, then let Sparky be chosen too at a 1/4 chance.
                                    if (monsterSelection.playerPrefIdentifier == "MonsterCounts")
                                    {
                                        int upperIndexLimit = GameObject.FindGameObjectsWithTag("Monster").Length;
                                        int randomNumber = UnityEngine.Random.Range(0, upperIndexLimit + 1);
                                        if (randomNumber == upperIndexLimit)
                                        {
                                            monsterGameObject = Sparky.CreateSparky(monsterSelection).gameObject;
                                        }
                                    }
                                }
                                if (ModSettings.bannedRandomMonsters.Count > 0)
                                {
                                    if (ModSettings.bannedRandomMonsters.Count == 3)
                                    {
                                        if (ModSettings.useSparky)
                                        {
                                            monsterGameObject = Sparky.CreateSparky(monsterSelection).gameObject;
                                        }
                                        else
                                        {
                                            monsterGameObject = Instantiate(monsterSelection.Select());
                                        }
                                    }
                                    else
                                    {
                                        do
                                        {
                                            monsterGameObject = Instantiate(monsterSelection.Select());
                                        } while (ModSettings.bannedRandomMonsters.Contains(monsterGameObject.GetComponent<Monster>().monsterType));
                                    }
                                }
                                monsterList.Add(monsterGameObject);
                            }
                            else if (i < ModSettings.numberOfRandomMonsters + ModSettings.numberOfBrutes)
                            {
                                monsterList.Add(UnityEngine.Object.Instantiate<GameObject>(monsterSelection.NameToObject("Brute")));
                            }
                            else if (i < ModSettings.numberOfRandomMonsters + ModSettings.numberOfBrutes + ModSettings.numberOfHunters)
                            {
                                monsterList.Add(UnityEngine.Object.Instantiate<GameObject>(monsterSelection.NameToObject("Hunter")));
                            }
                            else if (i < ModSettings.numberOfRandomMonsters + ModSettings.numberOfBrutes + ModSettings.numberOfHunters + ModSettings.numberOfFiends)
                            {
                                monsterList.Add(UnityEngine.Object.Instantiate<GameObject>(monsterSelection.NameToObject("Fiend")));
                            }
                            else if (i < ModSettings.numberOfRandomMonsters + ModSettings.numberOfBrutes + ModSettings.numberOfHunters + ModSettings.numberOfFiends + ModSettings.numberOfSparkies)
                            {
                                monsterList.Add(Sparky.CreateSparky(monsterSelection).gameObject);
                            }
                            else if (i < ModSettings.numberOfRandomMonsters + ModSettings.numberOfBrutes + ModSettings.numberOfHunters + ModSettings.numberOfFiends + ModSettings.numberOfSparkies + ModSettings.numberOfSmokeMonsters)
                            {
                                monsterList.Add(SmokeMonster.CreateSmokeMonster(monsterSelection));
                            }

                            monsterListMonsterComponents.Add(monsterList[i].GetComponent<Monster>());
                            monsterInstanceIDtoMonsterNumberDict.Add(monsterListMonsterComponents[i].GetInstanceID(), i);
                            monsterListStates.Add(monsterList[i].GetComponent<MState>());
                            UnityEngine.Debug.Log(string.Concat(new object[] { "INSTANCE ID FOR MONSTER NUMBER ", i, " OF TYPE ", monsterListMonsterComponents[i].MonsterType.ToString(), " ----- The ID stored is " + monsterListMonsterComponents[i].GetInstanceID() + "." }));
                        }
                        catch (Exception e)
                        {
                            UnityEngine.Debug.Log(string.Concat(new object[] { "Error while creating monster number ", i, " during level generation:\n", e.ToString() }));
                        }
                    }

                    /*
                    for (int i = 0; i < monsterList.Count; i++)
                    {
                        for (int j = 0; i < monsterList.Count; j++)
                        {
                            if (i != j)
                            {
                                Physics.IgnoreCollision(monsterList[i].GetComponent<Collider>(), monsterList[j].GetComponent<Collider>(), true);
                            }
                        }
                    }
                    */

                    FillBrutes();
                    FillHunters();
                    FillFiends();

                    try
                    {
                        if (hunters != null && hunters.Count > 0)
                        {
                            levelGeneration.selectedMonster = hunters[0];
                        }
                        else
                        {
                            levelGeneration.selectedMonster = monsterList[0];
                        }
                    }
                    catch
                    {
                        Debug.Log("Error while assigning selected monster during level generation.");
                    }
                    levelGeneration.chosenMonstType = levelGeneration.selectedMonster.GetComponent<Monster>().MonsterType;
                    UnityEngine.Debug.Log(string.Concat(new object[] { "The monster used for References.Monster is of type " + levelGeneration.chosenMonstType }));
                }
                else
                {
                    Debug.Log("MonsterSelection is not in the scene! No monster!");
                }
                BaseFeatures.CommonLevelGeneration2(levelGeneration);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Liferaft

            private static void HookLiferaft(On.Liferaft.orig_Start orig, Liferaft liferaft)
            {
                orig.Invoke(liferaft);
                for (int i = 1; i < monsterList.Count; i++)
                {
                    monsterList[i].GetComponentsInChildren<RaftEscapeCheck>(true)[0].liferaft = liferaft;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @LockedInState

            private static IEnumerator HookLockedInState(On.LockedInState.orig_LerpToVent orig, LockedInState lockedInState)
            {
                float t = 0f;
                Vector3 startPosition = ((MState)lockedInState).transform.position;
                Vector3 targetPosition = lockedInState.goHere;
                while (t < 1f)
                {
                    t += Time.deltaTime * lockedInState.lerpSpeed;
                    ((MState)lockedInState).transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                    yield return null;
                }
                ((MState)lockedInState).transform.position = targetPosition;
                lockedInState.GoIntoHiding();
                lockedInState.timeAttemptingToHide.StartTimer();
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @LockerReverb

            private static void HookLockerReverb(On.LockerReverb.orig_Update orig, LockerReverb lockerReverb)
            {
                foreach (Monster monster in monsterListMonsterComponents)
                {
                    LockerReverb.monster = monster;

                    if (!ModSettings.enableMultiplayer)
                    {
                        orig.Invoke(lockerReverb);
                    }
                    else
                    {
                        MultiplayerMode.LockerReverbUpdate(lockerReverb);
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MAlertMeters

            private static void HookMAlertMeters(On.MAlertMeters.orig_ProximityChecker orig, MAlertMeters mAlertMeters)
            {
                if (mAlertMeters.monster != null)
                {
                    if (PlayerToMonsterCast(mAlertMeters.monster) && PlayerToMonsterCast2(mAlertMeters.monster) && !mAlertMeters.monster.IsPlayerHiding)
                    {
                        mAlertMeters.proxDist = (((MonoBehaviour)mAlertMeters).transform.position - mAlertMeters.monster.player.transform.position).magnitude;
                        if (mAlertMeters.proxDist < mAlertMeters.proxRange)
                        {
                            mAlertMeters.IncreaseAlert("Prox", 100f / (mAlertMeters.proxDist * 0.2f));
                            mAlertMeters.proxTimeout = 0f;
                        }
                    }
                    else if (mAlertMeters.proxTimeout > 5f || mAlertMeters.monster.IsPlayerHiding)
                    {
                        mAlertMeters.DecreaseAlert("Prox", 50f);
                    }
                    float num = mAlertMeters.mProxAlert;
                    if (mAlertMeters.savedProximity != num)
                    {
                        if (mAlertMeters.savedProximity < num)
                        {
                            mAlertMeters.isWithinProximity = true;
                        }
                        else
                        {
                            mAlertMeters.isWithinProximity = false;
                        }
                        mAlertMeters.savedProximity = num;
                    }
                    if (mAlertMeters.savedProximity > 90f)
                    {
                        mAlertMeters.timeAtHighProximity += Time.deltaTime;
                        if (mAlertMeters.timeAtHighProximity > 3f || mAlertMeters.proxDist <= 5f)
                        {
                            mAlertMeters.highProximity = true;
                        }
                    }
                    else
                    {
                        mAlertMeters.timeAtHighProximity = 0f;
                        mAlertMeters.highProximity = false;
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MAttackingState2

            private static void HookMAttackingState2()
            {
                On.MAttackingState2.CheckForDoor += new On.MAttackingState2.hook_CheckForDoor(HookMAttackingState2CheckForDoor);
                new Hook(typeof(MAttackingState2).GetMethod("LerpToThis", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.ManyMonstersMode).GetMethod("HookMAttackingState2LerpToThis"), null);
                On.MAttackingState2.OnEnter += new On.MAttackingState2.hook_OnEnter(HookMAttackingState2OnEnter);
                On.MAttackingState2.OnUpdate += new On.MAttackingState2.hook_OnUpdate(HookMAttackingState2OnUpdate);
            }

            private static void HookMAttackingState2CheckForDoor(On.MAttackingState2.orig_CheckForDoor orig, MAttackingState2 mAttackingState2)
            {
                Vector3 vector = ((MState)mAttackingState2).monster.transform.position + Vector3.up;
                Vector3 vector2 = mAttackingState2.playerClass.transform.position + Vector3.up;
                Vector3 direction = vector2 - vector;
                float maxDistance = Vector3.Distance(vector, vector2);
                RaycastHit raycastHit;
                if (Physics.Raycast(new Ray(vector, direction), out raycastHit, maxDistance, mAttackingState2.doorMask))
                {
                    Door door = raycastHit.collider.GetComponentInParent<Door>();
                    if (door != null && ChooseAttack.atkList != ChooseAttack.AttackList.StandHiding && ChooseAttack.atkList != ChooseAttack.AttackList.CrouchHidingWithDoor)
                    {
                        BlastOffDoor(((MState)mAttackingState2).monster.gameObject, door, 2f);
                    }
                }
            }

            public static System.Collections.IEnumerator HookMAttackingState2LerpToThis(On.MAttackingState2.orig_LerpToThis orig, MAttackingState2 mAttackingState2, Vector3 targetPos, Vector3 faceThis, MAttackingState2.Characters thisCharacter)
            {
                ((MState)mAttackingState2).monster.MoveControl.LockRotation = true;
                float t = 0f;
                GameObject target;
                if (thisCharacter == MAttackingState2.Characters.Monster)
                {
                    target = monsterList[MonsterNumber(((MState)mAttackingState2).monster.GetInstanceID())];
                }
                else
                {
                    target = ((MState)mAttackingState2).monster.player;
                }
                Vector3 startPosition = target.transform.position;
                Quaternion startRotation = target.transform.rotation;
                Vector3 forward = faceThis - targetPos;
                Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);
                while (t < 1f)
                {
                    t += Time.deltaTime * 2f;
                    if (thisCharacter == MAttackingState2.Characters.Player)
                    {
                        mAttackingState2.playerTargetPos = targetPos;
                        mAttackingState2.playerTargetRot = targetRotation;
                    }
                    else
                    {
                        mAttackingState2.monsterTargetPos = targetPos;
                        mAttackingState2.monsterTargetRot = targetRotation;
                    }
                    target.transform.position = Vector3.Lerp(startPosition, targetPos, t);
                    target.transform.rotation = Quaternion.Lerp(startRotation, targetRotation, t);
                    yield return null;
                }
                target.transform.position = targetPos;
                target.transform.rotation = targetRotation;
                mAttackingState2.OnFacingPosition(thisCharacter);
                yield break;
            }

            private static void HookMAttackingState2OnEnter(On.MAttackingState2.orig_OnEnter orig, MAttackingState2 mAttackingState2)
            {
                mAttackingState2.playerClass = ((MState)mAttackingState2).monster.player.GetComponent<NewPlayerClass>();
                mAttackingState2.upperBodyLock = mAttackingState2.playerClass.upperBodyLock;
                mAttackingState2.playerClass.IsDying = true;
                ManyMonstersMode.SetMonsterLayers(mAttackingState2.monster);
                MultiplayerMode.SetPlayerLayers(mAttackingState2.playerClass);
                mAttackingState2.readyForAttack = false;
                mAttackingState2.playerInPosition = false;
                ((MState)mAttackingState2).monster.MoveControl.MaxSpeed = 0f;
                ((MState)mAttackingState2).monster.MoveControl.GetAniControl.AnimationSpeed = 0f;
                Inventory inventory;
                if (!ModSettings.enableMultiplayer)
                {
                    inventory = References.Inventory;
                }
                else
                {
                    inventory = MultiplayerMode.PlayerInventory(mAttackingState2.playerClass);
                }
                inventory.DisableItemChanges();
                inventory.DropAndDestroy();
                mAttackingState2.LerpToPosition();
                mAttackingState2.doorMask = 1 << LayerMask.NameToLayer("VisionOnly");
            }

            private static void HookMAttackingState2OnUpdate(On.MAttackingState2.orig_OnUpdate orig, MAttackingState2 mAttackingState2)
            {
                ManyMonstersMode.SetMonsterLayers(mAttackingState2.monster);
                MultiplayerMode.SetPlayerLayers(((MState)mAttackingState2).monster.player.GetComponent<NewPlayerClass>());
                ((MState)mAttackingState2).monster.GetMainCollider.isTrigger = true;
                if (mAttackingState2.dying)
                {
                    mAttackingState2.upperBodyLock.weighting3 -= Time.deltaTime * 3f;
                }
                if (mAttackingState2.readyForAttack && mAttackingState2.playerInPosition)
                {
                    ((MState)mAttackingState2).monster.MoveControl.GetAniControl.IsAttacking = true;
                    ((MState)mAttackingState2).monster.MoveControl.enabled = false;
                    mAttackingState2.playerClass.LockEverything();
                    mAttackingState2.playerClass.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.None;
                    mAttackingState2.CheckForDoor();
                    mAttackingState2.ForcePosAndRot();
                    if (!mAttackingState2.greatFadeStarted)
                    {
                        mAttackingState2.greatFadeStarted = true;
                        Ducking.InversetSetValue("Monster", 1E-05f, 2f);

                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MChasingState

            private static void HookMChasingState()
            {
                // StateChanges and Chase have been moved to BaseFeatures.
                On.MChasingState.OnEnter += new On.MChasingState.hook_OnEnter(HookMChasingStateOnEnter);
                On.MChasingState.OnUpdate += new On.MChasingState.hook_OnUpdate(HookMChasingStateOnUpdate);
                On.MChasingState.OnExit += new On.MChasingState.hook_OnExit(HookMChasingStateOnExit);
            }

            private static void HookMChasingStateOnEnter(On.MChasingState.orig_OnEnter orig, MChasingState mChasingState)
            {
                if (ModSettings.enableMultiplayer && !MultiplayerMode.useLegacyAudio)
                {
                    if (((MState)mChasingState).monster.AudSource != null && !((MState)mChasingState).monster.IsRoaring && ((MState)mChasingState).monster.TimeSinceLastRoar > 20f)
                    {
                        AudioSource originalAudioSource = ((MState)mChasingState).monster.AudSource;
                        VirtualAudioSource virtualAudioSource = originalAudioSource.gameObject.GetComponent<VirtualAudioSource>();
                        if (virtualAudioSource != null)
                        {
                            virtualAudioSource.Stop();
                        }
                        else
                        {
                            //Debug.Log("VAS is null!\n" + new StackTrace().ToString()); // # VAS null case to look at in the future.
                            MultiplayerMode.AddVirtualAudioSourceToAudioSource(ref originalAudioSource);
                            virtualAudioSource = originalAudioSource.gameObject.GetComponent<VirtualAudioSource>();
                            if (virtualAudioSource != null)
                            {
                                virtualAudioSource.Stop();
                            }
                            else if (ModSettings.logDebugText)
                            {
                                Debug.Log("VAS is still null!\n" + new StackTrace().ToString());
                            }
                        }
                    }
                }

                if (ModSettings.useSparky && ((MState)mChasingState).monster.monsterType.Equals("Sparky") && !((MState)mChasingState).monster.PreviousWasClimb && !((MState)mChasingState).monster.PreviousWasDestroy)
                {
                    if (!ModSettings.giveAllMonstersAFiendAura)
                    {
                        ((MState)mChasingState).monster.GetComponent<FiendAura>().enabled = true;
                        FiendLightDisruptor fiendLightDisruptor = ((MState)mChasingState).monster.GetComponent<FiendLightDisruptor>();
                        fiendLightDisruptor.enabled = true;
                        TimeScaleManager.Instance.StartCoroutine(SparkyMode.UpdateFiendDisruptorAfterAFrame(fiendLightDisruptor));
                    }
                    //Debug.Log("Triggering EMP from chase");
                    ((MState)mChasingState).monster.GetComponent<SparkyAura>().EMP(true); // Trigger an EMP when Sparky enters a chase.
                }

                orig.Invoke(mChasingState);

                /*
                if (ModSettings.letAllMonstersLockDoors && ((MState)mChasingState).monster.MonsterType != Monster.MonsterTypeEnum.Fiend)
                {
                    mChasingState.sinceDoorCheck.StartTimer();
                    mChasingState.DoDoorCheck(true);
                }
                */

                if (ModSettings.enableMultiplayer)
                {
                    if (((MState)mChasingState).monster.CanSeePlayerNotHiding || ((MState)mChasingState).monster.FoundPlayerBySound)
                    {
                        ((MState)mChasingState).monster.LastSeenPlayerPosition = ((MState)mChasingState).monster.player.transform.position;
                    }
                }

                lastMonsterSeen = ((MState)mChasingState).monster;
            }

            private static void HookMChasingStateOnUpdate(On.MChasingState.orig_OnUpdate orig, MChasingState mChasingState)
            {
                if (!ModSettings.useMonsterUpdateGroups || IsMonsterInActiveGroup(mChasingState.monster))
                {
                    orig.Invoke(mChasingState);
                }
            }

            private static void HookMChasingStateOnExit(On.MChasingState.orig_OnExit orig, MChasingState mChasingState)
            {
                if (ModSettings.useSparky && ((MState)mChasingState).monster.monsterType.Equals("Sparky") && !((MState)mChasingState).monster.MoveControl.shouldClimb && !((MState)mChasingState).monster.IsMonsterDestroying)
                {
                    // Disable the fiend components if applicable and reset Sparky's state.
                    if (!ModSettings.giveAllMonstersAFiendAura)
                    {
                        FiendAura fiendAura = ((MState)mChasingState).monster.GetComponent<FiendAura>();
                        FiendLightDisruptor fiendLightDisruptor = ((MState)mChasingState).monster.GetComponent<FiendLightDisruptor>();
                        TimeScaleManager.Instance.StartCoroutine(SparkyMode.DisableFiendAuraAfterAFrame(fiendAura, fiendLightDisruptor));
                    }
                }

                orig.Invoke(mChasingState);

                if (ModSettings.enableMultiplayer && !((MState)mChasingState).monster.MoveControl.shouldClimb && !((MState)mChasingState).monster.IsMonsterDestroying)
                {
                    MultiplayerMode.ChanceToChooseNewPlayer(((MState)mChasingState).monster);
                }
            }

            // Unity3d cSharp - Vector3 as default parameter - Programmer - https://stackoverflow.com/questions/30294216/unity3d-c-sharp-vector3-as-default-parameter - Accessed 25.08.2022
            public static IEnumerator SwitchMonster(Monster monsterToDisable, Vector3? spawnPosition = null)
            {
                // Let any other after chase code finish.
                while (monsterToDisable.GetComponent<FSM>().Current.typeofState == FSMState.StateTypes.Chase)
                {
                    yield return null;
                }
                yield return null;
                yield return null;
                int monsterNumberToDisable = MonsterNumber(monsterToDisable.GetInstanceID());
                int monsterNumberToEnable = 0;
                bool foundMonster = false;
                // Try getting a random monster to switch in first.
                for (int i = 0; i < 5 && !foundMonster; i++)
                {
                    int randomMonsterNumber = UnityEngine.Random.Range(0, monsterList.Count);
                    if (!monsterList[randomMonsterNumber].activeInHierarchy)
                    {
                        foundMonster = true;
                        monsterNumberToEnable = randomMonsterNumber;
                    }
                }

                // If the random search failed to find a disabled monster, get the next disabled monster.
                for (int i = monsterNumberToDisable + 1; i != monsterNumberToDisable && !foundMonster; i++)
                {
                    if (i == monsterList.Count)
                    {
                        i = 0;
                    }
                    if (!monsterList[i].activeInHierarchy)
                    {
                        foundMonster = true;
                        monsterNumberToEnable = i;
                    }
                }

                if (foundMonster)
                {
                    Debug.Log("Switched monster number " + monsterNumberToDisable + " with " + monsterNumberToEnable + "! Number of monsters = " + ModSettings.numberOfMonsters + ", number of alternating monsters = " + ModSettings.numberOfAlternatingMonsters);
                    if (monsterListMonsterComponents[monsterNumberToEnable].MonsterType != Monster.MonsterTypeEnum.Hunter)
                    {
                        bool spawnedAtCustomSpawn = false;
                        if (spawnPosition != null)
                        {
                            Collider[] colliders;
                            if (monsterToDisable.MonsterType == Monster.MonsterTypeEnum.Hunter)
                            {
                                colliders = Physics.OverlapBox((Vector3)spawnPosition, new Vector3(2f, 0.5f, 2f));
                            }
                            else
                            {
                                colliders = Physics.OverlapBox(monsterToDisable.transform.position, new Vector3(2f, 0.5f, 2f));
                            }
                            for (int i = 0; i < colliders.Length; i++)
                            {
                                Room nearbyRoom = colliders[i].GetComponentInChildren<Room>();
                                if (nearbyRoom != null)
                                {
                                    spawnedAtCustomSpawn = true;
                                    monsterList[monsterNumberToEnable].transform.position = nearbyRoom.RoomBounds.center;
                                    break;
                                }
                            }
                        }
                        if (!spawnedAtCustomSpawn)
                        {
                            monsterList[monsterNumberToEnable].transform.position = monsterToDisable.transform.position;
                        }
                    }
                    monsterList[monsterNumberToEnable].SetActive(true);
                    if (monsterListMonsterComponents[monsterNumberToEnable].MonsterType == Monster.MonsterTypeEnum.Fiend || ModSettings.giveAllMonstersAFiendAura)
                    {
                        FiendAura fiendAura = monsterListMonsterComponents[monsterNumberToEnable].gameObject.GetComponentInChildren<FiendAura>();
                        FiendLightDisruptor fiendLightDisruptor = monsterListMonsterComponents[monsterNumberToEnable].gameObject.GetComponentInChildren<FiendLightDisruptor>();

                        if (fiendAura != null && fiendLightDisruptor != null)
                        {
                            fiendAura.enabled = true;
                            fiendLightDisruptor.enabled = true;
                            yield return SparkyMode.UpdateFiendDisruptorAfterAFrame(fiendLightDisruptor);
                        }
                        else
                        {
                            Debug.Log("Fiend Light Disruptor or Fiend Aura could not be found 1.");
                        }
                    }
                    if (monsterToDisable.MonsterType == Monster.MonsterTypeEnum.Fiend || ModSettings.giveAllMonstersAFiendAura)
                    {
                        FiendAura fiendAura = monsterToDisable.gameObject.GetComponentInChildren<FiendAura>();
                        FiendLightDisruptor fiendLightDisruptor = monsterToDisable.gameObject.GetComponentInChildren<FiendLightDisruptor>();
                        if (fiendAura != null && fiendLightDisruptor != null)
                        {
                            if (fiendLightDisruptor.enabled)
                            {
                                yield return SparkyMode.DisableFiendAuraAfterAFrame(fiendAura, fiendLightDisruptor);
                            }
                            else
                            {
                                Debug.Log("Fiend Light Disruptor is disabled.");
                            }
                        }
                        else
                        {
                            Debug.Log("Fiend Light Disruptor or Fiend Aura could not be found 2.");
                        }
                    }
                    monsterList[monsterNumberToDisable].transform.position = Vector3.zero;
                    monsterList[monsterNumberToDisable].SetActive(false);
                }
                else
                {
                    Debug.Log("Could not find monster to switch! Number of monsters = " + ModSettings.numberOfMonsters + ", number of alternating monsters = " + ModSettings.numberOfAlternatingMonsters);
                }
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MClimbingState

            private static void HookMClimbingState()
            {
                new Hook(typeof(MClimbingState).GetNestedType("<LerpTo>c__Iterator0", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.ManyMonstersMode).GetMethod("HookMClimbingStateLerpToIntermediateHook"), null);

                // Moved to BaseFeatures to fix a non-reversal bug affecting music in Persistent Monster mode.
                //On.MClimbingState.OnEnter += new On.MClimbingState.hook_OnEnter(HookMClimbingStateOnEnter);
                //On.MClimbingState.FinishClimb += new On.MClimbingState.hook_FinishClimb(HookMClimbingStateFinishClimb);
            }

            public static bool HookMClimbingStateLerpToIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookMClimbingStateLerpTo((MClimbingState)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookMClimbingStateLerpTo(MClimbingState mClimbingState)
            {
                mClimbingState.t = 0f;
                Vector3 startPosition = ((MState)mClimbingState).transform.position;
                Quaternion startRotation = ((MState)mClimbingState).monster.transform.rotation;
                Vector3 normal = mClimbingState.FaceThis - mClimbingState.LerpPosition;
                normal.y = 0f;
                normal.Normalize();
                Quaternion targetRotation = Quaternion.LookRotation(normal, Vector3.up);
                while (mClimbingState.t < 1.5f)
                {
                    mClimbingState.t += Time.deltaTime;
                    ((MState)mClimbingState).transform.rotation = Quaternion.Lerp(startRotation, targetRotation, mClimbingState.t);
                    ((MState)mClimbingState).transform.position = Vector3.Lerp(startPosition, mClimbingState.LerpPosition, mClimbingState.t);
                    yield return null;
                }
                ((MState)mClimbingState).transform.rotation = targetRotation;
                ((MState)mClimbingState).transform.position = mClimbingState.LerpPosition;
                ((MState)mClimbingState).monster.MoveControl.GetAniControl.climbUp = true;
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MDestroyState

            public static bool HookMDestroyStateIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookMDestroyStateLerpToDoor((MDestroyState)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookMDestroyStateLerpToDoor(MDestroyState mDestroyState)
            {
                float t = 0f;
                Quaternion startRotation = ((MState)mDestroyState).monster.transform.rotation;
                Vector3 startPosition = ((MState)mDestroyState).monster.transform.position;
                GameObject target = null;
                float lerpSpeed = 3f;
                if (((MState)mDestroyState).monster.CurrentDoor != null)
                {
                    GameObject gameObject = ClosestSide(((MState)mDestroyState).monster, ((MState)mDestroyState).monster.CurrentDoor);
                    if (gameObject != null)
                    {
                        /*
                        Monster Destroy Child number 0 is Brute (UnityEngine.Transform)
                        Monster Destroy Child number 1 is Fiend (UnityEngine.Transform)
                        Monster Destroy Child number 2 is Hunter (UnityEngine.Transform)
                        */

                        Monster.MonsterTypeEnum monsterType = ((MState)mDestroyState).monster.MonsterType;
                        if (monsterType != Monster.MonsterTypeEnum.Brute)
                        {
                            if (monsterType != Monster.MonsterTypeEnum.Hunter)
                            {
                                if (monsterType == Monster.MonsterTypeEnum.Fiend)
                                {
                                    target = gameObject.transform.FindChild("Fiend").gameObject;
                                }
                            }
                            else
                            {
                                target = gameObject.transform.FindChild("Hunter").gameObject;
                            }
                        }
                        else
                        {
                            target = gameObject.transform.FindChild("Brute").gameObject;
                        }
                    }
                }
                if (target != null)
                {
                    Vector3 targetPosition = target.transform.position;
                    Vector3 normal = target.transform.forward;
                    normal.y = 0f;
                    normal.Normalize();
                    Quaternion targetRotation = Quaternion.LookRotation(normal, Vector3.up);
                    while (t < 1f)
                    {
                        t += Time.deltaTime * lerpSpeed;
                        Debug.DrawRay(startPosition, normal, Color.white, 0.1f);
                        ((MState)mDestroyState).transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                        ((MState)mDestroyState).transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                        yield return null;
                    }
                    ((MState)mDestroyState).transform.position = targetPosition;
                    ((MState)mDestroyState).transform.rotation = targetRotation;
                }
                else
                {
                    mDestroyState.LeaveState();
                    ((MState)mDestroyState).monster.InDestroyState = false;
                }
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MetalDoorBreak

            private static void HookMetalDoorBreak(On.MetalDoorBreak.orig_OnDoorDestroy orig, MetalDoorBreak metalDoorBreak)
            {
                metalDoorBreak.doorRigidbody.constraints = RigidbodyConstraints.None;
                metalDoorBreak.doorRigidbody.AddForce(monsterList[ClosestMonsterToThis(metalDoorBreak.doorRigidbody.transform.position)].transform.forward, ForceMode.Impulse);
                metalDoorBreak.doorRigidbody.transform.parent.parent = null;
                ((MonoBehaviour)metalDoorBreak).Invoke("DestroyAfterTime", 5f);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MHideState

            private static void HookMHideState()
            {
                if (!ModSettings.enableCrewVSMonsterMode)
                {
                    On.MHideState.FinishedHiding += new On.MHideState.hook_FinishedHiding(HookMHideStateFinishedHiding); // Alternating Monsters Mode
                }
                new Hook(typeof(MHideState).GetNestedType("<MoveTowardsHidingPlace>c__Iterator0", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.ManyMonstersMode).GetMethod("HookMHideStateMoveTowardsHidingPlaceIntermediateHook"), null);
                On.MHideState.StateChanges += new On.MHideState.hook_StateChanges(HookMHideStateStateChanges);
                On.MHideState.OnEnter += new On.MHideState.hook_OnEnter(HookMHideStateOnEnter);
                On.MHideState.OnUpdate += new On.MHideState.hook_OnUpdate(HookMHideStateOnUpdate);
            }

            private static void HookMHideStateFinishedHiding(On.MHideState.orig_FinishedHiding orig, MHideState mHideState)
            {
                if (ModSettings.alternatingMonstersMode && ModSettings.numberOfMonsters > ModSettings.numberOfAlternatingMonsters)
                {
                    TimeScaleManager.Instance.StartCoroutine(SwitchMonster(((MState)mHideState).monster, mHideState.trapToReturn.HideFromHere.position));
                }
                orig.Invoke(mHideState);
            }

            public static bool HookMHideStateMoveTowardsHidingPlaceIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookMHideStateMoveTowardsHidingPlace((MHideState)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookMHideStateMoveTowardsHidingPlace(MHideState mHideState)
            {
                Vector3 MONPOS = ((MState)mHideState).monster.transform.position;
                Vector3 HIDEPOS = mHideState.trapToReturn.HideFromHere.position;
                Quaternion StartRot = ((MState)mHideState).monster.transform.rotation;
                Vector3 faceThis = new Vector3(mHideState.trapToReturn.transform.position.x, MONPOS.y, mHideState.trapToReturn.transform.position.z);
                Vector3 look = faceThis - HIDEPOS;
                Quaternion targetRotation = Quaternion.LookRotation(look, Vector3.up);
                float t = 0f;
                while (t < 1f)
                {
                    t += Time.deltaTime * 0.5f;
                    MONPOS.x = Mathf.Lerp(MONPOS.x, HIDEPOS.x, t);
                    MONPOS.z = Mathf.Lerp(MONPOS.z, HIDEPOS.z, t);
                    ((MState)mHideState).monster.transform.rotation = Quaternion.Lerp(StartRot, targetRotation, t);
                    ((MState)mHideState).monster.transform.position = new Vector3(MONPOS.x, ((MState)mHideState).monster.transform.position.y, MONPOS.z);
                    yield return null;
                }
                ((MState)mHideState).monster.transform.position = new Vector3(HIDEPOS.x, ((MState)mHideState).monster.transform.position.y, HIDEPOS.z);
                ((MState)mHideState).monster.transform.rotation = targetRotation;
                mHideState.GoIntoHiding();
                yield break;
            }

            private static void HookMHideStateStateChanges(On.MHideState.orig_StateChanges orig, MHideState mHideState)
            {
                if (mHideState.hideStarted)
                {
                    mHideState.hideStartedTimer += Time.deltaTime;
                    if (mHideState.hideStartedTimer > 5f)
                    {
                        mHideState.hideStarted = false;
                        mHideState.hideStartedTimer = 0f;
                    }
                }
                else
                {
                    mHideState.hideStartedTimer = 0f;
                    if (!((MState)mHideState).monster.InSameRegion())
                    {
                        mHideState.diffRegionTime += Time.deltaTime;
                    }
                    else
                    {
                        mHideState.diffRegionTime = 0f;
                    }
                    if (((MState)mHideState).monster.SubEventBeenStarted())
                    {
                        mHideState.hideAttemptTime.StopTimer();
                        ((MState)mHideState).SendEvent("EventStarted");
                    }
                    else if ((mHideState.hideAttemptTime.TimeElapsed > 60f || mHideState.diffRegionTime > 15f) && !mHideState.hideStarted)
                    {
                        if (mHideState.timeOutOfVision > 3f)
                        {
                            mHideState.GoIntoHiding();
                        }
                    }
                    else if (((MState)mHideState).monster.MoveControl.shouldClimb)
                    {
                        ((MState)mHideState).SendEvent("Climb");
                    }
                    else if (((MState)mHideState).monster.IsMonsterDestroying)
                    {
                        ((MState)mHideState).SendEvent("Destroy");
                    }
                    else if (!((MState)mHideState).monster.IsMonsterRetreating)
                    {
                        if (((MState)mHideState).monster.CanSeePlayerNotHiding)
                        {
                            mHideState.hideAttemptTime.StopTimer();
                            ((MState)mHideState).SendEvent("Chase");
                        }
                        if (((MState)mHideState).monster.CanSensePlayerNear || (((MState)mHideState).monster.CanHearNoise && !IsBelowThreshold(((MState)mHideState).monster) && ((MState)mHideState).monster.DistanceToPlayer < 17f))
                        {
                            mHideState.MAXSPEED = 0f;
                            mHideState.hideAttemptTime.StopTimer();
                            ((MState)mHideState).SendEvent("Search");
                        }
                    }
                }
                Vector3 monsterPosition = new Vector3(((MState)mHideState).transform.position.x, 0f, ((MState)mHideState).transform.position.z);
                Vector3 hidingPlacePosition = new Vector3(mHideState.trapToReturn.HideFromHere.position.x, 0f, mHideState.trapToReturn.HideFromHere.position.z);
                float distanceToHidingPlace = Vector3.Distance(monsterPosition, hidingPlacePosition);
                float heightDifference = Mathf.Abs(((MState)mHideState).transform.position.y - mHideState.trapToReturn.HideFromHere.position.y);
                if (distanceToHidingPlace < 1f && heightDifference < 1.5f && !mHideState.hideStarted)
                {
                    mHideState.MAXSPEED = 0f;
                    mHideState.hideStarted = true;
                    if (logHunterActions)
                    {
                        Debug.Log("Moving towards hiding place");
                    }
                    ((MState)mHideState).StartCoroutine(mHideState.MoveTowardsHidingPlace());
                }
                ((MState)mHideState).monster.MoveControl.MaxSpeed = mHideState.MAXSPEED;
            }

            private static void HookMHideStateOnEnter(On.MHideState.orig_OnEnter orig, MHideState mHideState)
            {
                if (!mHideState.hideAttemptTime.IsRunning)
                {
                    mHideState.hideAttemptTime.ResetTimer();
                    mHideState.hideAttemptTime.StartTimer();
                }
                mHideState.timeOutOfVision = 0f;
                if (((MState)mHideState).monster.IsMonsterRetreating)
                {
                    mHideState.MAXSPEED = 100f;
                }
                else
                {
                    mHideState.MAXSPEED = 30f;
                }
                mHideState.hideTypeFound = -1f;
                mHideState.typeofState = FSMState.StateTypes.LowAlert;
                ((MState)mHideState).monster.MoveControl.enabled = true;
                ((MState)mHideState).monster.BeenBlinded = false;
                mHideState.trapState = ((MState)mHideState).GetComponent<MTrappingState>();
                mHideState.huntState = ((MState)mHideState).GetComponent<MHuntingState>();
                mHideState.hideStarted = false;
                mHideState.hideStartedTimer = 0f;
                if (!((MState)mHideState).monster.PreviousWasClimb)
                {
                    ((MState)mHideState).monster.MoveControl.CancelPath();
                    mHideState.trapToReturn = FindAmbush(((MState)mHideState).transform.position, true, ((MState)mHideState).monster);
                    Vector3 goal = mHideState.trapToReturn.HideFromHere.transform.position + Vector3.up;
                    ((MState)mHideState).monster.SetGoal(goal);
                }
            }

            private static void HookMHideStateOnUpdate(On.MHideState.orig_OnUpdate orig, MHideState mHideState)
            {
                if (!ModSettings.useMonsterUpdateGroups || IsMonsterInActiveGroup(mHideState.monster))
                {
                    if (!PlayerToMonsterCast(((MState)mHideState).monster))
                    {
                        mHideState.timeOutOfVision += Time.deltaTime;
                    }
                    else
                    {
                        mHideState.timeOutOfVision = 0f;
                    }
                    if (mHideState.trapToReturn != null && mHideState.hideTypeFound == -1f)
                    {
                        if (mHideState.trapToReturn.trapType == AmbushPoint.TrapType.Vent)
                        {
                            mHideState.hideTypeFound = 0f;
                        }
                        else if (mHideState.trapToReturn.trapType == AmbushPoint.TrapType.Ceiling)
                        {
                            mHideState.hideTypeFound = 1f;
                        }
                        else if (mHideState.trapToReturn.trapType == AmbushPoint.TrapType.EnginePipe)
                        {
                            mHideState.hideTypeFound = 2f;
                        }
                        else if (mHideState.trapToReturn.trapType == AmbushPoint.TrapType.Deck)
                        {
                            mHideState.hideTypeFound = 3f;
                        }
                        ((MState)mHideState).monster.HunterAnimations.HideType = mHideState.hideTypeFound;
                    }
                    if (mHideState.trapToReturn != null)
                    {
                        Vector3 vector = mHideState.trapToReturn.HideFromHere.transform.position + Vector3.up;
                        if (((MState)mHideState).monster.MoveControl.Goal != vector && (((MState)mHideState).monster.Pathfinding.IsComplete || ((MState)mHideState).monster.MoveControl.Goal == Vector3.zero))
                        {
                            ((MState)mHideState).monster.SetGoal(vector);
                        }
                        else
                        {
                            mHideState.StateChanges();
                        }
                    }
                    else
                    {
                        ((MState)mHideState).monster.MoveControl.CancelPath();
                        mHideState.trapToReturn = FindAmbush(((MState)mHideState).transform.position, true, ((MState)mHideState).monster);
                        mHideState.StateChanges();
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MHuntingState

            private static void HookMHuntingState()
            {
                On.MHuntingState.OnUpdate += new On.MHuntingState.hook_OnUpdate(HookMHuntingStateOnUpdate);
                On.MHuntingState.UpdateSoundStuff += new On.MHuntingState.hook_UpdateSoundStuff(HookMHuntingStateUpdateSoundStuff);
            }

            private static void HookMHuntingStateOnUpdate(On.MHuntingState.orig_OnUpdate orig, MHuntingState mHuntingState)
            {
                if (!ModSettings.useMonsterUpdateGroups || IsMonsterInActiveGroup(mHuntingState.monster))
                {
                    orig.Invoke(mHuntingState);

                    if (ModSettings.enableMultiplayer)
                    {
                        if (((MState)mHuntingState).TimeInState > 3f)
                        {
                            ((MState)mHuntingState).monster.GetEars.MoveTo(((MState)mHuntingState).monster.player.transform.position);
                        }
                    }
                }
            }

            private static void HookMHuntingStateUpdateSoundStuff(On.MHuntingState.orig_UpdateSoundStuff orig, MHuntingState mHuntingState)
            {
                if (((MState)mHuntingState).monster.SoundAlert == 100f)
                {
                    if (!mHuntingState.targetSetForAmbush)
                    {
                        AmbushPoint ambushPoint = FindAmbush(((MState)mHuntingState).monster.Hearing.PointOfInterest, false, ((MState)mHuntingState).monster);
                        if (ambushPoint != null)
                        {
                            mHuntingState.SetTargetTo(ambushPoint.transform.position);
                        }
                    }
                    mHuntingState.timeAtMaxSoundAlert += Time.deltaTime;
                    if (mHuntingState.HeardHighAlert() || (mHuntingState.CoolDownDone() && (((MState)mHuntingState).monster.PlayerDetectRoom.GetRoomCategory == RoomCategory.Outside || !mHuntingState.IsPlayerInANormalRoom())))
                    {
                        mHuntingState.moveType = MHuntingState.MoveType.ToDistraction;
                        if (mHuntingState.timeAtMaxSoundAlert > 3f)
                        {
                            mHuntingState.targetSetForAmbush = true;
                            mHuntingState.ambushing = true;
                        }
                    }
                }
                else
                {
                    mHuntingState.moveType = MHuntingState.MoveType.AroundPlayer;
                    if (((MState)mHuntingState).monster.PlayerDetectRoom.GetRoomCategory == RoomCategory.Outside)
                    {
                        mHuntingState.moveType = MHuntingState.MoveType.AroundPlayer;
                        mHuntingState.shipPos = MHuntingState.ShipPosition.Outside;
                        Ducking.SetValues("Monster", 2.5f, 3f);
                    }
                    else
                    {
                        mHuntingState.moveType = MHuntingState.MoveType.AroundPlayer;
                        Ducking.SetValues("Monster", 1f, 1f);
                        mHuntingState.shipPos = MHuntingState.ShipPosition.CrewDeck;
                    }
                    mHuntingState.targetSetForAmbush = false;
                    mHuntingState.timeAtMaxSoundAlert = 0f;
                }
                mHuntingState.MoveSource();
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MIdleState

            private static void HookMIdleState(On.MIdleState.orig_OnUpdate orig, MIdleState mIdleState)
            {
                if (!ModSettings.useMonsterUpdateGroups || IsMonsterInActiveGroup(mIdleState.monster))
                {
                    ((Action)Activator.CreateInstance(typeof(Action), mIdleState, typeof(MState).GetMethod("OnUpdate").MethodHandle.GetFunctionPointer()))();
                    mIdleState.timer += Time.deltaTime;
                    if (((MState)mIdleState).monster != null && ((MState)mIdleState).monster.IsMonsterActive)
                    {
                        ((MState)mIdleState).monster.MoveControl.MaxSpeed = 0f;
                        if (((MState)mIdleState).monster.SubEventBeenStarted())
                        {
                            ((MState)mIdleState).SendEvent("EventStarted");
                        }
                        else if (((MState)mIdleState).monster.GetAlertMeters.mSightAlert > 90f && (((MState)mIdleState).monster.PersistChase || ((MState)mIdleState).monster.IsPlayerLocationKnown))
                        {
                            if (!ModSettings.useSparky)
                            {
                                ((MState)mIdleState).SendEvent("Chase");
                            }
                            else
                            {
                                if (!((MState)mIdleState).monster.monsterType.Equals("Sparky"))
                                {
                                    ((MState)mIdleState).SendEvent("Chase");
                                }
                                else
                                {
                                    ((MState)mIdleState).SendEvent("Lurk");
                                }
                            }
                        }
                        else if (((MState)mIdleState).monster.ShouldSearchRoom && ((MState)mIdleState).monster.MoveControl.GetAniControl.InRoom && ((MState)mIdleState).monster.RoomDetect.CurrentRoom.ValidHidingSpots.Count > 0)
                        {
                            ((MState)mIdleState).SendEvent("SearchRoom");
                        }
                        else if (mIdleState.timer > mIdleState.timeBeforeStart)
                        {
                            if (!IsBelowThreshold(((MState)mIdleState).monster) && ((MState)mIdleState).monster.DistanceToPlayer <= 17f)
                            {
                                if (((MState)mIdleState).monster.OnHighChase)
                                {
                                    ((MState)mIdleState).SendEvent("Search");
                                }
                                else if (((MState)mIdleState).monster.CanSensePlayerNear)
                                {
                                    ((MState)mIdleState).SendEvent("Search");
                                }
                                else if (((MState)mIdleState).monster.CanHearNoise)
                                {
                                    ((MState)mIdleState).SendEvent("Search");
                                }
                                else
                                {
                                    ((MState)mIdleState).SendEvent("StartWander");
                                }
                            }
                            else
                            {
                                ((MState)mIdleState).SendEvent("StartWander");
                            }
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MInvestigateState - Not Being Used

            /*
            private static void HookMInvestigateState(On.MInvestigateState.orig_OnUpdate orig, MInvestigateState mInvestigateState)
            {

            }
            */

            /*----------------------------------------------------------------------------------------------------*/
            // @ModSettings (Custom Calculations)

            public static int ClosestMonsterToPlayer()
            {
                int closestMonster = 0;
                float closestDistance = PlayerToMonsterDistance(monsterListMonsterComponents[0]);
                for (int i = 1; i < monsterList.Count; i++)
                {
                    if (PlayerToMonsterDistance(monsterListMonsterComponents[i]) < closestDistance)
                    {
                        closestDistance = PlayerToMonsterDistance(monsterListMonsterComponents[i]);
                        closestMonster = i;
                    }
                }
                return closestMonster;
            }

            public static int ClosestMonsterToThis(Vector3 passedPosition)
            {
                int closestMonster = 0;
                float closestDistance = Vector3.Distance(monsterList[0].transform.position, passedPosition);
                for (int i = 1; i < monsterList.Count; i++)
                {
                    if (Vector3.Distance(monsterList[i].transform.position, passedPosition) < closestDistance)
                    {
                        closestDistance = Vector3.Distance(monsterList[i].transform.position, passedPosition);
                        closestMonster = i;
                    }
                }
                return closestMonster;
            }

            /*
            private static int ClosestMonsterToThis(Vector3 passedPosition, Monster.MonsterTypeEnum monsterType)
            {
                int[] monsterNumbersOfGivenType = new int[monsterList.Count];
                int numberOfMonstersOfGivenType = 0;
                for (int i = 0; i < monsterList.Count; i++)
                {
                    if (monsterListMonsterComponents[i].MonsterType == monsterType)
                    {
                        monsterNumbersOfGivenType[numberOfMonstersOfGivenType] = i;
                        numberOfMonstersOfGivenType++;
                    }
                }

                int closestMonster = monsterNumbersOfGivenType[0];
                float closestDistance = Vector3.Distance(monsterList[monsterNumbersOfGivenType[0]].transform.position, passedPosition);
                for (int i = 1; i < monsterNumbersOfGivenType.Length; i++)
                {
                    if (Vector3.Distance(monsterList[i].transform.position, passedPosition) < closestDistance)
                    {
                        closestDistance = Vector3.Distance(monsterList[i].transform.position, passedPosition);
                        closestMonster = i;
                    }
                }
                return closestMonster;
            }
            */

            public static int MonsterNumber(int passedMonsterInstanceID)
            {
                if (monsterList != null)
                {
                    if (monsterInstanceIDtoMonsterNumberDict.ContainsKey(passedMonsterInstanceID))
                    {
                        return monsterInstanceIDtoMonsterNumberDict[passedMonsterInstanceID];
                    }
                    else
                    {
                        Debug.Log("monsterInstanceIDs does not contain passed ID!");
                    }
                }
                else
                {
                    Debug.Log("monsterList is not initialised (written from MonsterNumber())");
                }
                return 0; // Default return.
            }

            /*
            private static int MonsterNumber(int monsterInstanceIDToUse, int[] passedMonsterInstanceIDs)
            {
                int monsterNumber = 0;
                if (passedMonsterInstanceIDs != null)
                {
                    for (int i = 1; i < passedMonsterInstanceIDs.Length; i++)
                    {
                        if (monsterInstanceIDs[i] == monsterInstanceIDToUse)
                        {
                            monsterNumber = i;
                        }
                    }
                }
                else
                {
                    Debug.Log("The passed instance IDs list is not initialised (written from MonsterNumber())");
                }

                return monsterNumber;
            }
            */

            private static int HunterNumber(int passedHunterInstanceID)
            {
                if (hunters != null)
                {
                    if (huntersInstanceIDs.ContainsKey(passedHunterInstanceID))
                    {
                        return huntersInstanceIDs[passedHunterInstanceID];
                    }
                    else
                    {
                        Debug.Log("huntersInstanceIDs does not contain passed ID!");
                    }
                }
                else
                {
                    Debug.Log("hunters is not initialised (written from HunterNumber())");
                }
                return 0;
            }

            private static bool IsMonsterInRoom(Monster monster, Room room)
            {
                if (monster.RoomDetect.CurrentRoom == room)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MonstDetectRoom

            private static void HookMonstDetectRoom()
            {
                On.MonstDetectRoom.NearDoorCheck += new On.MonstDetectRoom.hook_NearDoorCheck(HookMonstDetectRoomNearDoorCheck);
                On.MonstDetectRoom.Start += new On.MonstDetectRoom.hook_Start(HookMonstDetectRoomStart);
                On.MonstDetectRoom.Update += new On.MonstDetectRoom.hook_Update(HookMonstDetectRoomUpdate);
            }

            private static void HookMonstDetectRoomNearDoorCheck(On.MonstDetectRoom.orig_NearDoorCheck orig, MonstDetectRoom monstDetectRoom)
            {
                if (monstDetectRoom.CurrentDoor != null && monstDetectRoom.CurrentDoor.BeenOpenFor > 5f && monstDetectRoom.CurrentDoor.monsterClose && monstDetectRoom.CurrentDoor.DoorType != Door.doorType.Sealed && monstDetectRoom.CurrentDoor.DoorType != Door.doorType.Powered)
                {
                    BlastOffDoor(monstDetectRoom.monster.gameObject, monstDetectRoom.CurrentDoor, 50f);
                }
            }

            private static void HookMonstDetectRoomStart(On.MonstDetectRoom.orig_Start orig, MonstDetectRoom monstDetectRoom)
            {
                if (monstDetectRoom.monster == null)
                {
                    monstDetectRoom.monster = ((MonoBehaviour)monstDetectRoom).GetComponentInParent<Monster>();
                    monstDetectRoom.stairzonesWithin = new List<Collider>();
                    monstDetectRoom.atDoorTimer = new Timer();
                    monstDetectRoom.atDoorTimer.ResetTimer();
                }
            }

            private static void HookMonstDetectRoomUpdate(On.MonstDetectRoom.orig_Update orig, MonstDetectRoom monstDetectRoom)
            {
                if (monstDetectRoom.monster == null)
                {
                    monstDetectRoom.monster = ((MonoBehaviour)monstDetectRoom).GetComponentInParent<Monster>();
                }
                orig.Invoke(monstDetectRoom);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Monster

            private static void HookMonster()
            {
                On.Monster.DoorAI += new On.Monster.hook_DoorAI(HookMonsterDoorAI);
                On.Monster.IsPlayerInAttackRange += new On.Monster.hook_IsPlayerInAttackRange(HookMonsterIsPlayerInAttackRange);
                On.Monster.SightAI += new On.Monster.hook_SightAI(HookMonsterSightAI);
                On.Monster.Update += new On.Monster.hook_Update(HookMonsterUpdate);
            }

            private static void HookMonsterDoorAI(On.Monster.orig_DoorAI orig, Monster monster)
            {
                if (monster.CurrentDoor != null)
                {
                    bool isOpen = monster.CurrentDoor.isOpen;
                    bool isInside = false;
                    bool isLocked = false;
                    bool isPowerDoorInteractAllowed = true;
                    monster.MoveControl.GetAniControl.doorType = (int)monster.CurrentDoor.DoorType;
                    if (monster.CurrentDoor.DoorType == Door.doorType.Powered)
                    {
                        isPowerDoorInteractAllowed = monster.CurrentDoor.PowerDoorInteractAllowed();
                    }
                    if (isPowerDoorInteractAllowed && !isOpen && monster.monsterRoomDetect.CurrentDoor.attached && monster.MoveControl.MaxSpeed > 0f && monster.CurrentDoor.monsterClose && monster.CurrentDoor.GetComponentInChildren<HidingSpot>() == null && !monster.InDestroyState)
                    {
                        if (monster.CurrentDoor.IsDoorLocked)
                        {
                            isLocked = true;
                        }
                        if (ClosestSide(monster, monster.CurrentDoor) != null && ClosestSide(monster, monster.CurrentDoor).transform.name == "Inside")
                        {
                            isInside = true;
                        }
                        if (monster.CurrentDoor.allowDestroy)
                        {
                            monster.MoveControl.GetAniControl.doorAnimation = MonsterDoorAnimation.Combine(isLocked, isInside);
                            monster.IsMonsterDestroying = true;
                        }
                        else
                        {
                            monster.MoveControl.GetAniControl.doorAnimation = MonsterDoorAnimation.Combine(isLocked, isInside);
                            monster.IsMonsterDestroying = false;
                        }
                    }
                }
            }

            private static bool HookMonsterIsPlayerInAttackRange(On.Monster.orig_IsPlayerInAttackRange orig, Monster monster)
            {
                if (monster.player == null)
                {
                    return false;
                }
                float num = 0f;
                if (monster.IsPlayerHiding)
                {
                    num = 1f;
                    Room getRoom = monster.PlayerDetectRoom.GetRoom;
                    if (getRoom != null)
                    {
                        HidingSpot hidingSpot = monster.PlayerDetectRoom.PlayerHidingSpot(getRoom.HidingSpots);
                        if (hidingSpot != null)
                        {
                            float num2 = Vector3.Distance(hidingSpot.MonsterPoint, ((MonoBehaviour)monster).transform.position);
                            if (num2 < num)
                            {
                                return true;
                            }
                        }
                    }
                }
                else
                {
                    Monster.MonsterTypeEnum monsterTypeEnum = monster.MonsterType;
                    if (monsterTypeEnum != Monster.MonsterTypeEnum.Brute)
                    {
                        if (monsterTypeEnum != Monster.MonsterTypeEnum.Hunter)
                        {
                            if (monsterTypeEnum == Monster.MonsterTypeEnum.Fiend)
                            {
                                if (monster.player.GetComponent<NewPlayerClass>().IsProne())
                                {
                                    num = 3f;
                                }
                                else if (monster.player.GetComponent<NewPlayerClass>().IsCrouched())
                                {
                                    num = 2f;
                                }
                                else
                                {
                                    num = 2f;
                                }
                            }
                        }
                        else if (monster.player.GetComponent<NewPlayerClass>().IsProne())
                        {
                            num = 1.5f;
                        }
                        else if (monster.player.GetComponent<NewPlayerClass>().IsCrouched())
                        {
                            num = 2f;
                        }
                        else
                        {
                            num = 2.8f;
                        }
                    }
                    else if (monster.player.GetComponent<NewPlayerClass>().IsProne())
                    {
                        num = 1.5f;
                    }
                    else if (monster.player.GetComponent<NewPlayerClass>().IsCrouched())
                    {
                        num = 1.3f;
                    }
                    else
                    {
                        num = 1.5f;
                    }
                }
                return PlayerToMonsterDistance(monster) < num;
            }

            private static void HookMonsterSightAI(On.Monster.orig_SightAI orig, Monster monster)
            {
                if ((monster.CanSeePlayer || monster.CanSeeTorch) && !monster.BeenBlinded)
                {
                    monster.TimeOutVision.ResetTimer();
                    float num = PlayerToMonsterDistance(monster);
                    if (num < 20f)
                    {
                        monster.alertMeters.IncreaseAlert("Sight", 100f);
                    }
                    else if (num < 50f)
                    {
                        monster.alertMeters.IncreaseAlert("Sight", 100f / num);
                    }
                    monster.LastSeenPlayerPosition = monster.player.transform.position;
                }
                if (monster.PersistChase && !monster.IsPlayerHiding)
                {
                    monster.LastSeenPlayerPosition = monster.player.transform.position;
                }
                float howEffective = monster.GetMonEffectiveness.HowEffective;
                float f = howEffective / monster.effectiveness.MaxEffectiveness * monster.maxPersistenceBonus + monster.basePersistence;
                monster.persistanceTime = (float)Mathf.RoundToInt(f);
                if (monster.timeoutVision.TimeElapsed > monster.persistanceTime)
                {
                    monster.reduction = 3f;
                    monster.alertMeters.DecreaseAlert("Sight", monster.reduction / howEffective);
                }
            }

            private static void HookMonsterUpdate(On.Monster.orig_Update orig, Monster monster)
            {
                if (!monster.audSource.isPlaying)
                {
                    monster.sourceToUse = monster.audSource;
                }
                else
                {
                    monster.sourceToUse = monster.audSource2;
                }

                if (!ModSettings.useMonsterUpdateGroups || IsMonsterInActiveGroup(monster))
                {
                    monster.UpdateTimers();
                    monster.DoorAI();
                    monster.SightAI();
                    monster.SetRoomVariables();
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MonsterApproachCamera

            private static void HookMonsterApproachCamera(On.MonsterApproachCamera.orig_OnMonsterApproach orig, MonsterApproachCamera monsterApproachCamera, Transform _trans)
            {
                if (Vector3.Distance(((MonoBehaviour)monsterApproachCamera).transform.position, _trans.position) < 2.5f)
                {
                    monsterApproachCamera.alarm.StopTheAlarm();
                    Monster monster = _trans.gameObject.GetComponentInParent<Monster>();
                    monster.GetComponent<MovementControl>().MaxSpeed = 0f;
                    monster.GetComponent<MonsterHearing>().ForceDrop = true;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MonsterBreathing

            private static void HookMonsterBreathing(On.MonsterBreathing.orig_Update orig, MonsterBreathing monsterBreathing)
            {
                if (MonsterStarter.spawned)
                {
                    if (monsterBreathing.breathSource != null)
                    {
                        if (!monsterBreathing.breathSource.isPlaying)
                        {
                            monsterBreathing.timeSinceBreath += Time.deltaTime;
                            if (monsterBreathing.timeSinceBreath > monsterBreathing.breathDelay)
                            {
                                foreach (Monster monster in monsterListMonsterComponents)
                                {
                                    if (monster.GetAniEvents != null)
                                    {
                                        monster.GetAniEvents.Breathe();
                                    }
                                    else
                                    {
                                        Debug.Log("GetAniEvents is null in MonsterBreathing");
                                    }
                                }
                            }
                        }
                        else
                        {
                            monsterBreathing.timeSinceBreath = 0f;
                        }
                    }
                    else
                    {
                        Debug.Log("BreathSource is null in MonsterBreathing");
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MonsterHearing

            private static void HookMonsterHearing()
            {
                On.MonsterHearing.AddSoundChecks += new On.MonsterHearing.hook_AddSoundChecks(HookMonsterHearingAddSoundChecks);
                On.MonsterHearing.FixedUpdate += new On.MonsterHearing.hook_FixedUpdate(HookMonsterHearingFixedUpdate);
                On.MonsterHearing.Update += new On.MonsterHearing.hook_Update(HookMonsterHearingUpdate);
            }

            private static void HookMonsterHearingAddSoundChecks(On.MonsterHearing.orig_AddSoundChecks orig, MonsterHearing monsterHearing, DistractionSound _sound)
            {
                if (!monsterHearing.sounds.Contains(_sound))
                {
                    monsterHearing.sounds.Add(_sound);
                    float delta = CalculateVolume(_sound, monsterHearing.monster);
                    monsterHearing.monster.GetAlertMeters.IncreaseAlert("Sound", delta);
                }
                else if (_sound.Gameplay.minIncrease > 0f)
                {
                    float delta2 = CalculateVolume(_sound, monsterHearing.monster);
                    monsterHearing.monster.GetAlertMeters.IncreaseAlert("Sound", delta2);
                }
            }

            private static void HookMonsterHearingFixedUpdate(On.MonsterHearing.orig_FixedUpdate orig, MonsterHearing monsterHearing)
            {
                if (monsterHearing.currentSound != null && monsterHearing.monster.IsMonsterActive && monsterHearing.currentSound.Source.isPlaying)
                {
                    monsterHearing.currentSound.currentVolume = CalculateVolume(monsterHearing.currentSound, monsterHearing.monster);
                    monsterHearing.currentInterest = monsterHearing.currentSound.currentVolume;
                    monsterHearing.CastDownToRoom(monsterHearing.currentSound);
                    if (!monsterHearing.currentSound.Gameplay.onlyIncreaseOnce)
                    {
                        monsterHearing.alertMeters.IncreaseAlert("Sound", monsterHearing.currentSound.currentVolume);
                    }
                }
            }

            private static void HookMonsterHearingUpdate(On.MonsterHearing.orig_Update orig, MonsterHearing monsterHearing)
            {
                monsterHearing.previousSound = monsterHearing.currentSound;
                monsterHearing.sinceLastNoise += Time.deltaTime;
                if (monsterHearing.monsterFSM != null && monsterHearing.monsterFSM.Current != null)
                {
                    if (monsterHearing.monsterFSM.Current.typeofState == FSMState.StateTypes.Chase)
                    {
                        monsterHearing.noiseBlinker = MonsterHearing.NoiseBlinker.OnlyDistractions;
                    }
                    else
                    {
                        monsterHearing.noiseBlinker = MonsterHearing.NoiseBlinker.AllNoises;
                    }
                }
                else
                {
                    monsterHearing.monsterFSM = monsterHearing.monster.GetComponent<FSM>();
                    monsterHearing.noiseBlinker = MonsterHearing.NoiseBlinker.AllNoises;
                }
                for (int i = 0; i < monsterHearing.sounds.Count; i++)
                {
                    if (!monsterHearing.sounds[i].explored && monsterHearing.monster.IsMonsterActive && monsterHearing.sounds[i].Source != null && monsterHearing.sounds[i].Source.isPlaying)
                    {
                        float volume = CalculateVolume(monsterHearing.sounds[i], monsterHearing.monster);
                        if (volume > monsterHearing.currentInterest || monsterHearing.mostInteresting == monsterHearing.sounds[i])
                        {
                            monsterHearing.currentInterest = volume;
                            monsterHearing.mostInteresting = monsterHearing.sounds[i];
                        }
                        else if (monsterHearing.sounds[i].Source != monsterHearing.LastHighestSource)
                        {
                            monsterHearing.DestroySound(monsterHearing.sounds[i]);
                        }
                    }
                }
                if (monsterHearing.mostInteresting != null)
                {
                    if (!monsterHearing.mostInteresting.explored)
                    {
                        monsterHearing.SetCurrentMostInteresting(monsterHearing.mostInteresting);
                    }
                    else
                    {
                        monsterHearing.SetCurrentMostInteresting(null);
                    }
                }
                for (int j = 0; j < monsterHearing.sounds.Count; j++)
                {
                    if (monsterHearing.sounds[j] == null)
                    {
                        monsterHearing.DestroySound(monsterHearing.sounds[j--]);
                    }
                    else
                    {
                        bool flag = true;
                        if (!monsterHearing.sounds[j].Source.isPlaying)
                        {
                            if (monsterHearing.LastHighestSource != null)
                            {
                                if (monsterHearing.sounds[j].Source == monsterHearing.LastHighestSource && !monsterHearing.sounds[j].expired)
                                {
                                    monsterHearing.sinceHighestNoiseEnded += Time.deltaTime;
                                    if (monsterHearing.sinceHighestNoiseEnded > 10f)
                                    {
                                        monsterHearing.currentInterest = 0f;
                                    }
                                    else
                                    {
                                        flag = false;
                                    }
                                }
                                if (flag)
                                {
                                    monsterHearing.DestroySound(monsterHearing.sounds[j]);
                                }
                            }
                        }
                        else
                        {
                            if (monsterHearing.LastHighestSource != null && monsterHearing.sounds[j].Source == monsterHearing.LastHighestSource)
                            {
                                monsterHearing.sinceHighestNoiseEnded = 0f;
                            }
                            monsterHearing.sounds[j].expired = false;
                        }
                    }
                }
                if (monsterHearing.sinceLastNoise > monsterHearing.monster.SoundTracking || monsterHearing.monster.IsAtPointOfInterest() || monsterHearing.monster.Hearing.PointOfInterest == Vector3.zero || monsterHearing.forceDrop)
                {
                    monsterHearing.forceDrop = false;
                    if (monsterHearing.monster.SoundAlert > 0f)
                    {
                        if (monsterHearing.currentInterest != 0f)
                        {
                            monsterHearing.alertMeters.DecreaseAlert("Sound", monsterHearing.sinceLastNoise / monsterHearing.currentInterest + 10f);
                        }
                        else
                        {
                            monsterHearing.alertMeters.DecreaseAlert("Sound", 2f);
                        }
                    }
                    monsterHearing.currentInterest = 0f;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MonsterFootsteps

            private static void HookMonsterFootsteps(On.MonsterFootsteps.orig_Start orig, MonsterFootsteps monsterFootsteps)
            {
                try
                {
                    ((Action)Activator.CreateInstance(typeof(Action), monsterFootsteps, typeof(FootStepManager).GetMethod("Start").MethodHandle.GetFunctionPointer()))();
                    monsterFootsteps.monsterAnimation = ((FootStepManager)monsterFootsteps).GetComponentInParent<AnimationControl>();
                    Debug.Log("MonsterFootsteps hooked successfully.");
                }
                catch
                {
                    Debug.Log("The MonsterFootsteps hook failed.");
                    orig.Invoke(monsterFootsteps);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MonsterLight

            private static void HookMonsterLight(On.MonsterLight.orig_Update orig, MonsterLight monsterLight)
            {
                if (!ModSettings.doNotRenderBruteLight)
                {
                    for (int i = 0; i < brutes.Count; i++)
                    {
                        if (!ModSettings.useMonsterUpdateGroups || IsMonsterInActiveGroup(brutesMonsterComponents[i]))
                        {
                            try
                            {
                                monsterLight.monster = brutesMonsterComponents[i];
                            }
                            catch
                            {
                                Debug.Log("ML1");
                            }
                            try
                            {
                                monsterLight.stateManager = brutes[i].GetComponent<MState>();
                            }
                            catch
                            {
                                Debug.Log("ML2");
                            }
                            /*
                            if (monsterLight.stateManager == null)
                            {
                                Debug.Log("MState is null ML3");
                                monsterLight.stateManager = monsterLight.monster.GetComponent<MState>();
                            }
                            else if (monsterLight.stateManager.Fsm == null)
                            {
                                Debug.Log("FSM is null ML3");
                                monsterLight.stateManager.fsm = monsterLight.monster.GetComponent<FSM>();
                                if (monsterLight.stateManager.Fsm == null)
                                {
                                    Debug.Log("FSM is still null ML3");
                                }
                                else if (monsterLight.stateManager.Fsm.Current == null)
                                {
                                    Debug.Log("Current is null ML3 2");
                                    if (monsterLight.stateManager.Fsm.startState == null)
                                    {
                                        Debug.Log("Had to assign startState ML3 2");
                                        monsterLight.stateManager.Fsm.startState = brutes[0].GetComponent<FSM>().startState;
                                    }
                                    monsterLight.stateManager.Fsm.StartFSM();

                                    if (monsterLight.stateManager.Fsm.Current == null)
                                    {
                                        Debug.Log("Current is still null ML3 2");
                                    }
                                }
                            }
                            else if (monsterLight.stateManager.Fsm.Current == null)
                            {
                                Debug.Log("Current is null ML3 1");
                                if (monsterLight.stateManager.Fsm.startState == null)
                                {
                                    Debug.Log("Had to assign startState ML3 1");
                                    monsterLight.stateManager.Fsm.startState = brutes[0].GetComponent<FSM>().startState;
                                }

                                monsterLight.stateManager.Fsm.StartFSM();

                                if (monsterLight.stateManager.Fsm.Current == null)
                                {
                                    Debug.Log("Current is still null ML3 1");
                                }
                            }
                            */
                            try
                            {
                                monsterLight.currentState = monsterLight.stateManager.Fsm.Current;
                            }
                            catch
                            {
                                Debug.Log("ML3");
                            }
                            orig.Invoke(monsterLight);
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MonsterReaction

            private static void HookMonsterReaction()
            {
                new Hook(typeof(MonsterReaction).GetNestedType("<LerpToDestruction>c__Iterator0", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.ManyMonstersMode).GetMethod("HookMonsterReactionLerpToDestructionIntermediateHook"), null);
                On.MonsterReaction.OnTriggerEnter += new On.MonsterReaction.hook_OnTriggerEnter(HookMonsterReactionOnTriggerEnter);
                On.MonsterReaction.OnTriggerExit += new On.MonsterReaction.hook_OnTriggerExit(HookMonsterReactionOnTriggerExit);
                On.MonsterReaction.Update += new On.MonsterReaction.hook_Update(HookMonsterReactionUpdate);
            }

            public static bool HookMonsterReactionLerpToDestructionIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookMonsterReactionLerpToDestruction((MonsterReaction)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookMonsterReactionLerpToDestruction(MonsterReaction monsterReaction)
            {
                float t = 0f;
                Vector3 startPosition = monsterReaction.monster.gameObject.transform.position;
                Vector3 targetPosition = ((MonoBehaviour)monsterReaction).transform.position;
                Quaternion startRotation = monsterReaction.monster.gameObject.transform.rotation;
                Vector3 normal = ((MonoBehaviour)monsterReaction).transform.forward;
                normal.y = 0f;
                normal.Normalize();
                Quaternion targetRotation = Quaternion.LookRotation(normal, Vector3.up);
                while (t < 1f)
                {
                    t += Time.deltaTime;
                    monsterReaction.monster.gameObject.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
                    monsterReaction.monster.gameObject.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                    yield return null;
                }
                monsterReaction.monster.gameObject.transform.position = targetPosition;
                monsterReaction.monster.gameObject.transform.rotation = targetRotation;
                monsterReaction.inPosition = true;
                yield break;
            }

            private static void HookMonsterReactionOnTriggerEnter(On.MonsterReaction.orig_OnTriggerEnter orig, MonsterReaction monsterReaction, Collider _other)
            {
                /*
                if (monsterList != null)
                {
                    for (int i = 0; i < monsterList.Count; i++)
                    {
                        if (_other.transform.root.tag == "Monster" && _other.GetComponentInParent<Monster>().GetInstanceID() == monsterInstanceIDs[i])
                        {
                            monsterReaction.monster = monsterListMonsterComponents[i];
                            monsterReaction.monNearby = true;
                            if (monsterReaction.destroyingThis != null)
                            {
                                monsterReaction.monster.DestroyingThis = monsterReaction.destroyingThis;
                            }
                            break;
                        }
                    }
                }
                */
                if (_other.transform.root.tag == "Monster")
                {
                    monsterReaction.monster = _other.GetComponentInParent<Monster>();
                    monsterReaction.monNearby = true;
                    if (monsterReaction.destroyingThis != null)
                    {
                        monsterReaction.monster.DestroyingThis = monsterReaction.destroyingThis;
                    }
                }
            }

            private static void HookMonsterReactionOnTriggerExit(On.MonsterReaction.orig_OnTriggerExit orig, MonsterReaction monsterReaction, Collider _other)
            {
                if (_other.transform.root.tag == "Monster" && _other.GetComponentInParent<Monster>().GetInstanceID() == monsterReaction.monster.GetInstanceID())
                {
                    monsterReaction.monNearby = false;
                    monsterReaction.monster.DestroyingThis = null;
                }
            }

            private static void HookMonsterReactionUpdate(On.MonsterReaction.orig_Update orig, MonsterReaction monsterReaction)
            {
                if (monsterReaction.monster != null)
                {
                    if (monsterReaction.monNearby && !monsterReaction.beenDestroyed && !monsterReaction.monster.IsPlayerLocationKnown && !monsterReaction.monster.PersistChase)
                    {
                        ((MonoBehaviour)monsterReaction).StartCoroutine(monsterReaction.LerpToDestruction());
                        if (monsterReaction.inPosition && !monsterReaction.doOnce)
                        {
                            monsterReaction.doOnce = true;
                            monsterReaction.monster.MoveControl.LockRotation = true;
                            monsterReaction.monster.MoveControl.GetAniControl.SetPieceAnimation = 0;
                            ((MonoBehaviour)monsterReaction).Invoke("EndDestruction", 3f);
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MonsterStarter

            private static void HookMonsterStarter()
            {
                On.MonsterStarter.GetMonsterStuff += new On.MonsterStarter.hook_GetMonsterStuff(HookMonsterStarterGetMonsterStuff);
                On.MonsterStarter.Spawn += new On.MonsterStarter.hook_Spawn(HookMonsterStarterSpawn);
            }

            private static void HookMonsterStarterGetMonsterStuff(On.MonsterStarter.orig_GetMonsterStuff orig, MonsterStarter monsterStarter)
            {
                monsterStarter.monster = LevelGeneration.Instance.GetMonster;
                if (monsterStarter.monster != null)
                {
                    monsterStarter.monsterStates = monsterStarter.monster.GetComponent<MState>();
                    foreach (MState mState in monsterListStates)
                    {
                        if (mState != null)
                        {
                            if (mState.monster != null)
                            {
                                mState.monster.Starter = monsterStarter;
                            }
                            else
                            {
                                Debug.Log("GMS MS M is null");
                            }
                        }
                        else
                        {
                            Debug.Log("GMS MS is null");
                        }
                    }
                }
            }

            private static void HookMonsterStarterSpawn(On.MonsterStarter.orig_Spawn orig, MonsterStarter monsterStarter)
            {
                if (!MonsterStarter.spawned)
                {
                    monsterStarter.GetMonsterStuff();
                    monsterStarter.spawning = true;
                    MonsterStarter.spawned = true;
                    monsterStarter.timeActive.StartTimer();
                }

                if (!ModSettings.spawnMonsterInStarterRoom)
                {
                    List<int> spawnTimes = new List<int>(); //HashSet<int> spawnTimes = new HashSet<int>();
                    for (int i = 0; i < monsterList.Count; i++)
                    {
                        bool validSpawnTime = false;
                        while (!validSpawnTime)
                        {
                            int randomTime = UnityEngine.Random.Range((int)ModSettings.minSpawnTime, (int)ModSettings.maxSpawnTime + 1);
                            if (!spawnTimes.Contains(randomTime))
                            {
                                spawnTimes.Add(randomTime);
                                validSpawnTime = true;
                            }
                        }
                    }

                    int[] spawnTimesArray = spawnTimes.ToArray();
                    for (int i = 0; i < monsterList.Count; i++)
                    {
                        ((MonoBehaviour)monsterStarter).StartCoroutine(SpawnMonster(i, spawnTimesArray[i]));
                    }
                }
                else
                {
                    for (int i = 0; i < monsterList.Count; i++)
                    {
                        ((MonoBehaviour)monsterStarter).StartCoroutine(SpawnMonster(i, monsterStarter.spawnTime));
                    }
                }
            }

            public static List<int> alternatingMonsterNumbersToSpawn;

            private static IEnumerator SpawnMonster(int i, float timeToWait = 0f)
            {
                try
                {
                    Debug.Log(string.Concat(new object[] { "Attempting to spawn monster number ", i, " of type ", monsterListMonsterComponents[i].monsterType, "." }));
                }
                catch
                {
                    Debug.Log("Error in SpawnMonster 0");
                }

                try
                {
                    if (monsterListMonsterComponents[i].MonsterType == Monster.MonsterTypeEnum.Hunter)
                    {
                        timeToWait = UnityEngine.Random.Range(1f, 2f);
                    }
                }
                catch
                {
                    Debug.Log("Error in SpawnMonster 1");
                }

                yield return new WaitForSeconds(timeToWait);

                Vector3 position;
                try
                {
                    if (ModSettings.spawnMonsterInStarterRoom)
                    {
                        if (monsterListMonsterComponents[i].MonsterType != Monster.MonsterTypeEnum.Hunter)
                        {
                            position = ModSettings.temporaryPlayerPosition;
                        }
                        else
                        {
                            position = Vector3.zero;
                        }
                    }
                    else
                    {
                        position = monsterListMonsterComponents[i].starter.ChooseSpawnPoint();
                    }
                    monsterList[i].transform.position = position;
                }
                catch
                {
                    Debug.Log("Error in SpawnMonster 2");
                }
                try
                {
                    if (monsterListMonsterComponents[i].starter.chosenSpawn != null)
                    {
                        monsterList[i].transform.localRotation = monsterListMonsterComponents[i].starter.chosenSpawn.transform.localRotation;
                    }
                }
                catch
                {
                    Debug.Log("Error in SpawnMonster 3");
                }
                try
                {
                    MAlertMeters mAlertMeters = monsterListMonsterComponents[i].GetAlertMeters;
                    /*
                    if (mAlertMeters == null)
                    {
                        Debug.Log("MAlertMeters is null in SpawnMonster 4");
                        monsterListMonsterComponents[i].alertMeters = monsterListMonsterComponents[i].GetComponent<MAlertMeters>();
                        mAlertMeters = monsterListMonsterComponents[i].GetAlertMeters;
                        if (mAlertMeters == null)
                        {
                            Debug.Log("MAlertMeters is still null in SpawnMonster 4");
                        }
                    }
                    */
                    monsterListMonsterComponents[i].GetAlertMeters.mSightAlert = 0f;
                    monsterListMonsterComponents[i].GetAlertMeters.mProxAlert = 0f;
                    monsterListMonsterComponents[i].GetAlertMeters.mSoundAlert = 0f;
                }
                catch
                {
                    Debug.Log("Error in SpawnMonster 4");
                }
                try
                {
                    monsterListMonsterComponents[i].SetAIType();
                }
                catch
                {
                    Debug.Log("Error in SpawnMonster 5");
                }
                try
                {
                    monsterListMonsterComponents[i].IsMonsterActive = true;
                }
                catch
                {
                    Debug.Log("Error in SpawnMonster 6");
                }
                try
                {
                    monsterList[i].SetActive(true);
                }
                catch
                {
                    Debug.Log("Error in SpawnMonster 7");
                }
                try
                {
                    monsterListMonsterComponents[i].MoveControl.SnapToFloor();
                }
                catch
                {
                    Debug.Log("Error in SpawnMonster 8");
                }
                try
                {
                    if (ModSettings.varyingMonsterSizes && ModSettings.customMonsterScale == 1f && i != 0)
                    {
                        monsterList[i].transform.localScale -= new Vector3(0.2f, 0.2f + (float)(i / 10), 0.2f + (float)(i / 10));
                    }
                    else if (ModSettings.customMonsterScale != 1f)
                    {
                        monsterList[i].transform.localScale = new Vector3(ModSettings.customMonsterScale, ModSettings.customMonsterScale, ModSettings.customMonsterScale);
                    }
                }
                catch
                {
                    Debug.Log("Error in SpawnMonster 9");
                }
                try
                {
                    if (ModSettings.enableMultiplayer)
                    {
                        MultiplayerMode.ChoosePlayerMidRound(monsterListMonsterComponents[i]);
                    }

                    if (ModSettings.alternatingMonstersMode && ModSettings.numberOfMonsters > ModSettings.numberOfAlternatingMonsters && !alternatingMonsterNumbersToSpawn.Contains(i))
                    {
                        monsterList[i].transform.position = Vector3.zero;
                        monsterList[i].SetActive(false);
                    }
                    else if (ModSettings.seerMonster)
                    {
                        ModSettings.ForceChase(monsterListMonsterComponents[i]);
                    }
                }
                catch
                {
                    Debug.Log("Error in SpawnMonster 10");
                }
                try
                {
                    Debug.Log(string.Concat(new object[] { "Spawned monster ", i, " of type ", monsterListMonsterComponents[i].monsterType, " successfully." }));
                }
                catch
                {
                    Debug.Log("Error in SpawnMonster 11");
                }
                yield break;
            }

            public static void CreateNewMonster(float spawnDelay = 0f, string monsterToCreateString = "")
            {
                if (monsterToCreateString.Equals(""))
                {
                    // Determine which monsters are being used in the game.
                    bool bruteInGame = ModSettings.numberOfBrutes != 0;
                    bool hunterInGame = ModSettings.numberOfHunters != 0;
                    bool fiendInGame = ModSettings.numberOfFiends != 0;

                    // Pick a monster to create. Default is Brute (0). Max must be one above monster index because max is not inclusive.
                    int monsterToCreateInteger = 0;

                    // Conditionals that cover all possible monster presence combinations. These are 6 total. 5 below and 1 above. 100, 010, 001, 110, 011, 111.
                    if (bruteInGame)
                    {
                        if (hunterInGame)
                        {
                            if (fiendInGame)
                            {
                                monsterToCreateInteger = UnityEngine.Random.Range(0, 3);
                            }
                            else
                            {
                                monsterToCreateInteger = UnityEngine.Random.Range(0, 2);
                            }

                        }
                        else if (fiendInGame)
                        {
                            int randomMonsterNumber = UnityEngine.Random.Range(0, 2);
                            if (randomMonsterNumber == 1)
                            {
                                randomMonsterNumber++;
                            }
                            monsterToCreateInteger = randomMonsterNumber;
                        }
                    }
                    else if (hunterInGame)
                    {
                        if (fiendInGame)
                        {
                            monsterToCreateInteger = UnityEngine.Random.Range(1, 3);
                        }
                        else
                        {
                            monsterToCreateInteger = 1;
                        }
                    }
                    else if (fiendInGame)
                    {
                        monsterToCreateInteger = 2;
                    }

                    // Change the monsterToCreateString based on the monsterToCreateInteger.
                    switch (monsterToCreateInteger)
                    {
                        case 1:
                            monsterToCreateString = "Hunter";
                            break;
                        case 2:
                            monsterToCreateString = "Fiend";
                            break;
                        default:
                            monsterToCreateString = "Brute";
                            break;
                    }
                }

                // # Ensure that list size declaration works. If not, fix it by adding elements manually, i.e. hunter thresholds and fiend auras.

                // Resize the relevant monster arrays.
                ModSettings.numberOfMonsters++;
                int oldSize = monsterList.Count;

                try
                {
                    // Using this for the array / list position will refer to the newly added object.
                    //int newSize = oldSize + 1;
                    //Array.Resize(ref monsterList, newSize);
                    //Array.Resize(ref monsterListMonsterComponents, newSize);
                    //Array.Resize(ref monsterInstanceIDs, newSize);
                    //Array.Resize(ref monsterListStates, newSize);
                    switch (monsterToCreateString)
                    {
                        case "Hunter":
                            //int newSizeHunters = hunters.Count + 1;
                            //Array.Resize(ref hunters, newSizeHunters);
                            //Array.Resize(ref huntersMonsterComponents, newSizeHunters);
                            //Array.Resize(ref huntersInstanceIDs, newSizeHunters);
                            //Array.Resize(ref huntersTrappingStates, newSizeHunters);
                            //Array.Resize(ref sightBelowThresholdArray, newSizeHunters);
                            //Array.Resize(ref soundBelowThresholdArray, newSizeHunters);
                            //Array.Resize(ref proxBelowThresholdArray, newSizeHunters);
                            //Array.Resize(ref AllBelowThresholdArray, newSizeHunters);
                            //Array.Resize(ref hunterThresholdValArray, newSizeHunters);

                            //monsterList[oldSize] = UnityEngine.Object.Instantiate<GameObject>(UnityEngine.Object.FindObjectOfType<MonsterSelection>().NameToObject("Hunter"));

                            //monsterList[oldSize] = UnityEngine.Object.Instantiate<GameObject>(hunters[0]);

                            monsterList.Add(UnityEngine.Object.Instantiate<GameObject>(UnityEngine.Object.FindObjectOfType<MonsterSelection>().NameToObject("Hunter")));

                            //monsterList.Add(UnityEngine.Object.Instantiate<GameObject>(hunters[0]));
                            break;
                        case "Fiend":
                            //int newSizeFiends = fiends.Count + 1;
                            //Array.Resize(ref fiends, newSizeFiends);
                            //Array.Resize(ref fiendsMonsterComponents, newSizeFiends);
                            //Array.Resize(ref fiendsInstanceIDs, newSizeFiends);
                            //Array.Resize(ref fiendsThatAreInRangeOfPlayer, newSizeFiends);
                            //Array.Resize(ref fiendsThatSeePlayer, newSizeFiends);
                            //Array.Resize(ref auras, newSizeFiends);

                            //monsterList[oldSize] = UnityEngine.Object.Instantiate<GameObject>(UnityEngine.Object.FindObjectOfType<MonsterSelection>().NameToObject("Fiend"));

                            //monsterList[oldSize] = UnityEngine.Object.Instantiate<GameObject>(fiends[0]);

                            monsterList.Add(UnityEngine.Object.Instantiate<GameObject>(UnityEngine.Object.FindObjectOfType<MonsterSelection>().NameToObject("Fiend")));

                            //monsterList.Add(UnityEngine.Object.Instantiate<GameObject>(fiends[0]));
                            break;
                        default:
                            //int newSizeBrutes = brutes.Count + 1;
                            //Array.Resize(ref brutes, newSizeBrutes);
                            //Array.Resize(ref brutesMonsterComponents, newSizeBrutes);
                            //Array.Resize(ref brutesInstanceIDs, newSizeBrutes);

                            //monsterList[oldSize] = UnityEngine.Object.Instantiate<GameObject>(UnityEngine.Object.FindObjectOfType<MonsterSelection>().NameToObject("Brute"));

                            //monsterList[oldSize] = UnityEngine.Object.Instantiate<GameObject>(brutes[0]);

                            monsterList.Add(UnityEngine.Object.Instantiate<GameObject>(UnityEngine.Object.FindObjectOfType<MonsterSelection>().NameToObject("Brute")));

                            //monsterList.Add(UnityEngine.Object.Instantiate<GameObject>(brutes[0]));
                            break;
                    }
                }
                catch
                {
                    Debug.Log("Failed monster specific cloning operations in create new monster.");
                }
                try
                {

                    monsterList[oldSize].SetActive(true); // This will run Monster.Awake, letting components be assigned.
                    monsterList[oldSize].SetActive(false);
                    monsterListMonsterComponents.Add(monsterList[oldSize].GetComponent<Monster>());
                    monsterInstanceIDtoMonsterNumberDict.Add(monsterListMonsterComponents[oldSize].GetInstanceID(), oldSize);
                    monsterListStates.Add(monsterList[oldSize].GetComponent<MState>());
                    monsterListStates[oldSize].m = monsterListMonsterComponents[oldSize];
                    /*
                    if (monsterListStates[oldSize].Fsm == null)
                    {
                        Debug.Log("FSM is null CreateNewMonster");
                        monsterListStates[oldSize].fsm = monsterListMonsterComponents[oldSize].GetComponent<FSM>();
                        if (monsterListStates[oldSize].Fsm == null)
                        {
                            Debug.Log("FSM is still null CreateNewMonster");
                        }
                        else if (monsterListStates[oldSize].Fsm.Current == null)
                        {
                            Debug.Log("Current is null CreateNewMonster 2");
                            if (monsterListStates[oldSize].Fsm.startState == null)
                            {
                                Debug.Log("Had to assign startState CreateNewMonster 2");
                                monsterListStates[oldSize].Fsm.startState = brutes[0].GetComponent<FSM>().startState;
                            }
                            monsterListStates[oldSize].Fsm.StartFSM();

                            if (monsterListStates[oldSize].Fsm.Current == null)
                            {
                                Debug.Log("Current is still null CreateNewMonster 2");
                            }
                        }
                    }
                    else if (monsterListStates[oldSize].Fsm.Current == null)
                    {
                        Debug.Log("Current is null CreateNewMonster 1");
                        if (monsterListStates[oldSize].Fsm.startState == null)
                        {
                            Debug.Log("Had to assign startState CreateNewMonster 1");
                            monsterListStates[oldSize].Fsm.startState = brutes[0].GetComponent<FSM>().startState;
                        }

                        monsterListStates[oldSize].Fsm.StartFSM();

                        if (monsterListStates[oldSize].Fsm.Current == null)
                        {
                            Debug.Log("Current is still null CreateNewMonster 1");
                        }
                    }
                    */
                    UnityEngine.Debug.Log(string.Concat(new object[] { "INSTANCE ID FOR NEWLY CREATED MONSTER NUMBER ", oldSize, " of type ", monsterListMonsterComponents[oldSize].monsterType, " ----- The ID stored is " + monsterInstanceIDtoMonsterNumberDict[oldSize] + "." }));
                }
                catch
                {
                    Debug.Log("Failed generic monster list adjustment operations in create new monster.");
                }

                try
                {
                    // Reassign components that were null.
                    monsterListMonsterComponents[oldSize].playerRoomDetect = References.Player.GetComponentInChildren<DetectRoom>();
                    monsterListMonsterComponents[oldSize].monsterRoomDetect = ((MonoBehaviour)monsterListMonsterComponents[oldSize]).GetComponentInChildren<MonstDetectRoom>();
                    monsterListMonsterComponents[oldSize].sourceToUse = Instantiate<AudioSource>(monsterListMonsterComponents[0].sourceToUse, ((MonoBehaviour)monsterListMonsterComponents[oldSize]).transform);
                    monsterListMonsterComponents[oldSize].GetComponent<PatrolPoints>().monster = monsterListMonsterComponents[oldSize];
                }
                catch
                {
                    Debug.Log("Failed null component adjustment operations in create new monster.");
                }

                try
                {
                    switch (monsterToCreateString)
                    {
                        case "Hunter":
                            hunters.Add(monsterList[oldSize]);
                            huntersMonsterComponents.Add(monsterListMonsterComponents[oldSize]);
                            huntersInstanceIDs.Add(monsterInstanceIDtoMonsterNumberDict[oldSize], ModSettings.numberOfHunters);
                            ModSettings.numberOfHunters++;
                            try
                            {
                                huntersTrappingStates.Add(monsterListMonsterComponents[oldSize].TrappingState);
                            }
                            catch
                            {
                                Debug.Log("Failed to add to hunter trapping states.");
                            }
                            try
                            {
                                sightBelowThresholdList.Add(true);
                                soundBelowThresholdList.Add(true);
                                proxBelowThresholdList.Add(true);
                                allBelowThresholdList.Add(true);
                                hunterThresholdValList.Add(0f);
                            }
                            catch
                            {
                                Debug.Log("Failed to add to hunter thresholds.");
                            }
                            if (monsterListMonsterComponents[oldSize].ears == null)
                            {
                                monsterListMonsterComponents[oldSize].ears = Instantiate<MonsterEars>(huntersMonsterComponents[0].ears, ((MonoBehaviour)monsterListMonsterComponents[oldSize]).transform);
                            }
                            break;
                        case "Fiend":
                            fiends.Add(monsterList[oldSize]);
                            fiendsMonsterComponents.Add(monsterListMonsterComponents[oldSize]);
                            fiendsInstanceIDs.Add(monsterInstanceIDtoMonsterNumberDict[oldSize], ModSettings.numberOfFiends);
                            ModSettings.numberOfFiends++;
                            fiendsThatAreInRangeOfPlayer.Add(false);
                            fiendsThatSeePlayer.Add(false);
                            auras.Add(monsterList[oldSize].GetComponentsInChildren<FiendAura>(true)[0]);
                            fiendMindAttackFiendsTargetingPlayer[0].Add(MonsterNumber(monsterInstanceIDtoMonsterNumberDict[oldSize]));
                            break;
                        default:
                            brutes.Add(monsterList[oldSize]);
                            brutesMonsterComponents.Add(monsterListMonsterComponents[oldSize]);
                            brutesInstanceIDs.Add(monsterInstanceIDtoMonsterNumberDict[oldSize], ModSettings.numberOfBrutes);
                            ModSettings.numberOfBrutes++;
                            break;
                    }
                }
                catch
                {
                    Debug.Log("Failed monster specific list adjustment operations in create new monster.");
                }

                // Redeclare monster update groups if these are being used.
                if (ModSettings.useMonsterUpdateGroups)
                {
                    DeclareMonsterGroups();
                }

                try
                {
                    if (monsterListMonsterComponents[0].Starter != null)
                    {
                        monsterListMonsterComponents[oldSize].Starter = monsterListMonsterComponents[0].Starter;
                        if (MonsterStarter.spawned)
                        {
                            //monsterListMonsterComponents[oldSize].Awake();
                            //monsterListMonsterComponents[oldSize].Patrol.monster = monsterListMonsterComponents[oldSize];

                            //((MonoBehaviour)monsterListMonsterComponents[oldSize]).StartCoroutine(SpawnMonster(oldSize));
                            TimeScaleManager.Instance.StartCoroutine(SpawnMonster(oldSize));
                        }
                    }
                }
                catch
                {
                    Debug.Log("Failed starter operations in create new monster.");
                }
            }

            private static IEnumerator MonsterSpawnSpeedrunMonsterSpawner()
            {
                int originalNumberOfMonsters = ModSettings.numberOfMonsters;
                while (!MonsterStarter.spawned && !monsterList[originalNumberOfMonsters - 1].activeSelf)
                {
                    yield return null;
                }
                while (MonsterStarter.spawned && (ModSettings.monsterSpawningLimit == 0 || ModSettings.numberOfMonsters <= ModSettings.monsterSpawningLimit))
                {
                    yield return new WaitForSeconds(ModSettings.monsterSpawnSpeedrunSpawnTime);
                    Debug.Log(string.Concat(new object[] { ModSettings.monsterSpawnSpeedrunSpawnTime, " seconds have passed. Spawning new monster." }));
                    CreateNewMonster(monsterList.Count - originalNumberOfMonsters + 1f);
                }
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MovementControl

            private static IEnumerator HookMovementControl(On.MovementControl.orig_LerpToThis orig, MovementControl movementControl, Vector3 targetPos, Vector3 faceThis, string lerpReason)
            {
                if (!movementControl.lerpStarted)
                {
                    movementControl.lerpStarted = true;
                    movementControl.LockRotation = true;
                    float t = 0f;
                    Vector3 startPosition = movementControl.monster.transform.position;
                    Quaternion startRotation = movementControl.monster.transform.rotation;
                    Vector3 look = faceThis - targetPos;
                    Quaternion targetRotation = Quaternion.LookRotation(look, Vector3.up);
                    while (t < 1f)
                    {
                        t += Time.deltaTime;
                        movementControl.monster.GetComponent<Monster>().transform.position = Vector3.Lerp(startPosition, targetPos, t);
                        movementControl.monster.GetComponent<Monster>().transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
                        yield return null;
                    }
                    movementControl.monster.transform.position = targetPos;
                    movementControl.monster.transform.rotation = targetRotation;
                    movementControl.OnFacingPosition(lerpReason);
                }
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MRoomSearch

            private static void HookMRoomSearch()
            {
                On.MRoomSearch.FindDeckWalkToPoints += new On.MRoomSearch.hook_FindDeckWalkToPoints(HookMRoomSearchFindDeckWalkToPoints);
                On.MRoomSearch.Mec_OnFinishIntro += new On.MRoomSearch.hook_Mec_OnFinishIntro(HookMRoomSearchMec_OnFinishIntro);
                On.MRoomSearch.Mec_OnFinishGrab += new On.MRoomSearch.hook_Mec_OnFinishGrab(HookMRoomSearchMec_OnFinishGrab);
                On.MRoomSearch.StartInvestigate += new On.MRoomSearch.hook_StartInvestigate(HookMRoomSearchStartInvestigate);
                On.MRoomSearch.StartSearch += new On.MRoomSearch.hook_StartSearch(HookMRoomSearchStartSearch);
                On.MRoomSearch.OnAnimationEvent += new On.MRoomSearch.hook_OnAnimationEvent(HookMRoomSearchOnAnimationEvent);
                On.MRoomSearch.OnEnter += new On.MRoomSearch.hook_OnEnter(HookMRoomSearchOnEnter);
                On.MRoomSearch.OnMonsterFinishLerp += new On.MRoomSearch.hook_OnMonsterFinishLerp(HookMRoomSearchOnMonsterFinishLerp);
                On.MRoomSearch.OnUpdate += new On.MRoomSearch.hook_OnUpdate(HookMRoomSearchOnUpdate);
            }

            private static void HookMRoomSearchFindDeckWalkToPoints(On.MRoomSearch.orig_FindDeckWalkToPoints orig, MRoomSearch mRoomSearch)
            {
                Collider[] array = Physics.OverlapSphere(((MState)mRoomSearch).monster.transform.position, 10f, mRoomSearch.floorMask);
                for (int i = 0; i < array.Length; i++)
                {
                    Room room = array[i].GetComponent<Room>();
                    if (room == null)
                    {
                        room = array[i].GetComponentInParent<Room>();
                    }
                    if (room != null)
                    {
                        SearchEventSpot componentInChildren = room.GetComponentInChildren<SearchEventSpot>();
                        if (componentInChildren != null && componentInChildren.gameObject.activeSelf)
                        {
                            NodeData nodeDataAtPosition = LevelGeneration.GetNodeDataAtPosition(componentInChildren.gameObject.transform.position);
                            if (nodeDataAtPosition.nodeRoom != null)
                            {
                                RoomStructure roomType = nodeDataAtPosition.nodeRoom.RoomType;
                                NodeData nodeDataAtPosition2 = LevelGeneration.GetNodeDataAtPosition(mRoomSearch.playerclass.transform.position);
                                if (roomType != RoomStructure.Deck)
                                {
                                    if (roomType != RoomStructure.Engine)
                                    {
                                        if (roomType == RoomStructure.Walkway)
                                        {
                                            if (nodeDataAtPosition.regionNode.y == nodeDataAtPosition2.regionNode.y)
                                            {
                                                mRoomSearch.validWalkToPoints.Add(componentInChildren);
                                            }
                                        }
                                    }
                                    else if (nodeDataAtPosition.regionNode.y == nodeDataAtPosition2.regionNode.y)
                                    {
                                        mRoomSearch.validWalkToPoints.Add(componentInChildren);
                                    }
                                }
                                else
                                {
                                    mRoomSearch.validWalkToPoints.Add(componentInChildren);
                                }
                            }
                        }
                    }
                }
            }

            private static void HookMRoomSearchMec_OnFinishIntro(On.MRoomSearch.orig_Mec_OnFinishIntro orig, MRoomSearch mRoomSearch)
            {
                if (((MState)mRoomSearch).IsActive)
                {
                    mRoomSearch.foundYou = false;
                    if (mRoomSearch.targetHidingSpot != null && mRoomSearch.savedPlayerSpot != null && mRoomSearch.targetHidingSpot == mRoomSearch.savedPlayerSpot)
                    {
                        int monsterNumber = MonsterNumber(mRoomSearch.monster.GetInstanceID());
                        monstersFinishedGrab[monsterNumber] = false;

                        mRoomSearch.StartCoroutine(StopMonstersFromKillingPlayer(5f, mRoomSearch.monster));
                        mRoomSearch.foundYou = true;
                        lastMonsterSentMessage = mRoomSearch.monster;
                        mRoomSearch.targetHidingSpot.SendHidingMessage(MonsterHidingEvents.HidingEvent.OnMonsterStartGrab);
                    }
                    if (!mRoomSearch.foundYou && mRoomSearch.targetHidingSpot != null)
                    {
                        lastMonsterSentMessage = mRoomSearch.monster;
                        mRoomSearch.targetHidingSpot.SendHidingMessage(MonsterHidingEvents.HidingEvent.OnMonsterStartSearch);
                    }
                    if (mRoomSearch.foundYou)
                    {
                        mRoomSearch.maxSpeed = 0f;
                        ((MState)mRoomSearch).monster.ShouldSearchRoom = false;
                    }
                    else
                    {
                        mRoomSearch.foundYou = false;
                        mRoomSearch.maxSpeed = mRoomSearch.topSpeed;
                    }
                }
            }

            public static IEnumerator StopMonstersFromKillingPlayer(float timeToStopMonsters, Monster mRoomSearchMonster, bool stopAllMonsters = false)
            {
                int crewPlayerIndex = 0;
                if (ModSettings.enableMultiplayer)
                {
                    crewPlayerIndex = MultiplayerMode.crewPlayers.IndexOf(mRoomSearchMonster.PlayerDetectRoom.player);
                }
                ModSettings.spawnProtection[crewPlayerIndex] = true;
                ModSettings.SetInvincibilityMode(true, crewPlayerIndex);
                int monsterNumber = MonsterNumber(mRoomSearchMonster.GetInstanceID());
                foreach (Monster monster in monsterListMonsterComponents)
                {
                    if (/* monster.IsPlayerInRoom */ monster != mRoomSearchMonster || stopAllMonsters)
                    {
                        monster.RoomSearcher.allowChase = false;
                        monster.RoomSearcher.changingState = true;
                        monster.MoveControl.LockRotation = true;
                    }
                }
                while (!monstersFinishedGrab[monsterNumber])
                {
                    yield return null;
                }
                yield return new WaitForSeconds(timeToStopMonsters);
                foreach (Monster monster in monsterListMonsterComponents)
                {
                    if (monster != mRoomSearchMonster || stopAllMonsters)
                    {
                        monster.RoomSearcher.allowChase = true;
                        monster.RoomSearcher.changingState = false;
                        monster.MoveControl.LockRotation = false;
                    }
                }
                ModSettings.SetInvincibilityMode(false, crewPlayerIndex);
                ModSettings.spawnProtection[crewPlayerIndex] = false;
                yield break;
            }

            private static void HookMRoomSearchMec_OnFinishGrab(On.MRoomSearch.orig_Mec_OnFinishGrab orig, MRoomSearch mRoomSearch, int timeToWait)
            {
                mRoomSearch.allowChase = false;
                if (timeToWait == 0)
                {
                    timeToWait = 3;
                }
                if (mRoomSearch.targetHidingSpot != null && ((MState)mRoomSearch).IsActive)
                {
                    lastMonsterSentMessage = mRoomSearch.monster;
                    mRoomSearch.targetHidingSpot.SendHidingMessage(MonsterHidingEvents.HidingEvent.OnMonsterFinishGrab);
                }
                mRoomSearch.maxSpeed = 0f;
                if (((MState)mRoomSearch).IsActive)
                {
                    ((MState)mRoomSearch).Invoke("Mec_OnFinishGrabRoar", (float)timeToWait);
                }
                int monsterNumber = MonsterNumber(mRoomSearch.monster.GetInstanceID());
                monstersFinishedGrab[monsterNumber] = true;
            }

            private static void HookMRoomSearchStartInvestigate(On.MRoomSearch.orig_StartInvestigate orig, MRoomSearch mRoomSearch, Vector3 _reachedSpot)
            {
                if ((mRoomSearch.searchType == MRoomSearch.ESearchType.Investigate || mRoomSearch.searchType == MRoomSearch.ESearchType.Initial) && mRoomSearch.targetInvestigateSpot != null && Vector3.Distance(mRoomSearch.targetInvestigateSpot.MonsterPoint, _reachedSpot) < 0.75f && !mRoomSearch.arrived)
                {
                    mRoomSearch.arrived = true;
                    mRoomSearch.allowChase = false;
                    ((MState)mRoomSearch).monster.MoveControl.LockRotation = true;
                    mRoomSearch.triesTimeout = 0;
                    lastMonsterSentMessage = mRoomSearch.monster;
                    mRoomSearch.targetInvestigateSpot.SendSearchMessage(RoomSearchEvent.SearchEvent.OnMonsterStartInvestigateLerp);
                }
            }

            private static void HookMRoomSearchStartSearch(On.MRoomSearch.orig_StartSearch orig, MRoomSearch mRoomSearch, Vector3 _reachedSpot)
            {
                if ((mRoomSearch.searchType == MRoomSearch.ESearchType.Searching || mRoomSearch.searchType == MRoomSearch.ESearchType.LocationKnown) && mRoomSearch.targetHidingSpot != null && Vector3.Distance(mRoomSearch.targetHidingSpot.MonsterPoint, _reachedSpot) < 0.75f && !mRoomSearch.arrived)
                {
                    mRoomSearch.triesTimeout = 0;
                    mRoomSearch.arrived = true;
                    mRoomSearch.allowChase = false;
                    mRoomSearch.lookTowardsNextTarget = false;
                    ((MState)mRoomSearch).monster.MoveControl.LockRotation = true;
                    lastMonsterSentMessage = mRoomSearch.monster;
                    mRoomSearch.targetHidingSpot.SendHidingMessage(MonsterHidingEvents.HidingEvent.OnMonsterStartLerp);
                    mRoomSearch.savedPlayerSpot = mRoomSearch.PlayerHidingSpot;
                    if (mRoomSearch.savedPlayerSpot == mRoomSearch.targetHidingSpot)
                    {
                        mRoomSearch.playerclass.Motor.enabled = false;
                    }
                    mRoomSearch.LockMovement();
                }
                else
                {
                    Debug.Log("MRoomSearch.StartSearch test failed");
                }
            }

            private static void HookMRoomSearchOnAnimationEvent(On.MRoomSearch.orig_OnAnimationEvent orig, MRoomSearch mRoomSearch, string _animationEvent)
            {
                if (mRoomSearch.targetHidingSpot != null)
                {
                    lastMonsterSentMessage = mRoomSearch.monster;
                    mRoomSearch.targetHidingSpot.SendMessageUpwards(_animationEvent, SendMessageOptions.DontRequireReceiver);
                }
                else
                {
                    mRoomSearch.targetHidingSpot = mRoomSearch.PlayerHidingSpot;
                    if (mRoomSearch.targetHidingSpot != null)
                    {
                        lastMonsterSentMessage = mRoomSearch.monster;
                        mRoomSearch.targetHidingSpot.SendMessageUpwards(_animationEvent, SendMessageOptions.DontRequireReceiver);
                    }
                }
            }

            private static void HookMRoomSearchOnEnter(On.MRoomSearch.orig_OnEnter orig, MRoomSearch mRoomSearch)
            {
                if (ModSettings.noHiding)
                {
                    ModSettings.ForceChase(mRoomSearch.monster);
                }

                mRoomSearch.OnMonsterFinishLerp();

                mRoomSearch.playerclass = ((MState)mRoomSearch).monster.player.GetComponent<NewPlayerClass>();
                mRoomSearch.detectRoom = mRoomSearch.playerclass.gameObject.GetComponentInChildren<DetectRoom>();

                ((Action)Activator.CreateInstance(typeof(Action), mRoomSearch, typeof(MState).GetMethod("OnEnter").MethodHandle.GetFunctionPointer()))();
                mRoomSearch.RandomiseAnimation();
                mRoomSearch.ResetLists();
                ((MState)mRoomSearch).GetComponent<PatrolPoints>().enabled = false;
                ((MState)mRoomSearch).GetComponent<PatrolPoints>().StopAllCoroutines();
                mRoomSearch.typeofState = FSMState.StateTypes.RoomSearch;
                mRoomSearch.monEff = ((MState)mRoomSearch).transform.GetComponent<MonsterEffectiveness>();
                mRoomSearch.room = ((MState)mRoomSearch).monster.RoomDetect.CurrentRoom;

                mRoomSearch.foundYou = false;
                if (((MState)mRoomSearch).monster.BothOnDeck() || ((MState)mRoomSearch).monster.BothInEngineRoom())
                {
                    mRoomSearch.MaxTimePerSpot = 60;
                    mRoomSearch.spots = mRoomSearch.FindDeckHidingSpots();
                }
                else
                {
                    mRoomSearch.MaxTimePerSpot = 30;
                    mRoomSearch.spots = mRoomSearch.room.HidingSpots;
                    foreach (GameObject item in mRoomSearch.room.PatrolPoints)
                    {
                        mRoomSearch.roomPatrolPoints.Add(item);
                    }
                }
                mRoomSearch.timebetweenSpots = 0f;
                mRoomSearch.timeOutRoom = 0f;
                mRoomSearch.searchType = MRoomSearch.ESearchType.Initial;
                mRoomSearch.goal = Vector3.zero;
                mRoomSearch.allowChase = true;
                mRoomSearch.testForRoom = false;

                if (!ModSettings.enableCrewVSMonsterMode || MonsterNumber(mRoomSearch.monster.GetInstanceID()) >= ModSettings.numbersOfMonsterPlayers.Count || CrewVsMonsterMode.letAIControlMonster)
                {
                    ((MState)mRoomSearch).monster.ShouldSearchRoom = true;
                    mRoomSearch.maxSpeed = mRoomSearch.topSpeed;
                    ((MState)mRoomSearch).monster.MoveControl.MaxSpeed = 0f;
                    ((MState)mRoomSearch).monster.AudSource.maxDistance = 15f;
                    ((MState)mRoomSearch).monster.GetAlertMeters.mSightAlert = 0f;
                    ((MState)mRoomSearch).monster.GetAlertMeters.mProxAlert = 0f;
                    ((MState)mRoomSearch).monster.GetAlertMeters.mSoundAlert = 0f;
                    mRoomSearch.CombineLists();
                    ((MState)mRoomSearch).Invoke("TestRoom", 3f);
                }

                lastMonsterSeen = ((MState)mRoomSearch).monster;
            }

            private static void HookMRoomSearchOnMonsterFinishLerp(On.MRoomSearch.orig_OnMonsterFinishLerp orig, MRoomSearch mRoomSearch)
            {
                if (mRoomSearch.targetHidingSpot != null && (mRoomSearch.searchType == MRoomSearch.ESearchType.Searching || mRoomSearch.searchType == MRoomSearch.ESearchType.LocationKnown))
                {
                    lastMonsterSentMessage = mRoomSearch.monster;
                    mRoomSearch.targetHidingSpot.SendHidingMessage(MonsterHidingEvents.HidingEvent.OnMonsterReachedHiding);
                    mRoomSearch.CheckSpot(mRoomSearch.targetHidingSpot);
                }
                else if (mRoomSearch.targetInvestigateSpot != null && (mRoomSearch.searchType == MRoomSearch.ESearchType.Investigate || mRoomSearch.searchType == MRoomSearch.ESearchType.Initial))
                {
                    lastMonsterSentMessage = mRoomSearch.monster;
                    mRoomSearch.targetInvestigateSpot.SendSearchMessage(RoomSearchEvent.SearchEvent.OnMonsterReachedEvent);
                    mRoomSearch.CheckEventSpot(mRoomSearch.targetInvestigateSpot);
                }
            }

            private static void HookMRoomSearchOnUpdate(On.MRoomSearch.orig_OnUpdate orig, MRoomSearch mRoomSearch)
            {
                if (!ModSettings.useMonsterUpdateGroups || IsMonsterInActiveGroup(mRoomSearch.monster))
                {
                    ((Action)Activator.CreateInstance(typeof(Action), mRoomSearch, typeof(MState).GetMethod("OnUpdate").MethodHandle.GetFunctionPointer()))();
                    int crewPlayerIndex = 0;
                    if (ModSettings.enableMultiplayer)
                    {
                        crewPlayerIndex = MultiplayerMode.crewPlayers.IndexOf(mRoomSearch.monster.PlayerDetectRoom.player);
                    }
                    if (!ModSettings.spawnProtection[crewPlayerIndex])
                    {
                        mRoomSearch.changingState = false;
                    }
                    if (((MState)mRoomSearch).IsActive)
                    {
                        mRoomSearch.timebetweenSpots += Time.deltaTime;
                        mRoomSearch.RoomSearchDucking();
                        if (!ModSettings.enableCrewVSMonsterMode || MonsterNumber(mRoomSearch.monster.GetInstanceID()) >= ModSettings.numbersOfMonsterPlayers.Count || CrewVsMonsterMode.letAIControlMonster)
                        {
                            mRoomSearch.PointTargeting();
                        }
                        else
                        {
                            if (mRoomSearch.allowChase && MultiplayerMode.GetPlayerKey("Jump", ModSettings.numbersOfMonsterPlayers[MonsterNumber(mRoomSearch.monster.GetInstanceID())]).JustPressed() && mRoomSearch.spots.Length > 0)
                            {
                                HidingSpot closestHidingSpot = mRoomSearch.spots[0];
                                float closestHidingSpotDistance = Vector3.Distance(mRoomSearch.monster.transform.position, closestHidingSpot.transPoint);
                                for (int i = 1; i < mRoomSearch.spots.Length; i++)
                                {
                                    float distanceToPotentialHidingSpot = Vector3.Distance(mRoomSearch.monster.transform.position, mRoomSearch.spots[i].transPoint);
                                    if (distanceToPotentialHidingSpot < closestHidingSpotDistance)
                                    {
                                        closestHidingSpot = mRoomSearch.spots[i];
                                        closestHidingSpotDistance = distanceToPotentialHidingSpot;
                                    }
                                }
                                if (closestHidingSpotDistance < 2f)
                                {
                                    mRoomSearch.searchType = MRoomSearch.ESearchType.Searching;
                                    mRoomSearch.targetHidingSpot = closestHidingSpot;
                                    mRoomSearch.arrived = false;

                                    Debug.Log("Searching spot at " + closestHidingSpot.transPoint + " for monster " + mRoomSearch.monster.transform.position);
                                    mRoomSearch.StartSearch(mRoomSearch.targetHidingSpot.MonsterPoint);
                                }
                            }
                        }
                        mRoomSearch.ChangeOfStates();
                        mRoomSearch.ChaseChecking();
                        if (!ModSettings.enableCrewVSMonsterMode || MonsterNumber(mRoomSearch.monster.GetInstanceID()) >= ModSettings.numbersOfMonsterPlayers.Count || CrewVsMonsterMode.letAIControlMonster)
                        {
                            mRoomSearch.SpeedChanges();
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MSearchingState

            private static void HookMSearchingState()
            {
                On.MSearchingState.OnEnter += new On.MSearchingState.hook_OnEnter(HookMSearchingStateOnEnter);
                On.MSearchingState.OnUpdate += new On.MSearchingState.hook_OnUpdate(HookMSearchingStateOnUpdate);
                On.MSearchingState.StateChanges += new On.MSearchingState.hook_StateChanges(HookMSearchingStateStateChanges);
            }

            private static void HookMSearchingStateOnEnter(On.MSearchingState.orig_OnEnter orig, MSearchingState mSearchingState)
            {
                ((Action)Activator.CreateInstance(typeof(Action), mSearchingState, typeof(MState).GetMethod("OnEnter").MethodHandle.GetFunctionPointer()))();
                if (((MState)mSearchingState).monster.HeliStarted())
                {
                    mSearchingState.typeofState = FSMState.StateTypes.FinaleEvent;
                }
                else
                {
                    mSearchingState.typeofState = FSMState.StateTypes.LowAlert;
                }
                ((MState)mSearchingState).monster.MoveControl.enabled = true;
                mSearchingState.finishedFirstSearch = false;
                mSearchingState.secondarySearch = false;
                ((MState)mSearchingState).monster.ShouldSearchRoom = false;
                mSearchingState.aggression = ((MState)mSearchingState).monster.GetMonEffectiveness.HowEffective;
                mSearchingState.FindFirstGoal(true);
                ((MState)mSearchingState).monster.MoveControl.MaxSpeed = 50f;
                if (!((MState)mSearchingState).monster.PreviousWasDestroy && !((MState)mSearchingState).monster.PreviousWasClimb)
                {
                    ((MState)mSearchingState).monster.MoveControl.RecalculatePath();
                }
                SetThresholdValue(((MState)mSearchingState).monster);
                HunterThresholdChanges(((MState)mSearchingState).monster);
            }

            private static void HookMSearchingStateOnUpdate(On.MSearchingState.orig_OnUpdate orig, MSearchingState mSearchingState)
            {
                if (!ModSettings.useMonsterUpdateGroups || IsMonsterInActiveGroup(mSearchingState.monster))
                {
                    ((Action)Activator.CreateInstance(typeof(Action), mSearchingState, typeof(MState).GetMethod("OnUpdate").MethodHandle.GetFunctionPointer()))();
                    HunterThresholdChanges(((MState)mSearchingState).monster);
                    mSearchingState.StateChanges();
                    mSearchingState.FindFirstGoal(false);
                    mSearchingState.MakeSound();
                }
            }

            private static void HookMSearchingStateStateChanges(On.MSearchingState.orig_StateChanges orig, MSearchingState mSearchingState)
            {
                ((MState)mSearchingState).monster.MoveControl.MaxSpeed = mSearchingState.SetSpeed();
                if (((MState)mSearchingState).monster.SubEventBeenStarted())
                {
                    ((MState)mSearchingState).SendEvent("EventStarted");
                }
                else if (((MState)mSearchingState).monster.MoveControl.shouldClimb)
                {
                    ((MState)mSearchingState).SendEvent("Climb");
                }
                else if (((MState)mSearchingState).monster.IsMonsterRetreating)
                {
                    ((MState)mSearchingState).SendEvent("Retreat");
                }
                else if (((MState)mSearchingState).monster.IsPlayerLocationKnown || ((MState)mSearchingState).monster.FoundPlayerBySound)
                {
                    mSearchingState.finishedFirstSearch = true;
                    mSearchingState.isSearchingSight = false;
                    mSearchingState.isSearchingSound = false;
                    mSearchingState.isSearchingProx = false;
                    mSearchingState.secondarySearch = false;
                    if (!ModSettings.useSparky)
                    {
                        ((MState)mSearchingState).SendEvent("PlayerFound");
                    }
                    else
                    {
                        if (!((MState)mSearchingState).monster.monsterType.Equals("Sparky"))
                        {
                            ((MState)mSearchingState).SendEvent("PlayerFound");
                        }
                        else
                        {
                            ((MState)mSearchingState).SendEvent("Lurk");
                        }
                    }
                }
                else if (((MState)mSearchingState).monster.IsMonsterDestroying)
                {
                    ((MState)mSearchingState).SendEvent("Destroy");
                }
                else if (IsBelowThreshold(((MState)mSearchingState).monster) || mSearchingState.HunterOutOfRange())
                {
                    if (!ModSettings.enableCrewVSMonsterMode && (ModSettings.enableCrewVSMonsterMode && CrewVsMonsterMode.letAIControlMonster))
                    {
                        ((MState)mSearchingState).SendEvent("Retreat");
                    }
                }
                else
                {
                    if (((MState)mSearchingState).monster.IsAtPointOfInterest())
                    {
                        mSearchingState.isSearchingSound = false;
                    }
                    if (((MState)mSearchingState).monster.FoundPlayerBySound)
                    {
                        if (!ModSettings.useSparky)
                        {
                            ((MState)mSearchingState).SendEvent("PlayerFound");
                        }
                        else
                        {
                            if (!((MState)mSearchingState).monster.monsterType.Equals("Sparky"))
                            {
                                ((MState)mSearchingState).SendEvent("PlayerFound");
                            }
                            else
                            {
                                ((MState)mSearchingState).SendEvent("Lurk");
                            }
                        }
                    }
                    else if (((MState)mSearchingState).monster.InSearchableArea() && !IsBelowThreshold(((MState)mSearchingState).monster) && (!ModSettings.enableCrewVSMonsterMode || CrewVsMonsterMode.letAIControlMonster))
                    {
                        if (!((MState)mSearchingState).monster.IsPlayerLocationKnown && (((MState)mSearchingState).monster.Hearing.CurrentSoundSource == null || !((MState)mSearchingState).monster.Hearing.CurrentSoundSource.Source.isPlaying) && ((MState)mSearchingState).monster.GetAlertMeters.currentHigh > 75)
                        {
                            ((MState)mSearchingState).monster.ShouldSearchRoom = true;
                            ((MState)mSearchingState).SendEvent("Search Room");
                        }
                    }
                    else if ((((MState)mSearchingState).monster.MoveControl.IsAtDestination || ((MState)mSearchingState).monster.MoveControl.IsStuck) && ((MState)mSearchingState).TimeInState > 5f)
                    {
                        ((MState)mSearchingState).monster.GetAniEvents.Growl();
                        ((MState)mSearchingState).SendEvent("Go Idle");
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MState

            private static void HookMState(On.MState.orig_OnReset orig, MState mState)
            {
                if (mState.timeout == null)
                {
                    mState.timeout = new Timer();
                }
                orig.Invoke(mState);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MTrappingState

            private static int ambushPointsSet;
            private static List<AmbushPoint> deployedPoints;
            private static Timer trapsActiveFor;
            private static bool deployingTraps;
            private static bool deployingSuccessfully;

            private static void HookMTrappingState()
            {
                On.MTrappingState.AfterSpawnCheck += new On.MTrappingState.hook_AfterSpawnCheck(HookMTrappingStateAfterSpawnCheck);
                On.MTrappingState.Ambush += new On.MTrappingState.hook_Ambush(HookMTrappingStateAmbush);
                On.MTrappingState.AmbushDeploy += new On.MTrappingState.hook_AmbushDeploy(HookMTrappingStateAmbushDeploy);
                On.MTrappingState.OnUpdate += new On.MTrappingState.hook_OnUpdate(HookMTrappingStateOnUpdate);
                On.MTrappingState.Respawn += new On.MTrappingState.hook_Respawn(HookMTrappingStateRespawn);
                On.MTrappingState.SetOffTrap += new On.MTrappingState.hook_SetOffTrap(HookMTrappingStateSetOffTrap);
                On.MTrappingState.SetUpAmbush += new On.MTrappingState.hook_SetUpAmbush(HookMTrappingStateSetUpAmbush);
                On.MTrappingState.SetUpHunting += new On.MTrappingState.hook_SetUpHunting(HookMTrappingStateSetUpHunting);
                On.MTrappingState.StateChanges += new On.MTrappingState.hook_StateChanges(HookMTrappingStateStateChanges);
                On.MTrappingState.OnEnter += new On.MTrappingState.hook_OnEnter(HookMTrappingStateOnEnter);
            }

            private static void HookMTrappingStateOnEnter(On.MTrappingState.orig_OnEnter orig, MTrappingState mTrappingState)
            {
                lastMonsterSentMessage = ((MState)mTrappingState).monster;
                orig.Invoke(mTrappingState);
            }

            private static void HookMTrappingStateAfterSpawnCheck(On.MTrappingState.orig_AfterSpawnCheck orig, MTrappingState mTrappingState)
            {
                if (mTrappingState.investState == null)
                {
                    mTrappingState.investState = ((MState)mTrappingState).GetComponent<MInvestigateState>();
                }
                ((MState)mTrappingState).monster.GetEars.MoveBackToOriginal();
                if (!AreOtherHuntersInTrappingState(true))
                {
                    if (logHunterActions)
                    {
                        Debug.Log("Allowing disarm from after spawn check");
                    }
                    AmbushSystem.AllowDisarm = true;
                }
                if (mTrappingState.ShouldSpawnImmediately || (mTrappingState.TriggerCollider != null && mTrappingState.TriggerCollider.transform.root.tag == "Player"))
                {
                    if (mTrappingState.ShouldAmbush || (mTrappingState.TriggerCollider != null && mTrappingState.TriggerCollider.transform.root.tag == "Player"))
                    {
                        mTrappingState.triggeredByPlayer = true;
                        mTrappingState.ShouldAmbush = false;
                        ((MState)mTrappingState).monster.LastSeenPlayerPosition = mTrappingState.monster.transform.position;
                        ((MState)mTrappingState).monster.GetAlertMeters.mSightAlert = 100f;
                        ((MState)mTrappingState).monster.TimeOutVision.ResetTimer();
                        ((MState)mTrappingState).monster.GetComponent<MChasingState>().IsAmbushingPlayer = true;
                        mTrappingState.TriggerCollider = null;
                        mTrappingState.ShouldSpawnImmediately = false;
                        ((MState)mTrappingState).SendEvent("Chase");
                    }
                    else
                    {
                        if (((MState)mTrappingState).monster.HasPointOfInterest && ((MState)mTrappingState).monster.Hearing.PointOfInterest.y > 0f)
                        {
                            mTrappingState.investState.POI = ((MState)mTrappingState).monster.Hearing.PointOfInterest;
                        }
                        else
                        {
                            mTrappingState.investState.POI = mTrappingState.monster.transform.position;
                        }
                        mTrappingState.ShouldSpawnImmediately = false;
                        if (((MState)mTrappingState).monster.Hearing.CurrentSoundSource != null && ((MState)mTrappingState).monster.Hearing.CurrentSoundSource.Gameplay.highAlertSound)
                        {
                            ((MState)mTrappingState).SendEvent("Search");
                        }
                        else
                        {
                            ((MState)mTrappingState).SendEvent("Investigate");
                        }
                    }
                }
                else
                {
                    mTrappingState.triggeredByPlayer = false;
                    if (mTrappingState.TriggerCollider != null && mTrappingState.TriggerCollider.transform.position.y > 0f)
                    {
                        mTrappingState.investState.POI = mTrappingState.TriggerCollider.transform.position;
                        mTrappingState.investState.ColliderOfInterest = mTrappingState.TriggerCollider;
                    }
                    else
                    {
                        mTrappingState.investState.POI = mTrappingState.monster.transform.position;
                    }
                    ((MState)mTrappingState).SendEvent("Investigate");
                }
            }

            private static void HookMTrappingStateAmbush(On.MTrappingState.orig_Ambush orig, MTrappingState mTrappingState, Vector3 monsterPointOfInterest)
            {
                if (logHunterActions)
                {
                    Debug.Log("Attempting Ambush");
                }
                if (!mTrappingState.OutOfTrap)
                {
                    RoomCategory getRoomCategory = ((MState)mTrappingState).monster.PlayerDetectRoom.GetRoomCategory;
                    if (!mTrappingState.trapSet)
                    {
                        AmbushPoint ambushPoint;
                        if (ambushPointsSet == 0)
                        {
                            if (mTrappingState.huntState.TrueAmbush)
                            {
                                ambushPoint = FindAmbush(monsterPointOfInterest, false, ((MState)mTrappingState).monster);
                            }
                            else if (getRoomCategory != RoomCategory.Outside)
                            {
                                ambushPoint = FindAmbushInArea(monsterPointOfInterest, 3f, 15f, ((MState)mTrappingState).monster, false);
                            }
                            else
                            {
                                ambushPoint = FindAmbushInArea(monsterPointOfInterest, 5f, 20f, ((MState)mTrappingState).monster, false);
                            }
                        }
                        else
                        {
                            // ambushPoint = FindAmbushInArea(monsterPointOfInterest, 5f, 20f, ((MState)mTrappingState).monster, true); // Changed when creating multiplayer. Why is this not FindNearestDeployedAmbush?
                            ambushPoint = AmbushSystem.FindNearestDeployedAmbush(((MState)mTrappingState).monster.player.transform.position);
                        }
                        if (ambushPoint != null)
                        {
                            mTrappingState.trapSet = true;
                            mTrappingState.trapPos = ambushPoint.MonsterSpawnPoint.position;
                            mTrappingState.triggeredAmbush = ambushPoint;
                            mTrappingState.lookAt = mTrappingState.triggeredAmbush.lookAt.position;
                            mTrappingState.Respawn();
                        }
                        else if (mTrappingState.timesRetried < mTrappingState.maxRetries)
                        {
                            mTrappingState.timesRetried += 1f;
                            mTrappingState.shouldretryAmbush = true;
                        }
                        else
                        {
                            mTrappingState.SetUpHunting();
                        }
                    }
                }
            }

            private static void HookMTrappingStateAmbushDeploy(On.MTrappingState.orig_AmbushDeploy orig, MTrappingState mTrappingState, List<AmbushPoint> _ambushes)
            {
                if (AmbushSystem.AllowDisarm || ambushPointsSet == 0)
                {
                    if (logHunterActions)
                    {
                        Debug.Log("Running trapping state ambushdeploy");
                    }
                    trapsActiveFor = new Timer();
                    if (!mTrappingState.OutOfTrap)
                    {
                        if (_ambushes.Count < 2)
                        {
                            ((MState)mTrappingState).SendEvent("Hunt");
                        }
                        else
                        {
                            ambushPointsSet = _ambushes.Count;
                            deployedPoints = _ambushes;
                            trapsActiveFor.ResetTimer();
                        }
                    }
                }
            }

            private static void HookMTrappingStateOnUpdate(On.MTrappingState.orig_OnUpdate orig, MTrappingState mTrappingState)
            {
                if (!ModSettings.useMonsterUpdateGroups || IsMonsterInActiveGroup(mTrappingState.monster))
                {
                    if (ModSettings.enableCrewVSMonsterMode && !CrewVsMonsterMode.letAIControlMonster)
                    {
                        mTrappingState.shouldretryAmbush = false;
                    }
                    lastMonsterSentMessage = ((MState)mTrappingState).monster;
                    orig.Invoke(mTrappingState);
                }
            }

            private static void HookMTrappingStateRespawn(On.MTrappingState.orig_Respawn orig, MTrappingState mTrappingState)
            {
                mTrappingState.OutOfTrap = true;
                ((MState)mTrappingState).monster.BlockSightDecrease = true;
                float num = 0f;
                switch (mTrappingState.triggeredAmbush.trapType)
                {
                    case AmbushPoint.TrapType.Ceiling:
                        ((MState)mTrappingState).monster.HunterAnimations.SpawnType = 2f;
                        num = 1f;
                        break;
                    case AmbushPoint.TrapType.Pod:
                        ((MState)mTrappingState).monster.HunterAnimations.SpawnType = 1f;
                        break;
                    case AmbushPoint.TrapType.Vent:
                        ((MState)mTrappingState).monster.HunterAnimations.SpawnType = 0f;
                        break;
                    case AmbushPoint.TrapType.Deck:
                        ((MState)mTrappingState).monster.HunterAnimations.SpawnType = 3f;
                        break;
                    case AmbushPoint.TrapType.Cargo:
                        ((MState)mTrappingState).monster.HunterAnimations.SpawnType = 1f;
                        break;
                    case AmbushPoint.TrapType.LWDPod:
                        ((MState)mTrappingState).monster.HunterAnimations.SpawnType = 1f;
                        break;
                    case AmbushPoint.TrapType.EnginePipe:
                        {
                            ((MState)mTrappingState).monster.HunterAnimations.SpawnType = 5f;
                            HunterPipeDestruction component = mTrappingState.triggeredAmbush.transform.parent.parent.GetComponent<HunterPipeDestruction>();
                            if (component != null)
                            {
                                component.DestroyPipe();
                            }
                            break;
                        }
                }
                ((MState)mTrappingState).monster.HunterAnimations.IdleType = num;
                ((MState)mTrappingState).Invoke("MoveToTrap", 0.1f);
                if (num == 1f)
                {
                    mTrappingState.ChooseStandSpawnDirection();
                }
                bool flag = ((MState)mTrappingState).monster.HunterAnimations.SpawnHunter();
                if (flag)
                {
                    foreach (SkinnedMeshRenderer skinnedMeshRenderer in ((MState)mTrappingState).monster.MonsterMesh)
                    {
                        skinnedMeshRenderer.enabled = true;
                    }
                    ((MState)mTrappingState).monster.LastSeenPlayerPosition = mTrappingState.monster.player.transform.position;
                    if (deployedPoints != null && deployedPoints.Count > 0 && !AreOtherHuntersInTrappingState(true))
                    {
                        deployedPoints.Clear();
                    }
                    mTrappingState.ambushFailureCount = 0;
                }
                else
                {
                    mTrappingState.OutOfTrap = false;
                }
            }

            private static bool HookMTrappingStateSetOffTrap(On.MTrappingState.orig_SetOffTrap orig, MTrappingState mTrappingState, Vector3 _trapPos, Vector3 _lookAt)
            {
                if (logHunterActions)
                {
                    Debug.Log("Running set off trap");
                }
                int localAmbushPointsSet = ambushPointsSet;
                mTrappingState.trapsTried += 1f;
                if (localAmbushPointsSet > 5)
                {
                    localAmbushPointsSet = 5;
                }
                if (((MState)mTrappingState).monster.PlayerDetectRoom.RoomType == RoomStructure.Cargo || mTrappingState.triggeredAmbush.trapType == AmbushPoint.TrapType.EnginePipe)
                {
                    mTrappingState.Chance = 100f;
                }
                else
                {
                    mTrappingState.Chance = (1f + 0.1f * (float)(mTrappingState.ambushFailureCount * localAmbushPointsSet)) / (float)localAmbushPointsSet;
                }
                float randomChance = (float)UnityEngine.Random.Range(0, 100);
                randomChance *= 0.01f;
                if (logHunterActions)
                {
                    Debug.Log("Monster ID: " + mTrappingState.monster.GetInstanceID() + ", for which trap set is " + mTrappingState.trapSet + " and out of trap is " + mTrappingState.OutOfTrap + ". Inherent chance is " + mTrappingState.Chance + " and random chance is " + randomChance);
                }
                if (randomChance < mTrappingState.Chance)
                {
                    if (!mTrappingState.trapSet && !mTrappingState.OutOfTrap)
                    {
                        mTrappingState.trapSet = true;
                        mTrappingState.trapPos = _trapPos;
                        mTrappingState.lookAt = _lookAt;
                        mTrappingState.trapsTried = 0f;
                        mTrappingState.Respawn();
                        if (logHunterActions)
                        {
                            Debug.Log("Setting off trap");
                            Debug.Log("Ambush points before1 " + ambushPointsSet);
                        }
                        ambushPointsSet--;
                        if (logHunterActions)
                        {
                            Debug.Log("Ambush points after1 " + ambushPointsSet);
                        }
                        return true;
                    }
                }
                else if (ambushPointsSet > 0)
                {
                    mTrappingState.allowEscape = true;
                    if (logHunterActions)
                    {
                        Debug.Log("Ambush points before2 " + ambushPointsSet);
                    }
                    ambushPointsSet--;
                    if (logHunterActions)
                    {
                        Debug.Log("Ambush points after2 " + ambushPointsSet);
                    }
                }
                else
                {
                    if (logHunterActions)
                    {
                        Debug.Log("Setting up hunting from SetOffTrap");
                    }
                    mTrappingState.SetUpHunting();
                }
                return false;
            }

            private static void HookMTrappingStateSetUpAmbush(On.MTrappingState.orig_SetUpAmbush orig, MTrappingState mTrappingState)
            {
                if (mTrappingState.ShouldSpawnImmediately)
                {
                    if (((MState)mTrappingState).monster.PlayerDetectRoom.GetRoomCategory == RoomCategory.Outside)
                    {
                        if (logHunterActions)
                        {
                            Debug.Log("Ambushing immediately at point of interest outside");
                        }
                        mTrappingState.Ambush(((MState)mTrappingState).monster.Hearing.PointOfInterest);
                    }
                    else if (mTrappingState.PlayerInCargoHold())
                    {
                        if (logHunterActions)
                        {
                            Debug.Log("Ambushing player immediately in cargo hold");
                        }
                        mTrappingState.isCargoPods = true;
                        AmbushSystem.DeployAreaAmbushes(AmbushPoint.TrapType.Cargo, new AmbushSystem.OnAmbushDeploy(mTrappingState.AmbushDeploy));
                    }
                    else if (mTrappingState.PlayerInEngineRoom())
                    {
                        if (logHunterActions)
                        {
                            Debug.Log("Ambushing player immediately in engine room");
                        }
                        mTrappingState.Ambush(((MState)mTrappingState).monster.Hearing.PointOfInterest);
                    }
                    else if (((MState)mTrappingState).monster.Hearing.PointOfInterest != Vector3.zero && ((MState)mTrappingState).monster.CanHearNoise)
                    {
                        if (logHunterActions)
                        {
                            Debug.Log("Ambushing point of interest immediately generically");
                        }
                        mTrappingState.Ambush(((MState)mTrappingState).monster.Hearing.PointOfInterest);
                    }
                    else
                    {
                        if (logHunterActions)
                        {
                            Debug.Log("Ambushing immediately generically");
                        }
                        mTrappingState.Ambush(References.Player.transform.position);
                    }
                }
                else if (mTrappingState.PlayerInCargoHold())
                {
                    if (logHunterActions)
                    {
                        Debug.Log("Ambushing player in cargo hold");
                    }
                    mTrappingState.isCargoPods = true;
                    AmbushSystem.DeployAreaAmbushes(AmbushPoint.TrapType.Cargo, new AmbushSystem.OnAmbushDeploy(mTrappingState.AmbushDeploy));
                }
                else if (mTrappingState.PlayerInEngineRoom())
                {
                    if (logHunterActions)
                    {
                        Debug.Log("Ambushing player in engine room");
                    }
                    mTrappingState.isEnginePods = true;
                    AmbushSystem.DeployAreaAmbushes(AmbushPoint.TrapType.EnginePipe, new AmbushSystem.OnAmbushDeploy(mTrappingState.AmbushDeploy));
                }
                else
                {
                    if (logHunterActions)
                    {
                        Debug.Log("Ambushing generically");
                    }
                    //lastMonsterSentMessage = monsterListMonsterComponents[MonsterNumber(((MState)mTrappingState).monster.GetInstanceID())]; //((MState)mTrappingState).monster;
                    if (lastMonsterSentMessage == null)
                    {
                        Debug.Log("Last monster sent message is null in deploy");
                    }
                    else
                    {
                        Debug.Log("Last monster sent message is not null in deploy");
                    }
                    //monsterDeployingNumber = 100; //MonsterNumber(((MState)mTrappingState).monster.GetInstanceID());
                    AmbushSystem.Deploy(new AmbushSystem.OnAmbushDeploy(mTrappingState.AmbushDeploy));
                    //Deploy(new AmbushSystem.OnAmbushDeploy(mTrappingState.AmbushDeploy), ((MState)mTrappingState).monster);
                }
            }
            // lastMonsterSentMessage must be updated in OnEnter or OnUpdate because it cannot be updated persistently by this method. lastMonsterSentMessage would always be null in Deploy.

            private static void HookMTrappingStateSetUpHunting(On.MTrappingState.orig_SetUpHunting orig, MTrappingState mTrappingState)
            {
                mTrappingState.ambushFailureCount++;
                if (!AreOtherHuntersInTrappingState(true) || (ambushPointsSet == 0 && !deployingTraps))
                {
                    if (logHunterActions)
                    {
                        Debug.Log("Allowing disarm from setuphunting");
                    }
                    AmbushSystem.AllowDisarm = true;
                }
                ((MState)mTrappingState).monster.HunterAnimations.GoToHiding = true;
                ((MState)mTrappingState).SendEvent("Hunt");
            }

            private static void HookMTrappingStateStateChanges(On.MTrappingState.orig_StateChanges orig, MTrappingState mTrappingState)
            {
                if (!ModSettings.enableCrewVSMonsterMode || (ModSettings.enableCrewVSMonsterMode && !CrewVsMonsterMode.letAIControlMonster))
                {
                    bool isPlayerOnStairs = false;
                    if (((MState)mTrappingState).monster.PlayerDetectRoom != null)
                    {
                        Room getRoom = ((MState)mTrappingState).monster.PlayerDetectRoom.GetRoom;
                        if (getRoom != null && getRoom.HasTag("Stairs"))
                        {
                            isPlayerOnStairs = true;
                            mTrappingState.allowEscape = true;
                        }
                    }
                    if (mTrappingState.isCargoPods && !mTrappingState.PlayerInCargoHold())
                    {
                        mTrappingState.allowEscape = true;
                    }
                    if (mTrappingState.isEnginePods && !mTrappingState.PlayerInEngineRoom())
                    {
                        mTrappingState.allowEscape = true;
                    }
                    if (((MState)mTrappingState).monster.SubEventBeenStarted())
                    {
                        ((MState)mTrappingState).SendEvent("EventStarted");
                    }
                    if (!mTrappingState.trapSet && ((MState)mTrappingState).monster.HasPointOfInterest && ((MState)mTrappingState).monster.Hearing.currentSound != null && ((MState)mTrappingState).monster.Hearing.currentSound.Gameplay.highAlertSound && !mTrappingState.ShouldAmbush)
                    {
                        mTrappingState.ShouldSpawnImmediately = true;
                        mTrappingState.Ambush(((MState)mTrappingState).monster.Hearing.PointOfInterest);
                    }
                    if (ambushPointsSet > 0)
                    {
                        if (!trapsActiveFor.IsRunning)
                        {
                            trapsActiveFor.StartTimer();
                        }
                        if (!mTrappingState.trapSet && mTrappingState.allowEscape)
                        {
                            AmbushPoint nearestDeployedAmbush = AmbushSystem.FindNearestDeployedAmbush(mTrappingState.monster.player.transform.position);
                            bool isPlayerOnMainStairs = isPlayerOnStairs && !mTrappingState.PlayerInEngineRoom() && !mTrappingState.PlayerInCargoHold();
                            if (isPlayerOnMainStairs || (mTrappingState.isCargoPods && !mTrappingState.PlayerInCargoHold()) || (mTrappingState.isEnginePods && !mTrappingState.PlayerInEngineRoom()) || (nearestDeployedAmbush != null && Vector3.Distance(mTrappingState.monster.player.transform.position, nearestDeployedAmbush.transform.position) > 8f))
                            {
                                if (!mTrappingState.TrapDodgedTimer.IsRunning)
                                {
                                    mTrappingState.TrapDodgedTimer.StartTimer();
                                }
                                if (isPlayerOnMainStairs || (mTrappingState.isCargoPods && !mTrappingState.PlayerInCargoHold()) || mTrappingState.TrapDodgedTimer.TimeElapsed > 5f)
                                {
                                    bool canATrapBeSeen = false;
                                    foreach (AmbushPoint ambushPoint in deployedPoints)
                                    {
                                        canATrapBeSeen = ambushPoint.CanTrapBeSeen();
                                        if (canATrapBeSeen)
                                        {
                                            break;
                                        }
                                    }
                                    if (!canATrapBeSeen)
                                    {
                                        List<AmbushPoint> list = new List<AmbushPoint>();
                                        foreach (AmbushPoint ambushPoint3 in deployedPoints)
                                        {
                                            if (!ambushPoint3.IsTriggered)
                                            {
                                                list.Add(ambushPoint3);
                                            }
                                        }
                                        if (list.Count > 0)
                                        {
                                            int index = UnityEngine.Random.Range(0, list.Count);
                                            mTrappingState.huntState.lastHidingSpot = list[index];
                                        }
                                        mTrappingState.SetUpHunting();
                                        mTrappingState.TrapDodgedTimer.StopTimer();
                                        mTrappingState.TrapDodgedTimer.ResetTimer();
                                    }
                                }
                            }
                            else
                            {
                                mTrappingState.TrapDodgedTimer.StopTimer();
                                mTrappingState.TrapDodgedTimer.ResetTimer();
                            }
                        }
                    }
                    if (ambushPointsSet < 1 && ((MState)mTrappingState).Fsm.Current.TimeInState > 10f && !mTrappingState.shouldretryAmbush)
                    {
                        mTrappingState.SetUpHunting();
                    }
                }
            }

            private static bool AreOtherHuntersInTrappingState(bool isHunterCallingInTrappingState)
            {
                bool oneHunterIgnored = false;
                foreach (Monster hunter in huntersMonsterComponents)
                {
                    if (hunter.GetComponent<MState>().Fsm.Current.GetType() == typeof(MTrappingState))
                    {
                        if (oneHunterIgnored || !isHunterCallingInTrappingState)
                        {
                            if (logHunterActions)
                            {
                                Debug.Log("Other Hunters are in trapping state");
                            }
                            return true;
                        }
                        oneHunterIgnored = true;
                    }
                }

                if (logHunterActions)
                {
                    Debug.Log("Other Hunters are NOT in trapping state");
                    Debug.Log(new StackTrace().ToString());
                }
                return false;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MVentFrenzyState

            private static void HookMVentFrenzyState(On.MVentFrenzyState.orig_OnUpdate orig, MVentFrenzyState mVentFrenzyState)
            {
                if (!ModSettings.useMonsterUpdateGroups || IsMonsterInActiveGroup(mVentFrenzyState.monster))
                {
                    ((MState)mVentFrenzyState).monster.MoveControl.MaxSpeed = 0f;
                    mVentFrenzyState.ChooseVents();
                    mVentFrenzyState.MakeNoise();
                    mVentFrenzyState.AttemptToBreakIn();
                    mVentFrenzyState.CheckIfInRoom();
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MWanderState

            private static void HookMWanderState(On.MWanderState.orig_OnUpdate orig, MWanderState mWanderState)
            {
                if (!ModSettings.useMonsterUpdateGroups || IsMonsterInActiveGroup(mWanderState.monster))
                {
                    orig.Invoke(mWanderState);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @NoIntroAnimation

            private static void HookNoIntroAnimation(On.NoIntroAnimation.orig_OnHidingEventTriggered orig, NoIntroAnimation noIntroAnimation)
            {
                HidingSpot componentInChildren = ((MonsterHidingEvents)noIntroAnimation).GetComponentInChildren<HidingSpot>();
                if (componentInChildren != null)
                {
                    lastMonsterSentMessage.gameObject.SendMessage("Mec_OnFinishIntro", SendMessageOptions.DontRequireReceiver);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @NoSearchAnimation

            private static void HookNoSearchAnimation(On.NoSearchAnimation.orig_OnHidingEventTriggered orig, NoSearchAnimation noSearchAnimation)
            {
                HidingSpot componentInChildren = ((MonsterHidingEvents)noSearchAnimation).GetComponentInChildren<HidingSpot>();
                if (componentInChildren != null)
                {
                    lastMonsterSentMessage.gameObject.SendMessage("Mec_OnFinishSearch", SendMessageOptions.DontRequireReceiver);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @PitTrap

            private static void HookPitTrap()
            {
                On.PitTrap.OnTriggerEnter += new On.PitTrap.hook_OnTriggerEnter(HookPitTrapOnTriggerEnter);
                On.PitTrap.OnTriggerExit += new On.PitTrap.hook_OnTriggerExit(HookPitTrapOnTriggerExit);
            }

            private static void HookPitTrapOnTriggerEnter(On.PitTrap.orig_OnTriggerEnter orig, PitTrap pitTrap, Collider _collider)
            {
                if (!pitTrap.destroyed)
                {
                    if (_collider.name.Contains("humanBody"))
                    {
                        pitTrap.player = _collider.gameObject.GetComponentInParent<NewPlayerClass>();
                        AudioSystem.PlaySound("Noises/Footsteps/Metal");
                        pitTrap.playerCount++;
                        if (pitTrap.playerCount == 1)
                        {
                            ((MonoBehaviour)pitTrap).StartCoroutine("PlayerTriggerStay");
                        }
                    }
                    if (_collider.transform.root.tag == "Monster" && !pitTrap.checkingMonster)
                    {
                        Monster monster = _collider.gameObject.GetComponentInParent<Monster>();
                        if (monster != null && monster.MonsterType != Monster.MonsterTypeEnum.Fiend)
                        {
                            PitTrap.monster = _collider.gameObject.GetComponent<FSM>();
                            pitTrap.checkingMonster = true;
                            ((MonoBehaviour)pitTrap).StartCoroutine("MonsterTriggerStay");
                        }
                    }
                }
            }

            private static void HookPitTrapOnTriggerExit(On.PitTrap.orig_OnTriggerExit orig, PitTrap pitTrap, Collider _collider)
            {
                if (_collider.name.Contains("humanBody"))
                {
                    pitTrap.playerCount--;
                    if (pitTrap.playerCount == 0)
                    {
                        ((MonoBehaviour)pitTrap).StopCoroutine("PlayerTriggerStay");
                    }
                }
                if (_collider.transform.root.tag == "Monster" && _collider.gameObject.GetComponent<FSM>() == PitTrap.monster)
                {
                    pitTrap.checkingMonster = false;
                    ((MonoBehaviour)pitTrap).StopCoroutine("MonsterTriggerStay");
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @PlayMonsterAnimation

            private static void HookPlayMonsterAnimation()
            {
                On.PlayMonsterAnimation.Play += new On.PlayMonsterAnimation.hook_Play(HookPlayMonsterAnimationPlay);
                On.PlayMonsterAnimation.PlayEffect += new On.PlayMonsterAnimation.hook_PlayEffect(HookPlayMonsterAnimationPlayEffect);
            }

            private static void HookPlayMonsterAnimationPlay(On.PlayMonsterAnimation.orig_Play orig, PlayMonsterAnimation playMonsterAnimation, PlayMonsterAnimation.MonsterAnimation _animation)
            {
                if (lastMonsterSentMessage.GetComponent<AnimationControl>().fixedEffectAnimation == PlayMonsterAnimation.MonsterEffectAnimation.None && lastMonsterSentMessage.GetComponent<AnimationControl>().fixedEffectAnimation == PlayMonsterAnimation.MonsterEffectAnimation.None)
                {
                    Debug.Log("Fixed animation is " + _animation);
                    lastMonsterSentMessage.GetComponent<AnimationControl>().fixedAnimation = _animation;
                }
            }

            private static void HookPlayMonsterAnimationPlayEffect(On.PlayMonsterAnimation.orig_PlayEffect orig, PlayMonsterAnimation playMonsterAnimation, PlayMonsterAnimation.MonsterEffectAnimation _animation)
            {
                if (lastMonsterSentMessage.GetComponent<AnimationControl>().fixedEffectAnimation == PlayMonsterAnimation.MonsterEffectAnimation.None && lastMonsterSentMessage.GetComponent<AnimationControl>().fixedEffectAnimation == PlayMonsterAnimation.MonsterEffectAnimation.None)
                {
                    Debug.Log("Fixed effect animation is " + _animation);
                    lastMonsterSentMessage.GetComponent<AnimationControl>().fixedEffectAnimation = _animation;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @References

            private static void FillBrutes()
            {
                if (monsterList != null)
                {
                    // Find the number of Brutes.
                    int size = FindNumberOfSpecificMonsters(Monster.MonsterTypeEnum.Brute);

                    // Initialise the reference and instance IDs lists.
                    brutes = new List<GameObject>();
                    brutesMonsterComponents = new List<Monster>();
                    brutesInstanceIDs = new Dictionary<int, int>();
                    if (size > 0)
                    {
                        // Assign the references and instance IDs.
                        FillSpecificMonsterLists(ref brutes, ref brutesMonsterComponents, ref brutesInstanceIDs, Monster.MonsterTypeEnum.Brute, size); // Pass the size, not ModSettings.numberOfBrutes as that does not include random monsters.
                    }
                }
            }

            private static void FillHunters()
            {
                if (monsterList != null)
                {
                    // Find the number of Hunters.
                    int size = FindNumberOfSpecificMonsters(Monster.MonsterTypeEnum.Hunter);

                    // Initialise the reference and instance IDs lists.
                    hunters = new List<GameObject>();
                    huntersMonsterComponents = new List<Monster>();
                    huntersInstanceIDs = new Dictionary<int, int>();
                    if (size > 0)
                    {
                        // Assign the references and instance IDs.
                        FillSpecificMonsterLists(ref hunters, ref huntersMonsterComponents, ref huntersInstanceIDs, Monster.MonsterTypeEnum.Hunter, size);  // Pass the size, not ModSettings.numberOfHunters as that does not include random monsters.
                    }
                }
            }

            private static void FillFiends()
            {
                if (monsterList != null)
                {
                    // Find the number of Fiends.
                    int size = FindNumberOfSpecificMonsters(Monster.MonsterTypeEnum.Fiend);

                    // Initialise the reference and instance IDs lists.
                    fiends = new List<GameObject>();
                    fiendsMonsterComponents = new List<Monster>();
                    fiendsInstanceIDs = new Dictionary<int, int>();
                    if (size > 0)
                    {
                        // Assign the references and instance IDs.
                        FillSpecificMonsterLists(ref fiends, ref fiendsMonsterComponents, ref fiendsInstanceIDs, Monster.MonsterTypeEnum.Fiend, size);  // Pass the size, not ModSettings.numberOfFiends as that does not include random monsters.
                    }
                }
            }

            private static int FindNumberOfSpecificMonsters(Monster.MonsterTypeEnum specificMonsterType)
            {
                Debug.Log("Specific monster type is " + specificMonsterType);
                int size = 0;

                try
                {
                    // Find the number of monsters of the given type.
                    for (int i = 0; i < (int)monsterList.Count; i++)
                    {
                        if (monsterListMonsterComponents[i].MonsterType == specificMonsterType)
                        {
                            size++;
                        }
                    }
                }
                catch
                {
                    Debug.Log("Error while increasing size in FindNumberOfSpecificMonsters");
                }
                return size;
            }

            private static void FillSpecificMonsterLists(ref List<GameObject> specificMonsterList, ref List<Monster> specificMonsterListMonsterComponents, ref Dictionary<int, int> specificMonsterInstanceIDs, Monster.MonsterTypeEnum specificMonsterType, int specificNumberOfMonsters)
            {
                // As the specific lists are passed by direct reference, changing the parameter lists will change the original lists too.
                if (specificMonsterList != null && monsterList != null)
                {
                    // Fill the passed lists.
                    for (int monsterNumber = 0, specificMonsterNumber = 0; specificMonsterNumber < specificNumberOfMonsters; monsterNumber++)
                    {
                        if (monsterListMonsterComponents[monsterNumber].MonsterType == specificMonsterType)
                        {
                            specificMonsterList.Add(monsterList[monsterNumber]);
                            specificMonsterListMonsterComponents.Add(monsterListMonsterComponents[monsterNumber]);
                            specificMonsterInstanceIDs.Add(monsterListMonsterComponents[monsterNumber].GetInstanceID(), specificMonsterNumber);
                            specificMonsterNumber++;
                        }
                    }
                }
                else
                {
                    UnityEngine.Debug.Log("The monster list to be filled is not initialised (it is null).");
                }
            }

            private static void SendHidingMessage(MonsterHidingEvents.HidingEvent _hidingEvent, HidingSpot hidingSpot, GameObject monsterGameObject)
            {
                if (hidingSpot.pointSelection != null)
                {
                    Debug.Log("Sent: " + _hidingEvent);
                    hidingSpot.pointSelection.SendMessageUpwards(_hidingEvent.ToString("g"), monsterGameObject, SendMessageOptions.DontRequireReceiver);
                }
            }

            private static List<int> monsterGroups;
            private static int numberOfMonstersPerGroup;
            public static int groupCounter = 1;

            private static void DeclareMonsterGroups()
            {
                numberOfMonstersPerGroup = (int)Math.Round((float)ModSettings.numberOfMonsters / ModSettings.NumberOfMonsterUpdateGroups, 0);
                Debug.Log("Number of monsters per group is " + numberOfMonstersPerGroup);

                monsterGroups = new List<int>(new int[ModSettings.numberOfMonsters]);
                for (int i = 0; i < monsterGroups.Count; i++)
                {
                    monsterGroups[i] = FindMonsterGroup(i);
                }
            }

            private static bool IsMonsterInActiveGroup(Monster monster)
            {
                if (monsterGroups != null)
                {
                    Debug.Log("Monster group is " + monsterGroups[MonsterNumber(monster.GetInstanceID())] + " and active group is " + groupCounter + " and group counter plus one is " + groupCounter + 1);
                }
                if (monsterGroups != null && monsterGroups[MonsterNumber(monster.GetInstanceID())] == groupCounter + 1)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            private static int FindMonsterGroup(int monsterNumber)
            {
                int monsterGroup = 1;
                for (int i = 2; i <= ModSettings.NumberOfMonsterUpdateGroups; i++)
                {
                    int maximumMonsterNumberInGroup = numberOfMonstersPerGroup * i;
                    int minimumMonsterNumberInGroup = maximumMonsterNumberInGroup - numberOfMonstersPerGroup;

                    if (monsterNumber >= minimumMonsterNumberInGroup && monsterNumber < maximumMonsterNumberInGroup)
                    {
                        monsterGroup = i;
                    }
                }
                return monsterGroup;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @RenderOnce

            public static bool HookRenderOnceIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookRenderOnce((RenderOnce)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookRenderOnce(RenderOnce renderOnce)
            {
                Vector3[] monsterPositions = new Vector3[monsterList.Count];
                if (!ModSettings.doNotPreRenderMonsters)
                {
                    for (int i = 0; i < monsterList.Count; i++)
                    {
                        monsterPositions[i] = monsterList[i].transform.position;
                        monsterList[i].transform.position = ((LevelGenerationPP)renderOnce).GetComponent<Camera>().transform.position + ((LevelGenerationPP)renderOnce).GetComponent<Camera>().transform.forward * 20f;
                        monsterList[i].SetActive(true);
                    }
                }
                float shadowDistance = QualitySettings.shadowDistance;
                QualitySettings.shadowDistance = 0f;
                if (RenderOnce.tex == null)
                {
                    RenderOnce.tex = new RenderTexture(1024, 1024, 24, RenderTextureFormat.ARGB32);
                }
                else
                {
                    Debug.Log("reused rendertexture");
                }
                Debug.Log("Is created: " + RenderOnce.tex.IsCreated());
                if (!RenderOnce.tex.IsCreated())
                {
                    RenderOnce.tex.Create();
                }
                ((LevelGenerationPP)renderOnce).GetComponent<Camera>().targetTexture = RenderOnce.tex;
                if (!RenderOnce.tex.IsCreated())
                {
                    RenderOnce.tex.Create();
                }
                ((LevelGenerationPP)renderOnce).GetComponent<Camera>().RenderWithShader(Shader.Find("Bumped Diffuse"), string.Empty);
                if (!ModSettings.doNotPreRenderMonsters)
                {
                    for (int i = 0; i < monsterList.Count; i++)
                    {
                        monsterList[i].SetActive(false);
                        monsterList[i].transform.position = monsterPositions[i];
                    }
                }
                QualitySettings.shadowDistance = shadowDistance;
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ResizeCones

            private static void HookResizeCones()
            {
                On.ResizeCones.Start += new On.ResizeCones.hook_Start(HookResizeConesStart); // // If ResizeCones is a subcomponent of the monsters.
                                                                                             // On.ResizeCones.Update += new On.ResizeCones.hook_Update(HookResizeConesUpdate); // If ResizeCones is independent of the monsters.
            }

            private static void HookResizeConesStart(On.ResizeCones.orig_Start orig, ResizeCones resizeCones)
            {
                try
                {
                    Debug.Log("Starting ResizeCones");
                    resizeCones.cones = ((MonoBehaviour)resizeCones).GetComponentsInChildren<MeshRenderer>();
                    resizeCones.scale = resizeCones.cones[0].gameObject.transform.localScale;
                    resizeCones.monsterLight = ((MonoBehaviour)resizeCones).GetComponentInChildren<Light>();
                    resizeCones.stateManager = ((MonoBehaviour)resizeCones).GetComponentInParent<MState>();
                    resizeCones.currentState = resizeCones.stateManager.Fsm.Current;
                    resizeCones.coneRange = 1f;
                    Debug.Log("ResizeCones Monster State ID: " + resizeCones.stateManager.GetInstanceID());
                    //Debug.Log(((MonoBehaviour)resizeCones).GetComponentInParent<Monster>().GetInstanceID());
                }
                catch
                {
                    Debug.Log("Error in ResizeCones");
                    orig.Invoke(resizeCones);
                }
            }

            /*
            private static void HookResizeConesUpdate(On.ResizeCones.orig_Update orig, ResizeCones resizeCones)
            {
                foreach (MState mState in monsterListStates)
                {
                    if (!ModSettings.useMonsterUpdateGroups || IsMonsterInActiveGroup(mState.monster))
                    {
                        resizeCones.stateManager = mState;
                        resizeCones.currentState = resizeCones.stateManager.Fsm.Current;
                        orig.Invoke(resizeCones);
                    }
                }
            }
            */

            /*----------------------------------------------------------------------------------------------------*/
            // @RipOffCurtain

            private static void HookRipOffCurtain(On.RipOffCurtain.orig_Mec_OnGrabDoor orig, RipOffCurtain ripOffCurtain)
            {
                ripOffCurtain.showerCurtain.OnHandGrab();
                ripOffCurtain.grabbed = true;
                ripOffCurtain.attachPoint = lastMonsterSentMessage.gameObject.GetComponentInChildren<AttachPoint>();
                ripOffCurtain.OnClothGrab();
                ((MonoBehaviour)ripOffCurtain).StartCoroutine(ripOffCurtain.MonsterHold());
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @RipOutPlayer

            private static void HookRipOutPlayer()
            {
                On.RipOutPlayer.Mec_OnGrabPlayer += new On.RipOutPlayer.hook_Mec_OnGrabPlayer(HookRipOutPlayerMec_OnGrabPlayer);
                On.RipOutPlayer.Mec_OnReleasePlayer += new On.RipOutPlayer.hook_Mec_OnReleasePlayer(HookRipOutPlayerMec_OnReleasePlayer);
            }

            private static void HookRipOutPlayerMec_OnGrabPlayer(On.RipOutPlayer.orig_Mec_OnGrabPlayer orig, RipOutPlayer ripOutPlayer)
            {
                if (lastMonsterSentMessage != null)
                {
                    ripOutPlayer.monster = lastMonsterSentMessage;
                    if (ModSettings.enableMultiplayer)
                    {
                        ripOutPlayer.player = ripOutPlayer.monster.player.GetComponent<NewPlayerClass>();
                        ripOutPlayer.DOH = ripOutPlayer.player.GetComponent<DraggedOutHiding>();
                    }
                }
                orig.Invoke(ripOutPlayer);
            }

            private static void HookRipOutPlayerMec_OnReleasePlayer(On.RipOutPlayer.orig_Mec_OnReleasePlayer orig, RipOutPlayer ripOutPlayer)
            {
                /*
                if (lastMonsterSentMessage != null)
                {
                    ripOutPlayer.monster = lastMonsterSentMessage;
                }
                */
                FollowTransform2 component = ripOutPlayer.player.GetComponent<FollowTransform2>();
                component.enabled = false;
                ripOutPlayer.monster.LastSeenPlayerPosition = ripOutPlayer.player.transform.position;
                ripOutPlayer.monster.GetMainCollider.enabled = true;
                ripOutPlayer.player.IsGrabbed = false;
                ripOutPlayer.player.Motor.enabled = true;
                ripOutPlayer.player.Motor.useGravity = true;
                if (ripOutPlayer.setToProne)
                {
                    ripOutPlayer.player.SetToProne();
                    TriggerObjectives.instance.monsterThrow = true;
                }
                if (ripOutPlayer.player.transform.position.y < ripOutPlayer.monster.transform.position.y - 0.2f)
                {
                    ripOutPlayer.player.transform.position = new Vector3(ripOutPlayer.player.transform.position.x, ripOutPlayer.monster.transform.position.y + 0.05f, ripOutPlayer.player.transform.position.z);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Room - Unused. Breaks Level Generation.

            /*
            private static int HookRoom(On.Room.orig_ChooseRandomRoomID orig, Room room)
            {
                room.roomChances = new List<float>();
                int randomRoomCount = 0;
                if (room.roomModels.Count > 1)
                {
                    if (room.HasTag("StartRoom"))
                    {
                        if (room.regionChoice == room.regionsCustomIDs[1])
                        {
                            return 1;
                        }
                        return 0;
                    }
                    else
                    {
                        for (int i = 0; i < room.roomModels.Count; i++)
                        {
                            if (room.roomModels[i].defineSpawnArea && room.regionNode.x >= room.roomModels[i].lowerSpawnPosLimit.x && room.regionNode.x <= room.roomModels[i].upperSpawnPosLimit.x && room.regionNode.y >= room.roomModels[i].lowerSpawnPosLimit.y && room.regionNode.y <= room.roomModels[i].upperSpawnPosLimit.y && room.regionNode.z >= room.roomModels[i].lowerSpawnPosLimit.z && room.regionNode.z <= room.roomModels[i].upperSpawnPosLimit.z)
                            {
                                return i;
                            }
                        }
                        for (int i = 0; i < room.roomModels.Count; i++)
                        {
                            if (!Room.roomModelCountDictionary.ContainsKey(room.roomModels[i].model))
                            {
                                Room.roomModelCountDictionary.Add(room.roomModels[i].model, 0);
                            }
                        }
                        for (int i = 0; i < room.roomModels.Count; i++)
                        {
                            if (room.roomModels[i].defineSpawnArea)
                            {
                                room.roomModels.RemoveAt(i);
                                i--;
                            }
                        }
                        if (!room.roomModelMinMaxCounts)
                        {
                            randomRoomCount = UnityEngine.Random.Range(0, room.roomModels.Count);
                            if (!room.equalChance)
                            {
                                if (room.difMonstChances)
                                {
                                    if (ModSettings.numberOfBrutes > 0)
                                    {
                                        for (int i = 0; i < room.roomModels.Count; i++)
                                        {
                                            room.roomChances.Add(room.roomModels[i].bruteChance);
                                        }
                                        randomRoomCount = MathHelper.RandomSelection(room.roomChances);
                                    }
                                    if (ModSettings.numberOfHunters > 0)
                                    {
                                        for (int i = 0; i < room.roomModels.Count; i++)
                                        {
                                            room.roomChances.Add(room.roomModels[i].hunterChance);
                                        }
                                        randomRoomCount = MathHelper.RandomSelection(room.roomChances);
                                    }
                                    if (ModSettings.numberOfFiends > 0)
                                    {
                                        for (int i = 0; i < room.roomModels.Count; i++)
                                        {
                                            room.roomChances.Add(room.roomModels[i].fiendChance);
                                        }
                                        randomRoomCount = MathHelper.RandomSelection(room.roomChances);
                                    }
                                }
                                else
                                {
                                    for (int i = 0; i < room.roomModels.Count; i++)
                                    {
                                        room.roomChances.Add(room.roomModels[i].spawnChance);
                                    }
                                    randomRoomCount = MathHelper.RandomSelection(room.roomChances);
                                }
                            }
                        }
                        else
                        {
                            bool flag = false;
                            for (int i = 0; i < room.roomModels.Count; i++)
                            {
                                if (Room.roomModelCountDictionary[room.roomModels[i].model] < room.roomModels[i].model.GetComponent<ModelSpawnCountData>().minCount)
                                {
                                    randomRoomCount = i;
                                    flag = true;
                                    break;
                                }
                            }
                            if (!flag)
                            {
                                List<ModelData> list = room.roomModels;
                                while (!flag)
                                {
                                    randomRoomCount = UnityEngine.Random.Range(0, list.Count);
                                    if (!room.equalChance)
                                    {
                                        for (int num4 = 0; num4 < list.Count; num4++)
                                        {
                                            room.roomChances.Add(list[num4].spawnChance);
                                        }
                                        randomRoomCount = MathHelper.RandomSelection(room.roomChances);
                                    }
                                    if (list[randomRoomCount].model.GetComponent<ModelSpawnCountData>().maxCount > Room.roomModelCountDictionary[room.roomModels[randomRoomCount].model])
                                    {
                                        break;
                                    }
                                    list.RemoveAt(randomRoomCount);
                                    if (list.Count == 0)
                                    {
                                        Debug.LogError("No model to spawn - max models met.");
                                    }
                                }
                            }
                            Dictionary<GameObject, int> dictionary;
                            GameObject model;
                            (dictionary = Room.roomModelCountDictionary)[model = room.roomModels[randomRoomCount].model] = dictionary[model] + 1;
                        }
                    }
                }
                return randomRoomCount;
            }
            */

            /*----------------------------------------------------------------------------------------------------*/
            // @SealedDoorBreach

            private static void HookSealedDoorBreach(On.SealedDoorBreach.orig_OnDoorDestroy orig, SealedDoorBreach sealedDoorBreach)
            {
                if (monsterListMonsterComponents != null)
                {
                    foreach (Monster monster in monsterListMonsterComponents)
                    {
                        monster.TheSubAlarm.RoomBreached = true;
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Sub

            private static void HookSub(On.Sub.orig_Update orig, Sub sub)
            {
                /*
                if (orig != null && sub != null && EscapeChecker.Completeness.Complete != null)
                {
                    if (sub.submarineEsc.subWeldFixed != null && sub.comWelder != null)
                    {
                        if (sub.submarineEsc.subWeldFixed)
                        {
                            sub.comWelder = EscapeChecker.Completeness.Complete;
                            sub.UpdateList();
                        }
                    }
                    if (sub.submarineEsc.subBatteryFixed != null && sub.comBattery != null)
                    {
                        if (sub.submarineEsc.subBatteryFixed)
                        {
                            sub.comBattery = EscapeChecker.Completeness.Complete;
                            sub.UpdateList();
                        }
                    }
                    if (sub.submarineEsc.subHeadLightsFixed != null & sub.comHeadLights != null)
                    {
                        if (sub.submarineEsc.subHeadLightsFixed)
                        {
                            sub.comHeadLights = EscapeChecker.Completeness.Complete;
                            sub.UpdateList();
                        }
                    }
                }
                */
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SubAlarm

            private static void HookSubAlarm()
            {
                On.SubAlarm.Start += new On.SubAlarm.hook_Start(HookSubAlarmStart);
                On.SubAlarm.StartTheEvent += new On.SubAlarm.hook_StartTheEvent(HookSubAlarmStartTheEvent);
            }

            private static void HookSubAlarmStart(On.SubAlarm.orig_Start orig, SubAlarm subAlarm)
            {
                subAlarm.subRoom = ((MonoBehaviour)subAlarm).transform.GetComponentInParent<Room>();
                subAlarm.submarine = subAlarm.subRoom.GetComponentInChildren<Submarine>();
                subAlarm.subTimerCharging = 0f;
                subAlarm.seqOfEvents = ((MonoBehaviour)subAlarm).GetComponent<SubEventSequence>();
                subAlarm.roomPowered = false;
                subAlarm.eventStarted = false;
                subAlarm.lib = "Noises/Submarine/Loud Silence";
                subAlarm.moveMonsterHere = subAlarm.subRoom.GetComponentInChildren<MonsterStarter>().transform;

                foreach (Monster monster in monsterListMonsterComponents)
                {
                    monster.TheSubAlarm = subAlarm;
                }
                subAlarm.monsterScript = monsterListMonsterComponents[0];
            }

            private static void HookSubAlarmStartTheEvent(On.SubAlarm.orig_StartTheEvent orig, SubAlarm subAlarm)
            {
                if ((subAlarm.roomPowered && subAlarm.submarine.subFixed) || subAlarm.forGary)
                {
                    Objectives.Tasks("SubRepair").Tasks("ButtonStartEvent").Complete();

                    ((MonoBehaviour)subAlarm).GetComponent<ChangeShader>().Deactivate();
                    ((MonoBehaviour)subAlarm).GetComponent<ChangeShader>().enabled = false;
                    ((MonoBehaviour)subAlarm).GetComponent<BoxCollider>().enabled = false;
                    ((MonoBehaviour)subAlarm).GetComponent<ChangeShader>().enabled = false;
                    subAlarm.submarine.eventStarted = true;
                    subAlarm.subTimerCharging = subAlarm.SetTime;
                    subAlarm.seqOfEvents.EventStarted();
                    foreach (Monster monster in monsterListMonsterComponents)
                    {
                        if (!monster.IsMonsterActive)
                        {
                            monster.Starter.Spawn();
                        }
                        if (monster.RoomDetect.CurrentRoom != monster.PlayerDetectRoom.GetRoom)
                        {
                            subAlarm.monsterScript = monster;
                            subAlarm.MonsterChanges();
                        }
                    }
                    GameObject gameObject = ((MonoBehaviour)subAlarm).transform.FindChild("MonsterTrigger").gameObject;
                    gameObject.GetComponent<MonsterReaction>().enabled = true;
                    gameObject.GetComponent<Collider>().enabled = true;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Submarine

            private static void HookSubmarine(On.Submarine.orig_Start orig, Submarine submarine)
            {
                orig.Invoke(submarine);
                for (int i = 1; i < monsterList.Count; i++)
                {
                    monsterList[i].GetComponentsInChildren<Sub>(true)[0].submarineEsc = submarine;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Trolley

            private static void HookTrolley(On.Trolley.orig_BlockingCode orig, Trolley trolley)
            {
                if (trolley.trolleyCollider != null)
                {
                    if (brutes != null)
                    {
                        foreach (GameObject monsterObject in brutes)
                        {
                            MDoorBashState mDoorBasher = monsterObject.GetComponent<MDoorBashState>();

                            if (mDoorBasher != null)
                            {
                                if (mDoorBasher.TargetDoor != null)
                                {
                                    trolley.TryToBlockDoor(mDoorBasher.TargetDoor);
                                }
                                if (trolley.prevBlockedDoor != null && trolley.prevBlockedDoor != mDoorBasher.TargetDoor)
                                {
                                    trolley.TryToBlockDoor(trolley.prevBlockedDoor);
                                }
                            }
                        }
                    }
                    if (hunters != null)
                    {
                        foreach (GameObject monsterObject in hunters)
                        {
                            MVentFrenzyState mVentFrenzy = monsterObject.GetComponent<MVentFrenzyState>();

                            if (mVentFrenzy.TargetVent != null)
                            {
                                trolley.TryToBlockVent(mVentFrenzy.TargetVent);
                            }
                            if (trolley.prevBlockedVent != null && trolley.prevBlockedVent != mVentFrenzy.TargetVent)
                            {
                                trolley.TryToBlockVent(trolley.prevBlockedVent);
                            }
                        }
                    }
                    if (fiends != null)
                    {
                        foreach (GameObject monsterObject in fiends)
                        {
                            MFiendSubDoors mFiendDoors = monsterObject.GetComponent<MFiendSubDoors>();

                            if (mFiendDoors.TargetDoor != null)
                            {
                                trolley.TryToBlockDoor(mFiendDoors.TargetDoor);
                            }
                            if (trolley.prevBlockedDoor != null)
                            {
                                trolley.TryToBlockDoor(trolley.prevBlockedDoor);
                            }
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Vision

            private static void HookVision()
            {
                On.Vision.Start += new On.Vision.hook_Start(HookVisionStart);
                On.Vision.PlayerVision += new On.Vision.hook_PlayerVision(HookVisionPlayerVision);
            }

            private static void HookVisionStart(On.Vision.orig_Start orig, Vision vision)
            {
                vision.monster = ((MonoBehaviour)vision).GetComponentInParent<Monster>();
                vision.player = vision.monster.player;
                vision.playerLayerInt = LayerMask.NameToLayer(vision.playerLayer);
                vision.lightLayerInt = LayerMask.NameToLayer(vision.lightVolume);
            }

            private static void HookVisionPlayerVision(On.Vision.orig_PlayerVision orig, Vision vision)
            {
                if (vision.monsterCam == null)
                {
                    vision.monsterCam = monsterList[MonsterNumber(vision.monster.GetInstanceID())].GetComponentInChildren<Camera>();
                }

                if (Physics.CheckSphere(((MonoBehaviour)vision).transform.position, 0.01f, vision.raycastMask) || ModSettings.InvisibleMode || (ModSettings.foggyShip && ModSettings.monsterVisionAffectedByFog && ManyMonstersMode.PlayerToMonsterDistance(vision.monster) > ModSettings.fogFarDistance))
                {
                    vision.playerTotal = 0f;
                    vision.lightTotal = 0f;
                }
                else if (!vision.monster.BeenBlinded)
                {
                    Vision.VisionType visionType = vision.visionType;
                    if (visionType != Vision.VisionType.Rayblast)
                    {
                        if (visionType == Vision.VisionType.Cone)
                        {
                            if (!ModSettings.enableMultiplayer && vision.monster.player != null)
                            {
                                vision.playerTotal = 0f;
                                vision.lightTotal = 0f;
                                RaycastHit raycastHit = default(RaycastHit);
                                Vector3 vector = vision.monster.player.GetComponent<NewPlayerClass>().Motor.CController.center + vision.monster.player.transform.position;
                                if (vision.monsterCam != null)
                                {
                                    if (GeoHelper.InsideCone(((MonoBehaviour)vision).transform.position, vector, ((MonoBehaviour)vision).transform.forward, vision.coneAngle, 1000f))
                                    {
                                        Ray ray = new Ray(vision.monsterCam.transform.position, (vector - vision.monsterCam.transform.position).normalized);
                                        if (Physics.Raycast(ray, out raycastHit, 1000f, vision.raycastMask))
                                        {
                                            float maxDistance = Vector3.Distance(ray.origin, raycastHit.point);
                                            ray.origin = raycastHit.point;
                                            ray.direction *= -1f;
                                            if (!Physics.Raycast(ray, maxDistance, vision.raycastMask))
                                            {
                                                Collider collider = raycastHit.collider;
                                                GameObject gameObject = collider.gameObject;
                                                if (gameObject.layer == vision.playerLayerInt)
                                                {
                                                    vision.playerTotal = 1f;
                                                }
                                                else if (gameObject.layer == vision.lightLayerInt)
                                                {
                                                    vision.lightTotal = 1f;
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        Ray ray2 = new Ray(vision.monsterCam.transform.position, vision.monsterCam.transform.forward);
                                        if (Physics.Raycast(ray2, out raycastHit, 1000f, vision.raycastMask))
                                        {
                                            Collider collider2 = raycastHit.collider;
                                            GameObject gameObject2 = collider2.gameObject;
                                            if (gameObject2.layer == vision.lightLayerInt)
                                            {
                                                vision.lightTotal = 1f;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (MultiplayerMode.crewPlayers != null)
                                {
                                    for (int i = 0; i < MultiplayerMode.crewPlayers.Count; i++)
                                    {
                                        if (!MultiplayerMode.playersDowned[i])
                                        {
                                            vision.playerTotal = 0f;
                                            vision.lightTotal = 0f;
                                            RaycastHit raycastHit = default(RaycastHit);
                                            Vector3 crewPlayerVector = MultiplayerMode.crewPlayers[i].Motor.CController.center + MultiplayerMode.crewPlayers[i].transform.position;
                                            if (vision.monsterCam != null)
                                            {
                                                if (GeoHelper.InsideCone(((MonoBehaviour)vision).transform.position, crewPlayerVector, ((MonoBehaviour)vision).transform.forward, vision.coneAngle, 1000f))
                                                {
                                                    Ray ray = new Ray(vision.monsterCam.transform.position, (crewPlayerVector - vision.monsterCam.transform.position).normalized);
                                                    if (Physics.Raycast(ray, out raycastHit, 1000f, vision.raycastMask))
                                                    {
                                                        float maxDistance = Vector3.Distance(ray.origin, raycastHit.point);
                                                        ray.origin = raycastHit.point;
                                                        ray.direction *= -1f;
                                                        if (!Physics.Raycast(ray, maxDistance, vision.raycastMask))
                                                        {
                                                            Collider collider = raycastHit.collider;
                                                            GameObject gameObject = collider.gameObject;
                                                            if (gameObject.layer == vision.playerLayerInt)
                                                            {
                                                                NewPlayerClass npcFromCollider = collider.GetComponentInParent<NewPlayerClass>();
                                                                int playerNumber = MultiplayerMode.PlayerNumber(npcFromCollider.GetInstanceID());
                                                                if (npcFromCollider != null && playerNumber == i)
                                                                {
                                                                    vision.monster.player = MultiplayerMode.crewPlayers[i].gameObject;
                                                                    vision.monster.playerRoomDetect = vision.monster.player.GetComponentInChildren<DetectRoom>();
                                                                    vision.monster.hiding = vision.monster.player.GetComponentInChildren<Hiding>();
                                                                    vision.monster.anievents.playerHealth = vision.monster.player.GetComponentInChildren<PlayerHealth>();
                                                                    vision.player = vision.monster.player;
                                                                    vision.playerTotal = 1f;
                                                                    if (ModSettings.logDebugText)
                                                                    {
                                                                        Debug.Log(vision.monster.monsterType + " (monster number " + MonsterNumber(vision.monster.GetInstanceID()) + ") saw player number " + MultiplayerMode.PlayerNumber(vision.monster.player.GetComponent<NewPlayerClass>().GetInstanceID()) + "! Breaking out of detection loop.");
                                                                    }
                                                                    break;
                                                                }
                                                            }
                                                            else if (gameObject.layer == vision.lightLayerInt)
                                                            {
                                                                vision.lightTotal = 1f;
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    Ray ray2 = new Ray(vision.monsterCam.transform.position, vision.monsterCam.transform.forward);
                                                    if (Physics.Raycast(ray2, out raycastHit, 1000f, vision.raycastMask))
                                                    {
                                                        Collider collider2 = raycastHit.collider;
                                                        GameObject gameObject2 = collider2.gameObject;
                                                        if (gameObject2.layer == vision.lightLayerInt)
                                                        {
                                                            vision.lightTotal = 1f;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        vision.RenderMap();
                        if (vision.renderTexture != null)
                        {
                            vision.playerTotal = 0f;
                            vision.lightTotal = 0f;
                            float num = 0f;
                            RaycastHit raycastHit2 = default(RaycastHit);
                            Vector3 zero = Vector3.zero;
                            int num2 = 0;
                            while ((float)num2 < vision.eyeResolution.x)
                            {
                                int num3 = 0;
                                while ((float)num3 < vision.eyeResolution.y)
                                {
                                    zero.x = (float)num2;
                                    zero.y = (float)num3;
                                    Ray ray3 = vision.eyesCamera.ScreenPointToRay(zero);
                                    ray3.direction = ray3.direction.normalized * 1000f;
                                    if (Physics.Raycast(ray3, out raycastHit2, (float)vision.raycastMask))
                                    {
                                        Collider collider3 = raycastHit2.collider;
                                        GameObject gameObject3 = collider3.gameObject;
                                        if (gameObject3.layer == vision.playerLayerInt || gameObject3.layer == vision.lightLayerInt)
                                        {
                                            float num4 = 1f;
                                            if (gameObject3.layer == vision.playerLayerInt)
                                            {
                                                vision.playerTotal += num4;
                                            }
                                            else if (gameObject3.layer == vision.lightLayerInt)
                                            {
                                                vision.lightTotal += num4;
                                            }
                                            num += 1f;
                                        }
                                    }
                                    num3 += (int)vision.raycastResolution.y;
                                }
                                num2 += (int)vision.raycastResolution.x;
                            }
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
        }

    }
}
// ~End Of File