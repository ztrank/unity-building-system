namespace ZTrank.BuildingSystem
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    [RequireComponent(typeof(Collider))]
    public class Preview : MonoBehaviour
    {
        private List<Collider> m_Collisions;

        [SerializeField]
        private MeshRenderer m_MeshRenderer;

        [SerializeField]
        private BuildingType m_BuildingType;

        [SerializeField]
        private bool m_OverrideOrientationCoefficient = false;

        [SerializeField]
        private float m_OverrideOrientationCoefficientAmount;

        protected virtual float OrientationCoefficient => 90f;

        public Vector3 Orientation { get; set; }

        private BuildingSystemSettings m_BuildingSystemSettings;

        private void Awake()
        {
            this.m_Collisions = new List<Collider>();
        }

        public bool IsPlaceable => (!this.Blueprint.RequireFoundation || this.SnapPoint != null) && !this.m_Collisions.Any();

        public string GetReasons()
        {
            if (this.Blueprint.RequireFoundation && this.SnapPoint == null)
            {
                return "Must be built on a snap point.";
            }

            if (this.m_Collisions.Any())
            {
                return $"Building Obstructed: {this.m_Collisions[0].gameObject.name}";
            }

            return string.Empty;
        }

        public SnapPoint SnapPoint { get; set; }

        public Blueprint Blueprint { get; set; }

        public void UpdatePositionAndRotation(Vector3 targetPosition, Vector3 targetRotation, BuildingFace targetFace)
        {

            this.transform.position = this.GetPosition(targetPosition, targetRotation, targetFace);

            if (this.SnapPoint != null && targetFace != BuildingFace.None) 
            {
                this.transform.rotation = this.GetSnapPointRotation() * Quaternion.Euler(this.Orientation * (this.m_OverrideOrientationCoefficient ? this.m_OverrideOrientationCoefficientAmount : this.OrientationCoefficient));
            }
            else
            {
                this.transform.Rotate(targetRotation);
            }
        }

        public void SetSettings(BuildingSystemSettings settings)
        {
            this.m_BuildingSystemSettings = settings;
        }

        private void Start()
        {
            Collider collider = this.GetComponent<Collider>();
        }

        private void Update()
        {
            
        }

        protected virtual Quaternion GetSnapPointRotation()
        {
            return this.Blueprint.UseBuildingRotation ? this.SnapPoint.Building.transform.rotation : this.SnapPoint.transform.rotation;
        }

        protected virtual Vector3 GetPosition(Vector3 targetPosition, Vector3 targetRotation, BuildingFace targetFace)
        {
            Vector3 newPosition = targetPosition;
            if (this.SnapPoint != null)
            {
                Vector3 offsetDirection = Vector3.zero;
                float offsetAmount = 0;

                if (this.SnapPoint.SlotDirection != BuildingFace.None)
                {
                    switch (targetFace)
                    {
                        case BuildingFace.Up:
                            offsetDirection = this.SnapPoint.transform.up;
                            offsetAmount = this.Blueprint.Size.y / 2;
                            break;
                        case BuildingFace.Down:
                            offsetDirection = this.SnapPoint.transform.up;
                            offsetAmount = -this.Blueprint.Size.y / 2;
                            break;
                        case BuildingFace.East:
                        case BuildingFace.West:
                        case BuildingFace.North:
                        case BuildingFace.South:
                            offsetDirection = this.SnapPoint.transform.forward;
                            offsetAmount = this.Blueprint.Size.z / 2;
                            break;
                    }
                }
                else
                {
                    offsetAmount = this.Blueprint.Size.z / 2;
                    offsetDirection = this.SnapPoint.transform.forward;
                }

                newPosition += offsetDirection * offsetAmount;
            }
            
            return newPosition;
        }

        private void LateUpdate()
        {
            this.UpdateMaterial();
        }

        private void UpdateMaterial()
        {
            if (this.IsPlaceable && this.m_MeshRenderer.material != this.m_BuildingSystemSettings.m_PreviewPlaceable)
            {
                this.m_MeshRenderer.material = this.m_BuildingSystemSettings.m_PreviewPlaceable;
            }
            else if (!this.IsPlaceable && this.m_MeshRenderer.material != this.m_BuildingSystemSettings.m_PreviewUnplaceable)
            {
                this.m_MeshRenderer.material = this.m_BuildingSystemSettings.m_PreviewUnplaceable;
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (((1 << collision.gameObject.layer) & this.m_BuildingSystemSettings.m_BuildingBlockedLayers) != 0)
            {
                this.m_Collisions.Add(collision.collider);
                if (this.m_MeshRenderer.material != this.m_BuildingSystemSettings.m_PreviewUnplaceable)
                {
                    this.m_MeshRenderer.material = this.m_BuildingSystemSettings.m_PreviewUnplaceable;
                }
            }
        }

        private void OnCollisionExit(Collision collision)
        {
            if (((1 << collision.gameObject.layer) & this.m_BuildingSystemSettings.m_BuildingBlockedLayers) != 0)
            {
                this.m_Collisions.Remove(collision.collider);
            }
        }
    }
}
