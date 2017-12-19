using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour {


    public float zoomSpeed = 3f;
    public float MoveSpeed = 30f;

    public float Width = 10f;
    public float FrontDistance = 0f;
    public float BackDistance = 10f;

    public float MaxZoom = 5f;
    public float MinZoom = -2f;
    public float CurrentZoom = 0f;

    public Grid gameField;
    public ButtonStoreClick Button;

    private Vector3 oldPos;
    private Vector3 panOrigin;
    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        //zoom
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            float NewZoom = CurrentZoom + scroll * zoomSpeed;
            if (NewZoom < MaxZoom && NewZoom > MinZoom)
            {
                CurrentZoom = NewZoom;
                transform.Translate(0, 0, scroll * zoomSpeed);
            }
        }

        MouseMove();
    }

    void MouseMove()
    {
        if (!gameField.IsActiveItemMove() && !Button.IsStoreOpened())
        {
            ControlPC();
            ControlMobile();
        }
    }

    void ControlPC()
    {
        if (Input.GetMouseButtonDown(1))
        {
            oldPos = transform.position;
            panOrigin = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        }
        if (Input.GetMouseButton(1))
        {
            Vector3 pos = Camera.main.ScreenToViewportPoint(Input.mousePosition) - panOrigin;
            pos = new Vector3(pos.x, pos.z, pos.y);
            Vector3 newPos = Vector3.Lerp(transform.position, oldPos + -pos * MoveSpeed, Time.deltaTime);
            transform.position = MakeMove(newPos);
        }

    }

    void ControlMobile()
    {
        if (Input.touchCount == 1)
        {
            Touch LastTouch = Input.GetTouch(0);
            if (LastTouch.phase == TouchPhase.Began)
            {

                oldPos = transform.position;
                panOrigin = Camera.main.ScreenToViewportPoint(LastTouch.position);
            }
            else if (LastTouch.phase == TouchPhase.Stationary)
            {
                Vector3 pos = Camera.main.ScreenToViewportPoint(LastTouch.position) - panOrigin;
                pos = new Vector3(pos.x, pos.z, pos.y);
                Vector3 newPos = Vector3.Lerp(transform.position, oldPos + -pos * MoveSpeed, Time.deltaTime);
                transform.position = MakeMove(newPos);
            }  
        }
    }

    Vector3 MakeMove(Vector3 Position)
    {
        Vector3 GameFieldPosition = gameField.transform.position;
        float DiffInX = Position.x - GameFieldPosition.x;
        float DiffInZ = Position.z - GameFieldPosition.z;

        if (DiffInX > Width)
        {
            Position.x = GameFieldPosition.x + Width;
        }
        else if (DiffInX < -Width)
        {
            Position.x = GameFieldPosition.x - Width;
        }

        if (DiffInZ > FrontDistance)
        {
            Position.z = GameFieldPosition.z + FrontDistance;
        }
        else if (DiffInZ < -BackDistance)
        {
            Position.z = GameFieldPosition.z - BackDistance;
        }
        return Position;
    }
}
