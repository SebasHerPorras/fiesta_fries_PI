// Detectar entorno
const isDevelopment = process.env.NODE_ENV === 'development';
const isProduction = process.env.NODE_ENV === 'production';

// Configuración de rutas por entorno
const API_CONFIG = {
  development: {
    BASE_URL: 'https://localhost:7056',
    API_PREFIX: '/api'
  },
  production: {
    BASE_URL: 'https://tu-servidor-produccion.com', // Cambiar por tu dominio real
    API_PREFIX: '/api'
  }
};

// Seleccionar configuración según entorno
const currentConfig = isProduction 
  ? API_CONFIG.production 
  : API_CONFIG.development;

// Exportar rutas
export const API_BASE_URL = currentConfig.BASE_URL;
export const API_PREFIX = currentConfig.API_PREFIX;

// Helper para construir URLs completas
const buildApiUrl = (endpoint) => {
  // Remover "/" inicial si existe
  const cleanEndpoint = endpoint.startsWith('/') ? endpoint.slice(1) : endpoint;
  return `${API_BASE_URL}${API_PREFIX}/${cleanEndpoint}`;
};

// Endpoints específicos
export const API_ENDPOINTS = {
  USER_LOGIN: buildApiUrl("user/login"),
  MIS_EMPRESAS: (userId) => buildApiUrl(`empresa/misEmpresas/${userId}`),
  MIS_EMPRESAS_ID: (userId) => buildApiUrl(`empresa/mis-empresas/${userId}`),
  EMPLEADOS: (cedulaEmpresa) =>
    buildApiUrl(`Empleado/empresa/${cedulaEmpresa}`),
  CREATE_EMPRESA: buildApiUrl("empresa"),
    CREATE_EMPLEADO: buildApiUrl("Person/empleado"),
    HIRE_DATE: (id) => `${buildApiUrl("Empleado/GetEmployeeHireDate")}?id=${encodeURIComponent(id)}`,
    WORK_DAY_HOURS: (dateW, dateD, id) => `${buildApiUrl("Empleado/GetEmployeeWorkDayHours")}?dateW=${encodeURIComponent(dateW)}&dateD=${encodeURIComponent(dateD)}&id=${encodeURIComponent(id)}`,
    WORK_WEEK_HOURS: (date, id) => `${buildApiUrl("Empleado/GetWorkHoursWeek")}?date=${encodeURIComponent(date)}&id=${encodeURIComponent(id)}`,
    ADD_WORK_DAY_HOURS: (dateW, dateD, hours, id) => `${buildApiUrl("Empleado/AddWorkHours")}?dateW=${encodeURIComponent(dateW)}&dateD=${encodeURIComponent(dateD)}&hours=${encodeURIComponent(hours)}&id=${encodeURIComponent(id)}`,

  EMPRESA_BY_EMPLOYEE: (userId) => buildApiUrl(`empresa/employee-company/${userId}`),

  BENEFICIOS_SELECCIONADOS: (empleadoId) => buildApiUrl(`EmployeeBenefit/${empleadoId}`),
  ELEGIR_BENEFICIO: buildApiUrl("EmployeeBenefit"),
  CAN_SELECT_BENEFIT: (employeeId, benefitId) => buildApiUrl(`EmployeeBenefit/can-select?employeeId=${employeeId}&benefitId=${benefitId}`),
  PERSON_BY_USER: (userGuid) => buildApiUrl(`person/by-user/${userGuid}`),
  EMPLOYEE_BENEFIT_SELECTED: (employeeId) => buildApiUrl(`EmployeeBenefit/selected?employeeId=${employeeId}`),
  PAYROLL_PREVIEW: buildApiUrl("payroll/preview"),


  CREATE_BENEFICIO: buildApiUrl("Beneficio"),
  UPDATE_BENEFICIO: (id) => buildApiUrl(`Beneficio/${id}`),
  GET_BENEFICIO: (id) => buildApiUrl(`Beneficio/${id}`),
  DELETE_BENEFICIO: (id) => buildApiUrl(`Beneficio/${id}`),


  ID_VALIDATE: buildApiUrl("idverification/idvalidate"),
  USER_EMAIL_VERIFY: (email) =>
    `${buildApiUrl("user/emailverify")}?email=${encodeURIComponent(email)}`,
  USER_CREATE_EMPLOYER: buildApiUrl("user/createUserEmployer"),
  PERSON_CREATE: buildApiUrl("person/create"),
  EMPLEADO_CREATE_WITH_PERSON: buildApiUrl("Empleado/create-with-person"),

  EMPLEADO_UPDATE_BY_ID: (id) => buildApiUrl(`Empleado/${id}`),
  EMPLEADO_GET_BY_ID: (id) => buildApiUrl(`Empleado/${id}`),

  USER_NOTIFY_EMPLOYER: buildApiUrl("Empleado/EmailNotificationMessage"),
  PERSON_PROFILE: (userId) => buildApiUrl(`person/profile/${userId}`),
  EMPRESAS_TODAS: buildApiUrl("empresa/todas"),
  EMPRESAS_BY_USER: (personaId) => buildApiUrl(`empresa/byUser/${personaId}`),
  GET_EMPRESA_POR_CEDULA: cedula => buildApiUrl(`/empresa/por-cedula/${cedula}`),
  MODIFICAR_EMPRESA_PROPIA: (cedula) => buildApiUrl(`/empresa/modificar-empresa/${cedula}`),

  VALIDAR_MODIFICACION_BENEFICIOS: (cedula) =>
  buildApiUrl(`empresa/validacion/modificacion-beneficios/${cedula}`),


  BENEFICIOS_POR_EMPRESA: (cedulaEmpresa) =>
    buildApiUrl(`Beneficio/por-empresa/${cedulaEmpresa}`),
  USER_CREATE: buildApiUrl("user/create"),
  EMPLEADO_CREATE: buildApiUrl("Empleado/create"),
  PAYROLLS: (cedulaEmpresa) => buildApiUrl(`Payroll/company/${cedulaEmpresa}`),
  PAYROLL_PROCESS: buildApiUrl("Payroll/process"),
  PAYROLL_NEXT_PERIOD: (companyId) => buildApiUrl(`Payroll/${companyId}/next-period`),
  PAYROLL_PENDING_PERIODS: (companyId) => buildApiUrl(`Payroll/${companyId}/pending-periods`),
  PAYROLL_OVERDUE_PERIODS: (companyId) => buildApiUrl(`Payroll/${companyId}/overdue-periods`),

  // Reportes de Planilla
  PAYROLL_REPORT_LAST_12: (companyId) => buildApiUrl(`PayrollReport/company/${companyId}/last-12`),
  PAYROLL_REPORT_PDF: (payrollId) => buildApiUrl(`PayrollReport/${payrollId}/pdf`),
  PAYROLL_REPORT_CSV: (payrollId) => buildApiUrl(`PayrollReport/${payrollId}/csv`),
  PAYROLL_REPORT_JSON: (payrollId) => buildApiUrl(`PayrollReport/${payrollId}`),

};

// Log de configuración (solo en desarrollo)
if (isDevelopment) {
  console.log('🔧 API Configuration:');
  console.log('   Environment:', process.env.NODE_ENV);
  console.log('   Base URL:', API_BASE_URL);
  console.log('   API Prefix:', API_PREFIX);
  console.log('   Example endpoint:', API_ENDPOINTS.USER_LOGIN);
}

export default {
  API_BASE_URL,
  API_PREFIX,
  API_ENDPOINTS,
  buildApiUrl,
  isDevelopment,
  isProduction
};