namespace BuildingRegistry.Grb.Processor.Upload.Zip.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class DbaseHeaderSchemaMismatchException : Exception
    {
        public string FileName { get; }

        public DbaseHeaderSchemaMismatchException(string fileName) : base("")
        {
            FileName = fileName;
        }

        private DbaseHeaderSchemaMismatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
