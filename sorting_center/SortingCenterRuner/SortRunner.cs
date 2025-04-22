using AbstractModel;
using SortingCenterModel;

namespace sorting_center_runer
{
    class SortRunner
    {


        static void Main(string[] args)
        {

            //SortingCenterConfig sortConfig = new SortingCenterConfig();
            //Helper.SerializeXMLToFile(sortConfig, "sorting_center_config.xml");

            SortingCenterConfig sortConfig = Helper.DeserializeXMLFromFile<SortingCenterConfig>("sorting_center_config.xml");

            SortCenterWrapper wrapper = new SortCenterWrapper(sortConfig);
            while (wrapper.Next())
            {

            }
        }
    }
}

