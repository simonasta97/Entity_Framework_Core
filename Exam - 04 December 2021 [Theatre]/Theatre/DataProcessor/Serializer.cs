namespace Theatre.DataProcessor
{
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using System;
    using System.Linq;
    using System.Text;
    using System.Xml.Serialization;
    using Theatre.Data;
    using Theatre.DataProcessor.ExportDto;
    using Theatre.XmlHelper;

    public class Serializer
    {
        public static string ExportTheatres(TheatreContext context, int numbersOfHalls)
        {

            var theatres = context
                .Theatres
                .ToList()
                .Where(x => x.NumberOfHalls >= numbersOfHalls && x.Tickets.Count() >= 20)
                .Select(x => new ExportTheatresDto
                {
                    Name = x.Name,
                    Halls = x.NumberOfHalls,
                    TotalIncome = x.Tickets.Where(t => t.RowNumber >= 1 && t.RowNumber <= 5).Sum(t => t.Price),
                    Tickets = x.Tickets
                        .Where(t => t.RowNumber >= 1 && t.RowNumber <= 5)
                        .Select(t => new TicketsDto
                        {
                            Price = t.Price,
                            RowNumber = t.RowNumber
                        })
                        .OrderByDescending(t => t.Price)
                        .ToArray()
                })
                .OrderByDescending(x => x.Halls)
                .ThenBy(x => x.Name)
                .ToArray();

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                
            };

            string theatresAsJson = JsonConvert.SerializeObject(theatres, jsonSettings);
            return theatresAsJson;
        }

        public static string ExportPlays(TheatreContext context, double rating)
        {
            var plays = context.Plays
                .Include(x => x.Casts)
                .ToList()
                .Where(x => x.Rating <= rating)
                .Select(x => new ExportPlaysDto
                {
                    Title = x.Title,
                    Duration = x.Duration.ToString("c"),
                    Rating = x.Rating == 0 ? "Rating" : x.Rating.ToString(),
                    Genre = x.Genre.ToString(),
                    Actors = context.Casts.Where(c => c.IsMainCharacter && c.Play.Title == x.Title).Select(s => new ActorDto
                    {
                        FullName = s.FullName,
                        MainCharacter = $"Plays main character in '{x.Title}'."
                    })
                    .OrderByDescending(x => x.FullName)
                    .ToList()
                })
                .OrderBy(x => x.Title)
                .ThenByDescending(x => x.Genre)
                .ToList();

            var xml = XmlConverter.Serialize(plays, "Plays");

            return xml;
        }
    }
}
