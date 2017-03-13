using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardControl : MonoBehaviour{
    public Sprite[] ProgressCardSprites;
    private GameObject myClient;
    private Card myCard;
    public void Start()
    {
    }

    public void setCard(Card card)
    {
        myCard = card;
        transform.GetComponent<Image>().sprite = ProgressCardSprites[(int)myCard.k];
    }
}

public class Card
{
    public ProgressCardKind k { get; private set; }

    public Card(ProgressCardKind k)
    {
        this.k = k;
    }
     
}


