using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;


public class LevelStartScreen : MonoBehaviour {
	private GameStateManager t_GameStateManager;
	private float loadScreenDelay = 2;

	public Text WorldTextHUD;
	public Text ScoreTextHUD;
	public Text CoinTextHUD;
	public Text WorldTextMain;
	public Text livesText;

	// Use this for initialization
	void Start () {
		Time.timeScale = 1;

		t_GameStateManager = GameStateManager.GetOrCreate (this);
		string worldName = t_GameStateManager.sceneToLoad;
		if (string.IsNullOrEmpty (worldName)) {
			/* No level queued - this screen was opened on its own, so there is nothing to
			 * count down to. Send the player to the Main Menu instead of loading null. */
			worldName = "Main Menu";
			t_GameStateManager.sceneToLoad = worldName;
		}

		WorldTextHUD.text = GameStateManager.WorldLabel (worldName);
		ScoreTextHUD.text = t_GameStateManager.scores.ToString ("D6");
		CoinTextHUD.text = "x" + t_GameStateManager.coins.ToString ("D2");
		WorldTextMain.text = worldName.ToUpper ();
		livesText.text = t_GameStateManager.lives.ToString ();

		StartCoroutine (LoadSceneDelayCo (t_GameStateManager.sceneToLoad, loadScreenDelay));

		Debug.Log (this.name + " Start: current scene is " + SceneManager.GetActiveScene ().name);
	}
	
	IEnumerator LoadSceneDelayCo(string sceneName, float delay) {
		yield return new WaitForSecondsRealtime (delay);
		SceneManager.LoadScene (sceneName);
	}
}
