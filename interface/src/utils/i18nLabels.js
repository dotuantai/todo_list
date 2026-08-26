const roleTranslationKeys = {
  owner: 'common.SCR0027',
  manager: 'common.SCR0028',
  member: 'common.SCR0029',
  admin: 'common.SCR0036',
}

export const getRoleLabel = (t, role) => {
  const key = roleTranslationKeys[String(role || '').toLowerCase()]
  return key ? t(key) : (role || t('common.SCR0033'))
}
