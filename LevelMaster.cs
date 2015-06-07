/*******************************************************************************
Filename:   LevelMaster.cs
Author:     Geoffrey Mok
Date:       Oct 26, 2014
Purpose:    Script attached to the level master gameobject. Responsible for the
 *          tic tac toe game logic, including AI
*******************************************************************************/
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Container for minmax return
class Move
{
    public int boardPos;
    public int score;
}

public class LevelMaster : MonoBehaviour {

    // Input variables for game logic
    public GameObject[] GamePieces;
    public GameObject[] GhostGamePieces;
    public GameObject[] TilePlanes;
    public Material AltHoverMat;
    public LayerMask PlacementLayerMask;
    public string OpenPlacementTag = "PlacementOpen";
    public string ClosedPlacementTag = "PlacementClosed";

    public float spawnHeight = 10;

    // Results screen
    public GameObject UIResults;
    public UILabel UI_lbl_GameOutcome;

    public GameObject music;

    // Player indexes in piece array
    private int playerPiece = 0;
    private int aiPiece = 1;

    // Object player ray hit
    private GameObject lastHitObj;

    // Game logic
    private int[] currentBoard;
    private int[][] winConditions;
    private int[] availableMoves;
    private int winningCombo;

    private boardState currentBoardState;

    // Turn delays
    private float waitTime = 0.5f;
    private float aiDelayTime = .75f;
    private float menuDelayTime = 1f;

    // Flags
    private bool isPlayerTurn;
    private bool isGameOver = false;
    private bool isResultsShown = false;

    private GameObject[] _ghostPieces;
    private AudioSource[] soundResultEffects;

    // Board states for tic tac toe game
    enum boardState
    {
        NONE,
        PLAYER_WINS,
		AI_WINS,
        TIE,
    }

	void Start () {
        // Get selected piece from main menu, either x or o and assign them
        // to player and ai respectively
        //PlayerPrefs.SetInt("Piece", 0);
        playerPiece = PlayerPrefs.GetInt("Piece");
        aiPiece = playerPiece == 0 ? 1 : 0;
        
        // initialize board -1 = empty space
        currentBoard = new int[9];
        for (int i = 0; i < currentBoard.Length; i++ )
            currentBoard[i] = -1;

        // Assign all possible win conditions
        winConditions = new int[8][];
        winConditions[0] = new int[] { 0, 1, 2 };
        winConditions[1] = new int[] { 3, 4, 5 };
        winConditions[2] = new int[] { 6, 7, 8 };
        winConditions[3] = new int[] { 0, 3, 6 };
        winConditions[4] = new int[] { 1, 4, 7 };
        winConditions[5] = new int[] { 2, 5, 8 };
        winConditions[6] = new int[] { 0, 4, 8 };
        winConditions[7] = new int[] { 6, 4, 2 };

        // Randomize who goes first
        isPlayerTurn = (Random.value > 0.5f);

        // Create "ghost" pieces, phantom pieces showing player available moves
        _ghostPieces = new GameObject[TilePlanes.Length];
        for (int i = 0; i < TilePlanes.Length; i++)
        {
            _ghostPieces[i] = Instantiate(GhostGamePieces[playerPiece], TilePlanes[i].transform.position, GhostGamePieces[playerPiece].transform.rotation) as GameObject;
            _ghostPieces[i].SetActive(false);
        }

        // Load all sounds attached to LevelMaster
        soundResultEffects = GetComponents<AudioSource>();
    }

    void Update ()
    {
        if (Time.time > waitTime)
        {
            if (!isGameOver)
            {
                if (isPlayerTurn)
                {
                    for (int i = 0; i < Input.touchCount; i++)
                    {
                        if (Input.GetTouch(i).phase == TouchPhase.Began)
                        {
                            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                            RaycastHit hit;

                            // Raycast to an available tileplane, available positions will display a ghost piece
                            if (Physics.Raycast(ray, out hit, 1000f, PlacementLayerMask) && hit.collider.gameObject.tag.Equals(OpenPlacementTag))
                            {
                                if (lastHitObj)
                                {
                                    if (hit.collider.gameObject == lastHitObj)
                                    {
                                        _ghostPieces[lastHitObj.GetComponent<TileScript>().PieceNo].SetActive(false);
                                        PlacePiece(playerPiece, lastHitObj.GetComponent<TileScript>().PieceNo);
                                        waitTime = Time.time + aiDelayTime;

                                        isPlayerTurn = false;
                                        break;
                                    }

                                    //lastHitObj.renderer.enabled = false;
                                    _ghostPieces[lastHitObj.GetComponent<TileScript>().PieceNo].SetActive(false);
                                }

                                lastHitObj = hit.collider.gameObject;
                                //lastHitObj.renderer.enabled = true;
                                _ghostPieces[lastHitObj.GetComponent<TileScript>().PieceNo].SetActive(true);
                            }
                            else
                            {
                                if (lastHitObj)
                                {
                                    //lastHitObj.renderer.enabled = false;
                                    _ghostPieces[lastHitObj.GetComponent<TileScript>().PieceNo].SetActive(false);
                                    lastHitObj = null;
                                }
                            }


                            // placing piece on tile
                            //if (Input.GetTouch(i).tapCount == 2 && lastHitObj)
                            //{
                            //    _ghostPieces[lastHitObj.GetComponent<TileScript>().PieceNo].SetActive(false);
                            //    PlacePiece(playerPiece, lastHitObj.GetComponent<TileScript>().PieceNo);
                            //    waitTime = Time.time + aiDelayTime;

                            //    isPlayerTurn = false;
                            //}
                        }

                    }
                }
                else
                {
                    AITurn();
                    isPlayerTurn = true;
                }
            }
            else
                ShowResultsGUI();
        }
	}

    // Place a piece on the board and check new board state, if game finished, display results
    void PlacePiece(int player, int boardPos)
    {
        Vector3 spawnPos = TilePlanes[boardPos].transform.position;
        spawnPos.y = spawnHeight;
        Instantiate(GamePieces[player], spawnPos, GamePieces[playerPiece].transform.rotation);

		TilePlanes[boardPos].tag = ClosedPlacementTag;
		TilePlanes[boardPos].renderer.enabled = false;

        currentBoard[boardPos] = player;

        currentBoardState = CheckBoardState();
        isGameOver = (currentBoardState != boardState.NONE);

        if (isGameOver)
        {
            ShowWinningMove();
            waitTime = Time.time + menuDelayTime;
        }
    }

    // Checks board state by checking against win conditions
    boardState CheckBoardState()
    {
        boardState result = boardState.NONE;
        bool boardFull = true;

        // Check if player wins
        for (int i = 0; i < winConditions.Length; i++)
        {
            result = boardState.PLAYER_WINS;
            for (int j = 0; j < 3; j++)
            {
                if (currentBoard[winConditions[i][j]] != playerPiece)
                {
                    result = boardState.NONE;
                    break;
                }
            }
            if (result == boardState.PLAYER_WINS)
            {
                winningCombo = i;
                return result;
            }
        }

        // Check if AI wins
        for (int i = 0; i < winConditions.Length; i++)
        {
            result = boardState.AI_WINS;
            for (int j = 0; j < 3; j++)
            {
                if (currentBoard[winConditions[i][j]] != aiPiece)
                {
                    result = boardState.NONE;
                    break;
                }
            }
            if (result == boardState.AI_WINS)
            {
                winningCombo = i;
                return result;
            }
        }

        // Check if board full, if so is a tie
        for (int i = 0; i < currentBoard.Length; i++)
        {
            if (currentBoard[i] == -1)
            {
                boardFull = false;
                break;
            }
        }

        if (boardFull)
            result = boardState.TIE;
        return result;
    }

    // AI turn, wrapper for minmax
    void AITurn ()
    {
        Move move = MinMax(aiPiece, 0, 0, -999, 999);
        //Debug.Log("BEST SCORE : " + move.score);
        //Debug.Log("BEST MOVE : " + move.boardPos);

		PlacePiece (aiPiece, move.boardPos);
    }

    // AI logic to determine best move, recursive
    // pos = board positions
    // depth = score evaluation heuristic, focus on longer games if no winning moves
    // alpha/beta = for alpha beta pruning
    Move MinMax(int player, int pos, int depth, int alpha, int beta)
    {
        // Create return containter, mainly used for final return
        Move bestMove = new Move();
        bestMove.boardPos = pos;

        // Exit conditions and score calculations
        boardState result = CheckBoardState();
        if (result == boardState.TIE)
        {
            bestMove.score = 0;
            return bestMove;
        }
		else if (result == boardState.PLAYER_WINS)
	    {
			bestMove.score = depth - 10;
			return bestMove;
	    }
		else if (result == boardState.AI_WINS)
	    {
			bestMove.score = 10 - depth;
			return bestMove;
	    }

        depth++;

        List<int>moves = GetAvailableMoves();

        // Iterate through all available moves for current board, and call minmax on them
        foreach (int move in moves)
        {
            // temporarily make the move
            currentBoard[move] = player;

            // max, get highest score
            if (player == aiPiece)
            {
                int score = MinMax(playerPiece, move, depth, alpha, beta).score;

                if (score > alpha)
                {
                    alpha = score;

                    bestMove.score = alpha;
                    bestMove.boardPos = move;
                }
            } // min, get lowest score
            else
            {
                int score = MinMax(aiPiece, move, depth, alpha, beta).score;

                if (score < beta)
                {
                    beta = score;

                    bestMove.score = beta;
                    bestMove.boardPos = move;
                }
            }

            // undo the move after score evaluations
            currentBoard[move] = -1;

            // pruning, stop iterating through rest of available moves
            if (alpha >= beta)
            {
                break;
            }
        }

        bestMove.score = player == aiPiece ? alpha : beta;
        return bestMove;
    }

    // Returns a list of all available moves for current board state, available move = -1
    List <int> GetAvailableMoves()
    {
        List <int> moves = new List<int>();

        for (int i = 0; i < currentBoard.Length; i++)
        {
            if (currentBoard[i] == -1)
                moves.Add(i);
        }

        return moves;
    }

    // Displays winning move by displaying a red aura on the tiles
    void ShowWinningMove()
    {
        if (currentBoardState != boardState.TIE)
        {
            foreach (int tile in winConditions[winningCombo])
            {
                TilePlanes[tile].renderer.material = AltHoverMat;
                TilePlanes[tile].renderer.enabled = true;
            }
        }
    }

    // Display game over screen and play sound
    void ShowResultsGUI()
    {
        if (isGameOver && !isResultsShown)
        {
            music.audio.volume = 0.1f;
            isResultsShown = true;
            UIResults.gameObject.SetActive(true);
            if (currentBoardState == boardState.TIE)
            {
                UI_lbl_GameOutcome.text = "Tie!";
                soundResultEffects[2].Play();
            }
            else if (currentBoardState == boardState.AI_WINS)
            {
                UI_lbl_GameOutcome.text = "You Lose!";
                soundResultEffects[1].Play();
            }
            else if (currentBoardState == boardState.PLAYER_WINS)
            {
                UI_lbl_GameOutcome.text = "You Win!";
                soundResultEffects[0].Play();
            }
        }
    }

    // Callback for back button
    void GUI_Back()
    {
        Application.LoadLevel("MainMenu");
    }

    // Callback for replay button
    void GUI_Replay()
    {
        Application.LoadLevel("Game");
    }
}
