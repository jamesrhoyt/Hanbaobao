/*
 * Factory.cs
 * 
 * The Boss of Stage 4.
 * An arrangement of mechanical parts, processing units, and protective chassis, 
 * which defends itself by spawning enemies that it builds.
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Factory : Boss
{
    //"Flash" Variables:
    private Shader shaderGUIText;           //A Text Shader, used to turn the Factory's objects solid white when they are shot.
    private Shader shaderSpritesDefault;    //The GameObjects' default Shader, used to restore the objects to their original colors.

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        shaderGUIText = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");
    }

    /// <summary>
    /// Run the introductory animation for the Boss as it enters the Stage, in whatever form that animation takes.
    /// For the Factory, [TBD].
    /// </summary>
    public override IEnumerator BossIntroAnimation()
    {
        yield return new WaitForEndOfFrame();
        //Let LevelManager know that the Boss Introduction is complete.
        bossIntroComplete = true;
    }

    /// <summary>
    /// Run the death animation for the Boss when its HP reaches 0, in whatever form that animation takes.
    /// For the Factory, [TBD].
    /// </summary>
    public override IEnumerator BossDeathAnimation()
    {
        yield return new WaitForEndOfFrame();
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
    /// 1: The Bullet has hit and damaged the Factory.
    /// 2: The Bullet has hit a damage-immune part of the Factory.
    /// </returns>
    public override int CheckCollision(Collider2D collider, int damageValue)
    {
        //throw new System.NotImplementedException();
        return 0;
    }

    // Update is called once per frame
    protected override void Update()
    {
        base.Update();
    }
}
