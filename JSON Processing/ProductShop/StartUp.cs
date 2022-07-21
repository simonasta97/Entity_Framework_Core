using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using ProductShop.Data;
using ProductShop.Models;
using ProductShop.Dtos.Input;
using AutoMapper;
using Newtonsoft.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace ProductShop
{
    public class StartUp
    {

        private static IMapper mapper;

        public static void Main(string[] args)
        {
            var context = new ProductShopContext();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
           
            string usersJsonAsString = File.ReadAllText("../../../Datasets/users.json");
            string productsJsonAsString = File.ReadAllText("../../../Datasets/products.json");
            string categoriesJsonAsString = File.ReadAllText("../../../Datasets/categories.json");
            string categoryProductsJsonAsString = File.ReadAllText("../../../Datasets/categories-products.json");

            Console.WriteLine(GetUsersWithProducts(context));
        }

        public static string ImportUsers(ProductShopContext context, string inputJson)
        {
            IEnumerable<UserInputDto> users = JsonConvert.DeserializeObject<IEnumerable<UserInputDto>>(inputJson);

            InitializeMapper();

            var mappedUsers = mapper.Map<IEnumerable<User>>(users);
            context.Users.AddRange(mappedUsers);
            context.SaveChanges();
            
            return $"Successfully imported {mappedUsers.Count()}";
        }

        public static string ImportProducts(ProductShopContext context, string inputJson)
        {
            IEnumerable<ProductInputDto> products = JsonConvert.DeserializeObject<IEnumerable<ProductInputDto>>(inputJson);
            InitializeMapper();

            var mappedProducts = mapper.Map<IEnumerable<Product>>(products);
            context.Products.AddRange(mappedProducts);
            context.SaveChanges();
            return $"Successfully imported {mappedProducts.Count()}";
        }

        public static string ImportCategories(ProductShopContext context, string inputJson)
        {
            IEnumerable<CategoryInputDto> categories = JsonConvert.DeserializeObject<IEnumerable<CategoryInputDto>>(inputJson)
                .Where(x => !string.IsNullOrEmpty(x.Name));
            InitializeMapper();

            var mappedCategories = mapper.Map<IEnumerable<Category>>(categories);
            context.Categories.AddRange(mappedCategories);
            context.SaveChanges();

            return $"Successfully imported {mappedCategories.Count()}";
        }

        public static string ImportCategoryProducts(ProductShopContext context, string inputJson)
        {
            IEnumerable<CategoryProductsInputDto> categoryProducts = JsonConvert.DeserializeObject<IEnumerable<CategoryProductsInputDto>>(inputJson);
            InitializeMapper();

            var mappedCategoryProducts = mapper.Map<IEnumerable<CategoryProduct>>(categoryProducts);
            context.CategoryProducts.AddRange(mappedCategoryProducts);
            context.SaveChanges();

            return $"Successfully imported {mappedCategoryProducts.Count()}";
        }

        private static void InitializeMapper()
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProductShopProfile>();
            });

            mapper = new Mapper(mapperConfiguration);
        }

        public static string GetProductsInRange(ProductShopContext context)
        {
            var products = context
                .Products
                .Where(p => p.Price >= 500 && p.Price <= 1000)
                .OrderBy(p => p.Price)
                .Select(x => new
                {
                    Name = x.Name,
                    Price = x.Price,
                    Seller = $"{x.Seller.FirstName} {x.Seller.LastName}"
                })
                .ToArray();

            DefaultContractResolver contractsResolver = new DefaultContractResolver {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = contractsResolver
            };

            string productsAsJson = JsonConvert.SerializeObject(products, jsonSettings);
            return productsAsJson;
        }

        public static string GetSoldProducts(ProductShopContext context)
        {
            var users = context
                .Users
                .Include(x=>x.ProductsSold)
                .Where(u => u.ProductsSold.Any(p=>p.Buyer!=null))
                .Select(x => new
                {
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    soldProducts = x.ProductsSold.Select(p => new
                    {
                        Name = p.Name,
                        Price = p.Price,
                        BuyerFirstName = p.Buyer.FirstName,
                        BuyerLastName = p.Buyer.LastName
                    })
                })
                .OrderBy(x=>x.LastName)
                .ThenBy(x=>x.FirstName)
                .ToArray();

            DefaultContractResolver contractsResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = contractsResolver
            };

            string usersAsJson = JsonConvert.SerializeObject(users, jsonSettings);
            return usersAsJson;
        }

        public static string GetCategoriesByProductsCount(ProductShopContext context)
        {
            var result = context
                .Categories
                .OrderByDescending(x => x.CategoryProducts.Count)
                .Select(x => new
                {
                    Category = x.Name,
                    ProductsCount = x.CategoryProducts.Count,
                    AveragePrice = $"{x.CategoryProducts.Average(cp => cp.Product.Price):f2}",
                    TotalRevenue = $"{x.CategoryProducts.Sum(cp => cp.Product.Price):f2}"
                })
                .ToArray();
            DefaultContractResolver contractsResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                ContractResolver = contractsResolver
            };

            string categories = JsonConvert.SerializeObject(result, jsonSettings);
            return categories;

        }

        public static string GetUsersWithProducts(ProductShopContext context)
        {
            var users = context.Users
                .Include(x => x.ProductsSold)
                .ToList()
                .Where(p => p.ProductsSold.Any(b => b.BuyerId != null))
                .Select(u => new
                {
                    firstName = u.FirstName,
                    lastName = u.LastName,
                    age = u.Age,
                    soldProducts = new
                    {
                        count = u.ProductsSold
                            .Where(x => x.BuyerId != null)
                            .Count(),
                        products = u.ProductsSold
                            .Where(x => x.BuyerId != null)
                            .Select(p => new
                            {
                                name = p.Name,
                                price = p.Price,
                            })
                    }
                })
                .OrderByDescending(x => x.soldProducts.count)
                .ToList();

            var resultObject = new
            {
                usersCount = users.Count(),
                users = users
            };

            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            var resultJson = JsonConvert.SerializeObject(resultObject, Formatting.Indented, jsonSettings);

            return resultJson;
        }
    }
}