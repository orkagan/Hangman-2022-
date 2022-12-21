using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    //Sun rising intro
    public GameObject sun;
    public float rotTime = 5f;
    private Vector3 _sunVelocity = Vector3.zero;

    #region Dictionary in inspector workaround
    //workaround to get a sort of dictionary editable in inspector
    [Serializable]
    public struct namedObj
    {
        public string name;
        public GameObject obj;
    }
    public List<namedObj> screens;
    //going to populate dictionary using the screens list
    #endregion
    private Dictionary<string,GameObject> _screens;

    //animating the game page
    public Vector3 pageUpPos;
    public Vector3 pageDownPos;
    public float pageSmoothTime = 0.1f;
    private Vector3 pageVelocity = Vector3.zero;

    public GameManager gameManager;

    #region Singleton Setup
    //Staticly typed property setup for MenuHandler.Instance
    private static MenuHandler _instance;
    public static MenuHandler Instance
    {
        get => _instance;
        private set
        {
            //check if instance of this class already exists and if so keep orignal existing instance
            if (_instance == null)
            {
                _instance = value;
            }
            else if (_instance != value)
            {
                Debug.Log($"{nameof(MenuHandler)} instance already exists, destroy the duplicate!");
                Destroy(value);
            }
        }
    }
    private void Awake()
    {
        Instance = this; //sets the static class instance
    }
    #endregion

    // Start is called before the first frame update
    void Start()
    {
        //Sun rising intro
        Quaternion sunEndRot = sun.transform.rotation;
        sun.transform.rotation = Quaternion.Euler(-90,0,0); //jank-aly setting the rotation so the sky starts dark
        StartCoroutine(RotateFromTo(sun.transform, sunEndRot, rotTime)); //coroutine lerps back to original rot

        //workaround populating the dictionary
        _screens = new Dictionary<string, GameObject>();
        foreach (namedObj screen in screens)
        {
            _screens.Add(screen.name, screen.obj);
        }

        gameManager = GameManager.Instance;

        pageUpPos = _screens["Game"].transform.position;
        pageDownPos = _screens["Game"].transform.position;
        pageDownPos.y -= 3f;
        _screens["Game"].transform.position = pageDownPos;
    }

    // Update is called once per frame
    void Update()
    {
        //page animations
        if (gameManager.gameState == GameState.Game)
        {
            if (_screens["Game"].transform.position != pageUpPos)
            {
                _screens["Game"].transform.position = Vector3.SmoothDamp(_screens["Game"].transform.position, pageUpPos, ref pageVelocity, pageSmoothTime);
            }
        }
        else if (_screens["Game"].transform.position != pageDownPos)
        {
            _screens["Game"].transform.position = Vector3.SmoothDamp(_screens["Game"].transform.position, pageDownPos, ref pageVelocity, pageSmoothTime);
        }
    }

    public void Play()
    {
        //start game
        GameManager.Instance.StartGame();
    }
    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        Debug.Log("ExitGame attempted.");
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    IEnumerator RotateFromTo(Transform transform, Quaternion target, float rotTime)
    {
        while (Quaternion.Angle(transform.rotation,target)>0.01)
        {
            //accidentally made a jank smooth damp effect by having the transform in the first parameter
            transform.rotation = Quaternion.Lerp(sun.transform.rotation, target, Time.deltaTime);
            yield return null;
        }
        transform.rotation = target;
    }
}
