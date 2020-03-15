# Shred Metal Neon Gamplay Demo
___________________________________

This was going to be a game where you played a gun that could jump around, flip, and propel yourself through the air with recoil.

![](https://github.com/gljmelton/UnityPrototype.ShredMetalNeon/blob/master/Images/healthAndAmmo.gif?raw=true)
![](https://github.com/gljmelton/UnityPrototype.ShredMetalNeon/blob/master/Images/More%20gameplay.gif?raw=true)

## Missile components and projectiles

This project is a couple years old, and looking back at it I can see where there are definitely some areas that could use some streamlining. I went into the project trying to keep things as modular as possible, and mostly got there, but still ended up with soem funky coupling that could have been remedied by some inheritance.

Here is a look at the inspector for the Missile object. The key scripts for it are the <b>missile</b> component and the <b>projectile</b> component. The though process here is that the projectile script controls the physics of the object and the missile script controls the explosive/weaponized aspects of the object.

![](https://github.com/gljmelton/UnityPrototype.ShredMetalNeon/blob/master/Images/missile%20inspector.JPG?raw=true)

Some stuff point of importance here are the missile <b>damage falloff curve</b> and the <b>attributes section</b> of the projectile.

The damage falloff curve is used for the explosion when the missile dies. It takes the distance of any nearby objects and evaluates how much damage to apply based on a curve. Here's a look at the code:
```C#
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
```
[Script](https://github.com/gljmelton/UnityPrototype.ShredMetalNeon/blob/master/Code%20Samples/Missile.cs)

The attributes section has a few toggles and options for how the projectile should act. The idea here was to have one script that could create a wide variet of projectiles, from lobbed grenades, homing missiles, flak cannons and more. This is also set up in a way where you could have a projectile that shoots projectiles, like a slow homing missile that shoots bullets at the player.

Another big component that leveraged Unity's Cinemachine plugin was my camera tracking code. Looking at the gifs above you can see that the camera moves to not only focus on the player but any enemies within range. I used a weighted average system, where the player has a higher priority than enemies, to control a point in space that the camera follows. Any objects that are tracked by the player have a self contained camera tracking script. This script evaluates how far it is from the player and if it's within a range it adds itself to CameraLookTarget.

```C#
    public static CameraLookTarget instance;

    public CameraTarget player;
    public List<CameraTarget> lookTargets;

    private Vector3 targetPos;
    public float lookWeight;

	// Use this for initialization
	void Start () {
        //Set up the singleton instance
        if (instance == null)
            instance = this;
        else Destroy(gameObject);

        lookWeight = player.weight;
	}
	
	// Update is called once per frame
	void Update () {
        targetPos = player.transform.position;

        if (!lookTargets.Count.Equals(0)) {
            for (int i = 0; i < lookTargets.Count; i++)
            {
                //Nudge the cameras target toward a weighted average position between the targets and the player.
                targetPos += (lookTargets[i].weight / lookWeight) * (lookTargets[i].transform.position - player.transform.position);
            }
        }

        //Set the position of the look target.
        transform.position = targetPos;
	}

    //Add a new object to the list of targets that should be kept in view.
    public void AddObject(CameraTarget obj) {
        lookTargets.Add(obj);
        lookWeight += obj.weight;
    }

    //Remove an object from the list of targets that should be kept in view.
    public void RemoveObject(CameraTarget obj) {
        lookTargets.Remove(obj);
        lookWeight -= obj.weight;
    }
```
[Script](https://github.com/gljmelton/UnityPrototype.ShredMetalNeon/blob/master/Code%20Samples/CameraLookTarget.cs)
