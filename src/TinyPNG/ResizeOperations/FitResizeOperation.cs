namespace TinyPng.ResizeOperations;


/// <summary>
/// Represents a fit resize operation.
/// </summary>
public class FitResizeOperation(int width, int height) : ResizeOperation(ResizeType.Fit, width, height)
{
}
