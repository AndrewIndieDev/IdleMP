using AndrewDowsett.CommonObservers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SingleButtonSelection : MonoBehaviour, IUpdateObserver
{
    [Header("Buttons")]
    [SerializeField] private Button[] buttons;

    [Header("Colors")]
    [SerializeField] private Color initialColor;
    [SerializeField] private Color selectedColor;
    
    private Color[] toColor;

    private void Start()
    {
        UpdateManager.RegisterObserver(this);

        toColor = new Color[buttons.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].onClick.AddListener(OnClick);
            toColor[i] = initialColor;
        }
    }

    private void OnDestroy()
    {
        UpdateManager.UnregisterObserver(this);
    }

    private void OnClick()
    {
        int index = System.Array.IndexOf(buttons, EventSystem.current.currentSelectedGameObject.GetComponent<Button>());
        for (int i = 0; i < buttons.Length; i++)
        {
            toColor[i] = i == index ? selectedColor : initialColor;
        }
    }

    public void ObservedUpdate(float deltaTime)
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].image.color = Color.Lerp(buttons[i].image.color, toColor[i], deltaTime * 10f);
        }
    }
}
