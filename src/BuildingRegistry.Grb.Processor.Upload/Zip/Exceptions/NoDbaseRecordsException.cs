namespace BuildingRegistry.Grb.Processor.Upload.Zip.Exceptions
{
    using System;

    public class NoDbaseRecordsException : Exception
    {
        public string FileName { get; }

        public  NoDbaseRecordsException(string fileName) : base("")
        {
            FileName = fileName;
        }
    }
}
