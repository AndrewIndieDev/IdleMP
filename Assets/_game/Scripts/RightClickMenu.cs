using System;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using AndrewDowsett.Utility;

public class RightClickMenu : MonoBehaviour
{
    private static RightClickMenu Instance;
    private void Awake() => Instance = this;

    //[SerializeField] private TMP_Text titleText;
    [SerializeField] private Button[] optionButtons; // These options dictate the max amount of options
    [SerializeField] private GameObject rightClickMenuPanel;
    [SerializeField] private GameObject contentPanel;

    private List<RightClickOption> options;

    public static void Show(List<RightClickOption> options)
    {
        if (Instance == null)
            return;
        if (Instance.options == null)
            Instance.options = new();
        Instance.options.Clear();

        for (int i = 0; i < options.Count; i++)
        {
            Instance.options.Add(options[i]);
        }

        if (Instance.options.Count <= 0)
            return;

        for (int i = 0; i < Instance.optionButtons.Length; i++)
        {
            if (i >= options.Count || options[i].ShouldShow != null && !options[i].ShouldShow())
            {
                Instance.optionButtons[i].gameObject.SetActive(false);
                continue;
            }
            Instance.optionButtons[i].RightClickSetup(options[i].OptionText, (x) => Instance.OnResultButtonClicked(x), i);
            Instance.optionButtons[i].gameObject.SetActive(true);
        }

        Instance.contentPanel.GetComponent<RectTransform>().anchoredPosition = Input.mousePosition;
        Instance.rightClickMenuPanel.SetActive(true);
    }

    public void OnResultButtonClicked(int result)
    {
        if (result >= 0 && result < options.Count)
            options[result].Action?.Invoke();
        rightClickMenuPanel.SetActive(false);
    }
}

[Serializable]
public struct RightClickOption
{
    public string OptionText;
    public Action Action;
    public Func<bool> ShouldShow;

    public RightClickOption(string optionText, Action action, Func<bool> shouldShow = null)
    {
        OptionText = optionText;
        Action = action;
        ShouldShow = shouldShow;
    }
}