using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using ZTrank.BuildingSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private LayerMask m_GroundMask;

    [SerializeField]
    private BuildingSystem m_BuildingSystem;

    [SerializeField]
    private LayerMask m_UILayer;

    [SerializeField]
    private Canvas m_Canvas;


    private void Start()
    {
        this.m_BuildingSystem.OnBuildSuccess += obj => Debug.Log($"Building Success: {obj.name}");
        this.m_BuildingSystem.OnPreBuildingRemove += obj => Debug.Log($"Removing: {obj.name}");
    }

    private void Update()
    {
        this.m_BuildingSystem.SetScreenRay(Camera.main.ScreenPointToRay(Input.mousePosition));
        //this.m_BuildingSystem.MovePreview(this.m_GroundPointerPosition);

        if (Input.GetMouseButtonDown(0) && !this.IsHittingUI())
        {
            if (this.m_BuildingSystem.TryBuild(out Building child))
            {
                Debug.Log($"Building Successful: {child.name}");
            }
            else
            {
                Debug.Log($"{this.m_BuildingSystem.GetInvalidReasons()}");
            }
        }
        

        if (Input.GetMouseButtonDown(1) && !this.IsHittingUI())
        {
            if (this.m_BuildingSystem.TryRemove())
            {
                Debug.Log($"Building removed!");
            }
            else
            {
                this.m_BuildingSystem.SetBlueprint(null);
            }
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            if (this.m_BuildingSystem.IsPaused)
            {
                this.m_BuildingSystem.Resume();
            }
            else
            {
                this.m_BuildingSystem.Pause();
            }
        }
    }

    private bool IsHittingUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void FixedUpdate()
    {
        
    }
}
