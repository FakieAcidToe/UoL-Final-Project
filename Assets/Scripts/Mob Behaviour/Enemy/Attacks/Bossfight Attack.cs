using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[CreateAssetMenu(fileName = "Bossfight Attack", menuName = "Attack/Enemy Attacks/Bossfight")]
public class BossfightAttack : EnemyAttackGrid
{
	[Header("Prefabs")]
	//[SerializeField] Hitbox hitboxPrefab;
	[SerializeField] Enemy enemyPrefab;
	[SerializeField] EnemyStats[] enemiesToSpawn;
	//[Header("Audio")]
	//[SerializeField] AudioClip startSfx;
	//[SerializeField] AudioClip attackSfx;
	//[Header("Timings")]
	//[SerializeField, Min(0)] float attackDistance = 4f;
	//[SerializeField, Min(0)] float unchargeDistance = 0.8f;
	//[SerializeField, Min(0)] float chargeMinTime = 0.3f;
	//[SerializeField, Min(0)] float chargeMaxTime = 1.2f;

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
				int[] windows = {1, 2, 3};
				self.attack.SetWindow(windows[Random.Range(0, windows.Length)]);
				vars.windowTimer = 0;
				vars.hasAttacked = false;
				vars.spawnedGuy = false;
				break;
			case 1: // idle
				Debug.Log("Idle");
				if (vars.windowTimer > 2f)
					self.attack.SetWindow(0);
				break;
			case 2: // move
				Debug.Log("Move");
				self.SetMovement(self.pathfinding.PathfindToTarget());

				if (vars.windowTimer > 2f)
					self.attack.SetWindow(0);
				break;
			case 3: // spawn a guy
				Debug.Log("Spawn a Guy");
				if (!vars.spawnedGuy)
				{
					vars.spawnedGuy = true;
					SpawnEnemy(self, self.transform.position);
				}
				if (vars.windowTimer > 10f)
					self.attack.SetWindow(0);
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