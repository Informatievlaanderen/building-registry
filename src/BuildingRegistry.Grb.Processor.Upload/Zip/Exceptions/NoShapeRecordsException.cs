namespace BuildingRegistry.Grb.Processor.Upload.Zip.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class NoShapeRecordsException : Exception
    {
        public string FileName { get; }

        public  NoShapeRecordsException(string fileName) : base("")
        {
            FileName = fileName;
        }

        private NoShapeRecordsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
