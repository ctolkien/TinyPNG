namespace TinyPng
{
    public class ResizeOperation
    {
        public ResizeOperation(ResizeType type, int width, int height)
        {
            Method = type;
            Width = width;
            Height = height;
        }

        public int Width { get; set; }
        public int Height { get; set; }
        public ResizeType Method { get; set; }
    }

    public enum ResizeType
    {
        Fit,
        Scale,
        Cover
    }
}
