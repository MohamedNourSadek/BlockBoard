using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.GPUSort;

public enum GroundSide { Center, Left, Right }

public class DominoGeometery : MonoBehaviour
{
    public static DominoGeometery Instance;

    [Header("Ground Positioning Constants")]
    [SerializeField] float NormalSpacing = 0.088f;
    [SerializeField] float DoubleSpacing = 0.068f;
    [SerializeField] float MaxMargin = 0.14f;
    [SerializeField] Vector2 SwitchFixIsLastDouble;
    [SerializeField] Vector2 SwitchFixAmIDouble;
    [SerializeField] Vector2 SwitchFixNoDouble;
    [SerializeField] float AnimationSpeed = 0.1f;

    [Header("Ground Positioning Objects")]
    [SerializeField] GameObject CenterPosition;
    [SerializeField] GameObject LeftPosition;
    [SerializeField] GameObject RightPosition;
    [SerializeField] GameObject OutObjects;

    [Header("Other")]
    [SerializeField] public List<DominoTile> DominoCards = new List<DominoTile>();
    [SerializeField] LayerMask Bounds;

    [NonSerialized] public DominoTile CenterCard;
    [NonSerialized] public List<DominoTile> LeftCards = new List<DominoTile>();
    [NonSerialized] public List<DominoTile> RightCards = new List<DominoTile>();
    [NonSerialized] public TileName LeftAvailable;
    [NonSerialized] public TileName RightAvailable;


    public DominoGeometery()
    {
        Instance = this;
    }


    #region Public Functions
    public void PlayCardOnGround(DominoTile tile, GroundSide groundSide, CardSide cardSide)
    {
        bool switchDirections = ShouldSwitchDirection(groundSide);

        if (switchDirections)
            SwitchDirections(groundSide);

        SetNextAvailableSides(tile, groundSide, cardSide);
        SetTileRotation(tile, groundSide, cardSide, switchDirections);
        SetTileFinalPosition(tile, groundSide, switchDirections);

        if (groundSide == GroundSide.Center)
        {
            CenterCard = tile;
        }
        else if (groundSide == GroundSide.Left)
        {
            LeftCards.Add(tile);
        }
        else if (groundSide == GroundSide.Right)
        {
            RightCards.Add(tile);
        }
    }
    public Vector3 GetNextTilePosition(GroundSide groundSide)
    {
        return GetTilePosition(false, groundSide, ShouldSwitchDirection(groundSide));
    }
    public void OrganizeCardsOutside(List<DominoTile> cards)
    {
        int increment = 0;

        foreach (DominoTile card in cards)
        {
            card.transform.position = OutObjects.transform.position + new Vector3(0.05f * increment, 0f, 0f);
            increment++;
        }
    }

    #endregion

    #region Private Functions
    private void SwitchDirections(GroundSide groundSide)
    {
        if (groundSide == GroundSide.Right)
            RightPosition.transform.Rotate(new Vector3(0f, 90f, 0f));
        else if (groundSide == GroundSide.Left)
            LeftPosition.transform.Rotate(new Vector3(0f, 90f, 0f));
    }
    private bool ShouldSwitchDirection(GroundSide groundSide)
    {
        DominoTile lastCard = GetLastCard(groundSide);

        if (groundSide == GroundSide.Right)
        {
            return ShouldSwitchDirection(lastCard, RightPosition.transform.forward);
        }
        else if (groundSide == GroundSide.Left)
        {
            return ShouldSwitchDirection(lastCard, -LeftPosition.transform.forward);
        }
        else if (groundSide == GroundSide.Center)
        {
            return false;
        }
        else
        {
            return false;
        }
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
        if (groundSide == GroundSide.Right)
        {
            return (RightCards.Count >= 1) ? RightCards[^1] : CenterCard;
        }
        else if (groundSide == GroundSide.Left)
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
        var finalPosition = GetTilePosition(tile.Double, groundSide, didSwitchDirection);
        tile.FinalPosition = finalPosition;
        StartCoroutine(MoveTileToPosition(tile));
    }
    private Vector3 GetTilePosition(bool isTileDouble, GroundSide groundSide, bool didSwitchDirection)
    {
        DominoTile lastCard = GetLastCard(groundSide);
        Vector3 position = new Vector3();

        if (groundSide == GroundSide.Center)
        {
            position = CenterPosition.transform.position;
        }
        else if (groundSide == GroundSide.Left)
        {
            if ((lastCard.Double) || isTileDouble)
                position = lastCard.FinalPosition - (LeftPosition.transform.forward * DoubleSpacing);
            else
                position = lastCard.FinalPosition - (LeftPosition.transform.forward * NormalSpacing);

            //Switching Fix
            if (didSwitchDirection)
            {
                if (lastCard.Double)
                {
                    position += SwitchFixIsLastDouble.x * LeftPosition.transform.forward;
                    position += SwitchFixIsLastDouble.y * LeftPosition.transform.right;
                }
                else if (isTileDouble)
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
            if ((lastCard.Double) || isTileDouble)
                position = lastCard.FinalPosition + (RightPosition.transform.forward * DoubleSpacing);
            else
                position = lastCard.FinalPosition + (RightPosition.transform.forward * NormalSpacing);

            //Switching Fix
            if (didSwitchDirection)
            {
                if (lastCard.Double)
                {
                    position -= SwitchFixIsLastDouble.x * RightPosition.transform.forward;
                    position -= SwitchFixIsLastDouble.y * RightPosition.transform.right;
                }
                else if (isTileDouble)
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
            myOjb.gameObject.transform.position = Vector3.Lerp(myOjb.transform.position, Point2, Time.fixedDeltaTime / AnimationSpeed);
            yield return new WaitForSeconds(Time.fixedDeltaTime * AnimationSpeed);
        }

        while ((myOjb.gameObject.transform.position - FinalPosition).magnitude >= 0.001f)
        {
            myOjb.gameObject.transform.position = Vector3.Lerp(myOjb.transform.position, FinalPosition, Time.fixedDeltaTime / AnimationSpeed);
            yield return new WaitForSeconds(Time.fixedDeltaTime * AnimationSpeed);
        }
    }
    #endregion
}
