using System;
using System.Collections;
using UnityEngine;

public class XPOrbDropper : MonoBehaviour
{
	[SerializeField] XPOrb orbPrefab;
	[SerializeField, Range(0, 1)] float chance = 0.2f;

	public void DropXPOrbs(int dropAmount = 1)
	{
		for (int i = 0; i < dropAmount; ++i)
		{
			XPOrb orb = Instantiate(orbPrefab, transform.position, Quaternion.identity);
			// choose xp orb type if have enough remaining drops
			IList list = Enum.GetValues(typeof(XPOrb.XPOrbType));
			for (int j = list.Count - 1; j >= 0; --j)
			{
				XPOrb.XPOrbType type = (XPOrb.XPOrbType)list[j];
				if (i + (int)type - 1 < dropAmount)
				{
					orb.SetXpWorth(type);
					i += (int)type - 1;
					break;
				}
			}
		}
	}

	public void ChanceDropXPOrbs(int dropAmount = 1)
	{
		if (UnityEngine.Random.value < chance)
			DropXPOrbs(dropAmount);
	}
}