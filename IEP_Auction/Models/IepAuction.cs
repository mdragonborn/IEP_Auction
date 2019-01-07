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

        public virtual DbSet<AspNetRoles> AspNetRoles { get; set; }
        public virtual DbSet<AspNetUserClaims> AspNetUserClaims { get; set; }
        public virtual DbSet<AspNetUserLogins> AspNetUserLogins { get; set; }
        public virtual DbSet<AspNetUsers> AspNetUsers { get; set; }
        public virtual DbSet<Auction> Auction { get; set; }
        public virtual DbSet<Balance> Balance { get; set; }
        public virtual DbSet<Bid> Bid { get; set; }
        public virtual DbSet<BidAuction> BidAuction { get; set; }
        public virtual DbSet<Reservation> Reservation { get; set; }
        public virtual DbSet<TokenOrders> TokenOrders { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AspNetRoles>()
                .HasMany(e => e.AspNetUsers)
                .WithMany(e => e.AspNetRoles)
                .Map(m => m.ToTable("AspNetUserRoles").MapLeftKey("RoleId").MapRightKey("UserId"));

            modelBuilder.Entity<AspNetUsers>()
                .HasMany(e => e.AspNetUserClaims)
                .WithRequired(e => e.AspNetUsers)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<AspNetUsers>()
                .HasMany(e => e.AspNetUserLogins)
                .WithRequired(e => e.AspNetUsers)
                .HasForeignKey(e => e.UserId);

            modelBuilder.Entity<AspNetUsers>()
                .HasMany(e => e.Auction)
                .WithRequired(e => e.AspNetUsers)
                .HasForeignKey(e => e.CreatorId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AspNetUsers>()
                .HasOptional(e => e.Balance)
                .WithRequired(e => e.AspNetUsers);

            modelBuilder.Entity<AspNetUsers>()
                .HasMany(e => e.Bid)
                .WithRequired(e => e.AspNetUsers)
                .HasForeignKey(e => e.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AspNetUsers>()
                .HasMany(e => e.Reservation)
                .WithRequired(e => e.AspNetUsers)
                .HasForeignKey(e => e.UserId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<AspNetUsers>()
                .HasMany(e => e.TokenOrders)
                .WithRequired(e => e.AspNetUsers)
                .HasForeignKey(e => e.UserId)
                .WillCascadeOnDelete(false);

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
