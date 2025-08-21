using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CMS_Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tblAcccount",
                columns: table => new
                {
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PassWord = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreateDay = table.Column<DateTime>(type: "datetime", nullable: true),
                    CreateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdateDay = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblAcccount", x => x.UserName);
                });

            migrationBuilder.CreateTable(
                name: "tblBillMoney",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ScaleId = table.Column<string>(type: "nchar(10)", fixedLength: true, maxLength: 10, nullable: true, defaultValueSql: "((0))"),
                    TypeId = table.Column<int>(type: "int", nullable: true, defaultValue: 0, comment: "Hình thức thu, chi"),
                    TypeCate = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CustomerId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CustomerName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomerAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Money = table.Column<double>(type: "float", nullable: true, defaultValue: 0.0),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreateDay = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdateDay = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblBillMoney", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblCategoryPayment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TypeId = table.Column<int>(type: "int", nullable: true, defaultValue: 1, comment: "1: Thu; 2: Chi"),
                    CreateDay = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdateDay = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCategoryPayment", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblCompany",
                columns: table => new
                {
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    AccOnline = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassOnline = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreateDay = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdateDay = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCompany", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "tblCustomer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DebtFirst = table.Column<double>(type: "float", nullable: true, defaultValue: 0.0),
                    BankAccount = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Note = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CreateDay = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdateDay = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblCustomer", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblProduct",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UnitId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    UnitName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Proportion = table.Column<decimal>(type: "decimal(18,2)", nullable: true, defaultValue: 0m),
                    PriceInput = table.Column<double>(type: "float", nullable: true, defaultValue: 0.0),
                    PriceOutput = table.Column<double>(type: "float", nullable: true, defaultValue: 0.0),
                    TypeProduct = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    CreateDay = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdateDay = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblProduct", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblScale",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Vehicle = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DriverName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TimeIn = table.Column<DateTime>(type: "datetime", nullable: true),
                    WeightIn = table.Column<decimal>(type: "decimal(18,0)", nullable: true, defaultValue: 0m),
                    TimeOut = table.Column<DateTime>(type: "datetime", nullable: true),
                    WeightOut = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    WeightImpurity = table.Column<decimal>(type: "decimal(18,1)", nullable: true),
                    ProductNumber = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    ProductName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    CustomerId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CustomerName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomerAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CustomerPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ProductId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ProductPrice = table.Column<double>(type: "float", nullable: true, defaultValue: 0.0),
                    ProductMoney = table.Column<double>(type: "float", nullable: true, defaultValue: 0.0),
                    Proportion = table.Column<double>(type: "float", nullable: true, defaultValue: 0.0),
                    Exchange = table.Column<decimal>(type: "decimal(18,0)", nullable: true),
                    Note = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    TypeId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    TopSeal = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    TotalMoney = table.Column<double>(type: "float", nullable: true, defaultValue: 0.0),
                    MoneyPayment = table.Column<double>(type: "float", nullable: true, defaultValue: 0.0),
                    MoneyDiscount = table.Column<double>(type: "float", nullable: true),
                    MoneyDebt = table.Column<double>(type: "float", nullable: true),
                    StatusPayment = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Img11 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Img12 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Img13 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Img21 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Img22 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Img23 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IndexInDay = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CreateDay = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdateDay = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblScale", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblUnit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreateDay = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdateDay = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblUnit", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tblVehicle",
                columns: table => new
                {
                    VehiceCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RfidCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DriverName = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    DriverPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TonnageDefault = table.Column<double>(type: "float", nullable: true, defaultValue: 0.0),
                    TransportationCompany = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<bool>(type: "bit", nullable: true, defaultValue: true),
                    CreateDay = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UpdateDay = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdateBy = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tblVehicle", x => x.VehiceCode);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "tblAcccount");

            migrationBuilder.DropTable(
                name: "tblBillMoney");

            migrationBuilder.DropTable(
                name: "tblCategoryPayment");

            migrationBuilder.DropTable(
                name: "tblCompany");

            migrationBuilder.DropTable(
                name: "tblCustomer");

            migrationBuilder.DropTable(
                name: "tblProduct");

            migrationBuilder.DropTable(
                name: "tblScale");

            migrationBuilder.DropTable(
                name: "tblUnit");

            migrationBuilder.DropTable(
                name: "tblVehicle");
        }
    }
}
