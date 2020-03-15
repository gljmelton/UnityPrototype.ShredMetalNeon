using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour {

    public LayerMask collisionMask;
    public float collisionCheckRadius;

    Rigidbody2D rb2d;
    public GameObject explode;
    public int damage;
    public float explosionRadius;
    public float explosionDamageRadius;
    public float explosionForce;
    public AnimationCurve damageFalloff;

    private bool inPlayerRange;

    //Components
    public Health health;
    private CameraTarget lookTarget;
    private Projectile projectile;

	// Use this for initialization
	void Start () {
        rb2d = GetComponent<Rigidbody2D>();

        lookTarget = GetComponent<CameraTarget>();

        health = GetComponent<Health>();
        health.onDeath += OnDeath;
        health.InitHealth();

        projectile = GetComponent<Projectile>();
        projectile.onDie += OnDeath;
        projectile.Init(rb2d);

        //ammoDropper = GetComponent<AmmoDropper>();
        
        inPlayerRange = false;
        
	}

    void Update() {

        //Check to see how far away this projectile is from the player. If they're within range add them to the camera target.
        float distanceToPlayer = Vector3.Distance(GameData.playerCharacter.transform.position, gameObject.transform.position);

        if (!inPlayerRange && distanceToPlayer <= lookTarget.maxPlayerDistance) {
            CameraLookTarget.instance.AddObject(lookTarget);
            inPlayerRange = true;
        }
        else if (inPlayerRange && distanceToPlayer > lookTarget.maxPlayerDistance) {
            CameraLookTarget.instance.RemoveObject(lookTarget);
            inPlayerRange = false;
        }

        projectile.Move(GameData.playerCharacter.transform.position);
        
    }
    
    void FixedUpdate() {

        CheckCollision();

    }

    //What happens on death
    public void OnDeath() {
        ClearDelegates(); //clear any delegate relations
        CameraLookTarget.instance.RemoveObject(gameObject.GetComponent<CameraTarget>()); //Remove self as camera target

        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius); //See who I'm hitting

        //Apply physics force to anything in explosion radius
        foreach(Collider2D collider in colliders) {
            Rigidbody2D rb = collider.GetComponent<Rigidbody2D>();

            if (rb != null) {
                Rigidbody2DExtension.AddExplosionForce(rb, explosionForce, transform.position, explosionRadius);
            }
        }

        //ammoDropper.DropLoot(); //Drop ammo
        Instantiate(explode, transform.position, explode.transform.rotation);

        RaycastHit2D[] hits = Physics2D.CircleCastAll(transform.position, explosionDamageRadius, Vector2.zero, 0f, collisionMask);

        //Apply damage to any nearby damageable objects.
        foreach (RaycastHit2D hit in hits) {
            if (hit.collider != null) {
                Health targetHealth = hit.collider.gameObject.GetComponent<Health>();
                if (targetHealth != null) {
                    targetHealth.Hit(new Hit(gameObject, CalculateDamage(hit.collider.transform)));
                }
                
            }
        }

        Destroy(gameObject);
    }

    //Calculate damage for nearby objects when exploding
    public int CalculateDamage(Transform target) {

        float resultF = damageFalloff.Evaluate(Vector2.Distance(transform.position, target.position)/explosionDamageRadius); //Get distance between divided by max radius to evaluate falloff graph

        resultF *= damage; //Mutliply by damage to get non-between 1 and 0 damage value

        int result = Mathf.RoundToInt(resultF); //conver float result to int
        return result;
    }

    public void ClearDelegates() {
        health.onDeath -= OnDeath;
    }

    private void CheckCollision() {
        if (Physics2D.CircleCast(transform.position, collisionCheckRadius, Vector2.zero, 0f, collisionMask)) {
            OnDeath();
        }
    }

}
