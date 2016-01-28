using System;

namespace TinyPngApi
{

    public class ScaleWidthResizeOperation : ResizeOperation
    {
        public ScaleWidthResizeOperation(int width) : base (ResizeType.Scale, width, 0)
        {

        }
    }

    public class ScaleHeightResizeOperation : ResizeOperation
    {
        public ScaleHeightResizeOperation(int height) : base(ResizeType.Scale, 0, height)
        {

        }
    }

    public class FitResizeOperation : ResizeOperation
    {
        public FitResizeOperation(int width, int height) : base(ResizeType.Fit, width, height)
        {

        }
    }

    public class CoverResizeOperation : ResizeOperation
    {
        public CoverResizeOperation(int width, int height) : base(ResizeType.Cover, width, height)
        {
            if (width == 0)
            {
                throw new ArgumentException("You must specify a width", nameof(width));
            }
            if (height == 0)
            {
                throw new ArgumentException("You must specify a height", nameof(width));
            }
        }
    }

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
