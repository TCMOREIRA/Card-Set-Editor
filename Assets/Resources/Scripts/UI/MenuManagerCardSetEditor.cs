using System;
using System.Collections.Generic;
using Assets.Scripts.UI;
using UnityEngine;
using UnityEngine.UI;


public class MenuManagerCardSetEditor : MonoBehaviour
{
    public static MenuManagerCardSetEditor instance;

    [SerializeField]
    private Menu CurrentMenu;
    [SerializeField]
    private UI_FileBrowser UiFileBrowser;

    [SerializeField]
    private Image CardBeingEditedImage;
    [SerializeField]
    private Text CardBeingEditedName;
    [SerializeField]
    private Text CardBeingEditedText;
    [SerializeField]
    private InputField CardBeingEditedNameInputField;
    [SerializeField]
    private InputField CardBeingEditedTextInputField;
    // [SerializeField]
    // private GameObject SerializedFieldCardBeingEditedGameObject;

    [SerializeField]
    private GameObject UICardList;

    public static GameObject CurrentSelectedPreviewCard;
    public static GameObject LastCardBeingEditedSaved;
    public static GameObject CardBeingEditedGameObject;

    private List<GameObject> CardListInstatiatedPrefabsReferences = new List<GameObject>();


    public string currentCardSetEditorProjectDirectory = null;

    /// <summary>
    /// [Singleton Pattern]
    ///  This is how you implement the Singleton Pattern in Unity.
    ///  It's a little different since we're in a Entity-Component System environment.
    /// [Singleton Pattern]
    /// </summary>
    void Start()
    {
        instance = this;
        ShowMenu(CurrentMenu);
    }

    public void NewProject(string directory)
    {
        // Open File Browser
        // User Selects a place to put their new Project
        // User Selects a name for their new Project
        // Remember this project root in a public static string current_project_root attribute
    }

    public void SaveProject(string directory)
    {
        // Open File Browser
        // User Selects a place to put their new Project
        // User Selects a name for their new Project
        // Remember this project root in a public static string current_project_root attribute
    }

    public void OpenProject(string directory)
    {
        // Open File Browser
        // User Selects a Project file, hits 'Open'
        // Import everything onto the Editor if it's empty, else:
        // Ask user to either: 
        // Save the current stuff as another project, Forget about it or Append to the project being opened
    }

    public void SaveCurrentCard()
    {
        SaveCardToList();
        // ReadWriteEditorFilesManager.WriteJson.WriteCardAsJSON(CurrentSelectedPreviewCard, null);
    }

    public void SaveAllCardsFromCardList()
    {
        // SaveCardToList();
        // ReadWriteEditorFilesManager.WriteJson.WriteCardsFromCardListAsJSON(UICardList, null);
    }

    private void ConfigureUiFileBrowserToImportStuff()
    {
        UiFileBrowser.SaveFileButtonCanvasGroup.alpha = 0;
        UiFileBrowser.SaveFileButtonCanvasGroup.interactable = false;
        UiFileBrowser.SaveFileButtonCanvasGroup.blocksRaycasts = false;
        UiFileBrowser.FileNameInputFieldCanvasGroup.alpha = 0;
        UiFileBrowser.FileNameInputFieldCanvasGroup.interactable = false;
        UiFileBrowser.FileNameInputFieldCanvasGroup.blocksRaycasts = false;
        UiFileBrowser.OpenFileButtonCanvasGroup.alpha = 1;
        UiFileBrowser.OpenFileButtonCanvasGroup.interactable = true;
        UiFileBrowser.OpenFileButtonCanvasGroup.blocksRaycasts = true;
        UiFileBrowser.FiltersButtonCanvasGroup.alpha = 1;
        UiFileBrowser.FiltersButtonCanvasGroup.interactable = true;
        UiFileBrowser.FiltersButtonCanvasGroup.blocksRaycasts = true;
    }

    private void ConfigureUiFileBrowserToExportStuff()
    {
        UiFileBrowser.SaveFileButtonCanvasGroup.alpha = 1;
        UiFileBrowser.SaveFileButtonCanvasGroup.interactable = true;
        UiFileBrowser.SaveFileButtonCanvasGroup.blocksRaycasts = true;
        UiFileBrowser.FileNameInputFieldCanvasGroup.alpha = 1;
        UiFileBrowser.FileNameInputFieldCanvasGroup.interactable = true;
        UiFileBrowser.FileNameInputFieldCanvasGroup.blocksRaycasts = true;
        UiFileBrowser.OpenFileButtonCanvasGroup.alpha = 0;
        UiFileBrowser.OpenFileButtonCanvasGroup.interactable = false;
        UiFileBrowser.OpenFileButtonCanvasGroup.blocksRaycasts = false;
        UiFileBrowser.FiltersButtonCanvasGroup.alpha = 0;
        UiFileBrowser.FiltersButtonCanvasGroup.interactable = false;
        UiFileBrowser.FiltersButtonCanvasGroup.blocksRaycasts = false;
    }

    public void ImportImageToCurrentCardImage(Menu file_browser_menu)
    {
        UI_FileBrowser.setUI_FileBrowserMode(UI_FileBrowser.UI_FileBrowserMode.importImageToCurrentCardImage);
        ConfigureUiFileBrowserToImportStuff();
        ShowMenu(file_browser_menu);
    }

    public void ImportToCurrentCardCardSetEditorCard(Menu file_browser_menu)
    {
        UI_FileBrowser.setUI_FileBrowserMode(UI_FileBrowser.UI_FileBrowserMode.importToCurrentCardCardSetEditorCard);
        ConfigureUiFileBrowserToImportStuff();
        ShowMenu(file_browser_menu);
    }

    public void ImportToCardListCardSetEditorCard(Menu file_browser_menu)
    {
        UI_FileBrowser.setUI_FileBrowserMode(UI_FileBrowser.UI_FileBrowserMode.importToCardListCardSetEditorCard);
        ConfigureUiFileBrowserToImportStuff();
        ShowMenu(file_browser_menu);
    }

    public void ImportCardSetEditorCardsFromCardSetEditorProject(Menu file_browser_menu)
    {
        UI_FileBrowser.setUI_FileBrowserMode(UI_FileBrowser.UI_FileBrowserMode.importCardSetEditorCardsFromCardSetEditorProject);
        ConfigureUiFileBrowserToImportStuff();
        ShowMenu(file_browser_menu);
    }

    public void ExportFromCurrentCardCardSetEditorCard(Menu file_browser_menu)
    {
        UI_FileBrowser.setUI_FileBrowserMode(UI_FileBrowser.UI_FileBrowserMode.exportFromCurrentCardCardSetEditorCard);
        ConfigureUiFileBrowserToExportStuff();
        ShowMenu(file_browser_menu);
    }

    public void ExportFromCardListCardSetEditorCards(Menu file_browser_menu)
    {
        UI_FileBrowser.setUI_FileBrowserMode(UI_FileBrowser.UI_FileBrowserMode.exportFromCardListCardSetEditorCards);
        ConfigureUiFileBrowserToExportStuff();
        ShowMenu(file_browser_menu);
    }

    public void ExportCardSetEditorProject(Menu file_browser_menu)
    {
        UI_FileBrowser.setUI_FileBrowserMode(UI_FileBrowser.UI_FileBrowserMode.exportCardSetEditorProject);
        ConfigureUiFileBrowserToExportStuff();
        ShowMenu(file_browser_menu);
    }

    public void ReadImageFromImageFileToCurrentCardImage(string directory)
    {
        byte[] fileData = System.IO.File.ReadAllBytes(directory);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);

        TextureScaler.Bilinear(texture, (int)CardBeingEditedImage.preferredWidth, (int)CardBeingEditedImage.preferredHeight);

        Sprite sprite = new Sprite();
        sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
        CardBeingEditedImage.overrideSprite = sprite;
    }

    public void ReadCardFromCardFileToCurrentCard(string directory, string id)
    {

        if (ReadWriteEditorFilesManager.isRunningCoroutine == false)
        {
            StartCoroutine(ReadWriteEditorFilesManager.ReadCardFromCardFileToCurrentCardCoroutine(CardBeingEditedImage, CardBeingEditedNameInputField, CardBeingEditedTextInputField, directory, id));
        }
        else
        {
            // Mensagem Popup dizendo pra pessoa esperar
        }
    }

    public void ReadCardFromCardFileAndImportToCardList(string directory)
    {

        if (ReadWriteEditorFilesManager.isRunningCoroutine == false)
        {
            StartCoroutine(ReadWriteEditorFilesManager.ReadCardFromCardFileAndImportToCardListCoroutine(UICardList, directory));
        }
        else
        {
            // Mensagem Popup dizendo pra pessoa esperar
        }
    }

    public void ReadCardsFromCardSetEditorProjectAndImportToCardList(string directory)
    {

        if (ReadWriteEditorFilesManager.isRunningCoroutine == false)
        {
            StartCoroutine(ReadWriteEditorFilesManager.ReadCardsFromCardSetEditorProjectAndImportToCardListCoroutine(UICardList, directory));
        }
        else
        {
            // Mensagem Popup dizendo pra pessoa esperar
        }
    }

    public void WriteCurrentCardToCardFile(string directory, string fileName)
    {
        // Guarantees current card has sibling index = 0
        SaveCurrentCard();

        // Open File Browser
        // User Selects a place to put their new Card file
        // User Selects a name for their new Card file
        // string directory = "";
        // ReadWriteEditorFilesManager.WriteJson.WriteCardAsJSON(CurrentSelectedPreviewCard, directory);

        if (ReadWriteEditorFilesManager.isRunningCoroutine == false)
        {
            StartCoroutine(ReadWriteEditorFilesManager.WriteCurrentCardToCardFileCoroutine(CardBeingEditedImage, CardBeingEditedNameInputField, CardBeingEditedTextInputField, directory, fileName));
        }
        else
        {
            // Mensagem Popup dizendo pra pessoa esperar
        }
    }

    public void WriteAllCardsFromCardListToCardFiles(string directory)
    {
        // Open File Browser
        // User Selects a place to put their new Card files
        // User Selects a name for their new Card files     
        // string directory = "";
        // ReadWriteEditorFilesManager.WriteJson.WriteCardsFromCardListAsJSON(UICardList, directory);

        if (ReadWriteEditorFilesManager.isRunningCoroutine == false)
        {
            StartCoroutine(ReadWriteEditorFilesManager.WriteAllCardsFromCardListToCardFilesCoroutine(UICardList, directory));
        }
        else
        {
            // Mensagem Popup dizendo pra pessoa esperar
        }
    }

    public void CreateNewCard()
    {
        GameObject SelectCardOnListButtonPrefab = Instantiate(UnityEngine.Resources.Load("Prefabs/SelectCardOnListButton")) as GameObject;
        // This is where we add this new created card to the UICardList
        SelectCardOnListButtonPrefab.transform.SetParent(UICardList.transform, false);
        SelectCardOnListButtonPrefab.transform.SetSiblingIndex(0);

        foreach (var child in SelectCardOnListButtonPrefab.GetComponentsInChildren<Text>())
        {
            if (child.name == "CardName")
            {
                child.text = "New Card";
            }

            if (child.name == "CardTextPreview")
            {
                child.text = "This card has no Text yet";
            }
        }

        // There's a little problem, if you saved and then continued editing the card this will be a false positive!
        if (MenuManagerCardSetEditor.LastCardBeingEditedSaved != MenuManagerCardSetEditor.CardBeingEditedGameObject)
        {
            // Open Warning Popup saying:
            // "Are you sure you want to create another card without saving this one? (Maybe you forgot to save the current one)"
            // "Oh I forgot! Thanks! Save and Proceed!" "Nah! This card is bollocks. Create another one! "
        }

        OpenCardFromList(SelectCardOnListButtonPrefab);
        CardListInstatiatedPrefabsReferences.Add(SelectCardOnListButtonPrefab);
        CurrentSelectedPreviewCard = SelectCardOnListButtonPrefab;
    }

    public void SaveCardToList()
    {
        int sibling_index = 0;

        // Check if we're editing a previously created card
        if (CurrentSelectedPreviewCard != null)
        {
            if (CardListInstatiatedPrefabsReferences.Contains(CurrentSelectedPreviewCard))
            {
                sibling_index = CurrentSelectedPreviewCard.transform.GetSiblingIndex();
                // For us to save this guy, we need to destroy it's old version first
                CardListInstatiatedPrefabsReferences.Remove(CurrentSelectedPreviewCard);
                Destroy(CurrentSelectedPreviewCard);
            }
        }

        GameObject SelectCardOnListButtonPrefab = Instantiate(UnityEngine.Resources.Load("Prefabs/SelectCardOnListButton")) as GameObject;
        foreach (var child in SelectCardOnListButtonPrefab.GetComponentsInChildren<Image>())
        {
            if (child.name == "CardImage")
            {
                child.overrideSprite = CardBeingEditedImage.overrideSprite;
            }
        }


        foreach (var child in SelectCardOnListButtonPrefab.GetComponentsInChildren<Text>())
        {
            if (child.name == "CardName")
            {
                child.text = CardBeingEditedName.text;
            }

            if (child.name == "CardTextPreview")
            {
                child.text = CardBeingEditedText.text;
            }
        }

        // This is where we add this new created card to the UICardList
        SelectCardOnListButtonPrefab.transform.SetParent(UICardList.transform, false);
        SelectCardOnListButtonPrefab.transform.SetSiblingIndex(sibling_index);

        LastCardBeingEditedSaved = CardBeingEditedGameObject;
        CardListInstatiatedPrefabsReferences.Add(SelectCardOnListButtonPrefab);
        CurrentSelectedPreviewCard = SelectCardOnListButtonPrefab;
    }

    public void OpenCardFromList(GameObject openThisCard)
    {

        foreach (var child in openThisCard.GetComponentsInChildren<Image>())
        {
            if (child.name == "CardImage")
            {
                CardBeingEditedImage.overrideSprite = child.overrideSprite;
            }
        }


        foreach (var child in openThisCard.GetComponentsInChildren<Text>())
        {
            if (child.name == "CardName")
            {
                CardBeingEditedNameInputField.text = child.text;
            }

            if (child.name == "CardTextPreview")
            {
                CardBeingEditedTextInputField.text = child.text;
            }
        }


    }

    public void DeleteCardFromList(GameObject deleteThisCard)
    {
        // Open Warning Popup saying:
        // "Are you sure you want to delete this card? (This will also delete card from disk, like, forever!)"
        // "No! Wait!" "Yes"

        if (CardListInstatiatedPrefabsReferences.Contains(deleteThisCard))
        {
            CardListInstatiatedPrefabsReferences.Remove(deleteThisCard);
        }
        Destroy(deleteThisCard);
    }


    public void ShowMenu(Menu menu)
    {
        if (CurrentMenu != null)
        {
            CurrentMenu.IsOpen = false;
        }
        CurrentMenu = menu;
        CurrentMenu.IsOpen = true;

    }

}

