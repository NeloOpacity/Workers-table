using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Data;
using System.Windows;

namespace wpfwithadotest
{
    public class Database
    {
        DataGrid grid;
        string connectionString = "Integrated Security = SSPI;" +
                                     "Initial Catalog=new_db;" +
                                     "Data Source=(localdb)\\MSSQLLocalDB";
        SqlDataAdapter dAdapt;
        DataSet ds = new DataSet();


        public Database(DataGrid _grid)
        {
            grid = _grid;
            dAdapt = new SqlDataAdapter("SELECT Workers.[Id], Workers.[Фамилия],Workers.[Имя],Workers.[Отчество],FORMAT(Workers.[Дата поступления],'dd-mm-yyyy') AS [Дата поступления]," +
                                        "Workers.[Должность],Workers.[Зарплата],Department.[Отдел],Department.[Адрес], Courses.[Курс], Courses.[Место обучения] " +
                                        "FROM Workers JOIN Department ON Workers.[Номер отдела] = Department.[Номер] JOIN Courses ON Workers.[Номер пр. курса] = Courses.[Номер курса]", connectionString);
        }

        TableFilter filter;
        public TableFilter Filter
        {
            set
            {
                filter = value;
                if (value == null)
                {
                    GetAllWorkers();
                    return;
                }
                string cmd = "SELECT Workers.[Id], Workers.[Фамилия],Workers.[Имя],Workers.[Отчество],FORMAT(Workers.[Дата поступления],'dd-mm-yyyy') AS [Дата поступления]," +
                                        "Workers.[Должность],Workers.[Зарплата],Department.[Отдел],Department.[Адрес], Courses.[Курс], Courses.[Место обучения] " +
                                        "FROM Workers JOIN Department ON Workers.[Номер отдела] = Department.[Номер] JOIN Courses ON Workers.[Номер пр. курса] = Courses.[Номер курса] " +
                                        $"WHERE (Workers.[Зарплата]>{filter.MinSalary}";
                if (filter.MaxSalary > 0)
                {
                    cmd += $" AND Workers.[Зарплата]<{filter.MaxSalary}";
                }
                if (!String.IsNullOrEmpty(filter.Department))
                {
                    cmd += $" AND Department.[Отдел] LIKE (N'%{filter.Department}%')";
                }
                if (!String.IsNullOrEmpty(filter.Course))
                {
                    cmd += $" AND Courses.[Курс] LIKE (N'%{filter.Course}%')";
                }
                cmd += ")";
                dAdapt.SelectCommand.CommandText = cmd;
                GetAllWorkers();
            }
            get
            {
                return filter;
            }
        }

        public void DropFilters()
        {
            dAdapt = new SqlDataAdapter("SELECT Workers.[Id], Workers.[Фамилия],Workers.[Имя],Workers.[Отчество],FORMAT(Workers.[Дата поступления],'dd-mm-yyyy') AS [Дата поступления]," +
                                        "Workers.[Должность],Workers.[Зарплата],Department.[Отдел],Department.[Адрес], Courses.[Курс], Courses.[Место обучения] " +
                                        "FROM Workers JOIN Department ON Workers.[Номер отдела] = Department.[Номер] JOIN Courses ON Workers.[Номер пр. курса] = Courses.[Номер курса]", connectionString);
            Filter = null;
        }

        public void GetAllWorkers()
        {
            ds.Clear();
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    dAdapt.Fill(ds, "Workers");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                grid.ItemsSource = ds.Tables["Workers"].DefaultView;
            }
        }

        public void GetDepartments(DataGrid table)
        {
            DataSet ds = new DataSet();
            SqlDataAdapter depAdapt = new SqlDataAdapter("SELECT * FROM Department", connectionString);
            try
            {
                depAdapt.Fill(ds, "Departments");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            table.ItemsSource = ds.Tables["Departments"].DefaultView;
        }


        public void GetCourses(DataGrid table)
        {
            DataSet ds = new DataSet();
            SqlDataAdapter courseAdapt = new SqlDataAdapter("SELECT * FROM Courses", connectionString);
            try
            {
                courseAdapt.Fill(ds, "Courses");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            table.ItemsSource = ds.Tables["Courses"].DefaultView;
        }

        public void AddWorker(Worker worker)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand insertCmd = new SqlCommand("INSERT INTO Workers ( [Фамилия], [Имя] , [Отчество]  ,  [Дата поступления] , [Должность] , " +
                    "[Зарплата] , [Номер отдела], [Номер пр. курса]) VALUES (@Фамилия,@Имя,@Отчество,@Дата,@Должность,@Зарплата, @Номер, @Курс)", connection);
                insertCmd.Parameters.Add("@Фамилия", SqlDbType.NChar);
                insertCmd.Parameters["@Фамилия"].Value = worker.LastName;
                List<SqlParameter> sd = new List<SqlParameter>();
                SqlParameter namePam = new SqlParameter("@Имя", worker.FirstName);
                SqlParameter patronPam = new SqlParameter("@Отчество", worker.Pantronymic);
                //insertCmd.Parameters.Add(patronPam);
                SqlParameter datePam = new SqlParameter("@Дата", worker.DateOfEntering);
                //insertCmd.Parameters.Add(datePam);
                SqlParameter posPam = new SqlParameter("@Должность", worker.Position);
                //insertCmd.Parameters.Add(posPam);
                SqlParameter pointsPam = new SqlParameter("@Зарплата", worker.Salary);
                //insertCmd.Parameters.Add(pointsPam);
                SqlParameter departPam = new SqlParameter("@Номер", worker.NumberOfDepartment);
                //insertCmd.Parameters.Add(departPam);
                SqlParameter coursePam = new SqlParameter("@Курс", worker.NumberOfCourse);
                sd.AddRange(new List<SqlParameter> { namePam, patronPam, datePam, posPam, pointsPam, departPam, coursePam });
                insertCmd.Parameters.AddRange(sd.ToArray());
                try
                {
                    insertCmd.ExecuteNonQuery();
                    GetAllWorkers();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public void DeleteWorker()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    DataRowView row = (DataRowView)grid.SelectedItem;
                    SqlCommand cmd = new SqlCommand($"DELETE FROM Workers WHERE [Id]={row.Row.ItemArray[0]}", connection);
                    connection.Open();
                    cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                GetAllWorkers();
            }
        }

        public void GetAverageSalary()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                SqlCommand cmd = new SqlCommand($"SELECT AVG(Зарплата) FROM Workers",connection);
                var AverageSalary = cmd.ExecuteScalar();
                MessageBox.Show($"Средняя зарплата - {AverageSalary} рублей");
            }
        }
    }
}
