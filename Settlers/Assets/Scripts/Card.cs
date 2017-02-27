using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card {
	private CardType  cardType;
}

public class CardType{
	
}

public class ProgressCard : CardType {
	private ProgressCardKind k;

	public ProgressCard(ProgressCardKind k){
		this.k = k;
	}
}

public class Stealable : CardType {
	
}


public class Commodity : Stealable {
	private CommodityKind k;

	public Commodity(CommodityKind k){
		this.k = k;
	}
}

public class Resource : Stealable {
	private ResourceKind k;

	public Resource(ResourceKind k){
		this.k = k;
	}
}