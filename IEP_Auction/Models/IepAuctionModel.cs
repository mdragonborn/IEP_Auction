namespace IEP_Auction.Models
{
    using System;
    using System.Data.Entity;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public partial class IepAuctionModel : DbContext
    {
        public IepAuctionModel()
            : base("name=AuctionModels")
        {
        }

        public virtual DbSet<Auction> Auctions { get; set; }
        public virtual DbSet<Balance> Balances { get; set; }
        public virtual DbSet<Bid> Bids { get; set; }
        public virtual DbSet<BidAuction> BidAuctions { get; set; }
        public virtual DbSet<Reservation> Reservations { get; set; }
        public virtual DbSet<TokenOrder> TokenOrders { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Auction>()
                .Property(e => e.TimeStart)
                .IsFixedLength();

            modelBuilder.Entity<Bid>()
                .Property(e => e.Time)
                .IsFixedLength();

            modelBuilder.Entity<Bid>()
                .HasMany(e => e.Auctions)
                .WithOptional(e => e.Bid)
                .HasForeignKey(e => e.LastBidId);

            modelBuilder.Entity<Bid>()
                .HasOptional(e => e.BidAuction)
                .WithRequired(e => e.Bid);

            modelBuilder.Entity<Bid>()
                .HasMany(e => e.Reservations)
                .WithRequired(e => e.Bid)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<TokenOrder>()
                .Property(e => e.Time)
                .IsFixedLength();
        }
    }
}
