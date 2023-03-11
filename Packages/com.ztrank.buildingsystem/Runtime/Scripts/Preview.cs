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

        private BuildingSystemSettings m_BuildingSystemSettings;

        private void Awake()
        {
            this.m_Collisions = new List<Collider>();
        }

        public bool IsPlaceable => !this.m_Collisions.Any() && this.m_BuildingType == BuildingType.Foundation || this.SnapPoint != null;

        public SnapPoint SnapPoint { get; set; }

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
            if (this.SnapPoint != null)
            {
                this.transform.rotation = this.SnapPoint.Rotation;
            }
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
