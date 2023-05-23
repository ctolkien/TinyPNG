using System;

namespace TinyPng
{
    [Flags]
    public enum PreserveMetadata
    {
        None = 1 << 0,
        Copyright = 1 << 1,
        Creation = 1 << 2,
        Location = 1 << 3
    }
}
