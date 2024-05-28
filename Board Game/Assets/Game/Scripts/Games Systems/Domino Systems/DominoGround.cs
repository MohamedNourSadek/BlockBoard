using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;


public enum GroundSide { Center, Left, Right}


public class DominoGround : MonoBehaviour
{
    [Header("Ground Positioning Constants")]
    [SerializeField] float HorizontalSpacing = 0.085f;
    [SerializeField] float VerticalSpacing = 0.05f;
    [SerializeField] float MaxMargin = 0.15f;
    [SerializeField] Vector2 SwitchFixIsLastDouble;
    [SerializeField] Vector2 SwitchFixAmIDouble;
    [SerializeField] Vector2 SwitchFixNoDouble;
    [SerializeField] float AnimationSpeed = 0.1f;

    [Header("Ground Positioning Objects")]
    [SerializeField] GameObject CenterPosition;
    [SerializeField] GameObject LeftPosition;
    [SerializeField] GameObject RightPosition;

    [Header("Other")]
    [SerializeField] LayerMask Bounds;

    public DominoTile CenterCard;
    public List<DominoTile> LeftCards = new List<DominoTile>();
    public List<DominoTile> RightCards = new List<DominoTile>();
    public TileName LeftAvailable;
    public TileName RightAvailable;


    #region Public Functions
    public void PlayCardOnGround(DominoTile tile, GroundSide groundSide, CardSide cardSide)
    {
        bool didSwitchDirection = SwitchDirectionsIfNeeded(groundSide);

        SetNextAvailableSides(tile, groundSide, cardSide);
        SetTileRotation(tile, groundSide, cardSide, didSwitchDirection);
        SetTileFinalPosition(tile, groundSide, didSwitchDirection);

        if (groundSide == GroundSide.Center)
        {
            CenterCard = tile;
        }
        else if(groundSide == GroundSide.Left)
        {
            LeftCards.Add(tile);
        }
        else if(groundSide == GroundSide.Right)
        {
            RightCards.Add(tile);
        }
    }
    public Vector3 GetNextPosition(GroundSide groundSide)
    {
        DominoTile lastCard = CenterCard;
        bool Switch_Direction = false;
        Vector3 position = Vector3.zero;

        if (groundSide == GroundSide.Right)
        {
            lastCard = (RightCards.Count >= 1) ? RightCards[RightCards.Count - 1] : CenterCard;
            RaycastHit hit;
            Physics.Raycast(lastCard.FinalPosition, RightPosition.transform.forward, out hit, 50f, Bounds);
            float Distance = (hit.point - lastCard.FinalPosition).magnitude;
            Switch_Direction = (Distance < MaxMargin);

            if (Switch_Direction)
                RightPosition.transform.Rotate(new Vector3(0f, 90f, 0f));
        }
        else if (groundSide == GroundSide.Left)
        {
            lastCard = (LeftCards.Count >= 1) ? LeftCards[LeftCards.Count - 1] : CenterCard;
            RaycastHit hit;
            Physics.Raycast(lastCard.FinalPosition, -LeftPosition.transform.forward, out hit, 50f, Bounds);
            float Distance = (hit.point - lastCard.FinalPosition).magnitude;
            Switch_Direction = (Distance < MaxMargin);

            if (Switch_Direction)
                LeftPosition.transform.Rotate(new Vector3(0f, 90f, 0f));
        }

        if (groundSide == GroundSide.Left)
        {
            if (lastCard.Double)
                position = lastCard.FinalPosition - (LeftPosition.transform.forward * VerticalSpacing);
            else
                position = lastCard.FinalPosition - (LeftPosition.transform.forward * HorizontalSpacing);

            //Switching Fix
            if (Switch_Direction)
            {
                if (lastCard.Double)
                {
                    position += SwitchFixIsLastDouble.x * LeftPosition.transform.forward;
                    position += SwitchFixIsLastDouble.y * LeftPosition.transform.right;
                }
                else
                {
                    position += SwitchFixNoDouble.x * LeftPosition.transform.forward;
                    position += SwitchFixNoDouble.y * LeftPosition.transform.right;
                }
            }
        }
        else if (groundSide == GroundSide.Right)
        {
            if (lastCard.Double)
                position = lastCard.FinalPosition + (RightPosition.transform.forward * VerticalSpacing);
            else
                position = lastCard.FinalPosition + (RightPosition.transform.forward * HorizontalSpacing);

            //Switching Fix
            if (Switch_Direction)
            {
                if (lastCard.Double)
                {
                    position -= SwitchFixIsLastDouble.x * RightPosition.transform.forward;
                    position -= SwitchFixIsLastDouble.y * RightPosition.transform.right;
                }
                else
                {
                    position -= SwitchFixNoDouble.x * RightPosition.transform.forward;
                    position -= SwitchFixNoDouble.y * RightPosition.transform.right;
                }
            }
        }

        if (groundSide == GroundSide.Right)
        {
            if (Switch_Direction)
                RightPosition.transform.Rotate(new Vector3(0f, -90f, 0f));
        }
        else if (groundSide == GroundSide.Left)
        {
            if (Switch_Direction)
                LeftPosition.transform.Rotate(new Vector3(0f, -90f, 0f));
        }

        return position;
    }
    #endregion

    #region Private Functions
    private bool SwitchDirectionsIfNeeded(GroundSide groundSide)
    {
        DominoTile lastCard = GetLastCard(groundSide);
        bool didSwitchDirection = false;

        if (groundSide == GroundSide.Right)
        {
            didSwitchDirection = ShouldSwitchDirection(lastCard, RightPosition.transform.forward);

            if (didSwitchDirection)
            {
                RightPosition.transform.Rotate(new Vector3(0f, 90f, 0f));
            }
        }
        else if (groundSide == GroundSide.Left)
        {
            didSwitchDirection = ShouldSwitchDirection(lastCard, -LeftPosition.transform.forward);

            if (didSwitchDirection)
            {
                LeftPosition.transform.Rotate(new Vector3(0f, 90f, 0f));
            }
        }
        else if (groundSide == GroundSide.Center)
        {
            didSwitchDirection = false;
        }

        return didSwitchDirection;
    }
    private bool ShouldSwitchDirection(DominoTile lastCard, Vector3 direction)
    {
        RaycastHit hit;
        Physics.Raycast(lastCard.FinalPosition, direction, out hit, 50f, Bounds);
        float distance = (hit.point - lastCard.FinalPosition).magnitude;
        return (distance < MaxMargin);
    }
    private DominoTile GetLastCard(GroundSide groundSide)
    {
        if(groundSide == GroundSide.Right)
        {
            return (RightCards.Count >= 1) ? RightCards[^1] : CenterCard;
        }
        else if(groundSide == GroundSide.Left)
        {
            return (LeftCards.Count >= 1) ? LeftCards[^1] : CenterCard;
        }
        else
        {
            return CenterCard;
        }
    }
    private void SetNextAvailableSides(DominoTile tile, GroundSide groundSide, CardSide cardSide)
    {
        if (groundSide == GroundSide.Center)
        {
            if (tile.Double)
            {
                RightAvailable = tile.Up;
                LeftAvailable = tile.Up;
            }
            else if (cardSide == CardSide.Up)
            {
                RightAvailable = tile.Up;
                LeftAvailable = tile.Down;
            }
            else if (cardSide == CardSide.Down)
            {
                RightAvailable = tile.Down;
                LeftAvailable = tile.Up;
            }
        }
        else if (groundSide == GroundSide.Left)
        {
            if (tile.Double)
                LeftAvailable = tile.Up;
            else if (cardSide == CardSide.Up)
                LeftAvailable = tile.Down;
            else if (cardSide == CardSide.Down)
                LeftAvailable = tile.Up;
        }
        else if (groundSide == GroundSide.Right)
        {
            if (tile.Double)
                RightAvailable = tile.Up;
            else if (cardSide == CardSide.Up)
                RightAvailable = tile.Down;
            else if (cardSide == CardSide.Down)
                RightAvailable = tile.Up;
        }
    }
    private void SetTileRotation(DominoTile tile, GroundSide groundSide, CardSide cardSide, bool didSwitchDirection)
    {
        if (groundSide == GroundSide.Center)
            tile.transform.rotation = CenterPosition.transform.rotation;
        else if (groundSide == GroundSide.Right)
            tile.transform.rotation = RightPosition.transform.rotation;
        else if (groundSide == GroundSide.Left)
            tile.transform.rotation = LeftPosition.transform.rotation;

        
        if (tile.Double && !didSwitchDirection) //if it's a double and not switching
        {
            tile.transform.Rotate(new Vector3(0f, 0f, 0f));
        }
        else if (!CenterCard) //if this is the first card and not a double (Up is pointing left)
        {
            tile.transform.Rotate(new Vector3(0f, -90, 0f));
        }
        else if (groundSide == GroundSide.Right)  //if it's on the right side and not a double
        {
            if (cardSide == CardSide.Up)
                tile.transform.Rotate(new Vector3(0f, 90f, 0f));
            if (cardSide == CardSide.Down)
                tile.transform.Rotate(new Vector3(0f, -90f, 0f));
        }
        else if (groundSide == GroundSide.Left) //if it's on the left side and not a double
        {
            if (cardSide == CardSide.Up)
                tile.transform.Rotate(new Vector3(0f, -90f, 0f));
            if (cardSide == CardSide.Down)
                tile.transform.Rotate(new Vector3(0f, 90f, 0f));
        }
    }
    private void SetTileFinalPosition(DominoTile tile, GroundSide groundSide, bool didSwitchDirection)
    {
        var finalPosition = GetTilePosition(tile, groundSide, didSwitchDirection);
        tile.FinalPosition = finalPosition;
        StartCoroutine(MoveTileToPosition(tile));
    }
    private Vector3 GetTilePosition(DominoTile tile, GroundSide groundSide, bool didSwitchDirection)
    {
        DominoTile lastCard = GetLastCard(groundSide);
        Vector3 position = new Vector3();

        if (groundSide == GroundSide.Center)
        {
            position = CenterPosition.transform.position;
        }
        else if (groundSide == GroundSide.Left)
        {
            if ((lastCard.Double) || tile.Double)
                position = lastCard.FinalPosition - (LeftPosition.transform.forward * VerticalSpacing);
            else
                position = lastCard.FinalPosition - (LeftPosition.transform.forward * HorizontalSpacing);

            //Switching Fix
            if (didSwitchDirection)
            {
                if (lastCard.Double)
                {
                    position += SwitchFixIsLastDouble.x * LeftPosition.transform.forward;
                    position += SwitchFixIsLastDouble.y * LeftPosition.transform.right;
                }
                else if (tile.Double)
                {
                    position += SwitchFixAmIDouble.x * LeftPosition.transform.forward;
                    position += SwitchFixAmIDouble.y * LeftPosition.transform.right;
                }
                else
                {
                    position += SwitchFixNoDouble.x * LeftPosition.transform.forward;
                    position += SwitchFixNoDouble.y * LeftPosition.transform.right;
                }
            }
        }
        else if (groundSide == GroundSide.Right)
        {
            if ((lastCard.Double) || tile.Double)
                position = lastCard.FinalPosition + (RightPosition.transform.forward * VerticalSpacing);
            else
                position = lastCard.FinalPosition + (RightPosition.transform.forward * HorizontalSpacing);

            //Switching Fix
            if (didSwitchDirection)
            {
                if (lastCard.Double)
                {
                    position -= SwitchFixIsLastDouble.x * RightPosition.transform.forward;
                    position -= SwitchFixIsLastDouble.y * RightPosition.transform.right;
                }
                else if (tile.Double)
                {
                    position -= SwitchFixAmIDouble.x * RightPosition.transform.forward;
                    position -= SwitchFixAmIDouble.y * RightPosition.transform.right;
                }
                else
                {
                    position -= SwitchFixNoDouble.x * RightPosition.transform.forward;
                    position -= SwitchFixNoDouble.y * RightPosition.transform.right;
                }
            }
        }

        return position;
    }
    private IEnumerator MoveTileToPosition(DominoTile myOjb)
    {
        myOjb.gameObject.SetActive(true);

        Vector3 Point2 = myOjb.FinalPosition + (0.1f * Vector3.up); ;
        Vector3 FinalPosition = myOjb.FinalPosition;

        while ((myOjb.gameObject.transform.position - Point2).magnitude >= 0.001f)
        {
            myOjb.gameObject.transform.position = Vector3.Lerp(myOjb.transform.position, Point2, Time.deltaTime / AnimationSpeed);
            yield return new WaitForSecondsRealtime(Time.deltaTime* AnimationSpeed);
        }

        while ((myOjb.gameObject.transform.position - FinalPosition).magnitude >= 0.001f)
        {
            myOjb.gameObject.transform.position = Vector3.Lerp(myOjb.transform.position, FinalPosition, Time.deltaTime / AnimationSpeed);
            yield return new WaitForSecondsRealtime(Time.deltaTime* AnimationSpeed);
        }
    }
    #endregion
}
