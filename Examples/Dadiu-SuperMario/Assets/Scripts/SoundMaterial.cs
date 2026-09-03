using UnityEngine;


/* What a piece of level geometry is made of, for Wwise.
 *
 * Put this on a ground / brick / pipe / castle-floor prefab and pick the matching value
 * of a Surface Switch group. Mario looks it up on the collider he lands on and applies
 * it before posting the landing Event, so one Event covers every surface in the game
 * instead of needing one Event per material.
 *
 * This is the Switch lesson in miniature: the Event stays the same, the Switch decides
 * which sound comes out. Leave it off a prefab and the Level Manager's swSurfaceDefault
 * is used instead, so an un-tagged surface still lands on a deliberate sound.
 */
public class SoundMaterial : MonoBehaviour {
	public AK.Wwise.Switch surface;

	public void ApplyTo(GameObject audioGameObject) {
		surface.SetValue (audioGameObject);
	}

	/* Convenience for callers that have a collider rather than the object itself - the
	 * component is usually on the prefab root while the collider is on a child.
	 *
	 * fallback is used when the thing hit carries no SoundMaterial, so an un-tagged
	 * prefab lands on a known surface instead of silently reusing the last one set. */
	public static void ApplyFrom(Component hit, GameObject audioGameObject,
		AK.Wwise.Switch fallback = null) {
		SoundMaterial material = hit != null
			? hit.GetComponentInParent<SoundMaterial> ()
			: null;
		if (material != null) {
			material.ApplyTo (audioGameObject);
		} else if (fallback != null && fallback.IsValid ()) {
			fallback.SetValue (audioGameObject);
		}
	}
}
