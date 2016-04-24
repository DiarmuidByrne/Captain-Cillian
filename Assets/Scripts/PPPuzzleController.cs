using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using PuzzleMaker;

/// <summary>
/// Class handles pick and place puzzle type creation and user interaction
/// </summary>
public class PPPuzzleController : UserControlledJigsawPuzzle
{
    /// <summary>
    /// Display mode for puzzle pieces for user pickup
    /// </summary>
    [HideInInspector]
    public EPieceDisplayMode PiecesDisplayMode = EPieceDisplayMode.ShowOnePieceAtATime;

    /// <summary>
    /// If enabled shuffles pieces
    /// </summary>
    [HideInInspector]
    public bool ShufflePieces = true;
    /// <summary>
    /// If true hides all pieces and display actualy image in single mesh when all pieces are joined
    /// </summary>
    [HideInInspector]
    public bool ActualImageOnPuzzleComplete = true;

    /// <summary>
    /// Animated color if placed in wrong place
    /// </summary>
    [HideInInspector]
    public bool AnimateColorOnWrongPiece = true;
    /// <summary>
    /// Wrong placement effect animation color
    /// </summary>
    [HideInInspector]
    public Color WrongPieceAnimationColor = Color.red;
    /// <summary>
    /// Wrong placement effect blink speed
    /// </summary>
    [HideInInspector]
    public float BlinkSpeed = 0.1f;
    /// <summary>
    /// No of times to blink wrong placement effect
    /// </summary>
    [HideInInspector]
    public int NoOfTimesToBlink = 2;


    //Music and sfx variables
    /// <summary>
    /// AudioClip to play when piece is placed in right place
    /// </summary>
    [HideInInspector]
    public AudioClip PiecePlacedSound;
    /// <summary>
    /// AudioClip to play when piece is placed in wrong place
    /// </summary>
    [HideInInspector]
    public AudioClip WrongPlacementSound;
    /// <summary>
    /// AudioClip to play when user picks up piece
    /// </summary>
    [HideInInspector]
    public AudioClip PiecePickupSound;
    /// <summary>
    /// AudioClip to play when all pieces are joined or puzzle is completed
    /// </summary>
    [HideInInspector]
    public AudioClip PuzzleCompletionSound;
    /// <summary>
    /// Continuous running background AudioClip
    /// </summary>
    [HideInInspector]
    public AudioClip BackgroundMusic;

    /// <summary>
    /// Puzzle Event In Editor: Called when puzzle is completed / all pieces are joined
    /// </summary>
    [HideInInspector]
    public UnityEvent OnPuzzleComplete;
    /// <summary>
    /// Puzzle Event In Editor: Called when piece is placed in correct position
    /// </summary>
    [HideInInspector]
    public UnityEvent OnPiecePlaced;


    private int _NoOfPiecesPlaced = 0;      //Keeps track of number of pieces placed inside puzzle successfully

    private GameObject[] _ChildColliders;   //Holds collider instances used to detect pieces position
    private Vector3[] _PuzzlePiecesOrigPos; //Holds starting position of puzzle pieces instances

    //List for controlling side scrolling gameobjects
    private GameObjectsVerticleSlider _gameObjectsVerticleSliderInst = null;

    //Holds instance for audio manager
    private AudioManager _audioManager;



    /// <summary>
    /// UnityMethod: Initialize events and get required components
    /// </summary>
    void Awake()
    { 
        _audioManager = GetComponent<AudioManager>();

        OnPiecePickup += PPPuzzleController_OnPiecePickup;
        OnPieceDrop += PPPuzzleController_OnPieceDrop;

        _gameObjectsVerticleSliderInst = GetComponent<GameObjectsVerticleSlider>();
    }

    /// <summary>
    /// UnityMethod: Initialize actual image mesh and handle camera resizing and placement
    /// </summary>
    /// <returns>Coroutine</returns>
    public override IEnumerator Start()
    {
        if (BackgroundMusic != null)
            _audioManager.PlayMusic(BackgroundMusic);

        //Store current transform and translate to origin for everythings placement
        Vector3 OrigionalPosition = transform.position;
        Vector3 OrigionalRotation = transform.localRotation.eulerAngles;

        yield return StartCoroutine(base.Start());
        
        //Initialize and set Colliders to detect piece in place

        //Get child colider object
        GameObject _ChildCollider = gameObject.transform.FindChild("Collider").gameObject;

        //Instantiate required number of childcollider objects
        _ChildColliders = new GameObject[PiecesInCol * PiecesInRow];

        //Holds initial positions for puzzle pieces
        _PuzzlePiecesOrigPos = new Vector3[_puzzlePieces.Length];


        float PieceWidthInBackground = 1;
        float PieceHeightInBackground = ((float)PuzzleImage.height / (float)PuzzleImage.width);


        for (int CCArray = 0; CCArray < PiecesInCol * PiecesInRow; CCArray++)
        {
            _ChildColliders[CCArray] = Object.Instantiate(_ChildCollider) as GameObject;

            //Assign this class instance
            _ChildColliders[CCArray].GetComponent<PPPiecePositionDetection>().PPPuzzleControlerInstance = this;

            BoxCollider CollisionDetector = _ChildColliders[CCArray].GetComponent<BoxCollider>();

            //Set colliders size
            CollisionDetector.size = new Vector3(0.2f, ((float)PuzzleImage.height / (float)PuzzleImage.width) * 0.2f, 1f);
            
            _ChildColliders[CCArray].name = "BackgroundCollider" + CCArray.ToString();


            float CalculatedX = transform.position.x - (transform.localScale.x / 2) + 
                                     PieceWidthInBackground * Mathf.Repeat(CCArray, PiecesInRow) + (PieceWidthInBackground/2);
            float CalculatedY = transform.position.y - (transform.localScale.y/2) + 
                                     (PieceHeightInBackground * (CCArray/PiecesInRow)) + (PieceHeightInBackground/2);


            _ChildColliders[CCArray].transform.position = new Vector3( CalculatedX, CalculatedY, transform.position.z - 0.1f );
            _ChildColliders[CCArray].transform.parent = transform;
            
        }

        Destroy(_ChildCollider);

        

        //Pieces Initialization And Placement
        float PieceZ = transform.position.z;
        float PieceX = transform.position.x - (transform.localScale.x/2);
        float PieceY = transform.position.y - (transform.localScale.y / 2) + PieceHeightInBackground;

		_gameObjectsVerticleSliderInst.CurrentPosition = new Vector2(PieceX, PieceY);

        for (int RowTrav = 0; RowTrav < PiecesInCol; RowTrav++)
        {
            for (int ColTrav = 0; ColTrav < PiecesInRow; ColTrav++, PieceZ -= 0.005f)
            {
                int CalculatedIndex = (RowTrav * PiecesInRow) + ColTrav;
                GameObject Temp = _puzzlePieces[CalculatedIndex];
  
                //Piece display mode setting
                if (PiecesDisplayMode == EPieceDisplayMode.ShowOnePieceAtATime)
                {
                    //Place pieces
                    Temp.transform.position = new Vector3(PieceX, PieceY, PieceZ);

                    if (CalculatedIndex > 0)
                        Temp.SetActive(false);
                }
                else if (PiecesDisplayMode == EPieceDisplayMode.LeftSliderWithPieces)
                {
                    _gameObjectsVerticleSliderInst.Add(Temp);
                }

            }
        }


        ShufflePiecesInPlace();

        //Make all puzzle pieces child of gameobjects
        for (int i = 0; i < _puzzlePieces.Length; i++)
            //Hold positions for pieces
            _PuzzlePiecesOrigPos[i] = _puzzlePieces[i].transform.position;
        

        //Translate everything back to its origional position
        transform.position = new Vector3( OrigionalPosition.x, OrigionalPosition.y, transform.position.z);
        transform.rotation = Quaternion.Euler( OrigionalRotation );

        //If have to display single mesh on puzzle completion set that mesh
        if (ActualImageOnPuzzleComplete)
        {
            GameObject ActualImagePrefab = Resources.Load("TestBackgroundImage", typeof(GameObject)) as GameObject;

            _actualImageGameObject = Instantiate(ActualImagePrefab);

            _actualImageGameObject.transform.position = new Vector3( transform.position.x,
				transform.position.y, transform.position.z);
            _actualImageGameObject.transform.localScale = transform.localScale;

            _actualImageGameObject.name = "ActualPuzzleImage";

            _actualImageGameObject.GetComponent<Renderer>().material.mainTexture = _puzzlePieceMaker.PuzzleOrigionalImage;

            _actualImageGameObject.AddComponent<BoxCollider>();

            _actualImageGameObject.SetActive(false);
        }



        //Resize camera to display whole puzzle in camera view
        float widthToBeSeen = gameObject.GetComponent<Renderer>().bounds.size.x + PieceWidthInBackground * 1.35f;
        float heightToBeSeen = gameObject.GetComponent<Renderer>().bounds.size.y;

        parentCam.orthographicSize = widthToBeSeen > heightToBeSeen ? widthToBeSeen * 0.4f : heightToBeSeen * 0.4f;


        //parentCamTransform.position = new Vector3(CalcCameraX, CalcCameraY, _pieceInstances[1][0].transform.position.z - 3);
        parentCamTransform.position = new Vector3(0.28f, 0, transform.position.z - 10);

	}

    Vector3 _pickupPosiiton;
    /// <summary>
    /// Piece pickup event handler
    /// </summary>
    /// <param name="Piece">Dropped piece instance</param>
    void PPPuzzleController_OnPiecePickup(GameObject Piece)
    {
        _audioManager.PlaySFXSound(PiecePickupSound);
        _pickupPosiiton = Piece.transform.position;
    }

    /// <summary>
    /// Piece drop event handler
    /// </summary>
    /// <param name="Piece">Dropped piece instance</param>
    void PPPuzzleController_OnPieceDrop(GameObject Piece)
    {
        Piece.transform.position = _pickupPosiiton;
    }

    /// <summary>
    /// This method is called throw SendMessage from piece when it collides with appropriate collider for its placement.
    /// </summary>
    /// <param name="ObjNum">Number of collider this piece found its place with</param>
    private void PiecePlaceFound(int ObjNum)
    {
        //Enable next piece if display mode one piece at a time
        if (PiecesDisplayMode == EPieceDisplayMode.ShowOnePieceAtATime)
        {

            foreach (GameObject item in _puzzlePieces)
            {
                if (!item.activeSelf)
                {

                    //Randomly activate a piece
                    int RndIndex = 0;

                    while (_puzzlePieces[RndIndex].activeSelf)
                    {
                        RndIndex = Random.Range(0, _puzzlePieces.Length);
                    }

                    _puzzlePieces[RndIndex].SetActive(true);

                    break;
                }
            }

        }
        else if (PiecesDisplayMode == EPieceDisplayMode.LeftSliderWithPieces)
        {
            _gameObjectsVerticleSliderInst.Remove(_CurrentHoldingPiece);
        }



        if (_CurrentHoldingPiece != null)
        {
            
            int PieceRow = 0;
            int PieceCol = 0;


            if (ArrayPosToRC(ObjNum, PiecesInCol, PiecesInRow, out PieceRow, out PieceCol))
            {
                
                //Get this piece joints information for placement position calculation
                SPieceInfo TempPieceInfo = _puzzlePieceMaker._CreatedPiecesData[PieceRow, PieceCol];
                

                // _CurrentHoldingPiece.transform.position = _ChildColliders[ObjNum].transform.position;
                _CurrentHoldingPiece.transform.position = _ChildColliders[ObjNum].transform.position;

                //Disable this piece
                _CurrentHoldingPiece.GetComponent<CharacterController>().enabled = false;

                //Call event function
                E_OnPiecePlaced(_CurrentHoldingPiece, ObjNum);
                OnPiecePlaced.Invoke();

                _CurrentHoldingPiece = null;

                //Update trackers
                _NoOfPiecesPlaced++;


                if (_NoOfPiecesPlaced >= _puzzlePieces.Length)
                {
                    _audioManager.PlaySFXSound(PuzzleCompletionSound);
                    OnPuzzleComplete.Invoke();

                    if (ActualImageOnPuzzleComplete)
                    {
                        HideAllPieces();
                        _actualImageGameObject.SetActive(true);

                    }
                }
                else
                {
                    _audioManager.PlaySFXSound(PiecePlacedSound);
                }

            }


        }

        

    }

    /// <summary>
    /// Shuffle pieces in their position for Show one piece at a time mode
    /// </summary>
    private void ShufflePiecesInPlace()
    {

        //Piece Shuffling
        if (ShufflePieces)
        {
            if (PiecesDisplayMode == EPieceDisplayMode.ShowOnePieceAtATime)
            {
                //Shuffle pieces position using Fisher-Yates shuffle algorithm                
                Random.seed = System.DateTime.Now.Second;

                for (int i = _puzzlePieces.Length - 1; i > 0; i--)
                {
                    int RndPos = Random.Range(0, i - 1);

                    Vector3 Swapvar = _puzzlePieces[i].transform.position;
                    _puzzlePieces[i].transform.position = _puzzlePieces[RndPos].transform.position;
                    _puzzlePieces[RndPos].transform.position = Swapvar;

                }

            }
            else if (PiecesDisplayMode == EPieceDisplayMode.LeftSliderWithPieces)
            {
                _gameObjectsVerticleSliderInst.ShuffleGameObjects();
            }
        }

    }



    /// <summary>
    /// Plays wrong placement sound called by PPPiecePositionDetection
    /// </summary>
    public void PlayWrongPlacementSound()
    {
        _audioManager.PlaySFXSound(WrongPlacementSound);
    }

    /// <summary>
    /// Triggered when piece has been placed successfully in its place inside puzzle
    /// </summary>
    /// <param name="PieceInstance">Gameobject instance of piece placed in puzzle</param>
    /// <param name="PieceNum">Number of this piece in sequential array</param>
    public void E_OnPiecePlaced(GameObject PieceInstance, int PieceNum)
    {
        //Debug.Log("Piece number " + PieceNum + " placed");
    }

    /// <summary>
    /// Method to be called using UUI, Toggles display of actual image on puzzle completion
    /// </summary>
    /// <param name="value">Toggle Value</param>
    public void ToggleActualImageOnPuzzleComplete(bool value)
    {
        ActualImageOnPuzzleComplete = value;
    }

    /// <summary>
    /// Resets puzzle to start
    /// </summary>
    public void Reset()
    {
        _NoOfPiecesPlaced = 0;

        ShowAllPieces();

        if (_actualImageGameObject != null)
            _actualImageGameObject.SetActive(false);

        if (PiecesDisplayMode == EPieceDisplayMode.LeftSliderWithPieces)
            _gameObjectsVerticleSliderInst.Clear();

        //Reset all puzzle pieces
        for (int PuzzlePiecesTraverse = 0; PuzzlePiecesTraverse < _puzzlePieces.Length; PuzzlePiecesTraverse++)
        {
            _puzzlePieces[PuzzlePiecesTraverse].transform.localPosition = _PuzzlePiecesOrigPos[PuzzlePiecesTraverse];
            _puzzlePieces[PuzzlePiecesTraverse].GetComponent<CharacterController>().enabled = true;

            if (PiecesDisplayMode == EPieceDisplayMode.ShowOnePieceAtATime)
            {
                if (PuzzlePiecesTraverse > 0)
                    _puzzlePieces[PuzzlePiecesTraverse].SetActive(false);
            }
            else if (PiecesDisplayMode == EPieceDisplayMode.LeftSliderWithPieces)
            {
                _gameObjectsVerticleSliderInst.Add(_puzzlePieces[PuzzlePiecesTraverse]);
            }

        }

        ShufflePiecesInPlace();

        
    }

    /// <summary>
    /// To be used by UUI when Piece display mode is EPieceDisplayMode.LeftSliderWithPieces.
    /// Moves pieces in slider up
    /// </summary>
    public void MoveSliderUp()
    {
        if ( PiecesDisplayMode == EPieceDisplayMode.LeftSliderWithPieces)
        {
            _gameObjectsVerticleSliderInst.SlideUp();
        }
    }

    /// <summary>
    /// To be used by UUI when Piece display mode is EPieceDisplayMode.LeftSliderWithPieces.
    /// Moves pieces in slider down
    /// </summary>
    public void MoveSliderDown()
    {
        if (PiecesDisplayMode == EPieceDisplayMode.LeftSliderWithPieces)
        {
            _gameObjectsVerticleSliderInst.SlideDown();
        }
    }

}


/// <summary>
/// Puzzle pieces display modes for Pick and place puzzle
/// </summary>
public enum EPieceDisplayMode
{
    ShowOnePieceAtATime,
    LeftSliderWithPieces
}