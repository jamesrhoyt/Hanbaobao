/*
 * Gradient.cs
 * 
 * Applies a vertical "gradient" effect to any Object this Script is attached to as a Component, which overwrites its Color value.
 * (Currently, used solely for Text objects in the HUD and the High Score Screen.)
 * 
 */

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[AddComponentMenu("UI/Effects/Gradient")]
public class Gradient : BaseMeshEffect
{
    //Use this to move the gradient up and down the UI Element.
    [SerializeField]
    [Range(-1.5f, 1.5f)]
    public float Offset = 0f;   //How much to delay transitioning from one Color to the next.

    [SerializeField]
    public Color32 StartColor = Color.gray; //The first (top) color in the gradient.
    [SerializeField]
    public Color32 MidColor = Color.white; //The second (middle) color in the gradient.
    [SerializeField]
    public Color32 EndColor = Color.gray; //The third (bottom) color in the gradient.

    //Create a list of vertices to be colored in ModifyVertices.
    public override void ModifyMesh(VertexHelper vh)
    {
        if (!IsActive())
            return;

        List<UIVertex> list = new List<UIVertex>();
        vh.GetUIVertexStream(list);

        ModifyVertices(list);

        vh.Clear();
        vh.AddUIVertexTriangleStream(list);
    }

    //Color each vertex in the mesh based off of its vertical position.
    public void ModifyVertices(List<UIVertex> vertices)
    {
        if (!IsActive())
            return;

        //Initialize the variables to find the top and bottom of the UI element.
        int count = vertices.Count;
        float bottomY = vertices[0].position.y;
        float topY = vertices[0].position.y;
        float y = 0f;

        //Iterate through all of the vertices to find the minimum and maximum y-values for the UI element.
        for (int i = 1; i < count; i++)
        {
            y = vertices[i].position.y;
            //Get the highest vertex y-value to help guide the interpolation.
            if (y > topY)
                topY = y;
            //Get the lowest vertex y-value to help guide the interpolation.
            else if (y < bottomY)
                bottomY = y;
        }

        //Get the height of the UI Element by subtracting the highest y-value from the lowest one.
        float uiElementHeight = topY - bottomY;

        //Take each vertex out of the list, modify its color, then put it back in the list.
        for (int i = 0; i < count; i++)
        {
            //Create a temporary Vertex to edit.
            UIVertex uiVertex = vertices[i];
            //If the vertex is in the upper half of the UI Element, Lerp between the first two colors.
            if(uiVertex.position.y - bottomY >= (uiElementHeight / 2))
                uiVertex.color = Color32.Lerp(MidColor, StartColor, (((uiVertex.position.y - bottomY) - uiElementHeight) / uiElementHeight) - Offset);
            //If the vertex is in the lower half of the UI Element, Lerp between the last two colors.
            else
                uiVertex.color = Color32.Lerp(EndColor, MidColor, ((uiVertex.position.y - bottomY) / uiElementHeight) - Offset);
            //Clamp the vertex's red, green, and blue values to a predetermined palette.
            uiVertex.color.r = RoundToPalette(uiVertex.color.r);
            uiVertex.color.g = RoundToPalette(uiVertex.color.g);
            uiVertex.color.b = RoundToPalette(uiVertex.color.b);
            //Save the new Vertex back into the list.
            vertices[i] = uiVertex;
        }
    }

    //Round the RGB values of a vertex's color to a predetermined palette.
    //Each RGB value should equal one of these numbers: [0, 52, 87, 116, 144, 172, 206, 255]
    public byte RoundToPalette(byte colorValue)
    {
        //Check which value to round colorInt to, based on the midpoints between the acceptable values.
        if (colorValue <= 26)
            colorValue = 0;
        else if (colorValue >= 27 && colorValue <= 70)
            colorValue = 52;
        else if (colorValue >= 71 && colorValue <= 101)
            colorValue = 87;
        else if (colorValue >= 102 && colorValue <= 130)
            colorValue = 116;
        else if (colorValue >= 131 && colorValue <= 158)
            colorValue = 144;
        else if (colorValue >= 159 && colorValue <= 189)
            colorValue = 172;
        else if (colorValue >= 190 && colorValue <= 230)
            colorValue = 206;
        else if (colorValue >= 231)
            colorValue = 255;
        return colorValue;
    }
}