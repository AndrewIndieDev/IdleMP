using Steamworks;
using Steamworks.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;
using AndrewDowsett.SceneLoading;
using UnityEngine.SceneManagement;
using AndrewDowsett.Utility;

[System.Serializable]
public class AvatarImage
{
    public SteamId steamID;
    public Steamworks.Data.Image image;
    public Texture2D processedImage;
}

public class PlayerPartyManager : MonoBehaviour
{
    public static PlayerPartyManager Instance;
    
    public static Lobby lobby;
    public static bool lobbyCreated = false;

    public GameObject partyMemberButtonTemplate;
    public GameObject onlineFriendListItemTemplate;
    public TMP_Text onlineFriendCountText;
    public TMP_Text partySizeText;
    public GameObject leaveButtonContainer;
    public PartyMember Player;
    public List<AvatarImage> partyMemberImages = new List<AvatarImage>();
    public List<AvatarImage> onlineFriendImages = new List<AvatarImage>();
    public CanvasGroup canvasVisibilityGroup;
    public CanvasGroup inviteVisibilityGroup;
    public Dictionary<SteamId, PartyMember> partyMembers = new Dictionary<SteamId, PartyMember>();
    public Dictionary<SteamId, FriendListItem> onlineFriends = new Dictionary<SteamId, FriendListItem>();
    public SteamId openedProfileId;
    public RectTransform invitePanelRectTransform;
    public TMP_Text inviteTitleText;

    public int totalPartyMemberCount => partyMembers.Count + 1; // All other players + you

    [Header("Translations")]
    public string kickedTitleText;
    public string kickedContentText;
    public string invitedText;

    Dictionary<Friend, Lobby> invitedLobbies = new Dictionary<Friend, Lobby>();
    List<Tuple<Friend, Lobby>> inviteQueue = new List<Tuple<Friend, Lobby>>();
    Tuple<Friend, Lobby> currentInvite = null;
    bool closeCurrentInvite = false;

    public void Init()
    {
        Instance = this;

        StartCoroutine(ShowInviteLoop());

        onlineFriendListItemTemplate.SetActive(false);
        partyMemberButtonTemplate.SetActive(false);

        Player = Instantiate(partyMemberButtonTemplate, partyMemberButtonTemplate.transform.parent).GetComponent<PartyMember>();
        Player.gameObject.SetActive(true);
        Player.steamID = SteamClient.SteamId;
        Player.Name = SteamClient.Name;
        Player.profileButtonContainer.SetActive(false);
        Player.kickButtonContainer.SetActive(false);
        Player.promoteButtonContainer.SetActive(false);

        SteamMatchmaking.OnLobbyMemberJoined += OnLobbyMemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyMemberDisconnected += OnLobbyMemberLeave;
        SteamMatchmaking.OnLobbyEntered += OnLobbyEntered;
        SteamMatchmaking.OnLobbyInvite += OnLobbyInvite;
        SteamFriends.OnGameLobbyJoinRequested += OnGameLobbyJoinRequested;
        SteamFriends.OnGameRichPresenceJoinRequested += OnGameRichPresenceJoinRequested;
        SteamMatchmaking.OnChatMessage += OnChatMessageReceived;
        NetworkManager.Singleton.OnServerStopped += OnServerStopped;

        if (SteamClient.IsValid)
        {
            SteamFriends.GetMediumAvatarAsync(SteamClient.SteamId).ContinueWith(t =>
            {
               if (t.Result.HasValue)
               {
                   lock (partyMemberImages)
                   {
                       partyMemberImages.Add(new AvatarImage() { image = t.Result.Value, steamID = SteamClient.SteamId });
                   }
               }
               else
               {
                   Debug.LogError("Failed to fetch player avatar.");
               }
            });

            CreateLobby();
        }

        UpdatePartySize();

        StartCoroutine(UpdateOwnershipStatus());
        StartCoroutine(UpdateOnlinePlayerList());
        StartCoroutine(ConnectToLaunchParameterLobbyIfNeeded());
    }

    private void OnServerStopped(bool obj)
    {
        SendDisconnectMessage();
    }

    internal void EnableAllInviteButtons()
    {
        foreach (FriendListItem item in onlineFriends.Values)
        {
            item.inviteButton.interactable = true;
        }
    }

    private void OnGameRichPresenceJoinRequested(Friend friend, string text)
    {
        JoinLobby(ulong.Parse(text));
    }

    private IEnumerator UpdateOnlinePlayerList()
    {
        while (true)
        {
            List<SteamId> idsToRemove = onlineFriends.Keys
                .Select(s => new Friend(s))
                .Where(friend => !friend.IsOnline)
                .Select(friend => friend.Id).ToList();

            foreach (SteamId id in idsToRemove)
            {
                Destroy(onlineFriends[id].gameObject);
                onlineFriends.Remove(id);
            }

            foreach (Friend friend in SteamFriends.GetFriends())
            {
                if (friend.IsOnline && !onlineFriends.ContainsKey(friend.Id))
                {
                    SteamId friendId = friend.Id;
                    SteamFriends.GetMediumAvatarAsync(friendId).ContinueWith(t =>
                    {
                        if (t.Result.HasValue)
                        {
                            lock (onlineFriendImages)
                            {
                                onlineFriendImages.Add(new AvatarImage() { image = t.Result.Value, steamID = friendId });
                            }
                        }
                        else
                        {
                            Debug.LogError($"Failed to fetch avatar for player {friendId}.");
                        }
                    });
                }
            }

            foreach (FriendListItem item in onlineFriends.Values)
            {
                if (item.steamFriend.IsAway || item.steamFriend.IsBusy ||
                    item.steamFriend.IsSnoozing)
                    item.usernameText.color = item.awayColor;
                if (item.steamFriend.IsPlayingThisGame)
                    item.usernameText.color = item.playingColor;
            }

            SortFriends();

            yield return new WaitForSeconds(10f);
        }
    }

    private void OnChatMessageReceived(Lobby lobby, Friend friend, string message)
    {
        if (friend.Id == SteamClient.SteamId) return;

        Debug.Log("Debug: " + message);
        string[] messageParts = message.Split('|');

        switch (messageParts[0])
        {
            case "JOIN":
                {
                    if (messageParts.Length == 2)
                    {
                        if (NetworkManager.Singleton.IsConnectedClient)
                        {
                            SceneLoader.Instance.LoadScene("MainMenu", true, LoadSceneMode.Single);
                            NetworkManager.Singleton.Shutdown();
                            NetworkManager.Singleton.OnClientStopped += (bool x) =>
                            {
                                ulong lobbyID = ulong.Parse(messageParts[1]);
                                NetworkManager.Singleton.GetComponent<SteamP2PRelayTransport>().serverId = lobbyID;
                                NetworkManager.Singleton.StartClient();
                                NetworkManager.Singleton.OnClientConnectedCallback += JoinGameOnceConnected;
                            };
                        }
                        else
                        {
                            ulong lobbyID = ulong.Parse(messageParts[1]);
                            NetworkManager.Singleton.GetComponent<SteamP2PRelayTransport>().serverId = lobbyID;
                            NetworkManager.Singleton.StartClient();
                            NetworkManager.Singleton.OnClientConnectedCallback += JoinGameOnceConnected;
                        }
                    }
                    break;
                }

            case "DISCONNECT":
                {
                    if (messageParts.Length < 2) break;
                    string scene = messageParts[1];
                    if (string.IsNullOrEmpty(scene))
                    {
                        NetworkManager.Singleton.Shutdown();
                        SceneLoader.Instance.LoadScene("MainMenu", true, LoadSceneMode.Single);
                    }
                    else
                    {
                        NetworkManager.Singleton.Shutdown();
                        SceneLoader.Instance.LoadScene(scene, true, LoadSceneMode.Single);
                    }
                    PopupManager.Instance.ShowDialoguePanel("Server Shutdown", "Server has been shutdown.", EDialoguePanelType.OK);
                    break;
                }

            case "KICK":
                {
                    if (messageParts.Length < 2) break;
                    if (!lobby.IsOwnedBy(friend.Id)) break;

                    if (ulong.TryParse(messageParts[1], out ulong kickedid))
                    {
                        if (kickedid == SteamClient.SteamId)
                        {
                            LeaveLobby();
                            PopupManager.Instance.ShowDialoguePanel(kickedTitleText, kickedContentText, EDialoguePanelType.OK);
                        }
                    }
                
                    break;
                }

            default:
                break;
        }
    }

    public void SetPartyVisibility(bool visible)
    {
        canvasVisibilityGroup.alpha = visible ? 1.0f : 0.0f;
        inviteVisibilityGroup.alpha = visible ? 1.0f : 0.0f;
    }

    void UpdateCrownPlacement()
    {
        if (!SteamClient.IsValid) return;
        bool amIOwner = lobby.IsOwnedBy(SteamClient.SteamId);

        foreach(var index in partyMembers.Values)
        {
            index.crownImage.enabled = lobby.IsOwnedBy(index.steamID);
            index.kickButtonContainer.SetActive(amIOwner);
            index.promoteButtonContainer.SetActive(amIOwner);
        }

        Player.crownImage.enabled = amIOwner;
    }

    void JoinGameOnceConnected(ulong id)
    {
        SceneLoader.Instance.LoadScene("MultiplayerGame", true, LoadSceneMode.Single);
        NetworkManager.Singleton.OnClientConnectedCallback -= JoinGameOnceConnected;
    }
    
    // Called when you enter a lobby.
    private void OnLobbyEntered(Lobby lobby)
    {
        PlayerPartyManager.lobby = lobby;

        foreach(var index in lobby.Members)
        {
            AddPartyMember(index.Id);
        }

        uint ip = 0;
        ushort port = 0;
        SteamId steamId = 0;
        lobby.GetGameServer(ref ip, ref port, ref steamId);
        if (steamId.IsValid)
        {
            NetworkManager.Singleton.GetComponent<SteamP2PRelayTransport>().serverId = steamId;
            SceneLoader.Instance.LoadScene("MultiplayerGame", true, LoadSceneMode.Single);
            NetworkManager.Singleton.StartClient();
        }
    }

    public void UpdateLobbyVisibility(int visibility)
    {
        if (lobby.Owner.Id != SteamClient.SteamId) return;

        switch (visibility)
        {
            case 0: // Private
                lobby.SetPrivate();
                break;
            case 1: // Friends
                lobby.SetFriendsOnly();
                break;
            case 2: // Public
                lobby.SetPublic();
                break;
            default: // huh?
                lobby.SetPublic();
                Debug.Log("Visibility weird. Defaulting to public.");
                break;
        }
    }

    void CreateLobby()
    {
        if (!lobbyCreated)
        {
            SteamMatchmaking.CreateLobbyAsync(8).ContinueWith(l =>
            {
                if (l.Result.HasValue)
                {
                    lobby = l.Result.Value;
                    // Do I need to set a room visibility here?
                    lobbyCreated = true;
                }
                else
                {
                    Debug.LogError("Failed to create lobby!");
                }
            });
        }
    }

    // Called when a lobby member leaves
    private void OnLobbyMemberLeave(Lobby lobby, Friend friend)
    {
        //if (MainMenu.Instance != null && partyMembers.ContainsKey(friend.Id))
        //    MainMenu.Instance.RemovePartyMember(partyMembers[friend.Id]);
        DestroyPartyMember(friend.Id);
    }

    void AddPartyMember(SteamId steamId)
    {
        if (partyMembers.ContainsKey(steamId)) return;
        SteamFriends.GetMediumAvatarAsync(steamId).ContinueWith(t =>
        {
            if (t.Result.HasValue)
            {
                lock (partyMemberImages)
                {
                    partyMemberImages.Add(new AvatarImage() { image = t.Result.Value, steamID = steamId });
                }
            }
            else
            {
                Debug.LogError($"Failed to fetch avatar for player {steamId}.");
            }
        });
    }

    void DestroyPartyMember(SteamId steamId)
    {
        if (!partyMembers.ContainsKey(steamId)) return;
        DestroyImmediate(partyMembers[steamId].gameObject);
        partyMembers.Remove(steamId);
        LayoutRebuilder.ForceRebuildLayoutImmediate(partyMemberButtonTemplate.transform.parent.GetComponent<RectTransform>());
        UpdatePartySize();
        if (onlineFriends.ContainsKey(steamId))
        {
            onlineFriends[steamId].gameObject.SetActive(true);
        }
    }

    // Called when you accept an invite on Steam or right-click "Join" a friend
    public void OnGameLobbyJoinRequested(Lobby lobby, SteamId steamId)
    {
        JoinLobby(lobby.Id);
        PlayerPartyManager.lobby = lobby;
    }

    public void JoinLobby(SteamId lobbyId)
    {
        if (lobbyId == lobby.Id) return;

        LeaveLobby(false);
        SteamMatchmaking.JoinLobbyAsync(lobbyId);
        SendJoinMessage(lobbyId);
    }

    // Called when somebody enters the lobby you're in
    private void OnLobbyMemberJoined(Lobby lobby, Friend friend)
    {
        AddPartyMember(friend.Id);
    }

    void Update()
    {
        lock (partyMemberImages)
        {
            foreach (var index in partyMemberImages.Where(index => index.processedImage == null))
            {
                index.processedImage = CreateProfileTexture((int)index.image.Width, (int)index.image.Height, index.image.Data);
                if (index.steamID == SteamClient.SteamId)
                    Player.gameObject.GetComponentInChildren<RawImage>().texture = index.processedImage;
                else
                {
                    PartyMember member = Instantiate(partyMemberButtonTemplate, partyMemberButtonTemplate.transform.parent).GetComponent<PartyMember>();
                    member.gameObject.SetActive(true);
                    //member.transform.SetSiblingIndex(1);
                    member.steamID = index.steamID;
                    member.Name = new Friend(member.steamID).Name;
                    member.gameObject.GetComponentInChildren<RawImage>().texture = index.processedImage;
                    member.profileButton.onClick.AddListener(() =>
                    {
                        GotoProfilePage(member.steamID);
                    });
                    member.kickButton.onClick.AddListener(() =>
                    {
                        KickPlayer(member.steamID);
                    });
                    member.promoteButton.onClick.AddListener(() =>
                    {
                        lobby.Owner = new Friend(member.steamID);
                    });
                    member.kickButtonContainer.SetActive(false);
                    member.promoteButtonContainer.SetActive(false);
                    //member.GetComponent<Button>().onClick.AddListener(() => OnPartyMemberButtonClicked(index.steamID));
                    partyMembers.Add(index.steamID, member);
                    LayoutRebuilder.ForceRebuildLayoutImmediate(member.transform.parent.GetComponent<RectTransform>());
                    UpdatePartySize();
                    if (onlineFriends.ContainsKey(index.steamID))
                    {
                        onlineFriends[index.steamID].gameObject.SetActive(false);
                    }
                }
            }
        }
        lock (onlineFriendImages)
        {
            bool changed = false;
            foreach (var index in onlineFriendImages.Where(index => index.processedImage == null))
            {
                changed = true;
                index.processedImage = CreateProfileTexture((int)index.image.Width, (int)index.image.Height, index.image.Data);
                GameObject newListItem = Instantiate(onlineFriendListItemTemplate, onlineFriendListItemTemplate.transform.parent);
                FriendListItem listItemComponent = newListItem.GetComponent<FriendListItem>();
                listItemComponent.steamFriend = new Friend(index.steamID);
                listItemComponent.usernameText.text = listItemComponent.steamFriend.Name;
                if (listItemComponent.steamFriend.IsAway || listItemComponent.steamFriend.IsBusy ||
                    listItemComponent.steamFriend.IsSnoozing)
                    listItemComponent.usernameText.color = listItemComponent.awayColor;
                if (listItemComponent.steamFriend.IsPlayingThisGame)
                    listItemComponent.usernameText.color = listItemComponent.playingColor;
                listItemComponent.profileImage.texture = index.processedImage;
                newListItem.SetActive(true);
                onlineFriends.Add(index.steamID, listItemComponent);
            }

            if (changed)
            {
                SortFriends();
                onlineFriendCountText.text = onlineFriends.Count.ToString();
            }
        }
    }

    private void SortFriends()
    {
        // Sort players
        List<Friend> friends = onlineFriends.Keys.Select(s => new Friend(s))
            .OrderBy(f => f.IsPlayingThisGame ? 0 : 1)
            .ThenBy(f => (f.IsAway || f.IsBusy || f.IsSnoozing) ? 1 : 0)
            .ThenBy(f => f.Name).ToList();
        foreach (FriendListItem item in onlineFriends.Values)
        {
            item.transform.SetSiblingIndex(friends.IndexOf(item.steamFriend));
        }
    }

    Texture2D CreateProfileTexture(int width, int height, byte[] colors)
    {
        Texture2D profileTex = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
        profileTex.LoadRawTextureData(colors);
        profileTex.Apply();
        return profileTex;
    }

    public void OnAddPartyMemberButtonClicked()
    {
        SteamFriends.OpenGameInviteOverlay(lobby.Id);
    }

    //public void OnPartyMemberButtonClicked(SteamId steamId)
    //{
    //    Friend friend = new Friend(steamId);
    //
    //    partyMemberContextMenu.ClearOptions();
    //    partyMemberContextMenu.AddOption("View Steam profile", s => SteamFriends.OpenUserOverlay(steamId, "steamid"));
    //    if (friend.Relationship == Relationship.None)
    //        partyMemberContextMenu.AddOption("Add friend", s => SteamFriends.OpenUserOverlay(steamId, "friendadd"));
    //    else if (friend.Relationship == Relationship.RequestRecipient)
    //        partyMemberContextMenu.AddOption("Accept friend request", s => SteamFriends.OpenUserOverlay(steamId, "friendrequestaccept"));
    //    partyMemberContextMenu.AddOption("View profile", s => GotoProfilePage(steamId));
    //    if (lobby.IsOwnedBy(SteamClient.SteamId))
    //    {
    //        partyMemberContextMenu.AddOption("Kick player", s => KickPlayer(steamId));
    //        partyMemberContextMenu.AddOption("Promote to party leader", s => lobby.Owner = friend);
    //    }
    //    partyMemberContextMenu.Show();
    //}
    //
    //public void OnSelfMemberButtonClicked()
    //{
    //    partyMemberContextMenu.ClearOptions();
    //    partyMemberContextMenu.AddOption("View profile", s => GotoProfilePage(SteamClient.SteamId));
    //    if (partyMembers.Count > 0)
    //        partyMemberContextMenu.AddOption("Leave party", s => LeaveLobby());
    //    partyMemberContextMenu.Show();
    //}

    private void GotoProfilePage(SteamId steamid)
    {
        openedProfileId = steamid;
        SceneLoader.Instance.LoadScene("UserProfile");
    }

    private void KickPlayer(SteamId steamid)
    {
        lobby.SendChatString($"KICK|{steamid.Value.ToString()}");
    }

    public void LeaveLobby(bool createNew = true)
    {
        lobby.Leave();
        lobbyCreated = false;
        SteamId[] ids = partyMembers.Keys.ToArray();
        foreach (var i in ids)
        {
            //if (MainMenu.Instance != null)
            //    MainMenu.Instance.RemovePartyMember(partyMembers[i]);
            DestroyPartyMember(i);
        }
        if (createNew)
            CreateLobby();
    }

    IEnumerator UpdateOwnershipStatus()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.5f);
            UpdateCrownPlacement();
            UpdatePlayButtons();
        }
    }

    private void UpdatePlayButtons()
    {
        //if (MainMenu.Instance != null)
        //{
        //    if (!SteamClient.IsValid)
        //    {
        //        MainMenu.Instance.playButtonGroup.interactable = true;
        //    }
        //    else
        //    {
        //        MainMenu.Instance.playButtonGroup.interactable = lobby.IsOwnedBy(SteamClient.SteamId);
        //    }
        //}
    }

    private void UpdatePartySize()
    {
        partySizeText.text = totalPartyMemberCount + "/8";
        leaveButtonContainer.SetActive(partyMembers.Count > 0);
    }

    public void OnLeaveButtonPressed()
    {
        LeaveLobby();
    }

    public void SendJoinMessage(string ip, int port, string password)
    {
        lobby.SendChatString($"JOIN|{ip}|{port.ToString()}|{password}");
    }

    public void SendJoinMessage(SteamId steamId)
    {
        lobby.SendChatString($"JOIN|{steamId.Value}");
    }

    public void SendDisconnectMessage(string sceneToLoad = "")
    {
        lobby.SendChatString($"DISCONNECT|{sceneToLoad}");
    }

    IEnumerator ConnectToLaunchParameterLobbyIfNeeded()
    {
        yield return new WaitForSeconds(1.0f);
        int connectionIndex = System.Environment.GetCommandLineArgs().ToList().IndexOf("+connect_lobby");
        if (connectionIndex != -1 && System.Environment.GetCommandLineArgs().Length >= connectionIndex + 2)
        {
            string lobbyIDString = System.Environment.GetCommandLineArgs()[connectionIndex + 1];
            if (ulong.TryParse(lobbyIDString, out ulong lobbyID))
            {
                JoinLobby(lobbyID);
            }
            else
            {
                Debug.LogError("Could not parse launch parameter for Steam Lobby ID (Game Start)");
            }
        }
    }

    public void OnLobbyInvite(Friend friend, Lobby lobby)
    {
        inviteQueue.Add(new Tuple<Friend, Lobby>(friend, lobby));
    }

    public void AcceptInvitation()
    {
        JoinLobby(currentInvite.Item2.Id);
        closeCurrentInvite = true;
    }

    public void DenyInvitation()
    {
        closeCurrentInvite = true;
    }

    IEnumerator ShowInviteLoop()
    {
        invitePanelRectTransform.gameObject.SetActive(false);
        currentInvite = null;
        while (true)
        {
            if (inviteQueue.Count > 0)
            {
                currentInvite = inviteQueue[0];
                inviteQueue.RemoveAt(0);
                closeCurrentInvite = false;
                invitePanelRectTransform.gameObject.SetActive(true);
                //object[] args = new object[] { currentInvite.Item1.Name };
                string invitedBy = currentInvite.Item1.Name;
                if (invitedBy.Length > 12)
                {
                    invitedBy = invitedBy.Substring(0, 10) + "..";
                }
                inviteTitleText.text = invitedText.Replace("{0}", invitedBy);
                float i = 0;
                while (i < 1.0f)
                {
                    i += Time.deltaTime * 2.0f;
                    invitePanelRectTransform.sizeDelta = new Vector2(Mathf.Max(Mathf.Pow(i, 2.0f) * 500.0f, 1.0f), 100.0f);
                    yield return 0;
                }
                i = 0.0f;
                while (i < 5.0f)
                {
                    i += Time.deltaTime;
                    if (closeCurrentInvite)
                        i = 500.0f;
                    yield return 0;
                }
                i = 0.0f;
                while (i < 1.0f)
                {
                    i += Time.deltaTime * 2.0f;
                    invitePanelRectTransform.sizeDelta = new Vector2(Mathf.Max(Mathf.Pow(1.0f - i, 2.0f) * 500.0f, 1.0f), 100.0f);
                    yield return 0;
                }
                invitePanelRectTransform.gameObject.SetActive(false);
                currentInvite = null;
            }
            yield return new WaitForSeconds(0.5f);
        }
    }
}
