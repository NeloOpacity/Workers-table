using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace wpfwithadotest
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Database db;
        public MainWindow()
        {
            InitializeComponent();
            db = new Database(DataGrid1);
            db.GetAllWorkers();
        }

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            string[] fio = FIO.Text.Split(' ');
            try
            {
                Worker worker = new Worker
                {
                    FirstName = fio[1],
                    LastName = fio[0],
                    Pantronymic = fio[2],
                    DateOfEntering = datePick.SelectedDate.Value,
                    Position = position.Text,
                    Salary = Int32.Parse(Salary.Text),
                    NumberOfDepartment = Int32.Parse(depnum.Text),
                    NumberOfCourse = Int32.Parse(numcourse.Text)
                };
                db.AddWorker(worker);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void Del_Click(object sender, RoutedEventArgs e)
        {
            db.DeleteWorker();
            db.GetAllWorkers();
        }

        private void DepartmentTable_Click(object sender, RoutedEventArgs e)
        {
            Deps window1 = new Deps(db);
            window1.ShowDialog();
        }

        private void CoursesTable_Click(object sender, RoutedEventArgs e)
        {
            Courses window2 = new Courses(db);
            window2.ShowDialog();
        }

        private void DoFilteration_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                TableFilter filter = new TableFilter
                {
                    Course = coursefilt.Text,
                    Department = departmentfilt.Text
                };
                if (!String.IsNullOrEmpty(minsal.Text))
                    filter.MinSalary = Int32.Parse(minsal.Text);
                if (!String.IsNullOrEmpty(maxsal.Text))
                    filter.MaxSalary = Int32.Parse(maxsal.Text);
                db.Filter = filter;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DropFilters_Click(object sender, RoutedEventArgs e)
        {
            db.DropFilters();
        }

        private void bAverageSalary_Click(object sender, RoutedEventArgs e)
        {
            db.GetAverageSalary();
        }
    }
}
