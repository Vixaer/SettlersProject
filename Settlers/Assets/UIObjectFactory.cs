using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIObjectFactory : MonoBehaviour {
    public Road roadPrefab;
    public Ship shipPrefab;
    public Village settlementPrefab;
    public Village cityPrefab;

    // Materials
    public Material red;
    public Material blue;
    public Material green;
    public Material orange;

    private Dictionary<int, Material> playerColors;

	// Use this for initialization
	void Start () {
        playerColors = new Dictionary<int, Material>
        {
            { 1, red },
            { 2, blue },
            { 3, green },
            { 4, orange }
        };	
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public Road CreateRoad(Transform parent, int playerRef)
    {
        var newObj = Instantiate(roadPrefab, parent);
        newObj.transform.localPosition = Vector3.zero;
        newObj.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
        newObj.GetComponent<Renderer>().material = playerColors[playerRef];
        return newObj;
    }

    public Ship CreateShip(Transform parent, int playerRef)
    {
        var newObj = Instantiate(shipPrefab, parent);
        newObj.transform.localPosition = Vector3.zero;
        newObj.transform.localRotation = Quaternion.Euler(new Vector3(0, 0, 90));
        // Set both children's materials
        foreach (Transform t in newObj.transform)
        {
            t.gameObject.GetComponent<Renderer>().material = playerColors[playerRef];
        }
        return newObj;
    }

    public Village CreateSettlement(Transform parent, int playerRef)
    {
        var newObj = Instantiate(settlementPrefab, parent);
        newObj.transform.localPosition = Vector3.zero;
        newObj.GetComponent<Renderer>().material = playerColors[playerRef];
        newObj.tag = "Settlement";
        return newObj;
    }

    public Village CreateCity(Transform parent, int playerRef)
    {
        var newObj = Instantiate(cityPrefab, parent);
        newObj.transform.localPosition = Vector3.zero;
        newObj.GetComponent<Renderer>().material = playerColors[playerRef];
        newObj.tag = "City";
        return newObj;
    }
}
