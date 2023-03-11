using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZTrank.BuildingSystem
{
    [CreateAssetMenu(fileName = "BuildingSystemSettings", menuName = "Building System/Settings", order = 1)]
    public class BuildingSystemSettings : ScriptableObject
    {
        public Material m_PreviewPlaceable;
        public Material m_PreviewUnplaceable;
        public LayerMask m_BuildableLayer;
        public LayerMask m_BuildingLayer;
        public LayerMask m_BuildingPreviewLayer;
        public LayerMask m_BuildingBlockedLayers;
        public LayerMask m_SnapPointLayer;
    }
}
