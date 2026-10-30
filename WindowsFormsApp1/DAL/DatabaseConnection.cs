using System.Data.SqlClient;

namespace WindowsFormsApp1.DAL
{
    public class DatabaseConnection
    {
        // Thêm TrustServerCertificate=True và đặt Encrypt=False để tránh lỗi chứng chỉ LocalDB
        private static string strConn = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=HotelDB;Integrated Security=True;Pooling=False;Encrypt=False;TrustServerCertificate=True";

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(strConn);
        }
    }
}