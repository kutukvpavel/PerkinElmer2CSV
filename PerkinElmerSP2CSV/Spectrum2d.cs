using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper;

namespace PerkinElmerSP2CSV
{
    public class Spectrum2d : IData
    {
        public double StartX { get; set; }
        public double EndX { get; set; }
        public double ResolutionX { get; set; }
        public string LabelX { get; set; }
        public string LabelY { get; set; }
        public string Name { get; set; }
        public string Alias { get; set; }
        public double[] PointsY { get; set; }

        public void WriteCsv(CsvWriter w)
        {
            //Header
            w.WriteField(LabelX);
            w.WriteField(LabelY);
            w.NextRecord();
            //Rows
            double x = StartX;
            foreach (var item in PointsY)
            {
                w.WriteField(x);
                w.WriteField(item);
                w.NextRecord();
                x += ResolutionX;
            }
        }
    }
}
