using TMPro;
using UnityEngine;

public class NamePicker : MonoBehaviour
{

    [SerializeField] TMP_InputField inputField;
    [SerializeField] MainMenu menu;
    [SerializeField] bool isPopup;
    [HideInInspector] public bool PopupFromJoin; //If this is true, then once the name is updated we can proceed to either join or host afterwards
    [HideInInspector] public bool PopupFromHost; //Same idea, but for the host button

    // Start is called before the first frame update
    void Awake()
    {
        if (inputField == null)
            inputField = GetComponent<TMP_InputField>();
        inputField.onEndEdit.AddListener((string text) => UpdatePlayerName());
    }

    void UpdatePlayerName()
    {
        PlayerInfo.localPlayerName = inputField.text;
        if (PopupFromHost)
        {
            menu.HostGame();
            return;
        }
        if (PopupFromJoin)
        {
            menu.ShowJoinScreen();
            return;
        }
        if (isPopup)
            menu.ShowMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
