using AndrewDowsett.CommonObservers;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DateVisual : MonoBehaviour, IUpdateObserver
{
    [SerializeField] private string weekPrefix;
    [SerializeField] private string monthPrefix;
    [SerializeField] private string yearPrefix;
    [SerializeField] private TMP_Text weekText;
    [SerializeField] private TMP_Text monthText;
    [SerializeField] private TMP_Text yearText;
    [SerializeField] private Image fillBar;
    [SerializeField] private float fillSpeed;

    private void Start()
    {
        UpdateManager.RegisterObserver(this);
        TimeManager.OnDatesChanged += UpdateDate;
    }
    private void OnDestroy()
    {
        UpdateManager.UnregisterObserver(this);
        TimeManager.OnDatesChanged -= UpdateDate;
    }

    public void ObservedUpdate(float deltaTime)
    {
        fillBar.fillAmount = Mathf.Lerp(fillBar.fillAmount, TimeManager.Instance.WeekPercent, deltaTime * fillSpeed);
    }

    private void UpdateDate()
    {
        weekText.text = $"{weekPrefix}{TimeManager.Instance.Week}";
        monthText.text = $"{monthPrefix}{TimeManager.Instance.Month}";
        yearText.text = $"{yearPrefix}{TimeManager.Instance.Year}";
    }
}
