using Microsoft.AspNetCore.Identity;

namespace OnlineBookReader.Models.Seeding
{
    public class SeedData
    {
        public static async Task Initialize(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            if (!context.Books.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Khoa học" },
                    new Category { Name = "Văn học" },
                    new Category { Name = "Lịch sử" }
                };

                var authors = new List<Author>
                {
                    new Author { Name = "Tác giả 1" },
                    new Author { Name = "Tác giả 2" },
                    new Author { Name = "Tác giả 3" }
                };

                context.Categories.AddRange(categories);
                context.Authors.AddRange(authors);
                await context.SaveChangesAsync();

                var books = new List<Book>
                {
                    new Book
                    {
                        Title = "Thank Everything",
                        ShortDescription = "Deceptively simple, each page acknowledges the contributions of a different object (spoon, rock, beasts) or concept (summer, slumber, surprise), with each image ",
                        Content = "Nội dung cuốn sách...",
                        UrlImage = "/image/book-2.png",
                        AuthorId = authors[0].Id,
                        CategoryId = categories[0].Id
                    },
                    new Book
                    {
                        Title = "The Art City",
                        ShortDescription = "Mô tả ngắn về cuốn sách 2",
                        Content = "Nội dung cuốn sách",
                        UrlImage = "/image/book-1.png",
                        AuthorId = authors[1].Id,
                        CategoryId = categories[1].Id
                    },
                    new Book
                    {
                        Title = "Your name",
                        ShortDescription = "Mô tả ngắn về cuốn sách 3",
                        Content = "Nội dung cuốn sách",
                        UrlImage = "/image/book-3.png",
                        AuthorId = authors[2].Id,
                        CategoryId = categories[2].Id
                    },
                    new Book
                    {
                        Title = "Cuốn sách 4",
                        ShortDescription = "Mô tả ngắn về cuốn sách 4",
                        Content = "Nội dung cuốn sách",
                        UrlImage = "/image/book-4.png",
                        AuthorId = authors[1].Id,
                        CategoryId = categories[1].Id
                    },
                    new Book
                    {
                        Title = "Gratefull And Give Thank",
                        ShortDescription = "Mô tả ngắn về cuốn sách 5",
                        Content = "Nội dung cuốn sách",
                        UrlImage = "/image/book5.png",
                        AuthorId = authors[2].Id,
                        CategoryId = categories[1].Id
                    }
                };

                context.Books.AddRange(books);
                await context.SaveChangesAsync();
            }

            var roleExists = await roleManager.RoleExistsAsync("Admin");
            if (!roleExists)
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = "Admin" });
            }

            roleExists = await roleManager.RoleExistsAsync("User");
            if (!roleExists)
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = "User" });
            }

            var adminUser = await userManager.FindByEmailAsync("admin@example.com");
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = "NguyenA",
                    Email = "admin@gmail.com",
                    FullName = "Nguyễn Văn A"
                };

                await userManager.CreateAsync(adminUser, "Admin@1234");
            }

            if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            var regularUser = await userManager.FindByEmailAsync("user@example.com");
            if (regularUser == null)
            {
                regularUser = new ApplicationUser
                {
                    UserName = "LeThiB",
                    Email = "user@gmail.com",
                    FullName = "Lê Thị B"
                };

                await userManager.CreateAsync(regularUser, "User@1234");
            }

            if (!await userManager.IsInRoleAsync(regularUser, "User"))
            {
                await userManager.AddToRoleAsync(regularUser, "User");
            }
        }

    }
}
