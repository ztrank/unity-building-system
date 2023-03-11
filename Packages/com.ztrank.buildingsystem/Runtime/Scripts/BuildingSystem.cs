namespace ZTrank.BuildingSystem
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
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
        enum BuildingSystemEventType
        {
            Add,
            Remove
        }

        class BuildingSystemEvent
        {
            public BuildingSystemEventType Type { get; set; }
            public Building Building { get; set; }
            public Blueprint Blueprint { get; set; }
            public Vector3 Position { get; set; }
            public SnapPoint SnapPoint { get; set; }
            public Quaternion Rotation { get; set; }
            public Transform Parent { get; set; }
        }

        [SerializeField]
        private BuildingSystemSettings m_BuildingSystemSettings;

        public event Action<Building> OnBuildSuccess;
        public event Action<Building> OnPreBuildingRemove;
        private List<BuildingSystemEvent> m_EventList;

        public BuildingSystemSettings Settings => this.m_BuildingSystemSettings;

        private Blueprint m_CurrentBlueprint;
        private Preview m_CurrentPreview;
        private Vector3 m_CurrentPosition;
        private Vector3 m_CurrentRotation;
        private bool m_IsPaused = false;
        private BuildingFace m_CurrentFace;

        private void Awake()
        {
            this.m_EventList = new List<BuildingSystemEvent>();
        }

        public bool IsPaused => this.m_IsPaused;

        public void Rotate(float amount)
        {
            this.m_CurrentRotation = this.m_BuildingSystemSettings.m_RotationSpeed * amount * Vector3.up;
        }

        public void SnapRotate(Vector3 orientation)
        {
            if (this.m_CurrentPreview != null)
            {
                this.m_CurrentPreview.Orientation += orientation;
            }
        }

        public bool IsSnapped => this.m_CurrentPreview != null && this.m_CurrentPreview.SnapPoint != null;

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

        private List<BuildingSystemEvent> m_RedoEvents = new List<BuildingSystemEvent>();


        private Building CreateBuilding(Building prefab, Transform parent, Vector3 position, Quaternion rotation, Blueprint blueprint, SnapPoint snapPoint)
        {
            Building obj = Instantiate(prefab, parent);

            obj.transform.SetPositionAndRotation(position, rotation);
            obj.gameObject.name = obj.gameObject.name.Replace("(Clone)", "");
            obj.Blueprint = blueprint;

            if (snapPoint != null)
            {
                snapPoint.Occupy(obj);
                obj.SnapPoint = snapPoint;
            }
            this.OnBuildSuccess?.Invoke(obj);
            return obj;
        }

        private void DestroyBuilding(Building building)
        {
            this.OnPreBuildingRemove?.Invoke(building);
            Destroy(building.gameObject);
        }

        public void Redo()
        {
            if (this.m_RedoEvents.Any())
            {
                BuildingSystemEvent evt = this.m_RedoEvents.Last();
                if (evt.Type == BuildingSystemEventType.Add)
                {
                    Building obj = this.CreateBuilding(evt.Blueprint.Prefab, evt.Parent, evt.Position, evt.Rotation, evt.Blueprint, evt.SnapPoint);
                    this.m_EventList.Add(new BuildingSystemEvent()
                    {
                        Type = BuildingSystemEventType.Add,
                        Blueprint = evt.Blueprint,
                        SnapPoint = evt.SnapPoint,
                        Building = obj,
                        Position = evt.Position,
                        Rotation = evt.Rotation,
                        Parent = evt.Parent
                    });
                }
                else if (evt.Type == BuildingSystemEventType.Remove)
                {
                    this.m_EventList.Add(new BuildingSystemEvent()
                    {
                        Type = BuildingSystemEventType.Remove,
                        Blueprint = evt.Blueprint,
                        SnapPoint = evt.SnapPoint,
                        Position = evt.Position,
                        Rotation = evt.Rotation,
                        Parent = evt.Parent
                    });
                    this.DestroyBuilding(evt.Building);
                }

                this.m_RedoEvents.Remove(evt);
            }
        }

        public void Undo()
        {
            if (this.m_EventList.Any())
            {
                BuildingSystemEvent evt = this.m_EventList.Last();
                
                if (evt.Type == BuildingSystemEventType.Add)
                {
                    this.m_RedoEvents.Add(new BuildingSystemEvent()
                    {
                        Type = BuildingSystemEventType.Add,
                        Blueprint = evt.Blueprint,
                        SnapPoint = evt.SnapPoint,
                        Position = evt.Position,
                        Rotation = evt.Rotation,
                        Parent = evt.Parent
                    });
                    this.DestroyBuilding(evt.Building);
                    
                }
                else if (evt.Type == BuildingSystemEventType.Remove)
                {
                    Building obj = this.CreateBuilding(evt.Blueprint.Prefab, evt.Parent, evt.Position, evt.Rotation, evt.Blueprint, evt.SnapPoint);
                    this.m_RedoEvents.Add(new BuildingSystemEvent()
                    {
                        Type = BuildingSystemEventType.Remove,
                        Blueprint = evt.Blueprint,
                        SnapPoint = evt.SnapPoint,
                        Building = obj,
                        Position = evt.Position,
                        Rotation = evt.Rotation,
                        Parent = evt.Parent
                    });
                }

                this.m_EventList.Remove(evt);
            }
        }

        private void AddEvent(BuildingSystemEvent evt)
        {
            // Clear redo list
            this.m_RedoEvents.Clear();
            this.m_EventList.Add(evt);
        }

        public bool TryRemove(Func<Building, bool> canRemove = null)
        {
            if (!this.m_IsPaused && Physics.Raycast(this.m_ScreenRay, out RaycastHit hitInfo, Mathf.Infinity, this.m_BuildingSystemSettings.m_BuildingLayer))
            {
                Building building = hitInfo.collider.gameObject.GetComponentInParent<Building>();
                if (building != null)
                {
                    if (canRemove == null || canRemove(building))
                    {
                        this.AddEvent(new BuildingSystemEvent()
                        {
                            Type = BuildingSystemEventType.Remove,
                            Blueprint = building.Blueprint,
                            SnapPoint = building.SnapPoint,
                            Position = building.transform.position,
                            Rotation = building.transform.rotation,
                            Parent = building.transform.parent
                        });
                        this.DestroyBuilding(building);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool TryBuild(out Building obj, Transform parent = null)
        {
            if (!this.m_IsPaused && this.m_CurrentBlueprint != null && this.m_CurrentPreview != null && this.m_CurrentPreview.IsPlaceable && Physics.Raycast(this.m_ScreenRay, Mathf.Infinity, this.m_BuildingSystemSettings.m_BuildableLayer))
            {
                SnapPoint sp = this.m_CurrentPreview.SnapPoint;

                if (sp != null && sp.IsOccupied)
                {
                    obj = null;
                    return false;
                }

                obj = this.CreateBuilding(this.m_CurrentBlueprint.Prefab, parent ? parent : this.transform, this.m_CurrentPreview.transform.position, this.m_CurrentPreview.transform.rotation, this.m_CurrentBlueprint, sp);
                this.AddEvent(new BuildingSystemEvent()
                {
                    Type = BuildingSystemEventType.Add,
                    Blueprint = this.m_CurrentBlueprint,
                    SnapPoint = sp,
                    Building = obj,
                    Position = this.m_CurrentPreview.transform.position,
                    Rotation = this.m_CurrentPreview.transform.rotation,
                    Parent = parent ? parent : this.transform
                });
                return true;
            }
            

            obj = null;
            return false;
        }

        public string GetInvalidReasons()
        {
            if (this.m_CurrentPreview != null)
            {
                return this.m_CurrentPreview.GetReasons();
            }

            return string.Empty;
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
                this.m_CurrentPreview.Blueprint = this.m_CurrentBlueprint;
            }
        }

        private Ray m_ScreenRay;
        public void SetScreenRay(Ray screenRay)
        {
            this.m_ScreenRay = screenRay;
        }


        private void UpdatePreviewPositionAndRotation()
        {
            if (!this.m_IsPaused && this.m_CurrentPreview != null && this.m_CurrentBlueprint != null)
            {
                this.m_CurrentPreview.UpdatePositionAndRotation(this.m_CurrentPosition, this.m_CurrentRotation, this.m_CurrentFace);
            }
        }
    }
}
