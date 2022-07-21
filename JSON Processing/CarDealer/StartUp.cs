using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using CarDealer.Data;
using CarDealer.DTO.Input;
using CarDealer.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace CarDealer
{
    public class StartUp
    {
        private static IMapper mapper;

        public static void Main()
        {
            var context = new CarDealerContext();

            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();

            string suppliersJsonAsString = File.ReadAllText("../../../Datasets/suppliers.json");
            string partsJsonAsString = File.ReadAllText("../../../Datasets/parts.json");
            string carsJsonAsString = File.ReadAllText("../../../Datasets/cars.json");
            string salesJsonAsString = File.ReadAllText("../../../Datasets/sales.json");

            Console.WriteLine(GetCarsWithTheirListOfParts(context));
        }

        public static string ImportSuppliers(CarDealerContext context, string inputJson)
        {
            IEnumerable<SupplierInputDto> suppliers = JsonConvert.DeserializeObject<IEnumerable<SupplierInputDto>>(inputJson);

            InitializeMapper();

            var mappedSuppliers = mapper.Map<IEnumerable<Supplier>>(suppliers);
            context.Suppliers.AddRange(mappedSuppliers);
            context.SaveChanges();
            return $"Successfully imported {mappedSuppliers.Count()}.";
        }

        public static string ImportParts(CarDealerContext context, string inputJson)
        {
            var supplierIds = context.Suppliers
                .Select(x => x.Id)
                .ToArray();

            IEnumerable<PartInputDto> parts = JsonConvert.DeserializeObject<IEnumerable<PartInputDto>>(inputJson)
                .Where(s => supplierIds.Contains(s.SupplierId))
                .ToList(); ;

            InitializeMapper();

            var mappedParts = mapper.Map<IEnumerable<Part>>(parts);
            context.Parts.AddRange(mappedParts);
            context.SaveChanges();
            return $"Successfully imported {mappedParts.Count()}.";
        }

        public static string ImportCars(CarDealerContext context, string inputJson)
        {
            IEnumerable<CarInputDto> cars = JsonConvert.DeserializeObject<IEnumerable<CarInputDto>>(inputJson);

            InitializeMapper();

            var mappedCars = mapper.Map<IEnumerable<Car>>(cars);
            context.Cars.AddRange(mappedCars);
            context.SaveChanges();
            return $"Successfully imported {mappedCars.Count()}.";
        }

        public static string ImportCustomers(CarDealerContext context, string inputJson)
        {
            IEnumerable<CustumerInputDto> customers = JsonConvert.DeserializeObject<IEnumerable<CustumerInputDto>>(inputJson);

            InitializeMapper();

            var mappedCustomers = mapper.Map<IEnumerable<Customer>>(customers);
            context.Customers.AddRange(mappedCustomers);
            context.SaveChanges();
            return $"Successfully imported {mappedCustomers.Count()}.";
        }

        public static string ImportSales(CarDealerContext context, string inputJson)
        {
            IEnumerable<SalesInputDto> sales = JsonConvert.DeserializeObject<IEnumerable<SalesInputDto>>(inputJson);

            InitializeMapper();

            var mappedSales = mapper.Map<IEnumerable<Sale>>(sales);
            context.Sales.AddRange(mappedSales);
            context.SaveChanges();
            return $"Successfully imported {mappedSales.Count()}.";
        }

        private static void InitializeMapper()
        {
            var mapperConfiguration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<CarDealerProfile>();
            });

            mapper = new Mapper(mapperConfiguration);
        }


        public static string GetOrderedCustomers(CarDealerContext context)
        {
            var custumers = context
                .Customers
                .OrderBy(x => x.BirthDate)
                .ThenBy(x => x.IsYoungDriver)
                .Select(c => new
                {
                    Name = c.Name,
                    BirthDate = c.BirthDate.ToString("dd/MM/yyyy"),
                    IsYoungDriver = c.IsYoungDriver
                })
                .ToArray();


            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            string custumersAsJson = JsonConvert.SerializeObject(custumers, jsonSettings);
            return custumersAsJson;
        }

        public static string GetCarsFromMakeToyota(CarDealerContext context)
        {
            var cars = context
                .Cars
                .Where(x => x.Make == "Toyota")
                .OrderBy(x => x.Model)
                .ThenByDescending(x => x.TravelledDistance)
                .Select(x => new
                {
                    Id = x.Id,
                    Make = x.Make,
                    Model = x.Model,
                    TravelledDistance = x.TravelledDistance
                })
                .ToArray();

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            string carsAsJson = JsonConvert.SerializeObject(cars, jsonSettings);
            return carsAsJson;
        }

        public static string GetLocalSuppliers(CarDealerContext context)
        {
            var suppliers = context
                .Suppliers
                .Where(x => !x.IsImporter)
                .Select(x => new
                {
                    Id = x.Id,
                    Name = x.Name,
                    PartsCount = x.Parts.Count
                })
                .ToArray();


            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            string suppliersAsJson = JsonConvert.SerializeObject(suppliers, jsonSettings);
            return suppliersAsJson;
        }

        public static string GetCarsWithTheirListOfParts(CarDealerContext context)
        {
            var cars = context
                .Cars
                .Select(x => new
                {
                    car = new
                    {
                        Make = x.Make,
                        Model = x.Model,
                        TravelledDistance = x.TravelledDistance
                    },
                    parts = x.PartCars.Select(p => new
                    {
                        Name = p.Part.Name,
                        Price = $"{p.Part.Price:F2}"
                    })
                })
                .ToArray();

            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            string carsAsJson = JsonConvert.SerializeObject(cars, jsonSettings);
            return carsAsJson;
        }

        public static string GetTotalSalesByCustomer(CarDealerContext context)
        {
            var customers = context
                .Customers
                .Where(x => x.Sales.Any())
                .Select(x => new
                {
                    fullName = x.Name,
                    boughtCars = x.Sales.Count,
                    spentMoney = x.Sales.Sum(s => s.Car.PartCars.Sum(p => p.Part.Price))
                })
                .OrderByDescending(x => x.spentMoney)
                .ThenByDescending(x => x.boughtCars)
                .ToList();


            var jsonSettings = new JsonSerializerSettings
            {
                Formatting = Formatting.Indented
            };

            string customersAsJson = JsonConvert.SerializeObject(customers, jsonSettings);
            return customersAsJson;
        }

        public static string GetSalesWithAppliedDiscount(CarDealerContext context)
        {
            var info = context.Sales
                .Select(s => new
                {
                    car = new
                    {
                        Make = s.Car.Make,
                        Model = s.Car.Model,
                        TravelledDistance = s.Car.TravelledDistance,
                    },
                    customerName = s.Customer.Name,
                    Discount = s.Discount.ToString("F2"),
                    price = s.Car.PartCars.Sum(pc => pc.Part.Price).ToString("F2"),
                    priceWithDiscount = ((s.Car.PartCars.Sum(pc => pc.Part.Price)) * (1 - s.Discount * 0.01m)).ToString("F2"),
                })
                .Take(10)
                .ToList();

            var json = JsonConvert.SerializeObject(info, Formatting.Indented);

            return json;
        }
    }
}