/*
 * HydraHead.cs
 * 
 * The underlying logic for each individual "head" of the Hydra
 * Miniboss, this script holds functions that will be called by
 * Hydra.cs.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HydraHead : EnemyController
{
    public GameObject hydraBullet;  //The "Master Copy" of the Bullet that the Hydra Head will fire.
    public GameObject hydraNeckTemplate;    //The GameObject that "hydraSegment" will be filled with copies of.
    public GameObject[] hydraSegment;   //The 5 objects that make up the head and neck of each HydraHead instance.
    private Vector2 anchor; //The "anchor" point of the Head (the position of the Neck Object attached to the Hydra Body).

    private float diameter;     //The diameter for each object in the Hydra Head.
    private float headLength;   //The length of a Hydra Head, from the center of its head to the center of its last "neck" object.
    private float movementAngle;//The new angle (in degrees) that determines the Hydra Head's new target.
    private float xDist;        //The magnitude (absolute value) of the horizontal distance between Head base and target.
    private float yDist;        //The magnitude (absolute value) of the vertical distance between Head base and target.
    private float cooldownTimer;//The timer for each of the pauses between the Hydra Head's behavior states.
    //private Vector3 target;     //The new target position for the Hydra Head, relative to the parent "Hydra" object.

    private float distance;         //The distance between the Hydra's head and its new target per frame.
    private float maxDistance;      //The distance between the Hydra Head's last target position and its current target position.
    private float distPercentage;   //The remaining percentage of distance ("distance" / "maxDistance").
    private float maxSpeed;         //The Head's initial speed when it starts a new movement cycle.
    private float minSpeed;         //The Head's minimum speed during its movement cycle.
    private Vector3 step;           //The amount that each Neck object will move this frame (same as "velocity").

    //"Flash" Variables:
    private SpriteRenderer hydraRenderer;   //The Sprite Renderer attached to this GameObject.
    private SpriteRenderer noseRenderer;    //The Sprite Renderer attached to the Head's "hydraNose" child Object.
    private Shader shaderGUIText;           //A Text Shader, used to turn the Hydra Head solid white, even during its Animations.
    private Shader shaderSpritesDefault;    //The GameObject's default Shader, used to restore it to its original color.
    private float damageTimer;              //The amount of time that the Hydra Head has "flashed" invincible.

	// Use this for initialization
	protected override void Start()
    {
        //Call EnemyController's Start.
        base.Start();
        //Override EnemyController's default values.
        hp = 100;
        scoreValue = 1000;
        SetSpeed(0);
        maxSpeed = .3f;
        minSpeed = .01f;
        rateOfFire = 4f;
        diameter = GetComponent<CircleCollider2D>().radius * 2f * transform.localScale.x/*transform.lossyScale.x*/;
        headLength = diameter * hydraSegment.Length;
        //Initialize the rest of each head's "Neck" array.
        for (int i = 0; i < hydraSegment.Length; i++)
        {
            //Put each Body object behind the last one on the z-axis, to prevent them from overlapping incorrectly.
            hydraSegment[i] = Instantiate(hydraNeckTemplate, transform.position + new Vector3(0, 0, (i + 1) * .02f), Quaternion.identity);
            //Make each part of the Hydra's neck immune to damage; only the head can be damaged directly.
            hydraSegment[i].GetComponent<EnemyController>().dmgImmune = true;
            //Make each part of the Neck a child of the main Hydra object.
            hydraSegment[i].transform.SetParent(transform.parent, true);
        }
        anchor = hydraSegment[hydraSegment.Length - 1].transform.localPosition;

        //Initialize the objects used for the Hydra head's "Hit Flash" effect.
        hydraRenderer = gameObject.GetComponent<SpriteRenderer>();
        noseRenderer = gameObject.GetComponentsInChildren<SpriteRenderer>()[1];
        shaderGUIText = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");
	}

    //Create a new target position for the head via a random function.
    private Vector2 GetNewTarget(int seed)
    {
        Random.InitState(seed);
        movementAngle = Random.Range(0, 15) * 22.5f;
        //Set the two pythagorean components of the Head's new movement target (the hypotenuse should always be as long as the Head itself).
        xDist = Mathf.Cos(movementAngle * Mathf.Deg2Rad) * headLength;
        yDist = Mathf.Sin(movementAngle * Mathf.Deg2Rad) * headLength;
        return new Vector2(anchor.x + xDist, anchor.y + yDist);
    }

    //Have the Head fire a shot before picking a new position for it to move to, and sending it toward that positon.
    public IEnumerator MovementCycle(int index)
    {
        //Yield out of this Coroutine to let the others start.
        yield return new WaitForEndOfFrame();

        //BULLET FIRING:
        //Create an instance of the Bullet that will appear behind the Hydra's head (on the z-axis).
        GameObject bullet = Instantiate(hydraBullet, transform.position + new Vector3(-2f, 0f, 1f), Quaternion.identity);
        //Add the Bullet to the LevelManager's list.
        LevelManager.instance.AddBulletToList(bullet);
        //Set the laser to travel straight left.
        bullet.GetComponent<Bullet>().SetAngleInDegrees(180f);
        bullet.GetComponent<Bullet>().SetSpeed(.8f);

        //QUARTER-SECOND WAIT:
        //Reset the Cooldown Timer.
        cooldownTimer = 0;
        //Wait another quarter-second before starting the next movement cycle.
        while (cooldownTimer < .25f)
        {
            //If the Game is paused, don't update the Cooldown Timer.
            if (!LevelManager.instance.gamePaused)
            {
                cooldownTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }

        //START NEW MOVEMENT:
        //Get the Head's new target position.
        SetTargetLocal(GetNewTarget(Time.frameCount * (index + 1)));
        //SetTargetLocal(GetNewTarget(LevelManager.instance.shotsFired));
        //Set the movement angle for all the Neck objects to the Head's new angle.
        foreach (GameObject g in hydraSegment)
        {
            g.GetComponent<HydraNeck>().SetAngleInDegrees(GetAngleInDegrees());
        }
        //Determine the distance between the Head's current position and its next target.
        maxDistance = Vector2.Distance(transform.localPosition, GetTarget());
        //Reset the head's speed.
        SetSpeed(maxSpeed);
        
        //Run this cycle until the Head has reached its target (or is at least close enough).
        while (Vector2.Distance(transform.localPosition, GetTarget()) >= 2)
        {
            //Check the remaining distance between the Hydra head and its new target.
            distance = Vector2.Distance(transform.localPosition, GetTarget());
            //Calculate the percentage of distance remaining.
            distPercentage = distance / maxDistance;
            //Set the head's speed for this frame.
            SetSpeed(/*Mathf.Max(*/maxSpeed * distPercentage/*, minSpeed)*/);
            //Set the distance that each moving object will move this frame.
            step.Set(Mathf.Cos(GetAngleInRadians()) * GetSpeed(), Mathf.Sin(GetAngleInRadians()) * GetSpeed(), 0);

            //Check the distances between each adjacent part of the Head and Neck, to keep them contiguous.
            for (int i = 0; i < hydraSegment.Length - 1; i++)
            {
                //Check the distance between the Head and 1st Neck sphere.
                if (i == 0)
                {
                    //Check if the 1st Neck sphere is too far away from the Head,
                    //and that moving won't put the 1st Neck sphere too far from the anchor.
                    if (Vector2.Distance(transform.localPosition, hydraSegment[0].transform.localPosition) >= diameter
                    && (Vector2.Distance(hydraSegment[0].transform.localPosition + step, anchor) <= headLength * ((float)(hydraSegment.Length - (i + 1)) / (float)hydraSegment.Length)))
                    {
                        //Update the speed of this Neck sphere, provided its link in the chain hasn't reached its limit.
                        hydraSegment[i].GetComponent<HydraNeck>().SetSpeed(GetSpeed());
                    }
                    //Otherwise, stop this Neck sphere.
                    else
                    {
                        hydraSegment[i].GetComponent<HydraNeck>().SetSpeed(0);
                    }
                }
                //Check the distance between the rest of the adjacent Neck spheres (except for the last one).
                else
                {
                    //Check if this Neck sphere is too far away from the one in front of it,
                    //and that moving won't put this Neck sphere too far from the anchor.
                    if (Vector2.Distance(hydraSegment[i].transform.localPosition, hydraSegment[i - 1].transform.localPosition) >= diameter
                    && (Vector2.Distance(hydraSegment[i].transform.localPosition + step, anchor) <= headLength * ((float)(hydraSegment.Length - (i + 1)) / (float)hydraSegment.Length)))
                    {
                        //Update the speed of each Neck sphere over time, provided its link in the chain hasn't reached its limit.
                        hydraSegment[i].GetComponent<HydraNeck>().SetSpeed(GetSpeed());
                    }
                    //Otherwise, stop this Neck sphere.
                    else
                    {
                        hydraSegment[i].GetComponent<HydraNeck>().SetSpeed(0);
                    }
                }
            }
            //Yield out of this Coroutine at the end of every frame.
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Once the Head is close enough to its target position, stop all of the objects in place.
        SetSpeed(0);
        foreach (GameObject g in hydraSegment)
        {
            g.GetComponent<HydraNeck>().SetSpeed(0);
        }
    }

    //Turn the Hydra Head white, and make it invincible, for a very short period of time.
    public IEnumerator DamageCooldown()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Make the Snake immune to damage while it is flashing.
        dmgImmune = true;
        //Make the Snake solid white to show that it has been hit.
        hydraRenderer.material.shader = shaderGUIText;
        hydraRenderer.color = Color.white;
        //Do the same to the Snake's nose.
        noseRenderer.material.shader = shaderGUIText;
        noseRenderer.color = Color.white;
        //Reset the Damage Timer.
        damageTimer = 0;
        //Let the Snake be invincible for 1/30 of a second.
        while (damageTimer < Time.deltaTime * 2)
        {
            //If the Game is paused, don't update the Damage Timer.
            if (!LevelManager.instance.gamePaused)
            {
                damageTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Let the Snake be susceptible to damage again.
        dmgImmune = false;
        //Change the Shader back to the default.
        hydraRenderer.material.shader = shaderSpritesDefault;
        hydraRenderer.color = Color.white;
        //Do the same to the Snake's nose.
        noseRenderer.material.shader = shaderSpritesDefault;
        noseRenderer.color = Color.white;
    }

    // Update is called once per frame
    protected override void Update()
    {
        //Call EnemyController's Update.
        base.Update();
	}
}
