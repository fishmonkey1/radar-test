using UnityEngine;
using Mirror;
using TMPro;

/// <summary>
/// Shows each of the screens, linked up to the buttons on the UI prefab.
/// </summary>
public class MainMenu : MonoBehaviour
{

    public GameObject Menu; //For hiding/showing the initial screen
    public GameObject OptionsPopup; //Show the options screen
    public GameObject JoinPopup; //Show the join screen
    public GameObject CreditsPopup; //Need to link this to a credits file at some point
    public GameObject NamePopup; //If you have no name assigned this appears instead of hosting or joining a game
    public TMP_InputField IPInput;
    public TMP_InputField PortInput;
    GameObject currentScreen; //Check which screen you're showing right now

    private void Start()
    {
        currentScreen = Menu;
    }

    /// <summary>
    /// Show the name popup if no profile is loaded, or start a game as the host. The Namepicker also calls this method after a profile is loaded.
    /// </summary>
    public void HostGame()
    {
        if (PlayerProfile.LoadedProfileName == null)
        { //Pick a name before hosting
            //If you host or join and get a name popup, this is true and makes the popup join the game
            NamePopup.GetComponent<NamePicker>().PopupFromHost = true;
            ShowNamePopup();
            return;
        }
        TankRoomManager manager = NetworkManager.singleton as TankRoomManager;
        manager.StartHost();
    }

    /// <summary>
    /// Let the player pick a name, which loads a profile currently.
    /// </summary>
    public void ShowNamePopup()
    {
        currentScreen.SetActive(false);
        currentScreen = NamePopup;
        currentScreen.SetActive(true);
    }

    /// <summary>
    /// Shows the join screen if a profile is loaded, otherwise it shows the NamePicker. NamePicker also calls this after a profile has been loaded.
    /// </summary>
    public void ShowJoinScreen()
    {
        //Check the local PlayerProfile to check that there's a loaded profile
        if (PlayerProfile.LoadedProfileName == null)
        { //Pick a name before joining
            ShowNamePopup();
            //If you host or join and get a name popup, this is true and makes the popup join the game
            NamePopup.GetComponent<NamePicker>().PopupFromJoin = true;
            return;
        }
        currentScreen.SetActive(false);
        currentScreen = JoinPopup;
        currentScreen.SetActive(true);
    }

    /// <summary>
    /// Returns to the starting menu, where the player can navigate to the other screens
    /// </summary>
    public void ShowMenu()
    {
        currentScreen.SetActive(false);
        currentScreen = Menu;
        currentScreen.SetActive(true);
    }

    /// <summary>
    /// Show the options screen, which currently holds a NamePicker.
    /// </summary>
    public void ShowOptions()
    {
        currentScreen.SetActive(false);
        currentScreen = OptionsPopup;
        currentScreen.SetActive(true);
    }
    /// <summary>
    /// Shows the credits screen. TODO: Have this load from a credits.txt file we can keep updated.
    /// </summary>
    public void ShowCredits()
    {
        currentScreen.SetActive(false);
        currentScreen = CreditsPopup;
        currentScreen.SetActive(true);
    }

    /// <summary>
    /// Stops the editor, or closes the application down.
    /// </summary>
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
            if (ushort.TryParse(PortInput.text, out ushort port))
                portTransport.Port = port;
        }
        manager.StartClient(); //Actually join into the server

    }

}
