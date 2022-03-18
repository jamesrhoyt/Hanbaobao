/*
 * FormationBox.cs
 * 
 * An Enemy that slowly travels in a rigid "figure-eight" pattern, 
 * and fires a Bullet toward the Player whenever it changes direction.
 * 
 */

using UnityEngine;
using System.Collections;

public class FormationBox : EnemyController
{
    private float width;    //The width of the Formation Box, to be used in the movement pattern.
    private float height;   //The height of the Formation Box, to be used in the movement pattern.
    private Vector2 lastTarget; //The target Vector that the Formation Box is traveling from.
    private Vector2 moveTarget; //The target Vector that the Formation Box is traveling to.
    private float diagDistance; //The distance the Formation Box will be traveling diagonally.
    private int targetIndex;    //The index (0-3) of the current destination target.
    private float damageTimer;  //The amount of time that the Formation Box has "flashed" invincible.

    public GameObject boxBullet;    //The "Master Copy" of the Bullet that the Formation Box fires.
    private float bulletSpeed;      //The speed of the Bullets the Formation Box fires.
    private Vector3 shotTarget;     //The position of the Player at the time of firing.

    //"Flash" Variables:
    private SpriteRenderer formboxRenderer; //The Sprite Renderer attached to this GameObject.
    private Shader shaderGUIText;           //A Text Shader, used to turn the Formation Box solid white, even during its Animations.
    private Shader shaderSpritesDefault;    //The GameObject's default Shader, used to restore it to its original color.

	// Use this for initialization
	protected override void Start()
    {
        //Call EnemyController's Start.
        base.Start();
        //Overwrite the EnemyController default values.
        hp = 15;
        SetSpeed(.03f);
        scoreValue = 200;
        
        //Calculate the distance lengths that the Formation Box will need to travel.
        width = hitbox.bounds.size.x;
        height = hitbox.bounds.size.y;
        diagDistance = Mathf.Sqrt(Mathf.Pow(width, 2) + Mathf.Pow(height, 2));
        //Calculate and set the 1st movement target for the Formation Box.
        lastTarget = transform.position;
        targetIndex = 0;
        moveTarget = new Vector2(transform.position.x - width, transform.position.y + height);
        //ChangeTarget(moveTarget);
        SetTarget(moveTarget);
        bulletSpeed = .6f;
        //Initialize the objects used for the Formation Box's "Hit Flash" effect.
        formboxRenderer = gameObject.GetComponent<SpriteRenderer>();
        shaderGUIText = Shader.Find("GUI/Text Shader");
        shaderSpritesDefault = Shader.Find("Sprites/Default");
        //Have the Formation Box start firing.
        StartCoroutine(FiringCooldown());
    }

    //Activate the Box's behaviors when it appears on screen.
    void OnTriggerEnter2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.AddEnemyToList(gameObject);
        }
        //Otherwise, check if this is a Player-controlled Bullet. 
        else if (box.gameObject.CompareTag("PlayerBullet"))
        {
            //If it is, make the Formation Box "flash" and make it invincible temporarily.
            if (hp > 0) { StartCoroutine(DamageCooldown()); }
            //Have the Formation Box take damage.
            if (!dmgImmune) { TakeDamage(box.gameObject.GetComponent<Bullet>().dmgValue); }
        }
        //Otherwise, check if this is an Explosion.
        else if (box.gameObject.CompareTag("Explosion"))
        {
            //Check if this explosion does damage.
            if (box.gameObject.GetComponent<Explosion>().isDamaging)
            {
                //If it is, make the Disc "flash" and make it invincible temporarily.
                if (hp > 0) { StartCoroutine(DamageCooldown()); }
                //Have the Disc take damage.
                if (!dmgImmune) { TakeDamage(box.gameObject.GetComponent<Explosion>().dmgValue); }
            }
        }
    }

    //Despawn the Box when it passes outside of the screen.
    void OnTriggerExit2D(Collider2D box)
    {
        //Check if this is the Collider surrounding the Camera view.
        if (box.gameObject.CompareTag("ScreenBox"))
        {
            LevelManager.instance.RemoveEnemyFromList(gameObject);
        }
    }

    //Handle the Formation Box's movement and firing.
    IEnumerator FiringCooldown()
    {
        while (hp > 0)
        {
            //If the target index is 0 or 2, the Box is moving diagonally.
            if (targetIndex == 0 || targetIndex == 2)
            {
                //Wait until the Box has traveled far enough from its last target before continuing.
                yield return new WaitWhile(() => Vector2.Distance(lastTarget, transform.position) < diagDistance);
            }
            //If the target index is 1 or 3, the Box is moving laterally.
            else if (targetIndex == 1 || targetIndex == 3)
            {
                //Wait until the Box has traveled far enough from its last target before continuing.
                yield return new WaitWhile(() => Vector2.Distance(lastTarget, transform.position) < width);
            }
            //Get the Player's location to use as the firing target.
            shotTarget = GameObject.FindGameObjectWithTag("Player").transform.position;
            //Create an instance of the Bullet that will appear in front of the Box (on the z-axis).
            GameObject bullet = Instantiate(boxBullet, transform.position + Vector3.back, Quaternion.identity);
            //Add the Bullet to the LevelManager's list.
            LevelManager.instance.AddBulletToList(bullet);
            //Assign the Bullet's target and speed.
            //boxBullet.GetComponent<Bullet>().ChangeTarget(shotTarget, 150f);
            bullet.GetComponent<Bullet>().SetTarget(shotTarget);
            bullet.GetComponent<Bullet>().SetSpeed(bulletSpeed);
            //Get the new movement target for the Formation Box.
            targetIndex++;
            targetIndex %= 4;
            UpdateTarget(targetIndex);
            //Reset the animation trigger to play the "rotation" again.
            GetComponent<Animator>().SetTrigger("deactivate");
            //Play the animation to show the Cube rotating to the left.
            GetComponent<Animator>().SetTrigger("activate");
        }
    }

    //Turn the Formation Box white, and make it invincible, for a very short period of time.
    IEnumerator DamageCooldown()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Make the Cube immune to damage while it is flashing.
        dmgImmune = true;
        //Make the Cube solid white to show that it has been hit.
        formboxRenderer.material.shader = shaderGUIText;
        formboxRenderer.color = Color.white;
        //Reset the Damage Timer.
        damageTimer = 0;
        //Let the Cube be invincible for 1/30 of a second.
        while (damageTimer < Time.deltaTime * 2)
        {
            //If the Game is paused, don't update the Damage Timer.
            if (!LevelManager.instance.gamePaused)
            {
                damageTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Let the Cube be susceptible to damage again.
        dmgImmune = false;
        //Change the Shader back to the default.
        formboxRenderer.material.shader = shaderSpritesDefault;
        formboxRenderer.color = Color.white;
    }

    //Change the destination vector based on where in its movement pattern the Box currently is.
    private void UpdateTarget(int index)
    {
        //Set the last movement target to the "start" point for this segment of the movement pattern.
        lastTarget = moveTarget;
        switch (index)
        {
            //Set the box to travel up and to the left.
            case 0:
                moveTarget = new Vector2(transform.position.x - width, transform.position.y + height);
                break;
            //Set the box to travel to the right.
            case 1: case 3: //Movement targets are relative to current position, so both Position 1 and 3 are represented the same way.
                moveTarget = new Vector2(transform.position.x + width, transform.position.y);
                break;
            //Set the box to travel down and to the left.
            case 2:
                moveTarget = new Vector2(transform.position.x - width, transform.position.y - height);
                break;
        }
        //Update the movement target in EnemyController.
        //ChangeTarget(moveTarget);
        SetTarget(moveTarget);
    }

    // Update is called once per frame
    protected override void Update()
    {
        //If the Game isn't paused, update the movement target Vectors.
        if(!LevelManager.instance.gamePaused)
        {
            lastTarget.Set(lastTarget.x + (BGManager.instance.scrollValues[0] + BGManager.instance.scrollOffsets[0]), lastTarget.y);
            moveTarget.Set(moveTarget.x + (BGManager.instance.scrollValues[0] + BGManager.instance.scrollOffsets[0]), moveTarget.y);
        }
        //Call EnemyController's Update.
        base.Update();
    }
}
