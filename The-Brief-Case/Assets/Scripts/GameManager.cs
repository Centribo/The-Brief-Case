﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class GameManager : MonoBehaviour {

	public static GameManager instance = null;
	public static GameManager Instance { //Singleton pattern instance
		get { //Getter
			if(instance == null){ //If its null,
				instance = (GameManager)FindObjectOfType(typeof(GameManager)); //Find it
			}
			return instance; //Return it
		}
	}

	public enum State {MainMenu, CountingDown, Playing}; //What state is the game in right now?

	//Public variables
	public Material BlackSkybox;
	public Material NatureSkybox;
	public State state = State.MainMenu;
	public int maxTime; //Time that the players have to escape/win the game
	public bool[] puzzlesSolved = new bool[3];
	public bool isReadyToEnd = false;

	//Private variables
	DateTime initialTime;

	void Awake(){
		DontDestroyOnLoad(transform.gameObject);
	}

	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
		switch(state){
			case State.MainMenu:
				//Do nothing, most of this is handled in GUI events
			break;
			case State.CountingDown:
				int secondsLeft = 11 - GetTime();
				GameObject.Find("CountdownText").GetComponent<Text>().text = "" + (secondsLeft);

				if(secondsLeft <= 0){
					if(PlayerController.Instance.player == PlayerController.PlayerType.A){
						SceneManager.LoadScene("PlayerAScene");
					} else if(PlayerController.Instance.player == PlayerController.PlayerType.B){
						SceneManager.LoadScene("PlayerBScene");
					}
					

					state = State.Playing;
					ResetTimer();
					//MovePlayer
					if(PlayerController.Instance.player == PlayerController.PlayerType.A){
						PlayerController.Instance.currentRoom = Room.RoomType.Control;
						PlayerController.Instance.transform.position = new Vector3(0, 2.441f, -11.9f);
					} else if(PlayerController.Instance.player == PlayerController.PlayerType.B){
						PlayerController.Instance.currentRoom = Room.RoomType.Briefcase;
						PlayerController.Instance.transform.position = new Vector3(0, 2.441f, -0.9349999f);
					}
				}
			break;
		}
	}

	public void ResetTimer(){
		initialTime = DateTime.Now;
	}

	public int GetTimeRemaining(){ //Returns, in seconds, the time remaining before the players lose
		return maxTime - GetTime();
	}

	public int GetTime(){ //Returns time, in seconds, since timer was started
		return (DateTime.Now - initialTime).Seconds;
	}

	public void StartPlayerA(){
		PlayerController.Instance.player = PlayerController.PlayerType.A;
		StartCountdown();
		GameObject.Find("HelpText").GetComponent<Text>().text = "Your are in the control room.\nYour partner is in the room with the briefcase, communicate with them to find the code and escape.\nLook at objects and press the button to interact with them.\n\nGood Luck!";
		ResetTimer();
	}

	public void StartPlayerB(){
		PlayerController.Instance.player = PlayerController.PlayerType.B;
		StartCountdown();
		GameObject.Find("HelpText").GetComponent<Text>().text = "Your are with the briefcase.\nYour partner is in the control room, communicate with them to open the briefcase and escape.\nLook at objects and press the button to interact with them.\n\nGood Luck!";
		ResetTimer();
	}

	public void SetSolved(Room.RoomType room, bool isSolved){
		switch(room){
			case Room.RoomType.Maze:
				puzzlesSolved[0] = isSolved;
				BriefcaseController.Instance.SetLED(BriefcaseController.LED.Maze, true);
			break;
			case Room.RoomType.Wire:
				puzzlesSolved[1] = isSolved;
				BriefcaseController.Instance.SetLED(BriefcaseController.LED.Wire, true);
			break;
			case Room.RoomType.Riddle:
				puzzlesSolved[2] = isSolved;
				BriefcaseController.Instance.SetLED(BriefcaseController.LED.Riddle, true);
			break;
		}
	}

	public bool ArePuzzlesCompleted(){
		if(puzzlesSolved[0] && puzzlesSolved[1] && puzzlesSolved[2]){
			return true;
		}

		return false;
	}

	public void LoseGame(){
		Debug.Log("Game Lost!");
	}

	public void SetReadyToFinish(bool ready){
		isReadyToEnd = ready;
	}

	public bool GetReadyToFinish(){
		return isReadyToEnd;
	}

	public void FinishGame(){
		PlayerController.Instance.FadeToWhite();
		StartCoroutine(LoadLevelAfterDelay(5.0f, "EndingScene"));
		StartCoroutine(SetPlayerPosition(5.0f, new Vector3(0, 3, 0)));
	}

	IEnumerator LoadLevelAfterDelay(float delay, string level){
		yield return new WaitForSeconds(delay);
		SceneManager.LoadScene(level);
	}

	IEnumerator SetPlayerPosition(float delay, Vector3 position){
		yield return new WaitForSeconds(delay);
		PlayerController.Instance.transform.position = position;
	}

	void StartCountdown(){
		Transform canvasParent = GameObject.Find("Menu Canvas").transform;

		for(int i = canvasParent.childCount-1; i >= 0; i--){
			if(canvasParent.GetChild(i).name != "CountdownText"){
				if(canvasParent.GetChild(i).name == "HelpText"){
					canvasParent.GetChild(i).gameObject.SetActive(true);
				} else {
					canvasParent.GetChild(i).gameObject.SetActive(false);
				}
			}
		}

		ResetTimer();
		state = State.CountingDown;
	}

}
