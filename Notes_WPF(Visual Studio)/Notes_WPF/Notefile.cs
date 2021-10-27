using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Notes_WPF
{
    class Notefile
    {
        public string Path;
        public string Name;
        public DateTime CreatiionDate;

        public Notefile()
        {
            Path = "";
            Name = "";
            CreatiionDate = DateTime.MinValue;
        }

        public string ReadFromFile()
        {
            return File.ReadAllText(Path);
        }

        public void WriteToFile(string text)
        {
            File.WriteAllText(Path, text);
        }
    }
}
