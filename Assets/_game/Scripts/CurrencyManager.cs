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

    private void OnCopiesSold(ulong currency, ulong fans)
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
        currencyText.text = $"$ {GetString(currency)}";
        fansText.text = $"Fans: {GetString(fans)}";
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
