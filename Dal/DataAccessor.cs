using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using NetworkProgrammingP47.Models;
using NetworkProgrammingP47.Services;

namespace NetworkProgrammingP47.Dal
{
    internal class DataAccessor
    {
        private readonly String connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\Comp\source\repos\zhhtmss\NetworkProgrammingP47\Database\Database1.mdf;Integrated Security=True";
        private SqlConnection connection;
        public DataAccessor()
        {
            connection = new SqlConnection(connectionString);
            try
            {
                connection.Open();
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.Message);
                throw;
            }
        }

        public void ConfirmEmail(UserEntity userEntity)
        {
            String sql = $"UPDATE Users SET Code = NULL, CodeAt = NULL WHERE Id = '{userEntity.Id}'";
            using SqlCommand cmd = new(sql, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
        public UserEntity? Authenticate(String email, String password)
        {
            // email = user' or '1'='1
            // SELECT * FROM Users u WHERE u.Email = '{email}'
            // SELECT * FROM Users u WHERE u.Email = 'user' or '1'='1'

            String sql = $"SELECT * FROM Users u WHERE u.Email = '{email}'";
            using SqlCommand command = new(sql, connection);
            command.Parameters.AddWithValue("@Email", email);
            using SqlDataReader reader = command.ExecuteReader();
            if (reader.Read())
            {
                UserEntity userEntity = new(reader);
                String dk = KdfService.Dk(password,  userEntity.Id.ToString());
                if (dk == userEntity.Dk) return userEntity;
                else return null;
            }
            return null;
        }

        public void AddUser(UserSignupModel model)
        {
            String sql = "INSERT INTO Users(Id, Name, Email, Code, CodeAt, Dk)"
                + " VALUES(@Id, @Name, @Email, @Code, @CodeAt, @Dk)";
            String id = Guid.NewGuid().ToString();
            String dk = KdfService.Dk(model.Password, id);

            using SqlCommand cmd = new(sql, connection);
            cmd.Parameters.AddWithValue("@Id",     id               );
            cmd.Parameters.AddWithValue("@Name",   model.Name       );
            cmd.Parameters.AddWithValue("@Email",  model.Email      );
            cmd.Parameters.AddWithValue("@Code",   model.ConfirmCode); 
            cmd.Parameters.AddWithValue("@CodeAt", DateTime.Now     );
            cmd.Parameters.AddWithValue("@Dk",     dk               );
            try { cmd.ExecuteNonQuery();  } 
            catch(Exception ex){ Console.WriteLine(ex.Message); throw; }
        }

        public void InstallTables()
        {
            String sql = @"CREATE TABLE Users (
                [Id]     UNIQUEIDENTIFIER PRIMARY KEY,
                [Name]   NVARCHAR(128)    NOT NULL,
                [Email]  NVARCHAR(256)    NOT NULL UNIQUE,
                [Code]   VARCHAR(10)      NULL,
                [CodeAt] DATETIME2        NULL,
                [RegAt]  DATETIME2        DEFAULT CURRENT_TIMESTAMP,
                [Dk]     CHAR(32)         NOT NULL -- COMMENT 'Derived Key by RFC 2898'
             )";
            using SqlCommand cmd = new(sql, connection);
            try
            {
                cmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw;
            }
        }
    }
}
