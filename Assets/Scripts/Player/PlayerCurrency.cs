// PlayerCurrency.cs
using UnityEngine;
using UnityEngine.Events;

public class PlayerCurrency : MonoBehaviour
{
    [Header("Starting Currency")]
    public int startingCurrency = 0;

    public int CurrentCurrency { get; private set; }

    public UnityEvent<int> OnCurrencyChanged = new UnityEvent<int>();

    private void Awake()
    {
        CurrentCurrency = startingCurrency;
    }

    public void AddCurrency(int amount)
    {
        CurrentCurrency += amount;
        OnCurrencyChanged.Invoke(CurrentCurrency);
        Debug.Log($"[Currency] +{amount} — Total: {CurrentCurrency}");
    }

    public bool SpendCurrency(int amount)
    {
        if (CurrentCurrency < amount)
        {
            Debug.Log($"[Currency] Not enough currency! Have {CurrentCurrency}, need {amount}");
            return false;
        }

        CurrentCurrency -= amount;
        OnCurrencyChanged.Invoke(CurrentCurrency);
        Debug.Log($"[Currency] -{amount} — Total: {CurrentCurrency}");
        return true;
    }
}
