using Unity.Entities;
using UnityEngine.VFX;

[GenerateAuthoringComponent]
public struct PlayerData : IComponentData {
    public float speed;
    public float reloadTime;
    public float nextFire;
    
    public Entity bulletPrefabEntity;
}
