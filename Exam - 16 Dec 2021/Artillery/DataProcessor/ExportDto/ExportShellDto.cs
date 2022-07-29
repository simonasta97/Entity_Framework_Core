using Artillery.Data.Models.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Artillery.DataProcessor.ExportDto
{
    public class ExportShellDto
    {
        public double ShellWeight { get; set; }

        [Required]
        public string Caliber { get; set; }

        public ICollection<ExportGunDto> Guns { get; set; }
    }
    [JsonObject("Guns")]
    public class ExportGunDto 
    {
        public string GunType { get; set; }

        public int GunWeight { get; set; }

        public double BarrelLength { get; set; }

        public string Range { get; set; }
        
    }
}
