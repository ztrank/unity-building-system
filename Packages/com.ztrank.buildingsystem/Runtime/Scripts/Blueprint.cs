using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZTrank.BuildingSystem
{
    [CreateAssetMenu(fileName = "Blueprint", menuName = "Building System/Blueprint", order = 2)]
    public class Blueprint : ScriptableObject
    {
        public Building Prefab;
        public Preview Preview;
        public Vector3 Size;
        public BuildingType BuildingType;
        public bool UseBuildingRotation;
        public bool RequireFoundation;
    }
}
