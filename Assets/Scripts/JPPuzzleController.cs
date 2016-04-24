using UnityEngine;
using UnityEngine.Events;
using System.IO;
using System.Collections;
using PuzzleMaker;

/// <summary>
/// Main puzzle controller class for managing Join pieces puzzle
/// </summary>
public class JPPuzzleController : UserControlledJigsawPuzzle
{
    /// <summary>
    /// Determines threshold for piece joining
    /// </summary>
    [Range(0.001f, 0.2f)]
    public float PieceJoinSensitivity = 0.8f;

    /// <summary>
    /// If true hides all pieces and display actualy image in single mesh when all pieces are joined
    /// </summary>
    public bool ActualImageOnPuzzleComplete = true;


    /// <summary>
    /// AudioClip to play when piece is joined with other piece
    /// </summary>
    [HideInInspector]
    public AudioClip PieceJoinedSound;
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
    /// Puzzle Event In Editor: Called when two pieces are joined together
    /// </summary>
    [HideInInspector]
    public UnityEvent OnPieceJoined;



    //Holds instance for audio manager
    private AudioManager _audioManager;

    /// <summary>
    /// UnityMethod: Initialize events and get required components
    /// </summary>
    void Awake()
    {
        _audioManager = GetComponent<AudioManager>();
              
        OnPiecePickup += Event_OnPiecePickup;
    }

    /// <summary>
    /// UnityMethod: Initialize actual image mesh and handle camera resizing and placement
    /// </summary>
    /// <returns>Coroutine</returns>
	public override IEnumerator Start () {
        if (BackgroundMusic != null)
            _audioManager.PlayMusic(BackgroundMusic);

        yield return StartCoroutine(base.Start());

        
#region "Pieces Initialization And Placement"
            

        Random.seed = System.DateTime.Now.Millisecond;

        //Place pieces randomly on screen and scale them accordingly
        for (int RowTrav = 0; RowTrav < PiecesInCol; RowTrav++)
        {
            for (int ColTrav = 0; ColTrav < PiecesInRow; ColTrav++)
            {

                GameObject Temp = _puzzlePieces[(RowTrav * PiecesInRow) + ColTrav];

                JPPieceController TempPieceControllerInst = Temp.GetComponent<JPPieceController>();
                TempPieceControllerInst.JpPuzzleControllerInstance = this;
                 

                //Get this piece information
                SPieceInfo ThisPieceData = _puzzlePieceMaker._CreatedPiecesData[RowTrav, ColTrav].MakeCopy();
                TempPieceControllerInst.ThisPieceData = ThisPieceData;

                
                //Position pieces randomly on screen
                Vector3 CalcPosition = Camera.main.ScreenToWorldPoint(
                    new Vector3(Random.Range(_puzzlePieceMaker.PieceWidthWithoutJoint, Screen.width - _puzzlePieceMaker.PieceWidthWithoutJoint),
                                Random.Range(_puzzlePieceMaker.PieceHeightWithoutJoint, Screen.height - _puzzlePieceMaker.PieceHeightWithoutJoint),
                                0) );
                CalcPosition.z = transform.position.z - 0.4f;

                Temp.transform.position = CalcPosition;


                //Enable collider for this piece
                Temp.GetComponent<BoxCollider>().enabled = true;
                TempPieceControllerInst.enabled = true;

                //Enable piece
                Temp.SetActive(true);
            }
        }

#endregion

        

        //If have to display single mesh on puzzle completion set that mesh
        if (ActualImageOnPuzzleComplete)
        {
            GameObject ActualImagePrefab = Resources.Load("TestBackgroundImage", typeof(GameObject)) as GameObject;

            _actualImageGameObject = Instantiate(ActualImagePrefab);
            //_actualImageGameObject = GameObject.CreatePrimitive(PrimitiveType.Quad);
            _actualImageGameObject.transform.position = new Vector3( transform.position.x, transform.position.y,
                transform.position.z -1);
            _actualImageGameObject.transform.localScale = transform.localScale;

            _actualImageGameObject.AddComponent<ActualImageObjController>();

            _actualImageGameObject.name = "ActualPuzzleImage";

            _actualImageGameObject.GetComponent<Renderer>().material.mainTexture = _puzzlePieceMaker.PuzzleOrigionalImage;
            
            _actualImageGameObject.AddComponent<BoxCollider>();

            _actualImageGameObject.SetActive(false);
        }

#region "Camera Resizing And Placement"

        //Resize camera to display whole puzzle in camera view
        float widthToBeSeen = PiecesInRow * PieceWidthInWorld;
        float heightToBeSeen = PiecesInCol * PieceHeightInWorld;

        parentCam.orthographicSize = widthToBeSeen > heightToBeSeen ? widthToBeSeen * 0.4f : heightToBeSeen * 0.4f;

        //Position camera in centre of puzzle
        //float CalcCameraX = ;
        //float CalcCameraY = ;

        //parentCamTransform.position = new Vector3(CalcCameraX, CalcCameraY, _pieceInstances[1][0].transform.position.z - 3);
        parentCamTransform.position = new Vector3(0,0,transform.position.z - 10 ) ;

#endregion

        
	}


    /// <summary>
    /// Piece pickup event handler
    /// </summary>
    /// <param name="PickedupPiece">Instance of piece picked</param>
    public void Event_OnPiecePickup(GameObject PickedupPiece)
    {
        _audioManager.PlaySFXSound(PiecePickupSound);

        Transform Temp = _CurrentHoldingPiece.transform.root;

        if (Temp != null)
        {
            _CurrentHoldingPiece.transform.parent = null;

            if (Temp.name != _CurrentHoldingPiece.name)
                Temp.parent = _CurrentHoldingPiece.transform;
        }
    }

    /// <summary>
    /// Method called by JPPieceController activate actual image object
    /// if set by user.
    /// </summary>
    /// <param name="Position">Position</param>
    public void PuzzleCompleteCentrePosition(Vector3 Position)
    {
        if (ActualImageOnPuzzleComplete)
        {
            HideAllPieces();
            _actualImageGameObject.SetActive(true);

        }
        
    }

    /// <summary>
    /// Returns ID of current piece user is holding
    /// </summary>
    /// <returns>Returns ID of current piece user is holding</returns>
    public int HoldingPieceID()
    {
        if (_CurrentHoldingPiece != null)
        {
            return _CurrentHoldingPiece.GetComponent<JPPieceController>().ThisPieceData.ID;
        }

        return -1;
        
    }

    /// <summary>
    /// Called when piece collides with wrong piece
    /// </summary>
    /// <param name="Obj">Colliding piece</param>
    public void WrongPieceCollision(GameObject Obj)
    {
        //GetComponent<ColorAnimator>().StartEffect(Obj, WrongPieceAnimationColor );
    }

    /// <summary>
    /// Resets puzzle for resolve
    /// </summary>
    public void Reset()
    {
        ShowAllPieces();

        if (_actualImageGameObject != null)
            _actualImageGameObject.SetActive(false);

        for (int i = 0; i < _puzzlePieces.Length; i++)
        {

            _puzzlePieces[i].GetComponent<BoxCollider>().enabled = false;

            _puzzlePieces[i].transform.parent = null;

            //Position pieces randomly on screen
            Vector3 CalcPosition = Camera.main.ScreenToWorldPoint(
                new Vector3(Random.Range(_puzzlePieceMaker.PieceWidthWithoutJoint, Screen.width - _puzzlePieceMaker.PieceWidthWithoutJoint),
                            Random.Range(_puzzlePieceMaker.PieceHeightWithoutJoint, Screen.height - _puzzlePieceMaker.PieceHeightWithoutJoint),
                            0));

            CalcPosition.z = transform.position.z - 0.01f;

            _puzzlePieces[i].transform.position = CalcPosition;

            _puzzlePieces[i].GetComponent<BoxCollider>().enabled = true;
        }

    }

    /// <summary>
    /// Method to be called using UUI, Toggles display of actual image on puzzle completion
    /// </summary>
    /// <param name="value">Toggle value</param>
    public void ToggleActualImageOnPuzzleComplete(bool value)
    {
        ActualImageOnPuzzleComplete = value;
    }

    /// <summary>
    /// Event called by JpPieceController on piece joining
    /// </summary>
    public void EOnPieceJoined()
    {
        _audioManager.PlaySFXSound(PieceJoinedSound);
        OnPieceJoined.Invoke();
    }

    /// <summary>
    /// Event called by JpPieceController on puzzle completion
    /// </summary>
    public void EOnPuzzleComplete()
    {
        _audioManager.PlaySFXSound(PuzzleCompletionSound);
        OnPuzzleComplete.Invoke();
    }

}
