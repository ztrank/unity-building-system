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

        private Collider m_Collider;
        private Building m_Building;
        private Building m_Occupied;

        public BuildingFace BuildingFace => this.m_BuildingFace;

        public BuildingType[] BuildingTypes => this.m_BuildingTypes;

        public BuildingFace SlotDirection => this.m_SlotDirection;

        public Collider Collider => this.m_Collider;

        public Building Building => this.m_Building;

        public bool IsOccupied => this.m_Occupied != null;

        private void Awake()
        {
            this.m_Collider = this.GetComponent<Collider>();
            this.m_Building = this.GetComponentInParent<Building>();
            Debug.Assert(this.m_Building != null);
        }

        public void Occupy(Building building)
        {
            this.m_Occupied = building;
        }
    }
}
