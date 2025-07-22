using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : Hitbox
{
	[Header("Projectile Properties")]
	[SerializeField, Tooltip("Projectile movement speed")]
	float speed = 1f;

	Rigidbody2D rb;

	protected override void Awake()
	{
		base.Awake();

		rb = GetComponent<Rigidbody2D>();
	}

	void FixedUpdate()
	{
		if (speed != 0)
			rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
	}
}