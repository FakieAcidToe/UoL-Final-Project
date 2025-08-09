using UnityEngine;
using UnityEngine.UI;

public class DamageNumberCanvas : MonoBehaviour
{
	[SerializeField] Text text;
	[SerializeField, Min(0)] float fadeOutRate = 1f;
	[SerializeField] Vector2 speed;

	float opacity = 1f;

	void Update()
	{
		opacity -= fadeOutRate * Time.deltaTime;
		text.rectTransform.position += (Vector3)speed * Time.deltaTime * opacity;

		if (opacity > 0)
			text.color = new Color(text.color.r, text.color.g, text.color.b, opacity);
		else
			Destroy(gameObject);
	}

	public void SetDamageNumberText(int damageNumber)
	{
		text.text = damageNumber.ToString();
	}
}