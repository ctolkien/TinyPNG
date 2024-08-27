namespace TinyPng.ResizeOperations;


/// <summary>
/// Represents a resize operation that scales the height of an image.
/// </summary>
public class ScaleHeightResizeOperation(int height) : ResizeOperation(ResizeType.Scale, null, height)
{
}
