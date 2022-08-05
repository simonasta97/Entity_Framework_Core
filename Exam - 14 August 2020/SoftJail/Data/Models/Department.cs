namespace SoftJail.Data.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Department
    {
        public Department()
        {
            this.Cells = new HashSet<Cell>();
        }
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(25)]
        public string Name  { get; set; }

        public ICollection<Cell> Cells  { get; set; }
    }
}
