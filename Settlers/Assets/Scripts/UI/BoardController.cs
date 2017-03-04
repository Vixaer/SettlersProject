using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardController : MonoBehaviour {

	// Use this for initialization
	public GameObject hex;
	private GameObject hexHolder;
	private GameObject edgeHolder;
	private GameObject interHolder;
	private Vector3 myPos;
	public GameObject edge;
	public GameObject inter;

	private TerrainHex[,] CollectionOfHexes;
	private Edges[,] CollectionOfEdges;
	private Intersection[,] CollectionOfIntersections;

	private TerrainKind[,] hexTerrain3Players;
	private int[,] hexNumberTokens3Players;
	private HarbourKind[,] intersectionHarbours3Players;

	public Material seaMaterial;
	public Material hillMaterial;
	public Material forestMaterial;
	public Material mountainMaterial;
	public Material fieldMaterial;
	public Material pastureMaterial;
	public Material desertMaterial;
	public Material goldMaterial;

	void Start () {
		Initialize3PlayerBoard();
		Create3PlayerBoard();
		SetNeighbours3Players();

		hexHolder.transform.Rotate(0,0,30);
		edgeHolder.transform.Rotate(0,0,30);
		interHolder.transform.Rotate(0,0,30);
		hexHolder.transform.position += new Vector3(150.0f, -150.0f, 0.0f);
		edgeHolder.transform.position += new Vector3(150.0f, -150.0f, 0.0f);
		interHolder.transform.position += new Vector3(150.0f, -150.0f, 0.0f);	
	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void Initialize3PlayerBoard() {
		//Through the Desert 3 Players
		hexTerrain3Players = new TerrainKind[7,7]{{TerrainKind.Sea, TerrainKind.Sea, TerrainKind.Sea, TerrainKind.Pasture, TerrainKind.Mountains, TerrainKind.Forest, TerrainKind.Sea},
		{TerrainKind.Sea, TerrainKind.Sea, TerrainKind.Pasture, TerrainKind.Fields, TerrainKind.Hills, TerrainKind.Sea, TerrainKind.Fields},
		{TerrainKind.Sea, TerrainKind.Sea, TerrainKind.Forest, TerrainKind.Fields, TerrainKind.Mountains, TerrainKind.Desert, TerrainKind.Forest},
		{TerrainKind.Mountains, TerrainKind.Sea, TerrainKind.Hills, TerrainKind.Forest, TerrainKind.Hills, TerrainKind.Desert, TerrainKind.GoldMine},
		{TerrainKind.Fields, TerrainKind.Sea, TerrainKind.Sea, TerrainKind.Pasture, TerrainKind.Forest, TerrainKind.Desert, TerrainKind.Sea},
		{TerrainKind.Sea, TerrainKind.GoldMine, TerrainKind.Sea, TerrainKind.Sea, TerrainKind.Sea, TerrainKind.Sea, TerrainKind.Sea},
		{TerrainKind.Sea, TerrainKind.Sea, TerrainKind.Pasture, TerrainKind.Mountains, TerrainKind.Sea, TerrainKind.Sea, TerrainKind.Sea}};

		hexNumberTokens3Players = new int[7,7]{{-1, -1, -1, 8, 10, 11, 0},
		{-1, -1, 4, 9, 6, 0, 6},
		{-1, 0, 8, 2, 3, 0, 3},
		{5, 0, 9, 10, 5, 0, 4},
		{9, 0, 0, 6, 4, 0, -1},
		{0, 5, 0, 0, 0, -1, -1},
		{0, 0, 11, 8, -1, -1, -1}};

		intersectionHarbours3Players = new HarbourKind[16,8]{
		{HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None},
		{HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None},
		{HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None},
		{HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None},
		{HarbourKind.None, HarbourKind.None, HarbourKind.Generic, HarbourKind.None, HarbourKind.Generic, HarbourKind.None, HarbourKind.None, HarbourKind.None},
		{HarbourKind.None, HarbourKind.None, HarbourKind.Generic, HarbourKind.Generic, HarbourKind.Generic, HarbourKind.None, HarbourKind.None, HarbourKind.None},
		{HarbourKind.None, HarbourKind.SpecialBrick, HarbourKind.None, HarbourKind.None, HarbourKind.Generic, HarbourKind.None, HarbourKind.None, HarbourKind.None},
		{HarbourKind.SpecialBrick, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None},
		{HarbourKind.SpecialOre, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None},
		{HarbourKind.SpecialOre, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.SpecialLumber, HarbourKind.None, HarbourKind.None},
		{HarbourKind.None, HarbourKind.None, HarbourKind.SpecialGrain, HarbourKind.None, HarbourKind.None, HarbourKind.SpecialLumber, HarbourKind.None, HarbourKind.None},
		{HarbourKind.SpecialWool, HarbourKind.SpecialGrain, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None},
		{HarbourKind.SpecialWool, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None},
		{HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None},
		{HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None},
		{HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None, HarbourKind.None}};

		CollectionOfHexes = new TerrainHex[7,7]; //offsets: x: +3, y: + 3
		CollectionOfEdges = new Edges[15,15]; //offsets: x: +3, y: + 11
		CollectionOfIntersections = new Intersection[16,8]; //offsets: x: +3, y: + 4

	}

	void Create3PlayerBoard() {
		hexHolder = new GameObject();
		hexHolder.name = "testHexes";
		hexHolder.transform.parent = transform;
		hexHolder.transform.localScale = new Vector3(1.0f,1.0f,1.0f);
	
		edgeHolder = new GameObject();
		edgeHolder.name = "testEdges";
		edgeHolder.transform.parent = transform;
		edgeHolder.transform.localScale = new Vector3(1.0f,1.0f,1.0f);	

		interHolder = new GameObject();
		interHolder.name = "testIntersections";
		interHolder.transform.parent = transform;
		interHolder.transform.localScale = new Vector3(1.0f,1.0f,1.0f);
		
		int[] xHexValuesStart = new int [] {-3,-3,-3,-3,-2,-1,0};
		int[] xHexValuesEnd = new int [] {0,1,2,3,3,3,3};
		float[] xHexOffset = new float [] {0.0f, -150.0f, -300.0f,
		-450.0f, -600.0f, -750.0f, -900.0f};

		for (int i = 0; i < 7; i++){
			for (int j = xHexValuesStart[i]; j <= xHexValuesEnd[i]; j++){
				
				// Hex
				GameObject tempHex;
				myPos = new Vector3 (600.0f + 300.0f*(j) + xHexOffset[i], -600.0f + 257.5f*(i), 0.0f);
				tempHex = Instantiate(hex, myPos, Quaternion.identity) as GameObject;
				tempHex.transform.parent = hexHolder.transform;
				tempHex.transform.localScale = new Vector3(1.0f,1.0f,1.0f);
				int yAdjusted = -(i - 3);	
				tempHex.name = "hex_" + j + "_" + yAdjusted;
				TerrainHex hexToAdd = tempHex.GetComponent("TerrainHex") as TerrainHex;
				hexToAdd.initialize(j, yAdjusted, hexTerrain3Players[yAdjusted+3, j+3], hexNumberTokens3Players[yAdjusted+3, j+3]);
				Renderer rend = tempHex.GetComponent<Renderer>();
				rend.material = getHexMaterial(hexToAdd);
				CollectionOfHexes[j+3, yAdjusted+3] = hexToAdd;

				// Bottom edges of hex
				GameObject tempEdge1;
				GameObject tempEdge2;
				Vector3 edge1Pos = new Vector3(600.0f + 300.0f*(j) + xHexOffset[i] - 0.45f*(150.0f), -600.0f + 257.5f*(i) - 0.85f*(150.0f), -5.0f);
				Vector3 edge2Pos = new Vector3(600.0f + 300.0f*(j) + xHexOffset[i] + 0.45f*(150.0f), -600.0f + 257.5f*(i) - 0.85f*(150.0f), -5.0f);
				tempEdge1 = Instantiate(edge, edge1Pos, Quaternion.Euler(0,0,60)) as GameObject;
				tempEdge2 = Instantiate(edge, edge2Pos, Quaternion.Euler(0,0,-60)) as GameObject;
				tempEdge1.transform.parent = edgeHolder.transform;
				tempEdge2.transform.parent = edgeHolder.transform;
				tempEdge1.transform.localScale = new Vector3(0.8f,0.7f,1.0f);
				tempEdge2.transform.localScale = new Vector3(0.8f,0.7f,1.0f);
				tempEdge1.name = "edge_" + (2*j+3) + "_" + (2*yAdjusted-3);
				tempEdge2.name = "edge_" + (2*j+4) + "_" + (2*yAdjusted-3);
				Edges edgeToAdd1 = tempEdge1.GetComponent("Edges") as Edges;
				Edges edgeToAdd2 = tempEdge2.GetComponent("Edges") as Edges;
				edgeToAdd1.initialize((2*j+3), (2*yAdjusted-3));
				edgeToAdd2.initialize((2*j+4), (2*yAdjusted-3));
				CollectionOfEdges[(2*j+3)+3, (2*yAdjusted-3)+11] = edgeToAdd1;
				CollectionOfEdges[(2*j+4)+3, (2*yAdjusted-3)+11] = edgeToAdd2;

				// left edge of hex
				GameObject tempEdge3;			
				Vector3 edge3Pos = new Vector3(600.0f + 300.0f*(j) + xHexOffset[i] - 1.0f*(150.0f), -600.0f + 257.5f*(i), -5.0f);
				tempEdge3 = Instantiate(edge, edge3Pos, Quaternion.identity) as GameObject;
				tempEdge3.transform.parent = edgeHolder.transform;
				tempEdge3.transform.localScale = new Vector3(0.8f,0.7f,1.0f);
				tempEdge3.name = "edge_" + (2*j+3) + "_" + (2*yAdjusted-4);
				Edges edgeToAdd3 = tempEdge3.GetComponent("Edges") as Edges;
				edgeToAdd3.initialize((2*j+3), (2*yAdjusted-4));
				CollectionOfEdges[(2*j+3)+3, (2*yAdjusted-4)+11] = edgeToAdd3;

				//Bottom 2 intersections
				GameObject tempInter1;
				GameObject tempInter2;
				Vector3 inter1Pos = new Vector3(600.0f + 300.0f*(j) + xHexOffset[i] - (150.0f), -600.0f + 257.5f*(i) - 0.5f*(150.0f), -10.0f);
				Vector3 inter2Pos = new Vector3(600.0f + 300.0f*(j) + xHexOffset[i], -600.0f + 257.5f*(i) -  1.075f*150.0f, -10.0f);
				tempInter1 = Instantiate(inter, inter1Pos, Quaternion.identity) as GameObject;
				tempInter2 = Instantiate(inter, inter2Pos, Quaternion.identity) as GameObject;
				tempInter1.transform.parent = interHolder.transform;
				tempInter2.transform.parent = interHolder.transform;
				tempInter1.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
				tempInter2.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
				tempInter1.name = "intersection_" + (2*j+3) + "_" + yAdjusted;
				tempInter2.name = "intersection_" + (2*j+4) + "_" + yAdjusted;
				Intersection interToAdd1 = tempInter1.GetComponent("Intersection") as Intersection;
				Intersection interToAdd2 = tempInter2.GetComponent("Intersection") as Intersection;
				interToAdd1.initialize((2*j+3), yAdjusted, intersectionHarbours3Players[(2*j+3)+3, yAdjusted+4]);
				interToAdd2.initialize((2*j+4), yAdjusted, intersectionHarbours3Players[(2*j+4)+3, yAdjusted+4]);
				CollectionOfIntersections[(2*j+3)+3, yAdjusted+4] = interToAdd1;
				CollectionOfIntersections[(2*j+4)+3, yAdjusted+4] = interToAdd2;

				// right edge and bottom right intersection of hex if last hex in line
				if (j == xHexValuesEnd[i]){
					GameObject tempEdge4;
					Vector3 edge4Pos = new Vector3(600.0f + 300.0f*(j) + xHexOffset[i] + 1.0f*(150.0f), -600.0f + 257.5f*(i), -5.0f);
					tempEdge4 = Instantiate(edge, edge4Pos, Quaternion.identity) as GameObject;
					tempEdge4.transform.parent = edgeHolder.transform;
					tempEdge4.transform.localScale = new Vector3(0.8f,0.7f,1.0f);
					tempEdge4.name = "edge_" + (2*j+5) + "_" + (2*yAdjusted-4);
					Edges edgeToAdd4 = tempEdge4.GetComponent("Edges") as Edges;
					edgeToAdd4.initialize((2*j+5), (2*yAdjusted-4));
					CollectionOfEdges[(2*j+5)+3, (2*yAdjusted-4)+11] = edgeToAdd4;

					GameObject tempInter3;
					Vector3 inter3Pos = new Vector3(600.0f + 300.0f*(j) + xHexOffset[i] + (150.0f), -600.0f + 257.5f*(i) - 0.5f*(150.0f), -10.0f);
					tempInter3 = Instantiate(inter, inter3Pos, Quaternion.identity) as GameObject;
					tempInter3.transform.parent = interHolder.transform;
					tempInter3.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
					tempInter3.name = "intersection_" + (2*j+5) + "_" + yAdjusted;
					Intersection interToAdd3 = tempInter3.GetComponent("Intersection") as Intersection;
					interToAdd3.initialize((2*j+5), yAdjusted, intersectionHarbours3Players[ (2*j+5)+3, yAdjusted+4]);
					CollectionOfIntersections[(2*j+5)+3, yAdjusted+4] = interToAdd3;

				}
			
				// if at last row of hexes, add edges on top. Also add top left and top centre intersections
				if (i == 6){
					GameObject tempEdge5;
					GameObject tempEdge6;
					Vector3 edge5Pos = new Vector3(600.0f + 300.0f*(j) + xHexOffset[i] - 0.45f*(150.0f), -600.0f + 257.5f*(i) + 0.85f*(150.0f), -5.0f);
					Vector3 edge6Pos = new Vector3(600.0f + 300.0f*(j) + xHexOffset[i] + 0.45f*(150.0f), -600.0f + 257.5f*(i) + 0.85f*(150.0f), -5.0f);
					tempEdge5 = Instantiate(edge, edge5Pos, Quaternion.Euler(0,0,-60)) as GameObject;
					tempEdge6 = Instantiate(edge, edge6Pos, Quaternion.Euler(0,0,60)) as GameObject;
					tempEdge5.transform.parent = edgeHolder.transform;
					tempEdge6.transform.parent = edgeHolder.transform;
					tempEdge5.transform.localScale = new Vector3(0.8f,0.7f,1.0f);
					tempEdge6.transform.localScale = new Vector3(0.8f,0.7f,1.0f);
					tempEdge5.name = "edge_" + (2*j+4) + "_" + (2*yAdjusted-5);
					tempEdge6.name = "edge_" + (2*j+5) + "_" + (2*yAdjusted-5);
					Edges edgeToAdd5 = tempEdge5.GetComponent("Edges") as Edges;
					edgeToAdd5.initialize((2*j+4), (2*yAdjusted-5));
					Edges edgeToAdd6 = tempEdge6.GetComponent("Edges") as Edges;
					edgeToAdd6.initialize((2*j+5), (2*yAdjusted-5));
					CollectionOfEdges[(2*j+4)+3, (2*yAdjusted-5)+11] = edgeToAdd5;
					CollectionOfEdges[(2*j+5)+3, (2*yAdjusted-5)+11] = edgeToAdd6;

					GameObject tempInter4;
					GameObject tempInter5;
					Vector3 inter4Pos = new Vector3(600.0f + 300.0f*(j) + xHexOffset[i] - (150.0f), -600.0f + 257.5f*(i) + 0.5f*(150.0f), -10.0f);
					Vector3 inter5Pos = new Vector3(600.0f + 300.0f*(j) + xHexOffset[i], -600.0f + 257.5f*(i) + 1.075f*150.0f, -10.0f);
					tempInter4 = Instantiate(inter, inter4Pos, Quaternion.identity) as GameObject;
					tempInter5 = Instantiate(inter, inter5Pos, Quaternion.identity) as GameObject;
					tempInter4.transform.parent = interHolder.transform;
					tempInter5.transform.parent = interHolder.transform;
					tempInter4.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
					tempInter5.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
					tempInter4.name = "intersection_" + (2*j+4) + "_" +  (yAdjusted-1);
					tempInter5.name = "intersection_" + (2*j+5) + "_" +  (yAdjusted-1);
					Intersection interToAdd4 = tempInter4.GetComponent("Intersection") as Intersection;
					Intersection interToAdd5 = tempInter5.GetComponent("Intersection") as Intersection;
					interToAdd4.initialize((2*j+4), (yAdjusted - 1), intersectionHarbours3Players[(2*j+4)+3, (yAdjusted - 1)+4]);
					interToAdd5.initialize((2*j+5), (yAdjusted - 1), intersectionHarbours3Players[(2*j+5)+3, (yAdjusted - 1)+4]);
					CollectionOfIntersections[(2*j+4)+3, (yAdjusted-1)+4] = interToAdd4;
					CollectionOfIntersections[(2*j+5)+3, (yAdjusted-1)+4] = interToAdd5;

					//if also at last hex of last row add top right intersection
					if (j == xHexValuesEnd[i]){
						GameObject tempInter6;
						Vector3 inter6Pos = new Vector3(600.0f + 300.0f*(j) + xHexOffset[i] + (150.0f), -600.0f + 257.5f*(i) + 0.5f*(150.0f), -10.0f);
						tempInter6 = Instantiate(inter, inter6Pos, Quaternion.identity) as GameObject;
						tempInter6.transform.parent = interHolder.transform;
						tempInter6.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
						tempInter6.name = "intersection_" + (2*j+6) + "_" + (yAdjusted-1);
						Intersection interToAdd6 = tempInter6.GetComponent("Intersection") as Intersection;
						interToAdd6.initialize((2*j+6), (yAdjusted - 1), intersectionHarbours3Players[(2*j+6)+3, (yAdjusted - 1)+4]);
						CollectionOfIntersections[(2*j+6)+3, (yAdjusted-1)+4] = interToAdd6;
					}
				}
				// else if no hexes on top, then add proper edge and intersection on top
				else if (xHexValuesStart[i] < xHexValuesStart[i+1]){
					// left top edge and left top intersection
					if (j == xHexValuesStart[i]){
						GameObject tempEdge5;
						Vector3 edge5Pos = new Vector3(600.0f + 300.0f*(j) + xHexOffset[i] - 0.45f*(150.0f), -600.0f + 257.5f*(i) + 0.85f*(150.0f), -5.0f);
						tempEdge5 = Instantiate(edge, edge5Pos, Quaternion.Euler(0,0,-60)) as GameObject;
						tempEdge5.transform.parent = edgeHolder.transform;
						tempEdge5.transform.localScale = new Vector3(0.8f,0.7f,1.0f);
						tempEdge5.name = "edge_" + (2*j+4) + "_" + (2*yAdjusted-5);
						Edges edgeToAdd5 = tempEdge5.GetComponent("Edges") as Edges;
						edgeToAdd5.initialize((2*j+4), (2*yAdjusted-5));
						CollectionOfEdges[(2*j+4)+3, (2*yAdjusted-5)+11] = edgeToAdd5;		

						GameObject tempInter4;
						Vector3 inter4Pos = new Vector3(600.0f + 300.0f*(j) + xHexOffset[i] - (150.0f), -600.0f + 257.5f*(i) + 0.5f*(150.0f), -10.0f);
						tempInter4 = Instantiate(inter, inter4Pos, Quaternion.identity) as GameObject;
						tempInter4.transform.parent = interHolder.transform;
						tempInter4.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
						tempInter4.name = "intersection_" + (2*j+4) + "_" +  (yAdjusted-1);
						Intersection interToAdd4 = tempInter4.GetComponent("Intersection") as Intersection;
						interToAdd4.initialize((2*j+4), (yAdjusted - 1), intersectionHarbours3Players[(2*j+4)+3, (yAdjusted - 1)+4]);
						CollectionOfIntersections[(2*j+4)+3, (yAdjusted-1)+4] = interToAdd4;					
					}
					// right top edge and right top intersection
					else if (j == xHexValuesEnd[i]){
						GameObject tempEdge6;
						Vector3 edge6Pos = new Vector3(600.0f + 300.0f*(j) + xHexOffset[i] + 0.45f*(150.0f), -600.0f + 257.5f*(i) + 0.85f*(150.0f), -5.0f);
						tempEdge6 = Instantiate(edge, edge6Pos, Quaternion.Euler(0,0,60)) as GameObject;
						tempEdge6.transform.parent = edgeHolder.transform;
						tempEdge6.transform.localScale = new Vector3(0.8f,0.7f,1.0f);
						tempEdge6.name = "edge_" + (2*j+5) + "_" + (2*yAdjusted-5);
						Edges edgeToAdd6 = tempEdge6.GetComponent("Edges") as Edges;
						edgeToAdd6.initialize((2*j+5), (2*yAdjusted-5));
						CollectionOfEdges[(2*j+5)+3, (2*yAdjusted-5)+11] = edgeToAdd6;

						GameObject tempInter6;
						Vector3 inter6Pos = new Vector3(600.0f + 300.0f*(j) + xHexOffset[i] + (150.0f), -600.0f + 257.5f*(i) + 0.5f*(150.0f), -10.0f);
						tempInter6 = Instantiate(inter, inter6Pos, Quaternion.identity) as GameObject;
						tempInter6.transform.parent = interHolder.transform;
						tempInter6.transform.localScale = new Vector3(0.5f,0.5f,0.5f);
						tempInter6.name = "intersection_" + (2*j+6) + "_" + (yAdjusted-1);
						Intersection interToAdd6 = tempInter6.GetComponent("Intersection") as Intersection;
						interToAdd6.initialize((2*j+6), (yAdjusted - 1), intersectionHarbours3Players[(2*j+6)+3, (yAdjusted - 1)+4]);
						CollectionOfIntersections[(2*j+6)+3, (yAdjusted-1)+4] = interToAdd6;
					}
				}


			}

			
		}
	}

	Material getHexMaterial (TerrainHex h){
		switch (h.myTerrain) {
			case TerrainKind.Sea : return seaMaterial;
			case TerrainKind.Hills : return hillMaterial;
			case TerrainKind.Forest : return forestMaterial;
			case TerrainKind.Mountains : return mountainMaterial;
			case TerrainKind.Fields : return fieldMaterial;
			case TerrainKind.Pasture : return pastureMaterial;
			case TerrainKind.Desert : return desertMaterial;
			case TerrainKind.GoldMine : return goldMaterial;
			default: return seaMaterial;
		}	
	}

	void SetNeighbours3Players(){
		int[] xHexValuesStart = new int [] {-3,-3,-3,-3,-2,-1,0};
		int[] xHexValuesEnd = new int [] {0,1,2,3,3,3,3};

		for (int i = 0; i < 7; i++){
			for (int j = xHexValuesStart[i]; j <= xHexValuesEnd[i]; j++){
				
				//Neighbour intersections of hexes

				int yAdjusted = -(i - 3);
				Intersection[] hexInterNeighbours = new Intersection[6]{CollectionOfIntersections[(2*j+3)+3,yAdjusted+4],
				CollectionOfIntersections[(2*j+4)+3,yAdjusted+4],
				CollectionOfIntersections[(2*j+5)+3,yAdjusted+4],
				CollectionOfIntersections[(2*j+4)+3,(yAdjusted-1)+4],
				CollectionOfIntersections[(2*j+5)+3,(yAdjusted-1)+4],
				CollectionOfIntersections[(2*j+6)+3,(yAdjusted-1)+4]};

				CollectionOfHexes[j+3, yAdjusted+3].setInterNeighbours(hexInterNeighbours);

			
				// Bottom edges of hex
				
				CollectionOfEdges[(2*j+3)+3, (2*yAdjusted-3)+11].setHexNeighbours(neighbourHexesOfEdges((2*j+3), (2*yAdjusted-3)));
				CollectionOfEdges[(2*j+4)+3, (2*yAdjusted-3)+11].setHexNeighbours(neighbourHexesOfEdges((2*j+4), (2*yAdjusted-3)));
				CollectionOfEdges[(2*j+3)+3, (2*yAdjusted-3)+11].setIntersectionNeighbours(neighbourIntersectionsOfEdges((2*j+3), (2*yAdjusted-3)));
				CollectionOfEdges[(2*j+4)+3, (2*yAdjusted-3)+11].setIntersectionNeighbours(neighbourIntersectionsOfEdges((2*j+4), (2*yAdjusted-3)));

				// left edge of hex
				
				CollectionOfEdges[(2*j+3)+3, (2*yAdjusted-4)+11].setHexNeighbours(neighbourHexesOfEdges((2*j+3), (2*yAdjusted-4)));
				CollectionOfEdges[(2*j+3)+3, (2*yAdjusted-4)+11].setIntersectionNeighbours(neighbourIntersectionsOfEdges((2*j+3), (2*yAdjusted-4)));

				//Bottom 2 intersections
				
				CollectionOfIntersections[(2*j+3)+3, (yAdjusted)+4].setHexNeighbours(neighbourHexesOfIntersections((2*j+3), (yAdjusted)));
				CollectionOfIntersections[(2*j+4)+3, (yAdjusted)+4].setHexNeighbours(neighbourHexesOfIntersections((2*j+4), (yAdjusted)));
				CollectionOfIntersections[(2*j+3)+3, (yAdjusted)+4].setEdgeNeighbours(neighbourEdgesOfIntersections((2*j+3), (yAdjusted)));
				CollectionOfIntersections[(2*j+4)+3, (yAdjusted)+4].setEdgeNeighbours(neighbourEdgesOfIntersections((2*j+4), (yAdjusted)));

				// right edge and bottom right intersection of hex if last hex in line
				if (j == xHexValuesEnd[i]){
					
					CollectionOfEdges[(2*j+5)+3, (2*yAdjusted-4)+11].setHexNeighbours(neighbourHexesOfEdges((2*j+5), (2*yAdjusted-4)));
					CollectionOfEdges[(2*j+5)+3, (2*yAdjusted-4)+11].setIntersectionNeighbours(neighbourIntersectionsOfEdges((2*j+5), (2*yAdjusted-4)));

					CollectionOfIntersections[(2*j+5)+3, (yAdjusted)+4].setHexNeighbours(neighbourHexesOfIntersections((2*j+5), (yAdjusted)));
					CollectionOfIntersections[(2*j+5)+3, (yAdjusted)+4].setEdgeNeighbours(neighbourEdgesOfIntersections((2*j+5), (yAdjusted)));

				}
			
				// if at last row of hexes, add edges on top. Also add top left and top centre intersections
				if (i == 6){
					
					CollectionOfEdges[(2*j+4)+3, (2*yAdjusted-5)+11].setHexNeighbours(neighbourHexesOfEdges((2*j+4), (2*yAdjusted-5)));
					CollectionOfEdges[(2*j+5)+3, (2*yAdjusted-5)+11].setHexNeighbours(neighbourHexesOfEdges((2*j+5), (2*yAdjusted-5)));
					CollectionOfEdges[(2*j+4)+3, (2*yAdjusted-5)+11].setIntersectionNeighbours(neighbourIntersectionsOfEdges((2*j+4), (2*yAdjusted-5)));
					CollectionOfEdges[(2*j+5)+3, (2*yAdjusted-5)+11].setIntersectionNeighbours(neighbourIntersectionsOfEdges((2*j+5), (2*yAdjusted-5)));
					
					CollectionOfIntersections[(2*j+4)+3, (yAdjusted-1)+4].setHexNeighbours(neighbourHexesOfIntersections((2*j+4), (yAdjusted-1)));
					CollectionOfIntersections[(2*j+5)+3, (yAdjusted-1)+4].setHexNeighbours(neighbourHexesOfIntersections((2*j+5), (yAdjusted-1)));
					CollectionOfIntersections[(2*j+4)+3, (yAdjusted-1)+4].setEdgeNeighbours(neighbourEdgesOfIntersections((2*j+4), (yAdjusted-1)));
					CollectionOfIntersections[(2*j+5)+3, (yAdjusted-1)+4].setEdgeNeighbours(neighbourEdgesOfIntersections((2*j+5), (yAdjusted-1)));

					//if also at last hex of last row add top right intersection
					if (j == xHexValuesEnd[i]){
						
						CollectionOfIntersections[(2*j+6)+3, (yAdjusted-1)+4].setHexNeighbours(neighbourHexesOfIntersections((2*j+6), (yAdjusted-1)));
					}
				}
				// else if no hexes on top, then add proper edge and intersection on top
				else if (xHexValuesStart[i] < xHexValuesStart[i+1]){
					// left top edge and left top intersection
					if (j == xHexValuesStart[i]){

						CollectionOfEdges[(2*j+4)+3, (2*yAdjusted-5)+11].setHexNeighbours(neighbourHexesOfEdges((2*j+4), (2*yAdjusted-5)));
						CollectionOfEdges[(2*j+4)+3, (2*yAdjusted-5)+11].setIntersectionNeighbours(neighbourIntersectionsOfEdges((2*j+4), (2*yAdjusted-5)));

						CollectionOfIntersections[(2*j+4)+3, (yAdjusted-1)+4].setHexNeighbours(neighbourHexesOfIntersections((2*j+4), (yAdjusted-1)));
						CollectionOfIntersections[(2*j+4)+3, (yAdjusted-1)+4].setEdgeNeighbours(neighbourEdgesOfIntersections((2*j+4), (yAdjusted-1)));						
					}
					// right top edge and right top intersection
					else if (j == xHexValuesEnd[i]){
						
						CollectionOfEdges[(2*j+5)+3, (2*yAdjusted-5)+11].setHexNeighbours(neighbourHexesOfEdges((2*j+5), (2*yAdjusted-5)));
						CollectionOfEdges[(2*j+5)+3, (2*yAdjusted-5)+11].setIntersectionNeighbours(neighbourIntersectionsOfEdges((2*j+5), (2*yAdjusted-5)));

						CollectionOfIntersections[(2*j+6)+3, (yAdjusted-1)+4].setHexNeighbours(neighbourHexesOfIntersections((2*j+6), (yAdjusted-1)));
						CollectionOfIntersections[(2*j+4)+3, (yAdjusted-1)+4].setEdgeNeighbours(neighbourEdgesOfIntersections((2*j+4), (yAdjusted-1)));	
					}
				}

			}
		}
	}


	TerrainHex[] neighbourHexesOfEdges(int x, int y){
		ArrayList list = new ArrayList();

		if ((y+100)%2 == 0) {
			int leftX = x - (x+5)/2;
			int rightX = x - (x+5)/2 +1;
			int leftAndRightY = y - (y-4)/2;
			
			if (isValidHex3Player(leftX, leftAndRightY)) list.Add(CollectionOfHexes[leftX + 3, leftAndRightY + 3]);
			if (isValidHex3Player(rightX, leftAndRightY)) list.Add(CollectionOfHexes[rightX + 3, leftAndRightY + 3]);

		}
		else {
			if ((x+100)%2 == 1){
				int leftX = x - (x+5)/2;
				int rightX = x - (x+5)/2 +1;
				int leftY = y - (y-5)/2;
				int rightY = y - (y-5)/2 -1;

				if (isValidHex3Player(leftX, leftY)) list.Add(CollectionOfHexes[leftX + 3, leftY + 3]);
				if (isValidHex3Player(rightX, rightY)) list.Add(CollectionOfHexes[rightX + 3, rightY + 3]);

			}
			else {
				int leftAndRightX = x - (x+4)/2;
				int leftY = y - (y-5)/2;
				int rightY = y - (y-5)/2 -1;

				if (isValidHex3Player(leftAndRightX, leftY)) list.Add(CollectionOfHexes[leftAndRightX + 3, leftY + 3]);
				if (isValidHex3Player(leftAndRightX, rightY)) list.Add(CollectionOfHexes[leftAndRightX + 3, rightY + 3]);

			}
		}

		TerrainHex[] toReturn = list.ToArray(typeof(TerrainHex)) as TerrainHex[];
		return toReturn;
	}

	Intersection[] neighbourIntersectionsOfEdges(int x, int y){
		Intersection[] temp;
		if ((y+100)%2 == 1){
			temp = new Intersection[]{CollectionOfIntersections[x + 3, y-(y-3)/2 + 4],CollectionOfIntersections[x+1 + 3, y-(y-3)/2 + 4]};
		}
		else {
			temp = new Intersection[]{CollectionOfIntersections[x + 3, y-(y-2)/2 + 1 + 4],CollectionOfIntersections[x+1 + 3, y-(y-2)/2 + 4]};
		}
		return temp;
	}

	TerrainHex[] neighbourHexesOfIntersections(int x, int y){
		ArrayList list = new ArrayList();
		if ((x+100)%2 == 0){
			if (isValidHex3Player((x-4)/2, y)) list.Add(CollectionOfHexes[(x-4)/2 + 3, y + 3]);
			if (isValidHex3Player((x-6)/2, y+1)) list.Add(CollectionOfHexes[(x-6)/2 + 3, y+1 + 3]);
			if (isValidHex3Player((x-4)/2, y+1)) list.Add(CollectionOfHexes[(x-4)/2 + 3, y+1 + 3]);
		}
		else {
			if (isValidHex3Player((x-5)/2, y+1)) list.Add(CollectionOfHexes[(x-5)/2 + 3, y+1 + 3]);
			if (isValidHex3Player((x-3)/2, y)) list.Add(CollectionOfHexes[(x-3)/2 + 3, y + 3]);
			if (isValidHex3Player((x-5)/2, y)) list.Add(CollectionOfHexes[(x-5)/2 + 3, y + 3]);
		}
		TerrainHex[] toReturn = list.ToArray(typeof(TerrainHex)) as TerrainHex[];
		return toReturn;
	}

	Edges[] neighbourEdgesOfIntersections(int x, int y){
		ArrayList list = new ArrayList();
		if ((x+100)%2 == 0){
			if (isValidEdge3Player(x-1, y + (y-2)-1)) list.Add(CollectionOfEdges[x-1 + 3, y + (y-2)-1 + 11]);
			if (isValidEdge3Player(x, y + (y-2)-1)) list.Add(CollectionOfEdges[x + 3, y + (y-2)-1 + 11]);
			if (isValidEdge3Player(x-1, y + (y-2))) list.Add(CollectionOfEdges[x-1 + 3, y + (y-2) + 11]);
		}
		else {
			if (isValidEdge3Player(x, y + (y-3))) list.Add(CollectionOfEdges[x + 3, y + (y-3) + 11]);
			if (isValidEdge3Player(x-1, y + (y-3))) list.Add(CollectionOfEdges[x-1 + 3, y + (y-3) + 11]);
			if (isValidEdge3Player(x, y + (y-3)-1)) list.Add(CollectionOfEdges[x + 3, y + (y-3) -1 + 11]);
		}
		Edges[] toReturn = list.ToArray(typeof(Edges)) as Edges[];
		return toReturn;
	}

	bool isValidHex3Player(int x, int y){
		if ((x + y) < -3 || (x + y) > 3 || x < -3 || x > 3 || y < -3 || y > 3){
			return false;
		}
		else {
			return true;
		}
	}

	bool isValidEdge3Player(int x, int y){

		if ((y+100)%2 == 0) {
			int leftX = x - (x+5)/2;
			int rightX = x - (x+5)/2 +1;
			int leftAndRightY = y - (y-4)/2;

			if (isValidHex3Player(leftX, leftAndRightY) || isValidHex3Player(rightX, leftAndRightY)) return true;
			else return false;
		}
		else {
			if ((x+100)%2 == 1){
				int leftX = x - (x+5)/2;
				int rightX = x - (x+5)/2 +1;
				int leftY = y - (y-5)/2;
				int rightY = y - (y-5)/2 -1;

				if (isValidHex3Player(leftX, leftY) || isValidHex3Player(rightX, rightY)) return true;
				else return false;

			}
			else {
				int leftAndRightX = x - (x+4)/2;
				int leftY = y - (y-5)/2;
				int rightY = y - (y-5)/2 -1;

				if (isValidHex3Player(leftAndRightX, leftY) || isValidHex3Player(leftAndRightX, rightY)) return true;
				else return false;

			}
		}
	}



	
}
