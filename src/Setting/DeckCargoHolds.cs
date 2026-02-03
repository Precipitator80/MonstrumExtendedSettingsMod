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
            // The left corner depends on whether an additional crew deck building is being added.
            List<int> boundaryNodeXCoords = GetBoundaryNodeXCoords();

            // Destroy all the children of the shell parent game object at the expected coordinates, which should only include the corners.
            foreach (Transform child in SpawnCargoHoldLG.shellParent.transform)
            {
                Vector3 node = RegionManager.Instance.ConvertPointToRegionNode(child.transform.position);
                if (boundaryNodeXCoords.Contains((int)node.x) && node.y == 5 && (node.z >= 2 && node.z <= 3 || node.z >= 13 && node.z <= 14))
                {
                    Object.Destroy(child.gameObject);
                }
            }
        }

        /// <summary>
        /// Adds standard crates to the given range of nodes.
        /// Standard crates are guaranteed to spawn, making them useful for filling space.
        /// </summary>
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

        /// <summary>
        /// Gets a list of x coordinates of nodes on the deck cargo hold boundary.
        /// </summary>
        private static List<int> GetBoundaryNodeXCoords()
        {
            List<int> nodes = new List<int>() { 35, 36 };
            nodes.AddRange(
                !ExtendedSettingsModScript.ModSettings.addAdditionalCrewDeckBuilding ?
                new[] { 23, 24 } : new[] { 32, 33 }
            );
            return nodes;
        }

        /// <summary>
        /// Connects any nodes on the deck cargo hold boundary via line of sight data.
        /// </summary>
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
                        Vector3 vector;
                        if (_UDLRFBIndex == 2)
                        {
                            vector = _originalNode;
                        }
                        else
                        {
                            vector = _moddedNode;
                        }
                        if (_room.roomDoorData.Count > 0 && RoomAppendageData.CheckAppendageList<DoorData>(vector, Orientation.Vertical))
                        {
                            PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                        }
                        if (_room.roomFloorJointData.Count > 0 && RoomAppendageData.CheckAppendageList<JoiningFloorData>(vector, Orientation.Vertical))
                        {
                            _activeNode.connectedNodesUDLRFB[_UDLRFBIndex] = true;
                            nodeData.connectedNodesUDLRFB[oppositeDirection] = true;
                        }
                        if (_room.roomWindowData.Count > 0 && RoomAppendageData.CheckAppendageList<WindowData>(vector, Orientation.Vertical))
                        {
                            PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                        }
                        if (_room.LOSJointData.Count > 0)
                        {
                            // Allow any nodes on the deck cargo hold boundary to connect via line of sight data.
                            List<int> boundaryNodeXCoords = GetBoundaryNodeXCoords();
                            if (boundaryNodeXCoords.Contains((int)vector.x) && vector.y == 5 && (_room.name.Contains("CargoContainer") || _room.name.Contains("Deck_CargoWalkway_TJPlatform"))
                            || RoomAppendageData.CheckAppendageList<LineOfSightData>(vector, Orientation.Vertical))
                            {
                                PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                            }
                        }
                    }
                    else
                    {
                        Vector3 vector2 = Vector3.zero;
                        if (_UDLRFBIndex == 5)
                        {
                            vector2 = _originalNode;
                        }
                        else
                        {
                            vector2 = _moddedNode;
                        }
                        if (_room.roomDoorData.Count > 0 && RoomAppendageData.CheckAppendageList<DoorData>(vector2, Orientation.Horizontal))
                        {
                            PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                        }
                        if (_room.roomFloorJointData.Count > 0 && RoomAppendageData.CheckAppendageList<JoiningFloorData>(vector2, Orientation.Horizontal))
                        {
                            PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                        }
                        if (_room.roomWindowData.Count > 0 && RoomAppendageData.CheckAppendageList<WindowData>(vector2, Orientation.Horizontal))
                        {
                            PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                        }
                        if (_room.LOSJointData.Count > 0 && RoomAppendageData.CheckAppendageList<LineOfSightData>(vector2, Orientation.Horizontal))
                        {
                            PositionRoomLG.SetNodeConnections(_activeNode, nodeData, _UDLRFBIndex, oppositeDirection);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Do not spawn rails at the edges of containers above deck 4 so that the connection to the exterior is clean.
        /// </summary>
        private static void HookSpawnCargoContainersLGSpawnBrokenRails(On.SpawnCargoContainersLG.orig_SpawnBrokenRails orig, Vector3 _regionNode, List<Room> _roomsInUse, GameObject _cargoParent, GameObject _railing)
        {
            if (_regionNode.y < 5)
            {
                orig.Invoke(_regionNode, _roomsInUse, _cargoParent, _railing);
            }
        }

        /// <summary>
        /// Allows cargo containers to spawn higher up than before.
        /// </summary>
        private static void HookSpawnCargoContainersLGSpawnCrateBlockPaths(On.SpawnCargoContainersLG.orig_SpawnCrateBlockPaths orig, float _leftSideX, float _rightSideX, List<int> _nodeFrontIndices, List<int> _nodeBackIndices, int _catWalkID, bool _altPathLeft)
        {
            List<Vector3> leftPoints = new List<Vector3>();
            List<Vector3> rightPoints = new List<Vector3>();
            int numberOfPaths = 2;
            int numberOfAltPaths = 1;
            int cratesToSpawn = 4;
            int maxY = 4;
            if (_rightSideX == 44f)
            {
                maxY = 3;
            }
            int maxLeftY = maxY;
            int maxRightY = maxY;

            // Raise the max height of the cargo hold on the left side of the ship.
            if (_leftSideX == 24f)
            {
                // Do not raise the left side if an extra crew deck is being spawned.
                if (!ExtendedSettingsModScript.ModSettings.addAdditionalCrewDeckBuilding)
                {
                    // Add standard crates to fill in the walls on deck 5 (with support below).
                    AddStandardCrates(new Vector3Int(24, 1, 2), new Vector3Int(24, 5, 13));
                    maxLeftY = 5;
                }
                AddStandardCrates(new Vector3Int(33, 1, 2), new Vector3Int(33, 5, 13));
                maxRightY = 5;
                numberOfPaths += 2;
            }
            for (int z = 0; z < 4; z++)
            {
                for (int y = 1; y <= maxLeftY; y++)
                {
                    leftPoints.Add(new Vector3(_leftSideX, y, z * 3 + 2));
                }
                for (int y = 1; y <= maxRightY; y++)
                {
                    rightPoints.Add(new Vector3(_rightSideX, y, z * 3 + 2));
                }
            }
            for (int pathsSpawned = 0; pathsSpawned < numberOfPaths; pathsSpawned++)
            {
                // Guarantee that there is at least one path at max height when using exterior cargo holds.
                var leftList = pathsSpawned == 0 && maxLeftY == 5 ? leftPoints.FindAll(point => point.y == maxLeftY) : leftPoints;
                SpawnCargoContainersLG.pointOne = leftList.Random();
                leftPoints.Remove(SpawnCargoContainersLG.pointOne);
                var rightList = pathsSpawned == 1 && maxRightY == 5 ? rightPoints.FindAll(point => point.y == maxRightY) : rightPoints;
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

        /// <summary>
        /// Do not spawn walls above deck 4 so that exterior access to the cargo holds is not blocked.
        /// </summary>
        private static GameObject HookSpawnCargoHoldLGHorizontalOrVerticalWall(On.SpawnCargoHoldLG.orig_HorizontalOrVerticalWall orig, int _horIndex, int _vertIndex, Vector3 _regionNode, int _cargoID, RoomStructure[] _nodeChecks)
        {
            if (_regionNode.y < 5)
            {
                return orig.Invoke(_horIndex, _vertIndex, _regionNode, _cargoID, _nodeChecks);
            }
            return null;
        }

        /// <summary>
        /// Do not spawn deck cargo walls that would cover up the cargo containers.
        /// </summary>
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
                var hasExtendedHoldBehindWall = _node.x < 37f && (RegionManager.Instance.CheckNodeForRegion(_node + Vector3.right, (int)RegionId.CargoMainHold) || RegionManager.Instance.CheckNodeForRegion(_node + Vector3.left, (int)RegionId.CargoMainHold));
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
    }
}