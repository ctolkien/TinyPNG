namespace TinyPng.ResizeOperations;

/// <summary>
/// Represents a resize operation that scales the width of an image.
/// </summary>
/// <remarks>
/// Initializes a new instance of the ScaleWidthResizeOperation class with the specified width.
/// </remarks>
/// <param name="width">The desired width of the image.</param>
public class ScaleWidthResizeOperation(int width) : ResizeOperation(ResizeType.Scale, width, null)
{
}
