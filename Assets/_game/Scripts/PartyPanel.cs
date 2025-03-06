using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PartyPanel : UIBehaviour
{
    public Vector2 closedSize;
    public Vector2 openedSize;
    public GameObject contentContainer;
    public Button panelButton;
    public GameObject clickBlock;
    public GameObject scrollbar;

    private RectTransform rectTransform;
    private bool open = false;

    protected override void Start()
    {
        base.Start();
        rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = open ? openedSize : closedSize;
        contentContainer.SetActive(open);
        clickBlock.SetActive(open);
        scrollbar.SetActive(open);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (open)
            {
                open = false;
                UpdateOpened();
            }
        }
    }

    public void OnButtonClicked()
    {
        open = !open;
        UpdateOpened();
    }

    private void UpdateOpened()
    {
        rectTransform.sizeDelta = open ? openedSize : closedSize;
        clickBlock.SetActive(open);
        scrollbar.SetActive(open);
        contentContainer.SetActive(open);

        if (open)
            PlayerPartyManager.Instance.EnableAllInviteButtons();
    }
}
