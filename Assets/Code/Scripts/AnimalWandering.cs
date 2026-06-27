using System.Collections;
using UnityEngine;

public class AnimalWandering : MonoBehaviour
{
    private Animal animal;

    private void Awake() => animal = GetComponent<Animal>();

    private void OnEnable() => StartCoroutine(WanderLoop());

    private IEnumerator WanderLoop()
    {
        while (true)
        {
            if (animal == null || animal.data == null)
            {
                yield return new WaitForSeconds(1f);
                continue;
            }

            Vector3 target = PickTarget();
            yield return MoveTo(target);
            yield return new WaitForSeconds(Random.Range(animal.data.wanderPauseMin, animal.data.wanderPauseMax));
        }
    }

    private Vector3 PickTarget()
    {
        if (animal.hasEscaped)
        {
            Vector2 offset = Random.insideUnitCircle * 5f;
            return new Vector3(transform.position.x + offset.x, transform.position.y, transform.position.z + offset.y);
        }

        if (animal.habitat == null) return transform.position;

        float cellSize = GridCreator.Instance != null ? GridCreator.Instance.cellSize : 1f;
        float minX = animal.habitat.xMin * cellSize + 0.3f;
        float maxX = (animal.habitat.xMax + 1) * cellSize - 0.3f;
        float minZ = animal.habitat.yMin * cellSize + 0.3f;
        float maxZ = (animal.habitat.yMax + 1) * cellSize - 0.3f;

        if (minX >= maxX || minZ >= maxZ) return transform.position;

        return new Vector3(Random.Range(minX, maxX), transform.position.y, Random.Range(minZ, maxZ));
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        if (animal == null || animal.data == null) yield break;

        float speed = animal.data.wanderSpeed;
        target.y = transform.position.y;

        while (Vector3.Distance(
                   new Vector3(transform.position.x, 0f, transform.position.z),
                   new Vector3(target.x, 0f, target.z)) > 0.1f)
        {
            Vector3 dir = target - transform.position;
            dir.y = 0f;
            dir.Normalize();
            transform.position += dir * speed * Time.deltaTime;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * 5f);
            yield return null;
        }
    }
}
