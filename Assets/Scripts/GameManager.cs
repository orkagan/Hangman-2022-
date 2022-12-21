using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameState gameState;

    public string wordListPath = "Assets/wordlist.txt";
    private List<string> _words;
    [SerializeField]
    private string _word;
    public TextMeshProUGUI displayText;

    public List<GameObject> limbs;

    #region Singleton Setup
    //Staticly typed property setup for GameManager.Instance
    private static GameManager _instance;
    public static GameManager Instance
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
                Debug.Log($"{nameof(GameManager)} instance already exists, destroy the duplicate!");
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
        gameState = GameState.MainMenu;
        _words = ReadFileLines(wordListPath);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public List<string> ReadFileLines(string filePath)
    {
        List<string> lines = new List<string>();
        StreamReader reader = new StreamReader(filePath);
        while (!reader.EndOfStream)
        {
            string line = reader.ReadLine();
            lines.Add(line);
        }
        reader.Close();
        return lines;
    }

    public void ChooseRandomWord(List<string> words)
    {
        _word = words[Random.Range(0,words.Count)];
    }

    public void StartGame()
    {
        gameState = GameState.Game;
        ChooseRandomWord(_words);
        //display blanks of chosen word letters
        displayText.text = "";
        for (int i = 0; i < _word.Length; i++)
        {
            displayText.text += "_";
        }
    }

    public void MakeGuess(char letter)
    {
        if (_word.Contains(letter))
        {
            //correct guess
            //reveal letters
            string newText = "";
            for (int i = 0; i < _word.Length; i++)
            {
                //adds revealed letter
                if (_word[i]==letter) newText += letter;
                //or adds existing char
                else newText += displayText.text[i];
            }
            displayText.text = newText;

            //check if game over
        }
        else
        {
            //wrong guess
            //add limb
            for (int i = 0; i < limbs.Count; i++)
            {
                if (!limbs[i].activeSelf)
                {
                    limbs[i].SetActive(true);
                    break;
                }
            }
            //check if game over
        }
    }

}
public enum GameState
{
    MainMenu,
    Game,
    Pause,
    Lose,
    Win
}
