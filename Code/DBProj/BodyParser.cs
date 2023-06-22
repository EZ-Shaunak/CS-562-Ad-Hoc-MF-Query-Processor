using System.Text.Json;

namespace DBProj
{//process json to  ParametersModel object
    public static class BodyParser
    {
        public static ParametersModel ConvertToModel(string path)
        {
            //read
            string json = File.ReadAllText(path);

            //desirealize
            var parameters= JsonSerializer.Deserialize<ParametersModel>(json);

            return parameters;
        }

    }
}
