using Plugin.Maui.Audio;
using System.Collections.ObjectModel;
using System.ComponentModel;
//using Plugin.Maui.Audio;

namespace countdown2;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
    private readonly Random _random = new();
    private readonly System.Timers.Timer _gameTimer = new();
    private List<char> _currentLetters = new();
    private int _timeRemaining;
    private bool _gameActive;
    private readonly HashSet<string> _validWords;

    // Vowels and consonants for letter selection
    private readonly char[] _vowels = { 'A', 'E', 'I', 'O', 'U' };
    private readonly char[] _consonants = { 'B', 'C', 'D', 'F', 'G', 'H', 'J', 'K', 'L', 'M', 'N', 'P', 'Q', 'R', 'S', 'T', 'V', 'W', 'X', 'Y', 'Z' };

    // Simple word list for validation (in a real app, you'd use a proper dictionary)
    
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

    private async void OnStartGameClicked(object sender, EventArgs e)
    {
        StartNewGame();

        //starting the mp3 file when 'start game' gets clicked
        var player = audioManager.CreatePlayer(await FileSystem.OpenAppPackageFileAsync
            ("countdown_clock_only.mp3"));

        player.Play();
    }

    private void StartNewGame()
    {
        _gameActive = true;
        _timeRemaining = 30; // 30 seconds per round

        GenerateRandomLetters();
        DisplayLetters();

        WordEntry.Text = "";
        WordEntry.IsEnabled = true;
        SubmitButton.IsEnabled = true;
        StartButton.Text = "New Game";
        ResultsFrame.IsVisible = false;

        TimerLabel.Text = $"Time: {_timeRemaining}";
        _gameTimer.Start();
    }

    private void GenerateRandomLetters()
    {
        _currentLetters.Clear();

        // Generate 9 letters: mix of vowels and consonants
        // Add 3-4 vowels
        int vowelCount = _random.Next(3, 5);
        for (int i = 0; i < vowelCount; i++)
        {
            _currentLetters.Add(_vowels[_random.Next(_vowels.Length)]);
        }

        // Fill the rest with consonants
        int consonantCount = 9 - vowelCount;
        for (int i = 0; i < consonantCount; i++)
        {
            _currentLetters.Add(_consonants[_random.Next(_consonants.Length)]);
        }

        // Shuffle the letters
        for (int i = _currentLetters.Count - 1; i > 0; i--)
        {
            int j = _random.Next(i + 1);
            (_currentLetters[i], _currentLetters[j]) = (_currentLetters[j], _currentLetters[i]);
        }
    }

    private void DisplayLetters()
    {
        LettersContainer.Children.Clear();

        foreach (char letter in _currentLetters)
        {
            var letterLabel = new Label
            {
                Text = letter.ToString(),
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
            DisplayAlert("Invalid Word", "This word cannot be made from the available letters or is not in our dictionary.", "OK");
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
        // Check if word is in our dictionary
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

public class WordScore
{
    public string Word { get; set; } = "";
    public int Score { get; set; }
}