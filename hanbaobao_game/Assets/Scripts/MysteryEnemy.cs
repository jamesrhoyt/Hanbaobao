/*
 * MysteryEnemy.cs
 * 
 * A special kind of Enemy that spawns a copy of any other airborne Enemy
 * in the game when either the Player or one of their Bullets collide with it.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MysteryEnemy : EnemyController
{
    public GameObject[] potentialEnemies;   //A copy of every flying Enemy in the game (except for the Mystery Box), to randomly spawn one when the Mystery Enemy is hit.
    private GameObject enemyToSpawn;        //The Enemy GameObject chosen to instantiate, selected from the array above.

	// Use this for initialization
	protected override void Start()
    {
		//Call EnemyController's Start.
        base.Start();
        //Overwrite EnemyController's default values.
        hp = 1;
        SetSpeed(0f);
        scoreValue = 10;
	}
	
	// Update is called once per frame
	protected override void Update()
    {
		//Call EnemyController's Update.
        base.Update();
	}

    //Generate a random Enemy if this Box is shot by/collides with the Player.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check that the Player is alive, or that the collider is a Player Bullet or a damaging Explosion.
        if (box.gameObject.CompareTag("Player") && box.gameObject.GetComponent<ShipController>().isAlive || box.gameObject.CompareTag("PlayerBullet") || box.gameObject.CompareTag("Explosion") && box.gameObject.GetComponent<Explosion>().isDamaging)
        {
            //Call the function to create a new Enemy in place of this one.
            GetRandomEnemy();
            //Despawn the Enemy.
            LevelManager.instance.RemoveEnemyFromList(gameObject);
        }
        //If the Box is entering the screen, add it to LevelManager's list.
        else if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.AddEnemyToList(gameObject);
        }
    }

    //Despawn the Mystery Box when it passes outside of the screen.
    void OnTriggerExit2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.RemoveEnemyFromList(gameObject);
        }
    }

    //Generate a random Enemy based off of a seeded value.
    private void GetRandomEnemy()
    {
        try
        {
            Random.InitState(GameManager.instance.score.ToString().PadLeft(6, '0').ToCharArray()[3]);
        }
        catch (System.NullReferenceException)
        {
            Random.InitState(Time.frameCount);
        }
        enemyToSpawn = Instantiate(potentialEnemies[Random.Range(0, potentialEnemies.Length)], transform.position, Quaternion.identity);
        enemyToSpawn.GetComponent<EnemyController>().item = gameObject.GetComponent<EnemyController>().item;
    }
}
