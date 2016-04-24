﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections;

/// <summary>
/// Class handles sliding pieces puzzle type creation and user interaction
/// </summary>
public class SPPuzzleController : MonoBehaviour
{

#region "Public Variables"

    /// <summary>
    /// Main puzzle image from which pieces will be created
    /// </summary>
    public Texture2D PuzzleImage;
    /// <summary>
    /// If set to true pieces will be shuffled on puzzle creation
    /// </summary>
    public bool ShufflePiecesAtStart = true;
    /// <summary>
    /// Display helping background image?
    /// </summary>
    public bool DisplayBackgroundImage = true;

    /// <summary>
    /// Speed of animation for sliding of pieces
    /// </summary>
    [Space(15)]
    [Range(5,15)]
    public float SlidingSpeed = 1f;

    /// <summary>
    /// No of rows in puzzle
    /// </summary>
    [Range(3, 10)]
    public int TotalRows = 3;
    /// <summary>
    /// No of columns in puzzle
    /// </summary>
    [Range(3, 10)]
    public int TotalCols = 3;

    /// <summary>
    /// Sound to be played when piece slides to user gestured position
    /// </summary>
    [Space(15)]
    //Music and sfx variables
    public AudioClip PieceSlide;
    /// <summary>
    /// Played when puzzle is solved
    /// </summary>
    public AudioClip PuzzleCompletionSound;
    /// <summary>
    /// Continuous running background AudioClip
    /// </summary>
    public AudioClip BackgroundMusic;


    /// <summary>
    /// Puzzle Event In Editor: Called when piece moved to destination position
    /// </summary>
    [Space(20)]
    public UnityEvent OnPieceMovementComplete;
    /// <summary>
    /// Puzzle Event In Editor: Called when puzzle is completed / all pieces are joined
    /// 
    /// </summary>
    public UnityEvent OnPuzzleSolved;

#endregion

#region "Private Variables"
 
    private GameObject[][] _pieceInstances;
    private Vector3[][] _piecePositions;        //Holds positions of pieces for animation pupose

    private GameObject _spPuzzlePiece;
    private GameObject _spFrame;
    private GameObject _spBackground;

    private const float PiecesSpacing = 0.01f;


    private const float PieceWidthInWorld = 1f;

    private GameObject _currentClickedPiece = null;
    private Vector3 _mouseDownPosition = Vector3.zero;

    //Holds instance for audio manager
    private AudioManager _audioManager;

    private GameObject _currentPieceToMove = null;
    private ESPMovementDirection _curPieceMovDirection = ESPMovementDirection.None;
    private int _pieceFromRow = -1;
    private int _pieceFromCol = -1;

    private Rect _frameRectangle;

#endregion


    void Awake ()
    {
        _audioManager = GetComponent<AudioManager>();

    }

	void Start () {
        if (BackgroundMusic != null)
            _audioManager.PlayMusic(BackgroundMusic);

        //int TotalPieces = TotalCols * TotalRows;

        //Find piece gameobject
        _spPuzzlePiece = gameObject.transform.FindChild("PuzzlePieceSP").gameObject;

        //Assign image to puzzle piece instance
        _spPuzzlePiece.GetComponent<Renderer>().material.mainTexture = PuzzleImage;

        //Find frame gameobject
        _spFrame = gameObject.transform.FindChild("SPFrameObj").gameObject;


        //Seperate from cam for cam resizing to adjust whole puzzle in cam view
        Transform parentCamTransform = gameObject.transform.parent;
        Camera parentCam = parentCamTransform.GetComponent<Camera>();
        gameObject.transform.parent = null;


#region "Initialize pieces and position them in place"

        _pieceInstances = new GameObject[TotalRows][];
        _piecePositions = new Vector3[TotalRows][];

        int PieceWidth = PuzzleImage.width / TotalCols;
        int PieceHeight = PuzzleImage.height / TotalRows;

        float PieceHeightWidthRatio = (float)PieceHeight / (float)PieceWidth;

        for (int Rowtrav = 0; Rowtrav < TotalRows; Rowtrav++)
        {
            _pieceInstances[Rowtrav] = new GameObject[TotalCols];
            _piecePositions[Rowtrav] = new Vector3[TotalCols];

            for (int Coltrav = 0; Coltrav < TotalCols; Coltrav++)
            {
                //Instantiate
                _pieceInstances[Rowtrav][Coltrav] = GameObject.Instantiate(_spPuzzlePiece) as GameObject;
                
                JigsawPuzzleCreator.SetPuzzlePieceUV(_pieceInstances[Rowtrav][Coltrav], Coltrav, Rowtrav,
                    TotalRows, TotalCols);


                //Name this piece instance
                _pieceInstances[Rowtrav][Coltrav].name = ArrIndexToPieceName(Rowtrav, Coltrav);

                //Make child of main gameobject
                _pieceInstances[Rowtrav][Coltrav].transform.parent = gameObject.transform;

                //Resize
                _pieceInstances[Rowtrav][Coltrav].transform.localScale = new Vector3(PieceWidthInWorld,
                                            PieceWidthInWorld * PieceHeightWidthRatio, 1);

                //Place in position
                float PositionX = //gameObject.transform.position.x + 
                    (Coltrav * PieceWidthInWorld) + (Coltrav * PiecesSpacing);
                float PositionY = //gameObject.transform.position.y + 
                    (Rowtrav * PieceWidthInWorld * PieceHeightWidthRatio) + (Rowtrav * PiecesSpacing * 1.02f);

                Vector3 CalcPosition = new Vector3(PositionX, PositionY, 0);

                _pieceInstances[Rowtrav][Coltrav].transform.localPosition = CalcPosition;
                _piecePositions[Rowtrav][Coltrav] = CalcPosition;

                //Enable instance
                _pieceInstances[Rowtrav][Coltrav].SetActive(true);

            }
        }

        //Disable last piece in array so that space for sliding will be created
        Destroy(_pieceInstances[0][TotalCols - 1]);
        _pieceInstances[0][TotalCols - 1] = null;

#endregion

        //Resize camera to display whole puzzle in camera view
        float widthToBeSeen = TotalCols * PieceWidthInWorld + (PiecesSpacing * (TotalCols - 1));
        float heightToBeSeen = TotalRows * PieceWidthInWorld * PieceHeightWidthRatio + (PiecesSpacing * (TotalRows - 1));

        parentCam.orthographicSize = widthToBeSeen > heightToBeSeen ?  widthToBeSeen * 2.4f : heightToBeSeen * 2.4f;

        //Position camera in centre of puzzle
        float CalcCameraX = ((_pieceInstances[1][TotalCols-1].transform.position.x -
            _pieceInstances[1][0].transform.position.x)/2) + _pieceInstances[1][0].transform.position.x;
        float CalcCameraY = ((_pieceInstances[TotalRows - 1][0].transform.position.y -
            _pieceInstances[0][0].transform.position.y) / 2) + _pieceInstances[0][0].transform.position.y;

        parentCamTransform.position = new Vector3(CalcCameraX, CalcCameraY, _pieceInstances[1][0].transform.position.z - 3);


        //Resize Frame
        float xScaleOfFrame = (_pieceInstances[0][0].transform.localScale.x * TotalCols) * 1.10f + (PiecesSpacing * (TotalCols-1));
        float yScaleOfFrame = (_pieceInstances[0][0].transform.localScale.y * TotalRows) * 1.15f + (PiecesSpacing * (TotalRows-1));
        _spFrame.transform.localScale = new Vector3(xScaleOfFrame, yScaleOfFrame, 1);

        //Position Frame
        float xPosFrame = parentCamTransform.position.x;
        float yPosFrame = parentCamTransform.position.y;
        float zPosFrame = parentCamTransform.position.z + 1;
        _spFrame.transform.position = new Vector3(xPosFrame, yPosFrame, zPosFrame);

        _spFrame.SetActive(true);

        //Shuffle pieces
        if (ShufflePiecesAtStart)
            ShufflePuzzle();


#region "Background Image Work"

        if (DisplayBackgroundImage)
        {
            //Get background gameobject
            _spBackground = transform.FindChild("SPBackground").gameObject;

            //Set background texture
            _spBackground.GetComponent<Renderer>().material.mainTexture = ImgToBackground(PuzzleImage);

            //Resize background object
            float xScaleBackground = (_pieceInstances[0][0].transform.localScale.x * TotalCols) + (PiecesSpacing * (TotalCols - 1));
            float yScaleBackground = (_pieceInstances[0][0].transform.localScale.y * TotalRows) * 1.15f + (PiecesSpacing * (TotalRows - 1));
            _spBackground.transform.localScale = new Vector3(xScaleBackground, yScaleBackground, 1);

            //Position background object
            _spBackground.transform.position = new Vector3(parentCamTransform.position.x, parentCamTransform.position.y,
                                            _pieceInstances[0][0].transform.position.z + 1);

            //Activate background
            _spBackground.SetActive(true);
        }

#endregion

    }
	
	void Update () {
        
        if (Input.GetMouseButtonDown(0) && _currentPieceToMove == null)
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit[] hits = Physics.RaycastAll(ray, 30f);

            int pieceHitIndex = -1;

            for ( int hitsTrav = 0; hitsTrav < hits.Length; hitsTrav++ ){
                if ( hits[hitsTrav].collider.transform.name.Contains("Piece") ){
                    pieceHitIndex = hitsTrav;
                    break;
                }
            }


            if (pieceHitIndex > -1)
            {
                _currentClickedPiece = hits[pieceHitIndex].collider.gameObject;
                _mouseDownPosition = Input.mousePosition;
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (_currentClickedPiece != null)
            {

#region "Piece Movement Work"

                //Check which direction this piece needs to be moved
                ESPMovementDirection CalcMovementDirection = ESPMovementDirection.None;
                Vector3 currentMousePosition = Input.mousePosition;

                int DetectionThreshold = 20;

                //Check for left right movement
                if (currentMousePosition.y > _mouseDownPosition.y - DetectionThreshold &&
                        currentMousePosition.y < _mouseDownPosition.y + DetectionThreshold)
                {
                    if (currentMousePosition.x < _mouseDownPosition.x)
                        CalcMovementDirection = ESPMovementDirection.Left;
                    else if (currentMousePosition.x > _mouseDownPosition.x)
                        CalcMovementDirection = ESPMovementDirection.Right;

                }
                //Check for top down movement
                else if(currentMousePosition.x > _mouseDownPosition.x - DetectionThreshold &&
                            currentMousePosition.x < _mouseDownPosition.x + DetectionThreshold)
                {
                    if (currentMousePosition.y < _mouseDownPosition.y)
                        CalcMovementDirection = ESPMovementDirection.Down;
                    else if (currentMousePosition.y > _mouseDownPosition.y)
                        CalcMovementDirection = ESPMovementDirection.Top;
                }


                //If that slot doesn`t have a piece already in it and this piece can move there?
                if (CalcMovementDirection != ESPMovementDirection.None)
                {

                    _currentPieceToMove = _currentClickedPiece;
                    _curPieceMovDirection = CalcMovementDirection;

                    int curPieceRow = 0;
                    int curPieceCol = 0;

                    PieceInstToArrIndex(_currentPieceToMove, out curPieceRow, out curPieceCol);

                    _pieceFromCol = curPieceCol;
                    _pieceFromRow = curPieceRow;


                    switch (_curPieceMovDirection)
                    {
                        case ESPMovementDirection.Left:
                            //Check for any piece present in left area
                            if (curPieceCol > 0)
                            {
                                if (_pieceInstances[curPieceRow][curPieceCol - 1] == null)
                                {
                                    goto MovementAllowed;
                                }
                            }
                            goto default;

                        case ESPMovementDirection.Right:
                            if (curPieceCol < TotalCols - 1)
                            {
                                if (_pieceInstances[curPieceRow][curPieceCol + 1] == null)
                                {
                                    goto MovementAllowed;
                                }
                            }
                            goto default;

                        case ESPMovementDirection.Top:
                            if (curPieceRow < TotalRows - 1)
                            {
                                if (_pieceInstances[curPieceRow + 1][curPieceCol] == null)
                                {
                                    goto MovementAllowed;
                                }
                            }
                            goto default;

                        case ESPMovementDirection.Down:
                            if (curPieceRow > 0)
                            {
                                if (_pieceInstances[curPieceRow - 1][curPieceCol] == null)
                                {
                                    goto MovementAllowed;
                                }
                            }
                            goto default;


                        MovementAllowed:
                            Debug.Log("movement allowed for direction " + _curPieceMovDirection);
                            break;

                        default:
                            //Cancel movement
                            _currentPieceToMove = null;
                            _curPieceMovDirection = ESPMovementDirection.None;

                            _pieceFromRow = -1;
                            _pieceFromCol = -1;
                            break;
                    }

                }

#endregion

                _mouseDownPosition = Vector3.zero;
                _currentClickedPiece = null;

            }
        }


#region "Piece Movement Animation Work"

        if (_currentPieceToMove != null)
        {
            switch (_curPieceMovDirection)
            {
                case ESPMovementDirection.Left:
                    if (SlidePieceLeft(_currentPieceToMove, _pieceFromRow, _pieceFromCol))
                        goto default;
                    break;

                case ESPMovementDirection.Right:
                    if (SlidePieceRight(_currentPieceToMove, _pieceFromRow, _pieceFromCol))
                        goto default;
                    break;

                case ESPMovementDirection.Top:
                    if (SlidePieceUp(_currentPieceToMove, _pieceFromRow, _pieceFromCol))
                        goto default;
                    break;

                case ESPMovementDirection.Down:
                    if (SlidePieceDown(_currentPieceToMove, _pieceFromRow, _pieceFromCol))
                        goto default;
                    break;


                default:
                    //Reset for next animation
                    _currentPieceToMove = null;
                    _curPieceMovDirection = ESPMovementDirection.None;

                    _pieceFromRow = -1;
                    _pieceFromCol = -1;

                    OnPieceMovementComplete.Invoke();
                    E_OnPieceMovementComplete();
                    _audioManager.PlaySFXSound(PieceSlide);

                    //Check if puzzle is solved
                    if (IsPuzzleSolved())
                    {
                        OnPuzzleSolved.Invoke();
                        E_OnPuzzleSolved();
                        _audioManager.PlaySFXSound(PuzzleCompletionSound);
                    }
                    
                    
                    break;
            }
        }

#endregion


    }


    

    /// <summary>
    /// Searches for this instance of piece in pieceinstance holder array
    /// and returns its row and col where this piece exists.
    /// </summary>
    /// <param name="PieceInstance">Instance of piece to be seached in array</param>
    /// <param name="Row">Output row of this piece in instance holder array . Value should be greate then -1</param>
    /// <param name="Col">Output col of this piece in instance holder array . Value should be greate then -1</param>
    private void PieceInstToArrIndex(GameObject PieceInstance, out int Row, out int Col)
    {
        Row = -1;
        Col = -1;

        for (int Rowtrav = 0; Rowtrav < TotalRows; Rowtrav++)
        {
            for (int Coltrav = 0; Coltrav < TotalCols; Coltrav++)
            {
                if (Object.ReferenceEquals(PieceInstance, _pieceInstances[Rowtrav][Coltrav]))
                {
                    Row = Rowtrav;
                    Col = Coltrav;
                    return;
                }
            }
        }

    }

    /// <summary>
    /// Merges provided row and column number to form piece name
    /// </summary>
    /// <param name="Row">Row number to be inserted in name . Value should be greate then -1</param>
    /// <param name="Col">Col number to be inserted in name . Value should be greate then -1</param>
    /// <returns>Returns piece name created using provided position</returns>
    private string ArrIndexToPieceName(int Row, int Col)
    {
        return "SPPiece" + Row.ToString() + "_" + Col.ToString();
    }

    /// <summary>
    /// Randomly shuffle pieces in instance holder variable and in world
    /// </summary>
    private void ShufflePuzzle()
    {
        for (int i = 0; i < TotalRows * TotalCols; i++)
        {
            int RndSrcRow = Random.Range(0, TotalRows - 1);
            int RndSrcCol = Random.Range(0, TotalCols - 1);

            int RndDestRow = Random.Range(0, TotalRows - 1);
            int RndDestCol = Random.Range(0, TotalCols - 1);

            //Swap positions of gameobjects
            if ( _pieceInstances[RndSrcRow][RndSrcCol] != null )
                _pieceInstances[RndSrcRow][RndSrcCol].transform.localPosition = _piecePositions[RndDestRow][RndDestCol];

            if ( _pieceInstances[RndDestRow][RndDestCol] != null )
                _pieceInstances[RndDestRow][RndDestCol].transform.localPosition = _piecePositions[RndSrcRow][RndSrcCol];

            //Swap gameobjects
            GameObject Temp = _pieceInstances[RndSrcRow][RndSrcCol];
            _pieceInstances[RndSrcRow][RndSrcCol] = _pieceInstances[RndDestRow][RndDestCol];
            _pieceInstances[RndDestRow][RndDestCol] = Temp;
        }

    }

    /// <summary>
    /// Checks whether puzzle is solved.
    /// </summary>
    /// <returns>Returns true if puzzle is solved</returns>
    private bool IsPuzzleSolved()
    {
        for (int Rowtrav = 0; Rowtrav < TotalRows; Rowtrav++)
        {
            for (int Coltrav = 0; Coltrav < TotalCols; Coltrav++)
            {
                if ( _pieceInstances[Rowtrav][Coltrav] != null )
                    if (_pieceInstances[Rowtrav][Coltrav].name != ArrIndexToPieceName(Rowtrav, Coltrav))
                        return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Converts puzzle image to grayscale background image.
    /// </summary>
    /// <param name="PuzzleImage">Puzzle texture to be converted</param>
    /// <returns>Returns created background image</returns>
    private Texture2D ImgToBackground(Texture2D PuzzleImage)
    {
        Texture2D ImageCopy = Object.Instantiate(PuzzleImage) as Texture2D;

        //Give image an effect
        Color[] texColors = ImageCopy.GetPixels();
        for (int i = 0; i < texColors.Length; i++)
        {
            float grayValue = texColors[i].grayscale;
            texColors[i] = new Color(grayValue, grayValue, grayValue, texColors[i].a);
        }

        ImageCopy.SetPixels(texColors);

        ImageCopy.Apply();

        return ImageCopy;
    }




    /// <summary>
    /// Slide piece down with animation
    /// </summary>
    /// <param name="PieceToSlide">gameobject of piece to slide</param>
    /// <param name="PieceFromRow">Row number of piece to be moved . Value should be greate then -1</param>
    /// <param name="PieceFromCol">Col number of piece to be moves . Value should be greate then -1</param>
    /// <returns>True when piece reaches to its destination</returns>
    private bool SlidePieceDown(GameObject PieceToSlide, int PieceFromRow, int PieceFromCol)
    {
        PieceToSlide.transform.localPosition = new Vector3(PieceToSlide.transform.localPosition.x, 
                Mathf.Lerp( PieceToSlide.transform.localPosition.y, _piecePositions[PieceFromRow-1][PieceFromCol].y, Time.deltaTime * SlidingSpeed ), 
                PieceToSlide.transform.localPosition.z );

        if (PieceToSlide.transform.localPosition == _piecePositions[PieceFromRow - 1][PieceFromCol])
        {
            //Change position in array
            _pieceInstances[PieceFromRow][PieceFromCol] = null;
            _pieceInstances[PieceFromRow - 1][PieceFromCol] = PieceToSlide;

            return true;
        }
        

        return false;
    }

    /// <summary>
    /// Slide piece up with animation
    /// </summary>
    /// <param name="PieceToSlide">gameobject of piece to slide</param>
    /// <param name="PieceFromRow">Row number of piece to be moved . Value should be greate then -1</param>
    /// <param name="PieceFromCol">Col number of piece to be moves . Value should be greate then -1</param>
    /// <returns>True when piece reaches to its destination</returns>
    private bool SlidePieceUp(GameObject PieceToSlide, int PieceFromRow, int PieceFromCol)
    {
        PieceToSlide.transform.localPosition = new Vector3(PieceToSlide.transform.localPosition.x,
                Mathf.Lerp(PieceToSlide.transform.localPosition.y, _piecePositions[PieceFromRow +1][PieceFromCol].y, Time.deltaTime * SlidingSpeed),
                PieceToSlide.transform.localPosition.z);

        if (PieceToSlide.transform.localPosition == _piecePositions[PieceFromRow + 1][PieceFromCol])
        {
            //Change position in array
            _pieceInstances[PieceFromRow][PieceFromCol] = null;
            _pieceInstances[PieceFromRow + 1][PieceFromCol] = PieceToSlide;

            return true;
        }


        return false;
    }

    /// <summary>
    /// Slide piece left with animation
    /// </summary>
    /// <param name="PieceToSlide">gameobject of piece to slide</param>
    /// <param name="PieceFromRow">Row number of piece to be moved . Value should be greate then -1</param>
    /// <param name="PieceFromCol">Col number of piece to be moves . Value should be greate then -1</param>
    /// <returns>True when piece reaches to its destination</returns>
    private bool SlidePieceLeft(GameObject PieceToSlide, int PieceFromRow, int PieceFromCol)
    {
        PieceToSlide.transform.localPosition = new Vector3(Mathf.Lerp( PieceToSlide.transform.localPosition.x,
                        _piecePositions[PieceFromRow][PieceFromCol-1].x, Time.deltaTime * SlidingSpeed),
                PieceToSlide.transform.localPosition.y,
                PieceToSlide.transform.localPosition.z);

        if (PieceToSlide.transform.localPosition == _piecePositions[PieceFromRow][PieceFromCol - 1])
        {
            //Change position in array
            _pieceInstances[PieceFromRow][PieceFromCol] = null;
            _pieceInstances[PieceFromRow][PieceFromCol - 1] = PieceToSlide;

            return true;
        }
        
        return false;
    }

    /// <summary>
    /// Slide piece right with animation
    /// </summary>
    /// <param name="PieceToSlide">gameobject of piece to slide</param>
    /// <param name="PieceFromRow">Row number of piece to be moved . Value should be greate then -1</param>
    /// <param name="PieceFromCol">Col number of piece to be moves . Value should be greate then -1</param>
    /// <returns>True when piece reaches to its destination</returns>
    private bool SlidePieceRight(GameObject PieceToSlide, int PieceFromRow, int PieceFromCol)
    {
        PieceToSlide.transform.localPosition = new Vector3(Mathf.Lerp(PieceToSlide.transform.localPosition.x,
                        _piecePositions[PieceFromRow][PieceFromCol + 1].x, Time.deltaTime * SlidingSpeed),
                PieceToSlide.transform.localPosition.y,
                PieceToSlide.transform.localPosition.z);

        if (PieceToSlide.transform.localPosition == _piecePositions[PieceFromRow][PieceFromCol + 1])
        {
            //Change position in array
            _pieceInstances[PieceFromRow][PieceFromCol] = null;
            _pieceInstances[PieceFromRow][PieceFromCol + 1] = PieceToSlide;

            return true;
        }

        return false;
    }




    /// <summary>
    /// Resets puzzle to start
    /// </summary>
    public void Reset()
    {
        ShufflePuzzle();
    }
    



    /// <summary>
    /// Called on successfull completion of this puzzle
    /// </summary>
    public void E_OnPuzzleSolved()
    {
        Debug.LogWarning("Puzzle is solved");
    }

    /// <summary>
    /// Called when piece successfully completes its sliding animation
    /// </summary>
    public void E_OnPieceMovementComplete()
    {
        Debug.LogWarning("Movement completed");
    }


    /// <summary>
    /// Pieces sliding directions
    /// </summary>
    public enum ESPMovementDirection
    {
        Left,
        Right,
        Top,
        Down,
        None
    }

}
