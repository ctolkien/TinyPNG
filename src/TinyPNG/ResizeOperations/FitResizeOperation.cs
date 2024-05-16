namespace TinyPng.ResizeOperations;

public class FitResizeOperation(int width, int height) : ResizeOperation(ResizeType.Fit, width, height)
{
}
