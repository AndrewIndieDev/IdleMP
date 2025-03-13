using AndrewDowsett.Utility;
using TMPro;
using UnityEngine;

public class CurrencyManager : MonoBehaviour
{
    private static CurrencyManager Instance;
    private void Awake() => Instance = this;

    [SerializeField] private TMP_Text currencyText;
    [SerializeField] private TMP_Text fansText;

    public ulong Currency => currency;
    [SerializeField] private ulong currency = 70000;

    public ulong Fans => fans;
    private ulong fans;

    private void Start()
    {
        SellingManager.OnCopiesSold += OnCopiesSold;
        UpdateTexts();
    }

    private void OnDestroy()
    {
        SellingManager.OnCopiesSold -= OnCopiesSold;
    }

    private void OnCopiesSold(ulong copies, ulong currency, ulong fans)
    {
        AddCurrency(currency);
        AddFans(fans);
    }

    public static void AddCurrency(ulong amount)
    {
        if (Instance == null) return;
        Instance.currency += amount;
        Instance.UpdateTexts();
    }

    public void AddFans(ulong amount)
    {
        if (Instance == null) return;
        fans += amount;
        UpdateTexts();
    }

    public static bool RemoveCurrency(ulong amount)
    {
        if (Instance == null || Instance.currency < amount) return false;
        Instance.currency -= amount;
        Instance.UpdateTexts();
        return true;
    }

    public static void RemoveFans(ulong amount)
    {
        if (Instance == null) return;
        Instance.fans -= amount;
        if (Instance.fans < 0)
            Instance.fans = 0;
    }

    private void UpdateTexts()
    {
        currencyText.text = $"$ {Utilities.GetShortCostString(currency)}";
        fansText.text = $"Fans: {Utilities.GetShortCostString(fans)}";
    }
}
