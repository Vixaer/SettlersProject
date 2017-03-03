using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaritimeTradePanel : MonoBehaviour {
    public Dropdown selectedResource;
    public Dropdown providedResource;
    public Button confirmButton;
    public Button cancelButton;
    private playerControl localPlayer;
	// Use this for initialization
	void Start () {
        confirmButton.GetComponent<Button>().onClick.AddListener(ConfirmTrade);
        cancelButton.GetComponent<Button>().onClick.AddListener(CancelTrade);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Init()
    {
        var allResourceNames = new List<string>(Enum.GetNames(typeof(ResourceKind)));
        localPlayer = GameObject.FindObjectOfType<playerControl>();
        providedResource.AddOptions(allResourceNames);
        selectedResource.AddOptions(allResourceNames);
        
    }

    void ConfirmTrade()
    {
        localPlayer.CmdMaritimeTrade(providedResource.options[providedResource.value].text, 
            selectedResource.options[selectedResource.value].text);
        var pr = (ResourceKind)Enum.Parse(typeof(ResourceKind), providedResource.options[providedResource.value].text);
        var sr = (ResourceKind)Enum.Parse(typeof(ResourceKind), selectedResource.options[selectedResource.value].text);
        var command = new MaritimeTradeCommand(null, pr, sr);
        command.Execute();
        this.CancelTrade();
    }

    void CancelTrade()
    {
        providedResource.ClearOptions();
        selectedResource.ClearOptions();
        this.gameObject.SetActive(false);
    }
}
