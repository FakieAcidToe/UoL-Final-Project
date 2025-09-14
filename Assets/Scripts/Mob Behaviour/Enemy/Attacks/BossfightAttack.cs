using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Bossfight Attack", menuName = "Attack/Enemy Attacks/Bossfight")]
public class BossfightAttack : EnemyAttackGrid
{
	[Header("Prefabs")]
	//[SerializeField] Hitbox hitboxPrefab;
	[SerializeField] Enemy enemyPrefab;
	[SerializeField] EnemyStats[] enemiesToSpawn;

	[Header("Random Windows")]
	[SerializeField] int[] windowsToPick = { 1, 2, 3 };

	//[Header("Audio")]
	//[SerializeField] AudioClip startSfx;
	//[SerializeField] AudioClip attackSfx;

	[Header("Timings")]
	[SerializeField, Min(0)] float moveTime = 2f;
	[SerializeField, Min(0)] float idleTime = 2f;
	[SerializeField, Min(0)] float spawnAGuyChargeTime = 1f;
	[SerializeField, Min(0)] float spawnAGuyChargeSpawnTime = 0.12f;

	// EVERY enemy of the same species share the same script and variables. we need to handle this.
	Dictionary<Enemy, UniqueVariables> varsDict = new Dictionary<Enemy, UniqueVariables>();
	struct UniqueVariables
	{
		public bool hasAttacked;
		public bool spawnedGuy;
		public Hitbox hbox;
		public float windowTimer;
	}

	List<Enemy> enemyObjs = new List<Enemy>();

	// runs when starting an attack
	public override void AttackStart(Enemy self)
	{
		UniqueVariables vars = new UniqueVariables();

		vars.hasAttacked = false;
		vars.spawnedGuy = false;

		//self.PlaySFX(startSfx);

		if (varsDict.ContainsKey(self)) varsDict[self] = vars;
		else varsDict.Add(self, vars);
	}

	// runs every frame of the attack
	public override void AttackUpdate(Enemy self, int window, float windowTimer, float chargeTimer)
	{
		if (!varsDict.ContainsKey(self)) return;
		UniqueVariables vars = varsDict[self];

		vars.windowTimer += Time.deltaTime;

		switch (window)
		{
			case 0: // window picker
				self.attack.SetWindow(windowsToPick[Random.Range(0, windowsToPick.Length)]);
				vars.windowTimer = 0;
				vars.hasAttacked = false;
				vars.spawnedGuy = false;
				break;
			case 1: // idle
				Debug.Log("Idle");
				if (vars.windowTimer > idleTime)
					self.attack.SetWindow(0);
				break;
			case 2: // move
				Debug.Log("Move");
				self.SetMovement(self.pathfinding.PathfindToTarget());

				if (vars.windowTimer > moveTime)
					self.attack.SetWindow(0);
				break;
			case 3: // spawn a guy 1 curl up
				Debug.Log("Spawn a guy");
				break;
			case 4: // spawn a guy 2 loop curl
				if (vars.windowTimer > spawnAGuyChargeTime)
					self.attack.SetWindow(5);
				break;
			// case 5: // spawn a guy 3 point up
			case 6: // spawn a guy 4 point down
				if (!vars.spawnedGuy && vars.windowTimer > spawnAGuyChargeSpawnTime)
				{
					vars.spawnedGuy = true;
					SpawnEnemy(self, self.transform.position);
				}
				//if (vars.windowTimer > 10f)
				//	self.attack.SetWindow(0);
				break;

			//case 4: // hit
			//	if (!vars.hasAttacked)
			//	{
			//		// spawn hitbox
			//		Hitbox hbox = Instantiate(hitboxPrefab, self.transform.position, Quaternion.identity, self.transform);
			//		hbox.SetDirection(vars.direction);
			//		hbox.owner = self;
			//		vars.hbox = hbox;
			//
			//		vars.hasAttacked = true;
			//	}
			//	break;
		}

		varsDict[self] = vars;
	}

	// runs when an attack ends or gets interrupted
	public override void AttackEnd(Enemy self)
	{
		if (!varsDict.ContainsKey(self)) return;
		UniqueVariables vars = varsDict[self];

		if (vars.hbox != null)
			Destroy(vars.hbox.gameObject);
		vars.hbox = null;

		varsDict.Remove(self);
	}

	// when should the cpu begin its attack?
	public override bool ShouldAttack(Enemy self) { return true; }

	Enemy SpawnEnemy(Enemy self, Vector2 location)
	{
		Enemy enemy = Instantiate(enemyPrefab, location, Quaternion.identity);
		enemyObjs.Add(enemy);

		if (enemiesToSpawn != null && enemiesToSpawn.Length > 0)
		{
			enemy.stats = enemiesToSpawn[Random.Range(0, enemiesToSpawn.Length)];
			enemy.name = enemy.stats.enemyName + "(Clone)";
		}

		enemy.playerStats = self.playerStats;
		enemy.target = self.target;
		enemy.level = self.level;

		enemy.pathfinding.tiles = self.pathfinding.tiles;
		enemy.pathfinding.mapOffset = self.pathfinding.mapOffset;
		enemy.pathfinding.neighborCache = self.pathfinding.neighborCache;

		enemy.health.healthbarUIMonster = self.health.healthbarUIMonster;
		enemy.health.healthbarUIPlayer = self.health.healthbarUIPlayer;

		return enemy;
	}
}