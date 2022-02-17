/*
 * Explosion.cs
 * 
 * Play the Animation for an explosion when the Player, an Enemy, or a Bullet dies,
 * and check it for collision while it is onscreen if it is a Damaging explosion.
 * 
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Explosion : MonoBehaviour
{
    public bool isDamaging; //Whether this explosion can damage Enemies/the Player.
    public int dmgValue;    //How much damage the explosion can do.

	// Use this for initialization
	void Start()
    {
		
	}
	
	// Update is called once per frame
	void Update()
    {
        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsName("explosionEnd"))
        {
            LevelManager.instance.RemoveExplosionFromList(this.gameObject);
        }
	}
}
