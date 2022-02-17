/*
 * HydraNeck.cs
 * 
 * Manages the movement of each part of the Hydra Head's Neck,
 * as dictated by the movement behaviors in its head ("HydraHead.cs").
 * 
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HydraNeck : EnemyController
{
	// Use this for initialization
	protected override void Start()
    {
        //Call EnemyController's Start.
        base.Start();
        //Overwrite the EnemyController default values.
        hp = 1;
        scoreValue = 0;
        SetSpeed(0f);
	}

	// Update is called once per frame
	protected override void Update()
    {
        //Call EnemyController's Update.
        base.Update();
	}
}
