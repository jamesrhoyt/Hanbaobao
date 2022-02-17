/*
 * Item.cs
 * 
 * Objects that the Player can pick up by colliding with them,
 * which offer various benefits and bonuses.
 * 
 */

using UnityEngine;
using System.Collections;

public class Item : Movable2
{
    public int itemID; //The ID number for each Item, used to identify its effect.

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

    //Activate the Item's behaviors if it collides with the Player.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check for Player collision with the Item.
        if (box.gameObject.CompareTag("Player") && box.gameObject.GetComponent<ShipController>().isAlive)
        {
            //Do the Item Effect for Case 0 (change it to a different Item).
            ItemEffect();
            //Despawn the Item.
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    //Destroy the Item if it leaves the Screen.
    void OnTriggerExit2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            //Despawn the Item.
            gameObject.SetActive(false);
            Destroy(gameObject);
        }
    }

    //Perform whatever effect picking up the Item has.
    private void ItemEffect()
    {
        //Check the item's ID to determine its effect.
        switch (itemID)
        {
            //ID #0 is the Mystery Box, which is handled in MysteryItem.cs.
            //ID #1, 1-Up: Increase the Player's Lives by one, and update the HUD counter.
            case 1:
                try
                {
                    //Only give the Extra Life if the Player has fewer than 10.
                    if (GameManager.instance.lives < 10)
                    {
                        GameManager.instance.lives++;
                        LevelManager.instance.UpdateLives();
                    }
                    //Otherwise, give them a point bonus.
                    else
                    {
                        LevelManager.instance.UpdateScores(20000);
                    }
                }
                //If there is no GameManager, use LevelManager's variables instead.
                catch (System.NullReferenceException)
                {
                    //Only give the Extra Life if the Player has fewer than 10.
                    if (LevelManager.instance.lives < 10)
                    {
                        LevelManager.instance.lives++;
                        LevelManager.instance.UpdateLives();
                    }
                    //Otherwise, give them a point bonus.
                    else
                    {
                        LevelManager.instance.UpdateScores(20000);
                    }
                }
                break;
            //ID #2, Bomb: Increase the Player's Bombs by one, and update the HUD counter.
            case 2:
                try
                {
                    //Only give the Extra Bomb if the Player has fewer than 9.
                    if (GameManager.instance.bombs < 9)
                    {
                        GameManager.instance.bombs++;
                        LevelManager.instance.UpdateBombs();
                    }
                    //Otherwise, give them a point bonus.
                    else
                    {
                        LevelManager.instance.UpdateScores(5000);
                    }
                }
                //If there is no GameManager, use LevelManager's variables instead.
                catch (System.NullReferenceException)
                {
                    //Only give the Extra Bomb if the Player has fewer than 9.
                    if (LevelManager.instance.bombs < 9)
                    {
                        LevelManager.instance.bombs++;
                        LevelManager.instance.UpdateBombs();
                    }
                    //Otherwise, give them a point bonus.
                    else
                    {
                        LevelManager.instance.UpdateScores(5000);
                    }
                }
                break;
            //ID #3, Shield: Give the Player a protective one-hit Shield.
            case 3:
                //Only give the Player a Shield if they don't have one.
                if (!LevelManager.instance.player.GetComponent<ShipController>().shieldActive)
                {
                    LevelManager.instance.player.GetComponent<ShipController>().shieldActive = true;
                    //Make the Shield visible.
                    LevelManager.instance.player.GetComponent<ShipController>().shield.SetActive(true);
                }
                //Otherwise, give them a point bonus.
                else
                {
                    LevelManager.instance.UpdateScores(2500);
                }
                break;
            //ID #4, Base Gun: Equip the Player with the Base Gun, or improve the Base Gun they have equipped.
            case 4:
                LevelManager.instance.player.GetComponent<ShipController>().ChangeWeapon(0);
                break;
            //ID #5, Pierce Laser: Equip the Player with the Pierce Laser, or improve the Pierce Laser they have equipped.
            case 5:
                LevelManager.instance.player.GetComponent<ShipController>().ChangeWeapon(1);
                break;
            //ID #6, Bolo Gun: Equip the Player with the Bolo Gun, or improve the Bolo Gun they have equipped.
            case 6:
                LevelManager.instance.player.GetComponent<ShipController>().ChangeWeapon(2);
                break;
            //ID #7, Boomerang: Equip the Player with the Boomerang, or improve the Boomerang they have equipped.
            case 7:
                LevelManager.instance.player.GetComponent<ShipController>().ChangeWeapon(3);
                break;
            //ID #8, Lightning: Equip the Player with the Lightning, or improve the Lightning they have equipped.
            case 8:
                LevelManager.instance.player.GetComponent<ShipController>().ChangeWeapon(4);
                break;
            //ID #9, Flak Gun: Equip the Player with the Flak Gun, or improve the Flak Gun they have equipped.
            case 9:
                LevelManager.instance.player.GetComponent<ShipController>().ChangeWeapon(5);
                break;
            //ID #10, Multiplier 2X: Multiply any points the Player accrues by 2 for 30 seconds.
            case 10:
                LevelManager.instance.StartMultiplier(2);
                break;
            //ID #11, Multiplier 3X: Multiply any points the Player accrues by 3 for 30 seconds.
            case 11:
                LevelManager.instance.StartMultiplier(3);
                break;
            //ID #12, Multiplier 5X: Multiply any points the Player accrues by 5 for 30 seconds.
            case 12:
                LevelManager.instance.StartMultiplier(5);
                break;
            //ID #13, Multiplier 10X: Multiply any points the Player accrues by 10 for 30 seconds.
            case 13:
                LevelManager.instance.StartMultiplier(10);
                break;
            //default (won't ever be reached).
            default:
                break;
        }
    }
}
