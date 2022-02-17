/*
 * Snake.cs
 * 
 * An Enemy that asymptotically travels to a random point on a circle around its head, whose radius its equal to its length.
 * When it gets close enough to that point, it fires a slow-moving laser before picking a new point and traveling to it.
 * 
 */

using UnityEngine;
using System.Collections;

public class Snake : EnemyController
{
    public GameObject snakeBullet;          //The "Master Copy" of the Bullet that the Snake will fire.
    public GameObject snakeBodyTemplate;    //The GameObject that "snakeBody" will be filled with copies of.
    public GameObject[] snakeBody;          //The array of Objects that make up the Snake's Body.

    private Vector3 lastTarget; //The last position the Snake moved toward (used for body part distance calculations).
    private Vector3 nextTarget; //The next position for the Snake to start moving toward.

    private float movementAngle;//The new angle (in degrees) that determines the Snake's new trajectory.
    private int moveCycles;     //How many times the Snake has picked a new target (used for seeding).
    private float diameter;     //The diameter of each Snake "part"'s CircleCollider.
    private float distance;     //The distance between the Snake's head and its new target per frame.
    private float xDist;        //The magnitude (absolute value) of the horizontal distance between starting position and target.
    private float yDist;        //The magnitude (absolute value) of the vertical distance between starting position and target.
    private float snakeLength;  //The total length of the Snake, from the Head's position to the last Body sphere's position.
    private float maxSpeed;     //The Snake's initial speed when it starts a new movement cycle.
    private float minSpeed;     //The Snake's minimum speed during its movement cycle.
    private float currentSpeed; //The Snake's desired speed every frame (normally base speed times remaining distance).
    private float distPercentage;   //The remaining percentage of distance ("distance" / "snakeLength")
    private float distTraveled; //The current distance traveled by the Snake during each movement cycle.
    private float cooldownTimer;//The timer for each of the pauses between the Snake's behavior states.
    private float damageTimer;  //The amount of time that the Snake has "flashed" invincible.

    //"Flash" Variables:
    private SpriteRenderer snakeRenderer;   //The Sprite Renderer attached to this GameObject.
    private SpriteRenderer noseRenderer;    //The Sprite Renderer attached to the Snake's "snakeNose" child Object.
    private Shader shaderGUIText;           //A Text Shader, used to turn the Snake Head solid white, even during its Animations.
    private Shader shaderSpritesDefault;    //The GameObject's default Shader, used to restore it to its original color.

    // Use this for initialization
    protected override void Start()
    {
        //Call EnemyController's Start.
        base.Start();
        //Overwrite the EnemyController default values.
        SetSpeed(0f);
        hp = 8;
        scoreValue = 300;
        rateOfFire = 2.5f;
        cooldownTimer = 0f;

        dmgImmune = false;
        lastTarget = transform.position;
        nextTarget = transform.position;
        //Get the constant values regarding the Snake, to use in movement calculation.
        diameter = GetComponent<CircleCollider2D>().radius * 2f * transform.lossyScale.x;
        snakeLength = diameter * snakeBody.Length;
        maxSpeed = .3f;
        minSpeed = .01f;
        //Initialize the Snake's "Body" array.
        for (int i = 0; i < snakeBody.Length; i++)
        {
            //Put each Body object behind the last one on the z-axis, to prevent them from overlapping incorrectly.
            snakeBody[i] = Instantiate(snakeBodyTemplate, transform.position + new Vector3(0, 0, i + 1), Quaternion.identity) as GameObject;
            //Make each part of the Snake's body immune to damage; only the head can be damaged directly.
            snakeBody[i].GetComponent<EnemyController>().dmgImmune = true;
        }
        //Initialize the objects used for the Snake's "Hit Flash" effect.
        snakeRenderer = gameObject.GetComponent<SpriteRenderer>();
        noseRenderer = gameObject.GetComponentsInChildren<SpriteRenderer>()[1];
        shaderGUIText = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");
    }

    //Activate the Snake's behaviors when it appears on screen.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            //Add the Head to LevelManager's list of Enemies.
            LevelManager.instance.AddEnemyToList(gameObject);
            //Add every part of the Body to LevelManager's list of Enemies.
            foreach (GameObject g in snakeBody)
            {
                LevelManager.instance.AddEnemyToList(g);
            }
            //Have the Snake start firing.
            StartCoroutine(MovementLogic());
        }
        //Otherwise, check if this is a Player-controlled Bullet. 
        else if (box.gameObject.CompareTag("PlayerBullet"))
        {
            //If it is, make the Snake "flash" and make it invincible temporarily.
            StartCoroutine(DamageCooldown());
            if (!dmgImmune)
            {
                //Have the Head take damage.
                TakeDamage(box.gameObject.GetComponent<Bullet>().dmgValue);
                //Have each of the Body spheres take the same amount of damage.
                for (int i = 0; i < snakeBody.Length; i++)
                {
                    snakeBody[i].GetComponent<EnemyController>().TakeDamage(box.gameObject.GetComponent<Bullet>().dmgValue);
                }
            }
        }
        //Otherwise, check if this is an Explosion.
        else if (box.gameObject.CompareTag("Explosion"))
        {
            //Also check if the Explosion is damaging.
            if (box.gameObject.GetComponent<Explosion>().isDamaging)
            {
                //If it is, make the Snake "flash" and make it invincible temporarily.
                StartCoroutine(DamageCooldown());
                if (!dmgImmune)
                {
                    //Have the Head take damage.
                    TakeDamage(box.gameObject.GetComponent<Explosion>().dmgValue);
                    //Have each of the Body spheres take the same amount of damage.
                    for (int i = 0; i < snakeBody.Length; i++)
                    {
                        snakeBody[i].GetComponent<EnemyController>().TakeDamage(box.gameObject.GetComponent<Explosion>().dmgValue);
                    }
                }
            }
        }
    }

    //Despawn the Snake when it passes outside of the screen.
    void OnTriggerExit2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            //Remove the Head from LevelManager's list of Enemies.
            LevelManager.instance.RemoveEnemyFromList(gameObject);
            //Remove every part of the Body from LevelManager's list of Enemies.
            foreach (GameObject g in snakeBody)
            {
                LevelManager.instance.RemoveEnemyFromList(g);
            }
        }
    }

    //Create a new "target" position via a random function, seeded by the number of bullets fired by the Player.
    private Vector2 GetNewAngle(int seed)
    {
        //Find an angle, and get the x- and y-distance based off of that angle.
        do
        {
            //Random.InitState(seed);
            movementAngle = Random.Range(0, 15) * 22.5f;
            //Set the two pythagorean components of the Snake's new movement target (the hypotenuse should always be as long as the Snake itself).
            xDist = Mathf.Cos(movementAngle * Mathf.Deg2Rad) * snakeLength;
            yDist = Mathf.Sin(movementAngle * Mathf.Deg2Rad) * snakeLength;
        }
        //If this angle would push the Snake back off of the screen in any direction (except left), find a different one.
        while (transform.position.x + xDist >= LevelManager.instance.screenEdges.bounds.max.x ||
        transform.position.y + yDist >= LevelManager.instance.screenEdges.bounds.max.y ||
        transform.position.y + yDist <= LevelManager.instance.screenEdges.bounds.min.y);
        //Return the new target position.
        return new Vector3(transform.position.x + xDist, transform.position.y + yDist, transform.position.z);
    }

    //Handle all of the Snake's movement and firing logic.
    IEnumerator MovementLogic()
    {
        //Wait for 1.5 seconds before starting movement.
        while (cooldownTimer < 1.5f)
        {
            //If the Game is paused, don't update the Cooldown Timer.
            if (!LevelManager.instance.gamePaused)
            {
                cooldownTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Keep the Snake firing and moving while it still has HP.
        while (hp > 0)
        {
            //Create an instance of the Bullet that will appear behind the Snake's head (on the z-axis).
            GameObject bullet = Instantiate(snakeBullet, transform.position + new Vector3(-2f, 0f, 1f), Quaternion.identity) as GameObject;
            //Add the Bullet to the LevelManager's list.
            LevelManager.instance.AddBulletToList(bullet);
            //Set the target for the laser directly to the left of the Snake's Head.
            bullet.GetComponent<Bullet>().SetAngleInDegrees(180f);
            bullet.GetComponent<Bullet>().SetSpeed(.4f);
            //Reset the Cooldown Timer.
            cooldownTimer = 0;
            //Wait another half-second before starting the next movement cycle.
            while (cooldownTimer < .5f)
            {
                //If the Game is paused, don't update the Cooldown Timer.
                if (!LevelManager.instance.gamePaused)
                {
                    cooldownTimer += Time.deltaTime;
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //Pass the current target Vector to "lastTarget".
            lastTarget = nextTarget;
            //Get the new target position via a random function,
            //seeded with the Snake's number of elapsed movement cycles times the number of Bullets the Player fired in this Stage so far.
            nextTarget = GetNewAngle(LevelManager.instance.shotsFired * moveCycles);
            //Set the new movement target for the Head.
            SetTarget(nextTarget);
            //Reset the Snake's travel distance.
            distTraveled = 0;
            //Set the speed here to prevent the Snake from moving prematurely.
            SetSpeed(maxSpeed);
            //Set the next movement target for each part of the Body to travel to, after it reaches its last one.
            for (int i = 0; i < snakeBody.Length; i++)
            {
                snakeBody[i].GetComponent<SnakeBody>().UpdateTarget(nextTarget);
            }
            //Continue the next loop until the Snake head reaches its next destination.
            while (distTraveled < snakeLength)
            {
                //Check the remaining distance between the Snake's Head and its movement target.
                distance = Vector2.Distance(transform.position, nextTarget);
                //Check the distance that the Snake has traveled this cycle.
                distTraveled = Vector2.Distance(transform.position, lastTarget);
                //Calculate the percentage of distance remaining between "lastTarget" and "nextTarget".
                distPercentage = distance / snakeLength;
                //Set the Snake's speed for this frame.
                currentSpeed = maxSpeed * distPercentage;
                //If the speed is too low and the Snake hasn't traveled far enough yet, cap it at .1.
                if (currentSpeed < minSpeed && distTraveled < snakeLength)
                {
                    currentSpeed = minSpeed;
                }
                //Use Movable's "smoothMove" to slow the Snake down as it approaches its target, making its movement asymptotic.
                SetSpeed(currentSpeed);
                //Check the distances between each adjacent part of the Snake, to keep them contiguous.
                for (int i = 0; i < snakeBody.Length; i++)
                {
                    //Check the distance between the Head and 1st Body sphere.
                    if (i == 0)
                    {
                        //If the Body is too far away from the Head, move it.
                        //Also check if this any movement cycle after the 1st, as all of the parts will move regardless from then on.
                        if (Vector2.Distance(transform.position, snakeBody[i].transform.position) >= diameter || moveCycles > 0)
                        {
                            //Update the speed of each Body sphere over time, to match the speed of the Head.
                            snakeBody[i].GetComponent<SnakeBody>().SetSpeed(currentSpeed);
                        }
                        else
                        {
                            snakeBody[i].GetComponent<SnakeBody>().SetSpeed(0);
                        }
                    }
                    //Check the distance between the rest of the adjacent Body spheres.
                    else if (i >= 1)
                    {
                        //If the Body spheres are too far apart, move the next one in line.
                        //Also check if this any movement cycle after the 1st, as all of the parts will move regardless from then on.
                        if (Vector2.Distance(snakeBody[i].transform.position, snakeBody[i - 1].transform.position) >= diameter || moveCycles > 0)
                        {
                            //Update the speed of each Body sphere over time, to match the speed of the Head.
                            snakeBody[i].GetComponent<SnakeBody>().SetSpeed(currentSpeed);
                        }
                        else
                        {
                            snakeBody[i].GetComponent<SnakeBody>().SetSpeed(0);
                        }
                    }
                }
                //Yield out of this Coroutine at the end of every cycle.
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //Once the Snake head has reached its destination, stop it in place.
            currentSpeed = 0;
            SetSpeed(currentSpeed);
            //Reset the Cooldown Timer.
            cooldownTimer = 0;
            //Wait .5 seconds before firing the next laser.
            while (cooldownTimer < .5f)
            {
                //If the Game is paused, don't update the Cooldown Timer.
                if (!LevelManager.instance.gamePaused)
                {
                    cooldownTimer += Time.deltaTime;
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //Increment the number of movement cycles that have elasped.
            moveCycles++;
        }
    }

    // Update is called once per frame
    protected override void Update()
    {
        //If the Game isn't paused, update the movement target Vectors.
        if (!LevelManager.instance.gamePaused)
        {
            lastTarget.Set(lastTarget.x + (BGManager.instance.scrollValues[0] + BGManager.instance.scrollOffsets[0]), lastTarget.y, lastTarget.z);
            nextTarget.Set(nextTarget.x + (BGManager.instance.scrollValues[0] + BGManager.instance.scrollOffsets[0]), nextTarget.y, nextTarget.z);
        }
        //Call EnemyController's Update.
        base.Update();
    }

    //Turn the Snake white, and make it invincible, for a very short period of time.
    IEnumerator DamageCooldown()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Make the Snake immune to damage while it is flashing.
        dmgImmune = true;
        //Make the Snake solid white to show that it has been hit.
        snakeRenderer.material.shader = shaderGUIText;
        snakeRenderer.color = Color.white;
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
        snakeRenderer.material.shader = shaderSpritesDefault;
        snakeRenderer.color = Color.white;
        //Do the same to the Snake's nose.
        noseRenderer.material.shader = shaderSpritesDefault;
        noseRenderer.color = Color.white;
    }
}
