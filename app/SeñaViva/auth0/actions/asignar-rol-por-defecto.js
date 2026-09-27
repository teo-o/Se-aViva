/**
 * Post-Login Action: asigna el rol por defecto de SeñaViva a los usuarios que todavía no tienen ningún rol.
 *
 * Secrets de la Action (Actions → la Action → Secrets). Nunca se escriben en este archivo:
 *   AUTH0_DOMAIN     Dominio del tenant, sin https:// (ej. dev-xxxx.us.auth0.com)
 *   CLIENT_ID        Aplicación con "Client access" a la Auth0 Management API (scope update:users)
 *   CLIENT_SECRET    Client Secret de esa aplicación
 *   DEFAULT_ROLE_ID  Id del rol "Usuario" (rol_...), en User Management → Roles
 *   API_AUDIENCE     Identificador de la API de SeñaViva (https://api.senaviva.local)
 */
const DEFAULT_PERMISSIONS = ['read:sessions', 'write:sessions'];

exports.onExecutePostLogin = async (event, api) => {
  const roles = event.authorization?.roles ?? [];
  if (roles.length > 0) {
    return;
  }

  try {
    const managementToken = await getManagementToken(event.secrets);
    await assignDefaultRole(event.secrets, managementToken, event.user.user_id);
  } catch (error) {
    console.log(`No se pudo asignar el rol por defecto a ${event.user.user_id}: ${error.message}`);
    return;
  }

  // A role assigned during this login only shows up in tokens issued afterwards,
  // so the access token being issued now receives the role's permissions explicitly.
  if (event.resource_server?.identifier === event.secrets.API_AUDIENCE) {
    DEFAULT_PERMISSIONS.forEach((permission) => api.accessToken.addScope(permission));
  }
};

async function getManagementToken({ AUTH0_DOMAIN, CLIENT_ID, CLIENT_SECRET }) {
  const response = await fetch(`https://${AUTH0_DOMAIN}/oauth/token`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({
      grant_type: 'client_credentials',
      client_id: CLIENT_ID,
      client_secret: CLIENT_SECRET,
      audience: `https://${AUTH0_DOMAIN}/api/v2/`
    })
  });

  if (!response.ok) {
    throw new Error(`token ${response.status}: ${await response.text()}`);
  }

  return (await response.json()).access_token;
}

async function assignDefaultRole({ AUTH0_DOMAIN, DEFAULT_ROLE_ID }, managementToken, userId) {
  const response = await fetch(`https://${AUTH0_DOMAIN}/api/v2/users/${encodeURIComponent(userId)}/roles`, {
    method: 'POST',
    headers: {
      Authorization: `Bearer ${managementToken}`,
      'Content-Type': 'application/json'
    },
    body: JSON.stringify({ roles: [DEFAULT_ROLE_ID] })
  });

  if (!response.ok) {
    throw new Error(`roles ${response.status}: ${await response.text()}`);
  }
}
