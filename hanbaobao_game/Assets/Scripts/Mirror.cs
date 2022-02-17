/*
 * Mirror.cs
 * 
 * The Miniboss of Stage 1.
 * A black-and-red version of the Player's ship, with most of the same actions
 * including 8-directional movement, barrel rolling, speed changing, shooting,
 * weapon switching, and turning invincible.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mirror : Miniboss
{
    public Collider2D hitbox;       //The collision hitbox for the Mirror.

    //Child GameObjects:
    public GameObject jetFlames;    //The object that displays the Mirror's "flames" Animation.
    public GameObject hitboxLight;  //The object that displays the Mirror's hitbox.

    //"Movement" Variables:
    private float baseSpeed;        //The basic, unmodified speed value for the Mirror.
    private float speedModifier;    //The speed variable that baseSpeed gets multiplied by to create the Mirror's current speed.
    private int speedSetting;       //The integer (1-4) that tracks which speed the Mirror is moving.
    private Vector2 direction;      //The values to apply to the Mirror's X- and Y-direction (-1, 0, or 1).
    private float movementStateTimer;       //How much time has elapsed during a Movement State.
    private float movementActiveDuration;   //The amount of time (in seconds) for the Mirror to be moving.
    private float movementIdleDuration;     //The amount of time (in seconds) for the Mirror to be stationary.
    private bool verticallyBlocked; //Whether there is a Bullet directly above or below the Mirror, obstructing its vertical movement.
    private List<Vector2> bulletPositions;  //The x- and y-positions of each of the Player-fired Bullets on screen.
    private float vertDistToPlayer; //The vertical distance between the Mirror and the Player; used for changing the Mirror's speed.
    private int bulletsAbove;       //The number of Player-fired Bullets directly in front of and slightly above the Mirror.
    private int bulletsBelow;       //The number of Player-fired Bullets directly in front of and slightly below the Mirror.
    
    //"Barrel Roll" Variables:
    private bool barrelRolling;     //Whether the Mirror is currently in the middle of a Barrel Roll.
    private float barrelRollMaxSpeedBoost;  //The initial amount of extra speed applied to the Mirror during a Barrel Roll.
    private float barrelRollCurrentSpeedBoost;  //The amount of speed applied to the Mirror during a Barrel Roll, which starts at "Max" and decays until it reaches 0.
    private float barrelRollSpeedBoostDecayFactor;  //The amount to scale "CurrentSpeedBoost" by, to create an asymptotic speed burst.
    private float barrelRollTimer;  //How much time has elapsed during a Barrel Roll.
    private float barrelRollDuration;   //The length (in seconds) of the Barrel Roll Animation Clip.
    public AnimationClip barrelRollClip;    //The Animation clip for the Upward Barrel Roll, to calculate "barrelRollDuration" with.
    private AnimationClip[] clips;  //The Animation clips used in the Barrel Roll, to calculate "barrelRollDuration" with.

    //Screen Bounds Variables:
    private bool clampPosition;     //Whether the Mirror's position should be clamped to the screen.
    public Collider2D screenEdges;  //The boundaries of the screen, which the Mirror can not travel past once its fight starts.
    private float halfWidth;        //The distance between the center of the ship and its left and right boundaries.
    private float halfHeight;       //The distance between the center of the ship and its top and bottom boundaries.
    private float xOverlap;         //How far past a horizontal boundary the Mirror has gone.
    private float yOverlap;         //How far past a vertical boundary the Mirror has gone.

    //"Weapons" Variables:
    public GameObject[] weapons;    //Prefab instances of each of the Mirror's possible weapons.
    private int weaponIndex;        //The index of the current Weapon that the Mirror has equipped.

    //"Cooldown" Variables:
    private float rateOfFire;       //The time that needs to elapse before another Bullet can be fired.
    private float cooldownTimer;    //The amount of time that has elapsed since the last Bullet fired.
    private bool inCooldown;        //Whether enough time has elapsed before the Mirror can fire again.
    private float weaponChangeCooldownTimer;    //The time that has elapsed since the Mirror switched weapons.
    private bool inWeaponChangeCooldown;        //Whether enough time has elapsed before the Mirror can switch weapons again.

    //"Respawn" Variables:
    public bool isAlive;                //Whether the Mirror is currently alive (and, by extension, can perform actions).
    private float deathTimer;           //The time that has elapsed since the Mirror died.
    private bool isInvincible;          //Whether the Mirror can currently take damage.
    private float invincibilityTimer;   //The time that has elapsed since the Mirror became invincible.

    //Sprite Variables:
    public Sprite spriteNeutral;    //The basic Side View of the Mirror.
    public Sprite spriteTiltedUp;   //The Side Mirror of the Ship with the left wing tilted up slightly.
    public Sprite spriteTiltedDown; //The Side Mirror of the Ship with the left wing tilted down slightly.

	// Use this for initialization
	protected override void Start()
    {
        //Call Miniboss' Start.
        base.Start();
        //Set the Mirror's "lives".
        try { hp = GameManager.instance.startingLives; }
        catch (System.NullReferenceException) { hp = 3; }
        //Initialize the basic movement variables.
        baseSpeed = 0.35f;
        SetSpeed(baseSpeed);
        barrelRollMaxSpeedBoost = 1.8f;
        barrelRollSpeedBoostDecayFactor = 0.96f;
        SpeedChange(2);
        clips = GetComponent<Animator>().runtimeAnimatorController.animationClips;
        direction = new Vector2(0, 0);
        movementStateTimer = 0f;
        movementActiveDuration = 0.5f;
        movementIdleDuration = 0.5f;
        verticallyBlocked = false;
        bulletPositions = new List<Vector2>();
        vertDistToPlayer = 0;
        bulletsAbove = 0;
        bulletsBelow = 0;
        //Set the variables used to keep the Mirror on-screen.
        clampPosition = false;
        halfWidth = GetComponent<SpriteRenderer>().bounds.extents.x;
        halfHeight = GetComponent<SpriteRenderer>().bounds.extents.y;
        //Set the "Alive"/"Invincible" values.
        isAlive = true;
        deathTimer = 0;
        isInvincible = false;
        invincibilityTimer = 0;
        //Set the Mirror's Weapon.
        ChangeWeapon(0);
        //Have the Mirror start moving.
        StartCoroutine(MovementLogic());
        //Enable the Mirror's shooting logic.
        StartCoroutine(ShootingLogic());
        //Enable the Mirror's weapon changing logic.
        StartCoroutine(WeaponChangingLogic());
    }

    //Check the Mirror's collider for collision with a Wall.
    void OnTriggerEnter2D(Collider2D box)
    {
        //If the object is tagged "Enemy", it can only be a Wall.
        if (box.gameObject.CompareTag("Enemy"))
        {
            //Reduce the Mirror's lives to 1, to kill it immediately.
            hp = 1;
            //Take the Mirror's last life.
            StartCoroutine(Die());
        }
    }

    /// <summary>
    /// Check the Collider component against the Mirror's hitbox.
    /// </summary>
    /// <param name="collider">The collider to check, attached to a Bullet-type GameObject.</param>
    /// <param name="damageValue">The amount of damage the Bullet would do. Used if "TakeDamage" needs to be called.</param>
    /// <returns>Whether or not the Bullet is touching another Collider.
    /// 0: The Bullet has not hit anything.
    /// 1: The Bullet has hit and damaged the Mirror.
    /// 2: The Bullet has hit a damage-immune part of the Mirror.
    /// </returns>
    public override int CheckCollision(Collider2D collider, int damageValue)
    {
        //Check the Bullet's Collider against the Mirror's Collider.
        if (collider.IsTouching(hitbox) && isAlive && !isInvincible)
        {
            //Kill the Mirror (for now).
            StartCoroutine(Die());
            return 1;
        }
        return 0;
    }

    /// <summary>
    /// Enable/disable all of the Mirror's Animators when the Pause Button is pressed.
    /// </summary>
    /// <param name="active">Whether the Animators should be enabled or not.</param>
    public override void ToggleAnimations(bool active)
    {
        GetComponent<Animator>().enabled = active;
        jetFlames.GetComponent<Animator>().enabled = active;
        hitboxLight.GetComponent<Animator>().enabled = active;
    }

    //Get all of the positions of the Player's Bullets, to inform the Mirror's movement.
    private void GetBulletPositions()
    {
        bulletPositions.Clear();
        foreach (GameObject bullet in GameObject.FindGameObjectsWithTag("PlayerBullet"))
        {
            bulletPositions.Add(bullet.transform.position);
        }
    }

    //Handle the underlying logic for all of the Mirror's basic movement, using a number of external factors.
    IEnumerator MovementLogic()
    {
        //Send the Mirror quickly into frame.
        yield return new WaitForEndOfFrame();
        SpeedChange(4);
        SetAngleInDegrees(180f);
        yield return new WaitForSeconds(.5f);
        //Stop the Mirror on the right edge of the screen.
        SpeedChange(1);
        SetTarget(transform.position);
        //Start clamping the Mirror's position to within the screen boundaries.
        clampPosition = true;
        //Run the rest of the Movement Logic while the Mirror is alive.
        while (hp > 0)
        {
            //Only update the Mirror's A.I. while both it and the Player are alive.
            if (isAlive && LevelManager.instance.player.GetComponent<ShipController>().isAlive)
            {
                //Update the Bullet positions.
                GetBulletPositions();

                #region Changing Speed
                //Get the vertical distance between the Player and the Mirror.
                vertDistToPlayer = Mathf.Abs(LevelManager.instance.player.transform.position.y - transform.position.y);
                //Set the Mirror's new speed according to the vertical distance.
                if (vertDistToPlayer > 120)
                {
                    SpeedChange(4);
                }
                else if (vertDistToPlayer > 90 && vertDistToPlayer <= 120)
                {
                    SpeedChange(3);
                }
                else if (vertDistToPlayer > 60 && vertDistToPlayer <= 90)
                {
                    SpeedChange(2);
                }
                else if (vertDistToPlayer <= 60)
                {
                    SpeedChange(1);
                }
                #endregion

                #region Vertical Movement
                //Check if the Player is too far above the Mirror.
                if (LevelManager.instance.player.transform.position.y > transform.position.y && Mathf.Abs(LevelManager.instance.player.transform.position.y - transform.position.y) > 10)
                {
                    verticallyBlocked = false;
                    foreach (Vector2 b in bulletPositions)
                    {
                        //Check if the Mirror would hit a Bullet by moving upward.
                        if (b.y - transform.position.y < 5 && Mathf.Abs(b.x - transform.position.x) < 20)
                        {
                            verticallyBlocked = true;
                        }
                    }
                    //If the Mirror would hit a Bullet, don't move up. Otherwise, do.
                    direction.y = verticallyBlocked ? 0 : 1;
                }
                //Check if the Player is too far below the Mirror.
                else if (LevelManager.instance.player.transform.position.y < transform.position.y && Mathf.Abs(LevelManager.instance.player.transform.position.y - transform.position.y) > 10)
                {
                    verticallyBlocked = false;
                    foreach (Vector2 b in bulletPositions)
                    {
                        //Check if the Mirror would hit a Bullet by moving downward.
                        if (transform.position.y - b.y < 5 && Mathf.Abs(b.x - transform.position.x) < 20)
                        {
                            verticallyBlocked = true;
                        }
                    }
                    //If the Mirror would hit a Bullet, don't move down. Otherwise, do.
                    direction.y = verticallyBlocked ? 0 : -1;
                }
                //If the Mirror is within range, stop moving it vertically.
                else
                {
                    direction.y = 0;
                }
                #endregion

                #region Horizontal Movement
                //If the Mirror is too far, horizontally, from the Player, move it left.
                if (transform.position.x - LevelManager.instance.player.transform.position.x > 180f)
                {
                    direction.x = -1;
                }
                //Otherwise, if the Mirror is too close to the Mirror, move it right.
                else if (transform.position.x - LevelManager.instance.player.transform.position.x < 90f)
                {
                    direction.x = 1;
                }
                //If the Mirror is in just the right relative position, don't move it horizontally.
                else
                {
                    direction.x = 0;
                }
                #endregion

                #region Barrel Rolling
                //Reset the bullet counters.
                bulletsAbove = 0;
                bulletsBelow = 0;
                foreach (Vector2 b in bulletPositions)
                {
                    //Check if a Bullet is directly in front of the Mirror.
                    if (transform.position.x - b.x > 0 && transform.position.x - b.x < 40)
                    {
                        //If the Bullet is slightly above the Mirror, move the Mirror backward and increase the counter.
                        if (b.y > transform.position.y && Mathf.Abs(b.y - transform.position.y) < 20)
                        {
                            direction.x = 1;
                            bulletsAbove++;
                        }
                        //If the Bullet is slightly below the Mirror, move the Mirror backward and increase that counter.
                        else if (b.y < transform.position.y && Mathf.Abs(b.y - transform.position.y) < 20)
                        {
                            direction.x = 1;
                            bulletsBelow++;
                        }
                    }
                }
                //If there are at least 5 Bullets approaching above the Mirror (and fewer below), barrel roll downward.
                if (bulletsAbove > 5 && bulletsAbove > bulletsBelow && !barrelRolling)
                {
                    StartCoroutine(BarrelRoll(-1));
                }
                //If there are at least 5 Bullets approaching below the Mirror, barrel roll upward.
                else if (bulletsBelow > 5 && !barrelRolling)
                {
                    StartCoroutine(BarrelRoll(1));
                }
                #endregion

                #region Update Visuals
                //Only update the Sprite and the Flames if the Mirror isn't Barrel Rolling.
                if (!barrelRolling)
                {
                    //Update the Sprite based on the Vertical Direction of the Mirror.
                    switch (direction.y)
                    {
                        case -1:
                            GetComponent<SpriteRenderer>().sprite = spriteTiltedDown;
                            break;
                        case 0:
                            GetComponent<SpriteRenderer>().sprite = spriteNeutral;
                            break;
                        case 1:
                            GetComponent<SpriteRenderer>().sprite = spriteTiltedUp;
                            break;
                    }
                    //Update the Sprite based on the Horizontal Direction of the Mirror.
                    switch (direction.x)
                    {
                        case -1:
                            UpdateFlames(speedSetting + 1);
                            break;
                        case 0:
                            UpdateFlames(speedSetting);
                            break;
                        case 1:
                            UpdateFlames(speedSetting - 1);
                            break;
                    }
                }
                #endregion

                //Change the Mirror's direction using the results of the above logic.
                SetTarget((Vector2)transform.position + direction);
            }
            //Otherwise, stop in place.
            else
            {
                SetSpeed(0);
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    //Keep the Ship within the bounds of the Camera.
    private void ClampPosition()
    {
        //Check the Mirror Ship's left boundary.
        xOverlap = screenEdges.bounds.min.x - (transform.position.x - halfWidth);
        if (xOverlap > 0) { transform.Translate(xOverlap, 0, 0); }
        //Check the Mirror Ship's right boundary.
        else
        {
            xOverlap = screenEdges.bounds.max.x - (transform.position.x + halfWidth);
            if (xOverlap < 0) { transform.Translate(xOverlap, 0, 0); }
        }
        //Check the Mirror Ship's top boundary.
        yOverlap = screenEdges.bounds.min.y - (transform.position.y - halfHeight);
        if (yOverlap > 0) { transform.Translate(0, yOverlap, 0); }
        //Check the Mirror Ship's bottom boundary.
        else
        {
            yOverlap = screenEdges.bounds.max.y - (transform.position.y + halfHeight);
            if (yOverlap < 0) { transform.Translate(0, yOverlap, 0); }
        }
    }

    //Set the Mirror moving directly up (dir=1) or directly down (dir=-1).
    IEnumerator BarrelRoll(int dir)
    {
        barrelRolling = true;
        GetComponent<Animator>().enabled = true;
        if (dir > 0)
        {
            GetComponent<Animator>().SetTrigger("rollup");
        }
        else
        {
            GetComponent<Animator>().SetTrigger("rolldown");
        }
        //Yield out of this Coroutine to update the Animator state.
        yield return new WaitForEndOfFrame();
        //Set the Mirror's velocity.
        direction.x = 0;
        direction.y = dir;
        SetTarget((Vector2)transform.position + direction);
        barrelRollCurrentSpeedBoost = barrelRollMaxSpeedBoost;
        //Set the timer and get the duration of the Barrel Roll Animation.
        barrelRollDuration = barrelRollClip.length / Mathf.Abs(GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).speed);
        barrelRollTimer = 0;
        //Run the timer for the duration of the Barrel Roll.
        while (barrelRollTimer < barrelRollDuration)
        {
            //If the Game is paused, pause the Barrel Roll timing.
            if (!LevelManager.instance.gamePaused)
            {
                //First, check that the Mirror is still alive.
                if (!isAlive)
                {
                    //If not, stop the Barrel Roll.
                    barrelRolling = false;
                    SetTarget(transform.position);
                    SetSpeed(0);
                    yield break;
                }
                //Decay the speed boost.
                barrelRollCurrentSpeedBoost *= barrelRollSpeedBoostDecayFactor;
                SetSpeed(barrelRollCurrentSpeedBoost);
                //Increment the duration timer.
                barrelRollTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        barrelRolling = false;
        GetComponent<Animator>().enabled = false;
        SetTarget(transform.position);
        SetSpeed(0);
    }

    /// <summary>
    /// Change the Mirror's speed.
    /// </summary>
    /// <param name="newSpeed">The speed level (1-4) to change "speedModifier" with.</param>
    private void SpeedChange(int newSpeed)
    {
        //Set the Mirror's overall speed setting to the new value.
        speedSetting = newSpeed;
        //Make the speed modifier half of the passed-in setting.
        speedModifier = newSpeed / 2f;
        //Update the Mirror's speed using its base speed times its modifier.
        SetSpeed(baseSpeed * speedModifier);
        //Update the Mirror's "jet flames" Animation.
        UpdateFlames(newSpeed);
    }

    //Start the Animation for the Mirror's flames based on speed and movement direction.
    private void UpdateFlames(int newFlameState)
    {
        //Check that the Animator is not already on the Animation it is being told to change to.
        if (jetFlames.GetComponent<Animator>().GetInteger("setFlameNumber") != newFlameState)
        {
            //Change to the new Animation State, which will start the next Animation cycle.
            jetFlames.GetComponent<Animator>().SetInteger("setFlameNumber", newFlameState);
        }
    }

    //Determine when to switch weapons, using the Player's horizontal distance from the Mirror.
    IEnumerator WeaponChangingLogic()
    {
        yield return new WaitForEndOfFrame();
        //Run this routine for as long as the Mirror is alive.
        while (hp > 0)
        {
            //If the Player is far in front of the Mirror, switch to the Base Gun.
            if (transform.position.x - LevelManager.instance.player.transform.position.x > halfWidth * 10)
            {
                //Check that the Mirror is currently alive, its weapon switch action is not in cooldown, and the weapon isn't equipped already.
                if (isAlive && !inWeaponChangeCooldown && weaponIndex != 0)
                {
                    ChangeWeapon(0);
                }
            }
            //If the Player is close to or behind the Mirror, switch to the Boomerang.
            else if (transform.position.x - LevelManager.instance.player.transform.position.x < halfWidth * 10)
            {
                //Check that the Mirror is currently alive, its weapon switch action is not in cooldown, and the weapon isn't equipped already.
                if (isAlive && !inWeaponChangeCooldown && weaponIndex != 1)
                {
                    ChangeWeapon(1);
                }
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    //Change the Mirror's Weapon when necessary.
    private void ChangeWeapon(int newWeaponID)
    {
        //Set the new Weapon index.
        weaponIndex = newWeaponID;
        //Change the rate of fire to match the new Weapon.
        switch (weaponIndex)
        {
            //Case 0: Base Gun
            case 0:
                rateOfFire = 1f / 20f;
                break;
            //Case 1: Boomerang
            case 1:
                rateOfFire = 1f / 3f;
                break;
            //Case 2: Pierce Laser
            case 2:
                rateOfFire = 1f / 5f;
                break;
            //Default (never reached)
            default:
                break;
        }
        //Switch to the new Hitbox color.
        hitboxLight.GetComponent<Animator>().SetInteger("setHitboxColor", newWeaponID);
        hitboxLight.GetComponent<Animator>().SetTrigger("changingWeapon");
        //Start the Weapon Switch Cooldown timer.
        StartCoroutine(WeaponChangingCooldown());
    }

    //Run a 5-second cooldown between the last time the Mirror switched weapons and the next time.
    IEnumerator WeaponChangingCooldown()
    {
        //Put the Mirror's weapon switch ability in cooldown.
        inWeaponChangeCooldown = true;
        //Reset the timer to keep track of the time between weapon switches.
        weaponChangeCooldownTimer = 0f;
        //Start the Cooldown Timer.
        while (weaponChangeCooldownTimer < 5)
        {
            //If the Game is paused, pause the Cooldown Timer.
            if (!LevelManager.instance.gamePaused)
            {
                weaponChangeCooldownTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Enable the Mirror to switch Weapons again when the timer expires.
        inWeaponChangeCooldown = false;
    }

    //Determine when the Mirror should try to fire, using the Player's relative position and distance.
    IEnumerator ShootingLogic()
    {
        yield return new WaitForEndOfFrame();
        //Keep this loop running as long as the Mirror has lives left.
        while (hp > 0)
        {
            //Check that the Player is within vertical range of the Mirror, and is in front of it.
            if (Mathf.Abs(LevelManager.instance.player.transform.position.y) - Mathf.Abs(transform.position.y) < (halfHeight * 2) && LevelManager.instance.player.transform.position.x < transform.position.x)
            {
                //Check that the Player is currently alive.
                if (LevelManager.instance.player.GetComponent<ShipController>().isAlive)
                {
                    //See if the Mirror can currently shoot.
                    Shoot();
                }
            }
            //Yield to allow the rest of the Game to run.
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    //Fire a projectile when the Mirror's weapon is not in cooldown.
    private void Shoot()
    {
        //Check that the Mirror's weapon is not in "cooldown" mode.
        if (!inCooldown && isAlive)
        {
            //Put the Mirror's weapon in "cooldown" mode.
            inCooldown = true;
            //Fire the Mirror's weapon, then track its "rate-of-fire" timer.
            StartCoroutine(FiringCooldown());
        }
    }

    //Create a new instance of the Mirror's current projectile, then wait briefly before creating another.
    private IEnumerator FiringCooldown()
    {
        //Add a newly-created instance of the Mirror's Bullet to the LevelManager's list, to use in LevelManager's collision checks.
        LevelManager.instance.AddBulletToList(CreateBullet());
        //Wait for a brief period of time before letting the Mirror fire another Bullet.
        //Reset the Cooldown Timer.
        cooldownTimer = 0f;
        //Start the Cooldown Timer.
        while (cooldownTimer < rateOfFire)
        {
            //If the Game is paused, pause the Cooldown Timer.
            if (!LevelManager.instance.gamePaused)
            {
                cooldownTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Take the Mirror's Weapon out of "cooldown" mode when the timer expires.
        inCooldown = false;
    }

    //Create the Bullet for the Mirror to fire.
    private GameObject CreateBullet()
    {
        //Create the template "bullet" object.
        GameObject bullet;
        switch(weaponIndex)
        {
            //Case 0: Base Gun
            case 0:
                bullet = Instantiate(weapons[0], transform.position + Vector3.forward, Quaternion.identity) as GameObject;
                bullet.GetComponent<Bullet>().SetAngleInDegrees(180f);
                bullet.GetComponent<Bullet>().SetSpeed(3f);
                return bullet;
            //Case 1: Boomerang
            case 1:
                bullet = Instantiate(weapons[1], transform.position + Vector3.forward, Quaternion.identity) as GameObject;
                bullet.GetComponent<Bullet>().SetAngleInDegrees(180f);
                bullet.GetComponent<Bullet>().SetSpeed(1.8f);
                return bullet;
            //Case 2: Pierce Laser
            case 2:
                bullet = Instantiate(weapons[2], transform.position + Vector3.forward, Quaternion.identity) as GameObject;
                bullet.GetComponent<Bullet>().SetAngleInDegrees(180f);
                bullet.GetComponent<Bullet>().SetSpeed(4f);
                return bullet;
            //Default (never reached)
            default:
                bullet = Instantiate(weapons[0], transform.position + Vector3.forward, Quaternion.identity) as GameObject;
                bullet.GetComponent<Bullet>().SetAngleInDegrees(180f);
                bullet.GetComponent<Bullet>().SetSpeed(3f);
                return bullet;
        }
    }

    //Have the Mirror lose a Life, and either respawn it (if it has a spare Life) or end the fight (if it doesn't).
    IEnumerator Die()
    {
        //Reduce the Mirror's HP by one "life".
        TakeDamage(1);
        //Set the Mirror to "dead" (so it doesn't perform any actions before respawning).
        isAlive = false;
        //Make the Mirror's Sprite invisible.
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        jetFlames.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        hitboxLight.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        //Wait 3 seconds before re-enabling the Mirror's AI.
        //Reset the Death Timer.
        deathTimer = 0;
        //Start the Death Timer.
        while (deathTimer < 3)
        {
            //If the Game gets paused, pause the Death Timer.
            if (!LevelManager.instance.gamePaused)
            {
                deathTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Check if the Mirror has a life left.
        if (hp > 0)
        {
            //Set the Mirror back to "alive" (so it can perform actions again).
            isAlive = true;
            //Give the Mirror its invincibility grace period after respawning.
            StartCoroutine(Invincibility());
        }
    }

    //Make the Mirror invincible and semi-transparent for 5 seconds, then set it back to normal.
    private IEnumerator Invincibility()
    {
        //Make the Mirror immune to damage.
        isInvincible = true;
        //Make the Mirror's Sprite semi-transparent.
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, .5f);
        jetFlames.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, .5f);
        hitboxLight.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, .5f);
        //Give the Mirror five seconds of invincibility.
        //Reset the Invincibility Timer.
        invincibilityTimer = 0f;
        //Start the Invincibility Timer.
        while (invincibilityTimer < 5)
        {
            //If the Game gets paused, pause the Death Timer.
            if (!LevelManager.instance.gamePaused)
            {
                invincibilityTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Make the Mirror's Sprite fully opaque.
        GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        jetFlames.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        hitboxLight.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        //Make the Mirror susceptible to damage again.
        isInvincible = false;
    }

	// Update is called once per frame
	protected override void Update()
    {
        //Only clamp the Mirror's position if this flag is true.
        if (clampPosition)
        {
            //Keep the Mirror within the edges of the Screen during its fight.
            ClampPosition();
        }
        //Call Miniboss' Update.
        base.Update();
    }
}
