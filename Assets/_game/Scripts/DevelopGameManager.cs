using TMPro;
using UnityEngine;
using System.Linq;
using AndrewDowsett.Utility;
using System;
using System.Collections;
using System.Collections.Generic;

public enum EDevSkill
{
    // Basic Skills
    Programming = 0,
    Writing,
    Design,
    UI,
    Art,
    Sound,
    Debugging,
    Marketing,
    Polishing,

    // Programming Expanded
    Programming_Engine = 10,
    Programming_Systems,
    Programming_AI,

    // Writing Expanded
    Writing_Story = 20,
    Writing_Characters,
    Writing_World,

    // Design Expanded
    Design_Environment = 30,
    Design_Characters,
    Design_Progression,

    // UI Expanded
    UI_UX = 40,
    UI_Interaction,
    UI_Backgrounds,

    // Art Expanded
    Art_Technical = 50,
    Art_2D,
    Art_3D,

    // Sound Expanded
    Sound_Music = 60,
    Sound_SFX,
    Sound_Environment
}

public class DevelopGameManager : MonoBehaviour
{
    private static DevelopGameManager Instance;
    private void Awake() => Instance = this;

    public const ulong GAME_PRICE_PER_STEP = 5;
    public static Action<Game> OnGameReleased;
    public static Action<GameDevStep> OnStartedNewStep;
    public static bool IsNotDevelopingGame() => Instance.developGameCoroutine == null;

    private Coroutine developGameCoroutine;
    private ulong developmentCost;
    private int previousPlatformIndex;

    public static string NextDefaultGameName() => Instance == null ? "New Game" : $"New Game #{SellingManager.GamesReleased + 1}";

    #region Dev Menu Panel
    [Header("Dev Panel References")]
    [SerializeField] private GameObject devMenuPanel;
    [SerializeField] private TMP_InputField gameNameInputField;
    [SerializeField] private TMP_Dropdown genreDropdown;
    [SerializeField] private TMP_Dropdown topicDropdown;
    [SerializeField] private TMP_Dropdown platformDropdown;
    [SerializeField] private Transform featureContentPanel;
    [SerializeField] private GameIdea currentGameIdea;
    [SerializeField] private TMP_Text totalCostText;

    [Header("Dev Panel Prefabs")]
    [SerializeField] private GameObject categoryTitlePrefab;
    [SerializeField] private Transform categoryContentPrefab;
    [SerializeField] private UIGameFeature categoryFeaturePrefab;

    public void DropdownChanged_Genre(int index) => currentGameIdea.Genre.Name = genreDropdown.options[index].text;
    public void DropdownChanged_Topic(int index) => currentGameIdea.Topic.Name = topicDropdown.options[index].text;
    public void DropdownChanged_Platform(int index)
    {
        Platform currentPlatform = GameManager.Instance.GetPlatformByName(platformDropdown.options[index].text);
        if (previousPlatformIndex >= 0)
        {
            RemoveDevelopmentCost(GameManager.Instance.GetPlatformByName(platformDropdown.options[previousPlatformIndex].text).Price);
        }
        previousPlatformIndex = index;
        currentGameIdea.Platform.Name = currentPlatform.Name;
        AddDevelopmentCost(currentPlatform.Price);
    }
    public void InputChanged_GameName(string name) => currentGameIdea.Name = name;

    public void StartDevelopment()
    {
        if (CheckForErrors())
            return;

        if (!CurrencyManager.RemoveCurrency(developmentCost))
        {
            PopupManager.Instance.ShowDialoguePanel("Error", "You cannot afford to develop this game!", EDialoguePanelType.OK);
            return;
        }

        CloseDevMenu();

        developGameCoroutine = StartCoroutine(DevelopGame(currentGameIdea));
    }

    private bool CheckForErrors()
    {
        if (SellingManager.HasReleasedGameWithName(currentGameIdea.Name))
        {
            PopupManager.Instance.ShowDialoguePanel("Error", "You already have a released game with that name!", EDialoguePanelType.OK);
            return true;
        }

        if (gameNameInputField.text == "")
        {
            PopupManager.Instance.ShowDialoguePanel("Error", "You need to name your game!", EDialoguePanelType.OK);
            return true;
        }

        if (currentGameIdea.Genre.Name == "Choose a Genre" || currentGameIdea.Genre.Name == "")
        {
            PopupManager.Instance.ShowDialoguePanel("Error", "Select a genre for your game!", EDialoguePanelType.OK);
            return true;
        }

        if (currentGameIdea.Topic.Name == "Choose a Topic" || currentGameIdea.Topic.Name == "")
        {
            PopupManager.Instance.ShowDialoguePanel("Error", "Select a topic for your game!", EDialoguePanelType.OK);
            return true;
        }

        if (currentGameIdea.Platform.Name == "Choose a Platform" || currentGameIdea.Platform.Name == "")
        {
            PopupManager.Instance.ShowDialoguePanel("Error", "Select a platform for your game!", EDialoguePanelType.OK);
            return true;
        }

        return false;
    }

    public static void OpenDevMenu()
    {
        if (Instance == null) return;

        Instance.ResetDevelopmentCost();

        Instance.currentGameIdea = new(NextDefaultGameName(), new Genre(""), new Topic(""), new Platform("", 0), new List<GameDevStep>
        {
            new GameDevStep("Prototype", "Creating a prototype", new List<GameDevTask>
            {
                new GameDevTask("Desiging the systems", EDevSkill.Design, 2),
                new GameDevTask("Coding the game logic", EDevSkill.Programming, 2),
                new GameDevTask("Writing the story", EDevSkill.Writing, 2),
            }),
            new GameDevStep("Vertical Slice", "Create a polished small part of your game", new List<GameDevTask>
            {
                new GameDevTask("Desiging and implementing the UI", EDevSkill.UI, 2),
                new GameDevTask("Working on the game art", EDevSkill.Art, 2),
                new GameDevTask("Making noises into a microphone", EDevSkill.Sound, 2),
            }),
            new GameDevStep("Final Product", "Creating the final product", new List<GameDevTask>
            {
                new GameDevTask("Removing all those pesky bugs", EDevSkill.Debugging, 5),
                new GameDevTask("Polishing the game", EDevSkill.Polishing, 5),
                new GameDevTask("Marketing the game on social media", EDevSkill.Marketing, 10),
            }),
        });
        Instance.gameNameInputField.text = NextDefaultGameName();

        Instance.genreDropdown.ClearOptions();
        Instance.genreDropdown.options.Add(new TMP_Dropdown.OptionData("Choose a Genre"));
        Instance.genreDropdown.AddOptions(GameManager.Instance.GenreNames.ToList());

        Instance.topicDropdown.ClearOptions();
        Instance.topicDropdown.options.Add(new TMP_Dropdown.OptionData("Choose a Topic"));
        Instance.topicDropdown.AddOptions(GameManager.Instance.TopicNames.ToList());

        Instance.platformDropdown.ClearOptions();
        Instance.platformDropdown.options.Add(new TMP_Dropdown.OptionData("Choose a Platform"));
        Instance.platformDropdown.AddOptions(GameManager.Instance.PlatformNames.ToList());

        // FEATURE PANEL
        for (int i = Instance.featureContentPanel.childCount - 1; i >= 0; i--)
        {
            Destroy(Instance.featureContentPanel.GetChild(i).gameObject);
        }

        Feature[] features = GameManager.Instance.UnlockedFeatures;
        string[] categories = features.Select(feature => feature.SkillCategory.ToString()).Distinct().ToArray();
        for (int i = 0; i < categories.Length; i++)
        {
            GameObject categoryTitle = Instantiate(Instance.categoryTitlePrefab, Instance.featureContentPanel);
            categoryTitle.GetComponentInChildren<TMP_Text>().text = categories[i];
            Transform categoryContent = Instantiate(Instance.categoryContentPrefab, Instance.featureContentPanel);
            for (int j = 0; j < features.Length; j++)
            {
                if (features[j].SkillCategory.ToString() == categories[i])
                {
                    UIGameFeature gameFeature = Instantiate(Instance.categoryFeaturePrefab, categoryContent);
                    gameFeature.Set(features[j]);
                    gameFeature.AssignToggle(features[j].Name);
                }
            }
        }
        ////////////////

        Instance.devMenuPanel.SetActive(true);
    }

    public void CloseDevMenu() => devMenuPanel.SetActive(false);

    public void AddDevelopmentCost(ulong cost)
    {
        developmentCost += cost;
        UpdateCost();
    }

    public void RemoveDevelopmentCost(ulong cost)
    {
        developmentCost -= cost;
        UpdateCost();
    }

    private void ResetDevelopmentCost()
    {
        developmentCost = 0;
        previousPlatformIndex = 0;
        UpdateCost();
    }

    public static void AddFeature(Feature feature)
    {
        Instance.currentGameIdea.Features.Add(feature);
        Instance.AddDevelopmentCost(feature.Cost);
        Debug.Log($"Added {feature.Name}. . .");
    }

    public static void RemoveFeature(Feature feature)
    {
        string name = feature.Name;
        for (int i = Instance.currentGameIdea.Features.Count - 1; i >= 0; i--)
        {
            if (Instance.currentGameIdea.Features[i].Name == name)
            {
                Instance.currentGameIdea.Features.RemoveAt(i);
                Instance.RemoveDevelopmentCost(feature.Cost);
                Debug.Log($"Removed {feature.Name}. . .");
                break;
            }
        }
    }

    private void UpdateCost()
    {
        totalCostText.text = $"$ {Utilities.GetLongCostString(developmentCost)}";
    }
    #endregion

    private IEnumerator DevelopGame(GameIdea gameIdea)
    {
        yield return new WaitForSeconds(1.0f);
        Debug.Log($"Begin Development of a {gameIdea.Genre} game named {gameIdea.Name}.\nSteps: {gameIdea.GameDevSteps.Count}\nTotal Time: {gameIdea.TotalTimeToComplete} seconds.\nFeatures: {gameIdea.Features.Count}");

        GameDevStep step = gameIdea.GameDevSteps[0];
        GameDevTask task = step.Tasks[0];
        ulong price = GAME_PRICE_PER_STEP * (ulong)gameIdea.GameDevSteps.Count;

        UIStepPanel.OnTaskValueChanged += OnTaskValueChanged;

        while (gameIdea.GameDevSteps.Count > 0)
        {
            if (!TimeManager.Instance.IsPaused())
            {
                step = gameIdea.GameDevSteps[0];
                if (!Character.LocalCharacter.HasTask())
                {
                    if (step.Tasks.Count > 0) // There are still tasks, so assign the new task to the character
                    {
                        task = step.Tasks[0];
                        Character.LocalCharacter.AssignTask(new Task(task.Name, task.TimeToComplete, null));
                        gameIdea.GameDevSteps[0].Tasks.RemoveAt(0);
                    }
                    else // We have finished that step, and we need a new step
                    {
                        yield return new WaitForSeconds(0.1f);

                        gameIdea.GameDevSteps.RemoveAt(0);

                        if (gameIdea.GameDevSteps.Count > 0)
                            OnStartedNewStep?.Invoke(gameIdea.GameDevSteps[0]);

                        yield return new WaitForSeconds(1f);
                    }
                }
            }
            yield return null;
        }

        UIStepPanel.OnTaskValueChanged -= OnTaskValueChanged;

        developGameCoroutine = null;
        Game game = gameIdea;
        game.Price = price;
        OnGameReleased?.Invoke(game);
    }

    private void OnTaskValueChanged(int taskIndex, float value)
    {
        GameDevTask task = currentGameIdea.GameDevSteps[0].Tasks[taskIndex];
        task.UpdateContribution(value);
        currentGameIdea.GameDevSteps[0].Tasks[taskIndex] = task;
    }

    public static void CancelDevelopment()
    {
        if (Instance.developGameCoroutine != null)
            Instance.StopCoroutine(Instance.developGameCoroutine);
        Instance.currentGameIdea = default;
    }
}

public struct GameIdea
{
    //public static implicit operator GameIdea(Game a) => new GameIdea(a.Name, a.Genre);

    public string Name;
    public Genre Genre;
    public Topic Topic;
    public Platform Platform;
    public List<Feature> Features;
    public List<GameDevStep> GameDevSteps;

    public float TotalTimeToComplete;

    public GameIdea(string name, Genre genre, Topic topic, Platform platform, List<GameDevStep> gameDevSteps)
    {
        Name = name;
        Genre = genre;
        Topic = topic;
        Platform = platform;
        GameDevSteps = gameDevSteps;
        
        TotalTimeToComplete = gameDevSteps.Sum(step => step.Tasks.Sum(task => task.TimeToComplete));

        Features = new();
    }
}

public struct GameDevStep
{
    public string Name;
    public string Description;
    public List<GameDevTask> Tasks;

    public GameDevStep(string name, string description, List<GameDevTask> tasks)
    {
        Name = name;
        Description = description;
        Tasks = tasks;
    }
}

public struct GameDevTask
{
    public string Name;
    public EDevSkill Skills;
    public float TimeToComplete;
    public float MyContributionPercentage;

    private int timeToComplete;

    public GameDevTask(string name, EDevSkill skills, int timeToComplete)
    {
        Name = name;
        Skills = skills;
        this.timeToComplete = timeToComplete;
        TimeToComplete = timeToComplete;
        MyContributionPercentage = 1f;
    }

    public void UpdateContribution(float value)
    {
        MyContributionPercentage = 1f - value;
        float costMultiplier = MyContributionPercentage.Remap(0, 1, 0.5f, 1f);
        TimeToComplete = timeToComplete * costMultiplier;
    }
}