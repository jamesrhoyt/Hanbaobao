/*
 * MysteryItem.cs
 * 
 * A special kind of Item that spawns a copy of any other Item
 * in the game when either the Player or one of their Bullets collide with it.
 * 
 */

using UnityEngine;
using System.Collections;

public class MysteryItem : Movable2
{
    public GameObject[] potentialItems; //A copy of every Item in the game (except for the Mystery Box), to randomly spawn one when the Mystery Item is hit.

	// Use this for initialization
	protected override void Start()
    {
        //Call Movable's Start.
        base.Start();
	}
	
	// Update is called once per frame
	protected override void Update()
    {
        //Call Movable's Update.
        base.Update();
	}

    //Generate a random Item if this Box is shot by/collides with the Player.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check that the Player is alive, or that the collider is a Player Bullet.
        if (box.gameObject.CompareTag("Player") && box.gameObject.GetComponent<ShipController>().isAlive || box.gameObject.CompareTag("PlayerBullet"))
        {
            //Call the function to create a new Item in place of this one.
            GetRandomItem();
            //Despawn the Item.
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    //Generate a random Item based off of a seeded value.
    private void GetRandomItem()
    {
        try
        {
            Random.InitState(GameManager.instance.score.ToString().PadLeft(6,'0').ToCharArray()[3]);
        }
        catch (System.NullReferenceException)
        {
            Random.InitState(Time.frameCount);
        }
        Instantiate(potentialItems[Random.Range(0, potentialItems.Length)], transform.position, Quaternion.identity);
    }
}
