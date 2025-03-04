using UnityEngine;
using UnityEngine.U2D;

[ExecuteInEditMode]
public class TerrainGenerator : MonoBehaviour
{
    [SerializeField] SpriteShapeController shapeController;

    [Header("Parameters")]
    [SerializeField, Range (3,1000)] int levelLength;
    [SerializeField, Range (0.1f, 10)] float xMultiplier;
    [SerializeField, Range(0.1f, 10)] float yMultiplier;
    [SerializeField, Range (0,1)] float curveSmoothness;
    [SerializeField] float noiseStep;
    [SerializeField] float bottom;

    Vector2 lastPosition;

    private void OnValidate()
    {
        shapeController.spline.Clear();

        for (int i = 0; i < levelLength; i++)
        {
            if (i !=0) lastPosition = new Vector3(i * xMultiplier, i * yMultiplier * Mathf.PerlinNoise(0,i*noiseStep) / 300);
            else lastPosition = new Vector3(0, bottom);

            shapeController.spline.InsertPointAt(i, lastPosition);

            if (i != levelLength - 1 && i > 1)
            {
                shapeController.spline.SetTangentMode(i, ShapeTangentMode.Continuous);
                shapeController.spline.SetLeftTangent(i, xMultiplier * curveSmoothness * Vector3.left);
                shapeController.spline.SetRightTangent(i, xMultiplier * curveSmoothness * Vector3.right);
            }
        }

        shapeController.spline.InsertPointAt(levelLength, new Vector3(lastPosition.x,- bottom));
        shapeController.spline.InsertPointAt(levelLength + 1, new Vector3(0,- bottom));
    }

}
