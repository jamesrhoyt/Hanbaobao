/*
 * Hydra.cs
 * 
 * The Miniboss of Stage 3.
 * A large grey asteroid with 7 Snake-type Enemies embedded in its front.
 * Each Snake head drifts to a random point within its travel radius,
 * fires a laser, then repeats, while the asteroid itself slowly drifts 
 * toward the Player, eventually passing offscreen if it isn't destroyed.
 * 
 */

using UnityEngine;
using System.Collections;

public class Hydra : Miniboss
{
    public Collider2D hitbox;       //The collision hitbox for the Hydra's base "asteroid" object.
    private float xSpeed;           //The horizontal speed for the Hydra.
    private float ySpeed;           //The vertical speed for the Hydra.
    private float startingYPos;     //The Hydra's starting y-position, used for its vertical oscillation.

    public GameObject[] hydraHeads; //The 7 head objects that will exist on the front of the Hydra.
    private int headsRemaining;     //The number of Heads still alive (used as a flag to switch from Phase One to Phase Two.
    private float cooldownTimer;    //Keeps track of how much time has elapsed between actions.
    private IEnumerator headPhase;  //The IEnumerator instance used to switch between "HeadPhase" Coroutines.

	// Use this for initialization
	protected override void Start()
    {
        //Call Miniboss' Start.
        base.Start();
        //Override Miniboss' default values.
        hp = 700;
        //Set any other variables.
        xSpeed = /*Constants.cameraSpeed * .9999f*/-0.045f;
        ySpeed = /*Constants.cameraSpeed * .0001f*/0.011f;
        SetSpeed(0.015f);
        SetTarget(new Vector2(transform.position.x + xSpeed, transform.position.y + ySpeed));
        startingYPos = transform.position.y;
        headsRemaining = hydraHeads.Length;
        //Initialize the IEnumerator.
        headPhase = HeadPhaseOne();
        //Start the Hydra's movement logic.
        StartCoroutine(MovementLogic());
        //Start the Hydra's attacking logic.
        StartCoroutine(headPhase);
    }
	
    /// <summary>
    /// Check the Collider component against each of the objects that make up the Hydra.
    /// </summary>
    /// <param name="collider">The collider to check, attached to a Bullet-type GameObject.</param>
    /// <param name="damageValue">The amount of damage the Bullet would do. Used if "TakeDamage" needs to be called.</param>
    /// <returns>Whether or not the Bullet is touching another Collider.
    /// 0: The Bullet has not hit anything.
    /// 1: The Bullet has hit and damaged the Hydra.
    /// 2: The Bullet has hit a damage-immune part of the Hydra.
    /// </returns>
    public override int CheckCollision(Collider2D collider, int damageValue)
    {
        //If the Bullet hits the asteroid, tell LevelManager to destroy it.
        if (collider.IsTouching(hitbox))
        {
            return 2;
        }
        //Check the Bullet against each part of each of the Hydra's heads.
        else
        {
            for (int i = 0; i < hydraHeads.Length; i++)
            {
                //First, see if the Head still has HP.
                if (hydraHeads[i].GetComponent<EnemyController>().hp > 0)
                {
                    //See if the Bullet is touching the Head object itself.
                    if (collider.IsTouching(hydraHeads[i].GetComponent<EnemyController>().hitbox))
                    {
                        //See if the Head is currently immune to damage.
                        if (hydraHeads[i].GetComponent<EnemyController>().dmgImmune)
                        {
                            return 2;
                        }
                        //If not, damage it, damage the Hydra itself, and return.
                        else
                        {
                            //Damage all of the Neck objects first, in case the Head is destroyed.
                            foreach (GameObject g in hydraHeads[i].GetComponent<HydraHead>().hydraSegment)
                            {
                                g.GetComponent<EnemyController>().TakeDamage(damageValue);
                            }
                            hydraHeads[i].GetComponent<EnemyController>().TakeDamage(damageValue);
                            StartCoroutine(hydraHeads[i].GetComponent<HydraHead>().DamageCooldown());
                            TakeDamage(damageValue);
                            return 1;
                        }
                    }
                    //If not, check the Bullet against each of its HydraNeck objects.
                    else
                    {
                        for (int j = 0; j < hydraHeads[i].GetComponent<HydraHead>().hydraSegment.Length; j++)
                        {
                            if(collider.IsTouching(hydraHeads[i].GetComponent<HydraHead>().hydraSegment[j].GetComponent<EnemyController>().hitbox))
                            {
                                return 2;
                            }
                        }
                    }
                }
            }
        }
        return 0;
    }

    /// <summary>
    /// Toggle any Animators connected to the Hydra.
    /// </summary>
    /// <param name="active">Whether the Animators should be enabled or not.</param>
    public override void ToggleAnimations(bool active)
    {
        throw new System.NotImplementedException();
    }

    //The Coroutine that lets the Hydra drift slowly up and down.
    IEnumerator MovementLogic()
    {
        //Yield out of this Coroutine to let the others start.
        yield return new WaitForEndOfFrame();
        while (hp > 0)
        {
            //Move the Hydra slightly upward until it drifts 20 units vertically.
            while (transform.position.y - startingYPos < 20)
            {
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //Reverse the Hydra's Y-direction.
            ySpeed *= -1;
            SetTarget(new Vector2(transform.position.x + xSpeed, transform.position.y + ySpeed));
            //Move the Hydra slightly downward until it drifts 20 units vertically from its starting position.
            while (startingYPos - transform.position.y < 20)
            {
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //Reverse the Hydra's Y-direction.
            ySpeed *= -1;
            SetTarget(new Vector2(transform.position.x + xSpeed, transform.position.y + ySpeed));
        }
    }

    //The Hydra's first phase, where every head fires and moves simultaneously.
    IEnumerator HeadPhaseOne()
    {
        //Yield out of this Coroutine to let the others start.
        yield return new WaitForEndOfFrame();
        //Run this loop as long as the Hydra is alive.
        //(This loop will be ended externally.)
        while (hp > 0)
        {
            for (int i = 0; i < hydraHeads.Length; i++)
            {
                //If the head is still alive, run its movement/firing cycle.
                if (hydraHeads[i].GetComponent<EnemyController>().hp > 0)
                {
                    StartCoroutine(hydraHeads[i].GetComponent<HydraHead>().MovementCycle(i));
                }
            }

            //FOUR SECOND WAIT:
            //Reset the Cooldown Timer.
            cooldownTimer = 0;
            //Wait another four seconds before starting the next movement cycle.
            while (cooldownTimer < 4f)
            {
                //If the Game is paused, don't update the Cooldown Timer.
                if (!LevelManager.instance.gamePaused)
                {
                    cooldownTimer += Time.deltaTime;
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    //The Hydra's second phase, where the heads move and shoot in a round.
    IEnumerator HeadPhaseTwo()
    {
        yield return new WaitForEndOfFrame();
        //Run this loop for the rest of the Hydra's life.
        while (hp > 0)
        {
            //Iterate through every head in the array.
            for (int i = 0; i < hydraHeads.Length; i++)
            {
                //If the head is still alive, run its cycle and wait .75 seconds.
                if (hydraHeads[i].GetComponent<EnemyController>().hp > 0)
                {
                    StartCoroutine(hydraHeads[i].GetComponent<HydraHead>().MovementCycle(i));
                    //Reset the Cooldown Timer.
                    cooldownTimer = 0;
                    //Wait another .75 seconds before starting the next movement cycle.
                    while (cooldownTimer < .75f)
                    {
                        //If the Game is paused, don't update the Cooldown Timer.
                        if (!LevelManager.instance.gamePaused)
                        {
                            cooldownTimer += Time.deltaTime;
                        }
                        yield return new WaitForSeconds(Time.deltaTime);
                    }
                }
                //Otherwise, skip to the next one.
            }
        }
    }

    //Decrement the Hydra's head counter, and switch its firing behavior if it drops low enough.
    public void LoseHead()
    {
        //Lower the head counter by one.
        headsRemaining--;
        //Once the Hydra reaches four heads, change its logic Coroutine.
        if (headsRemaining == 4)
        {
            StopCoroutine(headPhase);
            headPhase = HeadPhaseTwo();
            StartCoroutine(headPhase);
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        //Call Miniboss' Update.
        base.Update();
	}
}
