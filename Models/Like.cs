using System;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Secrets.Models
{
    public class Like
    {
        [Key]
        public int Id {get; set;}

        public int UserId {get; set;}
        
        public User User {get; set;}

        public int SecretId {get; set;}

        public Secret Secret {get; set;}

        public DateTime Created_At {get; set;}
        
        public DateTime Updated_At {get; set;}
    }
}