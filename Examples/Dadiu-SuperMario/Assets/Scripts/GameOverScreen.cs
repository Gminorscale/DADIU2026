using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;


public class GameOverScreen : MonoBehaviour {
	private GameStateManager t_GameStateManager;

	public Text WorldTextHUD;
	public Text ScoreTextHUD;
	public Text CoinTextHUD;
	public Text MessageText;

	[Header ("Wwise")]
	public AK.Wwise.Event WwGameOverMusic;
	public AK.Wwise.RTPC RTPC_MusicVolume;
	// Wwise Events don't expose their length to C#; measured from 09-game-over.mp3.
	public float gameOverMusicDuration = 6.38f;


	// Use this for initialization
	void Start () {
		Time.timeScale = 1;

		t_GameStateManager = GameStateManager.GetOrCreate (this);
		string worldName = t_GameStateManager.sceneToLoad;

		WorldTextHUD.text = GameStateManager.WorldLabel (worldName);
		ScoreTextHUD.text = t_GameStateManager.scores.ToString ("D6");
		CoinTextHUD.text = "x" + t_GameStateManager.coins.ToString ("D2");

		bool timeup = t_GameStateManager.timeup;
		if (!timeup) {
			MessageText.text = "GAME OVER";
		} else {
			StartCoroutine (ChangeMessageCo ());
		}

		RTPC_MusicVolume.SetGlobalValue (PlayerPrefs.GetFloat ("musicVolume", 1) * 100f);
		WwGameOverMusic.Post (gameObject);
		LoadMainMenu (gameOverMusicDuration);

		Debug.Log (this.name + " Start: current scene is " + SceneManager.GetActiveScene ().name);
	}

	IEnumerator LoadSceneDelayCo(string sceneName, float delay = 0) {
		yield return new WaitForSecondsRealtime (delay);
		SceneManager.LoadScene (sceneName);
	}

	IEnumerator ChangeMessageCo() { // TIME UP to GAME OVER
		MessageText.text = "TIME UP";
		yield return new WaitForSecondsRealtime (1f);
		MessageText.text = "GAME OVER";
	}

	void Update() {
		if (Input.GetButton("Pause")) {
			LoadMainMenu ();
		}
	}

	void LoadMainMenu(float delay = 0) {
		StartCoroutine (LoadSceneDelayCo ("Main Menu", delay));
	}
}
