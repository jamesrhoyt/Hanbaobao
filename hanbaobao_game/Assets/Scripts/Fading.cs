/*
 * Fading.cs
 * 
 * Control the transitions between Scenes by fading the Screen to black (when leaving a Scene),
 * or fading the Screen in from black (when entering a Scene).
 * 
 */

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class Fading : MonoBehaviour
{
    public Texture2D fadeOutOverlay;    //The black/white Texture to fade over the Screen
    public float fadeSpeed = 0.9f;      //The speed at which to fade the Overlay Texture.

    private int drawDepth = -100;   //Draw at a negative depth to make sure the effect is drawn over everything else.
    private float alpha = 1.0f;     //The transparency value for the Overlay Texture at any given time.
    private int fadeDirection = -1; //Whether the Scene is fading in (-1) or fading to black (1).

    //OnGUI is called every frame, while the Fading Component is enabled.
	void OnGUI()
    {
        //Increment the value by the deltaTime value, times the fade speed, 
        //with the fade direction determining whether it is positive or negative.
        alpha += fadeDirection * fadeSpeed * Time.deltaTime;
        //Clamp the alpha value between 0 (invisible) and 1 (fully opaque)
        alpha = Mathf.Clamp01(alpha);

        //Set the overall color of the GUI, with the current alpha value.
        GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, alpha);
        //Set the depth of the GUI to the negative, front-most value.
        GUI.depth = drawDepth;
        //Draw the Overlay Texture over the entirety of the Screen.
        GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), fadeOutOverlay);
	}

    //Start fading the Scene in/out.
    public float StartFade(int direction)
    {
        fadeDirection = direction;
        //Return the time it will take to fade the Screen completely out/in.
        return fadeSpeed;
    }

    //Add a delegate call when the Fader is enabled.
    //(This is called after Awake, but before Start.)
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    //Remove the delegate call when the Fader is disabled.
    //(This is called when the Scene is terminated.)
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    //OnSceneLoaded is called as soon as the Scene is loaded.
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartFade(-1);
    }
}
