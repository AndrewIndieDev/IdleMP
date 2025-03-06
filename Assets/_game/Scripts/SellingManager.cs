using AndrewDowsett.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SellingManager : MonoBehaviour
{
    private static SellingManager Instance;
    private void Awake() => Instance = this;

    public static Action<ulong, ulong> OnCopiesSold; // currency made, fans

    [SerializeField] private TMP_Text totalSalesText;
    [SerializeField] private TMP_Text thisMonthSalesText;
    [SerializeField] private TMP_Text highestSellingGameText;
    [SerializeField] private List<Image> fillBars = new();
    [SerializeField] private ulong effectiveSellingMonths;

    public static ulong GamesReleased => Instance == null ? 0 : Instance.gamesReleased;
    public static bool HasReleasedGameWithName(string name) => Instance == null ? false : Instance.currentlySelling.Any(game => game.Game.Name == name);

    private List<ulong> monthlyTotalSalesGraphBars;
    private ulong highestSellingMonth => (monthlyTotalSalesGraphBars.Max() > 1000 ? monthlyTotalSalesGraphBars.Max() : 1000);
    private List<SellingGame> currentlySelling = new();
    private ulong gamesReleased;
    private ulong currencyEarnedThisMonth;
    private ulong fansEarnedThisMonth;
    private void Start()
    {
        DevelopGameManager.OnGameReleased += OnGameReleased;
        TimeManager.OnDailyUpdate += OnDailyUpdate;
        TimeManager.OnWeeklyUpdate += OnWeeklyUpdate;
        TimeManager.OnMonthlyUpdate += OnMonthlyUpdate;

        monthlyTotalSalesGraphBars = new List<ulong>();
        for (int i = 0; i < fillBars.Count; i++)
        {
            monthlyTotalSalesGraphBars.Add(0);
        }
    }
    private void OnDestroy()
    {
        DevelopGameManager.OnGameReleased -= OnGameReleased;
        TimeManager.OnDailyUpdate -= OnDailyUpdate;
        TimeManager.OnWeeklyUpdate -= OnWeeklyUpdate;
        TimeManager.OnMonthlyUpdate -= OnMonthlyUpdate;
    }

    private void OnGameReleased(Game game)
    {
        currentlySelling.Add(new SellingGame(game));
        gamesReleased++;
    }

    public void RemoveSellingGame(string gameName)
    {
        for (int i = 0; i < currentlySelling.Count; i++)
        {
            if (currentlySelling[i].Game.Name == gameName)
            {
                currentlySelling.RemoveAt(i);
                break;
            }
        }
    }

    private void OnDailyUpdate()
    {
        ulong totalCopiesSold = 0;
        ulong dailyCopiesSold = 0;
        SellingGame game;
        for (int i = 0; i < currentlySelling.Count; i++)
        {
            float copiesSold = UnityEngine.Random.Range(1, 20) // The base amount of copies to sell
                * Mathf.Clamp((float)effectiveSellingMonths / (float)currentlySelling[i].WeeksSinceStartedSelling, 0.01f, 1f) // The amount of copies to sell based on how long the game has been selling
                * GameManager.Instance.GetGenreByName(currentlySelling[i].Game.Genre).Popularity.Last() // The amount of copies to sell based on genre popularity
                .Remap(0f, 10f, 0.2f, 4f); // Remap the popularity so we don't get 0 sales as often, and it's not as harsh
            if (copiesSold < 1)
            {
                float chance = UnityEngine.Random.Range(0f, 1f);
                if (chance <= copiesSold)
                    copiesSold = 1;
            }
            ulong gameCopiesSold = (ulong)copiesSold;
            monthlyTotalSalesGraphBars[0] += gameCopiesSold;
            dailyCopiesSold += gameCopiesSold;
            totalCopiesSold += currentlySelling[i].QuantitySold;
            game = currentlySelling[i];
            game.Buy(gameCopiesSold);
            currentlySelling[i] = game;
            currencyEarnedThisMonth += gameCopiesSold * game.Game.Price;
            Debug.Log($"Daily: {gameCopiesSold * game.Game.Price} | Monthly: {currencyEarnedThisMonth}");

            fansEarnedThisMonth += (ulong)((float)gameCopiesSold * 0.1f);
        }
        //OnCopiesSold?.Invoke(dailyCurrencyMade, (ulong)((float)dailyCopiesSold * 0.1f));

        for (int i = 0; i < fillBars.Count; i++)
        {
            fillBars[i].fillAmount = (float)monthlyTotalSalesGraphBars[i] / (float)highestSellingMonth;
        }

        totalSalesText.text = $"Total: {GetString(totalCopiesSold)}";
        thisMonthSalesText.text = $"Sold: {GetString(monthlyTotalSalesGraphBars[0])}";
        string highestSellingGame = "";
        ulong highestSellingAmount = 0;
        foreach (var selling in currentlySelling)
        {
            if (selling.QuantitySold > highestSellingAmount)
            {
                highestSellingAmount = selling.QuantitySold;
                highestSellingGame = selling.Game.Name;
            }
        }
        highestSellingGameText.text = $"Best Seller: {highestSellingGame}";
    }

    private void OnWeeklyUpdate()
    {
        SellingGame game;
        for (int i = 0; i < currentlySelling.Count; i++)
        {
            game = currentlySelling[i];
            game.WeeklyTick();
            currentlySelling[i] = game;
        }
    }

    private void OnMonthlyUpdate()
    {
        for (int i = monthlyTotalSalesGraphBars.Count - 1; i >= 0; i--)
        {
            if (i == 0)
            {
                monthlyTotalSalesGraphBars[i] = 0;
                continue;
            }
            monthlyTotalSalesGraphBars[i] = monthlyTotalSalesGraphBars[i - 1];
        }

        // Gain currency for the monthly sales
        OnCopiesSold?.Invoke(currencyEarnedThisMonth, fansEarnedThisMonth);
        currencyEarnedThisMonth = 0;
        fansEarnedThisMonth = 0;
        // play cash sound
        // play cash animation
    }

    private string GetString(ulong amount)
    {
        if (amount < 1000)
        {
            return amount.ToString();
        }
        else if (amount < 1000000)
        {
            float amountInThousands = (float)amount / 1000f;
            return (amountInThousands).ToString((amountInThousands < 100 ? "N2" : "N0")) + " k";
        }
        else
        {
            float amountInMillions = (float)amount / 1000000f;
            return (amountInMillions).ToString((amountInMillions < 100 ? "N2" : "N0")) + " M";
        }
    }
}

public struct SellingGame
{
    public Game Game;
    public ulong QuantitySold;
    public ulong TotalMoneyMade;
    public ulong WeeksSinceStartedSelling;

    public SellingGame(Game game)
    {
        Game = game;
        QuantitySold = 0;
        TotalMoneyMade = 0;
        WeeksSinceStartedSelling = 1;
    }

    public void WeeklyTick()
    {
        WeeksSinceStartedSelling++;
    }

    public void Buy(ulong amount)
    {
        QuantitySold += amount;
        TotalMoneyMade = QuantitySold * Game.Price;
    }
}