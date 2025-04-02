using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Models;

// класс контекста для работы с базой через Entity Framework
// управляет подключением к БД
public partial class BanksContext : DbContext
{
    // конструктор без параметров
    public BanksContext()
    {
    }

    // конструктор с настройками подключения БД
    public BanksContext(DbContextOptions<BanksContext> options)
        : base(options)
    {
    }

	// DbSet - коллекции, которые представляют таблицы в БД
	public virtual DbSet<ArgConsist> ArgConsists { get; set; }

    public virtual DbSet<Argument> Arguments { get; set; }

    public virtual DbSet<Bank> Banks { get; set; }

    public virtual DbSet<Data101> Data101s { get; set; }

    public virtual DbSet<DataNor> DataNors { get; set; }

    public virtual DbSet<FormInfo> FormInfos { get; set; }

    public virtual DbSet<Method> Methods { get; set; }

    public virtual DbSet<MethodsResult> MethodsResults { get; set; }

    public virtual DbSet<Templates101> Templates101s { get; set; }

    public virtual DbSet<TemplatesNor> TemplatesNors { get; set; }


   /* protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseNpgsql("Host=localhost;Database=banks;Username=admin;Password=password");
   */


    // метод для связывания таблиц и свойств моделей
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ArgConsist>(entity =>
        {
			// HasKey - первичный ключ
			entity.HasKey(e => e.IdArgConsist).HasName("arg_consist_pkey");

            // связывание таблицы с сущностью
			entity.ToTable("arg_consist", tb => tb.HasComment("Таблица содержит состав показателей методик"));


            entity.Property(e => e.IdArgConsist)
                .ValueGeneratedOnAdd()
                .HasComment("Id строки")
                .HasColumnName("id_arg_consist"); // задание имени колонки в БД
            entity.Property(e => e.IdArg)
                .HasComment("Id показателя")
                .HasColumnName("id_arg");
            entity.Property(e => e.IdT101)
                .HasComment("Id строки из 101 формы (БС2)")
                .HasColumnName("id_t101");
            entity.Property(e => e.IdTnor)
                .HasComment("Id строки из 123/135 форм (код норматива)")
                .HasColumnName("id_tnor");

            // создание внешнего ключа
            entity.HasOne(d => d.IdArgNavigation).WithMany(p => p.ArgConsists) // указание типа связи
                .HasForeignKey(d => d.IdArg) // указание внешнего ключа
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("arg_consist_id_arg_fkey"); // имя ограничения в таблице

			entity.HasOne(d => d.IdT101Navigation).WithMany(p => p.ArgConsists)
                .HasForeignKey(d => d.IdT101)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("arg_consist_id_t101_fkey"); 

            entity.HasOne(d => d.IdTnorNavigation).WithMany(p => p.ArgConsists)
                .HasForeignKey(d => d.IdTnor)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("arg_consist_id_tnor_fkey");
        });

        modelBuilder.Entity<Argument>(entity =>
        {
            entity.HasKey(e => e.IdArg).HasName("arguments_pkey");

            entity.ToTable("arguments", tb => tb.HasComment("Таблица содержит список показателей, входящих в методики"));

            entity.Property(e => e.IdArg)
                .ValueGeneratedOnAdd()
                .HasComment("Id показателя")
                .HasColumnName("id_arg");
            entity.Property(e => e.Consist)
                .HasComment("Из каких параметров отчетных форм состоит")
                .HasColumnName("consist");
            entity.Property(e => e.Descr)
                .HasComment("Описание показателя, принцип расчета, на основе чего строится")
                .HasColumnName("descr");
            entity.Property(e => e.Expression)
                .HasComment("Формула для расчета показателя")
                .HasColumnName("expression");
            entity.Property(e => e.IdMethods)
                .HasComment("Id методики, в которую включен показатель")
                .HasColumnName("id_methods");
            entity.Property(e => e.Name)
                .HasComment("Название показателя")
                .HasColumnName("name");
            entity.Property(e => e.ShortName)
                .HasComment("Краткий код, короткое название показателя")
                .HasColumnName("short_name");

            entity.HasOne(d => d.IdMethodsNavigation).WithMany(p => p.Arguments)
                .HasForeignKey(d => d.IdMethods)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("arguments_id_methods_fkey");
        });

        modelBuilder.Entity<Bank>(entity =>
        {
            entity.HasKey(e => e.Regnum).HasName("banks_pkey");

			entity.ToTable("banks", tb => tb.HasComment("Таблица содержит список банков и их регистрационные номера"));

            entity.Property(e => e.Regnum)
                .ValueGeneratedNever()
                .HasComment("Регистрационный номер банка")
                .HasColumnName("regnum");
            entity.Property(e => e.Name)
                .HasComment("Наименование банка")
                .HasColumnName("name");
        });

        modelBuilder.Entity<Data101>(entity =>
        {
            entity.HasKey(e => e.Id101).HasName("data101_pkey");

            entity.ToTable("data101", tb => tb.HasComment("Таблица содержит данные 101 формы для банка на дату"));

            entity.Property(e => e.Id101)
                .ValueGeneratedOnAdd()
                .HasComment("Id строки")
                .HasColumnName("id_101");
            entity.Property(e => e.IdInfo)
                .HasComment("Id шапки, т.е., для какого банка и на какую дату")
                .HasColumnName("id_info");
            entity.Property(e => e.IdT101)
                .HasComment("Id строки из 101 формы (БС2)")
                .HasColumnName("id_t101");
            entity.Property(e => e.Iitg)
                .HasPrecision(33)
                .HasComment("Исходящие остатки итого, тыс. руб")
                .HasColumnName("iitg");
            entity.Property(e => e.Vint)
                .HasPrecision(33)
                .HasComment("Входящие остатки итого, тыс. руб")
                .HasColumnName("vint");

            entity.HasOne(d => d.IdInfoNavigation).WithMany(p => p.Data101s)
                .HasForeignKey(d => d.IdInfo)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("data101_id_info_fkey");

            entity.HasOne(d => d.IdT101Navigation).WithMany(p => p.Data101s)
                .HasForeignKey(d => d.IdT101)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("data101_id_t101_fkey");
        });

        modelBuilder.Entity<DataNor>(entity =>
        {
            entity.HasKey(e => e.IdNor).HasName("data_nor_pkey");

            entity.ToTable("data_nor", tb => tb.HasComment("Таблица содержит данные 123/135 форм для банка на дату"));

            entity.Property(e => e.IdNor)
                .ValueGeneratedOnAdd()
                .HasComment("Id строки")
                .HasColumnName("id_nor");
            entity.Property(e => e.IdInfo)
                .HasComment("Id шапки, т.е., для какого банка и на какую дату")
                .HasColumnName("id_info");
            entity.Property(e => e.IdTnor)
                .HasComment("Id строки из 123/135 форм (код норматива)")
                .HasColumnName("id_tnor");
            entity.Property(e => e.Val)
                .HasPrecision(19)
                .HasComment("Значение норматива")
                .HasColumnName("val");

            entity.HasOne(d => d.IdInfoNavigation).WithMany(p => p.DataNors)
                .HasForeignKey(d => d.IdInfo)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("data_nor_id_info_fkey");

            entity.HasOne(d => d.IdTnorNavigation).WithMany(p => p.DataNors)
                .HasForeignKey(d => d.IdTnor)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("data_nor_id_tnor_fkey");
        });

        modelBuilder.Entity<FormInfo>(entity =>
        {
            entity.HasKey(e => e.IdInfo).HasName("form_info_pkey");

            entity.ToTable("form_info", tb => tb.HasComment("Таблица содержит шапку данных форм"));

            entity.Property(e => e.IdInfo)
                .ValueGeneratedOnAdd()
                .HasComment("Id строки")
                .HasColumnName("id_info");
            entity.Property(e => e.Dt)
                .HasComment("Дата, на которую расчитывается форма")
                .HasColumnName("dt");
            entity.Property(e => e.Regnum)
                .HasComment("Рег. номер банка, для которого расчитывается форма")
                .HasColumnName("regnum");

            entity.HasOne(d => d.RegnumNavigation).WithMany(p => p.FormInfos)
                .HasForeignKey(d => d.Regnum)
                .HasConstraintName("form_info_regnum_fkey");
        });

        modelBuilder.Entity<Method>(entity =>
        {
            entity.HasKey(e => e.IdMethods).HasName("methods_pkey");

            entity.ToTable("methods", tb => tb.HasComment("Таблица содержит список методик"));

			entity.Property(e => e.IdMethods)
                .ValueGeneratedOnAdd()
                .HasComment("Id методики")
                .HasColumnName("id_methods");
            entity.Property(e => e.Descr)
                .HasComment("Описание методики, ее история, основные принципы")
                .HasColumnName("descr");
            entity.Property(e => e.Name)
                .HasComment("Наименование методики")
                .HasColumnName("name");
        });

        modelBuilder.Entity<MethodsResult>(entity =>
        {
            entity.HasKey(e => e.IdRes).HasName("methods_result_pkey");

            entity.ToTable("methods_result", tb => tb.HasComment("Таблица содержит расчитанные показатели по методикам для банка на дату"));

            entity.Property(e => e.IdRes)
                .ValueGeneratedOnAdd()
                .HasComment("Id строки")
                .HasColumnName("id_res");
            entity.Property(e => e.IdArg)
                .HasComment("Id показателя")
                .HasColumnName("id_arg");
            entity.Property(e => e.IdInfo)
                .HasComment("Id шапки, т.е., для какого банка и на какую дату")
                .HasColumnName("id_info");
            entity.Property(e => e.Val)
                .HasPrecision(19)
                .HasComment("Значение показателя")
                .HasColumnName("val");

            entity.HasOne(d => d.IdArgNavigation).WithMany(p => p.MethodsResults)
                .HasForeignKey(d => d.IdArg)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("methods_result_id_arg_fkey");

            entity.HasOne(d => d.IdInfoNavigation).WithMany(p => p.MethodsResults)
                .HasForeignKey(d => d.IdInfo)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("methods_result_id_info_fkey");
        });

        modelBuilder.Entity<Templates101>(entity =>
        {
            entity.HasKey(e => e.IdT101).HasName("templates_101_pkey");

            entity.ToTable("templates_101", tb => tb.HasComment("Таблица содержит струкутру 101 формы"));

            entity.Property(e => e.IdT101)
                .ValueGeneratedOnAdd()
                .HasComment("Id строки")
                .HasColumnName("id_t101");
            entity.Property(e => e.AP)
                .HasMaxLength(1)
                .HasComment("Признак счета (А/П)")
                .HasColumnName("a_p");
            entity.Property(e => e.Name)
                .HasComment("Наименование БС2")
                .HasColumnName("name");
            entity.Property(e => e.NumSc)
                .HasMaxLength(5)
                .IsFixedLength()
                .HasComment("Номер счета 1-го, 2-го порядка или суммы БС2")
                .HasColumnName("num_sc");
            entity.Property(e => e.Plan)
                .HasMaxLength(1)
                .HasComment("Глава плана счетов (А, Б, В, Г, Д)")
                .HasColumnName("plan");
        });

        modelBuilder.Entity<TemplatesNor>(entity =>
        {
            entity.HasKey(e => e.IdTnor).HasName("templates_nor_pkey");

            entity.ToTable("templates_nor", tb => tb.HasComment("Таблица содержит струкутру 123 и 135 форм"));

            entity.Property(e => e.IdTnor)
                .ValueGeneratedOnAdd()
                .HasComment("Id строки")
                .HasColumnName("id_tnor");
            entity.Property(e => e.Code)
                .HasMaxLength(15)
                .IsFixedLength()
                .HasComment("Код показателя")
                .HasColumnName("code");
            entity.Property(e => e.Form)
                .HasMaxLength(3)
                .IsFixedLength()
                .HasComment("Номер формы (123/135)")
                .HasColumnName("form");
            entity.Property(e => e.Name)
                .HasComment("Наименование показателя")
                .HasColumnName("name");
        });

        OnModelCreatingPartial(modelBuilder);
    }

	// метод, если нужно дополнительно настроить модель в другом файле
	partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
