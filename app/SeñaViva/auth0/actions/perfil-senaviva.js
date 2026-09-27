/**
 * Post-Login Action: muestra el formulario "SeñaViva - Completar perfil" mientras falten los datos del documento,
 * y agrega los datos del perfil a los tokens:
 *   - ID token (lo lee la web): nombre completo, tipo y número de documento, tipo de usuario y roles.
 *   - Access token (lo lee la API para registrar al usuario): email, nombre completo, tipo de usuario y roles.
 *
 * El tipo de usuario (Deaf/Hearing) solo lo tienen quienes llenaron el formulario al registrarse;
 * la web crea su usuario en SeñaViva automáticamente con ese rol.
 *
 * Reemplaza FORM_ID por el id del formulario (ap_...), visible en Actions → Forms.
 * Los nombres de los claims deben coincidir con Shared/Auth/SenaVivaClaims.cs.
 */
const FORM_ID = 'ap_REEMPLAZA_CON_TU_FORM_ID';
const NAMESPACE = 'https://senaviva.local/';

exports.onExecutePostLogin = async (event, api) => {
  const metadata = event.user.user_metadata ?? {};
  if (!metadata.tipo_documento || !metadata.numero_documento) {
    api.prompt.render(FORM_ID);
    return;
  }

  addProfileClaims(api, event, {
    fullName: event.user.name,
    documentType: metadata.tipo_documento,
    documentNumber: metadata.numero_documento,
    userType: metadata.tipo_usuario
  });
};

// Runs after the form is submitted. event.user may not reflect the metadata saved by the form's flow yet,
// so the submitted fields are used directly.
exports.onContinuePostLogin = async (event, api) => {
  const fields = event.prompt?.fields ?? {};
  if (fields.tipo_usuario) {
    api.user.setUserMetadata('tipo_usuario', fields.tipo_usuario);
  }

  addProfileClaims(api, event, {
    fullName: fields.nombre_completo ?? event.user.name,
    documentType: fields.tipo_doc,
    documentNumber: fields.documento,
    userType: fields.tipo_usuario
  });
};

function addProfileClaims(api, event, profile) {
  const roles = event.authorization?.roles ?? [];

  setClaim(api.accessToken, 'email', event.user.email);
  setClaim(api.accessToken, 'nombre_completo', profile.fullName);
  setClaim(api.accessToken, 'tipo_usuario', profile.userType);
  api.accessToken.setCustomClaim(`${NAMESPACE}roles`, roles);

  setClaim(api.idToken, 'nombre_completo', profile.fullName);
  setClaim(api.idToken, 'tipo_documento', profile.documentType);
  setClaim(api.idToken, 'numero_documento', profile.documentNumber);
  setClaim(api.idToken, 'tipo_usuario', profile.userType);
  api.idToken.setCustomClaim(`${NAMESPACE}roles`, roles);
}

function setClaim(token, name, value) {
  if (value) {
    token.setCustomClaim(`${NAMESPACE}${name}`, value);
  }
}
