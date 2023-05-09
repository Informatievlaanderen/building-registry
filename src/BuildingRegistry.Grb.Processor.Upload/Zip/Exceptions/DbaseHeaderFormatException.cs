namespace BuildingRegistry.Grb.Processor.Upload.Zip.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class DbaseHeaderFormatException : Exception
    {
        public string FileName { get; }

        public DbaseHeaderFormatException(string fileName, Exception innerException) : base("", innerException)
        {
            FileName = fileName;
        }

        private DbaseHeaderFormatException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
