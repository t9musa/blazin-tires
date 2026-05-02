using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Disables physics collisions between all cars in the scene.
// Attach to any GameObject in the race scene — runs once on Start.
public class CarCollisionDisabler : MonoBehaviour
{
    static readonly string[] CarTags = { "Player", "Bluecar", "Yellowcar", "Redcar" };

    void Start()
    {
        StartCoroutine(DisableAfterStart());
    }

    IEnumerator DisableAfterStart()
    {
        yield return null; // wait one frame so all Start() methods finish first

        // Collect colliders per car — use Rigidbody as the car root since all cars have one
        // and tags work regardless of the car controller type used
        var allRigidbodies = FindObjectsByType<Rigidbody>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var carColliders = new List<Collider[]>();

        foreach (var rb in allRigidbodies)
        {
            foreach (var tag in CarTags)
            {
                if (rb.CompareTag(tag))
                {
                    var cols = rb.GetComponentsInChildren<Collider>(true);
                    if (cols.Length > 0)
                    {
                        carColliders.Add(cols);
                        Debug.Log($"[CarCollisionDisabler] Car: {rb.gameObject.name} (tag={tag}, colliders={cols.Length})");
                    }
                    break;
                }
            }
        }

        int pairs = 0;
        for (int i = 0; i < carColliders.Count; i++)
            for (int j = i + 1; j < carColliders.Count; j++)
                foreach (var a in carColliders[i])
                    foreach (var b in carColliders[j])
                    {
                        Physics.IgnoreCollision(a, b, true);
                        pairs++;
                    }

        Debug.Log($"[CarCollisionDisabler] {carColliders.Count} cars found, {pairs} collider pairs ignored");
    }
}
