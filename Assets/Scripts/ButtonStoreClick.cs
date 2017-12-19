using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonStoreClick : MonoBehaviour {
    public GameObject store;

    private bool Show = false;

    void Start()
    {
        transform.GetComponent<Button>().onClick.AddListener(TaskOnClick);
    }

    public void TaskOnClick()
    {
        Show = !Show;
        if (store)
        {
            store.SetActive(Show);
            Managers.Units.DisableAllSelection();
        }
    }

    public bool IsStoreOpened()
    {
        return Show;
    }
}
