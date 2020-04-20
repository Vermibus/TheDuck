using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct CivilianShipData : IComponentData {
    public float speed;
    public float followDistance;
    public int currentWaypoint;
    // public bool alive;
    public Entity explosionPrefabEntity;
}
