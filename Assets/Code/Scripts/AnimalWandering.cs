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

            if (animal.hasEscaped)
            {
                yield return EscapedStep();
                continue;
            }

            float speed = animal.data.wanderSpeed * Random.Range(1f - animal.data.wanderSpeedVariance, 1f + animal.data.wanderSpeedVariance);
            yield return MoveTo(PickHabitatTarget(), speed);
            yield return IdleLook(Random.Range(animal.data.wanderPauseMin, animal.data.wanderPauseMax));
        }
    }

    private IEnumerator EscapedStep()
    {
        VisitorNPC target = FindNearestVisitor();
        if (target == null)
        {
            yield return MoveTo(PickFreeRoamTarget(), animal.data.wanderSpeed);
            yield return new WaitForSeconds(Random.Range(animal.data.wanderPauseMin, animal.data.wanderPauseMax));
            yield break;
        }

        yield return Chase(target);
    }

    private VisitorNPC FindNearestVisitor()
    {
        VisitorNPC nearest = null;
        float nearestSqr = animal.data.chaseDetectionRadius * animal.data.chaseDetectionRadius;

        foreach (var visitor in VisitorNPC.ActiveVisitors)
        {
            if (visitor == null) continue;
            float sqr = (visitor.transform.position - transform.position).sqrMagnitude;
            if (sqr <= nearestSqr)
            {
                nearestSqr = sqr;
                nearest = visitor;
            }
        }

        return nearest;
    }

    private IEnumerator Chase(VisitorNPC target)
    {
        float giveUpRadius = animal.data.chaseDetectionRadius * 1.5f;

        while (animal.hasEscaped && target != null)
        {
            Vector3 flat = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 targetFlat = new Vector3(target.transform.position.x, 0f, target.transform.position.z);
            float dist = Vector3.Distance(flat, targetFlat);

            if (dist > giveUpRadius) yield break;

            Vector3 dir = targetFlat - flat;
            dir.Normalize();
            transform.position += dir * animal.data.chaseSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * animal.data.rotationSpeed);
            yield return null;
        }
    }

    private Vector3 PickHabitatTarget()
    {
        if (animal.habitat == null) return transform.position;

        float cellSize = GridCreator.Instance != null ? GridCreator.Instance.cellSize : 1f;
        float minX = animal.habitat.xMin * cellSize + 0.3f;
        float maxX = (animal.habitat.xMax + 1) * cellSize - 0.3f;
        float minZ = animal.habitat.yMin * cellSize + 0.3f;
        float maxZ = (animal.habitat.yMax + 1) * cellSize - 0.3f;

        if (minX >= maxX || minZ >= maxZ) return transform.position;

        return new Vector3(Random.Range(minX, maxX), transform.position.y, Random.Range(minZ, maxZ));
    }

    private Vector3 PickFreeRoamTarget()
    {
        Vector2 offset = Random.insideUnitCircle * animal.data.chaseDetectionRadius;
        return new Vector3(transform.position.x + offset.x, transform.position.y, transform.position.z + offset.y);
    }

    private IEnumerator MoveTo(Vector3 target, float speed)
    {
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
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * animal.data.rotationSpeed);
            yield return null;
        }
    }

    private IEnumerator IdleLook(float duration)
    {
        Quaternion startRot = transform.rotation;
        Quaternion lookRot = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRot, lookRot, Mathf.Clamp01(elapsed / duration));
            yield return null;
        }
    }
}
