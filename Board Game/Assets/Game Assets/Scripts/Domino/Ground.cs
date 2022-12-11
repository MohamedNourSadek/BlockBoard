using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Ground_Side { Center, Left, Right}
public class Ground : MonoBehaviour
{
    [Header("Ground Positioning")]
    [SerializeField] Camera_Controller camera_Controller;
    [SerializeField] float H_Spacing = 0.085f;
    [SerializeField] float V_Spacing = 0.05f;
    [SerializeField] float Max_Margin = 0.15f;
    [SerializeField] Vector2 Switch_Fix_lastIsDouble;
    [SerializeField] Vector2 Switch_Fix_IamDouble;
    [SerializeField] Vector2 Switch_Fix_No_Double;

    [SerializeField] GameObject CenterPosition;
    [SerializeField] GameObject LeftPosition;
    [SerializeField] GameObject RightPosition;

    [SerializeField] GameObject Camera_LeftSoft;
    [SerializeField] GameObject Camera_Right;
    [SerializeField] GameObject Camera_LeftHard;
    [SerializeField] LayerMask Bounds;
    [SerializeField] Player p;

    public Tile Center_Card;
    public List<Tile> Left_cards = new List<Tile>();
    public List<Tile> Right_cards = new List<Tile>();

    public tile_name left_Avaiable;
    public tile_name right_Avaiable;

    //Fix for a glitch (Tiles played after a double tile played after switching is placed improperly. )
    bool SwitchDirection_OnLeft; 
    bool SwitchDirection_OnRight; //Fix for a glitch (Tiles played after a double tile played after switching is placed improperly. )
    bool SwitchDirection_OnLastLeft;
    bool SwitchDirection_OnLastRight;


    //Place a card properly on the ground.
    public void Set_Card(Tile tile, Ground_Side ground_side, Card_Side card_side)
    {
        Tile lastCard;

        SwitchDirection_OnLastLeft = SwitchDirection_OnLeft;
        SwitchDirection_OnLastRight = SwitchDirection_OnRight;

        bool Switch_Direction = false;


        //Switch Direction or not
        if (ground_side == Ground_Side.Right)
        {
            lastCard = (Right_cards.Count >= 1) ? Right_cards[Right_cards.Count - 1] : Center_Card;
            RaycastHit hit;
            Physics.Raycast(lastCard.FinalPosition, RightPosition.transform.forward, out hit,50f,Bounds);
            float Distance = (hit.point - lastCard.FinalPosition).magnitude;
            Switch_Direction = (Distance < Max_Margin);

            if (Switch_Direction)
            {
                RightPosition.transform.Rotate(new Vector3(0f, 90f, 0f));
                SwitchDirection_OnRight = true;
            }
            else
            {
                SwitchDirection_OnRight = false;
            }
        }
        else if (ground_side == Ground_Side.Left)
        {
            lastCard = (Left_cards.Count >= 1) ? Left_cards[Left_cards.Count - 1] : Center_Card;
            RaycastHit hit;
            Physics.Raycast(lastCard.FinalPosition, -LeftPosition.transform.forward, out hit, 50f, Bounds);
            float Distance = (hit.point - lastCard.FinalPosition).magnitude;
            Switch_Direction = (Distance < Max_Margin);

            if (Switch_Direction)
            {
                LeftPosition.transform.Rotate(new Vector3(0f, 90f, 0f));
                SwitchDirection_OnLeft = true;
            }
            else
            {
                SwitchDirection_OnLeft = false;
            }
        }
        else
        {
            Switch_Direction = false;
            lastCard = Center_Card;
        }


        ////////////// Setting Avaiable sides////////////////////////////
        if (ground_side == Ground_Side.Center)
        {
            if (tile.Double)
            {
                right_Avaiable = tile.Up;
                left_Avaiable = tile.Up;
            }
            else if (card_side == Card_Side.Up)
            {
                right_Avaiable = tile.Up;
                left_Avaiable = tile.Down;
            }
            else if (card_side == Card_Side.Down)
            {
                right_Avaiable = tile.Down;
                left_Avaiable = tile.Up;
            }
        }
        else if (ground_side == Ground_Side.Left)
        {
            if (tile.Double)
                left_Avaiable = tile.Up;
            else if (card_side == Card_Side.Up)
                left_Avaiable = tile.Down;
            else if (card_side == Card_Side.Down)
                left_Avaiable = tile.Up;
        }
        else if(ground_side == Ground_Side.Right)
        {
            //it does the opposite because it's fliped on right side

            if (tile.Double)
                right_Avaiable = tile.Up;
            else if (card_side == Card_Side.Up)
                right_Avaiable = tile.Down; 
            else if (card_side == Card_Side.Down)
                right_Avaiable = tile.Up;   
        }


        ///////////  Rotation ///////////////////////////

        //reset rotation
        if(ground_side == Ground_Side.Center)
            tile.transform.rotation = CenterPosition.transform.rotation;
        else if(ground_side == Ground_Side.Right)
            tile.transform.rotation = RightPosition.transform.rotation;
        else if (ground_side == Ground_Side.Left)
            tile.transform.rotation = LeftPosition.transform.rotation;


        //if it's a double and not switching
        if (tile.Double && !Switch_Direction)
        {
            tile.transform.Rotate(new Vector3(0f, 0f, 0f));
        }

        //if this is the first card and not a double (Up is pointing left)
        else if (!Center_Card)
        {
            tile.transform.Rotate(new Vector3(0f, -90, 0f));
        }

        //if it's on the right side and not a double
        else if (ground_side == Ground_Side.Right)
        {
            if (card_side == Card_Side.Up)
                tile.transform.Rotate(new Vector3(0f, 90f, 0f));
            if (card_side == Card_Side.Down)
                tile.transform.Rotate(new Vector3(0f, -90f, 0f));
        }

        //if it's on the left side and not a double
        else if (ground_side == Ground_Side.Left)
        {
            if (card_side == Card_Side.Up)
                tile.transform.Rotate(new Vector3(0f, -90f, 0f));
            if (card_side == Card_Side.Down)
                tile.transform.Rotate(new Vector3(0f, 90f, 0f));
        }


        ///////////////// Position //////////////////////////////
        ///
        Vector3 Position = new Vector3();

        if (ground_side == Ground_Side.Center)
        {
            Center_Card = tile;
            Position = CenterPosition.transform.position;
        }
        else if(ground_side == Ground_Side.Left)
        {
            if ((lastCard.Double && !SwitchDirection_OnLastLeft) || tile.Double)
                Position = lastCard.FinalPosition - (LeftPosition.transform.forward * V_Spacing);
            else
                Position = lastCard.FinalPosition - (LeftPosition.transform.forward * H_Spacing);


            //Switching Fix
            if (Switch_Direction)
            {
                if (lastCard.Double)
                {
                    Position += Switch_Fix_lastIsDouble.x * LeftPosition.transform.forward;
                    Position += Switch_Fix_lastIsDouble.y * LeftPosition.transform.right;
                }
                else if (tile.Double)
                {
                    Position += Switch_Fix_IamDouble.x * LeftPosition.transform.forward;
                    Position += Switch_Fix_IamDouble.y * LeftPosition.transform.right;
                }
                else
                {
                    Position += Switch_Fix_No_Double.x * LeftPosition.transform.forward;
                    Position += Switch_Fix_No_Double.y * LeftPosition.transform.right;
                }
            }

            Left_cards.Add(tile);
        }
        else if (ground_side == Ground_Side.Right)
        {
            if ((lastCard.Double && !SwitchDirection_OnLastRight) || tile.Double)
                Position = lastCard.FinalPosition + (RightPosition.transform.forward * V_Spacing);
            else
                Position = lastCard.FinalPosition + (RightPosition.transform.forward * H_Spacing);


            //Switching Fix
            if (Switch_Direction)
            {
                if (lastCard.Double)
                {
                    Position -= Switch_Fix_lastIsDouble.x * RightPosition.transform.forward;
                    Position -= Switch_Fix_lastIsDouble.y * RightPosition.transform.right;
                }
                else if (tile.Double)
                {
                    Position -= Switch_Fix_IamDouble.x * RightPosition.transform.forward;
                    Position -= Switch_Fix_IamDouble.y * RightPosition.transform.right;
                }
                else
                {
                    Position -= Switch_Fix_No_Double.x * RightPosition.transform.forward;
                    Position -= Switch_Fix_No_Double.y * RightPosition.transform.right;
                }
            }

            Right_cards.Add(tile);
        }

        tile.FinalPosition = Position;
        StartCoroutine(Move_Animation(tile));

        tile.gameObject.SetActive(true);

    }

    public float speed = 0.1f;

    IEnumerator Move_Animation(Tile myOjb)
    {

        Vector3 Point2 = myOjb.FinalPosition + (0.1f * Vector3.up); ;
        Vector3 FinalPosition = myOjb.FinalPosition;

        while ((myOjb.gameObject.transform.position - Point2).magnitude >= 0.001f)
        {
            myOjb.gameObject.transform.position = Vector3.Lerp(myOjb.transform.position, Point2, Time.deltaTime / speed);
            yield return new WaitForSecondsRealtime(Time.deltaTime* speed);
        }

        while ((myOjb.gameObject.transform.position - FinalPosition).magnitude >= 0.001f)
        {
            myOjb.gameObject.transform.position = Vector3.Lerp(myOjb.transform.position, FinalPosition, Time.deltaTime / speed);
            yield return new WaitForSecondsRealtime(Time.deltaTime* speed);
        }
    }

    //Get potential next place
    public Vector3 GetNextPosition(Ground_Side ground_side)
    {
        Tile lastCard = Center_Card;
        bool Switch_Direction = false;
        Vector3 position = Vector3.zero;

        if (ground_side == Ground_Side.Right)
        {
            lastCard = (Right_cards.Count >= 1) ? Right_cards[Right_cards.Count - 1] : Center_Card;
            RaycastHit hit;
            Physics.Raycast(lastCard.FinalPosition, RightPosition.transform.forward, out hit, 50f, Bounds);
            float Distance = (hit.point - lastCard.FinalPosition).magnitude;
            Switch_Direction = (Distance < Max_Margin);

            if (Switch_Direction)
                RightPosition.transform.Rotate(new Vector3(0f, 90f, 0f));

        }
        else if (ground_side == Ground_Side.Left)
        {
            lastCard = (Left_cards.Count >= 1) ? Left_cards[Left_cards.Count - 1] : Center_Card;
            RaycastHit hit;
            Physics.Raycast(lastCard.FinalPosition, -LeftPosition.transform.forward, out hit, 50f, Bounds);
            float Distance = (hit.point - lastCard.FinalPosition).magnitude;
            Switch_Direction = (Distance < Max_Margin);

            if (Switch_Direction)
                LeftPosition.transform.Rotate(new Vector3(0f, 90f, 0f));
        }

        if (ground_side == Ground_Side.Left)
        {
            if (lastCard.Double)
                position = lastCard.FinalPosition - (LeftPosition.transform.forward * V_Spacing);
            else
                position = lastCard.FinalPosition - (LeftPosition.transform.forward * H_Spacing);

            //Switching Fix
            if (Switch_Direction)
            {
                if (lastCard.Double)
                {
                    position += Switch_Fix_lastIsDouble.x * LeftPosition.transform.forward;
                    position += Switch_Fix_lastIsDouble.y * LeftPosition.transform.right;
                }
                else
                {
                    position += Switch_Fix_No_Double.x * LeftPosition.transform.forward;
                    position += Switch_Fix_No_Double.y * LeftPosition.transform.right;
                }
            }
        }
        else if (ground_side == Ground_Side.Right)
        {
            if (lastCard.Double)
                position = lastCard.FinalPosition + (RightPosition.transform.forward * V_Spacing);
            else
                position = lastCard.FinalPosition + (RightPosition.transform.forward * H_Spacing);

            //Switching Fix
            if (Switch_Direction)
            {
                if (lastCard.Double)
                {
                    position -= Switch_Fix_lastIsDouble.x * RightPosition.transform.forward;
                    position -= Switch_Fix_lastIsDouble.y * RightPosition.transform.right;
                }
                else
                {
                    position -= Switch_Fix_No_Double.x * RightPosition.transform.forward;
                    position -= Switch_Fix_No_Double.y * RightPosition.transform.right;
                }
            }
        }

        if (ground_side == Ground_Side.Right)
        {
            if (Switch_Direction)
                RightPosition.transform.Rotate(new Vector3(0f, -90f, 0f));
        }
        else if (ground_side == Ground_Side.Left)
        {
            if (Switch_Direction)
                LeftPosition.transform.Rotate(new Vector3(0f, -90f, 0f));
        }

        return position;
    }


    //Change Camera Position and rotation
    IEnumerator MoveCamera(GameObject finalPosition)
    {
        Vector3 distance = finalPosition.transform.position - Camera.main.transform.position;
        Vector3 Delta_Rotation = (finalPosition.transform.eulerAngles - Camera.main.transform.eulerAngles);

        while((Camera.main.transform.position - finalPosition.transform.position).magnitude > 0.01f)
        {
            Camera.main.transform.position += (distance * Time.fixedDeltaTime);
            Camera.main.transform.eulerAngles += (Delta_Rotation * Time.fixedDeltaTime);
            yield return new WaitForSecondsRealtime(Time.fixedDeltaTime);
            p.ReOrganize_Cards_InHand();
        }
    }
}
