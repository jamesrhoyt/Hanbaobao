using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lightning : Bullet
{
    private enum Direction { UP, ANY, DOWN };   //The 3 possible directions that this arc can look for targets.
    private Direction direction;        //Which direction(s) this arc can look for targets.
    private Vector3 startPoint;         //The beginning point for the Lightning arc.
    private Vector3 endPoint;           //The ending point for the Lightning arc.
    private float distance;             //The distance between startPoint and endPoint.
    private GameObject targetObject;    //The target used to set "endPoint" when the Lightning is first fired.
    private bool hasHadTarget;          //Whether this Lightning arc has had a "target" object or not.
    public GameObject segmentTemplate;  //The GameObject that "segments" will be filled with copies of.
    private GameObject[] segments;      //The array of segments that will make up the Lightning arc.
    private int numSegments;            //The current number of segments (used to update "segments" without creating a new array).
    private float maxSegmentLength;     //The maximum length an individual segment can be.

	// Use this for initialization
	protected override void Start()
    {
        maxSegmentLength = .384f;
        hasHadTarget = false;
        segments = new GameObject[10];
        //Call Bullet's Start.
        base.Start();
	}
	
	// Update is called once per frame
	protected override void Update()
    {
        //If this Lightning arc has not had a target, search for one.
        if (!hasHadTarget)
        {
            targetObject = LevelManager.instance.FindClosestEnemyToPlayer(GetDirection());
        }
        //If this Lightning arc has a target, disable the flag that says it doesn't.
        if (targetObject != null && hasHadTarget == false)
        {
            hasHadTarget = true;
        }
        UpdateSegments();
        //Call Bullet's Update.
        base.Update();
	}

    //Set the Direction value for this Lightning arc.
    public void SetDirection(int dir)
    {
        switch (dir)
        {
            case 1:
                direction = Direction.UP;
                break;
            case 0:
                direction = Direction.ANY;
                break;
            case -1:
                direction = Direction.DOWN;
                break;
        }
    }

    //Get the Direction value for this Lightning arc.
    public string GetDirection()
    {
        if (direction == Direction.UP) { return "UP"; }
        else if (direction == Direction.ANY) { return "ANY"; }
        else if (direction == Direction.DOWN) { return "DOWN"; }
        //All code paths need to return a value, so this "else" is added as a (likely unreachable) default.
        else { return "ANY"; }
    }

    //Update the positions, rotations, and scales of each object in this Lightning arc.
    private void UpdateSegments()
    {
        //Get the start and end points for the Arc.
        startPoint = LevelManager.instance.player.transform.position;
        if (targetObject != null)
        {
            endPoint = targetObject.transform.position;
        }
        //If there is no target, set the end point based on the Arc's direction.
        else
        {
            switch (direction)
            {
                case Direction.UP:
                    endPoint = startPoint + new Vector3(3.0f, 0.8f);
                    break;
                case Direction.ANY:
                    endPoint = startPoint + new Vector3(3.0f, 0f);
                    break;
                case Direction.DOWN:
                    endPoint = startPoint + new Vector3(3.0f, -0.8f);
                    break;
            }
        }
        //Get the distance between the start and end points.
        distance = Vector2.Distance(startPoint, endPoint);
        //Get the number of segments this Arc must currently consist of.
        numSegments = Mathf.CeilToInt(distance / maxSegmentLength);
        //Create or update each of the segments.
        for (int i = 0; i < numSegments; i++)
        {
            //If there is no object at this index, create one.
            if (segments[i] == null)
            {
                segments[i] = Instantiate(segmentTemplate, transform.position, Quaternion.identity);
                LevelManager.instance.AddBulletToList(segments[i]);
            }
            //Set the position and scale of this segment.
            segments[i].transform.position = Vector3.Lerp(startPoint, endPoint, (float)(1 + (2 * i)) / (numSegments * 2));
            //segments[i].transform.lossyScale.Set(((Vector2.Distance(startPoint, endPoint) / (float)numSegments) / .384f) * .44f, .4f, 1f);
            //Rotate each segment to point towards the target.
            segments[i].transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(startPoint.y - endPoint.y, startPoint.x - endPoint.x) * Mathf.Rad2Deg);
        }
        //If there are existing segments that are not needed this frame, destroy them.
        for (int j = numSegments; j < segments.Length; j++)
        {
            if (segments[j] != null)
            {
                GameObject.Destroy(segments[j]);
            }
        }
        //If the back half of the first Lightning Segment is sticking out of the other side of the Player ship, move it further down the Arc.
        if (Vector2.Distance(startPoint, segments[0].transform.position) < (segments[0].GetComponent<Collider2D>().bounds.size.x / 2.0f))
        {
            segments[0].transform.position = Vector3.Lerp(startPoint, endPoint, (segments[0].GetComponent<Collider2D>().bounds.size.x / 2.0f) / distance);
        }
        //Position and rotate the end segment of the Arc.
        transform.position = endPoint + new Vector3(0, 0, -0.1f);
        //transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(startPoint.y - endPoint.y, startPoint.x - endPoint.x) * Mathf.Rad2Deg);
    }
}
