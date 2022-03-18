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
    private float angle;        //The new angle (in degrees) that determines the Head's new target.
    private float xDist;        //The magnitude (absolute value) of the horizontal distance between Head base and target.
    private float yDist;        //The magnitude (absolute value) of the vertical distance between Head base and target.
    private float cooldownTimer;//The timer for each of the pauses between the Hydra Head's behavior states.
    //private Vector3 target;     //The new target position for the Hydra Head, relative to the parent "Hydra" object.

    private float distance;         //The distance between the Hydra's head and its new target per frame.
    private float maxDistance;      //The distance between the Hydra Head's last target position and its current target position.
    private float distPercentage;   //The remaining percentage of distance ("distance" / "maxDistance").
    private float maxSpeed;         //The Head's initial speed when it starts a new movement cycle.

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
        maxSpeed = 1.4f;
        rateOfFire = 4f;
        diameter = GetComponent<CircleCollider2D>().radius * 2f * transform.lossyScale.x;
        headLength = diameter * (hydraSegment.Length - 1);
        //Initialize the rest of each head's "Neck" array.
        for (int i = 1; i < hydraSegment.Length; i++)
        {
            //Put each Body object behind the last one on the z-axis, to prevent them from overlapping incorrectly.
            hydraSegment[i] = Instantiate(hydraNeckTemplate, transform.position + new Vector3(0, 0, i * .02f), Quaternion.identity);
            //Make each part of the Hydra's neck immune to damage; only the head can be damaged directly.
            hydraSegment[i].GetComponent<EnemyController>().dmgImmune = true;
            //Make each part of the Neck a child of the main Hydra object.
            hydraSegment[i].transform.SetParent(transform.parent, true);
        }
        //hydraSegment[hydraSegment.Length - 1].transform.SetParent(transform.parent, true);
        //hydraSegment[0].transform.SetParent(hydraSegment[hydraSegment.Length - 1].transform, true);
        //hydraSegment[1].transform.SetParent(hydraSegment[hydraSegment.Length - 1].transform, true);
        //hydraSegment[2].transform.SetParent(hydraSegment[hydraSegment.Length - 1].transform, true);
        anchor = hydraSegment[hydraSegment.Length - 1].transform.localPosition;

        //Initialize the objects used for the Hydra head's "Hit Flash" effect.
        hydraRenderer = gameObject.GetComponent<SpriteRenderer>();
        noseRenderer = gameObject.GetComponentsInChildren<SpriteRenderer>()[1];
        shaderGUIText = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");
	}

    //Create a new target position for the head via a random function.
    private void GetNewAngle(int seed)
    {
        Random.InitState(seed);
        angle = Random.Range(0, 15) * 22.5f;
        //Set the two pythagorean components of the Head's new movement target (the hypotenuse should always be as long as the Head itself).
        xDist = Mathf.Cos(angle * Mathf.Deg2Rad) * headLength;
        yDist = Mathf.Sin(angle * Mathf.Deg2Rad) * headLength;
        //target = new Vector3(hydraSegment[hydraSegment.Length - 1].transform.position.x + xDist, hydraSegment[hydraSegment.Length - 1].transform.position.y + yDist, transform.position.z);
        SetTarget(new Vector2(anchor.x + xDist, anchor.y + yDist));
        maxDistance = Vector2.Distance(transform.localPosition, GetTarget());
        //Update the movement targets of all of the Head's "Neck spheres".
        for (int i = 1; i < hydraSegment.Length; i++)
        {
            hydraSegment[i].GetComponent<HydraNeck>().SetTargetLocal(new Vector2(anchor.x + (xDist * (1.0f - (i/hydraSegment.Length-1))), anchor.y + (yDist * (1.0f - (i / hydraSegment.Length - 1)))));
        }
    }

    //Have the Head fire a shot before picking a new position for it to move to.
    public IEnumerator ShootAndMove(int index)
    {
        //Yield out of this Coroutine to let the others start.
        yield return new WaitForEndOfFrame();

        //BULLET FIRING:
        //Set the target for the laser directly to the left of the Hydra's Head.
        hydraBullet.GetComponent<Bullet>().SetAngleInDegrees(180f);
        hydraBullet.GetComponent<Bullet>().SetSpeed(.8f);
        //Create an instance of the Bullet that will appear behind the Hydra's head (on the z-axis).
        GameObject bullet = Instantiate(hydraBullet, transform.position + new Vector3(-0.02f, 0f, 1f), Quaternion.identity);
        //Add the Bullet to the LevelManager's list.
        LevelManager.instance.AddBulletToList(bullet);

        //HALF-SECOND WAIT:
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
            yield return new WaitForSeconds(0);
        }

        //START NEW MOVEMENT:
        //Get the Head's new target position
        GetNewAngle(Time.frameCount * index);
        //GetNewAngle(LevelManager.instance.shotsFired);
        //Reset the head's speed.
        SetSpeed(maxSpeed);
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
            yield return new WaitForSeconds(0);
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
        //If this head dies, destroy its neck.
        if (hp <= 0)
        {
            for (int i = 1; i < hydraSegment.Length; i++)
            {
                hydraSegment[i].GetComponent<EnemyController>().TakeDamage(hydraSegment[i].GetComponent<EnemyController>().hp);
            }
        }
        //Check the remaining distance between the Hydra head and its new target.
        distance = Vector2.Distance(transform.localPosition, GetTarget());
        //Calculate the percentage of distance remaining.
        distPercentage = distance / maxDistance;
        //Set the head's speed for this frame.
        SetSpeed(maxSpeed * distPercentage);
        //If the speed is too low, cap it at .09.
        /*if (currentSpeed < .09f)
        {
            currentSpeed = .09f;
        }*/
        //Check the distances between each adjacent part of the Hydra head, to keep them contiguous.
        for (int i = 1; i < hydraSegment.Length-1; i++)
        {
            //If the objects are too far apart, move the next one in line.
            if (Vector2.Distance(hydraSegment[i].transform.position, hydraSegment[i - 1].transform.position) >= diameter)
            {
                //Update the speed of each Neck sphere over time, to match the speed of the Head.
                hydraSegment[i].GetComponent<HydraNeck>().SetSpeed(GetSpeed());
            }
            else
            {
                //Update the speed of each Neck sphere over time, to match the speed of the Head.
                hydraSegment[i].GetComponent<HydraNeck>().SetSpeed(0);
            }
        }
        //If the head has extended its full length, stop all of the objects.
        if (Vector2.Distance(transform.position, hydraSegment[hydraSegment.Length - 1].transform.position) >= headLength)
        {
            //Stop the head (this object).
            SetSpeed(0);
            //Stop each of the Neck objects.
            for (int i = 1; i < hydraSegment.Length; i++)
            {
                hydraSegment[i].GetComponent<HydraNeck>().SetSpeed(0);
            }
        }
        //Call EnemyController's Update.
        base.Update();
	}
}
