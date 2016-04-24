using UnityEngine;
using System.Collections;
using PuzzleMaker;

/// <summary>
/// Jigsaw puzzle piece controller class, for JPPuzzle piece
/// </summary>
public class JPPieceController : MonoBehaviour {

    /// <summary>
    /// Holds JPPuzzleController instance from which this piece is initialized.
    /// Set by JPPuzzleController
    /// </summary>
    [HideInInspector]
    public JPPuzzleController JpPuzzleControllerInstance;
    /// <summary>
    /// Holds this piece metadata at the time of instantiation of this piece
    /// </summary>
    [HideInInspector]
    public SPieceInfo ThisPieceData;

    /// <summary>
    /// Unity Event: Checks for and manages joining of pieces
    /// </summary>
    /// <param name="ColObj">Colliding piece</param>
    void OnTriggerStay (Collider ColObj)
    {

        if (ColObj.name.Contains("Piece"))
            if (transform.parent == null && transform.childCount == 0)
                OnTriggerEnter(ColObj);

    }

    /// <summary>
    /// Unity Event: Checks for and manages joining of pieces
    /// </summary>
    /// <param name="ColObj">Colliding piece</param>
    void OnTriggerEnter (Collider ColObj) {

        if (ColObj.name.Contains("Piece"))
        {
            if (JpPuzzleControllerInstance.IsHoldingPiece)
            {
                // Is it a side collision  or top or bottom collision using collision range

                Vector3 CollisionVector = transform.position - ColObj.transform.position;
                CollisionVector.Normalize();

                float CollisionThreshold = JpPuzzleControllerInstance.PieceJoinSensitivity;
                float DirectionCollisionThreshold = 0.3f;

                GameObject ColGameObject = ColObj.gameObject;
                JPPieceController ColObjController = ColGameObject.GetComponent<JPPieceController>();

                //Check for correct or wrong piece collision for display of wrong piece animation
                if ( ThisPieceData.ID == JpPuzzleControllerInstance.HoldingPieceID() )
                    JpPuzzleControllerInstance.WrongPieceCollision(ColGameObject);

                if (CollisionVector.y < CollisionThreshold && CollisionVector.y > -CollisionThreshold)
                {
                    
                    if (CollisionVector.x < DirectionCollisionThreshold) //Right Collision
                    {
                        //Debug.Log("Right Collision Piece " + ThisPieceData.ID + " " + CollisionVector.normalized);
                        
                        if (ColObjController.ThisPieceData.HaveJoint(EJointPosition.Left))
                            OnRightCollision(ColObjController.ThisPieceData.ID, ColGameObject, ColObjController.ThisPieceData, ColObjController);
                    
                    }
                    else if (CollisionVector.x > -DirectionCollisionThreshold) //Left Collision
                    {
                        //Debug.Log("Left collision Piece " + ThisPieceData.ID + " " + CollisionVector.normalized);

                        if (ColObjController.ThisPieceData.HaveJoint(EJointPosition.Right))
                            OnLeftCollision(ColObjController.ThisPieceData.ID, ColGameObject, ColObjController.ThisPieceData, ColObjController);
                        
                    }

                }

                else if (CollisionVector.x < CollisionThreshold && CollisionVector.x > -CollisionThreshold)
                {

                    if (CollisionVector.y < DirectionCollisionThreshold) //Top Collision
                    {
                        //Debug.Log("Top Collision Piece " + ThisPieceData.ID + " " + CollisionVector.normalized);

                        if (ColObjController.ThisPieceData.HaveJoint(EJointPosition.Bottom))
                            OnTopCollision(ColObjController.ThisPieceData.ID, ColGameObject, ColObjController.ThisPieceData, ColObjController);
                        
                    }
                    else if (CollisionVector.y > DirectionCollisionThreshold) //Bottom collision
                    {
                        //Debug.Log("Bottom collision Piece " + ThisPieceData.ID + " " + CollisionVector.normalized);

                        if (ColObjController.ThisPieceData.HaveJoint(EJointPosition.Top))
                            OnBottomCollision(ColObjController.ThisPieceData.ID, ColGameObject, ColObjController.ThisPieceData, ColObjController);
                        
                    }

                }


            }
        }


    }


    /// <summary>
    /// Called when piece is colliding from its right side
    /// </summary>
    /// <param name="CollidingPieceId">Id of piece colliding with</param>
    /// <param name="ColP">Instance of piece colliding with</param>
    /// <param name="ColPPieceData">Metadata of piece colliding with</param>
    /// <param name="ColObjController">JPPieceController instance of piece colliding with</param>
    void OnRightCollision(int CollidingPieceId, GameObject ColP, SPieceInfo ColPPieceData, JPPieceController ColObjController)
    {
        
        //Check if this is child of current holding piece then apply collision logic
        bool IsChildOfHoldingPiece = false;

        JPPieceController[] ChildrenControllers = transform.GetComponentsInChildren<JPPieceController>();
        foreach (JPPieceController item in ChildrenControllers)
        {
            if (ThisPieceData.ID == item.ThisPieceData.ID)
            {
                IsChildOfHoldingPiece = true;
                break;
            }
        }


        if (ThisPieceData.ID == JpPuzzleControllerInstance.HoldingPieceID() || IsChildOfHoldingPiece)
        {

            if (ThisPieceData.ID == CollidingPieceId - 1)
            {

                //If is child of holding piece make it parent
                //Make this piece parent of all
                Transform Temp = transform.root;
                transform.parent = null;
                Temp.parent = transform;

                JpPuzzleControllerInstance.UnholdPiece();

                transform.position = new Vector3(ColP.transform.position.x - JpPuzzleControllerInstance.PieceWidthInWorld ,
                    ColP.transform.position.y, ColP.transform.position.z);

                ColP.transform.root.parent = transform;

                OnPieceJoined();

                CheckForPuzzleComplete(transform);
            }
        }
        else
        {
            //Wrong Piece

        }
        
    }

    /// <summary>
    /// Called when piece is colliding from its left side
    /// </summary>
    /// <param name="CollidingPieceId">Id of piece colliding with</param>
    /// <param name="ColP">Instance of piece colliding with</param>
    /// <param name="ColPPieceData">Metadata of piece colliding with</param>
    /// <param name="ColObjController">JPPieceController instance of piece colliding with</param>
    void OnLeftCollision(int CollidingPieceId, GameObject ColP, SPieceInfo ColPPieceData, JPPieceController ColObjController)
    {
        
        //Check if this is child of current holding piece then apply collision logic
        bool IsChildOfHoldingPiece = false;

        JPPieceController[] ChildrenControllers = transform.GetComponentsInChildren<JPPieceController>();
        foreach (JPPieceController item in ChildrenControllers)
        {
            if (ThisPieceData.ID == item.ThisPieceData.ID)
            {
                IsChildOfHoldingPiece = true;
                break;
            }
        }


        if (ThisPieceData.ID == JpPuzzleControllerInstance.HoldingPieceID() || IsChildOfHoldingPiece)
        {

            if (ThisPieceData.ID == CollidingPieceId + 1)
            {
                //If is child of holding piece make it parent
                //Make this piece parent of all
                Transform Temp = transform.root;
                transform.parent = null;
                Temp.parent = transform;


                JpPuzzleControllerInstance.UnholdPiece();

                transform.position = new Vector3(ColP.transform.position.x + JpPuzzleControllerInstance.PieceWidthInWorld,
                    ColP.transform.position.y, ColP.transform.position.z);

                ColP.transform.root.parent = transform;

                OnPieceJoined();

                CheckForPuzzleComplete(transform);
            }
        }
        
    }

    /// <summary>
    /// Called when piece is colliding from its top side
    /// </summary>
    /// <param name="CollidingPieceId">Id of piece colliding with</param>
    /// <param name="ColP">Instance of piece colliding with</param>
    /// <param name="ColPPieceData">Metadata of piece colliding with</param>
    /// <param name="ColObjController">JPPieceController instance of piece colliding with</param>
    void OnTopCollision(int CollidingPieceId, GameObject ColP, SPieceInfo ColPPieceData, JPPieceController ColObjController)
    {
        
        //Check if this is child of current holding piece then apply collision logic
        bool IsChildOfHoldingPiece = false;

        JPPieceController[] ChildrenControllers = transform.GetComponentsInChildren<JPPieceController>();
        foreach (JPPieceController item in ChildrenControllers)
        {
            if (ThisPieceData.ID == item.ThisPieceData.ID)
            {
                IsChildOfHoldingPiece = true;
                break;
            }
        }


        if (ThisPieceData.ID == JpPuzzleControllerInstance.HoldingPieceID() || IsChildOfHoldingPiece)
        {
            

            //Debug.Log(CollidingPieceId + "," + ThisPieceData.ID + " Top Collision");

            //Get colliding piece position in grid
            int CPElementRow = -1;
            int CPElementCol = -1;
            PPPuzzleController.ArrayPosToRC(CollidingPieceId, JpPuzzleControllerInstance.PiecesInCol,
                            JpPuzzleControllerInstance.PiecesInRow, out CPElementRow, out CPElementCol);

            

            //Get this piece position in grid
            int PElementRow = -1;
            int PElementCol = -1;
            PPPuzzleController.ArrayPosToRC(ThisPieceData.ID, JpPuzzleControllerInstance.PiecesInCol,
                            JpPuzzleControllerInstance.PiecesInRow, out PElementRow, out PElementCol);

            if (ThisPieceData.ID < CollidingPieceId && PElementCol == CPElementCol && PElementRow == CPElementRow - 1)
            {

                //If is child of holding piece make it parent
                //Make this piece parent of all
                Transform Temp = transform.root;
                transform.parent = null;
                Temp.parent = transform;

                JpPuzzleControllerInstance.UnholdPiece();

                transform.position = new Vector3(ColP.transform.position.x,
                    ColP.transform.position.y - JpPuzzleControllerInstance.PieceHeightInWorld, ColP.transform.position.z);

                ColP.transform.root.parent = transform;

                OnPieceJoined();

                CheckForPuzzleComplete(transform);
            }
        }
        
        
    }

    /// <summary>
    /// Called when piece is colliding from its bottom side
    /// </summary>
    /// <param name="CollidingPieceId">Id of piece colliding with</param>
    /// <param name="ColP">Instance of piece colliding with</param>
    /// <param name="ColPPieceData">Metadata of piece colliding with</param>
    /// <param name="ColObjController">JPPieceController instance of piece colliding with</param>
    void OnBottomCollision(int CollidingPieceId, GameObject ColP, SPieceInfo ColPPieceData, JPPieceController ColObjController)
    {
        
        //Check if this is child of current holding piece then apply collision logic
        bool IsChildOfHoldingPiece = false;

        JPPieceController[] ChildrenControllers = transform.GetComponentsInChildren<JPPieceController>();
        foreach (JPPieceController item in ChildrenControllers)
        {
            if (ThisPieceData.ID == item.ThisPieceData.ID)
            {
                IsChildOfHoldingPiece = true;
                break;
            }
        }


        if (ThisPieceData.ID == JpPuzzleControllerInstance.HoldingPieceID() || IsChildOfHoldingPiece)
        {

            //Get colliding piece position in grid
            int CPElementRow = 0;
            int CPElementCol = 0;
            PPPuzzleController.ArrayPosToRC(CollidingPieceId, JpPuzzleControllerInstance.PiecesInCol,
                            JpPuzzleControllerInstance.PiecesInRow, out CPElementRow, out CPElementCol);


            //Get this piece position in grid
            int PElementRow = 0;
            int PElementCol = 0;
            PPPuzzleController.ArrayPosToRC(ThisPieceData.ID, JpPuzzleControllerInstance.PiecesInCol,
                            JpPuzzleControllerInstance.PiecesInRow, out PElementRow, out PElementCol);


            if (ThisPieceData.ID > CollidingPieceId && PElementCol == CPElementCol && PElementRow == CPElementRow + 1)
            {
                //If is child of holding piece make it parent
                //Make this piece parent of all
                Transform Temp = transform.root;
                transform.parent = null;
                Temp.parent = transform;


                JpPuzzleControllerInstance.UnholdPiece();

                transform.position = new Vector3(ColP.transform.position.x,
                    ColP.transform.position.y + JpPuzzleControllerInstance.PieceHeightInWorld, ColP.transform.position.z);

                ColP.transform.root.parent = transform;

                OnPieceJoined();

                CheckForPuzzleComplete(transform);
            }

        }

    }

    /// <summary>
    /// Checks whether all pieces are joined
    /// </summary>
    /// <param name="rootTransform">Root transform of joined pieces</param>
    void CheckForPuzzleComplete(Transform rootTransform)
    {
        int ChildCount = 0;
        Transform[] Childs = rootTransform.GetComponentsInChildren<Transform>();
        foreach (Transform Child in Childs)
        {
            if (Child.gameObject.name.Contains("Piece"))
                ChildCount++;
        }


        if ((JpPuzzleControllerInstance.PiecesInCol * JpPuzzleControllerInstance.PiecesInRow) == ChildCount)
        {
            JpPuzzleControllerInstance.PuzzleCompleteCentrePosition(rootTransform.position);

            JpPuzzleControllerInstance.EOnPuzzleComplete();
            
        }

    }


    /// <summary>
    /// Called when piece is successfully joins with another piece.
    /// This calls main event on piece joined in JPPuzzleController
    /// </summary>
    void OnPieceJoined()
    {
        JpPuzzleControllerInstance.EOnPieceJoined();
    }

}
