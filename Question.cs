using UnityEngine;
using UnityEngine.UIElements;

public class Question : MonoBehaviour
{
    // UI Document references - Components that manage UI assets in Unity's UI Toolkit
    UIDocument titleUIDocument;    // Controls the title screen
    UIDocument questionUIDocument; // Controls the questionnaire screen 
    UIDocument resultUIDocument;   // Controls the results screen

    // Root visual elements for each UI panel - these are the container elements in each UXML file
    VisualElement titleRoot;      // Root element in the Title.uxml (contains the welcome message and start button)
    VisualElement questionRoot;   // Root element in the Question.uxml (contains question text and answer buttons)
    VisualElement resultRoot;     // Root element in the Result.uxml (contains score display and restart button)

    // List of question and the corresponding category
    string[] questions = new string[] {
        "I found it hard to wind down.",
        "I was aware of dryness of my mouth.",
        "I couldn't seem to experience any positive feeling at all.",
        "I experienced breathing difficulty (e.g. excessively rapid breathing, breathlessness in the absence of physical exertion).",
        "I found it difficult to work up the initiative to do things.",
        "I tended to over-react to situations.",
        "I experienced trembling (e.g. in the hands).",
        "I felt that I was using a lot of nervous energy.",
        "I was worried about situations in which I might panic and make a fool of myself.",
        "I felt that I had nothing to look forward to.",
        "I found myself getting agitated.",
        "I found it difficult to relax.",
        "I felt down-hearted and blue.",
        "I was intolerant of anything that kept me from getting on with what I was doing.",
        "I felt I was close to panic.",
        "I was unable to become enthusiastic about anything.",
        "I felt I wasn't worth much as a person.",
        "I felt that I was rather touchy.",
        "I was aware of the action of my heart in the absence of physical exertion (e.g. sense of heart rate increase, heart missing a beat).",
        "I felt scared without any good reason.",
        "I felt that life was meaningless."
    };

    char[] category = new char[] {
        'S', 'A', 'D', 'A', 'D',
        'S', 'A', 'S', 'A', 'D',
        'S', 'S', 'D', 'S', 'A',
        'D', 'D', 'S', 'A', 'A', 'D'
    };

    int[] score = new int[21];
    int currentQuestionIndex = 0;
    int depressionScore, anxietyScore, stressScore = 0;
    string depressionLevel, anxietyLevel, stressLevel = "";

    // UI elements
    Label questionLabel;
    Label questionNumberLabel;
    Button[] answerButtons = new Button[4];
    Button nextButton;
    Button previousButton;

    // MonoBehaviour Awake method is called when the script is being executed
    void Awake()
    {
        // Find UI Documents by using their GameObject names in the hierarchy
        titleUIDocument = GameObject.Find("Title").GetComponent<UIDocument>();
        questionUIDocument = GameObject.Find("Question").GetComponent<UIDocument>();
        resultUIDocument = GameObject.Find("Result").GetComponent<UIDocument>();

        // Get the root visual elements from each UI document by element name
        // Root visual element is the top-level hierarchy which contains all other UI elements
        // Control visibility of the entire screen
        titleRoot = titleUIDocument.rootVisualElement.Q("Background");
        questionRoot = questionUIDocument.rootVisualElement.Q("Background");
        resultRoot = resultUIDocument.rootVisualElement.Q("Background");

        // Initially show only the title screen
        SetUIVisibility(showTitle: true, showQuestion: false, showResult: false);
    }

    // Control visibility of different screens
    // When boolean value is true, DisplayStyle.Flex is used to show the screen
    // When boolean value is false, DisplayStyle.None is used to hide the screen
    private void SetUIVisibility(bool showTitle, bool showQuestion, bool showResult)
    {
        // ? symbol is used to check if the value is true or false
        titleRoot.style.display = showTitle ? DisplayStyle.Flex : DisplayStyle.None;
        questionRoot.style.display = showQuestion ? DisplayStyle.Flex : DisplayStyle.None;
        resultRoot.style.display = showResult ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // MonoBehaviour Start method is called before the first frame update
    void Start()
    {
        SetupTitleUI();
        SetupQuestionUI();
        SetupResultUI();
    }

    void SetupTitleUI()
    {
        // Get the root visual element first to access to all UI elements in the title screen
        // Find the start button in the title screen
        // Use .Q() query method to find the element by name
        var startButton = titleUIDocument.rootVisualElement.Q<Button>("startButton");

        // User clicks the start button to start the questionnaire
        startButton.clicked += StartQuestionnaire;
    }

    void StartQuestionnaire()
    {
        // let all the score to be -1
        // -1 means that the question has not been answered yet
        for (int i = 0; i < score.Length; i++) score[i] = -1;

        // Hide the title screen and show the question screen
        SetUIVisibility(false, true, false);

        SetupQuestionForDisplay();
    }

    void SetupQuestionForDisplay()
    {
        currentQuestionIndex = 0;
        UpdateQuestionDisplay();
        ClearAllButtonSelections();
    }

    // When the user clicks the "Next" or "Previous" button
    void UpdateQuestionDisplay()
    {
        // Change the question text and the question number
        questionLabel.text = questions[currentQuestionIndex];
        questionNumberLabel.text = $"Question {currentQuestionIndex + 1} of {questions.Length}";

        // For the first question (Q1), the "Previous" button is hidden
        previousButton.style.display = (currentQuestionIndex == 0) ? DisplayStyle.None : DisplayStyle.Flex;

        // If user reaches the last question (Q21), change the "Next" button to "Submit"
        nextButton.text = (currentQuestionIndex == questions.Length - 1) ? "Submit" : "Next";

        UpdateSelectedState();
    }

    void UpdateSelectedState()
    {
        for (int i = 0; i < 4; i++)
        {
            answerButtons[i].RemoveFromClassList("selected");
        }

        // Store the selected score for the current question into the array
        int selectedAnswer = score[currentQuestionIndex];


        if (selectedAnswer >= 0 && selectedAnswer < 4)
        {
            answerButtons[selectedAnswer].AddToClassList("selected");
        }
    }

    void ClearAllButtonSelections()
    {
        for (int i = 0; i < 4; i++)
        {
            answerButtons[i].RemoveFromClassList("selected");
        }
    }

    void SetupQuestionUI()
    {
        var root = questionUIDocument.rootVisualElement;

        questionLabel = root.Q<Label>("question");
        questionNumberLabel = root.Q<Label>("questionNumber");

        answerButtons = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            answerButtons[i] = root.Q<Button>($"answer{i}");
        }

        nextButton = root.Q<Button>("nextButton");
        previousButton = root.Q<Button>("previousButton");

        SetupAnswerButtons();
        SetupNavigationButtons();
    }

    void SetupAnswerButtons()
    {
        for (int i = 0; i < 4; i++)
        {
            int capturedIndex = i;
            answerButtons[i].clicked += () =>
            {
                score[currentQuestionIndex] = capturedIndex;
                UpdateSelectedState();
            };
        }
    }

    void SetupNavigationButtons()
    {
        nextButton.clicked += () =>
        {
            if (currentQuestionIndex < questions.Length - 1)
            {
                currentQuestionIndex++;
                UpdateQuestionDisplay();

                // When reached the last question
            }
            else
            {
                CalculateTotalScore();
                DetermineSeverity();
                ShowFinalResults();
            }
        };

        previousButton.clicked += () =>
        {
            // Except the first question
            if (currentQuestionIndex > 0)
            {
                currentQuestionIndex--;
                UpdateQuestionDisplay();
            }
        };
    }

    void CalculateTotalScore()
    {
        depressionScore = anxietyScore = stressScore = 0;

        for (int i = 0; i < questions.Length; i++)
        {
            if (score[i] >= 0)
            {
                // Add the score following the corresponding category
                if (category[i] == 'D') depressionScore += (score[i] * 2);
                else if (category[i] == 'A') anxietyScore += (score[i] * 2);
                else if (category[i] == 'S') stressScore += (score[i] * 2);
            }
        }
    }

    void DetermineSeverity()
    {
        // Determine depression level
        if (depressionScore <= 9) depressionLevel = "Normal";
        else if (depressionScore <= 13) depressionLevel = "Mild";
        else if (depressionScore <= 20) depressionLevel = "Moderate";
        else if (depressionScore <= 27) depressionLevel = "Severe";
        else depressionLevel = "Extremely Severe";

        // Determine anxiety level
        if (anxietyScore <= 7) anxietyLevel = "Normal";
        else if (anxietyScore <= 9) anxietyLevel = "Mild";
        else if (anxietyScore <= 14) anxietyLevel = "Moderate";
        else if (anxietyScore <= 19) anxietyLevel = "Severe";
        else anxietyLevel = "Extremely Severe";

        // Determine stress level
        if (stressScore <= 14) stressLevel = "Normal";
        else if (stressScore <= 18) stressLevel = "Mild";
        else if (stressScore <= 25) stressLevel = "Moderate";
        else if (stressScore <= 33) stressLevel = "Severe";
        else stressLevel = "Extremely Severe";
    }

    void ShowFinalResults()
    {
        // Hide the question screen and show the result screen
        SetUIVisibility(false, false, true);

        // Get the root visual element to access all the UI elements
        var resultRoot = resultUIDocument.rootVisualElement;

        // Match the corresponding labels with the variables
        var depressionScoreLabel = resultRoot.Q<Label>("depressionScore");
        var depressionLevelLabel = resultRoot.Q<Label>("depressionLevel");
        var anxietyScoreLabel = resultRoot.Q<Label>("anxietyScore");
        var anxietyLevelLabel = resultRoot.Q<Label>("anxietyLevel");
        var stressScoreLabel = resultRoot.Q<Label>("stressScore");
        var stressLevelLabel = resultRoot.Q<Label>("stressLevel");

        // Set the text of the labels to display the scores and levels
        depressionScoreLabel.text = depressionScore.ToString();
        depressionLevelLabel.text = depressionLevel;
        anxietyScoreLabel.text = anxietyScore.ToString();
        anxietyLevelLabel.text = anxietyLevel;
        stressScoreLabel.text = stressScore.ToString();
        stressLevelLabel.text = stressLevel;
    }
    
    void SetupResultUI() {
    var resultRoot = resultUIDocument.rootVisualElement;
    var restartButton = resultRoot.Q<Button>("restartButton");
    
    restartButton.clickable = new Clickable(() => {
        for (int i = 0; i < score.Length; i++) {
            score[i] = -1;
        }

        depressionScore = anxietyScore = stressScore = 0;
        depressionLevel = anxietyLevel = stressLevel = "";
        currentQuestionIndex = 0;
        
        SetUIVisibility(true, false, false);
    });
    }
}