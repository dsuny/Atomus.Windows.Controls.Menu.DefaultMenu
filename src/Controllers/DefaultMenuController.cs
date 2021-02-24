using Atomus.Windows.Controls.Menu.Models;
using Atomus.Database;
using Atomus.Service;
using System.Threading.Tasks;

namespace Atomus.Windows.Controls.Menu.Controllers
{
    internal static class DefaultMenuControllerController
    {
        internal static async Task<IResponse> SearchAsync(this ICore core, DefaultMenuSearchModel search)
        {
            IServiceDataSet serviceDataSet;

            serviceDataSet = new ServiceDataSet
            {
                ServiceName = core.GetAttribute("ServiceName"),
                TransactionScope = false
            };
            serviceDataSet["LoadMenu"].ConnectionName = core.GetAttribute("DatabaseName");
            serviceDataSet["LoadMenu"].CommandText = core.GetAttribute("ProcedureMenu");
            serviceDataSet["LoadMenu"].AddParameter("@START_MENU_ID", DbType.Decimal, 18);
            serviceDataSet["LoadMenu"].AddParameter("@ONLY_PARENT_MENU_ID", DbType.Decimal, 18);
            serviceDataSet["LoadMenu"].AddParameter("@USER_ID", DbType.Decimal, 18);

            serviceDataSet["LoadMenu"].NewRow();
            serviceDataSet["LoadMenu"].SetValue("@START_MENU_ID", search.START_MENU_ID.MinusToDBNullValue());
            serviceDataSet["LoadMenu"].SetValue("@ONLY_PARENT_MENU_ID", search.ONLY_PARENT_MENU_ID.MinusToDBNullValue());
            serviceDataSet["LoadMenu"].SetValue("@USER_ID", Config.Client.GetAttribute("Account.USER_ID"));

            return await core.ServiceRequestAsync(serviceDataSet);
        }
        internal static IResponse Search(this ICore core, DefaultMenuSearchModel search)
        {
            IServiceDataSet serviceDataSet;

            serviceDataSet = new ServiceDataSet
            {
                ServiceName = core.GetAttribute("ServiceName"),
                TransactionScope = false
            };
            serviceDataSet["LoadMenu"].ConnectionName = core.GetAttribute("DatabaseName");
            serviceDataSet["LoadMenu"].CommandText = core.GetAttribute("ProcedureMenu");
            serviceDataSet["LoadMenu"].AddParameter("@START_MENU_ID", DbType.Decimal, 18);
            serviceDataSet["LoadMenu"].AddParameter("@ONLY_PARENT_MENU_ID", DbType.Decimal, 18);
            serviceDataSet["LoadMenu"].AddParameter("@USER_ID", DbType.Decimal, 18);

            serviceDataSet["LoadMenu"].NewRow();
            serviceDataSet["LoadMenu"].SetValue("@START_MENU_ID", search.START_MENU_ID.MinusToDBNullValue());
            serviceDataSet["LoadMenu"].SetValue("@ONLY_PARENT_MENU_ID", search.ONLY_PARENT_MENU_ID.MinusToDBNullValue());
            serviceDataSet["LoadMenu"].SetValue("@USER_ID", Config.Client.GetAttribute("Account.USER_ID"));

            return core.ServiceRequest(serviceDataSet);
        }
    }
}