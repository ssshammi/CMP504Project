using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellProp : MonoBehaviour
{
    // Start is called before the first frame update
    public int cellPositionRow = 0;
    public int cellPositionColumn = 0;

    Color[] colorBlock = new Color[] {
       new Color(0.219f, 0.219f, 0.219f),
       Color.green,
       Color.yellow,
       new Color(1f, 0.647f, 0f),
       Color.red
    };
    public WallState wallProp = 0000;

    public int visitedCount = 0;

    public void changeVisitedCount()
    {
        visitedCount++;
        //this.GetComponent<>();
        Color newcolor;
        if (visitedCount > 4) newcolor = new Color(0.313f, 0.196f, 0.470f);
        else
            newcolor = colorBlock[visitedCount];
        this.GetComponent<Renderer>().material.SetColor("_Color", newcolor);
    }

    public void resetVisited()
    {
        visitedCount = 0;
        //this.GetComponent<>();


        this.GetComponent<Renderer>().material.SetColor("_Color", colorBlock[0]);
    }

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
