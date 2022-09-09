namespace Footballers.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Text;
    using Data;
    using VaporStore.XmlHelper;
    using ImportDto;
    using System.Linq;
    using Data.Models;
    using System.Globalization;
    using Data.Models.Enums;
    using Newtonsoft.Json;

    public class Deserializer
    {
        private const string ErrorMessage = "Invalid data!";

        private const string SuccessfullyImportedCoach
            = "Successfully imported coach - {0} with {1} footballers.";

        private const string SuccessfullyImportedTeam
            = "Successfully imported team - {0} with {1} footballers.";

        public static string ImportCoaches(FootballersContext context, string xmlString)
        {
            var sb = new StringBuilder();

            var coachesDtos = XmlConverter.Deserializer<ImportCoachDto>(xmlString, "Coaches").ToList();
            var coaches = new HashSet<Coach>();

            foreach (var coachDto in coachesDtos)
            {
                if (!IsValid(coachDto) || string.IsNullOrEmpty(coachDto.Nationality))
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var coach = new Coach
                {
                    Name = coachDto.Name,
                    Nationality = coachDto.Nationality
                };

                var footballers = new HashSet<Footballer>();

                foreach (var footballerDto in coachDto.Footballers)
                {
                    if (!IsValid(footballerDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    bool isContractStartDateValid = DateTime.TryParseExact(footballerDto.ContractStartDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime contractStartDate);
                    bool isContractEndDateValid = DateTime.TryParseExact(footballerDto.ContractEndDate, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime contractEndDate);

                    if (!isContractStartDateValid || !isContractEndDateValid || contractStartDate>contractEndDate)
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var footballer = new Footballer
                    {
                        Name=footballerDto.Name,
                        ContractStartDate=contractStartDate,
                        ContractEndDate=contractEndDate,
                        BestSkillType = (BestSkillType)footballerDto.BestSkillType,
                        PositionType=(PositionType)footballerDto.PositionType
                    };

                    footballers.Add(footballer);
                }

                coach.Footballers = footballers;
                coaches.Add(coach);
                sb.AppendLine(string.Format(SuccessfullyImportedCoach, coach.Name, coach.Footballers.Count));
            }

            context.Coaches.AddRange(coaches);
            context.SaveChanges();

            return sb.ToString().TrimEnd();
        }
        public static string ImportTeams(FootballersContext context, string jsonString)
        {
            var teamsDtos = JsonConvert.DeserializeObject<List<ImportTeamDto>>(jsonString);
            var sb = new StringBuilder();
            var teams = new HashSet<Team>();

            foreach (var teamDto in teamsDtos)
            {

                if (!IsValid(teamDto) || teamDto.Trophies<= 0)
                {
                    sb.AppendLine(ErrorMessage);
                    continue;
                }

                var team = new Team
                {
                    Name = teamDto.Name,
                    Nationality = teamDto.Nationality,
                    Trophies = teamDto.Trophies
                };

                var uniqueFootballers = teamDto.Footballers.Distinct().ToList();

                foreach (var footballerDto in uniqueFootballers)
                {
                    if (!context.Footballers.Any(x => x.Id == footballerDto) || !IsValid(footballerDto))
                    {
                        sb.AppendLine(ErrorMessage);
                        continue;
                    }

                    var teamFootballer = new TeamFootballer()
                    {
                        FootballerId = footballerDto,
                        Team = team
                    };

                    team.TeamsFootballers.Add(teamFootballer);

                }

                teams.Add(team);
                sb.AppendLine(string.Format(SuccessfullyImportedTeam, team.Name, team.TeamsFootballers.Count));
            }

            context.Teams.AddRange(teams);
            context.SaveChanges();
            return sb.ToString().TrimEnd();
        }

        private static bool IsValid(object dto)
        {
            var validationContext = new ValidationContext(dto);
            var validationResult = new List<ValidationResult>();

            return Validator.TryValidateObject(dto, validationContext, validationResult, true);
        }
    }
}
