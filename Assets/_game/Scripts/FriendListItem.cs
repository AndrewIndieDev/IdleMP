using System.Collections;
using System.Collections.Generic;
using Steamworks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FriendListItem : MonoBehaviour
{
    public RawImage profileImage;
    public TMP_Text usernameText;
    public Button inviteButton;
    public Friend steamFriend;
    public Color awayColor;
    public Color playingColor;
    public Steamworks.Data.Lobby lobby;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnInviteButtonPressed()
    {
        if (lobby.Id.IsValid)
            PlayerPartyManager.Instance.JoinLobby(lobby.Id);
        else
            PlayerPartyManager.lobby.InviteFriend(steamFriend.Id);
        StartCoroutine(InteractableManagement());
    }

    IEnumerator InteractableManagement()
    {
        inviteButton.interactable = false;
        yield return new WaitForSeconds(5.0f);
        inviteButton.interactable = true;
    }

    private void OnDisable()
    {
        inviteButton.interactable = true;
    }
}
