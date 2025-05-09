using System;
using System.Collections.Generic;
using System.IO;
using OpenMcdf;

namespace PerkinElmerSP2CSV
{
    /// <summary>
    /// PRF files use Microsoft Compund File format v3 (512 bytes/sector)
    /// </summary>
    public class PrfFileProvider : IFileProvider
    {
        private static readonly Guid _guid = new Guid("ac5d256a-804e-11d0-88f2-0020afc9a5cc");
        private static readonly string _xStreamName = "prop36";
        private static readonly string _yStreamName = "prop37";
        private static readonly string _contentsStreamName = "CONTENTS";
        private static readonly PrfFileProvider _instance = new PrfFileProvider();
        const int SizeofDouble = 8;
        const int SizeofInt = 4;
        const int ProfileLengthOffset = 0xE4;

        private static double[] GetPointsFromStream(CFStream stream, int number)
        {
            if (!BitConverter.IsLittleEndian)
                throw new NotSupportedException("BigEndian architectures are not supported (yet).");
            int totalBytes = number * SizeofDouble;
            if (totalBytes > stream.Size) throw new InvalidDataException("This many points can't fit into this CFStream!");
            byte[] xBytes = new byte[totalBytes];
            stream.Read(xBytes, 0, totalBytes);
            double[] points = new double[number];
            for (int i = 0; i < number; i++)
            {
                points[i] = BitConverter.ToDouble(xBytes, i * SizeofDouble);
            }
            return points;
        }
        private PrfFileProvider() { }

        public static PrfFileProvider Instance { get => _instance; }

        public string Extension => ".prf";
        public Guid StorageGuid => _guid;

        public IData GetData(string path)
        {
            var prf = new IntensityProfile() { Name = Path.GetFileNameWithoutExtension(path) };
            CompoundFile file = new CompoundFile(path);
            List<CFItem> items = new List<CFItem>();
            file.RootStorage.VisitEntries((cfi) =>
            {
                if (cfi.IsStorage && (cfi.CLSID == StorageGuid)) items.Add(cfi);
            }, false);
            if (items.Count == 0) return null;
            var storage = (CFStorage)(items[0]);
            var contents = storage.GetStream(_contentsStreamName);
            byte[] buffer = new byte[SizeofInt];
            contents.Read(buffer, ProfileLengthOffset, buffer.Length);
            int numberOfPoints = BitConverter.ToInt32(buffer, 0);
            prf.PointsX = GetPointsFromStream(storage.GetStream(_xStreamName), numberOfPoints);
            prf.PointsY = GetPointsFromStream(storage.GetStream(_yStreamName), numberOfPoints);
            return prf;
        }
    }
}