/*
 * FoldWallR.cs
 * 
 * A rotating version of the Fold Wall that extends in a straight line 
 * in both directions, while rotating.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoldWallR : EnemyController
{

    public GameObject foldWallSegment;      //The template for the gold segments that extend the Fold Wall.
    public GameObject redSegmentTemplate;   //The template to create the two red segments off of.
    public GameObject redSegmentA;          //The 1st lead segment of the Fold Wall.
    private Vector3 leftPivot;              //The point to rotate the 1st lead segment around.
    public GameObject redSegmentB;          //The 2nd lead segment of the Fold Wall.
    private Vector3 rightPivot;             //The point to rotate the 2nd lead segment around.
    private float goldRotationDirection;    //The direction to rotate the entire Fold Wall assembly.
    private List<GameObject> wallSegments;  //Every segment in this instance of the Type-R Fold Wall.

	// Use this for initialization
	protected override void Start()
    {
        //Call EnemyController's Start.
        base.Start();
        //Overwrite the EnemyController default values.
        hp = 1;
        SetSpeed(0f);
        scoreValue = 0;
        //Set the Fold Wall's spin direction.
        goldRotationDirection = transform.lossyScale.x * (1 / Mathf.Abs(transform.lossyScale.x));
        //Instantiate the Segments list.
        wallSegments = new List<GameObject>();
        //Instantiate the two head segments.
        redSegmentA = Instantiate(redSegmentTemplate, transform.position - new Vector3(0,0,.1f), transform.rotation) as GameObject;
        redSegmentB = Instantiate(redSegmentTemplate, transform.position - new Vector3(0,0,.2f), transform.rotation) as GameObject;
        //Set the pivot points' starting positions.
        leftPivot = new Vector3(transform.position.x, transform.position.y + hitbox.bounds.extents.y, 2f);
        rightPivot = new Vector3(transform.position.x, transform.position.y - hitbox.bounds.extents.y, 2f);
        //Add the 3 starting segments to the list.
        wallSegments.Add(gameObject);
        wallSegments.Add(redSegmentA);
        wallSegments.Add(redSegmentB);
	}

    //Activate the Fold Wall's behaviors when it appears on screen.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.AddEnemyToList(gameObject);
            LevelManager.instance.AddEnemyToList(redSegmentA);
            LevelManager.instance.AddEnemyToList(redSegmentB);
            //Have the Fold Wall start rotating.
            StartCoroutine(RotateAllSegments());
            //Have each of the Red Segments start rotating separately.
            StartCoroutine(RotateRedSegment(redSegmentA, leftPivot, goldRotationDirection));
            StartCoroutine(RotateRedSegment(redSegmentB, rightPivot, goldRotationDirection));
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

    //Rotate all of the segments in the Fold Wall.
    IEnumerator RotateAllSegments()
    {
        while (hp > 0)
        {
            foreach (GameObject g in wallSegments)
            {
                g.transform.RotateAround(transform.position, Vector3.forward, 7.5f * goldRotationDirection);
            }
            leftPivot = new Vector3(redSegmentA.transform.position.x + Mathf.Cos((transform.eulerAngles.z % 360) * Mathf.Deg2Rad),
                (redSegmentA.transform.position.y + hitbox.bounds.extents.y) + Mathf.Sin((transform.eulerAngles.z % 360) * Mathf.Deg2Rad),
                2f);
            rightPivot = new Vector3(redSegmentB.transform.position.x + Mathf.Cos((transform.eulerAngles.z % 360) * Mathf.Deg2Rad),
                (redSegmentB.transform.position.y - hitbox.bounds.extents.y) + Mathf.Sin((transform.eulerAngles.z % 360) * Mathf.Deg2Rad),
                2f);
            transform.RotateAround(transform.position, Vector3.forward, 7.5f);
            yield return new WaitForSeconds(Time.deltaTime * 4);
        }
    }

    /// <summary>
    /// Rotate the two red segments around their pivot points.
    /// </summary>
    /// <param name="segment">The GameObject (red segment) to rotate.</param>
    /// <param name="pivot">The point "segment" rotates around.</param>
    /// <param name="verticalDirection">Determines whether "pivot"'s base y-value increases or decreases.</param>
    /// <returns></returns>
    IEnumerator RotateRedSegment(GameObject segment, Vector3 pivot, float verticalDirection)
    {
        //Track how much this red segment has rotated since placing a new gold segment.
        float rotationAmount = 0;
        //Track the direction to rotate the red segment in.
        float redRotationDirection = -1; //Make this -1 to start, because the while loop will flip it immediately.
        //Run this loop while the red segment is still alive.
        while (segment.GetComponent<EnemyController>().hp > 0)
        {
            //Flip the red segment's rotation direction.
            redRotationDirection *= -1;
            //Update the pivot point.
            pivot = new Vector3(segment.transform.position.x + Mathf.Cos((segment.transform.eulerAngles.z % 360) * Mathf.Deg2Rad), 
                segment.transform.position.y + (hitbox.bounds.extents.y * verticalDirection) + Mathf.Sin((segment.transform.eulerAngles.z % 360) * Mathf.Deg2Rad), 
                2f);
            //Reset the rotation counter.
            rotationAmount = 0;
            //Keep rotating until the segment has turned 180 degrees around its pivot.
            while (rotationAmount < 180f)
            {
                //Only rotate the segment if the game is not paused.
                if (!LevelManager.instance.gamePaused)
                {
                    //Rotate the red segment.
                    segment.transform.RotateAround(pivot, Vector3.forward, 22.5f * redRotationDirection * verticalDirection);
                    //Tell the red segment to move to its own position (it will get stuck in place otherwise).
                    //smoothMove(segment.transform.position.x, segment.transform.position.y, speed);
                    SetTarget(segment.transform.position);
                    //Increase the rotation counter.
                    rotationAmount += 22.5f;
                }
                //Pause until the next frame.
                yield return new WaitForSeconds(Time.deltaTime * 4);
            }
            //Create a new "gold" segment.
            GameObject newSegment = Instantiate(foldWallSegment, segment.transform.position + Vector3.forward, segment.transform.rotation) as GameObject;
            //Add the new segment to the list of segments.
            wallSegments.Add(newSegment);
            //Increase the red segment's point value.
            segment.GetComponent<EnemyController>().scoreValue += 10;
        }
    }

	// Update is called once per frame
	protected override void Update()
    {
        base.Update();
	}
}
