using UnityEngine;


/* What a piece of level geometry is made of, for Wwise.
 *
 * Put this on a ground / brick / pipe / castle-floor prefab and pick the matching value
 * of a Surface Switch group. Mario looks it up on the collider he lands on and applies
 * it before posting the landing Event, so one Event covers every surface in the game
 * instead of needing one Event per material.
 *
 * This is the Switch lesson in miniature: the Event stays the same, the Switch decides
 * which sound comes out. Leave it off a prefab and the Switch simply isn't changed, so
 * the surface keeps whatever value was last set - give the Switch group a sensible
 * default in Wwise.
 */
public class SoundMaterial : MonoBehaviour {
	public AK.Wwise.Switch surface;

	public void ApplyTo(GameObject audioGameObject) {
		surface.SetValue (audioGameObject);
	}

	/* Convenience for callers that have a collider rather than the object itself - the
	 * component is usually on the prefab root while the collider is on a child. */
	public static void ApplyFrom(Component hit, GameObject audioGameObject) {
		if (hit == null) {
			return;
		}
		SoundMaterial material = hit.GetComponentInParent<SoundMaterial> ();
		if (material != null) {
			material.ApplyTo (audioGameObject);
		}
	}
}
