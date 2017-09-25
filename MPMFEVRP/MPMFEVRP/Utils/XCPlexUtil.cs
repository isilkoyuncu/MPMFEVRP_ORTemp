using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MPMFEVRP.Models.XCPlex;

namespace MPMFEVRP.Utils
{
    public class XCPlexUtil
    {
        public static List<String> GetTSPModelNamesForSolver()
        {
            List<String> result = new List<string>();

            var allXCPlexModels = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(XCPlexBase).IsAssignableFrom(p))
                .Where(type => typeof(XCPlexBase).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            foreach (var xcplexModel in allXCPlexModels)
            {
                if (xcplexModel.GetMethod("IsTSPModel").Invoke(Activator.CreateInstance(xcplexModel), null).ToString()=="True")
                    result.Add(xcplexModel.GetMethod("GetModelName").Invoke(Activator.CreateInstance(xcplexModel), null).ToString());
            }
            return result;
        }
    }
}
