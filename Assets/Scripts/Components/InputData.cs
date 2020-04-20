using Unity.Entities;

[GenerateAuthoringComponent]
public struct InputData : IComponentData {
    public float axisVertical;
    public float axisHorizontal;
}
