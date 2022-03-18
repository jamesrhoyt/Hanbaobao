/*
 * Cube.cs
 * 
 * A stationary Cube that fires a single Bullet toward the Player, 
 * then plays a "rotation" animation that turns it to another side.
 * 
 */

using UnityEngine;
using System.Collections;

public class Cube : EnemyController
{
    public GameObject cubeBullet;
    private Vector3 bulletTarget;   //The location of the Player when the Cube is ready to fire.
    private float bulletSpeed;      //The speed of the Cube's Bullets.
    private float cooldownTimer;    //The amount of time elapsed since the last Bullet fired.
    private float damageTimer;      //The amount of time that the Cube has "flashed" invincible.

    //"Flash" Variables:
    private SpriteRenderer cubeRenderer;    //The Sprite Renderer attached to this GameObject.
    private Shader shaderGUIText;           //A Text Shader, used to turn the Cube solid white, even during its Animations.
    private Shader shaderSpritesDefault;    //The GameObject's default Shader, used to restore it to its original color.

	// Use this for initialization
	protected override void Start()
    {
        //Call EnemyController's Start.
        base.Start();
        //Overwrite the EnemyController default values.
        hp = 15;
        SetSpeed(0f);
        bulletSpeed = .4f;
        scoreValue = 150;
        rateOfFire = 2f;
        cooldownTimer = 0f;
        //Initialize the objects used for the Cube's "Hit Flash" effect.
        cubeRenderer = gameObject.GetComponent<SpriteRenderer>();
        shaderGUIText = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");
	}

    //Activate the Cube's behaviors when it appears on screen.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.AddEnemyToList(gameObject);
            //Have the Cube start firing.
            StartCoroutine(FiringCooldown());
        }
        //Otherwise, check if this is a Player-controlled Bullet. 
        else if (box.gameObject.CompareTag("PlayerBullet"))
        {
            //If it is, make the Cube "flash" and make it invincible temporarily.
            if (hp > 0) { StartCoroutine(DamageCooldown()); }
            //Have the Cube take damage.
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

    //Despawn the Cube when it passes outside of the screen.
    void OnTriggerExit2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.RemoveEnemyFromList(gameObject);
        }
    }

    //Fire a Bullet from the center of the Cube, toward the Player.
    IEnumerator FiringCooldown()
    {
        while (hp > 0)
        {
            //Get the Player's location to use as the target.
            bulletTarget = GameObject.FindGameObjectWithTag("Player").transform.position;
            //Create an instance of the Bullet that will appear in front of the Cube (on the z-axis).
            GameObject bullet = Instantiate(cubeBullet, transform.position + Vector3.back, Quaternion.identity);
            //Add the Bullet to the LevelManager's list.
            LevelManager.instance.AddBulletToList(bullet);
            //Assign the Bullet's target and speed.
            //cubeBullet.GetComponent<Bullet>().ChangeTarget(target, 75f);
            bullet.GetComponent<Bullet>().SetTarget(bulletTarget);
            bullet.GetComponent<Bullet>().SetSpeed(bulletSpeed);
            //Wait for .5 seconds before playing the Cube's "rotation" animation.
            //Reset the Cooldown Timer.
            cooldownTimer = 0;
            //Have the Timer run for .5 seconds before continuing.
            while (cooldownTimer < .5)
            {
                //If the Game is paused, don't update the Cooldown Timer.
                if (!LevelManager.instance.gamePaused)
                {
                    cooldownTimer += Time.deltaTime;
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //Reset the animation trigger to play the "rotation" again.
            GetComponent<Animator>().SetTrigger("deactivate");
            //Play the animation to show the Cube rotating to the left.
            GetComponent<Animator>().SetTrigger("activate");
            //Wait for the remaining "rate-of-fire" time before firing again.
            //Reset the Cooldown Timer.
            cooldownTimer = 0;
            //Have the Timer run for the remainder time before continuing.
            while (cooldownTimer < rateOfFire - .5)
            {
                //If the Game is paused, don't update the Cooldown Timer.
                if (!LevelManager.instance.gamePaused)
                {
                    cooldownTimer += Time.deltaTime;
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
    }

    //Turn the Cube white, and make it invincible, for a very short period of time.
    IEnumerator DamageCooldown()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Make the Cube immune to damage while it is flashing.
        dmgImmune = true;
        //Make the Cube solid white to show that it has been hit.
        cubeRenderer.material.shader = shaderGUIText;
        cubeRenderer.color = Color.white;
        //Reset the Damage Timer.
        damageTimer = 0;
        //Let the Cube be invincible for 1/30 of a second.
        while (damageTimer < Time.deltaTime * 2)
        {
            //If the Game is paused, don't update the Damage Timer.
            if (!LevelManager.instance.gamePaused)
            {
                damageTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Let the Cube be susceptible to damage again.
        dmgImmune = false;
        //Change the Shader back to the default.
        cubeRenderer.material.shader = shaderSpritesDefault;
        cubeRenderer.color = Color.white;
    }

	// Update is called once per frame
	protected override void Update()
    {
        //Call EnemyController's Update.
        base.Update();
	}
}
