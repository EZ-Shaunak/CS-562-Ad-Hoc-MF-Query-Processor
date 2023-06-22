namespace DBProj
{
    public class ParametersModel
    {
        public List<string> SelectAttributes { get; set; }

        public int NumberOfGroupingVariables { get; set; }

        public List<string> GroupingAttributes { get; set; }

        public List<string> AggregateFunctions { get; set; }

        public List<string> GroupingVariablePredicate { get; set; }

        public string HavingCondition { get; set; }

        public string WhereCondition { get; set; }

        public string TableName
        {
            get
            {
                return "sales";
            }
        }

    }
}
