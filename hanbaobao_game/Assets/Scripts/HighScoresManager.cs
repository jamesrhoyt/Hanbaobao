/*
 * HighScoresManager.cs
 * 
 * The Manager Object for the "High Scores" Screen, accessible via the "Options" Screen.
 * Displays the first 10 score listings, allows a shift to the second 10 listings, then fades back to the Options Screen.
 * 
 */

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class HighScoresManager : MonoBehaviour
{
    //The 8 columns of the High Score Screen.
    public Text[] highScoreRankTextOneToTen;
    public Text[] highScoresTextOneToTen;
    public Text[] initialsTextOneToTen;
    public Text[] stageNumberTextOneToTen;
    public Text[] highScoreRankTextElevenToTwenty;
    public Text[] highScoresTextElevenToTwenty;
    public Text[] initialsTextElevenToTwenty;
    public Text[] stageNumberTextElevenToTwenty;

    private string[] hiScoreRanks; //The list of Rank Numbers.
    private string[] initials; //The list of saved Initials.
    private int[] highScores; //The list of saved Scores.
    private string[] stageNumbers; //The list of saved Stage Numbers.

    private bool textMoving; //Whether or not the High Score Text is moving.
    private bool secondHalfLoaded; //Whether or not the screen is currently displaying the Rank 11-20 scores.

	// Use this for initialization
	void Awake()
    {
        //Create/Load the High Scores.
        InitializeHighScoreTable();
        //Change the Scene Text to display the Score info.
        LoadHighScoresText();
	}

    //Load the High Scores, or create and save a new High Score table if one does not exist.
    void InitializeHighScoreTable()
    {
        //Create the arrays used to manage the High Scores.
        hiScoreRanks = new string[20];
        initials = new string[20];
        highScores = new int[20];
        stageNumbers = new string[20];
        for (int i = 0; i < 20; i++)
        {
            //If no Scores have been saved, create the default High Score table.
            if (!PlayerPrefs.HasKey("highscore" + i))
            {
                //Create the default values for all of the Score arrays.
                hiScoreRanks[i] = (i + 1).ToString();
                initials[i] = "AAA";
                highScores[i] = 400000 - (i * 15000);
                stageNumbers[i] = Mathf.CeilToInt((20f - i) / 4f).ToString();
                //Save the default values to the Player Preferences, for subsequent loading.
                PlayerPrefs.SetString("highscoreRank" + i, hiScoreRanks[i]);
                PlayerPrefs.SetString("highscoreInitials" + i, initials[i]);
                PlayerPrefs.SetInt("highscore" + i, highScores[i]);
                PlayerPrefs.SetString("highscoreStage" + i, stageNumbers[i]);
                //Save the values.
                PlayerPrefs.Save();
            }
            //Load the High Scores.
            else
            {
                //Load the saved values, after the 1st time the Score Table has been created. 
                hiScoreRanks[i] = PlayerPrefs.GetString("highscoreRank" + i);
                initials[i] = PlayerPrefs.GetString("highscoreInitials" + i);
                highScores[i] = PlayerPrefs.GetInt("highscore" + i);
                stageNumbers[i] = PlayerPrefs.GetString("highscoreStage" + i);
            }
        }
    }

    //Change the Text Objects to match the saved High Scores.
    void LoadHighScoresText()
    {
        for (int i = 0; i < 10; i++)
        {
            //Set the text for the 1st 10 Rank designations.
            highScoreRankTextOneToTen[i].text = hiScoreRanks[i];
            //Keep the High Score gradient blue if it is less than 1,000,000 (7 characters).
            if (highScores[i] < 1000000)
            {
                //Set the text for the 1st 10 Score objects.
                highScoresTextOneToTen[i].text = highScores[i].ToString();
            }
            //If the High Score is greater than 1,000,000, turn its gradient green and (possibly) display the last 6 characters.
            else if (highScores[i] >= 1000000)
            {
                highScoresTextOneToTen[i].GetComponent<Gradient>().EndColor = new Color(0, .57f, 0, 1f);
                //highScoresTextOneToTen[i].text = highScores[i].ToString().Substring(1,6);
                //Set the text for the 1st 10 Score objects.
                highScoresTextOneToTen[i].text = highScores[i].ToString();
            }
            //Set the text for the 1st 10 Initials listings.
            initialsTextOneToTen[i].text = initials[i];
            //Set the text for the 1st 10 Stage Number listings.
            stageNumberTextOneToTen[i].text = stageNumbers[i];
            //Set the text for the last 10 Rank designations.
            highScoreRankTextElevenToTwenty[i].text = hiScoreRanks[i+10];
            //Keep the High Score gradient blue if it is less than 1,000,000 (7 characters).
            if (highScores[i+10] < 1000000)
            {
                //Set the text for the last 10 Score objects.
                highScoresTextElevenToTwenty[i].text = highScores[i + 10].ToString();
            }
            //If the High Score is greater than 1,000,000, turn its gradient green and (possibly) display the last 6 characters.
            else if (highScores[i+10] >= 1000000)
            {
                highScoresTextElevenToTwenty[i].GetComponent<Gradient>().EndColor = new Color(0, .57f, 0, 1f);
                //highScoresTextElevenToTwenty[i].text = highScores[i+10].ToString().Substring(1, 6);
                //Set the text for the last 10 Score objects.
                highScoresTextElevenToTwenty[i].text = highScores[i + 10].ToString();
            }
            //Set the text for the last 10 Initials listings.
            initialsTextElevenToTwenty[i].text = initials[i+10];
            //Set the text for the last 10 Stage Number listings.
            stageNumberTextElevenToTwenty[i].text = stageNumbers[i+10];
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Only take Player input via the face buttons if the Text objects aren't moving.
        if (!textMoving && GameManager.instance.action_A.triggered || GameManager.instance.action_B.triggered || GameManager.instance.action_C.triggered)
        {
            //If the Rank 1-10 scores are onscreen, start scrolling to the Rank 11-20 scores.
            if (!secondHalfLoaded)
            {
                textMoving = true;
            }
            //If the Rank 11-20 scores are onscreen, close the Scene instead.
            else
            {
                StartCoroutine(ChangeLevel("HighScoreScreen"));
            }
        }
        //If the text is still transitioning, increment its position more.
        else if (textMoving)
        {
            MoveText();
        }
	}

    //Increment the Score Text's position to the left slightly.
    void MoveText()
    {
        //If the Rank 11-20 scores aren't yet lined up onscreen, keep moving them.
        if (highScoreRankTextElevenToTwenty[0].transform.position.x > 40)
        {
            for (int i = 0; i < 10; i++)
            {
                highScoreRankTextOneToTen[i].transform.Translate(-4, 0, 0);
                highScoresTextOneToTen[i].transform.Translate(-4, 0, 0);
                initialsTextOneToTen[i].transform.Translate(-4, 0, 0);
                stageNumberTextOneToTen[i].transform.Translate(-4, 0, 0);
                highScoreRankTextElevenToTwenty[i].transform.Translate(-4, 0, 0);
                highScoresTextElevenToTwenty[i].transform.Translate(-4, 0, 0);
                initialsTextElevenToTwenty[i].transform.Translate(-4, 0, 0);
                stageNumberTextElevenToTwenty[i].transform.Translate(-4, 0, 0);
            }
        }
        //If the scores are lined up, set the Scene up to be closed afterward.
        else
        {
            textMoving = false;
            secondHalfLoaded = true;
        }
    }

    //Fade the High Score Screen to black, then unload it.
    /// <param name="sceneName">The name of the Scene to unload (Always "HighScoreScreen").</param>
    IEnumerator ChangeLevel(string sceneName)
    {
        //Start fading the screen to black.
        float fadeTime = GameManager.instance.GetComponent<Fading>().StartFade(1);
        //Wait until the screen has completely faded to black before continuing.
        yield return new WaitForSeconds(fadeTime);
        //Unload this Scene, which takes the Player back to the Options Screen.
        SceneManager.UnloadSceneAsync(sceneName);
        //Start fading the screen out from black.
        fadeTime = GameManager.instance.GetComponent<Fading>().StartFade(-1);
        //Wait until the screen has completely faded back in before continuing.
        yield return new WaitForSeconds(fadeTime);
    }
}
