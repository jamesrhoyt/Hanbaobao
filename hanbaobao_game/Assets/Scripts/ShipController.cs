/*
 * ShipController.cs
 * 
 * Handles the Player's input via the Directional and A/B/C Buttons, Player Bullet creation, 
 * and collision between the Player's Ship and Enemies and Enemy Bullets. 
 * 
 */

using UnityEngine;
using System.Collections;

public class ShipController : Movable2
{
    // Use this for initialization

    private bool buttonPressed;     //Whether a directional button has been pressed this frame.
    public GameObject playerBullet; //The currently equipped Weapon projectile prefab.
    private Vector2 direction;      //The values to apply to the Ship X- and Y-direction (-1, 0, or 1).
    private float baseSpeed;        //The basic speed value for the Player's ship.
    private float exitingSpeed;     //The speed of the Ship as it exits the screen to the right.
    private float speedModifier;    //How the Ship's speed will be affected, based on Speed setting.
    public int speedSetting;        //The value used to update the Speed Indicators' visibility.
    private float rateOfFire;       //The time that needs to elapse before another Bullet can be fired.
    private float cooldownTimer;    //The amount of time that has elapsed since the last Bullet fired.
    private bool inCooldown;        //Whether enough time has elapsed before the Player can fire again.
    public bool shieldActive;       //Whether the Player's currently has a shield equipped.
    public bool controlsEnabled;    //Whether the Ship's controls are enabled. (They are disabled for "transition" states and Cutscenes during the game.)

    public bool isAlive;                //Whether the Player's ship is currently alive (and, by extension, can take Player input).
    private float deathTimer;           //The time that has elapsed since the Player's Ship died.
    private bool isInvincible;          //Whether the Player's ship can currently take damage.
    private float invincibilityTimer;   //The time that has elapsed since the Player's Ship became invincible.

    //Screen Bounds Variables:
    private float halfWidth;        //The distance between the center of the ship and its left and right boundaries.
    private float halfHeight;       //The distance between the center of the ship and its top and bottom boundaries.
    private float xOverlap;         //How far past a horizontal boundary the Player has gone.
    private float yOverlap;         //How far past a vertical boundary the Player has gone.

    //Button "tap" Variables, used in activating the Barrel Roll maneuver:
    private int upTaps;                 //How many times the "Up" Button has been "tapped" (held for <.5 seconds) in a row.
    private float upTimer;              //How long the "Up" Button has been held continuously.
    private float upReleasedTimer;      //How long the "Up" Button has been released (the Tap counter resets if the Key hasn't been pressed recently).
    private int downTaps;               //How many times the "Down" Button has been "tapped" (held for <.5 seconds) in a row.
    private float downTimer;            //How long the "Down" Button has been held continuously.
    private float downReleasedTimer;    //How long the "Down" Button has been released (the Tap counter resets if the Key hasn't been pressed recently).

    //Barrel Roll Animation-related Variables:
    private bool barrelRolling;             //Whether the Player's ship is currently in the middle of a Barrel Roll.
    private float barrelRollMaxSpeedBoost;  //The initial amount of extra speed applied to the Ship during a Barrel Roll.
    private float barrelRollCurrentSpeedBoost;  //The amount of speed applied to the Ship during a Barrel Roll, which starts at "Max" and decays until it reaches 0.
    private float barrelRollSpeedBoostDecayFactor;  //The amount to scale "CurrentSpeedBoost" by, to create an asymptotic speed burst.
    private float barrelRollTimer;          //How much time has elapsed during a Barrel Roll.
    private float barrelRollDuration;       //The length (in seconds) of the Barrel Roll Animation Clip.
    public AnimationClip barrelRollClip;    //The Animation clip for the Upward Barrel Roll, to calculate "barrelRollDuration" with.

    //Sprite Variables:
    public Sprite spriteNeutral;    //The basic Side View of the Ship.
    public Sprite spriteTiltedUp;   //The Side View of the Ship with the right wing tilted up slightly.
    public Sprite spriteTiltedDown; //The Side View of the Ship with the right wing tilted down slightly.

    //Weapons Variables:
    public GameObject[] weapons;    //Prefab instances of each of the Player's possible weapons.
    public int[] weaponLevels;      //The power levels for each of the weapons that the Player can have equipped.
    public int weaponIndex;         //The index of the current weapon that the Player has equipped.
    private bool firingLightning;   //Whether or not the Player is currently firing the Lighting weapon, to prevent duplicate instances from being created.
    private float bulletSpeed_baseGun;  //The speed of the Base Gun Weapon's bullets.
    private float bulletSpeed_pierceLaser;  //The speed of the Pierce Laser Weapon's bullets.
    private float bulletSpeed_boomerang1;   //The speed of the Boomerang Weapon's bullets at Power Level 1.
    private float bulletSpeed_boomerang2;   //The speed of the Boomerang Weapon's bullets at Power Level 2.
    private float bulletSpeed_boomerang3;   //The speed of the Boomerang Weapon's bullets at Power Level 3.
    private float bulletSpeed_flakGunMinimum;   //The minimum speed of the Flak Gun Weapon's bullets.
    private float bulletSpeed_flakGunMaximum;   //The maximum speed of the Flak Gun Weapon's bullets.

    //Child GameObjects:
    public GameObject jetFlames;    //The object that displays the Player Ship's "flames" Animation.
    public GameObject hitboxLight;  //The object displays the Player Ship's hitbox and Weapon Indicator.
    public GameObject shield;       //The object that displays the Player's Shield, when they have one equipped.
    public AnimationClip shieldPopClip; //The Animation Clip for the Shield's destruction (its length is used for "Invoke" calls).

    protected override void Start()
    {
        buttonPressed = true;
        barrelRolling = false;
        direction = new Vector2(0, 0);
        baseSpeed = 0.35f;
        barrelRollMaxSpeedBoost = 1.8f;
        barrelRollSpeedBoostDecayFactor = 0.96f;
        exitingSpeed = 1.4f;
        speedModifier = 1.0f;
        //Reset the Speed Setting back to its default value.
        speedSetting = 2;
        //Update the Speed Indicators, to match the setting reset.
        UIManager.instance.UpdateSpeedDisplay(speedSetting);
        isAlive = true;
        deathTimer = 0f;
        shieldActive = false;
        controlsEnabled = false;
        isInvincible = false;
        invincibilityTimer = 0f;
        //Set the variables used to keep the Player on-screen.
        halfWidth = GetComponent<SpriteRenderer>().bounds.extents.x;
        halfHeight = GetComponent<BoxCollider2D>().bounds.extents.y;
        //Initialize the Player's weapons, weapon levels, and rate-of-fire.
        cooldownTimer = 0f;
        try
        {
            weaponIndex = GameManager.instance.weaponIndex;
            weaponLevels = GameManager.instance.weaponLevels;
            hitboxLight.GetComponent<Animator>().SetInteger("setHitboxColor", weaponIndex);
            hitboxLight.GetComponent<Animator>().SetTrigger("changingWeapon");
        }
        catch (System.NullReferenceException)
        {
            weaponIndex = 0;
        }
        ChangeRateOfFire();
        firingLightning = false;
        //Set the Bullet Speeds for the Weapons.
        bulletSpeed_baseGun = 0.7f;
        bulletSpeed_pierceLaser = 0.9f;
        bulletSpeed_boomerang1 = 2.0f;
        bulletSpeed_boomerang2 = 2.5f;
        bulletSpeed_boomerang3 = 3.0f;
        bulletSpeed_flakGunMinimum = 0.5f;
        bulletSpeed_flakGunMaximum = 1.2f;
        //Update the HUD's Power Bar to match the current Weapon Power.
        UIManager.instance.UpdatePowerDisplay(weaponLevels[weaponIndex]);
        //Enable the check for Button Input based on Config.
        try
        {
            switch (GameManager.instance.buttonConfig)
            {
                case 1: StartCoroutine(CheckButtons1()); break;
                case 2: StartCoroutine(CheckButtons2()); break;
                case 3: StartCoroutine(CheckButtons3()); break;
                case 4: StartCoroutine(CheckButtons4()); break;
                case 5: StartCoroutine(CheckButtons5()); break;
                case 6: StartCoroutine(CheckButtons6()); break;
            }
        }
        //If the Title Screen was never loaded, just use the default controls.
        catch (System.NullReferenceException)
        {
            StartCoroutine(CheckButtons1());
        }
        //Call Movable's Start.
        base.Start();
    }

    // Update is called once per frame
    protected override void Update()
    {
        //Only check for D-Pad input if the Player is alive and their controls are enabled, and the Game isn't Paused or in a Cutscene.
        if (isAlive && controlsEnabled && !LevelManager.instance.inCutscene && !LevelManager.instance.gamePaused)
        {
            //Check for input via the "Up" Keys.
            #region D-Pad Up Check
            if (LevelManager.instance.action_up.ReadValue<float>() > 0)
            {
                //Only take "Up" Input if the Down Key is not being held, and the Player is not currently barrel-rolling.
                if (downTimer == 0f && !barrelRolling)
                {
                    //Reset the "upReleased" timer.
                    upReleasedTimer = 0f;
                    //If the Up key wasn't held before this frame, increment the "tap" count.
                    if (upTimer == 0f)
                    {
                        //Set the Ship's Sprite to its "tilted up" pose.
                        GetComponent<SpriteRenderer>().sprite = spriteTiltedUp;
                        //Increase the tap count by one.
                        upTaps++;
                        //If the Up key has been double-tapped, start the Upward Barrel Roll.
                        if (upTaps == 2)
                        {
                            barrelRolling = true;
                            StartCoroutine(BarrelRoll(1));
                            upTaps = 0;
                        }
                    }
                    //Increment the "Up" Timer, and reset the "tap" counter if the Key is being held (>.5 seconds).
                    upTimer += Time.deltaTime;
                    if (upTimer >= .5f)
                    {
                        upTaps = 0;
                    }
                    //Change the Player's velocity.
                    direction.y = 1;
                    buttonPressed = true;
                }
            }
            //Reset the "Up" Timer if the key isn't being held.
            else
            {
                upTimer = 0f;
                //Set the Ship's Sprite back to its "neutral" pose.
                if (GetComponent<SpriteRenderer>().sprite == spriteTiltedUp)
                {
                    GetComponent<SpriteRenderer>().sprite = spriteNeutral;
                }
                //Increment the "upReleased" timer.
                upReleasedTimer += Time.deltaTime;
                //If the "Up" Key hasn't been pressed in .2 seconds, reset the Tap counter.
                if (upReleasedTimer >= .2f)
                {
                    upTaps = 0;
                }
            }
            #endregion

            //Check for input via the "Down" Keys.
            #region D-Pad Down Check
            if (LevelManager.instance.action_down.ReadValue<float>() > 0)
            {
                //Only take "Down" Input if the Up Key is not being held, and the Player is not currently barrel-rolling.
                if (upTimer == 0f && !barrelRolling)
                {
                    //Reset the "downReleased" timer.
                    downReleasedTimer = 0f;
                    //If the Down key wasn't held before this frame, increment the "tap" count.
                    if (downTimer == 0f)
                    {
                        //Set the Ship's Sprite to its "tilted down" pose.
                        GetComponent<SpriteRenderer>().sprite = spriteTiltedDown;
                        //Increase the tap count by one.
                        downTaps++;
                        //If the Down key has been double-tapped, start the Downward Barrel Roll.
                        if (downTaps == 2)
                        {
                            barrelRolling = true;
                            StartCoroutine(BarrelRoll(-1));
                            downTaps = 0;
                        }
                    }
                    //Increment the "Down" Timer, and reset the "tap" counter if the Key is being held (>.5 seconds).
                    downTimer += Time.deltaTime;
                    if (downTimer >= .5f)
                    {
                        downTaps = 0;
                    }
                    //Change the Player's velocity.
                    direction.y = -1;
                    //Only compensate for horizontal movement if the Player is not in a Boss fight.
                    //if (!LevelManager.instance.inBossFight)
                    //{
                    //    xNew += baseSpeed * Time.deltaTime * 2.5f * (1f / speedModifier);
                    //}
                    buttonPressed = true;
                }
            }
            //Reset the "Down" Timer if the key isn't being held.
            else
            {
                downTimer = 0f;
                //Set the Ship's Sprite back to its "neutral" pose.
                if (GetComponent<SpriteRenderer>().sprite == spriteTiltedDown)
                {
                    GetComponent<SpriteRenderer>().sprite = spriteNeutral;
                }
                //Increment the "downReleased" timer.
                downReleasedTimer += Time.deltaTime;
                //If the "Down" Key hasn't been pressed in .2 seconds, reset the Tap counter.
                if (downReleasedTimer >= .2f)
                {
                    downTaps = 0;
                }
            }
            #endregion

            //Reset "direction.y" if neither vertical key is pressed.
            if (LevelManager.instance.action_up.ReadValue<float>() == 0 && LevelManager.instance.action_down.ReadValue<float>() == 0 && !barrelRolling)
            {
                direction.y = 0;
            }

            //Check for input via the "Left" Keys.
            #region D-Pad Left Check
                if (LevelManager.instance.action_left.ReadValue<float>() > 0)
            {
                //Change the Player's velocity.
                direction.x = -1;
                buttonPressed = true;
                //Change the Ship's Flame Animation to account for the movement direction.
                UpdateFlames(speedSetting - 1);
            }
            else
            {
                //If there is no input, change the Flame setting back to normal.
                UpdateFlames(speedSetting);
            }
            #endregion

            //Check for input via the "Right" Keys.
            #region D-Pad Right Check
            if (LevelManager.instance.action_right.ReadValue<float>() > 0)
            {
                //Change the Player's velocity.
                direction.x = 1;
                buttonPressed = true;
                //Change the Ship's Flame Animation to account for the movement direction.
                UpdateFlames(speedSetting + 1);
            }
            else
            {
                //If there is no input, change the Flame setting back to normal.
                UpdateFlames(speedSetting);
            }
            #endregion

            //Reset "direction.x" if neither horizontal key is pressed.
            if (LevelManager.instance.action_left.ReadValue<float>() == 0 && LevelManager.instance.action_right.ReadValue<float>() == 0 && !barrelRolling)
            {
                direction.x = 0;
            }

            //Set the Ship's target direction based on the D-Pad inputs.
            SetTarget((Vector2)transform.position + direction);
            //Only update the Speed here if the Ship isn't Barrel Rolling (otherwise, "BarrelRoll" will manage the Speed.
            if (!barrelRolling)
            {
                //If there is an input, set the speed to move.
                if (direction.x != 0 || direction.y != 0)
                {
                    SetSpeed(baseSpeed * speedModifier);
                }
                //Otherwise, stop the ship.
                else
                {
                    SetSpeed(0);
                }
            }
        }
        //Make sure the Player's Ship hasn't gone off-screen, while its controls are enabled.
        if (controlsEnabled)
        {
            ClampPosition();
        }
        
        //Call Movable2's Update.
        base.Update();
    }

    ///Decelerate the Ship until it stops in the center of the screen.
    ///<param name="targetPos">The target x-position for the Ship.</param>
    public IEnumerator EnterFromLeft(float targetPos)
    {
        //Set the position from which the Ship will enter the screen.
        transform.position = new Vector3(LevelManager.instance.screenEdges.bounds.min.x - 40, transform.position.y, transform.position.z);
        //Create an offset value for the Ship's speed to control its deceleration over time.
        float accelOffset = 2f;
        //Send the Ship speeding back onto the screen.
        SetAngleInDegrees(0);
        SetSpeed(baseSpeed + accelOffset);
        //Continue at this speed until the Ship passes the 1/4th horizontal mark of the screen.
        yield return new WaitWhile(() => transform.position.x < targetPos);
        //Decelerate the Ship until it starts moving backwards and reaches the middle of the screen again.
        while (transform.position.x > targetPos)
        {
            accelOffset -= .06f;
            SetSpeed(baseSpeed + accelOffset);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Stop the Ship once its reached its target position.
        SetTarget(transform.position);
        SetSpeed(0);
        //Deactivate the Ship's "afterburner" trail.
        //jetFlames.GetComponent<TrailRenderer>().enabled = false;
        //Once the Ship has hit its mark, re-enable the Player's controls.
        LevelManager.instance.inCutscene = false;
        controlsEnabled = true;
    }

    //Accelerate the Ship until it passes the right side of the screen.
    public IEnumerator ExitToRight()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Activate the Ship's "afterburner" trail.
        //jetFlames.GetComponent<TrailRenderer>().enabled = true;
        //Send the Ship quickly past the right edge of the screen.
        SetAngleInDegrees(0);
        SetSpeed(exitingSpeed);
        //Continue until it passes the right edge of the screen.
        yield return new WaitWhile(() => transform.position.x < LevelManager.instance.screenEdges.bounds.max.x);
    }

    //Ensure that the Ship flies off the left edge of the Screen when shifting into the Boss fight.
    public IEnumerator BackAwayFromBossDoor()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Set the Ship moving to the left.
        SetAngleInDegrees(180);
        //Accelerate the Ship to the left until it passes offscreen.
        while ((transform.position.x + halfWidth) > LevelManager.instance.screenEdges.bounds.min.x)
        {
            //If the Game is paused, don't increase the Ship speed.
            if (!LevelManager.instance.gamePaused)
            {
                SetSpeed(GetSpeed() + .01f);
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Position the Ship just outside the Boss arena, and stop it in place.
        transform.position = new Vector3(LevelManager.instance.screenEdges.bounds.min.x - (halfWidth + 8), transform.position.y, transform.position.z);
        SetSpeed(0);
    }

    //Slow the Player to a stop when they get far enough into the Boss Room.
    public IEnumerator EnterBossRoom()
    {
        //Yield out of this Coroutine to let any other ones start.
        yield return new WaitForEndOfFrame();
        //Send the player to the right.
        SetAngleInDegrees(0);
        SetSpeed(baseSpeed);
        //Wait for the Player to reach the middle of the left side of the screen.
        yield return new WaitWhile(() => transform.position.x < LevelManager.instance.screenEdges.bounds.min.x + (LevelManager.instance.screenEdges.bounds.extents.x / 2));
        //Create a float to reduce the Player's speed as it enters the Boss Room.
        float speedDecay = 0;
        //Quickly slow the Ship down until it stops.
        while (speedDecay < baseSpeed)
        {
            //If the Game is paused, don't decay the Ship speed.
            if (!LevelManager.instance.gamePaused)
            {
                speedDecay += .01f;
                SetSpeed(baseSpeed - speedDecay);
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Zero out the Ship's speed in case "speedDecay" surpassed "baseSpeed".
        SetSpeed(0);
        //Wait for the Boss to finish its intro Animation before starting the fight.
        /*yield return new WaitForSeconds(3);
        LevelManager.instance.inCutscene = false;
        StartCoroutine(LevelManager.instance.BossFight());*/
    }

    //Set the Player's ship moving directly up (dir=1) or directly down (dir=-1).
    IEnumerator BarrelRoll(int dir)
    {
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
        //Change the Player's velocity.
        direction.x = 0;
        direction.y = dir;
        SetTarget((Vector2)transform.position + direction);
        barrelRollCurrentSpeedBoost = barrelRollMaxSpeedBoost;
        //Set up the Barrel Roll Timer variables.
        barrelRollDuration = barrelRollClip.length / Mathf.Abs(GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).speed);
        barrelRollTimer = 0;
        //Run this loop for the Duration of the Barrel Roll Animation.
        while (barrelRollTimer < barrelRollDuration)
        {
            //If the Game is paused, pause the Barrel Roll timing.
            if (!LevelManager.instance.gamePaused)
            {
                //First, check that the Player is still alive.
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
                //Increment the Barrel Roll Timer.
                barrelRollTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        barrelRolling = false;
        GetComponent<Animator>().enabled = false;
    }

    //Toggle the Animators of the Ship and its Children on or off.
    public void ToggleAnimations(bool active)
    {
        //Toggle the Animator for the Ship itself.
        GetComponent<Animator>().enabled = active;
        //Toggle the Animator for the Jet Flames.
        jetFlames.GetComponent<Animator>().enabled = active;
        //Toggle the Animator for the Shield.
        shield.GetComponent<Animator>().enabled = active;
        //Toggle the Animator for the Hitbox Light.
        hitboxLight.GetComponent<Animator>().enabled = active;
    }

    //Keep the Ship within the bounds of the Camera.
    private void ClampPosition()
    {
        //Check the Player Ship's left boundary.
        xOverlap = LevelManager.instance.screenEdges.bounds.min.x - (transform.position.x - halfWidth);
        if (xOverlap > 0) { transform.Translate(xOverlap, 0, 0); }
        //Check the Player Ship's right boundary.
        else
        { 
            xOverlap = LevelManager.instance.screenEdges.bounds.max.x - (transform.position.x + halfWidth);
            if (xOverlap < 0) { transform.Translate(xOverlap, 0, 0); }
        }
        //Check the Player Ship's top boundary.
        yOverlap = LevelManager.instance.screenEdges.bounds.min.y - (transform.position.y - halfHeight);
        if (yOverlap > 0) { transform.Translate(0, yOverlap, 0); }
        //Check the Player Ship's bottom boundary.
        else
        {
            yOverlap = LevelManager.instance.screenEdges.bounds.max.y - (transform.position.y + halfHeight);
            if (yOverlap < 0) { transform.Translate(0, yOverlap, 0); }
        }
    }

    //Cycle between the four Speed settings.
    void SpeedChange()
    {
        //Increase the Speed Setting by 1.
        speedModifier += .5f;
        speedSetting += 1;
        //If the Speed Setting has exceeded 4, reset it back to 1.
        if (speedSetting > 4)
        {
            speedModifier = .5f;
            speedSetting = 1;
        }
        //Change the Ship's Flame Animation to match the new Speed Setting.
        UpdateFlames(speedSetting);
        //Update the HUD's Speed Bar.
        UIManager.instance.UpdateSpeedDisplay(speedSetting);
    }

    //Start the Animation for the Ship's flames based on speed and movement direction.
    void UpdateFlames(int newFlameState)
    {
        //Check that the Animator is not already on the Animation it is being told to change to.
        if (jetFlames.GetComponent<Animator>().GetInteger("setFlameNumber") != newFlameState)
        {
            //Change to the new Animation State, which will start the next Animation cycle.
            jetFlames.GetComponent<Animator>().SetInteger("setFlameNumber", newFlameState);
        }
    }

    //Drop a Bomb that destroys every Enemy on screen (and damages any Boss on screen)
    void UseBomb()
    {
        //Prevent the Player from wasting a Bomb while they are in a Cutscene.
        if (!LevelManager.instance.inCutscene)
        {
            //Check that the GameManager is loaded. Otherwise, LevelManager's variables will be used.
            try
            {
                //Check that the Player has a Bomb in reserve (or has "Infinite Bombs" enabled.
                if (GameManager.instance.bombs >= 1 || GameManager.instance.cheat_infiniteBombsEnabled)
                {
                    //Reduce the number of Bombs by one.
                    GameManager.instance.bombs--;
                    //Update the HUD's Bomb counter.
                    LevelManager.instance.UpdateBombs();
                    //Blow up every susceptible Enemy on the screen, or damage the Boss.
                    LevelManager.instance.DropBomb();
                }
                //If the player has an impossible number of Bombs (-1 or lower), they used a Cheat and
                //their Achievements should be disabled until the game is reset, or the cheat is disabled.
                if (GameManager.instance.bombs < 0 && GameManager.instance.cheat_infiniteBombsEnabled)
                {
                    GameManager.instance.achievementsEnabled = false;
                }
            }
            //Use LevelManager's local variables if the GameManager isn't loaded.
            catch (System.NullReferenceException)
            {
                //Check that the Player has a Bomb in reserve.
                if (LevelManager.instance.bombs >= 1)
                {
                    //Reduce the number of Bombs by one.
                    LevelManager.instance.bombs--;
                    //Update the HUD's Bomb counter.
                    LevelManager.instance.UpdateBombs();
                    //Blow up every susceptible Enemy on the screen, or damage the Boss.
                    LevelManager.instance.DropBomb();
                }
            }
        }
    }

    //Fire a projectile when the Player's weapon is not in cooldown.
    void Shoot()
    {
        //Check that the Player's weapon is not in "cooldown" mode, and that the Game is not in a Cutscene.
        if (!inCooldown && !LevelManager.instance.inCutscene)
        {
            //Put the Player's weapon in "cooldown" mode.
            inCooldown = true;
            //Fire the Player's weapon, then track their "rate-of-fire" timer.
            StartCoroutine(FiringCooldown());
        }
    }

    //Change/Improve the Player's current weapon when they pick up an Item.
    public void ChangeWeapon(int newWeaponID)
    {
        //If the Player picked up a different weapon than the one they have equipped, switch to it.
        if (weaponIndex != newWeaponID)
        {
            //Update the Weapon Index to match the ID of the new Weapon.
            weaponIndex = newWeaponID;
            //Change the Player's Bullet prefab to that of the new Weapon.
            playerBullet = weapons[weaponIndex];
            //Change the Player's rate-of-fire to fit the new Weapon.
            ChangeRateOfFire();
            //Switch to the new Hitbox color.
            hitboxLight.GetComponent<Animator>().SetInteger("setHitboxColor", weaponIndex);
            hitboxLight.GetComponent<Animator>().SetTrigger("changingWeapon");
        }
        //If the Player picked up an Item for their current weapon, improve it.
        else
        {
            //If this weapon has not reached its max tier, upgrade it.
            if (weaponLevels[weaponIndex] < 4)
            {
                weaponLevels[weaponIndex]++;
            }
            //Otherwise, give the Player a point bonus.
            else
            {
                LevelManager.instance.UpdateScores(10000);
            }
        }
        //Reset the Player's "firingLightning" tag, to allow them to fire the Lightning at increased power, if applicable.
        firingLightning = false;
        LevelManager.instance.ClearLightning();
        //Update the HUD's Power Bar to match the current Weapon Power.
        UIManager.instance.UpdatePowerDisplay(weaponLevels[weaponIndex]);
    }

    /// <summary>
    /// Change the Player's Rate-of-Fire, based on the new Weapon they have equipped.
    /// </summary>
    private void ChangeRateOfFire()
    {
        switch (weaponIndex)
        {
            //Weapon 0: Base Gun
            //Fires 20 Bullets/second.
            case 0:
                rateOfFire = 1f / 20f;
                break;
            //Weapon 1: Pierce Laser
            //Fires 5 Lasers/second.
            case 1:
                rateOfFire = 1f / 5f;
                break;
            //Weapon 2: Bolo Gun (TBD)
            //Fires 4 Bolos/second.
            case 2:
                rateOfFire = 1f / 4f;
                break;
            //Weapon 3: Boomerang
            //Fires 3 Boomerangs/second
            case 3:
                rateOfFire = 1f / 3f;
                break;
            //Weapon 4: Lightning
            //Fires 15 Bolts/second.
            case 4:
                rateOfFire = 1f / 15f;
                break;
            //Weapon 5: Flak Gun (TBD)
            //Fires 2 Bursts/second.
            case 5:
                rateOfFire = 1f / 2f;
                break;
            default:
                break;
        }
    }

    //Checks for collision against Enemies and Enemy Bullets.
    //Collision checks against Item Objects happens in "Item.cs".
    void OnTriggerEnter2D(Collider2D collider)
    {
        //Do this only if the Collider belongs to an Enemy.
        if (collider.gameObject.tag == "Enemy")
        {
            //Check that the Player is alive and not invincible, and that the Enemy object isn't immune to damage.
            if (isAlive && !isInvincible && !collider.gameObject.GetComponent<EnemyController>().dmgImmune)
            {
                //Kill the Enemy that the Player collided with (by subtracting all of their current HP).
                collider.gameObject.GetComponent<EnemyController>().TakeDamage(collider.gameObject.GetComponent<EnemyController>().hp);
            }
        }
        //Do this only if the Collider belongs to an Enemy Bullet.
        if (collider.gameObject.tag == "EnemyBullet")
        {
            //Check that the Player is alive and not invincible.
            if (isAlive && !isInvincible)
            {
                //Remove this Bullet from the game (to prevent it from colliding with anything else).
                LevelManager.instance.RemoveBulletFromList(collider.gameObject);
            }
        }
        //Check if the Collider belongs to either an Enemy, an EnemyBullet, or a damaging Explosion.
        if (collider.gameObject.tag == "Enemy" || collider.gameObject.tag == "EnemyBullet" || collider.gameObject.tag == "Explosion" && collider.gameObject.GetComponent<Explosion>().isDamaging)
        {
            //Check that the Player is alive and not invincible.
            if (isAlive && !isInvincible)
            {
                //If the Player does not have a Shield, they die.
                if (!shieldActive)
                {
                    StartCoroutine(Die());
                }
                //Otherwise, they simply lose their Shield.
                else
                {
                    //Deactivate the Shield.
                    shieldActive = false;
                    //Play the Shield's "pop" animation.
                    shield.GetComponent<Animator>().SetTrigger("pop");
                    //Let the Player be invincible for 5 seconds.
                    StartCoroutine(Invincibility());
                    //Set the Shield to disappear when the animation is complete.
                    Invoke("PopShield", shieldPopClip.length);
                }
            }
        }
    }

    //Deactivate the Shield child object.
    private void PopShield()
    {
        shield.SetActive(false);
    }

    //Check the Face Buttons for Button Config 1.
    IEnumerator CheckButtons1()
    {
        //This check will occur constantly throughout gameplay.
        while (true)
        {
            //Only take Player input if the Player is alive, and the Game isn't paused.
            if (isAlive && !LevelManager.instance.gamePaused)
            {
                //If the "A Button" is pressed, change the Ship's speed.
                if (LevelManager.instance.action_A.triggered) { SpeedChange(); }
                //If the "B Button" is pressed, try to use a Bomb.
                if (LevelManager.instance.action_B.triggered) { UseBomb(); }
                //If the "C Button" is held down, try to fire a projectile.
                if (LevelManager.instance.action_C.ReadValue<float>() > 0) { Shoot(); }
                //If the "C Button" is released, turn off the "firingLightning" flag (even if the current weapon isn't Lightning).
                LevelManager.instance.action_C.canceled += context => { firingLightning = false; LevelManager.instance.ClearLightning(); };
            }
            yield return new WaitForSeconds(0);
        }
    }

    //Check the Face Buttons for Button Config 2.
    IEnumerator CheckButtons2()
    {
        //This check will occur constantly throughout gameplay.
        while (true)
        {
            //Only take Player input if the Player is alive, and the Game isn't paused.
            if (isAlive && !LevelManager.instance.gamePaused)
            {
                //If the "A Button" is pressed, change the Ship's speed.
                if (LevelManager.instance.action_A.triggered) { SpeedChange(); }
                //If the "B Button" is held down, try to fire a projectile.
                if (LevelManager.instance.action_B.ReadValue<float>() > 0) { Shoot(); }
                //If the "B Button" is released, turn off the "firingLightning" flag (even if the current weapon isn't Lightning).
                LevelManager.instance.action_B.canceled += context => { firingLightning = false; LevelManager.instance.ClearLightning(); };
                //If the "C Button" is pressed, try to use a Bomb.
                if (LevelManager.instance.action_C.triggered) { UseBomb(); }
            }
            yield return new WaitForSeconds(0);
        }
    }

    //Check the Face Buttons for Button Config 3.
    IEnumerator CheckButtons3()
    {
        //This check will occur constantly throughout gameplay.
        while (true)
        {
            //Only take Player input if the Player is alive, and the Game isn't paused.
            if (isAlive && !LevelManager.instance.gamePaused)
            {
                //If the "A Button" is held down, try to fire a projectile.
                if (LevelManager.instance.action_A.ReadValue<float>() > 0) { Shoot(); }
                //If the "A Button" is released, turn off the "firingLightning" flag (even if the current weapon isn't Lightning).
                LevelManager.instance.action_A.canceled += context => { firingLightning = false; LevelManager.instance.ClearLightning(); };
                //If the "B Button" is pressed, change the Ship's speed.
                if (LevelManager.instance.action_B.triggered) { SpeedChange(); }
                //If the "C Button" is pressed, try to use a Bomb.
                if (LevelManager.instance.action_C.triggered) { UseBomb(); }
            }
            yield return new WaitForSeconds(0);
        }
    }

    //Check the Face Buttons for Button Config 4.
    IEnumerator CheckButtons4()
    {
        //This check will occur constantly throughout gameplay.
        while (true)
        {
            //Only take Player input if the Player is alive, and the Game isn't paused.
            if (isAlive && !LevelManager.instance.gamePaused)
            {
                //If the "A Button" is held down, try to fire a projectile.
                if (LevelManager.instance.action_A.ReadValue<float>() > 0) { Shoot(); }
                //If the "A Button" is released, turn off the "firingLightning" flag (even if the current weapon isn't Lightning).
                LevelManager.instance.action_A.canceled += context => { firingLightning = false; LevelManager.instance.ClearLightning(); };
                //If the "B Button" is pressed, try to use a Bomb.
                if (LevelManager.instance.action_B.triggered) { UseBomb(); }
                //If the "C Button" is pressed, change the Ship's speed.
                if (LevelManager.instance.action_C.triggered) { SpeedChange(); }
            }
            yield return new WaitForSeconds(0);
        }
    }

    //Check the Face Buttons for Button Config 5.
    IEnumerator CheckButtons5()
    {
        //This check will occur constantly throughout gameplay.
        while (true)
        {
            //Only take Player input if the Player is alive, and the Game isn't paused.
            if (isAlive && !LevelManager.instance.gamePaused)
            {
                //If the "A Button" is pressed, try to use a Bomb.
                if (LevelManager.instance.action_A.triggered) { UseBomb(); }
                //If the "B Button" is pressed, change the Ship's speed.
                if (LevelManager.instance.action_B.triggered) { SpeedChange(); }
                //If the "C Button" is held down, try to fire a projectile.
                if (LevelManager.instance.action_C.ReadValue<float>() > 0) { Shoot(); }
                //If the "C Button" is released, turn off the "firingLightning" flag (even if the current weapon isn't Lightning).
                LevelManager.instance.action_C.canceled += context => { firingLightning = false; LevelManager.instance.ClearLightning(); };
            }
            yield return new WaitForSeconds(0);
        }
    }

    //Check the Face Buttons for Button Config 6.
    IEnumerator CheckButtons6()
    {
        //This check will occur constantly throughout gameplay.
        while (true)
        {
            //Only take Player input if the Player is alive, and the Game isn't paused.
            if (isAlive && !LevelManager.instance.gamePaused)
            {
                //If the "A Button" is pressed, try to use a Bomb.
                if (LevelManager.instance.action_A.triggered) { UseBomb(); }
                //If the "B Button" is held down, try to fire a projectile.
                if (LevelManager.instance.action_B.ReadValue<float>() > 0) { Shoot(); }
                //If the "B Button" is released, turn off the "firingLightning" flag (even if the current weapon isn't Lightning).
                LevelManager.instance.action_B.canceled += context => { firingLightning = false; LevelManager.instance.ClearLightning(); };
                //If the "C Button" is pressed, change the Ship's speed.
                if (LevelManager.instance.action_C.triggered) { SpeedChange(); }
            }
            yield return new WaitForSeconds(0);
        }
    }

    //Have the Player lose a Life, and either respawn them (if they have a spare Life) or trigger the Game Over screen (if they don't).
    IEnumerator Die()
    {
        //Set the Player to "dead" (so they don't "collide" with anything else before respawning).
        isAlive = false;
        //Set the Player's "firingLightning" flag to false, in case they were firing at time of death.
        firingLightning = false;
        LevelManager.instance.ClearLightning();
        //Make the Player's Sprite invisible.
        this.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        jetFlames.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        hitboxLight.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0f);
        //Toggle the flag for the "No Miss" bonus in LevelManager.
        LevelManager.instance.lifeUsed = true;
        //Wait 3 seconds (respawn time) before deciding what to do next.
        //Reset the Death Timer.
        deathTimer = 0f;
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
        //Check if the Title Screen (and by extension, the GameManager) has been loaded.
        if (GameManager.instance != null)
        {
            //Check if the Player has a spare Life (or has "Infinite Lives" enabled).
            if (GameManager.instance.lives > 1 || GameManager.instance.cheat_infiniteLivesEnabled)
            {
                //Reduce the number of Lives by one.
                GameManager.instance.lives--;
                //Update the HUD's Life counter.
                LevelManager.instance.UpdateLives();
                //Set the Player back to "alive" (so they can move/perform actions/pick up Items).
                isAlive = true;
                //Give the Player their Invincibility grace period after respawning.
                StartCoroutine(Invincibility());
            }
            //If they don't, trigger the "Game Over" state.
            else
            {
                StartCoroutine(LevelManager.instance.GameOver());
            }
            //If the player has an impossible number of Lives (0 or lower), they used a Cheat and
            //their Achievements should be disabled until the game is reset, or the cheat is disabled.
            if (GameManager.instance.lives < 1 && GameManager.instance.cheat_infiniteLivesEnabled)
            {
                GameManager.instance.achievementsEnabled = false;
            }
        }
        //Use LevelManager's local variables if the GameManager isn't loaded.
        else
        {
            if (LevelManager.instance.lives > 1)
            {
                //Reduce the number of Lives by one.
                LevelManager.instance.lives--;
                //Update the HUD's Life counter.
                LevelManager.instance.UpdateLives();
                //Set the Player back to "alive" (so they can move/perform actions/pick up Items).
                isAlive = true;
                //Give the Player their Invincibility grace period after respawning.
                StartCoroutine(Invincibility());
            }
            //If they don't, trigger the "Game Over" state.
            else
            {
                StartCoroutine(LevelManager.instance.GameOver());
            }
        }
    }

    //When the Ship is rendered invincible, make it half-visible for 5 seconds, then restore alpha to full.
    public IEnumerator Invincibility()
    {
        //Make the Player immune to damage.
        isInvincible = true;
        //Make the Player's Sprite semi-transparent.
        this.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, .5f);
        jetFlames.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, .5f);
        hitboxLight.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, .5f);
        //Give the Player five seconds of invincibility.
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
        //Make the Player's Sprite fully opaque.
        this.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        jetFlames.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        hitboxLight.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 1f);
        //Make the Player susceptible to damage again.
        isInvincible = false;
    }

    //Create a new instance of the Player's current projectile, then wait briefly before creating another.
    IEnumerator FiringCooldown()
    {
        //If the Player is not currently firing Lightning (or doesn't have Lightning equipped), allow them to fire.
        if (weaponIndex != 4 || !firingLightning)
        {
            //Add a newly-created instance of the Player's Bullet to the LevelManager's list, to use in LevelManager's collision checks.
            LevelManager.instance.AddBulletToList(CreateBullet());
        }
        //Increment the "shotsFired" counter by one (used in Accuracy calculations for "End-of-Stage" bonuses).
        LevelManager.instance.shotsFired++;
        //Wait for a brief period of time before letting the Player fire another Bullet.
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
        //Take the Player's Weapon out of "cooldown" mode when the timer expires.
        inCooldown = false;
    }

    //Create and change the target of any Bullet(s) as dictated by current weapon/power level.
    private GameObject CreateBullet()
    {
        //Create a "bullet" object that will become a copy of the Player's "Master" Bullet.
        GameObject bullet;
        switch(weaponIndex)
        {
            //Weapon 0: The Base Gun
            case 0:
                //Weapon Level 1: Create the 1st Bullet directly in front of the Player.
                bullet = Instantiate(playerBullet, transform.position + Vector3.forward, Quaternion.identity) as GameObject;
                bullet.GetComponent<Bullet>().SetAngleInDegrees(0);
                bullet.GetComponent<Bullet>().SetSpeed(bulletSpeed_baseGun);
                //Weapon Level 2: Create a 2nd Bullet .05 above the Player.
                if (weaponLevels[0] >= 2)
                {
                    //Create a 2nd Bullet copy, and add it to LevelManager's list here directly.
                    GameObject bullet2 = Instantiate(playerBullet, transform.position + new Vector3(0, .05f, 1f), Quaternion.identity) as GameObject;
                    bullet2.GetComponent<Bullet>().SetAngleInDegrees(0);
                    bullet2.GetComponent<Bullet>().SetSpeed(bulletSpeed_baseGun);
                    LevelManager.instance.AddBulletToList(bullet2);
                    //Increment the "shotsFired" counter by one (used in Accuracy calculations for "End-of-Stage" bonuses).
                    LevelManager.instance.shotsFired++;
                    //Weapon Level 3: Create a 3rd Bullet .05 below the Player.
                    if (weaponLevels[0] >= 3)
                    {
                        //Create a 3rd Bullet copy, and add it to LevelManager's list here directly.
                        GameObject bullet3 = Instantiate(playerBullet, transform.position + new Vector3(0, -.05f, 1f), Quaternion.identity) as GameObject;
                        bullet3.GetComponent<Bullet>().SetAngleInDegrees(0);
                        bullet3.GetComponent<Bullet>().SetSpeed(bulletSpeed_baseGun);
                        LevelManager.instance.AddBulletToList(bullet3);
                        //Increment the "shotsFired" counter by one (used in Accuracy calculations for "End-of-Stage" bonuses).
                        LevelManager.instance.shotsFired++;
                    }
                }
                //Return the 1st copy of the Bullet to "FiringCooldown".
                return bullet;
            //Weapon 1: The Pierce Laser
            case 1:
                bullet = Instantiate(playerBullet, transform.position + Vector3.forward, Quaternion.identity) as GameObject;
                //Set the Laser moving to the right at a decent speed.
                bullet.GetComponent<Bullet>().SetAngleInDegrees(0);
                bullet.GetComponent<Bullet>().SetSpeed(bulletSpeed_pierceLaser);
                //Set the Laser's damage equal to its Power Level times 3.
                bullet.GetComponent<Bullet>().dmgValue = 3 * weaponLevels[1];
                return bullet;
            //Weapon 3: The Boomerang
            case 3:
                switch (weaponLevels[3])
                {
                    //Weapon Level 1: Create a slow-moving (and by extension, short-range) copy of the Boomerang.
                    case 1:
                        bullet = Instantiate(playerBullet, transform.position + Vector3.forward, Quaternion.identity) as GameObject;
                        bullet.GetComponent<Bullet>().SetAngleInDegrees(0);
                        bullet.GetComponent<Bullet>().SetSpeed(bulletSpeed_boomerang1);
                        return bullet;
                    //Weapon Level 2: Create a moderate-moving (and by extension, mid-range) copy of the Boomerang.
                    case 2:
                        bullet = Instantiate(playerBullet, transform.position + Vector3.forward, Quaternion.identity) as GameObject;
                        bullet.GetComponent<Bullet>().SetAngleInDegrees(0);
                        bullet.GetComponent<Bullet>().SetSpeed(bulletSpeed_boomerang2);
                        return bullet;
                    //Weapon Level 3: Create a fast-moving (and by extension, long-range) copy of the Boomerang.
                    case 3:
                        bullet = Instantiate(playerBullet, transform.position + Vector3.forward, Quaternion.identity) as GameObject;
                        bullet.GetComponent<Bullet>().SetAngleInDegrees(0);
                        bullet.GetComponent<Bullet>().SetSpeed(bulletSpeed_boomerang3);
                        return bullet;
                    //Default Weapon Level (shouldn't be reached):
                    default:
                        bullet = Instantiate(playerBullet, transform.position + Vector3.forward, Quaternion.identity) as GameObject;
                        bullet.GetComponent<Bullet>().SetAngleInDegrees(0);
                        bullet.GetComponent<Bullet>().SetSpeed(bulletSpeed_boomerang1);
                        return bullet;
                }

            //Weapon 4: The Lightning
            case 4:
                //Weapon Level 1: Create a bolt of Lightning that, when first fired, will hone in on the closest Enemy.
                playerBullet.GetComponent<Bullet>().SetTarget(transform.position);
                playerBullet.GetComponent<Bullet>().SetSpeed(5f);
                bullet = Instantiate(playerBullet, transform.position + Vector3.forward, Quaternion.identity) as GameObject;
                //Weapon Level 1: Create 1 Arc that can attack any Enemy.
                if (weaponLevels[4] == 1)
                {
                    bullet.GetComponent<Lightning>().SetDirection(0);
                }
                //Weapon Level 2: Create 2 Arcs, 1 which attacks an Enemy above the Player, and 1 which attacks an Enemy below the Player.
                else if (weaponLevels[4] == 2)
                {
                    bullet.GetComponent<Lightning>().SetDirection(-1);
                    GameObject bullet2 = Instantiate(playerBullet, transform.position + Vector3.forward, Quaternion.identity) as GameObject;
                    bullet2.GetComponent<Lightning>().SetDirection(1);
                    LevelManager.instance.AddBulletToList(bullet2);
                    LevelManager.instance.shotsFired++;
                }
                //Weapon Level 3: Create 3 Arcs, 1 which attacks an Enemy above the Player, 1 which attacks an Enemy below the Player, and 1 which can attack any Enemy.
                else if (weaponLevels[4] == 3)
                {
                    bullet.GetComponent<Lightning>().SetDirection(0);
                    GameObject bullet2 = Instantiate(playerBullet, transform.position + Vector3.forward, Quaternion.identity) as GameObject;
                    bullet2.GetComponent<Lightning>().SetDirection(1);
                    LevelManager.instance.AddBulletToList(bullet2);
                    LevelManager.instance.shotsFired++;
                    GameObject bullet3 = Instantiate(playerBullet, transform.position + Vector3.forward, Quaternion.identity) as GameObject;
                    bullet3.GetComponent<Lightning>().SetDirection(-1);
                    LevelManager.instance.AddBulletToList(bullet3);
                    LevelManager.instance.shotsFired++;
                }
                //Prevent more Lightning Arcs from being created while the Player is using this one.
                firingLightning = true;
                return bullet;
            //Default value (shouldn't ever be reached)
            default:
                playerBullet.GetComponent<Bullet>().SetAngleInDegrees(0);
                playerBullet.GetComponent<Bullet>().SetSpeed(3f);
                return Instantiate(playerBullet, transform.position + Vector3.forward, Quaternion.identity) as GameObject;
        }
    }
}
