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
                    //LINQ: la expresion lambda emp => emp.anulado == e.anulado compara el campo anulado de cada empleado con el valor que llega del checkbox
                    .Where(emp => emp.anulado == e.anulado)
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
                //LINQ: la lambda i => evalua cada registro para filtrar por nombre o DNI segun el texto ingresado
                list = empleadosContext.empleado.Include("Departamento").Where(i =>
                    (i.Nombre != null ? i.Nombre.Contains(e.Nombre ?? "") : true)
                    ||
                    (i.Dni != null ? i.Dni.Contains(e.Dni ?? "") : true)
                    )
                    //LINQ: la segunda lambda emp => emp.anulado == e.anulado vuelve a filtrar por el estado anulado elegido por el usuario
                    .Where(emp => emp.anulado == e.anulado)
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
            e.anulado = false;
            empleadosContext.Add(e);
            empleadosContext.SaveChanges();
            if (e.EmpleadoId == null)
                return 0;

            return (int)e.EmpleadoId;

        }

        public static bool Update(Empleado e)
        {
            empleadosContext = new AdminEmpleadosDBContext();

            //LINQ: la lambda c => c.EmpleadoId == e.EmpleadoId busca el primer registro cuyo ID coincide con el que se quiere modificar
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

            //LINQ: la lambda c => c.EmpleadoId == id localiza el empleado a anular comparando los IDs
            var empleadoBD = empleadosContext.empleado.FirstOrDefault(c => c.EmpleadoId == id);
            if (empleadoBD == null)
                return false;

            empleadoBD.anulado = true;

            empleadosContext.SaveChanges();

            return true;
        }

        public static int DeleteAnulados()
        {
            empleadosContext = new AdminEmpleadosDBContext();

            if (empleadosContext.empleado == null)
            {
                //devuelvo cero porque no hay tabla para consultar (por ejemplo en una BD sin migraciones)
                return 0;
            }

            //busco todos los empleados marcados como anulados y los guardo en la lista solicitada
            List<Empleado> listaParaDeletear = empleadosContext.empleado
                //LINQ: la lambda emp => emp.anulado devuelve solo los empleados cuyo campo anulado es verdadero
                .Where(emp => emp.anulado)
                .ToList();

            if (listaParaDeletear.Count == 0)
            {
                //si la lista esta vacia no hay nada para borrar, retorno cero
                return 0;
            }

            //elimino todos los empleados anulados usando RemoveRange como pidio el requerimiento
            empleadosContext.empleado.RemoveRange(listaParaDeletear);

            //persisto los cambios en la base de datos
            empleadosContext.SaveChanges();

            //retorno la cantidad de registros eliminados para informar al usuario
            return listaParaDeletear.Count;
        }
    }
}
