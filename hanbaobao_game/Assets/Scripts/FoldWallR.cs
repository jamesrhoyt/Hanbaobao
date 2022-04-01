/*
 * FoldWallR.cs
 * 
 * A rotating version of the Fold Wall that extends in a straight line 
 * in both directions, while rotating.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class FoldWallR : EnemyController
{

    public GameObject goldSegmentTemplate;  //The template for the gold segments that extend the Fold Wall.
    public GameObject redSegmentUpper;      //The upper lead segment of the Fold Wall.
    private Vector3 pivotUpper;             //The point to rotate the upper lead segment around.
    private List<GameObject> segmentsUpper; //Every gold segment in the upper half of the Fold Wall.
    public GameObject redSegmentLower;      //The lower lead segment of the Fold Wall.
    private Vector3 pivotLower;             //The point to rotate the lower lead segment around.
    private List<GameObject> segmentsLower; //Every gold segment in the lower half of the Fold Wall.
    private float goldSegmentRadius;        //The y-extents of the hitbox when it is perfectly straight.
                                            //(Used in calculating the "pivot" points.)
    private float goldRotationDirection;    //The direction to rotate the entire Fold Wall assembly.
    private float redRotationDirection;     //The direction to rotate the Upper Red Segment.
                                            //(The direction to rotate the Lower Red Segment will always be the inverse of this.)
    private float rotationAmountStep;   //How much to rotate the assembly every step of its movement.
    private float redRotationStep;      //How much to rotate the "red" segments around their pivots every step.
    private float redRotationTotal;     //How much each "red" segment has rotated since placing a new "gold" segment.
    private float redRotationLimit;     //How much to rotate each "red" segment around their pivots before changing their rotation direction.
    private float rotationDelayTimer;   //How much time has elapsed since the last rotation step.
    private float rotationDelayDuration;//How much time to wait between rotation steps.

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
        redRotationDirection = -goldRotationDirection;
        //Set the flag to not despawn the segment template when it leaves the screen.
        //(This should set it for all of its copies.)
        goldSegmentTemplate.GetComponent<FoldWallSegment>().destroyOnExit = false;
        //Instantiate the two head segments.
        redSegmentUpper = Instantiate(redSegmentUpper, transform.position - Vector3.forward, transform.rotation);
        redSegmentLower = Instantiate(redSegmentLower, transform.position - Vector3.forward, transform.rotation);
        //Set the flag to not despawn the head segments when they leave the screen.
        redSegmentUpper.GetComponent<FoldWallSegment>().destroyOnExit = false;
        redSegmentLower.GetComponent<FoldWallSegment>().destroyOnExit = false;
        //Instantiate the Segments lists.
        segmentsUpper = new List<GameObject>();
        segmentsLower = new List<GameObject>();
        //Instantiate the first object in each of the Segments lists.
        segmentsUpper.Add(Instantiate(goldSegmentTemplate, redSegmentUpper.transform.position, redSegmentUpper.transform.rotation));
        segmentsLower.Add(Instantiate(goldSegmentTemplate, redSegmentLower.transform.position, redSegmentLower.transform.rotation));
        //Set the flags to not despawn the gold segments when they leave the screen.
        //("WaitToDespawn" will handle this.)
        segmentsUpper.Last().GetComponent<FoldWallSegment>().destroyOnExit = false;
        segmentsLower.Last().GetComponent<FoldWallSegment>().destroyOnExit = false;
        //Set the pivot points' starting positions.
        goldSegmentRadius = hitbox.bounds.extents.y;
        pivotUpper = new Vector3(transform.position.x, transform.position.y + hitbox.bounds.extents.y, transform.position.z);
        pivotLower = new Vector3(transform.position.x, transform.position.y - hitbox.bounds.extents.y, transform.position.z);
        //Initialize the rotation values.
        rotationAmountStep = 2.8125f;
        rotationDelayTimer = 0;
        rotationDelayDuration = Time.deltaTime * 15f;
        redRotationStep = 5.625f;
        redRotationTotal = 0;
        redRotationLimit = 180f;
	}

    //Activate the Fold Wall's behaviors when it appears on screen.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.AddEnemyToList(gameObject);
            //Have the Fold Wall start rotating.
            StartCoroutine(RotationLogic());
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
            //LevelManager.instance.RemoveEnemyFromList(gameObject);
            StartCoroutine(WaitToDespawn(box.bounds.min.x));
        }
    }

    //Rotate all of the segments in the Fold Wall.
    IEnumerator RotationLogic()
    {
        while (hp > 0)
        {
            transform.Rotate(Vector3.forward, rotationAmountStep * goldRotationDirection);
            //Make the entire Fold Wall assembly appear to be rotating around the base segment.
            //Rotate and revolve all of the Upper Segments.
            foreach (GameObject g in segmentsUpper)
            {
                //Revolve all of the gold segments around the base one.
                g.transform.RotateAround(transform.position, Vector3.forward, rotationAmountStep * goldRotationDirection);
            }
            //Rotate and revolve all of the Lower Segments.
            foreach (GameObject g in segmentsLower)
            {
                //Revolve all of the gold segments around the base one.
                g.transform.RotateAround(transform.position, Vector3.forward, rotationAmountStep * goldRotationDirection);
            }
            //Update the Upper Pivot Point.
            pivotUpper.Set(segmentsUpper.Last().transform.position.x +
                (Mathf.Cos((transform.eulerAngles.z + 90f) * Mathf.Deg2Rad) * goldSegmentRadius),
                segmentsUpper.Last().transform.position.y +
                (Mathf.Sin((transform.eulerAngles.z + 90f) * Mathf.Deg2Rad) * goldSegmentRadius),
                transform.position.z);
            //Update the Lower Pivot Point.
            pivotLower.Set(segmentsLower.Last().transform.position.x +
                (Mathf.Cos((transform.eulerAngles.z + 270f) * Mathf.Deg2Rad) * goldSegmentRadius),
                segmentsLower.Last().transform.position.y +
                (Mathf.Sin((transform.eulerAngles.z + 270f) * Mathf.Deg2Rad) * goldSegmentRadius),
                transform.position.z);
            //Rotate the upper red segment around the base segment, and around its pivot.
            try
            {
                redSegmentUpper.transform.RotateAround(transform.position, Vector3.forward, rotationAmountStep * goldRotationDirection);
                redSegmentUpper.transform.RotateAround(pivotUpper, Vector3.forward, redRotationStep * redRotationDirection);
            }
            catch (MissingReferenceException) { }
            //Rotate the lower red segment around the base segment, and around its pivot.
            try
            {
                redSegmentLower.transform.RotateAround(transform.position, Vector3.forward, rotationAmountStep * goldRotationDirection);
                redSegmentLower.transform.RotateAround(pivotLower, Vector3.forward, redRotationStep * -redRotationDirection);
            }
            catch (MissingReferenceException) { }
            //Increase "redRotationTotal" by "redRotationStep".
            redRotationTotal += redRotationStep;
            //If the red segments have rotated 180 degrees around their pivots,
            //create a new gold segment in their place and reverse their pivoting directions.
            if (redRotationTotal >= redRotationLimit)
            {
                //Only do this if the Upper Red Segment hasn't been destroyed.
                try
                {
                    segmentsUpper.Add(Instantiate(goldSegmentTemplate, redSegmentUpper.transform.position + Vector3.forward, redSegmentUpper.transform.rotation));
                    redSegmentUpper.GetComponent<EnemyController>().scoreValue += 10;
                    segmentsUpper.Last().GetComponent<FoldWallSegment>().destroyOnExit = false;
                }
                catch (MissingReferenceException) { }
                //Only do this if the Lower Red Segment hasn't been destroyed.
                try
                {
                    segmentsLower.Add(Instantiate(goldSegmentTemplate, redSegmentLower.transform.position + Vector3.forward, redSegmentLower.transform.rotation));
                    redSegmentLower.GetComponent<EnemyController>().scoreValue += 10;
                    segmentsLower.Last().GetComponent<FoldWallSegment>().destroyOnExit = false;
                }
                catch (MissingReferenceException) { }
                //Update their pivots to the last segment in each list.
                pivotUpper.Set(segmentsUpper.Last().transform.position.x + 
                    (Mathf.Cos((transform.eulerAngles.z + 90f) * Mathf.Deg2Rad) * goldSegmentRadius), 
                    segmentsUpper.Last().transform.position.y + 
                    (Mathf.Sin((transform.eulerAngles.z + 90f) * Mathf.Deg2Rad) * goldSegmentRadius),
                    transform.position.z);
                pivotLower.Set(segmentsLower.Last().transform.position.x +
                    (Mathf.Cos((transform.eulerAngles.z + 270f) * Mathf.Deg2Rad) * goldSegmentRadius),
                    segmentsLower.Last().transform.position.y +
                    (Mathf.Sin((transform.eulerAngles.z + 270f) * Mathf.Deg2Rad) * goldSegmentRadius),
                    transform.position.z);
                //Flip their rotation directions.
                redRotationDirection *= -1;
                //Reset the "red" rotation count.
                redRotationTotal = 0;
            }

            //Wait 1/4th of a second before incrementing the rotation.
            //Reset the Delay Timer.
            rotationDelayTimer = 0;
            //Run while the timer is less than the allotted duration.
            while (rotationDelayTimer < rotationDelayDuration)
            {
                //If the Game is paused, don't update the Delay Timer.
                if (!LevelManager.instance.gamePaused)
                {
                    rotationDelayTimer += Time.deltaTime;
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
    }

    //Wait until none of the Fold Wall's Segments can possibly reenter the screen, then despawn all of them.
    IEnumerator WaitToDespawn(float minX)
    {
        yield return new WaitUntil(() => transform.position.x + (goldSegmentRadius * 2 * Mathf.Max(segmentsUpper.Count(), segmentsLower.Count())) < minX);
        //Despawn the two red segments (if they haven't died already).
        try
        {
            LevelManager.instance.RemoveEnemyFromList(redSegmentUpper);
        }
        catch (MissingReferenceException) { }
        try
        {
            LevelManager.instance.RemoveEnemyFromList(redSegmentLower);
        }
        catch (MissingReferenceException) { }
        //Despawn all of the created gold segments.
        foreach (GameObject g in segmentsUpper)
        {
            LevelManager.instance.RemoveEnemyFromList(g);
        }
        foreach (GameObject g in segmentsLower)
        {
            LevelManager.instance.RemoveEnemyFromList(g);
        }
        //Despawn the base segment (this object).
        LevelManager.instance.RemoveEnemyFromList(gameObject);
    }

	// Update is called once per frame
	protected override void Update()
    {
        base.Update();
	}
}
