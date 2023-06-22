using System.Text.RegularExpressions;

namespace DBProj
{
    public class FileWriter
    {
        private ParametersModel _parameters { get; set; }

        public FileWriter(ParametersModel parameters)
        {
            _parameters = parameters;
            WriteFile();
        }

        private void WriteFile()
        {

            string filePath = @"D:\OutputFiles"; //Folder for generated MF program
            string excelFilePath = @"D:\OutputExcelFiles"; //Folder for generated .xlsx output from MF program
            Directory.CreateDirectory(filePath);
            Directory.CreateDirectory(excelFilePath);
            var fileName = filePath + "\\Output_" + Guid.NewGuid().ToString() + ".cs";
            var excelFileName = excelFilePath + "\\Output_" + Guid.NewGuid().ToString() + ".xlsx";

            List<string> codeStrings = new List<string>();// this string is written to the generated MF program

            //adding dependencies
            codeStrings.Add(@"using Npgsql;                                             
                            using System.Data;
                            using System.Reflection;
                            using DBProj;
                            using ClosedXML.Excel;");

            //entrypoint of MF program
            codeStrings.Add(@"
                            static void main()
                            {
                                MF op = new MF();
                                op.algo();

                            }
                            main();

                            ");

            codeStrings.Add(@"namespace DBProj          
                            {");                    //dbproj start


            codeStrings.Add(@"public class MF
                                        {");                    //class testoutput start

            codeStrings.Add(@"private NpgsqlConnection _connection = new NpgsqlConnection(""Server=DBProj;User ID=postgres;Password=welcome@123;Host=localhost;Database=DB1;Port=5432"");
                                            private List<MF_Struct> final = new List<MF_Struct>();       //Initiating a list of instances of MF to add values to 
                                            private List<MF_Struct> resultSet = new List<MF_Struct>();       //final output with having condition
                                        ");
            ////////////////algo//////////////
            
            codeStrings.Add(@"public void algo()
                            { 
                                MF_Struct mf = new MF_Struct();

                                List<Sales> sales = Query(); ");

            //check if there are aggregates applicable on full table 
            var aggreagtesOnFullTable = _parameters.AggregateFunctions.Where(x => x.Contains("_0")).Select(x => x).ToList();

            // condition for optimized or not--> if yes only one loop on sales
            //check GroupingVariablePredicate for interdependencies
            var hasDependencies = CheckForDependencies(_parameters.GroupingVariablePredicate);


            //looping on number of graouping variables to generate respective number of scans
            for (int i = 0; i <= _parameters.NumberOfGroupingVariables; i++)
            {
                if (i == 0 && aggreagtesOnFullTable.Count == 0)
                    continue;

                //only one scan for queries without dependency
                if (hasDependencies)
                {
                    codeStrings.Add(@"//multiple scan due to interdependent condition");
                    codeStrings.Add(@"foreach(var s in sales){");
                }
                else if (i == 1 || i == 0)
                {
                    codeStrings.Add(@"//single scan as there is no interdependent condition");
                    codeStrings.Add(@"foreach(var s in sales){");

                }

                var listOfPredicates = new List<string>();
                if (i != 0)
                {
                    listOfPredicates = _parameters.GroupingVariablePredicate[i - 1].Split(" ").ToList();

                    for (int j = 0; j < listOfPredicates.Count; j++)
                    {
                        if (listOfPredicates[j].Contains("sum") || listOfPredicates[j].Contains("avg") ||
                         listOfPredicates[j].Contains("min") || listOfPredicates[j].Contains("max") ||
                         listOfPredicates[j].Contains("count"))
                        {
                            listOfPredicates[j] = "existing." + listOfPredicates[j];
                        }
                        else
                        {
                            listOfPredicates[j] = listOfPredicates[j].Replace("state_" + i, "s.State");
                            listOfPredicates[j] = listOfPredicates[j].Replace("quant_" + i, "s.Quant");
                            listOfPredicates[j] = listOfPredicates[j].Replace("cust_" + i, "s.Cust");
                            listOfPredicates[j] = listOfPredicates[j].Replace("prod_" + i, "s.Prod");
                            listOfPredicates[j] = listOfPredicates[j].Replace("day_" + i, "s.Day");
                            listOfPredicates[j] = listOfPredicates[j].Replace("month_" + i, "s.Month");
                            listOfPredicates[j] = listOfPredicates[j].Replace("year_" + i, "s.Year");
                            listOfPredicates[j] = listOfPredicates[j].Replace("date_" + i, "s.Date");
                            listOfPredicates[j] = listOfPredicates[j].Replace("'", "\"");
                            listOfPredicates[j] = listOfPredicates[j].Replace("and", "&&");
                            listOfPredicates[j] = listOfPredicates[j].Replace("or", "||");
                            listOfPredicates[j] = listOfPredicates[j].Replace("=", "==");
                            listOfPredicates[j] = listOfPredicates[j].Replace("<==", "<=");
                            listOfPredicates[j] = listOfPredicates[j].Replace(">==", ">=");

                            var splitDate = listOfPredicates[j].Split("-").ToList();
                            if (splitDate.Count == 3)
                            {
                                listOfPredicates[j] = $"new DateTime({Convert.ToInt32(splitDate[0])}, {Convert.ToInt32(splitDate[1])}, {Convert.ToInt32(splitDate[2])})";
                            }
                        }
                    }
                }
                var condition = "true";
                if (i != 0)
                    condition = string.Join(" ", listOfPredicates);

                var groupingAttributes = _parameters.GroupingAttributes;

                // get existing entry with given grouping attributes then update existing
                var existing = $"var existing  = final.Where(x => ";
                for (int j = 0; j < groupingAttributes.Count; j++)
                {
                    existing += $"x.{groupingAttributes[j]} == s.{char.ToUpper(groupingAttributes[j][0]) + groupingAttributes[j].Substring(1)}";
                    if (j < groupingAttributes.Count - 1)
                        existing += " && ";
                }
                existing += $").Select(x => x).FirstOrDefault(); //current row";

                if (hasDependencies || i==0 || i==1)
                {
                    codeStrings.Add(existing);
                    codeStrings.Add($"if( {condition} )");
                    codeStrings.Add(@"{");
                }
                else
                {
                    codeStrings.Add($"if( {condition} )");
                    codeStrings.Add(@"{");
                }

                // check if already exists in final list with given grouping attributes then update existing

                codeStrings.Add($"if(existing != null)");
                codeStrings.Add(@"{");

                //get current grouping variable aggregate functions
                var thisGroupingVariableAggregates = _parameters.AggregateFunctions.Where(x => x.Contains("_" + i)).ToList();
                var listOfAddedProperties = new List<string>();
                foreach (var groupingVariableAggregate in thisGroupingVariableAggregates)
                {
                    var propertyFromGV = groupingVariableAggregate.Split("_")[1];
                    var property = char.ToUpper(propertyFromGV[0]) + propertyFromGV.Substring(1);


                    if (!listOfAddedProperties.Contains(propertyFromGV))
                    {

                        if (propertyFromGV == "quant" || propertyFromGV == "day" ||
                            propertyFromGV == "month" || propertyFromGV == "year") // for int properties
                        {

                            codeStrings.Add(@"existing.count_" + propertyFromGV + "_" + i + "+=1;");
                            codeStrings.Add(@"existing.sum_" + propertyFromGV + "_" + i + "+=s." + property + ";");
                            codeStrings.Add(@"existing.avg_" + propertyFromGV + "_" + i + "=(decimal)(existing.sum_" + propertyFromGV + "_" + i + ")/(decimal)(existing.count_" + propertyFromGV + "_" + i + ");");
                            codeStrings.Add(@"existing.max_" + propertyFromGV + "_" + i + "=s." + property + " > existing.max_" + propertyFromGV + "_" + i + " ? s." + property + " : existing.max_" + propertyFromGV + "_" + i + ";");
                            codeStrings.Add(@"existing.min_" + propertyFromGV + "_" + i + "=s." + property + " < existing.min_" + propertyFromGV + "_" + i + " ? s." + property + " : existing.min_" + propertyFromGV + "_" + i + ";");

                        }
                        else
                        {
                            codeStrings.Add(@"existing.count_" + propertyFromGV + "_" + i + "+=1;");
                            if (propertyFromGV != "date")
                            {
                                codeStrings.Add(@"existing.max_" + propertyFromGV + "_" + i + "= string.Compare(s." + property + ", existing.max_" + propertyFromGV + "_" + i + ") == 1 ? s." + property + " : existing.max_" + propertyFromGV + "_" + i + ";");
                                codeStrings.Add(@"existing.min_" + propertyFromGV + "_" + i + "= string.Compare(s." + property + ", existing.min_" + propertyFromGV + "_" + i + ") == -1 ? s." + property + " : existing.min_" + propertyFromGV + "_" + i + ";");
                            }
                            else
                            {
                                codeStrings.Add(@"existing.max_" + propertyFromGV + "_" + i + "=s." + property + " > existing.max_" + propertyFromGV + "_" + i + " ? s." + property + " : existing.max_" + propertyFromGV + "_" + i + ";");
                                codeStrings.Add(@"existing.min_" + propertyFromGV + "_" + i + "=s." + property + " < existing.min_" + propertyFromGV + "_" + i + " ? s." + property + " : existing.min_" + propertyFromGV + "_" + i + ";");
                            }

                        }
                        listOfAddedProperties.Add(propertyFromGV);
                    }
                }

                codeStrings.Add(@"}");
                codeStrings.Add(@"else{");//adding new object(row) of MF_Struct in final list of MF_Struct
                var firstEntry = "final.Add(new MF_Struct {";
                for (int j = 0; j < groupingAttributes.Count; j++)
                {
                    firstEntry += $"{groupingAttributes[j]} = s.{char.ToUpper(groupingAttributes[j][0]) + groupingAttributes[j].Substring(1)}";
                    if (j < groupingAttributes.Count - 1)
                        firstEntry += " , ";
                }

                foreach (var propertyFromGV in listOfAddedProperties)
                {
                    var property = char.ToUpper(propertyFromGV[0]) + propertyFromGV.Substring(1);

                    firstEntry += ", count_" + propertyFromGV + "_" + i + "=1";
                    firstEntry += ", min_" + propertyFromGV + "_" + i + "=s." + property;
                    firstEntry += ", max_" + propertyFromGV + "_" + i + "=s." + property;

                    if (propertyFromGV == "quant" || propertyFromGV == "day" ||
                            propertyFromGV == "month" || propertyFromGV == "year") // for int properties
                    {
                        firstEntry += ", sum_" + propertyFromGV + "_" + i + "=s." + property;
                        firstEntry += ", avg_" + propertyFromGV + "_" + i + "=s." + property;

                    }

                }
                firstEntry += "});";


                codeStrings.Add(firstEntry);
                //close bracket of outer for loop on sales
                if (hasDependencies)
                {
                    codeStrings.Add(@"}");
                }
                else if (i == _parameters.NumberOfGroupingVariables || i == 0)
                {
                    codeStrings.Add(@"}");

                }

                codeStrings.Add(@"}");
                codeStrings.Add(@"}");

            }
            if (_parameters.HavingCondition.Trim() != string.Empty)
            {
                codeStrings.Add("foreach(var f in final){");

                var splitHavingCondition = _parameters.HavingCondition.Split(" ").ToList();

                for (int j = 0; j < splitHavingCondition.Count; j++)
                {
                    if (splitHavingCondition[j].Contains("sum") || splitHavingCondition[j].Contains("avg") ||
                        splitHavingCondition[j].Contains("min") || splitHavingCondition[j].Contains("max") ||
                        splitHavingCondition[j].Contains("count"))
                    {
                        splitHavingCondition[j] = "f." + splitHavingCondition[j];
                    }

                }
                var havingStr = string.Join(" ", splitHavingCondition);
                havingStr = havingStr.Replace("or", "||");
                havingStr = havingStr.Replace("and", "&&");

                codeStrings.Add("if(" + havingStr + ")");
                codeStrings.Add("resultSet.Add(f);");

                codeStrings.Add("}");
            }
            else
            {
                codeStrings.Add("resultSet = final;");
            }

            //order by grouping attributes
            var orderBy = "resultSet = resultSet.";

            for (int j = 0; j < _parameters.GroupingAttributes.Count; j++)
            {
                if (j == 0)
                    orderBy += "OrderBy(x=>x." + _parameters.GroupingAttributes[j] + ")";
                else
                    orderBy += ".ThenBy(x=>x." + _parameters.GroupingAttributes[j] + ")";
            }
            orderBy += ".ToList();";

            codeStrings.Add(orderBy);

            //print results on console
            var printStatement = "Console.WriteLine(\"=== ";

            foreach (var s in _parameters.SelectAttributes)
            {
                printStatement += $"{s}===";
            }

            printStatement += "\");";
            codeStrings.Add(printStatement);

            var resultPrintStatement = "foreach (var r in resultSet){";
            resultPrintStatement += "Console.WriteLine($\"";
            foreach (var s in _parameters.SelectAttributes)
            {
                resultPrintStatement += "   {r." + s + "}";
            }
            resultPrintStatement += "\");";

            resultPrintStatement += "}";
            codeStrings.Add(resultPrintStatement);

            //calling export to excel function
            codeStrings.Add(@"ExportToExcel(resultSet);");

            codeStrings.Add(@"}");
            ////////////////////
            
            // Query function connects to DB and fetches the records
            codeStrings.Add(@"public List<Sales> Query()
                                        {
                                            _connection.Open();

                                            var queryString = ""select * from sales"";


                                            NpgsqlDataAdapter adapter = new NpgsqlDataAdapter();

                                            adapter.SelectCommand = new NpgsqlCommand(queryString, _connection);

                                            DataTable table = new DataTable();
                                            adapter.Fill(table);

                                            var records = ConvertDataTable(table);

                                            _connection.Dispose();

                                            return records;

                                        }"); //start and end of Query function

            //ConvertDataTable function coverts the SQL records to Sales Model 
            codeStrings.Add(@" private static List<Sales> ConvertDataTable(DataTable dt)
                                {
                                    List<Sales> data = new List<Sales>();
                                    foreach (DataRow row in dt.Rows)
                                    {
                                        Sales item = GetItem(row);
                                        data.Add(item);
                                    }
                                    return data;
                                }");                //start and end convertdatatable function

            //Iterates over each row and reads properties of Sales and SQL data row, matches property names and fills object
            codeStrings.Add(@"private static Sales GetItem(DataRow dr)
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
                            }");                //start and end GetItem function


            //Export to excel function
            var exportToExcel = @"private void ExportToExcel(List<MF_Struct> itemList)
                              {
                                  var workbook = new XLWorkbook();
                                  workbook.AddWorksheet(""mfWorkSheet"");
                                  var ws = workbook.Worksheet(""mfWorkSheet"");

                                   int row = 1;
                                   ";
            for (int j = 1; j <= _parameters.SelectAttributes.Count; j++)
            {
                exportToExcel += $"ws.Cell(row, {j}).Value = \"{_parameters.SelectAttributes[j - 1]}\"; \n";
                exportToExcel += $"ws.Cell(row, {j}).Style.Font.Bold = true;\n";
            }

            exportToExcel += @"
                                   row++;
                                   foreach (var item in itemList)
                                   {";
            for (int j = 1; j <= _parameters.SelectAttributes.Count; j++)
            {
                exportToExcel += $"ws.Cell(row, {j}).Value = item.{_parameters.SelectAttributes[j - 1]}.ToString(); \n";

            }

            exportToExcel += @" row++;
                            }";

            exportToExcel += $"workbook.SaveAs(@\"{excelFileName}\");";
            exportToExcel += "}";

            codeStrings.Add(exportToExcel);

            codeStrings.Add(@"}");                  //class testoutput close

            codeStrings.Add(@"public class MF_Struct
                             {");                   //MF Struct open

            foreach (string x in _parameters.GroupingAttributes)        // to add grouping attributes
            {
                codeStrings.Add(@"public string " + x + ";");
            }


            for (int i = 0; i <= _parameters.NumberOfGroupingVariables; i++) // to add aggregate functions for each grouping variable
            {

                var thisGroupingVariableAggregates = _parameters.AggregateFunctions.Where(x => x.Contains("_" + i)).ToList();
                var listOfAddedProperties = new List<string>();
                foreach (var groupingVariableAggregate in thisGroupingVariableAggregates)
                {
                    var propertyFromGV = groupingVariableAggregate.Split("_")[1];

                    if (!listOfAddedProperties.Contains(propertyFromGV))
                    {

                        codeStrings.Add(@"public int count_" + propertyFromGV + "_" + i + ";");

                        if (propertyFromGV == "quant" || propertyFromGV == "day" ||
                                propertyFromGV == "month" || propertyFromGV == "year") // for int properties
                        {
                            codeStrings.Add(@"public int sum_" + propertyFromGV + "_" + i + ";");
                            codeStrings.Add(@"public decimal avg_" + propertyFromGV + "_" + i + ";");
                            codeStrings.Add(@"public int min_" + propertyFromGV + "_" + i + ";");
                            codeStrings.Add(@"public int max_" + propertyFromGV + "_" + i + ";");
                        }
                        else if (propertyFromGV == "date")
                        {
                            codeStrings.Add(@"public DateTime min_" + propertyFromGV + "_" + i + ";");
                            codeStrings.Add(@"public DateTime max_" + propertyFromGV + "_" + i + ";");

                        }
                        else
                        {
                            codeStrings.Add(@"public string min_" + propertyFromGV + "_" + i + ";");
                            codeStrings.Add(@"public string max_" + propertyFromGV + "_" + i + ";");
                        }

                        listOfAddedProperties.Add(propertyFromGV);
                    }

                }

            }

            codeStrings.Add(@"public int sum;");                            //to add aggregate functions for entire table
            codeStrings.Add(@"public int avg;");
            codeStrings.Add(@"public int count;");
            codeStrings.Add(@"public int max;");
            codeStrings.Add(@"public int min;");

            codeStrings.Add(@"public int NumberOfGroupingVariables;");

            codeStrings.Add(@"public MF_Struct()                    
                            {");                                            //open constructor to init variables

            foreach (var x in _parameters.GroupingAttributes)        // init grouping variables as empty strings
            {
                codeStrings.Add(x + @"=string.Empty;");
            }

            for (int i = 0; i <= _parameters.NumberOfGroupingVariables; i++) // to init aggregate functions for each grouping variable
            {
                var thisGroupingVariableAggregates = _parameters.AggregateFunctions.Where(x => x.Contains("_" + i)).ToList();
                var listOfAddedProperties = new List<string>();
                foreach (var groupingVariableAggregate in thisGroupingVariableAggregates)
                {
                    var propertyFromGV = groupingVariableAggregate.Split("_")[1];

                    if (!listOfAddedProperties.Contains(propertyFromGV))
                    {

                        codeStrings.Add(@"count_" + propertyFromGV + "_" + i + "=0;");


                        if (propertyFromGV == "quant" || propertyFromGV == "day" ||
                                propertyFromGV == "month" || propertyFromGV == "year") // for int properties
                        {
                            codeStrings.Add(@"sum_" + propertyFromGV + "_" + i + "=0;");
                            codeStrings.Add(@"avg_" + propertyFromGV + "_" + i + "=0;");
                            codeStrings.Add(@"min_" + propertyFromGV + "_" + i + "=0;");
                            codeStrings.Add(@"max_" + propertyFromGV + "_" + i + "=0;");

                        }
                        else if (propertyFromGV == "date")
                        {
                            codeStrings.Add(@"min_" + propertyFromGV + "_" + i + "=DateTime.Now;");
                            codeStrings.Add(@"max_" + propertyFromGV + "_" + i + "=new DateTime(2015,12,31);");
                        }
                        else
                        {
                            codeStrings.Add(@"min_" + propertyFromGV + "_" + i + "=string.Empty;");
                            codeStrings.Add(@"max_" + propertyFromGV + "_" + i + "=string.Empty;");
                        }
                        listOfAddedProperties.Add(propertyFromGV);
                    }

                }
            }

            codeStrings.Add(@"sum=0;");                            //to init aggregate functions for entire table
            codeStrings.Add(@"avg=0;");
            codeStrings.Add(@"count=0;");
            codeStrings.Add(@"max=0;");
            codeStrings.Add(@"min=0;");

            codeStrings.Add(@"}");                  //close constructor



            codeStrings.Add(@"}");                  //MF_Struct close

            codeStrings.Add(@"public class Sales
                                {
                                    public string Cust { get; set; }

                                    public string Prod { get; set; }

                                    public int Day { get; set; }

                                    public int Month { get; set; }

                                    public int Year { get; set; }

                                    public string State { get; set; }

                                    public int Quant { get; set; }

                                    public DateTime Date { get; set; }
                                }");                   //Sales class start and end

            codeStrings.Add(@"}");                  //dbrpoj close

            ////////////////////////////////////////////////////////////////

            //Write codestring to a .cs file which can be run in a Solution
            File.WriteAllLines(fileName, codeStrings);

        }


        private bool CheckForDependencies(List<string> groupingVariablePredicates)
        {
            int index = 1;
            foreach (var gvp in groupingVariablePredicates)
            {
                if (index > 1)
                {
                    var splitGVP = gvp.Split(" ");
                    string pattern = @"_\d+";  //check for _ following with any integer e.g. _2 , _3 etc.
                    Regex regex = new Regex(pattern);

                    foreach (var splitPredicate in splitGVP)
                    {
                        Match match = regex.Match(splitPredicate);
                        if (match.Success)
                        {
                            var gvpNumber = Convert.ToInt32(match.Value.Split("_")[1]);

                            if (gvpNumber < index && gvpNumber !=0)
                                return true;
                        }
                    }

                }
                index++;
            }
            return false;
        }



    }
}
