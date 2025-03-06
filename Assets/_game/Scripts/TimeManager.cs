using AndrewDowsett.CommonObservers;
using System;
using UnityEngine;

public class TimeManager : MonoBehaviour, IUpdateObserver
{
    public static TimeManager Instance;
    private void Awake() => Instance = this;

    public ulong Week => week + 1;
    public ulong Month => month + 1;
    public ulong Year => year + 1;

    [HideInInspector] public float WeekPercent;
    [HideInInspector] public static Action OnDatesChanged;
    [HideInInspector] public static Action OnDailyUpdate;
    [HideInInspector] public static Action OnWeeklyUpdate;
    [HideInInspector] public static Action OnMonthlyUpdate;
    [HideInInspector] public static Action OnQuarterlyUpdate;
    [HideInInspector] public static Action OnYearlyUpdate;

    [SerializeField] private float secondsPerWeek;

    private float timer;
    private float dailyTimer;
    private ulong week, month, year;
    public bool IsPaused()
    {
        return GameManager.Instance.HasPopups();

        // Add more conditions for pausing
    }

    private void Start() => UpdateManager.RegisterObserver(this);
    private void OnDestroy() => UpdateManager.UnregisterObserver(this);

    public void ObservedUpdate(float deltaTime)
    {
        if (IsPaused())
            return;

        timer += deltaTime;
        if (timer >= secondsPerWeek)
        {
            timer -= secondsPerWeek;
            ProgressDate();
        }

        dailyTimer += deltaTime;
        if (dailyTimer >= secondsPerWeek / 7f)
        {
            dailyTimer -= secondsPerWeek / 7f;
            OnDailyUpdate?.Invoke();
        }

        WeekPercent = timer != 0 ? timer / secondsPerWeek : 0f;
    }

    private void ProgressDate()
    {
        week++;
        if (week >= 4)
        {
            week -= 4;
            month++;
            OnMonthlyUpdate?.Invoke();

            if (month == 12 || month == 3 || month == 6 || month == 9)
                OnQuarterlyUpdate?.Invoke();

            if (month >= 12)
            {
                month -= 12;
                year++;
                OnYearlyUpdate?.Invoke();
            }
        }
        OnWeeklyUpdate?.Invoke();
        OnDatesChanged?.Invoke();
    }
}
