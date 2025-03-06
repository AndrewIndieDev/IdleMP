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
