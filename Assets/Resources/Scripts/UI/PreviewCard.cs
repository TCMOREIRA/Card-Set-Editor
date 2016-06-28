using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class PreviewCard : MonoBehaviour
{
    // Upon deletion of a card, a id slot will be open. Say you deleted card 6 and next_id is now 9.
    // However upon opening a new project [Found a better way to do this! Hopefully it will work! [SiblingIndex]]
    // private static int next_id = 0;
    // private int id { get; set; }

    void Start()
    {
        // id = next_id;
        // next_id++;
    }

    public void OpenCardFromList(GameObject OpenThisCard)
    {
        if (OpenThisCard != MenuManagerCardSetEditor.CurrentSelectedPreviewCard
            && MenuManagerCardSetEditor.CurrentSelectedPreviewCard != null
            && MenuManagerCardSetEditor.LastCardBeingEditedSaved != MenuManagerCardSetEditor.CardBeingEditedGameObject)
        {
            // Open Warning Popup saying:
            // "Are you sure you want to open another card without saving? (Maybe you forgot to save the current one)"
            // "Nah! This card is bollocks. Open the other one! "  "Oh I forgot! Thanks! Save and Proceed!"
        }

        MenuManagerCardSetEditor.instance.OpenCardFromList(OpenThisCard);
        MenuManagerCardSetEditor.CurrentSelectedPreviewCard = this.gameObject;
    }

    // Deleting a Card needs the user to be able to Select a card from the list first
    public void DeleteCardFromList(GameObject deleteThisCard)
    {
        MenuManagerCardSetEditor.instance.DeleteCardFromList(deleteThisCard);
    }
}
