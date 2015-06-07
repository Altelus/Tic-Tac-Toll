/*******************************************************************************
Filename:   MainMenuMaster.cs
Author:     Geoffrey Mok
Date:       Oct 26, 2014
Purpose:    Responsible for the main menu screen. Contains callbacks for GUI
 *          components
*******************************************************************************/
using UnityEngine;
using System.Collections;

public class MainMenuMaster : MonoBehaviour {

    // Button animations
    public TweenPosition StartButtonTween;
    public TweenPosition [] PieceSelectionTweens;

    public UILabel UILabelTitle;

    public GameObject CreditsScreenRoot;
    public GameObject[] TitleScreenElements;

    private bool creditsScreenOpen = false;
	void Start () {
	}
	
	void Update () {
	
	}

    // Callback for start button, show piece selection screen
    void ShowPieceSelection()
    {
        //UILabelTitle.enabled = false;
        StartButtonTween.Play(true);
        iTweenEvent.GetEvent(gameObject, "TiltToScene").Play();
        foreach (TweenPosition tp in PieceSelectionTweens)
        {
            tp.Play(true);
        }
    }

    // Callback for o button, set piece to o and play camera animation
    void O_Selected()
    {
        PlayerPrefs.SetInt("Piece", 0);
        StartGame();
    }

    // Callback for x button, set piece to x and play camera animation
    void X_Selected()
    {
        PlayerPrefs.SetInt("Piece", 1);
        StartGame();
    }

    // Plays camera animation
    void StartGame()
    {
        foreach (TweenPosition tp in PieceSelectionTweens)
        {
            tp.Play(false);
        }

        iTweenEvent.GetEvent(gameObject, "TransitionToGamePos").Play();
        iTweenEvent.GetEvent(gameObject, "TiltToGamePos").Play();
    }

    // Callback for camera animation, called when finished
    void LoadGame()
    {
        Application.LoadLevel("Game");
    }

    void ToogleShowCreditsScreen()
    {
        if (!creditsScreenOpen)
        {
            CreditsScreenRoot.SetActive(true);
            for (int i = 0; i < TitleScreenElements.Length; i ++)
            {
                TitleScreenElements[i].SetActive(false);
            }
        }
        else
        {
            CreditsScreenRoot.SetActive(false);
            for (int i = 0; i < TitleScreenElements.Length; i++)
            {
                TitleScreenElements[i].SetActive(true);
            }
        }

        creditsScreenOpen = !creditsScreenOpen;
    }
}
