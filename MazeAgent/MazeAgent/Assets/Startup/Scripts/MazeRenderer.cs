using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class MazeRenderer : MonoBehaviour
{

    [SerializeField]
    [Range(1, 50)]
    private int width = 10;

    [SerializeField]
    [Range(1, 50)]
    private int height = 10;

    [SerializeField]
    private float size = 1f;

    [SerializeField]
    private Transform mazeContainer = null;


    [SerializeField]
    private Transform wallPrefab = null;

    [SerializeField]
    private Transform floorPrefab = null;

    /*
    [[137,133,141,137,131,131,131,133,139,133],[140,140,140,138,131,131,133,140,137,132],[140,138,134,137,131,133,140,140,140,140],[140,137,131,134,137,134,142,138,134,140],[140,140,137,131,132,137,131,131,131,134],[140,140,138,133,142,138,131,131,131,133],[138,134,137,134,137,131,129,129,135,140],[137,133,140,137,134,137,132,142,137,134],[140,138,134,138,133,140,142,137,134,141],[138,131,131,131,134,138,131,130,131,134]]
[[137,131,131,133,137,131,129,131,133,141],[140,137,133,138,134,141,136,133,138,134],[140,140,138,131,133,140,142,138,129,133],[140,138,133,137,134,138,131,133,142,140],[138,133,140,138,131,133,141,138,131,134],[137,134,138,133,137,134,136,135,137,133],[140,137,131,134,138,133,138,129,134,140],[140,136,133,139,133,138,133,140,137,132],[140,142,140,137,134,137,134,142,140,140],[138,131,134,138,131,130,131,131,134,142]]
   [[137,131,135,137,131,129,133,139,131,133],[136,131,131,134,137,134,138,133,137,132],[140,137,133,137,134,141,137,134,140,142],[140,140,142,138,133,140,138,133,138,133],[140,140,137,131,134,136,131,134,137,132],[140,140,138,131,133,138,133,139,132,140],[140,136,131,135,140,141,138,131,134,140],[140,140,137,131,134,136,133,137,133,140],[138,132,138,131,133,142,138,132,138,134],[139,130,131,135,138,131,135,138,131,135]]
    */
    private WallState[,] mazememory;
    // Start is called before the first frame update
    void Start()
    {
        size = size + 0.05f;
        var maze = MazeGenerator.Generate(width, height);
        //Debug.Log(JsonConvert.SerializeObject(maze[0,0].HasFlag(WallState.UP)));
        //Debug.Log(JsonConvert.SerializeObject(maze[0,0].HasFlag(WallState.RIGHT)));

        // Debug.Log(JsonConvert.SerializeObject(maze[0,0].HasFlag(WallState.LEFT)));
        string mazeJson = JsonConvert.SerializeObject(maze);
        Debug.Log(mazeJson);
        Debug.Log(JsonConvert.DeserializeObject<WallState[,]>(mazeJson));
        //  WallState[,] stringMaze = JsonHelper.getJsonArray(mazeJson);
        // Draw(stringMaze);
        Draw(JsonConvert.DeserializeObject<WallState[,]>("[[137, 131, 135, 137, 131, 129, 133, 139, 131, 133],[136,131,131,134,137,134,138,133,137,132],[140,137,133,137,134,141,137,134,140,142],[140,140,142,138,133,140,138,133,138,133],[140,140,137,131,134,136,131,134,137,132],[140,140,138,131,133,138,133,139,132,140],[140,136,131,135,140,141,138,131,134,140],[140,140,137,131,134,136,133,137,133,140],[138,132,138,131,133,142,138,132,138,134],[139,130,131,135,138,131,135,138,131,135]]"));

        mazememory = maze;
        mazeContainer.localScale = new Vector3(10, 0.5f, 10);
        mazeContainer.Translate(new Vector3(56.0f, 0.0f, 56f));

    }
    //return a grid in visited flag flase
    public WallState[,] getMaze()
    {

        /* foreach(var row in mazememory){
               foreach(WallState cell in row)

            cell |= WallState.VISITED;
         }*/

        return mazememory;
    }


    private void Draw(WallState[,] maze)
    {

        //  var floor = Instantiate(floorPrefab, transform);
        //   floor.localScale = new Vector3(width, 1, height);

        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                var cell = maze[i, j];
                var position = new Vector3(-width / 2 + i, 0, -height / 2 + j);

                if (cell.HasFlag(WallState.UP))
                {
                    var topWall = Instantiate(wallPrefab, transform) as Transform;
                    topWall.position = position + new Vector3(0, 0, size / 2);
                    topWall.localScale = new Vector3(size, topWall.localScale.y, topWall.localScale.z);
                }

                if (cell.HasFlag(WallState.LEFT))
                {
                    var leftWall = Instantiate(wallPrefab, transform) as Transform;
                    leftWall.position = position + new Vector3(-size / 2, 0, 0);
                    leftWall.localScale = new Vector3(size, leftWall.localScale.y, leftWall.localScale.z);
                    leftWall.eulerAngles = new Vector3(0, 90, 0);
                }

                if (i == width - 1)
                {
                    if (cell.HasFlag(WallState.RIGHT))
                    {
                        var rightWall = Instantiate(wallPrefab, transform) as Transform;
                        rightWall.position = position + new Vector3(+size / 2, 0, 0);
                        rightWall.localScale = new Vector3(size, rightWall.localScale.y, rightWall.localScale.z);
                        rightWall.eulerAngles = new Vector3(0, 90, 0);
                    }
                }

                if (j == 0)
                {
                    if (cell.HasFlag(WallState.DOWN))
                    {
                        var bottomWall = Instantiate(wallPrefab, transform) as Transform;
                        bottomWall.position = position + new Vector3(0, 0, -size / 2);
                        bottomWall.localScale = new Vector3(size, bottomWall.localScale.y, bottomWall.localScale.z);
                    }
                }
            }

        }

    }

    // Update is called once per frame
    void Update()
    {

    }
}



