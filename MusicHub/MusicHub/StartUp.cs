using MusicHub.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MusicHub
{
    public class StartUp
    {
        public static void Main()
        {

            MusicHubDbContext context =
                new MusicHubDbContext();

            DbInitializer.ResetDatabase(context);

            var result = ExportAlbumsInfo(context, 4);

            Console.WriteLine(result);
        }

        public static string ExportAlbumsInfo(MusicHubDbContext context, int producerId)
        {
            StringBuilder output = new StringBuilder();

            var allAlbums = context
                .Albums
                .Where(a => a.ProducerId == producerId)
                .Select(x => new
                {
                    AlbumName = x.Name,
                    ReleaseDate = x.ReleaseDate.ToString("MM/dd/yyyy"),
                    ProducerName = x.Producer.Name,
                    Songs = x.Songs
                        .Select(s => new
                        {
                            SongName = s.Name,
                            Price = s.Price,
                            Writer = s.Writer.Name
                        })
                        .OrderByDescending(s => s.SongName)
                        .ThenBy(s => s.Writer),
                    AlbumPrice = x.Price
                })
                .OrderByDescending(a => a.AlbumPrice)
                .ToArray();

            foreach (var a in allAlbums)
            {
                output.AppendLine($"-AlbumName: {a.AlbumName}");
                output.AppendLine($"-ReleaseDate: {a.ReleaseDate}");
                output.AppendLine($"-ProducerName: {a.ProducerName}");
                output.AppendLine("-Songs:");
                int num = 0;
                foreach (var s in a.Songs)
                {
                    num++;
                    output.AppendLine($"---#{num}");
                    output.AppendLine($"---SongName: {s.SongName}");
                    output.AppendLine($"---Price: {s.Price:f2}");
                    output.AppendLine($"---Writer: {s.Writer}");
                }
                output.AppendLine($"-AlbumPrice: {a.AlbumPrice:F2}");
            }

            return output.ToString().TrimEnd();
        }
    }
}
