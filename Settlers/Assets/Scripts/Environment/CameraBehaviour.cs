using UnityEngine;
using System.Collections;

public class CameraBehaviour : MonoBehaviour {
    GameObject following;
    Transform positioning;
    Vector3 moving;
	// Use this for initialization
	void Start () {
        following = GameObject.Find("Head");
        positioning = GetComponent<Transform>();
        moveCamera();
	}
	
	// Update is called once per frame
	void Update () {
        moveCamera();
        
    }
    public void moveCamera()
    {
        moving.x = following.GetComponent<Transform>().position.x;
        moving.y = following.GetComponent<Transform>().position.y;
        moving.z = positioning.position.z;
        positioning.position = moving;
    }
}
