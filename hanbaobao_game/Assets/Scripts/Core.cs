/*
 * Core.cs
 * 
 * The Boss of Stage 1.
 * A single spherical core surrounded by a large stationary metal shell, 
 * with various weapons attached to its front.
 * 
 */

using UnityEngine;
using System.Collections;

public class Core : Boss
{
    public GameObject mainCore; //The sphere at the center of the Boss.
    private SpriteRenderer coreRenderer; //"mainCore"'s SpriteRenderer.
    public Sprite[] mainCoreSprites;    //The Sprites to use for the mainCore object.
    private int startingHP; //The Core's starting health, used in determining what mainCore Sprite to load.
    private bool dmgImmune; //Whether or not "mainCore" is currently immune to damage.
    private bool corePulsing;   //Whether or not the "PulseCoreSprite" Coroutine has already been started.

    //"Flash" Variables:
    private Shader shaderGUIText;           //A Text Shader, used to turn the Core's objects solid white when they are shot.
    private Shader shaderSpritesDefault;    //The GameObjects' default Shader, used to restore the objects to their original colors.
    private float damageTimer;  //The amount of time that the Main Core has "flashed" invincible.

	// Use this for initialization
	protected override void Start()
    {
        base.Start();
        coreRenderer = mainCore.GetComponent<SpriteRenderer>();
        startingHP = hp;
        corePulsing = false;
        shaderGUIText = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");
	}

    /// <summary>
    /// Run the introductory animation for the Boss as it enters the Stage, in whatever form that animation takes.
    /// For the Core, simply change the color value of the Core's Sprites from black to white gradually.
    /// </summary>
    public override IEnumerator BossIntroAnimation()
    {
        yield return new WaitForEndOfFrame();
        //Let LevelManager know that the Boss Introduction is complete.
        bossIntroComplete = true;
    }

    /// <summary>
    /// Run the death animation for the Boss when its HP reaches 0, in whatever form that animation takes.
    /// For the Core, [TBD].
    /// </summary>
    public override IEnumerator BossDeathAnimation()
    {
        yield return new WaitForEndOfFrame();
        //Simply wait 4 seconds (As a Placeholder)
        yield return new WaitForSeconds(4);
        //Let LevelManager know that the Boss Death Animation is complete.
        bossDeathComplete = true;
    }

    /// <summary>
    /// Check the Collider component against each of the objects that make up the Core Boss.
    /// </summary>
    /// <param name="collider">The collider to check, attached to a Bullet-type GameObject.</param>
    /// <param name="damageValue">The amount of damage the Bullet would do. Used if "TakeDamage" needs to be called.</param>
    /// <returns>Whether or not the Bullet is touching another Collider.
    /// 0: The Bullet has not hit anything.
    /// 1: The Bullet has hit and damaged the Core.
    /// 2: The Bullet has hit a damage-immune part of the Core.
    /// </returns>
    public override int CheckCollision(Collider2D collider, int damageValue)
    {
        //throw new System.NotImplementedException();
        //Check the Bullet's Collider against the Main Core's Collider.
        if (collider.IsTouching(mainCore.GetComponent<CircleCollider2D>()))
        {
            //Do damage to the Boss.
            TakeDamage(damageValue);
            StartCoroutine(DamageCooldown());
            return 1;
        }
        return 0;
    }

    //Turn one of the objects that makes up the Boss invincible for a short period of time.
    private IEnumerator DamageCooldown()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Check that the Object is not immune to damage already.
        if (!dmgImmune)
        {
            //Make the Object immune for the duration of the Coroutine.
            dmgImmune = true;
            //Make the Object solid white to show that it has been hit.
            coreRenderer.material.shader = shaderGUIText;
            coreRenderer.color = Color.white;
            //Determine whether to change the Core's Sprite.
            CalculateDamage();
            //Reset the Damage Timer.
            damageTimer = 0;
            //Let the object be invincible for 1/30th of a second.
            while (damageTimer < Time.deltaTime * 2)
            {
                //If the Game is paused, don't update the Damage Timer.
                if (!LevelManager.instance.gamePaused)
                {
                    damageTimer += Time.deltaTime;
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //Let the Object be susceptible to damage again.
            dmgImmune = false;
            //Change the Object's shader back to the default.
            coreRenderer.material.shader = shaderSpritesDefault;
            coreRenderer.color = Color.white;
        }
    }

    //Determine what percentage of HP the Core has left, and update its Sprite if necessary.
    private void CalculateDamage()
    {
        float hpRatio = (float)hp / startingHP;
        //If hp is greater then 80%, make the Core blue.
        if (hpRatio <= 1f && hpRatio > .8f)
        {
            if (coreRenderer.sprite != mainCoreSprites[0])
            {
                coreRenderer.sprite = mainCoreSprites[0];
            }
        }
        //If hp is between 60% and 80%, make the Core green.
        else if (hpRatio <= .8f && hpRatio > .6f)
        {
            if (coreRenderer.sprite != mainCoreSprites[1])
            {
                coreRenderer.sprite = mainCoreSprites[1];
            }
        }
        //If hp is between 40% and 60%, make the Core yellow.
        else if (hpRatio <= .6f && hpRatio > .4f)
        {
            if (coreRenderer.sprite != mainCoreSprites[2])
            {
                coreRenderer.sprite = mainCoreSprites[2];
            }
        }
        //If hp is between 20% and 40%, make the Core orange.
        else if (hpRatio <= .4f && hpRatio > .2f)
        {
            if (coreRenderer.sprite != mainCoreSprites[3])
            {
                coreRenderer.sprite = mainCoreSprites[3];
            }
        }
        //If hp is less than 20%, make the Core red.
        else if (hpRatio <= .2f && hpRatio > 0f)
        {
            if (coreRenderer.sprite != mainCoreSprites[4])
            {
                coreRenderer.sprite = mainCoreSprites[4];
            }
        }
        //If hp drops below 10%, start making the Main Core flash on and off, if it isn't already.
        if (hpRatio <= .1f && !corePulsing)
        {
            StartCoroutine(PulseCoreSprite());
            corePulsing = true;
        }
    }

    //Fade the Main Core's Sprite Renderer color between black and white.
    private IEnumerator PulseCoreSprite()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Run this code in a loop while the Core still has health.
        while (hp > 0)
        {
            //Fade the coreRenderer's base color from white to black.
            while (coreRenderer.color != Color.black)
            {
                //Only change the color if the Game isn't paused.
                if (!LevelManager.instance.gamePaused)
                {
                    coreRenderer.color = new Color(coreRenderer.color.r - .125f, coreRenderer.color.g - .125f, coreRenderer.color.b - .125f);
                }
                yield return new WaitForSeconds(Time.deltaTime * 6);
            }
            //Fade the coreRenderer's base color from black back to white.
            while (coreRenderer.color != Color.white)
            {
                //Only change the color if the Game isn't paused.
                if (!LevelManager.instance.gamePaused)
                {
                    coreRenderer.color = new Color(coreRenderer.color.r + .125f, coreRenderer.color.g + .125f, coreRenderer.color.b + .125f);
                }
                yield return new WaitForSeconds(Time.deltaTime * 6);
            }
        }
    }

	// Update is called once per frame
	protected override void Update()
    {
        base.Update();
	}
}
