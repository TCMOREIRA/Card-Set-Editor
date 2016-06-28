using UnityEngine;
using System.Collections;

public class DragItem : MonoBehaviour {

    private Vector3 screenPoint;
    private Vector3 offset;

    void OnMouseDown() {
        screenPoint = Camera.main.WorldToScreenPoint(gameObject.transform.position);

    }

    void OnMouseDrag() {
        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, screenPoint.z);

        Vector3 curPos = Camera.main.ScreenToWorldPoint(curScreenPoint);
        gameObject.transform.position = curPos;
    }
}
