using AndrewDowsett.Utility;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIStepPanel : MonoBehaviour
{
    [Header("Variables")]
    [SerializeField] private int maxFreelancerCost;
    private ulong totalCost;

    [Header("References")]
    [SerializeField] private GameObject stepPanel;
    [SerializeField] private TMP_Text stepPanelTitle;
    [SerializeField] private Slider taskTotalSlider;
    [SerializeField] private TMP_Text totalPercentageText;
    [SerializeField] private TMP_Text freelancerCostText;

    [Header("Task 1")]
    [SerializeField] private TMP_Text taskOneNameText;
    [SerializeField] private Slider taskOneSlider;
    [SerializeField] private TMP_Text taskOnePercentageText;

    [Header("Task 2")]
    [SerializeField] private TMP_Text taskTwoNameText;
    [SerializeField] private Slider taskTwoSlider;
    [SerializeField] private TMP_Text taskTwoPercentageText;

    [Header("Task 3")]
    [SerializeField] private TMP_Text taskThreeNameText;
    [SerializeField] private Slider taskThreeSlider;
    [SerializeField] private TMP_Text taskThreePercentageText;

    public static Action<int, float> OnTaskValueChanged;

    private void Start()
    {
        taskOneSlider.onValueChanged.AddListener(OnTaskOneValueChanged);
        taskTwoSlider.onValueChanged.AddListener(OnTaskTwoValueChanged);
        taskThreeSlider.onValueChanged.AddListener(OnTaskThreeValueChanged);

        DevelopGameManager.OnStartedNewStep += OnStartedNewStep;
    }

    private void OnDestroy()
    {
        taskOneSlider.onValueChanged.RemoveListener(OnTaskOneValueChanged);
        taskTwoSlider.onValueChanged.RemoveListener(OnTaskTwoValueChanged);
        taskThreeSlider.onValueChanged.RemoveListener(OnTaskThreeValueChanged);

        DevelopGameManager.OnStartedNewStep -= OnStartedNewStep;
    }

    private void OnStartedNewStep(GameDevStep step)
    {
        stepPanelTitle.text = step.Name;

        taskOneNameText.text = step.Tasks[0].Name;
        taskTwoNameText.text = step.Tasks[1].Name;
        taskThreeNameText.text = step.Tasks[2].Name;

        OnTaskOneValueChanged(0.5f);
        OnTaskTwoValueChanged(0.5f);
        OnTaskThreeValueChanged(0.5f);

        stepPanel.SetActive(true);
    }

    private void OnTaskOneValueChanged(float value)
    {
        taskOneSlider.value = value;
        //taskOnePercentageText.text = $"{taskOneSlider.value * 100}%";
        float sliderValue = (taskOneSlider.value * 100).Remap(0, 100, -100, 100);
        taskOnePercentageText.text = $"{(sliderValue > 75 ? ">>>" : sliderValue > 50 ? ">>" : sliderValue > 25 ? ">" : sliderValue < -75 ? "<<<" : sliderValue < -50 ? "<<" : sliderValue < -25 ? "<" : "")}";
        UpdateTotalSlider();
        OnTaskValueChanged?.Invoke(0, value);
    }

    private void OnTaskTwoValueChanged(float value)
    {
        taskTwoSlider.value = value;
        //taskTwoPercentageText.text = $"{taskTwoSlider.value * 100}%";
        float sliderValue = (taskTwoSlider.value * 100).Remap(0, 100, -100, 100);
        taskTwoPercentageText.text = $"{(sliderValue > 75 ? ">>>" : sliderValue > 50 ? ">>" : sliderValue > 25 ? ">" : sliderValue < -75 ? "<<<" : sliderValue < -50 ? "<<" : sliderValue < -25 ? "<" : "")}";
        UpdateTotalSlider();
        OnTaskValueChanged?.Invoke(1, value);
    }

    private void OnTaskThreeValueChanged(float value)
    {
        taskThreeSlider.value = value;
        //taskThreePercentageText.text = $"{taskThreeSlider.value * 100}%";
        float sliderValue = (taskThreeSlider.value * 100).Remap(0, 100, -100, 100);
        taskThreePercentageText.text = $"{(sliderValue > 75 ? ">>>" : sliderValue > 50 ? ">>" : sliderValue > 25 ? ">" : sliderValue < -75 ? "<<<" : sliderValue < -50 ? "<<" : sliderValue < -25 ? "<" : "")}";
        UpdateTotalSlider();
        OnTaskValueChanged?.Invoke(2, value);
    }

    private void UpdateTotalSlider()
    {
        float total = taskOneSlider.value + taskTwoSlider.value + taskThreeSlider.value;
        taskTotalSlider.value = (total > 0) ? total / 3f : 0f;
        float sliderValue = (taskTotalSlider.value * 100).Remap(0, 100, -100, 100);
        totalPercentageText.text = $"{(sliderValue > 75 ? ">>>" : sliderValue > 50 ? ">>" : sliderValue > 25 ? ">" : sliderValue < -75 ? "<<<" : sliderValue < -50 ? "<<" : sliderValue < -25 ? "<" : "")}";

        totalCost = (ulong)Mathf.RoundToInt(total * maxFreelancerCost);
        freelancerCostText.text = $"$ {totalCost}";
    }

    public void CloseWindow()
    {
        if (!CurrencyManager.RemoveCurrency(totalCost))
        {
            PopupManager.Instance.ShowDialoguePanel("Unable to pay!", "You cannot afford to pay freelancers for this job.\nTry doing more work yourself!", EDialoguePanelType.OK);
            return;
        }

        stepPanel.SetActive(false);
    }
}
