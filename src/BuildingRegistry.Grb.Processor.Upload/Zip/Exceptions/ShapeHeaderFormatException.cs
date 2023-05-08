namespace BuildingRegistry.Grb.Processor.Upload.Zip.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class ShapeHeaderFormatException : Exception
    {
        public string FileName { get; }

        public ShapeHeaderFormatException(string fileName, Exception innerException) : base("", innerException)
        {
            FileName = fileName;
        }

        private ShapeHeaderFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
