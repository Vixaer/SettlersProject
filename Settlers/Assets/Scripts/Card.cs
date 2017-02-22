using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card {
		
}

public class ProgressCard : Card {
	private ProgressCardKind k;

	public ProgressCard(ProgressCardKind k){
		this.k = k;
	}
}

public class Stealable : Card {
	
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