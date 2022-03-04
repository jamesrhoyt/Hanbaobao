/*
 * SnakeBody.cs
 * 
 * Manages the movement of each part of the Snake Enemy's Body,
 * as dictated by the movement behaviors in its head ("Snake.cs").
 * 
 */

using UnityEngine;

public class SnakeBody : EnemyController
{
    private Vector3 currentTarget;  //The current target for this Snake segment.
    private Vector3 nextTarget;     //The next target in line for this Snake segment, after "currentTarget" has been reached.
    private int movementCycles;     //The number of elapsed movement cycles.

	// Use this for initialization
	protected override void Start()
    {
        //Call EnemyController's Start.
        base.Start();
        //Overwrite the EnemyController default values.
        hp = 8;
        scoreValue = 0;
        SetSpeed(0f);
        //Initialize the movement target Vectors.
        currentTarget = Vector3.zero;
        nextTarget = Vector3.zero;
        movementCycles = 0;
	}

    //Despawn the SnakeBody when it passes outside of the screen.
    void OnTriggerExit2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            //Remove the Body from LevelManager's list of Enemies.
            LevelManager.instance.RemoveEnemyFromList(gameObject);
        }
    }

    //Add the next target position for the Body sphere to move towards.
    public void UpdateTarget(Vector3 newTarget)
    {
        //If this is the 1st target setting, set the current target to this position.
        if (movementCycles == 0)
        {
            currentTarget = newTarget;
            SetTarget(currentTarget);
        }
        //Otherwise, set it as the next target.
        else
        {
            nextTarget = newTarget;
        }
        movementCycles++;
    }

	// Update is called once per frame
	protected override void Update()
    {
        //Only update the Snake Body's movement targets if the Game isn't paused.
        if (!LevelManager.instance.gamePaused)
        {
            //Move the targets along with the background.
            currentTarget.Set(currentTarget.x + (BGManager.instance.scrollValues[0] + BGManager.instance.scrollOffsets[0]), currentTarget.y, currentTarget.z);
            nextTarget.Set(nextTarget.x + (BGManager.instance.scrollValues[0] + BGManager.instance.scrollOffsets[0]), nextTarget.y, nextTarget.z);
            //If the Body sphere has reached its last target position, make it start moving toward the next one.
            if (Vector2.Distance(transform.position, currentTarget) <= .5)
            {
                //Debug.Log("Current Target: X: " + currentTarget.x + ", Y: " + currentTarget.y);
                currentTarget = nextTarget;
                //Debug.Log("Next Target: X: " + nextTarget.x + ", Y: " + nextTarget.y);
                SetTarget(currentTarget);
            }
        }
        //Call EnemyController's Update.
        base.Update();
	}
}
