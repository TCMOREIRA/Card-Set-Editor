// Copyright (c) 2015 All Right Reserved
//
// </copyright>
// <author>Benjamin Huguenin</author>
// <email>huguenin.benjamin@gmail.com</email>
// <date>30-03-2015</date>
// <summary>This is the bridge between the file browser and the UI.</summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using System.Threading;

public class UI_FileBrowser : MonoBehaviour
{
    #region Variables

    [SerializeField]
    public Text FileNameInputFieldText;
    [SerializeField]
    public CanvasGroup FileNameInputFieldCanvasGroup;
    [SerializeField]
    public CanvasGroup FiltersButtonCanvasGroup;
    [SerializeField]
    public CanvasGroup OpenFileButtonCanvasGroup;
    [SerializeField]
    public CanvasGroup SaveFileButtonCanvasGroup;


    // The file browser

    private string StartingDirectory = ""; // You can specify a starting directory here for the file browser. An empty string open the game folder.
    private FileBrowser _FB;

    // UI Main Managment

    public enum UI_FileBrowserMode
    {
        importImageToCurrentCardImage,
        importToCurrentCardCardSetEditorCard,
        importToCardListCardSetEditorCard,
        exportFromCurrentCardCardSetEditorCard,
        exportFromCardListCardSetEditorCards,
        importCardSetEditorProject,
        exportCardSetEditorProject,
        importCardSetEditorCardsFromCardSetEditorProject
    };

    private static UI_FileBrowserMode currentUiFileBrowserMode;

    public static void setUI_FileBrowserMode(UI_FileBrowserMode mode)
    {
        currentUiFileBrowserMode = mode;
    }

    private enum ButtonType
    {
        File,
        Folder,
        Drive,
        Computer
    };

    public RectTransform MainPanel; // The panel where the buttons will be displayed
    public Button MenuItem; // The button template
    private List<Button> Buttons; // The current buttons list
    public Slider ButtonSize; // The height of the buttons

    private string m_pOpenedPath = null; // The path of the file you choose to open

    public GameObject FileBrowserWindow; // The menu root

    public Toggle FolderOnly; // Choose to show files or not

    // Files sprites
    public Sprite Folder;
    public Sprite Drive;
    public Sprite File;
    public Sprite Computer;

    // Quick Navigation

    private bool ValidateDoubleClick = false; // A variable coupled with a cooldown so you can double click on the menu items to open them
    private Button CurrentlySelected; // The currently selected file

    // Folder Managment

    public RectTransform SortModeOptions; // The sort modes you can select
    public InputField RenameInputField; // The rename input text field

    // Favorites

    public RectTransform FavoritePanel; // The panel where the buttons will be displayed
    public Button FavoriteTemplate; // The button template
    private List<Button> Favorites; // The current buttons list
    public Image FavoriteImage; // The "star" image, so we can change it's color to yellow if current folder is already a favorite

    // Filters

    public RectTransform FiltersOptions; // The panel where the filters will be displayed
    public Button FilterItem; // The filters template
    private List<Button> FiltersList; // The filters button list
    private List<string[]> FiltersStrings; // A list of strings that will initialize the filters button list

    private string[] SupportedImageFilesList =
    {".jpg", ".jpeg", ".png", ".psd", ".tiff", ".tga", ".gif", ".bmp", ".iff", ".pict"};
    private string[] SupportedCardSetEditorFilesList =
    { ".cse-card", ".cse-project" };


    public Text SelectedFilter; // The selected filter text area

    // Search

    public Text SearchField; // The search field text area
    public GameObject Loading;

    // Adress Bar

    public InputField CurrentAdress; // The search field text area

    // Thumbnails

    // Struct containing the data of a single thumbnail
    struct ThumbData
    {
        public byte[] _bytes;
        public string _fullName;
        public string _name;
        public float SizeX;
        public float SizeY;
    }

    private Thread ThumbnailsThread; // Thumbnails Thread
    private FileInfo[] _ThumbnailsFiles = new FileInfo[0]; // Files that will need a thumbnail
    private List<ThumbData> m_pImagesBytesArray = new List<ThumbData>(); // List of the thumbnails data that will be generated from the above FileInfo array
    private bool GenerateNewThumbnails = false; // Boolean stating if we need to start generating new thumbnails
    private bool m_bGenerationCompleted = false; // Boolean stating the state of the current thumbnail generation
    private bool CancelGeneration = false; // Boolean allowing us to cancel the current thumbnails generation
    private bool CancelThread = false; // Boolean allowing us to quit to thumbnails thread
    private int ThumbSize; // The size of the thumbnails (initialized to the sized if the "ItemSize" slider)
    #endregion

    #region UnityCallBack
    void Awake()
    {
        // Start Thread
        ThreadStart threadDelegate = new ThreadStart(this.GenerateThumbnails);
        ThumbnailsThread = new Thread(threadDelegate);
        ThumbnailsThread.Name = "Thumbnails Thread";
        ThumbnailsThread.Start();

        // The next line is Debug and Develop only
        StartingDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments); //Application.dataPath + "/Card Set Editor Projects/";

        // Create file browser
        if (StartingDirectory == null || StartingDirectory == string.Empty)
            _FB = new FileBrowser();
        else
            _FB = new FileBrowser(StartingDirectory);

        // Initialize the lists
        FiltersStrings = new List<string[]>();
        FiltersList = new List<Button>();
        Buttons = new List<Button>();
        Favorites = new List<Button>();

        // Set the differents file filters

        FiltersStrings.Add(null); // Let this one, it is the default
        // FiltersStrings.Add(new string[] { ".txt" }); // Add a new filter like this. This one will only show the ".txt" files
        if (currentUiFileBrowserMode == UI_FileBrowserMode.importImageToCurrentCardImage)
            FiltersStrings.Add(SupportedImageFilesList); // You can also specify a list a filters, so any file using any of this extension will be displayed.
        else
            FiltersStrings.Add(SupportedCardSetEditorFilesList);

        CreateFavoriteFolder();
        RefreshFavorites();

        m_pOpenedPath = null;
    }

    void Start()
    {
        ThumbSize = (int)ButtonSize.value;

        // Create the list of filters from the one specified in "Awake"
        CreateFiltersOptions(FiltersStrings);

        // Refresh the buttons
        RefreshButtons();
    }

    void OnApplicationQuit()
    {
        // Cancel Threads
        _FB.QuitSearchThread();
        QuitThumbnailsThread();
    }
    #endregion

    #region UI_MainManagment
    public void Open()
    {
        m_pOpenedPath = null;
        FileBrowserWindow.SetActive(true);

        if (StartingDirectory == null || StartingDirectory == string.Empty)
            _FB.Relocate(null);
        else
            _FB.Relocate(StartingDirectory);

        RefreshButtons();
    }

    public void Open(string Path)
    {
        m_pOpenedPath = null;
        FileBrowserWindow.SetActive(true);

        if (Path == null || Path == string.Empty)
            _FB.Relocate(null);
        else
            _FB.Relocate(Path);

        RefreshButtons();
    }

    public void Cancel()
    {
        FileBrowserWindow.SetActive(false);
    }

    public string GetPath()
    {
        return m_pOpenedPath;
    }

    public void RefreshButtons()
    {
        CancelCurrentThumbnails();

        MainPanel.sizeDelta = Vector2.zero;

        // Destroy previous buttons
        for (int i = 0; i < Buttons.Count; i++)
        {
            Buttons[i].gameObject.SetActive(false);
            DestroyImmediate(Buttons[i].gameObject);
        }

        // Clear List
        Buttons.Clear();

        DirectoryInfo Dir = _FB.GetCurrentDirectory();

        if (Dir != null)
            CurrentAdress.text = Dir.FullName;
        else
            CurrentAdress.text = string.Empty;

        bool isFavorite = false;
        for (int i = 0; i < Favorites.Count; i++)
        {
            RectTransform _CurrentRect = Favorites[i].gameObject.GetComponent<RectTransform>();

            if (_CurrentRect.FindChild("Name").gameObject.GetComponent<Text>().text == CurrentAdress.text)
            {
                isFavorite = true;
                break;
            }
        }

        if (isFavorite)
        {
            FavoriteImage.color = new Color(1, 1, 0, 1);
        }
        else
        {
            FavoriteImage.color = new Color(1, 1, 1, 1);
        }

        // Retrieve sub directories
        DirectoryInfo[] _childs = _FB.GetChildDirectories();

        // Add a button for each folder
        for (int i = 0; i < _childs.Length; i++)
        {
            // Dissociate folders from drives
            if (_childs[i].Parent != null)
                AddButton(_childs[i].Name, ButtonType.Folder);
            else
                AddButton(_childs[i].Name, ButtonType.Drive);
        }

        if (!FolderOnly.isOn)
        {
            // Retrieve files
            FileInfo[] _files = _FB.GetFiles();

            // Add a button for each file
            for (int i = 0; i < _files.Length; i++)
            {
                AddButton(_files[i].Name, ButtonType.File);
            }

            lock (_ThumbnailsFiles)
            {
                _ThumbnailsFiles = _files;
            }

            GenerateNewThumbnails = true;
            StartCoroutine(WaitForThumbnails());
        }

        CheckButtonsVisibility();
    }

    private void AddButton(string FileName, ButtonType Type)
    {
        // Get height from template button
        float ButtonHeight = MenuItem.gameObject.GetComponent<RectTransform>().rect.height;

        // Create new button
        Button NewButton = GameObject.Instantiate(MenuItem) as Button;
        NewButton.gameObject.SetActive(true);
        NewButton.gameObject.transform.SetParent(MainPanel.gameObject.transform);

        // Place button
        RectTransform _CurrentRect = NewButton.gameObject.GetComponent<RectTransform>();
        _CurrentRect.localPosition = new Vector3(0, (-10) * (Buttons.Count + 1) + (-ButtonHeight) * Buttons.Count, 0);
        _CurrentRect.sizeDelta = new Vector2(-20, ButtonHeight);

        // Set button label to retrieve it later
        _CurrentRect.FindChild("Text").gameObject.GetComponent<Text>().text = FileName;

        // Set button name to store it's type
        switch (Type)
        {
            case ButtonType.File:
                NewButton.gameObject.name = "File";
                _CurrentRect.FindChild("Image").gameObject.GetComponent<Image>().sprite = File;
                break;
            case ButtonType.Folder:
                NewButton.gameObject.name = "Folder";
                _CurrentRect.FindChild("Image").gameObject.GetComponent<Image>().sprite = Folder;
                break;
            case ButtonType.Drive:
                NewButton.gameObject.name = "Drive";
                _CurrentRect.FindChild("Image").gameObject.GetComponent<Image>().sprite = Drive;
                break;
            case ButtonType.Computer:
                NewButton.gameObject.name = "Computer";
                _CurrentRect.FindChild("Image").gameObject.GetComponent<Image>().sprite = Computer;
                break;
        }

        // Add the button callback
        NewButton.onClick.RemoveAllListeners();
        NewButton.onClick.AddListener(() => SelectFile(NewButton));

        // Add the button to the list
        Buttons.Add(NewButton);

        // Resize parent panel to fit the new button
        MainPanel.sizeDelta = new Vector2(0, 10 * (Buttons.Count + 1) + ButtonHeight * Buttons.Count);
    }

    public void ChangeItemSize()
    {
        ThumbSize = (int)ButtonSize.value;

        RectTransform _CurrentRect = MenuItem.gameObject.GetComponent<RectTransform>();
        _CurrentRect.localPosition = new Vector3(-5, -10, 0);
        _CurrentRect.sizeDelta = new Vector2(-30, ThumbSize);

        RectTransform _image = _CurrentRect.FindChild("Image").gameObject.GetComponent<RectTransform>();
        _image.anchoredPosition3D = new Vector3(ThumbSize / 2, 0, 0);
        _image.sizeDelta = new Vector2(ThumbSize - 10, ThumbSize - 10);

        RectTransform _text = _CurrentRect.FindChild("Text").gameObject.GetComponent<RectTransform>();
        _text.anchoredPosition3D = new Vector3(-20, 0, 0);
        _text.sizeDelta = new Vector2(-(ThumbSize * 2) - 40, 40);

        RefreshButtons();
    }
    #endregion

    #region Shortcuts
    // HIERARCHY

    // Go the the previous file opened
    public void ToPrevious()
    {
        _FB.GoToPrevious();
        RefreshButtons();
    }

    // Go the the next file opened
    public void ToNext()
    {
        _FB.GotToNext();
        RefreshButtons();
    }

    // Go back in the hierarchy
    public void ToParent()
    {
        _FB.GoInParent();
        RefreshButtons();
    }

    // Open root folder
    public void ToRoot()
    {
        _FB.GoToRoot(true);
        RefreshButtons();
    }

    // SPECIAl FOLDERS

    // Open desktop
    public void Desktop()
    {
        string Desktop = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);
        _FB.RetrieveFiles(new DirectoryInfo(Desktop), true);
        RefreshButtons();
    }

    // Open MyDocuments
    public void MyDocuments()
    {
        string MyDocuments = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        _FB.RetrieveFiles(new DirectoryInfo(MyDocuments), true);
        RefreshButtons();
    }

    // Open MyMusic
    public void MyMusic()
    {
        string MyMusic = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyMusic);
        _FB.RetrieveFiles(new DirectoryInfo(MyMusic), true);
        RefreshButtons();
    }

    // Open MyPictures
    public void MyPictures()
    {
        string MyPictures = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
        _FB.RetrieveFiles(new DirectoryInfo(MyPictures), true);
        RefreshButtons();
    }

    // Open Game directory
    public void GameDir()
    {
        string GameDirectory;
        if (MenuManagerCardSetEditor.instance.currentCardSetEditorProjectDirectory != null)
        {
            GameDirectory = MenuManagerCardSetEditor.instance.currentCardSetEditorProjectDirectory;
        }
        else GameDirectory = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        _FB.RetrieveFiles(new DirectoryInfo(GameDirectory), true);
        RefreshButtons();
    }
    #endregion

    #region QuickNavigation
    IEnumerator DoubleClick()
    {
        ValidateDoubleClick = true;

        yield return new WaitForSeconds(.5f);

        ValidateDoubleClick = false;
    }

    void SelectFile(Button _Button)
    {
        // If file has been double clicked, open it
        if (_Button == CurrentlySelected && ValidateDoubleClick)
        {
            OpenFile();
        }
        // Selecte the file and start the double click cooldown
        else
        {
            CurrentlySelected = _Button;
            StartCoroutine(DoubleClick());
        }
    }


    public void OpenFile()
    {

        if (CurrentlySelected == null)
            return;

        // Retrieve the name of the directory stored in the button label
        RectTransform _CurrentRect = CurrentlySelected.gameObject.GetComponent<RectTransform>();
        string _text = _CurrentRect.FindChild("Text").gameObject.GetComponent<Text>().text;

        if (CurrentlySelected.gameObject.name == "Folder" || CurrentlySelected.gameObject.name == "Drive")
        {
            // Open the folder if it is one
            _FB.GoInSubDirectory(_text);
        }
        else if (CurrentlySelected.gameObject.name == "File")
        {
            m_pOpenedPath = _FB.GetFilePath(_text);

            if (currentUiFileBrowserMode == UI_FileBrowserMode.importImageToCurrentCardImage)
            {
                bool isASupportedExtension = false;
                foreach (var supportedImageExtension in SupportedImageFilesList)
                {
                    if (Path.GetExtension(m_pOpenedPath).Contains(supportedImageExtension))
                    {
                        isASupportedExtension = true;
                    }

                }

                if (isASupportedExtension)
                //  ".jpg", ".jpeg", ".png", ".psd", ".tiff", ".tga", ".gif", ".bmp", ".iff", ".pict" 
                {
                    MenuManagerCardSetEditor.instance.ReadImageFromImageFileToCurrentCardImage(m_pOpenedPath);
                }
                else
                {
                    /*Unsupported format Pop-Up*/
                }
            }

            if (currentUiFileBrowserMode == UI_FileBrowserMode.importToCurrentCardCardSetEditorCard)
            {
                if (Path.GetExtension(m_pOpenedPath).Contains(".cse-card"))
                {
                    string directory = Path.GetDirectoryName(m_pOpenedPath) + @"\";
                    MenuManagerCardSetEditor.instance.ReadCardFromCardFileToCurrentCard(directory, Path.GetFileNameWithoutExtension(m_pOpenedPath));
                }
            }

            if (currentUiFileBrowserMode == UI_FileBrowserMode.importToCardListCardSetEditorCard)
            {
                if (Path.GetExtension(m_pOpenedPath).Contains(".cse-card"))
                {
                    MenuManagerCardSetEditor.instance.ReadCardFromCardFileAndImportToCardList(m_pOpenedPath);
                }
            }

            if (currentUiFileBrowserMode == UI_FileBrowserMode.importCardSetEditorCardsFromCardSetEditorProject)
            {
                if (Path.GetExtension(m_pOpenedPath).Contains(".cse-project"))
                {
                    MenuManagerCardSetEditor.instance.ReadCardsFromCardSetEditorProjectAndImportToCardList(m_pOpenedPath);
                }
            }

            if (currentUiFileBrowserMode == UI_FileBrowserMode.importCardSetEditorProject)
            {
                if (Path.GetExtension(m_pOpenedPath).Contains(".cse-project"))
                {
                    MenuManagerCardSetEditor.instance.OpenProject(m_pOpenedPath);
                }
            }

        }

        // Refresh the buttons with the new hierarchy
        RefreshButtons();
        CurrentlySelected = null;

    }

    public void SaveFile()
    {
        string directory = _FB.GetCurrentDirectory().FullName + @"\";
        if (CurrentlySelected == null && !FileNameInputFieldText.text.Equals(""))
        {
            if (currentUiFileBrowserMode == UI_FileBrowserMode.exportFromCurrentCardCardSetEditorCard)
            {
                {
                    MenuManagerCardSetEditor.instance.WriteCurrentCardToCardFile(directory, FileNameInputFieldText.text);
                }

            }

            if (currentUiFileBrowserMode == UI_FileBrowserMode.exportFromCardListCardSetEditorCards)
            {
                {
                    MenuManagerCardSetEditor.instance.WriteAllCardsFromCardListToCardFiles(directory);
                }
            }

            if (currentUiFileBrowserMode == UI_FileBrowserMode.importCardSetEditorCardsFromCardSetEditorProject)
            {
                {
                    MenuManagerCardSetEditor.instance.SaveProject(directory);
                    MenuManagerCardSetEditor.instance.currentCardSetEditorProjectDirectory = directory;
                }
            }
        }
        else if (CurrentlySelected == null && FileNameInputFieldText.text.Equals(""))
        {
            // Open Popup saying "Hey, you forgot to name this file!"
        }
        else if (CurrentlySelected != null)
        {
            // Retrieve the name of the directory stored in the button label
            RectTransform _CurrentRect = CurrentlySelected.gameObject.GetComponent<RectTransform>();
            string _text = _CurrentRect.FindChild("Text").gameObject.GetComponent<Text>().text;

            if (CurrentlySelected.gameObject.name == "File")
            {
                m_pOpenedPath = _FB.GetFilePath(_text);

                // Open Pop-Up warning the user that she is overwritting another file

                if (currentUiFileBrowserMode == UI_FileBrowserMode.exportFromCurrentCardCardSetEditorCard)
                {
                    {
                        MenuManagerCardSetEditor.instance.WriteCurrentCardToCardFile(m_pOpenedPath, "");
                    }

                }

                if (currentUiFileBrowserMode == UI_FileBrowserMode.exportFromCardListCardSetEditorCards)
                {
                    {
                        MenuManagerCardSetEditor.instance.WriteAllCardsFromCardListToCardFiles(m_pOpenedPath);
                    }
                }

                if (currentUiFileBrowserMode == UI_FileBrowserMode.importCardSetEditorCardsFromCardSetEditorProject)
                {
                    {
                        MenuManagerCardSetEditor.instance.SaveProject(m_pOpenedPath);
                        MenuManagerCardSetEditor.instance.currentCardSetEditorProjectDirectory = directory;
                    }
                }

            }
        }

        // Refresh the buttons with the new hierarchy
        RefreshButtons();
        CurrentlySelected = null;
    }

    #endregion

    #region FolderManagment
    public void Rename()
    {
        if (RenameInputField.text.Trim() != string.Empty
           && CurrentlySelected != null
           && RenameInputField.text.IndexOfAny(new char[] { '/', '\\', '?', '%', '*', ':', '|', '"', '<', '>' }) == -1)
        {
            RectTransform _CurrentRect = CurrentlySelected.gameObject.GetComponent<RectTransform>();
            string _text = _CurrentRect.FindChild("Text").gameObject.GetComponent<Text>().text;

            string dirfullpath = _FB.GetDirectoryPath(_text);

            if (dirfullpath != null)
            {
                DirectoryInfo _current = new DirectoryInfo(dirfullpath);

                DirectoryInfo[] _currentChilds = _current.GetDirectories();
                FileInfo[] _currentFiles = _current.GetFiles();

                DirectoryInfo _new = new DirectoryInfo(NewFolder(RenameInputField.text));

                for (int i = 0; i < _currentChilds.Length; i++)
                {
                    System.IO.Directory.Move(_currentChilds[i].FullName, _new.FullName + "\\" + _currentChilds[i].Name);
                }

                for (int i = 0; i < _currentFiles.Length; i++)
                {
                    System.IO.Directory.Move(_currentFiles[i].FullName, _new.FullName + "\\" + _currentFiles[i].Name);
                }

                System.IO.Directory.Delete(_current.FullName);
            }
            else
            {
                string filefullpath = _FB.GetFilePath(_text);

                if (filefullpath != null)
                {
                    FileInfo _file = new FileInfo(filefullpath);

                    string _New = RenameInputField.text;
                    int iTry = 0;

                    while (_FB.GetFilePath(_New) != null)
                    {
                        _New = RenameInputField.text + " (" + iTry + ")";

                        iTry++;
                    }

                    byte[] fileData = new byte[_file.Length];

                    FileStream fs = _file.OpenRead();

                    fs.Read(fileData, 0, (int)_file.Length);

                    fs.Dispose();

                    System.IO.File.WriteAllBytes(_FB.GetCurrentDirectory().FullName + "\\" + _New + _file.Extension, fileData);

                    System.IO.File.Delete(_file.FullName);
                }
            }

            _FB.Refresh();
            RefreshButtons();
        }

        WaitForNewName();
        RenameInputField.text = string.Empty;
    }

    public void WaitForNewName()
    {
        RenameInputField.gameObject.SetActive(!RenameInputField.gameObject.activeSelf);

        if (CurrentlySelected != null)
        {
            RectTransform _rect = RenameInputField.gameObject.GetComponent<RectTransform>();

            _rect.position = CurrentlySelected.gameObject.GetComponent<RectTransform>().position;
            _rect.sizeDelta = CurrentlySelected.gameObject.GetComponent<RectTransform>().sizeDelta;
            _rect.SetParent(null, true);
            _rect.SetParent(CurrentlySelected.gameObject.GetComponent<RectTransform>().parent, true);
            RenameInputField.Select();
        }
    }

    public void NewFolder()
    {
        string _New = "New Folder (0)";
        int iTry = 0;

        while (_FB.GetDirectoryPath(_New) != null)
        {
            _New = "New Folder (" + iTry + ")";

            iTry++;
        }

        System.IO.Directory.CreateDirectory(_FB.GetCurrentDirectory().FullName + "\\" + _New);
        _FB.Refresh();
        RefreshButtons();
    }

    public string NewFolder(string name)
    {
        string _New = name;
        int iTry = 0;

        while (_FB.GetDirectoryPath(_New) != null)
        {
            _New = name + " (" + iTry + ")";

            iTry++;
        }

        System.IO.Directory.CreateDirectory(_FB.GetCurrentDirectory().FullName + "\\" + _New);
        _FB.Refresh();
        RefreshButtons();

        return _FB.GetCurrentDirectory().FullName + "\\" + _New;
    }

    public void Delete()
    {
        if (CurrentlySelected != null)
        {
            RectTransform _CurrentRect = CurrentlySelected.gameObject.GetComponent<RectTransform>();
            string _text = _CurrentRect.FindChild("Text").gameObject.GetComponent<Text>().text;

            string FileToDelete = _FB.GetFilePath(_text);

            if (FileToDelete != null)
            {
                System.IO.File.Delete(FileToDelete);
            }
            else
            {
                string FolderToDelete = _FB.GetDirectoryPath(_text);

                if (FolderToDelete != null)
                {
                    System.IO.Directory.Delete(FolderToDelete);
                }
                else
                {
                    return;
                }
            }

            _FB.Refresh();
            RefreshButtons();
        }
    }

    public void SetSortMode(string mode)
    {
        if (mode == FileBrowser.SortingMode.Name.ToString())
            _FB.SetSortMode(FileBrowser.SortingMode.Name);
        else if (mode == FileBrowser.SortingMode.Date.ToString())
            _FB.SetSortMode(FileBrowser.SortingMode.Date);
        else if (mode == FileBrowser.SortingMode.Type.ToString())
            _FB.SetSortMode(FileBrowser.SortingMode.Type);

        SortModeOptions.parent.FindChild("Text").gameObject.GetComponent<Text>().text = "Sort by " + mode.ToLower() + ".";

        RefreshButtons();
        ShowSortMode();
    }

    public void ShowSortMode()
    {
        SortModeOptions.gameObject.SetActive(!SortModeOptions.gameObject.activeSelf);
    }
    #endregion

    #region Favorites
    private void CreateFavoriteFolder()
    {
        FileInfo _fav = new FileInfo(Application.dataPath + "\\favorites.fav");

        if (!_fav.Exists)
        {
            StreamWriter sw = System.IO.File.CreateText(Application.dataPath + "\\favorites.fav");
            sw.Close();
        }
    }

    private void RefreshFavorites()
    {
        FavoritePanel.sizeDelta = Vector2.zero;

        // Destroy previous buttons
        for (int i = 0; i < Favorites.Count; i++)
        {
            Favorites[i].gameObject.SetActive(false);
            DestroyImmediate(Favorites[i].gameObject);
        }

        // Clear List
        Favorites.Clear();

        StreamReader _sr = new StreamReader(Application.dataPath + "\\favorites.fav");

        string text = _sr.ReadToEnd();

        string[] paths = text.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        // Get height from template button
        float ButtonHeight = FavoriteTemplate.gameObject.GetComponent<RectTransform>().rect.height;

        for (int i = 0; i < paths.Length; i++)
        {
            DirectoryInfo dir = new DirectoryInfo(paths[i]);

            // Create new button
            Button NewButton = GameObject.Instantiate(FavoriteTemplate) as Button;
            NewButton.gameObject.SetActive(true);
            NewButton.gameObject.transform.SetParent(FavoritePanel.gameObject.transform, false);

            // Place button
            RectTransform _CurrentRect = NewButton.gameObject.GetComponent<RectTransform>();
            _CurrentRect.localPosition = new Vector3(-5, (-10) * (Favorites.Count + 7) + (-ButtonHeight) * (Favorites.Count + 6), 0);
            _CurrentRect.sizeDelta = new Vector2(-30, ButtonHeight);

            // Set button label to retrieve it later
            _CurrentRect.FindChild("Text").gameObject.GetComponent<Text>().text = dir.Name;
            _CurrentRect.FindChild("Name").gameObject.GetComponent<Text>().text = dir.FullName;
            NewButton.gameObject.name = "Favorite";

            // Add the button callback
            NewButton.onClick.RemoveAllListeners();
            NewButton.onClick.AddListener(() => OpenFavorite(NewButton));

            // Add the button to the list
            Favorites.Add(NewButton);

            // Resize parent panel to fit the new button
            FavoritePanel.sizeDelta = new Vector2(0, 10 * (Favorites.Count + 7) + ButtonHeight * (Favorites.Count + 6));
        }

        _sr.Dispose();
    }

    public void OpenFavorite(Button _Button)
    {
        RectTransform _CurrentRect = _Button.gameObject.GetComponent<RectTransform>();
        _FB.Relocate(_CurrentRect.FindChild("Name").gameObject.GetComponent<Text>().text);
        RefreshButtons();
    }

    public void AddToFavs()
    {
        if (_FB.GetCurrentDirectory() == null)
            return;

        StreamReader _sr = new StreamReader(Application.dataPath + "\\favorites.fav");

        string text = _sr.ReadToEnd();
        string[] paths = text.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);

        bool bExist = false;
        string AllDirs = string.Empty;
        for (int i = 0; i < paths.Length; i++)
        {
            DirectoryInfo dir = new DirectoryInfo(paths[i]);

            if (dir.FullName == _FB.GetCurrentDirectory().FullName)
            {
                bExist = true;
            }
            else
            {
                AllDirs += dir.FullName;
                AllDirs += "\n";
            }
        }

        _sr.Dispose();

        if (!bExist)
        {
            StreamWriter _sw = new StreamWriter(Application.dataPath + "\\favorites.fav", true);
            _sw.WriteLine("\n" + _FB.GetCurrentDirectory().FullName);
            _sw.Dispose();
            FavoriteImage.color = new Color(1, 1, 0, 1);
        }
        else
        {
            StreamWriter _sw = new StreamWriter(Application.dataPath + "\\favorites.fav");
            _sw.Write(AllDirs);
            _sw.Dispose();
            FavoriteImage.color = new Color(1, 1, 1, 1);
        }

        RefreshFavorites();
    }
    #endregion

    #region Filters
    // Function called to create the filters drop down list
    private void CreateFiltersOptions(List<string[]> Filters)
    {
        for (int i = 0; i < Filters.Count; i++)
        {
            // Create a new filter button
            Button NewFilter = GameObject.Instantiate(FilterItem) as Button;
            NewFilter.gameObject.SetActive(true);
            NewFilter.gameObject.transform.SetParent(FiltersOptions.gameObject.transform);

            // Place it
            RectTransform _CurrentRect = NewFilter.gameObject.GetComponent<RectTransform>();
            _CurrentRect.localPosition = new Vector3(0, 10 + 20 * i, 0);
            _CurrentRect.sizeDelta = new Vector2(0, _CurrentRect.rect.height);

            // Create button label by concatenating each filter
            string text = string.Empty;

            if (Filters[i] != null)
            {
                for (int j = 0; j < Filters[i].Length; j++)
                {
                    text += "\"" + Filters[i][j] + "\" ";
                }
            }
            else
                text = "\".*\"";

            _CurrentRect.FindChild("Text").gameObject.GetComponent<Text>().text = text;

            // Add new callback
            NewFilter.onClick.RemoveAllListeners();
            NewFilter.onClick.AddListener(() => ChangeFilters(NewFilter));

            // Add filter
            FiltersList.Add(NewFilter);
        }

        // Set default filter
        SelectedFilter.text = "\".*\"";
    }

    // Show the filters drop down list or hide it
    public void ShowFilters()
    {
        FiltersOptions.gameObject.SetActive(!FiltersOptions.gameObject.activeSelf);
    }

    // Callback for the filters buttons
    public void ChangeFilters(Button _Button)
    {
        // Set the selected filter tex
        RectTransform _CurrentRect = _Button.gameObject.GetComponent<RectTransform>();
        SelectedFilter.text = _CurrentRect.FindChild("Text").gameObject.GetComponent<Text>().text;

        // Retrieve the button index in the filters strings array

        int iIndex = 0;

        for (int i = 0; i < FiltersList.Count; i++)
        {
            if (_Button == FiltersList[i])
            {
                iIndex = i;
                break;
            }
        }

        // Set the new filter

        _FB.SetFilters(FiltersStrings[iIndex]);

        // Hide the filters and refresh the buttons with the new filter applied
        ShowFilters();
        RefreshButtons();
    }
    #endregion

    #region Search
    public void Search()
    {
        Loading.SetActive(true);

        CancelCurrentThumbnails();

        MainPanel.sizeDelta = Vector2.zero;

        // Destroy previous buttons
        for (int i = 0; i < Buttons.Count; i++)
        {
            Buttons[i].gameObject.SetActive(false);
            DestroyImmediate(Buttons[i].gameObject);
        }

        // Clear List
        Buttons.Clear();

        _FB.SearchFor(SearchField.text);

        StartCoroutine(WaitResults());
    }

    private IEnumerator WaitResults()
    {
        StartCoroutine(_FB.WaitForSearchResult());

        while (!_FB.IsSearchComplete())
        {
            RefreshButtons();
            yield return new WaitForSeconds(.5f);
        }

        RefreshButtons();

        Loading.SetActive(false);
    }
    #endregion

    #region AdressBar
    public void SetNewPath()
    {
        _FB.Relocate(CurrentAdress.text);
        RefreshButtons();
    }
    #endregion

    #region Thumbnails
    // Quit Thread
    public void QuitThumbnailsThread()
    {
        CancelThread = true;

        while (CancelThread)
        {
            Thread.Sleep(20);
        }

        ThumbnailsThread.Abort();
    }

    // Quit Current Generation
    public void CancelCurrentThumbnails()
    {
        if (!GenerateNewThumbnails)
            return;

        CancelGeneration = true;

        while (CancelGeneration)
        {
            Thread.Sleep(20);
        }
    }

    // Main function
    private void GenerateThumbnails()
    {
        while (true)
        {
            if (GenerateNewThumbnails)
            {
                m_bGenerationCompleted = false;

                lock (_ThumbnailsFiles)
                {
                    // Add a button for each file
                    for (int i = 0; i < _ThumbnailsFiles.Length; i++)
                    {
                        if (_ThumbnailsFiles[i].Extension.ToLower() == ".png"
                            || _ThumbnailsFiles[i].Extension.ToLower() == ".jpg"
                            || _ThumbnailsFiles[i].Extension.ToLower() == ".jpeg"
                            || _ThumbnailsFiles[i].Extension.ToLower() == ".gif"
                            || _ThumbnailsFiles[i].Extension.ToLower() == ".bmp")
                        {
                            ThumbData _data = new ThumbData();
                            _data._fullName = _ThumbnailsFiles[i].FullName;
                            _data._name = _ThumbnailsFiles[i].Name;

                            try
                            {
                                System.Drawing.Image image = System.Drawing.Image.FromFile(_data._fullName);
                                System.Drawing.Image thumb = image.GetThumbnailImage(ThumbSize, ThumbSize, () => false, System.IntPtr.Zero);

                                _data._bytes = imageToByteArray(thumb);

                                if (image.Width == image.Height)
                                {
                                    _data.SizeX = ThumbSize;
                                    _data.SizeY = ThumbSize;
                                }
                                else if (image.Width > image.Height)
                                {
                                    _data.SizeX = ThumbSize;
                                    _data.SizeY = ThumbSize * ((float)image.Height / (float)image.Width);
                                }
                                else if (image.Width < image.Height)
                                {
                                    _data.SizeX = ThumbSize * ((float)image.Width / (float)image.Height);
                                    _data.SizeY = ThumbSize;
                                }

                                thumb.Dispose();
                                image.Dispose();
                            }
                            catch (System.ArgumentException e)
                            {
                                Debug.LogError("ArgumentException when generating thumbnail " + i + ".\n\n" + e.Message);
                            }
                            catch (System.IO.FileNotFoundException e)
                            {
                                Debug.LogError("FileNotFoundException when generating thumbnail " + i + ".\n\n" + e.Message);
                            }
                            catch (System.OutOfMemoryException e)
                            {
                                Debug.LogError("OutOfMemoryException when generating thumbnail " + i + ".\n\n" + e.Message);
                            }
                            catch
                            {
                                Debug.LogError("Unknow error when generating thumbnail " + i + ".\n\n");
                            }

                            lock (m_pImagesBytesArray)
                            {
                                m_pImagesBytesArray.Add(_data);
                            }

                            if (CancelThread || CancelGeneration)
                            {
                                break;
                            }
                        }
                    }
                }

                m_bGenerationCompleted = true;
                GenerateNewThumbnails = false;
            }

            if (CancelThread)
            {
                CancelThread = false;
                break;
            }

            if (CancelGeneration)
            {
                m_bGenerationCompleted = true;
                GenerateNewThumbnails = false;
                lock (m_pImagesBytesArray)
                {
                    m_pImagesBytesArray.Clear();
                }

                lock (_ThumbnailsFiles)
                {
                    _ThumbnailsFiles = new FileInfo[0];
                }
                CancelGeneration = false;
            }

            Thread.Sleep(100);
        }
    }

    // Convert a System.Drawing.Image to a byte array so we can convert it to a Texture2D later
    public byte[] imageToByteArray(System.Drawing.Image imageIn)
    {
        using (var ms = new MemoryStream())
        {
            imageIn.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
            return ms.ToArray();
        }
    }

    // Coroutine to display new thumbnails at set intervales
    public IEnumerator WaitForThumbnails()
    {
        while (true)
        {
            lock (m_pImagesBytesArray)
            {
                if (m_pImagesBytesArray.Count > 0)
                {
                    for (int i = 0; i < m_pImagesBytesArray.Count; i++)
                    {
                        for (int j = 0; j < Buttons.Count; j++)
                        {
                            RectTransform _CurrentRect = Buttons[j].gameObject.GetComponent<RectTransform>();
                            string _name = _CurrentRect.FindChild("Text").gameObject.GetComponent<Text>().text;

                            if (m_pImagesBytesArray[i]._name == _name)
                            {
                                Image _pic = Buttons[j].gameObject.transform.FindChild("Image").gameObject.GetComponent<Image>();

                                Texture2D _thumbnail = new Texture2D(2, 2);

                                _thumbnail.LoadImage(m_pImagesBytesArray[i]._bytes);

                                Sprite _sprite = Sprite.Create(_thumbnail, new Rect(0, 0, _thumbnail.width, _thumbnail.height), new Vector2(0.0f, 0.0f));

                                _pic.sprite = _sprite;
                                _pic.rectTransform.sizeDelta = new Vector2(m_pImagesBytesArray[i].SizeX - 10, m_pImagesBytesArray[i].SizeY - 10);
                            }

                            if (CancelGeneration || CancelThread)
                            {
                                break;
                            }
                        }
                    }
                }

                m_pImagesBytesArray.Clear();
            }

            if (m_bGenerationCompleted)
            {
                break;
            }
            else
            {
                yield return new WaitForSeconds(0.2f);
            }
        }

        _ThumbnailsFiles = new FileInfo[0];
        m_bGenerationCompleted = false;
    }

    public void CheckButtonsVisibility()
    {
        for (int i = 0; i < Buttons.Count; i++)
        {
            if (Buttons[i].gameObject.transform.position.y > 1200)
            {
                Buttons[i].gameObject.SetActive(false);
            }
            else if (Buttons[i].gameObject.transform.position.y < 100)
            {
                Buttons[i].gameObject.SetActive(false);
            }
            else
            {
                Buttons[i].gameObject.SetActive(true);
            }
        }
    }
    #endregion
}
