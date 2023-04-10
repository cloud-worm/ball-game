using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Mathematics;
using TMPro;

namespace BallGame
{
    // All player logic, including input, GameObject behaviour, and level environment handling.
    public class Player : MonoBehaviour
    {
        // Get current scene's level details
        private LevelDetails currentLevel;
        
        // Editor variables
        [Header("Player Control Settings")]
        [SerializeField] private float minPower = 1f;
        [SerializeField] private float maxPower = 10f;
        [Space]
        [Header("Objects/Components")]
        [SerializeField] private LayerMask wallLayer = 3;
        [SerializeField] private GameObject pointerContainer;
        [SerializeField] private GameObject pointer;
        [SerializeField] private Rigidbody2D rigidBody2D;
        [Space]
        [Header("Attempts")]
        [SerializeField] private TextMeshProUGUI shotsLabel;
        [SerializeField] private GameObject restartLabel;
        [SerializeField] private float timeBeforeRestart = 1.5f;
        [SerializeField] private bool infiniteShots = false;

        private int shots; // Variable to keep track of amount of shots

        private bool gotObjective = false; // Stays true if any IsObjective()
        private State currentState = State.Playing;

        private float interp = 0; // Interpolant for variable force calculation
        private float force; // Value of force to be applied to the ball

        private Vector2 startPos;// = new Vector2(); // Start position of screen touch movement
        private Vector2 offset; // Represents the direction of the player's touch movement

        private void Awake()
        {
            if (rigidBody2D == null)
                rigidBody2D = GetComponent<Rigidbody2D>();
            // Initialise current level info
            currentLevel = GameObject.FindGameObjectWithTag("origin").GetComponent<LevelDetails>();
        }

        private void Start()
        {
            // timeScale is global, we must reset it
            Time.timeScale = 1;

            // Hide the pointer (it shouldn't appear until we touch it), and the restart label
            pointerContainer.SetActive(false);
            restartLabel.SetActive(false);
            // Get amount of attempts
            shots = currentLevel.NumAttempts();
            // Show amount of attemps
            ShotsLabelUpd();
        }

        void Update()
        {
            // Debug
            Debug.Log(currentState.ToString());

            // Make the pointer follow the player
            pointerContainer.transform.position = transform.position;

            HandleTouches();
            
            // Legacy.
            //if (IsObjective())
            //{
            //    Time.timeScale = .1f;
            //    currentState = State.CanAdvance;
            //    restartLabel.GetComponent<TextMeshProUGUI>().text = "TOUCH SCREEN TO ADVANCE";
            //    restartLabel.SetActive(true);
            //}
            //else if (shots <= 0)
            //{
            //    currentState = State.Lost;
            //    // Indicate that shots are finished
            //    ShotsLabelUpd(State.Lost);
            //    StartCoroutine(restartMsg());
            //}

            if (IsObjective())
            {
                Time.timeScale = .1f;
                gotObjective = true;
            }

            if (shots <= 0)
            {
                StartCoroutine(endMsg());
            }
        }

        #region Touch Logic

        // Execute actions depending on the touch phase
        private void HandleTouch(int touchFingerId, Vector3 touchPosition, TouchPhase touchPhase)
        {
            switch (currentState)
            {
                case State.Playing:
                    switch (touchPhase)
                    {
                        case TouchPhase.Began:
                            TouchBegan();
                            break;
                        case TouchPhase.Moved:
                            TouchMoved();
                            break;
                        case TouchPhase.Ended:
                            TouchEnded();
                            break;
                    }
                    break;

                case State.CanRestart:
                    if (touchPhase == TouchPhase.Began)
                        TouchBeganRestart();
                    break;

                case State.CanAdvance:
                    if (touchPhase == TouchPhase.Began)
                        TouchBeganAdvance();
                    break;
            }
        }
        
        // Allow mouse touch simulation and touch action execution 
        private void HandleTouches()
        {
            // Handle native touch events
            foreach (Touch touch in Input.touches)
                HandleTouch(touch.fingerId, Camera.main.ScreenToWorldPoint(touch.position), touch.phase);

            // Simulate touch events from mouse events
            if (Input.touchCount == 0)
            {
                if (Input.GetMouseButtonDown(0))
                    HandleTouch(10, Camera.main.ScreenToWorldPoint(Input.mousePosition), TouchPhase.Began);
                if (Input.GetMouseButton(0))
                    HandleTouch(10, Camera.main.ScreenToWorldPoint(Input.mousePosition), TouchPhase.Moved);
                if (Input.GetMouseButtonUp(0))
                    HandleTouch(10, Camera.main.ScreenToWorldPoint(Input.mousePosition), TouchPhase.Ended);
            }
        }

        #endregion

        #region Touch Actions

        private void TouchBegan() { startPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y); }

        private void TouchBeganRestart() { SceneManager.LoadScene(SceneManager.GetActiveScene().name); }

        private void TouchBeganAdvance() { SceneManager.LoadScene("Level" + (currentLevel.number + 1).ToString()); }

        private void TouchMoved()
        {
            // `offset` is the vector from the mouse drag origin to the current mouse drag position
            offset = new Vector2(Input.mousePosition.x - startPos.x, Input.mousePosition.y - startPos.y);
            // Interpolant to calculate force of shot based on drag distance (`offset.magnitude`).
            // 1500 is an arbitrary value that works well (roughly based on screen resolutions)
            interp = math.clamp(offset.magnitude / 1500f, 0, 1);

            // Slow down time!
            //Time.timeScale = .5f;

            // Color pointer depending on ability to shoot
            pointer.GetComponent<SpriteRenderer>().color = IsNormalWall() ? new Color(0, 0.422f, 1) : Color.red;
            // Show & rotate pointer when pressing (i.e. 'aiming'), but only when moved (otherwise, no good direction)
            RotatePointer();
            if (offset.magnitude != 0)
                pointerContainer.SetActive(true);
        }

        private void TouchEnded()
        {
            // Time is normal again.
            //Time.timeScale = 1f;
            // Hide pointer
            pointerContainer.SetActive(false);

            // Stay between `minPower` and `maxPower`, depending on that interpolant from before
            force = Mathf.Lerp(minPower, maxPower, interp);
            // Shoot if on wall
            if (IsNormalWall())
            {
                // Stall ball (otherwise, impossible to go up if falling fast, etc)
                rigidBody2D.velocity = Vector2.zero;
                // Shoot
                rigidBody2D.AddForce(offset.normalized * force, ForceMode2D.Impulse);
            }

            // `infiniteShots` is never used in the real game, it's mostly for debugging
            if (!infiniteShots)
            {
                // Decrease amount of shots and update indicator
                shots -= 1;
                ShotsLabelUpd();
            }
        }

        #endregion

        // Rotate and animate the direction pointer
        private void RotatePointer()
        {
            // Pointer rotation
            Vector3 mousePos = Input.mousePosition;
            Vector3 selfPos = Camera.main.WorldToScreenPoint(transform.position);
            mousePos.x = mousePos.x - selfPos.x;
            mousePos.y = mousePos.y - selfPos.y;
            float angle = Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
            pointerContainer.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

            // Pointer animation (change distance depending on force)
            //Vector3 relativePointerPos = new Vector3(Mathf.Lerp(.5f, 1f, interp), 0, 0);
            Vector3 relativePointerPos = Vector3.Lerp(new Vector3(.5f, 0), new Vector3(1f, 0), interp);
            Vector3 relativePointerScale = new Vector3(Mathf.Lerp(.18f, .4f, interp), Mathf.Lerp(.18f, .4f, interp), 0);
            pointer.transform.localScale = relativePointerScale;
            pointer.transform.localPosition = relativePointerPos;
        }

        private void ShotsLabelUpd(State state = State.Playing) 
        {
            shotsLabel.text = state == State.Lost ? "No more shots!" : shots.ToString();
        }

        // Get any kind of wall if touching it (can be objective, normal wall...). /*Allow a small buffer zone.*/
        private Collider2D CheckCollision() 
        { 
            return Physics2D.OverlapCircle(
                transform.position, 
                transform.localScale.magnitude / 4f /*+ transform.localScale.magnitude / 10f*/, 
                wallLayer); 
        }

        private bool IsNormalWall() { return CheckCollision() != null && CheckCollision().tag == "Wall"; }
        private bool IsObjective() { return CheckCollision() != null && CheckCollision().tag == "Objective"; }

        // Coroutine to wait 1 second before allowing level restart.
        private IEnumerator endMsg()
        {
            yield return new WaitForSeconds(timeBeforeRestart);
            if (gotObjective)
            {
                restartLabel.GetComponent<TextMeshProUGUI>().text = "TOUCH SCREEN TO ADVANCE";
                currentState = State.CanAdvance;
            }
            else
            {
                restartLabel.GetComponent<TextMeshProUGUI>().text = "TOUCH SCREEN TO RESTART";
                currentState = State.CanRestart;
            }
            restartLabel.SetActive(true);
        }
    }
}

public enum State
{
    Playing,
    Lost,
    CanRestart,
    Won,
    CanAdvance
}
