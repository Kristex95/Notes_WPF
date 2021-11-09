using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using CsvHelper.Configuration.Attributes;
using System.Globalization;

namespace Notes_WPF
{
    class Notefile
    {
        [Index(0)]
        public string Name { get; set; }
        [Index(1)]
        public string Text { get; set; }
        [Index(2)]
        public DateTime CreationDate { get; set; }
        [Index(3)]
        public bool IsArchived { get; set; }
        [Index(4)]
        public string Tag { get; set; }



        public Notefile()
        {
            Name = "";
            Text = "";
            CreationDate = DateTime.MinValue;
            IsArchived = false;
            Tag = "";
        }

        public Notefile(string Name, string Text, DateTime CreationDate, bool IsArchived, string Tag)
        {
            this.Name = Name;
            this.Text = Text;
            this.CreationDate = CreationDate;
            this.IsArchived = IsArchived;
            this.Tag = Tag;
        }
    }
}
