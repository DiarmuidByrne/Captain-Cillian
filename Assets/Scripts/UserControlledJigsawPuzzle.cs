using UnityEngine;
using System.Collections;

/// <summary>
/// Implements user controlling facility for puzzle created by JigsawPuzzleCreator
/// </summary>
public class UserControlledJigsawPuzzle : JigsawPuzzleCreator {

    //Variables use to move pieces to place
    /// <summary>
    /// Holds instance of user picked up piece
    /// </summary>
    protected GameObject _CurrentHoldingPiece = null;
    /// <summary>
    /// Default position of holding piece
    /// </summary>
    protected Vector3 _CurrentHoldingPieceDPos = new Vector3();

    /// <summary>
    /// Event delegate for UserControlledJigsawPuzzle
    /// </summary>
    /// <param name="Piece">Pieceinstance</param>
    public delegate void DOnPiecePickup(GameObject Piece);
    /// <summary>
    /// Event called when Piece is picked by user
    /// </summary>
    public event DOnPiecePickup OnPiecePickup;
    /// <summary>
    /// Event called when piece is dropped by user
    /// </summary>
    public event DOnPiecePickup OnPieceDrop;


    /// <summary>
    /// Returns true if user is holding piece false otherwise
    /// </summary>
    public bool IsHoldingPiece
    {
        get { return _CurrentHoldingPiece != null; }
    }

    /// <summary>
    /// UnityMethod: Handles user input for puzzle pieces
    /// </summary>
    public virtual void Update()
    {
        if (IsHoldingPiece)
        {

            float MousePositionX = Camera.main.ScreenToWorldPoint(Input.mousePosition).x;
            float MousePositionY = Camera.main.ScreenToWorldPoint(Input.mousePosition).y;

            _CurrentHoldingPiece.transform.position = new Vector3(MousePositionX, MousePositionY,
                                                _CurrentHoldingPiece.transform.position.z);
        }

        if (Input.GetMouseButtonDown(0))
        {

            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;


            if (Physics.Raycast(ray, out hit, 100f))
            {
                if (hit.collider.transform.name.Contains("Piece"))
                {

                    _CurrentHoldingPiece = hit.collider.transform.gameObject;
                    _CurrentHoldingPieceDPos = _CurrentHoldingPiece.transform.position;


                    //Get top parent in hierarchy of current holding piece
                        //Set it z to -0.6f
                            //Get all transforms in children of that parent
                                //Set theie z to 0
                    Transform Root = _CurrentHoldingPiece.transform.root;
                    if (Root != null)
                    {
                        Root.position = new Vector3(Root.position.x, Root.position.y, Root.position.z - 0.4f);
                    }
                    else
                    {
                        _CurrentHoldingPiece.transform.position = new Vector3(_CurrentHoldingPieceDPos.x, _CurrentHoldingPieceDPos.y,
                                                                            _CurrentHoldingPieceDPos.z - 0.4f);
                        Root = _CurrentHoldingPiece.transform;
                    }

                    
                    Transform[] Temp = Root.GetComponentsInChildren<Transform>();
                    foreach (Transform item in Temp)
                        if ( item.gameObject.name != Root.gameObject.name )
                            item.localPosition = new Vector3( item.localPosition.x, item.localPosition.y, 0);


                    if (OnPiecePickup != null)
                        OnPiecePickup.Invoke(hit.collider.transform.gameObject);
                    
                }
            }


        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (IsHoldingPiece)
            {
                if ( OnPieceDrop != null )
                    OnPieceDrop.Invoke(_CurrentHoldingPiece);
                UnholdPiece();
            }
        }

    }

    /// <summary>
    /// Drops user picked up piece
    /// </summary>
    public void UnholdPiece()
    {
        _CurrentHoldingPiece.transform.position = new Vector3(_CurrentHoldingPiece.transform.position.x,
                        _CurrentHoldingPiece.transform.position.y, _CurrentHoldingPiece.transform.position.z + 0.4f);
        
        _CurrentHoldingPiece = null;
    }
    
}
