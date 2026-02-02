using UnityEngine;
using System.Collections.Generic;
using SRF;
using MonstrumExtendedSettingsMod.Enum;

namespace MonstrumExtendedSettingsMod.Setting
{
    class DeckCargoHolds : Setting
    {
        private bool hookedCrateBlockPaths = false;

        protected override bool ShouldSettingBeEnabled()
        {
            return ExtendedSettingsModScript.ModSettings.deckCargoHolds;
        }

        protected override void OnEnable()
        {
            // Removes cargo walls from deck 5.
            On.SpawnCargoHoldLG.HorizontalOrVerticalWall += HookSpawnCargoHoldLGHorizontalOrVerticalWall;
            // Stops deck walls from spawning in front of cargo containers.
            On.SpawnDeckCargoWalls.GenerateDeckCargoWall += HookSpawnDeckCargoWallsGenerateDeckCargoWall;
            // Fakes the connection between the deck and the cargo containers to stop culling.
            On.PositionRoomLG.CheckAdjacentNodeForConnection += HookPositionRoomLGCheckAdjacentNodeForConnection;
        }

        protected override void OnDisable()
        {
            On.SpawnCargoHoldLG.HorizontalOrVerticalWall -= HookSpawnCargoHoldLGHorizontalOrVerticalWall;
            On.SpawnDeckCargoWalls.GenerateDeckCargoWall -= HookSpawnDeckCargoWallsGenerateDeckCargoWall;
            On.PositionRoomLG.CheckAdjacentNodeForConnection -= HookPositionRoomLGCheckAdjacentNodeForConnection;

            if (hookedCrateBlockPaths)
            {
                On.SpawnCargoContainersLG.SpawnCrateBlockPaths -= HookSpawnCargoContainersLGSpawnCrateBlockPaths;
                On.SpawnCargoContainersLG.SpawnBrokenRails -= HookSpawnCargoContainersLGSpawnBrokenRails;
                hookedCrateBlockPaths = false;
            }
        }

        public override void EarlyInitialisation()
        {
            if (!hookedCrateBlockPaths)
            {
                On.SpawnCargoContainersLG.SpawnCrateBlockPaths += HookSpawnCargoContainersLGSpawnCrateBlockPaths;
                On.SpawnCargoContainersLG.SpawnBrokenRails += HookSpawnCargoContainersLGSpawnBrokenRails;

                hookedCrateBlockPaths = true;
            }
        }

        public override void LateInitialisation()
        {
            // Bodge to remove awkard corner pieces spawned inside SpawnCargoHoldLG.SpawnCargoArea (num5 == 2). 2 => corner.
            foreach (Transform child in SpawnCargoHoldLG.shellParent.transform)
            {
                Vector3 node = RegionManager.Instance.ConvertPointToRegionNode(child.transform.position);
                if (
                    node.y == 5 &&
                    (node.x >= 24 && node.x <= 25 || node.x >= 35 && node.x <= 36) &&
                    (node.z >= 2 && node.z <= 3 || node.z >= 13 && node.z <= 14)
                )
                {
                    Object.Destroy(child.gameObject);
                }
            }
        }

        private static void HookPositionRoomLGCheckAdjacentNodeForConnection(On.PositionRoomLG.orig_CheckAdjacentNodeForConnection orig, Room _room, Vector3 _moddedNode, Vector3 _originalNode, NodeData _activeNode, int _UDLRFBIndex)
        {
            if (CheckBoundariesLG.NodeWithinShipBounds(_moddedNode))
            {
                NodeData nodeData = LevelGeneration.Instance.nodeData[(int)_moddedNode.x][(int)_moddedNode.y][(int)_moddedNode.z];
                int oppositeDirection = NodeData.GetOppositeDirection(_UDLRFBIndex);
                if (_UDLRFBIndex == 0 && ((nodeData.nodeType == RoomStructure.None && _room.PrimaryRegion == PrimaryRegionType.Engine) || ((nodeData.nodeType == RoomStructure.Inaccessible || nodeData.nodeType == RoomStructure.None) && (_room.PrimaryRegion == PrimaryRegionType.OuterDeck || _room.PrimaryRegion == PrimaryRegionType.OuterDeckCargo || _room.PrimaryRegion == PrimaryRegionType.CargoHold))))
                {
                    if (_room.PrimaryRegion == PrimaryRegionType.Engine)
                    {
                        if (_room.name.Contains("LargeEngine"))
                        {
                            if (_activeNode.regionNode.y >= 3f)
                            {
                                PositionRoomLG.mainEngineID = RegionManager.Instance.StringToRegionID("Engines_MainArea");
                                if (RegionManager.Instance.CheckNodeForRegionExclusive(_moddedNode, PositionRoomLG.mainEngineID, 1) || !RegionManager.Instance.CheckNodeForRegion(_moddedNode, PositionRoomLG.mainEngineID, -1))
                                {
                                    PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                                }
                            }
                        }
                        else
                        {
                            PositionRoomLG.mainEngineID = RegionManager.Instance.StringToRegionID("Engines_MainArea");
                            if (RegionManager.Instance.CheckNodeForRegionExclusive(_moddedNode, PositionRoomLG.mainEngineID, 1) || !RegionManager.Instance.CheckNodeForRegion(_moddedNode, PositionRoomLG.mainEngineID, -1))
                            {
                                PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                            }
                        }
                    }
                    else
                    {
                        if (_room.PrimaryRegion == PrimaryRegionType.CargoHold && _room.RoomType == RoomStructure.Walkway && _room.WalkwayType == WalkwayStructure.Cargo && _room.RoomConnectionsType != ConnectorType.Room && _room.RegionNode.y < 4f)
                        {
                            return;
                        }
                        PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                    }
                }
                if (_UDLRFBIndex < 2)
                {
                    if (!CheckBoundariesLG.NodeOutwithRoom(_room, _moddedNode))
                    {
                        if (_room.name.Contains("LargeEngine"))
                        {
                            if (_activeNode.regionNode.y >= 3f || _activeNode.regionNode.x == 5f)
                            {
                                PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                            }
                        }
                        else
                        {
                            if (_room.PrimaryRegion == PrimaryRegionType.CargoHold && _room.RoomType == RoomStructure.Walkway && _room.WalkwayType == WalkwayStructure.Cargo && _room.RoomConnectionsType != ConnectorType.Room && _room.RegionNode.y < 4f)
                            {
                                return;
                            }
                            PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                        }
                    }
                    if (_room.HasTag("Stairs") && nodeData.nodeRoom != null && nodeData.nodeRoom.HasTag("Stairs"))
                    {
                        PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                    }
                }
                else
                {
                    if (_UDLRFBIndex == 2 && RoomAppendageData.CheckAppendageList<NoCullingAppendage>(_originalNode, Orientation.Vertical))
                    {
                        return;
                    }
                    if (_UDLRFBIndex == 3 && RoomAppendageData.CheckAppendageList<NoCullingAppendage>(_originalNode + Vector3.right, Orientation.Vertical))
                    {
                        return;
                    }
                    if (_UDLRFBIndex == 4 && RoomAppendageData.CheckAppendageList<NoCullingAppendage>(_originalNode + Vector3.forward, Orientation.Horizontal))
                    {
                        return;
                    }
                    if (_UDLRFBIndex == 5 && RoomAppendageData.CheckAppendageList<NoCullingAppendage>(_originalNode, Orientation.Horizontal))
                    {
                        return;
                    }
                    if (!CheckBoundariesLG.NodeOutwithRoom(_room, _moddedNode))
                    {
                        PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                    }
                    if (_UDLRFBIndex == 2 || _UDLRFBIndex == 3)
                    {
                        Vector3 regionNode = Vector3.zero;
                        if (_UDLRFBIndex == 2)
                        {
                            regionNode = _originalNode;
                        }
                        else
                        {
                            regionNode = _moddedNode;
                        }
                        if (_room.roomDoorData.Count > 0 && RoomAppendageData.CheckAppendageList<DoorData>(regionNode, Orientation.Vertical))
                        {
                            PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                        }
                        if (_room.roomFloorJointData.Count > 0 && RoomAppendageData.CheckAppendageList<JoiningFloorData>(regionNode, Orientation.Vertical))
                        {
                            _activeNode.connectedNodesUDLRFB[_UDLRFBIndex] = true;
                            nodeData.connectedNodesUDLRFB[oppositeDirection] = true;
                        }
                        if (_room.roomWindowData.Count > 0 && RoomAppendageData.CheckAppendageList<WindowData>(regionNode, Orientation.Vertical))
                        {
                            PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                        }
                        if (_room.LOSJointData.Count > 0 /*&& RoomAppendageData.CheckAppendageList<LineOfSightData>(regionNode, Orientation.Vertical)*/)
                        {
                            var _regionNode = regionNode;
                            var _orient = Orientation.Vertical;
                            var check = false;

                            var text = $"Checking vertical appendage data for {_regionNode}, which has orientation {_orient}.";
                            if (CheckBoundariesLG.NodeWithinShipBounds(_regionNode))
                            {
                                RoomAppendageData.appData = LevelGeneration.Instance.nodeData[(int)_regionNode.x][(int)_regionNode.y][(int)_regionNode.z].appendageData;

                                for (int i = 0; i < RoomAppendageData.appData.Count; i++)
                                {
                                    if (RoomAppendageData.appData[i] is LineOfSightData)
                                    {
                                        text += $" Checking appendage data {i} {RoomAppendageData.appData[i].regionNode}. This has orientation {RoomAppendageData.appData[i].currentOrientation}.";
                                        if (_orient == Orientation.None)
                                        {
                                            text += " Passed via orientation none.";
                                            check = true;
                                            break;
                                        }
                                        if (RoomAppendageData.appData[i].currentOrientation == _orient)
                                        {
                                            text += " Passed via matching orientation.";
                                            check = true;
                                            break;
                                        }
                                        if (_room.RoomType == RoomStructure.Cargo && RoomAppendageData.appData[i].joiningRoom.RoomType == RoomStructure.Deck || _room.RoomType == RoomStructure.Deck && RoomAppendageData.appData[i].joiningRoom.RoomType == RoomStructure.Cargo)
                                        {
                                            check = true;
                                            break;
                                        }
                                    }
                                }

                                // if ((_regionNode.x == 23 || _regionNode.x == 24 || _regionNode.x == 35 || _regionNode.x == 36) && _regionNode.y == 5 && (_room.name.Contains("CargoContainer") || _room.name.Contains("Deck_CargoWalkway")))
                                // {
                                //     Debug.Log($"Room: {_room}. Room region: {_room.PrimaryRegion}. Room coords: {_room.regionNode}. Cargo container has following appendage: {RoomAppendageData.appData.Count}");
                                //     foreach (RoomAppendageData data in RoomAppendageData.appData)
                                //     {
                                //         Debug.Log($"JR: {data.joiningRoom}. Join room coords: {data.joiningRoom.RegionNode}. Join room type: {data.joiningRoom.PrimaryRegion}. IR: {data.initialOrientation}. Node: {data.regionNode}. x: {data.x}. z: {data.z}. Rot: {data.rotationQuadrant}");
                                //     }
                                //     check = true;
                                // }
                            }



                            // THIS CHECK IS WHAT ALLOWS THEM TO JOIN
                            // PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                            if (check)
                            {

                                text += " Passed check.";
                                PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                            }
                            else
                            {
                                text += " Failed check";
                            }
                            // Debug.Log(text);
                        }
                    }
                    else
                    {
                        Vector3 regionNode2 = Vector3.zero;
                        if (_UDLRFBIndex == 5)
                        {
                            regionNode2 = _originalNode;
                        }
                        else
                        {
                            regionNode2 = _moddedNode;
                        }
                        if (_room.roomDoorData.Count > 0 && RoomAppendageData.CheckAppendageList<DoorData>(regionNode2, Orientation.Horizontal))
                        {
                            PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                        }
                        if (_room.roomFloorJointData.Count > 0 && RoomAppendageData.CheckAppendageList<JoiningFloorData>(regionNode2, Orientation.Horizontal))
                        {
                            PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                        }
                        if (_room.roomWindowData.Count > 0 && RoomAppendageData.CheckAppendageList<WindowData>(regionNode2, Orientation.Horizontal))
                        {
                            PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                        }
                        if (_room.LOSJointData.Count > 0 /*&& RoomAppendageData.CheckAppendageList<LineOfSightData>(regionNode2, Orientation.Horizontal)*/)
                        {
                            var _regionNode = regionNode2;
                            var _orient = Orientation.Horizontal;
                            var check = false;

                            var text = $"Checking horizontal appendage data for {_regionNode}, which has orientation {_orient}.";
                            if (CheckBoundariesLG.NodeWithinShipBounds(_regionNode))
                            {
                                RoomAppendageData.appData = LevelGeneration.Instance.nodeData[(int)_regionNode.x][(int)_regionNode.y][(int)_regionNode.z].appendageData;
                                for (int i = 0; i < RoomAppendageData.appData.Count; i++)
                                {
                                    if (RoomAppendageData.appData[i] is LineOfSightData)
                                    {
                                        text += $" Checking appendage data {i} ({RoomAppendageData.appData[i].regionNode}). This has orientation {RoomAppendageData.appData[i].currentOrientation}.";
                                        if (_orient == Orientation.None)
                                        {
                                            text += " Passed via orientation none.";
                                            check = true;
                                            break;
                                        }
                                        if (RoomAppendageData.appData[i].currentOrientation == _orient)
                                        {
                                            text += " Passed via matching orientation.";
                                            check = true;
                                            break;
                                        }
                                    }
                                }
                            }

                            // PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                            if (check)
                            {
                                text += " Passed check.";
                                PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                            }
                            else
                            {
                                text += " Failed check";
                            }
                            // Debug.Log(text);
                        }
                    }
                }
            }
        }

        private static void HookSpawnDeckCargoWallsGenerateDeckCargoWall(On.SpawnDeckCargoWalls.orig_GenerateDeckCargoWall orig, GameObject _parentObj, int _deckConBaseID, Vector3 _node)
        {
            bool[] array = new bool[4];
            if (RegionManager.Instance.CheckNodeForRegion(_node + Vector3.right, _deckConBaseID, -1))
            {
                array[0] = true;
            }
            if (RegionManager.Instance.CheckNodeForRegion(_node + Vector3.left, _deckConBaseID, -1))
            {
                array[1] = true;
            }
            if (RegionManager.Instance.CheckNodeForRegion(_node + Vector3.forward, _deckConBaseID, -1))
            {
                array[2] = true;
            }
            if (RegionManager.Instance.CheckNodeForRegion(_node + Vector3.back, _deckConBaseID, -1))
            {
                array[3] = true;
            }
            Debug.Log($"Adjacencies: Right {array[0]}. Left {array[1]}. Forward {array[2]}. Back {array[3]}.");
            bool flag = false;
            if (array[2] && array[3])
            {
                var hasExtendedHoldBehindWall = RegionManager.Instance.CheckNodeForRegion(_node + Vector3.right, (int)RegionId.CargoMainHold) || RegionManager.Instance.CheckNodeForRegion(_node + Vector3.left, (int)RegionId.CargoMainHold);
                if (hasExtendedHoldBehindWall)
                {
                    return;
                }
                GameObject gameObject;
                if (CheckBoundariesLG.NodeWithinShipBounds(_node + Vector3.forward * 3f) && CheckBoundariesLG.NodeWithinShipBounds(_node + Vector3.back * 3f))
                {
                    Debug.Log("Front wall 1 at " + _node);
                    gameObject = SpawnDeckCargoWalls.CreateWallPiece(_node, SpawnDeckCargoWalls.frontWall02, _parentObj);
                }
                else
                {
                    Debug.Log("Front wall 2 at " + _node);
                    gameObject = SpawnDeckCargoWalls.CreateWallPiece(_node, SpawnDeckCargoWalls.frontWall01, _parentObj);
                    if ((!CheckBoundariesLG.NodeWithinShipBounds(_node + Vector3.forward * 3f) && !RegionManager.Instance.CheckNodeForRegion(_node + Vector3.left, SpawnDeckCargoWalls.deckCargoWalkwaysID, -1)) || (!CheckBoundariesLG.NodeWithinShipBounds(_node + Vector3.back * 3f) && !RegionManager.Instance.CheckNodeForRegion(_node + Vector3.right, SpawnDeckCargoWalls.deckCargoWalkwaysID, -1)))
                    {
                        gameObject.transform.localScale = new Vector3(-1f, 1f, 1f);
                    }
                }
                if (RegionManager.Instance.CheckNodeForRegion(_node + Vector3.right, SpawnDeckCargoWalls.deckCargoWalkwaysID, -1))
                {
                    gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));
                    if (gameObject.transform.localScale.x == 1f)
                    {
                        gameObject.transform.localPosition = gameObject.transform.localPosition + new Vector3(0f, 0f, 2f);
                    }
                }
                else
                {
                    gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 270f, 0f));
                    gameObject.transform.localPosition = gameObject.transform.localPosition + new Vector3(2f, 0f, 0f);
                    if (gameObject.transform.localScale.x == -1f)
                    {
                        gameObject.transform.localPosition = gameObject.transform.localPosition + new Vector3(0f, 0f, 2f);
                    }
                }
            }
            else if ((array[0] && array[2]) || (array[1] && array[2]) || (array[1] && array[3]) || (array[0] && array[3]))
            {
                Debug.Log("Corner at " + _node);
                GameObject gameObject = SpawnDeckCargoWalls.CreateWallPiece(_node, SpawnDeckCargoWalls.corner, _parentObj);
                if (RegionManager.Instance.CheckNodeForRegion(_node + Vector3.right, _deckConBaseID, -1) && RegionManager.Instance.CheckNodeForRegion(_node + Vector3.forward, _deckConBaseID, -1) && !RegionManager.Instance.CheckNodeForRegion(_node + Vector3.right + Vector3.forward, _deckConBaseID, -1))
                {
                    gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
                    gameObject.transform.localPosition = gameObject.transform.localPosition + new Vector3(2f, 0f, 2f);
                    flag = true;
                }
                else if (RegionManager.Instance.CheckNodeForRegion(_node + Vector3.left, _deckConBaseID, -1) && RegionManager.Instance.CheckNodeForRegion(_node + Vector3.forward, _deckConBaseID, -1) && !RegionManager.Instance.CheckNodeForRegion(_node + Vector3.left + Vector3.forward, _deckConBaseID, -1))
                {
                    gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 90f, 0f));
                    gameObject.transform.localPosition = gameObject.transform.localPosition + new Vector3(0f, 0f, 2f);
                }
                else if (RegionManager.Instance.CheckNodeForRegion(_node + Vector3.right, _deckConBaseID, -1) && RegionManager.Instance.CheckNodeForRegion(_node + Vector3.back, _deckConBaseID, -1) && !RegionManager.Instance.CheckNodeForRegion(_node + Vector3.right + Vector3.back, _deckConBaseID, -1))
                {
                    gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 270f, 0f));
                    gameObject.transform.localPosition = gameObject.transform.localPosition + new Vector3(2f, 0f, 0f);
                    flag = true;
                }
                if (_node.x < 37f || _node.x > 52f)
                {
                    Debug.Log("Support at " + _node);
                    gameObject = SpawnDeckCargoWalls.CreateWallPiece(_node, SpawnDeckCargoWalls.support01, _parentObj);
                    if (CheckBoundariesLG.NodeWithinShipBounds(_node + Vector3.forward * 3f))
                    {
                        gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
                        gameObject.transform.localPosition = gameObject.transform.localPosition + new Vector3(0f, 0f, 2f);
                    }
                    if (flag)
                    {
                        gameObject.transform.localPosition = gameObject.transform.localPosition + new Vector3(2f, 0f, 0f);
                    }
                }
            }
            else
            {
                bool flag2 = _node.x == 25f || _node.x == 34f || _node.x == 39f || _node.x == 48f || _node.x == 53f || _node.x == 59f;
                GameObject gameObject;
                if (flag2)
                {
                    Debug.Log("Locker at " + _node);
                    gameObject = SpawnDeckCargoWalls.CreateWallPiece(_node, SpawnDeckCargoWalls.locker, _parentObj);
                    gameObject.transform.parent = LevelGeneration.Instance.nodeData[(int)_node.x][(int)_node.y][(int)_node.z].nodeRoom.transform;
                    gameObject.transform.localPosition = Vector3.zero;
                    gameObject.transform.localRotation = Quaternion.identity;
                }
                else
                {
                    Debug.Log("Special wall 1 at " + _node);
                    gameObject = SpawnDeckCargoWalls.CreateWallPiece(_node, SpawnDeckCargoWalls.sideWalls[UnityEngine.Random.Range(0, SpawnDeckCargoWalls.sideWalls.Count)], _parentObj);
                }
                if (_node.z == 1f && !flag2)
                {
                    gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
                    gameObject.transform.localPosition = gameObject.transform.localPosition + new Vector3(2f, 0f, 2f);
                }
            }
            if (_node.x % 3f == 0f && (!CheckBoundariesLG.NodeWithinShipBounds(_node + Vector3.forward * 2f) || !CheckBoundariesLG.NodeWithinShipBounds(_node + Vector3.back * 2f)) && (_node.x < 37f || _node.x > 52f))
            {
                Debug.Log("Special wall 2 at " + _node);
                GameObject gameObject = SpawnDeckCargoWalls.CreateWallPiece(_node, SpawnDeckCargoWalls.support01, _parentObj);
                if (CheckBoundariesLG.NodeWithinShipBounds(_node + Vector3.forward * 5f))
                {
                    gameObject.transform.localRotation = Quaternion.Euler(new Vector3(0f, 180f, 0f));
                    gameObject.transform.localPosition = gameObject.transform.localPosition + new Vector3(0f, 0f, 2f);
                }
                if (_node.x > 37f)
                {
                    gameObject.transform.localPosition = gameObject.transform.localPosition + new Vector3(-2f, 0f, 0f);
                }
                if (_node.x > 52f)
                {
                    gameObject.transform.localPosition = gameObject.transform.localPosition + new Vector3(-2f, 0f, 0f);
                }
            }
        }

        private static void AddStandardCrates(Vector3Int start, Vector3Int end)
        {
            for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
            {
                for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                {
                    for (int z = Mathf.Min(start.z, end.z); z <= Mathf.Max(start.z, end.z); z++)
                    {
                        SpawnCargoContainersLG.stdCrateNodes.Add(new Vector3(x, y, z));
                    }
                }
            }
        }

        private static void HookSpawnCargoContainersLGSpawnCrateBlockPaths(On.SpawnCargoContainersLG.orig_SpawnCrateBlockPaths orig, float _leftSideX, float _rightSideX, List<int> _nodeFrontIndices, List<int> _nodeBackIndices, int _catWalkID, bool _altPathLeft)
        {
            List<Vector3> leftPoints = new List<Vector3>();
            List<Vector3> rightPoints = new List<Vector3>();
            int numberOfPaths = 2;
            int numberOfAltPaths = 1;
            int cratesToSpawn = 4;
            int maxY = 4;
            if (_leftSideX == 24f) // ONLY IF OUTER DECK CARGO EXT
            {
                maxY = 5;
                // Add crates to fill in the walls on deck 5 (with support below).
                AddStandardCrates(new Vector3Int(24, 1, 2), new Vector3Int(24, 5, 13));
                AddStandardCrates(new Vector3Int(33, 1, 2), new Vector3Int(33, 5, 13));
                numberOfPaths += 2;
            }
            if (_rightSideX == 44f)
            {
                maxY = 3;
            }
            for (int y = 1; y <= maxY; y++)
            {
                for (int z = 0; z < 4; z++)
                {
                    leftPoints.Add(new Vector3(_leftSideX, y, z * 3 + 2));
                    rightPoints.Add(new Vector3(_rightSideX, y, z * 3 + 2));
                }
            }
            for (int pathsSpawned = 0; pathsSpawned < numberOfPaths; pathsSpawned++)
            {
                // Guarantee that there is at least one path at max height when using exterior cargo holds.
                var leftList = pathsSpawned == 0 && maxY == 5 ? leftPoints.FindAll(point => point.y == maxY) : leftPoints;
                SpawnCargoContainersLG.pointOne = leftList.Random();
                leftPoints.Remove(SpawnCargoContainersLG.pointOne);
                var rightList = pathsSpawned == 1 && maxY == 5 ? rightPoints.FindAll(point => point.y == maxY) : rightPoints;
                SpawnCargoContainersLG.pointTwo = rightList.Random();
                rightPoints.Remove(SpawnCargoContainersLG.pointTwo);
                SpawnCargoContainersLG.SpawnMainPath(SpawnCargoContainersLG.pointOne, SpawnCargoContainersLG.pointTwo, _nodeBackIndices, _nodeFrontIndices, ref SpawnCargoContainersLG.prePathCount, ref SpawnCargoContainersLG.postPathCount);
            }
            for (int altPathsSpawned = 0; altPathsSpawned < numberOfAltPaths; altPathsSpawned++)
            {
                SpawnCargoContainersLG.pointOne = SpawnCargoContainersLG.cargoNodes[UnityEngine.Random.Range(SpawnCargoContainersLG.prePathCount, SpawnCargoContainersLG.postPathCount)].node;
                if (_altPathLeft)
                {
                    SpawnCargoContainersLG.pointTwo = leftPoints[UnityEngine.Random.Range(0, leftPoints.Count)];
                    leftPoints.Remove(SpawnCargoContainersLG.pointTwo);
                }
                else
                {
                    SpawnCargoContainersLG.pointTwo = rightPoints[UnityEngine.Random.Range(0, rightPoints.Count)];
                    rightPoints.Remove(SpawnCargoContainersLG.pointTwo);
                }
                SpawnCargoContainersLG.SpawnAlternatePath(SpawnCargoContainersLG.pointOne, SpawnCargoContainersLG.pointTwo, _nodeBackIndices, _nodeFrontIndices, ref SpawnCargoContainersLG.prePathCount, ref SpawnCargoContainersLG.postPathCount, _catWalkID);
            }
            if (LevelGeneration.Instance.GetMonster.name.Contains("Hunter"))
            {
                SpawnCargoContainersLG.SpawnSideCrates(cratesToSpawn, SpawnCargoContainersLG.totalCratesPrePaths, SpawnCargoContainersLG.postPathCount);
            }
            SpawnCargoContainersLG.totalCratesPrePaths = SpawnCargoContainersLG.cargoNodes.Count;
        }

        private static void HookSpawnCargoContainersLGSpawnBrokenRails(On.SpawnCargoContainersLG.orig_SpawnBrokenRails orig, Vector3 _regionNode, List<Room> _roomsInUse, GameObject _cargoParent, GameObject _railing)
        {
            if (_regionNode.y < 5)
            {
                orig.Invoke(_regionNode, _roomsInUse, _cargoParent, _railing);
            }
        }

        private static GameObject HookSpawnCargoHoldLGHorizontalOrVerticalWall(On.SpawnCargoHoldLG.orig_HorizontalOrVerticalWall orig, int _horIndex, int _vertIndex, Vector3 _regionNode, int _cargoID, RoomStructure[] _nodeChecks)
        {
            if (_regionNode.y == 5f)
            {
                return null;
            }
            return orig.Invoke(_horIndex, _vertIndex, _regionNode, _cargoID, _nodeChecks);
        }
    }
}