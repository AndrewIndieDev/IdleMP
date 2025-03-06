using AndrewDowsett.Utility;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class NewGameDetection : MonoBehaviour
{
    [SerializeField] private GameObject characterCustomisationPopup;
    [SerializeField] private TMP_InputField companyNameInput;
    [SerializeField] private TMP_InputField characterNameInput;

    private void Start()
    {
        // ONLY DO THIS IF WE DETECT IT THIS IS A NEW GAME
        PopupManager.Instance.ShowDialoguePanel("Welcome!", "Welcome to We Are Game Devs!\nMake games, research new topics and features, skill-up your character and take the Game Industry by storm!", EDialoguePanelType.OK, false, async (result, x) =>
        {
            await UniTask.Delay(500);
            PopupManager.Instance.ShowDialoguePanel("Notice!", "Before you start, you need to decide on a Company Name, and create your persona!", EDialoguePanelType.OK, false, async (result, x) =>
            {
                await UniTask.Delay(500);
                characterCustomisationPopup.SetActive(true);
            });
        });
    }

    public void ButtonClicked_StartGame()
    {
        if (CheckForErrors())
            return;

        characterCustomisationPopup.SetActive(false);
    }

    public void ButtonClicked_GenderChanged(int gender)
    {
        Character.LocalCharacter.gender = (EGender)gender;
    }

    private bool CheckForErrors()
    {
        if (companyNameInput.text == "")
        {
            PopupManager.Instance.ShowDialoguePanel("Error", "You need to name your company!", EDialoguePanelType.OK);
            return true;
        }

        if (characterNameInput.text == "")
        {
            PopupManager.Instance.ShowDialoguePanel("Error", "You need to name your character!", EDialoguePanelType.OK);
            return true;
        }

        if (Character.LocalCharacter.gender == EGender.None)
        {
            PopupManager.Instance.ShowDialoguePanel("Error", "Select a gender for your character!", EDialoguePanelType.OK);
            return true;
        }

        return false;
    }
}
