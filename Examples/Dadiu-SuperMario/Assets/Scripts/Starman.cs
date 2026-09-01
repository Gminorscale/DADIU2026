using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Starman : MonoBehaviour {
	private LevelManager t_LevelManager;

	// Use this for initialization
	void Start () {
		t_LevelManager = FindObjectOfType<LevelManager> ();
		t_LevelManager.WwpowerupAppearSound.Post (t_LevelManager.gameObject);
	}
	
	void OnCollisionEnter2D(Collision2D other) {
		if (other.gameObject.tag == "Player") {
			t_LevelManager.MarioInvincibleStarman ();
			t_LevelManager.WwpowerupSound.Post (t_LevelManager.gameObject);
			Destroy (gameObject);
		}
	}
}
