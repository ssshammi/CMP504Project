using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameArea : MonoBehaviour
{
    [Tooltip("The agent inside the area")]
    public MazeAgent mazeAgent;



    [Tooltip("The TextMeshPro text that shows the cumulative reward of the agent")]
    public TextMeshPro cumulativeRewardText;

    [Tooltip("Prefab of a target cheese")]
    public Unity.MLAgentsExamples.TargetController cheesePrefab;

    // [Tooltip("Prefab of the regurgitated cheese that appears when the chesee is rotten")]
    //public GameObject regurgitatedcheesePrefab;

    private List<GameObject> cheeseList;


    /// <summary>
    /// Reset the area, including cheese and mouse placement
    /// </summary>
    public void ResetArea()
    {
        RemoveAllcheese();
        PlaceMouse();

        Spawncheese(20, .5f);
    }

    /// <summary>
    /// Remove a specific cheese from the area when it is eaten
    /// </summary>
    /// <param name="cheeseObject">The cheese to remove</param>
    public void RemoveSpecificcheese(GameObject cheeseObject)
    {
        cheeseList.Remove(cheeseObject);
        Destroy(cheeseObject);
    }

    /// <summary>
    /// The number of cheese remaining
    /// </summary>
    public int cheeseRemaining
    {
        get { return cheeseList.Count; }
    }

    /// <summary>
    /// Choose a random position on the X-Z plane within a partial donut shape
    /// </summary>
    /// <param name="center">The center of the donut</param>
    /// <param name="minAngle">Minimum angle of the wedge</param>
    /// <param name="maxAngle">Maximum angle of the wedge</param>
    /// <param name="minRadius">Minimum distance from the center</param>
    /// <param name="maxRadius">Maximum distance from the center</param>
    /// <returns>A position falling within the specified region</returns>
    public static Vector3 ChooseRandomPosition(Vector3 center, float minAngle, float maxAngle, float minRadius, float maxRadius)
    {
        float radius = minRadius;
        float angle = minAngle;

        if (maxRadius > minRadius)
        {
            // Pick a random radius
            radius = UnityEngine.Random.Range(minRadius, maxRadius);
        }

        if (maxAngle > minAngle)
        {
            // Pick a random angle
            angle = UnityEngine.Random.Range(minAngle, maxAngle);
        }

        // Center position + forward vector rotated around the Y axis by "angle" degrees, multiplies by "radius"
        return center + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * radius;
    }


  public static Vector3 ChooseRandomPositionCell(Vector3 center, float minDistance, float maxDistance)
    {
        float radius = minDistance;
        float angle = maxDistance;

        //if (maxRadius > minRadius)
       // {
            // Pick a random radius
            radius = UnityEngine.Random.Range(0f, 10f) +6f;
      //  }

       // if (maxAngle > minAngle)
      //  {
            // Pick a random angle
      //      angle = UnityEngine.Random.Range(minAngle, maxAngle);
      //  }

        // Center position + forward vector rotated around the Y axis by "angle" degrees, multiplies by "radius"
           // return new Vector3(Random.Range(0f, 1f) *100-minDistance, 0f,Random.Range(0f, 1f) *100+ minDistance) + center;
           Debug.Log(Random.Range(0, 9));
            return new Vector3(Random.Range(0, 9) *minDistance, 0f,Random.Range(0, 9) * minDistance);
    }
    /// <summary>
    /// Remove all cheese from the areaf
    /// </summary>
    private void RemoveAllcheese()
    {
        if (cheeseList != null)
        {
            for (int i = 0; i < cheeseList.Count; i++)
            {
                if (cheeseList[i] != null)
                {
                    Destroy(cheeseList[i]);
                }
            }
        }

        cheeseList = new List<GameObject>();
    }

    /// <summary>
    /// Place the Mouse in the area
    /// </summary>
    private void PlaceMouse()
    {
        Rigidbody rigidbody = mazeAgent.GetComponent<Rigidbody>();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        mazeAgent.transform.position = new Vector3(6f,2.4f,9.5f);
      //  mazeAgent.transform.position = ChooseRandomPosition(transform.position, 0f, 0f, 0f, 0f) ;
       // mazeAgent.transform.rotation = Quaternion.Euler(0f, UnityEngine.Random.Range(0f, 360f), 0f);
    }



    /// <summary>
    /// Spawn some number of cheese in the area and set their swim speed
    /// </summary>
    /// <param name="count">The number to spawn</param>
    /// <param name="cheeseSpeed">The swim speed</param>  .satic object
    ///
    private void Spawncheese(int count, float cheeseSpeed)
    {

     /*     GameObject[] cheeseObjects;
        cheeseObjects = GameObject.FindGameObjectsWithTag("target");

        for (int i = 0; i < cheeseObjects.Length ; i++)
        {

              cheeseObjects[i].transform.SetParent(transform);
                cheeseList.Add(cheeseObjects[i].transform.gameObject);
        }

        return;*/
      for (int i = 0; i < count; i++)
        {
            // Spawn and place the cheese
            GameObject cheeseObject = Instantiate<GameObject>(cheesePrefab.gameObject);
            // add to random place on the maze
            cheeseObject.transform.position = ChooseRandomPositionCell( mazeAgent.transform.position , 6f, 6f) ;//+ Vector3.up * 1.1f
            cheeseObject.transform.rotation = Quaternion.Euler(0f,0f, 0f);

            // Set the cheese's parent to this area's transform
            cheeseObject.transform.SetParent(transform);

            // Keep track of the cheese
            cheeseList.Add(cheeseObject);

            // Set the cheese decay time
            //cheeseObject.GetComponent<cheese>().cheeseSpeed = cheeseSpeed;
        }
    }
    //we return the postion of the closet cheese asume rat has smell. because just with visit it not able to do anything
    public Vector3 getPostionOfTarget(){


        float minDistance = float.MaxValue;
         GameObject closestFood = null;
        for (int i = 0; i < cheeseList.Count; i++)
        {



            float thisDistance =Vector3.Distance(cheeseList[i].transform.position, mazeAgent.transform.position);
            if (thisDistance < minDistance)
            {
             minDistance = thisDistance;
             closestFood = cheeseList[i];
            }

        }
        return closestFood.transform.position;

    }

      public int getNumberOfTarget(){


       return cheeseList.Count;

    }



    /// <summary>
    /// Called when the game starts
    /// </summary>
    private void Start()
    {
        ResetArea();
    }

    /// <summary>
    /// Called every frame
    /// </summary>
    private void Update()
    {
        // Update the cumulative reward text
        cumulativeRewardText.text = mazeAgent.GetCumulativeReward().ToString("0.00");
    }

}
