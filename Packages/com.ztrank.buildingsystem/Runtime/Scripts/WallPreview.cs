using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ZTrank.BuildingSystem
{
    public class WallPreview : Preview
    {
        protected override float OrientationCoefficient => 180f;
        protected override Quaternion GetSnapPointRotation()
        {
            if (this.SnapPoint.Building.BuildingType == BuildingType.Wall && (this.SnapPoint.BuildingFace == BuildingFace.East || this.SnapPoint.BuildingFace == BuildingFace.West || this.SnapPoint.BuildingFace == BuildingFace.Up || this.SnapPoint.BuildingFace == BuildingFace.Down))
            {
                return this.SnapPoint.Building.transform.rotation;
            }

            return base.GetSnapPointRotation();
        }

        protected override Vector3 GetPosition(Vector3 targetPosition, Vector3 targetRotation, BuildingFace targetFace)
        {
            Vector3 basePosition = base.GetPosition(targetPosition, targetRotation, targetFace);

            if (this.SnapPoint != null && this.SnapPoint.Building.BuildingType == BuildingType.Wall)
            {
                Vector3 offsetDirection = this.SnapPoint.transform.forward;
                float offsetAmount = 0;
                switch (this.SnapPoint.SlotDirection)
                {
                    case BuildingFace.Up:
                        offsetAmount = this.Blueprint.Size.y / 2;
                        break;
                    case BuildingFace.Down:
                        offsetAmount = -this.Blueprint.Size.y / 2;
                        break;
                    case BuildingFace.East:
                    case BuildingFace.West:
                    case BuildingFace.North:
                    case BuildingFace.South:
                        offsetAmount = this.Blueprint.Size.x / 2;
                        break;
                }

                basePosition = targetPosition + offsetDirection * offsetAmount;
            }

            return basePosition;
        }
    }
}
