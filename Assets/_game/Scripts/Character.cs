using AndrewDowsett.CommonObservers;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//using Unity.Netcode;
//public struct Character : INetworkSerializable
//{
//    public string Name;
//    public int Level;

//    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
//    {
//        serializer.SerializeValue(ref Name);
//        serializer.SerializeValue(ref Level);
//    }
//}

public enum EGender
{
    None,
    Male,
    Female,
    NonBinary,
    Transgender
}

public class Character : MonoBehaviour, IPointerClickHandler, IUpdateObserver
{
    public static Character LocalCharacter;
    private void Awake()
    {
        if (LocalCharacter != null)
            return;
        LocalCharacter = this;
    }

    // Right click menu
    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            RightClickMenu.Show(characterOptions);
        }
    }

    public new string name;
    public EGender gender;

    public bool IsFree() => currentTask == null;
    public bool HasTask() => currentTask != null; // replace the check when I need to

    [SerializeField] private GameObject progressBarVisual;
    [SerializeField] private Image taskFill;
    [SerializeField] private Nullable<Task> currentTask;
    [SerializeField] private TMP_Text taskNameText;

    private void Start()
    {
        UpdateManager.RegisterObserver(this);

        characterOptions = new()
        {
            new RightClickOption("Develop A Game", DevelopGameManager.OpenDevMenu, IsFree),
            new RightClickOption("Train", null , IsFree),
            new RightClickOption("Cancel Development", Option_CancelDevelopment, HasTask)
        };
    }

    private void OnDestroy()
    {
        UpdateManager.UnregisterObserver(this);
    }

    public void ObservedUpdate(float deltaTime)
    {
        if (currentTask == null)
            return;
        progressBarVisual.SetActive(true);
        Task task = currentTask.Value;
        task.Progress += deltaTime;
        currentTask = task;
        taskFill.fillAmount = currentTask.Value.Progress / currentTask.Value.TimeToComplete;

        if (currentTask.Value.Progress >= currentTask.Value.TimeToComplete)
        {
            currentTask.Value.OnComplete?.Invoke();
            currentTask = null;
            progressBarVisual.SetActive(false);
        }
    }

    public void AssignTask(Task task)
    {
        currentTask = task;
        taskNameText.text = task.Name;
    }

    public Task UnassignTask()
    {
        Task task = currentTask.Value;
        currentTask = null;
        return task;
    }

    #region Character Actions
    private List<RightClickOption> characterOptions;
    public void AddCharacterOption(RightClickOption option)
    {
        characterOptions.Add(option);
    }
    public void RemoveCharacterOption(string optionName)
    {
        int index = -1;
        for (int i = 0; i < characterOptions.Count; i++)
        {
            if (characterOptions[i].OptionText == optionName)
            {
                index = i;
                break;
            }
        }
        characterOptions.RemoveAt(index);
    }
    //public void Option_DevelopAGame()
    //{
    //    if (!IsFree())
    //        return;
    //    Debug.Log($"{name} has started developing a game.");
    //    if (!CurrencyManager.RemoveCurrency(25000))
    //        return;
    //    currentTask = new Task("Developing a game.", 10f, () => 
    //    {
    //        Debug.Log($"{name} has finished developing a game.");
    //        GameManager.OnStartSellingGame.Invoke(new Game($"Test{UnityEngine.Random.Range(10000, 99999)}", GameManager.Instance.Genres[UnityEngine.Random.Range(0, GameManager.Instance.Genres.Length)].Name));
    //    });
    //    progressBarVisual.SetActive(true);
    //}
    public void Option_CancelDevelopment()
    {
        DevelopGameManager.CancelDevelopment();
        UnassignTask();
        progressBarVisual.SetActive(false);
    }
    #endregion
}