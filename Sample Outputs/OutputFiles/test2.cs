using Npgsql;                                             
                            using System.Data;
                            using System.Reflection;
                            using DBProj;
                            using ClosedXML.Excel;

                            static void main()
                            {
                                MF op = new MF();
                                op.algo();

                            }
                            main();

                            
namespace DBProj          
                            {
public class MF
                                        {
private NpgsqlConnection _connection = new NpgsqlConnection("Server=DBProj;User ID=postgres;Password=welcome@123;Host=localhost;Database=DB1;Port=5432");
                                            private List<MF_Struct> final = new List<MF_Struct>();       //Initiating a list of instances of MF to add values to 
                                            private List<MF_Struct> resultSet = new List<MF_Struct>();       //final output with having condition
                                        
public void algo()
                            { 
                                MF_Struct mf = new MF_Struct();

                                List<Sales> sales = Query(); 
//multiple scan due to interdependent condition
foreach(var s in sales){
var existing  = final.Where(x => x.cust == s.Cust && x.prod == s.Prod).Select(x => x).FirstOrDefault(); //current row
if( s.State == "NY" )
{
if(existing != null)
{
existing.count_quant_1+=1;
existing.sum_quant_1+=s.Quant;
existing.avg_quant_1=(decimal)(existing.sum_quant_1)/(decimal)(existing.count_quant_1);
existing.max_quant_1=s.Quant > existing.max_quant_1 ? s.Quant : existing.max_quant_1;
existing.min_quant_1=s.Quant < existing.min_quant_1 ? s.Quant : existing.min_quant_1;
existing.count_month_1+=1;
existing.sum_month_1+=s.Month;
existing.avg_month_1=(decimal)(existing.sum_month_1)/(decimal)(existing.count_month_1);
existing.max_month_1=s.Month > existing.max_month_1 ? s.Month : existing.max_month_1;
existing.min_month_1=s.Month < existing.min_month_1 ? s.Month : existing.min_month_1;
existing.count_day_1+=1;
existing.sum_day_1+=s.Day;
existing.avg_day_1=(decimal)(existing.sum_day_1)/(decimal)(existing.count_day_1);
existing.max_day_1=s.Day > existing.max_day_1 ? s.Day : existing.max_day_1;
existing.min_day_1=s.Day < existing.min_day_1 ? s.Day : existing.min_day_1;
existing.count_cust_1+=1;
existing.max_cust_1= string.Compare(s.Cust, existing.max_cust_1) == 1 ? s.Cust : existing.max_cust_1;
existing.min_cust_1= string.Compare(s.Cust, existing.min_cust_1) == -1 ? s.Cust : existing.min_cust_1;
}
else{
final.Add(new MF_Struct {cust = s.Cust , prod = s.Prod, count_quant_1=1, min_quant_1=s.Quant, max_quant_1=s.Quant, sum_quant_1=s.Quant, avg_quant_1=s.Quant, count_month_1=1, min_month_1=s.Month, max_month_1=s.Month, sum_month_1=s.Month, avg_month_1=s.Month, count_day_1=1, min_day_1=s.Day, max_day_1=s.Day, sum_day_1=s.Day, avg_day_1=s.Day, count_cust_1=1, min_cust_1=s.Cust, max_cust_1=s.Cust});
}
}
}
//multiple scan due to interdependent condition
foreach(var s in sales){
var existing  = final.Where(x => x.cust == s.Cust && x.prod == s.Prod).Select(x => x).FirstOrDefault(); //current row
if( s.State == "NJ" )
{
if(existing != null)
{
existing.count_quant_2+=1;
existing.sum_quant_2+=s.Quant;
existing.avg_quant_2=(decimal)(existing.sum_quant_2)/(decimal)(existing.count_quant_2);
existing.max_quant_2=s.Quant > existing.max_quant_2 ? s.Quant : existing.max_quant_2;
existing.min_quant_2=s.Quant < existing.min_quant_2 ? s.Quant : existing.min_quant_2;
}
else{
final.Add(new MF_Struct {cust = s.Cust , prod = s.Prod, count_quant_2=1, min_quant_2=s.Quant, max_quant_2=s.Quant, sum_quant_2=s.Quant, avg_quant_2=s.Quant});
}
}
}
//multiple scan due to interdependent condition
foreach(var s in sales){
var existing  = final.Where(x => x.cust == s.Cust && x.prod == s.Prod).Select(x => x).FirstOrDefault(); //current row
if( s.State == "CT" && existing.avg_quant_2 > s.Quant )
{
if(existing != null)
{
existing.count_quant_3+=1;
existing.sum_quant_3+=s.Quant;
existing.avg_quant_3=(decimal)(existing.sum_quant_3)/(decimal)(existing.count_quant_3);
existing.max_quant_3=s.Quant > existing.max_quant_3 ? s.Quant : existing.max_quant_3;
existing.min_quant_3=s.Quant < existing.min_quant_3 ? s.Quant : existing.min_quant_3;
existing.count_year_3+=1;
existing.sum_year_3+=s.Year;
existing.avg_year_3=(decimal)(existing.sum_year_3)/(decimal)(existing.count_year_3);
existing.max_year_3=s.Year > existing.max_year_3 ? s.Year : existing.max_year_3;
existing.min_year_3=s.Year < existing.min_year_3 ? s.Year : existing.min_year_3;
}
else{
final.Add(new MF_Struct {cust = s.Cust , prod = s.Prod, count_quant_3=1, min_quant_3=s.Quant, max_quant_3=s.Quant, sum_quant_3=s.Quant, avg_quant_3=s.Quant, count_year_3=1, min_year_3=s.Year, max_year_3=s.Year, sum_year_3=s.Year, avg_year_3=s.Year});
}
}
}
foreach(var f in final){
if(f.sum_quant_1 > 2 * f.sum_quant_2 || f.avg_quant_1 > f.avg_quant_3)
resultSet.Add(f);
}
resultSet = resultSet.OrderBy(x=>x.cust).ThenBy(x=>x.prod).ToList();
Console.WriteLine("=== cust===prod===sum_quant_1===max_cust_1===avg_day_1===sum_quant_2===sum_quant_3===count_year_3===");
foreach (var r in resultSet){Console.WriteLine($"   {r.cust}   {r.prod}   {r.sum_quant_1}   {r.max_cust_1}   {r.avg_day_1}   {r.sum_quant_2}   {r.sum_quant_3}   {r.count_year_3}");}
ExportToExcel(resultSet);
}
public List<Sales> Query()
                                        {
                                            _connection.Open();

                                            var queryString = "select * from sales";


                                            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();

                                            adapter.SelectCommand = new NpgsqlCommand(queryString, _connection);

                                            DataTable table = new DataTable();
                                            adapter.Fill(table);

                                            var records = ConvertDataTable(table);

                                            _connection.Dispose();

                                            return records;

                                        }
 private static List<Sales> ConvertDataTable(DataTable dt)
                                {
                                    List<Sales> data = new List<Sales>();
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        Sales item = GetItem(row);
                                        data.Add(item);
                                    }
                                    return data;
                                }
private static Sales GetItem(DataRow dr)
                            {
                                Sales obj = new Sales();

                                foreach (DataColumn column in dr.Table.Columns)
                                {
                                    foreach (PropertyInfo pro in typeof(Sales).GetProperties())
                                    {
                                        if (pro.Name.ToLower() == column.ColumnName.ToLower())
                                            pro.SetValue(obj, dr[column.ColumnName], null);
                                        else
                                            continue;
                                    }
                                }
                                return obj;
                            }
private void ExportToExcel(List<MF_Struct> itemList)
                              {
                                  var workbook = new XLWorkbook();
                                  workbook.AddWorksheet("mfWorkSheet");
                                  var ws = workbook.Worksheet("mfWorkSheet");

                                   int row = 1;
                                   ws.Cell(row, 1).Value = "cust"; 
ws.Cell(row, 1).Style.Font.Bold = true;
ws.Cell(row, 2).Value = "prod"; 
ws.Cell(row, 2).Style.Font.Bold = true;
ws.Cell(row, 3).Value = "sum_quant_1"; 
ws.Cell(row, 3).Style.Font.Bold = true;
ws.Cell(row, 4).Value = "max_cust_1"; 
ws.Cell(row, 4).Style.Font.Bold = true;
ws.Cell(row, 5).Value = "avg_day_1"; 
ws.Cell(row, 5).Style.Font.Bold = true;
ws.Cell(row, 6).Value = "sum_quant_2"; 
ws.Cell(row, 6).Style.Font.Bold = true;
ws.Cell(row, 7).Value = "sum_quant_3"; 
ws.Cell(row, 7).Style.Font.Bold = true;
ws.Cell(row, 8).Value = "count_year_3"; 
ws.Cell(row, 8).Style.Font.Bold = true;

                                   row++;
                                   foreach (var item in itemList)
                                   {ws.Cell(row, 1).Value = item.cust.ToString(); 
ws.Cell(row, 2).Value = item.prod.ToString(); 
ws.Cell(row, 3).Value = item.sum_quant_1.ToString(); 
ws.Cell(row, 4).Value = item.max_cust_1.ToString(); 
ws.Cell(row, 5).Value = item.avg_day_1.ToString(); 
ws.Cell(row, 6).Value = item.sum_quant_2.ToString(); 
ws.Cell(row, 7).Value = item.sum_quant_3.ToString(); 
ws.Cell(row, 8).Value = item.count_year_3.ToString(); 
 row++;
                            }workbook.SaveAs(@"D:\OutputExcelFiles\Output_3aaf4428-41b4-429b-8f6e-e240d8ab5fc9.xlsx");}
}
public class MF_Struct
                             {
public string cust;
public string prod;
public int count_quant_1;
public int sum_quant_1;
public decimal avg_quant_1;
public int min_quant_1;
public int max_quant_1;
public int count_month_1;
public int sum_month_1;
public decimal avg_month_1;
public int min_month_1;
public int max_month_1;
public int count_day_1;
public int sum_day_1;
public decimal avg_day_1;
public int min_day_1;
public int max_day_1;
public int count_cust_1;
public string min_cust_1;
public string max_cust_1;
public int count_quant_2;
public int sum_quant_2;
public decimal avg_quant_2;
public int min_quant_2;
public int max_quant_2;
public int count_quant_3;
public int sum_quant_3;
public decimal avg_quant_3;
public int min_quant_3;
public int max_quant_3;
public int count_year_3;
public int sum_year_3;
public decimal avg_year_3;
public int min_year_3;
public int max_year_3;
public int sum;
public int avg;
public int count;
public int max;
public int min;
public int NumberOfGroupingVariables;
public MF_Struct()                    
                            {
cust=string.Empty;
prod=string.Empty;
count_quant_1=0;
sum_quant_1=0;
avg_quant_1=0;
min_quant_1=0;
max_quant_1=0;
count_month_1=0;
sum_month_1=0;
avg_month_1=0;
min_month_1=0;
max_month_1=0;
count_day_1=0;
sum_day_1=0;
avg_day_1=0;
min_day_1=0;
max_day_1=0;
count_cust_1=0;
min_cust_1=string.Empty;
max_cust_1=string.Empty;
count_quant_2=0;
sum_quant_2=0;
avg_quant_2=0;
min_quant_2=0;
max_quant_2=0;
count_quant_3=0;
sum_quant_3=0;
avg_quant_3=0;
min_quant_3=0;
max_quant_3=0;
count_year_3=0;
sum_year_3=0;
avg_year_3=0;
min_year_3=0;
max_year_3=0;
sum=0;
avg=0;
count=0;
max=0;
min=0;
}
}
public class Sales
                                {
                                    public string Cust { get; set; }

                                    public string Prod { get; set; }

                                    public int Day { get; set; }

                                    public int Month { get; set; }

                                    public int Year { get; set; }

                                    public string State { get; set; }

                                    public int Quant { get; set; }

                                    public DateTime Date { get; set; }
                                }
}
