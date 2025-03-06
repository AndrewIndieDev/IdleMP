using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Netcode;
using Steamworks;
using AndrewDowsett.Utility;
using AndrewDowsett.SceneLoading;

[System.Serializable]
public struct RoomData : INetworkSerializable
{
    public string name;
    public int maxPlayers;
    public string password;
    public string gamemode;
    public string maps;

    public RoomData(string name, int maxPlayers, string password, string gamemode, string[] maps)
    {
        this.name = name;
        this.maxPlayers = maxPlayers;
        this.password = password;
        this.gamemode = gamemode;
        this.maps = string.Join(",", maps);
    }

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref name);
        serializer.SerializeValue(ref maxPlayers);
        serializer.SerializeValue(ref gamemode);
        serializer.SerializeValue(ref maps);
    }
}

public class CreateRoom : MonoBehaviour
{
    public TMP_InputField nameText;
    public TMP_InputField passwordText;
    public TMP_Dropdown maxPlayersDropdown;
    public TMP_Dropdown gameModeDropdown;

    [Header("Translations")]
    public string errorCreatingRoomTitleText;
    public string errorCreatingRoomContentText;
    public string serverErrorCodeTitleText;
    public string roomCouldNotBeCreatedContentText;

    RoomData roomData;

    public void OnStartRoomButtonPressed()
    {
        string errors = FindErrors();
        if (errors != "")
        {
            PopupManager.Instance.ShowDialoguePanel(errorCreatingRoomTitleText, errorCreatingRoomContentText, EDialoguePanelType.OK);
        }
        else
        {
            string serverName = nameText.text;
            int serverMaxPlayers = int.Parse(maxPlayersDropdown.options[maxPlayersDropdown.value].text);
            string serverPassword = passwordText.text == "" ? "" : passwordText.text.GetSha256();
            string serverGameMode = gameModeDropdown.options[gameModeDropdown.value].text;
            string[] serverMaps = new string[] { "Default" };

            roomData = new RoomData(serverName, serverMaxPlayers, serverPassword, serverGameMode, serverMaps);

            NetworkManager.Singleton.OnClientStarted += () =>
            {
                SceneLoader.Instance.LoadScene("MultiplayerGame", true, UnityEngine.SceneManagement.LoadSceneMode.Single);
            };

            NetworkManager.Singleton.StartHost();
            CustomNetworkManager.Instance.roomData = roomData;
            PlayerPartyManager.Instance.SendJoinMessage(PlayerPartyManager.lobby.Owner.Id);

            PlayerPartyManager.lobby.SetData("LobbyType", "Game");
            PlayerPartyManager.lobby.SetData("Name", serverName.SanitizeText());
            PlayerPartyManager.lobby.SetData("GameMode", serverGameMode);
            PlayerPartyManager.lobby.SetData("Maps", serverMaps[0]);
            PlayerPartyManager.lobby.SetGameServer(PlayerPartyManager.lobby.Owner.Id);
            PlayerPartyManager.lobby.MaxMembers = serverMaxPlayers;
            PlayerPartyManager.lobby.SetPublic();
        }
    }
    
    string FindErrors()
    {
        string errors = "";

        if (nameText.text.Length < 3 || nameText.text.Length > 32)
            errors += "Name must be between 3-32 characters\n";

        bool valid = true;
        if (int.TryParse(maxPlayersDropdown.options[maxPlayersDropdown.value].text, out int maxplayers))
        {
            if (maxplayers < 1 || maxplayers > 12)
                valid = false;
        }
        else
            valid = false;
        if (!valid)
            errors += "Max Players must be between 1-12\n";

        return errors.Trim();
    }
}
