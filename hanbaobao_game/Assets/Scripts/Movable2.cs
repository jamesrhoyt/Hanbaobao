/*
 * Movable2.cs
 * 
 * Handles the basic movement functionality for every moving object found in the levels
 * (Namely the Player, the Enemies (including the walls), the Items, and all of the Bosses).
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movable2 : MonoBehaviour
{
    private float speed;            //The speed of the Object, measured in Units per Frame.
    private Vector3 target;         //The target position of the Object (optional).
    private float angle;            //The angle (in radians) at which the Object moves (either to reach "target" or to travel in a predetermined direction).
    private Vector3 velocity;       //The X- and Y-distance the Object will move every Frame; made by combining "speed" and "angle".
    public bool moveWithBackground; //Whether this Object moves at the same rate as the main Background layer, to simulate it not moving.

    //Remember to turn off the Gravity Scale for each Rigidbody.
    private Rigidbody2D rb2D;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        rb2D = GetComponent<Rigidbody2D>();
        rb2D.gravityScale = 0;
    }

    //Get the speed of the Object.
    public float GetSpeed()
    {
        return speed;
    }

    //Set the speed of the Object.
    public void SetSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    //Return the angle of the Object's direction in radians.
    public float GetAngleInRadians()
    {
        return angle;
    }

    //Return the angle of the Object's direction in degrees.
    public float GetAngleInDegrees()
    {
        return angle * Mathf.Rad2Deg;
    }

    //Set the angle of the Object, independent of a target, using a value in radians.
    public void SetAngleInRadians(float newAngle)
    {
        angle = newAngle;
    }

    //Set the angle of the Object, independent of a target, using a value in degrees.
    public void SetAngleInDegrees(float newAngle)
    {
        angle = newAngle * Mathf.Deg2Rad;
    }

    //Return "target" as a Vector3.
    public Vector3 GetTarget()
    {
        return target;
    }

    //Set the target of the Object using a Vector2.
    public void SetTarget(Vector2 newTarget)
    {
        target.Set(newTarget.x, newTarget.y, transform.position.z);
        SetAngleInRadians(Mathf.Atan2(target.y - transform.position.y, target.x - transform.position.x));
    }

    //Set the target of the Object using a Vector3.
    public void SetTarget(Vector3 newTarget)
    {
        target.Set(newTarget.x, newTarget.y, transform.position.z);
        SetAngleInRadians(Mathf.Atan2(target.y - transform.position.y, target.x - transform.position.x));
    }

    //Set the target of the Object relative to its Parent using a Vector2.
    public void SetTargetLocal(Vector2 newTarget)
    {
        target.Set(newTarget.x, newTarget.y, transform.localPosition.z);
        SetAngleInRadians(Mathf.Atan2(target.y - transform.localPosition.y, target.x - transform.localPosition.x));
    }

    //Set the target of the Object relative to its Parent using a Vector3.
    public void SetTargetLocal(Vector3 newTarget)
    {
        target.Set(newTarget.x, newTarget.y, transform.localPosition.z);
        SetAngleInRadians(Mathf.Atan2(target.y - transform.localPosition.y, target.x - transform.localPosition.x));
    }

    //Move the Object with the Background, if necessary.
    private void MoveWithBackground()
    {
        transform.Translate(BGManager.instance.scrollValues[0] + BGManager.instance.scrollOffsets[0], 0, 0, Space.World);
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //Only update the Object's position if the game isn't paused.
        if (!LevelManager.instance.gamePaused)
        {
            if (moveWithBackground) MoveWithBackground();
            velocity.Set(Mathf.Cos(angle) * speed, Mathf.Sin(angle) * speed, 0);
            transform.Translate(velocity, Space.World);
        }
    }
}
