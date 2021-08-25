using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NodeRevitAddins.Helpers
{
    public static class SharedParameterHelper
    {
        public static DefinitionFile CreateSharedParameterFile(Application app)
        {
            string path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + @"\Node LV parameters\";
            string filename = "SharedParameter.txt";

            app.SharedParametersFilename = path + filename;
            if(app.OpenSharedParameterFile() != null)
            {
                return app.OpenSharedParameterFile();
            }
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                FileStream fs = File.Create(path + filename);
                fs.Close();
                app.SharedParametersFilename = path + filename;
                return app.OpenSharedParameterFile();
            }
            else
            {
                FileStream fs = File.Create(path + filename);
                fs.Close();
                app.SharedParametersFilename = path + filename;
                return app.OpenSharedParameterFile();
            }
        }

        public static DefinitionGroup GetDefinitionGroup(DefinitionFile definitionFile, string groupName)
        {
            DefinitionGroup definitionGroup = definitionFile.Groups.get_Item(groupName);
            if(definitionGroup != null)
            {
                return definitionGroup;
            }
            else
            {
                return definitionFile.Groups.Create(groupName);
            }
        }
    }
}
