# SeñaViva — Backend (Entrega 2)

API REST en **.NET 10 (LTS)** con el CRUD de **usuarios** y **sesiones de conversación**. Usa Entity Framework Core sobre SQL Server, Swagger y autenticación JWT con **Auth0**.

## Estructura

```text
SeñaViva/
├── SeñaViva.sln
├── global.json            SDK 10.0.x
├── dotnet-tools.json      dotnet-ef como herramienta local
├── API/                   ASP.NET Core Web API
│   ├── Auth/              Opciones de Auth0, permisos y políticas
│   ├── Controllers/       UsersController, ConversationSessionsController
│   ├── Data/              DataContext + configuraciones de EF
│   ├── Errors/            Excepciones de dominio y manejador global (ProblemDetails)
│   ├── Mappings/          Entidad → DTO de respuesta
│   ├── Migrations/        InitialCreate
│   ├── Services/          Reglas de negocio de cada entidad
│   └── Swagger/           Swagger con OAuth2 (PKCE) y Bearer
└── Shared/                Tipos compartidos con el frontend
    ├── DTOs/Requests|Responses
    ├── Entities/          User, ConversationSession
    └── Enums/             UserRole, SessionStatus, SignLanguage
```

El flujo es: `Controller` (HTTP) → `Service` (reglas) → `DataContext` (EF Core).

## Relación con el UML de la Entrega 1

| UML / modelo Workbench | Código | Estado |
|---|---|---|
| `usuario` | `User` | CRUD implementado |
| `SesionConversacion` / `sesion_conversacion` | `ConversationSession` | CRUD implementado (estado persistido) |
| `EstrategiaTraduccion.getIdiomaSoportado()` | `ConversationSession.SignLanguage` (`LSC`) | Solo el dato; las estrategias llegan después |
| `transcripcion`, `glosario_lsc` | — | Entregas posteriores |
| Strategy, Adapter y Observer (`setEstrategia`, `procesar`, `notificar`, MediaPipe, Whisper) | — | Entregas posteriores (tiempo real) |

`ConversationSession` es el estado persistente del *Context* y *Subject* del UML. Cuando se implemente la traducción en tiempo real, la sesión en ejecución cargará este registro y le asignará la estrategia según su `SignLanguage`. Agregar una nueva lengua de señas consiste en agregar un valor al enum y una estrategia nueva.

El `Id` de la sesión es un `Guid`, como el `idSesion: UUID` del UML. El tipo y el número de documento **no** se guardan en la base de datos: viven como `user_metadata` en Auth0, como pide la rúbrica. `User.Auth0Id` guarda el `sub` de Auth0 y enlaza el registro local con el perfil de Auth0.

## Requisitos

- .NET SDK 10.0.x.
- SQL Server LocalDB (viene con Visual Studio). Para usar otra instancia, sobrescribe `ConnectionStrings:DefaultConnection`.
- Una cuenta de Auth0 (ver la guía más abajo).

## Puesta en marcha

Ejecuta estos comandos desde `app/SeñaViva`:

```bash
dotnet tool restore

# Auth0: estos valores no son secretos, pero se mantienen fuera del repositorio
dotnet user-secrets set "Auth0:Domain" "TU-TENANT.us.auth0.com" --project API
dotnet user-secrets set "Auth0:Audience" "https://api.senaviva.local" --project API
dotnet user-secrets set "Auth0:SwaggerClientId" "CLIENT_ID_DE_LA_APP_SPA" --project API

# Opcional: otra base de datos
# dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Database=SenaVivaDb;..." --project API

dotnet ef database update --project API
dotnet run --project API --launch-profile https
```

Swagger queda disponible en **https://localhost:7168/swagger** (solo en Development).

`Auth0:Domain` va **sin** `https://`. Si falta `Domain` o `Audience`, la API no arranca y muestra un mensaje que lo indica; esto también aplica a los comandos de `dotnet ef`. Como alternativa a user-secrets puedes usar variables de entorno (`Auth0__Domain`, `Auth0__Audience`, `Auth0__SwaggerClientId`, `ConnectionStrings__DefaultConnection`).

## Endpoints

Todos exigen un token de Auth0 válido. Sin token responden **401**; con token pero sin el permiso, **403**.

| Método | Ruta | Permiso |
|---|---|---|
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
- El email es único (sin distinguir mayúsculas) y el `Auth0Id` también. Un duplicado responde **409**.
- No se puede eliminar un usuario que tiene sesiones (**409**); en ese caso se desactiva con `isActive: false`.
- Crear una sesión con un `userId` inexistente responde **404**.
- Si faltan campos o un enum tiene un valor inválido, la respuesta es **400** con `ValidationProblemDetails`.
- Los enums viajan como texto:
  - `role`: `Deaf`, `Hearing`, `Interpreter`, `Administrator`.
  - `status`: `Active`, `Finished`.
  - `signLanguage`: `LSC`.

Ejemplos de cuerpo:

```json
POST /api/users
{ "name": "Ana Pérez", "email": "ana@example.com", "role": "Deaf", "auth0Id": "auth0|65f..." }

POST /api/conversation-sessions
{ "userId": 1, "signLanguage": "LSC" }

PUT /api/conversation-sessions/{id}
{ "status": "Finished", "signLanguage": "LSC" }
```

---

## Guía de configuración de Auth0

En el entorno de desarrollo no había un MCP de Auth0 disponible, así que la configuración se hace a mano desde el dashboard de Auth0. **No subas al repositorio ningún Client Secret ni token.**

### 1. Dominio

En el dashboard, **Settings** del tenant (o cualquier aplicación → *Settings* → *Domain*), copia el dominio, por ejemplo `dev-abc123.us.auth0.com`. Ese valor va en `Auth0:Domain`.

### 2. API (Audience)

1. Ve a **Applications → APIs → Create API**.
   - **Name:** `SeñaViva API`
   - **Identifier:** `https://api.senaviva.local`. Es el **Audience** y va en `Auth0:Audience`. No tiene que ser una URL real y no se puede cambiar después.
   - **Signing Algorithm:** `RS256`
2. En la pestaña **Settings**, dentro de *RBAC Settings*, activa:
   - **Enable RBAC**
   - **Add Permissions in the Access Token**
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

### 4. Application para Swagger (Client ID)

1. Ve a **Applications → Applications → Create Application** y elige **Single Page Web Applications**. Nómbrala `SeñaViva Swagger`.
2. En **Settings**:
   - **Allowed Callback URLs:** `https://localhost:7168/swagger/oauth2-redirect.html`
   - **Allowed Web Origins:** `https://localhost:7168`
   - **Allowed Origins (CORS):** `https://localhost:7168`
3. Si la aplicación tiene la pestaña **APIs**, autoriza el acceso de usuario (*User Access*) a `SeñaViva API`.
4. Copia el **Client ID** en `Auth0:SwaggerClientId`. Una SPA no usa Client Secret: Swagger usa **Authorization Code + PKCE**.

La aplicación de login del frontend (la *Regular Web Application* de las clases) es independiente de esta y la configura el frontend.

### 5. Usuarios de prueba

En **User Management → Users → Create User**, con la conexión *Username-Password-Authentication*:
- `admin.senaviva@example.com`: pestaña **Roles**, asignar `Administrador`.
- `usuario.senaviva@example.com`: pestaña **Roles**, asignar `Usuario`.

Para enlazar a cada uno con la base de datos local, copia su **user_id** (por ejemplo `auth0|65f...`) y úsalo como `auth0Id` al crear el usuario con `POST /api/users`.

### 6. Obtener un token y probar en Swagger

**Opción A: iniciar sesión desde Swagger (recomendada)**
1. Abre `https://localhost:7168/swagger` y pulsa **Authorize**.
2. En el esquema **Auth0 (OAuth2, authorizationCode)** el `client_id` ya viene lleno. Deja `client_secret` vacío, marca `openid`, `profile` y `email`, y pulsa **Authorize**.
3. Inicia sesión con un usuario de prueba. Swagger adjuntará el token a cada petición.
   - Con `Administrador` funcionan todos los endpoints.
   - Con `Usuario`, los endpoints de `/api/users` responden **403**.

**Opción B: token de máquina (M2M)**
1. En **Applications → APIs → SeñaViva API → Machine To Machine Applications**, autoriza la aplicación de prueba y marca los permisos que quieras probar.
2. En la pestaña **Test** de la API, copia el `access_token` de la respuesta.
3. En Swagger, pulsa **Authorize** → esquema **Bearer**, pega el token sin el prefijo `Bearer ` y pulsa **Authorize**.

Para revisar qué contiene un token puedes usar https://jwt.io: busca `aud`, `iss` y `permissions` (o `scope`). No pegues ahí tokens de producción.

### 7. Metadata del perfil: tipo y número de documento (rúbrica, punto 3)

Esta parte se configura en Auth0, sin código, siguiendo el método de los encuentros explicativos. El backend no llama a la Management API.

1. **Form:** en **Actions → Forms**, crea un formulario con los campos *Nombre completo*, *Tipo de documento* (dropdown: Cédula, Tarjeta de identidad…) y *Número de documento*. Renombra los IDs de los campos a nombres legibles, por ejemplo `nombre_completo`, `tipo_doc` y `documento`.
2. **Post-Login Action:** usa el código de *render* que genera el Form en una Action del trigger **Post Login**. Luego haz *Deploy* y arrástrala al flujo.
3. **Vault:** crea una conexión de tipo **Auth0** con el Domain, el Client ID y el Client Secret de la aplicación de login. Usa como audience el identificador de **Auth0 Management API**. En esa aplicación, pestaña **APIs → Auth0 Management API**, concede estos *client access* scopes: `read:users`, `update:users`, `update:users_app_metadata` y `update:current_user_metadata`.
4. **Flow:** después del Form, agrega un Flow con la acción **Update user**. Usa el `user.id` del contexto y un body como este:

   ```json
   {
     "name": "{{fields.nombre_completo}}",
     "user_metadata": {
       "tipo_documento": "{{fields.tipo_doc}}",
       "numero_documento": "{{fields.documento}}"
     }
   }
   ```

   Los valores de texto van **entre comillas**; si no, Auth0 responde *user data is not valid JSON*.
5. **Verificación:** revisa las **Executions** del Flow (debe haber una respuesta 200) y, en *User Management → Users*, el `user_metadata` del usuario.
