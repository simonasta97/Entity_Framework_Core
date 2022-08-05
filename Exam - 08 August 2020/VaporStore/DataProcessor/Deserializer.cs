namespace VaporStore.DataProcessor
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Linq;
    using System.Text;
    using VaporStore.Data;
    using VaporStore.Data.Models;
    using VaporStore.DataProcessor.Dto.Import;
    using Newtonsoft.Json;
    using System.Globalization;
    using VaporStore.XmlHelper;

    public static class Deserializer
	{
		public static string ImportGames(VaporStoreDbContext context, string jsonString)
		{
			var gamesDtos = JsonConvert.DeserializeObject<List<ImportGameDto>>(jsonString);
			var sb = new StringBuilder();
			var games = new HashSet<Game>();

            foreach (var gameDto in gamesDtos)
            {
				if(gameDto.Price<0 || !IsValid(gameDto) || gameDto.Tags.Length<=0)
                {
					sb.AppendLine("Invalid Data");
					continue;
                }

				var genre = context.Genres.FirstOrDefault(x => x.Name == gameDto.Genre)
					?? new Genre { Name = gameDto.Genre };

				var developer = context.Developers.FirstOrDefault(x => x.Name == gameDto.Developer)
					?? new Developer { Name = gameDto.Developer };


				var game = new Game
				{
					Name = gameDto.Name,
					Price = gameDto.Price,
					ReleaseDate = DateTime.ParseExact(gameDto.ReleaseDate, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None),
					Developer = developer,
					Genre = genre
				};

                foreach (var tagDto in gameDto.Tags)
                {
					var tag = context.Tags.FirstOrDefault(x => x.Name == tagDto)
						?? new Tag { Name = tagDto };

					game.GameTags.Add(new GameTag { Tag = tag });
				}

				games.Add(game);
				sb.AppendLine($"Added {gameDto.Name} ({gameDto.Genre}) with {gameDto.Tags.Count()} tags");
			}
			context.Games.AddRange(games);
			context.SaveChanges();
			return sb.ToString().TrimEnd();
		}

		public static string ImportUsers(VaporStoreDbContext context, string jsonString)
		{
			var usersDtos = JsonConvert.DeserializeObject<List<ImportUserDto>>(jsonString);
			var sb = new StringBuilder();
			var users = new HashSet<User>();

            foreach (var userDto in usersDtos)
            {
				if (!IsValid(userDto) ||
					!userDto.Cards.All(IsValid) ||
					!userDto.Cards.Any())
				{
					sb.AppendLine("Invalid Data");
					continue;
				}

				var user = new User
				{
					Username = userDto.Username,
					FullName = userDto.FullName,
					Email = userDto.Email,
					Age = userDto.Age,
					Cards = userDto.Cards.Select(x => new Card
					{
						Number = x.Number,
						Cvc = x.CVC,
						Type = x.Type.Value,
					})
					.ToList()
				};

				users.Add(user);
				sb.AppendLine($"Imported {user.Username} with {user.Cards.Count} cards");
			}
			context.Users.AddRange(users);
			context.SaveChanges();

			return sb.ToString().TrimEnd();
		}

		public static string ImportPurchases(VaporStoreDbContext context, string xmlString)
		{
			var sb = new StringBuilder();

			var purchases = XmlConverter.Deserializer<ImportPurchaseDto>(xmlString, "Purchases").ToList();

			foreach (var xmlPurchase in purchases)
			{
				if (!IsValid(xmlPurchase))
				{
					sb.AppendLine("Invalid Data");
					continue;
				}

				var parsedDate = DateTime.TryParseExact(
					xmlPurchase.Date, "dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date);

				if (!parsedDate)
				{
					sb.AppendLine("Invalid Data");
					continue;
				}

				var purchase = new Purchase
				{
					Type = xmlPurchase.Type.Value,
					ProductKey = xmlPurchase.Key,
					Date = date,
					Card = context.Cards.FirstOrDefault(x => x.Number == xmlPurchase.Card),
					Game = context.Games.FirstOrDefault(x => x.Name == xmlPurchase.Title)
				};

				var username = context.Users.Where(x => x.Id == purchase.Card.UserId).Select(x => x.Username).FirstOrDefault();

				context.Purchases.Add(purchase);
				context.SaveChanges();

				sb.AppendLine($"Imported {xmlPurchase.Title} for {username}");
			}

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