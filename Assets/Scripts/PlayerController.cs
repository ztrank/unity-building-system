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

    [SerializeField]
    private float m_RotationSpeed = 10f;

    [SerializeField]
    private Camera m_Camera;


    private void Start()
    {
        this.m_BuildingSystem.OnBuildSuccess += obj => Debug.Log($"Building Success: {obj.name}");
        this.m_BuildingSystem.OnPreBuildingRemove += obj => Debug.Log($"Removing: {obj.name}");
    }


    [SerializeField]
    private float m_MovementSpeed;

    private void PlayerMovement()
    {
        Vector3 movementVector = Vector3.zero;
        if (Input.GetKey(KeyCode.W))
        {
            // Move forward
            movementVector += this.m_MovementSpeed * Time.deltaTime * Vector3.forward;
        }

        if (Input.GetKey(KeyCode.S))
        {
            // Move forward
            movementVector -= this.m_MovementSpeed * Time.deltaTime * Vector3.forward;
        }

        if (Input.GetKey(KeyCode.A))
        {
            // Move left
            movementVector -= this.m_MovementSpeed * Time.deltaTime * Vector3.right;
        }

        if (Input.GetKey(KeyCode.D))
        {
            // Move Right
            movementVector += this.m_MovementSpeed * Time.deltaTime * Vector3.right;
        }

        if (Input.GetKey(KeyCode.R))
        {
            movementVector += this.m_MovementSpeed / 2 * Time.deltaTime * Vector3.up;
        }

        if (Input.GetKey(KeyCode.F))
        {
            movementVector -= this.m_MovementSpeed / 2 * Time.deltaTime * Vector3.up;
        }

        if (movementVector != Vector3.zero)
        {
            this.transform.Translate(movementVector);
        }
    }


    private void PlayerRotation()
    {
        /*float yRotation = 0;
        if (Input.GetKey(KeyCode.Q))
        {
            yRotation -= this.m_RotationSpeed * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.E))
        {
            yRotation += this.m_RotationSpeed * Time.deltaTime;
        }

        if (yRotation != 0)
        {
            this.transform.Rotate(Vector3.up * yRotation);
        }*/

        if (Input.GetMouseButton(2))
        {
            float xDelta = this.m_RotationSpeed * Input.GetAxis("Mouse X");
            float yDelta = this.m_RotationSpeed * Input.GetAxis("Mouse Y");
            this.m_Camera.transform.Rotate(Vector3.right * -yDelta);
            this.transform.Rotate(Vector3.up * xDelta);
        }
    }

    private void RotateBlueprint()
    {
        if (this.m_BuildingSystem.IsSnapped)
        {
            Vector3 orientation = Vector3.zero;
            if (Input.GetKeyDown(KeyCode.Q))
            {
                orientation = new Vector3(0, 1, 0);
            }
            else if (Input.GetKeyDown(KeyCode.E))
            {
                orientation = new Vector3(0, -1, 0);
            }
            this.m_BuildingSystem.SnapRotate(orientation);
        }
        else
        {
            float yRotation = 0;
            if (Input.GetKey(KeyCode.Q))
            {
                yRotation -= this.m_RotationSpeed * Time.deltaTime;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                yRotation += this.m_RotationSpeed * Time.deltaTime;
            }
            this.m_BuildingSystem.Rotate(yRotation);
        }
    }

    private void Update()
    {
        this.m_BuildingSystem.SetScreenRay(Camera.main.ScreenPointToRay(Input.mousePosition));
        //this.m_BuildingSystem.MovePreview(this.m_GroundPointerPosition);
        this.PlayerRotation();
        this.PlayerMovement();

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

        this.RotateBlueprint();
    }

    private bool IsHittingUI()
    {
        return EventSystem.current.IsPointerOverGameObject();
    }

    private void FixedUpdate()
    {
        
    }
}
