using Plugin.Maui.Audio;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace countdown2;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private readonly System.Timers.Timer _gameTimer = new();
    static private char[] _currentLetters = new char[9];
    private int _timeRemaining;
    private bool _gameActive;
    private readonly HashSet<string> _validWords;
    static private int count = 0;
    private IAudioPlayer? _currentPlayer; // Track current audio player

    private readonly IAudioManager audioManager;

    public ObservableCollection<WordScore> SubmittedWords { get; set; } = new();

    public MainPage(IAudioManager audioManager)
    {
        InitializeComponent();
        BindingContext = this;
        SetupTimer();
        _validWords = LoadWordsFromFile();

        this.audioManager = audioManager;
    }

    private HashSet<string> LoadWordsFromFile()
    {
        var words = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        string fileName = "dictionary.txt";

        // Get the full path to the file in the app bundle
        using var stream = FileSystem.OpenAppPackageFileAsync(fileName).Result;
        using var reader = new StreamReader(stream);

        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();
            if (!string.IsNullOrWhiteSpace(line))
                words.Add(line.Trim());
        }

        return words;
    }

    private void SetupTimer()
    {
        _gameTimer.Interval = 1000; // 1 second
        _gameTimer.Elapsed += OnTimerElapsed;
    }

    private void OnTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        _timeRemaining--;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            TimerLabel.Text = $"Time: {_timeRemaining}";

            if (_timeRemaining <= 0)
            {
                EndGame();
            }
        });
    }

    private void OnVowelClicked(object sender, EventArgs e)
    {

        count++;

        if (_gameActive == false && count < 9)
        {
            DisplayLetter(GenerateVowel());
        }
        else if (_gameActive == false && count >= 9)
        {
            StartNewGame();
        }
        
    }

    private void OnConsonantClicked(object sender, EventArgs e)
    {
        count++;

        if (_gameActive == false && count < 9)
        {
            DisplayLetter(GenerateConsonant());
        }
        else if (_gameActive == false && count >= 9)
        {
            StartNewGame();
        }
    }

    private async void StartNewGame()
    {
        // Stop any currently playing audio
        _currentPlayer?.Stop();

        //starting the mp3 file when the game starts
        _currentPlayer = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync
            ("countdown_clock_only.mp3"));

        _currentPlayer.Play();


        _gameActive = true;
        _timeRemaining = 30; // 30 seconds per round

        WordEntry.Text = "";
        WordEntry.IsEnabled = true;
        SubmitButton.IsEnabled = true;
        ResultsFrame.IsVisible = false;

        TimerLabel.Text = $"Time: {_timeRemaining}";
        _gameTimer.Start();
    }

    private void DisplayLetter(char VowOrCon)
    {
        //LettersContainer.Children.Clear();

        var letterLabel = new Label
        {
            Text = VowOrCon.ToString(),
            FontSize = 24,
            FontAttributes = FontAttributes.Bold,
            BackgroundColor = Colors.White,
            TextColor = Colors.Black,
            Padding = new Thickness(15, 10),
            Margin = new Thickness(5),
            HorizontalOptions = LayoutOptions.Center,
            VerticalOptions = LayoutOptions.Center
        };

        var frame = new Frame
        {
            Content = letterLabel,
            BackgroundColor = Colors.White,
            BorderColor = Colors.Gray,
            CornerRadius = 8,
            Padding = 0,
            HasShadow = true
        };

        LettersContainer.Children.Add(frame);
    }

    private void OnSubmitWordClicked(object sender, EventArgs e)
    {
        if (!_gameActive) return;

        string word = WordEntry.Text?.Trim().ToUpper() ?? "";

        if (string.IsNullOrEmpty(word))
        {
            DisplayAlert("Invalid Word", "Please enter a word.", "OK");
            return;
        }

        if (!IsValidWord(word))
        {
            DisplayAlert("Invalid Word", "This word cannot be made from the available letters.", "OK");
            return;
        }

        int score = CalculateScore(word);
        SubmittedWords.Add(new WordScore { Word = word, Score = score });

        ResultLabel.Text = $"'{word}' is valid!";
        ScoreLabel.Text = $"Score: {score} points";
        ResultsFrame.IsVisible = true;

        WordEntry.Text = "";
    }

    private bool IsValidWord(string word)
    {
        // Check if word is in the dictionary
        if (!_validWords.Contains(word))
            return false;

        // Check if word can be made from available letters
        var availableLetters = new List<char>(_currentLetters);

        foreach (char c in word)
        {
            if (!availableLetters.Contains(c))
                return false;

            availableLetters.Remove(c);
        }

        return true;
    }

    static char GenerateVowel()

    {
        char[] vowels = new char[67];

        //first 15 filled with 'a'
        for (int i = 0; i < 15; i++)
        {
            vowels[i] = 'a';
        }

        //next 21 filled with e
        for (int i = 15; i < 37; i++)
        {
            vowels[i] = 'e';
        }

        //i
        for (int i = 37; i < 50; i++)
        {
            vowels[i] = 'i';
        }

        //o
        for (int i = 50; i < 63; i++)
        {
            vowels[i] = 'o';
        }

        //u
        for (int i = 63; i < 67; i++)
        {
            vowels[i] = 'u';
        }

        //random number between 0 and 67
        Random random = new Random();
        int randomNumber = random.Next(0, 67);

        //setting this so that we can use _currentLetters array to determine if word is valid
        _currentLetters[count] = vowels[randomNumber];

        return vowels[randomNumber];

    }
    static char GenerateConsonant()
    {
        char[] consonants = new char[74];

        //b
        for (int i = 0; i < 2; i++)
        {
            consonants[i] = 'b';
        }

        //c
        for (int i = 2; i < 5; i++)
        {
            consonants[i] = 'c';
        }

        //d
        for (int i = 5; i < 11; i++)
        {
            consonants[i] = 'd';
        }

        //f
        for (int i = 11; i < 13; i++)
        {
            consonants[i] = 'f';
        }

        //g
        for (int i = 13; i < 16; i++)
        {
            consonants[i] = 'g';
        }

        //h
        for (int i = 16; i < 18; i++)
        {
            consonants[i] = 'h';
        }

        //j
        for (int i = 18; i < 19; i++)
        {
            consonants[i] = 'j';
        }

        //k
        for (int i = 19; i < 20; i++)
        {
            consonants[i] = 'k';
        }

        //l
        for (int i = 20; i < 25; i++)
        {
            consonants[i] = 'l';
        }

        //m
        for (int i = 25; i < 29; i++)
        {
            consonants[i] = 'm';
        }

        //n
        for (int i = 29; i < 37; i++)
        {
            consonants[i] = 'n';
        }

        //p
        for (int i = 37; i < 41; i++)
        {
            consonants[i] = 'p';
        }

        //q
        for (int i = 41; i < 42; i++)
        {
            consonants[i] = 'q';
        }

        //r
        for (int i = 42; i < 51; i++)
        {
            consonants[i] = 'r';
        }

        //s
        for (int i = 51; i < 60; i++)
        {
            consonants[i] = 's';
        }

        //t
        for (int i = 60; i < 69; i++)
        {
            consonants[i] = 't';
        }

        //v
        for (int i = 69; i < 70; i++)
        {
            consonants[i] = 'v';
        }

        //w
        for (int i = 70; i < 71; i++)
        {
            consonants[i] = 'w';
        }

        //x
        for (int i = 71; i < 72; i++)
        {
            consonants[i] = 'x';
        }

        //y
        for (int i = 72; i < 73; i++)
        {
            consonants[i] = 'y';
        }

        //z
        for (int i = 73; i < 74; i++)
        {
            consonants[i] = 'z';
        }

        //random number between 0 and 74
        Random random = new Random();
        int randomNumber = random.Next(0, 74);

        //setting this so that we can use _currentLetters array to determine if word is valid
        _currentLetters[count] = consonants[randomNumber];

        return consonants[randomNumber];
    }

    private int CalculateScore(string word)
    {
        // Simple scoring: 1 point per letter, bonus for longer words
        int baseScore = word.Length;
        int bonus = word.Length >= 6 ? word.Length * 2 : 0;
        return baseScore + bonus;
    }

    private void EndGame()
    {
        _gameActive = false;
        _gameTimer.Stop();

        WordEntry.IsEnabled = false;
        SubmitButton.IsEnabled = false;

        TimerLabel.Text = "Time's Up!";

        if (!ResultsFrame.IsVisible)
        {
            ResultLabel.Text = "Time's up! Try again.";
            ScoreLabel.Text = "";
            ResultsFrame.IsVisible = true;
        }
    }
}

