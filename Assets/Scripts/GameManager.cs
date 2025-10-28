using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    public GameState gameState;

    private List<string> _words;
    [SerializeField]
    private string _word;
    public TextMeshProUGUI displayText;
    public string guessedLetters;
    public int wrongGuesses;

    public GameObject keyboard;
    private Dictionary<string, Button> _letterButtons;

    public List<GameObject> limbs;
    public GameObject hangedMan;
    public GameObject aliveMan;
    public TextMeshProUGUI winText;
    public TextMeshProUGUI loseText;

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
        //read in dictionary of words to choose from
        //string wordListPath = $"{Application.streamingAssetsPath}/wordlist.txt";
        /*string wordListPath = Path.Combine(Application.streamingAssetsPath, "wordlist.txt");
        _words = ReadFileLines(wordListPath);*/
        TextAsset loadedWords = Resources.Load<TextAsset>("wordlist");
        _words = loadedWords.text.Split().ToList();

        //populate dictionary of letter buttons and add functionality
        _letterButtons = new Dictionary<string, Button>();
        foreach (Button key in keyboard.GetComponentsInChildren<Button>())
        {
            Button button = key.GetComponent<Button>();
            //adds onClick event to make guess based on the name of the gameObject
            button.onClick.AddListener(delegate { MakeGuess(key.name); });
            _letterButtons.Add(key.name.ToLower(), button);
        }

        hangedMan.SetActive(false);
        aliveMan.SetActive(false);
        
        gameState = GameState.MainMenu;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnGUI()
    {
        #region Keyboard Input Handler
        Event e = Event.current;

        //Check of in the game state
        //Check the type of the current event, making sure to take in only the KeyDown of the keystroke.
        //char.IsLetter to filter out all other KeyCodes besides alphabetical.
        if (gameState == GameState.Game &&
            e.type == EventType.KeyDown &&
            e.keyCode.ToString().Length == 1 &&
            char.IsLetter(e.keyCode.ToString()[0]))
        {
            //This is your desired action
            //Debug.Log("Detected key code: " + e.keyCode);
            MakeGuess(e.keyCode.ToString().ToLower());
        }
        #endregion
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
        Debug.Log($"Random word chosen{_word}");
    }

    public void StartGame()
    {
        gameState = GameState.Game;
        MenuHandler.Instance.ChangeScreen();
        wrongGuesses = 0;
        guessedLetters = "";
        ChooseRandomWord(_words);
        //display blanks of chosen word letters
        displayText.text = "";
        for (int i = 0; i < _word.Length; i++)
        {
            displayText.text += "_";
        }

        //disable sketch of hangman
        foreach (GameObject limb in limbs)
        {
            limb.SetActive(false);
        }
        //disable hanged/alive man models
        hangedMan.SetActive(false);
        aliveMan.SetActive(false);

        //enable keyboard buttons
        foreach (KeyValuePair<string,Button> item in _letterButtons)
        {
            item.Value.interactable = true;
        }
    }

    public void MakeGuess(string letter)
    {
        //check if letter has already been guessed
        if (guessedLetters.Contains(letter) || gameState != GameState.Game)
        {
            return;
        }
        else
        {
            //add letter to guessed letters
            guessedLetters += letter;
            //disable button
            Debug.Log($"Disable: {letter}");
            _letterButtons[letter].interactable = false;
        }

        if (_word.Contains(letter))
        {
            //correct guess
            //reveal letters
            string newText = "";
            for (int i = 0; i < _word.Length; i++)
            {
                //adds revealed letter
                if (_word[i].ToString()==letter) newText += letter;
                //or adds existing char
                else newText += displayText.text[i];
            }
            displayText.text = newText;

            //check if game over
            if (!displayText.text.Contains("_"))
            {
                Win();
                return;
            }
        }
        else
        {
            //wrong guess
            wrongGuesses++;
            //check if game over
            if (wrongGuesses>limbs.Count)
            {
                Lose();
                return;
            }
            //show limb
            if(limbs!=null) limbs[wrongGuesses-1].SetActive(true);
        }
    }

    public void Win()
    {
        //a winner is you
        gameState = GameState.Win;
        MenuHandler.Instance.ChangeScreen();

        //show word
        winText.text = _word;

        //show a man walking free
        aliveMan.SetActive(true);
    }

    public void Lose()
    {
        //skill issue
        gameState = GameState.Lose;
        MenuHandler.Instance.ChangeScreen();

        //show word
        loseText.text = _word;

        //fatality
        hangedMan.SetActive(true);
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
