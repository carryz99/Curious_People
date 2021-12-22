using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuizManager : MonoBehaviour
{
#pragma warning disable 649
    //referencia QuizGameUI script
    [SerializeField] private QuizGameUI quizGameUI;
    //ref scriptableobject file
    [SerializeField] private List<QuizDataScriptable> quizDataList;
    [SerializeField] private float timeInSeconds;
#pragma warning restore 649

    private string currentCategory = "";
    private int correctAnswerCount = 0;
    //questions data
    private List<Question> questions;
    //current question data
    private Question selectedQuetion = new Question();
    private int gameScore;
    private int lifesRemaining;
    private float currentTime;
    private QuizDataScriptable dataScriptable;
    private AudioSource sonido;
    public AudioClip correctSound;
    public AudioClip incorrectSound;

    private GameStatus gameStatus = GameStatus.NEXT;

    public GameStatus GameStatus { get { return gameStatus; } }

    public List<QuizDataScriptable> QuizData { get => quizDataList; }

    void Start()
    {
        sonido = GetComponent<AudioSource>();
    }
    public void StartGame(int categoryIndex, string category)
    {
        currentCategory = category;
        correctAnswerCount = 0;
        gameScore = 0;
        lifesRemaining = 3;
        currentTime = timeInSeconds;
        //set the questions data
        questions = new List<Question>();
        dataScriptable = quizDataList[categoryIndex];
        questions.AddRange(dataScriptable.questions);
        //select the question
        SelectQuestion();
        gameStatus = GameStatus.PLAYING;
    }

    
    private void SelectQuestion()
    {
        //pregunta random
        int val = UnityEngine.Random.Range(0, questions.Count);
        
        selectedQuetion = questions[val];
        //envía la pregunta a quizGameUI
        quizGameUI.SetQuestion(selectedQuetion);

        questions.RemoveAt(val);
    }

    private void Update()
    {
        
        if (gameStatus == GameStatus.PLAYING)
        {
            currentTime -= Time.deltaTime;
            SetTime(currentTime);
        }
    }

    void SetTime(float value)
    {
        TimeSpan time = TimeSpan.FromSeconds(currentTime);                       //set the time value
        quizGameUI.TimerText.text = time.ToString("mm':'ss");   //convert time to Time format

        if (currentTime <= 0)
        {
            //Game Over
            GameEnd();
        }
    }

    /// <param name="selectedOption">answer string</param>
    /// <returns></returns>
    public bool Answer(string selectedOption)
    {
        //set default to false
        bool correct = false;
        //respuesta = a respuesta correcta
        if (selectedQuetion.correctAns == selectedOption)
        {
            //Yes, Ans is correct
            correctAnswerCount++;
            correct = true;
            gameScore += 50;
            quizGameUI.ScoreText.text = "Score:" + gameScore;
            sonido.clip = correctSound;
            sonido.Play();
        }
        else
        {
            //No, Ans is wrong
            //Reduce Life
            lifesRemaining--;
            quizGameUI.ReduceLife(lifesRemaining);

            if (lifesRemaining == 0)
            {
                GameEnd();
            }

            sonido.clip = incorrectSound;
            sonido.Play();
        }

       
        if (gameStatus == GameStatus.PLAYING)
        {
            

            if (questions.Count > 0)
            {
                //call SelectQuestion method again after 1s
                Invoke("SelectQuestion", 0.4f);
            }
            else
            {
                GameEnd();
            }
        }

        if (gameScore >= 500)
        {
            NextLevel();
        }
        


        //return the value of correct bool
        return correct;
    }

    private void GameEnd()
    {
        quizGameUI.GameOverPanel.SetActive(true);

       
        //guarda el score
    }
    
    private void NextLevel()
    {
        gameStatus = GameStatus.NEXT;
        quizGameUI.NextLevelPanel.SetActive(true);

        PlayerPrefs.SetInt(currentCategory, correctAnswerCount);


    }

}

//Datastructure for storeing the quetions data
[System.Serializable]
public class Question
{
    public string questionInfo;         //question text
    public QuestionType questionType;   //type
    public Sprite questionImage;        //image for Image Type
    public AudioClip audioClip;         //audio for audio type
    public UnityEngine.Video.VideoClip videoClip;   //video for video type
    public List<string> options;        //options to select
    public string correctAns;           //correct option
}

[System.Serializable]
public enum QuestionType
{
    TEXT,
    IMAGE,
    AUDIO,
    VIDEO
}

[SerializeField]
public enum GameStatus
{
    PLAYING,
    NEXT
}