using AndrewDowsett.Utility;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        RandomizePopularity();
    }

    #region Popups
    public bool HasPopups() => Popups != null && Popups.Count > 0;
    public void AddPopup(Popup popup) => Popups?.Add(popup);
    public void RemovePopup(Popup popup) => Popups?.Remove(popup);
    private List<Popup> Popups = new();
    #endregion

    #region Genres
    public Genre[] Genres => genres;
    public string[] GenreNames => genres.Select(x => x.Name).ToArray();
    public Genre GetGenreByName(string name)
    {
        foreach (var genre in genres)
        {
            if (genre.Name == name)
                return genre;
        }
        return default;
    }
    [SerializeField] private Genre[] genres;
    private void RandomizePopularity()
    {
        foreach (var genre in genres)
        {
            for (int i = 0; i < genre.Popularity.Length; i++)
            {
                if (i == 0)
                    genre.Popularity[i] = Random.Range(3f, 7f);
                else
                    genre.Popularity[i] = Mathf.Clamp(genre.Popularity[i - 1] + Random.Range(-1f, 1f), 0f, 10f);
            }
        }
    }
    #endregion

    #region Topics
    public Topic[] Topics => topics;
    public string[] TopicNames => topics.Select(x => x.Name).ToArray();
    public Topic GetTopicByName(string name)
    {
        foreach (var topic in topics)
        {
            if (topic.Name == name)
                return topic;
        }
        return default;
    }
    [SerializeField] private Topic[] topics;
    #endregion

    #region Platforms
    public Platform[] Platforms => platforms;
    public string[] PlatformNames => platforms.Select(x => x.Name).ToArray();
    public Platform GetPlatformByName(string name)
    {
        foreach (var platform in platforms)
        {
            if (platform.Name == name)
                return platform;
        }
        return default;
    }
    [SerializeField] private Platform[] platforms;
    #endregion

    #region Features
    public Feature[] AllFeatures => features;
    public Feature[] UnlockedFeatures => features.Where(x => x.IsUnlocked).ToArray();
    public Feature GetFeatureByName(string name)
    {
        foreach (var feature in features)
        {
            if (feature.Name == name)
                return feature;
        }
        return default;
    }
    [SerializeField] private Feature[] features;
    #endregion

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Quit();
        }
    }

    #region Quitting
    public void Quit()
    {
        PopupManager.Instance.ShowDialoguePanel("Quit Game?", "Are you sure you want to quit?", EDialoguePanelType.CustomTwoButtons, false, OnResultButtonClicked, "Yes", "No");
    }
    private void OnResultButtonClicked(EDialogueResult result, string x)
    {
        if (result == EDialogueResult.CustomOne) // Yes
        {
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
    #endregion
}
