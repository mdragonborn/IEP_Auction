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

        public virtual DbSet<AspNetRole> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaim> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogin> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUser> AspNetUsers { get; set; }
        public virtual DbSet<Auction> Auctions { get; set; }
        public virtual DbSet<Balance> Balances { get; set; }
        public virtual DbSet<Bid> Bids { get; set; }
        public virtual DbSet<BidAuction> BidAuctions { get; set; }
        public virtual DbSet<Reservation> Reservations { get; set; }
        public virtual DbSet<TokenOrder> TokenOrders { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetRole>()
                .HasMany(e => e.AspNetUsers)
                .WithMany(e => e.AspNetRoles)
                .Map(m => m.ToTable("AspNetUserRoles").MapLeftKey("RoleId").MapRightKey("UserId"));

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.AspNetUserClaims)
                .WithRequired(e => e.AspNetUser)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.AspNetUserLogins)
                .WithRequired(e => e.AspNetUser)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.Auctions)
                .WithRequired(e => e.AspNetUser)
                .HasForeignKey(e => e.CreatorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AspNetUser>()
                .HasOptional(e => e.Balance)
                .WithRequired(e => e.AspNetUser);

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.Bids)
                .WithRequired(e => e.AspNetUser)
                .HasForeignKey(e => e.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.Reservations)
                .WithRequired(e => e.AspNetUser)
                .HasForeignKey(e => e.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AspNetUser>()
                .HasMany(e => e.TokenOrders)
                .WithRequired(e => e.AspNetUser)
                .HasForeignKey(e => e.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Auction>()
                .Property(e => e.Description)
                .IsUnicode(false);

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
        }
    }
}
