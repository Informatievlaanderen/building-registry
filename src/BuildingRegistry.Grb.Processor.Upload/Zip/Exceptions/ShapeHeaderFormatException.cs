namespace BuildingRegistry.Grb.Processor.Upload.Zip.Exceptions;

using System;

public class ShapeHeaderFormatException : Exception
{
    public ShapeHeaderFormatException(Exception innerException) : base("", innerException)
    {
    }
}
