using AndrewDowsett.Utility;
using System.Collections;
using TMPro;
using Unity.Mathematics;
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
        float2 initialPosition = -rect.rect.position;
        Vector2 newPosition = new Vector2(initialPosition.x, initialPosition.y);
        Debug.Log(newPosition.magnitude);
        float previousMagnitude = 9999999f;
        while (alpha < 1f || newPosition.magnitude > 0f)
        {
            newPosition.x -= Time.deltaTime * initialPosition.x * 2f;
            newPosition.y -= Time.deltaTime * initialPosition.y * 2f;
            if (newPosition.magnitude < previousMagnitude)
                previousMagnitude = newPosition.magnitude;
            else
                newPosition = Vector2.zero;
            rect.anchoredPosition = newPosition;

            alpha = Mathf.Clamp01(alpha + Time.deltaTime * 2f);
            for (int i = 0; i < graphics.Length; i++)
            {
                graphics[i].color = new Color(graphics[i].color.r, graphics[i].color.g, graphics[i].color.b, alpha);
            }
            
            yield return null;
        }
        Debug.Log("Finished");
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
