using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Theatre.DataProcessor.ExportDto
{
    public class ExportTheatresDto
    {
        public string Name { get; set; }

        public sbyte Halls { get; set; }

        public decimal TotalIncome { get; set; }

        public ICollection<TicketsDto> Tickets { get; set; }
    }


    public class TicketsDto
    {
        public decimal Price { get; set; }

        public int RowNumber { get; set; }

    }
}

