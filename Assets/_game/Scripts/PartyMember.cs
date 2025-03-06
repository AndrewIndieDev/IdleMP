using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PartyMember : MonoBehaviour
{
    public SteamId steamID;

    private new string name;
    public string Name
    {
        get => name;
        set 
        { 
            name = value;
            nameText.text = name;
        }
    }

    public TMP_Text nameText;

    public Image crownImage;
    public string characterTheme;
    public string characterName;
    public string characterStyle;
    public bool alreadySpawnedPreview = false;
    public GameObject promoteButtonContainer;
    public Button promoteButton;
    public GameObject kickButtonContainer;
    public Button kickButton;
    public GameObject profileButtonContainer;
    public Button profileButton;
}
