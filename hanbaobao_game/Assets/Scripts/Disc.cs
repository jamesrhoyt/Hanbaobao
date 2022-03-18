/* 
 * Disc.cs
 * 
 * An Enemy that slowly travels to the left,
 * and fires a Bullet directly at the Player every couple of seconds.
 * 
 */

using UnityEngine;
using System.Collections;

public class Disc : EnemyController
{
    public GameObject discBullet;
    private Vector3 bulletTarget;   //The position of the Player at the time of firing.
    private float bulletSpeed;      //The speed of the Bullets the Disc fires.
    private float cooldownTimer;    //The amount of time elapsed since the last Bullet fired.
    private float damageTimer;      //The amount of time that the Disc has "flashed" invincible.

    //"Flash" Variables:
    private SpriteRenderer discRenderer;    //The Sprite Renderer attached to this GameObject.
    private Shader shaderGUIText;           //A Text Shader, used to turn the Disc solid white, even during its Animations.
    private Shader shaderSpritesDefault;    //The GameObject's default Shader, used to restore it to its original color.

	// Use this for initialization
	protected override void Start()
    {
        //Call EnemyController's Start.
        base.Start();
        //Overwrite the EnemyController default values.
        SetSpeed(.2f);
        hp = 2;
        scoreValue = 50;
        rateOfFire = 1.8f;
        cooldownTimer = 0f;
        bulletSpeed = .175f;
        //Start the Disc moving to the left.
        SetAngleInDegrees(180f);
        dmgImmune = false;
        //Initialize the objects used for the Disc's "Hit Flash" effect.
        discRenderer = gameObject.GetComponent<SpriteRenderer>();
        shaderGUIText = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");
	}

    //Activate the Box's behaviors when it appears on screen.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.AddEnemyToList(gameObject);
            //Have the Disc start firing.
            StartCoroutine(FiringCooldown());
        }
        //Otherwise, check if this is a Player-controlled Bullet. 
        else if (box.gameObject.CompareTag("PlayerBullet"))
        {
            //If it is, make the Disc "flash" and make it invincible temporarily.
            if (hp > 0) { StartCoroutine(DamageCooldown()); }
            //Have the Disc take damage.
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

    //Despawn the Disc when it passes outside of the screen.
    void OnTriggerExit2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.RemoveEnemyFromList(gameObject);
        }
    }

    //Fire a Bullet directly at the Player every couple of seconds.
    IEnumerator FiringCooldown()
    {
        while (hp > 0)
        {
            //Wait first before firing, to keep the Disc from firing as soon as it enters the screen.
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
            //Get the Player's location to use as the firing target.
            bulletTarget = GameObject.FindWithTag("Player").transform.position;
            //Create an instance of the Bullet that will appear in front of the Disc (on the z-axis).
            GameObject bullet = Instantiate(discBullet, transform.position + Vector3.back, Quaternion.identity);
            //Add the Bullet to the LevelManager's list.
            LevelManager.instance.AddBulletToList(bullet);
            //Assign the Bullet's target and speed.
            bullet.GetComponent<Bullet>().SetTarget(bulletTarget);
            bullet.GetComponent<Bullet>().SetSpeed(bulletSpeed);
            //Reset the Cooldown Timer.
            cooldownTimer = 0;
        }
    }

    //Turn the Disc white, and make it invincible, for a very short period of time.
    IEnumerator DamageCooldown()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Make the Disc immune to damage while it is flashing.
        dmgImmune = true;
        //Make the Disc solid white to show that it has been hit.
        discRenderer.material.shader = shaderGUIText;
        discRenderer.color = Color.white;
        //Reset the Damage Timer.
        damageTimer = 0;
        //Let the Disc be invincible for 1/30 of a second.
        while (damageTimer < Time.deltaTime * 2)
        {
            //If the Game is paused, don't update the Damage Timer.
            if (!LevelManager.instance.gamePaused)
            {
                damageTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Let the Disc be susceptible to damage again.
        dmgImmune = false;
        //Change the Shader back to the default.
        discRenderer.material.shader = shaderSpritesDefault;
        discRenderer.color = Color.white;
    }

	// Update is called once per frame
	protected override void Update()
    {
        //Call EnemyController's Update.
        base.Update();
	}
}
