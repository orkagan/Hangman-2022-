using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuHandler : MonoBehaviour
{
    public GameObject sun;
    public float rotTime = 5f;
    private Vector3 _sunVelocity = Vector3.zero;

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
        Quaternion sunEndRot = sun.transform.rotation;
        sun.transform.rotation = Quaternion.Euler(-90,0,0);
        StartCoroutine(RotateFromTo(sun.transform, sunEndRot, rotTime));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Play()
    {
        //start game
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
            transform.rotation = Quaternion.Lerp(sun.transform.rotation, target, Time.deltaTime);
            yield return null;
        }
    }
}
