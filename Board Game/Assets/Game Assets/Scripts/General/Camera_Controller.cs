using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camera_Controller : MonoBehaviour
{
    [SerializeField] public GameObject Camera_Transform;
    [SerializeField] public GameObject RotateAround_Obj;

    [Header("Rotation Parameters")]
    [SerializeField] Vector3 RotatingSpeed;
    [SerializeField] Vector2 Rotation_X_Constrains;
    [SerializeField] float BackTo_Origin_Speed = 1f;

    [Header("Zoom Parameters")]
    [SerializeField] float ZoomSpeed = 1f;
    [SerializeField] float Zoom_ScrollSpeed = 100f;
    [SerializeField] Vector2 Zoom_Constrains;
    [SerializeField] float BackTo_Origin_Position_Speed = 1f;

    public Quaternion InitialRotation;
    public Vector3 InitialPosition;

    private void Start()
    {
        Change_Transform();
    }
    public void Change_Transform()
    {
        InitialRotation = transform.localRotation;
        InitialPosition = transform.position;

        Camera_Transform.transform.position = InitialPosition;
        Camera_Transform.transform.rotation = InitialRotation;
    }
    public void Change_Transform(Vector3 Position, Quaternion Rotation)
    {
        InitialRotation = Rotation;
        InitialPosition = Position;

        Camera_Transform.transform.rotation = Rotation;
        Camera_Transform.transform.position = Position;
    }


    //General
    bool First_Touch = true;
    float Added_Rotation_X;

    //Rotation Internal Variables
    Vector3 InitialScreenPosition;


    //Zoom Internal Variables
    Vector2 First_Pervious_Touch = new Vector2();
    Vector2 Second_Previous_Touch = new Vector2();

    bool Zoom_Touch_Down = false;
    Vector3 Current_Position;

    private void Update()
    {
        //Rotation
        if((Input.GetMouseButtonDown(0) || Input.touchCount >= 1) && First_Touch)
        {
            First_Touch = false;

            if (Input.GetMouseButtonDown(0))
                InitialScreenPosition = Input.mousePosition;

            if (Input.touchCount >= 1)
                InitialScreenPosition = Input.GetTouch(0).position;
        }
        else if((Input.GetMouseButton(0) || Input.touchCount == 1) && !(Input.touchCount > 1))
        {
            Vector3 Difference = new Vector3();

            if (Input.GetMouseButton(0))
                Difference = Input.mousePosition - InitialScreenPosition;
            else if (Input.touchCount == 1)
                Difference = new Vector3(Input.touches[0].position.x, Input.touches[0].position.y, 0f) - InitialScreenPosition;

            float Delta_X = -Difference.y * RotatingSpeed.x * Time.deltaTime;
            float Delta_Y = Difference.x * RotatingSpeed.y * Time.deltaTime;

            Added_Rotation_X = Mathf.Clamp(Added_Rotation_X + Delta_X, Rotation_X_Constrains.x, Rotation_X_Constrains.y);

            if ((Added_Rotation_X < Rotation_X_Constrains.y) && (Added_Rotation_X > Rotation_X_Constrains.x))
                this.transform.RotateAround(RotateAround_Obj.transform.position, transform.right, Delta_X);

            this.transform.RotateAround(RotateAround_Obj.transform.position, Vector3.up, Delta_Y);
        }
        else
        {
            First_Touch = true;

            //Reset Camera
            if (Cross_Scene_Data.Camera_Snap)
            {
                this.transform.rotation = Quaternion.Lerp(this.transform.rotation, InitialRotation, BackTo_Origin_Speed * Time.deltaTime);
                Added_Rotation_X = Mathf.Lerp(Added_Rotation_X, 0f, BackTo_Origin_Speed * Time.deltaTime);
            }
        }

        //Zoom
        if(Input.touchCount == 2)
        {
            if(!Zoom_Touch_Down)
            {
                Zoom_Touch_Down = true;

                if (Input.touchCount == 2)
                {
                    First_Pervious_Touch = Input.touches[0].position;
                    Second_Previous_Touch = Input.touches[1].position;
                }

                Current_Position = transform.position;
            }
            else
            {
                float Zoom_Factor;

                Vector2 Current_First_Touch = Input.touches[0].position;
                Vector2 Current_Second_Touch = Input.touches[1].position;

                float Current_Distance = (Current_First_Touch - Current_Second_Touch).magnitude;
                float Previous_Distance = (First_Pervious_Touch - Second_Previous_Touch).magnitude;

                Zoom_Factor = Current_Distance - Previous_Distance;

                float Distance = (this.transform.position - RotateAround_Obj.transform.position).magnitude;

                bool Within_Limits = (Distance > Zoom_Constrains.x) && (Distance < Zoom_Constrains.y);
                bool Out_ButGettingIn = ((Distance < Zoom_Constrains.x) && (Zoom_Factor < 0)) || ((Distance > Zoom_Constrains.y) && (Zoom_Factor > 0));
                
                if (Within_Limits || Out_ButGettingIn)
                {
                    transform.position = Current_Position + (transform.forward * Zoom_Factor * ZoomSpeed);
                }
            }
        }
        else
        {
            if (Cross_Scene_Data.Camera_Snap)
            {
                transform.position = Vector3.Lerp(transform.position, InitialPosition, Time.deltaTime * BackTo_Origin_Position_Speed);
            }

            Zoom_Touch_Down = false;
        }
    }
}
