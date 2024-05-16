namespace TinyPng.ResizeOperations;

public class ScaleHeightResizeOperation(int height) : ResizeOperation(ResizeType.Scale, null, height)
{
}
