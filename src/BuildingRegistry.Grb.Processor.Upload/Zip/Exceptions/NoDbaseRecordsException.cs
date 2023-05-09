namespace BuildingRegistry.Grb.Processor.Upload.Zip.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public sealed class NoDbaseRecordsException : Exception
    {
        public string FileName { get; }

        public  NoDbaseRecordsException(string fileName) : base("")
        {
            FileName = fileName;
        }

        private NoDbaseRecordsException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
