using System;
using CsvHelper;

namespace PerkinElmerSP2CSV
{
    public class IntensityProfile : IData
    {
        public string Name { get; set; }
        public double[] PointsX { get; set; }
        public double[] PointsY { get; set; }

        public void WriteCsv(CsvWriter w)
        {
            //Header
            w.WriteField("Time, s");
            w.WriteField("RMS Intensity, abs");
            w.NextRecord();
            //Rows
            int len = Math.Min(PointsX.Length, PointsY.Length);
            for (int i = 0; i < len; i++)
            {
                w.WriteField(PointsX[i]);
                w.WriteField(PointsY[i]);
                w.NextRecord();
            }
        }
    }
}
