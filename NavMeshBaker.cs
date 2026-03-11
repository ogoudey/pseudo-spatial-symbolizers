using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;
public class NavMeshBaker : MonoBehaviour
{
    void Awake()
    {
        var surface = FindFirstObjectByType<NavMeshSurface>();
        if (surface != null)
        {
            // BuildNavMesh here works at runtime
            surface.BuildNavMesh();
        }
    }
}