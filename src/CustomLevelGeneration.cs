using System.Collections.Generic;
using System.Linq;
using MonstrumExtendedSettingsMod.Enum;
using UnityEngine;

namespace MonstrumExtendedSettingsMod
{
    class CustomLevelGeneration
    {
        public static void SetNodes(Vector3Int start, Vector3Int end, params RegionId[] newRegionIds)
        {
            for (int x = Mathf.Min(start.x, end.x); x <= Mathf.Max(start.x, end.x); x++)
            {
                for (int y = Mathf.Min(start.y, end.y); y <= Mathf.Max(start.y, end.y); y++)
                {
                    for (int z = Mathf.Min(start.z, end.z); z <= Mathf.Max(start.z, end.z); z++)
                    {
                        SetNode(new Vector3Int(x, y, z), newRegionIds);
                    }
                }
            }
        }

        public static void SetNode(Vector3Int coords, params RegionId[] newRegionIds)
        {
            // Check whether the reference / copy node is within ship bounds.
            if (!CheckBoundariesLG.NodeWithinShipBounds(coords))
            {
                Debug.Log($"Node {coords} is not in ship bounds!");
                return;
            }

            // Convert the region ID params into a set that can be manipulated.
            // This means that we do not have to process any region entries the node is already a part of.
            var newRegionIdsSet = new HashSet<RegionId>(newRegionIds);

            // Clean up the node data and any region entries that the node should no longer be a part of.
            RegionNodeDataZ nodeData = RegionManager.Instance.regionData[coords.x].regionDataY[coords.y].regionDataZ[coords.z];
            for (int i = nodeData.regionID.Count - 1; i >= 0; i--)
            {
                if (System.Enum.IsDefined(typeof(RegionId), nodeData.regionID[i]))
                {
                    RegionId oldRegionId = (RegionId)nodeData.regionID[i];
                    // If the old ID is already in the new region IDs list, remove the ID from the new region IDs list.
                    // This means that the ID does not have to be processed later.
                    if (newRegionIdsSet.Contains(oldRegionId))
                    {
                        newRegionIdsSet.Remove(oldRegionId);
                    }
                    // Else, remove the ID from the region data and clean up the associated region entry.
                    else
                    {
                        nodeData.regionID.RemoveAt(i);
                        int regionIndex = RegionManager.Instance.IDToIndex((int)oldRegionId);
                        RegionManager.Instance.regions[regionIndex].associatedNodes.Remove(coords);
                    }
                }
                else
                {
                    Debug.LogError($"Node had RegionId value that could not be converted to enum type. ID: {nodeData.regionID[i]}. Name: {RegionManager.Instance.IDToName(nodeData.regionID[i])}");
                }
            }

            // Now process any remaining IDs that the node should be a part of.
            foreach (RegionId newRegionId in newRegionIdsSet)
            {
                nodeData.regionID.Add((int)newRegionId);
                int regionIndex = RegionManager.Instance.IDToIndex((int)newRegionId);
                RegionManager.Instance.regions[regionIndex].associatedNodes.Add(coords);
            }
        }
    }
}