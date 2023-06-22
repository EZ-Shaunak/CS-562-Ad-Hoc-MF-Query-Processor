using DBProj;

void main() {
    Console.WriteLine("Please enter JSON file path :");
    string path = Console.ReadLine();

    if (path != null) { 
        //convert input json file to ParametersModel
        ParametersModel parameters =   BodyParser.ConvertToModel(path);

        //pass params to mfgenerator --> generate a .cs file 
        FileWriter f = new FileWriter(parameters);
        
    }
}


main();

