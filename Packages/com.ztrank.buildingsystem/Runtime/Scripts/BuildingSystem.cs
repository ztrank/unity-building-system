//----------------------------------------------------------------------
// <copyright file="BuildingSystem.cs" company="Zachary Trank-Zelewicz">
//      Copyright (c) Zachary Trank-Zelewciz. All rights reserved.
// </copyright>
//----------------------------------------------------------------------

namespace ZTrank.BuildingSystem
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;

    /// <summary>
    /// Building Face Directions
    /// </summary>
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

    /// <summary>
    /// Building System
    /// </summary>
    public class BuildingSystem : MonoBehaviour
    {
        /// <summary>
        /// On Build Success Event
        /// </summary>
        public event Action<Building> OnBuildSuccess;

        /// <summary>
        /// On Pre Build Remove Event
        /// </summary>
        public event Action<Building> OnPreBuildingRemove;

        /// <summary>
        /// Build System Settings
        /// </summary>
        [SerializeField]
        private BuildingSystemSettings m_BuildingSystemSettings;

        /// <summary>
        /// Build System Event List
        /// </summary>
        private List<BuildingSystemEvent> m_EventList;

        /// <summary>
        /// Build System Redo Events List
        /// </summary>
        private List<BuildingSystemEvent> m_RedoEvents;

        /// <summary>
        /// Current Selected Blueprint
        /// </summary>
        private Blueprint m_CurrentBlueprint;

        /// <summary>
        /// Current Preview
        /// </summary>
        private Preview m_CurrentPreview;

        /// <summary>
        /// Current Position
        /// </summary>
        private Vector3 m_CurrentPosition;

        /// <summary>
        /// Current Rotation
        /// </summary>
        private Vector3 m_CurrentRotation;

        /// <summary>
        /// Is the build system paused
        /// </summary>
        private bool m_IsPaused = false;

        /// <summary>
        /// Current Face hit by screen ray
        /// </summary>
        private BuildingFace m_CurrentFace;

        /// <summary>
        /// Screen Ray
        /// </summary>
        private Ray m_ScreenRay;

        /// <summary>
        /// Gets a value indicating whether the build system is paused.
        /// </summary>
        public bool IsPaused => this.m_IsPaused;

        /// <summary>
        /// Gets the Build System Settings
        /// </summary>
        public BuildingSystemSettings Settings => this.m_BuildingSystemSettings;

        /// <summary>
        /// Gets a value indicating whether the current preview is active and attached to a snap point
        /// </summary>
        public bool IsSnapped => this.m_CurrentPreview != null && this.m_CurrentPreview.SnapPoint != null;

        /// <summary>
        /// Gets the Current Building Face from the raycast hit
        /// </summary>
        /// <param name="hit">Ray Cast Hit</param>
        /// <returns>Building Face</returns>
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

        /// <summary>
        /// Rotates by the amount and rotation speed around the Y Axis
        /// </summary>
        /// <param name="amount">Amount to rotate</param>
        public void Rotate(float amount)
        {
            this.m_CurrentRotation = this.m_BuildingSystemSettings.m_RotationSpeed * amount * Vector3.up;
        }

        /// <summary>
        /// Rotates the Preview in blueprint defined increments based on the orientation provided
        /// </summary>
        /// <param name="orientation">Orientation to rotate</param>
        public void SnapRotate(Vector3 orientation)
        {
            if (this.m_CurrentPreview != null)
            {
                this.m_CurrentPreview.Orientation += orientation;
            }
        }

        /// <summary>
        /// Pauses the Building System
        /// </summary>
        public void Pause()
        {
            this.m_IsPaused = true;
            if (this.m_CurrentPreview != null)
            {
                this.m_CurrentPreview.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Resumes the Building System
        /// </summary>
        public void Resume()
        {
            this.m_IsPaused = false;

            if (this.m_CurrentPreview != null)
            {
                this.m_CurrentPreview.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Gets the reason the blueprint can't be built
        /// </summary>
        /// <returns>String reason the building cannot be placed</returns>
        public string GetInvalidReasons()
        {
            if (this.m_CurrentPreview != null)
            {
                return this.m_CurrentPreview.GetReasons();
            }

            return string.Empty;
        }

        /// <summary>
        /// Sets the blueprint to the given blueprint. If it matches then it clears the blueprint
        /// </summary>
        /// <param name="blueprint">Blueprint to use</param>
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

        /// <summary>
        /// Sets the screen ray for translating pointer location to raycast hit.
        /// </summary>
        /// <param name="screenRay">Screen Ray</param>
        public void SetScreenRay(Ray screenRay)
        {
            this.m_ScreenRay = screenRay;
        }

        /// <summary>
        /// Attempts to build the current blueprint as a building.
        /// </summary>
        /// <param name="obj">Successful building created</param>
        /// <param name="parent">Parent transform to attach this to if it shouldn't be the build system</param>
        /// <returns>True if successful</returns>
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

                obj = this.CreateBuilding(parent ? parent : this.transform, this.m_CurrentPreview.transform.position, this.m_CurrentPreview.transform.rotation, this.m_CurrentBlueprint, sp);
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

        /// <summary>
        /// Tries to remove the building.
        /// </summary>
        /// <param name="canRemove">Function to test if the building can be removed</param>
        /// <returns>True if the building is removed</returns>
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

        /// <summary>
        /// Undoes the last command.
        /// </summary>
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
                    Building obj = this.CreateBuilding(evt.Parent, evt.Position, evt.Rotation, evt.Blueprint, evt.SnapPoint);
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

        /// <summary>
        /// Replays the last command that was undone, so long as no new actions were taken in between.
        /// </summary>
        public void Redo()
        {
            if (this.m_RedoEvents.Any())
            {
                BuildingSystemEvent evt = this.m_RedoEvents.Last();
                if (evt.Type == BuildingSystemEventType.Add)
                {
                    Building obj = this.CreateBuilding(evt.Parent, evt.Position, evt.Rotation, evt.Blueprint, evt.SnapPoint);
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

        /// <summary>
        /// Adds the event to the event list and clears the redo list.
        /// </summary>
        /// <param name="evt">Event to add</param>
        private void AddEvent(BuildingSystemEvent evt)
        {
            // Clear redo list
            this.m_RedoEvents.Clear();
            this.m_EventList.Add(evt);
        }

        /// <summary>
        /// Instaitates the building
        /// </summary>
        /// <param name="parent">Parent transform</param>
        /// <param name="position">Position to place it</param>
        /// <param name="rotation">Building Rotation</param>
        /// <param name="blueprint">Blue print</param>
        /// <param name="snapPoint">Snap Point</param>
        /// <returns>Created Building</returns>
        private Building CreateBuilding(Transform parent, Vector3 position, Quaternion rotation, Blueprint blueprint, SnapPoint snapPoint)
        {
            Building obj = Instantiate(blueprint.Prefab, parent);

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

        /// <summary>
        /// Destroys the building
        /// </summary>
        /// <param name="building">Building to destroy</param>
        private void DestroyBuilding(Building building)
        {
            this.OnPreBuildingRemove?.Invoke(building);
            Destroy(building.gameObject);
        }

        /// <summary>
        /// Updates the position and rotation of the preview
        /// </summary>
        private void UpdatePreviewPositionAndRotation()
        {
            if (!this.m_IsPaused && this.m_CurrentPreview != null && this.m_CurrentBlueprint != null)
            {
                this.m_CurrentPreview.UpdatePositionAndRotation(this.m_CurrentPosition, this.m_CurrentRotation, this.m_CurrentFace);
            }
        }

        /// <summary>
        /// On Awake message
        /// </summary>
        private void Awake()
        {
            this.m_EventList = new List<BuildingSystemEvent>();
            this.m_RedoEvents = new List<BuildingSystemEvent>();
        }

        /// <summary>
        /// Update message
        /// </summary>
        private void Update()
        {
            this.UpdatePreviewPositionAndRotation();
        }

        /// <summary>
        /// FixedUpdate message
        /// </summary>
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

        /// <summary>
        /// Event Type
        /// </summary>
        enum BuildingSystemEventType
        {
            Add,
            Remove
        }

        /// <summary>
        /// Building System Event
        /// </summary>
        class BuildingSystemEvent
        {
            /// <summary>
            /// Gets or sets the event type
            /// </summary>
            public BuildingSystemEventType Type { get; set; }

            /// <summary>
            /// Gets or sets the building
            /// </summary>
            public Building Building { get; set; }

            /// <summary>
            /// Gets or sets the blueprint
            /// </summary>
            public Blueprint Blueprint { get; set; }

            /// <summary>
            /// Gets or sets the position
            /// </summary>
            public Vector3 Position { get; set; }

            /// <summary>
            /// Gets or sets the snappoint
            /// </summary>
            public SnapPoint SnapPoint { get; set; }

            /// <summary>
            /// Gets or sets the rotation
            /// </summary>
            public Quaternion Rotation { get; set; }

            /// <summary>
            /// Gets or sets the transform parent
            /// </summary>
            public Transform Parent { get; set; }
        }
    }
}
