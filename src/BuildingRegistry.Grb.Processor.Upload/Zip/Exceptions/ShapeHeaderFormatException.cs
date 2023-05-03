namespace BuildingRegistry.Grb.Processor.Upload.Zip.Exceptions
{
    using System;

    public class ShapeHeaderFormatException : Exception
    {
        public string FileName { get; }

        public ShapeHeaderFormatException(string fileName, Exception innerException) : base("", innerException)
        {
            FileName = fileName;
        }
    }
}
