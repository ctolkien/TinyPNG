using System;

namespace TinyPng.ResizeOperations;

/// <summary>
/// Represents a resize operation that covers the specified width and height.
/// </summary>
public class CoverResizeOperation : ResizeOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CoverResizeOperation"/> class with the specified width and height.
    /// </summary>
    /// <param name="width">The width to cover.</param>
    /// <param name="height">The height to cover.</param>
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
