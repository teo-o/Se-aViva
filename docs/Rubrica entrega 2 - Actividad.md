# Actividad de la Guía de Trabajo No. 2

**Taller evaluativo 20%**

## 1. Identificación de la asignatura

- **Asignatura:** Arquitectura de Software II
- **Código:** 190304011
- **Área:** Sistemas
- **Plan de Estudios:** 
- **Semestre:** 
- **Créditos:** 3
- **TPS:** 
- **TIS:** 
- **TPT:** 
- **TIT:** 
- **Trabajo Independiente:** 
- **Trabajo Presencial:** 
- **Trabajo Teórico:** 
- **Trabajo Práctico:** 
- **Trabajo Teórico:** 
- **Trabajo Práctico:** 

## 1. Actividad

### Instrucciones generales

Cada grupo (máximo 3 o 4 participantes) teniendo ya su escenario propio del primer momento y teniendo en cuenta que lo debe mantener como caso de estudio a lo largo del semestre.

Realizar las siguientes configuraciones de servicios para su funcionamiento.

La entrega debe realizarse en dos formatos y solo la debe cargar un integrante del grupo en la plataforma:

1. **Documento escrito** (1 por grupo) que incluya todo el material solicitado.
2. **Video de exposición** (1 por grupo) de entre 5 y 7 minutos, donde todos los integrantes participen explicando las secciones.

### Roles dentro del grupo

- **Desarrollador Frontend:** interfaz y experiencia de usuario.
- **Desarrollador Backend:** lógica de negocio y patrones aplicados.
- **Arquitecto de Software:** integración de decisiones técnicas y justificación de patrones.
- **Encargado de Base de Datos:** diseño e implementación del modelo de datos.

**Documentación de Apis de Auth0:** https://auth0.com/docs/api/management/v2

Cada integrante debe asumir uno de estos roles y participar activamente en la entrega escrita y en el video.

## 1. Contexto del Escenario (1.0 punto)

- Breve descripción del sistema y su objetivo.
- Identificar los módulos principales del aplicativo (usuarios, mascotas, pedidos, artículos, facturas, etc.).
- Justificación de la relevancia del escenario elegido.

## 2. CRUD Completo (1.0 punto)

- Implementar todas las operaciones CRUD (crear, leer, actualizar, eliminar) para:
  - El módulo de usuarios.
  - Al menos un elemento adicional del escenario (ejemplo: mascotas, pedidos, artículos, facturas, reservas, clientes, etc.).
  - Debe estar documentado con capturas de pantalla o fragmentos de código clave.

## 3. Autenticación con Auth0 (1.0 punto)

- Integrar el login en el aplicativo utilizando Auth0.
- La interfaz de autenticación debe ser la del aplicativo (no usar la plantilla por defecto de Auth0, sino personalizarla con creatividad).
- En el registro se debe capturar además de correo y contraseña:
  - Nombre completo del usuario → guardarlo en el campo `name`.
  - Flujo adicional: formulario posterior que solicite tipo y número de documento.
  - Esta información debe almacenarse como metadata del perfil en Auth0.

## 4. Diagramación y Modelo de Datos (1.0 punto)

- Reutilizar la diagramación general del sistema en Draw.io, incluyendo módulos, actores y conexiones principales.
- Reutilizar el diseño ya entregado MySQL Workbench, mostrando:
  - Tablas.
  - Relaciones.
  - Llaves primarias y foráneas.

## 5. Integración y Explicación (1.0 punto)

- Explicación de cómo los diferentes componentes (frontend, backend, base de datos, autenticación) se integran en el escenario.
- Reflexión sobre posibles mejoras o ampliaciones futuras del sistema.

## Entrega en Video

- **Duración:** entre 5 y 7 minutos.
- Todos los integrantes deben participar en la exposición.
- El video debe mostrar de manera práctica:
  - El CRUD funcionando.
  - El flujo de autenticación con Auth0.
  - La metadata capturada en el perfil.
  - Los diagramas y explicaciones del diseño.

## 3. Resultado esperado

- Cargar en el espacio de entregas de la plataforma un documento de texto en PDF.
- El documento debe llevar una hoja con la información del estudiante, el programa y fecha.
- Las hojas siguientes y necesarias para responder los 5 puntos planteados.
- Una hoja con la bibliografía utilizada para los resultados del trabajo.

## 4. Recursos requeridos

- Leer el material de clase.
- Equipo de cómputo con conexión a Internet.
- Software de bases de datos, Draw.io.

---

**Elaborado por:** Jonathan Sánchez Giraldo  
**Versión:** 1.0  
**Fecha:** Febrero de 2025
