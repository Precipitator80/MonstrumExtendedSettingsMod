// ~Beginning Of File
using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using MonoMod.RuntimeDetour;
using System.Diagnostics;
using System.Text;
using System.Linq;
using System.IO;
using UnityEngine.UI;

namespace MonstrumExtendedSettingsMod
{

    public partial class ExtendedSettingsModScript
    {
        /*----------------------------------------------------------------------------------------------------*/
        // ~BaseFeatures

        private static class BaseFeatures
        {
            // #InitialiseBaseFeatures
            public static void InitialiseBaseFeatures()
            {
                Debug.Log("INITIALISING BASE FEATURES");
                // Reading Mod Settings Again
                On.StartGame.ButtonPressed += new On.StartGame.hook_ButtonPressed(HookStartGameButtonPressed); // Trying to log text in a constructor does not work. Debug.Log behavior in a constructor - Foniks - https://forum.unity.com/threads/debug-log-behavior-in-a-constructor.35435/ - Accessed 04.07.2021

                // FOV
                On.CameraFOV.Awake += new On.CameraFOV.hook_Awake(HookCameraFOV);
                On.FOVCheck.Start += new On.FOVCheck.hook_Start(HookFOVCheck);
                HookFOVSlider();

                // Inventory Size
                if (!ModSettings.enableMultiplayer)
                {
                    On.Inventory.Start += new On.Inventory.hook_Start(HookInventoryStart);
                }

                // Timescale
                HookTimeScaleManager();

                // Player Movement Speed
                On.PlayerMotor.ClampSpeed += new On.PlayerMotor.hook_ClampSpeed(HookPlayerMotorClampSpeed);

                // Monster Selection, Seed, Starter Room Location & Wallhacks Mode
                HookLevelGeneration();

                // Monster Animation Speed Multiplier
                On.AnimationControl.Start += new On.AnimationControl.hook_Start(HookAnimationControlStart);

                // Monster Movement Speed Multiplier
                if (!ModSettings.enableCrewVSMonsterMode)
                {
                    HookMovementControl();
                }

                // Monster Spawntime, Spawn Monster In Starter Room, Varying Monster Sizes & Invisible Monsters
                HookMonsterStarter();

                // Monster Always Finds You
                On.MRoomSearch.ChanceOfFindingPlayer += new On.MRoomSearch.hook_ChanceOfFindingPlayer(HookMRoomSearchChanceOfFindingPlayer);

                // No Monster Stun Immunity
                On.AnimationControl.CheckIfStunned += new On.AnimationControl.hook_CheckIfStunned(HookAnimationControlCheckIfStunned);

                // Fiend Flicker Range
                HookFiendAura();

                // Overpowered Hunter & Aggressive Hunter
                On.MHuntingState.StateChanges += new On.MHuntingState.hook_StateChanges(HookMHuntingState);

                // Dark Ship Mode, Powerable Lights & Ship Generic Lights Colour, Intensity Multiplier & Range Multiplier
                HookGenericLight();

                // No Pre-filled Fuse Boxes
                On.FuseBoxManager.SetupFuse += new On.FuseBoxManager.hook_SetupFuse(HookFuseBoxManagerSetupFuse);

                // No Cameras
                On.SecurityCamera.Start += new On.SecurityCamera.hook_Start(HookSecurityCamera);

                // No Steam
                On.SteamVentManager.Awake += new On.SteamVentManager.hook_Awake(HookSteamVentManager);

                // Colour & Light Settings (Except Brute Light and Ship Generic Light)
                On.Flashlight.Start += new On.Flashlight.hook_Start(HookFlashlightStart);
                On.GlowStick.Start += new On.GlowStick.hook_Start(HookGlowStick);

                // Debug Mode, Invincibility Mode & Extra Lives
                On.EscapeRoute.OnInteract += new On.EscapeRoute.hook_OnInteract(HookEscapeRoute);
                On.FiendMindAttack.Update += new On.FiendMindAttack.hook_Update(HookFiendMindAttackUpdate);
                if (!ModSettings.startedWithMMM)
                {
                    On.GlobalMusic.CheckMusicState += new On.GlobalMusic.hook_CheckMusicState(HookGlobalMusic);
                }
                On.MChasingState.StateChanges += new On.MChasingState.hook_StateChanges(HookMChasingStateStateChanges);
                HookNoClipFixes();
                HookPlayerHealth();
                On.PlayerMotor.HandleFallDamage += new On.PlayerMotor.hook_HandleFallDamage(HookPlayerMotorHandleFallDamage);
                //On.TutorialLockerModelSwap.Update += new On.TutorialLockerModelSwap.hook_Update(HookTutorialLockerModelSwap); // Debug text & Death countdown were moved to MonsterStarter.

                // Loading Background
                On.LoadingBackground.Awake += new On.LoadingBackground.hook_Awake(HookLoadingBackground);
                Debug.Log("INITIALISED BASE FEATURES");

                // No Brute Light & Brute Light Colour
                HookLightShafts();


                // Indev / New Area

                // Achievements
                On.Achievements.CompleteAchievement += new On.Achievements.hook_CompleteAchievement(HookAchievements);

                // Door Alteration
                if (ModSettings.useCustomDoors)
                {
                    On.SpawnDoorsLG.GenerateDoors += new On.SpawnDoorsLG.hook_GenerateDoors(HookSpawnDoorsLGGenerateDoors);
                    On.SpawnFuseBoxNearby.OnGenerationComplete += new On.SpawnFuseBoxNearby.hook_OnGenerationComplete(HookSpawnFuseBoxNearbyOnGenerationComplete);
                    HookTutorialDoor();
                }

                // Menu Screen Skip
                HookSplashMovie(); // Instantly start the loading process and do not play the splash movie.
                On.StartGame.Start += new On.StartGame.hook_Start(HookStartGameStart); // Press the new game button as soon as the menu has loaded.
                On.MouseLock.LateUpdate += new On.MouseLock.hook_LateUpdate(HookMouseLockLateUpdate); // Stop the mouse lock from updating until the level has finished generating if the menu has been skipped.
                On.MenusEventSystemManager.Delay += new On.MenusEventSystemManager.hook_Delay(HookMenusEventSystemManagerDelay);

                // Disable Monster Particle Systems
                HookMonster();

                // Increase Key Items
                // On.KeyItem.RandomizeCount += new On.KeyItem.hook_RandomizeCount(HookKeyItemRandomizeCount); // Integrated into Spawn Deactivated Items.

                // No Player Jump Cooldown In Debug Mode (Integrated Into Debug Mode)
                On.PlayerMotor.PerformJump += new On.PlayerMotor.hook_PerformJump(HookPlayerMotorPerformJump);

                // Invincible Pit Traps
                if (!ModSettings.enableMultiplayer)
                {
                    On.PitTrap.DestroyFloor += new On.PitTrap.hook_DestroyFloor(HookPitTrapDestroyFloor);
                }

                // Overpowered Steam Vents
                On.SteamVariation.Start += new On.SteamVariation.hook_Start(HookSteamVariationStart);

                // In-game MESM Options
                On.OptionsUI.Start += new On.OptionsUI.hook_Start(HookOptionsUIStart);

                // Glowstick Hunt
                On.GlowStick.ActivateGlowstick += new On.GlowStick.hook_ActivateGlowstick(HookGlowStickActivateGlowstick);

                // Overpowered Flare Gun
                On.FlareGun.Update += new On.FlareGun.hook_Update(HookFlareGunUpdate);
                On.FlareObject.HitMonster += new On.FlareObject.hook_HitMonster(HookFlareObjectHitMonster);
                On.FlareObject.Start += new On.FlareObject.hook_Start(HookFlareObjectStart);

                // Spawn Deactivated Items, Change Key Item Spawn Numbers & Diverse Spawns
                On.KeyItemSystem.SetUpLists += new On.KeyItemSystem.hook_SetUpLists(HookKeyItemSystemSetUpLists);
                On.KeyItemPlaceholder.CalculateSuitability += new On.KeyItemPlaceholder.hook_CalculateSuitability(HookKeyItemPlaceholderCalculateSuitability);

                // Infinite Flashlight Power
                HookFlashlight();

                // Teleport Through Door
                On.MDestroyState.OnEnter += new On.MDestroyState.hook_OnEnter(HookMDestroyStateOnEnter);

                // Walkie Talkie Fix
                HookWalkieTalkie();

                // Monster Compass
                On.CompassScript.Update += new On.CompassScript.hook_Update(HookCompassScriptUpdate);

                // Unlock Player Head
                if (!ModSettings.enableMultiplayer)
                {
                    On.MouseLookCustom.Start += new On.MouseLookCustom.hook_Start(HookMouseLookCustomStart);
                    On.MouseLookCustom.Update += new On.MouseLookCustom.hook_Update(HookMouseLookCustomUpdate);
                }

                // Custom Stair Support
                On.SpawnStairsLG.SpawnStairStack += new On.SpawnStairsLG.hook_SpawnStairStack(HookSpawnStairsLGSpawnStairStack);

                // Add Additional Crew Deck Building Support
                On.CraneChain.Start += new On.CraneChain.hook_Start(HookCraneChainStart);
                On.RaftEscapeCheck.Update += new On.RaftEscapeCheck.hook_Update(HookRaftEscapeCheckUpdate);
                On.FuseBoxManager.AddFuseBox += new On.FuseBoxManager.hook_AddFuseBox(HookFuseBoxManagerAddFuseBox);
                if (!ModSettings.enableMultiplayer)
                {
                    On.DuctTape.OnFinishItemAnimation += new On.DuctTape.hook_OnFinishItemAnimation(HookDuctTapeOnFinishItemAnimation);
                }

                // Infinite Fuel Can Fuel & Fire Duration Multiplier
                On.FuelParticles.Update += new On.FuelParticles.hook_Update(HookFuelParticlesUpdate);
                On.FuelDecal.Start += new On.FuelDecal.hook_Start(HookFuelDecalStart);

                // Infinite Fire Extinguisher Fuel
                On.FireExtinguisher.Update += new On.FireExtinguisher.hook_Update(HookFireExtinguisherUpdate);

                // Speedrun Timer
                On.FadeScript.BeginFadeIn += new On.FadeScript.hook_BeginFadeIn(HookFadeScriptBeginFadeIn);

                // Notification Formatting
                On.TriggerNotification.SetNotificationString += new On.TriggerNotification.hook_SetNotificationString(HookTriggerNotificationSetNotificationString);

                // Frequent Level Generation Crash Fix
                On.NoCullingAppendage.CheckNoCullingJoints += new On.NoCullingAppendage.hook_CheckNoCullingJoints(HookNoCullingAppendageCheckNoCullingJoints);

                /*
                // Let All Monsters Lock Doors
                On.MChasingState.DoDoorCheck += new On.MChasingState.hook_DoDoorCheck(HookMChasingStateDoDoorCheck);
                if (!ModSettings.startedWithMMM)
                {
                    On.MChasingState.OnEnter += new On.MChasingState.hook_OnEnter(HookMChasingStateOnEnter);
                }
                */

                // Invisible Mode
                if (!ModSettings.startedWithMMM)
                {
                    On.Vision.PlayerVision += new On.Vision.hook_PlayerVision(HookVisionPlayerVision);
                }

                // Give All Monsters A Fiend Aura
                if (!ModSettings.startedWithMMM)
                {
                    On.FiendLightController.LateUpdate += new On.FiendLightController.hook_LateUpdate(HookFiendLightControllerLateUpdate);
                }

                // Alpha Bridge
                //On.Room.SpawnRoomModel += new On.Room.hook_SpawnRoomModel(HookRoomSpawnRoomModel);


                // Smoky Ship, Always Smoky & Smoke Monster
                On.Room.OnPowerUp += new On.Room.hook_OnPowerUp(HookRoomOnPowerUp);
                On.Room.OnPowerDown += new On.Room.hook_OnPowerDown(HookRoomOnPowerDown);

                // Fire Shroud Fire Blast
                if (!ModSettings.useSparky)
                {
                    On.MChasingState.DoDoorCheck += new On.MChasingState.hook_DoDoorCheck(HookMChasingStateDoDoorCheck);
                }

                // Increase Map Size
                On.RegionManager.InitialiseRegionData += new On.RegionManager.hook_InitialiseRegionData(HookRegionManagerInitialiseRegionData);

                // Monsters Search Randomly [& Moved Multiplayer Function]
                On.MWanderState.OnEnter += new On.MWanderState.hook_OnEnter(HookMWanderStateOnEnter);

                // MonsterHearing Player room is null fix.
                //On.MonsterHearing.FindHidingSpot += new On.MonsterHearing.hook_FindHidingSpot(HookMonsterHearingFindHidingSpot); // Crashes the game.
                //new Hook(typeof(MonsterHearing).GetMethod("FindHidingSpot", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.BaseFeatures).GetMethod("HookMonsterHearingFindHidingSpot"), null); // Also crashes the game.

                // Looping Radio
                On.RadioChannel.GetTrack += new On.RadioChannel.hook_GetTrack(HookRadioChannelGetTrack);
                On.RadioChannel.GetTimeInTrack += new On.RadioChannel.hook_GetTimeInTrack(HookRadioChannelGetTimeInTrack);

                // Better Smashables
                On.Smashable.OnCollisionEnter += new On.Smashable.hook_OnCollisionEnter(HookSmashableOnCollisionEnter);

                // Molotov and Smoke Grenade
                On.Smashable.Start += new On.Smashable.hook_Start(HookSmashableStart);

                // No Time Freeze In Pause Menu
                On.PauseMenu.TogglePause += new On.PauseMenu.hook_TogglePause(HookPauseMenuTogglePause);
                On.PauseMenu.Update += new On.PauseMenu.hook_Update(HookPauseMenuUpdate);

                // Hide Inventory
                On.Inventory.DisplayInventory += new On.Inventory.hook_DisplayInventory(HookInventoryDisplayInventory);

                // DeathMenu
                On.DeathMenu.Start += new On.DeathMenu.hook_Start(HookDeathMenuStart);
                On.MAttackingState2.KillThePlayer += new On.MAttackingState2.hook_KillThePlayer(HookMAttackingState2KillThePlayer);
                On.ChooseAttack.WhatDeathByPlayer += new On.ChooseAttack.hook_WhatDeathByPlayer(HookChooseAttackWhatDeathByPlayer);

                // Fire Steam Damage Fix and Moved Multiplayer Component
                On.FireDamage.Start += new On.FireDamage.hook_Start(HookFireDamageStart);

                // Alternating Monsters, Multiplayer and Persistent Monster
                On.MChasingState.Chase += new On.MChasingState.hook_Chase(HookMChasingStateChase);

                // Many Monsters Mode and Persistent Monster
                On.MClimbingState.OnEnter += new On.MClimbingState.hook_OnEnter(HookMClimbingStateOnEnter);
                On.MClimbingState.FinishClimb += new On.MClimbingState.hook_FinishClimb(HookMClimbingStateFinishClimb);

                // Fiend Mind Attack Customisation
                if (!ModSettings.startedWithMMM)
                {
                    On.FiendMindAttack.AttackCheck += new On.FiendMindAttack.hook_AttackCheck(HookFiendMindAttackAttackCheck);
                    On.FiendMindAttack.ChangeTimers += new On.FiendMindAttack.hook_ChangeTimers(HookFiendMindAttackChangeTimers);
                }

                // Disable Running
                if (!ModSettings.enableMultiplayer)
                {
                    On.NewPlayerClass.ButtonInput += new On.NewPlayerClass.hook_ButtonInput(HookNewPlayerClassButtonInput);
                }

                // Overpowered Hiding Spots
                new Hook(typeof(Hiding).GetProperty("IsHiding", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetGetMethod(), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.BaseFeatures).GetMethod("HookHidingget_IsHiding"), null);

                // No Hiding
                if (!ModSettings.startedWithMMM)
                {
                    On.MRoomSearch.OnEnter += new On.MRoomSearch.hook_OnEnter(HookMRoomSearchOnEnter);
                }
            }

            /*
            // Not actually sure how to use the dictionary implementation for the enum.
            // https://stackoverflow.com/questions/2779743/can-you-add-to-an-enum-type-in-run-time
            public static Dictionary<int, string> monsterDictionary = new Dictionary<int, string>();
            monsterDictionary.Add(3, "Sparky");
            */

            /*----------------------------------------------------------------------------------------------------*/
            // @Achievements

            private static void HookAchievements(On.Achievements.orig_CompleteAchievement orig, Achievements achievements, string _identifier)
            {
                if (achievements.achievements[_identifier].completed == false)
                {
                    orig.Invoke(achievements, _identifier);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Active Features

            private static void ActiveFeatures()
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    // Debug & 0 Monsters Mode Monster Disabler
                    if ((ModSettings.debugMode && Input.GetKeyDown(KeyCode.Period)) || (ModSettings.numberOfMonsters == 0 && References.Monster.activeInHierarchy))
                    {
                        if (ModSettings.numberOfMonsters > 1 && ManyMonstersMode.monsterList != null)
                        {
                            for (int i = 0; i < ManyMonstersMode.monsterList.Count; i++)
                            {
                                ManyMonstersMode.monsterList[i].SetActive(false);
                            }
                        }
                        else
                        {
                            References.Monster.SetActive(false);
                        }

                        if ((ModSettings.debugMode && Input.GetKeyDown(KeyCode.Period)))
                        {
                            ModSettings.ShowTextOnScreen("Disabled Monster(s)");
                        }
                    }

                    // Grouping Feature Frame Update
                    if (ModSettings.useMonsterUpdateGroups && ModSettings.numberOfMonsters > 1)
                    {
                        if (ManyMonstersMode.groupCounter < ModSettings.NumberOfMonsterUpdateGroups)
                        {
                            ManyMonstersMode.groupCounter++;
                        }
                        else
                        {
                            ManyMonstersMode.groupCounter = 1;
                        }
                    }

                    if (ModSettings.showSpeedrunTimerOnScreen)
                    {
                        if (ModSettings.finalTime.Equals(string.Empty))
                        {
                            ModSettings.ShowTextOnScreen(Mathf.FloorToInt(ModSettings.speedrunTimer.TimeElapsed / 60f).ToString() + ":" + (ModSettings.speedrunTimer.TimeElapsed % 60f).ToString("00.00"));
                        }
                        else
                        {
                            ModSettings.ShowTextOnScreen(ModSettings.finalTime);
                        }
                    }
                    else if (ModSettings.scavengerMode)
                    {
                        if (nearbyInventoryItem == null || nearbyInventoryItem.inInventory)
                        {
                            bool validItem = false;
                            InventoryItem[] inventoryItems = FindObjectsOfType<InventoryItem>();
                            while (!validItem)
                            {
                                int randomIndex = UnityEngine.Random.Range(0, inventoryItems.Length);
                                nearbyInventoryItem = inventoryItems[randomIndex];

                                if (!nearbyInventoryItem.inInventory)
                                {
                                    validItem = true;
                                }
                            }
                            float itemHeight = RegionManager.Instance.ConvertPointToRegionNode(nearbyInventoryItem.transform.position).y; //nearbyInventoryItem.transform.position.y;
                            nearbyInventoryItemDeck = (int)itemHeight;
                        }
                        int distanceToItem = Mathf.RoundToInt(Vector3.Distance(nearbyInventoryItem.transform.position, References.player.transform.position)); // (int)Math.Round((nearbyInventoryItem.transform.position - References.player.transform.position).magnitude); // Can use Vector3.Distance instead.

                        ModSettings.ShowTextOnScreen("You are " + distanceToItem + "m away from the item " + nearbyInventoryItem.itemName + ", which is on deck " + nearbyInventoryItemDeck + ".");
                    }
                    else if (ModSettings.monsterRadar && ModSettings.numberOfMonsters > 0)
                    {
                        Monster closestMonster;
                        if (ModSettings.numberOfMonsters == 1)
                        {
                            closestMonster = References.Monster.GetComponent<Monster>();
                        }
                        else
                        {
                            closestMonster = ManyMonstersMode.monsterListMonsterComponents[ManyMonstersMode.ClosestMonsterToPlayer()];
                        }
                        int distanceToMonster = Mathf.RoundToInt(Vector3.Distance(closestMonster.transform.position, References.player.transform.position)); // (int)Math.Round((closestMonster.transform.position - References.player.transform.position).magnitude); // Can use Vector3.Distance instead.
                        int distanceToRoot = Mathf.RoundToInt(Vector3.Distance(closestMonster.monsterMesh[0].rootBone.transform.position, References.player.transform.position)); // (int)Math.Round((closestMonster.monsterMesh[0].rootBone.transform.position - References.player.transform.position).magnitude); // Can use Vector3.Distance instead.

                        // If the monster is out of ship bounds, use its height instead.
                        float closestMonsterDeck;
                        try
                        {
                            closestMonsterDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(closestMonster.transform.position).y;
                        }
                        catch
                        {
                            closestMonsterDeck = (int)closestMonster.transform.position.y;
                            Debug.Log("Closest monster to player, " + closestMonster.monsterType + ", does not seem to be in bounds.");
                        }

                        ModSettings.ShowTextOnScreen("You are " + distanceToMonster + "m away from the monster " + closestMonster.monsterType + ", which is on deck " + closestMonsterDeck + ". Is the monster visible? " + closestMonster.monsterMesh[0].isVisible + ". Root is " + distanceToRoot + "m away.");
                    }
                    else if (ModSettings.glowstickRadar)
                    {
                        // # Optimise this using a pre-declared list that is checked where glowsticks are removed when they are used. Could also let the user press a key to update the item. Might also be good for scavenger mode.
                        GlowStick[] allGlowsticks = FindObjectsOfType<GlowStick>();
                        List<GlowStick> unusedGlowsticks = new List<GlowStick>();

                        foreach (GlowStick glowStick in allGlowsticks)
                        {
                            if (!glowStick.used)
                            {
                                unusedGlowsticks.Add(glowStick);
                            }
                        }

                        GlowStick closestUnusedGlowstick = unusedGlowsticks[0];

                        for (int glowstickNumber = 0; glowstickNumber < unusedGlowsticks.Count; glowstickNumber++)
                        {
                            if (Vector3.Distance(unusedGlowsticks[glowstickNumber].transform.position, References.Player.transform.position) < Vector3.Distance(closestUnusedGlowstick.transform.position, References.Player.transform.position))
                            {
                                closestUnusedGlowstick = unusedGlowsticks[glowstickNumber];
                            }
                        }

                        int distanceToClosestUnusedGlowstick = Mathf.RoundToInt(Vector3.Distance(closestUnusedGlowstick.transform.position, References.Player.transform.position));

                        // If the closest unused glowstick is out of ship bounds, use its height instead.
                        float closestUnusedGlowstickDeck;
                        try
                        {
                            closestUnusedGlowstickDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(closestUnusedGlowstick.transform.position).y;
                        }
                        catch
                        {
                            closestUnusedGlowstickDeck = (int)closestUnusedGlowstick.transform.position.y;
                            Debug.Log("Closest glowstick to player does not seem to be in bounds.");
                        }

                        ModSettings.ShowTextOnScreen("You are " + distanceToClosestUnusedGlowstick + "m away from a glowstick on deck " + closestUnusedGlowstickDeck + ".");
                    }
                    else if (ModSettings.playerRegionNodeText)
                    {
                        Vector3 playerRegionNode = RegionManager.Instance.ConvertPointToRegionNode(References.Player.transform.position);
                        if (CheckBoundariesLG.NodeWithinShipBounds(playerRegionNode))
                        {
                            StringBuilder stringBuilder = new StringBuilder();
                            NodeData playerRegionNodeData = LevelGeneration.GetNodeDataAtPosition(References.Player.transform.position);
                            if (playerRegionNodeData != null && playerRegionNodeData.nodeRoom != null)
                            {
                                stringBuilder.Append(playerRegionNodeData.nodeRoom.PrimaryRegion.ToString());
                            }

                            for (int i = 0; i < RegionManager.Instance.regionData[(int)playerRegionNode.x].regionDataY[(int)playerRegionNode.y].regionDataZ[(int)playerRegionNode.z].regionID.Count; i++)
                            {
                                string regionString = RegionManager.Instance.IDToName(RegionManager.Instance.regionData[(int)playerRegionNode.x].regionDataY[(int)playerRegionNode.y].regionDataZ[(int)playerRegionNode.z].regionID[i]);
                                if (!regionString.Equals("Inaccessible"))
                                {
                                    stringBuilder.Append(" / ");
                                    stringBuilder.Append(regionString);
                                }
                            }

                            ModSettings.ShowTextOnScreen("Current player region node is: " + playerRegionNode + "\n(" + stringBuilder.ToString() + ")");
                        }
                        else
                        {
                            ModSettings.ShowTextOnScreen("Current player region node is: " + playerRegionNode + "\n(Out Of Bounds)");
                        }
                    }

                    // Debug Features
                    if (ModSettings.debugMode)
                    {
                        if (ModSettings.BreakTheGameLight)
                        {
                            if (LevelGeneration.Instance.finishedGenerating || ModSettings.BreakTheGameHeavy)
                            {
                                LevelGeneration.Instance.selectedMonster = null;
                                References.monster = null;
                            }
                        }

                        if (Input.GetKeyDown(KeyCode.V))
                        {
                            ModSettings.noclip = !ModSettings.noclip;
                            NewPlayerClass.Instance.playerMotor.allowFallDamage = !NewPlayerClass.Instance.playerMotor.allowFallDamage;


                            if (ModSettings.noclip)
                            {
                                ModSettings.ShowTextOnScreen("Switched On Noclip");
                            }
                            else
                            {
                                ModSettings.ShowTextOnScreen("Turned Off Noclip");
                            }
                            /*
                            if (!ModSettings.noclip)
                            {
                                // Set default keys.
                                KeyCode forwardKey = KeyCode.W;
                                KeyCode backKey = KeyCode.S;
                                KeyCode leftKey = KeyCode.A;
                                KeyCode rightKey = KeyCode.D;

                                // Get player preference keys if available.
                                if (PlayerPrefs.HasKey("SavedForwardKey"))
                                {
                                   forwardKey = GetKeyPrefCopy("SavedForwardKey", forwardKey);
                                }
                                if (PlayerPrefs.HasKey("SavedBackKey"))
                                {
                                    backKey = GetKeyPrefCopy("SavedBackKey", backKey);
                                }
                                if (PlayerPrefs.HasKey("SavedLeftKey"))
                                {
                                    leftKey = GetKeyPrefCopy("SavedLeftKey", leftKey);
                                }
                                if (PlayerPrefs.HasKey("SavedRightKey"))
                                {
                                    rightKey = GetKeyPrefCopy("SavedRightKey", rightKey);
                                }

                                // Set keys.
                                PlayerPrefs.SetInt("SavedForwardKey", (int)forwardKey);
                                PlayerPrefs.SetInt("SavedBackKey", (int)backKey);
                                PlayerPrefs.SetInt("SavedLeftKey", (int)leftKey);
                                PlayerPrefs.SetInt("SavedRightKey", (int)rightKey);
                            }*/
                        }

                        if (ModSettings.noclip)
                        {
                            if (ModSettings.enableCrewVSMonsterMode && ModSettings.numbersOfMonsterPlayers.Contains(0) && MonsterStarter.spawned)
                            {
                                if (Input.GetKey(KeyCode.U))
                                {
                                    References.Monster.transform.position = References.Monster.transform.position + Camera.main.transform.forward * 7.5f * Time.deltaTime;
                                }
                                if (Input.GetKey(KeyCode.H))
                                {
                                    References.Monster.transform.position = References.Monster.transform.position + Camera.main.transform.right * -7.5f * Time.deltaTime;
                                }
                                if (Input.GetKey(KeyCode.J))
                                {
                                    References.Monster.transform.position = References.Monster.transform.position + Camera.main.transform.forward * -7.5f * Time.deltaTime;
                                }
                                if (Input.GetKey(KeyCode.K))
                                {
                                    References.Monster.transform.position = References.Monster.transform.position + Camera.main.transform.right * 7.5f * Time.deltaTime;
                                }
                            }
                            else
                            {
                                if (Input.GetKey(KeyCode.U))
                                {
                                    References.Player.transform.position = References.Player.transform.position + Camera.main.transform.forward * 7.5f * Time.deltaTime;
                                }
                                if (Input.GetKey(KeyCode.H))
                                {
                                    References.Player.transform.position = References.Player.transform.position + Camera.main.transform.right * -7.5f * Time.deltaTime;
                                }
                                if (Input.GetKey(KeyCode.J))
                                {
                                    References.Player.transform.position = References.Player.transform.position + Camera.main.transform.forward * -7.5f * Time.deltaTime;
                                }
                                if (Input.GetKey(KeyCode.K))
                                {
                                    References.Player.transform.position = References.Player.transform.position + Camera.main.transform.right * 7.5f * Time.deltaTime;
                                }
                            }
                        }

                        if (Input.GetKeyDown(KeyCode.Comma))
                        {
                            if (ModSettings.numberOfMonsters > 1 && ManyMonstersMode.monsterList != null)
                            {
                                for (int i = 0; i < ManyMonstersMode.monsterList.Count; i++)
                                {
                                    ManyMonstersMode.monsterList[i].SetActive(true);
                                }

                            }
                            else
                            {
                                References.Monster.SetActive(true);
                            }

                            ModSettings.ShowTextOnScreen("Enabled Monster(s)");
                        }

                        if (Input.GetKeyDown(KeyCode.O))
                        {
                            ModSettings.ShowTextOnScreen("Started Helicopter Escape Sequence");

                            HelicopterEscape helicopterEscape = (UnityEngine.Object.FindObjectOfType(typeof(HelicopterEscape)) as HelicopterEscape);
                            foreach (HelicopterChain chain in helicopterEscape.chains)
                            {
                                chain.Break();
                            }

                            HeliLock heliLock = (UnityEngine.Object.FindObjectOfType(typeof(HeliLock)) as HeliLock);
                            heliLock.OnFinishFixedAnimation();

                            HeliLockStatus heliLockStatus = (UnityEngine.Object.FindObjectOfType(typeof(HeliLockStatus)) as HeliLockStatus);
                            heliLockStatus.OnStartFixedAnimation();

                            HeliDoor heliDoor = (UnityEngine.Object.FindObjectOfType(typeof(HeliDoor)) as HeliDoor);
                            heliDoor.OnHandGrab();

                            SlerpFuelTrolley slerpFuelTrolley = (UnityEngine.Object.FindObjectOfType(typeof(SlerpFuelTrolley)) as SlerpFuelTrolley);
                            slerpFuelTrolley.StartTrolleySlerp();

                            FuelPump fuelPump = (UnityEngine.Object.FindObjectOfType(typeof(FuelPump)) as FuelPump);
                            fuelPump.AddFuel(fuelPump.maxFuel);

                            FuelConnection fuelConnection = (UnityEngine.Object.FindObjectOfType(typeof(FuelConnection)) as FuelConnection);
                            FuelPipeEnd fuelPipeEnd = (UnityEngine.Object.FindObjectOfType(typeof(FuelPipeEnd)) as FuelPipeEnd);
                            FuelPumpLever fuelPumpLever = (UnityEngine.Object.FindObjectOfType(typeof(FuelPumpLever)) as FuelPumpLever);
                            fuelConnection.StartCoroutine(ConnectHelicopterHoseWhenReady(fuelConnection, fuelPipeEnd, fuelPumpLever));
                        }

                        if (Input.GetKeyDown(KeyCode.P))
                        {
                            ModSettings.ShowTextOnScreen("Started Submersible Escape Sequence");

                            SubAlarm subAlarm = (UnityEngine.Object.FindObjectOfType(typeof(SubAlarm)) as SubAlarm);
                            subAlarm.forGary = true;
                            subAlarm.StartTheEvent();
                        }

                        if (Input.GetKeyDown(KeyCode.I))
                        {
                            TimeScaleManager.Instance.StartCoroutine(ReadyLiferaft(true));
                        }

                        if (Input.GetKeyDown(KeyCode.L))
                        {
                            UnityEngine.Object.Instantiate<GameObject>(UnityEngine.Object.FindObjectOfType<Radio>().gameObject, References.Player.transform.position + References.Player.transform.up + References.Player.transform.forward, References.Player.transform.rotation);
                            if (ModSettings.spawnDeactivatedItems)
                            {
                                UnityEngine.Object.Instantiate<GameObject>(UnityEngine.Object.FindObjectOfType<WalkieTalkie>().gameObject, References.Player.transform.position + References.Player.transform.up + References.Player.transform.forward, References.Player.transform.rotation);
                                UnityEngine.Object.Instantiate<GameObject>(UnityEngine.Object.FindObjectOfType<WalkieTalkie>().gameObject, References.Player.transform.position + References.Player.transform.up + References.Player.transform.forward, References.Player.transform.rotation);
                                UnityEngine.Object.Instantiate<GameObject>(UnityEngine.Object.FindObjectOfType<CompassScript>().gameObject, References.Player.transform.position + References.Player.transform.up + References.Player.transform.forward, References.Player.transform.rotation);
                            }
                            /*
                            foreach (InventorySlot inventorySlot in References.Inventory.inventorySlots)
                            {
                                if (inventorySlot == null)
                                {
                                    WalkieTalkie walkieTalkie = References.PlayerClass.gameObject.AddComponent<WalkieTalkie>();
                                    if (walkieTalkie != null)
                                    {
                                        inventorySlot.AddItem(walkieTalkie.item);
                                    }
                                    else
                                    {
                                        Debug.Log("Walkie Talkie is null");
                                    }
                                    break;
                                }
                            }
                            */
                        }

                        if (Input.GetKeyDown(KeyCode.Semicolon))
                        {
                            for (int i = 0; i < 10; i++)
                            {
                                UnityEngine.Object.Instantiate<GameObject>(UnityEngine.Object.FindObjectOfType<Fuse>().gameObject, References.Player.transform.position + References.Player.transform.up + References.Player.transform.forward, References.Player.transform.rotation);
                            }
                        }

                        if (Input.GetKeyDown(KeyCode.R) && ModSettings.startedWithMMM)
                        {
                            if (ModSettings.numberOfBrutes > 0)
                            {
                                ModSettings.ShowTextOnScreen("Spawning Brute");
                                ManyMonstersMode.CreateNewMonster(0f, "Brute");
                            }
                            else
                            {
                                ModSettings.ShowTextOnScreen("Could not spawn a Brute as the game was started without any");
                            }
                        }

                        if (Input.GetKeyDown(KeyCode.T) && ModSettings.startedWithMMM)
                        {
                            if (ModSettings.numberOfHunters > 0)
                            {
                                ModSettings.ShowTextOnScreen("Spawning Hunter");
                                ManyMonstersMode.CreateNewMonster(0f, "Hunter");
                            }
                            else
                            {
                                ModSettings.ShowTextOnScreen("Could not spawn a Hunter as the game was started without any");
                            }
                        }

                        if ((Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(KeyCode.Z)) && ModSettings.startedWithMMM)
                        {
                            if (ModSettings.numberOfFiends > 0)
                            {
                                ModSettings.ShowTextOnScreen("Spawning Fiend");
                                ManyMonstersMode.CreateNewMonster(0f, "Fiend");
                            }
                            else
                            {
                                ModSettings.ShowTextOnScreen("Could not spawn a Fiend as the game was started without any");
                            }
                        }

                        /* Attempt at letting the user reverse the effects of debug mode on the walls. This seems to be more complicated than at first glance. The "wallhacks" seem to occur due to a lack of room collision of some kind.
                        if (Input.GetKeyDown(KeyCode.L))
                        {
                            Room[] rooms = Resources.FindObjectsOfTypeAll(typeof(Room)) as Room[];

                            foreach(Room room in rooms)
                            {

                            }
                        }
                        */

                        if (Input.GetKeyDown(KeyCode.Quote))
                        {
                            InventoryItem[] inventoryItems = FindObjectsOfType<InventoryItem>();
                            foreach (InventoryItem inventoryItem in inventoryItems)
                            {
                                if (inventoryItem != null && !inventoryItem.IsInInventory())
                                {
                                    inventoryItem.gameObject.transform.position = References.Player.transform.position + new Vector3(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0.5f, 1f), UnityEngine.Random.Range(-1f, 1f));
                                }
                            }
                            /*
                            GlowStick[] allGlowsticks = FindObjectsOfType<GlowStick>();
                            foreach (GlowStick glowStick in allGlowsticks)
                            {
                                glowStick.transform.position = References.Player.transform.position;
                            }
                            */
                        }

                        if (Input.GetKeyDown(KeyCode.G))
                        {
                            // Numbers from 1 = (false, false).
                            // 1 -> 2 | (false, false) -> (true, false) | Switch on godmode.
                            // 2 -> 3 | (true, false) -> (true, true) | Switch on invisibility.
                            // 3 -> 4 | (true, true) -> (false, turn) | Switch off godmode.
                            // 4 -> 1 | (false, true) -> (false, false) | Switch off invisibility.
                            if ((ModSettings.invincibilityMode[0] && ModSettings.InvisibleMode) || (!ModSettings.invincibilityMode[0] && !ModSettings.InvisibleMode))
                            {
                                ModSettings.startedWithInvincibilityMode = !ModSettings.startedWithInvincibilityMode;
                                ModSettings.SetInvincibilityMode(!ModSettings.invincibilityMode[0], true);
                            }
                            else
                            {
                                ModSettings.InvisibleMode = !ModSettings.InvisibleMode;
                            }

                            /*
                            if (ModSettings.invincibilityMode[0])
                            {
                                if (ModSettings.InvisibleMode)
                                {
                                    ModSettings.SetInvincibilityMode(false, true);
                                }
                                else
                                {
                                    ModSettings.InvisibleMode = true;
                                }
                            }
                            else
                            {
                                if (ModSettings.InvisibleMode)
                                {
                                    ModSettings.InvisibleMode = false;
                                }
                                else
                                {
                                    ModSettings.SetInvincibilityMode(true, true);
                                }
                            }
                            */

                            ModSettings.ShowTextOnScreen("Godmode " + (ModSettings.invincibilityMode[0] ? "✓" : "×") + " | Invisibility " + (ModSettings.InvisibleMode ? "✓" : "×"));
                        }

                        if (Input.GetKeyDown(KeyCode.B))
                        {
                            ModSettings.ForceChase();
                        }

                        if (Input.GetKeyDown(KeyCode.N))
                        {
                            ModSettings.ForceStopChase();
                        }

                        if (Input.GetKeyDown(KeyCode.M))
                        {
                            ModSettings.BurnHunter();
                        }

                        if (Input.GetKeyDown(KeyCode.Backspace))
                        {
                            References.Monster.transform.position = References.Player.transform.position;
                            References.Monster.transform.rotation = References.Player.transform.rotation;

                            //if(ModSettings.simpleSparkyTest /* && ModSettings.finishedCreatingSimpleSparky*/)
                            //SparkyMode.SimpleSparkyModelTest();

                            /*
                            SkinnedMeshRenderer sparkySMR = SparkyMode.simpleSparkyGO.GetComponentInChildren<SkinnedMeshRenderer>();
                            if (sparkySMR == null)
                            {
                                Debug.Log("Sparky SMR is null in ActiveFeatures!");
                            }
                            else
                            {
                                if (sparkySMR.rootBone == null)
                                {
                                    Debug.Log("Sparky root bone is null in ActiveFeatures!");
                                }
                                else
                                {
                                    int distanceToMonster = Mathf.RoundToInt(Vector3.Distance(SparkyMode.simpleSparkyGO.transform.position, References.Player.transform.position)); // (int)Math.Round((closestMonster.transform.position - References.player.transform.position).magnitude); // Can use Vector3.Distance instead.
                                    int distanceToRoot = Mathf.RoundToInt(Vector3.Distance(sparkySMR.rootBone.position, References.Player.transform.position)); // (int)Math.Round((closestMonster.monsterMesh[0].rootBone.transform.position - References.player.transform.position).magnitude); // Can use Vector3.Distance instead.

                                    // If the monster is out of ship bounds, use its height instead.
                                    float closestMonsterDeck;
                                    try
                                    {
                                        closestMonsterDeck = (int)RegionManager.Instance.ConvertPointToRegionNode(SparkyMode.simpleSparkyGO.transform.position).y;
                                    }
                                    catch
                                    {
                                        closestMonsterDeck = (int)SparkyMode.simpleSparkyGO.transform.position.y;
                                        Debug.Log("Simple Sparky GO does not seem to be in bounds.");
                                    }

                                    ModSettings.ShowTextOnScreen("You are " + distanceToMonster + "m away from simple sparky, which is on deck " + closestMonsterDeck + ". Is the SMR visible? " + sparkySMR.isVisible + ". Root is " + distanceToRoot + "m away.");
                                }
                            }
                            */
                        }
                    }

                    if (ModSettings.playerStaminaMode)
                    {
                        for (int i = 0; i < ModSettings.staminaTimer.Count; i++)
                        {
                            NewPlayerClass newPlayerClass;
                            if (!ModSettings.enableMultiplayer)
                            {
                                newPlayerClass = References.PlayerClass;
                            }
                            else
                            {
                                newPlayerClass = MultiplayerMode.crewPlayers[i];
                            }
                            /*
                            float staminaModeExponential = Mathf.Exp(ModSettings.staminaModeHorizontalShiftConstant - ModSettings.staminaModeTimeCoefficient * ModSettings.staminaTimer);
                            ModSettings.playerMovementSpeedDynamicMultiplier = ModSettings.staminaModeVerticalShiftConstant + ModSettings.staminaModeVerticalScalingCoefficient * (staminaModeExponential / (ModSettings.playerMovementSpeedEndMultiplier + ModSettings.staminaModeMaximumMultiplierChange * staminaModeExponential));
                            */
                            ModSettings.playerMovementSpeedDynamicMultiplier[i] = ModSettings.playerMovementSpeedEndMultiplier + ModSettings.staminaModeMaximumMultiplierChange / (1f + Mathf.Exp(ModSettings.staminaModeTimeCoefficient * ModSettings.staminaTimer[i] - 5));

                            if (References.PlayerClass.IsRunning())
                            {
                                ModSettings.staminaTimer[i] += Time.deltaTime;
                                //ModSettings.playerMovementSpeedDynamicMultiplier = Mathf.SmoothDamp(ModSettings.playerMovementSpeedDynamicMultiplier, ModSettings.playerMovementSpeedEndMultiplier, ref staminaSpeed, 5f);
                                //ModSettings.playerMovementSpeedDynamicMultiplier = Mathf.Lerp(ModSettings.playerMovementSpeedDynamicMultiplier, ModSettings.playerMovementSpeedEndMultiplier, (ModSettings.playerMovementSpeedEndMultiplier - ModSettings.playerMovementSpeedDynamicMultiplier) / 2);
                                ModSettings.playerMovementSpeedMultiplier[i] = ModSettings.playerMovementSpeedDynamicMultiplier[i];
                            }
                            else if (ModSettings.staminaTimer[i] > 0f)
                            {
                                if (!References.PlayerClass.Motor.Moving)
                                {
                                    ModSettings.staminaTimer[i] -= ModSettings.playerStaminaModeStandingRecoveryFactor * Time.deltaTime;
                                }
                                else
                                {
                                    ModSettings.staminaTimer[i] -= ModSettings.playerStaminaModeWalkingRecoveryFactor * Time.deltaTime;
                                }
                                //ModSettings.playerMovementSpeedDynamicMultiplier = Mathf.SmoothDamp(ModSettings.playerMovementSpeedDynamicMultiplier, ModSettings.playerMovementSpeedStartMultiplier, ref staminaSpeed, 3f);
                                //ModSettings.playerMovementSpeedDynamicMultiplier = Mathf.Lerp(ModSettings.playerMovementSpeedDynamicMultiplier, ModSettings.playerMovementSpeedStartMultiplier, (ModSettings.playerMovementSpeedStartMultiplier - ModSettings.playerMovementSpeedDynamicMultiplier) / 2);
                                ModSettings.playerMovementSpeedMultiplier[i] = ModSettings.playerMovementSpeedStartMultiplier;
                            }
                            else
                            {
                                ModSettings.staminaTimer[i] = 0f;
                                ModSettings.playerMovementSpeedDynamicMultiplier[i] = ModSettings.playerMovementSpeedStartMultiplier;
                                ModSettings.playerMovementSpeedMultiplier[i] = ModSettings.playerMovementSpeedStartMultiplier;
                            }

                            /*
                            if (References.PlayerClass.IsRunning())
                            {
                                if (!startedRunning)
                                {
                                    ModSettings.playerMovementSpeedDynamicMultiplier -= 0.001f * ModSettings.playerMovementSpeedStartMultiplier;
                                    startedRunning = true;
                                }
                                else
                                {
                                    //ModSettings.playerMovementSpeedDynamicMultiplier = Mathf.MoveTowards(ModSettings.playerMovementSpeedDynamicMultiplier, ModSettings.playerMovementSpeedEndMultiplier, (ModSettings.playerMovementSpeedStartMultiplier - ModSettings.playerMovementSpeedDynamicMultiplier) / ModSettings.playerMovementSpeedEndMultiplier);
                                    ModSettings.playerMovementSpeedDynamicMultiplier -= (Time.deltaTime) * (ModSettings.playerMovementSpeedStartMultiplier - ModSettings.playerMovementSpeedDynamicMultiplier) / ModSettings.playerMovementSpeedEndMultiplier;
                                    if (ModSettings.playerMovementSpeedDynamicMultiplier < ModSettings.playerMovementSpeedEndMultiplier)
                                    {
                                        ModSettings.playerMovementSpeedDynamicMultiplier = ModSettings.playerMovementSpeedEndMultiplier;
                                    }
                                }
                                ModSettings.playerMovementSpeedMultiplier = ModSettings.playerMovementSpeedDynamicMultiplier;
                            }
                            else
                            {
                                //ModSettings.playerMovementSpeedDynamicMultiplier = Mathf.MoveTowards(ModSettings.playerMovementSpeedDynamicMultiplier, ModSettings.playerMovementSpeedStartMultiplier, (ModSettings.playerMovementSpeedStartMultiplier - ModSettings.playerMovementSpeedDynamicMultiplier) / ModSettings.playerMovementSpeedEndMultiplier);
                                ModSettings.playerMovementSpeedDynamicMultiplier += (Time.deltaTime) * (ModSettings.playerMovementSpeedStartMultiplier - ModSettings.playerMovementSpeedDynamicMultiplier) / ModSettings.playerMovementSpeedEndMultiplier;
                                if (ModSettings.playerMovementSpeedDynamicMultiplier > ModSettings.playerMovementSpeedStartMultiplier)
                                {
                                    ModSettings.playerMovementSpeedDynamicMultiplier = ModSettings.playerMovementSpeedStartMultiplier;
                                }
                                ModSettings.playerMovementSpeedMultiplier = ModSettings.playerMovementSpeedStartMultiplier;
                                startedRunning = false;
                            }
                            */

                            /*
                            ModSettings.playerMovementSpeedDynamicMultiplier = ModSettings.playerMovementSpeedEndMultiplier + (ModSettings.playerMovementSpeedStartMultiplier - ModSettings.playerMovementSpeedEndMultiplier) * (1 / (ModSettings.playerMovementSpeedEndMultiplier + (ModSettings.playerMovementSpeedStartMultiplier - ModSettings.playerMovementSpeedEndMultiplier) * Mathf.Exp(ModSettings.playerStaminaModeStaminaDecayRate * ModSettings.staminaTimer)));

                            if (References.PlayerClass.IsRunning())
                            {
                                ModSettings.staminaTimer += Time.deltaTime;
                                //ModSettings.playerMovementSpeedDynamicMultiplier = Mathf.SmoothDamp(ModSettings.playerMovementSpeedDynamicMultiplier, ModSettings.playerMovementSpeedEndMultiplier, ref staminaSpeed, 5f);
                                //ModSettings.playerMovementSpeedDynamicMultiplier = Mathf.Lerp(ModSettings.playerMovementSpeedDynamicMultiplier, ModSettings.playerMovementSpeedEndMultiplier, (ModSettings.playerMovementSpeedEndMultiplier - ModSettings.playerMovementSpeedDynamicMultiplier) / 2);
                                ModSettings.playerMovementSpeedMultiplier = ModSettings.playerMovementSpeedDynamicMultiplier;
                            }
                            else if (ModSettings.staminaTimer > 0f)
                            {
                                if (!References.PlayerClass.Motor.Moving)
                                {
                                    ModSettings.staminaTimer -= 3 * Time.deltaTime;
                                }
                                else
                                {
                                    ModSettings.staminaTimer -= Time.deltaTime;
                                }
                                //ModSettings.playerMovementSpeedDynamicMultiplier = Mathf.SmoothDamp(ModSettings.playerMovementSpeedDynamicMultiplier, ModSettings.playerMovementSpeedStartMultiplier, ref staminaSpeed, 3f);
                                //ModSettings.playerMovementSpeedDynamicMultiplier = Mathf.Lerp(ModSettings.playerMovementSpeedDynamicMultiplier, ModSettings.playerMovementSpeedStartMultiplier, (ModSettings.playerMovementSpeedStartMultiplier - ModSettings.playerMovementSpeedDynamicMultiplier) / 2);
                                ModSettings.playerMovementSpeedMultiplier = ModSettings.playerMovementSpeedStartMultiplier;
                            }
                            else
                            {
                                ModSettings.staminaTimer = 0f;
                                ModSettings.playerMovementSpeedMultiplier = ModSettings.playerMovementSpeedStartMultiplier;
                            }
                            */
                        }

                        if (ModSettings.playerStaminaModeStaminaText)
                        {
                            ModSettings.ShowTextOnScreen("Current dynamic speed multiplier: " + ModSettings.playerMovementSpeedDynamicMultiplier[0]);
                        }
                    }

                    if (ModSettings.enableMultiplayer)
                    {
                        //MultiplayerMode.MultiplayerModeActiveFeatures();

                        if (ModSettings.enableCrewVSMonsterMode)
                        {
                            CrewVsMonsterMode.CrewVSMonsterModeActiveFeatures();
                        }
                    }

                    if (ModSettings.useSmokeMonster)
                    {
                        SmokeMonster.SmokeMonsterActiveFeatures();
                    }
                }
            }

            private static IEnumerator ReadyLiferaft(bool startedViaDebug = false)
            {
                // The code has been adapted to handle multiple liferafts as needed in the case of when the Add Additional Crew Deck Building setting is enabled.
                if (startedViaDebug)
                {
                    ModSettings.ShowTextOnScreen("Started Liferaft Escape Sequence");
                }

                CraneSpoolBox[] craneSpoolBoxes = FindObjectsOfType<CraneSpoolBox>();
                Spool[] spools = FindObjectsOfType<Spool>();
                List<CraneSpoolBox> emptyCraneSpoolBoxes = new List<CraneSpoolBox>();
                List<Spool> usableSpools = new List<Spool>();
                for (int i = 0; i < craneSpoolBoxes.Length; i++)
                {
                    if (craneSpoolBoxes[i].spool == null)
                    {
                        emptyCraneSpoolBoxes.Add(craneSpoolBoxes[i]);
                    }
                }
                for (int i = 0; i < spools.Length; i++)
                {
                    if (spools[i].spoolBox == null)
                    {
                        usableSpools.Add(spools[i]);
                    }
                }
                for (int i = 0; i < Math.Min(emptyCraneSpoolBoxes.Count, usableSpools.Count); i++)
                {
                    usableSpools[i].AddToSpoolBox(emptyCraneSpoolBoxes[i]);
                }

                bool fixingLiferafts = true;
                Liferaft[] liferafts = FindObjectsOfType<Liferaft>();
                Crane[] cranes = FindObjectsOfType<Crane>();
                LiferaftHook[] liferaftHooks = FindObjectsOfType<LiferaftHook>();
                bool[] liferaftsFixedBools = new bool[liferafts.Length];
                while (fixingLiferafts)
                {
                    for (int i = 0; i < liferafts.Length && i < cranes.Length; i++)
                    {
                        if (liferafts[i].state != Liferaft.LifeRaftState.Inflated)
                        {
                            switch (liferafts[i].state)
                            {
                                case Liferaft.LifeRaftState.OffEdge:
                                    liferafts[i].OnStartFixedAnimation();
                                    break;

                                case Liferaft.LifeRaftState.Dragging:
                                    liferafts[i].AdvanceState();
                                    break;

                                case Liferaft.LifeRaftState.Torn:
                                    liferafts[i].OnFinishFixedAnimation();
                                    break;

                                default:
                                    liferafts[i].OnStartLoopAnimation();
                                    break;
                            }
                        }

                        if (!cranes[i].chain.hook.IsConnected)
                        {
                            MoveCrane(cranes[i], "Left");
                            MoveCrane(cranes[i], "Down");
                            if (liferafts[i].state == Liferaft.LifeRaftState.Inflated && cranes[i].angle == cranes[i].minAngle && cranes[i].chain.IsFullyExtended)
                            {
                                cranes[i].chain.hook.transform.position = liferaftHooks[i].transform.position;
                                cranes[i].chain.hook.hook = liferaftHooks[i];
                            }
                        }
                        else
                        {
                            MoveCrane(cranes[i], "Right");
                            if (cranes[i].angle == cranes[i].maxAngle)
                            {
                                MoveCrane(cranes[i], "Down");
                                if (cranes[i].chain.IsFullyExtended)
                                {
                                    liferaftsFixedBools[i] = true;
                                }
                            }
                        }
                    }

                    fixingLiferafts = false;
                    foreach (bool isLiferaftFixed in liferaftsFixedBools)
                    {
                        if (isLiferaftFixed == false)
                        {
                            fixingLiferafts = true;
                            break;
                        }
                    }

                    yield return null;
                }
                yield break;
            }

            private static InventoryItem nearbyInventoryItem;
            private static int nearbyInventoryItemDeck;

            private static IEnumerator ConnectHelicopterHoseWhenReady(FuelConnection fuelConnection, FuelPipeEnd fuelPipeEnd, FuelPumpLever fuelPumpLever)
            {
                yield return new WaitForSeconds(2.5f);
                fuelPipeEnd.transform.position = fuelConnection.transform.position;
                fuelPumpLever.OnHandGrab();
                yield break;
            }

            private static void MoveCrane(Crane crane, String direction)
            {
                switch (direction)
                {
                    case "Left":
                        if (crane.angle == crane.minAngle && crane.rotationDirection != 0f)
                        {
                            crane.OnRotationStop();
                        }
                        else if (crane.angle == crane.maxAngle && crane.rotationDirection == 0f)
                        {
                            crane.OnRotateLeft();
                        }
                        return;

                    case "Right":
                        if (crane.angle == crane.maxAngle && crane.rotationDirection != 0f)
                        {
                            crane.OnRotationStop();
                        }
                        else if (crane.angle == crane.minAngle && crane.rotationDirection == 0f)
                        {
                            crane.OnRotateRight();
                        }
                        return;

                    case "Down":
                        if (crane.chain.IsFullyExtended && crane.chainDirection != 0f)
                        {
                            crane.OnChainStop();
                        }
                        else if (!crane.chain.IsFullyExtended && crane.chainDirection == 0f)
                        {
                            crane.OnChainDown();
                        }
                        return;

                    case "Up":
                        if (crane.chain.MinChainLengthMet && crane.chainDirection != 0f)
                        {
                            crane.OnChainStop();
                        }
                        else if (!crane.chain.MinChainLengthMet && crane.chainDirection == 0f)
                        {
                            crane.OnChainUp();
                        }
                        return;
                }
            }

            // Copy of KeyControl's GetKeyPref
            private static KeyCode GetKeyPrefCopy(string _savedKey, KeyCode _defaultKey)
            {
                if (PlayerPrefs.HasKey(_savedKey))
                {
                    return (KeyCode)PlayerPrefs.GetInt(_savedKey, 0);
                }
                return _defaultKey;
            }

            private static void HookNoClipFixes()
            {
                On.AmbienceRaycasting.IsOutside += new On.AmbienceRaycasting.hook_IsOutside(HookAmbienceRaycastingNoClipFix);
                On.Monster.BothInEngineRoom += new On.Monster.hook_BothInEngineRoom(HookMonsterNoClipFix);
                //On.DetectRoom.PlayerHidingSpot += new On.DetectRoom.hook_PlayerHidingSpot(HookDetectRoomNoClipFix); // While this gets rid of the null reference exception when the player is out of bounds, it also affects normal gameplay when the player is in bounds.
            }

            private static bool HookAmbienceRaycastingNoClipFix(On.AmbienceRaycasting.orig_IsOutside orig, AmbienceRaycasting ambienceRaycasting)
            {
                if (ModSettings.noclip)
                {
                    return true;
                }
                else
                {
                    return orig.Invoke(ambienceRaycasting);
                }
            }

            private static bool HookMonsterNoClipFix(On.Monster.orig_BothInEngineRoom orig, Monster monster)
            {
                if (ModSettings.noclip)
                {
                    return false;
                }
                else
                {
                    return orig.Invoke(monster);
                }
            }

            private static HidingSpot HookDetectRoomNoClipFix(On.DetectRoom.orig_PlayerHidingSpot orig, DetectRoom detectRoom, HidingSpot[] _spots)
            {
                if (ModSettings.noclip)
                {
                    return null;
                }
                return orig.Invoke(detectRoom, _spots);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @AnimationControl

            private static void HookAnimationControlCheckIfStunned(On.AnimationControl.orig_CheckIfStunned orig, AnimationControl animationControl)
            {
                orig.Invoke(animationControl);
                if (ModSettings.noMonsterStunImmunity || (ModSettings.enableCrewVSMonsterMode && !CrewVsMonsterMode.letAIControlMonster && ManyMonstersMode.MonsterNumber(animationControl.monster.GetInstanceID()) < ModSettings.numbersOfMonsterPlayers.Count))
                {
                    if (animationControl.immuneTime > 1.5f)
                    {
                        animationControl.immuneToStun = false;
                    }
                }
            }

            private static void HookAnimationControlStart(On.AnimationControl.orig_Start orig, AnimationControl animationControl)
            {
                orig.Invoke(animationControl);
                animationControl.monsterAnimation.speed = ModSettings.monsterAnimationSpeedMultiplier;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @CameraFOV

            private static void HookCameraFOV(On.CameraFOV.orig_Awake orig, CameraFOV cameraFOV)
            {
                if (PlayerPrefs.HasKey("FOV"))
                {
                    cameraFOV.fovFloat = 50f + ((PlayerPrefs.GetFloat("FOV") - ModSettings.minimumValueOnFOVSlider) * 20f / (ModSettings.maximumValueOnFOVSlider - ModSettings.minimumValueOnFOVSlider));
                    if (cameraFOV.fovFloat < ModSettings.minimumValueOnFOVSlider)
                    {
                        cameraFOV.fovFloat = 50f;//ModSettings.minimumValueOnFOVSlider;
                    }
                    else if (cameraFOV.fovFloat > ModSettings.maximumValueOnFOVSlider)
                    {
                        cameraFOV.fovFloat = 70f;//ModSettings.maximumValueOnFOVSlider;
                    }
                    for (int i = 0; i < cameraFOV.cameraList.Count; i++)
                    {
                        cameraFOV.cameraList[i].fieldOfView = cameraFOV.fovFloat;
                    }
                }
                else
                {
                    for (int j = 0; j < cameraFOV.cameraList.Count; j++)
                    {
                        cameraFOV.cameraList[j].fieldOfView = 60f;
                    }
                }

            }

            /*----------------------------------------------------------------------------------------------------*/
            // @ChooseAttack

            private static void HookChooseAttackWhatDeathByPlayer(On.ChooseAttack.orig_WhatDeathByPlayer orig, ChooseAttack.PlayerDeath PD)
            {
                Room playerRoom = References.Monster.GetComponent<Monster>().PlayerDetectRoom.GetRoom;
                if (playerRoom != null)
                {
                    deathRegion = playerRoom.PrimaryRegion;
                }
                deathType = PD;
                orig.Invoke(PD);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @CompassScript

            private static void HookCompassScriptUpdate(On.CompassScript.orig_Update orig, CompassScript compassScript)
            {
                if (!ModSettings.monsterCompass || !MonsterStarter.spawned)
                {
                    orig.Invoke(compassScript);
                }
                else
                {
                    if (compassScript.equip)
                    {
                        Transform monsterTransform;
                        if (!ModSettings.startedWithMMM)
                        {
                            monsterTransform = References.Monster.transform;
                        }
                        else
                        {
                            monsterTransform = ManyMonstersMode.monsterList[ManyMonstersMode.ClosestMonsterToThis(compassScript.needleTrans.position)].transform;
                        }

                        Transform playerTransform;
                        if (!ModSettings.enableMultiplayer)
                        {
                            playerTransform = References.Player.transform;
                        }
                        else
                        {
                            playerTransform = MultiplayerMode.InventoryFromItemClass(compassScript.gameObject).newPlayerClass.transform;
                        }

                        Quaternion rotation = compassScript.baseTrans.rotation;
                        compassScript.baseTrans.rotation = Quaternion.identity;
                        Vector3 distanceVectorToMonster = monsterTransform.position - playerTransform.position;
                        distanceVectorToMonster.y = playerTransform.forward.y;
                        float angle = MultiplayerMode.SignedAngleBetween(playerTransform.forward, distanceVectorToMonster, Vector3.up);
                        //Debug.Log("Angle between compass and monster is " + angle);

                        compassScript.needleTrans.rotation = Quaternion.Euler(0, angle, 0);
                        //compassScript.needleTrans.rotation = compassScript.rotate90AroundUp * Quaternion.LookRotation(Vector3.forward, Vector3.up);

                        //compassScript.baseTrans.rotation = Quaternion.AngleAxis(0f /*Vector3.Angle(Vector3.up, compassScript.baseTrans.up)*/, Vector3.up) * Quaternion.Euler(0, 90f, 0); //* compassScript.baseTrans.rotation;// * Quaternion.Euler(0, Vector3.Angle(compassScript.baseTrans.up, monsterTransform.up), 0);
                        //compassScript.needleTrans.rotation = compassScript.rotate90AroundUp * Quaternion.LookRotation(Vector3.forward, Vector3.up);
                        //compassScript.needleTrans.rotation *= Quaternion.Euler(0, Quaternion.Angle(compassScript.needleTrans.rotation, monsterTransform.rotation), 0);
                        //compassScript.needleTrans.rotation = compassScript.rotate90AroundUp * Quaternion.LookRotation(/*Vector3.forward*/monsterTransform.position, Vector3.up);
                        //compassScript.needleTrans.rotation = new Quaternion(compassScript.needleTrans.rotation.x, compassScript.needleTrans.rotation.y * Quaternion.Angle(Quaternion.LookRotation(Vector3.forward, Vector3.up), monsterTransform.rotation), compassScript.needleTrans.rotation.z, compassScript.needleTrans.rotation.w);

                        compassScript.baseTrans.rotation = rotation;
                        //compassScript.baseTrans.rotation = Quaternion.identity;

                        /*
                        Transform monsterTransform;
                        if (!ModSettings.startedWithMMM)
                        {
                            monsterTransform = References.Monster.transform;
                        }
                        else
                        {
                            monsterTransform = ManyMonstersMode.monsterList[ManyMonstersMode.ClosestMonsterToThis(compassScript.needleTrans.position)].transform;
                        }
                        Quaternion rotation = compassScript.baseTrans.rotation;
                        //Vector3 axis = Vector3.Cross(compassScript.baseTrans.up, Vector3.up);
                        compassScript.baseTrans.rotation = Quaternion.AngleAxis(0f, Vector3.up);////Quaternion.Euler(0, Quaternion.Angle(compassScript.baseTrans.rotation, monsterTransform.rotation), 0);

                        Vector3 distanceVectorToMonster = monsterTransform.position - References.Player.transform.position;
                        float angle = MultiplayerMode.SignedAngleBetween(References.Player.transform.forward, distanceVectorToMonster, Vector3.up);
                        Debug.Log("Angle between compass and monster is " + angle);
                        //compassScript.baseTrans.rotation *= Quaternion.Euler(0, angle, 0);
                        compassScript.baseTrans.rotation *= Quaternion.Euler(0, 0, 0);

                        compassScript.needleTrans.rotation = Quaternion.Euler(0, angle, 0);
                        //compassScript.needleTrans.rotation = compassScript.rotate90AroundUp * Quaternion.LookRotation(Vector3.forward, Vector3.up);
                        */

                        //compassScript.baseTrans.rotation = Quaternion.AngleAxis(0f /*Vector3.Angle(Vector3.up, compassScript.baseTrans.up)*/, Vector3.up) * Quaternion.Euler(0, 90f, 0); //* compassScript.baseTrans.rotation;// * Quaternion.Euler(0, Vector3.Angle(compassScript.baseTrans.up, monsterTransform.up), 0);
                        //compassScript.needleTrans.rotation = compassScript.rotate90AroundUp * Quaternion.LookRotation(Vector3.forward, Vector3.up);
                        //compassScript.needleTrans.rotation *= Quaternion.Euler(0, Quaternion.Angle(compassScript.needleTrans.rotation, monsterTransform.rotation), 0);
                        //compassScript.needleTrans.rotation = compassScript.rotate90AroundUp * Quaternion.LookRotation(/*Vector3.forward*/monsterTransform.position, Vector3.up);
                        //compassScript.needleTrans.rotation = new Quaternion(compassScript.needleTrans.rotation.x, compassScript.needleTrans.rotation.y * Quaternion.Angle(Quaternion.LookRotation(Vector3.forward, Vector3.up), monsterTransform.rotation), compassScript.needleTrans.rotation.z, compassScript.needleTrans.rotation.w);

                        //compassScript.baseTrans.rotation = rotation;
                        //compassScript.baseTrans.rotation = Quaternion.identity;


                        /*
                        Transform monsterTransform;
                        if (!ModSettings.startedWithMMM)
                        {
                            monsterTransform = References.Monster.transform;
                        }
                        else
                        {
                            monsterTransform = ManyMonstersMode.monsterList[ManyMonstersMode.ClosestMonsterToThis(compassScript.needleTrans.position)].transform;
                        }
                        Quaternion rotation = compassScript.baseTrans.rotation;
                        Vector3 axis = Vector3.Cross(compassScript.baseTrans.up, Vector3.up);
                        compassScript.baseTrans.rotation = Quaternion.AngleAxis(Vector3.Angle(Vector3.up, compassScript.baseTrans.up), axis) * compassScript.baseTrans.rotation;
                        compassScript.needleTrans.rotation = compassScript.rotate90AroundUp; //* Quaternion.LookRotation(Vector3.forward, Vector3.up);
                        compassScript.needleTrans.rotation = new Quaternion(compassScript.needleTrans.rotation.x, compassScript.needleTrans.rotation.y * Quaternion.Angle(compassScript.rotate90AroundUp, monsterTransform.rotation), compassScript.needleTrans.rotation.z, compassScript.needleTrans.rotation.w);
                        compassScript.baseTrans.rotation = rotation;
                        */
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @CraneChain

            private static void HookCraneChainStart(On.CraneChain.orig_Start orig, CraneChain craneChain)
            {
                if (ModSettings.addAdditionalCrewDeckBuilding)
                {
                    //Debug.Log("Max length before is " + craneChain.maxLength + ".");
                    //craneChain.maxLength = 7 * (RegionManager.Instance.ConvertPointToRegionNode(craneChain.transform.position).y - 1) / 7;
                    //craneChain.maxLength = RegionManager.Instance.ConvertPointToRegionNode(craneChain.transform.position).y - 1;
                    craneChain.maxLength = 7 * (RegionManager.Instance.ConvertPointToRegionNode(craneChain.transform.position).y - 1 - 5) / 2; // This gives the chain a maximum length as a ratio of deck difference of the corrected deck number to check and the original position times the original length. ORIGINAL: Deck 8 -> Deck 7 adjusted. Deck 7 to deck 5 is 2 decks difference. The original maximum chain length is 7 -> 7 * 2 / 2 = 7. DIFFERENT EXAMPLE: Deck 9 -> Deck 8 adjusted. Deck 8 to deck 5 is 3 decks differnce. The original maximum chain length is 7 -> 7 * 3 / 2 = 10.5.
                    //craneChain.maxLength = 9;
                    //Debug.Log("Using max length of " + craneChain.maxLength + " for crane chain as it is at " + RegionManager.Instance.ConvertPointToRegionNode(craneChain.transform.position) + ".");
                }
                orig.Invoke(craneChain);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DeathMenu

            private static Dictionary<PrimaryRegionType, List<Sprite>> regionBackgrounds; // Death Menu Region Backgrounds.
            private static Dictionary<String, List<Sprite>> monsterFrames; // Death Menu Monster Frames.
            private static Dictionary<ChooseAttack.PlayerDeath, List<Sprite>> deathTypeFrames; // Death Menu Death Type Frames.

            private static PrimaryRegionType deathRegion; // The region the player died in.
            private static string deathMonster = string.Empty; // The monster the player died to.
            private static ChooseAttack.PlayerDeath deathType; // The death type the player had (non-direct to monster).

            private static void LoadDeathScreens()
            {
                regionBackgrounds = new Dictionary<PrimaryRegionType, List<Sprite>>();
                monsterFrames = new Dictionary<string, List<Sprite>>();
                deathTypeFrames = new Dictionary<ChooseAttack.PlayerDeath, List<Sprite>>();

                UnityEngine.Object[] deathScreensUnpacked = Utilities.LoadAssetBundle("deathscreens");

                Sprite referenceSprite = Hints.GetRandomHint().texture;
                foreach (UnityEngine.Object deathScreenObject in deathScreensUnpacked)
                {
                    if (deathScreenObject.GetType() == typeof(Texture2D))
                    {
                        Texture2D deathScreenTexture = (Texture2D)deathScreenObject;
                        Sprite loadingScreenSprite = Sprite.Create(deathScreenTexture, referenceSprite.rect, referenceSprite.pivot, referenceSprite.pixelsPerUnit);

                        bool added = false;
                        // Check whether the image is a region background.
                        foreach (PrimaryRegionType primaryRegionType in Enum.GetValues(typeof(PrimaryRegionType)))
                        {
                            // Check the name of the image against the region enum.
                            if (deathScreenTexture.name.Contains(primaryRegionType.ToString()))
                            {
                                // Create a list of sprites if there is none for the key.
                                if (!regionBackgrounds.ContainsKey(primaryRegionType))
                                {
                                    regionBackgrounds.Add(primaryRegionType, new List<Sprite>());
                                }
                                // Add the image to the list of sprites for the key.
                                regionBackgrounds[primaryRegionType].Add(loadingScreenSprite);
                                added = true;
                                break;
                            }
                        }
                        if (!added)
                        {
                            // Check whether the image is a monster frame.
                            foreach (string monsterName in ModSettings.monsterNames)
                            {
                                // Check the name of the image against the monster names.
                                if (deathScreenTexture.name.Substring(0, deathScreenTexture.name.Length - 1).Equals(monsterName)) // Assumes there is a one-digit number at the end!
                                {
                                    // Create a list of sprites if there is none for the key.
                                    if (!monsterFrames.ContainsKey(monsterName))
                                    {
                                        monsterFrames.Add(monsterName, new List<Sprite>());
                                    }
                                    // Add the image to the list of sprites for the key.
                                    monsterFrames[monsterName].Add(loadingScreenSprite);
                                    added = true;
                                    break;
                                }
                            }
                            if (!added)
                            {
                                // Check whether the image is a death type frame.
                                foreach (ChooseAttack.PlayerDeath playerDeath in Enum.GetValues(typeof(ChooseAttack.PlayerDeath)))
                                {
                                    // Check the name of the image against the playerDeath enum.
                                    if (deathScreenTexture.name.Contains(playerDeath.ToString()))
                                    {
                                        // Create a list of sprites if there is none for the key.
                                        if (!deathTypeFrames.ContainsKey(playerDeath))
                                        {
                                            deathTypeFrames.Add(playerDeath, new List<Sprite>());
                                        }
                                        // Add the image to the list of sprites for the key.
                                        deathTypeFrames[playerDeath].Add(loadingScreenSprite);
                                        added = true;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                }
                // Copy Fiend death screens to Mind Attack death screens.
                deathTypeFrames.Add(ChooseAttack.PlayerDeath.MindAttack, monsterFrames[Monster.MonsterTypeEnum.Fiend.ToString()]);
            }

            private static void HookDeathMenuStart(On.DeathMenu.orig_Start orig, DeathMenu deathMenu)
            {
                // Load custom death screens if required.
                if (regionBackgrounds == null)
                {
                    LoadDeathScreens();
                }

                // Unhide the image.
                deathMenu.backgroundImage.color = Color.white;

                // Set a custom frame.
                // Duplicate elements inside the canvas - Hosnkobf - https://forum.unity.com/threads/duplicate-elements-inside-the-canvas.612415/ - Accessed 29.04.2023
                Image frame = Instantiate(deathMenu.backgroundImage, deathMenu.backgroundImage.rectTransform);
                frame.preserveAspect = true;

                if (deathMonster.Equals("Sparky") && !ModSettings.customSparkyModel)
                {
                    deathMonster = "SparkyBrute";
                }

                if (ModSettings.monsterNames.Contains(deathMonster) && monsterFrames.ContainsKey(deathMonster) && monsterFrames[deathMonster].Count > 0)
                {
                    Debug.Log("Setting monster frame.");
                    frame.sprite = monsterFrames[deathMonster][UnityEngine.Random.Range(0, monsterFrames[deathMonster].Count)];
                }
                else if (deathTypeFrames.ContainsKey(deathType) && deathTypeFrames[deathType].Count > 0)
                {
                    Debug.Log("Setting special death frame.");
                    frame.sprite = deathTypeFrames[deathType][UnityEngine.Random.Range(0, deathTypeFrames[deathType].Count)];
                }
                else
                {
                    Debug.Log("Disabling death frame.");
                    frame.enabled = false;
                }

                // Set a custom background. Set it after the frame so that a black background is not duplicated.
                deathMenu.backgrounds = new Sprite[1];
                DeathMenu.backgroundID = 0;

                if (regionBackgrounds.ContainsKey(deathRegion) && regionBackgrounds[deathRegion].Count > 0)
                {
                    Debug.Log("Setting death region background.");
                    deathMenu.backgrounds[0] = regionBackgrounds[deathRegion][UnityEngine.Random.Range(0, regionBackgrounds[deathRegion].Count)];
                }
                else
                {
                    Debug.Log("Disabling region background.");
                    deathMenu.backgroundImage.color = Color.black;
                }

                // Reset the values for the next round after chosen.
                // What is the default value for enum variable? - BoltClock - https://stackoverflow.com/questions/4967656/what-is-the-default-value-for-enum-variable - Accessed 30.04.2023
                Debug.Log("Death Region: " + deathRegion + "\nDeath Monster: " + deathMonster + "\nDeath Type: " + deathType);
                deathRegion = default(PrimaryRegionType);
                deathMonster = string.Empty;
                deathType = default(ChooseAttack.PlayerDeath); // Default is Steam. MindAttack is also used for specific Fiend frames.

                // Change the render mode of the canvas to overlay the screen.
                deathMenu.backgroundImage.canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                // Resize the image to fit the screen. Requires an unusual resolution for some reason.
                deathMenu.backgroundImage.rectTransform.sizeDelta = new Vector2(1820, 1100);

                // Add an event to the exit button to hide the frame.
                deathMenu.exit.GetComponentInChildren<Button>().onClick.AddListener(delegate ()
                {
                    frame.enabled = false;
                });

                // Scale up the buttons.
                Button button = deathMenu.exit.GetComponentInChildren<Button>();
                for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                {
                    UnityEngine.Object ueObject = button.onClick.GetPersistentTarget(i);
                    if (ueObject.GetType() == typeof(GameObject))
                    {
                        foreach (Text text in ((GameObject)ueObject).GetComponentsInChildren<Text>())
                        {
                            text.transform.parent.parent.localScale = new Vector3(1.65f, 1.65f, 1.65f);
                        }
                    }
                    // Show the frame when hiding the exit confirmation.
                    if (ueObject.name.Equals("NoButton") && frame.enabled)
                    {
                        Button noButton = ((Button)ueObject);
                        noButton.onClick.AddListener(delegate ()
                        {
                            frame.enabled = true;
                        });
                    }
                }

                // Reset the scale of the canvas.
                deathMenu.backgroundImage.canvas.transform.localScale = Vector3.one;

                // Reset the position of the image.
                deathMenu.backgroundImage.transform.localPosition = Vector3.zero;

                // Reset the scale of the image and its parents.
                deathMenu.backgroundImage.rectTransform.localScale = Vector3.one;
                deathMenu.backgroundImage.rectTransform.parent.localScale = Vector3.one;
                deathMenu.backgroundImage.rectTransform.parent.parent.localScale = Vector3.one;

                // Shift the image to the right.
                deathMenu.backgroundImage.rectTransform.localPosition = new Vector3(50f, 0f, 0f);

                // Put the buttons at the right height.
                deathMenu.backgroundImage.rectTransform.parent.parent.localPosition = new Vector3(deathMenu.backgroundImage.rectTransform.parent.parent.localPosition.x, 0f, deathMenu.backgroundImage.rectTransform.parent.parent.localPosition.z);

                // Scale the canvas to the right size (same mode as loading screen).
                CanvasScaler canvasScaler = deathMenu.backgroundImage.canvas.GetComponent<CanvasScaler>();
                canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasScaler.matchWidthOrHeight = 1;
                canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
                canvasScaler.referenceResolution = new Vector2(1920, 1080);

                /*
                Debug.Log("Image size:" + deathMenu.backgroundImage.rectTransform.sizeDelta);
                Debug.Log("anchorMin: " + deathMenu.backgroundImage.rectTransform.anchorMin);
                Debug.Log("anchorMax: " + deathMenu.backgroundImage.rectTransform.anchorMax);
                Debug.Log("pivot: " + deathMenu.backgroundImage.rectTransform.pivot);
                Debug.Log("Preserve aspect: " + deathMenu.backgroundImage.preserveAspect);
                Debug.Log("anchoredPosition: " + deathMenu.backgroundImage.rectTransform.anchoredPosition);
                Debug.Log("anchoredPosition3D: " + deathMenu.backgroundImage.rectTransform.anchoredPosition3D);
                Debug.Log("canvasScaler: " + canvasScaler);
                Debug.Log("canvasScaler scaleFactor:" + canvasScaler.scaleFactor);
                Debug.Log("canvasScaler uiScaleMode:" + canvasScaler.uiScaleMode);
                Debug.Log("canvasScaler matchWidthOrHeight:" + canvasScaler.matchWidthOrHeight);
                Debug.Log("canvasScaler screenMatchMode:" + canvasScaler.screenMatchMode);
                Debug.Log("canvasScaler referenceResolution:" + canvasScaler.referenceResolution);
                Debug.Log("canvasScaler referencePixelsPerUnit:" + canvasScaler.referencePixelsPerUnit);
                */

                orig.Invoke(deathMenu);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @DeathScreen

            /*----------------------------------------------------------------------------------------------------*/
            // @DuctTape

            private static void HookDuctTapeOnFinishItemAnimation(On.DuctTape.orig_OnFinishItemAnimation orig, DuctTape ductTape)
            {
                if (ModSettings.addAdditionalCrewDeckBuilding)
                {
                    foreach (Liferaft liferaft in ModSettings.liferafts)
                    {
                        DuctTape.lifeRaft = liferaft;
                        orig.Invoke(ductTape);
                    }
                }
                else
                {
                    orig.Invoke(ductTape);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @EscapeRoute

            private static void HookEscapeRoute(On.EscapeRoute.orig_OnInteract orig, EscapeRoute escapeRoute)
            {
                // Record the final time when an escape attempt is made to stop lag from the checks affecting the time.
                float finalTime = float.MaxValue;
                if (ModSettings.useSpeedrunTimer)
                {
                    finalTime = ModSettings.speedrunTimer.TimeElapsed;
                }

                // Check whether the escape route type is allowed by the mod settings.
                if (EscapeRouteTypeAllowed(escapeRoute.escapeRouteType))
                {
                    // Check whether the number of routes completed matches the required count by the mod settings if this is set to a non-standard number.
                    if (ModSettings.escapeRoutesToWin > 1)
                    {
                        int routesCompleted = 0;

                        EscapeLifeRaft[] escapeLifeRafts = FindObjectsOfType<EscapeLifeRaft>();
                        if (escapeLifeRafts != null)
                        {
                            bool canEscape = false;
                            if (ModSettings.escapeRoutesToWin == 3)
                            {
                                // If doing 100%, ensure all life rafts have been readied.
                                canEscape = true;
                                foreach (EscapeLifeRaft escapeLifeRaft in escapeLifeRafts)
                                {
                                    if (!escapeLifeRaft.canEscape)
                                    {
                                        canEscape = false;
                                    }
                                }
                            }
                            else
                            {
                                // If not doing 100%, just check whether one life raft has been readied.
                                foreach (EscapeLifeRaft escapeLifeRaft in escapeLifeRafts)
                                {
                                    if (escapeLifeRaft.canEscape)
                                    {
                                        canEscape = true;
                                        break;
                                    }
                                }
                            }
                            if (canEscape)
                            {
                                routesCompleted++;
                            }
                        }

                        HelicopterVictory helicopterVictory = FindObjectOfType<HelicopterVictory>();
                        if (helicopterVictory != null && helicopterVictory.defendTask.Completed && helicopterVictory.lockTask.Completed && helicopterVictory.cableTask.Completed)
                        {
                            routesCompleted++;
                        }

                        Submarine submarine = FindObjectOfType<Submarine>();
                        if (submarine != null && submarine.subReleased)
                        {
                            routesCompleted++;
                        }

                        if (routesCompleted < ModSettings.escapeRoutesToWin)
                        {
                            return;
                        }
                    }

                    // Let the player escape. This only occurs when the route checks have passed.
                    if (ModSettings.useSpeedrunTimer)
                    {
                        ModSettings.speedrunTimer.StopTimer();
                        ModSettings.finalTime = Mathf.FloorToInt(finalTime / 60f).ToString() + ":" + (finalTime % 60f).ToString("00.000000");
                        Debug.Log("Speedrun timer when exiting ship: " + ModSettings.finalTime + ". This occurred at " + DateTime.Now + " local / " + DateTime.UtcNow + " UTC");
                        if (ModSettings.currentChallenge != null)
                        {
                            ChallengeParser.UpdateChallengeTime(ModSettings.currentChallenge, TimeSpan.FromSeconds((double)finalTime));
                        }
                    }
                    if (ModSettings.debugMode)
                    {
                        ModSettings.ShowDebugModeText();
                    }
                    if (ModSettings.numberOfMonsters > 1)
                    {
                        LevelGeneration.Instance.chosenMonstType = ManyMonstersMode.monsterListMonsterComponents[ManyMonstersMode.ClosestMonsterToPlayer()].MonsterType;
                    }
                    orig.Invoke(escapeRoute);
                }
            }

            private static bool EscapeRouteTypeAllowed(EscapeRoute.EscapeRouteType escapeRouteType)
            {
                switch (escapeRouteType)
                {
                    case EscapeRoute.EscapeRouteType.LifeRaft:
                        {
                            return !ModSettings.disableLiferaft || (ModSettings.glowstickHunt && ModSettings.glowstickHuntCounter >= ModSettings.specialGlowsticksRequired);
                        }
                    case EscapeRoute.EscapeRouteType.Heli:
                        {
                            return !ModSettings.disableHelicopter;
                        }
                    case EscapeRoute.EscapeRouteType.Submersible:
                        {
                            return !ModSettings.disableSubmersible;
                        }
                }
                return false;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FadeScript

            private static void HookFadeScriptBeginFadeIn(On.FadeScript.orig_BeginFadeIn orig)
            {
                if (ModSettings.useSpeedrunTimer)
                {
                    ModSettings.speedrunTimer.StartTimer();
                    Debug.Log("Started speedrun timer at " + DateTime.Now + " local / " + DateTime.UtcNow + " UTC");
                }
                orig.Invoke();
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FiendAura

            private static void HookFiendAura()
            {
                On.FiendAura.ctor += new On.FiendAura.hook_ctor(HookFiendAuraCtor);
                On.FiendAura.ResetRanges += new On.FiendAura.hook_ResetRanges(HookFiendAuraResetRanges);
                On.FiendAura.SetRangesTo += new On.FiendAura.hook_SetRangesTo(HookFiendAuraSetRangesTo);
                On.FiendAura.Start += new On.FiendAura.hook_Start(HookFiendAuraStart);
            }

            private static void HookFiendAuraCtor(On.FiendAura.orig_ctor orig, FiendAura fiendAura)
            {
                if (ModSettings.fiendFlickerMin != 0f && ModSettings.fiendFlickerMed != 0f && ModSettings.fiendFlickerMax != 0f)
                {
                    fiendAura.largeRadius = ModSettings.fiendFlickerMax;
                    fiendAura.mediumRadius = ModSettings.fiendFlickerMed;
                    fiendAura.smallRadius = ModSettings.fiendFlickerMin;
                }
            }

            private static void HookFiendAuraResetRanges(On.FiendAura.orig_ResetRanges orig, FiendAura fiendAura)
            {
                if (ModSettings.fiendFlickerMin != 0f && ModSettings.fiendFlickerMed != 0f && ModSettings.fiendFlickerMax != 0f)
                {
                    fiendAura.smallRadius = fiendAura.srt_smlRad;
                    fiendAura.mediumRadius = fiendAura.srt_medRad;
                    fiendAura.largeRadius = fiendAura.srt_lrgRad;
                }
            }

            private static void HookFiendAuraSetRangesTo(On.FiendAura.orig_SetRangesTo orig, FiendAura fiendAura, float smallRad, float medRad, float largeRad)
            {
                if (ModSettings.fiendFlickerMin != 0f && ModSettings.fiendFlickerMed != 0f && ModSettings.fiendFlickerMax != 0f)
                {
                    fiendAura.smallRadius = smallRad;
                    fiendAura.mediumRadius = medRad;
                    fiendAura.largeRadius = largeRad;
                }
            }

            private static void HookFiendAuraStart(On.FiendAura.orig_Start orig, FiendAura fiendAura)
            {
                if (ModSettings.fiendFlickerMin != 0f && ModSettings.fiendFlickerMed != 0f && ModSettings.fiendFlickerMax != 0f)
                {
                    fiendAura.largeRadius = ModSettings.fiendFlickerMax;
                    fiendAura.mediumRadius = ModSettings.fiendFlickerMed;
                    fiendAura.smallRadius = ModSettings.fiendFlickerMin;
                }
                fiendAura.srt_lrgRad = fiendAura.largeRadius;
                fiendAura.srt_medRad = fiendAura.mediumRadius;
                fiendAura.srt_smlRad = fiendAura.smallRadius;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FiendLightController

            // FiendLightController is a component of Flashlight.
            private static void HookFiendLightControllerLateUpdate(On.FiendLightController.orig_LateUpdate orig, FiendLightController fiendLightController)
            {
                if (ModSettings.giveAllMonstersAFiendAura)
                {
                    if (fiendLightController.aura != null)
                    {
                        if (fiendLightController.aura != null && fiendLightController.aura.enabled)
                        {
                            FiendLightDisruptor fiendLightDisruptor = fiendLightController.aura.GetComponent<FiendLightDisruptor>();
                            if (fiendLightDisruptor.timeSinceDisrupt == 0f)
                            {
                                if (MathHelper.RoundToNearest(fiendLightController.aura.transform.position, Settings.CuboidDim.y).y == MathHelper.RoundToNearest(((MonoBehaviour)fiendLightController).transform.position, Settings.CuboidDim.y).y)
                                {
                                    float num = Vector3.Distance(fiendLightController.aura.transform.position, ((MonoBehaviour)fiendLightController).transform.position);
                                    if (num < fiendLightController.aura.largeRadius * Settings.CuboidDim.x)
                                    {
                                        if (num < fiendLightController.aura.smallRadius * Settings.CuboidDim.x)
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
                        }
                    }
                    else
                    {
                        fiendLightController.aura = References.Monster.GetComponent<FiendAura>();
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
                else
                {
                    orig.Invoke(fiendLightController);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FiendMindAttack

            private static void HookFiendMindAttackUpdate(On.FiendMindAttack.orig_Update orig, FiendMindAttack fiendMindAttack)
            {
                int crewPlayerIndex = 0;
                if (ModSettings.enableMultiplayer)
                {
                    crewPlayerIndex = MultiplayerMode.crewPlayers.IndexOf(fiendMindAttack.monster.PlayerDetectRoom.player);
                }
                if (ModSettings.invincibilityMode[crewPlayerIndex])
                {
                    return;
                }
                else
                {
                    orig.Invoke(fiendMindAttack);
                }
            }

            private static void HookFiendMindAttackAttackCheck(On.FiendMindAttack.orig_AttackCheck orig, FiendMindAttack fiendMindAttack)
            {
                bool flag = false;
                if (Calculations.PlayerToMonsterDistance() < fiendMindAttack.attackRange && fiendMindAttack.monster.CanSeePlayer && fiendMindAttack.attackTimer == fiendMindAttack.maxTime && fiendMindAttack.delayTimer == fiendMindAttack.maxDelay)
                {
                    if (fiendMindAttack.fiendAnims != null)
                    {
                        flag = true;
                    }
                    else
                    {
                        fiendMindAttack.fiendAnims = fiendMindAttack.monster.GetComponentInChildren<FiendAnimations>();
                    }
                }
                if (flag)
                {
                    fiendMindAttack.fiendAnims.DoMindAttack = true;
                    fiendMindAttack.monster.MoveControl.GetAniControl.DesiredUpperBodyWeight = 1f;
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
                    fiendMindAttack.playerHealth.DoDamage(35f * ModSettings.fiendMindAttackDamageMultiplier, false, PlayerHealth.DamageTypes.MindAttack, false);
                }
            }

            private static void HookFiendMindAttackChangeTimers(On.FiendMindAttack.orig_ChangeTimers orig, FiendMindAttack fiendMindAttack)
            {
                bool flag = false;
                if (fiendMindAttack.stateMachine.Current.ToString().Contains("MChasingState") && fiendMindAttack.monster.CanSeePlayer)
                {
                    flag = true;
                }
                if (fiendMindAttack.delayTimer == fiendMindAttack.maxDelay)
                {
                    if (flag && fiendMindAttack.attackTimer < fiendMindAttack.maxTime)
                    {
                        fiendMindAttack.attackTimer += Time.deltaTime * ModSettings.fiendMindAttackAttackTimerChargeRate;
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
                        fiendMindAttack.attackTimer -= Time.deltaTime * ModSettings.fiendMindAttackAttackTimerDecayRate;
                        if (fiendMindAttack.currentSound != string.Empty)
                        {
                            float num = 0f;
                            if (fiendMindAttack.coolSource != null)
                            {
                                num = fiendMindAttack.chargeSource.clip.length - fiendMindAttack.chargeSource.time;
                            }
                            if (fiendMindAttack.PlaySound(fiendMindAttack.coolSound, fiendMindAttack.coolSource))
                            {
                                fiendMindAttack.coolSource.time = num;
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
                    fiendMindAttack.attackTimer -= Time.deltaTime * 3f * ModSettings.fiendMindAttackAttackTimerDecayRate;
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
                fiendMindAttack.delayTimer += Time.deltaTime * ModSettings.fiendMindAttackDelayTimerRate;
                fiendMindAttack.delayTimer = Mathf.Clamp(fiendMindAttack.delayTimer, 0f, fiendMindAttack.maxDelay);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Fire

            private static void HookFireDamageStart(On.FireDamage.orig_Start orig, FireDamage fireDamage)
            {
                orig.Invoke(fireDamage);
                fireDamage.fireDamageParent.GetComponentInChildren<DamageScript>().name = "FireDamage";
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FireExtinguisher

            private static void HookFireExtinguisherUpdate(On.FireExtinguisher.orig_Update orig, FireExtinguisher fireExtinguisher)
            {
                if (ModSettings.infiniteFireExtinguisherFuel && fireExtinguisher.IsUsing)
                {
                    fireExtinguisher.ammo = 3.99f;
                }
                orig.Invoke(fireExtinguisher);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FlareGun

            private static void HookFlareGunUpdate(On.FlareGun.orig_Update orig, FlareGun flareGun)
            {
                if (!ModSettings.overpoweredFlareGun)
                {
                    orig.Invoke(flareGun);
                }
                else
                {
                    flareGun.readyTofire = true;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FlareObject

            private static void HookFlareObjectHitMonster(On.FlareObject.orig_HitMonster orig, FlareObject flareObject)
            {
                if (flareObject.monster.MonsterType != Monster.MonsterTypeEnum.Brute || ModSettings.overpoweredFlareGun)
                {
                    flareObject.monster.MoveControl.GetAniControl.Stun(1f);
                    if (ModSettings.numberOfMonsters < 2)
                    {
                        flareObject.hasStunnedMonster = true;
                    }
                    if (ModSettings.flaresDisableMonsters)
                    {
                        flareObject.monster.gameObject.SetActive(false);
                    }
                    if (ModSettings.flaresTeleportMonsters)
                    {
                        for (int i = 0, maxAttempts = 5; i < maxAttempts; i++)
                        {
                            Vector3 spawnPosition = LevelGeneration.Instance.MonsterSpawnPoints[UnityEngine.Random.Range(0, LevelGeneration.Instance.MonsterSpawnPoints.Count)].transform.position;
                            Vector3 closestPlayerPosition;
                            if (ModSettings.enableMultiplayer)
                            {
                                closestPlayerPosition = MultiplayerMode.newPlayerClasses[MultiplayerMode.ClosestPlayerToThis(spawnPosition)].transform.position;
                            }
                            else
                            {
                                closestPlayerPosition = References.Player.transform.position;
                            }
                            if (Vector3.Distance(closestPlayerPosition, spawnPosition) > 16f || i == maxAttempts - 1)
                            {
                                flareObject.monster.gameObject.transform.position = spawnPosition;
                                ModSettings.ForceStopChase(flareObject.monster);
                                break;
                            }
                        }
                    }
                }
                else
                {
                    Achievements.Instance.CompleteAchievement("FLAREGUN_BRUTE");
                }
            }

            private static void HookFlareObjectStart(On.FlareObject.orig_Start orig, FlareObject flareObject)
            {
                orig.Invoke(flareObject);
                flareObject.MaxLifeTime = ModSettings.flareLifetime;
                if (ModSettings.overpoweredFlareGun)
                {
                    flareObject.gameObject.AddComponent<DestroyAfterTime>().Trigger(flareObject.MaxLifeTime);
                }
            }

            private class DestroyAfterTime : MonoBehaviour
            {
                public void Trigger(float time)
                {
                    this.StartCoroutine(DestructionTimer(time));
                }

                private IEnumerator DestructionTimer(float time)
                {
                    yield return new WaitForSeconds(time);
                    Destroy(this.gameObject);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Flashlight

            private static void HookFlashlight()
            {
                On.Flashlight.Update += new On.Flashlight.hook_Update(HookFlashlightUpdate);
            }

            private static void HookFlashlightStart(On.Flashlight.orig_Start orig, Flashlight flashlight)
            {
                orig.Invoke(flashlight);

                if (ModSettings.UseCustomColour(ModSettings.flashlightColour) || ModSettings.randomFlashlightColours)
                {
                    if (ModSettings.UseCustomColour(ModSettings.flashlightColour) && ModSettings.randomFlashlightColours)
                    {
                        float randomChance = UnityEngine.Random.value;
                        if (randomChance > 0.5f)
                        {
                            flashlight.flashLight.color = ModSettings.ConvertColourStringToColour(ModSettings.flashlightColour);
                        }
                        else
                        {
                            flashlight.flashLight.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0.5f, 1f));
                        }
                    }
                    else if (ModSettings.UseCustomColour(ModSettings.flashlightColour))
                    {
                        flashlight.flashLight.color = ModSettings.ConvertColourStringToColour(ModSettings.flashlightColour);
                    }
                    else
                    {
                        flashlight.flashLight.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0.5f, 1f));
                    }
                    flashlight.originalFlashLightColor = flashlight.flashLight.color;
                }
                flashlight.flashLightIntensity *= ModSettings.flashlightIntensityMultiplier;
                flashlight.maxFlashLightIntensity *= ModSettings.flashlightIntensityMultiplier;
                flashlight.flashLight.range *= ModSettings.flashlightRangeMultiplier;
            }

            private static void HookFlashlightUpdate(On.Flashlight.orig_Update orig, Flashlight flashlight)
            {
                if (ModSettings.infiniteFlashlightPower && flashlight.on)
                {
                    flashlight.power = 260f;
                }
                orig.Invoke(flashlight);
                if (ModSettings.enableMultiplayer && !MultiplayerMode.useLegacyAudio && !(flashlight.on && flashlight.power >= 15f) && !(flashlight.on && flashlight.power < 15f) && LevelGeneration.Instance.finishedGenerating)
                {
                    VirtualAudioSource virtualAudioSource = flashlight.torchSource.gameObject.GetComponent<VirtualAudioSource>();
                    if (virtualAudioSource != null)
                    {
                        virtualAudioSource.Pause();
                    }
                    else if (ModSettings.logDebugText)
                    {
                        Debug.Log("VAS is null!\n" + new StackTrace().ToString());
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FOVCheck

            private static void HookFOVCheck(On.FOVCheck.orig_Start orig, FOVCheck fOVCheck)
            {
                if (PlayerPrefs.HasKey("FOV"))
                {
                    fOVCheck.fovFloat = 50f + ((PlayerPrefs.GetFloat("FOV") - ModSettings.minimumValueOnFOVSlider) * 20f / (ModSettings.maximumValueOnFOVSlider - ModSettings.minimumValueOnFOVSlider));
                    if (fOVCheck.fovFloat < ModSettings.minimumValueOnFOVSlider)
                    {
                        fOVCheck.fovFloat = 50f;//ModSettings.minimumValueOnFOVSlider;
                    }
                    else if (fOVCheck.fovFloat > ModSettings.maximumValueOnFOVSlider)
                    {
                        fOVCheck.fovFloat = 70f;//ModSettings.maximumValueOnFOVSlider;
                    }
                    for (int i = 0; i < fOVCheck.cameraList.Count; i++)
                    {
                        fOVCheck.cameraList[i].fieldOfView = fOVCheck.fovFloat;
                    }
                }
                else
                {
                    for (int j = 0; j < fOVCheck.cameraList.Count; j++)
                    {
                        fOVCheck.cameraList[j].fieldOfView = 60f;
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FOVSlider

            private static void HookFOVSlider()
            {
                On.FOVSlider.Start += new On.FOVSlider.hook_Start(HookFOVSliderStart);
                On.FOVSlider.Test += new On.FOVSlider.hook_Test(HookFOVSliderTest);
            }

            private static void HookFOVSliderStart(On.FOVSlider.orig_Start orig, FOVSlider fOVSlider)
            {
                fOVSlider.FOVSliderObject.minValue = ModSettings.minimumValueOnFOVSlider;
                fOVSlider.FOVSliderObject.maxValue = ModSettings.maximumValueOnFOVSlider;


                fOVSlider.delta.IsRealtime = true;
                fOVSlider.delta.StartTimer();
                fOVSlider.width = (float)Screen.width;
                fOVSlider.height = (float)Screen.height;
                fOVSlider.aspectRatio = fOVSlider.width / fOVSlider.height;
                if (PlayerPrefs.HasKey("FOV"))
                {
                    fOVSlider.fovFloat = PlayerPrefs.GetFloat("FOV");
                    if (fOVSlider.fovFloat < fOVSlider.FOVSliderObject.minValue)
                    {
                        fOVSlider.fovFloat = fOVSlider.FOVSliderObject.minValue;
                    }
                    else if (fOVSlider.fovFloat > fOVSlider.FOVSliderObject.maxValue)
                    {
                        fOVSlider.fovFloat = fOVSlider.FOVSliderObject.maxValue;
                    }
                    //fOVSlider.fovFloat = 50f + ((PlayerPrefs.GetFloat("FOV") - ModSettings.minimumValueOnFOVSlider) * 20f / (ModSettings.maximumValueOnFOVSlider - ModSettings.minimumValueOnFOVSlider));
                    fOVSlider.FOVSliderObject.value = fOVSlider.fovFloat;
                    fOVSlider.vFOVInRads = fOVSlider.fovFloat * 0.0174532924f;
                    fOVSlider.hFOVInRads = 2f * Mathf.Atan(Mathf.Tan(fOVSlider.vFOVInRads / 2f) * fOVSlider.aspectRatio);

                    //Debug.Log("Current scene name is: " + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
                    if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "MainSecondary") // MainSecondary is the generated level. Use the first bracket if the player is not in the generated level, which locks the FOV to the vanilla range of 50 to 70 to allow for proper menu UI interaction.
                    {
                        for (int i = 0; i < fOVSlider.cameraList.Count; i++)
                        {
                            fOVSlider.cameraList[i].fieldOfView = 50f + (fOVSlider.fovFloat - ModSettings.minimumValueOnFOVSlider) * 20f / (ModSettings.maximumValueOnFOVSlider - ModSettings.minimumValueOnFOVSlider);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < fOVSlider.cameraList.Count; i++)
                        {
                            fOVSlider.cameraList[i].fieldOfView = fOVSlider.fovFloat;
                        }
                    }
                }
                else
                {
                    fOVSlider.fovFloat = 60f;
                    fOVSlider.FOVSliderObject.value = fOVSlider.fovFloat;
                    fOVSlider.vFOVInRads = 1.04719758f;
                    fOVSlider.hFOVInRads = 2f * Mathf.Atan(Mathf.Tan(fOVSlider.vFOVInRads / 2f) * fOVSlider.aspectRatio);
                }


                //orig.Invoke(fOVSlider);
            }

            private static void HookFOVSliderTest(On.FOVSlider.orig_Test orig, FOVSlider fOVSlider)
            {
                if (LevelGeneration.Instance != null && LevelGeneration.Instance.finishedGenerating)
                {
                    for (int i = 0; i < fOVSlider.cameraList.Count; i++)
                    {
                        fOVSlider.aspectRatio = fOVSlider.width / fOVSlider.height;
                        fOVSlider.vFOVInRads = fOVSlider.FOVSliderObject.value * 0.0174532924f;
                        fOVSlider.hFOVInRads = 2f * Mathf.Atan(Mathf.Tan(fOVSlider.vFOVInRads / 2f) * fOVSlider.aspectRatio);
                        fOVSlider.vFOVInRads = 2f * Mathf.Atan(Mathf.Tan(fOVSlider.hFOVInRads / 2f) / fOVSlider.aspectRatio);
                        fOVSlider.cameraList[i].fieldOfView = fOVSlider.vFOVInRads * 57.29578f;
                    }
                }
                else
                {
                    for (int i = 0; i < fOVSlider.cameraList.Count; i++)
                    {
                        fOVSlider.aspectRatio = fOVSlider.width / fOVSlider.height;
                        fOVSlider.vFOVInRads = (50f + ((fOVSlider.FOVSliderObject.value - ModSettings.minimumValueOnFOVSlider) * 20f / (ModSettings.maximumValueOnFOVSlider - ModSettings.minimumValueOnFOVSlider))) * 0.0174532924f;
                        fOVSlider.hFOVInRads = 2f * Mathf.Atan(Mathf.Tan(fOVSlider.vFOVInRads / 2f) * fOVSlider.aspectRatio);
                        fOVSlider.vFOVInRads = 2f * Mathf.Atan(Mathf.Tan(fOVSlider.hFOVInRads / 2f) / fOVSlider.aspectRatio);
                        fOVSlider.cameraList[i].fieldOfView = fOVSlider.vFOVInRads * 57.29578f;
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FuelDecal

            private static void HookFuelDecalStart(On.FuelDecal.orig_Start orig, FuelDecal fuelDecal)
            {
                orig.Invoke(fuelDecal);
                fuelDecal.flammable.fireFuel *= ModSettings.fireDurationMultiplier;
                fuelDecal.flammable.lowFuel *= ModSettings.fireDurationMultiplier;
                fuelDecal.maxFuel *= ModSettings.fireDurationMultiplier;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FuelParticles

            private static void HookFuelParticlesUpdate(On.FuelParticles.orig_Update orig, FuelParticles fuelParticles)
            {
                if (ModSettings.infiniteFuelCanFuel && fuelParticles.pouring)
                {
                    fuelParticles.fuel = fuelParticles.maxFuel;
                }
                orig.Invoke(fuelParticles);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @FuseBoxManager

            private static void HookFuseBoxManagerAddFuseBox(On.FuseBoxManager.orig_AddFuseBox orig, FuseBoxManager fuseBoxManager, FuseBox _fuseBox)
            {
                if (!fuseBoxManager.fuseboxes.ContainsKey(_fuseBox.powerRegion))
                {
                    orig.Invoke(fuseBoxManager, _fuseBox);
                }
                else
                {
                    ModSettings.additionalFuseBoxesToSetUpAfterGeneration.Add(_fuseBox);
                }
            }

            private static void HookFuseBoxManagerSetupFuse(On.FuseBoxManager.orig_SetupFuse orig, FuseBoxManager fuseBoxManager, FuseBox _fusebox)
            {
                if (!ModSettings.noPreFilledFuseBoxes)
                {
                    _fusebox.AddFuse();
                    _fusebox.AddPreExistingFuse();
                    if (!ModSettings.darkShip)
                    {
                        _fusebox.transform.parent.GetComponentInChildren<FuseBoxLever>().PullLever(false);
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @GenericLight

            private static void HookGenericLight()
            {
                On.GenericLight.OnGenerationComplete += new On.GenericLight.hook_OnGenerationComplete(HookGenericLightOnGenerationComplete);
                On.GenericLight.OnPowerUp += new On.GenericLight.hook_OnPowerUp(HookGenericLightOnPowerUp);
                On.GenericLight.Start += new On.GenericLight.hook_Start(HookGenericLightStart);
            }

            private static void HookGenericLightOnGenerationComplete(On.GenericLight.orig_OnGenerationComplete orig, GenericLight genericLight)
            {
                genericLight.lightCuller = ((MonoBehaviour)genericLight).GetComponent<Light>().GetComponent<LightCull2>();
                genericLight.UpdateLight();
                if (ModSettings.darkShip)
                {
                    if (!ModSettings.powerableLights)
                    {
                        genericLight.startingLightType = GenericLight.LightTypes.Off;
                    }
                    genericLight.lightType = GenericLight.LightTypes.Off;
                }
                GenericLight.genComplete = true;
            }

            private static void HookGenericLightOnPowerUp(On.GenericLight.orig_OnPowerUp orig, GenericLight genericLight)
            {
                genericLight.isPowered = true;
                if (ModSettings.darkShip && ModSettings.powerableLights)
                {
                    genericLight.lightType = GenericLight.LightTypes.Normal;
                }
                genericLight.UpdateLight();
            }

            private static void HookGenericLightStart(On.GenericLight.orig_Start orig, GenericLight genericLight)
            {
                if (ModSettings.UseCustomColour(ModSettings.shipGenericLightsColour) || ModSettings.randomShipGenericLightsColours)
                {
                    if (ModSettings.UseCustomColour(ModSettings.shipGenericLightsColour) && ModSettings.randomShipGenericLightsColours)
                    {
                        float randomChance = UnityEngine.Random.value;
                        if (randomChance > 0.5f)
                        {
                            ((MonoBehaviour)genericLight).GetComponent<Light>().color = ModSettings.ConvertColourStringToColour(ModSettings.shipGenericLightsColour);
                        }
                        else
                        {
                            ((MonoBehaviour)genericLight).GetComponent<Light>().color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0.5f, 1f));
                        }
                    }
                    else if (ModSettings.UseCustomColour(ModSettings.shipGenericLightsColour))
                    {
                        ((MonoBehaviour)genericLight).GetComponent<Light>().color = ModSettings.ConvertColourStringToColour(ModSettings.shipGenericLightsColour);
                    }
                    else
                    {
                        ((MonoBehaviour)genericLight).GetComponent<Light>().color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0.5f, 1f));
                    }
                }
                genericLight.normalIntensity *= ModSettings.shipGenericLightIntensityMultiplier;
                ((MonoBehaviour)genericLight).GetComponent<Light>().range *= ModSettings.shipGenericLightRangeMultiplier;
                orig.Invoke(genericLight);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @GlobalMusic

            private static void HookGlobalMusic(On.GlobalMusic.orig_CheckMusicState orig, GlobalMusic globalMusic)
            {
                // This code is only run when MMM is not enabled.
                bool doesAPlayerHaveSpawnProtection = false;
                for (int i = 0; i < ModSettings.spawnProtection.Count; i++)
                {
                    if (ModSettings.spawnProtection[i])
                    {
                        doesAPlayerHaveSpawnProtection = true;
                        break;
                    }
                }
                /*
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    Debug.Log("GlobalMusic monster has type " + globalMusic.monsterType);
                }
                */
                if (!doesAPlayerHaveSpawnProtection)
                {
                    orig.Invoke(globalMusic);
                }
                else
                {
                    globalMusic.song = "Music/NoAlert/" + globalMusic.monsterType;
                    if (globalMusic.currentlyPlaying != globalMusic.song)
                    {
                        globalMusic.ChangeVariables(false, true, true, false, 0f);
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @GlowStick

            private static void HookGlowStickActivateGlowstick(On.GlowStick.orig_ActivateGlowstick orig, GlowStick glowStick)
            {
                orig.Invoke(glowStick);
                if (ModSettings.glowstickHunt && ModSettings.glowstickHuntColours.Contains(glowStick.glowLight.color))
                {
                    ModSettings.glowstickHuntCounter++;
                    if (ModSettings.glowstickHuntCounter >= ModSettings.specialGlowsticksRequired)
                    {
                        TimeScaleManager.Instance.StartCoroutine(StartGlowstickHuntFinale());
                        //((MonoBehaviour)glowStick).StartCoroutine(StartGlowstickHuntFinale());
                    }
                }
            }

            private static IEnumerator StartGlowstickHuntFinale()
            {
                // Start the liferaft escape sequence.
                TimeScaleManager.Instance.StartCoroutine(ReadyLiferaft());

                yield return new WaitForSeconds(5f);

                // Create a custom escape string to show to the player.
                string escapeString = "All glowsticks found.\n";

                if (ModSettings.numberOfMonsters > 1)
                {
                    escapeString += "Monsters approaching... ";
                }
                else if (ModSettings.numberOfMonsters == 1)
                {
                    escapeString += "Monster approaching... ";
                }

                escapeString += "Get to the liferaft!";


                // After 5 seconds (which is when the life raft readying text will have disappeared), warn the player that the monsters are approaching.
                ModSettings.ShowTextOnScreen(escapeString, 10f, true);

                yield return new WaitForSeconds(5f);

                if (ModSettings.numberOfMonsters > 0)
                {
                    // After 10 seconds, make the monster(s) persistent and force a chase.
                    ModSettings.persistentMonster = true;
                    ModSettings.ForceChase();
                }
                yield break;
            }

            private static void HookGlowStick(On.GlowStick.orig_Start orig, GlowStick glowStick)
            {
                orig.Invoke(glowStick);
                try
                {
                    if (ModSettings.UseCustomColour(ModSettings.glowstickColour))
                    {
                        AssignCustomGlowstickColour(glowStick, ModSettings.ConvertColourStringToColour(ModSettings.glowstickColour));
                    }
                    try
                    {
                        glowStick.intensity *= ModSettings.glowstickIntensityMultiplier;
                    }
                    catch
                    {
                        Debug.Log("Could not update glowstick intensity.");
                    }
                    try
                    {
                        glowStick.glowLight.range *= ModSettings.glowstickRangeMultiplier;
                    }
                    catch
                    {
                        Debug.Log("Could not update glowstick range.");
                    }
                }
                catch
                {
                    Debug.Log("Error initialising glowstick");
                }
            }

            public static void AssignCustomGlowstickColour(GlowStick glowStick, Color customColour, bool modifyOnlyOneGlowstick = false)
            {
                glowStick.glowLight.color = customColour;
                if (modifyOnlyOneGlowstick)
                {
                    // If the colour is to be assigned to only one glowstick, give the glowstick a unique material so that other glowsticks do not use a modified material.
                    glowStick.onMat = Instantiate(glowStick.onMat);
                }
                glowStick.onMat.color = customColour;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Hiding

            public static bool HookHidingget_IsHiding(Hiding hiding)
            {
                if (ModSettings.overpoweredHidingSpots)
                {
                    return false;
                }
                return hiding.hideValue >= 3f;
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


            private static void HookInventoryDisplayInventory(On.Inventory.orig_DisplayInventory orig, Inventory inventory)
            {
                if (!ModSettings.hideInventory)
                {
                    orig.Invoke(inventory);
                }
            }

            private static void HookInventoryStart(On.Inventory.orig_Start orig, Inventory inventory)
            {
                if (ModSettings.inventorySize != 0)
                {
                    inventory.maxInventoryCapacity = ModSettings.inventorySize;
                }
                orig.Invoke(inventory);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @KeyItem

            private static void HookKeyItemRandomizeCount(On.KeyItem.orig_RandomizeCount orig, KeyItem keyItem)
            {
                if (ModSettings.changeKeyItemSpawnNumbers != 0)
                {
                    // Make sure the minimum number of each key item that spawns is at least 1. This should spawn items that are usually disabled.
                    if (keyItem.minCount <= 0)
                    {
                        keyItem.minCount = 1;
                    }

                    // If the increase in the maximum number of items is greater than 0, increase the maximum number of the key item to spawn. Else, set the maximum number of the key item to spawn to the minimum.
                    if (ModSettings.changeKeyItemSpawnNumbers > 0)
                    {
                        keyItem.maxCount += ModSettings.changeKeyItemSpawnNumbers;
                    }
                    if (ModSettings.changeKeyItemSpawnNumbers < 0)
                    {
                        keyItem.maxCount = keyItem.minCount;
                    }
                }
                orig.Invoke(keyItem);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @KeyItemPlaceholder

            private static float HookKeyItemPlaceholderCalculateSuitability(On.KeyItemPlaceholder.orig_CalculateSuitability orig, KeyItemPlaceholder keyItemPlaceholder, PrimaryRegionType _idealRegion, KeyItem _item, Dictionary<PrimaryRegionType, Dictionary<string, int>> _regionItemCounts, Dictionary<PrimaryRegionType, Dictionary<string, int>> _maxRegionItems)
            {
                if (!ModSettings.spawnItemsAnywhere)
                {
                    if (_item.category == KeyItem.KeyItemCategory.Wall && keyItemPlaceholder.category != KeyItem.KeyItemCategory.Wall)
                    {
                        return -1f;
                    }
                    if (!keyItemPlaceholder.room.allowKeyItemSpawning && !ModSettings.diverseItemSpawns)
                    {
                        return -1f;
                    }
                    float suitability = UnityEngine.Random.Range(0f, 1f);
                    bool flag = false;
                    if (_item.category == keyItemPlaceholder.category && _item.category != KeyItem.KeyItemCategory.Any)
                    {
                        suitability += 1000f;
                        flag = true;
                    }
                    else
                    {
                        switch (_item.category)
                        {
                            case KeyItem.KeyItemCategory.Small:
                                {
                                    KeyItem.KeyItemCategory keyItemCategory = keyItemPlaceholder.category;
                                    if (keyItemCategory != KeyItem.KeyItemCategory.Medium)
                                    {
                                        if (keyItemCategory != KeyItem.KeyItemCategory.Large)
                                        {
                                            if (keyItemCategory == KeyItem.KeyItemCategory.Any)
                                            {
                                                suitability += 700f;
                                                flag = true;
                                            }
                                        }
                                        else
                                        {
                                            suitability += 800f;
                                            flag = true;
                                        }
                                    }
                                    else
                                    {
                                        suitability += 900f;
                                        flag = true;
                                    }
                                    break;
                                }
                            case KeyItem.KeyItemCategory.Medium:
                                {
                                    KeyItem.KeyItemCategory keyItemCategory2 = keyItemPlaceholder.category;
                                    if (keyItemCategory2 != KeyItem.KeyItemCategory.Large)
                                    {
                                        if (keyItemCategory2 == KeyItem.KeyItemCategory.Any)
                                        {
                                            suitability += 950f;
                                            flag = true;
                                        }
                                    }
                                    else
                                    {
                                        suitability += 1000f;
                                        flag = true;
                                    }
                                    break;
                                }
                            case KeyItem.KeyItemCategory.Large:
                                {
                                    KeyItem.KeyItemCategory keyItemCategory3 = keyItemPlaceholder.category;
                                    if (keyItemCategory3 == KeyItem.KeyItemCategory.Any)
                                    {
                                        suitability += 1000f;
                                        flag = true;
                                    }
                                    break;
                                }
                            case KeyItem.KeyItemCategory.Any:
                                switch (keyItemPlaceholder.category)
                                {
                                    case KeyItem.KeyItemCategory.Small:
                                        suitability += 1000f;
                                        flag = true;
                                        break;
                                    case KeyItem.KeyItemCategory.Medium:
                                        suitability += 900f;
                                        flag = true;
                                        break;
                                    case KeyItem.KeyItemCategory.Large:
                                        suitability += 800f;
                                        flag = true;
                                        break;
                                    case KeyItem.KeyItemCategory.Any:
                                        suitability += 700f;
                                        flag = true;
                                        break;
                                }
                                break;
                        }
                    }
                    if (!flag)
                    {
                        return -1f;
                    }
                    if (keyItemPlaceholder.IsValidRegion(_item, _regionItemCounts, _maxRegionItems))
                    {
                        suitability += 25f;
                    }
                    suitability -= Mathf.Clamp((float)keyItemPlaceholder.room.itemsSpawned * KeyItemPlaceholder.itemPenalty, 0f, 1f);
                    if (_idealRegion == keyItemPlaceholder.primaryRegion)
                    {
                        suitability += 25f;
                    }
                    return suitability;
                }
                else
                {
                    return float.MaxValue;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @KeyItemSystem

            private static void HookKeyItemSystemSetUpLists(On.KeyItemSystem.orig_SetUpLists orig, KeyItemSystem keyItemSystem)
            {
                try
                {
                    if (ModSettings.spawnDeactivatedItems || ModSettings.spawnWithFlareGun)
                    {
                        KeyItem[] keyItems = Resources.FindObjectsOfTypeAll<KeyItem>();

                        if (keyItems != null)
                        {
                            foreach (KeyItem keyItem in keyItems)
                            {
                                if (ModSettings.logDebugText)
                                {
                                    Debug.Log("Key item found via FindObjectsOfTypeAll is " + keyItem.name + ". It is set to spawn a minimum of " + keyItem.minCount + " times and a maximum of " + keyItem.maxCount + " times. Its priority is " + keyItem.priority + " and its category is " + keyItem.category + ".");
                                }
                                if (ModSettings.spawnDeactivatedItems && keyItem.name.Equals("WalkieTalkie"))
                                {
                                    keyItem.gameObject.SetActive(true);
                                    keyItem.minCount = 4;
                                    keyItem.maxCount = 6;
                                    keyItem.priority = 1;
                                    keyItemSystem.keyItems.Add(keyItem);
                                    Debug.Log("Walkie talkie added successfully via FindObjectsOfTypeAll.");
                                }
                                else if (ModSettings.spawnWithFlareGun && keyItem.name.Equals("FlareGunNew") && !keyItemSystem.keyItems.Contains(keyItem))
                                {
                                    keyItem.gameObject.SetActive(true);
                                    keyItem.minCount = 1;
                                    keyItem.maxCount = 1;
                                    keyItemSystem.keyItems.Add(keyItem);
                                    Debug.Log("Flare Gun added successfully via FindObjectsOfTypeAll when it wasn't added normally.");
                                }
                            }
                        }
                        else
                        {
                            Debug.Log("Could not find key items.");
                        }
                    }

                    foreach (KeyItem keyItem in keyItemSystem.keyItems)
                    {
                        if (ModSettings.spawnDeactivatedItems && keyItem.name.Equals("Compass"))
                        {
                            keyItem.minCount = 2;
                            keyItem.maxCount = 3;
                            keyItem.priority = 1;
                        }

                        if (ModSettings.logDebugText)
                        {
                            Debug.Log("Key item in KeyItemSystem is " + keyItem.name + ". It is set to spawn a minimum of " + keyItem.minCount + " times and a maximum of " + keyItem.maxCount + " times. Its priority is " + keyItem.priority + " and its category is " + keyItem.category + ".");
                        }
                    }
                    orig.Invoke(keyItemSystem);

                    if (ModSettings.spawnDeactivatedItems)
                    {
                        keyItemSystem.maxes[PrimaryRegionType.CrewDeck]["WalkieTalkie"] = 10;
                    }

                    if (ModSettings.changeKeyItemSpawnNumbers != 0)
                    {
                        foreach (KeyItem keyItem in keyItemSystem.keyItems)
                        {
                            // If the increase in the maximum number of items is greater than 0, increase the maximum number of the key item to spawn. Else, set the maximum number of the key item to spawn to the minimum.

                            keyItem.minCount += ModSettings.changeKeyItemSpawnNumbers;
                            keyItem.maxCount += ModSettings.changeKeyItemSpawnNumbers;

                            if (keyItem.maxCount < 1)
                            {
                                // If the maximum count is less than 1, then the minimum count will also be less than 1 as the highest the minimum count can be is the maximum count.
                                if (!ModSettings.allowKeyItemsToNotSpawnAtAll)
                                {
                                    keyItem.minCount = 1;
                                    keyItem.maxCount = 1;
                                }
                                else
                                {
                                    keyItem.minCount = 0;
                                    keyItem.maxCount = 0;
                                }
                            }
                            else if (keyItem.minCount < 1)
                            {
                                // Even if the maximum count is not less than one, the minimum count still has to be checked as this may be less than the maximum count and thus below zero.
                                if (!ModSettings.allowKeyItemsToNotSpawnAtAll)
                                {
                                    keyItem.minCount = 1;
                                }
                                else
                                {
                                    keyItem.minCount = 0;
                                }
                            }

                            foreach (PrimaryRegionType primaryRegionType in keyItemSystem.maxes.Keys)
                            {
                                keyItemSystem.maxes[primaryRegionType][keyItem.name] += ModSettings.changeKeyItemSpawnNumbers;
                                if (keyItemSystem.maxes[primaryRegionType][keyItem.name] < 1)
                                {
                                    if (!ModSettings.allowKeyItemsToNotSpawnAtAll)
                                    {
                                        keyItemSystem.maxes[primaryRegionType][keyItem.name] = 1;
                                    }
                                    else
                                    {
                                        keyItemSystem.maxes[primaryRegionType][keyItem.name] = 0;
                                    }
                                }
                            }
                        }
                    }

                    if (ModSettings.diverseItemSpawns)
                    {
                        foreach (KeyItem keyItem in keyItemSystem.keyItems)
                        {
                            foreach (PrimaryRegionType primaryRegionType in keyItemSystem.maxes.Keys)
                            {
                                if (keyItemSystem.maxes[primaryRegionType][keyItem.name] < 1)
                                {
                                    keyItemSystem.maxes[primaryRegionType][keyItem.name] = 1;
                                }
                            }
                        }
                    }
                }
                catch
                {
                    Debug.Log("Could not modify KeyItemSystem.");
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @LevelGeneration

            private static void HookLevelGeneration()
            {
                if (!ModSettings.startedWithMMM)
                {
                    On.LevelGeneration.Awake += new On.LevelGeneration.hook_Awake(HookLevelGenerationAwake);
                }
                On.LevelGeneration.Begin += new On.LevelGeneration.hook_Begin(HookLevelGenerationBegin);
                On.LevelGeneration.DoBreak += new On.LevelGeneration.hook_DoBreak(HookLevelGenerationDoBreak);
                On.LevelGeneration.SpawnInitialRooms += new On.LevelGeneration.hook_SpawnInitialRooms(HookLevelGenerationSpawnInitialRooms);
                new Hook(typeof(LevelGeneration).GetNestedType("<SpawnRooms>c__Iterator1", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.BaseFeatures).GetMethod("HookLevelGenerationSpawnRoomsIntermediateHook"), null);
                new Hook(typeof(LevelGeneration).GetNestedType("<SpawnRandomRooms>c__Iterator2", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.BaseFeatures).GetMethod("HookLevelGenerationSpawnRandomRoomsIntermediateHook"), null);
                new Hook(typeof(LevelGeneration).GetNestedType("<SpawnCorridors>c__Iterator3", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.BaseFeatures).GetMethod("HookLevelGenerationSpawnCorridorsIntermediateHook"), null);
                new Hook(typeof(LevelGeneration).GetNestedType("<SpawnJointsAndDoors>c__Iterator4", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static).GetMethod("MoveNext", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance), typeof(MonstrumExtendedSettingsMod.ExtendedSettingsModScript.BaseFeatures).GetMethod("HookLevelGenerationSpawnJointsAndDoorsIntermediateHook"), null);

                /*
                On.SpawnDeckLG.SpawnDeckPieces += new On.SpawnDeckLG.hook_SpawnDeckPieces(HookSpawnDeckLGSpawnDeckPieces);
                On.SpawnDeckCargoArea.GenerateCargoArea += new On.SpawnDeckCargoArea.hook_GenerateCargoArea(HookSpawnDeckCargoAreaGenerateCargoArea);
                On.SpawnWalkwaysLG.SpawnWalkways += new On.SpawnWalkwaysLG.hook_SpawnWalkways(HookSpawnWalkwaysLGSpawnWalkways);
                //On.SpawnEnginesLG.SpawnEngineArea += new On.SpawnEnginesLG.hook_SpawnEngineArea(HookSpawnEnginesLGSpawnEngineArea);
                On.SpawnCargoHoldLG.SpawnCargoArea += new On.SpawnCargoHoldLG.hook_SpawnCargoArea(HookSpawnCargoHoldLGSpawnCargoArea);
                On.SpawnShellLG.GenerateShell += new On.SpawnShellLG.hook_GenerateShell(HookSpawnShellLGGenerateShell);
                */
            }

            public static void CommonLevelGeneration1(LevelGeneration levelGeneration)
            {
                ModSettings.ReadBeforeGeneration();

                levelGeneration.removeEngines = false;
                levelGeneration.removeCargo = false;
                Loading.UpdateLoading(15f);
                int randomSeed = UnityEngine.Random.Range(0, int.MaxValue);
                if (ModSettings.useCustomSeed)
                {
                    randomSeed = ModSettings.seed;
                    levelGeneration.levelSeed = ModSettings.seed;
                }
                Debug.Log("Test Seed: " + randomSeed);
                if (levelGeneration.levelSeed != 0)
                {
                    UnityEngine.Random.InitState(levelGeneration.levelSeed);
                    Debug.Log("Level Test: " + levelGeneration.levelSeed);
                }
                else
                {
                    UnityEngine.Random.InitState(randomSeed);
                    Debug.Log("Level Test: " + randomSeed);
                }
            }

            public static void CommonLevelGeneration2(LevelGeneration levelGeneration)
            {
                if (ModSettings.WallhacksMode)
                {
                    levelGeneration.winterWonderland = true;
                }

                if (ModSettings.startRoomRegion.Equals("Upper Deck") || ModSettings.startRoomRegion.Equals("upper deck"))
                {
                    LevelGeneration.Instance.spawnRoomType = LevelGeneration.SpawnRoomType.Upper;
                }
                else if (ModSettings.startRoomRegion.Equals("Lower Deck") || ModSettings.startRoomRegion.Equals("lower deck"))
                {
                    LevelGeneration.Instance.spawnRoomType = LevelGeneration.SpawnRoomType.Lower;
                }
            }

            private static void HookLevelGenerationAwake(On.LevelGeneration.orig_Awake orig, LevelGeneration levelGeneration)
            {
                CommonLevelGeneration1(levelGeneration);
                MonsterSelection monsterSelection = UnityEngine.Object.FindObjectOfType<MonsterSelection>();
                levelGeneration.allMonsters = GameObject.FindGameObjectsWithTag("Monster");
                if (monsterSelection != null)
                {
                    if (ModSettings.numberOfBrutes == 1)
                    {
                        levelGeneration.selectedMonster = monsterSelection.NameToObject("Brute");
                    }
                    else if (ModSettings.numberOfHunters == 1)
                    {
                        levelGeneration.selectedMonster = monsterSelection.NameToObject("Hunter");
                    }
                    else if (ModSettings.numberOfFiends == 1)
                    {
                        levelGeneration.selectedMonster = monsterSelection.NameToObject("Fiend");
                    }
                    else if (ModSettings.numberOfSmokeMonsters == 1)
                    {
                        levelGeneration.selectedMonster = SmokeMonster.CreateSmokeMonster(monsterSelection);
                    }
                    else
                    {
                        if (ModSettings.bannedRandomMonsters.Count > 0 && ModSettings.bannedRandomMonsters.Count < 3)
                        {
                            do
                            {
                                levelGeneration.selectedMonster = monsterSelection.Select();
                            } while (ModSettings.bannedRandomMonsters.Contains(levelGeneration.selectedMonster.GetComponent<Monster>().monsterType));
                        }
                        else
                        {
                            levelGeneration.selectedMonster = monsterSelection.Select();
                        }
                    }
                    levelGeneration.chosenMonstType = levelGeneration.selectedMonster.GetComponent<Monster>().MonsterType;
                }
                else
                {
                    Debug.Log("MonsterSelection is not in the scene! No monster!");
                }
                CommonLevelGeneration2(levelGeneration);
            }

            private static void HookLevelGenerationBegin(On.LevelGeneration.orig_Begin orig, LevelGeneration levelGeneration)
            {
                levelGeneration.StartCoroutine(StartGenerationAfterAFrame(orig, levelGeneration));
            }

            private static IEnumerator StartGenerationAfterAFrame(On.LevelGeneration.orig_Begin orig, LevelGeneration levelGeneration)
            {
                Debug.Log("Starting LevelGeneration.Begin");
                LoadingBackground loadingBackground = FindObjectOfType<LoadingBackground>();
                string loadingProgressText = "Setting Up Custom Level Generation";
                SetLoadingText(loadingBackground, loadingProgressText);
                yield return null;
                /*
                foreach (Room room in levelGeneration.roomPrefabs)
                {
                    if (room != null)
                    {
                        Debug.Log("Room prefab is " + room.name);
                        if (room.ActiveModel != null)
                        {
                            Debug.Log("and activeModel is " + room.ActiveModel.name);
                        }

                        if (room.name.Equals("Bridge_v1"))
                        {
                            AssetBundle ab = null;
                            UnityEngine.Object[] ob = null;
                            string sparkyFilePathNew = Path.Combine(Directory.GetCurrentDirectory(), "alphabridge");
                            Debug.Log("File path used for Sparky Asset Bundle is: " + sparkyFilePathNew);
                            try
                            {
                                try
                                {
                                    ab = AssetBundle.LoadFromFile(sparkyFilePathNew);
                                }
                                catch
                                {
                                    Debug.Log("Error loading Asset Bundle from file");
                                }
                                try
                                {
                                    if (ab != null)
                                    {
                                        ob = ab.LoadAllAssets();
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

                            GameObject alphaBridge = Instantiate((GameObject)ob[0]);
                            alphaBridge.transform.SetParent(room.transform);

                            /*
                            foreach (UnityEngine.Object sparkyObject in ob)
                            {
                                Debug.Log("Object from asset bundle is called " + sparkyObject.name + " and has type " + sparkyObject.GetType());

                                if (sparkyObject.GetType() == typeof(GameObject))
                                {
                                    Component[] sparkyGOComponentsInChildren = ((GameObject)sparkyObject).GetComponentsInChildren<Component>();
                                    foreach (Component component in sparkyGOComponentsInChildren)
                                    {
                                        Debug.Log("Component in children of asset bundle object name is " + component.name + " and type is " + component.GetType());
                                    }
                                }
                            }

                            foreach (Component component in room.GetComponentsInChildren<Component>())
                            {
                                Debug.Log("Component in children of Bridge name is " + component.name + " and type is " + component.GetType());
                                if (component.GetType() == typeof(MeshFilter))
                                {
                                    MeshFilter mf = (MeshFilter)component;
                                    mf = ((GameObject)ob[0]).GetComponentInChildren<MeshFilter>();
                                }
                                else if (component.GetType() == typeof(MeshRenderer))
                                {
                                    MeshRenderer mf = (MeshRenderer)component;
                                    mf = ((GameObject)ob[0]).GetComponentInChildren<MeshRenderer>();
                                }
                            }

                            PrefabProxy bridgePrefab = (PrefabProxy)room.GetComponentsInChildren<Component>()[12];

                            foreach (Component component in bridgePrefab.prefab.GetComponentsInChildren<Component>())
                            {
                                Debug.Log("Component in children of Bridge Prefab name is " + component.name + " and type is " + component.GetType());
                                if (component.GetType() == typeof(MeshFilter))
                                {
                                    MeshFilter mf = (MeshFilter)component;
                                    mf = ((GameObject)ob[0]).GetComponentInChildren<MeshFilter>();
                                }
                                else if (component.GetType() == typeof(MeshRenderer))
                                {
                                    MeshRenderer mf = (MeshRenderer)component;
                                    mf = ((GameObject)ob[0]).GetComponentInChildren<MeshRenderer>();
                                }
                            }

                            room.activeModel = (GameObject)ob[0];
                            if (room.ActiveModel != null)
                            {
                                Debug.Log("New activeModel is " + room.ActiveModel.name);
                            }
                        }
                    }
                }
                */


                /*
                try
                {
                    // ~ Experimental removal.
                    levelGeneration.removeCargo = true; // Removes lower cargo hold and corridors on side.
                    //levelGeneration.removeDeckCargo = true; // Removes upper cargo containers and external walkways.
                    //levelGeneration.removeWalkways = true; // Bridge walkways? Heli stairs too? Also shell of ship?
                    CopyRegionOverYUsingReferenceNode(60, 1, 3, 17, 53, 0, 4, 0, (int)Settings.ShipCubesCount.z - 1); // Replace cargo holds by deck 0 like layout.

                    // Normal changes.
                    LevelGenerationChanges(); // Calling this before lists are setup is fine as the rooms are all in the pre-setup prefab room lists until spawned in anyway. It actually must be called before lists are setup as during the list setup regionData from the RegionManager is used to set up LevelGeneration's nodeData.
                }
                catch (Exception exception)
                {
                    ProcessLoadingError(loadingProgressText, loadingBackground, exception);
                }
                */

                LevelGenerationChanges(); // Calling this before lists are setup is fine as the rooms are all in the pre-setup prefab room lists until spawned in anyway. It actually must be called before lists are setup as during the list setup regionData from the RegionManager is used to set up LevelGeneration's nodeData.
                orig.Invoke(levelGeneration);
                ((MonoBehaviour)levelGeneration).StartCoroutine(WaitUntilGenerationIsFinished());
                yield break;
            }


            private static bool HookLevelGenerationDoBreak(On.LevelGeneration.orig_DoBreak orig, LevelGeneration levelGeneration)
            {
                if (ModSettings.useCustomSeed || ModSettings.consistentLevelGeneration)
                {
                    return false;
                }
                return orig.Invoke(levelGeneration);
            }

            private static void HookLevelGenerationSpawnInitialRooms(On.LevelGeneration.orig_SpawnInitialRooms orig, LevelGeneration levelGeneration)
            {
                if (ModSettings.addAdditionalCrewDeckBuilding)
                {
                    int initialCount = levelGeneration.victoryRooms.Count;
                    for (int victoryRoomIndex = 0; victoryRoomIndex < initialCount; victoryRoomIndex++)
                    {
                        if (levelGeneration.victoryRooms[victoryRoomIndex].MinRoomCount > 1)
                        {
                            for (int roomsAdded = 1; roomsAdded < levelGeneration.victoryRooms[victoryRoomIndex].MinRoomCount; roomsAdded++)
                            {
                                levelGeneration.victoryRooms.Add(levelGeneration.victoryRooms[victoryRoomIndex]);
                            }
                        }
                    }
                }

                orig.Invoke(levelGeneration);

                /*
                LoadingBackground loadingBackground = FindObjectOfType<LoadingBackground>();
                string loadingProgressText = "Spawning Initial Rooms";
                SetLoadingText(loadingBackground, loadingProgressText);
                try
                {
                    orig.Invoke(levelGeneration);
                }
                catch
                {
                    string loadingProgressTextError = "Error While " + loadingProgressText;
                    Debug.Log(loadingProgressTextError);
                    SetLoadingText(loadingBackground, loadingProgressTextError);
                    ModSettings.errorDuringLevelGeneration = true;
                }
                */
            }

            /*
            private static void HookSpawnDeckLGSpawnDeckPieces(On.SpawnDeckLG.orig_SpawnDeckPieces orig, List<Room> _deckPrefabs, List<Room> _deckInUse, GameObject _deckParent)
            {
                LoadingBackground loadingBackground = FindObjectOfType<LoadingBackground>();
                string loadingProgressText = "Spawning Deck Pieces";
                SetLoadingText(loadingBackground, loadingProgressText);
                try
                {
                    orig.Invoke(_deckPrefabs, _deckInUse, _deckParent);
                }
                catch
                {
                    string loadingProgressTextError = "Error While " + loadingProgressText;
                    Debug.Log(loadingProgressTextError);
                    SetLoadingText(loadingBackground, loadingProgressTextError);
                    ModSettings.errorDuringLevelGeneration = true;
                }
            }

            private static void HookSpawnDeckCargoAreaGenerateCargoArea(On.SpawnDeckCargoArea.orig_GenerateCargoArea orig, List<GameObject> _onDeckCargoPrefabs, GameObject _parentObj, List<Room> _roomsInUse, List<Room> _cargoContainerRooms)
            {
                LoadingBackground loadingBackground = FindObjectOfType<LoadingBackground>();
                string loadingProgressText = "Spawning Deck Cargo Containers";
                SetLoadingText(loadingBackground, loadingProgressText);
                try
                {
                    orig.Invoke(_onDeckCargoPrefabs, _parentObj, _roomsInUse, _cargoContainerRooms);
                }
                catch
                {
                    string loadingProgressTextError = "Error While " + loadingProgressText;
                    Debug.Log(loadingProgressTextError);
                    SetLoadingText(loadingBackground, loadingProgressTextError);
                    ModSettings.errorDuringLevelGeneration = true;
                }
            }

            private static void HookSpawnWalkwaysLGSpawnWalkways(On.SpawnWalkwaysLG.orig_SpawnWalkways orig, List<Room> _rooms, List<Room> _walkwaysList, GameObject _roomsParent)
            {
                LoadingBackground loadingBackground = FindObjectOfType<LoadingBackground>();
                string loadingProgressText = "Spawning Walkways";
                SetLoadingText(loadingBackground, loadingProgressText);
                try
                {
                    orig.Invoke(_rooms, _walkwaysList, _roomsParent);
                }
                catch
                {
                    string loadingProgressTextError = "Error While " + loadingProgressText;
                    Debug.Log(loadingProgressTextError);
                    SetLoadingText(loadingBackground, loadingProgressTextError);
                    ModSettings.errorDuringLevelGeneration = true;
                }
                loadingProgressText = "Spawning Engine Room [No Error Notifications]";
                SetLoadingText(loadingBackground, loadingProgressText);
            }

            private static void HookSpawnEnginesLGSpawnEngineArea(On.SpawnEnginesLG.orig_SpawnEngineArea orig, List<Room> _enginesInUse, GameObject _engineParent)
            {
                LoadingBackground loadingBackground = FindObjectOfType<LoadingBackground>();
                string loadingProgressText = "Spawning Engine Room";
                SetLoadingText(loadingBackground, loadingProgressText);
                try
                {
                    orig.Invoke(_enginesInUse, _engineParent);
                }
                catch
                {
                    string loadingProgressTextError = "Error While " + loadingProgressText;
                    Debug.Log(loadingProgressTextError);
                    SetLoadingText(loadingBackground, loadingProgressTextError);
                    ModSettings.errorDuringLevelGeneration = true;
                }
            }


            private static void HookSpawnCargoHoldLGSpawnCargoArea(On.SpawnCargoHoldLG.orig_SpawnCargoArea orig, List<GameObject> _cargoWalkwayPrefabs, List<Room> _cargoRoomPrefabs, List<Room> _cargoRoomsInUse, GameObject _cargoParent)
            {
                LoadingBackground loadingBackground = FindObjectOfType<LoadingBackground>();
                string loadingProgressText = "Spawning Cargo Hold";
                SetLoadingText(loadingBackground, loadingProgressText);
                try
                {
                    orig.Invoke(_cargoWalkwayPrefabs, _cargoRoomPrefabs, _cargoRoomsInUse, _cargoParent);
                }
                catch
                {
                    string loadingProgressTextError = "Error While " + loadingProgressText;
                    Debug.Log(loadingProgressTextError);
                    SetLoadingText(loadingBackground, loadingProgressTextError);
                    ModSettings.errorDuringLevelGeneration = true;
                }
                loadingProgressText = "Spawning Random Rooms [No Error Notifications]";
                SetLoadingText(loadingBackground, loadingProgressText);
            }


            private static void HookSpawnShellLGGenerateShell(On.SpawnShellLG.orig_GenerateShell orig, List<GameObject> _shellPrefabs, List<GameObject> _shellsInUse, GameObject _shellsParent)
            {
                LoadingBackground loadingBackground = FindObjectOfType<LoadingBackground>();
                string loadingProgressText = "Spawning Shell";
                SetLoadingText(loadingBackground, loadingProgressText);
                try
                {
                    orig.Invoke(_shellPrefabs, _shellsInUse, _shellsParent);
                }
                catch
                {
                    string loadingProgressTextError = "Error While " + loadingProgressText;
                    Debug.Log(loadingProgressTextError);
                    SetLoadingText(loadingBackground, loadingProgressTextError);
                    ModSettings.errorDuringLevelGeneration = true;
                }
                loadingProgressText = "Spawning Corridors, Joints And Doors & Handling Post Processing [No Error Notifications]";
                SetLoadingText(loadingBackground, loadingProgressText);
            }
            */

            private static bool busyWithCoroutine;

            public static bool HookLevelGenerationSpawnRoomsIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!ManyMonstersMode.IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookLevelGenerationSpawnRooms((LevelGeneration)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    ManyMonstersMode.IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookLevelGenerationSpawnRooms(LevelGeneration levelGeneration)
            {
                // ~ Experimental level generation changes.
                //levelGeneration.victoryRooms.Clear(); // Forcibly removes victory rooms.

                // Normal changes.
                // # Other methods cannot be returned by an IEnumerator method that uses an intermediate hook.
                LoadingBackground loadingBackground = FindObjectOfType<LoadingBackground>();
                int spawnOperations = 12;
                float loadIncrement = 50f / (float)spawnOperations;
                Loading.UpdateLoading(loadIncrement);
                string loadingProgressText = "Spawning Stairs";
                Debug.Log(loadingProgressText);
                SetLoadingText(loadingBackground, loadingProgressText);
                yield return null;
                try
                {
                    SpawnStairsLG.SpawnStairStack(levelGeneration.roomsInUse, levelGeneration.stairPrefabs, levelGeneration.roomsParent);
                }
                catch (Exception exception)
                {
                    ProcessLoadingError(loadingProgressText, loadingBackground, exception);
                }

                Loading.UpdateLoading(loadIncrement);
                loadingProgressText = "Spawning Initial Rooms";
                Debug.Log(loadingProgressText);
                SetLoadingText(loadingBackground, loadingProgressText);
                yield return null;
                try
                {
                    levelGeneration.SpawnInitialRooms();
                }
                catch (Exception exception)
                {
                    ProcessLoadingError(loadingProgressText, loadingBackground, exception);
                }


                Loading.UpdateLoading(loadIncrement);
                if (!levelGeneration.removeWalkways)
                {
                    loadingProgressText = "Spawning Deck Pieces";
                    Debug.Log(loadingProgressText);
                    SetLoadingText(loadingBackground, loadingProgressText);
                    yield return null;
                    try
                    {
                        SpawnDeckLG.SpawnDeckPieces(levelGeneration.deckPrefabs, levelGeneration.deckInUse, levelGeneration.deckParent);
                    }
                    catch (Exception exception)
                    {
                        ProcessLoadingError(loadingProgressText, loadingBackground, exception);
                    }
                }

                Loading.UpdateLoading(loadIncrement);
                if (!levelGeneration.removeDeckCargo)
                {
                    loadingProgressText = "Spawning Deck Cargo Containers";
                    Debug.Log(loadingProgressText);
                    SetLoadingText(loadingBackground, loadingProgressText);
                    yield return null;
                    try
                    {
                        SpawnDeckCargoArea.GenerateCargoArea(levelGeneration.outerDeckWalkways, levelGeneration.deckCargoParent, levelGeneration.deckCargoInUse, levelGeneration.cargoRoomPrefabs);
                    }
                    catch (Exception exception)
                    {
                        ProcessLoadingError(loadingProgressText, loadingBackground, exception);
                    }
                }

                Loading.UpdateLoading(loadIncrement);
                if (!levelGeneration.removeWalkways)
                {
                    loadingProgressText = "Spawning Walkways";
                    Debug.Log(loadingProgressText);
                    SetLoadingText(loadingBackground, loadingProgressText);
                    yield return null;
                    try
                    {
                        SpawnWalkwaysLG.SpawnWalkways(levelGeneration.walkwaysInUse, levelGeneration.walkwayPrefabs, levelGeneration.walkwaysParent);
                    }
                    catch (Exception exception)
                    {
                        ProcessLoadingError(loadingProgressText, loadingBackground, exception);
                    }
                }

                Loading.UpdateLoading(loadIncrement);
                if (!levelGeneration.removeEngines)
                {
                    loadingProgressText = "Spawning Engine Room";
                    Debug.Log(loadingProgressText);
                    SetLoadingText(loadingBackground, loadingProgressText);
                    yield return null;
                    try
                    {
                        SpawnEnginesLG.SpawnEngineArea(levelGeneration.engineRoomsInUse, levelGeneration.enginesParent);
                    }
                    catch (Exception exception)
                    {
                        ProcessLoadingError(loadingProgressText, loadingBackground, exception);
                    }
                }

                Loading.UpdateLoading(loadIncrement);
                if (!levelGeneration.removeCargo)
                {
                    loadingProgressText = "Spawning Cargo Hold";
                    Debug.Log(loadingProgressText);
                    SetLoadingText(loadingBackground, loadingProgressText);
                    yield return null;
                    try
                    {
                        SpawnCargoHoldLG.SpawnCargoArea(levelGeneration.cargoShellPrefabs, levelGeneration.cargoRoomPrefabs, levelGeneration.cargoRoomsInUse, levelGeneration.cargoParent); // # Stop cargo hold spawning if commented out
                    }
                    catch (Exception exception)
                    {
                        ProcessLoadingError(loadingProgressText, loadingBackground, exception);
                    }
                }

                Loading.UpdateLoading(loadIncrement);
                loadingProgressText = "Spawning Random Rooms [No Error Notifications]";
                Debug.Log(loadingProgressText);
                SetLoadingText(loadingBackground, loadingProgressText);
                yield return null;
                busyWithCoroutine = true;
                ((MonoBehaviour)levelGeneration).StartCoroutine(levelGeneration.SpawnRandomRooms());
                while (busyWithCoroutine)
                {
                    yield return null;
                }

                Loading.UpdateLoading(loadIncrement);
                if (!levelGeneration.removeWalkways)
                {
                    loadingProgressText = "Spawning Shell";
                    Debug.Log(loadingProgressText);
                    SetLoadingText(loadingBackground, loadingProgressText);
                    yield return null;
                    try
                    {
                        SpawnShellLG.GenerateShell(levelGeneration.shellPrefabs, levelGeneration.shellsInUse, levelGeneration.shellsParent);
                    }
                    catch (Exception exception)
                    {
                        ProcessLoadingError(loadingProgressText, loadingBackground, exception);
                    }
                }

                Loading.UpdateLoading(loadIncrement);
                loadingProgressText = "Spawning Corridors [No Error Notifications]";
                Debug.Log(loadingProgressText);
                SetLoadingText(loadingBackground, loadingProgressText);
                yield return null;
                busyWithCoroutine = true;
                ((MonoBehaviour)levelGeneration).StartCoroutine(levelGeneration.SpawnCorridors());
                while (busyWithCoroutine)
                {
                    yield return null;
                }

                Loading.UpdateLoading(loadIncrement);
                loadingProgressText = "Spawning Joints And Doors [No Error Notifications]";
                Debug.Log(loadingProgressText);
                SetLoadingText(loadingBackground, loadingProgressText);
                yield return null;
                busyWithCoroutine = true;
                ((MonoBehaviour)levelGeneration).StartCoroutine(levelGeneration.SpawnJointsAndDoors());
                while (busyWithCoroutine)
                {
                    yield return null;
                }

                Loading.UpdateLoading(loadIncrement);
                loadingProgressText = "Handling Node Data";
                Debug.Log(loadingProgressText);
                SetLoadingText(loadingBackground, loadingProgressText);
                yield return null;
                try
                {
                    GameObject connectParent = new GameObject("Connect Parent");
                    for (int i = 0; i < levelGeneration.nodeData.Count; i++)
                    {
                        for (int j = 0; j < levelGeneration.nodeData[i].Count; j++)
                        {
                            for (int k = 0; k < levelGeneration.nodeData[i][j].Count; k++)
                            {
                                NodeData tempNode = levelGeneration.nodeData[i][j][k];
                                if (!tempNode.occupied && tempNode.nodeRoom == null)
                                {
                                    if (tempNode.primaryRegion == PrimaryRegionType.CargoHold || tempNode.primaryRegion == PrimaryRegionType.Engine || (j > 5 && RegionManager.Instance.CheckNodeForRegionExclusive(new Vector3((float)i, (float)j, (float)k), 0, -1)))
                                    {
                                        tempNode.nodeType = RoomStructure.None;
                                    }
                                    else
                                    {
                                        tempNode.nodeType = RoomStructure.Inaccessible;
                                    }
                                }
                                if (tempNode.nodeRoom != null)
                                {
                                    tempNode.nodeRoom.RoomNodes.Add(new Vector3((float)i, (float)j, (float)k));
                                }
                                if (tempNode.nodeType == RoomStructure.None && (tempNode.primaryRegion == PrimaryRegionType.CargoHold || (tempNode.primaryRegion == PrimaryRegionType.None && j > 5) || (tempNode.primaryRegion == PrimaryRegionType.Engine && j < 4)))
                                {
                                    for (int l = 0; l < 6; l++)
                                    {
                                        if (l != 2 || !RoomAppendageData.CheckAppendageList<NoCullingAppendage>(tempNode.regionNode, Orientation.Vertical))
                                        {
                                            if (l != 3 || !RoomAppendageData.CheckAppendageList<NoCullingAppendage>(tempNode.regionNode + Vector3.right, Orientation.Vertical))
                                            {
                                                if (l != 4 || !RoomAppendageData.CheckAppendageList<NoCullingAppendage>(tempNode.regionNode + Vector3.forward, Orientation.Horizontal))
                                                {
                                                    if (l != 5 || !RoomAppendageData.CheckAppendageList<NoCullingAppendage>(tempNode.regionNode, Orientation.Horizontal))
                                                    {
                                                        tempNode.connectedNodesUDLRFB[l] = true;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                if (levelGeneration.showConnectionGrid)
                                {
                                    for (int m = 0; m < tempNode.connectedNodesUDLRFB.Length; m++)
                                    {
                                        if (tempNode.connectedNodesUDLRFB[m])
                                        {
                                            GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Cube);
                                            temp.transform.parent = connectParent.transform;
                                            temp.transform.position = RegionManager.Instance.ConvertRegionNodeToShipWorldSpace(new Vector3((float)i, (float)j, (float)k));
                                            UnityEngine.Object.Destroy(temp.GetComponent<Collider>());
                                            switch (m)
                                            {
                                                case 0:
                                                    temp.transform.localScale = new Vector3(0.5f, Settings.CuboidDim.y * 0.5f, 0.5f);
                                                    temp.transform.position = new Vector3(temp.transform.position.x + Settings.CuboidDim.x * 0.5f, temp.transform.position.y + Settings.CuboidDim.y * 0.25f + 0.2f, temp.transform.position.z + Settings.CuboidDim.z * 0.5f);
                                                    break;
                                                case 1:
                                                    temp.transform.localScale = new Vector3(0.5f, Settings.CuboidDim.y * 0.5f, 0.5f);
                                                    temp.transform.position = new Vector3(temp.transform.position.x + Settings.CuboidDim.x * 0.5f, temp.transform.position.y - Settings.CuboidDim.y * 0.25f + 0.2f, temp.transform.position.z + Settings.CuboidDim.z * 0.5f);
                                                    break;
                                                case 2:
                                                    temp.transform.localScale = new Vector3(Settings.CuboidDim.x * 0.5f, 0.5f, 0.5f);
                                                    temp.transform.position = new Vector3(temp.transform.position.x - Settings.CuboidDim.x * 0.25f + Settings.CuboidDim.x * 0.5f, temp.transform.position.y + 0.2f, temp.transform.position.z + Settings.CuboidDim.z * 0.5f);
                                                    break;
                                                case 3:
                                                    temp.transform.localScale = new Vector3(Settings.CuboidDim.x * 0.5f, 0.5f, 0.5f);
                                                    temp.transform.position = new Vector3(temp.transform.position.x + Settings.CuboidDim.x * 0.25f + Settings.CuboidDim.x * 0.5f, temp.transform.position.y + 0.2f, temp.transform.position.z + Settings.CuboidDim.z * 0.5f);
                                                    break;
                                                case 4:
                                                    temp.transform.localScale = new Vector3(0.5f, 0.5f, Settings.CuboidDim.z * 0.5f);
                                                    temp.transform.position = new Vector3(temp.transform.position.x + Settings.CuboidDim.x * 0.5f, temp.transform.position.y + 0.2f, temp.transform.position.z + Settings.CuboidDim.z * 0.25f + Settings.CuboidDim.z * 0.5f);
                                                    break;
                                                case 5:
                                                    temp.transform.localScale = new Vector3(0.5f, 0.5f, Settings.CuboidDim.z * 0.5f);
                                                    temp.transform.position = new Vector3(temp.transform.position.x + Settings.CuboidDim.x * 0.5f, temp.transform.position.y + 0.2f, temp.transform.position.z - Settings.CuboidDim.z * 0.25f + Settings.CuboidDim.z * 0.5f);
                                                    break;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception exception)
                {
                    ProcessLoadingError(loadingProgressText, loadingBackground, exception);
                }

                Loading.UpdateLoading(loadIncrement);
                loadingProgressText = "Handling Post Processing";
                Debug.Log(loadingProgressText);
                SetLoadingText(loadingBackground, loadingProgressText);
                yield break;
            }

            public static bool HookLevelGenerationSpawnRandomRoomsIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!ManyMonstersMode.IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookLevelGenerationSpawnRandomRooms((LevelGeneration)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    ManyMonstersMode.IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookLevelGenerationSpawnRandomRooms(LevelGeneration levelGeneration)
            {
                //Debug.Log("Spawn random rooms is being hooked");
                List<Room> extraRooms = new List<Room>();
                int jLower = 0;
                int jUpper = 0;
                Room currentRoom = null;
                int highestPriorityValue = 0;
                for (int l = 0; l < levelGeneration.levelRooms.Count; l++)
                {
                    if (levelGeneration.levelRooms[l].RoomPriority > highestPriorityValue)
                    {
                        highestPriorityValue = levelGeneration.levelRooms[l].RoomPriority;
                    }
                }
                int currentPriority = 0;
                int minCount = 0;
                while (currentPriority <= highestPriorityValue)
                {
                    for (int m = 0; m < levelGeneration.levelRooms.Count; m++)
                    {
                        if (levelGeneration.levelRooms[m].RoomPriority == currentPriority && levelGeneration.levelRooms[m].MinRoomCount > minCount)
                        {
                            minCount = levelGeneration.levelRooms[m].MinRoomCount;
                        }
                    }
                    for (int i = 0; i < minCount; i++)
                    {
                        int j = 0;
                        while (j < levelGeneration.levelRooms.Count)
                        {
                            if (levelGeneration.levelRooms[j].RoomPriority != currentPriority || levelGeneration.levelRooms[j].ceaseSpawning)
                            {
                                goto IL_53C;
                            }
                            if (levelGeneration.categoryData[(int)levelGeneration.levelRooms[j].Category].chosenAmount > levelGeneration.categoryData[(int)levelGeneration.levelRooms[j].Category].currentAmount)
                            {
                                if (levelGeneration.levelRooms[j].RoomType == RoomStructure.Room || levelGeneration.levelRooms[j].RoomConnectionsType == ConnectorType.Room)
                                {
                                    jLower = levelGeneration.levelRooms[j].MinRoomCount;
                                    if (levelGeneration.levelRooms[j].RoomCount < jLower)
                                    {
                                        levelGeneration.levelRooms[j].SaveDoors = true;
                                        levelGeneration.roomsInUse.Add(UnityEngine.Object.Instantiate<Room>(levelGeneration.levelRooms[j]));
                                        currentRoom = levelGeneration.roomsInUse[levelGeneration.roomsInUse.Count - 1];
                                        currentRoom.name = string.Concat(new object[]
                                        {
                                currentRoom.name.Substring(0, currentRoom.name.Length - 7),
                                " (",
                                levelGeneration.levelRooms[j].RoomCount,
                                ")"
                                        });
                                        currentRoom.transform.position = Vector3.zero;
                                        currentRoom.transform.parent = levelGeneration.roomsParent.transform;
                                        currentRoom.roomIndex = j;
                                        currentRoom.RandomRegion();
                                        if (levelGeneration.removeRooms && ((levelGeneration.removeCrewRooms && currentRoom.PrimaryRegion == PrimaryRegionType.CrewDeck) || (levelGeneration.removeLwdRooms && currentRoom.PrimaryRegion == PrimaryRegionType.LowerDeck) || (levelGeneration.removeUpdRooms && currentRoom.PrimaryRegion == PrimaryRegionType.UpperDeck)))
                                        {
                                            levelGeneration.levelRooms.RemoveAt(j);
                                            j--;
                                            UnityEngine.Object.DestroyImmediate(currentRoom.gameObject);
                                            goto IL_567;
                                        }
                                        PositionRoomLG.PositionRoom(currentRoom, -1, -1, -1);
                                        if (currentRoom != null)
                                        {
                                            if (levelGeneration.levelRooms[j].ceaseSpawning)
                                            {
                                                levelGeneration.DeleteRoom(currentRoom);
                                            }
                                            Room.HandleAppendageData(currentRoom);
                                            levelGeneration.levelRooms[j].RoomCount++;
                                            levelGeneration.categoryData[(int)levelGeneration.levelRooms[j].Category].currentAmount++;
                                        }
                                    }
                                }
                                goto IL_53C;
                            }
                            levelGeneration.levelRooms[j].ceaseSpawning = true;
                            goto IL_53C;
                        IL_567:
                            j++;
                            continue;
                        IL_53C:
                            if (levelGeneration.DoBreak())
                            {
                                yield return null;
                                goto IL_567;
                            }
                            goto IL_567;
                        }
                    }
                    currentPriority++;
                }
                for (int n = 0; n < levelGeneration.levelRooms.Count; n++)
                {
                    jLower = levelGeneration.levelRooms[n].MinRoomCount;
                    jUpper = levelGeneration.levelRooms[n].MaxRoomCount;
                    if (levelGeneration.levelRooms[n].RandomRoomAmount)
                    {
                        jUpper = UnityEngine.Random.Range(levelGeneration.levelRooms[n].MinRoomCount, levelGeneration.levelRooms[n].MaxRoomCount + 1);
                    }
                    if (!levelGeneration.levelRooms[n].MinimumCountMet)
                    {
                        for (int num = jLower; num < jUpper; num++)
                        {
                            extraRooms.Add(levelGeneration.levelRooms[n]);
                            extraRooms[extraRooms.Count - 1].roomIndex = n;
                        }
                        levelGeneration.levelRooms[n].MinimumCountMet = true;
                    }
                }
                int index = 0;
                int k = 0;
                while (k < extraRooms.Count)
                {
                    index = UnityEngine.Random.Range(0, extraRooms.Count);
                    if (!levelGeneration.levelRooms[extraRooms[index].roomIndex].ceaseSpawning)
                    {
                        if (levelGeneration.categoryData[(int)levelGeneration.levelRooms[extraRooms[index].roomIndex].Category].chosenAmount > levelGeneration.categoryData[(int)levelGeneration.levelRooms[extraRooms[index].roomIndex].Category].currentAmount)
                        {
                            levelGeneration.levelRooms[extraRooms[index].roomIndex].SaveDoors = true;
                            levelGeneration.roomsInUse.Add(UnityEngine.Object.Instantiate<Room>(extraRooms[index]));
                            currentRoom = levelGeneration.roomsInUse[levelGeneration.roomsInUse.Count - 1];
                            currentRoom.name = string.Concat(new object[]
                            {
                    currentRoom.name.Substring(0, currentRoom.name.Length - 7),
                    " (",
                    extraRooms[index].RoomCount,
                    ")"
                            });
                            currentRoom.transform.position = Vector3.zero;
                            currentRoom.transform.parent = levelGeneration.roomsParent.transform;
                            currentRoom.RandomRegion();
                            if (levelGeneration.removeRooms && ((levelGeneration.removeCrewRooms && currentRoom.PrimaryRegion == PrimaryRegionType.CrewDeck) || (levelGeneration.removeLwdRooms && currentRoom.PrimaryRegion == PrimaryRegionType.LowerDeck) || (levelGeneration.removeUpdRooms && currentRoom.PrimaryRegion == PrimaryRegionType.UpperDeck)))
                            {
                                extraRooms.RemoveAt(index);
                                k--;
                                UnityEngine.Object.DestroyImmediate(currentRoom.gameObject);
                                goto IL_B31;
                            }
                            PositionRoomLG.PositionRoom(currentRoom, -1, -1, -1);
                            if (currentRoom != null)
                            {
                                if (levelGeneration.levelRooms[extraRooms[index].roomIndex].ceaseSpawning)
                                {
                                    levelGeneration.DeleteRoom(currentRoom);
                                    extraRooms.RemoveAt(index);
                                    k--;
                                    goto IL_B31;
                                }
                                Room.HandleAppendageData(currentRoom);
                                extraRooms[index].RoomCount++;
                                levelGeneration.categoryData[(int)levelGeneration.levelRooms[extraRooms[index].roomIndex].Category].currentAmount++;
                                extraRooms.RemoveAt(index);
                                k--;
                            }
                        }
                        else
                        {
                            extraRooms[index].ceaseSpawning = true;
                        }
                        goto IL_B06;
                    }
                    extraRooms.RemoveAt(index);
                    k--;
                    goto IL_B06;
                IL_B31:
                    k++;
                    continue;
                IL_B06:
                    if (levelGeneration.DoBreak())
                    {
                        yield return null;
                        goto IL_B31;
                    }
                    goto IL_B31;
                }
                busyWithCoroutine = false;
                yield break;
            }

            public static bool HookLevelGenerationSpawnCorridorsIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!ManyMonstersMode.IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookLevelGenerationSpawnCorridors((LevelGeneration)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    ManyMonstersMode.IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookLevelGenerationSpawnCorridors(LevelGeneration levelGeneration)
            {
                //Debug.Log("Spawn corridors is being hooked");
                int surroundingNodes = 0;
                bool oppositeDoors = false;
                for (int i = 0; i < levelGeneration.CorridorNodes.Count; i++)
                {
                    for (int j = 0; j < levelGeneration.CorridorNodes[i].Count; j++)
                    {
                        surroundingNodes = OrientCorridorLG.DetermineSurroundingNodes(levelGeneration.CorridorNodes[i][j]);
                        oppositeDoors = false;
                        if (surroundingNodes == 2)
                        {
                            oppositeDoors = OrientCorridorLG.StraightOrCorner(levelGeneration.CorridorNodes[i][j]);
                        }
                        switch (surroundingNodes)
                        {
                            case 0:
                                Debug.LogError("ERROR! NO SURROUNDING NODES, CAPTAIN!");
                                break;
                            case 1:
                                SpawnCorridorsLG.SpawnCorridor(levelGeneration.corridorsInUse, i, j, levelGeneration.corridorEndPieces, levelGeneration.corridorParent, surroundingNodes);
                                break;
                            case 2:
                                if (oppositeDoors)
                                {
                                    SpawnCorridorsLG.SpawnCorridor(levelGeneration.corridorsInUse, i, j, levelGeneration.corridorStraights, levelGeneration.corridorParent, surroundingNodes);
                                }
                                else
                                {
                                    SpawnCorridorsLG.SpawnCorridor(levelGeneration.corridorsInUse, i, j, levelGeneration.corridorCorners, levelGeneration.corridorParent, surroundingNodes);
                                }
                                break;
                            case 3:
                                SpawnCorridorsLG.SpawnCorridor(levelGeneration.corridorsInUse, i, j, levelGeneration.corridorTJunctions, levelGeneration.corridorParent, surroundingNodes);
                                break;
                            case 4:
                                SpawnCorridorsLG.SpawnCorridor(levelGeneration.corridorsInUse, i, j, levelGeneration.corridor4Ways, levelGeneration.corridorParent, surroundingNodes);
                                break;
                        }
                        if (levelGeneration.DoBreak())
                        {
                            yield return null;
                        }
                    }
                }
                busyWithCoroutine = false;
                yield break;
            }

            public static bool HookLevelGenerationSpawnJointsAndDoorsIntermediateHook(IEnumerator self)
            {
                IEnumerator replacement;
                if (!ManyMonstersMode.IEnumeratorDictionary.TryGetValue(self, out replacement))
                {
                    replacement = HookLevelGenerationSpawnJointsAndDoors((LevelGeneration)self.GetType().GetField("$this", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(self));
                    ManyMonstersMode.IEnumeratorDictionary[self] = replacement;
                }
                return replacement.MoveNext();
            }

            private static IEnumerator HookLevelGenerationSpawnJointsAndDoors(LevelGeneration levelGeneration)
            {
                //Debug.Log("Joints and doors is being hooked");
                for (int i = 0; i < levelGeneration.doorConnections.Count; i++)
                {
                    if (levelGeneration.doorConnections[i].spawnJoint)
                    {
                        SpawnJointsLG.SpawnJoint(levelGeneration.jointsInUse, levelGeneration.jointPrefabs, levelGeneration.jointParent, levelGeneration.doorConnections[i], levelGeneration.navMeshes, i);
                        if (levelGeneration.DoBreak())
                        {
                            yield return null;
                        }
                    }
                }
                SpawnDoorsLG.GenerateDoors(levelGeneration.doorConnections, levelGeneration.doorPrefabs, levelGeneration.doorParent, levelGeneration.doorsInUse);
                busyWithCoroutine = false;
                yield break;
            }

            private static IEnumerator WaitUntilGenerationIsFinished()
            {
                while (!LevelGeneration.Instance.finishedGenerating)
                {
                    yield return null;
                }
                ModSettings.ReadAfterGeneration();
                yield break;
            }

            /* All Region Names:
            Inaccessible
            Crew Deck
            Walkways
            LifeRaftEscape
            Bridge
            Stairs
            Shell
            Deck
            Ladder Region (Temp)
            Upper Deck
            Lower Deck
            Cargo_Catwalks
            Cargo_Containers
            Cargo_MainHold
            Cargo_Stairs
            Sub Escape
            StartRoom
            LockedRooms - (Randomly?) designated areas where power locked rooms may appear.
            Engines_MainArea
            Engines_Catwalks
            Engines_MainEngine
            Engines_SingleFloorMachines
            Engines_MultiFloorMachines
            Engines_WalledRooms
            Engines_Stairs
            Engines_ControlRoom
            Lwd_StartRoom
            Deck_CargoContainers
            Deck_CargoWalkways
            Deck_CargoTowers
            Deck_CargoBowStairs
            HeliEscape
            Deck_MainCargoArea
            Deck_Bow
            Deck_ContainerBase
            Deck_HoldCover
            Deck_SingleContainers
            Deck_HeliContainers
            Deck_Stern
            Deck_BridgeWalkways
            
            Region Information Text:
            Region with index 0 has name Inaccessible and ID 0
            Region with index 1 has name Crew Deck and ID 53
            Region with index 2 has name Walkways and ID 59
            Region with index 3 has name LifeRaftEscape and ID 79
            Region with index 4 has name Bridge and ID 80
            Region with index 5 has name Stairs and ID 94
            Region with index 6 has name Shell and ID 95
            Region with index 7 has name Deck and ID 96
            Region with index 8 has name Ladder Region (Temp) and ID 97
            Region with index 9 has name Upper Deck and ID 98
            Region with index 10 has name Lower Deck and ID 100
            Region with index 11 has name Cargo_Catwalks and ID 112
            Region with index 12 has name Cargo_Containers and ID 113
            Region with index 13 has name Cargo_MainHold and ID 114
            Region with index 14 has name Cargo_Stairs and ID 115
            Region with index 15 has name Sub Escape and ID 116
            Region with index 16 has name StartRoom and ID 117
            Region with index 17 has name LockedRooms and ID 118
            Region with index 18 has name Engines_MainArea and ID 119
            Region with index 19 has name Engines_Catwalks and ID 120
            Region with index 20 has name Engines_MainEngine and ID 121
            Region with index 21 has name Engines_SingleFloorMachines and ID 122
            Region with index 22 has name Engines_MultiFloorMachines and ID 124
            Region with index 23 has name Engines_WalledRooms and ID 125
            Region with index 24 has name Engines_Stairs and ID 126
            Region with index 25 has name Engines_ControlRoom and ID 127
            Region with index 26 has name Lwd_StartRoom and ID 128
            Region with index 27 has name Deck_CargoContainers and ID 129
            Region with index 28 has name Deck_CargoWalkways and ID 130
            Region with index 29 has name Deck_CargoTowers and ID 131
            Region with index 30 has name Deck_CargoBowStairs and ID 132
            Region with index 31 has name HeliEscape and ID 133
            Region with index 32 has name Deck_MainCargoArea and ID 134
            Region with index 33 has name Deck_Bow and ID 135
            Region with index 34 has name Deck_ContainerBase and ID 136
            Region with index 35 has name Deck_HoldCover and ID 137
            Region with index 36 has name Deck_SingleContainers and ID 140
            Region with index 37 has name Deck_HeliContainers and ID 141
            Region with index 38 has name Deck_Stern and ID 142
            Region with index 39 has name Deck_BridgeWalkways and ID 143
            */

            private static void ListRegions()
            {
                for (int j = 0; j < RegionManager.Instance.regions.Count; j++)
                {
                    RegionEntry regionEntry = RegionManager.Instance.regions[j];
                    Debug.Log("Region name is: " + regionEntry.regionName + ". This has associated nodes:");
                    foreach (Vector3 vector in regionEntry.associatedNodes)
                    {
                        Debug.Log(vector);
                    }
                }
            }

            private static void RegionPreviewTest(string regionName, int yLock = -1)
            {
                for (int j = 0; j < RegionManager.Instance.regions.Count; j++)
                {
                    RegionEntry regionEntry = RegionManager.Instance.regions[j];
                    if (regionEntry.regionName.Equals("Inaccessible"))
                    {
                        RegionEntry holdCover = RegionManager.Instance.regions[RegionManager.Instance.StringToRegionIndex(regionName)];
                        for (int i = 0; i < regionEntry.associatedNodes.Count; i++)
                        {
                            Vector3 vector = regionEntry.associatedNodes[i];
                            bool editVector = false;
                            if (yLock != -1)
                            {
                                if (vector.y == yLock)
                                {
                                    editVector = true;
                                }
                            }
                            else
                            {
                                editVector = true;
                            }

                            if (editVector)
                            {
                                holdCover.associatedNodes.Add(vector);
                                regionEntry.associatedNodes.Remove(vector);
                                if (!RegionManager.Instance.regionData[(int)vector.x].regionDataY[(int)vector.y].regionDataZ[(int)vector.z].regionID.Contains(holdCover.regionID))
                                {
                                    //RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID.Add(regionID);
                                    RegionManager.Instance.regionData[(int)vector.x].regionDataY[(int)vector.y].regionDataZ[(int)vector.z].regionID[0] = holdCover.regionID;
                                }
                            }
                        }
                    }
                }
            }

            private static void RegionAddTest(string regionKeyWord, string regionName)
            {
                for (int j = 0; j < RegionManager.Instance.regions.Count; j++)
                {
                    RegionEntry regionEntry = RegionManager.Instance.regions[j];
                    if (regionEntry.regionName.Contains(regionKeyWord) && !regionEntry.regionName.Equals(regionName))
                    {
                        RegionEntry holdCover = RegionManager.Instance.regions[RegionManager.Instance.StringToRegionIndex(regionName)];
                        for (int i = 0; i < regionEntry.associatedNodes.Count; i++)
                        {
                            Vector3 vector = regionEntry.associatedNodes[i];
                            holdCover.associatedNodes.Add(vector);
                            if (!RegionManager.Instance.regionData[(int)vector.x].regionDataY[(int)vector.y].regionDataZ[(int)vector.z].regionID.Contains(holdCover.regionID))
                            {
                                RegionManager.Instance.regionData[(int)vector.x].regionDataY[(int)vector.y].regionDataZ[(int)vector.z].regionID.Add(holdCover.regionID);
                            }
                        }
                    }
                }
            }

            private static void RegionIDAddTest(string regionKeyWord, string regionName)
            {
                for (int j = 0; j < RegionManager.Instance.regions.Count; j++)
                {
                    RegionEntry regionEntry = RegionManager.Instance.regions[j];
                    if (regionEntry.regionName.Contains(regionKeyWord) && !regionEntry.regionName.Equals(regionName))
                    {
                        RegionEntry holdCover = RegionManager.Instance.regions[RegionManager.Instance.StringToRegionIndex(regionName)];
                        for (int i = 0; i < regionEntry.associatedNodes.Count; i++)
                        {
                            Vector3 vector = regionEntry.associatedNodes[i];
                            //holdCover.associatedNodes.Add(vector);
                            if (!RegionManager.Instance.regionData[(int)vector.x].regionDataY[(int)vector.y].regionDataZ[(int)vector.z].regionID.Contains(holdCover.regionID))
                            {
                                RegionManager.Instance.regionData[(int)vector.x].regionDataY[(int)vector.y].regionDataZ[(int)vector.z].regionID.Add(holdCover.regionID);
                            }
                        }
                    }
                }
            }

            // ~ Main menu skip menu screen text not working anymore...? (Maybe only when dnSpy compiled the main dll) Does doing multiple rounds not work anymore in MMM?
            private static void HelicopterCargoExtension()
            {
                //CopyRegionOverYUsingReferenceNode(24, 4, 2, 24, 35, 4, 7, 2, 13);
                CopyRegionOverYUsingReferenceNode(24, 4, 2, 27, 35, 5, 5, 3, 12);
                //CopyRegionOverYUsingReferenceNode(23, 4, 2, 36, 36, 6, 6, 4, 11);
                //RegionAddTest("Tower", "Cargo_Catwalks");
                //RegionAddTest("Tower", "Cargo_MainHold");
                //RegionIDAddTest("Tower", "Cargo_Catwalks");

                // Edit SpawnCargoContainersLG (Currently edited in dnSpy)


                //SetRegionInRange("Cargo_Catwalks", 36, 36, 5, 5, 3, 12, true);
                //SetRegionInRange("Cargo_MainHold", 36, 36, 5, 5, 3, 12);
            }

            private static void LevelGenerationChanges()
            {
                //RegionPreviewTest("Deck_HoldCover");
                //RegionPreviewTest("Deck_CargoBowStairs", 5);
                //RegionPreviewTest("Deck_CargoBowStairs", 4);
                //RegionAddTest("Cargo", "Engines_Stairs");

                //HelicopterCargoExtension();

                Settings.stairsPerLevel = 4; // Reset stairs per level as the count is kept across multiple rounds.
                customBottomStairLevelList = new List<int>();
                string[] coreRegions = { "Crew Deck", "Upper Deck", "Lower Deck" };
                string[] convertibleRegions = { "Inaccessible"/*, "Deck_CargoContainers", "Deck_CargoWalkways", "Deck_MainCargoArea", "Deck_SingleContainers", "Deck_Bow", "Deck_ContainerBase", "Deck_HoldCover", "Deck_Stern", "Deck_BridgeWalkways", "Engines_MainArea", "Engines_Catwalks", "Engines_MainEngine", "Engines_SingleFloorMachines", "Engines_MultiFloorMachines", "Engines_WalledRooms", "Engines_Stairs", "Engines_ControlRoom", "Shell", "Deck", "Bridge", "Walkways"*/ };

                if (ModSettings.experimentalShipExtension && ModSettings.increaseMapSizeVector.z < 16f)
                {
                    ModSettings.increaseMapSizeVector = new Vector3(ModSettings.increaseMapSizeVector.x, ModSettings.increaseMapSizeVector.y, 32f);
                }

                if (ModSettings.useDeckFourOnSubmersibleSide)
                {
                    // The section below tries to let deck 4 of the lower deck side be used like deck 3.
                    CopyRegionOverY(49, (int)Settings.ShipCubesCount.x - 1, 3, 4, 0, (int)Settings.ShipCubesCount.z - 1);
                    IncreaseRoomCounts(false, PrimaryRegionType.LowerDeck, 4, 6);
                }

                if (ModSettings.extendLowerDecks)
                {
                    int maxY = 3;
                    if (ModSettings.useDeckFourOnSubmersibleSide)
                    {
                        maxY = 4;
                    }
                    SetRegionInRange("Lower Deck", 49, (int)Settings.ShipCubesCount.x - 1, 1, maxY, 1, 14); // This has a similar effect to the extend map one, but only affects the lower decks on the submersible side and has much faster loading times.
                    SetRegionInRange("Lower Deck", 16, 20, 3, 3, 1, 14); // Deck 3 Region Definition
                    IncreaseRoomCounts(false, PrimaryRegionType.LowerDeck, 2, 3);
                }

                if (ModSettings.numberOfCorridorsToCargoHoldFromDeckThree > 0)
                {
                    int minX = 47; // Short Version
                    if (ModSettings.lengthenedCargoHoldCorridors)
                    {
                        minX = 36; // Long Version
                    }
                    int maxX = 61;

                    if (ModSettings.numberOfCorridorsToCargoHoldFromDeckThree == 2)
                    {
                        CopyRegionOverY(minX, maxX, 2, 3, 1, 1); // Sub Side Near Corridor Deck 3
                        CopyRegionOverY(minX, maxX, 2, 3, 14, 14); // Sub Side Far Corridor Deck 3
                        if (ModSettings.useDeckFourOnSubmersibleSide)
                        {
                            CopyRegionOverY(minX, maxX, 2, 4, 1, 1); // Sub Side Near Corridor Deck 4
                            CopyRegionOverY(minX, maxX, 2, 4, 14, 14); // Sub Side Far Corridor Deck 4
                        }
                    }
                    else
                    {
                        float randomChance = UnityEngine.Random.value;
                        if (randomChance > 0.5f)
                        {
                            CopyRegionOverY(minX, maxX, 2, 3, 1, 1); // Sub Side Near Corridor Deck 3
                            if (ModSettings.useDeckFourOnSubmersibleSide)
                            {
                                CopyRegionOverY(minX, maxX, 2, 4, 14, 14); // Sub Side Far Corridor Deck 4
                            }
                        }
                        else
                        {
                            CopyRegionOverY(minX, maxX, 2, 3, 14, 14); // Sub Side Far Corridor Deck 3
                            if (ModSettings.useDeckFourOnSubmersibleSide)
                            {
                                CopyRegionOverY(minX, maxX, 2, 4, 1, 1); // Sub Side Near Corridor Deck 4
                            }
                        }
                    }
                }

                if (ModSettings.addLowerDeckNextToEngineRoom)
                {
                    // ----- Side Connections To Cargo Rooms -----
                    int minX = 16;
                    int maxX = 23; // Short Version
                    if (ModSettings.lengthenedCargoHoldCorridors)
                    {
                        maxX = 37; // Long Version
                    }

                    if (ModSettings.numberOfCorridorsToCargoHoldFromNewLowerDeck == 2)
                    {
                        CopyRegionOverY(minX, maxX, 4, 1, 1, 1); // Engine Side Near Corridor Deck 1
                        CopyRegionOverY(minX, maxX, 4, 1, 14, 14); // Engine Side Far Corridor Deck 1
                        CopyRegionOverY(minX, maxX, 4, 2, 1, 1); // Engine Side Near Corridor Deck 2
                        CopyRegionOverY(minX, maxX, 4, 2, 14, 14); // Engine Side Far Corridor Deck 2
                    }
                    else if (ModSettings.numberOfCorridorsToCargoHoldFromNewLowerDeck == 1)
                    {
                        float randomChance = UnityEngine.Random.value;
                        if (randomChance > 0.5f)
                        {
                            CopyRegionOverY(minX, maxX, 4, 1, 1, 1); // Engine Side Near Corridor Deck 1
                            CopyRegionOverY(minX, maxX, 4, 2, 14, 14); // Engine Side Far Corridor Deck 2
                        }
                        else
                        {
                            CopyRegionOverY(minX, maxX, 4, 1, 14, 14); // Engine Side Far Corridor Deck 1
                            CopyRegionOverY(minX, maxX, 4, 2, 1, 1); // Engine Side Near Corridor Deck 2
                        }
                    }
                    //CopyRegionOverY(minX, maxX, 4, 1, 1, 14); // Engine Side Near To Far Corridor Deck 1 // This is not necessary if the stairs are placed as those are connected naturally by the code.
                    //CopyRegionOverY(minX, maxX, 4, 2, 1, 14); // Engine Side Near To Far Corridor Deck 2 // This is not necessary if the stairs are placed as those are connected naturally by the code.


                    // ----- Deck Extensions -----

                    CopyRegionOverYUsingReferenceNode(60, 1, 3, 14, 20, 1, 1, 0, 15); // Engine Side Deck 1 Rooms
                    CopyRegionOverYUsingReferenceNode(60, 1, 3, 10, 20, 2, 2, 0, 15); // Engine Side Deck 2 Rooms
                    // Region definitions are not required when copying using a reference node.
                    //SetRegionInRange("Lower Deck", 14, 20, 1, 1, 1, 14); // Deck 1 Region Definition
                    //SetRegionInRange("Lower Deck", 10, 20, 2, 2, 1, 14); // Deck 2 Region Definition

                    // ----- Bonus Stairs -----
                    // Sub side stair test
                    //CopyRegionOverYUsingReferenceNode(64, 1, 2, 64, 66, 1, 4, 4, 5); // Stairs test 
                    //CopyRegionOverYUsingReferenceNode(64, 5, 2, 64, 66, 5, 5, 4, 5); // Stairs test // copyZ = 1 is not the same as copyZ = 2 here because of cargo walkways!
                    //Settings.stairsPerLevel = 5;

                    // Engine side stair test
                    //CopyRegionOverYUsingReferenceNode(13, 3, 4, 13, 15, 3, 4, 7, 8); // Stairs test 
                    //CopyRegionOverYUsingReferenceNode(13, 5, 4, 13, 15, 5, 6, 7, 8); // Stairs test 
                    //CopyRegionOverYUsingReferenceNode(13, 7, 4, 13, 15, 7, 8, 7, 8); // Stairs test 
                    //Settings.stairsPerLevel = 5;

                    int bottomStairLevel = 1;
                    if (ModSettings.addDeckZero)
                    {
                        bottomStairLevel = 0;
                    }
                    int topStairLevel = 2;

                    // Set 1 (Traditional Lineup)
                    CopyRegionOverYUsingReferenceNode(13, 3, 4, 11, 13, bottomStairLevel, topStairLevel, 0, 1);
                    CopyRegionOverYUsingReferenceNode(13, 3, 4, 11, 13, bottomStairLevel, topStairLevel, 14, 15);

                    // Set 2 (Rotated Lineup)
                    //CopyRegionOverYUsingReferenceNode(13, 3, 4, 11, 13, bottomStairLevel, topStairLevel, 7, 8);
                    //CopyRegionOverYUsingReferenceNode(13, 8, 4, 17, 19, topStairLevel, topStairLevel, 7, 8);

                    customBottomStairLevelList.Add(bottomStairLevel);
                    Settings.stairsPerLevel++;
                    customBottomStairLevelList.Add(bottomStairLevel);
                    Settings.stairsPerLevel++;

                    //CopyRegionOverYUsingReferenceNode(1, 1, 3, 18, 19, 1, 2, 7, 7); // Engine room stairs copy test

                    // ----- Engine room access route ----- // Doesn't seem to encourage doors to spawn. I think the room count may actually do this. It is still inconsistent though. Maybe that is not actually the link.
                    //CopyRegionOverYUsingReferenceNode(9, 2, 9, 12, 13, 1, 1, 9, 9); // Not required as there is already a cleared area here.
                    if (ModSettings.addDeckZero)
                    {
                        CopyRegionOverYUsingReferenceNode(9, 0, 7, 12, 13, 0, 0, 6, 8);
                    }

                    IncreaseRoomCounts(false, PrimaryRegionType.LowerDeck, 6, 8);
                }

                if (ModSettings.addDeckZero)
                {
                    CopyRegionOverYUsingReferenceNode(60, 1, 3, 14, 65, 0, 0, 0, (int)Settings.ShipCubesCount.z - 1); // Engine Side Deck 0 Rooms
                    //SetRegionInRange("Lower Deck", 14, 65, 0, 0, 0, (int)Settings.ShipCubesCount.z - 1); // Deck 0 Region Definition

                    // Do not let stuff spawn under the sub bay.
                    CopyRegionOverYUsingReferenceNode(0, 0, 0, 60, 63, 0, 0, 7, 8);

                    // Add in inaccessible zone to spawn the corridors in zig-zags.
                    if (ModSettings.increaseMapSizeVector.z < 5f)
                    {
                        float randomChance1 = UnityEngine.Random.value;
                        if (randomChance1 > 0.5f)
                        {
                            CopyRegionOverYUsingReferenceNode(0, 0, 0, (int)Settings.ShipCubesCount.x / 3, (int)Settings.ShipCubesCount.x / 3, 0, 0, 0, (int)Settings.ShipCubesCount.z - 4);
                        }
                        else
                        {
                            CopyRegionOverYUsingReferenceNode(0, 0, 0, (int)Settings.ShipCubesCount.x / 3, (int)Settings.ShipCubesCount.x / 3, 0, 0, 3, (int)Settings.ShipCubesCount.z - 1);
                        }

                        float randomChance2 = UnityEngine.Random.value;
                        if (randomChance2 > 0.5f)
                        {
                            CopyRegionOverYUsingReferenceNode(0, 0, 0, 2 * (int)Settings.ShipCubesCount.x / 3, 2 * (int)Settings.ShipCubesCount.x / 3, 0, 0, 3, (int)Settings.ShipCubesCount.z - 1);
                        }
                        else
                        {
                            CopyRegionOverYUsingReferenceNode(0, 0, 0, 2 * (int)Settings.ShipCubesCount.x / 3, 2 * (int)Settings.ShipCubesCount.x / 3, 0, 0, 0, (int)Settings.ShipCubesCount.z - 4);
                        }

                        CopyRegionOverYUsingReferenceNode(0, 0, 0, (int)Settings.ShipCubesCount.x / 2, (int)Settings.ShipCubesCount.x / 2, 0, 0, 0, ((int)Settings.ShipCubesCount.z / 2) - 2);
                        CopyRegionOverYUsingReferenceNode(0, 0, 0, (int)Settings.ShipCubesCount.x / 2, (int)Settings.ShipCubesCount.x / 2, 0, 0, ((int)Settings.ShipCubesCount.z / 2) + 2, (int)Settings.ShipCubesCount.z - 1);
                    }
                    else
                    {
                        CopyRegionOverYUsingReferenceNode(0, 0, 0, (int)Settings.ShipCubesCount.x / 3, (int)Settings.ShipCubesCount.x / 3, 0, 0, 3, (int)Settings.ShipCubesCount.z - 4);
                        CopyRegionOverYUsingReferenceNode(0, 0, 0, 2 * (int)Settings.ShipCubesCount.x / 3, 2 * (int)Settings.ShipCubesCount.x / 3, 0, 0, 3, (int)Settings.ShipCubesCount.z - 4);

                        CopyRegionOverYUsingReferenceNode(0, 0, 0, (int)Settings.ShipCubesCount.x / 2, (int)Settings.ShipCubesCount.x / 2, 0, 0, 3, ((int)Settings.ShipCubesCount.z / 2) - 2);
                        CopyRegionOverYUsingReferenceNode(0, 0, 0, (int)Settings.ShipCubesCount.x / 2, (int)Settings.ShipCubesCount.x / 2, 0, 0, ((int)Settings.ShipCubesCount.z / 2) + 2, (int)Settings.ShipCubesCount.z - 4);
                    }

                    /*
                    CopyRegionOverYUsingReferenceNode(0, 0, 0, (int)Settings.ShipCubesCount.x / 5, (int)Settings.ShipCubesCount.x / 5, 0, 0, 3, (int)Settings.ShipCubesCount.z - 1);
                    CopyRegionOverYUsingReferenceNode(0, 0, 0, 2 * (int)Settings.ShipCubesCount.x / 5, 2 * (int)Settings.ShipCubesCount.x / 5, 0, 0, 0, (int)Settings.ShipCubesCount.z - 4);
                    CopyRegionOverYUsingReferenceNode(0, 0, 0, 3 * (int)Settings.ShipCubesCount.x / 5, 3 * (int)Settings.ShipCubesCount.x / 5, 0, 0, 3, (int)Settings.ShipCubesCount.z - 1);
                    CopyRegionOverYUsingReferenceNode(0, 0, 0, 4 * (int)Settings.ShipCubesCount.x / 5, 4 * (int)Settings.ShipCubesCount.x / 5, 0, 0, 0, (int)Settings.ShipCubesCount.z - 4);
                    */

                    // Add an extra set of stairs

                    int bottomStairLevelDeckZero = 0;
                    int topStairLevelDeckZero = 3;

                    if (ModSettings.useDeckFourOnSubmersibleSide)
                    {
                        topStairLevelDeckZero = 4;
                    }

                    int stairStartPoint = 7;

                    if (!ModSettings.addLowerDeckNextToEngineRoom)
                    {
                        stairStartPoint = 5;
                        int stairStartPoint2 = 9;
                        CopyRegionOverYUsingReferenceNode(13, 3, 4, 51, 53, bottomStairLevelDeckZero, topStairLevelDeckZero, stairStartPoint2, stairStartPoint2 + 1);
                        CopyRegionOverYUsingReferenceNode(0, 0, 0, 51, 54, bottomStairLevelDeckZero + 1, topStairLevelDeckZero - 1, stairStartPoint2 - 1, stairStartPoint2 - 1);
                        CopyRegionOverYUsingReferenceNode(0, 0, 0, 54, 54, bottomStairLevelDeckZero + 1, topStairLevelDeckZero - 1, stairStartPoint2, stairStartPoint2 + 1);
                        CopyRegionOverYUsingReferenceNode(0, 0, 0, 51, 54, bottomStairLevelDeckZero + 1, topStairLevelDeckZero - 1, stairStartPoint2 + 2, stairStartPoint2 + 2);
                        customBottomStairLevelList.Add(bottomStairLevelDeckZero);
                        Settings.stairsPerLevel++;
                    }


                    CopyRegionOverYUsingReferenceNode(13, 3, 4, 51, 53, bottomStairLevelDeckZero, topStairLevelDeckZero, stairStartPoint, stairStartPoint + 1);
                    CopyRegionOverYUsingReferenceNode(0, 0, 0, 51, 54, bottomStairLevelDeckZero + 1, topStairLevelDeckZero - 1, stairStartPoint - 1, stairStartPoint - 1);
                    CopyRegionOverYUsingReferenceNode(0, 0, 0, 54, 54, bottomStairLevelDeckZero + 1, topStairLevelDeckZero - 1, stairStartPoint, stairStartPoint + 1);
                    CopyRegionOverYUsingReferenceNode(0, 0, 0, 51, 54, bottomStairLevelDeckZero + 1, topStairLevelDeckZero - 1, stairStartPoint + 2, stairStartPoint + 2);
                    customBottomStairLevelList.Add(bottomStairLevelDeckZero);
                    Settings.stairsPerLevel++;

                    IncreaseRoomCounts(false, PrimaryRegionType.LowerDeck, 10, 15);
                }

                if (ModSettings.addAdditionalCrewDeckBuilding)
                {
                    CopyRegionOverYUsingReferenceNode(21, 5, 2, 30, 31, 5, 5, 2, 13); // Copy Cargo To Crew Deck Separation Part 1
                    CopyRegionOverYUsingReferenceNode(23, 5, 2, 32, 32, 5, 5, 2, 13); // Copy Cargo To Crew Deck Separation Part 2
                    CopyRegionOverYUsingReferenceNode(0, 0, 0, 23, 32, 6, 7, 0, 15); // Clear Air

                    CopyRegionOverYUsingReferenceNode(21, 5, 1, 23, 31, 5, 5, 1, 1); // Set Walkway Near
                    CopyRegionOverYUsingReferenceNode(21, 5, 1, 23, 31, 5, 5, 14, 14); // Set Walkway Far
                    CopyRegionOverYUsingReferenceNode(21, 5, 2, 23, 31, 5, 5, 2, 13); // Set Walkway Centre

                    int crewDeckExtensionLeftPoint = 23;
                    int crewDeckExtensionRightPoint = 30;
                    int crewDeckExtensionNearPoint = 3;
                    int crewDeckExtensionFarPoint = 12;

                    CopyRegionOverYUsingReferenceNode(16, 5, 4, crewDeckExtensionLeftPoint + 1, crewDeckExtensionRightPoint - 1, 5, 6, crewDeckExtensionNearPoint + 1, crewDeckExtensionFarPoint - 1); // Crew Deck Extension
                    CopyRegionOverYUsingReferenceNode(16, 5, 3, crewDeckExtensionLeftPoint, crewDeckExtensionRightPoint, 5, 6, crewDeckExtensionNearPoint, crewDeckExtensionNearPoint); // Crew Deck Shell Extension Near
                    CopyRegionOverYUsingReferenceNode(16, 5, 3, crewDeckExtensionLeftPoint, crewDeckExtensionRightPoint, 5, 6, crewDeckExtensionFarPoint, crewDeckExtensionFarPoint); // Crew Deck Shell Extension Far
                    CopyRegionOverYUsingReferenceNode(16, 5, 3, crewDeckExtensionLeftPoint, crewDeckExtensionLeftPoint, 5, 6, crewDeckExtensionNearPoint, crewDeckExtensionFarPoint); // Crew Deck Shell Extension Across Left
                    CopyRegionOverYUsingReferenceNode(16, 5, 3, crewDeckExtensionRightPoint, crewDeckExtensionRightPoint, 5, 6, crewDeckExtensionNearPoint, crewDeckExtensionFarPoint); // Crew Deck Shell Extension Across Right


                    CopyRegionOverYUsingReferenceNode(16, 7, 10, crewDeckExtensionLeftPoint + 1, crewDeckExtensionRightPoint - 1, 7, 7, crewDeckExtensionNearPoint + 2, crewDeckExtensionFarPoint - 1); // Upper Deck Extension
                    CopyRegionOverYUsingReferenceNode(16, 7, 11, crewDeckExtensionLeftPoint, crewDeckExtensionRightPoint, 7, 7, crewDeckExtensionNearPoint + 1, crewDeckExtensionNearPoint + 1); // Upper Deck Shell Extension Near
                    CopyRegionOverYUsingReferenceNode(16, 7, 11, crewDeckExtensionLeftPoint, crewDeckExtensionRightPoint, 7, 7, crewDeckExtensionFarPoint, crewDeckExtensionFarPoint); // Upper Deck Shell Extension Far
                    CopyRegionOverYUsingReferenceNode(16, 7, 11, crewDeckExtensionLeftPoint, crewDeckExtensionLeftPoint, 7, 7, 4, crewDeckExtensionFarPoint); // Upper Deck Shell Extension Across Left
                    CopyRegionOverYUsingReferenceNode(16, 7, 11, crewDeckExtensionRightPoint, crewDeckExtensionRightPoint, 7, 7, 4, crewDeckExtensionFarPoint); // Upper Deck Shell Extension Across Right
                    CopyRegionOverYUsingReferenceNode(16, 7, 12, crewDeckExtensionLeftPoint, crewDeckExtensionRightPoint, 7, 7, crewDeckExtensionNearPoint, crewDeckExtensionNearPoint); // Outer Deck Walkway Near

                    CopyRegionOverYUsingReferenceNode(12, 7, 3, crewDeckExtensionLeftPoint, crewDeckExtensionRightPoint, 8, 8, crewDeckExtensionNearPoint + 1, crewDeckExtensionFarPoint); // Outer Deck To Cover Upper Deck Extension

                    CopyRegionOverYUsingReferenceNode(6, 7, 11, crewDeckExtensionRightPoint - 6, crewDeckExtensionRightPoint, 8, 8, crewDeckExtensionFarPoint - 2, crewDeckExtensionFarPoint); // Life Raft Area Duplicate
                    CopyRegionOverYUsingReferenceNode(9, 5, 14, crewDeckExtensionRightPoint - 3, crewDeckExtensionRightPoint - 1, 5, 5, 14, 14); // Life Raft Escape Area Duplicate Near
                    CopyRegionOverYUsingReferenceNode(9, 5, 15, crewDeckExtensionRightPoint - 3, crewDeckExtensionRightPoint - 1, 5, 5, 15, 15); // Life Raft Escape Area Duplicate Far

                    // Stairs - Sadly using only one set does not work.
                    int bottomStairLevel2 = 5;
                    int topStairLevel2 = 7;
                    CopyRegionOverYUsingReferenceNode(13, 5, 4, crewDeckExtensionLeftPoint + 1, crewDeckExtensionLeftPoint + 3, bottomStairLevel2, topStairLevel2, crewDeckExtensionNearPoint + 2, crewDeckExtensionNearPoint + 3);
                    CopyRegionOverYUsingReferenceNode(13, 5, 4, crewDeckExtensionLeftPoint + 1, crewDeckExtensionLeftPoint + 3, bottomStairLevel2, topStairLevel2, crewDeckExtensionFarPoint - 2, crewDeckExtensionFarPoint - 1);

                    customBottomStairLevelList.Add(bottomStairLevel2);
                    Settings.stairsPerLevel++;
                    customBottomStairLevelList.Add(bottomStairLevel2);
                    Settings.stairsPerLevel++;

                    // Parts 1 to 3 create the original shape, while parts 4 and 5 add the custom full-length side shape.
                    CopyRegionOverYUsingReferenceNode(18, 7, 3, crewDeckExtensionRightPoint - 2, crewDeckExtensionRightPoint, 7, 7, 3, 3); // Extra Bridge Walkway Near Part 1
                    CopyRegionOverYUsingReferenceNode(18, 7, 3, crewDeckExtensionRightPoint - 3, crewDeckExtensionRightPoint, 8, 8, 2, 4); // Extra Bridge Walkway Near Part 2
                    CopyRegionOverYUsingReferenceNode(18, 7, 3, crewDeckExtensionRightPoint, crewDeckExtensionRightPoint, 8, 8, 1, 1); // Extra Bridge Walkway Near Part 3
                    //CopyRegionOverYUsingReferenceNode(18, 7, 3, crewDeckExtensionRightPoint, crewDeckExtensionRightPoint, 8, 8, 4, 4); // Extra Bridge Walkway Near Part 4
                    CopyRegionOverYUsingReferenceNode(18, 7, 3, crewDeckExtensionLeftPoint, crewDeckExtensionRightPoint - 4, 8, 8, 2, 4); // Extra Bridge Walkway Near Part 5 (Stopping light from blocking door)
                    CopyRegionOverYUsingReferenceNode(18, 7, 3, crewDeckExtensionLeftPoint, crewDeckExtensionLeftPoint, 8, 8, 1, 1); // Extra Bridge Walkway Near Part 6 (Polishing part 5)

                    //CopyRegionOverYUsingReferenceNode(18, 7, 3, 21, crewDeckExtensionLeftPoint - 1, 8, 8, 2, 3); // Optional Bridge Walkway Connecting Bridge To New Building // Unused because it doesn't generate well.


                    //CopyRegionOverYUsingReferenceNode(18, 7, 3, crewDeckExtensionRightPoint - 2, crewDeckExtensionRightPoint, 7, 7, 12, 12); // Extra Bridge Walkway Far Part 1
                    //CopyRegionOverYUsingReferenceNode(18, 7, 3, crewDeckExtensionRightPoint - 3, crewDeckExtensionRightPoint, 8, 8, 11, 13); // Extra Bridge Walkway Far Part 2
                    //CopyRegionOverYUsingReferenceNode(18, 7, 3, crewDeckExtensionRightPoint, crewDeckExtensionRightPoint, 8, 8, 14, 14); // Extra Bridge Walkway Far Part 3
                    //CopyRegionOverYUsingReferenceNode(18, 7, 3, crewDeckExtensionRightPoint, crewDeckExtensionRightPoint, 8, 8, 11, 11); // Extra Bridge Walkway Near Part 4

                    // Error to keep in mind in future [Should have been fixed now]
                    /* This error sometimes occurs due to the new extension. Is it the pole spawning on the outer deck? Not sure. Might have seen it there before. Might instead happen when stairs can't spawn next to bridge...? Seems to be related to nodes, so probably not. Definitely not the bridge issue as an occurrence has happened where the game didn't stop loading. Does the error occur due to stairs being set to inaccessible through incorrect copying?
    ArgumentOutOfRangeException: Argument is out of range.
    Parameter name: index
    System.Collections.Generic.List`1[NodeData].get_Item (Int32 index)
    NoCullingAppendage.CheckNoCullingJoints (.Room _room)
    Room.HandleAppendageData (.Room _room)
    SpawnCorridorsLG.SpawnCorridor (System.Collections.Generic.List`1 _corridorsInUse, Int32 _indexI, Int32 _indexJ, System.Collections.Generic.List`1 _corridorTypePrefabs, UnityEngine.GameObject _corParent, Int32 _surroundingNodes)
    LevelGeneration+<SpawnCorridors>c__Iterator3.MoveNext ()
    UnityEngine.SetupCoroutine.InvokeMoveNext (IEnumerator enumerator, IntPtr returnValueAddress)
                    */

                    /*
                    Room[] rooms = FindObjectsOfType<Room>();
                    foreach (Room room in rooms)
                    {
                        if (RegionManager.Instance.IDToName(room.regionChoice).Equals("Deck_BridgeWalkways"))
                        {
                            Debug.Log("Found room for Deck_BridgeWalkways. Its name is " + room.name + ". Its minRoomCount is " + room.MinRoomCount + " and its maxRoomCount is " + room.MaxRoomCount + ".");
                        }
                        else
                        {
                            Debug.Log("Found room not for Deck_BridgeWalkways. Its name is " + room.name + ". Its minRoomCount is " + room.MinRoomCount + " and its maxRoomCount is " + room.MaxRoomCount + ".");
                        }
                    }
                    if (rooms.Length == 0)
                    {
                        Debug.Log("Could not find any rooms.");
                    }
                    */

                    /*
                    foreach (Room room in LevelGeneration.Instance.walkwaysInUse)
                    {
                        if (RegionManager.Instance.IDToName(room.regionChoice).Equals("Deck_BridgeWalkways"))
                        {
                            Debug.Log("Found room for Deck_BridgeWalkways. Its name is " + room.name + ". Its minRoomCount is " + room.MinRoomCount + " and its maxRoomCount is " + room.MaxRoomCount + ".");
                        }
                        else
                        {
                            Debug.Log("Found room not for Deck_BridgeWalkways. Its name is " + room.name + ". Its minRoomCount is " + room.MinRoomCount + " and its maxRoomCount is " + room.MaxRoomCount + ".");
                        }
                    }
                    if (LevelGeneration.Instance.walkwaysInUse.Count == 0)
                    {
                        Debug.Log("Could not find any walkways.");
                    }
                    */

                    foreach (Room room in LevelGeneration.Instance.walkwayPrefabs)
                    {
                        //Debug.Log("Found walkwayPrefab. Its name is " + room.name + ". Its minRoomCount is " + room.MinRoomCount + " and its maxRoomCount is " + room.MaxRoomCount + ".");
                        if (room.name.Equals("Deck_BridgeWalkway_Stairs"))
                        {
                            //Debug.Log("Found stairs. Increasing their room count.");
                            room.roomAmountMin++;
                            room.roomAmountMax++;
                            break;
                        }
                    }
                    if (LevelGeneration.Instance.walkwayPrefabs.Count == 0)
                    {
                        Debug.Log("Could not find any walkway prefabs.");
                    }

                    foreach (Room room in LevelGeneration.Instance.roomPrefabs)
                    {
                        //Debug.Log("Found roomPrefab. Its name is " + room.name + ". Its minRoomCount is " + room.MinRoomCount + " and its maxRoomCount is " + room.MaxRoomCount + ".");
                        if (room.name.Equals("Escape_Ext_LifeRaftRoom"))
                        {
                            //Debug.Log("Found life raft. Increasing its room count.");
                            room.roomAmountMin++;
                            room.roomAmountMax++;
                            break;
                        }
                    }
                    if (LevelGeneration.Instance.roomPrefabs.Count == 0)
                    {
                        Debug.Log("Could not find any room prefabs.");
                    }

                    foreach (Room room in LevelGeneration.Instance.deckPrefabs)
                    {
                        //Debug.Log("Found deckPrefab. Its name is " + room.name + ". Its minRoomCount is " + room.MinRoomCount + " and its maxRoomCount is " + room.MaxRoomCount + ".");
                        if (room.name.Equals("Ext_LadderEscape_Temp") || room.name.Equals("Room_Deck_Central_5x6") || room.name.Equals("Room_Deck_1x6_Side"))
                        {
                            //Debug.Log("Found " + room.name + ". Increasing its room count.");
                            room.roomAmountMin *= 2;
                            room.roomAmountMax *= 2;
                        }
                    }
                    if (LevelGeneration.Instance.deckPrefabs.Count == 0)
                    {
                        Debug.Log("Could not find any deck prefabs.");
                    }

                    /*
                    foreach (Room room in LevelGeneration.Instance.corridorPrefabs)
                    {
                        Debug.Log("Found corridorPrefab. Its name is " + room.name + ". Its minRoomCount is " + room.MinRoomCount + " and its maxRoomCount is " + room.MaxRoomCount + ".");


                    }
                    if (LevelGeneration.Instance.corridorPrefabs.Count == 0)
                    {
                        Debug.Log("Could not find any corridor prefabs.");
                    }
                    */

                    foreach (KeyItem keyItem in FindObjectOfType<KeyItemSystem>().keyItems)
                    {
                        if (keyItem.name.Equals("DuctTape") || keyItem.name.Equals("Spool (GoodCondition)"))
                        {
                            keyItem.minCount += 1;
                            keyItem.maxCount += 1;
                        }
                    }

                    IncreaseRoomCounts(false, PrimaryRegionType.CrewDeck, 4, 6);
                    IncreaseRoomCounts(false, PrimaryRegionType.UpperDeck, 2, 3);

                    // CopyRegionOverYUsingReferenceNode(23, 4, 2, 23, 23, 5, 5, 2, 2); // Cargo Hold Ceiling Cover Test
                }

                /*
                if (ModSettings.addAdditionalCrewDeckBuilding)
                {
                    // Failed as stairs cannot spawn here for some reason.
                    int crewDeckExtensionLeftPoint = 57;
                    int crewDeckExtensionRightPoint = 61;
                    int crewDeckExtensionNearPoint = 3;
                    int crewDeckExtensionFarPoint = 12;

                    CopyRegionOverYUsingReferenceNode(21, 5, 2, 56, 57, 5, 5, 2, 13); // Copy Cargo To Crew Deck Separation Part 1
                    CopyRegionOverYUsingReferenceNode(23, 5, 2, 55, 55, 5, 5, 2, 13); // Copy Cargo To Crew Deck Separation Part 2
                    CopyRegionOverYUsingReferenceNode(0, 0, 0, 55, 60, 6, 7, 0, 15); // Clear Air

                    CopyRegionOverYUsingReferenceNode(21, 5, 1, 56, 61, 5, 5, 1, 1); // Set Walkway Near
                    CopyRegionOverYUsingReferenceNode(21, 5, 1, 56, 61, 5, 5, 14, 14); // Set Walkway Far
                    CopyRegionOverYUsingReferenceNode(21, 5, 2, 56, 61, 5, 5, 2, 13); // Set Walkway Centre


                    //int bottomStairLevel2 = 5;
                    //int topStairLevel2 = 5;

                    CopyRegionOverYUsingReferenceNode(16, 5, 4, crewDeckExtensionLeftPoint + 1, crewDeckExtensionRightPoint - 1, 5, 6, crewDeckExtensionNearPoint + 1, crewDeckExtensionFarPoint - 1); // Crew Deck Extension
                    CopyRegionOverYUsingReferenceNode(16, 5, 3, crewDeckExtensionLeftPoint, crewDeckExtensionRightPoint, 5, 6, crewDeckExtensionNearPoint, crewDeckExtensionNearPoint); // Crew Deck Shell Extension Near
                    CopyRegionOverYUsingReferenceNode(16, 5, 3, crewDeckExtensionLeftPoint, crewDeckExtensionRightPoint, 5, 6, crewDeckExtensionFarPoint, crewDeckExtensionFarPoint); // Crew Deck Shell Extension Far
                    CopyRegionOverYUsingReferenceNode(16, 5, 3, crewDeckExtensionLeftPoint, crewDeckExtensionLeftPoint, 5, 6, crewDeckExtensionNearPoint, crewDeckExtensionFarPoint); // Crew Deck Shell Extension Across Left
                    CopyRegionOverYUsingReferenceNode(16, 5, 3, crewDeckExtensionRightPoint, crewDeckExtensionRightPoint, 5, 6, crewDeckExtensionNearPoint, crewDeckExtensionFarPoint); // Crew Deck Shell Extension Across Right


                    CopyRegionOverYUsingReferenceNode(16, 7, 10, crewDeckExtensionLeftPoint + 1, crewDeckExtensionRightPoint - 1, 7, topStairLevel2, crewDeckExtensionNearPoint + 1, crewDeckExtensionFarPoint - 1); // Upper Deck Extension
                    CopyRegionOverYUsingReferenceNode(16, 7, 11, crewDeckExtensionLeftPoint, crewDeckExtensionRightPoint, 7, topStairLevel2, crewDeckExtensionNearPoint, crewDeckExtensionNearPoint); // Upper Deck Shell Extension Near
                    CopyRegionOverYUsingReferenceNode(16, 7, 11, crewDeckExtensionLeftPoint, crewDeckExtensionRightPoint, 7, topStairLevel2, crewDeckExtensionFarPoint, crewDeckExtensionFarPoint); // Upper Deck Shell Extension Far
                    CopyRegionOverYUsingReferenceNode(16, 7, 11, crewDeckExtensionLeftPoint, crewDeckExtensionLeftPoint, 7, topStairLevel2, crewDeckExtensionNearPoint, crewDeckExtensionFarPoint); // Upper Deck Shell Extension Across Left
                    CopyRegionOverYUsingReferenceNode(16, 7, 11, crewDeckExtensionRightPoint, crewDeckExtensionRightPoint, 7, topStairLevel2, crewDeckExtensionNearPoint, crewDeckExtensionFarPoint); // Upper Deck Shell Extension Across Right

                    //CopyRegionOverYUsingReferenceNode(13, 5, 4, crewDeckExtensionLeftPoint + 1, crewDeckExtensionLeftPoint + 3, bottomStairLevel2, topStairLevel2, crewDeckExtensionNearPoint + 1, crewDeckExtensionNearPoint + 2);
                    //CopyRegionOverYUsingReferenceNode(13, 5, 4, crewDeckExtensionLeftPoint + 1, crewDeckExtensionLeftPoint + 3, bottomStairLevel2, topStairLevel2, crewDeckExtensionFarPoint - 2, crewDeckExtensionFarPoint - 1);


                    //CopyRegionOverYUsingReferenceNode(16, 5, 3, 62, 62, 5, 5, 2, 2); // Crew Deck Shell Extension Near
                    //CopyRegionOverYUsingReferenceNode(16, 5, 3, 62, 62, 5, 5, 14, 14); // Crew Deck Shell Extension Far
                }
                */

                if (ModSettings.spawnAdditionalEngineRoomWorkshops)
                {
                    // ----- Engine room extra rooms -----
                    CopyRegionOverYUsingReferenceNode(10, 0, 4, 0, 3, 0, 0, 10, 12); // Engine Room Deck 0 Extra Room 1
                    CopyRegionOverYUsingReferenceNode(10, 0, 4, 7, 9, 2, 2, 10, 12); // Engine Room Deck 2 Extra Room 1

                    if (ModSettings.aggressiveWorkshopSpawning)
                    {
                        CopyRegionOverYUsingReferenceNode(10, 0, 4, 7, 9, 1, 1, 10, 12); // Engine Room Deck 1 Extra Room 1
                        CopyRegionOverYUsingReferenceNode(0, 1, 12, 7, 8, 1, 1, 9, 9); // Space For Engine Room Deck 1 Extra Room 1

                        CopyRegionOverYUsingReferenceNode(10, 0, 4, 4, 4, 0, 0, 10, 12); // Engine Room Deck 0 Extra Room 1 Extension
                        CopyRegionOverYUsingReferenceNode(5, 0, 10, 5, 5, 0, 0, 11, 12); // Space For Engine Room Deck 0 Extra Room 1 Extension

                        CopyRegionOverYUsingReferenceNode(10, 0, 4, 6, 8, 0, 0, 10, 12); // Engine Room Deck 0 Extra Room 2
                        CopyRegionOverYUsingReferenceNode(5, 0, 10, 7, 8, 0, 0, 9, 9); // Space For Engine Room Deck 0 Extra Room 2
                    }

                    foreach (Room room in LevelGeneration.Instance.engineRoomPrefabs)
                    {
                        //Debug.Log("Found engineRoomPrefab. Its name is " + room.name + ". Its minRoomCount is " + room.MinRoomCount + " and its maxRoomCount is " + room.MaxRoomCount + ". ceaseSpawning = " + room.ceaseSpawning + ".");
                        if (room.name.Contains("Workshop"))
                        {
                            //Debug.Log("Found " + room.name + ". Increasing its room count.");
                            room.roomAmountMin *= 6;
                            room.roomAmountMax *= 6;
                        }
                    }
                    if (LevelGeneration.Instance.engineRoomPrefabs.Count == 0)
                    {
                        Debug.Log("Could not find any engine room prefabs.");
                    }
                }

                if (ModSettings.reduceNormalNumberOfCorridorsToCargoHold > 0)
                {
                    // The following lines REMOVE corridors.
                    if (ModSettings.reduceNormalNumberOfCorridorsToCargoHold == 2)
                    {
                        CopyRegionOverYUsingReferenceNode(0, 0, 0, 22, 37, 3, 4, 1, 1); // Engine Side Near Corridor Deck 3 & 4
                        CopyRegionOverYUsingReferenceNode(0, 0, 0, 22, 37, 3, 4, 14, 14); // Engine Side Far Corridor Deck 3 & 4
                        CopyRegionOverYUsingReferenceNode(0, 0, 0, 36, 48, 1, 2, 1, 1); // Sub Side Near Corridor Deck 1 & 2
                        CopyRegionOverYUsingReferenceNode(0, 0, 0, 36, 48, 1, 2, 14, 14); // Sub Side Far Corridor Deck 1 & 2
                    }
                    else
                    {
                        float randomChance = UnityEngine.Random.value;
                        if (randomChance > 0.5f)
                        {
                            CopyRegionOverYUsingReferenceNode(0, 0, 0, 22, 37, 3, 3, 1, 1); // Engine Side Near Corridor Deck 3
                            CopyRegionOverYUsingReferenceNode(0, 0, 0, 22, 37, 4, 4, 14, 14); // Engine Side Far Corridor Deck 4
                            CopyRegionOverYUsingReferenceNode(0, 0, 0, 36, 48, 2, 2, 1, 1); // Sub Side Near Corridor Deck 2
                            CopyRegionOverYUsingReferenceNode(0, 0, 0, 36, 48, 1, 1, 14, 14); // Sub Side Far Corridor Deck 1
                        }
                        else
                        {
                            CopyRegionOverYUsingReferenceNode(0, 0, 0, 22, 37, 4, 4, 1, 1); // Engine Side Near Corridor Deck 4
                            CopyRegionOverYUsingReferenceNode(0, 0, 0, 22, 37, 3, 3, 14, 14); // Engine Side Far Corridor Deck 3
                            CopyRegionOverYUsingReferenceNode(0, 0, 0, 36, 48, 1, 1, 1, 1); // Sub Side Near Corridor Deck 1
                            CopyRegionOverYUsingReferenceNode(0, 0, 0, 36, 48, 2, 2, 14, 14); // Sub Side Far Corridor Deck 2
                        }
                    }
                }

                if (ModSettings.shortenedCargoHoldCorridors)
                {
                    // This REMOVES corridors.
                    CopyRegionOverYUsingReferenceNode(0, 0, 0, 24, 46, 1, 4, 1, 1);
                    CopyRegionOverYUsingReferenceNode(0, 0, 0, 24, 46, 1, 4, 14, 14);
                }

                if (ModSettings.experimentalShipExtension)
                {
                    /*
                    CopyRegionOverYUsingReferenceNode(3, 5, 1, 4, 4, 5, 5, (int)Settings.ShipCubesCount.z / 2 - 1, (int)Settings.ShipCubesCount.z - 2); // Set Walkway
                    CopyRegionOverYUsingReferenceNode(3, 5, 0, 3, 3, 5, 5, (int)Settings.ShipCubesCount.z / 2 - 1, (int)Settings.ShipCubesCount.z - 2); // Set Outer Deck Cargo Left
                    CopyRegionOverYUsingReferenceNode(3, 5, 0, 5, 5, 5, 5, (int)Settings.ShipCubesCount.z / 2 - 1, (int)Settings.ShipCubesCount.z - 2); // Set Outer Deck Cargo Right
                    CopyRegionOverYUsingReferenceNode(3, 5, 0, 3, 5, 5, 5, (int)Settings.ShipCubesCount.z - 1, (int)Settings.ShipCubesCount.z - 1); // Set Outer Deck Cargo Far
                    */

                    CopyRegionOverYUsingReferenceNode(60, 1, 3, 14, 65, 1, 4, (int)Settings.ShipCubesCount.z / 2 + 1, (int)Settings.ShipCubesCount.z - 1); // Lower deck rooms to fill the new space.

                    int bottomStairLevel = 0;
                    int topStairLevel = 4;
                    // Set 1 (Traditional Lineup)
                    CopyRegionOverYUsingReferenceNode(13, 3, 4, 61, 63, bottomStairLevel, topStairLevel, 16, 17);
                    CopyRegionOverYUsingReferenceNode(13, 3, 4, 61, 63, bottomStairLevel, topStairLevel, 30, 31);

                    customBottomStairLevelList.Add(bottomStairLevel);
                    Settings.stairsPerLevel++;
                    customBottomStairLevelList.Add(bottomStairLevel);
                    Settings.stairsPerLevel++;
                }

                if (ModSettings.increaseRoomMinimumCount != 0 || ModSettings.increaseRoomMaximumCount != 0)
                {
                    if (ModSettings.includeUniqueRoomsInCountChange)
                    {
                        IncreaseRoomCounts(true);
                    }
                    else
                    {
                        IncreaseRoomCounts(false);
                    }
                }

                if (ModSettings.extendMapAdditive)
                {
                    Debug.Log("Starting additive map extension.");
                    foreach (string convertibleRegion in convertibleRegions)
                    {
                        ExtendMapAdditively(coreRegions, convertibleRegion);
                    }
                    Debug.Log("Additive map extension completed.");
                }
                else if (ModSettings.extendMap)
                {
                    Debug.Log("Starting map extension.");
                    foreach (string convertibleRegion in convertibleRegions)
                    {
                        // Add all inaccessible nodes to the core regions at their respective y levels.
                        ExtendSpecificRegion(coreRegions, convertibleRegion);
                    }
                    Debug.Log("Map extension completed.");
                }

                if (ModSettings.shuffledRegions)
                {
                    Debug.Log("Starting region shuffle.");
                    ShuffleRegions(coreRegions);
                    Debug.Log("Region shuffle completed.");
                }

                //IncreaseStairHeight();

                // # Temporary Test Level Generation Changes
                //CopyRegionOverYUsingReferenceNode(0, 3, 3, 10, 13, 2, 2, 7, 9); // Mini engine room extension attempt
                //CopyRegionOverYUsingReferenceNode(new Vector3(0, 3, 3), new Vector3(10, 2, 3), new Vector3(21, 2, 13)); // Bigger engine room extension attempt
                //CopyRegionOverYUsingReferenceNode(new Vector3(0, 3, 3), new Vector3(11, 2, 3), new Vector3(21, 2, 13)); // Separated engine room extension attempt
                //CopyRegionOverYUsingReferenceNode(new Vector3(0, 3, 3), new Vector3(11, 2, 3), new Vector3(53, 2, 13)); // Replacing cargo hold with engine room
            }

            private static void ShuffleRegions(string[] coreRegions)
            {
                // Create a list of permitted region IDs.
                List<int> allowedRegionIDs = new List<int>();

                // Add all core regions to the allowed regions list. If crazy shuffle mode is on, add all regions to the allowed regions list.
                if (!ModSettings.crazyShuffle)
                {
                    foreach (string regionName in coreRegions)
                    {
                        allowedRegionIDs.Add(RegionManager.Instance.StringToRegionID(regionName));
                    }
                }
                else
                {
                    foreach (RegionEntry regionEntry in RegionManager.Instance.regions)
                    {
                        allowedRegionIDs.Add(regionEntry.regionID);
                    }
                }

                // Create an array of region IDs to choose from.
                int[] regionIDs = allowedRegionIDs.ToArray();

                for (int swapCount = 0; swapCount < ModSettings.numberOfTimesToShuffleRegions; swapCount++)
                {
                    // Choose two region indices between 0 and the number of regions IDs in the array minus 1.
                    int regionIndex1 = UnityEngine.Random.Range(0, regionIDs.Length);
                    int regionIndex2 = UnityEngine.Random.Range(0, regionIDs.Length);

                    // Get the IDs from the region ID array.
                    int regionID1 = regionIDs[regionIndex1];
                    int regionID2 = regionIDs[regionIndex2];

                    // Get the indeces used by the region manager from the IDs.
                    int regionManagerRegionIndex1 = RegionManager.Instance.IDToIndex(regionID1);
                    int regionManagerRegionIndex2 = RegionManager.Instance.IDToIndex(regionID2);

                    // If the two random indices are the same, repeat the loop one more time.
                    if (regionManagerRegionIndex1 != regionManagerRegionIndex2)
                    {
                        SwapRegions(regionManagerRegionIndex1, regionManagerRegionIndex2);
                    }
                    else
                    {
                        swapCount--;
                    }
                }
            }

            private static void SwapRegions(int regionIndex1, int regionIndex2)
            {
                // First, swap the vectors of associated nodes of the two regions.
                List<Vector3> associatedNodes = RegionManager.Instance.regions[regionIndex1].associatedNodes;
                RegionManager.Instance.regions[regionIndex1].associatedNodes = RegionManager.Instance.regions[regionIndex2].associatedNodes;
                RegionManager.Instance.regions[regionIndex2].associatedNodes = associatedNodes;

                // As the associated nodes were already swapped and are not being changed again, we can simply make a point in space inherit the region ID we want based on the associated nodes.
                ConvertRegionID(regionIndex1, regionIndex2);
                ConvertRegionID(regionIndex2, regionIndex1);

                Debug.Log("Swapped regions: " + RegionManager.Instance.regions[regionIndex1].regionName + " & " + RegionManager.Instance.regions[regionIndex2].regionName);
            }

            private static void ConvertRegion(string increasingRegion, string convertingRegion, bool heightDependence = false, bool removePointsFromConvertingRegion = true)
            {
                int increasingRegionIndex = RegionManager.Instance.StringToRegionIndex(increasingRegion);
                int convertingRegionIndex = RegionManager.Instance.StringToRegionIndex(convertingRegion);

                if (!heightDependence)
                {
                    RegionManager.Instance.regions[increasingRegionIndex].associatedNodes.AddRange(RegionManager.Instance.regions[convertingRegionIndex].associatedNodes);
                    ConvertRegionID(increasingRegionIndex, convertingRegionIndex);
                }
                else
                {
                    // First, get the minimum and maximum y values of the region.
                    float minY = RegionManager.Instance.regions[increasingRegionIndex].associatedNodes[0].y;
                    float maxY = minY;

                    for (int i = 1; i < RegionManager.Instance.regions[increasingRegionIndex].associatedNodes.Count; i++)
                    {
                        if (RegionManager.Instance.regions[increasingRegionIndex].associatedNodes[i].y < minY)
                        {
                            minY = RegionManager.Instance.regions[increasingRegionIndex].associatedNodes[i].y;
                        }
                        else if (RegionManager.Instance.regions[increasingRegionIndex].associatedNodes[i].y > minY)
                        {
                            maxY = RegionManager.Instance.regions[increasingRegionIndex].associatedNodes[i].y;
                        }
                    }

                    if (RegionManager.Instance.regions[increasingRegionIndex].regionName.Equals("Upper Deck"))
                    {
                        maxY = Settings.ShipCubesCount.y;
                    }
                    else if (RegionManager.Instance.regions[increasingRegionIndex].regionName.Equals("Lower Deck"))
                    {
                        minY = 0;
                    }

                    // If the node in the converting region is at the right y level, add it to the increasing region.
                    foreach (Vector3 node in RegionManager.Instance.regions[convertingRegionIndex].associatedNodes)
                    {
                        if (node.y >= minY && node.y <= maxY)
                        {
                            RegionManager.Instance.regions[increasingRegionIndex].associatedNodes.Add(node);
                        }
                    }

                    ConvertRegionID(increasingRegionIndex, convertingRegionIndex, (int)minY, (int)maxY);
                }

                if (removePointsFromConvertingRegion)
                {
                    RegionManager.Instance.regions[convertingRegionIndex].associatedNodes.Clear();
                }
            }

            private static void ExtendMapAdditively(string[] coreRegions, string convertingRegion)
            {
                int[] increasingRegionIndices = new int[coreRegions.Length];
                for (int regionIndex = 0; regionIndex < coreRegions.Length; regionIndex++)
                {
                    increasingRegionIndices[regionIndex] = RegionManager.Instance.StringToRegionIndex(coreRegions[regionIndex]);
                }

                int convertingRegionIndex = RegionManager.Instance.StringToRegionIndex(convertingRegion);

                for (int regionIndex = 0; regionIndex < coreRegions.Length; regionIndex++)
                {
                    RegionManager.Instance.regions[increasingRegionIndices[regionIndex]].associatedNodes.AddRange(RegionManager.Instance.regions[convertingRegionIndex].associatedNodes);
                    AddRegionIDToNewAssociatedNodes(increasingRegionIndices[regionIndex], convertingRegionIndex);
                    Debug.Log("Added all " + RegionManager.Instance.regions[convertingRegionIndex].regionName + " nodes to the " + RegionManager.Instance.regions[increasingRegionIndices[regionIndex]].regionName + " region.");
                }

                RegionManager.Instance.regions[convertingRegionIndex].associatedNodes.Clear();
            }

            private static void ExtendSpecificRegion(string[] coreRegions, string convertingRegion)
            {
                for (int increasingRegionIndex = 0; increasingRegionIndex < coreRegions.Length; increasingRegionIndex++)
                {
                    if (increasingRegionIndex != coreRegions.Length - 1)
                    {
                        ConvertRegion(coreRegions[increasingRegionIndex], convertingRegion, true, false);
                    }
                    else
                    {
                        ConvertRegion(coreRegions[increasingRegionIndex], convertingRegion, true, true);
                    }
                    Debug.Log("Extended the " + coreRegions[increasingRegionIndex] + " region using the " + convertingRegion + " region.");
                }
            }

            private static void ConvertRegionID(int increasingRegionIndex, int convertingRegionIndex, int minY = 0, int maxY = 0)
            {
                if (maxY == 0)
                {
                    maxY = (int)Settings.shipCubesCount.y;
                }
                for (int x = 0; x < Settings.ShipCubesCount.x; x++)
                {
                    for (int y = minY; y < maxY; y++)
                    {
                        for (int z = 0; z < Settings.ShipCubesCount.z; z++)
                        {
                            for (int regionIDFromArrayIndex = 0; regionIDFromArrayIndex < RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID.Count; regionIDFromArrayIndex++)
                            {
                                /*
                                A point in space can be associated with multiple regions.
                                If the point in space is associated with region 1 but has region 2's ID, give it region 1's ID where it had region 2's ID before.

                                Example:
                                The point could be associated with the Engine region and Lower Deck region, but we wanted to swap the Lower Deck with the Upper Deck.
                                This means that the point will be contained in the Upper Deck's associated nodes as we swapped the nodes earlier, but will have the Lower Deck region ID.
                                This means we can replace the ID where the Lower Deck's region ID was stored before with the Upper Deck region ID.
                                After this operation, the point will be associated with the Engine region and the Upper Deck region, which will allow it to pass the ID consistency check in PositionRoomLG.RoomCollision.
                                */
                                if (RegionManager.Instance.regions[increasingRegionIndex].associatedNodes.Contains(new Vector3((float)x, (float)y, (float)z)) && RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID[regionIDFromArrayIndex] == RegionManager.Instance.regions[convertingRegionIndex].regionID)
                                {
                                    RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID[regionIDFromArrayIndex] = RegionManager.Instance.regions[increasingRegionIndex].regionID;
                                }
                            }
                        }
                    }
                }
            }

            private static void AddRegionIDToNewAssociatedNodes(int increasingRegionIndex, int convertingRegionIndex)
            {
                int increasingRegionID = RegionManager.Instance.regions[increasingRegionIndex].regionID;
                for (int x = 0; x < Settings.ShipCubesCount.x; x++)
                {
                    for (int y = 0; y < Settings.shipCubesCount.y; y++)
                    {
                        for (int z = 0; z < Settings.ShipCubesCount.z; z++)
                        {
                            if (RegionManager.Instance.regions[increasingRegionIndex].associatedNodes.Contains(new Vector3((float)x, (float)y, (float)z)))
                            {
                                bool containsRegionAlready = false;

                                for (int regionIDFromArrayIndex = 0; regionIDFromArrayIndex < RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID.Count; regionIDFromArrayIndex++)
                                {
                                    if (RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID[regionIDFromArrayIndex] == increasingRegionID)
                                    {
                                        containsRegionAlready = true;
                                        break;
                                    }
                                }

                                if (!containsRegionAlready)
                                {
                                    RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID.Add(increasingRegionID);
                                }
                            }
                        }
                    }
                }
            }

            // # Giving monster some kind of radar for the monster player would help balance. Could also indicate camera positions when triggered. Items are shown to monster even when player is hiding. Only in lockers? Can't use interactable like crane when first player is crouched and hiding. Other player still lerps on some occasions. Maybe only first lerp of a kind. Add tip to screen when player is downed. Player cameras / body positions are still not always properly reset after a player has been revived. Monster light of Hunter is still a problem (Only when monster is number 1...???!!!!).

            private static void IncreaseRoomCounts(bool changeSingleRooms = false, PrimaryRegionType specificRegion = PrimaryRegionType.None, int specificMinimum = 0, int specificMaximum = 0)
            {
                string[] specialRooms = { "Bridge_v1", "Upd_FuseRoom_CrewDeck", "Escape_SubRoom", "Lwd_FuseRoom", "EscapeRoom_Helicopter", "Deck_BowRoom", "Deck_CargoWalkwayTower", "Deck_Stern" };
                int minToUse = ModSettings.increaseRoomMinimumCount;
                int maxToUse = ModSettings.increaseRoomMaximumCount;
                if (specificMinimum != 0)
                {
                    minToUse = specificMinimum;
                }
                if (specificMaximum != 0)
                {
                    maxToUse = specificMaximum;
                }

                foreach (Room room in LevelGeneration.Instance.roomPrefabs)
                {
                    //Debug.Log("Found roomPrefab. Its name is " + room.name + ". Its minRoomCount is " + room.MinRoomCount + " and its maxRoomCount is " + room.MaxRoomCount + ". ceaseSpawning = " + room.ceaseSpawning + ".");
                    if ((specificRegion == PrimaryRegionType.None || specificRegion == room.PrimaryRegion) && (changeSingleRooms || room.roomAmountMax > 1) && !specialRooms.Contains(room.name))
                    {
                        room.roomAmountMin += minToUse;
                        room.roomAmountMax += maxToUse;

                        if (room.roomAmountMin < 1)
                        {
                            room.roomAmountMin = 1;
                        }
                        if (room.roomAmountMax < 1)
                        {
                            room.roomAmountMax = 1;
                        }

                        foreach (ModelData modelData in room.roomModels)
                        {
                            if (modelData.model != null)
                            {
                                ModelSpawnCountData modelSpawnCountData = modelData.model.GetComponent<ModelSpawnCountData>();
                                if (modelSpawnCountData != null)
                                {
                                    modelSpawnCountData.minCount += minToUse;
                                    modelSpawnCountData.maxCount += maxToUse;
                                    if (modelSpawnCountData.minCount < 1)
                                    {
                                        modelSpawnCountData.minCount = 1;
                                    }
                                    if (modelSpawnCountData.maxCount < 1)
                                    {
                                        modelSpawnCountData.maxCount = 1;
                                    }
                                }
                                else
                                {
                                    if (ModSettings.logDebugText)
                                    {
                                        Debug.Log("Room modelSpawnCountData is null!");
                                    }
                                }
                            }
                            else
                            {
                                if (ModSettings.logDebugText)
                                {
                                    Debug.Log("Room modelData.model is null!");
                                }
                            }
                        }
                    }
                }
            }

            private static void IncreaseStairHeight()
            {
                int stairsRegionIndex = RegionManager.Instance.StringToRegionIndex("Stairs");
                int inaccessibleRegionIndex = RegionManager.Instance.StringToRegionIndex("Inaccessible");

                for (int nodeIndex = 0; nodeIndex < RegionManager.Instance.regions[stairsRegionIndex].associatedNodes.Count; nodeIndex++)
                {
                    Vector3 nodeAbove = RegionManager.Instance.regions[stairsRegionIndex].associatedNodes[nodeIndex] + Vector3.up;

                    ConvertNode(nodeAbove, stairsRegionIndex, inaccessibleRegionIndex);
                }
            }

            private static void ConvertNode(Vector3 node, int increasingRegionIndex, int convertingRegionIndex)
            {
                if (CheckBoundariesLG.NodeWithinShipBounds(node) && !RegionManager.Instance.regions[increasingRegionIndex].associatedNodes.Contains(node + Vector3.up))
                {
                    int nodeX = (int)node.x;
                    int nodeY = (int)node.y;
                    int nodeZ = (int)node.z;

                    RegionManager.Instance.regions[increasingRegionIndex].associatedNodes.Add(node);

                    for (int x = 0; x < Settings.ShipCubesCount.x; x++)
                    {
                        for (int y = 0; y < Settings.ShipCubesCount.y; y++)
                        {
                            for (int z = 0; z < Settings.ShipCubesCount.z; z++)
                            {
                                for (int regionIDFromArrayIndex = 0; regionIDFromArrayIndex < RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID.Count; regionIDFromArrayIndex++)
                                {
                                    if (RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID[regionIDFromArrayIndex] == RegionManager.Instance.regions[convertingRegionIndex].regionID)
                                    {
                                        RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID[regionIDFromArrayIndex] = RegionManager.Instance.regions[increasingRegionIndex].regionID;
                                    }
                                }
                            }
                        }
                    }

                    /*
                    for (int stairIndex = 0; stairIndex < LevelGeneration.Instance.stairPrefabs.Count; stairIndex++)
                    {
                        if (LevelGeneration.Instance.stairPrefabs[stairIndex].StairsType == StairStructure.Middle)
                        {
                            LevelGeneration.Instance.stairPrefabs.Add(LevelGeneration.Instance.stairPrefabs[stairIndex]);
                            break;
                        }
                    }
                    */

                    /*
                    foreach (int regionIDFromArrayIndex in RegionManager.Instance.regionData[nodeX].regionDataY[nodeY].regionDataZ[nodeZ].regionID)
                    {
                        if (RegionManager.Instance.regionData[nodeX].regionDataY[nodeY].regionDataZ[nodeZ].regionID[regionIDFromArrayIndex] == RegionManager.Instance.regions[convertingRegionIndex].regionID)
                        {
                            RegionManager.Instance.regionData[nodeX].regionDataY[nodeY].regionDataZ[nodeZ].regionID[regionIDFromArrayIndex] = RegionManager.Instance.regions[increasingRegionIndex].regionID;
                        }
                    }
                    */
                }
            }

            private static void CopyRegionOverY(int minX, int maxX, int copyY, int pasteY, int minZ, int maxZ)
            {
                Debug.Log("Starting region copy over y.");
                for (int x = minX; x <= maxX; x++)
                {
                    for (int z = minZ; z <= maxZ; z++)
                    {
                        List<int> copyRegionIDs = RegionManager.Instance.regionData[x].regionDataY[copyY].regionDataZ[z].regionID;
                        List<int> pasteRegionIDs = RegionManager.Instance.regionData[x].regionDataY[pasteY].regionDataZ[z].regionID;

                        // May be able to use either of the two lines below. Properties are occupied and regionID, so a direct copy is probably better.
                        RegionManager.Instance.regionData[x].regionDataY[pasteY].regionDataZ[z] = RegionManager.Instance.regionData[x].regionDataY[copyY].regionDataZ[z];
                        //RegionManager.Instance.regionData[x].regionDataY[pasteY].regionDataZ[z].regionID = RegionManager.Instance.regionData[x].regionDataY[copyY].regionDataZ[z].regionID;

                        Vector3 copyNode = RegionManager.Instance.ConvertPointToRegionNode(Settings.ShipPos + new Vector3(x * Settings.CuboidDim.x, copyY * Settings.CuboidDim.y, z * Settings.CuboidDim.z));
                        Vector3 pasteNode = RegionManager.Instance.ConvertPointToRegionNode(Settings.ShipPos + new Vector3(x * Settings.CuboidDim.x, pasteY * Settings.CuboidDim.y, z * Settings.CuboidDim.z));
                        //Debug.Log("Copy node is " + copyNode + " and paste node is " + pasteNode + ".");

                        foreach (int pasteRegionID in pasteRegionIDs)
                        {
                            int pasteRegionIndex = RegionManager.Instance.IDToIndex(pasteRegionID);
                            RegionManager.Instance.regions[pasteRegionIndex].associatedNodes.Remove(pasteNode);
                        }
                        foreach (int copyRegionID in copyRegionIDs)
                        {
                            int copyRegionIndex = RegionManager.Instance.IDToIndex(copyRegionID);
                            if (!RegionManager.Instance.regions[copyRegionIndex].associatedNodes.Contains(pasteNode))
                            {
                                RegionManager.Instance.regions[copyRegionIndex].associatedNodes.Add(pasteNode);
                            }
                        }

                        /*
                        for (int i = 0; i < RegionManager.Instance.regions.Count; i++)
                        {
                            if (RegionManager.Instance.regions[i].associatedNodes.Contains(pasteNode))
                            {
                                //Debug.Log(RegionManager.Instance.regions[i].regionName + " contains node " + pasteNode + ". Removing it.");
                                RegionManager.Instance.regions[i].associatedNodes.Remove(pasteNode);
                                break;
                            }
                        }

                        for (int i = 0; i < RegionManager.Instance.regions.Count; i++)
                        {
                            if (RegionManager.Instance.regions[i].associatedNodes.Contains(copyNode))
                            {
                                //Debug.Log(RegionManager.Instance.regions[i].regionName + " contains node " + copyNode + ". Adding " + pasteNode + ".");
                                RegionManager.Instance.regions[i].associatedNodes.Add(pasteNode);
                                break;
                            }
                        }
                        */
                    }
                }

                /*
                foreach (RegionEntry regionEntry in RegionManager.Instance.regions)
                {
                    Debug.Log("Nodes of " + regionEntry.regionName + ":");
                    foreach (Vector3 vector in regionEntry.associatedNodes)
                    {
                        Debug.Log(vector);
                    }
                }
                */
            }

            private static void CopyRegionOverYUsingReferenceNode(Vector3 copyVector, Vector3 rangeStartVector, Vector3 rangeEndVector)
            {
                CopyRegionOverYUsingReferenceNode((int)copyVector.x, (int)copyVector.y, (int)copyVector.z, (int)rangeStartVector.x, (int)rangeEndVector.x, (int)rangeStartVector.y, (int)rangeEndVector.y, (int)rangeStartVector.z, (int)rangeEndVector.z);
            }

            private static void CopyRegionOverYUsingReferenceNode(int copyX, int copyY, int copyZ, int minX, int maxX, int minY, int maxY, int minZ, int maxZ)
            {
                if (!CheckBoundariesLG.NodeWithinShipBounds(new Vector3(copyX, copyY, copyZ)))
                {
                    Debug.Log("Copy node " + new Vector3(copyX, copyY, copyZ) + " is not in ship bounds!");
                    return;
                }
                RegionNodeDataZ copyRegionData = RegionManager.Instance.regionData[copyX].regionDataY[copyY].regionDataZ[copyZ];
                List<int> copyRegionIDs = copyRegionData.regionID;
                Vector3 copyNode = RegionManager.Instance.ConvertPointToRegionNode(Settings.ShipPos + new Vector3(copyX * Settings.CuboidDim.x, copyY * Settings.CuboidDim.y, copyZ * Settings.CuboidDim.z));

                Debug.Log("Starting region copy over y using reference node.");
                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        for (int z = minZ; z <= maxZ; z++)
                        {
                            if (CheckBoundariesLG.NodeWithinShipBounds(new Vector3(x, y, z)))
                            {
                                List<int> pasteRegionIDs = RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID;

                                RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z] = copyRegionData;
                                Vector3 pasteNode = RegionManager.Instance.ConvertPointToRegionNode(Settings.ShipPos + new Vector3(x * Settings.CuboidDim.x, y * Settings.CuboidDim.y, z * Settings.CuboidDim.z));

                                foreach (int pasteRegionID in pasteRegionIDs)
                                {
                                    int pasteRegionIndex = RegionManager.Instance.IDToIndex(pasteRegionID);
                                    RegionManager.Instance.regions[pasteRegionIndex].associatedNodes.Remove(pasteNode);
                                }
                                foreach (int copyRegionID in copyRegionIDs)
                                {
                                    int copyRegionIndex = RegionManager.Instance.IDToIndex(copyRegionID);
                                    if (!RegionManager.Instance.regions[copyRegionIndex].associatedNodes.Contains(pasteNode))
                                    {
                                        RegionManager.Instance.regions[copyRegionIndex].associatedNodes.Add(pasteNode);
                                    }
                                }
                            }
                            else
                            {
                                Debug.Log("Paste node " + new Vector3(x, y, z) + " is not in ship bounds!");
                            }
                        }
                    }
                }

                /*
                foreach (RegionEntry regionEntry in RegionManager.Instance.regions)
                {
                    Debug.Log("Nodes of " + regionEntry.regionName + ":");
                    foreach (Vector3 vector in regionEntry.associatedNodes)
                    {
                        Debug.Log(vector);
                    }
                }
                */
            }

            private static void SetRegionInRange(string regionString, int minX, int maxX, int minY, int maxY, int minZ, int maxZ, bool useAsOnlyRegion = false)
            {
                int regionID = RegionManager.Instance.StringToRegionID(regionString);
                int regionIndex = RegionManager.Instance.StringToRegionIndex(regionString);
                for (int x = minX; x <= maxX; x++)
                {
                    for (int y = minY; y <= maxY; y++)
                    {
                        for (int z = minZ; z <= maxZ; z++)
                        {
                            Vector3 copyNode = RegionManager.Instance.ConvertPointToRegionNode(Settings.ShipPos + new Vector3(x * Settings.CuboidDim.x, y * Settings.CuboidDim.y, z * Settings.CuboidDim.z));

                            if (!RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID.Contains(regionID))
                            {
                                if (useAsOnlyRegion)
                                {
                                    /*
                                    for (int i = 1; i < RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID.Count; i++)
                                    {
                                        RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID.Remove(i);
                                    }
                                    */
                                    RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID = new List<int>();
                                    for (int i = 0; i < RegionManager.Instance.regions.Count; i++)
                                    {
                                        if (i != regionIndex && RegionManager.Instance.regions[i].associatedNodes.Contains(copyNode))
                                        {
                                            RegionManager.Instance.regions[i].associatedNodes.Remove(copyNode);
                                        }
                                    }

                                    RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID.Add(regionID);
                                }
                                else
                                {
                                    //RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID.Add(regionID);
                                    RegionManager.Instance.regionData[x].regionDataY[y].regionDataZ[z].regionID[0] = regionID;
                                }
                            }

                            if (!RegionManager.Instance.regions[regionIndex].associatedNodes.Contains(copyNode))
                            {
                                RegionManager.Instance.regions[regionIndex].associatedNodes.Add(copyNode);
                            }
                        }
                    }
                }
            }

            /* All Region Names:
            Inaccessible
            Crew Deck
            Walkways
            LifeRaftEscape
            Bridge
            Stairs
            Shell
            Deck
            Ladder Region (Temp)
            Upper Deck
            Lower Deck
            Cargo_Catwalks
            Cargo_Containers
            Cargo_MainHold
            Cargo_Stairs
            Sub Escape
            StartRoom
            LockedRooms - (Randomly?) designated areas where power locked rooms may appear.
            Engines_MainArea
            Engines_Catwalks
            Engines_MainEngine
            Engines_SingleFloorMachines
            Engines_MultiFloorMachines
            Engines_WalledRooms
            Engines_Stairs
            Engines_ControlRoom
            Lwd_StartRoom
            Deck_CargoContainers
            Deck_CargoWalkways
            Deck_CargoTowers
            Deck_CargoBowStairs
            HeliEscape
            Deck_MainCargoArea
            Deck_Bow
            Deck_ContainerBase
            Deck_HoldCover
            Deck_SingleContainers
            Deck_HeliContainers
            Deck_Stern
            Deck_BridgeWalkways
            
            Region Information Text:
            Region with index 0 has name Inaccessible and ID 0
            Region with index 1 has name Crew Deck and ID 53
            Region with index 2 has name Walkways and ID 59
            Region with index 3 has name LifeRaftEscape and ID 79
            Region with index 4 has name Bridge and ID 80
            Region with index 5 has name Stairs and ID 94
            Region with index 6 has name Shell and ID 95
            Region with index 7 has name Deck and ID 96
            Region with index 8 has name Ladder Region (Temp) and ID 97
            Region with index 9 has name Upper Deck and ID 98
            Region with index 10 has name Lower Deck and ID 100
            Region with index 11 has name Cargo_Catwalks and ID 112
            Region with index 12 has name Cargo_Containers and ID 113
            Region with index 13 has name Cargo_MainHold and ID 114
            Region with index 14 has name Cargo_Stairs and ID 115
            Region with index 15 has name Sub Escape and ID 116
            Region with index 16 has name StartRoom and ID 117
            Region with index 17 has name LockedRooms and ID 118
            Region with index 18 has name Engines_MainArea and ID 119
            Region with index 19 has name Engines_Catwalks and ID 120
            Region with index 20 has name Engines_MainEngine and ID 121
            Region with index 21 has name Engines_SingleFloorMachines and ID 122
            Region with index 22 has name Engines_MultiFloorMachines and ID 124
            Region with index 23 has name Engines_WalledRooms and ID 125
            Region with index 24 has name Engines_Stairs and ID 126
            Region with index 25 has name Engines_ControlRoom and ID 127
            Region with index 26 has name Lwd_StartRoom and ID 128
            Region with index 27 has name Deck_CargoContainers and ID 129
            Region with index 28 has name Deck_CargoWalkways and ID 130
            Region with index 29 has name Deck_CargoTowers and ID 131
            Region with index 30 has name Deck_CargoBowStairs and ID 132
            Region with index 31 has name HeliEscape and ID 133
            Region with index 32 has name Deck_MainCargoArea and ID 134
            Region with index 33 has name Deck_Bow and ID 135
            Region with index 34 has name Deck_ContainerBase and ID 136
            Region with index 35 has name Deck_HoldCover and ID 137
            Region with index 36 has name Deck_SingleContainers and ID 140
            Region with index 37 has name Deck_HeliContainers and ID 141
            Region with index 38 has name Deck_Stern and ID 142
            Region with index 39 has name Deck_BridgeWalkways and ID 143
            */

            /*----------------------------------------------------------------------------------------------------*/
            // @LightShafts

            private static void HookLightShafts()
            {
                On.LightShafts.OnRenderObject += new On.LightShafts.hook_OnRenderObject(HookLightShaftsOnRenderObject);
                On.LightShafts.UpdateLightType += new On.LightShafts.hook_UpdateLightType(HookLightShaftsUpdateLightType);
            }

            private static void HookLightShaftsOnRenderObject(On.LightShafts.orig_OnRenderObject orig, LightShafts lightShafts)
            {
                if (ModSettings.doNotRenderBruteLight || (ModSettings.enableCrewVSMonsterMode && ModSettings.numbersOfMonsterPlayers.Contains(0) && ManyMonstersMode.monsterListMonsterComponents[0].MonsterType == Monster.MonsterTypeEnum.Brute))
                {
                    return;
                }
                orig.Invoke(lightShafts);
            }

            private static void HookLightShaftsUpdateLightType(On.LightShafts.orig_UpdateLightType orig, LightShafts lightShafts)
            {
                if (lightShafts.m_Light == null)
                {
                    lightShafts.m_Light = ((MonoBehaviour)lightShafts).GetComponent<Light>();
                    if (ModSettings.UseCustomColour(ModSettings.bruteLightColour) || ModSettings.randomBruteLightColours)
                    {
                        if (ModSettings.UseCustomColour(ModSettings.bruteLightColour) && ModSettings.randomBruteLightColours)
                        {
                            float randomChance = UnityEngine.Random.value;
                            if (randomChance > 0.5f)
                            {
                                lightShafts.m_Light.color = ModSettings.ConvertColourStringToColour(ModSettings.bruteLightColour);
                            }
                            else
                            {
                                lightShafts.m_Light.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0.5f, 1f));
                            }
                        }
                        else if (ModSettings.UseCustomColour(ModSettings.bruteLightColour))
                        {
                            lightShafts.m_Light.color = ModSettings.ConvertColourStringToColour(ModSettings.bruteLightColour);
                        }
                        else
                        {
                            lightShafts.m_Light.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0.5f, 1f));
                        }
                    }
                    lightShafts.m_Light.intensity *= ModSettings.bruteLightIntensityMultiplier;
                    lightShafts.m_Light.range *= ModSettings.bruteLightRangeMultiplier;
                }
                lightShafts.m_LightType = lightShafts.m_Light.type;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @LoadingBackground

            private static Hints.HintCollection[] extendedLoadingScreenArray;
            private static readonly Dictionary<string, string[]> customLoadingScreenMessages = new Dictionary<string, string[]>()
            {
                { "AlphaBunks",             new String[]{"The structure of the Hisa Maru was changed a lot throughout construction."}},
                { "Brute",                  new String[]{"The Brute is a hulk of a monster that will excel in a direct chase."}},
                { "BrutePresence",          new String[]{"Paying attention to your surroundings can mean the difference between life and death."}},
                { "Clock",                  new String[]{"A good sense of time can help you to predict a monster's next action."}},
                { "ColourSettings",         new String[]{"The Colour Settings allow you to customise the appearance of the Hisa Maru."}},
                { "Container",              new String[]{"The cargo hold contains a large variety of items, but its maze of containers be difficult to navigate safely.", "Some of the cargo carried aboard the Hisa Maru proved difficult to contain..."}},
                { "DarkShip",               new String[]{"In Dark Ship mode, the Hisa Maru is plunged into darkness.", "Some monsters may benefit more than others from darkness."}},
                { "DeckZero",               new String[]{"Deck Zero is a dangerous stretch of corridors forming the deepest parts of the ship."} },
                { "Fiend",                  new String[]{"The Fiend is an intelligent monster with mysterious telekinetic powers."}},
                { "FiendPresence",          new String[]{"Having a keen ear will let you hear a monster before seeing it."}},
                { "Helicopter1",            new String[]{"Refueling the helicopter is an elaborate process that is sure to attract the attention of any monsters nearby."}},
                { "Helicopter2",            new String[]{"The helicopter can be used for fast transportation to land in case of emergencies."}},
                { "Hunter",                 new String[]{"The Hunter is a gelatinous monster that inspects the ship using its ventilation system."}},
                { "HunterPresence",         new String[]{"Some things on the ship are best not investigated too closely..."}},
                { "LevelGenerationSettings",new String[]{"The Level Generation Settings allow you to customise the ship's layout in unusual ways."}},
                { "Liferaft",               new String[]{"The life raft is often the safest option to escape against any monsters."}},
                { "LowerDecks",             new String[]{"The lower decks hold a lot of the ship's larger equipment in a maze of workshop rooms."}},
                { "Map",                    new String[]{"Maps placed on the walls throughout the ship may help you relocate yourself if lost."}},
                { "MESMSettings",           new String[]{"The Extended Settings Mod offers hundreds of settings to customise your experience."}},
                { "Monstrum2Documents",     new String[]{"The future answers many questions left unanswered by the past, but opens many others..."}},
                { "Monstrum2HisaMaru",      new String[]{"The Hisa Maru survived into the 21st century, severely damaged by time.", "The contents of some of the cargo carried aboard the Hisa Maru remained classified.", "While other parts of the ship decayed severely over time, the Hisa Maru's upper decks remained in a relatively good condition."}},
                { "Monstrum2SeaFort",       new String[]{"Genetic research on the monsters continued into the 21st century aboard a seemingly derelict array of sea forts."}},
                { "Multiplayer",            new String[]{"The local Multiplayer mode lets you play with friends on your computer. Third party software enables online play."}},
                { "OverpoweredSteam",       new String[]{"Steam onboard the Hisa Maru can be quite dangerous, especially with additional modifications..."}},
                { "Sparky",                 new String[]{"Sparky is a monster adept at lurking the player and interfering with the ship's power.", "Sparky was Monstrum's pre-alpha monster, reimagined in the mod with additional abilities."}},
                { "SparkyEasterEgg1",       new String[]{"Implementing a new monster takes a lot of time and dedication, but opens up the possibility for many new experiences."}},
                { "SparkyEasterEgg2",       new String[]{"Congratulations! You have stumbled upon the golden Sparky. This is the original model designed for the Monstrum 1 pre-alpha."}},
                { "SparkyEasterEgg3",       new String[]{"Don't let Sparky catch you in the dark...", "It is unwise to let Sparky drain all the ship's power..." }},
                { "SparkyPresence",         new String[]{"Sparky can drain the ship's power, requiring a region's electricity to be restored."}},
                { "SteamShutoff",           new String[]{"The ship's steam can be shut off, but doing so can prove difficult."}},
                { "Submersible",            new String[]{"The submersible requires uninterrupted time charging, providing monsters unaccounted for an ample opportunity to interfere."}},
                { "TV",                     new String[]{"Some objects that seem useless at first glance may prove to be more useful than thought."}},
                { "UpperDecks",             new String[]{"Rooms in the upper decks are filled with smaller items and plenty of hiding spots."}},
                { "Workstation",            new String[]{"Some items, notes and easter eggs may require a more thorough inspection of the environment to find."}}
            };

            private static void HookLoadingBackground(On.LoadingBackground.orig_Awake orig, LoadingBackground loadingBackground)
            {
                if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "Menus" || !ModSettings.skippedMenuScreen)
                {
                    Hints.HintCollection randomHint = Hints.GetRandomHint();

                    if (!ModSettings.disableCustomLoadingText)
                    {
                        //Debug.Log("Hints length before is: " + Hints.instance.hints.Length);

                        // Create an extended loading screen array of the default loading screens with the custom ones.
                        if (extendedLoadingScreenArray == null)
                        {
                            List<Hints.HintCollection> extendedLoadingScreenList = new List<Hints.HintCollection>(Hints.instance.hints);
                            UnityEngine.Object[] loadingScreensUnpacked = Utilities.LoadAssetBundle("loadingscreens");
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

                SetLoadingText(loadingBackground);
            }

            private static void SetLoadingText(LoadingBackground loadingBackground, string loadingProgressText = "")
            {
                if (!ModSettings.errorDuringLevelGeneration) // Don't update the text if an error has occurred during level generation.
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

                    if (LoadingBackground.loadingSprite != null && !ModSettings.disableCustomLoadingText)
                    {
                        if (!ModSettings.errorWhileReadingModSettings)
                        {
                            if (ModSettings.currentChallenge == null)
                            {
                                loadingBackground.text.text += "\n\nMonstrum Extended Settings Mod Version " + VERSION_WITH_TEXT + " Active";
                            }
                            else
                            {
                                loadingBackground.text.text += "\n\nMESM Version " + VERSION_WITH_TEXT + " Active With Challenge: " + ModSettings.currentChallenge.name;
                            }
                        }
                        else
                        {
                            loadingBackground.text.text += "\n\nError While Reading " + ModSettings.modSettingsErrorString + " Mod Settings - Fix Required";
                        }

                        if (!ModSettings.skippedMenuScreen)
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

            /*----------------------------------------------------------------------------------------------------*/
            // @MAttackingState2

            private static void HookMAttackingState2KillThePlayer(On.MAttackingState2.orig_KillThePlayer orig, MAttackingState2 mAttackingState2)
            {
                deathRegion = mAttackingState2.monster.PlayerDetectRoom.GetRoom.PrimaryRegion;
                deathMonster = mAttackingState2.monster.monsterType;
                orig.Invoke(mAttackingState2);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MChasingState

            /*
            // Let All Monsters Lock Doors Version
            private static void HookMChasingStateDoDoorCheck(On.MChasingState.orig_DoDoorCheck orig, MChasingState mChasingState, bool _overwrite)
            {
                if ((((MState)mChasingState).monster.MonsterType == Monster.MonsterTypeEnum.Fiend || ModSettings.letAllMonstersLockDoors) && (_overwrite || mChasingState.sinceDoorCheck.TimeElapsed > mChasingState.timeBetweenDoorCheck))
                {
                    mChasingState.sinceDoorCheck.ResetTimer();
                    FiendDoorSlam.SealDoors(((MState)mChasingState).monster.transform.position);
                    if (FiendDoorSlam.PlayFiendAnimation)
                    {
                        ((MState)mChasingState).monster.MoveControl.GetAniControl.DesiredUpperBodyWeight = 1f;
                    }
                }
            }

            private static void HookMChasingStateOnEnter(On.MChasingState.orig_OnEnter orig, MChasingState mChasingState)
            {
                orig.Invoke(mChasingState);

                if (ModSettings.letAllMonstersLockDoors && ((MState)mChasingState).monster.MonsterType != Monster.MonsterTypeEnum.Fiend)
                {
                    mChasingState.sinceDoorCheck.StartTimer();
                    mChasingState.DoDoorCheck(true);
                }
            }
            */

            // Fire Shroud Fire Blast Version
            private static void HookMChasingStateDoDoorCheck(On.MChasingState.orig_DoDoorCheck orig, MChasingState mChasingState, bool _overwrite)
            {
                if (ModSettings.giveAllMonstersAFireShroud || (ModSettings.bruteFireShroud && mChasingState.monster.MonsterType == Monster.MonsterTypeEnum.Brute))
                {
                    if (!mChasingState.sinceDoorCheck.timerStarted || _overwrite)
                    {
                        if (!mChasingState.sinceDoorCheck.timerStarted)
                        {
                            mChasingState.sinceDoorCheck.StartTimer();
                        }
                        ((MState)mChasingState).monster.GetComponent<FireShroud>().FireBlast();
                    }
                    else if (mChasingState.sinceDoorCheck.TimeElapsed > mChasingState.timeBetweenDoorCheck)
                    {
                        if (((MState)mChasingState).monster.MonsterType != Monster.MonsterTypeEnum.Fiend)
                        {
                            mChasingState.sinceDoorCheck.ResetTimer();
                        }
                        ((MState)mChasingState).monster.GetComponent<FireShroud>().FireBlast();
                    }
                }
                orig.Invoke(mChasingState, _overwrite);
            }

            private static void HookMChasingStateStateChanges(On.MChasingState.orig_StateChanges orig, MChasingState mChasingState)
            {
                int crewPlayerIndex = 0;
                if (ModSettings.enableMultiplayer)
                {
                    crewPlayerIndex = MultiplayerMode.crewPlayers.IndexOf(mChasingState.monster.PlayerDetectRoom.player);
                }
                if (!ModSettings.spawnProtection[crewPlayerIndex])
                {
                    mChasingState.changingState = false;
                    if (((MState)mChasingState).monster.MoveControl.GetAniControl.DoARoar || ((MState)mChasingState).monster.MoveControl.GetAniControl.IsHaulted)
                    {
                        ((MState)mChasingState).monster.MoveControl.MaxSpeed = 0f;
                    }
                    else
                    {
                        ((MState)mChasingState).monster.MoveControl.MaxSpeed = 100f;
                        if (((MState)mChasingState).monster.SubEventBeenStarted())
                        {
                            mChasingState.changingState = true;
                            ((MState)mChasingState).SendEvent("EventStarted");
                        }
                        else if (((MState)mChasingState).monster.IsMonsterRetreating)
                        {
                            mChasingState.changingState = true;
                            ((MState)mChasingState).SendEvent("Retreat");
                        }
                        else if (((MState)mChasingState).monster.IsMonsterDestroying)
                        {
                            mChasingState.changingState = true;
                            ((MState)mChasingState).SendEvent("Destroy");
                        }
                        else if (((MState)mChasingState).monster.MoveControl.shouldClimb)
                        {
                            mChasingState.changingState = true;
                            ((MState)mChasingState).SendEvent("Climb");
                        }
                        else if (((MState)mChasingState).monster.IsPlayerLocationKnown || ((MState)mChasingState).monster.WasFoundBySound())
                        {
                            ((MState)mChasingState).monster.GetAlertMeters.mSightAlert = 100f;
                            ((MState)mChasingState).monster.ShouldSearchRoom = false;
                            ((MState)mChasingState).Timeout.ResetTimer();
                            if (((MState)mChasingState).monster.IsPlayerLocationKnown)
                            {
                                ((MState)mChasingState).monster.Hearing.ClearLastHeardPosition();
                                ((MState)mChasingState).monster.Hearing.MarkAllSoundsAsExplored();
                            }
                            if (!ModSettings.invincibilityMode[crewPlayerIndex] && ((MState)mChasingState).monster.IsPlayerInAttackRange())
                            {
                                if ((((MState)mChasingState).monster.FoundPlayerBySound || ((MState)mChasingState).monster.IsPlayerLocationKnown || ((MState)mChasingState).monster.CanSeePlayer || ((MState)mChasingState).monster.CanSeeTorch) && (((MState)mChasingState).monster.IsPlayerHiding || ManyMonstersMode.PlayerToMonsterCast(((MState)mChasingState).monster)))
                                {
                                    NewPlayerClass monsterNewPlayerClass = ((MState)mChasingState).monster.player.GetComponent<NewPlayerClass>();
                                    if (!ModSettings.enableMultiplayer || (ModSettings.enableMultiplayer && MultiplayerMode.AllOtherPlayersDown(monsterNewPlayerClass)))
                                    {
                                        if (!ModSettings.PlayerHasLivesLeft())
                                        {
                                            if (((MState)mChasingState).monster.IsPlayerHiding)
                                            {
                                                mChasingState.spot = mChasingState.playerRoomDetect.PlayerHidingSpot(mChasingState.playerRoomDetect.GetRoom.HidingSpots);
                                                ChooseAttack.WhatAttackHiding(mChasingState.spot);

                                                if (ModSettings.enableCrewVSMonsterMode)
                                                {
                                                    CrewVsMonsterMode.playersGoneIntoHiding[MultiplayerMode.PlayerNumber(mChasingState.monster.PlayerDetectRoom.player.GetInstanceID())] = false;
                                                    foreach (Renderer renderer in mChasingState.monster.PlayerDetectRoom.player.GetComponentsInChildren<Renderer>())
                                                    {
                                                        renderer.enabled = true;
                                                    }
                                                    foreach (Light light in mChasingState.monster.PlayerDetectRoom.player.GetComponentsInChildren<Light>())
                                                    {
                                                        light.enabled = true;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                ManyMonstersMode.WhatAttackOpen(((MState)mChasingState).monster);
                                            }
                                            mChasingState.changingState = true;
                                            ((MState)mChasingState).SendEvent("AttackPlayer");
                                        }
                                        else
                                        {
                                            ModSettings.TakeLife(monsterNewPlayerClass.GetComponentInChildren<PlayerHealth>());
                                        }
                                    }
                                    else
                                    {
                                        MultiplayerMode.DownPlayer(monsterNewPlayerClass);
                                    }
                                }
                            }
                            else
                            {
                                mChasingState.lerpingToHidingSpot = false;
                                ((MState)mChasingState).StopCoroutine("LerpToHidingSpot");
                            }
                        }
                        else if (((MState)mChasingState).monster.InSearchableArea() && !((MState)mChasingState).monster.IsPlayerLocationKnown && (!ModSettings.enableCrewVSMonsterMode || CrewVsMonsterMode.letAIControlMonster))
                        {
                            mChasingState.changingState = true;
                            ((MState)mChasingState).SendEvent("Search Room");
                        }
                    }
                }
                else if (!ModSettings.seerMonster) // If the monster is not a seer monster, stop it from seeing the player when they have spawn protection
                {
                    if (ModSettings.logDebugText)
                    {
                        Debug.Log("Making monster lose sight");
                    }
                    ((MState)mChasingState).SendEvent("PlayerLoseSight");
                }
            }

            private static void HookMChasingStateStateChangesOld(On.MChasingState.orig_StateChanges orig, MChasingState mChasingState)
            {
                int crewPlayerIndex = 0;
                if (ModSettings.enableMultiplayer)
                {
                    crewPlayerIndex = MultiplayerMode.crewPlayers.IndexOf(mChasingState.monster.PlayerDetectRoom.player);
                }
                if (!ModSettings.spawnProtection[crewPlayerIndex])
                {
                    mChasingState.changingState = false;
                    if (((MState)mChasingState).monster.MoveControl.GetAniControl.DoARoar || ((MState)mChasingState).monster.MoveControl.GetAniControl.IsHaulted)
                    {
                        ((MState)mChasingState).monster.MoveControl.MaxSpeed = 0f;
                    }
                    else
                    {
                        /*
                        if (ModSettings.persistentMonster && ((MState)mChasingState).monster.EyeVision.CheckIfPlayer())
                        {
                            ((MState)mChasingState).monster.GetAlertMeters.mSightAlert = 100f;

                            // ((MState)mChasingState).monster.EyeVision.playerTotal = 1f; // Breaks the Fiend as they will always mind attack you.
                            // ((MState)mChasingState).monster.sawPlayerEnterHiding = true; // Does not work properly.
                        }
                        */
                        ((MState)mChasingState).monster.MoveControl.MaxSpeed = 100f;
                        if (ModSettings.persistentMonster)
                        {
                            // The sub event may stop persistent monster from working. However, it seems the monster automatically chases the player after the doors are opened in the sub event, so this may not actually be an issue.
                            if (((MState)mChasingState).monster.SubEventBeenStarted())
                            {
                                mChasingState.changingState = true;
                                ((MState)mChasingState).SendEvent("EventStarted");
                            }
                            else if (((MState)mChasingState).monster.IsMonsterDestroying)
                            {
                                mChasingState.changingState = true;
                                ((MState)mChasingState).SendEvent("Destroy");
                            }
                            else if (((MState)mChasingState).monster.MoveControl.shouldClimb)
                            {
                                mChasingState.changingState = true;
                                ((MState)mChasingState).SendEvent("Climb");
                            }
                            else
                            {
                                ((MState)mChasingState).monster.GetAlertMeters.mSightAlert = 100f;
                                ((MState)mChasingState).monster.ShouldSearchRoom = false;
                                ((MState)mChasingState).Timeout.ResetTimer();
                                if (((MState)mChasingState).monster.IsPlayerLocationKnown)
                                {
                                    ((MState)mChasingState).monster.Hearing.ClearLastHeardPosition();
                                    ((MState)mChasingState).monster.Hearing.MarkAllSoundsAsExplored();
                                }
                                if (!ModSettings.invincibilityMode[crewPlayerIndex] && ((MState)mChasingState).monster.IsPlayerInAttackRange())
                                {
                                    if ((((MState)mChasingState).monster.FoundPlayerBySound || ((MState)mChasingState).monster.IsPlayerLocationKnown || ((MState)mChasingState).monster.CanSeePlayer || ((MState)mChasingState).monster.CanSeeTorch) && (((MState)mChasingState).monster.IsPlayerHiding || ManyMonstersMode.PlayerToMonsterCast(((MState)mChasingState).monster)))
                                    {
                                        NewPlayerClass monsterNewPlayerClass = ((MState)mChasingState).monster.player.GetComponent<NewPlayerClass>();
                                        if (!ModSettings.enableMultiplayer || (ModSettings.enableMultiplayer && MultiplayerMode.AllOtherPlayersDown(monsterNewPlayerClass)))
                                        {
                                            if (!ModSettings.PlayerHasLivesLeft())
                                            {
                                                if (((MState)mChasingState).monster.IsPlayerHiding)
                                                {
                                                    mChasingState.spot = mChasingState.playerRoomDetect.PlayerHidingSpot(mChasingState.playerRoomDetect.GetRoom.HidingSpots);
                                                    ChooseAttack.WhatAttackHiding(mChasingState.spot);

                                                    if (ModSettings.enableCrewVSMonsterMode)
                                                    {
                                                        CrewVsMonsterMode.playersGoneIntoHiding[MultiplayerMode.PlayerNumber(mChasingState.monster.PlayerDetectRoom.player.GetInstanceID())] = false;
                                                        foreach (Renderer renderer in mChasingState.monster.PlayerDetectRoom.player.GetComponentsInChildren<Renderer>())
                                                        {
                                                            renderer.enabled = true;
                                                        }
                                                        foreach (Light light in mChasingState.monster.PlayerDetectRoom.player.GetComponentsInChildren<Light>())
                                                        {
                                                            light.enabled = true;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    ManyMonstersMode.WhatAttackOpen(((MState)mChasingState).monster);
                                                }
                                                mChasingState.changingState = true;
                                                ((MState)mChasingState).SendEvent("AttackPlayer");
                                            }
                                            else
                                            {
                                                ModSettings.TakeLife(monsterNewPlayerClass.GetComponentInChildren<PlayerHealth>());
                                            }
                                        }
                                        else
                                        {
                                            MultiplayerMode.DownPlayer(monsterNewPlayerClass);
                                        }
                                    }
                                }
                                else
                                {
                                    mChasingState.lerpingToHidingSpot = false;
                                    ((MState)mChasingState).StopCoroutine("LerpToHidingSpot");
                                }
                            }
                        }
                        else
                        {
                            if (((MState)mChasingState).monster.SubEventBeenStarted())
                            {
                                mChasingState.changingState = true;
                                ((MState)mChasingState).SendEvent("EventStarted");
                            }
                            else if (((MState)mChasingState).monster.IsMonsterRetreating)
                            {
                                mChasingState.changingState = true;
                                ((MState)mChasingState).SendEvent("Retreat");
                            }
                            else if (((MState)mChasingState).monster.IsMonsterDestroying)
                            {
                                mChasingState.changingState = true;
                                ((MState)mChasingState).SendEvent("Destroy");
                            }
                            else if (((MState)mChasingState).monster.MoveControl.shouldClimb)
                            {
                                mChasingState.changingState = true;
                                ((MState)mChasingState).SendEvent("Climb");
                            }
                            else if (((MState)mChasingState).monster.IsPlayerLocationKnown || ((MState)mChasingState).monster.WasFoundBySound())
                            {
                                ((MState)mChasingState).monster.GetAlertMeters.mSightAlert = 100f;
                                ((MState)mChasingState).monster.ShouldSearchRoom = false;
                                ((MState)mChasingState).Timeout.ResetTimer();
                                if (((MState)mChasingState).monster.IsPlayerLocationKnown)
                                {
                                    ((MState)mChasingState).monster.Hearing.ClearLastHeardPosition();
                                    ((MState)mChasingState).monster.Hearing.MarkAllSoundsAsExplored();
                                }
                                if (!ModSettings.invincibilityMode[crewPlayerIndex] && ((MState)mChasingState).monster.IsPlayerInAttackRange())
                                {
                                    if ((((MState)mChasingState).monster.FoundPlayerBySound || ((MState)mChasingState).monster.IsPlayerLocationKnown || ((MState)mChasingState).monster.CanSeePlayer || ((MState)mChasingState).monster.CanSeeTorch) && (((MState)mChasingState).monster.IsPlayerHiding || ManyMonstersMode.PlayerToMonsterCast(((MState)mChasingState).monster)))
                                    {
                                        NewPlayerClass monsterNewPlayerClass = ((MState)mChasingState).monster.player.GetComponent<NewPlayerClass>();
                                        if (!ModSettings.enableMultiplayer || (ModSettings.enableMultiplayer && MultiplayerMode.AllOtherPlayersDown(monsterNewPlayerClass)))
                                        {
                                            if (!ModSettings.PlayerHasLivesLeft())
                                            {
                                                if (((MState)mChasingState).monster.IsPlayerHiding)
                                                {
                                                    mChasingState.spot = mChasingState.playerRoomDetect.PlayerHidingSpot(mChasingState.playerRoomDetect.GetRoom.HidingSpots);
                                                    ChooseAttack.WhatAttackHiding(mChasingState.spot);

                                                    if (ModSettings.enableCrewVSMonsterMode)
                                                    {
                                                        CrewVsMonsterMode.playersGoneIntoHiding[MultiplayerMode.PlayerNumber(mChasingState.monster.PlayerDetectRoom.player.GetInstanceID())] = false;
                                                        foreach (Renderer renderer in mChasingState.monster.PlayerDetectRoom.player.GetComponentsInChildren<Renderer>())
                                                        {
                                                            renderer.enabled = true;
                                                        }
                                                        foreach (Light light in mChasingState.monster.PlayerDetectRoom.player.GetComponentsInChildren<Light>())
                                                        {
                                                            light.enabled = true;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    ManyMonstersMode.WhatAttackOpen(((MState)mChasingState).monster);
                                                }
                                                mChasingState.changingState = true;
                                                ((MState)mChasingState).SendEvent("AttackPlayer");
                                            }
                                            else
                                            {
                                                ModSettings.TakeLife(monsterNewPlayerClass.GetComponentInChildren<PlayerHealth>());
                                            }
                                        }
                                        else
                                        {
                                            MultiplayerMode.DownPlayer(monsterNewPlayerClass);
                                        }
                                    }
                                }
                                else
                                {
                                    mChasingState.lerpingToHidingSpot = false;
                                    ((MState)mChasingState).StopCoroutine("LerpToHidingSpot");
                                }
                            }
                            else if (((MState)mChasingState).monster.InSearchableArea() && !((MState)mChasingState).monster.IsPlayerLocationKnown && (!ModSettings.enableCrewVSMonsterMode || CrewVsMonsterMode.letAIControlMonster))
                            {
                                mChasingState.changingState = true;
                                ((MState)mChasingState).SendEvent("Search Room");
                            }
                        }
                    }
                }
                else
                {
                    if (!ModSettings.seerMonster) // If the monster is not a seer monster, stop it from seeing the player when they have spawn protection
                    {
                        ((MState)mChasingState).SendEvent("PlayerLoseSight");
                    }
                }
            }

            private static void HookMChasingStateChase(On.MChasingState.orig_Chase orig, MChasingState mChasingState, bool instantCalc)
            {
                if (ModSettings.persistentMonster)
                {
                    ((MState)mChasingState).monster.GetAlertMeters.mSightAlert = 100f;
                    ((MState)mChasingState).Timeout.ResetTimer();
                    ((MState)mChasingState).monster.TimeOutVision.ResetTimer();
                }
                ((MState)mChasingState).monster.IsDistracted = false;
                if (((MState)mChasingState).monster.IsPlayerLocationKnown || mChasingState.IsAmbushingPlayer || (((MState)mChasingState).monster.Hearing.SoundIsFromPlayer && ((MState)mChasingState).monster.GetAlertMeters.HighestAlert == "Sound" && ((MState)mChasingState).monster.Hearing.CurrentSoundSource != null && ((MState)mChasingState).monster.Hearing.CurrentSoundSource.Gameplay.canBeADistraction))
                {
                    mChasingState.chaseType = MChasingState.ChaseType.DirectChase;
                    mChasingState.range = 0f;
                    if (((MState)mChasingState).monster.IsPlayerHiding)
                    {
                        mChasingState.spot = mChasingState.playerRoomDetect.PlayerHidingSpot(mChasingState.playerRoomDetect.GetRoom.HidingSpots);
                        if (mChasingState.spot != null)
                        {
                            mChasingState.ChaseGoal(mChasingState.spot.MonsterPoint);
                        }
                    }
                    else
                    {
                        mChasingState.ChaseGoal(((MState)mChasingState).monster.player.transform.position + Vector3.up);
                    }
                }
                else if (((MState)mChasingState).monster.HasPointOfInterest)
                {
                    mChasingState.chaseType = MChasingState.ChaseType.TowardsDistraction;
                    ((MState)mChasingState).monster.IsDistracted = true;
                    ((MState)mChasingState).monster.MoveControl.GetAniControl.HeardASound = true;
                    mChasingState.range = 0f;
                    mChasingState.ChaseGoal(((MState)mChasingState).monster.Hearing.PointOfInterest + Vector3.up * 0.1f);
                }
                else if (((MState)mChasingState).monster.PersistChase)
                {
                    mChasingState.chaseType = MChasingState.ChaseType.DirectChase;
                    mChasingState.range = 0f;
                    if (((MState)mChasingState).monster.IsPlayerHiding)
                    {
                        mChasingState.spot = mChasingState.playerRoomDetect.PlayerHidingSpot(mChasingState.playerRoomDetect.GetRoom.HidingSpots);
                        if (mChasingState.spot != null)
                        {
                            mChasingState.ChaseGoal(mChasingState.spot.MonsterPoint);
                        }
                    }
                    else
                    {
                        mChasingState.ChaseGoal(((MState)mChasingState).monster.player.transform.position + Vector3.up);
                    }
                }
                else if (((MState)mChasingState).monster.ShouldSearchRoom && ((MState)mChasingState).monster.SightAlert > 99f)
                {
                    mChasingState.chaseType = MChasingState.ChaseType.ToRoom;
                    mChasingState.range = 0f;
                    mChasingState.ChaseGoal(((MState)mChasingState).monster.PlayerDetectRoom.GetRoom.RoomBounds.center);
                }
                else if (((MState)mChasingState).monster.GetAlertMeters.mSightAlert > 50f)
                {
                    if ((((MState)mChasingState).monster.IsAtEndOfPath || ((MState)mChasingState).monster.IsStuck) && (mChasingState.timeSincePathChange > 3f || mChasingState.chaseType != MChasingState.ChaseType.TowardsLastSeen))
                    {
                        mChasingState.chaseType = MChasingState.ChaseType.TowardsLastSeen;
                        mChasingState.range = (100f - ((MState)mChasingState).monster.SightAlert) / (((MState)mChasingState).monster.GetMonEffectiveness.EffectTotal * ((MState)mChasingState).monster.GetIntelligence);
                        mChasingState.range = Mathf.Clamp(mChasingState.range, 5f, 20f);
                        mChasingState.ChaseGoal(((MState)mChasingState).monster.LastSeenPlayerPosition + Vector3.up);
                    }
                }
                else if (!mChasingState.changingState && !ModSettings.persistentMonster)
                {
                    ((MState)mChasingState).SendEvent("PlayerLoseSight");
                    if (ModSettings.alternatingMonstersMode && ModSettings.numberOfMonsters > ModSettings.numberOfAlternatingMonsters && ((MState)mChasingState).monster.MonsterType != Monster.MonsterTypeEnum.Hunter)
                    {
                        TimeScaleManager.Instance.StartCoroutine(ManyMonstersMode.SwitchMonster(((MState)mChasingState).monster));
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MClimbingState

            private static void HookMClimbingStateOnEnter(On.MClimbingState.orig_OnEnter orig, MClimbingState mClimbingState)
            {
                ((Action)Activator.CreateInstance(typeof(Action), mClimbingState, typeof(MState).GetMethod("OnEnter").MethodHandle.GetFunctionPointer()))();
                mClimbingState.targeting = ((MState)mClimbingState).GetComponent<TargetMatching>();
                mClimbingState.typeofState = FSMState.StateTypes.IgnoreThis;
                ((MState)mClimbingState).monster.MoveControl.MaxSpeed = 0f;
                mClimbingState.t = 0f;
                mClimbingState.facethis = Vector3.zero;
                ((MState)mClimbingState).monster.MoveControl.LockRotation = true;
                ((MState)mClimbingState).monster.MoveControl.isClimbing = true;
                SetFaceThis(mClimbingState.currentClimb, mClimbingState.monster);
                ((MState)mClimbingState).monster.GetMainCollider.enabled = false;
                ((MState)mClimbingState).monster.PreviousWasClimb = true;
                if (mClimbingState.facethis != Vector3.zero)
                {
                    ((MState)mClimbingState).StartCoroutine(mClimbingState.LerpTo());
                }
                else
                {
                    // Reset values
                    ((MState)mClimbingState).monster.MoveControl.LockRotation = false;
                    ((MState)mClimbingState).monster.MoveControl.isClimbing = false;
                    ((MState)mClimbingState).monster.GetMainCollider.enabled = true;
                    ((MState)mClimbingState).monster.MoveControl.enabled = true;

                    // Persistent Monster reverse check
                    ReverseCheck(mClimbingState);
                }
            }

            private static void HookMClimbingStateFinishClimb(On.MClimbingState.orig_FinishClimb orig, MClimbingState mClimbingState)
            {
                ((MState)mClimbingState).StartCoroutine(mClimbingState.LerpToClimbPos());
                ((MState)mClimbingState).monster.MoveControl.isClimbing = false;
                ((MState)mClimbingState).monster.MoveControl.GetAniControl.climbUp = false;
                ((MState)mClimbingState).monster.MoveControl.shouldClimb = false;
                ((MState)mClimbingState).monster.MoveControl.LockRotation = false;
                ((MState)mClimbingState).monster.MoveControl.GetAniControl.inClimbingZone = false;
                mClimbingState.t = 0f;

                // Persistent Monster reverse check
                ReverseCheck(mClimbingState);
            }

            private static void ReverseCheck(MState mState)
            {
                if (mState.Fsm.Previous != null)
                {
                    mState.RevertState();
                }
                else
                {
                    mState.SendEvent("Idle");
                }
            }

            private static void SetFaceThis(ClimbCheck climbCheck, Monster monster)
            {
                MonsterClimbPoint monsterClimbPoint = null;
                climbCheck.minDistance = 99f;
                for (int i = 0; i < climbCheck.mClimbPoints.Count; i++)
                {
                    if (climbCheck.mClimbPoints[i].gameObject.activeSelf && climbCheck.mClimbPoints[i].canClimb)
                    {
                        climbCheck.mClimbPoints[i].distance = climbCheck.GetDistance(climbCheck.mClimbPoints[i].transform.position, monster.MoveControl.FarAheadNodePos());
                        if (climbCheck.mClimbPoints[i].distance < climbCheck.minDistance)
                        {
                            climbCheck.minDistance = climbCheck.mClimbPoints[i].distance;
                            monsterClimbPoint = climbCheck.mClimbPoints[i];
                        }
                    }
                }
                if (monsterClimbPoint != null)
                {
                    monster.Climber.closestClimb = monsterClimbPoint;
                    monster.Climber.FaceThis = monsterClimbPoint.transform.position;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MDestroyState

            private static void HookMDestroyStateOnEnter(On.MDestroyState.orig_OnEnter orig, MDestroyState mDestroyState)
            {
                Monster.MonsterTypeEnum monsterType = ((MState)mDestroyState).monster.MonsterType;
                if (ModSettings.applyDoorTeleportationToAllMonsters || (monsterType == Monster.MonsterTypeEnum.Fiend && ModSettings.fiendDoorTeleportation))
                {
                    FSMState previous = ((MState)mDestroyState).Fsm.Previous;
                    if (previous != null && previous.GetType() != typeof(MDoorBashState) && previous.GetType() != typeof(MFiendSubDoors) && ((MState)mDestroyState).monster.CurrentDoor != null && ((MState)mDestroyState).monster.CurrentDoor.DoorType != Door.doorType.Barricaded && ((MState)mDestroyState).monster.CurrentDoor.DoorType != Door.doorType.Locker && ((MState)mDestroyState).monster.CurrentDoor.DoorType != Door.doorType.Ripable && ((MState)mDestroyState).monster.CurrentDoor.DoorType != Door.doorType.Powered && ((MState)mDestroyState).monster.CurrentDoor.DoorType != Door.doorType.Sealed)
                    {
                        GameObject target = null;

                        GameObject doorClosestSide = ((MState)mDestroyState).monster.CurrentDoor.ClosestSide();
                        if (doorClosestSide != null)
                        {
                            if (monsterType != Monster.MonsterTypeEnum.Brute)
                            {
                                if (monsterType != Monster.MonsterTypeEnum.Hunter)
                                {
                                    if (monsterType == Monster.MonsterTypeEnum.Fiend)
                                    {
                                        target = doorClosestSide.transform.FindChild("Fiend").gameObject;
                                    }
                                }
                                else
                                {
                                    target = doorClosestSide.transform.FindChild("Hunter").gameObject;
                                }
                            }
                            else
                            {
                                target = doorClosestSide.transform.FindChild("Brute").gameObject;
                            }
                        }
                        if (target != null)
                        {
                            Vector3 normal = target.transform.forward;
                            Vector3 targetPosition = target.transform.position + 2f * normal;
                            normal.y = 0f;
                            normal.Normalize();
                            Quaternion targetRotation = Quaternion.LookRotation(normal, Vector3.up);
                            ((MState)mDestroyState).monster.transform.position = targetPosition;
                            ((MState)mDestroyState).monster.transform.rotation = targetRotation;
                            //Debug.Log("Running door teleportation code for monster type " + monsterType);
                        }
                        /*
                        else
                        {
                            Debug.Log("Not running door teleportation code for monster type " + monsterType + "(Loc 3)");
                        }
                        */
                        mDestroyState.LeaveState();
                        ((MState)mDestroyState).monster.InDestroyState = false;
                    }
                    else
                    {
                        //Debug.Log("Not running door teleportation code for monster type " + monsterType + "(Loc 2)");
                        orig.Invoke(mDestroyState);
                    }
                }
                else
                {
                    //Debug.Log("Not running door teleportation code for monster type " + monsterType + "(Loc 1)");
                    orig.Invoke(mDestroyState);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MenusEventSystemManager

            private static IEnumerator HookMenusEventSystemManagerDelay(On.MenusEventSystemManager.orig_Delay orig, MenusEventSystemManager menusEventSystemManager)
            {
                if (!ModSettings.skippedMenuScreen)
                {
                    yield return new WaitForSeconds(5f);
                }
                menusEventSystemManager.originalEventSystem.enabled = false;
                menusEventSystemManager.newEventSystem.enabled = true;
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MHuntingState

            private static void HookMHuntingState(On.MHuntingState.orig_StateChanges orig, MHuntingState mHuntingState)
            {
                if (((MState)mHuntingState).monster.SubEventBeenStarted())
                {
                    ((MState)mHuntingState).SendEvent("EventStarted");
                }
                else if (mHuntingState.shouldSetUpTrap || mHuntingState.NotSeenInAges() || (ModSettings.overpoweredHunter && MonsterStarter.spawned))
                {
                    mHuntingState.shouldSetUpTrap = false;
                    bool isPlayerOutside = false;
                    if (((MState)mHuntingState).monster.PlayerDetectRoom.GetRoomCategory == RoomCategory.Outside)
                    {
                        isPlayerOutside = true;
                    }
                    if (mHuntingState.ambushing || isPlayerOutside || mHuntingState.NotSeenInAges() || ModSettings.aggressiveHunter)
                    {
                        mHuntingState.trapper.ShouldSpawnImmediately = true;
                    }
                    else
                    {
                        mHuntingState.trapper.ShouldAmbush = false;
                        mHuntingState.trapper.ShouldSpawnImmediately = false;
                    }
                    ((MState)mHuntingState).SendEvent("Set Up Trap");
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Monster

            private static void HookMonster()
            {
                On.Monster.Awake += new On.Monster.hook_Awake(HookMonsterAwake);
            }

            public static void DisableMonsterParticles(GameObject monsterGameObject)
            {
                try
                {
                    ParticleSystem[] monsterParticleSystems = monsterGameObject.GetComponentsInChildren<ParticleSystem>();
                    if (monsterParticleSystems != null)
                    {
                        foreach (ParticleSystem particleSystem in monsterParticleSystems)
                        {
                            ParticleSystem.EmissionModule emissionModule = particleSystem.emission;
                            emissionModule.enabled = false;
                        }
                    }
                }
                catch
                {
                    Debug.Log("Error while trying to disable monster particles.");
                }
            }

            private static void HookMonsterAwake(On.Monster.orig_Awake orig, Monster monster)
            {
                //Debug.Log("Running Monster.Awake");
                orig.Invoke(monster);
                if (ModSettings.disableMonsterParticles)
                {
                    DisableMonsterParticles(monster.gameObject);
                    /*
                    try
                    {
                        Debug.Log("Listing monster objects");
                        UnityEngine.Object[] objects = ((MonoBehaviour)monster).GetComponentsInChildren<UnityEngine.Object>();
                        foreach (UnityEngine.Object objecta in objects)
                        {
                            Debug.Log("Object name is " + objecta.name);

                            if (objecta.name.Equals("Brute(Clone)") && monster.monsterType != null && monster.monsterType == "Brute")
                            {
                                //Debug.Log("Destroying Brute clone");
                                //Destroy(objecta);
                            }
                        }
                        Debug.Log("Done listing monster objects");
                    }
                    catch
                    {
                        Debug.Log("Could not list monster objects");
                    }

                    try
                    {
                        Debug.Log("Listing monster lights");
                        Light[] lights = ((MonoBehaviour)monster).GetComponentsInChildren<Light>();
                        foreach (Light light in lights)
                        {
                            Debug.Log("Light name is " + light.name);
                        }
                        Debug.Log("Done listing monster lights.");
                    }
                    catch
                    {
                        Debug.Log("Could not list monster lights");
                    }
                    */
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MonsterStarter

            private static void HookMonsterStarter()
            {
                On.MonsterStarter.OnTriggerEnter += new On.MonsterStarter.hook_OnTriggerEnter(HookMonsterStarterOnTriggerEnter);
                if (!ModSettings.startedWithMMM)
                {
                    On.MonsterStarter.Spawn += new On.MonsterStarter.hook_Spawn(HookMonsterStarterSpawn);
                }
            }

            private static void HookMonsterStarterOnTriggerEnter(On.MonsterStarter.orig_OnTriggerEnter orig, MonsterStarter monsterStarter, Collider other)
            {
                if (PlayerHelper.IsPlayerBody(other) && !monsterStarter.spawning)
                {
                    if (ModSettings.spawnMonsterInStarterRoom || ModSettings.extraLives > 0)
                    {
                        ModSettings.temporaryPlayerPosition = other.transform.position; //References.Player.transform.position; // Use collider instead in case of multiplayer where references player may be a monster.
                    }

                    // Only use these spawn times if there are only Hunters in the round.
                    if (monsterStarter.monster.GetComponent<Monster>().MonsterType == Monster.MonsterTypeEnum.Hunter && (ManyMonstersMode.hunters == null || ManyMonstersMode.hunters.Count == ModSettings.numberOfMonsters))
                    {
                        monsterStarter.minSpawnTime = 1f;
                        monsterStarter.maxSpawnTime = 2f;
                    }
                    ModSettings.VerifySpawnTimes(monsterStarter.minSpawnTime, monsterStarter.maxSpawnTime);
                    monsterStarter.spawnTime = UnityEngine.Random.Range(ModSettings.minSpawnTime, ModSettings.maxSpawnTime);

                    monsterStarter.GetMonsterStuff();
                    if ((monsterStarter.monster != null && monsterStarter.monster.activeInHierarchy) || ModSettings.numberOfMonsters > 1)
                    {
                        monsterStarter.Spawn();
                    }
                    else
                    {
                        ((MonoBehaviour)monsterStarter).Invoke("Spawn", monsterStarter.spawnTime);
                    }
                    monsterStarter.spawning = true;

                    if (ModSettings.debugMode)
                    {
                        ModSettings.ShowDebugModeText();
                    }
                    if (ModSettings.deathCountdown > 0)
                    {
                        if (ModSettings.enableMultiplayer)
                        {
                            foreach (NewPlayerClass newPlayerClass in MultiplayerMode.crewPlayers)
                            {
                                ((MonoBehaviour)TimeScaleManager.Instance).StartCoroutine(ModSettings.DeathCountDown(newPlayerClass.gameObject)); // Start the death countdown. Start the coroutine on the TimeScaleManager instead of the TutorialLockerModelSwap because that is about to be deleted.
                            }
                        }
                        else
                        {
                            ((MonoBehaviour)TimeScaleManager.Instance).StartCoroutine(ModSettings.DeathCountDown(References.Player));
                        }
                    }
                    if (ModSettings.useSpeedrunTimer)
                    {
                        Debug.Log("Speedrun timer when exiting starting room: " + Mathf.FloorToInt(ModSettings.speedrunTimer.TimeElapsed / 60f).ToString() + ":" + (ModSettings.speedrunTimer.TimeElapsed % 60f).ToString("00.000000") + ". This occurred at " + DateTime.Now + " local / " + DateTime.UtcNow + " UTC");
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
                    Vector3 position;
                    if (ModSettings.spawnMonsterInStarterRoom)
                    {
                        if (monsterStarter.monster.GetComponent<Monster>().MonsterType != Monster.MonsterTypeEnum.Hunter)
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
                        position = monsterStarter.ChooseSpawnPoint();
                    }
                    Monster monster = monsterStarter.monster.GetComponent<Monster>();
                    monsterStarter.monster.transform.position = position;
                    if (monsterStarter.chosenSpawn != null)
                    {
                        monsterStarter.monster.transform.localRotation = monsterStarter.chosenSpawn.transform.localRotation;
                    }
                    monster.GetAlertMeters.mSightAlert = 0f;
                    monster.GetAlertMeters.mProxAlert = 0f;
                    monster.GetAlertMeters.mSoundAlert = 0f;
                    monster.SetAIType();
                    monster.IsMonsterActive = true;
                    monsterStarter.monster.SetActive(true);
                    monster.MoveControl.SnapToFloor();
                    if (ModSettings.customMonsterScale != 1f)
                    {
                        monster.transform.localScale = new Vector3(ModSettings.customMonsterScale, ModSettings.customMonsterScale, ModSettings.customMonsterScale);
                    }
                    monsterStarter.timeActive.StartTimer();
                    if (ModSettings.seerMonster)
                    {
                        ModSettings.ForceChase(monster);
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MouseLock

            private static void HookMouseLockLateUpdate(On.MouseLock.orig_LateUpdate orig, MouseLock mouseLock)
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    //ModSettings.skipMenuScreen = false; // Stop the menu screen skip so that the menu can still be returned to properly after the game has been started.
                    orig.Invoke(mouseLock);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MouseLookCustom

            private static void HookMouseLookCustomStart(On.MouseLookCustom.orig_Start orig, MouseLookCustom mouseLookCustom)
            {
                orig.Invoke(mouseLookCustom);
                if (ModSettings.unlockPlayerHead)
                {
                    mouseLookCustom.maximumY = 90f;
                    mouseLookCustom.minimumY = -90f;
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
                            if (!mouseLookCustom.headLock)
                            {
                                if (ControllerCheck.enableControllerSupport)
                                {
                                    mouseLookCustom.rotationX += Input.GetAxis("Mouse X") * (mouseLookCustom.mouse_SensitivityX * (float)mouseLookCustom.invertX) + XboxCtrlrInput.XCI.RightStickValueX() * (mouseLookCustom.stick_SensitivityX * (float)mouseLookCustom.invertX);
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
                                if (ControllerCheck.enableControllerSupport)
                                {
                                    mouseLookCustom.rotationY += Input.GetAxis("Mouse Y") * (mouseLookCustom.mouse_SensitivityY * (float)mouseLookCustom.invertY) + XboxCtrlrInput.XCI.RightStickValueY() * (mouseLookCustom.stick_SensitivityY * (float)mouseLookCustom.invertY);
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

            /*----------------------------------------------------------------------------------------------------*/
            // @MovementControl

            // Only do this if Crew VS Monsters Mode is not active.
            private static void HookMovementControl()
            {
                On.MovementControl.LerpRotate += new On.MovementControl.hook_LerpRotate(HookMovementControlLerpRotate);
                On.MovementControl.WorkOutSpeed += new On.MovementControl.hook_WorkOutSpeed(HookMovementControlWorkOutSpeed);
            }

            private static void HookMovementControlWorkOutSpeed(On.MovementControl.orig_WorkOutSpeed orig, MovementControl movementControl)
            {
                CustomMovementControlWorkOutSpeed(movementControl);
            }

            private static IEnumerator SpeedCalculation(Transform transform)
            {
                Vector3 initialPosition = transform.position;
                yield return null;
                float speed = Vector3.Distance(transform.position, initialPosition) / Time.deltaTime;
                if (Time.deltaTime != 0f)
                {
                    Debug.Log("Transform is moving at speed " + speed);
                }
                yield break;
            }

            private static IEnumerator AverageSpeedCalculation(Transform transform, float sampleTime)
            {
                float timePassed = 0f;
                float averageSpeed = 0f;
                int numberOfSamples = 0;
                while (timePassed < sampleTime)
                {
                    Vector3 initialPosition = transform.position;
                    yield return null;
                    if (Time.deltaTime != 0f)
                    {
                        averageSpeed += Vector3.Distance(transform.position, initialPosition) / Time.deltaTime;
                        timePassed += Time.deltaTime;
                        numberOfSamples++;
                    }
                }
                averageSpeed /= numberOfSamples;
                Debug.Log("Transform is moving at average speed " + averageSpeed);
                yield break;
            }

            private static void HookMovementControlLerpRotate(On.MovementControl.orig_LerpRotate orig, MovementControl movementControl, Vector3 _targetPos, bool overwrite)
            {
                CustomMovementControlLerpRotate(movementControl, _targetPos, overwrite);
            }

            public static void CustomMovementControlLerpRotate(MovementControl movementControl, Vector3 _targetPos, bool overwrite)
            {
                float overwriteFactor = 1f;
                float rotationSpeed = movementControl.rotationSpeed;
                if (overwrite)
                {
                    overwriteFactor = 1.5f;
                }
                if ((!movementControl.LockRotation || overwrite) && !movementControl.isClimbing)
                {
                    float factoredRotationSpeed = movementControl.GetAniControl.CurrentAngle;
                    if (movementControl.AnimationSpeed > 50f || Mathf.Abs(factoredRotationSpeed) > 60f)
                    {
                        rotationSpeed *= 2.5f;
                    }
                    if (((factoredRotationSpeed < 120f && factoredRotationSpeed > -120f) || movementControl.monster.MonsterType == Monster.MonsterTypeEnum.Hunter) && factoredRotationSpeed != 0f)
                    {
                        factoredRotationSpeed /= rotationSpeed;
                        factoredRotationSpeed = 1f / factoredRotationSpeed;
                        factoredRotationSpeed *= Time.deltaTime * overwriteFactor;
                        movementControl.oldRotation = ((MonoBehaviour)movementControl).transform.localRotation;
                        Vector3 position = ((MonoBehaviour)movementControl).transform.position;
                        position.y = 0f;
                        Vector3 aheadNodePos = movementControl.AheadNodePos;
                        aheadNodePos.y = 0f;
                        Vector3 forward = aheadNodePos - position;
                        movementControl.newRotation = Quaternion.LookRotation(forward, Vector3.up);
                        float rotationSpeedFactor = ModSettings.monsterRotationSpeedMultiplier;
                        if (ModSettings.useSparky && movementControl.monster.monsterType.Equals("Sparky") && movementControl.monster.GetComponent<MState>().Fsm.Current.GetType() != typeof(MLurkState))
                        {
                            rotationSpeedFactor += ModSettings.sparkyRotationSpeedFactor;
                        }
                        factoredRotationSpeed *= rotationSpeedFactor;
                        Quaternion localRotation = Quaternion.Slerp(movementControl.oldRotation, movementControl.newRotation, factoredRotationSpeed);
                        if (!float.IsNaN(localRotation.x))
                        {
                            ((MonoBehaviour)movementControl).transform.localRotation = localRotation;
                        }
                    }
                }
            }

            public static void CustomMovementControlWorkOutSpeed(MovementControl movementControl)
            {
                // ((MonoBehaviour)movementControl).StartCoroutine(AverageSpeedCalculation(((MonoBehaviour)movementControl).gameObject.transform, 3f)); // See Naiden chat for monster speeds.
                bool shouldMonsterSlowDown = false;
                Ray ray = new Ray(((MonoBehaviour)movementControl).gameObject.GetComponent<Collider>().ClosestPointOnBounds(movementControl.SnapToFloorPositions[movementControl.SnapToFloorPositions.Count - 1].transform.position) + Vector3.up * 0.25f, ((MonoBehaviour)movementControl).transform.forward);
                float monsterMaxSpeed = movementControl.MaxSpeed;
                float raycastHitDistance = 0f;
                movementControl.aheadRayDistance = Mathf.Lerp(movementControl.minRayDistance, movementControl.maxRayDistance, movementControl.MaxSpeed / 100f);
                RaycastHit raycastHit;
                if (movementControl.path == null || (movementControl.path != null && movementControl.path.Count < 1))
                {
                    monsterMaxSpeed = 0f;
                    shouldMonsterSlowDown = true;
                }
                else if (Vector3.Distance(((MonoBehaviour)movementControl).transform.position, movementControl.CurrentNodePos) > 5f)
                {
                    monsterMaxSpeed = 0f;
                    shouldMonsterSlowDown = true;
                }
                else if (movementControl.GetAniControl.CurrentAngle > 120f)
                {
                    monsterMaxSpeed = 0f;
                    shouldMonsterSlowDown = false;
                }
                else if (movementControl.IsCloseToGoal() && !movementControl.monster.CanSeePlayer)
                {
                    if (movementControl.IsAtDestination)
                    {
                        monsterMaxSpeed = 0f;
                        shouldMonsterSlowDown = true;
                    }
                    else
                    {
                        monsterMaxSpeed = 15f;
                    }
                }
                else if (movementControl.shouldClimb || movementControl.shouldJumpDown)
                {
                    monsterMaxSpeed = 0f;
                    shouldMonsterSlowDown = true;
                }
                else if ((movementControl.IsStuck || movementControl.path == null) && movementControl.monster.Pathfinding.IsComplete)
                {
                    monsterMaxSpeed = 0f;
                    shouldMonsterSlowDown = true;
                }
                else if (Physics.Raycast(ray, out raycastHit, movementControl.aheadRayDistance, movementControl.obstacleMask) && raycastHit.distance < movementControl.aheadRayDistance && movementControl.AnimationSpeed > 0f)
                {
                    raycastHitDistance = raycastHit.distance;
                    if (!movementControl.atStairs && !movementControl.monster.RoomDetect.CurrentRoom.HasTag("Stairs"))
                    {
                        movementControl.facingWallTimer += Time.deltaTime;
                        monsterMaxSpeed = Mathf.Lerp(0f, movementControl.MaxSpeed, raycastHitDistance / movementControl.aheadRayDistance);
                    }
                    else
                    {
                        movementControl.facingWallTimer = 0f;
                    }
                    if (movementControl.facingWallTimer > 3f)
                    {
                        movementControl.MaxSpeed = 0f;
                        monsterMaxSpeed = 0f;
                        shouldMonsterSlowDown = true;
                    }
                }
                else
                {
                    movementControl.facingWallTimer = 0f;
                    monsterMaxSpeed = movementControl.MaxSpeed;
                }
                if (movementControl.AnimationSpeed > monsterMaxSpeed)
                {
                    if (shouldMonsterSlowDown)
                    {
                        movementControl.monster.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
                        movementControl.AnimationSpeed = Mathf.MoveTowards(movementControl.AnimationSpeed, 0f, Time.deltaTime * 75f);
                    }
                    else if (raycastHitDistance != 0f)
                    {
                        movementControl.monster.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
                        movementControl.AnimationSpeed = Mathf.MoveTowards(movementControl.AnimationSpeed, monsterMaxSpeed * ModSettings.monsterMovementSpeedMultiplier, Time.deltaTime * 100f);
                    }
                    else
                    {
                        movementControl.monster.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
                        movementControl.AnimationSpeed = Mathf.MoveTowards(movementControl.AnimationSpeed, monsterMaxSpeed * ModSettings.monsterMovementSpeedMultiplier, Time.deltaTime * 30f);
                    }
                }
                else
                {
                    movementControl.monster.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
                    movementControl.AnimationSpeed = Mathf.MoveTowards(movementControl.AnimationSpeed, monsterMaxSpeed * ModSettings.monsterMovementSpeedMultiplier, Time.deltaTime * 50f);
                }
                if (movementControl.AnimationSpeed > 50f)
                {
                    movementControl.HunterMoveType = "Crawl";
                }
                else
                {
                    movementControl.HunterMoveType = "Walk";
                }

                MovementControlBonusSpeedCheck(movementControl);
            }

            public static void MovementControlBonusSpeedCheck(MovementControl movementControl)
            {
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    if (ModSettings.applyChaseSpeedBuffToAllMonsters || (ModSettings.bruteChaseSpeedBuff && movementControl.monster.MonsterType == Monster.MonsterTypeEnum.Brute) || (ModSettings.useSparky && movementControl.monster.monsterType.Equals("Sparky")))
                    {
                        FSMState.StateTypes stateType = movementControl.monster.GetComponent<FSM>().Current.typeofState;
                        float sparkySpeedFactor = 1f;
                        if (movementControl.monster.monsterType.Equals("Sparky"))
                        {
                            if (stateType == FSMState.StateTypes.Chase)
                            {
                                sparkySpeedFactor += ModSettings.sparkyChaseFactor;
                                if (ModSettings.applyChaseSpeedBuffToAllMonsters)
                                {
                                    sparkySpeedFactor += ModSettings.bruteChaseSpeedBuffMultiplier - 1f;
                                }
                                sparkySpeedFactor += ModSettings.sparkyMaxChaseFactorIncreaseFromBuff * movementControl.monster.GetComponent<SparkyAura>().buffPercentage;
                                sparkySpeedFactor /= ModSettings.bruteChaseSpeedBuffMultiplier;
                            }
                            else
                            {
                                sparkySpeedFactor += ModSettings.sparkyMaxSpeedFactorIncreaseFromBuff * movementControl.monster.GetComponent<SparkyAura>().buffPercentage;
                            }
                        }
                        if (movementControl.AnimationSpeed > 95f && stateType == FSMState.StateTypes.Chase)
                        {
                            movementControl.animController.monsterAnimation.speed = Mathf.MoveTowards(movementControl.animController.monsterAnimation.speed, ModSettings.bruteChaseSpeedBuffMultiplier * ModSettings.monsterAnimationSpeedMultiplier * sparkySpeedFactor, Time.deltaTime / 15f);
                        }
                        else
                        {
                            movementControl.animController.monsterAnimation.speed = Mathf.MoveTowards(movementControl.animController.monsterAnimation.speed, ModSettings.monsterAnimationSpeedMultiplier * sparkySpeedFactor, Time.deltaTime / 5f);
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MRoomSearch

            private static float HookMRoomSearchChanceOfFindingPlayer(On.MRoomSearch.orig_ChanceOfFindingPlayer orig, MRoomSearch mRoomSearch)
            {
                if (ModSettings.monsterAlwaysFindsYou)
                {
                    return 1f;
                }
                return orig.Invoke(mRoomSearch);
            }

            private static void HookMRoomSearchOnEnter(On.MRoomSearch.orig_OnEnter orig, MRoomSearch mRoomSearch)
            {
                if (ModSettings.noHiding)
                {
                    ModSettings.ForceChase(mRoomSearch.monster);
                }
                orig.Invoke(mRoomSearch);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @MWanderState

            private static void HookMWanderStateOnEnter(On.MWanderState.orig_OnEnter orig, MWanderState mWanderState)
            {
                ((Action)Activator.CreateInstance(typeof(Action), mWanderState, typeof(MState).GetMethod("OnEnter").MethodHandle.GetFunctionPointer()))();
                mWanderState.typeofState = FSMState.StateTypes.LowAlert;
                ((MState)mWanderState).monster.ShouldSearchRoom = false;
                mWanderState.SpeedControl();
                if (!((MState)mWanderState).monster.PreviousWasDestroy && !((MState)mWanderState).monster.PreviousWasClimb)
                {
                    Vector3 targetPosition;
                    if (ModSettings.monstersSearchRandomly)
                    {
                        targetPosition = RandomShipPosition(/*((MState)mWanderState).monster.transform.position*/);
                        //Debug.Log("Random ship position is " + targetPosition + " / " + RegionManager.Instance.ConvertPointToRegionNode(targetPosition));
                    }
                    else
                    {
                        targetPosition = mWanderState.monster.player.transform.position;
                    }
                    targetPosition += Vector3.up * 0.25f;
                    ((MState)mWanderState).monster.Patrol.GetRooms(mWanderState.moddedRange, targetPosition, PatrolPoints.PatrolType.PatrolPoint, mWanderState.floorChange);
                }
            }

            private static Vector3 RandomShipPosition(/*Vector3 monsterPosition*/)
            {
                /*
                Vector3 randomNode = Vector3.zero;
                bool accessible = false;
                while (!accessible)
                {
                    randomNode = new Vector3(UnityEngine.Random.Range(0f, Settings.ShipCubesCount.x), UnityEngine.Random.Range(0f, Settings.ShipCubesCount.y), UnityEngine.Random.Range(0f, Settings.ShipCubesCount.z));
                    if (!RegionManager.Instance.CheckNodeForRegion(randomNode, 0)) // 0 is inaccessible.
                    {
                        accessible = true;
                    }
                }
                return RegionManager.Instance.ConvertRegionNodeToShipWorldSpace(randomNode);
                */

                return RegionManager.Instance.ConvertRegionNodeToShipWorldSpace(ModSettings.accessibleNodes[UnityEngine.Random.Range(0, ModSettings.accessibleNodes.Count)]);

                /*
                Vector3 randomPosition;
                do
                {
                    randomPosition = RegionManager.Instance.ConvertRegionNodeToShipWorldSpace(ModSettings.accessibleNodes[UnityEngine.Random.Range(0, ModSettings.accessibleNodes.Count)]);
                }
                while (Vector3.Distance(monsterPosition, randomPosition) > 25f);
                return randomPosition;
                */
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @NewPlayerClass

            private static void HookNewPlayerClassButtonInput(On.NewPlayerClass.orig_ButtonInput orig, NewPlayerClass newPlayerClass)
            {
                orig.Invoke(newPlayerClass);
                if (ModSettings.disableRunning)
                {
                    newPlayerClass.playerRunning = false;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @NoCullingAppendage

            private static void HookNoCullingAppendageCheckNoCullingJoints(On.NoCullingAppendage.orig_CheckNoCullingJoints orig, Room _room)
            {
                foreach (NoCullingAppendage noCullingAppendage in _room.noCullingData)
                {
                    noCullingAppendage.regionNode = RoomAppendageData.ConvertAppendageToRegionSpace(_room, noCullingAppendage);
                    int num = noCullingAppendage.rotationQuadrant + _room.rotationQuadrant;
                    if (num >= 4)
                    {
                        num -= 4;
                    }
                    noCullingAppendage.finalRotationQuadrant = num;
                    bool outOfRange = false;
                    if ((int)noCullingAppendage.regionNode.x >= LevelGeneration.Instance.nodeData.Count || (int)noCullingAppendage.regionNode.x < 0)
                    {
                        outOfRange = true;
                    }
                    else if ((int)noCullingAppendage.regionNode.y >= LevelGeneration.Instance.nodeData[(int)noCullingAppendage.regionNode.x].Count || (int)noCullingAppendage.regionNode.y < 0)
                    {
                        outOfRange = true;
                    }
                    else if ((int)noCullingAppendage.regionNode.z >= LevelGeneration.Instance.nodeData[(int)noCullingAppendage.regionNode.x][(int)noCullingAppendage.regionNode.y].Count || (int)noCullingAppendage.regionNode.z < 0)
                    {
                        outOfRange = true;
                    }
                    if (!outOfRange)
                    {
                        NodeData nodeData = LevelGeneration.Instance.nodeData[(int)noCullingAppendage.regionNode.x][(int)noCullingAppendage.regionNode.y][(int)noCullingAppendage.regionNode.z];
                        for (int i = 0; i < nodeData.appendageData.Count; i++)
                        {
                            if (nodeData.appendageData[i].currentOrientation == noCullingAppendage.currentOrientation)
                            {
                                nodeData.appendageData.RemoveAt(i);
                                i--;
                                if (noCullingAppendage.currentOrientation == Orientation.Horizontal)
                                {
                                    nodeData.connectedNodesUDLRFB[5] = false;
                                    LevelGeneration.Instance.nodeData[(int)noCullingAppendage.regionNode.x][(int)noCullingAppendage.regionNode.y][(int)noCullingAppendage.regionNode.z - 1].connectedNodesUDLRFB[4] = false;
                                }
                                if (noCullingAppendage.currentOrientation == Orientation.Vertical)
                                {
                                    nodeData.connectedNodesUDLRFB[2] = false;
                                    LevelGeneration.Instance.nodeData[(int)noCullingAppendage.regionNode.x - 1][(int)noCullingAppendage.regionNode.y][(int)noCullingAppendage.regionNode.z].connectedNodesUDLRFB[3] = false;
                                }
                            }
                        }
                    }
                }
                /*
                foreach (NoCullingAppendage noCullingAppendage in _room.noCullingData)
                {
                    try
                    {
                        noCullingAppendage.regionNode = RoomAppendageData.ConvertAppendageToRegionSpace(_room, noCullingAppendage);
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Error in NoCullingAppendage.CheckNoCullingJoints 1:\n" + e.ToString());
                    }
                    try
                    {
                        int num = noCullingAppendage.rotationQuadrant + _room.rotationQuadrant;
                        if (num >= 4)
                        {
                            num -= 4;
                        }
                        noCullingAppendage.finalRotationQuadrant = num;
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Error in NoCullingAppendage.CheckNoCullingJoints 2:\n" + e.ToString());
                    }
                    bool outOfRange = false;
                    // Might not log because this is called inside an IEnumerator method.
                    if ((int)noCullingAppendage.regionNode.x >= LevelGeneration.Instance.nodeData.Count || (int)noCullingAppendage.regionNode.x < 0)
                    {
                        Debug.Log("x = " + (int)noCullingAppendage.regionNode.x + " is out of range of the map!");
                        outOfRange = true;
                        //throw new Exception("x = " + (int)noCullingAppendage.regionNode.x + " is out of range of the map!");
                    }
                    else if ((int)noCullingAppendage.regionNode.y >= LevelGeneration.Instance.nodeData[(int)noCullingAppendage.regionNode.x].Count || (int)noCullingAppendage.regionNode.y < 0)
                    {
                        Debug.Log("y = " + (int)noCullingAppendage.regionNode.y + " is out of range of the map!");
                        outOfRange = true;
                        //throw new Exception("y = " + (int)noCullingAppendage.regionNode.y + " is out of range of the map!");
                    }
                    else if ((int)noCullingAppendage.regionNode.z >= LevelGeneration.Instance.nodeData[(int)noCullingAppendage.regionNode.x][(int)noCullingAppendage.regionNode.y].Count || (int)noCullingAppendage.regionNode.z < 0)
                    {
                        Debug.Log("z = " + (int)noCullingAppendage.regionNode.z + " is out of range of the map!");
                        outOfRange = true;
                        //throw new Exception("z = " + (int)noCullingAppendage.regionNode.z + " is out of range of the map!");
                    }
                    if (!outOfRange)
                    {
                        NodeData nodeData = LevelGeneration.Instance.nodeData[(int)noCullingAppendage.regionNode.x][(int)noCullingAppendage.regionNode.y][(int)noCullingAppendage.regionNode.z];
                        try
                        {
                            for (int i = 0; i < nodeData.appendageData.Count; i++)
                            {
                                try
                                {
                                    if (nodeData.appendageData[i].currentOrientation == noCullingAppendage.currentOrientation)
                                    {
                                        try
                                        {
                                            nodeData.appendageData.RemoveAt(i);
                                            i--;
                                        }
                                        catch (Exception e)
                                        {
                                            Debug.Log("Error in NoCullingAppendage.CheckNoCullingJoints 5:\n" + e.ToString());
                                        }
                                        try
                                        {
                                            if (noCullingAppendage.currentOrientation == Orientation.Horizontal)
                                            {
                                                try
                                                {
                                                    nodeData.connectedNodesUDLRFB[5] = false;

                                                }
                                                catch (Exception e)
                                                {
                                                    Debug.Log("Error in NoCullingAppendage.CheckNoCullingJoints 8:\n" + e.ToString());
                                                }
                                                try
                                                {
                                                    LevelGeneration.Instance.nodeData[(int)noCullingAppendage.regionNode.x][(int)noCullingAppendage.regionNode.y][(int)noCullingAppendage.regionNode.z - 1].connectedNodesUDLRFB[4] = false;
                                                }
                                                catch (Exception e)
                                                {
                                                    Debug.Log("Error in NoCullingAppendage.CheckNoCullingJoints 9:\n" + e.ToString());
                                                }
                                            }

                                        }
                                        catch (Exception e)
                                        {
                                            Debug.Log("Error in NoCullingAppendage.CheckNoCullingJoints 6:\n" + e.ToString());
                                        }
                                        try
                                        {
                                            if (noCullingAppendage.currentOrientation == Orientation.Vertical)
                                            {
                                                try
                                                {
                                                    nodeData.connectedNodesUDLRFB[2] = false;
                                                }
                                                catch (Exception e)
                                                {
                                                    Debug.Log("Error in NoCullingAppendage.CheckNoCullingJoints 10:\n" + e.ToString());
                                                }
                                                try
                                                {
                                                    LevelGeneration.Instance.nodeData[(int)noCullingAppendage.regionNode.x - 1][(int)noCullingAppendage.regionNode.y][(int)noCullingAppendage.regionNode.z].connectedNodesUDLRFB[3] = false;
                                                }
                                                catch (Exception e)
                                                {
                                                    Debug.Log("Error in NoCullingAppendage.CheckNoCullingJoints 11:\n" + e.ToString());
                                                }
                                            }

                                        }
                                        catch (Exception e)
                                        {
                                            Debug.Log("Error in NoCullingAppendage.CheckNoCullingJoints 7:\n" + e.ToString());
                                        }
                                    }
                                }
                                catch (Exception e)
                                {
                                    Debug.Log("Error in NoCullingAppendage.CheckNoCullingJoints 4:\n" + e.ToString());
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Error in NoCullingAppendage.CheckNoCullingJoints 3:\n" + e.ToString());
                        }
                    }
                }
                */
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @PauseMenu

            public static bool pausedWithoutTimeFreeze = false;

            private static void HookPauseMenuTogglePause(On.PauseMenu.orig_TogglePause orig, PauseMenu pauseMenu)
            {
                pauseMenu.objects = UnityEngine.Object.FindObjectsOfType(typeof(GameObject));
                if ((!ModSettings.noTimeFreezeInPauseMenu && !pauseMenu.pause) || (ModSettings.noTimeFreezeInPauseMenu && !pausedWithoutTimeFreeze))
                {
                    pauseMenu.pausedSources.Clear();

                    if (!ModSettings.noTimeFreezeInPauseMenu)
                    {
                        foreach (GameObject gameObject in pauseMenu.objects)
                        {
                            if (gameObject != null)
                            {
                                AudioSource[] components = gameObject.GetComponents<AudioSource>();
                                if (components.Length > 0)
                                {
                                    for (int j = 0; j < components.Length; j++)
                                    {
                                        if (components[j] != null && components[j].isPlaying)
                                        {
                                            components[j].Pause();
                                            pauseMenu.pausedSources.Add(components[j]);
                                        }
                                    }
                                }
                                gameObject.SendMessage("OnPauseGame", SendMessageOptions.DontRequireReceiver);
                            }
                        }
                    }
                    else
                    {
                        foreach (MouseLock gameObject in FindObjectsOfType<MouseLock>())
                        {
                            gameObject.SendMessage("OnPauseGame", SendMessageOptions.DontRequireReceiver);
                        }
                        foreach (OculusPauseUI gameObject in FindObjectsOfType<OculusPauseUI>())
                        {
                            gameObject.SendMessage("OnPauseGame", SendMessageOptions.DontRequireReceiver);
                        }
                        foreach (PauseOculusCube gameObject in FindObjectsOfType<PauseOculusCube>())
                        {
                            gameObject.SendMessage("OnPauseGame", SendMessageOptions.DontRequireReceiver);
                        }
                        foreach (PauseMenu gameObject in FindObjectsOfType<PauseMenu>())
                        {
                            gameObject.SendMessage("OnPauseGame", SendMessageOptions.DontRequireReceiver);
                        }
                        foreach (OculusTutorialPromptsManager gameObject in FindObjectsOfType<OculusTutorialPromptsManager>())
                        {
                            gameObject.SendMessage("OnPauseGame", SendMessageOptions.DontRequireReceiver);
                        }
                        foreach (OculusInputMisc gameObject in FindObjectsOfType<OculusInputMisc>())
                        {
                            gameObject.SendMessage("OnPauseGame", SendMessageOptions.DontRequireReceiver);
                        }
                        foreach (GraphicsButton gameObject in FindObjectsOfType<GraphicsButton>())
                        {
                            gameObject.SendMessage("OnPauseGame", SendMessageOptions.DontRequireReceiver);
                        }
                        foreach (OVRControllerManager gameObject in FindObjectsOfType<OVRControllerManager>())
                        {
                            gameObject.SendMessage("OnPauseGame", SendMessageOptions.DontRequireReceiver);
                        }
                        foreach (MouseLookCustom gameObject in FindObjectsOfType<MouseLookCustom>())
                        {
                            gameObject.SendMessage("OnPauseGame", SendMessageOptions.DontRequireReceiver);
                        }
                        foreach (OculusReticule gameObject in FindObjectsOfType<OculusReticule>())
                        {
                            gameObject.SendMessage("OnPauseGame", SendMessageOptions.DontRequireReceiver);
                        }
                        foreach (OculusMainSceneControl gameObject in FindObjectsOfType<OculusMainSceneControl>())
                        {
                            gameObject.SendMessage("OnPauseGame", SendMessageOptions.DontRequireReceiver);
                        }
                    }
                    pauseMenu.Show();
                    pauseMenu.PauseGame();
                    for (int k = 0; k < pauseMenu.listDPAD.Count; k++)
                    {
                        pauseMenu.listDPAD[k].DisableSelected();
                    }
                }
                else
                {
                    for (int l = 0; l < pauseMenu.listDPAD.Count; l++)
                    {
                        pauseMenu.listDPAD[l].DisableSelected();
                    }
                    foreach (GameObject gameObject2 in pauseMenu.objects)
                    {
                        if (gameObject2 != null)
                        {
                            gameObject2.SendMessage("OnExitPauseGame", SendMessageOptions.DontRequireReceiver);
                        }
                    }
                    foreach (AudioSource audioSource in pauseMenu.pausedSources)
                    {
                        if (audioSource != null)
                        {
                            audioSource.Play();
                        }
                    }
                    pauseMenu.UnPauseGame();
                    pauseMenu.Hide();
                    pauseMenu.EnableToggle();
                }
                if (!ModSettings.noTimeFreezeInPauseMenu)
                {
                    pauseMenu.pause = !pauseMenu.pause;
                }
                else
                {
                    pausedWithoutTimeFreeze = !pausedWithoutTimeFreeze;
                }
            }

            private static void HookPauseMenuUpdate(On.PauseMenu.orig_Update orig, PauseMenu pauseMenu)
            {
                if (pauseMenu.fadeIn.fadeInComplete && pauseMenu.enableToggle)
                {
                    if ((!ModSettings.noTimeFreezeInPauseMenu && !pauseMenu.pause) || (ModSettings.noTimeFreezeInPauseMenu && !pausedWithoutTimeFreeze))
                    {
                        if (MouseLock.Instance.IsLocked && (Input.GetKeyDown(KeyCode.Escape) || XboxCtrlrInput.XCI.GetButtonDown(XboxCtrlrInput.XboxButton.Start) || pauseMenu.pauseFromOverlay))
                        {
                            pauseMenu.pauseFromOverlay = false;
                            pauseMenu.TogglePause();
                        }
                    }
                    else if (Input.GetKeyDown(KeyCode.Escape) || XboxCtrlrInput.XCI.GetButtonDown(XboxCtrlrInput.XboxButton.Start))
                    {
                        if (pauseMenu.optionsEnabled)
                        {
                            pauseMenu.pauseButtons.SetActive(true);
                            pauseMenu.optionsButtons.SetActive(false);
                            pauseMenu.optionsEnabled = false;
                        }
                        else
                        {
                            pauseMenu.TogglePause();
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @PitTrap

            private static void HookPitTrapDestroyFloor(On.PitTrap.orig_DestroyFloor orig, PitTrap pitTrap, string _reason)
            {
                if (!ModSettings.unbreakablePitTraps)
                {
                    orig.Invoke(pitTrap, _reason);
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

            /*----------------------------------------------------------------------------------------------------*/
            // @PlayerHealth

            private static void HookPlayerHealth()
            {
                On.PlayerHealth.CheckIfDead += new On.PlayerHealth.hook_CheckIfDead(HookPlayerHealthCheckIfDead);
                On.PlayerHealth.DoDamage += new On.PlayerHealth.hook_DoDamage(HookPlayerHealthDoDamage);
            }

            private static void HookPlayerHealthCheckIfDead(On.PlayerHealth.orig_CheckIfDead orig, PlayerHealth playerHealth)
            {
                if (playerHealth.currentHP <= 0f)
                {
                    if (!ModSettings.enableMultiplayer || (ModSettings.enableMultiplayer && MultiplayerMode.AllOtherPlayersDown(playerHealth.NPC)))
                    {
                        if (!ModSettings.PlayerHasLivesLeft())
                        {
                            playerHealth.dead = true;
                        }
                        else
                        {
                            ModSettings.TakeLife(playerHealth);
                        }
                    }
                    else
                    {
                        MultiplayerMode.DownPlayer(playerHealth.NPC);
                    }
                }
                if (playerHealth.dead)
                {
                    if (!playerHealth.deathStarted)
                    {
                        playerHealth.deathStarted = true;
                        playerHealth.SelectAnimation();
                        if (!ModSettings.enableMultiplayer)
                        {
                            References.Inventory.DropAndDestroy();
                        }
                        else
                        {
                            MultiplayerMode.inventories[MultiplayerMode.PlayerNumber(playerHealth.NPC.GetInstanceID())].DropAndDestroy();
                        }
                    }
                    MultiplayerMode.SetPlayerLayers(playerHealth.NPC);
                    playerHealth.NPC.LockEverything();
                    playerHealth.upperBodyLock.weighting3 -= Time.deltaTime * 3f;
                }
            }

            private static void HookPlayerHealthDoDamage(On.PlayerHealth.orig_DoDamage orig, PlayerHealth playerHealth, float damageVal, bool overTime, PlayerHealth.DamageTypes DMG, bool isAreaEffectDamage)
            {
                int crewPlayerIndex = 0;
                if (ModSettings.enableMultiplayer)
                {
                    crewPlayerIndex = MultiplayerMode.crewPlayers.IndexOf(playerHealth.NPC);
                }
                if (ModSettings.logDebugText)
                {
                    Debug.Log("Damaged by type: " + DMG + " with damage " + damageVal + ". Stack Trace:\n" + new StackTrace().ToString());
                }
                if (!ModSettings.invincibilityMode[crewPlayerIndex])
                {
                    orig.Invoke(playerHealth, damageVal, overTime, DMG, isAreaEffectDamage);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @PlayerMotor

            private static void HookPlayerMotorClampSpeed(On.PlayerMotor.orig_ClampSpeed orig, PlayerMotor playerMotor)
            {
                // Debug.Log("Format is (x,z). Player crouch speed is: (" + playerMotor.crouchX + "," + playerMotor.crouchZ + "). Player walk speed is: (" + playerMotor.walkX + "," + playerMotor.walkZ + "). Player run speed is: (" + playerMotor.runX + "," + playerMotor.runZ + "). Player prone speed is: (" + playerMotor.proneX + "," + playerMotor.proneZ + "). Player backwards penalty is: " + playerMotor.backwardsPenaltyMultiplier + ".");
                // Format is (x,z). Player crouch speed is: (1.3,1.5). Player walk speed is: (1.5,1.7). Player run speed is: (3.5,4.4). Player prone speed is: (0,0). Player backwards penalty is: 0.57.
                int playerNumber = 0;
                if (ModSettings.enableMultiplayer)
                {
                    if (MultiplayerMode.crewPlayers.Contains(playerMotor.player))
                    {
                        playerNumber = MultiplayerMode.crewPlayers.IndexOf(playerMotor.player); // This is not really needed as the player motor of monster players is not updated, but is still a useful check in case it ever is.
                    }
                }
                float xMax = playerMotor.walkX * ModSettings.playerMovementSpeedMultiplier[playerNumber];
                float zMax = playerMotor.walkZ * ModSettings.playerMovementSpeedMultiplier[playerNumber];
                PlayerMotor.Pose playerPose = playerMotor.pose;
                if (playerPose != PlayerMotor.Pose.Run)
                {
                    if (playerPose != PlayerMotor.Pose.Prone)
                    {
                        if (playerPose == PlayerMotor.Pose.Crouch)
                        {
                            xMax = playerMotor.crouchX * ModSettings.playerMovementSpeedMultiplier[playerNumber];
                            zMax = playerMotor.crouchZ * ModSettings.playerMovementSpeedMultiplier[playerNumber];
                        }
                    }
                    else
                    {
                        xMax = playerMotor.proneX * ModSettings.playerMovementSpeedMultiplier[playerNumber];
                        zMax = playerMotor.proneZ * ModSettings.playerMovementSpeedMultiplier[playerNumber];
                    }
                }
                else
                {
                    xMax = playerMotor.runX * ModSettings.playerMovementSpeedMultiplier[playerNumber];
                    zMax = playerMotor.runZ * ModSettings.playerMovementSpeedMultiplier[playerNumber];
                }
                playerMotor.xMovement = Mathf.Clamp(playerMotor.xMovement, -xMax, xMax);
                playerMotor.zMovement = Mathf.Clamp(playerMotor.zMovement, -zMax * playerMotor.backwardsPenaltyMultiplier, zMax);
            }

            private static void HookPlayerMotorHandleFallDamage(On.PlayerMotor.orig_HandleFallDamage orig, PlayerMotor playerMotor)
            {
                int crewPlayerIndex = 0;
                if (ModSettings.enableMultiplayer)
                {
                    crewPlayerIndex = MultiplayerMode.crewPlayers.IndexOf(playerMotor.player);
                    //Debug.Log("PlayerMotor.HandleFallDamage crewPlayerIndex is " + crewPlayerIndex);
                }
                if (!ModSettings.invincibilityMode[crewPlayerIndex])
                {
                    if (playerMotor.lastGroundPosition.y - ((MonoBehaviour)playerMotor).transform.position.y > 1f)
                    {
                        if (playerMotor.yMovement < playerMotor.deathVelocity)
                        {
                            playerMotor.deathFromHeight = true;
                        }
                        if (playerMotor.yMovement >= playerMotor.deathVelocity && playerMotor.yMovement < playerMotor.hurtVelocity)
                        {
                            playerMotor.hurtFromHeight = true;
                        }
                    }
                    if (playerMotor.deathFromHeight && playerMotor.yMovement > playerMotor.deathVelocity)
                    {
                        if (!ModSettings.enableMultiplayer || (ModSettings.enableMultiplayer && MultiplayerMode.AllOtherPlayersDown(playerMotor.player)))
                        {
                            if (!ModSettings.PlayerHasLivesLeft())
                            {
                                playerMotor.pHealth.InstantKill(PlayerHealth.DamageTypes.Fall);
                                DeathMenu.backgroundID = 1;
                                playerMotor.hurtFromHeight = false;
                            }
                            else
                            {
                                AudioSystem.PlaySound("Noises/Animations/Death Animations/Self/Fall");
                                ModSettings.TakeLife(playerMotor.player.GetComponentInChildren<PlayerHealth>());
                                playerMotor.deathFromHeight = false;
                                playerMotor.hurtFromHeight = false;
                                playerMotor.lastGroundPosition.y = ((MonoBehaviour)playerMotor).transform.position.y;
                                playerMotor.yMovement = 0f;
                            }
                        }
                        else
                        {
                            MultiplayerMode.DownPlayer(playerMotor.player);
                        }
                    }
                    else if (playerMotor.hurtFromHeight && playerMotor.yMovement >= playerMotor.hurtVelocity)
                    {
                        playerMotor.pHealth.DoDamage((float)Mathf.CeilToInt(playerMotor.yMovement * 10f), false, PlayerHealth.DamageTypes.Fall, false);
                        playerMotor.hurtFromHeight = false;
                    }
                }
            }

            private static void HookPlayerMotorPerformJump(On.PlayerMotor.orig_PerformJump orig, PlayerMotor playerMotor)
            {
                orig.Invoke(playerMotor);
                if (ModSettings.debugMode && ModSettings.noclip)
                {
                    playerMotor.timeSinceLastJump = 2f;
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @RadioChannel

            private static string HookRadioChannelGetTrack(On.RadioChannel.orig_GetTrack orig, RadioChannel radioChannel)
            {
                if (!ModSettings.loopingRadio)
                {
                    return orig.Invoke(radioChannel);
                }

                // Reduce the timer until the time passed is less than the length of the current track.
                float timeSinceLevelLoad = Time.timeSinceLevelLoad;
                for (int i = 0; timeSinceLevelLoad > 0f; i++)
                {
                    if (i == radioChannel.audioClips.Length)
                    {
                        i = 0;
                    }
                    if (radioChannel.audioClips[i].length > timeSinceLevelLoad)
                    {
                        return radioChannel.tracks[i];
                    }
                    timeSinceLevelLoad -= radioChannel.audioClips[i].length;
                }
                return radioChannel.tracks[0]; // Should never be called in the new system.
            }

            private static float HookRadioChannelGetTimeInTrack(On.RadioChannel.orig_GetTimeInTrack orig, RadioChannel radioChannel)
            {
                if (!ModSettings.loopingRadio)
                {
                    return orig.Invoke(radioChannel);
                }

                // Reduce the timer until the time passed is less than the length of the current track.
                float timeSinceLevelLoad = Time.timeSinceLevelLoad;
                for (int i = 0; timeSinceLevelLoad > 0f; i++)
                {
                    if (i == radioChannel.audioClips.Length)
                    {
                        i = 0;
                    }
                    if (radioChannel.audioClips[i].length > timeSinceLevelLoad)
                    {
                        return timeSinceLevelLoad;
                    }
                    timeSinceLevelLoad -= radioChannel.audioClips[i].length;
                }
                return 0f; // Should never be called in the new system.
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @RaftEscapeCheck

            private static void HookRaftEscapeCheckUpdate(On.RaftEscapeCheck.orig_Update orig, RaftEscapeCheck raftEscapeCheck)
            {
                // Not sure what caused this to be unassigned without any level generation settings being used.
                if (LevelGeneration.Instance.finishedGenerating)
                {
                    if (ModSettings.addAdditionalCrewDeckBuilding)
                    {
                        bool updateRaft = false;
                        if (raftEscapeCheck.mon != null && raftEscapeCheck.mon.IsMonsterActive)
                        {
                            Liferaft.LifeRaftState highestCompleted = Liferaft.LifeRaftState.OffEdge;
                            bool bothCompleted = true;
                            for (int i = 0; i < ModSettings.liferafts.Length; i++)
                            {
                                if (ModSettings.liferafts[i].StateofRaft > highestCompleted)
                                {
                                    highestCompleted = ModSettings.liferafts[i].StateofRaft;
                                }
                                ModSettings.liferafts[i].EscapeState = ModSettings.liferafts[i].StateofRaft;
                                if (ModSettings.liferafts[i].StateofRaft != ModSettings.liferafts[i].EscapeState)
                                {
                                    bothCompleted = false;
                                }
                            }
                            if (!bothCompleted)
                            {
                                if (highestCompleted == Liferaft.LifeRaftState.OffEdge)
                                {
                                    raftEscapeCheck.comDrag = EscapeChecker.Completeness.NotDone;
                                }
                                else if (highestCompleted == Liferaft.LifeRaftState.Torn)
                                {
                                    raftEscapeCheck.comDrag = EscapeChecker.Completeness.Complete;
                                }
                                else if (highestCompleted == Liferaft.LifeRaftState.Taped)
                                {
                                    raftEscapeCheck.comTape = EscapeChecker.Completeness.Complete;
                                }
                                else if (highestCompleted == Liferaft.LifeRaftState.Inflated)
                                {
                                    raftEscapeCheck.comPump = EscapeChecker.Completeness.Complete;
                                }
                                updateRaft = true;
                            }
                            for (int i = 0; i < ModSettings.cranes.Length; i++)
                            {
                                if (ModSettings.cranes[i] != null && !LevelGeneration.Instance.removeWalkways)
                                {
                                    if (ModSettings.cranes[i].ReplacedChain && !raftEscapeCheck.chainreplaced)
                                    {
                                        raftEscapeCheck.comSpool = EscapeChecker.Completeness.Complete;
                                        raftEscapeCheck.chainreplaced = true;
                                        updateRaft = true;
                                    }
                                    if (ModSettings.cranes[i].EscapeRaft.Attached && !raftEscapeCheck.hookattached && raftEscapeCheck.chainreplaced)
                                    {
                                        raftEscapeCheck.hookattached = true;
                                        raftEscapeCheck.comAttached = EscapeChecker.Completeness.Complete;
                                        updateRaft = true;
                                    }
                                }
                            }
                        }
                        else if (References.Monster != null)
                        {
                            raftEscapeCheck.mon = References.Monster.GetComponent<Monster>();
                        }
                        if (updateRaft)
                        {
                            raftEscapeCheck.UpdateRaft();
                        }
                    }
                    else
                    {
                        if (raftEscapeCheck != null)
                        {
                            if (raftEscapeCheck.crane != null)
                            {
                                if (raftEscapeCheck.crane.EscapeRaft == null)
                                {
                                    if (ModSettings.logDebugText)
                                    {
                                        Debug.Log("raftEscapeCheck.crane.EscapeRaft == null");
                                    }
                                    if (References.escapeLifeRaft != null)
                                    {
                                        raftEscapeCheck.crane.escapeLifeRaft = References.escapeLifeRaft;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                if (raftEscapeCheck.liferaft == null)
                                {
                                    if (ModSettings.logDebugText)
                                    {
                                        Debug.Log("raftEscapeCheck.liferaft == null");
                                    }
                                    if (References.lifeRaft != null)
                                    {
                                        raftEscapeCheck.liferaft = References.lifeRaft;
                                    }
                                    else
                                    {
                                        return;
                                    }
                                }
                                orig.Invoke(raftEscapeCheck);
                            }
                        }
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @RegionManager

            private static void HookRegionManagerInitialiseRegionData(On.RegionManager.orig_InitialiseRegionData orig, RegionManager regionManager)
            {
                Settings.shipCubesCount = new Vector3(76f, 10f, 16f) + ModSettings.increaseMapSizeVector;
                orig.Invoke(regionManager);
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Room

            private static void HookRoomOnPowerUp(On.Room.orig_OnPowerUp orig, Room room, GameObject _powerer = null)
            {
                orig.Invoke(room, _powerer);
                if (ModSettings.smokyShip && !ModSettings.alwaysSmoky && room.RoomType == RoomStructure.Corridor && room.PowerState == RoomPowerState.Powerable)
                {
                    foreach (ParticleSystem particleSystem in room.GetComponentsInChildren<ParticleSystem>())
                    {
                        if (particleSystem.name.Equals("SmokeEmitter"))
                        {
                            TimeScaleManager.Instance.StartCoroutine(ChangeFogAfterTime(particleSystem.emission, room));
                            break;
                        }
                    }
                }
            }

            private static void HookRoomOnPowerDown(On.Room.orig_OnPowerDown orig, Room room, GameObject _powerer = null)
            {
                orig.Invoke(room, _powerer);
                if (ModSettings.smokyShip && !ModSettings.alwaysSmoky && room.RoomType == RoomStructure.Corridor && room.PowerState == RoomPowerState.Powerable)
                {
                    foreach (ParticleSystem particleSystem in room.GetComponentsInChildren<ParticleSystem>())
                    {
                        if (particleSystem.name.Equals("SmokeEmitter"))
                        {
                            TimeScaleManager.Instance.StartCoroutine(ChangeFogAfterTime(particleSystem.emission, room));
                            break;
                        }
                    }
                }
            }

            private static IEnumerator ChangeFogAfterTime(ParticleSystem.EmissionModule emission, Room room)
            {
                float time = 0f;
                float timeToWait = UnityEngine.Random.Range(5f, 15f);
                while (time < timeToWait)
                {
                    time += Time.deltaTime;
                    yield return null;
                }

                emission.enabled = !FuseBoxManager.Instance.fuseboxes[room.PrimaryRegion].powered;
                yield break;
            }

            private static void HookRoomSpawnRoomModel(On.Room.orig_SpawnRoomModel orig, Room room, int _index, Room _room, Vector3 _sepObjNode = default(Vector3))
            {
                // # Use static alphabridge variable so loading twice error is avoided.
                if (room != null)
                {
                    if (room.name.Contains("Bridge_v1"))
                    {
                        Debug.Log("Room prefab is " + room.name);
                        if (room.ActiveModel != null)
                        {
                            Debug.Log("and activeModel is " + room.ActiveModel.name);
                        }
                        AssetBundle ab = null;
                        UnityEngine.Object[] ob = null;
                        string sparkyFilePathNew = Path.Combine(Directory.GetCurrentDirectory(), "alphabridge");
                        Debug.Log("File path used for Sparky Asset Bundle is: " + sparkyFilePathNew);
                        try
                        {
                            try
                            {
                                ab = AssetBundle.LoadFromFile(sparkyFilePathNew);
                            }
                            catch
                            {
                                Debug.Log("Error loading Asset Bundle from file");
                            }
                            try
                            {
                                if (ab != null)
                                {
                                    ob = ab.LoadAllAssets();
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

                        GameObject alphaBridge = Instantiate((GameObject)ob[0]);
                        MeshRenderer alphaBridgeMeshRenderer = alphaBridge.GetComponent<MeshRenderer>();
                        //alphaBridgeMeshRenderer.sharedMaterials = room.roomModels[_index].model.GetComponent<MeshRenderer>().sharedMaterials;

                        /*
                        foreach (Material material in alphaBridgeMeshRenderer.sharedMaterials)
                        {
                            Debug.Log(material.name);
                        }

                        Material[] allMaterials = FindObjectsOfType<Material>();
                        foreach (Material material in allMaterials)
                        {
                            Debug.Log(material);
                        }

                        foreach (Material material in room.roomModels[_index].model.GetComponentsInChildren<Material>())
                        {
                            Debug.Log(material);
                        }
                        */

                        // Changing material at runtime of a meshrenderer won't work - digvijay027 - https://answers.unity.com/questions/1826197/changing-material-at-runtime-of-a-meshrenderer-won.html - Accessed 28.01.2022
                        Material[] newSharedMaterials = new Material[7];

                        foreach (MeshRenderer meshRenderer in room.roomModels[_index].model.GetComponentsInChildren<MeshRenderer>())
                        {
                            foreach (Material material in meshRenderer.sharedMaterials)
                            {
                                if (material.name.Equals("Upd_Rm_Wood01"))
                                {
                                    Debug.Log(material.name);
                                    newSharedMaterials[0] = material;
                                    newSharedMaterials[5] = material;
                                }
                                else if (material.name.Equals("Upd_Room_Window"))
                                {
                                    Debug.Log(material.name);
                                    newSharedMaterials[4] = material;
                                }
                                else if (material.name.Equals("Upd_Bridge01"))
                                {
                                    Debug.Log(material.name);
                                    newSharedMaterials[3] = material;
                                }
                                else if (material.name.Equals("Misc_Wood02Bridge_Mat"))
                                {
                                    Debug.Log(material.name);
                                    newSharedMaterials[2] = material;
                                }
                                else if (material.name.Equals("Upd_Rm_Default02"))
                                {
                                    Debug.Log(material.name);
                                    newSharedMaterials[1] = material;
                                }
                                else if (material.name.Equals("Upd_Corridor01"))
                                {
                                    Debug.Log(material.name);
                                    newSharedMaterials[6] = material;
                                }
                            }
                        }

                        alphaBridgeMeshRenderer.sharedMaterials = newSharedMaterials;

                        //alphaBridgeMeshRenderer.sharedMaterials[0] = (Material)Resources.Load("Misc_Wood02_01_Dif", typeof(Material));
                        //alphaBridgeMeshRenderer.sharedMaterials[1] = (Material)Resources.Load("ProGen_Upd_Rm_Default06_Dif", typeof(Material));
                        //alphaBridgeMeshRenderer.sharedMaterials[2] = (Material)Resources.Load("ProGen_Upd_Rm_Wood_02_Dif", typeof(Material));
                        //alphaBridgeMeshRenderer.sharedMaterials[3] = (Material)Resources.Load("ProGen_Upd_Bridge_Default01_Dif", typeof(Material));
                        //alphaBridgeMeshRenderer.sharedMaterials[4] = (Material)Resources.Load("ProGen_Upd_Rm_Window_01_Dif", typeof(Material));
                        //alphaBridgeMeshRenderer.sharedMaterials[5] = (Material)Resources.Load("Misc_Wood02_01_Dif", typeof(Material));
                        //alphaBridgeMeshRenderer.sharedMaterials[6] = (Material)Resources.Load("ProGen_Upd_Cor_04Dif", typeof(Material));

                        foreach (Material material in alphaBridgeMeshRenderer.sharedMaterials)
                        {
                            Debug.Log(material.name);
                        }

                        /*
                        wood
                        room
                        rwood
                        bridge
                        glass
                        wood2
                        corridor
                        */

                        room.roomModels[_index].model = alphaBridge;
                    }
                }
                orig.Invoke(room, _index, _room, _sepObjNode);
                if (room.name.Contains("Bridge_v1"))
                {
                    if (room.ActiveModel != null)
                    {
                        Debug.Log("New activeModel is " + room.ActiveModel.name);
                    }
                    room.ActiveModel.transform.rotation = Quaternion.Euler(room.ActiveModel.transform.rotation.x, room.ActiveModel.transform.rotation.y - 90f, room.ActiveModel.transform.rotation.z);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SecurityCamera

            private static void HookSecurityCamera(On.SecurityCamera.orig_Start orig, SecurityCamera securityCamera)
            {
                securityCamera.rigidBody = ((MonoBehaviour)securityCamera).GetComponent<Rigidbody>();
                securityCamera.amberState = ((MonoBehaviour)securityCamera).GetComponent<AmberState>();
                securityCamera.audiosources.maxDistance = 10f;
                securityCamera.neededString = " ";
                securityCamera.currentString = securityCamera.neededString;
                SecurityCamera.cameraList.Add(securityCamera);
                if (SecurityCamera.inventory == null)
                {
                    SecurityCamera.inventory = UnityEngine.Object.FindObjectOfType(typeof(Inventory)) as Inventory;
                }
                securityCamera.simpleTilt = ((MonoBehaviour)securityCamera).gameObject.GetComponentInChildren<SimpleTilt>();
                if (SecurityCamera.alarmManager == null)
                {
                    SecurityCamera.alarmManager = UnityEngine.Object.FindObjectOfType(typeof(AlarmManager)) as AlarmManager;
                }
                securityCamera.camRoom = ((MonoBehaviour)securityCamera).GetComponentInParent<Room>();
                securityCamera.ledRend = securityCamera.ledLight.GetComponent<Renderer>();
                if (ModSettings.noCameras)
                {
                    securityCamera.StopByDuctTape();
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Smashable

            private static void HookSmashableOnCollisionEnter(On.Smashable.orig_OnCollisionEnter orig, Smashable smashable, Collision _collision)
            {
                if ((ModSettings.betterSmashables && _collision.relativeVelocity.magnitude < 6f) || ModSettings.unsmashables)
                {
                    return;
                }
                orig.Invoke(smashable, _collision);
                if (smashable.smashed)
                {
                    if (ModSettings.addSmokeGrenade && smashable.name.Equals("Smoke Grenade"))
                    {
                        TimeScaleManager.Instance.StartCoroutine(TriggerSmokeGrenadeOnSmashable(smashable));
                    }
                    else if (ModSettings.addMolotov && smashable.name.Equals("Molotov"))
                    {
                        TimeScaleManager.Instance.StartCoroutine(TriggerMolotovOnSmashable(smashable));
                    }
                }
            }

            // Awake is called before Start. Both are called only once.
            private static void HookSmashableStart(On.Smashable.orig_Start orig, Smashable smashable)
            {
                orig.Invoke(smashable);
                MeshRenderer meshRenderer = smashable.GetComponentInChildren<MeshRenderer>();
                Material material = meshRenderer.material;
                if (material.name.Contains("Bottle"))
                {
                    float randomValue = UnityEngine.Random.value;
                    if (ModSettings.addSmokeGrenade && randomValue <= 0.33f)
                    {
                        if (smokeGrenadePrefab == null)
                        {
                            CreateSmokeGrenadePrefab();
                        }
                        smashable.name = "Smoke Grenade";
                        InventoryItem inventoryItem = smashable.GetComponentInParent<InventoryItem>();
                        inventoryItem.itemName = "Smoke Grenade";
                        // Updating the material colour only changes the colour when not glowing, i.e. no glowstick / flashlight nearby, not looking at item directly or when holding in hand.
                        // Updating the Inventory Item's glow shader does not work because colour values are hardcoded into the glow code.

                        if (smokeGrenadeModelPrefab == null)
                        {
                            try
                            {
                                UnityEngine.Object[] smokeGrenadeUnpacked = Utilities.LoadAssetBundle("smokegrenade");
                                if (smokeGrenadeUnpacked.Length > 0 && smokeGrenadeUnpacked[0] != null && smokeGrenadeUnpacked[0].GetType() == typeof(GameObject))
                                {
                                    smokeGrenadeModelPrefab = (GameObject)smokeGrenadeUnpacked[0];
                                }
                            }
                            catch
                            {
                                Debug.Log("Failed to load Smoke Grenade model");
                            }
                        }
                        if (smokeGrenadeModelPrefab != null)
                        {
                            GameObject smokeGrenadeModelGO = Instantiate(smokeGrenadeModelPrefab);
                            smokeGrenadeModelGO.SetActive(true);
                            smokeGrenadeModelGO.transform.SetParent(smashable.normalGO.transform, false);
                            MeshRenderer molotovMeshRenderer = smokeGrenadeModelGO.GetComponentInChildren<MeshRenderer>();
                            inventoryItem.Render.Remove(meshRenderer);
                            inventoryItem.glowShader.render.Remove(meshRenderer);
                            meshRenderer.transform.localScale = Vector3.zero;

                            ItemShadow itemShadow = inventoryItem.GetComponentInChildren<ItemShadow>();
                            inventoryItem.render = new List<MeshRenderer>();
                            inventoryItem.glowShader.render = new List<MeshRenderer>();
                            inventoryItem.GetMeshRender();
                            inventoryItem.glowShader.Start();
                            itemShadow.render = inventoryItem.GetComponentsInChildren<MeshRenderer>().ToList<MeshRenderer>();//((MonoBehaviour)itemShadow).GetComponentsInChildren<MeshRenderer>().ToList<MeshRenderer>(); // Does this do anything?

                        }
                    }
                    else if (ModSettings.addMolotov && randomValue >= 0.67f)
                    {
                        if (molotovPrefab == null)
                        {
                            CreateMolotovPrefab();
                        }
                        smashable.name = "Molotov";
                        InventoryItem inventoryItem = smashable.GetComponentInParent<InventoryItem>();
                        inventoryItem.itemName = "Molotov";

                        if (molotovModelPrefab == null)
                        {
                            try
                            {
                                UnityEngine.Object[] molotovUnpacked = Utilities.LoadAssetBundle("molotov");
                                if (molotovUnpacked.Length > 0 && molotovUnpacked[0] != null && molotovUnpacked[0].GetType() == typeof(GameObject))
                                {
                                    molotovModelPrefab = (GameObject)molotovUnpacked[0];
                                }
                            }
                            catch
                            {
                                Debug.Log("Failed to load Molotov model");
                            }
                        }
                        if (molotovModelPrefab != null)
                        {
                            GameObject molotovModelGO = Instantiate(molotovModelPrefab);
                            molotovModelGO.SetActive(true);
                            molotovModelGO.transform.SetParent(smashable.normalGO.transform, false);
                            MeshRenderer molotovMeshRenderer = molotovModelGO.GetComponentInChildren<MeshRenderer>();
                            inventoryItem.Render.Remove(meshRenderer);
                            inventoryItem.glowShader.render.Remove(meshRenderer);
                            meshRenderer.transform.localScale = Vector3.zero;

                            ItemShadow itemShadow = inventoryItem.GetComponentInChildren<ItemShadow>();
                            inventoryItem.render = new List<MeshRenderer>();
                            inventoryItem.glowShader.render = new List<MeshRenderer>();
                            inventoryItem.GetMeshRender();
                            inventoryItem.glowShader.Start();
                            itemShadow.render = inventoryItem.GetComponentsInChildren<MeshRenderer>().ToList<MeshRenderer>();//((MonoBehaviour)itemShadow).GetComponentsInChildren<MeshRenderer>().ToList<MeshRenderer>(); // Does this do anything?
                        }
                    }
                }
            }

            private static GameObject smokeGrenadeModelPrefab;
            private static GameObject molotovModelPrefab;

            private static GameObject smokeGrenadePrefab;
            private static float smokeGrenadeDuration = 26f;
            private static float smokeGrenadeParticleStartLifeTime = 8f;
            private static void CreateSmokeGrenadePrefab()
            {
                FireExtinguisher fireExtinguisher = FindObjectOfType<FireExtinguisher>();
                if (fireExtinguisher != null)
                {
                    GameObject smokeGrenade = new GameObject();
                    smokeGrenade.SetActive(false);
                    GameObject particlesParent = Instantiate(fireExtinguisher.particlesParent);
                    particlesParent.transform.SetParent(smokeGrenade.transform, false);
                    GameObject colliderGO = Instantiate(fireExtinguisher.particleCollider.gameObject);
                    colliderGO.transform.SetParent(smokeGrenade.transform, false);
                    colliderGO.SetActive(true);

                    ParticleSystem particleSystem = particlesParent.GetComponentInChildren<ParticleSystem>();
                    particlesParent.transform.name = "FireParticleSystemTransform";

                    float duration = smokeGrenadeDuration;
                    float startLifetime = smokeGrenadeParticleStartLifeTime;
                    ParticleSystem.EmissionModule psem = particleSystem.emission;
                    psem.enabled = true;
                    psem.rateOverTime = new ParticleSystem.MinMaxCurve(4f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe((startLifetime / 8f) / duration - 0.01f, 1f), new Keyframe((startLifetime / 8f) / duration, 0.25f), new Keyframe((duration - startLifetime) / duration - 0.01f, 0.25f), new Keyframe((duration - startLifetime) / duration, 0f), new Keyframe(1f, 0f)));

                    ParticleSystem.ShapeModule shapeModule = particleSystem.shape;
                    shapeModule.shapeType = ParticleSystemShapeType.Hemisphere;

                    ParticleSystem.MainModule main = particleSystem.main;
                    main.duration = duration;
                    main.startLifetime = startLifetime;
                    main.startSpeed = 0.25f;
                    main.startSize = 100f;
                    main.simulationSpace = ParticleSystemSimulationSpace.World;
                    main.maxParticles = 1000;

                    BoxCollider boxCollider = smokeGrenade.AddComponent<BoxCollider>();
                    boxCollider.transform.position += Vector3.up;
                    boxCollider.size = new Vector3(5f, 3f, 5f);
                    boxCollider.isTrigger = true;
                    smokeGrenade.layer = 12; // VisionOnly

                    AudioSource audioSource = smokeGrenade.AddComponent<AudioSource>();
                    fireExtinguisher.audSource.CopyTo(audioSource);
                    fireExtinguisher.audSource.CopyEffectsTo(audioSource);
                    // audioSource.spatialBlend = 1f; // This is done in the copy operation, but it is important to remember when creation any new AudioSource. Without this the sound is 2D.

                    smokeGrenadePrefab = smokeGrenade;
                }
            }

            /*
            Layer 0: Default
            Layer 1: TransparentFX
            Layer 2: Ignore Raycast
            Layer 3: 
            Layer 4: Water
            Layer 5: UI
            Layer 6: 
            Layer 7: 
            Layer 8: NavVision
            Layer 9: Player
            Layer 10: HUDLayer
            Layer 11: LightVolume
            Layer 12: VisionOnly
            Layer 13: CullLayer
            Layer 14: IgnorePhysics
            Layer 15: AttachPhysics
            Layer 16: NavOnly
            Layer 17: NoPhysicsNoRender
            Layer 18: Particles
            Layer 19: DefaultNavVision
            Layer 20: PlayerTriggers
            Layer 21: 
            Layer 22: PlayerClip
            Layer 23: Interactable
            Layer 24: MonsterVolumeLightBox
            Layer 25: GlowLightCheck
            Layer 26: 
            Layer 27: 
            Layer 28: 
            Layer 29: 
            Layer 30: 
            Layer 31: 
            */

            private static GameObject molotovPrefab;
            private static float molotovDuration = 26f;
            private static float molotovParticleStartLifeTime = 3f;
            private static void CreateMolotovPrefab()
            {
                FuelParticles fuelParticles = FindObjectOfType<FuelParticles>();
                if (fuelParticles != null)
                {
                    // Clone a FuelParticle. (Is this cloning only the FuelDecal or the FuelParticle too and whatever is in its parent? I think it may be cloning the parent, which might make the transform updates easier.) 
                    GameObject molotov = Instantiate(fuelParticles.fuelDecal);
                    molotov.SetActive(false);
                    FuelDecal fuelDecal = molotov.GetComponent<FuelDecal>();

                    // Possibly also prevents error from instantiation due to DecalManager. Decal can't be seen at molotov anyway any probably wouldn't fit the style.
                    fuelDecal.fuelDecalMaterial.enabled = false;

                    // Set the diameter.
                    float fireShroudDiameter = 6f;

                    // Make the fire follow the molotov.
                    //fuelDecalGO.transform.SetParent(molotov.transform, true);
                    //fuelDecal.transform.SetParent(molotov.transform, false);
                    fuelDecal.flammable.transform.SetParent(fuelDecal.transform, false);
                    fuelDecal.flammable.fire.transform.SetParent(fuelDecal.flammable.transform, false);

                    // Change the duration of the fire's burning.
                    float duration = molotovDuration;
                    float startLifetime = molotovParticleStartLifeTime;
                    float fireIntensityBuffer = 6f;
                    fuelDecal.flammable.fireFuel = duration - fireIntensityBuffer;
                    fuelDecal.flammable.lowFuel = duration - fireIntensityBuffer;
                    fuelDecal.maxFuel = duration - fireIntensityBuffer;

                    // Change the size of the PlayerDamage BoxCollider and rotate it to face correctly.
                    BoxCollider boxCollider = fuelDecal.flammable.fire.GetComponentInChildren<BoxCollider>();
                    boxCollider.transform.Rotate(90f, 0f, 0f);
                    boxCollider.size = new Vector3(fireShroudDiameter, 0.25f, fireShroudDiameter);

                    // Change the properties of the fire ParticleSystem.
                    ParticleSystem particleSystem = fuelDecal.flammable.fire.gameObject.GetComponent<ParticleSystem>();

                    ParticleSystem.MainModule main = particleSystem.main;
                    main.duration = duration;
                    main.startSize = 2.5f;
                    main.startLifetime = startLifetime;
                    main.maxParticles = 1000;

                    ParticleSystem.ShapeModule shape = particleSystem.shape;
                    shape.shapeType = ParticleSystemShapeType.Circle;
                    shape.radius = fireShroudDiameter / 2f;

                    ParticleSystem.EmissionModule emission = particleSystem.emission;
                    emission.rateOverTime = new ParticleSystem.MinMaxCurve(50f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe((duration - startLifetime) / duration - 0.01f, 1f), new Keyframe((duration - startLifetime) / duration, 0f), new Keyframe(1f, 0f)));

                    // Ignite the fire.
                    fuelDecal.flammable.StartFire();

                    AudioSource audioSource = molotov.AddComponent<AudioSource>();
                    fuelDecal.flammable.fireSource.CopyTo(audioSource);
                    fuelDecal.flammable.fireSource.CopyEffectsTo(audioSource);

                    molotovPrefab = molotov;
                }
            }

            public static IEnumerator TriggerSmokeGrenadeOnSmashable(Smashable smashable)
            {
                if (smokeGrenadePrefab == null)
                {
                    CreateSmokeGrenadePrefab();
                }
                GameObject smokeGrenade = Instantiate(smokeGrenadePrefab, smashable.debris[0].transform.position, smashable.debris[0].transform.rotation);
                smokeGrenade.SetActive(true);
                AudioSource audioSource = smokeGrenade.GetComponent<AudioSource>();
                AudioSystem.PlaySound("Noises/Actions/Extinguisher/ACT_ExtinguishLoop_00", audioSource);
                audioSource.pitch *= 0.85f;
                Destroy(smokeGrenade, smokeGrenadeDuration);
                float timePassed = 0f;
                while (smashable != null && smashable.debris[0] != null)
                {
                    timePassed += Time.deltaTime;
                    smokeGrenade.transform.position = smashable.debris[0].transform.position;
                    yield return null;
                }
                yield return new WaitForSeconds(smokeGrenadeDuration - smokeGrenadeParticleStartLifeTime - timePassed);
                audioSource.Stop();
                if (ModSettings.enableMultiplayer && !MultiplayerMode.useLegacyAudio)
                {
                    VirtualAudioSource virtualAudioSource = audioSource.gameObject.GetComponent<VirtualAudioSource>();
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
                yield break;
            }

            public static IEnumerator TriggerMolotovOnSmashable(Smashable smashable)
            {
                if (molotovPrefab == null)
                {
                    CreateMolotovPrefab();
                }
                GameObject molotov = Instantiate(molotovPrefab, smashable.debris[0].transform.position, smashable.debris[0].transform.rotation);
                molotov.SetActive(true);
                AudioSource audioSource = molotov.GetComponent<AudioSource>();
                AudioSystem.PlaySound("Noises/Actions/Fire", audioSource);
                molotov.GetComponent<VolumeController>().StartVolume *= 5f;
                Destroy(molotov, molotovDuration);
                float timePassed = 0f;
                ParticleSystem particleSystem = molotov.GetComponent<FuelDecal>().flammable.fire.gameObject.GetComponent<ParticleSystem>();
                while (smashable != null && smashable.debris[0] != null)
                {
                    timePassed += Time.deltaTime;
                    molotov.transform.position = smashable.debris[0].transform.position;
                    particleSystem.transform.position = molotov.transform.position;
                    yield return null;
                }
                yield return new WaitForSeconds(molotovDuration - molotovParticleStartLifeTime - timePassed);
                audioSource.Stop();
                if (ModSettings.enableMultiplayer && !MultiplayerMode.useLegacyAudio)
                {
                    VirtualAudioSource virtualAudioSource = audioSource.gameObject.GetComponent<VirtualAudioSource>();
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
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SpawnDoorsLG

            private static void HookSpawnDoorsLGGenerateDoors(On.SpawnDoorsLG.orig_GenerateDoors orig, List<DoorData> _connectionsList, List<GameObject> _doorsList, GameObject _doorParent, List<GameObject> _usedDoors)
            {
                for (int i = 0; i < _connectionsList.Count; i++)
                {
                    if (_connectionsList[i].spawnDoor)
                    {
                        if (!ModSettings.useCustomDoors)
                        {
                            if (_connectionsList[i].connection == ConnectionType.Outside)
                            {
                                _usedDoors.Add(UnityEngine.Object.Instantiate<GameObject>(_doorsList[1], new Vector3((float)_connectionsList[i].x, (float)_connectionsList[i].floor + 0.35f, (float)_connectionsList[i].z), Quaternion.identity));
                            }
                            else if (_connectionsList[i].connection == ConnectionType.Powered)
                            {
                                _usedDoors.Add(UnityEngine.Object.Instantiate<GameObject>(_doorsList[3], new Vector3((float)_connectionsList[i].x, (float)_connectionsList[i].floor + 0.35f, (float)_connectionsList[i].z), Quaternion.identity));
                            }
                            else if (_connectionsList[i].connection == ConnectionType.SubRoom)
                            {
                                _usedDoors.Add(UnityEngine.Object.Instantiate<GameObject>(_doorsList[2], new Vector3((float)_connectionsList[i].x, (float)_connectionsList[i].floor + 0.35f, (float)_connectionsList[i].z), Quaternion.identity));
                            }
                            else if (_connectionsList[i].connection == ConnectionType.Barricade)
                            {
                                _usedDoors.Add(UnityEngine.Object.Instantiate<GameObject>(_doorsList[5], new Vector3((float)_connectionsList[i].x, (float)_connectionsList[i].floor + 0.35f, (float)_connectionsList[i].z), Quaternion.identity));
                            }
                            else if (_connectionsList[i].joiningRoom.PrimaryRegion == PrimaryRegionType.Engine)
                            {
                                _usedDoors.Add(UnityEngine.Object.Instantiate<GameObject>(_doorsList[6], new Vector3((float)_connectionsList[i].x, (float)_connectionsList[i].floor + 0.35f, (float)_connectionsList[i].z), Quaternion.identity));
                            }
                            else if (LevelGeneration.Instance.nodeData[(int)_connectionsList[i].regionNode.x][(int)_connectionsList[i].regionNode.y][(int)_connectionsList[i].regionNode.z].primaryRegion != PrimaryRegionType.LowerDeck && LevelGeneration.Instance.nodeData[(int)_connectionsList[i].regionNode.x][(int)_connectionsList[i].regionNode.y][(int)_connectionsList[i].regionNode.z].primaryRegion != PrimaryRegionType.CargoHold)
                            {
                                _usedDoors.Add(UnityEngine.Object.Instantiate<GameObject>(_doorsList[0], new Vector3((float)_connectionsList[i].x, (float)_connectionsList[i].floor + 0.35f, (float)_connectionsList[i].z), Quaternion.identity));
                            }
                            else
                            {
                                _usedDoors.Add(UnityEngine.Object.Instantiate<GameObject>(_doorsList[4], new Vector3((float)_connectionsList[i].x, (float)_connectionsList[i].floor + 0.35f, (float)_connectionsList[i].z), Quaternion.identity));
                            }
                        }
                        else
                        {
                            if (!ModSettings.lightlyLockedDoors)
                            {
                                _usedDoors.Add(UnityEngine.Object.Instantiate<GameObject>(_doorsList[ModSettings.customDoorTypeNumber], new Vector3((float)_connectionsList[i].x, (float)_connectionsList[i].floor + 0.35f, (float)_connectionsList[i].z), Quaternion.identity));
                            }
                            else
                            {
                                if (_connectionsList[i].joiningRoom.primaryRegion == PrimaryRegionType.UpperDeck || _connectionsList[i].joiningRoom.primaryRegion == PrimaryRegionType.CrewDeck)
                                {
                                    _usedDoors.Add(UnityEngine.Object.Instantiate<GameObject>(_doorsList[0], new Vector3((float)_connectionsList[i].x, (float)_connectionsList[i].floor + 0.35f, (float)_connectionsList[i].z), Quaternion.identity));
                                }
                                else
                                {
                                    _usedDoors.Add(UnityEngine.Object.Instantiate<GameObject>(_doorsList[6], new Vector3((float)_connectionsList[i].x, (float)_connectionsList[i].floor + 0.35f, (float)_connectionsList[i].z), Quaternion.identity));
                                }
                            }
                        }
                        GameObject gameObject = _usedDoors[_usedDoors.Count - 1];
                        Door door = gameObject.GetComponentInChildren<Door>();
                        _connectionsList[i].joiningRoom.roomDoors.Add(door);

                        if (ModSettings.useCustomDoors && ModSettings.lightlyLockedDoors)
                        {
                            door.GetComponentInChildren<DoorBolt>().ToggleLock();
                        }

                        gameObject.transform.parent = _doorParent.transform;
                        gameObject.name = string.Concat(new object[]
                        {
                    gameObject.name.Substring(0, gameObject.name.Length - 7),
                    " (",
                    i,
                    ")"
                        });
                        gameObject.GetComponentInChildren<Door>().JoiningRoom = _connectionsList[i].joiningRoom;
                        gameObject.GetComponentInChildren<Door>().CheckIfBarricade();
                        gameObject.GetComponentInChildren<Door>().AttachedRoom = _connectionsList[i].joiningRoom;
                        int num = _connectionsList[i].joiningRoom.rotationQuadrant + _connectionsList[i].rotationQuadrant;
                        if (num >= 4)
                        {
                            num -= 4;
                        }
                        gameObject.GetComponentInChildren<Door>().rotationQuadrant = num;
                        float y = (float)_connectionsList[i].joiningRoom.rotationQuadrant * 90f + (float)_connectionsList[i].rotationQuadrant * 90f;
                        gameObject.transform.rotation = Quaternion.Euler(new Vector3(0f, y, 0f));
                    }
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SpawnFuseBoxNearby

            private static void HookSpawnFuseBoxNearbyOnGenerationComplete(On.SpawnFuseBoxNearby.orig_OnGenerationComplete orig, SpawnFuseBoxNearby spawnFuseBoxNearby)
            {
                if (spawnFuseBoxNearby.newFuseBox != null)
                {
                    orig.Invoke(spawnFuseBoxNearby);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SpawnStairsLG

            public static List<int> customBottomStairLevelList;

            private static void HookSpawnStairsLGSpawnStairStack(On.SpawnStairsLG.orig_SpawnStairStack orig, List<Room> _rooms, List<Room> _stairs, GameObject _roomsParent)
            {
                LoadingBackground loadingBackground = FindObjectOfType<LoadingBackground>();
                string loadingProgressText = "Spawning Stairs";
                SetLoadingText(loadingBackground, loadingProgressText);
                try
                {
                    List<Room> bottomStairs = new List<Room>();
                    List<Room> middleStairs = new List<Room>();
                    List<Room> topStairs = new List<Room>();
                    List<Room> noExitMiddleStairs = new List<Room>();
                    List<int> integerList = new List<int>();
                    int stairsPerLevel = Settings.StairsPerLevel;
                    int[] integerArray = new int[Settings.StairsPerLevel];
                    for (int i = 0; i < stairsPerLevel; i++)
                    {
                        integerArray[i] = 0;
                    }
                    bool finishedSpawningStairs = false;
                    for (int j = 0; j < _stairs.Count; j++)
                    {
                        if (_stairs[j].StairsType == StairStructure.Bottom)
                        {
                            bottomStairs.Add(_stairs[j]);
                        }
                        if (_stairs[j].StairsType == StairStructure.Middle)
                        {
                            middleStairs.Add(_stairs[j]);
                        }
                        if (_stairs[j].StairsType == StairStructure.Top)
                        {
                            topStairs.Add(_stairs[j]);
                        }
                        if (_stairs[j].StairsType == StairStructure.NoExitMiddle)
                        {
                            noExitMiddleStairs.Add(_stairs[j]);
                        }
                    }
                    bottomStairs = (from x in bottomStairs
                                    orderby x.name
                                    select x).ToList<Room>();
                    middleStairs = (from y in middleStairs
                                    orderby y.name
                                    select y).ToList<Room>();
                    topStairs = (from z in topStairs
                                 orderby z.name
                                 select z).ToList<Room>();
                    List<Room> processedBottomStairs = new List<Room>();
                    List<Room> processedMiddleStairs = new List<Room>();
                    List<Room> processedTopStairs = new List<Room>();
                    int stairRegionID = RegionManager.Instance.StringToRegionID("Stairs");
                    try
                    {
                        while (!finishedSpawningStairs)
                        {
                            for (int k = 0; k < _rooms.Count; k++)
                            {
                                PositionRoomLG.UnoccupyNodes(_rooms[_rooms.Count - 1]);
                                LevelGeneration.Instance.SendMessage("DeleteRoom", _rooms[_rooms.Count - 1]);
                            }
                            try
                            {
                                for (int bottomStairIndex = 0; bottomStairIndex < stairsPerLevel; bottomStairIndex++)
                                {
                                    processedBottomStairs = new List<Room>();
                                    for (int m = 0; m < bottomStairs.Count; m++)
                                    {
                                        processedBottomStairs.Add(bottomStairs[m]);
                                    }
                                    bool finishedProcessingBottomStairs = false;
                                    while (!finishedProcessingBottomStairs)
                                    {
                                        int index = UnityEngine.Random.Range(0, processedBottomStairs.Count);
                                        _rooms.Add(UnityEngine.Object.Instantiate<Room>(processedBottomStairs[index]));
                                        Room randomBottomStair = _rooms[_rooms.Count - 1];
                                        randomBottomStair.name = string.Concat(new object[]
                                        {
                        randomBottomStair.name.Substring(0, randomBottomStair.name.Length - 7),
                        " (",
                        bottomStairIndex,
                        ")"
                                        });
                                        randomBottomStair.transform.position = Vector3.zero;
                                        randomBottomStair.transform.parent = _roomsParent.transform;
                                        int nodeYLevel;
                                        if (bottomStairIndex < 2)
                                        {
                                            nodeYLevel = 1; // Sub side bottom stair.
                                        }
                                        else
                                        {
                                            if (bottomStairIndex < 4)
                                            {
                                                nodeYLevel = 3; // Engine side bottom stair.
                                            }
                                            else
                                            {
                                                nodeYLevel = customBottomStairLevelList[bottomStairIndex - 4];
                                            }
                                        }
                                        randomBottomStair.RegionNode = new Vector3(0f, (float)nodeYLevel, 0f);
                                        randomBottomStair.RandomRegion();
                                        PositionRoomLG.PositionRoom(randomBottomStair, nodeYLevel, -1, -1);
                                        if (randomBottomStair != null)
                                        {
                                            for (int n = 0; n < randomBottomStair.roomDoorData.Count; n++)
                                            {
                                                RoomAppendageData.ConvertAppendageToRegionSpace(randomBottomStair, randomBottomStair.roomDoorData[n]);
                                            }
                                            Room.HandleAppendageData(randomBottomStair);
                                            finishedProcessingBottomStairs = true;
                                            integerList.Add(0);
                                            break;
                                        }
                                        else
                                        {
                                            processedBottomStairs.RemoveAt(index);
                                        }
                                        if (processedBottomStairs.Count == 0)
                                        {
                                            finishedProcessingBottomStairs = true;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                Debug.Log("Error while spawning bottom stairs.");
                            }
                            if (_rooms.Count == stairsPerLevel /* 4 */)
                            {
                                finishedSpawningStairs = true;
                            }
                            try
                            {
                                for (int bottomStairIndex = 0; bottomStairIndex < stairsPerLevel; bottomStairIndex++)
                                {
                                    int middleStairLevel = (int)_rooms[bottomStairIndex].RegionNode.y + 1;
                                    //Debug.Log("Using middle stair level " + middleStairLevel + " for bottom stair at " + _rooms[bottomStairIndex].RegionNode);
                                    integerArray[bottomStairIndex] = middleStairLevel; // Extra line that should help stairs that don't have any middle.
                                    while (RegionManager.Instance.CheckNodeForRegion(new Vector3(_rooms[bottomStairIndex].RegionNode.x, (float)(middleStairLevel + 1), _rooms[bottomStairIndex].RegionNode.z), stairRegionID, -1)) // While the region node above the last created stairs is still a stairs region... Ensure that custom stairs are not underneath normal stairs, or else the spawning will break here.
                                    {
                                        //Debug.Log("Processing middle stair as " + new Vector3(_rooms[bottomStairIndex].RegionNode.x, (float)(middleStairLevel + 1), _rooms[bottomStairIndex].RegionNode.z) + " is a stair node");
                                        processedMiddleStairs = new List<Room>();
                                        for (int num4 = 0; num4 < middleStairs.Count; num4++)
                                        {
                                            processedMiddleStairs.Add(middleStairs[num4]);
                                        }
                                        bool finishedProcessingMiddleStairs = false;
                                        while (!finishedProcessingMiddleStairs)
                                        {
                                            integerArray[bottomStairIndex] = middleStairLevel + 1;
                                            int index = UnityEngine.Random.Range(0, processedMiddleStairs.Count);
                                            _rooms.Add(UnityEngine.Object.Instantiate<Room>(processedMiddleStairs[index]));
                                            Room randomMiddleStair = _rooms[_rooms.Count - 1];
                                            randomMiddleStair.name = string.Concat(new object[]
                                            {
                            randomMiddleStair.name.Substring(0, randomMiddleStair.name.Length - 7),
                            " (",
                            middleStairLevel * (bottomStairIndex + 1),
                            ")"
                                            });
                                            randomMiddleStair.transform.position = Vector3.zero;
                                            randomMiddleStair.transform.parent = _roomsParent.transform;
                                            randomMiddleStair.RegionNode = new Vector3(_rooms[bottomStairIndex].RegionNode.x, (float)middleStairLevel, _rooms[bottomStairIndex].RegionNode.z);
                                            randomMiddleStair.RandomRegion();
                                            PositionRoomLG.PositionRoom(randomMiddleStair, middleStairLevel, (int)_rooms[bottomStairIndex].RegionNode.x, (int)_rooms[bottomStairIndex].RegionNode.z);
                                            if (randomMiddleStair != null)
                                            {
                                                for (int num5 = 0; num5 < randomMiddleStair.roomDoorData.Count; num5++)
                                                {
                                                    RoomAppendageData.ConvertAppendageToRegionSpace(randomMiddleStair, randomMiddleStair.roomDoorData[num5]);
                                                }
                                                Room.HandleAppendageData(randomMiddleStair);
                                                finishedProcessingMiddleStairs = true;
                                                bool flag4 = false;
                                                for (int num6 = 0; num6 < middleStairs.Count; num6++)
                                                {
                                                    if (middleStairs[num6] == processedMiddleStairs[index])
                                                    {
                                                        flag4 = true;
                                                        break;
                                                    }
                                                }
                                                if (flag4)
                                                {
                                                    middleStairLevel++;
                                                    //Debug.Log("Updating middle stair level to " + middleStairLevel);
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                processedMiddleStairs.RemoveAt(index);
                                            }
                                            if (processedMiddleStairs.Count == 0)
                                            {
                                                integerArray[bottomStairIndex] = middleStairLevel + 1;
                                                index = UnityEngine.Random.Range(0, noExitMiddleStairs.Count);
                                                _rooms.Add(UnityEngine.Object.Instantiate<Room>(noExitMiddleStairs[index]));
                                                randomMiddleStair = _rooms[_rooms.Count - 1];
                                                randomMiddleStair.name = string.Concat(new object[]
                                                {
                                randomMiddleStair.name.Substring(0, randomMiddleStair.name.Length - 7),
                                " (",
                                middleStairLevel * (bottomStairIndex + 1),
                                ")"
                                                });
                                                randomMiddleStair.transform.position = Vector3.zero;
                                                randomMiddleStair.transform.parent = _roomsParent.transform;
                                                randomMiddleStair.RegionNode = new Vector3(_rooms[bottomStairIndex].RegionNode.x, (float)middleStairLevel, _rooms[bottomStairIndex].RegionNode.z);
                                                randomMiddleStair.RandomRegion();
                                                PositionRoomLG.PositionRoom(randomMiddleStair, middleStairLevel, (int)_rooms[bottomStairIndex].RegionNode.x, (int)_rooms[bottomStairIndex].RegionNode.z);
                                                if (randomMiddleStair != null)
                                                {
                                                    Room.HandleAppendageData(randomMiddleStair);
                                                    middleStairLevel++;
                                                    break;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                Debug.Log("Error while spawning middle stairs.");
                            }
                            try
                            {
                                for (int bottomStairIndex = 0; bottomStairIndex < stairsPerLevel; bottomStairIndex++)
                                {
                                    processedTopStairs = new List<Room>();
                                    for (int num8 = 0; num8 < topStairs.Count; num8++)
                                    {
                                        processedTopStairs.Add(topStairs[num8]);
                                    }
                                    bool finishedProcessingTopStairs = false;
                                    while (!finishedProcessingTopStairs)
                                    {
                                        int index = UnityEngine.Random.Range(0, processedTopStairs.Count);
                                        _rooms.Add(UnityEngine.Object.Instantiate<Room>(processedTopStairs[index]));
                                        Room randomTopStair = _rooms[_rooms.Count - 1];
                                        randomTopStair.name = string.Concat(new object[]
                                        {
                        randomTopStair.name.Substring(0, randomTopStair.name.Length - 7),
                        " (",
                        bottomStairIndex,
                        ")"
                                        });
                                        randomTopStair.transform.position = Vector3.zero;
                                        randomTopStair.transform.parent = _roomsParent.transform;
                                        randomTopStair.RegionNode = new Vector3(_rooms[bottomStairIndex].RegionNode.x, (float)integerArray[bottomStairIndex], _rooms[bottomStairIndex].RegionNode.z);
                                        randomTopStair.RandomRegion();
                                        PositionRoomLG.PositionRoom(randomTopStair, integerArray[bottomStairIndex], (int)_rooms[bottomStairIndex].RegionNode.x, (int)_rooms[bottomStairIndex].RegionNode.z);
                                        if (randomTopStair != null)
                                        {
                                            for (int num9 = 0; num9 < randomTopStair.roomDoorData.Count; num9++)
                                            {
                                                RoomAppendageData.ConvertAppendageToRegionSpace(randomTopStair, randomTopStair.roomDoorData[num9]);
                                            }
                                            Room.HandleAppendageData(randomTopStair);
                                            finishedProcessingTopStairs = true;
                                            bool flag5 = false;
                                            for (int num10 = 0; num10 < topStairs.Count; num10++)
                                            {
                                                if (topStairs[num10] == processedTopStairs[index])
                                                {
                                                    flag5 = true;
                                                    break;
                                                }
                                            }
                                            if (flag5)
                                            {
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            processedTopStairs.RemoveAt(index);
                                        }
                                        if (processedTopStairs.Count == 0)
                                        {
                                            finishedProcessingTopStairs = true;
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                Debug.Log("Error while spawning top stairs.");
                            }
                            try
                            {
                                for (int stairSetToConnectStartingIndex = 0; stairSetToConnectStartingIndex < _rooms.Count; stairSetToConnectStartingIndex += stairsPerLevel)
                                {
                                    bool isStairConnected = true;
                                    for (int stairToCheck = 0; stairToCheck < _rooms.Count; stairToCheck++)
                                    {
                                        if (_rooms[stairSetToConnectStartingIndex] != _rooms[stairToCheck] && _rooms[stairSetToConnectStartingIndex].RoomFloor == _rooms[stairToCheck].RoomFloor) // If the stair set to connect starting index and the stair to check are not the same but are on the same level, try to connect them.
                                        {
                                            for (int stairExitIndex = 0; stairExitIndex < _rooms[stairSetToConnectStartingIndex].roomDoorData.Count; stairExitIndex++)
                                            {
                                                if (!_rooms[stairSetToConnectStartingIndex].roomDoorData[stairExitIndex].connected)
                                                {
                                                    isStairConnected = false;
                                                }
                                            }
                                            if (!isStairConnected && !ConnectRoomsLG.PathBetween2Rooms(_rooms[stairSetToConnectStartingIndex], _rooms[stairToCheck], null, null) && LevelGeneration.Instance.DebugMessages)
                                            {
                                                Debug.Log("Failed paths: Stairs, Room to Room. Room 1: " + _rooms[stairSetToConnectStartingIndex].name + " Room 2: " + _rooms[stairToCheck].name);
                                            }
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                Debug.Log("Error while connecting stairs 1.");
                            }
                            try
                            {
                                for (int stairToCheck = 0; stairToCheck < _rooms.Count; stairToCheck++)
                                {
                                    bool isStairConnected = true;
                                    for (int stairExitIndex = 0; stairExitIndex < _rooms[stairToCheck].roomDoorData.Count; stairExitIndex++)
                                    {
                                        if (!_rooms[stairToCheck].roomDoorData[stairExitIndex].connected && _rooms[stairToCheck].PrimaryRegion != PrimaryRegionType.OuterDeckCargo && !ConnectRoomsLG.PathBetweenRoomCorridor(_rooms[stairToCheck], _rooms[stairToCheck].roomDoorData[stairExitIndex], false))
                                        {
                                            for (int otherStairToCheck = 0; otherStairToCheck < _rooms.Count; otherStairToCheck++)
                                            {
                                                if (_rooms[stairToCheck] != _rooms[otherStairToCheck] && _rooms[stairToCheck].RoomFloor == _rooms[otherStairToCheck].RoomFloor)
                                                {
                                                    for (int otherStairToCheckExitIndex = 0; otherStairToCheckExitIndex < _rooms[otherStairToCheck].roomDoorData.Count; otherStairToCheckExitIndex++)
                                                    {
                                                        if (!_rooms[otherStairToCheck].roomDoorData[otherStairToCheckExitIndex].connected)
                                                        {
                                                            isStairConnected = false;
                                                        }
                                                    }
                                                    if (!isStairConnected && !ConnectRoomsLG.PathBetween2Rooms(_rooms[stairToCheck], _rooms[otherStairToCheck], null, null) && LevelGeneration.Instance.DebugMessages)
                                                    {
                                                        Debug.Log("Failed paths: Stairs, Room to Room. Room 1: " + _rooms[stairToCheck].name + " Room 2: " + _rooms[otherStairToCheck].name);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            catch
                            {
                                Debug.Log("Error while connecting stairs 2.");
                            }
                        }
                    }
                    catch
                    {
                        Debug.Log("Error in spawning stairs while loop.");
                    }
                }
                catch (Exception exception)
                {
                    ProcessLoadingError(loadingProgressText, loadingBackground, exception);
                }
            }

            private static void ProcessLoadingError(string loadingProgressText, LoadingBackground loadingBackground, Exception exception)
            {
                string loadingProgressTextError = "Error While " + loadingProgressText;
                Debug.Log(loadingProgressTextError + "\n" + exception.ToString());
                SetLoadingText(loadingBackground, loadingProgressTextError);
                ModSettings.errorDuringLevelGeneration = true;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SplashMovie

            private static void HookSplashMovie()
            {
                On.SplashMovie.PlayMovie += new On.SplashMovie.hook_PlayMovie(HookSplashMoviePlayMovie);
                On.SplashMovie.Start += new On.SplashMovie.hook_Start(HookSplashMovieStart);
            }

            private static void HookSplashMoviePlayMovie(On.SplashMovie.orig_PlayMovie orig, SplashMovie splashMovie)
            {
                if (!ModSettings.skipMenuScreen && !ModSettings.skipSplashScreen)
                {
                    orig.Invoke(splashMovie);
                }
            }

            private static void HookSplashMovieStart(On.SplashMovie.orig_Start orig, SplashMovie splashMovie)
            {
                if (ModSettings.skipMenuScreen || ModSettings.skipSplashScreen)
                {
                    splashMovie.loadingPercentage.SetActive(false);
                    ((MonoBehaviour)splashMovie).Invoke("BeginMovie", 0f);
                }
                else
                {
                    orig.Invoke(splashMovie);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @StartGame

            private static void HookStartGameStart(On.StartGame.orig_Start orig, StartGame startGame)
            {
                orig.Invoke(startGame);

                /*
                if (ModSettings.debugMode)
                {
                    GameObject[] allResources = Resources.FindObjectsOfTypeAll<GameObject>();

                    Debug.Log("Listing all resources:");
                    foreach (GameObject gameObject in allResources)
                    {
                        Debug.Log(gameObject.name);

                        if (gameObject.name.Equals("OptionsButton"))
                        {
                            Debug.Log("Found " + gameObject.name + ". Listing its components.");
                            Component[] gameObjectGameObjects = gameObject.GetComponents<Component>();
                            foreach(Component gameObject1 in gameObjectGameObjects)
                            {
                                Debug.Log(gameObject1.name);
                            }
                            Debug.Log("Done listing the components of " + gameObject.name + ".");
                        }
                    }
                    Debug.Log("Done listing resources.");
                }
                */

                if (!ModSettings.skippedMenuScreen)
                {
                    //startGame.StartGameDelay();
                    //startGame.StartCoroutine(WaitForStartButton(startGame));
                    //FindObjectOfType<StartButton>().OnButtonInteract();
                    //startGame.startButton.GetComponentInChildren<StartButton>().OnButtonInteract();
                    startGame.ButtonPressed();
                }
            }

            private static void HookStartGameButtonPressed(On.StartGame.orig_ButtonPressed orig, StartGame startGame)
            {
                if (ModSettings.skippedMenuScreen || ModSettings.alwaysSkipMenuScreen)
                {
                    ModSettings.ReadModSettings();
                }
                if (ModSettings.debugMode)
                {
                    Debugging.allowCheats = true; // Not sure whether this works.
                }
                orig.Invoke(startGame);
            }

            private static IEnumerator WaitForStartButton(StartGame startGame)
            {
                while (startGame.startButton.GetComponentInChildren<StartButton>() == null)
                {
                    yield return null;
                }
                startGame.startButton.GetComponentInChildren<StartButton>().OnButtonInteract();
                yield break;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SteamVariation

            private static void HookSteamVariationStart(On.SteamVariation.orig_Start orig, SteamVariation steamVariation)
            {
                // If overpowered steam vents is not enabled, allow the normal code to switch off all but one of the steam vent output point variations.
                if (!ModSettings.overpoweredSteamVents)
                {
                    orig.Invoke(steamVariation);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @SteamVentManager

            private static void HookSteamVentManager(On.SteamVentManager.orig_Awake orig, SteamVentManager steamVentManager)
            {
                steamVentManager.vents = new List<SteamVents>();
                if (!ModSettings.noSteam)
                {
                    steamVentManager.masterControlOverride = true;
                    return;
                }
                steamVentManager.masterControlOverride = false;
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @TimeScaleManager

            private static void HookTimeScaleManager()
            {
                //On.TimeScaleManager.ctor += new On.TimeScaleManager.hook_ctor(HookTimeScaleManagerConstructor); // Trying to log text in a constructor does not work. Debug.Log behavior in a constructor - Foniks - https://forum.unity.com/threads/debug-log-behavior-in-a-constructor.35435/ - Accessed 04.07.2021
                On.TimeScaleManager.LateUpdate += new On.TimeScaleManager.hook_LateUpdate(HookTimeScaleManagerLateUpdate);
            }

            private static void HookTimeScaleManagerConstructor(On.TimeScaleManager.orig_ctor orig, TimeScaleManager timeScaleManager)
            {
                Debug.Log("TimeScaleManager CTOR hook is working.");
                ModSettings.ReadModSettings();
                if (ModSettings.debugMode)
                {
                    Debugging.allowCheats = true; // Not sure whether this works.
                }
            }

            private static void HookTimeScaleManagerLateUpdate(On.TimeScaleManager.orig_LateUpdate orig, TimeScaleManager timeScaleManager)
            {
                if (timeScaleManager.loading || timeScaleManager.paused)
                {
                    if (timeScaleManager.paused && !ModSettings.noTimeFreezeInPauseMenu)
                    {
                        Time.timeScale = 0f;
                    }
                }
                else
                {
                    Time.timeScale = timeScaleManager.scale * ModSettings.timeScaleMultiplier;
                    ActiveFeatures();
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @TriggerNotification

            private static void HookTriggerNotificationSetNotificationString(On.TriggerNotification.orig_SetNotificationString orig, TriggerNotification triggerNotification, string _notification)
            {
                if (!ModSettings.hideTaskNotifications)
                {
                    _notification += ".";
                    orig.Invoke(triggerNotification, _notification);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @TutorialDoor

            private static void HookTutorialDoor()
            {
                On.TutorialDoor.Update += new On.TutorialDoor.hook_Update(HookTutorialDoorUpdate);
                On.TutorialDoor.OnDoorLocked += new On.TutorialDoor.hook_OnDoorLocked(HookTutorialDoorOnDoorLocked);
            }

            private static void HookTutorialDoorUpdate(On.TutorialDoor.orig_Update orig, TutorialDoor tutorialDoor)
            {

            }

            private static void HookTutorialDoorOnDoorLocked(On.TutorialDoor.orig_OnDoorLocked orig, TutorialDoor tutorialDoor)
            {
                if (tutorialDoor.door != null)
                {
                    orig.Invoke(tutorialDoor);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @Vision

            private static void HookVisionPlayerVision(On.Vision.orig_PlayerVision orig, Vision vision)
            {
                if (ModSettings.InvisibleMode || (ModSettings.foggyShip && ModSettings.monsterVisionAffectedByFog && ManyMonstersMode.PlayerToMonsterDistance(vision.monster) > ModSettings.fogDistance))
                {
                    vision.playerTotal = 0f;
                    vision.lightTotal = 0f;
                }
                else
                {
                    orig.Invoke(vision);
                }
            }

            /*----------------------------------------------------------------------------------------------------*/
            // @WalkieTalkie

            private static void HookWalkieTalkie()
            {
                On.WalkieTalkie.DestroyWT += new On.WalkieTalkie.hook_DestroyWT(HookWalkieTalkieDestroyWT);
                On.WalkieTalkie.OnHandGrab += new On.WalkieTalkie.hook_OnHandGrab(HookWalkieTalkieOnHandGrab);
                On.WalkieTalkie.OnUseItem += new On.WalkieTalkie.hook_OnUseItem(HookWalkieTalkieOnUseItem);
                On.WalkieTalkie.Update += new On.WalkieTalkie.hook_Update(HookWalkieTalkieUpdate);
            }

            private static void HookWalkieTalkieDestroyWT(On.WalkieTalkie.orig_DestroyWT orig, WalkieTalkie walkieTalkie)
            {
                walkieTalkie.destroyed = true;
                //walkieTalkie.other.destroyed = true; // Original line that shouldn't occur as there will be more than 2 walkie talkies.
                DestroyableObject destroyableObject = ((MonoBehaviour)walkieTalkie).transform.GetComponent<DestroyableObject>();
                if (destroyableObject != null)
                {
                    destroyableObject.DestroyObject();
                }
            }

            private static void HookWalkieTalkieOnHandGrab(On.WalkieTalkie.orig_OnHandGrab orig, WalkieTalkie walkieTalkie)
            {
                try
                {
                    if (walkieTalkie.pickUpOnce)
                    {
                        /*
                        if (walkieTalkie.other != null)
                        {
                            walkieTalkie.other.SendMessage("OnHandGrab", SendMessageOptions.DontRequireReceiver); // Original line of code that does not work without a null check.
                        }
                        // Or maybe it doesn't work even with a null check? First it gives a null reference and now the render.RemoveAt index is out of range when keeping this. The other walkietalkie is now also only unique when both have been picked up.
                        */
                        try
                        {
                            walkieTalkie.pickUpOnce = false;
                        }
                        catch
                        {
                            Debug.Log("Error in WalkieTalkieOnHandGrab2");
                        }
                        try
                        {
                            walkieTalkie.glow.render.RemoveAt(1);
                        }
                        catch
                        {
                            Debug.Log("Error in WalkieTalkieOnHandGrab 3");
                        }
                        try
                        {
                            walkieTalkie.glow.SaveShaders();
                        }
                        catch
                        {
                            Debug.Log("Error in WalkieTalkieOnHandGrab 4");
                        }
                        try
                        {
                            walkieTalkie.glow.hoverOver = false;
                        }
                        catch
                        {
                            Debug.Log("Error in WalkieTalkieOnHandGrab 5");
                        }
                    }
                }
                catch
                {
                    Debug.Log("Error in WalkieTalkieOnHandGrab 1");
                }
                try
                {
                    walkieTalkie.OnEndReceive();
                    walkieTalkie.receiveLED.On = false;
                }
                catch
                {
                    Debug.Log("Error in WalkieTalkieOnHandGrab 6");
                }
            }

            private static void HookWalkieTalkieOnUseItem(On.WalkieTalkie.orig_OnUseItem orig, WalkieTalkie walkieTalkie)
            {
                if (!walkieTalkie.destroyed)
                {
                    bool placingWalkieTalkie = false;
                    Inventory inventory;
                    if (!ModSettings.enableMultiplayer)
                    {
                        inventory = References.Inventory;
                    }
                    else
                    {
                        inventory = MultiplayerMode.InventoryFromItemClass(walkieTalkie.gameObject);
                    }
                    if (inventory.CurrentItem == walkieTalkie.item && inventory.CurrentSlot.StackCount >= 2)
                    {
                        inventory.DropItem(inventory.CurrentSlot);
                        walkieTalkie.other = inventory.CurrentItem.GetComponent<WalkieTalkie>();
                        walkieTalkie.other.other = walkieTalkie;
                        placingWalkieTalkie = true;
                    }
                    if (!walkieTalkie.transmitLED.On)
                    {
                        if (!placingWalkieTalkie)
                        {
                            if (!walkieTalkie.outOfRangeLED.On)
                            {
                                walkieTalkie.PlaySound();
                                walkieTalkie.OnTransmit();
                                return;
                            }
                            walkieTalkie.PlaySound();
                            return;
                        }
                    }
                    else
                    {
                        walkieTalkie.OnEndTransmit();
                    }
                }
            }

            private static void HookWalkieTalkieUpdate(On.WalkieTalkie.orig_Update orig, WalkieTalkie walkieTalkie)
            {
                if (!walkieTalkie.pickUpOnce)
                {
                    if (walkieTalkie.source.isPlaying)
                    {
                        if (walkieTalkie.dSound == null)
                        {
                            walkieTalkie.dSound = walkieTalkie.source.GetComponent<DistractionSound>();
                        }
                        else
                        {
                            walkieTalkie.dSound.SetMonsterApproach(new DistractionSound.MonsterApproach(walkieTalkie.MonsterApproach));
                        }
                    }
                    if (walkieTalkie.other != null && walkieTalkie.other != walkieTalkie && !walkieTalkie.other.destroyed && Vector3.Distance(walkieTalkie.other.transform.position, ((MonoBehaviour)walkieTalkie).transform.position) < walkieTalkie.range)
                    {
                        walkieTalkie.outOfRangeLED.On = false;
                    }
                    else
                    {
                        Inventory inventory;
                        if (!ModSettings.enableMultiplayer)
                        {
                            inventory = References.Inventory;
                        }
                        else
                        {
                            if (walkieTalkie.item.IsInInventory() == false && walkieTalkie.other != null)
                            {
                                inventory = MultiplayerMode.InventoryFromItemClass(walkieTalkie.other.gameObject);
                            }
                            else
                            {
                                inventory = MultiplayerMode.InventoryFromItemClass(walkieTalkie.gameObject);
                            }
                        }
                        if (walkieTalkie.transmitLED.On)
                        {
                            walkieTalkie.OnEndTransmit();
                        }
                        if (inventory.CurrentItem != walkieTalkie.item)
                        {
                            walkieTalkie.outOfRangeLED.On = true;
                        }
                        else if (inventory.CurrentSlot.StackCount >= 2)
                        {
                            walkieTalkie.outOfRangeLED.On = false;
                        }
                        else
                        {
                            walkieTalkie.outOfRangeLED.On = true;
                        }
                    }
                    if (walkieTalkie.receiving && !walkieTalkie.outOfRangeLED.On)
                    {
                        if (walkieTalkie.receivingTime < walkieTalkie.minTime)
                        {
                            walkieTalkie.receivingTime = walkieTalkie.minTime;
                        }
                        walkieTalkie.receivingTime += Time.deltaTime;
                        walkieTalkie.receivingTime = Mathf.Clamp(walkieTalkie.receivingTime, 0f, walkieTalkie.maxTime);
                    }
                    else
                    {
                        walkieTalkie.receivingTime -= Time.deltaTime;
                        walkieTalkie.receivingTime = Mathf.Clamp(walkieTalkie.receivingTime, 0f, walkieTalkie.maxTime);
                    }
                    if (walkieTalkie.receivingTime >= walkieTalkie.maxTime || walkieTalkie.receiveLED.On)
                    {
                        if (walkieTalkie.receivingTime != 0f)
                        {
                            if (!walkieTalkie.receiveLED.On)
                            {
                                walkieTalkie.receiveLED.On = true;
                                walkieTalkie.OnReceive();
                            }
                        }
                        else if (walkieTalkie.receiveLED.On)
                        {
                            walkieTalkie.receiveLED.On = false;
                            walkieTalkie.OnEndReceive();
                        }
                    }
                }
            }

        }
    }
}
// ~End Of File