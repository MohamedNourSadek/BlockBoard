using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum Rank { Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten, J, Q, K, A}
public enum type { Heart, Club, Diamond, Spades}


public class Play_Card : MonoBehaviour
{
    public Rank card_Rank;
    public type card_Type;
}
