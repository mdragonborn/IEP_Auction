namespace IEP_Auction.Models
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;
    using System.Web;

    public class AuctionViewModels : DbContext
    {
        // Your context has been configured to use a 'AuctionViewModels' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'IEP_Auction.Models.AuctionViewModels' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'AuctionViewModels' 
        // connection string in the application configuration file.
        public AuctionViewModels()
            : base("name=AuctionViewModels")
        {
        }

        // Add a DbSet for each entity type that you want to include in your model. For more information 
        // on configuring and using a Code First model, see http://go.microsoft.com/fwlink/?LinkId=390109.

        // public virtual DbSet<MyEntity> MyEntities { get; set; }
    }
    public class JoinedAuctionUsers
    {
        public Auction Auction { get; set; }
        public string Email { get; set; }
    }

    public class CreateAuctionModel
    {
        [Required]
        [Display(Name = "Auction length")]
        [DisplayFormat(DataFormatString = "{0:hh\\:mm}", ApplyFormatInEditMode = true)]
        public TimeSpan AuctionLength { get; set; }

        [Required]
        [Display(Name = "Name")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Required]
        [Display(Name = "Initial price")]
        public int InitialPrice { get; set; }

        [Required]
        [Display(Name= "Upload item image")]
        public HttpPostedFileBase File { get; set; }
    }
}