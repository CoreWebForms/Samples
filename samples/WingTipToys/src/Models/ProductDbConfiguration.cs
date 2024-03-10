using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace WingtipToys.Models;

public class ProductDbConfiguration : DbConfiguration
{
    public ProductDbConfiguration()
    {
        //https://learn.microsoft.com/en-us/dotnet/framework/data/adonet/connection-string-syntax
        // SetDefaultConnectionFactory(new SqlConnectionFactory("Data Source=yourpublicip.opensource.com;Initial Catalog=WingtipToys;User Id=youruserid;Password=yourpassword"));
    }
}