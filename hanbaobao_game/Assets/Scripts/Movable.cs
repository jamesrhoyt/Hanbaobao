/*
 * Movable.cs
 * 
 * Handles the basic movement functionality for every moving object found in the levels
 * (Namely the Player, the Camera, all of the Enemies (including the walls), and all of the Bosses).
 * 
 */

using UnityEngine;
using System.Collections;

public abstract class Movable : MonoBehaviour
{
    /*
    private Vector3 cameraSpeed = new Vector3(0.005f, 0, 0);
    private Vector3 endOfLevel = new Vector3(6, 0, 0);
    */
    protected Vector3 startMarker;  //The starting interpolation point for the Object's movement.
    protected Vector3 endMarker;    //The ending (target) interpolation point for the Object's movement.
    private float movementSpeed;    //The speed at which the Object interpolates between the two markers.
    private float journeyLength;    //The distance between the two markers.
    private float distCovered;      //The distance the Object has traveled from its startMarker.
    private float fracJourney;      //The percentage of distance the Object has covered between startMarker and endMarker.

    //Remember to turn off the Gravity Scale for each Rigidbody.
    private Rigidbody2D rb2D;

    // Use this for initialization
    protected virtual void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
		rb2D.gravityScale = 0;
        smoothMove(0, 0, 0); // Initialize
    }

    /// <summary>
    /// Set the Movable's new target position and speed.
    /// </summary>
    /// <param name="xDir">The x-component of the new Target position.</param>
    /// <param name="yDir">The y-component of the new Target position.</param>
    /// <param name="speed">The new speed at which the Movable will move.</param>
    protected void smoothMove(float xDir, float yDir, float speed)
    {
        //Start the Movable's movement interpolation at its current position.
        startMarker = transform.position;
        //Set the Movable's target Vector according to the new arguments.
        endMarker = new Vector3(xDir, yDir, transform.position.z);
        //Set the Movable's new speed according to the new argument.
        movementSpeed = speed;
        //Set the new journeyLength according to the new Marker Vectors.
        journeyLength = Vector3.Distance(startMarker, endMarker);
        //Reset the "Distance Covered" values.
        distCovered = 0;
        fracJourney = 0;
    }

    //Update is called once per frame
    protected virtual void Update()
    {
        //Only update the Object's position if the game isn't paused.
        if (!LevelManager.instance.gamePaused)
        {
            //Only move the Object if the Object's position and its target are not the *exact* same value.
            if (Vector3.Distance(startMarker, endMarker) > 0)
            {
                //Increase the distance between the Object's starting and current positions incrementally.
                distCovered += movementSpeed * Time.deltaTime;
                //Increase the percentage of distance covered to match the new value.
                fracJourney = distCovered / journeyLength;
                //Move the Object further along the line between its start and end Markers.
                rb2D.MovePosition(Vector3.LerpUnclamped(startMarker, endMarker, fracJourney));
            }
        }
    }
}
