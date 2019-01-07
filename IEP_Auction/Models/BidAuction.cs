namespace IEP_Auction.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("BidAuction")]
    public partial class BidAuction
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public long BidId { get; set; }

        public Guid? AuctionId { get; set; }

        public virtual Auction Auction { get; set; }

        public virtual Bid Bid { get; set; }
    }
}
