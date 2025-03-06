using AndrewDowsett.CommonObservers;
using Cysharp.Threading.Tasks;
using Steamworks;
using System;
using UnityEngine;

public class SteamManager : MonoBehaviour, IUpdateObserver
{
    public static SteamManager Instance { get; set; }
    public const uint AppId = 480;
    [HideInInspector] public bool steamSucceeded = false;

    void CheckIfLaunchedOutsideSteam()
    {
        try
        {
            if (SteamClient.RestartAppIfNecessary(AppId))
            {
                Application.Quit();
            }
        }
        catch (Exception)
        {
            Application.Quit();
        }
    }

    public async UniTask Init()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        try
        {
            SteamClient.Init(AppId, false);
            CheckIfLaunchedOutsideSteam();
            steamSucceeded = true;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Steamworks failed to initialize: " + e.Message);
            // Something went wrong! Steam is closed?
        }

        if (steamSucceeded)
        {
            UpdateManager.RegisterObserver(this);
            Instance = this;
            Debug.Log($"Player name: {SteamClient.Name}, SteamID: {SteamClient.SteamId}, From: {SteamUtils.IpCountry}");
        }

        await UniTask.Delay(1000);
    }

    public void ObservedUpdate(float deltaTime)
    {
        SteamClient.RunCallbacks();
    }

    private void OnApplicationQuit()
    {
        SteamFriends.ClearRichPresence();
        SteamClient.Shutdown();
    }
}
