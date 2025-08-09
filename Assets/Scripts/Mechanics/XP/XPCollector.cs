using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class IntUnityEvent : UnityEvent<int> { }

[RequireComponent(typeof(Collider2D))]
public class XPCollector : MonoBehaviour
{
	public IntUnityEvent OnCollectOrb = new IntUnityEvent();
	public bool canCollect = false;

	void OnTriggerStay2D(Collider2D collision)
	{
		if (canCollect)
		{
			XPOrb orb = collision.gameObject.GetComponent<XPOrb>();
			if (orb != null) orb.Collect(this);
		}
	}
}