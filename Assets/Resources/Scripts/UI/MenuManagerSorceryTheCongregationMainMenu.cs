using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Assets.Scripts.UI
{
    public class MenuManagerSorceryTheCongregationMainMenu : MonoBehaviour
    {
        // public Menu CurrentMenu;


        public void LoadCardSetEditorScene()
        {
            SceneManager.LoadScene("CardSetEditorScene");
            // SimpleSceneFader.ChangeSceneWithFade("CardSetEditorScene");
        }

    }
}
