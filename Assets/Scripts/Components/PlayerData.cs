using Unity.Entities;
using UnityEngine.VFX;

[GenerateAuthoringComponent]
public struct PlayerData : IComponentData {
    
    // PlayerShip speed / thrust
    public float forwardThrust;
    public float backwardThrust;
    public float sideThrust;
    public float pitchThrust;

    public bool dampener;

    // Fire Stuff 
    public float reloadTime;
    public float nextFire;
    
    public Entity bulletPrefabEntity;
}
