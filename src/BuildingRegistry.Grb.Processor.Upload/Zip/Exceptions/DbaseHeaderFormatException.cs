namespace BuildingRegistry.Grb.Processor.Upload.Zip.Exceptions
{
    using System;

    public class DbaseHeaderFormatException : Exception
    {
        public string FileName { get; }

        public DbaseHeaderFormatException(string fileName, Exception innerException) : base("", innerException)
        {
            FileName = fileName;
        }
    }

    public class DbaseHeaderSchemaMismatchException : Exception
    {
        public string FileName { get; }

        public DbaseHeaderSchemaMismatchException(string fileName) : base("")
        {
            FileName = fileName;
        }
    }
}
