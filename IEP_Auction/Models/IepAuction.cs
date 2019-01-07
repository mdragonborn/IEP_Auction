namespace IEP_Auction.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class IepAuction : DbContext
    {
        public IepAuction()
            : base("name=IepAuction")
        {
        }

        public virtual DbSet<Auction> Auction { get; set; }
        public virtual DbSet<Balance> Balance { get; set; }
        public virtual DbSet<Bid> Bid { get; set; }
        public virtual DbSet<BidAuction> BidAuction { get; set; }
        public virtual DbSet<Reservation> Reservation { get; set; }
        public virtual DbSet<TokenOrders> TokenOrders { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Auction>()
                .Property(e => e.Description)
                .IsUnicode(false);

            modelBuilder.Entity<Bid>()
                .HasMany(e => e.Auction)
                .WithOptional(e => e.Bid)
                .HasForeignKey(e => e.LastBidId);

            modelBuilder.Entity<Bid>()
                .HasOptional(e => e.BidAuction)
                .WithRequired(e => e.Bid);

            modelBuilder.Entity<Bid>()
                .HasMany(e => e.Reservation)
                .WithRequired(e => e.Bid)
                .WillCascadeOnDelete(false);
        }
    }
}
