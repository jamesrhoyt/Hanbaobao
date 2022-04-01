/*
 * FoldWallSegment.cs
 * 
 * The gold segment that gets placed whenever a Fold Wall 
 * finishes one of its rotations.
 * 
 */

using UnityEngine;
using System.Collections;

public class FoldWallSegment : EnemyController
{
    public bool onscreen;   //Whether this object is onscreen or not.
                            //(Used in determining when to despawn the Fold Wall Enemy objects.)
    public bool destroyOnExit;  //Whether this segment should automatically be destroyed when it leaves the screen.

	// Use this for initialization
	protected override void Start()
    {
        //Call EnemyController's Start.
        base.Start();
        //Overwrite the EnemyController default values.
        hp = 1;
        SetSpeed(0f);
        scoreValue = 0;
        //Initialize the "onscreen" flag.
        onscreen = false;
	}

    //Add the segment to the list of enemies to check collisions on.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.AddEnemyToList(gameObject);
            onscreen = true;
        }
        //Otherwise, check if this is a Player-controlled Bullet. 
        else if (box.gameObject.CompareTag("PlayerBullet"))
        {
            if (!dmgImmune)
            {
                //Have the Fold Wall Segment take damage.
                TakeDamage(box.gameObject.GetComponent<Bullet>().dmgValue);
            }
        }
    }

    //Despawn the Fold Wall Segment when it passes outside of the screen.
    void OnTriggerExit2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            if (destroyOnExit)
            {
                LevelManager.instance.RemoveEnemyFromList(gameObject);
            }
            onscreen = false;
        }
    }

	// Update is called once per frame
	protected override void Update()
    {
        //Call EnemyController's Update.
        base.Update();
	}
}
