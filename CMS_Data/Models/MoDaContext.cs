using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CMS_Data.Models;

public partial class MoDaContext : DbContext
{
    public MoDaContext()
    {
    }

    public MoDaContext(DbContextOptions<MoDaContext> options)
        : base(options)
    {
        if (Database.IsSqlite())
        {
            Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
        }
    }

    public virtual DbSet<TblAcccount> TblAcccounts { get; set; }

    public virtual DbSet<TblBillMoney> TblBillMoneys { get; set; }

    public virtual DbSet<TblCategoryPayment> TblCategoryPayments { get; set; }

    public virtual DbSet<TblCompany> TblCompanies { get; set; }

    public virtual DbSet<TblCustomer> TblCustomers { get; set; }

    public virtual DbSet<TblProduct> TblProducts { get; set; }

    public virtual DbSet<TblScale> TblScales { get; set; }

    public virtual DbSet<TblUnit> TblUnits { get; set; }

    public virtual DbSet<TblVehicle> TblVehicles { get; set; }

 

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblAcccount>(entity =>
        {
            entity.HasKey(e => e.UserName);

            entity.ToTable("tblAcccount");

            entity.Property(e => e.UserName).HasMaxLength(50);
            entity.Property(e => e.CreateBy).HasMaxLength(50);
            entity.Property(e => e.CreateDay).HasColumnType("TEXT");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.FullName).HasMaxLength(250);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.PassWord).HasMaxLength(250);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.UpdateBy).HasMaxLength(50);
            entity.Property(e => e.UpdateDay).HasColumnType("TEXT");
        });

        modelBuilder.Entity<TblBillMoney>(entity =>
        {
            entity.ToTable("tblBillMoney", tb => tb.HasTrigger("UpdateCode"));

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreateBy).HasMaxLength(50);
            entity.Property(e => e.CreateDay)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("TEXT");
            entity.Property(e => e.CustomerAddress).HasMaxLength(500);
            entity.Property(e => e.CustomerId).HasDefaultValue(0);
            entity.Property(e => e.CustomerName).HasMaxLength(500);
            entity.Property(e => e.CustomerPhone).HasMaxLength(50);
            entity.Property(e => e.Money).HasDefaultValue(0.0);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.ScaleId)
                .HasMaxLength(10)
                .HasDefaultValueSql("((0))")
                .IsFixedLength();
            entity.Property(e => e.TypeCate).HasDefaultValue(0);
            entity.Property(e => e.TypeId)
                .HasDefaultValue(0)
                .HasComment("Hình thức thu, chi");
            entity.Property(e => e.UpdateBy).HasMaxLength(50);
            entity.Property(e => e.UpdateDay).HasColumnType("TEXT");
        });

        modelBuilder.Entity<TblCategoryPayment>(entity =>
        {
            entity.ToTable("tblCategoryPayment");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreateBy).HasMaxLength(50);
            entity.Property(e => e.CreateDay)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("TEXT");
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.TypeId)
                .HasDefaultValue(1)
                .HasComment("1: Thu; 2: Chi");
            entity.Property(e => e.UpdateBy).HasMaxLength(50);
            entity.Property(e => e.UpdateDay).HasColumnType("TEXT");
        });

        modelBuilder.Entity<TblCompany>(entity =>
        {
            entity.HasKey(e => e.Code);

            entity.ToTable("tblCompany");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.AccOnline).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.CreateBy).HasMaxLength(50);
            entity.Property(e => e.CreateDay)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("TEXT");
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.PassOnline).HasMaxLength(50);
            entity.Property(e => e.Phone).HasMaxLength(500);
            entity.Property(e => e.UpdateBy).HasMaxLength(50);
            entity.Property(e => e.UpdateDay).HasColumnType("TEXT");
        });

        modelBuilder.Entity<TblCustomer>(entity =>
        {
            entity.ToTable("tblCustomer");

            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.BankAccount).HasMaxLength(50);
            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CompanyId).HasDefaultValue(0);
            entity.Property(e => e.CreateBy).HasMaxLength(50);
            entity.Property(e => e.CreateDay)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("TEXT");
            entity.Property(e => e.DebtFirst).HasDefaultValue(0.0);
            entity.Property(e => e.Email).HasMaxLength(50);
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.Note).HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(50);
            entity.Property(e => e.UpdateBy).HasMaxLength(50);
            entity.Property(e => e.UpdateDay).HasColumnType("TEXT");
        });

        modelBuilder.Entity<TblProduct>(entity =>
        {
            entity.ToTable("tblProduct");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreateBy).HasMaxLength(50);
            entity.Property(e => e.CreateDay)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("TEXT");
            entity.Property(e => e.Name).HasMaxLength(500);
            entity.Property(e => e.PriceInput).HasDefaultValue(0.0);
            entity.Property(e => e.PriceOutput).HasDefaultValue(0.0);
            entity.Property(e => e.Proportion)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 2)");
            entity.Property(e => e.TypeProduct).HasDefaultValue(0);
            entity.Property(e => e.UnitId).HasDefaultValue(0);
            entity.Property(e => e.UnitName).HasMaxLength(50);
            entity.Property(e => e.UpdateBy).HasMaxLength(50);
            entity.Property(e => e.UpdateDay).HasColumnType("TEXT");
        });

        modelBuilder.Entity<TblScale>(entity =>
        {
            entity.ToTable("tblScale", tb => tb.HasTrigger("upDateIndexInDay"));

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreateBy).HasMaxLength(50);
            entity.Property(e => e.CreateDay)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("TEXT");
            entity.Property(e => e.CustomerAddress).HasMaxLength(500);
            entity.Property(e => e.CustomerId).HasDefaultValue(0);
            entity.Property(e => e.CustomerName).HasMaxLength(500);
            entity.Property(e => e.CustomerPhone).HasMaxLength(50);
            entity.Property(e => e.DriverName).HasMaxLength(250);
            entity.Property(e => e.Exchange).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.Img11).HasMaxLength(500);
            entity.Property(e => e.Img12).HasMaxLength(500);
            entity.Property(e => e.Img13).HasMaxLength(500);
            entity.Property(e => e.Img21).HasMaxLength(500);
            entity.Property(e => e.Img22).HasMaxLength(500);
            entity.Property(e => e.Img23).HasMaxLength(500);
            entity.Property(e => e.IndexInDay).HasDefaultValue(0);
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.MoneyPayment).HasDefaultValue(0.0);
            entity.Property(e => e.Note).HasMaxLength(250);
            entity.Property(e => e.ProductId).HasDefaultValue(0);
            entity.Property(e => e.ProductMoney).HasDefaultValue(0.0);
            entity.Property(e => e.ProductName).HasMaxLength(250);
            entity.Property(e => e.ProductNumber).HasColumnType("decimal(18, 0)");
            entity.Property(e => e.ProductPrice).HasDefaultValue(0.0);
            entity.Property(e => e.Proportion).HasDefaultValue(0.0);
            entity.Property(e => e.StatusPayment).HasMaxLength(50);
            entity.Property(e => e.TimeIn).HasColumnType("TEXT");
            entity.Property(e => e.TimeOut).HasColumnType("TEXT");
            entity.Property(e => e.TopSeal).HasMaxLength(250);
            entity.Property(e => e.TotalMoney).HasDefaultValue(0.0);
            entity.Property(e => e.TypeId).HasDefaultValue(0);
            entity.Property(e => e.UpdateBy).HasMaxLength(50);
            entity.Property(e => e.UpdateDay).HasColumnType("TEXT");
            entity.Property(e => e.Vehicle).HasMaxLength(50);
            entity.Property(e => e.WeightImpurity).HasColumnType("decimal(18, 1)");
            entity.Property(e => e.WeightIn)
                .HasDefaultValue(0m)
                .HasColumnType("decimal(18, 0)");
            entity.Property(e => e.WeightOut).HasColumnType("decimal(18, 0)");
        });

        modelBuilder.Entity<TblUnit>(entity =>
        {
            entity.ToTable("tblUnit");

            entity.Property(e => e.Code).HasMaxLength(50);
            entity.Property(e => e.CreateBy).HasMaxLength(50);
            entity.Property(e => e.CreateDay)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("TEXT");
            entity.Property(e => e.Name).HasMaxLength(250);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.UpdateBy).HasMaxLength(50);
            entity.Property(e => e.UpdateDay).HasColumnType("TEXT");
        });

        modelBuilder.Entity<TblVehicle>(entity =>
        {
            entity.HasKey(e => e.VehiceCode);

            entity.ToTable("tblVehicle");

            entity.Property(e => e.VehiceCode).HasMaxLength(50);
            entity.Property(e => e.CreateBy).HasMaxLength(50);
            entity.Property(e => e.CreateDay)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("TEXT");
            entity.Property(e => e.DriverName).HasMaxLength(250);
            entity.Property(e => e.DriverPhone).HasMaxLength(50);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.RfidCode).HasMaxLength(50);
            entity.Property(e => e.Status).HasDefaultValue(true);
            entity.Property(e => e.TonnageDefault).HasDefaultValue(0.0);
            entity.Property(e => e.TransportationCompany).HasMaxLength(500);
            entity.Property(e => e.UpdateBy).HasMaxLength(50);
            entity.Property(e => e.UpdateDay).HasColumnType("TEXT");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
