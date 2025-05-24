using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ScrollingImageUI : MonoBehaviour
{
	[SerializeField] Vector2 scrollSpeed;

	Image scrollingTextureImage;
	Vector2 offset;

	void Awake()
	{
		scrollingTextureImage = GetComponent<Image>();
	}

	void Update()
	{
		// scroll healthbar bg
		offset += scrollSpeed * Time.deltaTime;
		scrollingTextureImage.materialForRendering.SetTextureOffset("_MainTex", offset); // use materialForRendering because it's under a mask 
	}
}