using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ZTrank.BuildingSystem
{
    public class Building : MonoBehaviour
    {
        private Dictionary<BuildingFace, List<SnapPoint>> m_SnapPoints;

        private void Awake()
        {
            this.m_SnapPoints = new Dictionary<BuildingFace, List<SnapPoint>>();
        }

        private void Start()
        {
            foreach (SnapPoint snapPoint in this.GetComponentsInChildren<SnapPoint>())
            {
                if (!this.m_SnapPoints.ContainsKey(snapPoint.BuildingFace))
                {
                    this.m_SnapPoints.Add(snapPoint.BuildingFace, new List<SnapPoint>());
                }

                this.m_SnapPoints[snapPoint.BuildingFace].Add(snapPoint);
            }
        }

        public bool TryGetNearestSnapPoint(RaycastHit hitInfo, BuildingType type, out SnapPoint snapPoint)
        {
            BuildingFace face = BuildingSystem.GetHitFace(hitInfo);
            snapPoint = null;

            if (this.m_SnapPoints.TryGetValue(BuildingSystem.GetHitFace(hitInfo), out List<SnapPoint> snapPoints))
            {
                SnapPoint nearestSnapPoint = null;
                float distance = 0;
                foreach(SnapPoint testPoint in snapPoints.Where(sp => sp.BuildingTypes.Contains(type)))
                {
                    float testDistance = Vector3.Distance(hitInfo.point, testPoint.Collider.ClosestPoint(hitInfo.point));
                    if (nearestSnapPoint == null)
                    {
                        nearestSnapPoint = testPoint;
                        distance = testDistance;
                        continue;
                    }

                    if (testDistance < distance)
                    {
                        nearestSnapPoint = testPoint;
                        distance = testDistance;
                    }
                }

                snapPoint = nearestSnapPoint;
                return snapPoint != null;
            }

            return false;
        }
    }
}
