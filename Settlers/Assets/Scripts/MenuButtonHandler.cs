using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButtonHandler : MonoBehaviour {

    public Button rollDiceButton;
    public Button upgradeSettlementButton;
    public Button buildSettlementButton;
    public Button buildRoadButton;
    public Button buildShipButton;
    public Button maritimeTradeButton;
    public MaritimeTradePanel maritimeTradePanel;
    private Game currentGame;

	// Use this for initialization
	void Start () {
        currentGame = GameObject.Find("Game").GetComponent<Game>();
        rollDiceButton.GetComponent<Button>().onClick.AddListener(RollDice);
        maritimeTradeButton.GetComponent<Button>().onClick.AddListener(MaritimeTrade);
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void RollDice()
    {
        DiceController.rollDice(currentGame);
    }

    void MaritimeTrade()
    {
        maritimeTradePanel.gameObject.SetActive(true);
        maritimeTradePanel.Init(currentGame);
    }
}
