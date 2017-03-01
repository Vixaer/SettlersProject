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
    private Game currentGame;
	// Use this for initialization
	void Start () {
        confirmButton.GetComponent<Button>().onClick.AddListener(ConfirmTrade);
        cancelButton.GetComponent<Button>().onClick.AddListener(CancelTrade);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void Init(Game cg)
    {
        currentGame = cg;
        var allResourceNames = new List<string>(Enum.GetNames(typeof(ResourceKind)));
        var providableResources = new List<string>();
        foreach (ResourceKind r in Enum.GetValues(typeof(ResourceKind)))
        {
            if (currentGame.currentPlayer.resources[r] >= 4)
            {
                providableResources.Add(r.ToString());
            }
        }
        providedResource.AddOptions(providableResources);
        selectedResource.AddOptions(allResourceNames);
        
    }

    void ConfirmTrade()
    {
        var resourceProvided = (ResourceKind)Enum.Parse(typeof(ResourceKind),providedResource.options[providedResource.value].text);
        var resourceSelected = (ResourceKind)Enum.Parse(typeof(ResourceKind), selectedResource.options[selectedResource.value].text);
        currentGame.currentPlayer.MaritimeTrade(resourceProvided, resourceSelected);
        this.CancelTrade();
    }

    void CancelTrade()
    {
        providedResource.ClearOptions();
        selectedResource.ClearOptions();
        this.gameObject.SetActive(false);
    }
}
