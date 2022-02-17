/*
 * LightningWall.cs
 * 
 * One Node (of a pair) that will fire a stream of lightning in set intervals at the other Node in the pair.
 * If the other Node in the pair is destroyed, this Node will start firing Bullets diagonally every few seconds instead.
 * 
 */

using UnityEngine;
using System.Collections;

public class LightningWall : EnemyController
{
    public GameObject lightningBullet;      //One unit of the arc of lightning that the Wall will fire.
    public GameObject altLightningBullet;   //The "spark" Bullet that an electrode will fire when its partner is destroyed.
    private float altBulletSpeed;   //The speed of the "spark" Bullets.
    public GameObject partnerNode;  //The other Lightning Wall electrode that this one will arc lightning between.
    private float activeTimer;      //How long the "lightningBullet" Lightning field has been active.
    private float cooldownTimer;    //The timer for each of the pauses between the Lightning Wall's behavior states.
    private float damageTimer;      //The amount of time that the Node has "flashed" invincible.

    //"Flash" Variables:
    private SpriteRenderer nodeRenderer;    //The Sprite Renderer attached to this GameObject.
    private Shader shaderGUIText;           //A Text Shader, used to turn the Node solid white, even during its Animation.
    private Shader shaderSpritesDefault;    //The GameObject's default Shader, used to restore it to its original color.

	// Use this for initialization
	protected override void Start()
    {
        //Call EnemyController's Start.
        base.Start();
        //Overwrite the EnemyController default values.
        SetSpeed(0f);
        hp = 20;
        scoreValue = 200;
        rateOfFire = 3f;
        cooldownTimer = 0f;
        altBulletSpeed = .3f;
        //Set the firing timer to zero.
        activeTimer = 0f;
        //Initialize the objects used for the Node's "Hit Flash" effect.
        nodeRenderer = gameObject.GetComponent<SpriteRenderer>();
        shaderGUIText = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");
	}

    //Activate the Lightning Wall's behaviors when it appears on screen.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.AddEnemyToList(gameObject);
            //Add the Lightning field to LevelManager's Bullet list.
            LevelManager.instance.AddBulletToList(lightningBullet);
            //Have the Node start firing.
            StartCoroutine(FiringCooldown());
        }
        //Otherwise, check if this is a Player-controlled Bullet. 
        else if (box.gameObject.CompareTag("PlayerBullet"))
        {
            //If it is, make the Node "flash" and make it invincible temporarily.
            if (hp > 0) { StartCoroutine(DamageCooldown()); }
            //Have the Node take damage.
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

    //Fire a continuous stream of lightning from this Node to its partner Node.
    IEnumerator FiringCooldown()
    {
        while (hp > 0)
        {
            //Start the "flicker" animation when the lightning starts firing.
            GetComponent<Animator>().SetTrigger("activate");
            //Activate the Lightning field.
            lightningBullet.SetActive(true);
            //Have the lightning active for as long as the "rate-of-fire" dictates.
            while (activeTimer < rateOfFire)
            {
                //If the Partner Node dies, stop this loop.
                if (partnerNode == null)
                {
                    //Stop the "flicker" animation.
                    GetComponent<Animator>().SetTrigger("deactivate");
                    //Start the "solo firing" coroutine.
                    StartCoroutine(AltFiringCooldown());
                    //Stop this coroutine.
                    yield break;
                }
                //If the Game is paused, don't update the Active Timer.
                if (!LevelManager.instance.gamePaused)
                {
                    activeTimer += Time.deltaTime;
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //Stop the "flicker" animation when the lightning has stopped firing.
            GetComponent<Animator>().SetTrigger("deactivate");
            //Deactivate the Lightning field.
            lightningBullet.SetActive(false);
            //Wait for as long as the lightning was active before starting it up again.
            //Reset the Cooldown Timer.
            cooldownTimer = 0;
            //Have the lightning inactive for as long as the "rate-of-fire" dictates.
            while (cooldownTimer < rateOfFire)
            {
                //If the Game is paused, don't update the Cooldown Timer.
                if (!LevelManager.instance.gamePaused)
                {
                    cooldownTimer += Time.deltaTime;
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //Reset the Active Timer.
            activeTimer = 0f;
        }
    }

    //Fire two slow-moving Bullets diagonally from the center of this Node every few seconds.
    IEnumerator AltFiringCooldown()
    {
        while (hp > 0)
        {
            //Wait for a few seconds before firing the next set of Bullets.
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
            //Activate the "flicker" animation (it will only last for about a frame).
            GetComponent<Animator>().SetTrigger("activate");
            //Create the 1st instance of the Bullet behind the Node.
            GameObject altBullet = Instantiate(altLightningBullet, transform.position + Vector3.forward, Quaternion.identity) as GameObject;
            //Add the Bullet to the LevelManager's list.
            LevelManager.instance.AddBulletToList(altBullet);
            //Set the angle for the 1st Bullet to "diagonally up-right" (or down-left, if the Node is upside-down).
            altBullet.GetComponent<Bullet>().SetAngleInDegrees(45f + transform.rotation.z);
            //Set the Bullet's movement speed.
            altBullet.GetComponent<Bullet>().SetSpeed(altBulletSpeed);
            //Create the 2nd instance of the Bullet behind the Node.
            altBullet = Instantiate(altLightningBullet, transform.position + Vector3.forward, Quaternion.identity) as GameObject;
            //Add the Bullet to the LevelManager's list.
            LevelManager.instance.AddBulletToList(altBullet);
            //Set the angle for the 2nd Bullet to "diagonally up-left" (or down-right, if the Node is upside-down).
            altBullet.GetComponent<Bullet>().SetAngleInDegrees(135f + transform.rotation.z);
            //Set the Bullet's movement speed.
            altBullet.GetComponent<Bullet>().SetSpeed(altBulletSpeed);
            //Deactivate the "flicker" animation.
            GetComponent<Animator>().SetTrigger("deactivate");
        }
    }

    //Turn the Node white, and make it invincible, for a very short period of time.
    IEnumerator DamageCooldown()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Make the Node immune to damage while it is flashing.
        dmgImmune = true;
        //Make the Node solid white to show that it has been hit.
        nodeRenderer.material.shader = shaderGUIText;
        nodeRenderer.color = Color.white;
        //Reset the Damage Timer.
        damageTimer = 0;
        //Let the Node be invincible for 1/30 of a second.
        while (damageTimer < Time.deltaTime * 2)
        {
            //If the Game is paused, don't update the Damage Timer.
            if (!LevelManager.instance.gamePaused)
            {
                damageTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Let the Node be susceptible to damage again.
        dmgImmune = false;
        //Change the Shader back to the default.
        nodeRenderer.material.shader = shaderSpritesDefault;
        nodeRenderer.color = Color.white;
    }

	// Update is called once per frame
	protected override void Update()
    {
        //Call EnemyController's Update.
        base.Update();
	}
}
