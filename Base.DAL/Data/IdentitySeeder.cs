using Base.DAL.Contexts;
using Base.DAL.Models.BaseModels;
using Base.DAL.Models.HospitalModels;
using Base.DAL.Models.SystemModels;
using Base.Shared.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Base.DAL.Seeding
{
    public static class IdentitySeeder
    {
        public static async Task SeedAdminAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            // ── Seed Roles ────────────────────────────────────────────────
            var roleNames = Enum.GetNames<UserTypes>();
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                    await roleManager.CreateAsync(new IdentityRole(roleName));
            }

            // ── Seed System Admin ─────────────────────────────────────────
            string adminEmail = "admin@gmail.com";
            string adminPassword = "Admin@123";

            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    FullName = "System Admin",
                    Type = UserTypes.SystemAdmin,
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(adminUser, adminPassword);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, UserTypes.SystemAdmin.ToString());
            }
        }

        // ─────────────────────────────────────────────────────────────────
        public static async Task SeedLocationsAsync(AppDbContext context)
        {
            // Only seed if tables are empty
            if (await context.Governorates.AnyAsync()) return;

            // ── Governorates ──────────────────────────────────────────────
            var governorates = new List<Governorate>
            {
                new() { Id = "gov-cairo",      NameAr = "القاهرة",         NameEn = "Cairo"           },
                new() { Id = "gov-giza",       NameAr = "الجيزة",          NameEn = "Giza"            },
                new() { Id = "gov-alex",       NameAr = "الإسكندرية",      NameEn = "Alexandria"      },
                new() { Id = "gov-qalyubia",   NameAr = "القليوبية",       NameEn = "Qalyubia"        },
                new() { Id = "gov-sharqia",    NameAr = "الشرقية",         NameEn = "Sharqia"         },
                new() { Id = "gov-dakahlia",   NameAr = "الدقهلية",        NameEn = "Dakahlia"        },
                new() { Id = "gov-gharbia",    NameAr = "الغربية",         NameEn = "Gharbia"         },
                new() { Id = "gov-menoufia",   NameAr = "المنوفية",        NameEn = "Menoufia"        },
                new() { Id = "gov-kafr",       NameAr = "كفر الشيخ",      NameEn = "Kafr El-Sheikh"  },
                new() { Id = "gov-beheira",    NameAr = "البحيرة",         NameEn = "Beheira"         },
                new() { Id = "gov-ismailia",   NameAr = "الإسماعيلية",    NameEn = "Ismailia"        },
                new() { Id = "gov-suez",       NameAr = "السويس",          NameEn = "Suez"            },
                new() { Id = "gov-portsaid",   NameAr = "بورسعيد",        NameEn = "Port Said"       },
                new() { Id = "gov-damietta",   NameAr = "دمياط",           NameEn = "Damietta"        },
                new() { Id = "gov-fayoum",     NameAr = "الفيوم",          NameEn = "Fayoum"          },
                new() { Id = "gov-beni-suef",  NameAr = "بني سويف",       NameEn = "Beni Suef"       },
                new() { Id = "gov-minya",      NameAr = "المنيا",          NameEn = "Minya"           },
                new() { Id = "gov-asyut",      NameAr = "أسيوط",           NameEn = "Asyut"           },
                new() { Id = "gov-sohag",      NameAr = "سوهاج",           NameEn = "Sohag"           },
                new() { Id = "gov-qena",       NameAr = "قنا",             NameEn = "Qena"            },
                new() { Id = "gov-luxor",      NameAr = "الأقصر",          NameEn = "Luxor"           },
                new() { Id = "gov-aswan",      NameAr = "أسوان",           NameEn = "Aswan"           },
                new() { Id = "gov-redsea",     NameAr = "البحر الأحمر",   NameEn = "Red Sea"         },
                new() { Id = "gov-newvalley",  NameAr = "الوادي الجديد",  NameEn = "New Valley"      },
                new() { Id = "gov-matruh",     NameAr = "مطروح",           NameEn = "Matruh"          },
                new() { Id = "gov-northsinai", NameAr = "شمال سيناء",     NameEn = "North Sinai"     },
                new() { Id = "gov-southsinai", NameAr = "جنوب سيناء",     NameEn = "South Sinai"     },
            };

            await context.Governorates.AddRangeAsync(governorates);

            // ── Cities ────────────────────────────────────────────────────
            var cities = new List<City>
            {
                // Cairo
                new() { Id = "city-cairo-center",  GovernorateId = "gov-cairo",      NameAr = "القاهرة",         NameEn = "Cairo"            },
                new() { Id = "city-nasr",           GovernorateId = "gov-cairo",      NameAr = "مدينة نصر",       NameEn = "Nasr City"        },
                new() { Id = "city-heliopolis",     GovernorateId = "gov-cairo",      NameAr = "مصر الجديدة",    NameEn = "Heliopolis"       },
                new() { Id = "city-maadi",          GovernorateId = "gov-cairo",      NameAr = "المعادي",         NameEn = "Maadi"            },
                new() { Id = "city-zamalek",        GovernorateId = "gov-cairo",      NameAr = "الزمالك",         NameEn = "Zamalek"          },
                new() { Id = "city-shubra",         GovernorateId = "gov-cairo",      NameAr = "شبرا",            NameEn = "Shubra"           },
                new() { Id = "city-new-cairo",      GovernorateId = "gov-cairo",      NameAr = "القاهرة الجديدة", NameEn = "New Cairo"        },
                new() { Id = "city-6oct",           GovernorateId = "gov-cairo",      NameAr = "مدينة 6 أكتوبر", NameEn = "6th October"      },
                new() { Id = "city-ain-shams",      GovernorateId = "gov-cairo",      NameAr = "عين شمس",         NameEn = "Ain Shams"        },
                new() { Id = "city-abbassia",       GovernorateId = "gov-cairo",      NameAr = "العباسية",        NameEn = "Abbassia"         },
                new() { Id = "city-downtown-cairo", GovernorateId = "gov-cairo",      NameAr = "وسط البلد",       NameEn = "Downtown Cairo"   },

                // Giza
                new() { Id = "city-giza-center",    GovernorateId = "gov-giza",       NameAr = "الجيزة",          NameEn = "Giza"             },
                new() { Id = "city-dokki",          GovernorateId = "gov-giza",       NameAr = "الدقي",           NameEn = "Dokki"            },
                new() { Id = "city-mohandessin",    GovernorateId = "gov-giza",       NameAr = "المهندسين",       NameEn = "Mohandessin"      },
                new() { Id = "city-haram",          GovernorateId = "gov-giza",       NameAr = "الهرم",           NameEn = "Haram"            },
                new() { Id = "city-imbaba",         GovernorateId = "gov-giza",       NameAr = "إمبابة",          NameEn = "Imbaba"           },
                new() { Id = "city-sheikh-zayed",   GovernorateId = "gov-giza",       NameAr = "الشيخ زايد",     NameEn = "Sheikh Zayed"     },
                new() { Id = "city-agouza",         GovernorateId = "gov-giza",       NameAr = "العجوزة",         NameEn = "Agouza"           },
                new() { Id = "city-faisal",         GovernorateId = "gov-giza",       NameAr = "فيصل",            NameEn = "Faisal"           },
                new() { Id = "city-badrashin",      GovernorateId = "gov-giza",       NameAr = "البدرشين",        NameEn = "Badrashin"        },

                // Alexandria
                new() { Id = "city-alex-center",    GovernorateId = "gov-alex",       NameAr = "الإسكندرية",     NameEn = "Alexandria"       },
                new() { Id = "city-smouha",         GovernorateId = "gov-alex",       NameAr = "سموحة",           NameEn = "Smouha"           },
                new() { Id = "city-stanley",        GovernorateId = "gov-alex",       NameAr = "ستانلي",          NameEn = "Stanley"          },
                new() { Id = "city-miami-alex",     GovernorateId = "gov-alex",       NameAr = "ميامي",           NameEn = "Miami"            },
                new() { Id = "city-montaza",        GovernorateId = "gov-alex",       NameAr = "المنتزه",         NameEn = "Montaza"          },
                new() { Id = "city-agami",          GovernorateId = "gov-alex",       NameAr = "العجمي",          NameEn = "Agami"            },
                new() { Id = "city-sidi-gaber",     GovernorateId = "gov-alex",       NameAr = "سيدي جابر",      NameEn = "Sidi Gaber"       },
                new() { Id = "city-borg-arab",      GovernorateId = "gov-alex",       NameAr = "برج العرب",      NameEn = "Borg El-Arab"     },

                // Qalyubia
                new() { Id = "city-banha",          GovernorateId = "gov-qalyubia",   NameAr = "بنها",            NameEn = "Banha"            },
                new() { Id = "city-shubra-kheima",  GovernorateId = "gov-qalyubia",   NameAr = "شبرا الخيمة",    NameEn = "Shubra El-Kheima" },
                new() { Id = "city-obour",          GovernorateId = "gov-qalyubia",   NameAr = "العبور",          NameEn = "Obour"            },
                new() { Id = "city-qalyub",         GovernorateId = "gov-qalyubia",   NameAr = "قليوب",           NameEn = "Qalyub"           },

                // Sharqia
                new() { Id = "city-zagazig",        GovernorateId = "gov-sharqia",    NameAr = "الزقازيق",       NameEn = "Zagazig"          },
                new() { Id = "city-10th-ramadan",   GovernorateId = "gov-sharqia",    NameAr = "العاشر من رمضان", NameEn = "10th of Ramadan"  },
                new() { Id = "city-bilbeis",        GovernorateId = "gov-sharqia",    NameAr = "بلبيس",           NameEn = "Bilbeis"          },

                // Dakahlia
                new() { Id = "city-mansoura",       GovernorateId = "gov-dakahlia",   NameAr = "المنصورة",       NameEn = "Mansoura"         },
                new() { Id = "city-mit-ghamr",      GovernorateId = "gov-dakahlia",   NameAr = "ميت غمر",        NameEn = "Mit Ghamr"        },
                new() { Id = "city-talkha",         GovernorateId = "gov-dakahlia",   NameAr = "طلخا",            NameEn = "Talkha"           },

                // Gharbia
                new() { Id = "city-tanta",          GovernorateId = "gov-gharbia",    NameAr = "طنطا",            NameEn = "Tanta"            },
                new() { Id = "city-mahalla",        GovernorateId = "gov-gharbia",    NameAr = "المحلة الكبرى",  NameEn = "El-Mahalla"       },
                new() { Id = "city-kafr-zayat",     GovernorateId = "gov-gharbia",    NameAr = "كفر الزيات",     NameEn = "Kafr El-Zayat"    },

                // Menoufia
                new() { Id = "city-shibin",         GovernorateId = "gov-menoufia",   NameAr = "شبين الكوم",     NameEn = "Shibin El-Kom"    },
                new() { Id = "city-menouf",         GovernorateId = "gov-menoufia",   NameAr = "منوف",            NameEn = "Menouf"           },
                new() { Id = "city-sadat",          GovernorateId = "gov-menoufia",   NameAr = "مدينة السادات",  NameEn = "Sadat City"       },

                // Kafr El-Sheikh
                new() { Id = "city-kafr-center",    GovernorateId = "gov-kafr",       NameAr = "كفر الشيخ",      NameEn = "Kafr El-Sheikh"   },
                new() { Id = "city-desouq",         GovernorateId = "gov-kafr",       NameAr = "دسوق",            NameEn = "Desouq"           },
                new() { Id = "city-fuwwah",         GovernorateId = "gov-kafr",       NameAr = "فوه",             NameEn = "Fuwwah"           },

                // Beheira
                new() { Id = "city-damanhour",      GovernorateId = "gov-beheira",    NameAr = "دمنهور",          NameEn = "Damanhour"        },
                new() { Id = "city-kafr-dawwar",    GovernorateId = "gov-beheira",    NameAr = "كفر الدوار",     NameEn = "Kafr El-Dawwar"   },
                new() { Id = "city-rashid",         GovernorateId = "gov-beheira",    NameAr = "رشيد",            NameEn = "Rashid (Rosetta)" },

                // Ismailia
                new() { Id = "city-ismailia",       GovernorateId = "gov-ismailia",   NameAr = "الإسماعيلية",    NameEn = "Ismailia"         },
                new() { Id = "city-fayed",          GovernorateId = "gov-ismailia",   NameAr = "فايد",            NameEn = "Fayed"            },

                // Suez
                new() { Id = "city-suez",           GovernorateId = "gov-suez",       NameAr = "السويس",          NameEn = "Suez"             },
                new() { Id = "city-attaka",         GovernorateId = "gov-suez",       NameAr = "عتاقة",           NameEn = "Attaka"           },

                // Port Said
                new() { Id = "city-portsaid",       GovernorateId = "gov-portsaid",   NameAr = "بورسعيد",        NameEn = "Port Said"        },
                new() { Id = "city-port-fouad",     GovernorateId = "gov-portsaid",   NameAr = "بور فؤاد",       NameEn = "Port Fouad"       },

                // Damietta
                new() { Id = "city-damietta",       GovernorateId = "gov-damietta",   NameAr = "دمياط",           NameEn = "Damietta"         },
                new() { Id = "city-new-damietta",   GovernorateId = "gov-damietta",   NameAr = "دمياط الجديدة",  NameEn = "New Damietta"     },
                new() { Id = "city-faraskur",       GovernorateId = "gov-damietta",   NameAr = "فارسكور",         NameEn = "Faraskur"         },

                // Fayoum
                new() { Id = "city-fayoum",         GovernorateId = "gov-fayoum",     NameAr = "الفيوم",          NameEn = "Fayoum"           },
                new() { Id = "city-ibsheway",       GovernorateId = "gov-fayoum",     NameAr = "إبشواي",          NameEn = "Ibsheway"         },

                // Beni Suef
                new() { Id = "city-beni-suef",      GovernorateId = "gov-beni-suef",  NameAr = "بني سويف",       NameEn = "Beni Suef"        },
                new() { Id = "city-el-fashn",       GovernorateId = "gov-beni-suef",  NameAr = "الفشن",           NameEn = "El-Fashn"         },

                // Minya
                new() { Id = "city-minya",          GovernorateId = "gov-minya",      NameAr = "المنيا",          NameEn = "Minya"            },
                new() { Id = "city-mallawi",        GovernorateId = "gov-minya",      NameAr = "ملوي",            NameEn = "Mallawi"          },

                // Asyut
                new() { Id = "city-asyut",          GovernorateId = "gov-asyut",      NameAr = "أسيوط",           NameEn = "Asyut"            },
                new() { Id = "city-abnub",          GovernorateId = "gov-asyut",      NameAr = "أبنوب",           NameEn = "Abnub"            },

                // Sohag
                new() { Id = "city-sohag",          GovernorateId = "gov-sohag",      NameAr = "سوهاج",           NameEn = "Sohag"            },
                new() { Id = "city-akhmim",         GovernorateId = "gov-sohag",      NameAr = "أخميم",           NameEn = "Akhmim"           },

                // Qena
                new() { Id = "city-qena",           GovernorateId = "gov-qena",       NameAr = "قنا",             NameEn = "Qena"             },
                new() { Id = "city-nag-hammadi",    GovernorateId = "gov-qena",       NameAr = "نجع حمادي",      NameEn = "Nag Hammadi"      },

                // Luxor
                new() { Id = "city-luxor",          GovernorateId = "gov-luxor",      NameAr = "الأقصر",          NameEn = "Luxor"            },
                new() { Id = "city-armant",         GovernorateId = "gov-luxor",      NameAr = "أرمنت",           NameEn = "Armant"           },

                // Aswan
                new() { Id = "city-aswan",          GovernorateId = "gov-aswan",      NameAr = "أسوان",           NameEn = "Aswan"            },
                new() { Id = "city-kom-ombo",       GovernorateId = "gov-aswan",      NameAr = "كوم أمبو",        NameEn = "Kom Ombo"         },
                new() { Id = "city-edfu",           GovernorateId = "gov-aswan",      NameAr = "إدفو",            NameEn = "Edfu"             },

                // Red Sea
                new() { Id = "city-hurghada",       GovernorateId = "gov-redsea",     NameAr = "الغردقة",         NameEn = "Hurghada"         },
                new() { Id = "city-safaga",         GovernorateId = "gov-redsea",     NameAr = "سفاجا",           NameEn = "Safaga"           },
                new() { Id = "city-el-quseir",      GovernorateId = "gov-redsea",     NameAr = "القصير",          NameEn = "El-Quseir"        },

                // New Valley
                new() { Id = "city-kharga",         GovernorateId = "gov-newvalley",  NameAr = "الخارجة",         NameEn = "Kharga"           },
                new() { Id = "city-dakhla",         GovernorateId = "gov-newvalley",  NameAr = "الداخلة",         NameEn = "Dakhla"           },

                // Matruh
                new() { Id = "city-marsa-matruh",   GovernorateId = "gov-matruh",     NameAr = "مرسى مطروح",     NameEn = "Marsa Matruh"     },
                new() { Id = "city-siwa",           GovernorateId = "gov-matruh",     NameAr = "سيوة",            NameEn = "Siwa"             },

                // North Sinai
                new() { Id = "city-arish",          GovernorateId = "gov-northsinai", NameAr = "العريش",          NameEn = "Arish"            },
                new() { Id = "city-rafah",          GovernorateId = "gov-northsinai", NameAr = "رفح",             NameEn = "Rafah"            },

                // South Sinai
                new() { Id = "city-sharm",          GovernorateId = "gov-southsinai", NameAr = "شرم الشيخ",      NameEn = "Sharm El-Sheikh"  },
                new() { Id = "city-dahab",          GovernorateId = "gov-southsinai", NameAr = "دهب",             NameEn = "Dahab"            },
                new() { Id = "city-taba",           GovernorateId = "gov-southsinai", NameAr = "طابا",            NameEn = "Taba"             },
            };

            await context.Cities.AddRangeAsync(cities);
            await context.SaveChangesAsync();
        }
    }
}






//using Base.DAL.Contexts;
//using Base.DAL.Models.BaseModels;
//using Base.DAL.Models.SystemModels;
//using Base.Shared.DTOs;
//using Base.Shared.Enums;
//using Microsoft.AspNetCore.Identity;
//using System.Threading.Tasks;

//namespace Base.DAL.Seeding
//{
//    public static class IdentitySeeder
//    {
//        public static async Task SeedAdminAsync(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
//        {
//            var roleNames = Enum.GetNames<UserTypes>();
//            foreach (var roleName in roleNames)
//            {
//                if (!await roleManager.RoleExistsAsync(roleName))
//                    await roleManager.CreateAsync(new IdentityRole(roleName));
//            }

//            string adminEmail = "admin@gmail.com";
//            string adminPassword = "Admin@123";

//            var adminUser = await userManager.FindByEmailAsync(adminEmail);
//            if (adminUser == null)
//            {
//                adminUser = new ApplicationUser
//                {
//                    FullName = "System Admin",
//                    Type = UserTypes.SystemAdmin,
//                    UserName = adminEmail,
//                    Email = adminEmail,
//                    EmailConfirmed = true
//                };

//                var result = await userManager.CreateAsync(adminUser, adminPassword);

//                if (result.Succeeded)
//                {
//                    await userManager.AddToRoleAsync(adminUser, UserTypes.SystemAdmin.ToString());
//                }
//            }
//        }
//    }
//}