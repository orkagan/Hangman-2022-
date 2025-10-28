using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class MenuHandler : MonoBehaviour
{
    //Sun rising intro
    public GameObject sun;
    public float sunRotSpeed = 0.3f;

    #region Dictionary in inspector workaround
    //workaround to get a sort of dictionary editable in inspector by having a list of structs
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
    private Coroutine _pageMoveCR;
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
        Application.targetFrameRate = 60;
        //QualitySettings.vSyncCount = 1;

        //Sun rising intro
        Quaternion sunEndRot = sun.transform.rotation;
        sun.transform.rotation = Quaternion.Euler(-90,0,0); //jank-aly setting the rotation so the sky starts dark
        StartCoroutine(RotateFromTo(sun.transform, sunEndRot, sunRotSpeed)); //coroutine lerps back to original rot

        //workaround populating the dictionary
        _screens = new Dictionary<string, GameObject>();
        foreach (namedObj screen in screens)
        {
            _screens.Add(screen.name, screen.obj);
        }

        gameManager = GameManager.Instance;

        //setting variables for page position animations
        pageUpPos = _screens["Page"].transform.position;
        pageDownPos = _screens["Page"].transform.position;
        pageDownPos.y -= 3f;
        _screens["Page"].transform.position = pageDownPos;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Pause();
        }
    }

    public void Play()
    {
        //start game
        GameManager.Instance.StartGame();
        //ChangeScreen();
    }

    public void Pause()
    {
        if (gameManager.gameState == GameState.Game)
        {
            //pause
            gameManager.gameState = GameState.Pause;
            ChangeScreen();
        }
        else if (gameManager.gameState == GameState.Pause)
        {
            //unpause
            gameManager.gameState = GameState.Game;
            ChangeScreen();
        }
    }

    public void ReturnToMainMenu()
    {
        gameManager.gameState = GameState.MainMenu;
        ChangeScreen();
    }

    public void ChangeScreen()
    {
        string gameStateString = gameManager.gameState.ToString();
        Debug.Log($"ChangeScreen State: {gameStateString}");
        foreach (KeyValuePair<string,GameObject> screen in _screens)
        {
            //game page doesn't disable, just moves in or out of view
            if (screen.Key == "Page")
            {
                //have the page down if MainMenu state, else page is up
                if (gameStateString == "Game" || gameStateString == "Pause")
                {
                    if (_pageMoveCR != null) StopCoroutine(_pageMoveCR);
                    _pageMoveCR = StartCoroutine(PageInView(pageUpPos));
                }
                else
                {
                    if (_pageMoveCR != null) StopCoroutine(_pageMoveCR);
                    _pageMoveCR = StartCoroutine(PageInView(pageDownPos));
                }
            }
            //if current game state is screen then enable, else disable
            else if (gameStateString == screen.Key)
            {
                screen.Value.SetActive(true);
            }
            else
            {
                screen.Value.SetActive(false);
            }
        }
    }

    public void ExitGame()
    {
        Application.Quit();
#if UNITY_EDITOR
        Debug.Log("ExitGame attempted.");
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    IEnumerator RotateFromTo(Transform transform, Quaternion target, float rotSpeed)
    {
        Quaternion startRot = transform.rotation;
        float t = 0;
        while (Quaternion.Angle(transform.rotation,target)>0.001)
        {
            transform.rotation = Quaternion.Lerp(startRot, target, t * rotSpeed);
            t += Time.deltaTime;
            yield return null;
        }
    }

    IEnumerator PageInView(Vector3 pagePos)
    {
        while (_screens["Page"].transform.position != pagePos)
        {
            _screens["Page"].transform.position = Vector3.SmoothDamp(_screens["Page"].transform.position, pagePos, ref pageVelocity, pageSmoothTime);
            yield return null;
        }
    }
}
