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
//single scan as there is no interdependent condition
foreach(var s in sales){
var existing  = final.Where(x => x.cust == s.Cust && x.prod == s.Prod).Select(x => x).FirstOrDefault(); //current row
if( s.Cust == "Dan" && s.Prod == "Butter" )
{
if(existing != null)
{
existing.count_quant_1+=1;
existing.sum_quant_1+=s.Quant;
existing.avg_quant_1=(decimal)(existing.sum_quant_1)/(decimal)(existing.count_quant_1);
existing.max_quant_1=s.Quant > existing.max_quant_1 ? s.Quant : existing.max_quant_1;
existing.min_quant_1=s.Quant < existing.min_quant_1 ? s.Quant : existing.min_quant_1;
existing.count_date_1+=1;
existing.max_date_1=s.Date > existing.max_date_1 ? s.Date : existing.max_date_1;
existing.min_date_1=s.Date < existing.min_date_1 ? s.Date : existing.min_date_1;
}
else{
final.Add(new MF_Struct {cust = s.Cust , prod = s.Prod, count_quant_1=1, min_quant_1=s.Quant, max_quant_1=s.Quant, sum_quant_1=s.Quant, avg_quant_1=s.Quant, count_date_1=1, min_date_1=s.Date, max_date_1=s.Date});
}
}
if( s.Date > new DateTime(2017, 12, 23) )
{
if(existing != null)
{
existing.count_quant_2+=1;
existing.sum_quant_2+=s.Quant;
existing.avg_quant_2=(decimal)(existing.sum_quant_2)/(decimal)(existing.count_quant_2);
existing.max_quant_2=s.Quant > existing.max_quant_2 ? s.Quant : existing.max_quant_2;
existing.min_quant_2=s.Quant < existing.min_quant_2 ? s.Quant : existing.min_quant_2;
existing.count_date_2+=1;
existing.max_date_2=s.Date > existing.max_date_2 ? s.Date : existing.max_date_2;
existing.min_date_2=s.Date < existing.min_date_2 ? s.Date : existing.min_date_2;
}
else{
final.Add(new MF_Struct {cust = s.Cust , prod = s.Prod, count_quant_2=1, min_quant_2=s.Quant, max_quant_2=s.Quant, sum_quant_2=s.Quant, avg_quant_2=s.Quant, count_date_2=1, min_date_2=s.Date, max_date_2=s.Date});
}
}
}
resultSet = final;
resultSet = resultSet.OrderBy(x=>x.cust).ThenBy(x=>x.prod).ToList();
Console.WriteLine("=== cust===prod===sum_quant_1===min_date_1===sum_quant_2===max_date_2===");
foreach (var r in resultSet){Console.WriteLine($"   {r.cust}   {r.prod}   {r.sum_quant_1}   {r.min_date_1}   {r.sum_quant_2}   {r.max_date_2}");}
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
ws.Cell(row, 4).Value = "min_date_1"; 
ws.Cell(row, 4).Style.Font.Bold = true;
ws.Cell(row, 5).Value = "sum_quant_2"; 
ws.Cell(row, 5).Style.Font.Bold = true;
ws.Cell(row, 6).Value = "max_date_2"; 
ws.Cell(row, 6).Style.Font.Bold = true;

                                   row++;
                                   foreach (var item in itemList)
                                   {ws.Cell(row, 1).Value = item.cust.ToString(); 
ws.Cell(row, 2).Value = item.prod.ToString(); 
ws.Cell(row, 3).Value = item.sum_quant_1.ToString(); 
ws.Cell(row, 4).Value = item.min_date_1.ToString(); 
ws.Cell(row, 5).Value = item.sum_quant_2.ToString(); 
ws.Cell(row, 6).Value = item.max_date_2.ToString(); 
 row++;
                            }workbook.SaveAs(@"D:\OutputExcelFiles\Output_30bdf984-c9b7-4978-9e62-39b8142c0471.xlsx");}
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
public int count_date_1;
public DateTime min_date_1;
public DateTime max_date_1;
public int count_quant_2;
public int sum_quant_2;
public decimal avg_quant_2;
public int min_quant_2;
public int max_quant_2;
public int count_date_2;
public DateTime min_date_2;
public DateTime max_date_2;
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
count_date_1=0;
min_date_1=DateTime.Now;
max_date_1=new DateTime(2015,12,31);
count_quant_2=0;
sum_quant_2=0;
avg_quant_2=0;
min_quant_2=0;
max_quant_2=0;
count_date_2=0;
min_date_2=DateTime.Now;
max_date_2=new DateTime(2015,12,31);
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
