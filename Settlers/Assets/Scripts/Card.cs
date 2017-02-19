using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : Object {
	
}

public class ProgressCard : Card {
	
}

public class Stealable : Card {
	private bool isResource(){
		return this.GetType() == typeof(Resource);
	}
		
}


public class Commodity : Stealable {

}

public class Resource : Stealable {

}