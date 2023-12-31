﻿using HeatQuizAPI.Database;
using HeatQuizAPI.Models.BaseModels;
using HeatQuizAPI.Models.LevelsOfDifficulty;
using heatquizapp_api.Models.BaseModels;
using Microsoft.AspNetCore.Identity;

namespace HeatQuizAPI.Services
{
    public interface ISeedDatabase
    {
        void SeedLevelsOfDifficulty();
        void SeedRolesAndFirstUsers();
        void SeedDatapools();
    }

    public class SeedDatabase : ISeedDatabase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        //Please change later
        private readonly string _adminPassword = "AdminHeatQuiz1234!";
        private readonly string _hquserPassword = "EditorHeatQuiz1234!";

        public SeedDatabase(
            ApplicationDbContext context,
            UserManager<User> userManager,
            RoleManager<IdentityRole> roleManager
            )
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //Function to seed roles and first users "admin" - "hq_user"
        public void SeedRolesAndFirstUsers()
        {
            var roles = new List<string>() { "admin", "course_editor" };

            foreach (var role in roles)
            {
                //Check role already exists
                var roleExists = _roleManager.RoleExistsAsync(role).Result;

                if (!roleExists)
                {
                    _roleManager.CreateAsync(new IdentityRole()
                    {
                        Name = role.ToLower(),
                        NormalizedName = role.ToUpper()
                    }).Wait();
                }
            }
                
            //Create users 
            var Admin = new User()
            {
                Name = "Admin",
                UserName = "admin",
                NormalizedUserName = "admin".ToUpper(),
                Email = "admin@rwth-aachen.de",
                NormalizedEmail = "admin@rwth-aachen.de".ToUpper(),
                RegisteredOn = DateTime.UtcNow
            };

            var HQUser = new User()
            {
                Name = "HQUser",
                UserName = "hq_user",
                NormalizedUserName = "hq_user".ToUpper(),
                Email = "hq_user@rwth-aachen.de",
                NormalizedEmail = "hq_user@rwth-aachen.de".ToUpper(),
                RegisteredOn = DateTime.UtcNow,
                ProfilePicture=""
            };

            //Check users already exist
            var adminExists = _userManager.FindByNameAsync("admin").Result != null;
            var hqUserExists = _userManager.FindByNameAsync("hq_user").Result != null;

            //Set users with passwords and roles
            if (!adminExists)
            {
                _userManager.CreateAsync(Admin, _adminPassword).Wait();
                _userManager.AddToRoleAsync(Admin, roles[0].ToLower()).Wait();
            }

            if (!hqUserExists)
            {
                _userManager.CreateAsync(HQUser, _hquserPassword).Wait();
                _userManager.AddToRoleAsync(HQUser, roles[1].ToLower()).Wait();
            }

        }

        //Function to seed datapools
        public void SeedDatapools()
        {
            const string datapoolName = "Heat Transfer";

            //Check datapool already exists
            var datapoolExists = _context.DataPools.Any(dp => dp.Name == datapoolName || dp.NickName == datapoolName);

            if(!datapoolExists)
            {
                _context.DataPools.Add(new DataPool()
                {
                    Name = datapoolName,
                    NickName = datapoolName,
                    IsHidden = false,
                    DateCreated = DateTime.UtcNow,
                    DateModified = DateTime.UtcNow,
                });

                _context.SaveChangesAsync().Wait();
            }            
        }

        //Function to seed levels of difficulty
        public void SeedLevelsOfDifficulty()
        {
            var LODNames = new List<string>() { "Easy", "Medium", "Hard" };
            var LODColors = new List<string>() { "#417505", "#f8e71c", "#d0021b" };

            for(var i = 0; i< LODNames.Count; i++)
            {
                var name = LODNames[i];
                var color = LODColors[i];

                //Check lod already exists
                var LODExists = _context.LevelsOfDifficulty.Any(l => l.Name == name || l.HexColor == color);

                if (!LODExists)
                {
                    //Add lod
                    _context.LevelsOfDifficulty.Add(new LevelOfDifficulty()
                    {
                        Name = name,
                        HexColor = color,
                    });
                }
            }

            _context.SaveChangesAsync().Wait();

            var LeftValues = new List<string>() { "+", "-", "Zero"};

            foreach(var leftValue in LeftValues)
            {
                _context.LeftGradientValues.Add(new heatquizapp_api.Models.InterpretedTrees.LeftGradientValue()
                {
                    DataPoolId = 1,
                    Value = leftValue,
                });

                _context.SaveChanges();
            }

            var RightValues = new List<string>() { "+", "-", "Zero" };

            foreach (var leftValue in RightValues)
            {
                _context.RightGradientValues.Add(new heatquizapp_api.Models.InterpretedTrees.RightGradientValue()
                {
                    DataPoolId = 1,
                    Value = leftValue,
                });

                _context.SaveChanges();
            }

            var JumpValues = new List<string>() { ">", "NO JUMP", "<" };

            foreach (var leftValue in JumpValues)
            {
                _context.JumpValues.Add(new heatquizapp_api.Models.InterpretedTrees.JumpValue()
                {
                    DataPoolId = 1,
                    Value = leftValue,
                });

                _context.SaveChanges();
            }

            var RatioValies = new List<string>() { ">", "=", "<" };

            foreach (var leftValue in RatioValies)
            {
                _context.RationOfGradientsValues.Add(new heatquizapp_api.Models.InterpretedTrees.RationOfGradientsValue()
                {
                    DataPoolId = 1,
                    Value = leftValue,
                });

                _context.SaveChanges();
            }
        }
    }

   
}
