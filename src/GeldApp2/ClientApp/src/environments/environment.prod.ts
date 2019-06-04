export const environment = {
  production: true,

  // App version. This value is replaced by CI server on build.
  version: '0.0.0',

  // Release date and time.
  versionDate: '-',

  // Milliseconds after which the local category cache is
  // invalidated and fetched again from the server.
  categoryCacheInvalidationIntervalMs: 60000,

  // Number of items per page in the expense-list
  expenseItemsPerPage: 20
};
