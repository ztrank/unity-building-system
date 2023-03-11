namespace ZTrank.BuildingSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public enum BuildingFace
    {
        None,
        Up,
        Down,
        East,
        West,
        North, 
        South
    }

    public class BuildingSystem : MonoBehaviour
    {
        [SerializeField]
        private BuildingSystemSettings m_BuildingSystemSettings;

        public event Action<GameObject> OnBuildSuccess;
        public event Action<GameObject> OnPreBuildingRemove;

        public BuildingSystemSettings Settings => this.m_BuildingSystemSettings;

        private Blueprint m_CurrentBlueprint;
        private Preview m_CurrentPreview;
        private Vector3 m_CurrentPosition;
        private Vector3 m_CurrentRotation;
        private bool m_IsPaused = false;
        private BuildingFace m_CurrentFace;

        public bool IsPaused => this.m_IsPaused;

        private void Update()
        {
            this.UpdatePreviewPositionAndRotation();
        }

        private void FixedUpdate()
        {
            if (this.m_CurrentPreview != null && Physics.Raycast(this.m_ScreenRay, out RaycastHit hitInfo, Mathf.Infinity, this.m_BuildingSystemSettings.m_BuildableLayer))
            {

                Building building = hitInfo.collider.transform.gameObject.GetComponentInParent<Building>();


                if (building != null && building.TryGetNearestSnapPoint(hitInfo, this.m_CurrentBlueprint.BuildingType, out SnapPoint snapPoint))
                {
                    if (snapPoint != this.m_CurrentPreview.SnapPoint)
                    {
                        this.m_CurrentFace = snapPoint.BuildingFace;
                        this.m_CurrentPosition = snapPoint.transform.position;
                        this.m_CurrentPreview.SnapPoint = snapPoint;
                    }
                }
                else
                {
                    this.m_CurrentFace = BuildingFace.None;
                    this.m_CurrentPosition = hitInfo.point;
                    this.m_CurrentPreview.SnapPoint = null;
                }
            }
        }

        public static BuildingFace GetHitFace(RaycastHit hit)
        {
            Vector3Int incomingVector = Vector3Int.RoundToInt(hit.normal - Vector3.up);

            if (incomingVector == new Vector3Int(0, -1, -1))
            {
                return BuildingFace.South;
            }

            if (incomingVector == new Vector3Int(0, -1, 1))
            {
                return BuildingFace.North;
            }

            if (incomingVector == new Vector3Int(0, 0, 0))
            {
                return BuildingFace.Up;
            }

            if (incomingVector == new Vector3Int(1, 1, 1))
            {
                return BuildingFace.Down;
            }

            if (incomingVector == new Vector3Int(-1, -1, 0))
            {
                return BuildingFace.West;
            }

            if (incomingVector == new Vector3Int(1, -1, 0))
            {
                return BuildingFace.East;
            }

            return BuildingFace.None;
        }

        public void Pause()
        {
            this.m_IsPaused = true;
            if (this.m_CurrentPreview != null)
            {
                this.m_CurrentPreview.gameObject.SetActive(false);
            }
        }

        public void Resume()
        {
            this.m_IsPaused = false;

            if (this.m_CurrentPreview != null)
            {
                this.m_CurrentPreview.gameObject.SetActive(true);
            }
        }

        public bool TryRemove(Func<GameObject, bool> canRemove = null)
        {
            if (!this.m_IsPaused && Physics.Raycast(this.m_ScreenRay, out RaycastHit hitInfo, Mathf.Infinity, this.m_BuildingSystemSettings.m_BuildingLayer))
            {
                
                if (canRemove == null || canRemove(hitInfo.collider.transform.parent.gameObject))
                {
                    this.OnPreBuildingRemove?.Invoke(hitInfo.collider.transform.parent.gameObject);
                    Destroy(hitInfo.collider.transform.parent.gameObject);
                    return true;
                }

                return false;
            }

            return false;
        }

        public bool TryBuild(out GameObject obj, Transform parent = null)
        {
            if (!this.m_IsPaused && this.m_CurrentBlueprint != null && this.m_CurrentPreview != null && this.m_CurrentPreview.IsPlaceable && Physics.Raycast(this.m_ScreenRay, Mathf.Infinity, this.m_BuildingSystemSettings.m_BuildableLayer))
            {
                obj = Instantiate(this.m_CurrentBlueprint.Prefab, parent ? parent : this.transform);
                Quaternion rotation = this.m_CurrentPreview.SnapPoint == null ? this.m_CurrentPreview.transform.rotation : this.m_CurrentPreview.SnapPoint.Rotation;
                obj.transform.SetPositionAndRotation(this.m_CurrentPreview.transform.position, rotation);
                this.OnBuildSuccess?.Invoke(obj);
                return true;
            }
            

            obj = null;
            return false;
        }

        

        public void SetBlueprint(Blueprint blueprint)
        {
            if (this.m_CurrentPreview != null)
            {
                Destroy(this.m_CurrentPreview.gameObject);
            }

            this.m_CurrentBlueprint = this.m_CurrentBlueprint == blueprint ? null : blueprint;
            
            if (this.m_CurrentBlueprint != null)
            {
                this.m_CurrentPreview = Instantiate(this.m_CurrentBlueprint.Preview, this.transform);
                this.m_CurrentPreview.transform.position = this.m_CurrentPosition;
                this.m_CurrentPreview.gameObject.SetActive(!this.m_IsPaused);
                this.m_CurrentPreview.SetSettings(this.m_BuildingSystemSettings);
            }
        }

        private Ray m_ScreenRay;
        public void SetScreenRay(Ray screenRay)
        {
            this.m_ScreenRay = screenRay;
        }

        public void MovePreview(Vector3 position)
        {
            this.m_CurrentPosition = position;
        }

        public void RotatePreview(Vector3 angles)
        {
            this.m_CurrentRotation = angles;
        }

        private void UpdatePreviewPositionAndRotation()
        {
            if (!this.m_IsPaused && this.m_CurrentPreview != null && this.m_CurrentBlueprint != null)
            {
                Vector3 newPosition = this.m_CurrentPosition;
                Vector3 faceDirection = Vector3.zero;
                Debug.Log($"Current Face {this.m_CurrentFace}");
                switch(this.m_CurrentFace)
                {
                    case BuildingFace.Up:
                        faceDirection = new Vector3(0, this.m_CurrentBlueprint.Offset.y, 0);
                        break;
                    case BuildingFace.Down:
                        faceDirection = new Vector3(0, -this.m_CurrentBlueprint.Offset.y, 0);
                        break;
                    case BuildingFace.East:
                        faceDirection = new Vector3(this.m_CurrentBlueprint.Offset.x, 0, 0);
                        break;
                    case BuildingFace.West:
                        faceDirection = new Vector3(-this.m_CurrentBlueprint.Offset.x, 0, 0);
                        break;
                    case BuildingFace.North:
                        faceDirection = new Vector3(0, 0, this.m_CurrentBlueprint.Offset.z);
                        break;
                    case BuildingFace.South:
                        faceDirection = new Vector3(0, 0, -this.m_CurrentBlueprint.Offset.z);
                        break;
                }

                newPosition += faceDirection;

                if (this.m_CurrentFace == BuildingFace.None)
                {
                    this.m_CurrentPreview.transform.Rotate(this.m_CurrentRotation);
                }

                this.m_CurrentPreview.transform.position = newPosition + (this.m_CurrentPreview.SnapPoint == null ? Vector3.zero : this.Multiply(this.GetSlotOffsetVector(this.m_CurrentPreview.SnapPoint.SlotDirection), this.m_CurrentBlueprint.Offset));
            }
        }

        private Vector3 GetSlotOffsetVector(BuildingFace slotDirection)
        {
            return slotDirection switch
            {
                BuildingFace.North => new Vector3(0, 0, -1),
                BuildingFace.South => new Vector3(0, 0, 1),
                BuildingFace.East => new Vector3(-1, 0, 0),
                BuildingFace.West => new Vector3(1, 0, 0),
                BuildingFace.Up => new Vector3(0, 1, 0),
                BuildingFace.Down => new Vector3(0, -1, 0),
                _ => Vector3.zero,
            };
        }

        private Vector3 Multiply(Vector3 a, Vector3 b)
        {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }
    }
}