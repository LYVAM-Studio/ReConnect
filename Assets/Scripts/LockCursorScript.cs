using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LockCursorScript : MonoBehaviour
{

    bool currentState = false;

    // Start is called before the first frame update
    void OnEnable()
    {
        currentState = false;
        SetCursor(currentState);
    }

    private void OnDisable()
    {
        currentState = true;
        SetCursor(currentState);
    }

    // Update is called once per frame
    void Update()
    {
        //toggle cursor on pressing escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            currentState = !currentState;
            SetCursor(currentState);
        }
    }


    void SetCursor(bool state)
    {
        Cursor.lockState = state ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = state;
    }

}
