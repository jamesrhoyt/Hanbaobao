/*
 * Wheel.cs
 * 
 * A stationary Enemy that rotates slowly and fires four streams of slow-moving bullets.
 * 
 * 
 */

using UnityEngine;
using System.Collections;

public class Wheel : EnemyController
{
    public GameObject wheelBullet;  //The "Master Copy" of the Bullet that the Wheel will fire.
    private float bulletSpeed;      //The speed of the Wheel's Bullets.
    private float firingAngle;      //The base angle for each instance of the Bullet, based on the Wheel's z-rotation.
    private float cooldownTimer;    //The amount of time elapsed since the last Bullet fired.
    private float damageTimer;      //The amount of time that the Wheel has "flashed" invincible.

    //"Flash" Variables:
    private SpriteRenderer wheelRenderer;   //The Sprite Renderer attached to this GameObject.
    private Shader shaderGUIText;           //A Text Shader, used to turn the Wheel solid white, even during its Animations.
    private Shader shaderSpritesDefault;    //The GameObject's default Shader, used to restore it to its original color.

	// Use this for initialization
	protected override void Start()
    {
        //Call EnemyController's Start.
        base.Start();
        //Overwrite the EnemyController default values.
        hp = 12;
        SetSpeed(0f);
        bulletSpeed = .03f;
        scoreValue = 120;
        rateOfFire = .8f;
        cooldownTimer = 0f;
        //Initialize the objects used for the Wheel's "Hit Flash" effect.
        wheelRenderer = gameObject.GetComponent<SpriteRenderer>();
        shaderGUIText = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");
	}

    //Activate the Wheel's behaviors when it appears on screen.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.AddEnemyToList(gameObject);
            //Have the Wheel start firing.
            StartCoroutine(FiringCooldown());
        }
        //Otherwise, check if this is a Player-controlled Bullet. 
        else if (box.gameObject.CompareTag("PlayerBullet"))
        {
            //If it is, make the Wheel "flash" and make it invincible temporarily.
            if (hp > 0) { StartCoroutine(DamageCooldown()); }
            //Have the Wheel take damage.
            if (!dmgImmune) { TakeDamage(box.gameObject.GetComponent<Bullet>().dmgValue); }
        }
        //Otherwise, check if this is an Explosion.
        else if (box.gameObject.CompareTag("Explosion"))
        {
            //Check if this explosion does damage.
            if (box.gameObject.GetComponent<Explosion>().isDamaging)
            {
                //If it is, make the Wheel "flash" and make it invincible temporarily.
                if (hp > 0) { StartCoroutine(DamageCooldown()); }
                //Have the Wheel take damage.
                if (!dmgImmune) { TakeDamage(box.gameObject.GetComponent<Explosion>().dmgValue); }
            }
        }
    }

    //Despawn the Wheel when it passes outside of the screen.
    void OnTriggerExit2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.RemoveEnemyFromList(gameObject);
        }
    }

    //Fire four continuous streams of Bullets from each of the Wheel's four cannons as it rotates.
    IEnumerator FiringCooldown()
    {
        while (hp > 0)
        {
            //Get the Euler angle at which the 1st Bullet will be fired, modded by 360 degrees.
            firingAngle = transform.eulerAngles.z % 360;
            //Iterate through the loop 4 times, so 4 Bullets will be fired.
            for (int i = 0; i < 4; i++)
            {
                //Create the new Bullet behind the Wheel on the z-axis, by adding a Vector3 with a 1 in its z-component.
                GameObject bullet = Instantiate(wheelBullet, transform.position + Vector3.forward, Quaternion.identity);
                //Add the Bullet to the LevelManager's list.
                LevelManager.instance.AddBulletToList(bullet);
                //Set the angle for the Bullet, and set its speed.
                bullet.GetComponent<Bullet>().SetAngleInDegrees(firingAngle + (90 * i));
                bullet.GetComponent<Bullet>().SetSpeed(bulletSpeed);
            }
            //Wait a very short period of time before firing again.
            //Reset the Cooldown Timer.
            cooldownTimer = 0;
            //Have the Timer run for the "rateOfFire" in seconds before continuing.
            while (cooldownTimer < rateOfFire)
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

    //Turn the Wheel white, and make it invincible, for a very short period of time.
    IEnumerator DamageCooldown()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Make the Wheel immune to damage while it is flashing.
        dmgImmune = true;
        //Make the Wheel solid white to show that it has been hit.
        wheelRenderer.material.shader = shaderGUIText;
        wheelRenderer.color = Color.white;
        //Reset the Damage Timer.
        damageTimer = 0;
        //Let the Wheel be invincible for 1/30 of a second.
        while (damageTimer < Time.deltaTime * 2)
        {
            //If the Game is paused, don't update the Damage Timer.
            if (!LevelManager.instance.gamePaused)
            {
                damageTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Let the Wheel be susceptible to damage again.
        dmgImmune = false;
        //Change the Shader back to the default.
        wheelRenderer.material.shader = shaderSpritesDefault;
        wheelRenderer.color = Color.white;
    }

	// Update is called once per frame
	protected override void Update()
    {
        //Rotate the Wheel counterclockwise slightly if the Game isn't paused.
        if (!LevelManager.instance.gamePaused)
        {
            transform.Rotate(Vector3.forward, 0.05f, Space.World);
        }
        //Call EnemyController's Update.
        base.Update();
	}
}
