namespace IEP_Auction.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class PortalParameter
    {
        [Key]
        public string Name { get; set; }

        [StringLength(128)]
        public string Type { get; set; }

        public double? NumValue { get; set; }

        [StringLength(128)]
        public string StrValue { get; set; }
    }
}
