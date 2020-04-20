using Unity.Entities;

[GenerateAuthoringComponent]
public struct LifeTimeData : IComponentData {
    public bool infiniteLifetime; 
    public float lifeLeft;
    
    public bool alive; 
}
