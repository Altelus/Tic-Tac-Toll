/*******************************************************************************
Filename:   TileScript.cs
Author:     Geoffrey Mok
Date:       Oct 26, 2014
Purpose:    Tileplanes used with player mouse input, also plays
 *          impact sound when a piece enters the trigger volume (hits the table)
*******************************************************************************/
using UnityEngine;
using System.Collections;

public class TileScript : MonoBehaviour {

    public int PieceNo;
    private AudioSource[] soundEffects;

	void Start () {
        soundEffects = GetComponents<AudioSource>();
	}
	
    void OnTriggerEnter(Collider collider)
    {
        if (collider.transform.tag == "O")
            soundEffects[1].Play();
        else if (collider.transform.tag == "X")
            soundEffects[0].Play();
    }
}
