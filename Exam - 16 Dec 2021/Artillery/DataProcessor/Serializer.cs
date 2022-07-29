
namespace Artillery.DataProcessor
{
    using Artillery.Data;
    using Artillery.Data.Models.Enums;
    using Artillery.DataProcessor.ExportDto;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using System;
    using System.Linq;
    using Theatre.XmlHelper;

    public class Serializer
    {
        public static string ExportShells(ArtilleryContext context, double shellWeight)
        {
            var shells=context
                .Shells
                .Include(x => x.Guns)
                .ToList()
                .Where(x=>x.ShellWeight>shellWeight)
                .Select(x=> new ExportShellDto {
                    ShellWeight=x.ShellWeight,
                    Caliber=x.Caliber,
                    Guns = x.Guns.Where(x => x.GunType.ToString() == "AntiAircraftGun").Select(x => new ExportGunDto
                    {
                        GunType = x.GunType.ToString(),
                        GunWeight = x.GunWeight,
                        BarrelLength = x.BarrelLength,
                        Range = x.Range > 3000 ? "Long-range" : "Regular range",
                    })
                    .OrderByDescending(x => x.GunWeight)
                    .ToList()
                })
                .OrderBy(x => x.ShellWeight)
                .ToList();

            var json = JsonConvert.SerializeObject(shells, Formatting.Indented);

            return json;
        }

        public static string ExportGuns(ArtilleryContext context, string manufacturer)
        {
            var guns = context
                .Guns
                .Include(x => x.CountriesGuns)
                .ToList()
                .Where(x => x.Manufacturer.ManufacturerName == manufacturer)
                .Select(x => new ExportGunsDto
                {
                    Manufacturer = x.Manufacturer.ManufacturerName,
                    GunType = x.GunType.ToString(),
                    GunWeight = x.GunWeight,
                    BarrelLength = x.BarrelLength,
                    Range = x.Range,
                    Countries = x.CountriesGuns.Where(t => t.Country.ArmySize > 4500000).Select(t => new ExportCountryDto
                    {
                        Country = t.Country.CountryName,
                        ArmySize = t.Country.ArmySize
                    })
                    .OrderBy(t => t.ArmySize)
                    .ToArray()
                })
                .OrderBy(x => x.BarrelLength)
                .ToList();

            var xml = XmlConverter.Serialize(guns, "Guns");

            return xml;
        }
    }
}
