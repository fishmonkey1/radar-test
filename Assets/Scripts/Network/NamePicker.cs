using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NamePicker : MonoBehaviour
{

    [SerializeField] TMP_InputField inputField;
    [SerializeField] MainMenu menu;
    [SerializeField] bool isPopup;

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
        if (isPopup)
            menu.ShowMenu();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
