/*
 * OptionsManager.cs
 * 
 * Lets the Player edit the starting Lives value and Button Configuration,
 * play the BGM and Sound Effect tracks found in the game, and view the High Scores.
 * 
 */

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class OptionsManager : MonoBehaviour
{
    //The Text objects on the Options Screen's Canvas ("labels" are static, "texts" can be changed)
    public Text livesLabel;     //The Text object that labels the "Lives" value.
    public Text livesText;      //The number that represents the current Lives value, between 1 and 5.
    public Text configLabel;    //The Text object that labels the "Button Config" display.
    public Text configText;     //The lines that represent the current Button Configuration, by showing the behaviors for the A, B, and C Buttons.
    public Text bgmLabel;       //The Text object that labels the currently selected BGM track.
    public Text bgmText;        //The number that represents the currently selected Background Music track.
    public Text seLabel;        //The Text object that labels the currently selected SE track.
    public Text seText;         //The number that represents the currently selected Sound Effect track.
    public Text highScoreLabel; //The label the Player can select to go to the High Score Screen.

    private int selectedMenuItem;   //Whichever setting is currently selected, between 0 and 4.
    private int selectedBGM;        //The index for the currently selected Music Track.
    private int selectedSE;         //The index for the currently selected Sound Effect.
    private int[] codeEntry;        //The last four Sound Effects selected, to use as a cheat code entry.

    private bool dPadPressed;       //Whether or not a directional button is being held.

	// Use this for initialization
	void Awake()
    {
        //Put the cursor on the "Lives" setting.
        selectedMenuItem = 0;
        //Select the 1st Music Track.
        selectedBGM = 0;
        //Select the 1st Sound Effect.
        selectedSE = 0;
        //Initialize the "Code Entry" array.
        codeEntry = new int[] { 0, 0, 0, 0 };
        //Keep the Lives and Button Config text from resetting if the Options Screen is closed and reopened.
        ChangeLives(0);
        ChangeConfig(0);
    }
	
	// Update is called once per frame
	void Update()
    {
        //If the High Score Screen isn't up, check for Keyboard input.
        if (!SceneManager.GetSceneByName("HighScoreScreen").isLoaded)
        {
            //Decrement the selected setting by one.
            if (GameManager.instance.action_up.triggered)
            {
                //Only change the option if this button is being pressed, not held.
                if (!dPadPressed)
                {
                    selectedMenuItem--;
                    //If scrolling off of the first setting ("Lives"), wrap around to the last one ("High Scores").
                    if (selectedMenuItem < 0)
                    {
                        selectedMenuItem = 4;
                    }
                    //Update the text colors to show the newly selected setting.
                    ChangeColors();
                }
                dPadPressed = true;
            }
            //Increment the selected setting by one.
            else if (GameManager.instance.action_down.triggered)
            {
                //Only change the option if this button is being pressed, not held.
                if (!dPadPressed)
                {
                    selectedMenuItem++;
                    //If scrolling off of the last setting ("High Scores"), wrap around to the first one ("Lives").
                    if (selectedMenuItem > 4)
                    {
                        selectedMenuItem = 0;
                    }
                    //Update the text colors to show the newly selected setting.
                    ChangeColors();
                }
                dPadPressed = true;
            }
            //Change the value of the selected setting. 
            else if (GameManager.instance.action_left.triggered)
            {
                //Only change the value if this button is being pressed, not held.
                if (!dPadPressed)
                {
                    switch (selectedMenuItem)
                    {
                        //Decrement the Lives value by one.
                        case 0:
                            ChangeLives(-1);
                            break;
                        //Decrement the Button Config value by one.
                        case 1:
                            ChangeConfig(-1);
                            break;
                        //Decrement the currently selected BGM Track by one.
                        case 2:
                            ChangeBGM(-1);
                            break;
                        //Decrement the currently selected Sound Effect by one.
                        case 3:
                            ChangeSE(-1);
                            break;
                        //Default is the "High Scores" setting, which has no toggle.
                        default:
                            break;
                    }
                }
                dPadPressed = true;
            }
            //Change the value of the selected setting.
            else if (GameManager.instance.action_right.triggered)
            {
                //Only change the option if this button is being pressed, not held.
                if (!dPadPressed)
                {
                    switch (selectedMenuItem)
                    {
                        //Increment the Lives value by one.
                        case 0:
                            ChangeLives(1);
                            break;
                        //Increment the Button Config value by one.
                        case 1:
                            ChangeConfig(1);
                            break;
                        //Increment the currently selected BGM Track by one.
                        case 2:
                            ChangeBGM(1);
                            break;
                        //Increment the currently selected Sound Effect by one.
                        case 3:
                            ChangeSE(1);
                            break;
                        //Default is the "High Scores" setting, which has no toggle.
                        default:
                            break;
                    }
                }
                dPadPressed = true;
            }
            //If no directional button is being held, reset its flag.
            else
            {
                dPadPressed = false;
            }
            //Get the input for the A and C Buttons.
            if (GameManager.instance.action_A.triggered || GameManager.instance.action_C.triggered)
            {
                switch (selectedMenuItem)
                {
                    //Play the currently selected BGM Track.
                    case 2:
                        SoundManager.instance.PlayMusic(selectedBGM);
                        break;
                    //Play the currently selected Sound Effect.
                    case 3:
                        SoundManager.instance.PlaySound(selectedSE);
                        //Shift each recently-played Sound Effect over one.
                        for (int i = codeEntry.Length - 2; i >= 0; i--)
                        {
                            codeEntry[i + 1] = codeEntry[i];
                        }
                        //Add the index for the most recent sound effect.
                        codeEntry[0] = selectedSE;
                        CheckCodes();
                        break;
                    //Load the High Scores screen.
                    case 4:
                        StartCoroutine(ChangeLevel("HighScoreScreen"));
                        break;
                }
            }
            //Unload the Options Screen to return to the Title Screen.
            if (GameManager.instance.action_Start.triggered)
            {
                StartCoroutine(CloseLevel("OptionsScreen"));
            }
        }
    }

    //Change the colors of the Text objects.
    void ChangeColors()
    {
        //Set all of the Options to their default color first.
        livesLabel.color = Color.gray;
        livesText.color = Color.gray;
        configLabel.color = Color.gray;
        configText.color = Color.gray;
        bgmLabel.color = Color.gray;
        bgmText.color = Color.gray;
        seLabel.color = Color.gray;
        seText.color = Color.gray;
        highScoreLabel.color = Color.gray;
        //Change an option to "white" based on which one is selected.
        switch (selectedMenuItem)
        {
            //Make the "Lives" Text white.
            case 0:
                livesLabel.color = Color.white;
                livesText.color = Color.white;
                break;
            //Make the "Config" Text white.
            case 1:
                configLabel.color = Color.white;
                configText.color = Color.white;
                break;
            //Make the "BGM" Text white.
            case 2:
                bgmLabel.color = Color.white;
                bgmText.color = Color.white;
                break;
            //Make the "SE" Text white.
            case 3:
                seLabel.color = Color.white;
                seText.color = Color.white;
                break;
            //Make the "High Scores" Text white.
            case 4:
                highScoreLabel.color = Color.white;
                break;
        }
    }

    //Change the value of the "Lives" setting, and update the text to match it.
    void ChangeLives(int change)
    {
        GameManager.instance.lives += change;
        //If the Lives setting has gone below 1, wrap it around to 5.
        if (GameManager.instance.lives < 1)
        {
            GameManager.instance.lives = 5;
        }
        //If the Lives setting has gone above 5, wrap it around to 1.
        else if (GameManager.instance.lives > 5)
        {
            GameManager.instance.lives = 1;
        }
        //Update the "starting Lives" value (whenever the Player uses a Continue, they receive however many Lives this setting dictates).
        GameManager.instance.startingLives = GameManager.instance.lives;
        //Update the Text onscreen to match the new value.
        livesText.text = GameManager.instance.lives.ToString();
    }

    //Change the value of the "Button Config" setting, and update the text to match it.
    void ChangeConfig(int change)
    {
        GameManager.instance.buttonConfig += change;
        //If the Button Config setting has gone below 1, wrap it around to 6.
        if (GameManager.instance.buttonConfig < 1)
        {
            GameManager.instance.buttonConfig = 6;
        }
        //If the Button Config setting has gone above 6, wrap it around to 1.
        else if (GameManager.instance.buttonConfig > 6)
        {
            GameManager.instance.buttonConfig = 1;
        }
        //Update the Text onscreen to match the new value.
        switch (GameManager.instance.buttonConfig)
        {
            case 1:
                configText.text = "A: SPEED\nB: BOMB\nC: SHOT";
                break;
            case 2:
                configText.text = "A: SPEED\nB: SHOT\nC: BOMB";
                break;
            case 3:
                configText.text = "A: SHOT\nB: SPEED\nC: BOMB";
                break;
            case 4:
                configText.text = "A: SHOT\nB: BOMB\nC: SPEED";
                break;
            case 5:
                configText.text = "A: BOMB\nB: SPEED\nC: SHOT";
                break;
            case 6:
                configText.text = "A: BOMB\nB: SHOT\nC: SPEED";
                break;
        }
    }

    //Change the value of the currently selected Music track, and update the Text to match it.
    void ChangeBGM(int change)
    {
        selectedBGM += change;
        selectedBGM %= SoundManager.instance.musicTracks.Length;
        bgmText.text = selectedBGM.ToString();
    }

    //Change the value of the currently selected Sound effect, and update the Text to match it.
    void ChangeSE(int change)
    {
        selectedSE += change;
        selectedSE %= SoundManager.instance.soundEffects.Length;
        seText.text = selectedSE.ToString();
    }

    //Check the indices of the last four played Sound Effects, to see if any cheats have been unlocked.
    void CheckCodes()
    {
        //{12, 22, 19, 41}: Toggle the "Infinite Lives" Cheat.
        if(codeEntry.SequenceEqual(new int[] { 12, 22, 19, 41 }))
        {
            GameManager.instance.cheat_infiniteLivesEnabled = !GameManager.instance.cheat_infiniteLivesEnabled;
        }
        //{6, 10, 19, 72}: Toggle the "Infinite Continues" Cheat.
        if(codeEntry.SequenceEqual(new int[] { 6, 10, 19, 72 }))
        {
            GameManager.instance.cheat_infiniteContinuesEnabled = !GameManager.instance.cheat_infiniteContinuesEnabled;
        }
        //{7, 12, 19, 94}: Toggle the "Infinite Bombs" Cheat.
        if (codeEntry.SequenceEqual(new int[] { 7, 12, 19, 94 }))
        {
            GameManager.instance.cheat_infiniteBombsEnabled = !GameManager.instance.cheat_infiniteBombsEnabled;
        }
    }

    //Fade the Options Screen to black, then load another Scene.
    /// <param name="sceneName">The name of the new Scene to load (Always "HighScoreScreen").</param>
    IEnumerator ChangeLevel(string sceneName)
    {
        //Start fading the screen to black.
        float fadeTime = GameManager.instance.GetComponent<Fading>().StartFade(1);
        //Wait until the screen has completely faded to black before continuing.
        yield return new WaitForSeconds(fadeTime);
        //Load the High Scores Screen over the Options Screen.
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        //Start fading the screen out from black.
        fadeTime = GameManager.instance.GetComponent<Fading>().StartFade(-1);
        //Wait until the screen has completely faded back in before continuing.
        yield return new WaitForSeconds(fadeTime);
    }

    //Fade the Options Screen to black, then close it.
    /// <param name="sceneName">The name of the Scene to unload (Always "OptionsScreen").</param>
    IEnumerator CloseLevel(string sceneName)
    {
        //Start fading the screen to black.
        float fadeTime = GameManager.instance.GetComponent<Fading>().StartFade(1);
        //Wait until the screen has completely faded to black before continuing.
        yield return new WaitForSeconds(fadeTime);
        //Unload this Scene, which takes the Player back to the Title Screen.
        SceneManager.UnloadSceneAsync(sceneName);
        //Start fading the screen out from black.
        fadeTime = GameManager.instance.GetComponent<Fading>().StartFade(-1);
        //Wait until the screen has completely faded back in before continuing.
        yield return new WaitForSeconds(fadeTime);
    }
}
