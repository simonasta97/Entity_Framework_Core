namespace SoftJail.DataProcessor.ExportDto
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using System.Xml.Serialization;
    using SoftJail.Data.Models.Enums;

    [XmlType("Prisoner")]
    public class ExportPrisonerDto
    {
        [XmlElement("Id")]
        public int Id { get; set; }

        [XmlElement("Name")]
        public string Name { get; set; }

        [XmlElement("IncarcerationDate")]
        public string IncarcerationDate { get; set; }

        [XmlArray("EncryptedMessages")]
        public ExportMessageDto []  EncryptedMessages { get; set; }
    }

    [XmlType("Message")]
    public class ExportMessageDto
    {
        [XmlElement("Description")]
        public string Description { get; set; }
    }
}
