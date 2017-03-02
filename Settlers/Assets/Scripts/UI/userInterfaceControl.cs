using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class userInterfaceControl : MonoBehaviour {
    public bool isUserBarOpen = true;
    public bool isInventoryOpen =false;
    public bool isSkillsOpen = false;
    public bool isCharOpen = false;
    public bool isMenuOpen = false;
    public Sprite closeArrow;
    public Sprite openArrow;
    GameObject userBar;
    GameObject controlButton;
    GameObject inventoryPanel;
    GameObject skillsPanel;
    GameObject charPanel;
    GameObject menuPanel;
    // Use this for initialization
    void Start () {
        inventoryPanel = transform.GetChild(1).gameObject;
        inventoryPanel.SetActive(false);
        skillsPanel = transform.GetChild(2).gameObject;
        skillsPanel.SetActive(false);
        charPanel = transform.GetChild(3).gameObject;
        charPanel.SetActive(false);
        menuPanel = transform.GetChild(4).gameObject;
        menuPanel.SetActive(false);
        userBar = transform.GetChild(0).gameObject;
        controlButton = transform.GetChild(0).GetChild(0).gameObject;
        
	}
	
	// Update is called once per frame
	void Update () {
        if (Input.GetButtonDown("Inventory"))
        {
            controlInventoryWindow();
        }
        if (Input.GetButtonDown("Skills"))
        {
            controlSkillsWindow();
        }
        if(Input.GetButtonDown("Character"))
        {
            controlCharacterWindow();
        }
        if (Input.GetButtonDown("Cancel"))
        {
            controlMenuWindow();
        }
    }
    public void controlInterfaceBar()
    {
        if (isUserBarOpen)
        {
            userBar.GetComponent<Animation>().Play("OpenInterfaceBar");
            controlButton.GetComponent<Image>().sprite = openArrow;
            isUserBarOpen = false;
        }
        else
        {
            userBar.GetComponent<Animation>().Play("CloseInterfaceBar");
            controlButton.GetComponent<Image>().sprite = closeArrow;
            isUserBarOpen = true;
        }
    }
    public void controlInventoryWindow()
    {
        if (isInventoryOpen)
        {
            inventoryPanel.SetActive(false);
            isInventoryOpen = false;
        }
        else
        {
            inventoryPanel.SetActive(true);
            isInventoryOpen = true;
        }
    }
    public void controlSkillsWindow()
    {
        if (isSkillsOpen)
        {
            skillsPanel.SetActive(false);
            isSkillsOpen = false;
        }
        else
        {
            skillsPanel.SetActive(true);
            isSkillsOpen = true;
        }
    }
    public void controlCharacterWindow()
    {
        if (isCharOpen)
        {
            charPanel.SetActive(false);
            isCharOpen = false;
        }
        else
        {
            charPanel.SetActive(true);
            isCharOpen = true;
        }
    }
    public void controlMenuWindow()
    {
        if (isInventoryOpen)
        {
            controlInventoryWindow();
        }
        if (isSkillsOpen)
        {
            controlSkillsWindow();
        }
        if (isCharOpen)
        {
            controlCharacterWindow();
        }
        if (isMenuOpen)
        {
            menuPanel.SetActive(false);
            isMenuOpen = false;
        }
        else
        {
            menuPanel.SetActive(true);
            isMenuOpen = true;
        }
    }
}
