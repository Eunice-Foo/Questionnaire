using UnityEngine;
using UnityEngine.UIElements;

public class Question : MonoBehaviour {
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
    Label questionCounterLabel;
    Button[] answerButtons = new Button[4];
    Button nextButton;
    Button previousButton;

    // Find all the UI Documents by using their GameObject names in the hierarchy
    void FindUIDocuments() {
        GameObject titleUIObject = GameObject.Find("Title");
        titleUIDocument = titleUIObject.GetComponent<UIDocument>();
        
        GameObject questionUIObject = GameObject.Find("Question");
        questionUIDocument = questionUIObject.GetComponent<UIDocument>();
        
        GameObject resultUIObject = GameObject.Find("Result");
        resultUIDocument = resultUIObject.GetComponent<UIDocument>();
    }
        
    void Awake() {
        FindUIDocuments();
        
        // Get the root visual elements from each UI document
        // Note: Title uses "Background" (capital B) while others use "background" (lowercase b)
        titleRoot = titleUIDocument.rootVisualElement.Q("Background");
        questionRoot = questionUIDocument.rootVisualElement.Q(null, "background");  
        resultRoot = resultUIDocument.rootVisualElement.Q(null, "background");
        
        // Initially show only the title screen
        SetUIVisibility(showTitle: true, showQuestion: false, showResult: false);
    }

    void Start() {
        SetupTitleUI();
        SetupQuestionUI();
        SetupResultUI();
    }
    

    // Control which UI panel is visible
    private void SetUIVisibility(bool showTitle, bool showQuestion, bool showResult) {
        // Use display style to show/hide panels instead of activating/deactivating GameObjects
        titleRoot.style.display = showTitle ? DisplayStyle.Flex : DisplayStyle.None;
        questionRoot.style.display = showQuestion ? DisplayStyle.Flex : DisplayStyle.None;
        resultRoot.style.display = showResult ? DisplayStyle.Flex : DisplayStyle.None;
    }
    
    void SetupTitleUI() {
        var titleRoot = titleUIDocument.rootVisualElement;
        var startButton = titleRoot.Q<Button>("startButton");
        startButton.clicked += StartQuestionnaire;
    }
    
    void StartQuestionnaire() {
        for (int i = 0; i < score.Length; i++) {
            score[i] = -1;
        }
        
        SetUIVisibility(false, true, false);
        SetupQuestionForDisplay();
    }
    
    void SetupQuestionForDisplay() {
        currentQuestionIndex = 0;
        UpdateQuestionDisplay();
        ClearAllButtonSelections();
    }

    void ClearAllButtonSelections() {
        for (int i = 0; i < 4; i++) {
            answerButtons[i].RemoveFromClassList("selected");
        }
    }
    
    void SetupQuestionUI() {
        var root = questionUIDocument.rootVisualElement;
        
        questionLabel = root.Q<Label>("question");
        questionCounterLabel = root.Q<Label>("questionCounter");
        
        answerButtons = new Button[4];
        for (int i = 0; i < 4; i++) {
            answerButtons[i] = root.Q<Button>($"answer{i}");
        }
        
        nextButton = root.Q<Button>("nextButton");
        previousButton = root.Q<Button>("previousButton");
        
        SetupAnswerButtons();
        SetupNavigationButtons();
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

    void ShowFinalResults() {
        SetUIVisibility(false, false, true);
        ShowResultScreen();
    }

    void SetupAnswerButtons() {
        for (int i = 0; i < 4; i++) {
            int capturedIndex = i;
            answerButtons[i].clicked += () => {
                score[currentQuestionIndex] = capturedIndex;
                UpdateSelectedState();
            };
        }
    }

    void SetupNavigationButtons() {
        nextButton.clicked += () => {
            if (currentQuestionIndex < questions.Length - 1) {
                currentQuestionIndex++;
                UpdateQuestionDisplay();
            } else {
                CalculateTotalScore();
                DetermineSeverity();
                ShowFinalResults();
            }
        };

        previousButton.clicked += () => {
            if (currentQuestionIndex > 0) {
                currentQuestionIndex--;
                UpdateQuestionDisplay();
            }
        };
    }

    void UpdateQuestionDisplay() {
        questionLabel.text = questions[currentQuestionIndex];
        questionCounterLabel.text = $"Question {currentQuestionIndex + 1} of {questions.Length}";
        
        // Update next button text based on whether this is the last question
        nextButton.text = (currentQuestionIndex == questions.Length - 1) ? "Submit" : "Next";
        
        // Hide previous button on first question, show it for all others
        previousButton.style.display = (currentQuestionIndex == 0) ? DisplayStyle.None : DisplayStyle.Flex;
        
        UpdateSelectedState();
    }
    
    void UpdateSelectedState() {
        for (int i = 0; i < 4; i++) {
            answerButtons[i].RemoveFromClassList("selected");
        }

        int selectedAnswer = score[currentQuestionIndex];
        if (selectedAnswer >= 0 && selectedAnswer < 4) {
            answerButtons[selectedAnswer].AddToClassList("selected");
        }
    }
    
    void CalculateTotalScore() {
        depressionScore = anxietyScore = stressScore = 0;
        
        for (int i = 0; i < questions.Length; i++) {
            if (score[i] >= 0) {
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
    
    void ShowResultScreen() {
        var resultRoot = resultUIDocument.rootVisualElement;
        
        var depressionScoreLabel = resultRoot.Q<Label>("depressionScore");
        var depressionLevelLabel = resultRoot.Q<Label>("depressionLevel");
        var anxietyScoreLabel = resultRoot.Q<Label>("anxietyScore");
        var anxietyLevelLabel = resultRoot.Q<Label>("anxietyLevel");
        var stressScoreLabel = resultRoot.Q<Label>("stressScore");
        var stressLevelLabel = resultRoot.Q<Label>("stressLevel");
        
        depressionScoreLabel.text = depressionScore.ToString();
        depressionLevelLabel.text = depressionLevel;
        anxietyScoreLabel.text = anxietyScore.ToString();
        anxietyLevelLabel.text = anxietyLevel;
        stressScoreLabel.text = stressScore.ToString();
        stressLevelLabel.text = stressLevel;
    }
}