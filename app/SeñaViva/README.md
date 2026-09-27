# SeñaViva — Entrega 2

Solución en **.NET 10 (LTS)** con dos aplicaciones:

- **API:** API REST con el CRUD de **usuarios** y **sesiones de conversación**, Entity Framework Core sobre SQL Server, Swagger y autenticación JWT con **Auth0**.
- **Web:** aplicación **Blazor Web App**. Inicia sesión con Auth0, muestra el perfil con su metadata y consume la API para el CRUD.

## Estructura

```text
SeñaViva/
├── SeñaViva.sln
├── global.json            SDK 10.0.x
├── dotnet-tools.json      dotnet-ef como herramienta local
├── API/                   ASP.NET Core Web API (https://localhost:7168)
│   ├── Auth/              Opciones de Auth0, permisos, políticas y usuario actual
│   ├── Controllers/       UsersController, ConversationSessionsController
│   ├── Data/              DataContext + configuraciones de EF
│   ├── Errors/            Excepciones de dominio y manejador global (ProblemDetails)
│   ├── Mappings/          Entidad → DTO de respuesta
│   ├── Migrations/        InitialCreate
│   ├── Services/          Reglas de negocio de cada entidad
│   └── Swagger/           Swagger con OAuth2 (PKCE) y Bearer
├── Web/                   Blazor Web App, renderizado estático en servidor (https://localhost:7200)
│   ├── Auth/              Login/logout con Auth0 y token para llamar la API
│   ├── Components/        Layout HUD, páginas y componentes (PixelIcon, Notice, EmptyState, PixelLoader…)
│   ├── design/            pixelart.py: genera el logo, el favicon y las ilustraciones
│   ├── Display/           Textos en español para enums y fechas
│   └── Services/          Cliente HTTP de la API
├── Shared/                Tipos compartidos entre API y Web
│   ├── Auth/              Nombres de los claims personalizados de Auth0
│   ├── DTOs/Requests|Responses
│   ├── Entities/          User, ConversationSession
│   └── Enums/             UserRole, SessionStatus, SignLanguage
└── auth0/                 Configuración de Auth0 versionada, sin secretos
    ├── actions/           Código de las Post-Login Actions
    └── forms/             Plantilla del formulario "Completar perfil" (importable)
```

Así se conectan las piezas:

- **API:** `Controller` (HTTP) → `Service` (reglas) → `DataContext` (EF Core).
- **Web:** página Blazor → `SenaVivaApiClient` → API, con el access token del usuario.

## Cómo se integran los componentes

```text
Navegador ──► Web (Blazor) ──login──► Auth0 (Universal Login + Form + Actions)
                 │                        │
                 │  access token          ├─ Post-Login Action: rol "Usuario" por defecto
                 ▼                        └─ Form + Flow: nombre y documento en user_metadata
                API ──► SQL Server (Users, ConversationSessions)
```

- **Auth0** es la fuente de la identidad: credenciales, login, roles y la metadata del perfil (tipo y número de documento).
- **La Web** usa el flujo *Regular Web Application*, igual que el ejemplo de clase. Tiene las rutas `/Account/Login`, `/callback` y `/Account/Logout` y guarda la sesión en una cookie. El Client Secret solo existe en el servidor.
- **La API** valida el access token (JWT RS256) y autoriza cada endpoint por permiso.
- **La base de datos** guarda los datos del dominio. Cada `User` se enlaza con Auth0 por `Auth0Id` (el `sub`). El documento **no** se replica en la base de datos: vive solo en Auth0, así hay una sola fuente de verdad.

## Relación con el UML de la Entrega 1

| UML / modelo Workbench | Código | Estado |
|---|---|---|
| `usuario` | `User` | CRUD implementado |
| `SesionConversacion` / `sesion_conversacion` | `ConversationSession` | CRUD implementado (estado persistido) |
| `EstrategiaTraduccion.getIdiomaSoportado()` | `ConversationSession.SignLanguage` (`LSC`) | Solo el dato; las estrategias llegan después |
| `transcripcion`, `glosario_lsc` | — | Entregas posteriores |
| Strategy, Adapter y Observer (`setEstrategia`, `procesar`, `notificar`, MediaPipe, Whisper) | — | Entregas posteriores (tiempo real) |

`ConversationSession` es el estado persistente del *Context* y *Subject* del UML. Cuando se implemente la traducción en tiempo real, la sesión en ejecución cargará este registro y le asignará la estrategia según su `SignLanguage`. Agregar una nueva lengua de señas consiste en agregar un valor al enum y una estrategia nueva.

Cambios respecto al diseño de Workbench, documentados como permite el profesor:
- **Motor:** SQL Server en lugar de MySQL. El proveedor de MySQL más usado todavía no soporta EF Core 10.
- **Id de la sesión:** `Guid`, como el `idSesion: UUID` del UML.
- **`User`:** se agregó `Auth0Id`.

## Requisitos

- .NET SDK 10.0.x.
- SQL Server LocalDB (viene con Visual Studio). Para usar otra instancia, sobrescribe `ConnectionStrings:DefaultConnection`.
- Certificado HTTPS de desarrollo **de confianza**. Ejecuta `dotnet dev-certs https --trust` una vez y acepta la ventana de Windows. Sin esto, la Web no puede llamar a la API por HTTPS.
- Una cuenta de Auth0 (ver la guía más abajo).

## Puesta en marcha

Ejecuta estos comandos desde `app/SeñaViva`. Los valores de Auth0 se guardan con **user-secrets**, fuera del repositorio.

```bash
dotnet tool restore

# API
dotnet user-secrets set "Auth0:Domain" "TU-TENANT.us.auth0.com" --project API
dotnet user-secrets set "Auth0:Audience" "https://api.senaviva.local" --project API
dotnet user-secrets set "Auth0:SwaggerClientId" "CLIENT_ID_DE_LA_APP_SPA" --project API

# Web (Regular Web Application)
dotnet user-secrets set "Auth0:Domain" "TU-TENANT.us.auth0.com" --project Web
dotnet user-secrets set "Auth0:Audience" "https://api.senaviva.local" --project Web
dotnet user-secrets set "Auth0:ClientId" "CLIENT_ID_DE_LA_REGULAR_WEB_APP" --project Web
dotnet user-secrets set "Auth0:ClientSecret" "CLIENT_SECRET_DE_LA_REGULAR_WEB_APP" --project Web

dotnet ef database update --project API
```

Levanta cada aplicación en su propia terminal:

```bash
dotnet run --project API --launch-profile https   # https://localhost:7168/swagger
dotnet run --project Web --launch-profile https   # https://localhost:7200
```

Notas:
- **`Auth0:Domain` va sin `https://`.** Si falta un valor de Auth0, la aplicación no arranca y muestra cuál falta. En la API esto también aplica a los comandos de `dotnet ef`.
- **Variables de entorno:** en lugar de user-secrets puedes usar `Auth0__Domain`, `Auth0__ClientSecret`, etc.

## Web

| Página | Qué hace |
|---|---|
| `/` | Presentación. Enlaces **Crear cuenta** (abre directamente el registro de Auth0) e **Iniciar sesión** |
| `/perfil` | Datos de Auth0 (nombre, email, tipo y número de documento) y la cuenta en SeñaViva. A quien no tiene cuenta en SeñaViva (usuarios que ya existían y administradores) le pregunta *"¿Deseas crear tu usuario en SeñaViva?"*: los administradores quedan como Administrador y los demás eligen *persona sorda* u *oyente* |
| `/usuarios` | CRUD de usuarios: listar, crear, editar y eliminar (requiere `read:users` / `write:users`) |
| `/sesiones` | CRUD de sesiones de conversación. Una sesión nueva queda a nombre del usuario actual |

### Diseño (pixel art)

La interfaz usa un sistema de diseño propio, sin Bootstrap ni frameworks de UI. Todo el CSS está en `wwwroot/app.css`.

- **Paleta limitada:** tinta `#1c1b2b`, papel `#f3ead3`, nieve `#fffbf1`, verde azulado `#1f7a70`, sol `#f2b83a`, coral `#c2452b` y ciruela `#4b3a78`, con variantes oscuras para relieve y sombras. No hay degradados.
- **Tipografía:** *Press Start 2P* para la marca, el título de portada y los números. *Pixelify Sans* para títulos, etiquetas y texto. Press Start 2P no tiene mayúsculas con tilde, así que los títulos con tildes usan Pixelify. Las dos van alojadas en `wwwroot/fonts`, con licencia OFL.
- **Marcos pixel:** los elementos enmarcados usan un borde de 4 px con esquinas escalonadas y sombra dura; ver la variable `--frame`.
- **Animaciones:** todas son escalonadas (`steps()`), como los fotogramas de un sprite, y se desactivan con `prefers-reduced-motion`.
- **Logo, favicon e ilustraciones:** se dibujan como cuadrículas de texto en `design/pixelart.py`. Para regenerarlos, ejecuta `python design/pixelart.py` (requiere Pillow).
- **Íconos:** son de 9×9 y están en `Display/PixelIcons.cs`. Se usan con `<PixelIcon Name="..."/>` y toman el color del texto.
- **Estados de la interfaz:**
  - **Carga:** las listas y el perfil usan `[StreamRendering]` y muestran `PixelLoader` mientras llega la respuesta de la API. `wwwroot/js/app.js` muestra una barra de carga al navegar o enviar un formulario, marca el botón enviado como ocupado y evita el doble envío.
  - **Vacío:** `EmptyState`.
  - **Avisos de éxito, error o permiso:** `Notice`.
- **Responsive:** en pantallas de hasta 900 px la navegación pasa a un menú desplegable; en las de hasta 720 px las tablas se convierten en tarjetas.

**Alta automática de registros nuevos:** el formulario de Auth0 pregunta *"¿Cómo te comunicas?"* (`tipo_usuario`: `Deaf` o `Hearing`). Al volver del login, la web (`Auth/NewUserRegistration.cs`) crea el usuario en SeñaViva con ese rol mediante `POST /api/users/me`, sin preguntar nada más. Quien ya existía antes de este cambio no tiene `tipo_usuario` y los administradores se excluyen: a ellos se les pregunta en `/perfil`.

La web usa **renderizado estático en el servidor** con formularios. Cada llamada a la API ocurre mientras se atiende una petición HTTP, y así el access token siempre está disponible en `HttpContext`. Es el patrón que recomienda Microsoft para llamar APIs externas con el token del usuario sin exponerlo en el navegador.

## Endpoints de la API

Todos exigen un token de Auth0 válido. Sin token responden **401**; con token pero sin el permiso, **403**.

| Método | Ruta | Permiso |
|---|---|---|
| GET | `/api/users/me` | solo autenticado (usuario final) |
| POST | `/api/users/me` | solo autenticado (usuario final) |
| GET | `/api/users` | `read:users` |
| GET | `/api/users/{id}` | `read:users` |
| POST | `/api/users` | `write:users` |
| PUT | `/api/users/{id}` | `write:users` |
| DELETE | `/api/users/{id}` | `write:users` |
| GET | `/api/conversation-sessions` | `read:sessions` |
| GET | `/api/conversation-sessions/{id}` | `read:sessions` |
| POST | `/api/conversation-sessions` | `write:sessions` |
| PUT | `/api/conversation-sessions/{id}` | `write:sessions` |
| DELETE | `/api/conversation-sessions/{id}` | `write:sessions` |

Reglas:
- **`/api/users/me`:**
  - Registra o consulta al usuario del token.
  - Toma el `sub`, el email y el nombre de los claims que agrega la Action `perfil-senaviva.js`.
  - Permite los roles `Deaf` y `Hearing`. `Administrator` solo si el token trae el rol `Administrador` de Auth0 (claim `https://senaviva.local/roles`); si no, responde **403**. `Interpreter` lo asigna un administrador con `PUT /api/users/{id}`.
  - Si se llama dos veces, devuelve el mismo usuario.
  - No enlaza por email: si ya existe un usuario con ese email, responde **409** y un administrador debe asignarle el `auth0Id`.
  - Rechaza tokens de máquina (M2M) con **403**.
- **Unicidad:** el email es único (sin distinguir mayúsculas) y el `Auth0Id` también. Un duplicado responde **409**.
- **Eliminar usuarios:** no se puede eliminar un usuario que tiene sesiones (**409**); en ese caso se desactiva con `isActive: false`.
- **Sesiones:** crear una sesión con un `userId` inexistente responde **404**.
- **Validación:** si faltan campos o un enum tiene un valor inválido, la respuesta es **400** con los mensajes en español.
- **Enums:** viajan como texto:
  - `role`: `Deaf`, `Hearing`, `Interpreter`, `Administrator`.
  - `status`: `Active`, `Finished`.
  - `signLanguage`: `LSC`.

Ejemplos de cuerpo:

```json
POST /api/users
{ "name": "Ana Pérez", "email": "ana@example.com", "role": "Deaf", "auth0Id": "auth0|65f..." }

POST /api/users/me
{ "role": "Hearing" }

POST /api/conversation-sessions
{ "userId": 1, "signLanguage": "LSC" }

PUT /api/conversation-sessions/{id}
{ "status": "Finished", "signLanguage": "LSC" }
```

---

## Guía de configuración de Auth0

En el entorno de desarrollo no había un MCP de Auth0 disponible, así que la configuración se hace a mano desde el dashboard de Auth0. **No subas al repositorio ningún Client Secret ni token.**

### 1. Dominio

En cualquier aplicación → *Settings* → *Domain*, copia el dominio, por ejemplo `dev-abc123.us.auth0.com`. Ese valor va en `Auth0:Domain`.

### 2. API (Audience)

1. Ve a **Applications → APIs → Create API**.
   - **Name:** `SeñaViva API`
   - **Identifier:** `https://api.senaviva.local`. Es el **Audience** y va en `Auth0:Audience`. No tiene que ser una URL real y no se puede cambiar después.
   - **Signing Algorithm:** `RS256`
2. En la pestaña **Settings**, dentro de *RBAC Settings*, activa:
   - **Enable RBAC**
   - **Add Permissions in the Access Token**

   Sin esto el token no trae `permissions` y la API responde **403**.
3. En la pestaña **Permissions**, crea estos permisos:

| Permiso | Descripción |
|---|---|
| `read:users` | Consultar usuarios |
| `write:users` | Crear, actualizar y eliminar usuarios |
| `read:sessions` | Consultar sesiones de conversación |
| `write:sessions` | Crear, actualizar y eliminar sesiones |

### 3. Roles

En **User Management → Roles → Create Role**:

| Rol | Permisos (pestaña *Permissions* → *Add Permissions* → SeñaViva API) |
|---|---|
| `Administrador` | los 4 |
| `Usuario` | `read:sessions`, `write:sessions` |

Copia el **ID del rol `Usuario`** (`rol_...`); lo usa la Action del paso 7.

### 4. Aplicación de la Web (Regular Web Application)

En **Applications → Applications**, abre tu *Regular Web Application* o crea una nueva de ese tipo.

1. En **Settings**:
   - **Allowed Callback URLs:** `https://localhost:7200/callback`
   - **Allowed Logout URLs:** `https://localhost:7200/`
   - **Allowed Web Origins:** `https://localhost:7200`
2. En la pestaña **APIs**, edita `SeñaViva API`. En **User-delegated Access** elige **Authorized** y **marca los 4 permisos**; si no marcas ninguno, no se guarda. Sin este paso, Auth0 responde `Client "..." is not authorized to access resource server`.
3. Copia el **Client ID** y el **Client Secret** en los user-secrets del proyecto Web.

Esta misma aplicación tiene *Client access* a la **Auth0 Management API** para el Vault y la Action del rol (pasos 7 y 8). Necesita los scopes `read:users`, `update:users`, `update:users_app_metadata` y `update:current_user_metadata`, y en *Settings → Advanced → Grant Types* debe estar activo **Client Credentials**.

### 5. Aplicación para Swagger (SPA)

1. Ve a **Create Application** y elige **Single Page Web Applications**. Nómbrala `SeñaViva Swagger`.
2. En **Settings**:
   - **Allowed Callback URLs:** `https://localhost:7168/swagger/oauth2-redirect.html`
   - **Allowed Web Origins:** `https://localhost:7168`
   - **Allowed Origins (CORS):** `https://localhost:7168`
3. En la pestaña **APIs**, autoriza la *User-delegated Access* a `SeñaViva API` con los 4 permisos.
4. Copia el **Client ID** en `Auth0:SwaggerClientId` de la API. Una SPA no usa Client Secret: Swagger usa **Authorization Code + PKCE**.

### 6. Formulario "Completar perfil" (rúbrica, punto 3)

Captura el nombre completo, cómo se comunica la persona (`tipo_usuario`) y el tipo y número de documento. El Flow guarda el nombre y el documento en el perfil de Auth0 (`name` y `user_metadata`), siguiendo el método de los encuentros explicativos; la Action `perfil-senaviva.js` guarda `tipo_usuario` en `user_metadata`.

1. **Vault:** en **Actions → Forms → Vault**, crea una conexión **Auth0** con el Domain, el Client ID y el Client Secret de la aplicación del paso 4. Como audience usa `https://TU-TENANT.us.auth0.com/api/v2/`.
2. **Importar:** ve a **Forms → Create Form → Import** y carga [`auth0/forms/completar-perfil.json`](auth0/forms/completar-perfil.json). Trae el formulario y el Flow **Update user**. Si Auth0 pide asociar la conexión *REPLACE_WITH_M2M_CONNECTION*, elige la del Vault.
3. **Publicar:** publica el Form y el Flow. Copia el **ID del formulario** (`ap_...`).
4. **Verificar:** después de un login, revisa las **Executions** del Flow (deben tener respuesta 200) y el `user_metadata` del usuario.

### 7. Post-Login Actions

En **Actions → Triggers → post-login**, crea dos Actions con **+ → Build from scratch**. Pega el código de cada archivo, haz **Deploy** y **arrástralas al flujo en este orden**. Después pulsa **Apply**.

1. **[`auth0/actions/asignar-rol-por-defecto.js`](auth0/actions/asignar-rol-por-defecto.js)**
   - **Qué hace:** asigna el rol `Usuario` a quien no tiene roles, es decir, a quien se acaba de registrar. En ese mismo login agrega `read:sessions` y `write:sessions` al token.
   - **Secrets** (se agregan en la Action → *Secrets*):

     | Secret | Valor |
     |---|---|
     | `AUTH0_DOMAIN` | `TU-TENANT.us.auth0.com` |
     | `CLIENT_ID`, `CLIENT_SECRET` | Los de la aplicación del paso 4 |
     | `DEFAULT_ROLE_ID` | El `rol_...` del rol `Usuario` |
     | `API_AUDIENCE` | `https://api.senaviva.local` |

   - **Si el rol no se asigna:** en *Monitoring → Logs* aparece `No se pudo asignar el rol por defecto…`, seguido de la respuesta de Auth0. Si dice *Insufficient scope*, agrega el scope que indique a la aplicación.
2. **[`auth0/actions/perfil-senaviva.js`](auth0/actions/perfil-senaviva.js)**
   - **Configuración:** reemplaza `FORM_ID` por el `ap_...` del paso 6.
   - **Qué hace:** muestra el formulario mientras falten los datos del documento, y agrega a los tokens los claims `https://senaviva.local/...`:
     - **ID token:** nombre, tipo y número de documento, para la página *Mi perfil*.
     - **Access token:** email y nombre, para `POST /api/users/me`.

### 8. Personalizar el login (rúbrica: "no usar la plantilla por defecto")

En **Branding → Universal Login → Customization Options**:
- **Colores:** color primario `#0f766e` (el verde azulado de la web) y fondo de página `#1e3a8a`.
- **Logo:** la URL pública de un logo de SeñaViva.
- **Textos:** en *Branding → Universal Login → Advanced Options → Text customization* (o *Custom Text*), ajusta los textos de la pantalla de login y de registro en español.

Una plantilla HTML completamente propia requiere un dominio personalizado, como mostró el profesor.

### 9. Usuarios de prueba y verificación

1. **Registro:** en la Web, pulsa **Crear cuenta** y regístrate. Deben ocurrir tres cosas: aparece el formulario *Completar perfil*, se asigna el rol `Usuario` y vuelves a la web.
2. **Perfil:** en **Mi perfil** debes ver el nombre, el tipo y número de documento, y el formulario para registrarte en SeñaViva (*persona sorda* u *oyente*).
3. **Sesiones:** en **Sesiones**, crea, edita y elimina una sesión.
4. **Permisos:** con el rol `Usuario`, **Usuarios** muestra *"No tienes permiso"*, porque falta `read:users`. Para el CRUD de usuarios, asigna el rol `Administrador` a tu usuario en *User Management → Users → Roles*. Luego cierra sesión y vuelve a entrar, para que el token nuevo traiga los permisos.

Para probar la API directamente, Swagger ofrece dos opciones:
- **Authorize → esquema Auth0:** inicia sesión desde Swagger.
- **Authorize → esquema Bearer:** pega un token de la pestaña **Test** de la API en Auth0.

Para revisar qué contiene un token puedes usar https://jwt.io: busca `aud`, `permissions` (o `scope`) y los claims `https://senaviva.local/...`. No pegues ahí tokens reales de producción.
