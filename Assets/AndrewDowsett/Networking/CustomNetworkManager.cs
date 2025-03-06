using Unity.Netcode;
using UnityEngine;

public class CustomNetworkManager : NetworkManager
{
    public static CustomNetworkManager Instance;
    public GameObject customNetworkManagerPrefab;
    public RoomData roomData;
    
    public void Init()
    {
        Instance = this;
        SetSingleton();
        OnClientStopped += OnServerStoppedHandler;
        OnClientConnectedCallback += OnClientConnected;
    }

    void OnClientConnected(ulong id)
    {
        
    }
    
    private void OnServerStoppedHandler(bool isHostClient)
    {
        
    }
}
