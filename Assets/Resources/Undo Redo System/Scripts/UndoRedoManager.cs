using UnityEngine;
using System.Collections;

public class UndoRedoManager : MonoBehaviour
{
    private int steps = 0;
    public ObjectSystem _system = new ObjectSystem();

    [Tooltip("When 'true' user needs to hold control in conjunction with other keys. When 'false' holding control doesn't matter.")]
    public bool UseControl = true;
    [Tooltip("The key to use when you press to undo.")]
    public KeyCode UndoKey = KeyCode.Z;
    [Tooltip("The key to use when you press to redo.")]
    public KeyCode RedoKey = KeyCode.R;
    [Tooltip("The key to use when you press to Clear.")]
    public KeyCode ClearKey = KeyCode.Escape;
    [Tooltip("What Layer gets affected.")]
    public LayerMask ActiveLayer;
    [Tooltip("Object being tracked")]
    public GameObject _currentObj;

    // Use this for initialization
    void Start()
    {
        _system = new ObjectSystem();
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ActiveLayer))
            {
                _currentObj = hit.collider.gameObject;
                _system.Store(_currentObj, steps);
                steps = _system._spot;
            }
        }
        if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(0)) && _currentObj != null)
        {
            _system.Store(_currentObj, steps);
            steps = _system._spot;
        }

        if (!UseControl)
        {
            if (Input.GetKeyDown(ClearKey))
            {
                steps = _system._spot = 0;
                _system.StoredObj.Clear();
            }

            if (Input.GetKeyDown(UndoKey))
            {
                if (_system._spot > 1)
                {
                    steps--;
                    _system._spot--;
                    _system.Call(_system._spot - 1);
                }

            }

            if (Input.GetKeyDown(RedoKey))
            {
                if (_system._spot < _system.StoredObj.Count)
                {
                    steps++;
                    _system._spot++;
                    _system.Call(_system._spot - 1);
                }
            }
        }

        else
        {
            if (Input.GetKey(KeyCode.LeftControl))
            {
                if (Input.GetKeyDown(ClearKey))
                {
                    steps = _system._spot = 0;
                    _system.StoredObj.Clear();
                }

                if (Input.GetKeyDown(UndoKey))
                {
                    if (_system._spot > 1)
                    {
                        steps--;
                        _system._spot--;
                        _system.Call(_system._spot - 1);
                    }

                }

                if (Input.GetKeyDown(RedoKey))
                {
                    if (_system._spot < _system.StoredObj.Count)
                    {
                        steps++;
                        _system._spot++;
                        _system.Call(_system._spot - 1);
                    }
                }
            }
        }

    }

    /// <Undo and Redo Functions>
    /// To be used by click events
    /// </Undo and Redo Functions>
    public void Undo()
    {
        if (_system._spot > 1)
        {
            steps--;
            _system._spot--;
            _system.Call(_system._spot - 1);
        }
        Debug.Log("Hello");
    }

    public void Redo()
    {
        if (_system._spot < _system.StoredObj.Count)
        {
            steps++;
            _system._spot++;
            _system.Call(_system._spot - 1);
        }
    }

}
