using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DiceResultsPanel : MonoBehaviour {
    public Button closeButton;
    public Text eventDiceRoll;
    public Text redDiceRoll;
    public Text yellowDiceRoll;
    private playerControl localPlayer;
    // Use this for initialization
    void Start()
    {
        closeButton.GetComponent<Button>().onClick.AddListener(OnClose);
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(EventKind e, int r, int y)
    {
        eventDiceRoll.text = "Event dice roll: " + e.ToString();
        redDiceRoll.text = "Red dice roll: " + r;
        yellowDiceRoll.text = "Yellow dice roll: " + y;
    }

    void OnClose()
    {
        eventDiceRoll.text = "";
        redDiceRoll.text = "";
        yellowDiceRoll.text = "";
        this.gameObject.SetActive(false);
    }
}
