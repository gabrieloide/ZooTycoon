using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ZooTycoon.Data;

public class VisitorNPC : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float viewDurationMin = 3f;
    [SerializeField] private float viewDurationMax = 7f;
    [SerializeField] private float rotationSpeed = 5f;

    private VisitorConfig visitorConfig;
    private Vector2 currentCell;
    private bool fleeRequested;
    private Animal fleeThreat;

    private static readonly List<VisitorNPC> activeVisitors = new();
    public static IReadOnlyList<VisitorNPC> ActiveVisitors => activeVisitors;

    public void SetConfig(VisitorConfig config) => visitorConfig = config;

    private void OnEnable()
    {
        activeVisitors.Add(this);
        StartCoroutine(WanderLoop());
    }

    private void OnDisable()
    {
        activeVisitors.Remove(this);
        StopAllCoroutines();
    }

    private void Update()
    {
        if (fleeRequested || visitorConfig == null) return;

        Animal threat = FindNearestEscapedAnimal();
        if (threat != null)
        {
            fleeThreat = threat;
            fleeRequested = true;
        }
    }

    private Animal FindNearestEscapedAnimal()
    {
        Animal nearest = null;
        float nearestSqr = visitorConfig.fleeRadius * visitorConfig.fleeRadius;

        foreach (var animal in Animal.EscapedAnimals)
        {
            if (animal == null) continue;
            float sqr = (animal.transform.position - transform.position).sqrMagnitude;
            if (sqr <= nearestSqr)
            {
                nearestSqr = sqr;
                nearest = animal;
            }
        }

        return nearest;
    }

    private IEnumerator WanderLoop()
    {
        while (true)
        {
            if (fleeRequested)
            {
                yield return Flee();
                continue;
            }

            var nearest = PathManager.NearestCell(transform.position);
            if (!nearest.HasValue)
            {
                yield return InterruptibleWait(2f);
                continue;
            }
            currentCell = nearest.Value;

            var habitats = new List<HabitatSpace>(HabitatManager.GetAllHabitats());
            Shuffle(habitats);

            bool foundTarget = false;
            foreach (var habitat in habitats)
            {
                if (fleeRequested) break;

                var viewCell = habitat.GetNearestViewingCell();
                if (!viewCell.HasValue) continue;

                var route = PathManager.FindPath(currentCell, viewCell.Value);
                if (route == null) continue;

                yield return WalkRoute(route);
                if (fleeRequested) break;
                currentCell = viewCell.Value;

                yield return FaceTarget(habitat.GetWorldCenter());
                yield return InterruptibleWait(Random.Range(viewDurationMin, viewDurationMax));
                foundTarget = true;
                break;
            }

            if (!foundTarget && !fleeRequested)
                yield return InterruptibleWait(2f);
        }
    }

    private IEnumerator Flee()
    {
        float fleeSpeed = moveSpeed * visitorConfig.fleeSpeedMultiplier;
        float elapsed = 0f;

        while (elapsed < visitorConfig.fleeDuration)
        {
            if (fleeThreat == null) fleeThreat = FindNearestEscapedAnimal();
            if (fleeThreat == null) break;

            elapsed += Time.deltaTime;

            Vector3 away = transform.position - fleeThreat.transform.position;
            away.y = 0f;
            if (away.sqrMagnitude < 0.001f) away = transform.forward;
            away.Normalize();

            transform.position += away * fleeSpeed * Time.deltaTime;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(away), Time.deltaTime * rotationSpeed);
            yield return null;
        }

        fleeRequested = false;
        fleeThreat = null;
    }

    private IEnumerator InterruptibleWait(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration && !fleeRequested)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private IEnumerator WalkRoute(List<Vector2> route)
    {
        var gridCreator = GridCreator.Instance;
        if (gridCreator == null) yield break;

        foreach (var cell in route)
        {
            if (fleeRequested) yield break;
            Vector3 target = gridCreator.GetCellWorldPosition(cell);
            target.y = transform.position.y;
            yield return MoveTo(target);
        }
    }

    private IEnumerator MoveTo(Vector3 target)
    {
        while (true)
        {
            if (fleeRequested) yield break;

            Vector3 flat = new Vector3(transform.position.x, 0f, transform.position.z);
            Vector3 flatTarget = new Vector3(target.x, 0f, target.z);
            if (Vector3.Distance(flat, flatTarget) <= 0.15f) yield break;

            Vector3 dir = flatTarget - flat;
            dir.Normalize();
            transform.position += dir * moveSpeed * Time.deltaTime;
            if (dir.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), Time.deltaTime * rotationSpeed);
            yield return null;
        }
    }

    private IEnumerator FaceTarget(Vector3 worldPos)
    {
        Vector3 dir = worldPos - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) yield break;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        float elapsed = 0f;
        while (elapsed < 0.4f && !fleeRequested)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, elapsed / 0.4f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        if (!fleeRequested) transform.rotation = targetRot;
    }

    private static void Shuffle<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
