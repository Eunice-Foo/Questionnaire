using UnityEngine;
using UnityEngine.UIElements;

public class Question : MonoBehaviour {

    // Create variables to store the UI Document
    // Each UI Document is a separate GameObject in the hierarchy
    UIDocument titleUIDocument, questionUIDocument, resultUIDocument, notificationUIDocument;
    // Root visual element is the top hierarchy of the UI Document
    // It contains all the UI elements in the document
    VisualElement titleRoot, questionRoot, resultRoot, notificationRoot;

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

    // Declare all variables and UI elements
    int[] score = new int[21];
    Button[] scoreButtons = new Button[4];
    int currentQuestionIndex = 0;
    Label questionLabel, questionNumberLabel;
    Button nextButton, previousButton;
    int depressionScore, anxietyScore, stressScore = 0;
    string depressionLevel, anxietyLevel, stressLevel = "";
    VisualElement[] questionIndicators = new VisualElement[21];

    // MonoBehaviour Awake method is called when the script is being executed
    void Awake() {
        // Find UI Documents by using their GameObject names in the hierarchy
        titleUIDocument = GameObject.Find("Title").GetComponent<UIDocument>();
        questionUIDocument = GameObject.Find("Question").GetComponent<UIDocument>();
        resultUIDocument = GameObject.Find("Result").GetComponent<UIDocument>();
        notificationUIDocument = GameObject.Find("Notification").GetComponent<UIDocument>();

        // Get the root visual elements from each UI document by element name
        // Control visibility of the entire screen
        titleRoot = titleUIDocument.rootVisualElement.Q("Background");
        questionRoot = questionUIDocument.rootVisualElement.Q("Background");
        resultRoot = resultUIDocument.rootVisualElement.Q("Background");
        notificationRoot = notificationUIDocument.rootVisualElement.Q("Background");

        // Initially show only the title screen
        SetUIVisibility(showTitle: true, showQuestion: false, showResult: false);

        // Notification message is hidden initially unless trigglered
        notificationRoot.style.display = DisplayStyle.None;
    }

    // Control visibility of different screens
    // When boolean value is true, DisplayStyle.Flex is used to show the screen
    // When boolean value is false, DisplayStyle.None is used to hide the screen
    private void SetUIVisibility(bool showTitle, bool showQuestion, bool showResult) {
        // ? symbol is used to check if the value is true or false
        titleRoot.style.display = showTitle ? DisplayStyle.Flex : DisplayStyle.None;
        questionRoot.style.display = showQuestion ? DisplayStyle.Flex : DisplayStyle.None;
        resultRoot.style.display = showResult ? DisplayStyle.Flex : DisplayStyle.None;
    }

    // MonoBehaviour Start method is called before the first frame update
    // Match all the UI elements with the variables and set up the interactions
    // This method is call once only when the script is first executed
    void Start() {
        SetupTitleUI();
        SetupQuestionUI();
        SetupResultUI();
        SetupNotificationUI();
    }

    void SetupTitleUI() {
        // Get the root visual element first to access all UI elements
        var root = titleUIDocument.rootVisualElement;
        
        // Find the start button in the title screen
        var startButton = root.Q<Button>("startButton");
        
        // User clicks the start button to start the questionnaire
        startButton.clicked += StartQuestionnaire;
    }

    void StartQuestionnaire() {
        // Let all the scores be -1
        // -1 means that the question has not been answered yet
        for (int i = 0; i < score.Length; i++) score[i] = -1;

        // Hide the title screen and show the question screen
        SetUIVisibility(false, true, false);

        // Start with setting up the first question, question[0]
        currentQuestionIndex = 0;
        UpdateQuestionDisplay();
    }

    // When the user clicks the "Next" or "Previous" button
    void UpdateQuestionDisplay() {
        // Change the question text and the question number
        questionLabel.text = questions[currentQuestionIndex];
        questionNumberLabel.text = $"Question {currentQuestionIndex + 1} of {questions.Length}";

        // For the first question (Q1), the "Previous" button is hidden
        previousButton.style.display = (currentQuestionIndex == 0) ? DisplayStyle.None : DisplayStyle.Flex;

        // If user reaches the last question (Q21), change the "Next" button to "Submit"
        nextButton.text = (currentQuestionIndex == questions.Length - 1) ? "Submit" : "Next";

        UpdateSelectedState();
        UpdateQuestionIndicators();
    }

    // Show the score button is being selected
    void UpdateSelectedState() {
        // Clear all button selection first
        for (int i = 0; i < scoreButtons.Length; i++) {
            scoreButtons[i].RemoveFromClassList("selected");
        }

        // Get the selected score from the array
        int selectedAnswer = score[currentQuestionIndex];

        // If the selected score is between 0 and 3, show the selected state
        if (selectedAnswer >= 0 && selectedAnswer < 4) {
            scoreButtons[selectedAnswer].AddToClassList("selected");
        }

        // If the selected score is -1,
        // means that this question has not been answered yet
        // Therefore no score button is being selected
    }

    void SetupQuestionUI() {
        // Get the root visual element first to access all the UI elements
        var root = questionUIDocument.rootVisualElement;
        
        // Match the question label and question number label with the variables
        questionLabel = root.Q<Label>("question");
        questionNumberLabel = root.Q<Label>("questionNumber");
        
        // Match all four score buttons
        for (int i = 0; i < scoreButtons.Length; i++) {
            scoreButtons[i] = root.Q<Button>($"score{i}");
        }
        
        // Match the next and previous buttons
        nextButton = root.Q<Button>("nextButton");
        previousButton = root.Q<Button>("previousButton");
        
        // Add this line to set up the indicators
        SetupQuestionIndicators();
        
        SetupScoreButtons();
        SetupNavigationButtons();
    }

    // Set up the interaction for the score buttons
    void SetupScoreButtons() {
        for (int i = 0; i < scoreButtons.Length; i++)
        {
            int selectedScore = i;
            // When user clicks on the score button
            scoreButtons[i].clicked += () =>
            {
                // Store the selected score for the current question
                score[currentQuestionIndex] = selectedScore;
                
                // Show the score button as selected state
                UpdateSelectedState();
                UpdateQuestionIndicators(); // Add this line to update indicators after answering
            };
        }
    }

    // Set up the interaction for the navigation buttons
    void SetupNavigationButtons() {
        nextButton.clicked += () => {
            if (currentQuestionIndex < questions.Length - 1) {
                currentQuestionIndex++;
                UpdateQuestionDisplay();
            
            // When reached the last question and "Submit" button is clicked
            } else {
                // Check if all questions are answered
                // For unanswered questions, the score == -1
                for (int i = 0; i < score.Length; i++) {
                    if (score[i] == -1) {
                        // If found unanswered question, show notification
                        ToggleNotificationVisibility(true);
                        return; // Exit to stop further execution
                    }
                }
                
                // All questions are answered, proceed with submission
                CalculateTotalScore();
                DetermineSeverity();
                ShowFinalResults();
            }
        };

        previousButton.clicked += () => {
            // Except the first question, question[0]
            if (currentQuestionIndex > 0) {
                currentQuestionIndex--;
                UpdateQuestionDisplay();
            }
        };
    }

    // Control the visibility of the notification message
    private void ToggleNotificationVisibility(bool show) {
        notificationRoot.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
    }

    void SetupNotificationUI() {
        var root = notificationUIDocument.rootVisualElement;
        var confirmButton = root.Q<Button>("Confirm");
        // Close the message when click on the "Confirm" button
        confirmButton.clicked += () => ToggleNotificationVisibility(false);
    }

    void CalculateTotalScore() {
        depressionScore = anxietyScore = stressScore = 0;

        for (int i = 0; i < questions.Length; i++) {
            // If the score is not -1, means that the question has been answered
            if (score[i] >= 0) {
                // Add the score following the corresponding category
                if (category[i] == 'D') depressionScore += (score[i] * 2);
                else if (category[i] == 'A') anxietyScore += (score[i] * 2);
                else if (category[i] == 'S') stressScore += (score[i] * 2);
            }
        }
    }

    void DetermineSeverity() {
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

    // Assign colours based on severity level
    Color GetSeverityColour(string severityLevel) {
        switch (severityLevel) {
            case "Normal":
                return new Color(0.83f, 0.95f, 0.64f); // Green
            case "Mild":
                return new Color(1.00f, 0.96f, 0.71f); // Yellow
            case "Moderate":
                return new Color(0.98f, 0.74f, 0.52f); // Orange
            case "Severe":
                return new Color(0.92f, 0.53f, 0.62f); // Red
            case "Extremely Severe":
                return new Color(0.79f, 0.00f, 0.13f); // Dark red
            default:
                return Color.white;
        }
    }

    // Change text color based on severity level
    void UpdateTextColorForSeverity(Label scoreLabel, Label levelLabel, string severityLevel) {
        // Default text color is black
        Color textColor = Color.black;
        
        // For "Extremely Severe" level, use white text for better contrast
        if (severityLevel == "Extremely Severe") {
            textColor = Color.white;
        }
        
        // Apply the color to both the score and level labels
        scoreLabel.style.color = textColor;
        levelLabel.style.color = textColor;
    }

    // Modify ShowFinalResults method to apply colors
    void ShowFinalResults() {
        // Hide the question screen and show the result screen
        SetUIVisibility(false, false, true);
        
        // Get the root visual element to access all the UI elements
        var root = resultUIDocument.rootVisualElement;
        
        // Match the corresponding labels with the variables
        var depressionScoreLabel = root.Q<Label>("depressionScore");
        var depressionLevelLabel = root.Q<Label>("depressionLevel");
        var anxietyScoreLabel = root.Q<Label>("anxietyScore");
        var anxietyLevelLabel = root.Q<Label>("anxietyLevel");
        var stressScoreLabel = root.Q<Label>("stressScore");
        var stressLevelLabel = root.Q<Label>("stressLevel");
        
        // Get the result color containers
        var depressionColour = root.Q("depressionColour");
        var anxietyColour = root.Q("anxietyColour");
        var stressColour = root.Q("stressColour");
        
        // Set the text of the labels to display the scores and levels
        depressionScoreLabel.text = depressionScore.ToString();
        depressionLevelLabel.text = depressionLevel;
        anxietyScoreLabel.text = anxietyScore.ToString();
        anxietyLevelLabel.text = anxietyLevel;
        stressScoreLabel.text = stressScore.ToString();
        stressLevelLabel.text = stressLevel;
        
        // Apply colours based on severity levels
        depressionColour.style.backgroundColor = GetSeverityColour(depressionLevel);
        anxietyColour.style.backgroundColor = GetSeverityColour(anxietyLevel);
        stressColour.style.backgroundColor = GetSeverityColour(stressLevel);
        
        // Update text colors based on severity
        UpdateTextColorForSeverity(depressionScoreLabel, depressionLevelLabel, depressionLevel);
        UpdateTextColorForSeverity(anxietyScoreLabel, anxietyLevelLabel, anxietyLevel);
        UpdateTextColorForSeverity(stressScoreLabel, stressLevelLabel, stressLevel);
    }

    void SetupResultUI() {
        // Get the root visual element to access all UI elements
        var root = resultUIDocument.rootVisualElement;

        // Find the restart button in the result screen
        var restartButton = root.Q<Button>("restartButton");
        
        restartButton.clickable = new Clickable(() => {
            // Reset all questions to unanswered state
            for (int i = 0; i < score.Length; i++) score[i] = -1;
            
            // Reset all scores and levels
            depressionScore = anxietyScore = stressScore = 0;
            depressionLevel = anxietyLevel = stressLevel = "";
            currentQuestionIndex = 0;
            
            // Hide the result screen and show the title screen
            SetUIVisibility(true, false, false);
        });
    }

    void SetupQuestionIndicators() {
        var root = questionUIDocument.rootVisualElement;
        
        // Get all 21 question indicators
        for (int i = 0; i < questions.Length; i++) {
            int questionIndex = i; // Capture for lambda
            questionIndicators[i] = root.Q($"question{i+1}");
            
            // Add click event to navigate to the question
            questionIndicators[i].RegisterCallback<ClickEvent>(_ => {
                currentQuestionIndex = questionIndex;
                UpdateQuestionDisplay();
            });
        }
        
        // Initialize the indicators
        UpdateQuestionIndicators();
    }

    // Modify UpdateQuestionIndicators() to use classes instead of inline styles
    void UpdateQuestionIndicators() {
        for (int i = 0; i < questions.Length; i++) {
            bool isAnswered = score[i] != -1;
            bool isCurrentQuestion = i == currentQuestionIndex;
            
            // Remove any previous classes
            questionIndicators[i].RemoveFromClassList("answered");
            questionIndicators[i].RemoveFromClassList("current");
            
            // Add appropriate classes instead of setting style directly
            if (isCurrentQuestion) {
                questionIndicators[i].AddToClassList("current");
            } else if (isAnswered) {
                questionIndicators[i].AddToClassList("answered");
            }
            // No class for unanswered, non-current questions - will use default style
        }
    }
}