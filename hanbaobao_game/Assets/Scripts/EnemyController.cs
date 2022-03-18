/*
 * EnemyController.cs
 * 
 * Manages the basic functionality of Enemy-type Objects,
 * including taking damage from the Player and enabling Movable behavior.
 * 
 */

using UnityEngine;
using System.Collections;

public class EnemyController : Movable2
{
    public int hp;              //The Hit Point value for the Enemy. When this reaches 0, the Enemy dies.
    public int scoreValue;      //The number of points the Player achieves by killing this enemy.
    protected float rateOfFire; //The time (in seconds) that elapses before the enemy fires a projectile.
    public bool dmgImmune;      //Whether this Enemy GameObject is immune to Player fire/bombs.
    private bool itemDropped;   //Whether this Enemy has dropped its Item already (used to prevent Item dupes).

    public Collider2D hitbox;   //The Collider object that the Enemy checks for collision against various objects.
    public GameObject item;     //The Item that an Enemy can potentially drop.
    public GameObject explosion;//The Explosion that is created when an Enemy is destroyed.

	// Use this for initialization
	protected override void Start()
    {
        //Initialize values (will be overwritten by each individual enemy type).
        SetSpeed(0f);
        hp = 1;
        rateOfFire = 100f;
        itemDropped = false;
        //Set the Hitbox collider for the Enemy.
        hitbox = GetComponent<Collider2D>();
        //Call Movable's Start.
        base.Start();
	}
	
	// Update is called once per frame
	protected override void Update()
    {
        //Call Movable's Update.
        base.Update();
    }

    // Take damage from the Player, then check the HP and drop the Enemy's Item if it dies.
    public void TakeDamage(int damage)
    {
        //Subtract the damage value from the Enemy's HP.
        hp -= damage;
        LevelManager.instance.enemiesHit++;
        //If the enemy's HP hits 0, remove it from the game and update the Level Manager accordingly.
        if (hp <= 0)
        {
            LevelManager.instance.UpdateScores(scoreValue);
            LevelManager.instance.RemoveEnemyFromList(gameObject);
            //If the Enemy was immune to damage (like the Snake body), don't count it as it wasn't killed conventionally.
            if (!dmgImmune)
            {
                LevelManager.instance.enemiesKilled++;
            }
            //If the Enemy has an Item, drop it where the Enemy died.
            if (item != null && !itemDropped)
            {
                item = Instantiate(item, transform.position, Quaternion.identity);
                itemDropped = true;
            }
            explosion = Instantiate(explosion, transform.position, Quaternion.identity);
            LevelManager.instance.AddExplosionToList(explosion);
        }
    }
}
