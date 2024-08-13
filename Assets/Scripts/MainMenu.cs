using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using TMPro;

public class MainMenu : MonoBehaviour
{

    public GameObject Menu; //For hiding/showing the initial screen
    public GameObject OptionsPopup; //Show the options screen
    public GameObject JoinPopup; //Show the join screen
    public GameObject CreditsPopup; //Need to link this to a credits file at some point
    public TMP_InputField IPInput;
    public TMP_InputField PortInput;
    GameObject currentScreen; //Check which screen you're showing right now

    private void Start()
    {
        currentScreen = Menu;
    }

    public void HostGame()
    {
        TankRoomManager manager = NetworkManager.singleton as TankRoomManager;
        manager.StartHost();
    }

    public void ShowJoinScreen()
    {
        currentScreen.SetActive(false);
        currentScreen = JoinPopup;
        currentScreen.SetActive(true);
    }

    public void ShowMenu()
    {
        currentScreen.SetActive(false);
        currentScreen = Menu;
        currentScreen.SetActive(true);
    }

    public void ShowOptions()
    {
        currentScreen.SetActive(false);
        currentScreen = OptionsPopup;
        currentScreen.SetActive(true);
    }

    public void ShowCredits()
    {
        currentScreen.SetActive(false);
        currentScreen = CreditsPopup;
        currentScreen.SetActive(true);
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
                // Application.Quit() does not work in the editor so
                // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    public void JoinGame()
    { //Just duplicating some of the stuff from the NetworkManagerHUD
        TankRoomManager manager = NetworkManager.singleton as TankRoomManager;
        manager.networkAddress = IPInput.text;
        // only show a port field if we have a port transport
        // we can't have "IP:PORT" in the address field since this only
        // works for IPV4:PORT.
        // for IPV6:PORT it would be misleading since IPV6 contains ":":
        // 2001:0db8:0000:0000:0000:ff00:0042:8329
        if (Transport.active is PortTransport portTransport)
        {
            // use TryParse in case someone tries to enter non-numeric characters
            if (ushort.TryParse(GUILayout.TextField(portTransport.Port.ToString()), out ushort port))
                portTransport.Port = port;
        }
        manager.StartClient(); //Actually join into the server

    }

}
