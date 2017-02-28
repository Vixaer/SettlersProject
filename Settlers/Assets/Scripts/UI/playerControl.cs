using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Networking;

public class playerControl : NetworkBehaviour {
    Animator resourceAnimator;
    GameObject mainView;
    NetworkManager networkManage;
    public bool resourcesShown;
    InputField chat;
	// Use this for initialization
	void Start () {
        resourceAnimator = GetComponent<Animator>();
        resourcesShown = resourceAnimator.GetBool("ResourcesShown");
        //get networkManager that has been used (only works if a host/client has started)
        networkManage = NetworkManager.singleton;
        chat = transform.GetChild(2).GetChild(1).GetComponent<InputField>();
    }
	
	// Update is called once per frame
	void Update () {
        if (!isLocalPlayer) return;
        if (Input.GetButton("Tab"))
        {
            transform.GetChild(1).gameObject.SetActive(true);
        }
        else { transform.GetChild(1).gameObject.SetActive(false); }
        
        if (Input.GetAxis("Horizontal") > 0 && !chat.isFocused)
        {
            transform.GetChild(3).transform.Translate(new Vector3(0.25f*Input.GetAxis("Horizontal"), 0, 0));
        }
        if(Input.GetAxis("Horizontal") < 0 && !chat.isFocused)
        {
            transform.Translate(new Vector3(0.25f * Input.GetAxis("Horizontal"), 0, 0));
        }
        if (Input.GetAxis("Vertical") > 0 && !chat.isFocused)
        {
            transform.GetChild(3).transform.Translate(new Vector3(0, 0.25f * Input.GetAxis("Vertical"), 0));
        }
        if (Input.GetAxis("Vertical") < 0 && !chat.isFocused)
        {
            transform.GetChild(3).transform.Translate(new Vector3(0, 0.25f * Input.GetAxis("Vertical"), 0));
        }
        if (Input.GetButtonDown("Submit"))
        {
            GameObject chatMessage = transform.GetChild(2).GetChild(0).GetChild(2).gameObject;
            if(chatMessage.GetComponent<Text>().text != null)
            {
                
            }
        }
	}
    public void switchResourcesView()
    {
        resourcesShown = !resourcesShown;
        resourceAnimator.SetBool("ResourcesShown", resourcesShown);
    }
}
