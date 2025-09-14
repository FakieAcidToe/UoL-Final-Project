using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "Bossfight Attack", menuName = "Attack/Enemy Attacks/Bossfight")]
public class BossfightAttack : EnemyAttackGrid
{
	[Header("Enemy Spawning")]
	//[SerializeField] Hitbox hitboxPrefab;
	[SerializeField] Enemy enemyPrefab;
	[SerializeField] EnemyStats[] enemiesToSpawn;
	[SerializeField, Min(0)] float enemySpawnDist = 1f;
	[SerializeField, Min(0)] int numEnemiesOut = 3;

	[Header("Audio")]
	[SerializeField] AudioClip swooshSfx;

	[Header("Attack Distances")]
	[SerializeField, Min(0)] float minSpawnDistance = 6f;

	[Header("Timings")]
	[SerializeField, Min(0)] float moveTime = 2f;
	[SerializeField, Min(0)] float idleTime = 2f;
	[SerializeField, Min(0)] float spawnAGuyChargeTime = 1f;
	[SerializeField, Min(0)] float spawnAGuyChargeSpawnTime = 0.12f;

	// EVERY enemy of the same species share the same script and variables. we need to handle this.
	Dictionary<Enemy, UniqueVariables> varsDict = new Dictionary<Enemy, UniqueVariables>();
	struct UniqueVariables // DONT DELETE at the end of attack
	{
		public float windowTimer;
		public bool hasAttacked;
		public bool spawnedGuy;
		public Hitbox hbox;
		public List<Enemy> enemyObjs;
		public Vector2Int targetTile;
		public bool hasTargetedTile;
	}

	// runs when starting an attack
	public override void AttackStart(Enemy self)
	{
		UniqueVariables vars = varsDict.ContainsKey(self) ? varsDict[self] : new UniqueVariables();

		vars.windowTimer = 0;
		vars.hasAttacked = false;
		vars.spawnedGuy = false;
		vars.hasTargetedTile = false;
		if (vars.enemyObjs == null) vars.enemyObjs = new List<Enemy>();

		//self.PlaySFX(startSfx);

		if (varsDict.ContainsKey(self)) varsDict[self] = vars;
		else varsDict.Add(self, vars);
	}

	int WindowPicker(ref UniqueVariables vars, Enemy self)
	{
		// update enemyObjs
		int numEnemies = 0;
		for (int i = vars.enemyObjs.Count - 1; i >= 0; --i)
		{
			if (!vars.enemyObjs[i].gameObject.activeSelf)
			{
				Destroy(vars.enemyObjs[i].gameObject);
				vars.enemyObjs.RemoveAt(i);
			}
			else if (!vars.enemyObjs[i].IsBeingControlledByPlayer()) ++numEnemies;
		}

		bool farAway = (self.transform.position - self.target.transform.position).magnitude >= minSpawnDistance;

		if (numEnemies < numEnemiesOut && farAway)
			return 3; // spawn a guy
		else if (!farAway)
			return 2; // move

		return 1; // idle
	}

	void SetWindow(ref UniqueVariables vars, Enemy self, int newWindow)
	{
		self.attack.SetWindow(newWindow);
		vars.windowTimer = 0;
		vars.hasAttacked = false;
		vars.spawnedGuy = false;
		vars.hasTargetedTile = false;
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
				SetWindow(ref vars, self, WindowPicker(ref vars, self));
				break;
			case 1: // idle
				if (vars.windowTimer > idleTime)
					self.attack.SetWindow(0);
				break;
			case 2: // move
				//self.SetMovement(self.pathfinding.PathfindToTarget());
				if (!vars.hasTargetedTile)
				{
					HashSet<Vector2Int> tiles = self.pathfinding.tiles;
					vars.targetTile = tiles.ElementAt(Random.Range(0, tiles.Count));
					vars.hasTargetedTile = true;
				}
				self.SetMovement((vars.targetTile - (Vector2)self.transform.position).normalized);

				if (vars.windowTimer > moveTime || Vector2Int.RoundToInt((Vector2)self.transform.position + self.pathfinding.mapOffset) == vars.targetTile)
					self.attack.SetWindow(0);
				break;
			//case 3: // spawn a guy 1 curl up
			case 4: // spawn a guy 2 loop curl
				if (vars.windowTimer > spawnAGuyChargeTime)
					self.attack.SetWindow(5);
				break;
			// case 5: // spawn a guy 3 point up
			case 6: // spawn a guy 4 point down
				if (!vars.spawnedGuy && vars.windowTimer > spawnAGuyChargeSpawnTime)
				{
					vars.spawnedGuy = true;
					self.PlaySFX(swooshSfx);
					Vector3 spawnPos = self.transform.position + enemySpawnDist * (self.target.transform.position - self.transform.position).normalized;
					SpawnEnemy(ref vars, self, spawnPos);
				}
				break;
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

		// DONT DELETE at the end of attack
		//varsDict.Remove(self);
	}


	public override void AttackDie(Enemy self)
	{
		if (!varsDict.ContainsKey(self)) return;
		UniqueVariables vars = varsDict[self];

		// kill all enemies
		for (int i = vars.enemyObjs.Count - 1; i >= 0; --i)
			if (!vars.enemyObjs[i].IsBeingControlledByPlayer())
				vars.enemyObjs[i].Kill();

		varsDict.Remove(self);
	}

	// when should the cpu begin its attack?
	public override bool ShouldAttack(Enemy self) { return true; }

	Enemy SpawnEnemy(ref UniqueVariables vars, Enemy self, Vector2 location)
	{
		Enemy enemy = Instantiate(enemyPrefab, location, Quaternion.identity);
		vars.enemyObjs.Add(enemy);

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