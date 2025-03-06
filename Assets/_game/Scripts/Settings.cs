using QFSW.QC;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum ELanguage
{
    English,
    //German,
    //French,
    //Spanish,
    //Portuguese,
    //Russian,
    //Italian,
    //Simplified_Chinese,
    //Japanese,
    //Korean,
    //Polish
}

public enum EResolution
{
    p720,
    p1080,
    p1200,
    p1440,
    p1600
}

public class Settings : MonoBehaviour
{
    public static Settings Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    [Command]
    private void Check_Language()
    {
        DebugSetting("Language", Language.ToString(), ((ELanguage)PlayerPrefs.GetInt("Language")).ToString());
    }
    public void SetLanguage(int languageIndex)
    {
        Language = (ELanguage)languageIndex;
    }
    public ELanguage Language
    {
        get => language;
        set
        {
            language = value;
            PlayerPrefs.SetInt("Language", (int)value);
        }
    }
    private ELanguage language = ELanguage.English;
    [SerializeField] private TMP_Dropdown languageDropdown;

    [Command]
    private void Check_VSync()
    {
        DebugSetting("VSync", IsVSyncOn.ToString(), (PlayerPrefs.GetInt("IsVSyncOn") == 1).ToString(), QualitySettings.vSyncCount.ToString());
    }
    public void SetIsVSyncOn(bool isOn)
    {
        IsVSyncOn = isOn;
    }
    public bool IsVSyncOn
    {
        get => isVSyncOn;
        set
        {
            isVSyncOn = value;
            QualitySettings.vSyncCount = value ? 1 : 0;
            PlayerPrefs.SetInt("IsVSyncOn", value ? 1 : 0);
        }
    }
    private bool isVSyncOn = false;
    [SerializeField] private Toggle vSyncToggle;

    [Command]
    private void Check_Fullscreen()
    {
        DebugSetting("Fullscreen", IsFullscreen.ToString(), (PlayerPrefs.GetInt("IsFullscreen") == 1).ToString(), Screen.fullScreen.ToString());
    }
    public void SetIsFullscreen(bool isOn)
    {
        IsFullscreen = isOn;
    }
    public bool IsFullscreen
    {
        get => Screen.fullScreen;
        set
        {
            Screen.fullScreen = value;
            PlayerPrefs.SetInt("IsFullscreen", value ? 1 : 0);
        }
    }
    [SerializeField] private Toggle fullscreenToggle;

    [Command]
    private void Check_Resolution()
    {
        DebugSetting("Resolution", Resolution.ToString(), ((EResolution)PlayerPrefs.GetInt("Resolution")).ToString(), $"{Screen.currentResolution.width}x{Screen.currentResolution.height}");
    }
    public void SetResolution(int resolutionIndex)
    {
        Resolution = (EResolution)resolutionIndex;
    }
    public EResolution Resolution
    {
        get => resolution;
        set
        {
            resolution = value;
            int height = screenHeightList[(int)resolution];
            int width = screenWidthList[(int)resolution];
            Screen.SetResolution(width, height, IsFullscreen);
            PlayerPrefs.SetInt("Resolution", (int)value);
        }
    }
    private EResolution resolution = EResolution.p1080;
    [SerializeField] private TMP_Dropdown resolutionDropdown;

    [Command]
    private void Check_StreamerMode()
    {
        DebugSetting("StreamerMode", IsStreamerModeOn.ToString(), (PlayerPrefs.GetInt("IsStreamerModeOn") == 1).ToString());
    }
    public void SetStreamerModeOn(bool isOn)
    {
        IsStreamerModeOn = isOn;
    }
    public bool IsStreamerModeOn
    {
        get => isStreamerModeOn;
        set
        {
            isStreamerModeOn = value;
            PlayerPrefs.SetInt("IsStreamerModeOn", value ? 1 : 0);
        }
    }
    private bool isStreamerModeOn = false;
    [SerializeField] private Toggle streamerModeToggle;

    [Command]
    private void Check_SoundMuted()
    {
        DebugSetting("SoundMuted", IsSoundMuted.ToString(), (PlayerPrefs.GetInt("IsSoundMuted") == 1).ToString());
    }
    public void SetSoundMuted(bool isMuted)
    {
        IsSoundMuted = isMuted;
    }
    public bool IsSoundMuted
    {
        get => isSoundMuted;
        set
        {
            isSoundMuted = value;
            PlayerPrefs.SetInt("IsSoundMuted", value ? 1 : 0);
            if (isSoundMuted)
                AudioManager.Instance.MuteAll();
            else
                AudioManager.Instance.UnMuteAll();
        }
    }
    private bool isSoundMuted = false;
    [SerializeField] private Toggle soundMutedToggle;

    [Command]
    private void Check_MasterVolume()
    {
        DebugSetting("MasterVolume", MasterVolume.ToString(), PlayerPrefs.GetFloat("MasterVolume").ToString());
    }
    public void SetMasterVolume(float volume)
    {
        MasterVolume = volume;
    }
    public float MasterVolume
    {
        get => masterVolume;
        set
        {
            masterVolume = value;
            PlayerPrefs.SetFloat("MasterVolume", value);
            AudioManager.Instance.SetVolume(EMixerGroup.Master, value);
        }
    }
    private float masterVolume = 1;
    [SerializeField] private Slider masterVolumeSlider;

    [Command]
    private void Check_MusicVolume()
    {
        DebugSetting("MusicVolume", MusicVolume.ToString(), PlayerPrefs.GetFloat("MusicVolume").ToString());
    }
    public void SetMusicVolume(float volume)
    {
        MusicVolume = volume;
    }
    public float MusicVolume
    {
        get => musicVolume;
        set
        {
            musicVolume = value;
            PlayerPrefs.SetFloat("MusicVolume", value);
            AudioManager.Instance.SetVolume(EMixerGroup.Music, value);
        }
    }
    private float musicVolume = 1;
    [SerializeField] private Slider musicVolumeSlider;

    [Command]
    private void Check_SfxVolume()
    {
        DebugSetting("SFXVolume", SfxVolume.ToString(), PlayerPrefs.GetFloat("SFXVolume").ToString());
    }
    public void SetSfxVolume(float volume)
    {
        SfxVolume = volume;
    }
    public float SfxVolume
    {
        get => sfxVolume;
        set
        {
            sfxVolume = value;
            PlayerPrefs.SetFloat("SFXVolume", value);
            AudioManager.Instance.SetVolume(EMixerGroup.SFX, value);
        }
    }
    private float sfxVolume = 1;
    [SerializeField] private Slider sfxVolumeSlider;

    [Command]
    private void Check_AnnouncerVolume()
    {
        DebugSetting("AnnouncerVolume", AnnouncerVolume.ToString(), PlayerPrefs.GetFloat("AnnouncerVolume").ToString());
    }
    public void SetAnnouncerVolume(float volume)
    {
        AnnouncerVolume = volume;
    }
    public float AnnouncerVolume
    {
        get => announcerVolume;
        set
        {
            announcerVolume = value;
            PlayerPrefs.SetFloat("AnnouncerVolume", value);
            AudioManager.Instance.SetVolume(EMixerGroup.Announcer, value);
        }
    }
    private float announcerVolume = 1;
    [SerializeField] private Slider announcerVolumeSlider;

    [Command]
    private void Check_UIVolume()
    {
        DebugSetting("UIVolume", UIVolume.ToString(), PlayerPrefs.GetFloat("UIVolume").ToString());
    }
    public void SetUIVolume(float volume)
    {
        UIVolume = volume;
    }
    public float UIVolume
    {
        get => uiVolume;
        set
        {
            uiVolume = value;
            PlayerPrefs.SetFloat("UIVolume", value);
            AudioManager.Instance.SetVolume(EMixerGroup.UI, value);
        }
    }
    private float uiVolume = 1;
    [SerializeField] private Slider uiVolumeSlider;

    //public void SetActionConfirmationOn(bool isOn)
    //{
    //    IsActionConfirmationOn = isOn;
    //}
    //public bool IsActionConfirmationOn
    //{
    //    get => isActionConfirmationOn;
    //    set
    //    {
    //        isActionConfirmationOn = value;
    //        PlayerPrefs.SetInt("IsActionConfirmationOn", value ? 1 : 0);
    //    }
    //}
    //[SerializeField] private bool isActionConfirmationOn = true;
    //[SerializeField] private Toggle actionConfirmationToggle;

    private List<int> screenWidthList = new List<int> { 1280, 1920, 1920, 2560, 2560 };
    private List<int> screenHeightList = new List<int> { 720, 1080, 1200, 1440, 1600 };

    private void Start()
    {
        LoadGameSettings();
        LoadSoundSettings();

        languageDropdown.onValueChanged.AddListener(SetLanguage);
        vSyncToggle.onValueChanged.AddListener(SetIsVSyncOn);
        fullscreenToggle.onValueChanged.AddListener(SetIsFullscreen);
        //actionConfirmationToggle.onValueChanged.AddListener(SetActionConfirmationOn);
        resolutionDropdown.onValueChanged.AddListener(SetResolution);
        streamerModeToggle.onValueChanged.AddListener(SetStreamerModeOn);
        soundMutedToggle.onValueChanged.AddListener(SetSoundMuted);
        masterVolumeSlider.onValueChanged.AddListener(SetMasterVolume);
        musicVolumeSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxVolumeSlider.onValueChanged.AddListener(SetSfxVolume);
        announcerVolumeSlider.onValueChanged.AddListener(SetAnnouncerVolume);
        uiVolumeSlider.onValueChanged.AddListener(SetUIVolume);
    }

    [Command]
    public void LoadSoundSettings()
    {
        if (PlayerPrefs.HasKey("IsSoundMuted"))
        {
            isSoundMuted = PlayerPrefs.GetInt("IsSoundMuted") == 1;
            soundMutedToggle.isOn = isSoundMuted;
            if (isSoundMuted)
                AudioManager.Instance.MuteAll();
        }
        else
        {
            isSoundMuted = false;
            soundMutedToggle.isOn = false;
        }

        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            masterVolume = PlayerPrefs.GetFloat("MasterVolume");
            masterVolumeSlider.value = masterVolume;
            AudioManager.Instance.SetVolume(EMixerGroup.Master, masterVolume);
        }
        else
        {
            masterVolume = 1f;
            masterVolumeSlider.value = 1f;
        }

        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            musicVolume = PlayerPrefs.GetFloat("MusicVolume");
            musicVolumeSlider.value = musicVolume;
            AudioManager.Instance.SetVolume(EMixerGroup.Music, musicVolume);
        }
        else
        {
            musicVolume = 1f;
            musicVolumeSlider.value = 1f;
        }

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume");
            sfxVolumeSlider.value = sfxVolume;
            AudioManager.Instance.SetVolume(EMixerGroup.SFX, sfxVolume);
        }
        else
        {
            sfxVolume = 1f;
            sfxVolumeSlider.value = 1f;
        }

        if (PlayerPrefs.HasKey("AnnouncerVolume"))
        {
            announcerVolume = PlayerPrefs.GetFloat("AnnouncerVolume");
            announcerVolumeSlider.value = announcerVolume;
            AudioManager.Instance.SetVolume(EMixerGroup.Announcer, announcerVolume);
        }
        else
        {
            announcerVolume = 1f;
            announcerVolumeSlider.value = 1f;
        }

        if (PlayerPrefs.HasKey("UIVolume"))
        {
            uiVolume = PlayerPrefs.GetFloat("UIVolume");
            uiVolumeSlider.value = uiVolume;
            AudioManager.Instance.SetVolume(EMixerGroup.UI, uiVolume);
        }
        else
        {
            uiVolume = 1f;
            uiVolumeSlider.value = 1f;
        }
    }

    [Command]
    public void LoadGameSettings()
    {
        if (PlayerPrefs.HasKey("Language"))
        {
            language = ELanguage.English;
            languageDropdown.value = PlayerPrefs.GetInt("Language");
        }
        else
        {
            language = ELanguage.English;
            languageDropdown.value = 0;
        }

        if (PlayerPrefs.HasKey("IsVSyncOn"))
        {
            isVSyncOn = PlayerPrefs.GetInt("IsVSyncOn") == 1;
            vSyncToggle.isOn = isVSyncOn;
        }
        else
        {
            isVSyncOn = false;
            vSyncToggle.isOn = false;
        }

        if (PlayerPrefs.HasKey("IsFullscreen"))
        {
            bool fullscreen = PlayerPrefs.GetInt("IsFullscreen") == 1;
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = fullscreen;
            fullscreenToggle.isOn = fullscreen;
        }
        else
        {
            Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
            Screen.fullScreen = true;
            fullscreenToggle.isOn = true;
        }

        if (PlayerPrefs.HasKey("Resolution"))
        {
            int res = PlayerPrefs.GetInt("Resolution");
            int height = screenHeightList[res];
            int width = screenWidthList[res];
            Screen.SetResolution(width, height, IsFullscreen);
            resolution = (EResolution)res;
            resolutionDropdown.value = res;
        }
        else
        {
            int res = Screen.currentResolution.width;
            int closestToRes = 999999;
            EResolution closestResolution = EResolution.p1080;
            for (int i = 0; i < screenWidthList.Count; i++)
            {
                int checkedRes = screenWidthList[i];
                int abs = Mathf.Abs(res - checkedRes);
                if (abs < closestToRes)
                {
                    closestToRes = abs;
                    closestResolution = (EResolution)i;
                }
            }
            resolution = closestResolution;
            resolutionDropdown.value = (int)closestResolution;
        }

        if (PlayerPrefs.HasKey("IsStreamerModeOn"))
        {
            isStreamerModeOn = PlayerPrefs.GetInt("IsStreamerModeOn") == 1;
            streamerModeToggle.isOn = isStreamerModeOn;
        }
        else
        {
            isStreamerModeOn = false;
            streamerModeToggle.isOn = false;
        }
    }

    [Command]
    public void ResetSettingsToDefault()
    {
        PlayerPrefs.DeleteAll();
        soundMutedToggle.isOn = false;
        masterVolumeSlider.value = 1f;
        musicVolumeSlider.value = 1f;
        sfxVolumeSlider.value = 1f;
        announcerVolumeSlider.value = 1f;
        uiVolumeSlider.value = 1f;
        vSyncToggle.isOn = false;
        fullscreenToggle.isOn = true;
        
        ////////////////// FOR RESOLUTION
        int res = Screen.currentResolution.width;
        int closestToRes = 999999;
        EResolution closestResolution = EResolution.p1080;
        for (int i = 0; i < screenWidthList.Count; i++)
        {
            int checkedRes = screenWidthList[i];
            int abs = Mathf.Abs(res - checkedRes);
            if (abs < closestToRes)
            {
                closestToRes = abs;
                closestResolution = (EResolution)i;
            }
        }
        resolutionDropdown.value = (int)closestResolution;
        ///////////////////
        
        streamerModeToggle.isOn = false;
    }

    private void DebugSetting(string setting, string current, string saved, string actualSetting = "")
    {
        string settingName = $"=========={setting}==========";
        int charCount = settingName.Length;
        string ending = new string('=', charCount);
        Debug.Log(settingName);
        Debug.Log($"Current: {current}");
        Debug.Log($"Saved: {saved}");
        if (actualSetting.Length > 0)
            Debug.Log($"Actual: {actualSetting}");
        Debug.Log(ending);
    }
}
