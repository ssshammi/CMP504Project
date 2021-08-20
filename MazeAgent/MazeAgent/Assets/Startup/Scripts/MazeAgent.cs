using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Sensors.Reflection;
using System.Linq;
public class MazeAgent : Agent
{
    [Tooltip("How fast the agent moves forward")]
    private float moveSpeed = 10f;

    [Tooltip("How fast the agent turns")]
    public float turnSpeed = 180f; //not used

    [Tooltip("Prefab of the heart that appears when the mouse is fed")]
    public GameObject heartPrefab;

    [SerializeField]
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
        // 0000 -> NO WALLS
        // 1111 -> LEFT,RIGHT,UP,DOWN

        LEFT = 1, // 0001
        RIGHT = 2, // 0010
        UP = 4, // 0100
        DOWN = 8, // 1000

    }

    [Observable]
    public Direction currentDirection;
    [Observable]
    public int[] currentCellPosition = new int[2];

    [Observable]
    public WallState[,] gridLayout;

    public GameArea mazeArea;
    public float timeBetweenDecisionsAtInference;
    float m_TimeSinceDecision;
    private bool isFull; // If true, mouse has a full stomach

    /// <summary>
    /// Initial setup, called when the agent is enabled
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        gameArea = GetComponentInParent<GameArea>();
        gridLayout = mazeArea.mazeMap.GetComponent<MazeRenderer>().getMaze();
        rigidbody = GetComponent<Rigidbody>();

        currentCellPosition[0] = 0;
        currentCellPosition[1] = 0;
        currentDirection = Direction.UP;
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




        if (forwardAmount == k_Up)
        {
            // Apply movement

            transform.position = (transform.position + transform.forward * moveSpeed);//* moveSpeed
            Debug.Log(transform.position + "the forwardAmount is " + transform.forward);
        }

        // transform.Rotate(transform.up * turnAmount  * Time.fixedDeltaTime);
        if (turnAmount != 0f)
            transform.Rotate(transform.up * turnAmount, Space.World);
        // Apply a tiny negative reward every step to encourage action
        if (MaxStep > 0)
        {
            AddReward(-1f / MaxStep);
        }
        else
        {
            AddReward(-0.02f); // not able to complete in 200 steps
            EndEpisode();
        }
    }
    /*void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        //Check that it is being run in Play Mode, so it doesn't try to draw this in Editor mode

        //Draw a cube where the OverlapBox is (positioned where your GameObject is as well as a size)
        Gizmos.DrawWireCube(transform.position, new Vector3(0f, 0, 5f) * 5f);
    }*/
    [Tooltip("Selecting will turn on action masking. Note that a model trained with action " +
        "masking turned on may not behave optimally when action masking is turned off.")]
    public bool maskActions = true;
    const int k_NoAction = 0;  // do nothing!
    const int k_Up = 1;
    const int k_Down = 4;
    const int k_Left = 2;
    const int k_Right = 3;
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        // Mask the necessary actions if selected by the user.
        if (maskActions)
        {

            // Prevents the agent from picking an action that would make it collide with a wall
            RayPerceptionSensorComponent3D raySensor = this.GetComponent<RayPerceptionSensorComponent3D>();
            // sensor.RayLength  ;
            //RayPerceptionOutput
            var rayOutputs = RayPerceptionSensor
                .Perceive(raySensor.GetRayPerceptionInput())
                .RayOutputs;

            var lengthOfRayOutputs = RayPerceptionSensor
                .Perceive(raySensor.GetRayPerceptionInput())
                .RayOutputs
                .Length;
            Debug.Log(rayOutputs[0].HitTagIndex + "the ray length is " + 0);
            Debug.Log(rayOutputs[1].HitTagIndex + "the ray length is " + 1);
            Debug.Log(rayOutputs[2].HitTagIndex + "the ray length is " + 2);

            /*    if (rayOutputs[2].HitTagIndex ==0)
                {
                    actionMask.SetActionEnabled(1, 1, false); // no left
                }else
                 actionMask.SetActionEnabled(1, 1, true); // no left

                if (rayOutputs[1].HitTagIndex ==0)
                {
                    actionMask.SetActionEnabled(1, 2, false);// no right
                }else
                actionMask.SetActionEnabled(1, 2, true);// no right
                */
            if (rayOutputs[0].HitTagIndex == 0)
            {
                //branch  //actionsToDisable
                actionMask.SetActionEnabled(0, k_Up, false); //no forward
            }

            /* if ( tempState.HasFlag(WallState.DOWN))
             {
                 actionMask.SetActionEnabled(0, k_Up, false);
             }
            var hit = Physics.OverlapBox(transform.position, new Vector3(0f, 0, 5f) * 5f);
            if (hit.Where(col => col.gameObject.CompareTag("wall")).ToArray().Length == 0)
            {

                actionMask.SetActionEnabled(0, k_Up, false); //no forward
            }*/



        }
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
        if (Input.GetKey(KeyCode.W))
        {
            // move forward
            forwardAction = 1;
            Debug.Log("forward was pressed " + forwardAction);
            actionsOut.DiscreteActions.Array[0] = k_Up;
        }
        if (Input.GetKey(KeyCode.A))
        {
            // turn left
            turnAction = 1;
            actionsOut.DiscreteActions.Array[1] = turnAction;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            // turn right
            turnAction = 2;
            actionsOut.DiscreteActions.Array[1] = turnAction;
        }

        /*  if (Input.GetKey(KeyCode.A))
          {
              // turn left
              turnAction = 1;
              actionsOut.DiscreteActions.Array[0] = k_Left;
              actionsOut.DiscreteActions.Array[1] = turnAction;
          }

          else if (Input.GetKey(KeyCode.D))
          {
              // turn right
              turnAction = 2;
              ctionsOut.DiscreteActions.Array[0] = k_Right;
              actionsOut.DiscreteActions.Array[1] = turnAction;
          }
          else if (Input.GetKey(KeyCode.S))
          {
              // turn right
              turnAction = 3;
              actionsOut.DiscreteActions.Array[0] = k_Down;
              actionsOut.DiscreteActions.Array[1] = turnAction;
          }*/
        // Put the actions into the array


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

            AddReward(-0.1f);
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

        AddReward(10.0f / gameArea.getNumberOfTarget());
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
    public void FixedUpdate()
    {
        WaitTimeInference();
    }
    void WaitTimeInference()
    {

        if (Academy.Instance.IsCommunicatorOn)
        {
            RequestDecision();
        }
        else
        {
            if (m_TimeSinceDecision >= timeBetweenDecisionsAtInference)
            {
                m_TimeSinceDecision = 0f;
                RequestDecision();
            }
            else
            {
                m_TimeSinceDecision += Time.fixedDeltaTime;
            }
        }
    }

}

//mlagents-learn config/ppo/MazeAgent.yaml --run-id MazeAgent_Ray

//mlagents-learn config/ppo/MazeAgentRNN.yaml --run-id MazeAgent_RNA_Ray

