using UnityEngine;
using Unity.Mathematics;

public static class Utils {

    public static class Rotation {

        public static quaternion Forward(quaternion rotation) {
            return rotation;
        }

        public static quaternion Backward(quaternion rotation) {
            return math.mul(rotation, quaternion.Euler(0, math.radians(-180), 0));
        }

        public static quaternion Left (quaternion rotation) {
            return math.mul(rotation, quaternion.Euler(0, math.radians(-90), 0));
        }

        public static quaternion Right (quaternion rotation) {
            return math.mul(rotation, quaternion.Euler(0, math.radians(90), 0));
        }

        public static quaternion Up (quaternion rotation) {
            return math.mul(rotation, quaternion.Euler(math.radians(-90), 0, 0));
        }

        public static quaternion Down (quaternion rotation) {
            return math.mul(rotation, quaternion.Euler(math.radians(90), 0, 0));
        }
    }

    public static class Vector {

        public static float3 Forward(quaternion rotation) {
            return math.forward(Utils.Rotation.Forward(rotation));
        }

        public static float3 Backward(quaternion rotation) {
            return math.forward(Utils.Rotation.Backward(rotation));
        }

        public static float3 Left (quaternion rotation) {
            return math.forward(Utils.Rotation.Left(rotation));
        }

        public static float3 Right (quaternion rotation) {
            return math.forward(Utils.Rotation.Right(rotation));
        }

        public static float3 Up (quaternion rotation) {
            return math.forward(Utils.Rotation.Up(rotation));
        }

        public static float3 Down (quaternion rotation) {
            return math.forward(Utils.Rotation.Down(rotation));
        }
    }
}