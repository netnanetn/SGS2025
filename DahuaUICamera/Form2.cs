using CMS_Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DahuaUICamera
{
    public partial class Form2 : Form
    {
        private readonly MoDaContext _db;
        public Form2()
        {
            InitializeComponent();
            var dbPath = Path.Combine("E:\\MyProject\\SGS2025\\Database", "SGS2025OFFLINE.db");
            var options = new DbContextOptionsBuilder<MoDaContext>()
                .UseSqlite($"Data Source={dbPath}")
                .Options;

            _db = new MoDaContext(options);
        }

        private void btnLoadData_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();

            var users = _db.TblAcccounts.ToList();
            foreach (var u in users)
            {
                listBox1.Items.Add($"{u.Id} - {u.FullName}");
            }
        }

        private void btnAddAccount_Click(object sender, EventArgs e)
        {
            var acc = new TblAcccount
            {
                UserName = "admin",
                PassWord = "123456",
                FullName = "Nguyen Van Admin",
                Email = "admin@example.com",
                Phone = "0901234567",
                Status = true,
                CreateDay = DateTime.Now,
                CreateBy = "system"
            };

            _db.TblAcccounts.Add(acc);
            _db.SaveChanges();
        }
    }
}
