/*
 * Fly.cs
 * 
 * An airborne Enemy that stays on the right side of the screen,
 * and changes its vertical position to match the Player's before firing lasers at them.
 * 
 */

using UnityEngine;
using System.Collections;

public class Fly : EnemyController
{
    private Vector2 movementTarget; //Used to store the Fly's movement destinations throughout its behavior cycle.
    private float flySpeed;         //The Fly's speed while it's moving; used to easily set speed as Fly starts and stops movement.
    public GameObject flyBullet;    //The "Master Copy" of the Bullet that the Fly will fire.
    private float bulletSpeed;      //The speed of the Bullets that the Fly will fire.
    private float delayTimer;       //The time between when the Fly fires a laser and when it starts its next movement step.
    private float damageTimer;      //The amount of time that the Fly has "flashed" invincible.

    //"Flash" Variables:
    private SpriteRenderer flyRenderer;    //The Sprite Renderer attached to this GameObject.
    private Shader shaderGUIText;           //A Text Shader, used to turn the Fly solid white, even during its Animation.
    private Shader shaderSpritesDefault;    //The GameObject's default Shader, used to restore it to its original color.

	// Use this for initialization
	protected override void Start()
    {
        //Call EnemyController's Start.
        base.Start();
        //Overwrite the EnemyController default values.
        hp = 7;
        flySpeed = .8f;
        SetSpeed(flySpeed);
        bulletSpeed = .3f;
        scoreValue = 50;
        delayTimer = 0;
        //Initialize the objects used for the Fly's "Hit Flash" effect.
        flyRenderer = gameObject.GetComponent<SpriteRenderer>();
        shaderGUIText = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");
        //Have the Fly start moving.
        StartCoroutine(MovementCooldown());
    }

    //Activate the Fly's behaviors when it appears on screen.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.AddEnemyToList(gameObject);
        }
        //Otherwise, check if this is a Player-controlled Bullet. 
        else if (box.gameObject.CompareTag("PlayerBullet"))
        {
            //If it is, make the Fly "flash" and make it invincible temporarily.
            if (hp > 0) { StartCoroutine(DamageCooldown()); }
            //Have the Fly take damage.
            if (!dmgImmune) { TakeDamage(box.gameObject.GetComponent<Bullet>().dmgValue); }
        }
        //Otherwise, check if this is an Explosion.
        else if (box.gameObject.CompareTag("Explosion"))
        {
            //Check if this explosion does damage.
            if (box.gameObject.GetComponent<Explosion>().isDamaging)
            {
                //If it is, make the Fly "flash" and make it invincible temporarily.
                if (hp > 0) { StartCoroutine(DamageCooldown()); }
                //Have the Fly take damage.
                if (!dmgImmune) { TakeDamage(box.gameObject.GetComponent<Explosion>().dmgValue); }
            }
        }
    }

    //Despawn the Fly when it passes outside of the screen.
    void OnTriggerExit2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.RemoveEnemyFromList(gameObject);
        }
    }

    //Handle the Fly's timing for its movement and its projectile firing.
    IEnumerator MovementCooldown()
    {
        //Quickly move forward to a position along the right side of the screen.
        movementTarget = new Vector2(280, transform.position.y);
        //smoothMove(movementTarget.x, movementTarget.y, speed);
        SetTarget(movementTarget);
        //Wait until the Fly has moved into position.
        yield return new WaitUntil(() => Mathf.Abs(movementTarget.x - transform.position.x) < 3f);
        //Stop the Fly in this position.
        //smoothMove(transform.position.x, transform.position.y, speed);
        SetSpeed(0);
        //Create an instance of the Bullet that will appear behind the Fly (on the z-axis).
        GameObject bullet = Instantiate(flyBullet, transform.position + Vector3.forward, Quaternion.identity);
        //Add the Bullet to the LevelManager's list.
        LevelManager.instance.AddBulletToList(bullet);
        //Set the target for the laser directly to the left of the Fly.
        //flyBullet.GetComponent<Bullet>().ChangeTarget(new Vector2(-10f, transform.position.y), 90f);
        bullet.GetComponent<Bullet>().SetAngleInDegrees(180f);
        bullet.GetComponent<Bullet>().SetSpeed(bulletSpeed);
        //Do two more movement cycles before sending the Fly offscreen.
        for (int i = 0; i < 2; i++)
        {
            //Reset the Delay Timer.
            delayTimer = 0;
            //Wait for 1.5 seconds before starting movement.
            while (delayTimer < 1.5f)
            {
                //If the Game is paused, don't update the Delay Timer.
                if (!LevelManager.instance.gamePaused)
                {
                    delayTimer += Time.deltaTime;
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //Move the Fly along the right side of the screen until it matches the Player's current y-position.
            movementTarget.x = transform.position.x;
            movementTarget.y = GameObject.Find("Player").transform.position.y;
            //smoothMove(movementTarget.x, movementTarget.y, speed);
            SetTarget(movementTarget);
            SetSpeed(flySpeed);
            yield return new WaitUntil(() => Mathf.Abs(movementTarget.y - transform.position.y) < 3f);
            //Stop the Fly in this position.
            //smoothMove(transform.position.x, transform.position.y, speed);
            SetSpeed(0);
            //Create an instance of the Bullet that will appear behind the Fly (on the z-axis).
            bullet = Instantiate(flyBullet, transform.position + Vector3.forward, Quaternion.identity);
            //Add the Bullet to the LevelManager's list.
            LevelManager.instance.AddBulletToList(bullet);
            //Set the target for the laser directly to the left of the Fly.
            //flyBullet.GetComponent<Bullet>().ChangeTarget(new Vector2(-10f, transform.position.y), 90f);
            bullet.GetComponent<Bullet>().SetAngleInDegrees(180f);
            bullet.GetComponent<Bullet>().SetSpeed(bulletSpeed);
        }
        //Reset the Delay Timer.
        delayTimer = 0;
        //Wait for 1.5 seconds before sending the Fly offscreen.
        while (delayTimer < 1.5f)
        {
            //If the Game is paused, don't update the Delay Timer.
            if (!LevelManager.instance.gamePaused)
            {
                delayTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Quickly move the Fly to the left until it exits the screen.
        //smoothMove(transform.position.x - 10f, transform.position.y, speed);
        SetAngleInDegrees(180f);
        SetSpeed(flySpeed);
    }

    //Turn the Fly white, and make it invincible, for a very short period of time.
    IEnumerator DamageCooldown()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Make the Fly immune to damage while it is flashing.
        dmgImmune = true;
        //Make the Fly solid white to show that it has been hit.
        flyRenderer.material.shader = shaderGUIText;
        flyRenderer.color = Color.white;
        //Reset the Damage Timer.
        damageTimer = 0;
        //Let the Fly be invincible for 1/30 of a second.
        while (damageTimer < Time.deltaTime * 2)
        {
            //If the Game is paused, don't update the Damage Timer.
            if (!LevelManager.instance.gamePaused)
            {
                damageTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Let the Fly be susceptible to damage again.
        dmgImmune = false;
        //Change the Shader back to the default.
        flyRenderer.material.shader = shaderSpritesDefault;
        flyRenderer.color = Color.white;
    }

	// Update is called once per frame
	protected override void Update()
    {
        //Call EnemyController's Update.
        base.Update();
	}
}
