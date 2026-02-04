using BookStoreApi.Constants;
using BookStoreApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BookStoreApi.Data;

public class DbSeeder
{
	public static async Task SeedDefaultData(IServiceProvider service)
	{
		try
		{
			var context = service.GetService<ApplicationDbContext>();

						if ((await context.Database.GetPendingMigrationsAsync()).Count() > 0)
			{
				await context.Database.MigrateAsync();
			}

			var userMgr = service.GetService<UserManager<IdentityUser>>();
			var roleMgr = service.GetService<RoleManager<IdentityRole>>();

						var adminRoleExists = await roleMgr.RoleExistsAsync(Roles.Admin.ToString());

			if (!adminRoleExists)
			{
				await roleMgr.CreateAsync(new IdentityRole(Roles.Admin.ToString()));
			}

						var userRoleExists = await roleMgr.RoleExistsAsync(Roles.User.ToString());

			if (!userRoleExists)
			{
				await roleMgr.CreateAsync(new IdentityRole(Roles.User.ToString()));
			}

			
			var admin = new IdentityUser
			{
				UserName = "admin@gmail.com",
				Email = "admin@gmail.com",
				EmailConfirmed = true
			};

			var userInDb = await userMgr.FindByEmailAsync(admin.Email);
			if (userInDb is null)
			{
				await userMgr.CreateAsync(admin, "Admin@123");
				await userMgr.AddToRoleAsync(admin, Roles.Admin.ToString());
			}


			if (!context.Genres.Any())
			{
				await SeedGenreAsync(context);
			}

			if (!context.Books.Any())
			{
				await SeedBooksAsync(context);
								await context.Database.ExecuteSqlRawAsync(@"
                     INSERT INTO Stock(BookId,Quantity) 
                     SELECT 
                     b.Id,
                     10 
                     FROM Book b
                     WHERE NOT EXISTS (
                     SELECT * FROM [Stock]
                     );
           ");
			}

			if (!context.orderStatuses.Any())
			{
				await SeedOrderStatusAsync(context);
			}
		}
		catch (Exception ex)
		{
			Console.WriteLine(ex.Message);
		}
	}

	#region private methods

	private static async Task SeedGenreAsync(ApplicationDbContext context)
	{
		var genres = new[]
		 {
			new Genre { GenreName = "Romance" },
			new Genre { GenreName = "Action" },
			new Genre { GenreName = "Thriller" },
			new Genre { GenreName = "Crime" },
			new Genre { GenreName = "SelfHelp" },
			new Genre { GenreName = "Programming" }
		};

		await context.Genres.AddRangeAsync(genres);
		await context.SaveChangesAsync();
	}

	private static async Task SeedOrderStatusAsync(ApplicationDbContext context)
	{
		var orderStatuses = new[]
		{
			new OrderStatus { StatusId = 1, StatusName = "Pending" },
			new OrderStatus { StatusId = 2, StatusName = "Shipped" },
			new OrderStatus { StatusId = 3, StatusName = "Delivered" },
			new OrderStatus { StatusId = 4, StatusName = "Cancelled" },
			new OrderStatus { StatusId = 5, StatusName = "Returned" },
			new OrderStatus { StatusId = 6, StatusName = "Refund" }
		};

		await context.orderStatuses.AddRangeAsync(orderStatuses);
		await context.SaveChangesAsync();
	}

	private static async Task SeedBooksAsync(ApplicationDbContext context)
	{
		var books = new List<Book>
		{
            			new Book { BookName = "Pride and Prejudice", AuthorName = "Jane Austen", Price = 312000, GenreId = 1 },
			new Book { BookName = "The Notebook", AuthorName = "Nicholas Sparks", Price = 288000, GenreId = 1 },
			new Book { BookName = "Outlander", AuthorName = "Diana Gabaldon", Price = 360000, GenreId = 1 },
			new Book { BookName = "Me Before You", AuthorName = "Jojo Moyes", Price = 264000, GenreId = 1 },
			new Book { BookName = "The Fault in Our Stars", AuthorName = "John Green", Price = 240000, GenreId = 1 },

						new Book { BookName = "The Bourne Identity", AuthorName = "Robert Ludlum", Price = 360000, GenreId = 2 },
			new Book { BookName = "Die Hard", AuthorName = "Roderick Thorp", Price = 336000, GenreId = 2 },
			new Book { BookName = "Jurassic Park", AuthorName = "Michael Crichton", Price = 384000, GenreId = 2 },
			new Book { BookName = "The Da Vinci Code", AuthorName = "Dan Brown", Price = 312000, GenreId = 2 },
			new Book { BookName = "The Hunger Games", AuthorName = "Suzanne Collins", Price = 288000, GenreId = 2 },

						new Book { BookName = "Gone Girl", AuthorName = "Gillian Flynn", Price = 288000, GenreId = 3 },
			new Book { BookName = "The Girl with the Dragon Tattoo", AuthorName = "Stieg Larsson", Price = 264000, GenreId = 3 },
			new Book { BookName = "The Silence of the Lambs", AuthorName = "Thomas Harris", Price = 312000, GenreId = 3 },
			new Book { BookName = "Before I Go to Sleep", AuthorName = "S.J. Watson", Price = 240000, GenreId = 3 },
			new Book { BookName = "The Girl on the Train", AuthorName = "Paula Hawkins", Price = 336000, GenreId = 3 },

						new Book { BookName = "The Godfather", AuthorName = "Mario Puzo", Price = 336000, GenreId = 4 },
			new Book { BookName = "The Girl with the Dragon Tattoo", AuthorName = "Stieg Larsson", Price = 312000, GenreId = 4 },
			new Book { BookName = "The Cuckoo's Calling", AuthorName = "Robert Galbraith", Price = 360000, GenreId = 4 },
			new Book { BookName = "In Cold Blood", AuthorName = "Truman Capote", Price = 288000, GenreId = 4 },
			new Book { BookName = "The Silence of the Lambs", AuthorName = "Thomas Harris", Price = 384000, GenreId = 4 },

						new Book { BookName = "The 7 Habits of Highly Effective People", AuthorName = "Stephen R. Covey", Price = 240000, GenreId = 5 },
			new Book { BookName = "How to Win Friends and Influence People", AuthorName = "Dale Carnegie", Price = 216000, GenreId = 5 },
			new Book { BookName = "Atomic Habits", AuthorName = "James Clear", Price = 264000, GenreId = 5 },
			new Book { BookName = "The Subtle Art of Not Giving a F*ck", AuthorName = "Mark Manson", Price = 192000, GenreId = 5 },
			new Book { BookName = "You Are a Badass", AuthorName = "Jen Sincero", Price = 288000, GenreId = 5 },

						new Book { BookName = "Clean Code", AuthorName = "Robert C. Martin", Price = 480000, GenreId = 6 },
			new Book { BookName = "Design Patterns", AuthorName = "Erich Gamma", Price = 432000, GenreId = 6 },
			new Book { BookName = "Code Complete", AuthorName = "Steve McConnell", Price = 528000, GenreId = 6 },
			new Book { BookName = "The Pragmatic Programmer", AuthorName = "Andrew Hunt", Price = 456000, GenreId = 6 },
			new Book { BookName = "Head First Design Patterns", AuthorName = "Eric Freeman", Price = 504000, GenreId = 6 }
		};

		await context.Books.AddRangeAsync(books);
		await context.SaveChangesAsync();
	}

	#endregion
}
