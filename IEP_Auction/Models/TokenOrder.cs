namespace IEP_Auction.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    public partial class TokenOrder
    {
        public Guid Id { get; set; }

        [StringLength(10)]
        public string Status { get; set; }

        [Required]
        [StringLength(128)]
        public string UserId { get; set; }

        public long Amount { get; set; }

        public DateTime Time { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }
    }
}
