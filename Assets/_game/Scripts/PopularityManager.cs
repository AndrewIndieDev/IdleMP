using AndrewDowsett.CommonObservers;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class PopularityManager : MonoBehaviour, IUpdateObserver
{
    [SerializeField] private UILineRenderer lineGraph;
    [SerializeField] private TMP_Dropdown genreDropdown;
    [SerializeField] private float popularityUpdateInterval;
    [SerializeField] private float maxPopularityChangePerInterval;
    [SerializeField] private float popularityChangeSpeed;

    private Genre currentlySelectedGenre;
    private float popularityUpdateTimer;
    private List<float> genrePopularityHistory = new() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

    private void Start()
    {
        UpdateManager.RegisterObserver(this);
        TimeManager.OnQuarterlyUpdate += OnQuarterlyUpdate;

        genreDropdown.ClearOptions();
        genreDropdown.AddOptions(GameManager.Instance.GenreNames.ToList());

        currentlySelectedGenre = GameManager.Instance.Genres[0];
        OnQuarterlyUpdate();
    }

    private void OnDestroy()
    {
        UpdateManager.UnregisterObserver(this);
        TimeManager.OnQuarterlyUpdate -= OnQuarterlyUpdate;
    }

    public void ObservedUpdate(float deltaTime)
    {
        if (TimeManager.Instance.IsPaused())
            return;

        popularityUpdateTimer += deltaTime;
        if (popularityUpdateTimer >= popularityUpdateInterval)
        {
            popularityUpdateTimer -= popularityUpdateInterval;
            for (int i = 0; i < GameManager.Instance.Genres.Length; i++)
            {
                GameManager.Instance.Genres[i].Popularity[GameManager.Instance.Genres[i].Popularity.Length - 1]
                = Mathf.Clamp(GameManager.Instance.Genres[i].Popularity[GameManager.Instance.Genres[i].Popularity.Length - 1]
                    + Random.Range(-maxPopularityChangePerInterval, maxPopularityChangePerInterval), 0f, 10f);
            }
        }

        List<Vector2> points = new();
        for (int i = 0; i < genrePopularityHistory.Count; i++)
        {
            if (genrePopularityHistory[i] != currentlySelectedGenre.Popularity[i])
            {
                genrePopularityHistory[i] = Mathf.Lerp(genrePopularityHistory[i], currentlySelectedGenre.Popularity[i], deltaTime * popularityChangeSpeed);
            }
            points.Add(new Vector2(i, genrePopularityHistory[i]));
        }
        lineGraph.UpdatePoints(points);
    }

    private void OnQuarterlyUpdate()
    {
        int popularityLength = GameManager.Instance.Genres[0].Popularity.Length;
        List<Vector2> points = new();
        foreach (Genre genre in GameManager.Instance.Genres)
        {
            for (int i = 0; i < popularityLength; i++)
            {
                if (i + 1 < popularityLength)
                    genre.Popularity[i] = genre.Popularity[i + 1];
                if (genre.Name == currentlySelectedGenre.Name)
                    points.Add(new Vector2(i, genre.Popularity[i]));
            }
        }
        lineGraph.UpdatePoints(points);
    }

    public void OnGenreChanged(int index)
    {
        currentlySelectedGenre = GameManager.Instance.Genres[index];
    }
}
