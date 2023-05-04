namespace BuildingRegistry.Grb.Processor.Upload.Zip.Exceptions
{
    using System;

    public class NoShapeRecordsException : Exception
    {
        public string FileName { get; }

        public  NoShapeRecordsException(string fileName) : base("")
        {
            FileName = fileName;
        }
    }
}
