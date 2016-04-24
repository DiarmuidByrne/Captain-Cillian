using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Handles Memory puzzle generation and mechanism
/// </summary>
public class MPPuzzleController : MonoBehaviour
{

#region "Public Variables"

    /// <summary>
    /// Image to show when piece is hidden
    /// </summary>
    public Texture2D FlippedPieceImage;

    /// <summary>
    /// Total number of pieces a user have to flip open for matches to be successfull 
    /// </summary>
    public int NoPiecesToFind = 2;
    /// <summary>
    /// Total different types of pieces hidden in puzzle
    /// </summary>
    public int TotalTypes = 3;
    /// <summary>
    /// Total number of rows in puzzle, calculated by editor
    /// </summary>
    public int TotalRows = 3;
    /// <summary>
    /// Total number of columns in puzzle, calculated by editor
    /// </summary>
    public int TotalCols = 3;
    
    /// <summary>
    /// Holds different types of user provided hidden images for this puzzle
    /// </summary>
    public Texture2D[] HiddenImages;

    /// <summary>
    /// If set to true wrong piece effect is shown in pieces
    /// </summary>
    public bool AnimateColorOnWrongPiece = true;
    /// <summary>
    /// Color to be animated on wrong piece open
    /// </summary>
    public Color WrongPieceAnimationColor = Color.red;
    /// <summary>
    /// Speed of wrong effect animation pauses between blinks
    /// </summary>
    public float BlinkSpeed = 0.1f;
    /// <summary>
    /// Total number of blinks for wrong effect
    /// </summary>
    public int NoOfTimesToBlink = 2;


    //Music and sfx variables
    /// <summary>
    /// Played when incorrect piece pairs are opened
    /// </summary>
    [HideInInspector]
    public AudioClip IncorrectPairOpened;
    /// <summary>
    /// Played when correct piece pairs are opened
    /// </summary>
    [HideInInspector]
    public AudioClip CorrectPairOpened;
    /// <summary>
    /// Played when all pairs have been opened
    /// </summary>
    [HideInInspector]
    public AudioClip PuzzleCompletionSound;
    /// <summary>
    /// Continuously played background music
    /// </summary>
    [HideInInspector]
    public AudioClip BackgroundMusic;

    /// <summary>
    /// Puzzle Event In Editor: Calls User provided method when wrong pair is opened
    /// </summary>
    [HideInInspector]
    public UnityEvent OnWrongPiecesOpened;
    /// <summary>
    /// Puzzle Event In Editor: Calls user provided method when correct pair is opened
    /// </summary>
    [HideInInspector]
    public UnityEvent OnMatchedPiecesOpened;
    /// <summary>
    /// Puzzle Event In Editor: Calls user provided method when all pieces are successfully opened
    /// </summary>
    [HideInInspector]
    public UnityEvent OnAllPiecesOpened;

#endregion

#region "Private Variables"

    private GameObject _masterPieceObj;
    private GameObject _spFrame;
    private GameObject[][] _pieceInstances;
    private MPPieceController[][] _pieceControllerInstances;
    private Vector3[][] _piecePositions;
    private MPPieceController _pieceControllers;

    private int _correctOpenedPieces = 0;

    private List<GameObject> _openedPieces = new List<GameObject>();

    //Holds instance for audio manager
    private AudioManager _audioManager;

    private const float PiecesSpacing = 0.01f;
    private const float PieceWidthHeightInWorld = 1f;


#endregion

    /// <summary>
    /// UnityMethod: Get required compoenent instances
    /// </summary>
    void Awake ()
    {
        _audioManager = GetComponent<AudioManager>();

    }

    /// <summary>
    /// UnityMethod: Initializes puzzle and pieces and setup camera
    /// </summary>
	void Start () {

        if (BackgroundMusic != null)
            _audioManager.PlayMusic(BackgroundMusic);

        //Get master piece gameobject
        _masterPieceObj = gameObject.transform.FindChild("MPPiece").gameObject;

        //Seperate from cam for cam resizing to adjust whole puzzle in cam view
        Transform parentCamTransform = gameObject.transform.parent;
        Camera parentCam = parentCamTransform.GetComponent<Camera>();
        gameObject.transform.parent = null;

        //Find frame gameobject
        _spFrame = gameObject.transform.FindChild("SPFrameObj").gameObject;


#region "Generate Pieces"

        //Create Temporary queue with copies of images to be assigned
        List<Texture2D> TempHIList = new List<Texture2D>();
        for (int HPTrav = 0; HPTrav < HiddenImages.Length; HPTrav++)
            for (int PieceCopies = 0; PieceCopies < NoPiecesToFind; PieceCopies++)
                TempHIList.Add(HiddenImages[HPTrav]);


        _pieceInstances = new GameObject[TotalRows][];
        _pieceControllerInstances = new MPPieceController[TotalRows][];
        _piecePositions = new Vector3[TotalRows][];

        for (int Rowtrav = 0; Rowtrav < TotalRows; Rowtrav++)
        {
            _pieceInstances[Rowtrav] = new GameObject[TotalCols];
            _pieceControllerInstances[Rowtrav] = new MPPieceController[TotalCols];
            _piecePositions[Rowtrav] = new Vector3[TotalCols];


            for (int Coltrav = 0; Coltrav < TotalCols; Coltrav++)
            {
                //Instantiate new piece
                _pieceInstances[Rowtrav][Coltrav] = GameObject.Instantiate(_masterPieceObj) as GameObject;

                //Get piece controller
                MPPieceController TempPCInst = _pieceInstances[Rowtrav][Coltrav].GetComponent<MPPieceController>();
                _pieceControllerInstances[Rowtrav][Coltrav] = TempPCInst;

                //Name this piece instance
                _pieceInstances[Rowtrav][Coltrav].name = ArrIndexToPieceName(Rowtrav, Coltrav);

                //Make child of main gameobject
                _pieceInstances[Rowtrav][Coltrav].transform.parent = gameObject.transform;

                //Assign images
                TempPCInst.FlippedPieceImage = FlippedPieceImage;
                int SelectedPiece = Random.Range(0, TempHIList.Count);
                TempPCInst.HiddenPieceImage = TempHIList[SelectedPiece];
                TempHIList.RemoveAt(SelectedPiece);


                //Scale piece
                _pieceInstances[Rowtrav][Coltrav].transform.localScale = new Vector3(PieceWidthHeightInWorld, PieceWidthHeightInWorld, 1);


                //Place in position
                float PositionX = //gameObject.transform.position.x + 
                    (Coltrav * PieceWidthHeightInWorld) + (Coltrav * PiecesSpacing);
                float PositionY = //gameObject.transform.position.y + 
                    (Rowtrav * PieceWidthHeightInWorld) + (Rowtrav * PiecesSpacing);

                Vector3 CalcPosition = new Vector3(PositionX, PositionY, 0);

                _pieceInstances[Rowtrav][Coltrav].transform.localPosition = CalcPosition;

                _piecePositions[Rowtrav][Coltrav] = CalcPosition;



                //Enable instance
                _pieceInstances[Rowtrav][Coltrav].SetActive(true);

            }
        }

#endregion

        //Resize camera to display whole puzzle in camera view
        float widthToBeSeen = TotalCols * PieceWidthHeightInWorld + (PiecesSpacing * (TotalCols - 1));
        float heightToBeSeen = TotalRows * PieceWidthHeightInWorld + (PiecesSpacing * (TotalRows - 1));

        parentCam.orthographicSize = widthToBeSeen > heightToBeSeen ? widthToBeSeen * 0.4f : heightToBeSeen * 0.4f;

        //Position camera in centre of puzzle
        float CalcCameraX = ((_pieceInstances[1][TotalCols - 1].transform.position.x -
            _pieceInstances[1][0].transform.position.x) / 2) + _pieceInstances[1][0].transform.position.x;
        float CalcCameraY = ((_pieceInstances[TotalRows - 1][0].transform.position.y -
            _pieceInstances[0][0].transform.position.y) / 2) + _pieceInstances[0][0].transform.position.y;

        parentCamTransform.position = new Vector3(CalcCameraX, CalcCameraY, _pieceInstances[1][0].transform.position.z - 3);



        //Resize Frame
        float xScaleOfFrame = (_pieceInstances[0][0].transform.localScale.x * TotalCols) * 1.10f + (PiecesSpacing * (TotalCols - 1));
        float yScaleOfFrame = (_pieceInstances[0][0].transform.localScale.y * TotalRows) * 1.15f + (PiecesSpacing * (TotalRows - 1));
        _spFrame.transform.localScale = new Vector3(xScaleOfFrame, yScaleOfFrame, 1);

        //Position Frame
        float xPosFrame = parentCamTransform.position.x;
        float yPosFrame = parentCamTransform.position.y;
        float zPosFrame = parentCamTransform.position.z + 1;
        _spFrame.transform.position = new Vector3(xPosFrame, yPosFrame, zPosFrame);

        _spFrame.SetActive(true);

	}

    /// <summary>
    /// UnityMethod: Handles user input for pieces
    /// </summary>
    void Update() 
    {

        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 30f))
            {
                if (hit.transform.name.Contains("Piece"))
                {
                    MPPieceController Temp = hit.transform.GetComponent<MPPieceController>();

                    if (Temp.IsPieceHidden)
                    {
                        _openedPieces.Add(hit.transform.gameObject);

                        Temp.FlipPiece();

                        if (_openedPieces.Count == NoPiecesToFind)
                        {
                            if (MatchOpenedPieces())
                            {
                                _correctOpenedPieces += _openedPieces.Count;

                                //Check for all pieces opened correctly
                                if (_correctOpenedPieces == TotalCols * TotalRows)
                                {
                                    _audioManager.PlaySFXSound(PuzzleCompletionSound);
                                    E_OnAllPiecesOpened();
                                    OnAllPiecesOpened.Invoke();
                                }
                                else
                                {
                                    OnMatchedPiecesOpened.Invoke();
                                    E_OnMatchedPiecesOpened();
                                    _audioManager.PlaySFXSound(CorrectPairOpened);
                                }
                            }
                            else
                            {
                                if ( AnimateColorOnWrongPiece )
                                    GetComponent<ColorAnimator>().StartEffect(Temp.gameObject, WrongPieceAnimationColor, 
                                         NoOfTimesToBlink, BlinkSpeed);

                                CloseOpenedPieces();

                                _audioManager.PlaySFXSound(IncorrectPairOpened);
                                E_OnWrongPiecesOpened();
                                OnWrongPiecesOpened.Invoke();
                            }

                            _openedPieces.Clear();
                        }
                    }

                }

                
            }

        }

	}



    /// <summary>
    /// Matches opened pieces in opened pieces list
    /// </summary>
    /// <returns>Returns true if all pieces match</returns>
    private bool MatchOpenedPieces()
    {
        for (int i = 0; i < _openedPieces.Count - 1; i++)
        {
            MPPieceController Controller1 = _openedPieces[i].gameObject.GetComponent<MPPieceController>();
            MPPieceController Controller2 = _openedPieces[i+1].gameObject.GetComponent<MPPieceController>();

            if (!Object.ReferenceEquals(Controller1.HiddenPieceImage, Controller2.HiddenPieceImage))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Use to close all the pieces opened wrong
    /// </summary>
    private void CloseOpenedPieces()
    {
        int i = 0;
        for (i = 0; i < _openedPieces.Count; i++)
        {
            MPPieceController Controller = _openedPieces[i].gameObject.GetComponent<MPPieceController>();
            Controller.FlipPiece();
            Controller.HideOnShow();
        }

    }

    /// <summary>
    /// Merges provided row and column number to form piece name
    /// </summary>
    /// <param name="Row">Row number to be inserted in name . Value should be greate then -1</param>
    /// <param name="Col">Col number to be inserted in name . Value should be greate then -1</param>
    /// <returns>Returns piece name generated from provided positions</returns>
    private string ArrIndexToPieceName(int Row, int Col)
    {
        return "MPPiece" + Row.ToString() + "_" + Col.ToString();
    }




    /// <summary>
    /// Called when all the wrong pieces are opened
    /// </summary>
    public void E_OnWrongPiecesOpened()
    {
        Debug.Log("Wrong Pieces Opened");
    }

    /// <summary>
    /// Called when last piece with matching to previous pieces is opened
    /// </summary>
    public void E_OnMatchedPiecesOpened()
    {
        Debug.Log("Correct Pieces Opened");
    }

    /// <summary>
    /// Called when all pieces are opened
    /// </summary>
    public void E_OnAllPiecesOpened()
    {
        Debug.Log("All Pieces Opened");
    }


    /// <summary>
    /// Resets puzzle for replay
    /// </summary>
    public void Reset()
    {
        //Close all opened pieces
        for (int rowTrav = 0; rowTrav < TotalRows; rowTrav++)
        {
            for (int colTrav = 0; colTrav < TotalCols; colTrav++)
            {
                MPPieceController ThisPieceController = _pieceControllerInstances[rowTrav][colTrav];
                if (!ThisPieceController.IsPieceHidden)
                    ThisPieceController.FlipPiece();
            }
        }

        _openedPieces.Clear();
        _correctOpenedPieces = 0;


        //Reposition pieces places randomly
        for (int rowTrav = 0; rowTrav < TotalRows; rowTrav++) //Shuffle every row
        {
            for (int colTrav = 0; colTrav < TotalCols; colTrav++)
            {
                int r = Random.Range(0, TotalCols);

                //Swap positions
                Vector3 TempPosition = _pieceInstances[rowTrav][colTrav].transform.position;
                _pieceInstances[rowTrav][colTrav].transform.position = _pieceInstances[rowTrav][r].transform.position;
                _pieceInstances[rowTrav][r].transform.position = TempPosition;

                //Swap inside arrays
                GameObject TempGameobject = _pieceInstances[rowTrav][colTrav];
                _pieceInstances[rowTrav][colTrav] = _pieceInstances[rowTrav][r];
                _pieceInstances[rowTrav][r] = TempGameobject;

                MPPieceController TempMPPieceController = _pieceControllerInstances[rowTrav][colTrav];
                _pieceControllerInstances[rowTrav][colTrav] = _pieceControllerInstances[rowTrav][r];
                _pieceControllerInstances[rowTrav][r] = TempMPPieceController;

                Vector3 TempPiecePositions = _piecePositions[rowTrav][colTrav];
                _piecePositions[rowTrav][colTrav] = _piecePositions[rowTrav][r];
                _piecePositions[rowTrav][r] = TempPiecePositions;


            }

        }

    }


}
