using UnityEngine;
using System.Collections;
using System.Collections.Generic;


/// <summary>
/// Creates a slider based on game objects
/// </summary>
public class GameObjectsVerticleSlider : MonoBehaviour{

    private float _spacing = 1f;
    public float Spacing
    {
        get { return _spacing; }
        set { _spacing = value; UpdateGameObjectsPosition(); }
    }

    private List<GameObject> _objectsInSlider = new List<GameObject>();
    public GameObject this[int index]
    {
        get { return _objectsInSlider[index];}
    }

    public int Count
    {
        get { return _objectsInSlider.Count; }
    }

    private Vector2 _currentPosition = Vector2.zero;
    public Vector2 CurrentPosition
    {
        get { return _currentPosition; }
        set { _currentPosition = value; UpdateGameObjectsPosition(); }
    }



    void Awake()
    {
        this.enabled = false;
    }


    public void RemoveAt(int Index)
    {
        _objectsInSlider.RemoveAt(Index);
        UpdateGameObjectsPosition();
    }

    public void Remove(GameObject gameObject)
    {
        _objectsInSlider.Remove(gameObject);
        UpdateGameObjectsPosition();
    }

    public void Clear()
    {
        _objectsInSlider.Clear();
    }

    public void Add(GameObject gameObject)
    {
        _objectsInSlider.Add(gameObject);
        UpdateGameObjectsPosition();
    }

    /// <summary>
    /// Changes gameobjects positions from each other
    /// </summary>
    public void ShuffleGameObjects()
    {
        if (_objectsInSlider.Count > 1)
        {   
            //Shuffle pieces position using Fisher-Yates shuffle algorithm
            Random.seed = System.DateTime.Now.Second;
            
            
            for (int i = _objectsInSlider.Count - 1; i > 0; i--)
            {
                int RndPos = Random.Range(0, i - 1);

                Vector3 Swapvar = _objectsInSlider[i].transform.position;
                _objectsInSlider[i].transform.position = _objectsInSlider[RndPos].transform.position;
                _objectsInSlider[RndPos].transform.position = Swapvar;

                //Swap objects position in array
                GameObject SwapVarG = _objectsInSlider[i];
                _objectsInSlider[i] = _objectsInSlider[RndPos];
                _objectsInSlider[RndPos] = SwapVarG;

            }
            
        }
    }


    public void SlideUp()
    {
        if (_objectsInSlider.Count > 0)
        {
            if (_objectsInSlider[_objectsInSlider.Count - 1].transform.position.y < _currentPosition.y)
            {
                for (int pieceTrav = 0; pieceTrav < _objectsInSlider.Count; pieceTrav++)
                {
                    Vector3 Temp = _objectsInSlider[pieceTrav].transform.position;
                    Temp.y += _spacing;
                    _objectsInSlider[pieceTrav].transform.position = Temp;
                }
            }
        }
    }

    public void SlideDown()
    {
        if (_objectsInSlider.Count > 0)
        {
            if (_objectsInSlider[0].transform.position.y > _currentPosition.y + _spacing * 2)
            {
                for (int pieceTrav = 0; pieceTrav < _objectsInSlider.Count; pieceTrav++)
                {
                    Vector3 Temp = _objectsInSlider[pieceTrav].transform.position;
                    Temp.y -= _spacing;
                    _objectsInSlider[pieceTrav].transform.position = Temp;
                }
            }
        }
    }


    private void UpdateGameObjectsPosition()
    {
        if (_objectsInSlider.Count > 0)
        {
            if (_objectsInSlider.Count == 1)
            {
                Vector3 TempPos = _currentPosition;
                TempPos.y += _spacing;
                _objectsInSlider[0].transform.position = TempPos;
            }
            else
            {
                Vector3 TempPos  = _objectsInSlider[0].transform.position;
                for (int objsTrav = 1; objsTrav < _objectsInSlider.Count; objsTrav++)
                {
                    TempPos.y -= _spacing;
                    _objectsInSlider[objsTrav].transform.position = TempPos;
                }
            }
        }
        
    }

}
