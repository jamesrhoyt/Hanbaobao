/*
 * GameManager.cs
 * 
 * Handles the overall management of the Game, including managing the Introduction and Title Screen, 
 * loading the Player's language preferences, and transitioning between Stages when the Game has started.
 * 
 */

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager instance = null;  //The GameManager Object that the rest of the Program can access.

    //Controls:
    public PlayerInput inputs;          //The Object that handles Player input in the game proper.
    public InputAction action_up;       //The Action that represents the "Up" Button.
    public InputAction action_down;     //The Action that represents the "Down" Button.
    public InputAction action_left;     //The Action that represents the "Left" Button.
    public InputAction action_right;    //The Action that represents the "Right" Button.
    public InputAction action_A;        //The Action that represents the "A" Button.
    public InputAction action_B;        //The Action that represents the "B" Button.
    public InputAction action_C;        //The Action that represents the "C" Button.
    public InputAction action_Start;    //The Action that represents the "Start" Button.

    //Gameplay Variables:
    public int lives = 3;
    public int startingLives = 3;   //The number of Lives the Player will receive from a Continue.
    public int continues = 2;
    public int bombs = 3;
    public int score = 250000;
    public int buttonConfig = 1;
    public int[] weaponLevels;      //The power levels for each of the weapons that the Player can have equipped.
    public int weaponIndex;         //The index of the current weapon that the Player has equipped.

    //Scene Objects:
    public Image introBackground;
    public Image subtitleBackground;
    public Text startText;
    public Text optionsText;
    public Image menuCursor;
    public GameObject titleLogo;

    private string language;    //A string representing the language the Player has chosen.
    private bool menuActive;    //Whether the Title Screen is active and accepting input.
    private int selectedMenuItem;   //Which item of the Title Screen ("Start" or "Options") is currently selected.
    private Fading fader;   //The object that handles fading the Screen into/out from black.

    //Cheat Code Toggles:
    public bool cheat_infiniteLivesEnabled;     //Whether the "Infinite Lives" Cheat Code has been enabled via the Sound Test.
    public bool cheat_infiniteContinuesEnabled; //Whether the "Infinite Continues" Cheat Code has been enabled via the Sound Test.
    public bool cheat_infiniteBombsEnabled;     //Whether the "Infinite Bombs" Cheat Code has been enabled via the Sound Test.
    public bool achievementsEnabled;            //Whether or not achievements should be unlocked for this run of the game.
                                                //(Achievements are disabled if the player entered a Cheat, *and* that Cheat affected their progress.)

	// Use this for initialization
	void Awake()
    {
        //Make the GameManager a Singleton object.
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
        //Set/Load the player's language preference.
        InitializeText();
        //Disable all of the Cheats on startup.
        //cheat_infiniteLivesEnabled = false;
        //cheat_infiniteContinuesEnabled = false;
        //cheat_infiniteBombsEnabled = false;
        achievementsEnabled = true;
        //Load and disable the Title Screen.
        menuActive = false;
        menuCursor.gameObject.SetActive(false);
        startText.gameObject.SetActive(false);
        optionsText.gameObject.SetActive(false);
        subtitleBackground.gameObject.SetActive(true);
        fader = GetComponent<Fading>();
        //Start the Introduction.
        StartIntro();
	}

    //Set the language for the Subtitles and Title Logo.
    private void InitializeText()
    {
        //If all 3 "face" buttons are held, set language to "Japanese".
        if (action_A.triggered && action_B.triggered && action_C.triggered)
        {
            language = "japanese";
            PlayerPrefs.SetString("language", language);
            PlayerPrefs.Save();
        }
        //If a language preference hasn't been set yet, set it to "English".
        else if (!PlayerPrefs.HasKey("language"))
        {
            language = "english";
            PlayerPrefs.SetString("language", language);
            PlayerPrefs.Save();
        }
        //Otherwise, load the current language preference.
        else
        {
            language = PlayerPrefs.GetString("language");
        }
    }

    //Start all of the components of the Introduction.
    private void StartIntro()
    {
        //Start the Background's movement.
        StartCoroutine(MoveBackground());
    }

    //Change the Subtitle text to match the narration.
    private IEnumerator ChangeText()
    {
        //TODO: When the narration is recorded, a number of "WaitForSeconds" delays
        // will be used to change the Subtitle Text as the voiceover progresses.
        yield return new WaitForSeconds(3f);
    }

    //Slowly move the camera down the Intro Background.
    private IEnumerator MoveBackground()
    {
        float movementDelay = 0f;
        //Wait for 5 seconds before starting to scroll the Background.
        while (movementDelay < 5f)
        {
            //Check for the Start Button to skip the Intro cutscene.
            if (action_Start.triggered)
            {
                //Stop the Intro narration.
                //Skip to the Title Screen.
                StartCoroutine(SkipIntro());
                //"SkipIntro" will move the Background down to the Title Screen, so stop moving it here.
                yield break;
            }
            //Increment the timer that's preventing the Background from moving.
            movementDelay += Time.deltaTime;
            yield return new WaitForSeconds(0);
        }
        //If the Title Screen hasn't been activated, keep moving the Background.
        while (introBackground.gameObject.transform.position.y < 400)
        {
            //Scroll the Background up past the Camera slowly.
            introBackground.gameObject.transform.Translate(0, .06f, 0);
            //Check for the Start Button to skip the Intro cutscene.
            if (action_Start.triggered)
            {
                //Stop the Intro narration.
                //Skip to the Title Screen.
                StartCoroutine(SkipIntro());
                yield break;
            }
            //Wait until the next frame before moving the Background again.
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Deactivate the Subtitles, so they aren't blocking the Title Screen.
        subtitleBackground.gameObject.SetActive(false);
        ActivateTitleMenu();
    }

    //Fade the Title Screen to white, then jump straight to the Title Screen.
    private IEnumerator SkipIntro()
    {
        //fader.fadeOutOverlay = Texture2D.whiteTexture;
        //Start fading the screen to white.
        float fadeTime = fader.StartFade(1);
        //Wait until the screen has completely faded to white before continuing.
        yield return new WaitForSeconds(fadeTime);
        //Move the Background the rest of the way up.
        //(The new Vector3 factors in the position of the Canvas that the Background is a Child of).
        introBackground.gameObject.transform.position = new Vector3(160f, 400f, 1f);
        //Deactivate the Subtitles, so they aren't blocking the Title Screen.
        subtitleBackground.gameObject.SetActive(false);
        //Show the Start Screen.
        ActivateTitleMenu();
        //Start fading the screen out from white.
        fadeTime = fader.StartFade(-1);
        //Wait until the screen has completely faded back in before continuing.
        yield return new WaitForSeconds(fadeTime);
    }

    //Set all of the necessary parts of the Title Screen active.
    private void ActivateTitleMenu()
    {
        titleLogo.SetActive(true);
        titleLogo.GetComponent<Animator>().enabled = true;
        //If the language is Japanese, load that version of the Title Logo.
        //if (language == "japanese")
        //{
        //    titleLogo.GetComponent<Animator>().SetTrigger("japanese");
        //}
        //Otherwise, just load the standard English version.
        //else
        //{
            titleLogo.GetComponent<Animator>().SetTrigger("english");
        //}
        //Activate the two Title Screen options.
        startText.gameObject.SetActive(true);
        optionsText.gameObject.SetActive(true);
        menuActive = true;
        //Point the Menu Cursor toward "Start", and make it active.
        selectedMenuItem = 0;
        menuCursor.gameObject.SetActive(true);
        //Start checking for Keyboard input.
        StartCoroutine(GetInput());
    }

    //Check for Keyboard Input and update the Menu Cursor as needed.
    private IEnumerator GetInput()
    {
        //Wait until the next frame to prevent double inputs.
        yield return new WaitForEndOfFrame();
        //Keep checking for input until the Game starts.
        while (true)
        {
            //Only take Player input if the Player isn't on the Options/High Score Screen.
            if (!SceneManager.GetSceneByName("OptionsScreen").isLoaded)
            {
                //Decrement the selected option by one.
                if(action_up.triggered)
                {
                    selectedMenuItem--;
                    //If selectedMenuItem has gone below 0, set it to 1.
                    if (selectedMenuItem < 0)
                    {
                        selectedMenuItem = 1;
                    }
                }
                //Increment the selected option by one.
                if(action_down.triggered)
                {
                    selectedMenuItem++;
                    //If selectedMenuItem has gone past 1, set it to 0.
                    if (selectedMenuItem > 1)
                    {
                        selectedMenuItem = 0;
                    }
                }
                //Update the Main Cursor's position based on the current selected option.
                switch (selectedMenuItem)
                {
                    //Draw the cursor to the left of "Start".
                    case 0:
                        menuCursor.transform.position = new Vector3(startText.transform.position.x - 16, startText.transform.position.y, -1);
                        break;
                    //Draw the cursor to the left of "Options".
                    case 1:
                        menuCursor.transform.position = new Vector3(optionsText.transform.position.x - 16, optionsText.transform.position.y, -1);
                        break;
                }
                //Go to a new Scene, either Stage 1 or the Options Screen, based on which is selected.
                if (action_Start.triggered)
                {
                    switch (selectedMenuItem)
                    {
                        //"Start" is selected, so load the first Stage (currently the Movement Test).
                        case 0:
                            StartCoroutine(StartGame("Stage1"));
                            yield break;
                        //"Options" is selected, so load the Options Screen.
                        case 1:
                            StartCoroutine(ChangeLevel("OptionsScreen"));
                            break;
                    }
                }
            }
            yield return new WaitForSeconds(0);
        }
    }

    //Fade the Title Screen to black, then load another Scene.
    /// <param name="sceneName">The name of the new Scene to load.</param>
    private IEnumerator ChangeLevel(string sceneName)
    {
        //Start fading the screen to black.
        float fadeTime = fader.StartFade(1);
        //Wait until the screen has completely faded to black before continuing.
        yield return new WaitForSeconds(fadeTime);
        //Load the Scene over this one, to make it easier to return to this one.
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        //Start fading the screen out from black.
        fadeTime = fader.StartFade(-1);
        //Wait until the screen has completely faded back in before continuing.
        yield return new WaitForSeconds(fadeTime);
    }

    //Fade the Title Screen to black, then start the "Gameplay" Scene.
    /// <param name="sceneName">The name of the new Scene to load.</param>
    private IEnumerator StartGame(string sceneName)
    {
        //Start fading the screen to black.
        float fadeTime = fader.StartFade(1);
        //Wait until the screen has completely faded to black before continuing.
        yield return new WaitForSeconds(fadeTime);
        //Load the new Scene while unloading any others, for efficiency.
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        //Disable any outside behaviors from the GameManager, as we won't need them during gameplay.
        instance.enabled = false;
    }
}
