using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler {

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        Draggable dragscript = eventData.pointerDrag.GetComponent<Draggable>();
        if (dragscript != null)
        {
            dragscript.placeholderParent = this.transform;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerDrag == null) return;
        Draggable dragscript = eventData.pointerDrag.GetComponent<Draggable>();
        if (dragscript != null && dragscript.originalParent == this.transform)
        {
            dragscript.placeholderParent = dragscript.originalParent;
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        Draggable dragscript = eventData.pointerDrag.GetComponent<Draggable>();
        if (dragscript != null)
        {
            dragscript.originalParent = this.transform;
        }
    }

}
