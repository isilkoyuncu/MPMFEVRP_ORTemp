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
                .Where(p => typeof(XCPlexVRPBase).IsAssignableFrom(p))
                .Where(type => typeof(XCPlexVRPBase).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            foreach (var xcplexModel in allXCPlexModels)
            {
                result.Add(xcplexModel.GetMethod("GetModelName").Invoke(Activator.CreateInstance(xcplexModel), null).ToString());
            }
            return result;
        }

        public static Type GetXCPlexModelTypeByName(String XCPlexModelName)
        {
            var allXCPlexModels = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(s => s.GetTypes())
                .Where(p => typeof(XCPlexVRPBase).IsAssignableFrom(p))
                .Where(type => typeof(XCPlexVRPBase).IsAssignableFrom(type))
                .Where(t => !t.IsAbstract)
                .ToList();

            foreach (var XCPlexModel in allXCPlexModels)
            {
                XCPlexVRPBase createdXCPlexModel = (XCPlexVRPBase)(Activator.CreateInstance(XCPlexModel));
                string name = createdXCPlexModel.GetModelName();
                if (createdXCPlexModel.GetModelName() == XCPlexModelName)
                {
                    return createdXCPlexModel.GetType();
                }
            }

            return null;
        }



        
    }
}
