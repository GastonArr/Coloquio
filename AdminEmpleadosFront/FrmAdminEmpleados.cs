using AdminEmpleadosEntidades;
using AdminEmpleadosNegocio;

namespace AdminEmpleadosFront
{
    public partial class FrmAdminEmpleados : Form
    {
        List<Empleado> empleadosList = new List<Empleado>();

        public FrmAdminEmpleados()
        {
            InitializeComponent();
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            buscarEmpleados();
        }
        private void buscarEmpleados()
        {
            //Obtengo el nombre y DNI ingresado por el usuario
            string textoBuscar = txtBuscar.Text.Trim().ToUpper();

            //declaro el parametro
            Empleado parametro = new Empleado();

            //asigno el nombre ingresado
            if (!String.IsNullOrEmpty(textoBuscar.Trim()))
            {
                parametro.Nombre = textoBuscar;
                parametro.Dni = textoBuscar;
            }

            //seteo el nuevo filtro de anulados usando el valor del checkbox
            parametro.anulado = chkVerAnulados.Checked;

            //Busco la lista de empleados en la capa de negocio, pasandole el parametro ingresado
            empleadosList = EmpleadosNegocio.Get(parametro);
            //Actualizo la grilla
            refreshGrid();
        }

        private void refreshGrid()
        {
            //Actualizo el Binding con la lista de empleados que viene desde la BD
            empleadoBindingSource.DataSource = null;
            empleadoBindingSource.DataSource = empleadosList;

        }

        private void txtBuscar_KeyPress(object sender, KeyPressEventArgs e)
        {
            //Llamo al metodo buscar al presionar la tecla "Enter"
            if (e.KeyChar == (char)Keys.Enter)
            {
                buscarEmpleados();
            }
        }

        private void btnAlta_Click(object sender, EventArgs e)
        {
            FrmEditEmpleados frm = new FrmEditEmpleados();

            frm.modo = EnumModoForm.Alta;
            frm.ShowDialog();//modal

            buscarEmpleados();
        }

        private void btnModificar_Click(object sender, EventArgs e)
        {
            if (empleadoBindingSource.Current == null)
                return;

            FrmEditEmpleados frm = new FrmEditEmpleados();

            frm.modo = EnumModoForm.Modificacion;
            frm._empleado = (Empleado)empleadoBindingSource.Current;

            frm.ShowDialog();

            buscarEmpleados();
        }

        private void btnConsultar_Click(object sender, EventArgs e)
        {
            if (empleadoBindingSource.Current == null)
                return;

            FrmEditEmpleados frm = new FrmEditEmpleados();

            frm.modo = EnumModoForm.Consulta;
            frm._empleado = (Empleado)empleadoBindingSource.Current;

            frm.ShowDialog();

            buscarEmpleados();
        }

        private void btnBaja_Click(object sender, EventArgs e)
        {
            if (empleadoBindingSource.Current == null)
                return;

            Empleado emp = (Empleado)empleadoBindingSource.Current;

            //pregunto si quiere guardar los datos
            DialogResult res = MessageBox.Show("¿Confirma anular el empleado " + emp.Nombre + " ?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.No)
            {
                return;
            }

            try
            {
                EmpleadosNegocio.Anular((int)emp.EmpleadoId);
                MessageBox.Show("El empleado " + emp.Nombre + " se anuló correctamente", "Anulación", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);

            }

            buscarEmpleados();
        }

        private void btnCerrar_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void chkVerAnulados_CheckedChanged(object sender, EventArgs e)
        {
            //cada vez que el usuario tilda o destilda el check actualizo la grilla para respetar el filtro
            buscarEmpleados();
        }

        private void btnBorrarAnulados_Click(object sender, EventArgs e)
        {
            //primer mensaje de advertencia para evitar borrar registros por error
            DialogResult confirmarPrimeraVez = MessageBox.Show("¿Desea quitar de la BD todos los empleados anulados?", "Advertencia", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmarPrimeraVez != DialogResult.Yes)
            {
                //si la respuesta es No salgo del metodo
                return;
            }

            //segundo mensaje de confirmacion definitiva como pide el requerimiento
            DialogResult confirmarSegundaVez = MessageBox.Show("¿Confirma que va a borrar definitivamente de la BD los empleados anulados?", "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmarSegundaVez != DialogResult.Yes)
            {
                //si la respuesta es No salgo del metodo
                return;
            }

            try
            {
                //ejecuto la eliminacion definitiva y guardo cuantos empleados fueron borrados
                int cantidadEliminados = EmpleadosNegocio.DeleteAnulados();

                //informo el resultado al usuario mostrando la cantidad que salio de la capa de datos
                MessageBox.Show($"Se borraron definitivamente {cantidadEliminados} empleados anulados.", "Resultado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                //muestro cualquier error inesperado
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            //refresco la grilla para que desaparezcan los empleados eliminados
            buscarEmpleados();
        }


    }
}
