using AdminEmpleadosEF;
using AdminEmpleadosEntidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AdminEmpleadosDatos
{
    public static class EmpleadosDatosEF
    {
        static AdminEmpleadosDBContext? empleadosContext;

        public static List<Empleado> Get(Empleado e)
        {
            empleadosContext = new AdminEmpleadosDBContext();

            if (empleadosContext.empleado == null)
            {
                return new List<Empleado>();
            }
            //Lazy Loading
            //List<Empleado> list = empleadosContext.empleado.ToList(); //sin departamentos

            List<Empleado> list;
            if (String.IsNullOrWhiteSpace(e.Nombre) && String.IsNullOrWhiteSpace(e.Dni))
            {
                list = empleadosContext.empleado.Include("Departamento")
                    //filtra segun el valor del parametro anulado enviado desde la capa de presentacion // -codig agre-//
                    .Where(emp => emp.anulado == e.anulado) // -codig agre-//
                    .ToList();
            }
            else
            {
                
                /*
                //con warnings, va a dar excepcion si nombre o dni estan nulos en la BD
                list = empleadosContext.empleado.Include("Departamento").Where(i =>
                    i.Nombre.Contains(e.Nombre)
                    ||
                    i.Dni.Contains(e.Dni)
                    ).ToList();
                */

                //? operador ternario (es como un IF-ELSE) 
                //?? operador de fusion de null (Asigna un valor cuando es NULL la variable de la izquierda)                
                list = empleadosContext.empleado.Include("Departamento").Where(i =>
                    (i.Nombre != null ? i.Nombre.Contains(e.Nombre ?? "") : true)
                    ||
                    (i.Dni != null ? i.Dni.Contains(e.Dni ?? "") : true)
                    )
                    //aplico nuevamente el filtro por anulados cuando el usuario esta buscando por nombre o dni // -codig agre-//
                    .Where(emp => emp.anulado == e.anulado) // -codig agre-//
                    .ToList();
            }


            return list;
        }

        public static int Insert(Empleado e)
        {
            empleadosContext = new AdminEmpleadosDBContext();

            if (empleadosContext == null)
            {
                return 0;
            }
            //seteo el ID en null para que realice el insert porque si tiene otro valor EF lo toma como un update
            e.EmpleadoId = null;
            e.anulado = false; // -codig agre-//
            empleadosContext.Add(e);
            empleadosContext.SaveChanges();
            if (e.EmpleadoId == null)
                return 0;

            return (int)e.EmpleadoId;

        }

        public static bool Update(Empleado e)
        {
            empleadosContext = new AdminEmpleadosDBContext();

            var empleadoBD = empleadosContext.empleado.FirstOrDefault(c => c.EmpleadoId == e.EmpleadoId);
            if (empleadoBD == null)
                return false;

            empleadoBD.Direccion = e.Direccion;
            empleadoBD.Dni = e.Dni;
            empleadoBD.Salario = e.Salario;
            empleadoBD.FechaIngreso = e.FechaIngreso;
            empleadoBD.Nombre = e.Nombre;
            empleadoBD.dpto_id = e.dpto_id;
            empleadoBD.anulado = e.anulado;

            empleadosContext.SaveChanges();

            return true;
        }

        public static bool Anular(int id)
        {
            empleadosContext = new AdminEmpleadosDBContext();

            var empleadoBD = empleadosContext.empleado.FirstOrDefault(c => c.EmpleadoId == id);
            if (empleadoBD == null)
                return false;

            empleadoBD.anulado = true;

            empleadosContext.SaveChanges();

            return true;
        }

        public static int DeleteAnulados() // -codig agre-//
        {
            empleadosContext = new AdminEmpleadosDBContext();

            if (empleadosContext.empleado == null)
            {
                //devuelvo cero porque no hay tabla para consultar (por ejemplo en una BD sin migraciones) // -codig agre-//
                return 0;
            }

            //busco todos los empleados marcados como anulados y los guardo en la lista solicitada // -codig agre-//
            List<Empleado> listaParaDeletear = empleadosContext.empleado
                .Where(emp => emp.anulado) // -codig agre-//
                .ToList(); // -codig agre-//

            if (listaParaDeletear.Count == 0)
            {
                //si la lista esta vacia no hay nada para borrar, retorno cero // -codig agre-//
                return 0;
            }

            //elimino todos los empleados anulados usando RemoveRange como pidio el requerimiento // -codig agre-//
            empleadosContext.empleado.RemoveRange(listaParaDeletear); // -codig agre-//

            //persisto los cambios en la base de datos // -codig agre-//
            empleadosContext.SaveChanges(); // -codig agre-//

            //retorno la cantidad de registros eliminados para informar al usuario // -codig agre-//
            return listaParaDeletear.Count; // -codig agre-//
        }
    }
}
