namespace IEP_Auction.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Reservation")]
    public partial class Reservation
    {
        [Key]
        [Column(Order = 0)]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long BidId { get; set; }

        [Key]
        [Column(Order = 1)]
        public string UserId { get; set; }

        public long? Amount { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }

        public virtual Bid Bid { get; set; }
    }
}
