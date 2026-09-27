# Guion del video — SeñaViva (Entrega 2)

**Duración objetivo:** unos 6 minutos y medio, dentro de los 5 a 7 que pide la rúbrica.

**Integrantes:**
- **Cristian:** Frontend y Base de Datos.
- **Mateo:** Backend y Arquitecto.

**Qué debe mostrar el video, según la rúbrica:**
- El CRUD funcionando.
- El flujo de autenticación con Auth0.
- La metadata capturada en el perfil.
- Los diagramas y la explicación del diseño.

---

## Antes de grabar (preparación)

### Levantar la aplicación
Desde `app/SeñaViva`, cada una en su propia terminal:

```powershell
dotnet run --project API --launch-profile https
dotnet run --project Web --launch-profile https
```

### Cuentas y datos
- **Cuenta administradora:** una cuenta con el rol **Administrador** en Auth0, para el CRUD de usuarios.
- **Correo para el registro en vivo:** uno **nuevo**, por ejemplo `tucorreo+demo@hotmail.com`. Tiene que ser un usuario que no exista, para que aparezca el formulario.
- **Datos previos:** 3 o 4 usuarios, y **al menos uno con sesiones**, para mostrar la regla de eliminación.

### Pestañas abiertas
1. La app, https://localhost:7200, en una **ventana de incógnito** para el registro.
2. El dashboard de Auth0, en *User Management → Users*.
3. Auth0, en *Actions → Triggers → post-login* y en *Forms*.
4. Swagger: https://localhost:7168/swagger
5. El diagrama general en Draw.io (`docs/senaviva_arquitectura_entrega2.drawio`) y el Word de la Guía 2, abierto en la sección 4.

### Pantalla
- **Zoom del navegador:** entre 110 y 125 %. Oculta la barra de marcadores y silencia las notificaciones.
- **Qué no mostrar:** Client Secrets, la pestaña *Credentials* de Auth0, los user-secrets, tokens, ni correos o números de documento reales.
- **Idioma del login:** para que salga en español, en Auth0 ve a *Settings → General → Languages → Default Language: Spanish*.

### Grabación
Puedes usar Teams, OBS o `Win + Alt + R` en Windows. Pueden grabar por escenas y unirlas después.

---

## Guion

### Escena 1 — Presentación (0:00 – 0:30) · Cristian
**Mostrar:** la pantalla de inicio de la app, sin sesión, con el logo animado.

> "Hola, somos Cristian Méndez y Mateo Orozco, del curso Arquitectura de Software II. Les presentamos **SeñaViva**, una aplicación que permite a una persona sorda que se comunica en Lengua de Señas Colombiana y a una persona oyente conversar en tiempo real, sin depender de un intérprete. En esta entrega construimos la base: usuarios, sesiones de conversación, autenticación con Auth0 y la integración entre la web, la API y la base de datos."

### Escena 2 — Contexto y módulos (0:30 – 1:10) · Cristian
**Mostrar:** en el inicio, baja hasta las tarjetas *"Lo que puedes hacer"* y *"Cómo empezar"*. Opcional: la tabla de módulos del Word (sección 1.2).

> "Hoy la comunicación entre sordos y oyentes depende de intérpretes escasos y costosos, lo que crea barreras en una cita médica, un trámite o una clase. Los módulos principales son: **usuarios**, **autenticación y perfil**, **sesiones de conversación**, y en próximas entregas la **traducción de señas a texto y de voz a señas**, con **transcripción** y **glosario LSC**. En esta entrega implementamos los tres primeros."

### Escena 3 — Diagramas y modelo de datos (1:10 – 2:10)

**Cristian (1:10 – 1:40)**
**Mostrar:** el diagrama general en Draw.io y luego el modelo de datos del Word (sección 4.3): primero Workbench y después el modelo implementado.

> "Este es el diagrama general: los cuatro actores usan la web, que se autentica con Auth0 y consume la API, y la API guarda los datos en SQL Server. En el modelo reutilizamos el diseño de Workbench e implementamos dos tablas: **Users** y **ConversationSessions**, con relación uno a muchos mediante la llave foránea **UserId**. Ajustamos el motor a SQL Server y agregamos **Auth0Id** para enlazar cada usuario con su identidad."

**Mateo (1:40 – 2:10)**
**Mostrar:** el UML de la Entrega 1 (sección 4.2 del Word).

> "Este es el UML de la primera entrega, con Strategy, Adapter y Observer. La entidad que elegimos para el CRUD es **SesionConversacion**, porque es la clase central: el contexto de Strategy y el sujeto de Observer. Guardamos su lengua de señas, hoy LSC, que es el dato con el que en la siguiente entrega se elegirá la estrategia de traducción."

### Escena 4 — Autenticación con Auth0 y metadata (2:10 – 3:50) · Mateo
**Hacer y mostrar (en la ventana de incógnito):**
1. **Pulsa "Crear cuenta".** Se abre el **login personalizado de Auth0**. Señala el logo y los colores de SeñaViva.
2. **Regístrate** con el correo de demostración.
3. **Llena el formulario "Completar perfil":** nombre completo, *¿Cómo te comunicas?*, tipo y número de documento. Usa un número inventado.
4. **Al volver a la app, abre "Mi perfil".** Se ven el nombre, el tipo y el número de documento que vienen de Auth0, y la **cuenta de SeñaViva ya creada**.
5. **En el dashboard de Auth0,** abre *Users → el usuario nuevo → Metadata* y muestra el `user_metadata`.
6. **Muestra rápidamente** *Actions → post-login* (las dos Actions) y el formulario en *Forms*.

> "Para registrarse, la persona pulsa Crear cuenta y llega al login de Auth0, que personalizamos con nuestra identidad. Después del correo y la contraseña, Auth0 muestra un formulario adicional con el nombre completo, cómo se comunica y el tipo y número de documento. Un Flow de Auth0 guarda el nombre en el campo **name** y el documento como **metadata del perfil**, que es lo que pide la guía; aquí lo vemos en el dashboard. Además, dos Post-Login Actions le asignan el rol Usuario y agregan estos datos al token. Al volver, la aplicación crea automáticamente su cuenta en SeñaViva."

### Escena 5 — CRUD de sesiones y permisos (3:50 – 4:40) · Cristian
**Hacer y mostrar (con el usuario nuevo):**
1. **Sesiones → "Nueva sesión":** créala. Aparece el aviso "Sesión creada" en la lista.
2. **Editar:** cambia el estado a *Finalizada*.
3. **Eliminar:** muestra la confirmación y elimínala.
4. **Abre "Usuarios":** aparece *"No tienes permiso para esta acción"*.

> "Con este usuario gestionamos las sesiones: crear, consultar, editar y eliminar. Cada acción llama a la API con el token del usuario. Si intenta entrar a Usuarios, la API responde que no tiene permiso: su rol Usuario solo incluye permisos de sesiones. Así demostramos la autorización por roles."

### Escena 6 — CRUD de usuarios (4:40 – 5:40) · Cristian
**Hacer y mostrar:** cierra sesión e inicia con la **cuenta Administrador**.
1. **Usuarios:** muestra la lista con roles, estado y enlace con Auth0.
2. **Nuevo usuario:** pulsa *Crear* con el formulario vacío para mostrar las **validaciones en español**. Luego créalo bien.
3. **Email repetido:** intenta crear otro con un email que ya existe. Aparece el aviso de **conflicto**.
4. **Editar:** cambia su rol o su estado.
5. **Eliminar un usuario con sesiones:** aparece el mensaje de que no se puede y que debe desactivarse.
6. **Eliminar el usuario creado:** se elimina sin problema.

> "Como administrador tenemos el CRUD completo de usuarios. El sistema valida los datos, no permite emails repetidos y protege la integridad: un usuario con sesiones no se elimina, se desactiva."

### Escena 7 — La API y la integración (5:40 – 6:30) · Mateo
**Mostrar:**
1. **Swagger:** los 12 endpoints. Ejecuta un GET sin token para mostrar el **401**.
2. **Opcional:** vuelve al diagrama general.

> "Por detrás, la API en ASP.NET Core expone doce endpoints REST documentados en Swagger. Sin un token válido de Auth0 responde 401, y sin el permiso adecuado, 403. La integración funciona así: la web autentica con Auth0, recibe el token y llama a la API desde el servidor, así que el token nunca llega al navegador. La API valida el token, aplica la regla de negocio y guarda en SQL Server con Entity Framework. Como mejoras futuras implementaremos la traducción en tiempo real con los patrones del UML, las tablas de transcripción y glosario, y el despliegue en la nube."

### Cierre (6:30 – 6:45) · Los dos
> **Cristian:** "Así SeñaViva queda lista para crecer hacia la traducción en tiempo real."
>
> **Mateo:** "Muchas gracias."

---

## Revisa antes de subir el video

- [ ] Duración entre 5 y 7 minutos.
- [ ] Hablan los dos integrantes.
- [ ] Se ve el CRUD de usuarios y de sesiones funcionando.
- [ ] Se ve el flujo de Auth0 completo: registro, formulario y regreso a la app.
- [ ] Se ve la metadata en Auth0 y en "Mi perfil".
- [ ] Se muestran y explican los diagramas.
- [ ] No aparece ningún secreto, token ni dato personal real.
