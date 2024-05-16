namespace TinyPng.ResizeOperations;

public class ScaleWidthResizeOperation(int width) : ResizeOperation(ResizeType.Scale, width, null)
{
}
