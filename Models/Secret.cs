using System;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Secrets.Models
{
    public class Secret
    {
        [Key]
        public int Id {get; set;}

        [Required]
        [MinLength(5)]
        public string Content {get; set;}

        public int Likes {get; set;}

        public int UserId {get; set;}

        public User User {get; set;}

        public List<Like> LikedUsers {get; set;}

        public DateTime Created_At {get; set;}
        
        public DateTime Updated_At {get; set;}

        public Secret()
        {
            LikedUsers = new List<Like>();
        }
    }
}