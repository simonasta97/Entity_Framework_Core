namespace SoftJail.DataProcessor
{

    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using SoftJail.Data;
    using SoftJail.Data.Models;
    using SoftJail.DataProcessor.ExportDto;
    using Newtonsoft.Json;
    using System.Globalization;
    using VaporStore.XmlHelper;
    using SoftJail.Data.Models.Enums;

    public class Serializer
    {
        public static string ExportPrisonersByCells(SoftJailDbContext context, int[] ids)
        {
            var prisoners= context
                .Prisoners
                .ToList()
                .Where(x=> ids.Contains(x.Id))
                .Select(x=> new 
                {
                    Id=x.Id,
                    Name=x.FullName,
                    CellNumber=x.Cell.CellNumber,
                    Officers=x.PrisonerOfficers.Select(o=> new
                    {
                        OfficerName=o.Officer.FullName,
                        Department=o.Officer.Department.Name
                    })
                    .OrderBy(o=>o.OfficerName),
                    TotalOfficerSalary= decimal.Parse(x.PrisonerOfficers.Sum(o=>o.Officer.Salary).ToString("F2"))
                })
                .OrderBy(x=>x.Name)
                .ThenBy(x=>x.Id)
                .ToList();

            var json = JsonConvert.SerializeObject(prisoners, Formatting.Indented);

            return json;
        }

        public static string ExportPrisonersInbox(SoftJailDbContext context, string prisonersNames)
        {
            var inputNamesArray = prisonersNames.Split(",").ToArray();

            var prisoners = context
                .Prisoners
                .ToList()
                .Where(x => inputNamesArray.Contains(x.FullName))
                .Select(x => new ExportPrisonerDto
                {
                    Id = x.Id,
                    Name = x.FullName,
                    IncarcerationDate = x.IncarcerationDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
                    EncryptedMessages = x.Mails.Select(m => new ExportMessageDto
                    {
                        Description = string.Join("", m.Description.Reverse())
                    })
                    .ToArray()
                })
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Id)
                .ToArray();

            var xml = XmlConverter.Serialize(prisoners, "Prisoners");

            return xml;
        }
    }
}