using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Ucaldas.Models;

namespace Ucaldas.Controllers
{
    public class MonitoriaController : Controller
    {
        private Entities bd = new Entities();
        private int numRegistros;
        // GET: Monitoria
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Metodo GET CrearMonitoria redirecciona a la vista para crear las monitorias
        /// </summary>
        /// <returns>Vista crear monitoria </returns>
        public ActionResult CrearMonitoria()
        {
            return View(this);
        }

        /// <summary>
        /// Metodo POST CrearMonitoria recibe los datos necesarios y hace las validaciones correspondientes para generar el registro de una 
        /// nueva monitoria.
        /// </summary>
        /// <param name="COD_ESTUDIANTE"> codigo del estudiante</param>
        /// <param name="TIPO_MONITORIA"> id del tipo de monitoria</param>
        /// <param name="COD_ACT_ACADEMICA"> codigo de la actividad academica</param>
        /// <param name="HORAS_REPORTADAS"> numero de horas reportadas</param>
        /// <param name="HORAS_PAGAS"> numero de horas a pagar</param>
        /// <param name="PERIODO"> periodo academico en que se realizo la monitoria</param>
        /// <returns> Mensaje indicando si la operacion fue exitosa o se presento algun problema</returns>
        [HttpPost]
        public ActionResult CrearMonitoria(decimal COD_ESTUDIANTE, string TIPO_MONITORIA, string COD_ACT_ACADEMICA, decimal HORAS_REPORTADAS, decimal HORAS_PAGAS, string PERIODO)
        {
            if (validarExistenciaEstudiante(COD_ESTUDIANTE)) // Valida que el estudiante exista
            {
                if (!validarMonitoriaAcademica(TIPO_MONITORIA)) // valida que no sea monitoria academica
                {
                    if (!validarExistenciaMonitoria(COD_ESTUDIANTE, PERIODO)) //valida que no exista ya una monitoria para ese estudiante en ese periodo
                    {
                        decimal pagoTotal = calcularPagoMonitoria(TIPO_MONITORIA, HORAS_PAGAS);
                        MONITORIA nueva = new MONITORIA()
                        {
                            ID_ESTUDIANTE = COD_ESTUDIANTE,
                            PERIODO = PERIODO,
                            FECHA = DateTime.Now,
                            ID_TIPO_MONITORIA = TIPO_MONITORIA,
                            ID_ACTIVIDAD_ACADM = "-1",
                            HORAS_REPORTADAS = HORAS_REPORTADAS,
                            HORAS_PAGAS = HORAS_PAGAS,
                            TOTAL = pagoTotal,
                            ESTADO = 1
                        };
                        bd.MONITORIA.Add(nueva);
                        bd.SaveChangesAsync();
                        programarAlertSalida("Operacion Exitosa", "Se realizo correctamente el registro de la monitoria", "success", true); //Msg se creo la monitoria
                    }
                    else
                    {
                        programarAlertSalida("Error", "El estudiante ya tiene registrado una monitoria para el periodo " + PERIODO, "error", false); //Msg el estudiante ya tiene una monitoria para ese periodo
                    }
                }
                else
                {
                    if (validarExistenciaActividadAcademica(COD_ACT_ACADEMICA)) //valida que exista la actividad academica que ingreso
                    {
                        if (validarRequisitoNotaMonitoriaAcademica(COD_ESTUDIANTE, COD_ACT_ACADEMICA, PERIODO)) //valida que haya cursado esa actividad academica y que la nota fuera superior o igual a la establecida en variables del sistema
                        {
                            if (!validarExistenciaMonitoria(COD_ESTUDIANTE, PERIODO)) //valida que no exista ya una monitoria para ese estudiante en ese periodo
                            {
                                decimal pagoTotal = calcularPagoMonitoria(TIPO_MONITORIA, HORAS_PAGAS);
                                MONITORIA nueva = new MONITORIA()
                                {
                                    ID_ESTUDIANTE = COD_ESTUDIANTE,
                                    PERIODO = PERIODO,
                                    FECHA = DateTime.Now,
                                    ID_TIPO_MONITORIA = TIPO_MONITORIA,
                                    ID_ACTIVIDAD_ACADM = COD_ACT_ACADEMICA,
                                    HORAS_REPORTADAS = HORAS_REPORTADAS,
                                    HORAS_PAGAS = HORAS_PAGAS,
                                    TOTAL = pagoTotal,
                                    ESTADO = 1
                                };
                                bd.MONITORIA.Add(nueva);
                                bd.SaveChangesAsync();
                                programarAlertSalida("Operacion Exitosa", "Se realizo correctamente el registro de la monitoria", "success", true); //Msg se creo la monitoria
                            }
                            else
                            {
                                programarAlertSalida("Error", "El estudiante ya tiene registrado una monitoria para el periodo " + PERIODO, "error", false); //Msg el estudiante ya tiene una monitoria para ese periodo
                            }
                        }
                        else
                        {
                            programarAlertSalida("Error", "El estudiante no curso la actividad academica o su nota no es mayor o igual a " + variablesSistema.notaParaMonitoriaAcademica, "error", false); //Msg el estudiante no cumple con los requisitos
                        }
                    }
                    else
                    {
                        programarAlertSalida("Error", "El codigo de la actividad academica ingresado no existe, intente nuevamente", "error", false); //Msg la actividad academica no existe
                    }
                }
            }
            else
            {
                programarAlertSalida("Error", "El codigo de estudiante" + COD_ESTUDIANTE + " no esta registrado", "error", false); //Msg el estudiante no existe
            }
            return View(this);
        }
        #region metodos crear monitoria
        public string[] obtenerPeriodos()
        {
            string[] arreglo = { DateTime.Now.Year + "-I", DateTime.Now.Year + "-II" };
            return arreglo;
        }

        public List<TIPO_MONITORIA> obtenerTiposMonitoriaActivos()
        {
            List<TIPO_MONITORIA> lista = bd.TIPO_MONITORIA.Where(x => x.ESTADO == 1).ToList();
            return lista;
        }

        private void programarAlertSalida(string titulo, string mensaje, string tipo, bool salidaAutomatica)
        {
            ViewBag.viewMessage = true;
            ViewBag.TitleMSG = titulo;
            ViewBag.MessageMSG = mensaje;
            ViewBag.IconMSG = tipo;
            ViewBag.close = salidaAutomatica;
        }

        private bool validarExistenciaEstudiante(decimal codigoEstudiante)
        {
            foreach (ESTUDIANTE estudiante in bd.ESTUDIANTE.Where(x => x.ESTADO == 1).ToList())
            {
                if (estudiante.CODIGO == (codigoEstudiante))
                {
                    return true;
                }
            }
            return false;
        }

        private bool validarMonitoriaAcademica(string idTipoMonitoria)
        {
            TIPO_MONITORIA tipo = bd.TIPO_MONITORIA.Find(idTipoMonitoria);
            if (tipo.NOMBRE.ToUpper().Equals("MONITORIA ACADEMICA"))
            {
                return true;
            }
            return false;
        }

        private decimal calcularPagoMonitoria(string idTipoMonitoria, decimal horasPagas)
        {
            TIPO_MONITORIA tipo = bd.TIPO_MONITORIA.Find(idTipoMonitoria);
            decimal resultado = tipo.VALOR_HORA * horasPagas;
            return resultado;
        }

        private bool validarExistenciaMonitoria(decimal codEstudiante, string periodo)
        {
            MONITORIA monitoria = bd.MONITORIA.Find(codEstudiante, periodo);
            if (monitoria != null)
            {
                return true;
            }
            return false;
        }

        private bool validarExistenciaActividadAcademica(string codActividadAcad)
        {
            ACTIVIDAD_ACADEMICA actividad = bd.ACTIVIDAD_ACADEMICA.Find(codActividadAcad);
            if (actividad != null)
            {
                return true;
            }
            return false;
        }

        private bool validarRequisitoNotaMonitoriaAcademica(decimal codEstudiante, string codActAcademica, string periodo)
        {
            ACT_ACAD_ESTUDIANTE resultado = bd.ACT_ACAD_ESTUDIANTE.Find(codEstudiante, codActAcademica, periodo);
            if (resultado != null)
            {
                if (resultado.NOTA >= (decimal)variablesSistema.notaParaMonitoriaAcademica)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        /// <summary>
        /// Metodo GET ConsultarMonitoria redirecciona a la vista para consultar las monitorias
        /// </summary>
        /// <returns>vista consultar monitoria</returns>
        public ActionResult ConsultarMonitoria()
        {
            return View();
        }

        /// <summary>
        /// Metodo POST ConsultarMonitoria se encarga de consultar en los registros de monitorias cual concuerda con el codigo de estudiante
        /// ingresado y el periodo igualmente ingresado.
        /// </summary>
        /// <param name="COD_ESTUDIANTE">Codigo del estudiante</param>
        /// <param name="PERIODO"> Periodo academico en el que se realizo la monitoria</param>
        /// <returns>vista con los resultados en pantalla, en caso de haber error muestra el respectivo mensaje</returns>
        [HttpPost]
        public ActionResult ConsultarMonitoria(decimal COD_ESTUDIANTE, string PERIODO)
        {
            if (validarExistenciaMonitoria(COD_ESTUDIANTE, PERIODO))
            {
                MONITORIA monitoria = bd.MONITORIA.Find(COD_ESTUDIANTE, PERIODO);
                return View(monitoria);
            }
            else
            {
                if (validarExistenciaEstudiante(COD_ESTUDIANTE))
                {
                    if (validarEstructuraPeriodo(PERIODO))
                    {
                        programarAlertSalida("Informacion", "El estudiante ingresado actualmente no a realizado monitorias para ese periodo", "info", true); //Msg el estudiante no a realizado monitorias en ese periodo
                    }
                    else
                    {
                        programarAlertSalida("Error", "El formato de periodo ingresado es incorrecto, recuerde que el formato esta dado por  (YYYY)(-)(I o II), por ejemplo 2018-I, 2019-II", "error", false); //Msg formato de periodo incorrecto
                    }
                }
                else
                {
                    programarAlertSalida("Error", "El codigo de estudiante " + COD_ESTUDIANTE + " no esta registrado", "error", false); //Msg el estudiante no existe
                }
            }
            return View();
        }

        #region metodos consultar monitoria
        private bool validarEstructuraPeriodo(string periodo)
        {
            if (periodo.EndsWith("-I") || periodo.EndsWith("-II"))
            {
                string num = periodo.Substring(0, 4);
                try
                {
                    int numero = int.Parse(num);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }
            return false;
        }

        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult ModificarMonitoria()
        {
            ViewBag.CESTUDIANTE = "";
            ViewBag.CPERIODO = "";
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="COD_ESTUDIANTE"></param>
        /// <param name="PERIODO"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ModificarMonitoria(decimal COD_ESTUDIANTE, string PERIODO, string HORAS_REPORTADAS, string HORAS_PAGAS, string ESTADO)
        {
            if (validarExistenciaEstudiante(COD_ESTUDIANTE)) //valida el codigo del estudiante
            {
                if (validarEstructuraPeriodo(PERIODO)) //valida la estructura del periodo ingresado
                {
                    if (validarExistenciaMonitoria(COD_ESTUDIANTE, PERIODO))
                    {
                        ViewBag.CESTUDIANTE = COD_ESTUDIANTE;
                        ViewBag.CPERIODO = PERIODO;
                        MONITORIA monitoria = bd.MONITORIA.Find(COD_ESTUDIANTE, PERIODO);
                        if (HORAS_REPORTADAS != null && HORAS_PAGAS != null && ESTADO != null)
                        {
                            monitoria.HORAS_REPORTADAS = decimal.Parse(HORAS_REPORTADAS);
                            monitoria.HORAS_PAGAS = decimal.Parse(HORAS_PAGAS);
                            decimal pagoTotal = calcularPagoMonitoria(monitoria.ID_TIPO_MONITORIA, decimal.Parse(HORAS_PAGAS));
                            monitoria.TOTAL = pagoTotal;
                            monitoria.ESTADO = decimal.Parse(ESTADO);
                            bd.SaveChangesAsync();
                            ViewBag.CESTUDIANTE = "";
                            ViewBag.CPERIODO = "";
                            programarAlertSalida("Operacion Exitosa", "Se registraron los cambios correctamente", "success", true);
                        }
                        else
                        {
                            return View(monitoria);
                        }
                    }
                    else
                    {
                        programarAlertSalida("Informacion", "El estudiante ingresado actualmente no a realizado monitorias para ese periodo", "info", true); //Msg el estudiante no a realizado monitorias en ese periodo
                    }

                }
                else
                {
                    programarAlertSalida("Error", "El formato de periodo ingresado es incorrecto, recuerde que el formato esta dado por  (YYYY)(-)(I o II), por ejemplo 2018-I, 2019-II", "error", false); //Msg formato de periodo incorrecto
                }
            }
            else
            {
                programarAlertSalida("Error", "El codigo de estudiante " + COD_ESTUDIANTE + " no esta registrado", "error", false); //Msg el estudiante no existe
            }
            return View();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public ActionResult cargarMonitoriaCSV()
        {
            return View(this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="archivo"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult cargarMonitoriaCSV(HttpPostedFileBase ARCHIVO_CSV, string PERIODO)
        {
            numRegistros = 0;
            string rutaArchivo = string.Empty;
            if (ARCHIVO_CSV != null)
            {
                string ruta = Server.MapPath("~/ArchivosMONITORIA/");
                if (!Directory.Exists(ruta))
                {
                    Directory.CreateDirectory(ruta);
                }
                string fecha = DateTime.Today.ToShortDateString().Replace('/', '-');
                rutaArchivo = ruta + "-" + fecha + Path.GetFileName(ARCHIVO_CSV.FileName);

                ARCHIVO_CSV.SaveAs(rutaArchivo);


                string csvData = System.IO.File.ReadAllText(rutaArchivo);
                foreach (string fila in csvData.Split('\n'))
                {
                    if (!string.IsNullOrWhiteSpace(fila))
                    {
                        bool resultado = tratamientoLineaArchivoCSV(fila, PERIODO);
                        if (resultado)
                        {
                            numRegistros++;
                        }
                    }
                }
                if (ViewBag.viewMessage == null && numRegistros > 0)
                {
                    bd.SaveChangesAsync();
                    programarAlertSalida("Operacion exitosa", "se almacenaron correctamente los " + numRegistros + " registros", "success", true);
                }
            }
            else
            {
                programarAlertSalida("Error", "no se identifico el archivo que intento subir, intente nuevamente", "error", true);
            }
            return View(this);
        }

        private bool tratamientoLineaArchivoCSV(string linea, string periodo)
        {
            string[] datos = linea.Split(';');
            if (validarNumeroString(datos[0]))
            {
                if (!datos[1].Equals(""))
                {
                    if (!datos[5].Equals("") && validarNumeroString(datos[5]) && validarExistenciaEstudiante(decimal.Parse(datos[5])))
                    {
                        decimal idEstudiante = decimal.Parse(datos[5]);
                        if (!datos[9].Equals("") && validarExistenciaTipoMonitoria(datos[9]))
                        {
                            string tipoMonitoria = datos[9];
                            if (!validarMonitoriaAcademica(tipoMonitoria))
                            {
                                string actividaAcademica = "-1";

                                if (!validarExistenciaMonitoria(idEstudiante, periodo))
                                {
                                    if (!datos[12].Equals("") && !datos[13].Equals(""))
                                    {
                                        decimal horasReportadas = decimal.Parse(datos[12]);
                                        decimal horasPagas = decimal.Parse(datos[13]);

                                        decimal total = calcularPagoMonitoria(tipoMonitoria, horasPagas);

                                        MONITORIA monitoria = new MONITORIA()
                                        {
                                            ID_ESTUDIANTE = idEstudiante,
                                            PERIODO = periodo,
                                            FECHA = DateTime.Now,
                                            ID_TIPO_MONITORIA = tipoMonitoria,
                                            ID_ACTIVIDAD_ACADM = actividaAcademica,
                                            HORAS_REPORTADAS = horasReportadas,
                                            HORAS_PAGAS = horasPagas,
                                            TOTAL = total,
                                            ESTADO = 1
                                        };
                                        bd.MONITORIA.Add(monitoria);
                                        return true;
                                    }
                                }
                                else
                                {
                                    programarAlertSalida("Error", "El estudiante que registra en la fila " + (numRegistros + 1) + " ya tiene registrado una monitoria para el periodo " + periodo, "error", false);
                                }
                            }
                            else
                            {

                                if (!datos[8].Equals("") && validarExistenciaActividadAcademica(datos[8]))
                                {
                                    string actividaAcademica = datos[8];
                                    if (validarRequisitoNotaMonitoriaAcademica(idEstudiante, actividaAcademica, periodo))
                                    {
                                        if (!validarExistenciaMonitoria(idEstudiante, periodo))
                                        {
                                            if (!datos[12].Equals("") && !datos[13].Equals(""))
                                            {
                                                decimal horasReportadas = decimal.Parse(datos[12]);
                                                decimal horasPagas = decimal.Parse(datos[13]);

                                                decimal total = calcularPagoMonitoria(tipoMonitoria, horasPagas);

                                                MONITORIA monitoria = new MONITORIA()
                                                {
                                                    ID_ESTUDIANTE = idEstudiante,
                                                    PERIODO = periodo,
                                                    FECHA = DateTime.Now,
                                                    ID_TIPO_MONITORIA = tipoMonitoria,
                                                    ID_ACTIVIDAD_ACADM = actividaAcademica,
                                                    HORAS_REPORTADAS = horasReportadas,
                                                    HORAS_PAGAS = horasPagas,
                                                    TOTAL = total,
                                                    ESTADO = 1
                                                };
                                                bd.MONITORIA.Add(monitoria);
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            programarAlertSalida("Error", "El estudiante que registra en la fila " + (numRegistros + 1) + " ya tiene registrado una monitoria para el periodo " + periodo, "error", false);
                                        }

                                    }
                                    else
                                    {
                                        programarAlertSalida("Error", "El tipo de monitoria tiene unos requisitos, el estudiante que registra en la fila " + (numRegistros + 1) + " no cumple con los requisitos para realizar este tipo de monitoria", "error", false);
                                    }

                                }
                                else
                                {
                                    programarAlertSalida("Error", "La actividad academica que registra en la fila " + (numRegistros + 1) + " no se encuentra registrado", "error", false);
                                }
                            }
                        }
                        else
                        {
                            programarAlertSalida("Error", "El tipo de monitoria que registra en la fila " + (numRegistros + 1) + " no se encuentra registrado", "error", false);
                        }
                    }
                    else
                    {
                        programarAlertSalida("Error", "El codigo del estudiante que registra en la fila " + (numRegistros + 1) + " no se encuentra registrado", "error", false);
                    }
                }
            }
            return false;
        }

        private bool validarNumeroString(string numero)
        {
            try
            {
                decimal num = decimal.Parse(numero);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private bool validarExistenciaTipoMonitoria(string tipoMonitoria)
        {
            TIPO_MONITORIA tipo = bd.TIPO_MONITORIA.Find(tipoMonitoria);
            if (tipo != null)
            {
                return true;
            }
            return false;
        }
    }
}