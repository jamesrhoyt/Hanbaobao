/*
 * LevelManager.cs
 * 
 * Manager for each Stage (gameplay level) in the Game. Updates the in-game HUD information in real-time, handles the
 * Game Over/High Score/Continue gameplay loop, and occassionally performs functions that require Stage-wide information for other Scripts.
 * 
 */

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance = null; //The LevelManager object that the rest of the Program can access.
                                                //There should be a new one of these every Stage.
    
    public BoxCollider2D screenEdges;   //The outer edges of the Game Window; used for despawning Objects that leave the Screen boundaries.
    private Fading fader;       //The object that handles fading the Screen into/out from black.
    public int stageNumber;     //The number of this Stage.
    public bool gamePaused;     //Whether or not the Game has been paused.
    private bool gameOver;      //Whether or not the Game is in the "Game Over" state.
    public bool inCutscene;     //Whether the Game is currently showing a cutscene (used mostly to disable Player boundary-checking).

    //Controls:
    public PlayerInput inputs;          //The Object that handles Player input in this Level.
    public InputAction action_up;       //The Action that represents the "Up" Button.
    public InputAction action_down;     //The Action that represents the "Down" Button.
    public InputAction action_left;     //The Action that represents the "Left" Button.
    public InputAction action_right;    //The Action that represents the "Right" Button.
    public InputAction action_A;        //The Action that represents the "A" Button.
    public InputAction action_B;        //The Action that represents the "B" Button.
    public InputAction action_C;        //The Action that represents the "C" Button.
    public InputAction action_Start;    //The Action that represents the "Start" Button.
    
    //Audio Elements:
    public AudioSource stageMusic;  //The AudioSource component that plays the Stage BGM.
    public AudioClip[] musicTracks; //The various BGM tracks that can play during the Stage.
    private float musicTime;        //The timestamp for the Stage BGM whenever it stops or restarts.

    //UI-Related Variables:
    public float introTimer;                //The time (in seconds) between when the Stage starts and when the Intro screens are done.
    private int highScore;                  //The value used to display/update "hiScoreText".
    private int multiplier;                 //The value by which all Player-accrued points are multiplied.
    private float multiTimer;               //The 30-second timer that moderates any Multiplier Bonus.
    private bool multiplierActive;          //Whether or not to display a Multiplier indicator in the UI (used to "blink" it as the timer runs low).
    public IEnumerator multiplierCoroutine; //The single instance of the "MultiplierTimer" Coroutine, which will get started and stopped by Item.cs.
    private float continueTimer;            //The 10-second timer that accompanies the Continue Screen.
    private int[] initialIndices = {0, 0, 0};   //The letter indices for each of the Initial Entry Fields.
    private int rank;   //The High Score rank a player has achieved on game over, to use in saving their score to the game files.

    //In-Game GameObjects:
    public GameObject player;   //The instance of the Player's ship present in this Stage.
    private List<GameObject> bulletsOnScreen;   //All of the Bullets, fired from either Player or Enemy, that are currently on-screen.
    private List<GameObject> enemiesOnScreen;   //All of the Enemy objects that are currently on-screen.
    private List<GameObject> explosionsOnScreen;    //All of the Explosions that are currently on-screen.

    //Enemy Wave System-Related Variables:
    private float stageTimer;           //The amount of elapsed time since the current section of the Level started (excluding Pauses, Boss Fights, and Cutscenes).
    public GameObject[] enemyWaves;     //The array of parent Objects that hold all of the Enemies in the first half of the Level.
    public float[] waveTimestamps;      //The amount of time (in seconds) that each Enemy wave should be spawned after the first half of the Level starts.
    private int waveNumber;             //The next Enemy Wave index to check the timestamp for.
    public int minibossWave;            //The Wave Number that holds the Miniboss, in order to know when the Miniboss fight has started.
    [SerializeField]
    private bool inStageBSide;          //Whether the Player is in the "B"-side of the Level or not, to know which array of Enemy Waves to use.
    public GameObject[] enemyWavesB;    //The array of parent Objects that hold all of the Enemies in the second half of the Level.
    public float[] waveTimestampsB;     //The amount of time (in seconds) that each Enemy wave should be spawned after the second half of the Level starts.
    public int bossWave;                //The Wave Number that holds the Boss, in order to know when the Boss fight has started.

    //Miniboss-Related Variables:
    public GameObject miniboss;         //The Boss at the halfway point of the Stage.
    public IEnumerator minibossFight;   //The Coroutine that manages the Miniboss fight.
    private float minibossFightTimer;   //The length of time (in seconds) that has elapsed during the Miniboss fight.
    public float minibossFightTimeLimit;//The maximum amount of time allowed for the Miniboss fight, after which it ends.
    private float minibossDeathTimer;   //The length of time (in seconds) that has elapsed during the Miniboss' death.
    public float minibossDeathLength;   //The amount of time to wait after the Miniboss dies before continuing.

    //Boss-Related Variables:
    public BoxCollider2D bossRoomMidFlag;   //A collision "flag" to know when to slow the Background scrolling down while entering the Boss Room.
    public BoxCollider2D bossRoomEndFlag;   //A collision "flag" to know when to stop the Background scrolling while entering the Boss Room.
    private float bossFightTransitionTimer; //A timer to space out the states of the transition into the Boss Fight.
    public GameObject boss;         //The Boss of the Stage.
    public bool inBossFight;        //Whether or not the Player is currently fighting the Boss (used for various conditional statements).

    //"End Of Stage" Variables:
    public int shotsFired;      //How many times the Player fired a Shot.
    public int enemiesHit;      //How many times a Player-fired Shot hit an Enemy.
    private float accuracy;     //The quotient of shotsFired and enemiesHit.
    public int enemiesKilled;   //How many Enemies have been killed this Stage (used for calculating the Damage Bonus).
    public bool lifeUsed;       //Whether the Player lost a Life this level.
    private bool bombUsed;      //Whether the Player used a Bomb this level.

    //Local Variables (Only used when testing a Level on its own):
    private int score = 0;
    public int lives = 3;
    public int continues = 2;
    public int bombs = 3;

	// Use this for initialization
	void Awake()
    {
        //Make the LevelManager a Singleton object.
        //if (instance == null)
        //{
            instance = this;
        /*}
        else if (instance != this)
        {
            Destroy(gameObject);
        }*/
        DontDestroyOnLoad(gameObject);
        //Bind each of the controls.
        action_up = inputs.actions["Up"];
        action_down = inputs.actions["Down"];
        action_left = inputs.actions["Left"];
        action_right = inputs.actions["Right"];
        action_A = inputs.actions["A Button"];
        action_B = inputs.actions["B Button"];
        action_C = inputs.actions["C Button"];
        action_Start = inputs.actions["Start Button"];
        //Set up the "fade" transition.
        fader = GetComponent<Fading>();
        //Set the Player object.
        player = GameObject.Find("Player");
        //Instantiate the "pause" and "gameOver" variables.
        gamePaused = false;
        gameOver = false;
        //Create the lists for Enemies, Bullets, and Explosions.
        bulletsOnScreen = new List<GameObject>();
        enemiesOnScreen = new List<GameObject>();
        explosionsOnScreen = new List<GameObject>();
        //Set up the Enemy wave management.
        stageTimer = 0f;
        waveNumber = 0;
        //inStageBSide = false; //Commented out to test Boss Fight progression.
        //Set the High Score equal to the top score saved in the Player Preferences.
        highScore = PlayerPrefs.GetInt("highscore0");
        UIManager.instance.UpdateHighScoreDisplay(highScore.ToString());
        //Set the Score Multiplier to its base value (1).
        multiplier = 1;
        multiplierActive = false;
        //Instantiate the Multiplier Coroutine, to prevent conlicts with Item.cs.
        multiplierCoroutine = MultiplierTimer(1);
        //Refresh the Lives, Bombs, and Score Text objects, for when transitioning from one Stage to another.
        //(Also updates the High Score Text object, if the Player has already passed the top Score.)
        UpdateLives();
        UpdateBombs();
        UpdateScores(0);
        //Reset the accuracy variables.
        shotsFired = 0;
        enemiesHit = 0;
        enemiesKilled = 0;
        //Set the "End Of Stage" variables.
        lifeUsed = false;
        bombUsed = false;
        //Instantiate the MinibossFight Coroutine instance for later.
        minibossFight = MinibossFight();
        //Instantiate the rest of the Miniboss-related variables (Some are Level-specific, and will be set via the Inspector.)
        minibossFightTimer = 0;
        minibossDeathTimer = 0;
        //Start playing the intro cutscene.
        StartCoroutine(PlayIntro());
    }

    //Handle the introductory part of the Stage.
    private IEnumerator PlayIntro()
    {
        yield return new WaitForEndOfFrame();
        //Let the UIManager run all of the Intro animations.
        StartCoroutine(UIManager.instance.PlayIntroScreen());
        yield return new WaitForSeconds(introTimer);
        StartCoroutine(player.GetComponent<ShipController>().EnterFromLeft(screenEdges.size.x / 4));
    }

    //Used to add Enemies to the collision checks.
    public void AddEnemyToList(GameObject enemy)
    {
        enemiesOnScreen.Add(enemy);
    }

    //Used to Remove Enemies from the collision checks.
    public void RemoveEnemyFromList(GameObject enemy)
    {
        enemiesOnScreen.Remove(enemy);
        enemy.SetActive(false);
        Destroy(enemy);
    }

    /// <summary>
    /// Find whichever Enemy on screen is closest to the Player (Used for the Player's Lightning weapon).
    /// </summary>
    /// <param name="direction">Whether to only look below the Player ("DOWN"), above the Player ("UP"), or anywhere ("ANY").</param>
    /// <returns>The closest Enemy.</returns>
    public GameObject FindClosestEnemyToPlayer(string direction)
    {
        Vector2 playerPos = player.transform.position;
        GameObject target = null;
        //Create a large float, so it gets overwritten during the distance checks.
        float minDist = 1000f;
        //Iterate through every Enemy to find the closest one.
        foreach (GameObject e in enemiesOnScreen.ToArray())
        {
            //Check the distance first.
            if (Vector2.Distance(playerPos, e.transform.position) < minDist)
            {
                //Then check if this Enemy is within the bounds specified.
                if (direction != "DOWN" && e.transform.position.y > playerPos.y)
                {
                    minDist = Vector2.Distance(playerPos, e.transform.position);
                    target = e;
                }
                else if (direction != "UP" && e.transform.position.y < playerPos.y)
                {
                    minDist = Vector2.Distance(playerPos, e.transform.position);
                    target = e;
                }
            }
        }
        return target;
    }

    //Used to add Player- and Enemy-fired bullets to the collision checks.
    public void AddBulletToList(GameObject bullet)
    {
        bulletsOnScreen.Add(bullet);
    }

    //Used to remove Player- and Enemy-fired bullets from the collision checks.
    public void RemoveBulletFromList(GameObject bullet)
    {
        bulletsOnScreen.Remove(bullet);
        bullet.SetActive(false);
        Destroy(bullet);
    }

    //Used to destroy any Player-fired Lightning when the Player releases the "Shoot" button.
    public void ClearLightning()
    {
        foreach (GameObject b in bulletsOnScreen.ToArray())
        {
            if (b != null && b.name.Contains("PlayerLightning"))
            {
                RemoveBulletFromList(b);
            }
        }
    }

    //Used to iterate through Explosions easier and toggle their Animations.
    public void AddExplosionToList(GameObject explosion)
    {
        explosionsOnScreen.Add(explosion);
    }

    //Used to remove and destroy Explosion once their Animations have played out.
    public void RemoveExplosionFromList(GameObject explosion)
    {
        explosionsOnScreen.Remove(explosion);
        explosion.SetActive(false);
        Destroy(explosion);
    }

    //Zero out the HP of all of the Enemies on the screen, and damage any Boss currently on-screen.
    public void DropBomb()
    {
        foreach (GameObject enemy in enemiesOnScreen.ToArray())
        {
            if (!enemy.GetComponent<EnemyController>().dmgImmune)
            {
                enemy.GetComponent<EnemyController>().TakeDamage(enemy.GetComponent<EnemyController>().hp);
            }
        }
        //Toggle the flag for the "No Bombs" bonus.
        bombUsed = true;
    }

	// Update is called once per frame
	void Update()
    {
        //If the Player presses the Start Button, pause/unpause the game.
        if (action_Start.triggered)
        {
            //Check that the Game isn't "over" currently, and that the Player isn't in a Cutscene.
            if (!gameOver && !inCutscene)
            {
                //Toggle the "gamePaused" variable.
                gamePaused = !gamePaused;
                //Toggle the Pause Screen.
                UIManager.instance.TogglePauseScreen(gamePaused);
                //Toggle all of the Animator components.
                ToggleAnimations(!gamePaused);
            }
        }

        //Manage the Enemy waves.
        //If the Game isn't paused, the Game isn't "over", or the Player isn't fighting a Boss/Miniboss, update the timer.
        if (!gamePaused && !gameOver && !inCutscene && !inBossFight)
        {
            stageTimer += Time.deltaTime;
            //Check which half of the Stage the Player is in.
            //"A"-Side:
            if (!inStageBSide)
            {
                //If the timer passes the timestamp for the next wave in the queue, spawn that wave.
                if (stageTimer >= waveTimestamps[waveNumber])
                {
                    //Only activate the Wave if it is not already active.
                    if (!enemyWaves[waveNumber].activeSelf)
                    {
                        enemyWaves[waveNumber].SetActive(true);
                        //If this is the Miniboss Wave, start the Miniboss Fight Coroutine.
                        if (waveNumber == minibossWave)
                        {
                            StartCoroutine(minibossFight);
                        }
                    }
                    //Check the wave number to prevent IndexOutOfBoundsExceptions.
                    if (waveNumber < enemyWaves.Length - 1)
                    {
                        waveNumber++;
                    }
                }
            }
            //"B"-Side:
            else
            {
                //If the timer passes the timestamp for the next wave in the queue, spawn that wave.
                if (stageTimer >= waveTimestampsB[waveNumber])
                {
                    //Only activate the Wave if it is not already active.
                    if (!enemyWavesB[waveNumber].activeSelf)
                    {
                        enemyWavesB[waveNumber].SetActive(true);
                        //If this is the Boss Wave, start the Boss Fight Coroutine.
                        if (waveNumber == bossWave)
                        {
                            StartCoroutine(TransitionToBossFight());
                        }
                    }
                    //Check the wave number to prevent IndexOutOfBoundsExceptions.
                    if (waveNumber < enemyWavesB.Length - 1)
                    {
                        waveNumber++;
                    }
                }
            }
        }
	}

    //Manage the Stage progression while the Player is fighting a Miniboss.
    public IEnumerator MinibossFight()
    {
        //Keep the gameplay loop running while the Miniboss still has health and the fight's time limit hasn't been reached.
        while (miniboss.GetComponent<Miniboss>().hp > 0 && minibossFightTimer < minibossFightTimeLimit)
        {
            //Pass each of the Bullets to the Miniboss to check collision against each of its components.
            foreach (GameObject bullet in bulletsOnScreen.ToArray())
            {
                //First, make sure the Player fired the Bullet.
                if (bullet.CompareTag("PlayerBullet"))
                {
                    //Second, see if the Bullet has hit any part of the Boss.
                    switch (miniboss.GetComponent<Miniboss>().CheckCollision(bullet.GetComponent<Collider2D>(), bullet.GetComponent<Bullet>().dmgValue))
                    {
                        //Case 1: Bullet hit a non-immune part of the Miniboss.
                        case 1:
                            //See if the Bullet should be destroyed as a result.
                            if (!bullet.name.Contains("PlayerBoomerang") && !bullet.name.Contains("PlayerLaser"))
                            {
                                RemoveBulletFromList(bullet);
                            }
                            //Increment the hit counter.
                            enemiesHit++;
                        break;
                        //Case 2: Bullet hit an immune part of the Miniboss.
                        case 2:
                            //See if the Bullet should be destroyed as a result.
                            if (!bullet.name.Contains("PlayerBoomerang") && !bullet.name.Contains("PlayerLaser"))
                            {
                                RemoveBulletFromList(bullet);
                            }
                            //Do not increment the hit counter.
                        break;
                    }
                }
            }
            //If the Game is paused, don't increase the Miniboss Fight Timer.
            if (!gamePaused)
            {
                minibossFightTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //If the fight ended due to the Miniboss dying, do the usual "fade" transition.
        if (miniboss.GetComponent<Miniboss>().hp <= 0)
        {
            //Let the Miniboss' death Animation play out.
            while (minibossDeathTimer < minibossDeathLength)
            {
                //Allow the Player to pause during the Miniboss' death without affecting the timing.
                if (!gamePaused)
                {
                    minibossDeathTimer += Time.deltaTime;
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //Start transitioning to the B-side of the Stage.
            StartCoroutine(TransitionToStageB());
        }
        //If the fight ended due to the timer running out, continue to the 2nd half of the Stage with no transition.
        else if (minibossFightTimer >= minibossFightTimeLimit)
        {
            //Reset the progression variables so the "B"-side of the Stage can use them.
            waveNumber = 0;
            stageTimer = 0;
            inStageBSide = true;
        }
    }

    //Send the Player off-screen, fade to black, move to the entrance of Part B, fade back in, and put the Player on-screen.
    public IEnumerator TransitionToStageB()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Disable the Player's controls for the Scene transition.
        inCutscene = true;
        player.GetComponent<ShipController>().controlsEnabled = false;
        //Zoom the Player ship off the right side of the Screen.
        StartCoroutine(player.GetComponent<ShipController>().ExitToRight());
        //Wait until the Player ship is about to leave the screen.
        yield return new WaitWhile(() => player.transform.position.x < screenEdges.bounds.max.x);
        //Start fading the screen to black.
        float fadeTime = fader.StartFade(1);
        //Wait until the screen has completely faded to black before continuing.
        yield return new WaitForSeconds(fadeTime);
        //Zoom the Player ship in from the left side of the Screen.
        StartCoroutine(player.GetComponent<ShipController>().EnterFromLeft(screenEdges.size.x / 4));
        //Start fading the screen back in.
        fadeTime = fader.StartFade(-1);
        //Wait until the screen has completely faded in before continuing.
        yield return new WaitForSeconds(fadeTime);
        //Reset the progression variables so the "B"-side of the Stage can use them.
        waveNumber = 0;
        stageTimer = 0;
        inStageBSide = true;
    }

    //Manage the Transitions and Object movement in the leadup to the Boss Fight.
    private IEnumerator TransitionToBossFight()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Start playing the Warning Screen.
        UIManager.instance.PlayWarningScreen();
        //Disable the Player's controls.
        player.GetComponent<ShipController>().controlsEnabled = false;
        //Reset the Boss Fight Transition Timer.
        bossFightTransitionTimer = 0;
        //Wait 1.5 seconds before sending the Player offscreen and "scrolling forward" to the Boss Room.
        while (bossFightTransitionTimer < 1.5)
        {
            //If the Game is paused, don't update the Timer.
            if (!gamePaused)
            {
                bossFightTransitionTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Start moving the Player Ship off the left side of the screen.
        StartCoroutine(player.GetComponent<ShipController>().BackAwayFromBossDoor());
        //Increase the speed of the level scrolling until the middle of the Boss Room is visible.
        while (!screenEdges.IsTouching(bossRoomMidFlag))
        {
            //If the Game is paused, don't increase the scroll speeds.
            if (!gamePaused)
            {
                BGManager.instance.AdjustScrollOffset(0, -.001f);
                BGManager.instance.AdjustScrollOffset(1, -.0008f);
                BGManager.instance.AdjustScrollOffset(2, -.0006f);
                BGManager.instance.AdjustScrollOffset(3, -.0004f);
                BGManager.instance.AdjustScrollOffset(4, -.0004f);
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Decrease the speed of the level scrolling until the entire Boss Room is visible.
        while (!screenEdges.IsTouching(bossRoomEndFlag))
        {
            //If the Game is paused, don't decrease the scroll speeds.
            if (!gamePaused)
            {
                BGManager.instance.AdjustScrollOffset(0, .00125f);
                BGManager.instance.AdjustScrollOffset(1, .001f);
                BGManager.instance.AdjustScrollOffset(2, .00075f);
                BGManager.instance.AdjustScrollOffset(3, .0005f);
                BGManager.instance.AdjustScrollOffset(4, .0005f);
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Stop all of the foreground and background layers in place.
        BGManager.instance.AdjustScrollOffset(0, -BGManager.instance.scrollValues[0] - BGManager.instance.scrollOffsets[0]);
        BGManager.instance.AdjustScrollOffset(1, -BGManager.instance.scrollValues[1] - BGManager.instance.scrollOffsets[1]);
        BGManager.instance.AdjustScrollOffset(2, -BGManager.instance.scrollValues[2] - BGManager.instance.scrollOffsets[2]);
        BGManager.instance.AdjustScrollOffset(3, -BGManager.instance.scrollValues[3] - BGManager.instance.scrollOffsets[3]);
        BGManager.instance.AdjustScrollOffset(4, -BGManager.instance.scrollValues[4] - BGManager.instance.scrollOffsets[4]);
        //Reset the Boss Fight Transition Timer.
        bossFightTransitionTimer = 0;
        //Wait another 1.5 seconds before sending the Player into the Room.
        while (bossFightTransitionTimer < 1.5)
        {
            //If the Game is paused, don't update the Timer.
            if (!gamePaused)
            {
                bossFightTransitionTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Have the Player Ship enter the Boss Room.
        StartCoroutine(player.GetComponent<ShipController>().EnterBossRoom());
        //Reset the Boss Fight Transition Timer.
        bossFightTransitionTimer = 0;
        //Wait another 1.5 seconds before starting the Boss' Intro Animation.
        while (bossFightTransitionTimer < 1.5)
        {
            //If the Game is paused, don't update the Timer.
            if (!gamePaused)
            {
                bossFightTransitionTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Start the Boss' intro animation.
        StartCoroutine(boss.GetComponent<Boss>().BossIntroAnimation());
        //Wait until the animation is complete before continuing.
        yield return new WaitWhile(() => boss.GetComponent<Boss>().bossIntroComplete == false);
        //Reset the Boss Fight Transition Timer.
        bossFightTransitionTimer = 0;
        //Wait another 3 seconds before starting the Boss Fight proper.
        while (bossFightTransitionTimer < 3)
        {
            //If the Game is paused, don't update the Timer.
            if (!gamePaused)
            {
                bossFightTransitionTimer += Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Re-enable the Player's controls.
        player.GetComponent<ShipController>().controlsEnabled = true;
        //Start the Boss Fight.
        StartCoroutine(BossFight());

    }

    //Manage the Boss Fight progression, and start the "end-of-Stage" proceedings when it dies.
    public IEnumerator BossFight()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Let the rest of the Program know the Boss Fight has started.
        inBossFight = true;
        //Keep the gameplay loop running while the Boss still has health.
        while (boss.GetComponent<Boss>().hp > 0)
        {
            //Pass each of the Bullets to the Boss to check collision against each of its components.
            foreach (GameObject bullet in bulletsOnScreen.ToArray())
            {
                //First, make sure the Player fired the Bullet.
                if (bullet.CompareTag("PlayerBullet"))
                {
                    //Second, see if the Bullet has hit any part of the Boss.
                    switch (boss.GetComponent<Boss>().CheckCollision(bullet.GetComponent<Collider2D>(), bullet.GetComponent<Bullet>().dmgValue))
                    {
                        //Case 1: Bullet hit a non-immune part of the Boss.
                        case 1:
                            //Third, see if the Bullet should be destroyed as a result.
                            if (!bullet.name.Contains("PlayerBoomerang") && !bullet.name.Contains("PlayerLaser"))
                            {
                                RemoveBulletFromList(bullet);
                            }
                            //Increment the hit counter.
                            enemiesHit++;
                        break;
                        //Case 2: Bullet hit an immune part of the Boss.
                        case 2:
                            //See if the Bullet should be destroyed as a result.
                            if (!bullet.name.Contains("PlayerBoomerang") && !bullet.name.Contains("PlayerLaser"))
                            {
                                RemoveBulletFromList(bullet);
                            }
                            //Do not increment the hit counter.
                        break;
                    }
                }
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Put the game in Cutscene Mode for the end of the Stage.
        inCutscene = true;
        //Let the Boss' death Animation play out.
        StartCoroutine(boss.GetComponent<Boss>().BossDeathAnimation());
        yield return new WaitUntil(() => boss.GetComponent<Boss>().bossDeathComplete);
        //Start up the "Stage Clear" Screen.
        StartCoroutine(EndStage());
    }

    /// <summary>
    /// Enable/disable the Animator Components for all of the on-screen gameObjects.
    /// </summary>
    /// <param name="active">Whether the Animators should be active (true) or not (false).</param>
    private void ToggleAnimations(bool active)
    {
        //Iterate through all of the Enemies on screen.
        foreach (GameObject enemy in enemiesOnScreen.ToArray())
        {
            //Disabling the Animator Component will pause any Animation.
            //Enabling the Animator Component will resume the Animation where it left off.
            if (enemy.GetComponent<Animator>() != null) { enemy.GetComponent<Animator>().enabled = active; }
        }
        //Iterate through all of the Bullets on screen.
        foreach (GameObject bullet in bulletsOnScreen.ToArray())
        {
            //Disabling the Animator Component will pause any Animation.
            //Enabling the Animator Component will resume the Animation where it left off.
            if (bullet.GetComponent<Animator>() != null) { bullet.GetComponent<Animator>().enabled = active; }
        }
        //Iterate through all of the Explosions on screen.
        foreach (GameObject explosion in explosionsOnScreen.ToArray())
        {
            //Disabling the Animator Component will pause any Animation.
            //Enabling the Animator Component will resume the Animation where it left off.
            if (explosion.GetComponent<Animator>() != null) { explosion.GetComponent<Animator>().enabled = active; }
        }
        //Toggle the Player's Animator Components.
        player.GetComponent<ShipController>().ToggleAnimations(active);
        //Toggle any Animator Components the Miniboss may have.
        miniboss.GetComponent<Miniboss>().ToggleAnimations(active);
    }

    //Stop any currently-running instance of MultiplierTimer, then start a new one.
    public void StartMultiplier(int multi)
    {
        //If the Coroutine object has been set, stop it in case there is still an instance in progress.
        if(multiplierCoroutine != null)
        {
            StopCoroutine(multiplierCoroutine);
        }
        //Set the Coroutine object to a new instance, then run it.
        multiplierCoroutine = MultiplierTimer(multi);
        StartCoroutine(multiplierCoroutine);
    }

    //Multiply every achieved point value by a given multiplier for 30 seconds.
    public IEnumerator MultiplierTimer(int multi)
    {
        //Set the new Multiplier value.
        multiplier = multi;
        //Set the value to display the Multiplier Indicator.
        multiplierActive = true;
        //Set the Multiplier Text's value and enable it.
        UIManager.instance.SetMultiplierSprite(multi);
        UIManager.instance.ToggleMultiplierDisplay(multiplierActive);
        //Start the Multiplier Timer.
        multiTimer = 30f;
        //Have the Timer run for the first twenty seconds.
        while (multiTimer > 10.0f)
        {
            //If the Game is paused, don't decrease the Multiplier Timer.
            if (!gamePaused)
            {
                multiTimer -= Time.deltaTime;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Have the Timer blink on and off every half-second for the next 5 seconds.
        float blinkTimer = 0f;
        while (multiTimer > 5.0f)
        {
            multiplierActive = !multiplierActive;
            UIManager.instance.ToggleMultiplierDisplay(multiplierActive);
            //Run the Blink Timer for half a second.
            while (blinkTimer < 0.5f)
            {
                //If the Game is paused, don't decrease the Multiplier Timer or increase the Blink Timer.
                if (!gamePaused)
                {
                    multiTimer -= Time.deltaTime;
                    blinkTimer += Time.deltaTime;
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //Reset the Blink Timer.
            blinkTimer = 0f;
        }
        //Have the Timer blink on and off every quarter-second for the last 5 seconds.
        while (multiTimer > 0f)
        {
            multiplierActive = !multiplierActive;
            UIManager.instance.ToggleMultiplierDisplay(multiplierActive);
            //Run the Blink Timer for a quarter of a second.
            while (blinkTimer < 0.25f)
            {
                //If the Game is paused, don't decrease the Multiplier Timer or increase the Blink Timer.
                if (!gamePaused)
                {
                    multiTimer -= Time.deltaTime;
                    blinkTimer += Time.deltaTime;
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //Reset the Blink Timer.
            blinkTimer = 0f;
        }
        //Disable the Multiplier Text (in case it ended enabled) and reset the Multiplier to 1.
        multiplierActive = false;
        UIManager.instance.ToggleMultiplierDisplay(multiplierActive);
        multiplier = 1;
    }

    //Update the Score (and Hi-Score) on screen when points are acquired.
    public void UpdateScores(int value)
    {
        //If the Level has been loaded from the Title Screen, use the GameManager's variables.
        try
        {
            //Update the score proper.
            GameManager.instance.score += (value * multiplier);
            //Update the Text object representing the Score.
            UIManager.instance.UpdateScoreDisplay(GameManager.instance.score.ToString().PadLeft(6, '0'));
            //Check if the current Hi-Score is less than the Player's current score.
            if (highScore < GameManager.instance.score)
            {
                //Change the Hi-Score to the current score.
                highScore = GameManager.instance.score;
                //Update its Text object to match.
                UIManager.instance.UpdateHighScoreDisplay(highScore.ToString());
            }
        }
        //If this level is being tested standalone, there will be no GameManager. Use the local variables in this case.
        catch (System.NullReferenceException)
        {
            //Update the score proper.
            score += (value * multiplier);
            //Update the Text object representing the Score.
            UIManager.instance.UpdateScoreDisplay(score.ToString().PadLeft(6, '0'));
            //Check if the current Hi-Score is less than the Player's current score.
            if (highScore < score)
            {
                //Change the Hi-Score to the current score.
                highScore = score;
                //Update its Text object to match.
                UIManager.instance.UpdateHighScoreDisplay(highScore.ToString());
            }
        }
    }

    //Update the Lives Counter in the HUD.
    public void UpdateLives()
    {
        //If the Level has been loaded from the Title Screen, use the GameManager's variables.
        try
        {
            //If the "Infinite Lives" Cheat is enabled, display a "9".
            if (GameManager.instance.cheat_infiniteLivesEnabled)
            {
                UIManager.instance.UpdateLivesDisplay(9);
            }
            //Use the "lives" value, minus one. When the display reads "0", the Player is on their last Life.
            else
            {
                UIManager.instance.UpdateLivesDisplay(GameManager.instance.lives - 1);
            }
        }
        //If this level is being tested standalone, there will be no GameManager. Use the local variables in this case.
        catch (System.NullReferenceException)
        {
            UIManager.instance.UpdateLivesDisplay(lives - 1);
        }
    }

    //Update the Bombs Counter in the HUD.
    public void UpdateBombs()
    {
        //If the Level has been loaded from the Title Screen, use the GameManager's variables.
        try
        {
            //If the "Infinite Bombs" Cheat is enabled, display a "9".
            if (GameManager.instance.cheat_infiniteBombsEnabled)
            {
                UIManager.instance.UpdateBombsDisplay(9);
            }
            else
            {
                UIManager.instance.UpdateBombsDisplay(GameManager.instance.bombs);
            }
        }
        //If this level is being tested standalone, there will be no GameManager. Use the local variables in this case.
        catch (System.NullReferenceException)
        {
            UIManager.instance.UpdateBombsDisplay(bombs);
        }
    }

    //Run the Game Over functions when the Player runs out of Lives.
    public IEnumerator GameOver()
    {
        //Pause the game.
        gamePaused = true;
        //Signify the Game is "over" (to prevent unpausing during the Game Over Screen).
        gameOver = true;
        //Disable all of the Animators.
        ToggleAnimations(false);
        //Enable the Game Over Text.
        UIManager.instance.ToggleGameOverScreen(true);
        //Get the current timestamp for the Stage Music.
        musicTime = stageMusic.time;
        //Play the Game Over Music.
        stageMusic.clip = musicTracks[1];
        stageMusic.time = 0;
        stageMusic.Play();
        //Wait for the length of the Game Over Music before continuing.
        yield return new WaitForSeconds(stageMusic.clip.length);
        //Check the score.
        //Use GameManager's variable if it is available.
        if (GameManager.instance != null)
        {
            //If the Player's score is greater than or equal to the Rank 20 score, set up the High Score Entry state.
            if (GameManager.instance.score >= PlayerPrefs.GetInt("highscore19"))
            {
                HighScoreEntry();
            }
            //Otherwise, set up the Continue state.
            else
            {
                //Disable the Game Over Text.
                UIManager.instance.ToggleGameOverScreen(false);
                //Start the Continue Screen.
                StartCoroutine(Continue());
            }
        }
        //Use LevelManager's local variable if there is no GameManager.
        else
        {
            //If the Player's score is greater than or equal to the Rank 20 score, set up the High Score Entry state.
            if (score >= PlayerPrefs.GetInt("highscore19"))
            {
                HighScoreEntry();
            }
            //Otherwise, set up the Continue state.
            else
            {
                //Disable the Game Over Text.
                UIManager.instance.ToggleGameOverScreen(false);
                //Start the Continue Screen.
                StartCoroutine(Continue());
            }
        }
    }

    //Control the High Score Entry screen after the Player gets a Game Over.
    private void HighScoreEntry()
    {
        //Find the Rank that the Player achieved, by comparing their score each one up the list.
        rank = 19;
        try
        {
            while (GameManager.instance.score >= PlayerPrefs.GetInt("highscore" + rank) && rank > -1)
            {
                rank -= 1;
            }
        }
        //Use the local score if GameManager isn't loaded.
        catch (System.NullReferenceException)
        {
            while (score >= PlayerPrefs.GetInt("highscore" + rank) && rank > -1)
            {
                rank -= 1;
            }
        }
        //If the rank goes to -1, the Player got the High Score.
        //Disable the Game Over Text.
        UIManager.instance.ToggleGameOverScreen(false);
        //Set the Text objects for the High Score Entry Screen.
        try
        {
            UIManager.instance.SetHighScoreScreen((rank + 2).ToString().PadLeft(2, '0'), GameManager.instance.score.ToString().PadLeft(6, '0'), (char)stageNumber);
        }
        catch (System.NullReferenceException)
        {
            UIManager.instance.SetHighScoreScreen((rank + 2).ToString().PadLeft(2, '0'), score.ToString().PadLeft(6, '0'), (char)stageNumber);
        }
        //Enable the Text objects for the High Score Entry screen.
        UIManager.instance.ToggleHighScoreScreen(true);
        //Play the High Score Music.
        stageMusic.clip = musicTracks[2];
        stageMusic.time = 0;
        stageMusic.Play();
        //Start letting the Player enter their initials.
        StartCoroutine(EnterInitial(0));
    }

    //Allow the Player to change/enter one initial of the High Score Entry screen at a time.
    private IEnumerator EnterInitial(int initialIndex)
    {
        //Set/retrieve the current letter for this Initials object.
        int currentChar = initialIndices[initialIndex];
        UIManager.instance.SetInitialsText(initialIndex, currentChar);
        UIManager.instance.ToggleInitials(initialIndex, true);
        //Start making the current letter "blink".
        IEnumerator blink = UIManager.instance.BlinkInitial(initialIndex);
        StartCoroutine(blink);
        //Wait until the current frame ends, to prevent multiple button registers.
        yield return new WaitForEndOfFrame();
        //Keep checking for Player input until the current letter is confirmed/canceled.
        while (true)
        {
            //Switch to the previous character in the List.
            if (action_up.triggered || action_left.triggered)
            {
                //Decrement the character index.
                currentChar--;
                //If the character index is -1, wrap around to the end of the list.
                if(currentChar < 0) currentChar = 59;
                //Update the Text object.
                UIManager.instance.SetInitialsText(initialIndex, currentChar);
            }
            //Switch to the next character in the List.
            if (action_down.triggered || action_right.triggered)
            {
                //Increment the character index.
                currentChar++;
                //Mod the character index, in case it increases beyond the list size.
                currentChar %= 60;
                //Update the Text object.
                UIManager.instance.SetInitialsText(initialIndex, currentChar);
            }
            //Confirm the current character, and start entering the next one.
            if (action_A.triggered || action_C.triggered)
            {
                //Save the new Initial in the array.
                initialIndices[initialIndex] = currentChar;
                //Stop the current Initial from blinking.
                StopCoroutine(blink);
                //Make it active (in case it was inactive when the "blink" was stopped.
                UIManager.instance.ToggleInitials(initialIndex, true);
                //Check that the Player isn't on the 3rd Initial.
                if (initialIndex < 2)
                {
                    //Start a new instance of the Coroutine for the next Initial.
                    StartCoroutine(EnterInitial(initialIndex + 1));
                }
                //If the Player is on the 3rd Initial, move to the Confirmation state instead.
                else
                {
                    //Allow the Player to confirm/cancel their Initials.
                    StartCoroutine(ConfirmInitials());
                }
                //Stop this instance of the Coroutine.
                yield break;
            }
            //Back up to the previous character entry.
            if (action_B.triggered)
            {
                if (initialIndex > 0)
                {
                    StopCoroutine(blink);
                    UIManager.instance.ToggleInitials(initialIndex, false);
                    StartCoroutine(EnterInitial(initialIndex - 1));
                    yield break;
                }
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    //Allow the Player to save their High Score, or back out to change their last initial.
    private IEnumerator ConfirmInitials()
    {
        //Show the "ED" character after the Initials.
        UIManager.instance.ToggleInitialsEnd(true);
        //Wait until the current frame ends, to prevent multiple button registers.
        yield return new WaitForEndOfFrame();
        while (true)
        {
            //If the A or C Buttons are pressed, save the High Score.
            if (action_A.triggered || action_C.triggered)
            {
                //Hide the "ED" character.
                UIManager.instance.ToggleInitialsEnd(false);
                //Save the Player's High Score.
                SaveScore();
                //Start showing the Initials' confirmation "shimmer".
                StartCoroutine(UIManager.instance.ShimmerInitials());
                //Stop this Coroutine.
                yield break;
            }
            //If the B Button is pressed, go back to the 3rd Initial.
            else if (action_B.triggered)
            {
                //Hide the "ED" character.
                UIManager.instance.ToggleInitialsEnd(false);
                //Start the Entry Coroutine for the 3rd Initial.
                StartCoroutine(EnterInitial(2));
                //Stop this Coroutine.
                yield break;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    //Save the Player's high score to the High Score Table.
    private void SaveScore()
    {
        //Increase rank by one to match the numbering convention of the saved High Scores.
        rank++;
        //Move all of the High Score listings beneath the Player's current one down by one.
        for (int i = 19; i > rank; i--)
        {
            PlayerPrefs.SetInt("highscore" + i, PlayerPrefs.GetInt("highscore" + (i - 1)));
            PlayerPrefs.SetString("highscoreInitials" + i, PlayerPrefs.GetString("highscoreInitials" + (i - 1)));
            PlayerPrefs.SetString("highscoreStage" + i, PlayerPrefs.GetString("highscoreStage" + (i - 1)));
        }
        //Save the Player's current High Score.
        try
        {
            PlayerPrefs.SetInt("highscore" + rank, GameManager.instance.score);
        }
        catch (System.NullReferenceException)
        {
            PlayerPrefs.SetInt("highscore" + rank, score);
        }
        PlayerPrefs.SetString("highscoreInitials" + rank, UIManager.instance.letterList[initialIndices[0]].ToString() + UIManager.instance.letterList[initialIndices[1]].ToString() + UIManager.instance.letterList[initialIndices[2]].ToString());
        PlayerPrefs.SetString("highscoreStage" + rank, stageNumber.ToString());
        PlayerPrefs.Save();
    }

    //Give the Player a 10-second timer to continue the game, or go back to the Title Screen.
    public IEnumerator Continue()
    {
        //Play the Continue Music.
        stageMusic.clip = musicTracks[3];
        stageMusic.time = 0;
        stageMusic.Play();
        //Set the Continue Timer.
        continueTimer = 10f;
        //Use the GameManager's variable if it is present.
        if(GameManager.instance != null)
        {
            //If the Player has a Continue left (or has "Infinite Continues" enabled), start the Continue screen.
            if (GameManager.instance.continues > 0 || GameManager.instance.cheat_infiniteContinuesEnabled)
            {
                //Update the Continue Text to reflect the number of Continues remaining.
                if (!GameManager.instance.cheat_infiniteContinuesEnabled)
                {
                    UIManager.instance.SetContinueText(GameManager.instance.continues);
                }
                else
                {
                    UIManager.instance.SetContinueText(9);
                }
                //Enable the Continue Text.
                UIManager.instance.ToggleContinueScreen(true);
                while (continueTimer > 0)
                {
                    //If the Start Button is pressed, continue the game.
                    if (action_Start.triggered)
                    {
                        //Remove one continue.
                        GameManager.instance.continues--;
                        //Reset the number of Lives the Player has.
                        GameManager.instance.lives = GameManager.instance.startingLives;
                        UpdateLives();
                        //Reset the number of Bombs the Player has.
                        GameManager.instance.bombs = 3;
                        UpdateBombs();
                        //Reset the Player's Score.
                        UpdateScores(-GameManager.instance.score);
                        //Bring the Player's ship back to life.
                        player.GetComponent<ShipController>().isAlive = true;
                        //Make the Player's ship invincible.
                        StartCoroutine(player.GetComponent<ShipController>().Invincibility());
                        //Disable the Continue Text.
                        UIManager.instance.ToggleContinueScreen(false);
                        //Signify that the game isn't "over" anymore (re-enabling the Pause Button).
                        gameOver = false;
                        //Unpause the Game, allowing it to continue.
                        gamePaused = false;
                        //Re-enable all of the Animators.
                        ToggleAnimations(true);
                        //Restart the Stage Music where it left off.
                        stageMusic.clip = musicTracks[0];
                        stageMusic.Play();
                        stageMusic.time = musicTime;
                        //End the Continue Coroutine.
                        yield break;
                    }
                    //If any of the Face Buttons are pressed, drop one second from the timer.
                    else if (action_A.triggered || action_B.triggered || action_C.triggered)
                    {
                        //Only do this after the timer has elapsed for three seconds.
                        if (continueTimer < 7)
                        {
                            continueTimer -= 1;
                        }
                    }
                    //Decrement the Timer every frame.
                    continueTimer -= Time.deltaTime;
                    //Update the on-screen timer.
                    UIManager.instance.SetContinueTimer((int)continueTimer);
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                //If the Timer runs out, go back to the Title Screen.
                StartCoroutine(ChangeScene("IntroTitle"));
            }
            //Otherwise, go back to the Title Screen.
            else
            {
                StartCoroutine(ChangeScene("IntroTitle"));
            }
            //If the player has an impossible number of Continues (-1 or lower), they used a Cheat and
            //their Achievements should be disabled until the game is reset, or the cheat is disabled.
            if (GameManager.instance.continues < 0 && GameManager.instance.cheat_infiniteContinuesEnabled)
            {
                GameManager.instance.achievementsEnabled = false;
            }
        }
        //Otherwise, use LevelManager's local variable.
        else
        {
            //If the Player has a Continue left, start the Continue screen.
            if (continues > 0)
            {
                //Update the Continue Text to reflect the number of Continues remaining.
                UIManager.instance.SetContinueText(continues);
                //Enable the Continue Text.
                UIManager.instance.ToggleContinueScreen(true);
                //Countdown the Timer while checking for Player input.
                while (continueTimer > 0)
                {
                    //If the Start Button is pressed, continue the game.
                    if (action_Start.triggered)
                    {
                        //Remove one continue.
                        continues--;
                        //Reset the number of Lives the Player has.
                        lives = 3;
                        UpdateLives();
                        //Reset the number of Bombs the Player has.
                        bombs = 3;
                        UpdateBombs();
                        //Reset the Player's Score.
                        UpdateScores(-score);
                        //Bring the Player's ship back to life.
                        player.GetComponent<ShipController>().isAlive = true;
                        //Make the Player's ship invincible.
                        StartCoroutine(player.GetComponent<ShipController>().Invincibility());
                        //Disable the Continue Text.
                        UIManager.instance.ToggleContinueScreen(false);
                        //Signify that the game isn't "over" anymore (re-enabling the Pause Button).
                        gameOver = false;
                        //Unpause the Game, allowing it to continue.
                        gamePaused = false;
                        //Restart the Stage Music where it left off.
                        stageMusic.clip = musicTracks[0];
                        stageMusic.Play();
                        stageMusic.time = musicTime;
                        //End the Continue Coroutine.
                        yield break;
                    }
                    //If any of the Face Buttons are pressed, drop one second from the timer.
                    else if (action_A.triggered || action_B.triggered || action_C.triggered)
                    {
                        //Only do this after the timer has elapsed for three seconds.
                        if (continueTimer < 7)
                        {
                            continueTimer -= 1;
                        }
                    }
                    //Decrement the Timer every frame.
                    continueTimer -= Time.deltaTime;
                    //Update the on-screen timer.
                    UIManager.instance.SetContinueTimer((int)continueTimer);
                    yield return new WaitForSeconds(Time.deltaTime);
                }
                //If the Timer runs out, go back to the Title Screen.
                StartCoroutine(ChangeScene("IntroTitle"));
            }
            //Otherwise, go back to the Title Screen.
            else
            {
                StartCoroutine(ChangeScene("IntroTitle"));
            }
        }
    }

    //Display all of the "End-of-Stage" Bonuses, and add them to the Score total.
    private IEnumerator EndStage()
    {
        //Save the Player's current weapon, and all of their power levels.
        try
        {
            GameManager.instance.weaponIndex = player.GetComponent<ShipController>().weaponIndex;
            GameManager.instance.weaponLevels = player.GetComponent<ShipController>().weaponLevels;
        }
        catch (System.NullReferenceException)
        {
        }

        //Move all of the Stage Clear Text objects into place.
        for (int i = 0; i < UIManager.instance.clearLetters.Length; i++)
        {
            //Change the position incrementation based on whether the letter is before the space or after.
            if (i < 5)
            {
                StartCoroutine(UIManager.instance.DisplayStageClearBanner(i, -99 + (18 * i)));
            }
            else
            {
                StartCoroutine(UIManager.instance.DisplayStageClearBanner(i, -81 + (18 * i)));
            }
            yield return new WaitForSeconds(.25f);
        }

        //Wait half a second.
        yield return new WaitForSeconds(.5f);

        //Send the Player offscreen.
        StartCoroutine(player.GetComponent<ShipController>().ExitToRight());

        //Fade the Background incrementally until it is invisible.
        UIManager.instance.pauseBackground.GetComponent<CanvasRenderer>().SetAlpha(0);
        UIManager.instance.pauseBackground.gameObject.SetActive(true);
        float alphaStep = 0f;
        while (UIManager.instance.pauseBackground.GetComponent<CanvasRenderer>().GetAlpha() < 1)
        {
            //Increase the alpha step by .2.
            alphaStep += .2f;
            //Update the Background's alpha value.
            UIManager.instance.pauseBackground.GetComponent<CanvasRenderer>().SetAlpha(alphaStep);
            //Wait a quarter of a second before decreasing the alpha again.
            yield return new WaitForSeconds(.25f);
        }

        //Move the Stage Clear objects up to headline the Bonuses.
        /*while (clearLetters[0].transform.position.y < 778)
        {
            foreach (Text t in clearLetters)
            {
                t.transform.Translate(0, 4, 0, Space.World);
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }*/

        //Clear Bonus Calculation:
        #region Clear Bonus Calculation
            //Calculate the Clear Bonus Value.
            int clearBonus = stageNumber * 10000;
            UIManager.instance.SetClearBonusText(clearBonus);
            //Activate the "Clear Bonus" Text Objects.
            UIManager.instance.ToggleClearBonus(true);
            //Quickly count down the Bonus Points, and add them to the Score.
            while (clearBonus > 0)
            {
                //If there are still at least 200 points in the Clear Bonus, remove 200 points.
                if (clearBonus - 200 >= 0)
                {
                    UpdateScores(200);
                    clearBonus -= 200;
                }
                //Otherwise, just zero the Bonus out.
                else
                {
                    UpdateScores(clearBonus);
                    clearBonus = 0;
                }
                //Update the Bonus number text to match the new value.
                UIManager.instance.SetClearBonusText(clearBonus);
                //Wait for 1/30 seconds before subtracting another point amount.
                yield return new WaitForSeconds(Time.deltaTime * 2);
            }
        #endregion

        yield return new WaitForSeconds(.5f);

        //Accuracy Bonus Calculation:
        #region Accuracy Bonus Calculation
            //Calculate the accuracy value.
            accuracy = (float)enemiesHit / (float)shotsFired;
            //Round the accuracy value to the thousandths place.
            accuracy = (float)Mathf.RoundToInt(accuracy * 100.0f) / 100.0f;
            //Calculate the Accuracy Bonus Value.
            int accuracyBonus = (int)(accuracy * 10000f);
            //Display the Accuracy as a percentage, to the tenths place (e.g., XX.X%).
            UIManager.instance.SetAccuracyLabelText(accuracy * 100.0f);
            UIManager.instance.SetAccuracyBonusText(accuracyBonus);
            //Activate the "Accuracy Bonus" Text Objects.
            UIManager.instance.ToggleAccuracyBonus(true);
            //Quickly count down the Bonus Points, and add them to the Score.
            while (accuracyBonus > 0)
            {
                //If there are still at least 200 points in the Accuracy Bonus, remove 200 points.
                if (accuracyBonus - 200 >= 0)
                {
                    UpdateScores(200);
                    accuracyBonus -= 200;
                }
                //Otherwise, just zero the Bonus out.
                else
                {
                    UpdateScores(accuracyBonus);
                    accuracyBonus = 0;
                }
                //Update the Bonus number text to match the new value.
                UIManager.instance.SetAccuracyBonusText(accuracyBonus);
                //Wait for 1/30 seconds before subtracting another point amount.
                yield return new WaitForSeconds(Time.deltaTime * 2);
            }
        #endregion

        yield return new WaitForSeconds(.5f);

        //Damage Bonus Calculation:
        #region Damage Bonus Calculation
            //Calculate the Damage Bonus Value.
            int damageBonus = enemiesKilled * 100;
            UIManager.instance.SetDamageBonusText(damageBonus);
            //Activate the "Clear Bonus" Text Objects.
            UIManager.instance.ToggleDamageBonus(true);
            //Quickly count down the Bonus Points, and add them to the Score.
            while (damageBonus > 0)
            {
                //If there are still at least 30 points in the Accuracy Bonus, remove 30 points.
                if (damageBonus - 200 >= 0)
                {
                    UpdateScores(200);
                    damageBonus -= 200;
                }
                //Otherwise, just zero the Bonus out.
                else
                {
                    UpdateScores(damageBonus);
                    damageBonus = 0;
                }
                //Update the Bonus number text to match the new value.
                UIManager.instance.SetDamageBonusText(damageBonus);
                //Wait for 1/30 seconds before subtracting another point amount.
                yield return new WaitForSeconds(Time.deltaTime * 2);
            }
        #endregion

        yield return new WaitForSeconds(.5f);

        //Determine if the Player gets the "Perfect Aim" Bonus.
        //If the Player's accuracy was 100% (or better), they get the Bonus.
        if (accuracy >= 1)
        {
            //Show the "Perfect Aim!" message.
            UIManager.instance.TogglePerfectAimBonus(true);
            //Give the Player 10,000 points.
            UpdateScores(10000);
        }

        //Determine if the Player gets the "No Miss" Bonus.
        //If the Player didn't lose a Life this Stage, they get the Bonus.
        if (!lifeUsed)
        {
            yield return new WaitForSeconds(2f);
            //Hide the "Perfect Aim!" message.
            UIManager.instance.TogglePerfectAimBonus(false);
            //Show the "No Miss!" message.
            UIManager.instance.ToggleNoMissBonus(true);
            //Give the Player 10,000 points.
            UpdateScores(10000);
        }

        //Determine if the Player gets the "No Bombs" Bonus.
        //If the Player didn't use a Bomb this Stage, they get the Bonus.
        if (!bombUsed)
        {
            yield return new WaitForSeconds(2f);
            //Hide the "Perfect Aim!" message.
            UIManager.instance.TogglePerfectAimBonus(false);
            //Hide the "No Miss!" message.
            UIManager.instance.ToggleNoMissBonus(false);
            //Show the "No Bombs!" message.
            UIManager.instance.ToggleNoBombsBonus(true);
            //Give the Player 5,000 points.
            UpdateScores(5000);
        }

        yield return new WaitForSeconds(3f);

        //If this isn't the last Stage, load the next one in the Game.
        if (stageNumber < 5)
        {
            StartCoroutine(ChangeScene("stage" + (stageNumber+1)));
        }
    }

    //Fade to black and transition to another Scene (either the next Stage, or the Title Screen).
    /// <param name="sceneName">The name of the new Scene to load.</param>
    private IEnumerator ChangeScene(string sceneName)
    {
        //Start fading the screen to black.
        float fadeTime = fader.StartFade(1);
        //Wait until the screen has completely faded to black before continuing.
        yield return new WaitForSeconds(fadeTime);
        //Load the new Scene while unloading any others, for efficiency.
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        //Disable this instance of the LevelManager.
        instance.enabled = false;
    }
}
