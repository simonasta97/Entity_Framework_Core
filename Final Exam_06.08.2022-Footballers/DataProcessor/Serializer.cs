namespace Footballers.DataProcessor
{
    using System;
    using System.Globalization;
    using System.Linq;
    using Data;
    using Newtonsoft.Json;
    using Formatting = Newtonsoft.Json.Formatting;
    using Footballers.DataProcessor.ExportDto;
    using VaporStore.XmlHelper;
    using Microsoft.EntityFrameworkCore;

    public class Serializer
    {
        public static string ExportCoachesWithTheirFootballers(FootballersContext context)
        {
            var coaches = context
                .Coaches
                .ToArray()
                .Where(x => x.Footballers.Count >= 1)
                .Select(x => new ExportCoachDto
                {
                    FootballersCount = x.Footballers.Count,
                    CoachName = x.Name,
                    Footballers = x.Footballers.Select(f => new ExportFootballerDto
                    {
                        Name = f.Name,
                        Position = f.PositionType.ToString()
                    })
                    .OrderBy(f => f.Name)
                    .ToArray()
                })
                .OrderByDescending(x => x.FootballersCount)
                .ThenBy(x => x.CoachName)
                 .ToArray();

            var xml = XmlConverter.Serialize(coaches, "Coaches");

            return xml;
        }

        public static string ExportTeamsWithMostFootballers(FootballersContext context, DateTime date)
        {
            
            var teams = context
                .Teams
                .Include(x => x.TeamsFootballers)
                .ThenInclude(x=>x.Footballer)
                .ToList()
                .Where(x => x.TeamsFootballers.Any(x => x.Footballer.ContractStartDate >= date))
                .Select(x=>new 
                {
                    Name=x.Name,
                    Footballers=x.TeamsFootballers.Where(x => x.Footballer.ContractStartDate>= date).OrderByDescending(x => x.Footballer.ContractEndDate).ThenBy(x => x.Footballer.Name)
                    .Select(x=>new 
                    {
                        FootballerName= x.Footballer.Name,
                        ContractStartDate=x.Footballer.ContractStartDate.ToString("d", CultureInfo.InvariantCulture),
                        ContractEndDate = x.Footballer.ContractEndDate.ToString("d", CultureInfo.InvariantCulture),
                        BestSkillType= x.Footballer.BestSkillType.ToString(),
                        PositionType= x.Footballer.PositionType.ToString()
                    })
                    .ToList()
                })
                .OrderByDescending(x=>x.Footballers.Count)
                .ThenBy(x=>x.Name)
                .Take(5)
                .ToArray();

            var json = JsonConvert.SerializeObject(teams, Formatting.Indented);

            return json;
        }
    }
}
