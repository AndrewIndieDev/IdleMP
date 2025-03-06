using System;
using System.Collections;
using System.Collections.Generic;
using Steamworks;
using Unity.Netcode;
using UnityEngine;

public class PersistentClient : NetworkBehaviour
{
    public static Dictionary<ulong, PersistentClient> AllClients = new Dictionary<ulong, PersistentClient>();
    public static PersistentClient LocalClient;
    
    public int LastPing => nv_LastPing.Value;
    public ulong SteamID => nv_SteamID.Value;
    public string SteamName => steamName;

    private NetworkVariable<int> nv_LastPing = new(-1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    private NetworkVariable<ulong> nv_SteamID = new(0, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    

    [SerializeField] private string steamName;
    
    private DateTime lastPingTime;
    
    private void Start()
    {
        DontDestroyOnLoad(gameObject);
        if (IsOwner)
            nv_SteamID.Value = SteamClient.SteamId;
        OnSteamIDChanged(0, nv_SteamID.Value);
        if (NetworkManager.IsHost)
        {
            StartCoroutine(UpdatePing());
        }
    }

    private IEnumerator UpdatePing()
    {
        while (true)
        {
            PingServerRPC();
            yield return new WaitForSeconds(2.0f);
        }
    }

    [Rpc(SendTo.Server, RequireOwnership = false)]
    private void PingServerRPC()
    {
        lastPingTime = DateTime.UtcNow;
        PingClientRPC();
    }

    [Rpc(SendTo.ClientsAndHost)]
    private void PingClientRPC()
    {
        if (IsOwner)
            PingFromClientServerRPC();
    }

    [Rpc(SendTo.Server, RequireOwnership = true)]
    private void PingFromClientServerRPC(RpcParams rpcParams = default)
    {
        DateTime time = DateTime.UtcNow;
        AllClients[rpcParams.Receive.SenderClientId].nv_LastPing.Value = (int)(time - lastPingTime).TotalMilliseconds;
    }

    public override void OnNetworkSpawn()
    {
        nv_SteamID.OnValueChanged += OnSteamIDChanged;
        
        if (AllClients.ContainsKey(OwnerClientId))
            AllClients.Remove(OwnerClientId);
        AllClients.Add(OwnerClientId, this);
        
        if (IsOwner)
            LocalClient = this;
    }

    public override void OnNetworkDespawn()
    {
        nv_SteamID.OnValueChanged -= OnSteamIDChanged;
        
        if (AllClients.ContainsKey(OwnerClientId))
            AllClients.Remove(OwnerClientId);
    }

    private void OnSteamIDChanged(ulong previous, ulong current)
    {
        steamName = new Friend(current).Name;
        gameObject.name = "Persistent Client: " + steamName;
    }
}
