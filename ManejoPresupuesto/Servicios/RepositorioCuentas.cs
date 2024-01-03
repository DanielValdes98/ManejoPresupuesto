using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;

namespace ManejoPresupuesto.Servicios
{
    public interface IRepositorioCuentas
    {
        Task Actualizar(CuentaCreacionViewModel cuenta);
        Task Borrar(int id);
        Task<IEnumerable<Cuenta>> Buscar(int usuarioId);
        Task Crear(Cuenta cuenta);
        Task<Cuenta> ObtenerPorId(int id, int usuarioId);
    }

    public class RepositorioCuentas : IRepositorioCuentas
    {
        private readonly string connectionString;

        public RepositorioCuentas(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Cuenta cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"INSERT INTO cuentas (nombre, tipocuentaid, descripcion, balance)
                                                        VALUES (@Nombre, @TipoCuentaId, @Descripcion, @Balance);

                                                        SELECT SCOPE_IDENTITY();", cuenta);

            cuenta.Id = id;
        }

        public async Task<IEnumerable<Cuenta>> Buscar(int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Cuenta>(@"SELECT c.id, c.nombre, c.balance, tc.nombre AS TipoCuenta
                                                         FROM cuentas AS c
                                                         INNER JOIN tiposcuentas AS tc 
                                                         ON tc.id = c.tipocuentaid
                                                         WHERE tc.usuarioid = @UsuarioId
                                                         ORDER BY tc.orden", new { usuarioId });
        }

        public async Task<Cuenta> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Cuenta>(
                @"SELECT c.id, c.nombre, c.balance, c.descripcion, tipocuentaid
                FROM cuentas AS c
                INNER JOIN tiposcuentas AS tc 
                ON tc.id = c.tipocuentaid
                WHERE tc.usuarioid = @UsuarioId AND c.id = @Id", new { id, usuarioId });
        }

        public async Task Actualizar(CuentaCreacionViewModel cuenta)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE cuentas
                                            SET nombre = @Nombre, balance = @Balance, descripcion = @Descripcion, tipocuentaid = @TipoCuentaId
                                            WHERE id = @Id;", cuenta);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE cuentas WHERE id = @Id", new { id });
        }
    }
}
