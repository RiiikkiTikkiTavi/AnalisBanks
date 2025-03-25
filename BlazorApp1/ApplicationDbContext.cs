using Microsoft.EntityFrameworkCore;

namespace BlazorApp1
{
	// класс контекста для работы с базой через Entity Framework
	// управляет подключением к БД
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

		//public DbSet<User> Users { get; set; }  // Примерная таблица
	}
	
}
