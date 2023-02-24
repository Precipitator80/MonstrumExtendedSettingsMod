// ~Beginning Of File
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;

namespace MonstrumExtendedSettingsMod
{

    public partial class ExtendedSettingsModScript
    {
        /*----------------------------------------------------------------------------------------------------*/
        // ~CrewVsMonsterMode

        private static class CrewVsMonsterMode
        {
            //private static Vector3 monsterPositionModifier = Vector3.zero;
            private static Vector3[] playerHidingHeightDifferences;
            private static Vector3[] originalPlayerScales;
            public static bool[] playersGoneIntoHiding;
            public static bool letAIControlMonster;
            private static List<float[]> monsterAbilityTimes;
            public static List<bool> monsterAbilityReady;
            public static List<bool> monsterUsingActiveAbility;
            private static Dictionary<int, int> currentAmbushPoint;
            public static bool setUpAtPlayerMonster;
            public static List<bool> monsterInHidingEvent;

            // #CrewVsMonsterModeAfterGenerationInitialisation
            public static void CrewVsMonsterModeAfterGenerationInitialisation()
            {
                playerHidingHeightDifferences = new Vector3[ModSettings.NumberOfPlayers];
                originalPlayerScales = new Vector3[ModSettings.NumberOfPlayers];
                playersGoneIntoHiding = new bool[ModSettings.NumberOfPlayers];
                //letAIControlMonster = false;
                monsterAbilityTimes = new List<float[]>();
                monsterAbilityReady = new List<bool>();
                monsterUsingActiveAbility = new List<bool>();
                currentAmbushPoint = new Dictionary<int, int>();
                setUpAtPlayerMonster = false;
                monsterInHidingEvent = new List<bool>();

                for (int i = 0; i < MultiplayerMode.monsterPlayers.Count; i++)
                {
                    // Run player 0 monster code after each player is created as the players are all instantiated from player 0.
                    Debug.Log("Switching player " + i + "'s gravity off as they should be a monster");
                    MultiplayerMode.monsterPlayers[i].Motor.useGravity = false;
                    //MultiplayerMode.monsterPlayers[i].Motor.isHaulted = true;
                    //MultiplayerMode.monsterPlayers[i].Motor.disableMove = true;
                    foreach (Light light in MultiplayerMode.monsterPlayers[i].GetComponentsInChildren<Light>())
                    {
                        light.enabled = false;
                    }
                    switch (ManyMonstersMode.monsterListMonsterComponents[i].MonsterType)
                    {
                        case Monster.MonsterTypeEnum.Brute:
                            {
                                monsterAbilityTimes.Add(new float[] { ModSettings.bruteAbilityActiveTime, ModSettings.bruteAbilityCooldownTime });
                                break;
                            }
                        case Monster.MonsterTypeEnum.Hunter:
                            {
                                currentAmbushPoint.Add(i, 0);
                                monsterAbilityTimes.Add(new float[] { ModSettings.hunterAbilityActiveTime, ModSettings.hunterAbilityCooldownTime });
                                break;
                            }
                        case Monster.MonsterTypeEnum.Fiend:
                            {
                                monsterAbilityTimes.Add(new float[] { ModSettings.fiendAbilityActiveTime, ModSettings.fiendAbilityCooldownTime });
                                break;
                            }
                        default:
                            {
                                monsterAbilityTimes.Add(new float[] { 5f, 30f });
                                break;
                            }
                    }

                    monsterAbilityReady.Add(true);
                    monsterUsingActiveAbility.Add(false);
                    monsterInHidingEvent.Add(false);
                }

                /*
                for(int i = 0; i < ModSettings.NumberOfPlayers; i++)
                {
                    playersGoneIntoHiding[i] = false;
                }
                */
            }

            // #InitialiseCrewVsMonsterMode
            public static void InitialiseCrewVsMonsterMode()
            {
                HookHiding();
                On.DoorInteract.OnHandGrab += new On.DoorInteract.hook_OnHandGrab(HookDoorInteractOnHandGrab);
                On.MonstDetectRoom.DoorChecks += new On.MonstDetectRoom.hook_DoorChecks(HookMonstDetectRoomDoorChecks);
                On.MonsterCameraControl.Update += new On.MonsterCameraControl.hook_Update(HookMonsterCameraControlUpdate);
                HookMovementControl();
                On.Journal.Interpolate += new On.Journal.hook_Interpolate(HookJournalInterpolate);
                On.MHideState.FinishedHiding += new On.MHideState.hook_FinishedHiding(HookMHideStateFinishedHiding);
                On.MInvestigateState.OnUpdate += new On.MInvestigateState.hook_OnUpdate(HookMInvestigateStateOnUpdate);
                On.DoorInteract.IsConditionMet += new On.DoorInteract.hook_IsConditionMet(HookDoorInteractIsConditionMet);
                On.Door.Toggle += new On.Door.hook_Toggle(HookDoorToggle);
                On.MDoorBashState.StateChanges += new On.MDoorBashState.hook_StateChanges(HookMDoorBashStateStateChanges);
                On.MFiendSubDoors.StateChanges += new On.MFiendSubDoors.hook_StateChanges(HookMFiendSubDoorsStateChanges);
                On.MVentFrenzyState.MoveMonster += new On.MVentFrenzyState.hook_MoveMonster(HookMVentFrenzyStateMoveMonster);
                On.MVentFrenzyState.MakeNoise += new On.MVentFrenzyState.hook_MakeNoise(HookMVentFrenzyStateMakeNoise);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @CrewVSMonsterModeActiveFeatures

            public static IEnumerator MonsterPlayerHidingSpotSearchCoroutine(Monster monster, int playerMonsterNumber)
            {
                if (monster.RoomDetect != null & monster.RoomDetect.CurrentRoom != null && monster.RoomDetect.CurrentRoom.HidingSpots != null && monster.RoomDetect.CurrentRoom.HidingSpots.Length > 0)
                {
                    monsterInHidingEvent[playerMonsterNumber] = true;
                    HidingSpot[] hidingSpots = monster.RoomDetect.CurrentRoom.HidingSpots;
                    HidingSpot closestHidingSpot = hidingSpots[0];
                    float closestHidingSpotDistance = Vector3.Distance(monster.transform.position, closestHidingSpot.transPoint);
                    for (int i = 1; i < hidingSpots.Length; i++)
                    {
                        float distanceToPotentialHidingSpot = Vector3.Distance(monster.transform.position, hidingSpots[i].transPoint);
                        if (distanceToPotentialHidingSpot < closestHidingSpotDistance)
                        {
                            closestHidingSpot = hidingSpots[i];
                            closestHidingSpotDistance = distanceToPotentialHidingSpot;
                        }
                    }
                    if (closestHidingSpotDistance < 2f)
                    {
                        monster.RoomSearcher.targetHidingSpot = closestHidingSpot;
                        //mRoomSearch.triesTimeout = 0;
                        //mRoomSearch.arrived = true;
                        //mRoomSearch.allowChase = false;
                        //mRoomSearch.lookTowardsNextTarget = false;
                        monster.MoveControl.LockRotation = true;
                        ManyMonstersMode.lastMonsterSentMessage = monster;
                        closestHidingSpot.SendHidingMessage(MonsterHidingEvents.HidingEvent.OnMonsterStartLerp);

                        while (!ManyMonstersMode.monstersFinishedLerpToHidingSpot[playerMonsterNumber])
                        {
                            yield return null;
                        }

                        Vector3 originalPosition = monster.transform.position;

                        //mRoomSearch.savedPlayerSpot = mRoomSearch.PlayerHidingSpot;
                        HidingSpot playerHidingSpot = monster.PlayerDetectRoom.PlayerHidingSpot(monster.PlayerDetectRoom.GetRoom.HidingSpots);
                        if (playerHidingSpot == closestHidingSpot)
                        {
                            ManyMonstersMode.monstersFinishedGrab[playerMonsterNumber] = false;
                            monster.PlayerDetectRoom.player.Motor.enabled = false;

                            int playerNumber = MultiplayerMode.PlayerNumber(monster.PlayerDetectRoom.player.GetInstanceID());
                            playersGoneIntoHiding[playerNumber] = false;
                            foreach (Renderer renderer in MultiplayerMode.newPlayerClasses[playerNumber].GetComponentsInChildren<Renderer>())
                            {
                                renderer.enabled = true;
                            }
                            foreach (Light light in MultiplayerMode.newPlayerClasses[playerNumber].GetComponentsInChildren<Light>())
                            {
                                light.enabled = true;
                            }

                            //monster.GetComponent<MState>().SendEvent("Search Room");
                            monster.StartCoroutine(ManyMonstersMode.StopMonstersFromKillingPlayer(3f, monster, true));
                            closestHidingSpot.SendHidingMessage(MonsterHidingEvents.HidingEvent.OnMonsterStartGrab);
                        }
                        else
                        {
                            //closestHidingSpot.SendHidingMessage(MonsterHidingEvents.HidingEvent.OnMonsterReachedHiding);

                            closestHidingSpot.SendHidingMessage(MonsterHidingEvents.HidingEvent.OnMonsterStartSearch);
                            closestHidingSpot.SendHidingMessage(MonsterHidingEvents.HidingEvent.OnMonsterReachedHiding);

                            //monster.GetAniEvents.Mec_GrabDoor();
                        }

                        Camera monsterCamera = monster.GetComponentInChildren<Camera>();
                        //Camera playerCamera = MultiplayerMode.PlayerCamera(MultiplayerMode.monsterPlayers[playerMonsterNumber]);

                        if (monster.MonsterType != Monster.MonsterTypeEnum.Fiend)
                        {
                            while (!ManyMonstersMode.monstersFinishedGrab[playerMonsterNumber] || monster.MoveControl.GetAniControl.fixedAnimation != PlayMonsterAnimation.MonsterAnimation.None)
                            {
                                MultiplayerMode.monsterPlayers[playerMonsterNumber].gameObject.transform.rotation = monsterCamera.transform.rotation;
                                //playerCamera.transform.rotation = monsterCamera.transform.rotation;
                                monster.transform.position = originalPosition;
                                yield return null;
                            }
                        }
                        else
                        {
                            while (!ManyMonstersMode.monstersFinishedGrab[playerMonsterNumber] || monster.MoveControl.GetAniControl.fixedAnimation != PlayMonsterAnimation.MonsterAnimation.None)
                            {
                                MultiplayerMode.monsterPlayers[playerMonsterNumber].gameObject.transform.rotation = monsterCamera.transform.rotation * Quaternion.Euler(0f, 0f, 90f);
                                //playerCamera.transform.rotation = monsterCamera.transform.rotation;
                                monster.transform.position = originalPosition;
                                yield return null;
                            }
                        }
                        /*
                        while (!ManyMonstersMode.monstersFinishedGrab[playerMonsterNumber] || monster.MoveControl.GetAniControl.fixedAnimation != PlayMonsterAnimation.MonsterAnimation.None)
                        {
                            MultiplayerMode.monsterPlayers[playerMonsterNumber].gameObject.transform.rotation = monsterCamera.transform.rotation;
                            //playerCamera.transform.rotation = monsterCamera.transform.rotation;
                            monster.transform.position = originalPosition;
                            yield return null;
                        }
                        */

                        MultiplayerMode.monsterPlayers[playerMonsterNumber].gameObject.transform.rotation = new Quaternion(0f, MultiplayerMode.monsterPlayers[playerMonsterNumber].gameObject.transform.rotation.y, 0f, MultiplayerMode.monsterPlayers[playerMonsterNumber].gameObject.transform.rotation.w);
                        //playerCamera.transform.rotation = new Quaternion(0f, playerCamera.transform.rotation.y, 0f, playerCamera.transform.rotation.w);
                        monster.MoveControl.LockRotation = false;

                        /*

                        mRoomSearch.searchType = MRoomSearch.ESearchType.Searching;
                        mRoomSearch.targetHidingSpot = closestHidingSpot;
                        mRoomSearch.arrived = false;

                        Debug.Log("Searching spot at " + closestHidingSpot.transPoint + " for monster " + monster.transform.position);
                        mRoomSearch.StartSearch(mRoomSearch.targetHidingSpot.MonsterPoint);
                        */
                        //monster.GetComponent<MState>().SendEvent("Destroy");
                    }
                    monsterInHidingEvent[playerMonsterNumber] = false;
                }
                yield break;
            }

            public static void CrewVSMonsterModeActiveFeatures()
            {
                for (int playerMonsterNumber = 0; playerMonsterNumber < MultiplayerMode.monsterPlayers.Count; playerMonsterNumber++)
                {
                    if (MultiplayerMode.GetPlayerKey("Jump", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed())
                    {
                        Monster monster = ManyMonstersMode.monsterListMonsterComponents[playerMonsterNumber];
                        if (!monsterInHidingEvent[playerMonsterNumber])
                        {
                            monster.StartCoroutine(MonsterPlayerHidingSpotSearchCoroutine(monster, playerMonsterNumber));
                        }
                    }

                    try
                    {
                        if (monsterAbilityReady[playerMonsterNumber] && MultiplayerMode.GetPlayerKey("Crouch", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed())
                        {
                            Debug.Log("Starting monster ability coroutine.");
                            ManyMonstersMode.monsterListMonsterComponents[playerMonsterNumber].StartCoroutine(DoMonsterAbility(ManyMonstersMode.monsterListMonsterComponents[playerMonsterNumber], monsterAbilityTimes[playerMonsterNumber], playerMonsterNumber));
                        }
                    }
                    catch
                    {
                        Debug.Log("Error in ability part.");
                    }

                    try
                    {
                        bool spawnCheck = false;
                        bool hunterTypeCheck = false;
                        try
                        {
                            spawnCheck = MonsterStarter.spawned;
                        }
                        catch
                        {
                            Debug.Log("MS is null");
                        }
                        try
                        {
                            hunterTypeCheck = ManyMonstersMode.monsterListMonsterComponents[playerMonsterNumber].MonsterType == Monster.MonsterTypeEnum.Hunter;
                        }
                        catch
                        {
                            Debug.Log("Monster Type is null");
                        }
                        if (spawnCheck && hunterTypeCheck)
                        {
                            Monster monster = References.Monster.GetComponent<Monster>();
                            try
                            {
                                monster = ManyMonstersMode.monsterListMonsterComponents[playerMonsterNumber];
                            }
                            catch
                            {
                                Debug.Log("Monster is null");
                            }
                            MState mState = monster.GetComponent<MState>();
                            if (mState == null)
                            {
                                Debug.Log("MState is null");
                            }
                            Type monsterStateType = typeof(MWanderState);
                            try
                            {
                                monsterStateType = mState.Fsm.Current.GetType();
                            }
                            catch
                            {
                                Debug.Log("State type is null");
                            }

                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("Player Hunter " + playerMonsterNumber + " is in state " + monsterStateType);
                            }

                            try
                            {
                                if (monsterStateType == typeof(MHuntingState))
                                {
                                    try
                                    {
                                        Camera playerCamera = MultiplayerMode.PlayerCamera(MultiplayerMode.monsterPlayers[playerMonsterNumber]);
                                        if (playerCamera == null)
                                        {
                                            Debug.Log("Player camera is null");
                                        }
                                        try
                                        {
                                            if (AmbushSystem.ambushPoints != null)
                                            {
                                                Debug.Log("AmbushSystem.ambushPoints.Count is " + AmbushSystem.ambushPoints.Count);
                                                int originalAmbushPointDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(AmbushSystem.ambushPoints[currentAmbushPoint[playerMonsterNumber]].transform.position).y;
                                                Debug.Log("Original deck is " + originalAmbushPointDeck);
                                                Debug.Log("Ambush point position at start is " + AmbushSystem.ambushPoints[currentAmbushPoint[playerMonsterNumber]].transform.position);
                                                if (MultiplayerMode.GetPlayerKey("LeanRight", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed())
                                                {
                                                    // Move player up in trap indices on the same floor.
                                                    for (int newAmbushPointIndex = currentAmbushPoint[playerMonsterNumber] + 1; newAmbushPointIndex < AmbushSystem.ambushPoints.Count; newAmbushPointIndex++)
                                                    {
                                                        int ambushPointDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(AmbushSystem.ambushPoints[newAmbushPointIndex].transform.position).y;
                                                        if (ambushPointDeck == originalAmbushPointDeck)
                                                        {
                                                            Debug.Log("Found new ambush point up in index: " + newAmbushPointIndex);
                                                            currentAmbushPoint[playerMonsterNumber] = newAmbushPointIndex;
                                                            break;
                                                        }
                                                    }
                                                }
                                                else if (MultiplayerMode.GetPlayerKey("LeanLeft", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed())
                                                {
                                                    // Move player down in trap indices on the same floor.
                                                    for (int newAmbushPointIndex = currentAmbushPoint[playerMonsterNumber] - 1; newAmbushPointIndex >= 0; newAmbushPointIndex--)
                                                    {
                                                        int ambushPointDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(AmbushSystem.ambushPoints[newAmbushPointIndex].transform.position).y;
                                                        if (ambushPointDeck == originalAmbushPointDeck)
                                                        {
                                                            Debug.Log("Found new ambush point down in index: " + newAmbushPointIndex);
                                                            currentAmbushPoint[playerMonsterNumber] = newAmbushPointIndex;
                                                            break;
                                                        }
                                                    }
                                                }
                                                else if (MultiplayerMode.GetPlayerKey("ViewNote", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed())
                                                {
                                                    // Move player up a floor. Try to find a point near their last index.
                                                    bool foundSuitableAmbushPoint = false;
                                                    for (int newAmbushPointIndex = currentAmbushPoint[playerMonsterNumber]; newAmbushPointIndex < AmbushSystem.ambushPoints.Count && !foundSuitableAmbushPoint; newAmbushPointIndex++)
                                                    {
                                                        int ambushPointDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(AmbushSystem.ambushPoints[newAmbushPointIndex].transform.position).y;
                                                        if (ambushPointDeck == originalAmbushPointDeck + 1)
                                                        {
                                                            currentAmbushPoint[playerMonsterNumber] = newAmbushPointIndex;
                                                            foundSuitableAmbushPoint = true;
                                                        }
                                                    }
                                                    for (int newAmbushPointIndex = currentAmbushPoint[playerMonsterNumber]; newAmbushPointIndex >= 0 && !foundSuitableAmbushPoint; newAmbushPointIndex--)
                                                    {
                                                        int ambushPointDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(AmbushSystem.ambushPoints[newAmbushPointIndex].transform.position).y;
                                                        if (ambushPointDeck == originalAmbushPointDeck + 1)
                                                        {
                                                            currentAmbushPoint[playerMonsterNumber] = newAmbushPointIndex;
                                                            foundSuitableAmbushPoint = true;
                                                        }
                                                    }
                                                    if (foundSuitableAmbushPoint)
                                                    {
                                                        Debug.Log("Found new ambush point up in deck: " + currentAmbushPoint[playerMonsterNumber]);
                                                    }
                                                }
                                                else if (MultiplayerMode.GetPlayerKey("ListenToLog", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed())
                                                {
                                                    // Move player down a floor. Try to find a point near their last index.
                                                    bool foundSuitableAmbushPoint = false;
                                                    for (int newAmbushPointIndex = currentAmbushPoint[playerMonsterNumber]; newAmbushPointIndex < AmbushSystem.ambushPoints.Count && !foundSuitableAmbushPoint; newAmbushPointIndex++)
                                                    {
                                                        int ambushPointDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(AmbushSystem.ambushPoints[newAmbushPointIndex].transform.position).y;
                                                        if (ambushPointDeck == originalAmbushPointDeck - 1)
                                                        {
                                                            currentAmbushPoint[playerMonsterNumber] = newAmbushPointIndex;
                                                            foundSuitableAmbushPoint = true;
                                                        }
                                                    }
                                                    for (int newAmbushPointIndex = currentAmbushPoint[playerMonsterNumber]; newAmbushPointIndex >= 0 && !foundSuitableAmbushPoint; newAmbushPointIndex--)
                                                    {
                                                        int ambushPointDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(AmbushSystem.ambushPoints[newAmbushPointIndex].transform.position).y;
                                                        if (ambushPointDeck == originalAmbushPointDeck - 1)
                                                        {
                                                            currentAmbushPoint[playerMonsterNumber] = newAmbushPointIndex;
                                                            foundSuitableAmbushPoint = true;
                                                        }
                                                    }
                                                    if (foundSuitableAmbushPoint)
                                                    {
                                                        Debug.Log("Found new ambush point down in deck: " + currentAmbushPoint[playerMonsterNumber]);
                                                    }
                                                }
                                                else if (MultiplayerMode.GetPlayerKey("Jump", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed())
                                                {
                                                    DetectPlayer detectPlayer = FindObjectOfType<DetectPlayer>();
                                                    if (detectPlayer != null)
                                                    {
                                                        for (int newAmbushPointIndex = 0; newAmbushPointIndex < AmbushSystem.ambushPoints.Count; newAmbushPointIndex++)
                                                        {
                                                            if (newAmbushPointIndex != currentAmbushPoint[playerMonsterNumber])
                                                            {
                                                                Vector3 playerPosition = playerCamera.transform.position;
                                                                Vector3 ambushPointPosition = AmbushSystem.ambushPoints[newAmbushPointIndex].transform.position;
                                                                if (GeoHelper.InsideCone(playerPosition, ambushPointPosition, playerCamera.transform.forward, detectPlayer.coneAngle, detectPlayer.coneLength))
                                                                {
                                                                    Vector3 vector = ambushPointPosition - playerPosition;
                                                                    RaycastHit raycastHit;
                                                                    if (!Physics.Raycast(ambushPointPosition, vector.normalized, out raycastHit, vector.magnitude, detectPlayer.wallsLayer))
                                                                    {
                                                                        currentAmbushPoint[playerMonsterNumber] = newAmbushPointIndex;
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Debug.Log("Could not find a DetectPlayer to aid in ray casting");
                                                    }
                                                }

                                                try
                                                {
                                                    Debug.Log("Camera position at start is " + playerCamera.transform.position);
                                                    MultiplayerMode.monsterPlayers[playerMonsterNumber].gameObject.transform.position = AmbushSystem.ambushPoints[currentAmbushPoint[playerMonsterNumber]].transform.position;
                                                    //playerCamera.gameObject.transform.position = AmbushSystem.ambushPoints[currentAmbushPoint[playerMonsterNumber]].transform.position;
                                                    Debug.Log("Camera position at end is " + playerCamera.transform.position);
                                                    Debug.Log("Ambush point position at end is " + AmbushSystem.ambushPoints[currentAmbushPoint[playerMonsterNumber]].transform.position);
                                                }
                                                catch
                                                {
                                                    Debug.Log("Something in final part of Hunting State check is null");
                                                }
                                                /*
                                                foreach (AmbushPoint ambushPoint in AmbushSystem.ambushPoints)
                                                {

                                                }
                                                */
                                            }
                                            else
                                            {
                                                Debug.Log("AmbushSystem.ambushPoints is null.");
                                            }
                                        }
                                        catch
                                        {
                                            Debug.Log("Error checking AmbushSystem");
                                        }
                                    }
                                    catch
                                    {
                                        Debug.Log("Error getting Hunter camera");
                                    }
                                }
                                else if (monsterStateType == typeof(MTrappingState))
                                {
                                    try
                                    {
                                        Camera playerCamera = MultiplayerMode.PlayerCamera(MultiplayerMode.monsterPlayers[playerMonsterNumber]);
                                        if (playerCamera == null)
                                        {
                                            Debug.Log("Player camera is null");
                                        }
                                        try
                                        {
                                            if (AmbushSystem.finalAmbushes != null && AmbushSystem.finalAmbushes.Count > 0)
                                            {
                                                if (currentAmbushPoint[playerMonsterNumber] >= AmbushSystem.finalAmbushes.Count)
                                                {
                                                    currentAmbushPoint[playerMonsterNumber] = (int)Math.Floor(AmbushSystem.finalAmbushes.Count / 2f);
                                                }

                                                Debug.Log("AmbushSystem.finalAmbushes.Count is " + AmbushSystem.finalAmbushes.Count);
                                                int originalAmbushPointDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(AmbushSystem.finalAmbushes[currentAmbushPoint[playerMonsterNumber]].transform.position).y;
                                                Debug.Log("Original deck is " + originalAmbushPointDeck);
                                                Debug.Log("Ambush point position at start is " + AmbushSystem.finalAmbushes[currentAmbushPoint[playerMonsterNumber]].transform.position);
                                                if (MultiplayerMode.GetPlayerKey("LeanRight", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed())
                                                {
                                                    // Move player up in trap indices on the same floor.
                                                    for (int newAmbushPointIndex = currentAmbushPoint[playerMonsterNumber] + 1; newAmbushPointIndex < AmbushSystem.finalAmbushes.Count; newAmbushPointIndex++)
                                                    {
                                                        int ambushPointDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(AmbushSystem.finalAmbushes[newAmbushPointIndex].transform.position).y;
                                                        if (ambushPointDeck == originalAmbushPointDeck)
                                                        {
                                                            Debug.Log("Found new ambush point up in index: " + newAmbushPointIndex);
                                                            currentAmbushPoint[playerMonsterNumber] = newAmbushPointIndex;
                                                            break;
                                                        }
                                                    }
                                                }
                                                else if (MultiplayerMode.GetPlayerKey("LeanLeft", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed())
                                                {
                                                    // Move player down in trap indices on the same floor.
                                                    for (int newAmbushPointIndex = currentAmbushPoint[playerMonsterNumber] - 1; newAmbushPointIndex >= 0; newAmbushPointIndex--)
                                                    {
                                                        int ambushPointDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(AmbushSystem.finalAmbushes[newAmbushPointIndex].transform.position).y;
                                                        if (ambushPointDeck == originalAmbushPointDeck)
                                                        {
                                                            Debug.Log("Found new ambush point down in index: " + newAmbushPointIndex);
                                                            currentAmbushPoint[playerMonsterNumber] = newAmbushPointIndex;
                                                            break;
                                                        }
                                                    }
                                                }
                                                else if (MultiplayerMode.GetPlayerKey("ViewNote", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed())
                                                {
                                                    // Move player up a floor. Try to find a point near their last index.
                                                    bool foundSuitableAmbushPoint = false;
                                                    for (int newAmbushPointIndex = currentAmbushPoint[playerMonsterNumber]; newAmbushPointIndex < AmbushSystem.finalAmbushes.Count && !foundSuitableAmbushPoint; newAmbushPointIndex++)
                                                    {
                                                        int ambushPointDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(AmbushSystem.finalAmbushes[newAmbushPointIndex].transform.position).y;
                                                        if (ambushPointDeck == originalAmbushPointDeck + 1)
                                                        {
                                                            currentAmbushPoint[playerMonsterNumber] = newAmbushPointIndex;
                                                            foundSuitableAmbushPoint = true;
                                                        }
                                                    }
                                                    for (int newAmbushPointIndex = currentAmbushPoint[playerMonsterNumber]; newAmbushPointIndex >= 0 && !foundSuitableAmbushPoint; newAmbushPointIndex--)
                                                    {
                                                        int ambushPointDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(AmbushSystem.finalAmbushes[newAmbushPointIndex].transform.position).y;
                                                        if (ambushPointDeck == originalAmbushPointDeck + 1)
                                                        {
                                                            currentAmbushPoint[playerMonsterNumber] = newAmbushPointIndex;
                                                            foundSuitableAmbushPoint = true;
                                                        }
                                                    }
                                                    if (foundSuitableAmbushPoint)
                                                    {
                                                        Debug.Log("Found new ambush point up in deck: " + currentAmbushPoint[playerMonsterNumber]);
                                                    }
                                                }
                                                else if (MultiplayerMode.GetPlayerKey("ListenToLog", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed())
                                                {
                                                    // Move player down a floor. Try to find a point near their last index.
                                                    bool foundSuitableAmbushPoint = false;
                                                    for (int newAmbushPointIndex = currentAmbushPoint[playerMonsterNumber]; newAmbushPointIndex < AmbushSystem.finalAmbushes.Count && !foundSuitableAmbushPoint; newAmbushPointIndex++)
                                                    {
                                                        int ambushPointDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(AmbushSystem.finalAmbushes[newAmbushPointIndex].transform.position).y;
                                                        if (ambushPointDeck == originalAmbushPointDeck - 1)
                                                        {
                                                            currentAmbushPoint[playerMonsterNumber] = newAmbushPointIndex;
                                                            foundSuitableAmbushPoint = true;
                                                        }
                                                    }
                                                    for (int newAmbushPointIndex = currentAmbushPoint[playerMonsterNumber]; newAmbushPointIndex >= 0 && !foundSuitableAmbushPoint; newAmbushPointIndex--)
                                                    {
                                                        int ambushPointDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(AmbushSystem.finalAmbushes[newAmbushPointIndex].transform.position).y;
                                                        if (ambushPointDeck == originalAmbushPointDeck - 1)
                                                        {
                                                            currentAmbushPoint[playerMonsterNumber] = newAmbushPointIndex;
                                                            foundSuitableAmbushPoint = true;
                                                        }
                                                    }
                                                    if (foundSuitableAmbushPoint)
                                                    {
                                                        Debug.Log("Found new ambush point down in deck: " + currentAmbushPoint[playerMonsterNumber]);
                                                    }
                                                }
                                                else if (MultiplayerMode.GetPlayerKey("Jump", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed())
                                                {
                                                    DetectPlayer detectPlayer = FindObjectOfType<DetectPlayer>();
                                                    if (detectPlayer != null)
                                                    {
                                                        for (int newAmbushPointIndex = 0; newAmbushPointIndex < AmbushSystem.finalAmbushes.Count; newAmbushPointIndex++)
                                                        {
                                                            if (newAmbushPointIndex != currentAmbushPoint[playerMonsterNumber])
                                                            {
                                                                Vector3 playerPosition = playerCamera.transform.position;
                                                                Vector3 ambushPointPosition = AmbushSystem.finalAmbushes[newAmbushPointIndex].transform.position;
                                                                if (GeoHelper.InsideCone(playerPosition, ambushPointPosition, playerCamera.transform.forward, detectPlayer.coneAngle, detectPlayer.coneLength))
                                                                {
                                                                    Vector3 vector = ambushPointPosition - playerPosition;
                                                                    RaycastHit raycastHit;
                                                                    if (!Physics.Raycast(ambushPointPosition, vector.normalized, out raycastHit, vector.magnitude, detectPlayer.wallsLayer))
                                                                    {
                                                                        currentAmbushPoint[playerMonsterNumber] = newAmbushPointIndex;
                                                                        break;
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        Debug.Log("Could not find a DetectPlayer to aid in ray casting");
                                                    }
                                                }

                                                try
                                                {
                                                    Debug.Log("Camera position at start is " + playerCamera.transform.position);
                                                    MultiplayerMode.monsterPlayers[playerMonsterNumber].gameObject.transform.position = AmbushSystem.finalAmbushes[currentAmbushPoint[playerMonsterNumber]].transform.position;
                                                    //playerCamera.gameObject.transform.position = AmbushSystem.finalAmbushes[currentAmbushPoint[playerMonsterNumber]].transform.position;
                                                    Debug.Log("Camera position at end is " + playerCamera.transform.position);
                                                    Debug.Log("Ambush point position at end is " + AmbushSystem.finalAmbushes[currentAmbushPoint[playerMonsterNumber]].transform.position);
                                                }
                                                catch
                                                {
                                                    Debug.Log("Something in final part of Hunting State check is null");
                                                }
                                                /*
                                                foreach (AmbushPoint ambushPoint in AmbushSystem.finalAmbushes)
                                                {

                                                }
                                                */
                                            }
                                            else
                                            {
                                                Debug.Log("AmbushSystem.finalAmbushes is null.");
                                            }
                                        }
                                        catch
                                        {
                                            Debug.Log("Error checking AmbushSystem");
                                        }
                                    }
                                    catch
                                    {
                                        Debug.Log("Error getting Hunter camera");
                                    }
                                }
                            }
                            catch
                            {
                                Debug.Log("Error checking type");
                            }
                        }
                    }
                    catch
                    {
                        Debug.Log("Error in Hunter camera part");
                    }
                }
            }

            // @DoMonsterAbility
            private static IEnumerator DoMonsterAbility(Monster monster, float[] monsterActiveAndCooldownTime, int playerMonsterNumber)
            {
                Debug.Log("Doing monster ability coroutine.");
                monsterAbilityReady[playerMonsterNumber] = false;
                monsterUsingActiveAbility[playerMonsterNumber] = true;
                float animatorStartingSpeed = monster.MoveControl.GetAniControl.monsterAnimation.speed;
                float activeTimer = monsterActiveAndCooldownTime[0];
                float cooldownTimer = monsterActiveAndCooldownTime[1]/* - monsterActiveAndCooldownTime[0]*/;
                bool doubleClicked = false;
                bool secondaryAbility = false;
                switch (monster.MonsterType)
                {
                    // Implement any one-time ability code here.
                    case Monster.MonsterTypeEnum.Brute:
                        {
                            MState mState = monster.GetComponent<MState>();
                            Type monsterStateType = mState.Fsm.Current.GetType();

                            /*
                            if (monsterStateType == typeof(MChasingState) && monster.AudSource != null && !monster.IsRoaring && monster.TimeSinceLastRoar > 2.5f)
                            {
                                // ~ Does this roaring work?
                                monster.IsRoaring = true;
                                monster.ResetTimeSinceLastRoar();
                                monster.AudSource.Stop();
                                if (!MultiplayerMode.useLegacyAudio)
                                {
                                    VirtualAudioSource virtualAudioSource = monster.AudSource.gameObject.GetComponent<VirtualAudioSource>();
                                    if (virtualAudioSource != null)
                                    {
                                        virtualAudioSource.Stop();
                                    }
                                    else if (ModSettings.logDebugText)
                                    {
                                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                                    }
                                }
                                monster.MoveControl.GetAniControl.DoARoar = true;
                                monster.GetAniEvents.BigRoar();
                            }
                            */

                            if (monsterStateType == typeof(MChasingState) && monster.AudSource != null)
                            {
                                monster.AudSource.Stop();
                                if (!MultiplayerMode.useLegacyAudio)
                                {
                                    VirtualAudioSource virtualAudioSource = monster.AudSource.gameObject.GetComponent<VirtualAudioSource>();
                                    if (virtualAudioSource != null)
                                    {
                                        virtualAudioSource.Stop();
                                    }
                                    else if (ModSettings.logDebugText)
                                    {
                                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                                    }
                                }
                                monster.GetAniEvents.BigRoar();
                            }

                            //monster.MoveControl.GetAniControl.monsterAnimation.speed = ModSettings.monsterAnimationSpeedMultiplier;
                            //monster.MoveControl.GetAniControl.monsterAnimation.speed *= ModSettings.bruteAbilitySpeedMultiplier;
                            break;
                        }
                    case Monster.MonsterTypeEnum.Hunter:
                        {
                            //FSMState.StateTypes monsterState = monster.GetComponent<FSM>().Current.typeofState; // Does not encompass all state types.
                            //hunter.GetComponent<MState>().Fsm.Current.GetType() == typeof(MTrappingState) // Example of more specific code.
                            MState mState = monster.GetComponent<MState>();
                            Type monsterStateType = mState.Fsm.Current.GetType();

                            if (monsterStateType == typeof(MHuntingState))
                            {
                                Debug.Log("Started MHuntingState Hunter ability");
                                float doubleClickTimer = 0.5f;
                                while (doubleClickTimer >= 0f)
                                {
                                    if (MultiplayerMode.GetPlayerKey("Crouch", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed() && doubleClickTimer < 0.5f)
                                    {
                                        doubleClicked = true;
                                        Debug.Log("Double clicked in MHuntingState ability check");
                                    }
                                    doubleClickTimer -= Time.deltaTime;
                                    yield return null;
                                }
                                monster.huntState.shouldSetUpTrap = true;
                                monster.huntState.ambushing = false;
                                if (doubleClicked)
                                {
                                    monster.huntState.ambushing = true;
                                    monster.trapState.ShouldSpawnImmediately = true;
                                    monster.Hearing.PointOfInterest = AmbushSystem.ambushPoints[currentAmbushPoint[playerMonsterNumber]].transform.position;
                                    monster.alertMeters.mSoundAlert = 50f;
                                    monster.huntState.TrueAmbush = true;

                                    //monster.trapState.Ambush(monster.Hearing.PointOfInterest);

                                    monster.trapState.trapSet = true;
                                    //monster.trapState.trapPos = monster.Hearing.PointOfInterest;
                                    monster.trapState.triggeredAmbush = AmbushSystem.ambushPoints[currentAmbushPoint[playerMonsterNumber]];
                                    monster.trapState.trapPos = monster.trapState.triggeredAmbush.MonsterSpawnPoint.position;
                                    monster.trapState.lookAt = monster.trapState.triggeredAmbush.lookAt.position;
                                    monster.trapState.trapsTried = 0f;
                                    monster.trapState.Respawn();
                                }
                                else
                                {
                                    setUpAtPlayerMonster = true;
                                    monster.Hearing.PointOfInterest = AmbushSystem.ambushPoints[currentAmbushPoint[playerMonsterNumber]].transform.position;
                                    Debug.Log("Did not double click in MHuntingState ability check");
                                }
                                Debug.Log("Finished MHuntingState Hunter ability");
                            }
                            else if (monsterStateType == typeof(MTrappingState))
                            {
                                if (AmbushSystem.finalAmbushes.Count > 0)
                                {
                                    Debug.Log("Started MTrappingState Hunter ability");
                                    float doubleClickTimer = 0.5f;
                                    while (doubleClickTimer >= 0f)
                                    {
                                        if (MultiplayerMode.GetPlayerKey("Crouch", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed() && doubleClickTimer < 0.5f)
                                        {
                                            doubleClicked = true;
                                            Debug.Log("Double clicked in MTrappingState ability check");
                                        }
                                        doubleClickTimer -= Time.deltaTime;
                                        yield return null;
                                    }

                                    if (!doubleClicked)
                                    {
                                        Debug.Log("Did not double click in MTrappingState ability check");
                                        int closestTrapIndex = 0;
                                        float closestTrapDistance = Vector3.Distance(MultiplayerMode.monsterPlayers[playerMonsterNumber].transform.position, AmbushSystem.finalAmbushes[0].transform.position);
                                        for (int i = 1; i < AmbushSystem.finalAmbushes.Count; i++)
                                        {
                                            float distanceToCheck = Vector3.Distance(MultiplayerMode.monsterPlayers[playerMonsterNumber].transform.position, AmbushSystem.finalAmbushes[i].transform.position);
                                            if (distanceToCheck < closestTrapDistance)
                                            {
                                                closestTrapIndex = i;
                                                closestTrapDistance = distanceToCheck;
                                            }
                                        }
                                        //monster.trapState.SetOffTrap(AmbushSystem.finalAmbushes[closestTrapIndex].transform.position, MultiplayerMode.PlayerCamera(MultiplayerMode.monsterPlayers[playerMonsterNumber]).transform.forward);
                                        monster.trapState.trapSet = true;
                                        monster.trapState.triggeredAmbush = AmbushSystem.finalAmbushes[closestTrapIndex];
                                        monster.trapState.trapPos = monster.trapState.triggeredAmbush.MonsterSpawnPoint.position;
                                        monster.trapState.lookAt = monster.trapState.triggeredAmbush.lookAt.position;
                                        monster.trapState.trapsTried = 0f;
                                        monster.trapState.Respawn();
                                    }
                                    else
                                    {
                                        monster.trapState.SetUpHunting();
                                    }
                                    Debug.Log("Finished MTrappingState Hunter ability");
                                }
                                else
                                {
                                    Debug.Log("Could not do MTrappingState Hunter ability because there were no final ambushes.");
                                }
                            }
                            else if (monsterStateType != typeof(MVentFrenzyState))
                            {
                                Debug.Log("Started General State Hunter ability");
                                float doubleClickTimer = 0.5f;
                                while (doubleClickTimer >= 0f)
                                {
                                    if (MultiplayerMode.GetPlayerKey("Crouch", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed() && doubleClickTimer < 0.5f)
                                    {
                                        doubleClicked = true;
                                        Debug.Log("Double clicked in General State Hunter ability check");
                                    }
                                    doubleClickTimer -= Time.deltaTime;
                                    yield return null;
                                }

                                if (!doubleClicked)
                                {
                                    foreach (SkinnedMeshRenderer skinnedMeshRenderer in monster.MonsterMesh)
                                    {
                                        skinnedMeshRenderer.enabled = false;
                                    }

                                    secondaryAbility = true;

                                    activeTimer = ModSettings.hunterVentFrenzyAbilityActiveTime;
                                    cooldownTimer = ModSettings.hunterVentFrenzyAbilityCooldownTime;

                                    for (int crewPlayerIndex = 0; crewPlayerIndex < ModSettings.spawnProtection.Count; crewPlayerIndex++)
                                    {
                                        if (!ModSettings.invincibilityMode[crewPlayerIndex])
                                        {
                                            ModSettings.spawnProtection[crewPlayerIndex] = true;
                                            ModSettings.SetInvincibilityMode(true, crewPlayerIndex);
                                        }
                                    }
                                }
                                else
                                {
                                    MHideState mHideState = monster.GetComponent<MHideState>();

                                    monsterStateType = mState.Fsm.Current.GetType();

                                    if (monsterStateType != typeof(MHideState))
                                    {
                                        mState.SendEvent("Hunt");
                                        yield return null;
                                    }

                                    bool foundValidAmbushPoint = false;
                                    while (!mHideState.hideStarted)
                                    {
                                        //mHideState.trapToReturn = AmbushSystem.FindAmbush(monster.transform.position, true);
                                        if (!foundValidAmbushPoint)
                                        {
                                            foreach (AmbushPoint ambushPoint in AmbushSystem.ambushPoints)
                                            {
                                                /*
                                                mHideState.trapToReturn = ambushPoint;
                                                Vector3 a = new Vector3(monster.transform.position.x, 0f, monster.transform.position.z);
                                                Vector3 b = new Vector3(mHideState.trapToReturn.transform.position.x, 0f, mHideState.trapToReturn.transform.position.z);
                                                float num = Vector3.Distance(a, b);
                                                float num2 = Mathf.Abs(monster.transform.position.y - mHideState.trapToReturn.transform.position.y);
                                                if (num < 1f && num2 < 1.5f && !mHideState.hideStarted)
                                                {
                                                    mHideState.MAXSPEED = 0f;
                                                    mHideState.hideStarted = true;
                                                    monster.StartCoroutine(mHideState.MoveTowardsHidingPlace());
                                                    foundValidAmbushPoint = true;
                                                    break;
                                                }
                                                monster.MoveControl.MaxSpeed = mHideState.MAXSPEED;
                                                */

                                                if (Vector3.Distance(monster.transform.position, ambushPoint.transform.position) < 3f)
                                                {
                                                    mHideState.trapToReturn = ambushPoint;
                                                    mHideState.GoIntoHiding();
                                                    break;
                                                }
                                            }
                                        }
                                        yield return null;
                                    }
                                }
                                Debug.Log("Finished General State Hunter ability");
                            }
                            break;
                        }
                    case Monster.MonsterTypeEnum.Fiend:
                        {
                            Debug.Log("Sealing doors via monster ability coroutine.");
                            FiendDoorSlam.playFiendAnimation = true;
                            monster.MoveControl.GetAniControl.monsterAnimation.SetBool("CloseDoors", FiendDoorSlam.PlayFiendAnimation);
                            FiendDoorSlam.SealDoors(monster.transform.position);
                            if (FiendDoorSlam.PlayFiendAnimation)
                            {
                                monster.MoveControl.GetAniControl.DesiredUpperBodyWeight = 1f;
                            }
                            //FiendDoorSlam.playFiendAnimation = false;

                            FiendAura fiendAura = monster.GetComponentInChildren<FiendAura>();
                            if (fiendAura != null)
                            {
                                float auraMultiplier = ModSettings.fiendAbilityAuraRangeMultiplier;
                                fiendAura.largeRadius *= auraMultiplier;
                                fiendAura.mediumRadius *= auraMultiplier;
                                fiendAura.smallRadius *= auraMultiplier;
                                fiendAura.srt_lrgRad *= auraMultiplier;
                                fiendAura.srt_medRad *= auraMultiplier;
                                fiendAura.srt_smlRad *= auraMultiplier;
                            }
                            else
                            {
                                Debug.Log("Could not get Fiend Aura for Fiend ability.");
                            }
                            break;
                        }
                }

                while (activeTimer > 0f)
                {
                    // Implement any active abilities here. Active abilities requiring external code are implemented using monsterUsingActiveAbility[playerMonsterNumber].
                    if (MultiplayerMode.GetPlayerKey("Crouch", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed() && activeTimer < monsterActiveAndCooldownTime[0])
                    {
                        activeTimer = 0f;
                    }
                    activeTimer -= Time.deltaTime;
                    switch (monster.MonsterType)
                    {
                        case Monster.MonsterTypeEnum.Brute:
                            {
                                monster.MoveControl.MaxSpeed = 100f;
                                monster.MoveControl.AnimationSpeed = 100f * ModSettings.monsterMovementSpeedMultiplier;
                                if (ModSettings.bruteChaseSpeedBuff || ModSettings.applyChaseSpeedBuffToAllMonsters)
                                {
                                    monster.MoveControl.GetAniControl.monsterAnimation.speed = Mathf.MoveTowards(monster.MoveControl.GetAniControl.monsterAnimation.speed, ModSettings.bruteAbilitySpeedMultiplier * ModSettings.monsterAnimationSpeedMultiplier * ModSettings.bruteChaseSpeedBuffMultiplier, Time.deltaTime / 5f);
                                }
                                else
                                {
                                    monster.MoveControl.GetAniControl.monsterAnimation.speed = Mathf.MoveTowards(monster.MoveControl.GetAniControl.monsterAnimation.speed, ModSettings.bruteAbilitySpeedMultiplier * ModSettings.monsterAnimationSpeedMultiplier, Time.deltaTime / 5f);
                                }
                                if (monster.CurrentDoor != null && monster.CurrentDoor.DoorType != Door.doorType.Powered && monster.CurrentDoor.DoorType != Door.doorType.Sealed && Vector3.Distance(monster.transform.position, monster.CurrentDoor.transform.position) < 1.5f)
                                {
                                    monster.CurrentDoor.DamageDoor(1000);
                                    monster.CurrentDoor.BlastOffDoor(75f);
                                }
                                break;
                            }
                        case Monster.MonsterTypeEnum.Hunter:
                            {
                                if (secondaryAbility)
                                {
                                    if (MultiplayerMode.GetPlayerKey("Crouch", ModSettings.numbersOfMonsterPlayers[playerMonsterNumber]).JustPressed() && activeTimer < ModSettings.hunterVentFrenzyAbilityActiveTime)
                                    {
                                        activeTimer = 0f;
                                    }

                                    monster.MoveControl.AnimationSpeed = 0f;
                                    monster.MoveControl.MaxSpeed = 30f;

                                    int playerNumber = ModSettings.numbersOfMonsterPlayers[playerMonsterNumber];
                                    float playerInput = 0f;
                                    if (MultiplayerMode.customKeyBinds["Forward"][playerNumber].keyInUse == KeyCode.None)
                                    {
                                        // If the player is using a controller, get the value from the axis.
                                        playerInput = MultiplayerMode.GetPlayerAxisValue("Y", playerNumber);
                                    }
                                    else
                                    {
                                        // If the player is not using a controller, check whether their forward key is down.
                                        if (MultiplayerMode.GetPlayerKey("Forward", playerNumber).IsDown())
                                        {
                                            playerInput = 1f;
                                        }
                                    }

                                    //monster.MoveControl.MaxSpeed = 100f;
                                    //monster.MoveControl.AnimationSpeed = 100f * ModSettings.monsterMovementSpeedMultiplier;

                                    if (playerInput > 0.1f)
                                    {
                                        //monster.MoveControl.GetAniControl.monsterAnimation.speed = 3f * ModSettings.monsterAnimationSpeedMultiplier * playerInput;
                                        /*
                                        monster.AudSource.enabled = false;
                                        monster.audSource.enabled = false;
                                        monster.audSource2.enabled = false;
                                        */


                                        Camera playerCamera = MultiplayerMode.PlayerCamera(MultiplayerMode.monsterPlayers[playerMonsterNumber]);

                                        /*
                                        int directionMultiplier = 1;
                                        if (Vector3.Angle(playerCamera.transform.forward, ((MState)mVentFrenzyState).monster.MoveControl.AheadNodePos) > 90f)
                                        {
                                            directionMultiplier = -1;
                                        }

                                        ((MState)mVentFrenzyState).monster.MoveControl.SetToFace(((MState)mVentFrenzyState).monster.MoveControl.AheadNodePos);
                                        mVentFrenzyState.moveMomentum = Mathf.MoveTowards(0f, 1.5f, 3f) * directionMultiplier;
                                        */

                                        float moveMomentum = Mathf.MoveTowards(0f, 1.5f, 3f) * 6f * ModSettings.monsterAnimationSpeedMultiplier * playerInput * ModSettings.hunterVentFrenzyAbilitySpeedMultiplier;
                                        Vector3 movementVector = monster.transform.forward * moveMomentum * Time.deltaTime;
                                        monster.transform.position += movementVector;
                                        AudioSystem.PlaySound("Noises/Hunter/Movement/InVent/Run", monster.AudSource/*MultiplayerMode.monsterPlayers[playerMonsterNumber].headSource*/);
                                    }
                                }
                                break;
                            }
                        case Monster.MonsterTypeEnum.Fiend:
                            {
                                break;
                            }
                    }
                    yield return null;
                }

                // Implement any code to stop active abilities after the active time here.
                switch (monster.MonsterType)
                {
                    case Monster.MonsterTypeEnum.Brute:
                        {
                            monster.MoveControl.GetAniControl.monsterAnimation.speed = animatorStartingSpeed;
                            //monster.MoveControl.GetAniControl.monsterAnimation.speed = ModSettings.monsterAnimationSpeedMultiplier;
                            //monster.MoveControl.GetAniControl.monsterAnimation.speed /= ModSettings.bruteAbilitySpeedMultiplier;
                            break;
                        }
                    case Monster.MonsterTypeEnum.Hunter:
                        {
                            if (secondaryAbility)
                            {
                                foreach (SkinnedMeshRenderer skinnedMeshRenderer in monster.MonsterMesh)
                                {
                                    skinnedMeshRenderer.enabled = true;
                                }

                                int closestTrapIndex = -1;
                                float closestTrapDistance = float.MaxValue;
                                int originalDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(monster.transform.position).y;
                                for (int i = 0; i < AmbushSystem.ambushPoints.Count; i++)
                                {
                                    float distanceToCheck = Vector3.Distance(MultiplayerMode.monsterPlayers[playerMonsterNumber].transform.position, AmbushSystem.ambushPoints[i].transform.position);
                                    int deckToCheck = (int)RegionManager.Instance.ConvertPointToRegionNode(AmbushSystem.ambushPoints[i].transform.position).y;
                                    if (distanceToCheck < closestTrapDistance && originalDeck == deckToCheck)
                                    {
                                        closestTrapIndex = i;
                                        closestTrapDistance = distanceToCheck;
                                    }
                                }

                                if (closestTrapIndex == -1)
                                {
                                    closestTrapIndex = 0;
                                    closestTrapDistance = Vector3.Distance(MultiplayerMode.monsterPlayers[playerMonsterNumber].transform.position, AmbushSystem.ambushPoints[0].transform.position);
                                    for (int i = 1; i < AmbushSystem.ambushPoints.Count; i++)
                                    {
                                        float distanceToCheck = Vector3.Distance(MultiplayerMode.monsterPlayers[playerMonsterNumber].transform.position, AmbushSystem.ambushPoints[i].transform.position);
                                        if (distanceToCheck < closestTrapDistance)
                                        {
                                            closestTrapIndex = i;
                                            closestTrapDistance = distanceToCheck;
                                        }
                                    }
                                }
                                /*
                                //monster.trapState.SetOffTrap(AmbushSystem.finalAmbushes[closestTrap].transform.position, MultiplayerMode.PlayerCamera(MultiplayerMode.monsterPlayers[playerMonsterNumber]).transform.forward);
                                monster.trapState.trapSet = true;
                                monster.trapState.triggeredAmbush = AmbushSystem.ambushPoints[closestTrap];
                                monster.trapState.trapPos = monster.trapState.triggeredAmbush.MonsterSpawnPoint.position;
                                monster.trapState.lookAt = monster.trapState.triggeredAmbush.lookAt.position;
                                monster.trapState.trapsTried = 0f;
                                monster.trapState.Respawn();
                                */

                                AmbushPoint closestTrap = AmbushSystem.ambushPoints[closestTrapIndex];

                                if (closestTrap.HideFromHere != null)
                                {
                                    monster.transform.position = closestTrap.HideFromHere.transform.position;
                                }
                                else
                                {
                                    monster.transform.position = closestTrap.MonsterSpawnPoint.transform.position;
                                }
                                // monster.transform.position = AmbushSystem.ambushPoints[closestTrap].HideFromHere.position;

                                if (closestTrap.amPointSource != null)
                                {
                                    AudioSystem.PlaySound(closestTrap.AudioLibString, closestTrap.amPointSource);
                                }
                                if (closestTrap.trapType == AmbushPoint.TrapType.Ceiling || closestTrap.trapType == AmbushPoint.TrapType.Vent)
                                {
                                    ((MonoBehaviour)closestTrap).transform.GetParentOfType<Room>().BroadcastMessage("DisableVent", SendMessageOptions.DontRequireReceiver);
                                }

                                /*
                                monster.MoveControl.GetAniControl.monsterAnimation.speed = ModSettings.monsterAnimationSpeedMultiplier;
                                monster.MoveControl.AnimationSpeed = 0f;
                                */
                                //monster.MoveControl.GetAniControl.monsterAnimation.speed = ModSettings.monsterAnimationSpeedMultiplier;

                                //monster.MoveControl.HunterMoveType = "Walk";

                                /*
                                monster.AudSource.enabled = true;
                                monster.audSource.enabled = true;
                                monster.audSource2.enabled = true;
                                */

                                for (int crewPlayerIndex = 0; crewPlayerIndex < ModSettings.spawnProtection.Count; crewPlayerIndex++)
                                {
                                    if (ModSettings.spawnProtection[crewPlayerIndex])
                                    {
                                        ModSettings.spawnProtection[crewPlayerIndex] = false;
                                        ModSettings.SetInvincibilityMode(false, crewPlayerIndex);
                                    }
                                }
                            }
                            break;
                        }
                    case Monster.MonsterTypeEnum.Fiend:
                        {
                            FiendDoorSlam.ReleaseAllDoors();

                            FiendAura fiendAura = monster.GetComponentInChildren<FiendAura>();
                            if (fiendAura != null)
                            {
                                float auraMultiplier = ModSettings.fiendAbilityAuraRangeMultiplier;
                                fiendAura.largeRadius /= auraMultiplier;
                                fiendAura.mediumRadius /= auraMultiplier;
                                fiendAura.smallRadius /= auraMultiplier;
                                fiendAura.srt_lrgRad /= auraMultiplier;
                                fiendAura.srt_medRad /= auraMultiplier;
                                fiendAura.srt_smlRad /= auraMultiplier;
                            }
                            else
                            {
                                Debug.Log("Could not get Fiend Aura for Fiend ability.");
                            }
                            break;
                        }
                }

                monsterUsingActiveAbility[playerMonsterNumber] = false;

                while (cooldownTimer > 0f)
                {
                    // Implement any penalties during cooldown here.
                    cooldownTimer -= Time.deltaTime;
                    switch (monster.MonsterType)
                    {
                        case Monster.MonsterTypeEnum.Brute:
                            {

                                break;
                            }
                        case Monster.MonsterTypeEnum.Hunter:
                            {
                                if (secondaryAbility)
                                {
                                    monster.MoveControl.AnimationSpeed = Mathf.Clamp(monster.MoveControl.AnimationSpeed, 0f, 50f);
                                }
                                break;
                            }
                        case Monster.MonsterTypeEnum.Fiend:
                            {
                                break;
                            }
                    }
                    yield return null;
                }

                // Implement any code to reset penalties after the cooldown time here.
                switch (monster.MonsterType)
                {
                    case Monster.MonsterTypeEnum.Brute:
                        {

                            break;
                        }
                    case Monster.MonsterTypeEnum.Hunter:
                        {
                            break;
                        }
                    case Monster.MonsterTypeEnum.Fiend:
                        {
                            break;
                        }
                }

                monsterAbilityReady[playerMonsterNumber] = true;
                if (monster.MonsterType == Monster.MonsterTypeEnum.Brute || monster.MonsterType == Monster.MonsterTypeEnum.Fiend || (monster.MonsterType == Monster.MonsterTypeEnum.Hunter && secondaryAbility))
                {
                    int playerNumber = ModSettings.numbersOfMonsterPlayers[playerMonsterNumber];
                    ModSettings.ShowTextOnScreen(monster.MonsterType + "'s Ability Refreshed", 3f, false, playerNumber);
                }
                Debug.Log("Finished monster ability coroutine.");
                yield break;
            }

            private static void HookMHideStateFinishedHiding(On.MHideState.orig_FinishedHiding orig, MHideState mHideState)
            {
                try
                {
                    if (ModSettings.alternatingMonstersMode && ModSettings.numberOfMonsters > ModSettings.numberOfAlternatingMonsters)
                    {
                        TimeScaleManager.Instance.StartCoroutine(ManyMonstersMode.SwitchMonster(((MState)mHideState).monster));
                    }
                    ((MState)mHideState).monster.transform.position = Vector3.zero;
                    ((MState)mHideState).monster.HunterAnimations.GoToHiding = false;
                }
                catch
                {
                    Debug.Log("Error in finished hiding 1");
                }
                try
                {
                    if (mHideState.trapToReturn != null)
                    {
                        mHideState.huntState.lastHidingSpot = mHideState.trapToReturn;
                    }
                }
                catch
                {
                    Debug.Log("Error in finished hiding 2");
                }
                try
                {
                    mHideState.hideStarted = false;
                }
                catch
                {
                    Debug.Log("Error in finished hiding 3");
                }
                try
                {
                    mHideState.trapState = ((MState)mHideState).monster.GetComponent<MTrappingState>();
                }
                catch
                {
                    Debug.Log("Error in finished hiding 4");
                }
                try
                {
                    mHideState.trapState.OutOfTrap = false;
                }
                catch
                {
                    Debug.Log("Error in finished hiding 5");
                }
                try
                {
                    ((MState)mHideState).monster.IsMonsterRetreating = false;
                }
                catch
                {
                    Debug.Log("Error in finished hiding 6");
                }
                try
                {
                    mHideState.hideAttemptTime.StopTimer();
                    mHideState.hideAttemptTime.ResetTimer();
                }
                catch
                {
                    Debug.Log("Error in finished hiding 7");
                }
                try
                {
                    if (!((MState)mHideState).monster.IsInVentFrenzy)
                    {
                        ((MState)mHideState).SendEvent("Hunt");
                    }
                }
                catch
                {
                    Debug.Log("Error in finished hiding 8");
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Door

            private static void HookDoorToggle(On.Door.orig_Toggle orig, Door door, float _force = 0.05f, string byWho = "Player")
            {
                if (door.attached)
                {
                    if (door.conDynController != null)
                    {
                        door.conDynController.enabled = false;
                        door.doorRigidbody.collisionDetectionMode = CollisionDetectionMode.Discrete;
                    }
                    int playerNumber = MultiplayerMode.PlayerNumber(MultiplayerMode.lastPlayerSentMessage.GetInstanceID());
                    if ((!door.isDoorLocked && !door.fiendBlock) || (ModSettings.numbersOfMonsterPlayers.Contains(playerNumber) && ManyMonstersMode.monsterListMonsterComponents[ModSettings.numbersOfMonsterPlayers.IndexOf(playerNumber)].MonsterType == Monster.MonsterTypeEnum.Fiend))
                    {
                        door.opening = !door.opening;
                        door.openForce = 0.05f;
                        door.finishedMove = false;
                        door.doorRigidbody.isKinematic = false;
                        if (door.opening)
                        {
                            door.SetOcclusion(true);
                            if (byWho == "Monster")
                            {
                                AudioSystem.PlaySound(door.monsterOpenDoorLib, door.doorSource);
                                door.attemptedUnlocks = 0;
                            }
                            else
                            {
                                AudioSystem.PlaySound(door.doorOpenLib, door.doorSource);
                            }
                            door.isOpen = true;
                            door.CheckIfBarricade();
                            if (door.attached)
                            {
                                if (door.visionBox == null && door.visionOccluder != null)
                                {
                                    door.visionBox = door.visionOccluder.GetComponent<BoxCollider>();
                                }
                                else
                                {
                                    door.visionBox.gameObject.layer = LayerMask.NameToLayer("NavVision");
                                }
                            }
                            ((MonoBehaviour)door).BroadcastMessage("OnDoorOpen", SendMessageOptions.DontRequireReceiver);
                        }
                        else
                        {
                            if (door.doorPushPlayer != null)
                            {
                                door.doorPushPlayer.DeterminePushDirection();
                            }
                            if (door.doorSource != null)
                            {
                                AudioSystem.PlaySound(door.doorCloseLib, door.doorSource.transform, door.doorSource);
                            }
                            door.isOpen = false;
                            if (door.visionBox == null && door.visionOccluder != null)
                            {
                                door.visionBox = door.visionOccluder.GetComponent<BoxCollider>();
                            }
                            else
                            {
                                door.visionBox.gameObject.layer = LayerMask.NameToLayer("VisionOnly");
                            }
                            ((MonoBehaviour)door).BroadcastMessage("OnDoorClose", SendMessageOptions.DontRequireReceiver);
                        }
                    }
                    else
                    {
                        ((MonoBehaviour)door).BroadcastMessage("OnOpenFail", SendMessageOptions.DontRequireReceiver);
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DoorInteract

            private static bool HookDoorInteractIsConditionMet(On.DoorInteract.orig_IsConditionMet orig, DoorInteract doorInteract)
            {
                int playerNumber = MultiplayerMode.PlayerNumber(MultiplayerMode.lastPlayerCheckingInteractableConditions.GetInstanceID());
                if (ModSettings.numbersOfMonsterPlayers.Contains(playerNumber))
                {
                    bool monsterDestroyingDoor = MultiplayerMode.GetPlayerKey("LeanRight", playerNumber).IsDown();
                    if (ManyMonstersMode.monsterListMonsterComponents[ModSettings.numbersOfMonsterPlayers.IndexOf(playerNumber)].MonsterType == Monster.MonsterTypeEnum.Fiend)
                    {
                        /*
                        if (doorInteract.door.IsDoorLocked)
                        {
                            //doorInteract.door.doorbolt
                            ///doorInteract.door.Unlock(true);
                            DoorBolt doorBolt = doorInteract.door.GetComponentInChildren<DoorBolt>();
                            if (doorBolt != null)
                            {
                                doorBolt.ToggleLock();
                            }
                            return false;
                        }
                        */
                        return true;
                    }
                    else if (monsterDestroyingDoor)
                    {
                        return true;
                    }
                }
                return !doorInteract.door.IsDoorLocked && !doorInteract.door.FiendBlock;
            }

            private static void HookDoorInteractOnHandGrab(On.DoorInteract.orig_OnHandGrab orig, DoorInteract doorInteract)
            {
                String byWho = "Player";
                int lastPlayerSentMessagePlayerNumber = MultiplayerMode.PlayerNumber(MultiplayerMode.lastPlayerSentMessage.GetInstanceID());
                if (ModSettings.numbersOfMonsterPlayers.Contains(lastPlayerSentMessagePlayerNumber))
                {
                    byWho = "Monster";


                    if (doorInteract.door.DoorType == Door.doorType.Ripable || doorInteract.door.DoorType == Door.doorType.Locker)
                    {
                        int playerMonsterNumber = ModSettings.numbersOfMonsterPlayers.IndexOf(lastPlayerSentMessagePlayerNumber);
                        if (!monsterInHidingEvent[playerMonsterNumber])
                        {
                            doorInteract.StartCoroutine(MonsterPlayerHidingSpotSearchCoroutine(ManyMonstersMode.monsterListMonsterComponents[playerMonsterNumber], playerMonsterNumber));
                        }
                        //((MonoBehaviour)doorInteract.door).BroadcastMessage("OnOpenFail", SendMessageOptions.DontRequireReceiver);
                        return;
                    }
                    else if (doorInteract.door.DoorType != Door.doorType.Sealed && doorInteract.door.DoorType != Door.doorType.Powered)
                    {
                        if (MultiplayerMode.GetPlayerKey("LeanRight", lastPlayerSentMessagePlayerNumber).IsDown())
                        {
                            //doorInteract.door.PullOff();
                            doorInteract.door.DamageDoor(1000);
                            doorInteract.door.BlastOffDoor(30f);
                            return;
                            /*
                            int monsterNumber = MultiplayerMode.PlayerMonsterNumberFromPlayerNumber(lastPlayerSentMessagePlayerNumber);
                            Monster monster = ManyMonstersMode.monsterListMonsterComponents[monsterNumber];
                            if (monster.GetComponent<FSM>().Current.typeofState == FSMState.StateTypes.Chase)
                            {
                                doorInteract.door.BlastOffDoor(50f);
                            }
                            else if (monster.MonsterType == Monster.MonsterTypeEnum.Fiend)
                            {
                                doorInteract.door.RipOffDoor2();
                            }
                            else
                            {
                                doorInteract.door.RipOffDoor();
                            }
                            */
                        }
                        else if (ManyMonstersMode.monsterListMonsterComponents[ModSettings.numbersOfMonsterPlayers.IndexOf(lastPlayerSentMessagePlayerNumber)].MonsterType == Monster.MonsterTypeEnum.Fiend)
                        {
                            if (doorInteract.door.IsDoorLocked)
                            {
                                //doorInteract.door.doorbolt
                                ///doorInteract.door.Unlock(true);
                                DoorBolt doorBolt = doorInteract.door.GetComponentInChildren<DoorBolt>();
                                if (doorBolt != null)
                                {
                                    doorBolt.ToggleLock();
                                }
                            }
                        }
                    }
                }
                doorInteract.door.Toggle(0.05f, byWho);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Hiding

            private static void HookHiding()
            {
                On.Hiding.Awake += new On.Hiding.hook_Awake(HookHidingAwake);
                On.Hiding.Update += new On.Hiding.hook_Update(HookHidingUpdate);
            }

            private static void HookHidingAwake(On.Hiding.orig_Awake orig, Hiding hiding)
            {
                hiding.player = ((MonoBehaviour)hiding).GetComponentInParent<NewPlayerClass>();
                Debug.Log("Hiding player instance ID is: " + hiding.player.GetInstanceID());
            }

            private static void HookHidingUpdate(On.Hiding.orig_Update orig, Hiding hiding)
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    hiding.hideValue = 0f;
                    hiding.successPoints.Clear();
                    hiding.failPoints.Clear();
                    if (LevelGeneration.Instance.finishedGenerating)
                    {
                        hiding.pos = ((MonoBehaviour)hiding).transform.position;
                        hiding.up = ((MonoBehaviour)hiding).transform.up;
                        hiding.right = ((MonoBehaviour)hiding).transform.right;
                        hiding.forward = ((MonoBehaviour)hiding).transform.forward;
                        if (hiding.player.IsCrouched())
                        {
                            hiding.pos = ((MonoBehaviour)hiding).transform.parent.position + Vector3.up * 0.3f;
                            hiding.up = ((MonoBehaviour)hiding).transform.parent.right;
                            hiding.right = Vector3.up * 0.3f;
                            hiding.forward = ((MonoBehaviour)hiding).transform.parent.forward;
                        }
                        if (hiding.CheckPoint(hiding.pos))
                        {
                            hiding.hideValue += 1f;
                        }
                        if (hiding.CheckPoint(hiding.pos + hiding.up * hiding.distanceFromPlayer))
                        {
                            hiding.hideValue += 1f;
                        }
                        if (hiding.CheckPoint(hiding.pos - hiding.up * hiding.distanceFromPlayer))
                        {
                            hiding.hideValue += 1f;
                        }
                        if (hiding.CheckPoint(hiding.pos + hiding.right * hiding.distanceFromPlayer * 3f))
                        {
                            hiding.hideValue += 1f;
                        }
                        if (hiding.CheckPoint(hiding.pos - hiding.right * hiding.distanceFromPlayer * 3f))
                        {
                            hiding.hideValue += 1f;
                        }
                        if (hiding.CheckPoint(hiding.pos + hiding.forward * hiding.distanceFromPlayer))
                        {
                            hiding.hideValue += 1f;
                        }
                        if (hiding.CheckPoint(hiding.pos - hiding.forward * hiding.distanceFromPlayer))
                        {
                            hiding.hideValue += 1f;
                        }
                    }
                    int playerNumber = MultiplayerMode.PlayerNumber(hiding.player.GetInstanceID());
                    if (hiding.IsHiding)
                    {
                        AudioSource getHeadSource = hiding.player.GetHeadSource;
                        if (!getHeadSource.isPlaying)
                        {
                            AudioSystem.PlaySound("Noises/Breathing/Hiding", getHeadSource);
                        }


                        if (!playersGoneIntoHiding[playerNumber] && !hiding.player.IsDying)
                        {
                            MultiplayerMode.PlayerInventory(hiding.player).hideItem = true;
                            playersGoneIntoHiding[playerNumber] = true;
                            originalPlayerScales[playerNumber] = hiding.player.transform.localScale;
                            // Vector3 originalPlayerPosition = hiding.player.headSource.transform.position;
                            //hiding.player.gameObject.transform.localScale = Vector3.zero;
                            //hiding.player.gameObject.transform.localScale *= 0.001f;
                            foreach (Renderer renderer in hiding.player.GetComponentsInChildren<Renderer>())
                            {
                                renderer.enabled = false;
                            }
                            foreach (Light light in hiding.player.GetComponentsInChildren<Light>())
                            {
                                light.enabled = false;
                            }
                            Debug.Log("Player hiding is player number " + playerNumber);
                            /*
                            playerHidingHeightDifferences[playerNumber] = originalPlayerPosition - hiding.player.headSource.transform.position;
                            Debug.Log("Original position is " + originalPlayerPosition + " and current position is " + hiding.player.headSource.transform.position + " and height difference is " + playerHidingHeightDifferences[playerNumber]);

                            Debug.Log("Player position before: " + hiding.player.headSource.transform.position);
                            foreach (Camera camera in hiding.player.GetComponentsInChildren<Camera>())
                            {
                                Debug.Log("Camera " + camera.name + "'s position before: " + camera.transform.position);
                                camera.transform.position += playerHidingHeightDifferences[playerNumber];
                                Debug.Log("Camera " + camera.name + "'s position after: " + camera.transform.position);
                            }
                            Debug.Log("Player position after: " + hiding.player.headSource.transform.position);
                            */
                        }


                    }
                    else if (playersGoneIntoHiding[playerNumber])
                    {
                        MultiplayerMode.PlayerInventory(hiding.player).hideItem = false;
                        playersGoneIntoHiding[playerNumber] = false;
                        hiding.player.transform.localScale = originalPlayerScales[playerNumber];
                        foreach (Renderer renderer in hiding.player.GetComponentsInChildren<Renderer>())
                        {
                            renderer.enabled = true;
                        }
                        foreach (Light light in hiding.player.GetComponentsInChildren<Light>())
                        {
                            light.enabled = true;
                        }
                        Debug.Log("Player not hiding is player number " + playerNumber);
                        /*
                        foreach (Camera camera in hiding.player.GetComponentsInChildren<Camera>())
                        {
                            Debug.Log("Camera " + camera.name + "'s position before: " + camera.transform.position);
                            camera.transform.position -= playerHidingHeightDifferences[playerNumber];
                            Debug.Log("Camera " + camera.name + "'s position after: " + camera.transform.position);
                        }
                        */
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Journal

            private static void HookJournalInterpolate(On.Journal.orig_Interpolate orig, Journal journal)
            {
                bool flag = false;
                if (journal.IsMainGame())
                {
                    bool journalKeyJustPressed = false;
                    bool noteKeyJustPressed = false;
                    if (MultiplayerMode.newPlayerClasses != null && !ModSettings.numbersOfMonsterPlayers.Contains(0))
                    {
                        for (int i = 0; i < MultiplayerMode.newPlayerClasses.Count; i++)
                        {
                            if (!ModSettings.numbersOfMonsterPlayers.Contains(i))
                            {
                                if (MultiplayerMode.GetPlayerKey("ViewJournal", i).JustPressed())
                                {
                                    journalKeyJustPressed = true;
                                    break;
                                }
                                else if (MultiplayerMode.GetPlayerKey("ViewNote", i).JustPressed())
                                {
                                    noteKeyJustPressed = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (journalKeyJustPressed && !journal.DisallowJournel() && !journal.pause.pause)
                    {
                        journal.ToggleJournal();
                    }
                    if (noteKeyJustPressed && !journal.pause.pause && References.Inventory.AllowItemSwitching && !journal.DisallowJournel())
                    {
                        if (!journal.IsActive)
                        {
                            journal.EnableJournal();
                            flag = true;
                        }
                        else
                        {
                            journal.DisableJournal();
                            if (journal.IsMostRecentSelected)
                            {
                                journal.DisableJournal();
                            }
                            else
                            {
                                flag = true;
                            }
                        }
                    }
                    if (flag)
                    {
                        TabButton[] componentsInChildren = journal.journalCam.GetComponentsInChildren<TabButton>();
                        for (int i = 0; i < componentsInChildren.Length; i++)
                        {
                            if (componentsInChildren[i].tab.gameObject.name == "Notes")
                            {
                                componentsInChildren[i].SendMessage("OnButtonInteract");
                            }
                        }
                        NoteUI[] componentsInChildren2 = journal.notesListUI.GetComponentsInChildren<NoteUI>();
                        for (int j = 0; j < componentsInChildren2.Length; j++)
                        {
                            if (Note.lastNoteCollected == componentsInChildren2[j].transform.parent.GetComponent<TextMesh>().text)
                            {
                                componentsInChildren2[j].SendMessage("OnButtonInteract");
                                break;
                            }
                        }
                    }
                }
                if (journal.enable)
                {
                    journal.interpolateTime += Time.deltaTime * journal.speed;
                    if (journal.interpolateTime > 1f)
                    {
                        journal.interpolateTime = 1f;
                    }
                    if (journal.DisallowJournel())
                    {
                        journal.DisableJournal();
                    }
                    if (journal.IsMainGame())
                    {
                        if (journal.player.IsStanding())
                        {
                            journal.journalTarget = journal.onPosition;
                        }
                        else
                        {
                            journal.journalTarget = journal.onPositionCrouch;
                        }
                    }
                    else
                    {
                        journal.journalTarget = journal.onPosition;
                    }
                }
                else
                {
                    if (journal.interpolateTime < 0f)
                    {
                        journal.interpolateTime = 0f;
                    }
                    journal.interpolateTime -= Time.deltaTime;
                }
                if (journal.interpolateTime > 0f)
                {
                    ((MonoBehaviour)journal).transform.position = MathHelper.SmoothStep(journal.offPosition.position, journal.journalTarget.position, journal.interpolateTime);
                    ((MonoBehaviour)journal).transform.rotation = Quaternion.Slerp(journal.offPosition.rotation, journal.journalTarget.rotation, journal.interpolateTime);
                }
                else
                {
                    journal.journalCam.gameObject.SetActive(false);
                    journal.journalParent.SetActive(false);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MDoorBashState

            private static void HookMDoorBashStateStateChanges(On.MDoorBashState.orig_StateChanges orig, MDoorBashState mDoorBashState)
            {
                int monsterNumber = ManyMonstersMode.MonsterNumber(mDoorBashState.monster.GetInstanceID());
                if (monsterNumber < ModSettings.numbersOfMonsterPlayers.Count && !letAIControlMonster)
                {
                    int playerNumber = ModSettings.numbersOfMonsterPlayers[monsterNumber];
                    if ((MultiplayerMode.GetPlayerKey("Interact", playerNumber).JustPressed() || MultiplayerMode.GetPlayerKey("Jump", playerNumber).JustPressed()) && mDoorBashState.roomDoors != null && mDoorBashState.roomDoors.Count > 0)
                    {
                        /*
                        Door closestDoor = mDoorBashState.roomDoors[0];
                        float closestDoorDistance = Vector3.Distance(mDoorBashState.monster.transform.position, mDoorBashState.roomDoors[0].transform.position);
                        for (int i = 1; i < mDoorBashState.roomDoors.Count; i++)
                        {
                            float distanceToDoorToCheck = Vector3.Distance(mDoorBashState.monster.transform.position, mDoorBashState.roomDoors[i].transform.position);
                            if (distanceToDoorToCheck < closestDoorDistance)
                            {
                                closestDoor = mDoorBashState.roomDoors[i];
                                closestDoorDistance = distanceToDoorToCheck;
                            }
                        }
                        */

                        Door monsterCurrentDoor = mDoorBashState.monster.CurrentDoor;
                        if (monsterCurrentDoor != null && mDoorBashState.roomDoors.Contains(monsterCurrentDoor) && Vector3.Distance(mDoorBashState.monster.transform.position, mDoorBashState.monster.CurrentDoor.transform.position) < 2f)
                        {
                            foreach (Door door in mDoorBashState.roomDoors)
                            {
                                door.BlockPathfinding();
                            }

                            mDoorBashState.shouldChooseNewDoor = false;
                            mDoorBashState.ChosenDoor = monsterCurrentDoor;
                            mDoorBashState.ChosenDoor.AllowPathfinding();

                            mDoorBashState.ChosenDoor.allowDestroy = true;
                            mDoorBashState.monster.MoveControl.GetAniControl.doorAnimation = MonsterDoorAnimation.Combine(true, false);
                            mDoorBashState.monster.IsMonsterDestroying = true;

                            mDoorBashState.isGoingToDestroy = true;
                            ((MState)mDoorBashState).SendEvent("Destroy");
                        }
                    }
                }
                else if (((MState)mDoorBashState).monster.CurrentDoor != null && ((MState)mDoorBashState).monster.CurrentDoor == mDoorBashState.ChosenDoor && !mDoorBashState.shouldChooseNewDoor && ((MState)mDoorBashState).monster.MoveControl.MaxSpeed > 0f)
                {
                    mDoorBashState.isGoingToDestroy = true;
                    ((MState)mDoorBashState).SendEvent("Destroy");
                }
                else
                {
                    mDoorBashState.isGoingToDestroy = false;
                }
                if (((MState)mDoorBashState).monster.TheSubAlarm.RoomBreached)
                {
                    foreach (Door door in mDoorBashState.roomDoors)
                    {
                        if (door.attached)
                        {
                            door.BlockPathfinding();
                        }
                    }
                    ((MState)mDoorBashState).monster.MoveControl.BlockPathmaking = false;
                    ((MState)mDoorBashState).SendEvent("Chase");
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MFiendSubDoors

            private static void HookMFiendSubDoorsStateChanges(On.MFiendSubDoors.orig_StateChanges orig, MFiendSubDoors mFiendSubDoors)
            {
                int monsterNumber = ManyMonstersMode.MonsterNumber(mFiendSubDoors.monster.GetInstanceID());
                if (monsterNumber < ModSettings.numbersOfMonsterPlayers.Count && !letAIControlMonster && mFiendSubDoors.openDoorTime == 0f)
                {
                    int playerNumber = ModSettings.numbersOfMonsterPlayers[monsterNumber];
                    if ((MultiplayerMode.GetPlayerKey("Interact", playerNumber).JustPressed() || MultiplayerMode.GetPlayerKey("Jump", playerNumber).JustPressed()) && mFiendSubDoors.roomDoors != null && mFiendSubDoors.roomDoors.Count > 0)
                    {
                        Door monsterCurrentDoor = mFiendSubDoors.monster.CurrentDoor;
                        if (monsterCurrentDoor != null && mFiendSubDoors.roomDoors.Contains(monsterCurrentDoor) && Vector3.Distance(mFiendSubDoors.monster.transform.position, mFiendSubDoors.monster.CurrentDoor.transform.position) < 2f)
                        {
                            foreach (Door door in mFiendSubDoors.roomDoors)
                            {
                                door.BlockPathfinding();
                            }

                            mFiendSubDoors.shouldChooseNewDoor = false;
                            mFiendSubDoors.ChosenDoor = monsterCurrentDoor;
                            mFiendSubDoors.ChosenDoor.AllowPathfinding();

                            // Might not be applicable to Fiend.
                            //mFiendSubDoors.ChosenDoor.allowDestroy = true;
                            mFiendSubDoors.monster.MoveControl.GetAniControl.doorAnimation = MonsterDoorAnimation.Combine(true, false);
                            mFiendSubDoors.monster.IsMonsterDestroying = false;

                            ((MState)mFiendSubDoors).monster.MoveControl.MaxSpeed = 0f;
                            if (!((MState)mFiendSubDoors).monster.IsNearSubDoors)
                            {
                                ((MState)mFiendSubDoors).monster.GetComponent<MDestroyState>().LerpToTheDoor();
                                mFiendSubDoors.ChosenDoor.SubDoorUnlock();
                            }
                            mFiendSubDoors.PullOpenDoor();
                        }
                    }
                }
                else if (((MState)mFiendSubDoors).monster.CurrentDoor != null && ((MState)mFiendSubDoors).monster.CurrentDoor == mFiendSubDoors.ChosenDoor && !mFiendSubDoors.shouldChooseNewDoor && ((MState)mFiendSubDoors).monster.MoveControl.MaxSpeed > 0f)
                {
                    ((MState)mFiendSubDoors).monster.MoveControl.MaxSpeed = 0f;
                    if (!((MState)mFiendSubDoors).monster.IsNearSubDoors)
                    {
                        ((MState)mFiendSubDoors).monster.GetComponent<MDestroyState>().LerpToTheDoor();
                        mFiendSubDoors.ChosenDoor.SubDoorUnlock();
                    }
                    mFiendSubDoors.PullOpenDoor();
                }
                else
                {
                    mFiendSubDoors.openDoorTime = 0f;
                    ((MState)mFiendSubDoors).monster.IsNearSubDoors = false;
                }
                if (((MState)mFiendSubDoors).monster.TheSubAlarm.RoomBreached)
                {
                    foreach (Door door in mFiendSubDoors.roomDoors)
                    {
                        if (door.attached)
                        {
                            door.BlockPathfinding();
                        }
                    }
                    ((MState)mFiendSubDoors).monster.MoveControl.BlockPathmaking = false;
                    ((MState)mFiendSubDoors).SendEvent("Chase");
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MInvestigateState

            private static void HookMInvestigateStateOnUpdate(On.MInvestigateState.orig_OnUpdate orig, MInvestigateState mInvestigateState)
            {
                if (((MState)mInvestigateState).monster.SubEventBeenStarted())
                {
                    ((MState)mInvestigateState).SendEvent("EventStarted");
                }
                else if (((MState)mInvestigateState).monster.IsMonsterDestroying)
                {
                    ((MState)mInvestigateState).SendEvent("Destroy");
                }
                else if (((MState)mInvestigateState).monster.IsMonsterRetreating)
                {
                    ((MState)mInvestigateState).SendEvent("Retreat");
                }
                else if (((MState)mInvestigateState).monster.CanSeePlayerNotHiding)
                {
                    ((MState)mInvestigateState).SendEvent("Chase");
                }
                else if (((MState)mInvestigateState).monster.MoveControl.shouldClimb)
                {
                    ((MState)mInvestigateState).SendEvent("Climb");
                }
                else if (((MState)mInvestigateState).monster.Hearing.CurrentSoundSource != null && ((MState)mInvestigateState).monster.Hearing.CurrentSoundSource.Gameplay.highAlertSound)
                {
                    ((MState)mInvestigateState).SendEvent("Search");
                }
                else if ((((MState)mInvestigateState).monster.MoveControl.IsAtDestination || ((MState)mInvestigateState).monster.IsStuck || mInvestigateState.nullNoiseTimer > 30f || mInvestigateState.IsInSameRoom()) && ((MState)mInvestigateState).TimeInState > 10f)
                {
                    if (((MState)mInvestigateState).monster.FoundPlayerBySound)
                    {
                        ((MState)mInvestigateState).SendEvent("Chase");
                    }
                    if (((MState)mInvestigateState).monster.InSearchableArea() && letAIControlMonster)
                    {
                        if ((((MState)mInvestigateState).monster.Hearing.CurrentSoundSource == null || !((MState)mInvestigateState).monster.Hearing.CurrentSoundSource.Source.isPlaying))
                        {
                            ((MState)mInvestigateState).SendEvent("RoomSearch");
                        }
                    }
                    else if (mInvestigateState.wasHighAlert)
                    {
                        ((MState)mInvestigateState).SendEvent("Search");
                    }
                    else
                    {
                        if (letAIControlMonster)
                        {
                            ((MState)mInvestigateState).SendEvent("Hide");
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MonstDetectRoom

            private static void HookMonstDetectRoomDoorChecks(On.MonstDetectRoom.orig_DoorChecks orig, MonstDetectRoom monstDetectRoom)
            {
                int monsterNumber = ManyMonstersMode.MonsterNumber(monstDetectRoom.monster.GetInstanceID());
                if (letAIControlMonster || monsterNumber >= MultiplayerMode.monsterPlayers.Count)
                {
                    orig.Invoke(monstDetectRoom);
                }
                else
                {
                    /*
                    if (MultiplayerMode.GetPlayerKey("Crouch", MultiplayerMode.PlayerNumber(MultiplayerMode.monsterPlayers[monsterNumber].GetInstanceID())).JustPressed())
                    {
                        Vector3 direction = monstDetectRoom.monster.transform.forward - monstDetectRoom.monster.transform.position;
                        Ray ray = new Ray(monstDetectRoom.monster.transform.position + Vector3.up, direction);
                        float maxDistance = monstDetectRoom.doorDistCheck;
                        if (monstDetectRoom.monster.MoveControl.AnimationSpeed > 50f && !monstDetectRoom.monster.CanSeePlayer)
                        {
                            maxDistance = monstDetectRoom.doorDistCheck + 1f;
                        }
                        RaycastHit raycastHit;
                        if (Physics.Raycast(ray.origin, ray.direction, out raycastHit, maxDistance, monstDetectRoom.layer))
                        {
                            if (!monstDetectRoom.nearDoor || monstDetectRoom.currentDoor == null)
                            {
                                monstDetectRoom.currentDoor = raycastHit.collider.transform.GetParentOfType<Door>();
                            }
                            if (monstDetectRoom.currentDoor != null)
                            {
                                if (!monstDetectRoom.currentDoor.isOpen || monstDetectRoom.atDoorTimer.TimeElapsed > 3f)
                                {
                                    monstDetectRoom.nearDoor = true;
                                }
                                else if (!monstDetectRoom.atDoorTimer.IsRunning)
                                {
                                    monstDetectRoom.atDoorTimer.StartTimer();
                                }
                            }
                        }
                        else
                        {
                            monstDetectRoom.nearDoor = false;
                            monstDetectRoom.atDoorTimer.StopTimer();
                            monstDetectRoom.atDoorTimer.ResetTimer();
                            if (monstDetectRoom.currentDoor != null)
                            {
                                monstDetectRoom.currentDoor.monsterClose = false;
                                monstDetectRoom.currentDoor = null;
                            }
                        }

                        if (monstDetectRoom.CurrentDoor != null && monstDetectRoom.CurrentDoor.DoorType != Door.doorType.Sealed && monstDetectRoom.CurrentDoor.DoorType != Door.doorType.Powered)
                        {
                            monstDetectRoom.CurrentDoor.BlastOffDoor(50f);
                        }
                    }
                    */

                    /*
                    if (XboxCtrlrInput.XCI.GetButtonDown(XboxCtrlrInput.XboxButton.X))
                    {
                        Vector3 direction = monstDetectRoom.monster.transform.forward - monstDetectRoom.monster.transform.position;
                        Ray ray = new Ray(monstDetectRoom.monster.transform.position + Vector3.up, direction);
                        float maxDistance = monstDetectRoom.doorDistCheck;
                        if (monstDetectRoom.monster.MoveControl.AnimationSpeed > 50f && !monstDetectRoom.monster.CanSeePlayer)
                        {
                            maxDistance = monstDetectRoom.doorDistCheck + 1f;
                        }
                        RaycastHit raycastHit;
                        if (Physics.Raycast(ray.origin, ray.direction, out raycastHit, maxDistance, monstDetectRoom.layer))
                        {
                            if (!monstDetectRoom.nearDoor || monstDetectRoom.currentDoor == null)
                            {
                                monstDetectRoom.currentDoor = raycastHit.collider.transform.GetParentOfType<Door>();
                            }
                            if (monstDetectRoom.currentDoor != null)
                            {
                                if (!monstDetectRoom.currentDoor.isOpen || monstDetectRoom.atDoorTimer.TimeElapsed > 3f)
                                {
                                    monstDetectRoom.nearDoor = true;
                                }
                                else if (!monstDetectRoom.atDoorTimer.IsRunning)
                                {
                                    monstDetectRoom.atDoorTimer.StartTimer();
                                }
                            }
                        }
                        else
                        {
                            monstDetectRoom.nearDoor = false;
                            monstDetectRoom.atDoorTimer.StopTimer();
                            monstDetectRoom.atDoorTimer.ResetTimer();
                            if (monstDetectRoom.currentDoor != null)
                            {
                                monstDetectRoom.currentDoor.monsterClose = false;
                                monstDetectRoom.currentDoor = null;
                            }
                        }
                        if (monstDetectRoom.nearDoor && monstDetectRoom.currentDoor != null)
                        {
                            monstDetectRoom.currentDoor.monsterClose = true;
                        }
                        monstDetectRoom.NearDoorCheck();


                        if (monstDetectRoom.CurrentDoor != null)
                        {
                            Debug.Log("Door data: " + monstDetectRoom.CurrentDoor + " " + (monstDetectRoom.CurrentDoor.BeenOpenFor > 5f) + " " + monstDetectRoom.CurrentDoor.monsterClose + " " + (monstDetectRoom.CurrentDoor.DoorType != Door.doorType.Sealed) + " " + (monstDetectRoom.CurrentDoor.DoorType != Door.doorType.Powered));
                        }
                        else
                        {
                            Debug.Log("Door data is null.");
                        }
                    }
                    */
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MonsterCameraControl

            private static void HookMonsterCameraControlUpdate(On.MonsterCameraControl.orig_Update orig, MonsterCameraControl monsterCameraControl)
            {
                int monsterNumber = ManyMonstersMode.MonsterNumber(monsterCameraControl.monster.GetInstanceID());
                if (letAIControlMonster || monsterNumber >= MultiplayerMode.monsterPlayers.Count)
                {
                    orig.Invoke(monsterCameraControl);
                }
                /*
                else
                {
                    if (monsterCameraControl.monster != null)
                    {
                        if (monsterCameraControl.FT2 != null && monsterCameraControl.monster.MoveControl.AnimationSpeed > 50f && 1 == 0)
                        {
                            monsterPositionModifier = monsterCameraControl.monster.gameObject.transform.forward * 0.15f * monsterCameraControl.monster.gameObject.transform.localScale.y + Vector3.down * monsterCameraControl.monster.gameObject.transform.localScale.y;
                        }
                        else
                        {
                            monsterPositionModifier = monsterCameraControl.monster.gameObject.transform.forward * monsterCameraControl.monster.gameObject.transform.localScale.y * 0.15f;
                        }
                    }
                }
                */
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MovementControl

            private static void HookMovementControl()
            {
                On.MovementControl.LerpRotate += new On.MovementControl.hook_LerpRotate(HookMovementControlLerpRotate);
                On.MovementControl.WorkOutDirection += new On.MovementControl.hook_WorkOutDirection(HookMovementControlWorkOutDirection);
                On.MovementControl.WorkOutSpeed += new On.MovementControl.hook_WorkOutSpeed(HookMovementControlWorkOutSpeed);
                On.MovementControl.SnapToFloor += new On.MovementControl.hook_SnapToFloor(HookMovementControlSnapToFloor);
                On.MovementControl.FixedUpdate += new On.MovementControl.hook_FixedUpdate(HookMovementControlFixedUpdate);
            }

            private static void HookMovementControlFixedUpdate(On.MovementControl.orig_FixedUpdate orig, MovementControl movementControl)
            {
                if (Time.timeScale != 0f)
                {
                    movementControl.recalculatePathTime += Time.deltaTime;
                    movementControl.updateVelocityTimer += Time.deltaTime;
                    movementControl.timeSpentHeadingToNode += Time.deltaTime;
                    int currentNode = movementControl.CurrentNode;
                    if (movementControl.previousNode < currentNode)
                    {
                        movementControl.timeSpentHeadingToNode = 0f;
                    }
                    movementControl.previousNode = currentNode;
                    movementControl.CheckForStairs();
                    if (!movementControl.lastAtDestination && movementControl.IsAtDestination)
                    {
                        Vector3 vector = Vector3.zero;
                        if (movementControl.path != null && movementControl.path.Count > 0)
                        {
                            vector = movementControl.path[movementControl.path.Count - 1];
                        }
                        ((MonoBehaviour)movementControl).BroadcastMessage("OnArrive", vector);
                    }
                    movementControl.lastAtDestination = movementControl.IsAtDestination;
                    movementControl.Pathfinding();
                    movementControl.JumpChecking();
                    movementControl.shouldClimb = movementControl.ClimbChecking();
                    if (!movementControl.isClimbing && (!movementControl.nearJump || (!letAIControlMonster && !movementControl.lerpStarted)) && !movementControl.shouldJumpDown)
                    {
                        movementControl.WorkOutDirection();
                        movementControl.CheckRotation();
                        movementControl.SnapToFloor();
                    }
                    movementControl.animController.AnimationSpeed = movementControl.moveSpeed;
                }
            }

            private static void HookMovementControlSnapToFloor(On.MovementControl.orig_SnapToFloor orig, MovementControl movementControl)
            {
                if (!ModSettings.numbersOfMonsterPlayers.Contains(0) || !ModSettings.noclip)
                {
                    orig.Invoke(movementControl);
                }
            }

            private static void HookMovementControlLerpRotate(On.MovementControl.orig_LerpRotate orig, MovementControl movementControl, Vector3 _targetPos, bool overwrite)
            {
                int monsterNumber = ManyMonstersMode.MonsterNumber(movementControl.monster.GetInstanceID());
                if (letAIControlMonster || monsterNumber >= MultiplayerMode.monsterPlayers.Count)
                {
                    BaseFeatures.CustomMovementControlLerpRotate(movementControl, _targetPos, overwrite);
                }
                else
                {
                    if (LevelGeneration.Instance.finishedGenerating && !monsterInHidingEvent[monsterNumber])
                    {
                        Quaternion monsterRotation = MultiplayerMode.PlayerCamera(MultiplayerMode.monsterPlayers[monsterNumber]).transform.rotation;
                        monsterRotation.x = 0;
                        monsterRotation.z = 0;

                        /*
                        // # REMOVE THIS Y MOVEMENT PART IF YOU ARE ALLOWING MORE THAN ONE MONSTER
                        PlayerMotor newPlayerClassMotor = MultiplayerMode.monsterPlayers[1].Motor;
                        if (newPlayerClassMotor.yMovement > 0.2f)
                        {
                            newPlayerClassMotor.yMovement = 0;
                        }
                        */

                        /*
                        int maximumAxis = 0;

                        float yValue = XboxCtrlrInput.XCI.LeftStickValueY();
                        float xValue = XboxCtrlrInput.XCI.LeftStickValueX();
                        float rightValue = 0;
                        float leftValue = 0;
                        float forwardValue = 0;
                        float backwardsValue = 0;

                        if (xValue > Math.Abs(xValue))
                        {
                            rightValue = xValue;
                        }
                        else
                        {
                            leftValue = Math.Abs(xValue);
                        }
                        if (yValue > Math.Abs(yValue))
                        {
                            forwardValue = yValue;
                        }
                        else
                        {
                            backwardsValue = yValue;
                        }

                        if (Math.Abs(yValue) > Math.Abs(xValue))
                        {
                            if (forwardValue > backwardsValue)
                            {
                                if (forwardValue > 0.1f)
                                {
                                    maximumAxis = 0;
                                }
                            }
                            else
                            {
                                if (backwardsValue > 0.1f)
                                {
                                    maximumAxis = 2;
                                }
                            }
                        }
                        else
                        {
                            if (rightValue > leftValue)
                            {
                                if (rightValue > 0.1f)
                                {
                                    maximumAxis = 1;
                                }
                            }
                            else
                            {
                                if (leftValue > 0.1f)
                                {
                                    maximumAxis = 3;
                                }
                            }
                        }

                        switch (maximumAxis)
                        {
                            case (1):
                                {
                                    //movementControl.GetAniControl.CurrentAngle = 90f;
                                    //movementControl.monster.transform.localRotation = new Quaternion(0, 90f, 0, 0);
                                    //movementControl.newRotation = new Quaternion(0, 90f, 0, 0);
                                    monsterRotation = new Quaternion(monsterRotation.x, 90f, monsterRotation.z, monsterRotation.w);
                                    break;
                                }
                            case (2):
                                {
                                    //movementControl.GetAniControl.CurrentAngle = 180f;
                                    //movementControl.monster.transform.localRotation = new Quaternion(0, 180f, 0, 0);
                                    //movementControl.newRotation = new Quaternion(0, 180f, 0, 0);
                                    monsterRotation = new Quaternion(monsterRotation.x, 180f, monsterRotation.z, monsterRotation.w);
                                    break;
                                }
                            case (3):
                                {
                                    //movementControl.GetAniControl.CurrentAngle = 270f;
                                    //movementControl.monster.transform.localRotation = new Quaternion(0, 270f, 0, 0);
                                    //movementControl.newRotation = new Quaternion(0, 270f, 0, 0);
                                    monsterRotation = new Quaternion(monsterRotation.x, 270f, monsterRotation.z, monsterRotation.w);
                                    break;
                                }
                            default:
                                {
                                    //movementControl.GetAniControl.CurrentAngle = 0f;
                                    //movementControl.monster.transform.localRotation = new Quaternion(0, 0f, 0, 0);
                                    //movementControl.newRotation = new Quaternion(0, 0f, 0, 0);
                                    break;
                                }
                        }
                        */




                        //movementControl.LockRotation = false; // Doesn't work by itself.
                        movementControl.GetAniControl.CurrentAngle = Quaternion.Angle(movementControl.monster.transform.localRotation, monsterRotation);
                        ((MonoBehaviour)movementControl).transform.localRotation = monsterRotation;
                    }
                }
            }

            private static void HookMovementControlWorkOutDirection(On.MovementControl.orig_WorkOutDirection orig, MovementControl movementControl)
            {
                int monsterNumber = ManyMonstersMode.MonsterNumber(movementControl.monster.GetInstanceID());
                if (letAIControlMonster || monsterNumber >= MultiplayerMode.monsterPlayers.Count)
                {
                    orig.Invoke(movementControl);
                }
                /*
                else
                {
                    int maximumAxis = 0;

                    float yValue = XboxCtrlrInput.XCI.LeftStickValueY();
                    float xValue = XboxCtrlrInput.XCI.LeftStickValueX();
                    float rightValue = 0;
                    float leftValue = 0;
                    float forwardValue = 0;
                    float backwardsValue = 0;

                    if (xValue > Math.Abs(xValue))
                    {
                        rightValue = xValue;
                    }
                    else
                    {
                        leftValue = Math.Abs(xValue);
                    }
                    if (yValue > Math.Abs(yValue))
                    {
                        forwardValue = yValue;
                    }
                    else
                    {
                        backwardsValue = yValue;
                    }

                    if (Math.Abs(yValue) > Math.Abs(xValue))
                    {
                        if (forwardValue > backwardsValue)
                        {
                            if (forwardValue > 0.1f)
                            {
                                maximumAxis = 0;
                            }
                        }
                        else
                        {
                            if (backwardsValue > 0.1f)
                            {
                                maximumAxis = 2;
                            }
                        }
                    }
                    else
                    {
                        if (rightValue > leftValue)
                        {
                            if (rightValue > 0.1f)
                            {
                                maximumAxis = 1;
                            }
                        }
                        else
                        {
                            if (leftValue > 0.1f)
                            {
                                maximumAxis = 3;
                            }
                        }
                    }

                    switch (maximumAxis)
                    {
                        case (1):
                            {
                                //movementControl.GetAniControl.CurrentAngle = 90f;
                                //movementControl.monster.transform.localRotation = new Quaternion(0, 90f, 0, 0);
                                movementControl.newRotation = new Quaternion(0, 90f, 0, 0);
                                break;
                            }
                        case (2):
                            {
                                //movementControl.GetAniControl.CurrentAngle = 180f;
                                //movementControl.monster.transform.localRotation = new Quaternion(0, 180f, 0, 0);
                                movementControl.newRotation = new Quaternion(0, 180f, 0, 0);
                                break;
                            }
                        case (3):
                            {
                                //movementControl.GetAniControl.CurrentAngle = 270f;
                                //movementControl.monster.transform.localRotation = new Quaternion(0, 270f, 0, 0);
                                movementControl.newRotation = new Quaternion(0, 270f, 0, 0);
                                break;
                            }
                        default:
                            {
                                //movementControl.GetAniControl.CurrentAngle = 0f;
                                //movementControl.monster.transform.localRotation = new Quaternion(0, 0f, 0, 0);
                                movementControl.newRotation = new Quaternion(0, 0f, 0, 0);
                                break;
                            }
                    }
                }
                */
            }

            private static void HookMovementControlWorkOutSpeed(On.MovementControl.orig_WorkOutSpeed orig, MovementControl movementControl)
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    int monsterNumber = ManyMonstersMode.MonsterNumber(movementControl.monster.GetInstanceID());
                    /*
                    Type monsterStateTypeText = movementControl.monster.GetComponent<MState>().Fsm.Current.GetType();
                    Debug.Log("Current monster state is " + monsterStateTypeText);
                    */
                    if (monsterNumber >= MultiplayerMode.monsterPlayers.Count)
                    {
                        BaseFeatures.CustomMovementControlWorkOutSpeed(movementControl);
                        if (ModSettings.logDebugText)
                        {
                            Debug.Log("Monster number " + ManyMonstersMode.MonsterNumber(movementControl.monster.GetInstanceID()) + " is being controlled by AI in MovementControl as it is not assigned to a player and currently following player number " + MultiplayerMode.PlayerNumber(movementControl.monster.player.GetComponent<NewPlayerClass>().GetInstanceID()));
                        }
                    }
                    else
                    {
                        Camera monsterCamera = movementControl.monster.GetComponentInChildren<Camera>();

                        Type monsterStateType = movementControl.monster.GetComponent<MState>().Fsm.Current.GetType();
                        if (monsterStateType != typeof(MHuntingState) && monsterStateType != typeof(MDestroyState))
                        {
                            MultiplayerMode.monsterPlayers[monsterNumber].gameObject.transform.position = monsterCamera.transform.position + movementControl.monster.gameObject.transform.forward * movementControl.monster.gameObject.transform.localScale.y * 0.15f; // This causes the excessive movement bug when the monster is attacking a sub door (maybe also when pulling out player from locker)
                            /*
                            if (movementControl.MaxSpeed > 0f)
                            {
                                MultiplayerMode.monsterPlayers[monsterNumber].gameObject.transform.position = monsterCamera.transform.position + movementControl.monster.gameObject.transform.forward * movementControl.monster.gameObject.transform.localScale.y * 0.15f; // This causes the excessive movement bug when the monster is attacking a sub door (maybe also when pulling out player from locker)
                            }
                            */
                        }

                        int playerNumber = ModSettings.numbersOfMonsterPlayers[monsterNumber];
                        if (MultiplayerMode.GetPlayerKey("ViewJournal", playerNumber).JustPressed())
                        {
                            letAIControlMonster = !letAIControlMonster;
                            //MultiplayerMode.monsterPlayers[monsterNumber].gameObject.transform.rotation = Quaternion.identity;
                            if (!letAIControlMonster)
                            {
                                MultiplayerMode.monsterPlayers[monsterNumber].gameObject.transform.rotation = new Quaternion(0f, MultiplayerMode.monsterPlayers[monsterNumber].gameObject.transform.rotation.y, 0f, MultiplayerMode.monsterPlayers[monsterNumber].gameObject.transform.rotation.w);
                            }
                        }

                        if (letAIControlMonster)
                        {
                            if (movementControl.monster.MonsterType != Monster.MonsterTypeEnum.Fiend)
                            {
                                MultiplayerMode.monsterPlayers[monsterNumber].gameObject.transform.rotation = monsterCamera.transform.rotation;
                            }
                            else
                            {
                                MultiplayerMode.monsterPlayers[monsterNumber].gameObject.transform.rotation = monsterCamera.transform.rotation * Quaternion.Euler(0f, 0f, 90f);
                            }
                            BaseFeatures.CustomMovementControlWorkOutSpeed(movementControl);
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("Monster number " + ManyMonstersMode.MonsterNumber(movementControl.monster.GetInstanceID()) + " is being controlled by AI in MovementControl and currently following player number " + MultiplayerMode.PlayerNumber(movementControl.monster.player.GetComponent<NewPlayerClass>().GetInstanceID()));
                            }
                        }
                        else
                        {
                            if (ModSettings.logDebugText)
                            {
                                Debug.Log("Monster number " + ManyMonstersMode.MonsterNumber(movementControl.monster.GetInstanceID()) + " is being controlled by player in MovementControl and currently following player number " + MultiplayerMode.PlayerNumber(movementControl.monster.player.GetComponent<NewPlayerClass>().GetInstanceID()));
                            }
                            float playerInput = 0f;
                            if (MultiplayerMode.customKeyBinds["Forward"][playerNumber].keyInUse == KeyCode.None)
                            {
                                // If the player is using a controller, get the value from the axis.
                                playerInput = MultiplayerMode.GetPlayerAxisValue("Y", playerNumber);
                            }
                            else
                            {
                                // If the player is not using a controller, check whether their forward key is down.
                                if (MultiplayerMode.GetPlayerKey("Forward", playerNumber).IsDown())
                                {
                                    playerInput = 1f;
                                }
                            }
                            if (playerInput > 0.1f)
                            {
                                if (MultiplayerMode.GetPlayerKey("Sprint", playerNumber).IsDown() || MultiplayerMode.GetPlayerTriggerStateIfUsingController("Left", playerNumber))
                                {
                                    movementControl.MaxSpeed = 100f;
                                }
                                else
                                {
                                    movementControl.MaxSpeed = 30f;
                                }
                                movementControl.AnimationSpeed = Mathf.MoveTowards(movementControl.AnimationSpeed, movementControl.MaxSpeed * ModSettings.monsterMovementSpeedMultiplier * playerInput, Time.deltaTime * 100f);
                            }
                            else
                            {
                                movementControl.AnimationSpeed = Mathf.MoveTowards(movementControl.AnimationSpeed, 0f, Time.deltaTime * 75f);
                            }

                            if (movementControl.AnimationSpeed > 50f)
                            {
                                movementControl.HunterMoveType = "Crawl";
                            }
                            else
                            {
                                movementControl.HunterMoveType = "Walk";
                            }

                            BaseFeatures.MovementControlBonusSpeedCheck(movementControl);
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MVentFrenzyState

            private static void HookMVentFrenzyStateMoveMonster(On.MVentFrenzyState.orig_MoveMonster orig, MVentFrenzyState mVentFrenzyState)
            {
                int monsterNumber = ManyMonstersMode.MonsterNumber(mVentFrenzyState.monster.GetInstanceID());
                if (letAIControlMonster || monsterNumber >= MultiplayerMode.monsterPlayers.Count || mVentFrenzyState.hunterAction != MVentFrenzyState.HunterAction.Moving)
                {
                    orig.Invoke(mVentFrenzyState);
                }
                else
                {
                    int playerNumber = ModSettings.numbersOfMonsterPlayers[monsterNumber];

                    /*
                    int directionMultiplier = 1;
                    if (Vector3.Angle(playerCamera.transform.forward, ((MState)mVentFrenzyState).monster.MoveControl.AheadNodePos) > 90f)
                    {
                        directionMultiplier = -1;
                    }

                    ((MState)mVentFrenzyState).monster.MoveControl.SetToFace(((MState)mVentFrenzyState).monster.MoveControl.AheadNodePos);
                    mVentFrenzyState.moveMomentum = Mathf.MoveTowards(0f, 1.5f, 3f) * directionMultiplier;
                    */

                    float playerInput = 0f;
                    if (MultiplayerMode.customKeyBinds["Forward"][playerNumber].keyInUse == KeyCode.None)
                    {
                        // If the player is using a controller, get the value from the axis.
                        playerInput = MultiplayerMode.GetPlayerAxisValue("Y", playerNumber);
                    }
                    else
                    {
                        // If the player is not using a controller, check whether their forward key is down.
                        if (MultiplayerMode.GetPlayerKey("Forward", playerNumber).IsDown())
                        {
                            playerInput = 1f;
                        }
                    }

                    if (playerInput > 0.1f)
                    {
                        Camera playerCamera = MultiplayerMode.PlayerCamera(MultiplayerMode.monsterPlayers[monsterNumber]);
                        mVentFrenzyState.moveMomentum = Mathf.MoveTowards(0f, 1.5f, 3f) * 3f * ModSettings.monsterAnimationSpeedMultiplier * playerInput;
                        Vector3 movementVector = ((MState)mVentFrenzyState).monster.transform.forward * mVentFrenzyState.moveMomentum * Time.deltaTime;
                        ((MState)mVentFrenzyState).monster.transform.position += movementVector;
                        mVentFrenzyState.frenzySource.transform.position = ((MState)mVentFrenzyState).monster.transform.position + Vector3.up;
                        if (((MState)mVentFrenzyState).monster.MoveControl.IsStuck)
                        {
                            ((MState)mVentFrenzyState).monster.MoveControl.RecalculatePath();
                        }

                        AudioSystem.PlaySound("Noises/Hunter/Movement/InVent/Run", mVentFrenzyState.frenzySource);
                    }
                    else
                    {
                        mVentFrenzyState.moveMomentum = 0f;
                    }
                }
            }

            private static void HookMVentFrenzyStateMakeNoise(On.MVentFrenzyState.orig_MakeNoise orig, MVentFrenzyState mVentFrenzyState)
            {
                int monsterNumber = ManyMonstersMode.MonsterNumber(mVentFrenzyState.monster.GetInstanceID());
                if (letAIControlMonster || monsterNumber >= MultiplayerMode.monsterPlayers.Count)
                {
                    orig.Invoke(mVentFrenzyState);
                }
                else
                {
                    int playerNumber = ModSettings.numbersOfMonsterPlayers[monsterNumber];
                    if ((MultiplayerMode.GetPlayerKey("Interact", playerNumber).JustPressed() || MultiplayerMode.GetPlayerKey("Jump", playerNumber).JustPressed() || mVentFrenzyState.hunterAction != MVentFrenzyState.HunterAction.Moving) && mVentFrenzyState.ventPoints != null && mVentFrenzyState.ventPoints.Length > 0 && mVentFrenzyState.ventMeshes != null && mVentFrenzyState.ventMeshes.Length > 0)
                    {
                        if (mVentFrenzyState.hunterAction != MVentFrenzyState.HunterAction.Moving)
                        {
                            orig.Invoke(mVentFrenzyState);
                        }
                        else
                        {
                            AmbushPoint closestVentAB = mVentFrenzyState.ventPoints[0];
                            HunterVents closestVentHV = mVentFrenzyState.ventMeshes[0];
                            float closestVentDistance = Vector3.Distance(mVentFrenzyState.monster.transform.position, mVentFrenzyState.ventPoints[0].transform.position);
                            for (int i = 1; i < mVentFrenzyState.ventPoints.Length; i++)
                            {
                                float distanceToVentToCheck = Vector3.Distance(mVentFrenzyState.monster.transform.position, mVentFrenzyState.ventPoints[i].transform.position);
                                if (distanceToVentToCheck < closestVentDistance)
                                {
                                    closestVentAB = mVentFrenzyState.ventPoints[i];
                                    closestVentHV = mVentFrenzyState.ventMeshes[i];
                                    closestVentDistance = distanceToVentToCheck;
                                }
                            }

                            if (closestVentDistance < 5f)
                            {
                                mVentFrenzyState.chosenVent = closestVentAB;
                                mVentFrenzyState.chosenMesh = closestVentHV;
                                ((MState)mVentFrenzyState).monster.SetGoal(mVentFrenzyState.chosenVent.NearestGoTo.transform.position);
                                mVentFrenzyState.chooseNewVent = false;
                                mVentFrenzyState.VentParent = mVentFrenzyState.chosenVent.transform.parent.parent.gameObject;
                                if (mVentFrenzyState.VentParent != null)
                                {
                                    mVentFrenzyState.ventPieces[0] = mVentFrenzyState.VentParent.transform.FindChild("Vent01").gameObject;
                                    mVentFrenzyState.ventPieces[1] = mVentFrenzyState.VentParent.transform.FindChild("Vent02").gameObject;
                                    mVentFrenzyState.ventPieces[2] = mVentFrenzyState.VentParent.transform.FindChild("Vent03").gameObject;
                                }
                                ((MState)mVentFrenzyState).monster.MoveControl.RecalculatePath();
                                mVentFrenzyState.triedCurrentVent = false;

                                mVentFrenzyState.forceAttack = true;

                                mVentFrenzyState.hunterAction = MVentFrenzyState.HunterAction.Vent1;

                                orig.Invoke(mVentFrenzyState);
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