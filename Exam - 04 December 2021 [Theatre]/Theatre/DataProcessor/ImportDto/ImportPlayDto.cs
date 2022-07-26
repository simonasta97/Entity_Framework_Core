using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using Theatre.Data.Models.Enums;

namespace Theatre.DataProcessor.ImportDto
{
    [XmlType("Play")]
    public class ImportPlayDto
    {
        [XmlElement("Title")]
        public string Title { get; set; }

        [XmlElement("Duration")]
        public string Duration { get; set; }

        [XmlElement("Rating")]
        public float Rating { get; set; }

        [XmlElement("Genre")]
        public string Genre { get; set; }

        [XmlElement("Description")]
        public string Description { get; set; }

        [XmlElement("Screenwriter")]
        public string Screenwriter { get; set; }
    }
}
