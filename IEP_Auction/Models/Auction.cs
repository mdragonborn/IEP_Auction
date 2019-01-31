namespace IEP_Auction.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

    [Table("Auction")]
    public partial class Auction
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public Auction()
        {
            BidAuctions = new HashSet<BidAuction>();
        }

        public Guid Id { get; set; }

        [Required]
        [StringLength(128)]
        public string CreatorId { get; set; }

        [Required]
        [StringLength(10)]
        public string Status { get; set; }

        public int DurationMinutes { get; set; }

        public DateTime? TimeStart { get; set; }

        public DateTime? TimeEnd { get; set; }

        public long? LastBidId { get; set; }

        [Required]
        [StringLength(128)]
        public string Name { get; set; }

        [Column(TypeName = "text")]
        [Required]
        public string Description { get; set; }

        [StringLength(256)]
        public string ImagePath { get; set; }

        public byte[] ImageFile { get; set; }

        public virtual AspNetUser AspNetUser { get; set; }

        public virtual Bid Bid { get; set; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<BidAuction> BidAuctions { get; set; }
    }
}
