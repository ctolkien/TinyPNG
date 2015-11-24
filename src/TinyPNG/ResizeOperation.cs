namespace TinyPng
{
    public class ResizeOperation
    {
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
