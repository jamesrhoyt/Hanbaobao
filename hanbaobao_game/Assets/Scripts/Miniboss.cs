/*
 * Miniboss.cs
 * 
 * An abstract class to hold the common functions across all Minibosses in the Game,
 * including checking for collision and taking general damage.
 * 
 */

using UnityEngine;
using System.Collections;

public abstract class Miniboss : Movable2
{
    public int hp;  //The total Hit Points for the Miniboss.

	// Use this for initialization
	protected override void Start()
    {
        //Call Movable's Start.
        base.Start();
	}

    /// <summary>
    /// Check every Collider2D component in a Miniboss against the Collider in the argument.
    /// </summary>
    /// <param name="collider">The collider object being tested.</param>
    /// <param name="damageValue">The amount of damage the GameObject attached to the collider will do.</param>
    /// <returns>
    /// 0: The Bullet has not hit anything.
    /// 1: The Bullet has hit and damaged the Miniboss.
    /// 2: The Bullet has hit a damage-immune part of the Miniboss.
    /// </returns>
    public abstract int CheckCollision(Collider2D collider, int damageValue);

    //Toggle all Animators connected to the Miniboss when the Game is paused/unpaused.
    public abstract void ToggleAnimations(bool active);

    /// <summary>
    /// Reduce the Miniboss' HP when the Player damages it.
    /// </summary>
    /// <param name="damage">The amount to reduce the Miniboss' HP by.</param>
    protected void TakeDamage(int damage)
    {
        hp -= damage;
    }

	// Update is called once per frame
	protected override void Update()
    {
        //Call Movable's Update.
        base.Update();
	}
}
