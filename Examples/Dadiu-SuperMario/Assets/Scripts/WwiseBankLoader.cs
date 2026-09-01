using UnityEngine;


/* Loads the game's SoundBank no matter which scene you press Play on.
 *
 * BNK_Main is loaded by an AkBank component that only exists in the Main Menu scene, so
 * entering the game the normal way (Main Menu -> Level Start Screen -> a level) has the
 * bank in memory before anything posts an Event. Pressing Play directly on a level scene
 * (Test Scene, World 1-1, ...) skips the Main Menu, nothing ever loads the bank, and every
 * Post fails with:
 *
 *     WwiseUnity: Could not post event (name: ..., ID: ...).
 *     Please make sure to load or rebuild the appropriate SoundBank.
 *
 * The bank has to be loaded very early. Components post Events from Awake as well as from
 * Start - the AkAmbient on the Firebar prefab in World 1-4 is one - so loading after the
 * scene has finished loading is already too late for those. Wwise's own hook is used
 * instead: AkSoundEngineInitialization fires initializationDelegate at the end of
 * InitializeSoundEngine(), which happens in AkInitializer's OnEnable (script execution
 * order -100), before any AkBank (-75), AkEvent or AkAmbient (0) has run.
 *
 * AkBankManager reference-counts bank loads, so the Main Menu's AkBank component keeps
 * working exactly as before.
 *
 * If you add more SoundBanks in Wwise, add their names to BankNames.
 */
public static class WwiseBankLoader {
	/* Must match the SoundBank names in the Wwise project (SoundBanks work unit).
	 * Init.bnk is loaded by the sound engine itself and does not belong here. */
	public static readonly string[] BankNames = { "BNK_Main" };

	private static bool banksLoaded;

	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.BeforeSceneLoad)]
	private static void Install() {
		banksLoaded = false;

		/* -= first: the delegate lives on a static singleton, so it can survive from a
		 * previous play session when Enter Play Mode reload is turned off. */
		AkSoundEngineInitialization.Instance.initializationDelegate -= LoadBanks;
		AkSoundEngineInitialization.Instance.initializationDelegate += LoadBanks;

		// If the sound engine was already up before this ran, the delegate won't fire.
		if (AkSoundEngine.IsInitialized ()) {
			LoadBanks ();
		}
	}

	[RuntimeInitializeOnLoadMethod (RuntimeInitializeLoadType.AfterSceneLoad)]
	private static void VerifyBanksLoaded() {
		if (banksLoaded) {
			return;
		}

		if (AkSoundEngine.IsInitialized ()) {
			LoadBanks (); // last resort - Events posted from Awake have already failed by now
		} else {
			/* No sound engine means no WwiseGlobal object in the scene. Say so rather than
			 * letting the scene run silently with every Post failing. */
			Debug.LogWarning ("WwiseBankLoader: the Wwise sound engine is not initialised, so no "
				+ "SoundBank was loaded. Check that this scene has a WwiseGlobal object.");
		}
	}

	private static void LoadBanks() {
		foreach (string bankName in BankNames) {
			AkBankManager.LoadBank (bankName, false, false);
		}
		banksLoaded = true;
	}
}
