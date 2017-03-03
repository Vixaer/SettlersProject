using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class CameraBehaviour : NetworkBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
        if (isLocalPlayer) return;
        if (Input.GetAxis("Horizontal") > 0)
        {
            transform.GetChild(0).transform.Translate(new Vector3(10 * Input.GetAxis("Horizontal"), 0, 0));
           
        }
        if (Input.GetAxis("Horizontal") < 0)
        {
            transform.GetChild(0).transform.Translate(new Vector3(10 * Input.GetAxis("Horizontal"), 0, 0));
        }
        if (Input.GetAxis("Vertical") > 0)
        {
            transform.GetChild(0).transform.Translate(new Vector3(0, 10 * Input.GetAxis("Vertical"), 0));
        }
        if (Input.GetAxis("Vertical") < 0)
        {
            transform.GetChild(0).transform.Translate(new Vector3(0, 10 * Input.GetAxis("Vertical"), 0));
        }
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            gameObject.GetComponent<Camera>().orthographicSize += 50;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            gameObject.GetComponent<Camera>().orthographicSize -= 50;
        }
    }

}
