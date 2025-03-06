using AndrewDowsett.Utility;
using Cysharp.Threading.Tasks;
using QFSW.QC;
using UnityEngine;
using UnityEngine.Audio;

public enum EMixerGroup
{
    Master,
    Music,
    Announcer,
    SFX,
    UI,

    COUNT
}

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    [Header("Mixer Groups")]
    public AudioMixer mixer;
    public AudioMixerGroup masterGroup;
    public AudioMixerGroup musicGroup;
    public AudioMixerGroup announcerGroup;
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup uiGroup;

    private bool masterMuted;
    private bool musicMuted;
    private bool announcerMuted;
    private bool sfxMuted;
    private bool uiMuted;

    [Header("Clips")]
    public AudioClip introMusic;
    public AudioClip lobbyMusic;
    public AudioClip gameMusic;

    [Header("Debug")]
    [SerializeField] private bool muteAllSounds;

    private AudioSource currentMusic;

    private void Start()
    {
        // THIS STARTS THE INTRO MUSIC WHEN OPENING THE GAME
        PlaySound(introMusic, EMixerGroup.Music, doLoop: true);
    }

    public AudioSource PlaySound(AudioClip audioClip, EMixerGroup mixerGroup, float pitch = 1.0f, float volume = 1.0f, bool doLoop = false)
    {
#if UNITY_EDITOR
        if (muteAllSounds)
            return null;
#endif

        if (audioClip != null)
        {
            GameObject sound = new("Sound_" + audioClip.name);
            sound.transform.SetParent(transform);
            AudioSource source = sound.AddComponent<AudioSource>();
            source.clip = audioClip;
            source.pitch = pitch;
            source.volume = volume;
            source.loop = doLoop;
            source.outputAudioMixerGroup = GetMixerGroup(mixerGroup);
            source.Play();
            if (!doLoop)
                Destroy(sound, audioClip.length / Mathf.Max(pitch, 0.0001f));
            if (mixerGroup == EMixerGroup.Music)
            {
                if (currentMusic != null)
                    SwitchMusic(source, volume);
                else
                {
                    currentMusic = source;
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    FadeAudioTo(source, 0.5f, volume);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                }
            }
            return source;
        }
        else
        {
            if (mixerGroup == EMixerGroup.Music)
            {
                if (currentMusic != null)
                    SwitchMusic(null);
            }
            return null;
        }
    }

    public AudioMixerGroup GetMixerGroup(EMixerGroup mixerGroup)
    {
        switch (mixerGroup)
        { 
            case EMixerGroup.Master:
                return masterGroup;
            case EMixerGroup.Music:
                return musicGroup;
            case EMixerGroup.Announcer:
                return announcerGroup;
            case EMixerGroup.SFX:
                return sfxGroup;
            case EMixerGroup.UI:
                return uiGroup;
        }
        return null;
    }

    private async void SwitchMusic(AudioSource newMusic, float volume = 0.5f)
    {
        Debug.Log("fading out current music");
        await FadeAudioTo(currentMusic, 0.5f, 0f);
        Debug.Log("destroying current music");
        Destroy(currentMusic.gameObject);
        currentMusic = newMusic;
        Debug.Log("fading in new music");
        await  FadeAudioTo(currentMusic, 0.5f, volume);
        Debug.Log("finshed");
    }

    public async UniTask FadeAudioTo(AudioSource source, float duration, float toVolume)
    {
        if (source != null)
        {
            float i = 0f;
            float fromVolume = source.volume;
            while (i < duration)
            {
                i += Time.deltaTime;
                source.volume = Mathf.Abs(i.Remap(0f, duration, fromVolume, toVolume));
                await UniTask.Yield();
            }
        }
    }

    [Command]
    public void SetVolume(EMixerGroup mixerGroup, float volumePercentage)
    {
        switch (mixerGroup)
        {
            case EMixerGroup.Master:
                if (masterMuted) return;
                break;
            case EMixerGroup.Music:
                if (musicMuted) return;
                break;
            case EMixerGroup.Announcer:
                if (announcerMuted) return;
                break;
            case EMixerGroup.SFX:
                if (sfxMuted) return;
                break;
            case EMixerGroup.UI:
                if (uiMuted) return;
                break;
        }

        float volume = volumePercentage.Remap(0f, 1f, -40f, 0f);
        if (volumePercentage == 0f)
            volume = -80f;
        AudioMixerGroup group = GetMixerGroup(mixerGroup);
        group.audioMixer.SetFloat(mixerGroup.ToString(), volume);
    }

    public void MuteAll()
    {
        for (int i = 0; i < (int)EMixerGroup.COUNT; i++)
        {
            switch ((EMixerGroup)i)
            {
                case EMixerGroup.Master:
                    SetVolume(EMixerGroup.Master, 0);
                    masterMuted = true;
                    break;
                case EMixerGroup.Music:
                    SetVolume(EMixerGroup.Music, 0);
                    musicMuted = true;
                    break;
                case EMixerGroup.Announcer:
                    SetVolume(EMixerGroup.Announcer, 0);
                    announcerMuted = true;
                    break;
                case EMixerGroup.SFX:
                    SetVolume(EMixerGroup.SFX, 0);
                    sfxMuted = true;
                    break;
                case EMixerGroup.UI:
                    SetVolume(EMixerGroup.UI, 0);
                    uiMuted = true;
                    break;
            }
        }
    }

    public void UnMuteAll()
    {
        for (int i = 0; i < (int)EMixerGroup.COUNT; i++)
        {
            switch ((EMixerGroup)i)
            {
                case EMixerGroup.Master:
                    SetVolume(EMixerGroup.Master, Settings.Instance.MasterVolume);
                    masterMuted = false;
                    break;
                case EMixerGroup.Music:
                    SetVolume(EMixerGroup.Music, Settings.Instance.MusicVolume);
                    musicMuted = false;
                    break;
                case EMixerGroup.Announcer:
                    SetVolume(EMixerGroup.Announcer, Settings.Instance.AnnouncerVolume);
                    announcerMuted = false;
                    break;
                case EMixerGroup.SFX:
                    SetVolume(EMixerGroup.SFX, Settings.Instance.SfxVolume);
                    sfxMuted = false;
                    break;
                case EMixerGroup.UI:
                    SetVolume(EMixerGroup.UI, Settings.Instance.UIVolume);
                    uiMuted = false;
                    break;
            }
        }
    }
}
