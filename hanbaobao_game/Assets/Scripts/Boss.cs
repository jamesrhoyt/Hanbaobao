/*
 * Boss.cs
 * 
 * An abstract class to hold the common functions across all Stage Bosses in the Game,
 * including checking for collision and taking general damage.
 * 
 */

using UnityEngine;
using System.Collections;

public abstract class Boss : Movable2
{
    public int hp;  //The total Hit Points for the Boss.
    public bool bossIntroComplete;  //Whether the introductory animation for the Boss is complete.

	// Use this for initialization
	protected override void Start()
    {
        //Call Movable2's Start.
        base.Start();
        //Set "bossIntroComplete" to false.
        bossIntroComplete = false;
	}

    /// <summary>
    /// Run the introductory animation for the Boss as it enters the Stage, in whatever form that animation takes.
    /// </summary>
    /// <returns></returns>
    public abstract IEnumerator BossIntroAnimation();

    /// <summary>
    /// Check every Collider2D component in a Boss against the Collider in the argument.
    /// </summary>
    /// <param name="collider">The collider object being tested.</param>
    /// <param name="damageValue">The amount of damage the GameObject attached to the collider will do.</param>
    /// <returns>
    /// 0: The Bullet has not hit anything.
    /// 1: The Bullet has hit and damaged the Boss.
    /// 2: The Bullet has hit a damage-immune part of the Boss.
    /// </returns>
    public abstract int CheckCollision(Collider2D collider, int damageValue);

    /// <summary>
    /// Reduce the Boss' HP when the Player damages it.
    /// </summary>
    /// <param name="damage">The amount to reduce the Boss' HP by.</param>
    protected void TakeDamage(int damage)
    {
        hp -= damage;
    }

	// Update is called once per frame
	protected override void Update()
    {
        //Call Movable2's Update.
        base.Update();
	}
}
