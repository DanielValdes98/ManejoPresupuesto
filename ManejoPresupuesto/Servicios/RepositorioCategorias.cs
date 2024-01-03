using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace ManejoPresupuesto.Servicios
{

    public interface IRepositorioCategorias
    {
        Task Actualizar(Categoria categoria);
        Task Borrar(int id);
        Task Crear(Categoria categoria);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId);
        Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId);
        Task<Categoria> ObtenerPorId(int id, int usuarioId);
    }

    public class RepositorioCategorias : IRepositorioCategorias
    {
        private readonly string connectionString;

        public RepositorioCategorias(IConfiguration configuration)
        {
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task Crear(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>(@"
                                                        INSERT INTO categorias (nombre, tipotransaccionid, usuarioid)
                                                        Values (@Nombre, @TipoOperacionId, @UsuarioId);

                                                        SELECT SCOPE_IDENTITY();", categoria);
            categoria.Id = id;
        }

        public async Task<IEnumerable<Categoria>> Obtener (int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(@"SELECT id, nombre, tipotransaccionid AS tipooperacionid, usuarioid FROM categorias WHERE usuarioid = @UsuarioId", new { usuarioId });
        }

        public async Task<IEnumerable<Categoria>> Obtener(int usuarioId, TipoOperacion tipoOperacionId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Categoria>(@"SELECT id, nombre, tipotransaccionid AS tipooperacionid, usuarioid
                                                            FROM categorias 
                                                            WHERE usuarioid = @UsuarioId AND tipotransaccionid = @TipoOperacionId", new { usuarioId, tipoOperacionId });
        }

        public async Task<Categoria> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Categoria>(
                @"SELECT id, nombre, tipotransaccionid AS tipooperacionid, usuarioid FROM categorias WHERE id = @Id AND usuarioid = @UsuarioId",
                new { id, usuarioId });
        }

        public async Task Actualizar(Categoria categoria)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"UPDATE categorias
                                            SET nombre = @Nombre, tipotransaccionid = @TipoOperacionId
                                            WHERE id = @Id", categoria);
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync(@"DELETE categorias WHERE id = @Id", new {id});
        }
    }
}
