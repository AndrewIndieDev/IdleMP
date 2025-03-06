using AndrewDowsett.Utility;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UINotification : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform rect;
    [SerializeField] private MaskableGraphic[] graphics;
    [SerializeField] private TMP_Text titleText;

    private Coroutine coroutine;
    private string message;
    private string title;

    private void Start()
    {
        coroutine = StartCoroutine(ShowNotification());
    }

    private void OnDestroy()
    {
        if (coroutine != null)
            StopCoroutine(coroutine);
    }

    private IEnumerator ShowNotification()
    {
        yield return null;
        float alpha = 0f;
        float initialYPosition = rect.rect.height;
        float yPosition = initialYPosition;
        while (alpha < 1f && yPosition > 0f)
        {
            alpha = Mathf.Clamp01(alpha + Time.deltaTime * 2f);
            yPosition = Mathf.Clamp(yPosition - Time.deltaTime * initialYPosition * 2f, 0, initialYPosition);
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].color = new Color(graphics[i].color.r, graphics[i].color.g, graphics[i].color.b, alpha);
            }
            rect.anchoredPosition = new Vector2(rect.anchoredPosition.x, yPosition);
            yield return null;
        }
    }

    public void Close()
    {
        Destroy(gameObject);
    }

    public void UpdateText(string title, string message)
    {
        this.title = title;
        this.message = message;
        titleText.text = title;
    }

    private void Open()
    {
        PopupManager.Instance.ShowDialoguePanel(title, message, EDialoguePanelType.OK, false, (result, x) => Close());
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Left)
            Open();
        else if (eventData.button == PointerEventData.InputButton.Right)
            Close();
    }
}
