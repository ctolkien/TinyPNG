namespace TinyPng.ResizeOperations
{
    public class ScaleHeightResizeOperation : ResizeOperation
    {
        public ScaleHeightResizeOperation(int height) : base(ResizeType.Scale, 0, height) { }
    }
}
