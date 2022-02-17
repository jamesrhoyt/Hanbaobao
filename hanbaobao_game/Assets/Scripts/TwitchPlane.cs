/*
 * TwitchPlane.cs
 * 
 * A small Enemy that moves quickly in one of eight directions,
 * then changes its direction frequently.
 * 
 */

using UnityEngine;
using System.Collections;

public class TwitchPlane : EnemyController
{
    private int angleIndex;         //The digit (0-7) that represents the angle at which the Plane will move next.
    private int angleIncrementer;   //How much to increment the angleIndex (changes every movement cycle).
    private bool onScreen;          //Whether the Plane is currently onscreen.
    private float cooldownTimer;    //The amount of time elapsed since the last change in direction.
    private float damageTimer;      //The amount of time that the Twitch Plane has "flashed" invincible.

    //"Flash" Variables:
    private SpriteRenderer twitchRenderer;  //The Sprite Renderer attached to this GameObject.
    private Shader shaderGUIText;           //A Text Shader, used to turn the TwitchPlane solid white, even during its Animations.
    private Shader shaderSpritesDefault;    //The GameObject's default Shader, used to restore it to its original color.

	// Use this for initialization
	protected override void Start()
    {
        //Call EnemyController's Start.
        base.Start();
        //Overwrite the EnemyController default values.
        hp = 3;
        SetSpeed(1.0f);
        scoreValue = 50;
        rateOfFire = .45f;

        //Initialize the Twitch Plane's movement variables.
        angleIndex = 0;
        cooldownTimer = 0f;
        onScreen = false;
        //Initialize the objects used for the Twitch Plane's "Hit Flash" effect.
        twitchRenderer = gameObject.GetComponent<SpriteRenderer>();
        shaderGUIText = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");
	}

    //Activate the Twitch Plane's behaviors when it appears on screen.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox") && onScreen == false)
        {
            onScreen = true;
            LevelManager.instance.AddEnemyToList(gameObject);
            //Have the Twitch Plane start moving.
            StartCoroutine(MovementCooldown());
        }
        //Otherwise, check if this is a Player-controlled Bullet. 
        else if (box.gameObject.CompareTag("PlayerBullet"))
        {
            //If it is, make the Twitch Plane "flash" and make it invincible temporarily.
            if (hp > 0) { StartCoroutine(DamageCooldown()); }
            //Have the Twitch Plane take damage.
            if (!dmgImmune) { TakeDamage(box.gameObject.GetComponent<Bullet>().dmgValue); }
        }
        //Otherwise, check if this is an Explosion.
        else if (box.gameObject.CompareTag("Explosion"))
        {
            //Check if this explosion does damage.
            if (box.gameObject.GetComponent<Explosion>().isDamaging)
            {
                //If it is, make the Disc "flash" and make it invincible temporarily.
                if (hp > 0) { StartCoroutine(DamageCooldown()); }
                //Have the Disc take damage.
                if (!dmgImmune) { TakeDamage(box.gameObject.GetComponent<Explosion>().dmgValue); }
            }
        }
    }

    //Despawn the Twitch Plane when it passes outside of the screen, and is behind the Player.
    void OnTriggerExit2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view, and that the Player is to the right of the Twitch Plane.
        if (box.gameObject.CompareTag("ScreenBox") && GameObject.FindGameObjectWithTag("Player").transform.position.x > transform.position.x)
        {
            onScreen = false;
            LevelManager.instance.RemoveEnemyFromList(gameObject);
        }
    }

    /// <summary>
    /// Determine which of the eight directions the Twitch Plane will start traveling in now.
    /// </summary>
    /// <param name="seed">The base value to determine the Plane's new movement angle.</param>
    private void GetNewAngle(int seed)
    {
        //Set the seed value for the built-in Random function.
        Random.InitState(seed);
        //Get a random value to increment the Plane's movement angle by.
        angleIncrementer = Random.Range(1, 8);
        //Add the incrementation to the angle's representative index.
        angleIndex += angleIncrementer;
        //Mod the new index to fit the direction options.
        angleIndex %= 8;
        //Rotate the Sprite to match the new direction.
        transform.Rotate(0, 0, angleIncrementer * 45.0f);
    }

    //Change the Twitch Plane's movement direction roughly every half-second.
    IEnumerator MovementCooldown()
    {
        while (hp > 0)
        {
            //Get the Plane's new trajectory angle, based off its current index, the last incrementation value, and its x-position.
            GetNewAngle(angleIndex * ((int)transform.position.x * 50) * angleIncrementer);
            //Set the new movement target, based off the new x- and y-destination values.
            //smoothMove(transform.position.x + xDest, transform.position.y + yDest, speed);
            SetAngleInDegrees(angleIndex * 45f);
            //Wait a small amount of time before changing direction again.
            //Reset the Cooldown Timer.
            cooldownTimer = 0f;
            //Have the Timer run for the "rateOfFire" in seconds before continuing.
            while (cooldownTimer < rateOfFire)
            {
                //If the Game is paused, don't update the Cooldown Timer.
                if (!LevelManager.instance.gamePaused)
                {
                    cooldownTimer += Time.deltaTime;
                }
                yield return new WaitForSeconds(0);
            }
        }
    }

    //Turn the Twitch Plane white, and make it invincible, for a very short period of time.
    IEnumerator DamageCooldown()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Make the Twitch Plane immune to damage while it is flashing.
        dmgImmune = true;
        //Make the Twitch Plane solid white to show that it has been hit.
        twitchRenderer.material.shader = shaderGUIText;
        twitchRenderer.color = Color.white;
        //Reset the Damage Timer.
        damageTimer = 0;
        //Let the Twitch Plane be invincible for 1/30 of a second.
        while (damageTimer < Time.deltaTime * 2)
        {
            //If the Game is paused, don't update the Damage Timer.
            if (!LevelManager.instance.gamePaused)
            {
                damageTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(0);
        }
        //Let the Twitch Plane be susceptible to damage again.
        dmgImmune = false;
        //Change the Shader back to the default.
        twitchRenderer.material.shader = shaderSpritesDefault;
        twitchRenderer.color = Color.white;
    }

	// Update is called once per frame
	protected override void Update()
    {
        //Call EnemyController's Update.
        base.Update();
	}
}
