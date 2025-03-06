using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private CanvasGroup ui_blackBackground;
    [SerializeField] private List<GameObject> ui_screens;

    [Header("Debug")]
    [SerializeField] private bool debug;

    private Coroutine changeScreensCoroutine;

    public void ChangeScreen(EGameState state)
    {
        if (changeScreensCoroutine != null)
            return;

        changeScreensCoroutine = StartCoroutine(ChangeScreens(state));

        if (debug)
            DebugMessage($"Started changing UI to {state}. . .");
    }

    public void ChangeScreenImmediate(EGameState state)
    {
        LoadScreen(state);
        if (debug)
            DebugMessage($"Immediately changed UI to {state}. . .");
    }

    public void SetScreen(int index) => ChangeScreen((EGameState)index);

    private void LoadScreen(EGameState state)
    {
        int previousIndex = 0;
        for (int i = 0; i < ui_screens.Count; i++)
        {
            if (ui_screens[i].activeInHierarchy)
                previousIndex = i;
            ui_screens[i].SetActive(i == (int)state);
        }
    }

    private void DebugMessage(string message)
    {
        if (!debug)
            return;
        Debug.Log($"[UIManager] :: {message}");
    }

    private IEnumerator ChangeScreens(EGameState state)
    {
        ui_blackBackground.blocksRaycasts = true;

        float alpha = 0f;
        while (alpha < 1f)
        {
            alpha += Time.deltaTime * 2f;
            ui_blackBackground.alpha = alpha;
            yield return null;
        }

        LoadScreen(state);
        DebugMessage($"Changed UI to {state}. . .");

        while (alpha > 0f)
        {
            alpha -= Time.deltaTime * 2f;
            ui_blackBackground.alpha = alpha;
            yield return null;
        }

        ui_blackBackground.blocksRaycasts = false;
        changeScreensCoroutine = null;
    }
}
