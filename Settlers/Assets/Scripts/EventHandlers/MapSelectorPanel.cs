using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MapSelectorPanel : MonoBehaviour {

    public Button confirmButton;
    public Button cancelButton;
    public Text instructionText;
    private ActionType actionType;
    private List<GameObject> selectableObjects;
    private GameObject selectedObject;

    void Start()
    {
        confirmButton.GetComponent<Button>().onClick.AddListener(Confirm);
        cancelButton.GetComponent<Button>().onClick.AddListener(Cancel);
    }

    // Update is called once per frame
    void Update()
    {
        confirmButton.interactable = selectedObject != null;
    }

    public void Init(ActionType trigger)
    {
        confirmButton.interactable = false;
        selectedObject = null;
        this.actionType = trigger;
        switch (trigger)
        {
            case ActionType.buildRoad:
                // For now, all edges with no units
                selectableObjects = GameObject.FindGameObjectsWithTag("Edge")
                    .Where(i => i.GetComponent<Edges>().positionedUnit == null)
                    .ToList();
                instructionText.text = "Select an unoccupied edge";
                break;
            case ActionType.buildShip:
                // For now, all edges with no units
                selectableObjects = GameObject.FindGameObjectsWithTag("Edge")
                    .Where(i => i.GetComponent<Edges>().positionedUnit == null)
                    .ToList();
                instructionText.text = "Select an unoccupied edge";
                break;
            case ActionType.buildSettlement:
                // For now, all intersections with no units
                selectableObjects = GameObject.FindGameObjectsWithTag("Intersection")
                    .Where(i => i.GetComponent<Intersection>().positionedUnit == null)
                    .ToList();
                instructionText.text = "Select an unoccupied intersection";
                break;
            case ActionType.upgradeSettlement:
                // For now, all settlements (ownership checked on the server)
                selectableObjects = GameObject.FindGameObjectsWithTag("Settlement").ToList();
                instructionText.text = "Select an existing settlement";
                break;
        }
    }

    public void setSelectedObject(GameObject go)
    {
        if (selectableObjects.Contains(go))
        {
            selectedObject = go;
        }
    }

    #region Listeners
    private void Cancel()
    {
        selectedObject = null;
        instructionText.text = string.Empty;
        selectableObjects.Clear();
        this.gameObject.SetActive(false);
    }

    private void Confirm()
    {
        // Do the action
        switch (this.actionType)
        {
            case ActionType.buildRoad:
                NetworkCommand command = new BuildRoadCommand(null, this.selectedObject.GetComponent<Edges>());
                command.Execute();
                break;
            case ActionType.buildShip:
                command = new BuildShipCommand(null, this.selectedObject.GetComponent<Edges>());
                command.Execute();
                break;
            case ActionType.buildSettlement:
                command = new BuildSettlementCommand(null, this.selectedObject.GetComponent<Intersection>());
                command.Execute();
                break;
            case ActionType.upgradeSettlement:
                command = new UpgradeSettlementCommand(null, this.selectedObject.GetComponent<Village>());
                command.Execute();
                break;
        }
        // Close the UI panel afterwards
        this.Cancel();
    }
    #endregion
}

public enum ActionType
{
    buildSettlement = 1,
    buildRoad,
    buildShip,
    upgradeSettlement
}
