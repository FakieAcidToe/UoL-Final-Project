using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(Rigidbody2D))]
public class Projectile : Hitbox
{
	[Header("Projectile Properties")]
	[SerializeField, Tooltip("Projectile movement speed")]
	float speed = 1f;
	[SerializeField, Tooltip("Projectile destroys self when colliding with tilemap walls")]
	WallBehaviour wallCollisionBehaviour = WallBehaviour.destroySelf;

	Rigidbody2D rb;

	enum WallBehaviour
	{
		none,
		destroySelf
	}

	protected override void Awake()
	{
		base.Awake();

		rb = GetComponent<Rigidbody2D>();
	}

	public override void SetDirection(Vector2 _direction)
	{
		base.SetDirection(_direction);
		transform.localRotation =  Quaternion.Euler(0f, 0f, Mathf.Atan2(_direction.y, _direction.x) * Mathf.Rad2Deg);
	}

	void FixedUpdate()
	{
		if (speed != 0)
			rb.MovePosition(rb.position + direction * speed * Time.fixedDeltaTime);
	}
	protected override void CollisionDetected(GameObject collisionGO)
	{
		base.CollisionDetected(collisionGO);

		if (wallCollisionBehaviour == WallBehaviour.destroySelf)
		{
			TilemapCollider2D tilemap = collisionGO.GetComponent<TilemapCollider2D>();
			if (tilemap != null)
				Destroy();
		}
	}
}