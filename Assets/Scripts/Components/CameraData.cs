using Unity.Entities;

[GenerateAuthoringComponent]
public struct CameraData : IComponentData {

    // 0 - first person camera 
    // 1 - third person camera
    // default: 1
    public int cameraMode; 
}
