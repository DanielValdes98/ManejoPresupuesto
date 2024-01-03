using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using NuGet.Versioning;

namespace ManejoPresupuesto.Servicios
{

    public interface IRepositorioTiposCuentas
    {
        Task Actualizar(TipoCuenta tipoCuenta);
        Task Borrar(int id);
        Task Crear(TipoCuenta tipoCuenta);
        Task<bool> Existe(string nombre, int usuarioid);
        Task<IEnumerable<TipoCuenta>> Obtener(int usuarioid);
        Task<TipoCuenta> ObtenerPorId(int id, int usuarioid);
        Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados);
    }
    public class RepositorioTiposCuentas : IRepositorioTiposCuentas
    {
        private readonly string connectionString;
        public RepositorioTiposCuentas(IConfiguration configuration) 
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("SP_TiposCuentasInsertar",
                                                            new {usuarioid = tipoCuenta.Usuarioid,
                                                            nombre = tipoCuenta.Nombre},
                                                            commandType: System.Data.CommandType.StoredProcedure);
            tipoCuenta.Id = id;
        }

        public async Task<bool> Existe(string nombre, int usuarioid)
        {
            using var connection = new SqlConnection(connectionString);
            // QueryFirstOrDefaultAsync : consulta que trae el primer registro si hay, sino un 0 por defectos
            var existe = await connection.QueryFirstOrDefaultAsync<int>(
                @"SELECT 1 FROM tiposcuentas " +
                "WHERE nombre = @Nombre AND usuarioid = @Usuarioid;",
                new {nombre, usuarioid});

            return existe == 1;
        }

        public async Task<IEnumerable<TipoCuenta>> Obtener(int usuarioid)
        {
            using var connection = new SqlConnection(connectionString);
            // QueryAsync: realiza un query de select y trae un conjunto de resultados a un tipo de datos especifico (TipoCuenta)
            return await connection.QueryAsync<TipoCuenta>(
                @"SELECT id, nombre, orden FROM tiposcuentas " +
                "WHERE usuarioid = @Usuarioid ORDER BY orden;", new { usuarioid });
        }

        public async Task Actualizar(TipoCuenta tipoCuenta)
        {
            using var connection = new SqlConnection(connectionString);
            // ExecuteAsync : Ejecuta un query que no retorna nada
            await connection.ExecuteAsync(@"UPDATE tiposcuentas
                                            SET nombre = @Nombre
                                            WHERE id = @Id;", tipoCuenta);
        }

        public async Task<TipoCuenta> ObtenerPorId(int id, int usuarioid)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<TipoCuenta>(
                                    @"SELECT id, nombre, orden
                                    FROM tiposcuentas 
                                    WHERE id = @Id AND usuarioid = @Usuarioid;", new {id, usuarioid});
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE tiposcuentas
                                            WHERE id = @Id",  new { id });
        }

        public async Task Ordenar(IEnumerable<TipoCuenta> tipoCuentasOrdenados)
        {
            var query = "UPDATE tiposcuentas SET orden = @Orden WHERE id = @Id;";
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(query, tipoCuentasOrdenados);
        }

    }
}
