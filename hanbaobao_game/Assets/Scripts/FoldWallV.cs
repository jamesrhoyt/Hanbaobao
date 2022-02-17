/*
 * FoldWallV.cs
 * 
 * A version of the Fold Wall that extends vertically.
 * The lead of the Fold Wall rotates around a pivot, places a Wall Segment,
 * then gets a new pivot and reverses its rotation direction.
 * The direction of the extension is determined by the transform's y-scale.
 * 
 */

using UnityEngine;
using System.Collections;

public class FoldWallV : EnemyController
{

    public GameObject foldWallSegment;  //The template for the gold segment of the Fold Wall.
    private float rotationAmount;       //How much the "lead" segment has rotated since placing a new "gold" segment.
    private Vector3 pivot;              //The point to rotate the "lead" segment around.
    private float rotationDirection;    //The direction to rotate the lead segment in (1 for clockwise, -1 for counterclockwise).
    private float verticalDirection;    //The direction in which the Wall will unfold vertically (1 for upward, -1 for downward).

	// Use this for initialization
	protected override void Start()
    {
        //Call EnemyController's Start.
        base.Start();
        //Overwrite the EnemyController default values.
        hp = 1;
        SetSpeed(0f);
        scoreValue = 0;
        //Reset the rotation tally and direction.
        rotationAmount = 0;
        rotationDirection = -1; //Make this -1 to start, because "RotateWallSegment" will flip it immediately.
        //Set the Fold Wall's travel direction.
        verticalDirection = transform.lossyScale.y * (1 / Mathf.Abs(transform.lossyScale.y));
	}

    //Activate the Fold Wall's behaviors when it appears on screen.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.AddEnemyToList(gameObject);
            //Have the Fold Wall start rotating.
            StartCoroutine(RotateLeadSegment());
        }
        //Otherwise, check if this is a Player-controlled Bullet. 
        else if (box.gameObject.CompareTag("PlayerBullet"))
        {
            if (!dmgImmune)
            {
                //Have the Fold Wall take damage.
                TakeDamage(box.gameObject.GetComponent<Bullet>().dmgValue);
            }
        }
    }

    //Despawn the Fold Wall when it passes outside of the screen.
    void OnTriggerExit2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.RemoveEnemyFromList(gameObject);
        }
    }

    //Rotate the Fold Wall's lead segment and place "body" segments behind it.
    IEnumerator RotateLeadSegment()
    {
        while (hp > 0)
        {
            //Create a new "gold" segment.
            Instantiate(foldWallSegment, transform.position + Vector3.forward, transform.rotation);
            //Increase the red segment's point value.
            scoreValue += 10;
            //Flip the red segment's rotation direction.
            rotationDirection *= -1;
            //Update the pivot point.
            pivot = new Vector3(transform.position.x, transform.position.y + (hitbox.bounds.extents.y * verticalDirection), 2f);
            //Reset the rotation counter.
            rotationAmount = 0;
            //Keep rotating until the red segment has turned 180 degrees.
            while (rotationAmount < 180f)
            {
                //Only rotate the segment if the game is not paused.
                if (!LevelManager.instance.gamePaused)
                {
                    //Rotate the red segment.
                    transform.RotateAround(pivot, Vector3.forward, 11.25f * rotationDirection * verticalDirection);
                    //Increase the rotation counter.
                    rotationAmount += 11.25f;
                }
                //Pause until the next frame.
                yield return new WaitForSeconds(Time.deltaTime * 7.5f);
            }
        }
    }

	// Update is called once per frame
	protected override void Update()
    {
        //If the game isn't paused, move the "pivot" point along with the Background.
        if(!LevelManager.instance.gamePaused)
        {
            pivot.Set(pivot.x + (BGManager.instance.scrollValues[0] + BGManager.instance.scrollOffsets[0]), pivot.y, pivot.z);
        }
        //Call EnemyController's Update.
        base.Update();
	}
}
