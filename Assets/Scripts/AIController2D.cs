using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class AIController2D : MonoBehaviour, IDamagable
{
	[SerializeField] Animator animator;
	[SerializeField] SpriteRenderer spriteRenderer;

	[SerializeField] float speed;
	[SerializeField] float jumpHeight;
	[SerializeField] float doubleJumpHeight;
	[SerializeField, Range(1, 5)] float fallRateMultiplier;
	[SerializeField, Range(1, 5)] float lowJumpRateMultiplier;
	[Header("Ground")]
	[SerializeField] Transform groundTransform;
	[SerializeField] LayerMask groundLayerMask;
	[SerializeField] float groundRadius;
	[Header("AI")]
	[SerializeField] Transform[] waypoints;
	[SerializeField] float sightDistance = 1;
	[SerializeField] string enemyTag;
	[SerializeField] LayerMask raycastLayerMask;

	public float health = 100;

	Rigidbody2D rb;
	Vector2 velocity = Vector2.zero;
	bool faceRight = true;
	float groundAngle = 0;
	Transform targetWaypoint = null;
	GameObject enemy = null;

	enum State
	{ 
		IDLE,
		PATROL,
		CHASE,
		ATTACK,
		DEAD
	}

	State state = State.IDLE;
	float stateTimer = 1;

	void Start()
	{
		rb = GetComponent<Rigidbody2D>();
	}

	void Update()
	{
		CheckEnemySeen();
		Vector2 direction = Vector2.zero;

		// update ai
		switch (state)
		{
			case State.IDLE:
				if (enemy != null) state = State.CHASE; // go to chase state if enemy has been seen
				stateTimer -= Time.deltaTime;
				if (stateTimer <= 0)
				{
					SetNewWaypointTarget(); // select a waypoint after 
					state = State.PATROL;
				}
				break;
			case State.PATROL:
				{ 
					if (enemy != null) state = State.CHASE; // go to chase state if enemy has been seen
					direction.x = Mathf.Sign(targetWaypoint.position.x - transform.position.x); // move to waypoint location
					float dx = Mathf.Abs(targetWaypoint.position.x - transform.position.x); 
					if (dx <= 0.25f) // once at a waypoint
					{
						stateTimer = 1; // go back to idle state
						state = State.IDLE;
					}
				}
				break;
			case State.CHASE:
				{
					if (enemy == null) // if enemy is no longer seen
					{
						state = State.IDLE; // go to idle state
						stateTimer = 1;
						break;
					}
					float dx = Mathf.Abs(enemy.transform.position.x - transform.position.x);
					if (dx <= 1f) // if we are close to the enemy
					{
						state = State.ATTACK; // attack the enemy
						animator.SetTrigger("Attack");
					}
					else // otherwise go to the enemy
					{
						direction.x = Mathf.Sign(enemy.transform.position.x - transform.position.x);
					}
				}
				break;
			case State.ATTACK:
				if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1 && !animator.IsInTransition(0))
				{
					state = State.CHASE; // once the animation is done, go back to chase state
				}
				break;
			case State.DEAD:
				animator.SetTrigger("Death"); // if health is zero start death animation
				break;
			default:
				break;
		}

		// check health
		if (health <= 0)
		{
			state = State.DEAD;
		}

		// check if the character is on the ground
		bool onGround = Physics2D.OverlapCircle(groundTransform.position, groundRadius, groundLayerMask) != null;

		
		velocity.x = direction.x * speed;

		// set velocity
		if (onGround)
		{
			if (velocity.y < 0) velocity.y = 0;
		}
		float gravityMultiplier = 1;
		velocity.y += Physics.gravity.y * Time.deltaTime;

		// move character
		rb.velocity = velocity;

		// flip character to face direction of movement (velocity)
		if (velocity.x > 0 && !faceRight) Flip();
		if (velocity.x < 0 && faceRight) Flip();

		// update animator
		animator.SetFloat("Speed", Mathf.Abs(velocity.x));
	}

	IEnumerator DoubleJump()
	{ 
		yield return new WaitForSeconds(0.01f);
		while (velocity.y > 0)
		{
			if (Input.GetButtonDown("Jump"))
			{
				//velocity.y += Mathf.Sqrt(doubleJumpHeight * -2 * Physics.gravity.y);
				break;
			}
			yield return null;
		}
	}

	// flip the sprite 
	private void Flip()
	{ 
		faceRight = !faceRight;
		spriteRenderer.flipX = !faceRight;
	}

	// draw the sphere for colliding with the ground
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawSphere(groundTransform.position, groundRadius);
	}

	// find a new waypoint from the array of waypoints
	private void SetNewWaypointTarget()
	{
		Transform waypoint = null;
		do
		{
			waypoint = waypoints[Random.Range(0, waypoints.Length)];
		}
		while (waypoint == targetWaypoint);
		targetWaypoint = waypoint;
	}

	// determine if an enemy was seen
	private void CheckEnemySeen()
	{
		enemy = null;
		RaycastHit2D raycastHit = Physics2D.Raycast(transform.position, ((faceRight) ? Vector2.right : Vector2.left), sightDistance, raycastLayerMask);
		if (raycastHit.collider != null && raycastHit.collider.gameObject.CompareTag(enemyTag))
		{
			enemy = raycastHit.collider.gameObject;
			Debug.DrawRay(transform.position, ((faceRight) ? Vector2.right : Vector2.left) * sightDistance, Color.red);
		}
	}

	// apply damage to this character
	public void Damage(int damageAmount)
	{
		health -= damageAmount;
		print(health);
	}
}
