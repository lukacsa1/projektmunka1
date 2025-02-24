using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace webshop.Models;

public partial class WebshopContext : DbContext
{
    public WebshopContext()
    {
    }

    public WebshopContext(DbContextOptions<WebshopContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<Orderitem> Orderitems { get; set; }

    public virtual DbSet<Szamlazasicimek> Szamlazasicimeks { get; set; }

    public virtual DbSet<Termekek> Termekeks { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySQL("SERVER=localhost;PORT=3306;DATABASE=webshop;USER=root;PASSWORD=;SSL MODE=none;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("orders");

            entity.HasIndex(e => e.FelhasznaloId, "FelhasznaloId");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Datum)
                .HasDefaultValueSql("'current_timestamp()'")
                .HasColumnType("timestamp");
            entity.Property(e => e.FelhasznaloId).HasColumnType("int(11)");
            entity.Property(e => e.OrderNumber).HasMaxLength(8);
            entity.Property(e => e.Status).HasColumnType("int(1)");

            entity.HasOne(d => d.Felhasznalo).WithMany(p => p.Orders)
                .HasForeignKey(d => d.FelhasznaloId)
                .HasConstraintName("orders_ibfk_1");
        });

        modelBuilder.Entity<Orderitem>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("orderitems");

            entity.HasIndex(e => e.RendelésId, "RendelésId");

            entity.HasIndex(e => e.TermekId, "TermekId");

            entity.Property(e => e.Id).HasColumnType("int(11)");
            entity.Property(e => e.Darabszam).HasColumnType("int(11)");
            entity.Property(e => e.Meret).HasColumnType("enum('S','M','L','XL','XXL','XXXL')");
            entity.Property(e => e.RendelésId).HasColumnType("int(11)");
            entity.Property(e => e.TermekId).HasColumnType("int(11)");

            entity.HasOne(d => d.Rendelés).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.RendelésId)
                .HasConstraintName("orderitems_ibfk_1");

            entity.HasOne(d => d.Termek).WithMany(p => p.Orderitems)
                .HasForeignKey(d => d.TermekId)
                .HasConstraintName("orderitems_ibfk_2");
        });

        modelBuilder.Entity<Szamlazasicimek>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("szamlazasicimek");

            entity.Property(e => e.Id).HasColumnType("int(32)");
            entity.Property(e => e.Hazszam)
                .HasColumnType("int(32)")
                .HasColumnName("hazszam");
            entity.Property(e => e.Iranyitoszam)
                .HasColumnType("int(32)")
                .HasColumnName("iranyitoszam");
            entity.Property(e => e.Orszag).HasMaxLength(32);
            entity.Property(e => e.Utca)
                .HasMaxLength(64)
                .HasColumnName("utca");
            entity.Property(e => e.Varos).HasMaxLength(64);
        });

        modelBuilder.Entity<Termekek>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("termekek");

            entity.Property(e => e.Id).HasColumnType("int(32)");
            entity.Property(e => e.Ar)
                .HasColumnType("int(64)")
                .HasColumnName("ar");
            entity.Property(e => e.Kategoria)
                .HasMaxLength(32)
                .HasColumnName("kategoria");
            entity.Property(e => e.Kep)
                .HasMaxLength(32)
                .HasColumnName("kep");
            entity.Property(e => e.Meret)
                .HasMaxLength(64)
                .HasColumnName("meret");
            entity.Property(e => e.TermekNeve).HasMaxLength(64);
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.ToTable("user");

            entity.HasIndex(e => e.LoginName, "LoginNev");

            entity.HasIndex(e => e.SzamlazasiCimId, "fk_szamlazasiCim");

            entity.Property(e => e.Id).HasColumnType("int(32)");
            entity.Property(e => e.Active).HasColumnType("int(1)");
            entity.Property(e => e.Email).HasMaxLength(64);
            entity.Property(e => e.Hash)
                .HasMaxLength(64)
                .HasColumnName("HASH");
            entity.Property(e => e.LoginName).HasMaxLength(32);
            entity.Property(e => e.PermissionLevel).HasColumnType("int(11)");
            entity.Property(e => e.RegistarionDate)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("'current_timestamp()'")
                .HasColumnType("timestamp");
            entity.Property(e => e.Salt)
                .HasMaxLength(64)
                .HasColumnName("SALT");
            entity.Property(e => e.SzamlazasiCimId)
                .HasDefaultValueSql("'NULL'")
                .HasColumnType("int(32)")
                .HasColumnName("szamlazasiCimId");

            entity.HasOne(d => d.SzamlazasiCim).WithMany(p => p.Users)
                .HasForeignKey(d => d.SzamlazasiCimId)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_szamlazasiCim");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
