/*
 * UIManager.cs
 * 
 * Manager for each of the UI overlay elements that can appear during gameplay.
 * Screens managed here include the Pause Screen, Stage Clear Screen, Game Over Screen, 
 * High Score Entry Screen, and Continue Screen.
 * 
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class UIManager : MonoBehaviour
{
    public static UIManager instance = null; //The UIManager object that the Stage's LevelManager can access.
                                             //There should be a new one of these every Stage.
    //Intro Screen Elements:
    public Text[] introLetters;     //The individual letters that make up the intro's "Stage #" message.

    //HUD Elements:
    public Image livesText;  //The number that represents the number of Lives the Player has.
    public Image bombsText;  //The number that represents the number of Bombs the Player has.
    public Image[] scoreText;  //The 6-digit number that represents the Player's current score.
    public Image[] hiScoreText;    //The 6-digit number that represents the highest score saved to this copy of the game.
    public Image multiplierText;    //The display for the current multiplier value applied to the Player's accrued points.
    public Sprite[] multiplierSprites;  //The possible Sprites for the "multiplierText" Image.
    public Image[] speedIndicators; //The array of Sprite objects that indicate the Player's current speed.
    public Image[] powerIndicators; //The array of Sprite objects that indicate the Player's current Weapon Power Level.

    //Pause Screen Elements:
    public Image pauseBackground;   //The background used to dim the screen when the game is paused.
    public Text pauseText;          //The message that is displayed when the game is paused.

    //Warning Screen Elements:
    public Image warningBackground; //The semi-transparent red background for the Warning Screen.
    public Text warningText;        //The Text message for the Warning Screen.
    public Text[] warningLetters;   //The individual letters that make up the Warning Text.
    private Vector3 warningLettersScaleIncrement;       //The amount to increase the scale of the Warning Letters by every frame.
    private float stretchWarningLettersStaggerTimer;    //The timer between calls of the "StretchWarningLetters" Coroutine.
    public Image warningLine;       //The "underline" effect for the Warning Screen Text.
    private Vector3 warningLineScaleIncrement;          //The amount to increase the scale of the Warning Line by every frame.

    //Stage Clear Screen Elements:
    public Text[] clearLetters;     //The individual letters that make the "STAGE CLEAR!" message.
    public Text clearLabel;         //The Label for the "Clear Bonus".
    public Text clearNumber;        //The display for the "Clear Bonus" amount.
    public Text accuracyLabel;      //The Label for the "Accuracy Bonus".
    public Text accuracyNumber;     //The display for the "Accuracy Bonus" amount.
    public Text damageLabel;        //The Label for the "Damage Bonus".
    public Text damageNumber;       //The display for the "Damage Bonus" amount.
    public Text perfectAimLabel;    //The Label for the "Perfect Aim" special bonus.
    public Text perfectAimNumber;   //The Text denoting the "Perfect Aim" bonus amount.
    public Text noMissLabel;        //The Label for the "No Miss" special bonus.
    public Text noMissNumber;       //The Text denoting the "No Miss" bonus amount.
    public Text noBombsLabel;       //The Label for the "No Bombs" special bonus.
    public Text noBombsNumber;      //The Text denoting the "No Bombs" bonus amount.

    //Game Over Screen Elements:
    public Text gameOverText;       //The message that is displayed when the Player gets a Game Over.

    //High Score Entry Screen Elements:
    public Text newHighScoreBanner; //The banner along the top of the High Score Entry Screen.
    public Image rankLabel;         //The Label for the Rank Display.
    public Image[] rankNumber;      //The Rank number that the Player reached.
    public Image scoreLabel;        //The Label for the Score Display.
    public Image[] scoreNumber;     //The Score that the Player achieved.
    public Image initialsLabel;     //The Label for the Initials Entry.
    public Image[] initialsEntries; //The three entry fields for the Player's Initials.
    public Image initialsEnd;       //The "ED" character that prefaces confirmation of the High Score entry.
    public Image stageLabel;        //The Label for the Stage Display.
    public Image stageDisplay;      //The Stage that the Player reached.
    public readonly char[] letterList = {'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 
                                         'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '0', '1', '2', '3', 
                                         '4', '5', '6', '7', '8', '9', '_', '?', '!', '@', '#', '$', '%', '^', '&',
                                         '*', '(', ')', '+', '-', '=', ';', ':','\'', '"', ',', '.', '<', '>', '~'};      //The list of characters for the Player to choose from when entering their initials.
    public Sprite[] pressstartGlyphs;    //The array of Glyphs used to display dynamic objects in the "Press Start"-style font, such as the High Score Entry Screen.
    public Dictionary<char, Sprite> pressstartDictionary;    //The Dictionary to link the Images in "pressstartGlyphs" to the characters they represent.
    public Sprite[] pressstartitalicGlyphs;    //The array of Glyphs used to display dynamic objects in the "Press Start Italic"-style font, such as the Scores.
    public Dictionary<char, Sprite> pressstartitalicDictionary;    //The Dictionary to link the Images in "pressstartitalicGlyphs" to the characters they represent.
    public Sprite[] pressstartlongGlyphs;    //The array of Glyphs used to display dynamic objects in the "Press Start Long"-style font, such as the Scores.
    public Dictionary<char, Sprite> pressstartlongDictionary;    //The Dictionary to link the Images in "pressstartlongGlyphs" to the characters they represent.

    //Continue Screen Elements:
    public Text continueText;       //The message that is displayed when the Player has the option to Continue.
    public Text continueTimerText;  //The Text to display the Continue Text.

	// Use this for initialization
	void Awake()
    {
        //Make the UIManager a Singleton object.
        //if (instance == null)
        //{
            instance = this;
        /*}
        else if (instance != this)
        {
            Destroy(gameObject);
        }*/
        DontDestroyOnLoad(gameObject);
        warningLettersScaleIncrement = new Vector3(0, .04f, 0);
        warningLineScaleIncrement = new Vector3(.008f, 0, 0);
        FillDictionaries();
	}

    //Populate the Glyph Dictionaries for displaying Image-based text.
    private void FillDictionaries()
    {
        //Create the Dictionaries.
        pressstartDictionary = new Dictionary<char, Sprite>();
        pressstartitalicDictionary = new Dictionary<char, Sprite>();
        pressstartlongDictionary = new Dictionary<char, Sprite>();
        //Assign each character in "letterList" to the corresponding glyph in "pressstartGlyphs".
        //Both arrays have their characters in the same order.
        for (int i = 0; i < letterList.Length; i++)
        {
            pressstartDictionary.Add(letterList[i], pressstartGlyphs[i]);
            pressstartitalicDictionary.Add(letterList[i], pressstartitalicGlyphs[i]);
            pressstartlongDictionary.Add(letterList[i], pressstartlongGlyphs[i]);
        }
    }

    //Play the introductory "cutscene" for the Stage.
    public IEnumerator PlayIntroScreen()
    {
        //Wait two seconds before starting the fade-in.
        yield return new WaitForSeconds(2);
        //Fade the Game World in.
        //The Pause Background already covers the appropriate amount of the screen, so use that.
        //Create a float to decrease the Pause Background's alpha in steps.
        float alphaStep = 1f;
        //Fade the Background incrementally until it is invisible.
        while (pauseBackground.GetComponent<CanvasRenderer>().GetAlpha() > 0)
        {
            //Decrease the alpha step by .2.
            alphaStep -= .2f;
            //Update the Background's alpha value.
            pauseBackground.GetComponent<CanvasRenderer>().SetAlpha(alphaStep);
            //Wait a quarter of a second before decreasing the alpha again.
            yield return new WaitForSeconds(.25f);
        }
        //Reset the Pause Background for the Pause Screen itself.
        pauseBackground.gameObject.SetActive(false);
        pauseBackground.GetComponent<CanvasRenderer>().SetAlpha(.5f);
        //Wait for another 1.5 seconds.
        yield return new WaitForSeconds(1.5f);
        //Move all of the Intro Text objects into place.
        while (introLetters[5].transform.position.x < 54 + LevelManager.instance.screenEdges.bounds.extents.x)
        {
            foreach (Text t in introLetters)
            {
                t.transform.Rotate(0, .5f, 0, Space.Self);
                t.transform.Translate(.5f, 0, 0, Space.World);
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Leave the Text on the screen another 4 seconds.
        yield return new WaitForSeconds(4f);
        //Reset alphaStep to fade the Stage letters out.
        alphaStep = 1f;
        //Fade the letters incrementally until they are invisible.
        while (introLetters[0].GetComponent<CanvasRenderer>().GetAlpha() > 0)
        {
            //Decrease the alpha step by .2.
            alphaStep -= .2f;
            //Update the letters' alpha values.
            foreach (Text t in introLetters)
            {
                t.GetComponent<CanvasRenderer>().SetAlpha(alphaStep);
            }
            //Wait a quarter of a second before decreasing the alpha again.
            yield return new WaitForSeconds(.25f);
        }
        //Deactivate each of the letters.
        foreach (Text t in introLetters)
        {
            t.gameObject.SetActive(false);
        }
    }

    //Update the number of Lives displayed in the HUD.
    public void UpdateLivesDisplay(int lives)
    {
        livesText.sprite = pressstartlongDictionary[lives.ToString()[0]];
    }

    //Update the number of Bombs displayed in the HUD.
    public void UpdateBombsDisplay(int bombs)
    {
        bombsText.sprite = pressstartlongDictionary[bombs.ToString()[0]];
    }

    //Update the Speed Display in the HUD.
    public void UpdateSpeedDisplay(int speedSetting)
    {
        //Make all of the Speed Indicators invisible to start.
        foreach (Image g in speedIndicators)
        {
            g.GetComponent<CanvasRenderer>().SetAlpha(0f);
        }
        //Make all of the indicators up to and including the "current" one visible.
        for (int i = 0; i < speedSetting; i++)
        {
            speedIndicators[i].GetComponent<CanvasRenderer>().SetAlpha(1.0f);
        }
    }

    //Update the Power Display in the HUD.
    public void UpdatePowerDisplay(int powerSetting)
    {
        //Make all of the Power Indicators invisible to start.
        foreach (Image g in powerIndicators)
        {
            g.GetComponent<CanvasRenderer>().SetAlpha(0f);
        }
        //Make all of the indicators up to and including the "current" one visible.
        for (int i = 0; i < powerSetting; i++)
        {
            powerIndicators[i].GetComponent<CanvasRenderer>().SetAlpha(1.0f);
        }
    }

    //Update the Images that make up the Score in the HUD.
    public void UpdateScoreDisplay(string score)
    {
        //Iterate through all 6 Images that display the Score.
        for (int i = 0; i < score.Length; i++)
        {
            //Use the digit of the Score as the key to get the correct Sprite from the Dictionary.
            scoreText[i].sprite = pressstartDictionary[score[i]];
        }
    }

    //Update the Images that make up the Score in the HUD.
    public void UpdateHighScoreDisplay(string highScore)
    {
        //Iterate through all 6 Images that display the Score.
        for (int i = 0; i < highScore.Length; i++)
        {
            //Use the digit of the Score as the key to get the correct Sprite from the Dictionary.
            hiScoreText[i].sprite = pressstartDictionary[highScore[i]];
        }
    }

    //Toggle whether or not to display the Multiplier Indicator.
    public void ToggleMultiplierDisplay(bool active)
    {
        multiplierText.gameObject.SetActive(active);
    }

    //Change the Multiplier Indicator to match the current Multiplier value.
    public void SetMultiplierSprite(int value)
    {
        switch (value)
        {
            case 2:
                multiplierText.sprite = multiplierSprites[0];
                break;
            case 3:
                multiplierText.sprite = multiplierSprites[1];
                break;
            case 5:
                multiplierText.sprite = multiplierSprites[2];
                break;
            case 10:
                multiplierText.sprite = multiplierSprites[3];
                break;
        }
    }

    //Toggle the Pause Screen on (if "active" is true) or off (if false).
    public void TogglePauseScreen(bool active)
    {
        //Reset the Pause Background's alpha to .5, because it gets set to 1 after the Player pauses and unpauses once for some reason.
        pauseBackground.GetComponent<CanvasRenderer>().SetAlpha(.5f);
        //Toggle the active state of the Background and Text objects.
        pauseBackground.gameObject.SetActive(active);
        pauseText.gameObject.SetActive(active);
        //Toggle the music between half- and full-volume.
        LevelManager.instance.stageMusic.volume = .5f / LevelManager.instance.stageMusic.volume;
    }

    //Play the Warning Screen that prefaces a Boss battle.
    public void PlayWarningScreen()
    {
        //Start the Warning Background animation.
        StartCoroutine(PulseWarningBackground(.5f));
        //Show the Warning Text.
        //StartCoroutine(DisplayWarningMessage());
        StartCoroutine(StartWarningLetterScaling());
        //Start the Warning underline animation.
        StartCoroutine(StretchWarningLine());
    }

    ///Fade the Background for the Warning Screen in and out.
    private IEnumerator PulseWarningBackground(float duration)
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Make the Background completely transparent, in case it isn't already.
        warningBackground.GetComponent<CanvasRenderer>().SetAlpha(0f);
        //Enable the Background object.
        warningBackground.gameObject.SetActive(true);
        //Pulse the Background five times.
        for (int i = 0; i < 5; i++)
        {
            //Fade the background from 0 to .5 alpha over the course of the duration.
            while (warningBackground.GetComponent<CanvasRenderer>().GetAlpha() < .5f)
            {
                //If the Game is paused, don't update the Timer.
                if (!LevelManager.instance.gamePaused)
                {
                    warningBackground.GetComponent<CanvasRenderer>().SetAlpha(warningBackground.GetComponent<CanvasRenderer>().GetAlpha() + (Time.deltaTime / duration));
                }
                //Yield out of ths Coroutine for the others.
                yield return new WaitForSeconds(Time.deltaTime);
            }
            //Fade the background from .5 to 0 alpha over the course of the duration.
            while (warningBackground.GetComponent<CanvasRenderer>().GetAlpha() > 0f)
            {
                //If the Game is paused, don't update the Timer.
                if (!LevelManager.instance.gamePaused)
                {
                    warningBackground.GetComponent<CanvasRenderer>().SetAlpha(warningBackground.GetComponent<CanvasRenderer>().GetAlpha() - (Time.deltaTime / duration));
                }
                //Yield out of ths Coroutine for the others.
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
        //After the Background is finished, disable all of the Warning Screen elements.
        warningBackground.gameObject.SetActive(false);
        //warningText.gameObject.SetActive(false);
        foreach (Text t in warningLetters)
        {
            t.gameObject.SetActive(false);
        }
        warningLine.gameObject.SetActive(false);
    }

    //Display the Text for the Warning Screen Message.
    private IEnumerator DisplayWarningMessage()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Display the Warning Text.
        warningText.gameObject.SetActive(true);
    }

    //Start the scaling Coroutines for each letter in the Warning Text.
    private IEnumerator StartWarningLetterScaling()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Iterate through every letter in the Warning Text.
        for (int i = 0; i < warningLetters.Length; i++)
        {
            //Start scaling the letter from 0 to 1.
            StartCoroutine(StretchWarningLetters(i));
            //Reset the Timer for between calls.
            stretchWarningLettersStaggerTimer = 0;
            //Wait 1/4 of a second between each letter.
            while (stretchWarningLettersStaggerTimer < .25f)
            {
                //If the Game is paused, don't update the Timer.
                if (!LevelManager.instance.gamePaused)
                {
                    stretchWarningLettersStaggerTimer += Time.deltaTime;
                }
                yield return new WaitForSeconds(Time.deltaTime);
            }
        }
    }

    //Stretch the Warning Text letters vertically until they reach full-scale.
    private IEnumerator StretchWarningLetters(int letterIndex)
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Make the letter 0 pixels tall, in case it wasn't already.
        warningLetters[letterIndex].transform.localScale.Set(1, 0, 1);
        //Enable the Warning Text letter.
        warningLetters[letterIndex].gameObject.SetActive(true);
        //Expand the letter until it is full height.
        while (warningLetters[letterIndex].transform.localScale.y < 1)
        {
            //If the Game is paused, don't increase the letter scale.
            if (!LevelManager.instance.gamePaused)
            {
                //Increase the scale of the letter every frame.
                warningLetters[letterIndex].transform.localScale += warningLettersScaleIncrement;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    //Stretch the line under the Warning Text out to its full length.
    private IEnumerator StretchWarningLine()
    {
        //Yield out of this Coroutine to let the other ones start.
        yield return new WaitForEndOfFrame();
        //Make the line 0 pixels wide, in case it wasn't already.
        warningLine.transform.localScale.Set(0, 1, 1);
        //Enable the Warning Line.
        warningLine.gameObject.SetActive(true);
        //Expand the line until it is full length.
        while (warningLine.transform.localScale.x < 1)
        {
            //If the Game is paused, don't increase the line scale.
            if (!LevelManager.instance.gamePaused)
            {
                //Increase the scale of the line every frame.
                warningLine.transform.localScale += warningLineScaleIncrement;
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
    }

    //Toggle the Game Over Screen on (if "active" is true) or off (if false).
    public void ToggleGameOverScreen(bool active)
    {
        gameOverText.gameObject.SetActive(active);
    }

    /// <summary>
    /// Set the Sprites for the High Score Entry Screen's Image objects.
    /// </summary>
    /// <param name="rank">The rank number to display.</param>
    /// <param name="score">The Player's final score.</param>
    /// <param name="stage">The number of the Stage the Player was on (or "CLEAR", if they won the game).</param>
    public void SetHighScoreScreen(string rank, string score, char stage)
    {
        for (int i = 0; i < rankNumber.Length; i++)
        {
            rankNumber[i].sprite = pressstartDictionary[rank[i]];
        }
        for (int j = 0; j < scoreNumber.Length; j++)
        {
            scoreNumber[j].sprite = pressstartDictionary[score[j]];
        }
        stageDisplay.sprite = pressstartDictionary[stage];
    }

    //Toggle the High Score Entry Screen on (if "active" is true) or off (if false).
    public void ToggleHighScoreScreen(bool active)
    {
        newHighScoreBanner.gameObject.SetActive(active);
        rankLabel.gameObject.SetActive(active);
        foreach (Image i in rankNumber)
        {
            i.gameObject.SetActive(active);
        }
        scoreLabel.gameObject.SetActive(active);
        foreach (Image i in scoreNumber)
        {
            i.gameObject.SetActive(active);
        }
        initialsLabel.gameObject.SetActive(active);
        stageLabel.gameObject.SetActive(active);
        stageDisplay.gameObject.SetActive(active);
    }

    /// <summary>
    /// Set the text for the Initials objects on the High Score Entry Screen.
    /// </summary>
    /// <param name="initialIndex">The index of the Initial object to change.</param>
    /// <param name="currentChar">The index of the character to change the text to.</param>
    public void SetInitialsText(int initialIndex, int currentChar)
    {
        initialsEntries[initialIndex].sprite = pressstartGlyphs[currentChar];
    }

    /// <summary>
    /// Toggle one of the Initials objects on (if "active" is true) or off (if false).
    /// </summary>
    /// <param name="initialIndex">The index of the Initial object to toggle.</param>
    /// <param name="active">The "active" state the object should be set to.</param>
    public void ToggleInitials(int initialIndex, bool active)
    {
        initialsEntries[initialIndex].gameObject.SetActive(active);
    }

    //Make the current initial "blink" while the Player is entering it.
    public IEnumerator BlinkInitial(int initialIndex)
    {
        //This Coroutine will be stopped externally, so the loop will run infinitely.
        while (true)
        {
            //Activate the Text Object (making it visible).
            initialsEntries[initialIndex].gameObject.SetActive(true);
            //Wait half a second.
            yield return new WaitForSeconds(.5f);
            //Deactivate the Text Object's (making it invisible).
            initialsEntries[initialIndex].gameObject.SetActive(false);
            //Wait half a second.
            yield return new WaitForSeconds(.5f);
        }
    }

    //Toggle the "End" character that follows the Initials on (if "active" is true) or off (if false).
    public void ToggleInitialsEnd(bool active)
    {
        initialsEnd.gameObject.SetActive(active);
    }

    //Flicker the Initials between half- and full-transparency, to show that they have been entered, before disabling them.
    public IEnumerator ShimmerInitials()
    {
        //Create the timer for the "shimmer" effect.
        float shimmerTimer = 0f;
        //Have the text shimmer for two seconds.
        while (shimmerTimer < 2f)
        {
            //Set all of the Text objects to half-transparency.
            foreach (Image i in initialsEntries)
            {
                i.GetComponent<CanvasRenderer>().SetAlpha(.5f);
            }
            //Wait 1/30th of a second.
            yield return new WaitForSeconds(Time.deltaTime * 2);
            //Set all of the Text objects to full-transparency.
            foreach (Image i in initialsEntries)
            {
                i.GetComponent<CanvasRenderer>().SetAlpha(1);
            }
            //Wait 1/30th of a second.
            yield return new WaitForSeconds(Time.deltaTime * 2);
            //Increase the timer by 1/15th of a second.
            shimmerTimer += Time.deltaTime * 4;
        }
        //Disable the Text objects for the High Score Entry screen.
        ToggleHighScoreScreen(false);
        ToggleInitials(0, false);
        ToggleInitials(1, false);
        ToggleInitials(2, false);
        //Set up the Continue Screen.
        StartCoroutine(LevelManager.instance.Continue());
    }

    /// <summary>
    /// Set the Label Text for the Continue Screen.
    /// </summary>
    /// <param name="continues">The number of remaining Continues to display.</param>
    public void SetContinueText(int continues)
    {
        continueText.text = "C O N T I N U E ?   (" + continues + ")";
    }

    /// <summary>
    /// Display the current time remaining in the Continue Timer.
    /// </summary>
    /// <param name="seconds">The number of seconds left on the Timer (rounded up).</param>
    public void SetContinueTimer(int seconds)
    {
        continueTimerText.text = seconds.ToString();
    }

    //Toggle the Continue Screen on (if "active" is true) or off (if false).
    public void ToggleContinueScreen(bool active)
    {
        continueText.gameObject.SetActive(active);
        continueTimerText.gameObject.SetActive(active);
    }

    //Play the animation for the "Stage Clear!" Banner
    public IEnumerator DisplayStageClearBanner(int index, float targetX)
    {
        yield return new WaitForEndOfFrame();
        //Move the Letter and rotate it until it is in place.
        while (clearLetters[index].transform.position.x > targetX)
        {
            clearLetters[index].transform.Translate(-10, 0, 0, Space.World);
            clearLetters[index].transform.Rotate(0, 60, 0, Space.Self);
            //If the Letter jumps past its target, bring it back to its target.
            if (clearLetters[index].transform.position.x < targetX)
            {
                clearLetters[index].transform.Translate(targetX - clearLetters[index].transform.position.x, 0, 0, Space.World);
            }
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //Keep the Letter rotating until all of the Letters are in place.
        while (clearLetters[10].transform.position.x > 99)
        {
            clearLetters[index].transform.Rotate(0, 60, 0, Space.Self);
            yield return new WaitForSeconds(Time.deltaTime);
        }
        //When all the letters are in place, reset their rotation to 0.
        clearLetters[index].transform.Rotate(0, -clearLetters[index].transform.eulerAngles.y, 0, Space.Self);
        //Fade the Letter to white, then back to its normal color.
    }

    /// <summary>
    /// Update the display for the Clear Bonus points.
    /// </summary>
    /// <param name="clearBonus">The number of points remaining in the Bonus.</param>
    public void SetClearBonusText(int clearBonus)
    {
        clearNumber.text = clearBonus.ToString().PadLeft(5, '0');
    }

    //Toggle the Clear Bonus on (if "active" is true) or off (if false).
    public void ToggleClearBonus(bool active)
    {
        clearLabel.gameObject.SetActive(active);
        clearNumber.gameObject.SetActive(active);
    }

    /// <summary>
    /// Display the Accuracy Percentage the Player achieved.
    /// </summary>
    /// <param name="accuracy">The accuracy percentage, rounded to the tenths place.</param>
    public void SetAccuracyLabelText(float accuracy)
    {
        accuracyLabel.text = "ACCURACY BONUS (" + accuracy + "%):";
    }

    /// <summary>
    /// Update the display for the Accuracy Bonus points.
    /// </summary>
    /// <param name="accuracyBonus">The number of points remaining in the Bonus.</param>
    public void SetAccuracyBonusText(int accuracyBonus)
    {
        accuracyNumber.text = accuracyBonus.ToString().PadLeft(5, '0');
    }

    //Toggle the Accuracy Bonus on (if "active" is true) or off (if false).
    public void ToggleAccuracyBonus(bool active)
    {
        accuracyLabel.gameObject.SetActive(active);
        accuracyNumber.gameObject.SetActive(active);
    }

    /// <summary>
    /// Update the display for the Damage Bonus points.
    /// </summary>
    /// <param name="damageBonus">The number of points remaining in the Bonus.</param>
    public void SetDamageBonusText(int damageBonus)
    {
        damageNumber.text = damageBonus.ToString().PadLeft(5, '0');
    }

    //Toggle the Damage Bonus on (if "active" is true) or off (if false).
    public void ToggleDamageBonus(bool active)
    {
        damageLabel.gameObject.SetActive(active);
        damageNumber.gameObject.SetActive(active);
    }

    //Toggle the Perfect Aim Bonus on (if "active" is true) or off (if false).
    public void TogglePerfectAimBonus(bool active)
    {
        perfectAimLabel.gameObject.SetActive(active);
        perfectAimNumber.gameObject.SetActive(active);
    }

    //Toggle the No Miss Bonus on (if "active" is true) or off (if false).
    public void ToggleNoMissBonus(bool active)
    {
        noMissLabel.gameObject.SetActive(active);
        noMissNumber.gameObject.SetActive(active);
    }

    //Toggle the No Bombs Bonus on (if "active" is true) or off (if false).
    public void ToggleNoBombsBonus(bool active)
    {
        noBombsLabel.gameObject.SetActive(active);
        noBombsNumber.gameObject.SetActive(active);
    }
}
