/*
 * BGManager.cs
 * 
 * Manages the movement of the Foreground and Background Objects during regular gameplay,
 * as well as any acceleration/deceleration of the Objects' scroll speeds where appropriate, such as when approaching Boss Rooms.
 * 
 */

using UnityEngine;
using System.Collections;

public class BGManager : MonoBehaviour
{
    public static BGManager instance = null; //The BGManager object that the rest of the Program can access.
                                             //There should be a new one of these every Stage.
    
    private GameObject[] backgroundLayer1;  //The Objects on Layer 1 of the Background (used for parallax scrolling).
    private GameObject[] backgroundLayer2;  //The Objects on Layer 2 of the Background (used for parallax scrolling).
    private GameObject[] backgroundLayer3;  //The Objects on Layer 3 of the Background (used for parallax scrolling).
    private GameObject[] backgroundLayer4;  //The Objects on Layer 4 of the Background (used for parallax scrolling).
    private GameObject[] foregroundLayer;   //The Objects in the Foreground of the Stage.
    public float[] scrollValues;    //The different values which affect the scroll speeds of the different Background layers.
                                    //0: backgroundLayer1; 1: backgroundLayer2; 2: backgroundLayer3; 3: backgroundLayer4; 4: foregroundLayer
                                    //(This array will be populated manually via the Inspector.)
    public float[] scrollOffsets;   //Modifiers to adjust the speed of the Back/Foreground Objects where appropriate (e.g., Boss Intros, etc.)

    void Awake()
    {
        //Make the BGManager a Singleton object.
        //if (instance == null)
        //{
        instance = this;
        /*}
        else if (instance != this)
        {
            Destroy(gameObject);
        }*/
        DontDestroyOnLoad(gameObject);
        //Create dummy instantiations of the background/foreground objects arrays.
        backgroundLayer1 = new GameObject[0];
        backgroundLayer2 = new GameObject[0];
        backgroundLayer3 = new GameObject[0];
        backgroundLayer4 = new GameObject[0];
        foregroundLayer = new GameObject[0];
        FillBackgroundLayers();
    }

    //Populate all of the Background/Foreground Layers, using Object Tags.
    private void FillBackgroundLayers()
    {
        //Populate the Background/Foreground Objects arrays.
        backgroundLayer1 = GameObject.FindGameObjectsWithTag("BGLayer1");
        backgroundLayer2 = GameObject.FindGameObjectsWithTag("BGLayer2");
        backgroundLayer3 = GameObject.FindGameObjectsWithTag("BGLayer3");
        backgroundLayer4 = GameObject.FindGameObjectsWithTag("BGLayer4");
        foregroundLayer = GameObject.FindGameObjectsWithTag("FGLayer");
    }

    /// <summary>
    /// Adjust the scroll speed offset for one of the foreground or background layers.
    /// </summary>
    /// <param name="index">The index of the layer to adjust (0: backgroundLayer1; 1: backgroundLayer2; 2: backgroundLayer3; 3: backgroundLayer4; 4: foregroundLayer)</param>
    /// <param name="value">The amount to adjust the selected layer's scroll speed offset by.</param>
    public void AdjustScrollOffset(int index, float value)
    {
        scrollOffsets[index] += value;
    }
    
    // Update is called once per frame
    void Update()
    {
        //Only update the Background Objects if the game isn't paused.
        if (!LevelManager.instance.gamePaused)
        {
            //Move the background and foreground layers at different speeds, using the values in "scrollValues":
            //0: backgroundLayer1; 1: backgroundLayer2; 2: backgroundLayer3; 3: backgroundLayer4; 4: foregroundLayer
            for (int i = 0; i < backgroundLayer1.Length; i++)
            {
                backgroundLayer1[i].transform.Translate(scrollValues[0] + scrollOffsets[0], 0, 0);
            }
            for (int i = 0; i < backgroundLayer2.Length; i++)
            {
                backgroundLayer2[i].transform.Translate(scrollValues[1] + scrollOffsets[1], 0, 0);
            }
            for (int i = 0; i < backgroundLayer3.Length; i++)
            {
                backgroundLayer3[i].transform.Translate(scrollValues[2] + scrollOffsets[2], 0, 0);
            }
            for (int i = 0; i < backgroundLayer4.Length; i++)
            {
                backgroundLayer4[i].transform.Translate(scrollValues[3] + scrollOffsets[3], 0, 0);
            }
            for (int i = 0; i < foregroundLayer.Length; i++)
            {
                foregroundLayer[i].transform.Translate(scrollValues[4] + scrollOffsets[4], 0, 0);
            }
        }
    }
}