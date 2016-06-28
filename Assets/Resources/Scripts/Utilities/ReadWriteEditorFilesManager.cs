using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ICSharpCode.SharpZipLib.Tar;
using LitJson;
using ICSharpCode.SharpZipLib.GZip;
using UnityEngine.UI;

public class ReadWriteEditorFilesManager : MonoBehaviour
{
    private static ReadJSON ReadJson;
    private static WriteJSON WriteJson;
    private static ReadTARGZIP ReadTarGzip;
    private static WriteTARGZIP WriteTarGzip;

    public static bool isRunningCoroutine = false;


    #region READ_FUNCTIONS

    public static IEnumerator ReadCardFromCardFileToCurrentCardCoroutine(Image CardImage, InputField CardNameInputField, InputField CardTextInputField, string directory, string id)
    {
        isRunningCoroutine = true;

        string json_and_image_id = "0";

        // 1) Open ".card" by parsing and decompressing the ".tar.gzip" and put it into a compatible Json and TextureImage DAO
        ReadTarGzip.ReadCardFromTGZ(id + ".cse-card", directory);
        // 2) Read Json
        ReadJson.ReadCardFromJSON(CardNameInputField, CardTextInputField, directory, json_and_image_id);
        // 3) Read TextureImage
        CardImage.overrideSprite = ReadCardTextureFromPNGTextureImageData(directory, json_and_image_id);

        //After we're done reading the uncompressed stuff, we need to pretend they never were extracted and clean stuff
        if (directory == null)
        {
            directory = Application.dataPath + "/Card Set Editor Projects/";
        }

        File.Delete(directory + json_and_image_id + ".json");
        File.Delete(directory + json_and_image_id + ".png");

        yield return null;
        isRunningCoroutine = false;
    }

    public static IEnumerator ReadCardFromCardFileAndImportToCardListCoroutine(GameObject UICardList, string directory)
    {
        isRunningCoroutine = true;

        // 1) Open ".card" by parsing and decompressing the ".tar.gzip" and put it into a compatible Json and TextureImage DAO

        // 2) From the Json and Texture Image get the original card info and create a new CardPreview to insert into the UICardList from it

        yield return null;
        isRunningCoroutine = false;
    }

    public static IEnumerator ReadCardsFromCardSetEditorProjectAndImportToCardListCoroutine(GameObject UICardList, string directory)
    {
        isRunningCoroutine = true;

        // 1) Open the ".project" by parsing and decompressing the ".tar.gzip" and put all ".card" files onto a Project DAO (containing a list of ".card")  

        // 1) For each ".card" of the list Open it by parsing and decompressing the ".tar.gzip" and put each onto a compatible Json and TextureImage DAO
        // 1.2) From the Json and Texture Image get the original card info and create a new CardPreview to insert into the UICardList from it

        yield return null;
        isRunningCoroutine = false;
    }

    #endregion

    #region WRITE_FUNCTIONS

    public static IEnumerator WriteCurrentCardToCardFileCoroutine(Image CardImage, InputField CardNameInputField, InputField CardTextInputField, string directory, string fileName)
    {
        isRunningCoroutine = true;

        string id = "0";

        // 1) Generate JSON
        string cardJsonFileDireString = WriteJson.WriteCardAsJSON(id, CardNameInputField.text, CardTextInputField.text, directory);
        // 2) Extract TextureImage
        string cardPNGFileString = WriteCardTextureAsPNGTextureImageData(id, CardImage.overrideSprite.texture, directory);
        // 3) Put them onto Json and TextureImage DTO
        // 4) Create ".card" by parsing and compressing a ".tar.gzip" from the generated Json and TextureImage DTO
        WriteTarGzip.WriteTarGzipCard(fileName, new WriteTARGZIP.CardTarGzipDto(cardJsonFileDireString, cardPNGFileString), directory);

        yield return null;
        isRunningCoroutine = false;
    }

    /// <summary>
    /// This guy is used by the WriteAllCardsFromCardListToCardFilesCoroutine
    /// </summary>
    private static void WriteCurrentCardToCardFile(string id, Texture2D CardTexture, string CardName, string CardText, string directory)
    {
        // 1) Generate JSON
        string cardJsonFileDireString = WriteJson.WriteCardAsJSON(id, CardName, CardText, directory);
        // 2) Extract TextureImage
        string cardPNGFileString = WriteCardTextureAsPNGTextureImageData(id, CardTexture, directory);
        // 3) Put them onto Json and TextureImage DTO
        // 4) Create ".card" by parsing and compressing a ".tar.gzip" from the generated Json and TextureImage DTO
        WriteTarGzip.WriteTarGzipCard(id, new WriteTARGZIP.CardTarGzipDto(cardJsonFileDireString, cardPNGFileString), directory);

    }

    public static IEnumerator WriteAllCardsFromCardListToCardFilesCoroutine(GameObject UICardList, string directory)
    {
        isRunningCoroutine = true;

        // 1) For each card on the Card List create its Json and TextureImage DTO
        foreach (Transform child_transform in UICardList.transform)
        {
            string id = child_transform.GetSiblingIndex().ToString();
            string CardName = null, CardText = null;
            Texture2D CardTexture = null;

            foreach (var child in child_transform.gameObject.GetComponentsInChildren<Image>())
            {
                if (child.name == "CardImage")
                {
                    CardTexture = child.overrideSprite.texture;
                }
            }


            foreach (var child in child_transform.gameObject.GetComponentsInChildren<Text>())
            {
                if (child.name == "CardName")
                {
                    CardName = child.text;
                }

                if (child.name == "CardTextPreview")
                {
                    CardText = child.text;
                }
            }

            // 2) Then call "WriteCurrentCardToCardFileCoroutine" by passing the DTO attributes as parameters
            WriteCurrentCardToCardFile(id, CardTexture, CardName, CardText, null);

        }



        yield return null;
        isRunningCoroutine = false;
    }

    #endregion

    #region READ_CLASSES
    internal class ReadTARGZIP
    {
        internal class CardTarGzipDao
        {
            public string JsonString { get; set; }
            public byte[] PngTextureImageBytes { get; set; }

            public CardTarGzipDao(string JsonString, byte[] PngTextureImageBytes)
            {
                this.JsonString = JsonString;
                this.PngTextureImageBytes = PngTextureImageBytes;
            }

        }

        internal class ProjectTarGzipDao
        {

        }

        public void ReadCardFromTGZ(string tgzArchiveName, string destFolder)
        {
            if (destFolder == null)
            {
                destFolder = Application.dataPath + "/Card Set Editor Projects/";
            }

            string tgzArchivePath = destFolder + tgzArchiveName;

            Stream inStream = File.OpenRead(tgzArchivePath);
            Stream gzipStream = new GZipInputStream(inStream);

            TarArchive tarArchive = TarArchive.CreateInputTarArchive(gzipStream);
            tarArchive.ExtractContents(destFolder);
            tarArchive.Close();

            gzipStream.Close();
            inStream.Close();
        }


    }

    internal class ReadJSON
    {
        private JsonData cardJsonData;


        internal class CardJsonDao
        {
            public string name { get; set; }
            public string text { get; set; }

            public CardJsonDao(string name, string text)
            {
                this.name = name;
                this.text = text;
            }

        }

        /// <summary>
        /// Reads a JSON from specified parameters, aiming to load a card.
        /// If the directory isn't specified (that is, a 'null' is passed as value)
        /// it will read the JSON from the Current Project folder.
        public void ReadCardFromJSON(InputField CardNameInputField, InputField CardTextInputField, string directory, string id)
        {
            if (directory == null)
            {
                directory = Application.dataPath + "/Card Set Editor Projects/";
            }

            // Of course in the future the person will have to select a file
            string jsonString = File.ReadAllText(directory + id + ".json");
            cardJsonData = JsonMapper.ToObject(jsonString);
            var cardJsonDao = new CardJsonDao((string)cardJsonData["name"], (string)cardJsonData["text"]);


            CardNameInputField.text = cardJsonDao.name;

            CardTextInputField.text = cardJsonDao.text;

        }

        public void ReadCardFromJSONAndAddToUICardList(GameObject UICardList, string directory)
        {
            if (directory == null)
            {
                directory = Application.dataPath + "/Card Set Editor Projects/";
            }

        }

        /// <summary>
        /// Use this method to Import all cards from a project file.
        /// Condition: 
        /// This method will simply read the cards (if possible) and add
        /// them to the current Card List. It won't save the current project
        /// or switch to a new project or whatever. That has to be done
        /// externally.
        /// </summary>
        public void ReadCardsFromCardSetEditorProjectAndImport(GameObject UICardList, string directory)
        {
            if (directory == null)
            {
                directory = Application.dataPath + "/Card Set Editor Projects/";
            }

        }


    }
    #endregion

    #region WRITE_CLASSES
    internal class WriteTARGZIP
    {
        internal class CardTarGzipDto
        {
            public string cardJsonFileDireString { get; set; }
            public string cardPNGFileString { get; set; }

            public CardTarGzipDto(string cardJsonFileDireString, string cardPNGFileString)
            {
                this.cardJsonFileDireString = cardJsonFileDireString;
                this.cardPNGFileString = cardPNGFileString;
            }
        }

        internal class ProjectTarGzipDto
        {
            public List<CardTarGzipDto> cardTarGzipDtoList { get; set; }

            public ProjectTarGzipDto(List<CardTarGzipDto> cardTarGzipDtoList)
            {
                this.cardTarGzipDtoList = cardTarGzipDtoList;
            }
        }


        public void WriteTarGzipCard(string id, CardTarGzipDto cardTarGzipDto, string directory)
        {
            if (directory == null)
            {
                directory = Application.dataPath + "/Card Set Editor Projects/";
            }

            //Todo: Fix a Bug where the TGZ wont compress properly inside "Application.dataPath/Card Set Editor Projects/"

            Stream outStream = File.Create(directory + id + ".cse-card");
            Stream gzoStream = new GZipOutputStream(outStream);
            TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);

            // Note that the RootPath is currently case sensitive and must be forward slashes e.g. "c:/temp"
            // and must not end with a slash, otherwise cuts off first char of filename
            // This is scheduled for fix in next release
            tarArchive.RootPath = directory.Replace('\\', '/');
            if (tarArchive.RootPath.EndsWith("/"))
                tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

            // Add a Copy of these two guys to our tgz
            AddDirectoryFilesToTar(tarArchive, cardTarGzipDto.cardJsonFileDireString);
            AddDirectoryFilesToTar(tarArchive, cardTarGzipDto.cardPNGFileString);
            // Delete the originals
            File.Delete(cardTarGzipDto.cardJsonFileDireString);
            File.Delete(cardTarGzipDto.cardPNGFileString);

            tarArchive.Close();
        }

        public void WriteTarGzipProject(string id, ProjectTarGzipDto projectTarGzipDto, string directory)
        {
            if (directory == null)
            {
                directory = Application.dataPath + "/Card Set Editor Projects/";
            }

            Stream outStream = File.Create(directory + id + ".cse-project");
            Stream gzoStream = new GZipOutputStream(outStream);
            TarArchive tarArchive = TarArchive.CreateOutputTarArchive(gzoStream);

            // Note that the RootPath is currently case sensitive and must be forward slashes e.g. "c:/temp"
            // and must not end with a slash, otherwise cuts off first char of filename
            // This is scheduled for fix in next release
            tarArchive.RootPath = directory.Replace('\\', '/');
            if (tarArchive.RootPath.EndsWith("/"))
                tarArchive.RootPath = tarArchive.RootPath.Remove(tarArchive.RootPath.Length - 1);

            foreach (var cardTarGzipDto in projectTarGzipDto.cardTarGzipDtoList)
            {
                // Add a Copy of these two guys to our tgz
                AddDirectoryFilesToTar(tarArchive, cardTarGzipDto.cardJsonFileDireString);
                AddDirectoryFilesToTar(tarArchive, cardTarGzipDto.cardPNGFileString);
                // Delete the originals
                File.Delete(cardTarGzipDto.cardJsonFileDireString);
                File.Delete(cardTarGzipDto.cardPNGFileString);
            }

            tarArchive.Close();
        }


        private void AddDirectoryFilesToTar(TarArchive tarArchive, string sourceDirectory)
        {
            TarEntry tarEntry = TarEntry.CreateEntryFromFile(sourceDirectory);
            tarArchive.WriteEntry(tarEntry, false);
        }

    }

    internal class WriteJSON
    {
        private JsonData cardJsonData;

        internal class CardJsonDto
        {
            public string id { get; set; }
            public string name { get; set; }
            public string text { get; set; }

            public CardJsonDto(string id, string name, string text)
            {
                this.id = id;
                this.name = name;
                this.text = text;
            }

        }

        /// <summary>
        /// Writes a JSON from specified parameters, aiming to represent a card.
        /// If the directory isn't specified (that is, a 'null' is passed as value)
        /// it will write the JSON on the Current Project folder.
        /// </summary>
        public string WriteCardAsJSON(string id, string CardName, string CardText, string directory)
        {
            if (directory == null)
            {
                directory = Application.dataPath + "/Card Set Editor Projects/";
            }

            // All is set-up! 
            // Time to Write the JSON!
            cardJsonData = JsonMapper.ToJson(new CardJsonDto(id, CardName, CardText));
            File.WriteAllText(directory + id + ".json", cardJsonData.ToString());
            return directory + id + ".json";
        }
        public void WriteCardsFromCardListAsJSON(GameObject UICardList, string directory)
        {
            // Path related stuff
            if (directory == null)
            {
                directory = Application.dataPath + "/Card Set Editor Projects/";
            }

            // Let's traverse all created cards
            foreach (Transform child_transform in UICardList.transform)
            {
                int id = child_transform.GetSiblingIndex();
                string name = null, text = null;

                foreach (var child in child_transform.gameObject.GetComponentsInChildren<Text>())
                {
                    if (child.name == "CardName")
                    {
                        name = child.text;
                    }

                    if (child.name == "CardTextPreview")
                    {
                        text = child.text;
                    }
                }

                // All is set-up! 
                // Time to Write the JSON!
                cardJsonData = JsonMapper.ToJson(new CardJsonDto(id.ToString(), name, text));
                File.WriteAllText(directory + id.ToString() + ".json", cardJsonData.ToString());
            }

        }
    }

    #endregion

    #region READ_WRITE_TEXTUREIMAGES_FUNCTIONS

    private static string WriteCardTextureAsPNGTextureImageData(string id, Texture2D CardTexture, string directory)
    {
        if (directory == null)
        {
            directory = Application.dataPath + "/Card Set Editor Projects/";
        }

        byte[] pngBytes = CardTexture.EncodeToPNG();
        File.WriteAllBytes(directory + id + ".png", pngBytes);
        return directory + id + ".png";
    }
    private static Sprite ReadCardTextureFromPNGTextureImageData(string directory, string id)
    {
        if (directory == null)
        {
            directory = Application.dataPath + "/Card Set Editor Projects/";
        }

        byte[] pngBytes = System.IO.File.ReadAllBytes(directory + id + ".png");
        Texture2D CardTexture = new Texture2D(2, 2);
        CardTexture.LoadImage(pngBytes);

        Sprite sprite = new Sprite();
        sprite = Sprite.Create(CardTexture, new Rect(0, 0, CardTexture.width, CardTexture.height), new Vector2(0, 0));

        return sprite;
    }

    #endregion

    void Start()
    {
        ReadJson = new ReadJSON();
        WriteJson = new WriteJSON();
        ReadTarGzip = new ReadTARGZIP();
        WriteTarGzip = new WriteTARGZIP();
    }

}
