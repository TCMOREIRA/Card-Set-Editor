using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Draggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    /*
    Lots of stuff to fix. When using in a preview-card list, it doesnt feel quite right.
    There are bugs and a Null-Pointer problem. So for now I'll not use it in preview-card prefabs.
    */

    public Transform originalParent = null;
    public Transform placeholderParent = null;
    GameObject placeholder = null;
    [Tooltip("True: Hold Left mouse button to drag. False: Hold right mouse button to drag.")]
    public bool leftClick = true;
    [Tooltip("True: Middle Click also controls drag. False: Middle click does nothing.")]
    public bool middleClick = true;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!leftClick)
        {
            if (eventData.button == PointerEventData.InputButton.Left)
            {
                return;
            }
        }

        if (leftClick)
        {
            if (eventData.button == PointerEventData.InputButton.Right)
            {
                return;
            }
        }

        if (!middleClick)
        {
            if (eventData.button == PointerEventData.InputButton.Middle)
            {
                return;
            }
        }

        placeholder = new GameObject();
        placeholder.transform.SetParent(this.transform.parent);
        LayoutElement le = placeholder.AddComponent<LayoutElement>();
        le.preferredHeight = this.GetComponent<LayoutElement>().preferredHeight;
        le.preferredWidth = this.GetComponent<LayoutElement>().preferredWidth;
        le.flexibleHeight = 0;
        le.flexibleWidth = 0;
        placeholder.transform.SetSiblingIndex(this.transform.GetSiblingIndex());

        originalParent = this.transform.parent;
        placeholderParent = originalParent;
        this.transform.SetParent(this.transform.parent.parent);
        GetComponent<CanvasGroup>().blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        this.transform.position = eventData.position;
        if (placeholder.transform.parent != placeholderParent) { placeholder.transform.SetParent(placeholderParent); }
        int newSiblingIndex = placeholderParent.childCount;

        for (int i = 0; i < placeholderParent.childCount; i++)
        {
            if (this.transform.position.x < placeholderParent.GetChild(i).position.x)
            {
                newSiblingIndex = i;
                if (placeholder.transform.GetSiblingIndex() < newSiblingIndex)
                {
                    newSiblingIndex--;
                }
                break;
            }
        }

        placeholder.transform.SetSiblingIndex(newSiblingIndex);

    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.transform.SetParent(originalParent);
        this.transform.SetSiblingIndex(placeholder.transform.GetSiblingIndex());
        Destroy(placeholder);
        GetComponent<CanvasGroup>().blocksRaycasts = true;
    }

}
