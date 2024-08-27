namespace TinyPng.ResizeOperations;

public class ResizeOperation
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ResizeOperation"/> class.
    /// </summary>
    /// <param name="type">The resize type.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    public ResizeOperation(ResizeType type, int width, int height)
    {
        Method = type;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ResizeOperation"/> class.
    /// </summary>
    /// <param name="type">The resize type.</param>
    /// <param name="width">The width.</param>
    /// <param name="height">The height.</param>
    internal ResizeOperation(ResizeType type, int? width, int? height)
    {
        Method = type;
        Width = width;
        Height = height;
    }

    /// <summary>
    /// Gets the width.
    /// </summary>
    public int? Width { get; }

    /// <summary>
    /// Gets the height.
    /// </summary>
    public int? Height { get; }

    /// <summary>
    /// Gets the resize method.
    /// </summary>
    public ResizeType Method { get; }
}


/// <summary>
/// Represents the resize type.
/// </summary>
public enum ResizeType
{
    /// <summary>
    /// Fit resize type.
    /// </summary>
    Fit,

    /// <summary>
    /// Scale resize type.
    /// </summary>
    Scale,

    /// <summary>
    /// Cover resize type.
    /// </summary>
    Cover
}
