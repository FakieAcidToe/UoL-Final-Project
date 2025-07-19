using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
public class EnemyAttack : MonoBehaviour
{


	Enemy enemy;

	// Start is called before the first frame update
	void Awake()
	{
		enemy = GetComponent<Enemy>();
	}

	// Update is called once per frame
	void Update()
	{
		
	}
}