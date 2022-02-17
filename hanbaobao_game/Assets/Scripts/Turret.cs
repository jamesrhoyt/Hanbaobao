/*
 * Turret.cs
 * 
 * A stationary Enemy that fires its Bullets diagonally away from itself.
 * The Turret either fires a single Bullet, or a stream of 3.
 * 
 */

using UnityEngine;
using System.Collections;

public class Turret : EnemyController
{
    public GameObject turretBullet; //The "Master Copy" of the Bullet that the Turret will fire.
    private float bulletSpeed;      //The speed of the Turret's Bullets.
    public bool rapidFire; //Whether this Turret is equipped with Rapid Fire.
    private float cooldownTimer;    //The amount of time elapsed since the last Bullet fired.
    private float damageTimer;      //The amount of time that the Turret has "flashed" invincible.

    //"Flash" Variables:
    private SpriteRenderer turretRenderer;  //The Sprite Renderer attached to this GameObject.
    private Shader shaderGUIText;           //A Text Shader, used to turn the Turret solid white.
    private Shader shaderSpritesDefault;    //The GameObject's default Shader, used to restore it to its original color.

    // Use this for initialization
    protected override void Start()
    {
        //Start EnemyController's Start.
        base.Start();
        //Overwrite the EnemyController default values.
        SetSpeed(0f);
        bulletSpeed = 1.2f;
        rateOfFire = 3f;
        //Set HP to 10 if this is a Rapid-Fire Turret, and 15 if it is not.
        hp = rapidFire ? 10 : 15;
        //Set Score to 150 if this is a Rapid-Fire Turret, and 100 if it is not.
        scoreValue = rapidFire ? 150 : 100;

        dmgImmune = false;
        cooldownTimer = 0f;
        //Initialize the objects used for the Cube's "Hit Flash" effect.
        turretRenderer = gameObject.GetComponent<SpriteRenderer>();
        shaderGUIText = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");
    }

    //Activate the Turret's behaviors when it appears on screen.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if(box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.AddEnemyToList(gameObject);
            //Have the Turret start firing.
            StartCoroutine(FiringCooldown());
        }
        //Otherwise, check if this is a Player-controlled Bullet. 
        else if (box.gameObject.CompareTag("PlayerBullet"))
        {
            //If it is, make the Turret "flash" and make it invincible temporarily.
            if (hp > 0) { StartCoroutine(DamageCooldown()); }
            //Have the Turret take damage.
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

    //Despawn the Turret when it passes outside of the screen.
    void OnTriggerExit2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.RemoveEnemyFromList(gameObject);
        }
    }

    //Fire a Bullet (or short stream of Bullets) down the barrel of the Turret every few seconds.
    IEnumerator FiringCooldown()
    {
        while (hp > 0)
        {
            //Wait first before firing, to keep the Turret from firing as soon as it enters the screen.
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
                yield return new WaitForSeconds(0);
            }
            //If the Turret is upside-down, make the Bullet travel down and to the left.
            if (transform.lossyScale.y < 0)
            {
                //turretBullet.GetComponent<Bullet>().ChangeTarget(new Vector2(transform.position.x - 10f, transform.position.y - 10f), 1.2f);
                turretBullet.GetComponent<Bullet>().SetAngleInDegrees(225f);
            }
            //If the Turret is right-side-up, make the Bullet travel up and to the left.
            else
            {
                //turretBullet.GetComponent<Bullet>().ChangeTarget(new Vector2(transform.position.x - 10f, transform.position.y + 10f), 1.2f);
                turretBullet.GetComponent<Bullet>().SetAngleInDegrees(135f);
            }
            turretBullet.GetComponent<Bullet>().SetSpeed(bulletSpeed);
            //Create an instance of the Bullet that will appear behind the Turret (on the z-axis).
            GameObject bullet = Instantiate(turretBullet, transform.position + new Vector3(0f, 0f, 1f), Quaternion.identity) as GameObject;
            //Add the Bullet to the LevelManager's list.
            LevelManager.instance.AddBulletToList(bullet);
            //If this is a Rapid-Fire Turret, create two more bullets immediately after the first.
            if(rapidFire)
            {
                for (int i = 0; i < 2; i++)
                {
                    //Wait briefly before firing, so the Bullets form a line.
                    //Reset the Cooldown Timer.
                    cooldownTimer = 0;
                    //Have the Timer run for a very short time before continuing.
                    while (cooldownTimer < rateOfFire / 24f)
                    {
                        //If the Game is paused, don't update the Cooldown Timer.
                        if (!LevelManager.instance.gamePaused)
                        {
                            cooldownTimer += Time.deltaTime;
                        }
                        yield return new WaitForSeconds(0);
                    }
                    //If the Turret is upside-down, make the Bullet travel down and to the left.
                    if (transform.lossyScale.y < 0)
                    {
                        //turretBullet.GetComponent<Bullet>().ChangeTarget(new Vector2(transform.position.x - 10f, transform.position.y - 10f), 1.2f);
                        turretBullet.GetComponent<Bullet>().SetAngleInDegrees(225f);
                    }
                    //If the Turret is right-side-up, make the Bullet travel up and to the left.
                    else
                    {
                        //turretBullet.GetComponent<Bullet>().ChangeTarget(new Vector2(transform.position.x - 10f, transform.position.y + 10f), 1.2f);
                        turretBullet.GetComponent<Bullet>().SetAngleInDegrees(135f);
                    }
                    turretBullet.GetComponent<Bullet>().SetSpeed(bulletSpeed);
                    //Create an instance of the Bullet that will appear behind the Turret (on the z-axis).
                    bullet = Instantiate(turretBullet, transform.position + new Vector3(0f, 0f, 1f), Quaternion.identity) as GameObject;
                    //Add the Bullet to the LevelManager's list.
                    LevelManager.instance.AddBulletToList(bullet);
                }
            }
        }
    }

    //Turn the Turret white, and make it invincible, for a very short period of time.
    IEnumerator DamageCooldown()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Make the Turret immune to damage while it is flashing.
        dmgImmune = true;
        //Make the Turret solid white to show that it has been hit.
        turretRenderer.material.shader = shaderGUIText;
        turretRenderer.color = Color.white;
        //Reset the Damage Timer.
        damageTimer = 0;
        //Let the Turret be invincible for 1/30 of a second.
        while (damageTimer < Time.deltaTime * 2)
        {
            //If the Game is paused, don't update the Damage Timer.
            if (!LevelManager.instance.gamePaused)
            {
                damageTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(0);
        }
        //Let the Turret be susceptible to damage again.
        dmgImmune = false;
        //Change the Shader back to the default.
        turretRenderer.material.shader = shaderSpritesDefault;
        turretRenderer.color = Color.white;
    }

    // Update is called once per frame
    protected override void Update()
    {
        //Call EnemyController's Update.
        base.Update();
    }
}
