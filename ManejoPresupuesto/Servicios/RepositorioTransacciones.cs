using Dapper;
using ManejoPresupuesto.Models;
using Microsoft.Data.SqlClient;
using System.Configuration;

namespace ManejoPresupuesto.Servicios
{

    public interface IRepositorioTransacciones
    {
        Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnterior);
        Task Borrar(int id);
        Task Crear(Transaccion transaccion);
        Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo);
        Task<Transaccion> ObtenerPorId(int id, int usuarioId);
        Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int annio);
        Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo);
        Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo);
    }

    public class RepositorioTransacciones : IRepositorioTransacciones
    {
        private readonly string connectionString;

        public RepositorioTransacciones(IConfiguration configuration) 
        {
            connectionString = configuration.GetConnectionString("DefaultConnection"); 
        }

        public async Task Crear(Transaccion transaccion)
        {
            using var connection = new SqlConnection(connectionString);
            var id = await connection.QuerySingleAsync<int>("SP_transacciones_insertar",
                new
                {
                    transaccion.UsuarioId,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota
                },
                commandType: System.Data.CommandType.StoredProcedure);

            transaccion.Id = id;
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorCuentaId(ObtenerTransaccionesPorCuenta modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>
                (@"SELECT t.id, t.monto, t.fechatransaccion,
                    c.nombre AS Categoria, cu.nombre AS Cuenta,
                    c.tipotransaccionid 
                    FROM transacciones t 
                    INNER JOIN categorias  c
                    ON c.id = t.categoriaid
                    INNER JOIN cuentas cu
                    ON cu.id = t.cuentaid
                    WHERE t.cuentaid = @CuentaId AND t.usuarioid = @UsuarioId
                    AND fechatransaccion BETWEEN @FechaInicio AND @FechaFin", modelo);
        }

        public async Task<IEnumerable<Transaccion>> ObtenerPorUsuarioId(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<Transaccion>
                (@"SELECT t.id, t.monto, t.fechatransaccion,
                    c.nombre AS Categoria, cu.nombre AS Cuenta,
                    c.tipotransaccionid, nota
                    FROM transacciones t 
                    INNER JOIN categorias  c
                    ON c.id = t.categoriaid
                    INNER JOIN cuentas cu
                    ON cu.id = t.cuentaid
                    WHERE t.usuarioid = @UsuarioId
                    AND fechatransaccion BETWEEN @FechaInicio AND @FechaFin
                    ORDER BY t.fechatransaccion DESC", modelo);
        }

        public async Task Actualizar(Transaccion transaccion, decimal montoAnterior, int cuentaAnteriorId)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("SP_transacciones_actualizar",
                new
                {
                    transaccion.Id,
                    transaccion.FechaTransaccion,
                    transaccion.Monto,
                    transaccion.CategoriaId,
                    transaccion.CuentaId,
                    transaccion.Nota,
                    montoAnterior,
                    cuentaAnteriorId
                }, commandType: System.Data.CommandType.StoredProcedure);
        }

        public async Task<Transaccion> ObtenerPorId(int id, int usuarioId)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryFirstOrDefaultAsync<Transaccion>(
                @"SELECT transacciones.*, cat.tipotransaccionid AS tipooperacionid
                FROM transacciones
                INNER JOIN categorias cat 
                ON cat.id = transacciones.categoriaid
                WHERE transacciones.id = @Id AND transacciones.usuarioid = @UsuarioId;", new {id, usuarioId});
        }

        public async Task<IEnumerable<ResultadoObtenerPorSemana>> ObtenerPorSemana(ParametroObtenerTransaccionesPorUsuario modelo)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorSemana>(@"
                            SELECT DATEDIFF(d, @fechaInicio, fechatransaccion) / 7 + 1 AS Semana,
                            SUM(monto) AS Monto, cat.tipotransaccionid AS TipoOperacionId
                            FROM transacciones t
                            INNER JOIN categorias cat
                            ON cat.id = t.categoriaid
                            WHERE
                            t.usuarioid = @usuarioId AND
                            fechatransaccion BETWEEN @fechaInicio AND @fechaFin
                            GROUP BY DATEDIFF(d, @fechaInicio, fechatransaccion) / 7, cat.tipotransaccionid", modelo);
        }
        
        public async Task<IEnumerable<ResultadoObtenerPorMes>> ObtenerPorMes(int usuarioId, int annio)
        {
            using var connection = new SqlConnection(connectionString);
            return await connection.QueryAsync<ResultadoObtenerPorMes>(@"
                            SELECT MONTH(fechatransaccion) AS Mes, 
                            SUM(monto) AS Monto, cat.tipotransaccionid AS TipoOperacionId
                            FROM transacciones 
                            INNER JOIN categorias cat
                            ON cat.id = transacciones.categoriaid
                            WHERE transacciones.usuarioid = @usuarioId AND YEAR(fechatransaccion) = @annio
                            GROUP BY Month(fechatransaccion), cat.tipotransaccionid;", new { usuarioId, annio } );
        }

        public async Task Borrar(int id)
        {
            using var connection = new SqlConnection(connectionString);
            await connection.ExecuteAsync("SP_transacciones_borrar",
                new {id}, commandType: System.Data.CommandType.StoredProcedure);
        }
    }
}
