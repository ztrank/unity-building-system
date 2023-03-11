using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

namespace ZTrank.BuildingSystem
{
    [RequireComponent(typeof(Collider))]
    public class SnapPoint : MonoBehaviour
    {
        [SerializeField]
        private BuildingType[] m_BuildingTypes;

        [SerializeField]
        private BuildingFace m_BuildingFace;

        [SerializeField]
        private BuildingFace m_SlotDirection;

        [SerializeField]
        private bool m_UseBuildingRotation = false;

        private Collider m_Collider;

        public BuildingFace BuildingFace => this.m_BuildingFace;

        public BuildingType[] BuildingTypes => this.m_BuildingTypes;

        public BuildingFace SlotDirection => this.m_SlotDirection;

        public Collider Collider => this.m_Collider;

        public Quaternion Rotation => this.m_UseBuildingRotation ? this.GetComponentInParent<Building>().transform.rotation : this.transform.rotation;

        private void Awake()
        {
            this.m_Collider = this.GetComponent<Collider>();
        }
    }
}
