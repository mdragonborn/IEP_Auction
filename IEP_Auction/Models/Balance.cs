namespace IEP_Auction.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Balance")]
    public partial class Balance
    {
        public string Id { get; set; }

        public long? Tokens { get; set; }

        public virtual AspNetUsers AspNetUsers { get; set; }
    }
}
