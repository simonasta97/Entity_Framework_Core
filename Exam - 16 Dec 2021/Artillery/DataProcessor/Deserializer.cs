namespace Artillery.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using Artillery.Data;
    using Artillery.Data.Models;
    using Artillery.Data.Models.Enums;
    using Artillery.DataProcessor.ImportDto;
    using Newtonsoft.Json;
    using Theatre.XmlHelper;

    public class Deserializer
    {
        private const string ErrorMessage =
                "Invalid data.";
        private const string SuccessfulImportCountry =
            "Successfully import {0} with {1} army personnel.";
        private const string SuccessfulImportManufacturer =
            "Successfully import manufacturer {0} founded in {1}.";
        private const string SuccessfulImportShell =
            "Successfully import shell caliber #{0} weight {1} kg.";
        private const string SuccessfulImportGun =
            "Successfully import gun {0} with a total weight of {1} kg. and barrel length of {2} m.";

        public static string ImportCountries(ArtilleryContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var countriesDtos= XmlConverter.Deserializer<ImportCountryDto>(xmlString, "Countries").ToList();
            var countries = new HashSet<Country>();

            foreach (var countryDto in countriesDtos)
            {
                if (!IsValid(countryDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var country = new Country
                {
                    CountryName = countryDto.CountryName,
                    ArmySize = countryDto.ArmySize
                };

                countries.Add(country);
                sb.AppendLine(string.Format(SuccessfulImportCountry, country.CountryName, country.ArmySize));
            }
            context.Countries.AddRange(countries);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportManufacturers(ArtilleryContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var manufacturersDtos = XmlConverter.Deserializer<ImportManufacturerDto>(xmlString, "Manufacturers").ToList();
            var manufacturers = new HashSet<Manufacturer>();

            foreach (var manufacturerDto in manufacturersDtos)
            {
                if(IsValid(manufacturerDto) &&  !manufacturers.Any(x => x.ManufacturerName == manufacturerDto.ManufacturerName))
                {
                    var manufacturer = new Manufacturer
                    {
                        ManufacturerName = manufacturerDto.ManufacturerName,
                        Founded = manufacturerDto.Founded
                    };

                    manufacturers.Add(manufacturer);

                    var foundedLocation = manufacturer.Founded.Split(", ");
                    sb.AppendLine(String.Format(SuccessfulImportManufacturer
                        , manufacturer.ManufacturerName
                        , foundedLocation[foundedLocation.Length - 2] + ", " + foundedLocation[foundedLocation.Length - 1]));
                }
                else
                {
                    sb.AppendLine(ErrorMessage);
                }
            }
            context.Manufacturers.AddRange(manufacturers);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportShells(ArtilleryContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var shellsDtos = XmlConverter.Deserializer<ImportShellDto>(xmlString, "Shells").ToList();
            var shells = new HashSet<Shell>();

            foreach (var shellDto in shellsDtos)
            {
                if (!IsValid(shellDto))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }
                var shell = new Shell
                {
                    ShellWeight = shellDto.ShellWeight,
                    Caliber = shellDto.Caliber
                };

                shells.Add(shell);

                sb.AppendLine(string.Format(SuccessfulImportShell, shell.Caliber, shell.ShellWeight));
            }
            context.Shells.AddRange(shells);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportGuns(ArtilleryContext context, string jsonString)
        {
            var gunsDtos = JsonConvert.DeserializeObject<List<ImportGunDto>>(jsonString);
            var sb = new StringBuilder();
            var guns = new HashSet<Gun>();

            foreach (var gunDto in gunsDtos)
            {
                if (IsValid(gunDto) && Enum.TryParse(gunDto.GunType, out GunType gunType))
                {
                    var gun = new Gun
                    {
                        ManufacturerId = gunDto.ManufacturerId,
                        GunWeight = gunDto.GunWeight,
                        BarrelLength = gunDto.BarrelLength,
                        NumberBuild = gunDto.NumberBuild,
                        Range = gunDto.Range,
                        GunType = gunType,
                        ShellId=gunDto.ShellId,
                        CountriesGuns = gunDto.Countries.Select(x => new CountryGun
                        {
                            CountryId = x.Id
                        })
                        .ToList()
                    };
                    guns.Add(gun);

                    sb.AppendLine(String.Format(SuccessfulImportGun, gun.GunType.ToString(), gun.GunWeight, gun.BarrelLength));
                }
                else
                {
                    sb.AppendLine(ErrorMessage);
                }
            }
            context.Guns.AddRange(guns);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }
        private static bool IsValid(object obj)
        {
            var validator = new ValidationContext(obj);
            var validationRes = new List<ValidationResult>();

            var result = Validator.TryValidateObject(obj, validator, validationRes, true);
            return result;
        }
    }
}
