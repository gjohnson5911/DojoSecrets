using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Secrets.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Secrets.Controllers
{
    public class SecretController:Controller
    {
        public DojoContext context {get; set;}

        public SecretController(DojoContext cont)
        {
            context = cont;
        }

        [HttpGet("logreg")]
        public IActionResult Home()
        {
            //clears session and renders Home view of login/registration page
            HttpContext.Session.Clear();
            return View();
        }

        [HttpPost("Register")]
        public IActionResult Register(User newUser)
        {
            //verifies new user, then adds to DB. Sets session and redirects to secrets
            if(ModelState.IsValid)
            {
                //check to make sure username or email isn't already used
                User testUser = context.Users.FirstOrDefault(u=>u.Email == newUser.Email);
                User testUser2 = context.Users.FirstOrDefault(u=>u.Username == newUser.Username);
                if(testUser == null && testUser2 == null)
                {
                    //Hash password
                    PasswordHasher<User> hasher = new PasswordHasher<User>();
                    newUser.Password = hasher.HashPassword(newUser, newUser.Password);
                    //set created_at and updated_at
                    newUser.Created_At = DateTime.Now;
                    newUser.Updated_At = DateTime.Now;
                    //add user to DB and savechanges
                    context.Users.Add(newUser);
                    context.SaveChanges();
                    //pull user by email to get ID, then set session
                    User IdUser = context.Users.FirstOrDefault(user=>user.Email == newUser.Email);
                    HttpContext.Session.SetInt32("UserId", IdUser.Id);
                    //send to secrets
                    return RedirectToAction("secrets");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Username or Email is already taken. Please try again.");
                    return View("home");
                }                
            }
            else
            {
                return View("home");
            }
        }

        [HttpPost("Login")]
        public IActionResult Login(string email, string passwordAttempt)
        {
            //attempts to pull user from DB based on email, then verify passwordAttempt matches that users password. Sets session and redirects to secrets
            //see if user exists based on email
            User logUser = context.Users.FirstOrDefault(user => user.Email == email);
            if(logUser != null)
            {
                PasswordHasher<User> hasher = new PasswordHasher<User>();
                //test password
                if(hasher.VerifyHashedPassword(logUser, logUser.Password, passwordAttempt) !=0)
                {
                    //set session and redirect to secrets
                    HttpContext.Session.SetInt32("UserId", logUser.Id);
                    return RedirectToAction("Secrets");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Password was not correct.");
                }
            }
            else
            {
                ModelState.AddModelError(string.Empty, "No account with provided email address.");
            }
            return View("home");
        }

        [HttpGet("secrets")]
        public IActionResult Secrets()
        {
            //sets ViewBag data for User and a list of 5 most recent secrets. Renders Secret view
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            User sessionUser = context.Users.Include(u=>u.SecretsLiked).FirstOrDefault(u=>u.Id == userId);
            IEnumerable<Secret> secrets = context.Secrets.Include(s=>s.User).Include(s=>s.LikedUsers).OrderByDescending(s=>s.Created_At).Take(5);

            ViewBag.User = sessionUser;
            ViewBag.Secrets = secrets;
            return View();
        }

        [HttpPost("postsecret")]
        public IActionResult PostSecret(string msg)
        {
            //create secret and add to DB, redirect to secret
            //retrieve session User
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            User thisUser = context.Users.FirstOrDefault(u=>u.Id == userId);
            //create new secret
            Secret newSecret = new Secret {
                Content = msg,
                Likes = 1,
                UserId = userId,
                Created_At = DateTime.Now,
                Updated_At = DateTime.Now
            };
            //add secret to DB and savechanges
            context.Secrets.Add(newSecret);
            context.SaveChanges();
            //created user automatically likes their secret, so a like link needs to be established
            Like newLike = new Like {
                UserId = userId,
                SecretId = newSecret.Id,
                Created_At = DateTime.Now,
                Updated_At = DateTime.Now
            };
            //add like to DB and savechanges
            context.Likes.Add(newLike);
            context.SaveChanges();
            //retrieve like from DB and add to thisSecret and thisUser's List of likes
            thisUser.SecretsLiked.Add(newLike);
            newSecret.LikedUsers.Add(newLike);
            context.SaveChanges();
            return RedirectToAction("secrets");
        }

        [HttpGet("delete/{secretId}")]
        public IActionResult DeleteSecret(int secretId)
        {
            //remove secret and redirect to secret
            Secret delete = context.Secrets.FirstOrDefault(s=>s.Id == secretId);
            context.Secrets.Remove(delete);
            context.SaveChanges();
            return RedirectToAction("secrets");
        }

        [HttpGet("Liked/{secretId}")]
        public IActionResult LikeSecret(int secretId)
        {
            //retrieve user from session
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            User thisUser = context.Users.FirstOrDefault(u=>u.Id == userId);

            //retrieve secret from given Id
            Secret thisSecret = context.Secrets.FirstOrDefault(s=>s.Id == secretId);
            //create new like
            Like newLike = new Like {
                UserId = userId,
                User = thisUser,
                SecretId = thisSecret.Id,
                Secret = thisSecret,
                Created_At = DateTime.Now,
                Updated_At = DateTime.Now
            };
            //Add like to DB and savechanges
            context.Likes.Add(newLike);
            context.SaveChanges();
            //add to thisSecret and thisUser's list of likes, then savechanges
            thisUser.SecretsLiked.Add(newLike);
            thisSecret.LikedUsers.Add(newLike);
            thisSecret.Likes++;
            context.SaveChanges();
            //redirect to secrets
            return RedirectToAction("secrets");
        }

        [HttpGet("secrets/popular")]
        public IActionResult Popular()
        {
            //sets ViewBag data for a list of all secrets, sorted by most likes. Renders popular view
            int userId = (int)HttpContext.Session.GetInt32("UserId");
            User sessionUser = context.Users.Include(u=>u.SecretsLiked).FirstOrDefault(u=>u.Id == userId);
            IEnumerable<Secret> secrets = context.Secrets.Include(s=>s.User).Include(s=>s.LikedUsers).OrderByDescending(s=>s.Likes);

            ViewBag.User = sessionUser;
            ViewBag.Secrets = secrets;
            return View();
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            //log current user out and return to logreg
            HttpContext.Session.Clear();
            return RedirectToAction("Home");
        }
    }
}