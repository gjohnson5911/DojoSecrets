using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace Secrets.Models
{
    public class User
    {
        [Key]
        public int Id {get; set;}
        
        [Required]
        [MinLength(2)]
        public string Username {get; set;}

        [Required]
        [EmailAddress]
        public string Email {get; set;}

        [Required]
        [DataType(DataType.Password)]
        [MinLength(8)]
        public string Password {get; set;}

        [NotMapped]
        [DataType(DataType.Password)]
        [Compare("Password")]        
        public string Verify {get; set;}

        public List<Like> SecretsLiked {get; set;}

        public DateTime Created_At {get; set;}

        public DateTime Updated_At {get; set;}

        public User()
        {
            SecretsLiked = new List<Like>();
        }
    }
}