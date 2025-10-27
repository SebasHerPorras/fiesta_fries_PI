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

  EMPRESA_BY_EMPLOYEE: (userId) => buildApiUrl(`empresa/employee-company/${userId}`),

  BENEFICIOS_SELECCIONADOS: (empleadoId) => buildApiUrl(`EmployeeBenefit/${empleadoId}`),
  ELEGIR_BENEFICIO: buildApiUrl("EmployeeBenefit"),
  CAN_SELECT_BENEFIT: (employeeId, benefitId) => buildApiUrl(`EmployeeBenefit/can-select?employeeId=${employeeId}&benefitId=${benefitId}`),
  PERSON_BY_USER: (userGuid) => buildApiUrl(`person/by-user/${userGuid}`),
  EMPLOYEE_BENEFIT_SELECTED: (employeeId) => buildApiUrl(`EmployeeBenefit/selected?employeeId=${employeeId}`),


  CREATE_BENEFICIO: buildApiUrl("Beneficio"),
  ID_VALIDATE: buildApiUrl("idverification/idvalidate"),
  USER_EMAIL_VERIFY: (email) =>
    `${buildApiUrl("user/emailverify")}?email=${encodeURIComponent(email)}`,
  USER_CREATE_EMPLOYER: buildApiUrl("user/createUserEmployer"),
  PERSON_CREATE: buildApiUrl("person/create"),
  EMPLEADO_CREATE_WITH_PERSON: buildApiUrl("Empleado/create-with-person"),
  USER_NOTIFY_EMPLOYER: buildApiUrl("Empleado/EmailNotificationMessage"),
  PERSON_PROFILE: (userId) => buildApiUrl(`person/profile/${userId}`),
  EMPRESAS_TODAS: buildApiUrl("empresa/todas"),
  EMPRESAS_BY_USER: (personaId) => buildApiUrl(`empresa/byUser/${personaId}`),

  BENEFICIOS_POR_EMPRESA: (cedulaEmpresa) =>
    buildApiUrl(`Beneficio/por-empresa/${cedulaEmpresa}`),
  USER_CREATE: buildApiUrl("user/create"),
  EMPLEADO_CREATE: buildApiUrl("Empleado/create"),
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