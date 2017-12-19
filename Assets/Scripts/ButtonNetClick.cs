using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonNetClick : MonoBehaviour {
    public Grid grid;

    private bool Show = true;

    void Start()
    {
        transform.GetComponent<Button>().onClick.AddListener(TaskOnClick);
    }

    void TaskOnClick()
    {
        Show = !Show;
        if (grid)
            grid.ShowGrid(Show);
        if (Show)
        {
            transform.GetComponentInChildren<Text>().text = "Net Off";
        }
        else
        {
            transform.GetComponentInChildren<Text>().text = "Net On";
        }
    }
}
