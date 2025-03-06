using TMPro;
using UnityEngine;
using System.Linq;
using AndrewDowsett.Utility;
using System;
using System.Collections;
using System.Collections.Generic;

[Flags]
public enum EDevSkill
{
    // Early Game
    Programming,
    Writing,
    Design,
    
    // Early-Mid Game
    UI,
    Art,
    Sound,

    //// Mid Game
    //LevelDesign,
    //Animation,
    //Marketing,

    //// Mid-Late Game
    //Gameplay,
    //QATesting,
    //ArtificialIntelligence,

    //// Late Game
    //Networking,
    //Monetisation,
    //ServerInfrastructure
}

public class DevelopGameManager : MonoBehaviour
{
    private static DevelopGameManager Instance;
    private void Awake() => Instance = this;

    public const ulong GAME_PRICE_PER_STEP = 5;
    public static Action<Game> OnGameReleased;

    [SerializeField] private GameObject devMenuPanel;
    [SerializeField] private TMP_InputField gameNameInputField;
    [SerializeField] private TMP_Dropdown genreDropdown;

    private Coroutine developGameCoroutine;

    public static string NextDefaultGameName() => Instance == null ? "New Game" : $"New Game #{SellingManager.GamesReleased + 1}";
    public static void OpenDevMenu()
    {
        if (Instance == null) return;

        Instance.currentGameIdea = new(NextDefaultGameName(), "", new List<GameDevStep>
        {
            new GameDevStep("Ideation Phase", "Creating the Game Design Document", new List<GameDevTask>
            {
                new GameDevTask("Setup a moogle account", EDevSkill.Design, 2),
                new GameDevTask("Setup a moogle doc", EDevSkill.Design, 2),
                new GameDevTask("Write down all the game details", EDevSkill.Design, 2),
            }),
            new GameDevStep("Tech Demo", "Create a technical demo to show potential players", new List<GameDevTask>
            {
                new GameDevTask("Creating character movement", EDevSkill.Programming, 2),
                new GameDevTask("Working on the art assets", EDevSkill.Art, 2),
                new GameDevTask("Working on the UI elements.", EDevSkill.UI, 2),
            }),
            new GameDevStep("Final Product", "Creating the final product", new List<GameDevTask>
            {
                new GameDevTask("Tightening up game feel", EDevSkill.Programming, 2),
                new GameDevTask("Working on final shaders", EDevSkill.Art, 2),
                new GameDevTask("Throwing in sound effects last minute", EDevSkill.Sound, 2),
            }),
        });
        Instance.gameNameInputField.text = NextDefaultGameName();

        Instance.genreDropdown.ClearOptions();
        Instance.genreDropdown.options.Add(new TMP_Dropdown.OptionData("Choose a Genre"));
        Instance.genreDropdown.AddOptions(GameManager.Instance.GenreNames.ToList());

        Instance.devMenuPanel.SetActive(true);
    }
    public void CloseDevMenu() => devMenuPanel.SetActive(false);

    #region Dev Menu Panel
    [SerializeField] private GameIdea currentGameIdea;

    public void DropdownChanged_Genre(int genre) => currentGameIdea.Genre = genreDropdown.options[genre].text;
    public void InputChanged_GameName(string name) => currentGameIdea.Name = name;

    public void StartDevelopment()
    {
        if (CheckForErrors())
            return;
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

        if (currentGameIdea.Genre == "Choose a Genre" || currentGameIdea.Genre == "")
        {
            PopupManager.Instance.ShowDialoguePanel("Error", "Select a genre for your game!", EDialoguePanelType.OK);
            return true;
        }

        return false;
    }
    #endregion

    private IEnumerator DevelopGame(GameIdea gameIdea)
    {
        yield return new WaitForSeconds(1.0f);
        Debug.Log($"Begin Development of a {gameIdea.Genre} game named {gameIdea.Name}.\nSteps: {gameIdea.GameDevSteps.Count}\nTotal Time: {gameIdea.TotalTimeToComplete} seconds.");

        GameDevStep step = gameIdea.GameDevSteps[0];
        GameDevTask task = step.Tasks[0];
        ulong price = GAME_PRICE_PER_STEP * (ulong)gameIdea.GameDevSteps.Count;

        while (gameIdea.GameDevSteps.Count > 0)
        {
            step = gameIdea.GameDevSteps[0];
            if (!Character.LocalCharacter.HasTask())
            {
                if (step.Tasks.Count > 0)
                {
                    task = step.Tasks[0];
                    Character.LocalCharacter.AssignTask(new Task(task.Name, task.TimeToComplete, null));
                    gameIdea.GameDevSteps[0].Tasks.RemoveAt(0);
                }
                else
                {
                    gameIdea.GameDevSteps.RemoveAt(0);
                    yield return new WaitForSeconds(1.0f);
                }
            }
            yield return null;
        }

        developGameCoroutine = null;
        Game game = gameIdea;
        game.Price = price;
        OnGameReleased?.Invoke(game);
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
    public string Genre;
    public List<GameDevStep> GameDevSteps;

    public int TotalTimeToComplete;

    public GameIdea(string name, string genre, List<GameDevStep> gameDevSteps)
    {
        Name = name;
        Genre = genre;
        GameDevSteps = gameDevSteps;
        
        TotalTimeToComplete = gameDevSteps.Sum(step => step.Tasks.Sum(task => task.TimeToComplete));
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
    public int TimeToComplete;

    public GameDevTask(string name, EDevSkill skills, int timeToComplete)
    {
        Name = name;
        Skills = skills;
        TimeToComplete = timeToComplete;
    }
}