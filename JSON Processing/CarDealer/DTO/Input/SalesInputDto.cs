using System;
using System.Collections.Generic;
using System.Text;

namespace CarDealer.DTO.Input
{
    public class SalesInputDto
    {

        public int CarId { get; set; }

        public int CustomerId { get; set; }

        public decimal Discount { get; set; }
    }
}
