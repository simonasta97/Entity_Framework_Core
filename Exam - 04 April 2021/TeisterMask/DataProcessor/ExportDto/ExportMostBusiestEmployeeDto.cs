using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace TeisterMask.DataProcessor.ExportDto
{
    public class ExportMostBusiestEmployeeDto
    {
        public string Username { get; set; }

        public List<ExportTaskDto> Tasks { get; set; }

    }
    [JsonObject("Tasks")]
    public class ExportTaskDto
    {
        public string TaskName { get; set; }

        public string OpenDate { get; set; }

        public string  DueDate{ get; set; }

        public string LabelType { get; set; }

        public string ExecutionType { get; set; }
    }
}
