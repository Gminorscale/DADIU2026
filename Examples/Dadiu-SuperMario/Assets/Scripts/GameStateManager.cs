using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;


public class GameStateManager : MonoBehaviour {
	public bool spawnFromPoint;
	public int spawnPointIdx;
	public int spawnPipeIdx;

	public int marioSize;
	public int lives;
	public int coins;
	public int scores;
	public float timeLeft;
	public bool hurryUp;

	public string sceneToLoad; // what scene to load after level start screen finishes?
	public bool timeup;

	void Awake () {
		if (FindObjectsOfType (GetType ()).Length == 1) {
			DontDestroyOnLoad (gameObject);
			ConfigNewGame ();
		} else {
			Destroy (gameObject);
		}
	}
	
	public void ResetSpawnPosition() {
		spawnFromPoint = true;
		spawnPointIdx = 0;
		spawnPipeIdx = 0;
	}

	public void SetSpawnPipe(int idx) {
		spawnFromPoint = false;
		spawnPipeIdx = idx;
	}

	public void ConfigNewGame() {
		marioSize = 0;
		lives = 3;
		coins = 0;
		scores = 0;
		timeLeft = 400.5f;
		hurryUp = false;
		ResetSpawnPosition ();
		sceneToLoad = null;
		timeup = false;
	}

	public void ConfigNewLevel() {
		timeLeft = 400.5f;
		hurryUp = false;
		ResetSpawnPosition ();
	}

	public void ConfigReplayedLevel() { // e.g. Mario respawns
		timeLeft = 400.5f;
		hurryUp = false;
	}

	public void SaveGameState() {
		LevelManager t_LevelManager = FindObjectOfType<LevelManager> ();
		marioSize = t_LevelManager.marioSize;
		lives = t_LevelManager.lives;
		coins = t_LevelManager.coins;
		scores = t_LevelManager.scores;
		timeLeft = t_LevelManager.timeLeft;
		hurryUp = t_LevelManager.hurryUp;
	}

	/* Level scenes are normally entered through the Main Menu, which is where the
	 * DontDestroyOnLoad GameStateManager is created. Pressing Play directly on a level
	 * scene (Test Scene, World 1-1, ...) skips that, so every screen that reads game
	 * state used to NullReference. Spawn one with new-game defaults instead, so any
	 * scene can be opened and tested on its own. */
	public static GameStateManager GetOrCreate(UnityEngine.Object context = null) {
		GameStateManager t_GameStateManager = FindObjectOfType<GameStateManager> ();
		if (t_GameStateManager == null) {
			t_GameStateManager = new GameObject ("GameStateManager (auto-created)")
				.AddComponent<GameStateManager> ();
			Debug.LogWarning ((context != null ? context.name + ": " : "")
				+ "no GameStateManager in the scene - created one with new-game defaults. "
				+ "Enter through the Main Menu scene for normal play.", context);
		}
		return t_GameStateManager;
	}

	/* "World 1-1" -> "1-1", for the small world label in the corner of the level screens.
	 * Scenes that aren't named "World x-y" (Test Scene, anything a student adds) are passed
	 * through unchanged instead of throwing an IndexOutOfRange on the split. */
	public static string WorldLabel(string sceneName) {
		if (string.IsNullOrEmpty (sceneName)) {
			return "";
		}
		string[] parts = Regex.Split (sceneName, "World ");
		return parts.Length > 1 ? parts[parts.Length - 1] : sceneName;
	}

}
