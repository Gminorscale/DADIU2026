using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Text.RegularExpressions;


public class LevelManager : MonoBehaviour {
	private const float loadSceneDelay = 1f;

	public bool hurryUp; // within last 100 secs?
	public int marioSize; // 0..2
	public int lives;
	public int coins;
	public int scores;
	public float timeLeft;
	private int timeLeftInt;

	private bool isRespawning;
	private bool isPoweringDown;

	public bool isInvinciblePowerdown;
	public bool isInvincibleStarman;
	private float MarioInvinciblePowerdownDuration = 2;
	private float MarioInvincibleStarmanDuration = 12;
	private float transformDuration = 1;

	private GameStateManager t_GameStateManager;
	private Mario mario;
	private Animator mario_Animator;
	private Rigidbody2D mario_Rigidbody2D;

	[Header ("Game Data")]
	public Text scoreText;
	public Text coinText;
	public Text timeText;
	public GameObject FloatingTextEffect;
	private const float floatingTextOffsetY = 2f;

		[Header ("Game Stats")]
	public int coinBonus = 200;
	public int powerupBonus = 1000;
	public int starmanBonus = 1000;
	public int oneupBonus = 0;
	public int breakBlockBonus = 50;

	public Vector2 stompBounceVelocity = new Vector2 (0, 15);

	public bool gamePaused;
	public bool timerPaused;
	public bool musicPaused;


	/****************** Wwise audio
	 * All audio in this project is authored in Wwise. There are no AudioSources or
	 * AudioClips left in the game code - every sound below is an Event bound in the
	 * Inspector with the Wwise Picker.
	 *
	 * An unbound (None) Event is a safe no-op: AK.Wwise.Event.Post() checks IsValid()
	 * before it does anything. So while Events are still being authored the game runs
	 * silently instead of throwing - that also means "no sound" here usually means
	 * "Event not picked in the Inspector" or "SoundBank not generated", not a crash.
	 */

	[Header ("Wwise Music")]
	public AK.Wwise.Event musicSource;   // starts the interactive music system (MUS_PlayMainPlaylist)
	public AK.Wwise.Event levelMusic;
	public AK.Wwise.Event levelMusicHurry;
	public AK.Wwise.Event starmanMusic;
	public AK.Wwise.Event starmanMusicHurry;
	public AK.Wwise.Event levelCompleteMusic;
	public AK.Wwise.Event castleCompleteMusic;
	public AK.Wwise.Event marioAlive;    // EVT_MarioAlive
	public AK.Wwise.Event marioDead;     // EVT_MarioDead

	[Header ("Wwise Sound Events")]
	public AK.Wwise.Event oneUpSound;
	public AK.Wwise.Event bowserFallSound;
	public AK.Wwise.Event bowserFireSound;
	public AK.Wwise.Event breakBlockSound;
	public AK.Wwise.Event bumpSound;
	public AK.Wwise.Event coinSound;
	public AK.Wwise.Event deadSound;
	public AK.Wwise.Event fireballSound;
	public AK.Wwise.Event flagpoleSound;
	public AK.Wwise.Event jumpSmallSound;
	public AK.Wwise.Event jumpSuperSound;
	public AK.Wwise.Event kickSound;
	public AK.Wwise.Event pipePowerdownSound;
	public AK.Wwise.Event powerupSound;
	public AK.Wwise.Event powerupAppearSound;
	public AK.Wwise.Event stompSound;
	public AK.Wwise.Event warningSound;
	public AK.Wwise.Event pauseSound;    // played on both pause and unpause, as the original did

	/* Wwise Events don't expose their length to C# the way an AudioClip does, so the
	 * few places that used clip.length to time a coroutine use these instead. Defaults
	 * are measured from the original files in Assets/Sounds - retune them by ear if the
	 * authored Events end up a different length. */
	[Header ("Wwise SFX timing (seconds)")]
	public float deadSoundDuration = 3.72f;
	public float warningSoundDuration = 2.93f;
	public float pauseSoundDuration = .69f;
	public float flagpoleSoundDuration = 1.17f;
	public float levelCompleteMusicDuration = 7.82f;
	public float castleCompleteMusicDuration = 9.05f;

	[Header ("Wwise RTPC")]
	public AK.Wwise.RTPC RTPC_TimeLeft;
	public AK.Wwise.RTPC RTPC_MusicVolume; // driven from the Main Menu volume sliders
	public AK.Wwise.RTPC RTPC_SoundVolume;

	[Header ("Wwise States")]
	public AK.Wwise.State ST_MarioSmall;
	public AK.Wwise.State ST_MarioLarge;
	public AK.Wwise.State ST_MarioStar;
	public AK.Wwise.State ST_CurrentLevel; // ST_Level_101..104 - set per level scene

	// Whichever music cue is currently playing, so it can be stopped/paused/resumed.
	private AK.Wwise.Event currentMusicEvent;


	void Awake() {
		Time.timeScale = 1;
	}

	// Use this for initialization
	void Start () {
		t_GameStateManager = GameStateManager.GetOrCreate (this);
		RetrieveGameState ();

		mario = FindObjectOfType<Mario> ();
		mario_Animator = mario.gameObject.GetComponent<Animator> ();
		mario_Rigidbody2D = mario.gameObject.GetComponent<Rigidbody2D> ();
		mario.UpdateSize ();

		// Volume lives in Wwise now: the Main Menu sliders still write these PlayerPrefs,
		// we just push them into RTPCs instead of into AudioSource.volume.
		ApplyVolumeSettings ();

		// Music: tell Wwise which level this is, start the music system, and pick the cue.
		UpdateMarioSizeState ();
		ST_CurrentLevel.SetValue ();
		/* Start the music system if nothing has yet. Coming from the Main Menu it is already
		 * playing on the persistent GameStateManager, so posting again would stack a second
		 * copy of MUS_MainSwitch on top. Posted on the GameStateManager's music object rather
		 * than on this one so it survives the next scene load, and so PauseMusic/StopMusic can
		 * act on it - a Wwise Event is scoped to the game object it was posted on. */
		if (!t_GameStateManager.musicStarted) {
			musicSource.Post (t_GameStateManager.MusicGameObject);
			t_GameStateManager.musicStarted = true;
		}
		marioAlive.Post (gameObject);

		// HUD
		SetHudCoin ();
		SetHudScore ();
		SetHudTime ();
		ChangeLevelMusicEvent ();

		Debug.Log (this.name + " Start: current scene is " + SceneManager.GetActiveScene ().name);
	}

	void RetrieveGameState() {
		marioSize = t_GameStateManager.marioSize;
		lives = t_GameStateManager.lives;
		coins = t_GameStateManager.coins;
		scores = t_GameStateManager.scores;
		timeLeft = t_GameStateManager.timeLeft;
		hurryUp = t_GameStateManager.hurryUp;


	}

	void ApplyVolumeSettings() {
		// Sliders are 0..1; Wwise RTPCs for volume are typically authored 0..100.
		RTPC_MusicVolume.SetGlobalValue (PlayerPrefs.GetFloat ("musicVolume", 1) * 100f);
		RTPC_SoundVolume.SetGlobalValue (PlayerPrefs.GetFloat ("soundVolume", 1) * 100f);
	}


	/****************** Timer */
	void Update() {
		if (!timerPaused) {
			timeLeft -= Time.deltaTime / .4f; // 1 game sec ~ 0.4 real time sec
			SetHudTime ();
			RTPC_TimeLeft.SetValue(gameObject, timeLeft);
		}

		if (timeLeftInt < 100 && !hurryUp) {
			hurryUp = true;
			PauseMusicPlaySoundEvent (warningSound, warningSoundDuration, true);
			if (isInvincibleStarman) {
				ChangeMusicEvent (starmanMusicHurry, warningSoundDuration);
			} else {
				ChangeLevelMusicEvent (warningSoundDuration);
			}
		}

		if (timeLeftInt <= 0) {
			MarioRespawn (true);
		}

		if (Input.GetButtonDown ("Pause")) {
			if (!gamePaused) {
				StartCoroutine (PauseGameCo ());
			} else {
				StartCoroutine (UnpauseGameCo ());
			}
		}
	}

	/****************** Game pause */
	List<Animator> unscaledAnimators = new List<Animator> ();
	float pauseGamePrevTimeScale;
	bool pausePrevMusicPaused;

	IEnumerator PauseGameCo() {
		gamePaused = true;
		pauseGamePrevTimeScale = Time.timeScale;

		Time.timeScale = 0;
		pausePrevMusicPaused = musicPaused;
		PauseMusic ();
		musicPaused = true;

		// Set any active animators that use unscaled time mode to normal
		unscaledAnimators.Clear();
		foreach (Animator animator in FindObjectsOfType<Animator>()) {
			if (animator.updateMode == AnimatorUpdateMode.UnscaledTime) {
				unscaledAnimators.Add (animator);
				animator.updateMode = AnimatorUpdateMode.Normal;
			}
		}

		pauseSound.Post (gameObject);
		yield return new WaitForSecondsRealtime (pauseSoundDuration);
		Debug.Log (this.name + " PauseGameCo stops: records prevTimeScale=" + pauseGamePrevTimeScale.ToString());
	}

	IEnumerator UnpauseGameCo() {
		pauseSound.Post (gameObject);
		yield return new WaitForSecondsRealtime (pauseSoundDuration);

		musicPaused = pausePrevMusicPaused;
		if (!musicPaused) {
			ResumeMusic ();
		}

		// Reset animators
		foreach (Animator animator in unscaledAnimators) {
			animator.updateMode = AnimatorUpdateMode.UnscaledTime;
		}
		unscaledAnimators.Clear ();

		Time.timeScale = pauseGamePrevTimeScale;
		gamePaused = false;
		Debug.Log (this.name + " UnpauseGameCo stops: resume prevTimeScale=" + pauseGamePrevTimeScale.ToString());
	}


	/****************** Invincibility */
	public bool isInvincible() {
		return isInvinciblePowerdown || isInvincibleStarman;
	}

	public void MarioInvincibleStarman() {
		StartCoroutine (MarioInvincibleStarmanCo ());
		AddScore (starmanBonus, mario.transform.position);
	}

	IEnumerator MarioInvincibleStarmanCo() {
		isInvincibleStarman = true;
		mario_Animator.SetBool ("isInvincibleStarman", true);
		mario.gameObject.layer = LayerMask.NameToLayer ("Mario After Starman");
		ST_MarioStar.SetValue ();
		if (hurryUp) {
			ChangeMusicEvent (starmanMusicHurry);
		} else {
			ChangeMusicEvent (starmanMusic);
		}
		yield return new WaitForSeconds (MarioInvincibleStarmanDuration);
		isInvincibleStarman = false;
		mario_Animator.SetBool ("isInvincibleStarman", false);
		mario.gameObject.layer = LayerMask.NameToLayer ("Mario");
		UpdateMarioSizeState ();
		ChangeLevelMusicEvent ();
	}

	void MarioInvinciblePowerdown() {
		StartCoroutine (MarioInvinciblePowerdownCo ());
	}

	IEnumerator MarioInvinciblePowerdownCo() {
		isInvinciblePowerdown = true;
		mario_Animator.SetBool ("isInvinciblePowerdown", true);
		mario.gameObject.layer = LayerMask.NameToLayer ("Mario After Powerdown");
		yield return new WaitForSeconds (MarioInvinciblePowerdownDuration);
		isInvinciblePowerdown = false;
		mario_Animator.SetBool ("isInvinciblePowerdown", false);
		mario.gameObject.layer = LayerMask.NameToLayer ("Mario");
	}


	/****************** Powerup / Powerdown / Die */
	public void MarioPowerUp() {
		powerupSound.Post(gameObject);
		if (marioSize < 2) {
			StartCoroutine (MarioPowerUpCo ());
		}
		AddScore (powerupBonus, mario.transform.position);
	}

	IEnumerator MarioPowerUpCo() {
		ST_MarioLarge.SetValue();
		mario_Animator.SetBool ("isPoweringUp", true);
		Time.timeScale = 0f;
		mario_Animator.updateMode = AnimatorUpdateMode.UnscaledTime;

		yield return new WaitForSecondsRealtime (transformDuration);
		yield return new WaitWhile(() => gamePaused);

		Time.timeScale = 1;
		mario_Animator.updateMode = AnimatorUpdateMode.Normal;

		marioSize++;
		mario.UpdateSize ();
		mario_Animator.SetBool ("isPoweringUp", false);
		Debug.Log ("Mario is yuge");

	}

	public void MarioPowerDown() {
		if (!isPoweringDown) {
			Debug.Log (this.name + " MarioPowerDown: called and executed");
			isPoweringDown = true;

			if (marioSize > 0) {
				StartCoroutine (MarioPowerDownCo ());
				pipePowerdownSound.Post(gameObject);
			} else {
				MarioRespawn ();
			}
			Debug.Log (this.name + " MarioPowerDown: done executing");
		} else {
			Debug.Log (this.name + " MarioPowerDown: called but not executed");
		}
	}

	IEnumerator MarioPowerDownCo() {
		ST_MarioSmall.SetValue();
		mario_Animator.SetBool ("isPoweringDown", true);
		Time.timeScale = 0f;
		mario_Animator.updateMode = AnimatorUpdateMode.UnscaledTime;

		yield return new WaitForSecondsRealtime (transformDuration);
		yield return new WaitWhile(() => gamePaused);

		Time.timeScale = 1;
		mario_Animator.updateMode = AnimatorUpdateMode.Normal;
		MarioInvinciblePowerdown ();

		marioSize = 0;
		mario.UpdateSize ();
		mario_Animator.SetBool ("isPoweringDown", false);
		isPoweringDown = false;

	}

	public void MarioRespawn(bool timeup = false) {
		if (!isRespawning) {
			isRespawning = true;

			marioSize = 0;
			lives--;

			StopMusic ();
			musicPaused = true;
			deadSound.Post(gameObject);
			marioDead.Post(gameObject);

			Time.timeScale = 0f;
			mario.FreezeAndDie ();

			if (timeup) {
				Debug.Log(this.name + " MarioRespawn: called due to timeup");
			}
			Debug.Log (this.name + " MarioRespawn: lives left=" + lives.ToString ());

			if (lives > 0) {
				ReloadCurrentLevel (deadSoundDuration, timeup);
			} else {
				LoadGameOver (deadSoundDuration, timeup);
				Debug.Log(this.name + " MarioRespawn: all dead");
			}
		}
	}


	/****************** Kill enemy */
	public void MarioStompEnemy(Enemy enemy) {
		mario_Rigidbody2D.velocity = new Vector2 (mario_Rigidbody2D.velocity.x + stompBounceVelocity.x, stompBounceVelocity.y);
		enemy.StompedByMario ();
		stompSound.Post(gameObject);
		AddScore (enemy.stompBonus, enemy.gameObject.transform.position);
		Debug.Log (this.name + " MarioStompEnemy called on " + enemy.gameObject.name);
	}

	public void MarioStarmanTouchEnemy(Enemy enemy) {
		enemy.TouchedByStarmanMario ();
		kickSound.Post(gameObject);
		AddScore (enemy.starmanBonus, enemy.gameObject.transform.position);
		Debug.Log (this.name + " MarioStarmanTouchEnemy called on " + enemy.gameObject.name);
	}

	public void RollingShellTouchEnemy(Enemy enemy) {
		enemy.TouchedByRollingShell ();
		kickSound.Post(gameObject);
		AddScore (enemy.rollingShellBonus, enemy.gameObject.transform.position);
		Debug.Log (this.name + " RollingShellTouchEnemy called on " + enemy.gameObject.name);
	}

	public void BlockHitEnemy(Enemy enemy) {
		enemy.HitBelowByBlock ();
		AddScore (enemy.hitByBlockBonus, enemy.gameObject.transform.position);
		Debug.Log (this.name + " BlockHitEnemy called on " + enemy.gameObject.name);
	}

	public void FireballTouchEnemy(Enemy enemy) {
		enemy.HitByMarioFireball ();
		kickSound.Post(gameObject);
		AddScore (enemy.fireballBonus, enemy.gameObject.transform.position);
		Debug.Log (this.name + " FireballTouchEnemy called on " + enemy.gameObject.name);
	}

	/****************** Scene loading */
	void LoadSceneDelay(string sceneName, float delay = loadSceneDelay) {
		timerPaused = true;
		StartCoroutine (LoadSceneDelayCo (sceneName, delay));
	}

	IEnumerator LoadSceneDelayCo(string sceneName, float delay) {
		Debug.Log (this.name + " LoadSceneDelayCo: starts loading " + sceneName);

		float waited = 0;
		while (waited < delay) {
			if (!gamePaused) { // should not count delay while game paused
				waited += Time.unscaledDeltaTime;
			}
			yield return null;
		}
		yield return new WaitWhile (() => gamePaused);

		Debug.Log (this.name + " LoadSceneDelayCo: done loading " + sceneName);

		isRespawning = false;
		isPoweringDown = false;
		SceneManager.LoadScene (sceneName);
	}

	public void LoadNewLevel(string sceneName, float delay = loadSceneDelay) {
		t_GameStateManager.SaveGameState ();
		t_GameStateManager.ConfigNewLevel ();
		t_GameStateManager.sceneToLoad = sceneName;
		LoadSceneDelay ("Level Start Screen", delay);
	}

	public void LoadSceneCurrentLevel(string sceneName, float delay = loadSceneDelay) {
		t_GameStateManager.SaveGameState ();
		t_GameStateManager.ResetSpawnPosition (); // TODO
		LoadSceneDelay (sceneName, delay);
	}

	public void LoadSceneCurrentLevelSetSpawnPipe(string sceneName, int spawnPipeIdx, float delay = loadSceneDelay) {
		t_GameStateManager.SaveGameState ();
		t_GameStateManager.SetSpawnPipe (spawnPipeIdx);
		LoadSceneDelay (sceneName, delay);
		Debug.Log (this.name + " LoadSceneCurrentLevelSetSpawnPipe: supposed to load " + sceneName
			+ ", spawnPipeIdx=" + spawnPipeIdx.ToString () + "; actual GSM spawnFromPoint="
			+ t_GameStateManager.spawnFromPoint.ToString () + ", spawnPipeIdx="
			+ t_GameStateManager.spawnPipeIdx.ToString ());
	}

	public void ReloadCurrentLevel(float delay = loadSceneDelay, bool timeup = false) {
		t_GameStateManager.SaveGameState ();
		t_GameStateManager.ConfigReplayedLevel ();
		t_GameStateManager.sceneToLoad = SceneManager.GetActiveScene ().name;
		if (timeup) {
			LoadSceneDelay ("Time Up Screen", delay);
		} else {
			LoadSceneDelay ("Level Start Screen", delay);
		}
	}

	public void LoadGameOver(float delay = loadSceneDelay, bool timeup = false) {
		int currentHighScore = PlayerPrefs.GetInt ("highScore", 0);
		if (scores > currentHighScore) {
			PlayerPrefs.SetInt ("highScore", scores);
		}
		t_GameStateManager.timeup = timeup;
		LoadSceneDelay ("Game Over Screen", delay);
	}


	/****************** HUD */
	public void SetHudCoin() {
		coinText.text = "x" + coins.ToString ("D2");
	}

	public void SetHudScore() {
		scoreText.text = scores.ToString ("D6");
	}

	public void SetHudTime() {
		timeLeftInt = Mathf.RoundToInt (timeLeft);
		timeText.text = timeLeftInt.ToString ("D3");
	}

	public void CreateFloatingText(string text, Vector3 spawnPos) {
		GameObject textEffect = Instantiate (FloatingTextEffect, spawnPos, Quaternion.identity);
		textEffect.GetComponentInChildren<TextMesh> ().text = text.ToUpper ();
	}


	/****************** Music control
	 * Music actions go through the Wwise-Types API (Event.ExecuteAction) rather than
	 * raw AkSoundEngine calls. Both the music-system Event and the cue currently
	 * playing are acted on, so this works whether cue changes are driven by separate
	 * Events or by States inside one playlist. */
	void MusicAction(AkActionOnEventType action) {
		musicSource.ExecuteAction (t_GameStateManager.MusicGameObject, action, 0,
			AkCurveInterpolation.AkCurveInterpolation_Linear);
		if (currentMusicEvent != null && currentMusicEvent != musicSource) {
			currentMusicEvent.ExecuteAction (gameObject, action, 0, AkCurveInterpolation.AkCurveInterpolation_Linear);
		}
	}

	public void PauseMusic() {
		MusicAction (AkActionOnEventType.AkActionOnEventType_Pause);
	}

	public void ResumeMusic() {
		MusicAction (AkActionOnEventType.AkActionOnEventType_Resume);
	}

	public void StopMusic() {
		MusicAction (AkActionOnEventType.AkActionOnEventType_Stop);
	}

	/* Switch to this level's music cue, respecting the hurry-up variant.
	 *
	 * Which music a level plays is decided by the Levels State (ST_CurrentLevel, set in
	 * Start) and played by the main playlist - MUS_Levels_Sw picks MUS_Lvl101..104 from it.
	 * A per-level cue Event is therefore optional, and an empty levelMusic slot is a
	 * normal setup rather than a mistake, so don't warn about it here. Pick an Event in the
	 * Inspector if you want a level to post its own cue on top. */
	public void ChangeLevelMusicEvent(float delay = 0) {
		AK.Wwise.Event levelCue = hurryUp ? levelMusicHurry : levelMusic;
		if (levelCue != null && levelCue.IsValid ()) {
			ChangeMusicEvent (levelCue, delay);
		}
	}

	public void ChangeMusicEvent(AK.Wwise.Event newEvent, float delay = 0) {
		StartCoroutine (ChangeMusicEventCo (newEvent, delay));
	}

	IEnumerator ChangeMusicEventCo(AK.Wwise.Event newEvent, float delay) {
		yield return new WaitWhile (() => gamePaused);
		yield return new WaitForSecondsRealtime (delay);
		yield return new WaitWhile (() => gamePaused || musicPaused);

		if (isRespawning) {
			yield break;
		}

		if (newEvent == null || !newEvent.IsValid ()) {
			Debug.LogWarning (this.name + " ChangeMusicEventCo: music Event is not assigned in the Inspector");
			yield break;
		}

		// Stop the cue that's playing so the two don't stack.
		if (currentMusicEvent != null && currentMusicEvent.IsValid () && currentMusicEvent != newEvent) {
			currentMusicEvent.Stop (gameObject);
		}

		currentMusicEvent = newEvent;
		newEvent.Post (gameObject);
		Debug.Log (this.name + " ChangeMusicEventCo: music changed to " + newEvent.Name);
	}

	/* Pause the music, play a one-shot over it, then optionally bring the music back -
	 * used for the hurry-up warning and the flagpole. In Wwise this could equally be a
	 * ducking bus or a State; it's done here in code to match the original behaviour. */
	public void PauseMusicPlaySoundEvent(AK.Wwise.Event soundEvent, float duration, bool resumeMusic) {
		StartCoroutine (PauseMusicPlaySoundEventCo (soundEvent, duration, resumeMusic));
	}

	IEnumerator PauseMusicPlaySoundEventCo(AK.Wwise.Event soundEvent, float duration, bool resumeMusic) {
		musicPaused = true;
		PauseMusic ();
		soundEvent.Post (gameObject);

		yield return new WaitForSeconds (duration);

		if (resumeMusic) {
			ResumeMusic ();
		}
		musicPaused = false;
	}


	/****************** Game state */
	void UpdateMarioSizeState() {
		if (isInvincibleStarman) {
			ST_MarioStar.SetValue ();
		} else if (marioSize > 0) {
			ST_MarioLarge.SetValue ();
		} else {
			ST_MarioSmall.SetValue ();
		}
	}

	public void AddLife() {
		lives++;
		oneUpSound.Post(gameObject);
	}

	public void AddLife(Vector3 spawnPos) {
		lives++;
		oneUpSound.Post(gameObject);
		CreateFloatingText ("1UP", spawnPos);
	}

	public void AddCoin() {
		coins++;
		coinSound.Post(gameObject);
		if (coins == 100) {
			AddLife ();
			coins = 0;
		}
		SetHudCoin ();
		AddScore (coinBonus);
	}

	public void AddCoin(Vector3 spawnPos) {
		coins++;
		coinSound.Post(gameObject);
		if (coins == 100) {
			AddLife ();
			coins = 0;
		}
		SetHudCoin ();
		AddScore (coinBonus, spawnPos);
	}

	public void AddScore(int bonus) {
		scores += bonus;
		SetHudScore ();
	}

	public void AddScore(int bonus, Vector3 spawnPos) {
		scores += bonus;
		SetHudScore ();
		if (bonus > 0) {
			CreateFloatingText (bonus.ToString (), spawnPos);
		}
	}


	/****************** Misc */
	public Vector3 FindSpawnPosition() {
		Vector3 spawnPosition;
		GameStateManager t_GameStateManager = GameStateManager.GetOrCreate (this);
		Debug.Log (this.name + " FindSpawnPosition: GSM spawnFromPoint=" + t_GameStateManager.spawnFromPoint.ToString()
			+ " spawnPipeIdx= " + t_GameStateManager.spawnPipeIdx.ToString()
			+ " spawnPointIdx=" + t_GameStateManager.spawnPointIdx.ToString());
		if (t_GameStateManager.spawnFromPoint) {
			spawnPosition = GameObject.Find ("Spawn Points").transform.GetChild (t_GameStateManager.spawnPointIdx).transform.position;
		} else {
			spawnPosition = GameObject.Find ("Spawn Pipes").transform.GetChild (t_GameStateManager.spawnPipeIdx).transform.Find("Spawn Pos").transform.position;
		}
		return spawnPosition;
	}

	public string GetWorldName(string sceneName) {
		string[] sceneNameParts = Regex.Split (sceneName, " - ");
		return sceneNameParts[0];
	}

	public bool isSceneInCurrentWorld(string sceneName) {
		return GetWorldName (sceneName) == GetWorldName (SceneManager.GetActiveScene ().name);
	}

	public void MarioCompleteCastle() {
		timerPaused = true;
		ChangeMusicEvent (castleCompleteMusic);
		mario.AutomaticWalk(mario.castleWalkSpeedX);
	}

	public void MarioCompleteLevel() {
		timerPaused = true;
		ChangeMusicEvent (levelCompleteMusic);
	}

	public void MarioReachFlagPole() {
		timerPaused = true;
		PauseMusicPlaySoundEvent (flagpoleSound, flagpoleSoundDuration, false);
		mario.ClimbFlagPole ();
	}
}
