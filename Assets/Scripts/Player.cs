using System;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

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
        [SerializeField] private GameObject pointerContainer;
        [SerializeField] private GameObject pointer;
        [SerializeField] private Rigidbody2D rigidBody2D;
        [Space]
        [Header("Attempts")]
        [SerializeField] private TextMeshProUGUI shotsLabel;
        [SerializeField] private bool infiniteShots = false;

        // Variable to keep track of amount of shots
        private int shots;

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
            // Hide the pointer (it shouldn't appear until we touch it)
            pointerContainer.SetActive(false);
            // Get amount of attempts
            shots = currentLevel.NumAttempts();
            // Show amount of attemps
            ShotsLabelUpd();
        }

        void Update()
        {
            // Make the pointer follow the player
            pointerContainer.transform.position = transform.position;

            // Can't shoot if no more shots!
            if (shots > 0)
            {
                // Handle screen touches and allow mouse touchscreen simulation
                // Also handle touch actions
                HandleTouches();
            } else
            {
                // Indicate that shots are finished
                ShotsLabelUpd(true);
            }
        }

        #region Touch Logic

        // Execute actions depending on the touch phase
        private void HandleTouch(int touchFingerId, Vector3 touchPosition, TouchPhase touchPhase)
        {
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

        private void TouchBegan()
        {
            startPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
        }

        private void TouchMoved()
        {
            // `offset` is the vector from the mouse drag origin to the current mouse drag position
            offset = new Vector2(Input.mousePosition.x - startPos.x, Input.mousePosition.y - startPos.y);
            // Interpolant to calculate force of shot based on drag distance (`offset.magnitude`).
            // 1500 is an arbitrary value that works well (roughly based on screen resolutions)
            interp = math.clamp(offset.magnitude / 1500f, 0, 1);
            // Show & rotate pointer when touching (i.e. 'aiming')
            pointerContainer.SetActive(true);
            RotatePointer();
        }

        private void TouchEnded()
        {
            // Hide pointer
            pointerContainer.SetActive(false);
            // Stay between `minPower` and `maxPower`, depending on that interpolant from before
            force = Mathf.Lerp(minPower, maxPower, interp);
            // Stall ball (otherwise, impossible to go up if falling fast, etc)
            rigidBody2D.velocity = Vector2.zero;
            // Shoot
            rigidBody2D.AddForce(offset.normalized * force, ForceMode2D.Impulse);

            // `infiniteShots` is never used in game, mostly for debugging
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
            Vector3 relativePointerPos = new Vector3(Mathf.Lerp(.5f, 1f, interp), 0, 0);
            Vector3 relativePointerScale = new Vector3(Mathf.Lerp(.18f, .4f, interp), Mathf.Lerp(.18f, .4f, interp), 0);
            pointer.transform.localScale = relativePointerScale;
            pointer.transform.localPosition = relativePointerPos;
        }

        // NOTE: Add `true` for "No more shots!"
        private void ShotsLabelUpd(bool zero = false)
        {
            shotsLabel.text = zero ? "No more shots!" : shots.ToString();
        }
    }
}

/// Legacy Update. Don't know why I'm keeping it here, it's pretty useless.
//private void Update()
//{
//    if (Input.touchCount != 1)
//    {
//        dragging = false;
//        return;
//    }
//
//    Touch touch = Input.touches[0];
//
//    if (touch.phase == TouchPhase.Began)
//        startPos = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
//
//    if (touch.phase == TouchPhase.Moved)
//    {
//        offset = new Vector2(Input.mousePosition.x - startPos.x, Input.mousePosition.y - startPos.y);
//        Debug.Log(offset);
//    }
//
//    if (touch.phase == TouchPhase.Ended || touch.phase == TouchPhase.Canceled)
//    {
//        dragging = false;
//        rb.AddForce(offset, ForceMode2D.Impulse);
//    }
//}