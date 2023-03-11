using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZTrank.BuildingSystem
{
    public class Building : MonoBehaviour
    {
        [SerializeField]
        private BuildingType m_BuildingType;

        private Dictionary<BuildingFace, List<SnapPoint>> m_SnapPoints;

        private List<SnapPoint> m_AllSnapPoints;

        public BuildingType BuildingType => this.m_BuildingType;

        public SnapPoint SnapPoint { get; set; }

        private void OnDestroy()
        {
            if (this.SnapPoint != null)
            {
                this.SnapPoint.Occupy(null);
                this.SnapPoint = null;
            }
        }

        private void Awake()
        {
            this.m_SnapPoints = new Dictionary<BuildingFace, List<SnapPoint>>();
            this.m_AllSnapPoints = new List<SnapPoint>();
        }

        private void Start()
        {
            foreach (SnapPoint snapPoint in this.GetComponentsInChildren<SnapPoint>())
            {
                if (!this.m_SnapPoints.ContainsKey(snapPoint.BuildingFace))
                {
                    this.m_SnapPoints.Add(snapPoint.BuildingFace, new List<SnapPoint>());
                }
                this.m_AllSnapPoints.Add(snapPoint);
                this.m_SnapPoints[snapPoint.BuildingFace].Add(snapPoint);
            }
        }

        public bool TryGetNearestSnapPoint(RaycastHit hitInfo, BuildingType type, out SnapPoint snapPoint)
        {
            BuildingFace face = BuildingSystem.GetHitFace(hitInfo);
            snapPoint = null;

            SnapPoint nearestSnapPoint = null;
            float distance = 0;
            if (this.m_SnapPoints.TryGetValue(BuildingSystem.GetHitFace(hitInfo), out List<SnapPoint> snapPoints))
            {
                foreach(SnapPoint testPoint in snapPoints.Where(sp => !sp.IsOccupied && sp.BuildingTypes.Contains(type)))
                {
                    this.TestSnapPointDistance(testPoint, hitInfo, ref nearestSnapPoint, ref distance);
                }
            }

            if (nearestSnapPoint == null)
            {
                distance = 0;
                foreach (SnapPoint testPoint in this.m_AllSnapPoints.Where(sp => !sp.IsOccupied && sp.BuildingTypes.Contains(type)))
                {
                    this.TestSnapPointDistance(testPoint, hitInfo, ref nearestSnapPoint, ref distance);
                }
            }

            snapPoint = nearestSnapPoint;

            return snapPoint != null;
        }

        private void TestSnapPointDistance(SnapPoint testPoint, RaycastHit hitInfo, ref SnapPoint nearestSnapPoint, ref float distance)
        {
            float testDistance = Vector3.Distance(hitInfo.point, testPoint.Collider.ClosestPoint(hitInfo.point));
            if (nearestSnapPoint == null)
            {
                nearestSnapPoint = testPoint;
                distance = testDistance;
                return;
            }

            if (testDistance < distance)
            {
                nearestSnapPoint = testPoint;
                distance = testDistance;
            }
        }
    }
}
