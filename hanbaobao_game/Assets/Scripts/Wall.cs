/*
 * Wall.cs
 * 
 * The object that makes up the boundaries and obstacles of the "B"-section of each Stage.
 * (For collision detection purposes, Wall is treated in the code as an Enemy with no behaviors.)
 * 
 */

using UnityEngine;
using System.Collections;

public class Wall : EnemyController
{

	// Use this for initialization
	protected override void Start()
    {
        //Call EnemyController's Start.
        base.Start();
        //Overwrite the EnemyController default values.
        hp = 1;
        SetSpeed(0f);
        scoreValue = 0;
	}

    //Add the segment to the list of enemies to check collisions on.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.AddEnemyToList(gameObject);
        }
    }

    //Despawn the Fold Wall Segment when it passes outside of the screen.
    void OnTriggerExit2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.RemoveEnemyFromList(gameObject);
        }
    }

	// Update is called once per frame
	protected override void Update()
    {
        //Call EnemyController's Update.
        base.Update();
	}
}
