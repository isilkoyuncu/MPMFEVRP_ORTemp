using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Instance_Generation.Interfaces;
using Instance_Generation.FileReaders;
using Instance_Generation.FileWriters;

namespace Instance_Generation.Other
{
    public class ProblemConstants
    {
        public static readonly List<string> PROBLEMS = new List<string>() { "KoyuncuYavuz", "EMH_12", "Felipe_14", "Goeke_15", "Schneider_14", "YavuzCapar_17" };
    }

    public class FileTypeConstants
    {
        //var InputFileTypes = new IInputFile[] { ErdoganMiller_Hooks12 };
        public static readonly Dictionary<string, IRawReader> InputFileTypes =
            new Dictionary<string, IRawReader>()
            {
                {"CompletelyNew", null },
                //{"KoyuncuYavuz",new ErdoganMiller_Hooks12()},//TODO Add "KoyuncuYavuz"
                {"EMH_12",new ErdoganMiller_Hooks12Reader()},
                {"Felipe_14",new Felipe14Reader()},
                {"Goeke_15",new GoekeSchneider15Reader()},
                {"Schneider_14",new Schneider14Reader()},
                {"YavuzCapar_17",new YavuzCapar17Reader()},
            };
        public static readonly Dictionary<string, IWriter> OutputFileTypes =
            new Dictionary<string, IWriter>()
            {
                {"KoyuncuYavuz",new KoyuncuYavuzFileWriter()},
            };
    }
}
