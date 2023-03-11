using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZTrank.BuildingSystem
{
    [CreateAssetMenu(fileName = "Blueprint", menuName = "Building System/Blueprint", order = 2)]
    public class Blueprint : ScriptableObject
    {
        public GameObject Prefab;
        public Preview Preview;
        public Vector3 Offset;
        public BuildingType BuildingType;
    }
}
