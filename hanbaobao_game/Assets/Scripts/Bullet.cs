/*
 * Bullet.cs
 * 
 * Manages the movement of Bullets fired by either the Player, the Enemies, or the Bosses.
 * Only handles the destruction of Bullets when they leave the Screen; collision with other Objects is handled elsewhere.
 * 
 */

using UnityEngine;
using System.Collections;

public class Bullet : Movable2
{
    public Collider2D hitbox;   //The Collider component for the Bullet.
    public int dmgValue;    //The amount of damage this bullet will do to an enemy.
                            //(All "EnemyBullet" tagged bullets are One-Hit Kills)

	// Use this for initialization
	protected override void Start()
    {
        //Call Movable's Start.
        base.Start();
        hitbox = GetComponent<Collider2D>();
	}
	
	// Update is called once per frame
	protected override void Update()
    {
        //If this Bullet is a Boomerang, reduce its speed over time, which will eventually reverse its direction.
        if(gameObject.name.Contains("PlayerBoomerang"))
        {
            //Only update its behavior if the Game is not paused.
            if (!LevelManager.instance.gamePaused)
            {
                //Use the "SetSpeed" function to reduce the Boomerang's speed over time.
                SetSpeed(GetSpeed() - .025f);
            }
        }
        //If this Bullet is a Mirror-fired Boomerang, do the same reversal in the opposite direction.
        else if (gameObject.name.Contains("MirrorBoomerang"))
        {
            //Only update its behavior if the Game is not paused.
            if (!LevelManager.instance.gamePaused)
            {
                //Use the "SetSpeed" function to reduce the Boomerang's speed over time.
                SetSpeed(GetSpeed() - .025f);
            }
        }
        //Call Movable's Update.
        base.Update();
	}

    //Check this Bullet for collision against Enemies, Minibosses, and Stage Bosses.
    void OnTriggerEnter2D(Collider2D box)
    {
        //If this Bullet collides with an Enemy, was fired by the Player, and is not a Pierce Laser or a Boomerang, remove it.
        if (box.gameObject.CompareTag("Enemy") && gameObject.CompareTag("PlayerBullet") && !gameObject.name.Contains("PlayerLaser") && !gameObject.name.Contains("PlayerBoomerang") && !gameObject.name.Contains("PlayerLightning"))
        {
            LevelManager.instance.RemoveBulletFromList(gameObject);
        }
    }

    //Remove the Bullet from the game when it leaves the screen.
    void OnTriggerExit2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox") && !gameObject.name.Contains("PlayerLightning"))
        {
            LevelManager.instance.RemoveBulletFromList(gameObject);
        }
    }
}
