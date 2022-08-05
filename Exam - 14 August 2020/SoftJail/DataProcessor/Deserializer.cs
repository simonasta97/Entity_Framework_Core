namespace SoftJail.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using SoftJail.Data;
    using SoftJail.Data.Models;
    using SoftJail.DataProcessor.ImportDto;
    using Newtonsoft.Json;
    using System.Globalization;
    using VaporStore.XmlHelper;
    using SoftJail.Data.Models.Enums;

    public class Deserializer
    {
        public static string ImportDepartmentsCells(SoftJailDbContext context, string jsonString)
        {
            var departmentsDtos = JsonConvert.DeserializeObject<List<ImportDepartmentDto>>(jsonString);
            var sb = new StringBuilder();
            var departments = new HashSet<Department>();

            foreach (var departmentDto in departmentsDtos)
            {
                if (!IsValid(departmentDto)
                    || !departmentDto.Cells.Any()
                    || !departmentDto.Cells.All(IsValid))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var department = new Department
                {
                    Name = departmentDto.Name,
                    Cells = departmentDto.Cells.Select(c => new Cell
                    {
                        CellNumber = c.CellNumber,
                        HasWindow = c.HasWindow
                    })
                    .ToArray()
                };

                departments.Add(department);
                sb.AppendLine($"Imported {department.Name} with {department.Cells.Count} cells");
            }

            context.Departments.AddRange(departments);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportPrisonersMails(SoftJailDbContext context, string jsonString)
        {
            var prisonersDtos = JsonConvert.DeserializeObject<List<ImportPrisonerDto>>(jsonString);
            var sb = new StringBuilder();
            var prisoners = new HashSet<Prisoner>();

            foreach (var prisonerDto in prisonersDtos)
            {
                if (!IsValid(prisonerDto) || !prisonerDto.Mails.All(IsValid) || prisonerDto.Bail<0)
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var isValidReleaseDate = DateTime.TryParseExact(prisonerDto.ReleaseDate,
                    "dd/MM/yyyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime releaseDate);

                var prisoner = new Prisoner
                {
                    FullName = prisonerDto.FullName,
                    Nickname = prisonerDto.Nickname,
                    Age = prisonerDto.Age,
                    IncarcerationDate = DateTime.ParseExact(prisonerDto.IncarcerationDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None),
                    ReleaseDate = isValidReleaseDate ? (DateTime?)releaseDate : null,
                    Bail = prisonerDto.Bail,
                    CellId = prisonerDto.CellId,
                    Mails = prisonerDto.Mails.Select(x => new Mail
                    {
                        Description = x.Description,
                        Sender = x.Sender,
                        Address = x.Address
                    })
                    .ToArray()
                };

                prisoners.Add(prisoner);
                sb.AppendLine($"Imported {prisoner.FullName} {prisoner.Age} years old");
            }

            context.Prisoners.AddRange(prisoners);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }

        public static string ImportOfficersPrisoners(SoftJailDbContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var officers = XmlConverter.Deserializer<ImportOfficerDto>(xmlString, "Officers").ToList();

            foreach (var officer in officers)
            {
                if (!IsValid(officer))
                {
                    sb.AppendLine("Invalid Data");
                    continue;
                }

                var officerToAdd = new Officer
                {
                    FullName = officer.Name,
                    Salary = officer.Money,
                    Position = Enum.Parse<Position>(officer.Position),
                    Weapon = Enum.Parse<Weapon>(officer.Weapon),
                    DepartmentId = officer.DepartmentId,
                    OfficerPrisoners = officer.Prisoners
                    .Select(x => new OfficerPrisoner
                    {
                        PrisonerId = x.Id
                    })
                    .ToList()
                };

                

                context.Officers.Add(officerToAdd);
                context.SaveChanges();

                sb.AppendLine($"Imported {officerToAdd.FullName} ({officer.Prisoners.Length} prisoners)");
            }

            return sb.ToString().TrimEnd();

        }

        private static bool IsValid(object obj)
        {
            var validationContext = new System.ComponentModel.DataAnnotations.ValidationContext(obj);
            var validationResult = new List<ValidationResult>();

            bool isValid = Validator.TryValidateObject(obj, validationContext, validationResult, true);
            return isValid;
        }
    }
}