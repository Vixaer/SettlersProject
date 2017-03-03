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
    public Button endTurnButton;
    public MaritimeTradePanel maritimeTradePanel;
    public MapSelectorPanel mapSelectorPanel;
    public DiceResultsPanel diceResultsPanel;
    private Game gameState;

	// Use this for initialization
	void Start () {
        rollDiceButton.GetComponent<Button>().onClick.AddListener(RollDice);
        maritimeTradeButton.GetComponent<Button>().onClick.AddListener(MaritimeTrade);
        upgradeSettlementButton.GetComponent<Button>().onClick.AddListener(UpgradeSettlement);
        buildSettlementButton.GetComponent<Button>().onClick.AddListener(BuildSettlement);
        buildRoadButton.GetComponent<Button>().onClick.AddListener(BuildRoad);
        buildShipButton.GetComponent<Button>().onClick.AddListener(BuildShip);

    }
	
	// Update is called once per frame
	void Update () {
        // Activate buttons based on game state
        if (gameState == null)
        {
            var gameObj = GameObject.Find("GameState");
            if (gameObj != null)
                gameState = gameObj.GetComponent<Game>();
        }
        else {
            rollDiceButton.interactable = gameState.currentPhase == GamePhase.TurnStarted;
            upgradeSettlementButton.interactable = gameState.currentPhase == GamePhase.TurnFirstPhase ||
                gameState.currentPhase == GamePhase.TurnSecondPhase;
            buildSettlementButton.interactable = gameState.currentPhase == GamePhase.TurnFirstPhase ||
                gameState.currentPhase == GamePhase.TurnSecondPhase;
            buildRoadButton.interactable = gameState.currentPhase == GamePhase.TurnFirstPhase ||
                gameState.currentPhase == GamePhase.TurnSecondPhase;
            buildShipButton.interactable = gameState.currentPhase == GamePhase.TurnFirstPhase ||
                gameState.currentPhase == GamePhase.TurnSecondPhase;
            maritimeTradeButton.interactable = gameState.currentPhase == GamePhase.TurnFirstPhase ||
                gameState.currentPhase == GamePhase.TurnSecondPhase;
            endTurnButton.interactable = gameState.currentPhase == GamePhase.TurnFirstPhase ||
                gameState.currentPhase == GamePhase.TurnSecondPhase;
        }

    }

    void RollDice()
    {
        var command = new RollDiceCommand(null);
        command.Execute();

        // Initialize the panel
        diceResultsPanel.gameObject.SetActive(true);
        diceResultsPanel.Init(gameState.eventDiceRoll, gameState.redDiceRoll, gameState.yellowDiceRoll);
    }

    void MaritimeTrade()
    {
        maritimeTradePanel.gameObject.SetActive(true);
        maritimeTradePanel.Init();
    }

    void UpgradeSettlement()
    {
        mapSelectorPanel.gameObject.SetActive(true);
        mapSelectorPanel.Init(ActionType.upgradeSettlement);
    }

    void BuildSettlement()
    {
        mapSelectorPanel.gameObject.SetActive(true);
        mapSelectorPanel.Init(ActionType.buildSettlement);
    }

    void BuildRoad()
    {
        mapSelectorPanel.gameObject.SetActive(true);
        mapSelectorPanel.Init(ActionType.buildRoad);
    }

    void BuildShip()
    {
        mapSelectorPanel.gameObject.SetActive(true);
        mapSelectorPanel.Init(ActionType.buildShip);
    }
}
