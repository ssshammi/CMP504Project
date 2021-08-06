using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Sensors.Reflection;

public class MazeAgent : Agent
{
    [Tooltip("How fast the agent moves forward")]
    private float moveSpeed =5f;

    [Tooltip("How fast the agent turns")]
    public float turnSpeed = 180f;

    [Tooltip("Prefab of the heart that appears when the mouse is fed")]
    public GameObject heartPrefab;

     [ SerializeField]
        internal Vector3Int m_GridSize = new Vector3Int(10, 1, 10);
      [HideInInspector, SerializeField]
        internal bool m_ShowGizmos = true;
 /// <summary>
        /// Whether to show gizmos or not.
        /// </summary>
        public bool ShowGizmos
        {
            get { return m_ShowGizmos; }
            set { m_ShowGizmos = value; }
        }

     public Vector3Int GridSize
        {
            get { return m_GridSize; }
            set
            {

                if (value.y != 1)
                {
                    m_GridSize = new Vector3Int(value.x, 1, value.z);
                }
                else
                {
                    m_GridSize = value;
                }
            }
        }
    private GameArea gameArea;
    new private Rigidbody rigidbody;

    public struct Position
{
    public int row;
    public int collum;
}

public enum Direction
{
    North = 1,
    South = 2, // 0010
    East = 4, // 0100
    West = 8,
}

 [Observable]
public Direction currentDirection;
 [Observable]
public int[] currentCellPosition = new int[2];

[Observable]
public int[,] agentMemory;

    private bool isFull; // If true, mouse has a full stomach

    /// <summary>
    /// Initial setup, called when the agent is enabled
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        gameArea = GetComponentInParent<GameArea>();

        rigidbody = GetComponent<Rigidbody>();

    currentCellPosition[0] =1;
    currentCellPosition[1] =1;
    currentDirection = Direction.North;
    }

    /// <summary>
    /// Perform actions based on a vector of numbers
    /// </summary>
    /// <param name="actionBuffers">The struct of actions to take</param>
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        // Convert the first action to forward movement
        float forwardAmount = actionBuffers.DiscreteActions[0];

        // Convert the second action to turning left or right
        float turnAmount = 0f;
        if (actionBuffers.DiscreteActions[1] == 1f)
        {
            turnAmount = -90f;
        }
        else if (actionBuffers.DiscreteActions[1] == 2f)
        {
            turnAmount = 90f;
        }



        // Apply movement

        transform.position = (transform.position +transform.forward*forwardAmount  *moveSpeed) ;//* moveSpeed
         if(forwardAmount == 1f){
       Debug.Log(transform.position +"the forwardAmount is "+transform.forward);
        }

       // transform.Rotate(transform.up * turnAmount  * Time.fixedDeltaTime);
       if(turnAmount!=0f)
        transform.Rotate(transform.up * turnAmount, Space.World ) ;
        // Apply a tiny negative reward every step to encourage action
        if (MaxStep > 0) AddReward(1f / MaxStep);
    }

    /// <summary>
    /// Read inputs from the keyboard and convert them to a list of actions.
    /// This is called only when the player wants to control the agent and has set
    /// Behavior Type to "Heuristic Only" in the Behavior Parameters inspector.
    /// </summary>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        int forwardAction = 0;
        int turnAction = 0;
        if (Input.GetKeyUp(KeyCode.W))
        {
            // move forward
            forwardAction = 1;
             Debug.Log("forward was pressed "+forwardAction);
        }
        if (Input.GetKeyUp(KeyCode.A))
        {
            // turn left
            turnAction = 1;
        }
        else if (Input.GetKeyUp(KeyCode.D))
        {
            // turn right
            turnAction = 2;
        }

        // Put the actions into the array
        actionsOut.DiscreteActions.Array[0] = forwardAction;
        actionsOut.DiscreteActions.Array[1] = turnAction;
    }

    /// <summary>
    /// When a new episode begins, reset the agent and area
    /// </summary>
    public override void OnEpisodeBegin()
    {
        isFull = false;
        gameArea.ResetArea();
    }

    /// <summary>
    /// Collect all non-Raycast observations
    /// </summary>
    /// <param name="sensor">The vector sensor to add observations to</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Whether the mouse has eaten a cheese (1 float = 1 value)
        sensor.AddObservation(isFull);


         sensor.AddObservation(gameArea.getNumberOfTarget());

         // sensor.AddObservation(gameArea.getPostionOfTarget());

        // Direction mouse is facing (1 Vector3 = 3 values)
        sensor.AddObservation(transform.forward);

        // 1 + 1 + 3 + 3 = 8 total values
    }

    /// <summary>
    /// When the agent collides with something, take action
    /// </summary>
    /// <param name="collision">The collision info</param>
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("target"))
        {
            // Try to eat the cheese
            Eatcheese(collision.gameObject);

        }

        if (collision.transform.CompareTag("wall"))
        {
            // Try to eat the cheese

        AddReward(-1.0f);
        }

    }

    /// <summary>
    /// Check if agent is full, if not, eat the cheese and get a reward
    /// </summary>
    /// <param name="cheeseObject">The cheese to eat</param>
    private void Eatcheese(GameObject cheeseObject)
    {
        if (isFull) return; // Can't eat another cheese while full
       // isFull = true; // need to  define max hunger

        gameArea.RemoveSpecificcheese(cheeseObject);

        AddReward(2.0f/gameArea.getNumberOfTarget());
        CheckEnd();
    }

    /// <summary>
    /// Check if agent is full, if yes, exit the maze
    /// </summary>
   /* private void Regurgitatecheese()
    {
        if (!isFull) return; // Nothing to regurgitate
        isFull = false;


        // Spawn regurgitated cheese
        GameObject regurgitatedcheese = Instantiate<GameObject>(regurgitatedcheesePrefab);
        regurgitatedcheese.transform.parent = transform.parent;
        regurgitatedcheese.transform.position = transform.position;
        Destroy(regurgitatedcheese, 4f);
        AddReward(-1.0f);
    }*/
  private void CheckEnd()
    {
    if (!isFull) return; // Nothing to regurgitate
  //      isFull = false;
        // Spawn heart
        GameObject heart = Instantiate<GameObject>(heartPrefab);
        heart.transform.parent = transform.parent;
        heart.transform.position = transform.position + Vector3.up;
        Destroy(heart, 4f);



        if (gameArea.cheeseRemaining <= 0)
        {
            EndEpisode();
        }
    }
}

//mlagents-learn config/ppo/MazeAgent.yaml --run-id MazeAgent_Ray

//mlagents-learn config/ppo/MazeAgentRNN.yaml --run-id MazeAgent_RNA_Ray

