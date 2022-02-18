using System;
using System.Collections.Generic;
using System.Text;
using CsvHelper;

namespace PerkinElmerSP2CSV
{
    public interface IData
    {
        //public string Name { get; }
        public void WriteCsv(CsvWriter w);
    }
}
